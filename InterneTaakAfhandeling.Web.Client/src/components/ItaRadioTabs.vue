<template>
  <utrecht-fieldset class="ita-radio-tabs">
    <utrecht-legend class="visually-hidden">{{ legend }}</utrecht-legend>
    <utrecht-form-field v-for="(value, key) in options" :key="key" type="radio">
      <utrecht-radiobutton class="visually-hidden" :id="key" :value="value" v-model="model" />
      <utrecht-form-label
        type="radio"
        :for="key"
        :tabindex="model === value ? undefined : 0"
        @keydown="handleKeydown($event, value)"
        >{{ value }}</utrecht-form-label
      >
    </utrecht-form-field>
  </utrecht-fieldset>
</template>

<script setup lang="ts">
defineProps<{
  legend: string;
  options: Record<string, string>;
}>();

const model = defineModel<string>("modelValue", { required: true });

const handleKeydown = (event: KeyboardEvent, value: string) => {
  if (event.key === " " || event.key === "Enter") {
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
      + .utrecht-form-label {
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
