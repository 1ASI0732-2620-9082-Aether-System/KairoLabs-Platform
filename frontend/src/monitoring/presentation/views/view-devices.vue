<script setup>
import { ref, onMounted, onUnmounted, computed } from 'vue';
import { useRouter } from 'vue-router';
import { useI18n } from 'vue-i18n';
import { useToast } from 'primevue/usetoast';
import useMonitoringStore from '../../application/monitoring.store.js';
import MtConfirmDialog from '../../../shared/presentation/components/mt-confirm-dialog.vue';
import DeviceForm from './device-form.vue';
import SensorReadingsGrid from '../components/sensor-readings-grid.vue';

const monitoringStore = useMonitoringStore();
const router = useRouter();
const { t } = useI18n();
const toast = useToast();

const devices = ref([]);
const isLoading = ref(true);
const searchQuery = ref('');
const deleteTarget = ref(null);
const deleteDialogVisible = ref(false);
const deleting = ref(false);

const addOpen = ref(false);
const detailOpen = ref(false);
const detailDevice = ref(null);

const dialogPt = {
  mask: { class: 'mt-dialog-mask p-dialog-mask est-inspect-mask' },
  root: {
    class: 'mt-dialog-wrap p-dialog est-inspect-dialog',
    style: { border: 'none', background: 'transparent', boxShadow: 'none' },
  },
  content: {
    class: 'mt-dialog-content est-inspect-content',
    style: { padding: 0, background: 'transparent', border: 'none', overflow: 'visible' },
  },
};

onMounted(async () => {
  try {
    devices.value = await monitoringStore.fetchDevicesAsync();
    monitoringStore.startSimulation(8000);
  } catch (error) {
    console.error('Error loading devices', error);
  } finally {
    isLoading.value = false;
  }
});

onUnmounted(() => {
  monitoringStore.stopSimulation();
});

const filteredDevices = computed(() => {
  const q = searchQuery.value.trim().toLowerCase();
  if (!q) return devices.value;
  return devices.value.filter(
    (d) =>
      (d.exact_location || '').toLowerCase().includes(q) ||
      (d.type_of_medication || '').toLowerCase().includes(q),
  );
});

const stats = computed(() => {
  const total = devices.value.length;
  const alerts = devices.value.filter(
    (d) => Number(d.temperature) > 20 || Number(d.humidity) > 60,
  ).length;
  return { total, alerts };
});

const liveDetailDevice = computed(() => {
  if (!detailDevice.value) return null;
  return devices.value.find((d) => d.id === detailDevice.value.id) ?? detailDevice.value;
});

const detailMedType = computed(() => {
  const raw = String(liveDetailDevice.value?.type_of_medication || '').toUpperCase();
  const map = {
    REFRIGERATED: 'medicationVaccines',
    BIOLOGICAL: 'medicationBiologicals',
    CONTROLLED: 'medicationCreams',
    GENERAL: 'medicationSyrup',
    VACCINES: 'medicationVaccines',
    PILLS: 'medicationPills',
    CREAMS: 'medicationCreams',
    SYRUP: 'medicationSyrup',
    BIOLOGICALS: 'medicationBiologicals',
  };
  const key = map[raw];
  return key ? t(`monitoring.${key}`) : liveDetailDevice.value?.type_of_medication || '—';
});

function formatMedication(type) {
  const raw = String(type || '').toUpperCase();
  const map = {
    VACCINES: 'medicationVaccines',
    PILLS: 'medicationPills',
    CREAMS: 'medicationCreams',
    SYRUP: 'medicationSyrup',
    BIOLOGICALS: 'medicationBiologicals',
    REFRIGERATED: 'medicationVaccines',
    BIOLOGICAL: 'medicationBiologicals',
    CONTROLLED: 'medicationCreams',
    GENERAL: 'medicationSyrup',
  };
  const key = map[raw];
  return key ? t(`monitoring.${key}`) : type || '—';
}

function hasAlert(dev) {
  return Number(dev.temperature) > 20 || Number(dev.humidity) > 60;
}

function goHome() {
  router.push({ name: 'home-operational-staff' });
}

function openAdd() {
  addOpen.value = true;
}

async function onDeviceSaved(created) {
  addOpen.value = false;
  devices.value = await monitoringStore.fetchDevicesAsync();
  if (created) {
    detailDevice.value = created;
    detailOpen.value = true;
  }
}

function viewDevice(dev) {
  detailDevice.value = dev;
  detailOpen.value = true;
}

function askDelete(dev) {
  deleteTarget.value = dev;
  deleteDialogVisible.value = true;
}

