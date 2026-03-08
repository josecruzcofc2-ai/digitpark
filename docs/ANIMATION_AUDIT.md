# DigitPark - Auditoría Completa de Animaciones

> Análisis meticuloso escena por escena: estado actual, problemas DOTween, y propuestas de alto impacto.
> Fecha: 2026-03-07

---

## TABLA DE CONTENIDOS

1. [Arquitectura de Animaciones Actual](#1-arquitectura-de-animaciones-actual)
2. [Inventario Escena por Escena](#2-inventario-escena-por-escena)
3. [Problemas DOTween Detectados](#3-problemas-dotween-detectados)
4. [Propuestas de Animaciones WOW por Escena](#4-propuestas-de-animaciones-wow-por-escena)
5. [Prioridades de Implementación](#5-prioridades-de-implementación)

---

## 1. ARQUITECTURA DE ANIMACIONES ACTUAL

### Sistema Central
| Componente | Archivo | Rol |
|---|---|---|
| UIAnimations | `Scripts/Animations/Core/UIAnimations.cs` | 35+ métodos estáticos de animación reutilizables |
| UIAnimationManager | `Scripts/Animations/Core/UIAnimationManager.cs` | Singleton global, init DOTween, screen flash/shake |
| UIEffects | `Scripts/Animations/Components/UIEffects.cs` | Efectos compuestos reutilizables |
| AnimatedPanel | `Scripts/Animations/Components/AnimatedPanel.cs` | Animación universal de paneles |

### Animadores de Escena (8)
| Animador | Archivo | Función |
|---|---|---|
| MainMenuAnimator | `Animators/MainMenuAnimator.cs` | Entrada por fases: logo, header, botones stagger, nav |
| MatchmakingAnimator | `Animators/MatchmakingAnimator.cs` | Spinner búsqueda, reveal de oponente |
| RewardClaimAnimator | `Animators/RewardClaimAnimator.cs` | Monedas volando, popups, celebraciones |
| GameSelectorAnimator | `Animators/GameSelectorAnimator.cs` | Cards de juego con entrada staggered |
| CurrencyAnimator | `Animators/CurrencyAnimator.cs` | Feedback visual de ganancia/gasto |
| TrophyShowcaseAnimator | `Animators/TrophyShowcaseAnimator.cs` | Celebración unlock de logros |
| CashProfileAnimator | `Animators/CashProfileAnimator.cs` | Perfil CashBattle con efectos continuos |
| ParticleEffectSpawner | `Animators/ParticleEffectSpawner.cs` | Pool de partículas (confetti, sparkles, coins) |

### Componentes de Animación (11)
| Componente | Función |
|---|---|
| Button3D | Botón 3D con press/release, glow, partículas |
| BadgeAnimator | Pop-in de badges con punch |
| ScoreRevealAnimator | Contador de score con reveal dramático |
| StaggeredListAnimator | Entrada cascada de listas |
| TabTransitionAnimator | Transición de tabs con slide |
| NavTransitionAnimator | Transiciones de navegación |
| SceneTransitionManager | Fade entre escenas |
| AnimatedLoadingState | Estados de carga animados |
| EmptyStateAnimator | Estados vacíos con animación |

### Sistemas de Efectos
| Sistema | Archivo | Función |
|---|---|---|
| CelebrationManager | `Effects/CelebrationManager.cs` | Confetti (Small/Big/Epic), Fireworks, Star burst |
| ParticleSystemManager | `Effects/ParticleSystemManager.cs` | 7 tipos de partículas con pool de 20 |
| FeedbackManager | `Effects/FeedbackManager.cs` | Feedback general |
| FloatingText | `Effects/FloatingText.cs` | Texto flotante (damage, rewards) |
| NeonGlowEffect | `Effects/NeonGlowEffect.cs` | Efecto neon con shader |
| ButtonEffects | `Effects/ButtonEffects.cs` | Efectos de botones |
| GridGlowPulse | `UI/GridGlowPulse.cs` | Pulso glow en grids |
| Card3DEffect | `UI/Card3DEffect.cs` | Flip 3D de cartas (MemoryPairs) |

### Estadísticas Globales
- **79 archivos** importan `DG.Tweening`
- **102 archivos** usan CanvasGroup fade
- **79 archivos** usan StartCoroutine para timing
- **24 archivos** referencian ParticleSystem
- **~1000+ llamadas DOTween** en total
- **~150+ Sequences** DOTween
- **0 archivos .anim** propios (no usa Unity Animator clips)
- **0 archivos .controller** propios

---

## 2. INVENTARIO ESCENA POR ESCENA

### 🔑 AUTH (3 escenas)

#### Login.unity → LoginManager.cs
| Animación | Tipo | Estado |
|---|---|---|
| Title "Show" trigger | Unity Animator | ✅ Funciona |
| Panel fade-in | DOTween DOFade | ✅ Funciona |
| **FALTA** | Transición entre campos | ❌ No existe |
| **FALTA** | Botón login con feedback visual | ❌ No existe |

#### Register.unity → RegisterManager.cs
| Animación | Tipo | Estado |
|---|---|---|
| Panel transiciones | DOTween | ✅ Funciona |
| **FALTA** | Validación de campos animada | ❌ No existe |
| **FALTA** | Progreso step-by-step visual | ❌ No existe |

#### AgeVerification.unity → AgeVerificationManager.cs
| Animación | Tipo | Estado |
|---|---|---|
| Transiciones básicas | DOTween | ✅ Funciona |
| **FALTA** | Feedback visual edad seleccionada | ❌ No existe |

---

### 🏠 CORE (3 escenas)

#### Boot.unity → BootManager.cs + BootAnimator.cs
| Animación | Tipo | Estado |
|---|---|---|
| Logo animación entrada | BootAnimator (Coroutine) | ✅ Funciona |
| Barra de progreso | BootAnimator | ✅ Funciona |
| **FALTA** | Partículas de fondo | ❌ No existe |
| **FALTA** | Transición splash → menu fluida | ❌ No existe |

#### MainMenu.unity → MainMenuManager.cs + MainMenuAnimator.cs
| Animación | Tipo | Estado |
|---|---|---|
| Logo entrance + breathing | DOTween Sequence (5 fases) | ✅ Funciona |
| Header slide-in | DOTween DOAnchorPos | ✅ Funciona |
| Buttons stagger entrance | DOTween Insert con delay | ✅ Funciona |
| Featured content reveal | DOTween fade+scale | ✅ Funciona |
| Nav bar entrance | DOTween slide-up | ✅ Funciona |
| Logo glow pulse continuo | DOTween SetLoops(-1, Yoyo) | ✅ Funciona |
| Floating elements | DOTween SetLoops(-1, Yoyo) | ✅ Funciona |
| **FALTA** | Parallax en scroll | ❌ No existe |
| **FALTA** | Partículas ambientales neon | ❌ No existe |

#### Settings.unity → SettingsManager.cs
| Animación | Tipo | Estado |
|---|---|---|
| Panel transiciones | DOTween fade | ✅ Funciona |
| Dropdown interacción | DOTween | ✅ Funciona |
| **FALTA** | Toggle switches animados | ❌ No existe |
| **FALTA** | Feedback al cambiar tema | ❌ No existe |

---

### 🎮 GAMES - MINIGAMES (5 escenas)

#### DigitRush.unity → DigitRushController.cs
| Animación | Tipo | Estado |
|---|---|---|
| Combo partículas | ParticleSystem | ✅ Funciona |
| Haptic feedback | Vibración | ✅ Funciona |
| Error partículas | ParticleSystem | ✅ Funciona |
| **FALTA** | Countdown 3-2-1 dramático | ❌ No existe |
| **FALTA** | Combo multiplier visual escalating | ❌ No existe |
| **FALTA** | Screen shake en errores | ❌ No existe |
| **FALTA** | Efecto "fever mode" visual | ❌ No existe |

#### FlashTap.unity → FlashTapController.cs
| Animación | Tipo | Estado |
|---|---|---|
| Button3D press/release | Button3D component | ✅ Funciona |
| **FALTA** | Ripple effect al tocar | ❌ No existe |
| **FALTA** | Speed trails visuales | ❌ No existe |
| **FALTA** | Countdown con tensión creciente | ❌ No existe |

#### MemoryPairs.unity → MemoryPairsController.cs
| Animación | Tipo | Estado |
|---|---|---|
| Card flip 3D (scale-X morph) | Card3DEffect.cs | ✅ Funciona |
| Match success (green glow + pulse) | Card3DEffect.cs | ✅ Funciona |
| Error state (red glow + shake) | Card3DEffect.cs | ✅ Funciona |
| Combo celebration intensity | Card3DEffect.cs | ✅ Funciona |
| Victory jump | Card3DEffect.cs | ✅ Funciona |
| **FALTA** | Cartas que "respiran" face-down | ❌ No existe |
| **FALTA** | Match found → partículas connecting | ❌ No existe |

#### OddOneOut.unity → OddOneOutController.cs
| Animación | Tipo | Estado |
|---|---|---|
| Cell 3D press effects | OddOneOutCell3D.cs | ✅ Funciona |
| **FALTA** | Highlight progresivo del odd one | ❌ No existe |
| **FALTA** | Wrong answer → grid shake | ❌ No existe |

#### QuickMath.unity → QuickMathController.cs
| Animación | Tipo | Estado |
|---|---|---|
| Cell 3D interactions | QuickMathCell3D.cs | ✅ Funciona |
| **FALTA** | Números que "vuelan" al responder | ❌ No existe |
| **FALTA** | Streak visual fire effect | ❌ No existe |

---

### 🧭 GAMES - NAVIGATION (4 escenas)

#### GameSelector.unity → GameSelectorManager.cs + GameSelectorAnimator.cs
| Animación | Tipo | Estado |
|---|---|---|
| Game cards staggered entrance | GameSelectorAnimator | ✅ Funciona |
| Card selection highlight | DOTween | ✅ Funciona |
| **FALTA** | Card hover/preview 3D tilt | ❌ No existe |
| **FALTA** | Transición card → juego cinematográfica | ❌ No existe |

#### PlayModeSelection.unity → PlayModeSelectionManager.cs
| Animación | Tipo | Estado |
|---|---|---|
| Mode cards básicas | DOTween fade | ✅ Funciona |
| **FALTA** | Cards con parallax depth | ❌ No existe |

#### BetSelection.unity
| Animación | Tipo | Estado |
|---|---|---|
| Básicas | DOTween | ✅ Funciona |
| **FALTA** | Bet amount counter animado | ❌ No existe |

#### Matchmaking.unity → MatchmakingManager.cs + MatchmakingAnimator.cs
| Animación | Tipo | Estado |
|---|---|---|
| Search spinner ring | Unity Animator | ✅ Funciona |
| Opponent card reveal | DOTween Sequence | ✅ Funciona |
| VS text entrance | DOTween | ✅ Funciona |
| **FALTA** | Partículas eléctricas entre jugadores | ❌ No existe |
| **FALTA** | "FIGHT!" text con impact | ❌ No existe |

---

### 💰 MONETIZATION (4 escenas)

#### Shop.unity → PremiumManager.cs (ShopManager.cs)
| Animación | Tipo | Estado |
|---|---|---|
| Tab transitions | TabTransitionAnimator | ✅ Funciona |
| Theme preview | DOTween | ✅ Funciona |
| Item list stagger | StaggeredListAnimator | ✅ Funciona |
| **FALTA** | "HOT DEAL" badge pulsante | ❌ No existe |
| **FALTA** | Purchase celebration explosion | ❌ No existe |
| **FALTA** | Theme preview con transición morph | ❌ No existe |

#### Achievements.unity → AchievementsManager.cs
| Animación | Tipo | Estado |
|---|---|---|
| Trophy unlock celebration | TrophyShowcaseAnimator | ✅ Funciona |
| Detail panel reveal | DOTween | ✅ Funciona |
| List stagger | StaggeredListAnimator | ✅ Funciona |
| **FALTA** | Progress bar fill animado con partículas | ❌ No existe |
| **FALTA** | Locked → Unlocked transición dramática | ❌ No existe |

#### DailyMissions.unity → DailyMissionsManager.cs
| Animación | Tipo | Estado |
|---|---|---|
| Mission cards entrance | DOTween + DigitPark.Animations | ✅ Funciona |
| Progress bar fill | DOTween DOFillAmount | ✅ Funciona |
| Bonus claim animation | DOTween Sequence | ✅ Funciona |
| **FALTA** | Mission complete → confetti burst | ❌ No existe |
| **FALTA** | Timer countdown con urgencia visual | ❌ No existe |

#### DailyRewards.unity → DailyRewardsManager.cs
| Animación | Tipo | Estado |
|---|---|---|
| Reward reveal sequence | DOTween + Clash-style | ✅ Funciona |
| Claim animation | DOTween Sequence | ✅ Funciona |
| **FALTA** | Cofre/caja abriéndose con luz | ❌ No existe |
| **FALTA** | Reward item floating reveal | ❌ No existe |
| **FALTA** | Streak bonus visual escalation | ❌ No existe |

---

### 👥 SOCIAL (7 escenas)

#### Friends.unity → FriendsManager.cs
| Animación | Tipo | Estado |
|---|---|---|
| Friend cards stagger | StaggeredListAnimator | ✅ Funciona |
| Header transitions | DOTween | ✅ Funciona |
| **FALTA** | Online status pulse (green dot) | ❌ No existe |
| **FALTA** | Challenge friend → swoosh animation | ❌ No existe |

#### FriendRequests.unity → FriendRequestsSceneManager.cs
| Animación | Tipo | Estado |
|---|---|---|
| Tab transitions | TabTransitionAnimator | ✅ Funciona |
| Request item animations | DOTween | ✅ Funciona |
| **FALTA** | Accept → checkmark morph | ❌ No existe |
| **FALTA** | Decline → fade-out con swipe | ❌ No existe |

#### SearchPlayers.unity → SearchPlayersManager.cs
| Animación | Tipo | Estado |
|---|---|---|
| Search results stagger | DOTween | ✅ Funciona |
| **FALTA** | Search pulse while loading | ❌ No existe |
| **FALTA** | Result cards slide-in from right | ❌ No existe |

#### Profile.unity → ProfileManager.cs
| Animación | Tipo | Estado |
|---|---|---|
| Profile card animations | DOTween | ✅ Funciona |
| Stat reveals | DOTween | ✅ Funciona |
| **FALTA** | Stat counters animated counting up | ❌ No existe |
| **FALTA** | Level badge con shine sweep | ❌ No existe |
| **FALTA** | Avatar frame glow por rango | ❌ No existe |

#### Scores.unity → LeaderboardManager.cs
| Animación | Tipo | Estado |
|---|---|---|
| Leaderboard entries stagger | DOTween | ✅ Funciona |
| Game selector transitions | DOTween | ✅ Funciona |
| **FALTA** | Top 3 podium con entrada dramática | ❌ No existe |
| **FALTA** | "You" row highlight pulsante | ❌ No existe |
| **FALTA** | Rank change arrows animadas (↑↓) | ❌ No existe |

#### MatchHistory.unity → MatchHistorySceneManager.cs
| Animación | Tipo | Estado |
|---|---|---|
| Match entries list | DOTween | ✅ Funciona |
| Filter transitions | DOTween | ✅ Funciona |
| **FALTA** | Win/Loss color coded entrance | ❌ No existe |

#### Notifications.unity → NotificationsManager.cs
| Animación | Tipo | Estado |
|---|---|---|
| Notification cards | DOTween | ✅ Funciona |
| Tab transitions | DOTween | ✅ Funciona |
| **FALTA** | Swipe to dismiss | ❌ No existe |
| **FALTA** | New notification badge bounce | ❌ No existe |

---

### 🏆 TOURNAMENTS (3 escenas)

#### TournamentsBrowser.unity → TournamentsBrowserManager.cs
| Animación | Tipo | Estado |
|---|---|---|
| Tournament items stagger | DOTween + DigitPark.Animations | ✅ Funciona |
| Filter transitions | DOTween | ✅ Funciona |
| Load more animation | DOTween | ✅ Funciona |
| **FALTA** | Tournament card con timer countdown live | ❌ No existe |
| **FALTA** | Prize pool growing animation | ❌ No existe |

#### TournamentCreate.unity → TournamentCreateManager.cs
| Animación | Tipo | Estado |
|---|---|---|
| Form field animations | DOTween | ✅ Funciona |
| Prize estimation | DOTween | ✅ Funciona |
| **FALTA** | "Create" button con charging effect | ❌ No existe |

#### TournamentLobby.unity → TournamentLobbyManager.cs
| Animación | Tipo | Estado |
|---|---|---|
| Participant list stagger | DOTween + DigitPark.Animations | ✅ Funciona |
| Countdown animation | DOTween | ✅ Funciona |
| Tab transitions | DOTween | ✅ Funciona |
| **FALTA** | Participante entrando → slide-in con sonido | ❌ No existe |
| **FALTA** | Countdown final 3-2-1 dramático | ❌ No existe |

---

### 💸 CASHBATTLE (9 escenas)

#### CashBattle1v1.unity → CashBattleManager.cs
| Animación | Tipo | Estado |
|---|---|---|
| Card layout animations | DOTween | ✅ Funciona |
| Panel transitions | DOTween | ✅ Funciona |
| **FALTA** | Stake amount counter dramático | ❌ No existe |

#### CashBattleHub.unity → CashBattleManager.cs
| Animación | Tipo | Estado |
|---|---|---|
| Hub menu animations | DOTween | ✅ Funciona |
| Card reveals | DOTween | ✅ Funciona |
| **FALTA** | Balance display con counting animation | ❌ No existe |

#### CashMatchmaking.unity → CashMatchmakingManager.cs
| Animación | Tipo | Estado |
|---|---|---|
| Search ring spinner | Unity Animator | ✅ Funciona |
| Player/Opponent card reveal | DOTween Sequence | ✅ Funciona |
| **FALTA** | Partículas eléctricas entre avatares | ❌ No existe |
| **FALTA** | Stake amount reveal dramático | ❌ No existe |

#### CashProfile.unity + CashProfileAnimator.cs
| Animación | Tipo | Estado |
|---|---|---|
| Profile entrance | CashProfileAnimator | ✅ Funciona |
| Continuous effects | DOTween loops | ✅ Funciona |
| **FALTA** | Win rate visual gauge animado | ❌ No existe |

#### CashWallet.unity
| Animación | Tipo | Estado |
|---|---|---|
| Transiciones básicas | DOTween | ✅ Funciona |
| **FALTA** | Balance update con counting + particles | ❌ No existe |
| **FALTA** | Transaction history slide-in | ❌ No existe |

#### CashHistory.unity → CashHistorySceneController.cs
| Animación | Tipo | Estado |
|---|---|---|
| List animations | DOTween | ✅ Funciona |
| **FALTA** | Win entries → green glow, Loss → red subtle | ❌ No existe |

#### CashTournaments.unity → CashTournamentsManager.cs
| Animación | Tipo | Estado |
|---|---|---|
| Tournament cards | DOTween | ✅ Funciona |
| Filter transitions | DOTween | ✅ Funciona |
| **FALTA** | Real money badge con shine sweep | ❌ No existe |

#### CashTournamentCreate.unity → CashTournamentCreateManager.cs
| Animación | Tipo | Estado |
|---|---|---|
| Form animations | DOTween | ✅ Funciona |
| **FALTA** | Entry fee slider con haptic feedback | ❌ No existe |

#### CashTournamentLobby.unity → CashTournamentLobbyManager.cs
| Animación | Tipo | Estado |
|---|---|---|
| Participant list | DOTween | ✅ Funciona |
| Countdown | DOTween | ✅ Funciona |
| Chat animations | DOTween | ✅ Funciona |
| **FALTA** | Prize pool growing en tiempo real | ❌ No existe |

---

### 🎓 ONBOARDING (2 escenas)

#### Onboarding.unity → OnboardingManager.cs
| Animación | Tipo | Estado |
|---|---|---|
| Slide transitions | DOTween | ✅ Funciona |
| Progress dots | DOTween | ✅ Funciona |
| Input field animations | DOTween | ✅ Funciona |
| **FALTA** | Mascota/personaje guía animado | ❌ No existe |
| **FALTA** | Hand pointer tutorial gesture | ❌ No existe |

#### CashBattleOnboarding.unity → CashBattleOnboardingManager.cs
| Animación | Tipo | Estado |
|---|---|---|
| Continuous scroll (Clash-style) | DOTween | ✅ Funciona |
| Dot progress | DOTween | ✅ Funciona |
| Progress bar | DOTween | ✅ Funciona |
| **FALTA** | KYC step completion con checkmarks | ❌ No existe |

---

### 🎬 WIN/LOSE PANELS (7+ prefabs)

#### WinPanel_Normal, WinPanel_RealMoney, OnlineWinPanel, CashBattleWin, TournamentResultWin
| Animación | Tipo | Estado |
|---|---|---|
| Panel slide-in | DOTween | ✅ Funciona |
| Score reveal | ScoreRevealAnimator | ✅ Funciona |
| Celebration particles | CelebrationManager | ✅ Funciona |
| Currency claim fly | RewardClaimAnimator | ⚠️ Puede causar errores DOTween |
| **FALTA** | Confetti rain continuo de fondo | ❌ No existe |
| **FALTA** | Trophy/medal spin reveal 3D | ❌ No existe |
| **FALTA** | XP bar fill con level-up explosion | ❌ No existe |
| **FALTA** | "NEW HIGH SCORE!" shockwave text | ❌ No existe |

#### LosePanel_Normal, OnlineLosePanel, CashBattleLose, TournamentResultLose
| Animación | Tipo | Estado |
|---|---|---|
| Panel slide-in | DOTween | ✅ Funciona |
| Score reveal | DOTween | ✅ Funciona |
| **FALTA** | "Try Again" button con bounce invitador | ❌ No existe |
| **FALTA** | Stats comparison vs winner | ❌ No existe |

---

## 3. PROBLEMAS DOTWEEN DETECTADOS

### 🔴 CRÍTICOS (Probablemente causan los 30+ errores)

#### 3.1 RewardClaimAnimator.cs - Sequences huérfanas en loops
**Archivo**: `Scripts/Animations/Animators/RewardClaimAnimator.cs` (líneas 144-181)
**Problema**: Cada iteración del loop crea un `Sequence flySeq` local sin almacenar referencia. Si el GameObject se destruye antes de completar, las sequences quedan huérfanas intentando animar objetos destruidos.
**Impacto**: ~20-25 errores DOTween por sesión.
**Fix**: Almacenar todas las sequences en `List<Tween>` y matarlas en `OnDestroy()`.

#### 3.2 RewardClaimAnimator.cs - DOPath con posiciones inválidas
**Archivo**: `Scripts/Animations/Animators/RewardClaimAnimator.cs` (líneas 153-157)
**Problema**: `DOPath()` calcula la trayectoria con `iconRT.position` que puede haber sido modificada por tweens anteriores en el mismo frame.
**Impacto**: Monedas que vuelan a posiciones incorrectas o null references.

### 🟡 ALTOS

#### 3.3 InAppToastUI.cs - Sequence sin cleanup
**Archivo**: `Scripts/UI/InAppToastUI.cs` (líneas 317-345)
**Problema**: `PlayHideAnimation()` crea `Sequence hideSeq` localmente, la asigna a `_currentTween` como Tween pero la referencia original se pierde.
**Impacto**: 3-5 errores si toast se destruye durante animación.

#### 3.4 AchievementToastUI.cs - Mismo patrón
**Archivo**: `Scripts/UI/AchievementToastUI.cs` (líneas 353-387)
**Problema**: Mismo patrón que InAppToastUI.
**Impacto**: 2-3 errores.

#### 3.5 ErrorPanelUI.cs - Sin OnDestroy
**Archivo**: `Scripts/UI/Panels/ErrorPanelUI.cs` (líneas 108-122)
**Problema**: Crea sequences en `AnimateIn()` y `AnimateOut()` sin almacenar referencia ni tener `OnDestroy()`.
**Impacto**: 2-3 errores si panel se destruye por cambio de escena.

### 🟢 MEDIOS

#### 3.6 UIAnimations.cs - Métodos estáticos sin tracking
**Archivo**: `Scripts/Animations/Core/UIAnimations.cs`
**Problema**: Métodos estáticos retornan Tweens/Sequences que los callers pueden no almacenar/limpiar.
**Impacto**: Tweens huérfanos acumulativos.

#### 3.7 MatchmakingAnimator.cs - Sequences en coroutines
**Archivo**: `Scripts/Animations/Animators/MatchmakingAnimator.cs` (líneas 182-240)
**Problema**: `OpponentFoundCoroutine` crea sequences dentro de coroutines. Si la escena se descarga mid-coroutine, quedan huérfanas.

#### 3.8 Inconsistencia en SetUpdate
**Problema**: Algunos paneles usan `.SetUpdate(true)` (ignoreTimeScale) y otros no. Cuando `Time.timeScale` cambia, animaciones se comportan diferente.

### Resumen de Errores Estimados
| Fuente | Errores estimados |
|---|---|
| RewardClaimAnimator sequences huérfanas | ~20-25 |
| Toast sequences (InApp + Achievement) | ~5-8 |
| ErrorPanelUI sin cleanup | ~2-3 |
| Static UIAnimations sin tracking | ~1-2 |
| MatchmakingAnimator coroutines | ~1-2 |
| **TOTAL ESTIMADO** | **~30-40** |

---

## 4. PLAN DE ANIMACIONES — SOLO LAS QUE IMPORTAN

> Criterio de selección: ROI real para una app competitiva móvil.
> Descartamos animaciones de bajo impacto, baja frecuencia de uso, o alto consumo de batería/GPU.
> Solo 8 animaciones bien hechas > 25 mediocres.

### Por qué solo 8:
- **Living backgrounds** → consume batería y GPU, los usuarios notan lag antes que partículas
- **Matchmaking eléctrica** → demasiado complejo para 5 segundos de pantalla, el actual funciona
- **Chest opening** → DailyRewards se ve 1 vez al día, no justifica esfuerzo Alto
- **Achievement unlock rebuild** → ya tiene TrophyShowcaseAnimator, solo necesita polish
- **Leaderboard podium** → complejidad Alta para pantalla secundaria, no mueve retención
- **Card showcase gyroscope** → over-engineering, las cards con stagger ya funcionan
- **Pull-to-refresh custom** → el nativo de iOS/Android está bien, nadie nota uno custom
- **Login polish** → se ve 1 vez, no vale la inversión
- **Tournament lobby atmosphere** → nice-to-have, no must-have
- **Profile stats reveal** → pantalla de baja frecuencia
- **Skeleton loading, toggle animations, scroll particles** → V3+ someday

---

### TIER S — No-negociable (toda app profesional las tiene)

#### 4.1 Button Micro-Interactions (Global)
**Escenas**: TODAS
**Por qué es Tier S**: LA diferencia entre "app indie" y "app profesional". Clash Royale, Duolingo, Instagram — todos lo tienen. Sin esto la app se siente "muerta" al tocar.
**Descripción**:
- Press: scale down 0.95 + darken sutil (ya existe en Button3D, falta en botones normales)
- Release: bounce back con overshoot
- Primary CTA buttons: glow shimmer periódico para llamar atención
- Disabled → Enabled: color morph suave
**Complejidad**: Baja | **Esfuerzo**: ~1 día
**Inspiración**: iOS system buttons, Duolingo

#### 4.2 Scene Transitions Fluidas (Global)
**Escenas**: TODAS
**Por qué es Tier S**: El indicador #1 de calidad percibida. Un fade negro entre escenas grita "amateur". Es lo que el usuario ve CADA VEZ que navega.
**Descripción**:
- **Opción A - Iris wipe**: Círculo que se expande/contrae desde el punto tocado
- **Opción B - Slide con parallax**: Escena actual slide-out, nueva slide-in con capas a velocidades diferentes
- Color del wipe basado en el theme actual del usuario
**Complejidad**: Media | **Esfuerzo**: ~2 días
**Inspiración**: iOS app transitions, Material Design shared element transitions

#### 4.3 Countdown Cinematográfico 3-2-1 (Todos los minigames)
**Escenas**: DigitRush, FlashTap, MemoryPairs, OddOneOut, QuickMath + CashBattle1v1
**Por qué es Tier S**: Es el momento de TENSIÓN antes de cada partida. Sin esto el juego arranca "en frío". Todo juego competitivo lo tiene.
**Descripción**:
- Números gigantes en el centro con zoom-in → impacto → explota en partículas
- Screen shake sutil al impactar
- Colores: 3 (rojo) → 2 (amarillo) → 1 (verde) → GO! (blanco con glow neon)
- El "GO!" final hace shockwave que expande desde el centro
**Complejidad**: Media | **Esfuerzo**: ~1-2 días
**Inspiración**: UFC countdown, Formula 1 start lights

#### 4.4 Win Screen Celebration Deluxe (WinPanels)
**Escenas**: Todos los WinPanels (7 prefabs)
**Por qué es Tier S**: EL momento más importante de toda la app. Si ganar no se siente ÉPICO, el usuario no vuelve a jugar. Retención pura.
**Descripción**:
- **Fase 1** (0-0.5s): Screen flash blanco → fondo oscurece
- **Fase 2** (0.5-1.5s): Trophy/medal aparece con escala de 0→overshoot→1
- **Fase 3** (1.5-2.5s): Score counter rápido tipo slot machine (0 → score final)
- **Fase 4** (2.5-3.5s): Confetti rain continuo desde arriba, partículas doradas
- **Fase 5** (3.5-4s): XP bar fill con partículas y si hay level-up → explosion
- Si es **NEW HIGH SCORE**: texto gigante con shockwave + screen shake
**Complejidad**: Alta | **Esfuerzo**: ~2-3 días
**Inspiración**: Clash Royale victory, Candy Crush level complete

---

### TIER A — Alto impacto, implementar después del Tier S

#### 4.5 Currency Earn Celebration (Global)
**Escenas**: Todas las que otorgan monedas/gemas
**Por qué es Tier A**: Ya existe RewardClaimAnimator (con bugs). Mejorarlo = satisfacción dopamínica. Monedas volando al counter es ADICTIVO — Clash Royale, Coin Master, Subway Surfers todas lo tienen.
**Descripción**:
- Monedas/gemas salen del punto de origen como sprites individuales
- Vuelan en arco con trails dorados/esmeralda hacia el counter de la barra superior
- Al llegar cada una, el counter hace bump + incremento numérico rápido
- Si es cantidad grande: las monedas salen en ráfaga tipo "shower"
**Complejidad**: Media (mejora del existente) | **Esfuerzo**: ~1 día
**Inspiración**: Coin Master collect, Subway Surfers coin pickup

#### 4.6 Combo System Visual Escalation (Todos los minigames)
**Escenas**: DigitRush, FlashTap, MemoryPairs, OddOneOut, QuickMath
**Por qué es Tier A**: Core gameplay — lo que hace que el jugador quiera seguir acertando. El escalado visual (x3→x5→x10) crea "flow state". Es lo que separa un minigame olvidable de uno adictivo.
**Descripción**:
- **x2-x3**: Texto "COMBO x3" con punch scale, color verde
- **x4-x6**: Texto más grande, partículas por acierto, screen border glow verde
- **x7-x9**: "AMAZING!" text, partículas intensas, screen glow amarillo
- **x10+**: "INCREDIBLE!" / "UNSTOPPABLE!", fire effect en borders, haptic feedback fuerte
- **x15+**: "GODLIKE!" → pantalla entera con glow dorado, efectos máximos
**Complejidad**: Media-Alta | **Esfuerzo**: ~2 días
**Inspiración**: Guitar Hero streak, Beat Saber combo

#### 4.7 Notification Badge Bounce (MainMenu, Navigation)
**Escenas**: MainMenu.unity, todas las que tengan badges
**Por qué es Tier A**: 2 horas de trabajo, engagement garantizado. El badge que "respira" hace que el usuario toque. Micro-detalle que suma mucho.
**Descripción**:
- Entrada: scale 0→1.3→1 con bounce
- Idle: pulse sutil cada 3 segundos
- Nuevo: shake + glow intenso momentáneo
- Número cambia: flip animado del dígito
**Complejidad**: Muy Baja | **Esfuerzo**: ~2 horas

---

### DESCARTADAS (con justificación)

| Propuesta Original | Por qué NO |
|---|---|
| Matchmaking tensión eléctrica | Complejidad Alta para 5 segundos. El matchmaking actual (spinner + reveal) funciona bien |
| Daily reward chest opening | 1 vista/día. Complejidad Media-Alta no justificada por frecuencia de uso |
| Achievement unlock rebuild | TrophyShowcaseAnimator ya existe y funciona. No necesita rebuild completo |
| Leaderboard podium 3D | Complejidad Alta, pantalla secundaria, no impacta retención directamente |
| Main menu living background | Consume batería/GPU constantemente. Usuarios notan lag antes que estética |
| Game selector card showcase | Over-engineering. Cards con stagger ya se ven bien |
| Pull-to-refresh custom | El comportamiento nativo iOS/Android es lo que esperan los usuarios |
| Login/Register polish | Se ve 1 vez. Mejor invertir esas horas en gameplay |
| Tournament lobby atmosphere | Nice-to-have para V3+ cuando la base competitiva ya existe |
| Profile stats reveal | Pantalla de baja frecuencia, no mueve métricas |
| Skeleton loading | V3+ polish |
| Tab switch morph | Ya funciona con TabTransitionAnimator |
| Empty state illustrations | V3+ polish |
| Toggle switch iOS-style | V3+ polish |
| Theme change preview | V3+ polish |
| Mission progress particles | V3+ polish |
| Friend online indicator | V3+ polish |
| Scroll momentum particles | V3+ — riesgo de lag en listas largas |

---

## 5. PLAN DE IMPLEMENTACIÓN

### Orden de ejecución (total ~10-12 días)

#### Paso 0: Fix DOTween Errors (1 día) — PREREQUISITO
| # | Tarea | Impacto | Esfuerzo |
|---|---|---|---|
| 0a | Fix RewardClaimAnimator sequences huérfanas (3.1-3.2) | Elimina ~25 errores/sesión | Bajo |
| 0b | Fix Toast sequences cleanup — InAppToast + AchievementToast (3.3-3.4) | Elimina ~8 errores | Bajo |
| 0c | Fix ErrorPanelUI OnDestroy (3.5) | Elimina ~3 errores | Muy Bajo |
| 0d | Fix UIAnimations static tracking + SetUpdate inconsistency (3.6-3.8) | Previene errores acumulativos | Bajo |

**Sin este paso, las animaciones nuevas heredarán los mismos bugs.**

#### Paso 1: Quick Wins — Polish inmediato (1 día)
| # | Tarea | Esfuerzo |
|---|---|---|
| 1a | 4.1 Button micro-interactions en TODOS los botones | ~6 horas |
| 1b | 4.7 Notification badge bounce | ~2 horas |

**Resultado**: La app se siente "viva" al tocar. Esfuerzo mínimo, impacto máximo en percepción.

#### Paso 2: Scene Transitions (2 días)
| # | Tarea | Esfuerzo |
|---|---|---|
| 2a | 4.2 Implementar SceneTransitionManager con iris wipe o slide parallax | ~2 días |

**Resultado**: Cada navegación se siente fluida. Elimina el "parpadeo negro" amateur.

#### Paso 3: Gameplay Core (3-4 días)
| # | Tarea | Esfuerzo |
|---|---|---|
| 3a | 4.3 Countdown 3-2-1 cinematográfico en los 5 minigames + CashBattle | ~1-2 días |
| 3b | 4.6 Combo system visual escalation en los 5 minigames | ~2 días |

**Resultado**: Los minigames pasan de "funcionales" a "adictivos". El countdown crea tensión, los combos crean flow.

#### Paso 4: Celebration & Reward (3-4 días)
| # | Tarea | Esfuerzo |
|---|---|---|
| 4a | 4.4 Win screen celebration deluxe en 7 WinPanel prefabs | ~2-3 días |
| 4b | 4.5 Currency earn celebration (mejorar RewardClaimAnimator existente) | ~1 día |

**Resultado**: Ganar se siente ÉPICO. Recibir monedas es satisfactorio. El loop de "jugar → ganar → recompensa" queda completo.

---

## 6. REVISIÓN EXPERTA — AJUSTES AL PLAN

> Revisión post-auditoría con análisis de la infraestructura real existente.

### 6.1 Brecha Principal: 3D Buttons solo existen en Minigames

Los 3D buttons (Button3D, Cell3DButton, FlashTapButton3D) **solo se usan en minigames**.
Toda la UI de navegación (MainMenu, Auth, Shop, Settings, etc.) usa botones planos de Unity.
**Esa es la brecha visual más grande de la app.**

#### Dónde añadir Button3D (face/shadow/glow con depth real):

| Zona | Botones específicos | Impacto | Esfuerzo |
|------|---------------------|---------|----------|
| **MainMenu** | Play, Shop, Achievements, Premium, CashBattle (los 5 principales) | ALTÍSIMO — primera interacción del usuario | ~3-4 horas |
| **Auth** | Login, Register, Verify Age | Alto — primera impresión de la app | ~2 horas |
| **GameSelector** | Cards de cada minigame (5 cards) | Alto — selección de juego | ~2 horas |
| **BetSelection** | Bet amounts + Confirm | Alto — momento de decisión con dinero | ~1-2 horas |
| **WinPanels** | Play Again, Collect Reward, Share | Alto — loop de retención | ~2 horas |
| **Shop** | Buy/Purchase buttons | Medio — monetización directa | ~1-2 horas |
| **DailyRewards** | Claim button | Medio — engagement diario | ~30 min |
| **Matchmaking** | Cancel button | Bajo — poca interacción | ~30 min |

**Total estimado**: ~12-14 horas (~1.5-2 días)

#### Patrón de implementación:
- Reutilizar `Button3D.cs` existente (face/shadow/highlight hierarchy)
- Crear via UIBuilders (NO editar prefabs directamente)
- Cada botón necesita: Face (color principal), Side (borde 3D), Shadow (sombra baja)
- Press: face baja 4-6px, side se reduce, color oscurece sutilmente
- Release: bounce back con OutBack easing + overshoot
- CTA primarios: glow shimmer periódico (ya existe en Button3D)

### 6.2 Ajustes a las Tareas Existentes

#### 4.1 Button Micro-Interactions — Reducción de esfuerzo
**Original**: ~6 horas implementando sistema nuevo
**Ajustado**: ~2-3 horas — Ya existen `ButtonEffects.cs` y `UIAnimations.ButtonPress/ButtonBounce/ButtonPulse`.
Lo que falta NO es crear un sistema nuevo, sino **conectar** `ButtonEffects` a todos los botones UI que no son minigame.
Trabajo real: wiring + configuración, no implementación desde cero.

#### 4.2 Scene Transitions — Verificar antes de implementar
**Original**: ~2 días implementando SceneTransitionManager
**Ajustado**: Ya existe `SceneTransitionManager.cs` con 4 tipos de transición (Fade, CircleWipe, Slide, Flash).
**Antes de implementar**: verificar si ya está conectado a todas las escenas.
- Si NO está conectado → trabajo es **wiring** (~4-6 horas), no implementación
- Si SÍ está conectado → este paso ya está hecho, pasar al siguiente

#### 4.5 Currency Earn — Mejora, no rebuild
**Original**: ~1 día
**Confirmado**: RewardClaimAnimator ya existe y funciona. Solo necesita:
- Fix de sequences huérfanas (Paso 0)
- Polish visual (trails, bump counter)
- No es un rebuild completo

### 6.3 Nueva Tarea Recomendada: 3D Buttons para UI General

#### 4.8 Button3D en UI de Navegación (MainMenu + Auth + Key Screens)
**Escenas**: MainMenu, Login, Register, AgeVerification, GameSelector, BetSelection, WinPanels
**Por qué es Tier S**: Es LA diferencia entre "app indie con botones planos" y "app premium con depth". Los 3D buttons ya existen y funcionan en minigames — solo faltan en la UI general. Clash Royale, Brawl Stars, Coin Master — TODOS usan botones con profundidad.
**Descripción**:
- Convertir botones CTA principales a estructura Button3D (Face/Side/Shadow)
- MainMenu: los 5 botones principales con colores de tema (Play=cyan, Shop=naranja, etc.)
- Auth: botones de acción con glow sutil
- WinPanels: "Play Again" con glow shimmer invitador, "Collect" con glow dorado
**Complejidad**: Baja-Media (reutiliza componente existente) | **Esfuerzo**: ~1.5-2 días
**Inspiración**: Clash Royale battle button, Brawl Stars menu buttons

---

## 7. PLAN DE IMPLEMENTACIÓN REVISADO

### Orden de ejecución (total ~9-11 días)

#### Paso 0: Fix DOTween Errors (1 día) — PREREQUISITO
| # | Tarea | Impacto | Esfuerzo |
|---|---|---|---|
| 0a | Fix RewardClaimAnimator sequences huérfanas (3.1-3.2) | Elimina ~25 errores/sesión | Bajo |
| 0b | Fix Toast sequences cleanup — InAppToast + AchievementToast (3.3-3.4) | Elimina ~8 errores | Bajo |
| 0c | Fix ErrorPanelUI OnDestroy (3.5) | Elimina ~3 errores | Muy Bajo |
| 0d | Fix UIAnimations static tracking + SetUpdate inconsistency (3.6-3.8) | Previene errores acumulativos | Bajo |

**Sin este paso, las animaciones nuevas heredarán los mismos bugs.**

#### Paso 1: Quick Wins + 3D Buttons (2 días)
| # | Tarea | Esfuerzo |
|---|---|---|
| 1a | 4.1 Conectar ButtonEffects a todos los botones UI (wiring, no implementación) | ~2-3 horas |
| 1b | 4.7 Notification badge bounce | ~2 horas |
| 1c | 4.8 Button3D en MainMenu (5 botones principales) | ~3-4 horas |
| 1d | 4.8 Button3D en Auth + WinPanels + GameSelector | ~4-6 horas |

**Resultado**: La app se siente "viva" y con profundidad al tocar. Los botones ya no son planos.

#### Paso 2: Scene Transitions (0.5-2 días)
| # | Tarea | Esfuerzo |
|---|---|---|
| 2a | Verificar wiring de SceneTransitionManager existente | ~1 hora |
| 2b | Conectar/implementar transiciones faltantes (si las hay) | ~4h-2 días |

**Resultado**: Cada navegación se siente fluida. Elimina el "parpadeo negro" amateur.

#### Paso 3: Gameplay Core (3-4 días)
| # | Tarea | Esfuerzo |
|---|---|---|
| 3a | 4.3 Countdown 3-2-1 cinematográfico en los 5 minigames + CashBattle | ~1-2 días |
| 3b | 4.6 Combo system visual escalation en los 5 minigames | ~2 días |

**Resultado**: Los minigames pasan de "funcionales" a "adictivos".

#### Paso 4: Celebration & Reward (3-4 días)
| # | Tarea | Esfuerzo |
|---|---|---|
| 4a | 4.4 Win screen celebration deluxe en 7 WinPanel prefabs | ~2-3 días |
| 4b | 4.5 Currency earn celebration (polish de RewardClaimAnimator existente) | ~1 día |

**Resultado**: Ganar se siente ÉPICO. El loop "jugar → ganar → recompensa" queda completo.

---

## RESUMEN EJECUTIVO

### Estado Actual
- **42 escenas** en la app
- **79 archivos** usan DOTween
- **~30-40 errores DOTween** causados principalmente por RewardClaimAnimator (sequences huérfanas)
- La app tiene un **sistema de animación robusto** (UIAnimations 40+ métodos, UIAnimationManager, 8 animadores, 11 componentes, 6 sistemas de efectos)
- **3D Buttons existen** (Button3D, Cell3DButton, FlashTapButton3D) pero **solo en minigames** — toda la UI de navegación usa botones planos
- **SceneTransitionManager existe** con 4 tipos (Fade, CircleWipe, Slide, Flash) — verificar wiring
- **ButtonEffects existe** pero no está conectado a botones de UI general

### Plan Revisado: 9 animaciones, ~9-11 días
| Tier | Animación | Esfuerzo |
|------|-----------|----------|
| Pre | Fix DOTween errors (3.1-3.8) | 1 día |
| S | Button micro-interactions (wiring de ButtonEffects existente) | 2-3 horas |
| S | **Button3D en UI general** (MainMenu, Auth, WinPanels, GameSelector) | 1.5-2 días |
| S | Scene transitions (verificar wiring + conectar faltantes) | 0.5-2 días |
| S | Countdown 3-2-1 cinematográfico | 1-2 días |
| S | Win celebration deluxe | 2-3 días |
| A | Currency earn celebration (polish existente) | 1 día |
| A | Combo visual escalation | 2 días |
| A | Notification badge bounce | 2 horas |
| **TOTAL** | | **~9-11 días** |

### Cambios vs plan original
- **Añadido**: Button3D en UI general (Tier S) — brecha visual más grande de la app
- **Reducido**: Button micro-interactions de 6h a 2-3h (reutilizar ButtonEffects existente)
- **Reducido**: Scene transitions de 2 días a 0.5-2 días (SceneTransitionManager ya existe)
- **Total**: de ~10-12 días a ~9-11 días (menos tiempo, más impacto)

### Impacto esperado
La app pasa de "funcional con botones planos y fades básicos" a "profesional con depth y juice" — al nivel de **una app indie premium bien pulida** como Wordle, Duolingo, o Monument Valley en calidad de interacción. Los 3D buttons cierran la brecha más visible entre la UI de minigames (ya pulida) y la UI de navegación (actualmente plana).

---

## 8. CHECKLIST DE TASKS (Ejecución)

### PASO 0 — Fix DOTween Bugs (PREREQUISITO)
- [x] **T0a** — Fix RewardClaimAnimator: almacenar sequences en `_activeTweens`, kill en OnDestroy, cachear startPos para DOPath
- [x] **T0b** — Fix InAppToastUI: añadir DOKill de toastContainer, canvasGroup, borderOutline en OnDestroy
- [x] **T0c** — Fix AchievementToastUI: ya tenía buen cleanup (DOKill en targets), verificado OK
- [x] **T0d** — Fix ErrorPanelUI: añadir `_currentSequence` field, almacenar sequences, kill en OnDestroy
- [x] **T0e** — Fix MatchmakingAnimator: almacenar cardSlideSequence, vsSequence, quickVSSequence, vsGlowTween en fields, kill todos en OnDestroy

### PASO 1 — Quick Wins + 3D Buttons
> **NOTA**: Ya existen herramientas de Editor que hacen el batch automáticamente:
> - `AnimationSystemBatchSetup.cs` → "Convert ALL Buttons to Button3D" + "Add SimplePulse to CTA"
> - `EffectsSetup.cs` → "Setup All Scenes" (ButtonEffects a todos los botones, 40 escenas)
> - `UIBuilderAnimationUtils.cs` → AddButton3D(), CreateCTAButton(), AddPulse()
> - `AnimationManagersRepairTool.cs` → Repair ---ANIMATION_MANAGERS--- en todas las escenas

- [ ] **T1a** — EN UNITY: Ejecutar `DigitPark/Animation/Batch/FASE 1: Repair All ANIMATION_MANAGERS`
- [ ] **T1b** — EN UNITY: Ejecutar `DigitPark/Animation/Batch/APPLY ALL ANIMATIONS TO ALL SCENES` (Button3D + SimplePulse)
- [ ] **T1c** — EN UNITY: Ejecutar `DigitPark/Effects/Setup All Scenes` (ButtonEffects en 40 escenas)
- [ ] **T1d** — Notification badge bounce (BadgeAnimator en MainMenu + Navigation)

### PASO 2 — Scene Transitions
- [x] **T2a** — Verificado: SceneTransitionManager existe pero NO estaba conectado (0 llamadas runtime)
- [x] **T2b** — Conectado SceneNavigator.NavigateTo() y GoBack() a SceneTransitionManager.FadeTransition()

### PASO 3 — Gameplay Core
- [x] **T3a** — Countdown 3-2-1 cinematográfico: CountdownAnimator.cs creado (self-contained, DOTween, shockwave+shake)
- [x] **T3b** — Integrar countdown en 5 minigames: DigitRush, FlashTap (2 call sites), MemoryPairs, OddOneOut, QuickMath (nuevo)
- [x] **T3c** — CashBattle1v1 es lobby de selección, no escena de juego. Countdown ya cubierto via minigame controllers
- [x] **T3d** — ComboVisualController.cs creado: 5 tiers (x2→GODLIKE!), border glow, milestone text, combo break effect
- [x] **T3e** — Combo visuals integrados en 4 juegos con combo: DigitRush, OddOneOut, MemoryPairs, QuickMath (FlashTap no tiene combo)

### PASO 4 — Celebration & Reward
- [x] **T4a** — WinCelebrationAnimator.cs creado: flash blanco/rojo, confetti rain (6 colores), icon pop-in, NEW HIGH SCORE shockwave
- [x] **T4b** — Integrado en WinPanelController: PlayWin para victoria (normal + real money), PlayLose para derrota. Todos los 7 prefabs lo usan automáticamente
- [x] **T4c** — RewardClaimAnimator mejorado: nuevo shower mode (burst radial → fly-to-target rápido), auto-activación para cantidades 100+, counter tick rápido con bump
