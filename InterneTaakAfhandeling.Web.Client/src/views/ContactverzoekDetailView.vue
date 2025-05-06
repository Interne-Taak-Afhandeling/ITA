<template>
  <utrecht-heading :level="1">Contactverzoek {{ cvId }}</utrecht-heading>
  <router-link to="/">Terug</router-link>
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
      <utrecht-heading :level="2">Contactmoment maken</utrecht-heading>
      <form @submit.prevent="submit">
        <utrecht-fieldset>
          <utrecht-legend>Resultaat</utrecht-legend>
          <utrecht-form-field type="radio">
            <utrecht-radiobutton
              name="contact-gelukt"
              id="contact-gelukt"
              :value="RESULTS.contactGelukt"
              v-model="form.resultaat"
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
          <utrecht-textarea required id="informatie-burger" v-model="form.informatieBurger" />
        </utrecht-form-field>
        <utrecht-button type="submit" appearance="primary-action-button">Opslaan</utrecht-button>
      </form>
    </section>
  </div>
</template>

<script lang="ts" setup>
import DateTimeOrNvt from "@/components/DateTimeOrNvt.vue";
import UtrechtSpotlightSection from "@/components/UtrechtSpotlightSection.vue";
import { fakeInterneTaken } from "@/helpers/fake-data";
import { computed, ref } from "vue";
import { useRoute } from "vue-router";
const RESULTS = {
  contactGelukt: "Contact opnemen gelukt",
  geenGehoor: "Contact opnemen niet gelukt"
} as const;

const route = useRoute();
const cvId = computed(() => route.params.number);

const taak = fakeInterneTaken[0];
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
// 2. get the deeplink url config from an environment variable (same as in KISS)
// 3. build a deeplink to the zaak using the zaaknummer of the zaak and the deeplink url config (same as in KISS)
// 4. show the zaaknummer as a link in the data-list

const kanalen = [
  { label: "Selecteer een kanaal", value: "" },
  ...["Balie", "Telefoon"].map((value) => ({ label: value, value }))
];

const form = ref({
  resultaat: RESULTS.contactGelukt,
  kanaal: "",
  informatieBurger: ""
});

const submit = () => alert("submit!");
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
</style>
