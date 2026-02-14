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

        builder.HasIndex(e => new { e.UserId, e.ConsumedAt });

        builder.HasOne(e => e.FoodItem)
            .WithMany(f => f.IntakeEntries)
            .HasForeignKey(e => e.FoodItemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(e => e.CreatedAt)
            .HasDefaultValueSql("now()");
    }
}
