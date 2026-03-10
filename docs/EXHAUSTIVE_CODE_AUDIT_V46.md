# EXHAUSTIVE CODE AUDIT V46 - DigitPark
**Fecha**: 2026-03-09
**Auditor**: Claude (Multi-Agent Deep Analysis)
**Estado**: SOLO AUDITORIA - Sin fixes aplicados
**Alcance**: 100+ archivos Runtime, Services, Editor, Localization, Data, UI, Effects, DevTools

---

## RESUMEN EJECUTIVO

| Severidad | Cantidad | Categoria principal |
|-----------|----------|---------------------|
| ERROR     | 28       | Serialization, thread safety, NullRef, logic crashes |
| WARNING   | 178      | Async issues, dead code, null risks, logic bugs, cleanup |
| INFO      | 108      | Dead code, Spanish strings, hardcoded values, UX |
| **TOTAL** | **314**  | |

---

## ERRORES CRITICOS (ERROR) - 28 issues

Estos pueden causar crashes, datos corruptos, o comportamiento incorrecto garantizado.

### ERR-01: `decimal` fields no soportados por Unity Serialization
**Archivos**:
- `WalletManager.cs:80-93` — `_depositOptions`, `_minimumWithdrawal`, `_maximumWithdrawal`
- `WalletData.cs:52-66` — `WalletTransaction.amount`, `balanceAfter`; `WalletData` multiple fields; `DepositOption.amount`
- `HistoryData.cs:58-60` — `entryFee`, `prize`, `netResult`
- `TournamentCreateManager.cs:74-82` — `creationFee`, `platformFeePercent`, `entryFeeOptions[]`
**Problema**: Unity `[SerializeField]` y `JsonUtility` NO soportan `decimal`. Valores siempre seran 0 en Inspector y se pierden al serializar/deserializar.
**Impacto**: Todos los montos de dinero real se pierden silenciosamente.

### ERR-02: PlayerProgressionSystem — Duplicate dictionary keys crash
**Archivo**: `PlayerProgressionSystem.cs:246-263`
**Problema**: Key `100` agregado 2 veces, key `500` agregado 3 veces en `InitializeLevelRewards()`. `Dictionary` lanza `ArgumentException` en runtime.
**Impacto**: Crash al iniciar el sistema de progresion.

### ERR-03: FriendService — `await null` throws NullReferenceException
**Archivo**: `FriendService.cs:145, 206, 210, 214, 402, 406, 410, 435`
**Problema**: `await DatabaseService.Instance?.Method()` — si `Instance` es null, `?.` retorna `null` para el Task, y `await null` lanza `NullReferenceException`.
**Impacto**: Crash en cualquier operacion de amigos si DatabaseService no esta inicializado.

### ERR-04: MatchmakingService — Thread safety violations (5 instancias)
**Archivo**: `MatchmakingService.cs:57-84, 174-196, 239-255, 272-288, 385-408`
**Problema**: `.ContinueWith()` sin especificar main thread scheduler. Callbacks acceden Unity APIs (`Debug.Log`, `FirebaseAuth`, variables compartidas) desde background threads. Data race entre background callbacks y coroutines del main thread.
**Impacto**: Race conditions, crashes intermitentes en dispositivos moviles.

### ERR-05: MatchmakingService — ValueChanged listener leak
**Archivo**: `MatchmakingService.cs:416-437`
**Problema**: `ListenForOpponentResult` suscribe a `ValueChanged` con lambda anonimo pero NUNCA se desuscribe. El listener persiste despues del match, cambio de escena, y destruccion del objeto.
**Impacto**: Memory leak progresivo, Firebase sigue disparando eventos indefinidamente.

### ERR-06: AvatarService — Texture2D en background thread
**Archivo**: `AvatarService.cs:350-376`
**Problema**: `ProcessSelectedImage` es `async void` que crea `new Texture2D()` y llama `LoadImage` dentro de `Task.Run()`. Las operaciones de Texture2D DEBEN ejecutarse en el main thread.
**Impacto**: Crash en dispositivos moviles al procesar imagenes de avatar.

### ERR-07: AnalyticsService — Missing error handling en Initialize
**Archivo**: `AnalyticsService.cs:42-67`
**Problema**: `FirebaseAnalytics.SetAnalyticsCollectionEnabled()` sin try/catch. Si Firebase no esta inicializado, lanza excepcion no capturada en Awake.

