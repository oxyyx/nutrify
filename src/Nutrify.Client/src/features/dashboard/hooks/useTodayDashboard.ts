import { useQuery } from "@tanstack/react-query";
import { fetchTodayDashboard } from "../api/dashboard.api";

export function useTodayDashboard() {
  return useQuery({
    queryKey: ["dashboard", "today"],
    queryFn: fetchTodayDashboard,
  });
}
