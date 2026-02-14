import { Link } from "@tanstack/react-router";
import type { FoodItemDto } from "@/shared/lib/types";
import { FoodItemType } from "@/shared/lib/types";
import { formatCalories } from "@/shared/lib/utils";

interface FoodItemListProps {
  items: FoodItemDto[];
  onDelete: (id: number) => void;
}

export function FoodItemList({ items, onDelete }: FoodItemListProps) {
  return (
    <div className="overflow-hidden rounded-lg border border-gray-200 bg-white">
      <table className="w-full text-left text-sm">
        <thead className="border-b border-gray-200 bg-gray-50">
          <tr>
            <th className="px-4 py-3 font-medium text-gray-700">Name</th>
            <th className="px-4 py-3 font-medium text-gray-700">Type</th>
            <th className="px-4 py-3 font-medium text-gray-700">Category</th>
            <th className="px-4 py-3 font-medium text-gray-700">Calories</th>
            <th className="px-4 py-3 font-medium text-gray-700">P / C / F / Fiber</th>
            <th className="px-4 py-3 font-medium text-gray-700">Actions</th>
          </tr>
        </thead>
        <tbody className="divide-y divide-gray-100">
          {items.map((item) => (
            <tr key={item.id} className="hover:bg-gray-50">
              <td className="px-4 py-3 font-medium text-gray-900">{item.name}</td>
              <td className="px-4 py-3 text-gray-600">
                {item.type === FoodItemType.Food ? "Food" : "Drink"} ({item.unit})
              </td>
              <td className="px-4 py-3">
                {item.categoryName ? (
                  <span className="rounded-full bg-gray-100 px-2 py-0.5 text-xs text-gray-700">
                    {item.categoryName}
                  </span>
                ) : (
                  <span className="text-gray-400">-</span>
                )}
              </td>
              <td className="px-4 py-3 text-gray-600">{formatCalories(item.caloriesKcal)}</td>
              <td className="px-4 py-3 text-xs text-gray-500">
                {item.proteinG.toFixed(1)}g / {item.carbohydratesG.toFixed(1)}g / {item.fatG.toFixed(1)}g / {item.fiberG.toFixed(1)}g
              </td>
              <td className="px-4 py-3">
                <div className="flex gap-2">
                  <Link
                    to="/food-items/$foodItemId/edit"
                    params={{ foodItemId: String(item.id) }}
                    className="text-primary hover:text-primary-dark text-sm"
                  >
                    Edit
                  </Link>
                  <button
                    onClick={() => onDelete(item.id)}
                    className="text-sm text-red-600 hover:text-red-800"
                  >
                    Delete
                  </button>
                </div>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
