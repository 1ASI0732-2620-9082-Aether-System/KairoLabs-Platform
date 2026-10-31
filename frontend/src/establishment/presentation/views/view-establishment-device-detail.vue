<script setup>
import { ref, computed, onMounted, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { useI18n } from 'vue-i18n';
import { fetchDashboardPayload } from '../../../shared/infrastructure/dashboard-payload.js';
import SensorReadingsGrid from '../../../monitoring/presentation/components/sensor-readings-grid.vue';

const route = useRoute();
const router = useRouter();
const { t } = useI18n();

const loading = ref(true);
const establishment = ref(null);
const device = ref(null);


function sameEst(a, b) {
  return Number(a) === Number(b) || String(a) === String(b);
}

async function load() {
  loading.value = true;
  const eid = route.params.establishmentId;
  const did = route.params.deviceId;
  try {
    const data = await fetchDashboardPayload();
    establishment.value = data.establishments.find((e) => sameEst(e.id, eid)) ?? null;
    device.value =
      (data.devices || []).find(
        (d) => String(d.id) === String(did) && sameEst(d.establishment_id, eid),
      ) ?? null;
  } catch (e) {
    console.error(e);
    device.value = null;
  } finally {
    loading.value = false;
  }
}

onMounted(load);
watch(() => [route.params.establishmentId, route.params.deviceId], load);

function goTeam() {
  router.push({
    name: 'establishment-team',
    params: { establishmentId: route.params.establishmentId },
  });
}

const medType = computed(() => {
  const d = device.value;
  if (!d) return '—';
  const raw = String(d.type_of_medication || '').toUpperCase();
  const map = {
    VACCINES: 'medicationVaccines',
    PILLS: 'medicationPills',
    CREAMS: 'medicationCreams',
    SYRUP: 'medicationSyrup',
    BIOLOGICALS: 'medicationBiologicals',
  };
  const key = map[raw];
  if (key) return t(`monitoring.${key}`);
  return d.type_of_medication ?? d.medication_type ?? '—';
});
</script>

<template>
  <div class="est-flow-page">
    <nav class="est-flow-back-bar" aria-label="Navegación">
      <button type="button" class="est-flow-back-btn" @click="goTeam">
        <i class="pi pi-arrow-left" aria-hidden="true"></i>
        <span>{{ t('establishment.backToTeam') }}</span>
      </button>
    </nav>

    <div v-if="loading" class="est-flow-card est-flow-state">
      <i class="pi pi-spin pi-spinner" aria-hidden="true"></i>
      <span>{{ t('establishment.detailLoading') }}</span>
    </div>

    <div v-else-if="!device" class="est-flow-card est-flow-state est-flow-state--warn">
      <p>{{ t('establishment.deviceNotFound') }}</p>
      <button type="button" class="est-flow-btn est-flow-btn--ghost" @click="goTeam">
        {{ t('establishment.back') }}
      </button>
    </div>

    <div v-else class="est-flow-card">
      <header class="est-flow-head">
        <h1 class="est-flow-title">{{ device.exact_location || `Device #${device.id}` }}</h1>
        <p class="est-flow-subtitle">{{ t('establishment.deviceInfo') }}</p>
      </header>

      <div class="est-flow-fields est-flow-fields--span">
        <div class="est-flow-field">
          <span class="est-flow-field__label">{{ t('establishment.fieldId') }}</span>
          <span class="est-flow-field__value">{{ device.id }}</span>
        </div>
        <div class="est-flow-field">
          <span class="est-flow-field__label">{{ t('establishment.fieldAssignedEst') }}</span>
          <span class="est-flow-field__value">{{ establishment?.establishment_name || '—' }}</span>
        </div>
        <div class="est-flow-field est-flow-field--full">
          <span class="est-flow-field__label">{{ t('establishment.fieldExactLocation') }}</span>
          <span class="est-flow-field__value">{{ device.exact_location || '—' }}</span>
        </div>
        <div class="est-flow-field est-flow-field--full">
          <span class="est-flow-field__label">{{ t('establishment.fieldMedicationType') }}</span>
          <span class="est-flow-field__value">{{ medType }}</span>
        </div>
      </div>

      <h2 class="est-flow-section-title">{{ t('establishment.sensorReadings') }}</h2>
      <p class="est-flow-live-hint">{{ t('establishment.sensorLiveHint') }}</p>
      <SensorReadingsGrid :device="device" label-prefix="establishment" />

      <footer class="est-flow-actions">
        <button type="button" class="est-flow-btn est-flow-btn--ghost" @click="goTeam">
          <i class="pi pi-arrow-left" aria-hidden="true"></i>
          <span>{{ t('establishment.back') }}</span>
        </button>
      </footer>
    </div>
  </div>
</template>
