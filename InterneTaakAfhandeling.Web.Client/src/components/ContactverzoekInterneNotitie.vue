<template>
  <SimpleSpinner v-if="isLoading" />
  <form v-else ref="formRef">
    <interne-toelichting-section>
      <utrecht-form-field>
        <utrecht-form-label for="interne-toelichting-text">
          Interne toelichting
        </utrecht-form-label>
        <utrecht-textarea
          id="interne-toelichting-text"
          v-model="interneToelichtingForm.interneNotitie"
          :placeholder="undefined"
          :required="true"
        />
        <div class="small">
          Deze toelichting is alleen voor medewerkers te zien en is verborgen voor de burger/het
          bedrijf.
        </div>
      </utrecht-form-field>
    </interne-toelichting-section>

    <utrecht-button
      type="button"
      appearance="primary-action-button"
      :disabled="isLoading"
      @click="saveNote()"
    >
      <span v-if="isLoading">Bezig met opslaan...</span>
      <span v-else>Opslaan</span>
    </utrecht-button>
  </form>
</template>

<script setup lang="ts">
import { ref } from "vue";
import { toast } from "./toast/toast";
import { internetakenService } from "@/services/internetakenService";
import InterneToelichtingSection from "./InterneToelichtingSection.vue";
import SimpleSpinner from "@/components/SimpleSpinner.vue";
import type { Internetaken } from "@/types/internetaken";

const emit = defineEmits<{ success: [] }>();
const { taak } = defineProps<{ taak: Internetaken }>();

const isLoading = ref(false);
const formRef = ref<HTMLFormElement>();

const interneToelichtingForm = ref({
  interneNotitie: ""
});

function isValidForm() {
  if (!formRef.value?.checkValidity()) {
    formRef.value?.reportValidity();
    return false;
  }
  return true;
}

async function saveNote() {
  if (!isValidForm()) return;
  isLoading.value = true;
  try {
    await internetakenService.addNoteToInternetaak(
      taak.uuid,
      interneToelichtingForm.value.interneNotitie.trim()
    );
    toast.add({ text: "Notitie succesvol toegevoegd", type: "ok" });
    resetForm();
    emit("success");
  } catch (err: unknown) {
    handleError(err);
  } finally {
    isLoading.value = false;
  }
}

function resetForm() {
  interneToelichtingForm.value = {
    interneNotitie: ""
  };
}

function handleError(err: unknown) {
  const message = err instanceof Error && err.message ? err.message : "Er is een fout opgetreden.";
  toast.add({ text: message, type: "error" });
}
</script>

<style lang="scss" scoped>
.utrecht-form-label {
  display: block;
}

.utrecht-button-group {
  margin-top: 1rem;
}

.small {
  font-size: var(--denhaag-process-steps-step-meta-font-size);
  color: var(--denhaag-process-steps-step-meta-color);
}

textarea,
input,
select {
  max-width: 100%;
}
</style>
