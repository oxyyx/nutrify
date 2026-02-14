import type { IntakeEntryDto } from "@/shared/lib/types";
import { formatCalories, formatTime } from "@/shared/lib/utils";

interface IntakeEntryListProps {
  entries: IntakeEntryDto[];
  onDelete: (id: number) => void;
}

export function IntakeEntryList({ entries, onDelete }: IntakeEntryListProps) {
  return (
    <div className="space-y-2">
      {entries.map((entry) => (
        <div
          key={entry.id}
          className="flex items-center justify-between rounded-md border border-gray-200 bg-white p-3"
        >
          <div>
            <p className="font-medium text-gray-900">{entry.foodItemName}</p>
            <p className="text-sm text-gray-500">
              {entry.amount}{entry.foodItemUnit} at {formatTime(entry.consumedAt)}
            </p>
          </div>
          <div className="flex items-center gap-4">
            <div className="text-right">
              <p className="font-medium text-orange-600">{formatCalories(entry.caloriesKcal)}</p>
              <p className="text-xs text-gray-500">
                P: {entry.proteinG.toFixed(1)}g | C: {entry.carbohydratesG.toFixed(1)}g | F: {entry.fatG.toFixed(1)}g
              </p>
            </div>
            <button
              onClick={() => onDelete(entry.id)}
              className="text-sm text-red-600 hover:text-red-800"
            >
              Remove
            </button>
          </div>
        </div>
      ))}
    </div>
  );
}
