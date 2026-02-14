import { useMutation, useQueryClient } from "@tanstack/react-query";
import { updateFoodItem } from "../api/food-items.api";
import type { UpdateFoodItemRequest } from "@/shared/lib/types";

export function useUpdateFoodItem() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, data }: { id: number; data: UpdateFoodItemRequest }) =>
      updateFoodItem(id, data),
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: ["food-items"] });
      queryClient.invalidateQueries({ queryKey: ["food-items", variables.id] });
    },
  });
}
