using Microsoft.EntityFrameworkCore;
using Nutrify.Api.Data;
using Nutrify.Api.Entities;
using Nutrify.Api.Mapping;
using Nutrify.Contracts.Settings;

namespace Nutrify.Api.Services;

public class UserSettingsService(NutrifyDbContext db) : IUserSettingsService
{
    public async Task<UserSettingsDto> GetAsync(string userId)
    {
        var settings = await db.UserSettings
            .FirstOrDefaultAsync(s => s.UserId == userId);

        // No row until the user saves something; report empty (all-null) targets.
        return settings?.ToDto()
            ?? new UserSettingsDto(new NutritionTargetsDto(null, null, null, null, null));
    }

    public async Task<UserSettingsDto> UpdateAsync(string userId, UpdateUserSettingsRequest request)
    {
        var targets = request.Targets;
        ValidateNonNegative(targets);

        // Upsert: one row per user, created lazily on first save.
        var settings = await db.UserSettings
            .FirstOrDefaultAsync(s => s.UserId == userId);

        if (settings is null)
        {
            settings = new UserSettings { UserId = userId };
            db.UserSettings.Add(settings);
        }

        settings.TargetCaloriesKcal = targets.CaloriesKcal;
        settings.TargetProteinG = targets.ProteinG;
        settings.TargetCarbohydratesG = targets.CarbohydratesG;
        settings.TargetFatG = targets.FatG;
        settings.TargetFiberG = targets.FiberG;
        settings.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();

        return settings.ToDto();
    }

    private static void ValidateNonNegative(NutritionTargetsDto targets)
    {
        decimal?[] values =
        [
            targets.CaloriesKcal,
            targets.ProteinG,
            targets.CarbohydratesG,
            targets.FatG,
            targets.FiberG
        ];

        if (values.Any(v => v is < 0))
            throw new ArgumentException("Nutrition targets cannot be negative.");
    }
}
