namespace Nutrify.Api.Entities;

public class IntakeEntry
{
    public int Id { get; set; }
    public required string UserId { get; set; }
    public int FoodItemId { get; set; }
    public FoodItem FoodItem { get; set; } = null!;
    public decimal Amount { get; set; }
    public DateTime ConsumedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
