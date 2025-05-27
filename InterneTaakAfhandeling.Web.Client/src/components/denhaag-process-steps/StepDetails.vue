<template>
  <div :class="classes">
    <slot></slot>
  </div>
</template>
<script setup lang="ts">
import { computed } from "vue";

const { collapsed } = defineProps<{
  id: string;
  collapsed?: boolean;
}>();
const classes = computed(() => ({
  "denhaag-process-steps__step-details": true,
  "denhaag-process-steps__step-details--hidden": collapsed
}));
</script>
