<template>
  <SimpleSpinner v-if="isLoading" />
   <utrecht-paragraph v-else-if="!isKanalenExist">
    Er zijn nog geen kanalen ingeregeld. Zonder deze kanalen kun je geen contactverzoeken
    afhandelen. Zorg dat er, onder Beheer, minimaal één kanaal is aangemaakt. Neem eventueel contact
    op met Functioneel Beheer.
   </utrecht-paragraph>
    
    <form v-else @submit.prevent="submit">
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

      <interne-toelichting-field
        v-model="registerContactmomentForm.interneNotitie"
        placeholder="Optioneel"
      />
    </utrecht-fieldset>

    <utrecht-button-group>
      <utrecht-button type="submit" appearance="primary-action-button"
        >Contactmoment opslaan</utrecht-button
      >
    </utrecht-button-group>
  </form>

  <bevestigings-modal
    ref="bevestigingsModalRef"
    title="Contactverzoek afronden"
    message="Weet je zeker dat je het contactverzoek wilt opslaan en afronden?"
    confirm-text="Opslaan & afronden"
    cancel-text="Annuleren"
    @confirm="finishContactmoment"
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
import SimpleSpinner from "@/components/SimpleSpinner.vue";
import InterneToelichtingField from "./InterneToelichtingField.vue";

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

const createForm = () => ({
  resultaat: RESULTS.contactGelukt as (typeof RESULTS)[keyof typeof RESULTS],
  afsluiten: undefined as (typeof AFSLUITEN)[keyof typeof AFSLUITEN] | undefined,
  kanaal: "",
  informatieBurger: "",
  interneNotitie: ""
});

const registerContactmomentForm = ref(createForm());

const isKanalenExist = computed(() => !isLoading.value && kanalen.value.length > 1);
const isContactmoment = computed(() => form.value.handeling === HANDLINGS.contactmoment);
const isInformatieBurgerRequired = computed(
  () =>
    !(
      registerContactmomentForm.value.resultaat === RESULTS.geenGehoor &&
      registerContactmomentForm.value.afsluiten === AFSLUITEN.nee
    )
);
const submit = () =>
  registerContactmomentForm.value.afsluiten === AFSLUITEN.ja
    ? bevestigingsModalRef.value?.show()
    : saveContactmoment();

async function saveContactmoment() {
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
  isLoading.value = true;
  try {
    await klantcontactService.createRelatedKlantcontactAndCloseInterneTaak(
      getKlantcontactPayload()
    );
    toast.add({ text: "Contactmoment succesvol opgeslagen en afgerond", type: "ok" });
    router.back();
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
    interneTaakId: taak.uuid
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
  registerContactmomentForm.value = createForm();
}

function handleError(err: unknown) {
  const message = err instanceof Error && err.message ? err.message : "Er is een fout opgetreden.";
  toast.add({ text: message, type: "error" });
}
</script>
