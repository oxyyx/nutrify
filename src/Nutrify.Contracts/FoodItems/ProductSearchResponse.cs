namespace Nutrify.Contracts.FoodItems;

/// <summary>
/// Result of a free-text product search. <paramref name="ExistingItems"/> are the
/// caller's own food items matching the query and are shown first; external
/// products already saved under one of those barcodes are omitted.
/// Either list may be empty.
/// </summary>
public record ProductSearchResponse(
    IReadOnlyList<FoodItemDto> ExistingItems,
    IReadOnlyList<ExternalProductDto> ExternalProducts
);
