<template>
  <utrecht-alert-dialog ref="toewijzenAlertRef">
    <form method="dialog" @submit.prevent="toewijzen">
      <utrecht-paragraph> Wil je dit contactverzoek toewijzen aan jezelf? </utrecht-paragraph>
      <utrecht-button-group>
        <utrecht-button appearance="primary-action-button" type="submit"
          >Toewijzen aan mezelf</utrecht-button
        >
        <utrecht-button appearance="secondary-action-button" type="button" @click="close"
          >Annuleren</utrecht-button
        >
      </utrecht-button-group>
    </form>
  </utrecht-alert-dialog>
  <utrecht-button type="button" appearance="primary-action-button" @click="show"
    >Toewijzen aan mezelf</utrecht-button
  >
</template>

<script setup lang="ts">
import { toast } from "@/components/toast/toast";
import { ref } from "vue";
import { userService } from "@/services/user";
const props = defineProps<{ id: string }>();

const toewijzenAlertRef = ref<{ dialogRef?: HTMLDialogElement }>();
const show = () => toewijzenAlertRef.value?.dialogRef?.showModal();
const close = () => toewijzenAlertRef.value?.dialogRef?.close();

const toewijzen = async () => {
  await userService.assignInternetakenToSelf(props.id)
    .then(() => {
      toast.add("Contactverzoek toegewezen");
    })
    .catch(() => toast.add({ text: "Er ging iets mis. Probeer het later opnieuw.", type: "error" }))
    .finally(close);
};
</script>
