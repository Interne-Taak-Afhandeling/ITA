<template>
  <div class="utrecht-combobox">
    <input
      ref="inputRef"
      class="utrecht-combobox__input utrecht-textbox utrecht-textbox--html-input"
      type="search"
      role="combobox"
      :id="id"
      :placeholder="placeholder"
      :value="query"
      :aria-label="ariaLabel"
      :required="required"
      :aria-expanded="expanded"
      aria-haspopup="listbox"
      aria-autocomplete="list"
      :aria-controls="`${id}-listbox`"
      :aria-activedescendant="activeDescendant"
      autocomplete="off"
      @input="onInput"
      @keydown="onKeydown"
      @focus="onFocus"
      @blur="onBlur"
    />
    <small-spinner v-if="loading" class="utrecht-combobox__spinner" />
    <ul
      v-if="!loading && expanded && displayOptions.length > 0"
      :id="`${id}-listbox`"
      role="listbox"
      class="utrecht-combobox__popover utrecht-combobox__popover--block-end utrecht-listbox"
      :aria-label="ariaLabel"
      :aria-required="required ? 'true' : undefined"
    >
      <li
        v-for="(option, index) in displayOptions"
        :key="option.value"
        :id="`${id}-option-${index}`"
        role="option"
        class="utrecht-listbox__option"
        :class="{ 'utrecht-listbox__option--active': index === activeIndex }"
        :aria-selected="option.value === modelValue"
        @mousedown.prevent="selectOption(option)"
        @mouseover="activeIndex = index"
      >
        {{ option.label }}
      </li>
    </ul>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch } from "vue";
import SmallSpinner from "@/components/SmallSpinner.vue";

export interface ComboboxOption {
  label: string;
  value: string;
}

const props = withDefaults(
  defineProps<{
    id: string;
    options: ComboboxOption[];
    modelValue?: string;
    placeholder?: string;
    ariaLabel?: string;
    serverSide?: boolean;
    required?: boolean;
    loading?: boolean;
  }>(),
  { serverSide: false, required: false, loading: false, modelValue: "" }
);

const emit = defineEmits<{
  "update:modelValue": [value: string];
  selected: [option: ComboboxOption];
  search: [query: string];
}>();

const inputRef = ref<HTMLInputElement | null>(null);
const query = ref("");
const expanded = ref(false);
const activeIndex = ref(-1);
let blurTimeoutId: ReturnType<typeof setTimeout> | null = null;

const displayOptions = computed(() => {
  if (props.serverSide) return props.options;
  if (!query.value) return props.options;
  const q = query.value.toLowerCase();
  return props.options.filter((o) => o.label.toLowerCase().includes(q));
});

const activeDescendant = computed(() =>
  activeIndex.value >= 0 ? `${props.id}-option-${activeIndex.value}` : undefined
);

const validity = computed(() => {
  if (!query.value && props.required) return "";
  if (query.value && !props.modelValue) return "Kies een optie uit de lijst.";
  return "";
});

watch([inputRef, validity], ([el, v]) => {
  if (el instanceof HTMLInputElement) {
    el.setCustomValidity(v);
  }
});

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

watch(
  () => props.options,
  () => {
    activeIndex.value = Math.max(-1, Math.min(activeIndex.value, props.options.length - 1));
  }
);

function onInput(event: Event) {
  const target = event.target as HTMLInputElement;
  query.value = target.value;
  expanded.value = true;
  activeIndex.value = -1;
  if (!target.value) {
    emit("update:modelValue", "");
  }
  if (props.serverSide) {
    emit("search", target.value);
  }
}

function onFocus() {
  if (blurTimeoutId) {
    clearTimeout(blurTimeoutId);
    blurTimeoutId = null;
  }
  expanded.value = true;
}

function onBlur() {
  blurTimeoutId = setTimeout(() => {
    expanded.value = false;
    activeIndex.value = -1;
    blurTimeoutId = null;
  }, 150);
}

function onKeydown(event: KeyboardEvent) {
  switch (event.key) {
    case "ArrowDown":
      event.preventDefault();
      expanded.value = true;
      if (activeIndex.value < displayOptions.value.length - 1) {
        activeIndex.value++;
      }
      break;
    case "ArrowUp":
      event.preventDefault();
      if (activeIndex.value > 0) {
        activeIndex.value--;
      }
      break;
    case "Enter":
      event.preventDefault();
      if (activeIndex.value >= 0 && activeIndex.value < displayOptions.value.length) {
        selectOption(displayOptions.value[activeIndex.value]);
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
  inputRef.value?.focus();
}
</script>

<style scoped>
.utrecht-combobox {
  --utrecht-textbox-max-inline-size: 100%;
}

/* Override Utrecht 7.x :invalid styling — only show invalid state after user interaction */
.utrecht-combobox__input:invalid:not(:user-invalid) {
  border-color: var(--utrecht-textbox-border-color, var(--utrecht-form-control-border-color));
  background-color: var(
    --utrecht-textbox-background-color,
    var(--utrecht-form-control-background-color)
  );
}
</style>
