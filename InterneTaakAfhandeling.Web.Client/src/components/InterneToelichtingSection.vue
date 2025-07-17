<template>
  <utrecht-spotlight-section class="ita-interne-toelichting-section">
    <slot></slot>
  </utrecht-spotlight-section>
</template>

<script setup lang="ts">
import UtrechtSpotlightSection from "@/components/UtrechtSpotlightSection.vue";
</script>

<style scoped>
.ita-interne-toelichting-section {
  border-inline-start: var(--ita-interne-toelichting-section-border-inline-width)
    var(--ita-interne-toelichting-section-border-inline-style)
    var(--ita-interne-toelichting-section-border-inline-color);

  margin-inline-start: calc(-1 * var(--current-padding-inline-start));
}
</style>
