<template>
  <utrecht-badge-status v-if="urgentie" :status="badgeStatus">
    {{ label }}
  </utrecht-badge-status>
</template>

<script setup lang="ts">
import { computed } from "vue";
import { BadgeStatus as UtrechtBadgeStatus } from "@utrecht/component-library-vue";

export interface UrgentieInfo {
  status: "binnen_termijn" | "bijna_verlopen" | "verlopen";
  streefdatum: string;
}

const props = defineProps<{ urgentie: UrgentieInfo | null | undefined }>();

const badgeStatus = computed(() => {
  switch (props.urgentie?.status) {
    case "binnen_termijn":
      return "success";
    case "bijna_verlopen":
      return "warning";
    case "verlopen":
      return "error";
    default:
      return undefined;
  }
});

const label = computed(() => {
  if (!props.urgentie?.streefdatum) return "";

  const streefdatum = new Date(props.urgentie.streefdatum);
  const now = new Date();
  const diffMs = streefdatum.getTime() - now.getTime();
  const diffHours = Math.round(diffMs / (1000 * 60 * 60));

  if (diffHours > 0) {
    if (diffHours > 48) {
      const days = Math.round(diffHours / 24);
      return `nog ${days}d`;
    }
    return `nog ${diffHours}u`;
  }

  const verlopenHours = Math.abs(diffHours);
  if (verlopenHours > 48) {
    const days = Math.round(verlopenHours / 24);
    return `${days}d verlopen`;
  }
  return `${verlopenHours}u verlopen`;
});
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
