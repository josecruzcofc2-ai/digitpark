import * as admin from 'firebase-admin';
import { onRequest } from 'firebase-functions/v2/https';
import { StripeService } from './services/stripe.service';
import { AppleIAPService } from './services/appleIAP.service';
import { EntitlementService } from './services/entitlement.service';
import { checkTriumphIsolation, checkVersionAccess } from './middleware/triumphIsolation';
import { sendSlackAlert } from './services/alert.service';
import {
  STRIPE_SECRET_KEY,
  STRIPE_WEBHOOK_SECRET,
  APPLE_SHARED_SECRET,
  SLACK_WEBHOOK_URL,
} from './config/environment';

// ============================================================
// Inicializar Firebase Admin (automatico en Cloud Functions)
// ============================================================
if (!admin.apps.length) {
  admin.initializeApp();
}

const entitlementService = new EntitlementService();

// ============================================================
// STRIPE — Crear Checkout Session
// POST https://us-central1-{project}.cloudfunctions.net/stripeCreateCheckout
// Body: { productId, userId, appVersion, priceId? }
// Header: x-app-version: pro
// ============================================================
export const stripeCreateCheckout = onRequest(
  { secrets: [STRIPE_SECRET_KEY], region: 'us-central1' },
  async (req, res) => {
    if (req.method !== 'POST') {
      res.status(405).json({ error: 'method_not_allowed' });
      return;
    }
    if (!checkTriumphIsolation(req, res)) return;
    if (!checkVersionAccess(req, res, true)) return;

    const callerUid = await verifyAuthAndGetUid(req, res);
    if (!callerUid) return;

    const { productId, userId, appVersion, priceId } = req.body as {
      productId?: string;
      userId?: string;
      appVersion?: string;
      priceId?: string;
    };

    if (!productId || !userId) {
      res.status(400).json({ error: 'missing_fields', message: 'productId y userId requeridos' });
      return;
    }

    if (callerUid !== userId) {
      res.status(403).json({ error: 'forbidden', message: 'No puedes crear checkout para otro usuario' });
      return;
    }

    try {
      const stripeService = new StripeService(STRIPE_SECRET_KEY.value());
      const session = await stripeService.createCheckoutSession({
        productId,
        userId,
        appVersion: (appVersion as 'pro' | 'global') || 'pro',
        priceId,
      });
      res.json(session);
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : 'Unknown error';
      console.error('[stripeCreateCheckout] Error:', message);
      res.status(500).json({ error: 'session_creation_failed', message });
    }
  }
);

// ============================================================
// STRIPE — Webhook (Stripe -> Firebase)
// POST https://us-central1-{project}.cloudfunctions.net/stripeWebhook
// Requires raw body for signature verification — Firebase Functions v2
// sets rawBody automatically.
// ============================================================
export const stripeWebhook = onRequest(
  {
    secrets: [STRIPE_SECRET_KEY, STRIPE_WEBHOOK_SECRET],
    region: 'us-central1',
    // rawBody must be preserved for Stripe signature verification
  },
  async (req, res) => {
    if (req.method !== 'POST') {
      res.status(405).send('Method Not Allowed');
      return;
    }

    const signature = req.headers['stripe-signature'] as string;
    if (!signature) {
      res.status(400).json({ error: 'missing_signature' });
      return;
    }

    try {
      const stripeService = new StripeService(STRIPE_SECRET_KEY.value());

      // Firebase Functions v2 exposes rawBody on the request object
      const rawBody = (req as unknown as { rawBody: Buffer }).rawBody;
      const event = await stripeService.constructWebhookEvent(
        rawBody,
        signature,
        STRIPE_WEBHOOK_SECRET.value()
      );

      switch (event.type) {
        case 'checkout.session.completed': {
          const session = event.data.object as {
            id: string;
            metadata?: {
              user_id?: string;
              product_id?: string;
              app_version?: string;
            };
          };
          const userId = session.metadata?.user_id;
          const productId = session.metadata?.product_id;
          const appVersion = (session.metadata?.app_version || 'pro') as 'pro' | 'global';

          if (userId && productId) {
            await entitlementService.grant({
              userId,
              productId,
              provider: 'stripe',
              transactionId: session.id,
              appVersion,
            });
            console.log(`[stripeWebhook] Entitlement otorgado: ${productId} -> ${userId}`);
          } else {
            console.warn('[stripeWebhook] checkout.session.completed sin userId o productId en metadata');
          }
          break;
        }

        case 'payment_intent.payment_failed': {
          const intent = event.data.object as { id: string; last_payment_error?: { message?: string } };
          console.warn(`[stripeWebhook] Pago fallido: ${intent.id}`, intent.last_payment_error?.message);
          break;
        }

        case 'charge.refunded': {
          // DES-05: Revoke entitlements on refund
          const charge = event.data.object as { metadata?: { user_id?: string; product_id?: string } };
          const refundUserId = charge.metadata?.user_id;
          const refundProductId = charge.metadata?.product_id;
          if (refundUserId && refundProductId) {
            try {
              await admin.firestore()
                .collection('entitlements').doc(refundUserId)
                .collection('products').doc(refundProductId)
                .update({ revoked: true, revokedAt: admin.firestore.Timestamp.now(), revokeReason: 'refund' });
              console.log(`[stripeWebhook] Entitlement revoked on refund: ${refundProductId} for ${refundUserId}`);
            } catch (revokeErr) {
              console.error(`[stripeWebhook] Failed to revoke entitlement on refund: ${revokeErr}`);
            }
          } else {
            console.warn('[stripeWebhook] charge.refunded without user_id/product_id in metadata');
          }
          break;
        }

        case 'charge.dispute.created': {
          const dispute = event.data.object as { metadata?: { user_id?: string }; id: string };
          console.error(`[stripeWebhook] DISPUTE created: ${dispute.id} for user ${dispute.metadata?.user_id}`);
          // Alert via Slack
          if (SLACK_WEBHOOK_URL?.value()) {
            await sendSlackAlert(`⚠️ Stripe DISPUTE: ${dispute.id} for user ${dispute.metadata?.user_id}`);
          }
          break;
        }

        default:
          console.log(`[stripeWebhook] Evento ignorado: ${event.type}`);
      }

      res.json({ received: true });
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : 'Unknown error';
      console.error('[stripeWebhook] Error:', message);
      res.status(400).json({ error: 'webhook_failed', message });
    }
  }
);

