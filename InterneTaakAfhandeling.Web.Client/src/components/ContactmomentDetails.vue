<template>
  <utrecht-data-list>
    <div class="ita-data-list__group">
      <utrecht-data-list-item>
        <utrecht-data-list-key>Klantnaam</utrecht-data-list-key>
        <utrecht-data-list-value :value="klantNaam">
          {{ klantNaam }}
        </utrecht-data-list-value>
      </utrecht-data-list-item>

      <utrecht-data-list-item v-if="organisatienaam">
        <utrecht-data-list-key>Organisatie</utrecht-data-list-key>
        <utrecht-data-list-value :value="organisatienaam">
          {{ organisatienaam }}
        </utrecht-data-list-value>
      </utrecht-data-list-item>

      <utrecht-data-list-item>
        <utrecht-data-list-key>E-mailadres</utrecht-data-list-key>
        <utrecht-data-list-value :value="email" v-title-on-overflow>
          {{ email }}
        </utrecht-data-list-value>
      </utrecht-data-list-item>

      <utrecht-data-list-item>
        <utrecht-data-list-key>Telefoonnummer</utrecht-data-list-key>
        <utrecht-data-list-value :value="phoneNumber1?.adres">
          {{ phoneNumber1?.adres }}
        </utrecht-data-list-value>
      </utrecht-data-list-item>

      <utrecht-data-list-item v-if="phoneNumber2?.adres">
        <utrecht-data-list-key>{{ phoneNumber2.omschrijving }}</utrecht-data-list-key>
        <utrecht-data-list-value :value="phoneNumber2.adres">
          {{ phoneNumber2.adres }}
        </utrecht-data-list-value>
      </utrecht-data-list-item>
    </div>

    <div class="ita-data-list__group">
      <utrecht-data-list-item>
        <utrecht-data-list-key>Datum aangemaakt</utrecht-data-list-key>
        <utrecht-data-list-value
          value="empty values are handled by date-time-or-nvt so we hard code a value here"
        >
          <date-time-or-nvt :date="contactmoment.plaatsgevondenOp" />
        </utrecht-data-list-value>
      </utrecht-data-list-item>

      <utrecht-data-list-item>
        <utrecht-data-list-key>Aangemaakt door</utrecht-data-list-key>
        <utrecht-data-list-value :value="aangemaaktDoor">
          {{ aangemaaktDoor }}
        </utrecht-data-list-value>
      </utrecht-data-list-item>

      <utrecht-data-list-item>
        <utrecht-data-list-key>Behandelaar</utrecht-data-list-key>
        <utrecht-data-list-value :value="behandelaar">
          {{ behandelaar }}
        </utrecht-data-list-value>
      </utrecht-data-list-item>

      <utrecht-data-list-item>
        <utrecht-data-list-key>Status</utrecht-data-list-key>
        <utrecht-data-list-value :value="status">
          {{ status }}
        </utrecht-data-list-value>
      </utrecht-data-list-item>

      <utrecht-data-list-item>
        <utrecht-data-list-key>Kanaal</utrecht-data-list-key>
        <utrecht-data-list-value :value="contactmoment.kanaal">{{
          contactmoment.kanaal
        }}</utrecht-data-list-value>
      </utrecht-data-list-item>
    </div>
  </utrecht-data-list>
</template>

<script setup lang="ts">
import type { Actor, Klantcontact } from "@/types/internetaken";
import { computed } from "vue";
import DateTimeOrNvt from "./DateTimeOrNvt.vue";
import { vTitleOnOverflow } from "@/directives/v-title-on-overflow";

const { contactmoment, actoren } = defineProps<{
  contactmoment: Klantcontact;
  actoren: Actor[];
  status: string;
}>();

const pascalCase = (s: string | undefined) =>
  !s ? s : `${s[0].toLocaleUpperCase()}${s.substring(1) || ""}`;
const phoneNumbers = computed(
  () =>
    contactmoment._expand?.hadBetrokkenen?.[0]?._expand.digitaleAdressen
      ?.filter(
        ({ soortDigitaalAdres }: { soortDigitaalAdres?: string }) =>
          soortDigitaalAdres === "telefoonnummer"
      )
      .filter((x) => x.adres)
      .map(({ adres, omschrijving }, i) => ({
        adres,
        omschrijving: pascalCase(omschrijving) || `Telefoonnummer ${i + 1}`
      })) || []
);
const phoneNumber1 = computed(() =>
  phoneNumbers.value.length > 0 ? phoneNumbers.value[0] : undefined
);
const phoneNumber2 = computed(() =>
  phoneNumbers.value.length > 1 ? phoneNumbers.value[1] : undefined
);
const email = computed(
  () =>
    contactmoment._expand?.hadBetrokkenen?.[0]?._expand.digitaleAdressen
      ?.filter(
        ({ soortDigitaalAdres }: { soortDigitaalAdres?: string }) => soortDigitaalAdres === "email"
      )
      .map(({ adres }: { adres?: string }) => adres || "")
      .find(Boolean) || ""
);
const behandelaar = computed(() => {
  const mdwActor = actoren.find((x) => x.actoridentificator?.codeObjecttype === "mdw");
  if (mdwActor?.naam) return mdwActor.naam;
  return actoren[0]?.naam || "";
});
const aangemaaktDoor = computed(
  () => contactmoment.hadBetrokkenActoren?.map((x) => x.naam).find(Boolean) || ""
);

const klantNaam = computed(() =>
  contactmoment._expand?.hadBetrokkenen
    ?.map((x) => x.volledigeNaam || x.organisatienaam)
    .find(Boolean)
);

const organisatienaam = computed(() =>
  contactmoment._expand?.hadBetrokkenen
    ?.map((x) => x.organisatienaam)
    .filter((x) => x !== klantNaam.value)
    .find(Boolean)
);
</script>

<style lang="scss" scoped>
.utrecht-data-list {
  display: grid;
  grid-template-columns: minmax(min-content, max-content) auto;
  column-gap: var(--utrecht-space-inline-md);
}

.utrecht-data-list__item {
  @container (max-width: 32rem) {
    grid-template-columns: 1fr;
  }

  grid-template-columns: subgrid;
  grid-column: 1 / -1;
}

.utrecht-data-list__item-value {
  --utrecht-data-list-rows-item-value-margin-block-start: 0;

  text-overflow: ellipsis;
  overflow: hidden;
  &[title]:not([title=""]) {
    user-select: all;
  }
}

.ita-data-list__group {
  display: contents;

  &:not(:first-of-type) {
    .utrecht-data-list__item:first-of-type {
      margin-block-start: var(--utrecht-data-list-rows-item-margin-block-start);
      padding-block-start: var(--utrecht-data-list-rows-item-margin-block-start);
      border-block-start: 1px solid var(--ita-detail-section-border-color);
    }
  }
}
</style>
