<script setup>
import { ref, onMounted, onBeforeUnmount, computed, nextTick, watch } from 'vue';
import { useRouter } from 'vue-router';
import { useI18n } from 'vue-i18n';
import { fetchDashboardPayload } from '../../../shared/infrastructure/dashboard-payload.js';
import EstablishmentInspectFlow from '../components/establishment-inspect-flow.vue';

const router = useRouter();
const { t } = useI18n();

const establishments = ref([]);
const operators = ref([]);
const isLoading = ref(true);
const searchQuery = ref('');
const selectedId = ref(null);
const statusFilter = ref('all');
const typeFilter = ref('all');
const regionFilter = ref('all');
const mapContainerRef = ref(null);
const inspectRef = ref(null);

let map = null;
let markers = [];
let tileLayer = null;

const cityCoords = {
  Lima: [-12.046374, -77.042793],
  Arequipa: [-16.409047, -71.537451],
  Piura: [-5.194493, -80.632824],
  Trujillo: [-8.115989, -79.029984],
  Callao: [-12.056594, -77.128447],
  Miraflores: [-12.111062, -77.031591],
  'San Isidro': [-12.095034, -77.033318],
  'Ate Vitarte': [-12.025, -76.92],
};

function resolveCoords(est, index) {
  const lat = est.lat ?? est.latitude;
  const lng = est.lng ?? est.longitude;
  if (lat != null && lng != null && !Number.isNaN(Number(lat)) && !Number.isNaN(Number(lng))) {
    return [Number(lat), Number(lng)];
  }
  const specific = {
    'Hospital Nacional Arzobispo Loayza': [-12.0469, -77.0428],
    'Almacén Vitarte': [-12.025, -76.92],
    'Sede Miraflores': [-12.111, -77.031],
    'Sede San Isidro': [-12.095, -77.033],
    'Sede Trujillo': [-8.115, -79.029],
  };
  if (specific[est.establishment_name]) return specific[est.establishment_name];
  if (est.establishment_name?.includes('Miraflores')) return cityCoords.Miraflores;
  if (est.establishment_name?.includes('San Isidro')) return cityCoords['San Isidro'];
  if (est.establishment_name?.includes('Vitarte')) return cityCoords['Ate Vitarte'];
  return cityCoords[est.city_region] || cityCoords.Lima;
}

const loadData = async () => {
  try {
    const data = await fetchDashboardPayload();
    operators.value = data.operators ?? [];
    establishments.value = (data.establishments ?? []).map((est, index) => {
      const [lat, lng] = resolveCoords(est, index);
      return {
        ...est,
        lat,
        lng,
        status: index % 4 === 0 ? 'maintenance' : 'operational',
      };
    });
  } catch (error) {
    console.error('Error loading map data', error);
  } finally {
    isLoading.value = false;
    await nextTick();
    await initMap();
  }
};

function destroyMap() {
  markers.forEach((m) => {
    try {
      map?.removeLayer(m);
    } catch {
      /* ignore */
    }
  });
  markers = [];
  if (tileLayer && map) {
    map.removeLayer(tileLayer);
    tileLayer = null;
  }
  if (map) {
    map.remove();
    map = null;
  }
}

const filteredEstablishments = computed(() => {
  const q = searchQuery.value.trim().toLowerCase();
  return establishments.value.filter((est) => {
    const matchesSearch =
      !q ||
      (est.establishment_name || '').toLowerCase().includes(q) ||
      (est.city_region || '').toLowerCase().includes(q) ||
      (est.district || '').toLowerCase().includes(q);
    const matchesStatus = statusFilter.value === 'all' || est.status === statusFilter.value;
    const typeRaw = String(est.establishment_type || '').toUpperCase();
    const matchesType = typeFilter.value === 'all' || typeRaw === typeFilter.value;
    const matchesRegion =
      regionFilter.value === 'all' ||
      String(est.city_region || '').toLowerCase() === regionFilter.value.toLowerCase();
    return matchesSearch && matchesStatus && matchesType && matchesRegion;
  });
});

