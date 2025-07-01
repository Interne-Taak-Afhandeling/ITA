<template>
  <div v-if="loading" class="spinner-container">
    <simple-spinner />
  </div>
  <utrecht-alert v-else-if="error" appeareance="error" class="margin-top">
    Er ging iets mis. Probeer het later opnieuw.
  </utrecht-alert>
  <div v-else class="logboek-container">
    <div class="logboek-steps">
      <StepList>
        <Step
          v-for="logboekItem in logboekActiviteiten"
          :key="logboekItem.id"
          :appearance="
            getStepStatus(logboekItem.contactGelukt ? 'Contact gelukt' : 'Contact niet gelukt')
          "
          class="ita-step"
        >
          <StepHeader>
            <StepHeading
              :appearance="
                getStepStatus(logboekItem.contactGelukt ? 'Contact gelukt' : 'Contact niet gelukt')
              "
              class="actieomschrijving-titel"
              >{{
                logboekItem.contactGelukt ? "Contact gelukt" : "Contact niet gelukt"
              }}</StepHeading
            >
          </StepHeader>
          <StepBody>
            <utrecht-data-list>
              <utrecht-data-list-item v-if="logboekItem.tekst">
                <utrecht-data-list-key>Informatie voor burger/bedrijf</utrecht-data-list-key>
                <utrecht-data-list-value :value="logboekItem.tekst" multiline>{{
                  logboekItem.tekst
                }}</utrecht-data-list-value>
              </utrecht-data-list-item>
            </utrecht-data-list>
            <div class="ita-step-meta-list">
              <StepMeta date><date-time-or-nvt :date="logboekItem.datum" /></StepMeta>
              <StepMeta>{{ logboekItem.medewerker }}</StepMeta>
              <StepMeta>Kanaal: {{ logboekItem.kanaal }}</StepMeta>
            </div>
          </StepBody>
        </Step>
      </StepList>
    </div>
  </div>
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

const getStepStatus = (actieOmschrijving: string | undefined) => {
  switch (actieOmschrijving) {
    case "Afgerond":
      return "checked";
    case "Zaak gekoppeld":
    case "zaak gewijzigd":
      return "checked";
    case "Opgepakt":
      return "current";
    case "Contact gelukt":
      return "checked";
    case "Geen gehoor":
      return "warning";
    default:
      return "not-checked";
  }
};
</script>

<style lang="scss" scoped>
.ita-step {
  padding-block-end: var(--ita-step-padding-block-end);
  --utrecht-data-list-margin-block-start: 0.5rem;
  --utrecht-data-list-margin-block-end: 0.5rem;
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

  > :nth-last-child(2) {
    flex: 1;
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
