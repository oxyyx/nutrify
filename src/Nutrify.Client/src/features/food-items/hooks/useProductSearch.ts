import { useQuery } from "@tanstack/react-query";
import { searchProducts } from "../api/food-items.api";

/**
 * Searches own foods + Open Food Facts. Pass an empty term to stay idle;
 * the API rejects terms shorter than two characters.
 */
export function useProductSearch(term: string) {
  return useQuery({
    queryKey: ["product-search", term],
    queryFn: () => searchProducts(term),
    enabled: term.length >= 2,
    // External results change rarely and the provider is rate-limited.
    staleTime: 5 * 60 * 1000,
  });
}
