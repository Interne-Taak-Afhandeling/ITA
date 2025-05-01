<template>
  <time v-if="validated" :datetime="formatIsoDateTime(validated)">{{
    formatNlDateTime(validated)
  }}</time>
  <template v-else>N.v.t</template>
</template>

<script setup lang="ts">
import { formatIsoDateTime, formatNlDateTime, parseValidDate } from "@/utils/dateUtils";
import type { DateLike } from "@vueuse/core";
import { computed } from "vue";
const props = defineProps<{ date: DateLike }>();
const validated = computed(() => parseValidDate(props.date));
</script>
