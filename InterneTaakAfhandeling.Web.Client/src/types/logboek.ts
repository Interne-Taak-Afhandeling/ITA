// Types voor de toekomstige Logboek functionaliteit
export interface LogboekEntry {
  uuid: string;
  actieOmschrijving: 'afgerond' | 'zaak gekoppeld' | 'zaak gewijzigd' | 'opgepakt' | 'contactmoment';
  datum: string; // ISO datetime string
  actorNaam: string;
  details?: string; // Extra informatie over de actie
  kanaal?: string; // Voor contactmomenten
  contactGelukt?: boolean; // Voor contactmomenten
}

export interface LogboekResponse {
  logboekEntries: LogboekEntry[];
  laatsteBekendLogboekUuid: string;
}

// Mapping van actie types naar Nederlandse titels
export const ACTION_TITLES: Record<LogboekEntry['actieOmschrijving'], string> = {
  'afgerond': 'Afgerond',
  'zaak gekoppeld': 'Zaak gekoppeld', 
  'zaak gewijzigd': 'Zaak gewijzigd',
  'opgepakt': 'Opgepakt',
  'contactmoment': 'Contactmoment'
};

// Voor contactmomenten gebruiken we nog steeds de bestaande logica
export const CONTACTMOMENT_TITLES = {
  contactGelukt: 'Contact gelukt',
  geenGehoor: 'Geen gehoor'
} as const;