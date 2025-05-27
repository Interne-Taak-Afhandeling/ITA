<!-- eslint-disable vue/multi-word-component-names -->
<template>
  <li :class="classes" :aria-current="ariaCurrent">
    <slot></slot>
  </li>
</template>
<script setup lang="ts">
import { computed } from "vue";
import type { StepStatus } from "./types";

const { appearance = "not-checked", current } = defineProps<{
  appearance?: StepStatus;
  current?: boolean;
}>();

const classes = computed(() => [
  "denhaag-process-steps__step",
  `denhaag-process-steps__step--${appearance}`
]);

const ariaCurrent = computed(() => (current ? "step" : undefined));
</script>
