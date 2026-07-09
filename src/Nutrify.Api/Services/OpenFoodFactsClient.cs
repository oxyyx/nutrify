using System.Text.Json;
using System.Text.Json.Serialization;
using Nutrify.Contracts.FoodItems;

namespace Nutrify.Api.Services;

public class OpenFoodFactsClient(HttpClient httpClient, ILogger<OpenFoodFactsClient> logger) : IOpenFoodFactsClient
{
    private static readonly string[] DrinkQuantityUnits = ["ml", "cl", "dl", "l"];

    public async Task<ExternalProductDto?> GetProductAsync(string barcode, CancellationToken cancellationToken = default)
    {
        const string fields = "product_name,brands,nutriments,product_quantity_unit,quantity";

        try
        {
            using var response = await httpClient.GetAsync(
                $"api/v2/product/{Uri.EscapeDataString(barcode)}?fields={fields}",
                cancellationToken);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<OffProductResponse>(cancellationToken);
            if (result is not { Status: 1, Product: { } product })
                return null;

            return new ExternalProductDto(
                barcode,
                string.IsNullOrWhiteSpace(product.ProductName) ? null : product.ProductName.Trim(),
                string.IsNullOrWhiteSpace(product.Brands) ? null : product.Brands.Trim(),
                IsDrink(product) ? FoodItemType.Drink : FoodItemType.Food,
                GetCaloriesKcal(product.Nutriments),
                GetNutriment(product.Nutriments, "proteins_100g"),
                GetNutriment(product.Nutriments, "carbohydrates_100g"),
                GetNutriment(product.Nutriments, "fat_100g"),
                GetNutriment(product.Nutriments, "fiber_100g")
            );
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            // The external provider is best-effort: a failed lookup should surface
            // as "not found" so the user can still create the item manually.
            logger.LogWarning(ex, "Open Food Facts lookup failed for barcode {Barcode}", barcode);
            return null;
        }
    }

    private static bool IsDrink(OffProduct product)
    {
        var unit = product.ProductQuantityUnit?.Trim().ToLowerInvariant();
        if (unit is not null && DrinkQuantityUnits.Contains(unit))
            return true;

        // Fall back to the free-text quantity, e.g. "330 ml" or "1.5 L".
        var quantity = product.Quantity?.Trim().ToLowerInvariant();
        return quantity is not null && DrinkQuantityUnits.Any(u =>
            quantity.EndsWith(u, StringComparison.Ordinal) &&
            (quantity.Length == u.Length || !char.IsLetter(quantity[^(u.Length + 1)])));
    }

    private static decimal? GetCaloriesKcal(Dictionary<string, JsonElement>? nutriments)
    {
        var kcal = GetNutriment(nutriments, "energy-kcal_100g");
        if (kcal is not null)
            return kcal;

        // Some products only report energy in kJ.
        var kj = GetNutriment(nutriments, "energy-kj_100g") ?? GetNutriment(nutriments, "energy_100g");
        return kj is null ? null : Math.Round(kj.Value / 4.184m, 2);
    }

    // Nutriment values are usually numbers but occasionally numeric strings.
    private static decimal? GetNutriment(Dictionary<string, JsonElement>? nutriments, string key)
    {
        if (nutriments is null || !nutriments.TryGetValue(key, out var element))
            return null;

        return element.ValueKind switch
        {
            JsonValueKind.Number when element.TryGetDecimal(out var value) => value,
            JsonValueKind.String when decimal.TryParse(
                element.GetString(),
                System.Globalization.NumberStyles.Number,
                System.Globalization.CultureInfo.InvariantCulture,
                out var value) => value,
            _ => null
        };
    }

    private sealed record OffProductResponse(
        [property: JsonPropertyName("status")] int Status,
        [property: JsonPropertyName("product")] OffProduct? Product
    );

    private sealed record OffProduct(
        [property: JsonPropertyName("product_name")] string? ProductName,
        [property: JsonPropertyName("brands")] string? Brands,
        [property: JsonPropertyName("product_quantity_unit")] string? ProductQuantityUnit,
        [property: JsonPropertyName("quantity")] string? Quantity,
        [property: JsonPropertyName("nutriments")] Dictionary<string, JsonElement>? Nutriments
    );
}
