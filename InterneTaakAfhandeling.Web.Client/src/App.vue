<template>
  <the-header />

  <main id="main">
    <section class="utrecht-page utrecht-page-content">
      <router-view />
    </section>
  </main>

  <the-footer />

  <toast-section />
</template>

<script setup lang="ts">
import { RouterView } from "vue-router";
import TheHeader from "./components/TheHeader.vue";
import TheFooter from "./components/TheFooter.vue";
import ToastSection from "./components/toast/ToastSection.vue";
</script>
