import { useState } from "react";
import type { CreateIntakeEntryRequest, FoodItemDto } from "@/shared/lib/types";
import { FoodSearchSelect } from "./FoodSearchSelect";
import { calculateMacros, formatCalories, formatMacro } from "@/shared/lib/utils";
import { ErrorBanner } from "@/shared/components/ErrorBanner";

interface IntakeEntryFormProps {
  onSubmit: (data: CreateIntakeEntryRequest) => void;
  onCancel: () => void;
  isSubmitting?: boolean;
  error?: string | null;
}

/** How the amount field is interpreted: raw g/mL, or number of servings. */
type AmountMode = "unit" | "serving";

export function IntakeEntryForm({ onSubmit, onCancel, isSubmitting, error }: IntakeEntryFormProps) {
  const [selectedFood, setSelectedFood] = useState<FoodItemDto | null>(null);
  const [amount, setAmount] = useState("");
  const [mode, setMode] = useState<AmountMode>("unit");

  const servingSize = selectedFood?.servingSize ?? null;
  const servingName = selectedFood?.servingSizeName ?? "serving";

  // Amount in the item's unit (g/mL), regardless of entry mode.
  const effectiveAmount =
    amount === ""
      ? null
      : mode === "serving" && servingSize
        ? Number(amount) * servingSize
        : Number(amount);

  const preview =
    selectedFood && effectiveAmount !== null
      ? calculateMacros(selectedFood, effectiveAmount)
      : null;

  function handleSelectFood(food: FoodItemDto | null) {
    setSelectedFood(food);
    // Items with a serving default to serving entry — that's the quick path
    // ("1 can"); switching to raw g/mL stays one tap away.
    if (food?.servingSize) {
      setMode("serving");
      setAmount("1");
    } else {
      setMode("unit");
      setAmount("");
    }
  }

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (!selectedFood || effectiveAmount === null) return;

    onSubmit({
      foodItemId: selectedFood.id,
      amount: effectiveAmount,
    });
  }

  return (
    <form onSubmit={handleSubmit} className="space-y-4">
      <div>
        <label className="mb-1 block text-sm font-medium text-gray-700">Food Item</label>
        <FoodSearchSelect
          selected={selectedFood}
          onSelect={handleSelectFood}
        />
      </div>

      {selectedFood && (
        <div>
          <label className="mb-1 block text-sm font-medium text-gray-700">Amount</label>
          <div className="flex gap-2">
            <input
              type="number"
              step={mode === "serving" ? "0.5" : "0.1"}
              min="0"
              value={amount}
              onChange={(e) => setAmount(e.target.value)}
              required
              placeholder={
                mode === "serving"
                  ? `Number of ${servingName}s`
                  : `Amount in ${selectedFood.unit}`
              }
              className="min-w-0 flex-1 rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-primary focus:ring-1 focus:ring-primary focus:outline-none"
            />
            {servingSize ? (
              <select
                value={mode}
                onChange={(e) => setMode(e.target.value as AmountMode)}
                className="rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-primary focus:ring-1 focus:ring-primary focus:outline-none"
              >
                <option value="serving">
                  {servingName} ({servingSize}
                  {selectedFood.unit})
                </option>
                <option value="unit">{selectedFood.unit}</option>
              </select>
            ) : (
              <span className="flex items-center rounded-md bg-gray-50 px-3 text-sm text-gray-500">
                {selectedFood.unit}
              </span>
            )}
          </div>
          {mode === "serving" && effectiveAmount !== null && (
            <p className="mt-1 text-xs text-gray-500">
              = {Math.round(effectiveAmount * 10) / 10}{selectedFood.unit}
            </p>
          )}
        </div>
      )}

      {preview && (
        <div className="rounded-md bg-gray-50 p-3">
          <p className="mb-1 text-sm font-medium text-gray-700">Nutritional preview:</p>
          <p className="text-sm text-gray-600">
            {formatCalories(preview.calories)} | P: {formatMacro(preview.protein)} | C: {formatMacro(preview.carbohydrates)} | F: {formatMacro(preview.fat)} | Fiber: {formatMacro(preview.fiber)}
          </p>
        </div>
      )}

      {error && <ErrorBanner message={error} />}

      <div className="flex flex-col-reverse gap-3 pt-2 sm:flex-row sm:justify-end">
        <button
          type="button"
          onClick={onCancel}
          className="rounded-md bg-gray-100 px-4 py-2.5 text-sm text-gray-700 hover:bg-gray-200 sm:py-2"
        >
          Cancel
        </button>
        <button
          type="submit"
          disabled={isSubmitting || !selectedFood || !amount}
          className="rounded-md bg-primary px-4 py-2.5 text-sm text-white hover:bg-primary-dark disabled:opacity-50 sm:py-2"
        >
          Log Intake
        </button>
      </div>
    </form>
  );
}
