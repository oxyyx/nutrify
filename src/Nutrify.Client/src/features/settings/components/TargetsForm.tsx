import { useState } from "react";
import type { NutritionTargetsDto } from "@/shared/lib/types";
import { ErrorBanner } from "@/shared/components/ErrorBanner";

interface TargetsFormProps {
  initial: NutritionTargetsDto;
  onSubmit: (targets: NutritionTargetsDto) => void;
  isSubmitting?: boolean;
  isSaved?: boolean;
  error?: string | null;
}

const fields = [
  { key: "caloriesKcal", label: "Calories", unit: "kcal" },
  { key: "proteinG", label: "Protein", unit: "g" },
  { key: "carbohydratesG", label: "Carbs", unit: "g" },
  { key: "fatG", label: "Fat", unit: "g" },
  { key: "fiberG", label: "Fiber", unit: "g" },
] as const;

type TargetKey = (typeof fields)[number]["key"];

function toInput(value: number | null): string {
  return value === null ? "" : String(value);
}

export function TargetsForm({ initial, onSubmit, isSubmitting, isSaved, error }: TargetsFormProps) {
  const [values, setValues] = useState<Record<TargetKey, string>>({
    caloriesKcal: toInput(initial.caloriesKcal),
    proteinG: toInput(initial.proteinG),
    carbohydratesG: toInput(initial.carbohydratesG),
    fatG: toInput(initial.fatG),
    fiberG: toInput(initial.fiberG),
  });

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    const parse = (s: string): number | null => {
      const trimmed = s.trim();
      return trimmed === "" ? null : Number(trimmed);
    };
    onSubmit({
      caloriesKcal: parse(values.caloriesKcal),
      proteinG: parse(values.proteinG),
      carbohydratesG: parse(values.carbohydratesG),
      fatG: parse(values.fatG),
      fiberG: parse(values.fiberG),
    });
  }

  return (
    <form onSubmit={handleSubmit} className="space-y-4">
      <div className="grid grid-cols-2 gap-4 sm:grid-cols-3">
        {fields.map((field) => (
          <div key={field.key}>
            <label className="mb-1 block text-xs text-gray-600">
              {field.label} ({field.unit})
            </label>
            <input
              type="number"
              step="0.01"
              min="0"
              inputMode="decimal"
              value={values[field.key]}
              onChange={(e) => setValues((v) => ({ ...v, [field.key]: e.target.value }))}
              placeholder="No target"
              className="w-full rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-primary focus:ring-1 focus:ring-primary focus:outline-none"
            />
          </div>
        ))}
      </div>

      {error && <ErrorBanner message={error} />}

      <div className="flex items-center gap-3 pt-2">
        <button
          type="submit"
          disabled={isSubmitting}
          className="rounded-md bg-primary px-4 py-2.5 text-sm text-white hover:bg-primary-dark disabled:opacity-50 sm:py-2"
        >
          Save targets
        </button>
        {isSaved && !isSubmitting && <span className="text-sm text-green-600">Saved</span>}
      </div>
    </form>
  );
}
