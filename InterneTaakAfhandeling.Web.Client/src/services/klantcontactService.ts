import type {
  CreateKlantcontactRequest,
  RelatedKlantcontactResult,
  CreateRelatedKlantcontactRequest,
  Contactmoment  
} from "@/types/internetaken";
import { get, post } from "@/utils/fetchWrapper";

interface ContactmomentenResponse {
  contactmomenten: Contactmoment[];
  laatsteBekendKlantcontactUuid: string;
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

  getContactKeten: (
    klantcontactId: string,
    signal?: AbortSignal
  ): Promise<ContactmomentenResponse> =>
    get<ContactmomentenResponse>(
      `/api/klantcontacten/${klantcontactId}/klantcontacten`,
      undefined,
      { signal }
    )

  // /**
  //  * Helper-functie die alleen het UUID van het laatste klantcontact in de keten ophaalt
  //  * @param klantcontactId - De UUID van het klantcontact
  //  * @returns Een promise met alleen het UUID van het laatste klantcontact
  //  */
  // getLaatsteKlantcontactUuid: (klantcontactId: string): Promise<string> =>
  //   get<ContactmomentenResponse>(`/api/klantcontacten-overview/${klantcontactId}/contactketen`)
  //     .then(response => response.laatsteBekendKlantcontactUuid)
};

export type {
  CreateKlantcontactRequest,
  RelatedKlantcontactResult,
  Contactmoment,
  ContactmomentenResponse
};
