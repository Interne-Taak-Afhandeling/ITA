<template>
  <SimpleSpinner v-if="isLoading" />
  <form v-else ref="formRef">
    <!--
      Radio button group pattern which mimics tabs.
      Not using semantic tabs here as these buttons control form configuration
      rather than navigating between content panels.
    -->
    <ita-radio-tabs legend="Kies een handeling" :options="HANDLINGS" v-model="handeling" />

    <ContactverzoekRegisteren :taak="taak" @success="succes()" v-if="isRegisterContactmoment" />

    <ContactverzoekInterneNotitie :taak="taak" @success="succes()" v-if="isInterneToelichting" />

    <ContactverzoekDoorsturen :taak="taak" @success="succes()" v-if="isForwardContactmoment" />
  </form>
</template>

<script setup lang="ts">
import { computed, ref } from "vue";
import ContactverzoekInterneNotitie from "./ContactverzoekInterneNotitie.vue";
import ContactverzoekDoorsturen from "./ContactverzoekDoorsturen.vue";
import ContactverzoekRegisteren from "./ContactverzoekRegistreren.vue";
import type { Internetaken } from "@/types/internetaken";

import ItaRadioTabs from "./ItaRadioTabs.vue";
import SimpleSpinner from "@/components/SimpleSpinner.vue";
import { get } from "@/utils/fetchWrapper";
import UtrechtAlert from "@/components/UtrechtAlert.vue";

const { taak } = defineProps<{ taak: Internetaken }>();
const emit = defineEmits<{ success: [] }>();

const HANDLINGS = {
  contactmoment: "Contactmoment registreren",
  contactmomentDoorsturen: "Doorsturen",
  interneToelichting: "Alleen toelichting"
} as const;

const isLoading = ref(false);
  ...["Example 1", "Example 2"].map((value) => ({ label: value, value }))
];

const isLoading = ref(false);
const bevestigingsModalRef = ref<InstanceType<typeof BevestigingsModal>>();
const formRef = ref<HTMLFormElement>();

const handeling = ref(HANDLINGS.contactmoment as (typeof HANDLINGS)[keyof typeof HANDLINGS]);

const isRegisterContactmoment = computed(() => handeling.value === HANDLINGS.contactmoment);
function succes() {
  emit("success");
    indicatieContactGelukt: registerContactmomentForm.value.resultaat === RESULTS.contactGelukt,

function handleError(err: unknown) {
  const message = err instanceof Error && err.message ? err.message : "Er is een fout opgetreden.";
  toast.add({ text: message, type: "error" });
}
</script>

<style lang="scss" scoped>
.ita-radio-tabs {
  margin-block-start: 0;
  margin-inline-start: calc(-1 * var(--current-padding-inline-start));
  margin-inline-end: calc(-1 * var(--current-padding-inline-end));
}
</style>
