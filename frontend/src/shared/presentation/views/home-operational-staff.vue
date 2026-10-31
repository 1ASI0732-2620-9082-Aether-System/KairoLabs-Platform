<script setup>
import { useI18n } from 'vue-i18n';
import { computed, ref, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import { readAuthSession } from '../../../iam/infrastructure/auth-session.js';
import useEstablishmentStore from '../../../establishment/application/establishment.store.js';

const { t, locale } = useI18n();
const router = useRouter();
const establishmentStore = useEstablishmentStore();

const session = readAuthSession();
const userName = computed(() => session?.name ?? t('layout.guestUser'));
const establishmentName = ref('—');
const alertsAnswered = ref(null);
const todayLabel = computed(() => {
  try {
    return new Date().toLocaleDateString(locale.value === 'es' ? 'es-PE' : 'en-US', {
      weekday: 'long',
      day: 'numeric',
      month: 'long',
      year: 'numeric',
    });
  } catch {
    return '';
  }
});

onMounted(async () => {
  try {
    await establishmentStore.fetchOperatorsAsync();
    await establishmentStore.fetchEstablishmentsAsync();
    const operatorId = session?.operatorId;
    const op = operatorId
      ? establishmentStore.operators.find((o) => Number(o.id) === Number(operatorId))
      : null;
    alertsAnswered.value = op?.alerts_answered ?? 0;
    const est = op?.establishment_id
      ? establishmentStore.establishments.find((e) => Number(e.id) === Number(op.establishment_id))
      : null;
    if (est) establishmentName.value = est.establishment_name;
  } catch {
    // non-critical
  }
});

const quickActions = [
  {
    titleKey: 'monitoring.devices',
    descKey: 'homeOperational.actionDevicesDesc',
    icon: 'pi pi-box',
    to: '/monitoring/devices',
    tone: 'teal',
  },
  {
    titleKey: 'logistics.transports',
    descKey: 'homeOperational.actionTransportsDesc',
    icon: 'pi pi-truck',
    to: '/logistics/transports',
    tone: 'navy',
  },
  {
    titleKey: 'establishment.operators',
    descKey: 'homeOperational.actionOperatorsDesc',
    icon: 'pi pi-users',
    to: '/establishment/operators',
    tone: 'teal',
  },
];

const navigateTo = (path) => {
  router.push(path);
};
</script>

<template>
  <div class="home-dash-page">
    <section class="home-hero-banner">
      <div class="home-hero-banner__top">
        <div>
          <h1 class="home-hero-banner__title">
            {{ t('homeOperational.welcomeName', { name: userName }) }}
          </h1>
          <p class="home-hero-banner__subtitle">
            {{ t('homeOperational.subtitle') }}
            <span v-if="establishmentName !== '—'">
              {{ t('homeOperational.atEstablishment', { place: establishmentName }) }}
            </span>
          </p>
        </div>
        <span class="home-hero-banner__status">
          <i class="home-hero-banner__dot" aria-hidden="true"></i>
          {{ t('layout.systemActive') }}
        </span>
      </div>
      <div class="home-hero-banner__chips">
        <span class="home-hero-chip">
          <i class="pi pi-desktop" aria-hidden="true"></i>
          {{ t('homeOperational.title') }}
        </span>
        <span class="home-hero-chip">
          <i class="pi pi-user" aria-hidden="true"></i>
          {{ t('iam.operational.badge') }}
        </span>
        <span class="home-hero-chip">
          <i class="pi pi-calendar" aria-hidden="true"></i>
          {{ todayLabel }}
        </span>
        <span v-if="alertsAnswered !== null" class="home-hero-chip">
          <i class="pi pi-check-circle" aria-hidden="true"></i>
          {{ alertsAnswered }} {{ t('homeOperational.alertsAnswered') }}
        </span>
      </div>
    </section>

    <div class="home-actions" role="navigation" :aria-label="t('homeOperational.title')">
      <button
        v-for="action in quickActions"
        :key="action.titleKey"
        type="button"
        class="home-action"
        @click="navigateTo(action.to)"
      >
        <span
          class="home-action__icon"
          :class="action.tone === 'teal' ? 'home-action__icon--teal' : 'home-action__icon--navy'"
          aria-hidden="true"
        >
          <i :class="action.icon"></i>
        </span>
        <span class="home-action__body">
          <h2 class="home-action__title">{{ t(action.titleKey) }}</h2>
          <p class="home-action__desc">{{ t(action.descKey) }}</p>
        </span>
        <i class="pi pi-chevron-right home-action__chevron" aria-hidden="true"></i>
      </button>
    </div>
  </div>
</template>
