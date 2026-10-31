<script setup>
import { computed, ref, watch } from 'vue';
import { useRoute } from 'vue-router';
import AuthPanel from '../components/auth-panel.vue';

const route = useRoute();
const isSignup = computed(() => route.name === 'iam-register');
const transitionName = ref('auth-slide-left');

watch(
  () => route.name,
  (to, from) => {
    if (from === 'login' && to === 'iam-register') {
      transitionName.value = 'auth-slide-left';
    } else if (from === 'iam-register' && to === 'login') {
      transitionName.value = 'auth-slide-right';
    }
  },
);
</script>

<template>
  <auth-panel :signup="isSignup">
    <router-view v-slot="{ Component, route: viewRoute }">
      <Transition :name="transitionName" mode="out-in">
        <div v-if="Component" :key="viewRoute.fullPath" class="auth-view">
          <component :is="Component" />
        </div>
      </Transition>
    </router-view>
  </auth-panel>
</template>

<style>
.auth-view {
  width: 100%;
}

.auth-slide-left-enter-active,
.auth-slide-left-leave-active,
.auth-slide-right-enter-active,
.auth-slide-right-leave-active {
  transition: opacity 0.34s ease, transform 0.34s ease;
}

.auth-slide-left-leave-to {
  opacity: 0;
  transform: translateX(-28px);
}

.auth-slide-left-enter-from {
  opacity: 0;
  transform: translateX(28px);
}

.auth-slide-right-leave-to {
  opacity: 0;
  transform: translateX(28px);
}

.auth-slide-right-enter-from {
  opacity: 0;
  transform: translateX(-28px);
}

@media (prefers-reduced-motion: reduce) {
  .auth-slide-left-enter-active,
  .auth-slide-left-leave-active,
  .auth-slide-right-enter-active,
  .auth-slide-right-leave-active {
    transition: none;
  }
}
</style>
