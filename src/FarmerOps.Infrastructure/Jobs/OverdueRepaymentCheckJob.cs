using FarmerOps.Application.Common.Interfaces;
using FarmerOps.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FarmerOps.Infrastructure.Jobs;

/// <summary>
/// Hangfire recurring job (scheduled nightly): flags every disbursed loan past its due date as
/// Overdue. Each transition raises <c>LoanOverdueEvent</c>, which the Application layer's
/// notification handler turns into a farmer-facing SMS alert — the job itself only owns the
/// state transition, not the notification mechanics.
/// </summary>
public class OverdueRepaymentCheckJob(IApplicationDbContext db, ILogger<OverdueRepaymentCheckJob> logger)
{
    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        var candidates = await db.Loans
            .Where(l => l.Status == LoanStatus.Disbursed && l.DueDateUtc != null && l.DueDateUtc < DateTime.UtcNow)
            .ToListAsync(cancellationToken);

        var flaggedCount = 0;
        foreach (var loan in candidates)
        {
            if (loan.TryMarkOverdue())
                flaggedCount++;
        }

        if (flaggedCount > 0)
            await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Overdue repayment check flagged {Count} of {Total} candidate loan(s).", flaggedCount, candidates.Count);
    }
}
