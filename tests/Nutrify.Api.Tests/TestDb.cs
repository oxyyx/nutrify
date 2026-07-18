using Microsoft.EntityFrameworkCore;
using Nutrify.Api.Data;
using Nutrify.Api.Entities;
using Nutrify.Contracts.FoodItems;

namespace Nutrify.Api.Tests;

internal static class TestDb
{
    /// <summary>
    /// A fresh, uniquely-named InMemory database per call. TUnit runs tests in
    /// parallel, so nothing may be shared between them.
    /// </summary>
    public static NutrifyDbContext Create() =>
        new(new DbContextOptionsBuilder<NutrifyDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    public static FoodItem NewFood(
        string userId,
        string name = "Oatmeal",
        FoodItemType type = FoodItemType.Food,
        string unit = "g",
        decimal calories = 0,
        decimal protein = 0,
        decimal carbs = 0,
        decimal fat = 0,
        decimal fiber = 0,
        string? barcode = null,
        int? categoryId = null) => new()
        {
            Name = name,
            UserId = userId,
            Type = type,
            Unit = unit,
            CaloriesKcal = calories,
            ProteinG = protein,
            CarbohydratesG = carbs,
            FatG = fat,
            FiberG = fiber,
            Barcode = barcode,
            CategoryId = categoryId
        };

    /// <summary>
    /// An entry with its snapshot fields already filled in, as
    /// <see cref="Nutrify.Api.Services.IntakeService"/> would have written them.
    /// </summary>
    public static IntakeEntry NewEntry(
        string userId,
        string foodName = "Oatmeal",
        DateTime? consumedAt = null,
        decimal amount = 100,
        string unit = "g",
        decimal calories = 0,
        decimal protein = 0,
        decimal carbs = 0,
        decimal fat = 0,
        decimal fiber = 0,
        FoodItem? foodItem = null) => new()
        {
            UserId = userId,
            FoodItem = foodItem,
            Amount = amount,
            ConsumedAt = consumedAt ?? DateTime.UtcNow,
            FoodItemName = foodName,
            FoodItemUnit = unit,
            CaloriesKcal = calories,
            ProteinG = protein,
            CarbohydratesG = carbs,
            FatG = fat,
            FiberG = fiber
        };
}
