<template>
  <utrecht-heading :level="1">{{ route.meta.title }}</utrecht-heading>
  <AfdelingenEnGroepenSelectieEnOverzicht
    :afgerond="true"
    cacheKey="AfdelingenEnGroepenHistorieView"
  />
</template>

<script setup lang="ts">
import AfdelingenEnGroepenSelectieEnOverzicht from "@/components/AfdelingenEnGroepenSelectieEnOverzicht.vue";
import { useRoute } from "vue-router";
const route = useRoute();
</script>

<style lang="scss" scoped></style>
