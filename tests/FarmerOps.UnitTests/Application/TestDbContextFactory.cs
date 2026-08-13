using FarmerOps.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FarmerOps.UnitTests.Application;

/// <summary>Publishes nowhere — handler tests assert on persisted state, not on side-effect notifications.</summary>
internal sealed class NoOpPublisher : IPublisher
{
    public Task Publish(object notification, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : INotification => Task.CompletedTask;
}

internal static class TestDbContextFactory
{
    public static ApplicationDbContext Create()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options, new NoOpPublisher());
    }
}
