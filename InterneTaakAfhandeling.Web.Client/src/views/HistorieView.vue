<template>
  <utrecht-heading :level="1">{{ route.meta.title }}</utrecht-heading>

  <div class="ita-dashboard-tables">
    <section>
      <utrecht-heading :level="2" id="h2-a">Mijn afgeronde contactverzoeken</utrecht-heading>
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
import { onMounted, ref } from "vue";
import MyInterneTakenTable from "@/components/interne-taken-tables/MyInterneTakenTable.vue";
import SimpleSpinner from "@/components/SimpleSpinner.vue";
import ScrollContainer from "@/components/ScrollContainer.vue";
import { userService } from "@/services/userService";
import type { Internetaken } from "@/types/internetaken";
import { useRoute } from "vue-router";

const route = useRoute();

const assignedInternetaken = ref<Internetaken[]>([]);
const isLoading = ref(false);
const error = ref<string | null>(null);

const fetchAssignedAndFinishedInternetaken = async () => {
  isLoading.value = true;
  error.value = null;

  try {
    assignedInternetaken.value = await userService.getAssignedAndFinishedInternetaken();
  } catch (err: unknown) {
    error.value =
      err instanceof Error && err.message
        ? err.message
        : "Er is een fout opgetreden bij het ophalen van de interne taken";
    console.error("Error fetching internetaken:", err);
  } finally {
    isLoading.value = false;
  }
};

onMounted(async () => {
  await fetchAssignedAndFinishedInternetaken();
});
</script>
