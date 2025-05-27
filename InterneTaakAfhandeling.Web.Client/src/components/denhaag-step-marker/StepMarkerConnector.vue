<template>
  <div :class="classes">
    <slot></slot>
  </div>
</template>

<script setup lang="ts">
import { computed } from "vue";

// import React, { HTMLAttributes } from 'react';

// import clsx from 'clsx';

type MarkerType = "main" | "nested";
type Appearance = "checked" | "not-checked" | "current" | "warning" | "error" | "default";

const {
  appearance = "not-checked",
  from,
  to
} = defineProps<{
  from: MarkerType;
  to: MarkerType;
  appearance?: Appearance;
}>();

const classes = computed(() => [
  "denhaag-step-marker__connector",
  `denhaag-step-marker__connector--${appearance}`,
  `denhaag-step-marker__connector--${from}-to-${to}`
]);
</script>
