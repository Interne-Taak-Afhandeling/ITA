<template>
  <router-link :to="backButtonInfo.route">
    {{ backButtonInfo.text }}
  </router-link>
</template>

<script setup lang="ts">
import { useBackNavigation } from "@/composables/use-back-navigation";
const { backButtonInfo } = useBackNavigation();
</script>
