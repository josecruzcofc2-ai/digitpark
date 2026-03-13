# DIGIT PARK PRO — Plan de Implementacion Completo del Sistema de Pagos

> **Documento**: Plan exhaustivo para implementacion por Sonnet
> **Fecha**: 2026-03-10
> **Version**: 1.0
> **Autor**: Opus 4.6 (analisis arquitectonico)
> **Ejecutor**: Sonnet (implementacion)

---

## TABLA DE CONTENIDOS

1. [Contexto del Proyecto Existente](#1-contexto-del-proyecto-existente)
2. [Arquitectura de 3 Sistemas Aislados](#2-arquitectura-de-3-sistemas-aislados)
3. [Estructura de Archivos a Crear](#3-estructura-de-archivos-a-crear)
4. [FASE 1: Assembly Definitions (Fronteras de Compilacion)](#fase-1)
5. [FASE 2: Core Payment Layer](#fase-2)
6. [FASE 3: Feature Flag System](#fase-3)
7. [FASE 4: Stripe Integration (Primary Cosmetic Provider)](#fase-4)
8. [FASE 5: Apple IAP Failsafe Provider](#fase-5)
9. [FASE 6: Triumph Isolation Layer](#fase-6)
10. [FASE 7: Version Separation (Pro vs Global)](#fase-7)
11. [FASE 8: Entitlement Service](#fase-8)
12. [FASE 9: Abort Protocol](#fase-9)
13. [FASE 10: Backend (Node.js + TypeScript)](#fase-10)
14. [FASE 11: UI Integration](#fase-11)
15. [FASE 12: BootManager Integration](#fase-12)
16. [FASE 13: Tests](#fase-13)
17. [FASE 14: Monitoring & Alerts](#fase-14)
18. [FASE 15: Documentation Files](#fase-15)
19. [Pasos Manuales Post-Implementacion](#pasos-manuales)
20. [Checklist de Validacion Final](#checklist)

---

## 1. CONTEXTO DEL PROYECTO EXISTENTE

### 1.1 Lo que YA existe y NO se debe romper

| Componente | Archivo | Estado |
|---|---|---|
| **PremiumManager** | `Runtime/Features/Monetization/Premium/PremiumManager.cs` | COMPLETO. Tiene Unity IAP con IDetailedStoreListener, 6 non-consumable + 6 gem packs. **Se debe refactorizar para enrutar a traves del nuevo PaymentManager.** |
| **ShopManager** | `Runtime/Features/Monetization/Shop/ShopManager.cs` | COMPLETO. Maneja UI de tienda, compras con gems/coins/IAP. **Se debe conectar al nuevo PaymentManager para compras RealMoney.** |
| **ShopItemData** | `Runtime/Features/Monetization/Shop/ShopItemData.cs` | COMPLETO. ScriptableObject con PriceType.RealMoney que llama a PremiumManager. **Se debe modificar TryPurchase() para usar PaymentManager.** |
| **ServiceLocator** | `Runtime/Services/ServiceLocator.cs` | COMPLETO. Registra IKYCService, IWalletService, IMatchmakingService, ITournamentService para CashBattle. **NO tocar — es territorio exclusivo de Triumph.** |
| **TriumphManager** | `Runtime/Services/Triumph/TriumphManager.cs` | STUB. Singleton con mock mode. **Se debe aislar completamente del sistema de pagos cosmeticos.** |
| **TriumphServices** | `Runtime/Services/Triumph/TriumphServices.cs` | STUB. TriumphKYCService, TriumphWalletService, etc. con `throw NotImplementedException()`. **No tocar hasta que Triumph SDK real llegue.** |
| **BootManager** | `Runtime/Core/Boot/BootManager.cs` | COMPLETO. Secuencia de boot en 5 pasos. **Se debe agregar Paso 3.5 para inicializar PaymentManager + FeatureFlags.** |
| **AnalyticsService** | `Runtime/Services/Firebase/AnalyticsService.cs` | COMPLETO. Tiene LogPurchaseStarted/Completed/Failed. **Se debe usar para eventos de pago de Stripe/IAP.** |
| **DatabaseService** | `Runtime/Services/Firebase/DatabaseService.cs` | COMPLETO. Firebase Realtime DB. **Se puede usar para entitlements sync.** |
| **CurrencyManager** | `Runtime/Features/Monetization/Currency/CurrencyManager.cs` | COMPLETO. Maneja DigitGems + DigitCoins. **PaymentManager debe llamar a CurrencyManager.ProcessGemsPurchase() tras compra exitosa.** |
| **Interfaces IWalletService etc.** | `Runtime/Services/Interfaces/` | COMPLETO. Son exclusivamente para CashBattle/Triumph. **NUNCA mezclar con Stripe.** |

### 1.2 Namespaces existentes (seguir convencion)

```
DigitPark                         — Root
DigitPark.Services                — Servicios singleton
DigitPark.Services.Firebase       — Firebase services
DigitPark.Services.Mock           — Mock implementations
DigitPark.Services.Triumph        — Triumph SDK stubs
DigitPark.Services.Interfaces     — Service interfaces (CashBattle)
DigitPark.Managers                — Feature managers
DigitPark.Monetization            — ShopItemData, enums, shop system
DigitPark.Localization            — AutoLocalizer
DigitPark.Themes                  — Theme system
DigitPark.Navigation              — Scene navigation
DigitPark.UI                      — UI components
```

### 1.3 Namespaces NUEVOS a crear

```
DigitPark.Payments                — Core payment interfaces + manager
DigitPark.Payments.Stripe         — Stripe provider
DigitPark.Payments.AppleIAP       — Apple IAP failsafe provider
DigitPark.Payments.FeatureFlags   — Remote config + feature flags
DigitPark.Payments.Entitlements   — Entitlement tracking
DigitPark.Payments.Compliance     — Isolation guards + compliance
DigitPark.Payments.UI             — Payment-specific UI components
```

### 1.4 Patron singleton usado en el proyecto

```csharp
// TODOS los singletons siguen este patron exacto:
public class ServiceName : MonoBehaviour
{
    private static ServiceName _instance;
    public static ServiceName Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<ServiceName>();
                if (_instance == null)
                {
                    // Algunos crean el GO, otros solo loguean warning
                }
            }
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
        // Initialize...
    }
}
```

### 1.5 Bundle IDs existentes

```
Actual:     com.matrixsoftware.digitpark
Nuevo Pro:  com.matrixsoftware.digitpark.pro
Nuevo Global: com.matrixsoftware.digitpark (mantener actual)
```

### 1.6 Productos IAP existentes (ya en PremiumManager)

```
Non-Consumable:
  com.matrixsoftware.digitpark.createtournaments      ($3.99)
  com.matrixsoftware.digitpark.cashbattlecreate        ($6.99)
  com.matrixsoftware.digitpark.tournamentbundle         ($8.99)
  com.matrixsoftware.digitpark.stylespro                (Legacy)
  com.matrixsoftware.digitpark.premium_bundle           ($26.25)
  com.matrixsoftware.digitpark.complete_bundle          ($30.45)

Consumable (Gem Packs):
  com.matrixsoftware.digitpark.gems_100 ... gems_14000
```

---

## 2. ARQUITECTURA DE 3 SISTEMAS AISLADOS

```
+================================================================+
|                    DIGIT PARK PRO (US)                          |
|                                                                |
|  +---------------------------+  +---------------------------+  |
|  | SISTEMA A: Triumph SDK    |  | SISTEMA B: Cosmetic Store |  |
|  |                           |  |                           |  |
|  | - Torneos con dinero real |  | - Skins, temas, Sparks   |  |
|  | - Entry fees, deposits    |  | - Season pass (cosmetico)|  |
|  | - Retiros, KYC            |  | - Gem packs              |  |
|  | - Compliance legal propia |  |                           |  |
|  |                           |  | PRIMARY: Stripe           |  |
|  | Usa: ServiceLocator       |  | FAILSAFE: Apple IAP      |  |
|  |       TriumphManager      |  |                           |  |
|  |       IWalletService      |  | Usa: PaymentManager      |  |
|  |       IKYCService         |  |       IPaymentProvider   |  |
|  |       IMatchmakingService |  |       PaymentFeatureFlag |  |
|  |       ITournamentService  |  |       EntitlementService |  |
|  |                           |  |                           |  |
|  | ZERO contacto con Stripe  |  | ZERO contacto con        |  |
|  |                           |  | Triumph                  |  |
|  +---------------------------+  +---------------------------+  |
|                                                                |
|  +---------------------------------------------------------+   |
|  | BARRERA DE AISLAMIENTO (TriumphIsolationGuard)          |   |
|  | - Verifica zero cross-contamination en runtime          |   |
|  | - Assembly boundaries via .asmdef                       |   |
|  | - Compile-time guards via #if directives                |   |
|  +---------------------------------------------------------+   |
|                                                                |
+================================================================+

+================================================================+
| SISTEMA C: Version Separation                                  |
|                                                                |
| Pro  (US):     DIGIT_PARK_PRO   → Triumph + Stripe + IAP      |
| Global (ROW):  DIGIT_PARK_GLOBAL → Apple IAP only              |
|                                                                |
| Controlado por: Scripting Define Symbols en Player Settings    |
+================================================================+
```

---

## 3. ESTRUCTURA DE ARCHIVOS A CREAR

### 3.1 Archivos Unity (C#) — Ubicacion: `Assets/_Project/Scripts/`

```
Runtime/
  Payments/
    Core/
      IPaymentProvider.cs           ← Interfaz base para providers
      PaymentManager.cs             ← Orquestador central de pagos cosmeticos
      PaymentResult.cs              ← Modelo de resultado de pago
      PaymentConfig.cs              ← Configuracion de providers
      PaymentEvents.cs              ← Eventos estaticos del sistema de pagos
      ProductCatalog.cs             ← Catalogo de productos cosmeticos (Pro vs Global)
    Stripe/
      StripePaymentProvider.cs      ← Implementa IPaymentProvider via Stripe Checkout
      StripeCheckoutController.cs   ← Controla Safari View Controller para checkout
      StripeSessionPoller.cs        ← Polling de session status al backend
      StripeComplianceGuard.cs      ← Validador de cumplimiento Stripe ToS
    AppleIAP/
      AppleIAPProvider.cs           ← Implementa IPaymentProvider via Unity IAP existente
      AppleReceiptValidator.cs      ← Validacion de receipts contra backend
    FeatureFlags/
      PaymentFeatureFlag.cs         ← Estado central: que provider esta activo
      RemoteConfigService.cs        ← Firebase Remote Config fetch + cache
      LocalFlagCache.cs             ← PlayerPrefs cache de flags
    Entitlements/
      EntitlementService.cs         ← Fuente de verdad de lo que el usuario posee
      EntitlementRecord.cs          ← Modelo de datos de entitlement
    Compliance/
      TriumphIsolationGuard.cs      ← Monitor runtime de aislamiento
      VersionGuard.cs               ← Verifica que la version correcta acceda a endpoints correctos
    Abort/
      StripeAbortProtocol.cs        ← Protocolo de emergencia si Stripe cae
      AbortReason.cs                ← Enum de razones de abort
    UI/
      PaymentLoadingOverlay.cs      ← Overlay de loading durante pagos
      PaymentErrorDialog.cs         ← Dialogo de error de pago con retry/fallback

  Plugins/iOS/
    StoreKitBridge.mm               ← Objective-C bridge para StoreKit 2

Editor/
  Payments/
    PaymentDebugWindow.cs           ← Editor window para testing de pagos
    BuildProfileSwitcher.cs         ← Tool para cambiar entre Pro/Global

Tests/
  Payments/
    PaymentManagerTests.cs
    FeatureFlagTests.cs
    TriumphIsolationTests.cs
    StripeAppleIAPSwitchTests.cs
    EntitlementServiceTests.cs
    ComplianceGuardTests.cs
```

### 3.2 Assembly Definitions (.asmdef)

```
Runtime/Payments/DigitPark.Payments.asmdef
Runtime/Payments/Stripe/DigitPark.Payments.Stripe.asmdef
Runtime/Payments/AppleIAP/DigitPark.Payments.AppleIAP.asmdef
Runtime/Services/Triumph/DigitPark.Services.Triumph.asmdef
Tests/Payments/DigitPark.Payments.Tests.asmdef
```

### 3.3 Backend (Node.js + TypeScript) — Ubicacion: `Backend/`

```
Backend/
  package.json
  tsconfig.json
  .env.example
  .env.production.example
  src/
    index.ts                        ← Express server entry point
    config/
      environment.ts                ← Env vars + validation
      stripe.config.ts              ← Stripe SDK config
    routes/
      stripe.routes.ts              ← POST /checkout, webhook, session-status
      iap.routes.ts                 ← POST /validate-receipt
      entitlements.routes.ts        ← GET/POST entitlements
      health.routes.ts              ← GET /health/payments
    services/
      stripe.service.ts             ← Stripe Checkout Session creation + webhook handling
      appleIAP.service.ts           ← Receipt validation against Apple servers
      entitlement.service.ts        ← Grant/query entitlements in DB
      featureFlag.service.ts        ← Read/write Firebase Remote Config
      alert.service.ts              ← Email + Slack webhook alerts
    middleware/
      triumphIsolation.middleware.ts ← Blocks any Triumph data from reaching Stripe
      versionGuard.middleware.ts     ← Validates X-App-Version header
      rateLimiter.middleware.ts      ← Rate limiting
    types/
      payment.types.ts              ← TypeScript interfaces
      entitlement.types.ts
    database/
      schema.sql                    ← PostgreSQL schema (entitlements, transactions, audit)
      migrations/
        001_initial.sql
```

### 3.4 Documentation — Ubicacion: `docs/`

```
docs/
  PAYMENT_ARCHITECTURE.md           ← Diagrama + flujos de datos
  STRIPE_COMPLIANCE.md              ← Como presentarse a Stripe
  ABORT_RUNBOOK.md                  ← Pasos manuales si Stripe cae
  DEVELOPER_ONBOARDING.md           ← Setup local + sandbox testing
```

---

## FASE 1: Assembly Definitions (Fronteras de Compilacion) <a id="fase-1"></a>

### Objetivo
Crear .asmdef files que IMPIDAN que codigo de Stripe importe namespaces de Triumph y viceversa. Esto es la barrera de compilacion mas fuerte posible en Unity.

### Archivo 1: `Runtime/Payments/DigitPark.Payments.asmdef`

```json
{
    "name": "DigitPark.Payments",
    "rootNamespace": "DigitPark.Payments",
    "references": [
        "Unity.TextMeshPro",
        "DigitPark.Payments.Stripe",
        "DigitPark.Payments.AppleIAP"
    ],
    "includePlatforms": [],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": false,
    "precompiledReferences": [],
    "autoReferenced": true,
    "defineConstraints": [],
    "versionDefines": [],
    "noEngineReferences": false
}
```

**IMPORTANTE**: Este assembly NO referencia `DigitPark.Services.Triumph`. Esto significa que ningun archivo en `Payments/` puede hacer `using DigitPark.Services.Triumph` — el compilador lo rechaza.

### Archivo 2: `Runtime/Payments/Stripe/DigitPark.Payments.Stripe.asmdef`

```json
{
    "name": "DigitPark.Payments.Stripe",
    "rootNamespace": "DigitPark.Payments.Stripe",
    "references": [],
    "includePlatforms": [],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": false,
    "precompiledReferences": [],
    "autoReferenced": false,
    "defineConstraints": ["HAS_STRIPE"],
    "versionDefines": [],
    "noEngineReferences": false
}
```

**CRITICO**: `defineConstraints: ["HAS_STRIPE"]` significa que este assembly SOLO compila si `HAS_STRIPE` esta definido en Player Settings. En la version Global, este codigo ni siquiera existe en el build.

### Archivo 3: `Runtime/Payments/AppleIAP/DigitPark.Payments.AppleIAP.asmdef`

```json
{
    "name": "DigitPark.Payments.AppleIAP",
    "rootNamespace": "DigitPark.Payments.AppleIAP",
    "references": [
        "UnityEngine.Purchasing",
        "UnityEngine.Purchasing.Stores"
    ],
    "includePlatforms": [],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": false,
    "precompiledReferences": [],
    "autoReferenced": false,
    "defineConstraints": ["HAS_APPLE_IAP"],
    "versionDefines": [],
    "noEngineReferences": false
}
```

### Archivo 4: `Runtime/Services/Triumph/DigitPark.Services.Triumph.asmdef`

```json
{
    "name": "DigitPark.Services.Triumph",
    "rootNamespace": "DigitPark.Services.Triumph",
    "references": [],
    "includePlatforms": [],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": false,
    "precompiledReferences": [],
    "autoReferenced": false,
    "defineConstraints": ["HAS_TRIUMPH"],
    "versionDefines": [],
    "noEngineReferences": false
}
```

**RESULTADO**: Stripe y Triumph viven en assemblies separados que NO se referencian mutuamente. Si alguien intenta `using DigitPark.Services.Triumph` desde un archivo en `Payments/Stripe/`, Unity lanza error de compilacion.

### PROBLEMA A RESOLVER

El proyecto actual NO tiene .asmdef en ninguna parte de `_Project/Scripts/`. Agregar .asmdef al subdirectorio `Triumph/` hara que esos archivos dejen de compilar con el resto del proyecto (que esta en Assembly-CSharp.dll). Esto significa:

1. **`TriumphServices.cs`** usa `IKYCService`, `IWalletService`, etc. que estan en `Runtime/Services/Interfaces/`. Si creamos un .asmdef para Triumph, necesitamos o bien:
   - (A) Crear .asmdef tambien para Interfaces/ y referenciarla desde Triumph
   - (B) Mover las interfaces de CashBattle dentro del .asmdef de Triumph

2. **`ServiceLocator.cs`** importa `DigitPark.Services.Mock` y crea instancias de TriumphKYCService, etc. Si Triumph tiene su propio assembly, ServiceLocator necesita referenciarlo.

### DECISION ARQUITECTONICA: Approach Pragmatico

En lugar de reestructurar todo el proyecto con .asmdef (demasiado riesgo de romper cosas), usar un **approach hibrido**:

- **SI crear .asmdef** para el NUEVO codigo de Payments/ (Stripe, AppleIAP, FeatureFlags)
- **NO crear .asmdef** para el codigo existente de Triumph (ya vive en Assembly-CSharp)
- **Usar `#if` directives** como barrera secundaria dentro de archivos existentes
- El nuevo `DigitPark.Payments.asmdef` NO referencia Assembly-CSharp.dll para acceder a Triumph, pero SI puede acceder a traves de interfaces limpias

### Implementacion revisada de .asmdef

Solo crear estos:

**1. `Runtime/Payments/DigitPark.Payments.Runtime.asmdef`**
```json
{
    "name": "DigitPark.Payments.Runtime",
    "rootNamespace": "DigitPark.Payments",
    "references": [
        "Unity.TextMeshPro"
    ],
    "includePlatforms": [],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": true,
    "precompiledReferences": [
        "UnityEngine.Purchasing.dll"
    ],
    "autoReferenced": true,
    "defineConstraints": [],
    "versionDefines": [],
    "noEngineReferences": false
}
```

**2. `Tests/Payments/DigitPark.Payments.Tests.asmdef`**
```json
{
    "name": "DigitPark.Payments.Tests",
    "rootNamespace": "DigitPark.Payments.Tests",
    "references": [
        "DigitPark.Payments.Runtime",
        "UnityEngine.TestRunner",
        "UnityEditor.TestRunner"
    ],
    "includePlatforms": ["Editor"],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": true,
    "precompiledReferences": [
        "nunit.framework.dll"
    ],
    "autoReferenced": false,
    "defineConstraints": ["UNITY_INCLUDE_TESTS"],
    "versionDefines": [],
    "noEngineReferences": false
}
```

### Barrera via `#if` directives (archivos existentes)

En cada archivo de Triumph que ya existe, agregar al inicio:

```csharp
// En TriumphManager.cs, TriumphServices.cs:
#if UNITY_EDITOR || HAS_TRIUMPH
// ... todo el contenido del archivo ...
#endif
```

En cada archivo nuevo de Stripe:

```csharp
// En StripePaymentProvider.cs, StripeCheckoutController.cs, etc:
#if UNITY_EDITOR || HAS_STRIPE
// ... todo el contenido del archivo ...
#endif
```

---

## FASE 2: Core Payment Layer <a id="fase-2"></a>

### Archivo: `Runtime/Payments/Core/PaymentEvents.cs`

```csharp
namespace DigitPark.Payments
{
    /// <summary>
    /// Eventos estaticos del sistema de pagos.
    /// Cualquier parte del juego puede suscribirse sin dependencia directa.
    /// </summary>
    public static class PaymentEvents
    {
        // Cuando un pago inicia (cualquier provider)
        public static event System.Action<string /*productId*/, PaymentProvider> OnPurchaseStarted;

        // Cuando un pago se completa exitosamente
        public static event System.Action<PaymentResult> OnPurchaseCompleted;

        // Cuando un pago falla
        public static event System.Action<PaymentResult> OnPurchaseFailed;

        // Cuando el provider cambia (Stripe → AppleIAP o viceversa)
        public static event System.Action<PaymentProvider /*newProvider*/, string /*reason*/> OnProviderSwitched;

        // Cuando se ejecuta abort protocol
        public static event System.Action<AbortReason> OnAbortProtocolExecuted;

        // Metodos internos para emitir (solo PaymentManager debe llamar estos)
        internal static void EmitPurchaseStarted(string productId, PaymentProvider provider)
            => OnPurchaseStarted?.Invoke(productId, provider);
        internal static void EmitPurchaseCompleted(PaymentResult result)
            => OnPurchaseCompleted?.Invoke(result);
        internal static void EmitPurchaseFailed(PaymentResult result)
            => OnPurchaseFailed?.Invoke(result);
        internal static void EmitProviderSwitched(PaymentProvider p, string reason)
            => OnProviderSwitched?.Invoke(p, reason);
        internal static void EmitAbortExecuted(AbortReason reason)
            => OnAbortProtocolExecuted?.Invoke(reason);
    }

    public enum PaymentProvider
    {
        Stripe,
        AppleIAP,
        None  // Cuando ambos estan caidos
    }
}
```

### Archivo: `Runtime/Payments/Core/PaymentResult.cs`

```csharp
namespace DigitPark.Payments
{
    public class PaymentResult
    {
        public bool Success { get; set; }
        public string TransactionId { get; set; }
        public string ProductId { get; set; }
        public PaymentProvider ProviderUsed { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        public System.DateTime Timestamp { get; set; }
        public bool WasProviderSwitched { get; set; }  // true si se uso fallback

        public static PaymentResult Successful(string productId, string transactionId,
            PaymentProvider provider, bool wasSwitched = false)
        {
            return new PaymentResult
            {
                Success = true,
                ProductId = productId,
                TransactionId = transactionId,
                ProviderUsed = provider,
                Timestamp = System.DateTime.UtcNow,
                WasProviderSwitched = wasSwitched
            };
        }

        public static PaymentResult Failed(string productId, string errorCode,
            string errorMessage, PaymentProvider provider)
        {
            return new PaymentResult
            {
                Success = false,
                ProductId = productId,
                ErrorCode = errorCode,
                ErrorMessage = errorMessage,
                ProviderUsed = provider,
                Timestamp = System.DateTime.UtcNow
            };
        }
    }
}
```

### Archivo: `Runtime/Payments/Core/PaymentConfig.cs`

```csharp
namespace DigitPark.Payments
{
    [System.Serializable]
    public class PaymentConfig
    {
        // Stripe
        public string stripePublishableKey;
        public string stripeBackendUrl;

        // Apple IAP
        public string appleSharedSecret;
        public string iapValidationUrl;

        // General
        public string backendBaseUrl;
        public int stripeCheckoutTimeoutSeconds = 300;  // 5 min max
        public int stripePollingIntervalMs = 2000;       // Poll cada 2s
        public int maxStripeRetries = 3;
        public float sessionPollingTimeoutMinutes = 5f;
    }
}
```

### Archivo: `Runtime/Payments/Core/IPaymentProvider.cs`

```csharp
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DigitPark.Payments
{
    /// <summary>
    /// Interfaz que implementan tanto StripePaymentProvider como AppleIAPProvider.
    /// PaymentManager decide a quien llamar basado en PaymentFeatureFlag.
    /// NUNCA implementada por nada relacionado con Triumph.
    /// </summary>
    public interface IPaymentProvider
    {
        /// <summary>Nombre del provider para logging ("Stripe" o "AppleIAP")</summary>
        string ProviderName { get; }

        /// <summary>Si el provider esta disponible e inicializado</summary>
        bool IsAvailable { get; }

        /// <summary>Si el provider esta healthy (ultimo check)</summary>
        bool IsHealthy { get; }

        /// <summary>Inicializa el provider con configuracion</summary>
        Task Initialize(PaymentConfig config);

        /// <summary>Compra un producto</summary>
        Task<PaymentResult> PurchaseProduct(CosmeticProduct product, string userId);

        /// <summary>Restaura compras anteriores (solo aplica para IAP)</summary>
        Task<PaymentResult> RestorePurchases(string userId);

        /// <summary>Obtiene lista de productos disponibles con precios</summary>
        Task<List<CosmeticProduct>> FetchProducts(List<string> productIds);

        /// <summary>Health check rapido</summary>
        Task<bool> HealthCheck();

        /// <summary>Limpieza al desactivar</summary>
        void Dispose();
    }
}
```

### Archivo: `Runtime/Payments/Core/ProductCatalog.cs`

```csharp
using System.Collections.Generic;

namespace DigitPark.Payments
{
    /// <summary>
    /// Tipo de producto cosmetico
    /// </summary>
    public enum CosmeticProductType
    {
        Consumable,       // Gem packs (se pueden comprar multiples veces)
        NonConsumable,    // Skins, temas (se compran una vez)
        Subscription      // Season pass (renovacion periodica)
    }

    /// <summary>
    /// Definicion de producto cosmetico.
    /// Estos son los productos que pasan por Stripe/AppleIAP.
    /// NUNCA incluye entry fees, depositos, o cualquier cosa de Triumph.
    /// </summary>
    public class CosmeticProduct
    {
        public string ProductId { get; set; }
        public string DisplayName { get; set; }
        public CosmeticProductType Type { get; set; }
        public decimal PriceUSD { get; set; }
        public string StripePriceId { get; set; }     // Stripe Price ID (para Checkout)
        public string AppleProductId { get; set; }     // App Store product ID
        public int GemsAmount { get; set; }            // Si es gem pack
        public int BonusPercent { get; set; }          // Bonus percentage
        public string ThemeId { get; set; }            // Si desbloquea tema
        public Dictionary<string, string> Metadata { get; set; }

        public CosmeticProduct()
        {
            Metadata = new Dictionary<string, string>
            {
                { "type", "cosmetic" },
                { "app", "digit_park_pro" },
                { "has_tournament_benefit", "false" }
            };
        }
    }

    /// <summary>
    /// Catalogo de productos cosmeticos separado por version.
    /// REGLA CRITICA: Ningun producto puede tener:
    ///   - "tournament" en su ID o metadata
    ///   - "prize" en cualquier campo
    ///   - "cash" referido a dinero real
    ///   - "entry_fee" en cualquier campo
    ///   - "real_money" indicando que es gambling
    /// </summary>
    public static class ProductCatalog
    {
        // =====================================================
        // PRODUCTOS PRO (US) — Stripe primary, AppleIAP failsafe
        // =====================================================

        // Gem Packs (Consumable) — "Sparks" virtual currency, no valor real
        // Los IDs de Apple IAP deben coincidir con los de PremiumManager
        public static readonly CosmeticProduct[] ProProducts = new CosmeticProduct[]
        {
            new CosmeticProduct
            {
                ProductId = "sparks_100",
                DisplayName = "100 Sparks",
                Type = CosmeticProductType.Consumable,
                PriceUSD = 0.99m,
                AppleProductId = "com.matrixsoftware.digitpark.gems_100",
                GemsAmount = 100,
                BonusPercent = 0
            },
            new CosmeticProduct
            {
                ProductId = "sparks_500",
                DisplayName = "500 Sparks",
                Type = CosmeticProductType.Consumable,
                PriceUSD = 4.99m,
                AppleProductId = "com.matrixsoftware.digitpark.gems_500",
                GemsAmount = 500,
                BonusPercent = 10
            },
            new CosmeticProduct
            {
                ProductId = "sparks_1200",
                DisplayName = "1,200 Sparks",
                Type = CosmeticProductType.Consumable,
                PriceUSD = 9.99m,
                AppleProductId = "com.matrixsoftware.digitpark.gems_1200",
                GemsAmount = 1200,
                BonusPercent = 20
            },
            new CosmeticProduct
            {
                ProductId = "sparks_2500",
                DisplayName = "2,500 Sparks",
                Type = CosmeticProductType.Consumable,
                PriceUSD = 19.99m,
                AppleProductId = "com.matrixsoftware.digitpark.gems_2500",
                GemsAmount = 2500,
                BonusPercent = 25
            },
            new CosmeticProduct
            {
                ProductId = "sparks_6500",
                DisplayName = "6,500 Sparks",
                Type = CosmeticProductType.Consumable,
                PriceUSD = 49.99m,
                AppleProductId = "com.matrixsoftware.digitpark.gems_6500",
                GemsAmount = 6500,
                BonusPercent = 30
            },
            new CosmeticProduct
            {
                ProductId = "sparks_14000",
                DisplayName = "14,000 Sparks",
                Type = CosmeticProductType.Consumable,
                PriceUSD = 99.99m,
                AppleProductId = "com.matrixsoftware.digitpark.gems_14000",
                GemsAmount = 14000,
                BonusPercent = 35
            },
            // Non-Consumable (Premium features/bundles — COSMETICOS SOLAMENTE)
            new CosmeticProduct
            {
                ProductId = "premium_bundle",
                DisplayName = "Premium Theme Bundle",
                Type = CosmeticProductType.NonConsumable,
                PriceUSD = 26.25m,
                AppleProductId = "com.matrixsoftware.digitpark.premium_bundle"
            },
            new CosmeticProduct
            {
                ProductId = "complete_bundle",
                DisplayName = "Complete Theme Collection",
                Type = CosmeticProductType.NonConsumable,
                PriceUSD = 30.45m,
                AppleProductId = "com.matrixsoftware.digitpark.complete_bundle"
            }
        };

        // =====================================================
        // PRODUCTOS GLOBAL (Non-US) — Solo Apple IAP
        // Mismos productos, mismos IDs de Apple (los precios los
        // maneja App Store Connect por region)
        // =====================================================
        public static readonly CosmeticProduct[] GlobalProducts = ProProducts;
        // Nota: Los mismos productos. La diferencia es que Global
        // NO tiene Stripe como opcion ni Triumph.

        /// <summary>
        /// Retorna catalogo correcto segun version de la app
        /// </summary>
        public static CosmeticProduct[] GetCatalog(AppVersion version)
        {
            return version == AppVersion.Pro ? ProProducts : GlobalProducts;
        }

        /// <summary>
        /// Busca producto por ID
        /// </summary>
        public static CosmeticProduct FindProduct(string productId)
        {
            foreach (var p in ProProducts)
            {
                if (p.ProductId == productId) return p;
            }
            return null;
        }

        /// <summary>
        /// VALIDACION: Verifica que NINGUN producto tenga metadata prohibida.
        /// Llamar en Editor y en runtime init.
        /// </summary>
        public static bool ValidateCatalogCompliance()
        {
            string[] prohibitedTerms = { "tournament", "prize", "cash_game",
                "skill_game", "real_money", "entry_fee", "wager", "bet", "gambling" };

            foreach (var product in ProProducts)
            {
                string serialized = UnityEngine.JsonUtility.ToJson(product).ToLowerInvariant();
                foreach (var term in prohibitedTerms)
                {
                    if (serialized.Contains(term))
                    {
                        UnityEngine.Debug.LogError(
                            $"[ProductCatalog] COMPLIANCE VIOLATION: Product '{product.ProductId}' " +
                            $"contains prohibited term '{term}'. This product MUST NOT touch Stripe.");
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
```

### Archivo: `Runtime/Payments/Core/PaymentManager.cs`

Este es el orquestador central. Detalles de implementacion:

```csharp
// namespace DigitPark.Payments
// public class PaymentManager : MonoBehaviour (singleton)

// ============ FLUJO DE COMPRA ============
//
// 1. ShopManager.ProcessIAPPurchase(productId) llama a:
//    PaymentManager.Instance.PurchaseCosmetic(productId)
//
// 2. PaymentManager lee PaymentFeatureFlag.ActiveCosmeticProvider
//    → Si es Stripe: usa _stripeProvider.PurchaseProduct()
//    → Si es AppleIAP: usa _iapProvider.PurchaseProduct()
//
// 3. Si Stripe falla (cualquier error):
//    a. Loguea el fallo + razon
//    b. Incrementa _stripeFailureCount
//    c. Si _stripeFailureCount >= MAX_RETRIES (3):
//       - Llama PaymentFeatureFlag.ForceSwitch(AppleIAP)
//    d. INMEDIATAMENTE intenta con _iapProvider.PurchaseProduct()
//    e. Si IAP funciona: retorna exito con WasProviderSwitched=true
//    f. Si IAP tambien falla: retorna error final
//
// 4. Si compra exitosa:
//    a. Llama EntitlementService.Grant(productId, provider, transactionId)
//    b. Si es gem pack: llama CurrencyManager.ProcessGemsPurchase()
//    c. Si es theme: llama ThemeManager.UnlockTheme()
//    d. Emite PaymentEvents.OnPurchaseCompleted
//    e. Loguea a AnalyticsService
//
// ============ INICIALIZACION ============
//
// En Awake():
//   - Singleton + DontDestroyOnLoad
//   - Crea ambos providers (Stripe + AppleIAP)
//   - SIEMPRE inicializa AppleIAP (es el failsafe, debe estar listo)
//   - Inicializa Stripe solo si HAS_STRIPE esta definido y PaymentFeatureFlag lo permite
//   - Carga cached products
//
// ============ CAMPOS ============
//
// [SerializeField] PaymentConfig _config;
// IPaymentProvider _stripeProvider;   // null en version Global
// IPaymentProvider _iapProvider;      // SIEMPRE inicializado
// int _stripeFailureCount = 0;
// const int MAX_STRIPE_FAILURES = 3;
// bool _isPurchaseInProgress = false; // Previene compras simultaneas
//
// ============ METODOS PUBLICOS ============
//
// async Task<PaymentResult> PurchaseCosmetic(string productId)
//   → Flujo principal descrito arriba
//
// async Task<PaymentResult> RestorePurchases()
//   → Solo delega a _iapProvider.RestorePurchases()
//   → Stripe no necesita restore (server-side)
//
// PaymentProvider GetActiveProvider()
//   → Lee PaymentFeatureFlag.ActiveCosmeticProvider
//
// void ResetStripeFailureCount()
//   → Llamar cuando Remote Config confirma que Stripe esta healthy
//
// ============ REGLA CRITICA ============
//
// PaymentManager NUNCA importa, referencia, o llama a:
//   - TriumphManager
//   - ServiceLocator (el de CashBattle)
//   - IWalletService, IKYCService, IMatchmakingService, ITournamentService
//   - Cualquier namespace DigitPark.Services.Triumph
//
// Si necesita interactuar con algo de CashBattle, NO LO HACE.
// Son sistemas completamente independientes.
```

### Notas sobre integracion con PremiumManager existente

El PremiumManager existente YA implementa `IDetailedStoreListener` de Unity IAP. Hay dos approaches:

**Approach A (Recomendado): Wrapper sobre PremiumManager**
- AppleIAPProvider.cs delega internamente a PremiumManager para las compras IAP
- PremiumManager sigue manejando Unity IAP directamente
- PaymentManager solo habla con IPaymentProvider, no con PremiumManager

**Approach B: Refactorizar PremiumManager**
- Extraer la logica de Unity IAP a AppleIAPProvider
- PremiumManager se convierte en thin wrapper que usa PaymentManager
- Mas limpio pero mas riesgo de romper cosas

**Decision: Approach A**. Menos riesgo, PremiumManager ya funciona.

### Modificacion necesaria en PremiumManager.cs

Agregar un metodo publico que AppleIAPProvider pueda llamar:

```csharp
// AGREGAR a PremiumManager.cs (al final de la clase, antes del ultimo })

#region PaymentManager Bridge
/// <summary>
/// Permite que AppleIAPProvider inicie una compra IAP programaticamente.
/// Retorna via callback porque Unity IAP es asincrono basado en callbacks.
/// </summary>
public void PurchaseProduct(string appleProductId,
    System.Action<bool, string> onComplete)
{
    _pendingPurchaseCallback = onComplete;
    PurchaseByProductId(appleProductId);
}

private System.Action<bool, string> _pendingPurchaseCallback;

// Modificar OnPurchaseSucceeded y OnPurchaseFailed existentes para llamar:
// _pendingPurchaseCallback?.Invoke(true/false, transactionId/errorMsg);
// _pendingPurchaseCallback = null;
#endregion
```

### Modificacion necesaria en ShopManager.cs

Cambiar la logica de compra con dinero real para usar PaymentManager:

```csharp
// En ShopManager.cs, donde actualmente llama a PremiumManager para compras RealMoney,
// cambiar a:

private async void ProcessRealMoneyPurchase(ShopItemData item)
{
    if (PaymentManager.Instance == null)
    {
        Debug.LogError("[ShopManager] PaymentManager no disponible");
        return;
    }

    var result = await PaymentManager.Instance.PurchaseCosmetic(item.iapProductId);

    if (result.Success)
    {
        item.GrantRewards();
        PlayPurchaseCelebration();

        if (result.WasProviderSwitched)
        {
            Debug.Log($"[ShopManager] Compra exitosa via fallback ({result.ProviderUsed})");
        }
    }
    else
    {
        ShowPurchaseError(result.ErrorMessage);
    }
}
```

---

## FASE 3: Feature Flag System <a id="fase-3"></a>

### Archivo: `Runtime/Payments/FeatureFlags/PaymentFeatureFlag.cs`

```csharp
// namespace DigitPark.Payments
// public static class PaymentFeatureFlag

// Propiedades:
//   static PaymentProvider ActiveCosmeticProvider { get; private set; }
//   static AppVersion CurrentVersion { get; private set; }
//   static bool IsStripeEnabled { get; private set; }
//   static bool IsAppleIAPEnabled { get; private set; }
//   static bool IsTriumphEnabled { get; private set; }
//   static bool IsProVersion => CurrentVersion == AppVersion.Pro;
//   static bool IsMaintenanceMode { get; private set; }

// Metodos:
//   static void Initialize(RemoteConfigData data)
//     → Setea todos los flags desde Remote Config
//     → Si data es null (offline), usa LocalFlagCache
//
//   static void ForceSwitch(PaymentProvider provider, string reason)
//     → Cambia ActiveCosmeticProvider inmediatamente
//     → Guarda en LocalFlagCache
//     → Emite PaymentEvents.OnProviderSwitched
//     → Loguea a AnalyticsService
//
//   static void UpdateFromRemoteConfig(RemoteConfigData data)
//     → Actualiza flags sin resetear failure counts
//
// Enum AppVersion:
//   Pro,    // US - Triumph + Stripe + IAP
//   Global  // ROW - IAP only

// Inicializacion:
//   En la primera llamada, detectar version automaticamente:
//   #if DIGIT_PARK_PRO
//     CurrentVersion = AppVersion.Pro;
//   #elif DIGIT_PARK_GLOBAL
//     CurrentVersion = AppVersion.Global;
//   #else
//     CurrentVersion = AppVersion.Pro; // Default for development
//   #endif
```

### Archivo: `Runtime/Payments/FeatureFlags/RemoteConfigService.cs`

```csharp
// namespace DigitPark.Payments
// public class RemoteConfigService : MonoBehaviour (singleton)

// IMPORTANTE: Firebase Remote Config NO esta instalado actualmente.
// Se debe agregar el package: com.google.firebase.remote-config
// Ver seccion "Pasos Manuales" al final del documento.

// Funcionalidad:
//   - Fetch de Firebase Remote Config en Start()
//   - Poll cada 15 minutos via InvokeRepeating
//   - Cache result en LocalFlagCache
//   - Expone evento OnConfigUpdated
//
// Remote Config Keys:
//   "payment_provider"       → "stripe" | "apple_iap"
//   "stripe_enabled"         → true/false
//   "apple_iap_enabled"      → true/false
//   "cosmetic_store_enabled" → true/false
//   "triumph_enabled"        → true/false
//   "app_version"            → "pro" | "global"
//   "maintenance_mode"       → true/false
//
// Metodo ForceRemoteSwitch(string provider):
//   → Hace un fetch inmediato para confirmar el switch
//   → Si no puede fetchear, confiar en cache local
//
// Fallback chain:
//   1. Firebase Remote Config (primary)
//   2. LocalFlagCache (PlayerPrefs)
//   3. Defaults hardcoded (AppleIAP como provider seguro)
//
// NOTA SOBRE FIREBASE REMOTE CONFIG:
//   El proyecto ya tiene Firebase Auth, Database, Analytics, Messaging.
//   Remote Config es un package adicional que se debe instalar.
//   Si no esta instalado, este servicio debe funcionar solo con LocalFlagCache.
//   Usar #if FIREBASE_REMOTE_CONFIG para condicionar el codigo de Firebase.
```

### Archivo: `Runtime/Payments/FeatureFlags/LocalFlagCache.cs`

```csharp
// namespace DigitPark.Payments
// public static class LocalFlagCache

// PlayerPrefs keys:
//   "dp_payment_provider"     → "stripe" | "apple_iap"
//   "dp_stripe_enabled"       → "true" | "false"
//   "dp_iap_enabled"          → "true" | "false"
//   "dp_triumph_enabled"      → "true" | "false"
//   "dp_maintenance_mode"     → "true" | "false"
//   "dp_flags_timestamp"      → ISO 8601 timestamp del ultimo update
//   "dp_flags_source"         → "remote" | "local" | "default"
//
// Metodos:
//   static void Save(RemoteConfigData data)
//   static RemoteConfigData Load()
//   static bool HasCachedFlags()
//   static System.DateTime GetLastUpdateTime()
//   static RemoteConfigData GetDefaults()
//     → Retorna defaults seguros:
//       payment_provider = "apple_iap" (failsafe)
//       stripe_enabled = false
//       apple_iap_enabled = true
//       triumph_enabled = false (solo en Pro via #if)
//       maintenance_mode = false
```

---

## FASE 4: Stripe Integration <a id="fase-4"></a>

### Archivo: `Runtime/Payments/Stripe/StripePaymentProvider.cs`

```csharp
// namespace DigitPark.Payments.Stripe
// public class StripePaymentProvider : IPaymentProvider
// Envuelto en #if HAS_STRIPE ... #endif

// ============ FLUJO DE COMPRA VIA STRIPE ============
//
// 1. PurchaseProduct(product, userId) es llamado por PaymentManager
//
// 2. Llama al backend: POST /api/stripe/create-checkout-session
//    Body: { productId, userId, appVersion: "pro", priceId: product.StripePriceId }
//    Header: X-App-Version: pro
//
// 3. Backend retorna: { sessionId, checkoutUrl }
//
// 4. Abre checkoutUrl en Safari View Controller (iOS)
//    → En Editor/Android: abre en el browser del sistema
//    → Usa Application.OpenURL() como fallback
//    → En iOS produccion: usa SFSafariViewController via native bridge
//
// 5. Inicia StripeSessionPoller para monitorear estado:
//    → GET /api/stripe/session-status/{sessionId} cada 2 segundos
//    → Timeout: 5 minutos
//
// 6. Cuando poller detecta "completed":
//    → Retorna PaymentResult.Successful()
//
// 7. Si poller detecta "expired", "canceled", o timeout:
//    → Retorna PaymentResult.Failed()
//    → PaymentManager decidira si hacer fallback a IAP
//
// 8. Si hay error de red al llamar al backend:
//    → Retorna PaymentResult.Failed() inmediatamente
//    → PaymentManager hara fallback
//
// ============ HEALTH CHECK ============
//
// HealthCheck() → GET /api/health/payments
// Si responde y stripe.status == "healthy", retorna true
// Si no responde en 5 segundos, retorna false
//
// ============ COMPLIANCE ============
//
// Antes de enviar CUALQUIER request al backend de Stripe:
//   StripeComplianceGuard.ValidateProduct(product) debe retornar true
//   Si retorna false, la compra se rechaza con error de compliance
```

### Archivo: `Runtime/Payments/Stripe/StripeCheckoutController.cs`

```csharp
// namespace DigitPark.Payments.Stripe
// public class StripeCheckoutController
// Envuelto en #if HAS_STRIPE ... #endif

// Maneja la apertura del checkout en Safari View Controller:
//
// iOS Nativo (via StoreKitBridge.mm o plugin dedicado):
//   - SFSafariViewController para checkout en-app
//   - Universal Links para retorno (digitpark://stripe-return?session_id=xxx)
//   - El usuario NUNCA sale de la app completamente
//
// Fallback (Editor/Android):
//   - Application.OpenURL(checkoutUrl)
//   - Polling de session-status para detectar completado
//
// Deep Link Handler:
//   - DeepLinkService ya maneja "digitpark://" URLs
//   - Agregar handler para "digitpark://stripe-return"
//   - Extraer session_id del query string
//   - Notificar a StripePaymentProvider que el checkout termino
//
// INTEGRACION CON DeepLinkService.cs EXISTENTE:
//   El proyecto ya tiene DeepLinkService que maneja digitpark:// URLs.
//   Agregar un callback registration:
//     DeepLinkService.Instance.RegisterHandler("stripe-return", OnStripeReturn);
```

### Archivo: `Runtime/Payments/Stripe/StripeSessionPoller.cs`

```csharp
// namespace DigitPark.Payments.Stripe
// public class StripeSessionPoller
// Envuelto en #if HAS_STRIPE ... #endif

// Polling via coroutine o async/await:
//
// async Task<StripeSessionStatus> PollUntilComplete(string sessionId,
//     float timeoutMinutes = 5f, float intervalSeconds = 2f)
//
// StripeSessionStatus:
//   - Pending     → Seguir polling
//   - Completed   → Pago exitoso, retornar a PaymentProvider
//   - Expired     → Session expirada, fallar
//   - Canceled    → Usuario cancelo, fallar
//   - Error       → Error de red, fallar
//
// Usa UnityWebRequest para las llamadas HTTP:
//   GET {backendUrl}/api/stripe/session-status/{sessionId}
//   Header: X-App-Version: pro
//   Timeout: 10 seconds per request
```

### Archivo: `Runtime/Payments/Stripe/StripeComplianceGuard.cs`

```csharp
// namespace DigitPark.Payments.Compliance
// public static class StripeComplianceGuard

// Metodos:
//
// static bool ValidateProduct(CosmeticProduct product)
//   → Verifica que el producto NO contiene terminos prohibidos
//   → Prohibidos: "tournament", "prize", "cash_game", "skill_game",
//     "real_money", "entry_fee", "wager", "bet", "gambling", "triumph"
//   → Verifica en: ProductId, DisplayName, Metadata values
//   → Si falla: Debug.LogError + retorna false
//
// static bool ValidateSessionMetadata(Dictionary<string, string> metadata)
//   → Misma validacion pero para metadata de Stripe session
//   → Asegura que metadata siempre contenga:
//     type = "cosmetic"
//     has_tournament_benefit = "false"
//
// static void LogComplianceAudit(string productId, string provider,
//     bool passed, string details)
//   → Registra en PlayerPrefs un log de auditoria
//   → Formato: timestamp|productId|provider|passed|details
//   → Mantener ultimas 100 entradas
//
// VALIDACION EN EDITOR (Editor script):
//   En OnValidate() o via menu DigitPark/Validate Stripe Compliance:
//   → Corre ValidateCatalog() sobre todos los productos
//   → Escanea TODOS los archivos .cs en Payments/Stripe/ buscando:
//     - "using DigitPark.Services.Triumph"
//     - "using DigitPark.Services.Interfaces"
//     - "ServiceLocator"
//     - "TriumphManager"
//   → Si encuentra alguno: FALLA con mensaje claro
```

---

## FASE 5: Apple IAP Failsafe Provider <a id="fase-5"></a>

### Archivo: `Runtime/Payments/AppleIAP/AppleIAPProvider.cs`

```csharp
// namespace DigitPark.Payments.AppleIAP
// public class AppleIAPProvider : IPaymentProvider

// ESTRATEGIA: Wrapper sobre PremiumManager existente.
// PremiumManager YA tiene Unity IAP funcionando con IDetailedStoreListener.
// AppleIAPProvider adapta la interfaz IPaymentProvider delegando a PremiumManager.
//
// ============ INICIALIZACION ============
//
// Initialize(config):
//   → Verifica que PremiumManager.Instance existe
//   → Verifica que Unity IAP esta inicializado (PremiumManager internamente)
//   → Pre-carga productos para que el switch sea instantaneo
//   → Marca IsAvailable = true si PremiumManager esta listo
//
// NOTA: PremiumManager ya se inicializa en Awake() con DontDestroyOnLoad.
// AppleIAPProvider NO re-inicializa Unity IAP, solo lo usa.
//
// ============ PURCHASE ============
//
// async Task<PaymentResult> PurchaseProduct(CosmeticProduct product, string userId):
//   → Crea un TaskCompletionSource<PaymentResult>
//   → Llama PremiumManager.Instance.PurchaseProduct(product.AppleProductId, callback)
//   → callback(success, transactionId) resuelve el TaskCompletionSource
//   → Retorna el result
//
// Si PremiumManager no existe o IAP no inicializado:
//   → Retorna PaymentResult.Failed("IAP not available")
//
// ============ RESTORE ============
//
// RestorePurchases(userId):
//   → Llama PremiumManager.Instance.RestorePurchases()
//   → Espera callback
//   → Retorna result
//
// ============ HEALTH CHECK ============
//
// HealthCheck():
//   → Retorna PremiumManager.Instance != null && IsAvailable
//   → Opcionalmente: intenta FetchProducts() como test real
//
// ============ RECEIPT VALIDATION ============
//
// Despues de compra exitosa:
//   → Envia receipt al backend: POST /api/iap/validate-receipt
//   → Body: { receiptData, productId, userId, appVersion }
//   → Si validacion falla: NO revocar entitlement local (el usuario ya pago)
//   → Solo loguear warning para investigacion
```

### Archivo: `Runtime/Payments/AppleIAP/AppleReceiptValidator.cs`

```csharp
// namespace DigitPark.Payments.AppleIAP
// public class AppleReceiptValidator

// Server-side validation via backend:
//
// async Task<ReceiptValidationResult> ValidateReceipt(string receiptData,
//     string productId, string userId)
//
//   → POST {backendUrl}/api/iap/validate-receipt
//   → Body: { receiptData, productId, userId, appVersion, bundleId }
//   → Header: X-App-Version: pro (o global)
//
// ReceiptValidationResult:
//   bool IsValid
//   string TransactionId
//   string ProductId
//   string ErrorMessage
//
// NOTA: PremiumManager ya tiene CrossPlatformValidator (client-side).
// Este validador es ADICIONAL — server-side para seguridad extra.
// Si el backend no responde, la compra sigue siendo valida (ya se cobro).
```

### Archivo: `Plugins/iOS/StoreKitBridge.mm`

```objectivec
// Objective-C bridge para StoreKit 2 features avanzadas
// Solo necesario si queremos features de StoreKit 2 que Unity IAP no expone:
//   - JWS transaction verification
//   - App Store promotional offers
//   - Transaction.currentEntitlements
//
// NOTA: Unity IAP 5.1.2 ya soporta StoreKit 2 internamente en iOS 15+.
// Este bridge es OPCIONAL y solo para features avanzadas.
//
// Si se implementa:
//   - Debe registrar un UnitySendMessage callback para comunicar con C#
//   - Usar StoreKit 2 API (Product, Transaction, AppStore)
//   - Compilar condicionalmente: #if TARGET_OS_IPHONE
//
// RECOMENDACION: No implementar en V1. Unity IAP cubre todo lo necesario.
// Crear archivo placeholder con comentario explicando cuando seria necesario.
```

---

## FASE 6: Triumph Isolation Layer <a id="fase-6"></a>

### Archivo: `Runtime/Payments/Compliance/TriumphIsolationGuard.cs`

```csharp
// namespace DigitPark.Payments.Compliance
// public class TriumphIsolationGuard : MonoBehaviour (singleton)

// ============ PROPOSITO ============
// Monitor runtime que verifica CONTINUAMENTE que no hay
// contaminacion cruzada entre Triumph y Stripe/AppleIAP.
//
// ============ CHECKS QUE EJECUTA ============
//
// 1. En Start() y cada 60 segundos:
//    → VerifyIsolation() que verifica:
//
//    a. Ningun dato de Triumph aparece en PaymentManager._lastTransaction
//    b. Ningun dato de Stripe aparece en ServiceLocator.Wallet
//    c. PaymentEvents.OnPurchaseCompleted subscribers NO incluyen
//       ninguna clase del namespace DigitPark.Services.Triumph
//    d. TriumphManager.OnBalanceChanged subscribers NO incluyen
//       ninguna clase del namespace DigitPark.Payments
//
// 2. Metodo ValidateTransactionIsolation(PaymentResult result):
//    → Llamado por PaymentManager despues de cada transaccion
//    → Verifica que:
//      - result.ProviderUsed es Stripe o AppleIAP (nunca otro)
//      - result.TransactionId no empieza con "triumph_" ni "tmatch_"
//      - result.ProductId esta en ProductCatalog (cosmeticos validos)
//
// 3. Audit log:
//    → Cada 5 minutos escribe en PlayerPrefs:
//      "isolation_audit_latest" = "Triumph: {ON/OFF} | Stripe: {ON/OFF} |
//       Cross-contamination: NONE | Timestamp: {ISO8601}"
//    → Si detecta contaminacion:
//      - Loguea Debug.LogError CRITICO
//      - Llama StripeAbortProtocol.ExecuteAbort(AbortReason.CrossContamination)
//      - Deshabilita AMBOS sistemas de pago
//      - Envia alerta al developer
//
// ============ INTEGRACION ============
//
// En BootManager, despues de inicializar PaymentManager:
//   GameObject guard = new GameObject("TriumphIsolationGuard");
//   guard.AddComponent<TriumphIsolationGuard>();
//
// Solo se activa en version Pro (#if DIGIT_PARK_PRO || UNITY_EDITOR)
// En version Global no hay Triumph, asi que no hay riesgo.
```

### Modificaciones a TriumphManager.cs existente

```csharp
// AGREGAR al inicio del archivo (despues de using statements):
// #if UNITY_EDITOR || HAS_TRIUMPH

// AGREGAR al final del archivo (antes del ultimo }):
// #endif

// AGREGAR constante para identificar transacciones:
public const string TRANSACTION_PREFIX = "triumph_";

// AGREGAR en Initialize():
//   if (TriumphIsolationGuard.Instance != null)
//       TriumphIsolationGuard.Instance.RegisterTriumphActive(true);
```

### Archivo: `Runtime/Payments/Compliance/VersionGuard.cs`

```csharp
// namespace DigitPark.Payments.Compliance
// public static class VersionGuard

// Verifica en runtime que la version correcta accede a los endpoints correctos.
//
// Metodos:
//
// static bool CanAccessStripe()
//   #if DIGIT_PARK_PRO
//     return PaymentFeatureFlag.IsStripeEnabled;
//   #else
//     return false;  // Global NUNCA accede a Stripe
//   #endif
//
// static bool CanAccessTriumph()
//   #if DIGIT_PARK_PRO
//     return PaymentFeatureFlag.IsTriumphEnabled;
//   #else
//     return false;  // Global NUNCA accede a Triumph
//   #endif
//
// static bool CanAccessAppleIAP()
//   return PaymentFeatureFlag.IsAppleIAPEnabled;  // Ambas versiones
//
// static string GetRequiredAppVersionHeader()
//   #if DIGIT_PARK_PRO
//     return "pro";
//   #elif DIGIT_PARK_GLOBAL
//     return "global";
//   #else
//     return "development";
//   #endif
//
// static void ValidateEndpointAccess(string endpoint)
//   → Si endpoint contiene "/stripe/" y !CanAccessStripe(): throw
//   → Si endpoint contiene "/triumph/" y !CanAccessTriumph(): throw
```

---

## FASE 7: Version Separation (Pro vs Global) <a id="fase-7"></a>

### Scripting Define Symbols

En Player Settings → Other Settings → Scripting Define Symbols:

**Build Pro (US):**
```
DIGIT_PARK_PRO;HAS_TRIUMPH;HAS_STRIPE;HAS_APPLE_IAP
```

**Build Global (Non-US):**
```
DIGIT_PARK_GLOBAL;HAS_APPLE_IAP
```

**Desarrollo (Editor):**
```
DIGIT_PARK_PRO;HAS_TRIUMPH;HAS_STRIPE;HAS_APPLE_IAP;UNITY_INCLUDE_TESTS
```

### Archivo: `Editor/Payments/BuildProfileSwitcher.cs`

```csharp
// namespace DigitPark.Editor.Payments
// public class BuildProfileSwitcher : EditorWindow

// Menu: DigitPark/Build Profile Switcher
//
// UI con dos botones:
//   [Switch to PRO]    → Cambia Scripting Define Symbols a PRO
//   [Switch to GLOBAL] → Cambia Scripting Define Symbols a GLOBAL
//
// Tambien muestra:
//   - Version activa actual
//   - Bundle ID que se usaria
//   - Que features estan habilitadas
//
// Implementacion:
//   PlayerSettings.SetScriptingDefineSymbolsForGroup(
//     BuildTargetGroup.iOS, symbols);
//   PlayerSettings.SetApplicationIdentifier(
//     BuildTargetGroup.iOS, bundleId);
//
// Bundle IDs:
//   PRO:    "com.matrixsoftware.digitpark.pro"
//   GLOBAL: "com.matrixsoftware.digitpark"
//
// ADVERTENCIA: Cambiar define symbols causa recompilacion completa.
```

### Codigo condicional en archivos existentes

En BootManager.cs, agregar deteccion de version:

```csharp
// En InitializeGameManagers(), agregar:
#if DIGIT_PARK_PRO
    Debug.Log("[Boot] Version: PRO (US) — Triumph + Stripe + IAP");
    // Inicializar PaymentManager con Stripe + IAP
    InitializePaymentSystem(includeStripe: true, includeTriumph: true);
#elif DIGIT_PARK_GLOBAL
    Debug.Log("[Boot] Version: GLOBAL — IAP only");
    // Inicializar PaymentManager solo con IAP
    InitializePaymentSystem(includeStripe: false, includeTriumph: false);
#else
    Debug.Log("[Boot] Version: DEVELOPMENT — All features enabled");
    InitializePaymentSystem(includeStripe: true, includeTriumph: true);
#endif
```

---

## FASE 8: Entitlement Service <a id="fase-8"></a>

### Archivo: `Runtime/Payments/Entitlements/EntitlementRecord.cs`

```csharp
// namespace DigitPark.Payments.Entitlements

[System.Serializable]
public class EntitlementRecord
{
    public string userId;
    public string productId;
    public string provider;           // "stripe" | "apple_iap" (NUNCA "triumph")
    public string transactionId;
    public string grantedAt;           // ISO 8601
    public string appVersion;          // "pro" | "global"
    public bool hasTournamentBenefit;  // SIEMPRE false
    public bool isCosmetic;            // SIEMPRE true
}
```

### Archivo: `Runtime/Payments/Entitlements/EntitlementService.cs`

```csharp
// namespace DigitPark.Payments.Entitlements
// public class EntitlementService : MonoBehaviour (singleton)

// Fuente de verdad de lo que el usuario posee (compras cosmeticas).
//
// ============ STORAGE ============
//
// Local: PlayerPrefs con JSON serialization (offline-capable)
//   Key: "dp_entitlements_{userId}"
//   Value: JSON array de EntitlementRecord
//
// Remote: Firebase Realtime Database
//   Path: "entitlements/{userId}/{productId}"
//   Sync: bidireccional en cada app launch
//
// Backend: PostgreSQL via REST API
//   GET /api/entitlements/{userId}
//   POST /api/entitlements/sync
//
// ============ METODOS ============
//
// Grant(string productId, string provider, string transactionId)
//   → Crea EntitlementRecord
//   → Guarda local (PlayerPrefs)
//   → Sube a Firebase Database
//   → Envia a backend API
//   → Si Firebase/backend fallan: queue para retry en proximo app launch
//
// bool HasEntitlement(string productId)
//   → Busca en cache local (mas rapido)
//
// List<EntitlementRecord> GetAllEntitlements()
//   → Retorna lista completa desde cache
//
// async Task SyncWithServer()
//   → Llamar en cada app launch
//   → Merge: server tiene entitlements que local no tiene → agregar local
//   → Merge: local tiene entitlements que server no tiene → subir a server
//   → Resultado: ambos estan sincronizados
//
// ============ INTEGRACION CON PREMIUM MANAGER ============
//
// PremiumManager ya guarda premium status en PlayerPrefs.
// EntitlementService es una CAPA ADICIONAL de tracking mas robusta.
// Ambos coexisten — EntitlementService agrega audit trail + sync.
//
// Cuando EntitlementService.Grant() se llama:
//   → Tambien llama PremiumManager.UnlockProduct() si aplica
//   → Para gem packs, NO llama a PremiumManager (CurrencyManager se encarga)
```

---

## FASE 9: Abort Protocol <a id="fase-9"></a>

### Archivo: `Runtime/Payments/Abort/AbortReason.cs`

```csharp
namespace DigitPark.Payments
{
    public enum AbortReason
    {
        StripeCheckoutFailure,      // Checkout returno error 3+ veces
        StripeWebhookTimeout,       // Sin webhooks por 30+ min
        StripeRemoteDisabled,       // Remote Config desactivo Stripe
        ManualDeveloperTrigger,     // 5-finger tap en store
        CrossContamination,         // TriumphIsolationGuard detectó leak
        StripeAccountSuspended,     // HTTP 403 de Stripe
        ComplianceViolation         // StripeComplianceGuard fallo
    }
}
```

### Archivo: `Runtime/Payments/Abort/StripeAbortProtocol.cs`

```csharp
// namespace DigitPark.Payments
// public static class StripeAbortProtocol

// async Task ExecuteAbort(AbortReason reason):
//
//   Paso 1: Switch INMEDIATO a AppleIAP (local)
//     PaymentFeatureFlag.ForceSwitch(PaymentProvider.AppleIAP, reason.ToString());
//
//   Paso 2: Notificar backend para flip Remote Config
//     POST {backendUrl}/api/admin/force-switch
//     Body: { provider: "apple_iap", reason, timestamp }
//     Si falla: no importa, local ya cambio
//
//   Paso 3: Log evento critico
//     AnalyticsService.Instance?.LogEvent("stripe_abort_executed", new Dictionary{
//       {"reason", reason.ToString()},
//       {"timestamp", DateTime.UtcNow.ToString("O")}
//     });
//
//   Paso 4: Verificar que AppleIAP esta respondiendo
//     bool iapHealthy = await PaymentManager.Instance.GetIAPProvider().HealthCheck();
//     if (!iapHealthy):
//       Debug.LogError("[ABORT] Apple IAP TAMBIEN caido. Tienda deshabilitada.");
//       PaymentFeatureFlag.ForceSwitch(PaymentProvider.None, "both_down");
//
//   Paso 5: Refresh UIs activas
//     PaymentEvents.EmitProviderSwitched(PaymentProvider.AppleIAP, reason.ToString());
//
//   Paso 6: Alerta al developer (opcional — via backend webhook)
//     POST {backendUrl}/api/alerts/developer
//     Body: { message: "Stripe abort ejecutado", reason, timestamp }
//
// TRIGGERS AUTOMATICOS (configurar en PaymentManager):
//
//   - Stripe falla 3 veces consecutivas → auto-abort
//   - Remote Config stripe_enabled=false → auto-abort
//   - TriumphIsolationGuard detecta contamination → auto-abort
//
// TRIGGER MANUAL (hidden gesture):
//   En CosmeticStoreUI (o ShopManager), detectar gesto de 5 dedos:
//   Input.touchCount >= 5 && touch duration > 3 seconds → abort
//   Solo en builds con DIGIT_PARK_PRO || DEVELOPMENT_BUILD
```

---

## FASE 10: Backend (Node.js + TypeScript) <a id="fase-10"></a>

### Estructura completa del backend

El backend vive en `Backend/` en la raiz del repo (al mismo nivel que `Assets/`).
Es un proyecto Node.js independiente con su propio package.json.

### Archivo: `Backend/package.json`

```json
{
  "name": "digitpark-payment-backend",
  "version": "1.0.0",
  "scripts": {
    "dev": "ts-node-dev --respawn src/index.ts",
    "build": "tsc",
    "start": "node dist/index.js",
    "test": "jest"
  },
  "dependencies": {
    "express": "^4.18.2",
    "stripe": "^14.0.0",
    "cors": "^2.8.5",
    "helmet": "^7.1.0",
    "express-rate-limit": "^7.1.5",
    "pg": "^8.11.3",
    "redis": "^4.6.12",
    "firebase-admin": "^12.0.0",
    "dotenv": "^16.3.1",
    "winston": "^3.11.0",
    "uuid": "^9.0.0"
  },
  "devDependencies": {
    "typescript": "^5.3.3",
    "@types/express": "^4.17.21",
    "@types/cors": "^2.8.17",
    "@types/pg": "^8.10.9",
    "@types/uuid": "^9.0.7",
    "ts-node-dev": "^2.0.0",
    "jest": "^29.7.0",
    "@types/jest": "^29.5.11",
    "ts-jest": "^29.1.1"
  }
}
```

### Archivo: `Backend/.env.example`

```bash
# ====== STRIPE (Solo para version Pro - cosmeticos) ======
STRIPE_SECRET_KEY=sk_test_...
STRIPE_WEBHOOK_SECRET=whsec_...
STRIPE_PUBLISHABLE_KEY=pk_test_...

# ====== APPLE IAP ======
APPLE_SHARED_SECRET=your_shared_secret_here

# ====== FIREBASE ======
FIREBASE_PROJECT_ID=digitpark-xxxxx
FIREBASE_SERVICE_ACCOUNT_PATH=./firebase-service-account.json

# ====== DATABASE ======
DATABASE_URL=postgresql://user:pass@localhost:5432/digitpark_payments
REDIS_URL=redis://localhost:6379

# ====== APP CONFIG ======
APP_VERSION_PRO=true
BACKEND_URL_PRO=https://api-pro.digitpark.com
BACKEND_URL_GLOBAL=https://api-global.digitpark.com
PORT=3000

# ====== TRIUMPH (referencia, NO usado en este backend) ======
TRIUMPH_ISOLATION_MODE=strict

# ====== ALERTS ======
ALERT_EMAIL=dev@digitpark.com
SLACK_WEBHOOK_URL=https://hooks.slack.com/services/xxx/yyy/zzz
```

### Archivos del backend — Especificacion para cada uno:

**`src/index.ts`**: Express server con middleware chain: helmet → cors → rateLimiter → versionGuard → triumphIsolation → routes → error handler. Puerto configurable via env.

**`src/config/environment.ts`**: Validacion de env vars. Si falta alguna critica, process.exit(1). Exporta objeto tipado `config`.

**`src/config/stripe.config.ts`**: Inicializa Stripe SDK con secret key. Exporta instancia de Stripe.

**`src/routes/stripe.routes.ts`**:
- `POST /api/stripe/create-checkout-session` — Crea session con product metadata (type=cosmetic, has_tournament=false). Valida appVersion=pro.
- `POST /api/stripe/webhook` — Procesa Stripe events (checkout.session.completed, payment_intent.payment_failed). En success: llama entitlement.service.grant(). CRITICO: rechaza cualquier evento con metadata "tournament".
- `GET /api/stripe/session-status/:sessionId` — Retorna status de session para polling del cliente.

**`src/routes/iap.routes.ts`**:
- `POST /api/iap/validate-receipt` — Recibe receiptData + productId + userId + appVersion. Valida contra Apple production server primero, sandbox como fallback. Verifica bundleId (pro vs global). En success: llama entitlement.service.grant().

**`src/routes/entitlements.routes.ts`**:
- `GET /api/entitlements/:userId` — Retorna todos los entitlements activos.
- `POST /api/entitlements/grant` — Uso interno (desde webhook/validation).
- `POST /api/entitlements/sync` — Unity llama esto en app launch. Merge bidireccional.

**`src/routes/health.routes.ts`**:
- `GET /api/health/payments` — Retorna status JSON de Stripe, AppleIAP, Triumph isolation, active provider, abort readiness.

**`src/services/stripe.service.ts`**: Logica de creacion de Checkout Session + procesamiento de webhooks. NUNCA procesa metadata con "tournament". Genera audit trail.

**`src/services/appleIAP.service.ts`**: Validacion de receipts contra Apple. Primero production URL, si status=21007 reintenta con sandbox URL.

**`src/services/entitlement.service.ts`**: CRUD de entitlements en PostgreSQL. Grant + query + sync. Audit trail en tabla separada.

**`src/services/featureFlag.service.ts`**: Lee/escribe Firebase Remote Config via Admin SDK.

**`src/services/alert.service.ts`**: Envia alertas via email (nodemailer) y Slack (webhook POST).

**`src/middleware/triumphIsolation.middleware.ts`**: En CADA request, verifica que body/params/headers NO contienen: "triumph", "tournament_entry", "match_fee", "deposit_real_money". Si detecta: 403 + log CRITICO.

**`src/middleware/versionGuard.middleware.ts`**: Valida header X-App-Version. Rutas /api/stripe/* requieren "pro". Rutas /api/triumph/* requieren "pro". Version "global" solo accede a /api/iap/* y /api/entitlements/*.

**`src/middleware/rateLimiter.middleware.ts`**: express-rate-limit con limites por IP: 100 requests/15min general, 10 requests/min para checkout creation.

**`src/database/schema.sql`**:
```sql
CREATE TABLE entitlements (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id VARCHAR(128) NOT NULL,
    product_id VARCHAR(128) NOT NULL,
    provider VARCHAR(20) NOT NULL CHECK (provider IN ('stripe', 'apple_iap')),
    transaction_id VARCHAR(256) NOT NULL,
    granted_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    app_version VARCHAR(20) NOT NULL CHECK (app_version IN ('pro', 'global')),
    has_tournament_benefit BOOLEAN NOT NULL DEFAULT FALSE,
    is_cosmetic BOOLEAN NOT NULL DEFAULT TRUE,
    UNIQUE(user_id, product_id, transaction_id)
);

CREATE TABLE payment_audit (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id VARCHAR(128) NOT NULL,
    product_id VARCHAR(128),
    provider VARCHAR(20),
    action VARCHAR(50) NOT NULL,
    status VARCHAR(20) NOT NULL,
    details JSONB,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE stripe_sessions (
    session_id VARCHAR(256) PRIMARY KEY,
    user_id VARCHAR(128) NOT NULL,
    product_id VARCHAR(128) NOT NULL,
    status VARCHAR(20) NOT NULL DEFAULT 'pending',
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    completed_at TIMESTAMPTZ
);

CREATE INDEX idx_entitlements_user ON entitlements(user_id);
CREATE INDEX idx_audit_user ON payment_audit(user_id);
CREATE INDEX idx_sessions_user ON stripe_sessions(user_id);
```

---

## FASE 11: UI Integration <a id="fase-11"></a>

### Archivo: `Runtime/Payments/UI/PaymentLoadingOverlay.cs`

```csharp
// namespace DigitPark.Payments.UI
// public class PaymentLoadingOverlay : MonoBehaviour

// Canvas overlay semi-transparente con spinner + texto
// Mostrar cuando: compra en progreso
// Texto: AutoLocalizer.Get("payment_processing")
// Bloquea input del usuario (raycast block)
// Auto-dismiss tras compra o timeout
//
// Integracion:
//   PaymentEvents.OnPurchaseStarted += Show;
//   PaymentEvents.OnPurchaseCompleted += Hide;
//   PaymentEvents.OnPurchaseFailed += Hide;
```

### Archivo: `Runtime/Payments/UI/PaymentErrorDialog.cs`

```csharp
// namespace DigitPark.Payments.UI
// public class PaymentErrorDialog : MonoBehaviour

// Popup de error reutilizable (usa PopupManager existente)
//
// Casos:
//   1. Stripe fallo pero IAP funciono → NO mostrar error (seamless)
//   2. Ambos fallaron → Mostrar "payment_unavailable" con boton Retry
//   3. Maintenance mode → Mostrar "store_maintenance" sin boton
//
// Integracion con PopupManager existente:
//   PopupManager.Instance?.ShowPopup(title, message, buttons);
```

### Nuevas claves de traduccion para Translations.txt

```
payment_processing
  EN: Processing payment...
  ES: Procesando pago...
  FR: Traitement du paiement...
  PT: Processando pagamento...
  DE: Zahlung wird verarbeitet...

payment_unavailable
  EN: Payment temporarily unavailable. Please try again later.
  ES: Pago temporalmente no disponible. Intente de nuevo mas tarde.
  FR: Paiement temporairement indisponible. Veuillez reessayer plus tard.
  PT: Pagamento temporariamente indisponivel. Tente novamente mais tarde.
  DE: Zahlung vorubergehend nicht verfugbar. Bitte versuchen Sie es spater erneut.

store_maintenance
  EN: Store is under maintenance. We'll be back soon!
  ES: La tienda esta en mantenimiento. Volveremos pronto!
  FR: La boutique est en maintenance. Nous reviendrons bientot!
  PT: A loja esta em manutencao. Voltaremos em breve!
  DE: Der Shop ist in Wartung. Wir sind bald zuruck!

payment_success
  EN: Purchase successful!
  ES: Compra exitosa!
  FR: Achat reussi!
  PT: Compra bem-sucedida!
  DE: Kauf erfolgreich!

payment_retry
  EN: Retry
  ES: Reintentar
  FR: Reessayer
  PT: Tentar novamente
  DE: Erneut versuchen
```

---

## FASE 12: BootManager Integration <a id="fase-12"></a>

### Modificacion a `BootManager.cs`

Agregar un nuevo paso entre Paso 3 (Firebase) y Paso 4 (Game Managers):

```csharp
// NUEVO PASO 3.5: Inicializar sistema de pagos
yield return StartCoroutine(InitializePaymentSystem());
UpdateLoadingProgress(0.6f, "boot_loading_payments");

// -----

private IEnumerator InitializePaymentSystem()
{
    Debug.Log("[Boot] Inicializando sistema de pagos...");

    // 1. Crear RemoteConfigService (para feature flags)
    if (RemoteConfigService.Instance == null)
    {
        GameObject remoteConfigObj = new GameObject("RemoteConfigService");
        remoteConfigObj.AddComponent<RemoteConfigService>();
        Debug.Log("[Boot] RemoteConfigService creado");
    }

    // Esperar a que RemoteConfig haga su primer fetch (o use cache)
    float timeout = 5f;
    float elapsed = 0f;
    while (!RemoteConfigService.Instance.IsReady && elapsed < timeout)
    {
        elapsed += Time.deltaTime;
        yield return null;
    }

    // 2. Inicializar PaymentFeatureFlag con datos de Remote Config
    PaymentFeatureFlag.Initialize(RemoteConfigService.Instance.GetCurrentConfig());

    // 3. Crear PaymentManager
    if (PaymentManager.Instance == null)
    {
        GameObject paymentObj = new GameObject("PaymentManager");
        paymentObj.AddComponent<PaymentManager>();
        Debug.Log("[Boot] PaymentManager creado");
    }

    // 4. Crear EntitlementService
    if (EntitlementService.Instance == null)
    {
        GameObject entitlementObj = new GameObject("EntitlementService");
        entitlementObj.AddComponent<EntitlementService>();
        Debug.Log("[Boot] EntitlementService creado");
    }

#if DIGIT_PARK_PRO || UNITY_EDITOR
    // 5. Crear TriumphIsolationGuard (solo Pro)
    if (TriumphIsolationGuard.Instance == null)
    {
        GameObject guardObj = new GameObject("TriumphIsolationGuard");
        guardObj.AddComponent<TriumphIsolationGuard>();
        Debug.Log("[Boot] TriumphIsolationGuard creado");
    }
#endif

    // 6. Sync entitlements
    if (EntitlementService.Instance != null)
    {
        _ = EntitlementService.Instance.SyncWithServer();
        // Fire-and-forget, no bloquear el boot
    }

    Debug.Log("[Boot] Sistema de pagos inicializado correctamente");
}
```

### Nueva clave de traduccion para boot

```
boot_loading_payments
  EN: Loading payment services...
  ES: Cargando servicios de pago...
  FR: Chargement des services de paiement...
  PT: Carregando servicos de pagamento...
  DE: Zahlungsdienste werden geladen...
```

---

## FASE 13: Tests <a id="fase-13"></a>

### Archivo: `Tests/Payments/PaymentManagerTests.cs`

```csharp
// namespace DigitPark.Payments.Tests
// [TestFixture] public class PaymentManagerTests

// Tests a implementar:
//
// [Test] PurchaseCosmetic_WithStripeActive_UsesStripe()
//   → Mock IPaymentProvider para Stripe que retorna success
//   → Verificar que result.ProviderUsed == Stripe
//
// [Test] PurchaseCosmetic_StripeFailsOnce_FallsBackToIAP()
//   → Mock Stripe que retorna failure
//   → Mock IAP que retorna success
//   → Verificar result.Success && result.WasProviderSwitched
//
// [Test] PurchaseCosmetic_BothFail_ReturnsFinalError()
//   → Mock ambos que retornan failure
//   → Verificar !result.Success
//
// [Test] PurchaseCosmetic_PreventsConcurrentPurchases()
//   → Iniciar compra, intentar segunda antes de que termine
//   → Verificar que segunda retorna error "purchase_in_progress"
//
// [Test] PurchaseCosmetic_TriumphProductId_IsRejected()
//   → Intentar comprar un producto con "triumph" en el ID
//   → Verificar que falla con error de compliance
//
// [Test] PurchaseCosmetic_GlobalVersion_NeverUsesStripe()
//   → Setear AppVersion.Global
//   → Verificar que Stripe provider nunca se llama
```

### Archivo: `Tests/Payments/FeatureFlagTests.cs`

```csharp
// Tests:
// [Test] Initialize_WithRemoteConfig_SetsCorrectProvider()
// [Test] Initialize_WithNullConfig_DefaultsToAppleIAP()
// [Test] ForceSwitch_UpdatesActiveProvider()
// [Test] ForceSwitch_PersistsToLocalCache()
// [Test] LocalCache_SurvivesRestart() (PlayerPrefs mock)
```

### Archivo: `Tests/Payments/TriumphIsolationTests.cs`

```csharp
// Tests:
// [Test] NoTriumphNamespace_InStripeFiles()
//   → Usa reflection para verificar que ninguna clase en
//     DigitPark.Payments.Stripe referencia DigitPark.Services.Triumph
//
// [Test] NoStripeNamespace_InTriumphFiles()
//   → Inverso
//
// [Test] ProductCatalog_ContainsNoProhibitedTerms()
//   → Llama ProductCatalog.ValidateCatalogCompliance()
//
// [Test] PaymentResult_NeverHasTriumphTransactionId()
//   → Verifica que PaymentResult.TransactionId nunca empieza con "triumph_"
```

### Archivo: `Tests/Payments/StripeAppleIAPSwitchTests.cs`

```csharp
// Tests:
// [Test] Switch_StripeFails3Times_AutoSwitchesToIAP()
// [Test] Switch_AbortProtocol_CompletesUnder5Seconds()
// [Test] Switch_EntitlementsPreservedAfterSwitch()
//   → Grant entitlement via Stripe
//   → Switch to IAP
//   → Verify entitlement still exists
// [Test] Switch_UserSeesNoError_IfFallbackWorks()
```

### Archivo: `Tests/Payments/EntitlementServiceTests.cs`

```csharp
// Tests:
// [Test] Grant_CreatesLocalRecord()
// [Test] Grant_NeverHasTournamentBenefit()
// [Test] HasEntitlement_ReturnsTrueAfterGrant()
// [Test] Sync_MergesServerAndLocal()
```

### Archivo: `Tests/Payments/ComplianceGuardTests.cs`

```csharp
// Tests:
// [Test] ValidateProduct_CleanProduct_ReturnsTrue()
// [Test] ValidateProduct_WithTournamentInId_ReturnsFalse()
// [Test] ValidateProduct_WithCashInMetadata_ReturnsFalse()
// [Test] ValidateSessionMetadata_AlwaysHasCosmeticType()
```

---

## FASE 14: Monitoring & Alerts <a id="fase-14"></a>

### Backend endpoint: `/api/health/payments`

Retorna:
```json
{
  "stripe": {
    "status": "healthy",
    "last_successful_webhook": "2026-03-10T...",
    "error_rate_1h": 0.01,
    "sessions_last_hour": 42
  },
  "apple_iap": {
    "status": "healthy",
    "last_validation": "2026-03-10T...",
    "validation_success_rate": 0.99
  },
  "triumph": {
    "status": "isolated",
    "cross_contamination_detected": false,
    "last_isolation_check": "2026-03-10T..."
  },
  "active_provider": "stripe",
  "abort_protocol_ready": true,
  "app_version": "pro"
}
```

### Alertas automaticas (backend)

| Condicion | Severidad | Accion |
|---|---|---|
| Stripe error rate > 5% en 1h | WARNING | Email al developer |
| Stripe error rate > 15% en 1h | CRITICAL | Auto-trigger abort consideration |
| Triumph isolation breach detectado | CRITICAL | Alerta inmediata + shutdown |
| Apple IAP validation fail rate > 10% | WARNING | Email al developer |
| Feature flag fetch falla > 1 hora | WARNING | Email al developer |
| Stripe webhook sin eventos > 30 min | WARNING | Email al developer |

---

## FASE 15: Documentation Files <a id="fase-15"></a>

### `docs/PAYMENT_ARCHITECTURE.md`
- Diagrama ASCII de la arquitectura (3 sistemas)
- Flujo de datos para cada path de pago
- Garantias de aislamiento Triumph ↔ Stripe
- Explicacion de separacion de versiones

### `docs/STRIPE_COMPLIANCE.md`
- Como Digit Park Pro se presenta a Stripe:
  - "Mobile game selling cosmetic digital goods (skins, themes, virtual currency)"
  - "Zero gambling, no real-money prizes"
  - "Skill tournaments handled by separate legal entity (Triumph)"
- Palabras PROHIBIDAS en cualquier metadata de Stripe
- Explicacion de por que esta arquitectura NO viola Stripe ToS
- Audit trail documentation

### `docs/ABORT_RUNBOOK.md`
- Paso 1: Como flipear Remote Config en Firebase Console
- Paso 2: Como verificar productos Apple IAP en App Store Connect
- Paso 3: Como recuperar fondos retenidos de Stripe (timeline 90 dias)
- Paso 4: Como evaluar Paddle como reemplazo permanente
- Paso 5: Proceso de apelacion con Stripe
- Paso 6: Trigger manual (5-finger tap)

### `docs/DEVELOPER_ONBOARDING.md`
- Setup de environment variables
- Como testear Stripe en modo sandbox
- Como testear Apple IAP en sandbox
- Como simular abort protocol
- Como cambiar entre build Pro/Global

---

## PASOS MANUALES POST-IMPLEMENTACION <a id="pasos-manuales"></a>

Estos pasos NO se pueden automatizar con codigo. El developer debe hacerlos manualmente:

### A. Firebase Remote Config

1. **Instalar Firebase Remote Config SDK**
   - Unity Package Manager → agregar `com.google.firebase.remote-config`
   - O descargar desde Firebase Unity SDK releases
   - Agregar referencia en el .asmdef de Payments si aplica

2. **Configurar Remote Config en Firebase Console**
   - Ir a Firebase Console → Remote Config
   - Crear parametros:
     - `payment_provider` (String) = "stripe"
     - `stripe_enabled` (Boolean) = true
     - `apple_iap_enabled` (Boolean) = true
     - `triumph_enabled` (Boolean) = true
     - `cosmetic_store_enabled` (Boolean) = true
     - `app_version` (String) = "pro"
     - `maintenance_mode` (Boolean) = false
   - Publicar cambios

### B. Stripe Account Setup

1. **Crear cuenta Stripe**
   - Ir a stripe.com/register
   - Business type: "Software/SaaS"
   - Business description: "Mobile game selling cosmetic digital goods (character skins, themes, virtual currency). No gambling, no real-money prizes."
   - NUNCA mencionar: tournaments, skill gaming, cash prizes, Triumph

2. **Configurar productos en Stripe Dashboard**
   - Crear un Product para cada item en ProductCatalog
   - Obtener Price IDs (price_xxx) y agregarlos a CosmeticProduct.StripePriceId
   - Configurar Checkout settings

3. **Configurar webhooks en Stripe Dashboard**
   - Endpoint URL: https://api-pro.digitpark.com/api/stripe/webhook
   - Events: checkout.session.completed, payment_intent.payment_failed
   - Obtener Webhook Signing Secret (whsec_xxx)

4. **Obtener API Keys**
   - Dashboard → Developers → API Keys
   - Copiar Secret Key (sk_xxx) y Publishable Key (pk_xxx)
   - Guardar en .env del backend

### C. Apple IAP Setup

1. **App Store Connect**
   - Crear nueva app "Digit Park Pro" con bundle ID com.matrixsoftware.digitpark.pro
   - Crear In-App Purchases para cada producto del catalogo
   - Los IDs deben coincidir con `AppleProductId` en ProductCatalog
   - Configurar precios por region

2. **Shared Secret**
   - App Store Connect → App → In-App Purchases → App-Specific Shared Secret
   - Copiar y guardar en .env del backend como APPLE_SHARED_SECRET

3. **Sandbox Testing**
   - Crear Sandbox Tester accounts en App Store Connect
   - Configurar en dispositivo: Settings → App Store → Sandbox Account

### D. Backend Deployment (Firebase Cloud Functions)

El backend se implementa como Firebase Cloud Functions — no se necesita servidor dedicado.
Ver `docs/MANUAL_SETUP_INSTRUCTIONS.md` seccion D para instrucciones completas.

Pasos rapidos:
1. `npm install -g firebase-tools && firebase login`
2. `cd functions && npm install && npm run build`
3. Configurar secrets: `firebase functions:secrets:set STRIPE_SECRET_KEY` (y los demas)
4. `firebase deploy --only functions`
5. Actualizar URLs en Unity Inspector (PaymentConfig) y Stripe Dashboard (webhook URL)

### E. Unity Build Configuration

1. **Scripting Define Symbols**
   - Player Settings → Other Settings → Scripting Define Symbols
   - Para iOS build Pro: `DIGIT_PARK_PRO;HAS_TRIUMPH;HAS_STRIPE;HAS_APPLE_IAP`
   - Para iOS build Global: `DIGIT_PARK_GLOBAL;HAS_APPLE_IAP`

2. **Bundle Identifier**
   - Player Settings → Other Settings → Bundle Identifier
   - Pro: `com.matrixsoftware.digitpark.pro`
   - Global: `com.matrixsoftware.digitpark`

3. **Unity IAP Configuration**
   - Window → Unity IAP → Receipt Validation Obfuscator → Obfuscate
   - Esto genera Tangle files necesarios para validacion client-side

4. **App Transport Security (iOS)**
   - Si el backend usa HTTPS (obligatorio), no se necesita excepcion ATS
   - Si se usa HTTP para desarrollo: agregar excepcion en Info.plist

### F. Deep Link Configuration

1. **Universal Links (Stripe return)**
   - Configurar apple-app-site-association en el servidor Pro
   - Agregar path: `/stripe-return`
   - En Xcode: Capabilities → Associated Domains → applinks:api-pro.digitpark.com

2. **Custom URL Scheme**
   - Ya existe `digitpark://` en DeepLinkService
   - Agregar handler para `digitpark://stripe-return?session_id=xxx`

### G. Triumph SDK (Cuando este disponible)

1. **Instalar SDK**
   - Seguir documentacion de Triumph: docs.triumph.app
   - Importar .unitypackage o via Package Manager

2. **Implementar servicios**
   - Completar TriumphKYCService, TriumphWalletService, etc. en TriumphServices.cs
   - Reemplazar `throw NotImplementedException()` con llamadas reales al SDK

3. **Cambiar ServiceLocator a Production**
   - En el Inspector del GameObject ServiceLocator: cambiar ServiceMode a Production
   - Configurar API Key y Sandbox mode

### H. Testing Checklist Manual

1. **Stripe Sandbox**
   - [ ] Crear checkout session exitosamente
   - [ ] Completar pago con tarjeta de test (4242 4242 4242 4242)
   - [ ] Verificar webhook recibido en backend
   - [ ] Verificar entitlement granted en Firebase
   - [ ] Verificar gems/theme desbloqueado en juego

2. **Apple IAP Sandbox**
   - [ ] Comprar gem pack con sandbox tester
   - [ ] Verificar receipt validation contra backend
   - [ ] Restaurar compras non-consumable
   - [ ] Verificar entitlement sync

3. **Failsafe Switch**
   - [ ] Deshabilitar Stripe via Remote Config
   - [ ] Verificar que compras automaticamente usan IAP
   - [ ] Re-habilitar Stripe via Remote Config
   - [ ] Verificar que compras vuelven a usar Stripe

4. **Abort Protocol**
   - [ ] Simular 3 fallos de Stripe consecutivos
   - [ ] Verificar auto-switch a IAP
   - [ ] Verificar alerta recibida
   - [ ] Verificar que entitlements previos se mantienen

5. **Isolation**
   - [ ] Hacer compra cosmetica mientras CashBattle esta activo
   - [ ] Verificar que TriumphIsolationGuard no reporta contaminacion
   - [ ] Verificar audit log limpio

6. **Version Guard**
   - [ ] Build Global: verificar que Stripe/Triumph code no compila
   - [ ] Build Global: verificar que IAP funciona normalmente
   - [ ] Build Pro: verificar que todo funciona

---

## CHECKLIST DE VALIDACION FINAL <a id="checklist"></a>

### Compile-time checks
- [ ] Build Pro compila sin errores con: `DIGIT_PARK_PRO;HAS_TRIUMPH;HAS_STRIPE;HAS_APPLE_IAP`
- [ ] Build Global compila sin errores con: `DIGIT_PARK_GLOBAL;HAS_APPLE_IAP`
- [ ] Remover HAS_STRIPE no rompe el build Pro (solo desactiva Stripe)
- [ ] Remover HAS_TRIUMPH no rompe el build Pro (solo desactiva Triumph)
- [ ] Ningun archivo en Payments/Stripe/ importa DigitPark.Services.Triumph
- [ ] Ningun archivo en Services/Triumph/ importa DigitPark.Payments.Stripe

### Runtime checks
- [ ] TriumphIsolationGuard.VerifyIsolation() retorna true
- [ ] ProductCatalog.ValidateCatalogCompliance() retorna true
- [ ] PaymentManager no tiene referencia a ServiceLocator (CashBattle)
- [ ] ServiceLocator no tiene referencia a PaymentManager
- [ ] Stripe metadata nunca contiene "tournament", "prize", etc.

### Data flow checks
- [ ] Compra via Stripe → entitlement → gems/theme desbloqueado
- [ ] Compra via IAP → entitlement → gems/theme desbloqueado
- [ ] Stripe falla → fallback IAP → mismo resultado para usuario
- [ ] Ambos fallan → error amigable mostrado
- [ ] Abort protocol → switch completo < 5 segundos

### Architecture checks
- [ ] TriumphManager es independiente (borrar PaymentManager no lo rompe)
- [ ] PaymentManager es independiente (borrar TriumphManager no lo rompe)
- [ ] Backend rechaza requests de version Global a endpoints de Stripe
- [ ] Backend rechaza requests con metadata de tournament en Stripe routes

---

## ORDEN DE IMPLEMENTACION RECOMENDADO

Para Sonnet, implementar en este orden estricto:

```
1.  PaymentEvents.cs + AbortReason.cs    (enums y eventos, sin dependencias)
2.  PaymentResult.cs                      (modelo de datos, sin dependencias)
3.  PaymentConfig.cs                      (modelo de config, sin dependencias)
4.  CosmeticProduct + ProductCatalog.cs   (catalogo, sin dependencias)
5.  IPaymentProvider.cs                   (interfaz, depende de 2,4)
6.  LocalFlagCache.cs                     (cache PlayerPrefs, sin dependencias)
7.  PaymentFeatureFlag.cs                 (flags, depende de 1,6)
8.  RemoteConfigService.cs                (Remote Config, depende de 6,7)
9.  EntitlementRecord.cs                  (modelo, sin dependencias)
10. EntitlementService.cs                 (servicio, depende de 9)
11. StripeComplianceGuard.cs              (validacion, depende de 4)
12. StripeSessionPoller.cs                (polling HTTP, depende de 2)
13. StripeCheckoutController.cs           (Safari VC, depende de 12)
14. StripePaymentProvider.cs              (provider, depende de 5,11,12,13)
15. AppleReceiptValidator.cs              (validacion, depende de 2)
16. AppleIAPProvider.cs                   (provider, depende de 5,15)
17. PaymentManager.cs                     (orquestador, depende de 5,7,14,16)
18. StripeAbortProtocol.cs                (abort, depende de 7,17)
19. TriumphIsolationGuard.cs              (isolation, depende de 1,17)
20. VersionGuard.cs                       (version check, depende de 7)
21. PaymentLoadingOverlay.cs              (UI, depende de 1)
22. PaymentErrorDialog.cs                 (UI, depende de 1)
23. Modificar BootManager.cs              (integracion, depende de 8,10,17,19)
24. Modificar PremiumManager.cs           (bridge, depende de 16)
25. Modificar ShopManager.cs              (routing, depende de 17)
26. BuildProfileSwitcher.cs               (Editor tool)
27. PaymentDebugWindow.cs                 (Editor tool)
28. DigitPark.Payments.Runtime.asmdef     (assembly definition)
29. StoreKitBridge.mm                     (placeholder nativo)
30. Agregar traducciones a Translations.txt
31. Todos los tests
32. Backend completo (package.json → schema.sql)
33. Documentation files
```

---

## NOTAS FINALES PARA SONNET

1. **NO tocar archivos .yaml, .prefab, .unity, .asset** — El usuario lo prohibe explicitamente.

2. **Seguir el patron singleton exacto** del proyecto (ver seccion 1.4).

3. **Namespace convention**: Usar `DigitPark.Payments` como base para todo el codigo nuevo.

4. **Comentarios en espanol** para documentacion interna (XML doc), igual que el resto del proyecto.

5. **Log tags**: Usar `[PaymentManager]`, `[StripeProvider]`, `[AppleIAP]`, `[FeatureFlag]`, `[Isolation]`, `[Abort]` para consistency.

6. **Sync Translations.txt**: Despues de agregar claves, copiar `Resources/Translations.txt` a `Localization/Translations.txt`.

7. **UnityWebRequest** para HTTP calls (NO System.Net.HttpClient — no funciona bien en iOS).

8. **async/await** con UniTask si esta disponible, si no, con Task<> normal + .ConfigureAwait(false).

9. **PlayerPrefs keys** prefijados con `dp_` para evitar colisiones.

10. **El backend es un proyecto SEPARADO** en `Backend/` — no es codigo Unity.

11. **StoreKitBridge.mm es PLACEHOLDER** en V1 — Unity IAP 5.1.2 cubre todo lo necesario.

12. **Firebase Remote Config puede no estar instalado** — RemoteConfigService debe funcionar SOLO con LocalFlagCache si el package no existe. Usar `#if FIREBASE_REMOTE_CONFIG` para condicionar.
