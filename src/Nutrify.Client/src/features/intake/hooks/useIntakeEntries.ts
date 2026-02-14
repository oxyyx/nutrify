import { useQuery } from "@tanstack/react-query";
import { fetchIntakeEntries } from "../api/intake.api";

interface UseIntakeEntriesParams {
  page?: number;
  date?: string;
}

export function useIntakeEntries(params: UseIntakeEntriesParams = {}) {
  return useQuery({
    queryKey: ["intake", params],
    queryFn: () => fetchIntakeEntries(params),
  });
}
