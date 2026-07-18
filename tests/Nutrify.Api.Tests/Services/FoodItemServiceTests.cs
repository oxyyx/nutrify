using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Nutrify.Api.Data;
using Nutrify.Api.Entities;
using Nutrify.Api.Services;
using Nutrify.Contracts.Common;
using Nutrify.Contracts.FoodItems;
using TUnit.Assertions.Enums;

namespace Nutrify.Api.Tests.Services;

public class FoodItemServiceTests
{
    private static FoodItemService CreateService(
        NutrifyDbContext db, ExternalProductDto? externalProduct = null) =>
        new(db, new StubOpenFoodFactsClient(externalProduct));

    private static CreateFoodItemRequest MonsterRequest() => new(
        Name: "Monster Energy",
        Type: FoodItemType.Drink,
        CaloriesKcal: 47,
        ProteinG: 0,
        CarbohydratesG: 11,
        FatG: 0,
        FiberG: 0,
        CategoryId: null,
        Barcode: "5060337502900",
        ServingSize: 500,
        ServingSizeName: "can");

    private static UpdateFoodItemRequest UpdateRequest(
        string name = "Monster Energy",
        FoodItemType type = FoodItemType.Drink,
        int? categoryId = null,
        string? barcode = null,
        decimal? servingSize = null,
        string? servingSizeName = null) =>
        new(name, type, 47, 0, 11, 0, 0, categoryId, barcode, servingSize, servingSizeName);

    // ---- Serving size ----------------------------------------------------

    [Test]
    public async Task CreateAsync_PersistsServingSize()
    {
        await using var db = TestDb.Create();
        var service = CreateService(db);

        var dto = await service.CreateAsync("user1", MonsterRequest());

        await Assert.That(dto.ServingSize).IsEqualTo(500m);
        await Assert.That(dto.ServingSizeName).IsEqualTo("can");

        var entity = await db.FoodItems.SingleAsync();
        await Assert.That(entity.ServingSize).IsEqualTo(500m);
        await Assert.That(entity.ServingSizeName).IsEqualTo("can");
    }

    [Test]
    public async Task UpdateAsync_PersistsServingSize()
    {
        await using var db = TestDb.Create();
        var service = CreateService(db);
        var created = await service.CreateAsync(
            "user1", MonsterRequest() with { ServingSize = null, ServingSizeName = null });

        var updated = await service.UpdateAsync(created.Id, "user1",
            UpdateRequest(barcode: "5060337502900", servingSize: 500, servingSizeName: "can"));

        await Assert.That(updated!.ServingSize).IsEqualTo(500m);
        await Assert.That(updated.ServingSizeName).IsEqualTo("can");
    }

    [Test]
    public async Task CreateAsync_DropsServingName_WhenSizeIsNull()
    {
        await using var db = TestDb.Create();
        var service = CreateService(db);

        // A name without a size is meaningless, so both are cleared.
        var dto = await service.CreateAsync(
            "user1", MonsterRequest() with { ServingSize = null, ServingSizeName = "can" });

        await Assert.That(dto.ServingSize).IsNull();
        await Assert.That(dto.ServingSizeName).IsNull();
    }

    [Test]
    [Arguments("  can  ", "can")]
    [Arguments("   ", null)]
    [Arguments("", null)]
    public async Task CreateAsync_TrimsServingName_AndBlanksBecomeNull(string input, string? expected)
    {
        await using var db = TestDb.Create();
        var service = CreateService(db);

        var dto = await service.CreateAsync("user1", MonsterRequest() with { ServingSizeName = input });

        await Assert.That(dto.ServingSizeName).IsEqualTo(expected);
    }

    [Test]
    [Arguments(0)]
    [Arguments(-1)]
    public async Task CreateAsync_RejectsNonPositiveServingSize(int size)
    {
        await using var db = TestDb.Create();
        var service = CreateService(db);

        await Assert.That(async () => await service.CreateAsync(
                "user1", MonsterRequest() with { ServingSize = size }))
            .Throws<ArgumentException>();
    }

    // ---- Unit derivation -------------------------------------------------

    [Test]
    [Arguments(FoodItemType.Food, "g")]
    [Arguments(FoodItemType.Drink, "mL")]
    public async Task CreateAsync_DerivesUnitFromType(FoodItemType type, string expectedUnit)
    {
        await using var db = TestDb.Create();
        var service = CreateService(db);

        var dto = await service.CreateAsync(
            "user1", MonsterRequest() with { Type = type, Barcode = null });

        await Assert.That(dto.Unit).IsEqualTo(expectedUnit);
    }

