// In createKlantcontactService.ts
import type { 
  Klantcontact, 
  Betrokkene, 
  Onderwerpobject, 
  CreateKlantcontactRequest, 
  ActorKlantcontact, 
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
 
  getInterneTaakContactmomenten: (contactmomentId: string): Promise<Contactmoment[]> => 
    get<Contactmoment[]>(`/api/klantcontacten/${contactmomentId}`)
};

export type { 
  CreateKlantcontactRequest,
  RelatedKlantcontactResult,
  Contactmoment
};