using Microsoft.EntityFrameworkCore;

namespace Kongroo.BuildingBlocks.Infrastructure;

public abstract class RelationalDbContext<TDbContext>(DbContextOptions options) : DbContext(options)
    where TDbContext : RelationalDbContext<TDbContext>, IRelationalDbContext
{
    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.HasDefaultSchema(TDbContext.Schema);
}
