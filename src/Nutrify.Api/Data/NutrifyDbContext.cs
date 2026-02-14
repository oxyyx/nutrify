using Microsoft.EntityFrameworkCore;
using Nutrify.Api.Entities;

namespace Nutrify.Api.Data;

public class NutrifyDbContext(DbContextOptions<NutrifyDbContext> options) : DbContext(options)
{
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<FoodItem> FoodItems => Set<FoodItem>();
    public DbSet<IntakeEntry> IntakeEntries => Set<IntakeEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NutrifyDbContext).Assembly);
    }
}
