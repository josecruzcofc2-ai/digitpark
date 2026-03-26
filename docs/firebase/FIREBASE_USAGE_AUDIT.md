# FIREBASE USAGE AUDIT — DigitPark
> Generado: 2026-03-19 | **Actualizado V58: 2026-03-25**
> Eliminados: CashBattle, Stripe, Triumph, Themes, RemoteConfig, DailyRewards, DailyMissions, BattleCards, Achievements, Tournaments, Friends, Notifications, Progression, VictoryEffects, PlayerTitles, BackgroundPatterns, DeepLink.
> Sistemas activos: Auth, Realtime Database, Storage, Analytics, Firestore (backend), Cloud Functions.

---

## RESUMEN EJECUTIVO

| Producto Firebase | Estado | Cómo se accede |
|---|---|---|
| Authentication | ✅ REAL | SDK directo (AuthenticationService) |
| Realtime Database | ✅ REAL | SDK directo (DatabaseService) + accesos directos en 2 archivos |
| Analytics | ✅ REAL | SDK directo (AnalyticsService) |
| Storage | ✅ REAL | SDK directo (AvatarService) |
| Crashlytics | ⚙️ OPCIONAL (#if guard) | SDK directo en AuthenticationService |
| Firestore | ✅ REAL (solo backend) | Firebase Admin SDK en index.ts |
| Cloud Functions | ✅ REAL | Node.js backend + HTTP calls desde Unity |
| Cloud Messaging (FCM) | ❌ ELIMINADO | NotificationService.cs eliminado en simplificación |
| Remote Config | ❌ ELIMINADO | RemoteConfigService.cs eliminado en simplificación |

**Total de archivos .cs con algún tipo de uso Firebase**: ~22 archivos
**Simulación**: NO HAY. El fallback en Editor es PlayerPrefs local.

---

## SISTEMAS ELIMINADOS EN SIMPLIFICACIÓN

### V56–V57 (2026-03-24)
- **CashBattle**: CashProfileSceneController, WalletManager, CashMatchmakingManager
- **Themes**: ThemeManager (DB sync equippedTheme)
- **Stripe**: StripePaymentProvider, StripeSessionPoller, StripeCheckoutController, StripeAbortProtocol
- **Triumph**: TriumphManager, TriumphServices
- **Remote Config**: RemoteConfigService
- **Feature Flags**: LocalFlagCache, PaymentFeatureFlag
- **Achievements**: AchievementService (DB: achievements/{userId})
- **Tournaments**: TournamentManager, TournamentLobbyManager (DB: tournaments/, tournamentHistory/)
- **Friends**: FriendsManager, SearchPlayersManager, FriendRequestsSceneManager, FriendService (DB: friends/)
- **Notifications**: NotificationService (FCM), InAppNotificationManager, NotificationStorageService
- **Progression**: PlayerProgressionSystem (DB: xp/level)
- **VictoryEffects**: VictoryEffectService (DB: equippedVictoryEffect, ownedVictoryEffects)
- **PlayerTitles**: PlayerTitleService (DB: equippedTitle, ownedTitles)
- **BackgroundPatterns**: BackgroundPatternManager (DB: equippedBackground, ownedBackgrounds)
- **DeepLink**: DeepLinkService
- **UnityMainThreadDispatcher**: (ya no necesario, FCM eliminado)

### V58 (2026-03-25)
- **DailyRewards**: DailyRewardsManager, DailyRewardService (DB: dailyRewards)
- **BattleCards**: BattleCardService (DB: equippedBattleCard, ownedBattleCards)
- **DailyMissions**: DailyMissionsManager (DB: missions/)

---

## CÓMO LEER ESTE DOCUMENTO

- **Nivel A — SDK directo**: El archivo importa `using Firebase.*` y usa clases del SDK de Firebase directamente.
- **Nivel B — Service wrapper**: El archivo usa `DatabaseService.Instance`, `AuthenticationService.Instance`, etc.
- **Nivel C — HTTP a Cloud Functions**: El archivo llama a Firebase Cloud Functions via `UnityWebRequest` (sin SDK).
- **Nivel D — Modelo de datos**: El archivo define estructuras de datos que se serializan a Firebase DB.
- **Nivel E — Solo comentarios**: Menciona Firebase en comentarios/strings pero no lo usa.

---

## 1. FIREBASE AUTHENTICATION

### A — SDK directo
**`Services/Firebase/AuthenticationService.cs`**
- `Firebase`, `Firebase.Auth`, `Firebase.Extensions`
- `FirebaseApp.CheckAndFixDependenciesAsync()`, `FirebaseAuth.DefaultInstance`
- `SignInWithEmailAndPasswordAsync`, `CreateUserWithEmailAndPasswordAsync`, `SignInWithProviderAsync` (Google/Apple)
- `CurrentUser.DeleteAsync()`, `SendPasswordResetEmailAsync()`, `ReloadAsync()`, `UpdateUserProfileAsync()`
- `Crashlytics.SetUserId(userId)` — `#if FIREBASE_CRASHLYTICS`

**`Services/MatchmakingService.cs`** (acceso directo adicional)
- Lee `FirebaseAuth.DefaultInstance.CurrentUser.UserId` al inicializar
- Fallback anónimo si no hay usuario: `SystemInfo.deviceUniqueIdentifier`

### B — Via AuthenticationService.Instance
| Archivo | Qué hace con Auth |
|---|---|
| `Core/Boot/BootManager.cs` | Verifica sesión al arrancar |
| `Features/Auth/LoginManager.cs` | Login principal |
| `Features/Auth/RegisterManager.cs` | Registro de usuario |
| `Features/Auth/ForgotPasswordPopup.cs` | Reset password |
| `Features/Games/Core/MinigameBase.cs` | Lee userId para submit score |
| `Features/Games/Core/GameSessionManager.cs` | Lee userId para resultados |
| `Features/Games/Core/GameSelectorManager.cs` | Lee perfil para UI |
| `Features/Games/Navigation/MatchmakingManager.cs` | Lee userId antes de buscar rival |
| `Features/Games/Navigation/PlayModeSelectionManager.cs` | Lee perfil para mostrar datos |
| `Features/Games/DigitRush/DigitRushController.cs` | Lee userId |
| `Features/Monetization/Currency/CurrencyManager.cs` | Lee userId |
| `Features/Monetization/Premium/PremiumManager.cs` | Lee userId + restaura estado premium |
| `Features/Monetization/Shop/WelcomePackService.cs` | Lee userId |
| `Features/MainMenu/MainMenuManager.cs` | Lee perfil para UI principal |
| `Features/Settings/SettingsManager.cs` | Lee uid para logout/delete account |
| `Features/Social/Profile/ProfileManager.cs` | Lee userId |
| `Features/Social/Profile/LeaderboardManager.cs` | Lee userId para leaderboard |
| `Services/DailyOfferService.cs` | Lee userId |
| `Services/EmoteService.cs` | Lee uid para sync |
| `Services/PlayerFrameService.cs` | Lee uid para sync |
| `Services/RotatingContentService.cs` | Lee userId |
| `Services/AvatarService.cs` | Lee playerData (incluye userId) |
| `Services/PaymentBridgeWiring.cs` | Wirea `GetCurrentUserId` delegate |
| `UI/Components/AvatarUI.cs` | Lee playerData para cargar avatar |
| `DevTools/DebugManager.cs` | Muestra uid en pestaña Firebase |

---

## 2. FIREBASE REALTIME DATABASE

### A — SDK directo
**`Services/Firebase/DatabaseService.cs`** — Servicio central
- `Firebase`, `Firebase.Database`, `Firebase.Extensions`
- `FirebaseDatabase.DefaultInstance.RootReference`
- `Child()`, `GetValueAsync()`, `SetRawJsonValueAsync()`, `UpdateChildrenAsync()`, `OrderByChild()`, `LimitToFirst()`, `Remove()`, `Once()`, `RunTransaction()`

**Accesos directos adicionales:**

| Archivo | Path directo | Operación |
|---|---|---|
| `Services/MatchmakingService.cs` | `matchmaking_queue/{gameKey}` | Read/Write/Delete + `Push()` + `ServerValue.Timestamp` |
| `Services/MatchmakingService.cs` | `active_matches/{matchId}` | Read/Write + `ValueChanged` listener en tiempo real |
| `Services/RotatingContentService.cs` | `players/{uid}/rotatingContent` | Read (restaurar compras) |
| `Features/Monetization/Shop/WelcomePackService.cs` | `players/{uid}/welcomePacks` | Read (restaurar firstLogin + flags de compra) |

### Paths activos en la base de datos
```
players/{userId}                         → perfil completo
players/{userId}/rotatingContent         → contenido rotativo comprado
players/{userId}/welcomePacks            → estado Welcome Packs
players/{userId}/equippedEmotes          → emotes equipados
players/{userId}/ownedEmotePacks         → packs de emotes
players/{userId}/equippedFrame           → marco de perfil
players/{userId}/ownedFrames             → marcos desbloqueados
players/{userId}/premium                 → estado premium
leaderboards/global                      → ranking global
leaderboards/country                     → ranking por país
scores/{userId}                          → puntuaciones
matchHistory/{userId}                    → historial de partidas
ratelimits/{uid}/lastScoreSubmit         → rate-limiting anti-cheat
matchmaking_queue/{gameKey}/{entryKey}   → cola de matchmaking (1v1 normal)
active_matches/{matchId}                 → partidas en curso (1v1 normal)
```

> **Eliminados**: `players/{userId}/equippedTheme`, `players/{userId}/equippedBattleCard`, `players/{userId}/ownedBattleCards`, `players/{userId}/equippedVictoryEffect`, `players/{userId}/ownedVictoryEffects`, `players/{userId}/equippedTitle`, `players/{userId}/ownedTitles`, `players/{userId}/equippedBackground`, `players/{userId}/ownedBackgrounds`, `players/{userId}/notifications/stored`, `achievements/{userId}`, `notifications/{userId}`, `friends/{userId}`, `tournaments/{tournamentId}`, `tournamentHistory/{userId}`, `cash_scores/{uid}`

### B — Via DatabaseService.Instance
| Archivo | Qué escribe / lee |
|---|---|
| `Core/Boot/BootManager.cs` | Carga perfil inicial |
| `Features/Auth/LoginManager.cs` | Carga perfil post-login |
| `Features/Auth/RegisterManager.cs` | Crea perfil nuevo al registrar |
| `Features/Games/Core/GameSessionManager.cs` | Guarda resultados de partida |
| `Features/Games/Core/MinigameBase.cs` | Submit score |
| `Features/Games/DigitRush/DigitRushController.cs` | Score específico |
| `Features/Monetization/Currency/CurrencyManager.cs` | Guarda monedas/gemas |
| `Features/Monetization/Premium/PremiumManager.cs` | Sync + restore estado premium |
| `Features/Monetization/Shop/WelcomePackService.cs` | Escribe welcomePacks/* |
| `Features/MainMenu/MainMenuManager.cs` | Lee perfil para UI |
| `Features/Social/Profile/ProfileManager.cs` | Perfil público |
| `Features/Social/Profile/LeaderboardManager.cs` | Carga leaderboard |
| `Services/DailyOfferService.cs` | Sync oferta diaria |
| `Services/EmoteService.cs` | Sync emotes equipados/poseídos |
| `Services/PlayerFrameService.cs` | Sync marcos equipados/poseídos |
| `Services/RotatingContentService.cs` | Sync contenido rotativo |
| `Services/AvatarService.cs` | Guarda avatarUrl tras upload |
| `Services/PaymentBridgeWiring.cs` | Wirea `UpdatePlayerFields` delegate |

---

## 3. FIREBASE STORAGE

**`Services/AvatarService.cs`** — Único archivo con Firebase Storage SDK
- `Firebase.Storage` → `FirebaseStorage`, `StorageReference`, `MetadataChange`, `StorageException`
- Path: `avatars/{userId}/avatar.jpg` (JPEG 256×256, calidad 85%)
- Operaciones: `PutBytesAsync()` (upload), `GetDownloadUrlAsync()` (URL pública), `DeleteAsync()` (borrar)
- Cache local: `Application.persistentDataPath/AvatarCache/avatar_{userId}.jpg`
- Descarga de avatares de otros usuarios: via `UnityWebRequest` con URL de Storage (no SDK)
- Requiere plugin externo: `NativeGallery` (`#if UNITY_ANDROID || UNITY_IOS`)
- ⚠️ Nota GDPR: `deleteUserData` Cloud Function NO elimina avatares de Storage — pendiente

---

## 4. FIREBASE CLOUD MESSAGING (FCM)

**ELIMINADO** — `NotificationService.cs` eliminado en simplificación V57.
`DatabaseService.cs` ya no envía FCM token al backend.

---

## 5. FIREBASE ANALYTICS

**`Services/Firebase/AnalyticsService.cs`**
- `Firebase.Analytics` → `FirebaseAnalytics.SetAnalyticsCollectionEnabled()`, `LogEvent()`
- Respeta iOS ATT (`ATTService`)

### Eventos rastreados
| Evento | Origen |
|---|---|
| `screen_view` | Cambios de pantalla |
| `game_start`, `game_complete`, `game_abandoned`, `game_paused` | GameSessionManager |
| `purchase_initiated`, `purchase_completed`, `purchase_failed` | ShopItemUI, PaymentBridgeWiring |
| `login`, `signup` | LoginManager, RegisterManager |
| `rotating_content_purchased` | RotatingContentService |
| `welcome_pack_purchased` | WelcomePackService |

> **Eliminados**: `level_up`, `achievement_unlocked`, `friend_request_sent/accepted`, `challenge_sent`, `tournament_joined/completed`, `cash_matched/won/lost`

### B — Via AnalyticsService.Instance
| Archivo | Qué loguea |
|---|---|
| `Core/Boot/BootManager.cs` | `screen_view` en arranque |
| `Features/Auth/LoginManager.cs` | `login` |
| `Features/Auth/RegisterManager.cs` | `signup` |
| `Features/Games/Core/GameSessionManager.cs` | game events |
| `Features/Games/Core/GameSelectorManager.cs` | `screen_view` |
| `Features/Games/Navigation/PlayModeSelectionManager.cs` | `screen_view` |
| `Features/Games/Results/OnlineResultManager.cs` | `game_complete` |
| `Features/Monetization/Shop/WelcomePackService.cs` | `welcome_pack_purchased` |
| `Features/Monetization/Premium/PremiumManager.cs` | purchases premium |
| `Services/DailyOfferService.cs` | oferta aceptada |
| `Services/RotatingContentService.cs` | `rotating_content_purchased` |
| `Services/PaymentBridgeWiring.cs` | wirea LogCustomEvent + LogPurchaseCompleted delegates |
| `Editor/Tools/DebugMenuEditor.cs` | test event manual (solo Editor) |

---

## 6. FIREBASE CRASHLYTICS

**Guard**: `#if FIREBASE_CRASHLYTICS`
- Solo en `Services/Firebase/AuthenticationService.cs`: `Crashlytics.SetUserId(userId)` al login/registro
- No activo por defecto

---

## 7. FIRESTORE (Solo backend Node.js)

NO se usa desde Unity. Solo en `functions/src/index.ts`:

| Colección | Qué guarda |
|---|---|
| `entitlements/{userId}/{productId}` | Grants de productos IAP |

> **Eliminados**: `payment_config/admin` (adminKey Stripe abort), `payment_config/active` (provider activo Stripe/Triumph)

---

## 8. CLOUD FUNCTIONS (Backend Node.js)

**`functions/src/index.ts`** — Firebase Admin SDK, región `us-central1`

| Endpoint | Auth | Descripción |
|---|---|---|
| `iapValidateReceipt` | ✅ Firebase token | Valida receipts Apple IAP |
| `getEntitlements` | ✅ Firebase token | Obtiene entitlements del usuario |
| `checkEntitlement` | ✅ Firebase token | Verifica producto específico |
| `syncEntitlements` | ✅ Firebase token | Sync bidireccional |
| `paymentsHealth` | — | Health check |
| `validateScore` | ✅ Firebase token | Anti-cheat + rate limit (1/30s/user) |
| `deleteUserData` | ✅ Firebase token | GDPR: elimina todos los datos |

> **Eliminados**: `stripeCreateCheckout`, `stripeWebhook`, `stripeSessionStatus`, `adminForceSwitch`, `submitCashScore`

### C — Clientes HTTP de Cloud Functions (sin Firebase SDK)
| Archivo | Endpoint |
|---|---|
| `Payments/AppleIAP/AppleReceiptValidator.cs` | `iapValidateReceiptUrl` |
| `Payments/Entitlements/EntitlementService.cs` | `syncEntitlementsUrl` |

---

## 9. MODELOS DE DATOS PARA FIREBASE

| Archivo | Qué define |
|---|---|
| `Runtime/Data/PlayerData.cs` | Modelo completo del jugador. `ToFirebaseDictionary()` para serializar. |

---

## 10. INFRAESTRUCTURA DE SOPORTE (sin Firebase real)

| Archivo | Relación con Firebase |
|---|---|
| `Payments/Core/PaymentConfig.cs` | Stores URLs de Cloud Functions |
| `Payments/Core/PaymentBridge.cs` | Delegates bridge para desacoplar sistema de pagos del Firebase SDK |
| `Services/PaymentBridgeWiring.cs` | Conecta PaymentBridge delegates con AuthService, AnalyticsService, DatabaseService |
| `Services/ATTService.cs` | Menciona "antes de inicializar Firebase Analytics" en comentario |
