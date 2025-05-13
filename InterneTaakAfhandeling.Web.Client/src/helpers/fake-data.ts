import type { Internetaken } from "@/types/internetaken";
import type { _DeepPartial } from "pinia";

const _staticRecords: _DeepPartial<Internetaken>[] = [
   {
    uuid: "bf314e9a-dea8-4cfe-853b-b0aaff045e37",
    url: "https://openklant.dev.kiss-demo.nl/klantinteracties/api/v1/internetaken/bf314e9a-dea8-4cfe-853b-b0aaff045e37",
    nummer: "0000000033",
    gevraagdeHandeling: "Contact opnemen met betrokkene",
    aanleidinggevendKlantcontact: {
      uuid: "66b2a724-6bfc-4674-a87f-80639b93ed48",
      url: "https://openklant.dev.kiss-demo.nl/klantinteracties/api/v1/klantcontacten/66b2a724-6bfc-4674-a87f-80639b93ed48",
      plaatsgevondenOp: "2025-05-01T11:37:19.264+00:00",
      onderwerp: "Loterij organiseren - Mevrouw Swart wil graag weten hoe ze een loterij organiseert.",
      nummer: "0000000111",
      kanaal: "balie",
      inhoud: "",
      indicatieContactGelukt: true,
      taal: "nld",
      vertrouwelijk: false,
      gingOverOnderwerpobjecten: [
        { onderwerpobjectidentificator: { objectId: "02e2b0e9-d7cc-428e-83f0-edaa1847bea8" } }
      ],
      hadBetrokkenActoren: [{ 
        naam: "Amber Gerritsen",
        uuid: "3b033802-7482-4718-92da-170e5e2e4d63" 
      }],
      _expand: {
        hadBetrokkenen: [
          {
            uuid: "bbc09291-4c5e-4414-b2f7-b051c5ecbf77",
            volledigeNaam: "Saskia Swart",
            rol: "klant",
            initiator: true,
            _expand: {
              digitaleAdressen: [
                {
                  uuid: "b5e62d51-2db3-4bf6-b688-023945d60442",
                  adres: "0612345678",
                  soortDigitaalAdres: "telefoonnummer",
                  isStandaardAdres: false,
                  omschrijving: "telefoonnummer"
                }
              ]
            }
          }
        ],
        leiddeTotInterneTaken: [
          {
            uuid: "bf314e9a-dea8-4cfe-853b-b0aaff045e37",
            nummer: "0000000033",
            gevraagdeHandeling: "Contact opnemen met betrokkene",
            toelichting: "test",
            status: "te_verwerken",
            toegewezenOp: "2025-05-01T11:37:19.728006+00:00",
            toegewezenAanActoren: [
              {
                uuid: "3b033802-7482-4718-92da-170e5e2e4d63",
              },
              {
                uuid: "220dcf88-01c7-4a4a-a5b8-dc1d6672faa9",
              }
            ]
          }
        ]
      }
    },
    status: "te_verwerken",
    digitaleAdress: [{ 
      soortDigitaalAdres: "telefoonnummer", 
      adres: "0612345678" 
    }],
    betrokkene: {
      uuid: "bbc09291-4c5e-4414-b2f7-b051c5ecbf77",
      volledigeNaam: "Saskia Swart",
      rol: "klant"
    },
    toegewezenAanActoren: [
      { 
        uuid: "3b033802-7482-4718-92da-170e5e2e4d63",
        naam: "Jimme van der Jimme van der Meer" 
      },
      {
        uuid: "220dcf88-01c7-4a4a-a5b8-dc1d6672faa9",
      }
    ],
    toelichting: "test",
    toegewezenOp: "2025-05-01T11:37:19.728006+00:00"
  },
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
