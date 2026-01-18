# DigitPark - Inspector References Guide

Este documento lista todos los SerializeField de cada escena para facilitar la asignacion manual en el Inspector de Unity.

---

## Auth Scenes

### Login Scene
**Manager:** `LoginManager.cs`

| Header | Campo | Tipo | Descripcion |
|--------|-------|------|-------------|
| **UI - Login Panel** | loginPanel | GameObject | Panel principal de login |
| | titleText | TextMeshProUGUI | Titulo "Iniciar Sesion" |
| | emailInput | TMP_InputField | Campo de email |
| | passwordInput | TMP_InputField | Campo de contraseña |
| | rememberToggle | Toggle | Checkbox "Recordarme" |
| | loginButton | Button | Boton principal de login |
| | googleButton | Button | Boton login con Google |
| | appleButton | Button | Boton login con Apple |
| | registerButton | Button | Boton "Crear cuenta" |
| | forgotPasswordButton | Button | Boton "Olvidé mi contraseña" |
| **UI - Other** | loadingPanel | GameObject | Panel de carga/spinner |
| | backButton | Button | Boton volver |
| **UI - Panels** | errorPanel | ErrorPanelUI | Panel de errores (prefab) |
| **Animation** | titleAnimator | Animator | Animator del titulo |

---

### Register Scene
**Manager:** `RegisterManager.cs`

| Header | Campo | Tipo | Descripcion |
|--------|-------|------|-------------|
| **UI - Title** | titleText | TextMeshProUGUI | Titulo "Crear Cuenta" |
| **UI - Input Fields** | usernameInput | TMP_InputField | Campo de nombre de usuario |
| | emailInput | TMP_InputField | Campo de email |
| | passwordInput | TMP_InputField | Campo de contraseña |
| | confirmPasswordInput | TMP_InputField | Confirmar contraseña |
| **UI - Buttons** | createAccountButton | Button | Boton crear cuenta |
| | backButton | Button | Boton volver |
| **UI - Loading** | loadingPanel | GameObject | Panel de carga |
| **UI - Panels** | errorPanel | ErrorPanelUI | Panel de errores |

---

### AgeVerification Scene
**Manager:** `AgeVerificationManager.cs`

| Header | Campo | Tipo | Descripcion |
|--------|-------|------|-------------|
| **UI - Age Verification** | verificationPanel | GameObject | Panel principal |
| | titleText | TextMeshProUGUI | Titulo |
| | descriptionText | TextMeshProUGUI | Descripcion de verificacion |
| | dayInput | TMP_InputField | Dia de nacimiento |
| | monthInput | TMP_InputField | Mes de nacimiento |
| | yearInput | TMP_InputField | Año de nacimiento |
| | verifyButton | Button | Boton verificar |
| | backButton | Button | Boton volver |
| | errorText | TextMeshProUGUI | Texto de error |

---

## Core Scenes

### MainMenu Scene
**Manager:** `MainMenuManager.cs`

| Header | Campo | Tipo | Descripcion |
|--------|-------|------|-------------|
| **UI - Main Panel** | mainMenuPanel | GameObject | Panel principal |
| | titleText | TextMeshProUGUI | Titulo del juego |
| | playButton | Button | Boton "Jugar" |
| | scoresButton | Button | Boton "Puntuaciones" |
| | cashBattleButton | Button | Boton "Cash Battle" |
| | settingsButton | Button | Boton "Configuracion" |
| **UI - User Info** | userButton | Button | Boton perfil usuario |
| | userText | TextMeshProUGUI | Nombre de usuario |
| | searchButton | Button | Boton buscar jugadores |
| **UI - Premium** | premiumButton | Button | Boton premium/PRO |
| | premiumBadge | GameObject | Badge de premium activo |
| | premiumPanel | PremiumPanelUI | Panel premium (prefab) |
| **Animation** | titleAnimator | Animator | Animator del titulo |

---

### Settings Scene
**Manager:** `SettingsManager.cs`

| Header | Campo | Tipo | Descripcion |
|--------|-------|------|-------------|
| **UI - Settings Panel** | settingsPanel | GameObject | Panel principal |
| | titleText | TextMeshProUGUI | Titulo "Configuracion" |
| **UI - Volume Sliders** | soundVolumeSlider | Slider | Control volumen musica |
| | soundValueText | TextMeshProUGUI | Porcentaje volumen musica |
| | effectsVolumeSlider | Slider | Control volumen efectos |
| | effectsValueText | TextMeshProUGUI | Porcentaje volumen efectos |
| **UI - Language** | languageDropdown | TMP_Dropdown | Selector de idioma |
| | changeLangLabel | TextMeshProUGUI | Label "Cambiar idioma" |
| | languageStyler | LanguageDropdownStyler | Estilos del dropdown |
| **UI - Theme** | themeDropdown | TMP_Dropdown | Selector de tema |
| | changeThemeLabel | TextMeshProUGUI | Label "Cambiar tema" |
| **UI - Buttons** | changeNameButton | Button | Boton cambiar nombre |
| | logoutButton | Button | Boton cerrar sesion |
| | deleteAccountButton | Button | Boton eliminar cuenta |
| | backButton | Button | Boton volver |
| **UI - Premium Section** | premiumSection | GameObject | Seccion de compras |
| | removeAdsButton | Button | Boton quitar anuncios |
| | removeAdsButtonText | TextMeshProUGUI | Texto del boton |
| | premiumFullButton | Button | Boton premium completo |
| | premiumFullButtonText | TextMeshProUGUI | Texto del boton |
| | restorePurchasesButton | Button | Restaurar compras |
| **UI - Premium Button** | premiumButton | Button | Boton PRO |
| | premiumBadge | GameObject | Badge premium |
| | premiumPanel | PremiumPanelUI | Panel premium |
| **UI - Panels** | changeNamePanel | InputPanelUI | Panel cambio nombre |
| | deleteConfirmPanel | ConfirmPanelUI | Panel confirmar eliminar |
| | logoutConfirmPanel | ConfirmPanelUI | Panel confirmar logout |
| | errorPanel | ErrorPanelUI | Panel de errores |

