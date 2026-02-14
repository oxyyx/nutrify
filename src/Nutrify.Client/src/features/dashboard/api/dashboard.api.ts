import { apiClient } from "@/shared/lib/api-client";
import type { DailyDashboardDto, WeeklyOverviewDto } from "@/shared/lib/types";

export function fetchTodayDashboard(): Promise<DailyDashboardDto> {
  return apiClient.get("/dashboard/today");
}

export function fetchWeeklyOverview(): Promise<WeeklyOverviewDto> {
  return apiClient.get("/dashboard/weekly");
}
