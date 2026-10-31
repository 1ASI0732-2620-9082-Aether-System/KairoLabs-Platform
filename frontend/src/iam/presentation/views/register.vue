<script setup>
import { computed, ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { useI18n } from 'vue-i18n';
import { useToast } from 'primevue/usetoast';
import useIamStore from '../../application/iam.store.js';

const { t } = useI18n();
const route = useRoute();
const router = useRouter();
const toast = useToast();
const iamStore = useIamStore();

const initialRole =
  route.query.role === 'operational-staff' ? 'operational-staff' : 'health-entity';
const role = ref(initialRole);
const loading = ref(false);
const showPassword = ref(false);
const form = ref({
  name: '',
  email: '',
  password: '',
  entityName: '',
  entityCode: '',
});
const fieldErrors = ref({});

const isManager = computed(() => role.value === 'health-entity');

function selectRole(next) {
  role.value = next;
  fieldErrors.value = {};
}

async function onSubmit() {
  fieldErrors.value = {};
  if (!form.value.name) fieldErrors.value.name = t('iam.errors.requiredField');
  if (!form.value.email) fieldErrors.value.email = t('iam.errors.requiredField');
  if (!form.value.password) fieldErrors.value.password = t('iam.errors.requiredField');
  if (form.value.password && form.value.password.length < 6) {
    fieldErrors.value.password = t('iam.errors.weakPassword');
  }
  if (isManager.value && !form.value.entityName) {
    fieldErrors.value.entityName = t('iam.errors.requiredField');
  }
  if (!isManager.value && !form.value.entityCode) {
    fieldErrors.value.entityCode = t('iam.errors.requiredField');
  }
  if (Object.keys(fieldErrors.value).length) {
    toast.add({ severity: 'error', summary: t('iam.errors.title'), detail: t('iam.errors.required'), life: 4000 });
    return;
  }

  loading.value = true;
  try {
    if (isManager.value) {
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
      return;
    }

    const result = await iamStore.registerOperationalStaff(form.value);
    if (!result.ok) {
      toast.add({
        severity: 'error',
        summary: t('iam.errors.title'),
        detail: t(`iam.errors.${result.error}`),
        life: 4000,
      });
      return;
    }
    toast.add({
      severity: 'success',
      summary: t('iam.register.operatorSuccessTitle'),
      detail: t('iam.register.operatorSuccessDetail', { entity: result.entityName ?? '' }),
      life: 6000,
    });
    router.push({ name: 'login' });
  } catch (e) {
    console.error(e);
    toast.add({ severity: 'error', summary: t('iam.errors.title'), detail: t('iam.errors.network'), life: 4000 });
  } finally {
    loading.value = false;
  }
}
</script>

<template>
  <div class="reg-compact">
      <header class="reg-compact__head">
        <h1 class="reg-compact__title">{{ t('iam.register.getStarted') }}</h1>
        <p class="reg-compact__hint">{{ t('iam.register.chooseRoleHint') }}</p>
      </header>

      <div class="role-toggle" role="tablist">
        <span class="role-toggle__glider" :class="{ 'is-right': !isManager }" aria-hidden="true"></span>
        <button
          type="button"
          role="tab"
          class="role-toggle__btn"
          :class="{ 'is-active': isManager }"
          :aria-selected="isManager"
          @click="selectRole('health-entity')"
        >
          <i class="pi pi-building" aria-hidden="true"></i>
          {{ t('roles.healthEntity') }}
        </button>
        <button
          type="button"
          role="tab"
          class="role-toggle__btn"
          :class="{ 'is-active': !isManager }"
          :aria-selected="!isManager"
          @click="selectRole('operational-staff')"
        >
          <i class="pi pi-box" aria-hidden="true"></i>
          {{ t('roles.operationalStaff') }}
        </button>
      </div>
      <Transition name="role-appear" mode="out-in">
        <p :key="role" class="role-toggle__desc">
          {{ isManager ? t('roles.healthEntityDesc') : t('roles.operationalStaffDesc') }}
        </p>
      </Transition>

      <form class="reg-compact__form reg-compact__form--signup" @submit.prevent="onSubmit">
        <label class="reg-compact__field">
          <span>{{ t('iam.fields.name') }}</span>
          <input v-model="form.name" type="text" :placeholder="t('iam.placeholders.name')" />
          <small v-if="fieldErrors.name" class="reg-compact__error">{{ fieldErrors.name }}</small>
        </label>
        <label class="reg-compact__field">
          <span>{{ t('iam.fields.email') }}</span>
          <input v-model="form.email" type="email" :placeholder="t('iam.placeholders.email')" />
          <small v-if="fieldErrors.email" class="reg-compact__error">{{ fieldErrors.email }}</small>
        </label>
        <label class="reg-compact__field">
          <span>{{ t('iam.fields.password') }}</span>
          <div class="reg-compact__pw">
            <input
              v-model="form.password"
              :type="showPassword ? 'text' : 'password'"
              :placeholder="t('iam.placeholders.password')"
            />
            <button type="button" class="reg-compact__eye" @click="showPassword = !showPassword">
              <i :class="showPassword ? 'pi pi-eye-slash' : 'pi pi-eye'"></i>
            </button>
          </div>
          <small v-if="fieldErrors.password" class="reg-compact__error">{{ fieldErrors.password }}</small>
        </label>
        <div class="reg-compact__swap">
          <Transition name="role-appear" mode="out-in">
            <label v-if="isManager" key="entity-name" class="reg-compact__field">
              <span>{{ t('iam.fields.entityNameShort') }}</span>
              <input v-model="form.entityName" type="text" :placeholder="t('iam.placeholders.entityName')" />
              <small v-if="fieldErrors.entityName" class="reg-compact__error">{{ fieldErrors.entityName }}</small>
            </label>
            <label v-else key="entity-code" class="reg-compact__field">
              <span>{{ t('iam.fields.entityCode') }}</span>
              <input v-model="form.entityCode" type="text" :placeholder="t('iam.placeholders.entityCode')" />
              <small class="reg-compact__muted">{{ t('iam.register.entityCodeExamplesShort') }}</small>
              <small v-if="fieldErrors.entityCode" class="reg-compact__error">{{ fieldErrors.entityCode }}</small>
            </label>
          </Transition>
        </div>
        <button type="submit" class="btn-primary reg-compact__btn" :disabled="loading">
          <span>{{ isManager ? t('iam.register.viewPlans') : (loading ? '…' : t('iam.register.createAccount')) }}</span>
        </button>
      </form>

      <p class="reg-compact__footer">
        {{ t('iam.register.hasAccount') }}
        <button type="button" class="link-btn" @click="router.push({ name: 'login' })">
          {{ t('iam.register.signIn') }}
        </button>
      </p>
  </div>
</template>

<style scoped>
.role-toggle {
  position: relative;
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 0.45rem;
  padding: 0.3rem;
  margin: 0 0 0.65rem;
  border-radius: 999px;
  background: #f4f1ec;
}

.role-toggle__glider {
  position: absolute;
  top: 0.3rem;
  bottom: 0.3rem;
  left: 0.3rem;
  width: calc((100% - 0.6rem - 0.45rem) / 2);
  border-radius: 999px;
  background: #112433;
  transition: transform 0.34s cubic-bezier(0.22, 1, 0.36, 1);
}

.role-toggle__glider.is-right {
  transform: translateX(calc(100% + 0.45rem));
}

.role-toggle__btn {
  position: relative;
  z-index: 1;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 0.4rem;
  min-height: 2.5rem;
  padding: 0.45rem 0.7rem;
  border: none;
  border-radius: 999px;
  background: transparent;
  color: #5b6b7a;
  font-family: inherit;
  font-size: 0.78rem;
  font-weight: 700;
  cursor: pointer;
  transition: color 0.28s ease;
}

.role-toggle__btn.is-active {
  background: transparent;
  color: #fff;
}

.role-toggle__desc {
  margin: 0 0 0.7rem;
  min-height: 2.1em;
  text-align: center;
  font-size: 0.72rem;
  line-height: 1.35;
  color: #6b7785;
}

.role-appear-enter-active,
.role-appear-leave-active {
  transition: opacity 0.28s ease, transform 0.28s ease;
}

.role-appear-enter-from {
  opacity: 0;
  transform: translateX(16px);
}

.role-appear-leave-to {
  opacity: 0;
  transform: translateX(-12px);
}

@media (prefers-reduced-motion: reduce) {
  .role-toggle__glider,
  .role-appear-enter-active,
  .role-appear-leave-active {
    transition: none;
  }
}

@media (max-width: 420px) {
  .role-toggle {
    border-radius: 16px;
  }

  .role-toggle__glider {
    border-radius: 12px;
  }

  .role-toggle__btn {
    flex-direction: column;
    gap: 0.2rem;
    min-height: 3rem;
    font-size: 0.68rem;
    border-radius: 12px;
    padding: 0.35rem 0.4rem;
  }

  .role-toggle__btn i {
    font-size: 0.85rem;
  }
}
</style>
