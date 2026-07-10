<template>
  <utrecht-table aria-labelledby="h2-alle-contactverzoeken">
    <utrecht-table-caption v-if="$slots.caption">
      <slot name="caption"></slot>
    </utrecht-table-caption>
    <utrecht-table-header>
      <utrecht-table-row>
        <utrecht-table-header-cell scope="col">Datum</utrecht-table-header-cell>
        <utrecht-table-header-cell scope="col">Klantnaam</utrecht-table-header-cell>
        <utrecht-table-header-cell scope="col">Onderwerp / vraag</utrecht-table-header-cell>
        <utrecht-table-header-cell scope="col">Urgentie</utrecht-table-header-cell>
        <utrecht-table-header-cell scope="col">Afdeling</utrecht-table-header-cell>
        <utrecht-table-header-cell scope="col">Behandelaar</utrecht-table-header-cell>
        <utrecht-table-header-cell scope="col">Klantcontactnummer</utrecht-table-header-cell>
        <utrecht-table-header-cell scope="col" class="details-cell"
          >Details</utrecht-table-header-cell
        >
      </utrecht-table-row>
    </utrecht-table-header>

    <utrecht-table-body>
      <utrecht-table-row v-if="interneTaken.length === 0">
        <utrecht-table-cell colspan="8">Geen contactverzoeken gevonden</utrecht-table-cell>
      </utrecht-table-row>

      <utrecht-table-row
        v-for="taak in interneTaken"
        :key="taak.uuid"
        @mousedown="onRowMouseDown"
        @click="
          taak.contactmomentNummer &&
          navigateOnRowClick($event, `/contactmoment/${taak.contactmomentNummer}`)
        "
      >
        <utrecht-table-cell class="ita-no-wrap">
          <date-time-or-nvt :date="taak.contactDatum || taak.toegewezenOp" />
        </utrecht-table-cell>
        <utrecht-table-cell class="text-truncate" :title="taak.klantNaam || ''">
          {{ taak.klantNaam || "-" }}
        </utrecht-table-cell>
        <utrecht-table-cell
          class="text-truncate"
          :title="taak.onderwerp || taak.gevraagdeHandeling || ''"
        >
          {{ taak.onderwerp || taak.gevraagdeHandeling || "-" }}
        </utrecht-table-cell>
        <utrecht-table-cell>
          <urgentie-badge :urgentie="taak.urgentie" />
        </utrecht-table-cell>
        <utrecht-table-cell class="text-truncate" :title="taak.afdelingNaam || ''">
          {{ taak.afdelingNaam || "-" }}
        </utrecht-table-cell>
        <utrecht-table-cell class="text-truncate" :title="taak.behandelaarNaam || ''">
          {{ taak.behandelaarNaam || "-" }}
        </utrecht-table-cell>
        <utrecht-table-cell>
          {{ taak.contactmomentNummer || "-" }}
        </utrecht-table-cell>
        <utrecht-table-cell class="details-cell">
          <router-link
            v-if="taak.contactmomentNummer"
            :to="`/contactmoment/${taak.contactmomentNummer}`"
            :aria-label="`Open contactverzoek ${taak.klantNaam || taak.onderwerp || taak.contactmomentNummer}`"
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
import DateTimeOrNvt from "../DateTimeOrNvt.vue";
import UrgentieBadge from "../UrgentieBadge.vue";
import type { InterneTaakOverviewItem } from "@/types/internetaken";

defineProps<{ interneTaken: InterneTaakOverviewItem[] }>();

const { onRowMouseDown, navigateOnRowClick } = useRowNavigation();


</script>
