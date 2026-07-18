using Microsoft.EntityFrameworkCore;
using Nutrify.Api.Data;
using Nutrify.Api.Services;
using Nutrify.Contracts.Common;
using Nutrify.Contracts.FoodItems;
using Nutrify.Contracts.Intake;
using TUnit.Assertions.Enums;

namespace Nutrify.Api.Tests.Services;

public class IntakeServiceTests
{
    private static async Task SeedEntryAsync(
        NutrifyDbContext db, string userId, string foodName, DateTime consumedAt)
    {
        var foodItem = TestDb.NewFood(userId, foodName);
        db.FoodItems.Add(foodItem);
        db.IntakeEntries.Add(TestDb.NewEntry(userId, foodName, consumedAt, foodItem: foodItem));
        await db.SaveChangesAsync();
    }

    // ---- Snapshotting ----------------------------------------------------

    [Test]
    public async Task CreateAsync_SnapshotsFoodItemNutrition()
    {
        await using var db = TestDb.Create();
        var service = new IntakeService(db);
        var foodItem = TestDb.NewFood(
            "user1", "Oatmeal", calories: 380, protein: 13, carbs: 60, fat: 7, fiber: 10);
        db.FoodItems.Add(foodItem);
        await db.SaveChangesAsync();

        var dto = await service.CreateAsync("user1", new CreateIntakeEntryRequest(foodItem.Id, 50, null));

        // DTO reports totals for the logged amount (50g -> half the per-100g values).
        await Assert.That(dto.FoodItemName).IsEqualTo("Oatmeal");
        await Assert.That(dto.CaloriesKcal).IsEqualTo(190m);
        await Assert.That(dto.ProteinG).IsEqualTo(6.5m);

        // The entry stores the per-100g snapshot verbatim, independent of amount.
        var entity = await db.IntakeEntries.SingleAsync();
        await Assert.That(entity.FoodItemName).IsEqualTo("Oatmeal");
        await Assert.That(entity.FoodItemUnit).IsEqualTo("g");
        await Assert.That(entity.CaloriesKcal).IsEqualTo(380m);
        await Assert.That(entity.ProteinG).IsEqualTo(13m);
    }

    [Test]
    public async Task CreateAsync_SnapshotSurvivesFoodItemChange()
    {
        await using var db = TestDb.Create();
        var service = new IntakeService(db);
        var foodItem = TestDb.NewFood("user1", "Oatmeal", calories: 380);
        db.FoodItems.Add(foodItem);
        await db.SaveChangesAsync();

        await service.CreateAsync("user1", new CreateIntakeEntryRequest(foodItem.Id, 100, null));

        // Mutating the food item after the fact must not rewrite logged history.
        foodItem.Name = "Renamed Oats";
        foodItem.CaloriesKcal = 999;
        await db.SaveChangesAsync();

        var result = await service.GetEntriesAsync("user1", new PaginationRequest(1, 20));
        var entry = result.Items.Single();
        await Assert.That(entry.FoodItemName).IsEqualTo("Oatmeal");
        await Assert.That(entry.CaloriesKcal).IsEqualTo(380m);
    }

    [Test]
    public async Task CreateAsync_SnapshotsDrinkUnit()
    {
        await using var db = TestDb.Create();
        var service = new IntakeService(db);
        var drink = TestDb.NewFood(
            "user1", "Monster Energy", FoodItemType.Drink, "mL", calories: 47);
        db.FoodItems.Add(drink);
        await db.SaveChangesAsync();

        var dto = await service.CreateAsync("user1", new CreateIntakeEntryRequest(drink.Id, 500, null));

        await Assert.That(dto.FoodItemUnit).IsEqualTo("mL");
        await Assert.That(dto.CaloriesKcal).IsEqualTo(235m);
    }

    // ---- Creation guards -------------------------------------------------

    [Test]
    public async Task CreateAsync_RejectsUnknownFoodItem()
    {
        await using var db = TestDb.Create();
        var service = new IntakeService(db);

        await Assert.That(async () => await service.CreateAsync(
                "user1", new CreateIntakeEntryRequest(4321, 100, null)))
            .Throws<KeyNotFoundException>();
    }

