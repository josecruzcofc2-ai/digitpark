# FIREBASE USAGE AUDIT — DigitPark
> Generado: 2026-03-19 | **Actualizado post-simplificación V58: 2026-03-25**
> CashBattle, Stripe, Triumph, Themes, RemoteConfig, DailyRewards, BattleCards eliminados.
> Sistemas activos: Auth, Realtime Database, Storage, FCM, Analytics, Firestore (backend), Cloud Functions.

---

## RESUMEN EJECUTIVO

| Producto Firebase | Estado | Cómo se accede |
|---|---|---|
| Authentication | ✅ REAL | SDK directo (AuthenticationService) |
| Realtime Database | ✅ REAL | SDK directo (DatabaseService) + accesos directos en 4 archivos |
| Cloud Messaging (FCM) | ✅ REAL (#if guard) | SDK directo (NotificationService) |
| Analytics | ✅ REAL | SDK directo (AnalyticsService) |
| Storage | ✅ REAL | SDK directo (AvatarService) |
| Crashlytics | ⚙️ OPCIONAL (#if guard) | SDK directo en AuthenticationService |
| Firestore | ✅ REAL (solo backend) | Firebase Admin SDK en index.ts |
| Cloud Functions | ✅ REAL | Node.js backend + HTTP calls desde Unity |
| Remote Config | ❌ ELIMINADO | RemoteConfigService.cs eliminado en simplificación |

**Total de archivos .cs con algún tipo de uso Firebase**: ~42 archivos
**Simulación**: NO HAY. El fallback en Editor es PlayerPrefs local.

---

## SISTEMAS ELIMINADOS EN SIMPLIFICACIÓN (2026-03-24)

Los siguientes sistemas y sus archivos Firebase fueron eliminados:
- **CashBattle**: CashProfileSceneController, WalletManager, CashMatchmakingManager (Auth + DB + Analytics)
- **Themes**: ThemeManager (DB sync de equippedTheme)
- **Stripe**: StripePaymentProvider, StripeSessionPoller, StripeCheckoutController, StripeAbortProtocol (HTTP a Cloud Functions)
- **Triumph**: TriumphManager, TriumphServices
- **Remote Config**: RemoteConfigService (Firebase Remote Config SDK)
- **Feature Flags**: LocalFlagCache, PaymentFeatureFlag
- **ServiceLocator**: Eliminado (ya no hay interfaces CashBattle)

### V58 — 2026-03-25
- **DailyRewards**: DailyRewardsManager, DailyRewardService (DB: dailyRewards, premium rewards)
- **BattleCards**: BattleCardService (DB: equippedBattleCard, ownedBattleCards)

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
| `Features/Cosmetics/BattleCards/BattleCardService.cs` | `players/{uid}` | Read (equippedBattleCard, ownedBattleCards) |
| `Services/RotatingContentService.cs` | `players/{uid}/rotatingContent` | Read (restaurar compras) |
| `Features/Monetization/Shop/WelcomePackService.cs` | `players/{uid}/welcomePacks` | Read (restaurar firstLogin + flags de compra) |

### Paths activos en la base de datos
```
players/{userId}                         → perfil completo
players/{userId}/equippedBattleCard      → BattleCard activa
players/{userId}/ownedBattleCards        → BattleCards desbloqueadas
players/{userId}/rotatingContent         → contenido rotativo comprado
players/{userId}/welcomePacks            → estado Welcome Packs
players/{userId}/equippedBackground      → fondo de perfil activo
players/{userId}/ownedBackgrounds        → fondos desbloqueados
players/{userId}/equippedEmotes          → emotes equipados
players/{userId}/ownedEmotePacks         → packs de emotes
players/{userId}/equippedVictoryEffect   → efecto de victoria
players/{userId}/ownedVictoryEffects     → efectos desbloqueados
players/{userId}/equippedTitle           → título activo
players/{userId}/ownedTitles             → títulos desbloqueados
players/{userId}/equippedFrame           → marco de perfil
players/{userId}/ownedFrames             → marcos desbloqueados
players/{userId}/premium                 → estado premium
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
matchmaking_queue/{gameKey}/{entryKey}   → cola de matchmaking (1v1 normal)
active_matches/{matchId}                 → partidas en curso (1v1 normal)
```

> **Eliminados**: `players/{userId}/equippedTheme`, `players/{userId}/premium/cashBattle`, `cash_scores/{uid}`

### B — Via DatabaseService.Instance
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

> **Eliminados**: `cash_matched`, `cash_won`, `cash_lost`, eventos de wallet/transacciones CashBattle

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
| `Runtime/Data/TournamentData.cs` | Modelo de torneo. `ToDictionary()` para serializar a `tournaments/{id}`. |

---

## 10. INFRAESTRUCTURA DE SOPORTE (sin Firebase real)

| Archivo | Relación con Firebase |
|---|---|
| `Services/UnityMainThreadDispatcher.cs` | Marshaliza callbacks de Firebase/FCM (background threads) al main thread |
| `Payments/Core/PaymentConfig.cs` | Stores URLs de Cloud Functions |
| `Payments/Core/PaymentBridge.cs` | Delegates bridge para desacoplar sistema de pagos del Firebase SDK |
| `Services/PaymentBridgeWiring.cs` | Conecta PaymentBridge delegates con AuthService, AnalyticsService, DatabaseService |
| `Services/DeepLinkService.cs` | Lee `AuthenticationService.Instance` para obtener uid en deep links |
| `Services/ATTService.cs` | Menciona "antes de inicializar Firebase Analytics" en comentario |

---

## 11. HERRAMIENTAS DE EDITOR (no van en APK/IPA)

| Archivo | Uso Firebase |
|---|---|
| `Editor/Tools/DebugMenuEditor.cs` | Pestaña "Firebase": muestra uid, FCM token, test events de Analytics |
| `DevTools/DebugManager.cs` | Pestaña Firebase en runtime debug overlay |

---

## 12. NOTAS DE SEGURIDAD

1. **Firebase Storage rules**: deben permitir solo `auth.uid == userId` para `avatars/{userId}/*`.
2. **GDPR gap**: `deleteUserData` Cloud Function elimina DB pero NO avatares de Storage. Pendiente.
3. **FCM**: desactivado en Editor (`#if !UNITY_EDITOR`).
4. **Anti-cheat doble**: MinigameBase (cliente) + Cloud Function `validateScore` (servidor, 1 submit/30s).
5. **Crashlytics**: no activo por defecto — requiere `FIREBASE_CRASHLYTICS` en Player Settings.
6. **MatchmakingService**: usa `ValueChanged` listener en tiempo real — limpiar con `UnsubscribeOpponentListener()` en `OnDestroy` y `OnApplicationQuit`. Ya implementado.
