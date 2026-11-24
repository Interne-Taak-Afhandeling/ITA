<template>
  <div v-if="isGebruikerDataLoading" class="spinner-container">
    <simple-spinner />
  </div>

  <utrecht-alert v-else-if="errorGebruikerData" type="error">
    Er is een fout opgetreden. Herlaad de pagina. Als het probleem blijft bestaan, neem contact op
    met functioneel beheer
  </utrecht-alert>

  <UtrechtAlert v-else-if="!gebruikerData.length">
    Geen afdelingen of groepen gevonden
  </UtrechtAlert>

  <template v-else>
    <!-- the filters stay rendered while the list loads, so keyboard focus is not lost -->
    <div class="filters">
      <utrecht-form-field>
        <utrecht-form-label for="afdelingOfgroep"
          >Selecteer een afdeling of groep</utrecht-form-label
        >
        <UtrechtSelect id="afdelingOfgroep" v-model="actorFilterState" :options="gebruikerOptions">
        </UtrechtSelect>
      </utrecht-form-field>

      <medewerker-filter
        v-if="toonMedewerkerFilter"
        id="medewerkerFilter"
        v-model="medewerkerFilterState"
      />
    </div>

    <div v-if="isLoading" class="spinner-container">
      <simple-spinner />
    </div>

    <utrecht-alert v-else-if="error" type="error">
      Er is een fout opgetreden. Herlaad de pagina. Als het probleem blijft bestaan, neem contact op
      met functioneel beheer
    </utrecht-alert>

    <section v-else>
      <scroll-container>
        <all-interne-taken-table
          :interneTaken="results"
          :showAfgehandeldOp="afgerond"
          :showUrgentie="!afgerond"
          :emptyMessage="medewerkerFilterState ? geenResultatenVoorFilter : undefined"
        >
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
</template>

<script setup lang="ts">
import { computed, onMounted, ref, watch } from "vue";
import SimpleSpinner from "@/components/SimpleSpinner.vue";
import UtrechtAlert from "@/components/UtrechtAlert.vue";
import UtrechtPagination from "@/components/UtrechtPagination.vue";
import MedewerkerFilter, { type MedewerkerFilterValue } from "@/components/MedewerkerFilter.vue";
import { get } from "@/utils/fetchWrapper";
import AllInterneTakenTable from "@/components/interne-taken-tables/AllInterneTakenTable.vue";
import { usePagination } from "@/composables/use-pagination";
import ScrollContainer from "@/components/ScrollContainer.vue";
import type { InterneTakenPaginated } from "@/types/internetaken";
import { useState } from "@/composables/use-state";

const props = withDefaults(
  defineProps<{ afgerond: boolean; cacheKey: string; toonMedewerkerFilter?: boolean }>(),
  { toonMedewerkerFilter: false }
);

const geenResultatenVoorFilter = "Geen contactverzoeken gevonden voor dit filter";

const gebruikerData = ref<string[]>([]);
const errorGebruikerData = ref<boolean>(false);
const isGebruikerDataLoading = ref<boolean>(false);

const gebruikerOptions = computed(() => [
  { label: "-Selecteer-", value: "", disabled: true },
  ...gebruikerData.value.map((value) => ({ label: value, value }))
]);

const actorFilterState = useState<string>(props.cacheKey, "actorFilter");
const medewerkerFilterState = useState<MedewerkerFilterValue>(props.cacheKey, "medewerkerFilter");

// a changed filter starts at the first page again
watch([actorFilterState, () => medewerkerFilterState.value?.identificatie], () => {
  if (actorFilterState.value) {
    fetchData(1);
  }
});

const fetchInterneTaken = async (
  page: number,
  pageSize: number
): Promise<InterneTakenPaginated> => {
  return await get<InterneTakenPaginated>("/api/internetaken/afdelingen-groepen", {
    page,
    pageSize,
    naamActor: actorFilterState.value,
    afgerond: props.afgerond,
    medewerker: props.toonMedewerkerFilter ? medewerkerFilterState.value?.identificatie : undefined
  });
};

const pagnrState = useState<number>(props.cacheKey, "pagenr");

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
  initialPage: pagnrState.value ?? 1,
  initialPageSize: 20,
  maxVisiblePages: 5
});

watch(currentPage, () => (pagnrState.value = currentPage.value));

const fetchGebruikerData = async () => {
  isGebruikerDataLoading.value = true;
  errorGebruikerData.value = false;
  gebruikerData.value = [];
  try {
    gebruikerData.value = await get<string[]>("/api/gebruiker-groepen-and-afdelingen");
  } catch {
    errorGebruikerData.value = true;
  } finally {
    isGebruikerDataLoading.value = false;
  }
};

onMounted(() => {
  fetchGebruikerData();
  if (actorFilterState.value) {
    fetchData();
  }
});
</script>

<style lang="scss" scoped>
.utrecht-form-label {
  display: block;
}

.spinner-container {
  display: flex;
  justify-content: center;
  align-items: center;
  min-height: 200px;
}

// equal columns: a select is otherwise as wide as its longest option (e.g. a long afdeling name)
.filters {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(15rem, 1fr));
  align-items: start; // top-aligned: the medewerker field grows while searching (spinner), the others stay put
  gap: var(--utrecht-space-inline-md);
  --utrecht-select-max-inline-size: 100%;
  --utrecht-textbox-max-inline-size: 100%;

  > * {
    margin-block-end: 0;
    min-inline-size: 0;
  }
}
</style>
