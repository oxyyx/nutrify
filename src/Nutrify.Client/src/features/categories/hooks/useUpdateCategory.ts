import { useMutation, useQueryClient } from "@tanstack/react-query";
import { updateCategory } from "../api/categories.api";
import type { UpdateCategoryRequest } from "@/shared/lib/types";

export function useUpdateCategory() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, data }: { id: number; data: UpdateCategoryRequest }) =>
      updateCategory(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["categories"] });
    },
  });
}
