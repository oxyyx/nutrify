import type { MacroSummaryDto } from "@/shared/lib/types";
import { formatCalories, formatMacro } from "@/shared/lib/utils";

interface TodaySummaryCardProps {
  summary: MacroSummaryDto;
}

const macroItems = [
  { key: "totalCalories", label: "Calories", format: formatCalories, color: "text-orange-600" },
  { key: "totalProteinG", label: "Protein", format: (v: number) => formatMacro(v), color: "text-blue-600" },
  { key: "totalCarbohydratesG", label: "Carbs", format: (v: number) => formatMacro(v), color: "text-yellow-600" },
  { key: "totalFatG", label: "Fat", format: (v: number) => formatMacro(v), color: "text-red-600" },
  { key: "totalFiberG", label: "Fiber", format: (v: number) => formatMacro(v), color: "text-green-600" },
] as const;

export function TodaySummaryCard({ summary }: TodaySummaryCardProps) {
  return (
    <div className="rounded-lg border border-gray-200 bg-white p-6">
      <h2 className="mb-4 text-lg font-semibold text-gray-900">Today's Summary</h2>
      <div className="grid grid-cols-2 gap-4 sm:grid-cols-5">
        {macroItems.map((item) => (
          <div key={item.key} className="text-center">
            <p className={`text-2xl font-bold ${item.color}`}>
              {item.format(summary[item.key])}
            </p>
            <p className="text-sm text-gray-500">{item.label}</p>
          </div>
        ))}
      </div>
    </div>
  );
}
