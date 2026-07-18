using Nutrify.Api.Services;
using TUnit.Assertions.Enums;

namespace Nutrify.Api.Tests.Services;

/// <summary>
/// <see cref="DashboardService"/> derives its window from <c>DateTime.UtcNow</c>,
/// so these tests anchor their fixtures to "now" rather than to fixed dates.
/// </summary>
public class DashboardServiceTests
{
    private static DateTime Today(int hour = 12) =>
        DateOnly.FromDateTime(DateTime.UtcNow).ToDateTime(new TimeOnly(hour, 0), DateTimeKind.Utc);

    private static DateTime DaysAgo(int days, int hour = 12) => Today(hour).AddDays(-days);

    // ---- Today -----------------------------------------------------------

    [Test]
    public async Task GetTodayAsync_IncludesOnlyTodaysEntries()
    {
        await using var db = TestDb.Create();
        var service = new DashboardService(db);
        db.IntakeEntries.AddRange(
            TestDb.NewEntry("user1", "Yesterday", DaysAgo(1), calories: 100),
            TestDb.NewEntry("user1", "This morning", Today(8), calories: 200),
            TestDb.NewEntry("user1", "This evening", Today(20), calories: 300),
            TestDb.NewEntry("user1", "Tomorrow", Today().AddDays(1), calories: 400));
        await db.SaveChangesAsync();

        var result = await service.GetTodayAsync("user1");

        await Assert.That(result.Date).IsEqualTo(DateOnly.FromDateTime(DateTime.UtcNow));
        await Assert.That(result.Entries).Count().IsEqualTo(2);
        await Assert.That(result.Summary.TotalCalories).IsEqualTo(500m);
    }

    [Test]
    public async Task GetTodayAsync_SumsMacrosAcrossEntriesScaledByAmount()
    {
        await using var db = TestDb.Create();
        var service = new DashboardService(db);
        db.IntakeEntries.AddRange(
            // 200g of a 380kcal/13g-protein per-100g food -> 760 kcal, 26g protein.
            TestDb.NewEntry("user1", "Oatmeal", Today(8), amount: 200, calories: 380, protein: 13),
            // 50g of a 200kcal/20g-protein per-100g food -> 100 kcal, 10g protein.
            TestDb.NewEntry("user1", "Chicken", Today(13), amount: 50, calories: 200, protein: 20));
        await db.SaveChangesAsync();

        var result = await service.GetTodayAsync("user1");

        await Assert.That(result.Summary.TotalCalories).IsEqualTo(860m);
        await Assert.That(result.Summary.TotalProteinG).IsEqualTo(36m);
    }

    [Test]
    public async Task GetTodayAsync_ReturnsZeroedSummaryWhenNothingLogged()
    {
        await using var db = TestDb.Create();
        var service = new DashboardService(db);

        var result = await service.GetTodayAsync("user1");

        await Assert.That(result.Entries).IsEmpty();
        await Assert.That(result.Summary.TotalCalories).IsEqualTo(0m);
        await Assert.That(result.Summary.TotalProteinG).IsEqualTo(0m);
        await Assert.That(result.Summary.TotalFiberG).IsEqualTo(0m);
    }

    [Test]
    public async Task GetTodayAsync_DoesNotLeakOtherUsersEntries()
    {
        await using var db = TestDb.Create();
        var service = new DashboardService(db);
        db.IntakeEntries.AddRange(
            TestDb.NewEntry("user1", "Mine", Today(8), calories: 100),
            TestDb.NewEntry("user2", "Theirs", Today(8), calories: 999));
        await db.SaveChangesAsync();

        var result = await service.GetTodayAsync("user1");

        await Assert.That(result.Entries).Count().IsEqualTo(1);
        await Assert.That(result.Summary.TotalCalories).IsEqualTo(100m);
    }

    [Test]
    public async Task GetTodayAsync_ReturnsEntriesNewestFirst()
    {
        await using var db = TestDb.Create();
        var service = new DashboardService(db);
        db.IntakeEntries.AddRange(
            TestDb.NewEntry("user1", "Breakfast", Today(8)),
            TestDb.NewEntry("user1", "Dinner", Today(19)),
            TestDb.NewEntry("user1", "Lunch", Today(13)));
        await db.SaveChangesAsync();

        var result = await service.GetTodayAsync("user1");

        await Assert.That(result.Entries.Select(e => e.FoodItemName).ToList())
            .IsEquivalentTo(new List<string> { "Dinner", "Lunch", "Breakfast" }, CollectionOrdering.Matching);
    }

