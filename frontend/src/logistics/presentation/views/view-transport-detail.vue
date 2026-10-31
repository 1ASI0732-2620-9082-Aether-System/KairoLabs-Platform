<script setup>
import { ref, computed, onMounted, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { useI18n } from 'vue-i18n';
import useLogisticsStore from '../../application/logistics.store.js';
import SensorReadingsGrid from '../../../monitoring/presentation/components/sensor-readings-grid.vue';

const route = useRoute();
const router = useRouter();
const { t } = useI18n();
const logisticsStore = useLogisticsStore();

const loading = ref(true);
const transport = ref(null);

async function load() {
  loading.value = true;
  const tid = route.params.transportId;
  try {
    await logisticsStore.fetchTransportsAsync();
    transport.value =
      logisticsStore.transports.find((tr) => String(tr.id) === String(tid)) ?? null;
  } catch (e) {
    console.error(e);
    transport.value = null;
  } finally {
    loading.value = false;
  }
}

onMounted(load);
watch(() => route.params.transportId, load);

function goList() {
  router.push({ name: 'transports' });
}

const transportType = computed(() => {
  const raw = String(transport.value?.type_of_transport || '').toUpperCase()
    .replace('_', '');
  const map = {
    VAN: 'transportVan',
    OFFROAD: 'transportOffRoad',
    MOTORCYCLE: 'transportMotorcycle',
    REFRIGERATED: 'transportRefrigerated',
    COLDCHAIN: 'transportColdChain',
  };
  const key = map[raw];
  return key ? t(`logistics.${key}`) : transport.value?.type_of_transport || '—';
});

const medType = computed(() => {
  const raw = String(transport.value?.type_of_medication || '').toUpperCase();
  const map = {
    REFRIGERATED: 'medicationVaccines',
    BIOLOGICAL: 'medicationPills',
    CONTROLLED: 'medicationCreams',
    GENERAL: 'medicationSyrup',
  };
  const key = map[raw];
  return key ? t(`logistics.${key}`) : transport.value?.type_of_medication || '—';
});

const transportPlate = computed(() => {
  if (!transport.value?.id) return '—';
  const prefix = String(transport.value.type_of_transport || 'TR')
    .replace(/([a-z])([A-Z])/g, '$1_$2')
    .split(/[_\s-]+/)[0]
    .slice(0, 3)
    .toUpperCase() || 'TR';
  return `${prefix}-${String(transport.value.id).padStart(4, '0')}`;
});
</script>

<template>
  <div class="est-flow-page">
    <nav class="est-flow-back-bar" aria-label="Navegación">
      <button type="button" class="est-flow-back-btn" @click="goList">
        <i class="pi pi-arrow-left" aria-hidden="true"></i>
        <span>{{ t('logistics.backToTransports') }}</span>
      </button>
    </nav>

    <div v-if="loading" class="est-flow-card est-flow-state">
      <i class="pi pi-spin pi-spinner" aria-hidden="true"></i>
      <span>{{ t('logistics.detailLoading') }}</span>
    </div>

    <div v-else-if="!transport" class="est-flow-card est-flow-state est-flow-state--warn">
      <p>{{ t('logistics.transportNotFound') }}</p>
      <button type="button" class="est-flow-btn est-flow-btn--ghost" @click="goList">
        {{ t('logistics.back') }}
      </button>
    </div>

    <div v-else class="est-flow-card">
      <header class="est-flow-head">
        <p class="est-flow-eyebrow">{{ t('logistics.fieldPlate') }}</p>
        <h1 class="est-flow-title">{{ transportPlate }}</h1>
        <p class="est-flow-subtitle">{{ transportType }} · {{ t('logistics.transportInfo') }}</p>
      </header>

      <div class="est-flow-fields est-flow-fields--span">
        <div class="est-flow-field">
          <span class="est-flow-field__label">{{ t('logistics.fieldPlate') }}</span>
          <span class="est-flow-field__value transport-plate">{{ transportPlate }}</span>
        </div>
        <div class="est-flow-field">
          <span class="est-flow-field__label">{{ t('logistics.fieldId') }}</span>
          <span class="est-flow-field__value">{{ transport.id }}</span>
        </div>
        <div class="est-flow-field">
          <span class="est-flow-field__label">{{ t('logistics.fieldMedication') }}</span>
          <span class="est-flow-field__value">{{ medType }}</span>
        </div>
        <div class="est-flow-field est-flow-field--full">
          <span class="est-flow-field__label">{{ t('logistics.fieldTransportType') }}</span>
          <span class="est-flow-field__value">{{ transportType }}</span>
        </div>
      </div>

      <h2 v-if="transport" class="est-flow-section-title">{{ t('logistics.sensorReadings') }}</h2>
      <p v-if="transport" class="est-flow-live-hint">{{ t('monitoring.sensorLiveHint') }}</p>
      <SensorReadingsGrid
        v-if="transport"
        :device="transport"
        label-prefix="logistics"
      />

      <footer class="est-flow-actions">
        <button type="button" class="est-flow-btn est-flow-btn--ghost" @click="goList">
          <i class="pi pi-arrow-left" aria-hidden="true"></i>
          <span>{{ t('logistics.back') }}</span>
        </button>
      </footer>
    </div>
  </div>
</template>

<style scoped>
.transport-plate {
  font-family: ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, monospace;
  font-weight: 800;
  letter-spacing: 0.08em;
  color: var(--mt-primary);
}

.est-flow-eyebrow {
  margin: 0 0 0.35rem;
  font-size: 0.72rem;
  font-weight: 700;
  letter-spacing: 0.08em;
  text-transform: uppercase;
  color: var(--mt-text-muted);
}
</style>
