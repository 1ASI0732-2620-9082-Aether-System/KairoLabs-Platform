/**
 * Simulated payment gateway — UI/demo only. No real charges or card checks.
 * @module demo-payment.gateway
 */

const PROCESSING_MS = 1700;

function hasText(value) {
    return String(value ?? '').trim().length > 0;
}

/**
 * Demo validation: only checks that the user filled the form (design mock).
 * @param {{ cardNumber?: string, expiry?: string, cvv?: string }} payment
 * @returns {{ valid: boolean, errors: Record<string, string> }}
 */
export function validateDemoPayment(payment) {
    const errors = {};
    if (!hasText(payment?.cardNumber)) errors.cardNumber = 'invalidCardNumber';
    if (!hasText(payment?.expiry)) errors.expiry = 'invalidExpiry';
    if (!hasText(payment?.cvv)) errors.cvv = 'invalidCvv';
    return { valid: Object.keys(errors).length === 0, errors };
}

/** Brief delay so the billing screen feels like a real checkout. */
export function simulateDemoPaymentProcessing() {
    return new Promise((resolve) => setTimeout(resolve, PROCESSING_MS));
}
