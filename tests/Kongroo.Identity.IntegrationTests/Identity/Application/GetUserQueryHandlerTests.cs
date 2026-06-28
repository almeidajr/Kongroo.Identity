using Kongroo.BuildingBlocks.Application;
using Kongroo.BuildingBlocks.Domain.Exceptions;
using Kongroo.BuildingBlocks.Infrastructure;
using Kongroo.Identity.Application;
using Kongroo.Identity.Domain;
using Kongroo.Identity.Infrastructure;
using Kongroo.Identity.IntegrationTests.Fixtures;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using Shouldly;

namespace Kongroo.Identity.IntegrationTests.Identity.Application;

public sealed class GetUserQueryHandlerTests(PostgreSqlFixture postgreSqlFixture)
    : IClassFixture<PostgreSqlFixture>,
        IAsyncLifetime
{
    private readonly IdentityTestDatabase _database = new(postgreSqlFixture);

    [Fact]
    public async Task HandleAsync_WithExistingUserId_ShouldReturnManagedUser()
    {
        // Arrange
        await using var context = _database.CreateDbContext();
        var userId = await CreateUserAsync(
            new CreateUserCommand(
                "kongroo",
                "kongroo@example.com",
                "Sup3rSecure!Password",
                "Kongroo Cloud Games",
                UserRole.User
            ),
            context,
            TestContext.Current.CancellationToken
        );

        var handler = new GetUserQueryHandler(context);

        // Act
        var response = await handler.HandleAsync(new GetUserQuery(userId.Value), TestContext.Current.CancellationToken);

        // Assert
        response.ShouldSatisfyAllConditions(
            () => response.Id.ShouldBe(userId.Value),
            () => response.Username.ShouldBe("kongroo"),
            () => response.Email.ShouldBe("kongroo@example.com"),
            () => response.Name.ShouldBe("Kongroo Cloud Games"),
            () => response.Role.ShouldBe(UserRole.User)
        );
    }

    [Fact]
    public async Task HandleAsync_WhenUserDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        await using var context = _database.CreateDbContext();
        var userId = Guid.NewGuid();
        var handler = new GetUserQueryHandler(context);

        // Act
        var exception = await Should.ThrowAsync<NotFoundException>(() =>
            handler.HandleAsync(new GetUserQuery(userId), TestContext.Current.CancellationToken)
        );

        // Assert
        exception.ResourceName.ShouldBe(nameof(User));
        exception.Lookup.ShouldBe($"identifier '{userId}'");
    }

    public async ValueTask InitializeAsync() => await _database.ResetAsync(TestContext.Current.CancellationToken);

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    private static async Task<UserId> CreateUserAsync(
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
        var response = await handler.HandleAsync(command, cancellationToken);
        return new UserId(response.Id);
    }
}
