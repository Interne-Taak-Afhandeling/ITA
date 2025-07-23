import { get, post } from "@/utils/fetchWrapper";
import type { Internetaken } from "@/types/internetaken";

export const userService = {
  getAssignedInternetaken: (): Promise<Internetaken[]> => {
    return get<Internetaken[]>("/api/internetaken/aan-mij-toegewezen?afgerond=false");
  },
  assignInternetakenToSelf: (id: string): Promise<boolean> => {
    return post<boolean>(`/api/internetaken/${id}/aan-mij-toewijzen`, {});
  },
  getAssignedAndFinishedInternetaken: (): Promise<Internetaken[]> => {
    return get<Internetaken[]>("/api/internetaken/aan-mij-toegewezen?afgerond=true");
  }
};
