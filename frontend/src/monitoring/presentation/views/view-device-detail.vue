<script setup>
import { ref, computed, onMounted, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { useI18n } from 'vue-i18n';
import { useToast } from 'primevue/usetoast';
import useMonitoringStore from '../../application/monitoring.store.js';
import SensorReadingsGrid from '../components/sensor-readings-grid.vue';
import MtConfirmDialog from '../../../shared/presentation/components/mt-confirm-dialog.vue';

const route = useRoute();
const router = useRouter();
const { t } = useI18n();
const toast = useToast();
const monitoringStore = useMonitoringStore();

const loading = ref(true);
const device = ref(null);
const deleteDialogVisible = ref(false);
const deleting = ref(false);


async function load() {
  loading.value = true;
  const did = route.params.deviceId;
  try {
    await monitoringStore.fetchDevicesAsync();
    device.value =
      monitoringStore.devices.find((d) => String(d.id) === String(did)) ?? null;
  } catch (e) {
    console.error(e);
    device.value = null;
  } finally {
    loading.value = false;
  }
}

onMounted(load);
watch(() => route.params.deviceId, load);

function goList() {
  router.push({ name: 'devices' });
}

async function confirmDelete() {
  if (!device.value) return;
  deleting.value = true;
  try {
    await monitoringStore.deleteDeviceAsync(device.value);
    toast.add({ severity: 'success', summary: t('monitoring.deleteSuccess'), life: 3000 });
    goList();
  } catch {
    toast.add({ severity: 'error', summary: t('monitoring.deleteError'), life: 4000 });
  } finally {
    deleting.value = false;
  }
}

const medType = computed(() => {
  const raw = String(device.value?.type_of_medication || '').toUpperCase();
  const map = {
    REFRIGERATED: 'medicationVaccines',
    BIOLOGICAL: 'medicationBiologicals',
    CONTROLLED: 'medicationCreams',
    GENERAL: 'medicationSyrup',
    // legacy mock keys
    VACCINES: 'medicationVaccines',
    PILLS: 'medicationPills',
    CREAMS: 'medicationCreams',
    SYRUP: 'medicationSyrup',
    BIOLOGICALS: 'medicationBiologicals',
  };
  const key = map[raw];
  return key ? t(`monitoring.${key}`) : device.value?.type_of_medication || '—';
});
</script>

<template>
  <div class="est-flow-page">
    <nav class="est-flow-back-bar" aria-label="Navegación">
      <button type="button" class="est-flow-back-btn" @click="goList">
        <i class="pi pi-arrow-left" aria-hidden="true"></i>
        <span>{{ t('monitoring.backToDevices') }}</span>
      </button>
    </nav>

    <div v-if="loading" class="est-flow-card est-flow-state">
      <i class="pi pi-spin pi-spinner" aria-hidden="true"></i>
      <span>{{ t('monitoring.detailLoading') }}</span>
    </div>

    <div v-else-if="!device" class="est-flow-card est-flow-state est-flow-state--warn">
      <p>{{ t('monitoring.deviceNotFound') }}</p>
      <button type="button" class="est-flow-btn est-flow-btn--ghost" @click="goList">
        {{ t('monitoring.back') }}
      </button>
    </div>

    <div v-else class="est-flow-card">
      <header class="est-flow-head">
        <h1 class="est-flow-title">{{ device.exact_location || `Device #${device.id}` }}</h1>
        <p class="est-flow-subtitle">{{ t('monitoring.deviceInfo') }}</p>
      </header>

      <div class="est-flow-fields est-flow-fields--span">
        <div class="est-flow-field">
          <span class="est-flow-field__label">{{ t('monitoring.fieldId') }}</span>
          <span class="est-flow-field__value">{{ device.id }}</span>
        </div>
        <div class="est-flow-field">
          <span class="est-flow-field__label">{{ t('monitoring.fieldMedication') }}</span>
          <span class="est-flow-field__value">{{ medType }}</span>
        </div>
        <div class="est-flow-field est-flow-field--full">
          <span class="est-flow-field__label">{{ t('monitoring.fieldExactLocation') }}</span>
          <span class="est-flow-field__value">{{ device.exact_location || '—' }}</span>
        </div>
      </div>

      <h2 v-if="device" class="est-flow-section-title">{{ t('monitoring.sensorReadings') }}</h2>
      <p v-if="device" class="est-flow-live-hint">{{ t('monitoring.sensorLiveHint') }}</p>
      <SensorReadingsGrid
        v-if="device"
        :device="device"
      />
      <p v-else class="est-flow-subtitle" style="text-align: center; margin-top: 1rem">
        {{ t('monitoring.noSensorData') }}
      </p>

      <footer class="est-flow-actions">
        <button type="button" class="est-flow-btn est-flow-btn--danger" @click="deleteDialogVisible = true">
          <i class="pi pi-trash" aria-hidden="true"></i>
          <span>{{ t('monitoring.deleteDevice') }}</span>
        </button>
        <button type="button" class="est-flow-btn est-flow-btn--ghost" @click="goList">
          <i class="pi pi-arrow-left" aria-hidden="true"></i>
          <span>{{ t('monitoring.back') }}</span>
        </button>
      </footer>
    </div>

    <MtConfirmDialog
      v-model:visible="deleteDialogVisible"
      :title="t('monitoring.deleteConfirmTitle')"
      :message="t('monitoring.deleteConfirmMessage')"
      :meta="device ? (device.exact_location || `ID ${device.id}`) : ''"
      :confirm-label="t('monitoring.deleteDevice')"
      :cancel-label="t('common.cancel')"
      confirm-tone="danger"
      confirm-icon="pi pi-trash"
      @confirm="confirmDelete"
    />
  </div>
</template>
