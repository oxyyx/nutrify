import { useRef, useState } from "react";
import { Link, createFileRoute, useNavigate } from "@tanstack/react-router";
import { BarcodeScanner } from "@/features/food-items/components/BarcodeScanner";
import { lookupBarcode } from "@/features/food-items/api/food-items.api";
import { ApiError } from "@/shared/lib/api-client";
import { ErrorBanner } from "@/shared/components/ErrorBanner";
import { formatCalories, getErrorMessage } from "@/shared/lib/utils";
import type { FoodItemDto } from "@/shared/lib/types";
import { BarcodeLookupSource, FoodItemType } from "@/shared/lib/types";

function ScanBarcodePage() {
  const navigate = useNavigate();
  const [manualBarcode, setManualBarcode] = useState("");
  const [isLookingUp, setIsLookingUp] = useState(false);
  const [foundItem, setFoundItem] = useState<FoodItemDto | null>(null);
  const [notFoundBarcode, setNotFoundBarcode] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  // The camera fires the same code many times per second; also stay "busy"
  // while a result is on screen so it isn't immediately replaced by a rescan.
  const busyRef = useRef(false);

  function reset() {
    busyRef.current = false;
    setFoundItem(null);
    setNotFoundBarcode(null);
    setError(null);
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
        const p = result.externalProduct;
        navigate({
          to: "/food-items/new",
          search: {
            barcode: p.barcode,
            name: p.name ? (p.brand ? `${p.name} (${p.brand})` : p.name) : undefined,
            type: p.suggestedType === FoodItemType.Drink ? FoodItemType.Drink : undefined,
            caloriesKcal: p.caloriesKcal ?? undefined,
            proteinG: p.proteinG ?? undefined,
            carbohydratesG: p.carbohydratesG ?? undefined,
            fatG: p.fatG ?? undefined,
            fiberG: p.fiberG ?? undefined,
          },
        });
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

  return (
    <div className="mx-auto max-w-2xl space-y-6">
      <h1 className="text-2xl font-bold text-gray-900">Scan Barcode</h1>

      <div className="space-y-4 rounded-xl border border-gray-200 bg-white p-6 shadow-card">
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
            className="flex-1 rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-primary focus:ring-1 focus:ring-primary focus:outline-none"
          />
          <button
            type="submit"
            disabled={isLookingUp}
            className="rounded-md bg-primary px-4 py-2 text-sm text-white hover:bg-primary-dark disabled:opacity-50"
          >
            Search
          </button>
        </form>

        {isLookingUp && <p className="text-sm text-gray-500">Looking up barcode...</p>}
        {error && <ErrorBanner message={error} />}
      </div>

      {foundItem && (
        <div className="space-y-3 rounded-xl border border-gray-200 bg-white p-6 shadow-card">
          <p className="text-sm text-gray-500">Already in your food items:</p>
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
        <div className="space-y-3 rounded-xl border border-gray-200 bg-white p-6 shadow-card">
          <p className="text-sm text-gray-600">
            Barcode <span className="font-medium text-gray-900">{notFoundBarcode}</span> wasn't found in
            your food items or Open Food Facts.
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
  component: ScanBarcodePage,
});
