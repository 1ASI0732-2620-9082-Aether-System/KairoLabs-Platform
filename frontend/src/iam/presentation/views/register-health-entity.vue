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

const form = ref({ name: '', email: '', password: '', entityName: '' });
const loading = ref(false);
const showPassword = ref(false);

async function onSubmit() {
  if (!form.value.name || !form.value.email || !form.value.password || !form.value.entityName) {
    toast.add({ severity: 'error', summary: t('iam.errors.title'), detail: t('iam.errors.required'), life: 4000 });
    return;
  }
  loading.value = true;
  try {
    const result = await iamStore.startHealthEntityRegistration(form.value);
    if (!result.ok) {
      toast.add({
        severity: 'error',
        summary: t('iam.errors.title'),
        detail: t(`iam.errors.${result.error}`),
        life: 4000,
      });
      return;
    }
    router.push({ name: 'iam-register-plans' });
  } catch (e) {
    console.error(e);
    toast.add({ severity: 'error', summary: t('iam.errors.title'), detail: t('iam.errors.network'), life: 4000 });
  } finally {
    loading.value = false;
  }
}
</script>

<template>
  <auth-panel show-back signup @back="router.push({ name: 'iam-login-health-entity' })">
    <div class="reg-compact">
      <header class="reg-compact__head">
        <div class="reg-compact__brand">
          <img src="/logo.png" :alt="t('common.logoAlt')" class="reg-compact__logo" />
          <strong>{{ t('common.brandName') }}</strong>
        </div>
        <p class="reg-compact__badge">{{ t('iam.healthEntity.badge') }}</p>
        <h1 class="reg-compact__title">{{ t('iam.register.getStarted') }}</h1>
        <p class="reg-compact__hint">{{ t('iam.register.healthEntityHint') }}</p>
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
          <span>{{ t('iam.fields.entityNameShort') }}</span>
          <input v-model="form.entityName" type="text" :placeholder="t('iam.placeholders.entityName')" />
        </label>
        <button type="submit" class="btn-primary reg-compact__btn" :disabled="loading">
          {{ t('iam.register.viewPlans') }}
        </button>
      </form>

      <p class="reg-compact__footer">
        {{ t('iam.register.hasAccount') }}
        <button type="button" class="link-btn" @click="router.push({ name: 'iam-login-health-entity' })">
          {{ t('iam.register.signIn') }}
        </button>
      </p>
    </div>
  </auth-panel>
</template>
