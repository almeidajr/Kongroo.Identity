using Kongroo.BuildingBlocks.Application;
using Kongroo.BuildingBlocks.Domain;
using Kongroo.BuildingBlocks.Infrastructure;
using Kongroo.Identity.Domain;
using Kongroo.Identity.Infrastructure;
using Kongroo.Identity.IntegrationTests.Fixtures;
using Kongroo.Identity.IntegrationTests.Identity;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace Kongroo.Identity.IntegrationTests.BuildingBlocks.Infrastructure;

public sealed class UnitOfWorkTests(PostgreSqlFixture postgreSqlFixture)
    : IClassFixture<PostgreSqlFixture>,
        IAsyncLifetime
{
    private readonly IdentityTestDatabase _database = new(postgreSqlFixture);

    [Fact]
    public async Task CommitAsync_ShouldDispatchEvents_ClearThem_AndPersist()
    {
        // Arrange
        await using var context = _database.CreateDbContext();
        var user = User.Create(
            Username.From("kongroo"),
            Email.From("kongroo@example.com"),
            PasswordHash.From("hashed-password"),
            PersonName.From("Kongroo Cloud Games")
        );
        context.Users.Add(user);

        var recordingHandler = new RecordingHandler();
        var unitOfWork = new UnitOfWork<IdentityDbContext>(context, [recordingHandler]);

        // Act
        await unitOfWork.CommitAsync(TestContext.Current.CancellationToken);

        // Assert
        recordingHandler.Handled.ShouldHaveSingleItem().ShouldBeOfType<UserCreatedDomainEvent>();
        user.DomainEvents.ShouldBeEmpty();

        context.ChangeTracker.Clear();
        (await context.Users.CountAsync(TestContext.Current.CancellationToken)).ShouldBe(1);
    }

    public async ValueTask InitializeAsync() => await _database.ResetAsync(TestContext.Current.CancellationToken);

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    private sealed class RecordingHandler : IDomainEventHandler
    {
        public List<IDomainEvent> Handled { get; } = [];

        public Type EventType => typeof(UserCreatedDomainEvent);

        public Task HandleAsync(IDomainEvent domainEvent, CancellationToken cancellationToken)
        {
            Handled.Add(domainEvent);
            return Task.CompletedTask;
        }
    }
}
