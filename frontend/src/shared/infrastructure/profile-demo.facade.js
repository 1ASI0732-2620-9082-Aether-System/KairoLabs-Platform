/**
 * IAM application facade — demo profile orchestration across IAM, Establishment and Subscriptions APIs.
 *
 * @module profile-demo.facade
 */
import { IamApi } from '../infrastructure/iam-api.js';
import { UserAssembler } from '../infrastructure/user.assembler.js';
import { EstablishmentApi } from '../../establishment/infrastructure/establishment-api.js';
import { EstablishmentAssembler } from '../../establishment/infrastructure/establishment.assembler.js';
import { SubscriptionsApi } from '../../subscriptions/infrastructure/subscriptions-api.js';
import { SubscriptionAssembler } from '../../subscriptions/infrastructure/subscription.assembler.js';

const iamApi = new IamApi();
const establishmentApi = new EstablishmentApi();
const subscriptionsApi = new SubscriptionsApi();

function pickRandom(arr) {
    if (!arr?.length) return null;
    return arr[Math.floor(Math.random() * arr.length)];
}

/**
 * @param {{ persistedUserId?: number|null, planFromSession?: Object|null }} [options]
 * @returns {Promise<Object>}
 */
export async function fetchProfileDemoFromApis(options = {}) {
    const [usersRes, establishmentsRes, subscriptionsRes] = await Promise.all([
        iamApi.getUsers(),
        establishmentApi.getEstablishments(),
        subscriptionsApi.getSubscriptions(),
    ]);

    const users = UserAssembler.toEntitiesFromResponse(usersRes);
    const establishments = EstablishmentAssembler.toEntitiesFromResponse(establishmentsRes);
    const subscriptions = SubscriptionAssembler.toEntitiesFromResponse(subscriptionsRes);

    const user =
        (options.persistedUserId && users.find((u) => u.id === options.persistedUserId)) ??
        pickRandom(users);

    const establishment =
        establishments.find((e) => e.admin_id === user?.id) ?? pickRandom(establishments);

    const subscription =
        subscriptions.find((s) => s.admin_id === user?.id) ?? pickRandom(subscriptions);

    const randomSub = pickRandom(subscriptions);
    const planApi =
        options.planFromSession?.planApiValue != null
            ? options.planFromSession.planApiValue
            : randomSub?.plan ?? 'BASIC';
    const benefitsEndDate =
        options.planFromSession?.benefitsEndDate != null
            ? options.planFromSession.benefitsEndDate
            : randomSub?.end_date ?? subscription?.end_date ?? null;

    return {
        user,
        establishment,
        subscription,
        planApi,
        benefitsEndDate,
    };
}