async function confirmDelete() {
  if (!deleteTarget.value) return;
  deleting.value = true;
  try {
    await monitoringStore.deleteDeviceAsync(deleteTarget.value);
    devices.value = devices.value.filter((d) => d.id !== deleteTarget.value.id);
    if (detailDevice.value?.id === deleteTarget.value.id) {
      detailOpen.value = false;
      detailDevice.value = null;
    }
    toast.add({ severity: 'success', summary: t('monitoring.deleteSuccess'), life: 3000 });
  } catch {
    toast.add({ severity: 'error', summary: t('monitoring.deleteError'), life: 4000 });
  } finally {
    deleting.value = false;
    deleteTarget.value = null;
  }
}
</script>

<template>
  <div class="est-flow-page">
    <nav class="est-flow-back-bar" aria-label="Navegación">
      <button type="button" class="est-flow-back-btn" @click="goHome">
        <i class="pi pi-arrow-left" aria-hidden="true"></i>
        <span>{{ t('monitoring.backToStaffHome') }}</span>
      </button>
    </nav>

    <div class="est-flow-card">
      <header class="est-flow-head est-flow-head--row">
        <div class="est-flow-head__text">
          <h1 class="est-flow-title">{{ t('monitoring.devices') }}</h1>
          <p class="est-flow-subtitle">{{ t('monitoring.listPageSubtitle') }}</p>
        </div>
        <div class="est-flow-stats">
          <div class="est-flow-stat est-flow-stat--navy">
            <span class="est-flow-stat__icon" aria-hidden="true"><i class="pi pi-th-large"></i></span>
            <span class="est-flow-stat__label">{{ t('monitoring.statDevices') }}</span>
            <span class="est-flow-stat__value">{{ stats.total }}</span>
          </div>
          <div
            class="est-flow-stat"
            :class="stats.alerts === 0 ? 'est-flow-stat--ok' : 'est-flow-stat--warn'"
          >
            <span class="est-flow-stat__icon" aria-hidden="true">
              <i :class="stats.alerts === 0 ? 'pi pi-check-circle' : 'pi pi-exclamation-triangle'"></i>
            </span>
            <span class="est-flow-stat__label">{{ t('monitoring.statGlobalStatus') }}</span>
            <span class="est-flow-stat__value">
              {{ stats.alerts > 0 ? stats.alerts : t('monitoring.systemOk') }}
            </span>
          </div>
        </div>
      </header>

      <div class="mon-toolbar-actions">
        <button type="button" class="est-flow-btn est-flow-btn--accent" @click="openAdd">
          <i class="pi pi-plus" aria-hidden="true"></i>
          <span>{{ t('monitoring.addDevice') }}</span>
        </button>
      </div>

      <div class="est-flow-toolbar">
        <div class="est-flow-search">
          <i class="pi pi-search" aria-hidden="true"></i>
          <input
            v-model="searchQuery"
            type="search"
            :placeholder="t('monitoring.searchPlaceholder')"
            autocomplete="off"
          />
        </div>
      </div>

      <div v-if="isLoading" class="est-flow-state">
        <i class="pi pi-spin pi-spinner" aria-hidden="true"></i>
        <span>{{ t('monitoring.loadingDevices') }}</span>
      </div>

      <div v-else-if="filteredDevices.length === 0" class="est-flow-state">
        <i class="pi pi-filter-slash" aria-hidden="true"></i>
        <span>{{ t('monitoring.emptySearch') }}</span>
      </div>

      <div v-else class="est-flow-table-wrap">
        <table class="est-flow-table">
          <thead>
            <tr>
              <th>{{ t('monitoring.colLocation') }}</th>
              <th>{{ t('monitoring.colMedication') }}</th>
              <th>{{ t('monitoring.colTemp') }}</th>
              <th>{{ t('monitoring.colHumidity') }}</th>
              <th class="est-flow-table__actions">{{ t('monitoring.colActions') }}</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="dev in filteredDevices" :key="dev.id">
              <td>
                <div class="est-flow-table__name">
                  <i
                    v-if="hasAlert(dev)"
                    class="pi pi-exclamation-triangle"
                    style="color: #d97706; margin-right: 0.35rem; font-size: 0.85rem"
                    aria-hidden="true"
                  ></i>
                  {{ dev.exact_location || `Device #${dev.id}` }}
                </div>
                <div class="est-flow-table__meta">ID {{ dev.id }}</div>
              </td>
              <td>{{ formatMedication(dev.type_of_medication) }}</td>
              <td>
                <span :style="{ color: hasAlert(dev) ? '#dc2626' : '#059669', fontWeight: 600 }">
                  {{ dev.temperature != null ? `${dev.temperature}°C` : '—' }}
                </span>
              </td>
              <td>{{ dev.humidity != null ? `${dev.humidity}%` : '—' }}</td>
              <td class="est-flow-table__actions">
                <div class="est-flow-table__actions-row">
                  <button
                    type="button"
                    class="est-flow-icon-btn"
                    :aria-label="t('monitoring.viewDetail')"
                    @click="viewDevice(dev)"
                  >
                    <i class="pi pi-eye" aria-hidden="true"></i>
                  </button>
                  <button
                    type="button"
                    class="est-flow-icon-btn est-flow-icon-btn--danger"
                    :aria-label="t('monitoring.deleteDevice')"
                    @click="askDelete(dev)"
                  >
                    <i class="pi pi-trash" aria-hidden="true"></i>
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <pv-dialog
      v-model:visible="addOpen"
      modal
      append-to="body"
      :draggable="false"
      :closable="false"
      :show-header="false"
      dismissable-mask
      :style="{ width: 'min(94vw, 720px)' }"
      :pt="dialogPt"
    >
      <div class="mt-dialog-shell est-inspect-shell">
        <button
          type="button"
          class="mt-dialog-close"
          :aria-label="t('common.cancel')"
          @click="addOpen = false"
        >
          <i class="pi pi-times" aria-hidden="true"></i>
        </button>
        <header class="est-inspect-header">
          <h2 class="est-flow-title">{{ t('monitoring.addDeviceTitle') }}</h2>
          <p class="est-flow-subtitle">{{ t('monitoring.addDeviceSubtitle') }}</p>
        </header>
        <div class="est-inspect-scroll">
          <DeviceForm
            v-if="addOpen"
            embedded
            @saved="onDeviceSaved"
            @cancel="addOpen = false"
          />
        </div>
      </div>
    </pv-dialog>

    <pv-dialog
      v-model:visible="detailOpen"
      modal
      append-to="body"
      :draggable="false"
      :closable="false"
      :show-header="false"
      dismissable-mask
      :style="{ width: 'min(94vw, 780px)' }"
      :pt="dialogPt"
    >
      <div class="mt-dialog-shell est-inspect-shell">
        <button
          type="button"
          class="mt-dialog-close"
          :aria-label="t('common.cancel')"
          @click="detailOpen = false"
        >
          <i class="pi pi-times" aria-hidden="true"></i>
        </button>
        <template v-if="liveDetailDevice">
          <header class="est-inspect-header">
            <h2 class="est-flow-title">
              {{ liveDetailDevice.exact_location || `Device #${liveDetailDevice.id}` }}
            </h2>
            <p class="est-flow-subtitle">{{ t('monitoring.deviceInfo') }}</p>
          </header>
          <div class="est-inspect-scroll">
            <div class="est-flow-fields est-flow-fields--span">
              <div class="est-flow-field">
                <span class="est-flow-field__label">{{ t('monitoring.fieldId') }}</span>
                <span class="est-flow-field__value">{{ liveDetailDevice.id }}</span>
              </div>
              <div class="est-flow-field">
                <span class="est-flow-field__label">{{ t('monitoring.fieldMedication') }}</span>
                <span class="est-flow-field__value">{{ detailMedType }}</span>
              </div>
              <div class="est-flow-field est-flow-field--full">
                <span class="est-flow-field__label">{{ t('monitoring.fieldExactLocation') }}</span>
                <span class="est-flow-field__value">{{ liveDetailDevice.exact_location || '—' }}</span>
              </div>
            </div>
            <h3 class="est-flow-section-title">{{ t('monitoring.sensorReadings') }}</h3>
            <p class="est-flow-live-hint">{{ t('monitoring.sensorLiveHint') }}</p>
            <SensorReadingsGrid :device="liveDetailDevice" />
          </div>
          <footer class="est-inspect-footer">
            <button
              type="button"
              class="est-flow-btn est-flow-btn--danger"
              @click="askDelete(liveDetailDevice)"
            >
              <i class="pi pi-trash" aria-hidden="true"></i>
              <span>{{ t('monitoring.deleteDevice') }}</span>
            </button>
          </footer>
        </template>
      </div>
    </pv-dialog>

    <MtConfirmDialog
      v-model:visible="deleteDialogVisible"
      :title="t('monitoring.deleteConfirmTitle')"
      :message="t('monitoring.deleteConfirmMessage')"
      :meta="deleteTarget ? (deleteTarget.exact_location || `ID ${deleteTarget.id}`) : ''"
      :confirm-label="t('monitoring.deleteDevice')"
      :cancel-label="t('common.cancel')"
      confirm-tone="danger"
      confirm-icon="pi pi-trash"
      @confirm="confirmDelete"
    />
  </div>
</template>
