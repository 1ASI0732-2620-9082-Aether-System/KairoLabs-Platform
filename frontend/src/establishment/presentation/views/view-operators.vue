<script setup>
import { ref, onMounted, computed } from 'vue';
import { useRouter } from 'vue-router';
import { useI18n } from 'vue-i18n';
import { useToast } from 'primevue/usetoast';
import { fetchDashboardPayload } from '../../../shared/infrastructure/dashboard-payload.js';
import { readAuthSession } from '../../../iam/infrastructure/auth-session.js';
import { Operator } from '../../domain/model/operator.entity.js';
import useEstablishmentStore from '../../application/establishment.store.js';
import MtConfirmDialog from '../../../shared/presentation/components/mt-confirm-dialog.vue';

const router = useRouter();
const { t } = useI18n();
const toast = useToast();
const establishmentStore = useEstablishmentStore();

const operators = ref([]);
const users = ref([]);
const establishments = ref([]);
const isLoading = ref(true);
const searchQuery = ref('');
const session = ref(null);

const detailOpen = ref(false);
const detailRow = ref(null);
const deleteTarget = ref(null);
const deleteDialogVisible = ref(false);

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
  session.value = readAuthSession();
  try {
    const data = await fetchDashboardPayload();
    operators.value = data.operators;
    users.value = data.users;
    establishments.value = data.establishments;
  } catch (error) {
    console.error('Error loading operators', error);
  } finally {
    isLoading.value = false;
  }
});

const scopedOperators = computed(() => {
  if (!session.value?.adminId) return operators.value;
  return Operator.getAssignedForAdmin({
    operators: operators.value,
    establishments: establishments.value,
    adminId: session.value.adminId,
  });
});

const pendingUsers = computed(() => {
  const code = session.value?.entityCode;
  if (!code) return [];
  return Operator.getPendingUsers({
    users: users.value,
    operators: operators.value,
    entityCode: code,
  });
});

function getUserById(userId) {
  const id = Operator.normalizeId(userId);
  return users.value.find((u) => Operator.normalizeId(u.id) === id) ?? { name: t('establishment.unknownUser'), email: '—' };
}

function getEstablishmentName(establishmentId) {
  if (establishmentId == null) return t('establishment.unassignedSite');
  const est = establishments.value.find((e) => Operator.normalizeId(e.id) === Operator.normalizeId(establishmentId));
  return est?.establishment_name ?? '—';
}

const tableRows = computed(() => {
  const assigned = scopedOperators.value.map((op) => {
    const user = getUserById(op.users_id);
    return {
      key: `op-${op.id}`,
      type: 'assigned',
      operator: op,
      user,
      name: user.name,
      schedule: Operator.formatSchedule(op.schedule),
      alerts: op.alerts_answered ?? 0,
      establishment: getEstablishmentName(op.establishment_id),
    };
  });
  const pending = pendingUsers.value.map((user) => ({
    key: `pending-${user.id}`,
    type: 'pending',
    operator: null,
    user,
    name: user.name,
    schedule: '—',
    alerts: '—',
    establishment: t('establishment.unassignedSite'),
  }));
  return [...assigned, ...pending];
});

const filteredRows = computed(() => {
  const q = searchQuery.value.trim().toLowerCase();
  if (!q) return tableRows.value;
  return tableRows.value.filter((row) => (row.name || '').toLowerCase().includes(q));
});

const stats = computed(() => {
  const total = tableRows.value.length;
  const totalAlerts = scopedOperators.value.reduce(
    (acc, op) => acc + (op.alerts_answered || 0),
    0,
  );
  return { total, totalAlerts, pending: pendingUsers.value.length };
});

const scheduleRows = computed(() => {
  const sch = detailRow.value?.operator?.schedule;
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
  return [{ turn: t('establishment.scheduleShift'), day: Operator.formatSchedule(sch) }];
});

const entryDate = computed(() => {
  const op = detailRow.value?.operator;
  const u = detailRow.value?.user;
  if (!op && !u) return '—';
  const raw = op?.entry_date ?? u?.entry_date ?? op?.createdAt ?? op?.created_at;
  if (!raw) return '—';
  const d = new Date(raw);
  return Number.isNaN(d.getTime()) ? String(raw) : d.toLocaleDateString();
});

const userInitial = computed(() => {
  const n = detailRow.value?.user?.name;
  return n && n.length ? n.charAt(0).toUpperCase() : '?';
});

const photoUrl = computed(() => {
  const u = detailRow.value?.user;
  return u?.photo || u?.photo_url || u?.avatar || null;
});

function goHome() {
  router.push({ name: 'home-operational-staff' });
}

function viewOperator(row) {
  detailRow.value = row;
  detailOpen.value = true;
}

function goAssign() {
  router.push({ name: 'assign-operator' });
}

