<template>
  <utrecht-heading :level="1">Contactverzoek {{ cvId }}</utrecht-heading>
  <router-link to="/">Terug</router-link>
  
  <utrecht-alert v-if="error" appeareance="error">{{ error }}</utrecht-alert>
  <utrecht-alert v-if="success" appeareance="ok">Contactmoment succesvol bijgewerkt</utrecht-alert>
  
  <div class="ita-cv-detail-sections">
    <section>
      <utrecht-heading :level="2">Onderwerp / vraag</utrecht-heading>
      <utrecht-data-list>
        <utrecht-data-list-item>
          <utrecht-data-list-key>Vraag</utrecht-data-list-key>
          <utrecht-data-list-value :value="taak.aanleidinggevendKlantcontact?.onderwerp" multiline
            >{{ taak.aanleidinggevendKlantcontact?.onderwerp }}
          </utrecht-data-list-value>
        </utrecht-data-list-item>
        <utrecht-data-list-item>
          <utrecht-spotlight-section>
            <utrecht-data-list-key>Interne toelichting KCC</utrecht-data-list-key>
            <utrecht-data-list-value :value="taak.toelichting" multiline class="preserve-newline">{{
              taak.toelichting
            }}</utrecht-data-list-value>
          </utrecht-spotlight-section>
        </utrecht-data-list-item>
      </utrecht-data-list>
    </section>
    <section class="contact-data">
      <utrecht-heading :level="2">Gegevens van contact</utrecht-heading>
      <utrecht-data-list>
        <utrecht-data-list-item>
          <utrecht-data-list-key>Klantnaam</utrecht-data-list-key>
          <utrecht-data-list-value :value="taak.betrokkene?.volledigeNaam">{{
            taak.betrokkene?.volledigeNaam
          }}</utrecht-data-list-value>
        </utrecht-data-list-item>
        <utrecht-data-list-item>
          <utrecht-data-list-key>Telefoonnummer</utrecht-data-list-key>
          <utrecht-data-list-value :value="phoneNumber1">{{
            phoneNumber1
          }}</utrecht-data-list-value>
        </utrecht-data-list-item>
        <utrecht-data-list-item>
          <utrecht-data-list-key>Telefoonnummer 2</utrecht-data-list-key>
          <utrecht-data-list-value :value="phoneNumber2">{{
            phoneNumber2
          }}</utrecht-data-list-value>
        </utrecht-data-list-item>
        <utrecht-data-list-item>
          <utrecht-data-list-key>E-mailadres</utrecht-data-list-key>
          <utrecht-data-list-value :value="email">{{ email }}</utrecht-data-list-value>
        </utrecht-data-list-item>
        <utrecht-data-list-item v-for="zaakUuid in zaakUuids" :key="zaakUuid">
          <utrecht-data-list-key>Gekoppelde zaak</utrecht-data-list-key>
          <utrecht-data-list-value :value="zaakUuid">{{ zaakUuid }}</utrecht-data-list-value>
        </utrecht-data-list-item>
        <utrecht-data-list-item>
          <utrecht-data-list-key>Datum aangemaakt</utrecht-data-list-key>
          <utrecht-data-list-value value="x"
            ><date-time-or-nvt :date="taak.aanleidinggevendKlantcontact?.plaatsgevondenOp"
          /></utrecht-data-list-value>
        </utrecht-data-list-item>
        <utrecht-data-list-item>
          <utrecht-data-list-key>Kanaal</utrecht-data-list-key>
          <utrecht-data-list-value :value="taak.aanleidinggevendKlantcontact?.kanaal">{{
            taak.aanleidinggevendKlantcontact?.kanaal
          }}</utrecht-data-list-value>
        </utrecht-data-list-item>
        <utrecht-data-list-item>
          <utrecht-data-list-key>Behandelaar</utrecht-data-list-key>
          <utrecht-data-list-value :value="behandelaar">{{ behandelaar }}</utrecht-data-list-value>
        </utrecht-data-list-item>
        <utrecht-data-list-item>
          <utrecht-data-list-key>Status</utrecht-data-list-key>
          <utrecht-data-list-value :value="taak.status">{{ taak.status }}</utrecht-data-list-value>
        </utrecht-data-list-item>
        <utrecht-data-list-item>
          <utrecht-data-list-key>Aangemaakt door</utrecht-data-list-key>
          <utrecht-data-list-value :value="aangemaaktDoor">{{
            aangemaaktDoor
          }}</utrecht-data-list-value>
        </utrecht-data-list-item>
      </utrecht-data-list>
    </section>

    <section>
      <utrecht-heading :level="2">Contactmoment bijwerken</utrecht-heading>
      <form @submit.prevent="submit">
        <utrecht-fieldset>
          <utrecht-legend>Resultaat</utrecht-legend>
          <utrecht-form-field type="radio">
            <utrecht-radiobutton
              name="contact-gelukt"
              id="contact-gelukt"
              :value="RESULTS.contactGelukt"
              v-model="form.resultaat"
              required
            />
            <utrecht-form-label for="contact-gelukt" type="radio">{{
              RESULTS.contactGelukt
            }}</utrecht-form-label>
          </utrecht-form-field>
          <utrecht-form-field type="radio">
            <utrecht-radiobutton
              name="contact-gelukt"
              id="geen-gehoor"
              :value="RESULTS.geenGehoor"
              v-model="form.resultaat"
              required
            />
            <utrecht-form-label for="geen-gehoor" type="radio">{{
              RESULTS.geenGehoor
            }}</utrecht-form-label>
          </utrecht-form-field>
        </utrecht-fieldset>
        <utrecht-form-field>
          <utrecht-form-label for="kanalen">Kanaal</utrecht-form-label>
          <utrecht-select required id="kanalen" v-model="form.kanaal" :options="kanalen" />
        </utrecht-form-field>
        <utrecht-form-field>
          <utrecht-form-label for="informatie-burger"
            >Informatie voor burger / bedrijf</utrecht-form-label
          >
          <utrecht-textarea
            :required="form.resultaat === RESULTS.contactGelukt"
            id="informatie-burger"
            v-model="form.informatieBurger"
          />
        </utrecht-form-field>
        <utrecht-button 
          type="submit" 
          appearance="primary-action-button"
          :disabled="isLoading"
        >
          <span v-if="isLoading">Bezig met opslaan...</span>
          <span v-else>Opslaan</span>
        </utrecht-button>
      </form>
    </section>

    <section>
 <ContactverzoekContactmomenten :contactverzoekId="cvId" />
    </section>
  </div>
