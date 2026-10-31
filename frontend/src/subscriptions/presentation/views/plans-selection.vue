<script setup>
import { ref, computed, onMounted } from 'vue';
import { useI18n } from 'vue-i18n';
import { useRouter } from 'vue-router';
import { useToast } from 'primevue/usetoast';
import useSubscriptionsStore from '../../application/subscriptions.store.js';
import { Subscription } from '../../domain/model/subscription.entity.js';
import {
  readAuthSession,
  writeAuthSession,
} from '../../../iam/infrastructure/auth-session.js';
import useIamStore from '../../../iam/application/iam.store.js';
import { getProfileRoute } from '../../../shared/infrastructure/route.helpers.js';
import MtConfirmDialog from '../../../shared/presentation/components/mt-confirm-dialog.vue';
import { onBeforeRouteLeave } from 'vue-router';

const { t, locale } = useI18n();
const router = useRouter();
const toast = useToast();

const currentCatalogId = ref('basic');
const benefitsEndDate = ref(null);
const cancelOpen = ref(false);
const iamStore = useIamStore();
const subscriptionsStore = useSubscriptionsStore();

const planDefs = computed(() => [
  {
    id: 'basic',
    name: t('plansPage.names.basic'),
    price: t('plansPage.prices.basic'),
    desc: t('plansPage.desc.basic'),
    recommended: false,
    features: [
      { ok: true, text: t('plansPage.features.basic.0') },
      { ok: true, text: t('plansPage.features.basic.1') },
      { ok: true, text: t('plansPage.features.basic.2') },
      { ok: true, text: t('plansPage.features.basic.3') },
      { ok: false, text: t('plansPage.features.basic.4') },
      { ok: false, text: t('plansPage.features.basic.5') },
    ],
  },
  {
    id: 'professional',
    name: t('plansPage.names.pro'),
    price: t('plansPage.prices.pro'),
    desc: t('plansPage.desc.pro'),
    recommended: true,
    features: [
      { ok: true, text: t('plansPage.features.pro.0') },
      { ok: true, text: t('plansPage.features.pro.1') },
      { ok: true, text: t('plansPage.features.pro.2') },
      { ok: true, text: t('plansPage.features.pro.3') },
    ],
  },
  {
    id: 'premium',
    name: t('plansPage.names.premium'),
    price: t('plansPage.prices.premium'),
    desc: t('plansPage.desc.premium'),
    recommended: false,
    features: [
      { ok: true, text: t('plansPage.features.premium.0') },
      { ok: true, text: t('plansPage.features.premium.1') },
      { ok: true, text: t('plansPage.features.premium.2') },
      { ok: true, text: t('plansPage.features.premium.3') },
      { ok: true, text: t('plansPage.features.premium.4') },
      { ok: true, text: t('plansPage.features.premium.5') },
    ],
    contactSales: true,
  },
]);

function syncFromSession() {
  const ctx = subscriptionsStore.readPlanContext();
  if (ctx?.catalogPlanId) {
    currentCatalogId.value = ctx.catalogPlanId;
  } else if (ctx?.planApiValue) {
    currentCatalogId.value = Subscription.apiPlanToCatalogId(ctx.planApiValue);
  }
  benefitsEndDate.value = ctx?.benefitsEndDate ?? null;
}

onMounted(() => {
  syncFromSession();
});

onBeforeRouteLeave(() => {
  cancelOpen.value = false;
});

const formattedBenefitsEnd = computed(() => {
  if (!benefitsEndDate.value) return '—';
  try {
    return new Date(benefitsEndDate.value).toLocaleDateString(
      locale.value === 'es' ? 'es-PE' : 'en-US',
      { day: '2-digit', month: '2-digit', year: 'numeric' },
    );
  } catch {
    return benefitsEndDate.value;
  }
});

const backToProfile = () => {
  router.push(getProfileRoute());
};

const isCurrent = (id) => id === currentCatalogId.value;

const openCancel = () => {
  cancelOpen.value = true;
};

const closeCancel = () => {
  cancelOpen.value = false;
};

const confirmCancel = () => {
  const end = new Date();
  end.setMonth(end.getMonth() + 1);
  const iso = end.toISOString().slice(0, 10);
  const ctx = subscriptionsStore.readPlanContext() || {};
  subscriptionsStore.writePlanContext({
    ...ctx,
    planApiValue: 'BASIC',
    catalogPlanId: 'basic',
    benefitsEndDate: iso,
  });
  const auth = readAuthSession() || {};
  auth.plan = 'BASIC';
  writeAuthSession(auth);
  // actualizar store IAM en memoria para que el perfil refleje el cambio inmediatamente
  try {
    iamStore.profilePlanApi = 'BASIC';
    iamStore.profileBenefitsEndDate = iso;
  } catch (e) {
    // no bloquear si falla (demo)
  }
  currentCatalogId.value = 'basic';
  benefitsEndDate.value = iso;
  cancelOpen.value = false;
  toast.add({
    severity: 'success',
    summary: t('plansPage.cancelSuccessTitle'),
    detail: t('plansPage.cancelSuccessDetail'),
    life: 4000,
  });
};

