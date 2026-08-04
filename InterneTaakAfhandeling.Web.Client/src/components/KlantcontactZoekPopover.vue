<template>
  <li class="utrecht-nav-list__item klantcontact-zoek">
    <button
      ref="buttonRef"
      type="button"
      class="utrecht-button klantcontact-zoek__button"
      aria-haspopup="true"
      :aria-expanded="open"
      aria-label="Zoek contactverzoek op klantcontactnummer"
      title="Zoek contactverzoek op klantcontactnummer"
      @click="toggle"
    >
      <utrecht-icon icon="search-klantcontact" />
    </button>

    <div
      v-if="open"
      ref="panelRef"
      class="klantcontact-zoek__panel"
      aria-label="Zoek contactverzoek op klantcontactnummer"
    >
      <form class="klantcontact-zoek__form" @submit.prevent="onSearch">
        <utrecht-form-field class="klantcontact-zoek__field">
          <utrecht-form-label for="klantcontact-nummer-input"
            >Klantcontactnummer</utrecht-form-label
          >
          <div class="klantcontact-zoek__row">
            <utrecht-textbox
              id="klantcontact-nummer-input"
              ref="inputRef"
              v-model="nummer"
              type="text"
            />

            <utrecht-button
              type="submit"
              appearance="secondary-action-button"
              :busy="isSearching"
              :disabled="isSearching"
            >
              Zoeken
            </utrecht-button>
          </div>
        </utrecht-form-field>

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
        "Geen contactverzoek gevonden voor dit klantcontactnummer.";
    }
  } finally {
    isSearching.value = false;
  }
}
</script>

<style lang="scss" scoped>
.klantcontact-zoek {
  position: relative;
  align-items: center;
  border-inline-start: 1px solid currentColor;
  padding-inline-start: var(--utrecht-space-column-sm);
}

.klantcontact-zoek__button {
  --utrecht-button-icon-size: var(--utrecht-accordion-button-icon-size);
  --utrecht-button-min-block-size: var(--utrecht-space-column-3xl);
  --utrecht-button-min-inline-size: var(--utrecht-space-column-3xl);
  background-color: var(--utrecht-color-blue-60);
  color: var(--utrecht-page-header-color);
  border: none;
  border-radius: 50%;
  padding: var(--utrecht-space-column-2xs);
  display: inline-flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
}

.klantcontact-zoek__button:hover,
.klantcontact-zoek__button:focus-visible {
  background-color: var(--utrecht-document-background-color);
  color: var(--utrecht-page-header-background-color);
}

.klantcontact-zoek__panel {
  position: absolute;
  inset-block-start: 100%;
  inset-inline-end: 0;
  z-index: 10;
  min-inline-size: 16rem;
  background-color: var(--utrecht-document-background-color);
  color: var(--utrecht-color-grey-15);
  border-radius: var(--utrecht-form-control-border-radius, 0);
  padding: var(--utrecht-space-column-sm);
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.2);
}

.klantcontact-zoek__row {
  display: flex;
  align-items: flex-end;
  gap: var(--utrecht-space-column-xs);
}

.klantcontact-zoek__error {
  margin-block-start: var(--utrecht-space-row-xs);
}
</style>