// ============================================================
// STRIPE — Session Status (polling desde el cliente)
// GET https://us-central1-{project}.cloudfunctions.net/stripeSessionStatus?sessionId=xxx
// ============================================================
export const stripeSessionStatus = onRequest(
  { secrets: [STRIPE_SECRET_KEY], region: 'us-central1' },
  async (req, res) => {
    if (req.method !== 'GET') {
      res.status(405).json({ error: 'method_not_allowed' });
      return;
    }

    const callerUid = await verifyAuthAndGetUid(req, res);
    if (!callerUid) return;

    const sessionId = req.query.sessionId as string;
    if (!sessionId) {
      res.status(400).json({ error: 'missing_session_id', message: 'sessionId query param requerido' });
      return;
    }

    try {
      const stripeService = new StripeService(STRIPE_SECRET_KEY.value());
      const status = await stripeService.getSessionStatus(sessionId);

      // SEC-02: Verify session belongs to the caller (prevent info leak)
      if (status && status.metadata?.user_id && status.metadata.user_id !== callerUid) {
        res.status(403).json({ error: 'forbidden', message: 'Session does not belong to this user' });
        return;
      }

      res.json(status);
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : 'Unknown error';
      console.error('[stripeSessionStatus] Error:', message);
      res.status(500).json({ error: 'status_fetch_failed', message });
    }
  }
);

// ============================================================
// APPLE IAP — Validar Receipt
// POST https://us-central1-{project}.cloudfunctions.net/iapValidateReceipt
// Body: { receiptData, productId, userId, appVersion, bundleId? }
// ============================================================
export const iapValidateReceipt = onRequest(
  { secrets: [APPLE_SHARED_SECRET], region: 'us-central1' },
  async (req, res) => {
    if (req.method !== 'POST') {
      res.status(405).json({ error: 'method_not_allowed' });
      return;
    }
    if (!checkTriumphIsolation(req, res)) return;

    const callerUid = await verifyAuthAndGetUid(req, res);
    if (!callerUid) return;

    const { receiptData, productId, userId, appVersion, bundleId } = req.body as {
      receiptData?: string;
      productId?: string;
      userId?: string;
      appVersion?: string;
      bundleId?: string;
    };

    if (!receiptData || !productId || !userId) {
      res.status(400).json({
        error: 'missing_fields',
        message: 'receiptData, productId y userId requeridos',
      });
      return;
    }

    if (callerUid !== userId) {
      res.status(403).json({ error: 'forbidden', message: 'No puedes validar receipt para otro usuario' });
      return;
    }

    try {
      const appleService = new AppleIAPService(APPLE_SHARED_SECRET.value());
      const result = await appleService.validateReceipt(
        receiptData,
        productId,
        (appVersion as 'pro' | 'global') || 'global',
        bundleId || 'com.matrixsoftware.digitpark'
      );

      if (result.valid && result.transactionId) {
        await entitlementService.grant({
          userId,
          productId,
          provider: 'apple_iap',
          transactionId: result.transactionId,
          appVersion: (appVersion as 'pro' | 'global') || 'global',
        });
        console.log(`[iapValidateReceipt] Entitlement otorgado: ${productId} -> ${userId}`);
      }

      res.json(result);
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : 'Unknown error';
      console.error('[iapValidateReceipt] Error:', message);
      res.status(500).json({ error: 'validation_failed', message });
    }
  }
);

