import { get, post } from "@/utils/fetchWrapper";
import type { MyInterneTaakOverviewItem } from "@/components/interne-taken-tables/MyInterneTakenTable.vue";

export const userService = {
  getAssignedInternetaken: (): Promise<MyInterneTaakOverviewItem[]> => {
    return get<MyInterneTaakOverviewItem[]>("/api/internetaken/aan-mij-toegewezen?afgerond=false");
  },
  assignInternetakenToSelf: (id: string): Promise<boolean> => {
    return post<boolean>(`/api/internetaken/${id}/aan-mij-toewijzen`, {});
  },
  getAssignedAndFinishedInternetaken: (): Promise<MyInterneTaakOverviewItem[]> => {
    return get<MyInterneTaakOverviewItem[]>("/api/internetaken/aan-mij-toegewezen?afgerond=true");
  }
};
