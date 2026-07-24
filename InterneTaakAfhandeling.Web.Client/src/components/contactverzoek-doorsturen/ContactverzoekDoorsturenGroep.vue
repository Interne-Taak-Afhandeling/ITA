<template>
  <utrecht-form-field>
    <utrecht-form-label for="groepSelect">Groep</utrecht-form-label>
    <utrecht-select
      required
      name="groep"
      id="groepSelect"
      :options="groepen"
      v-model="selectedGroep"
      @change="selectedMedewerker = ''"
    />
  </utrecht-form-field>
  <template v-if="selectedGroep">
    <utrecht-form-field v-if="medewerkerLoading">
      <small-spinner />
    </utrecht-form-field>
    <utrecht-form-field v-else-if="medewerkerOptions?.length">
      <utrecht-form-label for="groep-medewerker-combobox">{{
        medewerkerLabel
      }}</utrecht-form-label>
      <utrecht-combobox
        id="groep-medewerker-combobox"
        :options="medewerkerOptions"
        v-model="selectedMedewerker"
        :required="!selectedGroepHeeftMailbox"
        placeholder="Zoek op naam..."
        aria-label="Medewerker zoeken binnen selectie"
      />
      <utrecht-paragraph v-if="!selectedGroepHeeftMailbox">
        Verplicht: deze groep heeft geen groepsmailbox.
      </utrecht-paragraph>
    </utrecht-form-field>
    <utrecht-form-field v-else-if="!selectedGroepHeeftMailbox">
      <utrecht-alert type="warning">
        Deze groep heeft geen groepsmailbox en er zijn geen medewerkers beschikbaar om te
        koppelen. Kies een andere groep of neem contact op met functioneel beheer.
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
  groepen: Array<{ label: string; value: string; heeftGroepsmailbox: boolean }>;
}>();

const selectedGroep = ref<string>("");
const groepMatch = computed(() => props.groepen.find((g) => g.value === selectedGroep.value));
const groepLabel = computed(() => groepMatch.value?.label);
const selectedGroepHeeftMailbox = computed(() => groepMatch.value?.heeftGroepsmailbox ?? true);
const medewerkerLabel = computed(() =>
  selectedGroepHeeftMailbox.value ? "Medewerker (optioneel)" : "Medewerker"
);
const selectedMedewerker = ref<string>("");

const { loading: medewerkerLoading, data: medewerkerOptions } = useLoader(() => {
  if (groepLabel.value) {
    return get<{ naam: string; identificatie: string }[]>("/api/medewerkers", {
      afdelingOfGroep: groepLabel.value,
      type: "Groep"
    }).then((groepen) =>
      groepen.map(({ naam, identificatie }) => ({ label: naam, value: identificatie }))
    );
  }
});
</script>
