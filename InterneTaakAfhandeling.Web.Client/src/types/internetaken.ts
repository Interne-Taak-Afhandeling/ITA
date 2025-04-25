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
  betrokken?: Actor;
}

export interface Klantcontact {
  uuid: string;
  url: string;
  gingOverOnderwerpobjecten?: Onderwerpobject[];
  hadBetrokkenActoren: Actor[];
  hadBetrokkenen: Betrokkene[];
  nummer?: string;
  kanaal?: string;
  onderwerp?: string;
  inhoud?: string;
  taal?: string;
  vertrouwelijk?: boolean;
  plaatsgevondenOp: string;   
  expand?: Expand;
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