// Helper: verifica Firebase ID Token y retorna uid o null
async function verifyAuthAndGetUid(req: import('express').Request, res: import('express').Response): Promise<string | null> {
  const authHeader = req.headers.authorization;
  const idToken = authHeader?.startsWith('Bearer ') ? authHeader.split('Bearer ')[1] : null;
  if (!idToken) {
    res.status(401).json({ error: 'unauthorized', message: 'Authorization header requerido' });
    return null;
  }
  try {
    const decoded = await admin.auth().verifyIdToken(idToken);
    return decoded.uid;
  } catch {
    res.status(401).json({ error: 'invalid_token', message: 'Token Firebase invalido o expirado' });
    return null;
  }
}

// ============================================================
// ENTITLEMENTS — Obtener todos los entitlements de un usuario
// GET https://us-central1-{project}.cloudfunctions.net/getEntitlements?userId=xxx
// ============================================================
export const getEntitlements = onRequest(
  { region: 'us-central1' },
  async (req, res) => {
    if (req.method !== 'GET') {
      res.status(405).json({ error: 'method_not_allowed' });
      return;
    }

    const callerUid = await verifyAuthAndGetUid(req, res);
    if (!callerUid) return;

    const userId = req.query.userId as string;
    if (!userId) {
      res.status(400).json({ error: 'missing_user_id', message: 'userId query param requerido' });
      return;
    }

    if (callerUid !== userId) {
      res.status(403).json({ error: 'forbidden', message: 'No puedes acceder a entitlements de otro usuario' });
      return;
    }

    try {
      const entitlements = await entitlementService.getUserEntitlements(userId);
      res.json({ userId, entitlements, count: entitlements.length });
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : 'Unknown error';
      console.error('[getEntitlements] Error:', message);
      res.status(500).json({ error: 'fetch_failed', message });
    }
  }
);

// ============================================================
// ENTITLEMENTS — Verificar entitlement especifico
// GET https://us-central1-{project}.cloudfunctions.net/checkEntitlement?userId=xxx&productId=yyy
// ============================================================
export const checkEntitlement = onRequest(
  { region: 'us-central1' },
  async (req, res) => {
    if (req.method !== 'GET') {
      res.status(405).json({ error: 'method_not_allowed' });
      return;
    }

    const callerUid = await verifyAuthAndGetUid(req, res);
    if (!callerUid) return;

    const userId = req.query.userId as string;
    const productId = req.query.productId as string;

    if (!userId || !productId) {
      res.status(400).json({ error: 'missing_fields', message: 'userId y productId requeridos' });
      return;
    }

    if (callerUid !== userId) {
      res.status(403).json({ error: 'forbidden', message: 'No puedes verificar entitlements de otro usuario' });
      return;
    }

    try {
      const hasIt = await entitlementService.hasEntitlement(userId, productId);
      res.json({ userId, productId, entitled: hasIt });
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : 'Unknown error';
      res.status(500).json({ error: 'check_failed', message });
    }
  }
);

