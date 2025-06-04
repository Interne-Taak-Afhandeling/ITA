<template>
  <utrecht-heading :level="1">Dashboard</utrecht-heading>

  <div class="ita-dashboard-tables">
    <section>
      <utrecht-heading :level="2" id="h2-a">Aan mij toegewezen contacten</utrecht-heading>
      <simple-spinner v-if="isLoading" />
      <scroll-container v-else>
        <my-interne-taken-table :interne-taken="assignedInternetaken" aria-labelledby="h2-a" />
      </scroll-container>
    </section>
  </div>
</template>

<style lang="scss" scoped>
.ita-dashboard-tables {
  display: flex;
  flex-wrap: wrap;
  column-gap: var(--ita-dashboard-tables-column-gap);

  > * {
    flex: 1;
    inline-size: 100%;
  }
}
</style>

<script setup lang="ts">
import { onMounted } from "vue";
import { useUserStore } from "@/stores/user";
import { storeToRefs } from "pinia";
import MyInterneTakenTable from "@/components/interne-taken-tables/MyInterneTakenTable.vue";
import SimpleSpinner from "@/components/SimpleSpinner.vue";
import ScrollContainer from "@/components/ScrollContainer.vue";

const userStore = useUserStore();
const { assignedInternetaken, isLoading } = storeToRefs(userStore);

onMounted(async () => {
  await userStore.fetchAssignedInternetaken();
});
</script>
