# Digit Park Pro — Guia de Compliance con Stripe

> **Version**: 1.0
> **Fecha**: 2026-03-10
> **Proposito**: Documentar como Digit Park Pro se presenta ante Stripe y por que la arquitectura es conforme con los Terminos de Servicio.

---

## 1. Descripcion del Negocio para Stripe

Cuando se configure la cuenta de Stripe, usar exactamente la siguiente descripcion en cada campo relevante del dashboard:

### Business Name (Legal)
```
Matrix Software LLC  (o razon social registrada)
```

### Business Description (en Stripe Dashboard → Business Settings → Business details)
```
Mobile game selling cosmetic digital goods. Players purchase virtual
currency (Sparks) and cosmetic theme packs for a skill-based mobile
game. All purchases are for non-fungible digital cosmetics with no
cash value. No gambling, wagering, or tournament entry fees are
processed through this account.
```

### Business Category
```
Category: Software → Mobile Apps & Games
Sub-category: Digital Goods / In-App Purchases
```

### Statement Descriptor (lo que aparece en el estado de cuenta del cliente)
```
DIGITPARK COSMETIC
```
*(max 22 caracteres, sin terminos financieros)*

### Support URL
```
https://digitpark.com/support
```

### Statement Descriptor Suffix (por producto, si se usa)
```
SPARKS  (para paquetes de moneda virtual)
THEME   (para bundles cosméticos)
```

---

## 2. Palabras Prohibidas en Cualquier Metadata de Stripe

Las siguientes palabras estan **absolutamente prohibidas** en cualquier campo que llegue a Stripe: nombres de productos, descripciones, metadata de sesiones, statement descriptors, o cualquier campo del API request.

### Lista Completa de Terminos Prohibidos

| Termino | Por que es prohibido |
|---|---|
| `tournament` | Implica competicion con premios monetarios |
| `prize` | Implica dinero real como recompensa |
| `cash_game` | Termino explicito de juego por dinero |
| `skill_game` | Termino regulado en varios estados de EE.UU. |
| `real_money` | Implica transaccion financiera real no cosmetica |
| `entry_fee` | Termino de torneos con dinero |
| `wager` | Apuesta — activa regulaciones de gambling |
| `bet` | Apuesta — activa regulaciones de gambling |
| `gambling` | Explicitamente prohibido en ToS de Stripe |
| `triumph` | Nombre del proveedor de skill gaming (Triumph.app) |
| `deposit` | Implica cuenta de usuario con saldo real |
| `withdrawal` | Implica retiro de fondos reales |
| `winnings` | Implica ganancias monetarias |

### Como se Aplica esta Restriccion en el Codigo

`StripeComplianceGuard.cs` valida estos terminos en tres lugares:

1. **Validacion de producto** (`ValidateProduct`): antes de crear cualquier sesion de Stripe, verifica `productId`, `displayName` y todos los valores de `Metadata`.
2. **Validacion de session metadata** (`ValidateSessionMetadata`): verifica que la metadata enviada al backend para crear la sesion cumpla con los campos obligatorios.
3. **Log de auditoria** (`LogComplianceAudit`): cada validacion (exitosa o fallida) queda registrada en `PlayerPrefs["dp_compliance_audit"]` con timestamp ISO 8601.

Si cualquier validacion falla, la compra es rechazada **antes de que se haga ningun request al backend o a Stripe**. El error queda en el log con el termino exacto que causo la violacion.

### Campos Obligatorios en Session Metadata

Cada sesion de Stripe DEBE incluir exactamente estos campos:
```json
{
  "type": "cosmetic",
  "app": "digit_park_pro",
  "has_tournament_benefit": "false"
}
```

Si `has_tournament_benefit != "false"` o `type != "cosmetic"`, la sesion es rechazada localmente.

---

## 3. Por que Esta Arquitectura NO Viola los ToS de Stripe

### 3.1 Que vende Digit Park Pro via Stripe

