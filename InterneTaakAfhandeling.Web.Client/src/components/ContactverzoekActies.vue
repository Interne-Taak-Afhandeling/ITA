<template>
  <SimpleSpinner v-if="isLoading" />

  <utrecht-alert v-else-if="error" type="error">
    {{ error }}
  </utrecht-alert>

  <form v-else ref="formRef" @submit.prevent="showConfirmation">
    <!--
      Radio button group pattern which mimics tabs.
      Not using semantic tabs here as these buttons control form configuration
      rather than navigating between content panels.
    -->
    <ita-radio-tabs legend="Kies een handeling" :options="HANDLINGS" v-model="handeling" />

    <utrecht-fieldset v-if="isRegisterContactmoment">
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

    <interne-toelichting-section v-if="isInterneToelichting">
      <utrecht-form-field>
        <utrecht-form-label for="interne-toelichting-text">
          Interne toelichting
        </utrecht-form-label>
        <utrecht-textarea
          id="interne-toelichting-text"
          v-model="interneToelichtingForm.interneNotitie"
          :placeholder="undefined"
          :required="true"
        />
        <div class="small">
          Deze toelichting is alleen voor medewerkers te zien en is verborgen voor de burger/het
          bedrijf.
        </div>
      </utrecht-form-field>
    </interne-toelichting-section>

    <utrecht-fieldset v-if="isForwardContactmoment">
      <utrecht-fieldset>
        <utrecht-legend>Contactmoment doorzetten naar</utrecht-legend>
        <utrecht-form-field v-for="(label, key) in FORWARD_OPTIONS" :key="key" type="radio">
          <utrecht-radiobutton
            name="forwardTo"
            :id="key"
            :value="label"
            v-model="forwardContactmomentForm.forwardTo"
            required
          />
          <utrecht-form-label :for="key" type="radio">{{ label }}</utrecht-form-label>
        </utrecht-form-field>
      </utrecht-fieldset>

      <utrecht-form-field v-if="forwardContactmomentForm.forwardTo == FORWARD_OPTIONS.afdeling">
        <utrecht-form-label for="forwardTo">Afdeling</utrecht-form-label>
        <utrecht-select
          required
          id="forwardTo"
          v-model="forwardContactmomentForm.afdeling"
          :options="afdelingen"
        />
      </utrecht-form-field>

      <utrecht-form-field v-if="forwardContactmomentForm.forwardTo == FORWARD_OPTIONS.groep">
        <utrecht-form-label for="forwardTo">Groep</utrecht-form-label>
        <utrecht-select
          required
          id="forwardTo"
          v-model="forwardContactmomentForm.groep"
          :options="groepen"
        />
      </utrecht-form-field>

      <utrecht-form-field>
        <utrecht-form-label for="medewerker">Medewerker</utrecht-form-label>
        <utrecht-select
          :required="forwardContactmomentForm.forwardTo == FORWARD_OPTIONS.medewerker"
          id="medewerker"
          v-model="forwardContactmomentForm.medewerker"
          :options="medewerkers"
        />
      </utrecht-form-field>

      <interne-toelichting-section>
        <utrecht-form-field>
          <utrecht-form-label for="interne-toelichting-text">
            Interne toelichting
          </utrecht-form-label>
          <utrecht-textarea
            id="interne-toelichting-text"
            v-model="forwardContactmomentForm.interneNotitie"
            :placeholder="'Optioneel'"
          />
          <div class="small">
            Deze toelichting is alleen voor medewerkers te zien en is verborgen voor de burger/het
            bedrijf.
          </div>
        </utrecht-form-field>
      </interne-toelichting-section>
    </utrecht-fieldset>

    <utrecht-button-group>
      <utrecht-button
        v-if="isRegisterContactmoment && registerContactmomentForm.afsluiten === AFSLUITEN.ja"
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
        @click="isRegisterContactmoment ? saveContactmoment() : saveNote()"
      >
        <span v-if="isLoading">Bezig met opslaan...</span>
        <span v-else-if="isRegisterContactmoment">Contactmoment opslaan</span>
        <span v-else>Opslaan</span>
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
import { computed, onMounted, ref } from "vue";
import { toast } from "./toast/toast";
import BevestigingsModal from "./BevestigingsModal.vue";
import { useRouter } from "vue-router";
import { internetakenService } from "@/services/internetakenService";
import InterneToelichtingSection from "./InterneToelichtingSection.vue";
import { useBackNavigation } from "@/composables/use-back-navigation";
import ItaRadioTabs from "./ItaRadioTabs.vue";
import SimpleSpinner from "@/components/SimpleSpinner.vue";
import { get } from "@/utils/fetchWrapper";
import UtrechtAlert from "@/components/UtrechtAlert.vue";

const { taak } = defineProps<{ taak: Internetaken }>();
const emit = defineEmits<{ success: [] }>();
const router = useRouter();

