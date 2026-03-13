export interface CheckoutSessionRequest {
  productId: string;
  userId: string;
  appVersion: 'pro' | 'global';
  priceId?: string;
}

export interface CheckoutSessionResponse {
  sessionId: string;
  checkoutUrl: string;
}

export interface SessionStatusResponse {
  status: 'pending' | 'complete' | 'expired' | 'canceled';
  sessionId: string;
  completedAt?: string;
}

export interface PaymentHealth {
  stripe: { status: string; lastCheck: string };
  apple_iap: { status: string; lastCheck: string };
  triumph: { status: 'isolated'; crossContaminationDetected: boolean };
  activeProvider: 'stripe' | 'apple_iap';
  abortProtocolReady: boolean;
}
