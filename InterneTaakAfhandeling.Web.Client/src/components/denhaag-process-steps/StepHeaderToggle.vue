<template>
  <utrecht-button
    type="button"
    appearance="subtle-button"
    class="denhaag-process-steps__step-header-toggle"
    @click="$emit('click')"
  >
    <slot></slot>
    <!-- to do icon -->
    <svg
      v-if="expanded"
      class="denhaag-icon"
      width="24"
      height="24"
      viewBox="0 0 24 24"
      fill="none"
      xmlns="http://www.w3.org/2000/svg"
    >
      <path
        d="M11.2929 7.29289C11.6834 6.90237 12.3166 6.90237 12.7071 7.29289L18.7071 13.2929C19.0976 13.6834 19.0976 14.3166 18.7071 14.7071C18.3166 15.0976 17.6834 15.0976 17.2929 14.7071L12 9.41421L6.70711 14.7071C6.31658 15.0976 5.68342 15.0976 5.29289 14.7071C4.90237 14.3166 4.90237 13.6834 5.29289 13.2929L11.2929 7.29289Z"
        fill="currentColor"
      />
    </svg>

    <svg
      v-else
      class="denhaag-icon"
      width="24"
      height="24"
      viewBox="0 0 24 24"
      fill="none"
      xmlns="http://www.w3.org/2000/svg"
    >
      <path
        d="M5.29289 8.29289C5.68342 7.90237 6.31658 7.90237 6.70711 8.29289L12 13.5858L17.2929 8.29289C17.6834 7.90237 18.3166 7.90237 18.7071 8.29289C19.0976 8.68342 19.0976 9.31658 18.7071 9.70711L12.7071 15.7071C12.3166 16.0976 11.6834 16.0976 11.2929 15.7071L5.29289 9.70711C4.90237 9.31658 4.90237 8.68342 5.29289 8.29289Z"
        fill="currentColor"
      />
    </svg>
  </utrecht-button>
</template>

<script setup lang="ts">
defineProps<{
  expanded?: boolean;
}>();

defineEmits<{ click: [] }>();
</script>

<style lang="scss" scoped>
.denhaag-icon {
  display: inline-block;
  flex-shrink: 0;
  width: 1em;
  height: 1em;
  font-size: 1.5rem;
  fill: currentColor;
  transition: fill 200ms cubic-bezier(0.4, 0, 0.2, 1) 0ms;
}
</style>
