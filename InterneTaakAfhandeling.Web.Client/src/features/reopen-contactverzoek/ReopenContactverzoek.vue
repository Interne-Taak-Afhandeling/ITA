<template>
  <utrecht-alert-dialog ref="heropenAlertRef">
    <form method="dialog" @submit.prevent="heropen">
      <utrecht-heading :level="2">Contactverzoek heropenen</utrecht-heading>
      <utrecht-paragraph>
        Geef een reden op voor het heropenen van dit contactverzoek.
      </utrecht-paragraph>
      <utrecht-form-field>
        <utrecht-form-label for="reopen-reden">Reden</utrecht-form-label>
        <utrecht-textarea
          id="reopen-reden"
          v-model="reden"
          :required="true"
          placeholder="Beschrijf waarom dit contactverzoek heropend moet worden"
          rows="3"
        />
      </utrecht-form-field>
      <utrecht-button-group>
        <utrecht-button appearance="primary-action-button" type="submit" :disabled="isLoading">
          {{ isLoading ? "Bezig..." : "Heropenen" }}
        </utrecht-button>
        <utrecht-button
          appearance="secondary-action-button"
          type="button"
          :disabled="isLoading"
          @click="close"
        >
          Annuleren
        </utrecht-button>
      </utrecht-button-group>
    </form>
  </utrecht-alert-dialog>
  <utrecht-button type="button" appearance="primary-action-button" @click="show">
    Heropenen
  </utrecht-button>
</template>

<script setup lang="ts">
import { toast } from "@/components/toast/toast";
import { ref } from "vue";
import { internetakenService } from "@/services/internetakenService";

const props = defineProps<{
  id: string;
}>();

const emit = defineEmits(["success"]);

const heropenAlertRef = ref<{ dialogRef?: HTMLDialogElement }>();
const reden = ref("");
const isLoading = ref(false);

const show = () => heropenAlertRef.value?.dialogRef?.showModal();
const close = () => {
  reden.value = "";
  heropenAlertRef.value?.dialogRef?.close();
};

const heropen = async () => {
  isLoading.value = true;
  try {
    const response = await internetakenService.reopenInternetaak(props.id, reden.value.trim());

    if (response.waarschuwing) {
      toast.add({ text: response.waarschuwing, type: "ok", timeout: 5000 });
    } else {
      toast.add("Contactverzoek heropend");
    }

    emit("success");
  } catch (error) {
    const message =
      error instanceof Error ? error.message : "Er ging iets mis. Probeer het later opnieuw.";
    toast.add({ text: message, type: "error" });
  } finally {
    isLoading.value = false;
    close();
  }
};
</script>

<style scoped>
/* Override Utrecht :invalid styling — only show invalid state after user interaction */
.utrecht-textarea:invalid:not(:user-invalid) {
  border-color: var(--utrecht-textarea-border-color, var(--utrecht-form-control-border-color));
  background-color: var(
    --utrecht-textarea-background-color,
    var(--utrecht-form-control-background-color)
  );
}
</style>
