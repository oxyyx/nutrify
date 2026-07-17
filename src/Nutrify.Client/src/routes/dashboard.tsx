import { createFileRoute } from "@tanstack/react-router";
import { useTodayDashboard } from "@/features/dashboard/hooks/useTodayDashboard";
import { useWeeklyOverview } from "@/features/dashboard/hooks/useWeeklyOverview";
import { useUserSettings } from "@/features/settings/hooks/useUserSettings";
import { TodaySummaryCard } from "@/features/dashboard/components/TodaySummaryCard";
import { WeeklyChart } from "@/features/dashboard/components/WeeklyChart";
import { IntakeTimeline } from "@/features/dashboard/components/IntakeTimeline";
import { LoadingSpinner } from "@/shared/components/LoadingSpinner";
import { ErrorState } from "@/shared/components/ErrorState";
import { getErrorMessage } from "@/shared/lib/utils";

function DashboardPage() {
  const todayQuery = useTodayDashboard();
  const weeklyQuery = useWeeklyOverview();
  // Targets are supplementary: never block or error the dashboard on them.
  const { data: settings } = useUserSettings();
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
          <div className="grid gap-4 sm:gap-6 lg:grid-cols-3">
            <div className="lg:col-span-2">
              <TodaySummaryCard summary={today.summary} targets={settings?.targets} />
            </div>
            {weekly && (
              <div className="lg:col-span-1">
                <WeeklyChart days={weekly.days} />
              </div>
            )}
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
