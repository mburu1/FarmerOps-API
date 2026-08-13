using FarmerOps.Application;
using FarmerOps.Infrastructure;
using FarmerOps.Infrastructure.Jobs;
using FarmerOps.Infrastructure.Persistence;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHangfireServer();

var host = builder.Build();

using (var scope = host.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();
    await ApplicationDbContextSeeder.SeedAsync(db);

    // The static RecurringJob API relies on JobStorage.Current, which AddHangfire() only wires up
    // for ASP.NET Core hosts. In a plain worker host, go through the DI-registered manager instead.
    var recurringJobs = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();

    // Nightly sweep: flags disbursed loans past their due date as Overdue, which fans out
    // LoanOverdueEvent -> Application's Alert handler -> mock SMS notification.
    recurringJobs.AddOrUpdate<OverdueRepaymentCheckJob>(
        "overdue-repayment-check",
        job => job.RunAsync(CancellationToken.None),
        Cron.Daily(2));

    // Drains the transactional outbox to webhook subscribers every minute.
    recurringJobs.AddOrUpdate<OutboxProcessorJob>(
        "outbox-processor",
        job => job.ProcessPendingMessagesAsync(CancellationToken.None),
        "*/1 * * * *");
}

host.Run();
