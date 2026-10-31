<script setup>
import { ref, onMounted, computed } from 'vue';
import { useRouter } from 'vue-router';
import { useI18n } from 'vue-i18n';
import { useToast } from 'primevue/usetoast';
import useEstablishmentStore from '../../application/establishment.store.js';
import useIamStore from '../../../iam/application/iam.store.js';
import { readAuthSession } from '../../../iam/infrastructure/auth-session.js';

const { t } = useI18n();
const router = useRouter();
const toast = useToast();
const establishmentStore = useEstablishmentStore();
const iamStore = useIamStore();
const establishments = ref([]);
const isLoading = ref(true);
const isSaving = ref(false);

const newEstablishment = ref({
  establishment_name: '',
  establishment_type: 'HOSPITAL',
  city_region: '',
  district: '',
  address: '',
});

const typeOptions = computed(() => [
  { value: 'HOSPITAL', label: t('establishment.typeHospital'), icon: 'pi pi-building' },
  { value: 'WAREHOUSE', label: t('establishment.typeWarehouse'), icon: 'pi pi-box' },
  { value: 'CLINIC', label: t('establishment.typeClinic'), icon: 'pi pi-home' },
]);

const selectPt = {
  root: { class: 'kl-select' },
  overlay: { class: 'kl-select-overlay' },
};

const selectedType = computed(() =>
  typeOptions.value.find((o) => o.value === newEstablishment.value.establishment_type) ?? null,
);

const loadData = async () => {
  isLoading.value = true;
  try {
    establishments.value = await establishmentStore.fetchEstablishmentsAsync();
  } catch (error) {
    console.error('Error loading establishments', error);
  } finally {
    isLoading.value = false;
  }
};

const goBack = () => {
  router.push({ name: 'home-health-entity' });
};

const handleSave = async () => {
  if (isSaving.value) return;

  isSaving.value = true;
  try {
    const nameSlug = newEstablishment.value.establishment_name.toLowerCase().replace(/[^a-z0-9]/g, '');
    const randomPhone = Math.floor(900000000 + Math.random() * 99999999).toString();

    // Resolve admin_id from session
    const session = readAuthSession();
    await iamStore.fetchAdminsAsync();
    const admin = iamStore.admins.find(
      (a) => Number(a.user_id ?? a.users_id) === Number(session?.userId)
    ) || iamStore.admins[0];
    const adminId = admin?.id ?? null;

    if (!adminId) {
      toast.add({ severity: 'error', summary: 'No se encontró el perfil de administrador.', life: 4000 });
      isSaving.value = false;
      return;
    }

    // Map enum to PascalCase (backend uses JsonStringEnumConverter)
    const typeMap = { HOSPITAL: 'Hospital', WAREHOUSE: 'Warehouse', CLINIC: 'Clinic', PHARMACY: 'Pharmacy', LABORATORY: 'Laboratory' };
    const backendType = typeMap[newEstablishment.value.establishment_type] ?? newEstablishment.value.establishment_type;

    const payload = {
      establishment_name: newEstablishment.value.establishment_name,
      establishment_type: backendType,
      address: newEstablishment.value.address,
      district: newEstablishment.value.district,
      city_region: newEstablishment.value.city_region,
      country: 'Perú',
      latitude: parseFloat((-12.04 + (Math.random() - 0.5) * 0.5).toFixed(6)),
      longitude: parseFloat((-77.03 + (Math.random() - 0.5) * 0.5).toFixed(6)),
      website: `www.${nameSlug}.com`,
      email: `${nameSlug}@gmail.com`,
      phone: randomPhone,
      admin_id: adminId,
    };

    await establishmentStore.createEstablishmentAsync(payload);

    toast.add({
      severity: 'success',
      summary: t('establishment.registerSuccess'),
      life: 3000,
    });

    newEstablishment.value = {
      establishment_name: '',
      establishment_type: 'HOSPITAL',
      city_region: '',
      district: '',
      address: '',
    };

    await loadData();
  } catch (error) {
    toast.add({
      severity: 'error',
      summary: t('establishment.registerError'),
      life: 5000,
    });
  } finally {
    isSaving.value = false;
  }
};

onMounted(() => {
  loadData();
});
</script>

