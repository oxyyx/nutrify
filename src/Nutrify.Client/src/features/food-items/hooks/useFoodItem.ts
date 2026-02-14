import { useQuery } from "@tanstack/react-query";
import { fetchFoodItem } from "../api/food-items.api";

export function useFoodItem(id: number) {
  return useQuery({
    queryKey: ["food-items", id],
    queryFn: () => fetchFoodItem(id),
    enabled: id > 0,
  });
}
