using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nutrify.Api.Entities;

namespace Nutrify.Api.Data.Configurations;

public class IntakeEntryConfiguration : IEntityTypeConfiguration<IntakeEntry>
{
    public void Configure(EntityTypeBuilder<IntakeEntry> builder)
    {
        builder.ToTable("intake_entries");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.UserId)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(e => e.Amount)
            .HasPrecision(10, 2)
            .IsRequired();

        builder.Property(e => e.FoodItemName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.FoodItemUnit)
            .HasMaxLength(5)
            .IsRequired();

        builder.Property(e => e.CaloriesKcal).HasPrecision(10, 2);
        builder.Property(e => e.ProteinG).HasPrecision(10, 2);
        builder.Property(e => e.CarbohydratesG).HasPrecision(10, 2);
        builder.Property(e => e.FatG).HasPrecision(10, 2);
        builder.Property(e => e.FiberG).HasPrecision(10, 2);

        builder.HasIndex(e => new { e.UserId, e.ConsumedAt });

        // Optional link: deleting a food item nulls this FK (the snapshot above
        // keeps the entry intact) rather than being blocked or cascading.
        builder.HasOne(e => e.FoodItem)
            .WithMany(f => f.IntakeEntries)
            .HasForeignKey(e => e.FoodItemId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Property(e => e.CreatedAt)
            .HasDefaultValueSql("now()");
    }
}
