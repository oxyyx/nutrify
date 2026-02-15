import { useRef, useState } from "react";
import { useCategories } from "@/features/categories/hooks/useCategories";
import { useCreateCategory } from "@/features/categories/hooks/useCreateCategory";

const NEW_SENTINEL = "__new__";

const selectClass =
  "w-full rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-primary focus:ring-1 focus:ring-primary focus:outline-none";

interface CategorySelectProps {
  value: number | null;
  onChange: (id: number | null) => void;
}

export function CategorySelect({ value, onChange }: CategorySelectProps) {
  const [mode, setMode] = useState<"select" | "create">("select");
  const [newName, setNewName] = useState("");
  const inputRef = useRef<HTMLInputElement>(null);

  const { data: categories } = useCategories();
  const createMutation = useCreateCategory();

  function handleSelectChange(e: React.ChangeEvent<HTMLSelectElement>) {
    if (e.target.value === NEW_SENTINEL) {
      setMode("create");
      setNewName("");
      // Focus the input on the next frame after it renders
      setTimeout(() => inputRef.current?.focus(), 0);
    } else {
      onChange(e.target.value ? Number(e.target.value) : null);
    }
  }

  function handleCreate(e: React.MouseEvent) {
    e.preventDefault();
    const trimmed = newName.trim();
    if (!trimmed) return;
    createMutation.mutate(
      { name: trimmed },
      {
        onSuccess: (created) => {
          onChange(created.id);
          setMode("select");
          setNewName("");
        },
      },
    );
  }

  function handleCancel() {
    setMode("select");
    setNewName("");
  }

  function handleKeyDown(e: React.KeyboardEvent<HTMLInputElement>) {
    if (e.key === "Escape") handleCancel();
  }

  if (mode === "create") {
    return (
      <div className="flex gap-2">
        <input
          ref={inputRef}
          autoFocus
          type="text"
          value={newName}
          onChange={(e) => setNewName(e.target.value)}
          onKeyDown={handleKeyDown}
          placeholder="Category name"
          className="flex-1 rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-primary focus:ring-1 focus:ring-primary focus:outline-none"
        />
        <button
          type="button"
          onClick={handleCreate}
          disabled={!newName.trim() || createMutation.isPending}
          className="rounded-md bg-primary px-3 py-2 text-sm font-medium text-white transition-colors hover:bg-primary-dark disabled:opacity-50"
        >
          {createMutation.isPending ? "…" : "Create"}
        </button>
        <button
          type="button"
          onClick={handleCancel}
          className="rounded-md bg-gray-100 px-3 py-2 text-sm text-gray-700 transition-colors hover:bg-gray-200"
        >
          ✕
        </button>
      </div>
    );
  }

  return (
    <select value={value ?? ""} onChange={handleSelectChange} className={selectClass}>
      <option value="">No category</option>
      {categories?.map((cat) => (
        <option key={cat.id} value={cat.id}>
          {cat.name}
        </option>
      ))}
      <option value={NEW_SENTINEL}>＋ New category…</option>
    </select>
  );
}
