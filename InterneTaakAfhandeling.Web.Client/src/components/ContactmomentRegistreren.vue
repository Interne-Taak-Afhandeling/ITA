<template>
  <form ref="formRef" @submit.prevent="showConfirmation">
    <utrecht-fieldset>
      <utrecht-legend>Kies een handeling</utrecht-legend>
      <utrecht-form-field v-for="(label, key) in HANDLINGS" :key="key" type="radio">
        <utrecht-radiobutton
          name="handeling"
          :id="key"
          :value="label"
          v-model="form.handeling"
          required
        />
        <utrecht-form-label :for="key" type="radio">{{ label }}</utrecht-form-label>
      </utrecht-form-field>
    </utrecht-fieldset>

    <utrecht-fieldset v-if="isContactmoment">
      <utrecht-fieldset>
        <utrecht-legend>Resultaat</utrecht-legend>
        <utrecht-form-field v-for="(label, key) in RESULTS" :key="key" type="radio">
          <utrecht-radiobutton
            name="resultaat"
            :id="key"
            :value="label"
            v-model="form.resultaat"
            required
          />
          <utrecht-form-label :for="key" type="radio">{{ label }}</utrecht-form-label>
        </utrecht-form-field>
      </utrecht-fieldset>

      <utrecht-form-field>
        <utrecht-form-label for="kanalen">Kanaal</utrecht-form-label>
        <utrecht-select required id="kanalen" v-model="form.kanaal" :options="kanalen" />
      </utrecht-form-field>
      <utrecht-form-field>
        <utrecht-form-label for="informatie-burger"
          >Informatie voor burger / bedrijf</utrecht-form-label
        >
        <utrecht-textarea required id="informatie-burger" v-model="form.informatieBurger" />
      </utrecht-form-field>
    </utrecht-fieldset>
    <interne-toelichting-section>
      <utrecht-form-field>
        <utrecht-form-label for="interne-toelichting-text">
          Interne toelichting
        </utrecht-form-label>
        <utrecht-textarea
          id="interne-toelichting-text"
          v-model="form.interneNotitie"
          placeholder="Optioneel"
          :required="!isContactmoment"
        />
        <utrecht-paragraph small>
          Deze toelichting is alleen voor medewerkers te zien en is verborgen voor de burger/het
          bedrijf.
        </utrecht-paragraph>
      </utrecht-form-field>
    </interne-toelichting-section>

    <utrecht-button-group>
      <utrecht-button
        type="submit"
        appearance="primary-action-button"
        :disabled="isLoading"
        v-if="isContactmoment"
      >
        <span v-if="isLoading">Bezig met opslaan...</span>
        <span v-else>Opslaan & afronden</span>
      </utrecht-button>
      <utrecht-button
        type="button"
        appearance="secondary-action-button"
        :disabled="isLoading"
        @click="isContactmoment ? saveContactmoment() : saveNote()"
      >
        <span v-if="isLoading">Bezig met opslaan...</span>
        <span v-else>Alleen opslaan</span>
      </utrecht-button>
    </utrecht-button-group>
  </form>

  <bevestigings-modal
    ref="bevestigingsModalRef"
    title="Contactverzoek afronden"
    message="Weet je zeker dat je het contactverzoek wilt opslaan en afronden?"
    confirm-text="Opslaan & afronden"
    cancel-text="Annuleren"
    @confirm="finishContactmoment()"
  />
</template>

<script setup lang="ts">
import { klantcontactService } from "@/services/klantcontactService";
import type { CreateKlantcontactRequest, Internetaken } from "@/types/internetaken";
import { computed, ref } from "vue";
import { toast } from "./toast/toast";
import BevestigingsModal from "./BevestigingsModal.vue";
import { useRouter } from "vue-router";
import { internetakenService } from "@/services/internetakenService";
import InterneToelichtingSection from "./InterneToelichtingSection.vue";

const { taak } = defineProps<{ taak: Internetaken }>();
const emit = defineEmits<{ success: [] }>();
const router = useRouter();
const HANDLINGS = {
  contactmoment: "Contactmoment",
  interneToelichting: "Interne toelichting"
} as const;

const RESULTS = {
  contactGelukt: "Contact opnemen gelukt",
  geenGehoor: "Contact opnemen niet gelukt"
} as const;

const kanalen = [
  { label: "Selecteer een kanaal", value: "" },
  ...["Balie", "Telefoon"].map((value) => ({ label: value, value }))
];

