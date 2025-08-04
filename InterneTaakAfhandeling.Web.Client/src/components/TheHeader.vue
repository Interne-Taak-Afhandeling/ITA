<template>
  <utrecht-page-header v-if="isAuthenticated">
    <div class="container-like-utrecht-page header-wrapper">
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
      <nav class="utrecht-nav-bar" aria-label="Hoofdmenu">
        <div class="utrecht-nav-bar__content">
          <ul role="list" class="utrecht-nav-list" id="menu">
            <li class="utrecht-nav-list__item" v-for="item in navItems" :key="item.name">
              <router-link
                :to="{ name: item.route }"
                class="utrecht-link utrecht-link--html-a utrecht-nav-list__link"
              >
                {{ item.name }}
              </router-link>
            </li>
            <li
              class="user-name utrecht-nav-list__item utrecht-link utrecht-link--html-a utrecht-nav-list__link"
            >
              {{ user?.name }}
            </li>
            <li class="utrecht-nav-list__item">
              <a
                href="/api/logoff"
                class="utrecht-link utrecht-link--html-a utrecht-nav-list__link"
              >
                Uitloggen
              </a>
            </li>
          </ul>
        </div>
      </nav>
    </div>
  </utrecht-page-header>
</template>

<script lang="ts" setup>
import { computed } from "vue";
import { injectResources } from "@/resources";
import { useAuthStore } from "@/stores/auth";
import { useRoute } from "vue-router";

const route = useRoute();
const resources = injectResources();
const authStore = useAuthStore();
const isAuthenticated = computed(() => authStore.isAuthenticated);
const user = computed(() => authStore.user);

const isBeheer = computed(() => route.path.startsWith("/beheer"));
const baseNavItems = [
  { name: "Mijn werkvoorraad", route: "dashboard" },
  { name: "Afdelingswerkvoorraad", route: "afdelingsContacten" },
  { name: "Mijn historie", route: "historie" },
  { name: "Afdelingshistorie", route: "afdelingsContactenHistorie" },
  { name: "Alle contactverzoeken", route: "alleContactverzoeken" },
  { name: "Beheer", route: "beheer" }
];

const beheerNavItem = { name: "Kanalen", route: "kanalen" };

const navItems = computed(() => {
  return isBeheer.value ? [...baseNavItems, beheerNavItem] : baseNavItems;
});

const svg = computed(() => {
  if (!resources?.logoUrl?.endsWith(".svg")) return;

  const svgTemplateId = btoa(resources.logoUrl);

  return (document.getElementById(svgTemplateId) as HTMLTemplateElement)?.innerHTML;
});
</script>

<style lang="scss" scoped>
.container-like-utrecht-page {
  margin-inline-end: auto;
  margin-inline-start: auto;
  max-inline-size: calc(
    var(--utrecht-page-max-inline-size) - var(--utrecht-page-margin-inline-start, 0px) - var(
        --utrecht-page-margin-inline-end,
        0px
      )
  );
}

.header-wrapper {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  justify-content: space-between;
  position: relative;
  inline-size: 100%;
}

.utrecht-page-header :deep(.utrecht-link) {
  color: currentColor !important;
}

:has(+ .user-name) {
  flex: 1;
}
</style>