    [Test]
    public async Task UpdateAsync_RederivesUnit_WhenTypeChanges()
    {
        await using var db = TestDb.Create();
        var service = CreateService(db);
        var created = await service.CreateAsync(
            "user1", MonsterRequest() with { Type = FoodItemType.Drink, Barcode = null });
        await Assert.That(created.Unit).IsEqualTo("mL");

        var updated = await service.UpdateAsync(created.Id, "user1", UpdateRequest(type: FoodItemType.Food));

        await Assert.That(updated!.Unit).IsEqualTo("g");
    }

    // ---- Barcodes --------------------------------------------------------

    [Test]
    [Arguments(null)]
    [Arguments("")]
    [Arguments("   ")]
    public async Task CreateAsync_TreatsBlankBarcodeAsNull(string? barcode)
    {
        await using var db = TestDb.Create();
        var service = CreateService(db);

        var dto = await service.CreateAsync("user1", MonsterRequest() with { Barcode = barcode });

        await Assert.That(dto.Barcode).IsNull();
    }

    [Test]
    [Arguments("12345")]                              // too short (min 6)
    [Arguments("123456789012345678901234567890123")]  // too long (max 32)
    [Arguments("50603375029AB")]                      // non-digits
    [Arguments("5060 33750290")]                      // embedded space
    public async Task CreateAsync_RejectsMalformedBarcode(string barcode)
    {
        await using var db = TestDb.Create();
        var service = CreateService(db);

        await Assert.That(async () => await service.CreateAsync(
                "user1", MonsterRequest() with { Barcode = barcode }))
            .Throws<ArgumentException>();
    }

    [Test]
    public async Task CreateAsync_TrimsSurroundingWhitespaceFromBarcode()
    {
        await using var db = TestDb.Create();
        var service = CreateService(db);

        var dto = await service.CreateAsync("user1", MonsterRequest() with { Barcode = "  5060337502900  " });

        await Assert.That(dto.Barcode).IsEqualTo("5060337502900");
    }

    [Test]
    public async Task CreateAsync_RejectsBarcodeAlreadyUsedByTheSameUser()
    {
        await using var db = TestDb.Create();
        var service = CreateService(db);
        await service.CreateAsync("user1", MonsterRequest());

        await Assert.That(async () => await service.CreateAsync(
                "user1", MonsterRequest() with { Name = "Monster Ultra" }))
            .Throws<InvalidOperationException>();
    }

    [Test]
    public async Task CreateAsync_AllowsSameBarcodeForDifferentUsers()
    {
        await using var db = TestDb.Create();
        var service = CreateService(db);
        await service.CreateAsync("user1", MonsterRequest());

        // Barcode uniqueness is scoped per user, not global.
        var other = await service.CreateAsync("user2", MonsterRequest());

        await Assert.That(other.Barcode).IsEqualTo("5060337502900");
    }

    [Test]
    public async Task UpdateAsync_AllowsKeepingItsOwnBarcode()
    {
        await using var db = TestDb.Create();
        var service = CreateService(db);
        var created = await service.CreateAsync("user1", MonsterRequest());

        // The uniqueness check must exclude the row being updated.
        var updated = await service.UpdateAsync(created.Id, "user1",
            UpdateRequest(name: "Monster Energy Zero", barcode: "5060337502900"));

        await Assert.That(updated!.Barcode).IsEqualTo("5060337502900");
        await Assert.That(updated.Name).IsEqualTo("Monster Energy Zero");
    }

    [Test]
    public async Task UpdateAsync_RejectsBarcodeOwnedByAnotherOfTheUsersItems()
    {
        await using var db = TestDb.Create();
        var service = CreateService(db);
        await service.CreateAsync("user1", MonsterRequest());
        var second = await service.CreateAsync(
            "user1", MonsterRequest() with { Name = "Red Bull", Barcode = "9002490100070" });

        await Assert.That(async () => await service.UpdateAsync(second.Id, "user1",
                UpdateRequest(name: "Red Bull", barcode: "5060337502900")))
            .Throws<InvalidOperationException>();
    }

    // ---- Category ownership ---------------------------------------------

