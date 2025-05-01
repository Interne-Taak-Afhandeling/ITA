<template>
  <utrecht-heading :level="1">Contactverzoek {{ cvId }}</utrecht-heading>
</template>

<script lang="ts" setup>
import { computed } from "vue";
import { useRoute } from "vue-router";

const route = useRoute();
const cvId = computed(() => route.params.number);
</script>
