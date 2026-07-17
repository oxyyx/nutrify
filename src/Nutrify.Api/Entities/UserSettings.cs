namespace Nutrify.Api.Entities;

/// <summary>
/// Per-user settings, one row per user (unique on <see cref="UserId"/>).
/// A row is created lazily the first time the user saves any setting.
/// </summary>
public class UserSettings
{
    public int Id { get; set; }
    public required string UserId { get; set; }

    // Daily nutrition targets. Null = no target set for that nutrient.
    public decimal? TargetCaloriesKcal { get; set; }
    public decimal? TargetProteinG { get; set; }
    public decimal? TargetCarbohydratesG { get; set; }
    public decimal? TargetFatG { get; set; }
    public decimal? TargetFiberG { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
