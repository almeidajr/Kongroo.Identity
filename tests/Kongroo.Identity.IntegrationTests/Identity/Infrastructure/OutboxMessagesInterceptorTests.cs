using Kongroo.Identity.Domain;
using Kongroo.Identity.IntegrationTests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace Kongroo.Identity.IntegrationTests.Identity.Infrastructure;

public sealed class OutboxMessagesInterceptorTests(PostgreSqlFixture postgreSqlFixture)
    : IClassFixture<PostgreSqlFixture>,
        IAsyncLifetime
{
    private readonly IdentityTestDatabase _database = new(postgreSqlFixture);

    [Fact]
    public async Task SaveChangesAsync_WithRaisedDomainEvents_ShouldPersistOutboxMessage()
    {
        // Arrange
        await using var context = _database.CreateDbContext();
        var user = CreateUser();
        context.Users.Add(user);

        // Act
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        context.ChangeTracker.Clear();
        var outboxMessageCount = await context.OutboxMessages.CountAsync(TestContext.Current.CancellationToken);

        outboxMessageCount.ShouldBe(1);
    }

    [Fact]
    public async Task SaveChangesAsync_WithRaisedDomainEvents_ShouldClearDomainEvents()
    {
        // Arrange
        await using var context = _database.CreateDbContext();
        var user = CreateUser();

        context.Users.Add(user);

        // Act
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        user.DomainEvents.ShouldBeEmpty();
    }

    [Fact]
    public async Task SaveChangesAsync_WithRaisedDomainEvents_ShouldRoundTripDomainEvent()
    {
        // Arrange
        await using var context = _database.CreateDbContext();
        var user = CreateUser();
        var raisedEvent = user.DomainEvents.Single().ShouldBeOfType<UserCreatedDomainEvent>();

        context.Users.Add(user);

        // Act
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        context.ChangeTracker.Clear();
        var outboxMessage = await context.OutboxMessages.SingleAsync(TestContext.Current.CancellationToken);
        var persistedEvent = outboxMessage.GetDomainEvent().ShouldBeOfType<UserCreatedDomainEvent>();

        persistedEvent.ShouldBe(raisedEvent);
    }

    public async ValueTask InitializeAsync() => await _database.ResetAsync(TestContext.Current.CancellationToken);

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    private static User CreateUser() =>
        User.Create(
            Username.From("kongroo"),
            Email.From("kongroo@example.com"),
            PasswordHash.From("hashed-password"),
            PersonName.From("Kongroo Cloud Games")
        );
}
