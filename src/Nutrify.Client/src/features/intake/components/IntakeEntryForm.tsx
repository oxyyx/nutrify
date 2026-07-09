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

export function IntakeEntryForm({ onSubmit, onCancel, isSubmitting, error }: IntakeEntryFormProps) {
  const [selectedFood, setSelectedFood] = useState<FoodItemDto | null>(null);
  const [amount, setAmount] = useState("");

  const preview = selectedFood && amount
    ? calculateMacros(selectedFood, Number(amount))
    : null;

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (!selectedFood) return;

    onSubmit({
      foodItemId: selectedFood.id,
      amount: Number(amount),
    });
  }

  return (
    <form onSubmit={handleSubmit} className="space-y-4">
      <div>
        <label className="mb-1 block text-sm font-medium text-gray-700">Food Item</label>
        <FoodSearchSelect
          selected={selectedFood}
          onSelect={setSelectedFood}
        />
      </div>

      {selectedFood && (
        <div>
          <label className="mb-1 block text-sm font-medium text-gray-700">
            Amount ({selectedFood.unit})
          </label>
          <input
            type="number"
            step="0.1"
            min="0"
            value={amount}
            onChange={(e) => setAmount(e.target.value)}
            required
            placeholder={`Amount in ${selectedFood.unit}`}
            className="w-full rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-primary focus:ring-1 focus:ring-primary focus:outline-none"
          />
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
