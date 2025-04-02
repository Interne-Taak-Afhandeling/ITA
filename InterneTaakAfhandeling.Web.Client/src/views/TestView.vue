<template>
    <utrecht-heading :level="1">Test Items</utrecht-heading>
  
    <div v-if="loading">
      <simple-spinner />
    </div>
    
    <utrecht-alert v-if="error">{{ error }}</utrecht-alert>
  
    <section v-if="!loading && !error">
      <utrecht-heading :level="2" id="test-items-heading">Database Test Items</utrecht-heading>
  
      <utrecht-table aria-labelledby="test-items-heading">
        <utrecht-table-header>
          <utrecht-table-row>
            <utrecht-table-header-cell scope="col">ID</utrecht-table-header-cell>
            <utrecht-table-header-cell scope="col">Name</utrecht-table-header-cell>
            <utrecht-table-header-cell scope="col">Description</utrecht-table-header-cell>
          </utrecht-table-row>
        </utrecht-table-header>
  
        <utrecht-table-body>
          <utrecht-table-row v-for="item in items" :key="item.id">
            <utrecht-table-cell>{{ item.id }}</utrecht-table-cell>
            <utrecht-table-cell>{{ item.name }}</utrecht-table-cell>
            <utrecht-table-cell>{{ item.description }}</utrecht-table-cell>
          </utrecht-table-row>
        </utrecht-table-body>
      </utrecht-table>
  
      <div class="form-section">
        <utrecht-heading :level="3">Add New Item</utrecht-heading>
        
        <form @submit.prevent="addNewItem">
          <utrecht-form-field>
            <utrecht-form-label for="item-name">Name</utrecht-form-label>
            <utrecht-textbox id="item-name" v-model="newItem.name" required />
          </utrecht-form-field>
  
          <utrecht-form-field>
            <utrecht-form-label for="item-description">Description</utrecht-form-label>
            <utrecht-textbox id="item-description" v-model="newItem.description" required />
          </utrecht-form-field>
  
          <div class="button-container">
            <utrecht-button type="submit">Add Item</utrecht-button>
          </div>
        </form>
      </div>
    </section>
  </template>
  
  <script setup lang="ts">
  import { ref, onMounted } from 'vue';
  import SimpleSpinner from '@/components/SimpleSpinner.vue';
  import UtrechtAlert from '@/components/UtrechtAlert.vue';
  
  interface TestItem {
    id: number;
    name: string;
    description: string;
  }
  
  const items = ref<TestItem[]>([]);
  const loading = ref(true);
  const error = ref<string | null>(null);
  
  const newItem = ref<Omit<TestItem, 'id'>>({
    name: '',
    description: ''
  });
  
  const fetchItems = async () => {
    loading.value = true;
    error.value = null;
    
    try {
      const response = await fetch('/api/test');
      
      if (!response.ok) {
        throw new Error(`Error: ${response.status} ${response.statusText}`);
      }
      
      items.value = await response.json();
    } catch (err) {
      console.error('Failed to fetch test items:', err);
      error.value = `Failed to load test items: ${err instanceof Error ? err.message : 'Unknown error'}`;
    } finally {
      loading.value = false;
    }
  };
  
  const addNewItem = async () => {
    if (!newItem.value.name || !newItem.value.description) {
      error.value = 'Please fill in all fields';
      return;
    }
    
    try {
      const response = await fetch('/api/test', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify(newItem.value)
      });
      
      if (!response.ok) {
        throw new Error(`Error: ${response.status} ${response.statusText}`);
      }
      
      // Reset form and refresh items
      newItem.value = { name: '', description: '' };
      await fetchItems();
    } catch (err) {
      console.error('Failed to add new item:', err);
      error.value = `Failed to add new item: ${err instanceof Error ? err.message : 'Unknown error'}`;
    }
  };
  
  onMounted(() => {
    fetchItems();
  });
  </script>
  
  <style scoped>
  .form-section {
    margin-top: 2rem;
    max-width: 40rem;
  }
  
  .button-container {
    margin-top: 1rem;
  }
  </style>