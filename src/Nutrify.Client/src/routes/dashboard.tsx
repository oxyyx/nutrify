import { createFileRoute } from "@tanstack/react-router";
import { useTodayDashboard } from "@/features/dashboard/hooks/useTodayDashboard";
import { useWeeklyOverview } from "@/features/dashboard/hooks/useWeeklyOverview";
import { TodaySummaryCard } from "@/features/dashboard/components/TodaySummaryCard";
import { MacroBreakdown } from "@/features/dashboard/components/MacroBreakdown";
import { WeeklyChart } from "@/features/dashboard/components/WeeklyChart";
import { IntakeTimeline } from "@/features/dashboard/components/IntakeTimeline";
import { LoadingSpinner } from "@/shared/components/LoadingSpinner";
import { ErrorState } from "@/shared/components/ErrorState";
import { getErrorMessage } from "@/shared/lib/utils";

function DashboardPage() {
  const todayQuery = useTodayDashboard();
  const weeklyQuery = useWeeklyOverview();
  const { data: today } = todayQuery;
  const { data: weekly } = weeklyQuery;

  if (todayQuery.isLoading || weeklyQuery.isLoading) {
    return <LoadingSpinner className="py-12" />;
  }

  if (todayQuery.isError || weeklyQuery.isError) {
    return (
      <ErrorState
        title="Couldn't load your dashboard"
        message={getErrorMessage(todayQuery.error ?? weeklyQuery.error)}
        onRetry={() => {
          todayQuery.refetch();
          weeklyQuery.refetch();
        }}
      />
    );
  }

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold text-gray-900">Dashboard</h1>

      {today && (
        <>
          <TodaySummaryCard summary={today.summary} />
          <div className="grid gap-4 sm:gap-6 lg:grid-cols-2">
            <MacroBreakdown summary={today.summary} />
            {weekly && <WeeklyChart days={weekly.days} />}
          </div>
          <IntakeTimeline entries={today.entries} />
        </>
      )}
    </div>
  );
}

export const Route = createFileRoute("/dashboard")({
  component: DashboardPage,
});
