using Nutrify.Contracts.FoodItems;

namespace Nutrify.Api.Services;

public interface IOpenFoodFactsClient
{
    /// <summary>Looks up a product by barcode; returns null when not found or the provider is unavailable.</summary>
    Task<ExternalProductDto?> GetProductAsync(string barcode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches products by free-text description; returns an empty list when
    /// nothing matches or the provider is unavailable.
    /// </summary>
    Task<IReadOnlyList<ExternalProductDto>> SearchProductsAsync(
        string query,
        int pageSize = 20,
        CancellationToken cancellationToken = default);
}
