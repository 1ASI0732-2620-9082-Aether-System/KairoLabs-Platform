<script setup>
import { computed } from 'vue';
import { useI18n } from 'vue-i18n';
import {
  Chart as ChartJS,
  Title,
  Tooltip,
  Legend,
  LineElement,
  PointElement,
  LinearScale,
  CategoryScale,
  Filler,
  RadialLinearScale,
  BarElement,
  ArcElement,
} from 'chart.js';
import { Line, Radar, Bar, Doughnut } from 'vue-chartjs';

ChartJS.register(
    Title,
    Tooltip,
    Legend,
    LineElement,
    PointElement,
    LinearScale,
    CategoryScale,
    Filler,
    RadialLinearScale,
    BarElement,
    ArcElement,
);

const props = defineProps({
  db: {
    type: Object,
    default: null,
  },
  loading: {
    type: Boolean,
    default: false,
  },
  embedded: {
    type: Boolean,
    default: false,
  },
});

const { t, locale } = useI18n();

const chartFont = () => ({
  family: "'Outfit', system-ui, sans-serif",
});

const KL = {
  navy: '#112433',
  navyMid: '#1a3a4f',
  orange: '#F37021',
  teal: '#0d9488',
  muted: '#64748b',
};

function numAvg(arr, key) {
  const vals = arr.map((x) => Number(x[key])).filter((n) => !Number.isNaN(n));
  if (!vals.length) return 0;
  return vals.reduce((a, b) => a + b, 0) / vals.length;
}

function truncateLabel(s, max = 16) {
  if (!s) return '—';
  const str = String(s);
  return str.length > max ? `${str.slice(0, max - 1)}…` : str;
}

/** Alertas por operador: suma por users_id (evita duplicar nombres en el gráfico) */
function aggregateAlertsByUser(operators, users) {
  const map = new Map();
  for (const op of operators || []) {
    const uid = op.users_id;
    if (uid == null) continue;
    map.set(uid, (map.get(uid) || 0) + (Number(op.alerts_answered) || 0));
  }
  const rows = [...map.entries()]
      .map(([usersId, total]) => ({
        usersId,
        total,
        name: users?.find((u) => Number(u.id) === Number(usersId))?.name ?? `ID ${usersId}`,
      }))
      .sort((a, b) => b.total - a.total)
      .slice(0, 6);
  return rows;
}

const devices = computed(() => props.db?.devices ?? []);
const transports = computed(() => props.db?.transports ?? []);
const operators = computed(() => props.db?.operators ?? []);
const users = computed(() => props.db?.users ?? []);
const subscriptions = computed(() => props.db?.subscriptions ?? []);
const establishments = computed(() => props.db?.establishments ?? []);

const lastSyncLabel = computed(() => {
  const dates = devices.value
      .map((d) => d.updated_at || d.created_at)
      .filter(Boolean)
      .map((s) => new Date(s).getTime())
      .filter((n) => !Number.isNaN(n));
  if (!dates.length) return '—';
  const max = new Date(Math.max(...dates));
  return max.toLocaleString(locale.value === 'es' ? 'es-PE' : 'en-US', {
    dateStyle: 'short',
    timeStyle: 'short',
  });
});

const kpis = computed(() => {
  const devs = devices.value;
  const temps = devs.map((d) => Number(d.temperature)).filter((n) => !Number.isNaN(n));
  const avgTemp = temps.length ? temps.reduce((a, b) => a + b, 0) / temps.length : null;
  const openDoors = devs.filter((d) => {
    const s = String(d.door_status || '').toUpperCase();
    return s === 'OPEN' || s === 'ABIERTA' || s === 'OPENED';
  }).length;
  return {
    devices: devs.length,
    establishments: establishments.value.length,
    transports: transports.value.length,
    operators: operators.value.length,
    avgTemp: avgTemp != null ? `${avgTemp.toFixed(1)}°C` : '—',
    openDoors,
  };
});

const planCounts = computed(() => {
  const c = { BASIC: 0, PREMIUM: 0, ENTERPRISE: 0, OTHER: 0 };
  for (const s of subscriptions.value) {
    const p = String(s.plan || '').toUpperCase();
    if (p === 'BASIC') c.BASIC++;
    else if (p === 'PREMIUM' || p === 'PROFESSIONAL' || p === 'PRO') c.PREMIUM++;
    else if (p === 'ENTERPRISE') c.ENTERPRISE++;
    else if (s.plan) c.OTHER++;
  }
  return c;
});

