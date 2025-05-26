import { get } from "@/utils/fetchWrapper";
import type { InterneTaakQueryParameters, Internetaken } from "@/types/internetaken";

export const internetakenService = {
  getInternetaak: (internetaakQuery: InterneTaakQueryParameters): Promise<Internetaken> => {
    return get<Internetaken>(`/api/internetaken`, internetaakQuery);
  }
};
