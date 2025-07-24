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
      v-model="naamActeur"
      :options="[
        { value: '', label: 'Afdelingen', disabled: true },
        ...(gebruikerData.afdelingen?.map((afd) => ({ value: afd, label: afd })) || []),
        { value: '', label: 'Groepen', disabled: true },
        ...(gebruikerData.groepen?.map((grp) => ({ value: grp, label: grp })) || [])
      ]"
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
import { computed, onMounted, ref, watch } from "vue";
import SimpleSpinner from "@/components/SimpleSpinner.vue";
import UtrechtAlert from "@/components/UtrechtAlert.vue";
import UtrechtPagination from "@/components/UtrechtPagination.vue";
import { get } from "@/utils/fetchWrapper";
import type { InterneTaakOverviewItem } from "@/components/interne-taken-tables/AllInterneTakenTable.vue";
import AfdelingsInterneTakenTable from "@/components/interne-taken-tables/AfdelingsInterneTakenTable.vue";
import { usePagination } from "@/composables/use-pagination";
import ScrollContainer from "@/components/ScrollContainer.vue";
import { InterneTaakStatus } from "@/types/internetaken";

interface MyInterneTakenResponse {
  count: number;
  next?: string;
  previous?: string;
  results: InterneTaakOverviewItem[];
}

interface GebruikerData {
  groepen?: string[];
  afdelingen?: string[];
}
const gebruikerData = ref<GebruikerData>({ groepen: [], afdelingen: [] });

const naamActeur = ref("");

watch(naamActeur, () => {
  reset();
  fetchData();
});
const fetchInterneTaken = async (
  page: number,
  pageSize: number
): Promise<MyInterneTakenResponse> => {
  return await get<MyInterneTakenResponse>("/api/internetaken-overview", {
    page,
    pageSize,
    naamActeur: naamActeur.value,
    status: InterneTaakStatus.TeVerwerken
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
    gebruikerData.value = await get<GebruikerData>("/api/gebruiker-groepen-and-afdelingen");
  } catch (err) {
    console.error("Error fetching gebruiker data:", err);
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
