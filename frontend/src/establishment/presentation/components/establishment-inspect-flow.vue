<script setup>
import { computed, ref, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import { fetchDashboardPayload } from '../../../shared/infrastructure/dashboard-payload.js';
import SensorReadingsGrid from '../../../monitoring/presentation/components/sensor-readings-grid.vue';

const props = defineProps({
  establishmentId: {
    type: [String, Number],
    default: null,
  },
});

const emit = defineEmits(['close']);

const { t } = useI18n();

const detailOpen = ref(false);
const teamOpen = ref(false);
const operatorOpen = ref(false);
const deviceOpen = ref(false);

const loading = ref(false);
const payload = ref(null);
const activeEstId = ref(null);
const activeOperatorId = ref(null);
const activeDeviceId = ref(null);

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
  transition: {
    enterFromClass: 'est-inspect-enter-from',
    enterActiveClass: 'est-inspect-enter-active',
    leaveFromClass: 'est-inspect-leave-from',
    leaveActiveClass: 'est-inspect-leave-active',
    leaveToClass: 'est-inspect-leave-to',
  },
};

function sameId(a, b) {
  return Number(a) === Number(b) || String(a) === String(b);
}

async function ensurePayload() {
  if (payload.value) return payload.value;
  loading.value = true;
  try {
    payload.value = await fetchDashboardPayload();
  } finally {
    loading.value = false;
  }
  return payload.value;
}

async function openDetail(establishmentId) {
  activeEstId.value = establishmentId;
  activeOperatorId.value = null;
  activeDeviceId.value = null;
  teamOpen.value = false;
  operatorOpen.value = false;
  deviceOpen.value = false;
  await ensurePayload();
  detailOpen.value = true;
}

watch(
  () => props.establishmentId,
  (id) => {
    if (id != null && id !== '') openDetail(id);
  },
  { immediate: true },
);

defineExpose({ openDetail });

const establishment = computed(() => {
  const data = payload.value;
  if (!data || activeEstId.value == null) return null;
  return data.establishments.find((e) => sameId(e.id, activeEstId.value)) ?? null;
});

const operatorsCount = computed(() => {
  if (!payload.value || activeEstId.value == null) return 0;
  return (payload.value.operators || []).filter((op) => sameId(op.establishment_id, activeEstId.value)).length;
});

const devicesCount = computed(() => {
  if (!payload.value || activeEstId.value == null) return 0;
  return (payload.value.devices || []).filter((d) => sameId(d.establishment_id, activeEstId.value)).length;
});

const teamOperators = computed(() => {
  if (!payload.value || activeEstId.value == null) return [];
  return (payload.value.operators || []).filter((op) => sameId(op.establishment_id, activeEstId.value));
});

const teamDevices = computed(() => {
  if (!payload.value || activeEstId.value == null) return [];
  return (payload.value.devices || []).filter((d) => sameId(d.establishment_id, activeEstId.value));
});

const allPeople = computed(() => {
  const data = payload.value;
  if (!data) return [];
  return [...(data.users || []), ...(data.admins || [])];
});

function personnel(userId) {
  return allPeople.value.find((u) => sameId(u.id, userId)) || { name: '—', email: '—' };
}

function deviceLabel(dev) {
  const loc = dev.exact_location || dev.name;
  const med = dev.type_of_medication || dev.medication_type;
  if (loc && med) return `${loc} — ${med}`;
  return loc || med || `#${dev.id}`;
}

const displayType = computed(() => {
  const raw = establishment.value?.establishment_type;
  if (!raw) return '—';
  const upper = String(raw).toUpperCase();
  if (upper === 'HOSPITAL') return t('establishment.typeHospital');
  if (upper === 'WAREHOUSE') return t('establishment.typeWarehouse');
  if (upper === 'CLINIC') return t('establishment.typeClinic');
  return String(raw).replace(/_/g, ' ');
});

const activeOperator = computed(() => {
  if (!payload.value || activeOperatorId.value == null) return null;
  return (
    (payload.value.operators || []).find(
      (o) => sameId(o.id, activeOperatorId.value) && sameId(o.establishment_id, activeEstId.value),
    ) ?? null
  );
});

const activeUser = computed(() => {
  if (!activeOperator.value) return null;
  return allPeople.value.find((u) => sameId(u.id, activeOperator.value.users_id)) ?? null;
});

