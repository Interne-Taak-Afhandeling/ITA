<script setup lang="ts">
import InternetakenList from './components/InternetakenList.vue'
</script>

<template>
  <header>
    <div class="header-content">
      <img alt="ITA logo" class="logo" src="./assets/logo.svg" width="125" height="125" />
      <h1 class="app-title">Interne Taak Afhandeling</h1>
    </div>
  </header>

  <main>
    <InternetakenList />
  </main>

  <footer>
    <p>&copy; {{ new Date().getFullYear() }} InterneTaakAfhandeling</p>
  </footer>
</template>

<style>
:root {
  --primary-color: #2c3e50;
  --secondary-color: #42b983;
  --light-color: #f8f9fa;
  --dark-color: #343a40;
}

body {
  margin: 0;
  padding: 0;
  font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
  background-color: var(--light-color);
  color: var(--dark-color);
}

#app {
  max-width: 1280px;
  margin: 0 auto;
  padding: 2rem;
}

header {
  margin-bottom: 2rem;
}

.header-content {
  display: flex;
  align-items: center;
  justify-content: center;
  flex-direction: column;
  gap: 1rem;
}

@media (min-width: 768px) {
  .header-content {
    flex-direction: row;
  }
}

.logo {
  display: block;
}

.app-title {
  font-size: 2rem;
  margin: 0;
  color: var(--primary-color);
}

main {
  background-color: white;
  border-radius: 8px;
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
  padding: 2rem;
  margin-bottom: 2rem;
}

footer {
  text-align: center;
  padding: 1rem 0;
  font-size: 0.9rem;
  color: #6c757d;
}
</style>