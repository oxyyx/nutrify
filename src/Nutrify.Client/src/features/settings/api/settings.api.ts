import { apiClient } from "@/shared/lib/api-client";
import type { UserSettingsDto, UpdateUserSettingsRequest } from "@/shared/lib/types";

export function fetchUserSettings(): Promise<UserSettingsDto> {
  return apiClient.get("/settings");
}

export function updateUserSettings(data: UpdateUserSettingsRequest): Promise<UserSettingsDto> {
  return apiClient.put("/settings", data);
}
