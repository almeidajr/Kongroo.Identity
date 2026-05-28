using Kongroo.BuildingBlocks.Application;
using Kongroo.Identity.Application;
using Kongroo.Identity.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Kongroo.Identity.Infrastructure;

public sealed class BootstrapAdminInitializer(
    ILogger<BootstrapAdminInitializer> logger,
    IOptions<BootstrapAdminOptions> options,
    IdentityDbContext context,
    CreateUserCommandHandler handler
) : IApplicationInitializer
{
    private readonly BootstrapAdminOptions _options = options.Value;

    public int Priority => 1;

    public async ValueTask<bool> IsEnabledAsync(CancellationToken cancellationToken) =>
        !await context.Users.AsNoTracking().AnyAsync(cancellationToken);

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        var response = await handler.HandleAsync(
            new CreateUserCommand(_options.Username, _options.Email, _options.Password, _options.Name, UserRole.Admin),
            cancellationToken
        );

        logger.LogInformation("Bootstrap admin user {Username} was created.", response.Username);
    }
}

