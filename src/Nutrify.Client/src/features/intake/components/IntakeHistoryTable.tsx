import type { IntakeEntryDto } from "@/shared/lib/types";
import { formatCalories, formatDate, formatTime } from "@/shared/lib/utils";

interface IntakeHistoryTableProps {
  entries: IntakeEntryDto[];
  onDelete: (id: number) => void;
}

export function IntakeHistoryTable({ entries, onDelete }: IntakeHistoryTableProps) {
  return (
    <>
      {/* Mobile: card list */}
      <div className="space-y-2 md:hidden">
        {entries.map((entry) => (
          <div key={entry.id} className="rounded-xl border border-gray-200 bg-white p-4 shadow-card">
            <div className="flex items-start justify-between gap-3">
              <div className="min-w-0">
                <p className="truncate font-medium text-gray-900">{entry.foodItemName}</p>
                <p className="mt-0.5 text-xs text-gray-500">
                  {formatDate(entry.consumedAt)} · {formatTime(entry.consumedAt)}
                </p>
              </div>
              <p className="shrink-0 font-medium text-orange-600">{formatCalories(entry.caloriesKcal)}</p>
            </div>
            <div className="mt-2 flex flex-wrap items-baseline justify-between gap-x-4">
              <p className="text-sm text-gray-600">
                {entry.amount}{entry.foodItemUnit}
              </p>
              <p className="text-xs whitespace-nowrap text-gray-500">
                P {entry.proteinG.toFixed(1)}g · C {entry.carbohydratesG.toFixed(1)}g · F{" "}
                {entry.fatG.toFixed(1)}g · Fiber {entry.fiberG.toFixed(1)}g
              </p>
            </div>
            <div className="mt-3 border-t border-gray-100 pt-3">
              <button
                onClick={() => onDelete(entry.id)}
                className="w-full rounded-md bg-red-50 py-2 text-sm text-red-600 active:bg-red-100"
              >
                Remove
              </button>
            </div>
          </div>
        ))}
      </div>

      {/* Desktop: table */}
      <div className="hidden overflow-hidden rounded-xl border border-gray-200 bg-white shadow-card md:block">
        <table className="w-full text-left text-sm">
          <thead className="border-b border-gray-200 bg-gray-50">
            <tr>
              <th className="px-4 py-3 font-medium text-gray-700">Date</th>
              <th className="px-4 py-3 font-medium text-gray-700">Time</th>
              <th className="px-4 py-3 font-medium text-gray-700">Food</th>
              <th className="px-4 py-3 font-medium text-gray-700">Amount</th>
              <th className="px-4 py-3 font-medium text-gray-700">Calories</th>
              <th className="px-4 py-3 font-medium text-gray-700">P / C / F / Fiber</th>
              <th className="px-4 py-3 font-medium text-gray-700">Actions</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-100">
            {entries.map((entry) => (
              <tr key={entry.id} className="hover:bg-gray-50">
                <td className="px-4 py-3 whitespace-nowrap text-gray-600">{formatDate(entry.consumedAt)}</td>
                <td className="px-4 py-3 whitespace-nowrap text-gray-600">{formatTime(entry.consumedAt)}</td>
                <td className="px-4 py-3 font-medium text-gray-900">{entry.foodItemName}</td>
                <td className="px-4 py-3 text-gray-600">
                  {entry.amount}{entry.foodItemUnit}
                </td>
                <td className="px-4 py-3 text-gray-600">{formatCalories(entry.caloriesKcal)}</td>
                <td className="px-4 py-3 text-xs text-gray-500">
                  {entry.proteinG.toFixed(1)}g / {entry.carbohydratesG.toFixed(1)}g / {entry.fatG.toFixed(1)}g / {entry.fiberG.toFixed(1)}g
                </td>
                <td className="px-4 py-3">
                  <button
                    onClick={() => onDelete(entry.id)}
                    className="text-sm text-red-600 hover:text-red-800"
                  >
                    Remove
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </>
  );
}
