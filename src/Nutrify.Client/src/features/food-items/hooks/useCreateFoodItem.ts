import { useMutation, useQueryClient } from "@tanstack/react-query";
import { createFoodItem } from "../api/food-items.api";

export function useCreateFoodItem() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: createFoodItem,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["food-items"] });
    },
  });
}
