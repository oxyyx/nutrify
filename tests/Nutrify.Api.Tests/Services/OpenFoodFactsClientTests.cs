using System.Net;
using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using Nutrify.Api.Services;
using Nutrify.Contracts.FoodItems;
using TUnit.Assertions.Enums;

namespace Nutrify.Api.Tests.Services;

public class OpenFoodFactsClientTests
{
    private static OpenFoodFactsClient CreateClient(HttpStatusCode statusCode, string json)
    {
        var handler = new StubHttpMessageHandler(statusCode, json);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://world.openfoodfacts.org/") };
        return new OpenFoodFactsClient(httpClient, NullLogger<OpenFoodFactsClient>.Instance);
    }

    [Test]
    public async Task GetProductAsync_ParsesFoundProduct()
    {
        const string json = """
            {
              "code": "5000112637922",
              "status": 1,
              "product": {
                "product_name": "Coca-Cola",
                "brands": "Coca-Cola",
                "quantity": "330 ml",
                "product_quantity_unit": "ml",
                "serving_quantity": "330",
                "product_quantity": 330,
                "nutriments": {
                  "energy-kcal_100g": 42.1,
                  "proteins_100g": 0,
                  "carbohydrates_100g": 10.6,
                  "fat_100g": 0,
                  "sugars_100g": 10.6
                }
              }
            }
            """;

        var product = await CreateClient(HttpStatusCode.OK, json).GetProductAsync("5000112637922");

        await Assert.That(product).IsNotNull();
        await Assert.That(product!.Name).IsEqualTo("Coca-Cola");
        await Assert.That(product.Brand).IsEqualTo("Coca-Cola");
        await Assert.That(product.SuggestedType).IsEqualTo(FoodItemType.Drink);
        await Assert.That(product.CaloriesKcal).IsEqualTo(42.1m);
        await Assert.That(product.ProteinG).IsEqualTo(0m);
        await Assert.That(product.CarbohydratesG).IsEqualTo(10.6m);
        await Assert.That(product.FatG).IsEqualTo(0m);
        await Assert.That(product.FiberG).IsNull();
        await Assert.That(product.ServingSize).IsEqualTo(330m);
    }

    [Test]
    public async Task GetProductAsync_FallsBackToPackageQuantityForServingSize()
    {
        const string json = """
            {
              "status": 1,
              "product": {
                "product_name": "Energy Drink",
                "product_quantity": "500",
                "nutriments": { "energy-kcal_100g": 47 }
              }
            }
            """;

        var product = await CreateClient(HttpStatusCode.OK, json).GetProductAsync("123456789");

        await Assert.That(product!.ServingSize).IsEqualTo(500m);
    }

    [Test]
    public async Task GetProductAsync_FallsBackToKilojoules()
    {
        const string json = """
            {
              "status": 1,
              "product": {
                "product_name": "Bread",
                "nutriments": { "energy_100g": 1046 }
              }
            }
            """;

        var product = await CreateClient(HttpStatusCode.OK, json).GetProductAsync("123456789");

        await Assert.That(product).IsNotNull();
        await Assert.That(product!.SuggestedType).IsEqualTo(FoodItemType.Food);
        await Assert.That(product.CaloriesKcal).IsEqualTo(Math.Round(1046m / 4.184m, 2));
    }

    [Test]
    public async Task GetProductAsync_ParsesStringNutrimentValues()
    {
        const string json = """
            {
              "status": 1,
              "product": {
                "product_name": "Cheese",
                "nutriments": { "energy-kcal_100g": "402.5", "fat_100g": "33" }
              }
            }
            """;

        var product = await CreateClient(HttpStatusCode.OK, json).GetProductAsync("123456789");

        await Assert.That(product!.CaloriesKcal).IsEqualTo(402.5m);
        await Assert.That(product.FatG).IsEqualTo(33m);
    }

