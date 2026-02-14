namespace Nutrify.Contracts.Dashboard;

public record WeeklyOverviewDto(
    DateOnly StartDate,
    DateOnly EndDate,
    List<DailyMacroDto> Days
);
