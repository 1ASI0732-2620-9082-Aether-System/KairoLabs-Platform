<script setup>
import { ref, onMounted, onUnmounted, computed } from 'vue';
import { useRouter } from 'vue-router';
import { useI18n } from 'vue-i18n';
import { useToast } from 'primevue/usetoast';
import useLogisticsStore from '../../application/logistics.store.js';
import MtConfirmDialog from '../../../shared/presentation/components/mt-confirm-dialog.vue';
import TransportForm from './transport-form.vue';
import SensorReadingsGrid from '../../../monitoring/presentation/components/sensor-readings-grid.vue';

const logisticsStore = useLogisticsStore();
const router = useRouter();
const { t } = useI18n();
const toast = useToast();

const transports = ref([]);
const isLoading = ref(true);
const searchQuery = ref('');
const deleteTarget = ref(null);
const deleteDialogVisible = ref(false);

const addOpen = ref(false);
const detailOpen = ref(false);
const detailTransport = ref(null);

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
    transports.value = await logisticsStore.fetchTransportsAsync();
    logisticsStore.startSimulation(8000);
  } catch (error) {
    console.error('Error loading transports', error);
  } finally {
    isLoading.value = false;
  }
});

onUnmounted(() => { logisticsStore.stopSimulation(); });

const filteredTransports = computed(() => {
  const q = searchQuery.value.trim().toLowerCase();
  if (!q) return transports.value;
  return transports.value.filter(
    (tr) =>
      (tr.type_of_transport || '').toLowerCase().includes(q) ||
      (tr.type_of_medication || '').toLowerCase().includes(q),
  );
});

const stats = computed(() => {
  const total = transports.value.length;
  const alerts = transports.value.filter(
    (tr) => Number(tr.temperature) > 8 || isDoorOpen(tr.door_status),
  ).length;
  return { total, alerts };
});

const liveDetail = computed(() => {
  if (!detailTransport.value) return null;
  return transports.value.find((tr) => tr.id === detailTransport.value.id) ?? detailTransport.value;
});

function isDoorOpen(status) {
  const s = String(status || '').toUpperCase();
  return s.includes('OPEN') || s.includes('ABIERTA');
}

function hasAlert(tr) {
  return Number(tr.temperature) > 8 || isDoorOpen(tr.door_status);
}

function formatTransportType(type) {
  const raw = String(type || '').toUpperCase().replace('_', '');
  const map = {
    VAN: 'transportVan',
    OFFROAD: 'transportOffRoad',
    MOTORCYCLE: 'transportMotorcycle',
    REFRIGERATED: 'transportRefrigerated',
    COLDCHAIN: 'transportColdChain',
  };
  if (map[raw]) return t(`logistics.${map[raw]}`);
  return type || '—';
}

function formatMedication(med) {
  const raw = String(med || '').toUpperCase();
  const map = {
    VACCINES: 'medicationVaccines',
    VACUNAS: 'medicationVaccines',
    PILLS: 'medicationPills',
    CREAMS: 'medicationCreams',
    SYRUP: 'medicationSyrup',
    INSULINA: 'medicationInsulin',
    INSULIN: 'medicationInsulin',
    REFRIGERATED: 'medicationVaccines',
    BIOLOGICAL: 'medicationPills',
    CONTROLLED: 'medicationCreams',
    GENERAL: 'medicationSyrup',
  };
  for (const [key, localeKey] of Object.entries(map)) {
    if (raw.includes(key)) return t(`logistics.${localeKey}`);
  }
  return med || '—';
}

function formatDoor(status) {
  if (isDoorOpen(status)) return t('logistics.doorOpen');
  return t('logistics.doorClosed');
}

function transportPlate(tr) {
  if (!tr?.id) return '—';
  const prefix = String(tr.type_of_transport || 'TR')
    .replace(/([a-z])([A-Z])/g, '$1_$2')
    .split(/[_\s-]+/)[0]
    .slice(0, 3)
    .toUpperCase() || 'TR';
  return `${prefix}-${String(tr.id).padStart(4, '0')}`;
}

function goHome() {
  router.push({ name: 'home-operational-staff' });
}

function openAdd() {
  addOpen.value = true;
}

async function onTransportSaved(created) {
  addOpen.value = false;
  transports.value = await logisticsStore.fetchTransportsAsync();
  if (created) {
    detailTransport.value = created;
    detailOpen.value = true;
  }
}