<template>
  <div class="est-page">
    <nav class="est-back-bar" aria-label="Navegación">
      <button type="button" class="est-back-btn" @click="goBack">
        <i class="pi pi-arrow-left" aria-hidden="true"></i>
        <span>{{ t('establishment.backToHome') }}</span>
      </button>
    </nav>

    <div class="est-card">
      <header class="est-head">
        <h1 class="est-title">{{ t('establishment.addEstablishment') }}</h1>
        <p class="est-subtitle">{{ t('establishment.addPageSubtitle') }}</p>
      </header>

      <section class="est-section">
        <div class="est-section__head">
          <i class="pi pi-plus-circle" aria-hidden="true"></i>
          <h2>{{ t('establishment.newEstablishmentTitle') }}</h2>
        </div>

        <form class="est-form" @submit.prevent="handleSave">
          <div class="est-form-grid">
            <label class="est-field">
              <span class="est-field__label">{{ t('establishment.fieldName') }}</span>
              <input
                v-model="newEstablishment.establishment_name"
                type="text"
                :placeholder="t('establishment.placeholderName')"
                required
              />
            </label>
            <label class="est-field">
              <span class="est-field__label">{{ t('establishment.fieldType') }}</span>
              <pv-select
                v-model="newEstablishment.establishment_type"
                :options="typeOptions"
                option-label="label"
                option-value="value"
                class="kl-select"
                append-to="body"
                :pt="selectPt"
              >
                <template #value>
                  <span v-if="selectedType" class="kl-select-item">
                    <span class="kl-select-item__icon"><i :class="selectedType.icon" aria-hidden="true"></i></span>
                    <span class="kl-select-item__text">{{ selectedType.label }}</span>
                  </span>
                </template>
                <template #option="{ option }">
                  <span class="kl-select-item">
                    <span class="kl-select-item__icon"><i :class="option.icon" aria-hidden="true"></i></span>
                    <span class="kl-select-item__text">{{ option.label }}</span>
                  </span>
                </template>
              </pv-select>
            </label>
            <label class="est-field">
              <span class="est-field__label">{{ t('establishment.fieldCity') }}</span>
              <input
                v-model="newEstablishment.city_region"
                type="text"
                :placeholder="t('establishment.placeholderCity')"
                required
              />
            </label>
            <label class="est-field">
              <span class="est-field__label">{{ t('establishment.fieldDistrict') }}</span>
              <input
                v-model="newEstablishment.district"
                type="text"
                :placeholder="t('establishment.placeholderDistrict')"
              />
            </label>
            <label class="est-field est-field--full">
              <span class="est-field__label">{{ t('establishment.fieldAddress') }}</span>
              <input
                v-model="newEstablishment.address"
                type="text"
                :placeholder="t('establishment.placeholderAddress')"
              />
            </label>
          </div>

          <div class="est-form-actions">
            <button type="submit" class="est-btn est-btn--primary" :disabled="isSaving">
              <i :class="isSaving ? 'pi pi-spin pi-spinner' : 'pi pi-check'" aria-hidden="true"></i>
              <span>{{ isSaving ? t('establishment.saving') : t('establishment.registerSubmit') }}</span>
            </button>
          </div>
        </form>
      </section>

      <section class="est-section est-section--table">
        <div class="est-section__head">
          <i class="pi pi-database" aria-hidden="true"></i>
          <h2>{{ t('establishment.registeredList') }}</h2>
        </div>

        <div v-if="isLoading" class="est-loader">
          <i class="pi pi-spin pi-spinner" aria-hidden="true"></i>
          <span>{{ t('establishment.loadingList') }}</span>
        </div>

        <div v-else class="est-table-wrap">
          <table class="est-table">
            <thead>
              <tr>
                <th>{{ t('establishment.fieldId') }}</th>
                <th>{{ t('establishment.colEstablishment') }}</th>
                <th>{{ t('establishment.colDigitalContact') }}</th>
                <th>{{ t('establishment.fieldPhone') }}</th>
                <th>{{ t('establishment.colCreatedAt') }}</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="est in establishments" :key="est.id">
                <td class="est-table__id">#{{ est.id }}</td>
                <td>
                  <div class="est-table__name">{{ est.establishment_name }}</div>
                  <div class="est-table__meta">{{ est.city_region }}, {{ est.district }}</div>
                </td>
                <td>
                  <div class="est-table__link">{{ est.website }}</div>
                  <div class="est-table__meta">{{ est.email }}</div>
                </td>
                <td>{{ est.phone || '—' }}</td>
                <td>{{ est.created_at || '—' }}</td>
              </tr>
            </tbody>
          </table>
        </div>
      </section>
    </div>
  </div>
</template>

<style scoped>
.est-page {
  width: 100%;
  max-width: 1080px;
  margin: 0 auto;
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
  padding-bottom: 1.5rem;
}

.est-back-bar {
  background: #fff;
  border: 1px solid var(--mt-border);
  border-radius: 14px;
  padding: 0.55rem 0.75rem;
  box-shadow: 0 2px 10px rgba(15, 23, 42, 0.04);
}

.est-back-btn {
  display: inline-flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.35rem 0.5rem;
  border: none;
  background: transparent;
  color: var(--mt-primary);
  font-size: 0.875rem;
  font-weight: 600;
  font-family: inherit;
  cursor: pointer;
  border-radius: 8px;
  transition: background 0.15s ease, color 0.15s ease;
}

.est-back-btn:hover {
  background: var(--mt-primary-soft);
  color: var(--mt-primary-hover);
}

.est-back-btn i {
  font-size: 0.8rem;
}

