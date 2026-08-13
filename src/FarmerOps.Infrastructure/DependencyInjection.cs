using FarmerOps.Application.Common.Interfaces;
using FarmerOps.Application.Common.Models;
using FarmerOps.Infrastructure.ExternalServices;
using FarmerOps.Infrastructure.Identity;
using FarmerOps.Infrastructure.Jobs;
using FarmerOps.Infrastructure.Persistence;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FarmerOps.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("SqlServer"),
                sql => sql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.Configure<SmsGatewaySettings>(configuration.GetSection(SmsGatewaySettings.SectionName));
        services.Configure<WebhookSettings>(configuration.GetSection(WebhookSettings.SectionName));

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<IJwtTokenService, JwtTokenService>();
        services.AddSingleton<ICropRecommendationEngine, MockCropRecommendationEngine>();

        services.AddHttpClient<ISmsGatewayService, MockSmsGatewayService>((sp, client) =>
        {
            var settings = configuration.GetSection(SmsGatewaySettings.SectionName).Get<SmsGatewaySettings>() ?? new SmsGatewaySettings();
            client.BaseAddress = new Uri(settings.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(settings.TimeoutSeconds);
        });

        services.AddHttpClient<IWebhookDispatcher, WebhookDispatcher>((sp, client) =>
        {
            var settings = configuration.GetSection(WebhookSettings.SectionName).Get<WebhookSettings>() ?? new WebhookSettings();
            client.Timeout = TimeSpan.FromSeconds(settings.TimeoutSeconds);
        });

        services.AddScoped<OutboxProcessorJob>();
        services.AddScoped<OverdueRepaymentCheckJob>();

        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseSqlServerStorage(configuration.GetConnectionString("SqlServer"), new SqlServerStorageOptions
            {
                CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                QueuePollInterval = TimeSpan.Zero,
                UseRecommendedIsolationLevel = true,
                DisableGlobalLocks = true
            }));

        return services;
    }
}
