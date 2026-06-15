using Kongroo.BuildingBlocks.Contracts;
using Kongroo.Identity.Application;
using Kongroo.Identity.Domain;
using MassTransit;
using NSubstitute;

namespace Kongroo.Identity.UnitTests.Identity.Application;

public sealed class UserCreatedDomainEventHandlerTests
{
    [Fact]
    public async Task HandleAsync_ShouldPublishIntegrationEventWithMappedFields()
    {
        // Arrange
        var publishEndpoint = Substitute.For<IPublishEndpoint>();
        var handler = new UserCreatedDomainEventHandler(publishEndpoint);

        var userId = UserId.Create();
        var email = Email.From("kongroo@example.com");
        var name = PersonName.From("Kongroo Cloud Games");
        var domainEvent = new UserCreatedDomainEvent(userId, email, name);

        // Act
        await handler.HandleAsync(domainEvent, TestContext.Current.CancellationToken);

        // Assert
        await publishEndpoint
            .Received(1)
            .Publish(
                Arg.Is<UserCreatedIntegrationEvent>(integrationEvent =>
                    integrationEvent.UserId == userId.Value
                    && integrationEvent.Email == email.Value
                    && integrationEvent.Name == name.Value
                ),
                TestContext.Current.CancellationToken
            );
    }
}
