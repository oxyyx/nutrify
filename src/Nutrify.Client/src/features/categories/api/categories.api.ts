import { apiClient } from "@/shared/lib/api-client";
import type {
  CategoryDto,
  CreateCategoryRequest,
  UpdateCategoryRequest,
} from "@/shared/lib/types";

export function fetchCategories(search?: string): Promise<CategoryDto[]> {
  const params = search ? `?search=${encodeURIComponent(search)}` : "";
  return apiClient.get(`/categories${params}`);
}

export function createCategory(data: CreateCategoryRequest): Promise<CategoryDto> {
  return apiClient.post("/categories", data);
}

export function updateCategory(id: number, data: UpdateCategoryRequest): Promise<CategoryDto> {
  return apiClient.put(`/categories/${id}`, data);
}

export function deleteCategory(id: number): Promise<void> {
  return apiClient.delete(`/categories/${id}`);
}
