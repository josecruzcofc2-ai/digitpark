# DigitPark — Auditoría Exhaustiva del Sistema de Animaciones

> Generado: 2026-03-19 | Versión del proyecto: V53b | **COBERTURA: 80/80 archivos (100%)**

---

## RESUMEN EJECUTIVO

El sistema de animaciones de DigitPark está construido **100% en código** usando **DOTween** como biblioteca principal. No existe ningún uso de Unity Animator, AnimationClips ni state machines. El sistema abarca **80 archivos** con código de animación distribuidos en 6 capas: infraestructura core, animadores especializados por escena, gestores de efectos, componentes reutilizables, paneles de resultado, y managers/UI con animaciones integradas.

**Biblioteca principal:** DOTween Pro
**Biblioteca secundaria:** Coroutines + Lerp (efectos de partículas y shake)
**Sin uso de:** Unity Animator, AnimationClips, LeanTween, iTween

---

## ESTRUCTURA DE CARPETAS

```
Assets/_Project/Scripts/Runtime/
├── Animations/
│   ├── Core/
│   │   ├── UIAnimationManager.cs       ← Singleton central de animaciones UI
│   │   └── UIAnimations.cs             ← Librería estática de 40+ métodos
│   ├── AnimConstants.cs                ← Constantes globales de duración y easing
│   └── Animators/
│       ├── MainMenuAnimator.cs
│       ├── GameSelectorAnimator.cs
│       ├── MatchmakingAnimator.cs
│       ├── CurrencyAnimator.cs
│       ├── RewardClaimAnimator.cs
│       ├── TrophyShowcaseAnimator.cs
│       └── CashProfileAnimator.cs
└── Effects/
    ├── CelebrationManager.cs
    ├── FeedbackManager.cs
    ├── ButtonEffects.cs
    ├── ParticleSystemManager.cs
    ├── VictoryEffectPlayer.cs
    ├── FloatingText.cs
    └── NeonGlowEffect.cs
```

---

## A. INFRAESTRUCTURA CORE

### A1. `UIAnimationManager.cs`
**Ruta:** `Scripts/Runtime/Animations/Core/UIAnimationManager.cs`
**Tipo:** Singleton MonoBehaviour
**Rol:** Hub central para animaciones de UI globales. Otros sistemas lo usan para efectos de pantalla completa.

| Método | Descripción | Duración | Easing |
|--------|-------------|----------|--------|
| `ScreenFlash(color)` | Overlay fade in/out de color | 0.3s | OutQuad |
| `WhiteFlash()` | Flash blanco (acierto) | 0.3s | OutQuad |
| `RedFlash()` | Flash rojo (error/daño) | 0.3s | OutQuad |
| `GoldFlash()` | Flash dorado (recompensa) | 0.3s | OutQuad |
| `ScreenShake(intensity, duration)` | DOShakePosition en cámara/canvas | Configurable | — |
| `BumpCurrencyDisplay()` | DOPunchScale en HUD de moneda | 0.15s | — |
| `PrepareForEntrance(elements[])` | Pone elementos en escala 0 listos para animar | Instant | — |
| `StaggeredEntrance(elements[], delay)` | Escala encadenada de items | 0.1s/item | OutBack |

---

### A2. `UIAnimations.cs`
**Ruta:** `Scripts/Runtime/Animations/Core/UIAnimations.cs`
**Tipo:** Clase estática (sin MonoBehaviour)
**Rol:** Librería de métodos de animación reutilizables llamada desde cualquier Manager.

#### Constantes de duración
| Constante | Valor |
|-----------|-------|
| `INSTANT` | 0.1s |
| `FAST` | 0.2s |
| `NORMAL` | 0.3s |
| `SLOW` | 0.5s |
| `VERY_SLOW` | 0.8s |

#### Categorías de métodos

**Botones**
- `ButtonPress(btn)` — Scale 0.92x → OutQuad → OutBack release
- `ButtonBounce(btn)` — Scale 1.3x → OutBounce
- `Button3DPress(btn)` — DOAnchorPosY con efecto profundidad

**Paneles / Ventanas**
- `SlideIn(panel, direction)` — Slide desde borde con OutBack
- `SlideOut(panel, direction)` — Slide hacia borde con InBack
- `PopupShow(panel)` — Scale 0→1 con OutBack
- `PopupHide(panel)` — Scale 1→0 con InBack
- `FadeIn(group)` — DOFade 0→1
- `FadeOut(group)` — DOFade 1→0

**Texto**
- `CounterAnimation(text, from, to, format)` — DOTween.To() con OutQuad
- `TypewriterEffect(text, content)` — Reveal carácter a carácter

**Recompensas**
- `FlyToTarget(icon, target)` — Arco curvo + scale down
- `RewardCelebration(element)` — Scale + bounce + punch rotation

**Efectos**
- `Shake(element)` — DOShakePosition
- `ShakeRotation(element)` — DOShakeRotation
- `GlowPulse(graphic)` — DOColor loop yoyo con InOutSine
- `Flash(graphic, color)` — Flash rápido de color
- `Spin(element)` — Rotación 360° continua
- `Float(element)` — Movimiento arriba/abajo con InOutSine
- `Breathe(element)` — Scale pulse sutil

