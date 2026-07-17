import { apiClient } from "@/shared/lib/api-client";
import type {
  IntakeEntryDto,
  CreateIntakeEntryRequest,
  UpdateIntakeEntryRequest,
  PagedResponse,
} from "@/shared/lib/types";

export interface IntakeParams {
  page?: number;
  pageSize?: number;
  date?: string;
  from?: string;
  to?: string;
  search?: string;
}

export function fetchIntakeEntries(params: IntakeParams = {}): Promise<PagedResponse<IntakeEntryDto>> {
  const searchParams = new URLSearchParams();
  if (params.page) searchParams.set("page", String(params.page));
  if (params.pageSize) searchParams.set("pageSize", String(params.pageSize));
  if (params.date) searchParams.set("date", params.date);
  if (params.from) searchParams.set("from", params.from);
  if (params.to) searchParams.set("to", params.to);
  if (params.search) searchParams.set("search", params.search);

  const qs = searchParams.toString();
  return apiClient.get(`/intake${qs ? `?${qs}` : ""}`);
}

export function createIntakeEntry(data: CreateIntakeEntryRequest): Promise<IntakeEntryDto> {
  return apiClient.post("/intake", data);
}

export function updateIntakeEntry(id: number, data: UpdateIntakeEntryRequest): Promise<IntakeEntryDto> {
  return apiClient.put(`/intake/${id}`, data);
}

export function deleteIntakeEntry(id: number): Promise<void> {
  return apiClient.delete(`/intake/${id}`);
}
