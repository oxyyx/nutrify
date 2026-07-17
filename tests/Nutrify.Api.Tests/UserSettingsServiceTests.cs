using Microsoft.EntityFrameworkCore;
using Nutrify.Api.Data;
using Nutrify.Api.Services;
using Nutrify.Contracts.Settings;

namespace Nutrify.Api.Tests;

public class UserSettingsServiceTests
{
    private static NutrifyDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<NutrifyDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new NutrifyDbContext(options);
    }

    private static UpdateUserSettingsRequest Request(
        decimal? calories = null, decimal? protein = null, decimal? carbs = null,
        decimal? fat = null, decimal? fiber = null) =>
        new(new NutritionTargetsDto(calories, protein, carbs, fat, fiber));

    [Fact]
    public async Task GetAsync_ReturnsEmptyTargets_WhenNoneSaved()
    {
        await using var db = CreateDb();
        var service = new UserSettingsService(db);

        var result = await service.GetAsync("user1");

        result.Targets.CaloriesKcal.Should().BeNull();
        result.Targets.ProteinG.Should().BeNull();
        // Nothing is persisted just from reading.
        (await db.UserSettings.AnyAsync()).Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_CreatesRowAndPersistsTargets()
    {
        await using var db = CreateDb();
        var service = new UserSettingsService(db);

        var updated = await service.UpdateAsync("user1", Request(calories: 2000, protein: 140, fiber: 30));

        updated.Targets.CaloriesKcal.Should().Be(2000);
        updated.Targets.ProteinG.Should().Be(140);
        updated.Targets.FiberG.Should().Be(30);
        updated.Targets.FatG.Should().BeNull();

        var reloaded = await service.GetAsync("user1");
        reloaded.Targets.CaloriesKcal.Should().Be(2000);
    }

    [Fact]
    public async Task UpdateAsync_UpsertsSingleRow_OnRepeatedSaves()
    {
        await using var db = CreateDb();
        var service = new UserSettingsService(db);

        await service.UpdateAsync("user1", Request(calories: 2000));
        await service.UpdateAsync("user1", Request(calories: 2200, protein: 150));

        (await db.UserSettings.CountAsync()).Should().Be(1);
        var result = await service.GetAsync("user1");
        result.Targets.CaloriesKcal.Should().Be(2200);
        result.Targets.ProteinG.Should().Be(150);
    }

    [Fact]
    public async Task UpdateAsync_CanClearATarget_BySettingNull()
    {
        await using var db = CreateDb();
        var service = new UserSettingsService(db);

        await service.UpdateAsync("user1", Request(calories: 2000, protein: 140));
        var cleared = await service.UpdateAsync("user1", Request(calories: 2000));

        cleared.Targets.ProteinG.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_RejectsNegativeTarget()
    {
        await using var db = CreateDb();
        var service = new UserSettingsService(db);

        var act = () => service.UpdateAsync("user1", Request(calories: -5));

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task Settings_AreIsolatedPerUser()
    {
        await using var db = CreateDb();
        var service = new UserSettingsService(db);

        await service.UpdateAsync("user1", Request(calories: 2000));

        var otherUser = await service.GetAsync("user2");
        otherUser.Targets.CaloriesKcal.Should().BeNull();
    }
}
