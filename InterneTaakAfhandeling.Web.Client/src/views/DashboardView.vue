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
          <utrecht-table-row v-if="assignedInternetaken.length === 0 && !isLoading">
            <utrecht-table-cell colspan="3">Geen interne taken gevonden</utrecht-table-cell>
          </utrecht-table-row>

          <utrecht-table-row v-for="taak in assignedInternetaken" :key="taak.uuid">
            <utrecht-table-cell>{{ formatDate(taak.aanleidinggevendKlantcontact?.plaatsgevondenOp) }}</utrecht-table-cell>
            <utrecht-table-cell>{{ taak.betrokken?.naam }}</utrecht-table-cell>
            <utrecht-table-cell>{{ taak.aanleidinggevendKlantcontact?.onderwerp }}</utrecht-table-cell>
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

</style>

<script setup lang="ts">
  import { onMounted } from 'vue';
  import { useUserStore } from '@/stores/user';
  import { storeToRefs } from 'pinia';
  import { formatDate } from '@/utils/dateUtils';

  const userStore = useUserStore();
  const { assignedInternetaken, isLoading } = storeToRefs(userStore);


onMounted(() => {
  userStore.fetchAssignedInternetaken();
});
</script>
