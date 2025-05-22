<template>
  <div :class="[
    'utrecht-alert', 
    `utrecht-alert--${appeareance}`
  ]">
    <div class="utrecht-alert__content">
      <div class="utrecht-alert__message" role="alert">
        <slot></slot>
      </div>
    </div>
  </div>
</template>
 
<script lang="ts">
  import { computed } from "vue";

  export type AlertType = "info" | "ok" | "warning" | "error"; 
  export default {
    name: 'UtrechtAlert',
    props: {
      appeareance: {
        type: String,
        default: 'info',
        validator: (value: string) => ['info', 'error', 'warning', 'ok', 'success'].includes(value)
      }
    }
  }
</script>