    // ---- Weekly ----------------------------------------------------------

    [Test]
    public async Task GetWeeklyAsync_ReturnsSevenConsecutiveDaysEndingToday()
    {
        await using var db = TestDb.Create();
        var service = new DashboardService(db);

        var result = await service.GetWeeklyAsync("user1");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        await Assert.That(result.EndDate).IsEqualTo(today);
        await Assert.That(result.StartDate).IsEqualTo(today.AddDays(-6));
        await Assert.That(result.Days).Count().IsEqualTo(7);
        await Assert.That(result.Days.Select(d => d.Date).ToList())
            .IsEquivalentTo(
                Enumerable.Range(0, 7).Select(i => today.AddDays(-6 + i)).ToList(),
                CollectionOrdering.Matching);
    }

    [Test]
    public async Task GetWeeklyAsync_BucketsEntriesIntoTheirOwnDay()
    {
        await using var db = TestDb.Create();
        var service = new DashboardService(db);
        db.IntakeEntries.AddRange(
            TestDb.NewEntry("user1", "Today", Today(8), calories: 100),
            TestDb.NewEntry("user1", "Two days ago", DaysAgo(2), calories: 200),
            TestDb.NewEntry("user1", "Two days ago again", DaysAgo(2, 18), calories: 50));
        await db.SaveChangesAsync();

        var result = await service.GetWeeklyAsync("user1");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var todayBucket = result.Days.Single(d => d.Date == today);
        await Assert.That(todayBucket.EntryCount).IsEqualTo(1);
        await Assert.That(todayBucket.Summary.TotalCalories).IsEqualTo(100m);

        var twoDaysAgo = result.Days.Single(d => d.Date == today.AddDays(-2));
        await Assert.That(twoDaysAgo.EntryCount).IsEqualTo(2);
        await Assert.That(twoDaysAgo.Summary.TotalCalories).IsEqualTo(250m);
    }

    [Test]
    public async Task GetWeeklyAsync_ZeroesDaysWithNoEntries()
    {
        await using var db = TestDb.Create();
        var service = new DashboardService(db);
        db.IntakeEntries.Add(TestDb.NewEntry("user1", "Today", Today(8), calories: 100));
        await db.SaveChangesAsync();

        var result = await service.GetWeeklyAsync("user1");

        var emptyDays = result.Days.Where(d => d.Date != DateOnly.FromDateTime(DateTime.UtcNow)).ToList();
        await Assert.That(emptyDays).Count().IsEqualTo(6);
        await Assert.That(emptyDays.All(d => d.EntryCount == 0)).IsTrue();
        await Assert.That(emptyDays.All(d => d.Summary.TotalCalories == 0m)).IsTrue();
    }

    [Test]
    public async Task GetWeeklyAsync_ExcludesEntriesOlderThanTheWindow()
    {
        await using var db = TestDb.Create();
        var service = new DashboardService(db);
        db.IntakeEntries.AddRange(
            // Day -6 is the first day in the window; day -7 falls outside it.
            TestDb.NewEntry("user1", "Edge of window", DaysAgo(6), calories: 100),
            TestDb.NewEntry("user1", "Too old", DaysAgo(7), calories: 999));
        await db.SaveChangesAsync();

        var result = await service.GetWeeklyAsync("user1");

        await Assert.That(result.Days.Sum(d => d.Summary.TotalCalories)).IsEqualTo(100m);
        await Assert.That(result.Days.Sum(d => d.EntryCount)).IsEqualTo(1);
    }

    [Test]
    public async Task GetWeeklyAsync_DoesNotLeakOtherUsersEntries()
    {
        await using var db = TestDb.Create();
        var service = new DashboardService(db);
        db.IntakeEntries.AddRange(
            TestDb.NewEntry("user1", "Mine", Today(8), calories: 100),
            TestDb.NewEntry("user2", "Theirs", Today(8), calories: 999));
        await db.SaveChangesAsync();

        var result = await service.GetWeeklyAsync("user1");

        await Assert.That(result.Days.Sum(d => d.Summary.TotalCalories)).IsEqualTo(100m);
    }
}
