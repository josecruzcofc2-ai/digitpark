# Digit Park Pro — Arquitectura del Sistema de Pagos

> **Version**: 1.0
> **Fecha**: 2026-03-10
> **Scope**: Sistema de pagos cosméticos (Stripe + Apple IAP) + aislamiento Triumph

---

## 1. Vision General: 3 Sistemas Completamente Aislados

El sistema de pagos de Digit Park Pro esta diseñado alrededor de un principio fundamental:
**cero contaminacion cruzada** entre los tres sistemas de monetizacion.

```
+==============================================================+
|                     DIGIT PARK PRO (US)                       |
|                                                              |
|  +------------------------+  +------------------------+     |
|  | SISTEMA A: Triumph SDK |  | SISTEMA B: Cosmetic    |     |
|  |                        |  | Store (Stripe / IAP)   |     |
|  | Torneos de habilidad   |  |                        |     |
|  | con dinero real        |  | Skins, Temas, Sparks   |     |
|  | Entry fees, depositos  |  | Gem packs, Bundles     |     |
|  | Retiros, KYC           |  |                        |     |
|  |                        |  | PRIMARY:  Stripe       |     |
|  | Usa:                   |  | FAILSAFE: Apple IAP    |     |
|  |  - ServiceLocator      |  |                        |     |
|  |  - TriumphManager      |  | Usa:                   |     |
|  |  - IWalletService      |  |  - PaymentManager      |     |
|  |  - IKYCService         |  |  - IPaymentProvider    |     |
|  |  - IMatchmakingService |  |  - PaymentFeatureFlag  |     |
|  |  - ITournamentService  |  |  - EntitlementService  |     |
|  |                        |  |                        |     |
|  | ZERO contacto Stripe   |  | ZERO contacto Triumph  |     |
|  +------------------------+  +------------------------+     |
|                                                              |
|  +----------------------------------------------------------+|
|  |         BARRERA DE AISLAMIENTO                           ||
|  |  - TriumphIsolationGuard (runtime)                       ||
|  |  - StripeComplianceGuard (runtime)                       ||
|  |  - Assembly boundaries (.asmdef)                         ||
|  |  - #if compile-time guards                               ||
|  +----------------------------------------------------------+|
|                                                              |
+==============================================================+

+==============================================================+
|              SISTEMA C: Version Separation                    |
|                                                              |
|  Pro  (US / mercados permitidos):                            |
|    Define: DIGIT_PARK_PRO                                    |
|    Accede: Triumph + Stripe + Apple IAP                      |
|    Bundle: com.matrixsoftware.digitpark.pro                  |
|                                                              |
|  Global (Rest of World):                                     |
|    Define: DIGIT_PARK_GLOBAL                                 |
|    Accede: Apple IAP solamente                               |
|    Bundle: com.matrixsoftware.digitpark                      |
|                                                              |
|  Control: Player Settings > Scripting Define Symbols         |
+==============================================================+
```

---

## 2. Diagrama de Componentes del Sistema B (Cosmetic Store)

```
  Unity Client (iOS)
  +------------------------------------------------------+
  |                                                      |
  |  ShopManager / PremiumManager (UI)                   |
  |         |                                            |
  |         v                                            |
  |  +-------------+     Firebase Remote Config         |
  |  | PaymentMgr  |<----+  (polling cada 15 min)       |
  |  +-------------+     |                              |
  |    |        |        | PaymentFeatureFlag            |
  |    |   +----+----+   |  .ActiveCosmeticProvider      |
  |    |   |         |   |  .IsStripeEnabled             |
  |    v   v         v   |  .IsTriumphEnabled (isolado)  |
  |  Stripe  Apple   |   |                              |
  |  Prov.   IAP     +---+                              |
  |    |     Prov.                                      |
  |    |       |                                        |
  |    v       v                                        |
  | EntitlementService                                  |
  |  - Grant(productId, provider, txId)                 |
  |  - IsEntitled(productId)                            |
  |  - Sincroniza con Firebase Realtime DB              |
  |                                                     |
  +------------------------------------------------------+
         |
         | HTTPS
         v
  Firebase Cloud Functions (Pro solamente)
  +------------------------------------------------------+
  | GET  .../paymentsHealth                              |
  | POST .../stripeCreateCheckout                        |
  | GET  .../stripeSessionStatus?sessionId=xxx           |
  | POST .../stripeWebhook                               |
  | POST .../iapValidateReceipt                          |
  | POST .../adminForceSwitch                            |
  | GET  .../getEntitlements                             |
  | POST .../syncEntitlements                            |
  +------------------------------------------------------+
         |
         v
  Stripe Dashboard    Apple Servers
  (checkout.stripe.com)  (buy.itunes.apple.com)
```

