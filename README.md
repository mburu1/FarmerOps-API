### 🌾 FarmerOps API — Smallholder Farmer Operations Platform

A scalable automation and operations API for smallholder farmer programs, built with **.NET 10**, **Scalar**, and **MSSQL**.

A production-grade **C# / .NET 10 REST API** simulating the kind of automation, integration, and operational intelligence platform a modern agri-tech operation needs to manage farmer operations at scale — loan disbursement, input order fulfillment, field agent coordination, and automated repayment alerts, all built on Clean Architecture with CQRS, a transactional outbox, and a pluggable rules engine.

---

#### 🏗️ Project Overview

**FarmerOps API** is a RESTful backend system for managing farmer profiles, loan disbursements, crop input orders, field agent assignments, and automated alert workflows for a smallholder-farmer operations program.

---

#### 🔧 Tech Stack

| Layer | Technology |
|---|---|
| Runtime | .NET 10 (C#) |
| API Docs | [Scalar](https://scalar.com/) (replacing Swagger UI) |
| Database | MSSQL (via EF Core 10) |
| CQRS / Mediator | MediatR |
| Validation | FluentValidation |
| Auth | JWT Bearer Tokens (access + rotating refresh tokens) |
| Background Jobs | Hangfire (hosted in a dedicated Worker service) |
| Testing | xUnit, FluentAssertions, Testcontainers (real MSSQL) |
| Migrations | EF Core Migrations |
| Containers | Docker / Docker Compose |

---

#### 📦 Core Modules

**1. Farmer Management** — `FarmersController`
CRUD for farmer profiles (name, phone, national ID, farm size, crop type), geo-tagged to a district/region hierarchy that mirrors Kenya's county structure, with pagination, filtering, and search.

**2. Loan & Input Order Processing** — `LoansController`, `InputOrdersController`
Loan application workflow with an explicit state machine (`Pending → Approved → Disbursed → Repaid`, with `Rejected`, `Overdue`, and `Defaulted` branches), gated by a pluggable **loan eligibility rules engine** (farm size, active-loan cap, default history, per-acre amount cap). Input orders (seeds, fertilizer, ...) can be linked to a disbursed loan.

**3. Field Agent Assignment Engine** — `AgentsController`, `VisitsController`
Assign field agents to farmers, schedule and track visits (`Scheduled → Completed/Missed/Cancelled`), and derive a running agent performance score from visit outcomes.

**4. Automation & Alert Workflows** — `AlertsController` + `FarmerOps.Worker`
- A nightly Hangfire job flags overdue loan repayments, which raises a domain event consumed by an Application-layer handler that creates an `Alert` and dispatches it through a mock SMS gateway (`HttpClient`-based, swappable for a real provider).
- A transactional **outbox** persists every domain event in the same DB transaction as the business change, and a separate Hangfire job fans those events out to webhook subscribers — reliable delivery without a distributed transaction.

**5. Analytics Endpoint** — `AnalyticsController`
Aggregate endpoints: repayment rates by region, input uptake by crop type, and field-agent coverage gaps by district.

**6. AI Integration Stub** — `InsightsController`
`GET /api/v1/insights/crop-recommendation` returns a mock crop-rotation recommendation behind a clean `ICropRecommendationEngine` seam — designed so a real ML model or external AI API can be swapped in without touching the API or Application layers.

---

#### 🧩 Architecture

Clean Architecture, dependencies pointing inward:

```
/src
  FarmerOps.Api            → .NET 10 Web API, Scalar docs, controllers, JWT auth
  FarmerOps.Application     → CQRS with MediatR (commands/queries), validators, domain event handlers
  FarmerOps.Domain          → Entities, enums, domain events, loan eligibility rules engine
  FarmerOps.Infrastructure  → EF Core + MSSQL, transactional outbox, JWT/SMS/webhook services, Hangfire jobs
  FarmerOps.Worker          → Hangfire host: nightly overdue check + outbox processor
/tests
  FarmerOps.UnitTests        → Domain + Application handler tests (xUnit, FluentAssertions)
  FarmerOps.IntegrationTests → Full HTTP pipeline against a real MSSQL container (Testcontainers)
```

See [`docs/architecture.md`](docs/architecture.md) for diagrams: layer dependencies, the loan state machine, the auth flow, and the outbox/webhook flow.

---

#### 🌟 Scalar API Explorer

Interactive API docs live at **`/scalar/v1`** in Development — a modern, faster alternative to Swagger UI. Click **Authorize**, paste an access token from `POST /auth/login`, and every endpoint becomes directly callable from the browser.

.NET 10 changed how JWT bearer auth is wired into the generated OpenAPI document (the .NET 9 `OpenApiReference` pattern no longer compiles). `src/FarmerOps.Api/OpenApi/BearerSecuritySchemeTransformer.cs` is the updated document transformer that makes the Authorize button actually attach a bearer token to requests.

---

## 🚀 Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server — LocalDB (Windows), a local instance, or `docker compose up sqlserver`
- Docker (optional, for the full `docker-compose` stack)

### 1. Configure connection strings & secrets

`src/FarmerOps.Api/appsettings.json` (and `src/FarmerOps.Worker/appsettings.json`) ship with **placeholders only** — replace them locally before running against a real database, and never commit real credentials:

```json
"ConnectionStrings": {
  "SqlServer": "Server=(localdb)\\MSSQLLocalDB;Database=FarmerOpsDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true",
  "Postgres": "Host=localhost;Port=5432;Username=postgres;Password=YOUR_PASSWORD;Database=postgres",
  "MySql": "Server=localhost;Port=3306;Database=FarmerOpsDb;User=root;Password=YOUR_PASSWORD;",
  "MongoDb": "mongodb://localhost:27017/FarmerOpsDb"
}
```

> Only `SqlServer` is actually wired into the app (EF Core + Hangfire storage); Postgres/MySql/MongoDb are kept as reference placeholders per the original brief and aren't used by any code path.

Also set a real `Jwt:SecretKey` (32+ characters) — the checked-in value is a placeholder.

### 2. Run the API

```bash
dotnet restore
dotnet run --project src/FarmerOps.Api
```

On first run in Development, the app applies EF Core migrations and seeds a handful of Kenyan counties/sub-counties automatically. Open your browser to:

| What | URL |
|---|---|
| Scalar API explorer | `https://localhost:7257/scalar/v1` (or `http://localhost:5047/scalar/v1`) |
| Raw OpenAPI document | `https://localhost:7257/openapi/v1.json` |
| Hangfire dashboard | `https://localhost:7257/hangfire` |

### 3. Run the background worker (optional, for alerts/outbox processing)

```bash
dotnet run --project src/FarmerOps.Worker
```

### 4. Try it end-to-end

```bash
# Register (role 0 = Admin, 1 = FieldAgent, 2 = OperationsManager)
curl -X POST http://localhost:5047/auth/register \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@farmerops.test","password":"P@ssw0rd123!","role":0,"fieldAgentId":null}'

# Use the returned accessToken as a Bearer token for everything else
curl http://localhost:5047/api/v1/regions -H "Authorization: Bearer <token>"
```

### 5. Or run the whole stack with Docker Compose

```bash
cp .env.example .env   # set MSSQL_SA_PASSWORD and JWT_SECRET_KEY
docker compose up --build
```

This brings up SQL Server, the API (`:8080`), and the Worker together.

---

## 🧪 Testing

```bash
# Fast, no external dependencies
dotnet test tests/FarmerOps.UnitTests

# Spins up a real SQL Server container via Testcontainers (requires Docker)
dotnet test tests/FarmerOps.IntegrationTests
```

Unit tests cover the loan state machine, the eligibility rules engine, and entity invariants in `FarmerOps.Domain`, plus a couple of Application-layer command handlers (including a regression test for a real EF Core change-tracking bug found while building this: issuing a refresh token for an already-tracked `User` without an `Include` on the collection navigation left the new `RefreshToken` untracked and caused a `DbUpdateConcurrencyException` on save).

Integration tests drive the full ASP.NET Core pipeline — auth, JWT validation, controllers, EF Core, migrations — against a disposable MSSQL container.

---

## 📖 Auth Endpoints

| Endpoint | Purpose |
|---|---|
| `POST /auth/register` | Create a user, get a token pair back |
| `POST /auth/login` | Exchange credentials for a JWT access + refresh token |
| `POST /auth/refresh` | Rotate a refresh token for a new pair |
| `GET /auth/me` | `[Authorize]`-gated — verifies the bearer token works |

The full endpoint list (30+ endpoints across Farmers, Loans, Input Orders, Agents, Visits, Alerts, Analytics, Insights, and Regions) is browsable and callable at `/scalar/v1`.

---

## 🗂️ Repository layout

```
.
├── src/                    Clean Architecture source projects
├── tests/                  Unit + integration test projects
├── docs/architecture.md    Diagrams: layers, loan state machine, auth flow, outbox flow
├── docker-compose.yml      API + Worker + SQL Server, one command
└── .github/workflows/ci.yml  Build, unit tests, integration tests, Docker image build
```
