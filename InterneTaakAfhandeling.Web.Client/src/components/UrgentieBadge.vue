<template>
  <utrecht-badge-status v-if="urgentie" :status="urgentie.status">
    {{ urgentie.label }}
  </utrecht-badge-status>
</template>

<script setup lang="ts">
import { BadgeStatus as UtrechtBadgeStatus } from "@utrecht/component-library-vue";
import type { UrgentieInfo } from "../types/internetaken";

defineProps<{
  urgentie?: UrgentieInfo | null;
}>();
</script>

<style lang="scss" scoped>
.utrecht-badge-status {
  --utrecht-badge-border-radius: 0.5ch;
  --utrecht-badge-font-size: 0.75rem;
  --utrecht-badge-font-weight: normal;
  --utrecht-badge-padding-block: 1ex;
  --utrecht-badge-padding-inline: 1ch;
}
</style>