**Combo**
- `ComboText(text, comboLevel)` — Scale + shake, intensidad según nivel
- `UltraCombo(text)` — Screen shake + zoom

**Entradas de lista**
- `StaggeredEntrance(items[], delay)` — Scale encadenado
- `CascadeEntrance(items[], direction, delay)` — Slide encadenado

---

### A3. `AnimConstants.cs`
**Ruta:** `Scripts/Runtime/Animations/AnimConstants.cs`
**Tipo:** Clase estática de constantes

```
Duraciones generales:  QUICK=0.15s, MEDIUM=0.25s, ENTER=0.35s

Easing presets:
  ENTER    = Ease.OutBack
  EXIT     = Ease.InBack
  EMPHASIS = Ease.OutBounce
  FADE     = Ease.OutQuad
  SMOOTH   = Ease.InOutQuad
  BOUNCE   = Ease.OutElastic

Countdown:
  FADE_IN=0.4s, NUMBER_IN=0.3s, NUMBER_OUT=0.25s
  GO_POP=0.35s, SHOCKWAVE=0.5s

Toast notifications:
  SLIDE_IN=0.3s, DISPLAY=5s, SLIDE_OUT=0.25s

Scale presets:
  START=0.85f, PULSE=1.08f, GO_POP=1.3f

UI interaction:
  BUTTON=0.08s, HOVER=0.12s, COLOR_FLASH=0.1s, FLY=0.4s
```

---

## B. ANIMADORES ESPECIALIZADOS

### B1. `MainMenuAnimator.cs`
**Ruta:** `Scripts/Runtime/Animations/Animators/MainMenuAnimator.cs`
**Escena:** MainMenu

**Secuencia de entrada (9 fases):**

| Fase | Elemento | Animación | Duración | Easing |
|------|----------|-----------|----------|--------|
| 1 | Logo | DOAnchorPosY caída | 0.5s | OutBounce |
| 1 | Logo | DOScale 0→1 | 0.4s | OutBack |
| 2 | Logo glow | DOFade pulse loop | ∞ | InOutSine |
| 3 | Header | DOAnchorPosY slide down | 0.35s | OutQuad |
| 4 | CurrencyDisplay | DOScale pop | 0.3s | OutBack |
| 5 | Botones menú | DOScale escalonado (+0.1s/item) | 0.3s/item | OutBack |
| 6 | FeaturedBanner | DOAnchorPosX slide | 0.4s | OutQuad |
| 7 | BottomNav | DOAnchorPosY slide up | 0.3s | OutQuad |
| 8 | FloatingElements | DOAnchorPosY float continuo | ∞ | InOutSine Yoyo |

**Efecto de salida:** Todo reverso (InBack) + fade
**Eventos:** `OnEntranceComplete`

---

### B2. `GameSelectorAnimator.cs`
**Ruta:** `Scripts/Runtime/Animations/Animators/GameSelectorAnimator.cs`
**Escena:** GameSelector

| Animación | Descripción |
|-----------|-------------|
| Card entrance | Scale + slide up, escalonado |
| Card navegación | Scale 1.0→1.15 (seleccionada), resto DOFade dimmed |
| Parallax layers | DOAnchorPosX basado en scroll position |
| Selection glow | DOFade pulse infinito (InOutSine) |
| New badge | Scale pop + pulse loop |
| Confirmación | Scale zoom + punch rotation + otras cards fade out |

**Eventos:** `OnCardSelected`, `OnCardConfirmed`

---

### B3. `MatchmakingAnimator.cs`
**Ruta:** `Scripts/Runtime/Animations/Animators/MatchmakingAnimator.cs`
**Escena:** Matchmaking

| Animación | Descripción | Duración | Easing |
|-----------|-------------|----------|--------|
| Search ring | DORotate -360° continuo | ∞ | Linear |
| Search dots | Fade escalonado in/out | loop | InOutSine |
| Opponent reveal | DOScale 0→1 | 0.4s | OutBack |
| Card slide | DOAnchorPosX desde lados | 0.5s | OutBack |
| VS pop | Scale 0→1.5→1 + rotation shake | 0.4s | — |
| Shockwave | Scale up + DOFade out | 0.5s | — |
| Screen shake | DOShakePosition (20f) | 0.3s | — |

**Eventos:** `OnMatchFound`, `OnBattleReady`

---

### B4. `CurrencyAnimator.cs`
**Ruta:** `Scripts/Runtime/Animations/Animators/CurrencyAnimator.cs`
**Escena:** Global (HUD)

| Animación | Trigger | Detalles |
|-----------|---------|---------|
| Counter | Cualquier cambio | DOTween.To() OutQuad |
| Color flash gain | Ganar moneda | Verde → original |
| Color flash spend | Gastar moneda | Naranja → original |
| Punch scale | Cambio de cantidad | DOPunchScale container |
| Icon rotation | Ganar moneda | DOPunchRotation |
| Glow in/out | Momentos de moneda | DOFade |
| Insufficient funds | Sin fondos | DOShakePosition + red flash loop |
| Plus button highlight | Fondos bajos | Scale bounce loop + glow loop |
| Flying icon | Recibir moneda | DOMove arco + DOScale + DORotate |

