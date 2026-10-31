<script setup>
import { ref, computed, onMounted, watch } from 'vue';
import { useRouter } from 'vue-router';
import { useI18n } from 'vue-i18n';
import { useToast } from 'primevue/usetoast';
import AuthPanel from '../components/auth-panel.vue';
import useIamStore from '../../application/iam.store.js';
import { readPendingPlan, readPendingRegistration } from '../../infrastructure/auth-session.js';

const { t } = useI18n();
const router = useRouter();
const toast = useToast();
const iamStore = useIamStore();

const card = ref({ cardNumber: '', expiry: '', cvv: '' });
const fieldErrors = ref({});
/** @type {import('vue').Ref<'form' | 'pressing' | 'processing' | 'success'>} */
const status = ref('form');
const successEmail = ref('');
const selectedPlan = readPendingPlan();

const planId = computed(() => selectedPlan?.catalogPlanId ?? '');
const planLabel = computed(() => (planId.value ? t(`plansPage.names.${planId.value}`) : ''));
const planPrice = computed(() => (planId.value ? t(`plansPage.prices.${planId.value}`) : ''));
const showModal = computed(() => status.value === 'processing' || status.value === 'success');

const digits = computed(() => card.value.cardNumber.replace(/\D/g, ''));
const maskedNumber = computed(() => {
  const d = digits.value.padEnd(16, '•');
  return `${d.slice(0, 4)}  ${d.slice(4, 8)}  ${d.slice(8, 12)}  ${d.slice(12, 16)}`;
});
const displayExpiry = computed(() => card.value.expiry || 'MM/AA');
const brand = computed(() => {
  const first = digits.value[0];
  if (first === '4') return 'Visa';
  if (first === '5') return 'Mastercard';
  if (first === '3') return 'Amex';
  return 'KairoLabs';
});

watch(showModal, (open) => {
  document.body.style.overflow = open ? 'hidden' : '';
});

onMounted(() => {
  if (!readPendingRegistration() || !selectedPlan?.planApiValue) {
    router.replace({ name: 'iam-register-plans' });
  }
});

function goBack() {
  if (status.value !== 'form') return;
  router.push({ name: 'iam-register-plans' });
}

function onCardNumberInput(event) {
  const raw = event.target.value.replace(/\D/g, '').slice(0, 16);
  card.value.cardNumber = raw.replace(/(\d{4})(?=\d)/g, '$1 ');
}

function onExpiryInput(event) {
  const raw = event.target.value.replace(/\D/g, '').slice(0, 4);
  card.value.expiry = raw.length <= 2 ? raw : `${raw.slice(0, 2)}/${raw.slice(2)}`;
}

function onCvvInput(event) {
  card.value.cvv = event.target.value.replace(/\D/g, '').slice(0, 4);
}

function wait(ms) {
  return new Promise((resolve) => setTimeout(resolve, ms));
}

async function finishAndGoLogin(email) {
  successEmail.value = email ?? '';
  status.value = 'success';
  await wait(1500);
  toast.add({
    severity: 'success',
    summary: t('iam.payment.successTitle'),
    detail: t('iam.payment.successDetail', { email: email || '—' }),
    life: 6000,
  });
  await router.push({ name: 'login' });
}

async function onSubmit() {
  if (status.value !== 'form') return;
  fieldErrors.value = {};
  if (!card.value.cardNumber.trim()) fieldErrors.value.cardNumber = t('iam.payment.invalidCardNumber');
  if (!card.value.expiry.trim()) fieldErrors.value.expiry = t('iam.payment.invalidExpiry');
  if (!card.value.cvv.trim()) fieldErrors.value.cvv = t('iam.payment.invalidCvv');
  if (Object.keys(fieldErrors.value).length) {
    toast.add({
      severity: 'warn',
      summary: t('iam.payment.title'),
      detail: t('iam.errors.required'),
      life: 4000,
    });
    return;
  }

  status.value = 'pressing';
  await wait(420);
  status.value = 'processing';

  try {
    const result = await iamStore.completeHealthEntityRegistration(card.value);

    if (!result.ok) {
      if (result.phase === 'payment' && result.errors) {
        status.value = 'form';
        const key = Object.keys(result.errors)[0];
        fieldErrors.value[key] = t(`iam.payment.${result.errors[key]}`);
        toast.add({
          severity: 'warn',
          summary: t('iam.payment.title'),
          detail: t(`iam.payment.${result.errors[key]}`),
          life: 5000,
        });
        return;
      }

      // Pago demo ok pero el alta en API falló: avisar y, si el correo ya existe, ir al login.
      status.value = 'form';
      const errorKey = result.error || 'network';
      toast.add({
        severity: errorKey === 'emailExists' ? 'warn' : 'error',
        summary: t('iam.errors.title'),
        detail: t(`iam.errors.${errorKey}`),
        life: 6000,
      });
      if (errorKey === 'emailExists') {
        await wait(400);
        await router.push({ name: 'login' });
      }
      return;
    }

    await finishAndGoLogin(result.email);
  } catch (e) {
    console.error(e);
    status.value = 'form';
    toast.add({
      severity: 'error',
      summary: t('iam.errors.title'),
      detail: t('iam.errors.network'),
      life: 4000,
    });
  }
}
</script>