const regionOptions = computed(() => {
  const regions = [...new Set(establishments.value.map((e) => e.city_region).filter(Boolean))];
  return regions.sort((a, b) => String(a).localeCompare(String(b)));
});

const statusSelectOptions = computed(() => [
  { label: t('establishment.mapFilterAllStatus'), value: 'all', icon: 'pi pi-filter' },
  { label: t('establishment.mapStatusOperational'), value: 'operational', icon: 'pi pi-check-circle' },
  { label: t('establishment.mapStatusMaintenance'), value: 'maintenance', icon: 'pi pi-cog' },
]);

const typeSelectOptions = computed(() => [
  { label: t('establishment.mapFilterAllTypes'), value: 'all', icon: 'pi pi-list' },
  { label: t('establishment.typeHospital'), value: 'HOSPITAL', icon: 'pi pi-building' },
  { label: t('establishment.typeClinic'), value: 'CLINIC', icon: 'pi pi-home' },
  { label: t('establishment.typeWarehouse'), value: 'WAREHOUSE', icon: 'pi pi-box' },
]);

const regionSelectOptions = computed(() => [
  { label: t('establishment.mapFilterAllRegions'), value: 'all', icon: 'pi pi-globe' },
  ...regionOptions.value.map((region) => ({ label: region, value: region, icon: 'pi pi-map-marker' })),
]);

const selectPt = {
  root: { class: 'map-pv-select kl-select' },
  label: { class: 'map-pv-select__label' },
  dropdown: { class: 'map-pv-select__trigger' },
  overlay: { class: 'map-pv-select__overlay kl-select-overlay' },
  list: { class: 'map-pv-select__list' },
  option: { class: 'map-pv-select__option' },
};

function findSelectOption(options, value) {
  return options.find((o) => o.value === value) ?? null;
}

watch(filteredEstablishments, () => {
  updateMarkers();
});

async function initMap() {
  if (typeof L === 'undefined') {
    await new Promise((resolve) => setTimeout(resolve, 300));
    return initMap();
  }

  await nextTick();

  const el = mapContainerRef.value;
  if (!el) {
    await new Promise((resolve) => setTimeout(resolve, 100));
    return initMap();
  }

  if (map) {
    updateMarkers();
    map.invalidateSize();
    return;
  }

  destroyMap();

  map = L.map(el, {
    zoomControl: false,
    attributionControl: false,
  }).setView([-9.19, -75.01], 5);

  tileLayer = L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
    maxZoom: 19,
    attribution: '&copy; OpenStreetMap',
  }).addTo(map);

  updateMarkers();

  requestAnimationFrame(() => {
    map?.invalidateSize();
  });
  setTimeout(() => map?.invalidateSize(), 200);
}

const updateMarkers = () => {
  if (!map) return;

  markers.forEach((m) => map.removeLayer(m));
  markers = [];

  const blueIcon = L.divIcon({ className: 'custom-pin navy' });
  const orangeIcon = L.divIcon({ className: 'custom-pin orange' });

  establishments.value.forEach((est) => {
    if (!filteredEstablishments.value.some((f) => f.id === est.id)) return;
    const icon = est.status === 'operational' ? blueIcon : orangeIcon;
    const marker = L.marker([est.lat, est.lng], { icon }).addTo(map);
    marker.on('click', () => {
      selectEstablishment(est.id);
    });
    markers.push(marker);
  });
};

const selectedEst = computed(() =>
  establishments.value.find((e) => e.id === selectedId.value),
);

function countOperatorsForEstablishment(est) {
  if (!est?.id || !operators.value.length) return 0;
  const estId = Number(est.id);
  if (Number.isNaN(estId)) return 0;
  return operators.value.filter((op) => Number(op.establishment_id) === estId).length;
}

