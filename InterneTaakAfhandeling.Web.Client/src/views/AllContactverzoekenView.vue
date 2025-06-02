<template>
  <utrecht-heading :level="1">Alle contactverzoeken</utrecht-heading>

  <div v-if="isLoading" class="spinner-container">
    <simple-spinner />
  </div>

  <utrecht-alert v-else-if="error" type="error">
    {{ error }}
  </utrecht-alert>

  <section v-else>
    <all-interne-taken-table :interneTaken="results" />

    <div v-if="totalCount > 0" class="page-info-container">
      <span class="page-info">
        {{ getItemRange }} van {{ totalCount }} items
      </span>
    </div>

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
import type { InterneTaakOverviewItem } from "@/components/interneTakenTables/AllInterneTakenTable.vue";
import AllInterneTakenTable from "@/components/interneTakenTables/AllInterneTakenTable.vue";
import { useBackNavigation } from "@/composables/useBackNavigation";
import { usePagination } from "@/composables/usePagination";

interface InterneTakenOverviewResponse {
  count: number;
  next?: string;
  previous?: string;
  results: InterneTaakOverviewItem[];
}

const { setPreviousRoute } = useBackNavigation();

const fetchInterneTaken = async (page: number, pageSize: number): Promise<InterneTakenOverviewResponse> => {
  return await get<InterneTakenOverviewResponse>("/api/internetaken-overview", {
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
  getItemRange,
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
  setPreviousRoute('alleContactverzoeken');
  fetchData();
});
</script>

<style lang="scss" scoped>
.text-truncate {
  max-width: 200px;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.spinner-container {
  display: flex;
  justify-content: center;
  align-items: center;
  min-height: 200px;
}

.page-info-container {
  display: flex;
  justify-content: center;
  margin: var(--utrecht-space-block-sm, 0.75rem) 0;
}

.page-info {
  font-weight: 500;
  color: var(--utrecht-color-grey-70);
  font-size: var(--utrecht-font-size-sm, 0.875rem);
  white-space: nowrap;
  padding: 0 1rem;
}

section {
  margin-top: 1rem;
}

</style>