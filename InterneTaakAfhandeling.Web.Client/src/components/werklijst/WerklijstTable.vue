<template>
  <utrecht-table>
    <utrecht-table-caption v-if="$slots.caption">
      <slot name="caption"></slot>
    </utrecht-table-caption>
    <utrecht-table-header>
      <utrecht-table-row>
        <utrecht-table-header-cell scope="col">Datum</utrecht-table-header-cell>
        <utrecht-table-header-cell scope="col">Onderwerp</utrecht-table-header-cell>
        <utrecht-table-header-cell scope="col">Afdeling / groep</utrecht-table-header-cell>
        <utrecht-table-header-cell scope="col">Medewerker</utrecht-table-header-cell>
        <utrecht-table-header-cell scope="col">Kanaal</utrecht-table-header-cell>
        <utrecht-table-header-cell scope="col">Details</utrecht-table-header-cell>
      </utrecht-table-row>
    </utrecht-table-header>

    <utrecht-table-body>
      <utrecht-table-row v-if="items.length === 0">
        <utrecht-table-cell colspan="6">Geen contactverzoeken gevonden</utrecht-table-cell>
      </utrecht-table-row>

      <utrecht-table-row v-for="item in items" :key="item.uuid">
        <utrecht-table-cell class="ita-no-wrap">
          <date-time-or-nvt :date="item.plaatsgevondenOp" />
        </utrecht-table-cell>
        <utrecht-table-cell class="text-truncate" :title="item.onderwerp || ''">
          {{ item.onderwerp || "-" }}
        </utrecht-table-cell>
        <utrecht-table-cell class="text-truncate" :title="afdelingGroepLabel(item)">
          {{ afdelingGroepLabel(item) }}
        </utrecht-table-cell>
        <utrecht-table-cell class="text-truncate" :title="item.medewerker || ''">
          {{ item.medewerker || "-" }}
        </utrecht-table-cell>
        <utrecht-table-cell>
          {{ item.kanaal || "-" }}
        </utrecht-table-cell>
        <utrecht-table-cell>
          <router-link :to="`/contactverzoek/${item.nummer}`">Klik hier</router-link>
        </utrecht-table-cell>
      </utrecht-table-row>
    </utrecht-table-body>
  </utrecht-table>
</template>

<script setup lang="ts">
import DateTimeOrNvt from "../DateTimeOrNvt.vue";
import type { WerklijstOverzichtItem } from "@/types/werklijst";

defineProps<{ items: WerklijstOverzichtItem[] }>();

const afdelingGroepLabel = (item: WerklijstOverzichtItem): string => {
  const parts = [item.afdeling, item.groep].filter(Boolean);
  return parts.length > 0 ? parts.join(" / ") : "-";
};
</script>

<style lang="scss" scoped>
.text-truncate {
  max-width: 200px;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}
</style>
