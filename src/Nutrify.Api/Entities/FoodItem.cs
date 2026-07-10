using Nutrify.Contracts.FoodItems;

namespace Nutrify.Api.Entities;

public class FoodItem
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public FoodItemType Type { get; set; }
    public string Unit { get; set; } = "g";
    public string? Barcode { get; set; }
    public required string UserId { get; set; }

    // Optional default serving, e.g. a 500mL can: ServingSize=500, ServingSizeName="can".
    // ServingSize is in the item's Unit; ServingSizeName is only set when ServingSize is.
    public decimal? ServingSize { get; set; }
    public string? ServingSizeName { get; set; }

    // Nutritional values per 100g or 100mL
    public decimal CaloriesKcal { get; set; }
    public decimal ProteinG { get; set; }
    public decimal CarbohydratesG { get; set; }
    public decimal FatG { get; set; }
    public decimal FiberG { get; set; }

    public int? CategoryId { get; set; }
    public Category? Category { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<IntakeEntry> IntakeEntries { get; set; } = [];
}
