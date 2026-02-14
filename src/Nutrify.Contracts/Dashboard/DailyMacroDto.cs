namespace Nutrify.Contracts.Dashboard;

public record DailyMacroDto(
    DateOnly Date,
    MacroSummaryDto Summary,
    int EntryCount
);
