<script setup>
import { ref, onMounted, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import { useRouter } from 'vue-router';
import { useToast } from 'primevue/usetoast';
import useIamStore from '../../application/iam.store.js';
import useSubscriptionsStore from '../../../subscriptions/application/subscriptions.store.js';
import { Subscription } from '../../../subscriptions/domain/model/subscription.entity.js';

const iamStore = useIamStore();
const subscriptionsStore = useSubscriptionsStore();
const { t } = useI18n();
const router = useRouter();
const toast = useToast();

const loading = ref(true);
const error = ref(null);
const user = ref(null);
const establishment = ref(null);
const planApi = ref('BASIC');
const isEditing = ref(false);

const form = ref({
  name: '',
  email: '',
  phone: '',
  entityName: '',
  jobTitle: '',
  dni: '',
  entityCode: '',
});

const planDisplay = computed(() => {
  const api = String(planApi.value || 'BASIC').toUpperCase();
  const catalog = Subscription.apiPlanToCatalogId(api);
  const keyMap = { basic: 'basic', professional: 'pro', pro: 'pro', premium: 'premium' };
  const localeKey = keyMap[catalog] || 'basic';
  try {
    return t(`plansPage.names.${localeKey}`);
  } catch {
    return String(api);
  }
});

const userInitial = computed(() => (form.value.name?.charAt(0) ?? 'U').toUpperCase());
const hasPhoto = computed(() => Boolean(user.value?.photo));
const inputsReadonly = computed(() => !isEditing.value);

const backLabel = computed(() => {
  const role = localStorage.getItem('userRole');
  if (role === 'operational-staff') return t('profileView.backToStaffHome');
  return t('profileView.backToHome');
});

function syncFormFromUser() {
  if (!user.value) return;
  form.value = {
    name: user.value.name ?? '',
    email: user.value.email ?? '',
    phone: user.value.phone ?? '',
    entityName:
      establishment.value?.establishment_name ??
      iamStore.profileEntityName ??
      '',
    jobTitle: user.value.job_title ?? '',
    dni: user.value.dni ?? '',
    entityCode: iamStore.profileEntityCode ? String(iamStore.profileEntityCode) : '',
  };
}

async function loadProfile() {
  loading.value = true;
  error.value = null;
  try {
    await iamStore.loadProfileFromSession();
    user.value = iamStore.profileUser;
    establishment.value = iamStore.profileEstablishment;
    planApi.value = iamStore.profilePlanApi;
    isEditing.value = false;
    syncFormFromUser();
  } catch (e) {
    console.error(e);
    error.value = t('profileView.loadError');
  } finally {
    loading.value = false;
  }
}

onMounted(loadProfile);

function goHome() {
  const role = localStorage.getItem('userRole');
  if (role === 'health-entity') router.push({ name: 'home-health-entity' });
  else if (role === 'operational-staff') router.push({ name: 'home-operational-staff' });
  else router.push({ name: 'login' });
}

function goPlans() {
  const ctx = subscriptionsStore.readPlanContext() || {};
  subscriptionsStore.writePlanContext({
    ...ctx,
    userId: user.value?.id,
    planApiValue: planApi.value,
    benefitsEndDate: ctx.benefitsEndDate,
    catalogPlanId: Subscription.apiPlanToCatalogId(planApi.value),
  });
  router.push({ name: 'plans' });
}

function startEdit() {
  syncFormFromUser();
  isEditing.value = true;
}

function cancelEdit() {
  syncFormFromUser();
  isEditing.value = false;
}

function saveEdit() {
  if (!user.value) return;
  user.value.name = form.value.name;
  user.value.email = form.value.email;
  user.value.phone = form.value.phone;
  user.value.job_title = form.value.jobTitle;
  if (establishment.value) {
    establishment.value.establishment_name = form.value.entityName;
  }
  isEditing.value = false;
  toast.add({
    severity: 'success',
    summary: t('profileView.editSavedTitle'),
    detail: t('profileView.editSavedDetail'),
    life: 3500,
  });
}
</script>

<template>
  <div class="profile-page">
    <nav v-if="!loading && !error" class="profile-back-bar" aria-label="Navegación">
      <button type="button" class="profile-back-btn" @click="goHome">
        <i class="pi pi-arrow-left" aria-hidden="true"></i>
        <span>{{ backLabel }}</span>
      </button>
    </nav>

    <div v-if="loading" class="profile-card state state--loading">
      <i class="pi pi-spin pi-spinner" aria-hidden="true"></i>
      <span>{{ t('profileView.loading') }}</span>
    </div>

    <div v-else-if="error" class="profile-card state state--error">
      <p>{{ error }}</p>
      <button type="button" class="profile-btn profile-btn--primary" @click="loadProfile">
        {{ t('profileView.retry') }}
      </button>
    </div>

    <div v-else-if="user" class="profile-card">
      <header class="profile-hero">
        <div class="profile-hero__identity">
          <img
            v-if="hasPhoto"
            class="profile-avatar profile-avatar--img"
            :src="user.photo"
            :alt="form.name"
            width="52"
            height="52"
          />
          <span
            v-else
            class="profile-avatar profile-avatar--initial"
          >{{ userInitial }}</span>
          <div class="profile-identity">
            <p class="profile-eyebrow">{{ t('profileView.userProfile') }}</p>
            <h1 class="profile-name">{{ form.name }}</h1>
            <p class="profile-email">{{ form.email }}</p>
          </div>
        </div>
        <aside class="profile-schedule" aria-label="schedule">
          <div class="profile-schedule__head">
            <i class="pi pi-clock" aria-hidden="true"></i>
            <span>{{ t('profileView.scheduleTitle') }}</span>
          </div>
          <table class="schedule-table">
            <thead>
              <tr>
                <th>{{ t('profileView.scheduleShift') }}</th>
                <th>{{ t('profileView.scheduleDay') }}</th>
              </tr>
            </thead>
            <tbody>
              <tr>
                <td>{{ t('profileView.scheduleShiftValue') }}</td>
                <td>{{ t('profileView.scheduleDayValue') }}</td>
              </tr>
            </tbody>
          </table>
        </aside>
      </header>

      <section class="fields-grid">
        <label class="field-block">
          <span class="field-block__label"><i class="pi pi-user" aria-hidden="true"></i> {{ t('profileView.name') }}</span>
          <input
            v-model="form.name"
            type="text"
            class="field-block__input"
            :readonly="inputsReadonly"
            autocomplete="name"
          />
        </label>

        <label class="field-block">
          <span class="field-block__label"><i class="pi pi-building" aria-hidden="true"></i> {{ t('profileView.entity') }}</span>
          <input
            v-model="form.entityName"
            type="text"
            class="field-block__input"
            :readonly="inputsReadonly"
            autocomplete="organization"
          />
        </label>

        <label class="field-block">
          <span class="field-block__label"><i class="pi pi-id-card" aria-hidden="true"></i> {{ t('profileView.dni') }}</span>
          <input
            v-model="form.dni"
            type="text"
            class="field-block__input field-block__input--readonly"
            readonly
          />
        </label>

        <label class="field-block">
          <span class="field-block__label"><i class="pi pi-hashtag" aria-hidden="true"></i> {{ t('profileView.entityCode') }}</span>
          <input
            v-model="form.entityCode"
            type="text"
            class="field-block__input field-block__input--readonly"
            readonly
          />
        </label>

        <label class="field-block">
          <span class="field-block__label"><i class="pi pi-envelope" aria-hidden="true"></i> {{ t('profileView.email') }}</span>
          <input
            v-model="form.email"
            type="email"
            class="field-block__input"
            :readonly="inputsReadonly"
            autocomplete="email"
          />
        </label>

        <label class="field-block">
          <span class="field-block__label"><i class="pi pi-briefcase" aria-hidden="true"></i> {{ t('profileView.jobTitle') }}</span>
          <input
            v-model="form.jobTitle"
            type="text"
            class="field-block__input"
            :readonly="inputsReadonly"
            autocomplete="organization-title"
          />
        </label>

        <label class="field-block">
          <span class="field-block__label"><i class="pi pi-phone" aria-hidden="true"></i> {{ t('profileView.phone') }}</span>
          <input
            v-model="form.phone"
            type="tel"
            class="field-block__input"
            :readonly="inputsReadonly"
            autocomplete="tel"
          />
        </label>

        <label class="field-block">
          <span class="field-block__label"><i class="pi pi-lock" aria-hidden="true"></i> {{ t('profileView.password') }}</span>
          <input
            type="text"
            class="field-block__input field-block__input--readonly"
            value="••••••••"
            readonly
          />
        </label>

        <div class="field-block field-block--plan">
          <span class="field-block__label"><i class="pi pi-credit-card" aria-hidden="true"></i> {{ t('profileView.actualPlan') }}</span>
          <div class="plan-row">
            <div class="plan-chip">{{ planDisplay }}</div>
            <button type="button" class="profile-btn profile-btn--plan" @click="goPlans">
              <i class="pi pi-sync" aria-hidden="true"></i>
              <span>{{ t('profileView.updatePlan') }}</span>
            </button>
          </div>
        </div>
      </section>

      <footer class="toolbar-actions">
        <template v-if="!isEditing">
          <button type="button" class="profile-btn profile-btn--edit" @click="startEdit">
            <i class="pi pi-pencil" aria-hidden="true"></i>
            <span>{{ t('profileView.edit') }}</span>
          </button>
        </template>
        <template v-else>
          <button type="button" class="profile-btn profile-btn--primary" @click="saveEdit">
            <i class="pi pi-check" aria-hidden="true"></i>
            <span>{{ t('profileView.save') }}</span>
          </button>
          <button type="button" class="profile-btn profile-btn--ghost" @click="cancelEdit">
            <span>{{ t('profileView.cancelEdit') }}</span>
          </button>
        </template>
      </footer>
    </div>
  </div>
</template>

<style scoped>
.profile-page {
  width: 100%;
  max-width: 920px;
  margin: 0 auto;
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

.profile-back-bar {
  background: #fff;
  border: 1px solid rgba(17, 36, 51, 0.1);
  border-radius: 14px;
  padding: 0.55rem 0.75rem;
  box-shadow: 0 2px 10px rgba(17, 36, 51, 0.04);
}

.profile-back-btn {
  display: inline-flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.35rem 0.5rem;
  border: none;
  background: transparent;
  color: #112433;
  font-size: 0.875rem;
  font-weight: 600;
  font-family: inherit;
  cursor: pointer;
  border-radius: 8px;
  transition: background 0.15s ease, color 0.15s ease;
}

.profile-back-btn:hover {
  background: rgba(17, 36, 51, 0.06);
  color: #f37021;
}

.profile-back-btn i {
  font-size: 0.8rem;
}

.profile-card {
  background: #fff;
  border-radius: 22px;
  border: 1px solid rgba(17, 36, 51, 0.1);
  box-shadow: 0 12px 36px rgba(17, 36, 51, 0.08);
  padding: 1.45rem 1.5rem 1.3rem;
}

.state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 0.75rem;
  min-height: 180px;
  color: #64748b;
  font-size: 0.875rem;
}

.state--error {
  color: #b91c1c;
}

.profile-hero {
  display: flex;
  flex-wrap: wrap;
  align-items: stretch;
  justify-content: space-between;
  gap: 1rem;
  margin-bottom: 1.2rem;
  padding: 1.05rem 1.15rem;
  border-radius: 16px;
  background:
    linear-gradient(135deg, rgba(243, 112, 33, 0.12) 0%, transparent 45%),
    linear-gradient(120deg, #163247 0%, #112433 100%);
  color: #fff;
}

.profile-hero__identity {
  display: flex;
  align-items: center;
  gap: 0.85rem;
  min-width: 0;
}

.profile-identity {
  min-width: 0;
}

.profile-avatar {
  flex-shrink: 0;
}

.profile-avatar--img,
.profile-avatar--initial {
  width: 4rem;
  height: 4rem;
  border-radius: 50%;
  border: 2px solid rgba(255, 255, 255, 0.35);
}

.profile-avatar--img {
  object-fit: cover;
}

.profile-avatar--initial {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  background: linear-gradient(145deg, #f37021, #e05a12);
  color: #fff;
  font-size: 1.35rem;
  font-weight: 800;
}

.profile-eyebrow {
  margin: 0 0 0.2rem;
  font-size: 0.65rem;
  font-weight: 700;
  letter-spacing: 0.1em;
  text-transform: uppercase;
  color: rgba(255, 255, 255, 0.7);
}

.profile-name {
  margin: 0 0 0.15rem;
  font-size: 1.15rem;
  font-weight: 800;
  color: #fff;
  letter-spacing: -0.02em;
  line-height: 1.25;
}

.profile-email {
  margin: 0;
  font-size: 0.82rem;
  font-weight: 500;
  color: rgba(255, 255, 255, 0.82);
  line-height: 1.35;
}

.profile-schedule {
  min-width: min(100%, 15rem);
  padding: 0.7rem 0.8rem;
  border-radius: 12px;
  background: rgba(255, 255, 255, 0.1);
  border: 1px solid rgba(255, 255, 255, 0.2);
}

.profile-schedule__head {
  display: flex;
  align-items: center;
  gap: 0.35rem;
  margin-bottom: 0.4rem;
  font-size: 0.65rem;
  font-weight: 700;
  letter-spacing: 0.08em;
  text-transform: uppercase;
  color: rgba(255, 255, 255, 0.78);
}

.profile-schedule__head i {
  color: #f37021;
}

.fields-grid {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 0.85rem;
  margin-bottom: 1.15rem;
}

.field-block {
  display: flex;
  flex-direction: column;
  gap: 0.4rem;
  min-width: 0;
}

.field-block--plan {
  grid-column: 1 / -1;
}

.field-block__label {
  display: inline-flex;
  align-items: center;
  gap: 0.35rem;
  font-size: 0.62rem;
  font-weight: 700;
  letter-spacing: 0.07em;
  text-transform: uppercase;
  color: #64748b;
}

.field-block__label i {
  color: #f37021;
  font-size: 0.72rem;
}

.field-block__input {
  width: 100%;
  margin: 0;
  padding: 0.72rem 0.85rem;
  border: 1.5px solid rgba(17, 36, 51, 0.16);
  border-radius: 12px;
  font-size: 0.9rem;
  font-weight: 600;
  color: #112433;
  background: #f8fafc;
  font-family: inherit;
  box-sizing: border-box;
  transition: border-color 0.15s ease, box-shadow 0.15s ease, background 0.15s ease;
}

.field-block__input:hover {
  border-color: rgba(17, 36, 51, 0.28);
}

.field-block__input:focus {
  outline: none;
  background: #fff;
  border-color: #f37021;
  box-shadow: 0 0 0 3px rgba(243, 112, 33, 0.15);
}

.field-block__input:read-only,
.field-block__input--readonly {
  background: #f1f5f9;
  border-color: rgba(17, 36, 51, 0.12);
  color: #334155;
  cursor: default;
}

.plan-row {
  display: flex;
  align-items: center;
  gap: 0.65rem;
  flex-wrap: wrap;
}

.plan-chip {
  display: inline-flex;
  align-items: center;
  min-height: 2.65rem;
  padding: 0.55rem 0.95rem;
  border-radius: 12px;
  border: 1.5px solid rgba(243, 112, 33, 0.3);
  background: #fff7f0;
  color: #c2410c;
  font-size: 0.92rem;
  font-weight: 800;
  letter-spacing: -0.01em;
}

.toolbar-actions {
  display: flex;
  flex-wrap: wrap;
  gap: 0.55rem;
  padding-top: 1rem;
  border-top: 1px solid rgba(17, 36, 51, 0.08);
}

.profile-btn {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 0.45rem;
  min-height: 2.65rem;
  padding: 0.6rem 1.05rem;
  border-radius: 12px;
  font-size: 0.86rem;
  font-weight: 700;
  font-family: inherit;
  cursor: pointer;
  border: none;
  transition: background 0.2s ease, transform 0.15s ease, box-shadow 0.15s ease, border-color 0.15s ease, color 0.15s ease;
}

.profile-btn i {
  font-size: 0.88rem;
}

.profile-btn--primary,
.profile-btn--edit {
  background: #112433;
  color: #fff;
  box-shadow: 0 2px 10px rgba(17, 36, 51, 0.18);
}

.profile-btn--primary:hover,
.profile-btn--edit:hover {
  background: #1a3648;
  transform: translateY(-1px);
  box-shadow: 0 6px 16px rgba(17, 36, 51, 0.22);
}

.profile-btn--plan {
  background: #f37021;
  color: #fff;
  border: none;
  white-space: nowrap;
  box-shadow: 0 2px 10px rgba(243, 112, 33, 0.28);
}

.profile-btn--plan:hover {
  background: #e05a12;
  transform: translateY(-1px);
  box-shadow: 0 6px 16px rgba(243, 112, 33, 0.32);
}

.profile-btn--ghost {
  background: #fff;
  color: #415a77;
  border: 1.5px solid rgba(17, 36, 51, 0.14);
}

.profile-btn--ghost:hover {
  background: #f8fafc;
  color: #112433;
  border-color: rgba(17, 36, 51, 0.28);
}

.schedule-table {
  width: 100%;
  border-collapse: collapse;
  font-size: 0.75rem;
}

.schedule-table th {
  text-align: left;
  font-size: 0.58rem;
  font-weight: 700;
  letter-spacing: 0.05em;
  text-transform: uppercase;
  color: rgba(255, 255, 255, 0.62);
  padding: 0 0.35rem 0.2rem 0;
}

.schedule-table td {
  color: #fff;
  font-weight: 600;
  font-size: 0.75rem;
  padding: 0.1rem 0.35rem 0 0;
}

.schedule-table td:first-child {
  color: #ffd2b0;
}

@media (max-width: 640px) {
  .profile-card {
    padding: 1rem;
    border-radius: 16px;
  }

  .fields-grid {
    grid-template-columns: 1fr;
  }

  .field-block--plan {
    grid-column: auto;
  }

  .plan-row {
    flex-direction: column;
    align-items: stretch;
  }

  .profile-btn {
    width: 100%;
  }
}
</style>
