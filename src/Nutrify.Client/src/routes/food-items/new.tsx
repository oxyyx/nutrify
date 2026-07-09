import { createFileRoute, useNavigate } from "@tanstack/react-router";
import { FoodItemForm } from "@/features/food-items/components/FoodItemForm";
import { useCreateFoodItem } from "@/features/food-items/hooks/useCreateFoodItem";
import { getErrorMessage } from "@/shared/lib/utils";
import { FoodItemType } from "@/shared/lib/types";

// Prefill passed by the barcode scanner page; all fields optional.
interface NewFoodItemSearch {
  barcode?: string;
  name?: string;
  type?: FoodItemType;
  caloriesKcal?: number;
  proteinG?: number;
  carbohydratesG?: number;
  fatG?: number;
  fiberG?: number;
}

function asNumber(value: unknown): number | undefined {
  const num = Number(value);
  return typeof value === "number" || (typeof value === "string" && value !== "" && !Number.isNaN(num))
    ? num
    : undefined;
}

function NewFoodItemPage() {
  const navigate = useNavigate();
  const search = Route.useSearch();
  const mutation = useCreateFoodItem();

  return (
    <div className="mx-auto max-w-2xl space-y-6">
      <h1 className="text-2xl font-bold text-gray-900">Add Food Item</h1>
      <div className="rounded-xl border border-gray-200 bg-white p-6 shadow-card">
        <FoodItemForm
          prefill={{
            barcode: search.barcode ?? null,
            name: search.name,
            type: search.type,
            caloriesKcal: search.caloriesKcal,
            proteinG: search.proteinG,
            carbohydratesG: search.carbohydratesG,
            fatG: search.fatG,
            fiberG: search.fiberG,
          }}
          onSubmit={(data) =>
            mutation.mutate(data, {
              onSuccess: () => navigate({ to: "/food-items" }),
            })
          }
          onCancel={() => navigate({ to: "/food-items" })}
          isSubmitting={mutation.isPending}
          error={mutation.isError ? getErrorMessage(mutation.error) : null}
        />
      </div>
    </div>
  );
}

export const Route = createFileRoute("/food-items/new")({
  validateSearch: (search: Record<string, unknown>): NewFoodItemSearch => ({
    barcode: typeof search.barcode === "string" ? search.barcode : undefined,
    name: typeof search.name === "string" ? search.name : undefined,
    type: asNumber(search.type) === FoodItemType.Drink ? FoodItemType.Drink : undefined,
    caloriesKcal: asNumber(search.caloriesKcal),
    proteinG: asNumber(search.proteinG),
    carbohydratesG: asNumber(search.carbohydratesG),
    fatG: asNumber(search.fatG),
    fiberG: asNumber(search.fiberG),
  }),
  component: NewFoodItemPage,
});
