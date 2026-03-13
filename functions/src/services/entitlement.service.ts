import * as admin from 'firebase-admin';
import { EntitlementRecord, EntitlementGrantRequest } from '../types/entitlement.types';

/**
 * Gestion de entitlements usando Firestore.
 * Coleccion: /entitlements/{userId}/products/{productId}
 * provider SIEMPRE es 'stripe' o 'apple_iap' — NUNCA 'triumph'.
 */
export class EntitlementService {
  private db: FirebaseFirestore.Firestore;

  constructor() {
    this.db = admin.firestore();
  }

  async grant(req: EntitlementGrantRequest): Promise<EntitlementRecord> {
    // Validacion critica de aislamiento
    if ((req.provider as string).includes('triumph')) {
      throw new Error('ISOLATION VIOLATION: provider no puede contener "triumph"');
    }

    const record: EntitlementRecord = {
      userId: req.userId,
      productId: req.productId,
      provider: req.provider,
      transactionId: req.transactionId,
      grantedAt: admin.firestore.Timestamp.now(),
      appVersion: req.appVersion,
      hasTournamentBenefit: false,
      isCosmetic: true,
    };

    // Guardar en Firestore con merge para evitar duplicados
    const docRef = this.db
      .collection('entitlements')
      .doc(req.userId)
      .collection('products')
      .doc(req.productId);

    await docRef.set(
      {
        ...record,
        transactions: admin.firestore.FieldValue.arrayUnion({
          transactionId: req.transactionId,
          provider: req.provider,
          grantedAt: admin.firestore.Timestamp.now(),
        }),
      },
      { merge: true }
    );

    // Log de auditoria
    await this.db.collection('payment_audit').add({
      userId: req.userId,
      productId: req.productId,
      provider: req.provider,
      action: 'grant',
      status: 'success',
      transactionId: req.transactionId,
      timestamp: admin.firestore.Timestamp.now(),
    });

    console.log(
      `[EntitlementService] Otorgado: ${req.productId} a ${req.userId} via ${req.provider}`
    );
    return record;
  }

  async getUserEntitlements(userId: string): Promise<EntitlementRecord[]> {
    const snapshot = await this.db
      .collection('entitlements')
      .doc(userId)
      .collection('products')
      .get();

    return snapshot.docs.map((doc) => doc.data() as EntitlementRecord);
  }

  async hasEntitlement(userId: string, productId: string): Promise<boolean> {
    const doc = await this.db
      .collection('entitlements')
      .doc(userId)
      .collection('products')
      .doc(productId)
      .get();
    return doc.exists;
  }

  async syncEntitlements(
    userId: string,
    localEntitlements: EntitlementRecord[]
  ): Promise<EntitlementRecord[]> {
    const batch = this.db.batch();

    for (const local of localEntitlements) {
      // Filtrar cualquier entrada que contenga 'triumph' — nunca debe llegar aqui, pero doble seguro
      if ((local.provider as string).includes('triumph')) continue;

      const docRef = this.db
        .collection('entitlements')
        .doc(userId)
        .collection('products')
        .doc(local.productId);

      batch.set(
        docRef,
        { ...local, isCosmetic: true, hasTournamentBenefit: false },
        { merge: true }
      );
    }

    await batch.commit();
    return await this.getUserEntitlements(userId);
  }
}
