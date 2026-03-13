import { defineSecret } from 'firebase-functions/params';

// Secrets (valores sensibles — usar Firebase Secret Manager)
export const STRIPE_SECRET_KEY = defineSecret('STRIPE_SECRET_KEY');
export const STRIPE_WEBHOOK_SECRET = defineSecret('STRIPE_WEBHOOK_SECRET');
export const APPLE_SHARED_SECRET = defineSecret('APPLE_SHARED_SECRET');
export const SLACK_WEBHOOK_URL = defineSecret('SLACK_WEBHOOK_URL');

// Configuracion no-sensible
export const APP_REGION = 'us-central1';

// Terminos prohibidos — si aparecen en requests de Stripe, rechazar inmediatamente
export const PROHIBITED_TERMS = [
  'tournament_entry', 'match_fee', 'deposit_real_money',
  'triumph', 'wager', 'gambling', 'prize_pool', 'skill_game_entry'
];

export const APPLE_PRODUCTION_URL = 'https://buy.itunes.apple.com/verifyReceipt';
export const APPLE_SANDBOX_URL = 'https://sandbox.itunes.apple.com/verifyReceipt';
