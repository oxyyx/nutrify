using System.Net;
using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using Nutrify.Api.Services;
using Nutrify.Contracts.FoodItems;

namespace Nutrify.Api.Tests;

public class OpenFoodFactsClientTests
{
    private static OpenFoodFactsClient CreateClient(HttpStatusCode statusCode, string json)
    {
        var handler = new StubHttpMessageHandler(statusCode, json);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://world.openfoodfacts.org/") };
        return new OpenFoodFactsClient(httpClient, NullLogger<OpenFoodFactsClient>.Instance);
    }

    [Fact]
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

        product.Should().NotBeNull();
        product!.Name.Should().Be("Coca-Cola");
        product.Brand.Should().Be("Coca-Cola");
        product.SuggestedType.Should().Be(FoodItemType.Drink);
        product.CaloriesKcal.Should().Be(42.1m);
        product.ProteinG.Should().Be(0m);
        product.CarbohydratesG.Should().Be(10.6m);
        product.FatG.Should().Be(0m);
        product.FiberG.Should().BeNull();
        product.ServingSize.Should().Be(330m);
    }

    [Fact]
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

        product!.ServingSize.Should().Be(500m);
    }

    [Fact]
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

        product.Should().NotBeNull();
        product!.SuggestedType.Should().Be(FoodItemType.Food);
        product.CaloriesKcal.Should().Be(Math.Round(1046m / 4.184m, 2));
    }

    [Fact]
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

        product!.CaloriesKcal.Should().Be(402.5m);
        product.FatG.Should().Be(33m);
    }

    [Fact]
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

        product!.CaloriesKcal.Should().Be(42.57m);
        product.ProteinG.Should().Be(1.01m);
        product.CarbohydratesG.Should().Be(10.6m);
        product.FatG.Should().Be(0.12m);
        product.FiberG.Should().Be(2.5m);
    }

    [Fact]
    public async Task GetProductAsync_ReturnsNullWhenStatusZero()
    {
        const string json = """{ "code": "00000000", "status": 0, "status_verbose": "no code or invalid code" }""";

        var product = await CreateClient(HttpStatusCode.OK, json).GetProductAsync("00000000");

        product.Should().BeNull();
    }

    [Fact]
    public async Task GetProductAsync_ReturnsNullOnNotFound()
    {
        var product = await CreateClient(HttpStatusCode.NotFound, "").GetProductAsync("999999999999");

        product.Should().BeNull();
    }

    [Fact]
    public async Task GetProductAsync_ReturnsNullOnServerError()
    {
        var product = await CreateClient(HttpStatusCode.InternalServerError, "oops").GetProductAsync("123456789");

        product.Should().BeNull();
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
