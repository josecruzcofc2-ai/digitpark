# PENDIENTES MANUALES
**Última actualización**: 2026-03-19
**Definición**: Acciones que requieren Firebase Console, Stripe Dashboard, App Store Connect, Google Play Console, Xcode, Unity Inspector/Editor o terminal. NO son cambios de código .cs/.ts.
**Ver también**: `docs/PENDIENTES_EDITOR.md` para tareas que sí se pueden automatizar con Editor scripts.

---

## P0 — BLOQUEANTES CRÍTICOS (hacer ANTES de publicar)

### M-43. Firebase Console — Corregir regla active_matches (fraude de scores) [SuperAudit V54]
- Ir a `https://console.firebase.google.com/project/digitpark-7d772/database/rules`
- La regla actual `.write: "auth != null"` en `active_matches/$matchId` permite a cualquier usuario autenticado sobreescribir `player1Score`/`player2Score` de partidas ajenas
- Reemplazar la regla de `active_matches/$matchId` por:
  ```json
  "$matchId": {
    ".write": "auth != null && (newData.child('player1Id').val() === auth.uid || newData.child('player2Id').val() === auth.uid || !data.exists())",
    ".validate": "newData.hasChildren(['gameKey', 'player1Id', 'player2Id', 'status'])"
  }
  ```
- Hacer click en **Publish**
- **Crítico para CashBattle**: con Triumph SDK activo + esta regla sin corregir, un usuario malicioso puede subir su score de forma fraudulenta en partidas de dinero real
- **Fuente**: SuperAudit V54 — SECURITY_AUDITOR/SEC-P1-02

### M-01. Firebase Console — Rotar API keys expuestas
- Ir a `https://console.cloud.google.com/apis/credentials?project=digitpark-7d772`
- Revocar/regenerar la API key antigua (estuvo expuesta en el repo)
- Ir a Firebase Console > Project Settings > General > descargar nuevo `GoogleService-Info.plist` (iOS) y `google-services.json` (Android)
- Reemplazar los archivos en `Assets/` (ya están en .gitignore)

### M-01b. Firebase Console — Actualizar Database Rules (CRÍTICO: amigos y GDPR)
- Ir a `https://console.firebase.google.com/project/digitpark-7d772/database/rules`
- Copiar el contenido completo de `database.rules.json` (en la raíz del repo) y pegarlo en el editor de reglas
- Hacer click en **Publish**
- **Por qué**: El archivo fue corregido el 2026-03-18 — se renombró `friendRequests` → `friend_requests` y se agregaron 4 rutas GDPR (`matchHistory`, `achievements`, `friends`, `tournamentHistory`)
- ⚠️ Sin este paso: feature de amigos no funciona en producción

### M-02. App Store Connect — Obtener Apple ID de la app
- Ir a `https://appstoreconnect.apple.com` > Mis Apps > DigitPark > campo "Apple ID"
- Copiar el número de 10 dígitos
- Abrirlo en `ReviewService.cs` línea 226 → reemplazar `idXXXXXXXXXX` por `id<número>`

### M-03. Terminal — Firebase Secrets (ANTES del deploy de Functions)
Ejecutar en `C:\Users\josec\digitPark`:
```
firebase functions:secrets:set STRIPE_SECRET_KEY
firebase functions:secrets:set STRIPE_WEBHOOK_SECRET
firebase functions:secrets:set APPLE_SHARED_SECRET
firebase functions:secrets:set SLACK_WEBHOOK_URL   (opcional)
```
- `STRIPE_SECRET_KEY`: stripe.com/dashboard > Developers > API Keys > Secret key (`sk_live_xxx`)
- `STRIPE_WEBHOOK_SECRET`: se obtiene en M-07 (Stripe webhook) → hacer M-07 primero o en paralelo
- `APPLE_SHARED_SECRET`: App Store Connect > Mis Apps > Compras dentro de la app > App-Specific Shared Secret

### M-04. Terminal — Deploy Firebase Cloud Functions
```
cd C:\Users\josec\digitPark\functions
npm install
firebase login
firebase use digitpark-7d772
firebase deploy --only functions
```
Verificar que aparecen: `paymentsHealth`, `stripeCreateCheckout`, `stripeSessionStatus`, `iapValidateReceipt`, `stripeWebhook`, `adminForceSwitch`, `getEntitlements`, `checkEntitlement`, `syncEntitlements`

