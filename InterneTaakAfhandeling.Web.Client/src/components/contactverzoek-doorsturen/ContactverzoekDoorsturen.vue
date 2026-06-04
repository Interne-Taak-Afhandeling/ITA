<template>
  <SimpleSpinner v-if="isLoading" />
  <utrecht-alert v-else-if="error" type="error">
    {{ error }}
  </utrecht-alert>
  <form v-else @submit.prevent="forwardContactverzoek" ref="forwardForm">
    <utrecht-fieldset>
      <utrecht-fieldset>
        <utrecht-legend>Contactmoment doorzetten naar</utrecht-legend>
        <utrecht-form-field v-for="(label, key) in FORWARD_OPTIONS" :key="key" type="radio">
          <utrecht-radiobutton
            name="forwardTo"
            :id="key"
            :value="label"
            v-model="forwardTo"
            required
          />
          <utrecht-form-label :for="key" type="radio">{{ label }}</utrecht-form-label>
        </utrecht-form-field>
      </utrecht-fieldset>

      <contactverzoek-doorsturen-afdeling
        v-if="forwardTo === FORWARD_OPTIONS.afdeling"
        :afdelingen="afdelingen"
      />

      <contactverzoek-doorsturen-groep
        v-if="forwardTo === FORWARD_OPTIONS.groep"
        :groepen="groepen"
      />

      <contactverzoek-doorsturen-medewerker
        v-if="forwardTo === FORWARD_OPTIONS.medewerker"
        :groepen="groepen"
        :afdelingen="afdelingen"
      />

      <interne-toelichting-field name="interneNotitie" placeholder="Optioneel" />
    </utrecht-fieldset>

    <utrecht-button-group>
      <utrecht-button type="submit" appearance="primary-action-button"
        >Contactverzoek doorsturen</utrecht-button
      >
    </utrecht-button-group>
  </form>
</template>

<script setup lang="ts">
import { ref, onMounted, useTemplateRef } from "vue";
import { toast } from "@/components/toast/toast.ts";
import InterneToelichtingField from "../InterneToelichtingField.vue";
import type { Internetaken } from "@/types/internetaken";
import SimpleSpinner from "@/components/SimpleSpinner.vue";
import { get } from "@/utils/fetchWrapper";
import UtrechtAlert from "@/components/UtrechtAlert.vue";
import { klantcontactService } from "@/services/klantcontactService";
import ContactverzoekDoorsturenAfdeling from "./ContactverzoekDoorsturenAfdeling.vue";
import ContactverzoekDoorsturenGroep from "./ContactverzoekDoorsturenGroep.vue";
import ContactverzoekDoorsturenMedewerker from "./ContactverzoekDoorsturenMedewerker.vue";

const emit = defineEmits<{ success: [] }>();
const { taak } = defineProps<{ taak: Internetaken }>();

const FORWARD_OPTIONS = {
  afdeling: "Afdeling",
  groep: "Groep",
  medewerker: "Medewerker"
} as const;
type FORWARD_OPTIONS = typeof FORWARD_OPTIONS;

const formRef = useTemplateRef("forwardForm");

const afdelingen = ref<{ label: string; value: string }[]>([]);
const groepen = ref<{ label: string; value: string }[]>([]);

const isLoading = ref(false);
const error = ref<string | null>(null);

// Form state
const forwardTo = ref<(typeof FORWARD_OPTIONS)[keyof typeof FORWARD_OPTIONS]>(
  FORWARD_OPTIONS.afdeling
);

function resetForm() {
  forwardTo.value = FORWARD_OPTIONS.afdeling;
  formRef.value?.reset();
}

function mapFormDataToObj<K extends string>(formData: FormData, keys: K[]): { [k in K]: string } {
  const obj = {} as Record<K, string>;
  keys.forEach((key) => {
    const value = formData.get(key);
    obj[key] = typeof value === "string" ? value : "";
  });
  return obj;
}

async function forwardContactverzoek(submitEvent: Event) {
  const formElement = submitEvent.target as HTMLFormElement;
  const formData = new FormData(formElement);
  const payload = mapFormDataToObj(formData, ["medewerker", "afdeling", "groep", "interneNotitie"]);

  isLoading.value = true;
  try {
    const response = await klantcontactService.forwardKlantContact(taak.uuid, payload);
    toast.add({ text: response.notificationResult, type: "ok", timeout: 5000 });
    resetForm();
    emit("success");
  } catch (err: unknown) {
    handleSubmitError(err);
  } finally {
    isLoading.value = false;
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

function sortListByNaam<T extends { naam: string }>(list: T[]): T[] {
  return list.sort((a, b) => a.naam.localeCompare(b.naam, undefined, { sensitivity: "base" }));
}

const fetchAfdelingen = async () => {
  const response = await get<{ naam: string; identificatie: string }[]>("/api/afdelingen");
  const sortedResponse = sortListByNaam(response);
  afdelingen.value = [
    { label: "Selecteer een afdeling", value: "" },
    ...sortedResponse.map((afdeling) => ({
      label: afdeling.naam,
      value: afdeling.identificatie
    }))
  ];
};

const fetchGroepen = async () => {
  const response = await get<{ naam: string; identificatie: string }[]>("/api/groepen");
  const sortedResponse = sortListByNaam(response);
  groepen.value = [
    { label: "Selecteer een groep", value: "" },
    ...sortedResponse.map((groep) => ({
      label: groep.naam,
      value: groep.identificatie
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
