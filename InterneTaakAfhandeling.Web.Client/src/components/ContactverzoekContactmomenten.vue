<template>
  <contact-timeline v-bind="timeLineProps" />
</template>

<script setup lang="ts">
import { computed } from "vue";
import type { Internetaken } from "@/types/internetaken";
import { klantcontactService } from "@/services/klantcontactService";
import { ContactTimeline, type ContactTimelineProps } from "./denhaag-contact-timeline";
import { useLoader } from "@/composables/use-loader";
const props = defineProps<{ taak: Internetaken }>();

const { data: contactmomenten } = useLoader((signal) => {
  if (props.taak.aanleidinggevendKlantcontact?.uuid) {
    return klantcontactService.getContactKeten(
      props.taak.aanleidinggevendKlantcontact.uuid,
      signal
    );
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
