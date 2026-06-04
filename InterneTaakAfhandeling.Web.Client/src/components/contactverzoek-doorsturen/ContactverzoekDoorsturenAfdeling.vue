<template>
  <utrecht-form-field>
    <utrecht-form-label for="afdelingSelect">Afdeling</utrecht-form-label>
    <utrecht-select
      required
      name="afdeling"
      id="afdelingSelect"
      :options="afdelingen"
      v-model="selectedAfdeling"
    />
  </utrecht-form-field>
  <template v-if="selectedAfdeling">
    <utrecht-form-field v-if="medewerkerLoading">
      <small-spinner />
    </utrecht-form-field>
    <utrecht-form-field v-if="medewerkerOptions?.length">
      <utrecht-form-label for="afdeling-groep-medewerker-combobox"
        >Medewerker (optioneel)</utrecht-form-label
      >
      <utrecht-combobox
        id="afdeling-groep-medewerker-combobox"
        :options="medewerkerOptions"
        v-model="selectedMedewerker"
        placeholder="Zoek op naam..."
        aria-label="Medewerker zoeken binnen selectie"
      />
    </utrecht-form-field>
    <input v-if="selectedMedewerker" type="hidden" name="medewerker" :value="selectedMedewerker" />
  </template>
</template>

<script setup lang="ts">
import { useLoader } from "@/composables/use-loader";
import { get } from "@/utils/fetchWrapper";
import { computed, ref } from "vue";
import UtrechtCombobox from "../UtrechtCombobox.vue";
import SmallSpinner from "@/components/SmallSpinner.vue";

const props = defineProps<{ afdelingen: Array<{ label: string; value: string }> }>();

const selectedAfdeling = ref<string>("");
const afdelingLabel = computed(
  () => props.afdelingen.find((a) => a.value === selectedAfdeling.value)?.label
);
const selectedMedewerker = ref<string>("");

const { loading: medewerkerLoading, data: medewerkerOptions } = useLoader(() => {
  if (afdelingLabel.value) {
    return get<{ naam: string; identificatie: string }[]>("/api/medewerkers", {
      afdelingOfGroep: afdelingLabel.value,
      type: "Afdeling"
    }).then((afdelingen) =>
      afdelingen.map(({ naam, identificatie }) => ({ label: naam, value: identificatie }))
    );
  }
});
</script>
