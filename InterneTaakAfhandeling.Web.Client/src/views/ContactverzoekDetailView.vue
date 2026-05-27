<template>
  <div class="ita-cv-detail-header">
    <div>
      <back-link class="back-link" />
    </div>
    <template v-if="taak">
      <utrecht-heading :level="1"
        >Contactverzoek {{ taak.aanleidinggevendKlantcontact?.nummer }}</utrecht-heading
      >
      <utrecht-button-group v-if="!isAfgehandeld">
        <assign-contactverzoek-to-me
          :id="taak.uuid"
          :user-email="userEmail"
          :actoren="taak.toegewezenAanActoren ?? []"
          @assignmentSuccess="fetchInternetaken"
        />
      </utrecht-button-group>
    </template>
  </div>

  <simple-spinner v-if="isLoadingTaak" />

  <utrecht-alert v-else-if="errorMessage" type="error">
    {{ errorMessage }}
  </utrecht-alert>

  <div v-else-if="taak" class="ita-cv-detail-sections">
    <detail-section title="Onderwerp / vraag">
      <contactverzoek-details
        :taak="taak"
        :zaak="taak.zaak"
        :is-afgehandeld="isAfgehandeld"
        @zaak-gekoppeld="handleZaakGekoppeld"
      />
    </detail-section>

    <detail-section
      v-if="taak.aanleidinggevendKlantcontact"
      class="contact-data"
      title="Gegevens van contact"
    >
      <contactmoment-details
        :contactmoment="taak.aanleidinggevendKlantcontact"
        :status="taak.status"
        :actoren="taak.toegewezenAanActoren || []"
      />
    </detail-section>

    <detail-section title="Acties">
      <contactverzoek-acties v-if="!isAfgehandeld" :taak="taak" @success="fetchInternetaken" />

      <utrecht-paragraph v-else class="same-margin-as-datalist" role="alert">
        Dit contactverzoek is afgehandeld en kan niet meer worden gewijzigd.
      </utrecht-paragraph>
    </detail-section>

    <detail-section title="Logboek contactverzoek" class="ita-detail-section--compact">
      <div class="same-margin-as-datalist">
        <contactverzoek-logboek v-if="taak" :taak="taak" />
      </div>
    </detail-section>
  </div>
</template>

<script lang="ts" setup>
import UtrechtAlert from "@/components/UtrechtAlert.vue";
import SimpleSpinner from "@/components/SimpleSpinner.vue";
import BackLink from "@/components/BackLink.vue";
import { computed, onMounted, ref } from "vue";
import ContactverzoekLogboek from "@/components/ContactverzoekLogboek.vue";

import type { Internetaken } from "@/types/internetaken";
import { internetakenService } from "@/services/internetakenService";
import { knownErrorMessages } from "@/utils/fetchWrapper";
import AssignContactverzoekToMe from "@/features/assign-contactverzoek-to-me/AssignContactverzoekToMe.vue";
import ContactverzoekDetails from "@/components/ContactverzoekDetails.vue";
import ContactmomentDetails from "@/components/ContactmomentDetails.vue";
import ContactverzoekActies from "@/components/ContactverzoekActies.vue";
import DetailSection from "@/components/DetailSection.vue";
import { useAuthStore } from "@/stores/auth";

const props = defineProps<{
  contactmomentNumber?: string;
  contactverzoekId?: string;
}>();

const authStore = useAuthStore();

const taak = ref<Internetaken | null>(null);
const isLoadingTaak = ref(false);
const errorMessage = ref<string | null>(null);

const isContactmomentRoute = computed(() => !!props.contactmomentNumber);
const routeNummer = computed(() => props.contactmomentNumber ?? props.contactverzoekId ?? "");
const userEmail = computed(() => authStore.user?.email ?? "");
const isAfgehandeld = computed(() => taak.value?.status === "verwerkt");

const handleZaakGekoppeld = () => {
  fetchInternetaken();
};

onMounted(async () => {
  await fetchInternetaken();
});

const fetchInternetaken = async () => {
  isLoadingTaak.value = true;
  errorMessage.value = null;
  try {
    taak.value = isContactmomentRoute.value
      ? await internetakenService.getByKlantcontactNummer(routeNummer.value)
      : await internetakenService.getInternetaak(routeNummer.value);
  } catch (err: unknown) {
    console.error("Error loading contactverzoek:", err);
    if (err instanceof Error && err.message === knownErrorMessages.notFound) {
      errorMessage.value = "Dit contactverzoek bestaat niet of is niet meer beschikbaar.";
    } else {
      errorMessage.value =
        "Er is iets misgegaan bij het ophalen van dit contactverzoek. Neem contact op met de beheerder als dit probleem zich blijft voordoen.";
    }
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

  > * {
    min-height: 22rem;
  }

  .ita-detail-section--compact {
    align-self: start;
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
