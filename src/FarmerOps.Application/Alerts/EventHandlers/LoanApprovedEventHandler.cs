using FarmerOps.Application.Alerts.Services;
using FarmerOps.Domain.Enums;
using FarmerOps.Domain.Events;
using MediatR;

namespace FarmerOps.Application.Alerts.EventHandlers;

public sealed class LoanApprovedEventHandler(AlertDispatchService alerts) : INotificationHandler<LoanApprovedEvent>
{
    public Task Handle(LoanApprovedEvent notification, CancellationToken cancellationToken) =>
        alerts.DispatchToFarmerAsync(
            notification.FarmerId,
            AlertType.LoanApproved,
            $"Good news! Your loan application for {notification.PrincipalAmount:C} has been approved.",
            cancellationToken);
}