---

### B5. `RewardClaimAnimator.cs`
**Ruta:** `Scripts/Runtime/Animations/Animators/RewardClaimAnimator.cs`
**Escenas:** DailyRewards, Shop, Missions

**Modo Estándar (amount < 100):**
- Íconos pop scale 1.3x → vuelan al target con arco
- Spawn escalonado (0.05s por ícono, máx 10)
- Bump en counter al llegar

**Modo Shower (amount >= 100):**
- Íconos burst radial 1.5x
- Todos vuelan al target tras 0.6s
- Counter se actualiza rápido (4× rotaciones)
- Big bump final

**Reward Popup:**
- Scale pop (0.3s OutBack) → settle (0.2s)
- Punch rotation en rewards grandes
- Screen flash dorado
- Icon bounce animation

---

### B6. `TrophyShowcaseAnimator.cs`
**Ruta:** `Scripts/Runtime/Animations/Animators/TrophyShowcaseAnimator.cs`
**Escena:** Achievements/Trophies

| Animación | Descripción |
|-----------|-------------|
| Card entrance | Scale 0.3→1.1→1 + fade + punch (0.08s stagger) |
| Tab transition | Cards fade+scale out → nuevas cascade in |
| Header trophy | DOAnchorPosY breathing (InOutSine yoyo) |
| Unlock celebration | Icon scale pop + infinite pulse |
| Celebration glow | DOFade pulse yoyo |
| Hover/press | Scale 1.05x / 0.97x |
| Detail panel | Scale pop desde posición de card (0.35s) |
| Progress bar | DOFillAmount OutQuad |
| Shine sweep | DOAnchorPosX traverse |

**Eventos:** `OnTabChanged`, `OnTrophyUnlocked`

---

### B7. `CashProfileAnimator.cs`
**Ruta:** `Scripts/Runtime/Animations/Animators/CashProfileAnimator.cs`
**Escena:** CashBattle Profile

**Secuencia de entrada (7 fases encadenadas):**

| Fase | Elemento | Animación | Duración | Easing |
|------|----------|-----------|----------|--------|
| 1 | Header | DOAnchorPosY slide down | 0.35s | OutQuad |
| 2 | Gold separator | DOFade in | 0.25s | OutQuad |
| 3 | Avatar card | DOScale pop | 0.4s | OutBack |
| 4 | Hero stats (×3) | DOFade + slide, 0.1s stagger | 0.3s | OutQuad |
| 5 | Counters | DOTween.To() number animate | 0.8s | OutQuad |
| 6 | Section header | DOFade | 0.3s | OutQuad |
| 7 | Stats grid (×10) | Cascade slide+fade, 0.06s stagger | 0.25s | OutQuad |

**Efectos continuos:**
- Avatar ring: Scale 1.04x breathing (2s, InOutSine)
- Gold separator: Glow pulse (1.5s)
- Accent lines: Glow pulse escalonado

---

## C. GESTORES DE EFECTOS

### C1. `CelebrationManager.cs`
**Ruta:** `Scripts/Runtime/Effects/CelebrationManager.cs`
**Tipo:** Singleton. Genera sistemas de partículas por código.

#### Tipos de partículas

| Tipo | Partículas | Duración | Colores | Especial |
|------|-----------|---------|---------|---------|
| Confetti Small | 50 | 2s | Neon palette ciclo | Gravity 0.5f, flutter |
| Confetti Big | 150 | 4s | Neon palette ciclo | Gravity 0.5f, flutter |
| Confetti Epic | 300 | 6s | Neon palette ciclo | Gravity 0.5f, flutter |
| Fireworks | 5 bursts × 60 | 3s | White→colored→white | Trail, radial |
| Star Burst | 30 stars | 1s | Gold gradient | Size curve, desde centro |

**Reducción de movimiento:** Respeta `AccessibilityHelper.ReducedMotion` (desactiva partículas o reduce count)

---

### C2. `FeedbackManager.cs`
**Ruta:** `Scripts/Runtime/Effects/FeedbackManager.cs`
**Tipo:** Singleton. Coordina haptics + partículas + efectos de pantalla.

#### Tipos de feedback

| Tipo | Haptic | Partículas | Pantalla |
|------|--------|-----------|---------|
| Button | Light | NeonBurst pequeño | — |
| Important | Medium | NeonBurstLarge | — |
| Success | Heavy | SuccessBurst | Green flash |
| Error | Heavy | ErrorBurst | Red flash + shake |
| Tile tap | Light | Ripple | — |
| Correct move | Medium | SuccessBurst | — |
| Wrong move | Medium | ErrorBurst | Shake |
| Combo | Variable (×combo) | ComboBurst | — |

#### Tipos de celebración

| Nivel | Efectos combinados |
|-------|-------------------|
| Small | Confetti small |
| Big | Confetti big + Fireworks + Screen shake + Gold flash pulse |
| Epic | Confetti epic + Fireworks + StarBurst + Heavy shake + White flash + Gold pulses ×3 |

