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

## BLOQUE 5 — Post-Simplificación Cleanup ✅ COMPLETADO

### ~~C-S13~~ ✅ AppleReceiptValidator.cs — Fix compile error (VersionGuard → Application.version)
### ~~C-S14~~ ✅ PaymentConfig.cs — Campos Stripe muertos eliminados
### ~~C-S15~~ ✅ PremiumManager.cs — Descripciones actualizadas (frames/titles/effects)
### ~~C-S16~~ ✅ ShopItemData.cs — ShopItemType.Theme y Avatar eliminados del enum
### ~~C-S17~~ ✅ AllScenesBatchBuilder.cs — Entradas AgeVerification y CashTournamentResults eliminadas
### ~~C-S18~~ ✅ BuildScenesConfigurator.cs — Dialog string actualizado (Cash Battle eliminado)
### ~~C-S19~~ ✅ GameSessionManager.cs — Comentario stale eliminado

*Actualizado 2026-03-24 — Bloque 2 marcado completado (todos los archivos confirmados modificados en git)*
*Actualizado 2026-03-24 — Bloque 5 añadido: 7 tareas de cleanup post-simplificación confirmadas contra código real*
*Actualizado 2026-03-24 — Bloque 5 completado: C-S13→C-S19 ejecutados y commiteados en V56*