const selectPlan = (catalogId) => {
  if (isCurrent(catalogId)) return;
  const plan = planDefs.value.find((p) => p.id === catalogId);
  if (plan?.contactSales) {
    toast.add({
      severity: 'info',
      summary: t('plansPage.contactTitle'),
      detail: t('plansPage.contactDetail'),
      life: 4000,
    });
    return;
  }
  const apiVal = Subscription.catalogIdToApiPlan(catalogId);
  const end = new Date();
  end.setFullYear(end.getFullYear() + 1);
  const iso = end.toISOString().slice(0, 10);
  const ctx = subscriptionsStore.readPlanContext() || {};
  subscriptionsStore.writePlanContext({
    ...ctx,
    planApiValue: apiVal,
    catalogPlanId: catalogId,
    benefitsEndDate: iso,
  });
  const auth = readAuthSession() || {};
  auth.plan = apiVal;
  writeAuthSession(auth);
  try {
    iamStore.profilePlanApi = apiVal;
    iamStore.profileBenefitsEndDate = iso;
  } catch (e) {
    // ignore
  }
  toast.add({
    severity: 'success',
    summary: t('plansPage.planUpdatedTitle'),
    detail: t('plansPage.planUpdatedDetail', { plan: plan?.name ?? '' }),
    life: 3500,
  });
  router.push(getProfileRoute());
};
</script>

<template>
  <div class="plans-page plans-selection">
    <nav class="plans-back-bar" aria-label="Navegación">
      <button type="button" class="plans-back-btn" @click="backToProfile">
        <i class="pi pi-arrow-left" aria-hidden="true"></i>
        <span>{{ t('plansPage.back') }}</span>
      </button>
    </nav>

    <div class="plans-card">
      <header class="plans-head">
        <h1 class="plans-title">{{ t('plansPage.title') }}</h1>
        <p class="plans-subtitle">{{ t('plansPage.subtitle') }}</p>
      </header>

      <div class="plans-grid">
        <article
          v-for="plan in planDefs"
          :key="plan.id"
          class="plan-card"
          :class="{
            'plan-card--current': isCurrent(plan.id),
            'plan-card--recommended': plan.recommended,
          }"
        >
          <div v-if="plan.recommended" class="badge-rec">{{ t('plansPage.recommended') }}</div>
          <h2 class="plan-name">{{ plan.name }}</h2>
          <p class="plan-price">{{ plan.price }}</p>
          <p class="plan-desc">{{ plan.desc }}</p>
          <ul class="plan-features">
            <li v-for="(f, idx) in plan.features" :key="idx" :class="f.ok ? 'ok' : 'no'">
              <i :class="f.ok ? 'pi pi-check' : 'pi pi-times'" aria-hidden="true"></i>
              <span>{{ f.text }}</span>
            </li>
          </ul>
          <div class="plan-actions">
            <template v-if="isCurrent(plan.id)">
              <button type="button" class="plan-btn plan-btn--danger" @click="openCancel">
                <i class="pi pi-times-circle" aria-hidden="true"></i>
                <span>{{ t('plansPage.cancelPlan') }}</span>
              </button>
            </template>
            <template v-else-if="plan.contactSales">
              <button type="button" class="plan-btn plan-btn--ghost" @click="selectPlan(plan.id)">
                <span>{{ t('plansPage.contactSales') }}</span>
                <i class="pi pi-envelope" aria-hidden="true"></i>
              </button>
            </template>
            <template v-else>
              <button type="button" class="plan-btn plan-btn--primary" @click="selectPlan(plan.id)">
                <span>{{ t('plansPage.startNow') }}</span>
                <i class="pi pi-arrow-right" aria-hidden="true"></i>
              </button>
            </template>
          </div>
        </article>
      </div>
    </div>

    <MtConfirmDialog
      v-if="cancelOpen"
      v-model:visible="cancelOpen"
      :eyebrow="t('plansPage.cancelEyebrow')"
      :title="t('plansPage.cancelConfirmTitle')"
      :message="t('plansPage.cancelBenefitsEnd', { date: formattedBenefitsEnd })"
      :confirm-label="t('plansPage.confirm')"
      :cancel-label="t('plansPage.cancel')"
      confirm-tone="danger"
      confirm-icon="pi pi-check"
      @confirm="confirmCancel"
      @cancel="closeCancel"
    />
  </div>
</template>

<style scoped>
.plans-page {
  width: 100%;
  max-width: 1080px;
  margin: 0 auto;
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
  padding-bottom: 1.5rem;
}

.plans-back-bar {
  background: #fff;
  border: 1px solid var(--mt-border);
  border-radius: 14px;
  padding: 0.55rem 0.75rem;
  box-shadow: 0 2px 10px rgba(15, 23, 42, 0.04);
}

.plans-back-btn {
  display: inline-flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.35rem 0.5rem;
  border: none;
  background: transparent;
  color: var(--mt-primary);
  font-size: 0.875rem;
  font-weight: 600;
  font-family: inherit;
  cursor: pointer;
  border-radius: 8px;
  transition: background 0.15s ease, color 0.15s ease;
}

.plans-back-btn:hover {
  background: var(--mt-primary-soft);
  color: var(--mt-primary-hover);
}

.plans-back-btn i {
  font-size: 0.8rem;
}

.plans-selection .plan-card {
  min-height: 26rem;
}

@media (max-width: 960px) {
  .plans-selection .plan-card {
    min-height: auto;
  }
}
</style>
