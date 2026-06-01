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
import { KnownMedewerkerIdentificators } from "@/constants/medewerkerIdentificators";
import type { Actor } from "@/types/internetaken";
const props = withDefaults(
  defineProps<{
    id: string;
    userEmail: string;
    objectregisterMedewerkerId: string;
    actoren: Actor[];
  }>(),
  { objectregisterMedewerkerId: "", actoren: () => [] }
);

const emit = defineEmits(["assignmentSuccess"]);

const isMedewerkerActor = (actor: Actor) =>
  actor.soortActor === "medewerker" &&
  actor.actoridentificator?.codeObjecttype === KnownMedewerkerIdentificators.codeObjecttype;

const matchesEmail = (actor: Actor) =>
  (actor.actoridentificator?.codeSoortObjectId ===
    KnownMedewerkerIdentificators.emailFromEntraId.codeSoortObjectId ||
    actor.actoridentificator?.codeSoortObjectId ===
      KnownMedewerkerIdentificators.emailHandmatig.codeSoortObjectId) &&
  actor.actoridentificator?.objectId?.toLowerCase() === props.userEmail.toLowerCase();

const matchesObjectRegisterId = (actor: Actor) =>
  !!props.objectregisterMedewerkerId &&
  actor.actoridentificator?.codeSoortObjectId ===
    KnownMedewerkerIdentificators.objectRegisterId.codeSoortObjectId &&
  actor.actoridentificator?.objectId === props.objectregisterMedewerkerId;

const isAlreadyAssigned = computed(() =>
  props.actoren.some(
    (actor) => isMedewerkerActor(actor) && (matchesEmail(actor) || matchesObjectRegisterId(actor))
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
