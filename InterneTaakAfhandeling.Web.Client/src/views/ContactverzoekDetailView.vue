<template>
  <utrecht-heading :level="1">Contactverzoek {{ cvId }}</utrecht-heading>
  <router-link to="/">Terug</router-link>
  <div class="ita-cv-detail-sections">
    <section class="vraag">
      <utrecht-heading :level="2">Onderwerp / vraag</utrecht-heading>
      <utrecht-data-list>
        <utrecht-data-list-item>
          <utrecht-data-list-key>Vraag</utrecht-data-list-key>
          <utrecht-data-list-value value="x" multiline>
            Loterij organiseren - Mevrouw Moulin wil graag weten hoe ze een loterij organiseert
          </utrecht-data-list-value>
        </utrecht-data-list-item>
        <utrecht-data-list-item>
          <utrecht-spotlight-section>
            <utrecht-data-list-key>Interne toelichting KCC</utrecht-data-list-key>
            <utrecht-data-list-value value="x" multiline>
              Niet op woensdagmiddag bellen.<br />
              Is de loterij voor een goed doel?; Ja<br />
              Is de locatie openbaar?; Nee, het is bij Mevrouw Moulin thuis.
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
          <utrecht-data-list-value value="x">Suzanne Moulin</utrecht-data-list-value>
        </utrecht-data-list-item>
        <utrecht-data-list-item>
          <utrecht-data-list-key>Telefoonnummer</utrecht-data-list-key>
          <utrecht-data-list-value value="x">02012345678</utrecht-data-list-value>
        </utrecht-data-list-item>
        <utrecht-data-list-item>
          <utrecht-data-list-key>Telefoonnummer 2</utrecht-data-list-key>
          <utrecht-data-list-value></utrecht-data-list-value>
        </utrecht-data-list-item>
        <utrecht-data-list-item>
          <utrecht-data-list-key>E-mailadres</utrecht-data-list-key>
          <utrecht-data-list-value></utrecht-data-list-value>
        </utrecht-data-list-item>
        <utrecht-data-list-item>
          <utrecht-data-list-key>Gekoppelde zaak</utrecht-data-list-key>
          <utrecht-data-list-value value="x">ZAAK-2025-XXXXYZ</utrecht-data-list-value>
        </utrecht-data-list-item>
        <utrecht-data-list-item>
          <utrecht-data-list-key>Datum aangemaakt</utrecht-data-list-key>
          <utrecht-data-list-value value="x"
            ><date-time-or-nvt date="2025-01-01T11:54"
          /></utrecht-data-list-value>
        </utrecht-data-list-item>
        <utrecht-data-list-item>
          <utrecht-data-list-key>Kanaal</utrecht-data-list-key>
          <utrecht-data-list-value value="x">Telefoon</utrecht-data-list-value>
        </utrecht-data-list-item>
        <utrecht-data-list-item>
          <utrecht-data-list-key>Behandelaar</utrecht-data-list-key>
          <utrecht-data-list-value value="x">Piet van Gelre</utrecht-data-list-value>
        </utrecht-data-list-item>
        <utrecht-data-list-item>
          <utrecht-data-list-key>Status</utrecht-data-list-key>
          <utrecht-data-list-value value="x">Te verwerken</utrecht-data-list-value>
        </utrecht-data-list-item>
        <utrecht-data-list-item>
          <utrecht-data-list-key>Aangemaakt door</utrecht-data-list-key>
          <utrecht-data-list-value value="x">Amber Gerritsen</utrecht-data-list-value>
        </utrecht-data-list-item>
      </utrecht-data-list>
    </section>
  </div>
</template>

<script lang="ts" setup>
import DateTimeOrNvt from "@/components/DateTimeOrNvt.vue";
import UtrechtSpotlightSection from "@/components/UtrechtSpotlightSection.vue";
import { computed } from "vue";
import { useRoute } from "vue-router";
const route = useRoute();
const cvId = computed(() => route.params.number);
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
    @container (min-width: 25rem) {
      columns: 2;
    }
    @container (min-width: 40rem) {
      columns: 3;
    }
  }
}

.utrecht-data-list:has(.utrecht-spotlight-section)
  .utrecht-data-list__item:not(:has(.utrecht-spotlight-section)) {
  padding-block-end: var(--utrecht-spotlight-section-padding-block-end);
  padding-block-start: var(--utrecht-spotlight-section-padding-block-start);
  padding-inline-end: var(--utrecht-spotlight-section-padding-inline-end);
  padding-inline-start: var(--utrecht-spotlight-section-padding-inline-start);
}
</style>
