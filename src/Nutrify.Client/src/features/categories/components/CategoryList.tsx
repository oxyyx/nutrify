import { useState } from "react";
import type { CategoryDto } from "@/shared/lib/types";
import { useUpdateCategory } from "../hooks/useUpdateCategory";
import { useDeleteCategory } from "../hooks/useDeleteCategory";
import { ConfirmDialog } from "@/shared/components/ConfirmDialog";
import { ErrorBanner } from "@/shared/components/ErrorBanner";
import { getErrorMessage } from "@/shared/lib/utils";

interface CategoryListProps {
  categories: CategoryDto[];
}

export function CategoryList({ categories }: CategoryListProps) {
  const [editingId, setEditingId] = useState<number | null>(null);
  const [editName, setEditName] = useState("");
  const [deleteId, setDeleteId] = useState<number | null>(null);

  const updateMutation = useUpdateCategory();
  const deleteMutation = useDeleteCategory();

  function startEdit(category: CategoryDto) {
    updateMutation.reset();
    setEditingId(category.id);
    setEditName(category.name);
  }

  function cancelEdit() {
    updateMutation.reset();
    setEditingId(null);
    setEditName("");
  }

  function saveEdit(id: number) {
    updateMutation.mutate(
      { id, data: { name: editName } },
      { onSuccess: () => cancelEdit() },
    );
  }

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
    <>
      <div className="space-y-2">
        {categories.map((category) => (
          <div
            key={category.id}
            className="flex items-center justify-between rounded-md border border-gray-200 bg-white p-3"
          >
            {editingId === category.id ? (
              <div className="flex flex-1 flex-col gap-2">
                <div className="flex flex-1 items-center gap-2">
                  <input
                    type="text"
                    value={editName}
                    onChange={(e) => setEditName(e.target.value)}
                    className="flex-1 rounded-md border border-gray-300 px-3 py-1.5 text-sm focus:border-primary focus:ring-1 focus:ring-primary focus:outline-none"
                    autoFocus
                    onKeyDown={(e) => {
                      if (e.key === "Enter") saveEdit(category.id);
                      if (e.key === "Escape") cancelEdit();
                    }}
                  />
                  <button
                    onClick={() => saveEdit(category.id)}
                    disabled={!editName.trim() || updateMutation.isPending}
                    className="rounded bg-primary px-3 py-1.5 text-sm text-white hover:bg-primary-dark disabled:opacity-50"
                  >
                    Save
                  </button>
                  <button
                    onClick={cancelEdit}
                    className="rounded bg-gray-100 px-3 py-1.5 text-sm text-gray-700 hover:bg-gray-200"
                  >
                    Cancel
                  </button>
                </div>
                {updateMutation.isError && (
                  <ErrorBanner message={getErrorMessage(updateMutation.error)} />
                )}
              </div>
            ) : (
              <>
                <div>
                  <span className="font-medium text-gray-900">{category.name}</span>
                  <span className="ml-2 text-sm text-gray-500">
                    ({category.foodItemCount} items)
                  </span>
                </div>
                <div className="flex gap-2">
                  <button
                    onClick={() => startEdit(category)}
                    className="text-sm text-primary hover:text-primary-dark"
                  >
                    Edit
                  </button>
                  <button
                    onClick={() => openDelete(category.id)}
                    className="text-sm text-red-600 hover:text-red-800"
                  >
                    Delete
                  </button>
                </div>
              </>
            )}
          </div>
        ))}
      </div>

      <ConfirmDialog
        isOpen={deleteId !== null}
        title="Delete Category"
        message="Are you sure you want to delete this category? This action cannot be undone."
        error={deleteMutation.isError ? getErrorMessage(deleteMutation.error) : null}
        isConfirming={deleteMutation.isPending}
        onConfirm={confirmDelete}
        onCancel={cancelDelete}
      />
    </>
  );
}
