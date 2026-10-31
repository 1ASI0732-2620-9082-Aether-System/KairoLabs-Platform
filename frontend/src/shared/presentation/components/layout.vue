<script setup>
import LanguageSwitcher from './language-switcher.vue';
import { computed, ref, onMounted, onBeforeUnmount } from 'vue';
import { useI18n } from 'vue-i18n';
import { useRouter, useRoute } from 'vue-router';
import { readAuthSession } from '../../../iam/infrastructure/auth-session.js';
import { getProfileRoute } from '../../../shared/infrastructure/route.helpers.js';
import useIamStore from '../../../iam/application/iam.store.js';

const { t } = useI18n();
const router = useRouter();
const route = useRoute();
const iamStore = useIamStore();

const sidebarCollapsed = ref(false);
const isMobile = ref(false);
const clockText = ref('');
const notifCount = ref(3);

const userRole = ref(localStorage.getItem('userRole') || 'health-entity');
const session = ref(readAuthSession());

const isHealthEntity = computed(() => userRole.value === 'health-entity');
const isOperationalStaff = computed(() => userRole.value === 'operational-staff');

const displayName = computed(() => session.value?.name ?? t('layout.guestUser'));
const userInitials = computed(() => {
  const parts = String(displayName.value).trim().split(/\s+/).filter(Boolean);
  if (parts.length >= 2) return `${parts[0][0]}${parts[1][0]}`.toUpperCase();
  return (parts[0]?.[0] ?? 'U').toUpperCase();
});
const roleLabel = computed(() =>
  isHealthEntity.value ? t('iam.healthEntity.badge') : t('iam.operational.badge'),
);

const healthEntityItems = [
  { label: 'option.home', to: '/iam/home/health-entity', icon: 'pi pi-home', section: 'control' },
  { label: 'establishment.establishments', to: '/establishment/establishments', icon: 'pi pi-building', section: 'modules' },
  { label: 'establishment.assignOperator', to: '/establishment/assign-operator', icon: 'pi pi-users', section: 'modules' },
  { label: 'establishment.addEstablishment', to: '/establishment/establishments/new', icon: 'pi pi-plus-circle', section: 'modules' },
  { label: 'establishment.mapOfEstablishments', to: '/establishment/map', icon: 'pi pi-map-marker', section: 'modules' },
  { label: 'plansPage.title', to: '/subscriptions/plans', icon: 'pi pi-credit-card', section: 'modules' },
];

const operationalItems = [
  { label: 'option.home', to: '/iam/home/operational-staff', icon: 'pi pi-home', section: 'control' },
  { label: 'monitoring.devices', to: '/monitoring/devices', icon: 'pi pi-server', section: 'modules' },
  { label: 'logistics.transports', to: '/logistics/transports', icon: 'pi pi-truck', section: 'modules' },
  { label: 'establishment.operators', to: '/establishment/operators', icon: 'pi pi-id-card', section: 'modules' },
  { label: 'monitoring.controlCenter', to: '/monitoring/control-center', icon: 'pi pi-chart-line', section: 'modules' },
];

const pageTitle = computed(() => {
  const match = [...healthEntityItems, ...operationalItems].find((item) => isActive(item.to));
  return match ? t(match.label) : t('layout.appTitle');
});

const navItems = computed(() =>
  isHealthEntity.value ? healthEntityItems : isOperationalStaff.value ? operationalItems : [],
);
const controlItems = computed(() => navItems.value.filter((item) => item.section === 'control'));
const moduleItems = computed(() => navItems.value.filter((item) => item.section === 'modules'));

let clockTimer = null;

function pad(n) {
  return String(n).padStart(2, '0');
}

function tickClock() {
  const now = new Date();
  clockText.value = `${pad(now.getHours())}:${pad(now.getMinutes())}`;
}

function toggleSidebar() {
  sidebarCollapsed.value = !sidebarCollapsed.value;
}

function goToProfile() {
  router.push(getProfileRoute());
}

function handleLogout() {
  sessionStorage.removeItem('meditrack_plan_context');
  iamStore.logout();
  router.push({ name: 'login' });
}

function isActive(to) {
  return route.path === to || route.path.startsWith(`${to}/`);
}

function toggleFullscreen() {
  if (!document.fullscreenElement) {
    document.documentElement.requestFullscreen?.();
  } else {
    document.exitFullscreen?.();
  }
}

onMounted(() => {
  session.value = readAuthSession();
  isMobile.value = window.innerWidth <= 1024;
  if (isMobile.value) sidebarCollapsed.value = true;
  tickClock();
  clockTimer = window.setInterval(tickClock, 1000);
  window.addEventListener('resize', onResize);
});

