import { defineSecret } from 'firebase-functions/params';

// Secrets (valores sensibles — usar Firebase Secret Manager)
export const APPLE_SHARED_SECRET = defineSecret('APPLE_SHARED_SECRET');
export const SLACK_WEBHOOK_URL = defineSecret('SLACK_WEBHOOK_URL');

// Configuracion no-sensible
export const APP_REGION = 'us-central1';

export const APPLE_PRODUCTION_URL = 'https://buy.itunes.apple.com/verifyReceipt';
export const APPLE_SANDBOX_URL = 'https://sandbox.itunes.apple.com/verifyReceipt';
