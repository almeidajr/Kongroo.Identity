using Kongroo.BuildingBlocks.Application;
using Kongroo.BuildingBlocks.Domain;
using Microsoft.EntityFrameworkCore;

namespace Kongroo.BuildingBlocks.Infrastructure;

public sealed class UnitOfWork<TDbContext>(TDbContext context, IDomainEventDispatcher dispatcher) : IUnitOfWork
    where TDbContext : DbContext
{
    public async Task CommitAsync(CancellationToken cancellationToken)
    {
        var aggregates = context
            .ChangeTracker.Entries<IHasDomainEvents>()
            .Select(entry => entry.Entity)
            .Where(entity => entity.DomainEvents.Count > 0)
            .ToList();

        await dispatcher.DispatchAsync(aggregates, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }
}
