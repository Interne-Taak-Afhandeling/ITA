<template>
  <utrecht-form-field>
    <utrecht-form-label :for="id">Medewerker</utrecht-form-label>
    <utrecht-combobox
      :id="id"
      :options="options"
      :model-value="modelValue?.identificatie ?? ''"
      placeholder="Begin met typen om te zoeken..."
      aria-label="Medewerker"
      :server-side="true"
      :loading="isLoading"
      @search="search"
      @update:model-value="onUpdate"
    />
  </utrecht-form-field>
</template>

<script setup lang="ts">
import { computed } from "vue";
import UtrechtCombobox from "@/components/UtrechtCombobox.vue";
import { useMedewerkerSearch } from "@/composables/use-medewerker-search";

export interface MedewerkerFilterValue {
  identificatie: string;
  naam: string;
}

const props = defineProps<{
  id: string;
  modelValue?: MedewerkerFilterValue;
}>();

const emit = defineEmits<{
  "update:modelValue": [value: MedewerkerFilterValue | undefined];
}>();

const { isLoading, results, search } = useMedewerkerSearch();

const options = computed(() => {
  const found = results.value.map(({ naam, identificatie }) => ({
    label: naam,
    value: identificatie
  }));
  // keep the current selection available, so its name is shown when the filter is restored
  const selected = props.modelValue;
  if (selected && !found.some(({ value }) => value === selected.identificatie)) {
    found.unshift({ label: selected.naam, value: selected.identificatie });
  }
  return found;
});

function onUpdate(identificatie: string) {
  if (!identificatie) {
    emit("update:modelValue", undefined);
    return;
  }
  const match = options.value.find(({ value }) => value === identificatie);
  if (match) {
    emit("update:modelValue", { identificatie, naam: match.label });
  }
}
</script>
