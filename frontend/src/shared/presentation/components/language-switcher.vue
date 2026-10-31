<script setup>
import { useI18n } from 'vue-i18n';

defineProps({
  compact: { type: Boolean, default: false },
  inverted: { type: Boolean, default: false },
});

const { locale, availableLocales } = useI18n();

function toggleLocale() {
  const list = availableLocales.length ? availableLocales : ['es', 'en'];
  const index = list.indexOf(locale.value);
  locale.value = list[(index + 1) % list.length];
}
</script>

<template>
  <button
    type="button"
    class="lang-toggle"
    :class="{
      'lang-toggle--compact': compact,
      'lang-toggle--inverted': inverted,
    }"
    :aria-label="locale.toUpperCase()"
    @click="toggleLocale"
  >
    <i class="pi pi-globe" aria-hidden="true"></i>
    <Transition name="lang-pop" mode="out-in">
      <span :key="locale">{{ locale.toUpperCase() }}</span>
    </Transition>
  </button>
</template>

<style scoped>
.lang-toggle {
  display: inline-flex;
  align-items: center;
  gap: 0.4rem;
  height: 2.35rem;
  padding: 0 0.9rem 0 0.75rem;
  border: 1.5px solid rgba(255, 255, 255, 0.55);
  border-radius: 999px;
  background: rgba(17, 36, 51, 0.28);
  color: #fff;
  font-family: inherit;
  font-size: 0.82rem;
  font-weight: 700;
  letter-spacing: 0.04em;
  cursor: pointer;
  backdrop-filter: blur(10px);
}

.lang-toggle:hover {
  background: rgba(17, 36, 51, 0.45);
}

.lang-toggle i {
  font-size: 0.95rem;
}

.lang-toggle--compact:not(.lang-toggle--inverted) {
  border-color: rgba(17, 36, 51, 0.16);
  background: #fff;
  color: #112433;
}

.lang-toggle--inverted {
  border-color: rgba(255, 255, 255, 0.7);
  background: transparent;
  color: #fff;
}

.lang-pop-enter-active,
.lang-pop-leave-active {
  transition: opacity 0.18s ease, transform 0.18s ease;
}

.lang-pop-enter-from {
  opacity: 0;
  transform: translateY(6px);
}

.lang-pop-leave-to {
  opacity: 0;
  transform: translateY(-6px);
}
</style>
