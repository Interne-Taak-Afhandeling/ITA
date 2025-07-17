<template>
  <UtrechtSpotlightSection>
    <template v-if="mode === 'view' && modelValue">
      <utrecht-heading>
        <strong><em>Interne toelichting</em></strong>
      </utrecht-heading>
      <utrecht-paragraph>
        <em> {{ modelValue }}</em>
      </utrecht-paragraph>
    </template>

    <template v-else-if="mode === 'edit'">
      <utrecht-form-field>
        <utrecht-form-label for="interne-toelichting-text">
          Interne toelichting
        </utrecht-form-label>

        <utrecht-textarea
          id="interne-toelichting-text"
          v-model="localValue"
          placeholder="Optioneel"
          :required="required"
        />
        <utrecht-paragraph small class="gray-text">
          Deze toelichting is alleen voor medewerkers te zien en is verborgen voor de burger/het
          bedrijf.
        </utrecht-paragraph>
      </utrecht-form-field>
    </template>
  </UtrechtSpotlightSection>
</template>

<script setup lang="ts">
import { computed } from "vue";
import UtrechtSpotlightSection from "@/components/UtrechtSpotlightSection.vue";
const props = defineProps({
  mode: {
    type: String,
    required: true,
    validator: (val: string) => ["view", "edit"].includes(val)
  },
  modelValue: {
    type: String,
    default: ""
  },
  required: {
    type: Boolean,
    default: false
  }
});

const emit = defineEmits(["update:modelValue"]);

const localValue = computed({
  get: () => props.modelValue,
  set: (val) => emit("update:modelValue", val)
});
</script>

<style scoped>
.utrecht-form-label {
  display: block;
}
.utrecht-form-field {
  margin: 0;
}
.utrecht-paragraph {
  margin-top: 5px;
}

.gray-text {
  color: var(--utrecht-color-grey-40);
}
</style>