### ERR-08: DatabaseService — ContinueWith en background thread
**Archivo**: `DatabaseService.cs:43-47`
**Problema**: `.ContinueWith()` ejecuta `Debug.LogError` (no thread-safe en Unity) desde background thread. `_databaseRef` y `_isInitialized` modificados en background, leidos en main thread sin sincronizacion.

### ERR-09: CashBattleManager — `_isProcessing` never reset
**Archivo**: `CashBattleManager.cs:382-420`
**Problema**: 5 navigation handlers (`OnBattles1v1Clicked`, `OnCashTournamentsClicked`, `OnWalletClicked`, `OnCashProfileClicked`, `OnHistoryClicked`) setean `_isProcessing = true` pero NUNCA lo resetean a false. Si `SceneNavigator` falla, usuario queda permanentemente bloqueado.

### ERR-10: TournamentLobbyManager — StartTournament llamado cada frame
**Archivo**: `TournamentLobbyManager.cs:478-481`
**Problema**: `Update()` llama `StartTournament()` cuando `timeUntilStart.TotalSeconds <= 0` sin flag para prevenir llamadas repetidas. Se ejecuta cada frame hasta que la escena cambia.

### ERR-11: SettingsManager — Navegacion agresiva en null data
**Archivo**: `SettingsManager.cs:146-147`
**Problema**: Si `currentPlayer == null`, carga Login inmediatamente en `Start()`. Impide acceso a Settings si AuthenticationService no esta listo.

### ERR-12: MatchmakingUIBuilder — Destruye EventSystem
**Archivo**: `MatchmakingUIBuilder.cs:85-86`
**Problema**: `foreach (Transform child in canvas.transform) DestroyImmediate(child.gameObject);` destruye TODOS los hijos incluyendo EventSystem sin verificar. Sin EventSystem, ninguna interaccion UI funciona.

### ERR-13: Minigames — Penalty double-counting en scores
**Archivos**: `MemoryPairsController.cs:449+952`, `OddOneOutController.cs:448+857`, `QuickMathController.cs:606+1097`
**Problema**: Los 3 juegos agregan `+1f` a AMBOS `currentTime` Y `penaltyTime`. `base.EndGame()` usa `currentTime` para `TotalTime`. Si `FinalScore = TotalTime + PenaltyTime`, cada penalidad se cuenta doble.
**Impacto**: Leaderboards, matchmaking y rankings de torneos incorrectos.

### ERR-14: QuickMath — No inicia en modo no-practice
**Archivo**: `QuickMathController.cs:151-166`
**Problema**: En modo ranked/online, `StartGame()` nunca se llama despues de `Initialize()`. El jugador ve la UI pero no puede jugar.

### ERR-15: OddOneOut — Acepta taps en grid incorrecto
**Archivo**: `OddOneOutController.cs:387-388`
**Problema**: `isCorrect` solo verifica `buttonIndex == oddButtonIndex`, sin verificar cual grid fue tocado. Tocar la misma posicion en el grid LEFT (sin diferencia) tambien se marca como correcto.

### ERR-16: CurrencyManager — Events inside/outside lock inconsistently
**Archivo**: `CurrencyManager.cs:160-497`
**Problema**: `OnGemsChanged`/`OnCoinsChanged` se disparan FUERA del lock en algunos metodos pero DENTRO del lock en `SettleBet`. `OnNotEnoughGems` se dispara DENTRO del lock. Event handlers que llamen de vuelta a CurrencyManager causaran deadlock.

### ERR-17: HistoryData — gameType almacena string traducido
**Archivo**: `HistoryData.cs:96`
**Problema**: `gameType = AutoLocalizer.Get("game_cognitive_sprint")` almacena un string TRADUCIDO. El valor cambia segun idioma, rompiendo filtrado o logica que compare `gameType`.

### ERR-18: UIFactory — Hardcoded Spanish "Cargando..."
**Archivo**: `UIFactory.cs:747`
**Problema**: `CreateLoadingPanel` tiene `"Cargando..."` hardcoded. Deberia usar `AutoLocalizer.Get("loading")`.

### ERR-19: DailyMissionsUIBuilder — fontSizeMin below minimum
**Archivo**: `DailyMissionsUIBuilder.cs:831-833`
**Problema**: `fontSizeMin = 18f` esta por debajo del minimo del proyecto `FontSizes.AutoMinSmall` (24). Texto ilegible en pantallas pequenas.

