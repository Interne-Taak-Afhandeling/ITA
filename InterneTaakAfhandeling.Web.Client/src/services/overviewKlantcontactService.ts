import type { Contactmoment } from "@/types/internetaken";
import { get } from '@/utils/fetchWrapper';

interface ContactmomentenResponse {
  contactmomenten: Contactmoment[];
  laatsteBekendKlantcontactUuid: string;
}

export const overviewKlantcontactService = {
  getContactKeten: (klantcontactId: string): Promise<ContactmomentenResponse> =>
    get<ContactmomentenResponse>(`/api/klantcontacten-overview/${klantcontactId}/contactketen`),
  
  /**
   * Helper-functie die alleen het UUID van het laatste klantcontact in de keten ophaalt
   * @param klantcontactId - De UUID van het klantcontact
   * @returns Een promise met alleen het UUID van het laatste klantcontact
   */
  getLaatsteKlantcontactUuid: (klantcontactId: string): Promise<string> =>
    get<ContactmomentenResponse>(`/api/klantcontacten-overview/${klantcontactId}/contactketen`)
      .then(response => response.laatsteBekendKlantcontactUuid)
};

export type { 
  Contactmoment,
  ContactmomentenResponse 
};