<template>
  <interne-toelichting-section>
    <utrecht-form-field>
      <utrecht-form-label :for="id"> Interne toelichting </utrecht-form-label>
      <utrecht-textarea
        :id="id"
        v-model.trim="model"
        :placeholder="placeholder"
        :required="required"
        :name="name"
        :aria-describedby="hintId"
      />
      <div :id="hintId" class="small">Bedoeld voor collega's.</div>
    </utrecht-form-field>
  </interne-toelichting-section>
</template>

<script setup lang="ts">
import { useId } from "vue";
import InterneToelichtingSection from "./InterneToelichtingSection.vue";
const id = `interne-toelichting-text${useId()}`;
const hintId = `${id}-hint`;
defineProps<{ required?: boolean; placeholder?: string; name?: string }>();
const model = defineModel<string>({ default: "" });
</script>
