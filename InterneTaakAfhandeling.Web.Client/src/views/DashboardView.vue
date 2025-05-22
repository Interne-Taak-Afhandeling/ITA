<template>
  <utrecht-heading :level="1">Dashboard</utrecht-heading>

  <div class="ita-dashboard-tables">
    <section>
      <utrecht-heading :level="2" id="h2-a">Aan mij toegewezen contacten</utrecht-heading>
      <simple-spinner v-if="isLoading" />
      <interne-taak-table
        v-else
        :interne-taken="assignedInternetaken"
        aria-labelledby="h2-a"
      />
    </section>

    <section>
      <utrecht-heading :level="2" id="h2-b">Oudste contacten voor afdeling</utrecht-heading>
      <simple-spinner v-if="isLoadingAfdelingContacten" />
      <interne-taak-table
        v-else
        :interne-taken="fakeInterneTaken"
        aria-labelledby="h2-b"
      />
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
import { onMounted, ref } from "vue";
import { useUserStore } from "@/stores/user";
import { storeToRefs } from "pinia";
import InterneTaakTable from "@/components/InterneTaakTable.vue";
import SimpleSpinner from "@/components/SimpleSpinner.vue";
import { fakeInterneTaken } from "@/helpers/fake-data";

const userStore = useUserStore();
const { assignedInternetaken, isLoading } = storeToRefs(userStore);
const isLoadingAfdelingContacten = ref(true);

onMounted(async () => {
  userStore.fetchAssignedInternetaken();
  
  // Simulate loading time for afdeling contacten (since it's fake data)
  // In a real implementation, you would fetch this data from an API
  setTimeout(() => {
    isLoadingAfdelingContacten.value = false;
  }, 1000);
});
</script>