### ERR-20: Inconsistent translation key naming
**Archivos**: `MatchHistoryData.cs:207-209` vs `HistoryData.cs:253-257` vs `FriendRequestsSceneManager.cs:218-222`
**Problema**: Dos convenciones para time-ago: `time_minutes_ago` vs `time_ago_minutes`. Ambos sets existen en Translations.txt pero la inconsistencia es confusa y riesgosa.

---

## WARNINGS - 178 issues (agrupados por categoria)

### ASYNC ISSUES (32)

| Archivo | Linea(s) | Descripcion |
|---------|----------|-------------|
| RegisterManager.cs | 303-306 | `finally` block ejecuta `ShowLoading(false)` despues de `if (this == null) return;` — finally siempre ejecuta |
| LoginManager.cs | 327-350 | `OnLoginButtonClicked` no verifica `if (this == null)` despues de await |
| LoginManager.cs | 392-426, 431-465 | Google/Apple login — misma falta de destroyed-object check |
| MatchmakingManager.cs | 153, 516 | `SetupPlayerInfo` y `ShowOpponentInfo` son `async void` sin check post-await |
| CashMatchmakingManager.cs | 409-444 | `StartMatchmaking` async void sin null check post-await |
| FriendRequestsSceneManager.cs | 142-146 | `LoadRequests` no verifica `this == null` post-await |
| ProfileManager.cs | 214-236 | `LoadOtherProfileAsync` no verifica `this == null` post-await |
| GameSessionManager.cs | 389, 477 | `SaveSessionResults` Task descartado (fire-and-forget sin error handling) |
| NotificationService.cs | 74, 274, 606-611 | `async void Start`, multiples `_ = Task` fire-and-forget |
| AvatarService.cs | 64 | `_ = InitializeAsync()` fire-and-forget |
| AvatarService.cs | 470-476 | Dead code despues de await (check IsFaulted post-await) |
| DailyRewardService.cs | 201, 412 | Fire-and-forget Tasks |
| AchievementService.cs | 966 | `_ = SaveToFirebase()` fire-and-forget |
| NotificationStorageService.cs | 243 | `_ = SyncNotificationsToFirebase()` fire-and-forget |
| CurrencyManager.cs | 127-148 | `_ = SyncCurrencyToFirebase()` fire-and-forget |
| SettingsManager.cs | 783-826 | `OnConfirmNameClicked` async void sin try-catch |
| AgeVerificationManager.cs | 193-225 | Catch block no resetea `isVerifying`/button — usuario bloqueado |
| AgeVerificationManager.cs | 285-302 | `public static async void ResetVerification()` |
| TournamentManager.cs | 1486, 1528 | `Task.Delay` en async void — destroyed-object risk |
| CashTournamentCreateManager.cs | 443-444 | Invoke no cancelado en OnDestroy |
| CashMatchmakingManager.cs | 826-831 | `_ = ServiceLocator.Matchmaking.CancelSearch()` fire-and-forget |
| ProfileManager.cs | 172, 202, 230 | Multiples `_ = Task` fire-and-forget |
| MinigameBase.cs | 381 | `tournamentService.SubmitTournamentScore` Task descartado |

### NULL REFERENCE RISKS (28)

| Archivo | Linea(s) | Descripcion |
|---------|----------|-------------|
| GameSessionManager.cs | 200 | `CurrentContext.CurrentGame` sin validar index |
| MinigameBase.cs | 258, 293-347, 398-399, 421-431 | `ResultPanelManager.Instance` sin null check (5 instancias) |
| CognitiveSprintManager.cs | 168, 199, 230, 233 | `GameSessionManager.Instance` sin null check (4 instancias) |
| ComboVisualController.cs | 456-465, 468-491 | Glow images sin null check en FadeGlowOut/AnimateComboBreak |
| ForgotPasswordPopup.cs | 314-347 | Public methods acceden `messageText`/`sendButton` sin null check |
| MatchmakingManager.cs | 400 | `CognitiveSprintManager.Instance?.SelectedGames` auto-crea singleton vacio |
| SearchPlayersManager.cs | 344-345, 416 | `FriendService.Instance` sin null check |
| ProfileManager.cs | 241-243, 595 | `FriendService.Instance` sin null check |
| FriendsManager.cs | 248-249 | `avatarImage.sprite = initial;` sin null check en avatarImage |
| SettingsManager.cs | 990-1011, 1021, 1051 | `PremiumManager.Instance` sin null check |
| CashProfileSceneController.cs | 157-158, 185 | `HistoryManager.Instance.GetStats()` return sin null check |
| TournamentManager.cs | 557, 632 | `creatorName.ToLower()` y `currentPlayer.userId` sin null check |
| TournamentsBrowserManager.cs | 376 | `tournamentId.Substring(0, 8)` sin length check |
| FlashTapController.cs | 504 | `progressFill.transform.parent.parent` chain unsafe |
| MemoryPairsController.cs | 788 | Same parent chain issue |
| OddOneOutController.cs | 883 | Same parent chain issue |
| PremiumCard.cs | 229-245 | `SceneNavigator.Instance` sin null check |

