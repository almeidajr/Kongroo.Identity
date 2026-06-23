using Kongroo.BuildingBlocks.Application;
using Kongroo.BuildingBlocks.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Kongroo.BuildingBlocks;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddBuildingBlocks(IConfiguration configuration)
        {
            services.AddSingleton(TimeProvider.System);
            services.AddHostedService<ApplicationInitializationService>();

            return services;
        }

        public IServiceCollection AddRelationalDbContext<TDbContext>(IConfiguration configuration)
            where TDbContext : DbContext, IRelationalDbContext
        {
            services.AddDbContext<TDbContext>(
                (serviceProvider, contextOptions) =>
                {
                    var environment = serviceProvider.GetRequiredService<IWebHostEnvironment>();

                    contextOptions
                        .UseNpgsql(
                            configuration.GetConnectionString("Database"),
                            postgresOptions => postgresOptions.MigrationsHistoryTable("migrations", TDbContext.Schema)
                        )
                        .UseSnakeCaseNamingConvention();

                    if (environment.IsDevelopment())
                    {
                        contextOptions.EnableDetailedErrors().EnableSensitiveDataLogging();
                    }
                }
            );
            services.AddApplicationInitializer<DbInitializer<TDbContext>>();
            services.AddScoped<IUnitOfWork, UnitOfWork<TDbContext>>();
            return services;
        }

        public IServiceCollection AddApplicationInitializer<TApplicationInitializer>()
            where TApplicationInitializer : class, IApplicationInitializer =>
            services.AddScoped<IApplicationInitializer, TApplicationInitializer>();
    }
}
