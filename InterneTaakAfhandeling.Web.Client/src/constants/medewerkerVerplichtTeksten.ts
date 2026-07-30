export type DoorstuurDoelType = "afdeling" | "groep";

export function getMedewerkerLabel(heeftGroepsmailbox: boolean): string {
  return heeftGroepsmailbox ? "Medewerker (optioneel)" : "Medewerker";
}

export function getMedewerkerVerplichtToelichting(doelType: DoorstuurDoelType): string {
  return `De geselecteerde ${doelType} heeft geen eigen e-mailadres, daarom is het verplicht om een medewerker te selecteren.`;
}

export function getGeenMedewerkerBeschikbaarMelding(doelType: DoorstuurDoelType): string {
  return `U kunt geen contactverzoeken doorsturen naar deze ${doelType}. Deze ${doelType} heeft geen e-mailadres en er zijn geen medewerkers beschikbaar binnen deze ${doelType}.`;
}
