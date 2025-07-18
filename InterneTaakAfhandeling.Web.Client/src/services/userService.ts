import { get, post } from "@/utils/fetchWrapper";
import type { Internetaken } from "@/types/internetaken";

export const userService = {
  getAssignedInternetaken: (): Promise<Internetaken[]> => {
    return get<Internetaken[]>("/api/internetaken/assigned-to-me?afgerond=false");
  },
  assignInternetakenToSelf: (id: string): Promise<boolean> => {
    return post<boolean>(`/api/internetaken/${id}/assign-to-me`, {});
  },
  getAssignedAndFinishedInternetaken: (): Promise<Internetaken[]> => {
    return get<Internetaken[]>("/api/internetaken/assigned-to-me?afgerond=true");
  }
};
