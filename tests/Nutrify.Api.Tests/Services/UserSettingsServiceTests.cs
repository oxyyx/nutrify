using Microsoft.EntityFrameworkCore;
using Nutrify.Api.Services;
using Nutrify.Contracts.Settings;

namespace Nutrify.Api.Tests.Services;

public class UserSettingsServiceTests
{
    private static UpdateUserSettingsRequest Request(
        decimal? calories = null, decimal? protein = null, decimal? carbs = null,
        decimal? fat = null, decimal? fiber = null) =>
        new(new NutritionTargetsDto(calories, protein, carbs, fat, fiber));

    [Test]
    public async Task GetAsync_ReturnsEmptyTargets_WhenNoneSaved()
    {
        await using var db = TestDb.Create();
        var service = new UserSettingsService(db);

        var result = await service.GetAsync("user1");

        await Assert.That(result.Targets.CaloriesKcal).IsNull();
        await Assert.That(result.Targets.ProteinG).IsNull();
        // Nothing is persisted just from reading.
        await Assert.That(await db.UserSettings.AnyAsync()).IsFalse();
    }

    [Test]
    public async Task UpdateAsync_CreatesRowAndPersistsTargets()
    {
        await using var db = TestDb.Create();
        var service = new UserSettingsService(db);

        var updated = await service.UpdateAsync("user1", Request(calories: 2000, protein: 140, fiber: 30));

        await Assert.That(updated.Targets.CaloriesKcal).IsEqualTo(2000m);
        await Assert.That(updated.Targets.ProteinG).IsEqualTo(140m);
        await Assert.That(updated.Targets.FiberG).IsEqualTo(30m);
        await Assert.That(updated.Targets.FatG).IsNull();

        var reloaded = await service.GetAsync("user1");
        await Assert.That(reloaded.Targets.CaloriesKcal).IsEqualTo(2000m);
    }

    [Test]
    public async Task UpdateAsync_UpsertsSingleRow_OnRepeatedSaves()
    {
        await using var db = TestDb.Create();
        var service = new UserSettingsService(db);

        await service.UpdateAsync("user1", Request(calories: 2000));
        await service.UpdateAsync("user1", Request(calories: 2200, protein: 150));

        await Assert.That(await db.UserSettings.CountAsync()).IsEqualTo(1);
        var result = await service.GetAsync("user1");
        await Assert.That(result.Targets.CaloriesKcal).IsEqualTo(2200m);
        await Assert.That(result.Targets.ProteinG).IsEqualTo(150m);
    }

    [Test]
    public async Task UpdateAsync_CanClearATarget_BySettingNull()
    {
        await using var db = TestDb.Create();
        var service = new UserSettingsService(db);

        await service.UpdateAsync("user1", Request(calories: 2000, protein: 140));
        var cleared = await service.UpdateAsync("user1", Request(calories: 2000));

        await Assert.That(cleared.Targets.ProteinG).IsNull();
    }

    [Test]
    public async Task UpdateAsync_AcceptsZeroAsATarget()
    {
        await using var db = TestDb.Create();
        var service = new UserSettingsService(db);

        // Zero is a legitimate target; only negatives are rejected.
        var updated = await service.UpdateAsync("user1", Request(calories: 0));

        await Assert.That(updated.Targets.CaloriesKcal).IsEqualTo(0m);
    }

    [Test]
    public async Task UpdateAsync_RejectsNegativeTarget()
    {
        await using var db = TestDb.Create();
        var service = new UserSettingsService(db);

        await Assert.That(async () => await service.UpdateAsync("user1", Request(calories: -5)))
            .Throws<ArgumentException>();
    }

    [Test]
    public async Task UpdateAsync_RejectsANegativeValueInAnyField()
    {
        await using var db = TestDb.Create();
        var service = new UserSettingsService(db);

        await Assert.That(async () => await service.UpdateAsync("user1", Request(fiber: -1)))
            .Throws<ArgumentException>();
    }

    [Test]
    public async Task UpdateAsync_DoesNotPersistAnythingWhenValidationFails()
    {
        await using var db = TestDb.Create();
        var service = new UserSettingsService(db);

        await Assert.That(async () => await service.UpdateAsync("user1", Request(calories: -5)))
            .Throws<ArgumentException>();

        await Assert.That(await db.UserSettings.AnyAsync()).IsFalse();
    }

    [Test]
    public async Task Settings_AreIsolatedPerUser()
    {
        await using var db = TestDb.Create();
        var service = new UserSettingsService(db);

        await service.UpdateAsync("user1", Request(calories: 2000));

        var otherUser = await service.GetAsync("user2");
        await Assert.That(otherUser.Targets.CaloriesKcal).IsNull();
    }

    [Test]
    public async Task UpdateAsync_KeepsUsersSettingsSeparate()
    {
        await using var db = TestDb.Create();
        var service = new UserSettingsService(db);

        await service.UpdateAsync("user1", Request(calories: 2000));
        await service.UpdateAsync("user2", Request(calories: 2500));

        await Assert.That(await db.UserSettings.CountAsync()).IsEqualTo(2);
        await Assert.That((await service.GetAsync("user1")).Targets.CaloriesKcal).IsEqualTo(2000m);
        await Assert.That((await service.GetAsync("user2")).Targets.CaloriesKcal).IsEqualTo(2500m);
    }
}
