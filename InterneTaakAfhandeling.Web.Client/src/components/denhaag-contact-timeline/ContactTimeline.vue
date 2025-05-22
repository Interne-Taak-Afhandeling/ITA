<template>
  <ContactTimelineList class="denhaag-process-steps">
    <ContactTimelineListItem
      v-for="{ item, nextItem } in mappedItems"
      v-bind="item"
      :key="item.id"
      :expanded="collapsible ? actualExpandedItems.includes(item.id) : false"
      :nextItem="nextItem"
      :labels="labels"
      :locale="locale"
      @toggleExpanded="toggleState(item.id)"
    >
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

const { items, expandedItems } = defineProps<ContactTimelineProps>();
const slots = defineSlots<ContactTimelineItemSlots>();

const mappedItems = computed(() =>
  items.map((item, index) => ({ item, nextItem: index < items.length - 1 }))
);

export interface ContactTimelineProps {
  items: ContactTimelineItemProps[];
  collapsible?: boolean;
  labels: Labels;
  locale?: string;
  expandedItems?: string[];
}

const actualExpandedItems = ref<string[]>([]);

watchEffect(() => expandedItems && (actualExpandedItems.value = expandedItems));

const toggleState = (key: string) => {
  if (actualExpandedItems.value.includes(key)) {
    actualExpandedItems.value = actualExpandedItems.value.filter((item) => item !== key);
  } else {
    actualExpandedItems.value = [...actualExpandedItems.value, key];
  }
};
</script>
