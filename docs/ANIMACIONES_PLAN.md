# PLAN MAESTRO: Animaciones 3D y Profundidad - DigitPark

> **Fecha de analisis:** 2026-02-17
> **Estado:** Analisis completo, pendiente implementacion
> **Tecnologia de animacion:** DOTween (DG.Tweening)
> **Referencia resolution:** 1080x1920 (Portrait 9:16)

---

## INDICE

1. [Componentes 3D Existentes](#1-componentes-3d-existentes)
2. [Mapa de Profundidad por Escena](#2-mapa-de-profundidad-por-escena)
3. [Estado de ANIMATION_MANAGERS](#3-estado-de-animation_managers)
4. [Patrones DOTween Identificados](#4-patrones-dotween-identificados)
5. [UIBuilders y que 3D crean](#5-uibuilders-y-que-3d-crean)
6. [Analisis de DOTween por Manager](#6-analisis-de-dotween-por-manager)
7. [Plan de Implementacion](#7-plan-de-implementacion)
8. [Archivos Clave de Referencia](#8-archivos-clave-de-referencia)
9. [Sistema de Colores](#9-sistema-de-colores)
10. [Notas de Implementacion](#10-notas-de-implementacion)

---

## 1. COMPONENTES 3D EXISTENTES

La app tiene 4 componentes principales de profundidad 3D implementados con DOTween:

### Button3D
- **Archivo:** `Assets/_Project/Scripts/Animations/Components/Button3D.cs` (428 lineas)
- **Uso:** Botones genericos con efecto 3D
- **Press Depth:** 6px
- **Estructura:** Face + Shadow + HighlightLine
- **Jerarquia:**
  ```
  ButtonContainer
  +-- Shadow (Image, debajo del face)
  +-- Face (RectTransform, se mueve hacia abajo al presionar)
  |   +-- Highlight (Image, linea superior, fade al presionar)
  |   +-- Text (TextMeshProUGUI)
  +-- Glow (opcional, Outline pulsante)
  +-- PressParticles (opcional)
  +-- ConstantParticles (opcional)
  ```
- **Animaciones DOTween:**
  - Press: DOAnchorPosY (face baja), DOSizeDelta (shadow reduce), DOColor (face oscurece), DOFade (highlight desaparece), DOScale (0.92x)
  - Release: Todo inverso con Ease.OutBack (bounce)
  - Hover: DOScale(1.03x), DOFade glow
- **Config:** pressDuration=0.08s, releaseDuration=0.12s, hoverScale=1.03f

### Cell3DButton
- **Archivo:** `Assets/_Project/Scripts/UI/Cell3DButton.cs` (754 lineas)
- **Uso:** Celdas de grids de juegos (DigitRush, QuickMath, OddOneOut)
- **Press Depth:** 12px
- **Estructura:** ButtonFace + ShadowImage + SideImage + FaceImage
- **Animaciones:**
  - Press: Face baja 12px, Side reduce de 12f a 2f, colores oscurecen, glow reduce (0.06s)
  - Release: Inverso con Ease.OutBounce (0.15s)
  - Completed: Flash blanco -> green/gold, scale pulse segun combo, side a 2f
  - Error: Flash rojo, shake random 0.35s con magnitud decreciente
  - PopUp: Wave effect con delays random, bounce ease
- **Color States:** Normal(0.08,0.12,0.2), Pressed(0.04,0.06,0.1), Completed(0.1,0.3,0.15), Error(0.4,0.08,0.08), Combo Gold(1.0,0.85,0.2)

### Card3DEffect
- **Archivo:** `Assets/_Project/Scripts/UI/Card3DEffect.cs` (1006 lineas)
- **Uso:** Cards de MemoryPairs
- **Press Depth:** 10px
- **Estructura:** CardFace + ShadowImage + SideImage + FaceImage
- **Animaciones:**
  - Hover Enter: Sube 4px, escala 1.03x
  - Press: Baja pressDepth*0.6, reduce side
  - Flip Reveal: Flip completo con escala (ilusion de rotacion)
  - Flip Hide: Flip inverso
  - Match Success: Flash blanco, scale 1.2x, sube 8-30px segun combo, color transitions (green->gold), glow pulse continuo
  - Error Fail: Flash rojo intenso, shake fuerte
  - Victory: Jump + scale + rotation con glow

### GridGlowPulse
- **Archivo:** `Assets/_Project/Scripts/UI/GridGlowPulse.cs`
- **Requiere:** Outline component
- **Funcion:** Pulsa alpha del Outline entre minAlpha(0.4) y maxAlpha(1.0), distance entre 2.0 y 3.5
- **Speed:** 1.5f default
- **Metodo especial:** FlashGlow() para eventos

### ButtonEffects (Auto-aplicado)
- **Archivo:** `Assets/_Project/Scripts/Effects/ButtonEffects.cs`
- **Aplicacion:** BatchEffectsSetup lo aplica AUTOMATICAMENTE a todos los botones de todas las escenas
- **Deteccion por nombre:**
  - "play", "start", "jugar", "buy", "comprar", "login" -> Important (enhanced scale, 40% flash)
  - "confirm", "ok", "accept", "aceptar" -> Success (bounce, green flash)
  - "delete", "eliminar", "cancel", "cancelar" -> Danger (shake, red flash)
  - "premium", "pro" -> Premium (glow, gold flash)
  - Default -> Normal (scale, basic flash)

---

## 2. MAPA DE PROFUNDIDAD POR ESCENA

### ESCENAS CON 3D COMPLETO (Shadow + Side + Component)

| Escena | Elementos 3D | Componente | UIBuilder |
|--------|-------------|-----------|-----------|
| DigitRush | Celdas del grid | Cell3DButton | DigitRushUIBuilder.cs (L374-382) |
| QuickMath | Botones respuesta + panel ecuacion | Cell3DButton | QuickMathUIBuilder.cs (L327-336, L422-430) |
| OddOneOut | Celdas del grid | Cell3DButton | OddOneOutUIBuilder.cs (L387-395) |
| MemoryPairs | Cards del grid | Card3DEffect | MemoryPairsUIBuilder.cs (L393-401) |
| FlashTap | Boton Start | Button3D | FlashTapUIBuilder.cs (L705) |
| PlayModeSelection | 3 cards + back button | Shadow+Side manual | PlayModeSelectionUIBuilder.cs (L131-138, L274-281) |

### ESCENAS CON 3D PARCIAL

| Escena | Que tiene | Que falta |
|--------|----------|-----------|
| MainMenu | Outline glow + Shadow en PlayCard y CashBattleCard | Side, Button3D. Shadow es dark cyan 3px (PlayCard) y orange 4px (CashBattleCard) |
| CashBattleHub | Shadow (agregado 2026-02-17) + Outline dorado | Side (profundidad real) |
| Achievements | Shadow en trofeo (L1333 del builder) | Side en trophy cards |

### ESCENAS COMPLETAMENTE PLANAS (Sin ningun 3D)

| Escena | DOTween calls | Que animaciones tiene |
|--------|--------------|----------------------|
| **Shop** | **0** | **NADA - completamente estatica** |
| Settings | 2 | Solo fade panel + scale badge premium |
| TournamentCreate | 2 | Solo loading overlay fade in/out |
| TournamentLobby | 4 | Solo fade participantes |
| DailyRewards | 3 | Basico: fade+scale grid, ScalePunch claim, CoinFly |
| DailyMissions | 7 | Tabs DOColor/DOScale, fade cards, icon float loop |
| CashBattleOnboarding | ? | Sin ANIMATION_MANAGERS |
| BetSelection | ? | Sin ANIMATION_MANAGERS |
| Friends | 11 | Staggered cards, slide headers, fade loading. Sin 3D |
| FriendRequests | 12 | Tab colors, staggered items, badges. Sin 3D |
| Notifications | 10 | Staggered cards, tab indicators, slide sections. Sin 3D |
| MatchHistory | 7 | Staggered entries, slide header/filters. Sin 3D |
| Scores | 8 | Tab colors, staggered entries, panel in/out. Sin 3D |
| SearchPlayers | 3 | Staggered results, fade loading. Sin 3D |
| CashWallet | 11 | Tab sliding direccional, balance counter. Sin 3D |
| CashHistory | ? | Sin analizar en detalle |
| CashTournaments | ? | Sin analizar en detalle |
| CashBattle1v1 | ? | Sin analizar en detalle |

---

## 3. ESTADO DE ANIMATION_MANAGERS

### Estructura correcta esperada
```
---ANIMATION_MANAGERS---
  UIAnimationManager
    EffectsCanvas
      ScreenFlash
    TransitionCanvas
      FadeImage
      CircleWipeImage
      SlidePanel
  ParticleEffectSpawner
    Particles
```

### COMPLETOS (22 escenas) - No requieren cambios
Login, Register, MainMenu, GameSelector, DigitRush, FlashTap, MemoryPairs, OddOneOut, QuickMath, Matchmaking, Profile, Scores, SearchPlayers, CashHistory, CashWallet, CashBattleHub, Achievements, DailyMissions, DailyRewards, Shop, TournamentsBrowser, AgeVerification

### CON ERRORES (6 escenas)

| Escena | Problema | Accion requerida |
|--------|---------|-----------------|
| **TournamentLobby** | PrizesBlocker y LeaveBlocker estan DENTRO de TransitionCanvas. Incluyen PrizesPopup (Title, Prize_1erLugar, Prize_2doLugar con Place/Amount, Prize_3erLugar, CloseButton) y LeavePopup. Estos NO pertenecen a TransitionCanvas, interfieren con transiciones de escena | Mover PrizesBlocker y LeaveBlocker al Canvas principal, fuera de ANIMATION_MANAGERS |
| **Settings** | TransitionCanvas existe pero VACIO (sin hijos FadeImage/CircleWipeImage/SlidePanel) | Agregar los 3 hijos estandar |
| **Onboarding** | TransitionCanvas VACIO (sin hijos) | Agregar los 3 hijos estandar |
| **TournamentCreate** | TransitionCanvas VACIO (sin hijos) | Agregar los 3 hijos estandar |
| **CashTournaments** | Sin TransitionCanvas en ANIMATION_MANAGERS | Crear TransitionCanvas con hijos |
| **CashBattle1v1** | Sin TransitionCanvas en ANIMATION_MANAGERS | Crear TransitionCanvas con hijos |

### SIN ANIMATION_MANAGERS (6 escenas) - Necesitan estructura completa

| Escena | Tiene Manager script? | Notas |
|--------|----------------------|-------|
| BetSelection | Si (BetSelectionPanel) | Escena de seleccion de apuesta |
| CashBattleOnboarding | Si (CashBattleOnboardingManager) | Onboarding para cash battles |
| FriendRequests | Si (FriendRequestsManager) | Ya tiene 12 DOTween calls pero sin infrastructure |
| Friends | Si (FriendsManager) | Ya tiene 11 DOTween calls pero sin infrastructure |
| MatchHistory | Si (MatchHistorySceneManager) | Ya tiene 7 DOTween calls pero sin infrastructure |
| **Notifications** | **NO tiene Manager script** | Escena mas incompleta de toda la app |

---

## 4. PATRONES DOTWEEN IDENTIFICADOS

### Patrones consistentes (reutilizables)

#### Tab Switching (9 managers lo usan)
```csharp
image.DOColor(isActive ? activeColor : inactiveColor, 0.2f);
text.DOColor(isActive ? Color.white : inactiveTextColor, 0.2f);
button.transform.DOScale(isActive ? 1.05f : 1f, 0.2f).SetEase(Ease.OutCubic);
```

#### Staggered Entrance (6 managers)
```csharp
item.transform.DOScale(1f, 0.3f).SetDelay(index * 0.05f).SetEase(Ease.OutBack);
// Variante con fade:
cg.DOFade(1f, 0.3f).SetDelay(index * 0.05f);
```

#### Loading Overlay (todos los managers)
```csharp
cg.DOFade(1f, 0.2f).SetUpdate(true);  // Show
cg.DOFade(0f, 0.2f).SetUpdate(true).OnComplete(() => go.SetActive(false));  // Hide
```

#### Badge Pulse
```csharp
badge.DOScale(1f, 0.35f).SetEase(Ease.OutBack);   // Show
badge.DOScale(0f, 0.15f).SetEase(Ease.InBack).OnComplete(() => SetActive(false));  // Hide
```

#### AnimatePanelIn/Out (solo 2 managers lo tienen: CashBattleManager, LeaderboardManager)
```csharp
// In:
Sequence().Join(t.DOScale(1f, 0.3f).SetEase(Ease.OutBack))
          .Join(cg.DOFade(1f, 0.25f));
// Out:
Sequence().Join(t.DOScale(0.9f, 0.2f).SetEase(Ease.InQuad))
          .Join(cg.DOFade(0f, 0.2f)).OnComplete(callback);
```

#### AnimateEntrance (solo Friends, Notifications, MatchHistory)
```csharp
headerTransform.DOAnchorPos(originalPos, 0.4f).SetEase(Ease.OutBack);  // Slide from top +200
// Con sequence para sections:
DOTween.Sequence().AppendInterval(0.15f)
  .Append(sectionRT.DOAnchorPos(pos, 0.35f).SetEase(Ease.OutCubic))
  .Join(sectionCG.DOFade(1f, 0.35f));
```

### Patrones ausentes que deberian existir
1. **AnimatePanelIn/Out** - La mayoria de managers usa `SetActive()` directo sin transicion
2. **Balance counter animation** - Solo CashWallet lo tiene (`DOTween.To()`)
3. **Progress bar animation** - Solo Achievements lo tiene (`Slider.DOValue()`)
4. **Button press 3D** - Ningun manager lo implementa directamente (delegado a ButtonEffects)

### Duraciones estandar (UIAnimations.cs)
```csharp
DURATION_INSTANT = 0.1f;
DURATION_FAST = 0.2f;
DURATION_NORMAL = 0.3f;
DURATION_SLOW = 0.5f;
DURATION_VERY_SLOW = 0.8f;
```

---

## 5. UIBUILDERS Y QUE 3D CREAN

| UIBuilder | Archivo | Shadow | Side | Outline/Glow | Button3D | GridGlowPulse |
|-----------|---------|--------|------|-------------|----------|--------------|
| PlayModeSelectionUIBuilder | Editor/PlayModeSelectionUIBuilder.cs | Si (L131,274) | Si (L138,281) | Si | Cell3DButton | Si |
| MemoryPairsUIBuilder | Editor/MemoryPairsUIBuilder.cs | Si (L393) | Si (L401) | Si | Card3DEffect | Si |
| DigitRushUIBuilder | Editor/DigitRushUIBuilder.cs | Si (L374) | Si (L382) | Si | Cell3DButton | Si |
| QuickMathUIBuilder | Editor/QuickMathUIBuilder.cs | Si (L327,422) | Si (L336,430) | Si | Cell3DButton | Si |
| OddOneOutUIBuilder | Editor/OddOneOutUIBuilder.cs | Si (L387) | Si (L395) | Si | Cell3DButton | Si |
| FlashTapUIBuilder | Editor/FlashTapUIBuilder.cs | Si (L705) | No | Si (Outline) | Button3D | No |
| CashBattleUIBuilder | Editor/CashBattleUIBuilder.cs | Si (L630) | No | Si (Outline dorado) | No | No |
| AchievementsUIBuilder | Editor/AchievementsUIBuilder.cs | Si (L1333) | No | Si | No | Si |
| MainMenuUIBuilder | Editor/MainMenuUIBuilder.cs | Si (cyan 3px, gold 4px) | No | Si (CYAN_GLOW 4px, GOLD_GLOW 5px) | No | No |
| SettingsUIBuilder | Editor/SettingsUIBuilder.cs | No | No | Si (Outline sutil 25%) | No | No |
| ShopUIBuilder | Editor/ShopUIBuilder.cs | No | No | Si (Gold si popular) | No | No |
| DailyMissionsUIBuilder | Editor/DailyMissionsUIBuilder.cs | No | No | Si (Cyan/Purple/Green) | No | No |
| DailyRewardsUIBuilder (si existe) | Editor/DailyRewardsUIBuilder.cs? | No | No | ? | No | No |
| WalletUIBuilder | Editor/WalletUIBuilder.cs | No | No | ? | No | No |

### Utilidades de 3D
- **UIBuilderAnimationUtils** (`Editor/UIBuilderAnimationUtils.cs`): Tiene `AddButton3D()`, `CreateButton3DComplete()`, `CreateCTAButton()`, `AddGlowPulse()`, `CreateGlowBehind()`
- **AnimationSystemBuilder** (`Editor/AnimationSystemBuilder.cs`): Crea prefabs Button3D.prefab, TransitionCanvas.prefab, UIAnimationManager.prefab, ParticleEffectSpawner.prefab

---

## 6. ANALISIS DE DOTWEEN POR MANAGER

### Ranking por cantidad de animaciones DOTween

| # | Manager | DOTween Calls | Mejor caracteristica |
|---|---------|--------------|---------------------|
| 1 | AchievementsManager | 14 | Detail panel sequences, progress bar DOValue, celebration glow |
| 2 | FriendRequestsManager | 12 | Tab colors, staggered items con fade+scale |
| 3 | CashWalletSceneController | 11 | Tab sliding direccional, balance counter con DOTween.To |
| 4 | FriendsManager | 11 | Staggered cards, AnimateEntrance con slides |
| 5 | NotificationsManager | 10 | Staggered cards, tab indicators, 4-section entrance |
| 6 | LeaderboardManager (Scores) | 8 | UIAnimations.StaggeredEntrance, AnimatePanelIn/Out |
| 7 | TournamentsBrowserManager | 8 | Tabs, staggered tournament items |
| 8 | DailyMissionsManager | 7 | Tabs, fade cards, icon float loop infinito |
| 9 | MatchHistorySceneManager | 7 | Staggered entries, AnimateEntrance |
| 10 | MainMenuManager | 6 | DOFade panel, DOPunchScale notification badge |
| 11 | CashBattleManager | 6 | AnimatePanelIn/Out con Sequences (bien implementado) |
| 12 | TournamentLobbyManager | 4 | Solo fade participantes y loading |
| 13 | DailyRewardsManager | 3 | Basico fade+scale, usa ScalePunch y CoinFlyAnimation custom |
| 14 | SearchPlayersManager | 3 | Staggered results |
| 15 | TournamentCreateManager | 2 | Solo loading overlay |
| 16 | SettingsManager | 2 | Solo fade panel + badge |
| 17 | **ShopManager** | **0** | **NINGUNA ANIMACION** |

### Detalle de metodos custom usados
- `ScalePunch.Play()` - Usado en DailyRewards y DailyMissions para claim buttons
- `CoinFlyAnimation.Play()` - Animacion de moneda volando al currency display
- `UIAnimations.StaggeredEntrance()` - Clase estatica en `Animations/Core/UIAnimations.cs`
- `UIAnimations.TextPunch()` - DOPunchScale con 5 vibraciones
- `UIAnimations.ButtonPress()` - Scale 0.9f con OutBack

---

## 7. PLAN DE IMPLEMENTACION

### FASE 1: Reparar ANIMATION_MANAGERS rotos (12 escenas)

**Prioridad: CRITICA - Sin esto las escenas no pueden hacer transiciones**

#### 1A. Agregar ANIMATION_MANAGERS completo (6 escenas)
Escenas: BetSelection, CashBattleOnboarding, FriendRequests, Friends, MatchHistory, Notifications

Accion: Usar BatchEffectsSetup o script dedicado para crear:
- `---ANIMATION_MANAGERS---` (empty parent)
  - `UIAnimationManager` con componente UIAnimationManager
    - `EffectsCanvas` (Canvas, sortOrder alto)
      - `ScreenFlash` (Image fullscreen, alpha 0, raycast off)
    - `TransitionCanvas` (Canvas, sortOrder mas alto)
      - `FadeImage` (Image fullscreen, negro, alpha 0)
      - `CircleWipeImage` (Image con material CircleWipe)
      - `SlidePanel` (Image fullscreen para slide transition)
  - `ParticleEffectSpawner` con componente ParticleEffectSpawner
    - `Particles` (ParticleSystem)

#### 1B. Arreglar TransitionCanvas vacios (4 escenas)
Escenas: Settings, Onboarding, TournamentCreate

Accion: Agregar hijos FadeImage, CircleWipeImage, SlidePanel al TransitionCanvas existente

Escenas: CashTournaments, CashBattle1v1

Accion: Crear TransitionCanvas completo dentro del UIAnimationManager existente

#### 1C. Mover popups fuera de TransitionCanvas (1 escena)
Escena: TournamentLobby

Accion: Mover PrizesBlocker (con PrizesPopup y toda su jerarquia) y LeaveBlocker (con LeavePopup) del TransitionCanvas al Canvas principal de la escena

### FASE 2: Agregar Side (profundidad 3D) a cards que ya tienen Shadow

**Prioridad: ALTA - Impacto visual inmediato en pantallas principales**

#### 2A. MainMenu - Botones principales
Archivo: `Assets/_Project/Scripts/Editor/MainMenuUIBuilder.cs`

Elementos a modificar:
- PlayCard: Ya tiene Shadow(cyan 3px) + Outline(CYAN_GLOW 4px). **Agregar Side** entre Shadow y Face
- CashBattleCard: Ya tiene Shadow(orange 4px) + Outline(GOLD_GLOW 5px) + InnerGlow. **Agregar Side**
- Side color: version oscura del accent color (ej: cyan dark para PlayCard, gold dark para CashBattle)
- Side height: 8-10px (visible cuando no presionado)
- Opcional: Agregar Button3D component para press animation

#### 2B. CashBattleHub - 4 cards de navegacion
Archivo: `Assets/_Project/Scripts/Editor/CashBattleUIBuilder.cs`

Elementos a modificar:
- Battles1v1Card, CashTournamentsCard, WalletCard, HistoryCard
- Ya tienen Shadow (agregado hoy) + Outline dorado
- **Agregar Side** element entre Shadow y card background
- Side color: version oscura del CARD_BORDER gold
- Side height: 6-8px

### FASE 3: Agregar 3D completo a Shop (la escena mas vacia)

**Prioridad: ALTA - Es la pantalla de monetizacion principal, 0 animaciones = mal UX**

Archivo: `Assets/_Project/Scripts/Monetization/Shop/ShopManager.cs` y `ShopUIBuilder.cs`

Necesita TODO:
- Entrance animation del panel (fade + slide)
- Shadow + Side en product cards
- Button press 3D en price buttons
- Staggered entrance de items
- Currency display animation cuando cambia balance
- Purchase confirmation popup con AnimatePanelIn/Out
- Celebration animation al comprar (particulas, scale punch)

### FASE 4: DailyRewards y DailyMissions - 3D en cards

**Prioridad: MEDIA - Impacta retencion diaria**

#### 4A. DailyRewards
Archivo: `DailyRewardsManager.cs`

- Agregar Shadow + Side a day cards
- Mejorar claim button con Button3D
- Agregar progress bar animation (Slider.DOValue)
- Mejorar celebration: particulas + glow + screen flash
- Staggered entrance con delays mas visibles

#### 4B. DailyMissions
Archivo: `DailyMissionsManager.cs`

- Agregar Shadow sutil a mission cards
- Progress bar animation al actualizar
- Claim button con Button3D
- Reward popup con AnimatePanelIn

### FASE 5: Tournament scenes - 3D basico

**Prioridad: MEDIA**

#### 5A. TournamentsBrowser
- Shadow en tournament cards
- Join button con Button3D
- Staggered entrance mejorada

#### 5B. TournamentLobby
- Entrance animation principal (actualmente no tiene)
- Shadow en participant cards
- Countdown animation
- Status badge color transitions animadas

#### 5C. TournamentCreate
- Animaciones en preview panel
- Validation feedback visual
- Create button con Button3D

### FASE 6: Social scenes - Shadow sutil

**Prioridad: BAJA - Ya tienen buenas animaciones DOTween, solo falta profundidad visual**

Escenas: Friends, FriendRequests, Notifications, MatchHistory, Scores, SearchPlayers

- Agregar Shadow sutil (3-4px, baja opacidad) a PlayerCards/items
- No agregar Side (demasiado para listas scrolleables)
- Mejorar AnimateEntrance donde no exista
- Agregar AnimatePanelIn/Out donde use SetActive directo

### FASE 7: Escenas menores

**Prioridad: BAJA**

- Settings: Agregar AnimatePanelIn/Out a sub-panels (changeName, deleteConfirm, logoutConfirm)
- CashWallet: Agregar shadow sutil a deposit option buttons
- CashHistory: Verificar animaciones
- Onboarding: Agregar entrance animations
- CashBattleOnboarding: Agregar entrance animations

---

## 8. ARCHIVOS CLAVE DE REFERENCIA

### Core Animation Framework
```
Assets/_Project/Scripts/Animations/Core/UIAnimationManager.cs    -- Central manager, screen flash, currency anims
Assets/_Project/Scripts/Animations/Core/UIAnimations.cs          -- Static utility: ButtonPress, SlideIn/Out, PopupShow/Hide, StaggeredEntrance, etc.
Assets/_Project/Scripts/Animations/Components/Button3D.cs        -- 3D button component
Assets/_Project/Scripts/Animations/Components/SceneTransitionManager.cs  -- Scene transitions
```

### 3D Effect Components
```
Assets/_Project/Scripts/UI/Card3DEffect.cs        -- MemoryPairs cards
Assets/_Project/Scripts/UI/Cell3DButton.cs         -- Game grid cells
Assets/_Project/Scripts/UI/GridGlowPulse.cs        -- Pulsing outline glow
Assets/_Project/Scripts/UI/TapButtonEffect.cs      -- FlashTap special
Assets/_Project/Scripts/Effects/ButtonEffects.cs   -- Auto-applied to all buttons
```

### Editor/Build Tools
```
Assets/_Project/Scripts/Editor/BatchEffectsSetup.cs              -- Batch applies ButtonEffects + NeonGlow to scenes
Assets/_Project/Scripts/Editor/AnimationSystemBuilder.cs          -- Creates animation prefabs
Assets/_Project/Scripts/Editor/UIBuilderAnimationUtils.cs         -- Utility: AddButton3D, CreateButton3DComplete, CreateCTAButton, AddGlowPulse
```

### UIBuilders con 3D (referencia para copiar patrones)
```
Assets/_Project/Scripts/Editor/PlayModeSelectionUIBuilder.cs     -- MEJOR REFERENCIA para Shadow+Side en cards
Assets/_Project/Scripts/Editor/MemoryPairsUIBuilder.cs           -- Referencia Card3DEffect
Assets/_Project/Scripts/Editor/DigitRushUIBuilder.cs             -- Referencia Cell3DButton
Assets/_Project/Scripts/Editor/QuickMathUIBuilder.cs             -- Referencia Cell3DButton + panel shadow
Assets/_Project/Scripts/Editor/CashBattleUIBuilder.cs            -- Cards con Shadow (sin Side aun)
Assets/_Project/Scripts/Editor/MainMenuUIBuilder.cs              -- Cards con Outline+Shadow (sin Side aun)
```

### UIBuilders sin 3D (necesitan modificacion)
```
Assets/_Project/Scripts/Editor/ShopUIBuilder.cs
Assets/_Project/Scripts/Editor/SettingsUIBuilder.cs
Assets/_Project/Scripts/Editor/DailyMissionsUIBuilder.cs
Assets/_Project/Scripts/Editor/WalletUIBuilder.cs
```

### Managers ordenados por prioridad de mejora
```
Assets/_Project/Scripts/Monetization/Shop/ShopManager.cs                    -- PRIORIDAD 1: 0 animaciones
Assets/_Project/Scripts/Managers/Monetization/DailyRewardsManager.cs        -- PRIORIDAD 2: 3 animaciones basicas
Assets/_Project/Scripts/Managers/Tournaments/TournamentCreateManager.cs     -- PRIORIDAD 3: 2 animaciones
Assets/_Project/Scripts/Managers/SettingsManager.cs                         -- PRIORIDAD 4: 2 animaciones
Assets/_Project/Scripts/Managers/Tournaments/TournamentLobbyManager.cs      -- PRIORIDAD 5: 4 animaciones
Assets/_Project/Scripts/Managers/Monetization/DailyMissionsManager.cs       -- PRIORIDAD 6: 7 animaciones, falta 3D
```

---

## 9. SISTEMA DE COLORES

### Neon Theme (usado en toda la app)
```csharp
// Primarios
CYAN_NEON     = new Color(0f, 1f, 1f);              // Accion principal
CYAN_GLOW     = new Color(0f, 0.85f, 1f, 0.8f);     // Outline glow
CYAN_DARK     = new Color(0f, 0.4f, 0.5f);           // Shadow/bordes oscuros

// Premium/Cash
GOLD          = new Color(1f, 0.84f, 0f);             // Premium/importante
GOLD_DARK     = new Color(0.8f, 0.6f, 0.1f);          // Shadow gold
GOLD_GLOW     = new Color(1f, 0.75f, 0f, 0.7f);       // Glow dorado
CARD_BORDER   = new Color(0.85f, 0.65f, 0.13f, 0.7f); // Borde cards CashBattle

// Otros acentos
PURPLE_PREMIUM = new Color(0.6f, 0.3f, 1f);
GREEN_SUCCESS  = new Color(0.2f, 0.9f, 0.4f);
ORANGE_OFFER   = new Color(1f, 0.5f, 0f);
PINK_COSMETIC  = new Color(1f, 0.3f, 0.6f);

// Backgrounds
DARK_BG       = new Color(0.02f, 0.04f, 0.08f);       // Fondo pagina
CARD_BG       = new Color(0.06f, 0.08f, 0.12f);       // Fondo cards
HEADER_BG     = new Color(0.03f, 0.06f, 0.1f, 0.98f); // Fondo headers
```

### Colores de estado en componentes 3D
```csharp
// Cell3DButton / Card3DEffect
NORMAL    = new Color(0.08f, 0.12f, 0.2f);
PRESSED   = new Color(0.04f, 0.06f, 0.1f);
COMPLETED = new Color(0.1f, 0.3f, 0.15f);
ERROR     = new Color(0.4f, 0.08f, 0.08f);
COMBO     = new Color(1.0f, 0.85f, 0.2f);
```

---

## 10. NOTAS DE IMPLEMENTACION

### Como crear un Shadow + Side en un UIBuilder

Referencia: `PlayModeSelectionUIBuilder.cs` lineas 270-290

```csharp
// Shadow (detras de todo)
GameObject shadow = new GameObject("Shadow");
shadow.transform.SetParent(card.transform, false);
RectTransform shadowRT = shadow.AddComponent<RectTransform>();
shadowRT.anchorMin = Vector2.zero;
shadowRT.anchorMax = Vector2.one;
shadowRT.offsetMin = new Vector2(8, -12);   // Desplazado derecha y abajo
shadowRT.offsetMax = Vector2.zero;
Image shadowImg = shadow.AddComponent<Image>();
shadowImg.color = new Color(0f, 0f, 0f, 0.45f);

// Side (profundidad 3D - entre shadow y face)
GameObject side = new GameObject("Side");
side.transform.SetParent(card.transform, false);
RectTransform sideRT = side.AddComponent<RectTransform>();
sideRT.anchorMin = new Vector2(0, 0);
sideRT.anchorMax = new Vector2(1, 0);
sideRT.offsetMin = new Vector2(0, -sideHeight);  // sideHeight = 8-12px
sideRT.offsetMax = new Vector2(0, 0);
Image sideImg = side.AddComponent<Image>();
sideImg.color = sideColor;  // Version oscura del accent color
```

### Como agregar Button3D a un boton existente
```csharp
// Usar UIBuilderAnimationUtils:
UIBuilderAnimationUtils.AddButton3D(buttonGameObject);

// O crear completo:
UIBuilderAnimationUtils.CreateButton3DComplete(
    parent, "MyButton", "CLICK ME",
    new Vector2(300, 80),
    faceColor, shadowColor, textColor
);
```

### Convencion de nombres en jerarquia
```
CardName/
+-- Shadow        (Image, negro semi-transparente, offset abajo-derecha)
+-- Side           (Image, color oscuro del accent, borde inferior)
+-- Face/Background (Image principal del card)
+-- Icon           (Image del icono)
+-- Title          (TextMeshProUGUI)
+-- Subtitle       (TextMeshProUGUI)
```

### DOTween Init (ya configurado en UIAnimationManager.Awake)
```csharp
DOTween.Init(true, true, LogBehaviour.ErrorsOnly);
DOTween.defaultAutoPlay = AutoPlay.All;
DOTween.defaultUpdateType = UpdateType.Normal;
DOTween.defaultTimeScaleIndependent = false;
```

### BatchEffectsSetup - Escenas que procesa actualmente
```
Boot, Login, Register, MainMenu, Scores, Settings, Tournaments,
HowToPlay, CountrySelector, Profile, SearchPlayers, CashBattle,
Games/GameSelector, Games/DigitRush, Games/MemoryPairs,
Games/QuickMath, Games/FlashTap, Games/OddOneOut
```
**Escenas NO procesadas por BatchEffectsSetup (pueden faltar ButtonEffects):**
Friends, FriendRequests, Notifications, MatchHistory, DailyRewards,
DailyMissions, Achievements, Shop, CashWallet, CashHistory,
CashTournaments, CashBattle1v1, TournamentsBrowser, TournamentLobby,
TournamentCreate, BetSelection, CashBattleOnboarding, Onboarding,
AgeVerification, Matchmaking, PlayModeSelection

### Cambios realizados el 2026-02-17 (esta sesion)
1. **EditorBootConfig.cs** - Bypass ahora setea Mock_KYC_Status=3 (FullyVerified) y AgeVerified=1
2. **CashBattleManager.cs** - Eliminadas todas las referencias a AgeVerification (panel, fields, methods, KYC checks en card handlers). CheckKYCVerification simplificado.
3. **CashBattleHubReferenceAssigner.cs** - Eliminadas 5 refs de age verification
4. **CashBattleUIBuilder.cs** - Nuevo layout: 4 cards full-width verticales (23% cada uno). Shadow agregado. Balance widget agrandado (300x70, autosize 28-44). Icons 100x100. Eliminado CreateTouchIndicator (arrow ">").
5. **CashBattleOnboardingManager.cs** - Respeta bypass auth
6. **PlayModeSelectionUIBuilder.cs** - Cards ahora llenan pantalla (childControlHeight=true, childForceExpandHeight=true). Eliminada altura fija de 280px.
