<template>
  <utrecht-form-field>
    <utrecht-form-label for="medewerker-combobox">Medewerker</utrecht-form-label>
    <utrecht-combobox
      id="medewerker-combobox"
      :options="medewerkerSearchOptions"
      v-model="selectedMedewerker"
      placeholder="Begin met typen om te zoeken..."
      aria-label="Medewerker zoeken"
      :server-side="true"
      :required="true"
      :loading="isMedewerkerSearchLoading"
      @search="onMedewerkerSearch"
      @selected="afdelingOfGroep = ''"
    />
  </utrecht-form-field>

  <input v-if="selectedMedewerker" type="hidden" name="medewerker" :value="selectedMedewerker" />

  <!-- if there is exactly one option, it is selected automatically in the hidden input -->
  <utrecht-form-field v-if="secondaryOptions.length > 1">
    <utrecht-form-label for="secondaryPicker">Afdeling of groep</utrecht-form-label>
    <utrecht-select
      required
      id="secondaryPicker"
      v-model="afdelingOfGroep"
      :options="secondaryOptions"
    />
  </utrecht-form-field>

  <utrecht-form-field v-if="selectedMedewerker && !secondaryOptions.length">
    <utrecht-alert type="warning">
      Geen afdelingen of groepen beschikbaar voor deze medewerker. Kies een andere medewerker.
    </utrecht-alert>
  </utrecht-form-field>

  <input
    v-if="hiddenAfdelingOfGroepInput"
    type="hidden"
    :name="hiddenAfdelingOfGroepInput.name"
    :value="hiddenAfdelingOfGroepInput.value"
  />
</template>
<script setup lang="ts">
import { get } from "@/utils/fetchWrapper";
import { computed, ref } from "vue";
import UtrechtCombobox from "../UtrechtCombobox.vue";
import UtrechtAlert from "@/components/UtrechtAlert.vue";

interface MedewerkerData {
  naam: string;
  identificatie: string;
  afdelingen: { afdelingnaam: string }[];
  groepen: { groepsnaam: string }[];
}

const props = defineProps<{
  afdelingen: Array<{ label: string; value: string }>;
  groepen: Array<{ label: string; value: string }>;
}>();

const isMedewerkerSearchLoading = ref(false);
const selectedMedewerker = ref<string>("");
const afdelingOfGroep = ref<string>("");

const medewerkerSearchResults = ref<MedewerkerData[]>([]);
const medewerkerSearchOptions = computed(() =>
  medewerkerSearchResults.value.map(({ naam, identificatie }) => ({
    label: naam,
    value: identificatie
  }))
);
const medewerkerMatch = computed(() =>
  medewerkerSearchResults.value.find(
    ({ identificatie }) => identificatie === selectedMedewerker.value
  )
);

const secondaryOptions = computed(() =>
  !medewerkerMatch.value
    ? []
    : [
        ...props.afdelingen
          .filter(({ label }) =>
            medewerkerMatch.value?.afdelingen.some(({ afdelingnaam }) => afdelingnaam === label)
          )
          .map(({ label, value }) => ({ label, value: `afdeling:${value}` })),
        ...props.groepen
          .filter(({ label }) =>
            medewerkerMatch.value?.groepen.some(({ groepsnaam }) => groepsnaam === label)
          )
          .map(({ label, value }) => ({ label, value: `groep:${value}` }))
      ]
);

const hiddenAfdelingOfGroepInput = computed(() => {
  const value =
    secondaryOptions.value.length === 1 ? secondaryOptions.value[0].value : afdelingOfGroep.value;
  if (!value) return null;
  const index = value.indexOf(":");
  const type = value.substring(0, index);
  const id = value.substring(index + 1);
  return { name: type, value: id };
});

let searchDebounceTimer: ReturnType<typeof setTimeout> | null = null;
let searchAbortController: AbortController | null = null;

function onMedewerkerSearch(query: string) {
  if (searchDebounceTimer) clearTimeout(searchDebounceTimer);
  if (searchAbortController) searchAbortController.abort();
  if (!query || query.length < 2) {
    medewerkerSearchResults.value = [];
    isMedewerkerSearchLoading.value = false;
    return;
  }
  isMedewerkerSearchLoading.value = true;
  searchDebounceTimer = setTimeout(async () => {
    searchAbortController = new AbortController();
    try {
      const response = await get<MedewerkerData[]>(
        "/api/medewerkers",
        { search: query },
        { signal: searchAbortController.signal }
      );
      medewerkerSearchResults.value = response;
    } catch (e) {
      if (e instanceof DOMException && e.name === "AbortError") return;
      medewerkerSearchResults.value = [];
    } finally {
      isMedewerkerSearchLoading.value = false;
    }
  }, 300);
}
</script>
