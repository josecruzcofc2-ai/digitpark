import * as admin from 'firebase-admin';
import { onRequest } from 'firebase-functions/v2/https';
import { AppleIAPService } from './services/appleIAP.service';
import { EntitlementService } from './services/entitlement.service';
import {
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
// APPLE IAP — Validar Receipt
// POST https://us-central1-{project}.cloudfunctions.net/iapValidateReceipt
// Body: { receiptData, productId, userId, bundleId? }
// ============================================================
export const iapValidateReceipt = onRequest(
  { secrets: [APPLE_SHARED_SECRET], region: 'us-central1' },
  async (req, res) => {
    if (req.method !== 'POST') {
      res.status(405).json({ error: 'method_not_allowed' });
      return;
    }

    const callerUid = await verifyAuthAndGetUid(req, res);
    if (!callerUid) return;

    const { receiptData, productId, userId, bundleId } = req.body as {
      receiptData?: string;
      productId?: string;
      userId?: string;
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
        'global',
        bundleId || 'com.matrixsoftware.digitpark'
      );

      if (result.valid && result.transactionId) {
        await entitlementService.grant({
          userId,
          productId,
          provider: 'apple_iap',
          transactionId: result.transactionId,
          appVersion: 'global',
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

    const VALID_PRODUCT_IDS = new Set([
      // Gem packs
      'sparks_100', 'sparks_300', 'sparks_500', 'sparks_1200', 'sparks_2500', 'sparks_6500', 'sparks_14000',
      // Packs
      'welcome_pack_basic', 'welcome_pack_vip', 'adfree_permanent',
      // Titles
      'title_quantum', 'title_immortal', 'title_transcendent', 'title_apex_predator',
      // Win effects
      'effect_cosmic_shatter', 'effect_quantum_rift', 'effect_divine_ascension',
      // Profile frames
      'frame_plasma_spark', 'frame_prism_shift', 'frame_aurora_borealis', 'frame_void_walker',
      'frame_storm_surge', 'frame_cosmic_rift', 'frame_infernal_god', 'frame_divine_light',
      'frame_quantum_break', 'frame_holographic', 'frame_quantum_fire', 'frame_legendary_crown',
    ]);

    const validEntitlements = (localEntitlements || []).filter((e: unknown) => {
      if (typeof e !== 'object' || e === null) return false;
      const record = e as Record<string, unknown>;
      return typeof record.productId === 'string' && record.productId.length > 0 &&
             typeof record.provider === 'string' && record.provider === 'apple_iap' &&
             typeof record.grantedAt === 'string' && record.grantedAt.length > 0 &&
             VALID_PRODUCT_IDS.has(record.productId as string);
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
// HEALTH — Estado del sistema
// GET https://us-central1-{project}.cloudfunctions.net/paymentsHealth
// ============================================================
export const paymentsHealth = onRequest(
  { secrets: [SLACK_WEBHOOK_URL], region: 'us-central1' },
  async (_req, res) => {
    const now = new Date().toISOString();
    res.json({
      status: 'ok',
      apple_iap: { status: 'healthy', lastCheck: now },
      timestamp: now,
    });
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