const selectedOperatorCount = computed(() =>
  selectedEst.value ? countOperatorsForEstablishment(selectedEst.value) : 0,
);

function personnelLabel(count) {
  if (count === 0) return t('establishment.noOperatorsAssigned');
  if (count === 1) return t('establishment.operatorsCountOne');
  return t('establishment.operatorsCount', { n: count });
}

function statusLabel(status) {
  return status === 'operational'
    ? t('establishment.mapStatusOperational')
    : t('establishment.mapStatusMaintenance');
}

function formatType(type) {
  const raw = String(type || '').toUpperCase();
  if (raw === 'HOSPITAL') return t('establishment.typeHospital');
  if (raw === 'WAREHOUSE') return t('establishment.typeWarehouse');
  if (raw === 'CLINIC') return t('establishment.typeClinic');
  return type || '—';
}

const selectEstablishment = (id) => {
  selectedId.value = id;
  const est = establishments.value.find((e) => e.id === id);
  if (est && map) {
    map.flyTo([est.lat, est.lng], 14);
  }
};

function goHome() {
  router.push({ name: 'home-health-entity' });
}

function manageSite() {
  if (!selectedEst.value) return;
  inspectRef.value?.openDetail(selectedEst.value.id);
}

onMounted(() => {
  if (document.querySelector('link[data-leaflet]')) {
    if (typeof L !== 'undefined') loadData();
    else {
      const s = document.querySelector('script[data-leaflet]');
      if (s) s.addEventListener('load', loadData);
    }
    return;
  }

  const link = document.createElement('link');
  link.rel = 'stylesheet';
  link.href = 'https://unpkg.com/leaflet@1.9.4/dist/leaflet.css';
  link.setAttribute('data-leaflet', '1');
  document.head.appendChild(link);

  const script = document.createElement('script');
  script.src = 'https://unpkg.com/leaflet@1.9.4/dist/leaflet.js';
  script.setAttribute('data-leaflet', '1');
  script.onload = loadData;
  document.head.appendChild(script);
});

onBeforeUnmount(() => {
  destroyMap();
});
</script>

