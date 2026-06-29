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
  resterendeUren: number;
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
  if (!props.urgentie) return "";

  const uren = props.urgentie.resterendeUren;

  if (uren > 0) {
    if (uren > 48) {
      const days = Math.round(uren / 24);
      return `nog ${days}d`;
    }
    return `nog ${uren}u`;
  }

  const verlopenUren = Math.abs(uren);
  if (verlopenUren > 48) {
    const days = Math.round(verlopenUren / 24);
    return `${days}d verlopen`;
  }
  return `${verlopenUren}u verlopen`;
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
