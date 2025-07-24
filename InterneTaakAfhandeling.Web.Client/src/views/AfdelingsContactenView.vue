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

    <UtrechtSelect v-model="naamActor" :busy="isgebruikerDataLoading" :options="gebruikerData">
    </UtrechtSelect>
    <utrecht-alert v-if="errorMessage" type="error">
      {{ errorMessage }}
    </utrecht-alert>
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
  disabled?: boolean;
}
const gebruikerData = ref<GebruikerData[]>([]);

const naamActor = ref<string>("");
const errorMessage = ref<string | null>(null);
const isgebruikerDataLoading = ref<boolean>(false);

watch(naamActor, () => {
  reset();
  fetchData();
});
const fetchInterneTaken = async (
  page: number,
  pageSize: number
): Promise<MyInterneTakenResponse> => {
  return await get<MyInterneTakenResponse>("/api/internetaken/afdelingen-groepen-overzicht", {
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
    isgebruikerDataLoading.value = true;
    gebruikerData.value = [{ value: '', label: 'Laden...', disabled: true }];
    gebruikerData.value = await get<GebruikerData[]>("/api/gebruiker-groepen-and-afdelingen");
    if (gebruikerData.value.length === 0) {
      gebruikerData.value = [
        { label: "Geen groepen of afdelingen gevonden", value: "", disabled: true }
      ];
    } else {
      gebruikerData.value.unshift({
        label: "Kies een Afdeling / Groep",
        value: "",
        disabled: true
      });
    }
  } catch (err) {
    errorMessage.value = "Er is een fout opgetreden. Herlaad de pagina. Als het probleem blijft bestaan, neem contact op met functioneel beheer.";
    console.error("Error loading gebruiker data:", err);
  } finally {
    isgebruikerDataLoading.value = false;
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