    [Test]
    public async Task GetProductAsync_RoundsNutrimentsToTwoDecimalPlaces()
    {
        const string json = """
            {
              "status": 1,
              "product": {
                "product_name": "Precise Snack",
                "nutriments": {
                  "energy-kcal_100g": 42.567,
                  "proteins_100g": 1.006,
                  "carbohydrates_100g": 10.601,
                  "fat_100g": 0.1249,
                  "fiber_100g": 2.5
                }
              }
            }
            """;

        var product = await CreateClient(HttpStatusCode.OK, json).GetProductAsync("123456789");

        await Assert.That(product!.CaloriesKcal).IsEqualTo(42.57m);
        await Assert.That(product.ProteinG).IsEqualTo(1.01m);
        await Assert.That(product.CarbohydratesG).IsEqualTo(10.6m);
        await Assert.That(product.FatG).IsEqualTo(0.12m);
        await Assert.That(product.FiberG).IsEqualTo(2.5m);
    }

    [Test]
    public async Task GetProductAsync_ReturnsNullWhenStatusZero()
    {
        const string json = """{ "code": "00000000", "status": 0, "status_verbose": "no code or invalid code" }""";

        var product = await CreateClient(HttpStatusCode.OK, json).GetProductAsync("00000000");

        await Assert.That(product).IsNull();
    }

    [Test]
    public async Task GetProductAsync_ReturnsNullOnNotFound()
    {
        var product = await CreateClient(HttpStatusCode.NotFound, "").GetProductAsync("999999999999");

        await Assert.That(product).IsNull();
    }

    [Test]
    public async Task GetProductAsync_ReturnsNullOnServerError()
    {
        var product = await CreateClient(HttpStatusCode.InternalServerError, "oops").GetProductAsync("123456789");

        await Assert.That(product).IsNull();
    }

    [Test]
    public async Task GetProductAsync_ReturnsNullOnMalformedJson()
    {
        // A truncated or non-JSON body must degrade to "not found", not throw.
        var product = await CreateClient(HttpStatusCode.OK, "{ not json").GetProductAsync("123456789");

        await Assert.That(product).IsNull();
    }

    [Test]
    public async Task GetProductAsync_ReturnsNullWhenProductObjectIsMissing()
    {
        var product = await CreateClient(HttpStatusCode.OK, """{ "status": 1 }""").GetProductAsync("123456789");

        await Assert.That(product).IsNull();
    }

    [Test]
    public async Task SearchProductsAsync_ParsesResults()
    {
        const string json = """
            {
              "count": 2,
              "products": [
                {
                  "code": "5060337502900",
                  "product_name": "Monster Energy",
                  "brands": "Monster",
                  "quantity": "500 ml",
                  "product_quantity_unit": "ml",
                  "serving_quantity": 500,
                  "nutriments": { "energy-kcal_100g": 47, "carbohydrates_100g": 11 }
                },
                {
                  "code": "7622210449283",
                  "product_name": "Oreo",
                  "brands": "Mondelez",
                  "nutriments": { "energy-kcal_100g": 480, "fat_100g": 20 }
                }
              ]
            }
            """;

        var results = await CreateClient(HttpStatusCode.OK, json).SearchProductsAsync("snack");

        await Assert.That(results.Select(p => p.Barcode))
            .IsEquivalentTo(["5060337502900", "7622210449283"], CollectionOrdering.Matching);
        await Assert.That(results[0].Name).IsEqualTo("Monster Energy");
        await Assert.That(results[0].SuggestedType).IsEqualTo(FoodItemType.Drink);
        await Assert.That(results[0].ServingSize).IsEqualTo(500m);
        await Assert.That(results[1].SuggestedType).IsEqualTo(FoodItemType.Food);
        await Assert.That(results[1].CaloriesKcal).IsEqualTo(480m);
    }

