import { useQuery } from "@tanstack/react-query";
import { fetchFoodItems } from "../api/food-items.api";
import type { FoodItemType } from "@/shared/lib/types";

interface UseFoodItemsParams {
  page?: number;
  pageSize?: number;
  search?: string;
  categoryId?: number;
  type?: FoodItemType;
}

export function useFoodItems(params: UseFoodItemsParams = {}) {
  return useQuery({
    queryKey: ["food-items", params],
    queryFn: () => fetchFoodItems(params),
  });
}
