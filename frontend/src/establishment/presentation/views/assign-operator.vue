<script setup>
import { ref, computed, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import { useI18n } from 'vue-i18n';
import { useToast } from 'primevue/usetoast';
import { fetchDashboardPayload } from '../../../shared/infrastructure/dashboard-payload.js';
import { readAuthSession } from '../../../iam/infrastructure/auth-session.js';
import { Operator } from '../../domain/model/operator.entity.js';
import useEstablishmentStore from '../../application/establishment.store.js';

const router = useRouter();
const { t } = useI18n();
const toast = useToast();
const establishmentStore = useEstablishmentStore();

const users = ref([]);
const establishments = ref([]);
const operators = ref([]);
const isLoading = ref(true);
const isSubmitting = ref(false);
const session = ref(null);

const selectedUserIds = ref(new Set());
const selectedEstablishmentId = ref(null);

onMounted(async () => {
  session.value = readAuthSession();
  try {
    const data = await fetchDashboardPayload();
    users.value = data.users || [];
    establishments.value = data.establishments || [];
    operators.value = data.operators || [];
    await establishmentStore.fetchOperatorsAsync();
  } catch (error) {
    console.error('Error loading assignment data', error);
  } finally {
    isLoading.value = false;
  }
});

const scopedEstablishments = computed(() => {
  const adminId = Operator.normalizeId(session.value?.adminId);
  if (adminId == null) return establishments.value;
  return establishments.value.filter((e) => Operator.normalizeId(e.admin_id) === adminId);
});

const operatorCandidates = computed(() => {
  const allOperators = (users.value || []).filter(
    (u) => String(u.role || '').toUpperCase() === 'OPERATOR',
  );

  // In mock mode, filter by entity_code; in real mode show all operators
  const code = String(session.value?.entityCode || '').trim().toUpperCase();
  if (!code) return allOperators;

  const byCode = allOperators.filter(
    (u) => String(u.entity_code || '').trim().toUpperCase() === code,
  );
  // Fall back to all operators if entity_code filtering leaves nothing
  return byCode.length > 0 ? byCode : allOperators;
});

function getOperatorForUser(userId) {
  const uid = Operator.normalizeId(userId);
  return (operators.value || []).find((op) => Operator.normalizeId(op.users_id) === uid) ?? null;
}

function establishmentNameForUser(userId) {
  const op = getOperatorForUser(userId);
  if (!op?.establishment_id) return null;
  const est = scopedEstablishments.value.find(
    (e) => Operator.normalizeId(e.id) === Operator.normalizeId(op.establishment_id),
  );
  return est?.establishment_name ?? null;
}

function isUserSelected(userId) {
  return selectedUserIds.value.has(Operator.normalizeId(userId));
}

function toggleUser(userId) {
  const id = Operator.normalizeId(userId);
  const next = new Set(selectedUserIds.value);
  if (next.has(id)) next.delete(id);
  else next.add(id);
  selectedUserIds.value = next;
}

function selectEstablishment(id) {
  selectedEstablishmentId.value = Operator.normalizeId(id);
}

function isEstablishmentSelected(id) {
  return selectedEstablishmentId.value === Operator.normalizeId(id);
}

const canConfirm = computed(
  () => selectedUserIds.value.size > 0 && selectedEstablishmentId.value != null,
);

async function confirmAssignment() {
  if (!canConfirm.value || isSubmitting.value) return;

  isSubmitting.value = true;
  try {
    const userIds = [...selectedUserIds.value];
    await establishmentStore.assignOperatorsToEstablishmentAsync({
      userIds,
      establishmentId: selectedEstablishmentId.value,
    });

    const est = scopedEstablishments.value.find(
      (e) => Operator.normalizeId(e.id) === selectedEstablishmentId.value,
    );
    const count = userIds.length;

    toast.add({
      severity: 'success',
      summary: t('establishment.assignSuccess'),
      detail: t('establishment.assignSuccessDetail', {
        count,
        name: est?.establishment_name ?? '',
      }),
      life: 4000,
    });

    selectedUserIds.value = new Set();
    selectedEstablishmentId.value = null;

    operators.value = await establishmentStore.fetchOperatorsAsync();
  } catch (e) {
    console.error(e);
    toast.add({
      severity: 'error',
      summary: t('establishment.assignError'),
      life: 5000,
    });
  } finally {
    isSubmitting.value = false;
  }
}

function goBack() {
  const role = localStorage.getItem('userRole');
  if (role === 'health-entity') router.push({ name: 'home-health-entity' });
  else router.push({ name: 'operators' });
}
</script>

<template>
  <div class="est-flow-page">
    <nav class="est-flow-back-bar" aria-label="Navegación">
      <button type="button" class="est-flow-back-btn" @click="goBack">
        <i class="pi pi-arrow-left" aria-hidden="true"></i>
        <span>{{ t('establishment.assignBack') }}</span>
      </button>
    </nav>

    <div class="est-flow-card">
      <header class="est-flow-head">
        <h1 class="est-flow-title">{{ t('establishment.assignOperator') }}</h1>
        <p class="est-flow-subtitle">{{ t('establishment.assignPageSubtitle') }}</p>
      </header>

      <div v-if="isLoading" class="est-flow-state">
        <i class="pi pi-spin pi-spinner" aria-hidden="true"></i>
        <span>{{ t('establishment.assignLoading') }}</span>
      </div>

      <template v-else>
        <div class="assign-panels">
          <section class="assign-panel" :aria-label="t('establishment.assignOperatorsColumn')">
            <h2 class="assign-panel__head">{{ t('establishment.assignOperatorsColumn') }}</h2>
            <ul v-if="operatorCandidates.length" class="assign-panel__list">
              <li v-for="user in operatorCandidates" :key="user.id" class="assign-panel__row">
                <div class="assign-panel__label">
                  {{ user.name }}
                  <span v-if="establishmentNameForUser(user.id)" class="assign-panel__meta">
                    {{ establishmentNameForUser(user.id) }}
                  </span>
                </div>
                <button
                  type="button"
                  class="assign-select-btn"
                  :class="{ 'assign-select-btn--active': isUserSelected(user.id) }"
                  @click="toggleUser(user.id)"
                >
                  {{
                    isUserSelected(user.id)
                      ? t('establishment.assignSelected')
                      : t('establishment.assignSelect')
                  }}
                </button>
              </li>
            </ul>
            <p v-else class="assign-panel__empty">{{ t('establishment.assignNoOperators') }}</p>
          </section>

          <section class="assign-panel" :aria-label="t('establishment.assignEstablishmentsColumn')">
            <h2 class="assign-panel__head">{{ t('establishment.assignEstablishmentsColumn') }}</h2>
            <ul v-if="scopedEstablishments.length" class="assign-panel__list">
              <li
                v-for="est in scopedEstablishments"
                :key="est.id"
                class="assign-panel__row"
              >
                <label class="assign-panel__label" :for="`est-${est.id}`">
                  {{ est.establishment_name }}
                  <span v-if="est.city_region" class="assign-panel__meta">
                    {{ est.city_region }}{{ est.district ? `, ${est.district}` : '' }}
                  </span>
                </label>
                <input
                  :id="`est-${est.id}`"
                  class="assign-radio"
                  type="radio"
                  name="establishment"
                  :checked="isEstablishmentSelected(est.id)"
                  @change="selectEstablishment(est.id)"
                />
              </li>
            </ul>
            <p v-else class="assign-panel__empty">{{ t('establishment.assignNoEstablishments') }}</p>
          </section>
        </div>

        <footer class="est-flow-actions">
          <button type="button" class="est-flow-btn est-flow-btn--ghost" @click="goBack">
            <i class="pi pi-arrow-left" aria-hidden="true"></i>
            <span>{{ t('establishment.back') }}</span>
          </button>
          <button
            type="button"
            class="est-flow-btn est-flow-btn--accent"
            :disabled="!canConfirm || isSubmitting"
            @click="confirmAssignment"
          >
            <i
              :class="isSubmitting ? 'pi pi-spin pi-spinner' : 'pi pi-check'"
              aria-hidden="true"
            ></i>
            <span>{{ isSubmitting ? t('establishment.saving') : t('establishment.assignConfirm') }}</span>
          </button>
        </footer>
      </template>
    </div>
  </div>
</template>

<style>
.assign-panels {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 1rem;
  margin-bottom: 1.25rem;
}

.assign-panel {
  border: 1px solid var(--mt-border);
  border-radius: 14px;
  overflow: hidden;
  background: #fff;
  min-height: 320px;
  display: flex;
  flex-direction: column;
}

.assign-panel__head {
  margin: 0;
  padding: 0.65rem 0.85rem;
  font-size: 0.65rem;
  font-weight: 800;
  letter-spacing: 0.08em;
  text-transform: uppercase;
  color: var(--mt-primary);
  background: #f8fafc;
  border-bottom: 1px solid var(--mt-border);
  text-align: center;
}

.assign-panel__list {
  list-style: none;
  margin: 0;
  padding: 0;
  flex: 1;
  overflow-y: auto;
  max-height: 420px;
}

.assign-panel__row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 0.75rem;
  padding: 0.6rem 0.85rem;
  border-bottom: 1px solid var(--mt-border);
}

