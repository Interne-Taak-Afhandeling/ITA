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

      <template v-if="forwardContactmomentForm.forwardTo === FORWARD_OPTIONS.medewerker">
        <utrecht-form-field v-if="medewerkerOptions.length === 0 && !isLoading">
          <utrecht-alert type="warning"> Er zijn geen medewerkers beschikbaar. </utrecht-alert>
        </utrecht-form-field>
        <utrecht-form-field v-else>
          <utrecht-form-label :for="`medewerker-combobox`">Medewerker</utrecht-form-label>
          <UtrechtCombobox
            id="medewerker-combobox"
            :options="medewerkerOptions"
            v-model="forwardContactmomentForm.medewerker"
            placeholder="Zoek op naam..."
            aria-label="Medewerker zoeken"
            @selected="onMedewerkerSelected"
          />
        </utrecht-form-field>

        <utrecht-form-field
          v-if="forwardContactmomentForm.medewerker && secondaryOptions.length > 0"
        >
          <utrecht-form-label for="secondaryPicker">Afdeling of groep</utrecht-form-label>
          <utrecht-select
            required
            id="secondaryPicker"
            v-model="forwardContactmomentForm.afdelingOfGroep"
            :options="secondaryOptions"
          />
        </utrecht-form-field>

        <utrecht-form-field
          v-if="forwardContactmomentForm.medewerker && secondaryOptions.length <= 1"
        >
          <utrecht-alert type="warning">
            Geen afdelingen of groepen beschikbaar voor deze medewerker.
          </utrecht-alert>
        </utrecht-form-field>
      </template>

      <utrecht-form-field v-if="forwardContactmomentForm.forwardTo === FORWARD_OPTIONS.afdeling">
        <utrecht-form-label for="forwardTo">Afdeling</utrecht-form-label>
        <utrecht-select
          required
          id="forwardTo"
          v-model="forwardContactmomentForm.afdeling"
          :options="afdelingen"
        />
      </utrecht-form-field>

      <utrecht-form-field v-if="forwardContactmomentForm.forwardTo === FORWARD_OPTIONS.groep">
        <utrecht-form-label for="forwardTo">Groep</utrecht-form-label>
        <utrecht-select
          required
          id="forwardTo"
          v-model="forwardContactmomentForm.groep"
          :options="groepen"
        />
      </utrecht-form-field>

      <interne-toelichting-field
        v-model="forwardContactmomentForm.interneNotitie"
        placeholder="Optioneel"
      />
    </utrecht-fieldset>

    <utrecht-button-group>
      <utrecht-button type="submit" appearance="primary-action-button"
        >Contactverzoek doorsturen</utrecht-button
      >
    </utrecht-button-group>
  </form>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from "vue";
import { toast } from "./toast/toast";
import InterneToelichtingField from "./InterneToelichtingField.vue";
import UtrechtCombobox from "./UtrechtCombobox.vue";
import type { ComboboxOption } from "./UtrechtCombobox.vue";
import type { Internetaken, ForwardKlantcontactRequest } from "@/types/internetaken";
import SimpleSpinner from "@/components/SimpleSpinner.vue";
import { get } from "@/utils/fetchWrapper";
import UtrechtAlert from "@/components/UtrechtAlert.vue";
import { klantcontactService } from "@/services/klantcontactService";

const emit = defineEmits<{ success: [] }>();
const { taak } = defineProps<{ taak: Internetaken }>();

const FORWARD_OPTIONS = {
  medewerker: "Medewerker",
  afdeling: "Afdeling",
  groep: "Groep"
} as const;

interface MedewerkerData {
  naam: string;
  identificatie: string;
  afdelingen: { afdelingnaam: string }[];
  groepen: { groepsnaam: string }[];
}

const medewerkers = ref<MedewerkerData[]>([]);
const afdelingen = ref<{ label: string; value: string }[]>([]);
const groepen = ref<{ label: string; value: string }[]>([]);

const isLoading = ref(false);
const error = ref<string | null>(null);

const createForm = () => ({
  forwardTo: FORWARD_OPTIONS.medewerker as (typeof FORWARD_OPTIONS)[keyof typeof FORWARD_OPTIONS],
  medewerker: "",
  afdelingOfGroep: "",
  groep: "",
  afdeling: "",
  interneNotitie: ""
});

const forwardContactmomentForm = ref(createForm());

const medewerkerOptions = computed<ComboboxOption[]>(() =>
  medewerkers.value.map((m) => ({
    label: m.naam,
    value: m.identificatie
  }))
);

const secondaryOptions = computed(() => {
  const selected = medewerkers.value.find(
    (m) => m.identificatie === forwardContactmomentForm.value.medewerker
  );
  if (!selected) return [];

  const options: { label: string; value: string }[] = [
    { label: "Selecteer een afdeling of groep", value: "" }
  ];

  for (const afd of selected.afdelingen ?? []) {
    options.push({ label: afd.afdelingnaam, value: `Afdeling:${afd.afdelingnaam}` });
  }
  for (const grp of selected.groepen ?? []) {
    options.push({ label: grp.groepsnaam, value: `Groep:${grp.groepsnaam}` });
  }

  return options;
});

function resetForm() {
  forwardContactmomentForm.value = createForm();
}

function onMedewerkerSelected() {
  forwardContactmomentForm.value.afdelingOfGroep = "";
}

async function forwardContactverzoek() {
  isLoading.value = true;

  try {
    const payload = getForwardContactVerzoekPayload();
    const forwardKlantContactResponse = await klantcontactService.forwardKlantContact(
      taak.uuid,
      payload
    );
    toast.add({ text: forwardKlantContactResponse.notificationResult, type: "ok", timeout: 5000 });
    resetForm();
    emit("success");
  } catch (err: unknown) {
    handleSubmitError(err);
  } finally {
    isLoading.value = false;
  }
}

function getForwardContactVerzoekPayload(): ForwardKlantcontactRequest {
  const form = forwardContactmomentForm.value;

  if (form.forwardTo === FORWARD_OPTIONS.medewerker) {
    const secondaryValue = form.afdelingOfGroep;
    const [type, identifier] = secondaryValue.includes(":")
      ? [secondaryValue.split(":")[0], secondaryValue.split(":").slice(1).join(":")]
      : ["", ""];

    return {
      actorType: "Medewerker",
      actorIdentifier: form.medewerker,
      afdelingOfGroep: { type, identifier },
      interneNotitie: form.interneNotitie || undefined
    };
  }

  return {
    actorType: form.forwardTo,
    actorIdentifier: form.forwardTo === FORWARD_OPTIONS.afdeling ? form.afdeling : form.groep,
    interneNotitie: form.interneNotitie || undefined
  };
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

const fetchMedewerkers = async () => {
  const response = await get<MedewerkerData[]>("/api/medewerkers");
  medewerkers.value = sortListByNaam(response);
};

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
    await Promise.all([fetchMedewerkers(), fetchAfdelingen(), fetchGroepen()]);
  } catch (err: unknown) {
    handleLoadingError(err);
  } finally {
    isLoading.value = false;
  }
});
</script>
