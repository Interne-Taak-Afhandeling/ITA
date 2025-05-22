<template>
  <contact-timeline v-bind="timeLineProps" :expanded-items="expandedItems" />
</template>

<script setup lang="ts">
import { computed, onWatcherCleanup, ref, watchEffect } from "vue";
import type { Contactmoment, Internetaken } from "@/types/internetaken";
import { klantcontactService } from "@/services/klantcontactService";
import { ContactTimeline, type ContactTimelineProps } from "./denhaag-contact-timeline";
const props = defineProps<{ taak: Internetaken }>();
const isLoading = ref(true);
const error = ref("");
const contactmomenten = ref<Contactmoment[]>([]);

const timeLineProps = computed<ContactTimelineProps>(() => ({
  labels: { today: "Vandaag", yesterday: "Gisteren" },
  collapsible: true,
  items: contactmomenten.value.map((item, index) => ({
    title: item.contactGelukt ? "Contact gelukt" : "Geen gehoor",
    id: index.toString(),
    channel: item.kanaal,
    isoDate: item.datum,
    description: item.tekst,
    sender: item.medewerker
  }))
}));

const expandedItems = computed(() => timeLineProps.value.items.map((x) => x.id));

watchEffect(async () => {
  const controller = new AbortController();
  onWatcherCleanup(() => controller.abort());
  isLoading.value = true;
  error.value = "";

  if (props.taak.aanleidinggevendKlantcontact?.uuid) {
    try {
      const response = await klantcontactService.getContactKeten(
        props.taak.aanleidinggevendKlantcontact.uuid,
        controller.signal
      );

      // Haal de contactmomenten uit de response
      contactmomenten.value = response.contactmomenten;
    } catch (err: unknown) {
      error.value =
        err instanceof Error && err.message
          ? err.message
          : "Er is een fout opgetreden bij het ophalen van de contactmomenten bij dit contactverzoek";
    } finally {
      isLoading.value = false;
    }
  }
});
</script>
