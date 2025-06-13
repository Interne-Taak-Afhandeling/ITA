<template>
  <utrecht-button type="button" appearance="secondary-action-button" @click="show">
    {{ isZaakGekoppeld ? "Zaakkoppeling wijzigen" : "Koppelen aan zaak" }}
  </utrecht-button>
  <utrecht-alert-dialog ref="zaakKoppelenAlertRef">
    <form method="dialog" @submit.prevent="koppelZaak">
      <utrecht-heading :level="2">{{
        isZaakGekoppeld ? "Zaakkoppeling wijzigen" : "Koppel aan zaak"
      }}</utrecht-heading>

      <div v-if="isLoading" class="spinner-container">
        <simple-spinner />
      </div>

      <template v-else>
        <utrecht-paragraph>
          {{ "Aan welke zaak wil je dit contactverzoek koppelen?" }}
        </utrecht-paragraph>

        <utrecht-form-field>
          <utrecht-form-label for="zaak-nummer">Zaaknummer</utrecht-form-label>
          <utrecht-textbox
            id="zaak-nummer"
            v-model="zaakNummer"
            placeholder="Voer hier een zaaknummer in"
          />
        </utrecht-form-field>

        <utrecht-alert v-if="error" appeareance="error" class="margin-top">
          {{ error }}
        </utrecht-alert>

        <utrecht-button-group>
          <utrecht-button appearance="primary-action-button" type="submit">
            Koppelen
          </utrecht-button>
          <utrecht-button appearance="secondary-action-button" type="button" @click="close">
            Annuleren
          </utrecht-button>
        </utrecht-button-group>
      </template>
    </form>
  </utrecht-alert-dialog>
</template>

<script setup lang="ts">
import { ref, computed } from "vue";
import { toast } from "@/components/toast/toast";
import UtrechtAlert from "@/components/UtrechtAlert.vue";
import SimpleSpinner from "@/components/SimpleSpinner.vue";

const props = defineProps<{
  aanleidinggevendKlantcontactUuid: string;
  zaakIdentificatie?: string;
}>();

const emit = defineEmits(["zaakGekoppeld"]);

const zaakKoppelenAlertRef = ref<{ dialogRef?: HTMLDialogElement }>();

const isZaakGekoppeld = computed(() => !!props.zaakIdentificatie);

const zaakNummer = ref("");
const isLoading = ref(false);
const error = ref<string | null>(null);

const show = () => {
  zaakKoppelenAlertRef.value?.dialogRef?.showModal();
  error.value = null;
  zaakNummer.value = "";
};

const close = () => zaakKoppelenAlertRef.value?.dialogRef?.close();

const koppelZaak = async () => {
  if (!zaakNummer.value) {
    error.value = "Voer een geldig zaaknummer in";
    return;
  }

  error.value = null;
  isLoading.value = true;

  try {
    const response = await fetch("/api/internetaken/koppel-zaak", {
      method: "POST",
      headers: {
        "Content-Type": "application/json"
      },
      body: JSON.stringify({
        zaakIdentificatie: zaakNummer.value,
        aanleidinggevendKlantcontactUuid: props.aanleidinggevendKlantcontactUuid
      })
    });

    if (!response.ok) {
      const errorData = await response.json();

      // Specifieke foutafhandeling voor meerdere gekoppelde zaken
      if (errorData.conflictCode === "MEERDERE_ZAKEN_GEKOPPELD") {
        throw new Error(
          "Het koppelen van een nieuwe zaak wordt niet ondersteund omdat er al meerdere zaken gekoppeld zijn aan dit contactverzoek."
        );
      }

      throw new Error(errorData.detail || "Er is een fout opgetreden bij het koppelen van de zaak");
    }

    const result = await response.json();

    if (result.zaak) {
      toast.add({ text: "Zaak succesvol gekoppeld", type: "ok" });
      emit("zaakGekoppeld", result.zaak);
      close();
    } else {
      throw new Error("Geen zaakgegevens ontvangen van de server");
    }
  } catch (err: unknown) {
    console.error("Error bij koppelen zaak:", err);
    error.value =
      err instanceof Error && err.message
        ? err.message
        : "Er is een fout opgetreden bij het koppelen van de zaak";
  } finally {
    isLoading.value = false;
  }
};
</script>
