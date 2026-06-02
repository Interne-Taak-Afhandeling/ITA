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

      <!-- MODE: Afdeling -->
      <utrecht-form-field v-if="forwardContactmomentForm.forwardTo === FORWARD_OPTIONS.afdeling">
        <utrecht-form-label for="afdelingSelect">Afdeling</utrecht-form-label>
        <utrecht-select
          required
          id="afdelingSelect"
          v-model="forwardContactmomentForm.afdeling"
          :options="afdelingen"
        />
      </utrecht-form-field>

      <!-- MODE: Groep -->
      <utrecht-form-field v-if="forwardContactmomentForm.forwardTo === FORWARD_OPTIONS.groep">
        <utrecht-form-label for="groepSelect">Groep</utrecht-form-label>
        <utrecht-select
          required
          id="groepSelect"
          v-model="forwardContactmomentForm.groep"
          :options="groepen"
        />
      </utrecht-form-field>

      <!-- Optional medewerker picker for Afdeling/Groep modes -->
      <template v-if="showAfdelingGroepMedewerkerPicker">
        <utrecht-form-field v-if="isAfdelingGroepMedewerkerLoading">
          <small-spinner />
        </utrecht-form-field>
        <utrecht-form-field v-else-if="afdelingGroepMedewerkerOptions.length > 0">
          <utrecht-form-label for="afdeling-groep-medewerker-combobox"
            >Medewerker (optioneel)</utrecht-form-label
          >
          <UtrechtCombobox
            id="afdeling-groep-medewerker-combobox"
            :options="afdelingGroepMedewerkerOptions"
            v-model="forwardContactmomentForm.afdelingGroepMedewerker"
            placeholder="Zoek op naam..."
            aria-label="Medewerker zoeken binnen selectie"
          />
        </utrecht-form-field>
      </template>

      <!-- MODE: Medewerker — server-side search combobox -->
      <template v-if="forwardContactmomentForm.forwardTo === FORWARD_OPTIONS.medewerker">
        <utrecht-form-field>
          <utrecht-form-label for="medewerker-combobox">Medewerker</utrecht-form-label>
          <UtrechtCombobox
            id="medewerker-combobox"
            :options="medewerkerSearchOptions"
            v-model="forwardContactmomentForm.medewerker"
            placeholder="Begin met typen om te zoeken..."
            aria-label="Medewerker zoeken"
            :server-side="true"
            :required="true"
            :loading="isMedewerkerSearchLoading"
            @search="onMedewerkerSearch"
            @selected="onMedewerkerDirectSelected"
          />
        </utrecht-form-field>

        <!-- Secondary afdeling/groep picker after medewerker selection -->
        <utrecht-form-field
          v-if="forwardContactmomentForm.medewerker && secondaryOptions.length > 1"
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
            Geen afdelingen of groepen beschikbaar voor deze medewerker. Kies een andere
            medewerker.
          </utrecht-alert>
        </utrecht-form-field>
      </template>

      <interne-toelichting-field
        v-model="forwardContactmomentForm.interneNotitie"
        placeholder="Optioneel"
      />
    </utrecht-fieldset>

    <utrecht-button-group>
      <utrecht-button
        type="submit"
        appearance="primary-action-button"
        :disabled="medewerkerMissingAfdelingGroep"
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
import SmallSpinner from "@/components/SmallSpinner.vue";
import { get } from "@/utils/fetchWrapper";
import UtrechtAlert from "@/components/UtrechtAlert.vue";
import { klantcontactService } from "@/services/klantcontactService";

const emit = defineEmits<{ success: [] }>();
const { taak } = defineProps<{ taak: Internetaken }>();

const FORWARD_OPTIONS = {
  afdeling: "Afdeling",
  groep: "Groep",
  medewerker: "Medewerker"
} as const;

interface MedewerkerData {
  naam: string;
  identificatie: string;
  afdelingen: { afdelingnaam: string }[];
  groepen: { groepsnaam: string }[];
}

const medewerkerSearchResults = ref<MedewerkerData[]>([]);
const afdelingGroepMedewerkers = ref<MedewerkerData[]>([]);
const afdelingen = ref<{ label: string; value: string }[]>([]);
const groepen = ref<{ label: string; value: string }[]>([]);

const isLoading = ref(false);
const isAfdelingGroepMedewerkerLoading = ref(false);
const isMedewerkerSearchLoading = ref(false);
const error = ref<string | null>(null);

let searchDebounceTimer: ReturnType<typeof setTimeout> | null = null;

const createForm = () => ({
  forwardTo: FORWARD_OPTIONS.afdeling as (typeof FORWARD_OPTIONS)[keyof typeof FORWARD_OPTIONS],
  medewerker: "",
  afdelingOfGroep: "",
  afdelingGroepMedewerker: "",
  groep: "",
  afdeling: "",
  interneNotitie: ""
});

