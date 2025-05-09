<template>
  <div :class="['utrecht-spotlight-section', appeareanceClass]"><slot /></div>
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

<style lang="scss" scoped>
@import "@utrecht/spotlight-section-css/src/index.scss";
</style>