</template>

<script lang="ts" setup>
import DateTimeOrNvt from "@/components/DateTimeOrNvt.vue";
import UtrechtSpotlightSection from "@/components/UtrechtSpotlightSection.vue";
import UtrechtAlert from "@/components/UtrechtAlert.vue";
import { fakeInterneTaken } from "@/helpers/fake-data";
import { computed, ref } from "vue";
import { useRoute } from "vue-router";
import type { Klantcontact } from "@/types/internetaken";
import { klantcontactService, type CreateKlantcontactRequest } from "@/services/createKlantcontactService";


import ContactverzoekContactmomenten from '@/components/ContactverzoekContactmomenten.vue'

const RESULTS = {
  contactGelukt: "Contact opnemen gelukt",
  geenGehoor: "Contact opnemen niet gelukt"
} as const;

const route = useRoute();
const cvId = computed(() => route.params.number);

const isLoading = ref(false);
const error = ref<string | null>(null);
const success = ref(false);

const taak = fakeInterneTaken[0];
computed(() => taak.aanleidinggevendKlantcontact?.uuid || '');
const phoneNumbers = computed(() =>
  taak.digitaleAdress
    ?.filter(({ soortDigitaalAdres }) => soortDigitaalAdres === "telefoonnummer")
    .map(({ adres }) => adres)
);
const phoneNumber1 = computed(() => phoneNumbers.value?.[0]);
const phoneNumber2 = computed(() => phoneNumbers.value?.[1]);
const email = computed(() =>
  taak.digitaleAdress
    ?.filter(({ soortDigitaalAdres }) => soortDigitaalAdres === "email")
    .map(({ adres }) => adres)
    .find(Boolean)
);
const behandelaar = computed(() => taak.toegewezenAanActoren?.map((x) => x.naam).find(Boolean));
const aangemaaktDoor = computed(() =>
  taak.aanleidinggevendKlantcontact?.hadBetrokkenActoren.map((x) => x.naam).find(Boolean)
);
const zaakUuids = computed(() =>
  taak.aanleidinggevendKlantcontact?.gingOverOnderwerpobjecten
    ?.map((x) => x.onderwerpobjectidentificator?.objectId)
    .filter(Boolean)
);
// TODO:
// 1. get zaak from openzaak by uuid
// 2. show the zaaknummer in the data-list
const kanalen = [
  { label: "Selecteer een kanaal", value: "" },
  ...["Balie", "Telefoon"].map((value) => ({ label: value, value }))
];

