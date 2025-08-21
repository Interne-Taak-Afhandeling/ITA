<template>
  <utrecht-heading :level="2">Kanaal</utrecht-heading>

  <SimpleSpinner v-if="isLoading" />

  <utrecht-fieldset v-else>
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
import { ref, onMounted } from "vue";
import { useRouter, useRoute } from "vue-router";
import SimpleSpinner from "@/components/SimpleSpinner.vue";
import { toast } from "@/components/toast/toast";

import { kanalenService, type Kanaal } from "@/services/kanalenService";

const router = useRouter();
const route = useRoute();

const item = ref<Kanaal>({
  id: "",
  naam: ""
});

const isEditing = ref(false);
const isLoading = ref(false);

const handleSuccess = () => {
  return router.push({ name: "kanalen" });
};

const fetchKanaal = async (id: string) => {
  try {
    isLoading.value = true;
    const kanaal = await kanalenService.getKanaalById(id);
    item.value = kanaal;
  } catch (error: unknown) {
    console.error("Failed to fetch kanaal:", error);
    toast.add({ text: error instanceof Error ? error?.message : "", type: "error" });
  } finally {
    isLoading.value = false;
  }
};

const submit = async () => {
  try {
    isLoading.value = true;
    if (isEditing.value) {
      await edit();
    } else {
      await create();
    }
  } catch (error: unknown) {
    console.error("Failed to save kanaal:", error);
    toast.add({ text: error instanceof Error ? error?.message : "", type: "error" });
  } finally {
    isLoading.value = false;
  }
};

async function create() {
  await kanalenService.createKanaal(item.value.naam);
  toast.add({ text: "Kanaal succesvol aangemaakt.", type: "ok" });
  return handleSuccess();
}

async function edit() {
  await kanalenService.editKanaal(item.value.id, item.value.naam);
  toast.add({ text: "Kanaal succesvol bijgewerkt.", type: "ok" });
  return handleSuccess();
}

onMounted(() => {
  const id = route.params.id as string;
  if (id) {
    isEditing.value = true;
    fetchKanaal(id);
  }
});
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
