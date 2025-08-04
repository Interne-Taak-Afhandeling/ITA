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
    <utrecht-data-list-item>
      <utrecht-data-list-key :id="labelId">
        Gekoppelde zaak

        <koppel-zaak-modal
          v-if="taak?.aanleidinggevendKlantcontact?.uuid"
          :aanleidinggevendKlantcontactUuid="taak.aanleidinggevendKlantcontact.uuid"
          :zaakIdentificatie="taak?.zaak?.identificatie"
          :internetaak-id="taak.uuid"
          :label-id="labelId"
          @zaak-gekoppeld="$emit(`zaakGekoppeld`)"
        />
      </utrecht-data-list-key>
      <utrecht-data-list-value :value="zaak?.identificatie">
        {{ zaak?.identificatie }}
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
import { useId } from "vue";
import InterneToelichtingSection from "./InterneToelichtingSection.vue";
import type { Internetaken, Zaak } from "@/types/internetaken";
import KoppelZaakModal from "@/components/KoppelZaakModal.vue";

defineProps<{ taak: Internetaken; zaak: Zaak | undefined }>();

const labelId = useId();
</script>

<style scoped>
.fullwidth {
  grid-template-columns: auto;
}
</style>
