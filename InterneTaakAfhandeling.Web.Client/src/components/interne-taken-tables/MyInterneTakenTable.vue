<template>
  <utrecht-table aria-labelledby="h2-a">
    <utrecht-table-header>
      <utrecht-table-row>
        <utrecht-table-header-cell scope="col">Datum</utrecht-table-header-cell>
        <utrecht-table-header-cell scope="col">Klantnaam</utrecht-table-header-cell>
        <utrecht-table-header-cell scope="col">Onderwerp / vraag</utrecht-table-header-cell>
        <utrecht-table-header-cell scope="col">Urgentie</utrecht-table-header-cell>
        <utrecht-table-header-cell scope="col">Details</utrecht-table-header-cell>
      </utrecht-table-row>
    </utrecht-table-header>

    <utrecht-table-body>
      <utrecht-table-row v-if="interneTaken.length === 0">
        <utrecht-table-cell colspan="5">Geen interne taken gevonden</utrecht-table-cell>
      </utrecht-table-row>

      <utrecht-table-row v-for="taak in interneTaken" :key="taak.uuid">
        <utrecht-table-cell class="ita-no-wrap">
          <date-time-or-nvt :date="taak.aanleidinggevendKlantcontact?.plaatsgevondenOp" />
        </utrecht-table-cell>
        <utrecht-table-cell>{{
          taak.aanleidinggevendKlantcontact?._expand?.hadBetrokkenen
            ?.map((x) => x.volledigeNaam)
            .find(Boolean)
        }}</utrecht-table-cell>
        <utrecht-table-cell>{{ taak.aanleidinggevendKlantcontact?.onderwerp }}</utrecht-table-cell>
        <utrecht-table-cell>
          <urgentie-badge :urgentie="taak.urgentie" />
        </utrecht-table-cell>
        <utrecht-table-cell>
          <router-link :to="`/contactmoment/${taak?.aanleidinggevendKlantcontact?.nummer}`"
            >Klik hier</router-link
          >
        </utrecht-table-cell>
      </utrecht-table-row>
    </utrecht-table-body>
  </utrecht-table>
</template>

<script setup lang="ts">
import type { MyInterneTaakOverviewItem } from "@/types/internetaken";
import DateTimeOrNvt from "../DateTimeOrNvt.vue";
import UrgentieBadge from "../UrgentieBadge.vue";

defineProps<{ interneTaken: MyInterneTaakOverviewItem[] }>();
</script>
