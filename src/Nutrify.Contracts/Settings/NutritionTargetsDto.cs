namespace Nutrify.Contracts.Settings;

/// <summary>
/// Daily targets per nutrient. A null value means no target is set for that
/// nutrient (the user can target only the ones they care about).
/// </summary>
public record NutritionTargetsDto(
    decimal? CaloriesKcal,
    decimal? ProteinG,
    decimal? CarbohydratesG,
    decimal? FatG,
    decimal? FiberG
);
