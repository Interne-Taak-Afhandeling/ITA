<template>
  <div v-if="isLoading || isGebruikerDataLoading" class="spinner-container">
    <simple-spinner />
  </div>

  <utrecht-alert v-else-if="error || errorGebruikerData" type="error">
    Er is een fout opgetreden. Herlaad de pagina. Als het probleem blijft bestaan, neem contact op
    met functioneel beheer
  </utrecht-alert>

  <UtrechtAlert v-else-if="!isGebruikerDataLoading && !errorGebruikerData && !gebruikerData.length">
    Geen afdelingen of groepen gevonden
  </UtrechtAlert>

  <section v-else>
    <utrecht-form-field>
      <utrecht-form-label for="afdelingOfgroep">Selecteer een afdeling of groep</utrecht-form-label>
      <UtrechtSelect
        id="afdelingOfgroep"
        v-model="actorFilter.filterValue.value"
        :options="gebruikerOptions"
      >
      </UtrechtSelect>
    </utrecht-form-field>

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
import { computed, onMounted, ref, watch } from "vue";
import SimpleSpinner from "@/components/SimpleSpinner.vue";
import UtrechtAlert from "@/components/UtrechtAlert.vue";
import UtrechtPagination from "@/components/UtrechtPagination.vue";
import { get } from "@/utils/fetchWrapper";
import AllInterneTakenTable from "@/components/interne-taken-tables/AllInterneTakenTable.vue";
import { usePagination } from "@/composables/use-pagination";
import ScrollContainer from "@/components/ScrollContainer.vue";
import type { InterneTakenPaginated } from "@/types/internetaken";
import { useActorFilter } from "@/composables/use-actorfilter";

const props = defineProps<{ afgerond: boolean; cacheKey: string }>();

const gebruikerData = ref<string[]>([]);
const errorGebruikerData = ref<boolean>(false);
const isGebruikerDataLoading = ref<boolean>(false);

const gebruikerOptions = computed(() => [
  { label: "-Selecteer-", value: "", disabled: true },
  ...gebruikerData.value.map((value) => ({ label: value, value }))
]);

const actorFilter = useActorFilter(props.cacheKey);

watch(actorFilter.filterValue, () => {
  fetchData();
});

const fetchInterneTaken = async (
  page: number,
  pageSize: number
): Promise<InterneTakenPaginated> => {
  return await get<InterneTakenPaginated>("/api/internetaken/afdelingen-groepen", {
    page,
    pageSize,
    naamActor: actorFilter.filterValue.value,
    afgerond: props.afgerond
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
} = usePagination(
  fetchInterneTaken,
  {
    initialPage: 1,
    initialPageSize: 20,
    maxVisiblePages: 5
  },
  props.cacheKey
);

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
  if (actorFilter.filterValue.value) {
    fetchData();
  }
});
</script>

<style lang="scss" scoped>
.utrecht-form-label {
  display: block;
}
</style>
