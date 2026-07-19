import { useRef, useState } from "react";
import { Link, createFileRoute, useNavigate } from "@tanstack/react-router";
import { BarcodeScanner } from "@/features/food-items/components/BarcodeScanner";
import { lookupBarcode } from "@/features/food-items/api/food-items.api";
import { useProductSearch } from "@/features/food-items/hooks/useProductSearch";
import { ApiError } from "@/shared/lib/api-client";
import { ErrorBanner } from "@/shared/components/ErrorBanner";
import { formatCalories, getErrorMessage } from "@/shared/lib/utils";
import type { ExternalProductDto, FoodItemDto } from "@/shared/lib/types";
import { BarcodeLookupSource, FoodItemType } from "@/shared/lib/types";

function FindFoodPage() {
  const navigate = useNavigate();
  const [manualBarcode, setManualBarcode] = useState("");
  const [isLookingUp, setIsLookingUp] = useState(false);
  const [foundItem, setFoundItem] = useState<FoodItemDto | null>(null);
  const [notFoundBarcode, setNotFoundBarcode] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  // The description search runs on submit, not on every keystroke, so the
  // provider isn't hit once per letter typed.
  const [descriptionInput, setDescriptionInput] = useState("");
  const [searchTerm, setSearchTerm] = useState("");
  const search = useProductSearch(searchTerm);

  // The camera fires the same code many times per second; also stay "busy"
  // while a result is on screen so it isn't immediately replaced by a rescan.
  const busyRef = useRef(false);

  function reset() {
    busyRef.current = false;
    setFoundItem(null);
    setNotFoundBarcode(null);
    setError(null);
  }

  // Providers often omit some nutriments (fiber especially); default them to 0
  // so the user doesn't have to fill every required field.
  function createFromProduct(p: ExternalProductDto) {
    navigate({
      to: "/food-items/new",
      search: {
        barcode: p.barcode,
        name: p.name ? (p.brand ? `${p.name} (${p.brand})` : p.name) : undefined,
        type: p.suggestedType === FoodItemType.Drink ? FoodItemType.Drink : undefined,
        caloriesKcal: p.caloriesKcal ?? 0,
        proteinG: p.proteinG ?? 0,
        carbohydratesG: p.carbohydratesG ?? 0,
        fatG: p.fatG ?? 0,
        fiberG: p.fiberG ?? 0,
        servingSize: p.servingSize ?? undefined,
      },
    });
  }

  async function handleBarcode(code: string) {
    const barcode = code.trim();
    if (!barcode || busyRef.current) return;
    busyRef.current = true;

    setError(null);
    setFoundItem(null);
    setNotFoundBarcode(null);
    setIsLookingUp(true);

    try {
      const result = await lookupBarcode(barcode);

      if (result.source === BarcodeLookupSource.Internal && result.existingItem) {
        setFoundItem(result.existingItem);
        return;
      }

      if (result.externalProduct) {
        createFromProduct(result.externalProduct);
      }
    } catch (err) {
      if (err instanceof ApiError && err.status === 404) {
        setNotFoundBarcode(barcode);
      } else {
        setError(getErrorMessage(err));
        busyRef.current = false;
      }
    } finally {
      setIsLookingUp(false);
    }
  }

  const results = search.data;
  const hasResults = !!results && (results.existingItems.length > 0 || results.externalProducts.length > 0);

  return (
    <div className="mx-auto max-w-2xl space-y-6">
      <h1 className="text-2xl font-bold text-gray-900">Find Food</h1>

      <div className="space-y-4 rounded-xl border border-gray-200 bg-white p-4 shadow-card sm:p-6">
        <BarcodeScanner onDetected={handleBarcode} />

        <form
          onSubmit={(e) => {
            e.preventDefault();
            reset();
            handleBarcode(manualBarcode);
          }}
          className="flex gap-3"
        >
          <input
            type="text"
            inputMode="numeric"
            pattern="[0-9]{6,32}"
            title="6 to 32 digits"
            value={manualBarcode}
            onChange={(e) => setManualBarcode(e.target.value)}
            placeholder="Or enter a barcode manually..."
            required
            className="min-w-0 flex-1 rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-primary focus:ring-1 focus:ring-primary focus:outline-none"
          />
          <button
            type="submit"
            disabled={isLookingUp}
            className="shrink-0 rounded-md bg-primary px-4 py-2 text-sm text-white hover:bg-primary-dark disabled:opacity-50"
          >
            Search
          </button>
        </form>

        {isLookingUp && <p className="text-sm text-gray-500">Looking up barcode...</p>}
        {error && <ErrorBanner message={error} />}
      </div>

      <div className="space-y-4 rounded-xl border border-gray-200 bg-white p-4 shadow-card sm:p-6">
        <h2 className="text-lg font-semibold text-gray-900">Search by description</h2>

        <form
          onSubmit={(e) => {
            e.preventDefault();
            setSearchTerm(descriptionInput.trim());
          }}
          className="flex gap-3"
        >
          <input
            type="search"
            value={descriptionInput}
            onChange={(e) => setDescriptionInput(e.target.value)}
            placeholder="e.g. greek yoghurt"
            minLength={2}
            required
            className="min-w-0 flex-1 rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-primary focus:ring-1 focus:ring-primary focus:outline-none"
          />
          <button
            type="submit"
            disabled={search.isFetching}
            className="shrink-0 rounded-md bg-primary px-4 py-2 text-sm text-white hover:bg-primary-dark disabled:opacity-50"
          >
            Search
          </button>
        </form>

        {search.isFetching && <p className="text-sm text-gray-500">Searching...</p>}
        {search.isError && <ErrorBanner message={getErrorMessage(search.error)} />}

        {!search.isFetching && results && !hasResults && (
          <p className="text-sm text-gray-600">
            Nothing matched "{searchTerm}" in your foods or Open Food Facts.{" "}
            <Link to="/food-items/new" className="font-medium text-primary hover:underline">
              Create it manually
            </Link>
            .
          </p>
        )}

        {results && results.existingItems.length > 0 && (
          <div className="space-y-2">
            <p className="text-sm font-medium text-gray-500">In your foods</p>
            <ul className="divide-y divide-gray-100">
              {results.existingItems.map((item) => (
                <li key={item.id} className="flex items-center justify-between gap-3 py-3">
                  <div className="min-w-0">
                    <p className="truncate font-medium text-gray-900">{item.name}</p>
                    <p className="text-sm text-gray-600">
                      {formatCalories(item.caloriesKcal)} · {item.proteinG.toFixed(1)}g protein ·{" "}
                      {item.carbohydratesG.toFixed(1)}g carbs · {item.fatG.toFixed(1)}g fat per 100
                      {item.unit}
                    </p>
                  </div>
                  <Link
                    to="/intake/new"
                    className="shrink-0 rounded-md bg-gray-100 px-3 py-1.5 text-sm text-gray-700 hover:bg-gray-200"
                  >
                    Log
                  </Link>
                </li>
              ))}
            </ul>
          </div>
        )}

        {results && results.externalProducts.length > 0 && (
          <div className="space-y-2">
            <p className="text-sm font-medium text-gray-500">From Open Food Facts</p>
            <ul className="divide-y divide-gray-100">
              {results.externalProducts.map((product) => (
                <li key={product.barcode}>
                  <button
                    type="button"
                    onClick={() => createFromProduct(product)}
                    className="flex w-full items-center justify-between gap-3 py-3 text-left hover:bg-gray-50"
                  >
                    <div className="min-w-0">
                      <p className="truncate font-medium text-gray-900">{product.name}</p>
                      <p className="truncate text-sm text-gray-600">
                        {product.brand ? `${product.brand} · ` : ""}
                        {product.caloriesKcal !== null
                          ? `${formatCalories(product.caloriesKcal)} per 100${
                              product.suggestedType === FoodItemType.Drink ? "mL" : "g"
                            }`
                          : "No nutrition data"}
                      </p>
                    </div>
                    <span className="shrink-0 text-sm font-medium text-primary">Add</span>
                  </button>
                </li>
              ))}
            </ul>
          </div>
        )}
      </div>

      {foundItem && (
        <div className="space-y-3 rounded-xl border border-gray-200 bg-white p-4 shadow-card sm:p-6">
          <p className="text-sm text-gray-500">Already in your foods:</p>
          <div>
            <p className="font-medium text-gray-900">{foundItem.name}</p>
            <p className="text-sm text-gray-600">
              {formatCalories(foundItem.caloriesKcal)} · {foundItem.proteinG.toFixed(1)}g protein ·{" "}
              {foundItem.carbohydratesG.toFixed(1)}g carbs · {foundItem.fatG.toFixed(1)}g fat per 100
              {foundItem.unit}
            </p>
          </div>
          <div className="flex flex-wrap gap-3 pt-1">
            <Link
              to="/intake/new"
              className="rounded-md bg-primary px-4 py-2 text-sm text-white hover:bg-primary-dark"
            >
              Log Intake
            </Link>
            <Link
              to="/food-items/$foodItemId/edit"
              params={{ foodItemId: String(foundItem.id) }}
              className="rounded-md bg-gray-100 px-4 py-2 text-sm text-gray-700 hover:bg-gray-200"
            >
              Edit Item
            </Link>
            <button
              onClick={reset}
              className="rounded-md bg-gray-100 px-4 py-2 text-sm text-gray-700 hover:bg-gray-200"
            >
              Scan Again
            </button>
          </div>
        </div>
      )}

      {notFoundBarcode && (
        <div className="space-y-3 rounded-xl border border-gray-200 bg-white p-4 shadow-card sm:p-6">
          <p className="text-sm text-gray-600">
            Barcode <span className="font-medium text-gray-900">{notFoundBarcode}</span> wasn't found in
            your foods or Open Food Facts. Try searching by description instead.
          </p>
          <div className="flex flex-wrap gap-3 pt-1">
            <Link
              to="/food-items/new"
              search={{ barcode: notFoundBarcode }}
              className="rounded-md bg-primary px-4 py-2 text-sm text-white hover:bg-primary-dark"
            >
              Create Manually
            </Link>
            <button
              onClick={reset}
              className="rounded-md bg-gray-100 px-4 py-2 text-sm text-gray-700 hover:bg-gray-200"
            >
              Scan Again
            </button>
          </div>
        </div>
      )}
    </div>
  );
}

export const Route = createFileRoute("/food-items/scan")({
  component: FindFoodPage,
});
