<template>
  <div :class="classes">
    <slot></slot>
  </div>
</template>
<script setup lang="ts">
import { computed } from "vue";

const { appearance = "not-checked", nested } = defineProps<{
  appearance?: "checked" | "not-checked" | "current" | "warning" | "error" | "disabled" | "default";
  nested?: boolean; // TODO: remove, rename class to subtle when there are no children
}>();

const classes = computed(() => [
  "denhaag-step-marker",
  `denhaag-step-marker--${appearance}`,
  { "denhaag-step-marker--nested": nested }
]);
</script>
