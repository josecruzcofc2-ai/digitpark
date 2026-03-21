# TAREAS AUTOMATIZABLES POR CÓDIGO
**Ultima actualizacion**: 2026-03-21

Estas tareas tienen implementacion directa en codigo C# o TypeScript.
Pide a Claude que las implemente **bloque por bloque** en el orden indicado.

---

## BLOQUE 1 — Cloud Functions ✅ COMPLETADO
> Deployar con: `cd functions && npm install && firebase deploy --only functions`

### ~~C-01~~ ✅ Server-Side Score Validation
- Endpoints `validateScore` + `submitCashScore` en `index.ts`
- Limites por juego: minTime + maxScore. Rate limit RTDB (30s). HMAC token anti-replay.

### ~~C-02~~ ✅ Rate Limiting + Anti-Replay
- Implementado dentro de `validateScore` y `submitCashScore` (RTDB atomic transaction).

### ~~C-03~~ ✅ Server-Side Time Validation
- `ServerTimeHelper.cs` usa `.info/serverTimeOffset` de Firebase RTDB (enfoque correcto, sin Cloud Function extra)
- Wired en `BootManager` (RefreshOffsetAsync al boot)
- `DailyMissionsManager` migrado: eliminado `_serverTimeOffsetMs` duplicado, usa `ServerTimeHelper.UtcNow`
- `RotatingContentService` migrado: 2 usos de `DateTime.UtcNow` → `ServerTimeHelper.UtcNow`
- `DailyRewardsManager`, `WelcomePackService`, `DailyOfferService` ya usaban ServerTimeHelper ✅

### ~~C-04~~ ✅ Grant Validation (lado servidor)
- Endpoint `validateDailyGrant` en `index.ts`: check atomico + rate limit por usuario/item/dia
- Real money ya cubierto por `iapValidateReceipt` + `stripeWebhook` ✅
- Unity (DailyOfferService, RotatingContentService): llamar `validateDailyGrant` antes de aplicar grant local (pendiente wiring Unity — C-04b)

### ~~C-10~~ ✅ GDPR Data Export
- Endpoint `exportUserData` en `index.ts`: exporta perfil, historial, logros, torneos, entitlements como JSON
- Unity: boton "Exportar mis datos" en Settings → pendiente wiring (C-10b)

---

## BLOQUE 2 — Editor Scripts ✅ COMPLETADO
> Archivo: `Assets/_Project/Scripts/Editor/Tools/DigitParkSetupTools.cs`
> Compilar en Unity Editor y ejecutar desde menu DigitPark > Setup > ...

### ~~C-14~~ ✅ PaymentManager URLs — auto-rellenar
- Menu: `DigitPark > Setup > Fill PaymentManager URLs`
- Abre Boot.unity primero. Rellena las 8 Cloud Function URLs via SerializedObject.
- PENDIENTE MANUAL: stripePublishableKey (pk_live_xxx) + IapProductIds

### ~~C-15~~ ✅ Tag "FrameLayer" — agregar automaticamente
- Menu: `DigitPark > Setup > Add FrameLayer Tag`
- Usa SerializedObject de TagManager. Idempotente (no duplica si ya existe).

### ~~C-16~~ ✅ Economy Rebalance — batch DC prices
- Menu: `DigitPark > Setup > Apply Economy Rebalance (DC Prices)`
- Itera Resources/Shop/, actualiza coinsPrice en Frames/Titles/WinEffects con priceType=DigitCoins.

### ~~C-17~~ ✅ Tier B themes — verificar/fijar priceType
- Menu: `DigitPark > Setup > Fix Tier B Theme Prices`
- Itera ShopItemData de tipo Theme, corrige priceType→DigitGems y gemsPrice→350.

---

## BLOQUE 3 — Unity C# sin prerequisitos ✅ COMPLETADO (todo ya estaba hecho)

### ~~C-09~~ ✅ GDPR Right-to-Delete
- `AuthenticationService.DeleteAccountAsync()` ya borra en cascada: Auth, leaderboards, match history, achievements, friends, notifications, tournaments.
- Cloud Function `deleteUserData` actua como backup server-side. Completo.

### ~~C-11~~ ✅ N/A — Loot Box Odds Disclosure
- `PlayerProgressionSystem` no tiene recompensas aleatorias. XP y niveles son deterministas. Sin loot boxes activos en el codebase.

### ~~C-13~~ TODOs — estado real
- `MainMenuManager.cs:420,433` ✅ — SceneNavigator.NavigateTo("Profile") y NavigateTo("SearchPlayers") ya implementados
- `DeepLinkService.cs:198` ✅ — PlayerPrefs.SetString("DP_ViewProfileId") + NavigateTo ya implementado
- `LeaderboardEntryUI.cs:65` ✅ — LoadAvatarAsync via UnityWebRequest ya implementado
- `ShopItemData.cs` avatar ✅ — Sistema avatar marcado DEPRECATED en el enum, no aplica
- `LocationRestrictionService.cs:83` → movido a `docs/Triumph.md` T-00a
- `DigitRushController.cs:1173` → movido a `docs/Triumph.md` T-00b

---

## BLOQUE 4 — Con prerequisitos (esperar tareas manuales)

### C-05. IAP Localized Prices
- Reemplazar precios hardcodeados en USD con `product.metadata.localizedPriceString` de Unity IAP
- **Archivos**: `PremiumManager.cs`, `WelcomePackService.cs`, `ShopPremiumUIBuilder.cs`, `ShopEffectsTabBuilder.cs`
- **Por que**: Usuarios fuera de USA ven precios en dolares cuando la tienda cobra en su moneda local
- **ESPERAR**: Tareas manuales #7 (PaymentConfig) y #8 (IAP products en tiendas) completadas

### C-06. Ad-Free guard en wrapper de ads
- Añadir guard al inicio del wrapper/helper que muestre anuncios:
  ```csharp
  if (PremiumManager.Instance?.IsAdFree == true) return;
  ```
- `PremiumManager.IsAdFree` ya existe y funciona — solo conectarlo
- **ESPERAR**: SDK de ads instalado manualmente (Unity Ads / AdMob / otro)

### C-12. Legal URLs configurables via Remote Config
- Reemplazar URLs hardcodeadas por valores leidos de Firebase Remote Config al inicio
- **Archivos**: `SettingsManager.cs:88-91`, `AgeVerificationManager.cs:35-36`
- **ESPERAR**: Tarea manual #13 (Firebase Remote Config instalado y parametros publicados)

---

## BLOQUE 5 — Decisiones tomadas ✅

### ~~C-07~~ ✅ DailyOfferService Seed — Opcion B ya implementada
- **Archivo**: `DailyOfferService.cs` — `GetDaySeed()` ya tenia `dateBase + userId.GetHashCode()`
- Descubierto al leer el archivo: la Opcion B estaba implementada antes de esta sesion. Sin cambios necesarios.

### ~~C-08~~ ✅ RotatingContentService — Catalogo Firebase RTDB
- **Archivo**: `RotatingContentService.cs` — `LoadCatalogFromFirebase()` implementado
- `Start()` lanza carga desde RTDB path `rotating_content/catalog/` al inicio
- Nuevos items se agregan desde Firebase Console sin publicar update de la app
- Catalogo local (hardcoded) sigue comentado — se activa cuando los items esten verificados post-launch
