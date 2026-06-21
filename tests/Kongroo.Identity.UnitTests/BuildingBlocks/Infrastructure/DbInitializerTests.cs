using Kongroo.BuildingBlocks.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Shouldly;

namespace Kongroo.Identity.UnitTests.BuildingBlocks.Infrastructure;

public sealed class DbInitializerTests
{
    [Theory]
    [InlineData("Development")]
    [InlineData("Production")]
    [InlineData("Staging")]
    public async Task IsEnabledAsync_RegardlessOfEnvironment_ReturnsTrue(string environmentName)
    {
        var environment = Substitute.For<IHostEnvironment>();
        environment.EnvironmentName.Returns(environmentName);

        using var context = new TestDbContext(new DbContextOptionsBuilder<TestDbContext>().Options);
        var initializer = new DbInitializer<TestDbContext>(
            environment,
            context,
            NullLogger<DbInitializer<TestDbContext>>.Instance
        );

        var enabled = await initializer.IsEnabledAsync(TestContext.Current.CancellationToken);

        enabled.ShouldBeTrue();
    }

    [Theory]
    [InlineData("Production")]
    [InlineData("Staging")]
    public async Task InitializeAsync_NonDevelopmentEnvironment_LogsWarning(string environmentName)
    {
        var environment = Substitute.For<IHostEnvironment>();
        environment.EnvironmentName.Returns(environmentName);

        using var context = new TestDbContext(new DbContextOptionsBuilder<TestDbContext>().Options);
        var logger = new SpyLogger();
        var initializer = new DbInitializer<TestDbContext>(environment, context, logger);

        await Should.ThrowAsync<Exception>(() => initializer.InitializeAsync(TestContext.Current.CancellationToken));

        logger.WarningCount.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task InitializeAsync_DevelopmentEnvironment_DoesNotLogWarning()
    {
        var environment = Substitute.For<IHostEnvironment>();
        environment.EnvironmentName.Returns("Development");

        using var context = new TestDbContext(new DbContextOptionsBuilder<TestDbContext>().Options);
        var logger = new SpyLogger();
        var initializer = new DbInitializer<TestDbContext>(environment, context, logger);

        await Should.ThrowAsync<Exception>(() => initializer.InitializeAsync(TestContext.Current.CancellationToken));

        logger.WarningCount.ShouldBe(0);
    }

    private sealed class TestDbContext(DbContextOptions<TestDbContext> options) : DbContext(options);

    private sealed class SpyLogger : ILogger<DbInitializer<TestDbContext>>
    {
        private sealed class NoOpScope : IDisposable
        {
            public static readonly NoOpScope Instance = new();

            public void Dispose() { }
        }

        public int WarningCount { get; private set; }

        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull => NoOpScope.Instance;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter
        )
        {
            if (logLevel == LogLevel.Warning)
            {
                WarningCount++;
            }
        }
    }
}
