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

const email = ref('');
const password = ref('');
const showPassword = ref(false);
const loading = ref(false);
const errors = ref({ email: '', password: '' });

async function onSubmit() {
  errors.value = { email: '', password: '' };
  if (!email.value || !password.value) {
    toast.add({ severity: 'error', summary: t('iam.errors.title'), detail: t('iam.errors.required'), life: 4000 });
    return;
  }
  loading.value = true;
  try {
    const result = await iamStore.loginOperationalStaff(email.value, password.value);
    if (!result.ok) {
      const msg = t(`iam.errors.${result.error}`);
      errors.value.email = msg;
      errors.value.password = msg;
      toast.add({ severity: 'error', summary: t('iam.errors.title'), detail: msg, life: 4000 });
      return;
    }
    if (result.notAssigned) {
      toast.add({
        severity: 'warn',
        summary: t('iam.login.pendingAssignmentTitle'),
        detail: t('iam.login.pendingAssignmentDetail', { admin: result.adminContact }),
        life: 5000,
      });
      router.push({ name: 'iam-not-assigned' });
      return;
    }
    toast.add({
      severity: 'success',
      summary: t('iam.login.successTitle'),
      detail: t('iam.login.successDetail'),
      life: 2500,
    });
    router.push({ name: 'home-operational-staff' });
  } catch (e) {
    console.error(e);
    toast.add({ severity: 'error', summary: t('iam.errors.title'), detail: t('iam.errors.network'), life: 4000 });
  } finally {
    loading.value = false;
  }
}
</script>

<template>
  <auth-panel show-back @back="router.push({ name: 'login' })">
    <div class="reg-compact">
      <header class="reg-compact__head">
        <div class="reg-compact__brand">
          <img src="/logo.png" :alt="t('common.logoAlt')" class="reg-compact__logo" />
          <strong>{{ t('common.brandName') }}</strong>
        </div>
        <p class="reg-compact__badge">{{ t('iam.operational.badge') }}</p>
        <h1 class="reg-compact__title">{{ t('iam.login.welcome') }}</h1>
        <p class="reg-compact__hint">{{ t('iam.login.credentialsHint') }}</p>
      </header>

      <form class="reg-compact__form" @submit.prevent="onSubmit">
        <label class="reg-compact__field">
          <span>{{ t('iam.fields.email') }}</span>
          <input
            v-model="email"
            type="email"
            :placeholder="t('iam.placeholders.email')"
            :class="{ 'input-invalid': errors.email }"
          />
          <small v-if="errors.email" class="reg-compact__error">{{ errors.email }}</small>
        </label>

        <label class="reg-compact__field">
          <span>{{ t('iam.fields.password') }}</span>
          <div class="reg-compact__pw">
            <input
              v-model="password"
              :type="showPassword ? 'text' : 'password'"
              :placeholder="t('iam.placeholders.password')"
              :class="{ 'input-invalid': errors.password }"
            />
            <button
              type="button"
              class="reg-compact__eye"
              @click="showPassword = !showPassword"
            >
              <i :class="showPassword ? 'pi pi-eye-slash' : 'pi pi-eye'"></i>
            </button>
          </div>
          <small v-if="errors.password" class="reg-compact__error">{{ errors.password }}</small>
        </label>

        <button type="submit" class="btn-primary reg-compact__btn" :disabled="loading">
          {{ loading ? t('iam.login.loading') : t('iam.login.submit') }}
        </button>
      </form>

      <p class="reg-compact__footer">
        {{ t('iam.login.noAccount') }}
        <button type="button" class="link-btn" @click="router.push({ name: 'iam-register-operational-staff' })">
          {{ t('iam.login.signUp') }}
        </button>
      </p>
    </div>
  </auth-panel>
</template>
