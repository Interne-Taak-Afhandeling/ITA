<template>
  <utrecht-page-header>
    <utrecht-skip-link href="#main">Naar inhoud</utrecht-skip-link>

    <utrecht-skip-link href="#menu">Naar menu</utrecht-skip-link>

    <router-link
      v-if="resources?.logoUrl"
      :to="{ name: 'dashboard' }"
      class="utrecht-link utrecht-link--html-a utrecht-link--box-content"
    >
      <figure v-if="svg" v-html="svg" class="utrecht-logo"></figure>

      <figure v-else class="utrecht-logo">
        <img :src="resources.logoUrl" :alt="`Logo ${resources.title}`" crossorigin="anonymous" />
      </figure>
    </router-link>

    <utrecht-nav-bar />

    <span class="utrecht-heading-2">{{ title }}</span>
  </utrecht-page-header>
</template>

<script lang="ts" setup>
import { computed } from "vue";
import UtrechtNavBar from "./UtrechtNavBar.vue";
import { injectResources } from "@/resources";

const resources = injectResources();

const svg = computed(() => {
  if (!resources?.logoUrl?.endsWith(".svg")) return;

  const svgTemplateId = btoa(resources.logoUrl);

  return (document.getElementById(svgTemplateId) as HTMLTemplateElement)?.innerHTML;
});

const title = computed(() => (resources?.title ? resources.title : "Interne Taak Afhandeling"));
</script>

<style lang="scss" scoped>
.utrecht-heading-2 {
  margin-inline: var(--ita-header-heading-margin-inline);
}
</style>
