namespace Nutrify.Contracts.Settings;

public record UpdateUserSettingsRequest(
    NutritionTargetsDto Targets
);
