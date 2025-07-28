<template>
  <utrecht-fieldset class="ita-radio-tabs" role="radiogroup">
    <utrecht-legend class="visually-hidden">{{ legend }}</utrecht-legend>
    <utrecht-form-field v-for="(label, key) in options" :key="key" type="radio">
      <!--
        Focus flow:
        - non-selected labels are focusable
        - checked radio is focusable for arrow keys
      -->
      <utrecht-radiobutton
        v-model="model"
        class="visually-hidden"
        :id="`radio-${key}`"
        :value="label"
        :aria-labelledby="`label-${key}`"
        :tabindex="model === label ? undefined : -1"
      />
      <span
        class="utrecht-form-label utrecht-form-label--radio"
        :id="`label-${key}`"
        :aria-label="label"
        :role="model === label ? undefined : `radio`"
        :aria-checked="model === label ? undefined : false"
        :tabindex="model === label ? undefined : 0"
        @keydown="handleKeydown($event, label)"
        @click="model = label"
        >{{ label }}</span
      >
    </utrecht-form-field>
  </utrecht-fieldset>
</template>

<script setup lang="ts">
import { watch } from "vue";

const props = defineProps<{
  legend: string;
  options: Record<string, string>;
}>();

const model = defineModel<string>("modelValue", { required: true }); // Store label value

// When modelValue is not set, activate first tab by setting modelValue with first label
watch(
  () => props.options,
  (options) => {
    if (!model.value && Object.keys(options || {}).length > 0) {
      model.value = Object.values(options)[0];
    }
  },
  { immediate: true }
);

const handleKeydown = (event: KeyboardEvent, value: string) => {
  if (event.key === " " || event.key === "Enter") {
    event.preventDefault();

    const label = event.target as HTMLLabelElement;
    (label.previousElementSibling as HTMLInputElement)?.focus();

    model.value = value;
  }
};
</script>

<style lang="scss" scoped>
@mixin focus-visible {
  outline-color: var(--utrecht-focus-outline-color);
  outline-offset: var(--utrecht-focus-outline-offset);
  outline-style: var(--utrecht-focus-outline-style);
  outline-width: var(--utrecht-focus-outline-width);
}

.ita-radio-tabs {
  border-block-end: var(--ita-radio-tabs-border-block-end);

  :deep(.utrecht-form-fieldset__fieldset) {
    display: flex;
    column-gap: var(--utrecht-focus-outline-width);
    padding-block: 0;

    .utrecht-form-field {
      grid-template-areas: "label";
      grid-template-columns: 1fr;
      margin-block: 0;
    }

    .utrecht-form-label {
      padding-block: var(--ita-radio-tabs-label-padding-block);
      padding-inline: var(--ita-radio-tabs-label-padding-inline);
      border-block-width: var(--ita-radio-tabs-label-border-block-width);
      border-block-style: var(--ita-radio-tabs-label-border-block-style);
      border-block-color: var(--ita-radio-tabs-label-border-block-color);

      &:focus-visible {
        @include focus-visible;
      }
    }

    .utrecht-radio-button:checked {
      & + .utrecht-form-label {
        font-weight: 600;
        color: var(--ita-radio-tabs-label-checked-color);
        background-color: var(--ita-radio-tabs-label-checked-background-color);
        border-block-end-color: var(--ita-radio-tabs-label-checked-border-color);
      }

      &:focus-visible + .utrecht-form-label {
        @include focus-visible;
      }
    }
  }
}
</style>
