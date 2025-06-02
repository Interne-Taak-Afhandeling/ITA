// internetaken.ts
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
  digitaleAdress?: DigitaleAdres[];
  betrokkene?: Betrokkene;
  zaak?: Zaak;
}

export interface InternetakenResponse {
  count: number;
  next?: string;
  previous?: string;
  results: Internetaken[];
}

export interface Klantcontact {
  uuid: string;
  url: string;
  gingOverOnderwerpobjecten?: Onderwerpobject[];
  hadBetrokkenActoren: Actor[];
  omvatteBijlagen?: unknown[];
  hadBetrokkenen?: Betrokkene[];
  leiddeTotInterneTaken?: Internetaken[];
  nummer?: string;
  kanaal?: string;
  onderwerp?: string;
  inhoud?: string;
  indicatieContactGelukt?: boolean;
  taal?: string;
  vertrouwelijk?: boolean;
  plaatsgevondenOp: string;
  _expand?: Expand;
}

export interface Onderwerpobject {
  uuid: string;
  url: string;
  klantcontact?: Klantcontact;
  wasKlantcontact?: Klantcontact;
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
  indicatieActief?: boolean;
  actoridentificator?: Actoridentificator;
  actorIdentificatie?: unknown;
}

export interface ActorResponse {
  count: number;
  next?: string;
  previous?: string;
  results: Actor[];
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
  verstrektDoorBetrokkene?: Betrokkene;
  verstrektDoorPartij?: { uuid: string; url: string };
  adres?: string;
  soortDigitaalAdres?: string;
  isStandaardAdres?: boolean;
  omschrijving?: string;
}

export interface PartijReference {
  uuid: string;
  url?: string;
}

export interface Betrokkene {
  uuid: string;
  url: string;
  wasPartij?: PartijReference;
  hadKlantcontact?: Klantcontact;
  digitaleAdressen: DigitaleAdres[];
  bezoekadres?: Adres;
  correspondentieadres?: Adres;
  contactnaam?: Contactnaam;
  volledigeNaam?: string;
  rol?: string;
  organisatienaam?: string;
  initiator: boolean;
  _expand: BetrokkeneExpand;
}

export interface Adres {
  nummeraanduidingId?: string;
  straatnaam?: string;
  huisnummer?: number;
  huisnummertoevoeging?: string;
  postcode?: string;
  stad?: string;
  adresregel1?: string;
  adresregel2?: string;
  adresregel3?: string;
  land?: string;
}

export interface Contactnaam {
  voorletters?: string;
  voornaam?: string;
  voorvoegselAchternaam?: string;
  achternaam?: string;
}

export interface Expand {
  hadBetrokkenen?: Betrokkene[];
  leiddeTotInterneTaken?: Internetaken[];
  gingOverOnderwerpobjecten?: Onderwerpobject[];
}

export interface BetrokkeneExpand {
  digitaleAdressen?: DigitaleAdres[];
  wasPartij?: PartijReference;
}

export enum SoortActor {
  medewerker = "medewerker",
  klant = "klant",
  vertegenwoordiger = "vertegenwoordiger"
}

export interface ActorQuery {
  actoridentificatorCodeObjecttype: string;
  actoridentificatorCodeRegister: string;
  actoridentificatorCodeSoortObjectId: string;
  actoridentificatorObjectId: string;
  soortActor: SoortActor;
  indicatieActief?: boolean;
}

export interface CreateKlantcontactRequest {
  kanaal?: string;
  onderwerp?: string;
  inhoud?: string;
  indicatieContactGelukt?: boolean;
  taal?: string;
  vertrouwelijk?: boolean;
  plaatsgevondenOp: string;
}

export interface ActorReference {
  uuid: string;
}

export interface KlantcontactReference {
  uuid: string;
}

export interface ActorKlantcontactRequest {
  actor: ActorReference;
  klantcontact: KlantcontactReference;
}

export interface ActorKlantcontact {
  uuid: string;
  url: string;
  actor: ActorReference;
  klantcontact: KlantcontactReference;
}

export interface BetrokkeneRequest {
  wasPartij: PartijReference;
  hadKlantcontact: KlantcontactReference;
  rol: string;
  initiator: boolean;
}

export interface ActorRequest {
  naam: string;
  soortActor: string;
  indicatieActief: boolean;
  actoridentificator: Actoridentificator;
  actorIdentificatie?: {
    functie?: string;
    emailadres?: string;
    telefoonnummer?: string;
  };
}

export interface ActorIdentificatie {
  functie?: string;
  emailadres?: string;
  telefoonnummer?: string;
}

export interface CreateRelatedKlantcontactRequest {
  klantcontactRequest: CreateKlantcontactRequest;
  aanleidinggevendKlantcontactUuid?: string;
  partijUuid?: string;
}

export interface CreateRelatedKlantcontactAndCloseInterneTaakRequest
  extends CreateRelatedKlantcontactRequest {
  interneTaakId: string;
}

export interface RelatedKlantcontactResult {
  klantcontact: Klantcontact;
  actorKlantcontact: ActorKlantcontact;
  onderwerpobject?: Onderwerpobject;
  betrokkene?: Betrokkene;
}

export interface Contactmoment {
  uuid: string;
  contactGelukt: boolean;
  tekst: string;
  datum: string;
  medewerker: string;
  kanaal: string;
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
  productenOfDiensten: unknown[];
  vertrouwelijkheidaanduiding: string;
  betalingsindicatie: string;
  betalingsindicatieWeergave: string;
  laatsteBetaaldatum?: string;
  zaakgeometrie?: unknown;
  verlenging?: unknown;
  opschorting: unknown;
  selectielijstklasse: string;
  hoofdzaak?: unknown;
  deelzaken: unknown[];
  relevanteAndereZaken: unknown[];
  eigenschappen: unknown[];
  rollen: string[];
  status: string;
  zaakinformatieobjecten: unknown[];
  zaakobjecten: unknown[];
  kenmerken: unknown[];
  archiefnominatie: string;
  archiefstatus: string;
  archiefactiedatum?: string;
  resultaat?: unknown;
  opdrachtgevendeOrganisatie: string;
  processobjectaard: string;
  startdatumBewaartermijn?: string;
  processobject: unknown;
  zaaksysteemId: string;
}

export type InterneTaakQueryParameters = {
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
};
