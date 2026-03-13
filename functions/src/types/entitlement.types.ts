export interface EntitlementRecord {
  userId: string;
  productId: string;
  provider: 'stripe' | 'apple_iap'; // NUNCA 'triumph'
  transactionId: string;
  grantedAt: FirebaseFirestore.Timestamp | Date;
  appVersion: 'pro' | 'global';
  hasTournamentBenefit: false;       // SIEMPRE false
  isCosmetic: true;                  // SIEMPRE true
}

export interface EntitlementGrantRequest {
  userId: string;
  productId: string;
  provider: 'stripe' | 'apple_iap';
  transactionId: string;
  appVersion: 'pro' | 'global';
}
