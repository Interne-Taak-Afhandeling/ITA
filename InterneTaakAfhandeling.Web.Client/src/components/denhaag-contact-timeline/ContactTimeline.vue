<template>
  <ContactTimelineList class="denhaag-process-steps">
    <ContactTimelineListItem v-for="item in mappedItems" v-bind="item" :key="item.id">
      <template v-for="(_, name) in slots" v-slot:[name]="slotProps">
        <slot v-if="slotProps" :name="name" v-bind="slotProps" />
      </template>
    </ContactTimelineListItem>
  </ContactTimelineList>
</template>
<script setup lang="ts">
import { computed, ref, watchEffect } from "vue";
import ContactTimelineList from "./ContactTimelineList.vue";
import ContactTimelineListItem, {
  type ContactTimelineItemProps,
  type ContactTimelineItemSlots,
  type Labels
} from "./ContactTimelineListItem.vue";

export interface ContactTimelineProps {
  items: ContactTimelineItemProps[];
  collapsible?: boolean;
  labels: Labels;
  locale?: string;
  expandedItems?: string[];
}

const { items, expandedItems, collapsible, labels, locale } = defineProps<ContactTimelineProps>();
const slots = defineSlots<ContactTimelineItemSlots>();

const actualExpandedItems = ref<string[]>([]);

const toggleState = (key: string) => {
  if (actualExpandedItems.value.includes(key)) {
    actualExpandedItems.value = actualExpandedItems.value.filter((item) => item !== key);
  } else {
    actualExpandedItems.value = [...actualExpandedItems.value, key];
  }
};

const mappedItems = computed(() =>
  items.map((item, index) => ({
    ...item,
    labels,
    locale,
    nextItem: index < items.length - 1,
    expanded: collapsible && actualExpandedItems.value.includes(item.id),
    onToggleExpanded: collapsible ? () => toggleState(item.id) : undefined
  }))
);

watchEffect(() => expandedItems && (actualExpandedItems.value = expandedItems));
</script>
