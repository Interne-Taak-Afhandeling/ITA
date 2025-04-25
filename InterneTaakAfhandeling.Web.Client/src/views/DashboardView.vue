<template>
  <utrecht-heading :level="1">Dashboard</utrecht-heading>

  <div class="ita-dashboard-tables">
    <section>
      <utrecht-heading :level="2" id="h2-a">Aan mij toegewezen contacten</utrecht-heading>

      <utrecht-table   aria-labelledby="h2-a">
        <utrecht-table-header>
          <utrecht-table-row>
            <utrecht-table-header-cell scope="col">Datum</utrecht-table-header-cell>
            <utrecht-table-header-cell scope="col">Naam</utrecht-table-header-cell>
            <utrecht-table-header-cell scope="col">Onderwerp</utrecht-table-header-cell>
          </utrecht-table-row>
        </utrecht-table-header>

        <utrecht-table-body>
          <utrecht-table-row v-if="assignedInternetaken.length === 0">
            <utrecht-table-cell colspan="3">Geen interne taken gevonden</utrecht-table-cell>
          </utrecht-table-row>
          
          <utrecht-table-row v-for="taak in assignedInternetaken" :key="taak.id">
            <utrecht-table-cell>{{ formatDate(taak.datum) }}</utrecht-table-cell>
            <utrecht-table-cell>{{ taak.naam }}</utrecht-table-cell>
            <utrecht-table-cell>{{ taak.onderwerp }}</utrecht-table-cell>
          </utrecht-table-row>
        </utrecht-table-body>
      </utrecht-table>
    </section>

    <section>
      <utrecht-heading :level="2" id="h2-b">Oudste contacten voor afdeling</utrecht-heading>

      <utrecht-table aria-labelledby="h2-b">
        <utrecht-table-header>
          <utrecht-table-row>
            <utrecht-table-header-cell scope="col">Datum</utrecht-table-header-cell>
            <utrecht-table-header-cell scope="col">Naam</utrecht-table-header-cell>
            <utrecht-table-header-cell scope="col">Onderwerp</utrecht-table-header-cell>
          </utrecht-table-row>
        </utrecht-table-header>

        <utrecht-table-body>
          <utrecht-table-row>
            <utrecht-table-cell>01-01-2025</utrecht-table-cell>
            <utrecht-table-cell>Saskia Swart</utrecht-table-cell>
            <utrecht-table-cell>Lorem ipsum</utrecht-table-cell>
          </utrecht-table-row>

          <utrecht-table-row>
            <utrecht-table-cell>02-01-2025</utrecht-table-cell>
            <utrecht-table-cell>Martijn de Groot</utrecht-table-cell>
            <utrecht-table-cell>Dolor sit amet</utrecht-table-cell>
          </utrecht-table-row>
        </utrecht-table-body>
      </utrecht-table>
    </section>
  </div>
</template>

<style lang="scss" scoped>
.ita-dashboard-tables {
  display: flex;
  flex-wrap: wrap;
  column-gap: var(--ita-dashboard-tables-column-gap);

  section {
    flex: 1;
  }
}

.loading {
  padding: 1rem 0;
  color: var(--ita-color-text, #333);
}

.error {
  padding: 1rem 0;
  color: var(--ita-color-error, #d52b1e);
}
</style>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { userService } from '@/services/user';
  import type { AssignedInternetaken } from '@/types/internetaken';
import { formatDate } from '@/utils/dateUtils';

const assignedInternetaken = ref<AssignedInternetaken[]>([]);
const isLoading = ref<boolean>(true);
const error = ref<string | null>(null);

const fetchInternetaken = async () => {
  isLoading.value = true;
  error.value = null;
  
  try {
    assignedInternetaken.value = await userService.getAssignedInternetaken();
  } catch (err: any) {
    error.value = err.message || 'Er is een fout opgetreden bij het ophalen van de interne taken';
    console.error('Error fetching internetaken:', err);
  } finally {
    isLoading.value = false;
  }
};

onMounted(() => {
  fetchInternetaken();
});
</script>