**Screen shake:** Coroutine con Perlin-noise decay
**Screen flash:** Manual `Color.Lerp` fade (no DOTween)

---

### C3. `ButtonEffects.cs`
**Ruta:** `Scripts/Runtime/Effects/ButtonEffects.cs`
**Tipo:** Componente adjunto a botones.

| Tipo de botón | Color de flash | Comportamiento extra |
|--------------|---------------|---------------------|
| Normal | Cyan | Scale press 0.92x |
| Important | Gold | Scale press + glow pulse |
| Success | Green | Triple bounce |
| Danger | Red | Shake on error |
| Premium | Gradient | Glow pulse continuo |

**Glow pulse:** Throttled a 30fps para performance
**Press animation:** Scale 0.92x → OutBack release (0.08s)

---

### C4. `ParticleSystemManager.cs`
**Ruta:** `Scripts/Runtime/Effects/ParticleSystemManager.cs`
**Tipo:** Factory + Pool de sistemas de partículas.

#### Catálogo completo de partículas

| ID | Lifetime | Speed | Size | Burst | Color | Forma |
|----|----------|-------|------|-------|-------|-------|
| NeonBurst | 0.4s | 5 m/s | 0.1 | 20 | Cyan→Purple | Radial |
| NeonBurstLarge | 0.6s | 8 m/s | 0.15 | 30 | Cyan→Purple | Radial |
| SuccessBurst | 0.8s | 6 m/s | 0.12 | 25 | Green→Gold→Cyan | Radial + gravity |
| ErrorBurst | 0.3s | 4 m/s | 0.1 | 15 | Red gradient | Radial |
| Ripple | 0.5s | 0 | 0.1→2 | 1 | Cyan | Ring expand |
| Sparkle | Loop | 1 m/s | 0.05-0.1 | 10/s | Gold→Cyan | Continuous |
| ComboBurst | 1.0s | 7 m/s | 0.15 | 20 | Gold→Cyan→Purple | Cono arriba |

**Pool:** Auto-return al pool cuando las partículas terminan.

---

### C5. `VictoryEffectPlayer.cs`
**Ruta:** `Scripts/Runtime/Effects/VictoryEffectPlayer.cs`

- Aplica colores custom a sistemas de partículas preconfigurados
- Auto-destruye tras la duración más larga
- **Evento:** `OnEffectComplete`

---

### C6. `FloatingText.cs`
**Ruta:** `Scripts/Runtime/Effects/FloatingText.cs`
**Uso:** Feedback visual de puntos/combos sobre el juego.

#### Tipos de texto flotante

| Tipo | Font Size | Scale inicial | Duración | Color |
|------|-----------|--------------|---------|-------|
| Points | 36 | 1.0x | 1.5s | Cyan |
| Combo | 42 | 1+(nivel×0.1) | 1.8s | Gold |
| Perfect | 48 | 1.3x | 2.0s | Green |
| Excellent | 48 | 1.4x | 2.0s | Gold |
| NewRecord | 56 | 1.5x | 2.5s | Gold |
| TimeBonus | 32 | 1.0x | 1.5s | Purple |
| Error | 28 | 1.0x | 1.5s | Red |

**Secuencia de animación (coroutine manual):**
1. Punch scale: 0.5 → 1.3 → 1 en 0.15s
2. Float up: `y += floatSpeed * t * (1 - t*0.5)`
3. Wobble: `sin(t×4π) × 10f` horizontal
4. Fade out: inicia en t=0.6
5. Scale reduce: de t=0.7 a t=1.0

---

### C7. `NeonGlowEffect.cs`
**Ruta:** `Scripts/Runtime/Effects/NeonGlowEffect.cs`
**Tipo:** Componente adjunto a UI elements.

| Modo | Descripción | Técnica |
|------|-------------|---------|
| Static | Color fijo | Outline set directo |
| Pulse | Alpha pulso suave | Lerp minGlow→maxGlow, InOutSine |
| Rainbow | Ciclo de colores | HSVToRGB hue cycle |
| Breathing | Pulso orgánico | Doble sine wave |
| Flicker | Parpadeo aleatorio | Random + random wait |

---

## D. COMPONENTES DE ANIMACIÓN REUTILIZABLES

### D1. `CountdownAnimator.cs`
**Animación:** Cuenta regresiva 3-2-1-GO! pantalla completa

| Fase | Color | Animación |
|------|-------|-----------|
| 3 | Cyan | Scale + fade, shockwave |
| 2 | Gold | Scale + fade, shockwave |
| 1 | Red | Scale + fade, shockwave |
| GO! | Green | Scale 1.3x pop + shockwave expand 15x |

Shockwave: Scale 0→15, DOFade out, 0.5s

---

### D2. `AnimatedPanel.cs`
**Componente genérico** para paneles con animación de entrada/salida.

| Tipo | Entrada | Salida |
|------|---------|--------|
| ScaleFade | Scale 0→1 + fade | Scale 1→0 + fade |
| SlideUp | Desde abajo | Hacia abajo |
| SlideDown | Desde arriba | Hacia arriba |
| SlideLeft | Desde izquierda | Hacia izquierda |
| SlideRight | Desde derecha | Hacia derecha |
| FadeOnly | Solo fade in | Solo fade out |

