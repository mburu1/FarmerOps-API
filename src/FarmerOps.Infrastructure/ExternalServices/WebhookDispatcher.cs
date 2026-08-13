using System.Text;
using System.Text.Json;
using FarmerOps.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FarmerOps.Infrastructure.ExternalServices;

/// <summary>
/// Fan-outs a single outbox event to every configured subscriber URL. Used by the outbox
/// processor background job; with no subscribers configured, dispatch is a no-op success so the
/// outbox message is still marked processed.
/// </summary>
public class WebhookDispatcher(
    HttpClient httpClient,
    IOptions<WebhookSettings> settings,
    ILogger<WebhookDispatcher> logger) : IWebhookDispatcher
{
    public async Task<bool> DispatchAsync(string eventType, string payloadJson, CancellationToken cancellationToken = default)
    {
        var subscribers = settings.Value.SubscriberUrls;
        if (subscribers.Length == 0)
        {
            logger.LogDebug("No webhook subscribers configured; skipping dispatch of {EventType}.", eventType);
            return true;
        }

        using var payloadDocument = JsonDocument.Parse(payloadJson);
        var envelope = new { eventType, occurredOnUtc = DateTime.UtcNow, data = payloadDocument.RootElement };

        var allSucceeded = true;
        foreach (var url in subscribers)
        {
            try
            {
                using var content = new StringContent(JsonSerializer.Serialize(envelope), Encoding.UTF8, "application/json");
                using var response = await httpClient.PostAsync(url, content, cancellationToken);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                logger.LogWarning(ex, "Webhook dispatch of {EventType} to {Url} failed.", eventType, url);
                allSucceeded = false;
            }
        }

        return allSucceeded;
    }
}