const isLoading = ref(false);
const bevestigingsModalRef = ref<InstanceType<typeof BevestigingsModal>>();
const formRef = ref<HTMLFormElement>();

const form = ref({
  handeling: HANDLINGS.contactmoment as (typeof HANDLINGS)[keyof typeof HANDLINGS],
  resultaat: RESULTS.contactGelukt as (typeof RESULTS)[keyof typeof RESULTS],
  kanaal: "",
  informatieBurger: "",
  interneNotitie: ""
});
const isContactmoment = computed(() => form.value.handeling === HANDLINGS.contactmoment);
function showConfirmation() {
  bevestigingsModalRef.value?.show();
}
function isValidForm() {
  if (!formRef.value?.checkValidity()) {
    formRef.value?.reportValidity();
    return false;
  }
  return true;
}
async function saveNote() {
  if (!isValidForm()) return;
  isLoading.value = true;
  try {
    await internetakenService.addNoteToInternetaak(taak.uuid, form.value.interneNotitie.trim());
    toast.add({ text: "Notitie succesvol toegevoegd", type: "ok" });
    resetForm();
    emit("success");
  } catch (err: unknown) {
    handleError(err);
  } finally {
    isLoading.value = false;
  }
}

async function saveContactmoment() {
  if (!isValidForm()) return;
  isLoading.value = true;
  try {
    await klantcontactService.createRelatedKlantcontact(getKlantcontactPayload());
    toast.add({ text: "Contactmoment succesvol bijgewerkt", type: "ok" });
    resetForm();
    emit("success");
  } catch (err: unknown) {
    handleError(err);
  } finally {
    isLoading.value = false;
  }
}

async function finishContactmoment() {
  if (!isValidForm()) return;
  isLoading.value = true;
  try {
    await klantcontactService.createRelatedKlantcontactAndCloseInterneTaak(
      getKlantcontactPayload()
    );
    toast.add({ text: "Contactmoment succesvol opgeslagen en afgerond", type: "ok" });
    router.push({ name: "dashboard" });
  } catch (err: unknown) {
    handleError(err);
  } finally {
    isLoading.value = false;
  }
}
function getKlantcontactPayload() {
  return {
    klantcontactRequest: buildKlantcontactModel(),
    aanleidinggevendKlantcontactUuid: getAanleidinggevendKlantcontactId(),
    partijUuid: getPartijId(),
    interneTaakId: taak.uuid,
    interneNotitie: form.value.interneNotitie
  };
}
function getAanleidinggevendKlantcontactId() {
  return taak.aanleidinggevendKlantcontact?.uuid;
}

function getPartijId() {
  let partijUuid: string | undefined = undefined;

  if (taak.aanleidinggevendKlantcontact?._expand?.hadBetrokkenen?.[0]) {
    const betrokkene = taak.aanleidinggevendKlantcontact._expand.hadBetrokkenen[0];

    if (betrokkene.wasPartij && "uuid" in betrokkene.wasPartij) {
      partijUuid = betrokkene.wasPartij.uuid;
    }
  }
  return partijUuid;
}

function buildKlantcontactModel(): CreateKlantcontactRequest {
  return {
    kanaal: form.value.kanaal,
    onderwerp: taak.aanleidinggevendKlantcontact?.onderwerp || "Opvolging contactverzoek",
    inhoud: form.value.informatieBurger,
    indicatieContactGelukt: form.value.resultaat === RESULTS.contactGelukt,
    taal: "nld", // ISO 639-2/B formaat
    vertrouwelijk: false,
    plaatsgevondenOp: new Date().toISOString()
  };
}

function resetForm() {
  form.value = {
    handeling: HANDLINGS.contactmoment,
    resultaat: RESULTS.contactGelukt,
    kanaal: "",
    informatieBurger: "",
    interneNotitie: ""
  };
}

function handleError(err: unknown) {
  const message = err instanceof Error && err.message ? err.message : "Er is een fout opgetreden.";
  toast.add({ text: message, type: "error" });
}
</script>

<style lang="scss" scoped>
.utrecht-form-label {
  display: block;
}

.utrecht-button-group {
  margin-top: 1rem;
}

.ita-interne-toelichting-section {
  .utrecht-form-field {
    margin-bottom: 0px;
  }

  .utrecht-paragraph--small {
    color: var(--ita-interne-toelichting-section-small-text-color);
    margin: 0px;
  }
}
</style>
