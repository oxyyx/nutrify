import { useMutation, useQueryClient } from "@tanstack/react-query";
import { deleteFoodItem } from "../api/food-items.api";

export function useDeleteFoodItem() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: deleteFoodItem,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["food-items"] });
    },
  });
}