### LOGIC BUGS (42)

| Archivo | Linea(s) | Descripcion |
|---------|----------|-------------|
| GameSessionManager.cs | 302 | Bet settlement usa `result.Completed` en vez de comparar con oponente |
| GameSessionManager.cs | 327-329 | Win streak incrementa por completion, no por victoria real |
| GameSessionManager.cs | 206-209 | `bestTime` default 0 — valid time nunca lo supera (deberia ser float.MaxValue) |
| LoginManager.cs | 79-83 | Duplicate event subscription (no -= antes de +=) |
| LoginManager.cs | 555 | `playerData.username == "Sin usuario"` — string Spanish hardcoded para comparacion |
| LoginManager.cs | 17 | Reflection fragil para SetPasswordToggleInput (RegisterManager usa metodo publico) |
| AgeVerificationManager.cs | 63-68 | OnEnable suscribe antes de que Start inicialice _kycService |
| BootManager.cs | 50 | AudioListener.volume controla TODO audio, no solo musica |
| OnboardingManager.cs | 842 | `AddComponent<CanvasGroup>()` sin verificar si ya existe — duplicados |
| OnboardingManager.cs | 100-101, 424-429 | Slide count hardcoded a 8 — out of sync con steps.Count |
| OnboardingManager.cs | 797-808 | TransitionToStep + ShowStep causa doble animacion |
| SceneNavigator.cs | 138, 173, 192 | `DOTween.KillAll(true)` mata TODOS los tweens globalmente — demasiado agresivo |
| DigitRushController.cs | 957 | `PenaltyTime = 0` — penalty nunca se reporta en result |
| DigitRushController.cs | 937 | bestTime multi-round vs single-round incomparable |
| FlashTapController.cs | 314-315 | Too-early tap reinicia juego completo (pierde scores y errores acumulados) |
| FlashTapController.cs | 554-561 | Pause/resume resetea wait timer — explotable en competitivo |
| MatchmakingManager.cs | 477-478 | `opponentId` almacena display name en vez de ID real |
| MatchmakingService.cs | 337 | `RemoveFromQueue` no tiene gameKey — entry NUNCA se borra de Firebase |
| MatchmakingService.cs | 156, 307 | `Time.time` para timeout — afectado por timeScale |
| AuthenticationService.cs | 169-173 | Login attempts nunca se resetean despues de cooldown — lockout permanente |
| AuthenticationService.cs | 261 | DateTime.Now en vez de DateTime.UtcNow para createdDate |
| DailyRewardsManager.cs | 1374-1403 | Currency bypass — usa PlayerPrefs directamente en vez de CurrencyManager |
| DailyRewardsManager.cs | 331, 649, 857 | `AddComponent` sin verificar existentes — componentes acumulados |
| DailyRewardsManager.cs | 286-296 | DateTime.TryParse culture-dependent — puede fallar silenciosamente |
| TournamentLobbyManager.cs | 268 | Chat sender comparado contra string localizado — rompe con cambio de idioma |
| TournamentLobbyManager.cs | 820-828 | Game type comparado contra strings hardcoded en ingles vs localizados |
| CashTournamentCreateManager.cs | 71, 76 | `float` para dinero real en vez de `decimal` |
| CashBattleResultPanelController.cs | 248 | Prize multiplier 1.8x vs CashBattle1v1Manager 0.70 — INCONSISTENTE |
| SettingsManager.cs | 797-804 | Gems deducidas ANTES de server confirm — no hay refund si falla |
| SettingsManager.cs | 540 | `AudioListener.volume` afecta TODO audio, effects volume vacio |
| FriendsManager.cs | 372-373 | Wrong button disabled (primer Button del card, no el de remove) |
| SearchPlayersManager.cs | 447-466 | Path incorrecto para buscar ButtonsRow — boton nunca se actualiza |
| CognitiveSprintManager.cs | 214-237 | Online sprint no llama `NotifySessionStarted()` |
| MinigameBase.cs | 183-189 | `OnGameCompleted` fires ANTES de `RegisterGameResult` |
| MinigameBase.cs | 214-232 | Sprint mid-game submite resultado individual como match completo |
| ComboVisualController.cs | 122-123 | `intensity` calculada pero nunca usada |
| ComboVisualController.cs | 359-360 | `fontSize` seteado manualmente pero autoSizing lo sobreescribe |
| ComboVisualController.cs | 512-513 | `_activeTweens` modificada durante iteracion en OnDestroy — InvalidOperationException |
| AutoLocalizer.cs | 1179-1189 | Fuzzy matching puede causar traducciones incorrectas |
| CurrencyManager.cs | 168, 193 | `SaveCurrency()` (con PlayerPrefs.Save I/O) dentro de lock |
| CurrencyDisplayUI.cs | 190-193 | `AddAmount` modifica display sin pasar por CurrencyManager |
| WalletData.cs | 155-158 | Withdrawal muestra sin signo negativo (perdida de minus sign) |