/** Línea: puntos = sensores reales (ubicación en eje X), temp y humedad desde API */
const biometricData = computed(() => {
  const devs = devices.value.slice(0, 10);
  if (!devs.length) {
    return {
      labels: ['—'],
      datasets: [
        { label: t('monitoring.ccLegendTemp'), data: [0], borderColor: KL.orange, backgroundColor: 'rgba(243, 112, 33, 0.1)', fill: true, tension: 0.35, pointRadius: 4 },
        { label: t('monitoring.ccLegendHum'), data: [0], borderColor: KL.navyMid, backgroundColor: 'rgba(17, 36, 51, 0.08)', fill: true, tension: 0.35, pointRadius: 4 },
      ],
    };
  }
  return {
    labels: devs.map((d) => truncateLabel(d.exact_location || d.type_of_medication, 14)),
    datasets: [
      {
        label: t('monitoring.ccLegendTemp'),
        data: devs.map((d) => Number(d.temperature) || 0),
        borderColor: KL.orange,
        backgroundColor: 'rgba(243, 112, 33, 0.14)',
        fill: true,
        tension: 0.35,
        pointRadius: 4,
        pointHoverRadius: 6,
        pointBackgroundColor: KL.orange,
      },
      {
        label: t('monitoring.ccLegendHum'),
        data: devs.map((d) => Number(d.humidity) || 0),
        borderColor: KL.navyMid,
        backgroundColor: 'rgba(26, 58, 79, 0.1)',
        fill: true,
        tension: 0.35,
        pointRadius: 4,
        pointHoverRadius: 6,
        pointBackgroundColor: KL.navyMid,
      },
    ],
  };
});

const biometricOptions = computed(() => ({
  responsive: true,
  maintainAspectRatio: false,
  animation: { duration: 900, easing: 'easeOutQuart' },
  interaction: { intersect: false, mode: 'index' },
  plugins: {
    legend: {
      display: true,
      position: 'top',
      align: 'end',
      labels: {
        usePointStyle: true,
        pointStyle: 'circle',
        boxWidth: 8,
        padding: 16,
        color: KL.muted,
        font: { ...chartFont(), size: 11, weight: '600' },
      },
    },
    tooltip: {
      backgroundColor: KL.navy,
      titleColor: '#f8fafc',
      bodyColor: '#e2e8f0',
      padding: 12,
      cornerRadius: 10,
      borderColor: 'rgba(255,255,255,0.08)',
      borderWidth: 1,
    },
  },
  scales: {
    y: {
      grid: { color: '#f1f5f9', lineWidth: 1 },
      ticks: { color: KL.muted, font: chartFont() },
      border: { display: false },
    },
    x: {
      grid: { display: false },
      ticks: {
        color: KL.muted,
        font: { ...chartFont(), size: 10 },
        maxRotation: 35,
        minRotation: 35,
      },
      border: { display: false },
    },
  },
}));

/** Radar: promedio de sensores de dispositivos; si hay transportes, mezcla leve */
const stabilityData = computed(() => {
  const devs = devices.value;
  const trans = transports.value;
  const vib = numAvg(devs, 'vibration') || (trans.length ? numAvg(trans, 'vibration') : 0);
  const press = numAvg(devs, 'atmospheric_pressure') || (trans.length ? numAvg(trans, 'atmospheric_pressure') : 1013);
  const part = numAvg(devs, 'suspended_particles') || (trans.length ? numAvg(trans, 'suspended_particles') : 0);
  const air = numAvg(devs, 'air_quality') || (trans.length ? numAvg(trans, 'air_quality') : 0);
  const toScore = (x, f) => Math.min(100, Math.max(8, Math.round(x * f)));

  const scores = [
    toScore(vib, 180),
    toScore(Math.max(0, press - 1005), 8),
    toScore(part, 4),
    toScore(air, 2.5),
  ];

  return {
    labels: [
      t('monitoring.ccRadarVibration'),
      t('monitoring.ccRadarPressure'),
      t('monitoring.ccRadarParticles'),
      t('monitoring.ccRadarAirQuality'),
    ],
    datasets: [
      {
        label: t('monitoring.ccStability'),
        data: scores,
        borderColor: KL.teal,
        backgroundColor: 'rgba(13, 148, 136, 0.2)',
        pointBackgroundColor: KL.teal,
        pointBorderColor: '#fff',
        pointHoverBackgroundColor: '#0f766e',
        borderWidth: 2,
      },
    ],
  };
});

