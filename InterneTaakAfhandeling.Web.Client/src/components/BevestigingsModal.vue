<template>
  <utrecht-alert-dialog ref="bevestigingsDialogRef">
    <form method="dialog" @submit.prevent="handleConfirm">
      <utrecht-heading :level="2">{{ title }}</utrecht-heading>

      <utrecht-paragraph>
        {{ message }}
      </utrecht-paragraph>

      <utrecht-button-group>
        <utrecht-button appearance="primary-action-button" type="submit">
          <span>{{ confirmText }}</span>
        </utrecht-button>
        <utrecht-button appearance="secondary-action-button" type="button" @click="handleCancel">
          {{ cancelText }}
        </utrecht-button>
      </utrecht-button-group>
    </form>
  </utrecht-alert-dialog>
</template>

<script setup lang="ts">
import { ref } from "vue";

interface Props {
  title?: string;
  message: string;
  confirmText?: string;
  cancelText?: string;
}

withDefaults(defineProps<Props>(), {
  title: "Bevestiging",
  confirmText: "Bevestigen",
  cancelText: "Annuleren"
});

const emit = defineEmits<{
  confirm: [];
  cancel: [];
}>();

const bevestigingsDialogRef = ref<{ dialogRef?: HTMLDialogElement }>();

const show = () => {
  bevestigingsDialogRef.value?.dialogRef?.showModal();
};

const close = () => {
  bevestigingsDialogRef.value?.dialogRef?.close();
};

const handleConfirm = () => {
  emit("confirm");
  close();
};

const handleCancel = () => {
  emit("cancel");
  close();
};

defineExpose({
  show,
  close
});
</script>

<style lang="scss" scoped></style>
