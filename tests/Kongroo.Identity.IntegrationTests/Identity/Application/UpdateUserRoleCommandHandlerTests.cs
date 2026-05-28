using Kongroo.BuildingBlocks.Domain.Exceptions;
using Kongroo.Identity.Application;
using Kongroo.Identity.Domain;
using Kongroo.Identity.Infrastructure;
using Kongroo.Identity.IntegrationTests.Fixtures;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace Kongroo.Identity.IntegrationTests.Identity.Application;

public sealed class UpdateUserRoleCommandHandlerTests(PostgreSqlFixture postgreSqlFixture)
    : IClassFixture<PostgreSqlFixture>,
        IAsyncLifetime
{
    private readonly IdentityTestDatabase _database = new(postgreSqlFixture);

    [Fact]
    public async Task HandleAsync_WithUserTargetRole_ShouldPromoteUserToAdmin()
    {
        // Arrange
        await using var context = _database.CreateDbContext();
        var actingUserId = await CreateUserAsync(
            new CreateUserCommand(
                "acting-admin",
                "acting-admin@example.com",
                "Sup3rSecure!",
                "Acting Admin",
                UserRole.User
            ),
            context,
            TestContext.Current.CancellationToken
        );
        var targetUserId = await CreateUserAsync(
            new CreateUserCommand(
                "target-user",
                "target-user@example.com",
                "Sup3rSecure!",
                "Target User",
                UserRole.User
            ),
            context,
            TestContext.Current.CancellationToken
        );

        var handler = new UpdateUserRoleCommandHandler(context);

        // Act
        var response = await handler.HandleAsync(
            new UpdateUserRoleCommand(actingUserId.Value, targetUserId.Value, UserRole.Admin),
            TestContext.Current.CancellationToken
        );

        // Assert
        response.ShouldSatisfyAllConditions(
            () => response.Id.ShouldBe(targetUserId.Value),
            () => response.Role.ShouldBe(UserRole.Admin)
        );
    }

    [Fact]
    public async Task HandleAsync_WithAdminTargetRole_ShouldDemoteAdminToUser()
    {
        // Arrange
        await using var context = _database.CreateDbContext();
        var actingUserId = await CreateUserAsync(
            new CreateUserCommand(
                "acting-admin",
                "acting-admin@example.com",
                "Sup3rSecure!",
                "Acting Admin",
                UserRole.User
            ),
            context,
            TestContext.Current.CancellationToken
        );
        var targetUserId = await CreateUserAsync(
            new CreateUserCommand(
                "target-admin",
                "target-admin@example.com",
                "Sup3rSecure!",
                "Target Admin",
                UserRole.User
            ),
            context,
            TestContext.Current.CancellationToken
        );

        var handler = new UpdateUserRoleCommandHandler(context);

        // Act
        var response = await handler.HandleAsync(
            new UpdateUserRoleCommand(actingUserId.Value, targetUserId.Value, UserRole.User),
            TestContext.Current.CancellationToken
        );

        // Assert
        response.ShouldSatisfyAllConditions(
            () => response.Id.ShouldBe(targetUserId.Value),
            () => response.Role.ShouldBe(UserRole.User)
        );
    }

    [Fact]
    public async Task HandleAsync_WhenTargetUserDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        await using var context = _database.CreateDbContext();
        var actingUserId = await CreateUserAsync(
            new CreateUserCommand(
                "acting-admin",
                "acting-admin@example.com",
                "Sup3rSecure!",
                "Acting Admin",
                UserRole.User
            ),
            context,
            TestContext.Current.CancellationToken
        );
        var handler = new UpdateUserRoleCommandHandler(context);
        var missingUserId = Guid.NewGuid();

        // Act
        var exception = await Should.ThrowAsync<NotFoundException>(() =>
            handler.HandleAsync(
                new UpdateUserRoleCommand(actingUserId.Value, missingUserId, UserRole.Admin),
                TestContext.Current.CancellationToken
            )
        );

        // Assert
        exception.ResourceName.ShouldBe(nameof(User));
        exception.Lookup.ShouldBe($"identifier '{missingUserId}'");
    }

    [Fact]
    public async Task HandleAsync_WhenAdminTargetsSelfForDemotion_ShouldThrowConflictException()
    {
        // Arrange
        await using var context = _database.CreateDbContext();
        var adminUserId = await CreateUserAsync(
            new CreateUserCommand("self-admin", "self-admin@example.com", "Sup3rSecure!", "Self Admin", UserRole.User),
            context,
            TestContext.Current.CancellationToken
        );

        var handler = new UpdateUserRoleCommandHandler(context);
        await handler.HandleAsync(
            new UpdateUserRoleCommand(adminUserId.Value, adminUserId.Value, UserRole.Admin),
            TestContext.Current.CancellationToken
        );

        // Act
        var exception = await Should.ThrowAsync<ConflictException>(() =>
            handler.HandleAsync(
                new UpdateUserRoleCommand(adminUserId.Value, adminUserId.Value, UserRole.User),
                TestContext.Current.CancellationToken
            )
        );

        context.ChangeTracker.Clear();
        var savedUser = await context.Users.SingleAsync(
            candidate => candidate.Id == adminUserId,
            TestContext.Current.CancellationToken
        );

        // Assert
        exception.ResourceName.ShouldBe(nameof(User));
        exception.Reason.ShouldBe("admins cannot remove their own admin access");
        savedUser.Role.ShouldBe(UserRole.Admin);
    }

    [Fact]
    public async Task HandleAsync_WhenRoleMatchesCurrentRole_ShouldRemainUnchanged()
    {
        // Arrange
        await using var context = _database.CreateDbContext();
        var actingUserId = await CreateUserAsync(
            new CreateUserCommand(
                "acting-admin",
                "acting-admin@example.com",
                "Sup3rSecure!",
                "Acting Admin",
                UserRole.User
            ),
            context,
            TestContext.Current.CancellationToken
        );
        var targetUserId = await CreateUserAsync(
            new CreateUserCommand(
                "target-user",
                "target-user@example.com",
                "Sup3rSecure!",
                "Target User",
                UserRole.User
            ),
            context,
            TestContext.Current.CancellationToken
        );

        var handler = new UpdateUserRoleCommandHandler(context);

        // Act
        var response = await handler.HandleAsync(
            new UpdateUserRoleCommand(actingUserId.Value, targetUserId.Value, UserRole.User),
            TestContext.Current.CancellationToken
        );

        // Assert
        response.ShouldSatisfyAllConditions(
            () => response.Id.ShouldBe(targetUserId.Value),
            () => response.Username.ShouldBe("target-user"),
            () => response.Email.ShouldBe("target-user@example.com"),
            () => response.Name.ShouldBe("Target User"),
            () => response.Role.ShouldBe(UserRole.User)
        );
    }

    public async ValueTask InitializeAsync() => await _database.ResetAsync(TestContext.Current.CancellationToken);

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    private static async Task<UserId> CreateUserAsync(
        CreateUserCommand command,
        IdentityDbContext context,
        CancellationToken cancellationToken
    )
    {
        var createHandler = new CreateUserCommandHandler(new PasswordHasher<string>(), context);
        var response = await createHandler.HandleAsync(command, cancellationToken);
        return UserId.From(response.Id);
    }
}

