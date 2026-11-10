<template>
  <utrecht-table aria-labelledby="h2-a">
    <utrecht-table-header>
      <utrecht-table-row>
        <utrecht-table-header-cell scope="col">Datum</utrecht-table-header-cell>
        <utrecht-table-header-cell scope="col">Klantnaam</utrecht-table-header-cell>
        <utrecht-table-header-cell scope="col">Onderwerp / vraag</utrecht-table-header-cell>
        <utrecht-table-header-cell scope="col">Urgentie</utrecht-table-header-cell>
        <utrecht-table-header-cell scope="col">Afdeling</utrecht-table-header-cell>
        <utrecht-table-header-cell scope="col">Klantcontactnummer</utrecht-table-header-cell>
        <utrecht-table-header-cell scope="col" class="details-cell"
          >Details</utrecht-table-header-cell
        >
      </utrecht-table-row>
    </utrecht-table-header>

    <utrecht-table-body>
      <utrecht-table-row v-if="interneTaken.length === 0">
        <utrecht-table-cell colspan="7">Geen interne taken gevonden</utrecht-table-cell>
      </utrecht-table-row>

      <utrecht-table-row
        v-for="taak in interneTaken"
        :key="taak.uuid"
        @mousedown="onRowMouseDown"
        @click="
          taak.aanleidinggevendKlantcontact?.nummer &&
          navigateOnRowClick($event, `/contactmoment/${taak.aanleidinggevendKlantcontact.nummer}`)
        "
      >
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
        <utrecht-table-cell class="text-truncate" :title="taak.afdelingNaam || ''">
          {{ taak.afdelingNaam || "-" }}
        </utrecht-table-cell>
        <utrecht-table-cell>
          {{ taak.aanleidinggevendKlantcontact?.nummer || "-" }}
        </utrecht-table-cell>
        <utrecht-table-cell class="details-cell">
          <router-link
            v-if="taak.aanleidinggevendKlantcontact?.nummer"
            :to="`/contactmoment/${taak.aanleidinggevendKlantcontact.nummer}`"
            :aria-label="`Open contactverzoek ${getKlantLabel(taak)}`"
            class="details-link"
            @click.stop
            >→</router-link
          >
          <span v-else>-</span>
        </utrecht-table-cell>
      </utrecht-table-row>
    </utrecht-table-body>
  </utrecht-table>
</template>

<script setup lang="ts">
import { useRowNavigation } from "@/composables/use-row-navigation";
import type { MyInterneTaakOverviewItem } from "@/types/internetaken";
import DateTimeOrNvt from "../DateTimeOrNvt.vue";
import UrgentieBadge from "../UrgentieBadge.vue";

defineProps<{ interneTaken: MyInterneTaakOverviewItem[] }>();

const { onRowMouseDown, navigateOnRowClick } = useRowNavigation();

function getKlantLabel(taak: MyInterneTaakOverviewItem): string {
  const contact = taak.aanleidinggevendKlantcontact;
  return (
    contact?._expand?.hadBetrokkenen?.map((x) => x.volledigeNaam).find(Boolean) ||
    contact?.onderwerp ||
    contact?.nummer ||
    ""
  );
}
</script>
