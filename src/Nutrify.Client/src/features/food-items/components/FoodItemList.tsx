import { Link } from "@tanstack/react-router";
import type { FoodItemDto } from "@/shared/lib/types";
import { FoodItemType } from "@/shared/lib/types";
import { formatCalories } from "@/shared/lib/utils";

interface FoodItemListProps {
  items: FoodItemDto[];
  onDelete: (id: number) => void;
}

function CategoryChip({ name }: { name: string | null }) {
  if (!name) return <span className="text-gray-400">-</span>;
  return (
    <span className="rounded-full bg-gray-100 px-2 py-0.5 text-xs text-gray-700">{name}</span>
  );
}

export function FoodItemList({ items, onDelete }: FoodItemListProps) {
  return (
    <>
      {/* Mobile: card list */}
      <div className="space-y-2 md:hidden">
        {items.map((item) => (
          <div
            key={item.id}
            className="rounded-xl border border-gray-200 bg-white p-4 shadow-card"
          >
            <div className="flex items-start justify-between gap-3">
              <div className="min-w-0">
                <p className="truncate font-medium text-gray-900">{item.name}</p>
                <p className="mt-0.5 text-xs text-gray-500">
                  {item.type === FoodItemType.Food ? "Food" : "Drink"} · per 100{item.unit}
                </p>
              </div>
              <CategoryChip name={item.categoryName} />
            </div>
            <p className="mt-2 text-sm text-gray-600">
              <span className="font-medium text-orange-600">{formatCalories(item.caloriesKcal)}</span>
              <span className="ml-2 text-xs text-gray-500">
                P {item.proteinG.toFixed(1)}g · C {item.carbohydratesG.toFixed(1)}g · F{" "}
                {item.fatG.toFixed(1)}g · Fiber {item.fiberG.toFixed(1)}g
              </span>
            </p>
            <div className="mt-3 flex gap-2 border-t border-gray-100 pt-3">
              <Link
                to="/food-items/$foodItemId/edit"
                params={{ foodItemId: String(item.id) }}
                className="flex-1 rounded-md bg-gray-100 py-2 text-center text-sm text-gray-700 active:bg-gray-200"
              >
                Edit
              </Link>
              <button
                onClick={() => onDelete(item.id)}
                className="flex-1 rounded-md bg-red-50 py-2 text-sm text-red-600 active:bg-red-100"
              >
                Delete
              </button>
            </div>
          </div>
        ))}
      </div>

      {/* Desktop: table */}
      <div className="hidden overflow-hidden rounded-xl border border-gray-200 bg-white shadow-card md:block">
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
                  <CategoryChip name={item.categoryName} />
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
    </>
  );
}
