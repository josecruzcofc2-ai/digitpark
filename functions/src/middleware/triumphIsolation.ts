import { Request, Response } from 'express';
import { PROHIBITED_TERMS } from '../config/environment';

/**
 * Guard de aislamiento Triumph<->Stripe.
 * Rechaza cualquier request que contenga terminos de real-money gaming.
 * Retorna true si la request pasa la validacion.
 */
export function checkTriumphIsolation(req: Request, res: Response): boolean {
  const bodyStr = JSON.stringify(req.body || {}).toLowerCase();

  for (const term of PROHIBITED_TERMS) {
    if (bodyStr.includes(term)) {
      console.error(`[IsolationGuard] CRITICO: Termino prohibido en request: '${term}'`);
      res.status(403).json({
        error: 'isolation_violation',
        message: 'Request contains prohibited terms',
        timestamp: new Date().toISOString(),
      });
      return false;
    }
  }

  // Verificar que no haya headers de Triumph en endpoints de Stripe
  const triumphHeader = req.headers['x-triumph-token'] || req.headers['x-triumph-api-key'];
  if (triumphHeader) {
    console.error('[IsolationGuard] Header de Triumph detectado en endpoint de pago cosmetico');
    res.status(403).json({ error: 'isolation_violation', message: 'Triumph headers not allowed' });
    return false;
  }

  return true;
}

/**
 * Verifica que la version de la app puede acceder al endpoint solicitado.
 */
export function checkVersionAccess(req: Request, res: Response, requirePro = false): boolean {
  const appVersion = req.headers['x-app-version'] as string;

  if (requirePro && appVersion !== 'pro' && appVersion !== 'development') {
    res.status(403).json({
      error: 'version_not_allowed',
      message: `App version '${appVersion}' cannot access this endpoint. Requires 'pro'.`,
    });
    return false;
  }

  return true;
}