### M-05. Unity Inspector — Crear PaymentConfig ScriptableObject
1. Project panel > `Assets/_Project/Resources/` > click derecho > Create > DigitPark > Payment Config > nombrar `PaymentConfig`
2. Rellenar en Inspector:
   - `Payments Health Url`: `https://us-central1-digitpark-7d772.cloudfunctions.net/paymentsHealth`
   - `Stripe Checkout Url`: `https://us-central1-digitpark-7d772.cloudfunctions.net/stripeCreateCheckout`
   - `Stripe Session Status Url`: `https://us-central1-digitpark-7d772.cloudfunctions.net/stripeSessionStatus`
   - `Iap Validate Url`: `https://us-central1-digitpark-7d772.cloudfunctions.net/iapValidateReceipt`
   - `Stripe Publishable Key`: tu clave pública Stripe (`pk_live_xxx`) ← **no se puede automatizar**
   - `Iap Product Ids`: los 6 IDs: `com.matrixsoftware.digitpark.gems_100/500/1200/2500/6500/14000`
3. Abrir `Boot.unity` > seleccionar GO con `PaymentManager` > arrastrar el ScriptableObject al campo `Config`
4. Guardar la escena

### M-06. App Store Connect + Google Play Console — Crear 6 IAP consumibles
| Product ID | Precio |
|---|---|
| `com.matrixsoftware.digitpark.gems_100` | $0.99 |
| `com.matrixsoftware.digitpark.gems_500` | $4.99 |
| `com.matrixsoftware.digitpark.gems_1200` | $9.99 |
| `com.matrixsoftware.digitpark.gems_2500` | $19.99 |
| `com.matrixsoftware.digitpark.gems_6500` | $49.99 |
| `com.matrixsoftware.digitpark.gems_14000` | $99.99 |
- En App Store Connect: Mis Apps > Compras dentro de la app > + > Consumable
- En Google Play Console: Monetización > Productos > Productos dentro de la app > Crear producto
- Después: Unity > Window > Unity IAP > Receipt Validation Obfuscator > pegar claves > Obfuscate

### M-07. Stripe Dashboard — Crear 6 productos + webhook
**Productos**: stripe.com/dashboard/products > + Add product (crear uno por cada paquete de gems, mismo precio en USD)

**Webhook**: Developers > Webhooks > + Add endpoint:
- URL: `https://us-central1-digitpark-7d772.cloudfunctions.net/stripeWebhook`
- Eventos: `checkout.session.completed`, `payment_intent.payment_failed`
- Copiar el Signing Secret (`whsec_xxx`) → usar en M-03 como `STRIPE_WEBHOOK_SECRET`

### M-08. Unity Build Settings — Verificar Development Build desmarcado
- File > Build Settings > desmarcar `Development Build`
- Player Settings > Other Settings > Scripting Define Symbols: verificar que NO está `DEVELOPMENT_BUILD`

---

## P1 — IMPORTANTES (antes o poco después de publicar)

### M-44. App Store Connect / Google Play — Ocultar botón CashBattle antes de App Review [SuperAudit V54]
- La zona CashBattle no es funcional (Triumph SDK pendiente). Apple/Google pueden rechazar por "features incompletas"
- **Opción A (recomendada)**: implementar Remote Config check en código → ver `docs/PENDIENTES_EDITOR.md` E-06. Prerequisito: M-12 + M-13 + crear parámetro `cash_battle_enabled=false` en Firebase Console
- **Opción B (rápida, sin código)**: Unity Inspector > `MainMenu.unity` > seleccionar GO del botón CashBattle > desactivarlo > guardar escena > rebuild
- Reactivar cuando Triumph SDK esté integrado y KYC aprobado
- **Fuente**: SuperAudit V54 — PRODUCTION_READINESS/PR-08

### M-45. Unity Inspector — Verificar configuración de Daily Rewards (no otorgar tipos no implementados) [SuperAudit V54]
- Abrir en Unity Editor los ScriptableObjects de configuración de DailyRewards Premium
- Verificar que **ningún** slot del ciclo usa `RewardType.PremiumTime`, `RewardType.Multiplier` ni `RewardType.RandomBox`
- Si alguno usa esos tipos: cambiar a `RewardType.DigitGems` o `RewardType.DigitCoins`
- **Por qué**: `DailyRewardService.cs` líneas 417–430 — estos tipos se "cobran" pero el jugador no recibe nada
- **Fuente**: SuperAudit V54 — PRODUCTION_READINESS/PR-11

