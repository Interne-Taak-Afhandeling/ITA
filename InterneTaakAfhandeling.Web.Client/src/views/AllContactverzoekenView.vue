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

    <div class="pagination-controls bottom" v-if="totalCount > pageSize">
      <utrecht-button
        appearance="secondary-action-button"
        @click="previousPage"
        :disabled="currentPage <= 1 || isLoading"
      >
        Vorige
      </utrecht-button>

      <div class="page-numbers">
        <button
          v-for="pageNum in visiblePages"
          :key="pageNum"
          @click="goToPage(pageNum)"
          :class="['page-number', { active: pageNum === currentPage }]"
          :disabled="isLoading"
        >
          {{ pageNum }}
        </button>
      </div>

      <span class="page-info"> ({{ getItemRange() }} van {{ totalCount }} items) </span>

      <utrecht-button
        appearance="secondary-action-button"
        @click="nextPage"
        :disabled="!hasNextPage || isLoading"
      >
        Volgende
      </utrecht-button>
    </div>
  </section>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from "vue";
import SimpleSpinner from "@/components/SimpleSpinner.vue";
import UtrechtAlert from "@/components/UtrechtAlert.vue";
import { get } from "@/utils/fetchWrapper";
import type { InterneTaakOverviewItem } from "@/components/interneTakenTables/AllInterneTakenTable.vue";
import AllInterneTakenTable from "@/components/interneTakenTables/AllInterneTakenTable.vue";
import { useBackNavigation } from "@/composables/useBackNavigation";

interface InterneTakenOverviewResponse {
  count: number;
  next?: string;
  previous?: string;
  results: InterneTaakOverviewItem[];
}

const { setPreviousRoute } = useBackNavigation();

const isLoading = ref(false);
const error = ref<string | null>(null);
const results = ref<InterneTaakOverviewItem[]>([]);
const totalCount = ref(0);
const currentPage = ref(1);
const pageSize = ref(20);
const hasNextPage = ref(false);
const hasPreviousPage = ref(false);
const totalPages = computed(() => Math.ceil(totalCount.value / pageSize.value));
const visiblePages = computed(() => {
  const total = totalPages.value;
  const current = currentPage.value;
  const pages: number[] = [];

  if (total <= 5) {
    for (let i = 1; i <= total; i++) {
      pages.push(i);
    }
  } else {
    let start = Math.max(1, current - 2);
    let end = Math.min(total, current + 2);

    if (end - start < 4) {
      if (start === 1) {
        end = Math.min(total, start + 4);
      } else {
        start = Math.max(1, end - 4);
      }
    }

    for (let i = start; i <= end; i++) {
      pages.push(i);
    }
  }

  return pages;
});

const fetchInterneTaken = async (page: number = 1) => {
  isLoading.value = true;
  error.value = null;

  try {
    const response = await get<InterneTakenOverviewResponse>("/api/internetaken-overview", {
      page: page,
      pageSize: pageSize.value
    });

    // Update state
    results.value = response.results;
    totalCount.value = response.count;
    currentPage.value = page;
    hasNextPage.value = !!response.next;
    hasPreviousPage.value = !!response.previous;
  } catch (err: unknown) {
    const message =
      err instanceof Error
        ? err.message
        : "Er is een fout opgetreden bij het ophalen van contactverzoeken";
    error.value = message;
  } finally {
    isLoading.value = false;
  }
};

const goToPage = (page: number) => {
  if (page !== currentPage.value && !isLoading.value && page >= 1 && page <= totalPages.value) {
    fetchInterneTaken(page);
  }
};

const nextPage = () => {
  if (hasNextPage.value && !isLoading.value) {
    fetchInterneTaken(currentPage.value + 1);
  }
};

const previousPage = () => {
  if (hasPreviousPage.value && !isLoading.value && currentPage.value > 1) {
    fetchInterneTaken(currentPage.value - 1);
  }
};

const getItemRange = () => {
  const start = (currentPage.value - 1) * pageSize.value + 1;
  const end = Math.min(currentPage.value * pageSize.value, totalCount.value);
  return `${start}-${end}`;
};

onMounted(() => {
  setPreviousRoute('alleContactverzoeken');
  fetchInterneTaken();
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

.pagination-controls {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 1rem;
  margin: 1rem 0;

  &.bottom {
    margin-top: 2rem;
  }
}

.page-numbers {
  display: flex;
  gap: 0.5rem;
  margin: 0 1rem;
}

.page-number {
  padding: 0.5rem 0.75rem;
  border: 1px solid #ccc;
  background: white;
  color: #333;
  border-radius: 4px;
  cursor: pointer;
  font-weight: 500;
  min-width: 40px;
  text-align: center;
  transition: all 0.2s ease;

  &:hover:not(:disabled) {
    background: #f5f5f5;
    border-color: #999;
  }

  &.active {
    background: #0070f3;
    color: white;
    border-color: #0070f3;
    cursor: default;
  }

  &:disabled {
    cursor: not-allowed;
    opacity: 0.5;
  }
}

.page-info {
  font-weight: 500;
  white-space: nowrap;
  padding: 0 1rem;
}

section {
  margin-top: 1rem;
}
</style>