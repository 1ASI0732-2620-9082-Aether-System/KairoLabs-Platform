<script setup>
import { ref, computed, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import { useI18n } from 'vue-i18n';
import { useToast } from 'primevue/usetoast';
import useMonitoringStore from '../../application/monitoring.store.js';
import { readAuthSession } from '../../../iam/infrastructure/auth-session.js';

const props = defineProps({
  embedded: {
    type: Boolean,
    default: false,
  },
});

const emit = defineEmits(['saved', 'cancel']);

const monitoringStore = useMonitoringStore();
const router = useRouter();
const { t } = useI18n();
const toast = useToast();

const isSaving = ref(false);
const previewId = ref('—');

const form = ref({
  type_of_medication: 'Refrigerated',
  exact_location: '',
});

const sensors = ref({
  temperature: true,
  humidity: true,
  light: true,
  co2: true,
  vibration: false,
  door: false,
  pressure: false,
  pm25: false,
});

const sensorDefs = [
  { key: 'temperature', labelKey: 'sensorTemp' },
  { key: 'humidity', labelKey: 'sensorHumidity' },
  { key: 'light', labelKey: 'sensorLight' },
  { key: 'co2', labelKey: 'sensorCo2' },
  { key: 'vibration', labelKey: 'sensorVibration' },
  { key: 'door', labelKey: 'sensorDoor' },
  { key: 'pressure', labelKey: 'sensorPressure' },
  { key: 'pm25', labelKey: 'sensorPm25' },
];

onMounted(async () => {
  await monitoringStore.fetchDevicesAsync();
  const list = monitoringStore.devices;
  const max = list.reduce((m, d) => Math.max(m, Number(d.id) || 0), 0);
  previewId.value = String(max + 1);
});

const medicationOptions = computed(() => [
  { value: 'Refrigerated', label: t('monitoring.medicationVaccines'), icon: 'pi pi-shield' },
  { value: 'Biological', label: t('monitoring.medicationPills'), icon: 'pi pi-circle' },
  { value: 'Controlled', label: t('monitoring.medicationCreams'), icon: 'pi pi-box' },
  { value: 'General', label: t('monitoring.medicationSyrup'), icon: 'pi pi-filter' },
]);

const selectPt = {
  root: { class: 'kl-select' },
  overlay: { class: 'kl-select-overlay' },
};

const selectedMedication = computed(() =>
  medicationOptions.value.find((o) => o.value === form.value.type_of_medication) ?? null,
);

function buildPayload() {
  const session = readAuthSession();
  const establishmentId = Number(session?.establishmentId) || 1;

  return {
    exact_location: form.value.exact_location.trim(),
    type_of_medication: form.value.type_of_medication,
    establishment_id: establishmentId,
    enabled_sensors: JSON.stringify(sensors.value),
  };
}

async function handleConfirm() {
  if (!form.value.exact_location.trim()) return;
  if (isSaving.value) return;

  isSaving.value = true;
  try {
    const created = await monitoringStore.createDeviceAsync(buildPayload());
    toast.add({
      severity: 'success',
      summary: t('monitoring.createSuccess'),
      life: 3000,
    });
    if (props.embedded) {
      emit('saved', created);
    } else {
      router.push({ name: 'device-detail', params: { deviceId: String(created.id) } });
    }
  } catch {
    toast.add({
      severity: 'error',
      summary: t('monitoring.createError'),
      life: 5000,
    });
  } finally {
    isSaving.value = false;
  }
}

function goBack() {
  if (props.embedded) {
    emit('cancel');
    return;
  }
  router.push({ name: 'devices' });
}
</script>

<template>
  <div :class="embedded ? 'device-form-embed' : 'est-flow-page'">
    <nav v-if="!embedded" class="est-flow-back-bar" aria-label="Navegación">
      <button type="button" class="est-flow-back-btn" @click="goBack">
        <i class="pi pi-arrow-left" aria-hidden="true"></i>
        <span>{{ t('monitoring.backToDevices') }}</span>
      </button>
    </nav>

    <div :class="embedded ? 'device-form-embed__body' : 'est-flow-card'">
      <header v-if="!embedded" class="est-flow-head">
        <h1 class="est-flow-title">{{ t('monitoring.addDeviceTitle') }}</h1>
        <p class="est-flow-subtitle">{{ t('monitoring.addDeviceSubtitle') }}</p>
      </header>

      <form class="est-flow-fields est-flow-fields--span" @submit.prevent="handleConfirm">
        <div class="est-flow-field">
          <span class="est-flow-field__label">{{ t('monitoring.fieldId') }}</span>
          <span class="est-flow-field__value">{{ previewId }}</span>
        </div>
        <div class="est-flow-field">
          <span class="est-flow-field__label">{{ t('monitoring.fieldMedication') }} *</span>
          <pv-select
            v-model="form.type_of_medication"
            :options="medicationOptions"
            option-label="label"
            option-value="value"
            class="kl-select"
            append-to="body"
            :pt="selectPt"
          >
            <template #value>
              <span v-if="selectedMedication" class="kl-select-item">
                <span class="kl-select-item__icon"><i :class="selectedMedication.icon" aria-hidden="true"></i></span>
                <span class="kl-select-item__text">{{ selectedMedication.label }}</span>
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
        <div class="est-flow-field est-flow-field--full">
          <span class="est-flow-field__label">{{ t('monitoring.fieldExactLocation') }} *</span>
          <input
            v-model="form.exact_location"
            type="text"
            class="est-field-input"
            :placeholder="t('monitoring.placeholderLocation')"
            required
          />
        </div>

        <div class="est-flow-field est-flow-field--full">
          <p class="mon-hint">{{ t('monitoring.sensorsHint') }}</p>
          <div class="mon-sensors-grid">
            <label
              v-for="def in sensorDefs"
              :key="def.key"
              class="mon-sensor-toggle"
            >
              <span class="mon-sensor-toggle__label">{{ t(`monitoring.${def.labelKey}`) }}</span>
              <input v-model="sensors[def.key]" type="checkbox" />
              <span class="mon-switch" aria-hidden="true"></span>
            </label>
          </div>
        </div>

        <footer class="est-flow-actions" style="border-top: none; padding-top: 0; margin-top: 0.5rem">
          <button type="button" class="est-flow-btn est-flow-btn--ghost" @click="goBack">
            <i class="pi pi-times" aria-hidden="true"></i>
            <span>{{ embedded ? t('common.cancel') : t('monitoring.back') }}</span>
          </button>
          <button type="submit" class="est-flow-btn est-flow-btn--accent" :disabled="isSaving">
            <i :class="isSaving ? 'pi pi-spin pi-spinner' : 'pi pi-check'" aria-hidden="true"></i>
            <span>{{ isSaving ? t('monitoring.saving') : t('monitoring.confirm') }}</span>
          </button>
        </footer>
      </form>
    </div>
  </div>
</template>

<style scoped>
.device-form-embed__body {
  padding: 0;
}

.est-field-input {
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
}

.est-field-input:focus {
  outline: none;
  background: #fff;
  border-color: #f37021;
  box-shadow: 0 0 0 3px rgba(243, 112, 33, 0.15);
}
</style>
