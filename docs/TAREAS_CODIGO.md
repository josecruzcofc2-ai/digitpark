# TAREAS AUTOMATIZABLES POR CÓDIGO
**Ultima actualizacion**: 2026-03-23 (Simplificacion — eliminado CashBattle, Stripe, Triumph, Temas)

Estas tareas tienen implementacion directa en codigo C# o TypeScript.

---

## BLOQUE 1 — Cloud Functions ✅ COMPLETADO (simplificado)
> Deployar con: `cd functions && npm install && firebase deploy --only functions`
> Endpoints activos: `iapValidateReceipt`, `getEntitlements`, `checkEntitlement`, `syncEntitlements`, `paymentsHealth`

### ~~C-01~~ ✅ Server-Side Score Validation
### ~~C-02~~ ✅ Rate Limiting + Anti-Replay
### ~~C-03~~ ✅ Server-Side Time Validation
### ~~C-04~~ ✅ Grant Validation (lado servidor)
### ~~C-10~~ ✅ GDPR Data Export

---

## BLOQUE 2 — Scripts modificados post-simplificacion ✅ COMPLETADO

### ~~C-S01~~ ✅ BootManager.cs — ThemeManager, ServiceLocator CashBattle, LocationRestrictionService, StripeAbortProtocol eliminados
### ~~C-S02~~ ✅ SceneNavigator.cs + SceneNames.cs — Constantes CashBattle eliminadas (incluyendo AgeVerification)
### ~~C-S03~~ ✅ MainMenuManager.cs — Boton CashBattle eliminado
### ~~C-S04~~ ✅ ServiceLocator.cs — Eliminado completo (ya no hay interfaces CashBattle)
### ~~C-S05~~ ✅ PaymentManager.cs — Solo Apple/Google IAP; Stripe, Triumph y FeatureFlags eliminados
### ~~C-S06~~ ✅ DatabaseService.cs — Queries CashBattle eliminadas
### ~~C-S07~~ ✅ AnalyticsService.cs — Eventos CashBattle eliminados
### ~~C-S08~~ ✅ AutoLocalizer.cs — Entradas Cash* del TextNameToKeyMap eliminadas
### ~~C-S09~~ ✅ Translations.txt — Claves cash_* y ach_cash_* eliminadas (×3 archivos sincronizados)
### ~~C-S10~~ ✅ SettingsManager.cs + SettingsUIBuilder.cs — Dropdown de temas eliminado
### ~~C-S11~~ ✅ OnboardingManager.cs — Slide CashBattle eliminado
### ~~C-S12~~ ✅ ShopManager.cs + ShopPremiumUIBuilder.cs — Items de temas eliminados

---

## BLOQUE 3 — Con prerequisitos

### C-05. IAP Localized Prices
- Reemplazar precios hardcodeados en USD con `product.metadata.localizedPriceString` de Unity IAP
- **ESPERAR**: Tarea manual #6 (IAP products en tiendas) completada

