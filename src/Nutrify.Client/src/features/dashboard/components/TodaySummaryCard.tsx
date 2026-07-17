import type { MacroSummaryDto, NutritionTargetsDto } from "@/shared/lib/types";
import { formatCalories, formatMacro } from "@/shared/lib/utils";

interface TodaySummaryCardProps {
  summary: MacroSummaryDto;
  targets?: NutritionTargetsDto | null;
}

const macroItems = [
  { key: "totalCalories", targetKey: "caloriesKcal", label: "Calories", format: formatCalories, color: "text-orange-600", bar: "bg-orange-500" },
  { key: "totalProteinG", targetKey: "proteinG", label: "Protein", format: (v: number) => formatMacro(v), color: "text-blue-600", bar: "bg-blue-500" },
  { key: "totalCarbohydratesG", targetKey: "carbohydratesG", label: "Carbs", format: (v: number) => formatMacro(v), color: "text-yellow-600", bar: "bg-yellow-500" },
  { key: "totalFatG", targetKey: "fatG", label: "Fat", format: (v: number) => formatMacro(v), color: "text-red-600", bar: "bg-red-500" },
  { key: "totalFiberG", targetKey: "fiberG", label: "Fiber", format: (v: number) => formatMacro(v), color: "text-green-600", bar: "bg-green-500" },
] as const;

export function TodaySummaryCard({ summary, targets }: TodaySummaryCardProps) {
  return (
    <div className="rounded-xl border border-gray-200 bg-white p-4 shadow-card sm:p-6">
      <h2 className="mb-4 text-sm font-semibold uppercase tracking-wide text-gray-400">Today's Summary</h2>
      <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-5">
        {macroItems.map((item) => {
          const value = summary[item.key];
          const target = targets?.[item.targetKey] ?? null;
          const hasTarget = target !== null && target > 0;
          const percent = hasTarget ? (value / target) * 100 : 0;
          const over = percent > 100;

          return (
            <div key={item.key} className="text-center">
              <p className={`text-2xl font-bold ${item.color}`}>
                {item.format(value)}
              </p>
              <p className="text-sm text-gray-500">{item.label}</p>
              {hasTarget && (
                <div className="mt-2">
                  <div className="h-1.5 w-full overflow-hidden rounded-full bg-gray-100">
                    <div
                      className={`h-full rounded-full ${over ? "bg-gray-400" : item.bar}`}
                      style={{ width: `${Math.min(percent, 100)}%` }}
                    />
                  </div>
                  <p className="mt-1 text-xs text-gray-400">
                    {Math.round(percent)}% of {item.format(target)}
                  </p>
                </div>
              )}
            </div>
          );
        })}
      </div>
    </div>
  );
}
