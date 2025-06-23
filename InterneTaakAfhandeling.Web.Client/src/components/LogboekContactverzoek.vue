<template>
  <status
    :steps="mockLogboekSteps"
    :collapsible="true"
    v-model:expanded-steps="expandedSteps"
  >
    <template #title="{ title }">
      {{ title }}
    </template>
  </status>
</template>

<script setup lang="ts">
import { ref } from "vue";
import { Status } from "@/components/denhaag-process-steps";
import type { StepProps } from "@/components/denhaag-process-steps/types";

const props = defineProps<{ taak: any }>();

const expandedSteps = ref<string[]>([]);

// Mock data voor verschillende logboek entries
const mockLogboekSteps: StepProps[] = [
  {
    id: "entry-1",
    title: "Contact gelukt",
    status: "checked",
    collapsible: true,
    steps: [{
      title: "23-06-2025 14:30 • Jan Janssen • Telefoon\n\nKlant geïnformeerd over de status van de aanvraag. Aanvullende documenten zijn niet nodig.",
      status: "checked"
    }]
  },
  {
    id: "entry-2", 
    title: "Zaak gekoppeld",
    status: "checked",
    collapsible: true,
    steps: [{
      title: "23-06-2025 10:15 • Jan Janssen\n\nZaak Z2025-001234 gekoppeld aan dit contactverzoek.",
      status: "checked"
    }]
  },
  {
    id: "entry-3",
    title: "Opgepakt", 
    status: "checked",
    collapsible: true,
    steps: [{
      title: "23-06-2025 09:45 • Jan Janssen\n\nContactverzoek toegewezen aan mezelf voor verdere afhandeling.",
      status: "checked"
    }]
  },
  {
    id: "entry-4",
    title: "Geen gehoor",
    status: "checked", 
    collapsible: true,
    steps: [{
      title: "22-06-2025 16:20 • Maria Pietersen • Telefoon\n\nGeprobeerd te bellen maar geen gehoor. Voicemail ingesproken.",
      status: "checked"
    }]
  },
  {
    id: "entry-5",
    title: "Contact gelukt",
    status: "checked",
    collapsible: true, 
    steps: [{
      title: "22-06-2025 14:10 • Maria Pietersen • E-mail\n\nBevestigingsmail gestuurd naar klant met referentienummer en vervolgstappen.",
      status: "checked"
    }]
  }
];

// Set all items as expanded by default for demo purposes
expandedSteps.value = mockLogboekSteps.map(step => step.id);
</script>

<style lang="scss" scoped>
// Custom styling for logbook entries
:deep(.denhaag-process-steps) {
  --denhaag-process-steps-step-distance: 1.5rem;
  
  .denhaag-process-steps__step-heading {
    font-weight: 600;
    font-size: 1rem;
  }
  
  .denhaag-process-steps__step-meta {
    color: var(--utrecht-color-grey-60);
    font-size: 0.875rem;
  }
}
</style>