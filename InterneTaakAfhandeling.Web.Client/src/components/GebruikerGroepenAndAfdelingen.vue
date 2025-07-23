<template>
  <div class="filter-container">
    <label for="filter-dropdown">Filter :</label>
    <div>
      <select 
        id="filter-dropdown" 
        v-model="selectedValue" 
      >
      <optgroup v-if="props.data?.afdelingen?.length" label="Afdelingen">
        <option v-for="afdeling in props.data.afdelingen" :key="afdeling" :value="afdeling">
          {{ afdeling }}
        </option>
      </optgroup>
      <optgroup v-if="props.data?.groepen?.length" label="Groepen">
        <option v-for="groep in props.data.groepen" :key="groep" :value="groep">
          {{ groep }}
        </option>
      </optgroup>
      </select>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from "vue"; 

interface MedewerkerData {
  groepen?: string[];
  afdelingen?: string[];
}

const props = defineProps<{
  modelValue?: string;
  data: MedewerkerData;
}>();

const emit = defineEmits<{
  'update:modelValue': [value: string];
}>();

const selectedValue = computed({
  get: () => props.modelValue || "",
  set: (value) => {
    emit('update:modelValue', value);
  }
}); 
</script>

<style lang="scss" scoped>
.filter-container {
  margin-bottom: 0.75rem;
  
  label {
    display: block;
    font-weight: bold;
    margin-bottom: 0.25rem;
  }
  
  select {
    padding: 0.5rem;
    border-radius: 4px;
    border: 1px solid #ccc;
    min-width: 200px;
  }
}
</style>
