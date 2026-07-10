namespace Nutrify.Contracts.FoodItems;

/// <summary>
/// Product data from an external provider, used to prefill a new food item.
/// Nutritional values are per 100g/100mL; null when the provider has no data.
/// ServingSize is one serving (or the package quantity) in g/mL.
/// </summary>
public record ExternalProductDto(
    string Barcode,
    string? Name,
    string? Brand,
    FoodItemType SuggestedType,
    decimal? CaloriesKcal,
    decimal? ProteinG,
    decimal? CarbohydratesG,
    decimal? FatG,
    decimal? FiberG,
    decimal? ServingSize
);
