import axios from 'axios';
import { APPLE_PRODUCTION_URL, APPLE_SANDBOX_URL } from '../config/environment';

export interface ReceiptValidationResult {
  valid: boolean;
  transactionId?: string;
  error?: string;
}

interface AppleVerifyResponse {
  status: number;
  receipt?: {
    bundle_id?: string;
    in_app?: AppleInAppPurchase[];
  };
  latest_receipt_info?: AppleInAppPurchase[];
}

interface AppleInAppPurchase {
  product_id: string;
  transaction_id: string;
  purchase_date_ms?: string;
}

/**
 * Valida receipts de Apple IAP contra los servidores de Apple.
 * Produccion primero, sandbox como fallback (status 21007).
 */
export class AppleIAPService {
  constructor(private sharedSecret: string) {}

  async validateReceipt(
    receiptData: string,
    productId: string,
    appVersion: 'pro' | 'global',
    bundleId: string
  ): Promise<ReceiptValidationResult> {
    let response = await this.callApple(receiptData, APPLE_PRODUCTION_URL);

    // Status 21007 = receipt de sandbox, reintentar
    if (response.status === 21007) {
      console.log('[AppleIAP] Receipt es de sandbox, reintentando...');
      response = await this.callApple(receiptData, APPLE_SANDBOX_URL);
    }

    if (response.status !== 0) {
      return { valid: false, error: `apple_status_${response.status}` };
    }

    // Verificar bundle ID
    const expectedBundleId = appVersion === 'pro'
      ? 'com.matrixsoftware.digitpark.pro'
      : 'com.matrixsoftware.digitpark';

    if (response.receipt?.bundle_id && response.receipt.bundle_id !== expectedBundleId) {
      console.error(`[AppleIAP] Bundle ID no coincide: ${response.receipt.bundle_id}`);
      return { valid: false, error: 'bundle_id_mismatch' };
    }

    const transactions: AppleInAppPurchase[] = response.latest_receipt_info || [];
    const productTx = transactions.find((tx) => tx.product_id === productId);

    if (!productTx) {
      console.warn(`[AppleIAP] productId '${productId}' no encontrado en el receipt`);
      return { valid: false, error: 'product_not_found_in_receipt' };
    }

    return {
      valid: true,
      transactionId: productTx.transaction_id,
    };
  }

  private async callApple(receiptData: string, url: string): Promise<AppleVerifyResponse> {
    try {
      const body: Record<string, string> = { 'receipt-data': receiptData };
      if (this.sharedSecret) body.password = this.sharedSecret;
      const r = await axios.post<AppleVerifyResponse>(url, body, { timeout: 10000 });
      return r.data;
    } catch (err) {
      console.error('[AppleIAP] Error llamando Apple:', err);
      return { status: -1 };
    }
  }
}
