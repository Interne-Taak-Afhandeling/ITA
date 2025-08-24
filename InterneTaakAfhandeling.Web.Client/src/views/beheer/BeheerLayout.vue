<template>
  <nav class="utrecht-nav-bar" aria-label="Beheermenu">
    <div class="utrecht-nav-bar__content">
      <ul data-v-6c285e9a="" role="list" class="utrecht-nav-list" id="menu">
        <li data-v-6c285e9a="" class="utrecht-nav-list__item">
          <router-link
            to="/beheer/kanalen"
            class="utrecht-link utrecht-link--html-a utrecht-nav-list__link"
            >Kanalen</router-link
          >
        </li>
      </ul>
    </div>
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
  font-size: 1rem;
}
</style>
