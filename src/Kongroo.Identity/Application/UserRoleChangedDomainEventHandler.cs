using Kongroo.BuildingBlocks.Application;
using Kongroo.BuildingBlocks.Contracts;
using Kongroo.Identity.Domain;
using MassTransit;

namespace Kongroo.Identity.Application;

public class UserRoleChangedDomainEventHandler(IPublishEndpoint publishEndpoint)
    : DomainEventHandler<UserRoleChangedDomainEvent>
{
    protected override async Task HandleAsync(
        UserRoleChangedDomainEvent domainEvent,
        CancellationToken cancellationToken
    ) =>
        await publishEndpoint.Publish(
            new UserRoleChangedIntegrationEvent(
                domainEvent.UserId.Value,
                domainEvent.PreviousRole.ToString(),
                domainEvent.CurrentRole.ToString()
            ),
            cancellationToken
        );
}