Digit Park Pro usa Stripe **exclusivamente** para:
- Paquetes de "Sparks" (moneda virtual cosmetica): $0.99 a $99.99
- Theme Bundle (15 temas cosméticos): $26.25
- Complete Theme Collection (20 temas cosmeticos): $30.45

Estos productos son **digital goods cosmeticos** — la categoria mas simple y aceptada por Stripe. Son equivalentes a comprar skins en Fortnite o temas en WhatsApp.

### 3.2 Que NO se vende via Stripe

| Actividad | Proveedor Correcto | Stripe? |
|---|---|---|
| Entry fee a torneo 1v1 CashBattle | Triumph SDK | NUNCA |
| Deposito de fondos jugables | Triumph SDK | NUNCA |
| Retiro de premios | Triumph SDK | NUNCA |
| Cualquier actividad de skill gaming | Triumph SDK | NUNCA |

Esta separacion es reforzada por:
1. Assembly definitions separadas (imposible importar TriumphManager desde el codigo de Stripe)
2. `VersionGuard.CanAccessStripe()` — verifica en runtime que el build sea Pro y que StripeEnabled sea true
3. `StripeComplianceGuard.ValidateProduct()` — rechaza productos con terminos prohibidos
4. Backend con `triumphIsolation.middleware.ts` — rechaza requests que contengan campos de Triumph
5. Builds separados con Scripting Defines diferentes (Pro vs Global)

### 3.3 Argumento Legal de Separacion

Desde la perspectiva de Stripe:

- **Quien paga**: Un jugador de un juego movil.
- **Por que paga**: Para obtener un tema visual o moneda virtual cosmetica.
- **Que recibe**: Un bien digital no fungible sin valor en efectivo.
- **Existe posibilidad de ganar dinero con la compra?**: NO. Los Sparks no se pueden convertir a dinero real. Los temas son cosmeticos puros.
- **Existe vinculo con torneos de dinero real?**: NO. La tienda cosmetica y el sistema Triumph son builds separados que nunca comparten sesion de pago.

Esto pone a Digit Park Pro en la misma categoria que miles de juegos moviles en la App Store que usan Stripe sin problemas.

---

## 4. Separacion Legal de Triumph vs Tienda Cosmetica

### 4.1 Triumph SDK (CashBattle)

Triumph es una plataforma de skill gaming (habilidad, no azar) que opera bajo sus propias licencias estatales en EE.UU. Triumph maneja:
- KYC (Know Your Customer) — verificacion de identidad
- Billetera de fondos reales
- Entry fees y distribucion de premios
- Cumplimiento estatal (no disponible en todos los estados)

Triumph tiene su propio sistema de pagos, sus propias cuentas bancarias, y sus propias relaciones regulatorias. **Triumph NUNCA procesa pagos a traves de la cuenta de Stripe de Digit Park.**

### 4.2 Separacion en el Codigo

```
ServiceLocator.cs (territorio exclusivo de Triumph)
  Registra: IKYCService, IWalletService, IMatchmakingService, ITournamentService

PaymentManager.cs (territorio de cosmeticos)
  Regla explicitada en comentario del codigo:
  "REGLA CRITICA: Este manager NUNCA importa ni referencia:
   - TriumphManager
   - ServiceLocator (el de CashBattle)
   - IWalletService, IKYCService, IMatchmakingService, ITournamentService
   - Cualquier namespace DigitPark.Services.Triumph"
```

Esta separacion esta documentada en el codigo fuente, en los assembly definitions, y en este documento — formando un trail de auditoria claro.

### 4.3 Separacion de Backend

El backend de cosmeticos (`api-pro.digitpark.com`) y el de Triumph son sistemas completamente separados:

- **Backend cosmeticos**: Firebase Cloud Functions. Solo conoce: Stripe, Apple IAP, Firebase.
- **Backend Triumph**: Proporcionado por Triumph SDK. Solo conoce: billeteras, KYC, torneos.

No hay endpoint compartido. No hay base de datos compartida. No hay credenciales compartidas.

---

## 5. Audit Trail Documentation

### 5.1 Registros en Cliente (PlayerPrefs)

