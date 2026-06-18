<template>
  <utrecht-data-list>
    <div class="ita-data-list__group">
      <utrecht-data-list-item>
        <utrecht-data-list-key>Klantnaam</utrecht-data-list-key>
        <utrecht-data-list-value :value="klantNaam ?? undefined">
          {{ klantNaam }}
        </utrecht-data-list-value>
      </utrecht-data-list-item>

      <utrecht-data-list-item v-if="organisatienaam">
        <utrecht-data-list-key>Organisatie</utrecht-data-list-key>
        <utrecht-data-list-value :value="organisatienaam ?? undefined">
          {{ organisatienaam }}
        </utrecht-data-list-value>
      </utrecht-data-list-item>

      <utrecht-data-list-item>
        <utrecht-data-list-key>E-mailadres</utrecht-data-list-key>
        <utrecht-data-list-value :value="email ?? undefined" v-title-on-overflow>
          {{ email }}
        </utrecht-data-list-value>
      </utrecht-data-list-item>

      <utrecht-data-list-item>
        <utrecht-data-list-key>Telefoonnummer</utrecht-data-list-key>
        <utrecht-data-list-value :value="telefoonnummer1?.adres">
          {{ telefoonnummer1?.adres }}
        </utrecht-data-list-value>
      </utrecht-data-list-item>

      <utrecht-data-list-item v-if="telefoonnummer2?.adres">
        <utrecht-data-list-key>{{ telefoonnummer2.omschrijving }}</utrecht-data-list-key>
        <utrecht-data-list-value :value="telefoonnummer2.adres">
          {{ telefoonnummer2.adres }}
        </utrecht-data-list-value>
      </utrecht-data-list-item>
    </div>

    <div class="ita-data-list__group">
      <utrecht-data-list-item>
        <utrecht-data-list-key>Datum aangemaakt</utrecht-data-list-key>
        <utrecht-data-list-value
          value="empty values are handled by date-time-or-nvt so we hard code a value here"
        >
          <date-time-or-nvt :date="plaatsgevondenOp ?? undefined" />
        </utrecht-data-list-value>
      </utrecht-data-list-item>

      <utrecht-data-list-item>
        <utrecht-data-list-key>Aangemaakt door</utrecht-data-list-key>
        <utrecht-data-list-value :value="aangemaaktDoor ?? undefined">
          {{ aangemaaktDoor }}
        </utrecht-data-list-value>
      </utrecht-data-list-item>

      <utrecht-data-list-item v-if="behandelaarNaam">
        <utrecht-data-list-key>Behandelaar</utrecht-data-list-key>
        <utrecht-data-list-value :value="behandelaarNaam">
          {{ behandelaarNaam }}
        </utrecht-data-list-value>
      </utrecht-data-list-item>

      <utrecht-data-list-item v-if="organisatorischeEenheidNaam">
        <utrecht-data-list-key>{{ organisatorischeEenheidType }}</utrecht-data-list-key>
        <utrecht-data-list-value :value="organisatorischeEenheidNaam">
          {{ organisatorischeEenheidNaam }}
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
        <utrecht-data-list-value :value="kanaal ?? undefined">{{ kanaal }}</utrecht-data-list-value>
      </utrecht-data-list-item>
    </div>
  </utrecht-data-list>
</template>

<script setup lang="ts">
import type { TelefoonnummerItem } from "@/types/internetaken";
import DateTimeOrNvt from "./DateTimeOrNvt.vue";
import { vTitleOnOverflow } from "@/directives/v-title-on-overflow";

defineProps<{
  status: string;
  klantNaam?: string | null;
  organisatienaam?: string | null;
  email?: string | null;
  telefoonnummer1?: TelefoonnummerItem | null;
  telefoonnummer2?: TelefoonnummerItem | null;
  plaatsgevondenOp?: string | null;
  kanaal?: string | null;
  aangemaaktDoor?: string | null;
  behandelaarNaam?: string | null;
  organisatorischeEenheidNaam?: string | null;
  organisatorischeEenheidType?: string | null;
}>();
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
