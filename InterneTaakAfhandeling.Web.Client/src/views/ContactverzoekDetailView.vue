<template>
  <div class="contactverzoek-header">
    <utrecht-heading :level="1">Contactverzoek {{ cvId }}</utrecht-heading>
    <div class="contactverzoek-actions">
      <KoppelZaakModal 
        v-if="taak?.aanleidinggevendKlantcontact?.uuid" 
        :aanleidinggevendKlantcontactUuid="taak.aanleidinggevendKlantcontact.uuid"
        :zaakIdentificatie="taak?.zaak?.identificatie"
        @zaak-gekoppeld="handleZaakGekoppeld"
      />
    </div>
  </div>
  
  <div class="back-link">
    <router-link to="/" class="utrecht-link">Terug</router-link>
  </div>

  <utrecht-alert v-if="error" appeareance="error">{{ error }}</utrecht-alert>
  <utrecht-alert v-if="success" appeareance="ok">Contactmoment succesvol bijgewerkt</utrecht-alert>

  <div class="ita-cv-detail-sections">
    <section v-if="taak">
      <utrecht-heading :level="2">Onderwerp / vraag</utrecht-heading>
      <utrecht-data-list>
        <utrecht-data-list-item>
          <utrecht-data-list-key>Vraag</utrecht-data-list-key>
          <utrecht-data-list-value :value="taak?.aanleidinggevendKlantcontact?.onderwerp" multiline>
            {{ taak?.aanleidinggevendKlantcontact?.onderwerp }}
          </utrecht-data-list-value>
        </utrecht-data-list-item>
        <utrecht-data-list-item>
          <utrecht-data-list-key>Informatie voor burger / bedrijf</utrecht-data-list-key>
          <utrecht-data-list-value
            :value="taak?.aanleidinggevendKlantcontact?.inhoud"
            multiline
            class="preserve-newline"
          >
            {{ taak?.aanleidinggevendKlantcontact?.inhoud }}
          </utrecht-data-list-value>
        </utrecht-data-list-item>
        <utrecht-data-list-item>
          <utrecht-spotlight-section>
            <utrecht-data-list-key>Interne toelichting KCC</utrecht-data-list-key>
            <utrecht-data-list-value :value="taak?.toelichting" multiline class="preserve-newline">
              {{ taak?.toelichting }}
            </utrecht-data-list-value>
          </utrecht-spotlight-section>
        </utrecht-data-list-item>
      </utrecht-data-list>
    </section>
    <section v-if="taak" class="contact-data">
      <utrecht-heading :level="2">Gegevens van contact</utrecht-heading>
      <utrecht-data-list>
        <utrecht-data-list-item>
          <utrecht-data-list-key>Klantnaam</utrecht-data-list-key>
          <utrecht-data-list-value v-title-on-overflow :value="klantNaam">
            {{ klantNaam }}
          </utrecht-data-list-value>
        </utrecht-data-list-item>
        <utrecht-data-list-item v-if="organisatienaam" class="ita-break-before-avoid">
          <utrecht-data-list-key>Organisatie</utrecht-data-list-key>
          <utrecht-data-list-value v-title-on-overflow :value="organisatienaam">
            {{ organisatienaam }}
          </utrecht-data-list-value>
        </utrecht-data-list-item>
        <utrecht-data-list-item>
          <utrecht-data-list-key>{{
            phoneNumber1?.omschrijving || "Telefoonnummer"
          }}</utrecht-data-list-key>
          <utrecht-data-list-value :value="phoneNumber1?.adres">{{
            phoneNumber1?.adres
          }}</utrecht-data-list-value>
        </utrecht-data-list-item>
        <utrecht-data-list-item class="ita-break-before-avoid" v-if="phoneNumber2?.adres">
          <utrecht-data-list-key>{{ phoneNumber2.omschrijving }}</utrecht-data-list-key>
          <utrecht-data-list-value :value="phoneNumber2.adres">{{
            phoneNumber2.adres
          }}</utrecht-data-list-value>
        </utrecht-data-list-item>
        <utrecht-data-list-item class="ita-break-before-avoid">
          <utrecht-data-list-key>E-mailadres</utrecht-data-list-key>
          <utrecht-data-list-value :value="email" v-title-on-overflow>{{
            email
          }}</utrecht-data-list-value>
        </utrecht-data-list-item>
        <utrecht-data-list-item>
          <utrecht-data-list-key>Gekoppelde zaak</utrecht-data-list-key>
          <utrecht-data-list-value v-title-on-overflow :value="taak?.zaak?.identificatie">{{
            taak?.zaak?.identificatie
          }}</utrecht-data-list-value>
        </utrecht-data-list-item>
        <utrecht-data-list-item>
          <utrecht-data-list-key>Datum aangemaakt</utrecht-data-list-key>
          <utrecht-data-list-value value="x"
            ><date-time-or-nvt :date="taak?.aanleidinggevendKlantcontact?.plaatsgevondenOp"
          /></utrecht-data-list-value>
        </utrecht-data-list-item>
        <utrecht-data-list-item>
          <utrecht-data-list-key>Kanaal</utrecht-data-list-key>
          <utrecht-data-list-value
            :value="taak?.aanleidinggevendKlantcontact?.kanaal"
            v-title-on-overflow
            >{{ taak?.aanleidinggevendKlantcontact?.kanaal }}</utrecht-data-list-value
          >
        </utrecht-data-list-item>
        <utrecht-data-list-item>
          <utrecht-data-list-key>Behandelaar</utrecht-data-list-key>
          <utrecht-data-list-value v-title-on-overflow :value="behandelaar">{{
            behandelaar
          }}</utrecht-data-list-value>
        </utrecht-data-list-item>
        <utrecht-data-list-item>
          <utrecht-data-list-key>Status</utrecht-data-list-key>
          <utrecht-data-list-value :value="taak?.status">{{
            taak?.status
          }}</utrecht-data-list-value>
        </utrecht-data-list-item>
        <utrecht-data-list-item>
          <utrecht-data-list-key>Aangemaakt door</utrecht-data-list-key>
          <utrecht-data-list-value v-title-on-overflow :value="aangemaaktDoor">{{
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
        <utrecht-button type="submit" appearance="primary-action-button" :disabled="isLoading">
          <span v-if="isLoading">Bezig met opslaan...</span>
          <span v-else>Opslaan</span>
        </utrecht-button>
      </form>
    </section>

    <section>
      <ContactverzoekContactmomenten :contactmomentNummer="cvId" />
    </section>
  </div>
</template>

<script lang="ts" setup>
import DateTimeOrNvt from "@/components/DateTimeOrNvt.vue";
import UtrechtSpotlightSection from "@/components/UtrechtSpotlightSection.vue";
import UtrechtAlert from "@/components/UtrechtAlert.vue";
import { computed, onMounted, ref } from "vue";
import { useRoute } from "vue-router";
import {
  klantcontactService,
  type CreateKlantcontactRequest
} from "@/services/klantcontactService";
import ContactverzoekContactmomenten from "@/components/ContactverzoekContactmomenten.vue";
import type { Internetaken, Zaak } from "@/types/internetaken";
import { internetakenService } from "@/services/internetakenService";
import { vTitleOnOverflow } from "@/directives/v-title-on-overflow";
import KoppelZaakModal from "@/components/KoppelZaakModal.vue";

const RESULTS = {
  contactGelukt: "Contact opnemen gelukt",
  geenGehoor: "Contact opnemen niet gelukt"
} as const;

const first = (v: string | string[]) => (Array.isArray(v) ? v[0] : v);
const route = useRoute();
const cvId = computed(() => first(route.params.number));

const isLoading = ref(false);
const error = ref<string | null>(null);
const success = ref(false);

const taak = ref<Internetaken | null>(null);

const handleZaakGekoppeld = (zaak: Zaak) => {
  if (taak.value) {
    taak.value.zaak = zaak;
  }
};

onMounted(async () => {
  taak.value = await internetakenService.getInternetaak({
    Nummer: String(cvId.value)
  });
});

const pascalCase = (s: string | undefined) =>
  !s ? s : `${s[0].toLocaleUpperCase()}${s.substring(1) || ""}`;

const phoneNumbers = computed(
  () =>
    taak.value?.aanleidinggevendKlantcontact?._expand?.hadBetrokkenen?.[0]?._expand?.digitaleAdressen
      ?.filter(
        ({ soortDigitaalAdres }: { soortDigitaalAdres?: string }) =>
          soortDigitaalAdres === "telefoonnummer"
      )
      .filter((x) => x.adres)
      .map(({ adres, omschrijving }, i) => ({
        adres,
        omschrijving: pascalCase(omschrijving) || `Telefoonnummer ${i + 1}`
      })) || []
);

const phoneNumber1 = computed(() =>
  phoneNumbers.value.length > 0 ? phoneNumbers.value[0] : undefined
);
const phoneNumber2 = computed(() =>
  phoneNumbers.value.length > 1 ? phoneNumbers.value[1] : undefined
);
const email = computed(
  () =>
    taak.value?.aanleidinggevendKlantcontact?._expand?.hadBetrokkenen?.[0]?._expand?.digitaleAdressen
      ?.filter(
        ({ soortDigitaalAdres }: { soortDigitaalAdres?: string }) => soortDigitaalAdres === "email"
      )
      .map(({ adres }: { adres?: string }) => adres || "")
      .find(Boolean) || ""
);
const behandelaar = computed(
  () => taak.value?.toegewezenAanActoren?.map((x) => x.naam).find(Boolean) || ""
);
const aangemaaktDoor = computed(
  () =>
    taak.value?.aanleidinggevendKlantcontact?.hadBetrokkenActoren
      ?.map((x) => x.naam)
      .find(Boolean) || ""
);

const klantNaam = computed(() =>
  taak.value?.aanleidinggevendKlantcontact?._expand?.hadBetrokkenen
    ?.map((x) => x.volledigeNaam || x.organisatienaam)
    .find(Boolean)
);

const organisatienaam = computed(() =>
  taak.value?.aanleidinggevendKlantcontact?._expand?.hadBetrokkenen
    ?.map((x) => x.organisatienaam)
    .filter((x) => x !== klantNaam.value)
    .find(Boolean)
);

const kanalen = [
  { label: "Selecteer een kanaal", value: "" },
  ...["Balie", "Telefoon"].map((value) => ({ label: value, value }))
];

const form = ref({
  resultaat: RESULTS.contactGelukt as (typeof RESULTS)[keyof typeof RESULTS],
  kanaal: "",
  informatieBurger: ""
});

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
    const createRequest: CreateKlantcontactRequest = {
      kanaal: form.value.kanaal,
      onderwerp: taak.value?.aanleidinggevendKlantcontact?.onderwerp || "Opvolging contactverzoek",
      inhoud: form.value.informatieBurger,
      indicatieContactGelukt: form.value.resultaat === RESULTS.contactGelukt,
      taal: "nld", // ISO 639-2/B formaat
      vertrouwelijk: false,
      plaatsgevondenOp: new Date().toISOString()
    };

    let partijUuid: string | undefined = undefined;

    if (taak.value?.aanleidinggevendKlantcontact?._expand?.hadBetrokkenen?.[0]) {
      const betrokkene = taak.value.aanleidinggevendKlantcontact._expand.hadBetrokkenen[0];

      if (betrokkene._expand?.wasPartij && "uuid" in betrokkene._expand.wasPartij) {
        partijUuid = betrokkene._expand.wasPartij.uuid;
        console.log("Using partijUuid from expand.wasPartij:", partijUuid);
      }
      // Als fallback, check ook direct in wasPartij
      else if (betrokkene.wasPartij && "uuid" in betrokkene.wasPartij) {
        partijUuid = betrokkene.wasPartij.uuid;
      }
    }

    const aanleidinggevendKlantcontactUuid = taak.value?.aanleidinggevendKlantcontact?.uuid;

    await klantcontactService.createRelatedKlantcontact(
      createRequest,
      aanleidinggevendKlantcontactUuid,
      partijUuid
    );

    success.value = true;
    form.value = {
      resultaat: RESULTS.contactGelukt,
      kanaal: "",
      informatieBurger: ""
    };
  } catch (err: unknown) {
    console.error("Error bij aanmaken klantcontact:", err);
    error.value =
      err instanceof Error && err.message
        ? err.message
        : "Er is een fout opgetreden bij het aanmaken van het contactmoment";
  } finally {
    isLoading.value = false;
  }
}
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

.contact-data {
  container-type: inline-size;

  .utrecht-data-list {
    gap: 2rem;
    max-width: fit-content;

    @container (min-width: 35rem) {
      columns: 2;
    }

    @container (min-width: 42rem) {
      columns: 3;
    }
  }

  .utrecht-data-list__item {
    break-inside: avoid;
    display: block;
  }

  .utrecht-data-list__item-key {
    inline-size: max-content;
  }

  .utrecht-data-list__item-value {
    text-overflow: ellipsis;
    white-space: nowrap;
    overflow: hidden;

    &[title]:not([title=""]) {
      user-select: all;
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