---

## 3. Flujo de Compra via Stripe (Happy Path)

```
Usuario          ShopManager      PaymentManager    Backend           Stripe
   |                 |                  |               |                |
   |--[toca Buy]---->|                  |               |                |
   |                 |--PurchaseCosmetic(productId)---->|                |
   |                 |                  |               |                |
   |                 |          [ValidateProduct()]      |                |
   |                 |          StripeComplianceGuard    |                |
   |                 |          VersionGuard             |                |
   |                 |                  |               |                |
   |                 |          [POST stripeCreate      |                |
   |                 |           CheckoutUrl]---------->|                |
   |                 |                  |               |--CreateSession->|
   |                 |                  |               |<--{sessionId,  |
   |                 |                  |               |    checkoutUrl}|
   |                 |                  |<--{sessionId, |                |
   |                 |                  |    checkoutUrl}               |
   |                 |                  |               |                |
   |                 |    [OpenCheckout(checkoutUrl)]    |                |
   |<--[SFSafariVC opens]----------------|               |                |
   |                                     |               |                |
   |--[usuario paga en Stripe]--------------------------------->|         |
   |                                     |               |<--webhook POST |
   |<--[deep link digitpark://stripe-return?session_id=xxx]               |
   |                                     |               |                |
   |                 |          [PollUntilComplete()]    |                |
   |                 |          StripeSessionPoller      |                |
   |                 |          GET stripeSession        |                |
   |                 |          StatusUrl?sessionId=xx-->|                |
   |                 |                  |<--{status:"complete"}           |
   |                 |                  |               |                |
   |                 |          [OnPurchaseSuccess()]    |                |
   |                 |          EntitlementService.Grant()|              |
   |                 |          CurrencyManager.ProcessGemsPurchase()    |
   |                 |          AnalyticsService.LogPurchaseCompleted()  |
   |<--[UI muestra exito]-------------|                  |                |
```

**Detalles del flujo:**
1. `StripeComplianceGuard.ValidateProduct()` verifica que el `productId` y metadata no contengan términos prohibidos antes de cualquier request.
2. `VersionGuard.CanAccessStripe()` verifica que el build sea `DIGIT_PARK_PRO`.
3. El backend crea una Stripe Checkout Session con `metadata_type: "cosmetic"` y `metadata_has_tournament_benefit: "false"`.
4. `StripeCheckoutController` abre la URL de checkout en `SFSafariViewController` en iOS (sin salir de la app).
5. `StripeSessionPoller` hace polling cada 2 segundos hasta `status == "complete"` o timeout a los 5 minutos.
6. El return desde Stripe llega via deep link `digitpark://stripe-return?session_id=xxx`.

---

## 4. Flujo de Compra via Stripe (Fallback a Apple IAP)

```
PaymentManager
     |
     |--[Stripe falla (error de red / rechazo)]
     |
     |  _stripeFailureCount++
     |
     |--[_stripeFailureCount >= 3?]
     |        |
     |       YES: PaymentFeatureFlag.ForceSwitch(AppleIAP, "stripe_3_failures")
     |        |   LocalFlagCache.SaveProviderOverride("apple_iap")
     |        |
     |       NO: seguir con fallback inmediato para esta compra
     |
     |--[_iapProvider.IsAvailable?]
     |        |
     |       YES: result = await _iapProvider.PurchaseProduct(product, userId)
     |        |   result.WasProviderSwitched = true
     |        |
     |       NO:  PaymentResult.Failed("no_provider_available")
     |
     v
  PaymentEvents.EmitPurchaseCompleted(result)  [si exito]
  o
  PaymentEvents.EmitPurchaseFailed(result)     [si fracaso]
```

