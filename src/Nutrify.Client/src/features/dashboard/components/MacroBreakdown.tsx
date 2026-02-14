import type { MacroSummaryDto } from "@/shared/lib/types";
import { formatMacro } from "@/shared/lib/utils";

interface MacroBreakdownProps {
  summary: MacroSummaryDto;
}

export function MacroBreakdown({ summary }: MacroBreakdownProps) {
  const total = summary.totalProteinG + summary.totalCarbohydratesG + summary.totalFatG;

  const macros = [
    { label: "Protein", value: summary.totalProteinG, color: "bg-blue-500" },
    { label: "Carbs", value: summary.totalCarbohydratesG, color: "bg-yellow-500" },
    { label: "Fat", value: summary.totalFatG, color: "bg-red-500" },
  ];

  return (
    <div className="rounded-lg border border-gray-200 bg-white p-6">
      <h2 className="mb-4 text-lg font-semibold text-gray-900">Macro Breakdown</h2>
      {total === 0 ? (
        <p className="text-sm text-gray-500">No intake recorded yet today.</p>
      ) : (
        <div className="space-y-3">
          {macros.map((macro) => {
            const percentage = total > 0 ? (macro.value / total) * 100 : 0;
            return (
              <div key={macro.label}>
                <div className="mb-1 flex justify-between text-sm">
                  <span className="text-gray-700">{macro.label}</span>
                  <span className="text-gray-500">
                    {formatMacro(macro.value)} ({percentage.toFixed(0)}%)
                  </span>
                </div>
                <div className="h-2 w-full rounded-full bg-gray-100">
                  <div
                    className={`h-2 rounded-full ${macro.color}`}
                    style={{ width: `${percentage}%` }}
                  />
                </div>
              </div>
            );
          })}
        </div>
      )}
    </div>
  );
}
