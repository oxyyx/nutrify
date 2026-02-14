import { useState } from "react";
import type { FoodItemDto } from "@/shared/lib/types";
import { FoodItemType } from "@/shared/lib/types";
import { useFoodItems } from "@/features/food-items/hooks/useFoodItems";
import { useDebounce } from "@/shared/hooks/useDebounce";

interface FoodSearchSelectProps {
  selected: FoodItemDto | null;
  onSelect: (item: FoodItemDto | null) => void;
}

export function FoodSearchSelect({ selected, onSelect }: FoodSearchSelectProps) {
  const [search, setSearch] = useState("");
  const [isOpen, setIsOpen] = useState(false);
  const debouncedSearch = useDebounce(search);

  const { data } = useFoodItems({ search: debouncedSearch, pageSize: 10 });

  if (selected) {
    return (
      <div className="flex items-center justify-between rounded-md border border-gray-300 px-3 py-2">
        <div>
          <span className="font-medium text-gray-900">{selected.name}</span>
          <span className="ml-2 text-sm text-gray-500">
            ({selected.type === FoodItemType.Food ? "Food" : "Drink"})
          </span>
        </div>
        <button
          type="button"
          onClick={() => onSelect(null)}
          className="text-sm text-gray-500 hover:text-gray-700"
        >
          Change
        </button>
      </div>
    );
  }

  return (
    <div className="relative">
      <input
        type="text"
        value={search}
        onChange={(e) => {
          setSearch(e.target.value);
          setIsOpen(true);
        }}
        onFocus={() => setIsOpen(true)}
        placeholder="Search food items..."
        className="w-full rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-primary focus:ring-1 focus:ring-primary focus:outline-none"
      />
      {isOpen && data && data.items.length > 0 && (
        <div className="absolute z-10 mt-1 max-h-60 w-full overflow-auto rounded-md border border-gray-200 bg-white shadow-lg">
          {data.items.map((item) => (
            <button
              key={item.id}
              type="button"
              onClick={() => {
                onSelect(item);
                setIsOpen(false);
                setSearch("");
              }}
              className="flex w-full items-center justify-between px-3 py-2 text-left text-sm hover:bg-gray-50"
            >
              <div>
                <span className="font-medium">{item.name}</span>
                {item.categoryName && (
                  <span className="ml-2 text-xs text-gray-500">{item.categoryName}</span>
                )}
              </div>
              <span className="text-xs text-gray-400">
                {Math.round(item.caloriesKcal)} kcal/100{item.unit}
              </span>
            </button>
          ))}
        </div>
      )}
    </div>
  );
}