function askDelete(row) {
  deleteTarget.value = row;
  deleteDialogVisible.value = true;
}

async function confirmDelete() {
  if (!deleteTarget.value?.operator) return;
  try {
    await establishmentStore.deleteOperatorAsync(deleteTarget.value.operator.id);
    operators.value = operators.value.filter(
      (op) => Number(op.id) !== Number(deleteTarget.value.operator.id),
    );
    if (detailRow.value?.operator?.id === deleteTarget.value.operator.id) {
      detailOpen.value = false;
      detailRow.value = null;
    }
    toast.add({ severity: 'success', summary: t('establishment.deleteDone'), life: 3000 });
  } catch {
    toast.add({ severity: 'error', summary: t('establishment.deleteError'), life: 4000 });
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
        <span>{{ t('establishment.backToStaffHome') }}</span>
      </button>
    </nav>

    <div class="est-flow-card">
      <header class="est-flow-head est-flow-head--row">
        <div class="est-flow-head__text">
          <h1 class="est-flow-title">{{ t('establishment.operators') }}</h1>
          <p class="est-flow-subtitle">{{ t('establishment.operatorsListSubtitle') }}</p>
        </div>
        <div class="est-flow-stats">
          <div class="est-flow-stat est-flow-stat--navy">
            <span class="est-flow-stat__icon" aria-hidden="true"><i class="pi pi-users"></i></span>
            <span class="est-flow-stat__label">{{ t('establishment.statTotalStaff') }}</span>
            <span class="est-flow-stat__value">{{ stats.total }}</span>
          </div>
          <div class="est-flow-stat est-flow-stat--orange">
            <span class="est-flow-stat__icon" aria-hidden="true"><i class="pi pi-bell"></i></span>
            <span class="est-flow-stat__label">{{ t('establishment.statAlertsAnswered') }}</span>
            <span class="est-flow-stat__value">{{ stats.totalAlerts }}</span>
          </div>
          <div v-if="stats.pending > 0" class="est-flow-stat est-flow-stat--warn">
            <span class="est-flow-stat__icon" aria-hidden="true"><i class="pi pi-user-plus"></i></span>
            <span class="est-flow-stat__label">{{ t('establishment.statUnassigned') }}</span>
            <span class="est-flow-stat__value">{{ stats.pending }}</span>
          </div>
        </div>
      </header>

      <div class="est-flow-toolbar">
        <div class="est-flow-search">
          <i class="pi pi-search" aria-hidden="true"></i>
          <input
            v-model="searchQuery"
            type="search"
            :placeholder="t('establishment.searchOperatorsPlaceholder')"
            autocomplete="off"
          />
        </div>
      </div>

      <div v-if="isLoading" class="est-flow-state">
        <i class="pi pi-spin pi-spinner" aria-hidden="true"></i>
        <span>{{ t('establishment.loadingOperators') }}</span>
      </div>

      <div v-else-if="filteredRows.length === 0" class="est-flow-state">
        <i class="pi pi-filter-slash" aria-hidden="true"></i>
        <span>{{ t('establishment.emptyOperatorsSearch') }}</span>
      </div>

      <div v-else class="est-flow-table-wrap">
        <table class="est-flow-table">
          <thead>
            <tr>
              <th>{{ t('establishment.colOpName') }}</th>
              <th>{{ t('establishment.colOpShift') }}</th>
              <th>{{ t('establishment.colOpAlerts') }}</th>
              <th>{{ t('establishment.colOpEstablishment') }}</th>
              <th class="est-flow-table__actions">{{ t('establishment.colActions') }}</th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="row in filteredRows"
              :key="row.key"
              :class="{ 'op-row--pending': row.type === 'pending' }"
            >
              <td>
                <div class="est-flow-table__name">{{ row.name }}</div>
                <div v-if="row.type === 'assigned'" class="est-flow-table__meta">
                  #{{ row.operator.id }}
                </div>
                <span v-else class="est-flow-type-tag" style="margin-top: 0.25rem">
                  {{ t('establishment.pendingBadge') }}
                </span>
              </td>
              <td>{{ row.schedule }}</td>
              <td>
                <span
                  v-if="row.type === 'assigned'"
                  style="font-weight: 700; color: var(--mt-primary)"
                >
                  {{ row.alerts }}
                </span>
                <span v-else>—</span>
              </td>
              <td>{{ row.establishment }}</td>
              <td class="est-flow-table__actions">
                <div class="est-flow-table__actions-row">
                  <template v-if="row.type === 'assigned'">
                    <button
                      type="button"
                      class="est-flow-icon-btn"
                      :aria-label="t('establishment.viewDetail')"
                      @click="viewOperator(row)"
                    >
                      <i class="pi pi-eye" aria-hidden="true"></i>
                    </button>
                    <button
                      type="button"
                      class="est-flow-icon-btn est-flow-icon-btn--danger"
                      :aria-label="t('establishment.deleteOperator')"
                      @click="askDelete(row)"
                    >
                      <i class="pi pi-trash" aria-hidden="true"></i>
                    </button>
                  </template>
                  <button
                    v-else
                    type="button"
                    class="est-flow-icon-btn"
                    :aria-label="t('establishment.assignSite')"
                    :title="t('establishment.assignSite')"
                    @click="goAssign"
                  >
                    <i class="pi pi-user-plus" aria-hidden="true"></i>
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <pv-dialog
      v-model:visible="detailOpen"
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
          @click="detailOpen = false"
        >
          <i class="pi pi-times" aria-hidden="true"></i>
        </button>
        <template v-if="detailRow?.operator">
          <header class="est-inspect-header op-profile__head" style="margin: 0">
            <p class="op-profile__eyebrow">{{ t('establishment.operatorInfo') }}</p>
            <h2 class="op-profile__title">{{ detailRow.user?.name || t('establishment.unknownUser') }}</h2>
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
                  <p class="op-profile__role">
                    {{ detailRow.user?.job_title || detailRow.user?.jobTitle || t('iam.operational.badge') }}
                  </p>
                  <a
                    v-if="detailRow.user?.email"
                    class="op-profile__email"
                    :href="`mailto:${detailRow.user.email}`"
                  >{{ detailRow.user.email }}</a>
                  <p v-else class="op-profile__email">—</p>
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
                    <tr v-for="(sch, idx) in scheduleRows" :key="idx">
                      <td>{{ sch.turn }}</td>
                      <td>{{ sch.day }}</td>
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
                <span class="op-profile__stat-value">{{ detailRow.operator.alerts_answered ?? 0 }}</span>
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
                <span class="op-profile__stat-value op-profile__stat-value--sm">{{ detailRow.establishment }}</span>
              </div>
            </div>

            <div class="op-profile__grid">
              <div class="op-profile__field">
                <span class="op-profile__field-label"><i class="pi pi-user" aria-hidden="true"></i> {{ t('establishment.fieldOpName') }}</span>
                <span class="op-profile__field-value">{{ detailRow.user?.name || '—' }}</span>
              </div>
              <div class="op-profile__field">
                <span class="op-profile__field-label"><i class="pi pi-id-card" aria-hidden="true"></i> {{ t('establishment.fieldDni') }}</span>
                <span class="op-profile__field-value">{{ detailRow.user?.dni || detailRow.user?.document || '—' }}</span>
              </div>
              <div class="op-profile__field">
                <span class="op-profile__field-label"><i class="pi pi-envelope" aria-hidden="true"></i> {{ t('establishment.fieldEmail') }}</span>
                <span class="op-profile__field-value">{{ detailRow.user?.email || '—' }}</span>
              </div>
              <div class="op-profile__field">
                <span class="op-profile__field-label"><i class="pi pi-phone" aria-hidden="true"></i> {{ t('establishment.fieldPhone') }}</span>
                <span class="op-profile__field-value">{{ detailRow.user?.phone || '—' }}</span>
              </div>
              <div class="op-profile__field">
                <span class="op-profile__field-label"><i class="pi pi-building" aria-hidden="true"></i> {{ t('establishment.fieldAssignedEst') }}</span>
                <span class="op-profile__field-value">{{ detailRow.establishment }}</span>
              </div>
              <div class="op-profile__field">
                <span class="op-profile__field-label"><i class="pi pi-briefcase" aria-hidden="true"></i> {{ t('establishment.fieldJobTitle') }}</span>
                <span class="op-profile__field-value">{{ detailRow.user?.job_title || detailRow.user?.jobTitle || '—' }}</span>
              </div>
            </div>
          </div>
          <footer class="est-inspect-footer">
            <button
              type="button"
              class="est-flow-btn est-flow-btn--danger"
              @click="askDelete(detailRow)"
            >
              <i class="pi pi-trash" aria-hidden="true"></i>
              <span>{{ t('establishment.deleteOperator') }}</span>
            </button>
          </footer>
        </template>
      </div>
    </pv-dialog>

    <MtConfirmDialog
      v-model:visible="deleteDialogVisible"
      :title="t('establishment.deleteOperator')"
      :message="t('establishment.deleteConfirm', { name: deleteTarget?.name || '' })"
      :meta="deleteTarget?.establishment || ''"
      :confirm-label="t('establishment.deleteOperator')"
      :cancel-label="t('common.cancel')"
      confirm-tone="danger"
      confirm-icon="pi pi-trash"
      @confirm="confirmDelete"
    />
  </div>
</template>

<style scoped>
.op-row--pending td {
  background: #fffbeb;
}
</style>
