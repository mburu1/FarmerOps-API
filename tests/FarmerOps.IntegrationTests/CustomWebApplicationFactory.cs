using FarmerOps.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MsSql;

namespace FarmerOps.IntegrationTests;

/// <summary>
/// Spins up a real, disposable SQL Server instance via Testcontainers for each test run, so
/// integration tests exercise the actual EF Core provider, migrations, and constraints instead of
/// an approximation like the InMemory provider.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MsSqlContainer _sqlContainer = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:SqlServer"] = _sqlContainer.GetConnectionString(),
                ["Jwt:SecretKey"] = "integration-test-signing-key-at-least-32-characters-long"
            });
        });
    }

    public async Task InitializeAsync()
    {
        await _sqlContainer.StartAsync();

        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.MigrateAsync();
    }

    // WebApplicationFactory<T> already exposes IAsyncDisposable.DisposeAsync() (ValueTask); this
    // is the separate Task-returning member xUnit's IAsyncLifetime requires, hence `new`.
    public new async Task DisposeAsync()
    {
        await _sqlContainer.DisposeAsync();
        await base.DisposeAsync();
    }
}
