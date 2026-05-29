using Kongroo.BuildingBlocks.Infrastructure;
using Kongroo.Identity.Domain;
using Microsoft.EntityFrameworkCore;

namespace Kongroo.Identity.Infrastructure;

public sealed class IdentityDbContext(DbContextOptions<IdentityDbContext> options)
    : OutboxDbContext<IdentityDbContext>(options),
        IRelationalDbContext
{
    public static string Schema => "identity";

    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new UserConfiguration());
    }
}
