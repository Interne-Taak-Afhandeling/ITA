<template>
  <div :class="['utrecht-alert', `utrecht-alert--${type}`]">
    <div class="utrecht-alert__content">
      <div class="utrecht-alert__message" role="alert">
        <slot></slot>
      </div>
    </div>
  </div>
</template>

<script lang="ts" setup>
export type AlertType = "info" | "ok" | "warning" | "error";
const { type = "info" } = defineProps<{ type?: AlertType }>();
</script>
