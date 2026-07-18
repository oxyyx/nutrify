using System.Net;
using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using Nutrify.Api.Services;
using Nutrify.Contracts.FoodItems;

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
