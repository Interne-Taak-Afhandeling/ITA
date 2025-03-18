<template>
  <utrecht-page-footer class="ita-page-footer">
    <div class="utrecht-page-footer__content">
      <!-- <div class="utrecht-page-footer__navigation">
        <ul role="list" class="utrecht-link-list utrecht-link-list--html-ul">
          <li class="utrecht-link-list__item">
            <router-link
              :to="{ name: 'dashboard' }"
              class="utrecht-link utrecht-link--html-a utrecht-nav-list__link"
              >Dashboard</router-link
            >
          </li>
        </ul>
      </div> -->
    </div>
  </utrecht-page-footer>
</template>

<style lang="scss" scoped>
.utrecht-link-list {
  flex-flow: row wrap;
  justify-content: center;
  column-gap: var(--ita-footer-link-list-column-gap);
}
</style>