    [Test]
    public async Task SearchProductsAsync_ParsesRealWorldPayloadShape()
    {
        // Trimmed from a live cgi/search.pl response: the provider returns extra
        // fields regardless of ?fields=, and serving_quantity is often null.
        const string json = """
            {
              "count": 2, "page": 1, "page_size": 2,
              "products": [
                {
                  "brands": "Oreo",
                  "code": "7622300336738",
                  "nutrition_data": "on",
                  "nutrition_data_prepared_per": "100g",
                  "product_name": "OREO ORIGINAL",
                  "product_quantity": 154,
                  "product_quantity_unit": "g",
                  "quantity": "154g",
                  "serving_quantity": 11,
                  "nutriments": {
                    "energy-kcal_100g": 472, "proteins_100g": 5.6,
                    "carbohydrates_100g": 67, "fat_100g": 19, "fiber_100g": 2.9,
                    "sodium_modifier": "~", "nova-group_unit": ""
                  },
                  "nutriments_estimated": { "alcohol_100g": 0 }
                },
                {
                  "brands": "Oreo, Mondelez",
                  "code": "6111031005576",
                  "product_name": "original  oreo",
                  "product_quantity": 55,
                  "product_quantity_unit": "g",
                  "quantity": "55g",
                  "serving_quantity": null,
                  "nutriments": { "energy-kcal_100g": 481, "fat_100g": 20 }
                }
              ]
            }
            """;

        var results = await CreateClient(HttpStatusCode.OK, json).SearchProductsAsync("oreo");

        await Assert.That(results.Count).IsEqualTo(2);
        await Assert.That(results[0].Name).IsEqualTo("OREO ORIGINAL");
        await Assert.That(results[0].Brand).IsEqualTo("Oreo");
        await Assert.That(results[0].SuggestedType).IsEqualTo(FoodItemType.Food);
        await Assert.That(results[0].CaloriesKcal).IsEqualTo(472m);
        await Assert.That(results[0].FiberG).IsEqualTo(2.9m);
        await Assert.That(results[0].ServingSize).IsEqualTo(11m);
        // Null serving_quantity falls back to the package quantity.
        await Assert.That(results[1].ServingSize).IsEqualTo(55m);
    }

    [Test]
    public async Task SearchProductsAsync_SkipsProductsWithoutCodeOrName()
    {
        // OFF returns partial records for products nobody has filled in yet;
        // without a code or name they can't prefill the new-food form.
        const string json = """
            {
              "products": [
                { "code": "111111", "product_name": "" },
                { "product_name": "No Barcode" },
                { "code": "222222", "product_name": "Usable" }
              ]
            }
            """;

        var results = await CreateClient(HttpStatusCode.OK, json).SearchProductsAsync("thing");

        await Assert.That(results.Select(p => p.Name!)).IsEquivalentTo(["Usable"]);
    }

    [Test]
    [Arguments("")]
    [Arguments("   ")]
    public async Task SearchProductsAsync_ReturnsEmptyForBlankQueryWithoutCallingProvider(string query)
    {
        var results = await CreateClient(HttpStatusCode.InternalServerError, "oops").SearchProductsAsync(query);

        await Assert.That(results).IsEmpty();
    }

    [Test]
    public async Task SearchProductsAsync_ReturnsEmptyOnServerError()
    {
        var results = await CreateClient(HttpStatusCode.InternalServerError, "oops").SearchProductsAsync("snack");

        await Assert.That(results).IsEmpty();
    }

    [Test]
    public async Task SearchProductsAsync_ReturnsEmptyOnMalformedJson()
    {
        var results = await CreateClient(HttpStatusCode.OK, "{ not json").SearchProductsAsync("snack");

        await Assert.That(results).IsEmpty();
    }

    [Test]
    public async Task SearchProductsAsync_ReturnsEmptyWhenProductsArrayIsMissing()
    {
        var results = await CreateClient(HttpStatusCode.OK, """{ "count": 0 }""").SearchProductsAsync("snack");

        await Assert.That(results).IsEmpty();
    }

    private sealed class StubHttpMessageHandler(HttpStatusCode statusCode, string json) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });
        }
    }
}
