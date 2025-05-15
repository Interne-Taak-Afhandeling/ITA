export interface Internetaken {
  uuid: string;
  url: string;
  nummer: string;
  gevraagdeHandeling: string;
  aanleidinggevendKlantcontact?: Klantcontact;
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
  nummer?: string;
  kanaal?: string;
  onderwerp?: string;
  inhoud?: string;
  taal?: string;
  vertrouwelijk?: boolean;
  plaatsgevondenOp: string;
  _expand?: Expand;
}

export interface Onderwerpobject {
  uuid: string;
  url: string;
  klantcontact?: Klantcontact;
  wasKlantcontact?: any;
  onderwerpobjectidentificator?: Onderwerpobjectidentificator;
}

export interface Onderwerpobjectidentificator {
  objectId: string;
  codeObjecttype: string;
  codeRegister: string;
  codeSoortObjectId: string;
}

export interface Actor {
  uuid: string;
  url: string;
  naam?: string;
  soortActor?: string;
  actoridentificator?: Actoridentificator;
  actorIdentificatie?: any;
}

export interface Actoridentificator {
  objectId: string;
  codeObjecttype: string;
  codeRegister: string;
  codeSoortObjectId: string;
}

export interface DigitaleAdres {
  uuid: string;
  url: string;
  adres?: string;
  soortDigitaalAdres?: string;
  omschrijving?: string;
}

export interface Betrokkene {
  uuid: string;
  url: string;
  wasPartij?: any;
  hadKlantcontact?: Klantcontact;
  digitaleAdressen: DigitaleAdres[];
  bezoekadres?: Adres;
  correspondentieadres?: Adres;
  contactnaam?: Contactnaam;
  volledigeNaam?: string;
  rol?: string;
  organisatienaam?: string;
  initiator: boolean;
  expand?: BetrokkeneExpand;
}

export interface Adres {
  nummeraanduidingId: string;
  adresregel1: string;
  adresregel2: string;
  adresregel3: string;
  land: string;
}

export interface Contactnaam {
  voorletters: string;
  voornaam: string;
  voorvoegselAchternaam: string;
  achternaam: string;
}

export interface Expand {
  hadBetrokkenen?: Betrokkene[];
}

export interface BetrokkeneExpand {
  digitaleAdressen?: DigitaleAdres[];
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
  opschorting: any;
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
  processobject: any;
  zaaksysteemId: string;
}

export interface InterneTaakQueryParameters {
  value?: string;
  AanleidinggevendKlantcontact_Url?: string;
  AanleidinggevendKlantcontact_Uuid?: string;

  Actoren_Naam?: string;
  Klantcontact_Nummer?: string;
  Klantcontact_Uuid?: string;

  Nummer?: string;

  Page?: number;
  PageSize?: number;

  Status?: string;

  ToegewezenAanActor_Url?: string;
  ToegewezenAanActor_Uuid?: string;

  ToegewezenOp?: Date;
}