    [Test]
    public async Task CreateAsync_RejectsAnotherUsersFoodItem()
    {
        await using var db = TestDb.Create();
        var service = new IntakeService(db);
        var foreign = TestDb.NewFood("user2", "Oatmeal", calories: 380);
        db.FoodItems.Add(foreign);
        await db.SaveChangesAsync();

        // Must be indistinguishable from a missing id, so ids can't be probed.
        await Assert.That(async () => await service.CreateAsync(
                "user1", new CreateIntakeEntryRequest(foreign.Id, 100, null)))
            .Throws<KeyNotFoundException>();
        await Assert.That(await db.IntakeEntries.AnyAsync()).IsFalse();
    }

    [Test]
    public async Task CreateAsync_DefaultsConsumedAtToNow_WhenOmitted()
    {
        await using var db = TestDb.Create();
        var service = new IntakeService(db);
        var foodItem = TestDb.NewFood("user1");
        db.FoodItems.Add(foodItem);
        await db.SaveChangesAsync();

        var before = DateTime.UtcNow;
        var dto = await service.CreateAsync("user1", new CreateIntakeEntryRequest(foodItem.Id, 100, null));
        var after = DateTime.UtcNow;

        await Assert.That(dto.ConsumedAt).IsGreaterThanOrEqualTo(before);
        await Assert.That(dto.ConsumedAt).IsLessThanOrEqualTo(after);
    }

    [Test]
    public async Task CreateAsync_HonoursExplicitConsumedAt()
    {
        await using var db = TestDb.Create();
        var service = new IntakeService(db);
        var foodItem = TestDb.NewFood("user1");
        db.FoodItems.Add(foodItem);
        await db.SaveChangesAsync();
        var backdated = new DateTime(2026, 3, 4, 9, 30, 0, DateTimeKind.Utc);

        var dto = await service.CreateAsync(
            "user1", new CreateIntakeEntryRequest(foodItem.Id, 100, backdated));

        await Assert.That(dto.ConsumedAt).IsEqualTo(backdated);
    }

    // ---- Update / delete -------------------------------------------------

    [Test]
    public async Task UpdateAsync_RecalculatesTotalsFromTheNewAmount()
    {
        await using var db = TestDb.Create();
        var service = new IntakeService(db);
        var foodItem = TestDb.NewFood("user1", "Oatmeal", calories: 380, protein: 13);
        db.FoodItems.Add(foodItem);
        await db.SaveChangesAsync();
        var created = await service.CreateAsync(
            "user1", new CreateIntakeEntryRequest(foodItem.Id, 100, null));
        var newTime = new DateTime(2026, 5, 1, 7, 0, 0, DateTimeKind.Utc);

        var updated = await service.UpdateAsync(
            created.Id, "user1", new UpdateIntakeEntryRequest(200, newTime));

        await Assert.That(updated!.Amount).IsEqualTo(200m);
        await Assert.That(updated.ConsumedAt).IsEqualTo(newTime);
        await Assert.That(updated.CaloriesKcal).IsEqualTo(760m);
        await Assert.That(updated.ProteinG).IsEqualTo(26m);

        // The per-100g snapshot itself is untouched by an amount change.
        var entity = await db.IntakeEntries.SingleAsync();
        await Assert.That(entity.CaloriesKcal).IsEqualTo(380m);
    }

    [Test]
    public async Task UpdateAsync_ReturnsNullForAnotherUsersEntry()
    {
        await using var db = TestDb.Create();
        var service = new IntakeService(db);
        await SeedEntryAsync(db, "user1", "Oatmeal", DateTime.UtcNow);
        var entry = await db.IntakeEntries.SingleAsync();

        var updated = await service.UpdateAsync(
            entry.Id, "user2", new UpdateIntakeEntryRequest(999, DateTime.UtcNow));

        await Assert.That(updated).IsNull();
        var untouched = await db.IntakeEntries.SingleAsync();
        await Assert.That(untouched.Amount).IsEqualTo(100m);
    }

    [Test]
    public async Task DeleteAsync_RemovesTheUsersOwnEntry()
    {
        await using var db = TestDb.Create();
        var service = new IntakeService(db);
        await SeedEntryAsync(db, "user1", "Oatmeal", DateTime.UtcNow);
        var entry = await db.IntakeEntries.SingleAsync();

        await Assert.That(await service.DeleteAsync(entry.Id, "user1")).IsTrue();
        await Assert.That(await db.IntakeEntries.AnyAsync()).IsFalse();
    }

