namespace Nutrify.Api.Entities;

public class IntakeEntry
{
    public int Id { get; set; }
    public required string UserId { get; set; }

    // Optional link back to the source food item. Nullable so a food item can be
    // deleted without erasing history; the entry renders entirely from the
    // snapshot below, and this FK is set to null when the food item is removed.
    public int? FoodItemId { get; set; }
    public FoodItem? FoodItem { get; set; }

    public decimal Amount { get; set; }
    public DateTime ConsumedAt { get; set; }
    public DateTime CreatedAt { get; set; }

    // Snapshot of the food item's identity and per-100g/mL nutrition, copied at
    // creation time. This makes the entry self-contained, so later edits to the
    // food item (or its deletion) never rewrite what was actually logged.
    public required string FoodItemName { get; set; }
    public required string FoodItemUnit { get; set; }
    public decimal CaloriesKcal { get; set; }
    public decimal ProteinG { get; set; }
    public decimal CarbohydratesG { get; set; }
    public decimal FatG { get; set; }
    public decimal FiberG { get; set; }
}
