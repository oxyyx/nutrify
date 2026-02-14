using Nutrify.Contracts.Intake;

namespace Nutrify.Contracts.Dashboard;

public record DailyDashboardDto(
    DateOnly Date,
    MacroSummaryDto Summary,
    List<IntakeEntryDto> Entries
);
