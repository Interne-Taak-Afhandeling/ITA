<template>
  <utrecht-heading :level="1">Alle contactverzoeken</utrecht-heading>

  <div v-if="isLoading" class="spinner-container">
    <simple-spinner />
  </div>

  <utrecht-alert v-else-if="error" type="error">
    {{ error }}
  </utrecht-alert>

  <section v-else>
    <utrecht-heading :level="2" id="h2-alle-contactverzoeken">Alle contactverzoeken</utrecht-heading>

    <utrecht-table aria-labelledby="h2-alle-contactverzoeken">
      <utrecht-table-header>
        <utrecht-table-row>
          <utrecht-table-header-cell scope="col">Datum</utrecht-table-header-cell>
          <utrecht-table-header-cell scope="col">Klantnaam</utrecht-table-header-cell>
          <utrecht-table-header-cell scope="col">Onderwerp / vraag</utrecht-table-header-cell>
          <utrecht-table-header-cell scope="col">Afdeling</utrecht-table-header-cell>
          <utrecht-table-header-cell scope="col">Behandelaar</utrecht-table-header-cell>
          <utrecht-table-header-cell scope="col">Details</utrecht-table-header-cell>
        </utrecht-table-row>
      </utrecht-table-header>

      <utrecht-table-body>
        <utrecht-table-row v-if="results.length === 0">
          <utrecht-table-cell colspan="6">Geen contactverzoeken gevonden</utrecht-table-cell>
        </utrecht-table-row>

        <utrecht-table-row v-for="taak in results" :key="taak.uuid">
          <utrecht-table-cell class="ita-no-wrap">
            <date-time-or-nvt :date="taak.contactDatum || taak.toegewezenOp" />
          </utrecht-table-cell>
          <utrecht-table-cell>
            {{ taak.klantNaam || '-' }}
          </utrecht-table-cell>
          <utrecht-table-cell>
            {{ taak.onderwerp || taak.gevraagdeHandeling || '-' }}
          </utrecht-table-cell>
          <utrecht-table-cell>
            {{ taak.afdelingNaam || '-' }}
          </utrecht-table-cell>
          <utrecht-table-cell>
            {{ taak.behandelaarNaam || '-' }}
          </utrecht-table-cell>
          <utrecht-table-cell>
            <router-link :to="`/contactverzoek/${taak.nummer}`">
              Klik hier
            </router-link>
          </utrecht-table-cell>
        </utrecht-table-row>
      </utrecht-table-body>
    </utrecht-table>

    <div class="pagination-controls bottom" v-if="totalCount > pageSize">
      <utrecht-button 
        appearance="secondary-action-button" 
        @click="previousPage"
        :disabled="currentPage <= 1 || isLoading"
      >
        Vorige
      </utrecht-button>
      
      <span class="page-info">
        Pagina {{ currentPage }} ({{ getItemRange() }} van {{ totalCount }} items)
      </span>
      
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
import { ref, computed, onMounted } from 'vue';
import SimpleSpinner from '@/components/SimpleSpinner.vue';
import UtrechtAlert from '@/components/UtrechtAlert.vue';
import DateTimeOrNvt from '@/components/DateTimeOrNvt.vue';
import { get } from '@/utils/fetchWrapper';

// Types based on your endpoint response
interface InterneTaakOverviewItem {
  uuid: string;
  nummer: string;
  gevraagdeHandeling: string;
  status: string;
  toegewezenOp: string;
  afgehandeldOp?: string;
  onderwerp?: string;
  klantNaam?: string;
  contactDatum?: string;
  afdelingNaam?: string;
  behandelaarNaam?: string;
  heeftBehandelaar: boolean;
}

interface InterneTakenOverviewResponse {
  count: number;
  next?: string;
  previous?: string;
  results: InterneTaakOverviewItem[];
}

// Reactive state
const isLoading = ref(false);
const error = ref<string | null>(null);
const results = ref<InterneTaakOverviewItem[]>([]);
const totalCount = ref(0);
const currentPage = ref(1);
const pageSize = ref(25); // Same as typical table pagination
const hasNextPage = ref(false);
const hasPreviousPage = ref(false);

// Methods
const fetchInterneTaken = async (page: number = 1) => {
  isLoading.value = true;
  error.value = null;
  
  try {
    console.log(`Fetching alle contactverzoeken - Page: ${page}, PageSize: ${pageSize.value}`);
    
    const response = await get<InterneTakenOverviewResponse>(
      '/api/internetaken-overview',
      {
        page: page,
        pageSize: pageSize.value
      }
    );
    
    console.log('Response received:', response);
    console.log(`Total count: ${response.count}`);
    console.log(`Results on this page: ${response.results.length}`);
    
    // Update state
    results.value = response.results;
    totalCount.value = response.count;
    currentPage.value = page;
    hasNextPage.value = !!response.next;
    hasPreviousPage.value = !!response.previous;
    
    // Log each item for debugging
    response.results.forEach((item, index) => {
      console.log(`Contactverzoek ${index + 1}:`, {
        nummer: item.nummer,
        status: item.status,
        klantNaam: item.klantNaam,
        behandelaar: item.behandelaarNaam,
        afdeling: item.afdelingNaam,
        onderwerp: item.onderwerp,
        contactDatum: item.contactDatum,
        toegewezenOp: item.toegewezenOp
      });
    });
    
  } catch (err: unknown) {
    const message = err instanceof Error ? err.message : 'Er is een fout opgetreden bij het ophalen van contactverzoeken';
    error.value = message;
    console.error('Error fetching alle contactverzoeken:', err);
  } finally {
    isLoading.value = false;
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

// Auto-fetch on component mount
onMounted(() => {
  fetchInterneTaken();
});
</script>

<style lang="scss" scoped>
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

.page-info {
  font-weight: 500;
  white-space: nowrap;
  padding: 0 1rem;
}

section {
  margin-top: 1rem;
}
</style>