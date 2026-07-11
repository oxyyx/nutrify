import type { IntakeEntryDto } from "@/shared/lib/types";
import { formatCalories, formatTime } from "@/shared/lib/utils";

interface IntakeTimelineProps {
  entries: IntakeEntryDto[];
}

export function IntakeTimeline({ entries }: IntakeTimelineProps) {
  if (entries.length === 0) {
    return (
      <div className="rounded-xl border border-gray-200 bg-white p-4 shadow-card sm:p-6">
        <h2 className="mb-4 text-sm font-semibold uppercase tracking-wide text-gray-400">Today's Intake</h2>
        <p className="text-sm text-gray-500">No entries yet. Start logging your meals!</p>
      </div>
    );
  }

  return (
    <div className="rounded-xl border border-gray-200 bg-white p-4 shadow-card sm:p-6">
      <h2 className="mb-4 text-sm font-semibold uppercase tracking-wide text-gray-400">Today's Intake</h2>
      <div className="space-y-3">
        {entries.map((entry) => (
          <div key={entry.id} className="rounded-md border border-gray-100 p-3">
            <div className="flex items-baseline justify-between gap-4">
              <p className="min-w-0 truncate font-medium text-gray-900">{entry.foodItemName}</p>
              <p className="shrink-0 font-medium text-orange-600">{formatCalories(entry.caloriesKcal)}</p>
            </div>
            <div className="mt-0.5 flex flex-wrap items-baseline justify-between gap-x-4">
              <p className="text-sm text-gray-500">
                {entry.amount}{entry.foodItemUnit} at {formatTime(entry.consumedAt)}
              </p>
              <p className="text-xs whitespace-nowrap text-gray-500">
                P: {entry.proteinG.toFixed(1)}g | C: {entry.carbohydratesG.toFixed(1)}g | F: {entry.fatG.toFixed(1)}g
              </p>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
