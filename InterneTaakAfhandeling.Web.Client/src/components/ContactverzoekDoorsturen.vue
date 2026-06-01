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
            @change="onModeChange"
          />
          <utrecht-form-label :for="key" type="radio">{{ label }}</utrecht-form-label>
        </utrecht-form-field>
      </utrecht-fieldset>

      <utrecht-form-field v-if="forwardContactmomentForm.forwardTo === FORWARD_OPTIONS.afdeling">
        <utrecht-form-label for="afdelingSelect">Afdeling</utrecht-form-label>
        <utrecht-select
          required
          id="afdelingSelect"
          v-model="forwardContactmomentForm.afdeling"
          :options="afdelingen"
          @change="onAfdelingGroepChange"
        />
      </utrecht-form-field>

      <utrecht-form-field v-if="forwardContactmomentForm.forwardTo === FORWARD_OPTIONS.groep">
        <utrecht-form-label for="groepSelect">Groep</utrecht-form-label>
        <utrecht-select
          required
          id="groepSelect"
          v-model="forwardContactmomentForm.groep"
          :options="groepen"
          @change="onAfdelingGroepChange"
        />
      </utrecht-form-field>

      <template v-if="showMedewerkerPicker">
        <utrecht-form-field v-if="isMedewerkerLoading">
          <SimpleSpinner />
        </utrecht-form-field>
        <utrecht-form-field v-else-if="medewerkerOptions.length === 0">
          <utrecht-alert type="info"> Geen medewerkers gevonden voor deze selectie. </utrecht-alert>
        </utrecht-form-field>
        <utrecht-form-field v-else>
          <utrecht-form-label :for="`medewerker-combobox`"
            >Medewerker (optioneel)</utrecht-form-label
          >
          <UtrechtCombobox
            id="medewerker-combobox"
            :options="medewerkerOptions"
            v-model="forwardContactmomentForm.medewerker"
            placeholder="Zoek op naam..."
            aria-label="Medewerker zoeken"
          />
        </utrecht-form-field>
      </template>

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
import { ref, computed, onMounted, watch } from "vue";
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
const isMedewerkerLoading = ref(false);
const error = ref<string | null>(null);

const createForm = () => ({
  forwardTo: FORWARD_OPTIONS.afdeling as (typeof FORWARD_OPTIONS)[keyof typeof FORWARD_OPTIONS],
  medewerker: "",
  groep: "",
  afdeling: "",
  interneNotitie: ""
});

const forwardContactmomentForm = ref(createForm());

const selectedAfdelingOfGroepNaam = computed(() => {
  const form = forwardContactmomentForm.value;
  if (form.forwardTo === FORWARD_OPTIONS.afdeling && form.afdeling) {
    const afd = afdelingen.value.find((a) => a.value === form.afdeling);
    return afd?.label ?? "";
  }
  if (form.forwardTo === FORWARD_OPTIONS.groep && form.groep) {
    const grp = groepen.value.find((g) => g.value === form.groep);
    return grp?.label ?? "";
  }
  return "";
});

const showMedewerkerPicker = computed(() => selectedAfdelingOfGroepNaam.value !== "");

const medewerkerOptions = computed<ComboboxOption[]>(() =>
  medewerkers.value.map((m) => ({
    label: m.naam,
    value: m.identificatie
  }))
);

function resetForm() {
  forwardContactmomentForm.value = createForm();
  medewerkers.value = [];
}

function onModeChange() {
  forwardContactmomentForm.value.medewerker = "";
  forwardContactmomentForm.value.afdeling = "";
  forwardContactmomentForm.value.groep = "";
  medewerkers.value = [];
}

function onAfdelingGroepChange() {
  forwardContactmomentForm.value.medewerker = "";
}

watch(selectedAfdelingOfGroepNaam, async (naam) => {
  if (!naam) {
    medewerkers.value = [];
    return;
  }
  await fetchMedewerkersForAfdelingOfGroep(naam);
});

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
  const afdelingOfGroepIdentifier =
    form.forwardTo === FORWARD_OPTIONS.afdeling ? form.afdeling : form.groep;

  if (form.medewerker) {
    return {
      actorType: "Medewerker",
      actorIdentifier: form.medewerker,
      afdelingOfGroep: {
        type: form.forwardTo,
        identifier: afdelingOfGroepIdentifier
      },
      interneNotitie: form.interneNotitie || undefined
    };
  }

  return {
    actorType: form.forwardTo,
    actorIdentifier: afdelingOfGroepIdentifier,
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

const fetchMedewerkersForAfdelingOfGroep = async (naam: string) => {
  isMedewerkerLoading.value = true;
  try {
    const response = await get<MedewerkerData[]>("/api/medewerkers", {
      afdelingOfGroep: naam
    });
    medewerkers.value = sortListByNaam(response);
  } catch {
    medewerkers.value = [];
  } finally {
    isMedewerkerLoading.value = false;
  }
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
    await Promise.all([fetchAfdelingen(), fetchGroepen()]);
  } catch (err: unknown) {
    handleLoadingError(err);
  } finally {
    isLoading.value = false;
  }
});
</script>
