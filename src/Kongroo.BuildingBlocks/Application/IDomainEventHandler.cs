using Kongroo.BuildingBlocks.Domain;

namespace Kongroo.BuildingBlocks.Application;

public interface IDomainEventHandler
{
    Type EventType { get; }

    Task HandleAsync(IDomainEvent domainEvent, CancellationToken cancellationToken);
}
