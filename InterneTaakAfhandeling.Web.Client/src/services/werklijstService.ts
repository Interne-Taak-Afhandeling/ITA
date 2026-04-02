import { get } from "@/utils/fetchWrapper";
import type { PaginationResponse } from "@/composables/use-pagination";
import type { WerklijstOverzichtItem } from "@/types/werklijst";

export const werklijstService = {
  fetchWerklijst: (
    page: number,
    pageSize: number
  ): Promise<PaginationResponse<WerklijstOverzichtItem>> => {
    return get<PaginationResponse<WerklijstOverzichtItem>>("/api/werklijst", {
      page,
      pageSize
    });
  }
};
