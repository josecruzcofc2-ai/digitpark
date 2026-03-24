# DIGITPARK — SIMPLIFICATION AUDIT
## Arquitectura Mínima Viable para Lanzamiento
**Fecha:** 2026-03-24 | **Branch:** master | **Estado:** Pre-commit (archivos ya eliminados del disco)

---

## RESUMEN EJECUTIVO

La simplificación está **casi completada a nivel de archivos**. La mayoría del código de CashBattle, Stripe, Triumph, Themes y Services CashBattle-only ya fue borrado del disco. Lo que queda pendiente:

1. **Commitear** las ~300 eliminaciones del working tree
2. **Corregir 1 compile error** (`AppleReceiptValidator.cs` → referencia a `VersionGuard` eliminado)
3. **Limpiar dead code residual** en ~8 archivos que sobrevivieron con referencias obsoletas
4. **Redefinir PremiumManager** sin el sistema de temas

---

## BLOQUE A — Payments

### Archivos Stripe/Compliance/FeatureFlags/Abort — TODOS ELIMINADOS del disco:
| Carpeta | Archivos eliminados |
|---------|---------------------|
| `Payments/Stripe/` | StripePaymentProvider, StripeCheckoutController, StripeSessionPoller, StripeComplianceGuard |
| `Payments/Abort/` | StripeAbortProtocol |
| `Payments/Compliance/` | TriumphIsolationGuard, **VersionGuard** |
| `Payments/FeatureFlags/` | RemoteConfigService, LocalFlagCache, PaymentFeatureFlag |
| `Payments/Core/` | AbortReason |

### Archivos Apple IAP que SOBREVIVEN (todos limpios):
| Archivo | Propósito | Estado |
|---------|-----------|--------|
| `PaymentManager.cs` | Orquestador. Solo inicializa `AppleIAPProvider`. `GetActiveProvider()` siempre retorna `AppleIAP` | ✅ Limpio |
| `PaymentEvents.cs` | Bus de eventos. Enum `PaymentProvider {AppleIAP, None}` — Stripe ya eliminado del enum | ✅ Limpio |
| `PaymentResult.cs` | POCO resultado de pago. Neutral | ✅ Limpio |
| `AppleReceiptValidator.cs` | Valida receipts via Cloud Function `iapValidateReceipt`. Fail-closed | ⚠️ **Compile error** (ver Issues) |
| `EntitlementService.cs` | Source of truth de ownership. Bloquea explícitamente Triumph como provider | ✅ Limpio |
| `PaymentErrorDialog.cs` | UI popup para errores de pago | ✅ Limpio |
| `PaymentBridge.cs` | Bridge de delegates entre assemblies | ✅ Limpio |
| `AppleIAPProvider.cs` | Wrapper sobre PremiumManager via delegates. Server-validates receipt | ✅ Limpio |
| `PaymentConfig.cs` | ScriptableObject de config | ⚠️ 7 campos Stripe muertos |
| `IPaymentProvider.cs` | Interfaz de provider | ✅ Limpio |
| `ProductCatalog.cs` | Catálogo de productos IAP | ✅ Limpio |
| `EntitlementRecord.cs` | POCO de entitlement | ✅ Limpio |
| `PaymentLoadingOverlay.cs` | UI overlay durante pagos | ✅ Limpio |

### Respuesta a preguntas clave del prompt:
- **¿Qué es Apple IAP vs Stripe/Triumph?** Apple IAP = los 13 archivos de arriba. Stripe/Triumph = todo eliminado.
- **¿Se puede mantener SOLO Apple IAP?** Sí — ya está así.
- **¿Cuánto código de compliance/guard existe solo por Stripe?** Compliance/ y FeatureFlags/ completos (5 archivos) — eliminados.
- **¿EntitlementService funciona con solo Apple IAP?** Sí, fue diseñado para esto. Bloquea Triumph activamente.

---

## BLOQUE B — CashBattle

**Todos los archivos en `Runtime/Features/CashBattle/` han sido eliminados del disco.**