<template>
  <div class="est-flow-page est-flow-page--map">
    <nav class="est-flow-back-bar" aria-label="Navegación">
      <button type="button" class="est-flow-back-btn" @click="goHome">
        <i class="pi pi-arrow-left" aria-hidden="true"></i>
        <span>{{ t('establishment.backToHome') }}</span>
      </button>
    </nav>

    <div class="est-flow-card est-flow-card--map">
      <header class="est-flow-head est-flow-head--row">
        <div class="est-flow-head__text">
          <h1 class="est-flow-title">{{ t('establishment.mapOfEstablishments') }}</h1>
          <p class="est-flow-subtitle">{{ t('establishment.mapPageSubtitle') }}</p>
        </div>
        <div class="est-flow-stats">
          <div class="est-flow-stat">
            <span class="est-flow-stat__icon" aria-hidden="true"><i class="pi pi-map-marker"></i></span>
            <span class="est-flow-stat__label">{{ t('establishment.mapSitesLabel') }}</span>
            <span class="est-flow-stat__value">{{ establishments.length }}</span>
          </div>
        </div>
      </header>

      <div v-if="isLoading" class="map-loader">
        <i class="pi pi-spin pi-spinner" aria-hidden="true"></i>
        <span>{{ t('establishment.mapLoading') }}</span>
      </div>

      <div v-else class="map-layout">
        <aside class="map-sidebar" :aria-label="t('establishment.mapOfEstablishments')">
          <div class="map-sidebar__head">
            <h2 class="map-sidebar__title">{{ t('establishment.mapListTitle') }}</h2>
            <span class="est-flow-stat">
              <span class="est-flow-stat__value" style="font-size: 0.8rem">{{
                filteredEstablishments.length
              }}</span>
            </span>
          </div>

          <div class="map-sidebar__search">
            <i class="pi pi-search" aria-hidden="true"></i>
            <input
              v-model="searchQuery"
              type="search"
              :placeholder="t('establishment.mapSearchPlaceholder')"
              autocomplete="off"
            />
          </div>

          <div class="map-sidebar__filters">
            <pv-select
              v-model="statusFilter"
              :options="statusSelectOptions"
              option-label="label"
              option-value="value"
              append-to="body"
              :aria-label="t('establishment.mapFilterStatus')"
              :pt="selectPt"
            >
              <template #value>
                <span v-if="findSelectOption(statusSelectOptions, statusFilter)" class="kl-select-item">
                  <span class="kl-select-item__icon">
                    <i :class="findSelectOption(statusSelectOptions, statusFilter).icon" aria-hidden="true"></i>
                  </span>
                  <span class="kl-select-item__text">{{ findSelectOption(statusSelectOptions, statusFilter).label }}</span>
                </span>
              </template>
              <template #option="{ option }">
                <span class="kl-select-item">
                  <span class="kl-select-item__icon"><i :class="option.icon" aria-hidden="true"></i></span>
                  <span class="kl-select-item__text">{{ option.label }}</span>
                </span>
              </template>
            </pv-select>
            <pv-select
              v-model="typeFilter"
              :options="typeSelectOptions"
              option-label="label"
              option-value="value"
              append-to="body"
              :aria-label="t('establishment.mapFilterType')"
              :pt="selectPt"
            >
              <template #value>
                <span v-if="findSelectOption(typeSelectOptions, typeFilter)" class="kl-select-item">
                  <span class="kl-select-item__icon">
                    <i :class="findSelectOption(typeSelectOptions, typeFilter).icon" aria-hidden="true"></i>
                  </span>
                  <span class="kl-select-item__text">{{ findSelectOption(typeSelectOptions, typeFilter).label }}</span>
                </span>
              </template>
              <template #option="{ option }">
                <span class="kl-select-item">
                  <span class="kl-select-item__icon"><i :class="option.icon" aria-hidden="true"></i></span>
                  <span class="kl-select-item__text">{{ option.label }}</span>
                </span>
              </template>
            </pv-select>
            <pv-select
              v-model="regionFilter"
              :options="regionSelectOptions"
              option-label="label"
              option-value="value"
              append-to="body"
              :aria-label="t('establishment.mapFilterRegion')"
              :pt="selectPt"
            >
              <template #value>
                <span v-if="findSelectOption(regionSelectOptions, regionFilter)" class="kl-select-item">
                  <span class="kl-select-item__icon">
                    <i :class="findSelectOption(regionSelectOptions, regionFilter).icon" aria-hidden="true"></i>
                  </span>
                  <span class="kl-select-item__text">{{ findSelectOption(regionSelectOptions, regionFilter).label }}</span>
                </span>
              </template>
              <template #option="{ option }">
                <span class="kl-select-item">
                  <span class="kl-select-item__icon"><i :class="option.icon" aria-hidden="true"></i></span>
                  <span class="kl-select-item__text">{{ option.label }}</span>
                </span>
              </template>
            </pv-select>
          </div>

          <div class="map-sidebar__list">
            <div
              v-for="est in filteredEstablishments"
              :key="est.id"
              class="map-sidebar__item"
              :class="{ 'map-sidebar__item--active': selectedId === est.id }"
              role="button"
              tabindex="0"
              @click="selectEstablishment(est.id)"
              @keydown.enter.prevent="selectEstablishment(est.id)"
            >
              <span
                class="map-sidebar__dot"
                :class="est.status === 'operational' ? 'map-sidebar__dot--ok' : 'map-sidebar__dot--warn'"
                aria-hidden="true"
              ></span>
              <div class="map-sidebar__item-body">
                <span class="map-sidebar__item-name">{{ est.establishment_name }}</span>
                <span class="map-sidebar__item-region">
                  <i class="pi pi-map-marker" aria-hidden="true"></i>
                  {{ est.city_region }}
                </span>
                <span class="map-sidebar__item-type">{{ formatType(est.establishment_type) }}</span>
              </div>
            </div>
          </div>
        </aside>

        <main class="map-canvas">
          <div ref="mapContainerRef" class="map-canvas__el"></div>

          <transition name="slide-up">
            <div v-if="selectedEst" class="map-detail">
              <div class="map-detail__top">
                <span class="map-detail__tag">{{ formatType(selectedEst.establishment_type) }}</span>
                <button
                  type="button"
                  class="map-detail__close"
                  :aria-label="t('establishment.back')"
                  @click="selectedId = null"
                >
                  <i class="pi pi-times" aria-hidden="true"></i>
                </button>
              </div>
              <h3 class="map-detail__name">{{ selectedEst.establishment_name }}</h3>
              <p class="map-detail__address">
                <i class="pi pi-map-marker" aria-hidden="true"></i>
                <span>{{ selectedEst.address }}, {{ selectedEst.district }}</span>
              </p>
              <div class="map-detail__meta">
                <div class="map-detail__meta-card">
                  <span class="map-detail__stat-label">{{ t('establishment.mapStateLabel') }}</span>
                  <span
                    class="map-detail__pill"
                    :class="
                      selectedEst.status === 'operational'
                        ? 'map-detail__pill--ok'
                        : 'map-detail__pill--warn'
                    "
                  >
                    <i
                      :class="selectedEst.status === 'operational' ? 'pi pi-check-circle' : 'pi pi-exclamation-circle'"
                      aria-hidden="true"
                    ></i>
                    {{ statusLabel(selectedEst.status) }}
                  </span>
                </div>
                <div class="map-detail__meta-card">
                  <span class="map-detail__stat-label">{{ t('establishment.mapStaffLabel') }}</span>
                  <span
                    class="map-detail__staff"
                    :class="{ 'map-detail__staff--empty': selectedOperatorCount === 0 }"
                  >
                    <i class="pi pi-users" aria-hidden="true"></i>
                    {{ personnelLabel(selectedOperatorCount) }}
                  </span>
                </div>
              </div>
              <button type="button" class="est-flow-btn est-flow-btn--accent est-flow-btn--block" @click="manageSite">
                <i class="pi pi-eye" aria-hidden="true"></i>
                <span>{{ t('establishment.mapManageSite') }}</span>
              </button>
            </div>
          </transition>

          <div class="map-tools">
            <button type="button" :aria-label="t('establishment.mapZoomIn')" @click="map?.zoomIn()">
              <i class="pi pi-plus" aria-hidden="true"></i>
            </button>
            <button type="button" :aria-label="t('establishment.mapZoomOut')" @click="map?.zoomOut()">
              <i class="pi pi-minus" aria-hidden="true"></i>
            </button>
            <button
              type="button"
              class="map-tools__primary"
              :aria-label="t('establishment.mapCenterLima')"
              @click="map?.setView([-12.046374, -77.042793], 13)"
            >
              <i class="pi pi-compass" aria-hidden="true"></i>
            </button>
          </div>
        </main>
      </div>
    </div>

    <EstablishmentInspectFlow ref="inspectRef" />
  </div>