**Nota**: El fallback es transparente para el usuario. La UI solo muestra exito o fracaso; no expone qué provider se uso.

---

## 5. Flujo de Compra via Apple IAP (Provider Activo)

```
Usuario          PaymentManager    AppleIAPProvider    Apple Servers
   |                  |                  |                   |
   |--PurchaseCosmetic|                  |                   |
   |                  |--PurchaseProduct(product, userId)--->|
   |                  |                  |                   |
   |                  |       [Unity IAP IStoreController    |
   |                  |        .InitiatePurchase()]          |
   |                  |                  |--[buyProduct]---->|
   |                  |                  |                   |
   |<--[iOS purchase dialog shown]-----  |                   |
   |--[usuario aprueba]---------------------------------------->|
   |                  |                  |<--[receipt]-------|
   |                  |                  |                   |
   |                  |       [POST iapValidateReceiptUrl]
   |                  |                  |--[to Firebase Fn]->|
   |                  |                  |   Cloud Fn verifica contra Apple
   |                  |                  |<--{valid: true}   |
   |                  |                  |                   |
   |                  |<--PaymentResult.Successful()         |
   |                  |                  |                   |
   |                  |--EntitlementService.Grant()          |
   |<--[UI muestra exito]                |                   |
```

**Diferencias vs Stripe:**
- Apple IAP usa el sistema nativo de iOS (no abre browser).
- La validacion del receipt se hace server-side via la Cloud Function `iapValidateReceipt`.
- Los Non-Consumable (`premium_bundle`, `complete_bundle`) son restaurables via `RestorePurchases()`.
- Apple IAP funciona en AMBAS versiones (Pro y Global).

---

## 6. Flujo del Abort Protocol

El abort protocol es el mecanismo de emergencia para desactivar Stripe instantaneamente.

```
TRIGGER (cualquiera de estos):
  A. Automatico: _stripeFailureCount >= MAX_STRIPE_FAILURES (3)
  B. Remote Config: payment_provider = "apple_iap" en Firebase Console
  C. Manual: StripeAbortProtocol.ExecuteAbort(AbortReason.ManualDeveloperTrigger)
  D. Gesture: 5-finger tap en la tienda (solo builds debug/staging)

     |
     v
StripeAbortProtocol.ExecuteAbort(reason)
     |
     |-- PASO 1 (INSTANTANEO, sin red):
     |   PaymentFeatureFlag.ForceSwitch(AppleIAP, reason)
     |   LocalFlagCache.SaveProviderOverride("apple_iap")
     |   --> _stripeEnabled = false (INMEDIATO)
     |
     |-- PASO 2 (async, no bloquea):
     |   POST adminForceSwitchUrl (Firebase Cloud Function)
     |   body: { provider: "apple_iap", reason: ..., timestamp: ... }
     |   timeout: 5 segundos (fire-and-forget)
     |
     |-- PASO 3:
     |   AnalyticsService.LogEvent("stripe_abort_executed", {reason, timestamp})
     |
     |-- PASO 4 (500ms delay):
     |   _iapProvider.HealthCheck()
     |   SI Apple IAP tambien falla:
     |     ForceSwitch(PaymentProvider.None, "both_providers_down")
     |     --> tienda completamente deshabilitada
     |
     |-- PASO 5:
     |   PaymentEvents.EmitAbortExecuted(reason)
     |   --> UIs activas reciben el evento y se actualizan
     |
     v
  Sistema operando con Apple IAP exclusivamente.
  Stripe NUNCA sera reactivado hasta reset manual + Firebase RC update.
```

**Recuperacion post-abort:**
1. Ir a Firebase Console → Remote Config → cambiar `payment_provider` a `"stripe"`, `stripe_enabled` a `true`.
2. En app: `RemoteConfigService.Instance.ForceRemoteSwitch("stripe")` o esperar el polling de 15 min.
3. En codigo: `StripeAbortProtocol.Reset()` (solo en editor/debug builds).

---

## 7. Garantias de Aislamiento Triumph ↔ Stripe

### 7.1 Separacion en Assembly Definitions

