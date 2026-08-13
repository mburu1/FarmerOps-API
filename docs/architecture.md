# Architecture

## 1. Layers and dependency direction

Clean Architecture: dependencies only ever point inward, toward `Domain`. `Api` and `Worker` are the two composition roots — they're the only projects that know about *every* other layer.

```mermaid
flowchart TB
    subgraph Outer["Composition roots"]
        Api["FarmerOps.Api<br/>controllers · JWT auth · Scalar/OpenAPI"]
        Worker["FarmerOps.Worker<br/>Hangfire host"]
    end

    Infra["FarmerOps.Infrastructure<br/>EF Core + MSSQL · outbox · JWT/SMS/webhook services · Hangfire jobs"]
    App["FarmerOps.Application<br/>MediatR commands/queries · validators · domain event handlers"]
    Domain["FarmerOps.Domain<br/>entities · enums · domain events · eligibility rules engine"]

    Api --> App
    Api --> Infra
    Api --> Domain
    Worker --> App
    Worker --> Infra
    Worker --> Domain
    Infra --> App
    Infra --> Domain
    App --> Domain

    style Domain fill:#2d5f3f,color:#fff
    style App fill:#2d4f6f,color:#fff
    style Infra fill:#5f4a2d,color:#fff
    style Api fill:#5f2d4a,color:#fff
    style Worker fill:#5f2d4a,color:#fff
```

`Domain` has zero dependencies on the other layers — no EF Core, no ASP.NET Core, no MediatR implementation (only `MediatR.Contracts` for the `IRequest`/`INotification` marker interfaces). It's pure C#: entities enforce their own invariants through methods, never public setters.

`Application` defines the interfaces (`IApplicationDbContext`, `IJwtTokenService`, `ISmsGatewayService`, `IWebhookDispatcher`, `ICropRecommendationEngine`, ...) that `Infrastructure` implements — the Dependency Inversion that makes this "Clean."

## 2. Loan state machine

Every transition lives on the `Loan` aggregate itself (`Approve()`, `Reject()`, `Disburse()`, `RecordRepayment()`, `TryMarkOverdue()`, `MarkDefaulted()`); an invalid transition throws rather than silently corrupting state.

```mermaid
stateDiagram-v2
    [*] --> Pending: ApplyForLoanCommand<br/>(eligibility engine must pass)
    Pending --> Approved: Approve()
    Pending --> Rejected: Reject(reason)
    Approved --> Disbursed: Disburse(termDays)
    Disbursed --> Repaid: RecordRepayment()<br/>balance reaches 0
    Disbursed --> Overdue: TryMarkOverdue()<br/>(nightly job, past due date)
    Overdue --> Repaid: RecordRepayment()<br/>balance reaches 0
    Overdue --> Defaulted: MarkDefaulted()
    Rejected --> [*]
    Repaid --> [*]
    Defaulted --> [*]
```

Each transition that matters operationally raises a domain event (`LoanApprovedEvent`, `LoanDisbursedEvent`, `LoanOverdueEvent`, `LoanRejectedEvent`, `LoanRepaymentRecordedEvent`) — see §4.

### Loan eligibility rules engine

`ApplyForLoanCommand` runs every registered `ILoanEligibilityRule` against the applicant before a `Loan` is even created (Specification pattern):

- `MinimumFarmSizeRule` — farm must be ≥ 0.25 acres
- `MaxOutstandingLoansRule` — at most one active (Pending/Approved/Disbursed/Overdue) loan at a time
- `NoDefaultedLoanHistoryRule` — no prior `Defaulted` loan on record
- `MaxRequestedAmountRule` — requested amount capped relative to farm size

All rules run to completion (no short-circuiting), so the caller — and `GET /api/v1/loans/eligibility` — gets a full report, not just the first failure.

## 3. Auth flow

