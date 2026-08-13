namespace FarmerOps.Infrastructure.ExternalServices;

public sealed class SmsGatewaySettings
{
    public const string SectionName = "SmsGateway";

    /// <summary>Base URL of the (external, third-party) SMS gateway. Left as a placeholder in this demo.</summary>
    public string BaseUrl { get; set; } = "https://sms-gateway.example.invalid";
    public string ApiKey { get; set; } = "PLACEHOLDER_SMS_API_KEY";
    public int TimeoutSeconds { get; set; } = 5;
}
