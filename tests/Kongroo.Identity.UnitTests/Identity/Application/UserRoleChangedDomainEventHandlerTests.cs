using Kongroo.BuildingBlocks.Contracts;
using Kongroo.Identity.Application;
using Kongroo.Identity.Domain;
using MassTransit;
using NSubstitute;

namespace Kongroo.Identity.UnitTests.Identity.Application;

public sealed class UserRoleChangedDomainEventHandlerTests
{
    [Fact]
    public async Task HandleAsync_ShouldPublishIntegrationEventWithMappedFields()
    {
        // Arrange
        var publishEndpoint = Substitute.For<IPublishEndpoint>();
        var handler = new UserRoleChangedDomainEventHandler(publishEndpoint);

        var userId = UserId.Create();
        var domainEvent = new UserRoleChangedDomainEvent(userId, UserRole.User, UserRole.Admin);

        // Act
        await handler.HandleAsync(domainEvent, TestContext.Current.CancellationToken);

        // Assert
        await publishEndpoint
            .Received(1)
            .Publish(
                Arg.Is<UserRoleChangedIntegrationEvent>(integrationEvent =>
                    integrationEvent.UserId == userId.Value
                    && integrationEvent.PreviousRole == nameof(UserRole.User)
                    && integrationEvent.CurrentRole == nameof(UserRole.Admin)
                ),
                TestContext.Current.CancellationToken
            );
    }
}
