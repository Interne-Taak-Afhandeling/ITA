import type {
  CreateKlantcontactRequest,
  RelatedKlantcontactResult,
  CreateRelatedKlantcontactRequest,
  Contactmoment
} from "@/types/internetaken";
import { get, post } from "@/utils/fetchWrapper";

interface LogboekActiviteit {
  datum: string;
  type: string;
  kanaal: string | undefined;
  tekst: string | undefined;
  id: string;
  contactGelukt: string | undefined;
  medewerker: string | undefined;
  zaakIdentificatie?: string; 
}

export const klantcontactService = {
  createRelatedKlantcontact: (
    request: CreateRelatedKlantcontactRequest
  ): Promise<RelatedKlantcontactResult> => {
    return post<RelatedKlantcontactResult>("/api/klantcontacten/add-klantcontact", request);
  },

  createRelatedKlantcontactAndCloseInterneTaak: (
    request: CreateRelatedKlantcontactRequest
  ): Promise<RelatedKlantcontactResult> => {
    return post<RelatedKlantcontactResult>("/api/internetaken/close-with-klantcontact", request);
  },

  getLogboek: (internetaakId: string, signal?: AbortSignal): Promise<LogboekActiviteit[]> =>
    get<LogboekActiviteit[]>(`/api/internetaken/${internetaakId}/logboek`, undefined, {
      signal
    })
};

export type {
  CreateKlantcontactRequest,
  RelatedKlantcontactResult,
  Contactmoment,
  LogboekActiviteit
};
