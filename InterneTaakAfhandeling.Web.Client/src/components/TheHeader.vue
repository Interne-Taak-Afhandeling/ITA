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
            <template v-if="$route.meta.showNav">
              <li class="utrecht-nav-list__item">
                <router-link
                  :to="{ name: 'dashboard' }"
                  class="utrecht-link utrecht-link--html-a utrecht-nav-list__link"
                  >Mijn werkvoorraad</router-link
                >
              </li>

              <li class="utrecht-nav-list__item">
                <router-link
                  :to="{ name: 'afdelingsContacten' }"
                  class="utrecht-link utrecht-link--html-a utrecht-nav-list__link"
                  >Afdelingswerkvoorraad</router-link
                >
              </li>

              <li class="utrecht-nav-list__item">
                <router-link
                  :to="{ name: 'historie' }"
                  class="utrecht-link utrecht-link--html-a utrecht-nav-list__link"
                  >Mijn historie</router-link
                >
              </li>
              <li class="utrecht-nav-list__item">
                <router-link
                  :to="{ name: 'afdelingsContactenHistorie' }"
                  class="utrecht-link utrecht-link--html-a utrecht-nav-list__link"
                  >Afdelingshistorie</router-link
                >
              </li>
              <li class="utrecht-nav-list__item">
                <router-link
                  :to="{ name: 'alleContactverzoeken' }"
                  class="utrecht-link utrecht-link--html-a utrecht-nav-list__link"
                  >Alle contactverzoeken</router-link
                >
              </li>
              <li class="utrecht-nav-list__item">
                <router-link
                  :to="{ name: 'beheer' }"
                  class="utrecht-link utrecht-link--html-a utrecht-nav-list__link"
                  >Beheer</router-link
                >
              </li>
            </template>
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

const resources = injectResources();
const authStore = useAuthStore();
const isAuthenticated = computed(() => authStore.isAuthenticated);
const user = computed(() => authStore.user);

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

.utrecht-nav-list {
  display: flex;
  justify-content: space-between;
  align-items: center;
  width: 100%;
}

.user-name {
  margin-left: auto;
}
</style>
