using Kongroo.BuildingBlocks.Application;
using Kongroo.BuildingBlocks.Domain;
using Microsoft.EntityFrameworkCore;

namespace Kongroo.BuildingBlocks.Infrastructure;

public sealed class UnitOfWork<TDbContext>(TDbContext context, IEnumerable<IDomainEventHandler> handlers) : IUnitOfWork
    where TDbContext : DbContext
{
    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        var aggregates = context
            .ChangeTracker.Entries<IHasDomainEvents>()
            .Select(entry => entry.Entity)
            .Where(entity => entity.DomainEvents.Count > 0)
            .ToList();

        var domainEvents = aggregates.SelectMany(aggregate => aggregate.DomainEvents).ToList();

        foreach (var aggregate in aggregates)
        {
            aggregate.ClearDomainEvents();
        }

        foreach (var domainEvent in domainEvents)
        {
            foreach (var handler in handlers.Where(handler => handler.EventType == domainEvent.GetType()))
            {
                await handler.HandleAsync(domainEvent, cancellationToken);
            }
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
