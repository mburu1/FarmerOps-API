using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FarmerOps.Application.Auth.Dtos;
using FarmerOps.Application.Farmers.Dtos;
using FarmerOps.Application.Regions.Dtos;
using FarmerOps.Domain.Enums;
using FluentAssertions;

namespace FarmerOps.IntegrationTests;

[Collection(nameof(IntegrationTestCollection))]
public class FarmersApiTests(CustomWebApplicationFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    private async Task<string> RegisterAdminAndGetTokenAsync()
    {
        var email = $"{Guid.NewGuid()}@farmerops.test";
        var response = await _client.PostAsJsonAsync("/auth/register", new
        {
            email,
            password = "P@ssw0rd123!",
            role = 0, // Admin
            fieldAgentId = (Guid?)null
        });

        var auth = await response.Content.ReadFromJsonAsync<AuthResultDto>();
        return auth!.AccessToken;
    }

    private HttpClient AuthorizedClient(string accessToken)
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return _client;
    }

    private async Task<Guid> CreateDistrictAsync(HttpClient client)
    {
        var regionResponse = await client.PostAsJsonAsync("/api/v1/regions", new { name = $"Region-{Guid.NewGuid():N}", code = Guid.NewGuid().ToString("N")[..6] });
        var region = await regionResponse.Content.ReadFromJsonAsync<RegionDto>();

        var districtResponse = await client.PostAsJsonAsync("/api/v1/regions/districts", new { name = $"District-{Guid.NewGuid():N}", regionId = region!.Id });
        var district = await districtResponse.Content.ReadFromJsonAsync<DistrictDto>();
        return district!.Id;
    }

    [Fact]
    public async Task GetFarmers_WithoutToken_ReturnsUnauthorized()
    {
        // A plain `new HttpClient()` would make a real network call; CreateClient() wires up the
        // TestServer's in-memory handler so this actually hits the app under test.
        using var anonymousClient = factory.CreateClient();

        var response = await anonymousClient.GetAsync("/api/v1/farmers");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateFarmer_ThenGetById_RoundTrips()
    {
        var token = await RegisterAdminAndGetTokenAsync();
        var client = AuthorizedClient(token);
        var districtId = await CreateDistrictAsync(client);

        var createResponse = await client.PostAsJsonAsync("/api/v1/farmers", new
        {
            firstName = "Jane",
            lastName = "Wanjiru",
            phoneNumber = "+254700000000",
            nationalId = Guid.NewGuid().ToString("N")[..8],
            districtId,
            farmSizeAcres = 2.5m,
            primaryCrop = CropType.Maize,
            geoLatitude = (double?)null,
            geoLongitude = (double?)null
        });

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<FarmerDto>();

        var getResponse = await client.GetAsync($"/api/v1/farmers/{created!.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var fetched = await getResponse.Content.ReadFromJsonAsync<FarmerDto>();
        fetched!.Id.Should().Be(created.Id);
        fetched.FirstName.Should().Be("Jane");
    }

    [Fact]
    public async Task CreateFarmer_WithZeroFarmSize_ReturnsValidationProblem()
    {
        var token = await RegisterAdminAndGetTokenAsync();
        var client = AuthorizedClient(token);
        var districtId = await CreateDistrictAsync(client);

        var response = await client.PostAsJsonAsync("/api/v1/farmers", new
        {
            firstName = "Jane",
            lastName = "Wanjiru",
            phoneNumber = "+254700000000",
            nationalId = Guid.NewGuid().ToString("N")[..8],
            districtId,
            farmSizeAcres = 0m,
            primaryCrop = CropType.Maize,
            geoLatitude = (double?)null,
            geoLongitude = (double?)null
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
