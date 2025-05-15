<template>
  <div :class="['utrecht-alert', className]">
    <div class="utrecht-alert__content">
      <div class="utrecht-alert__message" role="alert">
        <slot></slot>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from "vue";

export type AlertType = "info" | "ok" | "warning" | "error";
const { type = "info" } = defineProps<{ type?: AlertType }>();
const className = computed(() => `utrecht-alert--${type}`);
</script>
