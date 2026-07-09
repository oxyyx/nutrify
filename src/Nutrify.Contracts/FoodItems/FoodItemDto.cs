namespace Nutrify.Contracts.FoodItems;

public record FoodItemDto(
    int Id,
    string Name,
    FoodItemType Type,
    string Unit,
    string? Barcode,
    decimal CaloriesKcal,
    decimal ProteinG,
    decimal CarbohydratesG,
    decimal FatG,
    decimal FiberG,
    int? CategoryId,
    string? CategoryName,
    DateTime CreatedAt
);
