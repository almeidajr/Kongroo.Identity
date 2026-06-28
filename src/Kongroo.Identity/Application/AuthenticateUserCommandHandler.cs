using Kongroo.BuildingBlocks.Application;
using Kongroo.Identity.Application.Abstractions;
using Kongroo.Identity.Domain;
using Kongroo.Identity.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Kongroo.Identity.Application;

public sealed class AuthenticateUserCommandHandler(
    IdentityDbContext context,
    IPasswordHasher<string> passwordHasher,
    IAccessTokenIssuer accessTokenIssuer
) : ICommandHandler<AuthenticateUserCommand, AuthenticateUserResponse?>
{
    public async Task<AuthenticateUserResponse?> HandleAsync(
        AuthenticateUserCommand command,
        CancellationToken cancellationToken
    )
    {
        var user = await context
            .Users.AsNoTracking()
            .SingleOrDefaultAsync(user => user.Username == Username.From(command.Username), cancellationToken);

        if (user is null || !HasValidPassword(user, command.Password))
        {
            return null;
        }

        return accessTokenIssuer.IssueToken(user);
    }

    private bool HasValidPassword(User user, string password)
    {
        var verificationResult = passwordHasher.VerifyHashedPassword(
            user.Username.Value,
            user.PasswordHash.Value,
            password
        );

        return verificationResult == PasswordVerificationResult.Success;
    }
}
