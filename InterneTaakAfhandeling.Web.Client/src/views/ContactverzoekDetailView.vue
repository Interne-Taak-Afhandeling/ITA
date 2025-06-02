<template>
  <div class="ita-dv-detail-header">
    <div>
      <back-button />
    </div>
    <utrecht-heading :level="1">Contactverzoek {{ cvId }}</utrecht-heading>
    <utrecht-button-group v-if="taak?.uuid">
      <assign-contactverzoek-to-myself :id="taak.uuid" @assignmentSuccess="fetchInternetaken" />
      <KoppelZaakModal
        v-if="taak?.aanleidinggevendKlantcontact?.uuid"
        :aanleidinggevendKlantcontactUuid="taak.aanleidinggevendKlantcontact.uuid"
        :zaakIdentificatie="taak?.zaak?.identificatie"
        @zaak-gekoppeld="handleZaakGekoppeld"
      />
    </utrecht-button-group>
  </div>

  <simple-spinner v-if="isLoadingTaak" />

  <utrecht-alert v-else-if="!taak && !isLoadingTaak" type="error">
    Dit contactverzoek bestaat niet of is niet meer beschikbaar.
  </utrecht-alert>

  <div v-else-if="taak" class="ita-cv-detail-sections">
    <section>
      <utrecht-heading :level="2">Onderwerp / vraag</utrecht-heading>
      <contactverzoek-details :taak="taak" />
    </section>

    <section v-if="taak.aanleidinggevendKlantcontact" class="contact-data">
      <utrecht-heading :level="2">Gegevens van contact</utrecht-heading>
      <contactmoment-details
        :contactmoment="taak.aanleidinggevendKlantcontact"
        :zaak="taak.zaak"
        :status="taak.status"
        :actoren="taak.toegewezenAanActoren || []"
      />
    </section>

    <section>
      <utrecht-heading :level="2">Contactmoment bijwerken</utrecht-heading>
      <contactmoment-registreren :taak="taak" @success="fetchInternetaken" />
    </section>

    <section>
      <utrecht-heading :level="2"> Contactmomenten</utrecht-heading>
      <contactverzoek-contactmomenten v-if="taak" :taak="taak" />
    </section>
  </div>
</template>

<script lang="ts" setup>
import UtrechtAlert from "@/components/UtrechtAlert.vue";
import SimpleSpinner from "@/components/SimpleSpinner.vue";
import BackButton from "@/components/BackButton.vue";
import { computed, onMounted, ref } from "vue";
import { useRoute } from "vue-router";
import ContactverzoekContactmomenten from "@/components/ContactverzoekContactmomenten.vue";
import type { Internetaken, Zaak } from "@/types/internetaken";
import { internetakenService } from "@/services/internetakenService";
import AssignContactverzoekToMyself from "@/features/assign-contactverzoek-to-myself/AssignContactverzoekToMyself.vue";
import KoppelZaakModal from "@/components/KoppelZaakModal.vue";
import ContactverzoekDetails from "@/components/ContactverzoekDetails.vue";
import ContactmomentDetails from "@/components/ContactmomentDetails.vue";
import ContactmomentRegistreren from "@/components/ContactmomentRegistreren.vue";

const first = (v: string | string[]) => (Array.isArray(v) ? v[0] : v);
const route = useRoute();
const cvId = computed(() => first(route.params.number));
const isLoadingTaak = ref(false);
const taak = ref<Internetaken | null>(null);

const handleZaakGekoppeld = (zaak: Zaak) => {
  if (taak.value) {
    taak.value.zaak = zaak;
  }
};

onMounted(async () => {
  await fetchInternetaken();
});

const fetchInternetaken = async () => {
  isLoadingTaak.value = true;
  try {
    taak.value = await internetakenService.getInternetaak({
      Nummer: String(cvId.value)
    });
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
  grid-template-columns: repeat(auto-fill, minmax(min(100%, var(--_column-size)), 1fr));
}

.ita-dv-detail-header {
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
</style>