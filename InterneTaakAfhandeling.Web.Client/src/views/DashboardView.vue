<template>
  <utrecht-heading :level="1">Dashboard</utrecht-heading>

  <div class="ita-dashboard-tables">
    <section>
      <utrecht-heading :level="2" id="h2-a">Aan mij toegewezen contacten</utrecht-heading>
      <simple-spinner v-if="isLoading" />
      <my-interne-taken-table v-else :interne-taken="assignedInternetaken" aria-labelledby="h2-a" />
    </section>
  </div>
</template>

<style lang="scss" scoped>
.ita-dashboard-tables {
  display: flex;
  flex-wrap: wrap;
  column-gap: var(--ita-dashboard-tables-column-gap);

  section {
    min-inline-size: 40rem;
  }
}
</style>

<script setup lang="ts">
import { onMounted } from "vue";
import { useUserStore } from "@/stores/user";
import { storeToRefs } from "pinia";
import MyInterneTakenTable from "@/components/interneTakenTables/MyInterneTakenTable.vue";
import SimpleSpinner from "@/components/SimpleSpinner.vue";

const userStore = useUserStore();
const { assignedInternetaken, isLoading } = storeToRefs(userStore);

onMounted(async () => userStore.fetchAssignedInternetaken());
</script>
