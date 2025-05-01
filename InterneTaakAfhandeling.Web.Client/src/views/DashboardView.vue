<template>
  <utrecht-heading :level="1">Dashboard</utrecht-heading>

  <div class="ita-dashboard-tables">
    <section>
      <utrecht-heading :level="2" id="h2-a">Aan mij toegewezen contacten</utrecht-heading>
      <interne-taak-table
        v-if="!isLoading"
        :interne-taken="assignedInternetaken"
        aria-labelledby="h2-a"
      />
    </section>

    <section>
      <utrecht-heading :level="2" id="h2-b">Oudste contacten voor afdeling</utrecht-heading>
      <interne-taak-table v-if="!isLoading" :interne-taken="staticRecords" aria-labelledby="h2-b" />
    </section>
  </div>
</template>

<style lang="scss" scoped>
.ita-dashboard-tables {
  display: flex;
  flex-wrap: wrap;
  column-gap: var(--ita-dashboard-tables-column-gap);

  section {
    flex: 1;
  }
}
</style>

<script setup lang="ts">
import { onMounted } from "vue";
import { useUserStore } from "@/stores/user";
import { storeToRefs, type _DeepPartial } from "pinia";
import InterneTaakTable from "@/components/InterneTaakTable.vue";
import type { Internetaken } from "@/types/internetaken";
const userStore = useUserStore();
const { assignedInternetaken, isLoading } = storeToRefs(userStore);

const _staticRecords: _DeepPartial<Internetaken>[] = [
  {
    uuid: "1",
    aanleidinggevendKlantcontact: { plaatsgevondenOp: "2025-01-01", onderwerp: "Lorem ipsum" },
    betrokkene: { volledigeNaam: "Saskia Swart" }
  },
  {
    uuid: "2",
    aanleidinggevendKlantcontact: { plaatsgevondenOp: "2025-01-02", onderwerp: "Dolor sit amet" },
    betrokkene: { volledigeNaam: "Martijn de Groot" }
  }
];
const staticRecords = _staticRecords as unknown as Internetaken[];

onMounted(() => {
  userStore.fetchAssignedInternetaken();
});
</script>
