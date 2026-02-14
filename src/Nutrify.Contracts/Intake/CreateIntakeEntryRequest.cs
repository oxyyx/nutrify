namespace Nutrify.Contracts.Intake;

public record CreateIntakeEntryRequest(
    int FoodItemId,
    decimal Amount,
    DateTime? ConsumedAt
);