</template>

<style>
.custom-pin {
  width: 18px;
  height: 18px;
  border-radius: 50% 50% 50% 0;
  transform: rotate(-45deg);
  margin-left: -9px;
  margin-top: -9px;
  border: 2px solid white;
  box-shadow: 0 3px 8px rgba(17, 36, 51, 0.28);
}

.custom-pin::after {
  content: '';
  width: 6px;
  height: 6px;
  margin: 3px 0 0 3px;
  background: white;
  position: absolute;
  border-radius: 50%;
}

.custom-pin.navy,
.custom-pin.blue {
  background: #112433;
}

.custom-pin.orange {
  background: #f37021;
}

.leaflet-div-icon {
  background: transparent;
  border: none;
}
</style>

<style scoped>
.est-flow-page--map {
  max-width: 1360px;
}

.est-flow-card--map {
  padding: 1.35rem 1.35rem 1.25rem;
}

.est-flow-btn--block {
  width: 100%;
}

.slide-up-enter-active,
.slide-up-leave-active {
  transition: all 0.35s cubic-bezier(0.16, 1, 0.3, 1);
}

.slide-up-enter-from,
.slide-up-leave-to {
  opacity: 0;
  transform: translateY(16px);
}

.map-layout {
  display: flex;
  min-height: min(78vh, 760px);
  border: 1px solid rgba(17, 36, 51, 0.12);
  border-radius: 18px;
  overflow: hidden;
  background: #fff;
}

