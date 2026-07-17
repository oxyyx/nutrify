import { createFileRoute } from "@tanstack/react-router";
import { useUserSettings } from "@/features/settings/hooks/useUserSettings";
import { useUpdateUserSettings } from "@/features/settings/hooks/useUpdateUserSettings";
import { TargetsForm } from "@/features/settings/components/TargetsForm";
import { LoadingSpinner } from "@/shared/components/LoadingSpinner";
import { ErrorState } from "@/shared/components/ErrorState";
import { getErrorMessage } from "@/shared/lib/utils";

function SettingsPage() {
  const { data, isLoading, isError, error, refetch } = useUserSettings();
  const updateMutation = useUpdateUserSettings();

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold text-gray-900">Settings</h1>

      <section className="rounded-xl border border-gray-200 bg-white p-4 shadow-card sm:p-6">
        <h2 className="text-sm font-semibold uppercase tracking-wide text-gray-400">
          Daily Targets
        </h2>
        <p className="mt-1 mb-4 text-sm text-gray-500">
          Set a daily goal for any nutrient. Leave a field blank for no target — your
          dashboard shows today's progress toward the ones you set.
        </p>

        {isLoading ? (
          <LoadingSpinner className="py-8" />
        ) : isError ? (
          <ErrorState
            title="Couldn't load settings"
            message={getErrorMessage(error)}
            onRetry={() => refetch()}
          />
        ) : data ? (
          <TargetsForm
            initial={data.targets}
            onSubmit={(targets) => updateMutation.mutate({ targets })}
            isSubmitting={updateMutation.isPending}
            isSaved={updateMutation.isSuccess}
            error={updateMutation.isError ? getErrorMessage(updateMutation.error) : null}
          />
        ) : null}
      </section>
    </div>
  );
}

export const Route = createFileRoute("/settings")({
  component: SettingsPage,
});
