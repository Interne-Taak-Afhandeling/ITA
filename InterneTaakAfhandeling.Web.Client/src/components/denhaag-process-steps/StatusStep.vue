<template>
  <Step :appearance="step.status" :current="step.status === 'current'">
    <StepHeader>
      <StepMarker :appearance="step.status">
        <CheckedIcon v-if="step.status === 'checked'" />
        <AlertTriangleIcon v-else-if="step.status === 'warning'" />
        <CloseIcon v-else-if="step.status === 'error'" />
        <slot v-else name="marker"></slot>
      </StepMarker>
      <StepHeaderToggle
        v-if="$props.onToggleExpanded && canExpand(step)"
        :aria-controls="`${step.id}--details`"
        :expanded="expanded"
        @click="$emit('toggleExpanded')"
      >
        <StepHeading :appearance="step.status"
          ><slot name="title" :title="step.title"></slot
        ></StepHeading>
      </StepHeaderToggle>
      <StepHeading v-else :appearance="step.status"
        ><slot name="title" :title="step.title"></slot
      ></StepHeading>
    </StepHeader>
    <StepBody>
      <StepMarkerConnector
        v-if="nextStep"
        from="main"
        :to="expanded ? 'nested' : 'main'"
        :appearance="
          getLineAppearance({
            stepStatus: step.status,
            nextStepStatus: nextStatus
          })
        "
      />
      <StepMeta v-if="slots.meta"><slot name="meta"></slot></StepMeta>
      <StepMeta v-if="slots.date" date><slot name="date"></slot></StepMeta>
    </StepBody>
    <StepDetails v-if="mappedSteps?.length" :id="`${step.id}--details`" :collapsed="!expanded">
      <StepList>
        <SubStep v-for="({ substep, nextStatus, nextSubStep }, index) in mappedSteps" :key="index">
          <StepMarker :appearance="substep.status" nested>
            <CheckedIcon v-if="substep.status === 'checked'" />
          </StepMarker>
          <SubStepHeading><slot name="title" :title="substep.title"></slot></SubStepHeading>
          <StepMarkerConnector
            v-if="nextSubStep || nextStep"
            from="nested"
            :to="nextSubStep ? 'nested' : 'main'"
            :appearance="
              getLineAppearance({
                stepStatus: substep.status,
                nextStepStatus: nextStatus
              })
            "
          />
        </SubStep>
      </StepList>
    </StepDetails>
  </Step>
</template>

<script setup lang="ts">
import { computed } from "vue";
import { StepMarker, StepMarkerConnector } from "../denhaag-step-marker";
import Step from "./Step.vue";
import StepBody from "./StepBody.vue";
import StepDetails from "./StepDetails.vue";
import StepHeader from "./StepHeader.vue";
import StepHeaderToggle from "./StepHeaderToggle.vue";
import StepHeading from "./StepHeading.vue";
import StepList from "./StepList.vue";
import StepMeta from "./StepMeta.vue";
import SubStep from "./SubStep.vue";
import SubStepHeading from "./SubStepHeading.vue";
import type { StepProps, StepStatus } from "./types";

interface GetLineAppearanceProps {
  stepStatus?: StepStatus;
  nextStepStatus?: StepStatus;
  expanded?: boolean;
}

const { step, expanded, nextStep } = defineProps<{
  step: StepProps;
  expanded?: boolean;
  disabled?: boolean;
  nextStep?: StepProps;
}>();

defineEmits<{ toggleExpanded: [] }>();

const slots = defineSlots<{ title: { title: string }; marker?: void; date?: void; meta?: void }>();

const mappedSteps = computed(() =>
  step.steps?.map((substep, index, substeps) => {
    const nextSubStep = substeps[index + 1];
    const nextStatus = nextSubStep?.status || nextStep?.status;
    return { substep, nextSubStep, nextStatus };
  })
);

const getLineAppearance = ({
  stepStatus = "not-checked",
  nextStepStatus = "not-checked"
}: GetLineAppearanceProps) => {
  if (stepStatus === "checked" && nextStepStatus === "error") {
    return "checked";
  } else if (stepStatus === "checked" && nextStepStatus === "warning") {
    return "checked";
  } else if (stepStatus === "current" && nextStepStatus === "error") {
    return "checked";
  } else if (stepStatus === "current" && nextStepStatus === "warning") {
    return "checked";
  } else if (nextStepStatus === "not-checked") {
    return "not-checked";
  }
  return stepStatus;
};

const nextStatus = computed(() => (expanded && step.steps?.[0]?.status) || nextStep?.status);

const canExpand = (step: StepProps) => !!step.steps?.length;
</script>
