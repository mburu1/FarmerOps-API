using System.Net.Http.Json;
using FarmerOps.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FarmerOps.Infrastructure.ExternalServices;

/// <summary>
/// Simulates a third-party SMS gateway (e.g. Africa's Talking, Twilio) over HttpClient. The
/// configured endpoint is a placeholder, so the outbound call is expected to fail in this demo —
/// the failure is caught and logged, then delivery is simulated so alert workflows keep working
/// end to end. Swapping in a real gateway only requires changing configuration.
/// </summary>
public class MockSmsGatewayService(
    HttpClient httpClient,
    IOptions<SmsGatewaySettings> settings,
    ILogger<MockSmsGatewayService> logger) : ISmsGatewayService
{
    public async Task<bool> SendAsync(string phoneNumber, string message, CancellationToken cancellationToken = default)
    {
        var payload = new { to = phoneNumber, message, apiKey = settings.Value.ApiKey };

        try
        {
            using var response = await httpClient.PostAsJsonAsync("/messages", payload, cancellationToken);
            response.EnsureSuccessStatusCode();

            logger.LogInformation("SMS dispatched to {PhoneNumber} via gateway.", phoneNumber);
            return true;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            logger.LogWarning(
                ex,
                "SMS gateway at {BaseUrl} unreachable (expected in this demo). Simulating delivery to {PhoneNumber}.",
                httpClient.BaseAddress,
                phoneNumber);
            return true;
        }
    }
}
