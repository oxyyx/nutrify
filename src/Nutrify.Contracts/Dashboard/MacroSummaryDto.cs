namespace Nutrify.Contracts.Dashboard;

public record MacroSummaryDto(
    decimal TotalCalories,
    decimal TotalProteinG,
    decimal TotalCarbohydratesG,
    decimal TotalFatG,
    decimal TotalFiberG
);
