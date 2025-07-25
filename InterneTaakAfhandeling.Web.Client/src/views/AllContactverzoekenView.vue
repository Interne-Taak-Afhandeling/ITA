<template>
  <utrecht-heading :level="1">Alle contactverzoeken</utrecht-heading>

  <div v-if="isLoading" class="spinner-container">
    <simple-spinner />
  </div>

  <utrecht-alert v-else-if="error" type="error">
    {{ error }}
  </utrecht-alert>

  <section v-else>
    <scroll-container>
      <all-interne-taken-table :interneTaken="results">
        <template #caption v-if="itemRange">
          {{ itemRange.start }} tot {{ itemRange.end }} van {{ totalCount }} contactverzoeken
        </template>
      </all-interne-taken-table>
    </scroll-container>

    <utrecht-pagination
      v-if="totalPages > 1"
      :current-page="currentPage"
      :total-pages="totalPages"
      :has-next-page="hasNextPage"
      :has-previous-page="hasPreviousPage"
      :visible-pages="visiblePages"
      :is-loading="isLoading"
      @go-to-page="goToPage"
      @go-to-previous-page="goToPreviousPage"
      @go-to-next-page="goToNextPage"
    />
  </section>
</template>

<script setup lang="ts">
import { onMounted } from "vue";
import SimpleSpinner from "@/components/SimpleSpinner.vue";
import UtrechtAlert from "@/components/UtrechtAlert.vue";
import UtrechtPagination from "@/components/UtrechtPagination.vue";
import { get } from "@/utils/fetchWrapper";
import AllInterneTakenTable from "@/components/interne-taken-tables/AllInterneTakenTable.vue";
import { usePagination } from "@/composables/use-pagination";
import ScrollContainer from "@/components/ScrollContainer.vue";
import type { InterneTakenPaginated } from "@/types/internetaken";

const fetchInterneTaken = async (
  page: number,
  pageSize: number
): Promise<InterneTakenPaginated> => {
  return await get<InterneTakenPaginated>("/api/internetaken", {
    page,
    pageSize
  });
};

const {
  isLoading,
  error,
  results,
  totalCount,
  currentPage,
  totalPages,
  hasNextPage,
  hasPreviousPage,
  visiblePages,
  itemRange,
  fetchData,
  goToPage,
  goToNextPage,
  goToPreviousPage
} = usePagination(fetchInterneTaken, {
  initialPage: 1,
  initialPageSize: 20,
  maxVisiblePages: 5
});

onMounted(() => {
  fetchData();
});
</script>

<style lang="scss" scoped>
.spinner-container {
  display: flex;
  justify-content: center;
  align-items: center;
  min-height: 200px;
}

section {
  margin-top: 1rem;
  display: grid;
  justify-items: center;
  gap: 1rem;
}
</style>
