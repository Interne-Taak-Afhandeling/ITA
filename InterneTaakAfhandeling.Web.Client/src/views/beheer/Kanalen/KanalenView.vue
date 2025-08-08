<template>
  <utrecht-heading :level="2">Kanalen</utrecht-heading>

  <utrecht-unordered-list v-if="kanalen.data.length" class="kanalen-list">
    <utrecht-unordered-list-item v-for="{ id, naam } in kanalen.data" :key="id" class="kanaal-item">
      <div class="kanaal-content">
        <router-link :to="{ name: 'kanaal', params: { id } }" class="kanaal-link">{{
          naam
        }}</router-link>
        <button
          @click="confirmDelete(id, naam)"
          class="delete-button"
          :aria-label="`Verwijder kanaal ${naam}`"
          title="Verwijderen"
        >
          <!-- TODO integrate UtrechtIcon at the final stage of this story -->
          <span>x</span>
        </button>
      </div>
    </utrecht-unordered-list-item>
  </utrecht-unordered-list>
  <router-link
    :to="{ name: 'kanaal' }"
    title="Kanaal toevoegen"
    class="utrecht-button utrecht-button--primary-action add-button"
  >
    <span class="plus-icon" aria-hidden="true">+</span>
  </router-link>
</template>

<script lang="ts" setup>
import { kanalenService, type Kanaal } from "@/services/kanalenService";
import { ref, onMounted } from "vue";

const kanalen = ref<{
  data: Kanaal[];
}>({
  data: []
});

const fetchKanalen = async () => {
  const data = await kanalenService.getKanalen();
  kanalen.value.data = data;
};

const confirmDelete = (id: string, naam: string) => {
  if (confirm(`Weet je zeker dat je het kanaal "${naam}" wilt verwijderen?`)) {
    deleteKanaal(id);
  }
};

const deleteKanaal = async (id: string) => {
  try {
    await kanalenService.deleteKanaal(id);
    fetchKanalen();
  } catch (err) {
    console.error("Error deleting kanaal:", err);
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

.add-button {
  margin: 2rem;
  width: 3rem;
  height: 3rem;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  text-decoration: none;
}

.plus-icon {
  font-size: 1rem;
  font-weight: bold;
  color: var(--utrecht-color-white);
}
</style>