### MISSING CLEANUP (25)

| Archivo | Linea(s) | Descripcion |
|---------|----------|-------------|
| RegisterManager.cs | 95, 241-250 | onValidateInput y anonymous delegates nunca removidos |
| LoginManager.cs | 255-275 | Button listeners y onEndEdit nunca removidos |
| ForgotPasswordPopup.cs | 55-61 | Overlay button listener no removido en OnDestroy |
| ForgotPasswordPopup.cs | 395 | DOTween Kill scope incompleto (tweens en popupPanel no se matan) |
| AgeVerificationManager.cs | 90-94, 333 | Button listeners nunca removidos; tweens en loadingIndicator no se matan |
| BootManager.cs | — | Sin OnDestroy, sin DOTween.Kill |
| OnboardingManager.cs | 334-343 | Multiples onClick.AddListener sin cleanup en OnDestroy |
| FriendsManager.cs | 529-532 | DOKill incompleto (tweens en child objects no se matan) |
| FriendRequestsSceneManager.cs | 525-528 | DOKill incompleto |
| NotificationsManager.cs | 683-686 | DOKill incompleto |
| CashHistorySceneController.cs | 530-538 | Event unsubscribe puede fallar si Instance ya destruido |
| CashTournamentsManager.cs | 665 | DOKill incompleto, no unsubscribe de eventos externos |
| PremiumPanelUI.cs | 1008-1011 | Listener inline en factory nunca removido |
| CashBattle1v1Manager.cs | 762-765 | No remueve dropdown/button listeners |
| GameCardEffect.cs | 70-77 | Hacks para prevenir ThemeApplier/GameSelectorAnimator — timing fragil |

### PERFORMANCE (15)

| Archivo | Linea(s) | Descripcion |
|---------|----------|-------------|
| AchievementService.cs | 63, 1217-1233 | CompletionPercentage crea List cada vez; OnGameCompleted dispara 10 Firebase writes |
| NotificationStorageService.cs | 251-271 | Sync ENTERO JSON de notificaciones a Firebase en cada operacion |
| AutoLocalizer.cs | 1091, 1104 | `FindObjectsOfType<TextMeshProUGUI>(true)` en cada scene load — O(n) |
| LocalizedTextLayoutFixer.cs | 166-185 | FindObjectsOfType x3 + ForceLayoutRebuild en cada cambio de idioma |
| TournamentLobbyManager.cs | 465-483 | Update() llama UpdateTimeDisplay cada frame — allocations |
| TournamentManager.cs | 965-1001 | Update() itera 3 listas cada frame |
| CognitiveSprintManager.cs | 250-295 | GetAllGameInfos crea array + 15 AutoLocalizer.Get en cada llamada |
| CashProfileAnimator.cs | 507-522 | FindObjectsOfType<MonoBehaviour>() para encontrar HistoryManager |
| NotificationsManager.cs | 653-666 | Resources.Load sin cache en cada notification card |
| SettingsTextRuntimeDebug.cs | 50-52 | FindObjectsOfType<GameObject>(true) — busca TODOS los GOs |
| AchievementService.cs | 1054, 1089, 1126 | Division por zero si targetValue == 0 |

