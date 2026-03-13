import Stripe from 'stripe';
import { CheckoutSessionRequest, CheckoutSessionResponse } from '../types/payment.types';

const PROHIBITED_METADATA = [
  'tournament', 'prize', 'cash_game', 'real_money', 'entry_fee', 'gambling', 'triumph'
];

/**
 * Servicio de Stripe — solo procesa pagos cosmeticos.
 * NUNCA procesa metadata con terminos de real-money gaming.
 */
export class StripeService {
  private stripe: Stripe;

  constructor(secretKey: string) {
    this.stripe = new Stripe(secretKey, { apiVersion: '2023-10-16' });
  }

  async createCheckoutSession(req: CheckoutSessionRequest): Promise<CheckoutSessionResponse> {
    // Validacion de compliance antes de cualquier request a Stripe
    const allValues = JSON.stringify(req).toLowerCase();
    for (const term of PROHIBITED_METADATA) {
      if (allValues.includes(term) && term !== 'stripe') {
        throw new Error(`COMPLIANCE VIOLATION: metadata contiene termino prohibido: ${term}`);
      }
    }

    if (req.appVersion !== 'pro') {
      throw new Error('Stripe solo disponible para version Pro');
    }

    const sessionParams: Stripe.Checkout.SessionCreateParams = {
      mode: 'payment',
      payment_method_types: ['card'],
      metadata: {
        type: 'cosmetic',
        app: 'digit_park_pro',
        has_tournament_benefit: 'false',
        product_id: req.productId,
        user_id: req.userId,
        app_version: req.appVersion,
      },
      success_url: `digitpark://stripe-return?session_id={CHECKOUT_SESSION_ID}&status=complete`,
      cancel_url: `digitpark://stripe-return?session_id={CHECKOUT_SESSION_ID}&status=canceled`,
    };

    if (req.priceId) {
      sessionParams.line_items = [{ price: req.priceId, quantity: 1 }];
    }

    const session = await this.stripe.checkout.sessions.create(sessionParams);
    return { sessionId: session.id, checkoutUrl: session.url || '' };
  }

  async getSessionStatus(sessionId: string): Promise<{ status: string; sessionId: string }> {
    const session = await this.stripe.checkout.sessions.retrieve(sessionId);
    return { status: session.status || 'pending', sessionId: session.id };
  }

  async constructWebhookEvent(
    payload: Buffer | string,
    signature: string,
    secret: string
  ): Promise<Stripe.Event> {
    return this.stripe.webhooks.constructEvent(payload, signature, secret);
  }
}
