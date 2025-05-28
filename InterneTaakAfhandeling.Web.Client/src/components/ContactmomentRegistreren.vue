<template>
  <form @submit.prevent="submit">
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
    <utrecht-button type="submit" appearance="primary-action-button" :disabled="isLoading">
      <span v-if="isLoading">Bezig met opslaan...</span>
      <span v-else>Opslaan</span>
    </utrecht-button>
  </form>
</template>

<script setup lang="ts">
import { klantcontactService } from "@/services/klantcontactService";
import type { CreateKlantcontactRequest, Internetaken } from "@/types/internetaken";
import { ref } from "vue";
import { toast } from "./toast/toast";

const { taak } = defineProps<{ taak: Internetaken }>();
const emit = defineEmits<{ success: [] }>();

const RESULTS = {
  contactGelukt: "Contact opnemen gelukt",
  geenGehoor: "Contact opnemen niet gelukt"
} as const;

const kanalen = [
  { label: "Selecteer een kanaal", value: "" },
  ...["Balie", "Telefoon"].map((value) => ({ label: value, value }))
];

const isLoading = ref(false);

const form = ref({
  resultaat: RESULTS.contactGelukt as (typeof RESULTS)[keyof typeof RESULTS],
  kanaal: "",
  informatieBurger: ""
});

async function submit() {
  isLoading.value = true;

  try {
    const createRequest: CreateKlantcontactRequest = {
      kanaal: form.value.kanaal,
      onderwerp: taak.aanleidinggevendKlantcontact?.onderwerp || "Opvolging contactverzoek",
      inhoud: form.value.informatieBurger,
      indicatieContactGelukt: form.value.resultaat === RESULTS.contactGelukt,
      taal: "nld", // ISO 639-2/B formaat
      vertrouwelijk: false,
      plaatsgevondenOp: new Date().toISOString()
    };

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

    const aanleidinggevendKlantcontactUuid = taak.aanleidinggevendKlantcontact?.uuid;

    await klantcontactService.createRelatedKlantcontact(
      createRequest,
      aanleidinggevendKlantcontactUuid,
      partijUuid
    );

    form.value = {
      resultaat: RESULTS.contactGelukt,
      kanaal: "",
      informatieBurger: ""
    };

    toast.add({ text: "Contactmoment succesvol bijgewerkt", type: "ok" });
    emit("success");
  } catch (err: unknown) {
    const message =
      err instanceof Error && err.message
        ? err.message
        : "Er is een fout opgetreden bij het aanmaken van het contactmoment";
    toast.add({ text: message, type: "error" });
  } finally {
    isLoading.value = false;
  }
}
</script>

<style lang="scss" scoped>
.utrecht-form-label {
  display: block;
}
</style>
