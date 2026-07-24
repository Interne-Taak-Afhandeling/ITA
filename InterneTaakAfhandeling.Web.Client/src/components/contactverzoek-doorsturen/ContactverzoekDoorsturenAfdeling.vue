<template>
  <utrecht-form-field>
    <utrecht-form-label for="afdelingSelect">Afdeling</utrecht-form-label>
    <utrecht-select
      required
      name="afdeling"
      id="afdelingSelect"
      :options="afdelingen"
      v-model="selectedAfdeling"
      @change="selectedMedewerker = ''"
    />
  </utrecht-form-field>
  <template v-if="selectedAfdeling">
    <utrecht-form-field v-if="medewerkerLoading">
      <small-spinner />
    </utrecht-form-field>
    <utrecht-form-field v-else-if="medewerkerOptions?.length">
      <utrecht-form-label for="afdeling-groep-medewerker-combobox">{{
        medewerkerLabel
      }}</utrecht-form-label>
      <utrecht-combobox
        id="afdeling-groep-medewerker-combobox"
        :options="medewerkerOptions"
        v-model="selectedMedewerker"
        :required="!selectedAfdelingHeeftMailbox"
        placeholder="Zoek op naam..."
        aria-label="Medewerker zoeken binnen selectie"
      />
      <utrecht-paragraph v-if="!selectedAfdelingHeeftMailbox">
        Verplicht: deze afdeling heeft geen groepsmailbox.
      </utrecht-paragraph>
    </utrecht-form-field>
    <utrecht-form-field v-else-if="!selectedAfdelingHeeftMailbox">
      <utrecht-alert type="warning">
        Deze afdeling heeft geen groepsmailbox en er zijn geen medewerkers beschikbaar om te
        koppelen. Kies een andere afdeling of neem contact op met functioneel beheer.
      </utrecht-alert>
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
import UtrechtAlert from "@/components/UtrechtAlert.vue";

const props = defineProps<{
  afdelingen: Array<{ label: string; value: string; heeftGroepsmailbox: boolean }>;
}>();

const selectedAfdeling = ref<string>("");
const afdelingMatch = computed(() =>
  props.afdelingen.find((a) => a.value === selectedAfdeling.value)
);
const afdelingLabel = computed(() => afdelingMatch.value?.label);
const selectedAfdelingHeeftMailbox = computed(
  () => afdelingMatch.value?.heeftGroepsmailbox ?? true
);
const medewerkerLabel = computed(() =>
  selectedAfdelingHeeftMailbox.value ? "Medewerker (optioneel)" : "Medewerker"
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
