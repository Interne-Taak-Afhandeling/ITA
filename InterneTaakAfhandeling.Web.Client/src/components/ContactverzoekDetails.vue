<template>
  <utrecht-data-list>
    <utrecht-data-list-item>
      <utrecht-data-list-key>Vraag</utrecht-data-list-key>
      <utrecht-data-list-value :value="taak.aanleidinggevendKlantcontact?.onderwerp" multiline>
        {{ taak.aanleidinggevendKlantcontact?.onderwerp }}
      </utrecht-data-list-value>
    </utrecht-data-list-item>
    <utrecht-data-list-item>
      <utrecht-data-list-key>Informatie voor burger / bedrijf</utrecht-data-list-key>
      <utrecht-data-list-value
        :value="taak.aanleidinggevendKlantcontact?.inhoud"
        multiline
        class="preserve-newline"
      >
        {{ taak.aanleidinggevendKlantcontact?.inhoud }}
      </utrecht-data-list-value>
    </utrecht-data-list-item>
    <utrecht-data-list-item class="fullwidth">
      <interne-toelichting-section>
        <utrecht-data-list-key>Interne toelichting KCC</utrecht-data-list-key>
        <utrecht-data-list-value :value="taak.toelichting" multiline class="preserve-newline">
          {{ taak.toelichting }}
        </utrecht-data-list-value>
      </interne-toelichting-section>
    </utrecht-data-list-item>
  </utrecht-data-list>
</template>

<script setup lang="ts">
import InterneToelichtingSection from "./InterneToelichtingSection.vue";
import type { Internetaken } from "@/types/internetaken";
defineProps<{ taak: Internetaken }>();
</script>
<style scoped>
.fullwidth {
  grid-template-columns: auto;
}
</style>
