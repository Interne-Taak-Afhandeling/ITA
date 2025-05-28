import type {
  CreateKlantcontactRequest,
  RelatedKlantcontactResult,
  CreateRelatedKlantcontactRequest,
  Contactmoment,
  CloseInterneTaakWithKlantContactRequest
} from "@/types/internetaken";
import { get, post } from "@/utils/fetchWrapper";

interface ContactmomentenResponse {
  contactmomenten: Contactmoment[];
  laatsteBekendKlantcontactUuid: string;
}

export const klantcontactService = {
  createRelatedKlantcontact: (
    klantcontactRequest: CreateKlantcontactRequest,
    aanleidinggevendKlantcontactUuid?: string,
    partijUuid?: string
  ): Promise<RelatedKlantcontactResult> => {
    const request: CreateRelatedKlantcontactRequest = {
      klantcontactRequest,
      aanleidinggevendKlantcontactUuid,
      partijUuid
    };

    return post<RelatedKlantcontactResult>("/api/createklantcontact/relatedklantcontact", request);
  },

  closeInterneTaakWithKlantContact: (
    request: CloseInterneTaakWithKlantContactRequest
  ): Promise<RelatedKlantcontactResult> => {
    return post<RelatedKlantcontactResult>(
      "/api/CloseInterneTaakWithKlantContact/closeInterneTaakWithKlantContact", 
      request
    );
  },

  getContactKeten: (
    klantcontactId: string,
    signal?: AbortSignal
  ): Promise<ContactmomentenResponse> =>
    get<ContactmomentenResponse>(
      `/api/klantcontacten-overview/${klantcontactId}/contactketen`,
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