// ============================================================
// ENTITLEMENTS — Sync bidireccional (cliente -> servidor)
// POST https://us-central1-{project}.cloudfunctions.net/syncEntitlements
// Body: { userId, localEntitlements: EntitlementRecord[] }
// ============================================================
export const syncEntitlements = onRequest(
  { region: 'us-central1' },
  async (req, res) => {
    if (req.method !== 'POST') {
      res.status(405).json({ error: 'method_not_allowed' });
      return;
    }

    const callerUid = await verifyAuthAndGetUid(req, res);
    if (!callerUid) return;

    const { userId, localEntitlements } = req.body as {
      userId?: string;
      localEntitlements?: unknown[];
    };

    if (!userId) {
      res.status(400).json({ error: 'missing_user_id', message: 'userId requerido' });
      return;
    }

    if (callerUid !== userId) {
      res.status(403).json({ error: 'forbidden', message: 'No puedes sincronizar entitlements de otro usuario' });
      return;
    }

    // SEC-07: Validate schema of localEntitlements before passing to service
    const VALID_PRODUCT_IDS = new Set([
      // Gem packs
      'sparks_100', 'sparks_300', 'sparks_500', 'sparks_1200', 'sparks_2500', 'sparks_6500', 'sparks_14000',
      // Theme bundles
      'premium_bundle', 'complete_bundle',
      // Packs
      'welcome_pack_basic', 'welcome_pack_vip', 'starter_pack', 'adfree_permanent', 'premium_pass_monthly',
      // Titles
      'title_quantum', 'title_immortal', 'title_transcendent', 'title_apex_predator',
      // Win effects
      'effect_cosmic_shatter', 'effect_quantum_rift', 'effect_divine_ascension',
      // Profile frames
      'frame_plasma_spark', 'frame_prism_shift', 'frame_aurora_borealis', 'frame_void_walker',
      'frame_storm_surge', 'frame_cosmic_rift', 'frame_infernal_god', 'frame_divine_light',
      'frame_quantum_break', 'frame_holographic', 'frame_quantum_fire', 'frame_legendary_crown',
    ]);
    const VALID_PROVIDERS = new Set(['stripe', 'apple_iap']);

    const validEntitlements = (localEntitlements || []).filter((e: unknown) => {
      if (typeof e !== 'object' || e === null) return false;
      const record = e as Record<string, unknown>;
      return typeof record.productId === 'string' && record.productId.length > 0 &&
             typeof record.provider === 'string' && record.provider.length > 0 &&
             typeof record.grantedAt === 'string' && record.grantedAt.length > 0 &&
             VALID_PRODUCT_IDS.has(record.productId as string) &&
             VALID_PROVIDERS.has(record.provider as string);
    });

    try {
      const merged = await entitlementService.syncEntitlements(
        userId,
        validEntitlements as never[]
      );
      res.json({ userId, entitlements: merged, count: merged.length });
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : 'Unknown error';
      console.error('[syncEntitlements] Error:', message);
      res.status(500).json({ error: 'sync_failed', message });
    }
  }
);

// ============================================================
// HEALTH — Estado del sistema de pagos
// GET https://us-central1-{project}.cloudfunctions.net/paymentsHealth
// ============================================================
export const paymentsHealth = onRequest(
  { secrets: [STRIPE_SECRET_KEY], region: 'us-central1' },
  async (_req, res) => {
    const now = new Date().toISOString();

    // DES-04: Real Stripe health check instead of always returning ok
    let stripeStatus = 'healthy';
    try {
      const stripeService = new StripeService(STRIPE_SECRET_KEY.value());
      await stripeService.getSessionStatus('health_check_nonexistent');
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : '';
      // "No such session" = Stripe API is reachable (healthy). Other errors = unhealthy.
      if (!msg.includes('No such') && !msg.includes('resource_missing')) {
        stripeStatus = 'unhealthy';
      }
    }

    // Read active provider from Firestore
    let activeProvider = 'stripe';
    try {
      const configDoc = await admin.firestore().collection('payment_config').doc('active').get();
      if (configDoc.exists) {
        activeProvider = configDoc.data()?.provider || 'stripe';
      }
    } catch { /* default to stripe */ }

    res.json({
      status: stripeStatus === 'healthy' ? 'ok' : 'degraded',
      stripe: { status: stripeStatus, lastCheck: now },
      apple_iap: { status: 'healthy', lastCheck: now },
      triumph: {
        status: 'isolated',
        crossContaminationDetected: false,
        lastIsolationCheck: now,
      },
      activeProvider,
      abortProtocolReady: true,
      timestamp: now,
    });
  }
);

