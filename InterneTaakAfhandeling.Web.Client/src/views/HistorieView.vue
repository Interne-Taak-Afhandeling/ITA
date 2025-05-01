<template>
  <utrecht-heading :level="1">Historie</utrecht-heading>

  <section>
    <utrecht-heading :level="2" id="h2-a">Mijn afgeronde contacten</utrecht-heading>
    <interne-taak-table :interne-taken="fakeInterneTaken" aria-labelledby="h2-a" />
  </section>
</template>

<script setup lang="ts">
import InterneTaakTable from "@/components/InterneTaakTable.vue";
import { fakeInterneTaken } from "@/helpers/fake-data";
</script>
