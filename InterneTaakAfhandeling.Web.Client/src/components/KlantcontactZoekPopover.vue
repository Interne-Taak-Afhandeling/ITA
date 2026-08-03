<template>
  <li class="utrecht-nav-list__item klantcontact-zoek">
    <button
      ref="buttonRef"
      type="button"
      class="utrecht-button utrecht-button--subtle klantcontact-zoek__button"
      aria-haspopup="true"
      :aria-expanded="open"
      aria-label="Zoek contactverzoek op klantcontactnummer"
      title="Zoek contactverzoek op klantcontactnummer"
      @click="toggle"
    >
      <utrecht-icon icon="external" />
    </button>

    <div
      v-if="open"
      ref="panelRef"
      class="klantcontact-zoek__panel"
      aria-label="Zoek contactverzoek op klantcontactnummer"
    >
      <form @submit.prevent="onSearch">
        <utrecht-form-field>
          <utrecht-form-label for="klantcontact-nummer-input"
            >Klantcontactnummer</utrecht-form-label
          >
          <utrecht-textbox
            id="klantcontact-nummer-input"
            ref="inputRef"
            v-model="nummer"
            type="text"
            required
          />
        </utrecht-form-field>

        <utrecht-button
          type="submit"
          appearance="primary-action-button"
          :busy="isSearching"
          :disabled="isSearching"
        >
          Zoeken
        </utrecht-button>

        <p v-if="errorMessage" class="klantcontact-zoek__error" role="alert" aria-live="polite">
          {{ errorMessage }}
        </p>
      </form>
    </div>
  </li>
</template>

<script setup lang="ts">
import { nextTick, ref } from "vue";
import { useRouter } from "vue-router";
import { onClickOutside, onKeyStroke } from "@vueuse/core";
import UtrechtIcon from "@/components/UtrechtIcon.vue";
import { internetakenService } from "@/services/internetakenService";
import { knownErrorMessages } from "@/utils/fetchWrapper";

const router = useRouter();

const open = ref(false);
const nummer = ref("");
const isSearching = ref(false);
const errorMessage = ref<string | null>(null);

const buttonRef = ref<HTMLButtonElement | null>(null);
const panelRef = ref<HTMLElement | null>(null);
const inputRef = ref<{ $el: HTMLElement } | null>(null);

onClickOutside(
  panelRef,
  () => {
    close();
  },
  { ignore: [buttonRef] }
);

onKeyStroke("Escape", () => {
  if (open.value) close();
});

async function toggle() {
  if (open.value) {
    close();
    return;
  }
  open.value = true;
  errorMessage.value = null;
  await nextTick();
  inputRef.value?.$el?.querySelector("input")?.focus();
}

function close() {
  open.value = false;
  errorMessage.value = null;
  nummer.value = "";
}

async function onSearch() {
  if (!nummer.value) return;

  isSearching.value = true;
  errorMessage.value = null;
  try {
    await internetakenService.getByKlantcontactNummer(nummer.value);
    const contactmomentNummer = nummer.value;
    close();
    router.push({
      name: "contactmomentDetail",
      params: { contactmomentNumber: contactmomentNummer }
    });
  } catch (err: unknown) {
    if (err instanceof Error && err.message === knownErrorMessages.forbidden) {
      errorMessage.value = "Je hebt geen toegang tot dit contactverzoek.";
    } else {
      errorMessage.value =
        "Er is iets misgegaan bij het opzoeken van dit contactverzoek. Neem contact op met de beheerder als dit probleem zich blijft voordoen.";
    }
  } finally {
    isSearching.value = false;
  }
}
</script>

<style lang="scss" scoped>
.klantcontact-zoek {
  position: relative;
  border-inline-start: 1px solid var(--utrecht-form-control-border-color, currentColor);
  padding-inline-start: 1rem;
}

.klantcontact-zoek__button {
  inline-size: 2rem;
  block-size: 2rem;
  border-radius: 50%;
  display: inline-flex;
  align-items: center;
  justify-content: center;
}

.klantcontact-zoek__panel {
  position: absolute;
  inset-block-start: 100%;
  inset-inline-end: 0;
  z-index: 10;
  min-inline-size: 16rem;
  background-color: var(--utrecht-document-background-color, Canvas);
  border: 1px solid var(--utrecht-form-control-border-color, currentColor);
  border-radius: var(--utrecht-form-control-border-radius, 0);
  padding: 1rem;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.2);
}

.klantcontact-zoek__error {
  margin-block-start: 0.5rem;
}
</style>
