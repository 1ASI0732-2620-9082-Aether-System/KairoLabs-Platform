<script setup>
import { ref } from 'vue';
import { useRouter } from 'vue-router';
import { useI18n } from 'vue-i18n';
import { useToast } from 'primevue/usetoast';
import AuthPanel from '../components/auth-panel.vue';
import useIamStore from '../../application/iam.store.js';

const { t } = useI18n();
const router = useRouter();
const toast = useToast();
const iamStore = useIamStore();

const form = ref({ name: '', email: '', password: '', entityCode: '' });
const loading = ref(false);
const showPassword = ref(false);
const errors = ref({ name: '', email: '', password: '', entityCode: '' });

async function onSubmit() {
  errors.value = { name: '', email: '', password: '', entityCode: '' };
  if (!form.value.name) errors.value.name = t('iam.errors.requiredField');
  if (!form.value.email) errors.value.email = t('iam.errors.requiredField');
  if (!form.value.password) errors.value.password = t('iam.errors.requiredField');
  if (!form.value.entityCode) errors.value.entityCode = t('iam.errors.requiredField');
  if (form.value.password?.length < 6) errors.value.password = t('iam.errors.weakPassword');
  if (Object.values(errors.value).some(Boolean)) {
    toast.add({ severity: 'error', summary: t('iam.errors.title'), detail: t('iam.errors.required'), life: 4000 });
    return;
  }
  loading.value = true;
  try {
    const result = await iamStore.registerOperationalStaff(form.value);
    if (!result.ok) {
      toast.add({ severity: 'error', summary: t('iam.errors.title'), detail: t(`iam.errors.${result.error}`), life: 4000 });
      return;
    }
    toast.add({
      severity: 'success',
      summary: t('iam.register.operatorSuccessTitle'),
      detail: t('iam.register.operatorSuccessDetail', { entity: result.entityName ?? '' }),
      life: 6000,
    });
    router.push({ name: 'iam-login-operational-staff' });
  } catch (e) {
    console.error(e);
    toast.add({ severity: 'error', summary: t('iam.errors.title'), detail: t('iam.errors.network'), life: 4000 });
  } finally {
    loading.value = false;
  }
}
</script>

<template>
  <!-- compact = formulario pequeño; SIN single-column = panel lateral visible -->
  <auth-panel show-back signup @back="router.push({ name: 'iam-login-operational-staff' })">
    <div class="reg-compact">
      <header class="reg-compact__head">
        <div class="reg-compact__brand">
          <img src="/logo.png" :alt="t('common.logoAlt')" class="reg-compact__logo" />
          <strong>{{ t('common.brandName') }}</strong>
        </div>
        <p class="reg-compact__badge">{{ t('iam.operational.badge') }}</p>
        <h1 class="reg-compact__title">{{ t('iam.register.getStarted') }}</h1>
        <p class="reg-compact__hint">{{ t('iam.register.operatorCodeHint') }}</p>
      </header>

      <form class="reg-compact__form reg-compact__form--signup" @submit.prevent="onSubmit">
        <label class="reg-compact__field">
          <span>{{ t('iam.fields.name') }}</span>
          <input v-model="form.name" type="text" :placeholder="t('iam.placeholders.name')" />
        </label>
        <label class="reg-compact__field">
          <span>{{ t('iam.fields.email') }}</span>
          <input v-model="form.email" type="email" :placeholder="t('iam.placeholders.email')" />
        </label>
        <label class="reg-compact__field">
          <span>{{ t('iam.fields.password') }}</span>
          <div class="reg-compact__pw">
            <input v-model="form.password" :type="showPassword ? 'text' : 'password'" :placeholder="t('iam.placeholders.password')" />
            <button type="button" class="reg-compact__eye" @click="showPassword = !showPassword">
              <i :class="showPassword ? 'pi pi-eye-slash' : 'pi pi-eye'"></i>
            </button>
          </div>
        </label>
        <label class="reg-compact__field">
          <span>{{ t('iam.fields.entityCode') }}</span>
          <input v-model="form.entityCode" type="text" :placeholder="t('iam.placeholders.entityCode')" />
          <small class="reg-compact__muted">{{ t('iam.register.entityCodeExamplesShort') }}</small>
        </label>
        <button type="submit" class="btn-primary reg-compact__btn" :disabled="loading">
          {{ loading ? '…' : t('iam.register.createAccount') }}
        </button>
      </form>

      <p class="reg-compact__footer">
        {{ t('iam.register.hasAccount') }}
        <button type="button" class="link-btn" @click="router.push({ name: 'iam-login-operational-staff' })">
          {{ t('iam.register.signIn') }}
        </button>
      </p>
    </div>
  </auth-panel>
</template>
