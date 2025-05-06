import type { Internetaken } from "@/types/internetaken";
import type { _DeepPartial } from "pinia";

const _staticRecords: _DeepPartial<Internetaken>[] = [
  {
    uuid: "1",
    aanleidinggevendKlantcontact: {
      plaatsgevondenOp: "2025-01-01T15:00+01:00",
      onderwerp:
        "Loterij organiseren - Mevrouw Swart wil graag weten hoe ze een loterij organiseert.",
      nummer: "1",
      kanaal: "Telefoon",
      gingOverOnderwerpobjecten: [
        { onderwerpobjectidentificator: { objectId: "02e2b0e9-d7cc-428e-83f0-edaa1847bea8" } }
      ],
      hadBetrokkenActoren: [{ naam: "Amber Gerritsen" }]
    },
    status: "Te verwerken",
    digitaleAdress: [{ soortDigitaalAdres: "telefoonnummer", adres: "020123567" }],
    betrokkene: {
      volledigeNaam: "Saskia Swart"
    },
    toegewezenAanActoren: [{ naam: "Piet van Gelre" }],
    toelichting: `Niet op woensdagmiddag bellen.
Is de loterij voor een goed doel?: Ja
Is de locatie openbaar?: Nee, het is bij Mevrouw Moulin thuis.`
  },
  {
    uuid: "2",
    aanleidinggevendKlantcontact: {
      plaatsgevondenOp: "2025-01-02T15:00+01:00",
      onderwerp: "Dolor sit amet",
      nummer: "2"
    },
    betrokkene: { volledigeNaam: "Martijn de Groot" }
  }
];
export const fakeInterneTaken = _staticRecords as unknown as Internetaken[];
