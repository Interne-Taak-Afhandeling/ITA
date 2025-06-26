<template>
  <div v-if="loading" class="spinner-container">
    <simple-spinner />
  </div>
  <utrecht-alert v-else-if="error" appeareance="error" class="margin-top">
    Er ging iets mis. Probeer het later opnieuw.
  </utrecht-alert>
  <div v-else class="logboek-container">
    <!-- Debug info (tijdelijk) -->
    <div v-if="false" style="background: yellow; padding: 1rem; margin-bottom: 1rem;">
      <strong>Debug:</strong>
      <pre>{{ JSON.stringify(logboekSteps, null, 2) }}</pre>
    </div>
    
    <!-- Low-level components voor volledige controle -->
    <div class="logboek-steps">
      <StepList>
        <Step v-for="step in logboekSteps" :key="step.id" :appearance="step.status">
          <StepHeader>
            <StepMarker :appearance="step.status" />
            <StepHeading :appearance="step.status" class="actieomschrijving-titel">{{ step.title }}</StepHeading>
          </StepHeader>
          <StepDetails v-if="step.steps && step.steps.length > 0" :id="`${step.id}--details`" :collapsed="false">
<StepList nested>
  <!-- Informatie burger/bedrijf -->
  <SubStep v-for="subStep in step.steps.filter(s => !s.header.includes('Interne'))" :key="`${step.id}-${subStep.title}`">
    <StepMarker nested :appearance="subStep.status" />
    <div class="substep-content">
      <h4 class="substep-header">{{ subStep.header }}</h4>
      <SubStepHeading>{{ subStep.title }}</SubStepHeading>
    </div>
  </SubStep>
  
  <!-- Interne toelichting met grijze achtergrond -->
  <div class="internal-wrapper" v-if="step.steps.some(s => s.header.includes('Interne'))">
    <SubStep v-for="subStep in step.steps.filter(s => s.header.includes('Interne'))" :key="`${step.id}-${subStep.title}`">
      <StepMarker nested :appearance="subStep.status" />
      <div class="substep-content">
        <h4 class="substep-header">{{ subStep.header }}</h4>
        <SubStepHeading>{{ subStep.title }}</SubStepHeading>
      </div>
    </SubStep>
  </div>
</StepList>
            
            <!-- Aangepaste footer met datum/naam links en kanaal rechts -->
            <div class="step-footer">
              <div class="step-footer-left">
                {{ formatDatum(getStepData(step.id).datum) }} - {{ getStepData(step.id).medewerker }}
              </div>
              <div class="step-footer-right">
                Kanaal: {{ getStepData(step.id).kanaal }}
              </div>
            </div>
          </StepDetails>
        </Step>
      </StepList>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from "vue";
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
import type { StepProps } from "@/components/denhaag-process-steps/types";
import SimpleSpinner from "@/components/SimpleSpinner.vue";
import UtrechtAlert from "@/components/UtrechtAlert.vue";
import { formatNlDateTime } from "@/utils/dateUtils";

const props = defineProps<{ taak: Internetaken }>();

// Mock loading en error states  
const loading = ref(false);
const error = ref(false);
const expandedSteps = ref<string[]>(["1", "2", "3", "4"]); // Alle items standaard uitgeklapd

// Mock logboek data - met dezelfde structuur als ContactverzoekDetails
// Gesorteerd van nieuw naar oud
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
    informatieBurger: "Telefonisch contact opgenomen met klant. Vraag beantwoord over belastingaanslag.",
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
    case "zaak gekoppeld":
    case "zaak gewijzigd": 
      return "checked";
    case "opgepakt":
      return "current";
    case "Contact gelukt":
      return "checked";
    case "Geen gehoor":
      return "warning";
    default:
      return "not-checked";
  }
};

const getStepData = (stepId: string) => {
  return mockLogboekData.find(item => item.id === stepId) || mockLogboekData[0];
};

const logboekSteps = computed<StepProps[]>(() => {
  return mockLogboekData.map((item, index) => ({
    id: item.id,
    title: item.actieOmschrijving,
    status: getStepStatus(item.actieOmschrijving),
    collapsible: true,
    steps: [
      // Informatie voor burger/bedrijf
      {
        title: item.informatieBurger,
        header: "Informatie burger/bedrijf:",
        status: "checked"  
      },
      // Interne toelichting
      {
        title: item.interneToelichting,
        header: "Interne toelichting:",
        status: "checked"  
      }
    ]
  }));
});
</script>

<style lang="scss" scoped>
.step-footer {
  display: flex;
  justify-content: space-between;
  padding: 0.75rem 1rem;
  margin-top: 0.5rem;
  border-bottom: 1px solid #afb0b2; 
  font-size: 0.875rem;
  color: #6c757d;
}

.step-footer-right {
  font-style: italic;
}

.actieomschrijving-titel {
  color: #24578F;
}

.substep-header {
  color: #333;
  margin: 0 0 0.25rem 0;
}

.substep-content {
  margin-left: -7px; // Compensate for the extra 7px zodat die niet inspringt(23px - 16px)
}
.internal-wrapper {
  background-color: #ececec;
  padding: 4px;
  font-style: italic;
}
</style>