- Backdrop: DOFade coordinado
- `SetUpdate(true)` — ignora TimeScale
- Instant fallback para accesibilidad

---

### D3. `Button3D.cs`
**Componente** para botones con efecto de profundidad 3D.

| Estado | Animación |
|--------|-----------|
| Normal | Glow pulse continuo (hover) |
| Hover | Scale 1.03x, glow increase |
| Press | Face baja Y + shadow reduce + squash |
| Release | OutBack recovery |
| Partículas | Opcional: particles continuas en hover |

---

### D4. `UIEffects.cs`
**Componente modular** de efectos individuales.

| Efecto | Descripción |
|--------|-------------|
| Shine sweep | DOAnchorPosX traverse + loop |
| Pulse scale | Scale yoyo infinito |
| Float | Y yoyo infinito |
| Glow pulse | DOFade yoyo infinito |
| Rotation | Spin FastBeyond360 |
| Color cycle | Callback loop de colores |

---

### D5. `StaggeredListAnimator.cs`
**Componente** para animar listas y grids.

- Entrada: slide up + fade + scale opcional (0.05s/item)
- Salida: stagger reverso
- `OnEnable` auto-animate opcional

---

### D6. Otros componentes especializados

| Archivo | Propósito |
|---------|-----------|
| `SceneTransitionManager.cs` | Transiciones entre escenas (fade/slide) |
| `BadgeAnimator.cs` | Animación de badges/notificaciones |
| `EmptyStateAnimator.cs` | Estados vacíos (listas sin datos) |
| `NavTransitionAnimator.cs` | Transiciones de navegación entre tabs |
| `ScoreRevealAnimator.cs` | Reveal dramático de puntuación final |
| `TabTransitionAnimator.cs` | Transiciones entre pestañas |

---

## E. USO DE ANIMACIONES EN FEATURES (no Animators)

Archivos de features que contienen código DOTween directamente:

| Archivo | Animaciones directas |
|---------|---------------------|
| `OnlineResultPanelController.cs` | Score reveal, win/loss animations |
| `SprintSummaryPanelController.cs` | Stats cascade, XP bar fill |
| `CashBattleResultPanelController.cs` | Gold win animation, earnings reveal |
| `MemoryPairsController.cs` | Card flip DOTween (Y rotation 90°→0°) |
| `FlashTapController.cs` | Target appear/disappear scale |
| `OddOneOutController.cs` | Wrong answer shake |
| `QuickMathController.cs` | Correct/wrong flash |
| `DigitRushController.cs` | Digit pop animations |
| `UISparkleEffect.cs` | Sparkle particle effect en UI |
| `TrophyCardUI.cs` | Shimmer effect en trophy cards |
| `PremiumPanelUI.cs` | Premium card reveal animation |
| `PremiumCard.cs` | Card hover + select animations |
| `AchievementNotificationManager.cs` | Toast slide in/out |
| `InAppNotificationManager.cs` | Notification banner animations |
| `CurrencyDisplayUI.cs` | Value change bump |
| `TrophyProgressPanel.cs` | Progress bar fill DOFillAmount |
| `LeaderboardEntryUI.cs` | Entry slide in escalonado |

---

## F. PATRONES Y CONVENCIONES

### F1. Patrón SetLink (obligatorio)
```csharp
// CORRECTO — auto-kill cuando el GameObject se destruye
DOTween.Sequence()
    .Append(transform.DOScale(1.2f, 0.3f))
    .SetLink(gameObject);

// CORRECTO — guardado en field + kill en OnDestroy
private Sequence _entranceSeq;
_entranceSeq = DOTween.Sequence()...;
// OnDestroy: _entranceSeq?.Kill();
```

### F2. Easing estándar del proyecto
| Contexto | Ease |
|----------|------|
| Entradas de pantalla | `OutBack` |
| Salidas de pantalla | `InBack` |
| Loops y pulsos | `InOutSine` |
| Transiciones suaves | `OutQuad` |
| Énfasis/rebote | `OutBounce` / `OutElastic` |
| Rotación continua | `Linear` |

### F3. Accesibilidad
- **Todos** los animadores consultan `AccessibilityHelper.AnimDuration(baseDuration)`
- Con reduced motion ON: duraciones reducidas o animaciones omitidas
- `AnimatedPanel` tiene `InstantShow/Hide` fallback

### F4. Loops infinitos
```csharp
// Siempre stored en field para poder matar en OnDestroy/OnDisable
private Tween _glowLoop;
_glowLoop = graphic.DOFade(0.2f, 1.5f)
    .SetLoops(-1, LoopType.Yoyo)
    .SetEase(Ease.InOutSine)
    .SetLink(gameObject);
```

### F5. SetUpdate (ignora TimeScale)
```csharp
// Para animaciones UI que deben correr aunque el juego esté pausado
tween.SetUpdate(true);
```

---

## G. ESTADÍSTICAS GLOBALES

