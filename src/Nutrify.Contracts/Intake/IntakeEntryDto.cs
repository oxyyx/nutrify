namespace Nutrify.Contracts.Intake;

public record IntakeEntryDto(
    int Id,
    int? FoodItemId,
    string FoodItemName,
    string FoodItemUnit,
    decimal Amount,
    decimal CaloriesKcal,
    decimal ProteinG,
    decimal CarbohydratesG,
    decimal FatG,
    decimal FiberG,
    DateTime ConsumedAt
);
