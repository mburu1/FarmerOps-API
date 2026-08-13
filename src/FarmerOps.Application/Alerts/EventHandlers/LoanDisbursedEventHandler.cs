using FarmerOps.Application.Alerts.Services;
using FarmerOps.Domain.Enums;
using FarmerOps.Domain.Events;
using MediatR;

namespace FarmerOps.Application.Alerts.EventHandlers;

public sealed class LoanDisbursedEventHandler(AlertDispatchService alerts) : INotificationHandler<LoanDisbursedEvent>
{
    public Task Handle(LoanDisbursedEvent notification, CancellationToken cancellationToken) =>
        alerts.DispatchToFarmerAsync(
            notification.FarmerId,
            AlertType.LoanDisbursed,
            $"{notification.PrincipalAmount:C} has been disbursed to your account. Repayment is due by {notification.DueDateUtc:d}.",
            cancellationToken);
}
