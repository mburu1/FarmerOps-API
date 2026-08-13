namespace FarmerOps.Application.Common.Interfaces;

/// <summary>Delivers a serialized integration event to downstream subscriber webhooks.</summary>
public interface IWebhookDispatcher
{
    Task<bool> DispatchAsync(string eventType, string payloadJson, CancellationToken cancellationToken = default);
}
