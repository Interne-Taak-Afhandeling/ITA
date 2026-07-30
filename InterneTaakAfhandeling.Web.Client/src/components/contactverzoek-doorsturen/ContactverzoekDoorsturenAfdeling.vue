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
      <utrecht-alert type="info" v-if="!selectedAfdelingHeeftMailbox">
        {{ medewerkerVerplichtToelichting }}
      </utrecht-alert>
    </utrecht-form-field>
    <utrecht-form-field v-else-if="!selectedAfdelingHeeftMailbox">
      <utrecht-alert type="warning">
        {{ geenMedewerkerBeschikbaarMelding }}
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
import {
  getMedewerkerLabel,
  getMedewerkerVerplichtToelichting,
  getGeenMedewerkerBeschikbaarMelding
} from "@/constants/medewerkerVerplichtTeksten";

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
// Refactor: this computed-on-computed-on-computed chain exists only to toggle an
// "(optioneel)" label, which no other optional field in this form shows. That inconsistency,
// not the chain itself, is the real problem — worth revisiting the UX (e.g. how optionality is
// communicated form-wide) rather than just simplifying this derivation.
const medewerkerLabel = computed(() => getMedewerkerLabel(selectedAfdelingHeeftMailbox.value));
const medewerkerVerplichtToelichting = getMedewerkerVerplichtToelichting("afdeling");
const geenMedewerkerBeschikbaarMelding = getGeenMedewerkerBeschikbaarMelding("afdeling");
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
