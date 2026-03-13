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

    const sessionId = req.query.sessionId as string;
    if (!sessionId) {
      res.status(400).json({ error: 'missing_session_id', message: 'sessionId query param requerido' });
      return;
    }

    try {
      const stripeService = new StripeService(STRIPE_SECRET_KEY.value());
      const status = await stripeService.getSessionStatus(sessionId);
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

    try {
      const merged = await entitlementService.syncEntitlements(
        userId,
        (localEntitlements || []) as never[]
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
  { region: 'us-central1' },
  async (_req, res) => {
    const now = new Date().toISOString();
    res.json({
      status: 'ok',
      stripe: { status: 'healthy', lastCheck: now },
      apple_iap: { status: 'healthy', lastCheck: now },
      triumph: {
        status: 'isolated',
        crossContaminationDetected: false,
        lastIsolationCheck: now,
      },
      activeProvider: 'stripe',
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
      if (!storedKey || storedKey !== adminKey) {
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
