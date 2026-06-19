import { useState } from "react";
import { createFileRoute } from "@tanstack/react-router";
import { useCategories } from "@/features/categories/hooks/useCategories";
import { useCreateCategory } from "@/features/categories/hooks/useCreateCategory";
import { CategoryList } from "@/features/categories/components/CategoryList";
import { EmptyState } from "@/shared/components/EmptyState";
import { ErrorState } from "@/shared/components/ErrorState";
import { ErrorBanner } from "@/shared/components/ErrorBanner";
import { LoadingSpinner } from "@/shared/components/LoadingSpinner";
import { getErrorMessage } from "@/shared/lib/utils";

function CategoriesPage() {
  const [newName, setNewName] = useState("");
  const { data: categories, isLoading, isError, error, refetch } = useCategories();
  const createMutation = useCreateCategory();

  function handleCreate(e: React.FormEvent) {
    e.preventDefault();
    if (!newName.trim()) return;
    createMutation.mutate(
      { name: newName.trim() },
      { onSuccess: () => setNewName("") },
    );
  }

  return (
    <div className="mx-auto max-w-2xl space-y-6">
      <h1 className="text-2xl font-bold text-gray-900">Categories</h1>

      <form onSubmit={handleCreate} className="flex gap-3">
        <input
          type="text"
          value={newName}
          onChange={(e) => setNewName(e.target.value)}
          placeholder="New category name..."
          className="flex-1 rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-primary focus:ring-1 focus:ring-primary focus:outline-none"
        />
        <button
          type="submit"
          disabled={!newName.trim() || createMutation.isPending}
          className="rounded-md bg-primary px-4 py-2 text-sm text-white hover:bg-primary-dark disabled:opacity-50"
        >
          Add
        </button>
      </form>

      {createMutation.isError && (
        <ErrorBanner message={getErrorMessage(createMutation.error)} />
      )}

      {isLoading ? (
        <LoadingSpinner className="py-12" />
      ) : isError ? (
        <ErrorState
          title="Couldn't load categories"
          message={getErrorMessage(error)}
          onRetry={() => refetch()}
        />
      ) : categories && categories.length > 0 ? (
        <CategoryList categories={categories} />
      ) : (
        <EmptyState
          title="No categories yet"
          description="Create your first category above."
        />
      )}
    </div>
  );
}

export const Route = createFileRoute("/categories/")({
  component: CategoriesPage,
});