### M-46. Terminal — Verificar que google-services.json y GoogleService-Info.plist están en .gitignore [SuperAudit V54]
- Abrir terminal en `C:\Users\josec\digitPark`
- Ejecutar: `git check-ignore -v Assets/google-services.json Assets/GoogleService-Info.plist`
- Si alguno NO aparece como ignorado: agregarlo al `.gitignore` inmediatamente
- Verificar también historial: `git log --all --full-history -- "*google-services*" "*GoogleService-Info*"`
- Si aparecen en el historial: las API keys ya están comprometidas → ejecutar M-01 (rotar keys)
- **Fuente**: SuperAudit V54 — PRODUCTION_READINESS/checklist

### M-09. Unity Player Settings — Agregar define FIREBASE_MESSAGING
- Edit > Project Settings > Player > iOS y Android > Other Settings > Scripting Define Symbols
- Agregar `FIREBASE_MESSAGING`
- Esperar que Unity recompile

### M-10. Firebase Console — Configurar APNs para Push Notifications
- Ir a `https://console.firebase.google.com/project/digitpark-7d772/settings/cloudmessaging`
- Apple app configuration > subir archivo `.p8` de APNs (generar en developer.apple.com > Certificates > Keys > + con Apple Push Notifications service)
- Rellenar Key ID y Team ID

### M-11. Xcode — Push Notifications capabilities
- Hacer build iOS desde Unity
- Abrir `.xcworkspace` en Xcode
- Target > Signing & Capabilities > + Capability > Push Notifications
- + Capability > Background Modes > marcar "Remote notifications"

### M-12. Unity Package Manager — Instalar Firebase Remote Config
- Window > Package Manager > + > Add package by name: `com.google.firebase.remote-config`

### M-13. Firebase Console — Configurar Remote Config parámetros
- `https://console.firebase.google.com/project/digitpark-7d772/config`
- Crear parámetros:
  - `payment_provider` = `stripe` (String)
  - `stripe_enabled` = `true` (Boolean)
  - `maintenance_mode` = `false` (Boolean)
  - `min_app_version` = `1.0.0` (String)
  - `cash_battle_enabled` = `false` (Boolean) ← necesario para M-44 Opción A
- Hacer click en Publish changes

### M-14. Unity Package Manager — Instalar Firebase Crashlytics
- Window > Package Manager > + > Add package by name: `com.google.firebase.crashlytics`
- (Después de instalar: agregar `Crashlytics.SetUserId(userId)` en AuthenticationService — eso sí es código)

### M-15. Xcode — Sign In with Apple capability
- Target > Signing & Capabilities > verificar que "Sign In with Apple" está en la lista
- Si no: + Capability > Sign In with Apple
- En developer.apple.com > Identifiers > App ID de DigitPark > Sign In with Apple debe estar Enabled

### M-16. Firebase Console Firestore — Crear Admin API Key
- `https://console.firebase.google.com/project/digitpark-7d772/firestore`
- Crear colección `payment_config` > documento ID `admin` > campo `adminKey` (String) = clave de 256 bits
- Generar la clave: `openssl rand -hex 32` en terminal

### M-17. Decisión de diseño — Política FR tu/vous
- Decidir: opción A (todo "tu" informal) o opción B ("vous" solo para acciones graves)
- Impacto: ~15 claves FR en Translations.txt (delete_account, logout_confirm, self_exclusion, password_change)
- Una vez decidido, avisar para implementarlo en código

---

## P1 — THEMEAPPLIER (requieren Unity Editor abierto)

### M-18. Unity Editor — Ejecutar "Add to ALL Scenes"
- Menú: DigitPark > Polish > Themes > Add to ALL Scenes
- Hacerlo DESPUÉS de ejecutar todos los UIBuilders de la sesión
- Escenas excluidas automáticamente: CashBattle, AgeVerification

### M-19. Unity Editor — BackButton.prefab: ThemeApplier en child Icon
- Abrir `Assets/_Project/Prefabs/Common/BackButton.prefab` en modo prefab edit
- Seleccionar child `Icon` (o `Arrow`)
- Add Component > ThemeApplier → ElementType = `Accent`, applyToImage = true
- Guardar prefab (se propaga a todas las escenas)

