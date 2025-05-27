<!-- WORKAROUND, there's a bug in the original component -->
<template>
  <dd
    class="utrecht-data-list__item-value utrecht-data-list__item-value--html-dd"
    :class="{ 'utrecht-data-list__item-value--multiline': multiline }"
    :translate="typeof notranslate === 'boolean' ? (notranslate ? 'no' : 'yes') : undefined"
  >
    <span v-if="empty" :aria-label="emptyDescription">-</span>
    <slot v-else />
  </dd>
</template>

<script lang="ts" setup>
import { computed } from "vue";
const { value = "" } = defineProps<{
  value?: number | string;
  emptyDescription?: string;
  multiline?: boolean;
  notranslate?: boolean;
}>();
const empty = computed(() => value === "" || value === undefined);
</script>

<style src="@utrecht/data-list-css/dist/index.css"></style>