const radarOptions = computed(() => ({
  responsive: true,
  maintainAspectRatio: false,
  animation: { duration: 900, easing: 'easeOutQuart' },
  plugins: {
    legend: { display: false },
    tooltip: {
      backgroundColor: KL.navy,
      titleColor: '#f8fafc',
      bodyColor: '#e2e8f0',
      padding: 12,
      cornerRadius: 10,
    },
  },
  scales: {
    r: {
      min: 0,
      max: 100,
      ticks: { display: false, stepSize: 25 },
      grid: { color: '#e2e8f0' },
      angleLines: { color: '#e2e8f0' },
      pointLabels: {
        color: '#475569',
        font: { ...chartFont(), size: 11, weight: '600' },
      },
    },
  },
}));

const efficiencyData = computed(() => {
  const rows = aggregateAlertsByUser(operators.value, users.value);
  if (!rows.length) {
    return {
      labels: ['—'],
      datasets: [{ label: '—', data: [0], backgroundColor: '#e2e8f0', borderRadius: 8, barThickness: 14 }],
    };
  }
  return {
    labels: rows.map((r) => truncateLabel(r.name, 22)),
    datasets: [
      {
        label: t('monitoring.ccEfficiency'),
        data: rows.map((r) => r.total),
        backgroundColor: (ctx) => {
          const chart = ctx.chart;
          const { ctx: c, chartArea } = chart;
          if (!chartArea) return KL.teal;
          const g = c.createLinearGradient(chartArea.left, 0, chartArea.right, 0);
          g.addColorStop(0, KL.teal);
          g.addColorStop(1, KL.navyMid);
          return g;
        },
        borderRadius: 8,
        barThickness: 14,
      },
    ],
  };
});

const barOptions = computed(() => ({
  indexAxis: 'y',
  responsive: true,
  maintainAspectRatio: false,
  animation: { duration: 900, easing: 'easeOutQuart' },
  plugins: {
    legend: { display: false },
    tooltip: {
      backgroundColor: KL.navy,
      titleColor: '#f8fafc',
      bodyColor: '#e2e8f0',
      padding: 12,
      cornerRadius: 10,
    },
  },
  scales: {
    x: {
      grid: { color: '#f1f5f9', drawBorder: false },
      ticks: { color: '#94a3b8', font: chartFont() },
      border: { display: false },
    },
    y: {
      grid: { display: false },
      ticks: { color: '#334155', font: { ...chartFont(), size: 11, weight: '600' } },
      border: { display: false },
    },
  },
}));

const businessData = computed(() => {
  const subs = subscriptions.value;
  const active = subs.filter((s) => String(s.status).toUpperCase() === 'ACTIVE').length;
  const pending = subs.filter((s) => String(s.status).toUpperCase() === 'PENDING').length;
  const expired = subs.filter((s) => String(s.status).toUpperCase() === 'EXPIRED').length;
  const other = Math.max(0, subs.length - active - pending - expired);
  return {
    labels: [
      t('monitoring.ccSubActive'),
      t('monitoring.ccSubPending'),
      t('monitoring.ccSubExpired'),
      ...(other ? [t('monitoring.ccOther')] : []),
    ],
    datasets: [
      {
        data: other ? [active, pending, expired, other] : [active, pending, expired],
        backgroundColor: [KL.navy, KL.orange, '#ef4444', '#94a3b8'],
        borderWidth: 0,
        cutout: '72%',
        spacing: 2,
      },
    ],
  };
});

const doughnutOptions = computed(() => ({
  responsive: true,
  maintainAspectRatio: false,
  animation: { duration: 900, easing: 'easeOutQuart' },
  plugins: {
    legend: {
      display: true,
      position: 'bottom',
      labels: {
        usePointStyle: true,
        pointStyle: 'circle',
        boxWidth: 8,
        padding: 12,
        color: KL.muted,
        font: { ...chartFont(), size: 11 },
      },
    },
    tooltip: {
      backgroundColor: KL.navy,
      titleColor: '#f8fafc',
      bodyColor: '#e2e8f0',
      padding: 12,
      cornerRadius: 10,
    },
  },
}));

