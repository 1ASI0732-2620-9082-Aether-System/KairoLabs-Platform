<script setup>
import { ref, computed, watch, onMounted, onBeforeUnmount } from 'vue';
import { useI18n } from 'vue-i18n';
import MtConfirmDialog from '../../../shared/presentation/components/mt-confirm-dialog.vue';
import { Device } from '../../domain/model/device.entity.js';
import { EstablishmentApi } from '../../../establishment/infrastructure/establishment-api.js';
import { readAuthSession } from '../../../iam/infrastructure/auth-session.js';

const establishmentApi = new EstablishmentApi();

const props = defineProps({
  device: { type: Object, default: null },
  labelPrefix: { type: String, default: 'monitoring' },
});

const { t } = useI18n();

// ── Live sensor readings ──────────────────────────────────────────────────────

const TICK_MS = 2200;
const REGULARIZE_COOLDOWN_MS = 18000;

const live = ref({});
const regularizedUntil = ref({});
let timer = null;
let spikeCounter = 0;

function resetLive(dev) {
  if (!dev) { live.value = {}; regularizedUntil.value = {}; return; }
  live.value = Device.initLiveSnapshot(dev);
  regularizedUntil.value = {};
}

watch(() => props.device, (dev) => resetLive(dev), { immediate: true });

function isRegularized(key) {
  const until = regularizedUntil.value[key];
  return until != null && Date.now() < until;
}

function tick() {
  const dev = props.device;
  if (!dev || !Object.keys(live.value).length) return;
  spikeCounter += 1;
  const snap = { ...live.value };
  for (const def of Device.METRIC_DEFS) {
    if (!Device.isMetricEnabled(def, dev, snap)) continue;
    if (isRegularized(def.key)) { snap[def.key] = Device.getSafeValue(def.key); continue; }
    if (def.discrete) {
      // Door opens more frequently: every ~15 ticks with 30% chance
      if (spikeCounter % 15 === 0 && Math.random() < 0.30) snap[def.key] = 'OPEN';
      else if (spikeCounter % 5 === 0) snap[def.key] = 'CLOSED';
      continue;
    }
    // Spikes every 6-8 ticks on any sensor with 40% chance
    const spikeInterval = def.key === 'temperature' ? 6 : def.key === 'humidity' ? 7 : 8;
    if (spikeCounter % spikeInterval === 0 && Math.random() < 0.40) {
      // Generate an out-of-range value
      if (def.key === 'temperature') snap[def.key] = Number((8.3 + Math.random() * 2.5).toFixed(1));
      else if (def.key === 'humidity')  snap[def.key] = Number((62 + Math.random() * 15).toFixed(1));
      else if (def.key === 'light')     snap[def.key] = Math.round(290 + Math.random() * 150);
      else if (def.key === 'co2')       snap[def.key] = Math.round(720 + Math.random() * 180);
      else if (def.key === 'vibration') snap[def.key] = Number((0.09 + Math.random() * 0.06).toFixed(3));
      else snap[def.key] = Device.driftValue(def.key, snap[def.key]);
    } else {
      snap[def.key] = Device.driftValue(def.key, snap[def.key]);
    }
  }
  live.value = snap;
}

function regularize(key) {
  const safe = Device.getSafeValue(key);
  if (safe == null) return;
  live.value = { ...live.value, [key]: safe };
  regularizedUntil.value = { ...regularizedUntil.value, [key]: Date.now() + REGULARIZE_COOLDOWN_MS };
}

const metrics = computed(() => {
  const dev = props.device;
  if (!dev) return [];
  return Device.METRIC_DEFS.filter((def) => Device.isMetricEnabled(def, dev, live.value))
    .map((def) => {
      const raw = live.value[def.key] ?? Device.readField(dev, def);
      const status = Device.evaluateMetric(def.key, raw);
      const out = Device.isOutOfRange(def.key, raw);
      const needsRegularize = out && !isRegularized(def.key);
      const displayStatus = needsRegularize ? status : out ? status : status === 'unknown' ? 'unknown' : 'ok';
      return { key: def.key, label: t(`${props.labelPrefix}.${def.labelKey}`), text: Device.formatMetric(def, raw), status, displayStatus, needsRegularize, outOfRange: out };
    });
});

