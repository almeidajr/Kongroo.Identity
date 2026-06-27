using Kongroo.BuildingBlocks.Domain;

namespace Kongroo.BuildingBlocks.Application;

public abstract class DomainEventHandler<TDomainEvent> : IDomainEventHandler
    where TDomainEvent : IDomainEvent
{
    public Type EventType => typeof(TDomainEvent);

    public Task HandleAsync(IDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        if (domainEvent.GetType() != typeof(TDomainEvent))
        {
            throw new InvalidOperationException(
                $"Handler supports '{typeof(TDomainEvent).FullName}' but received '{domainEvent.GetType().FullName}'."
            );
        }

        return HandleAsync((TDomainEvent)domainEvent, cancellationToken);
    }

    public abstract Task HandleAsync(TDomainEvent domainEvent, CancellationToken cancellationToken);
}
