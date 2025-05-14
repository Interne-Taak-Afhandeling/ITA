// In createKlantcontactService.ts
import type { 
  CreateKlantcontactRequest, 
  RelatedKlantcontactResult,
  CreateRelatedKlantcontactRequest,
  Contactmoment
} from "@/types/internetaken";
import { get, post } from '@/utils/fetchWrapper';

// In createKlantcontactService.ts
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
    get<Contactmoment[]>(`/api/internetaak/${contactverzoekId}/contactmomenten`),
    
  // Methode wijzigen om de contactmomenten response te gebruiken
  getLaatsteKlantcontactUuid: (aanleidinggevendKlantcontactId: string): Promise<string> => 
    get<ContactmomentenResponse>(`/api/internetaak/klantcontacten/${aanleidinggevendKlantcontactId}/contactmomenten`)
      .then(response => response.laatsteBekendKlantcontactUuid)
};

// Voeg deze interface toe
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