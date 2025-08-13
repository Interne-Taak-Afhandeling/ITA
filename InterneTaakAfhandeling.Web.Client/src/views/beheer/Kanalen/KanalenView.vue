<template>
  <utrecht-heading :level="2">Kanalen</utrecht-heading>

  <SimpleSpinner v-if="isLoading" />

  <utrecht-unordered-list v-else-if="kanalen.data.length" class="kanalen-list">
    <utrecht-unordered-list-item v-for="{ id, naam } in kanalen.data" :key="id" class="kanaal-item">
      <div class="kanaal-content">
        <router-link :to="{ name: 'kanaal', params: { id } }" class="kanaal-link">{{
          naam
        }}</router-link>
        <button
          @click="confirmDelete(id, naam)"
          class="utrecht-button utrecht-button--primary-action round-button"
          :aria-label="`Verwijder kanaal ${naam}`"
          title="Verwijderen"
          :disabled="isLoading"
        >
          <UtrechtIcon icon="trash-can" />
        </button>
      </div>
    </utrecht-unordered-list-item>
  </utrecht-unordered-list>

  <div v-else-if="!isLoading" class="empty-state">
    <p>Geen kanalen gevonden.</p>
  </div>

  <router-link
    :to="{ name: 'kanaal' }"
    title="Kanaal toevoegen"
    class="utrecht-button utrecht-button--primary-action round-button"
  >
    <utrecht-icon icon="add-plus" aria-hidden="true" />
  </router-link> 
   
</template>

<script lang="ts" setup>
import { kanalenService, type Kanaal } from "@/services/kanalenService";
import { ref, onMounted } from "vue";
import UtrechtIcon from "@/components/UtrechtIcon.vue";
import SimpleSpinner from "@/components/SimpleSpinner.vue";
import { toast } from "@/components/toast/toast";

const kanalen = ref<{
  data: Kanaal[];
}>({
  data: []
});

const isLoading = ref(false);

const fetchKanalen = async () => {
  try {
    isLoading.value = true;
    const data = await kanalenService.getKanalen();
    kanalen.value.data = data;
  } catch (error: unknown) {
    console.error("Error fetching kanalen:", error);
    toast.add({ text: error instanceof Error ? error?.message : "", type: "error" });
  } finally {
    isLoading.value = false;
  }
};

const confirmDelete = (id: string, naam: string) => {
  if (confirm(`Weet je zeker dat je het kanaal "${naam}" wilt verwijderen?`)) {
    deleteKanaal(id);
  }
};

const deleteKanaal = async (id: string) => {
  try {
    isLoading.value = true;
    await kanalenService.deleteKanaal(id);
    await fetchKanalen();
    toast.add({ text: "Kanaal succesvol verwijderd.", type: "ok" });
  } catch (error: unknown) {
    console.error("Error deleting kanaal:", error);
    toast.add({ text: error instanceof Error ? error?.message : "", type: "error" });
  } finally {
    isLoading.value = false;
  }
};

onMounted(() => {
  fetchKanalen();
});
</script>

<style lang="scss" scoped>
.kanalen-list {
  margin: 1rem;
  max-width: 60%;
}

.kanaal-item {
  display: block;
  padding: 1rem;
  border-bottom: 0.1rem solid var(--utrecht-color-blue-40);
  color: var(--utrecht-color-blue-40);
  font-size: 1rem;
}

.kanaal-content {
  display: flex;
  justify-content: space-between;
  align-items: center;
  width: 100%;
}

 
.round-button {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  padding: 0.375rem;
  border-radius: 50%;
}

.round-button svg {
  width: 1.25rem;
  height: 1.25rem;
}



.empty-state {
  margin: 2rem;
  text-align: center;
  color: var(--utrecht-color-grey-60);
}
</style>
