import { useMutation, useQueryClient } from "@tanstack/react-query";
import { createIntakeEntry } from "../api/intake.api";

export function useCreateIntakeEntry() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: createIntakeEntry,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["intake"] });
      queryClient.invalidateQueries({ queryKey: ["dashboard"] });
    },
  });
}
