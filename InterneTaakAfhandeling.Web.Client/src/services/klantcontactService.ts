import { post } from '@/utils/fetchWrapper';
import type { Klantcontact } from '@/types/internetaken';

// Interface voor het aanmaken van een klantcontact
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

export const klantcontactService = {
  /**
   * Maakt een nieuw klantcontact aan via de API
   * POST /api/klantcontact/klantcontacten
   */
  createKlantcontact: (request: CreateKlantcontactRequest): Promise<Klantcontact> => {
    return post<Klantcontact>('/api/user/klantcontacten', request);
  }
};