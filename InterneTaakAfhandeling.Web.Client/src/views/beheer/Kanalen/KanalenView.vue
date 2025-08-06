<template>
  <utrecht-heading :level="2">Kanalen</utrecht-heading>

  <utrecht-unordered-list v-if="kanalen.data.length" class="kanalen-list">
    <utrecht-unordered-list-item v-for="{ id, naam } in kanalen.data" :key="id" class="kanaal-item">
      {{ naam }}
    </utrecht-unordered-list-item>
  </utrecht-unordered-list>
  <router-link
    :to="{ name: 'kanaal' }"
    title="toevoegen"
    class="utrecht-button utrecht-button--primary-action add-button"
  >
    <span class="plus-icon">+</span>
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
