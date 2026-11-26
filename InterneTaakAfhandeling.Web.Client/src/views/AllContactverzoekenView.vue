<template>
  <utrecht-heading :level="1">{{ route.meta.title }}</utrecht-heading>
  <werklijst-bevestiging />

  <div class="filters" role="search" aria-label="Contactverzoeken filteren">
    <utrecht-form-field>
      <utrecht-form-label for="statusFilter">Status</utrecht-form-label>
      <utrecht-select id="statusFilter" v-model="status" :options="statusOptions" />
    </utrecht-form-field>

    <utrecht-form-field>
      <utrecht-form-label for="afdelingOfGroepFilter">Afdeling of groep</utrecht-form-label>
      <utrecht-select
        id="afdelingOfGroepFilter"
        v-model="afdelingOfGroep"
        :options="afdelingOfGroepOptions"
        :disabled="afdelingenEnGroepenLoading"
      />
    </utrecht-form-field>

    <medewerker-filter id="medewerkerFilter" v-model="medewerker" />
  </div>

  <utrecht-alert v-if="afdelingenEnGroepenError" type="warning">
    De afdelingen en groepen konden niet worden opgehaald.
  </utrecht-alert>

  <div v-if="isLoading" class="spinner-container">
    <simple-spinner />
  </div>

  <utrecht-alert v-else-if="error" type="error">
    {{ error }}
  </utrecht-alert>

  <section v-else>
    <scroll-container>
      <all-interne-taken-table
        :interneTaken="results"
        :showUrgentie="!afgerond"
        :showAfgehandeldOp="afgerond"
        :emptyMessage="hasActiveFilter ? geenResultatenVoorFilter : undefined"
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

<script setup lang="ts">
import { computed, onMounted, watch } from "vue";
import SimpleSpinner from "@/components/SimpleSpinner.vue";
import UtrechtAlert from "@/components/UtrechtAlert.vue";
import WerklijstBevestiging from "@/components/WerklijstBevestiging.vue";
import UtrechtPagination from "@/components/UtrechtPagination.vue";
import MedewerkerFilter, { type MedewerkerFilterValue } from "@/components/MedewerkerFilter.vue";
import { get } from "@/utils/fetchWrapper";
import AllInterneTakenTable from "@/components/interne-taken-tables/AllInterneTakenTable.vue";
import { usePagination } from "@/composables/use-pagination";
import { useLoader } from "@/composables/use-loader";
import ScrollContainer from "@/components/ScrollContainer.vue";
import type { InterneTakenPaginated } from "@/types/internetaken";
import { useRoute } from "vue-router";
import { useState } from "@/composables/use-state";

type Status = "open" | "afgehandeld";

const geenResultatenVoorFilter = "Geen contactverzoeken gevonden voor dit filter";
const cacheKey = "allcontactverzoekenView";

const route = useRoute();

const statusOptions: { label: string; value: Status }[] = [
  { label: "Open", value: "open" },
  { label: "Afgehandeld", value: "afgehandeld" }
];

// filters are kept when navigating to a contactverzoek and back, like the page number
const statusState = useState<Status>(cacheKey, "status");
const afdelingOfGroepState = useState<string>(cacheKey, "afdelingOfGroep");
const medewerker = useState<MedewerkerFilterValue>(cacheKey, "medewerker");

const status = computed({
  get: () => statusState.value ?? "open",
  set: (v: Status) => (statusState.value = v)
});
const afdelingOfGroep = computed({
  get: () => afdelingOfGroepState.value ?? "",
  set: (v: string) => (afdelingOfGroepState.value = v)
});
const afgerond = computed(() => status.value === "afgehandeld");

const hasActiveFilter = computed(
  () => afgerond.value || !!afdelingOfGroep.value || !!medewerker.value
);

const {
  loading: afdelingenEnGroepenLoading,
  error: afdelingenEnGroepenError,
  data: afdelingenEnGroepen
} = useLoader((signal) =>
  Promise.all([
    get<{ naam: string }[]>("/api/afdelingen", undefined, { signal }),
    get<{ naam: string }[]>("/api/groepen", undefined, { signal })
  ]).then(([afdelingen, groepen]) =>
    [...new Set([...afdelingen, ...groepen].map(({ naam }) => naam))].sort((a, b) =>
      a.localeCompare(b, "nl")
    )
  )
);

const afdelingOfGroepOptions = computed(() => [
  { label: "Alle afdelingen en groepen", value: "" },
  ...(afdelingenEnGroepen.value ?? []).map((naam) => ({ label: naam, value: naam }))
]);

const fetchInterneTaken = async (
  page: number,
  pageSize: number
): Promise<InterneTakenPaginated> => {
  return await get<InterneTakenPaginated>("/api/internetaken", {
    page,
    pageSize,
    afgerond: afgerond.value,
    afdelingOfGroep: afdelingOfGroep.value || undefined,
    medewerker: medewerker.value?.identificatie
  });
};

const pagnrCache = useState<number>(cacheKey, "pagenr");

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
  initialPage: pagnrCache.value ?? 1,
  initialPageSize: 20,
  maxVisiblePages: 5
});

watch(currentPage, () => (pagnrCache.value = currentPage.value));

// a changed filter starts at the first page again
watch([afgerond, afdelingOfGroep, () => medewerker.value?.identificatie], () => fetchData(1));

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

section {
  margin-top: 1rem;
  display: grid;
  justify-items: center;
  gap: 1rem;
}
</style>
