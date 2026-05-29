using Kongroo.BuildingBlocks.Domain.Exceptions;
using Kongroo.Identity.Application;
using Kongroo.Identity.Domain;
using Kongroo.Identity.Infrastructure;
using Kongroo.Identity.IntegrationTests.Fixtures;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace Kongroo.Identity.IntegrationTests.Identity.Application;

public sealed class CreateUserCommandHandlerTests(PostgreSqlFixture postgreSqlFixture)
    : IClassFixture<PostgreSqlFixture>,
        IAsyncLifetime
{
    private readonly IdentityTestDatabase _database = new(postgreSqlFixture);
    private readonly PasswordHasher<string> _passwordHasher = new();

    [Fact]
    public async Task HandleAsync_WithUniqueUsernameAndEmail_ShouldReturnCreatedUserResponse()
    {
        // Arrange
        await using var context = _database.CreateDbContext();
        var handler = CreateHandler(context);

        // Act
        var response = await handler.HandleAsync(CreateUniqueUserCommand(), TestContext.Current.CancellationToken);

        // Assert
        response.Id.ShouldNotBe(Guid.Empty);
        response.Username.ShouldBe("kongroo");
        response.Email.ShouldBe("kongroo@example.com");
        response.Name.ShouldBe("Kongroo Cloud Games");
    }

    [Fact]
    public async Task HandleAsync_WithUniqueUsernameAndEmail_ShouldPersistUser()
    {
        // Arrange
        await using var context = _database.CreateDbContext();
        var handler = CreateHandler(context);

        // Act
        var response = await handler.HandleAsync(CreateUniqueUserCommand(), TestContext.Current.CancellationToken);

        context.ChangeTracker.Clear();
        var savedUser = await context.Users.SingleAsync(
            user => user.Id == UserId.From(response.Id),
            TestContext.Current.CancellationToken
        );

        // Assert
        savedUser.ShouldSatisfyAllConditions(
            () => savedUser.Username.Value.ShouldBe(response.Username),
            () => savedUser.Email.Value.ShouldBe(response.Email),
            () => savedUser.Name.Value.ShouldBe(response.Name),
            () => savedUser.Role.ShouldBe(UserRole.User)
        );
    }

    [Fact]
    public async Task HandleAsync_WithUniqueUsernameAndEmail_ShouldPersistHashedPassword()
    {
        // Arrange
        const string password = "Sup3rSecure!Password";
        await using var context = _database.CreateDbContext();
        var handler = CreateHandler(context);

        // Act
        var response = await handler.HandleAsync(
            CreateUniqueUserCommand(password),
            TestContext.Current.CancellationToken
        );

        context.ChangeTracker.Clear();
        var savedUser = await context.Users.SingleAsync(
            user => user.Id == UserId.From(response.Id),
            TestContext.Current.CancellationToken
        );

        // Assert
        savedUser.PasswordHash.Value.ShouldNotBe(password);
        _passwordHasher
            .VerifyHashedPassword(savedUser.Username.Value, savedUser.PasswordHash.Value, password)
            .ShouldBe(PasswordVerificationResult.Success);
    }

    [Fact]
    public async Task HandleAsync_WhenUsernameAlreadyExists_ShouldThrowConflictException()
    {
        // Arrange
        await using var context = _database.CreateDbContext();
        var handler = CreateHandler(context);
        await handler.HandleAsync(CreateUniqueUserCommand(), TestContext.Current.CancellationToken);
        var command = new CreateUserCommand(
            "kongroo",
            "other@example.com",
            "Sup3rSecure!",
            "Another User",
            UserRole.User
        );

        // Act
        var exception = await Should.ThrowAsync<ConflictException>(() =>
            handler.HandleAsync(command, TestContext.Current.CancellationToken)
        );

        // Assert
        exception.ResourceName.ShouldBe(nameof(User));
        exception.Reason.ShouldBe("username or email address is already in use");
    }

    [Fact]
    public async Task HandleAsync_WhenEmailAlreadyExists_ShouldThrowConflictException()
    {
        // Arrange
        await using var context = _database.CreateDbContext();
        var handler = CreateHandler(context);
        await handler.HandleAsync(CreateUniqueUserCommand(), TestContext.Current.CancellationToken);
        var command = new CreateUserCommand(
            "otheruser",
            "kongroo@example.com",
            "Sup3rSecure!",
            "Another User",
            UserRole.User
        );

        // Act
        var exception = await Should.ThrowAsync<ConflictException>(() =>
            handler.HandleAsync(command, TestContext.Current.CancellationToken)
        );

        // Assert
        exception.ResourceName.ShouldBe(nameof(User));
        exception.Reason.ShouldBe("username or email address is already in use");
    }

    [Theory]
    [InlineData("kongroo", "other@example.com")]
    [InlineData("otheruser", "kongroo@example.com")]
    public async Task HandleAsync_WhenUsernameOrEmailAlreadyExists_ShouldNotPersistAnotherUser(
        string username,
        string email
    )
    {
        // Arrange
        await using var context = _database.CreateDbContext();
        var handler = CreateHandler(context);
        await handler.HandleAsync(CreateUniqueUserCommand(), TestContext.Current.CancellationToken);
        var command = new CreateUserCommand(username, email, "Sup3rSecure!", "Another User", UserRole.User);

        // Act
        await Should.ThrowAsync<ConflictException>(() =>
            handler.HandleAsync(command, TestContext.Current.CancellationToken)
        );

        // Assert
        context.ChangeTracker.Clear();
        (await context.Users.CountAsync(TestContext.Current.CancellationToken)).ShouldBe(1);
    }

    public async ValueTask InitializeAsync() => await _database.ResetAsync(TestContext.Current.CancellationToken);

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task HandleAsync_WithAdminRole_ShouldPersistAdminUser()
    {
        // Arrange
        await using var context = _database.CreateDbContext();
        var handler = CreateHandler(context);

        // Act
        var response = await handler.HandleAsync(
            new CreateUserCommand(
                "kongroo-admin",
                "kongroo-admin@example.com",
                "Sup3rSecure!",
                "Kongroo Admin",
                UserRole.Admin
            ),
            TestContext.Current.CancellationToken
        );

        context.ChangeTracker.Clear();
        var savedUser = await context.Users.SingleAsync(
            user => user.Id == UserId.From(response.Id),
            TestContext.Current.CancellationToken
        );

        // Assert
        savedUser.Role.ShouldBe(UserRole.Admin);
    }

    private CreateUserCommandHandler CreateHandler(IdentityDbContext context) => new(_passwordHasher, context);

    private static CreateUserCommand CreateUniqueUserCommand(string password = "Sup3rSecure!") =>
        new("kongroo", "kongroo@example.com", password, "Kongroo Cloud Games", UserRole.User);
}
