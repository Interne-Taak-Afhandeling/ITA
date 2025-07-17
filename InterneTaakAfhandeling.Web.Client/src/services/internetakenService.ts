import { get } from "@/utils/fetchWrapper";
import { post } from "@/utils/fetchWrapper";
import type { Internetaken } from "@/types/internetaken";

export const internetakenService = {
  getInternetaak: (internetaakNummer: string): Promise<Internetaken> => {
    return get<Internetaken>(`/api/internetaken/${internetaakNummer}`);
  },
  addNoteToInternetaak: (internetaakId: string, notitie: string) => {
    return post(`/api/internetaken/${internetaakId}/notitie`, { notitie });
  }
};