<template>
  <auth-panel show-back single-column compact @back="goBack">
    <div class="checkout">
      <header class="checkout__head">
        <div class="checkout__brand">
          <img src="/logo.png" :alt="t('common.logoAlt')" />
          <strong>{{ t('common.brandName') }}</strong>
        </div>
        <h1>{{ t('iam.payment.title') }}</h1>
        <p class="checkout__secure">
          <i class="pi pi-lock" aria-hidden="true"></i>
          {{ t('iam.payment.secureNote') }}
        </p>
      </header>

      <div class="pay-visual" aria-hidden="true">
        <div class="pay-visual__glow"></div>
        <div class="pay-visual__chip"></div>
        <span class="pay-visual__brand">{{ brand }}</span>
        <p class="pay-visual__number">{{ maskedNumber }}</p>
        <div class="pay-visual__meta">
          <span>
            <small>{{ t('iam.payment.expiry') }}</small>
            {{ displayExpiry }}
          </span>
          <span>
            <small>{{ t('iam.payment.selectedPlan') }}</small>
            {{ planLabel || '—' }}
          </span>
        </div>
      </div>

      <div v-if="planLabel" class="checkout__summary">
        <span>{{ planLabel }}</span>
        <strong>{{ planPrice }}</strong>
      </div>

      <form class="checkout__form" @submit.prevent="onSubmit">
        <label class="checkout__field">
          <span>{{ t('iam.payment.cardNumber') }}</span>
          <div class="checkout__input">
            <i class="pi pi-credit-card" aria-hidden="true"></i>
            <input
              :value="card.cardNumber"
              inputmode="numeric"
              autocomplete="cc-number"
              placeholder="ACCT-000003"
              maxlength="19"
              :disabled="status !== 'form'"
              @input="onCardNumberInput"
            />
          </div>
          <small v-if="fieldErrors.cardNumber">{{ fieldErrors.cardNumber }}</small>
        </label>

        <div class="checkout__row">
          <label class="checkout__field">
            <span>{{ t('iam.payment.expiry') }}</span>
            <div class="checkout__input">
              <input
                :value="card.expiry"
                inputmode="numeric"
                autocomplete="cc-exp"
                placeholder="MM/AA"
                maxlength="5"
                :disabled="status !== 'form'"
                @input="onExpiryInput"
              />
            </div>
            <small v-if="fieldErrors.expiry">{{ fieldErrors.expiry }}</small>
          </label>
          <label class="checkout__field">
            <span>{{ t('iam.payment.cvv') }}</span>
            <div class="checkout__input">
              <input
                :value="card.cvv"
                type="password"
                inputmode="numeric"
                autocomplete="cc-csc"
                placeholder="•••"
                maxlength="4"
                :disabled="status !== 'form'"
                @input="onCvvInput"
              />
            </div>
            <small v-if="fieldErrors.cvv">{{ fieldErrors.cvv }}</small>
          </label>
        </div>

        <button
          type="submit"
          class="btn-primary checkout__pay"
          :class="{ 'is-pressing': status === 'pressing' }"
          :disabled="status !== 'form' && status !== 'pressing'"
        >
          <span>{{ t('iam.payment.submit') }}</span>
        </button>
      </form>
    </div>
  </auth-panel>

  <Teleport to="body">
    <Transition name="pay-modal">
      <div
        v-if="showModal"
        class="pay-modal"
        role="dialog"
        aria-modal="true"
        :aria-label="status === 'success' ? t('iam.payment.successHeadline') : t('iam.payment.processingTitle')"
      >
        <div class="pay-modal__backdrop" aria-hidden="true"></div>
        <div class="pay-modal__card">
          <div v-if="status === 'processing'" class="pay-modal__body">
            <div class="pay-modal__spinner">
              <span class="pay-modal__ring"></span>
              <span class="pay-modal__core">
                <img src="/logo.png" alt="" />
              </span>
            </div>
            <h2>{{ t('iam.payment.processingTitle') }}</h2>
            <p>{{ t('iam.payment.processingHint') }}</p>
          </div>
          <div v-else class="pay-modal__body">
            <div class="pay-modal__check">
              <i class="pi pi-check" aria-hidden="true"></i>
            </div>
            <h2>{{ t('iam.payment.successHeadline') }}</h2>
            <p>{{ t('iam.payment.successBody') }}</p>
            <p v-if="successEmail" class="pay-modal__email">{{ successEmail }}</p>
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<style scoped>
.checkout {
  width: 100%;
}

