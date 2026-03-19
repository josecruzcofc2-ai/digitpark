# FIREBASE USAGE AUDIT — DigitPark
> Generado: 2026-03-19 | Verificado con grep exhaustivo en 66 archivos | Todos los .cs + index.ts

---

## RESUMEN EJECUTIVO

| Producto Firebase | Estado | Cómo se accede |
|---|---|---|
| Authentication | ✅ REAL | SDK directo (AuthenticationService) |
| Realtime Database | ✅ REAL | SDK directo (DatabaseService) + accesos directos en 4 archivos |
| Cloud Messaging (FCM) | ✅ REAL (#if guard) | SDK directo (NotificationService) |
| Analytics | ✅ REAL | SDK directo (AnalyticsService) |
| Remote Config | ✅ REAL (#if guard, fallback local) | SDK directo (RemoteConfigService) |
| Storage | ✅ REAL | SDK directo (AvatarService) |
| Crashlytics | ⚙️ OPCIONAL (#if guard) | SDK directo en AuthenticationService |
| Firestore | ✅ REAL (solo backend) | Firebase Admin SDK en index.ts |
| Cloud Functions | ✅ REAL | Node.js backend + HTTP calls desde Unity |

**Total de archivos .cs con algún tipo de uso Firebase**: **55 archivos** (13 con SDK directo, 42 via service wrappers)
**Simulación**: NO HAY. El fallback en Editor es PlayerPrefs local.

---

## CÓMO LEER ESTE DOCUMENTO

- **Nivel A — SDK directo**: El archivo importa `using Firebase.*` y usa clases del SDK de Firebase directamente.
- **Nivel B — Service wrapper**: El archivo usa `DatabaseService.Instance`, `AuthenticationService.Instance`, etc. (los servicios de DigitPark que envuelven Firebase).
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

### B — Via AuthenticationService.Instance (40 archivos)
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
| `Features/Monetization/DailyMissions/DailyMissionsManager.cs` | Lee userId |
| `Features/Monetization/DailyRewards/DailyRewardsManager.cs` | Lee userId |
| `Features/Monetization/Currency/CurrencyManager.cs` | Lee userId |
| `Features/Monetization/Progression/PlayerProgressionSystem.cs` | Lee userId |
| `Features/Monetization/Premium/PremiumManager.cs` | Lee userId + restaura estado premium |
| `Features/Monetization/Shop/WelcomePackService.cs` | Lee userId |
| `Features/MainMenu/MainMenuManager.cs` | Lee perfil para UI principal |
| `Features/Settings/SettingsManager.cs` | Lee uid para logout/delete account |
| `Features/Social/Profile/ProfileManager.cs` | Lee userId |
| `Features/Social/Profile/LeaderboardManager.cs` | Lee userId para leaderboard |
| `Features/Social/Friends/FriendsManager.cs` | Lee userId |
| `Features/Social/Friends/SearchPlayersManager.cs` | Lee userId |
| `Features/Social/Friends/FriendRequestsSceneManager.cs` | Lee userId |
| `Features/Tournaments/TournamentManager.cs` | Lee userId |
| `Features/Tournaments/TournamentLobbyManager.cs` | Lee userId |
| `Features/CashBattle/Profile/CashProfileSceneController.cs` | Lee userId |
| `Features/Cosmetics/Backgrounds/BackgroundPatternManager.cs` | Lee uid para sync |
| `Features/Cosmetics/BattleCards/BattleCardService.cs` | Lee uid para sync |
| `Services/AchievementService.cs` | Lee userId |
| `Services/DailyRewardService.cs` | Lee userId |
| `Services/DailyOfferService.cs` | Lee userId |
| `Services/FriendService.cs` | Lee userId |
| `Services/EmoteService.cs` | Lee uid para sync |
| `Services/VictoryEffectService.cs` | Lee uid para sync |
| `Services/PlayerTitleService.cs` | Lee uid para sync |
| `Services/PlayerFrameService.cs` | Lee uid para sync |
| `Services/NotificationStorageService.cs` | Lee uid para sync |
| `Services/RotatingContentService.cs` | Lee userId |
| `Services/AvatarService.cs` | Lee playerData (incluye userId) |
| `Themes/ThemeManager.cs` | Lee uid para sync tema equipado |
| `Services/PaymentBridgeWiring.cs` | Wirea `GetCurrentUserId` delegate |
| `UI/Components/AvatarUI.cs` | Lee playerData para cargar avatar |
| `Features/CashBattle/Hub/CashMatchmakingManager.cs` | Carga avatar propio y del oponente vía AvatarService (Firebase Storage) |
| `DevTools/DebugManager.cs` | Muestra uid en pestaña Firebase |

---

## 2. FIREBASE REALTIME DATABASE

### A — SDK directo
**`Services/Firebase/DatabaseService.cs`** — Servicio central
- `Firebase`, `Firebase.Database`, `Firebase.Extensions`
- `FirebaseDatabase.DefaultInstance.RootReference`
- `Child()`, `GetValueAsync()`, `SetRawJsonValueAsync()`, `UpdateChildrenAsync()`, `OrderByChild()`, `LimitToFirst()`, `Remove()`, `Once()`, `RunTransaction()`

**Accesos directos adicionales (alias `FirebaseDB = global::Firebase.Database`):**

| Archivo | Path directo | Operación |
|---|---|---|
| `Services/MatchmakingService.cs` | `matchmaking_queue/{gameKey}` | Read/Write/Delete + `Push()` + `ServerValue.Timestamp` |
| `Services/MatchmakingService.cs` | `active_matches/{matchId}` | Read/Write + `ValueChanged` listener en tiempo real |
| `Features/Cosmetics/BattleCards/BattleCardService.cs` | `players/{uid}` | Read (equippedBattleCard, ownedBattleCards) |
| `Services/RotatingContentService.cs` | `players/{uid}/rotatingContent` | Read (restaurar compras) |
| `Features/Monetization/Shop/WelcomePackService.cs` | `players/{uid}/welcomePacks` | Read (restaurar firstLogin + flags de compra) |

### Paths en la base de datos
```
players/{userId}                         → perfil completo
players/{userId}/equippedBattleCard      → BattleCard activa
players/{userId}/ownedBattleCards        → BattleCards desbloqueadas
players/{userId}/rotatingContent         → contenido rotativo comprado
players/{userId}/welcomePacks            → estado Welcome Packs
players/{userId}/equippedBackground      → fondo de perfil activo
players/{userId}/ownedBackgrounds        → fondos desbloqueados
players/{userId}/equippedTheme           → tema activo
players/{userId}/equippedEmotes          → emotes equipados
players/{userId}/ownedEmotePacks         → packs de emotes
players/{userId}/equippedVictoryEffect   → efecto de victoria
players/{userId}/ownedVictoryEffects     → efectos desbloqueados
players/{userId}/equippedTitle           → título activo
players/{userId}/ownedTitles             → títulos desbloqueados
players/{userId}/equippedFrame           → marco de perfil
players/{userId}/ownedFrames             → marcos desbloqueados
players/{userId}/premium                 → estado premium (tournaments, cashBattle, stylesPro)
players/{userId}/notifications/stored    → notificaciones persistidas
leaderboards/global                      → ranking global
leaderboards/country                     → ranking por país
tournaments/{tournamentId}               → datos de torneos
scores/{userId}                          → puntuaciones
matchHistory/{userId}                    → historial de partidas
achievements/{userId}                    → logros
notifications/{userId}                   → notificaciones in-app
friends/{userId}                         → lista de amigos
tournamentHistory/{userId}               → historial de torneos
ratelimits/{uid}/lastScoreSubmit         → rate-limiting anti-cheat
matchmaking_queue/{gameKey}/{entryKey}   → cola de matchmaking
active_matches/{matchId}                 → partidas en curso (1v1)
```

### B — Via DatabaseService.Instance (34 archivos)
| Archivo | Qué escribe / lee |
|---|---|
| `Core/Boot/BootManager.cs` | Carga perfil inicial |
| `Features/Auth/LoginManager.cs` | Carga perfil post-login |
| `Features/Auth/RegisterManager.cs` | Crea perfil nuevo al registrar |
| `Features/Games/Core/GameSessionManager.cs` | Guarda resultados de partida |
| `Features/Games/Core/MinigameBase.cs` | Submit score |
| `Features/Games/DigitRush/DigitRushController.cs` | Score específico |
| `Features/Monetization/DailyMissions/DailyMissionsManager.cs` | Progreso misiones |
| `Features/Monetization/DailyRewards/DailyRewardsManager.cs` | Estado recompensas diarias |
| `Features/Monetization/Currency/CurrencyManager.cs` | Guarda monedas/gemas |
| `Features/Monetization/Progression/PlayerProgressionSystem.cs` | XP + nivel |
| `Features/Monetization/Premium/PremiumManager.cs` | Sync + restore estado premium |
| `Features/Monetization/Shop/WelcomePackService.cs` | Escribe welcomePacks/* |
| `Features/MainMenu/MainMenuManager.cs` | Lee perfil para UI |
| `Features/Social/Profile/ProfileManager.cs` | Perfil público |
| `Features/Social/Profile/LeaderboardManager.cs` | Carga leaderboard |
| `Features/Social/Friends/FriendsManager.cs` | Lista de amigos |
| `Features/Social/Friends/SearchPlayersManager.cs` | Búsqueda por username |
| `Features/Social/Friends/FriendRequestsSceneManager.cs` | Solicitudes de amistad |
| `Features/Tournaments/TournamentManager.cs` | CRUD torneos |
| `Features/Tournaments/TournamentLobbyManager.cs` | Estado lobby |
| `Features/CashBattle/Profile/CashProfileSceneController.cs` | Perfil CashBattle |
| `Features/Cosmetics/Backgrounds/BackgroundPatternManager.cs` | Sync/restore fondo equipado |
| `Features/Cosmetics/BattleCards/BattleCardService.cs` | Sync BattleCard equipada |
| `Services/AchievementService.cs` | Persiste logros |
| `Services/DailyRewardService.cs` | Persiste recompensas |
| `Services/DailyOfferService.cs` | Sync oferta diaria |
| `Services/FriendService.cs` | Solicitudes de amistad |
| `Services/EmoteService.cs` | Sync emotes equipados/poseídos |
| `Services/VictoryEffectService.cs` | Sync efectos de victoria |
| `Services/PlayerTitleService.cs` | Sync títulos equipados/poseídos |
| `Services/PlayerFrameService.cs` | Sync marcos equipados/poseídos |
| `Services/NotificationStorageService.cs` | Persiste notificaciones leídas |
| `Services/RotatingContentService.cs` | Sync contenido rotativo |
| `Services/AvatarService.cs` | Guarda avatarUrl tras upload |
| `Themes/ThemeManager.cs` | Sync tema activo |
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

**`Services/Firebase/NotificationService.cs`** — `#if FIREBASE_MESSAGING`
- `Firebase.Messaging` → `FirebaseMessaging.GetTokenAsync()`, eventos `TokenReceived` y `MessageReceived`
- Token guardado en `PlayerPrefs["FCM_Token"]`
- `#if !UNITY_EDITOR` — sin push en Editor

**`Services/Firebase/DatabaseService.cs`** — `#if FIREBASE_MESSAGING` (bloque interno)
- Envía FCM token al backend al registrarlo

### B — Via NotificationService.Instance
| Archivo | Uso |
|---|---|
| `Core/Boot/BootManager.cs` | Init al arrancar |
| `Features/Settings/SettingsManager.cs` | Habilitar/deshabilitar notificaciones push |
| `UI/Notifications/InAppNotificationManager.cs` | Muestra banners in-app |

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
| `level_up`, `achievement_unlocked` | PlayerProgressionSystem, AchievementService |
| `friend_request_sent`, `friend_request_accepted`, `challenge_sent` | FriendsManager, FriendService |
| `tournament_joined`, `tournament_completed` | TournamentManager |
| `login`, `signup` | LoginManager, RegisterManager |
| `rotating_content_purchased` | RotatingContentService |
| `welcome_pack_purchased` | WelcomePackService |

### B — Via AnalyticsService.Instance (24 archivos)
| Archivo | Qué loguea |
|---|---|
| `Core/Boot/BootManager.cs` | `screen_view` en arranque |
| `Features/Auth/LoginManager.cs` | `login` |
| `Features/Auth/RegisterManager.cs` | `signup` |
| `Features/Games/Core/GameSessionManager.cs` | game events |
| `Features/Games/Core/GameSelectorManager.cs` | `screen_view` |
| `Features/Games/Navigation/PlayModeSelectionManager.cs` | `screen_view` |
| `Features/Games/Results/OnlineResultManager.cs` | `game_complete` |
| `Features/Monetization/DailyMissions/DailyMissionsManager.cs` | misiones completadas |
| `Features/Monetization/DailyRewards/DailyRewardsManager.cs` | recompensas reclamadas |
| `Features/Monetization/Progression/PlayerProgressionSystem.cs` | `level_up` |
| `Features/Monetization/Shop/WelcomePackService.cs` | `welcome_pack_purchased` |
| `Features/Monetization/Premium/PremiumManager.cs` | purchases premium |
| `Features/Social/Friends/FriendsManager.cs` | `friend_request_sent` |
| `Services/AchievementService.cs` | `achievement_unlocked` |
| `Services/DailyOfferService.cs` | oferta aceptada |
| `Services/RotatingContentService.cs` | `rotating_content_purchased` |
| `Services/FriendService.cs` | friend events |
| `Features/CashBattle/Wallet/WalletManager.cs` | transacciones wallet |
| `Features/CashBattle/Hub/CashMatchmakingManager.cs` | carga avatares propio + oponente |
| `Services/PaymentBridgeWiring.cs` | wirea LogCustomEvent + LogPurchaseCompleted delegates |
| `Editor/Tools/DebugMenuEditor.cs` | test event manual (solo Editor) |

---

## 6. FIREBASE REMOTE CONFIG

**`Payments/FeatureFlags/RemoteConfigService.cs`** — `#if FIREBASE_REMOTE_CONFIG`
- `Firebase.RemoteConfig` → `FetchAsync()`, `ActivateAsync()`, `GetValue().StringValue / BooleanValue`
- Polling cada 15 min, timeout 10s
- Fallback: `LocalFlagCache` (JSON local) si no está instalado

| Key | Tipo | Valor |
|---|---|---|
| `payment_provider` | string | `stripe` / `apple_iap` |
| `stripe_enabled` | bool | — |
| `apple_iap_enabled` | bool | — |
| `triumph_enabled` | bool | — |
| `maintenance_mode` | bool | — |
| `app_version` | string | versión mínima |

---

## 7. FIREBASE CRASHLYTICS

**Guard**: `#if FIREBASE_CRASHLYTICS`
- Solo en `Services/Firebase/AuthenticationService.cs`: `Crashlytics.SetUserId(userId)` al login/registro
- No activo por defecto

---

## 8. FIRESTORE (Solo backend Node.js)

NO se usa desde Unity. Solo en `functions/src/index.ts`:

| Colección | Qué guarda |
|---|---|
| `entitlements/{userId}/{productId}` | Grants de productos |
| `payment_config/admin` | adminKey abort protocol |
| `payment_config/active` | Proveedor activo |

---

## 9. CLOUD FUNCTIONS (Backend Node.js)

**`functions/src/index.ts`** — Firebase Admin SDK, región `us-central1`

| Endpoint | Auth | Descripción |
|---|---|---|
| `stripeCreateCheckout` | ✅ Firebase token | Crea sesión Stripe |
| `stripeWebhook` | ✅ Stripe signature | Recibe eventos Stripe |
| `stripeSessionStatus` | ⚠️ Sin auth | Polling estado sesión |
| `iapValidateReceipt` | ✅ Firebase token | Valida receipts Apple |
| `getEntitlements` | ✅ Firebase token | Obtiene entitlements |
| `checkEntitlement` | ✅ Firebase token | Verifica producto |
| `syncEntitlements` | ✅ Firebase token | Sync bidireccional |
| `adminForceSwitch` | ✅ adminKey Firestore | Abort protocol. Fail-closed. Slack alert. |
| `validateScore` | ✅ Firebase token | Anti-cheat + rate limit (1/30s/user) |
| `deleteUserData` | ✅ Firebase token | GDPR: elimina todos los datos |
| `paymentsHealth` | — | Health check |

### C — Clientes HTTP de Cloud Functions (sin Firebase SDK)
| Archivo | Endpoint | Método |
|---|---|---|
| `Payments/AppleIAP/AppleReceiptValidator.cs` | `iapValidateReceiptUrl` | POST JSON |
| `Payments/Stripe/StripeSessionPoller.cs` | `stripeSessionStatusUrl` | GET polling |
| `Payments/Entitlements/EntitlementService.cs` | `syncEntitlementsUrl` | Via delegate |

---

## 10. MODELOS DE DATOS PARA FIREBASE

Estos archivos **no usan el SDK** pero definen las estructuras serializadas a/desde Firebase DB:

| Archivo | Qué define |
|---|---|
| `Runtime/Data/PlayerData.cs` | Modelo completo del jugador. `ToFirebaseDictionary()` para serializar. Todos los campos van a `players/{userId}`. |
| `Runtime/Data/TournamentData.cs` | Modelo de torneo. `ToDictionary()` para serializar a `tournaments/{id}`. |

---

## 11. INFRAESTRUCTURA DE SOPORTE (sin Firebase real)

| Archivo | Relación con Firebase |
|---|---|
| `Services/UnityMainThreadDispatcher.cs` | Marshaliza callbacks de Firebase/FCM (que llegan en background threads) al main thread. Mencionado en comentario únicamente — no usa SDK. |
| `Payments/Core/PaymentConfig.cs` | Stores URLs de Cloud Functions. Solo strings. |
| `Payments/Core/PaymentBridge.cs` | Delegates bridge para desacoplar el sistema de pagos del Firebase SDK. |
| `Services/PaymentBridgeWiring.cs` | Conecta `PaymentBridge` delegates con AuthService, AnalyticsService, DatabaseService, CurrencyManager, DeepLinkService reales. Usa `using DigitPark.Services.Firebase`. |
| `Payments/Core/PaymentManager.cs` | Usa `EntitlementService.Instance` + `PaymentBridge` delegates. Toca Firebase indirectamente vía la cadena: PaymentBridge → PaymentBridgeWiring → DatabaseService/AnalyticsService. |
| `Payments/Stripe/StripeCheckoutController.cs` | Usa `PaymentBridge` delegates (LogCustomEvent, ProcessGemsPurchase). Firebase indirecto via wiring. |
| `Payments/Abort/StripeAbortProtocol.cs` | Usa `PaymentBridge` delegates (LogCustomEvent). Firebase indirecto via wiring. |
| `Services/DeepLinkService.cs` | Lee `AuthenticationService.Instance` para obtener uid en deep links. |
| `Services/ATTService.cs` | Menciona "antes de inicializar Firebase Analytics" en comentario. No usa SDK. |
| `Features/Social/Notifications/NotificationsManager.cs` | Tiene `using DigitPark.Services.Firebase;` como **import muerto** — nunca llama ningún servicio Firebase. Grep confirmado. |

---

## 12. HERRAMIENTAS DE EDITOR (no van en APK/IPA)

| Archivo | Uso Firebase |
|---|---|
| `Editor/Tools/DebugMenuEditor.cs` | Pestaña "Firebase" en el debug menu: muestra uid, FCM token, lanza test events de Analytics. Usa `Services.Firebase.*` en tiempo de Editor. |
| `Editor/Tools/FolderReorgMigration.cs` | Herramienta de migración de carpetas. Solo menciona paths de Firebase para reorganizar. No usa SDK. |
| `DevTools/DebugManager.cs` | Pestaña Firebase en runtime debug overlay. Usa `AuthService`, `AnalyticsService`, `DatabaseService`. |

---

## 13. INVENTARIO COMPLETO — TODOS LOS ARCHIVOS

### Nivel A: Firebase SDK directo (13 archivos)
```
Services/Firebase/
├── AuthenticationService.cs     → Auth + Crashlytics (opcional)
├── DatabaseService.cs           → Realtime Database (servicio central)
├── NotificationService.cs       → FCM (#if FIREBASE_MESSAGING)
└── AnalyticsService.cs          → Analytics

Services/
├── MatchmakingService.cs        → Auth (read) + Realtime Database directa
├── AvatarService.cs             → Firebase Storage
└── RotatingContentService.cs    → Realtime Database directa

Features/Cosmetics/BattleCards/
└── BattleCardService.cs         → Realtime Database directa

Features/Monetization/Shop/
└── WelcomePackService.cs        → Realtime Database directa

Core/Boot/
└── BootManager.cs               → Init Firebase + usa Auth + DB + FCM + Analytics

Features/Monetization/
├── DailyMissions/DailyMissionsManager.cs  → DB directa (usando Firebase namespace)
├── DailyRewards/DailyRewardsManager.cs    → DB directa
└── Currency/CurrencyManager.cs            → DB directa

Payments/FeatureFlags/
└── RemoteConfigService.cs       → Remote Config (#if FIREBASE_REMOTE_CONFIG)
```

### Nivel B: Via service wrappers (42 archivos)
```
Auth consumers:          LoginManager, RegisterManager, ForgotPasswordPopup
Game consumers:          MinigameBase, GameSessionManager, GameSelectorManager,
                         MatchmakingManager, PlayModeSelectionManager, DigitRushController
                         ⚠️ FlashTap/MemoryPairs/OddOneOut/QuickMath NO usan Firebase —
                            delegan score/resultados a GameSessionManager/MinigameBase
Results:                 OnlineResultManager
                         ⚠️ SprintSummaryPanelController NO usa Firebase — confirmado por grep
Monetization:            PlayerProgressionSystem, PremiumManager, DailyOfferService
Social:                  ProfileManager, LeaderboardManager, FriendsManager,
                         SearchPlayersManager, FriendRequestsSceneManager,
                         FriendService
                         ⚠️ NotificationsManager tiene import muerto (using DigitPark.Services.Firebase)
                            pero NUNCA llama ningún servicio Firebase — no es consumer real
Tournaments:             TournamentManager, TournamentLobbyManager
                         ⚠️ TournamentsBrowserManager y TournamentCreateManager NO usan Firebase
CashBattle:              CashProfileSceneController, WalletManager, CashMatchmakingManager
Settings:                SettingsManager
MainMenu:                MainMenuManager
Cosmetics:               BackgroundPatternManager
Themes:                  ThemeManager
Services cosmetics:      EmoteService, VictoryEffectService, PlayerTitleService,
                         PlayerFrameService, NotificationStorageService
Payments (wiring):       PaymentBridgeWiring, EntitlementService
UI:                      AvatarUI, LeaderboardEntryUI, TournamentMyItemUI
Debug:                   DebugManager
Deep links:              DeepLinkService
```

### Nivel C: HTTP a Cloud Functions (3 archivos)
```
Payments/AppleIAP/AppleReceiptValidator.cs
Payments/Stripe/StripeSessionPoller.cs
Payments/Entitlements/EntitlementService.cs
```

### Nivel D: Modelos de datos (2 archivos)
```
Data/PlayerData.cs
Data/TournamentData.cs
```

### Nivel E: Solo infraestructura/comentarios (5 archivos)
```
Services/UnityMainThreadDispatcher.cs
Services/ATTService.cs
Payments/Core/PaymentConfig.cs
Payments/Core/PaymentBridge.cs
Editor/Tools/FolderReorgMigration.cs
```

### Backend
```
functions/src/index.ts   → Cloud Functions + Admin SDK + Firestore
```

---

## 14. NOTAS DE SEGURIDAD

1. **`stripeSessionStatus`**: sin auth — session IDs opacos, riesgo bajo pero documentado.
2. **Firebase Storage rules**: deben permitir solo `auth.uid == userId` para `avatars/{userId}/*`.
3. **GDPR gap**: `deleteUserData` Cloud Function elimina DB pero NO avatares en Storage. Pendiente.
4. **FCM**: desactivado en Editor (`#if !UNITY_EDITOR`).
5. **Remote Config**: fallback a `LocalFlagCache` si SDK no instalado. Sin crash.
6. **Anti-cheat doble**: MinigameBase (cliente) + Cloud Function `validateScore` (servidor, 1 submit/30s).
7. **Crashlytics**: no activo por defecto — requiere `FIREBASE_CRASHLYTICS` en Player Settings.
8. **MatchmakingService**: usa `ValueChanged` listener en tiempo real — limpiar con `UnsubscribeOpponentListener()` en `OnDestroy` y `OnApplicationQuit`. Ya implementado.