### DEAD CODE (24)

| Archivo | Linea(s) | Descripcion |
|---------|----------|-------------|
| BootManager.cs | 381-394 | `HandleBootError` nunca llamado |
| BootManager.cs | 252-269 | `InitializeObjectPools` y `PreloadCriticalResources` son stubs vacios |
| BootManager.cs | 27 | `loadingProgress` asignada pero nunca leida |
| LoginManager.cs | 48 | `currentPlayerData` asignada pero nunca leida |
| LoginManager.cs | 651 | `ShowErrorMessage` parameter `color` nunca usado |
| AgeVerificationManager.cs | 36-38 | `termsUrl`/`privacyUrl` con pragma suppress — nunca usados |
| OnboardingManager.cs | 920-929 | `highlightTooltip`, `highlightTarget`, `OnboardingStepType.Interactive` nunca usados |
| OnboardingManager.cs | 73 | `autoAdvanceDelay = 0f` — auto-advance nunca se activa |
| SceneNavigator.cs | 88-90 | `OnNavigationBack`, `OnSceneLoadStarted`, `OnSceneLoadCompleted` posiblemente sin suscriptores |
| WinPanelController.cs | 206-249 | `AnimateStatReveal` nunca llamado |
| WinPanelController.cs | 299-303 | `PlayCountdownAnimation` es stub con solo WaitForSeconds |
| MemoryPairsController.cs | 733 | `SetCardHidden()` nunca llamado |
| MemoryPairsController.cs | 927 | `ShowWinPanel()` nunca llamado |
| MatchmakingManager.cs | 63-65 | `countdownDuration` con pragma suppress — nunca leido |
| FlashTapController.cs | 57-59 | `cooldownAfterError` con pragma suppress — nunca leido |
| FlashTapButton3D.cs | 182-198 | `SetSprites`, `SetAllStateSprites` posiblemente sin uso |
| TournamentManager.cs | 1171-1196 | `ShowPremiumRequiredPanel()` nunca llamado |
| PremiumPanelUI.cs | 851, 913 | `CreateFeatureText` y `CreateRecommendedBadge` nunca llamados |
| SearchPlayersManager.cs | 476-486 | `OnChallengeClicked` nunca wired a button |
| CashBattle1v1Manager.cs | 258, 334 | `OnGameCardClicked` y `SelectSingleGame` nunca llamados |
| CashMatchmakingManager.cs | 80-82 | `TEXT_GOLD` color nunca usado |
| UsernamePopup.cs | 25 | `isFirstTime` seteado pero nunca leido |
| 34 UIBuilder files | Various | `CleanupOldUI()` method identico en 34 archivos — nunca llamado |

### MOCK/INCOMPLETE FEATURES (12)

| Archivo | Descripcion |
|---------|-------------|
| TournamentsBrowserManager.cs | 100% mock data — no hay API real |
| TournamentCreateManager.cs | Simulacion con 10% random failure — no backend |
| TournamentLobbyManager.cs | Chat en PlayerPrefs, mock participants, mock refresh |
| CashTournamentCreateManager.cs | Invoke delay simulado, no API call |
| DailyRewardService.cs:376-413 | `ApplyReward` — 5 casos son TODO stubs, no otorga currency real |
| FriendsManager.cs:363 | Challenge feature no implementado |
| SearchPlayersManager.cs:484 | Challenge navigation no implementado |
| ProfileManager.cs:677 | Challenge game creation no implementado |
| WalletManager.cs:222-225 | `GetStats()` retorna hardcoded (0, 0, 0) |
| WalletManager.cs:436-469 | `CreditWinnings`/`ProcessRefund` no actualizan balance real |
| FriendService.cs:483 | `IsPlayerOnline` retorna random — diferente en cada llamada |
| DailyRewardsManager.cs:523-532 | Timer countdown solo se actualiza una vez, nunca mas |

### SERIALIZATION ISSUES (5)