const subscriptionTotal = computed(() => subscriptions.value.length);

const kpiCards = computed(() => [
  { key: 'devices', icon: 'pi pi-th-large', value: String(kpis.value.devices), label: t('monitoring.ccKpiDevices'), variant: 'slate' },
  { key: 'est', icon: 'pi pi-building', value: String(kpis.value.establishments), label: t('monitoring.ccKpiEst'), variant: 'slate' },
  { key: 'trans', icon: 'pi pi-truck', value: String(kpis.value.transports), label: t('monitoring.ccKpiTransports'), variant: 'slate' },
  { key: 'ops', icon: 'pi pi-users', value: String(kpis.value.operators), label: t('monitoring.ccKpiOperators'), variant: 'slate' },
  { key: 'temp', icon: 'pi pi-chart-line', value: kpis.value.avgTemp, label: t('monitoring.ccKpiAvgTemp'), variant: 'accent' },
  { key: 'doors', icon: 'pi pi-lock-open', value: String(kpis.value.openDoors), label: t('monitoring.ccKpiOpenDoors'), variant: kpis.value.openDoors > 0 ? 'amber' : 'slate' },
]);
</script>

<template>
  <div v-if="db" class="control-panel" :class="{ 'control-panel--embedded': embedded }">
    <header class="cc-hero">
      <div class="cc-hero__main">
        <p class="cc-hero__eyebrow">KairoLabs · IoT</p>
        <h2 class="cc-hero__title">{{ t('monitoring.ccTitle') }}</h2>
        <p class="cc-hero__subtitle">{{ t('monitoring.ccSubtitle') }}</p>
        <p class="cc-hero__meta">
          <i class="pi pi-clock" aria-hidden="true"></i>
          <span>{{ t('monitoring.ccSync') }}: <strong>{{ lastSyncLabel }}</strong></span>
        </p>
      </div>
      <div class="cc-hero__status">
        <span class="cc-hero__badge">
          <span class="pulse-indicator" aria-hidden="true"></span>
          {{ t('monitoring.ccOnline') }}
        </span>
      </div>
    </header>

    <section class="kpi-strip" aria-label="KPI">
      <article
          v-for="(card, idx) in kpiCards"
          :key="card.key"
          class="kpi-card"
          :class="[`kpi-card--${card.variant}`]"
          :style="{ animationDelay: `${0.04 + idx * 0.05}s` }"
      >
        <div class="kpi-card__icon" aria-hidden="true">
          <i :class="card.icon"></i>
        </div>
        <div class="kpi-card__body">
          <span class="kpi-card__value">{{ card.value }}</span>
          <span class="kpi-card__label">{{ card.label }}</span>
        </div>
      </article>
    </section>

    <div class="plans-strip">
      <span class="plans-strip-title">{{ t('monitoring.ccPlans') }}</span>
      <div class="plans-pills">
        <span class="pill pill--b">{{ t('monitoring.ccPlanBasic') }} · {{ planCounts.BASIC }}</span>
        <span class="pill pill--p">{{ t('monitoring.ccPlanPro') }} · {{ planCounts.PREMIUM }}</span>
        <span class="pill pill--e">{{ t('monitoring.ccPlanEnt') }} · {{ planCounts.ENTERPRISE }}</span>
        <span v-if="planCounts.OTHER" class="pill pill--o">{{ t('monitoring.ccOther') }} · {{ planCounts.OTHER }}</span>
      </div>
    </div>

    <div class="bento-grid">
      <div class="bento-card main-chart" style="animation-delay: 0.22s">
        <div class="card-header">
          <h3>{{ t('monitoring.ccBiometric') }}</h3>
          <p>{{ t('monitoring.ccBiometricHint') }}</p>
        </div>
        <div class="chart-wrapper line-chart">
          <Line :data="biometricData" :options="biometricOptions" />
        </div>
      </div>

      <div class="bento-card stability-chart" style="animation-delay: 0.28s">
        <div class="card-header">
          <h3>{{ t('monitoring.ccStability') }}</h3>
          <p>{{ t('monitoring.ccStabilityHint') }}</p>
        </div>
        <div class="chart-wrapper">
          <Radar :data="stabilityData" :options="radarOptions" />
        </div>
      </div>

      <div class="bento-card efficiency-chart" style="animation-delay: 0.34s">
        <div class="card-header">
          <h3>{{ t('monitoring.ccEfficiency') }}</h3>
          <p>{{ t('monitoring.ccEfficiencyHint') }}</p>
        </div>
        <div class="chart-wrapper bar-wrap">
          <Bar :data="efficiencyData" :options="barOptions" />
        </div>
      </div>

      <div class="bento-card business-chart" style="animation-delay: 0.4s">
        <div class="card-header">
          <h3>{{ t('monitoring.ccSubscriptions') }}</h3>
          <p>{{ t('monitoring.ccSubscriptionsHint') }}</p>
        </div>
        <div class="doughnut-container">
          <div class="chart-wrapper dough-wrap">
            <Doughnut :data="businessData" :options="doughnutOptions" />
          </div>
          <div class="doughnut-center">
            <span class="center-label">{{ t('monitoring.ccDoughnutCenter') }}</span>
            <span class="center-value">{{ subscriptionTotal }}</span>
          </div>
        </div>
      </div>
    </div>
  </div>
  <div v-else-if="loading || !db" class="panel-loading" :class="{ 'panel-loading--embedded': embedded }">
    <div class="loader-content">
      <i class="pi pi-spin pi-spinner" style="font-size: 2rem" aria-hidden="true"></i>
      <p>{{ t('monitoring.ccLoading') }}</p>
    </div>
  </div>
