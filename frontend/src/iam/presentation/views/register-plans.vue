<script setup>
import { computed, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import { useI18n } from 'vue-i18n';
import { useToast } from 'primevue/usetoast';
import AuthPanel from '../components/auth-panel.vue';
import useIamStore from '../../application/iam.store.js';
import { readPendingRegistration } from '../../infrastructure/auth-session.js';
import { Subscription } from '../../../subscriptions/domain/model/subscription.entity.js';

const { t } = useI18n();
const router = useRouter();
const toast = useToast();
const iamStore = useIamStore();

onMounted(() => {
  if (!readPendingRegistration()) {
    router.replace({ name: 'iam-register' });
  }
});

const plans = computed(() =>
  Subscription.PLAN_CATALOG_ORDER.map((id) => {
    const def = Subscription.getPlanByCatalogId(id);
    const limits = def.limits;
    return {
      id,
      name: t(`plansPage.names.${id}`),
      price: t(`plansPage.prices.${id}`),
      desc: t(`plansPage.desc.${id}`),
      recommended: def.recommended,
      contactSales: id === 'enterprise',
      features: [
        t('plansPage.featureLabels.establishments', { n: limits.establishments }),
        t('plansPage.featureLabels.staticDevices', { n: limits.staticDevices }),
        t('plansPage.featureLabels.vehicleDevices', { n: limits.vehicleDevices }),
        def.customParameters
          ? t('plansPage.featureLabels.customParamsYes')
          : t('plansPage.featureLabels.customParamsNo'),
        t('plansPage.featureLabels.operators'),
        ...(id === 'enterprise' ? [t('plansPage.featureLabels.enterpriseExtras')] : []),
        t('plansPage.featureLabels.reports'),
      ],
    };
  }),
);

function selectPlan(plan) {
  if (plan.contactSales) {
    toast.add({
      severity: 'info',
      summary: t('plansPage.contactTitle'),
      detail: t('plansPage.contactDetail'),
      life: 4000,
    });
    return;
  }
  iamStore.saveRegistrationPlan(plan.id);
  router.push({ name: 'iam-billing' });
}

function goBack() {
  router.push({ name: 'iam-register' });
}
</script>

<template>
  <auth-panel show-back single-column wide @back="goBack">
    <div class="plans-page plans-page--auth">
      <div class="plans-card">
        <header class="plans-head">
          <img src="/logo.png" :alt="t('common.logoAlt')" class="plans-logo" />
          <h1 class="plans-title">{{ t('plansPage.title') }}</h1>
          <p class="plans-subtitle">{{ t('plansPage.subtitle') }}</p>
        </header>

        <div class="plans-grid">
          <article
            v-for="plan in plans"
            :key="plan.id"
            class="plan-card"
            :class="{ 'plan-card--recommended': plan.recommended }"
          >
            <div v-if="plan.recommended" class="badge-rec">{{ t('plansPage.recommended') }}</div>
            <h2 class="plan-name">{{ plan.name }}</h2>
            <p class="plan-price">{{ plan.price }}</p>
            <p class="plan-desc">{{ plan.desc }}</p>
            <ul class="plan-features">
              <li v-for="(f, i) in plan.features" :key="i" class="ok">
                <i class="pi pi-check" aria-hidden="true"></i>
                {{ f }}
              </li>
            </ul>
            <div class="plan-actions">
              <button
                v-if="plan.contactSales"
                type="button"
                class="plan-btn plan-btn--ghost"
                @click="selectPlan(plan)"
              >
                <span>{{ t('plansPage.contactSales') }}</span>
                <i class="pi pi-envelope" aria-hidden="true"></i>
              </button>
              <button
                v-else
                type="button"
                class="plan-btn plan-btn--primary"
                @click="selectPlan(plan)"
              >
                <span>{{ t('plansPage.startNow') }}</span>
                <i class="pi pi-arrow-right" aria-hidden="true"></i>
              </button>
            </div>
          </article>
        </div>
      </div>
    </div>
  </auth-panel>
</template>
