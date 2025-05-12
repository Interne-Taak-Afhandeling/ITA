export interface Internetaken {
  uuid: string;
  url: string;
  nummer: string;
  gevraagdeHandeling: string;
  aanleidinggevendKlantcontact: Klantcontact;
  toegewezenAanActor?: Actor;
  toegewezenAanActoren?: Actor[];
  toelichting?: string;
  status: string;
  toegewezenOp: string;   
  afgehandeldOp?: string;   
  zaak?: Zaak;
}

export interface Klantcontact {
  uuid: string;
  url: string;
  gingOverOnderwerpobjecten?: Onderwerpobject[];
  hadBetrokkenActoren: Actor[];
  omvatteBijlagen?: any[];
  hadBetrokkenen: Betrokkene[];
  leiddeTotInterneTaken?: Internetaak[];
  nummer?: string;
  kanaal?: string;
  onderwerp?: string;
  inhoud?: string;
  indicatieContactGelukt?: boolean;
  taal?: string;
  vertrouwelijk?: boolean;
  plaatsgevondenOp: string;   
  expand?: Expand;
}

export interface Actor {
  uuid: string;
  url?: string;
  naam?: string;
  soortActor?: string;
  indicatieActief?: boolean;
  actoridentificator?: Actoridentificator;
  actorIdentificatie?: any;
}

export interface Zaak {
  url: string;
  uuid: string;
  identificatie: string;
  bronorganisatie: string;
  omschrijving: string;
  toelichting: string;
  zaaktype: string;
  registratiedatum: string;   
  verantwoordelijkeOrganisatie: string;
  startdatum?: string;   
  einddatum?: string;   
  einddatumGepland?: string;   
  uiterlijkeEinddatumAfdoening?: string;   
  publicatiedatum?: string;   
  communicatiekanaal: string;
  communicatiekanaalNaam: string;
  productenOfDiensten: any[];
  vertrouwelijkheidaanduiding: string;
  betalingsindicatie: string;
  betalingsindicatieWeergave: string;
  laatsteBetaaldatum?: string;   
  zaakgeometrie?: any;
  verlenging?: any;
  opschorting: Opschorting;
  selectielijstklasse: string;
  hoofdzaak?: any;
  deelzaken: any[];
  relevanteAndereZaken: any[];
  eigenschappen: any[];
  rollen: string[];
  status: string;
  zaakinformatieobjecten: any[];
  zaakobjecten: any[];
  kenmerken: any[];
  archiefnominatie: string;
  archiefstatus: string;
  archiefactiedatum?: string;   
  resultaat?: any;
  opdrachtgevendeOrganisatie: string;
  processobjectaard: string;
  startdatumBewaartermijn?: string;   
  processobject: Processobject;
  zaaksysteemId: string;
}