### Referencias a CashBattle desde fuera de CashBattle:
| Archivo | Línea | Tipo de referencia |
|---------|-------|--------------------|
| `GameSessionManager.cs` | ~610 | Solo comentario: `"(Practice y Online, no CashBattle)"` — sin dependencia real |
| `SceneNames.cs` | — | Cero constantes de CashBattle |
| `SceneNavigator.cs` | — | Cero referencias |
| `BootManager.cs` | — | Cero referencias |
| `MainMenuManager.cs` | — | Cero referencias |
| `GameMode.cs` | — | Enum limpio: `Practice, SingleGame, CognitiveSprint, Tournament, Online` |
| `ResultPanelManager.cs` | — | Cero referencias |

### Respuesta a preguntas clave del prompt:
- **¿CashBattle tiene código compartido con el core?** No. Era completamente aislado.
- **¿La eliminación afecta GameSessionManager o MinigameBase?** No.
- **¿GameModes tiene un modo CashBattle?** No. El enum es limpio.
- **Acoplamiento con el core:** NINGUNO.

---

## BLOQUE C — Services

**Todos los archivos CashBattle-specific eliminados del disco:**
| Carpeta | Archivos eliminados |
|---------|---------------------|
| `Services/` | ServiceLocator.cs, LocationRestrictionService.cs |
| `Services/Interfaces/` | IKYCService, IWalletService, IMatchmakingService, ITournamentService |
| `Services/Mock/` | MockKYCService, MockWalletService, MockMatchmakingService, MockTournamentService |
| `Services/Triumph/` | TriumphManager, TriumphServices |

### Servicios core que sobreviven:
- `AchievementService.cs` — sin referencias a CashBattle ✅
- `RotatingContentService.cs` — sin referencias a CashBattle ✅
- `Firebase/DatabaseService.cs`, `AnalyticsService.cs`, `NotificationService.cs` — core ✅

### Respuesta a preguntas clave del prompt:
- **¿ServiceLocator es solo para CashBattle?** Sí — era exclusivo. Eliminado.
- **¿Los Mocks tienen valor en producción?** No. Eliminados.
- **¿LocationRestrictionService tiene función fuera de CashBattle?** No. Eliminado.
- **ServiceLocator usage fuera de CashBattle:** CERO — grep confirmado.

---

## BLOQUE D — Themes

**29 Theme ScriptableObjects eliminados. 7 Runtime scripts eliminados. 2 UI Components eliminados.**

### Assets eliminados:
```
Theme_Arctic, Theme_Aurora, Theme_Bioluminescence, Theme_BloodMoon, Theme_CoralSurge,
Theme_CyberFuchsia, Theme_DeepOcean, Theme_ElectricBlue, Theme_ElectricOrange,
Theme_Emerald, Theme_Glitch, Theme_IceFire, Theme_Infrared, Theme_Matrix,
Theme_Monochrome, Theme_Nebula, Theme_NeonDark, Theme_Outrun, Theme_Phantom,
Theme_PlasmaIndigo, Theme_Sakura, Theme_Synthwave, Theme_Thunder, Theme_Titanium,
Theme_ToxicLime, Theme_Ultraviolet, Theme_Vaporwave, Theme_Void, Theme_Volcanic, Theme_Y2KChrome
```
**Total: 29 temas (incluyendo NeonDark)**

### Scripts Runtime eliminados:
`ThemeManager, ThemeData, ThemeInitializer, ThemeApplier, ThemeSelector, NeonThemeColors, CashThemeForcer`
`ThemeDropdownController (UI), ThemeSelector (UI Component)`

### Referencias sobrevivientes — todas son COMENTARIOS de código, no imports:
- `BackgroundPatternReceiver.cs` — comentario "NO usa ThemeApplier"
- `Card3DEffect.cs` — comentario "override any ThemeApplier changes"
- `GameCardEffect.cs` — comentario "previene que ThemeApplier..."
- `OddOneOutController.cs` — comentario "ThemeApplier text color isn't clobbered"
- Múltiples UIBuilders — comentarios sobre ThemeApplier (inofensivos)

**Sin dependencias de compile-time al sistema de temas.**