    [Test]
    public async Task DeleteAsync_ReturnsFalseForAnotherUsersEntry()
    {
        await using var db = TestDb.Create();
        var service = new IntakeService(db);
        await SeedEntryAsync(db, "user1", "Oatmeal", DateTime.UtcNow);
        var entry = await db.IntakeEntries.SingleAsync();

        await Assert.That(await service.DeleteAsync(entry.Id, "user2")).IsFalse();
        await Assert.That(await db.IntakeEntries.CountAsync()).IsEqualTo(1);
    }

    [Test]
    public async Task GetByIdAsync_IsScopedToTheCaller()
    {
        await using var db = TestDb.Create();
        var service = new IntakeService(db);
        await SeedEntryAsync(db, "user1", "Oatmeal", DateTime.UtcNow);
        var entry = await db.IntakeEntries.SingleAsync();

        await Assert.That(await service.GetByIdAsync(entry.Id, "user1")).IsNotNull();
        await Assert.That(await service.GetByIdAsync(entry.Id, "user2")).IsNull();
    }

    // ---- Search ----------------------------------------------------------

    [Test]
    [Arguments("Oat")]
    [Arguments("oat")]
    [Arguments("OATMEAL")]
    public async Task GetEntriesAsync_FiltersByFoodItemNameCaseInsensitively(string search)
    {
        await using var db = TestDb.Create();
        var service = new IntakeService(db);
        await SeedEntryAsync(db, "user1", "Oatmeal", DateTime.UtcNow);
        await SeedEntryAsync(db, "user1", "Chicken Breast", DateTime.UtcNow);

        var result = await service.GetEntriesAsync("user1", new PaginationRequest(1, 20), search: search);

        await Assert.That(result.TotalCount).IsEqualTo(1);
        await Assert.That(result.Items.Single().FoodItemName).IsEqualTo("Oatmeal");
    }

    [Test]
    public async Task GetEntriesAsync_SearchesTheSnapshotAfterTheFoodItemIsDeleted()
    {
        await using var db = TestDb.Create();
        var service = new IntakeService(db);
        await SeedEntryAsync(db, "user1", "Oatmeal", DateTime.UtcNow);
        db.FoodItems.RemoveRange(await db.FoodItems.ToListAsync());
        await db.SaveChangesAsync();

        // History stays searchable because the name lives on the entry itself.
        var result = await service.GetEntriesAsync("user1", new PaginationRequest(1, 20), search: "Oat");

        await Assert.That(result.TotalCount).IsEqualTo(1);
    }

    [Test]
    public async Task GetEntriesAsync_SearchDoesNotLeakOtherUsersEntries()
    {
        await using var db = TestDb.Create();
        var service = new IntakeService(db);
        await SeedEntryAsync(db, "user1", "Oatmeal", DateTime.UtcNow);
        await SeedEntryAsync(db, "user2", "Oatmeal", DateTime.UtcNow);

        var result = await service.GetEntriesAsync("user1", new PaginationRequest(1, 20), search: "Oat");

        await Assert.That(result.TotalCount).IsEqualTo(1);
    }

    // ---- Date filtering --------------------------------------------------

    [Test]
    public async Task GetEntriesAsync_CombinesSearchWithDateRange()
    {
        await using var db = TestDb.Create();
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

        await Assert.That(result.TotalCount).IsEqualTo(1);
        await Assert.That(result.Items.Single().ConsumedAt)
            .IsEqualTo(new DateTime(2026, 7, 10, 8, 0, 0, DateTimeKind.Utc));
    }

    [Test]
    public async Task GetEntriesAsync_DateFilterCoversTheWholeDayInclusive()
    {
        await using var db = TestDb.Create();
        var service = new IntakeService(db);
        // Midnight and 23:59:59 both belong to the 10th; 00:00 on the 11th does not.
        await SeedEntryAsync(db, "user1", "A", new DateTime(2026, 7, 10, 0, 0, 0, DateTimeKind.Utc));
        await SeedEntryAsync(db, "user1", "B", new DateTime(2026, 7, 10, 23, 59, 59, DateTimeKind.Utc));
        await SeedEntryAsync(db, "user1", "C", new DateTime(2026, 7, 11, 0, 0, 0, DateTimeKind.Utc));

        var result = await service.GetEntriesAsync(
            "user1", new PaginationRequest(1, 20), date: new DateOnly(2026, 7, 10));

        await Assert.That(result.TotalCount).IsEqualTo(2);
        await Assert.That(result.Items.Select(e => e.FoodItemName).ToList())
            .IsEquivalentTo(new List<string> { "B", "A" });
    }

