<template>
  <!-- workaround: the components below each have their own form. but we need to wrap the ita-radio-tabs in a form as well -->
  <form @submit.prevent>
    <!--
      Radio button group pattern which mimics tabs.
      Not using semantic tabs here as these buttons control form configuration
      rather than navigating between content panels.
    -->
    <ita-radio-tabs legend="Kies een handeling" :options="HANDLINGS" v-model="handeling" />
  </form>

  <ContactverzoekRegisteren :taak="taak" @success="succes()" v-if="isRegisterContactmoment" />

  <ContactverzoekInterneNotitie :taak="taak" @success="succes()" v-if="isInterneToelichting" />

  <ContactverzoekDoorsturen :taak="taak" @success="succes()" v-if="isForwardContactmoment" />
</template>

<script setup lang="ts">
import { computed, ref } from "vue";
import ContactverzoekInterneNotitie from "./ContactverzoekInterneNotitie.vue";
import ContactverzoekDoorsturen from "./ContactverzoekDoorsturen.vue";
import ContactverzoekRegisteren from "./ContactverzoekRegistreren.vue";
import type { Internetaken } from "@/types/internetaken";
import ItaRadioTabs from "./ItaRadioTabs.vue";
const { taak } = defineProps<{ taak: Internetaken }>();
const emit = defineEmits<{ success: [] }>();
const HANDLINGS = {
  contactmoment: "Contactmoment registreren",
  contactmomentDoorsturen: "Doorsturen",
  interneToelichting: "Alleen toelichting"
} as const;
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

form + :deep(form) {
  .utrecht-form-label {
    display: block;
  }

  .utrecht-button-group {
    margin-top: 1rem;
  }

  .small {
    font-size: var(--denhaag-process-steps-step-meta-font-size);
    color: var(--denhaag-process-steps-step-meta-color);
  }

  textarea,
  input,
  select {
    max-width: 100%;
  }
}
</style>