function viewTransport(tr) {
  detailTransport.value = tr;
  detailOpen.value = true;
}

function askDelete(tr) {
  deleteTarget.value = tr;
  deleteDialogVisible.value = true;
}

async function confirmDelete() {
  if (!deleteTarget.value) return;
  try {
    await logisticsStore.deleteTransportAsync(deleteTarget.value);
    transports.value = transports.value.filter((tr) => tr.id !== deleteTarget.value.id);
    if (detailTransport.value?.id === deleteTarget.value.id) {
      detailOpen.value = false;
      detailTransport.value = null;
    }
    toast.add({ severity: 'success', summary: t('logistics.deleteSuccess'), life: 3000 });
  } catch {
    toast.add({ severity: 'error', summary: t('logistics.deleteError'), life: 4000 });
  } finally {
    deleteTarget.value = null;
  }
}
</script>

<template>
  <div class="est-flow-page">
    <nav class="est-flow-back-bar" aria-label="Navegación">
      <button type="button" class="est-flow-back-btn" @click="goHome">
        <i class="pi pi-arrow-left" aria-hidden="true"></i>
        <span>{{ t('logistics.backToStaffHome') }}</span>
      </button>
    </nav>

    <div class="est-flow-card">
      <header class="est-flow-head est-flow-head--row">
        <div class="est-flow-head__text">
          <h1 class="est-flow-title">{{ t('logistics.transports') }}</h1>
          <p class="est-flow-subtitle">{{ t('logistics.listPageSubtitle') }}</p>
        </div>
        <div class="est-flow-stats">
          <div class="est-flow-stat est-flow-stat--navy">
            <span class="est-flow-stat__icon" aria-hidden="true"><i class="pi pi-truck"></i></span>
            <span class="est-flow-stat__label">{{ t('logistics.statFleet') }}</span>
            <span class="est-flow-stat__value">{{ stats.total }}</span>
          </div>
          <div
            class="est-flow-stat"
            :class="stats.alerts > 0 ? 'est-flow-stat--warn' : 'est-flow-stat--ok'"
          >
            <span class="est-flow-stat__icon" aria-hidden="true">
              <i :class="stats.alerts > 0 ? 'pi pi-exclamation-triangle' : 'pi pi-check-circle'"></i>
            </span>
            <span class="est-flow-stat__label">{{ t('logistics.statAlerts') }}</span>
            <span class="est-flow-stat__value">{{ stats.alerts }}</span>
          </div>
        </div>
      </header>

      <div class="mon-toolbar-actions">
        <button type="button" class="est-flow-btn est-flow-btn--accent" @click="openAdd">
          <i class="pi pi-plus" aria-hidden="true"></i>
          <span>{{ t('logistics.addTransport') }}</span>
        </button>
      </div>

      <div class="est-flow-toolbar">
        <div class="est-flow-search">
          <i class="pi pi-search" aria-hidden="true"></i>
          <input
            v-model="searchQuery"
            type="search"
            :placeholder="t('logistics.searchPlaceholder')"
            autocomplete="off"
          />
        </div>
      </div>

      <div v-if="isLoading" class="est-flow-state">
        <i class="pi pi-spin pi-spinner" aria-hidden="true"></i>
        <span>{{ t('logistics.loadingTransports') }}</span>
      </div>

      <div v-else-if="filteredTransports.length === 0" class="est-flow-state">
        <i class="pi pi-filter-slash" aria-hidden="true"></i>
        <span>{{ t('logistics.emptySearch') }}</span>
      </div>

      <div v-else class="est-flow-table-wrap">
        <table class="est-flow-table">
          <thead>
            <tr>
              <th>{{ t('logistics.colTransport') }}</th>
              <th>{{ t('logistics.colMedication') }}</th>
              <th>{{ t('logistics.colTemp') }}</th>
              <th>{{ t('logistics.colHumidity') }}</th>
              <th>{{ t('logistics.colDoor') }}</th>
              <th class="est-flow-table__actions">{{ t('logistics.colActions') }}</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="tr in filteredTransports" :key="tr.id">
              <td>
                <div class="est-flow-table__name">
                  <i
                    v-if="hasAlert(tr)"
                    class="pi pi-exclamation-triangle"
                    style="color: #d97706; margin-right: 0.35rem; font-size: 0.85rem"
                    aria-hidden="true"
                  ></i>
                  {{ formatTransportType(tr.type_of_transport) }}
                </div>
                <div class="est-flow-table__meta">#{{ tr.id }}</div>
              </td>
              <td>{{ formatMedication(tr.type_of_medication) }}</td>
              <td>
                <span
                  :style="{
                    color: hasAlert(tr) && Number(tr.temperature) > 8 ? '#dc2626' : '#059669',
                    fontWeight: 600,
                  }"
                >
                  {{ tr.temperature != null ? `${tr.temperature}°C` : '—' }}
                </span>
              </td>
              <td>{{ tr.humidity != null ? `${tr.humidity}%` : '—' }}</td>
              <td>
                <span
                  :style="{
                    color: isDoorOpen(tr.door_status) ? '#dc2626' : '#059669',
                    fontWeight: 600,
                    fontSize: '0.78rem',
                  }"
                >
                  {{ formatDoor(tr.door_status) }}
                </span>
              </td>
              <td class="est-flow-table__actions">
                <div class="est-flow-table__actions-row">
                  <button
                    type="button"
                    class="est-flow-icon-btn"
                    :aria-label="t('logistics.viewDetail')"
                    @click="viewTransport(tr)"
                  >
                    <i class="pi pi-eye" aria-hidden="true"></i>
                  </button>
                  <button
                    type="button"
                    class="est-flow-icon-btn est-flow-icon-btn--danger"
                    :aria-label="t('logistics.deleteTransport')"
                    @click="askDelete(tr)"
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
          <h2 class="est-flow-title">{{ t('logistics.addTransportTitle') }}</h2>
          <p class="est-flow-subtitle">{{ t('logistics.addTransportSubtitle') }}</p>
        </header>
        <div class="est-inspect-scroll">
          <TransportForm
            v-if="addOpen"
            embedded
            @saved="onTransportSaved"
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
        <template v-if="liveDetail">
          <header class="est-inspect-header">
            <p class="est-flow-eyebrow">{{ t('logistics.fieldPlate') }}</p>
            <h2 class="est-flow-title">{{ transportPlate(liveDetail) }}</h2>
            <p class="est-flow-subtitle">
              {{ formatTransportType(liveDetail.type_of_transport) }} · {{ t('logistics.transportInfo') }}
            </p>
          </header>
          <div class="est-inspect-scroll">
            <div class="est-flow-fields est-flow-fields--span">
              <div class="est-flow-field">
                <span class="est-flow-field__label">{{ t('logistics.fieldPlate') }}</span>
                <span class="est-flow-field__value">{{ transportPlate(liveDetail) }}</span>
              </div>
              <div class="est-flow-field">
                <span class="est-flow-field__label">{{ t('logistics.fieldId') }}</span>
                <span class="est-flow-field__value">{{ liveDetail.id }}</span>
              </div>
              <div class="est-flow-field">
                <span class="est-flow-field__label">{{ t('logistics.fieldMedication') }}</span>
                <span class="est-flow-field__value">{{ formatMedication(liveDetail.type_of_medication) }}</span>
              </div>
              <div class="est-flow-field est-flow-field--full">
                <span class="est-flow-field__label">{{ t('logistics.fieldTransportType') }}</span>
                <span class="est-flow-field__value">{{ formatTransportType(liveDetail.type_of_transport) }}</span>
              </div>
            </div>
            <h3 class="est-flow-section-title">{{ t('logistics.sensorReadings') }}</h3>
            <p class="est-flow-live-hint">{{ t('logistics.sensorLiveHint') }}</p>
            <SensorReadingsGrid :device="liveDetail" label-prefix="logistics" />
          </div>
          <footer class="est-inspect-footer">
            <button
              type="button"
              class="est-flow-btn est-flow-btn--danger"
              @click="askDelete(liveDetail)"
            >
              <i class="pi pi-trash" aria-hidden="true"></i>
              <span>{{ t('logistics.deleteTransport') }}</span>
            </button>
          </footer>
        </template>
      </div>
    </pv-dialog>

    <MtConfirmDialog
      v-model:visible="deleteDialogVisible"
      :title="t('logistics.deleteConfirmTitle')"
      :message="t('logistics.deleteConfirmMessage')"
      :meta="deleteTarget ? transportPlate(deleteTarget) : ''"
      :confirm-label="t('logistics.deleteTransport')"
      :cancel-label="t('common.cancel')"
      confirm-tone="danger"
      confirm-icon="pi pi-trash"
      @confirm="confirmDelete"
    />
  </div>
</template>
