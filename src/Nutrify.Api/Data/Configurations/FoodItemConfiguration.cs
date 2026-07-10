using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nutrify.Api.Entities;

namespace Nutrify.Api.Data.Configurations;

public class FoodItemConfiguration : IEntityTypeConfiguration<FoodItem>
{
    public void Configure(EntityTypeBuilder<FoodItem> builder)
    {
        builder.ToTable("food_items");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(f => f.UserId)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(f => f.Unit)
            .HasMaxLength(5)
            .HasDefaultValue("g");

        builder.Property(f => f.Barcode)
            .HasMaxLength(64);

        builder.Property(f => f.ServingSize).HasPrecision(10, 2);

        builder.Property(f => f.ServingSizeName)
            .HasMaxLength(50);

        builder.Property(f => f.CaloriesKcal).HasPrecision(10, 2);
        builder.Property(f => f.ProteinG).HasPrecision(10, 2);
        builder.Property(f => f.CarbohydratesG).HasPrecision(10, 2);
        builder.Property(f => f.FatG).HasPrecision(10, 2);
        builder.Property(f => f.FiberG).HasPrecision(10, 2);

        builder.HasIndex(f => new { f.UserId, f.Name });
        builder.HasIndex(f => f.CategoryId);

        builder.HasIndex(f => new { f.UserId, f.Barcode })
            .IsUnique()
            .HasFilter("\"Barcode\" IS NOT NULL");

        builder.HasOne(f => f.Category)
            .WithMany(c => c.FoodItems)
            .HasForeignKey(f => f.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Property(f => f.CreatedAt)
            .HasDefaultValueSql("now()");

        builder.Property(f => f.UpdatedAt)
            .HasDefaultValueSql("now()");
    }
}