    [Test]
    public async Task CreateAsync_RejectsCategoryOwnedByAnotherUser()
    {
        await using var db = TestDb.Create();
        var service = CreateService(db);
        var foreign = new Category { Name = "Snacks", UserId = "user2" };
        db.Categories.Add(foreign);
        await db.SaveChangesAsync();

        // Another user's category must look exactly as missing as a bogus id.
        await Assert.That(async () => await service.CreateAsync(
                "user1", MonsterRequest() with { CategoryId = foreign.Id, Barcode = null }))
            .Throws<KeyNotFoundException>();
    }

    [Test]
    public async Task CreateAsync_RejectsUnknownCategory()
    {
        await using var db = TestDb.Create();
        var service = CreateService(db);

        await Assert.That(async () => await service.CreateAsync(
                "user1", MonsterRequest() with { CategoryId = 4321, Barcode = null }))
            .Throws<KeyNotFoundException>();
    }

    [Test]
    public async Task CreateAsync_PopulatesCategoryNameOnTheDto()
    {
        await using var db = TestDb.Create();
        var service = CreateService(db);
        var category = new Category { Name = "Drinks", UserId = "user1" };
        db.Categories.Add(category);
        await db.SaveChangesAsync();

        var dto = await service.CreateAsync(
            "user1", MonsterRequest() with { CategoryId = category.Id });

        await Assert.That(dto.CategoryId).IsEqualTo(category.Id);
        await Assert.That(dto.CategoryName).IsEqualTo("Drinks");
    }

    // ---- User isolation --------------------------------------------------

    [Test]
    public async Task GetByIdAsync_DoesNotReturnAnotherUsersItem()
    {
        await using var db = TestDb.Create();
        var service = CreateService(db);
        var created = await service.CreateAsync("user1", MonsterRequest());

        await Assert.That(await service.GetByIdAsync(created.Id, "user2")).IsNull();
        await Assert.That(await service.GetByIdAsync(created.Id, "user1")).IsNotNull();
    }

    [Test]
    public async Task UpdateAsync_ReturnsNullForAnotherUsersItem()
    {
        await using var db = TestDb.Create();
        var service = CreateService(db);
        var created = await service.CreateAsync("user1", MonsterRequest());

        var updated = await service.UpdateAsync(created.Id, "user2", UpdateRequest(name: "Hijacked"));

        await Assert.That(updated).IsNull();
        var untouched = await db.FoodItems.SingleAsync();
        await Assert.That(untouched.Name).IsEqualTo("Monster Energy");
    }

    [Test]
    public async Task DeleteAsync_ReturnsFalseForAnotherUsersItem()
    {
        await using var db = TestDb.Create();
        var service = CreateService(db);
        var created = await service.CreateAsync("user1", MonsterRequest());

        await Assert.That(await service.DeleteAsync(created.Id, "user2")).IsFalse();
        await Assert.That(await db.FoodItems.CountAsync()).IsEqualTo(1);
    }

    [Test]
    public async Task DeleteAsync_ReturnsFalseForUnknownId()
    {
        await using var db = TestDb.Create();
        var service = CreateService(db);

        await Assert.That(await service.DeleteAsync(9999, "user1")).IsFalse();
    }

    // ---- Listing, search, filters, paging --------------------------------

    [Test]
    [Arguments("mon")]
    [Arguments("MONSTER")]
    [Arguments("Energy")]
    public async Task GetAllAsync_SearchesNameCaseInsensitively(string search)
    {
        await using var db = TestDb.Create();
        var service = CreateService(db);
        db.FoodItems.AddRange(
            TestDb.NewFood("user1", "Monster Energy"),
            TestDb.NewFood("user1", "Chicken Breast"));
        await db.SaveChangesAsync();

        var result = await service.GetAllAsync("user1", new PaginationRequest(1, 20), search: search);

        await Assert.That(result.TotalCount).IsEqualTo(1);
        await Assert.That(result.Items.Single().Name).IsEqualTo("Monster Energy");
    }

    [Test]
    public async Task GetAllAsync_FiltersByType()
    {
        await using var db = TestDb.Create();
        var service = CreateService(db);
        db.FoodItems.AddRange(
            TestDb.NewFood("user1", "Monster Energy", FoodItemType.Drink, "mL"),
            TestDb.NewFood("user1", "Chicken Breast"));
        await db.SaveChangesAsync();

        var result = await service.GetAllAsync(
            "user1", new PaginationRequest(1, 20), type: FoodItemType.Drink);

        await Assert.That(result.TotalCount).IsEqualTo(1);
        await Assert.That(result.Items.Single().Name).IsEqualTo("Monster Energy");
    }

