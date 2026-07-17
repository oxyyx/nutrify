using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nutrify.Api.Entities;

namespace Nutrify.Api.Data.Configurations;

public class UserSettingsConfiguration : IEntityTypeConfiguration<UserSettings>
{
    public void Configure(EntityTypeBuilder<UserSettings> builder)
    {
        builder.ToTable("user_settings");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.UserId)
            .HasMaxLength(128)
            .IsRequired();

        // One settings row per user.
        builder.HasIndex(s => s.UserId).IsUnique();

        builder.Property(s => s.TargetCaloriesKcal).HasPrecision(10, 2);
        builder.Property(s => s.TargetProteinG).HasPrecision(10, 2);
        builder.Property(s => s.TargetCarbohydratesG).HasPrecision(10, 2);
        builder.Property(s => s.TargetFatG).HasPrecision(10, 2);
        builder.Property(s => s.TargetFiberG).HasPrecision(10, 2);

        builder.Property(s => s.CreatedAt)
            .HasDefaultValueSql("now()");

        builder.Property(s => s.UpdatedAt)
            .HasDefaultValueSql("now()");
    }
}
