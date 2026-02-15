import { createFileRoute, useNavigate } from "@tanstack/react-router";
import { FoodItemForm } from "@/features/food-items/components/FoodItemForm";
import { useFoodItem } from "@/features/food-items/hooks/useFoodItem";
import { useUpdateFoodItem } from "@/features/food-items/hooks/useUpdateFoodItem";
import { LoadingSpinner } from "@/shared/components/LoadingSpinner";

function EditFoodItemPage() {
  const { foodItemId } = Route.useParams();
  const navigate = useNavigate();
  const { data: foodItem, isLoading } = useFoodItem(Number(foodItemId));
  const mutation = useUpdateFoodItem();

  if (isLoading) {
    return <LoadingSpinner className="py-12" />;
  }

  if (!foodItem) {
    return (
      <div className="py-12 text-center text-gray-500">Food item not found.</div>
    );
  }

  return (
    <div className="mx-auto max-w-2xl space-y-6">
      <h1 className="text-2xl font-bold text-gray-900">Edit Food Item</h1>
      <div className="rounded-xl border border-gray-200 bg-white p-6 shadow-card">
        <FoodItemForm
          initialData={foodItem}
          onSubmit={(data) =>
            mutation.mutate(
              { id: Number(foodItemId), data },
              { onSuccess: () => navigate({ to: "/food-items" }) },
            )
          }
          onCancel={() => navigate({ to: "/food-items" })}
          isSubmitting={mutation.isPending}
        />
      </div>
    </div>
  );
}

export const Route = createFileRoute("/food-items/$foodItemId/edit")({
  component: EditFoodItemPage,
});
