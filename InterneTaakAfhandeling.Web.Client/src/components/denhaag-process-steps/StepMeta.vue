<template>
  <div :classes="classes">
    <slot></slot>
  </div>
</template>

<script setup lang="ts">
import { computed } from "vue";

const { date } = defineProps<{ date?: boolean }>();

const classes = computed(() => ({
  "denhaag-process-steps__step-meta": true,
  "denhaag-process-steps__step-meta--date": date
}));
</script>
