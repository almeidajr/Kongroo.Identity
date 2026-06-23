using Kongroo.BuildingBlocks.Infrastructure;
using Kongroo.Identity.Domain;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Kongroo.Identity.Infrastructure;

public sealed class IdentityDbContext(DbContextOptions<IdentityDbContext> options)
    : RelationalDbContext<IdentityDbContext>(options),
        IRelationalDbContext
{
    public static string Schema => "identity";

    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();

        modelBuilder.ApplyConfiguration(new UserConfiguration());
    }
}