### M-20. Unity Editor — Regenerar sprite BackButton como white glyph
- Crear en Figma/Illustrator/Photoshop: chevron `<` blanco puro `#FFFFFF`, fondo transparente, 128×128 px
- Exportar como PNG con alpha
- Reemplazar el sprite en `Assets/_Project/Art/Icons/` (buscar BackIcon o similar)
- En Unity Inspector del sprite: Texture Type = Sprite, Alpha Is Transparency = ✓, Apply

---

## P2 — MEJORAS POST-LANZAMIENTO (Unity Inspector/Editor)

### M-47. Unity Inspector — Verificar/ajustar recompensas de Missions ScriptableObjects [SuperAudit V54]
- Abrir Unity Editor > navegar a los ScriptableObjects de misiones (`MissionDefinitionSO`, `MissionPoolSO`)
- Para cada misión verificar campo de recompensa DC:
  - Misión diaria fácil: 25 DC | media: 50 DC | difícil: 100 DC
  - Misión semanal: 300–500 DC + 5 DG
- Ajustar valores fuera de estos rangos
- **Fuente**: SuperAudit V54 + PENDIENTES_ECONOMIA/EC-08

### M-48. App Store Connect — Verificar target age rating (COPPA) [SuperAudit V54]
- Ir a `https://appstoreconnect.apple.com` > Mis Apps > DigitPark > App Information > Rating
- Verificar que el rating objetivo es **4+** o **12+** si hay competencia online
- **Fuente**: SuperAudit V54 — SECURITY_AUDITOR/SEC-C-05

### M-49. Localización — Revisión PT-BR con native speaker antes de publicar [SuperAudit V54]
- Revisar la columna PT completa de `Assets/_Project/Resources/Translations.txt`
- El SuperAudit detectó pérdida de diacríticos (ç, ã, õ, á, é) en >50 claves PT
- **Fuente**: SuperAudit V54 — LOCALIZATION_AUDITOR/LOC-18

### M-50. Localización — Revisión DE con native speaker antes de publicar [SuperAudit V54]
- Revisar la columna DE completa de `Assets/_Project/Resources/Translations.txt`
- 7 casos confirmados de umlauts/ß faltantes + posiblemente más
- **Fuente**: SuperAudit V54 — LOCALIZATION_AUDITOR/LOC-20

### M-24. Unity Inspector — Verificar Tier B themes priceType
- Abrir cada ScriptableObject de tema Tier B (Sakura, Matrix, CyberFuchsia, etc.)
- Verificar: `priceType = DigitGems`, `gemsPrice = 350`
- Corregir los que tengan `priceType = DigitCoins`

### M-26. Unity Inspector — OnboardingManager: completionRewardGems = 0
- Abrir `Onboarding.unity` > seleccionar GO con `OnboardingManager`
- Inspector: campo `Completion Reward Gems` → cambiar a `0`

### M-27. Unity Inspector — Achievements.unity: asignar TrophyCard.prefab
- Abrir `Achievements.unity` > seleccionar GO con `AchievementsManager`
- Arrastrar `Assets/_Project/Prefabs/Monetization/TrophyCard.prefab` al campo `Trophy Card Prefab`

### M-28. Unity Inspector — Shop.unity: desactivar GO debug D80/D90
- Abrir `Shop.unity`
- Buscar en jerarquía GO con texto "D80" o "D90" visible
- Si está activo: desactivarlo (checkbox off) o eliminarlo

### M-29. Unity Inspector — Canvas Scaler: verificar 6 escenas
Para cada escena, seleccionar Canvas > Canvas Scaler y verificar:
- UI Scale Mode = Scale With Screen Size
- Reference Resolution = 1080 × 1920
- Match Width Or Height = 0.5
- Escenas: Boot.unity, MainMenu.unity, Settings.unity, AgeVerification.unity, Register.unity, CashBattleHub.unity

### M-30. Unity Inspector — Boot.unity: verificar BootManager fields wired
- Abrir `Boot.unity` > seleccionar GO con `BootManager`
- Verificar que `loadingBar`, `loadingText`, `versionText` NO están vacíos
- Si están vacíos: DigitPark > Scenes > Build Scene > Core > Boot

