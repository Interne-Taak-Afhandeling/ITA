<template>
  <nav>
    <router-link to="/">Home</router-link>
    <router-link to="/beheer/kanalen">Kanalen</router-link>
  </nav>

  <main>
    <router-view />
  </main>
</template>

<script lang="ts" setup></script>

<style lang="scss" scoped>
nav {
  display: flex;
  justify-content: space-between;
  align-items: center;
  width: 20%;
}

nav a {
  font-size: 1.5rem;
}
</style>
