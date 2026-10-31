/**
 * Application service store for the Subscriptions bounded context.
 * All data is loaded from the Render REST API.
 *
 * @module useSubscriptionsStore
 */
import { defineStore } from 'pinia';
import { computed, ref } from 'vue';
import { SubscriptionsApi } from '../infrastructure/subscriptions-api.js';
import { SubscriptionAssembler } from '../infrastructure/subscription.assembler.js';
import { Subscription } from '../domain/model/subscription.entity.js';

const subscriptionsApi = new SubscriptionsApi();

const useSubscriptionsStore = defineStore('subscriptions', () => {
    const subscriptions = ref([]);
    const errors = ref([]);
    const subscriptionsLoaded = ref(false);

    const subscriptionsCount = computed(() =>
        subscriptionsLoaded.value ? subscriptions.value.length : 0,
    );

    async function fetchSubscriptionsAsync() {
        try {
            const response = await subscriptionsApi.getSubscriptions();
            subscriptions.value = SubscriptionAssembler.toEntitiesFromResponse(response);
            subscriptionsLoaded.value = true;
            return subscriptions.value;
        } catch (error) {
            errors.value.push(error);
            return [];
        }
    }

    function fetchSubscriptions() {
        fetchSubscriptionsAsync();
    }

    function getSubscriptionById(id) {
        const idNum = parseInt(id, 10);
        return subscriptions.value.find((s) => s.id === idNum);
    }

    const PLAN_CONTEXT_KEY = 'meditrack_plan_context';

    function readPlanContext() {
        try {
            const raw = sessionStorage.getItem(PLAN_CONTEXT_KEY);
            return raw ? JSON.parse(raw) : null;
        } catch {
            return null;
        }
    }

    function writePlanContext(ctx) {
        sessionStorage.setItem(PLAN_CONTEXT_KEY, JSON.stringify(ctx));
    }

    function apiPlanToCatalogId(apiPlan) {
        return Subscription.apiPlanToCatalogId(apiPlan);
    }

    return {
        subscriptions,
        errors,
        subscriptionsLoaded,
        subscriptionsCount,
        fetchSubscriptions,
        fetchSubscriptionsAsync,
        getSubscriptionById,
        readPlanContext,
        writePlanContext,
        apiPlanToCatalogId,
    };
});

export default useSubscriptionsStore;