| Categoría | Cantidad |
|-----------|---------|
| Archivos dedicados de animación | 31 |
| Sistemas core | 3 (Manager, Utils, Constants) |
| Animadores especializados por escena | 7 |
| Gestores de efectos | 7 |
| Componentes reutilizables | 12+ |
| Tipos de partículas | 9 |
| Métodos en UIAnimations.cs | 40+ |
| Easing types utilizados | ~8 |

---

---

## H. PANELES DE RESULTADO (Animadores dedicados en Features)

### H1. `WinCelebrationAnimator.cs`
**Ruta:** `Scripts/Runtime/Features/Games/Results/WinCelebrationAnimator.cs`
**Tipo:** MonoBehaviour creado dinámicamente en runtime (no prefab).

**Secuencia Win (4 fases):**

| Fase | Timing | Animación |
|------|--------|-----------|
| 1 | 0.0s | Flash blanco pantalla completa → DOFade out (0.4s, SMOOTH) |
| 1 | 0.0s | ScreenShake (15f, 0.25s) via UIAnimationManager |
| 2 | 0.15s | ResultIcon: scale 0 → 1.3 → 1 (OutBack, 0.3s+0.1s) |
| 3 | 0.65s | ConfettiRain coroutine: 3s, spawn cada 0.05s con aceleración |
| 4 | 0.65s | GoldFlash via UIAnimationManager |

**Confetti (por pieza):**
- Fall: DOAnchorPosY InQuad
- Wobble horizontal: DOAnchorPosX InOutSine Yoyo loop
- Rotate: FastBeyond360 random ±720°
- Fade out: Insert en t=0.7

**Secuencia Lose:**
- Flash rojo → DOFade out (0.5s SMOOTH)

**New High Score:**
- Texto "NEW HIGH SCORE!" pop scale 0.3→1.3→1 (OutBack)
- Shockwave ring: scale 0→12 + DOFade out (0.5s)
- DOPunchScale del texto (0.4s)
- ScreenShake (20f, 0.3s) + GoldFlash

**Cleanup:** TrackTween list + Kill en OnDestroy. Auto-destruye el GO a los 5s.

---

### H2. `WinPanelController.cs`
**Ruta:** `Scripts/Runtime/Features/Games/Results/WinPanelController.cs`

**Show/Hide:**
- Show: DOFade 0→1 (0.3s OutQuad) + DOScale 0.9→1 (0.35s OutBack), SetUpdate(true)
- Hide: DOFade 1→0 (0.2s InQuad) + DOScale 1→0.9 (0.2s InQuad)

**Reveal secuencial de stats (0.25s stagger por stat):**
- Cada stat: DOFade + DOScale OutBack (0.25s)
- Cada counter: DOTween.To() OutQuad (1.2s) → DOPunchScale al terminar (×6, 0.3s)
- Stats: tiempo, errores, dinero ganado (real money mode), wager

**Efectos victoria:**
- Llama `WinCelebrationAnimator.PlayWin()` con resultIcon
- Llama `VictoryEffectService.PlayEquippedEffect()` (custom cosmetic)
- Fallback: `UISparkleEffect.PlayVictoryConfetti()` + `PlayCoinExplosion()`

---

### H3. `ComboVisualController.cs`
**Ruta:** `Scripts/Runtime/Features/Games/Results/ComboVisualController.cs`
**Tipo:** Overlay runtime creado dinámicamente sobre el canvas del juego.

**5 tiers de combo:**

| Tier | Combo | Color | Efectos extra |
|------|-------|-------|--------------|
| 1 | x2-x3 | Verde | Combo punch |
| 2 | x4-x6 | Cyan | + border glow |
| 3 | x7-x9 | Amarillo | + "AMAZING!" + LightScreenShake |
| 4 | x10-x14 | Naranja | + "INCREDIBLE!" + pulse loop |
| 5 | x15+ | Oro | + "GODLIKE!" + GoldFlash |

**Animaciones:**
- Combo text punch: `DOPunchScale(punchScale, 0.25s)` por cada hit (escala con tier)
- Milestone text: pop 0.3→1.2→1 (OutBack) + hold 0.6s + DOFade out + DOAnchorPosY up
- Border glow (4 edges): DOColor fade in (0.3s); tier 4+ pulsa loop yoyo
- Combo break: flash rojo + DOShakePosition (10f, 0.3s) + border red→clear

**Milestone texts (localizados):**
- tier 2: `milestone_great`, tier 3: `milestone_amazing`, tier 4: `milestone_incredible`, tier 5: `milestone_godlike`

---

### H4. `LevelUpPanel.cs`
**Ruta:** `Scripts/Runtime/Features/Monetization/Progression/LevelUpPanel.cs`
**Tipo:** Panel modal con secuencia de 6 fases. Cola de multi level-up.

**Secuencia principal (timeline absoluto):**

