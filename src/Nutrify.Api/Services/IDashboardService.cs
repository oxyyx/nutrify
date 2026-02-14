using Nutrify.Contracts.Dashboard;

namespace Nutrify.Api.Services;

public interface IDashboardService
{
    Task<DailyDashboardDto> GetTodayAsync(string userId);
    Task<WeeklyOverviewDto> GetWeeklyAsync(string userId);
}
