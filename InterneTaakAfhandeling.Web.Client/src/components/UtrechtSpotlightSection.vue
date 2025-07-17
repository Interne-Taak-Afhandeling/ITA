<template>
  <div
    :class="[
      'utrecht-spotlight-section',
      'detail-section-padding-cancelation',
      'border-left',
      appeareanceClass
    ]"
  >
    <slot />
  </div>
</template>

<script setup lang="ts">
import { computed } from "vue";

const props = defineProps<{
  appeareance?: "error" | "info" | "ok" | "warning";
}>();

const appeareanceClass = computed(
  () => props.appeareance && `utrecht-spotlight-section--${props.appeareance}`
);
</script>

<style scoped>
.detail-section-padding-cancelation {
  margin-left: calc(-1 * var(--ita-detail-section-header-padding-inline-start));
  margin-right: calc(-1 * var(--ita-detail-section-header-padding-inline-end));
}

.border-left {
  border-left: var(--ita-step-note-border-width) var(--ita-step-note-border-style)
    var(--ita-step-note-border-color);
}
</style>
