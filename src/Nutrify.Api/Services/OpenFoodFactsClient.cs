using System.Text.Json;
using System.Text.Json.Serialization;
using Nutrify.Contracts.FoodItems;

namespace Nutrify.Api.Services;

public class OpenFoodFactsClient(HttpClient httpClient, ILogger<OpenFoodFactsClient> logger) : IOpenFoodFactsClient
{
    private static readonly string[] DrinkQuantityUnits = ["ml", "cl", "dl", "l"];

    private const string ProductFields =
        "code,product_name,brands,nutriments,product_quantity_unit,quantity,serving_quantity,product_quantity";

    public async Task<ExternalProductDto?> GetProductAsync(string barcode, CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await httpClient.GetAsync(
                $"api/v2/product/{Uri.EscapeDataString(barcode)}?fields={ProductFields}",
                cancellationToken);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<OffProductResponse>(cancellationToken);
            if (result is not { Status: 1, Product: { } product })
                return null;

            return MapProduct(product, barcode);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            // The external provider is best-effort: a failed lookup should surface
            // as "not found" so the user can still create the item manually.
            logger.LogWarning(ex, "Open Food Facts lookup failed for barcode {Barcode}", barcode);
            return null;
        }
    }

    public async Task<IReadOnlyList<ExternalProductDto>> SearchProductsAsync(
        string query,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var terms = query.Trim();
        if (terms.Length == 0)
            return [];

        var size = Math.Clamp(pageSize, 1, 50);

        try
        {
            // The v2 API has no free-text search; cgi/search.pl is the documented
            // endpoint for it (https://openfoodfacts.github.io/openfoodfacts-server/api/).
            using var response = await httpClient.GetAsync(
                $"cgi/search.pl?search_terms={Uri.EscapeDataString(terms)}&search_simple=1&action=process&json=1&page_size={size}&fields={ProductFields}",
                cancellationToken);

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<OffSearchResponse>(cancellationToken);
            if (result?.Products is null)
                return [];

            // A product with no name or no barcode can't prefill the form usefully.
            return result.Products
                .Where(p => !string.IsNullOrWhiteSpace(p.Code) && !string.IsNullOrWhiteSpace(p.ProductName))
                .Select(p => MapProduct(p, p.Code!.Trim()))
                .ToList();
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            // Best-effort, as with barcode lookup: a provider failure degrades to
            // "no external matches" rather than failing the whole search.
            logger.LogWarning(ex, "Open Food Facts search failed for query {Query}", terms);
            return [];
        }
    }

    private static ExternalProductDto MapProduct(OffProduct product, string barcode) =>
        new(
            barcode,
            string.IsNullOrWhiteSpace(product.ProductName) ? null : product.ProductName.Trim(),
            string.IsNullOrWhiteSpace(product.Brands) ? null : product.Brands.Trim(),
            IsDrink(product) ? FoodItemType.Drink : FoodItemType.Food,
            GetCaloriesKcal(product.Nutriments),
            GetNutriment(product.Nutriments, "proteins_100g"),
            GetNutriment(product.Nutriments, "carbohydrates_100g"),
            GetNutriment(product.Nutriments, "fat_100g"),
            GetNutriment(product.Nutriments, "fiber_100g"),
            GetServingSize(product)
        );

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

    // Prefer the declared serving; fall back to the package quantity (a single
    // can/bottle is usually consumed whole, so it makes a sensible serving).
    private static decimal? GetServingSize(OffProduct product)
    {
        var serving = ParseDecimal(product.ServingQuantity) ?? ParseDecimal(product.ProductQuantity);
        return serving > 0 ? serving : null;
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

    // Nutriment values are usually numbers but occasionally numeric strings,
    // and OFF reports some with far more precision than is meaningful here.
    private static decimal? GetNutriment(Dictionary<string, JsonElement>? nutriments, string key)
    {
        if (nutriments is null || !nutriments.TryGetValue(key, out var element))
            return null;

        var value = ParseDecimal(element);
        return value is null ? null : Math.Round(value.Value, 2);
    }

    private static decimal? ParseDecimal(JsonElement? element)
    {
        return element?.ValueKind switch
        {
            JsonValueKind.Number when element.Value.TryGetDecimal(out var value) => value,
            JsonValueKind.String when decimal.TryParse(
                element.Value.GetString(),
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

    private sealed record OffSearchResponse(
        [property: JsonPropertyName("products")] List<OffProduct>? Products
    );

    private sealed record OffProduct(
        [property: JsonPropertyName("code")] string? Code,
        [property: JsonPropertyName("product_name")] string? ProductName,
        [property: JsonPropertyName("brands")] string? Brands,
        [property: JsonPropertyName("product_quantity_unit")] string? ProductQuantityUnit,
        [property: JsonPropertyName("quantity")] string? Quantity,
        // Number or numeric string depending on the product
        [property: JsonPropertyName("serving_quantity")] JsonElement? ServingQuantity,
        [property: JsonPropertyName("product_quantity")] JsonElement? ProductQuantity,
        [property: JsonPropertyName("nutriments")] Dictionary<string, JsonElement>? Nutriments
    );
}