.est-card {
  background: #fff;
  border-radius: 22px;
  border: 1px solid var(--mt-border);
  box-shadow: 0 10px 36px rgba(15, 23, 42, 0.07);
  padding: 1.65rem 1.75rem 1.5rem;
}

.est-head {
  text-align: center;
  margin-bottom: 1.35rem;
  padding-bottom: 1.1rem;
  border-bottom: 1px solid var(--mt-border);
}

.est-title {
  margin: 0;
  font-size: clamp(1.25rem, 2.5vw, 1.65rem);
  font-weight: 800;
  color: var(--mt-heading);
  letter-spacing: -0.03em;
}

.est-subtitle {
  margin: 0.45rem 0 0;
  font-size: 0.875rem;
  color: var(--mt-text-muted);
  line-height: 1.45;
}

.est-section {
  padding: 1.15rem 0;
  border-bottom: 1px solid var(--mt-border);
}

.est-section--table {
  border-bottom: none;
  padding-bottom: 0;
}

.est-section__head {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  margin-bottom: 1rem;
  color: var(--mt-primary);
}

.est-section__head i {
  font-size: 1rem;
}

.est-section__head h2 {
  margin: 0;
  font-size: 1rem;
  font-weight: 700;
  color: var(--mt-heading);
}

.est-form-grid {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 0.75rem;
}

.est-field {
  display: flex;
  flex-direction: column;
  gap: 0.3rem;
  min-width: 0;
}

.est-field--full {
  grid-column: 1 / -1;
}

.est-field__label {
  font-size: 0.65rem;
  font-weight: 700;
  letter-spacing: 0.06em;
  text-transform: uppercase;
  color: var(--mt-text-muted);
}

.est-field input {
  width: 100%;
  margin: 0;
  padding: 0.6rem 0.75rem;
  border: 1.5px solid rgba(17, 36, 51, 0.16);
  border-radius: 10px;
  font-size: 0.875rem;
  font-weight: 500;
  color: #112433;
  background: #f8fafc;
  font-family: inherit;
  box-sizing: border-box;
  transition: border-color 0.15s ease, box-shadow 0.15s ease, background 0.15s ease;
}

.est-field input::placeholder {
  color: #94a3b8;
}

.est-field input:focus {
  outline: none;
  background: #fff;
  border-color: #f37021;
  box-shadow: 0 0 0 3px rgba(243, 112, 33, 0.15);
}

.est-form-actions {
  margin-top: 1rem;
}

.est-btn {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 0.45rem;
  padding: 0.6rem 1rem;
  border-radius: 10px;
  font-size: 0.8125rem;
  font-weight: 600;
  font-family: inherit;
  cursor: pointer;
  border: none;
  transition: background 0.2s ease, transform 0.15s ease, box-shadow 0.15s ease;
}

.est-btn--primary {
  background: #f37021;
  color: #fff;
  box-shadow: 0 2px 10px rgba(243, 112, 33, 0.28);
}

.est-btn--primary:hover:not(:disabled) {
  transform: translateY(-1px);
  background: #e05a12;
  box-shadow: 0 4px 14px rgba(243, 112, 33, 0.34);
}

.est-btn--primary:disabled {
  background: #cbd5e1;
  box-shadow: none;
  cursor: not-allowed;
}

.est-table-wrap {
  overflow-x: auto;
  border: 1px solid var(--mt-border);
  border-radius: 12px;
}

.est-table {
  width: 100%;
  border-collapse: collapse;
  min-width: 640px;
}

.est-table th {
  text-align: left;
  padding: 0.65rem 0.85rem;
  background: #f8fafc;
  color: var(--mt-text-muted);
  font-size: 0.62rem;
  font-weight: 700;
  letter-spacing: 0.06em;
  text-transform: uppercase;
  border-bottom: 1px solid var(--mt-border);
}

.est-table td {
  padding: 0.75rem 0.85rem;
  border-bottom: 1px solid var(--mt-border);
  font-size: 0.8125rem;
  color: var(--mt-heading);
  vertical-align: top;
}

.est-table tbody tr:last-child td {
  border-bottom: none;
}

.est-table__id {
  font-weight: 700;
  color: var(--mt-primary);
}

.est-table__name {
  font-weight: 600;
  color: var(--mt-heading);
}

.est-table__meta {
  font-size: 0.72rem;
  color: var(--mt-text-muted);
  margin-top: 0.15rem;
}

.est-table__link {
  color: var(--mt-primary);
  font-weight: 600;
  font-size: 0.78rem;
}

.est-loader {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 0.65rem;
  padding: 2rem;
  color: var(--mt-text-muted);
  font-size: 0.875rem;
}

.est-loader i {
  font-size: 1.5rem;
}

@media (max-width: 768px) {
  .est-card {
    padding: 1.25rem 1rem;
    border-radius: 18px;
  }

  .est-form-grid {
    grid-template-columns: 1fr;
  }

  .est-field--full {
    grid-column: auto;
  }

  .est-btn--primary {
    width: 100%;
  }
}
</style>