---

## Social Scenes

### Profile Scene
**Manager:** `ProfileManager.cs`

| Header | Campo | Tipo | Descripcion |
|--------|-------|------|-------------|
| **UI - Header** | backButton | Button | Boton volver |
| | addFriendIconButton | Button | Icono agregar amigo |
| **UI - Profile Info** | usernameText | TextMeshProUGUI | Nombre de usuario |
| | avatarImage | Image | Imagen de avatar |
| | statusText | TextMeshProUGUI | Estado (Tu perfil/Amigo/etc) |
| **UI - General Stats** | totalGamesText | TextMeshProUGUI | Total partidas jugadas |
| | winsText | TextMeshProUGUI | Victorias |
| | winRateText | TextMeshProUGUI | Porcentaje victorias |
| | bestTimeText | TextMeshProUGUI | Mejor tiempo |
| | averageTimeText | TextMeshProUGUI | Tiempo promedio |
| **UI - Game Stats Values** | digitRushValueText | TextMeshProUGUI | Stats Digit Rush |
| | memoryPairsValueText | TextMeshProUGUI | Stats Memory Pairs |
| | quickMathValueText | TextMeshProUGUI | Stats Quick Math |
| | flashTapValueText | TextMeshProUGUI | Stats Flash Tap |
| | oddOneOutValueText | TextMeshProUGUI | Stats Odd One Out |
| **UI - Action Buttons** | friendsButton | Button | Ver amigos |
| | historyButton | Button | Ver historial |
| **UI - CTA Button** | challengeButton | Button | Boton retar |
| **UI - Game Selection** | gameSelectionPanel | GameObject | Panel seleccion juego |
| | darkOverlayButton | Button | Overlay oscuro |
| | cancelButton | Button | Boton cancelar |
| | digitRushButton | Button | Seleccionar Digit Rush |
| | memoryPairsButton | Button | Seleccionar Memory Pairs |
| | quickMathButton | Button | Seleccionar Quick Math |
| | flashTapButton | Button | Seleccionar Flash Tap |
| | oddOneOutButton | Button | Seleccionar Odd One Out |

---

### Scores Scene
**Manager:** `LeaderboardManager.cs`

| Header | Campo | Tipo | Descripcion |
|--------|-------|------|-------------|
| **UI - Header** | backButton | Button | Boton volver |
| | titleText | TextMeshProUGUI | Titulo "Puntuaciones" |
| **UI - Tabs** | globalTabButton | Button | Tab global |
| | friendsTabButton | Button | Tab amigos |
| **UI - Game Tabs** | digitRushTab | Button | Tab Digit Rush |
| | memoryPairsTab | Button | Tab Memory Pairs |
| | quickMathTab | Button | Tab Quick Math |
| | flashTapTab | Button | Tab Flash Tap |
| | oddOneOutTab | Button | Tab Odd One Out |
| **UI - List** | leaderboardContainer | Transform | Contenedor de items |
| | leaderboardItemPrefab | GameObject | Prefab item ranking |
| | scrollRect | ScrollRect | ScrollRect de la lista |
| | emptyStateText | TextMeshProUGUI | Mensaje lista vacia |
| **UI - My Rank** | myRankPanel | GameObject | Panel mi ranking |
| | myRankText | TextMeshProUGUI | Mi posicion |
| | myScoreText | TextMeshProUGUI | Mi puntuacion |

---

### SearchPlayers Scene
**Manager:** `SearchPlayersManager.cs`

| Header | Campo | Tipo | Descripcion |
|--------|-------|------|-------------|
| **UI - Header** | backButton | Button | Boton volver |
| | titleText | TextMeshProUGUI | Titulo |
| **UI - Search** | searchInput | TMP_InputField | Campo de busqueda |
| | searchButton | Button | Boton buscar |
| | clearButton | Button | Boton limpiar |
| **UI - Results** | resultsContainer | Transform | Contenedor resultados |
| | playerItemPrefab | GameObject | Prefab item jugador |
| | scrollRect | ScrollRect | ScrollRect lista |
| | emptyStateText | TextMeshProUGUI | Mensaje sin resultados |
| | loadingIndicator | GameObject | Indicador de carga |

---

## Games Scenes

### PlayModeSelection Scene
**Manager:** `PlayModeSelectionManager.cs`

| Header | Campo | Tipo | Descripcion |
|--------|-------|------|-------------|
| **UI - Main** | titleText | TextMeshProUGUI | Titulo "Modo de Juego" |
| | backButton | Button | Boton volver |
| **UI - Mode Buttons** | soloModeButton | Button | Modo Solo/Practica |
| | onlineModeButton | Button | Modo 1v1 Online |
| | tournamentsButton | Button | Modo Torneos |
| **UI - Mode Descriptions** | soloDescriptionText | TextMeshProUGUI | Descripcion modo solo |
| | onlineDescriptionText | TextMeshProUGUI | Descripcion modo online |
| | tournamentsDescriptionText | TextMeshProUGUI | Descripcion torneos |

---

### GameSelector Scene
**Manager:** `GameSelectorManager.cs`