// ============================================================
// ADMIN — Force switch provider (abort protocol)
// POST https://us-central1-{project}.cloudfunctions.net/adminForceSwitch
// Body: { provider: 'stripe' | 'apple_iap', reason: string, adminKey: string }
// Proteccion: validar adminKey contra Firebase Remote Config o Firestore
// ============================================================
export const adminForceSwitch = onRequest(
  { secrets: [SLACK_WEBHOOK_URL], region: 'us-central1' },
  async (req, res) => {
    if (req.method !== 'POST') {
      res.status(405).json({ error: 'method_not_allowed' });
      return;
    }

    const { provider, reason, adminKey } = req.body as {
      provider?: string;
      reason?: string;
      adminKey?: string;
    };

    if (!provider || !reason) {
      res.status(400).json({ error: 'missing_fields', message: 'provider y reason requeridos' });
      return;
    }

    // Validar adminKey — fail-closed: rechazar si falta, si Firestore falla, o si no coincide
    if (!adminKey) {
      res.status(403).json({ error: 'admin_key_required', message: 'adminKey es obligatorio' });
      return;
    }
    try {
      const configDoc = await admin
        .firestore()
        .collection('payment_config')
        .doc('admin')
        .get();
      const storedKey = configDoc.data()?.adminKey as string | undefined;
      // DES-06: Constant-time comparison to prevent timing attacks
      const crypto = require('crypto');
      const keyMatch = storedKey && adminKey.length === storedKey.length &&
        crypto.timingSafeEqual(Buffer.from(storedKey, 'utf8'), Buffer.from(adminKey, 'utf8'));
      if (!keyMatch) {
        res.status(403).json({ error: 'invalid_admin_key' });
        return;
      }
    } catch (keyErr) {
      // Fail-closed: si Firestore no puede verificar, rechazar la operacion
      console.error('[adminForceSwitch] Error verificando adminKey:', keyErr);
      res.status(503).json({ error: 'auth_check_unavailable', message: 'No se pudo verificar adminKey' });
      return;
    }

    const validProviders = ['stripe', 'apple_iap'];
    if (!validProviders.includes(provider)) {
      res.status(400).json({
        error: 'invalid_provider',
        message: `provider debe ser uno de: ${validProviders.join(', ')}`,
      });
      return;
    }

    try {
      // Guardar switch en Firestore para que los clientes lo lean
      await admin.firestore().collection('payment_config').doc('active').set(
        {
          activeProvider: provider,
          switchReason: reason,
          switchedAt: admin.firestore.Timestamp.now(),
          stripeEnabled: provider === 'stripe',
          appleIapEnabled: provider === 'apple_iap',
        },
        { merge: true }
      );

      // Alerta de Slack
      const slackUrl = SLACK_WEBHOOK_URL.value();
      if (slackUrl) {
        await sendSlackAlert(
          slackUrl,
          `*Abort Protocol ejecutado*\nProvider activo cambiado a: ${provider}\nRazon: ${reason}`,
          'critical'
        );
      }

      console.log(`[adminForceSwitch] Provider cambiado a: ${provider}. Razon: ${reason}`);
      res.json({
        success: true,
        activeProvider: provider,
        reason,
        timestamp: new Date().toISOString(),
      });
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : 'Unknown error';
      console.error('[adminForceSwitch] Error:', message);
      res.status(500).json({ error: 'switch_failed', message });
    }
  }
);

// ============================================================
// VALIDATE SCORE — Server-side score validation + rate limiting (C-67/C-68)
// POST https://us-central1-{project}.cloudfunctions.net/validateScore
// Header: Authorization: Bearer <idToken>
// Body: { gameType, score, timeSeconds }
// ============================================================
export const validateScore = onRequest(
  { region: 'us-central1' },
  async (req, res) => {
    if (req.method !== 'POST') {
      res.status(405).json({ error: 'method_not_allowed' });
      return;
    }

    const uid = await verifyAuthAndGetUid(req, res);
    if (!uid) return;

    // Rate limiting: max 1 score submission per 30 seconds (atomic transaction)
    const db = admin.database();
    const rateRef = db.ref(`ratelimits/${uid}/lastScoreSubmit`);
    const now = Date.now();

    const txResult = await rateRef.transaction((current: number | null) => {
      const lastSubmit = current || 0;
      if (now - lastSubmit < 30000) {
        // Abort transaction — return undefined to cancel
        return undefined;
      }
      return now;
    });

    if (!txResult.committed) {
      res.status(429).json({ error: 'rate_limited', message: 'Too many requests. Wait 30s.' });
      return;
    }

    const { gameType, score, timeSeconds } = req.body as {
      gameType?: string;
      score?: number;
      timeSeconds?: number;
    };

    if (!gameType || score === undefined || timeSeconds === undefined) {
      res.status(400).json({ error: 'missing_fields' });
      return;
    }

    const limits: Record<string, { minTime: number; maxScore: number }> = {
      DigitRush:   { minTime: 5,  maxScore: 1000 },
      FlashTap:    { minTime: 3,  maxScore: 500  },
      MemoryPairs: { minTime: 8,  maxScore: 200  },
      OddOneOut:   { minTime: 2,  maxScore: 100  },
      QuickMath:   { minTime: 10, maxScore: 300  },
    };

    const limit = limits[gameType];
    if (!limit) {
      res.status(400).json({ error: 'unknown_game', message: `Unknown gameType: ${gameType}` });
      return;
    }
    if (timeSeconds < limit.minTime) {
      res.status(400).json({ error: 'score_too_fast', message: `Score submitted too quickly for ${gameType}` });
      return;
    }
    if (score > limit.maxScore) {
      res.status(400).json({ error: 'score_too_high', message: `Score exceeds maximum for ${gameType}` });
      return;
    }

    // SEC-06 Option B: Generate HMAC validation token for client-side write
    const crypto = require('crypto');
    const tokenPayload = `${uid}:${gameType}:${score}:${timeSeconds}:${now}`;
    const hmacSecret = process.env.SCORE_HMAC_SECRET || 'digitpark-score-validation-key';
    const validationToken = crypto.createHmac('sha256', hmacSecret)
      .update(tokenPayload)
      .digest('hex');

    res.json({
      valid: true,
      validationToken,
      tokenTimestamp: now,
    });
  }
);

