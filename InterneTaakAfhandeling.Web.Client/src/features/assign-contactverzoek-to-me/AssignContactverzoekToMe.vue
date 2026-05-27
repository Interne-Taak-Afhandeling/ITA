<template>
  <template v-if="!isAlreadyAssigned">
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
</template>

<script setup lang="ts">
import { toast } from "@/components/toast/toast";
import { ref, computed } from "vue";
import { userService } from "@/services/userService";
import type { Actor } from "@/types/internetaken";
const props = withDefaults(
  defineProps<{
    id: string;
    userEmail: string;
    actoren: Actor[];
  }>(),
  { actoren: () => [] }
);

const emit = defineEmits(["assignmentSuccess"]);

const isAlreadyAssigned = computed(() =>
  props.actoren.some(
    (actor) =>
      actor.soortActor === "medewerker" &&
      actor.actoridentificator?.codeObjecttype === "mdw" &&
      actor.actoridentificator?.codeSoortObjectId === "email" &&
      actor.actoridentificator?.objectId?.toLowerCase() === props.userEmail.toLowerCase()
  )
);

const toewijzenAlertRef = ref<{ dialogRef?: HTMLDialogElement }>();
const show = () => toewijzenAlertRef.value?.dialogRef?.showModal();
const close = () => toewijzenAlertRef.value?.dialogRef?.close();

const toewijzen = async () => {
  await userService
    .assignInternetakenToSelf(props.id)
    .then(() => {
      toast.add("Contactverzoek toegewezen");
      emit("assignmentSuccess");
    })
    .catch(() => toast.add({ text: "Er ging iets mis. Probeer het later opnieuw.", type: "error" }))
    .finally(close);
};
</script>
