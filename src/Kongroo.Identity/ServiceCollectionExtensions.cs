using Kongroo.BuildingBlocks;
using Kongroo.Identity.Application;
using Kongroo.Identity.Application.Abstractions;
using Kongroo.Identity.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Kongroo.Identity;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddIdentityModule(IConfiguration configuration)
        {
            services.AddValidation();
            services.AddApplication();
            services.AddInfrastructure(configuration);

            return services;
        }

        private void AddApplication()
        {
            services.AddScoped<AuthenticateUserCommandHandler>();
            services.AddScoped<CreateUserCommandHandler>();
            services.AddScoped<GetUserQueryHandler>();
            services.AddScoped<GetUsersQueryHandler>();
            services.AddScoped<UpdateUserRoleCommandHandler>();
        }

        private void AddInfrastructure(IConfiguration configuration)
        {
            services.AddOutboxDbContext<IdentityDbContext>(configuration);

            services
                .AddOptions<BootstrapAdminOptions>()
                .Bind(configuration.GetRequiredSection(BootstrapAdminOptions.SectionName))
                .ValidateDataAnnotations()
                .ValidateOnStart();
            services
                .AddOptions<JwtOptions>()
                .Bind(configuration.GetRequiredSection(JwtOptions.SectionName))
                .ValidateDataAnnotations()
                .ValidateOnStart();
            services.AddApplicationInitializer<BootstrapAdminInitializer>();
            services.AddSingleton<IAccessTokenIssuer, JwtAccessTokenIssuer>();
            services.AddSingleton<IPasswordHasher<string>, PasswordHasher<string>>();
        }
    }
}

