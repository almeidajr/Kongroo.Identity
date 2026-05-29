using Kongroo.BuildingBlocks.Domain.Exceptions;
using Kongroo.Identity.Domain;
using Kongroo.Identity.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Kongroo.Identity.Application;

public sealed class UpdateUserRoleCommandHandler(IdentityDbContext context)
{
    public async Task<GetUserResponse> HandleAsync(UpdateUserRoleCommand command, CancellationToken cancellationToken)
    {
        var user =
            await context
                .Users.Where(candidate => candidate.Id == UserId.From(command.TargetUserId))
                .SingleOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(nameof(User), $"identifier '{command.TargetUserId}'");

        ThrowIfSelfDemotion(command.ActingUserId, user.Id.Value, command.Role);

        if (command.Role == UserRole.Admin)
        {
            user.GrantAdmin();
        }
        else
        {
            user.RevokeAdmin();
        }

        await context.SaveChangesAsync(cancellationToken);

        return new GetUserResponse(user.Id.Value, user.Username.Value, user.Email.Value, user.Name.Value, user.Role);
    }

    private static void ThrowIfSelfDemotion(Guid actingUserId, Guid targetUserId, UserRole role)
    {
        if (actingUserId == targetUserId && role == UserRole.User)
        {
            throw new ConflictException(nameof(User), "admins cannot remove their own admin access");
        }
    }
}
