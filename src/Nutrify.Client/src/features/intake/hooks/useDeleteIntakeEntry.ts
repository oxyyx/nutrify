import { useMutation, useQueryClient } from "@tanstack/react-query";
import { deleteIntakeEntry } from "../api/intake.api";

export function useDeleteIntakeEntry() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: deleteIntakeEntry,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["intake"] });
      queryClient.invalidateQueries({ queryKey: ["dashboard"] });
    },
  });
}
