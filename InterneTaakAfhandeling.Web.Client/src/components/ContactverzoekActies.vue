<template>
  <SimpleSpinner v-if="isLoading" />
  <form v-else ref="formRef">
    <!--
      Radio button group pattern which mimics tabs.
      Not using semantic tabs here as these buttons control form configuration
      rather than navigating between content panels.
    -->
    <ita-radio-tabs legend="Kies een handeling" :options="HANDLINGS" v-model="handeling" />

    <ContactverzoekRegister :taak="taak" @success="succes()" v-if="isRegisterContactmoment" />

    <ContactverzoekInterneNotitie :taak="taak" @success="succes()" v-if="isInterneToelichting" />

    <ContactverzoekForward :taak="taak" @success="succes()" v-if="isForwardContactmoment" />
  </form>
</template>

<script setup lang="ts">
import { computed, ref } from "vue";
import BevestigingsModal from "./BevestigingsModal.vue";
import ContactverzoekInterneNotitie from "./ContactverzoekInterneNotitie.vue";
import ContactverzoekForward from "./ContactverzoekForward.vue";
import ContactverzoekRegister from "./ContactverzoekRegister.vue";
import type { Internetaken } from "@/types/internetaken";

import ItaRadioTabs from "./ItaRadioTabs.vue";
import SimpleSpinner from "@/components/SimpleSpinner.vue";

const { taak } = defineProps<{ taak: Internetaken }>();
const emit = defineEmits<{ success: [] }>();

const HANDLINGS = {
  contactmoment: "Contactmoment registreren",
  contactmomentDoorsturen: "Doorsturen",
  interneToelichting: "Alleen toelichting"
} as const;

const isLoading = ref(false);
const formRef = ref<HTMLFormElement>();

const handeling = ref(HANDLINGS.contactmoment as (typeof HANDLINGS)[keyof typeof HANDLINGS]);

const isRegisterContactmoment = computed(() => handeling.value === HANDLINGS.contactmoment);
const isInterneToelichting = computed(() => handeling.value === HANDLINGS.interneToelichting);
const isForwardContactmoment = computed(
  () => handeling.value === HANDLINGS.contactmomentDoorsturen
);

function succes() {
  emit("success");
}
</script>

<style lang="scss" scoped>
.ita-radio-tabs {
  margin-block-start: 0;
  margin-inline-start: calc(-1 * var(--current-padding-inline-start));
  margin-inline-end: calc(-1 * var(--current-padding-inline-end));
}
</style>