| Header | Campo | Tipo | Descripcion |
|--------|-------|------|-------------|
| **Game Buttons** | digitRushButton | Button | Boton Digit Rush |
| | memoryPairsButton | Button | Boton Memory Pairs |
| | quickMathButton | Button | Boton Quick Math |
| | flashTapButton | Button | Boton Flash Tap |
| | oddOneOutButton | Button | Boton Odd One Out |
| **Cognitive Sprint** | cognitiveSprintButton | Button | Boton Cognitive Sprint |
| | cognitiveSprintPanel | GameObject | Panel configuracion sprint |
| | gameToggles | Toggle[] | Toggles seleccion juegos |
| | startSprintButton | Button | Iniciar sprint |
| | cancelSprintButton | Button | Cancelar sprint |
| | selectedCountText | TextMeshProUGUI | Contador juegos seleccionados |
| **Navigation** | backButton | Button | Boton volver |
| **Rules Panel** | rulesPanel | GameObject | Panel de reglas |
| | rulesTitleText | TextMeshProUGUI | Titulo reglas |
| | rulesContentText | TextMeshProUGUI | Contenido reglas |
| | dontShowToggle | Toggle | Toggle "No mostrar de nuevo" |
| | rulesPlayButton | Button | Boton jugar |
| | rulesCancelButton | Button | Boton cancelar |
| **Matchmaking UI** | matchmakingPanel | GameObject | Panel matchmaking |
| | matchmakingStatusText | TextMeshProUGUI | Estado matchmaking |
| | cancelMatchmakingButton | Button | Cancelar matchmaking |

---

### Matchmaking Scene
**Manager:** `MatchmakingManager.cs`

| Header | Campo | Tipo | Descripcion |
|--------|-------|------|-------------|
| **UI - Main** | titleText | TextMeshProUGUI | Titulo "Buscando oponente" |
| | statusText | TextMeshProUGUI | Estado de busqueda |
| | timerText | TextMeshProUGUI | Tiempo de espera |
| | cancelButton | Button | Boton cancelar |
| **UI - Animation** | searchingAnimator | Animator | Animacion de busqueda |
| | playerAvatar | Image | Avatar del jugador |
| | opponentAvatar | Image | Avatar del oponente |
| **UI - Found Opponent** | opponentFoundPanel | GameObject | Panel oponente encontrado |
| | opponentNameText | TextMeshProUGUI | Nombre del oponente |
| | opponentRatingText | TextMeshProUGUI | Rating del oponente |
| | readyButton | Button | Boton listo |

---

### DigitRush Scene
**Manager:** `DigitRushController.cs`

| Header | Campo | Tipo | Descripcion |
|--------|-------|------|-------------|
| **DigitRush - UI** | timerText | TextMeshProUGUI | Texto del timer |
| | currentNumberText | TextMeshProUGUI | Numero actual a presionar |
| | errorsText | TextMeshProUGUI | Contador de errores |
| | comboText | TextMeshProUGUI | Texto del combo |
| | winPanel | GameObject | Panel de victoria |
| **DigitRush - Grid** | digitButtons | Button[] | Array de 9 botones (numeros) |
| | digitTexts | TextMeshProUGUI[] | Textos de los botones |
| | cell3DEffects | Cell3DEffect[] | Efectos 3D de celdas |
| **Countdown** | countdownUI | CountdownUI | UI de cuenta regresiva |
| | useCountdown | bool | Usar cuenta regresiva |
| **Effects** | sparkleEffect | UISparkleEffect | Efecto de particulas |
| | enableHapticFeedback | bool | Habilitar vibracion |

---

### MemoryPairs Scene
**Manager:** `MemoryPairsController.cs`

| Header | Campo | Tipo | Descripcion |
|--------|-------|------|-------------|
| **Memory Pairs - Grid** | cardButtons | Button[] | Array de 16 botones (cartas) |
| | cardImages | Image[] | Imagenes de las cartas |
| | card3DEffects | Card3DEffect[] | Efectos 3D de cartas |
| **Memory Pairs - UI** | timerText | TextMeshProUGUI | Texto del timer |
| | pairsFoundText | TextMeshProUGUI | Pares encontrados |
| | errorsText | TextMeshProUGUI | Contador errores |
| | comboText | TextMeshProUGUI | Texto del combo |
| | winPanel | GameObject | Panel de victoria |
| **Countdown** | countdownUI | CountdownUI | UI cuenta regresiva |
| | useCountdown | bool | Usar cuenta regresiva |
| **Effects** | sparkleEffect | UISparkleEffect | Efecto particulas |
| | enableHapticFeedback | bool | Habilitar vibracion |
| **Memory Pairs - Sprites** | cardBackSprite | Sprite | Sprite dorso carta |
| | cardFrontSprites | Sprite[] | Sprites frente cartas |
| **Digits** | cardDigits | string[] | Digitos para cartas |

---

### QuickMath Scene
**Manager:** `QuickMathController.cs`

| Header | Campo | Tipo | Descripcion |
|--------|-------|------|-------------|
| **QuickMath - Equation** | problemText | TextMeshProUGUI | Texto ecuacion completa |
| | numberAText | TextMeshProUGUI | Primer numero |
| | numberBText | TextMeshProUGUI | Segundo numero |
| | operatorText | TextMeshProUGUI | Operador (+, -, x) |
| | questionMarkText | TextMeshProUGUI | Signo de interrogacion |
| | equationPanel | RectTransform | Panel de ecuacion |
| **QuickMath - Answers** | answerButtons | Button[] | 3 botones de respuesta |
| | answerTexts | TextMeshProUGUI[] | Textos de respuestas |
| **QuickMath - UI** | timerText | TextMeshProUGUI | Timer |
| | roundText | TextMeshProUGUI | Ronda actual |
| | errorsText | TextMeshProUGUI | Errores |
| | comboText | TextMeshProUGUI | Combo/Streak |
| | statsText | TextMeshProUGUI | Estadisticas finales |
| | roundIndicatorText | TextMeshProUGUI | Indicador de ronda |
| | winPanel | GameObject | Panel victoria |
| | winPanelCanvasGroup | CanvasGroup | CanvasGroup del panel |
| | comboCanvasGroup | CanvasGroup | CanvasGroup del combo |
| | progressFill | RectTransform | Barra de progreso |
| **Effects** | sparkleEffect | UISparkleEffect | Efecto particulas |
| | enableHapticFeedback | bool | Habilitar vibracion |
| **QuickMath - Settings** | totalRounds | int | Total de rondas |
| | maxNumber | int | Numero maximo |
| | includeMultiplication | bool | Incluir multiplicacion |

