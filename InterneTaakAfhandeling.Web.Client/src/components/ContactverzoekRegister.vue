<template>
  <SimpleSpinner v-if="isLoading" />
  <form v-else ref="formRef" @submit.prevent="showConfirmation">
    <utrecht-fieldset>
      <utrecht-fieldset>
        <utrecht-legend>Resultaat</utrecht-legend>
        <utrecht-form-field v-for="(label, key) in RESULTS" :key="key" type="radio">
          <utrecht-radiobutton
            name="resultaat"
            :id="key"
            :value="label"
            v-model="registerContactmomentForm.resultaat"
            required
          />
          <utrecht-form-label :for="key" type="radio">{{ label }}</utrecht-form-label>
        </utrecht-form-field>
      </utrecht-fieldset>

      <utrecht-fieldset>
        <utrecht-legend>Wil je het contactmoment afsluiten?</utrecht-legend>
        <utrecht-form-field v-for="(label, key) in AFSLUITEN" :key="key" type="radio">
          <utrecht-radiobutton
            name="afsluiten"
            :id="key"
            :value="label"
            v-model="registerContactmomentForm.afsluiten"
            required
          />
          <utrecht-form-label :for="key" type="radio">{{ label }}</utrecht-form-label>
        </utrecht-form-field>
      </utrecht-fieldset>

      <utrecht-form-field>
        <utrecht-form-label for="kanalen">Kanaal</utrecht-form-label>
        <utrecht-select
          required
          id="kanalen"
          v-model="registerContactmomentForm.kanaal"
          :options="kanalen"
        />
      </utrecht-form-field>
      <utrecht-form-field>
        <utrecht-form-label for="informatie-burger"
          >Informatie voor burger / bedrijf</utrecht-form-label
        >
        <utrecht-textarea
          id="informatie-burger"
          v-model="registerContactmomentForm.informatieBurger"
          :placeholder="!isInformatieBurgerRequired ? `Optioneel` : undefined"
          :required="isInformatieBurgerRequired"
        />
      </utrecht-form-field>
    </utrecht-fieldset>

    <utrecht-button-group>
      <utrecht-button
        v-if="registerContactmomentForm.afsluiten === AFSLUITEN.ja"
        type="submit"
        appearance="primary-action-button"
        :disabled="isLoading"
      >
        <span v-if="isLoading">Bezig met opslaan...</span>
        <span v-else>Contactmoment opslaan</span>
      </utrecht-button>

      <utrecht-button
        v-else
        type="button"
        appearance="primary-action-button"
        :disabled="isLoading"
        @click="saveContactmoment()"
      >
        <span v-if="isLoading">Bezig met opslaan...</span>
        <span v-else>Contactmoment opslaan</span>
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
import SimpleSpinner from "@/components/SimpleSpinner.vue";
import { useBackNavigation } from "@/composables/use-back-navigation";

const router = useRouter();
const emit = defineEmits<{ success: [] }>();
const { taak } = defineProps<{ taak: Internetaken }>();

const RESULTS = {
  contactGelukt: "Contact opnemen gelukt",
  geenGehoor: "Contact opnemen niet gelukt"
} as const;

const AFSLUITEN = {
  ja: "Ja",
  nee: "Nee"
} as const;

const kanalen = [
  { label: "Selecteer een kanaal", value: "" },
  ...["Balie", "Telefoon"].map((value) => ({ label: value, value }))
];

const isLoading = ref(false);
const bevestigingsModalRef = ref<InstanceType<typeof BevestigingsModal>>();
const formRef = ref<HTMLFormElement>();

const registerContactmomentForm = ref({
  resultaat: RESULTS.contactGelukt as (typeof RESULTS)[keyof typeof RESULTS],
  afsluiten: undefined as (typeof AFSLUITEN)[keyof typeof AFSLUITEN] | undefined,
  kanaal: "",
  informatieBurger: ""
});

const isInformatieBurgerRequired = computed(
  () =>
    !(
      registerContactmomentForm.value.resultaat === RESULTS.geenGehoor &&
      registerContactmomentForm.value.afsluiten === AFSLUITEN.nee
    )
);

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
    const backNavifation = useBackNavigation();
    router.push(backNavifation.backButtonInfo.value.route);
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
    interneNotitie: "" // TODO: This was always null before refactor?
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
    kanaal: registerContactmomentForm.value.kanaal,
    onderwerp: taak.aanleidinggevendKlantcontact?.onderwerp || "Opvolging contactverzoek",
    inhoud: registerContactmomentForm.value.informatieBurger,
    indicatieContactGelukt: registerContactmomentForm.value.resultaat === RESULTS.contactGelukt,
    taal: "nld", // ISO 639-2/B formaat
    vertrouwelijk: false,
    plaatsgevondenOp: new Date().toISOString()
  };
}

function resetForm() {
  registerContactmomentForm.value = {
    resultaat: RESULTS.contactGelukt,
    afsluiten: undefined,
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

.utrecht-button-group {
  margin-top: 1rem;
}

.small {
  font-size: var(--denhaag-process-steps-step-meta-font-size);
  color: var(--denhaag-process-steps-step-meta-color);
}

textarea,
input,
select {
  max-width: 100%;
}

.utrecht-button-group {
  margin-block-end: calc(
    var(--utrecht-space-around, 0) * var(--utrecht-data-list-margin-block-end, 0)
  );
}
</style>
