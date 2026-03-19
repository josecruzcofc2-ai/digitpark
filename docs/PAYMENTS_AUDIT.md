# Digit Park Pro — Auditoría del Sistema de Pagos

> **Fecha**: 2026-03-19
> **Auditor**: Claude Sonnet 4.6
> **Scope**: Todos los archivos en `Runtime/Payments/` + `docs/payments/`
> **Método**: Comparación exhaustiva código real vs documentación existente

---

## Índice

1. [BUG-01 — AbortProtocol no notifica al backend](#bug-01)
2. [BUG-02 — 9 productos Frame sin StripePriceId](#bug-02)
3. [BUG-03 — PaymentBridgeWiring.cs ausente](#bug-03)
4. [BUG-04 — Double-charge risk en timeout de Stripe](#bug-04)
5. [BUG-05 — Server-side receipt validation es un stub vacío](#bug-05)
6. [BUG-06 — EntitlementService.SyncWithServer() es un stub](#bug-06)
7. [DISC-01 — "100 Sparks" en docs vs "150 Sparks" en código](#disc-01)
8. [DISC-02 — ValidateCatalogCompliance() falta "triumph"](#disc-02)
9. [DISC-03 — PaymentConfig no es ScriptableObject](#disc-03)
10. [WARN-01 — Deep link handler nunca se desregistra](#warn-01)
11. [WARN-02 — ForceSwitch(None) guarda "apple_iap" en cache](#warn-02)
12. [WARN-03 — AsIEnumerator() puede no existir en todas las versiones de Firebase SDK](#warn-03)
13. [Tabla Resumen de Estado Real](#resumen)
14. [Prioridades de Reparación](#prioridades)

---

## BUG-01 — AbortProtocol no notifica al backend {#bug-01}

**Severidad**: 🔴 Crítico
**Archivo**: `Assets/_Project/Scripts/Runtime/Payments/Abort/StripeAbortProtocol.cs` — línea 88
**Tipo**: Lógica rota — funcionalidad documentada que no ocurre

### Descripción

El Paso 2 del Abort Protocol (notificar al backend cuando se ejecuta un abort) está completamente roto. El código lee la URL desde `PlayerPrefs`:

```csharp
// CÓDIGO ACTUAL (roto)
backendUrl = PlayerPrefs.GetString("dp_backend_url", "");
if (string.IsNullOrEmpty(backendUrl)) return;   // ← SIEMPRE ocurre
string url = $"{backendUrl}/api/admin/force-switch";
```

**Dos problemas simultáneos:**

1. `"dp_backend_url"` **nunca se escribe en ningún lugar del proyecto**. Siempre es string vacío → el método retorna silenciosamente sin hacer nada.
2. El path hardcodeado `/api/admin/force-switch` no coincide con el nombre real de la Firebase Cloud Function (`/adminForceSwitch`).

Mientras tanto, `PaymentConfig` tiene el campo correcto `adminForceSwitchUrl` que ya se configura en el Inspector pero **nunca es usado** por `NotifyBackend()`.

### Impacto

- El switch local (Paso 1) funciona correctamente: Stripe se desactiva en el dispositivo.
- El backend **jamás se entera** del abort. Firebase Remote Config no se actualiza automáticamente.
- El resto de los dispositivos no reciben el abort hasta el siguiente poll de Remote Config (máximo 15 minutos), y solo si alguien lo actualizó manualmente en Firebase Console.
- El runbook de emergencia (ABORT_RUNBOOK.md) describe el Paso 2 como funcional cuando no lo es.

### Solución Propuesta

En `StripeAbortProtocol.cs`, modificar `NotifyBackend()` para:
1. Leer la URL desde `PaymentManager.Instance._config.adminForceSwitchUrl` (o exponer el config vía una propiedad pública en PaymentManager).
2. Usar esa URL directamente, sin construir un path manual.

```csharp
// SOLUCIÓN
private static async Task NotifyBackend(AbortReason reason)
{
    var config = PaymentManager.Instance?._config;  // exponer como public o internal
    if (config == null || string.IsNullOrEmpty(config.adminForceSwitchUrl)) return;

    string url = config.adminForceSwitchUrl;  // usar el campo correcto
    // ... resto del método sin cambios
}
```

---

## BUG-02 — 9 productos Frame sin StripePriceId → crash en backend {#bug-02}

**Severidad**: 🔴 Crítico
**Archivo**: `Assets/_Project/Scripts/Runtime/Payments/Core/ProductCatalog.cs` — líneas 127–198
**Tipo**: Datos incompletos que causan error de red

### Descripción

El `ProductCatalog` tiene **17 productos** (no 8 como dicen todos los documentos):

| Grupo | Cantidad | StripePriceId | En docs/setup |
|---|---|---|---|
| Sparks (100–14000) | 6 | ❌ Sin asignar | ✅ Documentados |
| Bundles (premium, complete) | 2 | ❌ Sin asignar | ✅ Documentados |
| **Frames** (plasma_spark, prism_shift, aurora_borealis, void_walker, storm_surge, cosmic_rift, infernal_god, divine_light, quantum_break) | **9** | ❌ Sin asignar | ❌ **No documentados** |

Los 9 frames tienen `StripePriceId` a `null`. En `StripePaymentProvider.CreateCheckoutSession()`:

```csharp
priceId = product.StripePriceId ?? "",   // → "" para todos los frames
```

Enviar `priceId: ""` al backend de Stripe resulta en un error **400 Bad Request** del backend → `CreateCheckoutSession` retorna `null` → `PaymentResult.Failed("session_creation_failed")`.

Además, los 9 frames tampoco están registrados en:
- Stripe Dashboard (sin productos creados)
- App Store Connect (sin In-App Purchases creados)
- MANUAL_SETUP_INSTRUCTIONS.md (no mencionados)
- ABORT_RUNBOOK.md (lista de 8 IAP para verificar no incluye frames)

### Impacto

Cualquier intento de comprar un frame via Stripe falla silenciosamente con error de backend. El fallback a Apple IAP también fallaría porque el `AppleProductId` no está registrado en App Store Connect.

### Solución Propuesta

**Opción A (recomendada si los frames son reales y se van a vender):**
1. Crear los 9 productos en Stripe Dashboard y App Store Connect.
2. Copiar los `price_xxx` generados a cada `CosmeticProduct.StripePriceId` en `ProductCatalog.cs`.
3. Actualizar MANUAL_SETUP_INSTRUCTIONS.md con los 9 frames adicionales.

**Opción B (si los frames son WIP y no están listos para venta):**
1. Mover los 9 frames a un array separado `FrameProducts` o marcarlos con una propiedad `IsAvailable = false`.
2. Excluirlos del catálogo activo hasta que tengan `StripePriceId` válido.
3. Añadir validación en `ProductCatalog.ValidateCatalogCompliance()` que rechace productos sin `StripePriceId` si Stripe es el provider activo.

---

## BUG-03 — PaymentBridgeWiring.cs ausente → todos los pagos bloqueados {#bug-03}

**Severidad**: 🔴 Crítico
**Archivo esperado**: `Assets/_Project/Scripts/Runtime/PaymentBridgeWiring.cs` (no existe)
**Tipo**: Archivo faltante — funcionalidad central no conectada

### Descripción

`PaymentBridge.cs` define delegates estáticos con valores por defecto seguros:

```csharp
public static Func<string> GetCurrentUserId = () => "anonymous";  // default
public static Action<int, int> ProcessGemsPurchase = null;
public static Action<string, double, string, string> LogPurchaseCompleted = null;
```

El archivo `PaymentBridgeWiring.cs` (en Assembly-CSharp, fuera del assembly de Payments) debería inyectar las implementaciones reales apuntando a `AuthenticationService`, `CurrencyManager` y `AnalyticsService`. **Este archivo no existe.**

**Consecuencia directa en `PaymentManager.PurchaseCosmetic()`:**

```csharp
string userId = GetCurrentUserId();  // → "anonymous" siempre
if (userId == "anonymous")
    return PaymentResult.Failed(..., "user_not_authenticated", ...);  // ← SIEMPRE
```

**100% de las compras fallan** antes de llegar a Stripe o Apple IAP, para cualquier usuario, en cualquier build.

### Impacto

- Ninguna compra puede completarse.
- Incluso si una compra completara, `ProcessGemsPurchase = null` → los gems no se añaden al wallet.
- `LogPurchaseCompleted = null` → los eventos de compra no se loguean en Firebase Analytics.

### Solución Propuesta

Crear `Assets/_Project/Scripts/Runtime/PaymentBridgeWiring.cs` en Assembly-CSharp (NO en el assembly de Payments):

```csharp
using UnityEngine;
using DigitPark.Payments;

namespace DigitPark
{
    public class PaymentBridgeWiring : MonoBehaviour
    {
        private void Awake()
        {
            // Auth
            PaymentBridge.GetCurrentUserId = () =>
                Services.Firebase.AuthenticationService.Instance?.UserId ?? "anonymous";

            // Currency
            PaymentBridge.ProcessGemsPurchase = (gems, bonus) =>
                Managers.CurrencyManager.Instance?.AddGems(gems + bonus);

            // Analytics
            PaymentBridge.LogPurchaseCompleted = (productId, amount, currency, txId) =>
                Services.Firebase.AnalyticsService.Instance?.LogPurchaseCompleted(productId, amount, currency, txId);

            PaymentBridge.LogCustomEvent = (name, parameters) =>
                Services.Firebase.AnalyticsService.Instance?.LogCustomEvent(name, parameters);

            // Firebase Database
            PaymentBridge.UpdatePlayerFields = (fields) =>
                Services.Firebase.DatabaseService.Instance?.UpdatePlayerFields(fields);

            // Deep Link
            PaymentBridge.RegisterDeepLinkHandler = (scheme, handler) =>
                Services.DeepLinkService.Instance?.RegisterHandler(scheme, handler);

            Debug.Log("[PaymentBridgeWiring] Delegates inyectados correctamente");
        }
    }
}
```

Luego añadir este componente al GameObject `PaymentManager` en la escena Boot (o a un GameObject separado que se inicialice antes que PaymentManager.Start()).

---

## BUG-04 — Double-charge risk en timeout de Stripe {#bug-04}

**Severidad**: 🟠 Grave
**Archivo**: `Assets/_Project/Scripts/Runtime/Payments/Core/PaymentManager.cs` — líneas 139–156
**Tipo**: Race condition — el usuario puede ser cobrado dos veces

### Descripción

Si el usuario paga en Stripe pero el polling de `StripeSessionPoller` agota el timeout (5 minutos por defecto), la sesión se marca como `StripeSessionStatus.Expired` → `PaymentResult.Failed`. Luego en `PurchaseCosmetic()`:

```csharp
if (!result.Success)
{
    _stripeFailureCount++;
    // ...
    // Fallback a Apple IAP
    if (_iapProvider != null && _iapProvider.IsAvailable)
    {
        result = await _iapProvider.PurchaseProduct(product, userId);  // ← segundo cobro
        usedFallback = true;
    }
}
```

**El usuario ya pagó en Stripe + se le cobra otra vez por Apple IAP.**

El fallback tiene sentido para fallas de red donde Stripe nunca procesó el pago, pero no para casos donde el pago completó en Stripe y solo fue la confirmación lo que falló.

### Impacto

- Cobro duplicado al usuario.
- Riesgo de chargeback y disputa con Stripe.
- Desde el punto de vista de Stripe, esto sería "entrega doble de un bien ya pagado", pero el usuario tiene razón en disputar.

### Solución Propuesta

Distinguir entre "Stripe rechazó el pago" (fallback válido) y "Stripe timeout o expiró" (fallback NO válido):

```csharp
// Solo hacer fallback si el fallo fue ANTES de que el usuario pagara
// (no en timeout/expiry, donde el pago pudo haber ocurrido)
bool shouldFallback = !result.Success
    && result.ErrorCode != "session_expired"
    && result.ErrorCode != "stripe_timeout";

if (shouldFallback && _iapProvider != null && _iapProvider.IsAvailable)
{
    result = await _iapProvider.PurchaseProduct(product, userId);
    usedFallback = true;
}
else if (result.ErrorCode == "session_expired")
{
    // Mostrar mensaje: "Tu pago puede estar procesándose. Revisa tu correo de Stripe."
    // NO cobrar por Apple IAP
}
```

---

## BUG-05 — Server-side receipt validation de Apple IAP es un stub vacío {#bug-05}

**Severidad**: 🟠 Grave
**Archivo**: `Assets/_Project/Scripts/Runtime/Payments/AppleIAP/AppleIAPProvider.cs` — líneas 78–81 y 143–150
**Tipo**: Seguridad — validación documentada que no ocurre

### Descripción

`AppleReceiptValidator` se instancia en `Initialize()` pero `ValidateReceiptAsync()` no hace nada:

```csharp
private async Task ValidateReceiptAsync(string productId, string userId, string transactionId)
{
    Debug.Log($"[AppleIAP] Validando receipt server-side para: {productId}");
    // El receiptData real viene de Unity IAP vía PremiumManager
    // Por ahora log de intención
    await Task.Yield();   // ← no hace nada
}
```

Nunca llama a `_receiptValidator.ValidateReceipt()`. El receipt data de Unity IAP (en `PremiumManager`) nunca llega a este método porque `InvokePurchase` solo propaga `(success, transactionId)`, no el receipt en sí.

La documentación dice: _"La validacion del receipt se hace server-side via la Cloud Function `iapValidateReceipt`"_ — esto **no ocurre**.

### Impacto

- La validación ocurre solo client-side (Unity IAP).
- Un usuario con el dispositivo jailbreakeado podría realizar una compra fraudulenta que Unity IAP aprobaría sin validación server-side.
- Los docs de compliance y MANUAL_SETUP_INSTRUCTIONS.md describen este flujo como funcional.

### Solución Propuesta

1. Modificar el delegate `InvokePurchase` para que también propague el receipt data:
   ```csharp
   // En AppleIAPProvider.cs
   public static Action<string, Action<bool, string, string>> InvokePurchase = null;
   //                                              ↑ bool=success, string=txId, string=receiptData
   ```

2. En `PurchaseProduct()`, pasar el receipt a `ValidateReceiptAsync()`:
   ```csharp
   InvokePurchase(appleProductId, (success, transactionId, receiptData) =>
   {
       if (success)
       {
           ValidateReceiptAsync(appleProductId, userId, transactionId, receiptData)
               .ContinueWith(...);
           tcs.SetResult(...);
       }
   });
   ```

3. Implementar `ValidateReceiptAsync()` con la llamada real a `_receiptValidator.ValidateReceipt()`.

---

## BUG-06 — EntitlementService.SyncWithServer() es un stub {#bug-06}

**Severidad**: 🟠 Grave
**Archivo**: `Assets/_Project/Scripts/Runtime/Payments/Entitlements/EntitlementService.cs` — líneas 140–150
**Tipo**: Funcionalidad faltante — restore de compras incompleto

### Descripción

```csharp
public async Task SyncWithServer()
{
    // En produccion: GET {syncEntitlementsUrl}?userId={_currentUserId}
    // Por ahora solo recarga desde local
    LoadFromLocal();
    await Task.Yield();
    if (this == null) return;
    Debug.Log("[EntitlementService] Sync completado");
}
```

Nunca llama a `PaymentConfig.getEntitlementsUrl` ni `syncEntitlementsUrl`. Los entitlements solo persisten en `PlayerPrefs` + Firebase Realtime DB (vía `SyncToFirebase()` en `Grant()`). Si el usuario desinstala y reinstala la app, los entitlements de Stripe se pierden porque `PlayerPrefs` se borra.

### Impacto

- Compras de Stripe no se restauran tras reinstalación (solo Apple IAP Non-Consumables son restaurables por Apple).
- Si el usuario cambia de dispositivo, pierde todas las compras de Stripe.
- El restore de themes cosméticos comprados via Stripe está fundamentalmente roto.

### Solución Propuesta

Implementar `SyncWithServer()` real que llame al endpoint `getEntitlements`:

```csharp
public async Task SyncWithServer()
{
    var config = PaymentManager.Instance?._config;
    if (config == null || string.IsNullOrEmpty(config.getEntitlementsUrl)) return;

    string userId = PaymentBridge.GetCurrentUserId();
    string url = $"{config.getEntitlementsUrl}?userId={userId}";

    using (var request = UnityWebRequest.Get(url))
    {
        request.SetRequestHeader("Authorization", $"Bearer {await GetFirebaseIdToken()}");
        request.timeout = 10;
        var op = request.SendWebRequest();
        while (!op.isDone) await Task.Yield();

        if (request.result == UnityWebRequest.Result.Success)
        {
            // Parsear respuesta y merge con entitlements locales
            // Actualizar _localEntitlements y SaveToLocal()
        }
    }
}
```

---

## DISC-01 — "100 Sparks" en docs vs "150 Sparks" en código {#disc-01}

**Severidad**: 🟡 Menor — pero riesgo de rechazo de App Store
**Archivo**: `Assets/_Project/Scripts/Runtime/Payments/Core/ProductCatalog.cs` — línea 53
**Tipo**: Inconsistencia docs ↔ código

### Descripción

```csharp
DisplayName = "150 Sparks",  // E-EC01: 100→150 DG, cubre exactamente 1 Tier B (150 DG)
```

Todos los documentos de setup (MANUAL_SETUP_INSTRUCTIONS.md, ABORT_RUNBOOK.md) instruyen crear el producto como **"100 Sparks"** tanto en Stripe Dashboard como en App Store Connect. Si el developer sigue las instrucciones, habrá un mismatch visible:

- **En Stripe / App Store**: "100 Sparks" ($0.99)
- **En la app**: "150 Sparks" ($0.99)

Apple puede rechazar la app durante revisión por inconsistencia entre el nombre en App Store Connect y el mostrado en la interfaz. Stripe también puede marcar discrepancias entre el nombre del producto y lo que ve el usuario.

### Solución Propuesta

Actualizar MANUAL_SETUP_INSTRUCTIONS.md y ABORT_RUNBOOK.md para reflejar el nombre real "150 Sparks" en todos los pasos de configuración de Stripe y App Store. O cambiar el `DisplayName` de vuelta a "100 Sparks" si es que la economía cambió y los documentos no se actualizaron.

---

## DISC-02 — ValidateCatalogCompliance() falta "triumph" {#disc-02}

**Severidad**: 🟡 Menor — inconsistencia de seguridad
**Archivo**: `Assets/_Project/Scripts/Runtime/Payments/Core/ProductCatalog.cs` — línea 225
**Tipo**: Inconsistencia entre dos listas de términos prohibidos

### Descripción

Dos listas de términos prohibidos en dos clases distintas están desincronizadas:

| Clase | Lista de términos |
|---|---|
| `StripeComplianceGuard.ProhibitedTerms` | tournament, prize, cash_game, skill_game, real_money, entry_fee, wager, bet, gambling, **triumph** |
| `ProductCatalog.ValidateCatalogCompliance()` | tournament, prize, cash_game, skill_game, real_money, entry_fee, wager, bet, gambling *(falta "triumph")* |

`TriumphIsolationGuard.VerifyIsolation()` llama a `ProductCatalog.ValidateCatalogCompliance()`. Si alguien añade un producto con "triumph" en su ID o nombre, esta validación no lo detectaría — solo lo detectaría `StripeComplianceGuard` cuando se intente usar Stripe.

### Solución Propuesta

Añadir `"triumph"` a la lista de `prohibitedTerms` dentro de `ProductCatalog.ValidateCatalogCompliance()` para sincronizarla con `StripeComplianceGuard.ProhibitedTerms`. Mejor aún: unificar ambas listas en una constante compartida.

---

## DISC-03 — PaymentConfig no es un ScriptableObject {#disc-03}

**Severidad**: 🟡 Menor
**Archivo**: `Assets/_Project/Scripts/Runtime/Payments/Core/PaymentConfig.cs` y `Core/PaymentManager.cs` línea 69
**Tipo**: Inconsistencia entre mensaje de error y realidad

### Descripción

`PaymentManager.cs` muestra este error en consola cuando la config está vacía:
```
"PaymentManager] PaymentConfig no está asignada en Inspector o está vacía.
Asigna el ScriptableObject en el Inspector antes de hacer builds."
```

Pero `PaymentConfig` está marcada como `[System.Serializable]` (clase embebida en el MonoBehaviour), **no como `ScriptableObject`**. No existe ningún `.asset` generado. El mensaje de error es engañoso para el developer.

`TAREAS_MANUALES.md` lista "PaymentConfig ScriptableObject" como tarea P0 pendiente, lo que sugiere que la intención original era hacerla ScriptableObject pero nunca se implementó.

### Solución Propuesta

**Opción A**: Convertir `PaymentConfig` en `ScriptableObject` (más limpio, compartible entre escenas):
```csharp
[CreateAssetMenu(menuName = "DigitPark/PaymentConfig")]
public class PaymentConfig : ScriptableObject { ... }
```
Luego crear el `.asset` desde el menú y asignarlo en el Inspector.

**Opción B (mínima)**: Corregir el mensaje de error para que no diga "ScriptableObject":
```csharp
"PaymentConfig no está configurada en el Inspector. Completa todos los campos de URL."
```

---

## WARN-01 — Deep link handler nunca se desregistra {#warn-01}

**Severidad**: ⚠️ Warning
**Archivo**: `Assets/_Project/Scripts/Runtime/Payments/Stripe/StripeCheckoutController.cs` — línea 22
**Tipo**: Posible leak de handler

### Descripción

```csharp
PaymentBridge.RegisterDeepLinkHandler?.Invoke("stripe-return", OnStripeReturn);
```

No existe `UnregisterDeepLinkHandler` en ningún lugar. Los handlers se acumulan en `DeepLinkService` tras cada compra. Aunque `_isPurchaseInProgress` previene compras concurrentes, en sesiones largas pueden acumularse handlers de compras pasadas.

### Solución Propuesta

Añadir `UnregisterDeepLinkHandler` a `PaymentBridge` y llamarlo en `StripeCheckoutController` cuando la compra complete o cancele.

---

## WARN-02 — ForceSwitch(None) guarda "apple_iap" en cache {#warn-02}

**Severidad**: ⚠️ Warning
**Archivo**: `Assets/_Project/Scripts/Runtime/Payments/FeatureFlags/PaymentFeatureFlag.cs` — línea 90
**Tipo**: Estado inconsistente en cache

### Descripción

```csharp
LocalFlagCache.SaveProviderOverride(
    provider == PaymentProvider.Stripe ? "stripe" : "apple_iap"  // None → "apple_iap"
);
```

Cuando el Abort Protocol detecta que **ambos** providers están caídos (`ForceSwitch(PaymentProvider.None, "both_providers_down")`), el cache local guarda "apple_iap". Al relanzar la app se intentará usar Apple IAP aunque en la sesión anterior se determinó que estaba caído.

### Solución Propuesta

```csharp
string providerStr = provider switch {
    PaymentProvider.Stripe   => "stripe",
    PaymentProvider.AppleIAP => "apple_iap",
    _                        => "none"
};
LocalFlagCache.SaveProviderOverride(providerStr);
```

Y en `LocalFlagCache.Load()`, manejar `"none"` → `PaymentProvider.None`.

---

## WARN-03 — AsIEnumerator() puede no existir en todas las versiones de Firebase SDK {#warn-03}

**Severidad**: ⚠️ Warning — solo en compile-time con FIREBASE_REMOTE_CONFIG activo
**Archivo**: `Assets/_Project/Scripts/Runtime/Payments/FeatureFlags/RemoteConfigService.cs` — línea 91

### Descripción

```csharp
yield return Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance
    .ActivateAsync().AsIEnumerator();   // ← extensión específica del SDK
```

`.AsIEnumerator()` es un método de extensión del Firebase Unity SDK que no existe en todas las versiones. Si la versión instalada no lo tiene, falla en compile-time (solo visible cuando `FIREBASE_REMOTE_CONFIG` está activo en Scripting Defines).

### Solución Propuesta

Reemplazar con el patrón manual que funciona en todas las versiones del Firebase SDK:

```csharp
var activateTask = Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.ActivateAsync();
while (!activateTask.IsCompleted)
    yield return null;
```

---

## Tabla Resumen de Estado Real {#resumen}

| Componente | Estado Real | Estado según Docs |
|---|---|---|
| Abort — switch local a AppleIAP | ✅ Funcional | ✅ Documentado |
| Abort — notificación al backend | ❌ Roto (URL nunca leída) | ✅ Documentado como funcional |
| Compras Sparks/Bundles via Stripe | ⚠️ Funcional solo si StripePriceId se llena | ✅ Documentado |
| Compras Frames via Stripe | ❌ Falla (StripePriceId = null) | ❌ No documentados |
| Apple IAP — validación server-side | ❌ Stub vacío | ✅ Documentado como funcional |
| Entitlement sync con servidor | ❌ Stub — solo local | ✅ Documentado como funcional |
| PaymentBridge wiring | ❌ Archivo ausente | ✅ Asumido como existente |
| Fallback Stripe → AppleIAP | ✅ Funcional (pero con riesgo double-charge) | ✅ Documentado |
| Remote Config polling | ✅ Funcional (requiere define FIREBASE_REMOTE_CONFIG) | ✅ Documentado |
| StripeComplianceGuard | ✅ Funcional | ✅ Documentado |
| ProductCatalog compliance (falta "triumph") | ⚠️ Parcial | ✅ Documentado como completo |
| VersionGuard Pro/Global | ✅ Funcional | ✅ Documentado |

---

## Prioridades de Reparación {#prioridades}

| # | Severidad | Archivo a modificar | Acción |
|---|---|---|---|
| 1 | 🔴 Crítico | `Abort/StripeAbortProtocol.cs` | Reemplazar `PlayerPrefs("dp_backend_url")` con `PaymentManager.Instance._config.adminForceSwitchUrl` |
| 2 | 🔴 Crítico | Nuevo archivo `PaymentBridgeWiring.cs` | Crear en Assembly-CSharp e inyectar todos los delegates de Auth, Currency, Analytics, DeepLink |
| 3 | 🔴 Crítico | `Core/ProductCatalog.cs` + docs setup | Añadir `StripePriceId` y `AppleProductId` a los 9 frames O marcarlos como no disponibles + actualizar todos los docs de setup |
| 4 | 🟠 Grave | `Core/PaymentManager.cs` | No hacer fallback a Apple IAP si el error de Stripe fue `session_expired` o timeout |
| 5 | 🟠 Grave | `AppleIAP/AppleIAPProvider.cs` | Propagar receipt data a `ValidateReceiptAsync()` y llamar `_receiptValidator.ValidateReceipt()` |
| 6 | 🟠 Grave | `Entitlements/EntitlementService.cs` | Implementar `SyncWithServer()` real con llamada a `getEntitlementsUrl` |
| 7 | 🟡 Menor | `Core/ProductCatalog.cs` | Añadir `"triumph"` a `prohibitedTerms` en `ValidateCatalogCompliance()` |
| 8 | 🟡 Menor | `docs/payments/MANUAL_SETUP_INSTRUCTIONS.md` + `ABORT_RUNBOOK.md` | Actualizar "100 Sparks" → "150 Sparks" en todos los pasos de configuración |
| 9 | 🟡 Menor | `Core/PaymentConfig.cs` o `Core/PaymentManager.cs` | Convertir a ScriptableObject O corregir mensaje de error engañoso |
| 10 | ⚠️ Warning | `Stripe/StripeCheckoutController.cs` + `Core/PaymentBridge.cs` | Añadir `UnregisterDeepLinkHandler` y llamarlo al completar/cancelar compra |
| 11 | ⚠️ Warning | `FeatureFlags/PaymentFeatureFlag.cs` | Manejar `PaymentProvider.None` en `LocalFlagCache.SaveProviderOverride()` |
| 12 | ⚠️ Warning | `FeatureFlags/RemoteConfigService.cs` | Reemplazar `.AsIEnumerator()` con patrón `while (!task.IsCompleted) yield return null` |
