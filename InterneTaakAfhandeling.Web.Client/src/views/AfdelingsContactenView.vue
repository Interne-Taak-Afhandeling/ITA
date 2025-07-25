<template>
  <utrecht-heading :level="1">Afdelingscontacten</utrecht-heading>

  <div v-if="isLoading && !results.length" class="spinner-container">
    <simple-spinner />
  </div>

  <utrecht-alert v-else-if="error" type="error">
    {{ error }}
  </utrecht-alert>

  <section v-else>
    <label>Filter</label>

    <UtrechtSelect
      v-model="naamActor"
      :options="[{ value: '', label: 'Afdelingen', disabled: true }, ...gebruikerData]"
    >
    </UtrechtSelect>

    <scroll-container>
      <afdelings-interne-taken-table :interneTaken="results">
        <template #caption v-if="itemRange">
          {{ itemRange.start }} tot {{ itemRange.end }} van {{ totalCount }} internetaken
        </template>
      </afdelings-interne-taken-table>
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
import { onMounted, ref, watch } from "vue";
import SimpleSpinner from "@/components/SimpleSpinner.vue";
import UtrechtAlert from "@/components/UtrechtAlert.vue";
import UtrechtPagination from "@/components/UtrechtPagination.vue";
import { get } from "@/utils/fetchWrapper";
import type { InterneTaakOverviewItem } from "@/components/interne-taken-tables/AllInterneTakenTable.vue";
import AfdelingsInterneTakenTable from "@/components/interne-taken-tables/AfdelingsInterneTakenTable.vue";
import { usePagination } from "@/composables/use-pagination";
import ScrollContainer from "@/components/ScrollContainer.vue";

interface MyInterneTakenResponse {
  count: number;
  next?: string;
  previous?: string;
  results: InterneTaakOverviewItem[];
}

interface GebruikerData {
  label?: string;
  value?: string;
}
const gebruikerData = ref<GebruikerData[]>([]);

const naamActor = ref("");

watch(naamActor, () => {
  reset();
  fetchData();
});
const fetchInterneTaken = async (
  page: number,
  pageSize: number
): Promise<MyInterneTakenResponse> => {
  return await get<MyInterneTakenResponse>("/api/internetaken/afdelingen-groepen", {
    page,
    pageSize,
    naamActor: naamActor.value
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
  goToPreviousPage,
  reset
} = usePagination(fetchInterneTaken, {
  initialPage: 1,
  initialPageSize: 20,
  maxVisiblePages: 5
});

const fetchGebruikerData = async () => {
  try {
    gebruikerData.value = await get<GebruikerData[]>("/api/gebruiker-groepen-and-afdelingen");
  } catch (err) {
    gebruikerData.value = [{ label: "Fout bij het laden van groep en afdeling", value: "" }];
    console.error("Fout bij het laden van groep en afdeling:", err);
  }
};

onMounted(() => {
  fetchGebruikerData();
});
</script>

<style lang="scss" scoped>
label {
  display: block;
  font-weight: bold;
  font-size: 1rem;
}

.utrecht-select {
  max-width: 200px;
  height: 40px;
}
</style>