const HANDLINGS = {
  contactmoment: "Contactmoment registreren",
  contactmomentDoorsturen: "Doorsturen",
  interneToelichting: "Alleen toelichting"
} as const;

const RESULTS = {
  contactGelukt: "Contact opnemen gelukt",
  geenGehoor: "Contact opnemen niet gelukt"
} as const;

const FORWARD_OPTIONS = {
  afdeling: "Afdeling",
  groep: "Groep",
  medewerker: "Medewerker"
} as const;

const AFSLUITEN = {
  ja: "Ja",
  nee: "Nee"
} as const;

const kanalen = [
  { label: "Selecteer een kanaal", value: "" },
  ...["Balie", "Telefoon"].map((value) => ({ label: value, value }))
];

const medewerkers = [
  { label: "Selecteer een medewerker", value: "" },
  ...["Example 1", "Example 2"].map((value) => ({ label: value, value }))
];

const afdelingen = ref<{ label: string; value: string }[]>([]);

const groepen = ref<{ label: string; value: string }[]>([]);

const isLoading = ref(true);
const error = ref<string | null>(null);
const bevestigingsModalRef = ref<InstanceType<typeof BevestigingsModal>>();
const formRef = ref<HTMLFormElement>();

const handeling = ref(HANDLINGS.contactmoment as (typeof HANDLINGS)[keyof typeof HANDLINGS]);

const registerContactmomentForm = ref({
  resultaat: RESULTS.contactGelukt as (typeof RESULTS)[keyof typeof RESULTS],
  afsluiten: undefined as (typeof AFSLUITEN)[keyof typeof AFSLUITEN] | undefined,
  kanaal: "",
  informatieBurger: ""
});

const interneToelichtingForm = ref({
  interneNotitie: ""
});

const forwardContactmomentForm = ref({
  forwardTo: FORWARD_OPTIONS.afdeling as (typeof FORWARD_OPTIONS)[keyof typeof FORWARD_OPTIONS],
  medewerker: "",
  groep: "",
  afdeling: "",
  interneNotitie: ""
});

const isRegisterContactmoment = computed(() => handeling.value === HANDLINGS.contactmoment);
const isInterneToelichting = computed(() => handeling.value === HANDLINGS.interneToelichting);
const isForwardContactmoment = computed(
  () => handeling.value === HANDLINGS.contactmomentDoorsturen
);

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

async function saveNote() {
  if (!isValidForm()) return;
  isLoading.value = true;
  try {
    await internetakenService.addNoteToInternetaak(
      taak.uuid,
      interneToelichtingForm.value.interneNotitie.trim()
    );
    toast.add({ text: "Notitie succesvol toegevoegd", type: "ok" });
    resetForm();
    emit("success");
  } catch (err: unknown) {
    handleSubmitError(err);
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
    handleSubmitError(err);
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
    handleSubmitError(err);
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
    interneNotitie: interneToelichtingForm.value.interneNotitie
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
  interneToelichtingForm.value = {
    interneNotitie: ""
  };
  forwardContactmomentForm.value = {
    forwardTo: FORWARD_OPTIONS.afdeling as (typeof FORWARD_OPTIONS)[keyof typeof FORWARD_OPTIONS],
    medewerker: "",
    groep: "",
    afdeling: "",
    interneNotitie: ""
  };
}

function handleSubmitError(err: unknown) {
  const message = err instanceof Error && err.message ? err.message : "Er is een fout opgetreden.";
  toast.add({ text: message, type: "error" });
}

function handleLoadingError(err: unknown) {
  const message =
    err instanceof Error && err.message
      ? err.message
      : "Er is een fout opgetreden. Herlaad de pagina. Als het probleem blijft bestaan, neem contact op met functioneel beheer";
  error.value = message;
}

const fetchAfdelingen = async () => {
  afdelingen.value = [];

  afdelingen.value = [
    { label: "Selecteer een afdeling", value: "" },
    ...(await get<string[]>("/api/afdelingen")).map((value) => ({ label: value, value }))
  ];
};

const fetchGroepen = async () => {
  afdelingen.value = [];

  groepen.value = [
    { label: "Selecteer een groep", value: "" },
    ...(await get<string[]>("/api/groepen")).map((value) => ({ label: value, value }))
  ];
};

onMounted(async () => {
  try {
    isLoading.value = true;
    error.value = null;
    await Promise.all([fetchAfdelingen(), fetchGroepen()]);
  } catch (err: unknown) {
    handleLoadingError(err);
  } finally {
    isLoading.value = false;
  }
});
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

.ita-radio-tabs {
  margin-block-start: 0;
  margin-inline-start: calc(-1 * var(--current-padding-inline-start));
  margin-inline-end: calc(-1 * var(--current-padding-inline-end));
}

.utrecht-button-group {
  margin-block-end: calc(
    var(--utrecht-space-around, 0) * var(--utrecht-data-list-margin-block-end, 0)
  );
}
</style>
