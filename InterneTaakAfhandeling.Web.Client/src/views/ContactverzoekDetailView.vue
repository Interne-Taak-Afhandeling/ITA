<template>
  <utrecht-heading :level="1">Contactverzoek {{ cvId }}</utrecht-heading>
  <router-link to="/">Terug</router-link>
  <div class="ita-cv-detail-sections" v-if="taak">
    <section>
      <utrecht-heading :level="2">Onderwerp / vraag</utrecht-heading>
      <utrecht-data-list>
        <utrecht-data-list-item>
          <utrecht-data-list-key>Vraag</utrecht-data-list-key>
          <utrecht-data-list-value :value="taak?.aanleidinggevendKlantcontact?.onderwerp" multiline>
            {{ taak?.aanleidinggevendKlantcontact?.onderwerp }}
          </utrecht-data-list-value>
        </utrecht-data-list-item>
        <utrecht-data-list-item>
          <utrecht-spotlight-section>
            <utrecht-data-list-key>Interne toelichting KCC</utrecht-data-list-key>
            <utrecht-data-list-value :value="taak?.toelichting" multiline class="preserve-newline">
              {{
                taak?.toelichting
              }}
            </utrecht-data-list-value>
          </utrecht-spotlight-section>
        </utrecht-data-list-item>
      </utrecht-data-list>
    </section>
    <section class="contact-data">
      <utrecht-heading :level="2">Gegevens van contact</utrecht-heading>
      <utrecht-data-list>
        <utrecht-data-list-item>
          <utrecht-data-list-key>Klantnaam</utrecht-data-list-key>
          <utrecht-data-list-value
            :value="taak?.aanleidinggevendKlantcontact?._expand?.hadBetrokkenen?.[0]?.volledigeNaam">
            {{
              taak?.aanleidinggevendKlantcontact?._expand?.hadBetrokkenen?.[0]?.volledigeNaam
            }}
          </utrecht-data-list-value>
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
        <utrecht-data-list-item>
          <utrecht-data-list-key>Gekoppelde zaak</utrecht-data-list-key>
          <utrecht-data-list-value :value="taak?.zaak?.identificatie">{{ taak?.zaak?.identificatie
          }}</utrecht-data-list-value>
        </utrecht-data-list-item>
        <utrecht-data-list-item>
          <utrecht-data-list-key>Datum aangemaakt</utrecht-data-list-key>
          <utrecht-data-list-value value="x"><date-time-or-nvt
              :date="taak?.aanleidinggevendKlantcontact?.plaatsgevondenOp" /></utrecht-data-list-value>
        </utrecht-data-list-item>
        <utrecht-data-list-item>
          <utrecht-data-list-key>Kanaal</utrecht-data-list-key>
          <utrecht-data-list-value :value="taak?.aanleidinggevendKlantcontact?.kanaal">{{
            taak?.aanleidinggevendKlantcontact?.kanaal
          }}</utrecht-data-list-value>
        </utrecht-data-list-item>
        <utrecht-data-list-item>
          <utrecht-data-list-key>Behandelaar</utrecht-data-list-key>
          <utrecht-data-list-value :value="behandelaar">{{ behandelaar }}</utrecht-data-list-value>
        </utrecht-data-list-item>
        <utrecht-data-list-item>
          <utrecht-data-list-key>Status</utrecht-data-list-key>
          <utrecht-data-list-value :value="taak?.status">{{ taak?.status }}</utrecht-data-list-value>
        </utrecht-data-list-item>
        <utrecht-data-list-item>
          <utrecht-data-list-key>Aangemaakt door</utrecht-data-list-key>
          <utrecht-data-list-value :value="aangemaaktDoor">{{
            aangemaaktDoor
          }}</utrecht-data-list-value>
        </utrecht-data-list-item>
      </utrecht-data-list>
    </section>
  </div>
</template>

<script lang="ts" setup>
import DateTimeOrNvt from "@/components/DateTimeOrNvt.vue";
import UtrechtSpotlightSection from "@/components/UtrechtSpotlightSection.vue";

import { computed, onMounted, ref } from "vue";
import { useRoute } from "vue-router";
import { internetakenService } from '@/services/internetaken'
import type { Internetaken, Actor } from '@/types/internetaken';


const route = useRoute();
const cvId = computed(() => route.params.number);


let taak = ref<Internetaken | null>(null);

onMounted(async () => {
  taak.value = await internetakenService.getInternetaak({ 
    Klantcontact_Nummer: String(cvId.value)
  });
});

const phoneNumbers = computed(() =>
  taak.value?.aanleidinggevendKlantcontact?._expand?.hadBetrokkenen?.[0]?.digitaleAdressen
    ?.filter(({ soortDigitaalAdres }: { soortDigitaalAdres?: string }) => soortDigitaalAdres === "telefoonnummer")
    .map(({ adres }: { adres?: string }) => adres || '') || []
);
const phoneNumber1 = computed(() => phoneNumbers.value?.[0]);
const phoneNumber2 = computed(() => phoneNumbers.value?.[1]);
const email = computed(() =>
  taak.value?.aanleidinggevendKlantcontact?._expand?.hadBetrokkenen?.[0]?.digitaleAdressen
    ?.filter(({ soortDigitaalAdres }: { soortDigitaalAdres?: string }) => soortDigitaalAdres === "email")
    .map(({ adres }: { adres?: string }) => adres || '')
    .find(Boolean) || ''
);
const behandelaar = computed(() => taak.value?.toegewezenAanActoren?.map((x) => x.naam).find(Boolean) || '');
const aangemaaktDoor = computed(() =>
  taak.value?.aanleidinggevendKlantcontact?.hadBetrokkenActoren?.map((x: Actor) => x.naam).find(Boolean) || ''
);

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
</style>