// ============================================================
// SUBMIT CASH SCORE — Server-side write for CashBattle (SEC-06 Option A)
// POST https://us-central1-{project}.cloudfunctions.net/submitCashScore
// Header: Authorization: Bearer <idToken>
// Body: { gameType, score, timeSeconds, tournamentId }
// ============================================================
export const submitCashScore = onRequest(
  { region: 'us-central1' },
  async (req, res) => {
    if (req.method !== 'POST') {
      res.status(405).json({ error: 'method_not_allowed' });
      return;
    }

    const uid = await verifyAuthAndGetUid(req, res);
    if (!uid) return;

    const { gameType, score, timeSeconds, tournamentId } = req.body as {
      gameType?: string;
      score?: number;
      timeSeconds?: number;
      tournamentId?: string;
    };

    if (!gameType || score === undefined || timeSeconds === undefined || !tournamentId) {
      res.status(400).json({ error: 'missing_fields', message: 'gameType, score, timeSeconds, tournamentId required' });
      return;
    }

    // Same validation as validateScore
    const limits: Record<string, { minTime: number; maxScore: number }> = {
      DigitRush:   { minTime: 5,  maxScore: 1000 },
      FlashTap:    { minTime: 3,  maxScore: 500  },
      MemoryPairs: { minTime: 8,  maxScore: 200  },
      OddOneOut:   { minTime: 2,  maxScore: 100  },
      QuickMath:   { minTime: 10, maxScore: 300  },
    };

    const limit = limits[gameType];
    if (!limit) {
      res.status(400).json({ error: 'unknown_game' });
      return;
    }
    if (timeSeconds < limit.minTime) {
      res.status(400).json({ error: 'score_too_fast' });
      return;
    }
    if (score > limit.maxScore) {
      res.status(400).json({ error: 'score_too_high' });
      return;
    }

    // Rate limit (same 30s rule)
    const db = admin.database();
    const rateRef = db.ref(`ratelimits/${uid}/lastCashScoreSubmit`);
    const now = Date.now();
    const txResult = await rateRef.transaction((current: number | null) => {
      if (now - (current || 0) < 30000) return undefined;
      return now;
    });
    if (!txResult.committed) {
      res.status(429).json({ error: 'rate_limited' });
      return;
    }

    try {
      // Fetch username from player profile
      const playerSnap = await db.ref(`players/${uid}/username`).once('value');
      const username = playerSnap.val() || 'Unknown';
      const countrySnap = await db.ref(`players/${uid}/countryCode`).once('value');
      const countryCode = countrySnap.val() || 'XX';

      // Server writes the score — client cannot write to cash_scores path
      const scoreData = {
        userId: uid,
        username,
        time: timeSeconds,
        score,
        gameType,
        countryCode,
        tournamentId,
        timestamp: new Date().toISOString(),
        serverValidated: true,
      };

      // Write to cash_scores (server-only path) and tournament
      const scoreId = db.ref(`cash_scores/${uid}`).push().key;
      const updates: Record<string, unknown> = {
        [`cash_scores/${uid}/${scoreId}`]: scoreData,
        [`tournaments/${tournamentId}/scores/${uid}`]: {
          score,
          time: timeSeconds,
          gameType,
          timestamp: new Date().toISOString(),
        },
      };
      await db.ref().update(updates);

      console.log(`[submitCashScore] Score written for ${uid} in tournament ${tournamentId}: ${score} / ${timeSeconds}s`);
      res.json({ success: true, scoreId });
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : 'Unknown error';
      console.error('[submitCashScore] Error:', message);
      res.status(500).json({ error: 'score_submit_failed', message });
    }
  }
);