### Respuesta a preguntas clave del prompt:
- **¿Cuántos Theme*.asset existen?** 29 — todos eliminados.
- **¿NeonDark es uno de los temas existentes?** Sí, existía como `Theme_NeonDark.asset` — ahora eliminado junto con el sistema. Los colores NeonDark están hardcoded en los UIBuilders.
- **¿Qué hace ThemeApplier?** Modificaba colores/sprites de GameObjects en runtime según el tema activo. Eliminado.
- **¿ThemeDropdownController vive en Settings?** Sí, vivía en Settings — eliminado. Settings ya no tiene selector de tema.
- **¿Si se hardcodea NeonDark qué código queda inutilizable?** Todo el sistema ya fue eliminado. Los UIBuilders ya usan colores hardcoded.

---

## BLOQUE E — SceneNames y Navegación

### Todas las constantes en `SceneNames.cs` (estado actual):
| Constante | Categoría |
|-----------|-----------|
| Boot | Core |
| MainMenu | Core |
| Settings | Core |
| Login | Auth |
| Register | Auth |
| GameSelector | Games |
| DigitRush | Games |
| FlashTap | Games |
| MemoryPairs | Games |
| OddOneOut | Games |
| QuickMath | Games |
| GameResults | Games |
| Matchmaking | Games/Nav |
| BetSelection | Games/Nav |
| PlayModeSelection | Games/Nav |
| Tournaments | Tournaments |
| TournamentLobby | Tournaments |
| Profile | Social |
| Friends | Social |
| SearchPlayers | Social |
| Leaderboard | Social |
| Notifications | Social |
| Shop | Monetization |

**Cero constantes de CashBattle (escenas 32-40). Cero AgeVerification. Cero CashBattleOnboarding.**

### BootManager — servicios que inicializa:
SafeAreaManager, NetworkService, NetworkStatusBanner, ReviewService, DeepLinkService, BackButtonManager, LocalizationManager, AuthenticationService, DatabaseService, AnalyticsService, AchievementService, PaymentBridgeWiring, AppleIAPBridge, PaymentManager, EntitlementService, BattleCardService, DailyOfferService, WelcomePackService, RotatingContentService, GDPR consent popup.

**Sin inicialización de CashBattle/Triumph/Stripe en Boot.** El comentario en línea 474 dice "Stripe + AppleIAP + FeatureFlags + Entitlements" — es stale text, el código real solo inicializa Apple IAP + Entitlements.

### EditorBootConfig.cs:
Sin flags de CashBattle.

---

## BLOQUE F — Auth / AgeVerification

`AgeVerificationManager.cs` — ELIMINADO del disco.
`AgeVerification.unity` — ELIMINADA del disco.
`AgeVerificationUIBuilder.cs` — ELIMINADO del disco.

**Respuesta a pregunta clave:** AgeVerification existía exclusivamente por CashBattle/gambling compliance. **No es requerido por App Store** para ninguna feature que se mantiene (IAP estándar, tournaments free-to-play, etc.).

---

## BLOQUE G — Monetización

### ShopItemType enum — todos los valores existentes:
| Tipo | Estado sin CashBattle/Themes |
|------|------------------------------|
| `DigitGemsPack` | ✅ Mantener — IAP core |
| `DigitCoinsPack` | ✅ Mantener |
| `Theme` | ⛔ `GrantRewards()` es empty break — sistema eliminado |
| `Avatar` | ⛔ DEPRECATED, no-op con warning |
| `Frame` | ✅ Mantener |
| `Title` | ✅ Mantener |
| `SpecialOffer` | ✅ Mantener |
| `PremiumBundle` | ⚠️ Descriptions mencionan "15 premium themes" |
| `StarterPack` | ✅ Mantener |
| `WinEffect` | ✅ Mantener |
| `WinEffectBundle` | ✅ Mantener |
| `BattleCard` | ✅ Mantener |
| `BackgroundPattern` | ✅ Mantener |
| `TemporaryDecoration` | ✅ Mantener |

### Achievements — referencias a CashBattle:
**NINGUNA.** `AchievementsManager.cs` es completamente limpio. Los iconos Logro_Bolsa_100, Logro_Ficha_Cash, etc. son arte temático pero las definiciones no requieren CashBattle gameplay.

