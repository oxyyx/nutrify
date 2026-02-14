// Mirrors Nutrify.Contracts - keep in sync with the API

export enum FoodItemType {
  Food = 0,
  Drink = 1,
}

// --- Common ---

export interface PagedResponse<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

// --- Categories ---

export interface CategoryDto {
  id: number;
  name: string;
  foodItemCount: number;
}

export interface CreateCategoryRequest {
  name: string;
}

export interface UpdateCategoryRequest {
  name: string;
}

// --- Food Items ---

export interface FoodItemDto {
  id: number;
  name: string;
  type: FoodItemType;
  unit: string;
  caloriesKcal: number;
  proteinG: number;
  carbohydratesG: number;
  fatG: number;
  fiberG: number;
  categoryId: number | null;
  categoryName: string | null;
  createdAt: string;
}

export interface CreateFoodItemRequest {
  name: string;
  type: FoodItemType;
  caloriesKcal: number;
  proteinG: number;
  carbohydratesG: number;
  fatG: number;
  fiberG: number;
  categoryId: number | null;
}

export interface UpdateFoodItemRequest {
  name: string;
  type: FoodItemType;
  caloriesKcal: number;
  proteinG: number;
  carbohydratesG: number;
  fatG: number;
  fiberG: number;
  categoryId: number | null;
}

// --- Intake ---

export interface IntakeEntryDto {
  id: number;
  foodItemId: number;
  foodItemName: string;
  foodItemUnit: string;
  amount: number;
  caloriesKcal: number;
  proteinG: number;
  carbohydratesG: number;
  fatG: number;
  fiberG: number;
  consumedAt: string;
}

export interface CreateIntakeEntryRequest {
  foodItemId: number;
  amount: number;
  consumedAt?: string;
}

export interface UpdateIntakeEntryRequest {
  amount: number;
  consumedAt: string;
}

// --- Dashboard ---

export interface MacroSummaryDto {
  totalCalories: number;
  totalProteinG: number;
  totalCarbohydratesG: number;
  totalFatG: number;
  totalFiberG: number;
}

export interface DailyDashboardDto {
  date: string;
  summary: MacroSummaryDto;
  entries: IntakeEntryDto[];
}

export interface DailyMacroDto {
  date: string;
  summary: MacroSummaryDto;
  entryCount: number;
}

export interface WeeklyOverviewDto {
  startDate: string;
  endDate: string;
  days: DailyMacroDto[];
}
