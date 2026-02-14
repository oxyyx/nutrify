import { useQuery } from "@tanstack/react-query";
import { fetchWeeklyOverview } from "../api/dashboard.api";

export function useWeeklyOverview() {
  return useQuery({
    queryKey: ["dashboard", "weekly"],
    queryFn: fetchWeeklyOverview,
  });
}
