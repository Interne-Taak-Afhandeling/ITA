<template>
  <utrecht-page-header v-if="isAuthenticated">
    <div class="utrecht-page">
      <utrecht-skip-link href="#main">Naar inhoud</utrecht-skip-link>

      <utrecht-skip-link href="#menu">Naar menu</utrecht-skip-link>

      <router-link
        v-if="resources?.logoUrl"
        :to="{ name: 'dashboard' }"
        class="utrecht-link utrecht-link--html-a utrecht-link--box-content"
      >
        <figure v-if="svg" v-html="svg" class="utrecht-logo"></figure>
        <figure v-else class="utrecht-logo">
          <img :src="resources.logoUrl" alt="Logo gemeente" crossorigin="anonymous" />
        </figure>
      </router-link>
      <utrecht-nav-bar />
      <user-info />
    </div>
  </utrecht-page-header>
</template>

<script lang="ts" setup>
import { computed } from "vue";
import UtrechtNavBar from "./UtrechtNavBar.vue";
import { injectResources } from "@/resources";
import UserInfo from "./UserInfo.vue";
import { useAuthStore } from "@/stores/auth";

const resources = injectResources();
const authStore = useAuthStore();
const isAuthenticated = computed(() => authStore.isAuthenticated);

const svg = computed(() => {
  if (!resources?.logoUrl?.endsWith(".svg")) return;

  const svgTemplateId = btoa(resources.logoUrl);

  return (document.getElementById(svgTemplateId) as HTMLTemplateElement)?.innerHTML;
});
</script>

<style lang="scss" scoped>
.utrecht-page {
  display: flex;
  align-items: center;
  justify-content: space-between;
  position: relative;
  inline-size: 100%;
}

.utrecht-page-header :deep(.utrecht-link) {
  color: currentColor !important;
}
</style>
