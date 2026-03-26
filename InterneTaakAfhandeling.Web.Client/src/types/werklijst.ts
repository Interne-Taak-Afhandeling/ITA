export interface WerklijstOverzichtItem {
  url: string;
  uuid: string;
  nummer?: string;
  status?: string;
  onderwerp?: string;
  kanaal?: string;
  plaatsgevondenOp?: string;
  afdeling?: string;
  groep?: string;
  medewerker?: string;
  afhandeltermijnDatum?: string;
  verstrekenWerkdagen: number;
  isOverschreden: boolean;
}

export interface WerkverdelingRequest {
  internetaakUuids: string[];
  afdelingUuid?: string;
  groepUuid?: string;
  medewerkerEmail?: string;
}

export interface WerkverdelingResponse {
  succeeded: number;
  failed: number;
  results: WerkverdelingItemResult[];
}

export interface WerkverdelingItemResult {
  internetaakUuid: string;
  success: boolean;
  errorMessage?: string;
}
