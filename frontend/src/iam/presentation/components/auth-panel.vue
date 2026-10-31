<script setup>
import { ref, watch, onMounted, onUnmounted } from 'vue';
import { useI18n } from 'vue-i18n';
import LanguageSwitcher from '../../../shared/presentation/components/language-switcher.vue';
import '../auth-forms.css';

defineProps({
  showBack: { type: Boolean, default: false },
  singleColumn: { type: Boolean, default: false },
  compact: { type: Boolean, default: false },
  signup: { type: Boolean, default: false },
  wide: { type: Boolean, default: false },
});

const emit = defineEmits(['back']);
const { t, locale } = useI18n();

const headlines = [
  'login.headlineTemp',
  'login.headlineAlerts',
  'login.headlineTrace',
];

const activeSlide = ref(0);
const typedHeadline = ref('');
const carouselDelayMs = 5200;
const typeSpeedMs = 38;
const deleteSpeedMs = 18;

let carouselTimer = null;
let typeTimer = null;

function startCarousel() {
  stopCarousel();
  carouselTimer = setInterval(() => {
    activeSlide.value = (activeSlide.value + 1) % headlines.length;
  }, carouselDelayMs);
}

function stopCarousel() {
  if (carouselTimer !== null) {
    clearInterval(carouselTimer);
    carouselTimer = null;
  }
}

function selectSlide(index) {
  activeSlide.value = index;
  startCarousel();
}

function clearTypeTimer() {
  if (typeTimer !== null) {
    clearTimeout(typeTimer);
    typeTimer = null;
  }
}

function typeIn(target) {
  let count = 0;
  const step = () => {
    count += 1;
    typedHeadline.value = target.slice(0, count);
    if (count < target.length) {
      typeTimer = setTimeout(step, typeSpeedMs);
    }
  };
  step();
}

function deleteAllThen(next) {
  const step = () => {
    if (typedHeadline.value.length === 0) {
      next();
      return;
    }
    typedHeadline.value = typedHeadline.value.slice(0, -1);
    typeTimer = setTimeout(step, deleteSpeedMs);
  };
  step();
}

watch(
  [activeSlide, locale],
  () => {
    clearTypeTimer();
    const target = t(headlines[activeSlide.value]);
    if (!typedHeadline.value) {
      typeIn(target);
      return;
    }
    deleteAllThen(() => typeIn(target));
  },
  { immediate: true },
);

onMounted(() => {
  startCarousel();
  document.body.style.overflow = 'hidden';
});

onUnmounted(() => {
  stopCarousel();
  clearTypeTimer();
  document.body.style.overflow = '';
});
</script>

<template>
  <div
    class="auth-shell"
    :class="{
      'auth-shell--single': singleColumn,
      'auth-shell--compact': compact,
      'auth-shell--signup': signup,
      'auth-shell--wide': wide,
    }"
  >
    <div class="auth-backdrop" aria-hidden="true">
      <img src="/section_home-hero.jpeg" alt="" class="auth-backdrop-slide is-active" />
      <div class="auth-backdrop-tint"></div>
    </div>

    <div class="auth-lang">
      <language-switcher compact inverted />
    </div>

    <div class="auth-stage">
      <aside v-if="!singleColumn" class="auth-story">
        <video
          class="auth-story-video"
          autoplay
          muted
          loop
          playsinline
          preload="auto"
        >
          <source src="/auth-hero.mp4" type="video/mp4" />
        </video>

        <div class="auth-story-brand">
          <img src="/logo.png" :alt="t('common.logoAlt')" />
          <strong>{{ t('common.brandName') }}</strong>
        </div>

        <div class="auth-story-copy">
          <span class="auth-story-eyebrow">{{ t('login.brandMark') }}</span>
          <p class="auth-story-headline">
            {{ typedHeadline }}<span class="auth-caret" aria-hidden="true"></span>
          </p>
          <div class="auth-story-dots">
            <button
              v-for="(key, index) in headlines"
              :key="key"
              type="button"
              class="auth-story-dot"
              :class="{ 'is-active': activeSlide === index }"
              :aria-label="t(key)"
              @click="selectSlide(index)"
            />
          </div>
        </div>
      </aside>

      <section class="auth-panel">
        <button
          v-if="showBack"
          type="button"
          class="auth-back"
          :aria-label="t('establishment.back')"
          @click="emit('back')"
        >
          <i class="pi pi-arrow-left"></i>
        </button>
        <div class="auth-card">
          <div class="locale-pane">
            <slot />
          </div>
        </div>
      </section>
    </div>
  </div>
</template>