### MissionsManager — referencias a CashBattle: NINGUNA. ✅

### PlayerProgressionSystem — referencias a CashBattle: NINGUNA. ✅

### PremiumManager — qué define "Premium":
- `_canCreateTournaments` — crear torneos custom ✅ válido
- `_hasStylesPro` — themes desbloqueados ⚠️ **dangling flag — sistema eliminado**
- `IsPremiumPass` — `premium_pass_monthly` entitlement ✅
- `IsAdFree` — `adfree_permanent` entitlement ✅

### Products IAP existentes:
- Non-consumables: `createtournaments`, `tournamentbundle`, `premium_bundle`, `complete_bundle`
- Consumables: `gems_100`, `gems_300`, `gems_500`, `gems_1200`, `gems_2500`, `gems_6500`, `gems_14000`

### Respuesta a preguntas clave del prompt:
- **¿Qué compra el usuario en el Shop?** Gems (IAP), Frames, Titles, WinEffects, BattleCards, BackgroundPatterns, Premium.
- **Si se elimina Themes, ¿qué queda vendible?** Todo lo anterior — suficiente catálogo.
- **¿El sistema de Premium tiene sentido sin themes?** Sí: tournaments + ad-free son valor real.
- **¿Achievements de CashBattle son ≥20% del total?** No — cero achievements de CashBattle.

---

## BLOQUE H — Editor Tools

### CashBattle-only Editor files — TODOS ELIMINADOS:
| Carpeta | Archivos eliminados | Count |
|---------|---------------------|-------|
| `Editor/CashBattle/` | 10 UIBuilders (Hub, Wallet, History, Profile, Matchmaking, Tournaments, Results, 1v1, etc.) | 10 |
| `Editor/AutoAssigners/CashBattle/` | 8 AutoAssigners | 8 |
| `Editor/Themes/` | CashBattleGoldPreview, ThemeEditorPreview | 2 |
| `Editor/Payments/` | BuildProfileSwitcher, PaymentDebugWindow | 2 |
| `Editor/Onboarding/` | CashBattleOnboardingUIBuilder, CashBattleOnboardingMenuItems | 2 |
| `Editor/Games/` | CashThemePreview | 1 |
| `Editor/Tools/` | CashBattleScenesBatchBuilder, ChromaticThemeConfigurator, ThemeApplierHelper/Protector/Setup | 5 |
| `Editor/Auth/` | AgeVerificationUIBuilder | 1 |
| `Editor/AutoAssigners/Auth/` | AgeVerificationIconAssigner, AgeVerificationReferenceAssigner | 2 |
| `Editor/WinPanels/` | CashBattleResultPanelUIBuilder | 1 |
| **TOTAL** | | **34** |

### AllScenesBatchBuilder.cs — sección CashBattle:
- La sección `// ── CASH BATTLE ──` existe pero está vacía (sin entries)
- Línea 133: referencia a `CashTournamentResultsUIBuilder` (eliminado) — falla silenciosa via reflection
- Línea 74: referencia a `AgeVerification.unity` + `AgeVerificationUIBuilder` (eliminados) — falla silenciosa

### BuildScenesConfigurator.cs:
- `SCENE_ORDER` array: 30 entries, cero escenas CashBattle
- Línea 284: string del dialog menciona `[30-39] Cash Battle` — stale, sin impacto funcional

---

## BLOQUE I — DevTools

**`PremiumDebugController.cs` — ELIMINADO del disco.**

**`DebugManager.cs`** — Sobrevive. Sin referencias a CashBattle.

**`AchievementDebugPanel.cs`** — Sobrevive. Solo dev tool (Editor/debug builds).

---

## BLOQUE J — Backend Functions

### Todos los endpoints HTTP en `index.ts` (estado actual):
| Endpoint | Auth | Categoría |
|----------|------|-----------|
| `iapValidateReceipt` | Firebase ID Token + userId match | Apple IAP / Core |
| `getEntitlements` | Firebase ID Token + userId match | Core |
| `checkEntitlement` | Firebase ID Token + userId match | Core |
| `syncEntitlements` | Firebase ID Token + whitelist `apple_iap` only | Core |
| `paymentsHealth` | Ninguna (open) | Health check |

