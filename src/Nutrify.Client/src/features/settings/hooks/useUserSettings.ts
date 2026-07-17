import { useQuery } from "@tanstack/react-query";
import { fetchUserSettings } from "../api/settings.api";

export function useUserSettings() {
  return useQuery({
    queryKey: ["settings"],
    queryFn: fetchUserSettings,
  });
}
