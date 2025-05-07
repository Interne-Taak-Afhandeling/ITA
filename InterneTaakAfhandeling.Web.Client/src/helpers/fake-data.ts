import type { Internetaken } from "@/types/internetaken";
import type { _DeepPartial } from "pinia";

const _staticRecords: _DeepPartial<Internetaken>[] = [
  {
    uuid: "1",
    aanleidinggevendKlantcontact: {
      plaatsgevondenOp: "2025-01-01T15:00+01:00",
      onderwerp: "Lorem ipsum"
    },
    betrokkene: { volledigeNaam: "Saskia Swart" }
  },
  {
    uuid: "2",
    aanleidinggevendKlantcontact: {
      plaatsgevondenOp: "2025-01-02T15:00+01:00",
      onderwerp: "Dolor sit amet"
    },
    betrokkene: { volledigeNaam: "Martijn de Groot" }
  }
];
export const fakeInterneTaken = _staticRecords as unknown as Internetaken[];
