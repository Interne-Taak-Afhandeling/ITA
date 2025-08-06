<template>
  <utrecht-heading :level="2">Kanaal</utrecht-heading>
  <utrecht-fieldset>
    <form @submit.prevent="submit">
      <utrecht-form-field>
        <utrecht-form-label for="naam">Naam</utrecht-form-label>
        <utrecht-textbox id="naam" v-model="item.naam" required />
      </utrecht-form-field>

      <utrecht-button-group>
        <router-link
          :to="{ name: 'kanalen' }"
          class="utrecht-button utrecht-button--secondary-action"
        >
          Annuleren
        </router-link>
        <utrecht-button appearance="primary-action-button" type="submit"> Opslaan </utrecht-button>
      </utrecht-button-group>
    </form>
  </utrecht-fieldset>
</template>

<script setup lang="ts">
import { ref } from "vue";
import { useRouter } from "vue-router";

import { kanalenService, type Kanaal } from "@/services/kanalenService";

const router = useRouter();

const item = ref<Kanaal>({
  id: 0,
  naam: ""
});

const handleSuccess = (result: Kanaal) => {
  return router.push({ name: "kanalen" });
};

const submit = async () => {
  try {
    await create();
  } catch {
    console.error("Failed to create kanaal");
  }
};

async function create() {
  const result = await kanalenService.createKanaal(item.value.naam);
  return handleSuccess(result);
}
</script>

<style lang="scss" scoped>
form {
  display: flex;
  flex-direction: column;
  gap: 1rem;
  margin: 1rem;
}

.utrecht-form-field {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}
</style>