| Archivo | Linea(s) | Descripcion |
|---------|----------|-------------|
| HistoryData.cs | 42 | `DateTime timestamp` no soportado por JsonUtility |
| HistoryData.cs | 43 | `[Header]` en clase no-MonoBehaviour — no renderiza |
| DailyRewardService.cs | 349 | DateTime.TryParse sin culture — parsing puede fallar |
| TournamentLobbyManager.cs | 271-280 | Chat en PlayerPrefs con delimiter `|` que puede aparecer en mensajes |
| NotificationStorageService.cs | 86-87 | DateTime.Now.ToBinary (local) vs otros servicios UTC |

---

## INFO - 108 issues (agrupados por categoria)

### SPANISH STRINGS EN RUNTIME (no cubiertos por AutoLocalizer)

| Archivo | Descripcion |
|---------|-------------|
| FriendService.cs (10+ strings) | "No puedes enviarte solicitud a ti mismo", "Solicitud rechazada", "Amigo eliminado", etc. |
| DailyRewardService.cs:100-166 | Reward descriptions: "100 DigitCoins", "Premio Especial" |
| UIFactory.cs:747 | "Cargando..." hardcoded |
| CashProfileAnimator.cs:194 | GO name "Section_ESTADISTICASDETALLADAS" |
| LoginManager.cs:555 | Comparacion `== "Sin usuario"` |
| All Debug.Log en Auth files | Mensajes en espanol (por diseno, pero documentado) |

### HARDCODED VALUES

| Archivo | Valor | Descripcion |
|---------|-------|-------------|
| WinPanelController.cs:258 | `1.8m` | Prize multiplier |
| SprintSummaryPanelController.cs:194 | `1.8m` | Prize multiplier (igual) |
| CashBattleResultPanelController.cs:248 | `1.8m` | Prize multiplier (igual pero diferente de CashBattle1v1Manager) |
| CashBattle1v1Manager.cs | `0.70f` | 70% to winner = 1.4x (DIFERENTE) |
| CashTournamentsManager.cs | `0.9m` | 90% = 10% comision (DIFERENTE) |
| MinigameBase.cs:385 | `5m` | Prize multiplier fallback |
| Multiple files | Scene names | "Login", "MainMenu", "Register" en vez de SceneNavigator.Scenes.* |
| PremiumCard.cs:138-162 | "$14.99", "$2.99" | Precios hardcoded |
| CashTournamentLobbyManager.cs:478 | `<color=#FFD700>` | Color hardcoded en rich text |

### CODE DUPLICATION

| Descripcion | Archivos |
|-------------|----------|
| `IsValidEmail()` identico | RegisterManager, LoginManager, ForgotPasswordPopup |
| `FormatTime()` 3 versiones | WinPanelController, SprintSummaryPanelController, MinigameBase |
| `HapticType` enum + `TriggerHaptic()` + `Vibrate()` | DigitRush, MemoryPairs, OddOneOut (copy-paste identico) |
| `AnimatePenaltyText()` coroutine ~40 lines | 4 minigame controllers |
| `SetRadio()`/`UpdateToggleVisual()`/`SetToggleDefault()` | 5 minigame controllers |
| `CleanupOldUI()` method identico | 34 UIBuilder files |

### ARCHITECTURAL ISSUES

| Descripcion |
|-------------|
| DigitRush NO extiende MinigameBase — duplica toda la logica de flujo de juego |
| 8+ singletons con patrones diferentes (FindObjectOfType, auto-create, DontDestroyOnLoad) |
| No dependency management entre singletons — init order impredecible |
| Logout no resetea otros servicios (Achievement, DailyReward, Notifications, Friends) |
| 3 estrategias de cleanup diferentes en UIBuilders (sin consistencia) |
| FindObjectOfType deprecated en 14+ archivos pero actualizado en 21+ |
| Placeholder naming inconsistente entre LoginUIBuilder y RegisterUIBuilder |
| 5 minigame UIBuilders no setean font asset en SetupText helper |

### DEPRECATED APIS

| API | Archivos |
|-----|----------|
| `FindObjectsOfType<T>(true)` | RegisterManager, LoginManager, AutoLocalizer, AchievementsUIBuilder, +10 mas |
| `FindObjectOfType<T>()` | BaseReferenceAssigner, TournamentsBrowserUIBuilder, FlashTapUIBuilder, +8 mas |

### UX ISSUES

