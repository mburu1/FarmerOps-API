namespace FarmerOps.Application.Common.Interfaces;

/// <summary>Stub SMS gateway abstraction — the real implementation calls out via HttpClient.</summary>
public interface ISmsGatewayService
{
    Task<bool> SendAsync(string phoneNumber, string message, CancellationToken cancellationToken = default);
}
