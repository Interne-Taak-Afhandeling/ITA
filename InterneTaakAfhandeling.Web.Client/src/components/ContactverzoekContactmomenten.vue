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
    <template #description="{ description, interneNotitie }">
      <div>
        <p v-if="description">{{ description }}</p>
        <div v-if="interneNotitie" class="interne-notitie">
          <strong>Interne notitie:</strong>
          <p>{{ interneNotitie }}</p>
        </div>
      </div>
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
  data: logboekActiviteiten,
  loading,
  error
} = useLoader((signal) => {
  if (props.taak.aanleidinggevendKlantcontact?.uuid) {
    return klantcontactService.getLogboek(props.taak.uuid, signal);
  }
});

const timeLineProps = computed<ContactTimelineProps>(() => ({
  labels: { today: "Vandaag", yesterday: "Gisteren" },
  collapsible: true,
  items:
    logboekActiviteiten.value?.map(({ kanaal, datum, tekst, contactGelukt, id, medewerker, type, interneNotitie }) => {
      let title = "Onbekende actie";
      
      if (type === "klantcontact") {
        title = contactGelukt ? "Contact gelukt" : "Geen gehoor";
      } else if (type === "interne_notitie") {
        title = "Interne notitie toegevoegd";
      }

      return {
        title,
        id: id,
        channel: kanaal ?? "onbekend",
        isoDate: datum,
        description: tekst,
        sender: medewerker,
        interneNotitie: interneNotitie
      };
    }) ?? [],
  expandedItems: logboekActiviteiten.value?.map(({ id }) => id) ?? []
}));
</script>
