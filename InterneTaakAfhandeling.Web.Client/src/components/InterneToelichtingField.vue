<template>
  <interne-toelichting-section>
    <utrecht-form-field>
      <utrecht-form-label :for="id"> Interne toelichting </utrecht-form-label>
      <utrecht-textarea
        :id="id"
        v-model.trim="model"
        :placeholder="placeholder"
        :required="required"
      />
      <div class="small">
        Deze toelichting is alleen voor medewerkers te zien en is verborgen voor de burger/het
        bedrijf.
      </div>
    </utrecht-form-field>
  </interne-toelichting-section>
</template>

<script setup lang="ts">
import { useId } from "vue";
import InterneToelichtingSection from "./InterneToelichtingSection.vue";
const id = `interne-toelichting-text${useId()}`;
defineProps<{ required?: boolean; placeholder?: string }>();
const model = defineModel<string>({ required: true });
</script>
