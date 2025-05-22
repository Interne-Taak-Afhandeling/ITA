<template>
  <ol :class="classes">
    <slot></slot>
  </ol>
</template>

<script setup lang="ts">
import { computed } from "vue";

export interface StepListProps {
  nested?: boolean;
}

const { nested } = defineProps<StepListProps>();
const classes = computed(() => ({
  "denhaag-process-steps": true,
  "denhaag-process-steps--nested": nested
}));
</script>