<style scoped>
.auth-shell {
  position: relative;
  isolation: isolate;
  display: flex;
  flex-direction: column;
  min-height: 100dvh;
  height: 100dvh;
  max-height: 100dvh;
  overflow: hidden;
  padding: 1.25rem 1.75rem;
  box-sizing: border-box;
  background: #112433;
  font-family: 'Outfit', var(--mt-font), sans-serif;
}

.auth-backdrop {
  position: absolute;
  inset: 0;
  z-index: 0;
}

.auth-backdrop-slide {
  position: absolute;
  inset: 0;
  width: 100%;
  height: 100%;
  object-fit: cover;
  transform: scale(1.12);
  filter: blur(28px) saturate(1.08);
}

.auth-backdrop-tint {
  position: absolute;
  inset: 0;
  background:
    radial-gradient(ellipse 50% 40% at 50% 45%, rgba(243, 112, 33, 0.22) 0%, transparent 70%),
    rgba(17, 36, 51, 0.55);
}

.auth-lang {
  position: absolute;
  top: 1.35rem;
  right: 1.5rem;
  z-index: 3;
}

.auth-stage {
  position: relative;
  z-index: 1;
  width: 100%;
  max-width: 1160px;
  margin: auto;
  flex: 1 1 auto;
  min-height: 0;
  height: 100%;
  max-height: 100%;
  display: grid;
  grid-template-columns: minmax(0, 1.12fr) minmax(320px, 420px);
  grid-template-rows: minmax(0, 1fr);
  align-items: stretch;
  border-radius: 30px;
  overflow: hidden;
  background: #fff;
  box-shadow: 0 40px 90px rgba(17, 36, 51, 0.45);
}

.auth-shell--signup .auth-stage {
  max-width: 1180px;
  grid-template-columns: minmax(0, 1.08fr) minmax(340px, 460px);
}

.auth-shell--single .auth-stage,
.auth-shell--wide .auth-stage {
  grid-template-columns: 1fr;
  max-width: min(1080px, 100%);
}

.auth-story {
  position: relative;
  min-width: 0;
  min-height: 0;
  align-self: stretch;
  display: flex;
  flex-direction: column;
  justify-content: flex-end;
  padding: 2.25rem;
  overflow: hidden;
  background: #112433;
  border: 5px solid #fff;
  border-radius: 26px;
}

