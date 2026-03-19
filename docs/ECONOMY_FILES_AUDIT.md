# ECONOMY FILES AUDIT — DigitPark
> Generado: 2026-03-19 | Grep exhaustivo + Glob por carpeta | Sin agentes

Todos los archivos que tocan economía: monedas, gemas, compras, shop, premium, misiones, recompensas, logros, progresión, pagos reales, cash battle, cosméticos vendibles y backend.

---

## ÍNDICE DE CATEGORÍAS

1. [Moneda Virtual (Coins/Gems)](#1-moneda-virtual-coinsgems)
2. [Shop](#2-shop)
3. [Misiones Diarias](#3-misiones-diarias)
4. [Recompensas Diarias](#4-recompensas-diarias)
5. [Logros (Achievements)](#5-logros-achievements)
6. [Progresión (XP + Niveles)](#6-progresión-xp--niveles)
7. [Premium](#7-premium)
8. [Servicios de Economía (Services/)](#8-servicios-de-economía-services)
9. [Pagos Reales (Payments/)](#9-pagos-reales-payments)
10. [Cash Battle (Dinero Real)](#10-cash-battle-dinero-real)
11. [Triumph SDK](#11-triumph-sdk)
12. [Cosméticos Vendibles](#12-cosméticos-vendibles)
13. [Resultados de Partida (Earn/Lose)](#13-resultados-de-partida-earnlose)
14. [Modelos de Datos](#14-modelos-de-datos)
15. [Animaciones de Economía](#15-animaciones-de-economía)
16. [UI / Panels](#16-ui--panels)
17. [DevTools](#17-devtools)
18. [Editor — UIBuilders y PrefabBuilders](#18-editor--uibuilders-y-prefabbuilders)
19. [Config Assets (ScriptableObjects)](#19-config-assets-scriptableobjects)
20. [Backend — Cloud Functions](#20-backend--cloud-functions)
21. [Tests — Sistema de Pagos](#21-tests--sistema-de-pagos)
22. [Torneos DC (Soft-Currency Tournaments)](#22-torneos-dc-soft-currency-tournaments)
23. [Editor — Torneos DC UIBuilders](#23-editor--torneos-dc-uibuilders)
24. [Editor — WinPanels](#24-editor--winpanels)
25. [Editor — AutoAssigners de Economía](#25-editor--autoassigners-de-economía)

---

## 1. MONEDA VIRTUAL (COINS/GEMS)

| Archivo | Descripción |
|---|---|
| `Runtime/Economy/EconomyConstants.cs` | **FUENTE DE VERDAD.** Constantes centralizadas de todos los montos DC por modo de juego. Editar aquí rebalancea toda la economía automáticamente. 9 constantes: `COINS_PRACTICE_BASE=30`, `COINS_PRACTICE_PB_BONUS=15`, `COINS_SINGLEGAME_WIN=50`, `COINS_SINGLEGAME_LOSS=15`, `COINS_RANKED_WIN=15`, `COINS_RANKED_LOSS=5`, `COINS_RANKED_PERFECT_BONUS=25`, `COINS_RANKED_FWOTD_BONUS=50`, `COINS_TOURNAMENT_WIN=100`, `COINS_TOURNAMENT_LOSS=25`, `COINS_SPRINT_WIN=60`, `COINS_SPRINT_LOSS=15`. |
| `Runtime/Features/Monetization/Currency/CurrencyManager.cs` | Singleton central. Gestiona saldo de coins y gems, persiste en Firebase DB. Expone `AddCoins()`, `SpendCoins()`, `AddGems()`, `SpendGems()`. |
| `Runtime/Features/Monetization/Currency/CurrencyDisplayUI.cs` | UI que muestra saldo actualizado en tiempo real (suscribe eventos de CurrencyManager). |

---

## 2. SHOP

| Archivo | Descripción |
|---|---|
| `Runtime/Features/Monetization/Shop/ShopManager.cs` | Manager de la escena Shop. Carga categorías, items, filtra por tipo (Themes, Titles, Effects, Backgrounds, etc.). |
| `Runtime/Features/Monetization/Shop/ShopItemData.cs` | ScriptableObject/modelo de datos de un ítem de shop (nombre, precio, tipo, iconName, requiresPremium). |
| `Runtime/Features/Monetization/Shop/ShopItemUI.cs` | UI de una tarjeta de ítem. Muestra precio, estado owned/locked, botón comprar. Loguea Analytics. |
| `Runtime/Features/Monetization/Shop/WelcomePackService.cs` | Lógica del Welcome Pack (D1–D3). Verifica firstLogin, persiste flags en Firebase. Emite `welcome_pack_purchased` a Analytics. |
| `Runtime/Features/Monetization/Shop/WelcomePackUIController.cs` | UI del panel Welcome Pack. Muestra timer, beneficios, botón comprar. |
| `Runtime/Features/Monetization/Shop/DailyOfferUIController.cs` | UI de la oferta diaria rotativa. Conecta con DailyOfferService. |
| `Runtime/Features/Monetization/Shop/WinEffectPreviewPanel.cs` | Panel de preview de Win Effects antes de comprar. |

---

## 3. MISIONES DIARIAS

| Archivo | Descripción |
|---|---|
| `Runtime/Features/Monetization/DailyMissions/DailyMissionsManager.cs` | Manager de misiones diarias. Carga pool, trackea progreso, entrega rewards en Firebase. |
| `Runtime/Features/Monetization/DailyMissions/MissionCardUI.cs` | UI de una tarjeta de misión (nombre, progreso, reward coins). |
| `Runtime/Features/Monetization/Progression/MissionsManager.cs` | Manager alternativo/legacy de misiones (referencia cruzada con PlayerProgressionSystem). |
| `Runtime/Data/Missions/MissionDefinitionSO.cs` | ScriptableObject que define una misión (tipo, objetivo numérico, reward). |
| `Runtime/Data/Missions/MissionPoolSO.cs` | ScriptableObject que agrupa el pool de misiones disponibles por ciclo. |
| `Runtime/Data/Missions/MissionProgressReporter.cs` | Componente que reporta eventos de juego al sistema de misiones (puente entre juegos y misiones). |

---

## 4. RECOMPENSAS DIARIAS

| Archivo | Descripción |
|---|---|
| `Runtime/Features/Monetization/DailyRewards/DailyRewardsManager.cs` | Manager de daily rewards (login streak). Muestra rueda/calendario, entrega coins/gems/premium días. |
| `Runtime/Features/Monetization/DailyRewards/RewardDayItemUI.cs` | UI de un día individual en el calendario de recompensas (claimed/pending/future). |
| `Services/DailyRewardService.cs` | Servicio de persistence. Persiste estado de recompensas diarias en Firebase DB. |

---

## 5. LOGROS (ACHIEVEMENTS)

| Archivo | Descripción |
|---|---|
| `Runtime/Features/Monetization/Achievements/AchievementsManager.cs` | Manager de escena de logros. Lista todos los achievements, filtra por categoría. |
| `Runtime/Features/Monetization/Achievements/AchievementItemUI.cs` | UI de un logro individual (icono, nombre, descripción, progress bar, claimed). |
| `Runtime/Features/Monetization/Achievements/CategoryHeaderUI.cs` | Header de separador de categoría en la lista de logros. |
| `Runtime/Features/Monetization/Achievements/TrophyCardUI.cs` | UI de trofeo animado para achievements desbloqueados. |
| `Runtime/UI/Panels/TrophyProgressPanel.cs` | Panel que muestra progreso hacia el próximo trofeo. |
| `Runtime/UI/Notifications/AchievementNotificationManager.cs` | Muestra el toast/banner cuando se desbloquea un logro. |
| `Runtime/UI/Notifications/AchievementNotificationInitializer.cs` | Inicializa el sistema de notificaciones de logros en escena. |
| `Services/AchievementService.cs` | Servicio central. Valida condiciones de desbloqueo, persiste en Firebase DB, emite `achievement_unlocked` a Analytics. 52 logros registrados. |

---

## 6. PROGRESIÓN (XP + NIVELES)

| Archivo | Descripción |
|---|---|
| `Runtime/Features/Monetization/Progression/PlayerProgressionSystem.cs` | Singleton. Gestiona XP, nivel, tabla de rewards por nivel. Persiste en Firebase. Emite `level_up`. |
| `Runtime/Features/Monetization/Progression/LevelUpPanel.cs` | Panel modal que aparece al subir de nivel (animación, reward display). |
| `Runtime/Features/Monetization/Progression/LevelUpNotifier.cs` | Escucha eventos de `PlayerProgressionSystem` y dispara `LevelUpPanel`. |
| `Runtime/Features/Monetization/Progression/LevelUpRewardDisplay.cs` | UI de los rewards dentro del panel de level up. |

---

## 7. PREMIUM

| Archivo | Descripción |
|---|---|
| `Runtime/Features/Monetization/Premium/PremiumManager.cs` | Gestiona estado premium (tournaments, cashBattle, stylesPro). Restaura entitlements desde Firebase. Emite eventos de purchase. |
| `Runtime/UI/Panels/PremiumPanelUI.cs` | Panel de upgrade a premium. Muestra beneficios, precio, botón comprar. Integra con PaymentManager. |
| `Runtime/UI/Panels/PremiumCard.cs` | Tarjeta visual de un beneficio premium. |
| `DevTools/PremiumDebugController.cs` | Herramienta de debug para forzar estado premium en editor. |

---

## 8. SERVICIOS DE ECONOMÍA (Services/)

| Archivo | Descripción |
|---|---|
| `Services/AchievementService.cs` | Ver sección 5. |
| `Services/DailyRewardService.cs` | Ver sección 4. |
| `Services/DailyOfferService.cs` | Gestiona la oferta diaria rotativa. Lee/escribe en Firebase. Emite evento Analytics al aceptar. |
| `Services/RotatingContentService.cs` | Contenido rotativo de Shop (items que cambian periódicamente). Sincroniza en Firebase `players/{uid}/rotatingContent`. |
| `Services/WishlistService.cs` | Wishlist de ítems del Shop (nuevo, untracked). Persiste en local/Firebase la lista de ítems guardados. |
| `Services/EmoteService.cs` | Gestiona emotes: equipados, poseídos. Sincroniza en Firebase `players/{uid}/equippedEmotes` + `ownedEmotePacks`. |
| `Services/VictoryEffectService.cs` | Gestiona efectos de victoria: equipado, poseídos. Sincroniza en Firebase `players/{uid}/equippedVictoryEffect`. |
| `Services/PlayerTitleService.cs` | Gestiona títulos del jugador: equipado, poseídos. Sincroniza en Firebase `players/{uid}/equippedTitle`. |
| `Services/PlayerFrameService.cs` | Gestiona marcos de perfil: equipado, poseídos. Sincroniza en Firebase `players/{uid}/equippedFrame`. |
| `Services/PaymentBridgeWiring.cs` | Conecta los delegates de `PaymentBridge` con los servicios reales (AuthService, AnalyticsService, DatabaseService, CurrencyManager). |
| `Services/SecurePrefs.cs` | Almacenamiento seguro de preferencias sensibles (nuevo, untracked). Wrapper sobre PlayerPrefs con cifrado. |
| `Runtime/Features/Onboarding/CashBattleOnboardingManager.cs` | Onboarding específico de Cash Battle. Guía al jugador por el flujo KYC + wallet. Persiste progreso en PlayerPrefs. |
| `Services/Firebase/AnalyticsService.cs` | Loguea TODOS los eventos de economía a Firebase Analytics: `purchase`, `iap_purchase`, `gem_purchase`, `achievement_unlocked`, `level_up`, `welcome_pack_purchased`, `game_complete`, etc. Respeta ATT (iOS). |
| `Runtime/Core/Boot/BootManager.cs` | Entry point de la app. Inicializa `CurrencyManager` en arranque, maneja restore de coins/gems desde Firebase en reinstalación (líneas 278/343/368). |
| `Runtime/Features/MainMenu/MainMenuManager.cs` | Hub central del juego. Botones de Shop, DailyRewards y Premium. Suscribe `PremiumManager.OnPremiumStatusChanged` para actualizar badge de premium. Punto de entrada a todas las economía features. |
| `Runtime/Services/Firebase/DatabaseService.cs` | Capa de persistencia Firebase para TODOS los datos de economía: coins, gems, level, xp, achievements, streaks, equippedTheme, equippedBattleCard, dailyRewards. Expone `UpdatePlayerFields()`, `GetPlayerData()`, `SetCoins()`, `SetGems()`. |
| `Runtime/Features/Social/Profile/ProfileManager.cs` | Gestiona el perfil del jugador. Cobra DG por cambio de nombre: verifica `CurrencyManager.Instance.Gems` y llama `SpendGems(NAME_CHANGE_GEM_COST)` (líneas 738/776). |
| `Runtime/Features/Settings/SettingsManager.cs` | Gestión de ajustes. Cobra DG por cambio de nombre (líneas 788/827), integra `PremiumPanelUI` para upgrade, muestra badge premium en settings. |
| `Runtime/DevTools/DebugManager.cs` | Panel debug runtime. Tab "Premium": muestra estado de todos los productos premium, permite unlock/reset de `PremiumProduct.CreateTournaments`, `TournamentBundle`, `StylesPro`, `CashBattleCreate`. |

---

## 9. PAGOS REALES (Payments/)

### Core

| Archivo | Descripción |
|---|---|
| `Payments/Core/PaymentManager.cs` | Manager central de pagos. Decide qué proveedor usar (Stripe vs Apple IAP) vía RemoteConfigService. Bloquea usuarios anónimos. |
| `Payments/Core/PaymentBridge.cs` | Delegates bridge que desacoplan el sistema de pagos de Firebase SDK. Expone: `GetCurrentUserId`, `LogCustomEvent`, `ProcessGemsPurchase`, `UpdatePlayerFields`. |
| `Payments/Core/PaymentConfig.cs` | Stores URLs de Cloud Functions (stripeCreateCheckout, iapValidateReceiptUrl, etc.). Solo strings. |
| `Payments/Core/ProductCatalog.cs` | Catálogo de productos IAP (productId, precio, tipo). |
| `Payments/Core/IPaymentProvider.cs` | Interfaz que implementan Stripe y AppleIAP. |
| `Payments/Core/PaymentResult.cs` | Modelo de resultado de un pago. |
| `Payments/Core/PaymentEvents.cs` | Eventos C# del sistema de pagos (OnPurchaseCompleted, OnPurchaseFailed, etc.). |
| `Payments/Core/AbortReason.cs` | Enum de razones de abort para StripeAbortProtocol. |

### Stripe

| Archivo | Descripción |
|---|---|
| `Payments/Stripe/StripeCheckoutController.cs` | Abre checkout de Stripe via WebView/deep link. Usa PaymentBridge delegates. |
| `Payments/Stripe/StripeSessionPoller.cs` | Polling HTTP GET a `stripeSessionStatus` Cloud Function hasta confirmar pago. |
| `Payments/Stripe/StripePaymentProvider.cs` | Implementación de IPaymentProvider para Stripe. |
| `Payments/Stripe/StripeComplianceGuard.cs` | Guard que bloquea checkout de Stripe en plataformas no permitidas. |
| `Payments/Abort/StripeAbortProtocol.cs` | Protocolo de abort de emergencia. Llama a `adminForceSwitch` Cloud Function. |

### Apple IAP

| Archivo | Descripción |
|---|---|
| `Payments/AppleIAP/AppleReceiptValidator.cs` | Valida receipts de Apple via POST a `iapValidateReceipt` Cloud Function (fail-closed). |
| `Payments/AppleIAP/AppleIAPProvider.cs` | Implementación de IPaymentProvider para Apple IAP (Unity IAP). |
| `Services/AppleIAPBridge.cs` | Puente entre Unity Purchasing (IStoreListener) y PaymentManager. Inicia `UnityPurchasing.Initialize()`, captura `OnPurchaseComplete` y `OnPurchaseFailed`, delega a PaymentBridge. |

### Entitlements

| Archivo | Descripción |
|---|---|
| `Payments/Entitlements/EntitlementService.cs` | Sincroniza entitlements con Cloud Function `syncEntitlements`. Nivel C (HTTP). |
| `Payments/Entitlements/EntitlementRecord.cs` | Modelo de un entitlement (productId, timestamp, platform). |

### Feature Flags

| Archivo | Descripción |
|---|---|
| `Payments/FeatureFlags/RemoteConfigService.cs` | Lee Firebase Remote Config para decidir proveedor de pago activo (`payment_provider`, `stripe_enabled`, `apple_iap_enabled`, `triumph_enabled`). `#if FIREBASE_REMOTE_CONFIG`. Fallback a LocalFlagCache. |
| `Payments/FeatureFlags/LocalFlagCache.cs` | Cache local de feature flags (JSON). Fallback cuando Remote Config no está disponible. |
| `Payments/FeatureFlags/PaymentFeatureFlag.cs` | Enum/constantes de los keys de feature flags. |

### Compliance

| Archivo | Descripción |
|---|---|
| `Payments/Compliance/TriumphIsolationGuard.cs` | Bloquea que transacciones de Triumph lleguen al sistema cosmético de DigitPark. |
| `Payments/Compliance/VersionGuard.cs` | Bloquea compras si la versión de la app es inferior a la mínima permitida. |

### UI de Pagos

| Archivo | Descripción |
|---|---|
| `Payments/UI/PaymentLoadingOverlay.cs` | Overlay de carga durante el flujo de pago. |
| `Payments/UI/PaymentErrorDialog.cs` | Dialog de error de pago con mensaje localizado. |

---

## 10. CASH BATTLE (DINERO REAL)

### Hub / Core

| Archivo | Descripción |
|---|---|
| `Features/CashBattle/Hub/CashBattleManager.cs` | Manager principal del hub de Cash Battle. Coordina acceso (KYC, wallet, geo). |
| `Features/CashBattle/Hub/CashMatchmakingManager.cs` | Matchmaking para partidas 1v1 de dinero real. Carga avatares via AvatarService (Firebase Storage). |
| `Features/CashBattle/Hub/CashBattle1v1Manager.cs` | Manager de partida 1v1 de dinero real activa. |
| `Features/CashBattle/Hub/LocationRestrictionOverlay.cs` | Overlay que bloquea acceso por restricción geográfica (Triumph geo-fencing). |

### Wallet

| Archivo | Descripción |
|---|---|
| `Features/CashBattle/Wallet/WalletManager.cs` | Manager de la escena Wallet. Muestra saldo, historial, botones deposit/withdraw. Loguea a Analytics. |
| `Features/CashBattle/Wallet/WalletData.cs` | Modelo de datos del wallet (balance, pending, transactions). |
| `Features/CashBattle/Wallet/CashWalletSceneController.cs` | Controller de escena Wallet (wiring UI ↔ WalletManager). |
| `Features/CashBattle/Wallet/TransactionItemUI.cs` | UI de una transacción individual en el historial. |
| `Features/CashBattle/Wallet/DepositOptionUI.cs` | UI de una opción de depósito (cantidad, botón). |

### History

| Archivo | Descripción |
|---|---|
| `Features/CashBattle/History/HistoryManager.cs` | Carga historial de partidas cash. |
| `Features/CashBattle/History/HistoryData.cs` | Modelo de datos de una entrada de historial (rival, resultado, prize, fecha). |
| `Features/CashBattle/History/CashHistorySceneController.cs` | Controller de escena de historial cash. |
| `Features/CashBattle/History/HistoryEntryItemUI.cs` | UI de una entrada de historial en la lista. |

### Tournaments

| Archivo | Descripción |
|---|---|
| `Features/CashBattle/Tournaments/CashTournamentsManager.cs` | Browser de torneos de dinero real. |
| `Features/CashBattle/Tournaments/CashTournamentLobbyManager.cs` | Lobby de torneo cash. |
| `Features/CashBattle/Tournaments/CashTournamentCreateManager.cs` | Creación de torneo cash. |
| `Features/CashBattle/Tournaments/CashTournamentResultsPanelController.cs` | Panel de resultados de torneo cash (posición, prize). |

### Profile / Results

| Archivo | Descripción |
|---|---|
| `Features/CashBattle/Profile/CashProfileSceneController.cs` | Perfil del jugador en el contexto Cash Battle. Lee de Firebase. |
| `Features/CashBattle/Results/CashBattleResultPanelController.cs` | Panel de resultado de partida cash (win/loss, prize ganado/perdido). |

---

## 11. TRIUMPH SDK

Backend de Cash Battle real-money. **Zero nexo con Firebase desde Unity** — sistema de identidad independiente.

### Servicios reales (stubs, SDK no integrado aún)

| Archivo | Descripción |
|---|---|
| `Services/Triumph/TriumphManager.cs` | Punto único de contacto con Triumph SDK. Mock activo (`_isEnabled = false`). Gestiona KYC, Wallet, Matchmaking, Geo-fencing, Score submission. |
| `Services/Triumph/TriumphServices.cs` | 4 stubs: `TriumphKYCService`, `TriumphWalletService`, `TriumphMatchmakingService`, `TriumphTournamentService`. Todos retornan `SDK_NOT_INTEGRATED` — fail-closed. |

### Mocks (para desarrollo)

| Archivo | Descripción |
|---|---|
| `Services/Mock/MockKYCService.cs` | Simula KYC completo (edad, identidad, rechazo). Estado en PlayerPrefs. |
| `Services/Mock/MockWalletService.cs` | Simula wallet (balance, depósitos, retiros). Sin red. |
| `Services/Mock/MockMatchmakingService.cs` | Simula búsqueda de partida cash. Sin red. |
| `Services/Mock/MockTournamentService.cs` | Simula torneos cash. Sin red. |

### Interfaces

| Archivo | Descripción |
|---|---|
| `Services/Interfaces/IKYCService.cs` | Define `VerifyAge()`, `StartIdentityVerification()`, `KYCStatus`, `UserVerificationInfo`. |
| `Services/Interfaces/IWalletService.cs` | Define `Deposit()`, `Withdraw()`, `RefreshBalance()`, `GetTransactionHistory()`. |
| `Services/Interfaces/IMatchmakingService.cs` | Define `FindMatch()`, `SubmitScore()`, `CancelSearch()`. |
| `Services/Interfaces/ITournamentService.cs` | Define `GetAvailableTournaments()`, `JoinTournament()`, `SubmitTournamentScore()`. |

---

## 12. COSMÉTICOS VENDIBLES

Ítems que se venden en el Shop y tienen persistencia en Firebase.

| Archivo | Descripción |
|---|---|
| `Features/Cosmetics/BattleCards/BattleCardService.cs` | Gestiona BattleCard equipada/poseídas. Lee directo de Firebase DB (`players/{uid}/equippedBattleCard`). |
| `Features/Cosmetics/BattleCards/BattleCardData.cs` | Modelo de datos de una BattleCard (id, name, rarity, colors). |
| `Features/Cosmetics/BattleCards/BattleCardApplier.cs` | Aplica la BattleCard activa a la UI de tarjeta del jugador. |
| `Features/Cosmetics/Backgrounds/BackgroundPatternManager.cs` | Gestiona fondo de perfil: sincroniza en Firebase `players/{uid}/equippedBackground`. |
| `Features/Cosmetics/Backgrounds/BackgroundPatternReceiver.cs` | Componente que recibe y aplica el fondo activo a un panel de UI. |
| `Themes/ThemeManager.cs` | Gestiona tema visual activo (Free/Chromatic/Premium). Sincroniza en Firebase `players/{uid}/equippedTheme`. |
| `Themes/ThemeData.cs` | ScriptableObject de datos de un tema (colores, nombre, precio, requiresPremium). |
| `Runtime/UI/Components/ThemeSelector.cs` | Panel de selección de tema. Lanza el flujo de compra via IAP (`UnityPurchasing.BuyProduct`) para temas de pago. Muestra estado owned/locked. |

---

## 13. RESULTADOS DE PARTIDA (EARN/LOSE)

Archivos donde el jugador gana o pierde moneda/XP al terminar una partida.

| Archivo | Descripción |
|---|---|
| `Features/Games/Core/GameContext.cs` | Objeto de contexto que transporta datos económicos entre estados de juego. Propiedades: `EntryFee` (decimal), `BetAmount` (int), `BetCurrencyType` (enum DC/free). Pasado de BetSelectionPanel → GameSessionManager. |
| `Features/Games/Core/GameSessionManager.cs` | **CRÍTICO.** Orquesta toda la sesión de juego. `CalculatePostGameReward()` define los DC por modo (Practice +30, SingleGame win +50/loss +15, Tournament win +100/loss +25, CognitiveSprint win +60/loss +15). Llama a `CurrencyManager.SettleBet()` y `PlayerProgressionSystem.AddGameXP()`. |
| `Features/Games/Navigation/BetSelectionPanel.cs` | **CRÍTICO.** Gestiona las apuestas antes de cada partida. Presets: 50/100/250/500/1000 DC; custom 5–5000 DC en múltiplos de 5. Llama a `CurrencyManager.EscrowCoins()` al iniciar y `SettleBet()` al terminar. |
| `Features/Games/Results/OnlineResultManager.cs` | Manager de resultados online. Procesa win/loss, distribuye coins/XP, loguea `game_complete`. |
| `Features/Games/Results/OnlineResultPanelController.cs` | UI del panel de resultados online (score, rival, coins earned). |
| `Features/Games/Results/SprintSummaryPanelController.cs` | Panel de resumen de Cognitive Sprint (XP, misiones completadas, mejor score). |
| `Features/Games/Results/ResultPanelManager.cs` | Manager base de paneles de resultado. |
| `Features/Games/Results/WinPanelController.cs` | Panel específico de victoria con animación. |
| `Features/Games/Results/WinCelebrationAnimator.cs` | Animación de celebración al ganar. |
| `Features/Games/Results/UISparkleEffect.cs` | Efecto de partículas sparkle en paneles de resultado/logros. |
| `Features/Games/Results/ComboVisualController.cs` | Visualización de combos durante partida (multiplica coins). |

---

## 14. MODELOS DE DATOS

| Archivo | Descripción |
|---|---|
| `Runtime/Data/PlayerData.cs` | Modelo completo del jugador. Incluye: coins, gems, level, xp, premium flags, equippedTheme, equippedBattleCard, achievements, streaks. `ToFirebaseDictionary()` para serializar. |
| `Runtime/Data/PlayerSettings.cs` | Preferencias del jugador (sonido, vibración, idioma). Persiste en PlayerPrefs. |
| `Runtime/Data/TournamentData.cs` | Modelo de torneo. Incluye entry fee, prize pool, participant list. Serializa para Firebase. |
| `Runtime/Data/FrameData.cs` | Modelo de marco de perfil. Contiene `productId` (IAP), precio, nombre, rareza. |
| `Runtime/Data/MatchHistoryData.cs` | Modelo de historial de partida. Incluye resultado, coins ganados, rival, fecha. |
| `Runtime/Data/Missions/MissionDefinitionSO.cs` | Ver sección 3. |
| `Runtime/Data/Missions/MissionPoolSO.cs` | Ver sección 3. |
| `Runtime/Data/Missions/MissionProgressReporter.cs` | Ver sección 3. |
| `Features/CashBattle/Wallet/WalletData.cs` | Ver sección 10. |
| `Features/CashBattle/History/HistoryData.cs` | Ver sección 10. |

---

## 15. ANIMACIONES DE ECONOMÍA

Componentes de animación directamente ligados a flujos de coins/gems/rewards.

| Archivo | Descripción |
|---|---|
| `Runtime/Animations/Animators/CurrencyAnimator.cs` | Animación de contador de monedas (tween de número al ganar/gastar DC o DG). |
| `Runtime/Animations/Animators/RewardClaimAnimator.cs` | Animación de claim de recompensa (partículas + tween al reclamar daily reward, logro o level up). |

---

## 16. UI / PANELS

| Archivo | Descripción |
|---|---|
| `Runtime/UI/Panels/PremiumPanelUI.cs` | Panel de upgrade premium. Integra con PaymentManager. Muestra badge "ACQUIRED" si ya es premium. |
| `Runtime/UI/Panels/PremiumCard.cs` | Tarjeta visual de un beneficio premium. |
| `Runtime/UI/Panels/StylesProPromptPanel.cs` | Panel de estilos premium. Muestra pricing del bundle de temas (15 temas, precio tachado + descuento 30%). Lanza flujo de compra premium. |
| `Runtime/UI/Panels/TrophyProgressPanel.cs` | Panel de progreso hacia trofeo (% completado, reward próximo). |
| `Runtime/UI/Notifications/AchievementNotificationManager.cs` | Toast de logro desbloqueado (ícono + nombre animados). |
| `Runtime/UI/Notifications/AchievementNotificationInitializer.cs` | Inicializa el sistema de notificaciones de logros en escena. |
| `Runtime/UI/Builders/DailyRewardPanelBuilder.cs` | Builder runtime del panel de daily rewards (construye la grilla de días en escena). |
| `Runtime/UI/Components/ThemeDropdownController.cs` | Dropdown de selección de tema. Gate premium: bloquea temas de pago si no es premium/owned. |
| `Payments/UI/PaymentLoadingOverlay.cs` | Ver sección 9. |
| `Payments/UI/PaymentErrorDialog.cs` | Ver sección 9. |

---

## 17. DEVTOOLS

| Archivo | Descripción |
|---|---|
| `DevTools/PremiumDebugController.cs` | Fuerza estado premium en runtime para testing. Solo en Editor o builds de debug. |
| `DevTools/AchievementDebugPanel.cs` | Panel de debug para desbloquear logros manualmente y probar el sistema. |
| `DevTools/DebugManager.cs` | Ver sección 8 — panel debug con tab Premium + unlock/reset de productos premium. |

---

## 18. EDITOR — UIBuilders y PrefabBuilders

No van en APK/IPA. Solo en el proceso de construcción de escenas/prefabs.

### Monetization

| Archivo | Descripción |
|---|---|
| `Editor/Monetization/AchievementsUIBuilder.cs` | Construye la escena de logros programáticamente. |
| `Editor/Monetization/DailyMissionsUIBuilder.cs` | Construye la UI de misiones diarias. |
| `Editor/Monetization/DailyRewardsPremiumUIBuilder.cs` | Construye la UI de daily rewards (premium variant). |
| `Editor/Monetization/ShopPremiumUIBuilder.cs` | Construye la escena del Shop. |
| `Editor/Monetization/MonetizationPrefabBuilder.cs` | Builder general de prefabs de monetización. |
| `Editor/Monetization/ShopSceneConnector.cs` | Conecta referencias Inspector de la escena Shop. |
| `Editor/Monetization/ShopEffectsTabBuilder.cs` | Construye la pestaña de efectos/cosméticos en el Shop. |
| `Editor/Monetization/BackgroundShopItemBuilder.cs` | Construye ítems de Backgrounds en el Shop (nuevo, untracked). |
| `Editor/Monetization/LevelUpPanelBuilder.cs` | Construye el panel de level up. |
| `Editor/Monetization/AchievementCardStateToggler.cs` | Herramienta para alternar estado claimed/unclaimed en tarjetas de logros en Editor. |
| `Editor/Monetization/AchievementToastUIBuilder.cs` | Construye el prefab de toast de logro. |
| `Editor/Monetization/AchievementDebugTester.cs` | Tester de desbloqueo de logros en Editor. |
| `Editor/Monetization/AchievementIconImporter.cs` | Importa y configura los 52 íconos de logros en Resources. |
| `Editor/Monetization/MissionCardPrefabBuilder.cs` | Construye el prefab de tarjeta de misión. |
| `Editor/Monetization/MissionSystemCreator.cs` | Crea los ScriptableObjects del sistema de misiones. |

### Cash Battle

| Archivo | Descripción |
|---|---|
| `Editor/CashBattle/CashBattleUIBuilder.cs` | Construye la UI del hub de Cash Battle. |
| `Editor/CashBattle/CashBattle1v1UIBuilder.cs` | Construye la UI de partida 1v1. |
| `Editor/CashBattle/CashBattlePrefabBuilder.cs` | Builder de prefabs de Cash Battle. |
| `Editor/CashBattle/CashMatchmakingUIBuilder.cs` | Construye la UI de matchmaking cash. |
| `Editor/CashBattle/CashProfileUIBuilder.cs` | Construye la UI del perfil cash. |
| `Editor/CashBattle/CashHistoryUIBuilder.cs` | Construye la UI del historial cash. |
| `Editor/CashBattle/CashTournamentResultsUIBuilder.cs` | Construye la UI de resultados de torneo cash. |
| `Editor/CashBattle/CashTournamentCreateUIBuilder.cs` | Construye la UI de creación de torneo cash. |
| `Editor/CashBattle/CashTournamentLobbyUIBuilder.cs` | Construye la UI del lobby de torneo cash. |
| `Editor/CashBattle/CashTournamentsUIBuilder.cs` | Construye la UI del browser de torneos cash. |
| `Editor/CashBattle/WalletUIBuilder.cs` | Construye la UI del wallet. |
| `Editor/CashBattle/WalletPrefabGenerator.cs` | Genera prefabs de componentes del wallet. |

### AutoAssigners

| Archivo | Descripción |
|---|---|
| `Editor/AutoAssigners/Games/BetSelectionReferenceAssigner.cs` | Asigna automáticamente las referencias Inspector de BetSelectionPanel (preset buttons, custom slider, etc.). |
| `Editor/AutoAssigners/Monetization/ShopReferenceAssigner.cs` | Asigna automáticamente las referencias Inspector del ShopManager (tab buttons, item grid, etc.). |

### Payments (Editor)

| Archivo | Descripción |
|---|---|
| `Editor/Payments/PaymentDebugWindow.cs` | Ventana Editor para simular compras, forzar entitlements y probar flujos de pago sin transacción real. |
| `Editor/Payments/BuildProfileSwitcher.cs` | Herramienta para cambiar entre perfil Stripe y Apple IAP en el proceso de build (define PAYMENT_PROVIDER_STRIPE / APPLE). |

### Settings

| Archivo | Descripción |
|---|---|
| `Editor/Settings/SettingsUIBuilder.cs` | Construye la UI de Settings. Incluye sección Premium (tarjeta, overlay, badge), icono de DG en display de costo de cambio de nombre, filtro de temas por tipo (free/earnable/premium gold lock). |
| `Editor/AutoAssigners/Core/SettingsReferenceAssigner.cs` | Asigna referencias Inspector de SettingsManager: premium button, premium badge, premium panel, DG icon. |
| `Editor/AutoAssigners/Core/MainMenuReferenceAssigner.cs` | Asigna referencias Inspector de MainMenuManager: shopButton, dailyRewardsButton, premiumButton, premiumBadge, premiumPanel. |

### Tools (economía)

| Archivo | Descripción |
|---|---|
| `Editor/Tools/CurrencyHeaderBarHelper.cs` | Construye la barra de header con CoinsPill + GemsPill en todas las escenas que la requieren. |

---

## 19. CONFIG ASSETS (ScriptableObjects)

### Shop — Backgrounds (13 assets)

Path: `Assets/_Project/Resources/Configs/Shop/Backgrounds/`

| Asset | Patrón visual |
|---|---|
| `bg_neural.asset` | Red neuronal |
| `bg_circuit.asset` | Circuito PCB |
| `bg_dna.asset` | Doble hélice ADN |
| `bg_constellation.asset` | Constelaciones |
| `bg_fingerprint.asset` | Huella dactilar |
| `bg_triangles.asset` | Triángulos geométricos |
| `bg_waveform.asset` | Onda de audio |
| `bg_digits.asset` | Dígitos numéricos |
| `bg_binary.asset` | Código binario |
| `bg_hexgrid.asset` | Cuadrícula hexagonal |
| `bg_crosshatch.asset` | Tramado cruzado |
| `bg_grid.asset` | Cuadrícula simple |
| `bg_dots.asset` | Patrón de puntos |

---

## 20. BACKEND — CLOUD FUNCTIONS

**`functions/src/index.ts`** — Firebase Admin SDK + Firestore + Stripe + Apple IAP

| Endpoint | Auth | Economía |
|---|---|---|
| `stripeCreateCheckout` | ✅ Firebase token | Crea sesión de pago Stripe → entitlement |
| `stripeWebhook` | ✅ Stripe signature | Confirma pago → granta entitlement en Firestore |
| `stripeSessionStatus` | ⚠️ Sin auth | Polling estado de sesión (low risk) |
| `iapValidateReceipt` | ✅ Firebase token | Valida receipt Apple IAP → granta entitlement |
| `getEntitlements` | ✅ Firebase token | Lista entitlements activos del usuario |
| `checkEntitlement` | ✅ Firebase token | Verifica si un producto está activo |
| `syncEntitlements` | ✅ Firebase token | Sincronización bidireccional entitlements |
| `adminForceSwitch` | ✅ adminKey Firestore | Abort protocol: cambia proveedor de pago de emergencia |
| `validateScore` | ✅ Firebase token | Anti-cheat + rate limit (1 submit/30s/user) |
| `deleteUserData` | ✅ Firebase token | GDPR: elimina todos los datos del jugador (⚠️ no elimina avatares de Storage) |
| `paymentsHealth` | — | Health check del sistema de pagos |

---

## 21. TESTS — SISTEMA DE PAGOS

Suite de tests del sistema de pagos (Unity Test Runner). No van en APK/IPA.

| Archivo | Descripción |
|---|---|
| `Tests/Payments/PaymentManagerTests.cs` | Tests del PaymentManager: bloqueo de usuarios anónimos, selección de proveedor, flujo de compra. |
| `Tests/Payments/FeatureFlagTests.cs` | Tests de RemoteConfigService y LocalFlagCache: valores por defecto, fallback, lectura de flags. |
| `Tests/Payments/TriumphIsolationTests.cs` | Tests que verifican que transacciones de Triumph no cruzan al sistema cosmético de DigitPark. |
| `Tests/Payments/EntitlementServiceTests.cs` | Tests de EntitlementService: sync, verificación y persistencia de entitlements. |
| `Tests/Payments/ComplianceGuardTests.cs` | Tests de StripeComplianceGuard y VersionGuard: bloqueo por plataforma y versión mínima. |
| `Tests/Payments/StripeAppleIAPSwitchTests.cs` | Tests del switch dinámico Stripe ↔ Apple IAP via feature flag (RemoteConfigService). |

---

## 22. TORNEOS DC (Soft-Currency Tournaments)

Torneos con moneda virtual (DigitCoins). Entry fee en DC, prize pool en DC. Distinto del sistema CashBattle (dinero real).

| Archivo | Descripción |
|---|---|
| `Runtime/Features/Tournaments/TournamentManager.cs` | Manager principal de torneos DC. Gestiona creación, join, entry fee (`entryFee`), prize pool, score submission y resultados. |
| `Runtime/Features/Tournaments/TournamentsBrowserManager.cs` | Browser de torneos disponibles. Muestra entry fee y prize pool por torneo. |
| `Runtime/Features/Tournaments/TournamentLobbyManager.cs` | Lobby de torneo DC. Muestra participantes, prize pool, countdown. |
| `Runtime/Features/Tournaments/TournamentCreateManager.cs` | Creación de torneo DC. Permite definir entry fee, nombre, número de participantes. |
| `Runtime/Features/Tournaments/TournamentResultPanelController.cs` | Panel de resultados de torneo. Muestra posición final y prize ganado en DC. |
| `Runtime/Features/Tournaments/PrizeRowItemUI.cs` | UI de una fila de premio en la tabla de premios del torneo (posición → reward DC). |
| `Runtime/Features/Tournaments/TournamentItemUI.cs` | UI de un ítem de torneo en el browser (nombre, entry fee, plazas, estado). |

---

## 23. EDITOR — Torneos DC UIBuilders

Builders de UI para el sistema de torneos de moneda virtual. Comparten entryFee y prize pool display.

| Archivo | Descripción |
|---|---|
| `Editor/Tournaments/TournamentsBrowserUIBuilder.cs` | Construye la UI del browser de torneos DC (lista de torneos, entry fee, plazas). |
| `Editor/Tournaments/TournamentLobbyUIBuilder.cs` | Construye la UI del lobby de torneo DC (participantes, prize pool, countdown). |
| `Editor/Tournaments/TournamentCreateUIBuilder.cs` | Construye la UI de creación de torneo DC (nombre, entry fee, número de jugadores). |

---

## 24. EDITOR — WinPanels

Builders de los paneles de resultado de partida. Construyen la UI donde se muestran coins ganados/perdidos.

| Archivo | Descripción |
|---|---|
| `Editor/WinPanels/WinPanelUIBuilder.cs` | Construye el panel de victoria (coins earned, animación sparkle). |
| `Editor/WinPanels/WinPanelInlineBuilder.cs` | Versión inline del win panel (embedded en escena de juego). |
| `Editor/WinPanels/OnlineResultPanelUIBuilder.cs` | Construye el panel de resultado online (win/loss, DC ganados, rival). |
| `Editor/WinPanels/TournamentResultPanelUIBuilder.cs` | Construye el panel de resultado de torneo DC (posición, prize). |
| `Editor/WinPanels/CashBattleResultPanelUIBuilder.cs` | Construye el panel de resultado de Cash Battle (prize real ganado/perdido). |
| `Editor/WinPanels/SprintSummaryPanelUIBuilder.cs` | Construye el panel de resumen de Cognitive Sprint (XP, misiones, mejor score). |

---

## 25. EDITOR — AutoAssigners de Economía

AutoAssigners que conectan referencias Inspector de componentes de economía. No van en APK/IPA.

### Monetization

| Archivo | Descripción |
|---|---|
| `Editor/AutoAssigners/Monetization/AchievementsReferenceAssigner.cs` | Asigna referencias de AchievementsManager (lista, categorías, badges). |
| `Editor/AutoAssigners/Monetization/DailyMissionsReferenceAssigner.cs` | Asigna referencias de DailyMissionsManager (cards, bonus button, progress). |
| `Editor/AutoAssigners/Monetization/DailyRewardsReferenceAssigner.cs` | Asigna referencias de DailyRewardsManager (calendario, claim button, streak). |
| `Editor/AutoAssigners/AchievementIconAssigner.cs` | Asigna los 52 iconos de logros a sus AchievementItemUI en escena. |

### Tournaments

| Archivo | Descripción |
|---|---|
| `Editor/AutoAssigners/Tournaments/TournamentsBrowserReferenceAssigner.cs` | Asigna referencias del browser de torneos DC. |
| `Editor/AutoAssigners/Tournaments/TournamentCreateReferenceAssigner.cs` | Asigna referencias del panel de creación de torneo DC. |
| `Editor/AutoAssigners/Tournaments/TournamentLobbyReferenceAssigner.cs` | Asigna referencias del lobby de torneo DC. |

### Cash Battle

| Archivo | Descripción |
|---|---|
| `Editor/AutoAssigners/CashBattle/CashBattleHubReferenceAssigner.cs` | Asigna referencias del hub principal de Cash Battle. |
| `Editor/AutoAssigners/CashBattle/CashBattle1v1ReferenceAssigner.cs` | Asigna referencias de la partida 1v1 Cash Battle. |
| `Editor/AutoAssigners/CashBattle/CashMatchmakingReferenceAssigner.cs` | Asigna referencias del matchmaking Cash Battle. |
| `Editor/AutoAssigners/CashBattle/CashHistoryReferenceAssigner.cs` | Asigna referencias del historial Cash Battle. |
| `Editor/AutoAssigners/CashBattle/CashTournamentsReferenceAssigner.cs` | Asigna referencias del browser de torneos Cash. |
| `Editor/AutoAssigners/CashBattle/CashTournamentLobbyReferenceAssigner.cs` | Asigna referencias del lobby de torneo Cash. |
| `Editor/AutoAssigners/CashBattle/CashWalletReferenceAssigner.cs` | Asigna referencias del wallet Cash Battle (saldo, botones deposit/withdraw). |
| `Editor/AutoAssigners/CashBattle/CashTournamentCreateReferenceAssigner.cs` | Asigna referencias del panel de creación de torneo Cash. |

### Onboarding

| Archivo | Descripción |
|---|---|
| `Editor/AutoAssigners/Onboarding/OnboardingReferenceAssigner.cs` | Asigna referencias del flujo de onboarding principal. |
| `Editor/AutoAssigners/Onboarding/CashBattleOnboardingReferenceAssigner.cs` | Asigna referencias del onboarding específico de Cash Battle (KYC + wallet). |

### Cosmetics

| Archivo | Descripción |
|---|---|
| `Editor/Cosmetics/BattleCards/BattleCardCatalogBuilder.cs` | Construye el catálogo de BattleCards en el Shop (crea y configura todos los ítems vendibles). |
| `Editor/Effects/VictoryEffectPrefabBuilder.cs` | Construye los prefabs de Victory Effects vendibles en el Shop (cosmético de victoria). |
| `Editor/Themes/ThemeCollectionCreator.cs` | Crea los ScriptableObjects `ThemeData` para los 4 temas vendibles/desbloqueables (Emerald, Electric Blue, Electric Violet, Monochrome). |

---

## RESUMEN NUMÉRICO

| Categoría | Archivos Runtime | Archivos Editor / Tests |
|---|---|---|
| Moneda virtual (+ EconomyConstants) | 3 | — |
| Shop | 7 | 8 |
| Misiones diarias | 6 | 2 |
| Recompensas diarias | 3 | 1 |
| Logros | 8 | 5 |
| Progresión XP | 4 | 1 |
| Premium | 4 | — |
| Servicios de economía (+ Boot, MainMenu, DB, Profile, Settings, Onboarding) | 19 | — |
| Pagos reales (Payments/) | 19 | 2 |
| Cash Battle | 19 | 12 |
| Triumph SDK | 8 | — |
| Cosméticos vendibles (+ ThemeSelector, + ThemeDropdown) | 10 | — |
| Resultados de partida (+ GameContext) | 11 | — |
| Modelos de datos | 10 | — |
| Animaciones de economía | 2 | — |
| UI Panels (+ DailyRewardPanelBuilder, + StylesProPromptPanel) | 10 | — |
| DevTools (+ DebugManager) | 3 | — |
| Editor UIBuilders + Tools (+ Settings, + Effects, + Themes) | — | 38 |
| Config Assets | 13 assets | — |
| Backend | 1 (index.ts) | — |
| Tests | — | 6 |
| Torneos DC | 7 | 3 UIBuilders |
| Editor WinPanels | — | 6 |
| Editor AutoAssigners economía (+ Settings, + MainMenu) | — | 22 |
| **TOTAL** | **~174** | **~105** |
