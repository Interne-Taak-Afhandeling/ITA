<template>
  <time v-if="validated" :datetime="formatIsoDate(validated)">{{ formatNlDate(validated) }}</time>
  <template v-else>N.v.t</template>
</template>

<script setup lang="ts">
import { formatIsoDate, formatNlDate, parseValidDate } from "@/utils/dateUtils";
import type { DateLike } from "@vueuse/core";
import { computed } from "vue";
const props = defineProps<{ date: DateLike }>();
const validated = computed(() => parseValidDate(props.date));
</script>
