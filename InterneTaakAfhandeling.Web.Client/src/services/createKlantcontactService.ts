import { get, post } from '@/utils/fetchWrapper';
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

export interface Contactmoment {
  contactGelukt: boolean,
   tekst: string, 
   datum: string, 
   medewerker : string, 
   kanaal : string
}

export const klantcontactService = {
  createRelatedKlantcontact: (
    klantcontactRequest: CreateKlantcontactRequest, 
    previousKlantcontactUuid?: string
  ): Promise<RelatedKlantcontactResult> => {
    const request: CreateRelatedKlantcontactRequest = {
      klantcontactRequest,
      previousKlantcontactUuid
    };
    
    return post<RelatedKlantcontactResult>('/api/createklantcontact/relatedklantcontact', request);
  },
 
  getInterneTaakContactmomenten: (contactverzoekId : string ): Promise<Contactmoment[]> => get<Contactmoment[]>(`/api/internetaak/${contactverzoekId}/contactmomenten`)

};