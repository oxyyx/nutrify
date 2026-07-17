import { useMutation, useQueryClient } from "@tanstack/react-query";
import type { UserSettingsDto } from "@/shared/lib/types";
import { updateUserSettings } from "../api/settings.api";

export function useUpdateUserSettings() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: updateUserSettings,
    onSuccess: (data) => {
      // Prime the cache so the settings page and the dashboard (which reads the
      // same ["settings"] query for target progress) reflect the change at once.
      queryClient.setQueryData<UserSettingsDto>(["settings"], data);
    },
  });
}
