using Kongroo.BuildingBlocks.Domain.Exceptions;
using Kongroo.Identity.Domain;
using Kongroo.Identity.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Kongroo.Identity.Application;

public sealed class GetUserQueryHandler(IdentityDbContext context)
{
    public async Task<GetUserResponse> HandleAsync(GetUserQuery query, CancellationToken cancellationToken) =>
        await context
            .Users.AsNoTracking()
            .Where(user => user.Id == UserId.From(query.UserId))
            .Select(user => new GetUserResponse(
                user.Id.Value,
                user.Username.Value,
                user.Email.Value,
                user.Name.Value,
                user.Role
            ))
            .SingleOrDefaultAsync(cancellationToken)
        ?? throw new NotFoundException(nameof(User), $"identifier '{query.UserId}'");
}
