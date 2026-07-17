import { keepPreviousData, useQuery } from "@tanstack/react-query";
import { fetchIntakeEntries, type IntakeParams } from "../api/intake.api";

export function useIntakeEntries(params: IntakeParams = {}) {
  return useQuery({
    queryKey: ["intake", params],
    queryFn: () => fetchIntakeEntries(params),
    placeholderData: keepPreviousData,
  });
}
