import { createFileRoute, useNavigate } from "@tanstack/react-router";
import { IntakeEntryForm } from "@/features/intake/components/IntakeEntryForm";
import { useCreateIntakeEntry } from "@/features/intake/hooks/useCreateIntakeEntry";
import { getErrorMessage } from "@/shared/lib/utils";

function NewIntakePage() {
  const navigate = useNavigate();
  const mutation = useCreateIntakeEntry();

  return (
    <div className="mx-auto max-w-2xl space-y-6">
      <h1 className="text-2xl font-bold text-gray-900">Log Intake</h1>
      <div className="rounded-xl border border-gray-200 bg-white p-4 shadow-card sm:p-6">
        <IntakeEntryForm
          onSubmit={(data) =>
            mutation.mutate(data, {
              onSuccess: () => navigate({ to: "/intake" }),
            })
          }
          onCancel={() => navigate({ to: "/intake" })}
          isSubmitting={mutation.isPending}
          error={mutation.isError ? getErrorMessage(mutation.error) : null}
        />
      </div>
    </div>
  );
}

export const Route = createFileRoute("/intake/new")({
  component: NewIntakePage,
});
