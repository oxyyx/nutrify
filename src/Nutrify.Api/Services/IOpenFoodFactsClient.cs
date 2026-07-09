using Nutrify.Contracts.FoodItems;

namespace Nutrify.Api.Services;

public interface IOpenFoodFactsClient
{
    /// <summary>Looks up a product by barcode; returns null when not found or the provider is unavailable.</summary>
    Task<ExternalProductDto?> GetProductAsync(string barcode, CancellationToken cancellationToken = default);
}