### C-06. Ad-Free guard en wrapper de ads
- Añadir guard: `if (PremiumManager.Instance?.IsAdFree == true) return;`
- **ESPERAR**: SDK de ads instalado (tarea manual #13)

---

## BLOQUE 4 — Completado ✅

### ~~C-07~~ ✅ DailyOfferService Seed
### ~~C-08~~ ✅ RotatingContentService — Catalogo Firebase
### ~~C-09~~ ✅ GDPR Right-to-Delete
### ~~C-14~~ ✅ PaymentManager URLs editor script (obsoleto — Stripe eliminado)
### ~~C-15~~ ✅ Tag "FrameLayer"
### ~~C-16~~ ✅ Economy Rebalance DC prices
### ~~C-17~~ ✅ Tier B themes prices (obsoleto — temas eliminados)

---

## BLOQUE 5 — Post-Simplificación Cleanup (PENDIENTE)

### C-S13. AppleReceiptValidator.cs — Fix compile error P0
- **Archivo**: `Scripts/Runtime/Payments/AppleIAP/AppleReceiptValidator.cs` línea 76
- **Issue**: `Compliance.VersionGuard.GetRequiredAppVersionHeader()` — VersionGuard.cs fue eliminado, esto rompe la compilación
- **Fix**: Reemplazar por `Application.version`
- **Prioridad**: P0 — bloquea build

### C-S14. PaymentConfig.cs — Eliminar campos Stripe muertos
- **Archivo**: `Scripts/Runtime/Payments/Core/PaymentConfig.cs`
- **Issue**: 7 campos muertos: `stripePublishableKey`, `stripeCreateCheckoutUrl`, `stripeSessionStatusUrl`, `stripeWebhookUrl`, `stripeCheckoutTimeoutSeconds`, `stripePollingIntervalMs`, `maxStripeRetries` + header y comment de Stripe
- **Fix**: Eliminar esos campos y sus headers `[Header("Stripe...")]`
- **Prioridad**: P1

### C-S15. PremiumManager.cs — Actualizar descripciones y limpiar `_hasStylesPro`
- **Archivo**: `Scripts/Runtime/Payments/Core/PaymentConfig.cs`
- **Issue**: `PremiumProduct.PremiumBundle` dice "15 premium themes", `CompleteBundle` dice "19 themes", `StylesPro` dice "Unlock all premium themes" — sistema de temas eliminado
- **Fix**: Actualizar descriptions a lo que realmente desbloquean. `_hasStylesPro` se mantiene como "StylesPro = Frames + Titles + WinEffects bundle" (ver Tarea Manual #16)
- **ESPERAR**: Tarea manual #16 (decisión sobre `_hasStylesPro`)
- **Prioridad**: P1

### C-S16. ShopItemData.cs — Cleanup `ShopItemType.Theme`
- **Archivo**: `Scripts/Runtime/Features/Monetization/Shop/ShopItemData.cs`
- **Issue**: `case ShopItemType.Theme:` en `GrantRewards()` es empty break — el tema no se otorga a nadie. `ShopItemType.Avatar` también es DEPRECATED no-op
- **Fix**: Marcar `Theme` y `Avatar` como `[Obsolete]` o eliminarlos del enum tras confirmar que no hay items del catálogo activos de ese tipo
- **ESPERAR**: Tarea manual #16 (decisión sobre cleanup de Theme enum)
- **Prioridad**: P1

### C-S17. AllScenesBatchBuilder.cs — Eliminar entradas stale
- **Archivo**: `Scripts/Editor/Tools/AllScenesBatchBuilder.cs`
- **Issue**: Línea 74 referencia `AgeVerification.unity` + `AgeVerificationUIBuilder` (ambos eliminados). Línea 133 referencia `CashTournamentResultsUIBuilder` (eliminado)
- **Fix**: Eliminar ambas entradas `E(...)` de esas líneas
- **Prioridad**: P2

### C-S18. BuildScenesConfigurator.cs — Actualizar dialog string
- **Archivo**: `Scripts/Editor/Core/BuildScenesConfigurator.cs` línea 284
- **Issue**: String del dialog menciona `"[30-39] Cash Battle"` — esas escenas ya no existen
- **Fix**: Eliminar esa línea del string del dialog
- **Prioridad**: P2

### C-S19. GameSessionManager.cs — Eliminar comentario stale
- **Archivo**: `Scripts/Runtime/Features/Games/Core/GameSessionManager.cs` línea ~610
- **Issue**: Comentario `"(Practice y Online, no CashBattle)"` — CashBattle ya no existe
- **Fix**: Actualizar comentario
- **Prioridad**: P2

*Actualizado 2026-03-24 — Bloque 2 marcado completado (todos los archivos confirmados modificados en git)*
*Actualizado 2026-03-24 — Bloque 5 añadido: 7 tareas de cleanup post-simplificación confirmadas contra código real*
