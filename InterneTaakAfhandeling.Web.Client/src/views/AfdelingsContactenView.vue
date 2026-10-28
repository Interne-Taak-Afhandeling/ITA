<template>
  <utrecht-heading :level="1">{{ route.meta.title }}</utrecht-heading>
  <werklijst-bevestiging />
  <AfdelingenEnGroepenSelectieEnOverzicht :afgerond="false" cacheKey="AfdelingenEnGroepenView" />
</template>

<script setup lang="ts">
import AfdelingenEnGroepenSelectieEnOverzicht from "@/components/AfdelingenEnGroepenSelectieEnOverzicht.vue";
import WerklijstBevestiging from "@/components/WerklijstBevestiging.vue";
import { useRoute } from "vue-router";
const route = useRoute();
</script>

<style lang="scss" scoped></style>
