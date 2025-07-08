<template>
  <form ref="formRef" @submit.prevent="showConfirmation">
    <utrecht-fieldset>
      <utrecht-legend>Resultaat</utrecht-legend>
      <utrecht-form-field type="radio">
        <utrecht-radiobutton
          name="contact-gelukt"
          id="contact-gelukt"
          :value="RESULTS.contactGelukt"
          v-model="form.resultaat"
          required
        />
        <utrecht-form-label for="contact-gelukt" type="radio">
          {{ RESULTS.contactGelukt }}
        </utrecht-form-label>
      </utrecht-form-field>
      <utrecht-form-field type="radio">
        <utrecht-radiobutton
          name="contact-gelukt"
          id="geen-gehoor"
          :value="RESULTS.geenGehoor"
          v-model="form.resultaat"
          required
        />
        <utrecht-form-label for="geen-gehoor" type="radio">
          {{ RESULTS.geenGehoor }}
        </utrecht-form-label>
      </utrecht-form-field>
      <utrecht-form-field type="radio">
        <utrecht-radiobutton
          name="contact-gelukt"
          id="interne-notitie"
          :value="RESULTS.interneNotitie"
          v-model="form.resultaat"
          required
        />
        <utrecht-form-label for="interne-notitie" type="radio">
          {{ RESULTS.interneNotitie }}
        </utrecht-form-label>
      </utrecht-form-field>
    </utrecht-fieldset>
    <utrecht-form-field>
      <utrecht-form-label for="kanalen">Kanaal</utrecht-form-label>
      <utrecht-select 
        :required="!isInterneNotitie" 
        id="kanalen" 
        v-model="form.kanaal" 
        :options="kanalen" 
        :disabled="isInterneNotitie"
      />
    </utrecht-form-field>
    <utrecht-form-field>
      <utrecht-form-label for="informatie-burger"
        >Informatie voor burger / bedrijf</utrecht-form-label
      >
      <utrecht-textarea 
        :required="!isInterneNotitie" 
        id="informatie-burger" 
        v-model="form.informatieBurger" 
        :disabled="isInterneNotitie"
      />
    </utrecht-form-field>
    <utrecht-form-field>
      <utrecht-form-label for="interne-notitie-text">Interne notitie</utrecht-form-label>
      <utrecht-textarea 
        id="interne-notitie-text" 
        v-model="form.interneNotitie" 
        placeholder="Voeg hier een interne notitie toe (optioneel)"
      />
    </utrecht-form-field>

    <utrecht-button-group>
      <utrecht-button type="submit" appearance="primary-action-button" :disabled="isLoading">
        <span v-if="isLoading">Bezig met opslaan...</span>
        <span v-else>Opslaan & afronden</span>
      </utrecht-button>
      <utrecht-button
        type="button"
        appearance="secondary-action-button"
        :disabled="isLoading"
        @click="saveOnly"
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
    @confirm="saveAndFinish"
  />
</template>

<script setup lang="ts">
import { klantcontactService } from "@/services/klantcontactService";
import type { CreateKlantcontactRequest, Internetaken } from "@/types/internetaken";
import { ref, computed } from "vue";
import { toast } from "./toast/toast";
import BevestigingsModal from "./BevestigingsModal.vue";
import { useRouter } from "vue-router";

const { taak } = defineProps<{ taak: Internetaken }>();
const emit = defineEmits<{ success: [] }>();

const router = useRouter();

const RESULTS = {
  contactGelukt: "Contact opnemen gelukt",
  geenGehoor: "Contact opnemen niet gelukt",
  interneNotitie: "Interne notitie toevoegen"
} as const;

const kanalen = [
  { label: "Selecteer een kanaal", value: "" },
  ...["Balie", "Telefoon"].map((value) => ({ label: value, value }))
];

const isLoading = ref(false);
const bevestigingsModalRef = ref<InstanceType<typeof BevestigingsModal>>();
const formRef = ref<HTMLFormElement>();

const form = ref({
  resultaat: RESULTS.contactGelukt as (typeof RESULTS)[keyof typeof RESULTS],
  kanaal: "",
  informatieBurger: "",
  interneNotitie: ""
});

const isInterneNotitie = computed(() => form.value.resultaat === RESULTS.interneNotitie);

function showConfirmation() {
  bevestigingsModalRef.value?.show();
}

async function saveOnly() {
  if (isInterneNotitie.value && !form.value.interneNotitie.trim()) {
    toast.add({ text: "Interne notitie is verplicht wanneer je deze optie selecteert", type: "error" });
    return;
  }

  //HTML5 form validatie voor andere velden
  if (!formRef.value?.checkValidity()) {
    formRef.value?.reportValidity();
    return;
  }

  isLoading.value = true;
  try {
    await klantcontactService.createRelatedKlantcontact({
      klantcontactRequest: buildKlantcontactModel(),
      aanleidinggevendKlantcontactUuid: getAanleidinggevendKlantcontactId(),
      partijUuid: getPartijId(),
      interneTaakId: taak.uuid,
      interneNotitie: form.value.interneNotitie.trim() || undefined
    });
    toast.add({ text: "Contactmoment succesvol bijgewerkt", type: "ok" });
    resetForm();
    emit("success");
  } catch (err: unknown) {
    handleError(err);
  } finally {
    isLoading.value = false;
  }
}

async function saveAndFinish() {
  isLoading.value = true;
  try {
    await klantcontactService.createRelatedKlantcontactAndCloseInterneTaak({
      klantcontactRequest: buildKlantcontactModel(),
      aanleidinggevendKlantcontactUuid: getAanleidinggevendKlantcontactId(),
      partijUuid: getPartijId(),
      interneTaakId: taak.uuid,
      interneNotitie: form.value.interneNotitie.trim() || undefined
    });
    toast.add({ text: "Contactmoment succesvol opgeslagen en afgerond", type: "ok" });
    router.push({ name: "dashboard" });
  } catch (err: unknown) {
    handleError(err);
  } finally {
    isLoading.value = false;
  }
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
  if (isInterneNotitie.value) {
    // Voor interne notitie: geen klantcontact aanmaken, alleen logboek entry
    return {
      kanaal: "", // Leeg voor interne notitie
      onderwerp: "Interne notitie",
      inhoud: "", // Leeg voor interne notitie
      indicatieContactGelukt: undefined, // Niet van toepassing voor interne notitie
      taal: "nld",
      vertrouwelijk: false,
      plaatsgevondenOp: new Date().toISOString()
    };
  }

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
</style>