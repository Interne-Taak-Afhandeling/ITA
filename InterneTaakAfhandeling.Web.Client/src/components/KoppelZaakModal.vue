<template>
  <div>
    <utrecht-button type="button" appearance="primary-action-button" @click="openModal">
      {{ isZaakGekoppeld ? "Zaakkoppeling wijzigen" : "Koppelen aan zaak" }}
    </utrecht-button>

    <dialog ref="modalDialog" class="modal-dialog">
      <div class="modal-content">
        <form method="dialog" @submit.prevent="koppelZaak">
          <utrecht-heading :level="2">{{
            isZaakGekoppeld ? "Zaakkoppeling wijzigen" : "Koppel aan zaak"
          }}</utrecht-heading>

          <utrecht-paragraph>
            {{ "Aan welke zaak wil je dit contact koppelen?" }}
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

          <div v-if="isLoading" class="spinner-container">
            <simple-spinner />
            <span>Bezig met koppelen...</span>
          </div>

          <utrecht-button-group>
            <utrecht-button appearance="primary-action-button" type="submit">
              Koppelen
            </utrecht-button>
            <utrecht-button appearance="secondary-action-button" type="button" @click="closeModal">
              Annuleren
            </utrecht-button>
          </utrecht-button-group>

          <!-- <div v-else class="button-container margin-top">
            <utrecht-button appearance="primary-action-button" type="submit">
              Koppelen
            </utrecht-button>
            <utrecht-button appearance="secondary-action-button" type="button" @click="closeModal">
              Annuleren
            </utrecht-button>
          </div> -->
        </form>
      </div>
    </dialog>
  </div>
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

const isZaakGekoppeld = computed(() => !!props.zaakIdentificatie);

const modalDialog = ref<HTMLDialogElement | null>(null);
const zaakNummer = ref("");
const isLoading = ref(false);
const error = ref<string | null>(null);

const openModal = () => {
  error.value = null;
  zaakNummer.value = "";
  if (modalDialog.value) {
    modalDialog.value.showModal();
  }
};

const closeModal = () => {
  if (modalDialog.value) {
    modalDialog.value.close();
  }
};

const koppelZaak = async () => {
  if (!zaakNummer.value) {
    error.value = "Voer een geldig zaaknummer in";
    return;
  }

  error.value = null;
  isLoading.value = true;

  try {
    const response = await fetch("/api/koppelzaak/koppel-zaak-aan-klantcontact", {
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
          "Het koppelen van een nieuwe zaak wordt niet ondersteund omdat er al meerdere zaken gekoppeld zijn aan dit contact."
        );
      }

      throw new Error(errorData.detail || "Er is een fout opgetreden bij het koppelen van de zaak");
    }

    const result = await response.json();

    if (result.zaak) {
      toast.add({ text: "Zaak succesvol gekoppeld", type: "ok" });
      emit("zaakGekoppeld", result.zaak);
      closeModal();
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

<style scoped>
.margin-top {
  margin-top: 1rem;
}

.modal-dialog {
  padding: 0;
  border: none;
  border-radius: 0.5rem;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
  max-width: 90%;
  width: 550px;
}

.modal-dialog::backdrop {
  background-color: rgba(0, 0, 0, 0.3);
}

.modal-content {
  padding: 2rem;
}

.spinner-container {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  margin-top: 1.5rem;
  margin-bottom: 1rem;
}

.spinner-container span {
  margin-top: 1rem;
  font-weight: 500;
}

.button-container {
  display: flex;
  gap: 1rem;
}
</style>