const forwardContactmomentForm = ref(createForm());

// Modes 1/2: show medewerker picker after afdeling/groep selected
const selectedAfdelingOfGroepNaam = computed(() => {
  const form = forwardContactmomentForm.value;
  if (form.forwardTo === FORWARD_OPTIONS.afdeling && form.afdeling) {
    return afdelingen.value.find((a) => a.value === form.afdeling)?.label ?? "";
  }
  if (form.forwardTo === FORWARD_OPTIONS.groep && form.groep) {
    return groepen.value.find((g) => g.value === form.groep)?.label ?? "";
  }
  return "";
});

// Mode 3: combobox options from server-side search
const medewerkerSearchOptions = computed<ComboboxOption[]>(() =>
  medewerkerSearchResults.value.map((m) => ({
    label: m.naam,
    value: m.identificatie
  }))
);

// Mode 3: secondary picker (afdeling/groep of selected medewerker)
const secondaryOptions = computed(() => {
  const selected = medewerkerSearchResults.value.find(
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

const showAfdelingGroepMedewerkerPicker = computed(
  () =>
    forwardContactmomentForm.value.forwardTo !== FORWARD_OPTIONS.medewerker &&
    selectedAfdelingOfGroepNaam.value !== ""
);

const afdelingGroepMedewerkerOptions = computed<ComboboxOption[]>(() =>
  afdelingGroepMedewerkers.value.map((m) => ({
    label: m.naam,
    value: m.identificatie
  }))
);

const medewerkerMissingAfdelingGroep = computed(
  () =>
    forwardContactmomentForm.value.forwardTo === FORWARD_OPTIONS.medewerker &&
    forwardContactmomentForm.value.medewerker !== "" &&
    secondaryOptions.value.length <= 1
);

function resetForm() {
  forwardContactmomentForm.value = createForm();
  medewerkerSearchResults.value = [];
  afdelingGroepMedewerkers.value = [];
}

function onModeChange() {
  forwardContactmomentForm.value.medewerker = "";
  forwardContactmomentForm.value.afdelingOfGroep = "";
  forwardContactmomentForm.value.afdelingGroepMedewerker = "";
  forwardContactmomentForm.value.afdeling = "";
  forwardContactmomentForm.value.groep = "";
  medewerkerSearchResults.value = [];
  afdelingGroepMedewerkers.value = [];
}

// Modes 1/2: fetch medewerkers when afdeling/groep changes
watch(selectedAfdelingOfGroepNaam, async (naam) => {
  forwardContactmomentForm.value.afdelingGroepMedewerker = "";
  if (!naam) {
    afdelingGroepMedewerkers.value = [];
    return;
  }
  isAfdelingGroepMedewerkerLoading.value = true;
  try {
    const response = await get<MedewerkerData[]>("/api/medewerkers", {
      afdelingOfGroep: naam,
      type: forwardContactmomentForm.value.forwardTo
    });
    afdelingGroepMedewerkers.value = response;
  } catch {
    afdelingGroepMedewerkers.value = [];
  } finally {
    isAfdelingGroepMedewerkerLoading.value = false;
  }
});

// Mode 3: debounced server-side search
function onMedewerkerSearch(query: string) {
  if (searchDebounceTimer) clearTimeout(searchDebounceTimer);
  if (!query || query.length < 2) {
    medewerkerSearchResults.value = [];
    isMedewerkerSearchLoading.value = false;
    return;
  }
  isMedewerkerSearchLoading.value = true;
  searchDebounceTimer = setTimeout(async () => {
    try {
      const response = await get<MedewerkerData[]>("/api/medewerkers", { search: query });
      medewerkerSearchResults.value = response;
    } catch {
      medewerkerSearchResults.value = [];
    } finally {
      isMedewerkerSearchLoading.value = false;
    }
  }, 300);
}

function onMedewerkerDirectSelected() {
  forwardContactmomentForm.value.afdelingOfGroep = "";
}

async function forwardContactverzoek() {
  isLoading.value = true;
  try {
    const payload = getForwardContactVerzoekPayload();
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

function getForwardContactVerzoekPayload(): ForwardKlantcontactRequest {
  const form = forwardContactmomentForm.value;

  // Modes 1/2: Afdeling/Groep, optionally with medewerker
  const afdelingOfGroepIdentifier =
    form.forwardTo === FORWARD_OPTIONS.afdeling ? form.afdeling : form.groep;

  if (form.afdelingGroepMedewerker) {
    return {
      actorType: "Medewerker",
      actorIdentifier: form.afdelingGroepMedewerker,
      afdelingOfGroep: {
        type: form.forwardTo,
        identifier: afdelingOfGroepIdentifier
      },
      interneNotitie: form.interneNotitie || undefined
    };
  }

  // Mode 3: Medewerker direct
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