    [Test]
    public async Task GetEntriesAsync_ToBoundIsInclusiveOfThatDay()
    {
        await using var db = TestDb.Create();
        var service = new IntakeService(db);
        await SeedEntryAsync(db, "user1", "A", new DateTime(2026, 7, 15, 22, 0, 0, DateTimeKind.Utc));
        await SeedEntryAsync(db, "user1", "B", new DateTime(2026, 7, 16, 1, 0, 0, DateTimeKind.Utc));

        var result = await service.GetEntriesAsync(
            "user1", new PaginationRequest(1, 20), to: new DateOnly(2026, 7, 15));

        await Assert.That(result.TotalCount).IsEqualTo(1);
        await Assert.That(result.Items.Single().FoodItemName).IsEqualTo("A");
    }

    [Test]
    public async Task GetEntriesAsync_DateTakesPrecedenceOverFromAndTo()
    {
        await using var db = TestDb.Create();
        var service = new IntakeService(db);
        await SeedEntryAsync(db, "user1", "A", new DateTime(2026, 7, 10, 8, 0, 0, DateTimeKind.Utc));
        await SeedEntryAsync(db, "user1", "B", new DateTime(2026, 7, 20, 8, 0, 0, DateTimeKind.Utc));

        // `date` is an exact-day filter; from/to are ignored when it is supplied.
        var result = await service.GetEntriesAsync(
            "user1",
            new PaginationRequest(1, 20),
            date: new DateOnly(2026, 7, 10),
            from: new DateOnly(2026, 7, 15),
            to: new DateOnly(2026, 7, 25));

        await Assert.That(result.TotalCount).IsEqualTo(1);
        await Assert.That(result.Items.Single().FoodItemName).IsEqualTo("A");
    }

    // ---- Ordering and paging ---------------------------------------------

    [Test]
    public async Task GetEntriesAsync_ReturnsNewestFirst()
    {
        await using var db = TestDb.Create();
        var service = new IntakeService(db);
        await SeedEntryAsync(db, "user1", "Oldest", new DateTime(2026, 7, 1, 8, 0, 0, DateTimeKind.Utc));
        await SeedEntryAsync(db, "user1", "Newest", new DateTime(2026, 7, 20, 8, 0, 0, DateTimeKind.Utc));
        await SeedEntryAsync(db, "user1", "Middle", new DateTime(2026, 7, 10, 8, 0, 0, DateTimeKind.Utc));

        var result = await service.GetEntriesAsync("user1", new PaginationRequest(1, 20));

        await Assert.That(result.Items.Select(e => e.FoodItemName).ToList())
            .IsEquivalentTo(new List<string> { "Newest", "Middle", "Oldest" }, CollectionOrdering.Matching);
    }

    [Test]
    public async Task GetEntriesAsync_PagesResults()
    {
        await using var db = TestDb.Create();
        var service = new IntakeService(db);
        for (var day = 1; day <= 5; day++)
        {
            await SeedEntryAsync(
                db, "user1", $"Day {day}", new DateTime(2026, 7, day, 8, 0, 0, DateTimeKind.Utc));
        }

        var page2 = await service.GetEntriesAsync("user1", new PaginationRequest(2, 2));

        await Assert.That(page2.TotalCount).IsEqualTo(5);
        await Assert.That(page2.TotalPages).IsEqualTo(3);
        // Newest first, so page 2 of 2-per-page is days 3 and 2.
        await Assert.That(page2.Items.Select(e => e.FoodItemName).ToList())
            .IsEquivalentTo(new List<string> { "Day 3", "Day 2" }, CollectionOrdering.Matching);
    }

    [Test]
    public async Task GetEntriesAsync_ReturnsEmptyPageBeyondTheLastOne()
    {
        await using var db = TestDb.Create();
        var service = new IntakeService(db);
        await SeedEntryAsync(db, "user1", "Oatmeal", DateTime.UtcNow);

        var result = await service.GetEntriesAsync("user1", new PaginationRequest(5, 20));

        await Assert.That(result.Items).IsEmpty();
        await Assert.That(result.TotalCount).IsEqualTo(1);
    }
}
