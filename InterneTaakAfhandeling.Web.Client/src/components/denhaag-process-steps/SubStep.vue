<template>
  <li :class="classes"><slot></slot></li>
</template>
<script setup lang="ts">
import { computed } from "vue";
import type { StepStatus } from "./types";

const { appearance = "not-checked" } = defineProps<{ appearance?: StepStatus }>();
const classes = computed(
  () => `denhaag-process-steps__sub-step denhaag-process-steps__sub-step--${appearance}`
);
</script>