```
DigitPark.Payments.Runtime.asmdef
  - SOLO referencias a: Unity.Mathematics, com.unity.purchasing (IAP)
  - NUNCA referencia: DigitPark.Services.Triumph
  - NUNCA referencia: DigitPark.Services (el ServiceLocator de CashBattle)

DigitPark.Services.Triumph.asmdef (futuro)
  - NO referencia: DigitPark.Payments.Runtime
```

Las assembly definitions crean fronteras de compilacion: es literalmente imposible que `StripePaymentProvider` importe o use `TriumphManager` sin romper la compilacion.

### 7.2 Reglas de Namespace

```
// PERMITIDO en DigitPark.Payments:
using DigitPark.Payments;
using DigitPark.Payments.Stripe;
using DigitPark.Payments.AppleIAP;
using DigitPark.Payments.Compliance;

// PROHIBIDO en DigitPark.Payments (cualquier archivo):
using DigitPark.Services.Triumph;    // <- NUNCA
using DigitPark.Services.Interfaces; // <- NUNCA (IWalletService, etc.)
// ServiceLocator                    // <- NUNCA
```

### 7.3 Regla de Metadata de Productos

Todo producto que pase por Stripe DEBE tener en su metadata:
```
type = "cosmetic"
has_tournament_benefit = "false"
```

`StripeComplianceGuard.ValidateSessionMetadata()` verifica esto antes de cada request. Si falta o tiene valor incorrecto, el request es rechazado localmente (no llega al backend ni a Stripe).

### 7.4 Terminos Prohibidos en StripeComplianceGuard

Los siguientes strings causan rechazo inmediato si aparecen en `productId`, `displayName`, o cualquier valor de `Metadata`:

```
"tournament", "prize", "cash_game", "skill_game",
"real_money", "entry_fee", "wager", "bet", "gambling", "triumph"
```

El rechazo queda registrado en `PlayerPrefs["dp_compliance_audit"]` con timestamp, productId y termino encontrado.

### 7.5 Separacion en Firebase Cloud Functions

Las Firebase Cloud Functions del sistema cosmetico NUNCA tendran credenciales de Triumph. El middleware `triumphIsolation.ts` en `functions/src/middleware/` rechaza cualquier request que contenga headers o body fields relacionados con Triumph — implementado en cada Cloud Function del sistema de pagos.

---

## 8. Separacion Pro vs Global via Scripting Defines

```
                    DIGIT_PARK_PRO          DIGIT_PARK_GLOBAL
                    (US / mercados           (App Store global)
                     permitidos)

  Triumph           HABILITADO               DESHABILITADO
  Stripe            HABILITADO               DESHABILITADO
  Apple IAP         HABILITADO               HABILITADO
  CashBattle UI     VISIBLE                  OCULTA
  Firebase Fns      Todos los endpoints      Solo iapValidateReceipt
  Bundle ID         ...digitpark.pro         ...digitpark

  VersionGuard:
    CanAccessStripe()   -> true              -> false (hardcoded)
    CanAccessTriumph()  -> true              -> false (hardcoded)
    CanAccessAppleIAP() -> true              -> true
```

**Como funciona la separacion:**

`PaymentFeatureFlag.Initialize()` detecta el define al compilar:
```csharp
#if DIGIT_PARK_PRO
    _currentVersion = AppVersion.Pro;
#elif DIGIT_PARK_GLOBAL
    _currentVersion = AppVersion.Global;
#endif

// Al final del Initialize(), si es Global:
if (_currentVersion == AppVersion.Global)
{
    _stripeEnabled = false;      // ignorar Remote Config
    _triumphEnabled = false;     // ignorar Remote Config
    if (_activeCosmeticProvider == PaymentProvider.Stripe)
        _activeCosmeticProvider = PaymentProvider.AppleIAP;
}
```

Esto significa que aunque Firebase Remote Config enviara `stripe_enabled = true` a un build Global, el codigo lo ignora y fuerza AppleIAP. La barrera es en tiempo de compilacion Y en tiempo de ejecucion.

---

## 9. Flujo de Inicializacion en Boot

