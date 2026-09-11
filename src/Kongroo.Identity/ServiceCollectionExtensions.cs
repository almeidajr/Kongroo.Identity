using Kongroo.BuildingBlocks.Application;
using Kongroo.BuildingBlocks.Infrastructure;
using Kongroo.Identity.Application;
using Kongroo.Identity.Application.Abstractions;
using Kongroo.Identity.Contracts;
using Kongroo.Identity.Infrastructure;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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

            services.AddDomainEventHandler<UserCreatedDomainEventHandler>();
            services.AddDomainEventHandler<UserRoleChangedDomainEventHandler>();
        }

        private void AddInfrastructure(IConfiguration configuration)
        {
            services.AddSingleton(TimeProvider.System);

            services.AddRelationalDbContext<IdentityDbContext>(contextOptions =>
                contextOptions.UseNpgsql(
                    configuration.GetConnectionString("Database"),
                    postgresOptions => postgresOptions.MigrationsHistoryTable("migrations", IdentityDbContext.Schema)
                )
            );
            services.AddDbInitializer<IdentityDbContext>();

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

            services.AddMessaging(configuration);
        }

        private void AddMessaging(IConfiguration configuration)
        {
            var transport = configuration.GetValue("Messaging:Transport", MessagingTransport.RabbitMq);

            if (transport == MessagingTransport.RabbitMq)
            {
                services
                    .AddOptions<RabbitMqTransportOptions>()
                    .Bind(configuration.GetRequiredSection("RabbitMq"))
                    .ValidateDataAnnotations()
                    .ValidateOnStart();
            }

            services.AddMassTransit(busRegistration =>
            {
                busRegistration.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("identity"));

                busRegistration.AddEntityFrameworkOutbox<IdentityDbContext>(outbox =>
                {
                    outbox.UsePostgres();
                    outbox.UseBusOutbox();
                });

                if (transport == MessagingTransport.AmazonSqs)
                {
                    var region = configuration.GetValue<string>("Aws:Region");
                    if (string.IsNullOrWhiteSpace(region))
                    {
                        throw new InvalidOperationException(
                            "Configuration value 'Aws:Region' is required when Messaging:Transport is AmazonSqs."
                        );
                    }

                    busRegistration.UsingAmazonSqs(
                        (context, busFactory) =>
                        {
                            // Credentials come from the AWS SDK default chain (AWS_ACCESS_KEY_ID,
                            // AWS_SECRET_ACCESS_KEY, AWS_SESSION_TOKEN) — nothing to configure here.
                            busFactory.Host(region, static _ => { });
                            busFactory.Message<UserCreatedIntegrationEvent>(static message =>
                                message.SetEntityName(MessagingTopics.UserCreated)
                            );
                            busFactory.Message<UserRoleChangedIntegrationEvent>(static message =>
                                message.SetEntityName(MessagingTopics.UserRoleChanged)
                            );
                            busFactory.ConfigureEndpoints(context);
                        }
                    );
                }
                else
                {
                    busRegistration.UsingRabbitMq((context, busFactory) => busFactory.ConfigureEndpoints(context));
                }
            });
        }
    }
}
