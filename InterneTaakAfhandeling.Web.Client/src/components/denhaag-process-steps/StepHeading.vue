<template>
  <p :class="classes">
    <slot></slot>
  </p>
</template>
<script setup lang="ts">
import { computed } from "vue";
import type { StepStatus } from "./types";

const { appearance = "not-checked" } = defineProps<{
  appearance?: StepStatus;
}>();

const classes = computed(() => [
  "denhaag-process-steps__step-heading",
  `denhaag-process-steps__step-heading--${appearance}`
]);
</script>
