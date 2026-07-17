using Microsoft.EntityFrameworkCore;
using Nutrify.Api.Data;
using Nutrify.Api.Mapping;
using Nutrify.Contracts.Dashboard;

namespace Nutrify.Api.Services;

public class DashboardService(NutrifyDbContext db) : IDashboardService
{
    public async Task<DailyDashboardDto> GetTodayAsync(string userId)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var start = today.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var end = today.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

        var entries = await db.IntakeEntries
            .Where(e => e.UserId == userId && e.ConsumedAt >= start && e.ConsumedAt < end)
            .OrderByDescending(e => e.ConsumedAt)
            .ToListAsync();

        var entryDtos = entries.Select(e => e.ToDto()).ToList();
        var summary = entryDtos.ToMacroSummary();

        return new DailyDashboardDto(today, summary, entryDtos);
    }

    public async Task<WeeklyOverviewDto> GetWeeklyAsync(string userId)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var startDate = today.AddDays(-6);
        var start = startDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var end = today.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

        var entries = await db.IntakeEntries
            .Where(e => e.UserId == userId && e.ConsumedAt >= start && e.ConsumedAt < end)
            .ToListAsync();

        var entryDtos = entries.Select(e => e.ToDto()).ToList();

        var days = new List<DailyMacroDto>();
        for (var date = startDate; date <= today; date = date.AddDays(1))
        {
            var dayEntries = entryDtos
                .Where(e => DateOnly.FromDateTime(e.ConsumedAt) == date)
                .ToList();

            days.Add(new DailyMacroDto(
                date,
                dayEntries.ToMacroSummary(),
                dayEntries.Count
            ));
        }

        return new WeeklyOverviewDto(startDate, today, days);
    }
}
