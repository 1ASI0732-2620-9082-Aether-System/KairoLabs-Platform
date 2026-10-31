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
const operators = ref([]);
const devices = ref([]);
const users = ref([]);
const admins = ref([]);

function sameEst(a, b) {
  return Number(a) === Number(b) || String(a) === String(b);
}

function personnel(userId) {
  const all = [...users.value, ...admins.value];
  return all.find((u) => u.id === userId) || { name: '—', email: '—' };
}

function deviceLabel(dev) {
  const loc = dev.exact_location || dev.name;
  const med = dev.type_of_medication || dev.medication_type;
  if (loc && med) return `${loc} — ${med}`;
  return loc || med || `#${dev.id}`;
}

async function load() {
  loading.value = true;
  const eid = route.params.establishmentId;
  try {
    const data = await fetchDashboardPayload();
    establishment.value = data.establishments.find((e) => sameEst(e.id, eid)) ?? null;
    users.value = data.users || [];
    admins.value = data.admins || [];
    operators.value = (data.operators || []).filter((op) => sameEst(op.establishment_id, eid));
    devices.value = (data.devices || []).filter((d) => sameEst(d.establishment_id, eid));
  } catch (e) {
    console.error(e);
    establishment.value = null;
  } finally {
    loading.value = false;
  }
}

onMounted(load);
watch(() => route.params.establishmentId, load);

function goDetail() {
  router.push({
    name: 'establishment-detail',
    params: { establishmentId: route.params.establishmentId },
  });
}

function openOperator(op) {
  router.push({
    name: 'establishment-operator-detail',
    params: {
      establishmentId: route.params.establishmentId,
      operatorId: String(op.id),
    },
  });
}

function openDevice(dev) {
  router.push({
    name: 'establishment-device-detail',
    params: {
      establishmentId: route.params.establishmentId,
      deviceId: String(dev.id),
    },
  });
}

const pageTitle = computed(
  () => establishment.value?.establishment_name || t('establishment.teamTitle'),
);
</script>

<template>
  <div class="est-flow-page">
    <nav class="est-flow-back-bar" aria-label="Navegación">
      <button type="button" class="est-flow-back-btn" @click="goDetail">
        <i class="pi pi-arrow-left" aria-hidden="true"></i>
        <span>{{ t('establishment.backToEstablishment') }}</span>
      </button>
    </nav>

    <div v-if="loading" class="est-flow-card est-flow-state">
      <i class="pi pi-spin pi-spinner" aria-hidden="true"></i>
      <span>{{ t('establishment.detailLoading') }}</span>
    </div>

    <div v-else-if="!establishment" class="est-flow-card est-flow-state est-flow-state--warn">
      <p>{{ t('establishment.notFound') }}</p>
      <button type="button" class="est-flow-btn est-flow-btn--ghost" @click="goDetail">
        {{ t('establishment.back') }}
      </button>
    </div>

    <div v-else class="est-flow-card">
      <h1 class="est-flow-team-title">{{ pageTitle }}</h1>
      <p class="est-flow-subtitle" style="text-align: center; margin: -0.75rem 0 1.15rem">
        {{ t('establishment.teamSubtitle') }}
      </p>

      <div class="est-flow-panels">
        <section class="est-flow-panel" :aria-label="t('establishment.operatorsColumn')">
          <h2 class="est-flow-panel__head">{{ t('establishment.operatorsColumn') }}</h2>
          <ul v-if="operators.length" class="est-flow-panel__list">
            <li v-for="op in operators" :key="op.id" class="est-flow-panel__row">
              <span class="est-flow-panel__label">{{ personnel(op.users_id).name }}</span>
              <button
                type="button"
                class="est-flow-icon-btn"
                :aria-label="t('establishment.viewDetail')"
                @click="openOperator(op)"
              >
                <i class="pi pi-eye" aria-hidden="true"></i>
              </button>
            </li>
          </ul>
          <p v-else class="est-flow-panel__empty">{{ t('establishment.noOperatorsAssigned') }}</p>
        </section>

        <section class="est-flow-panel" :aria-label="t('establishment.devicesColumn')">
          <h2 class="est-flow-panel__head">{{ t('establishment.devicesColumn') }}</h2>
          <ul v-if="devices.length" class="est-flow-panel__list">
            <li v-for="dev in devices" :key="dev.id" class="est-flow-panel__row">
              <span class="est-flow-panel__label">{{ deviceLabel(dev) }}</span>
              <button
                type="button"
                class="est-flow-icon-btn"
                :aria-label="t('establishment.viewDetail')"
                @click="openDevice(dev)"
              >
                <i class="pi pi-eye" aria-hidden="true"></i>
              </button>
            </li>
          </ul>
          <p v-else class="est-flow-panel__empty">{{ t('establishment.noDevicesAtSite') }}</p>
        </section>
      </div>

      <footer class="est-flow-actions">
        <button type="button" class="est-flow-btn est-flow-btn--ghost" @click="goDetail">
          <i class="pi pi-arrow-left" aria-hidden="true"></i>
          <span>{{ t('establishment.back') }}</span>
        </button>
      </footer>
    </div>
  </div>
</template>
