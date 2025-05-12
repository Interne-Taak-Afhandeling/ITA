import { post, get } from '@/utils/fetchWrapper';
import type { Klantcontact, Onderwerpobject, Onderwerpobjectidentificator } from '@/types/internetaken';

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

export interface RelatedKlantcontactResult {
  klantcontactResult: KlantcontactWithActorResult;
  onderwerpobject: Onderwerpobject;
}

export const klantcontactService = {
  /**
   * Maakt een nieuw klantcontact aan en koppelt het automatisch aan de huidige ingelogde gebruiker
   * POST /api/createklantcontact/klantcontactenmetactor
   */
  createKlantcontactWithCurrentActor: (request: CreateKlantcontactRequest): Promise<KlantcontactWithActorResult> => {
    return post<KlantcontactWithActorResult>('/api/createklantcontact/klantcontactenmetactor', request);
  },

  /**
   * Koppelt een klantcontact aan een ander klantcontact als onderwerpobject
   * POST /api/createklantcontact/onderwerpobjecten
   */
  createOnderwerpobject: (onderwerpobject: {
    klantcontact?: { uuid: string };
    wasKlantcontact?: { uuid: string };
    onderwerpobjectidentificator?: Onderwerpobjectidentificator;
  }): Promise<Onderwerpobject> => {
    return post<Onderwerpobject>('/api/createklantcontact/onderwerpobjecten', onderwerpobject);
  },

  /**
   * Helper methode om een nieuw klantcontact aan te maken en te koppelen aan een vorig klantcontact
   */
  createRelatedKlantcontact: async (
    klantcontactRequest: CreateKlantcontactRequest, 
    previousKlantcontactUuid: string
  ): Promise<RelatedKlantcontactResult> => {
    // Maak eerst het nieuwe klantcontact aan
    const klantcontactResult = await klantcontactService.createKlantcontactWithCurrentActor(klantcontactRequest);
    
    // Maak een onderwerpobject aan dat het nieuwe klantcontact koppelt aan het vorige
    const onderwerpobject = await klantcontactService.createOnderwerpobject({
      klantcontact: {
        uuid: klantcontactResult.klantcontact.uuid
      },
      wasKlantcontact: {
        uuid: previousKlantcontactUuid
      }
    });
    
    return {
      klantcontactResult,
      onderwerpobject
    };
  }
};