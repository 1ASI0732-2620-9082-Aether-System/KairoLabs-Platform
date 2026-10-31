<script setup>
import { onMounted, ref } from 'vue';
import { useI18n } from 'vue-i18n';

const { t } = useI18n();
const done = ref(false);

onMounted(() => {
  const minDelay = 1400;
  const started = Date.now();

  const finish = () => {
    const wait = Math.max(0, minDelay - (Date.now() - started));
    window.setTimeout(() => {
      done.value = true;
      document.documentElement.classList.remove('is-loading');
    }, wait);
  };

  if (document.readyState === 'complete') {
    finish();
    return;
  }
  window.addEventListener('load', finish, { once: true });
});

document.documentElement.classList.add('is-loading');
</script>

<template>
  <div
    class="page-loader"
    :class="{ 'is-done': done }"
    aria-live="polite"
    :aria-busy="!done"
  >
    <div class="page-loader__ambient" aria-hidden="true"></div>
    <div class="page-loader__stage">
      <div class="page-loader__ring"></div>
      <div class="page-loader__core">
        <img src="/logo.png" :alt="t('common.logoAlt')" class="page-loader__logo" />
      </div>
    </div>
    <p class="page-loader__text">KairoLabs</p>
  </div>
</template>

<style scoped>
.page-loader {
  position: fixed;
  inset: 0;
  z-index: 10000;
  display: grid;
  place-items: center;
  align-content: center;
  gap: 1.4rem;
  background: #112433;
  transition: opacity 0.65s ease, visibility 0.65s ease;
}

.page-loader.is-done {
  opacity: 0;
  visibility: hidden;
  pointer-events: none;
}

.page-loader__ambient {
  position: absolute;
  inset: 0;
  background: radial-gradient(ellipse 50% 40% at 50% 45%, rgba(243, 112, 33, 0.2) 0%, transparent 70%);
  pointer-events: none;
}

.page-loader__stage {
  position: relative;
  z-index: 1;
  width: 132px;
  height: 132px;
  display: grid;
  place-items: center;
}

.page-loader__ring {
  position: absolute;
  inset: 0;
  border-radius: 50%;
  border: 2px solid rgba(255, 255, 255, 0.12);
  box-shadow: 0 0 0 1px rgba(243, 112, 33, 0.1);
}

.page-loader__ring::before {
  content: '';
  position: absolute;
  inset: -3px;
  border-radius: 50%;
  background: conic-gradient(
    from 0deg,
    transparent 0%,
    transparent 62%,
    rgba(243, 112, 33, 0.2) 72%,
    rgba(243, 112, 33, 0.9) 86%,
    #fff 94%,
    transparent 100%
  );
  -webkit-mask: radial-gradient(farthest-side, transparent calc(100% - 4px), #000 calc(100% - 3px));
  mask: radial-gradient(farthest-side, transparent calc(100% - 4px), #000 calc(100% - 3px));
  animation: pageLoaderSpin 1.35s linear infinite;
  filter: drop-shadow(0 0 10px rgba(243, 112, 33, 0.55));
}

.page-loader__core {
  position: relative;
  z-index: 1;
  width: 88px;
  height: 88px;
  display: grid;
  place-items: center;
  border-radius: 50%;
  background: #ffffff;
  border: 1px solid rgba(255, 255, 255, 0.2);
  box-shadow:
    0 0 0 1px rgba(243, 112, 33, 0.12) inset,
    0 10px 28px rgba(0, 0, 0, 0.35);
}

.page-loader__logo {
  width: 52px;
  height: 52px;
  object-fit: contain;
  filter: drop-shadow(0 0 10px rgba(243, 112, 33, 0.28));
  animation: pageLoaderPulse 1.8s ease-in-out infinite;
}

.page-loader__text {
  position: relative;
  z-index: 1;
  margin: 0;
  font-size: 0.82rem;
  font-weight: 700;
  letter-spacing: 0.2em;
  text-transform: uppercase;
  color: rgba(255, 255, 255, 0.78);
}

@keyframes pageLoaderSpin {
  to { transform: rotate(360deg); }
}

@keyframes pageLoaderPulse {
  0%, 100% { transform: scale(1); opacity: 1; }
  50% { transform: scale(1.06); opacity: 0.9; }
}

@media (prefers-reduced-motion: reduce) {
  .page-loader__ring::before,
  .page-loader__logo {
    animation: none;
  }
}
</style>
