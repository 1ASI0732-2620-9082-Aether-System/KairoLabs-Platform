<script setup>
import { computed } from 'vue';
import { useRouter } from 'vue-router';
import { useI18n } from 'vue-i18n';
import { readAuthSession } from '../../infrastructure/auth-session.js';

const { t } = useI18n();
const router = useRouter();
const session = computed(() => readAuthSession());

function goLogin() {
  router.push({ name: 'login' });
}
</script>

<template>
  <div class="not-assigned">
    <div class="not-assigned__card">
      <h1>{{ t('iam.notAssigned.title') }}</h1>
      <p>{{ t('iam.notAssigned.message', { admin: session?.adminContactName ?? '—' }) }}</p>
      <p v-if="session?.entityCode" class="not-assigned__code">
        {{ t('iam.notAssigned.entityCode') }}: <strong>{{ session.entityCode }}</strong>
      </p>
      <button type="button" class="btn-primary" @click="goLogin">{{ t('iam.notAssigned.backLogin') }}</button>
    </div>
  </div>
</template>

<style scoped>
.not-assigned {
  min-height: 60vh;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 2rem;
}
.not-assigned__card {
  max-width: 420px;
  padding: 1.5rem;
  background: #fff;
  border: 1px solid #dbe3ef;
  border-radius: 12px;
  text-align: center;
}
.not-assigned__card h1 { margin: 0 0 0.75rem; font-size: 1.2rem; }
.not-assigned__card p { margin: 0 0 0.5rem; color: #64748b; font-size: 0.9rem; line-height: 1.5; }
.not-assigned__code { margin: 1rem 0; }
.btn-primary {
  margin-top: 1rem;
  padding: 0.6rem 1.25rem;
  border: none;
  border-radius: 999px;
  background: #1e3a8a;
  color: #fff;
  font-weight: 700;
  cursor: pointer;
}
</style>