### M-31. Unity Inspector — CashBattle1v1.unity: verificar Rounds visual
- Abrir `CashBattle1v1.unity` > entrar en Play Mode
- Verificar que Round 1 aparece en gold al entrar (no Round 3)
- Si está mal: ver fix en PENDIENTES_CODIGO.md (CashBattle1v1UIBuilder.cs línea ~876)

### M-32. Unity Editor — CashHistory.unity: fix titleText null
- Abrir `CashHistory.unity`
- Ejecutar: DigitPark > Scenes > Build Scene > CashBattle > History

### M-39. Unity Editor — Regenerar 8 UIBuilders (tras agregar FrameRenderer en código)
Ejecutar en orden desde menú DigitPark:
1. Friends > Build Friends UI
2. Profile > Build Profile UI
3. Profile > Build Scores UI
4. Matchmaking > Build Matchmaking UI
5. Friends > Build Friend Requests UI
6. Friends > Build Search Players UI
7. Core > Build Main Menu UI
8. Monetization > Build Shop Premium UI

### M-41. Unity Editor — StarterPack: ejecutar Build Shop Premium UI (después del código)
- **Prerequisito**: Código de StarterPack timer D1–D3 implementado (ver PENDIENTES_ECONOMIA EC-01)
- Menú: DigitPark > Monetization > Build Shop Premium UI
- **Verificar en Play Mode**: que el panel de StarterPack aparece solo en D1–D3, desaparece después

### M-40. Unity Editor — Dual ThemeApplier: revisión manual de Outlines por escena (~200 casos)
- **Qué hacer**: Por cada escena, buscar GOs que tengan Image + Outline donde la Outline usa un color de tema
- **Patrón**: Add Component > ThemeApplier → ElementType = `Glow`, applyToImage = false, applyToOutline = true
- **Frecuencia**: 1 escena por sesión (workflow preference)
- **Scope**: ~200 casos estimados distribuidos en las 29 escenas temables
- **Escenas excluidas**: CashBattle (#08, #32-40), AgeVerification
- **Prerequisito**: M-18 (Add to ALL Scenes) ejecutado primero

---

## BLOQUE 2 — TRIUMPH SDK (esperar hasta recibir el SDK)

### T-M1. Integrar SDK de Triumph (core CashBattle)
- Una vez recibido el SDK: conectar `IKYCService`, `IWalletService`, `IMatchmakingService`, `ITournamentService` en `TriumphServices.cs`
- Cambiar `ServiceMode.Mock` → `ServiceMode.Live` en `ServiceLocator.cs:237`

### T-M2. Certificate Pinning para transacciones dinero real
- iOS: TrustKit via CocoaPods
- Android: OkHttp Certificate Pinner
- Integrar en las llamadas HTTP del SDK de Triumph (configurar en Xcode/Gradle)

### T-M3. Jailbreak/Root Detection para CashBattle
- iOS: IOSSecuritySuite via Swift Package Manager o CocoaPods
- Android: rootbeer via Gradle dependency
- Bloquear CashBattle si dispositivo comprometido

### T-M4. Responsible Gaming (Xcode/backend Triumph)
- Self-exclusion: conectar `SettingsManager.cs:1127` con API de Triumph
- Límites de gasto configurables desde backend Triumph

---

## ELIMINADAS (ya implementadas)
- ~~M-33~~ `PlayerSearchItem.prefab` — existe en `Prefabs/Social/`
- ~~M-34~~ `TournamentItem.prefab` — existe en `Prefabs/Tournaments/Browser/`
- ~~M-35~~ `ParticipantItem.prefab` — existe en `Prefabs/Tournaments/Lobby/`
- ~~M-36~~ `LeaderboardEntry.prefab` — existe en `Prefabs/Social/`
- ~~M-37~~ `TrophyCard.prefab` — existe en `Prefabs/Monetization/`
- ~~M-25~~ Mislabeled — es código (`ProductCatalog.cs`), ver PENDIENTES_CODIGO.md
- ~~M-21~~ Movida a `PENDIENTES_EDITOR.md` (E-01)
- ~~M-22~~ Movida a `PENDIENTES_EDITOR.md` (E-02)
- ~~M-23~~ Movida a `PENDIENTES_EDITOR.md` (E-03)
- ~~M-38~~ Movida a `PENDIENTES_EDITOR.md` (E-04)
- ~~M-42~~ Movida a `PENDIENTES_EDITOR.md` (E-05)

---
*Verificado contra código real en 2026-03-19*
