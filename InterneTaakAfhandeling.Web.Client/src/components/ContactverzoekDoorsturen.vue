<template>
  <SimpleSpinner v-if="isLoading" />
  <utrecht-alert v-else-if="error" type="error">
    {{ error }}
  </utrecht-alert>
  <form v-else ref="formRef">
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
        <utrecht-form-label for="medewerker">Medewerker</utrecht-form-label>
        <utrecht-select
          :required="forwardContactmomentForm.forwardTo == FORWARD_OPTIONS.medewerker"
          id="medewerker"
          v-model="forwardContactmomentForm.medewerker"
          :options="medewerkers"
        />
      </utrecht-form-field>

      <interne-toelichting-section>
        <utrecht-form-field>
          <utrecht-form-label for="interne-toelichting-text">
            Interne toelichting
          </utrecht-form-label>
          <utrecht-textarea
            id="interne-toelichting-text"
            v-model="forwardContactmomentForm.interneNotitie"
            :placeholder="'Optioneel'"
          />
          <div class="small">
            Deze toelichting is alleen voor medewerkers te zien en is verborgen voor de burger/het
            bedrijf.
          </div>
        </utrecht-form-field>
      </interne-toelichting-section>
    </utrecht-fieldset>

    <utrecht-button
      type="button"
      appearance="primary-action-button"
      :disabled="isLoading"
      @click="forwardContactverzoek()"
    >
      <span v-if="isLoading">Bezig met opslaan...</span>
      <span v-else>Contactverzoek doorsturen</span>
    </utrecht-button>
  </form>

  <bevestigings-modal
    ref="bevestigingsModalRef"
    title="Contactverzoek afronden"
    message="Weet je zeker dat je het contactverzoek wilt opslaan en afronden?"
    confirm-text="Opslaan & afronden"
    cancel-text="Annuleren"
    @confirm="forwardContactverzoek()"
  />
</template>

<script setup lang="ts">
import { ref, onMounted } from "vue";
import { toast } from "./toast/toast";
import BevestigingsModal from "./BevestigingsModal.vue";
import InterneToelichtingSection from "./InterneToelichtingSection.vue";
import SimpleSpinner from "@/components/SimpleSpinner.vue";
import { get } from "@/utils/fetchWrapper";
import UtrechtAlert from "@/components/UtrechtAlert.vue";

const emit = defineEmits<{ success: [] }>();

const FORWARD_OPTIONS = {
  afdeling: "Afdeling",
  groep: "Groep",
  medewerker: "Medewerker"
} as const;

const medewerkers = [
  { label: "Selecteer een medewerker", value: "" },
  ...["Example 1", "Example 2"].map((value) => ({ label: value, value }))
];

const afdelingen = ref<{ label: string; value: string }[]>([]);

const groepen = ref<{ label: string; value: string }[]>([]);

const isLoading = ref(true);
const error = ref<string | null>(null);
const bevestigingsModalRef = ref<InstanceType<typeof BevestigingsModal>>();
const formRef = ref<HTMLFormElement>();

const forwardContactmomentForm = ref({
  forwardTo: FORWARD_OPTIONS.afdeling as (typeof FORWARD_OPTIONS)[keyof typeof FORWARD_OPTIONS],
  medewerker: "",
  groep: "",
  afdeling: "",
  interneNotitie: ""
});

function isValidForm() {
  if (!formRef.value?.checkValidity()) {
    formRef.value?.reportValidity();
    return false;
  }
  return true;
}

function resetForm() {
  forwardContactmomentForm.value = {
    forwardTo: FORWARD_OPTIONS.afdeling as (typeof FORWARD_OPTIONS)[keyof typeof FORWARD_OPTIONS],
    medewerker: "",
    groep: "",
    afdeling: "",
    interneNotitie: ""
  };
}

async function forwardContactverzoek() {
  if (!isValidForm()) return;
  isLoading.value = true;
  try {
    // TODO api call
    toast.add({ text: "Contactmoment is doorgestuurd", type: "ok" });
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

const fetchAfdelingen = async () => {
  afdelingen.value = [];
  afdelingen.value = [
    { label: "Selecteer een afdeling", value: "" },
    ...(await get<string[]>("/api/afdelingen")).map((value) => ({ label: value, value }))
  ];
};

const fetchGroepen = async () => {
  afdelingen.value = [];
  groepen.value = [
    { label: "Selecteer een groep", value: "" },
    ...(await get<string[]>("/api/groepen")).map((value) => ({ label: value, value }))
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