---

### FlashTap Scene
**Manager:** `FlashTapController.cs`

| Header | Campo | Tipo | Descripcion |
|--------|-------|------|-------------|
| **Flash Tap - UI** | tapButton | Button | Boton principal |
| | button3D | FlashTapButton3D | Boton 3D personalizado |
| | instructionText | TextMeshProUGUI | Texto instruccion |
| | reactionTimeText | TextMeshProUGUI | Tiempo de reaccion |
| | roundText | TextMeshProUGUI | Ronda actual |
| | averageText | TextMeshProUGUI | Promedio |
| | bestTimeText | TextMeshProUGUI | Mejor tiempo |
| | winPanel | GameObject | Panel victoria |
| **3D Button Sprites** | buttonUpSprite | Sprite | Sprite boton arriba |
| | buttonDownSprite | Sprite | Sprite boton abajo |
| **Flash Tap - Settings** | totalAttempts | int | Total de intentos |
| | minWaitTime | float | Tiempo espera minimo |
| | maxWaitTime | float | Tiempo espera maximo |
| | restartDelayAfterError | float | Delay tras error |
| | delayBetweenAttempts | float | Delay entre intentos |
| **Flash Tap - Feedback** | enableSuccessParticles | bool | Particulas de exito |
| | enableHaptics | bool | Vibracion haptica |

---

### OddOneOut Scene
**Manager:** `OddOneOutController.cs`

| Header | Campo | Tipo | Descripcion |
|--------|-------|------|-------------|
| **Grid Izquierda** | leftGridButtons | Button[] | 16 botones grid izquierda |
| | leftButtonTexts | TextMeshProUGUI[] | Textos grid izquierda |
| | leftButtonImages | Image[] | Imagenes grid izquierda |
| **Grid Derecha** | rightGridButtons | Button[] | 16 botones grid derecha |
| | rightButtonTexts | TextMeshProUGUI[] | Textos grid derecha |
| | rightButtonImages | Image[] | Imagenes grid derecha |
| **Odd One Out - UI** | timerText | TextMeshProUGUI | Timer |
| | roundText | TextMeshProUGUI | Ronda |
| | errorsText | TextMeshProUGUI | Errores |
| | instructionText | TextMeshProUGUI | Instruccion |
| | comboText | TextMeshProUGUI | Combo |
| | statsText | TextMeshProUGUI | Estadisticas |
| | winPanel | GameObject | Panel victoria |
| | winPanelCanvasGroup | CanvasGroup | CanvasGroup panel |
| **Effects** | sparkleEffect | UISparkleEffect | Efecto particulas |
| | enableHapticFeedback | bool | Vibracion |
| **Settings** | totalRounds | int | Total de rondas |

---

## Cash Battle Scenes

### CashBattleHub Scene
**Manager:** `CashBattleManager.cs`

| Header | Campo | Tipo | Descripcion |
|--------|-------|------|-------------|
| **UI - Header** | backButton | Button | Boton volver |
| | titleText | TextMeshProUGUI | Titulo "Cash Battle" |
| | balanceText | TextMeshProUGUI | Balance de dinero |
| **UI - Navigation** | walletButton | Button | Ir a Wallet |
| | historyButton | Button | Ir a Historial |
| | tournamentsButton | Button | Ir a Torneos |
| **UI - Quick Match** | entryFeeDropdown | TMP_Dropdown | Selector entrada |
| | findMatchButton | Button | Buscar partida |
| | potentialWinningsText | TextMeshProUGUI | Ganancia potencial |
| **UI - Active Matches** | activeMatchesContainer | Transform | Contenedor partidas |
| | activeMatchPrefab | GameObject | Prefab partida activa |
| | noActiveMatchesText | TextMeshProUGUI | Sin partidas activas |

---

### CashWallet Scene
**Manager:** `CashWalletSceneController.cs`

| Header | Campo | Tipo | Descripcion |
|--------|-------|------|-------------|
| **Header** | backButton | Button | Boton volver |
| | balanceText | TextMeshProUGUI | Balance principal |
| | bonusBalanceText | TextMeshProUGUI | Balance bonus/pendiente |
| **Tabs** | depositTabButton | Button | Tab deposito |
| | withdrawTabButton | Button | Tab retiro |
| | historyTabButton | Button | Tab historial |
| | activeTabColor | Color | Color tab activo |
| | inactiveTabColor | Color | Color tab inactivo |
| **Panels** | depositPanel | GameObject | Panel deposito |
| | withdrawPanel | GameObject | Panel retiro |
| | transactionHistoryPanel | GameObject | Panel historial |
| **Deposit Panel** | depositOptionsContainer | Transform | Contenedor opciones |
| | depositOptionPrefab | GameObject | Prefab opcion deposito |
| | paymentMethodButtons | Button[] | Botones metodo pago |
| | paymentMethodsContainer | GameObject | Contenedor metodos |
| **Withdraw Panel** | withdrawAmountInput | TMP_InputField | Input cantidad |
| | withdrawButton | Button | Boton retirar |
| | withdrawableAmountText | TextMeshProUGUI | Cantidad disponible |
| | withdrawMinText | TextMeshProUGUI | Minimo retiro |
| | withdrawFeeText | TextMeshProUGUI | Comision |
| | kycRequiredPanel | GameObject | Panel KYC requerido |
| | verifyKycButton | Button | Boton verificar KYC |
| **Transaction History** | transactionsContainer | Transform | Contenedor transacciones |
| | transactionItemPrefab | GameObject | Prefab transaccion |
| | emptyHistoryText | TextMeshProUGUI | Sin transacciones |
| | loadMoreButton | Button | Cargar mas |
| **Overlays** | loadingOverlay | GameObject | Overlay cargando |
| | successOverlay | GameObject | Overlay exito |
| | errorOverlay | GameObject | Overlay error |
| | errorMessageText | TextMeshProUGUI | Mensaje error |
| **Configuration** | minimumWithdrawFloat | float | Minimo retiro |
| | withdrawFeePercentFloat | float | Porcentaje comision |
| | transactionsPerPage | int | Items por pagina |

