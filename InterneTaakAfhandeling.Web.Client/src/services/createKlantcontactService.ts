import type { 
  CreateKlantcontactRequest, 
  RelatedKlantcontactResult,
  CreateRelatedKlantcontactRequest
} from "@/types/internetaken";
import { post } from '@/utils/fetchWrapper';

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
    
    return post<RelatedKlantcontactResult>('/api/createklantcontact/relatedklantcontact', request);
  }
};

export type { 
  CreateKlantcontactRequest,
  RelatedKlantcontactResult
};