import type { FoodItemDto } from "./types";

export function formatMacro(value: number, unit = "g"): string {
  return `${value.toFixed(1)}${unit}`;
}

export function formatCalories(value: number): string {
  return `${Math.round(value)} kcal`;
}

export function calculateMacros(
  foodItem: FoodItemDto,
  amount: number,
): {
  calories: number;
  protein: number;
  carbohydrates: number;
  fat: number;
  fiber: number;
} {
  const multiplier = amount / 100;
  return {
    calories: foodItem.caloriesKcal * multiplier,
    protein: foodItem.proteinG * multiplier,
    carbohydrates: foodItem.carbohydratesG * multiplier,
    fat: foodItem.fatG * multiplier,
    fiber: foodItem.fiberG * multiplier,
  };
}

export function formatDate(dateString: string): string {
  return new Date(dateString).toLocaleDateString(undefined, {
    year: "numeric",
    month: "short",
    day: "numeric",
  });
}

export function formatTime(dateString: string): string {
  return new Date(dateString).toLocaleTimeString(undefined, {
    hour: "2-digit",
    minute: "2-digit",
  });
}

/** Local calendar date as YYYY-MM-DD (en-CA yields ISO-style output in local time). */
export function getLocalDateString(date = new Date()): string {
  return date.toLocaleDateString("en-CA");
}

/** Extracts a user-facing message from a thrown value (ApiError carries a real message). */
export function getErrorMessage(
  error: unknown,
  fallback = "Something went wrong. Please try again.",
): string {
  if (error instanceof Error && error.message) return error.message;
  return fallback;
}
