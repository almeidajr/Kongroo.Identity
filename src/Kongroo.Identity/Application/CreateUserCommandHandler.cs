using Kongroo.BuildingBlocks.Application;
using Kongroo.BuildingBlocks.Domain.Exceptions;
using Kongroo.Identity.Domain;
using Kongroo.Identity.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Kongroo.Identity.Application;

public sealed class CreateUserCommandHandler(
    IPasswordHasher<string> passwordHasher,
    IdentityDbContext context,
    IUnitOfWork unitOfWork
) : ICommandHandler<CreateUserCommand, CreateUserResponse>
{
    public async Task<CreateUserResponse> HandleAsync(CreateUserCommand command, CancellationToken cancellationToken)
    {
        var username = Username.From(command.Username);
        var email = Email.From(command.Email);
        var name = PersonName.From(command.Name);

        await ThrowIfDuplicateAsync(username, email, cancellationToken);

        var passwordHash = PasswordHash.From(passwordHasher.HashPassword(username.Value, command.Password));
        var user = User.Create(username, email, passwordHash, name);

        if (command.Role == UserRole.Admin)
        {
            user.GrantAdmin();
        }

        context.Users.Add(user);
        await unitOfWork.CommitAsync(cancellationToken);

        return new CreateUserResponse(user.Id.Value, user.Username.Value, user.Email.Value, user.Name.Value);
    }

    private async Task ThrowIfDuplicateAsync(Username username, Email email, CancellationToken cancellationToken)
    {
        var hasDuplicate = await context
            .Users.AsNoTracking()
            .Where(user => user.Username == username || user.Email == email)
            .AnyAsync(cancellationToken);

        if (hasDuplicate)
        {
            throw new ConflictException(nameof(User), "username or email address is already in use");
        }
    }
}
