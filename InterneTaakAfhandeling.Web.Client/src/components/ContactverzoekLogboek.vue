<template>
  <div v-if="loading" class="spinner-container">
    <simple-spinner />
  </div>
  <utrecht-alert v-else-if="error" appeareance="error" class="margin-top">
    Er ging iets mis. Probeer het later opnieuw.
  </utrecht-alert>
  <div v-else class="logboek-container">
    <div class="logboek-steps">
      <StepList>
        <Step v-for="logboekItem in mockLogboekData" :key="logboekItem.id" :appearance="getStepStatus(logboekItem.actieOmschrijving)">
          <StepHeader>
            <StepHeading :appearance="getStepStatus(logboekItem.actieOmschrijving)" class="actieomschrijving-titel">{{
              logboekItem.actieOmschrijving
            }}</StepHeading>
          </StepHeader>
          <StepDetails
            :id="`${logboekItem.id}--details`"
            :collapsed="false"
          >
            <StepList nested>
              <!-- Informatie burger/bedrijf -->
              <SubStep>
                <div class="substep-content">
                  <h4 class="substep-header">Informatie burger/bedrijf:</h4>
                  <SubStepHeading>{{ logboekItem.informatieBurger }}</SubStepHeading>
                </div>
              </SubStep>

              <!-- Interne toelichting met grijze achtergrond -->
              <div class="internal-wrapper">
                <SubStep>
                  <div class="substep-content">
                    <h4 class="substep-header">Interne toelichting:</h4>
                    <SubStepHeading>{{ logboekItem.interneToelichting }}</SubStepHeading>
                  </div>
                </SubStep>
              </div>
            </StepList>

            <!-- Footer met datum/naam links en kanaal rechts -->
            <div class="step-footer">
              <div class="step-footer-left">
                {{ formatDatum(logboekItem.datum) }} -
                {{ logboekItem.medewerker }}
              </div>
              <div class="step-footer-right">Kanaal: {{ logboekItem.kanaal }}</div>
            </div>
          </StepDetails>
        </Step>
      </StepList>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from "vue";
import type { Internetaken } from "@/types/internetaken";
import {
  Step,
  StepHeader,
  StepHeading,
  StepDetails,
  StepList,
  SubStep,
  SubStepHeading
} from "@/components/denhaag-process-steps";
import SimpleSpinner from "@/components/SimpleSpinner.vue";
import UtrechtAlert from "@/components/UtrechtAlert.vue";
import { formatNlDateTime } from "@/utils/dateUtils";

const props = defineProps<{ taak: Internetaken }>();

// Mock loading en error states
const loading = ref(false);
const error = ref(false);

// Mock logboek data - gesorteerd van nieuw naar oud
const mockLogboekData = [
  {
    id: "1",
    actieOmschrijving: "Afgerond",
    datum: new Date().toISOString(),
    medewerker: "John Doe",
    kanaal: "E-mail",
    informatieBurger: "Contactverzoek succesvol afgerond na het registreren van contactmoment",
    interneToelichting: "Alle benodigde informatie is verstrekt aan de klant"
  },
  {
    id: "2",
    actieOmschrijving: "Contact gelukt",
    datum: new Date(Date.now() - 3600000).toISOString(), // 1 uur geleden
    medewerker: "John Doe",
    kanaal: "Telefoon",
    informatieBurger:
      "Telefonisch contact opgenomen met klant. Vraag beantwoord over belastingaanslag.",
    interneToelichting: "Klant was tevreden met de uitleg over de belastingaanslag"
  },
  {
    id: "3",
    actieOmschrijving: "Zaak gekoppeld",
    datum: new Date(Date.now() - 86400000).toISOString(), // 1 dag geleden
    medewerker: "Jane Smith",
    kanaal: "Systeem",
    informatieBurger: "Zaak Z2024-001234 gekoppeld aan contactverzoek",
    interneToelichting: "Zaak betreft dezelfde belastingkwestie als het contactverzoek"
  },
  {
    id: "4",
    actieOmschrijving: "Opgepakt",
    datum: new Date(Date.now() - 172800000).toISOString(), // 2 dagen geleden
    medewerker: "Bob Johnson",
    kanaal: "Systeem",
    informatieBurger: "Contactverzoek in behandeling genomen",
    interneToelichting: "Contactverzoek toegewezen aan mezelf voor verdere afhandeling"
  }
];

const formatDatum = (datum: Date | string) => {
  return formatNlDateTime(datum);
};

const getStepStatus = (actieOmschrijving: string) => {
  switch (actieOmschrijving) {
    case "Afgerond":
      return "checked";
    case "Zaak gekoppeld":
    case "zaak gewijzigd":
      return "checked";
    case "Opgepakt":
      return "current";
    case "Contact gelukt":
      return "checked";
    case "Geen gehoor":
      return "warning";
    default:
      return "not-checked";
  }
};
</script>

<style lang="scss" scoped>
.step-footer {
  display: flex;
  justify-content: space-between;
  padding: 0.75rem 0rem;
  margin-top: 0.5rem;
  border-bottom: 1px solid #afb0b2;
  font-size: 0.875rem;
  color: #6c757d;
}

.step-footer-right {
  font-style: italic;
}

.actieomschrijving-titel {
  color: #24578f;
}

.substep-header {
  color: #333;
  margin: 0 0 0.25rem 0;
}

.internal-wrapper {
  background-color: #ececec;
  padding: 4px;
  font-style: italic;
}
</style>