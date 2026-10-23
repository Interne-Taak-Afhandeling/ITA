<template>
  <div role="status" class="ita-werklijst-bevestiging">
    <utrecht-alert v-if="isVisible && bevestiging" :type="bevestiging.type" role="none">
      <div class="ita-werklijst-bevestiging__content">
        <span class="preserve-newline">{{ bevestiging.tekst }}</span>
        <utrecht-button
          type="button"
          appearance="subtle-button"
          aria-label="Melding sluiten"
          @click="sluiten"
          >Sluiten</utrecht-button
        >
      </div>
    </utrecht-alert>
  </div>
</template>

<script setup lang="ts">
import { nextTick, onMounted, ref } from "vue";
import UtrechtAlert from "@/components/UtrechtAlert.vue";
import { useWerklijstBevestiging } from "@/composables/use-werklijst-bevestiging";

const { bevestiging, clearWerklijstBevestiging } = useWerklijstBevestiging();
const isVisible = ref(false);

function focusPageTitle() {
  const heading = document.querySelector<HTMLElement>("main h1");
  if (!heading) return;
  heading.tabIndex = -1;
  heading.focus();
}

function sluiten() {
  clearWerklijstBevestiging();
  focusPageTitle();
}

onMounted(async () => {
  await nextTick();
  isVisible.value = true;
  if (bevestiging.value) focusPageTitle();
});
</script>

<style lang="scss" scoped>
.ita-werklijst-bevestiging:not(:empty) {
  margin-block-end: 1rem;
}

.ita-werklijst-bevestiging__content {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  justify-content: space-between;
  gap: 1rem;
}
</style>
