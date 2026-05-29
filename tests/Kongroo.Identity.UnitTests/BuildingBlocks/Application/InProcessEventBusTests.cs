using Kongroo.BuildingBlocks.Application;
using Kongroo.BuildingBlocks.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Shouldly;

namespace Kongroo.Identity.UnitTests.BuildingBlocks.Application;

public sealed class InProcessEventBusTests
{
    [Fact]
    public async Task PublishAsync_WithMatchingHandlers_ShouldDispatchToAllHandlers()
    {
        // Arrange
        var integrationEvent = new TestIntegrationEvent("Portal");
        var firstHandler = Substitute.For<IIntegrationEventHandler<TestIntegrationEvent>>();
        var secondHandler = Substitute.For<IIntegrationEventHandler<TestIntegrationEvent>>();

        await using var serviceProvider = CreateServiceProvider(firstHandler, secondHandler);
        await using var scope = serviceProvider.CreateAsyncScope();
        var eventBus = scope.ServiceProvider.GetRequiredService<IEventBus>();

        // Act
        await eventBus.PublishAsync(integrationEvent, TestContext.Current.CancellationToken);

        // Assert
        await firstHandler.Received(1).HandleAsync(integrationEvent, TestContext.Current.CancellationToken);
        await secondHandler.Received(1).HandleAsync(integrationEvent, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task PublishAsync_WhenHandlerThrows_ShouldPropagateException()
    {
        // Arrange
        var integrationEvent = new TestIntegrationEvent("Portal");
        var handler = Substitute.For<IIntegrationEventHandler<TestIntegrationEvent>>();
        handler
            .HandleAsync(integrationEvent, TestContext.Current.CancellationToken)
            .Returns(_ => throw new InvalidOperationException("Simulated handler failure."));

        await using var serviceProvider = CreateServiceProvider(handler);
        await using var scope = serviceProvider.CreateAsyncScope();
        var eventBus = scope.ServiceProvider.GetRequiredService<IEventBus>();

        // Act
        var exception = await Should.ThrowAsync<InvalidOperationException>(() =>
            eventBus.PublishAsync(integrationEvent, TestContext.Current.CancellationToken)
        );

        // Assert
        exception.Message.ShouldBe("Simulated handler failure.");
        await handler.Received(1).HandleAsync(integrationEvent, TestContext.Current.CancellationToken);
    }

    private static ServiceProvider CreateServiceProvider(
        params IIntegrationEventHandler<TestIntegrationEvent>[] handlers
    )
    {
        var services = new ServiceCollection();

        services.AddLogging();
        services.AddScoped<IEventBus, InProcessEventBus>();

        foreach (var handler in handlers)
        {
            services.AddScoped<IIntegrationEventHandler<TestIntegrationEvent>>(_ => handler);
        }

        return services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
    }

    public sealed record TestIntegrationEvent(string Name) : IntegrationEvent;
}
