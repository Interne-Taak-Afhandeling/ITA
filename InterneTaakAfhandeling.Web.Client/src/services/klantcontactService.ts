import { post } from '@/utils/fetchWrapper';
import type { Klantcontact, Onderwerpobject } from '@/types/internetaken';

export interface CreateKlantcontactRequest {
  nummer?: string;
  kanaal: string;
  onderwerp: string;
  inhoud?: string;
  indicatieContactGelukt?: boolean;
  taal: string;
  vertrouwelijk: boolean;
  plaatsgevondenOp: string;
}

export interface ActorKlantcontact {
  uuid: string;
  url: string;
  actor: {
    uuid: string;
    url: string;
  };
  klantcontact: {
    uuid: string;
    url: string;
  };
}

export interface RelatedKlantcontactResult {
  klantcontact: Klantcontact;
  actorKlantcontact: ActorKlantcontact;
  onderwerpobject?: Onderwerpobject;
}

export interface CreateRelatedKlantcontactRequest {
  klantcontactRequest: CreateKlantcontactRequest;
  previousKlantcontactUuid?: string;
}

export const klantcontactService = {
  /**
   * Maakt een nieuw klantcontact aan dat gekoppeld is aan de huidige actor en optioneel aan een vorig klantcontact in één request
   * POST /api/createklantcontact/relatedklantcontact
   * 
   * Deze methode combineert het aanmaken van een klantcontact, koppelen aan de huidige actor,
   * en eventueel koppelen aan een vorig klantcontact in één API call.
   */
  createRelatedKlantcontact: (
    klantcontactRequest: CreateKlantcontactRequest, 
    previousKlantcontactUuid?: string
  ): Promise<RelatedKlantcontactResult> => {
    const request: CreateRelatedKlantcontactRequest = {
      klantcontactRequest,
      previousKlantcontactUuid
    };
    
    return post<RelatedKlantcontactResult>('/api/createklantcontact/relatedklantcontact', request);
  }
};