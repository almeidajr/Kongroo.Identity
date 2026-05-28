using Kongroo.BuildingBlocks.Application;
using Kongroo.BuildingBlocks.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;

namespace Kongroo.Identity.UnitTests.BuildingBlocks.Infrastructure;

public sealed class ApplicationInitializationServiceTests
{
    [Fact]
    public async Task StartAsync_WithEnabledInitializers_ShouldRunInPriorityOrder()
    {
        // Arrange
        var executions = new List<string>();
        var secondInitializer = new TestApplicationInitializer("second", priority: 2, isEnabled: true, executions);
        var firstInitializer = new TestApplicationInitializer("first", priority: 1, isEnabled: true, executions);

        await using var serviceProvider = CreateServiceProvider(secondInitializer, firstInitializer);
        var service = CreateService(serviceProvider);

        // Act
        await service.StartAsync(TestContext.Current.CancellationToken);

        // Assert
        executions.ShouldBe(["first", "second"]);
    }

    [Fact]
    public async Task StartAsync_WhenInitializerIsDisabled_ShouldSkipInitializer()
    {
        // Arrange
        var executions = new List<string>();
        var initializer = new TestApplicationInitializer("disabled", priority: 0, isEnabled: false, executions);

        await using var serviceProvider = CreateServiceProvider(initializer);
        var service = CreateService(serviceProvider);

        // Act
        await service.StartAsync(TestContext.Current.CancellationToken);

        // Assert
        initializer.IsEnabledCallCount.ShouldBe(1);
        initializer.InitializeCallCount.ShouldBe(0);
        executions.ShouldBeEmpty();
    }

    [Fact]
    public async Task StartAsync_WhenInitializerThrows_ShouldRethrow()
    {
        // Arrange
        var executions = new List<string>();
        var failure = new InvalidOperationException("Initializer failed.");
        var initializer = new TestApplicationInitializer("failing", priority: 3, isEnabled: true, executions, failure);

        await using var serviceProvider = CreateServiceProvider(initializer);
        var service = CreateService(serviceProvider);

        // Act
        var exception = await Should.ThrowAsync<InvalidOperationException>(() =>
            service.StartAsync(TestContext.Current.CancellationToken)
        );

        // Assert
        exception.ShouldBeSameAs(failure);
    }

    private static ServiceProvider CreateServiceProvider(params IApplicationInitializer[] initializers)
    {
        var services = new ServiceCollection();

        foreach (var initializer in initializers)
        {
            services.AddScoped<IApplicationInitializer>(_ => initializer);
        }

        return services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
    }

    private static ApplicationInitializationService CreateService(IServiceProvider serviceProvider) =>
        new(
            NullLogger<ApplicationInitializationService>.Instance,
            serviceProvider.GetRequiredService<IServiceScopeFactory>()
        );

    private sealed class TestApplicationInitializer(
        string name,
        int priority,
        bool isEnabled,
        List<string> executions,
        Exception? exception = null
    ) : IApplicationInitializer
    {
        public int Priority { get; } = priority;

        public int IsEnabledCallCount { get; private set; }

        public int InitializeCallCount { get; private set; }

        public ValueTask<bool> IsEnabledAsync(CancellationToken cancellationToken)
        {
            IsEnabledCallCount++;

            return ValueTask.FromResult(isEnabled);
        }

        public Task InitializeAsync(CancellationToken cancellationToken)
        {
            InitializeCallCount++;
            if (exception is not null)
            {
                throw exception;
            }

            executions.Add(name);

            return Task.CompletedTask;
        }
    }
}

