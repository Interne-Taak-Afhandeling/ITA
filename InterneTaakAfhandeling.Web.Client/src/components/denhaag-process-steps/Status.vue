<template>
  <StepList>
    <StatusStep
      v-for="({ step, collapsibleStep }, index) in mappedSteps"
      :key="step.id"
      :step="step"
      :expanded="collapsibleStep ? expandedSteps?.includes(step.id) : true"
      :disabled="disabledSteps?.includes(step.id)"
      :nextStep="steps[index + 1]"
      @toggle-expanded="
        collapsibleStep &&
        (() => {
          toggleState(step.id);
        })
      "
    />
  </StepList>
</template>

<script setup lang="ts">
import { computed } from "vue";
import StepList, { type StepListProps } from "./StepList.vue";
import type { StepProps } from "./types";
import StatusStep from "./StatusStep.vue";

interface StatusProps extends StepListProps {
  steps: StepProps[];
  collapsible?: boolean;
}

const expandedSteps = defineModel<string[]>("expanded-steps");
const disabledSteps = defineModel<string[]>("disabled-steps");

const { steps, collapsible } = defineProps<StatusProps>();
const mappedSteps = computed(() =>
  steps.map((step) => ({ step, collapsibleStep: collapsible && step.collapsible !== false }))
);

const toggleState = (key: string) => {
  if (!expandedSteps.value) {
    expandedSteps.value = [];
  }
  if (expandedSteps.value.includes(key)) {
    expandedSteps.value = expandedSteps.value.filter((item) => item !== key);
  } else {
    expandedSteps.value = [...expandedSteps.value, key];
  }
};
</script>
