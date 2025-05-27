<template>
  <Step class="denhaag-contact-timeline__step" appearance="default">
    <StepHeader class="denhaag-contact-timeline__step-header">
      <ContactTimelineHeaderDate>
        <slot name="date" v-if="$slots.date" v-bind="$props"></slot>
        <ContactTimelineMetaTimeItem v-else-if="isoDate" :datetime="isoDate">
          {{
            formatDate({
              dateTime: isoDate,
              locale: locale,
              format: shortDateOptions,
              labels: {
                ...labels
              }
            })[0]
          }}
        </ContactTimelineMetaTimeItem>
      </ContactTimelineHeaderDate>
      <StepMarker appearance="default" nested />
      <ContactTimelineHeaderChannel>
        <ContactTimelineMetaItem
          ><slot name="channel" v-bind="$props">{{ channel }}</slot></ContactTimelineMetaItem
        >
      </ContactTimelineHeaderChannel>
      <ContactTimelineHeaderContent>
        <StepHeaderToggle
          v-if="description && hasOnToggleExpandedListener"
          class="denhaag-contact-timeline__step-header-toggle"
          :aria-controls="`${id}--details`"
          :expanded="expanded"
          @click="$emit('toggleExpanded')"
        >
          <StepHeading
            ><slot name="title" v-bind="$props">{{ title }}</slot></StepHeading
          >
        </StepHeaderToggle>
        <StepHeading v-else
          ><slot name="title" v-bind="$props">{{ title }}</slot></StepHeading
        >
        <ContactTimelineMeta>
          <slot name="date" v-if="$slots.date" v-bind="$props"></slot>
          <ContactTimelineMetaTimeItem v-else-if="isoDate" :datetime="isoDate">
            {{
              formatDate({
                dateTime: isoDate,
                locale: locale,
                format: shortDateOptions,
                labels: {
                  ...labels
                }
              })[0]
            }}
          </ContactTimelineMetaTimeItem>
          <ContactTimelineMetaSeparator />
          <ContactTimelineMetaItem
            ><slot name="channel" v-bind="$props">{{ channel }}</slot></ContactTimelineMetaItem
          >
        </ContactTimelineMeta>
      </ContactTimelineHeaderContent>
      <StepMarkerConnector
        v-if="nextItem"
        class="denhaag-contact-timeline__step-marker-connector"
        appearance="default"
        from="main"
        to="main"
      />
    </StepHeader>
    <StepDetails
      class="denhaag-contact-timeline__step-details"
      :id="`${id}--details`"
      :collapsed="!expanded"
    >
      <ContactTimelineItemSender>{{ sender }}</ContactTimelineItemSender>
      <UtrechtParagraph
        ><slot name="description" v-bind="$props">{{ description }}</slot></UtrechtParagraph
      >
      <ContactTimelineItemFile v-if="$slots.file"
        ><slot name="file" v-bind="$props"></slot
      ></ContactTimelineItemFile>
    </StepDetails>
  </Step>
</template>

<script setup lang="ts">
import { StepMarker, StepMarkerConnector } from "@/components/denhaag-step-marker";
import {
  Step,
  StepHeader,
  StepHeading,
  StepHeaderToggle,
  StepDetails
} from "@/components/denhaag-process-steps";
import { formatDate, shortDateOptions } from "@gemeente-denhaag/utils";
import "./index.scss";
import ContactTimelineMetaSeparator from "./ContactTimelineMetaSeparator.vue";
import ContactTimelineMetaItem from "./ContactTimelineMetaItem.vue";
import ContactTimelineMetaTimeItem from "./ContactTimelineMetaTimeItem.vue";
import ContactTimelineMeta from "./ContactTimelineMeta.vue";
import ContactTimelineHeaderContent from "./ContactTimelineHeaderContent.vue";
import ContactTimelineHeaderDate from "./ContactTimelineHeaderDate.vue";
import ContactTimelineHeaderChannel from "./ContactTimelineHeaderChannel.vue";
import ContactTimelineItemSender from "./ContactTimelineItemSender.vue";
import ContactTimelineItemFile from "./ContactTimelineItemFile.vue";
import { computed, getCurrentInstance } from "vue";

export interface ContactTimelineItemProps {
  id: string;
  title: string;
  description?: string;
  isoDate?: string;
  sender?: string;
  channel: string;
}

export interface ContactTimelineItemSlots {
  title?(props: ContactTimelineItemProps): unknown;
  description?(props: ContactTimelineItemProps): unknown;
  file?(props: ContactTimelineItemProps): unknown;
  sender?(props: ContactTimelineItemProps): unknown;
  date?(props: ContactTimelineItemProps): unknown;
  channel?(props: ContactTimelineItemProps): unknown;
}

export interface Labels {
  today: string;
  yesterday: string;
}

interface ContactTimelineItemInternalProps extends ContactTimelineItemProps {
  labels: Labels;
  locale?: string;
  nextItem?: boolean;
  expanded?: boolean;
}

defineProps<ContactTimelineItemInternalProps>();
defineSlots<ContactTimelineItemSlots>();
defineEmits<{ toggleExpanded: [] }>();

const hasOnToggleExpandedListener = computed(
  () => !!getCurrentInstance()?.vnode?.props?.onToggleExpanded
);
</script>