</template>

<style scoped>
.control-panel {
  margin-top: 0;
  width: 100%;
}

.cc-hero {
  display: flex;
  flex-wrap: wrap;
  align-items: stretch;
  justify-content: space-between;
  gap: 1.1rem;
  margin-bottom: 1.25rem;
  padding: 1.25rem 1.35rem;
  border-radius: 18px;
  border: 1px solid rgba(17, 36, 51, 0.1);
  background:
    linear-gradient(135deg, rgba(243, 112, 33, 0.12) 0%, transparent 42%),
    linear-gradient(120deg, #163247 0%, #112433 100%);
  color: #fff;
  box-shadow: 0 12px 32px rgba(17, 36, 51, 0.14);
  animation: cc-rise 0.5s cubic-bezier(0.22, 1, 0.36, 1) both;
}

.cc-hero__main {
  min-width: 0;
  flex: 1;
}

.cc-hero__eyebrow {
  margin: 0 0 0.35rem;
  font-size: 0.68rem;
  font-weight: 700;
  letter-spacing: 0.1em;
  text-transform: uppercase;
  color: rgba(255, 255, 255, 0.72);
}

.cc-hero__title {
  margin: 0;
  font-size: clamp(1.25rem, 2.2vw, 1.55rem);
  font-weight: 800;
  letter-spacing: -0.03em;
  line-height: 1.2;
  color: #fff;
  font-family: 'Outfit', system-ui, sans-serif;
}

.cc-hero__subtitle {
  margin: 0.45rem 0 0;
  max-width: 42rem;
  font-size: 0.9rem;
  line-height: 1.45;
  color: rgba(255, 255, 255, 0.82);
}

.cc-hero__meta {
  margin: 0.7rem 0 0;
  display: inline-flex;
  align-items: center;
  gap: 0.4rem;
  font-size: 0.76rem;
  color: rgba(255, 255, 255, 0.7);
}

.cc-hero__meta strong {
  color: #ffd2b0;
  font-weight: 700;
}

.cc-hero__status {
  display: flex;
  align-items: flex-start;
}

.cc-hero__badge {
  display: inline-flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.55rem 0.9rem;
  border-radius: 12px;
  background: rgba(255, 255, 255, 0.12);
  border: 1px solid rgba(255, 255, 255, 0.22);
  font-size: 0.78rem;
  font-weight: 700;
  color: #fff;
  white-space: nowrap;
}

.control-panel--embedded .cc-hero {
  margin-bottom: 1.15rem;
}

.panel-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  gap: 1rem;
  margin-bottom: 1.1rem;
  padding: 0;
  animation: cc-rise 0.45s cubic-bezier(0.22, 1, 0.36, 1) both;
}

