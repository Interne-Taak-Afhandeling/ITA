import type { 
  CreateKlantcontactRequest, 
  RelatedKlantcontactResult,
  CreateRelatedKlantcontactRequest,
  Contactmoment
} from "@/types/internetaken";
import { get, post } from '@/utils/fetchWrapper';

export const klantcontactService = {
  createRelatedKlantcontact: (
    klantcontactRequest: CreateKlantcontactRequest, 
    previousKlantcontactUuid?: string,
    partijUuid?: string
  ): Promise<RelatedKlantcontactResult> => {
    const request: CreateRelatedKlantcontactRequest = {
      klantcontactRequest,
      previousKlantcontactUuid,
      partijUuid
    };
    
    return post<RelatedKlantcontactResult>('/api/createklantcontact/relatedklantcontact', request);
  },
 
  getInterneTaakContactmomenten: (contactverzoekId: string): Promise<Contactmoment[]> => 
    get<Contactmoment[]>(`/api/klantcontacten/${contactverzoekId}/contactmomenten`),
    
  getLaatsteKlantcontactUuid: (aanleidinggevendKlantcontactId: string): Promise<string> => 
    get<ContactmomentenResponse>(`/api/klantcontacten/${aanleidinggevendKlantcontactId}/contactketen`)
      .then(response => response.laatsteBekendKlantcontactUuid)
};

interface ContactmomentenResponse {
  contactmomenten: Contactmoment[];
  laatsteBekendKlantcontactUuid: string;
}

export type { 
  CreateKlantcontactRequest,
  RelatedKlantcontactResult,
  Contactmoment,
  ContactmomentenResponse
};