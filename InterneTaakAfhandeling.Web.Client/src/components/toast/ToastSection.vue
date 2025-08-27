<template>
  <transition-group name="toast" tag="section">
    <output
      v-for="(message, key) in messages"
      :key="key"
      role="status"
      @click="toast.remove(message)"
    >
      <utrecht-alert :type="message.type">
        <div v-html="message.text" class="preserve-newline"></div>
      </utrecht-alert>
    </output>
  </transition-group>
</template>

<script lang="ts" setup>
import UtrechtAlert from "../UtrechtAlert.vue";
import { messages, toast } from "./toast";
</script>

<style scoped lang="scss">
section {
  --_gap: var(--ita-toast-gap, 0.5rem);
  position: fixed;
  z-index: 2;
  display: grid;
  justify-items: center;
  justify-content: center;
  gap: var(--_gap);
  pointer-events: none;
  inset-block-start: min(50vh, 20rem);
  inset-inline: 0;
  padding: 0;
  margin: 0;
  border: none;
}

output {
  display: grid;
  grid: auto-flow / auto 1fr auto;
  pointer-events: auto;
}

.toast-move,
.toast-enter-active,
.toast-leave-active {
  transition: all 0.5s ease;
}

.toast-enter-from,
.toast-leave-to {
  opacity: 0;
}

.toast-leave-active {
  position: absolute;
}
</style>
