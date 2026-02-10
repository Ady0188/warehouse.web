using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace Warehouse.Web.Stores.Data;

internal class StoreDbContext : DbContext
{
  public StoreDbContext(DbContextOptions<StoreDbContext> options) : base(options)
  {
  }

  public DbSet<Store> Stores { get; set; } = null!;

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.HasDefaultSchema("Stores");

    modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
  }
}
