<script setup>
import {useRouter} from "vue-router";
import {useI18n} from "vue-i18n";

const router = useRouter();
const unavailableRoute = router.currentRoute.value.fullPath;
const { t } = useI18n();
</script>

<template>
  <div class="not-found-page">
    <div class="not-found-card">
      <div class="not-found-code" aria-hidden="true">404</div>
      <h1 class="not-found-title">{{ t('page-not-found.title')}}</h1>
      <p class="not-found-body">{{ t('page-not-found.content', { 'unavailable-route': unavailableRoute })}}</p>
      <router-link class="not-found-link" to="/home">{{ t('page-not-found.go-home') }}</router-link>
    </div>
  </div>
</template>

<style scoped>
.not-found-page {
  min-height: calc(100vh - 120px);
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 2rem 1rem;
}

.not-found-card {
  width: 100%;
  max-width: 28rem;
  text-align: center;
  padding: 2.5rem 2rem;
  background: #fff;
  border-radius: 20px;
  border: 1px solid #e2e8f0;
  box-shadow: 0 4px 6px rgba(15, 23, 42, 0.04), 0 24px 56px rgba(15, 23, 42, 0.1);
}

.not-found-code {
  font-size: 3.5rem;
  font-weight: 800;
  letter-spacing: -0.06em;
  line-height: 1;
  background: linear-gradient(135deg, #1e3a8a 0%, #0d9488 100%);
  -webkit-background-clip: text;
  background-clip: text;
  color: transparent;
  margin-bottom: 0.75rem;
}

.not-found-title {
  margin: 0 0 1rem;
  font-size: 1.35rem;
  font-weight: 700;
  color: #0f172a;
  letter-spacing: -0.02em;
}

.not-found-body {
  margin: 0 0 1.75rem;
  font-size: 0.95rem;
  line-height: 1.55;
  color: #64748b;
  word-break: break-all;
}

.not-found-link {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  padding: 0.65rem 1.35rem;
  font-size: 0.95rem;
  font-weight: 600;
  color: #fff !important;
  text-decoration: none !important;
  border-radius: 999px;
  background: linear-gradient(135deg, #1e3a8a 0%, #1e40af 100%);
  box-shadow: 0 8px 24px rgba(30, 58, 138, 0.35);
  transition: transform 0.2s ease, box-shadow 0.2s ease;
}

.not-found-link:hover {
  transform: translateY(-2px);
  box-shadow: 0 12px 28px rgba(30, 58, 138, 0.45);
}
</style>
