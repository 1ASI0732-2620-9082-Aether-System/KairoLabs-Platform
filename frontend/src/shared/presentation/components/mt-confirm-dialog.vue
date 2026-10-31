<script setup>
import { computed } from 'vue';
import { useI18n } from 'vue-i18n';

const visible = defineModel('visible', { type: Boolean, default: false });

const props = defineProps({
  title: { type: String, required: true },
  message: { type: String, default: '' },
  meta: { type: String, default: '' },
  confirmLabel: { type: String, required: true },
  cancelLabel: { type: String, required: true },
  closeAriaLabel: { type: String, default: '' },
  eyebrow: { type: String, default: '' },
  confirmIcon: { type: String, default: '' },
  confirmTone: {
    type: String,
    default: 'accent',
    validator: (v) => ['accent', 'primary', 'danger'].includes(v),
  },
});

const emit = defineEmits(['confirm', 'cancel']);

const { t } = useI18n();

const confirmBtnClass = computed(() => {
  if (props.confirmTone === 'primary') return 'est-flow-btn est-flow-btn--primary';
  if (props.confirmTone === 'danger') return 'est-flow-btn est-flow-btn--danger';
  return 'est-flow-btn est-flow-btn--accent';
});

const dialogPt = computed(() => ({
  mask: { class: 'mt-dialog-mask p-dialog-mask' },
  root: {
    class: 'mt-dialog-wrap p-dialog',
    style: { border: 'none', background: 'transparent', boxShadow: 'none' },
  },
  content: {
    class: 'mt-dialog-content',
    style: { padding: 0, background: 'transparent', border: 'none', overflow: 'visible' },
  },
}));

function close() {
  visible.value = false;
  emit('cancel');
}

function onConfirm() {
  emit('confirm');
  visible.value = false;
}
</script>

<template>
  <pv-dialog
    v-model:visible="visible"
    :modal="true"
    :draggable="false"
    :closable="false"
    :show-header="false"
    dismissable-mask
    :style="{ width: 'min(92vw, 440px)' }"
    :pt="dialogPt"
    @hide="emit('cancel')"
  >
    <div class="mt-dialog-shell">
      <button
        type="button"
        class="mt-dialog-close"
        :aria-label="closeAriaLabel || t('plansPage.closeModal')"
        @click="close"
      >
        <i class="pi pi-times" aria-hidden="true"></i>
      </button>
      <div class="mt-dialog-inner">
        <p v-if="eyebrow" class="mt-dialog-eyebrow">{{ eyebrow }}</p>
        <h2 class="mt-dialog-title">{{ title }}</h2>
        <p v-if="message" class="mt-dialog-sub">{{ message }}</p>
        <p v-if="meta" class="mt-dialog-meta">{{ meta }}</p>
        <div class="mt-dialog-actions">
          <button type="button" class="est-flow-btn est-flow-btn--ghost" @click="close">
            {{ cancelLabel }}
          </button>
          <button type="button" :class="confirmBtnClass" @click="onConfirm">
            <i v-if="confirmIcon" :class="confirmIcon" aria-hidden="true"></i>
            {{ confirmLabel }}
          </button>
        </div>
      </div>
    </div>
  </pv-dialog>
</template>
