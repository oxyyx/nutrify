namespace Nutrify.Contracts.Intake;

public record UpdateIntakeEntryRequest(
    decimal Amount,
    DateTime ConsumedAt
);
