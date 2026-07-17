namespace Nutrify.Contracts.Settings;

// Container for per-user settings. Targets is the first section; future
// settings categories become sibling properties here.
public record UserSettingsDto(
    NutritionTargetsDto Targets
);
