<template>
  <section class="ita-detail-section">
    <header class="ita-detail-section__header">
      <utrecht-heading :level="level">{{ title }}</utrecht-heading>
    </header>
    <div class="ita-detail-section__body">
      <slot></slot>
    </div>
  </section>
</template>

<script setup lang="ts">
const { level = 2 } = defineProps<{ title: string; level?: 2 | 3 | 4 }>();
</script>

<style lang="scss" scoped>
.ita-detail-section {
  background-color: var(--ita-detail-section-background-color);
  border-color: var(--ita-detail-section-border-color);
  border-radius: var(--ita-detail-section-border-radius, 0);
  border-style: solid;
  border-width: var(--ita-detail-section-border-width, 0);
  color: var(--ita-detail-section-color);
  margin-block-end: calc(
    var(--utrecht-space-around, 0) * var(--ita-detail-section-margin-block-end, 0)
  );
  margin-block-start: calc(
    var(--utrecht-space-around, 0) * var(--ita-detail-section-margin-block-start, 0)
  );
  padding-block-end: var(--ita-detail-section-padding-block-end);
  padding-block-start: var(--ita-detail-section-padding-block-start);
  padding-inline-end: var(--ita-detail-section-padding-inline-end);
  padding-inline-start: var(--ita-detail-section-padding-inline-start);
}

.ita-detail-section__header {
  display: flex;
  background-color: var(--ita-detail-section-header-background-color);
  border-color: var(--ita-detail-section-header-border-color);
  border-radius: var(--ita-detail-section-header-border-radius, 0);
  border-style: solid;
  border-width: var(--ita-detail-section-header-border-width, 0);
  color: var(--ita-detail-section-header-color);
  margin-block-end: calc(
    var(--utrecht-space-around, 0) * var(--ita-detail-section-header-margin-block-end, 0)
  );
  margin-block-start: calc(
    var(--utrecht-space-around, 0) * var(--ita-detail-section-header-margin-block-start, 0)
  );
  padding-block-end: var(--ita-detail-section-header-padding-block-end);
  padding-block-start: var(--ita-detail-section-header-padding-block-start);
  padding-inline-end: var(--ita-detail-section-header-padding-inline-end);
  padding-inline-start: var(--ita-detail-section-header-padding-inline-start);

  > * {
    margin: 0;
  }
}

.ita-detail-section__body {
  background-color: var(--ita-detail-section-body-background-color);
  border-color: var(--ita-detail-section-body-border-color);
  border-radius: var(--ita-detail-section-body-border-radius, 0);
  border-style: solid;
  border-width: var(--ita-detail-section-body-border-width, 0);
  color: var(--ita-detail-section-body-color);
  margin-block-end: calc(
    var(--utrecht-space-around, 0) * var(--ita-detail-section-body-margin-block-end, 0)
  );
  margin-block-start: calc(
    var(--utrecht-space-around, 0) * var(--ita-detail-section-body-margin-block-start, 0)
  );
  padding-block-end: var(--ita-detail-section-body-padding-block-end);
  padding-block-start: var(--ita-detail-section-body-padding-block-start);
  padding-inline-end: var(--ita-detail-section-body-padding-inline-end);
  padding-inline-start: var(--ita-detail-section-body-padding-inline-start);
  --current-padding-inline-start: var(--ita-detail-section-body-padding-inline-start);
}
</style>
