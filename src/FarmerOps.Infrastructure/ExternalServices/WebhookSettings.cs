namespace FarmerOps.Infrastructure.ExternalServices;

public sealed class WebhookSettings
{
    public const string SectionName = "Webhooks";

    /// <summary>Downstream subscriber endpoints notified for every outbox event. Empty by default.</summary>
    public string[] SubscriberUrls { get; set; } = [];
    public int TimeoutSeconds { get; set; } = 5;
}
