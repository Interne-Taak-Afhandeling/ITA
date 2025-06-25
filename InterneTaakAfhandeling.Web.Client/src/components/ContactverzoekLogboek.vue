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
    
    <!-- Low-level components -->
    <div class="logboek-steps">
      <StepList>
        <Step v-for="step in logboekSteps" :key="step.id" :appearance="step.status">
          <StepHeader>
            <StepMarker :appearance="step.status" />
            <StepHeading :appearance="step.status">{{ step.title }}</StepHeading>
          </StepHeader>
          <StepDetails v-if="step.steps && step.steps.length > 0" :id="`${step.id}--details`" :collapsed="false">
            <StepList nested>
              <SubStep v-for="subStep in step.steps" :key="`${step.id}-${subStep.title}`">
                <StepMarker nested :appearance="subStep.status" />
                <SubStepHeading>{{ subStep.title }}</SubStepHeading>
              </SubStep>
            </StepList>
          </StepDetails>
        </Step>
      </StepList>
    </div>
    
    <!-- Alternatief: als Status niet werkt, gebruik dan de low-level componenten -->
    <!--
    <div class="logboek-steps">
      <StepList>
        <Step v-for="step in logboekSteps" :key="step.id" :appearance="step.status">
          <StepHeader>
            <StepMarker :appearance="step.status" />
            <StepHeading :appearance="step.status">{{ step.title }}</StepHeading>
          </StepHeader>
          <StepDetails v-if="step.steps && step.steps.length > 0" :id="`${step.id}--details`">
            <StepList nested>
              <SubStep v-for="subStep in step.steps" :key="`${step.id}-${subStep.title}`">
                <StepMarker nested :appearance="subStep.status" />
                <SubStepHeading>{{ subStep.title }}</SubStepHeading>
              </SubStep>
            </StepList>
          </StepDetails>
        </Step>
      </StepList>
    </div>
    -->
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
import { StepMarker } from "@/components/denhaag-step-marker";
import type { StepProps } from "@/components/denhaag-process-steps/types";
import SimpleSpinner from "@/components/SimpleSpinner.vue";
import UtrechtAlert from "@/components/UtrechtAlert.vue";
import { formatNlDateTime } from "@/utils/dateUtils";

const props = defineProps<{ taak: Internetaken }>();

// Mock loading en error states  
const loading = ref(false);
const error = ref(false);
const expandedSteps = ref<string[]>(["1", "2", "3", "4"]); // Alle items standaard uitgeklapd

// Mock logboek data - later vervangen door echte API call
// Gesorteerd van nieuw naar oud
const mockLogboekData = [
  {
    id: "1",
    actieOmschrijving: "afgerond",
    datum: new Date().toISOString(),
    medewerker: "John Doe",
    details: "Contactverzoek succesvol afgerond na het registreren van contactmoment",
    kanaal: ""
  },
  {
    id: "2", 
    actieOmschrijving: "Contact gelukt",
    datum: new Date(Date.now() - 3600000).toISOString(), // 1 uur geleden
    medewerker: "John Doe", 
    details: "Telefonisch contact opgenomen met klant. Vraag beantwoord over belastingaanslag.",
    kanaal: "Telefoon"
  },
  {
    id: "3",
    actieOmschrijving: "zaak gekoppeld", 
    datum: new Date(Date.now() - 86400000).toISOString(), // 1 dag geleden
    medewerker: "Jane Smith",
    details: "Zaak Z2024-001234 gekoppeld aan contactverzoek",
    kanaal: ""
  },
  {
    id: "4",
    actieOmschrijving: "opgepakt",
    datum: new Date(Date.now() - 172800000).toISOString(), // 2 dagen geleden
    medewerker: "Bob Johnson", 
    details: "Contactverzoek toegewezen aan mezelf",
    kanaal: ""
  }
];

const formatDatum = (datum: Date | string) => {
  return formatNlDateTime(datum);
};

const getStepStatus = (actieOmschrijving: string) => {
  switch (actieOmschrijving) {
    case "afgerond":
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

const logboekSteps = computed<StepProps[]>(() => {
  return mockLogboekData.map((item, index) => ({
    id: item.id,
    title: item.actieOmschrijving,
    status: getStepStatus(item.actieOmschrijving),
    collapsible: true,
    steps: [
      // Compacte meta-informatie
      {
        title: `${formatDatum(item.datum)} - ${item.medewerker}${item.kanaal ? ` via ${item.kanaal}` : ''}`,
        status: "checked"
      },
      // Details alleen als ze bestaan en niet te lang zijn
      ...(item.details ? [{
        title: item.details,
        status: "checked"  
      }] : [])
    ]
  }));
});
</script>

<style lang="scss" scoped>

</style>