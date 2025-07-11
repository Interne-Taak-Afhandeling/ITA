<template>
  <div v-if="loading" class="spinner-container">
    <simple-spinner />
  </div>
  <utrecht-alert v-else-if="error" appeareance="error" class="margin-top">
    Er ging iets mis. Probeer het later opnieuw.
  </utrecht-alert>
  <StepList v-else>
    <Step v-for="logboekItem in logboekActiviteiten" :key="logboekItem.id" class="ita-step">
      <StepHeader>
        <StepHeading>{{ logboekItem.titel }}</StepHeading>
      </StepHeader>
      <StepBody>
        <utrecht-paragraph v-if="logboekItem.tekst" class="preserve-newline">
          {{ logboekItem.tekst }}
        </utrecht-paragraph>
        <div class="ita-step-meta-list">
          <StepMeta date><date-time-or-nvt :date="logboekItem.datum" /></StepMeta>
          <StepMeta v-if="logboekItem.uitgevoerdDoor">{{ logboekItem.uitgevoerdDoor }}</StepMeta>
          <StepMeta v-if="logboekItem.kanaal">Kanaal: {{ logboekItem.kanaal }}</StepMeta>
        </div>
      </StepBody>
    </Step>
  </StepList>
</template>

<script setup lang="ts">
import type { Internetaken } from "@/types/internetaken";
import {
  Step,
  StepHeader,
  StepHeading,
  StepBody,
  StepList,
  StepMeta
} from "@/components/denhaag-process-steps";
import SimpleSpinner from "@/components/SimpleSpinner.vue";
import UtrechtAlert from "@/components/UtrechtAlert.vue";
import DateTimeOrNvt from "./DateTimeOrNvt.vue";
import { useLoader } from "@/composables/use-loader";
import { klantcontactService } from "@/services/klantcontactService";

const props = defineProps<{ taak: Internetaken }>();
const {
  data: logboekActiviteiten,
  loading,
  error
} = useLoader((signal) => {
  if (props.taak.aanleidinggevendKlantcontact?.uuid) {
    return klantcontactService.getLogboek(props.taak.uuid, signal);
  }
});
</script>

<style lang="scss" scoped>
.ita-step {
  padding-block-end: var(--ita-step-padding-block-end);
  --utrecht-paragraph-margin-block-start: 0.5rem;
  --utrecht-paragraph-margin-block-end: 0.5rem;
}

.ita-step:not(:last-child) {
  border-bottom-width: var(--ita-step-border-bottom-width);
  border-bottom-color: var(--ita-step-border-bottom-color);
  border-bottom-style: solid;
}

.ita-step-meta-list {
  display: flex;
  flex-wrap: wrap;
  column-gap: var(--ita-step-meta-list-column-gap);
  row-gap: var(--ita-step-meta-list-row-gap);

  > :last-child:nth-child(n + 3) {
    flex: 1;
    text-align: end;
  }
}

// hack: the nl-design system component forces a 16px margin
.denhaag-process-steps__step-meta {
  margin-inline-start: 0;
}

.denhaag-process-steps {
  --denhaag-process-steps-step-distance: var(--ita-step-distance);
}
</style>
