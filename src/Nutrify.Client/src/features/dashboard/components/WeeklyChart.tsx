import type { DailyMacroDto } from "@/shared/lib/types";

interface WeeklyChartProps {
  days: DailyMacroDto[];
}

export function WeeklyChart({ days }: WeeklyChartProps) {
  const maxCalories = Math.max(...days.map((d) => d.summary.totalCalories), 1);

  return (
    <div className="rounded-xl border border-gray-200 bg-white p-4 shadow-card sm:p-6">
      <h2 className="mb-4 text-sm font-semibold uppercase tracking-wide text-gray-400">Weekly Overview</h2>
      <div className="flex items-end gap-2">
        {days.map((day) => {
          const height = (day.summary.totalCalories / maxCalories) * 100;
          const date = new Date(day.date);
          const dayLabel = date.toLocaleDateString(undefined, { weekday: "short" });

          return (
            <div key={day.date} className="flex flex-1 flex-col items-center gap-1">
              <span className="text-xs text-gray-500">
                {Math.round(day.summary.totalCalories)}
              </span>
              {/* Fixed-height track so the bar's percentage height resolves
                  against a definite value (a flex-grown parent doesn't). */}
              <div className="flex h-40 w-full items-end">
                <div
                  className="w-full rounded-t bg-primary/80"
                  style={{ height: `${Math.max(height, 2)}%` }}
                />
              </div>
              <span className="text-xs font-medium text-gray-600">{dayLabel}</span>
            </div>
          );
        })}
      </div>
    </div>
  );
}