**Stripe completamente removido.** Los endpoints `stripeCreateCheckout`, `stripeSessionStatus`, `stripeWebhook`, `adminForceSwitch` ya no existen.

`environment.ts` solo contiene: `APPLE_SHARED_SECRET`, `SLACK_WEBHOOK_URL`, `APP_REGION`, `APPLE_PRODUCTION_URL`, `APPLE_SANDBOX_URL` — sin Stripe keys.

### Respuesta a preguntas clave del prompt:
- **¿Cuántos endpoints quedan sin Stripe/Triumph?** 5 — todos core.
- **¿Son suficientes para Apple IAP nativo?** Sí. `iapValidateReceipt` + entitlements es el stack completo.
- **¿Apple IAP necesita backend de validación?** Técnicamente no (StoreKit directo), pero tenerlo es mejor práctica anti-fraude. El backend actual es correcto y mínimo.

---

## BLOQUE K — Onboarding

`CashBattleOnboardingManager.cs` — ELIMINADO.
`CashBattleOnboarding.unity` — ELIMINADA.

**`OnboardingManager.cs`** — Sobrevive. Sin referencias a CashBattle. Slides son del core del juego.

---

## INVENTARIO DE ISSUES PENDIENTES

### 🔴 P0 — Compile Error (bloquea build)

| # | Archivo | Línea | Issue | Fix |
|---|---------|-------|-------|-----|
| 1 | `AppleReceiptValidator.cs` | ~76 | `Compliance.VersionGuard.GetRequiredAppVersionHeader()` — VersionGuard.cs fue eliminado | Reemplazar con `Application.version` |

### 🟡 P1 — Dead Code / Logic Broken

| # | Archivo | Issue |
|---|---------|-------|
| 2 | `PaymentConfig.cs` | 7 campos Stripe muertos: `stripePublishableKey`, `stripeCreateCheckoutUrl`, `stripeSessionStatusUrl`, `stripeWebhookUrl`, `adminForceSwitchUrl`, `stripeTimeoutSeconds`, `stripeMaxRetries` |
| 3 | `PremiumManager.cs` | `_hasStylesPro` es dangling boolean. Product descriptions mencionan "15 premium themes" y "19 themes" |
| 4 | `ShopItemData.cs` | `ShopItemType.Theme` en `GrantRewards()` es empty break — compra no hace nada |
| 5 | `BootManager.cs` | Comentario línea ~474 dice "Stripe + AppleIAP + FeatureFlags" — stale |

### 🟢 P2 — Stale References / Silent Failures

| # | Archivo | Issue |
|---|---------|-------|
| 6 | `AllScenesBatchBuilder.cs` | Línea 133: referencia a `CashTournamentResultsUIBuilder` (eliminado) — falla silenciosa via reflection |
| 7 | `AllScenesBatchBuilder.cs` | Línea 74: referencia a `AgeVerification.unity` + `AgeVerificationUIBuilder` (eliminados) |
| 8 | `BuildScenesConfigurator.cs` | Línea ~284: dialog string menciona `[30-39] Cash Battle` |
| 9 | `GameSessionManager.cs` | Línea ~610: comentario `"(Practice y Online, no CashBattle)"` — ya no aplica |

---

## ARQUITECTURA OBJETIVO — DigitPark v1.0 Mínima