.checkout__head {
  text-align: center;
  margin-bottom: 1.1rem;
}

.checkout__brand {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.55rem;
  margin-bottom: 0.75rem;
}

.checkout__brand img {
  width: 36px;
  height: 36px;
  object-fit: contain;
}

.checkout__brand strong {
  font-size: 1.15rem;
  font-weight: 700;
  letter-spacing: -0.03em;
  color: #112433;
}

.checkout__head h1 {
  margin: 0;
  font-size: 1.35rem;
  font-weight: 800;
  letter-spacing: -0.03em;
  color: #112433;
}

.checkout__secure {
  display: inline-flex;
  align-items: center;
  gap: 0.4rem;
  margin: 0.55rem 0 0;
  font-size: 0.75rem;
  font-weight: 600;
  color: #6b7785;
}

.checkout__secure i {
  color: #f37021;
  font-size: 0.78rem;
}

.pay-visual {
  position: relative;
  overflow: hidden;
  min-height: 168px;
  margin-bottom: 1rem;
  padding: 1.15rem 1.25rem 1.05rem;
  border-radius: 18px;
  background: linear-gradient(145deg, #112433 0%, #1c3d55 52%, #0e1c28 100%);
  color: #fff;
  box-shadow: 0 16px 36px rgba(17, 36, 51, 0.28);
}

.pay-visual__glow {
  position: absolute;
  right: -20%;
  top: -40%;
  width: 180px;
  height: 180px;
  border-radius: 50%;
  background: radial-gradient(circle, rgba(243, 112, 33, 0.45) 0%, transparent 70%);
  pointer-events: none;
}

.pay-visual__chip {
  width: 38px;
  height: 28px;
  border-radius: 6px;
  background: linear-gradient(135deg, #f8d56b 0%, #f37021 100%);
  box-shadow: inset 0 0 0 1px rgba(255, 255, 255, 0.25);
}

.pay-visual__brand {
  position: absolute;
  top: 1.15rem;
  right: 1.2rem;
  font-size: 0.72rem;
  font-weight: 800;
  letter-spacing: 0.12em;
  text-transform: uppercase;
  color: rgba(255, 255, 255, 0.78);
}

.pay-visual__number {
  margin: 1.35rem 0 0.9rem;
  font-size: 1.05rem;
  font-weight: 600;
  letter-spacing: 0.12em;
  font-variant-numeric: tabular-nums;
}

.pay-visual__meta {
  display: flex;
  justify-content: space-between;
  gap: 1rem;
  font-size: 0.82rem;
  font-weight: 700;
  letter-spacing: 0.04em;
}

.pay-visual__meta small {
  display: block;
  margin-bottom: 0.15rem;
  font-size: 0.62rem;
  font-weight: 700;
  letter-spacing: 0.12em;
  text-transform: uppercase;
  color: rgba(255, 255, 255, 0.55);
}

.checkout__summary {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 1rem;
  padding: 0.75rem 0.9rem;
  border-radius: 12px;
  background: #f7f4ef;
  color: #112433;
  font-size: 0.88rem;
}

.checkout__summary strong {
  color: #f37021;
  font-weight: 800;
}

.checkout__form {
  display: flex;
  flex-direction: column;
  gap: 0.8rem;
}

.checkout__row {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 0.75rem;
}

.checkout__field {
  display: flex;
  flex-direction: column;
  gap: 0.35rem;
  font-size: 0.8rem;
  font-weight: 700;
  color: #112433;
}

.checkout__field small {
  font-size: 0.74rem;
  font-weight: 500;
  color: #d63b3b;
}

.checkout__input {
  display: flex;
  align-items: center;
  gap: 0.45rem;
  height: 46px;
  padding: 0 0.85rem;
  border: 1px solid #e4ddd4;
  border-radius: 12px;
  background: #fff;
  transition: border-color 0.2s ease, box-shadow 0.2s ease;
}

.checkout__input:focus-within {
  border-color: #f37021;
  box-shadow: 0 0 0 3px rgba(243, 112, 33, 0.15);
}

.checkout__input i {
  color: #8b95a1;
}

.checkout__input input {
  width: 100%;
  border: none;
  outline: none;
  background: transparent;
  font-family: inherit;
  font-size: 0.92rem;
  font-weight: 500;
  color: #112433;
}

.checkout__pay {
  margin-top: 0.25rem;
}

.checkout__pay.is-pressing {
  transform: scale(0.97);
  pointer-events: none;
}

.checkout__pay.is-pressing::before {
  transform: translateX(0);
}
</style>

<style>
.pay-modal {
  position: fixed;
  inset: 0;
  z-index: 9500;
  display: grid;
  place-items: center;
  padding: 1.25rem;
}

.pay-modal__backdrop {
  position: absolute;
  inset: 0;
  background: rgba(17, 36, 51, 0.55);
  backdrop-filter: blur(8px);
}

.pay-modal__card {
  position: relative;
  z-index: 1;
  width: min(100%, 360px);
  padding: 2rem 1.6rem 1.75rem;
  border-radius: 22px;
  background: #fff;
  box-shadow: 0 28px 60px rgba(17, 36, 51, 0.35);
  text-align: center;
}

.pay-modal__body h2 {
  margin: 1.1rem 0 0.4rem;
  font-size: 1.25rem;
  font-weight: 800;
  letter-spacing: -0.03em;
  color: #112433;
}

.pay-modal__body p {
  margin: 0;
  font-size: 0.9rem;
  line-height: 1.45;
  color: #6b7785;
}

.pay-modal__email {
  margin-top: 0.55rem !important;
  font-weight: 700;
  color: #112433 !important;
}

.pay-modal__spinner {
  position: relative;
  width: 96px;
  height: 96px;
  margin: 0 auto;
}

.pay-modal__ring {
  position: absolute;
  inset: 0;
  border-radius: 50%;
  border: 2px solid rgba(17, 36, 51, 0.08);
}

.pay-modal__ring::before {
  content: '';
  position: absolute;
  inset: -3px;
  border-radius: 50%;
  background: conic-gradient(
    from 0deg,
    transparent 0%,
    transparent 58%,
    rgba(243, 112, 33, 0.25) 72%,
    #f37021 88%,
    #fff 96%,
    transparent 100%
  );
  -webkit-mask: radial-gradient(farthest-side, transparent calc(100% - 4px), #000 calc(100% - 3px));
  mask: radial-gradient(farthest-side, transparent calc(100% - 4px), #000 calc(100% - 3px));
  animation: pay-modal-spin 1.15s linear infinite;
}

.pay-modal__core {
  position: absolute;
  inset: 16px;
  display: grid;
  place-items: center;
  border-radius: 50%;
  background: #fff;
  box-shadow: 0 8px 20px rgba(17, 36, 51, 0.12);
}

.pay-modal__core img {
  width: 36px;
  height: 36px;
  object-fit: contain;
}

.pay-modal__check {
  width: 88px;
  height: 88px;
  margin: 0 auto;
  display: grid;
  place-items: center;
  border-radius: 50%;
  background: #112433;
  color: #fff;
  font-size: 1.8rem;
  box-shadow: 0 0 0 8px rgba(243, 112, 33, 0.16);
  animation: pay-modal-pop 0.45s cubic-bezier(0.22, 1, 0.36, 1);
}

.pay-modal-enter-active,
.pay-modal-leave-active {
  transition: opacity 0.28s ease;
}

.pay-modal-enter-active .pay-modal__card,
.pay-modal-leave-active .pay-modal__card {
  transition: transform 0.32s cubic-bezier(0.22, 1, 0.36, 1), opacity 0.28s ease;
}

.pay-modal-enter-from,
.pay-modal-leave-to {
  opacity: 0;
}

.pay-modal-enter-from .pay-modal__card,
.pay-modal-leave-to .pay-modal__card {
  opacity: 0;
  transform: translateY(14px) scale(0.96);
}

@keyframes pay-modal-spin {
  to { transform: rotate(360deg); }
}

@keyframes pay-modal-pop {
  0% { transform: scale(0.6); opacity: 0; }
  100% { transform: scale(1); opacity: 1; }
}

@media (prefers-reduced-motion: reduce) {
  .pay-modal__ring::before,
  .pay-modal__check {
    animation: none;
  }
}
</style>
