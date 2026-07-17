using Microsoft.EntityFrameworkCore;
using Nutrify.Api.Data;
using Nutrify.Api.Entities;
using Nutrify.Api.Services;
using Nutrify.Contracts.Common;
using Nutrify.Contracts.Intake;

namespace Nutrify.Api.Tests;

public class IntakeServiceTests
{
    private static NutrifyDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<NutrifyDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new NutrifyDbContext(options);
    }

    private static async Task SeedEntryAsync(
        NutrifyDbContext db, string userId, string foodName, DateTime consumedAt)
    {
        var foodItem = new FoodItem { Name = foodName, UserId = userId };
        db.FoodItems.Add(foodItem);
        db.IntakeEntries.Add(new IntakeEntry
        {
            UserId = userId,
            FoodItem = foodItem,
            Amount = 100,
            ConsumedAt = consumedAt,
            FoodItemName = foodName,
            FoodItemUnit = "g"
        });
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task CreateAsync_SnapshotsFoodItemNutrition()
    {
        await using var db = CreateDb();
        var service = new IntakeService(db);
        var foodItem = new FoodItem
        {
            Name = "Oatmeal",
            Unit = "g",
            UserId = "user1",
            CaloriesKcal = 380,
            ProteinG = 13,
            CarbohydratesG = 60,
            FatG = 7,
            FiberG = 10
        };
        db.FoodItems.Add(foodItem);
        await db.SaveChangesAsync();

        var dto = await service.CreateAsync("user1", new CreateIntakeEntryRequest(foodItem.Id, 50, null));

        // DTO reports totals for the logged amount (50g -> half the per-100g values).
        dto.FoodItemName.Should().Be("Oatmeal");
        dto.CaloriesKcal.Should().Be(190);
        dto.ProteinG.Should().Be(6.5m);

        // The entry stores the per-100g snapshot verbatim, independent of amount.
        var entity = await db.IntakeEntries.SingleAsync();
        entity.FoodItemName.Should().Be("Oatmeal");
        entity.FoodItemUnit.Should().Be("g");
        entity.CaloriesKcal.Should().Be(380);
        entity.ProteinG.Should().Be(13);
    }

    [Fact]
    public async Task CreateAsync_SnapshotSurvivesFoodItemChange()
    {
        await using var db = CreateDb();
        var service = new IntakeService(db);
        var foodItem = new FoodItem { Name = "Oatmeal", Unit = "g", UserId = "user1", CaloriesKcal = 380 };
        db.FoodItems.Add(foodItem);
        await db.SaveChangesAsync();

        await service.CreateAsync("user1", new CreateIntakeEntryRequest(foodItem.Id, 100, null));

        // Mutating the food item after the fact must not rewrite logged history.
        foodItem.Name = "Renamed Oats";
        foodItem.CaloriesKcal = 999;
        await db.SaveChangesAsync();

        var result = await service.GetEntriesAsync("user1", new PaginationRequest(1, 20));
        result.Items.Should().ContainSingle()
            .Which.Should().Match<IntakeEntryDto>(e => e.FoodItemName == "Oatmeal" && e.CaloriesKcal == 380);
    }

    [Theory]
    [InlineData("Oat")]
    [InlineData("oat")]
    [InlineData("OATMEAL")]
    public async Task GetEntriesAsync_FiltersByFoodItemNameCaseInsensitively(string search)
    {
        await using var db = CreateDb();
        var service = new IntakeService(db);
        await SeedEntryAsync(db, "user1", "Oatmeal", DateTime.UtcNow);
        await SeedEntryAsync(db, "user1", "Chicken Breast", DateTime.UtcNow);

        var result = await service.GetEntriesAsync("user1", new PaginationRequest(1, 20), search: search);

        result.TotalCount.Should().Be(1);
        result.Items.Should().ContainSingle(e => e.FoodItemName == "Oatmeal");
    }

    [Fact]
    public async Task GetEntriesAsync_CombinesSearchWithDateRange()
    {
        await using var db = CreateDb();
        var service = new IntakeService(db);
        await SeedEntryAsync(db, "user1", "Oatmeal", new DateTime(2026, 7, 1, 8, 0, 0, DateTimeKind.Utc));
        await SeedEntryAsync(db, "user1", "Oatmeal", new DateTime(2026, 7, 10, 8, 0, 0, DateTimeKind.Utc));
        await SeedEntryAsync(db, "user1", "Chicken Breast", new DateTime(2026, 7, 10, 12, 0, 0, DateTimeKind.Utc));

        var result = await service.GetEntriesAsync(
            "user1",
            new PaginationRequest(1, 20),
            from: new DateOnly(2026, 7, 5),
            to: new DateOnly(2026, 7, 15),
            search: "Oat");

        result.TotalCount.Should().Be(1);
        result.Items.Should().ContainSingle(e => e.ConsumedAt == new DateTime(2026, 7, 10, 8, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public async Task GetEntriesAsync_SearchDoesNotLeakOtherUsersEntries()
    {
        await using var db = CreateDb();
        var service = new IntakeService(db);
        await SeedEntryAsync(db, "user1", "Oatmeal", DateTime.UtcNow);
        await SeedEntryAsync(db, "user2", "Oatmeal", DateTime.UtcNow);

        var result = await service.GetEntriesAsync("user1", new PaginationRequest(1, 20), search: "Oat");

        result.TotalCount.Should().Be(1);
    }
}
