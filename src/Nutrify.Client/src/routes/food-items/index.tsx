import { useState } from "react";
import { Link, createFileRoute } from "@tanstack/react-router";
import { useFoodItems } from "@/features/food-items/hooks/useFoodItems";
import { useDeleteFoodItem } from "@/features/food-items/hooks/useDeleteFoodItem";
import { useCategories } from "@/features/categories/hooks/useCategories";
import { FoodItemList } from "@/features/food-items/components/FoodItemList";
import { Pagination } from "@/shared/components/Pagination";
import { EmptyState } from "@/shared/components/EmptyState";
import { ErrorState } from "@/shared/components/ErrorState";
import { LoadingSpinner } from "@/shared/components/LoadingSpinner";
import { ConfirmDialog } from "@/shared/components/ConfirmDialog";
import { useDebounce } from "@/shared/hooks/useDebounce";
import { getErrorMessage } from "@/shared/lib/utils";
import type { FoodItemType } from "@/shared/lib/types";

function FoodItemsPage() {
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState("");
  const [categoryId, setCategoryId] = useState<number | undefined>();
  const [type, setType] = useState<FoodItemType | undefined>();
  const [deleteId, setDeleteId] = useState<number | null>(null);

  const debouncedSearch = useDebounce(search);
  const { data, isLoading, isError, error, refetch } = useFoodItems({ page, search: debouncedSearch, categoryId, type });
  const { data: categories } = useCategories();
  const deleteMutation = useDeleteFoodItem();

  function openDelete(id: number) {
    deleteMutation.reset();
    setDeleteId(id);
  }

  function cancelDelete() {
    deleteMutation.reset();
    setDeleteId(null);
  }

  function confirmDelete() {
    if (deleteId !== null) {
      deleteMutation.mutate(deleteId, {
        onSuccess: () => setDeleteId(null),
      });
    }
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold text-gray-900">Food Items</h1>
        <div className="flex gap-3">
          <Link
            to="/food-items/scan"
            className="rounded-md bg-gray-100 px-4 py-2 text-sm text-gray-700 hover:bg-gray-200"
          >
            Scan Barcode
          </Link>
          <Link
            to="/food-items/new"
            className="rounded-md bg-primary px-4 py-2 text-sm text-white hover:bg-primary-dark"
          >
            Add Food Item
          </Link>
        </div>
      </div>

      <div className="flex flex-wrap gap-3">
        <input
          type="text"
          value={search}
          onChange={(e) => { setSearch(e.target.value); setPage(1); }}
          placeholder="Search food items..."
          className="rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-primary focus:ring-1 focus:ring-primary focus:outline-none"
        />
        <select
          value={categoryId ?? ""}
          onChange={(e) => { setCategoryId(e.target.value ? Number(e.target.value) : undefined); setPage(1); }}
          className="rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-primary focus:ring-1 focus:ring-primary focus:outline-none"
        >
          <option value="">All categories</option>
          {categories?.map((cat) => (
            <option key={cat.id} value={cat.id}>{cat.name}</option>
          ))}
        </select>
        <select
          value={type ?? ""}
          onChange={(e) => { setType(e.target.value !== "" ? Number(e.target.value) as FoodItemType : undefined); setPage(1); }}
          className="rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-primary focus:ring-1 focus:ring-primary focus:outline-none"
        >
          <option value="">All types</option>
          <option value="0">Food</option>
          <option value="1">Drink</option>
        </select>
      </div>

      {isLoading ? (
        <LoadingSpinner className="py-12" />
      ) : isError ? (
        <ErrorState
          title="Couldn't load food items"
          message={getErrorMessage(error)}
          onRetry={() => refetch()}
        />
      ) : data && data.items.length > 0 ? (
        <>
          <FoodItemList items={data.items} onDelete={openDelete} />
          <Pagination page={data.page} totalPages={data.totalPages} onPageChange={setPage} />
        </>
      ) : (
        <EmptyState
          title="No food items found"
          description="Start by adding your first food item."
        />
      )}

      <ConfirmDialog
        isOpen={deleteId !== null}
        title="Delete Food Item"
        message="Are you sure you want to delete this food item? This action cannot be undone."
        error={deleteMutation.isError ? getErrorMessage(deleteMutation.error) : null}
        isConfirming={deleteMutation.isPending}
        onConfirm={confirmDelete}
        onCancel={cancelDelete}
      />
    </div>
  );
}

export const Route = createFileRoute("/food-items/")({
  component: FoodItemsPage,
});