function onResize() {
  isMobile.value = window.innerWidth <= 1024;
}

onBeforeUnmount(() => {
  if (clockTimer) clearInterval(clockTimer);
  window.removeEventListener('resize', onResize);
});
</script>

<template>
  <pv-confirm-dialog />

  <div class="app-shell" :class="{ 'app-shell--collapsed': sidebarCollapsed }">
    <aside class="app-sidebar" aria-label="Navegación principal">
      <div class="sidebar-brand">
        <img src="/logo.png" :alt="t('common.logoAlt')" class="sidebar-brand__logo" />
        <strong v-if="!sidebarCollapsed" class="sidebar-brand__name">{{ t('common.brandName') }}</strong>
      </div>

      <nav class="sidebar-nav">
        <p v-if="!sidebarCollapsed" class="sidebar-section">{{ t('layout.sectionControl') }}</p>
        <router-link
          v-for="item in controlItems"
          :key="item.to"
          :to="item.to"
          class="sidebar-link"
          :class="{ 'is-active': isActive(item.to) }"
          :title="t(item.label)"
        >
          <span class="sidebar-link__icon"><i :class="item.icon" aria-hidden="true"></i></span>
          <span v-if="!sidebarCollapsed" class="sidebar-link__label">{{ t(item.label) }}</span>
        </router-link>

        <p v-if="!sidebarCollapsed && moduleItems.length" class="sidebar-section">{{ t('layout.sectionModules') }}</p>
        <router-link
          v-for="item in moduleItems"
          :key="item.to"
          :to="item.to"
          class="sidebar-link"
          :class="{ 'is-active': isActive(item.to) }"
          :title="t(item.label)"
        >
          <span class="sidebar-link__icon"><i :class="item.icon" aria-hidden="true"></i></span>
          <span v-if="!sidebarCollapsed" class="sidebar-link__label">{{ t(item.label) }}</span>
        </router-link>
      </nav>

      <div class="sidebar-bottom">
        <button
          v-if="!sidebarCollapsed"
          type="button"
          class="sidebar-user"
          @click="goToProfile"
        >
          <span class="sidebar-user__avatar">{{ userInitials }}</span>
          <span class="sidebar-user__meta">
            <strong>{{ displayName }}</strong>
            <small>{{ roleLabel }}</small>
          </span>
        </button>
        <button
          v-else
          type="button"
          class="sidebar-user sidebar-user--compact"
          :title="displayName"
          @click="goToProfile"
        >
          <span class="sidebar-user__avatar">{{ userInitials }}</span>
        </button>

        <button type="button" class="sidebar-logout" @click="handleLogout">
          <i class="pi pi-sign-out" aria-hidden="true"></i>
          <span v-if="!sidebarCollapsed">{{ t('iam.logout') }}</span>
        </button>
      </div>
    </aside>

    <div
      v-if="!sidebarCollapsed && isMobile"
      class="sidebar-backdrop"
      aria-hidden="true"
      @click="toggleSidebar"
    ></div>

    <div class="app-main">
      <header class="app-topbar">
        <div class="topbar-start">
          <button
            type="button"
            class="topbar-circle"
            :aria-label="sidebarCollapsed ? t('layout.showSidebar') : t('layout.hideSidebar')"
            @click="toggleSidebar"
          >
            <i :class="sidebarCollapsed ? 'pi pi-bars' : 'pi pi-times'" aria-hidden="true"></i>
          </button>
          <div class="topbar-title-wrap">
            <h1>{{ pageTitle }}</h1>
            <p>{{ t('layout.appTitle') }}</p>
          </div>
        </div>

        <div class="topbar-end">
          <button type="button" class="topbar-circle topbar-notif" :aria-label="t('layout.notifications')">
            <i class="pi pi-bell" aria-hidden="true"></i>
            <span v-if="notifCount" class="topbar-notif__badge">{{ notifCount }}</span>
          </button>

          <button
            type="button"
            class="topbar-circle"
            :aria-label="t('layout.fullscreen')"
            @click="toggleFullscreen"
          >
            <i class="pi pi-window-maximize" aria-hidden="true"></i>
          </button>

          <language-switcher compact />

          <div class="topbar-pill topbar-pill--neutral">
            <i class="pi pi-clock" aria-hidden="true"></i>
            <span>{{ clockText }}</span>
          </div>
        </div>
      </header>

      <main class="main-content">
        <router-view />
      </main>
    </div>
  </div>
</template>

<style scoped>
.app-shell {
  --sidebar-width: 268px;
  --sidebar-collapsed-width: 84px;
  --topbar-height: 72px;
  display: flex;
  min-height: 100vh;
  background: #f4f6f8;
  font-family: 'Outfit', var(--mt-font), sans-serif;
}

