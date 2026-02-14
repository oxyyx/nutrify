using Nutrify.Api.Entities;
using Nutrify.Contracts.Categories;
using Nutrify.Contracts.Dashboard;
using Nutrify.Contracts.FoodItems;
using Nutrify.Contracts.Intake;

namespace Nutrify.Api.Mapping;

public static class MappingExtensions
{
    public static CategoryDto ToDto(this Category category)
    {
        return new CategoryDto(
            category.Id,
            category.Name,
            category.FoodItems.Count
        );
    }

    public static FoodItemDto ToDto(this FoodItem foodItem)
    {
        return new FoodItemDto(
            foodItem.Id,
            foodItem.Name,
            foodItem.Type,
            foodItem.Unit,
            foodItem.CaloriesKcal,
            foodItem.ProteinG,
            foodItem.CarbohydratesG,
            foodItem.FatG,
            foodItem.FiberG,
            foodItem.CategoryId,
            foodItem.Category?.Name,
            foodItem.CreatedAt
        );
    }

    public static IntakeEntryDto ToDto(this IntakeEntry entry)
    {
        var multiplier = entry.Amount / 100m;
        return new IntakeEntryDto(
            entry.Id,
            entry.FoodItemId,
            entry.FoodItem.Name,
            entry.FoodItem.Unit,
            entry.Amount,
            entry.FoodItem.CaloriesKcal * multiplier,
            entry.FoodItem.ProteinG * multiplier,
            entry.FoodItem.CarbohydratesG * multiplier,
            entry.FoodItem.FatG * multiplier,
            entry.FoodItem.FiberG * multiplier,
            entry.ConsumedAt
        );
    }

    public static MacroSummaryDto ToMacroSummary(this IEnumerable<IntakeEntryDto> entries)
    {
        return new MacroSummaryDto(
            entries.Sum(e => e.CaloriesKcal),
            entries.Sum(e => e.ProteinG),
            entries.Sum(e => e.CarbohydratesG),
            entries.Sum(e => e.FatG),
            entries.Sum(e => e.FiberG)
        );
    }
}