---

### CashHistory Scene
**Manager:** `CashHistorySceneController.cs`

| Header | Campo | Tipo | Descripcion |
|--------|-------|------|-------------|
| **Header** | backButton | Button | Boton volver |
| | titleText | TextMeshProUGUI | Titulo "Historial" |
| **Stats Panel** | statsPanel | GameObject | Panel estadisticas |
| | totalMatchesText | TextMeshProUGUI | Total partidas |
| | winRateText | TextMeshProUGUI | Porcentaje victorias |
| | netProfitText | TextMeshProUGUI | Ganancia neta |
| | winsText | TextMeshProUGUI | Victorias |
| | lossesText | TextMeshProUGUI | Derrotas |
| | drawsText | TextMeshProUGUI | Empates |
| | currentStreakText | TextMeshProUGUI | Racha actual |
| | bestStreakText | TextMeshProUGUI | Mejor racha |
| | tournamentsPlayedText | TextMeshProUGUI | Torneos jugados |
| | tournamentWinsText | TextMeshProUGUI | Torneos ganados |
| **Tabs** | allTabButton | Button | Tab todos |
| | matchesTabButton | Button | Tab partidas |
| | tournamentsTabButton | Button | Tab torneos |
| | activeTabColor | Color | Color activo |
| | inactiveTabColor | Color | Color inactivo |
| **Filters** | resultFilterDropdown | TMP_Dropdown | Filtro resultado |
| | dateFilterDropdown | TMP_Dropdown | Filtro fecha |
| | clearFiltersButton | Button | Limpiar filtros |
| **History List** | entriesContainer | Transform | Contenedor entradas |
| | historyEntryPrefab | GameObject | Prefab entrada |
| | scrollRect | ScrollRect | ScrollRect lista |
| | emptyStateText | TextMeshProUGUI | Sin entradas |
| | loadMoreButton | Button | Cargar mas |
| **Detail Panel** | detailPanel | GameObject | Panel detalle |
| | detailTitleText | TextMeshProUGUI | Titulo detalle |
| | detailSubtitleText | TextMeshProUGUI | Subtitulo |
| | detailResultText | TextMeshProUGUI | Resultado |
| | detailScoreText | TextMeshProUGUI | Puntuacion |
| | detailEntryFeeText | TextMeshProUGUI | Entrada |
| | detailPrizeText | TextMeshProUGUI | Premio |
| | detailNetText | TextMeshProUGUI | Neto |
| | detailDateText | TextMeshProUGUI | Fecha |
| | detailDurationText | TextMeshProUGUI | Duracion |
| | closeDetailButton | Button | Cerrar detalle |
| **Loading** | loadingIndicator | GameObject | Indicador carga |
| **Configuration** | entriesPerPage | int | Items por pagina |

---

### CashTournaments Scene
*Similar a TournamentsBrowser - usa el mismo patron de UI*

---

## Tournaments Scenes

### TournamentsBrowser Scene
**Manager:** `TournamentsBrowserManager.cs`

| Header | Campo | Tipo | Descripcion |
|--------|-------|------|-------------|
| **UI - Header** | backButton | Button | Boton volver |
| | titleText | TextMeshProUGUI | Titulo |
| | createTournamentButton | Button | Crear torneo |
| **UI - Filters** | gameFilterDropdown | TMP_Dropdown | Filtro por juego |
| | statusFilterDropdown | TMP_Dropdown | Filtro por estado |
| | searchInput | TMP_InputField | Busqueda |
| **UI - List** | tournamentsContainer | Transform | Contenedor |
| | tournamentItemPrefab | GameObject | Prefab torneo |
| | scrollRect | ScrollRect | ScrollRect |
| | emptyStateText | TextMeshProUGUI | Sin torneos |
| | loadMoreButton | Button | Cargar mas |
| **UI - Loading** | loadingIndicator | GameObject | Indicador carga |

---

### TournamentCreate Scene
**Manager:** `TournamentCreateManager.cs`

| Header | Campo | Tipo | Descripcion |
|--------|-------|------|-------------|
| **UI - Header** | backButton | Button | Boton volver |
| | titleText | TextMeshProUGUI | Titulo |
| **UI - Form** | tournamentNameInput | TMP_InputField | Nombre torneo |
| | gameTypeDropdown | TMP_Dropdown | Tipo de juego |
| | maxPlayersDropdown | TMP_Dropdown | Max jugadores |
| | entryFeeInput | TMP_InputField | Entrada |
| | prizePoolText | TextMeshProUGUI | Pool de premios |
| | startTimeDropdown | TMP_Dropdown | Hora inicio |
| | descriptionInput | TMP_InputField | Descripcion |
| **UI - Options** | privateToggle | Toggle | Torneo privado |
| | inviteCodeText | TextMeshProUGUI | Codigo invitacion |
| **UI - Actions** | createButton | Button | Crear torneo |
| | cancelButton | Button | Cancelar |
| **UI - Preview** | previewPanel | GameObject | Vista previa |

