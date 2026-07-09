namespace Nutrify.Contracts.FoodItems;

/// <summary>
/// Result of a barcode lookup. Exactly one of <paramref name="ExistingItem"/>
/// (when <paramref name="Source"/> is Internal) or <paramref name="ExternalProduct"/>
/// (when External) is set.
/// </summary>
public record BarcodeLookupResponse(
    BarcodeLookupSource Source,
    FoodItemDto? ExistingItem,
    ExternalProductDto? ExternalProduct
);