    [Test]
    public async Task GetAllAsync_FiltersByCategory()
    {
        await using var db = TestDb.Create();
        var service = CreateService(db);
        var category = new Category { Name = "Drinks", UserId = "user1" };
        db.Categories.Add(category);
        await db.SaveChangesAsync();
        db.FoodItems.AddRange(
            TestDb.NewFood("user1", "Monster Energy", categoryId: category.Id),
            TestDb.NewFood("user1", "Chicken Breast"));
        await db.SaveChangesAsync();

        var result = await service.GetAllAsync(
            "user1", new PaginationRequest(1, 20), categoryId: category.Id);

        await Assert.That(result.TotalCount).IsEqualTo(1);
        await Assert.That(result.Items.Single().Name).IsEqualTo("Monster Energy");
    }

    [Test]
    public async Task GetAllAsync_DoesNotLeakOtherUsersItems()
    {
        await using var db = TestDb.Create();
        var service = CreateService(db);
        db.FoodItems.AddRange(
            TestDb.NewFood("user1", "Oatmeal"),
            TestDb.NewFood("user2", "Oatmeal"));
        await db.SaveChangesAsync();

        var result = await service.GetAllAsync("user1", new PaginationRequest(1, 20));

        await Assert.That(result.TotalCount).IsEqualTo(1);
    }

    [Test]
    public async Task GetAllAsync_OrdersByName()
    {
        await using var db = TestDb.Create();
        var service = CreateService(db);
        db.FoodItems.AddRange(
            TestDb.NewFood("user1", "Zucchini"),
            TestDb.NewFood("user1", "Apple"),
            TestDb.NewFood("user1", "Mango"));
        await db.SaveChangesAsync();

        var result = await service.GetAllAsync("user1", new PaginationRequest(1, 20));

        await Assert.That(result.Items.Select(i => i.Name).ToList())
            .IsEquivalentTo(new List<string> { "Apple", "Mango", "Zucchini" }, CollectionOrdering.Matching);
    }

    [Test]
    public async Task GetAllAsync_PagesResults()
    {
        await using var db = TestDb.Create();
        var service = CreateService(db);
        db.FoodItems.AddRange(Enumerable.Range(1, 5).Select(i => TestDb.NewFood("user1", $"Food {i}")));
        await db.SaveChangesAsync();

        var page2 = await service.GetAllAsync("user1", new PaginationRequest(2, 2));

        await Assert.That(page2.TotalCount).IsEqualTo(5);
        await Assert.That(page2.TotalPages).IsEqualTo(3);
        await Assert.That(page2.Items).Count().IsEqualTo(2);
        await Assert.That(page2.Items.Select(i => i.Name).ToList())
            .IsEquivalentTo(new List<string> { "Food 3", "Food 4" }, CollectionOrdering.Matching);
    }

    [Test]
    [Arguments(0, 1)]
    [Arguments(-5, 1)]
    public async Task GetAllAsync_ClampsPageToAtLeastOne(int requestedPage, int expectedPage)
    {
        await using var db = TestDb.Create();
        var service = CreateService(db);
        db.FoodItems.Add(TestDb.NewFood("user1", "Apple"));
        await db.SaveChangesAsync();

        var result = await service.GetAllAsync("user1", new PaginationRequest(requestedPage, 20));

        await Assert.That(result.Page).IsEqualTo(expectedPage);
        await Assert.That(result.Items).Count().IsEqualTo(1);
    }

    [Test]
    [Arguments(0, 1)]
    [Arguments(500, 100)]
    public async Task GetAllAsync_ClampsPageSizeIntoRange(int requestedSize, int expectedSize)
    {
        await using var db = TestDb.Create();
        var service = CreateService(db);
        db.FoodItems.Add(TestDb.NewFood("user1", "Apple"));
        await db.SaveChangesAsync();

        var result = await service.GetAllAsync("user1", new PaginationRequest(1, requestedSize));

        await Assert.That(result.PageSize).IsEqualTo(expectedSize);
    }

    // ---- Barcode lookup --------------------------------------------------