| t= | Fase | Animación |
|----|------|-----------|
| 0.0s | Entry | Overlay DOFade 0→0.85 (0.35s Linear) |
| 0.0s | Entry | Panel DOScale 0.5→1 (0.45s OutBack/OutElastic si milestone) |
| 0.0s | Entry | Haptic en t=0.15s |
| 0.4s | Badge | "LEVEL UP!" label DOScale 0→1 (0.35s OutBack) |
| 0.5s | Milestone | milestoneLabel DOFade + DOScale si milestone level |
| 0.6s | Badge | BadgeRing DOScale 0→1 (0.5s OutElastic) |
| 0.7s | Número | OldLevel DOScale 1→0.3 + DOFade out + DOAnchorPosY +20 |
| 0.9s | Número | NewLevel DOFade + DOScale 0→1 (0.4s OutBack) |
| 1.0s | Número | DOPunchScale (0.25, 0.3s) + Haptic |
| 1.0s | XP | XP bar DOFade in |
| 1.1s | XP | DOFillAmount 0→1 (0.4s OutCubic) |
| 1.5s | XP | XP flash ×2 loops yoyo |
| 1.65s | XP | Reset fillAmount + DOFillAmount 0→progress (0.5s) |
| 1.8s | Reward | Card DOAnchorPosY slide in (0.45s OutBack) |
| 2.0s | Reward | "REWARD UNLOCKED" typewriter DOTween.To |
| 2.2s | Reward | RewardIcon DOScale 0→1 (0.35s OutBack) |
| 2.5s | CTA | Button DOFade + DOScale (0.3s OutBack) |
| 2.8s | CTA | Button pulse loop: DOScale 1.04x (0.75s InOutSine yoyo) |

**Loops contínuos:**
- Badge ring color cycle: CYAN_NEON → MAGENTA_NEON (1s loop yoyo)
- XP bar color: CYAN → MAGENTA blend (1s loop yoyo)

**Close:** DOScale 0.9 + DOFade (0.25s InBack)

---

### H5. `TournamentResultPanelController.cs`
**Ruta:** `Scripts/Runtime/Features/Tournaments/TournamentResultPanelController.cs`

**Stats reveal (DOTween pattern idéntico a WinPanelController):**
- Tiempo, errores, score: DOFade + DOScale OutBack (0.25s stagger) → DOTween.To counter → DOPunchScale
- Position text: DOScale 1.5→1 (0.4s OutBack)
- Prize counter: DOTween.To $0 → target (1.2s OutQuad) → DOPunchScale

**Show/Hide:** Coroutine Lerp manual (sin DOTween)

---

### H6. `OnlineResultPanelController.cs`
**Ruta:** `Scripts/Runtime/Features/Games/Results/OnlineResultPanelController.cs`

**Secuencia coroutine con DOTween interno:**
1. FadeIn (coroutine Lerp)
2. ShowPlayerInfo (player): DOFade + DOScale + DOTween.To counter (1.2s) → DOPunchScale
3. ShowPlayerInfo (opponent): ídem (0.3s después)
4. RevealResult: resultTitle DOScale 1.5→1 (0.3s OutBack) + DOFade subtítulo + resultIcon DOScale 0→1 + timeDiff counter + DOPunchScale en todos

---

### H7. `SprintSummaryPanelController.cs`
**Ruta:** `Scripts/Runtime/Features/Games/Results/SprintSummaryPanelController.cs`

**Game rows stagger:** Cada fila: DOFade + DOScale OutBack (i × 0.25s stagger)
**Totals reveal:** DOTween.To counters para total time, total errors, total score (tras todas las rows)
**VS section:** overallResultText DOScale 1.4→1 (0.4s OutBack)
**Money section (cash):** DOFade + DOScale + DOTween.To counter $0→prize → DOPunchScale

---

### H8. `CashBattleResultPanelController.cs`
**Ruta:** `Scripts/Runtime/Features/CashBattle/Results/CashBattleResultPanelController.cs`

**VS stats reveal:** playerTime, playerErrors, opponentTime, opponentErrors — DOFade + DOScale + DOTween.To counter (0.25s stagger)
**Money reveal:** DOFade + DOScale + DOTween.To $0→prize (1.2s) → DOPunchScale (delay 1.8s)
**EntryFee:** DOFade + DOScale (delay 2.0s)
**WinnerShare:** DOFade + DOScale (delay 2.2s)
**Efectos victoria:** UISparkleEffect.PlayVictoryConfetti() + PlayCoinExplosion()

---

## I. MANAGERS Y UI CON ANIMACIONES INTEGRADAS (50 archivos)

### I.1 Animaciones complejas (más de show/hide)

| Archivo | Animaciones destacadas |
|---------|----------------------|
| `DailyRewardsManager.cs` | **7 fases de claim** (gift shake, light burst, rewards reveal, pulse continuo). DOShakeRotation, DOPunchScale, DOFade, DOScale, rotación infinita loop |
| `ShopManager.cs` | Header entrance, stagger grid items, AnimatePanelIn/Out SetUpdate(true), DOPunchScale en compra, DOColor currency change |
| `PremiumPanelUI.cs` | blockerCG DOFade, panel AnimateIn con DOScale+DOFade SetUpdate(true) |
| `OnboardingManager.cs` | Staggered entrance de pasos, DOFade + DOScale por step |
| `AchievementsManager.cs` | Stagger de trophy cards, tab transitions via TrophyShowcaseAnimator |
| `TournamentLobbyManager.cs` | Participant cards stagger entrance DOFade + DOScale |
| `TournamentsBrowserManager.cs` | Tournament list stagger entrance |
| `CashTournamentsManager.cs` | Tournament items stagger |
| `InAppToastUI.cs` | Slide in/out desde borde (DOAnchorPosY/X), auto-dismiss con DOVirtual.DelayedCall |
| `AchievementToastUI.cs` | Slide in + icon pop (DOScale OutBack) + slide out |
| `PopupManager.cs` | Show/hide genérico DOScale+DOFade para todos los popups |
| `CashBattleOnboardingManager.cs` | Stagger de steps, CTA button pulse |