const activeDevice = computed(() => {
  if (!payload.value || activeDeviceId.value == null) return null;
  return (
    (payload.value.devices || []).find(
      (d) => sameId(d.id, activeDeviceId.value) && sameId(d.establishment_id, activeEstId.value),
    ) ?? null
  );
});

const scheduleRows = computed(() => {
  const sch = activeOperator.value?.schedule;
  if (!sch) {
    return [
      { turn: t('establishment.scheduleMorning'), day: '—' },
      { turn: t('establishment.scheduleAfternoon'), day: '—' },
      { turn: t('establishment.scheduleNight'), day: '—' },
    ];
  }
  if (typeof sch === 'string') {
    return [{ turn: t('establishment.scheduleShift'), day: sch }];
  }
  if (typeof sch === 'object' && !Array.isArray(sch)) {
    return Object.entries(sch).map(([turn, day]) => ({ turn, day: String(day) }));
  }
  if (Array.isArray(sch)) {
    return sch.map((row) => ({
      turn: row.turn || row.shift || '—',
      day: row.day || row.hours || row.range || '—',
    }));
  }
  return [{ turn: '—', day: String(sch) }];
});

const entryDate = computed(() => {
  const op = activeOperator.value;
  const u = activeUser.value;
  if (!op && !u) return '—';
  const raw = op?.entry_date ?? u?.entry_date ?? op?.createdAt ?? op?.created_at;
  if (!raw) return '—';
  const d = new Date(raw);
  return Number.isNaN(d.getTime()) ? String(raw) : d.toLocaleDateString();
});

const heroName = computed(() => activeUser.value?.name || t('establishment.unknownUser'));
const heroEmail = computed(() => activeUser.value?.email || '—');
const userInitial = computed(() => {
  const n = activeUser.value?.name;
  return n && n.length ? n.charAt(0).toUpperCase() : '?';
});
const photoUrl = computed(
  () => activeUser.value?.photo || activeUser.value?.photo_url || activeUser.value?.avatar || null,
);

