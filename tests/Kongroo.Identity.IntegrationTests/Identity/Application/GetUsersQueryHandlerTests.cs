using Kongroo.BuildingBlocks.Application;
using Kongroo.BuildingBlocks.Infrastructure;
using Kongroo.Identity.Application;
using Kongroo.Identity.Domain;
using Kongroo.Identity.Infrastructure;
using Kongroo.Identity.IntegrationTests.Fixtures;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using Shouldly;

namespace Kongroo.Identity.IntegrationTests.Identity.Application;

public sealed class GetUsersQueryHandlerTests(PostgreSqlFixture postgreSqlFixture)
    : IClassFixture<PostgreSqlFixture>,
        IAsyncLifetime
{
    private readonly IdentityTestDatabase _database = new(postgreSqlFixture);

    [Fact]
    public async Task HandleAsync_WithNoUsers_ShouldReturnEmptyList()
    {
        // Arrange
        await using var context = _database.CreateDbContext();
        var handler = new GetUsersQueryHandler(context);

        // Act
        var response = await handler.HandleAsync(new GetUsersQuery(), TestContext.Current.CancellationToken);

        // Assert
        response.ShouldBeEmpty();
    }

    [Fact]
    public async Task HandleAsync_WithExistingUsers_ShouldReturnUsersOrderedByUsername()
    {
        // Arrange
        await using var context = _database.CreateDbContext();
        await CreateUserAsync(
            new CreateUserCommand("zebra", "zebra@example.com", "Sup3rSecure!", "Zebra User", UserRole.User),
            context,
            TestContext.Current.CancellationToken
        );
        await CreateUserAsync(
            new CreateUserCommand("admin", "admin@example.com", "Sup3rSecure!", "Admin User", UserRole.User),
            context,
            TestContext.Current.CancellationToken
        );
        await CreateUserAsync(
            new CreateUserCommand("alpha", "alpha@example.com", "Sup3rSecure!", "Alpha User", UserRole.User),
            context,
            TestContext.Current.CancellationToken
        );

        var handler = new GetUsersQueryHandler(context);

        // Act
        var response = await handler.HandleAsync(new GetUsersQuery(), TestContext.Current.CancellationToken);

        // Assert
        response.Select(user => user.Username).ShouldBe(["admin", "alpha", "zebra"]);
    }

    public async ValueTask InitializeAsync() => await _database.ResetAsync(TestContext.Current.CancellationToken);

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    private static async Task CreateUserAsync(
        CreateUserCommand command,
        IdentityDbContext context,
        CancellationToken cancellationToken
    )
    {
        var handler = new CreateUserCommandHandler(
            new PasswordHasher<string>(),
            context,
            new UnitOfWork<IdentityDbContext>(context, Substitute.For<IDomainEventDispatcher>())
        );
        await handler.HandleAsync(command, cancellationToken);
    }
}
