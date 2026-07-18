using Nutrify.Api.Entities;
using Nutrify.Api.Mapping;
using Nutrify.Contracts.Intake;

namespace Nutrify.Api.Tests.Mapping;

public class MappingExtensionsTests
{
    private static IntakeEntry Entry(decimal amount) => new()
    {
        UserId = "user1",
        Amount = amount,
        ConsumedAt = DateTime.UtcNow,
        FoodItemName = "Oatmeal",
        FoodItemUnit = "g",
        CaloriesKcal = 380,
        ProteinG = 13,
        CarbohydratesG = 60,
        FatG = 7,
        FiberG = 10
    };

    [Test]
    [Arguments(100, 380)]  // exactly one 100g portion
    [Arguments(50, 190)]   // half
    [Arguments(250, 950)]  // two and a half
    [Arguments(0, 0)]      // nothing logged
    public async Task ToDto_ScalesPer100ValuesByAmount(int amount, int expectedCalories)
    {
        var dto = Entry(amount).ToDto();

        await Assert.That(dto.CaloriesKcal).IsEqualTo((decimal)expectedCalories);
        await Assert.That(dto.Amount).IsEqualTo((decimal)amount);
    }

    [Test]
    public async Task ToDto_ScalesEveryMacro()
    {
        var dto = Entry(50).ToDto();

        await Assert.That(dto.ProteinG).IsEqualTo(6.5m);
        await Assert.That(dto.CarbohydratesG).IsEqualTo(30m);
        await Assert.That(dto.FatG).IsEqualTo(3.5m);
        await Assert.That(dto.FiberG).IsEqualTo(5m);
    }

    [Test]
    public async Task ToDto_KeepsFractionalPrecision()
    {
        // 33g of 380kcal/100g is 125.4 — the mapping must not round to an int.
        var dto = Entry(33).ToDto();

        await Assert.That(dto.CaloriesKcal).IsEqualTo(125.4m);
    }

    [Test]
    public async Task ToDto_CarriesTheSnapshotIdentityNotTheLiveFoodItem()
    {
        var entry = Entry(100);
        entry.FoodItemId = null;  // source food item has since been deleted

        var dto = entry.ToDto();

        await Assert.That(dto.FoodItemId).IsNull();
        await Assert.That(dto.FoodItemName).IsEqualTo("Oatmeal");
        await Assert.That(dto.FoodItemUnit).IsEqualTo("g");
    }

    [Test]
    public async Task ToMacroSummary_SumsAcrossEntries()
    {
        var entries = new List<IntakeEntryDto>
        {
            Entry(100).ToDto(),
            Entry(50).ToDto()
        };

        var summary = entries.ToMacroSummary();

        await Assert.That(summary.TotalCalories).IsEqualTo(570m);
        await Assert.That(summary.TotalProteinG).IsEqualTo(19.5m);
        await Assert.That(summary.TotalCarbohydratesG).IsEqualTo(90m);
        await Assert.That(summary.TotalFatG).IsEqualTo(10.5m);
        await Assert.That(summary.TotalFiberG).IsEqualTo(15m);
    }

    [Test]
    public async Task ToMacroSummary_ReturnsZerosForNoEntries()
    {
        var summary = new List<IntakeEntryDto>().ToMacroSummary();

        await Assert.That(summary.TotalCalories).IsEqualTo(0m);
        await Assert.That(summary.TotalProteinG).IsEqualTo(0m);
        await Assert.That(summary.TotalCarbohydratesG).IsEqualTo(0m);
        await Assert.That(summary.TotalFatG).IsEqualTo(0m);
        await Assert.That(summary.TotalFiberG).IsEqualTo(0m);
    }

    [Test]
    public async Task CategoryToDto_ReportsFoodItemCount()
    {
        var category = new Category
        {
            Id = 1,
            Name = "Snacks",
            UserId = "user1",
            FoodItems = [TestDb.NewFood("user1", "Crisps"), TestDb.NewFood("user1", "Nuts")]
        };

        var dto = category.ToDto();

        await Assert.That(dto.Name).IsEqualTo("Snacks");
        await Assert.That(dto.FoodItemCount).IsEqualTo(2);
    }
}
