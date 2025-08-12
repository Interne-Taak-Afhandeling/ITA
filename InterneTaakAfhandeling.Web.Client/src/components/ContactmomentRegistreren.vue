<template>
  <SimpleSpinner v-if="isLoading" />

  <utrecht-paragraph v-else-if="!isKanalenExist">
    Er zijn nog geen kanalen ingeregeld. Zonder deze kanalen kun je geen contactverzoeken
    afhandelen. Zorg dat er, onder Beheer, minimaal één kanaal is aangemaakt. Neem eventueel contact
    op met Functioneel Beheer.
  </utrecht-paragraph>

  <form v-else ref="formRef" @submit.prevent="showConfirmation">
    <!--
      Radio button group pattern which mimics tabs.
      Not using semantic tabs here as these buttons control form configuration
      rather than navigating between content panels.
    -->
    <ita-radio-tabs legend="Kies een handeling" :options="HANDLINGS" v-model="form.handeling" />

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

      <utrecht-fieldset>
        <utrecht-legend>Wil je het contactmoment afsluiten?</utrecht-legend>
        <utrecht-form-field v-for="(label, key) in AFSLUITEN" :key="key" type="radio">
          <utrecht-radiobutton
            name="afsluiten"
            :id="key"
            :value="label"
            v-model="form.afsluiten"
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
        <utrecht-textarea
          id="informatie-burger"
          v-model="form.informatieBurger"
          :placeholder="!isInformatieBurgerRequired ? `Optioneel` : undefined"
          :required="isInformatieBurgerRequired"
        />
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
          :placeholder="isContactmoment ? `Optioneel` : undefined"
          :required="!isContactmoment"
        />
        <div class="small">
          Deze toelichting is alleen voor medewerkers te zien en is verborgen voor de burger/het
          bedrijf.
        </div>
      </utrecht-form-field>
    </interne-toelichting-section>

    <utrecht-button-group>
      <utrecht-button
        v-if="isContactmoment && form.afsluiten === AFSLUITEN.ja"
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
        @click="isContactmoment ? saveContactmoment() : saveNote()"
      >
        <span v-if="isLoading">Bezig met opslaan...</span>
        <span v-else-if="isContactmoment">Contactmoment opslaan</span>
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
import { kanalenService, type Kanaal } from "@/services/kanalenService";
import type { CreateKlantcontactRequest, Internetaken } from "@/types/internetaken";
import { computed, ref, onMounted } from "vue";
import { toast } from "./toast/toast";
import BevestigingsModal from "./BevestigingsModal.vue";
import { useRouter } from "vue-router";
import { internetakenService } from "@/services/internetakenService";
import InterneToelichtingSection from "./InterneToelichtingSection.vue";
import { useBackNavigation } from "@/composables/use-back-navigation";
import ItaRadioTabs from "./ItaRadioTabs.vue";
import SimpleSpinner from "@/components/SimpleSpinner.vue";

const { taak } = defineProps<{ taak: Internetaken }>();
const emit = defineEmits<{ success: [] }>();
const router = useRouter();

const HANDLINGS = {
  contactmoment: "Contactmoment registreren",
  interneToelichting: "Alleen toelichting"
} as const;

const RESULTS = {
  contactGelukt: "Contact opnemen gelukt",
  geenGehoor: "Contact opnemen niet gelukt"
} as const;

const AFSLUITEN = {
  ja: "Ja",
  nee: "Nee"
} as const;

const kanalen = ref([{ label: "Selecteer een kanaal", value: "" }]);

const fetchKanalen = async () => {
  try {
    const response = await kanalenService.getKanalen();
    kanalen.value = [
      ...kanalen.value,
      ...(response?.map((kanaal: Kanaal) => ({ label: kanaal.naam, value: kanaal.naam })) ?? [])
    ];
  } catch (error) {
    console.error("Failed to fetch kanalen:", error);
  }
};

onMounted(() => {
  fetchKanalen();
});

const isLoading = ref(false);
const bevestigingsModalRef = ref<InstanceType<typeof BevestigingsModal>>();
const formRef = ref<HTMLFormElement>();

const form = ref({
  handeling: HANDLINGS.contactmoment as (typeof HANDLINGS)[keyof typeof HANDLINGS],
  resultaat: RESULTS.contactGelukt as (typeof RESULTS)[keyof typeof RESULTS],
  afsluiten: undefined as (typeof AFSLUITEN)[keyof typeof AFSLUITEN] | undefined,
  kanaal: "",
  informatieBurger: "",
  interneNotitie: ""
});

const isKanalenExist = computed(() => !isLoading.value && kanalen.value.length > 1);
const isContactmoment = computed(() => form.value.handeling === HANDLINGS.contactmoment);
const isInformatieBurgerRequired = computed(
  () => !(form.value.resultaat === RESULTS.geenGehoor && form.value.afsluiten === AFSLUITEN.nee)
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
    afsluiten: undefined,
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