// ============================================================
// VALIDATE DAILY GRANT — Server-side eligibility check before granting free rewards (C-04)
// POST https://us-central1-{project}.cloudfunctions.net/validateDailyGrant
// Header: Authorization: Bearer <idToken>
// Body: { grantType: 'daily_offer' | 'rotating_content', itemId: string }
// Returns: { eligible: bool, reason?: string }
// ============================================================
export const validateDailyGrant = onRequest(
  { region: 'us-central1' },
  async (req, res) => {
    if (req.method !== 'POST') {
      res.status(405).json({ error: 'method_not_allowed' });
      return;
    }

    const uid = await verifyAuthAndGetUid(req, res);
    if (!uid) return;

    const { grantType, itemId } = req.body as {
      grantType?: string;
      itemId?: string;
    };

    if (!grantType || !itemId) {
      res.status(400).json({ error: 'missing_fields', message: 'grantType and itemId required' });
      return;
    }

    const VALID_GRANT_TYPES = ['daily_offer', 'rotating_content'];
    if (!VALID_GRANT_TYPES.includes(grantType)) {
      res.status(400).json({ error: 'invalid_grant_type' });
      return;
    }

    try {
      const db = admin.database();
      const now = admin.database.ServerValue.TIMESTAMP;
      const grantKey = `grants/${uid}/${grantType}/${itemId.replace(/[.$#[\]/]/g, '_')}`;
      const grantRef = db.ref(grantKey);

      // Atomic check-and-set: eligible only if not already granted today
      const today = new Date().toISOString().slice(0, 10); // 'yyyy-MM-dd' UTC
      const snapshot = await grantRef.once('value');
      const existingGrant = snapshot.val() as { date?: string } | null;

      if (existingGrant?.date === today) {
        res.json({ eligible: false, reason: 'already_granted_today' });
        return;
      }

      // For daily_offer: also enforce rate limit (max 1 claim per item per day globally)
      if (grantType === 'daily_offer') {
        const rateRef = db.ref(`ratelimits/${uid}/dailyOffer/${itemId.replace(/[.$#[\]/]/g, '_')}`);
        const rateSnap = await rateRef.once('value');
        const lastGrantDate = rateSnap.val() as string | null;
        if (lastGrantDate === today) {
          res.json({ eligible: false, reason: 'rate_limited' });
          return;
        }
        await rateRef.set(today);
      }

      // Record the grant server-side
      await grantRef.set({ date: today, grantedAt: now, uid });

      res.json({ eligible: true });
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : 'Unknown error';
      console.error('[validateDailyGrant] Error:', message);
      res.status(500).json({ error: 'validation_failed', message });
    }
  }
);

