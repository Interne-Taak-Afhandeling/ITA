<template>
  <div :class="['utrecht-alert', `utrecht-alert--${type}`]">
    <div class="utrecht-alert__content">
      <div class="utrecht-alert__message" :role="role">
        <slot></slot>
      </div>
    </div>
  </div>
</template>

<script lang="ts" setup>
export type AlertType = "info" | "ok" | "warning" | "error";
const { type = "info", role = "alert" } = defineProps<{
  type?: AlertType;
  role?: "alert" | "status" | "none";
}>();
</script>