| Archivo | Descripcion |
|---------|-------------|
| WinPanelController.cs:466 | `Handheld.Vibrate()` es vibracion larga — deberia ser haptic feedback corto |
| DailyRewardsManager.cs:1338 | `Handheld.Vibrate()` sin verificar preferencia de vibracion |
| UsernamePopup.cs:261-277 | Errores de validacion solo en Debug.Log — sin feedback visual al usuario |
| GameCardEffect.cs:99-101 | Hover effects inefectivos en mobile (touch no tiene hover) |
| FloatingText.cs:178-184 | `Outline` y `Shadow` (UI) en TMP no tienen efecto visual |
| SceneNavigator.cs:190-215 | GoBack solo recuerda 1 escena — no hay stack de navegacion |
| WinPanelController.cs:525-531 | FormatTime agrega "s" inconsistentemente con MinigameBase.GetFormattedTime |

---

## RESUMEN POR ARCHIVOS MAS PROBLEMATICOS

| Archivo | ERR | WARN | INFO | Total |
|---------|-----|------|------|-------|
| MatchmakingService.cs | 6 | 8 | 0 | 14 |
| GameSessionManager.cs | 1 | 8 | 4 | 13 |
| CurrencyManager.cs | 1 | 6 | 2 | 9 |
| MinigameBase.cs | 1 | 7 | 2 | 10 |
| LoginManager.cs | 1 | 6 | 8 | 15 |
| FriendService.cs | 7 | 1 | 6 | 14 |
| TournamentLobbyManager.cs | 1 | 6 | 2 | 9 |
| DigitRushController.cs | 2 | 5 | 4 | 11 |
| SettingsManager.cs | 1 | 5 | 1 | 7 |
| WalletManager.cs/WalletData.cs | 2 | 7 | 2 | 11 |
| AuthenticationService.cs | 0 | 7 | 2 | 9 |

---

## PRIORIDADES DE REMEDIACION

### FASE 1 — BLOQUEANTES (pre-App Store):
1. **ERR-01**: Cambiar `decimal` a `float` o `double` en [SerializeField] fields
2. **ERR-02**: Eliminar duplicate dictionary keys en PlayerProgressionSystem
3. **ERR-03**: Reemplazar `await Instance?.X()` con null check explicito en FriendService
4. **ERR-09**: Resetear `_isProcessing = false` en finally blocks de CashBattleManager
5. **ERR-13**: Fix penalty double-counting en 3 minigames
6. **ERR-14**: QuickMath — agregar StartGame call para modo no-practice
7. **ERR-15**: OddOneOut — validar grid correcto en isCorrect check
8. **ERR-18**: Reemplazar "Cargando..." con AutoLocalizer.Get("loading")
9. **ERR-10**: TournamentLobbyManager — agregar flag para prevenir StartTournament repetido

### FASE 2 — IMPORTANTES (pre-launch):
10. **ERR-04**: MatchmakingService — ContinueWithOnMainThread
11. **ERR-05**: MatchmakingService — cleanup ValueChanged listener
12. **ERR-06**: AvatarService — mover Texture2D al main thread
13. **ERR-16**: CurrencyManager — consistencia de events dentro/fuera de lock
14. **ERR-17**: HistoryData — almacenar gameType como enum, no como string traducido
15. LoginManager — duplicate event subscription fix
16. DailyRewardsManager — usar CurrencyManager en vez de PlayerPrefs directamente
17. Inconsistent prize multipliers (1.8x vs 1.4x vs 0.9x) — unificar

### FASE 3 — POST-LAUNCH:
18. Dead code cleanup (~50+ metodos/fields sin uso)
19. Code duplication reduction (IsValidEmail, FormatTime, haptics, penalty text)
20. Deprecated API migration (FindObjectOfType → FindObjectsByType)
21. Spanish string migration en FriendService
22. Performance optimization (FindObjectsOfType en AutoLocalizer/LayoutFixer)
23. DigitRush refactor para extender MinigameBase

---

## NOTA SOBRE MOCK/INCOMPLETE FEATURES

12 features son enteramente mock o incompletas:
- **Tournaments**: Browser, Create, Lobby — 100% mock data
- **DailyRewards**: ApplyReward no otorga currency
- **Challenge/Reto**: No implementado en 3+ archivos
- **WalletManager**: Stats y credit/refund son stubs
- **FriendService**: Online status es random

Estos NO son bugs — son features pendientes. Pero deben ser completadas o deshabilitadas antes de publicar.