---

### TournamentLobby Scene
**Manager:** `TournamentLobbyManager.cs`

| Header | Campo | Tipo | Descripcion |
|--------|-------|------|-------------|
| **UI - Header** | backButton | Button | Boton volver |
| | tournamentNameText | TextMeshProUGUI | Nombre torneo |
| | statusText | TextMeshProUGUI | Estado |
| **UI - Info** | gameTypeText | TextMeshProUGUI | Tipo juego |
| | entryFeeText | TextMeshProUGUI | Entrada |
| | prizePoolText | TextMeshProUGUI | Premio |
| | playersCountText | TextMeshProUGUI | Jugadores |
| | startTimeText | TextMeshProUGUI | Hora inicio |
| **UI - Players** | playersContainer | Transform | Contenedor jugadores |
| | playerItemPrefab | GameObject | Prefab jugador |
| | scrollRect | ScrollRect | ScrollRect |
| **UI - Actions** | joinButton | Button | Unirse |
| | leaveButton | Button | Salir |
| | startButton | Button | Iniciar (host) |
| | inviteButton | Button | Invitar |
| **UI - Bracket** | bracketPanel | GameObject | Panel bracket |
| | bracketContainer | Transform | Contenedor bracket |

---

## Monetization Scenes

### Shop Scene
**Manager:** `ShopManager.cs`

| Header | Campo | Tipo | Descripcion |
|--------|-------|------|-------------|
| **Tabs** | _gemsTabButton | Button | Tab gemas |
| | _coinsTabButton | Button | Tab monedas |
| | _themesTabButton | Button | Tab temas |
| | _offersTabButton | Button | Tab ofertas |
| **Content** | _gemsContent | GameObject | Contenido gemas |
| | _coinsContent | GameObject | Contenido monedas |
| | _themesContent | GameObject | Contenido temas |
| | _offersContent | GameObject | Contenido ofertas |
| **Colors** | _activeTabColor | Color | Color tab activo |
| | _inactiveTabColor | Color | Color tab inactivo |
| | _activeTextColor | Color | Color texto activo |
| | _inactiveTextColor | Color | Color texto inactivo |
| **Purchase Popup** | _purchasePopup | GameObject | Popup compra |
| | _notEnoughGemsPopup | GameObject | Popup sin gemas |
| | _popupItemIcon | Image | Icono item |
| | _popupItemName | TextMeshProUGUI | Nombre item |
| | _popupItemPrice | TextMeshProUGUI | Precio |
| | _popupConfirmButton | Button | Confirmar compra |
| | _popupCancelButton | Button | Cancelar |
| | _notEnoughCloseButton | Button | Cerrar popup |
| | _notEnoughGetGemsButton | Button | Ir a comprar gemas |
| **Navigation** | _backButton | Button | Boton volver |
| **Currency Display** | _gemsDisplay | CurrencyDisplayUI | Display gemas |
| | _coinsDisplay | CurrencyDisplayUI | Display monedas |
| | _headerGemsText | TextMeshProUGUI | Gemas en header |
| | _headerCoinsText | TextMeshProUGUI | Monedas en header |

---

### DailyRewards Scene
**Manager:** `DailyRewardsManager.cs`

| Header | Campo | Tipo | Descripcion |
|--------|-------|------|-------------|
| **UI - Header** | backButton | Button | Boton volver |
| | titleText | TextMeshProUGUI | Titulo |
| | streakText | TextMeshProUGUI | Racha de dias |
| | nextResetText | TextMeshProUGUI | Siguiente reset |
| **UI - Current Day** | currentDayHighlight | GameObject | Highlight dia actual |
| | currentDayText | TextMeshProUGUI | Texto dia |
| | currentDayRewardIcon | Image | Icono recompensa |
| | currentDayRewardText | TextMeshProUGUI | Texto recompensa |
| **UI - Rewards List** | rewardsContainer | Transform | Contenedor recompensas |
| | rewardDayPrefab | GameObject | Prefab dia |
| **UI - Claim Button** | claimButton | Button | Boton reclamar |
| | claimButtonText | TextMeshProUGUI | Texto boton |
| | claimGlow | GameObject | Efecto glow |
| | bonusInfoText | TextMeshProUGUI | Info bonus |
| **UI - Progress** | streakProgressBar | Slider | Barra progreso |
| | streakBonusText | TextMeshProUGUI | Texto bonus racha |
| **UI - Claim Animation** | claimAnimationPanel | GameObject | Panel animacion |
| | claimRewardText | TextMeshProUGUI | Texto recompensa |
| | claimRewardIcon | Image | Icono recompensa |
| | claimParticles | ParticleSystem | Particulas |
| | continueButton | Button | Continuar |
| **UI - Milestone** | milestonePanel | GameObject | Panel milestone |
| | milestoneText | TextMeshProUGUI | Texto milestone |
| | milestoneBonusText | TextMeshProUGUI | Bonus milestone |
| **Icons** | coinIcon | Sprite | Icono moneda |
| | gemIcon | Sprite | Icono gema |
| | chestIcon | Sprite | Icono cofre |
| | xpIcon | Sprite | Icono XP |
| | mysteryIcon | Sprite | Icono misterio |

---

### DailyMissions Scene
**Manager:** `DailyMissionsManager.cs`

