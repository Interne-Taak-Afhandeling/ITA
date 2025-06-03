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
        <utrecht-table-header-cell scope="col">Afdeling</utrecht-table-header-cell>
        <utrecht-table-header-cell scope="col">Behandelaar</utrecht-table-header-cell>
        <utrecht-table-header-cell scope="col">Details</utrecht-table-header-cell>
      </utrecht-table-row>
    </utrecht-table-header>

    <utrecht-table-body>
      <utrecht-table-row v-if="interneTaken.length === 0">
        <utrecht-table-cell colspan="6">Geen contactverzoeken gevonden</utrecht-table-cell>
      </utrecht-table-row>

      <utrecht-table-row v-for="taak in interneTaken" :key="taak.uuid">
        <utrecht-table-cell>
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
        <utrecht-table-cell class="text-truncate" :title="taak.afdelingNaam || ''">
          {{ taak.afdelingNaam || "-" }}
        </utrecht-table-cell>
        <utrecht-table-cell class="text-truncate" :title="taak.behandelaarNaam || ''">
          {{ taak.behandelaarNaam || "-" }}
        </utrecht-table-cell>
        <utrecht-table-cell>
          <router-link :to="`/contactverzoek/${taak.nummer}`">Klik hier</router-link>
        </utrecht-table-cell>
      </utrecht-table-row>
    </utrecht-table-body>
  </utrecht-table>
</template>

<script setup lang="ts">
import DateTimeOrNvt from "../DateTimeOrNvt.vue";
defineProps<{ interneTaken: InterneTaakOverviewItem[] }>();
export interface InterneTaakOverviewItem {
  uuid: string;
  nummer: string;
  gevraagdeHandeling: string;
  status: string;
  toegewezenOp: string;
  afgehandeldOp?: string;
  onderwerp?: string;
  klantNaam?: string;
  contactDatum?: string;
  afdelingNaam?: string;
  behandelaarNaam?: string;
  heeftBehandelaar: boolean;
}
</script>

<style lang="scss" scoped>
.text-truncate {
  max-width: 200px;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}
</style>
