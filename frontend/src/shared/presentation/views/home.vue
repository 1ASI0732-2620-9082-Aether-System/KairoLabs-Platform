<script setup>
import { ref, computed } from 'vue';
import HomeHealthEntity from './home-health-entity.vue';
import HomeOperationalStaff from './home-operational-staff.vue';

const userRole = ref('health-entity'); // Cambiar a 'operational-staff' para probar

const isHealthEntity = computed(() => userRole.value === 'health-entity');
const isOperationalStaff = computed(() => userRole.value === 'operational-staff');
</script>

<template>
  <home-health-entity v-if="isHealthEntity" />
  <home-operational-staff v-else-if="isOperationalStaff" />
</template>