.map-sidebar {
  width: min(100%, 320px);
  flex-shrink: 0;
  display: flex;
  flex-direction: column;
  border-right: 1px solid rgba(17, 36, 51, 0.1);
  background: linear-gradient(180deg, #f7f8fa 0%, #f3f5f7 100%);
}

.map-sidebar__head {
  padding: 1rem 1rem 0.85rem;
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 0.5rem;
  border-bottom: 1px solid rgba(17, 36, 51, 0.08);
}

.map-sidebar__title {
  margin: 0;
  font-size: 0.78rem;
  font-weight: 800;
  letter-spacing: 0.08em;
  text-transform: uppercase;
  color: #112433;
}

.map-sidebar__head .est-flow-stat {
  background: #112433;
  border-color: #112433;
}

.map-sidebar__head .est-flow-stat__value {
  color: #fff;
}

.map-sidebar__search {
  padding: 0.85rem 1rem 0.55rem;
  position: relative;
}

.map-sidebar__search i {
  position: absolute;
  left: 1.65rem;
  top: 50%;
  transform: translateY(-50%);
  color: #94a3b8;
  font-size: 0.85rem;
  pointer-events: none;
}

.map-sidebar__search input {
  width: 100%;
  box-sizing: border-box;
  padding: 0.6rem 0.75rem 0.6rem 2.25rem;
  border: 1px solid rgba(17, 36, 51, 0.12);
  border-radius: 12px;
  font-size: 0.8125rem;
  font-family: inherit;
  color: #112433;
  background: #fff;
}

.map-sidebar__search input:focus {
  outline: none;
  border-color: #f37021;
  box-shadow: 0 0 0 3px rgba(243, 112, 33, 0.15);
}

.map-sidebar__filters {
  display: grid;
  grid-template-columns: 1fr;
  gap: 0.5rem;
  padding: 0 1rem 0.85rem;
}

.map-sidebar__filters :deep(.map-pv-select),
.map-sidebar__filters :deep(.p-select) {
  width: 100%;
  border: 1.5px solid rgba(17, 36, 51, 0.2) !important;
  border-radius: 12px !important;
  background: #fff !important;
  min-height: 2.45rem;
  box-shadow: 0 1px 2px rgba(17, 36, 51, 0.04) !important;
  font-family: inherit;
}

.map-sidebar__filters :deep(.p-select:not(.p-disabled):hover),
.map-sidebar__filters :deep(.map-pv-select:hover) {
  border-color: rgba(243, 112, 33, 0.45) !important;
}

.map-sidebar__filters :deep(.p-select.p-focus),
.map-sidebar__filters :deep(.map-pv-select.p-focus) {
  border-color: #f37021 !important;
  box-shadow: 0 0 0 3px rgba(243, 112, 33, 0.14) !important;
}

.map-sidebar__filters :deep(.p-select-label),
.map-sidebar__filters :deep(.map-pv-select__label) {
  font-size: 0.78rem !important;
  font-weight: 600 !important;
  color: #112433 !important;
  padding: 0.55rem 0.75rem !important;
}

.map-sidebar__filters :deep(.p-select-dropdown) {
  width: 2.2rem !important;
  color: #64748b !important;
}

.map-sidebar__list {
  flex: 1;
  overflow-y: auto;
  padding: 0.35rem 0.55rem 0.85rem;
  scrollbar-width: thin;
  scrollbar-color: rgba(17, 36, 51, 0.25) transparent;
}

.map-sidebar__list::-webkit-scrollbar {
  width: 6px;
}

.map-sidebar__list::-webkit-scrollbar-thumb {
  background: rgba(17, 36, 51, 0.22);
  border-radius: 999px;
}

.map-sidebar__item {
  display: flex;
  align-items: flex-start;
  gap: 0.65rem;
  padding: 0.75rem 0.7rem;
  border-radius: 12px;
  cursor: pointer;
  border: 1px solid transparent;
  transition: background 0.15s ease, border-color 0.15s ease, transform 0.15s ease;
}

.map-sidebar__item:hover {
  background: #fff;
  border-color: rgba(17, 36, 51, 0.1);
}

.map-sidebar__item--active {
  background: #fff;
  border-color: rgba(243, 112, 33, 0.45);
  box-shadow: 0 4px 14px rgba(17, 36, 51, 0.06);
}

.map-sidebar__dot {
  width: 9px;
  height: 9px;
  border-radius: 50%;
  margin-top: 0.4rem;
  flex-shrink: 0;
  box-shadow: 0 0 0 3px rgba(16, 185, 129, 0.12);
}

.map-sidebar__dot--ok {
  background: #10b981;
}

.map-sidebar__dot--warn {
  background: #f37021;
  box-shadow: 0 0 0 3px rgba(243, 112, 33, 0.16);
}

.map-sidebar__item-body {
  min-width: 0;
  display: flex;
  flex-direction: column;
  gap: 0.18rem;
}

.map-sidebar__item-name {
  display: block;
  font-size: 0.84rem;
  font-weight: 700;
  color: #112433;
  line-height: 1.3;
}

.map-sidebar__item-region {
  display: inline-flex;
  align-items: center;
  gap: 0.28rem;
  font-size: 0.72rem;
  color: #64748b;
}

.map-sidebar__item-region i {
  color: #f37021;
  font-size: 0.65rem;
}

.map-sidebar__item-type {
  display: inline-flex;
  align-self: flex-start;
  margin-top: 0.15rem;
  padding: 0.12rem 0.4rem;
  border-radius: 999px;
  background: rgba(17, 36, 51, 0.06);
  color: #415a77;
  font-size: 0.6rem;
  font-weight: 700;
  letter-spacing: 0.04em;
  text-transform: uppercase;
}

.map-canvas {
  flex: 1;
  position: relative;
  min-width: 0;
  min-height: 560px;
  align-self: stretch;
}

.map-canvas__el {
  position: absolute;
  inset: 0;
  width: 100%;
  height: 100%;
  min-height: 560px;
  z-index: 1;
  background: #e7edf2;
}

.map-canvas__el.leaflet-container {
  width: 100% !important;
  height: 100% !important;
  font-family: inherit;
}

.map-detail {
  position: absolute;
  top: 1rem;
  right: 1rem;
  width: min(100% - 2rem, 310px);
  background: #fff;
  border: 1px solid rgba(17, 36, 51, 0.1);
  border-radius: 16px;
  padding: 1.1rem 1.15rem;
  box-shadow: 0 16px 36px rgba(17, 36, 51, 0.14);
  z-index: 1000;
}

.map-detail__top {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 0.5rem;
  margin-bottom: 0.65rem;
}

.map-detail__tag {
  font-size: 0.62rem;
  font-weight: 700;
  letter-spacing: 0.05em;
  text-transform: uppercase;
  padding: 0.25rem 0.55rem;
  border-radius: 999px;
  background: rgba(243, 112, 33, 0.12);
  color: #e05a12;
}

.map-detail__close {
  border: 1px solid rgba(17, 36, 51, 0.1);
  background: #fff;
  color: #64748b;
  font-size: 0.85rem;
  line-height: 1;
  cursor: pointer;
  width: 1.85rem;
  height: 1.85rem;
  border-radius: 8px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
}

.map-detail__close:hover {
  background: #112433;
  border-color: #112433;
  color: #fff;
}

.map-detail__name {
  margin: 0 0 0.4rem;
  font-size: 1rem;
  font-weight: 800;
  color: #112433;
  line-height: 1.3;
}

.map-detail__address {
  margin: 0 0 0.85rem;
  font-size: 0.78rem;
  color: #64748b;
  display: flex;
  align-items: flex-start;
  gap: 0.35rem;
  line-height: 1.4;
}

.map-detail__address i {
  color: #f37021;
  margin-top: 0.1rem;
}

.map-detail__stats {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 0.65rem;
  margin-bottom: 0.9rem;
}

.map-detail__meta {
  display: grid;
  grid-template-columns: 1fr;
  gap: 0.55rem;
  margin-bottom: 0.95rem;
}

.map-detail__meta-card {
  padding: 0.7rem 0.75rem;
  border-radius: 12px;
  border: 1px solid rgba(17, 36, 51, 0.08);
  background: #f8fafc;
}

.map-detail__stat-label {
  display: block;
  font-size: 0.58rem;
  font-weight: 700;
  letter-spacing: 0.06em;
  text-transform: uppercase;
  color: #64748b;
  margin-bottom: 0.35rem;
}

.map-detail__stat-value {
  font-size: 0.8125rem;
  font-weight: 700;
  color: #112433;
}

.map-detail__stat-value--ok { color: #059669; }
.map-detail__stat-value--warn { color: #f37021; }
.map-detail__stat-value--muted { color: #64748b; font-weight: 500; }

.map-detail__pill {
  display: inline-flex;
  align-items: center;
  gap: 0.35rem;
  padding: 0.28rem 0.55rem;
  border-radius: 999px;
  font-size: 0.78rem;
  font-weight: 700;
}

.map-detail__pill--ok {
  background: rgba(16, 185, 129, 0.12);
  color: #047857;
}

.map-detail__pill--warn {
  background: rgba(243, 112, 33, 0.14);
  color: #c2410c;
}

.map-detail__staff {
  display: inline-flex;
  align-items: flex-start;
  gap: 0.4rem;
  font-size: 0.82rem;
  font-weight: 600;
  color: #112433;
  line-height: 1.35;
}

.map-detail__staff i {
  color: #f37021;
  margin-top: 0.1rem;
}

.map-detail__staff--empty {
  color: #64748b;
  font-weight: 500;
}

.map-tools {
  position: absolute;
  bottom: 1rem;
  left: 1rem;
  display: flex;
  gap: 0.45rem;
  z-index: 1000;
}

.map-tools button {
  width: 2.5rem;
  height: 2.5rem;
  border: 1px solid rgba(17, 36, 51, 0.12);
  border-radius: 12px;
  background: #fff;
  color: #415a77;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  box-shadow: 0 4px 12px rgba(17, 36, 51, 0.08);
  transition: background 0.15s ease, color 0.15s ease, border-color 0.15s ease;
}

.map-tools button:hover {
  background: #112433;
  border-color: #112433;
  color: #fff;
}

.map-tools button.map-tools__primary {
  background: #f37021;
  color: #fff;
  border-color: #f37021;
}

.map-tools button.map-tools__primary:hover {
  background: #e05a12;
  border-color: #e05a12;
}

.map-loader {
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: 320px;
  color: #64748b;
  font-size: 0.875rem;
  gap: 0.65rem;
}

@media (max-width: 900px) {
  .map-layout { flex-direction: column; min-height: auto; }
  .map-sidebar { width: 100%; max-height: 260px; border-right: none; border-bottom: 1px solid rgba(17, 36, 51, 0.1); }
  .map-canvas { min-height: 400px; }
  .map-detail { left: 1rem; right: 1rem; width: auto; }
}
</style>