```
BootManager.cs
  |
  |-- Paso 1: ATTService (iOS tracking permission)
  |-- Paso 2: NetworkService (connectivity)
  |-- Paso 3: Firebase Auth + Analytics
  |
  |-- Paso 3.5 (NUEVO): Payment System Init
  |     RemoteConfigService.Start()
  |       -> fetch Firebase Remote Config (async, no bloquea boot)
  |       -> si offline: LocalFlagCache.Load() o GetDefaults()
  |     PaymentFeatureFlag.Initialize(configData)
  |       -> detectar Pro vs Global
  |       -> configurar provider activo
  |     PaymentManager.Start()
  |       -> inicializar AppleIAP (siempre)
  |       -> inicializar Stripe (solo si DIGIT_PARK_PRO + stripeEnabled)
  |
  |-- Paso 4: Escenas y UI
```

**Nota critica**: El boot NO espera la inicializacion de pagos para mostrar la UI. Si el usuario entra a la tienda antes de que `PaymentManager.IsInitialized == true`, la UI muestra un spinner y espera. Esto es intencional — el boot no se puede bloquear por pagos.

---

## 10. Modelo de Datos de Entitlements

```
EntitlementRecord
  productId:   string   ("sparks_500", "premium_bundle", ...)
  provider:    string   ("stripe", "apple_iap")
  transactionId: string (Stripe sessionId o Apple transactionId)
  purchaseDate: DateTime
  isRevoked:   bool     (para reembolsos)

Persistencia (en orden de confianza):
  1. Firebase Realtime DB: /entitlements/{userId}/{productId}
  2. PlayerPrefs (cache local para offline)
  3. Apple IAP receipt (para Non-Consumables, restaurable)
```

---

## 11. Referencia de Archivos Clave

| Archivo | Namespace | Responsabilidad |
|---|---|---|
| `Runtime/Payments/Core/PaymentManager.cs` | `DigitPark.Payments` | Orquestador. Decide provider. Maneja fallback. |
| `Runtime/Payments/Core/IPaymentProvider.cs` | `DigitPark.Payments` | Contrato que implementan Stripe y Apple IAP. |
| `Runtime/Payments/Core/ProductCatalog.cs` | `DigitPark.Payments` | 8 productos cosméticos. Validacion de compliance. |
| `Runtime/Payments/Core/PaymentResult.cs` | `DigitPark.Payments` | DTO de resultado (exito/fracaso + provider usado). |
| `Runtime/Payments/Core/PaymentEvents.cs` | `DigitPark.Payments` | Eventos estaticos (PurchaseStarted, Completed, etc.). |
| `Runtime/Payments/FeatureFlags/PaymentFeatureFlag.cs` | `DigitPark.Payments` | Estado central de flags. Pro vs Global. |
| `Runtime/Payments/FeatureFlags/RemoteConfigService.cs` | `DigitPark.Payments` | Fetch Firebase Remote Config. Polling 15 min. |
| `Runtime/Payments/Stripe/StripePaymentProvider.cs` | `DigitPark.Payments.Stripe` | Implementa IPaymentProvider para Stripe Checkout. |
| `Runtime/Payments/Stripe/StripeCheckoutController.cs` | `DigitPark.Payments.Stripe` | Abre SFSafariViewController. Maneja deep link return. |
| `Runtime/Payments/Stripe/StripeSessionPoller.cs` | `DigitPark.Payments.Stripe` | Polling de estado de sesion al backend cada 2s. |
| `Runtime/Payments/Stripe/StripeComplianceGuard.cs` | `DigitPark.Payments.Compliance` | Valida que productos/metadata no violan ToS de Stripe. |
| `Runtime/Payments/Abort/StripeAbortProtocol.cs` | `DigitPark.Payments` | Desactiva Stripe instantaneamente. Switch a Apple IAP. |
| `Runtime/Payments/Compliance/VersionGuard.cs` | `DigitPark.Payments.Compliance` | Verifica Pro vs Global en runtime. |
| `Runtime/Payments/Entitlements/EntitlementService.cs` | `DigitPark.Payments.Entitlements` | Fuente de verdad de lo que posee el usuario. |
| `functions/src/services/stripe.service.ts` | TypeScript | Crea sessions, verifica webhooks, query estado. |
| `functions/src/middleware/triumphIsolation.ts` | TypeScript | Rechaza requests con campos de Triumph en Cloud Functions. |
| `Editor/Payments/PaymentDebugWindow.cs` | `DigitPark.Editor.Payments` | Ventana de debug (menu: DigitPark > Payment Debug Window). |