    [Test]
    public async Task LookupByBarcodeAsync_PrefersTheUsersExistingItem()
    {
        await using var db = TestDb.Create();
        var external = new ExternalProductDto(
            "5060337502900", "Some Other Product", null, FoodItemType.Drink, 1, 1, 1, 1, 1, 250);
        var service = CreateService(db, external);
        await service.CreateAsync("user1", MonsterRequest());

        var result = await service.LookupByBarcodeAsync("5060337502900", "user1");

        await Assert.That(result!.Source).IsEqualTo(BarcodeLookupSource.Internal);
        await Assert.That(result.ExistingItem!.Name).IsEqualTo("Monster Energy");
        await Assert.That(result.ExternalProduct).IsNull();
    }

    [Test]
    public async Task LookupByBarcodeAsync_IgnoresAnotherUsersItemAndFallsBackToExternal()
    {
        await using var db = TestDb.Create();
        var external = new ExternalProductDto(
            "5060337502900", "Monster Energy", "Monster", FoodItemType.Drink, 47, 0, 11, 0, 0, 500);
        var service = CreateService(db, external);
        await service.CreateAsync("user2", MonsterRequest());

        var result = await service.LookupByBarcodeAsync("5060337502900", "user1");

        await Assert.That(result!.Source).IsEqualTo(BarcodeLookupSource.External);
        await Assert.That(result.ExistingItem).IsNull();
        await Assert.That(result.ExternalProduct!.Name).IsEqualTo("Monster Energy");
    }

    [Test]
    public async Task LookupByBarcodeAsync_ReturnsNullWhenNeitherSourceHasIt()
    {
        await using var db = TestDb.Create();
        var service = CreateService(db);

        await Assert.That(await service.LookupByBarcodeAsync("5060337502900", "user1")).IsNull();
    }

    [Test]
    [Arguments("12345")]
    [Arguments("abcdefgh")]
    [Arguments("")]
    public async Task LookupByBarcodeAsync_RejectsMalformedBarcode(string barcode)
    {
        await using var db = TestDb.Create();
        var service = CreateService(db);

        await Assert.That(async () => await service.LookupByBarcodeAsync(barcode, "user1"))
            .Throws<ArgumentException>();
    }

    // ---- History preservation -------------------------------------------

    [Test]
    public async Task DeleteAsync_SucceedsAndPreservesIntakeHistory()
    {
        await using var db = TestDb.Create();
        var service = CreateService(db);
        var created = await service.CreateAsync("user1", MonsterRequest());

        db.IntakeEntries.Add(new IntakeEntry
        {
            UserId = "user1",
            FoodItemId = created.Id,
            Amount = 500,
            ConsumedAt = DateTime.UtcNow,
            FoodItemName = created.Name,
            FoodItemUnit = created.Unit,
            CaloriesKcal = created.CaloriesKcal
        });
        await db.SaveChangesAsync();

        var deleted = await service.DeleteAsync(created.Id, "user1");

        await Assert.That(deleted).IsTrue();
        await Assert.That(await db.FoodItems.AnyAsync(f => f.Id == created.Id)).IsFalse();

        // The logged entry outlives the food item, keeping its snapshot intact.
        var entry = await db.IntakeEntries.SingleAsync();
        await Assert.That(entry.FoodItemName).IsEqualTo("Monster Energy");
        await Assert.That(entry.CaloriesKcal).IsEqualTo(47m);
    }

    // ---- Contract binding ------------------------------------------------

    [Test]
    public async Task CreateFoodItemRequest_BindsServingSizeFromCamelCaseJson()
    {
        // Same serializer settings minimal APIs use for request bodies.
        const string json = """
            {
              "name": "Monster Energy",
              "type": 1,
              "caloriesKcal": 47,
              "proteinG": 0,
              "carbohydratesG": 11,
              "fatG": 0,
              "fiberG": 0,
              "categoryId": null,
              "barcode": "5060337502900",
              "servingSize": 500,
              "servingSizeName": "can"
            }
            """;

        var request = JsonSerializer.Deserialize<CreateFoodItemRequest>(
            json, new JsonSerializerOptions(JsonSerializerDefaults.Web));

        await Assert.That(request!.ServingSize).IsEqualTo(500m);
        await Assert.That(request.ServingSizeName).IsEqualTo("can");
    }

    private sealed class StubOpenFoodFactsClient(ExternalProductDto? product) : IOpenFoodFactsClient
    {
        public Task<ExternalProductDto?> GetProductAsync(
            string barcode, CancellationToken cancellationToken = default) =>
            Task.FromResult(product);
    }
}
