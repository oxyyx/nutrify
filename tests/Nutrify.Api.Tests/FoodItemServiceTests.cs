using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Nutrify.Api.Data;
using Nutrify.Api.Services;
using Nutrify.Contracts.FoodItems;

namespace Nutrify.Api.Tests;

public class FoodItemServiceTests
{
    private static NutrifyDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<NutrifyDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new NutrifyDbContext(options);
    }

    private static FoodItemService CreateService(NutrifyDbContext db) =>
        new(db, new StubOpenFoodFactsClient());

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

    [Fact]
    public async Task CreateAsync_PersistsServingSize()
    {
        await using var db = CreateDb();
        var service = CreateService(db);

        var dto = await service.CreateAsync("user1", MonsterRequest());

        dto.ServingSize.Should().Be(500m);
        dto.ServingSizeName.Should().Be("can");

        var entity = await db.FoodItems.SingleAsync();
        entity.ServingSize.Should().Be(500m);
        entity.ServingSizeName.Should().Be("can");
    }

    [Fact]
    public async Task UpdateAsync_PersistsServingSize()
    {
        await using var db = CreateDb();
        var service = CreateService(db);
        var created = await service.CreateAsync("user1", MonsterRequest() with { ServingSize = null, ServingSizeName = null });

        var updated = await service.UpdateAsync(created.Id, "user1", new UpdateFoodItemRequest(
            "Monster Energy", FoodItemType.Drink, 47, 0, 11, 0, 0, null, "5060337502900", 500, "can"));

        updated!.ServingSize.Should().Be(500m);
        updated.ServingSizeName.Should().Be("can");
    }

    [Fact]
    public void CreateFoodItemRequest_BindsServingSizeFromCamelCaseJson()
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

        request!.ServingSize.Should().Be(500m);
        request.ServingSizeName.Should().Be("can");
    }

    [Fact]
    public async Task DeleteAsync_SucceedsAndPreservesIntakeHistory()
    {
        await using var db = CreateDb();
        var service = CreateService(db);
        var created = await service.CreateAsync("user1", MonsterRequest());

        db.IntakeEntries.Add(new Entities.IntakeEntry
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

        deleted.Should().BeTrue();
        (await db.FoodItems.AnyAsync(f => f.Id == created.Id)).Should().BeFalse();

        // The logged entry outlives the food item, keeping its snapshot intact.
        var entry = await db.IntakeEntries.SingleAsync();
        entry.FoodItemName.Should().Be("Monster Energy");
        entry.CaloriesKcal.Should().Be(47);
    }

    private sealed class StubOpenFoodFactsClient : IOpenFoodFactsClient
    {
        public Task<ExternalProductDto?> GetProductAsync(string barcode, CancellationToken cancellationToken = default) =>
            Task.FromResult<ExternalProductDto?>(null);
    }
}
