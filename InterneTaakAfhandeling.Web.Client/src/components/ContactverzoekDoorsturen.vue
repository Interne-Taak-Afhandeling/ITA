<template>
  <SimpleSpinner v-if="isLoading" />
  <utrecht-alert v-else-if="error" type="error">
    {{ error }}
  </utrecht-alert>
  <form v-else @submit.prevent="forwardContactverzoek">
    <utrecht-fieldset>
      <utrecht-fieldset>
        <utrecht-legend>Contactmoment doorzetten naar</utrecht-legend>
        <utrecht-form-field v-for="(label, key) in FORWARD_OPTIONS" :key="key" type="radio">
          <utrecht-radiobutton
            name="forwardTo"
            :id="key"
            :value="label"
            v-model="forwardContactmomentForm.forwardTo"
            required
          />
          <utrecht-form-label :for="key" type="radio">{{ label }}</utrecht-form-label>
        </utrecht-form-field>
      </utrecht-fieldset>

      <utrecht-form-field v-if="forwardContactmomentForm.forwardTo == FORWARD_OPTIONS.afdeling">
        <utrecht-form-label for="forwardTo">Afdeling</utrecht-form-label>
        <utrecht-select
          required
          id="forwardTo"
          v-model="forwardContactmomentForm.afdeling"
          :options="afdelingen"
        />
      </utrecht-form-field>

      <utrecht-form-field v-if="forwardContactmomentForm.forwardTo == FORWARD_OPTIONS.groep">
        <utrecht-form-label for="forwardTo">Groep</utrecht-form-label>
        <utrecht-select
          required
          id="forwardTo"
          v-model="forwardContactmomentForm.groep"
          :options="groepen"
        />
      </utrecht-form-field>

      <utrecht-form-field>
        <utrecht-form-label for="medewerker">E-mailadres medewerker</utrecht-form-label>
        <utrecht-textbox
          id="medewerker"
          type="email"
          v-model="forwardContactmomentForm.medewerker"
          placeholder="Voer een e-mailadres in"
        />
      </utrecht-form-field>

      <interne-toelichting-field
        v-model="forwardContactmomentForm.interneNotitie"
        placeholder="Optioneel"
      />
    </utrecht-fieldset>

    <utrecht-button type="submit" appearance="primary-action-button"
      >Contactverzoek doorsturen</utrecht-button
    >
  </form>
</template>

<script setup lang="ts">
import { ref, onMounted } from "vue";
import { toast } from "./toast/toast";
import InterneToelichtingField from "./InterneToelichtingField.vue";
import type { Internetaken } from "@/types/internetaken";
import SimpleSpinner from "@/components/SimpleSpinner.vue";
import { get } from "@/utils/fetchWrapper";
import UtrechtAlert from "@/components/UtrechtAlert.vue";
import { klantcontactService } from "@/services/klantcontactService";

const emit = defineEmits<{ success: [] }>();
const { taak } = defineProps<{ taak: Internetaken }>();

const FORWARD_OPTIONS = {
  afdeling: "Afdeling",
  groep: "Groep"
} as const;

const afdelingen = ref<{ label: string; value: string }[]>([]);

const groepen = ref<{ label: string; value: string }[]>([]);

const isLoading = ref(false);
const error = ref<string | null>(null);

const createForm = () => ({
  forwardTo: FORWARD_OPTIONS.afdeling as (typeof FORWARD_OPTIONS)[keyof typeof FORWARD_OPTIONS],
  medewerker: "",
  groep: "",
  afdeling: "",
  interneNotitie: ""
});

const forwardContactmomentForm = ref(createForm());

function resetForm() {
  forwardContactmomentForm.value = createForm();
}

async function forwardContactverzoek() {
  const email = forwardContactmomentForm.value.medewerker;

  if (email && email.trim() !== "") {
    if (!isValidEmail(email)) {
      toast.add({ text: "The e-mail is not a valid e-mail", type: "error" });
      return;
    }
  }

  isLoading.value = true;

  try {
    await klantcontactService.forwardKlantContact(taak.uuid, getForwardContactVerzoekPayload());
    toast.add({ text: "Contactmoment is doorgestuurd", type: "ok" });
    resetForm();
    emit("success");
  } catch (err: unknown) {
    handleSubmitError(err);
  } finally {
    isLoading.value = false;
  }
}

function isValidEmail(email: string) {
  const emailPattern = /^[\w.%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;
  return emailPattern.test(email);
}

function getForwardContactVerzoekPayload() {
  const payload = {
    actorType: forwardContactmomentForm.value.forwardTo,
    actorIdentifier: getActorIdentifier(),
    interneNotitie: forwardContactmomentForm.value.interneNotitie,
    medewerkerEmail: forwardContactmomentForm.value.medewerker
  };

  return payload;
}

function getActorIdentifier() {
  if (forwardContactmomentForm.value.forwardTo == FORWARD_OPTIONS.afdeling) {
    return forwardContactmomentForm.value.afdeling;
  } else if (forwardContactmomentForm.value.forwardTo == FORWARD_OPTIONS.groep) {
    return forwardContactmomentForm.value.groep;
  } else {
    return "";
  }
}

function handleSubmitError(err: unknown) {
  const message = err instanceof Error && err.message ? err.message : "Er is een fout opgetreden.";
  toast.add({ text: message, type: "error" });
}

function handleLoadingError(err: unknown) {
  const message =
    err instanceof Error && err.message
      ? err.message
      : "Er is een fout opgetreden. Herlaad de pagina. Als het probleem blijft bestaan, neem contact op met functioneel beheer";
  error.value = message;
}

const fetchAfdelingen = async () => {
  afdelingen.value = [];
  const response = await get<{ naam: string; uuid: string }[]>("/api/afdelingen");

  afdelingen.value = [
    { label: "Selecteer een afdeling", value: "" },
    ...response.map((afdeling) => ({
      label: afdeling.naam,
      value: afdeling.uuid
    }))
  ];
};
const fetchGroepen = async () => {
  groepen.value = [];
  const response = await get<{ naam: string; uuid: string }[]>("/api/groepen");

  groepen.value = [
    { label: "Selecteer een groep", value: "" },
    ...response.map((groep) => ({
      label: groep.naam,
      value: groep.uuid
    }))
  ];
};

onMounted(async () => {
  try {
    isLoading.value = true;
    error.value = null;
    await Promise.all([fetchAfdelingen(), fetchGroepen()]);
  } catch (err: unknown) {
    handleLoadingError(err);
  } finally {
    isLoading.value = false;
  }
});
</script>