const medType = computed(() => {
  const d = activeDevice.value;
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

function closeDetail() {
  detailOpen.value = false;
  teamOpen.value = false;
  operatorOpen.value = false;
  deviceOpen.value = false;
  emit('close');
}

function openTeam() {
  operatorOpen.value = false;
  deviceOpen.value = false;
  teamOpen.value = true;
}

function openOperator(op) {
  activeOperatorId.value = op.id;
  operatorOpen.value = true;
}

function openDevice(dev) {
  activeDeviceId.value = dev.id;
  deviceOpen.value = true;
}
</script>

<template>
  <!-- Detail -->
  <pv-dialog
    v-model:visible="detailOpen"
    modal
    append-to="body"
    :draggable="false"
    :closable="false"
    :show-header="false"
    dismissable-mask
    :style="{ width: 'min(92vw, 720px)' }"
    :pt="dialogPt"
    @hide="closeDetail"
  >
    <div class="mt-dialog-shell est-inspect-shell">
      <button
        type="button"
        class="mt-dialog-close"
        :aria-label="t('plansPage.closeModal')"
        @click="closeDetail"
      >
        <i class="pi pi-times" aria-hidden="true"></i>
      </button>

      <div v-if="loading" class="est-inspect-body est-flow-state">
        <i class="pi pi-spin pi-spinner" aria-hidden="true"></i>
        <span>{{ t('establishment.detailLoading') }}</span>
      </div>

      <div v-else-if="!establishment" class="est-inspect-body est-flow-state est-flow-state--warn">
        <p>{{ t('establishment.notFound') }}</p>
      </div>

      <template v-else>
        <header class="est-inspect-header">
          <h2 class="est-flow-title">{{ establishment.establishment_name }}</h2>
          <p class="est-flow-subtitle">{{ t('establishment.establishmentInfo') }}</p>
        </header>

        <div class="est-inspect-scroll">
          <div class="est-flow-fields est-flow-fields--span">
            <div class="est-flow-field">
              <span class="est-flow-field__label"><i class="pi pi-hashtag" aria-hidden="true"></i> {{ t('establishment.fieldId') }}</span>
              <span class="est-flow-field__value">{{ establishment.id }}</span>
            </div>
            <div class="est-flow-field">
              <span class="est-flow-field__label"><i class="pi pi-building" aria-hidden="true"></i> {{ t('establishment.fieldType') }}</span>
              <span class="est-flow-field__value">{{ displayType }}</span>
            </div>
            <div class="est-flow-field est-flow-field--full">
              <span class="est-flow-field__label"><i class="pi pi-home" aria-hidden="true"></i> {{ t('establishment.fieldName') }}</span>
              <span class="est-flow-field__value">{{ establishment.establishment_name }}</span>
            </div>
            <div class="est-flow-field">
              <span class="est-flow-field__label"><i class="pi pi-map-marker" aria-hidden="true"></i> {{ t('establishment.fieldAddress') }}</span>
              <span class="est-flow-field__value">{{ establishment.address || '—' }}</span>
            </div>
            <div class="est-flow-field">
              <span class="est-flow-field__label"><i class="pi pi-compass" aria-hidden="true"></i> {{ t('establishment.fieldDistrict') }}</span>
              <span class="est-flow-field__value">{{ establishment.district || '—' }}</span>
            </div>
            <div class="est-flow-field">
              <span class="est-flow-field__label"><i class="pi pi-globe" aria-hidden="true"></i> {{ t('establishment.fieldCity') }}</span>
              <span class="est-flow-field__value">{{ establishment.city_region || '—' }}</span>
            </div>
            <div class="est-flow-field">
              <span class="est-flow-field__label"><i class="pi pi-flag" aria-hidden="true"></i> {{ t('establishment.fieldCountry') }}</span>
              <span class="est-flow-field__value">{{ establishment.country || '—' }}</span>
            </div>
            <div class="est-flow-field">
              <span class="est-flow-field__label"><i class="pi pi-phone" aria-hidden="true"></i> {{ t('establishment.fieldPhone') }}</span>
              <span class="est-flow-field__value">{{ establishment.phone || '—' }}</span>
            </div>
            <div class="est-flow-field">
              <span class="est-flow-field__label"><i class="pi pi-envelope" aria-hidden="true"></i> {{ t('establishment.fieldEmail') }}</span>
              <span class="est-flow-field__value">{{ establishment.email || '—' }}</span>
            </div>
            <div class="est-flow-field">
              <span class="est-flow-field__label"><i class="pi pi-link" aria-hidden="true"></i> {{ t('establishment.fieldWebsite') }}</span>
              <span class="est-flow-field__value">{{ establishment.website || '—' }}</span>
            </div>
            <div class="est-flow-field">
              <span class="est-flow-field__label"><i class="pi pi-users" aria-hidden="true"></i> {{ t('establishment.fieldOperatorsCount') }}</span>
              <span class="est-flow-field__value">{{ operatorsCount }}</span>
            </div>
            <div class="est-flow-field">
              <span class="est-flow-field__label"><i class="pi pi-th-large" aria-hidden="true"></i> {{ t('establishment.fieldDevicesCount') }}</span>
              <span class="est-flow-field__value">{{ devicesCount }}</span>
            </div>
          </div>
        </div>

        <footer class="est-inspect-footer">
          <p class="est-inspect-footer__hint">{{ t('establishment.establishmentInfo') }}</p>
          <div class="est-inspect-cta">
            <button type="button" class="est-flow-btn est-flow-btn--accent" @click="openTeam">
              <i class="pi pi-users" aria-hidden="true"></i>
              <span>{{ t('establishment.viewOperators') }}</span>
            </button>
          </div>
        </footer>
      </template>
    </div>
  </pv-dialog>

  <!-- Team -->
  <pv-dialog
    v-model:visible="teamOpen"
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
        :aria-label="t('plansPage.closeModal')"
        @click="teamOpen = false"
      >
        <i class="pi pi-times" aria-hidden="true"></i>
      </button>

      <header class="est-inspect-header">
        <h2 class="est-flow-team-title">{{ establishment?.establishment_name || t('establishment.teamTitle') }}</h2>
        <p class="est-flow-subtitle" style="text-align: center">
          {{ t('establishment.teamSubtitle') }}
        </p>
      </header>

      <div class="est-inspect-scroll">
        <div class="est-flow-panels">
          <section class="est-flow-panel" :aria-label="t('establishment.operatorsColumn')">
            <h3 class="est-flow-panel__head">{{ t('establishment.operatorsColumn') }}</h3>
            <ul v-if="teamOperators.length" class="est-flow-panel__list">
              <li v-for="op in teamOperators" :key="op.id" class="est-flow-panel__row">
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
            <h3 class="est-flow-panel__head">{{ t('establishment.devicesColumn') }}</h3>
            <ul v-if="teamDevices.length" class="est-flow-panel__list">
              <li v-for="dev in teamDevices" :key="dev.id" class="est-flow-panel__row">
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
      </div>

      <footer class="est-inspect-footer">
        <p class="est-inspect-footer__hint">{{ t('establishment.teamSubtitle') }}</p>
      </footer>
    </div>
  </pv-dialog>

  <!-- Operator -->
  <pv-dialog
    v-model:visible="operatorOpen"
    modal
    append-to="body"
    :draggable="false"
    :closable="false"
    :show-header="false"
    dismissable-mask
    :style="{ width: 'min(92vw, 720px)' }"
    :pt="dialogPt"
  >
    <div class="mt-dialog-shell est-inspect-shell">
      <button
        type="button"
        class="mt-dialog-close"
        :aria-label="t('plansPage.closeModal')"
        @click="operatorOpen = false"
      >
        <i class="pi pi-times" aria-hidden="true"></i>
      </button>

      <div v-if="!activeOperator" class="est-inspect-body est-flow-state est-flow-state--warn">
        <p>{{ t('establishment.operatorNotFound') }}</p>
      </div>

      <template v-else>
        <header class="est-inspect-header op-profile__head" style="margin: 0">
          <p class="op-profile__eyebrow">{{ t('establishment.operatorInfo') }}</p>
          <h2 class="op-profile__title">{{ heroName }}</h2>
        </header>

        <div class="est-inspect-scroll op-profile">
          <div class="op-profile__banner">
            <div class="op-profile__identity">
              <img
                v-if="photoUrl"
                :src="photoUrl"
                alt=""
                class="op-profile__avatar-img"
                width="72"
                height="72"
              />
              <span v-else class="op-profile__avatar">{{ userInitial }}</span>
              <div class="op-profile__meta">
                <p class="op-profile__role">{{ activeUser?.job_title || activeUser?.jobTitle || t('iam.operational.badge') }}</p>
                <a v-if="heroEmail && heroEmail !== '—'" class="op-profile__email" :href="`mailto:${heroEmail}`">{{ heroEmail }}</a>
                <p v-else class="op-profile__email">{{ heroEmail }}</p>
              </div>
            </div>

            <aside class="op-profile__schedule" aria-label="schedule">
              <div class="op-profile__schedule-head">
                <i class="pi pi-clock" aria-hidden="true"></i>
                <span>{{ t('profileView.scheduleTitle') }}</span>
              </div>
              <table>
                <thead>
                  <tr>
                    <th>{{ t('profileView.scheduleShift') }}</th>
                    <th>{{ t('profileView.scheduleDay') }}</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="(row, idx) in scheduleRows" :key="idx">
                    <td>{{ row.turn }}</td>
                    <td>{{ row.day }}</td>
                  </tr>
                </tbody>
              </table>
            </aside>
          </div>

          <div class="op-profile__stats">
            <div class="op-profile__stat">
              <span class="op-profile__stat-label">
                <i class="pi pi-bell" aria-hidden="true"></i>
                {{ t('establishment.fieldAlertsAnswered') }}
              </span>
              <span class="op-profile__stat-value">{{ activeOperator.alerts_answered ?? 0 }}</span>
            </div>
            <div class="op-profile__stat">
              <span class="op-profile__stat-label">
                <i class="pi pi-calendar" aria-hidden="true"></i>
                {{ t('establishment.fieldEntryDate') }}
              </span>
              <span class="op-profile__stat-value op-profile__stat-value--sm">{{ entryDate }}</span>
            </div>
            <div class="op-profile__stat">
              <span class="op-profile__stat-label">
                <i class="pi pi-building" aria-hidden="true"></i>
                {{ t('establishment.fieldAssignedEst') }}
              </span>
              <span class="op-profile__stat-value op-profile__stat-value--sm">{{ establishment?.establishment_name || '—' }}</span>
            </div>
          </div>

          <div class="op-profile__grid">
            <div class="op-profile__field">
              <span class="op-profile__field-label"><i class="pi pi-user" aria-hidden="true"></i> {{ t('establishment.fieldOpName') }}</span>
              <span class="op-profile__field-value">{{ activeUser?.name || '—' }}</span>
            </div>
            <div class="op-profile__field">
              <span class="op-profile__field-label"><i class="pi pi-id-card" aria-hidden="true"></i> {{ t('establishment.fieldDni') }}</span>
              <span class="op-profile__field-value">{{ activeUser?.dni || activeUser?.document || '—' }}</span>
            </div>
            <div class="op-profile__field">
              <span class="op-profile__field-label"><i class="pi pi-envelope" aria-hidden="true"></i> {{ t('establishment.fieldEmail') }}</span>
              <span class="op-profile__field-value">{{ activeUser?.email || '—' }}</span>
            </div>
            <div class="op-profile__field">
              <span class="op-profile__field-label"><i class="pi pi-phone" aria-hidden="true"></i> {{ t('establishment.fieldPhone') }}</span>
              <span class="op-profile__field-value">{{ activeUser?.phone || '—' }}</span>
            </div>
            <div class="op-profile__field">
              <span class="op-profile__field-label"><i class="pi pi-building" aria-hidden="true"></i> {{ t('establishment.fieldAssignedEst') }}</span>
              <span class="op-profile__field-value">{{ establishment?.establishment_name || '—' }}</span>
            </div>
            <div class="op-profile__field">
              <span class="op-profile__field-label"><i class="pi pi-briefcase" aria-hidden="true"></i> {{ t('establishment.fieldJobTitle') }}</span>
              <span class="op-profile__field-value">{{ activeUser?.job_title || activeUser?.jobTitle || '—' }}</span>
            </div>
          </div>
        </div>

        <footer class="est-inspect-footer">
          <p class="est-inspect-footer__hint">{{ t('establishment.operatorInfo') }}</p>
        </footer>
      </template>
    </div>
  </pv-dialog>

  <!-- Device -->
  <pv-dialog
    v-model:visible="deviceOpen"
    modal
    append-to="body"
    :draggable="false"
    :closable="false"
    :show-header="false"
    dismissable-mask
    :style="{ width: 'min(92vw, 720px)' }"
    :pt="dialogPt"
  >
    <div class="mt-dialog-shell est-inspect-shell">
      <button
        type="button"
        class="mt-dialog-close"
        :aria-label="t('plansPage.closeModal')"
        @click="deviceOpen = false"
      >
        <i class="pi pi-times" aria-hidden="true"></i>
      </button>

      <div v-if="!activeDevice" class="est-inspect-body est-flow-state est-flow-state--warn">
        <p>{{ t('establishment.deviceNotFound') }}</p>
      </div>

      <template v-else>
        <header class="est-inspect-header">
          <h2 class="est-flow-title">{{ activeDevice.exact_location || `Device #${activeDevice.id}` }}</h2>
          <p class="est-flow-subtitle">{{ t('establishment.deviceInfo') }}</p>
        </header>

        <div class="est-inspect-scroll">
          <div class="est-flow-fields est-flow-fields--span">
            <div class="est-flow-field">
              <span class="est-flow-field__label"><i class="pi pi-hashtag" aria-hidden="true"></i> {{ t('establishment.fieldId') }}</span>
              <span class="est-flow-field__value">{{ activeDevice.id }}</span>
            </div>
            <div class="est-flow-field">
              <span class="est-flow-field__label"><i class="pi pi-building" aria-hidden="true"></i> {{ t('establishment.fieldAssignedEst') }}</span>
              <span class="est-flow-field__value">{{ establishment?.establishment_name || '—' }}</span>
            </div>
            <div class="est-flow-field est-flow-field--full">
              <span class="est-flow-field__label"><i class="pi pi-map-marker" aria-hidden="true"></i> {{ t('establishment.fieldExactLocation') }}</span>
              <span class="est-flow-field__value">{{ activeDevice.exact_location || '—' }}</span>
            </div>
            <div class="est-flow-field est-flow-field--full">
              <span class="est-flow-field__label"><i class="pi pi-box" aria-hidden="true"></i> {{ t('establishment.fieldMedicationType') }}</span>
              <span class="est-flow-field__value">{{ medType }}</span>
            </div>
          </div>

          <h3 class="est-flow-section-title">{{ t('establishment.sensorReadings') }}</h3>
          <p class="est-flow-live-hint">{{ t('establishment.sensorLiveHint') }}</p>
          <SensorReadingsGrid :device="activeDevice" label-prefix="establishment" />
        </div>

        <footer class="est-inspect-footer">
          <p class="est-inspect-footer__hint">{{ t('establishment.deviceInfo') }}</p>
        </footer>
      </template>
    </div>
  </pv-dialog>
</template>