### I.2 Show/Hide estándar (DOFade + DOScale)

Estos archivos usan el patrón básico `DOFade + DOScale OutBack/OutQuad` para mostrar/ocultar paneles o elementos:

| Archivo | Patrón |
|---------|--------|
| `MainMenuManager.cs` | Panel fade in (0.4s OutQuad) + premiumBadge scale pop |
| `LoginManager.cs` | Panel show/hide + error shake |
| `RegisterManager.cs` | Panel show/hide + error shake |
| `AgeVerificationManager.cs` | Panel DOFade + DOScale |
| `ForgotPasswordPopup.cs` | DOFade + DOScale OutBack |
| `SettingsManager.cs` | Panel DOFade 0.35s OutQuad |
| `ProfileManager.cs` | Secciones DOFade staggered |
| `MatchHistorySceneManager.cs` | emptyText DOFade (0.4s OutQuad) |
| `NotificationsManager.cs` | NotificationItems DOFade stagger |
| `FriendsManager.cs` | FriendItems stagger DOFade + DOScale |
| `FriendRequestsSceneManager.cs` | Items DOFade + DOScale |
| `SearchPlayersManager.cs` | Results DOFade stagger |
| `DailyMissionsManager.cs` | Mission cards DOFade + DOScale stagger |
| `TournamentManager.cs` | Panel transitions DOFade |
| `TournamentCreateManager.cs` | Form steps DOFade |
| `LeaderboardManager.cs` | Entries DOFade stagger |
| `GameSelectorManager.cs` | Delegates a GameSelectorAnimator |
| `SceneNavigator.cs` | DOFade overlay 0→1→0 en transiciones |
| `ThemeSelector.cs` | Theme cards DOScale pop + DOFade |
| `FrameRenderer.cs` | Frame entrance DOFade + DOScale |
| `RotatingContentService.cs` | Content swap DOFade crossfade |
| `CashBattleManager.cs` | Panels DOFade + DOScale |
| `CashWalletSceneController.cs` | Content DOFade + DOScale |
| `CashHistorySceneController.cs` | History items DOFade stagger |
| `InputPanelUI.cs` | Panel DOFade + DOScale OutBack |
| `ErrorPanelUI.cs` | Panel DOFade + DOScale + error icon shake |
| `ConfirmPanelUI.cs` | Panel DOFade + DOScale |
| `ConfirmationPopup.cs` | DOFade + DOScale |
| `LogoutConfirmationPopup.cs` | DOFade + DOScale |
| `LeaderboardEntryUI.cs` | MedalIndicator DOScale OutBack (0.35s) |
| `ShopItemUI.cs` | Badges DOScale OutBack + wishlist DOPunchScale |
| `PremiumCard.cs` | DiscountBadge DOScale OutBack (0.35s) |
| `TrophyCardUI.cs` | Card shine sweep + hover DOScale |
| `PlayerSearchItemUI.cs` | Item DOFade + DOScale |
| `TournamentSearchItemUI.cs` | Item DOFade + DOScale |
| `ParticipantItemUI.cs` | Item DOFade stagger |
| `BattleCardApplier.cs` | Card reveal DOFade + DOScale |
| `BattleCardData.cs` | Preview DOFade |

---

## J. ESTADÍSTICAS FINALES COMPLETAS

| Categoría | Cantidad |
|-----------|---------|
| **Total archivos con animaciones** | **80** |
| Archivos dedicados `/Animations/` | 23 |
| Archivos dedicados `/Effects/` | 7 |
| Paneles de resultado (Features) | 8 |
| Managers/UI con animaciones complejas | 12 |
| Managers/UI con show/hide estándar | 30 |
| Tipos de partículas distintos | 9+ |
| Métodos estáticos en UIAnimations.cs | 40+ |
| Easing types utilizados | ~10 |
| **Ocurrencias DOTween aprox.** | **~500+** |
| Cobertura SetLink | ~95% |

---

## K. LO QUE NO EXISTE EN EL PROYECTO

- ❌ Unity Animator component
- ❌ AnimationClip (.anim files)
- ❌ Animator State Machines
- ❌ LeanTween
- ❌ iTween
- ❌ Manual `transform.Translate` coroutines para UI
- ❌ CSS-style transitions (no aplica a Unity)

Todo es **DOTween puro en código** + coroutines para efectos de partículas y fade manual donde no se puede usar DOTween.

---

*Fin del documento — DigitPark Animation System Audit V53b — Cobertura 100% (80/80 archivos)*