| Header | Campo | Tipo | Descripcion |
|--------|-------|------|-------------|
| **UI - Header** | backButton | Button | Boton volver |
| | titleText | TextMeshProUGUI | Titulo |
| | resetTimerText | TextMeshProUGUI | Tiempo reset |
| **UI - Progress** | dailyProgressBar | Slider | Progreso diario |
| | completedCountText | TextMeshProUGUI | Misiones completadas |
| | bonusRewardText | TextMeshProUGUI | Bonus al completar |
| **UI - Missions List** | missionsContainer | Transform | Contenedor misiones |
| | missionItemPrefab | GameObject | Prefab mision |
| | scrollRect | ScrollRect | ScrollRect |
| | emptyStateText | TextMeshProUGUI | Sin misiones |
| **UI - Bonus Panel** | bonusClaimPanel | GameObject | Panel bonus |
| | bonusClaimButton | Button | Boton reclamar bonus |
| | bonusIcon | Image | Icono bonus |
| | bonusAmountText | TextMeshProUGUI | Cantidad bonus |

---

### BattlePass Scene
**Manager:** `BattlePassManager.cs`

| Header | Campo | Tipo | Descripcion |
|--------|-------|------|-------------|
| **UI - Header** | backButton | Button | Boton volver |
| | seasonNameText | TextMeshProUGUI | Nombre temporada |
| | timeRemainingText | TextMeshProUGUI | Tiempo restante |
| **UI - Level Progress** | currentLevelText | TextMeshProUGUI | Nivel actual |
| | currentXPText | TextMeshProUGUI | XP actual |
| | xpProgressBar | Slider | Barra XP |
| | xpToNextLevelText | TextMeshProUGUI | XP para siguiente |
| **UI - Premium** | premiumBadge | GameObject | Badge premium |
| | buyPremiumButton | Button | Comprar premium |
| | premiumPriceText | TextMeshProUGUI | Precio premium |
| **UI - Rewards** | rewardsScrollRect | ScrollRect | ScrollRect recompensas |
| | rewardsContainer | Transform | Contenedor |
| | rewardTierPrefab | GameObject | Prefab tier |
| **UI - Reward Detail** | rewardDetailPanel | GameObject | Panel detalle |
| | rewardDetailIcon | Image | Icono detalle |
| | rewardDetailNameText | TextMeshProUGUI | Nombre |
| | rewardDetailDescText | TextMeshProUGUI | Descripcion |
| | rewardDetailTypeText | TextMeshProUGUI | Tipo |
| | claimRewardButton | Button | Reclamar |
| | closeDetailButton | Button | Cerrar |
| **UI - Buy Premium Modal** | buyPremiumModal | GameObject | Modal compra |
| | premiumBenefitsText | TextMeshProUGUI | Beneficios |
| | confirmBuyButton | Button | Confirmar |
| | cancelBuyButton | Button | Cancelar |
| **UI - Reward Claimed** | rewardClaimedPopup | GameObject | Popup reclamado |
| | rewardClaimedIcon | Image | Icono |
| | rewardClaimedText | TextMeshProUGUI | Texto |

---

### ChestOpening Scene
**Manager:** `ChestOpeningManager.cs`

| Header | Campo | Tipo | Descripcion |
|--------|-------|------|-------------|
| **UI - Header** | backButton | Button | Boton volver |
| | skipButton | Button | Saltar animacion |
| | chestTypeText | TextMeshProUGUI | Tipo de cofre |
| **UI - Chest Display** | chestContainer | GameObject | Contenedor cofre |
| | chestImage | Image | Imagen cofre |
| | chestAnimator | Animator | Animator cofre |
| | chestParticles | ParticleSystem | Particulas cofre |
| | openingParticles | ParticleSystem | Particulas apertura |
| **UI - Rewards Panel** | rewardsPanel | GameObject | Panel recompensas |
| | rewardsContainer | Transform | Contenedor items |
| | rewardItemPrefab | GameObject | Prefab item |
| | totalValueText | TextMeshProUGUI | Valor total |
| | collectAllButton | Button | Recoger todo |
| **UI - Single Reward** | singleRewardPanel | GameObject | Panel item unico |
| | singleRewardIcon | Image | Icono |
| | singleRewardNameText | TextMeshProUGUI | Nombre |
| | singleRewardDescriptionText | TextMeshProUGUI | Descripcion |
| | singleRewardRarityText | TextMeshProUGUI | Rareza |
| | rarityParticles | ParticleSystem | Particulas rareza |
| **UI - Prompt** | tapPrompt | GameObject | Prompt tocar |
| | tapPromptText | TextMeshProUGUI | Texto prompt |
| **Audio** | audioSource | AudioSource | Fuente audio |
| | chestShakeClip | AudioClip | Sonido sacudir |
| | chestOpenClip | AudioClip | Sonido abrir |
| | rewardRevealClip | AudioClip | Sonido revelar |
| | rareRewardClip | AudioClip | Sonido raro |
| | epicRewardClip | AudioClip | Sonido epico |
| | legendaryRewardClip | AudioClip | Sonido legendario |
| **Chest Sprites** | commonChestSprite | Sprite | Cofre comun |
| | rareChestSprite | Sprite | Cofre raro |
| | epicChestSprite | Sprite | Cofre epico |
| | legendaryChestSprite | Sprite | Cofre legendario |
| **Rarity Colors** | commonColor | Color | Color comun |
| | rareColor | Color | Color raro |
| | epicColor | Color | Color epico |
| | legendaryColor | Color | Color legendario |

---

### Achievements Scene
**Manager:** `AchievementsManager.cs`

