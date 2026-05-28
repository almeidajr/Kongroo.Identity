using Kongroo.Identity.Application;
using Kongroo.Identity.Application.Abstractions;
using Kongroo.Identity.Domain;
using Kongroo.Identity.Infrastructure;
using Kongroo.Identity.IntegrationTests.Fixtures;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using Shouldly;

namespace Kongroo.Identity.IntegrationTests.Identity.Application;

public sealed class AuthenticateUserCommandHandlerTests(PostgreSqlFixture postgreSqlFixture)
    : IClassFixture<PostgreSqlFixture>,
        IAsyncLifetime
{
    private readonly IAccessTokenIssuer _accessTokenIssuer = Substitute.For<IAccessTokenIssuer>();
    private readonly IdentityTestDatabase _database = new(postgreSqlFixture);
    private readonly PasswordHasher<string> _passwordHasher = new();

    [Fact]
    public async Task HandleAsync_WithValidCredentials_ShouldReturnIssuedAccessToken()
    {
        // Arrange
        const string password = "Sup3rSecure!Password";
        await using var context = _database.CreateDbContext();
        var userId = await CreateUserAsync(
            new CreateUserCommand("kongroo", "kongroo@example.com", password, "Kongroo Cloud Games", UserRole.User),
            context,
            TestContext.Current.CancellationToken
        );

        var expectedResponse = new AuthenticateUserResponse("access-token", "Bearer", 3600);
        _accessTokenIssuer.IssueToken(Arg.Any<User>()).Returns(expectedResponse);

        var handler = new AuthenticateUserCommandHandler(context, _passwordHasher, _accessTokenIssuer);

        // Act
        var response = await handler.HandleAsync(
            new AuthenticateUserCommand("kongroo", password),
            TestContext.Current.CancellationToken
        );

        // Assert
        response.ShouldBe(expectedResponse);
        _accessTokenIssuer.Received(1).IssueToken(Arg.Is<User>(candidate => candidate.Id == userId));
    }

    [Fact]
    public async Task HandleAsync_WhenUsernameDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        await using var context = _database.CreateDbContext();
        var handler = new AuthenticateUserCommandHandler(context, _passwordHasher, _accessTokenIssuer);

        // Act
        var response = await handler.HandleAsync(
            new AuthenticateUserCommand("missing-user", "Sup3rSecure!Password"),
            TestContext.Current.CancellationToken
        );

        // Assert
        response.ShouldBeNull();
        _accessTokenIssuer.DidNotReceive().IssueToken(Arg.Any<User>());
    }

    [Fact]
    public async Task HandleAsync_WhenPasswordIsInvalid_ShouldReturnNull()
    {
        // Arrange
        await using var context = _database.CreateDbContext();
        await CreateUserAsync(
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

        var handler = new AuthenticateUserCommandHandler(context, _passwordHasher, _accessTokenIssuer);

        // Act
        var response = await handler.HandleAsync(
            new AuthenticateUserCommand("kongroo", "WrongPassword!"),
            TestContext.Current.CancellationToken
        );

        // Assert
        response.ShouldBeNull();
        _accessTokenIssuer.DidNotReceive().IssueToken(Arg.Any<User>());
    }

    public async ValueTask InitializeAsync() => await _database.ResetAsync(TestContext.Current.CancellationToken);

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    private async Task<UserId> CreateUserAsync(
        CreateUserCommand command,
        IdentityDbContext context,
        CancellationToken cancellationToken
    )
    {
        var handler = new CreateUserCommandHandler(_passwordHasher, context);
        var response = await handler.HandleAsync(command, cancellationToken);
        return new UserId(response.Id);
    }
}

