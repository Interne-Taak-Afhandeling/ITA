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
        <StepHeading>{{ getActionDescription(logboekItem) }}</StepHeading>
      </StepHeader>
      <StepBody>
        <utrecht-data-list>
          <utrecht-data-list-item v-if="logboekItem.tekst">
            <utrecht-data-list-key>Informatie voor burger/bedrijf</utrecht-data-list-key>
            <utrecht-data-list-value :value="logboekItem.tekst" multiline>{{
              logboekItem.tekst
            }}</utrecht-data-list-value>
          </utrecht-data-list-item>
          <utrecht-data-list-item v-if="logboekItem.type === 'toegewezen' && logboekItem.medewerker">
            <utrecht-data-list-value :value="logboekItem.medewerker">{{
              "Contactverzoek opgepakt door " + logboekItem.medewerker
            }}</utrecht-data-list-value>
          </utrecht-data-list-item>
        </utrecht-data-list>
        <div class="ita-step-meta-list">
          <StepMeta date><date-time-or-nvt :date="logboekItem.datum" /></StepMeta>
          <StepMeta v-if="logboekItem.medewerker">{{ logboekItem.medewerker }}</StepMeta>
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
import { klantcontactService, type LogboekActiviteit } from "@/services/klantcontactService";

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

const getActionDescription = (logboekItem: LogboekActiviteit) => {
  switch (logboekItem.type) {
    case "klantcontact":
      return logboekItem.contactGelukt ? "Contact gelukt" : "Contact niet gelukt";
    case "verwerkt":
      return "Afgerond";
    case "zaak-gekoppeld":
      return "Zaak gekoppeld";
    case "zaakkoppeling-gewijzigd":
      return "Zaak gewijzigd";
    case "toegewezen":
      return "Opgepakt";
    default:
      return logboekItem.type || "Onbekende actie";
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

  > :last-child:nth-child(n+3) {
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