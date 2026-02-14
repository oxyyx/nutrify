import { useQuery } from "@tanstack/react-query";
import { fetchCategories } from "../api/categories.api";

export function useCategories(search?: string) {
  return useQuery({
    queryKey: ["categories", { search }],
    queryFn: () => fetchCategories(search),
  });
}
