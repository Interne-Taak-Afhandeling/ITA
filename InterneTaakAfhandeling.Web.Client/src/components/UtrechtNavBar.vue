<template>
  <nav class="utrecht-nav-bar" aria-label="Hoofdmenu">
    <div class="utrecht-nav-bar__content">
      <ul role="list" class="utrecht-nav-list" id="menu">
        <li class="utrecht-nav-list__item">
          <router-link
            :to="{ name: 'dashboard' }"
            class="utrecht-link utrecht-link--html-a utrecht-nav-list__link"
            >Dashboard</router-link
          >
        </li>

        <li class="utrecht-nav-list__item">
          <router-link
            :to="{ name: 'alleContactverzoeken' }"
            class="utrecht-link utrecht-link--html-a utrecht-nav-list__link"
            >Alle contactverzoeken</router-link
          >
        </li>

        <!-- not implemented yet -->
        <!-- <li class="utrecht-nav-list__item">
          <router-link
            :to="{ name: 'historie' }"
            class="utrecht-link utrecht-link--html-a utrecht-nav-list__link"
            >Historie</router-link
          >
        </li> -->
      </ul>
    </div>
  </nav>
</template>
