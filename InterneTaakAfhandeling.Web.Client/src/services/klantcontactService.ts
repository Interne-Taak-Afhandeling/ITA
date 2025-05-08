import { post } from '@/utils/fetchWrapper';
import type { Klantcontact } from '@/types/internetaken';

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
export interface KlantcontactWithActorResult {
  klantcontact: Klantcontact;
  actorKlantcontact: ActorKlantcontact;
}

export const klantcontactService = {
  createKlantcontactWithCurrentActor: (request: CreateKlantcontactRequest): Promise<KlantcontactWithActorResult> => {
    return post<KlantcontactWithActorResult>('/api/user/klantcontactenmetactor', request);
  }
};
