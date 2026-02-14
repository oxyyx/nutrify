import { createFileRoute, useNavigate } from "@tanstack/react-router";
import { FoodItemForm } from "@/features/food-items/components/FoodItemForm";
import { useCreateFoodItem } from "@/features/food-items/hooks/useCreateFoodItem";

function NewFoodItemPage() {
  const navigate = useNavigate();
  const mutation = useCreateFoodItem();

  return (
    <div className="mx-auto max-w-2xl space-y-6">
      <h1 className="text-2xl font-bold text-gray-900">Add Food Item</h1>
      <div className="rounded-lg border border-gray-200 bg-white p-6">
        <FoodItemForm
          onSubmit={(data) =>
            mutation.mutate(data, {
              onSuccess: () => navigate({ to: "/food-items" }),
            })
          }
          onCancel={() => navigate({ to: "/food-items" })}
          isSubmitting={mutation.isPending}
        />
      </div>
    </div>
  );
}

export const Route = createFileRoute("/food-items/new")({
  component: NewFoodItemPage,
});