.app-sidebar {
  position: fixed;
  inset: 0 auto 0 0;
  z-index: 1100;
  width: var(--sidebar-width);
  display: flex;
  flex-direction: column;
  background: #fff;
  border-right: 1px solid rgba(17, 36, 51, 0.08);
  box-shadow: 8px 0 28px rgba(17, 36, 51, 0.04);
  transition: width 0.28s cubic-bezier(0.4, 0, 0.2, 1);
}

.app-shell--collapsed .app-sidebar {
  width: var(--sidebar-collapsed-width);
}

.sidebar-brand {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding: 1.25rem 1.1rem 1.1rem;
  border-bottom: 1px solid rgba(17, 36, 51, 0.08);
}

.sidebar-brand__logo {
  width: 40px;
  height: 40px;
  object-fit: contain;
  flex-shrink: 0;
}

.sidebar-brand__name {
  font-size: 1.12rem;
  font-weight: 800;
  letter-spacing: -0.03em;
  color: #112433;
  line-height: 1.1;
}

.sidebar-brand__text {
  display: none;
}

.sidebar-brand__text strong {
  font-size: 1.05rem;
  font-weight: 800;
  letter-spacing: -0.03em;
  color: #112433;
  line-height: 1.1;
}

.sidebar-brand__text span {
  display: none;
}

.sidebar-nav {
  flex: 1;
  overflow-y: auto;
  padding: 0.9rem 0.85rem 1rem;
  display: flex;
  flex-direction: column;
  gap: 0.3rem;
}

.sidebar-section {
  margin: 0.65rem 0.35rem 0.35rem;
  font-size: 0.68rem;
  font-weight: 700;
  letter-spacing: 0.12em;
  text-transform: uppercase;
  color: #9aa6b2;
}

.sidebar-section:first-child {
  margin-top: 0.15rem;
}

.sidebar-link {
  position: relative;
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding: 0.45rem 0.55rem 0.45rem 0.65rem;
  border-radius: 12px;
  text-decoration: none;
  color: #415a77;
  font-size: 0.9rem;
  font-weight: 600;
  transition: background 0.2s ease, color 0.2s ease;
}

.sidebar-link__icon {
  width: 2.35rem;
  height: 2.35rem;
  display: grid;
  place-items: center;
  border-radius: 10px;
  background: rgba(243, 112, 33, 0.12);
  color: #112433;
  flex-shrink: 0;
}

.sidebar-link__icon i {
  font-size: 0.95rem;
}

.sidebar-link:hover {
  background: #f7f8fa;
  color: #112433;
}

.sidebar-link.is-active {
  background: rgba(243, 112, 33, 0.1);
  color: #112433;
}

.sidebar-link.is-active::before {
  content: '';
  position: absolute;
  left: 0;
  top: 0.45rem;
  bottom: 0.45rem;
  width: 3px;
  border-radius: 999px;
  background: #112433;
}

.sidebar-link.is-active .sidebar-link__icon {
  background: #112433;
  color: #fff;
}

.app-shell--collapsed .sidebar-brand {
  justify-content: center;
  padding-inline: 0.65rem;
}

.app-shell--collapsed .sidebar-link {
  justify-content: center;
  padding: 0.45rem;
}

.sidebar-bottom {
  margin-top: auto;
  padding: 0.9rem 0.85rem 1rem;
  border-top: 1px solid rgba(17, 36, 51, 0.08);
  display: flex;
  flex-direction: column;
  gap: 0.65rem;
}

.sidebar-user {
  display: flex;
  align-items: center;
  gap: 0.7rem;
  width: 100%;
  padding: 0.7rem 0.75rem;
  border: none;
  border-radius: 14px;
  background: #f1f4f7;
  cursor: pointer;
  text-align: left;
  color: #112433;
}

.sidebar-user:hover {
  background: #e8edf2;
}

.sidebar-user--compact {
  justify-content: center;
  padding: 0.45rem;
  background: transparent;
}

.sidebar-user--compact:hover {
  background: #f7f8fa;
}

.sidebar-user__avatar {
  width: 2.5rem;
  height: 2.5rem;
  display: grid;
  place-items: center;
  border-radius: 12px;
  background: #112433;
  color: #fff;
  font-size: 0.82rem;
  font-weight: 800;
  flex-shrink: 0;
}

.sidebar-user__meta {
  min-width: 0;
  display: flex;
  flex-direction: column;
  gap: 0.05rem;
}

