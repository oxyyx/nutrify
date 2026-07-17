using Microsoft.EntityFrameworkCore;
using Nutrify.Api.Data;
using Nutrify.Api.Entities;
using Nutrify.Api.Services;
using Nutrify.Contracts.Common;

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
            ConsumedAt = consumedAt
        });
        await db.SaveChangesAsync();
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