```mermaid
sequenceDiagram
    participant Client
    participant Api as FarmerOps.Api
    participant App as Application (MediatR)
    participant DB as SQL Server

    Client->>Api: POST /auth/login {email, password}
    Api->>App: LoginCommand
    App->>DB: find User by email
    App->>App: BCrypt.Verify(password, hash)
    App->>App: JwtTokenService.GenerateAccessToken(user)
    App->>DB: db.RefreshTokens.Add(new RefreshToken)
    App-->>Api: AuthResultDto {accessToken, refreshToken}
    Api-->>Client: 200 OK

    Client->>Api: GET /api/v1/farmers<br/>Authorization: Bearer {accessToken}
    Api->>Api: JwtBearer middleware validates signature/issuer/audience/expiry
    Api->>App: GetFarmersQuery (ICurrentUserService reads claims)
    App-->>Api: PagedResult<FarmerDto>
    Api-->>Client: 200 OK

    Client->>Api: POST /auth/refresh {refreshToken}
    Api->>App: RefreshTokenCommand
    App->>DB: find + validate RefreshToken (not expired/revoked)
    App->>App: revoke old token, issue + persist a new one (rotation)
    App-->>Api: AuthResultDto {new accessToken, new refreshToken}
```

**.NET 10 note:** the .NET 9 pattern for wiring a JWT bearer "Authorize" button into the generated OpenAPI document (an `OpenApiReference`-based security scheme) doesn't compile against .NET 10's updated `Microsoft.OpenApi` object model. `BearerSecuritySchemeTransformer` (`src/FarmerOps.Api/OpenApi/`) is the .NET 10 replacement — an `IOpenApiDocumentTransformer` that adds the Bearer scheme and requires it on every operation, which is what makes Scalar's Authorize button actually attach tokens to requests.

## 4. Domain events → transactional outbox → webhooks / alerts

```mermaid
sequenceDiagram
    participant Handler as Command Handler
    participant Loan as Loan (AggregateRoot)
    participant Ctx as ApplicationDbContext
    participant DB as SQL Server
    participant MediatR
    participant AlertSvc as AlertDispatchService
    participant SMS as Mock SMS Gateway
    participant Outbox as OutboxProcessorJob
    participant Webhook as Webhook Subscribers

    Handler->>Loan: loan.Approve()
    Loan->>Loan: Raise(LoanApprovedEvent)
    Handler->>Ctx: SaveChangesAsync()
    Note over Ctx: 1. Serialize every raised domain event<br/>into an OutboxMessage row —<br/>same DB transaction as the Loan update
    Ctx->>DB: COMMIT (Loan status + OutboxMessage, atomic)
    Note over Ctx: 2. Only after commit succeeds,<br/>publish events in-process (best effort)
    Ctx->>MediatR: Publish(LoanApprovedEvent)
    MediatR->>AlertSvc: LoanApprovedEventHandler
    AlertSvc->>DB: insert Alert (Pending)
    AlertSvc->>SMS: SendAsync(phone, message)
    SMS-->>AlertSvc: delivered (simulated)
    AlertSvc->>DB: Alert.MarkSent()

    loop every minute (Hangfire)
        Outbox->>DB: SELECT unprocessed OutboxMessages
        Outbox->>Webhook: POST event envelope to each subscriber URL
        Webhook-->>Outbox: 2xx
        Outbox->>DB: OutboxMessage.MarkProcessed()
    end
```

The outbox write is atomic with the business change (true transactional outbox — a crash right after commit can never lose a webhook event). The in-process `Publish()` that drives the mock SMS alert is a secondary, best-effort side effect, not part of that same transaction — acceptable here since a missed SMS is far less costly than a lost integration event, and the alert itself is still durably recorded either way.

## 5. Background jobs (`FarmerOps.Worker`)

| Job | Schedule | What it does |
|---|---|---|
| `OverdueRepaymentCheckJob` | Nightly (02:00) | Finds `Disbursed` loans past `DueDateUtc`, calls `Loan.TryMarkOverdue()`, which raises `LoanOverdueEvent` → SMS alert |
| `OutboxProcessorJob` | Every minute | Drains unprocessed `OutboxMessage` rows to configured webhook subscribers |

Both are registered via `IRecurringJobManager` (not the static `RecurringJob` API — that relies on `JobStorage.Current`, which is only initialized automatically in ASP.NET Core hosts, not a plain `Microsoft.Extensions.Hosting` worker).
