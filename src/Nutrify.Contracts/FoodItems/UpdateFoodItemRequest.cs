namespace Nutrify.Contracts.FoodItems;

public record UpdateFoodItemRequest(
    string Name,
    FoodItemType Type,
    decimal CaloriesKcal,
    decimal ProteinG,
    decimal CarbohydratesG,
    decimal FatG,
    decimal FiberG,
    int? CategoryId,
    string? Barcode = null
);