.sidebar-user__meta strong {
  font-size: 0.88rem;
  font-weight: 700;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.sidebar-user__meta small {
  font-size: 0.72rem;
  color: #7a8694;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.sidebar-logout {
  width: 100%;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 0.55rem;
  min-height: 2.65rem;
  padding: 0.65rem 0.9rem;
  border: 1px solid rgba(17, 36, 51, 0.14);
  border-radius: 12px;
  background: #fff;
  color: #112433;
  font-family: inherit;
  font-size: 0.86rem;
  font-weight: 700;
  cursor: pointer;
  transition: background 0.2s ease, border-color 0.2s ease, color 0.2s ease;
}

.sidebar-logout:hover {
  background: #112433;
  border-color: #112433;
  color: #fff;
}

.app-shell--collapsed .sidebar-logout {
  padding: 0.65rem;
  border-radius: 12px;
}

.app-main {
  flex: 1;
  min-width: 0;
  margin-left: var(--sidebar-width);
  display: flex;
  flex-direction: column;
  transition: margin-left 0.28s cubic-bezier(0.4, 0, 0.2, 1);
}

.app-shell--collapsed .app-main {
  margin-left: var(--sidebar-collapsed-width);
}

.app-topbar {
  position: sticky;
  top: 0;
  z-index: 1000;
  min-height: var(--topbar-height);
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 1rem;
  padding: 0.75rem 1.35rem;
  background: rgba(255, 255, 255, 0.96);
  border-bottom: 1px solid rgba(17, 36, 51, 0.08);
  backdrop-filter: blur(10px);
}

.topbar-start,
.topbar-end {
  display: flex;
  align-items: center;
  gap: 0.65rem;
}

.topbar-title-wrap h1 {
  margin: 0;
  font-size: 1.05rem;
  font-weight: 700;
  letter-spacing: -0.02em;
  color: #112433;
  line-height: 1.2;
}

.topbar-title-wrap p {
  margin: 0.1rem 0 0;
  font-size: 0.72rem;
  color: #7a8694;
}

.topbar-circle {
  position: relative;
  width: 2.55rem;
  height: 2.55rem;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  border: 1px solid rgba(17, 36, 51, 0.1);
  border-radius: 50%;
  background: #fff;
  color: #415a77;
  cursor: pointer;
  transition: background 0.2s ease, color 0.2s ease, border-color 0.2s ease;
}

.topbar-circle:hover {
  background: #f7f8fa;
  color: #112433;
  border-color: rgba(243, 112, 33, 0.35);
}

.topbar-notif__badge {
  position: absolute;
  top: -0.15rem;
  right: -0.15rem;
  min-width: 1.1rem;
  height: 1.1rem;
  padding: 0 0.25rem;
  border-radius: 999px;
  background: #e24b4a;
  color: #fff;
  font-size: 0.62rem;
  font-weight: 800;
  display: grid;
  place-items: center;
  line-height: 1;
}

.topbar-pill {
  display: inline-flex;
  align-items: center;
  gap: 0.4rem;
  min-height: 2.4rem;
  padding: 0.35rem 0.85rem;
  border-radius: 999px;
  font-size: 0.82rem;
  font-weight: 700;
  font-variant-numeric: tabular-nums;
}

.topbar-pill--neutral {
  background: #f1f3f5;
  color: #415a77;
}

.main-content {
  flex: 1;
  width: 100%;
  max-width: 1400px;
  margin: 0 auto;
  padding: 1.25rem 1.5rem 2.25rem;
}

.sidebar-backdrop {
  display: none;
}

@media (max-width: 1024px) {
  .app-main {
    margin-left: 0;
  }

  .app-shell--collapsed .app-sidebar {
    transform: translateX(calc(-1 * var(--sidebar-width)));
    width: var(--sidebar-width);
  }

  .app-shell:not(.app-shell--collapsed) .app-sidebar {
    width: var(--sidebar-width);
  }

  .sidebar-backdrop {
    display: block;
    position: fixed;
    inset: 0;
    z-index: 1050;
    background: rgba(17, 36, 51, 0.45);
  }

  .app-shell--collapsed .sidebar-backdrop {
    display: none;
  }
}

@media (max-width: 720px) {
  .topbar-title-wrap p,
  .topbar-pill--neutral {
    display: none;
  }

  .topbar-end {
    gap: 0.4rem;
  }

  .main-content {
    padding: 0.85rem 0.75rem 1.5rem;
  }

  .app-topbar {
    padding-left: 0.75rem;
    padding-right: 0.75rem;
  }
}

@media (max-width: 480px) {
  .topbar-title-wrap h1 {
    font-size: 1rem;
  }

  .main-content {
    padding: 0.7rem 0.55rem 1.25rem;
  }
}
</style>
