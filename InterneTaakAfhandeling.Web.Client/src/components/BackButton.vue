<template>
  <router-link :to="backButtonInfo.route" class="back-link">
    {{ backButtonInfo.text }}
  </router-link>
</template>

<script setup lang="ts">
import { useBackNavigation } from "@/composables/use-back-navigation";

const { backButtonInfo } = useBackNavigation();
</script>

<style scoped>
.back-link {
  display: inline-block;
  margin-bottom: 1rem;
  color: var(--utrecht-link-color);
  text-decoration: none;
  font-weight: 500;
}

.back-link:hover {
  color: var(--utrecht-link-focus-color);
  text-decoration: underline;
}

.back-link::before {
  content: "← ";
  margin-right: 0.25rem;
}
</style>
