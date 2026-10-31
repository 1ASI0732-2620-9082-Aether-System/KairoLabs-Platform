<script setup>
import PageLoader from './shared/presentation/components/page-loader.vue';
</script>

<template>
  <div class="app-root">
    <page-loader />
    <pv-toast position="bottom-center" />
    <router-view />
  </div>
</template>

<style>
html.is-loading,
html.is-loading body {
  overflow: hidden;
}

.app-root {
  min-height: 100vh;
}
</style>
