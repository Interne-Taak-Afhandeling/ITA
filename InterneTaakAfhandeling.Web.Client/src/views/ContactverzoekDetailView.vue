<template>
  <div class="ita-cv-detail-header">
    <div>
      <back-link class="back-link" />
    </div>
    <utrecht-heading :level="1">Contactverzoek {{ cvId }}</utrecht-heading>
    <utrecht-button-group v-if="taak?.uuid">
      <assign-contactverzoek-to-me :id="taak.uuid" @assignmentSuccess="fetchInternetaken" />
      <KoppelZaakModal
        v-if="taak?.aanleidinggevendKlantcontact?.uuid"
        :aanleidinggevendKlantcontactUuid="taak.aanleidinggevendKlantcontact.uuid"
        :zaakIdentificatie="taak?.zaak?.identificatie"
        :internetaak-id="taak.uuid"
        @zaak-gekoppeld="handleZaakGekoppeld"
      />
    </utrecht-button-group>
  </div>

  <simple-spinner v-if="isLoadingTaak" />

  <utrecht-alert v-else-if="!taak && !isLoadingTaak" type="error">
    Dit contactverzoek bestaat niet of is niet meer beschikbaar.
  </utrecht-alert>

  <div v-else-if="taak" class="ita-cv-detail-sections">
    <detail-section title="Onderwerp / vraag">
      <contactverzoek-details :taak="taak" />
    </detail-section>

    <detail-section
      v-if="taak.aanleidinggevendKlantcontact"
      class="contact-data"
      title="Gegevens van contact"
    >
      <contactmoment-details
        :contactmoment="taak.aanleidinggevendKlantcontact"
        :zaak="taak.zaak"
        :status="taak.status"
        :actoren="taak.toegewezenAanActoren || []"
      />
    </detail-section>

    <detail-section title="Contactmoment registreren">
      <div class="same-margin-as-datalist">
        <contactmoment-registreren :taak="taak" @success="fetchInternetaken" />
      </div>
    </detail-section>

    <detail-section title="Logboek contactverzoek">
      <div class="same-margin-as-datalist">
        <contactverzoek-logboek v-if="taak" :key="logboekRefreshKey" :taak="taak" />
      </div>
    </detail-section>
  </div>
</template>

<script lang="ts" setup>
import UtrechtAlert from "@/components/UtrechtAlert.vue";
import SimpleSpinner from "@/components/SimpleSpinner.vue";
import BackLink from "@/components/BackLink.vue";
import { computed, onMounted, ref } from "vue";
import { useRoute } from "vue-router";
import ContactverzoekLogboek from "@/components/ContactverzoekLogboek.vue";

import type { Internetaken, Zaak } from "@/types/internetaken";
import { internetakenService } from "@/services/internetakenService";
import AssignContactverzoekToMe from "@/features/assign-contactverzoek-to-me/AssignContactverzoekToMe.vue";
import KoppelZaakModal from "@/components/KoppelZaakModal.vue";
import ContactverzoekDetails from "@/components/ContactverzoekDetails.vue";
import ContactmomentDetails from "@/components/ContactmomentDetails.vue";
import ContactmomentRegistreren from "@/components/ContactmomentRegistreren.vue";
import DetailSection from "@/components/DetailSection.vue";

const first = (v: string | string[]) => (Array.isArray(v) ? v[0] : v);
const route = useRoute();
const cvId = computed(() => first(route.params.number));
const isLoadingTaak = ref(false);
const taak = ref<Internetaken | null>(null);
const logboekRefreshKey = ref(0);

const handleZaakGekoppeld = (zaak: Zaak) => {
  if (taak.value) {
    taak.value.zaak = zaak;
  }

  logboekRefreshKey.value++;
};

onMounted(async () => {
  await fetchInternetaken();
});

const fetchInternetaken = async () => {
  isLoadingTaak.value = true;
  try {
    taak.value = await internetakenService.getInternetaak(cvId.value);
  } catch (err: unknown) {
    console.error("Error loading contactverzoek:", err);
  } finally {
    isLoadingTaak.value = false;
  }
};
</script>

<style lang="scss" scoped>
.contactverzoek-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1rem;
}

.back-link {
  margin-bottom: 1.5rem;
}

.ita-cv-detail-sections {
  --_column-size: 42rem;
  display: grid;
  column-gap: var(--ita-cv-details-sections-column-gap);
  row-gap: var(--ita-cv-details-sections-row-gap);
  grid-template-columns: repeat(auto-fill, minmax(min(100%, var(--_column-size)), 1fr));
  align-items: start;

  > * {
    min-height: 22rem;
  }
}

.ita-cv-detail-header {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  justify-content: space-between;

  > :first-child {
    min-inline-size: 100%;
  }
}

.contact-data {
  container-type: inline-size;
}

.same-margin-as-datalist {
  margin-block-end: calc(
    var(--utrecht-space-around, 0) * var(--utrecht-data-list-margin-block-end, 0)
  );
  margin-block-start: calc(
    var(--utrecht-space-around, 0) * var(--utrecht-data-list-margin-block-start, 0)
  );
}

.ita-cv-detail-sections :deep(.denhaag-contact-timeline__step:last-child) {
  padding-block-end: 0;
}
</style>