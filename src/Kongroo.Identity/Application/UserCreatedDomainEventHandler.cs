using Kongroo.BuildingBlocks.Application;
using Kongroo.BuildingBlocks.Contracts;
using Kongroo.Identity.Domain;
using MassTransit;

namespace Kongroo.Identity.Application;

public class UserCreatedDomainEventHandler(IPublishEndpoint publishEndpoint)
    : DomainEventHandler<UserCreatedDomainEvent>
{
    protected override async Task HandleAsync(
        UserCreatedDomainEvent domainEvent,
        CancellationToken cancellationToken
    ) =>
        await publishEndpoint.Publish(
            new UserCreatedIntegrationEvent(domainEvent.UserId.Value, domainEvent.Email.Value, domainEvent.Name.Value),
            cancellationToken
        );
}