.auth-story-video {
  position: absolute;
  inset: 0;
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.auth-story::after {
  content: '';
  position: absolute;
  inset: 0;
  background: linear-gradient(
    180deg,
    rgba(17, 36, 51, 0.12) 0%,
    rgba(17, 36, 51, 0.45) 48%,
    rgba(17, 36, 51, 0.88) 100%
  );
}

.auth-story-brand {
  position: relative;
  z-index: 1;
  display: flex;
  align-items: center;
  gap: 0.65rem;
  margin-bottom: auto;
}

.auth-story-brand img {
  width: 42px;
  height: 42px;
  object-fit: contain;
  filter: drop-shadow(0 6px 16px rgba(0, 0, 0, 0.35));
}

.auth-story-brand strong {
  font-size: 1.15rem;
  font-weight: 700;
  letter-spacing: -0.03em;
  color: #fff;
}

.auth-story-copy {
  position: relative;
  z-index: 1;
  max-width: 26rem;
  color: #fff;
}

.auth-story-eyebrow {
  display: inline-block;
  font-size: 0.72rem;
  font-weight: 700;
  letter-spacing: 0.14em;
  text-transform: uppercase;
  color: #f37021;
}

.auth-story-headline {
  margin: 0.85rem 0 0;
  min-height: 2.7em;
  font-size: clamp(1.35rem, 2.4vw, 1.7rem);
  font-weight: 600;
  line-height: 1.3;
  letter-spacing: -0.03em;
}

.auth-caret {
  display: inline-block;
  width: 3px;
  height: 0.9em;
  margin-left: 4px;
  vertical-align: -0.05em;
  background: #f37021;
  border-radius: 1px;
  animation: auth-caret-blink 1s steps(1, end) infinite;
}

@keyframes auth-caret-blink {
  0%, 50% { opacity: 1; }
  50.01%, 100% { opacity: 0; }
}

.auth-story-dots {
  display: flex;
  gap: 8px;
  margin-top: 1rem;
}

.auth-story-dot {
  width: 24px;
  height: 4px;
  border: none;
  border-radius: 999px;
  background: rgba(255, 255, 255, 0.35);
  cursor: pointer;
  padding: 0;
  transition: background 0.2s ease, width 0.2s ease;
}

.auth-story-dot.is-active {
  width: 36px;
  background: #f37021;
}

.auth-panel {
  position: relative;
  display: flex;
  align-items: center;
  align-items: safe center;
  justify-content: center;
  min-width: 0;
  min-height: 0;
  align-self: stretch;
  overflow-x: hidden;
  overflow-y: auto;
  padding: 1.5rem 1.75rem;
  background: #fff;
}

.auth-shell--compact .auth-panel,
.auth-shell--signup .auth-panel {
  align-items: center;
  padding: 1.5rem 1.6rem 1.6rem;
}

.auth-shell--wide .auth-panel {
  align-items: flex-start;
  padding: 1.5rem 1.25rem 2rem;
}

.auth-back {
  position: absolute;
  top: 1rem;
  left: 1rem;
  z-index: 2;
  width: 40px;
  height: 40px;
  border: 1px solid rgba(17, 36, 51, 0.12);
  border-radius: 50%;
  background: #fff;
  cursor: pointer;
  color: #112433;
  display: inline-flex;
  align-items: center;
  justify-content: center;
}

.auth-back:hover {
  border-color: #f37021;
  color: #f37021;
}

.auth-card {
  width: 100%;
  max-width: 400px;
  min-width: 0;
  overflow: hidden;
}

.locale-pane {
  width: 100%;
}

.locale-appear-enter-active,
.locale-appear-leave-active {
  transition: opacity 0.28s ease, transform 0.28s ease;
}

.locale-appear-enter-from {
  opacity: 0;
  transform: translateY(10px);
}

.locale-appear-leave-to {
  opacity: 0;
  transform: translateY(-8px);
}

@media (prefers-reduced-motion: reduce) {
  .locale-appear-enter-active,
  .locale-appear-leave-active {
    transition: none;
  }
}

.auth-shell--compact .auth-card,
.auth-shell--signup .auth-card {
  max-width: 440px;
}

.auth-shell--wide .auth-card {
  max-width: min(1080px, 100%);
}

.auth-shell--signup :deep(.btn-primary) {
  height: 44px;
}

:deep(.btn-primary) {
  width: 100%;
  height: 48px;
  position: relative;
  isolation: isolate;
  overflow: hidden;
  border: none;
  border-radius: 999px;
  background: #f37021;
  color: #fff;
  font-family: inherit;
  font-size: 0.95rem;
  font-weight: 600;
  cursor: pointer;
}

:deep(.btn-primary > *) {
  position: relative;
  z-index: 1;
}

:deep(.btn-primary::before) {
  content: '';
  position: absolute;
  inset: 0;
  z-index: 0;
  background: #112433;
  transform: translateX(-101%);
  transition: transform 0.48s cubic-bezier(0.22, 1, 0.36, 1);
}

:deep(.btn-primary:hover:not(:disabled)::before),
:deep(.btn-primary:focus-visible:not(:disabled)::before) {
  transform: translateX(0);
}

:deep(.btn-primary:disabled) {
  opacity: 0.55;
  cursor: not-allowed;
}

@media (prefers-reduced-motion: reduce) {
  :deep(.btn-primary::before) {
    transition: none;
  }
}

@media (max-width: 780px) {
  .auth-shell {
    padding: 0.65rem;
    padding-bottom: max(0.65rem, env(safe-area-inset-bottom));
  }

  .auth-stage {
    grid-template-columns: 1fr;
    grid-template-rows: minmax(0, 1fr);
    border-radius: 22px;
    max-width: 100%;
  }

  .auth-shell--signup .auth-stage,
  .auth-shell--single .auth-stage,
  .auth-shell--wide .auth-stage {
    max-width: 100%;
    grid-template-columns: 1fr;
  }

  .auth-story {
    display: none;
  }

  .auth-panel {
    align-items: flex-start;
    justify-content: flex-start;
    padding: 3.35rem 1.15rem 1.35rem;
    -webkit-overflow-scrolling: touch;
    overscroll-behavior: contain;
  }

  .auth-shell--compact .auth-panel,
  .auth-shell--signup .auth-panel,
  .auth-shell--wide .auth-panel {
    align-items: flex-start;
    padding: 3.35rem 1.15rem 1.5rem;
  }

  .auth-card {
    max-width: none;
    overflow: visible;
  }

  .auth-lang {
    top: 1.05rem;
    right: 1.05rem;
  }

  .auth-back {
    top: 0.85rem;
    left: 0.85rem;
  }
}

@media (max-width: 420px) {
  .auth-shell {
    padding: 0.4rem;
  }

  .auth-stage {
    border-radius: 18px;
  }

  .auth-panel,
  .auth-shell--compact .auth-panel,
  .auth-shell--signup .auth-panel,
  .auth-shell--wide .auth-panel {
    padding: 3.1rem 0.95rem 1.15rem;
  }
}
</style>
