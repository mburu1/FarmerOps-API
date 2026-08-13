using FarmerOps.Application.Alerts.Services;
using FarmerOps.Domain.Enums;
using FarmerOps.Domain.Events;
using MediatR;

namespace FarmerOps.Application.Alerts.EventHandlers;

/// <summary>Fired for every loan the nightly overdue-repayment job flags via <c>Loan.TryMarkOverdue</c>.</summary>
public sealed class LoanOverdueEventHandler(AlertDispatchService alerts) : INotificationHandler<LoanOverdueEvent>
{
    public Task Handle(LoanOverdueEvent notification, CancellationToken cancellationToken) =>
        alerts.DispatchToFarmerAsync(
            notification.FarmerId,
            AlertType.OverdueRepayment,
            $"Your loan repayment of {notification.OutstandingBalance:C} is now {notification.DaysOverdue} day(s) overdue. Please repay as soon as possible.",
            cancellationToken);
}
