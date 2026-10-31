<script setup>
import { ref, computed, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import { useI18n } from 'vue-i18n';
import { useToast } from 'primevue/usetoast';
import useLogisticsStore from '../../application/logistics.store.js';
import useEstablishmentStore from '../../../establishment/application/establishment.store.js';
import { readAuthSession } from '../../../iam/infrastructure/auth-session.js';

const props = defineProps({
  embedded: {
    type: Boolean,
    default: false,
  },
});

const emit = defineEmits(['saved', 'cancel']);

const logisticsStore = useLogisticsStore();
const establishmentStore = useEstablishmentStore();
const router = useRouter();
const { t } = useI18n();
const toast = useToast();

const isSaving = ref(false);
const previewId = ref('—');
const establishmentOptions = ref([]);
const selectedEstablishmentId = ref(null);
const noEstablishment = ref(false);

const form = ref({
  type_of_transport: 'Van',
  type_of_medication: 'Refrigerated',
});

const sensors = ref({
  temperature: true,
  humidity: true,
  light: true,
  co2: true,
  vibration: false,
  door: true,
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
  try {
    await logisticsStore.fetchTransportsAsync();
    const all = await establishmentStore.fetchEstablishmentsAsync();
    const session = readAuthSession();
    const adminId = session?.adminId;
    const owned = adminId
      ? all.filter((e) => Number(e.admin_id) === Number(adminId))
      : all;
    establishmentOptions.value = owned.length ? owned : all;
    selectedEstablishmentId.value = establishmentOptions.value[0]?.id ?? null;
    noEstablishment.value = !selectedEstablishmentId.value;

    const list = logisticsStore.transports;
    const max = list.reduce((m, tr) => Math.max(m, Number(tr.id) || 0), 0);
    previewId.value = String(max + 1);
  } catch (e) {
    console.error('transport-form onMounted error:', e);
    previewId.value = '—';
    noEstablishment.value = true;
  }
});

const transportOptions = computed(() => [
  { value: 'Van', label: t('logistics.transportVan'), icon: 'pi pi-car' },
  { value: 'OffRoad', label: t('logistics.transportOffRoad'), icon: 'pi pi-compass' },
  { value: 'Motorcycle', label: t('logistics.transportMotorcycle'), icon: 'pi pi-send' },
  { value: 'Refrigerated', label: t('logistics.transportRefrigerated'), icon: 'pi pi-box' },
  { value: 'ColdChain', label: t('logistics.transportColdChain'), icon: 'pi pi-link' },
]);

const medicationOptions = computed(() => [
  { value: 'Refrigerated', label: t('logistics.medicationVaccines'), icon: 'pi pi-shield' },
  { value: 'Biological', label: t('logistics.medicationPills'), icon: 'pi pi-circle' },
  { value: 'Controlled', label: t('logistics.medicationCreams'), icon: 'pi pi-box' },
  { value: 'General', label: t('logistics.medicationSyrup'), icon: 'pi pi-filter' },
]);

const establishmentSelectOptions = computed(() =>
  establishmentOptions.value.map((est) => ({
    value: est.id,
    label: est.establishment_name,
    icon: 'pi pi-building',
  })),
);

const selectPt = {
  root: { class: 'kl-select' },
  overlay: { class: 'kl-select-overlay' },
};

const selectedTransport = computed(() =>
  transportOptions.value.find((o) => o.value === form.value.type_of_transport) ?? null,
);
const selectedMedication = computed(() =>
  medicationOptions.value.find((o) => o.value === form.value.type_of_medication) ?? null,
);
const selectedEstablishment = computed(() =>
  establishmentSelectOptions.value.find((o) => o.value === selectedEstablishmentId.value) ?? null,
);

function buildPayload() {
  const establishmentId = Number(selectedEstablishmentId.value);
  if (!establishmentId) {
    throw new Error('noEstablishment');
  }
  return {
    type_of_transport: form.value.type_of_transport,
    type_of_medication: form.value.type_of_medication,
    establishment_id: establishmentId,
    enabled_sensors: JSON.stringify(sensors.value),
  };
}

async function handleConfirm() {
  if (isSaving.value) return;
  if (!selectedEstablishmentId.value) {
    toast.add({
      severity: 'warn',
      summary: t('logistics.createError'),
      detail: t('logistics.noEstablishmentForTransport'),
      life: 6000,
    });
    return;
  }
  isSaving.value = true;
  try {
    const created = await logisticsStore.createTransportAsync(buildPayload());
    toast.add({
      severity: 'success',
      summary: t('logistics.createSuccess'),
      life: 3000,
    });
    if (props.embedded) {
      emit('saved', created);
    } else {
      router.push({ name: 'transport-detail', params: { transportId: String(created.id) } });
    }
  } catch (e) {
    const detail = e?.message === 'noEstablishment'
      ? t('logistics.noEstablishmentForTransport')
      : t('logistics.createError');
    toast.add({
      severity: 'error',
      summary: t('logistics.createError'),
      detail,
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
  router.push({ name: 'transports' });
}
</script>

<template>
  <div :class="embedded ? 'transport-form-embed' : 'est-flow-page'">
    <nav v-if="!embedded" class="est-flow-back-bar" aria-label="Navegación">
      <button type="button" class="est-flow-back-btn" @click="goBack">
        <i class="pi pi-arrow-left" aria-hidden="true"></i>
        <span>{{ t('logistics.backToTransports') }}</span>
      </button>
    </nav>

    <div :class="embedded ? 'transport-form-embed__body' : 'est-flow-card'">
      <header v-if="!embedded" class="est-flow-head">
        <h1 class="est-flow-title">{{ t('logistics.addTransportTitle') }}</h1>
        <p class="est-flow-subtitle">{{ t('logistics.addTransportSubtitle') }}</p>
      </header>

      <form class="est-flow-fields est-flow-fields--span" @submit.prevent="handleConfirm">
        <div class="est-flow-field">
          <span class="est-flow-field__label">{{ t('logistics.fieldId') }}</span>
          <span class="est-flow-field__value">{{ previewId }}</span>
        </div>
        <div class="est-flow-field">
          <span class="est-flow-field__label">{{ t('logistics.fieldTransportType') }} *</span>
          <pv-select
            v-model="form.type_of_transport"
            :options="transportOptions"
            option-label="label"
            option-value="value"
            class="kl-select"
            append-to="body"
            :pt="selectPt"
          >
            <template #value>
              <span v-if="selectedTransport" class="kl-select-item">
                <span class="kl-select-item__icon"><i :class="selectedTransport.icon" aria-hidden="true"></i></span>
                <span class="kl-select-item__text">{{ selectedTransport.label }}</span>
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
          <span class="est-flow-field__label">{{ t('logistics.fieldMedication') }} *</span>
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
          <span class="est-flow-field__label">{{ t('logistics.fieldEstablishment') }} *</span>
          <pv-select
            v-if="establishmentSelectOptions.length"
            v-model="selectedEstablishmentId"
            :options="establishmentSelectOptions"
            option-label="label"
            option-value="value"
            class="kl-select"
            append-to="body"
            :pt="selectPt"
          >
            <template #value>
              <span v-if="selectedEstablishment" class="kl-select-item">
                <span class="kl-select-item__icon"><i :class="selectedEstablishment.icon" aria-hidden="true"></i></span>
                <span class="kl-select-item__text">{{ selectedEstablishment.label }}</span>
              </span>
            </template>
            <template #option="{ option }">
              <span class="kl-select-item">
                <span class="kl-select-item__icon"><i :class="option.icon" aria-hidden="true"></i></span>
                <span class="kl-select-item__text">{{ option.label }}</span>
              </span>
            </template>
          </pv-select>
          <p v-else class="mon-hint mon-hint--warn">{{ t('logistics.noEstablishmentForTransport') }}</p>
        </div>

        <div class="est-flow-field est-flow-field--full">
          <p class="mon-hint">{{ t('logistics.sensorsHint') }}</p>
          <div class="mon-sensors-grid">
            <label v-for="def in sensorDefs" :key="def.key" class="mon-sensor-toggle">
              <span class="mon-sensor-toggle__label">{{ t(`logistics.${def.labelKey}`) }}</span>
              <input v-model="sensors[def.key]" type="checkbox" />
              <span class="mon-switch" aria-hidden="true"></span>
            </label>
          </div>
        </div>

        <footer class="est-flow-actions" style="border-top: none; padding-top: 0; margin-top: 0.5rem">
          <button type="button" class="est-flow-btn est-flow-btn--ghost" @click="goBack">
            <i class="pi pi-times" aria-hidden="true"></i>
            <span>{{ embedded ? t('common.cancel') : t('logistics.back') }}</span>
          </button>
          <button type="submit" class="est-flow-btn est-flow-btn--accent" :disabled="isSaving || noEstablishment">
            <i :class="isSaving ? 'pi pi-spin pi-spinner' : 'pi pi-check'" aria-hidden="true"></i>
            <span>{{ isSaving ? t('logistics.saving') : t('logistics.confirm') }}</span>
          </button>
        </footer>
      </form>
    </div>
  </div>
</template>

<style scoped>
.mon-hint--warn {
  color: #b45309;
  font-weight: 600;
}
</style>
