using Kongroo.BuildingBlocks.Application;
using Kongroo.BuildingBlocks.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Kongroo.BuildingBlocks;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddBuildingBlocks(IConfiguration configuration)
        {
            services.AddSingleton(TimeProvider.System);
            services.AddApplication();
            services.AddHostedService<ApplicationInitializationService>();

            return services;
        }

        public IServiceCollection AddRelationalDbContext<TDbContext>(IConfiguration configuration)
            where TDbContext : DbContext, IRelationalDbContext
        {
            services.AddRelationalDbContext<TDbContext>(opts =>
                opts.UseNpgsql(
                    configuration.GetConnectionString("Database"),
                    pg => pg.MigrationsHistoryTable("migrations", TDbContext.Schema)
                )
            );
            services.AddDbInitializer<TDbContext>();
            return services;
        }
    }
}
