import { useState } from "react";
import type { CreateFoodItemRequest, FoodItemDto } from "@/shared/lib/types";
import { FoodItemType } from "@/shared/lib/types";
import { CategorySelect } from "./CategorySelect";
import { ErrorBanner } from "@/shared/components/ErrorBanner";

interface FoodItemFormProps {
  initialData?: FoodItemDto;
  onSubmit: (data: CreateFoodItemRequest) => void;
  onCancel: () => void;
  isSubmitting?: boolean;
  error?: string | null;
}

export function FoodItemForm({ initialData, onSubmit, onCancel, isSubmitting, error }: FoodItemFormProps) {
  const [name, setName] = useState(initialData?.name ?? "");
  const [type, setType] = useState<FoodItemType>(initialData?.type ?? FoodItemType.Food);
  const [caloriesKcal, setCaloriesKcal] = useState(String(initialData?.caloriesKcal ?? ""));
  const [proteinG, setProteinG] = useState(String(initialData?.proteinG ?? ""));
  const [carbohydratesG, setCarbohydratesG] = useState(String(initialData?.carbohydratesG ?? ""));
  const [fatG, setFatG] = useState(String(initialData?.fatG ?? ""));
  const [fiberG, setFiberG] = useState(String(initialData?.fiberG ?? ""));
  const [categoryId, setCategoryId] = useState<number | null>(initialData?.categoryId ?? null);

  const unit = type === FoodItemType.Drink ? "mL" : "g";

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    onSubmit({
      name,
      type,
      caloriesKcal: Number(caloriesKcal),
      proteinG: Number(proteinG),
      carbohydratesG: Number(carbohydratesG),
      fatG: Number(fatG),
      fiberG: Number(fiberG),
      categoryId,
    });
  }

  return (
    <form onSubmit={handleSubmit} className="space-y-4">
      <div>
        <label className="mb-1 block text-sm font-medium text-gray-700">Name</label>
        <input
          type="text"
          value={name}
          onChange={(e) => setName(e.target.value)}
          required
          className="w-full rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-primary focus:ring-1 focus:ring-primary focus:outline-none"
        />
      </div>

      <div>
        <label className="mb-1 block text-sm font-medium text-gray-700">Type</label>
        <div className="flex gap-4">
          {[FoodItemType.Food, FoodItemType.Drink].map((t) => (
            <label key={t} className="flex items-center gap-2 text-sm">
              <input
                type="radio"
                checked={type === t}
                onChange={() => setType(t)}
                className="text-primary focus:ring-primary"
              />
              {t === FoodItemType.Food ? "Food (g)" : "Drink (mL)"}
            </label>
          ))}
        </div>
      </div>

      <div>
        <label className="mb-1 block text-sm font-medium text-gray-700">Category</label>
        <CategorySelect value={categoryId} onChange={setCategoryId} />
      </div>

      <p className="text-sm font-medium text-gray-700">
        Nutritional values per 100{unit}
      </p>

      <div className="grid grid-cols-2 gap-4 sm:grid-cols-3">
        {[
          { label: "Calories (kcal)", value: caloriesKcal, setter: setCaloriesKcal },
          { label: "Protein (g)", value: proteinG, setter: setProteinG },
          { label: "Carbohydrates (g)", value: carbohydratesG, setter: setCarbohydratesG },
          { label: "Fat (g)", value: fatG, setter: setFatG },
          { label: "Fiber (g)", value: fiberG, setter: setFiberG },
        ].map((field) => (
          <div key={field.label}>
            <label className="mb-1 block text-xs text-gray-600">{field.label}</label>
            <input
              type="number"
              step="0.01"
              min="0"
              value={field.value}
              onChange={(e) => field.setter(e.target.value)}
              required
              className="w-full rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-primary focus:ring-1 focus:ring-primary focus:outline-none"
            />
          </div>
        ))}
      </div>

      {error && <ErrorBanner message={error} />}

      <div className="flex justify-end gap-3 pt-2">
        <button
          type="button"
          onClick={onCancel}
          className="rounded-md bg-gray-100 px-4 py-2 text-sm text-gray-700 hover:bg-gray-200"
        >
          Cancel
        </button>
        <button
          type="submit"
          disabled={isSubmitting}
          className="rounded-md bg-primary px-4 py-2 text-sm text-white hover:bg-primary-dark disabled:opacity-50"
        >
          {initialData ? "Update" : "Create"} Food Item
        </button>
      </div>
    </form>
  );
}
