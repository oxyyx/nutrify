import { Link, createFileRoute } from "@tanstack/react-router";
import { useIntakeEntries } from "@/features/intake/hooks/useIntakeEntries";
import { useDeleteIntakeEntry } from "@/features/intake/hooks/useDeleteIntakeEntry";
import { IntakeEntryList } from "@/features/intake/components/IntakeEntryList";
import { EmptyState } from "@/shared/components/EmptyState";
import { ErrorState } from "@/shared/components/ErrorState";
import { ErrorBanner } from "@/shared/components/ErrorBanner";
import { LoadingSpinner } from "@/shared/components/LoadingSpinner";
import { getErrorMessage, getLocalDateString } from "@/shared/lib/utils";
import { IconPlus } from "@/shared/components/nav";

function IntakePage() {
  const today = getLocalDateString();
  const { data, isLoading, isError, error, refetch } = useIntakeEntries({ date: today });
  const deleteMutation = useDeleteIntakeEntry();

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between gap-3">
        <h1 className="text-2xl font-bold text-gray-900">Today's Intake</h1>
        <Link
          to="/intake/new"
          aria-label="Add entry"
          title="Add entry"
          className="flex h-9 w-9 items-center justify-center rounded-md bg-primary text-white hover:bg-primary-dark"
        >
          <IconPlus className="h-5 w-5 shrink-0" />
        </Link>
      </div>

      {deleteMutation.isError && (
        <ErrorBanner message={getErrorMessage(deleteMutation.error)} />
      )}

      {isLoading ? (
        <LoadingSpinner className="py-12" />
      ) : isError ? (
        <ErrorState
          title="Couldn't load today's intake"
          message={getErrorMessage(error)}
          onRetry={() => refetch()}
        />
      ) : data && data.items.length > 0 ? (
        <IntakeEntryList
          entries={data.items}
          onDelete={(id) => deleteMutation.mutate(id)}
        />
      ) : (
        <EmptyState
          title="No intake logged today"
          description="Start tracking your nutrition by adding an entry."
        />
      )}
    </div>
  );
}

export const Route = createFileRoute("/intake/")({
  component: IntakePage,
});
