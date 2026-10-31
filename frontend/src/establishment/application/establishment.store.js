import { defineStore } from 'pinia';
import { computed, ref } from 'vue';
import { EstablishmentApi } from '../infrastructure/establishment-api.js';
import { EstablishmentAssembler } from '../infrastructure/establishment.assembler.js';
import { OperatorAssembler } from '../infrastructure/operator.assembler.js';

const establishmentApi = new EstablishmentApi();

const useEstablishmentStore = defineStore('establishment', () => {
    const establishments = ref([]);
    const operators = ref([]);
    const errors = ref([]);
    const establishmentsLoaded = ref(false);
    const operatorsLoaded = ref(false);

    const establishmentsCount = computed(() => (
        establishmentsLoaded.value ? establishments.value.length : 0
    ));

    const operatorsCount = computed(() => (
        operatorsLoaded.value ? operators.value.length : 0
    ));

    async function fetchEstablishmentsAsync() {
        try {
            const response = await establishmentApi.getEstablishments();
            establishments.value = EstablishmentAssembler.toEntitiesFromResponse(response);
            establishmentsLoaded.value = true;
            return establishments.value;
        } catch (error) {
            errors.value.push(error);
            return [];
        }
    }

    function fetchEstablishments() {
        fetchEstablishmentsAsync();
    }

    async function fetchOperatorsAsync() {
        try {
            const response = await establishmentApi.getOperators();
            operators.value = OperatorAssembler.toEntitiesFromResponse(response);
            operatorsLoaded.value = true;
            return operators.value;
        } catch (error) {
            errors.value.push(error);
            return [];
        }
    }

    function fetchOperators() {
        fetchOperatorsAsync();
    }

    async function createEstablishmentAsync(payload) {
        const response = await establishmentApi.createEstablishment(payload);
        const created = EstablishmentAssembler.toEntityFromResource(response.data);
        establishments.value = [...establishments.value, created];
        establishmentsLoaded.value = true;
        return created;
    }

    function getEstablishmentById(id) {
        const idNum = parseInt(id, 10);
        return establishments.value.find((establishment) => establishment.id === idNum);
    }

    function getOperatorById(id) {
        const idNum = parseInt(id, 10);
        return operators.value.find((operator) => operator.id === idNum);
    }

    async function createOperatorAsync(payload) {
        const response = await establishmentApi.createOperator(payload);
        const created = OperatorAssembler.toEntityFromResource(response.data);
        operators.value = [...operators.value, created];
        operatorsLoaded.value = true;
        return created;
    }

    async function updateOperatorAsync(payload) {
        const response = await establishmentApi.updateOperator(payload);
        const updated = OperatorAssembler.toEntityFromResource(response.data);
        const index = operators.value.findIndex((o) => o.id === updated.id);
        if (index !== -1) operators.value[index] = updated;
        return updated;
    }

    async function assignOperatorsToEstablishmentAsync({ userIds, establishmentId }) {
        const estId = Number(establishmentId);
        const results = [];
        for (const userId of userIds) {
            const uid = Number(userId);
            const existing = operators.value.find((o) => Number(o.users_id) === uid);
            if (existing) {
                const updated = await updateOperatorAsync({
                    id: existing.id,
                    users_id: existing.users_id,
                    establishment_id: estId,
                    alerts_answered: existing.alerts_answered,
                    schedule: existing.schedule,
                });
                results.push(updated);
            } else {
                const created = await createOperatorAsync({
                    users_id: uid,
                    establishment_id: estId,
                    alerts_answered: 0,
                    schedule: 'Mañana 06:00-14:00',
                });
                results.push(created);
            }
        }
        return results;
    }

    async function deleteOperatorAsync(id) {
        await establishmentApi.deleteOperator(id);
        operators.value = operators.value.filter((o) => Number(o.id) !== Number(id));
    }

    return {
        establishments,
        operators,
        errors,
        establishmentsLoaded,
        operatorsLoaded,
        establishmentsCount,
        operatorsCount,
        fetchEstablishments,
        fetchEstablishmentsAsync,
        fetchOperators,
        fetchOperatorsAsync,
        createEstablishmentAsync,
        getEstablishmentById,
        getOperatorById,
        createOperatorAsync,
        updateOperatorAsync,
        deleteOperatorAsync,
        assignOperatorsToEstablishmentAsync,
    };
});

export default useEstablishmentStore;
