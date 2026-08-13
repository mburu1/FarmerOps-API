using FarmerOps.Application.Common.Interfaces;
using FarmerOps.Domain.Entities;
using FarmerOps.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FarmerOps.Application.Alerts.Services;

/// <summary>
/// Creates an Alert record and attempts immediate delivery through the mock SMS gateway.
/// Shared by every domain-event notification handler that needs to notify a farmer.
/// </summary>
public sealed class AlertDispatchService(IApplicationDbContext db, ISmsGatewayService smsGateway)
{
    public async Task DispatchToFarmerAsync(Guid farmerId, AlertType type, string message, CancellationToken cancellationToken)
    {
        var farmer = await db.Farmers.AsNoTracking().FirstOrDefaultAsync(f => f.Id == farmerId, cancellationToken);
        if (farmer is null)
            return;

        var alert = new Alert(type, message, farmerId: farmerId);
        db.Alerts.Add(alert);
        await db.SaveChangesAsync(cancellationToken);

        var delivered = await smsGateway.SendAsync(farmer.PhoneNumber, message, cancellationToken);
        if (delivered)
            alert.MarkSent();
        else
            alert.MarkFailed("SMS gateway did not confirm delivery.");

        await db.SaveChangesAsync(cancellationToken);
    }
}
