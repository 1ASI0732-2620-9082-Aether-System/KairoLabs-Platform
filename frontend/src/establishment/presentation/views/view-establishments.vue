<script setup>
import { ref, onMounted, computed } from 'vue';
import { useRouter } from 'vue-router';
import { useI18n } from 'vue-i18n';
import useEstablishmentStore from '../../application/establishment.store.js';
import EstablishmentInspectFlow from '../components/establishment-inspect-flow.vue';

const establishmentStore = useEstablishmentStore();
const router = useRouter();
const { t } = useI18n();

const establishments = ref([]);
const isLoading = ref(true);
const searchQuery = ref('');
const inspectRef = ref(null);

const loadData = async () => {
  isLoading.value = true;
  try {
    establishments.value = await establishmentStore.fetchEstablishmentsAsync();
  } catch (error) {
    console.error('Failed to load establishments', error);
  } finally {
    isLoading.value = false;
  }
};

const filteredEst = computed(() => {
  const q = searchQuery.value.trim().toLowerCase();
  if (!q) return establishments.value;
  return establishments.value.filter(
    (e) =>
      (e.establishment_name || '').toLowerCase().includes(q) ||
      (e.city_region || '').toLowerCase().includes(q) ||
      (e.district || '').toLowerCase().includes(q),
  );
});

const stats = computed(() => {
  const total = establishments.value.length;
  const hospitals = establishments.value.filter(
    (e) => e.establishment_type?.toUpperCase() === 'HOSPITAL',
  ).length;
  const warehouses = establishments.value.filter((e) =>
    ['WAREHOUSE', 'ALMACEN', 'ALMACÉN', 'FACTORY', 'PLANTA'].includes(
      e.establishment_type?.toUpperCase(),
    ),
  ).length;
  return {
    total,
    hospitals,
    warehouses,
    others: total - (hospitals + warehouses),
  };
});

function formatLocation(est) {
  const parts = [est.city_region, est.district].filter(Boolean);
  return parts.length ? parts.join(', ') : '—';
}

function formatType(type) {
  const raw = String(type || '').toUpperCase();
  if (raw === 'HOSPITAL') return t('establishment.typeHospital');
  if (raw === 'WAREHOUSE') return t('establishment.typeWarehouse');
  if (raw === 'CLINIC') return t('establishment.typeClinic');
  return type || '—';
}

onMounted(loadData);

const viewDetails = (id) => {
  inspectRef.value?.openDetail(id);
};

const goHome = () => router.push({ name: 'home-health-entity' });
</script>

<template>
  <div class="est-flow-page">
    <nav class="est-flow-back-bar" aria-label="Navegación">
      <button type="button" class="est-flow-back-btn" @click="goHome">
        <i class="pi pi-arrow-left" aria-hidden="true"></i>
        <span>{{ t('establishment.backToHome') }}</span>
      </button>
    </nav>

    <div class="est-flow-card">
      <header class="est-flow-head est-flow-head--row">
        <div class="est-flow-head__text">
          <h1 class="est-flow-title">{{ t('establishment.establishments') }}</h1>
          <p class="est-flow-subtitle">{{ t('establishment.listPageSubtitle') }}</p>
        </div>
        <div class="est-flow-stats">
          <div class="est-flow-stat">
            <span class="est-flow-stat__icon" aria-hidden="true"><i class="pi pi-list"></i></span>
            <span class="est-flow-stat__label">{{ t('establishment.statTotal') }}</span>
            <span class="est-flow-stat__value">{{ stats.total }}</span>
          </div>
          <div class="est-flow-stat est-flow-stat--navy">
            <span class="est-flow-stat__icon" aria-hidden="true"><i class="pi pi-building"></i></span>
            <span class="est-flow-stat__label">{{ t('establishment.statHospitals') }}</span>
            <span class="est-flow-stat__value">{{ stats.hospitals }}</span>
          </div>
          <div class="est-flow-stat est-flow-stat--orange">
            <span class="est-flow-stat__icon" aria-hidden="true"><i class="pi pi-box"></i></span>
            <span class="est-flow-stat__label">{{ t('establishment.statWarehouses') }}</span>
            <span class="est-flow-stat__value">{{ stats.warehouses }}</span>
          </div>
          <div v-if="stats.others > 0" class="est-flow-stat">
            <span class="est-flow-stat__icon" aria-hidden="true"><i class="pi pi-sitemap"></i></span>
            <span class="est-flow-stat__label">{{ t('establishment.statOthers') }}</span>
            <span class="est-flow-stat__value">{{ stats.others }}</span>
          </div>
        </div>
      </header>

      <div class="est-flow-toolbar">
        <div class="est-flow-search">
          <i class="pi pi-search" aria-hidden="true"></i>
          <input
            v-model="searchQuery"
            type="search"
            :placeholder="t('establishment.searchPlaceholder')"
            autocomplete="off"
          />
        </div>
      </div>

      <div v-if="isLoading" class="est-flow-state">
        <i class="pi pi-spin pi-spinner" aria-hidden="true"></i>
        <span>{{ t('establishment.loadingEstablishments') }}</span>
      </div>

      <div v-else-if="filteredEst.length === 0" class="est-flow-state">
        <i class="pi pi-filter-slash" aria-hidden="true"></i>
        <span>{{ t('establishment.emptySearch') }}</span>
      </div>

      <div v-else class="est-flow-table-wrap">
        <table class="est-flow-table">
          <thead>
            <tr>
              <th>{{ t('establishment.colName') }}</th>
              <th>{{ t('establishment.colLocation') }}</th>
              <th>{{ t('establishment.colType') }}</th>
              <th>{{ t('establishment.statusActive') }}</th>
              <th class="est-flow-table__actions">{{ t('establishment.colActions') }}</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="est in filteredEst" :key="est.id">
              <td>
                <div class="est-flow-table__name">{{ est.establishment_name }}</div>
                <div v-if="est.address" class="est-flow-table__meta">{{ est.address }}</div>
              </td>
              <td>{{ formatLocation(est) }}</td>
              <td>
                <span class="est-flow-type-tag">{{ formatType(est.establishment_type) }}</span>
              </td>
              <td>
                <span class="est-flow-status">
                  <span class="est-flow-status__dot" aria-hidden="true"></span>
                  {{ t('establishment.statusActive') }}
                </span>
              </td>
              <td class="est-flow-table__actions">
                <button
                  type="button"
                  class="est-flow-icon-btn"
                  :aria-label="t('establishment.viewDetail')"
                  @click="viewDetails(est.id)"
                >
                  <i class="pi pi-eye" aria-hidden="true"></i>
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <EstablishmentInspectFlow ref="inspectRef" />
  </div>
</template>
