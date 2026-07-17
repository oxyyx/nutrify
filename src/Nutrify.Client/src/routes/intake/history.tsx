import { useState } from "react";
import { createFileRoute } from "@tanstack/react-router";
import { useIntakeEntries } from "@/features/intake/hooks/useIntakeEntries";
import { useDeleteIntakeEntry } from "@/features/intake/hooks/useDeleteIntakeEntry";
import { IntakeHistoryTable } from "@/features/intake/components/IntakeHistoryTable";
import { Pagination } from "@/shared/components/Pagination";
import { EmptyState } from "@/shared/components/EmptyState";
import { ErrorState } from "@/shared/components/ErrorState";
import { ErrorBanner } from "@/shared/components/ErrorBanner";
import { LoadingSpinner } from "@/shared/components/LoadingSpinner";
import { useDebounce } from "@/shared/hooks/useDebounce";
import { getErrorMessage } from "@/shared/lib/utils";

function IntakeHistoryPage() {
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState("");
  const [from, setFrom] = useState("");
  const [to, setTo] = useState("");

  const debouncedSearch = useDebounce(search);
  const { data, isLoading, isError, error, refetch } = useIntakeEntries({
    page,
    search: debouncedSearch || undefined,
    from: from || undefined,
    to: to || undefined,
  });
  const deleteMutation = useDeleteIntakeEntry();

  const hasFilters = search !== "" || from !== "" || to !== "";

  function clearFilters() {
    setSearch("");
    setFrom("");
    setTo("");
    setPage(1);
  }

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold text-gray-900">Intake History</h1>

      <div className="flex flex-wrap items-center gap-2 sm:gap-3">
        <input
          type="text"
          value={search}
          onChange={(e) => { setSearch(e.target.value); setPage(1); }}
          placeholder="Search by food name..."
          className="w-full rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-primary focus:ring-1 focus:ring-primary focus:outline-none sm:w-64"
        />
        <label className="flex flex-1 items-center gap-2 text-sm text-gray-600 sm:flex-none">
          From
          <input
            type="date"
            value={from}
            onChange={(e) => { setFrom(e.target.value); setPage(1); }}
            className="min-w-0 flex-1 rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-primary focus:ring-1 focus:ring-primary focus:outline-none"
          />
        </label>
        <label className="flex flex-1 items-center gap-2 text-sm text-gray-600 sm:flex-none">
          To
          <input
            type="date"
            value={to}
            onChange={(e) => { setTo(e.target.value); setPage(1); }}
            className="min-w-0 flex-1 rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-primary focus:ring-1 focus:ring-primary focus:outline-none"
          />
        </label>
        {hasFilters && (
          <button
            onClick={clearFilters}
            className="rounded-md px-3 py-2 text-sm text-gray-500 hover:bg-gray-100 hover:text-gray-700"
          >
            Clear
          </button>
        )}
      </div>

      {deleteMutation.isError && (
        <ErrorBanner message={getErrorMessage(deleteMutation.error)} />
      )}

      {isLoading ? (
        <LoadingSpinner className="py-12" />
      ) : isError ? (
        <ErrorState
          title="Couldn't load intake history"
          message={getErrorMessage(error)}
          onRetry={() => refetch()}
        />
      ) : data && data.items.length > 0 ? (
        <>
          <p className="text-sm text-gray-500">
            {data.totalCount} {data.totalCount === 1 ? "entry" : "entries"}
          </p>
          <IntakeHistoryTable
            entries={data.items}
            onDelete={(id) => deleteMutation.mutate(id)}
          />
          <Pagination page={data.page} totalPages={data.totalPages} onPageChange={setPage} />
        </>
      ) : (
        <EmptyState
          title={hasFilters ? "No entries match your filters" : "No intake logged yet"}
          description={
            hasFilters
              ? "Try adjusting the search or date range."
              : "Entries you log will show up here."
          }
        />
      )}
    </div>
  );
}

export const Route = createFileRoute("/intake/history")({
  component: IntakeHistoryPage,
});