.assign-panel__row:last-child {
  border-bottom: none;
}

.assign-panel__label {
  flex: 1;
  min-width: 0;
  font-size: 0.8125rem;
  font-weight: 600;
  color: var(--mt-heading);
  line-height: 1.35;
}

.assign-panel__meta {
  display: block;
  font-size: 0.68rem;
  font-weight: 500;
  color: var(--mt-text-muted);
  margin-top: 0.12rem;
}

.assign-select-btn {
  flex-shrink: 0;
  min-width: 5.5rem;
  padding: 0.35rem 0.65rem;
  border-radius: 8px;
  font-size: 0.65rem;
  font-weight: 700;
  letter-spacing: 0.04em;
  text-transform: uppercase;
  font-family: inherit;
  cursor: pointer;
  border: 1px solid var(--mt-border);
  background: #fff;
  color: var(--mt-primary);
  transition: background 0.15s ease, border-color 0.15s ease, color 0.15s ease;
}

.assign-select-btn:hover {
  background: var(--mt-primary-soft);
}

.assign-select-btn--active {
  background: linear-gradient(180deg, #14b8a6, var(--mt-accent));
  border-color: var(--mt-accent);
  color: #fff;
}

.assign-radio {
  flex-shrink: 0;
  width: 1.1rem;
  height: 1.1rem;
  margin: 0;
  accent-color: var(--mt-primary);
  cursor: pointer;
}

.assign-panel__empty {
  margin: 0;
  padding: 2rem 1rem;
  text-align: center;
  font-size: 0.8125rem;
  color: var(--mt-text-muted);
}

@media (max-width: 768px) {
  .assign-panels {
    grid-template-columns: 1fr;
  }
}
</style>
