<template>
  <div v-if="loading" class="spinner-container">
    <simple-spinner />
  </div>
  <utrecht-alert v-else-if="error" appeareance="error" class="margin-top">
    Er ging iets mis. Probeer het later opnieuw.
  </utrecht-alert>
  <contact-timeline v-else v-bind="timeLineProps">
    <template #date="{ isoDate }">
      <date-time-or-nvt :date="isoDate" />
    </template>
  </contact-timeline>
</template>

<script setup lang="ts">
import { computed } from "vue";
import type { Internetaken } from "@/types/internetaken";
import { klantcontactService } from "@/services/klantcontactService";
import { ContactTimeline, type ContactTimelineProps } from "./denhaag-contact-timeline";
import { useLoader } from "@/composables/use-loader";
import SimpleSpinner from "./SimpleSpinner.vue";
import UtrechtAlert from "./UtrechtAlert.vue";
import DateTimeOrNvt from "./DateTimeOrNvt.vue";

const props = defineProps<{ taak: Internetaken }>();
const {
  data: contactmomenten,
  loading,
  error
} = useLoader((signal) => {
  if (props.taak.aanleidinggevendKlantcontact?.uuid) {
    return klantcontactService.getContactKeten(
      props.taak.aanleidinggevendKlantcontact.uuid,
      signal
    );
    //todo replace with
    // return klantcontactService.getLogboek(props.taak.uuid, signal);
  }
});

const timeLineProps = computed<ContactTimelineProps>(() => ({
  labels: { today: "Vandaag", yesterday: "Gisteren" },
  collapsible: true,
  items:
    contactmomenten.value?.contactmomenten.map(
      ({ uuid, contactGelukt, kanaal, datum, tekst, medewerker }) => ({
        title: contactGelukt ? "Contact gelukt" : "Geen gehoor",
        id: uuid,
        channel: kanaal,
        isoDate: datum,
        description: tekst,
        sender: medewerker
      })
    ) ?? [],
  expandedItems: contactmomenten.value?.contactmomenten.map(({ uuid }) => uuid) ?? []
}));
</script>