onMounted(() => { timer = setInterval(tick, TICK_MS); });
onBeforeUnmount(() => { if (timer) clearInterval(timer); });

// ── Dialog ────────────────────────────────────────────────────────────────────

const dialogOpen = ref(false);
const pendingMetric = ref(null);

const dialogMeta = computed(() => {
  if (!pendingMetric.value) return '';
  return t('monitoring.regularizeConfirmMeta', {
    metric: pendingMetric.value.label,
    current: pendingMetric.value.text,
    safe: Device.formatSafeMetric(pendingMetric.value.key),
  });
});

function iconClass(st) {
  if (st === 'ok') return 'pi pi-check-circle sensor-card__icon--ok';
  if (st === 'warning') return 'pi pi-exclamation-triangle sensor-card__icon--warn';
  if (st === 'critical') return 'pi pi-times-circle sensor-card__icon--critical';
  return 'pi pi-minus-circle sensor-card__icon--unknown';
}

function statusBadge(st) {
  if (st === 'ok') return 'OK';
  if (st === 'warning') return t(`${props.labelPrefix}.statusBadgeWarn`);
  if (st === 'critical') return t(`${props.labelPrefix}.statusBadgeCritical`);
  return '—';
}

function openRegularizeDialog(metric) { pendingMetric.value = metric; dialogOpen.value = true; }
function closeDialog() { dialogOpen.value = false; pendingMetric.value = null; }
async function confirmRegularize() {
  if (!pendingMetric.value) return;
  regularize(pendingMetric.value.key);
  pendingMetric.value = null;

  // Increment alerts_answered for this operator in the backend
  try {
    const session = readAuthSession();
    const operatorId = session?.operatorId;
    if (operatorId) {
      await establishmentApi.incrementOperatorAlert(operatorId);
    }
  } catch {
    // non-fatal — counter will sync on next reload
  }
}
</script>

<template>
  <div class="sensor-readings-grid">
    <article
      v-for="m in metrics"
      :key="m.key"
      class="sensor-card"
      :class="{
        'sensor-card--ok': m.displayStatus === 'ok',
        'sensor-card--warn': m.displayStatus === 'warning',
        'sensor-card--critical': m.displayStatus === 'critical',
        'sensor-card--alert': m.outOfRange && m.needsRegularize,
      }"
    >
      <div class="sensor-card__top">
        <span class="sensor-card__label">{{ m.label }}</span>
        <span
          class="sensor-card__badge"
          :class="{
            'sensor-card__badge--ok': m.displayStatus === 'ok',
            'sensor-card__badge--warn': m.displayStatus === 'warning',
            'sensor-card__badge--critical': m.displayStatus === 'critical',
          }"
        >{{ statusBadge(m.displayStatus) }}</span>
      </div>

      <div class="sensor-card__row">
        <span
          class="sensor-card__value"
          :class="{ 'sensor-card__value--pulse': m.outOfRange && m.needsRegularize }"
        >{{ m.text }}</span>
        <div class="sensor-card__actions">
          <i :class="iconClass(m.displayStatus)" aria-hidden="true"></i>
          <button
            v-if="m.needsRegularize"
            type="button"
            class="sensor-card__fix"
            :title="t('monitoring.regularizeMetric')"
            :aria-label="t('monitoring.regularizeAria', { metric: m.label })"
            @click="openRegularizeDialog(m)"
          >
            <i class="pi pi-replay" aria-hidden="true"></i>
          </button>
        </div>
      </div>
    </article>
  </div>

  <MtConfirmDialog
    v-if="dialogOpen"
    v-model:visible="dialogOpen"
    :eyebrow="t('monitoring.regularizeEyebrow')"
    :title="t('monitoring.regularizeConfirmTitle')"
    :message="t('monitoring.regularizeConfirmMessage')"
    :meta="dialogMeta"
    :confirm-label="t('monitoring.regularize')"
    :cancel-label="t('monitoring.regularizeCancel')"
    confirm-tone="accent"
    confirm-icon="pi pi-replay"
    :close-aria-label="t('monitoring.regularizeClose')"
    @confirm="confirmRegularize"
    @cancel="closeDialog"
  />
</template>
