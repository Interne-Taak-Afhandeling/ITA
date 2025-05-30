<template>
  <form ref="formRef" @submit.prevent="saveOnly">
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
    </utrecht-fieldset>
    <utrecht-form-field>
      <utrecht-form-label for="kanalen">Kanaal</utrecht-form-label>
      <utrecht-select required id="kanalen" v-model="form.kanaal" :options="kanalen" />
    </utrecht-form-field>
    <utrecht-form-field>
      <utrecht-form-label for="informatie-burger"
        >Informatie voor burger / bedrijf</utrecht-form-label
      >
      <utrecht-textarea
        :required="form.resultaat === RESULTS.contactGelukt"
        id="informatie-burger"
        v-model="form.informatieBurger"
      />
    </utrecht-form-field>

    <utrecht-button-group>
      <utrecht-button
        type="button"
        appearance="primary-action-button"
        :disabled="isLoading"
        @click="showConfirmation"
      >
        <span v-if="isLoading">Bezig met opslaan...</span>
        <span v-else>Opslaan & afronden</span>
      </utrecht-button>
      <utrecht-button type="submit" appearance="secondary-action-button" :disabled="isLoading">
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
import { ref } from "vue";
import { toast } from "./toast/toast";
import BevestigingsModal from "./BevestigingsModal.vue";
import { useRouter } from "vue-router";

const { taak } = defineProps<{ taak: Internetaken }>();
const emit = defineEmits<{ success: [] }>();

const router = useRouter();

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
  resultaat: RESULTS.contactGelukt as (typeof RESULTS)[keyof typeof RESULTS],
  kanaal: "",
  informatieBurger: ""
});

function showConfirmation() {
  //HTML5 form validatie
  if (!formRef.value?.checkValidity()) {
    formRef.value?.reportValidity();
    return;
  }

  bevestigingsModalRef.value?.show();
}

async function saveOnly() {
  isLoading.value = true;
  try {
    await klantcontactService.createRelatedKlantcontact({
      klantcontactRequest: buildKlantcontactModel(),
      aanleidinggevendKlantcontactUuid: getAanleidinggevendKlantcontactId(),
      partijUuid: getPartijId()
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
      interneTaakId: taak.uuid
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

    if (betrokkene._expand?.wasPartij && "uuid" in betrokkene._expand.wasPartij) {
      partijUuid = betrokkene._expand.wasPartij.uuid;
      console.log("Using partijUuid from expand.wasPartij:", partijUuid);
    }

    // Als fallback, check ook direct in wasPartij
    else if (betrokkene.wasPartij && "uuid" in betrokkene.wasPartij) {
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
    resultaat: RESULTS.contactGelukt,
    kanaal: "",
    informatieBurger: ""
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
