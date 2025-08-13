<template>
  <SimpleSpinner v-if="isLoading" />
  <form v-else @submit.prevent="saveNote">
    <interne-toelichting-field v-model="interneToelichtingForm.interneNotitie" required />

    <utrecht-button-group>
      <utrecht-button type="submit" appearance="primary-action-button" :disabled="isLoading"
        >Opslaan</utrecht-button
      >
    </utrecht-button-group>
  </form>
</template>

<script setup lang="ts">
import { ref } from "vue";
import { toast } from "./toast/toast";
import { internetakenService } from "@/services/internetakenService";
import InterneToelichtingField from "./InterneToelichtingField.vue";
import SimpleSpinner from "@/components/SimpleSpinner.vue";
import type { Internetaken } from "@/types/internetaken";

const emit = defineEmits<{ success: [] }>();
const { taak } = defineProps<{ taak: Internetaken }>();

const isLoading = ref(false);

const createForm = () => ({
  interneNotitie: ""
});

const interneToelichtingForm = ref(createForm());

async function saveNote() {
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
  interneToelichtingForm.value = createForm();
}

function handleError(err: unknown) {
  const message = err instanceof Error && err.message ? err.message : "Er is een fout opgetreden.";
  toast.add({ text: message, type: "error" });
}
</script>
