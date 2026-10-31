<script setup>
import { ref, computed, onMounted, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { useI18n } from 'vue-i18n';
import { fetchDashboardPayload } from '../../../shared/infrastructure/dashboard-payload.js';

const route = useRoute();
const router = useRouter();
const { t } = useI18n();

const loading = ref(true);
const establishment = ref(null);
const operatorsCount = ref(0);
const devicesCount = ref(0);

function sameEst(a, b) {
  return Number(a) === Number(b) || String(a) === String(b);
}

async function load() {
  loading.value = true;
  const eid = route.params.establishmentId;
  try {
    const data = await fetchDashboardPayload();
    establishment.value = data.establishments.find((e) => sameEst(e.id, eid)) ?? null;
    operatorsCount.value = (data.operators || []).filter((op) => sameEst(op.establishment_id, eid)).length;
    devicesCount.value = (data.devices || []).filter((d) => sameEst(d.establishment_id, eid)).length;
  } catch (e) {
    console.error(e);
    establishment.value = null;
  } finally {
    loading.value = false;
  }
}

onMounted(load);
watch(() => route.params.establishmentId, load);

const displayType = computed(() => {
  const raw = establishment.value?.establishment_type;
  if (!raw) return '—';
  const upper = String(raw).toUpperCase();
  if (upper === 'HOSPITAL') return t('establishment.typeHospital');
  if (upper === 'WAREHOUSE') return t('establishment.typeWarehouse');
  if (upper === 'CLINIC') return t('establishment.typeClinic');
  return String(raw).replace(/_/g, ' ');
});

function goList() {
  router.push({ name: 'establishments' });
}

function goTeam() {
  router.push({
    name: 'establishment-team',
    params: { establishmentId: route.params.establishmentId },
  });
}
</script>

<template>
  <div class="est-flow-page">
    <nav class="est-flow-back-bar" aria-label="Navegación">
      <button type="button" class="est-flow-back-btn" @click="goList">
        <i class="pi pi-arrow-left" aria-hidden="true"></i>
        <span>{{ t('establishment.backToList') }}</span>
      </button>
    </nav>

    <div v-if="loading" class="est-flow-card est-flow-state">
      <i class="pi pi-spin pi-spinner" aria-hidden="true"></i>
      <span>{{ t('establishment.detailLoading') }}</span>
    </div>

    <div v-else-if="!establishment" class="est-flow-card est-flow-state est-flow-state--warn">
      <p>{{ t('establishment.notFound') }}</p>
      <button type="button" class="est-flow-btn est-flow-btn--ghost" @click="goList">
        {{ t('establishment.back') }}
      </button>
    </div>

    <div v-else class="est-flow-card">
      <header class="est-flow-head">
        <h1 class="est-flow-title">{{ establishment.establishment_name }}</h1>
        <p class="est-flow-subtitle">{{ t('establishment.establishmentInfo') }}</p>
      </header>

      <div class="est-flow-fields est-flow-fields--span">
        <div class="est-flow-field">
          <span class="est-flow-field__label">{{ t('establishment.fieldId') }}</span>
          <span class="est-flow-field__value">{{ establishment.id }}</span>
        </div>
        <div class="est-flow-field">
          <span class="est-flow-field__label">{{ t('establishment.fieldType') }}</span>
          <span class="est-flow-field__value">{{ displayType }}</span>
        </div>
        <div class="est-flow-field est-flow-field--full">
          <span class="est-flow-field__label">{{ t('establishment.fieldName') }}</span>
          <span class="est-flow-field__value">{{ establishment.establishment_name }}</span>
        </div>
        <div class="est-flow-field">
          <span class="est-flow-field__label">{{ t('establishment.fieldAddress') }}</span>
          <span class="est-flow-field__value">{{ establishment.address || '—' }}</span>
        </div>
        <div class="est-flow-field">
          <span class="est-flow-field__label">{{ t('establishment.fieldDistrict') }}</span>
          <span class="est-flow-field__value">{{ establishment.district || '—' }}</span>
        </div>
        <div class="est-flow-field">
          <span class="est-flow-field__label">{{ t('establishment.fieldCity') }}</span>
          <span class="est-flow-field__value">{{ establishment.city_region || '—' }}</span>
        </div>
        <div class="est-flow-field">
          <span class="est-flow-field__label">{{ t('establishment.fieldCountry') }}</span>
          <span class="est-flow-field__value">{{ establishment.country || '—' }}</span>
        </div>
        <div class="est-flow-field">
          <span class="est-flow-field__label">{{ t('establishment.fieldPhone') }}</span>
          <span class="est-flow-field__value">{{ establishment.phone || '—' }}</span>
        </div>
        <div class="est-flow-field">
          <span class="est-flow-field__label">{{ t('establishment.fieldEmail') }}</span>
          <span class="est-flow-field__value">{{ establishment.email || '—' }}</span>
        </div>
        <div class="est-flow-field">
          <span class="est-flow-field__label">{{ t('establishment.fieldWebsite') }}</span>
          <span class="est-flow-field__value">{{ establishment.website || '—' }}</span>
        </div>
        <div class="est-flow-field">
          <span class="est-flow-field__label">{{ t('establishment.fieldOperatorsCount') }}</span>
          <span class="est-flow-field__value">{{ operatorsCount }}</span>
        </div>
        <div class="est-flow-field">
          <span class="est-flow-field__label">{{ t('establishment.fieldDevicesCount') }}</span>
          <span class="est-flow-field__value">{{ devicesCount }}</span>
        </div>
      </div>

      <footer class="est-flow-actions">
        <button type="button" class="est-flow-btn est-flow-btn--ghost" @click="goList">
          <i class="pi pi-arrow-left" aria-hidden="true"></i>
          <span>{{ t('establishment.back') }}</span>
        </button>
        <button type="button" class="est-flow-btn est-flow-btn--accent" @click="goTeam">
          <i class="pi pi-users" aria-hidden="true"></i>
          <span>{{ t('establishment.viewOperators') }}</span>
        </button>
      </footer>
    </div>
  </div>
</template>