```
Auth → Boot → MainMenu
               │
               ├── Games (5 minijuegos)
               │     ├── GameSelector → PlayModeSelection → BetSelection
               │     └── Matchmaking → GameSession → ResultPanel
               │
               ├── Progresión
               │     ├── XP + Niveles (PlayerProgressionSystem)
               │     └── Daily Missions (MissionsManager)
               │
               ├── Achievements (52 icons, Firebase-persisted)
               │
               ├── Leaderboard (Firebase Realtime DB)
               │
               ├── Social
               │     ├── Friends + FriendRequests
               │     ├── SearchPlayers
               │     └── Notifications (FCM)
               │
               ├── Tournaments (Free-to-play, gems entry fee)
               │
               ├── Shop (Apple IAP únicamente)
               │     ├── Gem Packs (consumable IAP)
               │     ├── Frames, Titles, WinEffects, BattleCards
               │     ├── BackgroundPatterns
               │     └── Premium (tournaments + ad-free)
               │
               └── Settings (idioma / audio / vibración — sin selector de tema)

Tema visual: NeonDark hardcoded (colores fijos en UIBuilders)
Pagos: Apple StoreKit via Unity IAP (SKPaymentQueue)
Validación: Firebase Cloud Function `iapValidateReceipt`
Backend: Firebase Auth + Realtime DB + Cloud Functions (5 endpoints core)
```

---

## ESTIMACIÓN DE REDUCCIÓN

| Categoría | Archivos .cs eliminados |
|-----------|------------------------|
| `Runtime/Features/CashBattle/` | ~30 |
| `Runtime/Payments/Stripe + Compliance + FeatureFlags + Abort` | ~10 |
| `Runtime/Services/Interfaces + Mock + Triumph + ServiceLocator` | ~10 |
| `Runtime/Themes/` + `UI/Components/Theme*` | ~9 |
| `Editor/CashBattle/` + `Editor/AutoAssigners/CashBattle/` | ~18 |
| `Editor/Themes/ + Tools/Theme* + Games/CashTheme*` | ~8 |
| `Editor/Payments/ + Onboarding/CB* + Auth/AV* + WinPanels/CB*` | ~8 |
| `Runtime/Features/Auth/AgeVerification` + `Onboarding/CB*` + `DevTools/PremiumDebug` | ~3 |
| **TOTAL** | **~96 .cs eliminados** de ~320 totales (~30%) |

**Escenas eliminadas:** ~10 de ~40 (~25%)

**Recursos eliminados:** 29 Theme assets, ~15 iconos CashBattle Hub/UI, 2 Win Panel prefabs CashBattle, 4 prefabs CashBattle

**Riesgo de regresión en core: BAJO** — CashBattle era completamente aislado del gameplay core.

---

## ORDEN DE EJECUCIÓN RECOMENDADO

### Paso 1 — Commitear eliminaciones actuales
```bash
git add -A
git commit -m "Chore: remove CashBattle, Stripe, Triumph, Themes and related dead code"
```

### Paso 2 — Fix P0 compile error (1 línea)
`AppleReceiptValidator.cs` ~línea 76:
Reemplazar `Compliance.VersionGuard.GetRequiredAppVersionHeader()` → `Application.version`

### Paso 3 — Limpiar PaymentConfig.cs (7 campos Stripe muertos)

### Paso 4 — Actualizar PremiumManager
- Cambiar descriptions de producto que mencionan "premium themes"
- Decidir qué hace `_hasStylesPro` ahora (renombrar o repropósito)
- `ShopItemType.Theme`: eliminar o marcar DEPRECATED

### Paso 5 — Limpiar AllScenesBatchBuilder.cs
Eliminar entries de `CashTournamentResultsUIBuilder` y `AgeVerification.unity`

### Paso 6 — Limpiar BuildScenesConfigurator.cs
Actualizar string del dialog eliminando referencia a CashBattle

---

## DECISIONES PENDIENTES

| # | Decisión | Opciones |
|---|----------|----------|
| 1 | ¿Qué hace `_hasStylesPro` sin themes? | A) Eliminar bool. B) Repropósito como "StylesPro = Frames+Titles+Effects bundle" |
| 2 | ¿`ShopItemType.Theme` se elimina o queda como DEPRECATED? | A) Eliminar + cleanup. B) Dejar con comment |
| 3 | ¿Los achievement icons "Cash-themed" se reemplazan? | A) Reemplazar. B) Dejarlos — son arte válido temáticamente |
| 4 | ¿NeonDark colors están ya todos hardcoded en UIBuilders? | Confirmar que no hay ningún GO que dependa de ThemeData en runtime |

---

*Documento generado: 2026-03-24 — Análisis basado en lectura real de todos los archivos descritos en el prompt (Bloques A-K)*