.panel-title {
  margin: 0;
  font-size: 1.3rem;
  font-weight: 800;
  color: var(--mt-heading, #112433);
  letter-spacing: -0.03em;
  line-height: 1.2;
}

.panel-subtitle {
  margin: 0.35rem 0 0;
  color: var(--mt-text-muted, #64748b);
  font-size: 0.875rem;
  line-height: 1.45;
  max-width: 48rem;
}

.panel-meta {
  margin: 0.5rem 0 0;
  font-size: 0.75rem;
  color: #94a3b8;
  display: flex;
  align-items: center;
  gap: 0.35rem;
}

.panel-meta strong {
  color: #475569;
  font-weight: 600;
}

.status-badge {
  display: inline-flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.4rem 0.85rem;
  flex-shrink: 0;
  border: none;
  background: transparent;
  box-shadow: none;
}

.pulse-indicator {
  width: 7px;
  height: 7px;
  background: #34d399;
  border-radius: 50%;
  box-shadow: 0 0 0 0 rgba(52, 211, 153, 0.45);
  animation: pulse 2s ease-out infinite;
}

@keyframes pulse {
  0% { box-shadow: 0 0 0 0 rgba(52, 211, 153, 0.45); }
  70% { box-shadow: 0 0 0 8px rgba(52, 211, 153, 0); }
  100% { box-shadow: 0 0 0 0 rgba(52, 211, 153, 0); }
}

@keyframes cc-rise {
  from { opacity: 0; transform: translateY(12px); }
  to { opacity: 1; transform: translateY(0); }
}

.kpi-strip {
  display: grid;
  grid-template-columns: repeat(6, minmax(0, 1fr));
  gap: 0.9rem;
  margin-bottom: 1.15rem;
}

.kpi-card {
  display: flex;
  align-items: center;
  gap: 0.95rem;
  min-height: 5.6rem;
  padding: 1.05rem 1.1rem;
  background: #fff;
  border: 1px solid #e2e8f0;
  border-radius: 16px;
  box-shadow:
      0 1px 2px rgba(17, 36, 51, 0.04),
      0 8px 24px -8px rgba(17, 36, 51, 0.08);
  animation: cc-rise 0.5s cubic-bezier(0.22, 1, 0.36, 1) both;
  transition: transform 0.2s ease, box-shadow 0.2s ease, border-color 0.2s ease;
}

.kpi-card:hover {
  transform: translateY(-3px);
  box-shadow:
      0 4px 6px rgba(17, 36, 51, 0.05),
      0 16px 32px -12px rgba(17, 36, 51, 0.12);
  border-color: #cbd5e1;
}

.kpi-card__icon {
  flex-shrink: 0;
  width: 3.1rem;
  height: 3.1rem;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 14px;
  font-size: 1.3rem;
}

.kpi-card--slate .kpi-card__icon {
  background: linear-gradient(145deg, #f1f5f9 0%, #e2e8f0 100%);
  color: #112433;
  border: 1px solid rgba(148, 163, 184, 0.35);
}

.kpi-card--accent {
  border-color: rgba(243, 112, 33, 0.35);
  background: linear-gradient(135deg, #fff7f0 0%, #ffffff 55%, #ffffff 100%);
}

.kpi-card--accent .kpi-card__icon {
  background: linear-gradient(145deg, #f37021 0%, #e05f12 100%);
  color: #fff;
  border: none;
  box-shadow: 0 6px 16px rgba(243, 112, 33, 0.35);
}

.kpi-card--amber {
  border-color: rgba(245, 158, 11, 0.45);
  background: linear-gradient(135deg, #fffbeb 0%, #ffffff 55%, #ffffff 100%);
}

.kpi-card--amber .kpi-card__icon {
  background: linear-gradient(145deg, #fbbf24 0%, #f59e0b 100%);
  color: #fff;
  border: none;
  box-shadow: 0 6px 16px rgba(245, 158, 11, 0.35);
}

.kpi-card__body {
  display: flex;
  flex-direction: column;
  align-items: flex-start;
  justify-content: center;
  gap: 0.35rem;
  min-width: 0;
}

.kpi-card__value {
  font-size: 1.55rem;
  font-weight: 800;
  color: #112433;
  letter-spacing: -0.04em;
  line-height: 1;
}

.kpi-card__label {
  font-size: 0.68rem;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.07em;
  color: #64748b;
  line-height: 1.2;
}

.plans-strip {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 0.55rem 0.85rem;
  margin-bottom: 1.15rem;
  padding: 0.7rem 0.9rem;
  background: #f8fafc;
  border: 1px solid #e2e8f0;
  border-radius: 14px;
  animation: cc-rise 0.5s cubic-bezier(0.22, 1, 0.36, 1) 0.18s both;
}

.plans-strip-title {
  font-size: 0.65rem;
  font-weight: 700;
  letter-spacing: 0.06em;
  text-transform: uppercase;
  color: #64748b;
}

.plans-pills {
  display: flex;
  flex-wrap: wrap;
  gap: 0.4rem;
}

.pill {
  font-size: 0.72rem;
  font-weight: 700;
  padding: 0.3rem 0.65rem;
  border-radius: 999px;
  color: #fff;
}

.pill--b { background: #475569; }
.pill--p { background: #f37021; }
.pill--e { background: #112433; }
.pill--o { background: #94a3b8; }

.bento-grid {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 1rem;
}

.bento-card {
  background: #fff;
  border: 1px solid var(--mt-border, #e2e8f0);
  border-radius: 16px;
  padding: 1.15rem 1.2rem;
  box-shadow: 0 1px 3px rgba(17, 36, 51, 0.06);
  animation: cc-rise 0.55s cubic-bezier(0.22, 1, 0.36, 1) both;
  transition: border-color 0.2s ease, box-shadow 0.2s ease, transform 0.2s ease;
}

.bento-card:hover {
  border-color: #cbd5e1;
  box-shadow: 0 8px 28px rgba(17, 36, 51, 0.08);
  transform: translateY(-2px);
}

.card-header { margin-bottom: 0.7rem; }

.card-header h3 {
  margin: 0;
  font-size: 1rem;
  font-weight: 700;
  color: var(--mt-heading, #112433);
  letter-spacing: -0.02em;
}

.card-header p {
  margin: 0.25rem 0 0;
  font-size: 0.78rem;
  color: var(--mt-text-muted, #64748b);
  line-height: 1.4;
}

.chart-wrapper {
  height: 220px;
  position: relative;
}

.line-chart { height: min(300px, 36vh); }
.bar-wrap { height: 240px; }
.dough-wrap { height: 220px; }

.doughnut-container {
  position: relative;
  display: flex;
  align-items: center;
  justify-content: center;
}

.doughnut-center {
  position: absolute;
  display: flex;
  flex-direction: column;
  align-items: center;
  pointer-events: none;
}

.center-label {
  font-size: 0.65rem;
  color: #94a3b8;
  text-transform: uppercase;
  font-weight: 700;
  letter-spacing: 0.05em;
}

.center-value {
  font-size: 1.4rem;
  font-weight: 800;
  color: var(--mt-heading, #112433);
  letter-spacing: -0.03em;
}

.panel-loading {
  background: transparent;
  border: none;
  border-radius: 0;
  padding: 2.5rem 1rem;
  display: flex;
  align-items: center;
  justify-content: center;
  text-align: center;
  margin-top: 0;
}

.panel-loading--embedded { min-height: 12rem; }

.loader-content { color: var(--mt-text-muted, #64748b); }

.loader-content p {
  margin-top: 0.75rem;
  font-weight: 500;
  font-size: 0.9rem;
}

@media (max-width: 1200px) {
  .kpi-strip { grid-template-columns: repeat(3, minmax(0, 1fr)); }
}

@media (max-width: 900px) {
  .bento-grid { grid-template-columns: 1fr; }
  .line-chart { height: 260px; }
}

@media (max-width: 640px) {
  .panel-header { flex-direction: column; }
  .kpi-strip { grid-template-columns: 1fr 1fr; }
  .kpi-card {
    min-height: 5.1rem;
    padding: 0.9rem 0.95rem;
  }
  .kpi-card__value { font-size: 1.35rem; }
  .kpi-card__icon {
    width: 2.7rem;
    height: 2.7rem;
    font-size: 1.1rem;
  }
}
</style>