Cada validacion de compliance queda registrada:
```
PlayerPrefs["dp_compliance_audit"]
Formato por entrada: {timestamp_ISO}|{productId}|{provider}|{passed}|{details}
Ejemplo exitoso:     2026-03-10T15:30:00Z|sparks_500|stripe|True|OK
Ejemplo fallido:     2026-03-10T15:31:00Z|triumph_entry|stripe|False|Termino prohibido: triumph
Max entradas: 100 (FIFO)
```

### 5.2 Registros en Firestore

La Cloud Function `stripeWebhook` registra en Firestore cada request a Stripe (coleccion `payment_audit`):
```
session_id        string   -- cs_live_xxx o cs_test_xxx
product_id        string   -- sparks_500, premium_bundle, etc.
user_id           string   -- Firebase UID
status            string   -- created, complete, expired, canceled
metadata_type     string   -- SIEMPRE "cosmetic"
metadata_has_tb   string   -- SIEMPRE "false"
created_at        timestamp
completed_at      timestamp (null si no completado)
```

### 5.3 Registros en Stripe Dashboard

Stripe mantiene un audit log completo de:
- Todas las sessiones creadas (con metadata completa)
- Todos los pagos procesados
- Todos los webhooks enviados y su estado
- Todos los reembolsos

Estos registros son accesibles en `dashboard.stripe.com → Developers → Logs` y se pueden exportar en cualquier momento para auditoria.

### 5.4 Registros en Firebase Analytics

Cada evento de pago se loguea en Firebase Analytics:
```
purchase_started  { product_id, provider }
purchase_completed { product_id, provider, amount_usd }
purchase_failed   { product_id, provider, error_code }
stripe_abort_executed { reason, timestamp }
provider_switched { from_provider, to_provider, reason }
```

Estos eventos estan disponibles en Firebase Console → Analytics y pueden exportarse a BigQuery para analisis historico.

### 5.5 Como Usar el Audit Trail en una Disputa

Si Stripe abre una investigacion o un cliente disputa un cargo:

1. En **Stripe Dashboard** → buscar por `session_id` o email del cliente → ver metadata completa (`type: cosmetic`, `has_tournament_benefit: false`).
2. En **Backend DB** → `SELECT * FROM stripe_sessions WHERE user_id = 'xxx'` → confirmar que todos los productos son cosmeticos.
3. En **Firebase Analytics** → exportar todos los eventos `purchase_completed` del usuario → mostrar historial de compras cosmeticas exclusivamente.
4. En **Firebase Realtime DB** → `/entitlements/{userId}` → mostrar los bienes digitales entregados vs pagos recibidos.

Esta evidencia muestra claramente que la cuenta de Stripe procesa exclusivamente bienes cosmeticos digitales.

---

## 6. Proceso de Revision de Compliance Pre-Launch

Antes de activar Stripe en produccion, completar esta lista:

- [ ] Cuenta de Stripe creada con descripcion de negocio correcta (ver Seccion 1)
- [ ] Verificacion de identidad del negocio completada en Stripe Dashboard
- [ ] Todos los productos creados con nombres sin terminos prohibidos
- [ ] Webhooks configurados con los eventos correctos
- [ ] `STRIPE_WEBHOOK_SECRET` configurado en backend
- [ ] Test de compra exitoso con tarjeta `4242 4242 4242 4242`
- [ ] Test de rechazo con tarjeta `4000 0000 0000 9995` (fondos insuficientes)
- [ ] `ProductCatalog.ValidateCatalogCompliance()` ejecutado desde Payment Debug Window → resultado OK
- [ ] `StripeComplianceGuard.ValidateProduct()` testeado con producto con termino prohibido → resultado: rechazo correcto
- [ ] Backend con `TRIUMPH_ISOLATION_MODE=strict` configurado
- [ ] Ningun endpoint del backend de cosmeticos acepta campos de Triumph (verificado por `triumphIsolation.middleware.ts`)
- [ ] Statement Descriptor visible y sin terminos prohibidos
- [ ] Support URL activa y responde
