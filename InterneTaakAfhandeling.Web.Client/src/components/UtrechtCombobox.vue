<template>
  <div class="utrecht-combobox" role="combobox" :aria-expanded="expanded" aria-haspopup="listbox">
    <input
      ref="inputRef"
      class="utrecht-combobox__input utrecht-textbox utrecht-textbox--html-input"
      type="text"
      :id="id"
      :placeholder="placeholder"
      :value="query"
      :aria-label="ariaLabel"
      aria-autocomplete="list"
      :aria-controls="`${id}-listbox`"
      :aria-activedescendant="activeDescendant"
      autocomplete="off"
      @input="onInput"
      @keydown="onKeydown"
      @focus="onFocus"
      @blur="onBlur"
    />
    <ul
      :id="`${id}-listbox`"
      role="listbox"
      class="utrecht-combobox__popover utrecht-combobox__popover--block-end utrecht-listbox"
      :class="{ 'utrecht-combobox__popover--hidden': !expanded || filteredOptions.length === 0 }"
      :aria-label="ariaLabel"
      tabindex="-1"
    >
      <li
        v-for="(option, index) in filteredOptions"
        :key="option.value"
        :id="`${id}-option-${index}`"
        role="option"
        class="utrecht-listbox__option"
        :class="{ 'utrecht-listbox__option--active': index === activeIndex }"
        :aria-selected="option.value === modelValue"
        @mousedown.prevent="selectOption(option)"
      >
        {{ option.label }}
      </li>
    </ul>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch, nextTick } from "vue";

export interface ComboboxOption {
  label: string;
  value: string;
}

const props = defineProps<{
  id: string;
  options: ComboboxOption[];
  modelValue: string;
  placeholder?: string;
  ariaLabel?: string;
}>();

const emit = defineEmits<{
  "update:modelValue": [value: string];
  selected: [option: ComboboxOption];
}>();

const inputRef = ref<HTMLInputElement | null>(null);
const query = ref("");
const expanded = ref(false);
const activeIndex = ref(-1);

const filteredOptions = computed(() => {
  if (!query.value) return props.options;
  const q = query.value.toLowerCase();
  return props.options.filter((o) => o.label.toLowerCase().includes(q));
});

const activeDescendant = computed(() =>
  activeIndex.value >= 0 ? `${props.id}-option-${activeIndex.value}` : undefined
);

watch(
  () => props.modelValue,
  (val) => {
    if (val) {
      const selected = props.options.find((o) => o.value === val);
      if (selected) {
        query.value = selected.label;
      }
    } else {
      query.value = "";
    }
  },
  { immediate: true }
);

function onInput(event: Event) {
  const target = event.target as HTMLInputElement;
  query.value = target.value;
  expanded.value = true;
  activeIndex.value = -1;
  if (!target.value) {
    emit("update:modelValue", "");
  }
}

function onFocus() {
  expanded.value = true;
}

function onBlur() {
  setTimeout(() => {
    expanded.value = false;
    activeIndex.value = -1;
  }, 150);
}

function onKeydown(event: KeyboardEvent) {
  switch (event.key) {
    case "ArrowDown":
      event.preventDefault();
      expanded.value = true;
      activeIndex.value = Math.min(activeIndex.value + 1, filteredOptions.value.length - 1);
      break;
    case "ArrowUp":
      event.preventDefault();
      activeIndex.value = Math.max(activeIndex.value - 1, 0);
      break;
    case "Enter":
      event.preventDefault();
      if (activeIndex.value >= 0 && activeIndex.value < filteredOptions.value.length) {
        selectOption(filteredOptions.value[activeIndex.value]);
      }
      break;
    case "Escape":
      event.preventDefault();
      expanded.value = false;
      activeIndex.value = -1;
      break;
  }
}

function selectOption(option: ComboboxOption) {
  query.value = option.label;
  emit("update:modelValue", option.value);
  emit("selected", option);
  expanded.value = false;
  activeIndex.value = -1;
  nextTick(() => inputRef.value?.focus());
}
</script>
