import type { IntakeEntryDto } from "@/shared/lib/types";
import { formatCalories, formatTime } from "@/shared/lib/utils";

interface IntakeTimelineProps {
  entries: IntakeEntryDto[];
}

export function IntakeTimeline({ entries }: IntakeTimelineProps) {
  if (entries.length === 0) {
    return (
      <div className="rounded-lg border border-gray-200 bg-white p-6">
        <h2 className="mb-4 text-lg font-semibold text-gray-900">Today's Intake</h2>
        <p className="text-sm text-gray-500">No entries yet. Start logging your meals!</p>
      </div>
    );
  }

  return (
    <div className="rounded-lg border border-gray-200 bg-white p-6">
      <h2 className="mb-4 text-lg font-semibold text-gray-900">Today's Intake</h2>
      <div className="space-y-3">
        {entries.map((entry) => (
          <div
            key={entry.id}
            className="flex items-center justify-between rounded-md border border-gray-100 p-3"
          >
            <div>
              <p className="font-medium text-gray-900">{entry.foodItemName}</p>
              <p className="text-sm text-gray-500">
                {entry.amount}{entry.foodItemUnit} at {formatTime(entry.consumedAt)}
              </p>
            </div>
            <div className="text-right">
              <p className="font-medium text-orange-600">{formatCalories(entry.caloriesKcal)}</p>
              <p className="text-xs text-gray-500">
                P: {entry.proteinG.toFixed(1)}g | C: {entry.carbohydratesG.toFixed(1)}g | F: {entry.fatG.toFixed(1)}g
              </p>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