const form = ref({
  resultaat: RESULTS.contactGelukt as (typeof RESULTS)[keyof typeof RESULTS],
  kanaal: "",
  informatieBurger: ""
});

// In de submit functie in ContactverzoekDetailView.vue
async function submit() {
  error.value = null;
  success.value = false;
  
  if (!form.value.kanaal) {
    error.value = "Kies een kanaal voor het contactmoment";
    return;
  }
  
  if (form.value.resultaat !== RESULTS.geenGehoor && !form.value.informatieBurger) {
    error.value = "Vul informatie voor de burger in";
    return;
  }
  
  isLoading.value = true;
  
  try {
    // Extract partijUuid from the expand data if available
    let partijUuid: string | undefined = undefined;
    
    // Check if we have expand data with hadBetrokkenen
    if (taak.aanleidinggevendKlantcontact?._expand?.hadBetrokkenen?.length > 0) {
      const betrokkene = taak.aanleidinggevendKlantcontact._expand.hadBetrokkenen[0];
      
      // Check if wasPartij is available and has a uuid
      if (betrokkene.wasPartij && 'uuid' in betrokkene.wasPartij) {
        partijUuid = betrokkene.wasPartij.uuid;
        console.log('Using partijUuid from expand data:', partijUuid);
      }
    }
    
    const klantcontactRequest: CreateKlantcontactRequest = {
      kanaal: form.value.kanaal,
      onderwerp: taak.aanleidinggevendKlantcontact?.onderwerp || "Opvolging contactverzoek",
      inhoud: form.value.informatieBurger,
      indicatieContactGelukt: form.value.resultaat === RESULTS.contactGelukt,
      taal: "nld", // ISO 639-2/B formaat
      vertrouwelijk: false,
      plaatsgevondenOp: new Date().toISOString()
    };
    
    await klantcontactService.createRelatedKlantcontact(
      klantcontactRequest,
      taak.aanleidinggevendKlantcontact?.uuid,
      partijUuid
    );
        
    success.value = true;
    form.value = {
      resultaat: RESULTS.contactGelukt,
      kanaal: "",
      informatieBurger: ""
    };
  } catch (err: any) {
    console.error('Error bij aanmaken klantcontact:', err);
    error.value = err.message || 'Er is een fout opgetreden bij het aanmaken van het contactmoment';
  } finally {
    isLoading.value = false;
  }
}
</script>

<style lang="scss" scoped>
.ita-cv-detail-sections {
  --_column-size: 40rem;
  display: grid;
  column-gap: var(--ita-cv-details-sections-column-gap);
  grid-template-columns: repeat(auto-fill, minmax(min(100%, var(--_column-size)), 1fr));
}

.utrecht-data-list__item {
  break-inside: avoid;
}

.contact-data {
  container-type: inline-size;
  .utrecht-data-list {
    gap: 1rem;
    @container (min-width: 25rem) {
      columns: 2;
    }
    @container (min-width: 40rem) {
      columns: 3;
    }
  }
}

.utrecht-form-label {
  display: block;
}

.utrecht-alert {
  margin-bottom: 1rem;
}
</style>