| Header | Campo | Tipo | Descripcion |
|--------|-------|------|-------------|
| **UI - Header** | backButton | Button | Boton volver |
| | titleText | TextMeshProUGUI | Titulo |
| | totalPointsText | TextMeshProUGUI | Puntos totales |
| | completionText | TextMeshProUGUI | Porcentaje completado |
| **UI - Tabs** | allTab | Button | Tab todos |
| | inProgressTab | Button | Tab en progreso |
| | completedTab | Button | Tab completados |
| | secretTab | Button | Tab secretos |
| **UI - Categories** | categoriesContainer | Transform | Contenedor categorias |
| | categoryHeaderPrefab | GameObject | Prefab header |
| **UI - Achievements List** | achievementsContainer | Transform | Contenedor logros |
| | achievementItemPrefab | GameObject | Prefab logro |
| | scrollRect | ScrollRect | ScrollRect |
| | emptyStateText | TextMeshProUGUI | Sin logros |
| **UI - Achievement Detail** | detailPanel | GameObject | Panel detalle |
| | detailIcon | Image | Icono |
| | detailTitleText | TextMeshProUGUI | Titulo |
| | detailDescriptionText | TextMeshProUGUI | Descripcion |
| | detailProgressBar | Slider | Barra progreso |
| | detailProgressText | TextMeshProUGUI | Texto progreso |
| | detailRewardText | TextMeshProUGUI | Recompensa |
| | claimRewardButton | Button | Reclamar |
| | closeDetailButton | Button | Cerrar |
| **UI - Rewards** | rewardPopup | GameObject | Popup recompensa |
| | rewardPopupText | TextMeshProUGUI | Texto |
| | rewardPopupIcon | Image | Icono |
| **Configuration** | completedColor | Color | Color completado |
| | inProgressColor | Color | Color en progreso |
| | lockedColor | Color | Color bloqueado |

---

### Onboarding Scene
**Manager:** `OnboardingManager.cs`

| Header | Campo | Tipo | Descripcion |
|--------|-------|------|-------------|
| **UI - Header** | skipButton | Button | Boton saltar |
| | skipButtonText | TextMeshProUGUI | Texto saltar |
| | backButton | Button | Boton atras |
| **UI - Content** | stepImage | Image | Imagen del paso |
| | titleText | TextMeshProUGUI | Titulo |
| | descriptionText | TextMeshProUGUI | Descripcion |
| | characterContainer | GameObject | Contenedor personaje |
| | characterAnimator | Animator | Animator personaje |
| **UI - Navigation** | nextButton | Button | Siguiente |
| | prevButton | Button | Anterior |
| | nextButtonText | TextMeshProUGUI | Texto siguiente |
| **UI - Progress** | dotsContainer | Transform | Contenedor dots |
| | dotPrefab | GameObject | Prefab dot |
| | progressBar | Slider | Barra progreso |
| | stepCounterText | TextMeshProUGUI | Contador pasos |
| **UI - Highlight** | highlightOverlay | GameObject | Overlay highlight |
| | highlightTarget | RectTransform | Target highlight |
| | highlightTooltipText | TextMeshProUGUI | Tooltip |
| | tapToContinuePrompt | GameObject | Prompt continuar |
| **UI - Name Input** | nameInputPanel | GameObject | Panel nombre |
| | nameInput | TMP_InputField | Input nombre |
| | confirmNameButton | Button | Confirmar |
| | nameErrorText | TextMeshProUGUI | Error |
| **UI - Avatar Selection** | avatarSelectionPanel | GameObject | Panel avatar |
| | avatarContainer | Transform | Contenedor avatares |
| | avatarOptionPrefab | GameObject | Prefab avatar |
| **UI - Completion** | completionPanel | GameObject | Panel completado |
| | completionTitleText | TextMeshProUGUI | Titulo |
| | completionMessageText | TextMeshProUGUI | Mensaje |
| | rewardText | TextMeshProUGUI | Recompensa |
| | startPlayingButton | Button | Comenzar a jugar |
| **Step Images** | welcomeImage | Sprite | Imagen bienvenida |
| | gamesImage | Sprite | Imagen juegos |
| | cashBattleImage | Sprite | Imagen cash battle |
| | tournamentsImage | Sprite | Imagen torneos |
| | rewardsImage | Sprite | Imagen recompensas |
| | socialImage | Sprite | Imagen social |

---

## Notas Importantes

### Convenciones de Nombres en Jerarquia

Para facilitar la asignacion, usa estos nombres en la jerarquia de Unity:

```
SceneName (Scene)
└── Canvas
    ├── Header
    │   ├── BackButton
    │   ├── TitleText
    │   └── ...
    ├── ContentPanel
    │   ├── ScrollView
    │   │   └── Viewport
    │   │       └── Content (Container)
    │   └── ...
    ├── ActionButtons
    │   └── ...
    └── Overlays
        ├── LoadingPanel
        ├── ErrorPanel
        └── ...
```

### Prefabs Comunes

Los siguientes prefabs son usados en multiples escenas:

- `ErrorPanelUI` - Panel de errores
- `ConfirmPanelUI` - Panel de confirmacion
- `InputPanelUI` - Panel de input
- `PremiumPanelUI` - Panel premium
- `LoadingIndicator` - Indicador de carga
- `CurrencyDisplayUI` - Display de monedas/gemas

### Scripts de UI Items (para prefabs)

Los prefabs de listas necesitan estos scripts:

- `LeaderboardItemUI` - Item de ranking
- `PlayerSearchItemUI` - Item de busqueda
- `TournamentItemUI` - Item de torneo
- `MissionItemUI` - Item de mision
- `RewardDayItemUI` - Item de dia de recompensa
- `AchievementItemUI` - Item de logro
- `DepositOptionUI` - Opcion de deposito
- `TransactionItemUI` - Item de transaccion
- `HistoryEntryItemUI` - Entrada de historial

---

*Documento generado automaticamente - Ultima actualizacion: 2026-01-13*