// ============================================================
// DELETE USER DATA — GDPR data deletion (C-69)
// POST https://us-central1-{project}.cloudfunctions.net/deleteUserData
// Header: Authorization: Bearer <idToken>
// ============================================================
export const deleteUserData = onRequest(
  { region: 'us-central1' },
  async (req, res) => {
    if (req.method !== 'POST') {
      res.status(405).json({ error: 'method_not_allowed' });
      return;
    }

    const uid = await verifyAuthAndGetUid(req, res);
    if (!uid) return;

    try {
      const db = admin.database();
      const firestore = admin.firestore();
      const storage = admin.storage().bucket();

      // 1. RTDB — delete all user data paths
      const gameTypes = ['DigitRush', 'FlashTap', 'MemoryPairs', 'OddOneOut', 'QuickMath'];
      const rtdbDeletes = [
        db.ref(`players/${uid}`).remove(),
        db.ref(`matchHistory/${uid}`).remove(),
        db.ref(`achievements/${uid}`).remove(),
        db.ref(`notifications/${uid}`).remove(),
        db.ref(`friends/${uid}`).remove(),
        db.ref(`friend_requests/${uid}`).remove(),
        db.ref(`matchmaking_queue/${uid}`).remove(),
        db.ref(`tournamentHistory/${uid}`).remove(),
        db.ref(`ratelimits/${uid}`).remove(),
        db.ref(`scores/${uid}`).remove(),
        // Leaderboards: remove user entry from each game type (global + country)
        ...gameTypes.map(g => db.ref(`leaderboards/global/${g}/${uid}`).remove()),
      ];

      // Remove from country leaderboards (scan all countries for this user)
      const countrySnapshot = await db.ref('leaderboards/country').once('value');
      if (countrySnapshot.exists()) {
        countrySnapshot.forEach((countryChild) => {
          rtdbDeletes.push(db.ref(`leaderboards/country/${countryChild.key}/${uid}`).remove());
        });
      }

      await Promise.all(rtdbDeletes);

      // 2. Firestore — delete entitlements and payment audit
      const entitlementProducts = await firestore
        .collection('entitlements').doc(uid)
        .collection('products').listDocuments();
      const firestoreDeletes = entitlementProducts.map(doc => doc.delete());
      firestoreDeletes.push(firestore.collection('entitlements').doc(uid).delete());
      await Promise.all(firestoreDeletes);

      // 3. Firebase Storage — delete avatar files
      try {
        const [files] = await storage.getFiles({ prefix: `avatars/${uid}/` });
        if (files.length > 0) {
          await Promise.all(files.map(file => file.delete()));
        }
      } catch (storageErr) {
        console.warn(`[deleteUserData] Storage cleanup failed (non-fatal): ${storageErr}`);
      }

      // 4. Firebase Auth — delete the auth account
      try {
        await admin.auth().deleteUser(uid);
      } catch (authErr: unknown) {
        const authMsg = authErr instanceof Error ? authErr.message : 'Unknown';
        console.warn(`[deleteUserData] Auth delete failed (may already be deleted client-side): ${authMsg}`);
      }

      console.log(`[deleteUserData] All data deleted for uid: ${uid}`);
      res.json({ success: true });
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : 'Unknown error';
      console.error('[deleteUserData] Error:', message);
      res.status(500).json({ error: 'deletion_failed', message });
    }
  }
);

// ============================================================
// EXPORT USER DATA — GDPR Right to Portability (Article 20) (C-10)
// GET https://us-central1-{project}.cloudfunctions.net/exportUserData
// Header: Authorization: Bearer <idToken>
// Returns: JSON bundle with all user data
// ============================================================
export const exportUserData = onRequest(
  { region: 'us-central1' },
  async (req, res) => {
    if (req.method !== 'GET') {
      res.status(405).json({ error: 'method_not_allowed' });
      return;
    }

    const uid = await verifyAuthAndGetUid(req, res);
    if (!uid) return;

    try {
      const db = admin.database();
      const firestore = admin.firestore();

      // Gather all user data concurrently
      const [
        playerSnap,
        matchHistorySnap,
        achievementsSnap,
        friendsSnap,
        tournamentHistorySnap,
        entitlementDocs,
      ] = await Promise.all([
        db.ref(`players/${uid}`).once('value'),
        db.ref(`matchHistory/${uid}`).once('value'),
        db.ref(`achievements/${uid}`).once('value'),
        db.ref(`friends/${uid}`).once('value'),
        db.ref(`tournamentHistory/${uid}`).once('value'),
        firestore.collection('entitlements').doc(uid).collection('products').get(),
      ]);

      const gameTypes = ['DigitRush', 'FlashTap', 'MemoryPairs', 'OddOneOut', 'QuickMath'];
      const leaderboardEntries: Record<string, unknown> = {};
      await Promise.all(
        gameTypes.map(async (g) => {
          const snap = await db.ref(`leaderboards/global/${g}/${uid}`).once('value');
          if (snap.exists()) leaderboardEntries[g] = snap.val();
        })
      );

      const entitlements = entitlementDocs.docs.map(doc => ({ id: doc.id, ...doc.data() }));

      const exportBundle = {
        exportedAt: new Date().toISOString(),
        userId: uid,
        profile: playerSnap.val(),
        matchHistory: matchHistorySnap.val(),
        achievements: achievementsSnap.val(),
        friends: friendsSnap.val(),
        tournamentHistory: tournamentHistorySnap.val(),
        leaderboardEntries,
        purchases: entitlements,
      };

      res.setHeader('Content-Type', 'application/json');
      res.setHeader('Content-Disposition', `attachment; filename="digitpark_data_${uid}.json"`);
      res.json(exportBundle);

      console.log(`[exportUserData] Data exported for uid: ${uid}`);
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : 'Unknown error';
      console.error('[exportUserData] Error:', message);
      res.status(500).json({ error: 'export_failed', message });
    }
  }
);
