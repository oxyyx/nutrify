import { apiClient } from "@/shared/lib/api-client";
import type {
  FoodItemDto,
  CreateFoodItemRequest,
  UpdateFoodItemRequest,
  PagedResponse,
  FoodItemType,
} from "@/shared/lib/types";

interface FoodItemsParams {
  page?: number;
  pageSize?: number;
  search?: string;
  categoryId?: number;
  type?: FoodItemType;
}

export function fetchFoodItems(params: FoodItemsParams = {}): Promise<PagedResponse<FoodItemDto>> {
  const searchParams = new URLSearchParams();
  if (params.page) searchParams.set("page", String(params.page));
  if (params.pageSize) searchParams.set("pageSize", String(params.pageSize));
  if (params.search) searchParams.set("search", params.search);
  if (params.categoryId) searchParams.set("categoryId", String(params.categoryId));
  if (params.type !== undefined) searchParams.set("type", String(params.type));

  const qs = searchParams.toString();
  return apiClient.get(`/food-items${qs ? `?${qs}` : ""}`);
}

export function fetchFoodItem(id: number): Promise<FoodItemDto> {
  return apiClient.get(`/food-items/${id}`);
}

export function createFoodItem(data: CreateFoodItemRequest): Promise<FoodItemDto> {
  return apiClient.post("/food-items", data);
}

export function updateFoodItem(id: number, data: UpdateFoodItemRequest): Promise<FoodItemDto> {
  return apiClient.put(`/food-items/${id}`, data);
}

export function deleteFoodItem(id: number): Promise<void> {
  return apiClient.delete(`/food-items/${id}`);
}
