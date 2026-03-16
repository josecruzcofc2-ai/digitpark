# DIGITPARK — THEME AUDIT MASTER
**Workflow**: Analizar escena → Documentar objetos → Marcar done → Siguiente escena
**Última actualización**: 2026-03-14
**Total escenas**: 40

---

## 🎨 PROMPT DE AUDITORÍA DE TEMA

> Copia este prompt completo, reemplaza `[SCENE_NAME]` y pégalo en el chat.
> Pon las screenshots de la escena en `C:\Users\josec\OneDrive\Pictures\Screenshots` antes de lanzarlo.
> Después de cada escena, borra las screenshots y pon las de la siguiente.

---

```
=== DIGITPARK — THEME AUDIT PROMPT (THEMEAPPLIER CHECKLIST) ===

You are performing a ThemeApplier audit for the Unity scene: [SCENE_NAME]
Project: DigitPark — competitive mobile mini-games app (iOS + Android)
Engine: Unity, C#, TextMeshPro, DOTween, ThemeApplier system (30 themes)
Goal: Identify EVERY UI object in this scene, decide YES/NO for ThemeApplier,
assign the exact ElementType, and document results in THEME_AUDIT_MASTER.md.

⚠️ TRIUMPH SDK EXCLUSION — MANDATORY:
Do NOT touch anything related to IKYCService, IWalletService,
IMatchmakingService, ITournamentService, TriumphServices.cs,
CashWalletSceneController.cs, PaymentManager.cs, StripeAbortProtocol.cs,
or any real-money / CashBattle payment flow. Skip those objects entirely.

📸 SCREENSHOTS PROVIDED — READ THEM FIRST:
The user has placed current screenshots of this scene in:
  C:\Users\josec\OneDrive\Pictures\Screenshots
These show the CURRENT VISUAL STATE — Editor view and/or Runtime.
Read ALL screenshots in that folder before starting your analysis.
Screenshots are replaced after each scene — this folder always contains
only the screenshots for the scene currently being audited.
Use them to confirm which objects are actually visible, what colors they
currently have, and whether any hardcoded colors conflict with theming.

🚫 ZERO SKIPPING POLICY — ABSOLUTE:
Every single UI object must be evaluated. No object may be grouped,
skipped, or summarized. Each one gets an individual explicit YES or NO
with its own reasoning. If uncertain, flag it — never omit silently.

────────────────────────────────────────────────────────────────
STEP 0 — READ EVERYTHING FIRST (do not analyze until all reads done)
────────────────────────────────────────────────────────────────
Read ALL of the following before making any decision:

A) Theme system (understand ElementTypes and color properties):
   - Assets/_Project/Scripts/Runtime/Themes/ThemeApplier.cs
   - Assets/_Project/Scripts/Runtime/Themes/ThemeData.cs
   - Assets/_Project/Scripts/Runtime/Themes/ThemeManager.cs

B) Scene UIBuilder(s) and AutoAssigner(s):
   Search Assets/_Project/Scripts/Editor/ for any Builder or Assigner
   file containing the scene name. Read them ALL — they define the full
   GameObject hierarchy and every UI element created.

C) Scene runtime Manager script(s):
   Search Assets/_Project/Scripts/Runtime/ for the Manager(s) for this
   scene. Read them to find runtime-created objects and any objects whose
   color changes dynamically.

D) Any Prefab builders or referenced prefabs used by this scene.

E) The two tracking files — read BOTH before starting:
   - docs/THEME_AUDIT_MASTER.md  ← the file you will WRITE to
     Read: the ElementType reference table at the top, and the existing
     notes pre-filled for this scene's section.
   - docs/TAREAS_MANUALES.md  ← manual task tracker
     If you find objects that need ThemeApplier added manually in the
     Unity Inspector (not via code), check this file first. If the task
     is not already listed, add it under BLOQUE 1 at the correct priority.

────────────────────────────────────────────────────────────────
STEP 1 — BUILD THE COMPLETE OBJECT INVENTORY
────────────────────────────────────────────────────────────────
From UIBuilders + Manager scripts + screenshots, list EVERY UI GameObject:
- Static objects in the scene hierarchy (from UIBuilder)
- Runtime-instantiated objects (from prefabs or Instantiate() calls)
- Objects inside prefabs added to this scene
- Objects that appear conditionally (panels, modals, toasts, popups)
- **Inactive/hidden objects** (`SetActive(false)` en UIBuilder o desactivados al cargar la escena) — listarlos por separado con sus colores hardcoded

For each object record:
- Full path (e.g. Canvas/Header/TitleText)
- Unity components: Image, TextMeshProUGUI, Button, Toggle, Slider,
  InputField, ScrollRect, Outline, Shadow, etc.
- Visual role: background, card, button label, icon, border, etc.
- Current color from UIBuilder code or screenshots

Do NOT make YES/NO decisions yet. Complete the inventory first.

────────────────────────────────────────────────────────────────
STEP 2 — DECISION: THEMEAPPLIER YES or NO?
────────────────────────────────────────────────────────────────
For EVERY object in the inventory, make an explicit YES or NO.

ALWAYS YES — must get ThemeApplier:
✅ Scene/panel backgrounds (Image with background role)
✅ Cards and elevated panels
✅ Buttons — Image component (background fill)
✅ Button text labels (child TextMeshProUGUI on buttons)
✅ All text: titles, labels, descriptions, hints, timestamps
✅ Input fields: background + border + placeholder text
✅ Tab bars and navigation elements
✅ Toggle: background + checkmark
✅ Slider: track + fill + handle
✅ Scrollbar: track + handle
✅ Borders, outlines, glow strip elements
✅ Leaderboard rank rows and rank badges
✅ Loading spinners / progress bars with themed color
✅ Header backgrounds
✅ Modal and overlay backgrounds
✅ Section dividers and decorative accent lines

ALWAYS NO — must NOT get ThemeApplier:
❌ App logo / brand mark / isotipo
❌ Achievement icons (have their own fixed art colors — multi-color art)
❌ Game thumbnail / illustration images
❌ Social login icons (Google, Apple logos — branded colors)
❌ Language flag images
❌ Player avatar / profile photo (the photo content itself)
❌ Particle systems and confetti (use confettiPalette separately)
❌ Triumph SDK elements (payment flows, wallet UI)
❌ Objects whose color is set exclusively by runtime game logic
   and must stay a fixed semantic color (e.g. live green dot)
❌ Score/timer number text that intentionally stays white always

🔴 ICON TINTING RULE — CRITICAL:
An icon/sprite may ONLY receive ThemeApplier if it is 100% PURE WHITE
(RGBA 255,255,255,255) with no other colors in the sprite art.
This is because ThemeApplier colorizes by multiplying the Image color —
if the sprite has any non-white pixels, the result will be a distorted
blend of the original art and the theme color.

CHECKLIST before marking any icon as YES:
  ✅ The sprite is a flat white silhouette / monochrome white glyph
  ✅ Every pixel is white (or transparent) — no greys, no gradients,
     no colored fills, no shading
  ✅ It was designed as a tintable icon (UI glyph style)

If the sprite has ANY colored pixels, ANY gradients, ANY shading, or
ANY multi-color art → it is automatically NO regardless of its role.

Examples:
  ✅ TINTABLE: white outline back-arrow icon, white gear icon,
               white bell icon, white search magnifier
  ❌ NOT TINTABLE: achievement medal with gold/silver art, game game
                   thumbnail with colors, Google "G" logo, flag icons,
                   any icon with a gradient or drop shadow baked in

BORDERLINE: default to YES, explain reasoning, flag as
"⚠️ BORDERLINE — needs designer review" if genuinely uncertain.

────────────────────────────────────────────────────────────────
STEP 3 — ASSIGN ELEMENTTYPE FOR EACH YES OBJECT
────────────────────────────────────────────────────────────────
Assign the correct ElementType from the 45 available types:

BACKGROUNDS:
  PrimaryBackground    → Main scene/screen background Image
  SecondaryBackground  → Panel, section, nested background
  TertiaryBackground   → Modal, dropdown, elevated surface
  CardBackground       → Content card specifically
  Overlay              → Semi-transparent dark blocker

BUTTONS (on Image/Button component):
  ButtonPrimary        → Main CTA button
  ButtonSecondary      → Alternative/secondary button
  ButtonDanger         → Destructive action (delete, logout)
  ButtonSuccess        → Positive confirm button

BUTTON GLOWS (on Outline/Shadow of buttons):
  ButtonGlowPrimary    → Glow on primary button
  ButtonGlowPremium    → Glow on premium/gold button
  ButtonGlowSuccess    → Glow on success button
  ButtonGlowDanger     → Glow on danger button
  ButtonGlowNavy       → Glow on navy button

TEXT (on TextMeshProUGUI):
  TextPrimary          → Main text, titles, important labels
  TextSecondary        → Subtitles, descriptions, hints, timestamps
  TextDisabled         → Text in disabled/inactive state
  TextTitle            → Large screen header/display title
  TextOnPrimary        → Text on top of ButtonPrimary
  TextOnDanger         → Text on top of ButtonDanger
  TextOnSuccess        → Text on top of ButtonSuccess

INPUTS:
  InputBackground      → InputField background Image
  InputBorder          → InputField border/outline Image
  InputPlaceholder     → Placeholder TextMeshProUGUI

ACCENTS (decorative elements, icons tint, dividers, highlights):
  Accent               → Primary accent (cyan in NeonDark)
  AccentSecondary      → Secondary accent (magenta in NeonDark)
  AccentTertiary       → Tertiary accent (gold details)
  Premium              → Premium badge, crown, PRO label
  Glow                 → Generic neon border/glow

NAVIGATION:
  TabActive            → Selected tab background/indicator
  TabInactive          → Unselected tab background/indicator

TOGGLES & SLIDERS:
  ToggleBackground     → Toggle switch background
  ToggleCheckmark      → Toggle checkmark/knob
  SliderTrack          → Slider background track
  SliderFill           → Slider filled portion
  SliderHandle         → Slider draggable handle

SCROLLBAR:
  ScrollbarTrack       → Scrollbar background rail
  ScrollbarHandle      → Scrollbar draggable thumb

STATUS:
  Error                → Error state indicator or banner
  Warning              → Warning/alert indicator
  Success              → Success/confirmation indicator
  Info                 → Informational badge or banner

LEADERBOARD:
  Rank1                → 1st place row/badge (gold)
  Rank2                → 2nd place row/badge (silver)
  Rank3                → 3rd place row/badge (bronze)
  RowEven              → Even-numbered list row background
  RowOdd               → Odd-numbered list row background

SCENE-SPECIFIC (OddOneOut and QuickMath only):
  HeaderPurple         → Purple header (OddOneOut only)
  HeaderNavy           → Navy header (QuickMath only)
  BackgroundNavy       → Navy background (QuickMath only)
  BackgroundPurple     → Purple background (OddOneOut only)

────────────────────────────────────────────────────────────────
STEP 4 — IDENTIFY DUAL THEMEAPPLIER OBJECTS
────────────────────────────────────────────────────────────────
Some GameObjects need TWO ThemeApplier components because they contain
two separately colorable elements. Find ALL such cases.

Common dual cases:
- Button (Image) + child Text:
    #1 on Button root: ButtonPrimary, applyToImage=true
    #2 on Text child: TextOnPrimary, applyToText=true
- Card (Image fill) + Outline component:
    #1: CardBackground, applyToImage=true
    #2: Glow, applyToOutline=true
- InputField root (background) + border child:
    #1: InputBackground, applyToImage=true
    #2 on border: InputBorder, applyToImage=true
- Header bar (background) + accent bottom line:
    #1: SecondaryBackground, applyToImage=true
    #2 on accent strip: Accent, applyToImage=true

For each dual case, specify both ThemeApplier configs completely.

────────────────────────────────────────────────────────────────
STEP 5 — SCREENSHOT CROSS-CHECK
────────────────────────────────────────────────────────────────
Verify your decisions against the provided screenshots:
- Confirm every YES object is actually visible in the scene
- Flag any visible element in screenshots not found in Step 1
- Note any object whose current hardcoded color would CONFLICT with
  ThemeApplier (it must be reset to white/clear before ThemeApplier works)
- Note Editor vs Runtime visual differences that affect theming
  (e.g. an object visible in Editor but missing at Runtime)

────────────────────────────────────────────────────────────────
STEP 6 — WRITE RESULTS TO THEME_AUDIT_MASTER.md
────────────────────────────────────────────────────────────────
Open docs/THEME_AUDIT_MASTER.md and fill in the section for [SCENE_NAME]:

TABLE 1 "Objetos a TINTAR":
| Objeto (path) | Componente | ElementType | ThemeData Property | Color NeonDark | Notas |
- ThemeData Property: exact field (e.g. primaryAccent, buttonPrimary)
- Color NeonDark: hex value for NeonDark theme
- Notas: e.g. "reset Image color to white first", "added via code not Inspector"

TABLE 2 "Objetos que NO se tintan":
| Objeto (path) | Razón |
- One clear sentence per object

TABLE 3 "Casos especiales (2 ThemeAppliers)":
| Objeto | ThemeApplier #1 | ThemeApplier #2 | Razón |

TABLE 4 "Objetos ocultos (inactivos)":
| Objeto (path) | Componente | Color hardcoded | ¿Necesita ThemeApplier? | Notas |
- Listar TODOS los objetos con `SetActive(false)` al inicio o inactivos al cargar la escena
- Confirmar si necesitan ThemeApplier (sí lo necesitan si se activan en algún momento)
- Anotar sus colores hardcoded — mismos criterios que objetos activos

Update summary table at top of THEME_AUDIT_MASTER.md:
- Status: ⬜ → 📝
- Fill: Objetos totales / A tintar / No tintar

────────────────────────────────────────────────────────────────
OUTPUT FORMAT IN CHAT — MANDATORY
────────────────────────────────────────────────────────────────
## [SCENE_NAME] — Theme Audit Summary

### Stats
- Total UI objects evaluated: X
- Will get ThemeApplier: X
- Will NOT get ThemeApplier: X
- Dual ThemeApplier cases: X

### Objects TO TINT
| Path | Component | ElementType | NeonDark color |
|---|---|---|---|
(every YES object — no omissions)

### Objects NOT to tint
| Path | Reason |
|---|---|
(every NO object)

### Dual ThemeApplier cases
| Path | ThemeApplier #1 | ThemeApplier #2 |
|---|---|---|

### Objetos ocultos (inactivos en escena)
| Path | Componente | Color hardcoded | ¿Necesita ThemeApplier? |
|---|---|---|---|

### ⚠️ Flags
- Objects needing color reset to white before ThemeApplier
- Objects whose screenshots show conflicting hardcoded colors
- Borderline decisions needing designer review
- Objects created at runtime needing ThemeApplier added via code

### Confirmation
"Updated THEME_AUDIT_MASTER.md — section [SCENE_NAME] complete."
```

---

## 📚 REFERENCIA RÁPIDA — SISTEMA DE TEMAS

### ElementTypes disponibles en ThemeApplier (45 tipos)
| ElementType | ThemeData Property | Uso típico |
|---|---|---|
| `PrimaryBackground` | `primaryBackground` | Fondo principal de escena |
| `SecondaryBackground` | `secondaryBackground` | Paneles, cards secundarios |
| `TertiaryBackground` | `tertiaryBackground` | Elementos elevados, modales |
| `CardBackground` | `cardBackground` | Cards de contenido |
| `Overlay` | `overlayColor` | Bloqueadores semitransparentes |
| `ButtonPrimary` | `buttonPrimary` | Botón de acción principal |
| `ButtonSecondary` | `buttonSecondary` | Botón secundario |
| `ButtonDanger` | `buttonDanger` | Botón destructivo (rojo) |
| `ButtonSuccess` | `buttonSuccess` | Botón de éxito (verde) |
| `ButtonGlowPrimary` | `glowColor` | Glow de botón primario |
| `ButtonGlowPremium` | `premiumColor` | Glow de botón premium |
| `ButtonGlowSuccess` | `successColor` | Glow de botón success |
| `ButtonGlowDanger` | `errorColor` | Glow de botón danger |
| `ButtonGlowNavy` | `headerNavy` | Glow específico navy |
| `TextPrimary` | `textPrimary` | Texto principal (blanco) |
| `TextSecondary` | `textSecondary` | Subtítulos, hints |
| `TextDisabled` | `textDisabled` | Estado deshabilitado |
| `TextTitle` | `textTitle` | Títulos especiales |
| `TextOnPrimary` | `textOnPrimary` | Texto sobre botón primario |
| `TextOnDanger` | `textOnDanger` | Texto sobre botón danger |
| `TextOnSuccess` | `textOnSuccess` | Texto sobre botón success |
| `InputBackground` | `inputBackground` | Fondo de input field |
| `InputBorder` | `inputBorder` | Borde de input |
| `InputPlaceholder` | `inputPlaceholder` | Placeholder text |
| `Accent` | `primaryAccent` | Acento principal (cyan en NeonDark) |
| `AccentSecondary` | `secondaryAccent` | Acento secundario (magenta en NeonDark) |
| `AccentTertiary` | `tertiaryAccent` | Acento terciario (gold) |
| `Premium` | `premiumColor` | Elementos premium (gold) |
| `Glow` | `glowColor` | Bordes/glows genéricos |
| `TabActive` | `tabActive` | Tab seleccionado |
| `TabInactive` | `tabInactive` | Tab no seleccionado |
| `ToggleBackground` | `toggleOn` / `toggleOff` | Fondo de toggle |
| `ToggleCheckmark` | `toggleCheckmark` | Check del toggle |
| `SliderTrack` | `sliderTrack` | Pista del slider |
| `SliderFill` | `sliderFill` | Relleno del slider |
| `SliderHandle` | `sliderHandle` | Handle del slider |
| `ScrollbarTrack` | `scrollbarTrack` | Pista del scrollbar |
| `ScrollbarHandle` | `scrollbarHandle` | Handle del scrollbar |
| `Error` | `errorColor` | Estados de error |
| `Warning` | `warningColor` | Estados de aviso |
| `Success` | `successColor` | Estados de éxito |
| `Info` | `infoColor` | Estados informativos |
| `Rank1` | `rank1Color` | 1er lugar (gold) |
| `Rank2` | `rank2Color` | 2do lugar (silver) |
| `Rank3` | `rank3Color` | 3er lugar (bronze) |
| `RowEven` | `rowEven` | Fila par en lista |
| `RowOdd` | `rowOdd` | Fila impar en lista |
| `HeaderPurple` | `headerPurple` | Header púrpura (OddOneOut) |
| `HeaderNavy` | `headerNavy` | Header navy (QuickMath) |
| `BackgroundNavy` | `backgroundNavy` | Fondo navy |
| `BackgroundPurple` | `backgroundPurple` | Fondo púrpura |

### Regla de 2 ThemeAppliers por objeto
Un objeto puede necesitar **dos** ThemeApplier en casos como:
- Botón: uno para `ButtonPrimary` (Image) + uno para `TextOnPrimary` (Text)
- Card con borde: uno para `CardBackground` (Image fill) + uno para `Glow` (Outline)
- Input: uno para `InputBackground` + uno para `InputBorder`

### Objetos que NUNCA se tintan
- Logos y branding (DigitPark logo, isotipo)
- Iconos de achievement (tienen colores propios fijos)
- Iconos de redes sociales (Google, Apple)
- Banderas de idioma
- Avatares / fotos de perfil
- Imágenes ilustrativas / arte del juego
- Elementos de color semántico fijo: `errorColor` (rojo), `successColor` (verde)
- Indicadores de estado online/offline con colores estándar
- Partículas y confetti (usan `confettiPalette` especial)
- **CurrencyPills** — todos los objetos gestionados por `CurrencyHeaderBarHelper` son estáticos: `CoinsPill`, `GemsPill`, `CoinsAddButton`, `GemsAddButton`, `CoinsIcon`, `GemsIcon`, `CoinsValueText`, `GemsValueText`. NUNCA reciben ThemeApplier en ninguna escena. Ver nota de temas verdes abajo.
- **BackgroundPattern** — la capa cosmética de patrón usa siempre `white @ opacidad fija`. NUNCA recibe ThemeApplier (ver sección Backgrounds más abajo).

---

### ⚠️ PENDIENTE — BackButton sprites: regenerar en blanco + dorado

**Estado**: Prompts generados, imágenes pendientes de regenerar (versión correcta con flecha izquierda)

**Archivos destino**:
- `Assets/_Project/Art/Icons/Navigation/BackIcon.png` → reemplazar con versión blanca (para ThemeApplier)
- `Assets/_Project/Art/Icons/Navigation/BackIconGold.png` → reemplazar con versión dorada mejorada

**Una vez integrados los PNGs**:
1. `BackButton.prefab` → verificar que ThemeApplier(root) llega al Icon child Image; si no → mover ThemeApplier al GO `Icon` con ElementType `Accent`
2. `BackButtonGold.prefab` → asignar `BackIconGold.png` al campo `m_Sprite` del Icon Image (actualmente `fileID: 0` = null); poner `Image.color = white` (el color está baked en el PNG)

---

### ⚠️ NOTA — BackButton sprite debe ser WHITE GLYPH PURO

El sprite de la flecha en `BackButton.prefab` (hijo `Arrow` o `Icon`) **debe ser un PNG blanco puro (RGBA 255,255,255,255)** para que `ThemeApplier(Accent)` lo tiña correctamente al color de acento del tema activo.

**Razón técnica**: Unity multiplica el `Image.color` por los píxeles del sprite:
```
Sprite blanco (#FFFFFF) × ThemeApplier(Accent=#FF1493 magenta) = #FF1493 ✅ correcto
Sprite cyan   (#00FFFF) × ThemeApplier(Accent=#FF1493 magenta) = #000000 ❌ negro
```

Si el PNG actual de la flecha tiene píxeles cyan baked-in (el builder lo setea `color = CyanNeon`), hay que regenerarlo como blanco puro. El builder puede seguir seteando el `Image.color` a `CyanNeon` como valor por defecto, pero el sprite en sí debe ser blanco.

**Acción requerida**: Verificar `Assets/_Project/Art/Icons/UI/back_arrow.png` (o el nombre real del sprite). Si tiene píxeles de color propios → regenerar como blanco puro en la misma resolución y reemplazar el archivo.

**BackButtonGold**: usa `BackIconGold.png` — arte dorado intencional, NO es white glyph, NO recibe ThemeApplier(Accent). Se queda igual.

---

### ⚠️ NOTA — Temas verdes y CurrencyPills (choque estético)

Los colores de las CurrencyPills son **fijos** (estáticos, no tematizables):
- `CoinsPill`: fondo dorado/ámbar + icono moneda dorada
- `GemsPill`: fondo azul/cian + icono gema azul-violeta

Los siguientes temas tienen `primaryBackground` verde que crea **tensión visual** con las pills (no rompe legibilidad pero choca estéticamente):

| Tema | primaryBackground | Problema |
|------|------------------|---------|
| **Toxic Lime** | `#0C1A08` | GemsPill azul sobre fondo verde lima — máximo choque de hue |
| **Matrix** | `#080F08` | Ídem, más oscuro, menos visible |
| **Emerald** | `#0A1A14` | GemsPill teal vs fondo emerald teal — bajo contraste de pill |
| **Aurora Borealis** | `#040E0A` | Similar a Emerald |

**Decisión de diseño**: aceptado como tensión inherente al sistema. La alternativa (tematizar las pills) rompería la señal universal "moneda = dorado/azul" que los usuarios aprenden en las primeras sesiones. Las pills tienen su propio fondo translúcido que aísla el contenido — los valores numéricos siempre son legibles.

---

## 🖼 SISTEMA DE BACKGROUNDS (2 capas)

El fondo visual de cada escena es el resultado de **dos capas independientes**:

```
VISUAL FINAL = 1 color (tema activo)  +  1 patrón (cosmético del usuario)
```

### Capa 1 — Color del tema
| GO | Componente | ElementType | Quién lo controla |
|---|---|---|---|
| `Canvas/Background` | Image | `PrimaryBackground` | ThemeApplier (cambia con cada tema) |

- Esta capa es **obligatoria** en **todas las 28 escenas temáticas** (excl. CashBattle + excluidas).
- El color proviene de `ThemeData.primaryBackground`.
- **SIEMPRE recibe ThemeApplier** — no importa el estado de la escena.

### Capa 2 — Patrón cosmético
| GO | Componente | Configuración | Quién lo controla |
|---|---|---|---|
| `Canvas/BackgroundPattern` | Image | `color = new Color(1,1,1, opacidad_patron)` fija | Elección cosmética del usuario |

- Esta capa es **opcional** (el usuario la compra en el Shop).
- El sprite es un PNG blanco-sobre-negro (`bg_dots`, `bg_circuit`, `bg_hexgrid`, etc.).
- **NUNCA recibe ThemeApplier** — el color es siempre `white @ opacidad fija`.
- El sprite activo se guarda en `PlayerPrefs("active_background")`, separado del tema.
- Se aplica en las mismas 28 escenas temáticas que Capa 1.
- Ver diseño completo: `docs/BACKGROUNDS_COSMETIC_DESIGN.md`

### Jerarquía en escena
```
Canvas
  ├── Background         (Image — PrimaryBackground — ThemeApplier ✅)
  ├── BackgroundPattern  (Image — sprite PNG blanco — SIN ThemeApplier ❌)
  └── SafeArea
        └── ...UI...
```

### Escenas excluidas del sistema de backgrounds
Las mismas excluidas del ThemeApplier (ver tabla de estado abajo):
- CashBattle (#08, #32–40): paleta gold estática
- AgeVerification (#06): flujo KYC/legal
- Onboarding (#07): se muestra antes de que el usuario pueda comprar cosméticos

### Temas Chromatic — qué cambia vs temas Standard

Los temas "Chromatic" (2 colores) son una variante premium de ciertos temas que añade un campo extra en `ThemeData`:
```csharp
public bool isChromatic = false;
public Color patternTintColor = Color.white;
```

**Impacto en el ThemeApplier**: NINGUNO. Todos los botones, textos, cards, iconos, inputs, etc. usan exactamente los mismos ElementTypes en temas Chromatic que en temas Standard. El audit no cambia nada para ellos.

**El único elemento que cambia** es la Capa 2 (BackgroundPattern):

| Tipo de tema | BackgroundPattern tint |
|-------------|----------------------|
| **Standard** | `white @ opacidad fija` — patrón sutil sin color |
| **Chromatic** | `theme.patternTintColor @ opacidad fija` — patrón en el color de acento del tema |

`BackgroundPatternReceiver` lee `ThemeManager.Instance.CurrentTheme.isChromatic` en cada `Apply()` para decidir el tint. Esto ocurre en `BackgroundPatternReceiver.cs`, no en ThemeApplier.

**Resumen por elemento**:
| Elemento | Standard | Chromatic | ¿Diferente? |
|----------|---------|-----------|------------|
| Background (Capa 1) | `primaryBackground` via ThemeApplier | mismo | ❌ igual |
| BackgroundPattern (Capa 2) | `white @ opacity` | `patternTintColor @ opacity` | ✅ diferente |
| Botones, textos, cards | ElementTypes normales | mismo | ❌ igual |
| BackButton arrow icon | `primaryAccent` via ThemeApplier | mismo | ❌ igual |
| CurrencyPills | estáticos | estáticos | ❌ igual |

Ver implementación completa: `docs/BACKGROUNDS_IMPLEMENTATION_PLAN.md` → FASE 5b.

---

## 📊 ESTADO DE AUDITORÍAS DE TEMA

**Leyenda**: ⬜ Pendiente · 🔍 Analizando · 📝 Documentado · ✅ Implementado

| # | Escena | Estado | Objetos totales | A tintar | No tintar | Notas |
|---|--------|--------|----------------|----------|-----------|-------|
| 01 | `_Core/Boot.unity` | 📝 | 11 | 6 | 5 | Sin duales · BootAnimator ya maneja glow/fill |
| 02 | `_Core/MainMenu.unity` | 📝 | 75 | 56 | 19 | 10 duales · PlaySide/CashSide sin ElementType · AchievementsCard orange vs AccentSecondary gap |
| 03 | `_Core/Settings.unity` | 📝 | 144 | 118 | 26 | 8 duales · Separadores sin ElementType dedicado (→TertiaryBackground) · BackButton es prefab compartido |
| 04 | `Auth/Login.unity` | 📝 | 68 | 52 | 16 | 8 duales · GoogleButton/AppleButton bg=brand mandated · 2 popups runtime (UsernamePopup + ForgotPasswordPopup) |
| 05 | `Auth/Register.unity` | 📝 | 26 | 24 | 2 | 5 duales · Logo=branding NO · ErrorPanel=prefab separado · 2 EyeToggles BORDERLINE |
| 06 | `Auth/AgeVerification.unity` | 🚫 Excluida | — | — | — | Flujo KYC/legal de CashBattle — debe mantener presentación neutral, sin variación de tema |
| 07 | `Onboarding/Onboarding.unity` | 🚫 Excluida | — | — | — | Se muestra una sola vez antes de que el usuario pueda comprar temas — no tiene sentido tematizar |
| 08 | `Onboarding/CashBattleOnboarding.unity` | 🚫 | — | — | — | Zona CashBattle — paleta gold estática, sin ThemeApplier |
| 09 | `Games/Navigation/GameSelector.unity` | 📝 | 57 | 38 | 19 | 8 duales · Game cards NO se tiñen (arte ilustrado + UIBuilder elimina ThemeApplier) · CurrencyPills estáticos · RulesPanel pendiente→reemplazar por InfoButton · SelectedCountText runtime semántico (verde/blanco) |
| 10 | `Games/Navigation/PlayModeSelection.unity` | 📝 | 44 | 21 | 23 | 4 duales · Iconos de modo BORDERLINE (UIBuilder L298 comenta "WHITE color for ThemeApplier tinting" — verificar sprite) · Shadow de cards=efecto decorativo fijo NO · CurrencyPills estáticos |
| 11 | `Games/Navigation/BetSelection.unity` | 📝 | 115 | 77 | 38 | 16 duales · HighlightCard() sobreescribe Image+Outline de tarjetas en selección · SelectRounds() sobreescribe Rounds buttons · SetToggleVisual() sobreescribe CustomCoinsToggle · Coins=AccentGold · Gems=AccentSecondary · Custom=PrimaryAccent · CurrencyPills=estáticos |
| 12 | `Games/Navigation/Matchmaking.unity` | 🚫 Parcial | — | — | — | BattleCardApplier (cosmético) controla 6 elementos por card en runtime → conflicto con ThemeApplier · CountdownText verde semántico hardcoded · EXCEPCIÓN: `Canvas/Background` → ThemeApplier `PrimaryBackground` (obligatorio como todas las escenas) |
| 13 | `Games/Minigames/DigitRush.unity` | 📝 | 62 | 41 | 21 | 10 duales · Cell3DButton/ToggleVisual necesitan leer ThemeManager en runtime · 3 iconos stats BORDERLINE (verificar sprite blanco) |
| 14 | `Games/Minigames/FlashTap.unity` | 📝 | 37 | 27 | 10 | 7 duales · TapButton3D=arte del juego NO · UpdateToggleVisual sobreescribe runtime igual que DigitRush · ReactionTimeText colores semánticos de rendimiento |
| 15 | `Games/Minigames/MemoryPairs.unity` | 📝 | 68 | 34 | 34 | 7 duales · Card3DEffect EnforceColorsNextFrame bloquea ThemeApplier en cartas (intencional) · UpdateToggleVisual hardcoded (refactor pendiente) · ComboText/FeedbackText semánticos |
| 16 | `Games/Minigames/OddOneOut.unity` | 📝 | 204 | 96 | 108 | 14 duales (35 instancias) · OddOneOutCell3D sobreescribe colores de celdas igual que Cell3DButton · UpdateToggleVisual() bug sistémico · 3 iconos BORDERLINE · BackgroundPurple/AccentSecondary scene-specific |
| 17 | `Games/Minigames/QuickMath.unity` | 📝 | 87 | 59 | 28 | 19 duales (25 instancias) · 10 toggles con UpdateToggleVisual() bug · QuickMathCell3D sobreescribe botones · ComboContainer sin dual (Outline semántico naranja) · BackgroundNavy/HeaderNavy scene-specific |
| 18 | `Social/Profile/Profile.unity` | 📝 V52 | 129 | 69 | 60 | 8 duales · StatusText runtime-overridden (cyan/green/yellow/grey según friend status) → NO · BorderRing gestionado por FrameRenderer cosmético → NO · AccentBar/BarFill/Value de game rows (colores semánticos por juego) → NO · 5 stat values BORDERLINE mapeados a Accent/Success/AccentTertiary/Warning/AccentSecondary · AddFriendButton/Icon color runtime → NO · ChangeNamePanel+GameSelectionPanel: ThemeApplier via code |
| 19 | `Social/Profile/Scores.unity` | 📝 V52 | 101 | 18 | 83 | 3 duales · GameButton bg+Outline+Icon+Label (×5) runtime-overridden por `UpdateGameSelectorVisuals()` → NO · Tabs Image+Text DOColor override por `SetTabButtonState()` → NO · SampleEntries (×5 ×7 items) Editor-preview → NO · LeaderboardEntry prefab completo runtime-overridden por `LeaderboardEntryUI.Setup()` → NO · PositionNumber runtime-override (gold/grey) · PositionTime semántico verde · mejora futura: LeaderboardEntryUI usar ThemeData.GetRankColor() |
| 20 | `Social/Profile/MatchHistory.unity` | 📝 V52 | 43 | 12 | 31 | 2 duales · Filter chips runtime-overridden por `UpdateFilterButtonVisual()` (colores semánticos por juego) → NO · CurrencyPills estáticos → NO · MatchHistoryEntry prefab: 6 ThemeAppliers via `CreateMatchEntryPrefab()` · ColorBar/ResultBadge semánticos → NO · TotalCountText ausente en UIBuilder (⚠️ falta) |
| 21 | `Social/Friends/Friends.unity` | 📝 V52 | 28 | 15 | 6 duals | FriendCard prefab runtime-instanciado (ThemeApplier añadir vía código) · StatusText/OnlineIndicator runtime-overridden → NO · RequestsBadge rojo semántico → NO · FrameRenderer en AvatarFrame → NO · SearchBar dual InputBackground+InputBorder · RequestsNav dual CardBackground+AccentSecondary (purple) |
| 22 | `Social/Friends/FriendRequests.unity` | 📝 V52 | 27 | 10 | 6 duals | Tabs runtime-overridden por SwitchTab() (hardcodea ACTIVE_TAB/INACTIVE_TAB) → ThemeApplier + fix Manager necesario · AvatarFrame gestionado por FrameRenderer cosmético → NO · AcceptButton(Success) + RejectButton(Danger) + CancelButton(Secondary) pattern semántico · 3 ocultos (EmptyText, Loading, CancelButton) |
| 23 | `Social/Friends/SearchPlayers.unity` | 📝 V52 | 28 | 19 | 4 duals | PlayerCard prefab runtime-instanciado · SearchIcon/EmptyIcon BORDERLINE (⚠️ verificar sprite blanco) · ClearButton hidden al inicio (shown on type) · OnlineStatus/OnlineLabel runtime-overridden → NO · SearchButton vestigial hidden → NO |
| 24 | `Social/Notifications/Notifications.unity` | 📝 V52 | 34 | 12 | 4 duals | Tab Indicators runtime DOColor por categoría (cyan/azul/naranja/gold) → NO · Card root Image runtime-overridden (isRead state) → NO · TypeIcon/IconImage GetTypeColor() multi-color → NO · Title runtime dim/bright por read state → NO · MarkAllReadText runtime cyan/gris por unread count → NO · GroupSeparator creado en runtime → ThemeApplier via code |
| 25 | `Monetization/Shop.unity` | 📝 V52 | 24 | ~22 | 2 duals | Sin runtime color-overrides en UI estructural → ThemeApplier seguro · Card backgrounds/NameText: patrón ×103 cards (TertiaryBackground+TextPrimary) · Section dividers color semántico por sección (gold=currency, purple=themes) → NO · Item ColorPreview/swatch = arte cosmético fijo → NO · Price buttons = color semántico por tipo pago → NO · AnimateCurrencyChange header gems/coins restaura color original → NO · 14 ocultos (PurchaseBlocker + NotEnoughBlocker subtrees) |
| 26 | `Monetization/DailyRewards.unity` | 📝 V52 | 10 | ~51 | 2 duals | Day cards (Day1–7) state-managed en runtime (hardcoded colors) — ThemeApplier override inmediato · Gift icons arte multi-color · ClaimGlow semántico success (alpha 0.15 fijo) · ORANGE_FIRE StreakInfo sin ElementType · 13 ocultos (ClaimAnimationBlocker subtree) |
| 27 | `Monetization/DailyMissions.unity` | 📝 V52 | 24 | ~45 | 4 duals | Timer ORANGE_TIMER semántico (fijo) · MissionCard.prefab: 5 ThemeApplier entries · Placeholder cards (Mission1–9 + headers) destruidos en runtime · RewardClaimBlocker = rewardPopup + missionDetailPanel (doble función) · 16 ocultos |
| 28 | `Monetization/Achievements.unity` | 📝 V52 | 38 | 34 | 8 duals | TrophyCardUI gestiona estado (locked/inProgress/completed) en runtime — CardContainer NO ThemeApplier · DetailProgressText runtime green/gold semántico · CelebrationGlow animado en runtime · RewardIcon/Amount = gem currency fijo · 53 TrophyCard prefab instances · 25 ocultos (DetailPanel + RewardCelebration subtrees + TrophyCard hidden elements) |
| 29 | `Tournaments/TournamentsBrowser.unity` | 📝 V52 | 46 | 45 | 9 duals | Tab Image+Text NO (UpdateTabButton DOColor runtime) · Tab/Indicator YES (no tocado por manager) · 3 dropdowns × 7 template = 21 NO · SearchIcon ⚠️ borderline (white glyph) · FilterPanel oculto por defecto → TA se aplica igual · Runtime tournament items → ThemeApplier en prefab |
| 30 | `Tournaments/TournamentCreate.unity` | 📝 V52 | 102 | 81 | 31 duals | StatusText runtime-override por ShowStatus() (error rojo / success verde) → NO · SelectedGameIcon single TA con applyToImage+applyToOutline · 7 dropdowns × 7 template-internals = 49 NO objects · 3 toggles (StartImmediately/AllowSpectators/Private) + 3 inputs (Name/CustomFee/PrivateCode) auditados hidden-pero-temáticos · ConfirmBlocker + LoadingOverlay presentes como overlays |
| 31 | `Tournaments/TournamentLobby.unity` | 📝 V52 | 74 | 41 | 15 duals | StatusBadge Image NO (GetStatusColor runtime) · CountdownText NO (UpdateTimeDisplay runtime) · Tab indicators NO (SwitchToTab hardcoded) · ChatMessages runtime-created NO · StatusText YES (ShowStatus solo cambia .text) · 4 ⚠️ icon borderlines (GameIcon, ClockIcon, TimerIcon, fee/prize icons) |
| 32–40 | `CashBattle/*.unity` (9 escenas) | 🚫 | — | — | — | Zona CashBattle excluida — paleta gold estática, sin ThemeApplier en ninguna escena |

---

## 📝 DETALLE POR ESCENA

---

### 01 · `_Core/Boot.unity` — 📝 Documentado

#### Objetos a TINTAR
| Objeto (GameObject path) | Componente | ElementType | ThemeData Property | Color NeonDark | Notas |
|---|---|---|---|---|---|
| `Canvas/Background` | Image | `PrimaryBackground` | `primaryBackground` | #0C0C19 | Reset Image.color a blanco (255,255,255) antes de aplicar — actualmente #050A14 hardcoded |
| `Canvas/Subtitle` | TextMeshProUGUI | `TextSecondary` | `textSecondary` | #B2B2B2 | Color actual (0.7,0.7,0.7) coincide exactamente con NeonDark — sin conflicto |
| `Canvas/Subtitle2` | TextMeshProUGUI | `Accent` | `primaryAccent` | #00FFFF | Color actual CyanAccent (0,1,1) coincide exactamente con NeonDark — sin conflicto |
| `Canvas/LoadingBarContainer` | Image | `SliderTrack` | `sliderTrack` | #333340 | Rol de track/fondo de la barra; actual InputBG #1A1A26 — ThemeApplier lo cambiará a #333340 (más claro). Reset Image.color a blanco |
| `Canvas/LoadingText` | TextMeshProUGUI | `TextSecondary` | `textSecondary` | #B2B2B2 | Actual TextLoading #CCCCCC — ThemeApplier lo oscurece ligeramente a #B2B2B2; aceptable. Alternativa: TextPrimary si se quiere más brillante |
| `Canvas/VersionText` | TextMeshProUGUI | `TextDisabled` | `textDisabled` | #666666 | Color actual (0.4,0.4,0.4) coincide exactamente con NeonDark — sin conflicto |

#### Objetos que NO se tintan
| Objeto (GameObject path) | Razón |
|---|---|
| `Canvas/LogoContainer/BrainLogo` | Brand logo multicolor — ilustración de cerebro con degradados en azul, naranja y púrpura (confirmado en screenshots). Tintado destruiría el arte. |
| `Canvas/LogoContainer/TextLogo` | Brand mark — sprite "DIGIT PARK" texto logo; branding fijo, nunca se tinta. |
| `Canvas/NeonParticles` | ParticleSystem — no manejado por ThemeApplier. BootAnimator ya lee `theme.primaryAccent` y `theme.secondaryAccent` directamente desde ThemeManager. |
| `Canvas/LoadingBarContainer/LoadingBarGlowOuter` | Image animada frame a frame por `BootAnimator.AnimateGlowPulse()` usando `theme.glowColor`. Añadir ThemeApplier generaría conflicto con la animación continua. |
| `Canvas/LoadingBarContainer/LoadingBarFill` | Image animada por `BootAnimator.UpdateLoadingBarColor()` que interpola `primaryAccent → secondaryAccent` según progreso. ThemeApplier entraría en conflicto con la animación dinámica. |

#### Casos especiales (2 ThemeAppliers en mismo objeto)
| Objeto | ThemeApplier #1 | ThemeApplier #2 | Razón |
|---|---|---|---|
| — | — | — | Ningún caso dual en esta escena. Sin botones, inputs ni cards con borde. |

#### Notas de escena
- **Implementación**: Todos los ThemeApplier deben añadirse vía `BootUIBuilder.cs` (editor script, `[MenuItem]`), NO por Inspector. El builder reconstruye la escena programáticamente.
- **Fallback TMP**: Si `LogoDigitPark_Text.png` no existe, el builder crea un GO `Title` (TextMeshProUGUI, color CyanAccent). Si ese caso ocurre, `Title` SÍ debe recibir `ThemeApplier(Accent)`. Añadirlo al builder como caso condicional.
- **BootAnimator y ThemeManager**: `BootAnimator.Awake()` lee el tema una sola vez. No suscribe a `OnThemeChanged`. Aceptable — Boot es escena transitoria (no el usuario cambia de tema aquí).
- **AccentSecondary**: No se usa en esta escena. `Subtitle2` usa solo primaryAccent. Ningún elemento usa secondaryAccent directamente en UI (solo via BootAnimator para partículas y glow animado).
- **NetworkStatusBanner**: Se crea en runtime por `BootManager` (`DontDestroyOnLoad`). Se audita independientemente como servicio global, no como parte de Boot.

#### Objetos ocultos (inactivos en escena)
| Objeto (GameObject path) | Componente | Color hardcoded | ¿Necesita ThemeApplier? | Notas |
|---|---|---|---|---|
| *(pendiente — segunda pasada)* | — | — | — | — |

---

### 02 · `_Core/MainMenu.unity` — 📝 Documentado

#### Objetos a TINTAR
| Objeto (GameObject path) | Componente | ElementType | ThemeData Property | Color NeonDark | Notas |
|---|---|---|---|---|---|
| `Canvas/Background` | Image | `PrimaryBackground` | `primaryBackground` | #07111E | Reset Image.color a blanco antes de aplicar |
| `Canvas/Header` | Image | `SecondaryBackground` | `secondaryBackground` | #0D1A2D | Header bar background strip |
| `Canvas/Header/SettingsButton` | Image | `ButtonSecondary` | `buttonSecondary` | #1A2A3D | Reset Image.color a blanco |
| `Canvas/Header/SettingsButton/Icon` | Image | `Accent` | `primaryAccent` | #00FFFF | Icono gear blanco (UI glyph) — tintable ✅ |
| `Canvas/Header/LogoText` | TextMeshProUGUI | `Accent` | `primaryAccent` | #00FFFF | ⚠️ BORDERLINE — tiene VertexGradient CYAN→WHITE; ThemeApplier anulará el gradiente. Revisar con diseñador si perder el gradiente es aceptable |
| `Canvas/Header/CurrencyDisplay/GemsDisplay` | Image | `CardBackground` | `cardBackground` | #0D1E11 | Dual con Glow (Outline verde). Reset Image.color a blanco |
| `Canvas/Header/CurrencyDisplay/GemsDisplay/Amount` | TextMeshProUGUI | `TextPrimary` | `textPrimary` | #F2F2F2 | Color actual TEXT_WHITE (0.95,0.95,0.95) — sin conflicto |
| `Canvas/Header/CurrencyDisplay/GemsDisplay/Plus` | Image | `ButtonSuccess` | `buttonSuccess` | #33D966 | Botón CTA verde "comprar más moneda". Reset Image.color a blanco |
| `Canvas/Header/CurrencyDisplay/GemsDisplay/Plus/PlusText` | TextMeshProUGUI | `TextOnSuccess` | `textOnSuccess` | #0A1A0D | Color oscuro sobre fondo verde — sin conflicto |
| `Canvas/Header/CurrencyDisplay/CoinsDisplay` | Image | `CardBackground` | `cardBackground` | #0D1E11 | Dual con Glow (Outline). Misma estructura que GemsDisplay |
| `Canvas/Header/CurrencyDisplay/CoinsDisplay/Amount` | TextMeshProUGUI | `TextPrimary` | `textPrimary` | #F2F2F2 | — |
| `Canvas/Header/CurrencyDisplay/CoinsDisplay/Plus` | Image | `ButtonSuccess` | `buttonSuccess` | #33D966 | Reset Image.color a blanco |
| `Canvas/Header/CurrencyDisplay/CoinsDisplay/Plus/PlusText` | TextMeshProUGUI | `TextOnSuccess` | `textOnSuccess` | #0A1A0D | — |
| `Canvas/Header/NotificationsButton` | Image | `ButtonSecondary` | `buttonSecondary` | #1A2A3D | Reset Image.color a blanco |
| `Canvas/Header/NotificationsButton/BellIcon` | Image | `Accent` | `primaryAccent` | #00FFFF | Icono campana blanco (UI glyph) — tintable ✅ |
| `Canvas/ProfileCard` | Image | `CardBackground` | `cardBackground` | #0D1A2D | Dual con Glow (Outline). Reset Image.color a blanco |
| `Canvas/ProfileCard/AvatarFrame/GlowRing` | Image | `Glow` | `glowColor` | #00FFFF 80 | Anillo de glow cyan alrededor del avatar. Reset Image.color a blanco |
| `Canvas/ProfileCard/AvatarFrame/BorderRing` | Image | `Accent` | `primaryAccent` | #00FFFF | Anillo borde cyan del avatar. Reset Image.color a blanco |
| `Canvas/ProfileCard/Username` | TextMeshProUGUI | `TextPrimary` | `textPrimary` | #F2F2F2 | — |
| `Canvas/ProfileCard/LevelBadge` | Image | `Accent` | `primaryAccent` | #00FFFF | Badge de nivel con fondo accent. Reset Image.color a blanco |
| `Canvas/ProfileCard/LevelBadge/LevelText` | TextMeshProUGUI | `TextOnPrimary` | `textOnPrimary` | #0A0A12 | Texto oscuro sobre fondo accent |
| `Canvas/DailyRewardCard` | Image | `AccentSecondary` | `secondaryAccent` | #7A0DBF | ⚠️ BORDERLINE — fondo PURPLE; en NeonDark secondaryAccent es magenta (#FF00FF), no púrpura oscuro. Considerar usar `TertiaryBackground` con overlay separado para el color púrpura |
| `Canvas/DailyRewardCard/DailyRewardTitle` | TextMeshProUGUI | `TextPrimary` | `textPrimary` | #F2F2F2 | — |
| `Canvas/DailyRewardCard/DayLabel` | TextMeshProUGUI | `TextSecondary` | `textSecondary` | #B2B2B2 | — |
| `Canvas/DailyRewardCard/ClaimButton` | Image | `ButtonSuccess` | `buttonSuccess` | #33D966 | Dual con ButtonGlowSuccess (Outline/Shadow). Reset Image.color a blanco |
| `Canvas/DailyRewardCard/ClaimButton/ClaimText` | TextMeshProUGUI | `TextOnSuccess` | `textOnSuccess` | #0A1A0D | Texto sobre botón verde |
| `Canvas/QuickActionsPanel/RankingsCard` | Image | `CardBackground` | `cardBackground` | #0D1A2D | Reset Image.color a blanco |
| `Canvas/QuickActionsPanel/RankingsCard/RankingsIcon` | Image | `AccentTertiary` | `tertiaryAccent` | #FFD700 | ⚠️ Icono trofeo/rankings blanco (UI glyph white outline). Tintado gold para reforzar rol de "rankings". Si no es pure white → NO |
| `Canvas/QuickActionsPanel/RankingsCard/RankingsTitle` | TextMeshProUGUI | `TextPrimary` | `textPrimary` | #F2F2F2 | — |
| `Canvas/QuickActionsPanel/SearchCard` | Image | `CardBackground` | `cardBackground` | #0D1A2D | Reset Image.color a blanco |
| `Canvas/QuickActionsPanel/SearchCard/SearchIcon` | Image | `Accent` | `primaryAccent` | #00FFFF | Icono lupa blanco (UI glyph) — tintable ✅ |
| `Canvas/QuickActionsPanel/SearchCard/SearchTitle` | TextMeshProUGUI | `TextPrimary` | `textPrimary` | #F2F2F2 | — |
| `Canvas/QuickActionsPanel/MissionsCard` | Image | `CardBackground` | `cardBackground` | #0D1A2D | Reset Image.color a blanco |
| `Canvas/QuickActionsPanel/MissionsCard/MissionsIcon` | Image | `Accent` | `primaryAccent` | #00FFFF | ⚠️ Icono misiones blanco (UI glyph). Alternativa: `ButtonSuccess` para tinte verde (semántica de misiones=success). Revisar con diseñador |
| `Canvas/QuickActionsPanel/MissionsCard/MissionsTitle` | TextMeshProUGUI | `TextPrimary` | `textPrimary` | #F2F2F2 | — |
| `Canvas/PlayCard` | Image | `ButtonPrimary` | `buttonPrimary` | #00E5FF | Card principal de juego / CTA. Dual con ButtonGlowPrimary (Outline). Reset Image.color a blanco |
| `Canvas/PlayCard/PlayText` | TextMeshProUGUI | `TextOnPrimary` | `textOnPrimary` | #0A0A12 | Texto oscuro sobre fondo accent cyan |
| `Canvas/PlayCard/PlaySubText` | TextMeshProUGUI | `TextOnPrimary` | `textOnPrimary` | #0A0A12 | — |
| `Canvas/CashBattleCard` | Image | `Premium` | `premiumColor` | #FFD700 | Card premium gold. Dual con ButtonGlowPremium (Outline/Shadow). Reset Image.color a blanco. ⚠️ ConfigureButtonColors() no maneja Premium ElementType — Button.colors hover/pressed no se actualizarán automáticamente |
| `Canvas/CashBattleCard/CashBattleText` | TextMeshProUGUI | `TextOnPrimary` | `textOnPrimary` | #0A0A12 | Texto oscuro sobre fondo gold |
| `Canvas/CashBattleCard/CashBattleSubText` | TextMeshProUGUI | `TextOnPrimary` | `textOnPrimary` | #0A0A12 | — |
| `Canvas/CashBattleCard/AgeBadge` | Image | `TertiaryBackground` | `tertiaryBackground` | #1E2A3D | Badge "18+" con fondo oscuro elevado |
| `Canvas/CashBattleCard/AgeBadge/AgeText` | TextMeshProUGUI | `TextSecondary` | `textSecondary` | #B2B2B2 | — |
| `Canvas/ExtraRow/AchievementsCard` | Image | `CardBackground` | `cardBackground` | #0D1A2D | Dual con AccentSecondary (Outline ORANGE). Reset Image.color a blanco |
| `Canvas/ExtraRow/AchievementsCard/AchievementsTitle` | TextMeshProUGUI | `TextPrimary` | `textPrimary` | #F2F2F2 | — |
| `Canvas/ExtraRow/ShopCard` | Image | `CardBackground` | `cardBackground` | #0D1A2D | Dual con AccentTertiary (Outline GOLD). Reset Image.color a blanco |
| `Canvas/ExtraRow/ShopCard/ShopTitle` | TextMeshProUGUI | `TextPrimary` | `textPrimary` | #F2F2F2 | — |
| `Canvas/ExtraRow/PremiumCard` | Image | `CardBackground` | `cardBackground` | #0D1A2D | Dual con Premium (Outline GOLD). Reset Image.color a blanco |
| `Canvas/ExtraRow/PremiumCard/PremiumTitle` | TextMeshProUGUI | `AccentTertiary` | `tertiaryAccent` | #FFD700 | ⚠️ BORDERLINE — título gold para card premium; si el diseñador prefiere blanco → `TextPrimary` |
| `Canvas/PremiumPanel/BlockerPanel` | Image | `Overlay` | `overlayColor` | #000000 80 | Bloqueador semitransparente. Reset Image.color a blanco |
| `Canvas/PremiumPanel/Container` | Image | `TertiaryBackground` | `tertiaryBackground` | #1A2030 | Modal panel elevado. Dual con ButtonGlowPremium (Outline GOLD). Reset Image.color a blanco |
| `Canvas/PremiumPanel/Container/PremiumTitle` | TextMeshProUGUI | `AccentTertiary` | `tertiaryAccent` | #FFD700 | Título gold del panel premium |
| `Canvas/PremiumPanel/Container/PremiumDescription` | TextMeshProUGUI | `TextSecondary` | `textSecondary` | #B2B2B2 | — |
| `Canvas/PremiumPanel/Container/CloseButton` | Image | `ButtonSecondary` | `buttonSecondary` | #1A2A3D | Botón cerrar modal. Reset Image.color a blanco |
| `Canvas/PremiumPanel/Container/CloseButton/CloseText` | TextMeshProUGUI | `TextOnPrimary` | `textOnPrimary` | #F2F2F2 | "×" sobre botón secundario |
| `Canvas/PremiumPanel/Container/PurchaseButton` | Image | `Premium` | `premiumColor` | #FFD700 | CTA compra premium gold. Dual con ButtonGlowPremium (Outline). Reset Image.color a blanco |
| `Canvas/PremiumPanel/Container/PurchaseButton/PurchaseText` | TextMeshProUGUI | `TextOnPrimary` | `textOnPrimary` | #0A0A12 | Texto oscuro sobre gold |

#### Objetos que NO se tintan
| Objeto (GameObject path) | Razón |
|---|---|
| `Canvas/Header` | Contenedor puro (layout group); tiene Image background — incluida en la fila "Header" Image arriba |
| `Canvas/Header/CurrencyDisplay` | Contenedor HorizontalLayoutGroup puro, sin Image ni componente visual |
| `Canvas/Header/CurrencyDisplay/GemsDisplay/Icon` | Sprite de gema de marca (icon_digitgem_single.png) — tiene color GEM_COLOR (#66CCFF) baked in el código; no es pure white |
| `Canvas/Header/CurrencyDisplay/CoinsDisplay/Icon` | Sprite de moneda de marca (icon_digitcoin_single.png) — tiene color COIN_COLOR (#FFD94D) baked in; no es pure white |
| `Canvas/Header/NotificationsButton/Badge` | Punto rojo semántico de notificaciones — color fijo de alerta (rojo). Cambiar con temas rompería la convención universal de "tienes notificaciones" |
| `Canvas/ProfileCard/AvatarFrame` | Contenedor del marco de avatar, sin componente Image propio |
| `Canvas/ProfileCard/AvatarFrame/AvatarMask` | Componente Mask puro — sin Image visual que tintar |
| `Canvas/ProfileCard/AvatarFrame/AvatarImage` | Foto/avatar del jugador — contenido de usuario, no se tinta nunca |
| `Canvas/DailyRewardCard/DailyRewardIcon` | Icono de regalo con tint GOLD (#FFD700) baked in el builder — arte multicolor, no pure white |
| `Canvas/QuickActionsPanel` | Contenedor HorizontalLayoutGroup puro, sin Image |
| `Canvas/PlayCard/PlaySide` | Strip lateral más oscuro del color de ButtonPrimary — no existe ElementType "darker shade of primary". Se implementará como color derivado del tema en el builder |
| `Canvas/PlayCard/PlayIcon` | Icono de la card Play coloreado (#00E5FF cyan filled) — no es pure white |
| `Canvas/CashBattleCard/CashBattleSide` | Strip lateral más oscuro del color Premium/gold — sin ElementType para "darker shade of premium" |
| `Canvas/CashBattleCard/CashBattleInnerGlow` | Overlay decorativo cálido sobre el card — no hay ElementType que mapee a "warm golden glow overlay" |
| `Canvas/CashBattleCard/CashBattleIcon` | Icono de CashBattle coloreado (#00FF88 green filled) — no es pure white |
| `Canvas/ExtraRow` | Contenedor HorizontalLayoutGroup puro, sin Image |
| `Canvas/ExtraRow/AchievementsCard/AchievementsIcon` | Icono de logros coloreado (#FFD700 gold filled) — no es pure white |
| `Canvas/ExtraRow/ShopCard/ShopIcon` | Icono de tienda coloreado (#FF6B35 orange filled) — no es pure white |
| `Canvas/ExtraRow/PremiumCard/PremiumIcon` | Icono premium/corona coloreado (#FFD700 gold filled) — no es pure white |

#### Casos especiales (2 ThemeAppliers en mismo objeto)
| Objeto | ThemeApplier #1 | ThemeApplier #2 | Razón |
|---|---|---|---|
| `GemsDisplay` | `CardBackground` · applyToImage=true | `Glow` · applyToOutline=true | Pill bg oscura + borde verde glow |
| `CoinsDisplay` | `CardBackground` · applyToImage=true | `Glow` · applyToOutline=true | Misma estructura que GemsDisplay |
| `ProfileCard` | `CardBackground` · applyToImage=true | `Glow` · applyToOutline=true | Card bg oscura + borde cyan glowing |
| `DailyRewardCard` | `AccentSecondary` · applyToImage=true ⚠️ | `Glow` · applyToOutline=true | Bg púrpura + borde glow púrpura |
| `ClaimButton` (DailyReward) | `ButtonSuccess` · applyToImage=true | `ButtonGlowSuccess` · applyToOutline=true | Botón verde CTA + glow exterior |
| `PlayCard` | `ButtonPrimary` · applyToImage=true | `ButtonGlowPrimary` · applyToOutline=true | Card CTA cyan + glow exterior |
| `CashBattleCard` | `Premium` · applyToImage=true ⚠️ | `ButtonGlowPremium` · applyToOutline=true | Card gold premium + glow dorado |
| `AchievementsCard` | `CardBackground` · applyToImage=true | `AccentSecondary` · applyToOutline=true ⚠️ | Card dark bg + borde ORANGE. ⚠️ GAP: NeonDark secondaryAccent=magenta≠orange; el borde cambiará de color en otros temas — revisar con diseñador |
| `ShopCard` | `CardBackground` · applyToImage=true | `AccentTertiary` · applyToOutline=true | Card dark bg + borde gold |
| `PremiumCard` | `CardBackground` · applyToImage=true | `Premium` · applyToOutline=true | Card dark bg + borde gold premium |
| `PremiumPanel/Container` | `TertiaryBackground` · applyToImage=true | `ButtonGlowPremium` · applyToOutline=true | Modal elevado + borde glow dorado |
| `PurchaseButton` | `Premium` · applyToImage=true | `ButtonGlowPremium` · applyToOutline=true | CTA gold + glow dorado exterior |

#### Notas de escena
- **Implementación**: Todos los ThemeApplier deben añadirse vía `MainMenuUIBuilder.cs` (editor script, `[MenuItem]`), NO por Inspector.
- **LogoText vertex gradient**: El TMP LogoText tiene `VertexGradient` CYAN→WHITE configurado en el builder. `ThemeApplier` con `applyToText=true` establece un color uniforme, eliminando el degradado. Decisión del diseñador: ¿usar degradado fijo (NO ThemeApplier) o color plano adaptable (SÍ)?
- **DailyRewardCard purple**: `secondaryAccent` en NeonDark es magenta (#FF00FF), no el púrpura oscuro (#7A0DBF) del card. En temas como Emerald, el secondaryAccent podría ser violeta y quedar bien. En Monochrome sería gris, lo que cambiaría la identidad del DailyReward. Alternativa: dejar el fondo purple como hardcoded y solo tintar el texto/botón interior.
- **AchievementsCard borde naranja**: NeonDark no tiene un color "naranja" dedicado. `AccentSecondary` (magenta) es la opción más cercana pero visualmente incorrecta. Considerar añadir `OrangeAccent` como nuevo ElementType en ThemeApplier, o usar `AccentTertiary` (gold) como aproximación.
- **CashBattleCard Premium**: `ThemeApplier.ConfigureButtonColors()` en el switch/case solo maneja `ButtonPrimary`, `ButtonSecondary`, `ButtonDanger`, `ButtonSuccess` — NO `Premium`. Los estados hover/pressed del Button component NO se actualizarán automáticamente al cambiar de tema. Workaround: añadir case `Premium` en `ConfigureButtonColors()` en `ThemeApplier.cs`.
- **PlaySide / CashBattleSide strips**: No existe ElementType para "franja lateral más oscura del color principal". Implementar como colores calculados en `ThemeManager.OnThemeChanged` listener dentro del builder, derivando el color como `Color.Lerp(primaryColor, Color.black, 0.3f)`.
- **PremiumPanel ThemeSelector**: Los items del selector de temas dentro de PremiumPanel se auditan en la escena `Monetization/Shop.unity` (Escena #25) donde se implementa la misma lógica. Omitidos aquí.
- **AccentSecondary uso**: Este escena usa `AccentSecondary` en DailyRewardCard y AchievementsCard outline. En temas con dual-color activo (Emerald, Electric Blue, etc.), estos elementos adoptarán el segundo color del tema correctamente.
- **NetworkStatusBanner**: Global (DontDestroyOnLoad, creado por BootManager) — se audita independientemente, no como parte de MainMenu.

#### Objetos ocultos (inactivos en escena)
| Objeto (GameObject path) | Componente | Color hardcoded | ¿Necesita ThemeApplier? | Notas |
|---|---|---|---|---|
| *(pendiente — segunda pasada)* | — | — | — | — |

---

### 03 · `_Core/Settings.unity` — 📝 Documentado

#### Objetos a TINTAR
| Objeto (GameObject path) | Componente | ElementType | ThemeData Property | Color NeonDark | Notas |
|---|---|---|---|---|---|
| `Canvas/Background` | Image | `PrimaryBackground` | `primaryBackground` | #050A14 | Reset Image.color a blanco |
| `Canvas/Header/BackButton` | Image + Button | `ButtonSecondary` | `buttonSecondary` | #1A2A3D | ⚠️ Es un prefab compartido — auditar y agregar ThemeApplier a nivel de prefab, no por escena. Reset Image.color a blanco |
| `Canvas/Header/TitleText` | TextMeshProUGUI | `Accent` | `primaryAccent` | #00FFFF | Color actual CYAN_NEON — sin conflicto |
| `Canvas/ScrollView/Viewport/SettingsPanel/AccountCard` | Image | `CardBackground` | `cardBackground` | #141938 | Dual con Glow (Outline CYAN_BORDER). Reset Image.color a blanco |
| `AccountCard/AccountCardTitle` | TextMeshProUGUI | `Accent` | `primaryAccent` | #00FFFF | — |
| `AccountCard/ChangeNameButton` | Image | `ButtonSecondary` | `buttonSecondary` | #1A2033 | Reset Image.color a blanco (actual BUTTON_BG) |
| `AccountCard/ChangeNameButton/ChangeNameButtonText` | TextMeshProUGUI | `TextPrimary` | `textPrimary` | #F2F2F2 | — |
| `AccountCard/ChangeNameButton/CostContainer/ChangeNameCostText` | TextMeshProUGUI | `Accent` | `primaryAccent` | #00FFFF | Muestra el costo "100" en accent color — adapta con tema |
| `AccountCard/PlayerIDContainer` | Image | `CardBackground` | `cardBackground` | #141938 | Fila de ID dentro del card. Reset Image.color a blanco |
| `AccountCard/PlayerIDContainer/IDLabel` | TextMeshProUGUI | `TextSecondary` | `textSecondary` | #B2B2B2 | — |
| `AccountCard/PlayerIDContainer/IDText` | TextMeshProUGUI | `TextSecondary` | `textSecondary` | #B2B2B2 | Muestra el ID del jugador; color secundario muted |
| `AccountCard/PlayerIDContainer/CopyButton` | Image | `ButtonPrimary` | `buttonPrimary` | #00E5FF | Reset Image.color a blanco (actual CYAN_NEON) |
| `AccountCard/PlayerIDContainer/CopyButton/CopyButtonText` | TextMeshProUGUI | `TextOnPrimary` | `textOnPrimary` | #0A0A12 | — |
| `AccountCard/Separator` | Image | `TertiaryBackground` | `tertiaryBackground` | #1E2030 | ⚠️ No existe ElementType "Separator". Usando TertiaryBackground como separador de 1px. Reset Image.color a blanco |
| `Canvas/ScrollView/Viewport/SettingsPanel/AudioCard` | Image | `CardBackground` | `cardBackground` | #141938 | Dual con Glow (Outline). Reset Image.color a blanco |
| `AudioCard/AudioCardTitle` | TextMeshProUGUI | `Accent` | `primaryAccent` | #00FFFF | — |
| `AudioCard/SoundVolumeSliderContainer` | Image | `CardBackground` | `cardBackground` | #141938 | Fila de fondo del slider. Reset Image.color a blanco |
| `AudioCard/SoundVolumeSliderContainer/SoundVolumeSliderLabel` | TextMeshProUGUI | `TextPrimary` | `textPrimary` | #F2F2F2 | — |
| `AudioCard/SoundVolumeSliderContainer/SoundValueText` | TextMeshProUGUI | `Accent` | `primaryAccent` | #00FFFF | Porcentaje de volumen en accent color |
| `AudioCard/SoundVolumeSliderContainer/SoundVolumeSlider/Background` | Image | `SliderTrack` | `sliderTrack` | #262633 | Reset Image.color a blanco (actual SLIDER_TRACK) |
| `AudioCard/SoundVolumeSliderContainer/SoundVolumeSlider/Fill Area/Fill` | Image | `SliderFill` | `sliderFill` | #00FFFF | Reset Image.color a blanco (actual CYAN_NEON) |
| `AudioCard/SoundVolumeSliderContainer/SoundVolumeSlider/Handle Slide Area/Handle` | Image | `SliderHandle` | `sliderHandle` | #00FFFF | Reset Image.color a blanco (actual CYAN_NEON) |
| `AudioCard/Separator` (×2) | Image | `TertiaryBackground` | `tertiaryBackground` | #1E2030 | 2 separadores entre filas de Audio |
| `AudioCard/EffectsVolumeSliderContainer` | Image | `CardBackground` | `cardBackground` | #141938 | — |
| `AudioCard/EffectsVolumeSliderContainer/EffectsVolumeSliderLabel` | TextMeshProUGUI | `TextPrimary` | `textPrimary` | #F2F2F2 | — |
| `AudioCard/EffectsVolumeSliderContainer/EffectsValueText` | TextMeshProUGUI | `Accent` | `primaryAccent` | #00FFFF | — |
| `AudioCard/EffectsVolumeSliderContainer/EffectsVolumeSlider/Background` | Image | `SliderTrack` | `sliderTrack` | #262633 | Reset Image.color a blanco |
| `AudioCard/EffectsVolumeSliderContainer/EffectsVolumeSlider/Fill Area/Fill` | Image | `SliderFill` | `sliderFill` | #00FFFF | Reset Image.color a blanco |
| `AudioCard/EffectsVolumeSliderContainer/EffectsVolumeSlider/Handle Slide Area/Handle` | Image | `SliderHandle` | `sliderHandle` | #00FFFF | Reset Image.color a blanco |
| `AudioCard/VibrationToggleContainer` | Image | `CardBackground` | `cardBackground` | #141938 | — |
| `AudioCard/VibrationToggleContainer/VibrationToggleLabel` | TextMeshProUGUI | `TextPrimary` | `textPrimary` | #F2F2F2 | — |
| `AudioCard/VibrationToggleContainer/VibrationToggle` | Image | `ToggleBackground` | `toggleOn` / `toggleOff` | #00FFFF / #404050 | Reset Image.color a blanco; ThemeApplier establece toggleOn cuando isOn=true o toggleOff cuando isOn=false |
| `AudioCard/VibrationToggleContainer/VibrationToggle/VibrationToggleText` | TextMeshProUGUI | `ToggleCheckmark` | `toggleCheckmark` | #0A0A12 | Texto "ON"/"OFF" sobre el toggle; adapta con tema |
| `Canvas/ScrollView/Viewport/SettingsPanel/AppearanceCard` | Image | `CardBackground` | `cardBackground` | #141938 | Dual con Glow (Outline). Reset Image.color a blanco |
| `AppearanceCard/AppearanceCardTitle` | TextMeshProUGUI | `Accent` | `primaryAccent` | #00FFFF | — |
| `AppearanceCard/LanguageDropdownContainer` | Image | `CardBackground` | `cardBackground` | #141938 | — |
| `AppearanceCard/LanguageDropdownContainer/ChangeLanguageLabel` | TextMeshProUGUI | `TextPrimary` | `textPrimary` | #F2F2F2 | — |
| `AppearanceCard/LanguageDropdownContainer/LanguageDropdown` | Image | `InputBackground` | `inputBackground` | #1E2347 | Background del dropdown. Reset Image.color a blanco (actual DROPDOWN_BG) |
| `AppearanceCard/LanguageDropdownContainer/LanguageDropdown/Label` | TextMeshProUGUI | `Accent` | `primaryAccent` | #00FFFF | Texto del valor seleccionado |
| `AppearanceCard/LanguageDropdownContainer/LanguageDropdown/Arrow` | TextMeshProUGUI | `Accent` | `primaryAccent` | #00FFFF | Chevron "v" del dropdown |
| `LanguageDropdown/Template` | Image | `TertiaryBackground` | `tertiaryBackground` | #1E2030 | Template del dropdown (inactivo por defecto). Reset Image.color a blanco |
| `LanguageDropdown/Template/.../Item/Item Background` | Image | `CardBackground` | `cardBackground` | #141938 | Fondo de cada opción en la lista. Reset Image.color a blanco |
| `LanguageDropdown/Template/.../Item/Item Checkmark` | Image | `Accent` | `primaryAccent` | #00FFFF | Indicador de selección activa. Reset Image.color a blanco |
| `LanguageDropdown/Template/.../Item/Item Label` | TextMeshProUGUI | `TextPrimary` | `textPrimary` | #F2F2F2 | — |
| `AppearanceCard/Separator` | Image | `TertiaryBackground` | `tertiaryBackground` | #1E2030 | — |
| `AppearanceCard/ThemeDropdownContainer` | Image | `CardBackground` | `cardBackground` | #141938 | — |
| `AppearanceCard/ThemeDropdownContainer/ChangeThemeLabel` | TextMeshProUGUI | `TextPrimary` | `textPrimary` | #F2F2F2 | — |
| `AppearanceCard/ThemeDropdownContainer/ThemeDropdown` | Image | `InputBackground` | `inputBackground` | #1E2347 | Reset Image.color a blanco |
| `AppearanceCard/ThemeDropdownContainer/ThemeDropdown/Label` | TextMeshProUGUI | `Accent` | `primaryAccent` | #00FFFF | — |
| `AppearanceCard/ThemeDropdownContainer/ThemeDropdown/Arrow` | TextMeshProUGUI | `Accent` | `primaryAccent` | #00FFFF | — |
| `ThemeDropdown/Template` | Image | `TertiaryBackground` | `tertiaryBackground` | #1E2030 | Reset Image.color a blanco |
| `ThemeDropdown/Template/.../Item/Item Background` | Image | `CardBackground` | `cardBackground` | #141938 | Reset Image.color a blanco |
| `ThemeDropdown/Template/.../Item/Item Checkmark` | Image | `Accent` | `primaryAccent` | #00FFFF | Reset Image.color a blanco |
| `ThemeDropdown/Template/.../Item/Item Label` | TextMeshProUGUI | `TextPrimary` | `textPrimary` | #F2F2F2 | — |
| `Canvas/ScrollView/Viewport/SettingsPanel/PremiumSection` | Image | `CardBackground` | `cardBackground` | #141938 | Dual con Premium (Outline GOLD 0.2 alpha). Reset Image.color a blanco |
| `PremiumSection/PremiumSectionTitle` | TextMeshProUGUI | `AccentTertiary` | `tertiaryAccent` | #FFD700 | Título "PREMIUM" en gold |
| `PremiumSection/ShopButton` | Image | `ButtonSecondary` | `buttonSecondary` | #1A2033 | Reset Image.color a blanco |
| `PremiumSection/ShopButton/ShopButtonText` | TextMeshProUGUI | `TextPrimary` | `textPrimary` | #F2F2F2 | — |
| `PremiumSection/ShopButton/RightText` | TextMeshProUGUI | `Accent` | `primaryAccent` | #00FFFF | Chevron ">" |
| `PremiumSection/Separator` | Image | `TertiaryBackground` | `tertiaryBackground` | #1E2030 | — |
| `PremiumSection/RestorePurchasesButton` | Image | `ButtonSecondary` | `buttonSecondary` | #1A2033 | Reset Image.color a blanco |
| `PremiumSection/RestorePurchasesButton/RestorePurchasesButtonText` | TextMeshProUGUI | `TextPrimary` | `textPrimary` | #F2F2F2 | — |
| `PremiumSection/RestorePurchasesButton/RightText` | TextMeshProUGUI | `Accent` | `primaryAccent` | #00FFFF | — |
| `Canvas/ScrollView/Viewport/SettingsPanel/LegalCard` | Image | `CardBackground` | `cardBackground` | #141938 | Dual con Glow (Outline). Reset Image.color a blanco |
| `LegalCard/LegalCardTitle` | TextMeshProUGUI | `Accent` | `primaryAccent` | #00FFFF | — |
| `LegalCard/TermsButton` | Image | `ButtonSecondary` | `buttonSecondary` | #1A2033 | Reset Image.color a blanco |
| `LegalCard/TermsButton/TermsButtonText` | TextMeshProUGUI | `TextPrimary` | `textPrimary` | #F2F2F2 | — |
| `LegalCard/TermsButton/RightText` | TextMeshProUGUI | `Accent` | `primaryAccent` | #00FFFF | — |
| `LegalCard/PrivacyButton` | Image | `ButtonSecondary` | `buttonSecondary` | #1A2033 | — |
| `LegalCard/PrivacyButton/PrivacyButtonText` | TextMeshProUGUI | `TextPrimary` | `textPrimary` | #F2F2F2 | — |
| `LegalCard/PrivacyButton/RightText` | TextMeshProUGUI | `Accent` | `primaryAccent` | #00FFFF | — |
| `LegalCard/ResponsibleGamingButton` | Image | `ButtonSecondary` | `buttonSecondary` | #1A2033 | — |
| `LegalCard/ResponsibleGamingButton/ResponsibleGamingButtonText` | TextMeshProUGUI | `TextPrimary` | `textPrimary` | #F2F2F2 | — |
| `LegalCard/ResponsibleGamingButton/RightText` | TextMeshProUGUI | `Accent` | `primaryAccent` | #00FFFF | — |
| `LegalCard/TriumphTermsButton` | Image | `ButtonSecondary` | `buttonSecondary` | #1A2033 | — |
| `LegalCard/TriumphTermsButton/TriumphTermsButtonText` | TextMeshProUGUI | `TextPrimary` | `textPrimary` | #F2F2F2 | — |
| `LegalCard/TriumphTermsButton/RightText` | TextMeshProUGUI | `Accent` | `primaryAccent` | #00FFFF | — |
| `LegalCard/SelfExclusionButton` | Image | `ButtonSecondary` | `buttonSecondary` | #1A2033 | Reset Image.color a blanco |
| `LegalCard/SelfExclusionButton/SelfExclusionButtonText` | TextMeshProUGUI | `TextPrimary` | `textPrimary` | #F2F2F2 | — |
| `LegalCard/SelfExclusionButton/RightText` | TextMeshProUGUI | `Error` | `errorColor` | #FF4D4D | ⚠️ Este chevron es rojo DANGER_RED — usa Error para mantener semántica de advertencia |
| `LegalCard/Separator` (×4) | Image | `TertiaryBackground` | `tertiaryBackground` | #1E2030 | 4 separadores entre botones legales |
| `Canvas/ScrollView/Viewport/SettingsPanel/DangerCard` | Image | `CardBackground` | `cardBackground` | #141938 | Dual con ButtonGlowDanger (Outline DANGER_BORDER rojo). Reset Image.color a blanco |
| `DangerCard/DangerCardTitle` | TextMeshProUGUI | `Error` | `errorColor` | #FF4D4D | Título "DANGER ZONE" en rojo |
| `DangerCard/Separator` | Image | `TertiaryBackground` | `tertiaryBackground` | #1E2030 | — |
| `DangerCard/LogoutButton` | Image | `ButtonSecondary` | `buttonSecondary` | #404050 | Sign Out tiene fondo gris neutro (no es danger action directa). Reset Image.color a blanco |
| `DangerCard/LogoutButton/LogoutButtonText` | TextMeshProUGUI | `TextSecondary` | `textSecondary` | #B2B2B2 | Texto gris para Sign Out — acción menos peligrosa |
| `DangerCard/DeleteAccountButton` | Image | `ButtonDanger` | `buttonDanger` | #4D1414 | Dual con ButtonGlowDanger (Outline rojo). Reset Image.color a blanco |
| `DangerCard/DeleteAccountButton/DeleteAccountButtonText` | TextMeshProUGUI | `TextOnDanger` | `textOnDanger` | #FF4D4D | Texto rojo sobre fondo danger |
| `Canvas/ScrollView/Viewport/SettingsPanel/VersionText` | TextMeshProUGUI | `TextDisabled` | `textDisabled` | #666666 | Versión de app — texto inactivo muted |
| `LogoutConfirmPanel/BlockerPanel` | Image | `Overlay` | `overlayColor` | #000000 88 | Reset Image.color a blanco |
| `LogoutConfirmPanel/Panel` | Image | `TertiaryBackground` | `tertiaryBackground` | #141938 | Modal elevado. Reset Image.color a blanco |
| `LogoutConfirmPanel/Panel/ConfirmTitleLabel` | TextMeshProUGUI | `Error` | `errorColor` | #FF4D4D | Título del diálogo en rojo |
| `LogoutConfirmPanel/Panel/ConfirmMessageLabel` | TextMeshProUGUI | `TextPrimary` | `textPrimary` | #F2F2F2 | — |
| `LogoutConfirmPanel/Panel/ConfirmButton` | Image | `ButtonDanger` | `buttonDanger` | #4D1414 | Reset Image.color a blanco (actual DANGER_RED) |
| `LogoutConfirmPanel/Panel/ConfirmButton/ConfirmButtonText` | TextMeshProUGUI | `TextOnDanger` | `textOnDanger` | #FF4D4D | — |
| `LogoutConfirmPanel/Panel/CancelButton` | Image | `ButtonSecondary` | `buttonSecondary` | #1A2033 | Reset Image.color a blanco |
| `LogoutConfirmPanel/Panel/CancelButton/CancelButtonText` | TextMeshProUGUI | `TextSecondary` | `textSecondary` | #B2B2B2 | — |
| `DeleteConfirmPanel/BlockerPanel` | Image | `Overlay` | `overlayColor` | #000000 88 | (mismo patrón que LogoutConfirmPanel) |
| `DeleteConfirmPanel/Panel` | Image | `TertiaryBackground` | `tertiaryBackground` | #141938 | — |
| `DeleteConfirmPanel/Panel/ConfirmTitleLabel` | TextMeshProUGUI | `Error` | `errorColor` | #FF4D4D | — |
| `DeleteConfirmPanel/Panel/ConfirmMessageLabel` | TextMeshProUGUI | `TextPrimary` | `textPrimary` | #F2F2F2 | — |
| `DeleteConfirmPanel/Panel/ConfirmButton` | Image | `ButtonDanger` | `buttonDanger` | #4D1414 | — |
| `DeleteConfirmPanel/Panel/ConfirmButton/ConfirmButtonText` | TextMeshProUGUI | `TextOnDanger` | `textOnDanger` | #FF4D4D | — |
| `DeleteConfirmPanel/Panel/CancelButton` | Image | `ButtonSecondary` | `buttonSecondary` | #1A2033 | — |
| `DeleteConfirmPanel/Panel/CancelButton/CancelButtonText` | TextMeshProUGUI | `TextSecondary` | `textSecondary` | #B2B2B2 | — |
| `SelfExclusionConfirmPanel/BlockerPanel` | Image | `Overlay` | `overlayColor` | #000000 88 | — |
| `SelfExclusionConfirmPanel/Panel` | Image | `TertiaryBackground` | `tertiaryBackground` | #141938 | — |
| `SelfExclusionConfirmPanel/Panel/ConfirmTitleLabel` | TextMeshProUGUI | `Error` | `errorColor` | #FF4D4D | — |
| `SelfExclusionConfirmPanel/Panel/ConfirmMessageLabel` | TextMeshProUGUI | `TextPrimary` | `textPrimary` | #F2F2F2 | — |
| `SelfExclusionConfirmPanel/Panel/ConfirmButton` | Image | `ButtonDanger` | `buttonDanger` | #4D1414 | — |
| `SelfExclusionConfirmPanel/Panel/ConfirmButton/ConfirmButtonText` | TextMeshProUGUI | `TextOnDanger` | `textOnDanger` | #FF4D4D | — |
| `SelfExclusionConfirmPanel/Panel/CancelButton` | Image | `ButtonSecondary` | `buttonSecondary` | #1A2033 | — |
| `SelfExclusionConfirmPanel/Panel/CancelButton/CancelButtonText` | TextMeshProUGUI | `TextSecondary` | `textSecondary` | #B2B2B2 | — |
| `ChangeNamePanel/BlockerPanel` | Image | `Overlay` | `overlayColor` | #000000 88 | — |
| `ChangeNamePanel/Panel` | Image | `TertiaryBackground` | `tertiaryBackground` | #141938 | — |
| `ChangeNamePanel/Panel/ChangeNameTitle` | TextMeshProUGUI | `Accent` | `primaryAccent` | #00FFFF | — |
| `ChangeNamePanel/Panel/InputField` | Image | `InputBackground` | `inputBackground` | #1E2347 | Reset Image.color a blanco (actual DROPDOWN_BG) |
| `ChangeNamePanel/Panel/InputField/Text Area/Placeholder` | TextMeshProUGUI | `InputPlaceholder` | `inputPlaceholder` | #808080 | — |
| `ChangeNamePanel/Panel/InputField/Text Area/Text` | TextMeshProUGUI | `TextPrimary` | `textPrimary` | #F2F2F2 | Texto de entrada del usuario |
| `ChangeNamePanel/Panel/ConfirmButton` | Image | `ButtonPrimary` | `buttonPrimary` | #00E5FF | Reset Image.color a blanco (actual CYAN_NEON) |
| `ChangeNamePanel/Panel/ConfirmButton/ConfirmButtonText` | TextMeshProUGUI | `TextOnPrimary` | `textOnPrimary` | #0A0A12 | — |
| `ChangeNamePanel/Panel/CancelButton` | Image | `ButtonSecondary` | `buttonSecondary` | #1A2033 | Reset Image.color a blanco |
| `ChangeNamePanel/Panel/CancelButton/CancelButtonText` | TextMeshProUGUI | `TextSecondary` | `textSecondary` | #B2B2B2 | — |

#### Objetos que NO se tintan
| Objeto (GameObject path) | Razón |
|---|---|
| `Canvas/Header` | Contenedor puro, sin componente Image |
| `Canvas/ScrollView` | ScrollRect — componente funcional, sin Image visual |
| `Canvas/ScrollView/Viewport` | Image color=clear (transparente) + RectMask2D — invisible, no tintar |
| `Canvas/ScrollView/Viewport/SettingsPanel` | VerticalLayoutGroup + ContentSizeFitter puro, sin Image |
| `AccountCard/ChangeNameButton/CostContainer` | HorizontalLayoutGroup puro, sin Image |
| `AccountCard/ChangeNameButton/CostContainer/GemIcon` | Sprite de gema de marca (icon_digitgem_single.png) — tiene colores propios en el arte; no es pure white |
| `AudioCard/SoundVolumeSliderContainer/SoundVolumeSlider` | Slider root sin Image — componente funcional puro |
| `SoundVolumeSlider/Fill Area` | Contenedor de área, sin Image |
| `SoundVolumeSlider/Handle Slide Area` | Contenedor de área, sin Image |
| `AudioCard/EffectsVolumeSliderContainer/EffectsVolumeSlider` | Mismo motivo que SoundVolumeSlider |
| `EffectsVolumeSlider/Fill Area` | Contenedor, sin Image |
| `EffectsVolumeSlider/Handle Slide Area` | Contenedor, sin Image |
| `LanguageDropdown/Template/Viewport` | Mask con showMaskGraphic=false — Image invisible por diseño; tintar no tendría efecto |
| `LanguageDropdown/Template/Viewport/Content` | VLG + CSF, sin Image |
| `LanguageDropdown/Template/Viewport/Content/Item` | Toggle root sin Image directo |
| `ThemeDropdown/Template/Viewport` | Mismo motivo que LanguageDropdown Template/Viewport |
| `ThemeDropdown/Template/Viewport/Content` | Sin Image |
| `ThemeDropdown/Template/Viewport/Content/Item` | Toggle root sin Image |
| `ThemeDropdown/Template/.../Item/LockIcon` | Sprite icon_lock_gold — tiene color gold baked in; no es pure white. Gestionado por ThemeDropdownController |
| `LogoutConfirmPanel` (root) | Wrapper de ConfirmPanelUI sin Image |
| `DeleteConfirmPanel` (root) | Wrapper de ConfirmPanelUI sin Image |
| `SelfExclusionConfirmPanel` (root) | Wrapper de ConfirmPanelUI sin Image |
| `ChangeNamePanel` (root) | Wrapper de InputPanelUI sin Image |
| `ChangeNamePanel/Panel/InputField/Text Area` | RectMask2D puro, sin Image visual |
| `ChangeNamePanel/Panel/InputField/Text Area/Text` | Se lista en YES como TextPrimary — no duplicar |
| `Canvas/ScrollView/Viewport/SettingsPanel` (containers de VLG) | Todos los contenedores HLG/VLG sin Image en AccountCard, AudioCard, etc. |

#### Casos especiales (2 ThemeAppliers en mismo objeto)
| Objeto | ThemeApplier #1 | ThemeApplier #2 | Razón |
|---|---|---|---|
| `AccountCard` | `CardBackground` · applyToImage=true | `Glow` · applyToOutline=true | Card bg oscura + borde glow cyan (CYAN_BORDER) |
| `AudioCard` | `CardBackground` · applyToImage=true | `Glow` · applyToOutline=true | — |
| `AppearanceCard` | `CardBackground` · applyToImage=true | `Glow` · applyToOutline=true | — |
| `PremiumSection` | `CardBackground` · applyToImage=true | `Premium` · applyToOutline=true | Card con borde gold (GOLD 0.2 alpha) |
| `LegalCard` | `CardBackground` · applyToImage=true | `Glow` · applyToOutline=true | — |
| `DangerCard` | `CardBackground` · applyToImage=true | `ButtonGlowDanger` · applyToOutline=true | Card con borde danger rojo |
| `DeleteAccountButton` | `ButtonDanger` · applyToImage=true | `ButtonGlowDanger` · applyToOutline=true | Botón destructivo + glow rojo exterior |
| `ChangeNamePanel/Panel/ConfirmButton` | `ButtonPrimary` · applyToImage=true | `ButtonGlowPrimary` · applyToOutline=true | CTA confirm con glow primario ⚠️ solo si el builder agrega Outline al ConfirmButton — verificar |

#### Notas de escena
- **Implementación**: Todos los ThemeApplier deben añadirse vía `SettingsUIBuilder.cs`. También aplicar el mismo patrón en `CreateSettingsRow`, `CreateSliderRow`, `CreateToggleRow`, `CreateDropdownRow`, `CreateCard`, `BuildConfirmPanelOverlay`, `BuildInputPanelOverlay`.
- **BackButton prefab**: `Assets/_Project/Prefabs/Common/BackButton.prefab` es un prefab compartido entre varias escenas. El ThemeApplier (`ButtonSecondary` en Image + `Accent` en el ícono flecha) debe añadirse **a nivel de prefab**, no por escena. Cuando se audite cualquier escena con BackButton, se aplica una sola vez en el prefab.
- **Separadores sin ElementType**: `SEPARATOR_COLOR = (0.3, 0.3, 0.4, 0.2)` — no existe tipo `Separator` en ThemeApplier. Se usa `TertiaryBackground` como aproximación. Considerar añadir `Separator` ElementType en ThemeApplier (mapearía a `textDisabled` o `secondaryBackground` al 20% alpha). Son ~12 separadores en esta escena.
- **ToggleBackground runtime**: El toggle tiene dos colores según estado (isOn → CYAN_NEON, isOff → TOGGLE_OFF_BG). `ToggleBackground` en ThemeApplier mapea a `toggleOn` o `toggleOff` dependiendo del estado actual. Verificar en `ThemeApplier.GetColorForElement()` que el caso `ToggleBackground` lee `theme.toggleOn` o `theme.toggleOff` según `GetComponent<Toggle>()?.isOn`.
- **Dropdown Template**: Las Templates de los dropdowns están inactivas por defecto (`template.SetActive(false)`) — se activan al abrir el dropdown. Los ThemeAppliers en los items se aplican en `OnEnable`, que se llama cuando la template aparece. Funciona correctamente.
- **LockIcon en ThemeDropdown**: Gestionado por `ThemeDropdownController`. Muestra icon_lock_gold o icon_lock_silver según si el tema está bloqueado. Estos iconos son arte coloreado — no se tintan.
- **Screenshots cross-check**: Screenshot 2 muestra "change_theme" y "toggle_on" como textos — esto confirma un bug de localización en runtime (AutoLocalizer no encontró la key en el momento del screenshot). No afecta ThemeApplier. Los objetos están presentes en la jerarquía.
- **AccentSecondary**: No se usa en esta escena — sin elementos que usen secondaryAccent. Estructura muy monochromática (cyan + rojo danger + gold premium).
- **PremiumPanelOverlay**: `BuildPremiumPanelOverlay()` está definido en el builder pero **NO se llama** desde `BuildOverlayPanels()`. Si existe en la escena como prefab independiente, se auditará cuando se haga la auditoría de ese prefab.

#### Objetos ocultos (inactivos en escena)
| Objeto (GameObject path) | Componente | Color hardcoded | ¿Necesita ThemeApplier? | Notas |
|---|---|---|---|---|
| *(pendiente — segunda pasada)* | — | — | — | — |

---

### 04 · `Auth/Login.unity` — 📝 Documentado

Escena de login con email + Google + Apple. Incluye 2 popups creados en runtime (UsernamePopup, ForgotPasswordPopup). 68 objetos evaluados.

#### Objetos a TINTAR — Estáticos (UIBuilder) · 28 objetos
| Objeto (GameObject path) | Componente | ElementType | ThemeData Property | Color NeonDark | Notas |
|---|---|---|---|---|---|
| `Canvas/Background` | Image | `PrimaryBackground` | `primaryBackground` | #050A14 | Reset Image.color a blanco |
| `Canvas/BackButton` | Image+Button | `ButtonSecondary` | `buttonSecondary` | #1A1A2E | Prefab BackButton.prefab — añadir a nivel de prefab |
| `Canvas/LoginCard` | Image | `CardBackground` | `cardBackground` | #1E1E3A | Reset color · ver Casos especiales |
| `Canvas/LoginCard` | Outline | `Glow` | `glowColor` | #00FFFF 80% | Dual — borde neon cyan |
| `Canvas/LoginCard/Content/LoginTitle` | TextMeshProUGUI | `Accent` | `primaryAccent` | #00FFFF | "SIGN IN" en cyan |
| `Canvas/LoginCard/Content/EmailInput` | Image | `InputBackground` | `inputBackground` | #141C38 | Reset color · ver Casos especiales |
| `Canvas/LoginCard/Content/EmailInput` | Outline | `Glow` | `glowColor` | #00FFFF 80% | Dual — borde neon del input |
| `Canvas/LoginCard/Content/EmailInput/TextArea/EmailPlaceholder` | TextMeshProUGUI | `InputPlaceholder` | `inputPlaceholder` | #AAAAAA | Placeholder "Email" |
| `Canvas/LoginCard/Content/EmailInput/TextArea/Text` | TextMeshProUGUI | `TextPrimary` | `textPrimary` | #FFFFFF | Texto tipado del input |
| `Canvas/LoginCard/Content/PasswordInput` | Image | `InputBackground` | `inputBackground` | #141C38 | Reset color · ver Casos especiales |
| `Canvas/LoginCard/Content/PasswordInput` | Outline | `Glow` | `glowColor` | #00FFFF 80% | Dual |
| `Canvas/LoginCard/Content/PasswordInput/TextArea/PasswordPlaceholder` | TextMeshProUGUI | `InputPlaceholder` | `inputPlaceholder` | #AAAAAA | Placeholder "Password" |
| `Canvas/LoginCard/Content/PasswordInput/TextArea/Text` | TextMeshProUGUI | `TextPrimary` | `textPrimary` | #FFFFFF | Texto tipado del password |
| `Canvas/LoginCard/Content/PasswordInput/EyeToggle` | Image | `Accent` | `primaryAccent` | #00FFFF | ⚠️ BORDERLINE — EyeOpen.png probablemente white glyph (UIBuilder tinta con CyanNeon) |
| `Canvas/LoginCard/Content/ForgotPasswordButton` | TextMeshProUGUI | `Accent` | `primaryAccent` | #00FFFF | Link de texto sin Image bg. El TMP ES el gráfico visual |
| `Canvas/LoginCard/Content/CheckboxRow/RememberCheckbox/Background` | Image | `ToggleBackground` | `toggleOn`/`toggleOff` | #141C38 | Reset color · ver Casos especiales |
| `Canvas/LoginCard/Content/CheckboxRow/RememberCheckbox/Background` | Outline | `Glow` | `glowColor` | #00FFFF 80% | Dual — borde del toggle |
| `Canvas/LoginCard/Content/CheckboxRow/RememberCheckbox/Background/Checkmark` | Image | `ToggleCheckmark` | `toggleCheckmark` | #00FFFF | Checkmark del toggle. Reset color a blanco |
| `Canvas/LoginCard/Content/CheckboxRow/RememberMeText` | TextMeshProUGUI | `TextPrimary` | `textPrimary` | #FFFFFF | "Remember me" |
| `Canvas/LoginCard/Content/LoginButton` | Image+Button | `ButtonPrimary` | `buttonPrimary` | #00FFFF | Reset Image.color (era CyanNeon) |
| `Canvas/LoginCard/Content/LoginButton/LoginButtonText` | TextMeshProUGUI | `TextOnPrimary` | `textOnPrimary` | #050A14 | "Sign In" oscuro sobre botón cyan |
| `Canvas/LoginCard/Content/OrDivider/LeftLine` | Image | `TertiaryBackground` | `tertiaryBackground` | #333355 | Línea separadora gris |
| `Canvas/LoginCard/Content/OrDivider/OrText` | TextMeshProUGUI | `TextSecondary` | `textSecondary` | #AAAAAA | "or" separador |
| `Canvas/LoginCard/Content/OrDivider/RightLine` | Image | `TertiaryBackground` | `tertiaryBackground` | #333355 | Línea separadora gris |
| `Canvas/LoginCard/Content/GoogleButton/GoogleSignInText` | TextMeshProUGUI | `TextPrimary` | `textPrimary` | #FFFFFF | ⚠️ Brand guidelines Google mandan texto blanco — confirmar si themes cambian textPrimary a otro color |
| `Canvas/LoginCard/Content/AppleButton/AppleSignInText` | TextMeshProUGUI | `TextPrimary` | `textPrimary` | #FFFFFF | ⚠️ Brand guidelines Apple mandan texto blanco — mismo riesgo que Google |
| `Canvas/NoAccountText` | TextMeshProUGUI | `TextSecondary` | `textSecondary` | #AAAAAA | "Don't have an account?" |
| `Canvas/RegisterButton` | Image+Button | `ButtonPrimary` | `buttonPrimary` | #00FFFF | Reset Image.color (era CyanNeon) |
| `Canvas/RegisterButton/RegisterButtonText` | TextMeshProUGUI | `TextOnPrimary` | `textOnPrimary` | #050A14 | "Create an account" oscuro sobre cyan |
| `Canvas/LoadingPanel` | Image | `Overlay` | `overlayColor` | #00000080 | Oculto por defecto. ThemeApplier aplica al activar |
| `Canvas/LoadingPanel/Spinner` | Image | `Accent` | `primaryAccent` | #00FFFF | Reset Image.color a blanco |
| `Canvas/LoadingPanel/GeneralLoadingText` | TextMeshProUGUI | `TextPrimary` | `textPrimary` | #FFFFFF | "Loading..." |

#### Objetos a TINTAR — Runtime: UsernamePopup · 13 objetos
| Objeto (GameObject path) | Componente | ElementType | ThemeData Property | Color NeonDark | Notas |
|---|---|---|---|---|---|
| `Canvas/UsernamePopup/Overlay` | Image | `Overlay` | `overlayColor` | #000000B3 | Overlay semi-transparente del popup |
| `Canvas/UsernamePopup/Overlay/Panel` | Image | `CardBackground` | `cardBackground` | #1A1A33 | Fondo del panel · ver Casos especiales |
| `Canvas/UsernamePopup/Overlay/Panel` | Outline | `Glow` | `glowColor` | #00000050 | Dual — outline sutil del panel |
| `Canvas/UsernamePopup/Overlay/Panel/Title` | TextMeshProUGUI | `Accent` | `primaryAccent` | #00FFFF | UIFactory.ElectricBlue ≈ cyan |
| `Canvas/UsernamePopup/Overlay/Panel/UsernameInputField` | Image | `InputBackground` | `inputBackground` | #333350 | ver Casos especiales |
| `Canvas/UsernamePopup/Overlay/Panel/UsernameInputField` | Outline | `Glow` | `glowColor` | #00000033 | Dual — outline sutil |
| `Canvas/UsernamePopup/Overlay/Panel/UsernameInputField/Placeholder` | TextMeshProUGUI | `InputPlaceholder` | `inputPlaceholder` | #888888 | Placeholder "username" |
| `Canvas/UsernamePopup/Overlay/Panel/UsernameInputField/Text` | TextMeshProUGUI | `TextPrimary` | `textPrimary` | #FFFFFF | Texto tipado |
| `Canvas/UsernamePopup/Overlay/Panel/ErrorText` | TextMeshProUGUI | `Error` | `errorColor` | #FF4D4D | Color estático de error. Oculto por defecto |
| `Canvas/UsernamePopup/Overlay/Panel/ConfirmButton` | Image+Button | `ButtonPrimary` | `buttonPrimary` | #00FFFF | UIFactory.ElectricBlue |
| `Canvas/UsernamePopup/Overlay/Panel/ConfirmButton/Text` | TextMeshProUGUI | `TextOnPrimary` | `textOnPrimary` | #050A14 | Texto del botón Confirm |
| `Canvas/UsernamePopup/Overlay/Panel/LaterButton` | Image+Button | `ButtonSecondary` | `buttonSecondary` | #1A1A2E | Botón gris "Más tarde" |
| `Canvas/UsernamePopup/Overlay/Panel/LaterButton/Text` | TextMeshProUGUI | `TextPrimary` | `textPrimary` | #FFFFFF | Texto sobre botón secundario gris |
| `Canvas/UsernamePopup/Overlay/Panel/CancelButton` | Image+Button | `ButtonDanger` | `buttonDanger` | #B33333 | Botón rojo "Cancelar" |
| `Canvas/UsernamePopup/Overlay/Panel/CancelButton/Text` | TextMeshProUGUI | `TextOnDanger` | `textOnDanger` | #FFFFFF | Texto sobre botón danger |

#### Objetos a TINTAR — Runtime: ForgotPasswordPopup · 11 objetos
| Objeto (GameObject path) | Componente | ElementType | ThemeData Property | Color NeonDark | Notas |
|---|---|---|---|---|---|
| `Canvas/ForgotPasswordPopup/Overlay` | Image+Button | `Overlay` | `overlayColor` | #000000CC | Overlay + botón invisible para cerrar al tocar fuera |
| `Canvas/ForgotPasswordPopup/Overlay/Panel` | Image | `CardBackground` | `cardBackground` | #141428 | ver Casos especiales |
| `Canvas/ForgotPasswordPopup/Overlay/Panel` | Outline | `Glow` | `glowColor` | #00CCFF66 | Dual — outline neon panel |
| `Canvas/ForgotPasswordPopup/Overlay/Panel/Title` | TextMeshProUGUI | `Accent` | `primaryAccent` | #00FFFF | UIFactory.ElectricBlue |
| `Canvas/ForgotPasswordPopup/Overlay/Panel/Description` | TextMeshProUGUI | `TextSecondary` | `textSecondary` | #AAAAAA | Descripción gris |
| `Canvas/ForgotPasswordPopup/Overlay/Panel/EmailInputField` | Image | `InputBackground` | `inputBackground` | #262640 | ver Casos especiales |
| `Canvas/ForgotPasswordPopup/Overlay/Panel/EmailInputField` | Outline | `Glow` | `glowColor` | #00CCFF33 | Dual — outline sutil |
| `Canvas/ForgotPasswordPopup/Overlay/Panel/EmailInputField/TextArea/Placeholder` | TextMeshProUGUI | `InputPlaceholder` | `inputPlaceholder` | #888888 | Placeholder email |
| `Canvas/ForgotPasswordPopup/Overlay/Panel/EmailInputField/TextArea/Text` | TextMeshProUGUI | `TextPrimary` | `textPrimary` | #FFFFFF | Texto tipado |
| `Canvas/ForgotPasswordPopup/Overlay/Panel/SendButton` | Image+Button | `ButtonPrimary` | `buttonPrimary` | #00FFFF | UIFactory.ElectricBlue |
| `Canvas/ForgotPasswordPopup/Overlay/Panel/SendButton/Text` | TextMeshProUGUI | `TextOnPrimary` | `textOnPrimary` | #050A14 | Texto oscuro sobre cyan |
| `Canvas/ForgotPasswordPopup/Overlay/Panel/CancelButton` | Image+Button | `ButtonSecondary` | `buttonSecondary` | #1A1A2E | Botón gris cancelar |
| `Canvas/ForgotPasswordPopup/Overlay/Panel/CancelButton/Text` | TextMeshProUGUI | `TextPrimary` | `textPrimary` | #FFFFFF | Texto sobre botón secundario |

#### Objetos que NO se tintan (16 objetos)
| Objeto (GameObject path) | Razón |
|---|---|
| `Canvas/Logo` | Texto de marca "Digit Park" — branding |
| `Canvas/LoginCard/Content` | VerticalLayoutGroup puro — sin Image ni TMP |
| `Canvas/LoginCard/Content/EmailInput/TextArea` | RectTransform/Mask container — sin componente visual |
| `Canvas/LoginCard/Content/PasswordInput/TextArea` | RectTransform/Mask container — sin componente visual |
| `Canvas/LoginCard/Content/CheckboxRow` | HorizontalLayoutGroup puro — sin Image ni TMP |
| `Canvas/LoginCard/Content/CheckboxRow/RememberCheckbox` | Raíz del Toggle — no tiene Image propia (sus hijos la tienen) |
| `Canvas/LoginCard/Content/OrDivider` | LayoutElement container — sin componente visual directo |
| `Canvas/LoginCard/Content/GoogleButton` (Image) | Fondo mandado por Google Brand Guidelines (#131314) — no modificable por tema |
| `Canvas/LoginCard/Content/GoogleButton/Icon` | `google_g_logo.png` — icono multi-color de marca Google, NO tintable |
| `Canvas/LoginCard/Content/AppleButton` (Image) | Fondo negro mandado por Apple HIG — no modificable por tema |
| `Canvas/LoginCard/Content/AppleButton/Icon` | `apple_logo_black.png` — logo de marca Apple, NO tintable |
| `Canvas/ErrorPanel` | Prefab `ErrorPanel.prefab` — auditado por separado a nivel de prefab |
| `Canvas/UsernamePopup` | Raíz del GO runtime — solo MonoBehaviour, sin componente visual |
| `Canvas/ForgotPasswordPopup` | Raíz del GO runtime — solo MonoBehaviour, sin componente visual |
| `Canvas/ForgotPasswordPopup/Overlay/Panel/MessageText` | Color controlado en runtime: verde (`ShowSuccess`) o rojo (`ShowError`). Colores semánticos que no deben seguir el tema |
| `Canvas/ForgotPasswordPopup/Overlay/Panel/EmailInputField/TextArea` | RectTransform/Mask container — sin componente visual |

#### Casos especiales (2 ThemeAppliers en mismo objeto) — 8 casos
| Objeto | ThemeApplier #1 | ThemeApplier #2 | Razón |
|---|---|---|---|
| `Canvas/LoginCard` | `CardBackground`, applyToImage=true | `Glow`, applyToOutline=true | Image (fondo) + Outline (borde neon cyan) |
| `Canvas/LoginCard/Content/EmailInput` | `InputBackground`, applyToImage=true | `Glow`, applyToOutline=true | Image (fondo input) + Outline (borde cyan) |
| `Canvas/LoginCard/Content/PasswordInput` | `InputBackground`, applyToImage=true | `Glow`, applyToOutline=true | Image (fondo input) + Outline (borde cyan) |
| `Canvas/LoginCard/Content/CheckboxRow/RememberCheckbox/Background` | `ToggleBackground`, applyToImage=true | `Glow`, applyToOutline=true | Image (fondo toggle) + Outline (borde cyan) |
| `Canvas/UsernamePopup/Overlay/Panel` | `CardBackground`, applyToImage=true | `Glow`, applyToOutline=true | Panel popup: fondo + outline |
| `Canvas/UsernamePopup/Overlay/Panel/UsernameInputField` | `InputBackground`, applyToImage=true | `Glow`, applyToOutline=true | Input del popup |
| `Canvas/ForgotPasswordPopup/Overlay/Panel` | `CardBackground`, applyToImage=true | `Glow`, applyToOutline=true | Panel popup: fondo + outline neon |
| `Canvas/ForgotPasswordPopup/Overlay/Panel/EmailInputField` | `InputBackground`, applyToImage=true | `Glow`, applyToOutline=true | Input del popup |

#### Notas de escena
- **Google/Apple button backgrounds EXCLUIDOS**: Sus colores están mandados por brand guidelines (Google #131314, Apple black). ThemeApplier violaría los términos de uso de los SDKs sociales.
- **GoogleSignInText / AppleSignInText**: Incluidos con TextPrimary (blanco en NeonDark). ⚠️ Si algún tema usa `textPrimary` ≠ blanco, revisar contra brand guidelines de Google/Apple.
- **Runtime popups**: `UsernamePopup` y `ForgotPasswordPopup` se crean en `LoginManager.Start()` vía `UsernamePopup.Create()` y `ForgotPasswordPopup.Create()`. ThemeApplier debe añadirse en sus métodos `BuildPopup()` / `CreateContent()` vía código, NO via UIBuilder.
- **ForgotPasswordPopup.MessageText EXCLUIDO**: `ShowSuccess` → verde, `ShowError` → rojo. Colores semánticos de respuesta a la operación.
- **EyeToggle BORDERLINE**: UIBuilder asigna `eyeImage.color = CyanNeon`, sugiriendo que `EyeOpen.png` es un white glyph. Verificar en Inspector antes de añadir ThemeApplier(Accent).
- **ErrorPanel prefab**: Se instancia desde `Assets/_Project/Prefabs/Common/ErrorPanel.prefab`. Auditarlo por separado a nivel de prefab.
- **BackButton prefab**: Mismo prefab estándar que Settings. ThemeApplier a nivel de prefab se propaga automáticamente.

#### Objetos ocultos (inactivos en escena)
| Objeto (GameObject path) | Componente | Color hardcoded | ¿Necesita ThemeApplier? | Notas |
|---|---|---|---|---|
| *(pendiente — segunda pasada)* | — | — | — | — |

---

### 05 · `Auth/Register.unity` — 📝 Documentado

**Stats**: 26 objetos evaluados · 24 a tintar (29 componentes ThemeApplier) · 2 NO · 5 duales

#### Objetos a TINTAR
| Objeto (GameObject path) | Componente | ElementType | ThemeData Property | Color NeonDark | Notas |
|---|---|---|---|---|---|
| `Canvas/Background` | Image | `PrimaryBackground` | `primaryBackground` | #050A14 | Reset Image.color DarkNavy→white |
| `Canvas/BackButton` | Image | `ButtonSecondary` | `buttonSecondary` | #202060 | Prefab nivel — misma instancia que Login/Settings |
| `Canvas/BackButton/Arrow` | Image | `Accent` | `primaryAccent` | #00FFFF | Prefab nivel · ⚠️ BORDERLINE: white glyph confirmar |
| `Canvas/RegisterCard` | Image | `CardBackground` | `cardBackground` | #202060 | Reset color→white · DUAL #1 |
| `Canvas/RegisterCard` | Outline | `Glow` | `glowColor` | #00FFFF | Reset effectColor CyanNeon · DUAL #2 |
| `Canvas/RegisterCard/Content/RegisterTitleText` | TextMeshProUGUI | `Accent` | `primaryAccent` | #00FFFF | Reset color CyanNeon→white |
| `Canvas/RegisterCard/Content/UsernameInput` | Image | `InputBackground` | `inputBackground` | #141C38 | Reset color→white · DUAL #1 |
| `Canvas/RegisterCard/Content/UsernameInput` | Outline | `InputBorder` | `inputBorder` | #00FFFF | Reset effectColor CyanNeon · DUAL #2 |
| `Canvas/RegisterCard/Content/UsernameInput/TextArea/UsernameInputPlaceholder` | TextMeshProUGUI | `InputPlaceholder` | `inputPlaceholder` | #B2B2B2 | Reset color TextGray→white |
| `Canvas/RegisterCard/Content/UsernameInput/TextArea/Text` | TextMeshProUGUI | `TextPrimary` | `textPrimary` | #FFFFFF | — |
| `Canvas/RegisterCard/Content/EmailInput` | Image | `InputBackground` | `inputBackground` | #141C38 | Reset color→white · DUAL #1 |
| `Canvas/RegisterCard/Content/EmailInput` | Outline | `InputBorder` | `inputBorder` | #00FFFF | Reset effectColor CyanNeon · DUAL #2 |
| `Canvas/RegisterCard/Content/EmailInput/TextArea/EmailInputPlaceholder` | TextMeshProUGUI | `InputPlaceholder` | `inputPlaceholder` | #B2B2B2 | Reset color TextGray→white |
| `Canvas/RegisterCard/Content/EmailInput/TextArea/Text` | TextMeshProUGUI | `TextPrimary` | `textPrimary` | #FFFFFF | — |
| `Canvas/RegisterCard/Content/PasswordInput` | Image | `InputBackground` | `inputBackground` | #141C38 | Reset color→white · DUAL #1 |
| `Canvas/RegisterCard/Content/PasswordInput` | Outline | `InputBorder` | `inputBorder` | #00FFFF | Reset effectColor CyanNeon · DUAL #2 |
| `Canvas/RegisterCard/Content/PasswordInput/TextArea/PasswordInputPlaceholder` | TextMeshProUGUI | `InputPlaceholder` | `inputPlaceholder` | #B2B2B2 | Reset color TextGray→white |
| `Canvas/RegisterCard/Content/PasswordInput/TextArea/Text` | TextMeshProUGUI | `TextPrimary` | `textPrimary` | #FFFFFF | — |
| `Canvas/RegisterCard/Content/PasswordInput/EyeToggle` | Image | `Accent` | `primaryAccent` | #00FFFF | Reset CyanNeon→white · ⚠️ BORDERLINE: EyeOpen.png confirmar que es white glyph |
| `Canvas/RegisterCard/Content/ConfirmPasswordInput` | Image | `InputBackground` | `inputBackground` | #141C38 | Reset color→white · DUAL #1 |
| `Canvas/RegisterCard/Content/ConfirmPasswordInput` | Outline | `InputBorder` | `inputBorder` | #00FFFF | Reset effectColor CyanNeon · DUAL #2 |
| `Canvas/RegisterCard/Content/ConfirmPasswordInput/TextArea/ConfirmPasswordInputPlaceholder` | TextMeshProUGUI | `InputPlaceholder` | `inputPlaceholder` | #B2B2B2 | Reset color TextGray→white |
| `Canvas/RegisterCard/Content/ConfirmPasswordInput/TextArea/Text` | TextMeshProUGUI | `TextPrimary` | `textPrimary` | #FFFFFF | — |
| `Canvas/RegisterCard/Content/ConfirmPasswordInput/EyeToggle` | Image | `Accent` | `primaryAccent` | #00FFFF | Reset CyanNeon→white · ⚠️ BORDERLINE: EyeOpen.png confirmar white glyph |
| `Canvas/RegisterCard/Content/RegisterButton` | Image | `ButtonPrimary` | `buttonPrimary` | #00FFFF | Reset color CyanNeon→white |
| `Canvas/RegisterCard/Content/RegisterButton/RegisterButtonText` | TextMeshProUGUI | `TextOnPrimary` | `textOnPrimary` | #050A14 | Reset color DarkNavy→white |
| `Canvas/LoadingPanel` | Image | `Overlay` | `overlayColor` | #000000 80% | Reset color (0,0,0,0.7)→white |
| `Canvas/LoadingPanel/Spinner` | Image | `Accent` | `primaryAccent` | #00FFFF | Reset CyanNeon→white · WhiteSprite confirmado |
| `Canvas/LoadingPanel/CreatingAccountText` | TextMeshProUGUI | `TextPrimary` | `textPrimary` | #FFFFFF | — |

#### Objetos que NO se tintan
| Objeto (GameObject path) | Razón |
|---|---|
| `Canvas/Logo` | App brand/logo — `TextMeshProUGUI` con texto "Digit Park", color CyanNeon hardcoded. Nunca tintar logos de marca |
| `Canvas/ErrorPanel` | Prefab `Assets/_Project/Prefabs/Common/ErrorPanel.prefab` — auditar a nivel de prefab por separado |

#### Casos especiales (2 ThemeAppliers en mismo objeto)
| Objeto | ThemeApplier #1 | ThemeApplier #2 | Razón |
|---|---|---|---|
| `Canvas/RegisterCard` | `CardBackground` · applyToImage=true | `Glow` · applyToOutline=true | Image fill + Outline borde de la card |
| `Canvas/RegisterCard/Content/UsernameInput` | `InputBackground` · applyToImage=true | `InputBorder` · applyToOutline=true | Fondo oscuro del input + borde cyan |
| `Canvas/RegisterCard/Content/EmailInput` | `InputBackground` · applyToImage=true | `InputBorder` · applyToOutline=true | Fondo oscuro del input + borde cyan |
| `Canvas/RegisterCard/Content/PasswordInput` | `InputBackground` · applyToImage=true | `InputBorder` · applyToOutline=true | Fondo oscuro del input + borde cyan |
| `Canvas/RegisterCard/Content/ConfirmPasswordInput` | `InputBackground` · applyToImage=true | `InputBorder` · applyToOutline=true | Fondo oscuro del input + borde cyan |

#### Notas de escena
- **Logo NO tintable**: `Canvas/Logo` es TMP "Digit Park" color CyanNeon. Es branding, no UI temática. Mantener hardcoded.
- **EyeToggle BORDERLINE**: `PasswordInput/EyeToggle` y `ConfirmPasswordInput/EyeToggle` usan `EyeOpen.png` coloreado CyanNeon en el builder, lo que sugiere que la sprite es un white glyph. Confirmar en Inspector antes de añadir ThemeApplier(Accent). Si tiene píxeles de color propios, retirar ThemeApplier.
- **RegisterButton color reset necesario**: `RegisterButton` Image.color = CyanNeon (#00FFFF) y `RegisterButtonText` color = DarkNavy (#050A14). Ambos deben resetearse a blanco para que ThemeApplier pueda aplicar `buttonPrimary` y `textOnPrimary` correctamente.
- **ErrorPanel prefab**: Se instancia desde `Assets/_Project/Prefabs/Common/ErrorPanel.prefab`. Auditarlo por separado a nivel de prefab (igual que en Login).
- **BackButton prefab**: Mismo prefab estándar que Login/Settings. ThemeApplier a nivel de prefab se propaga automáticamente.
- **Sin popups runtime**: A diferencia de Login, Register no crea popups en runtime (no UsernamePopup, no ForgotPasswordPopup). Todo es estático de UIBuilder.

#### Objetos ocultos (inactivos en escena)
| Objeto (GameObject path) | Componente | Color hardcoded | ¿Necesita ThemeApplier? | Notas |
|---|---|---|---|---|
| *(pendiente — segunda pasada)* | — | — | — | — |

---

### 06 · `Auth/AgeVerification.unity` — 📝 Documentado

Escena de verificación de edad para CashBattle (KYC 18+). Gold Theme intencional. 20 objetos evaluados.

#### Objetos a TINTAR (12 objetos · 13 componentes ThemeApplier)
| Objeto (GameObject path) | Componente | ElementType | ThemeData Property | Color NeonDark | Notas |
|---|---|---|---|---|---|
| `Canvas/Background` | Image | `PrimaryBackground` | `primaryBackground` | #0A0A1A | Reset Image.color a blanco antes |
| `Canvas/BackButton` | Image + Button | `ButtonSecondary` | `buttonSecondary` | #1A1A2E | Prefab BackButtonGold — añadir en prefab, no en escena |
| `Canvas/VerificationCard` | Image | `CardBackground` | `cardBackground` | #1E1E3A | Reset Image.color a blanco · ver Casos especiales |
| `Canvas/VerificationCard` | Outline | `Glow` | `glowColor` | #00FFFF 80% | Dual (ver Casos especiales) |
| `Canvas/VerificationCard/Content/VerificationIcon` | Image | `AccentTertiary` | `tertiaryAccent` | #FFD700 | ⚠️ BORDERLINE — sprite probablemente blanco tintado en builder con GoldPremium. Verificar que sea pure white glyph; si tiene arte coloreado → mover a NO |
| `Canvas/VerificationCard/Content/AgeVerificationTitle` | TextMeshProUGUI | `Accent` | `primaryAccent` | #00FFFF | Título en cyan = accent primario. Reset color antes |
| `Canvas/VerificationCard/Content/AgeVerificationDesc` | TextMeshProUGUI | `TextPrimary` | `textPrimary` | #FFFFFF | Descripción "Real money competitions…" |
| `Canvas/VerificationCard/Content/VerifyButton` | Image + Button | `ButtonPrimary` | `buttonPrimary` | #00FFFF | Reset Image.color a blanco — actualmente GoldPremium hardcoded |
| `Canvas/VerificationCard/Content/VerifyButton/VerifyAgeButtonText` | TextMeshProUGUI | `TextOnPrimary` | `textOnPrimary` | #0A0A1A | Reset color — actualmente DarkBrown hardcoded |
| `Canvas/VerificationCard/Content/AgeVerificationLegalText` | TextMeshProUGUI | `TextSecondary` | `textSecondary` | #888888 | "Powered by Triumph™…" — texto nuestro, no SDK |
| `Canvas/LoadingIndicator` | Image | `Overlay` | `overlayColor` | #00000080 | Oculto por defecto (SetActive false). ThemeApplier aplica en OnEnable |
| `Canvas/LoadingIndicator/SpinnerContainer` | Image | `Accent` | `primaryAccent` | #00FFFF | Spinner de carga. Reset color a blanco |
| `Canvas/LoadingIndicator/VerifyingText` | TextMeshProUGUI | `TextPrimary` | `textPrimary` | #FFFFFF | "Verifying…" sobre overlay |

#### Objetos que NO se tintan (8 objetos)
| Objeto (GameObject path) | Razón |
|---|---|
| `Canvas/BackButton/Icon` | `BackIconGold.png` — icono con arte en color dorado, NO es pure white glyph → falla regla de tintado |
| `Canvas/Logo` | Texto de marca "Digit Park" — branding/logo, no se tinta aunque sea TMP |
| `Canvas/VerificationCard/Content` | Contenedor VerticalLayoutGroup puro — sin Image ni TMP, solo layout |
| `Canvas/VerificationCard/Content/Spacer` (×4) | Objetos LayoutElement-only sin componente visual (4 spacers: 10f, 30f, 10f, 10f) |
| `Canvas/VerificationCard/Content/AgeVerificationStatusText` | Color controlado EXCLUSIVAMENTE por runtime KYC logic: blanco (idle), dorado (pending), verde (verified), rojo (rejected/error). ThemeApplier colisionaría con Manager.ShowVerificationNeeded/Pending/FullyVerified/Rejected |

#### Casos especiales (2 ThemeAppliers en mismo objeto)
| Objeto | ThemeApplier #1 | ThemeApplier #2 | Razón |
|---|---|---|---|
| `Canvas/VerificationCard` | `CardBackground`, applyToImage=true | `Glow`, applyToOutline=true | Card tiene Image (fondo oscuro) + Outline (borde gold). Necesita dos componentes ThemeApplier separados |

#### Notas de escena
- **BackButtonGold prefab**: Esta escena usa `BackButtonGold.prefab` (diferente al `BackButton.prefab` estándar). Añadir `ButtonSecondary` en la Image del prefab gold. El Icon hijo usa `BackIconGold.png` (oro coloreado) → NO tintable.
- **AgeVerificationStatusText EXCLUIDO**: `AgeVerificationManager.cs` setea `.color` directamente en 4 métodos de estado KYC (líneas 137–186). Si ThemeApplier se añade a este texto, el color se resetearía al cambiar tema y rompería el feedback visual de estado.
- **VerifyButton actualmente gold**: `CreateGoldButton` hardcodea `bg.color = GoldPremium`. Debe resetearse a `Color.white` antes de añadir `ThemeApplier(ButtonPrimary)`. En NeonDark el botón pasará de dorado a cyan — considera si esto afecta la señal visual "premium/cash".
- **VerificationIcon BORDERLINE**: El builder setea `iconImage.color = GoldPremium`, lo que sugiere que el sprite es blanco tintado. Si al inspeccionar el PNG tiene arte coloreado → cambiar a NO.

#### Objetos ocultos (inactivos en escena)
| Objeto (GameObject path) | Componente | Color hardcoded | ¿Necesita ThemeApplier? | Notas |
|---|---|---|---|---|
| *(pendiente — segunda pasada)* | — | — | — | — |

---

### 07 · `Onboarding/Onboarding.unity` — 📝 Auditado V52

#### TABLE 1 — Objetos a TINTAR
| Objeto (GameObject path) | Componente | ElementType | ThemeData Property | Color NeonDark | Notas |
|---|---|---|---|---|---|
| `Background` | Image | PrimaryBackground | `primaryBackground` | #050A14 | — |
| `ProgressBar/Background` | Image | SliderTrack | `sliderTrack` | #191E26 | Reset Image.color a white |
| `ProgressBar/Fill Area/Fill` | Image | SliderFill | `sliderFill` | #00FFFF | Reset Image.color a white |
| `TopBar/TitleLabel` | TextMeshProUGUI | Accent | `primaryAccent` | #00FFFF | "DIGITPARK" label — acento principal |
| `TopBar/StepCounter` | TextMeshProUGUI | TextSecondary | `textSecondary` | #9999A6 | Runtime escribe "1/8" … "8/8" |
| `TopBar/SkipButton/Text` | TextMeshProUGUI | Accent | `primaryAccent` | #00FFFF | Color actual alpha 0.7 — reset a blanco |
| `SlidesContainer/Slide1/IconGlow` | Image | Glow | `glowColor` | rgba(0,255,255,0.06) | Reset Image.color a white; ThemeData.glowColor debe incluir alpha ~0.06 |
| `SlidesContainer/Slide1/SlideTitle` | TextMeshProUGUI | TextTitle | `textTitle` | #00FFFF | — |
| `SlidesContainer/Slide1/ContentCard` | Image | CardBackground | `cardBackground` | #0F1422 | Dual #1 (TABLE 3); reset Image.color |
| `SlidesContainer/Slide1/ContentCard` | Outline | Glow | `glowColor` | #006680 | Dual #2 (TABLE 3) |
| `SlidesContainer/Slide1/ContentCard/Content/Text` (descripción) | TextMeshProUGUI | TextPrimary | `textPrimary` | #F2F2F2 | — |
| `SlidesContainer/Slide1/ContentCard/Content/Text` (highlight "TRAIN…") | TextMeshProUGUI | Accent | `primaryAccent` | #00FFFF | Texto de énfasis en mayúsculas |
| `SlidesContainer/Slide1/ContentCard/Content/Text` (bullet 1) | TextMeshProUGUI | TextPrimary | `textPrimary` | #F2F2F2 | — |
| `SlidesContainer/Slide1/ContentCard/Content/Text` (bullet 2) | TextMeshProUGUI | TextPrimary | `textPrimary` | #F2F2F2 | — |
| `SlidesContainer/Slide1/ContentCard/Content/Text` (bullet 3) | TextMeshProUGUI | TextPrimary | `textPrimary` | #F2F2F2 | — |
| `SlidesContainer/Slide2/IconGlow` | Image | Glow | `glowColor` | rgba(0,255,255,0.06) | Oculto vía Slide2 SetActive false; mismo patrón que Slide1/IconGlow |
| `SlidesContainer/Slide2/SlideIcon` | Image | Accent | `primaryAccent` | #00FFFF | ⚠️ BORDERLINE — ProfileIcon.png, silueta blanca de persona; necesita revisión del diseñador |
| `SlidesContainer/Slide2/SlideTitle` | TextMeshProUGUI | TextTitle | `textTitle` | #00FFFF | Oculto vía Slide2 |
| `SlidesContainer/Slide2/NameInputPanel` | Image | CardBackground | `cardBackground` | #0F1422 | Oculto vía Slide2; dual #1 (TABLE 3) |
| `SlidesContainer/Slide2/NameInputPanel` | Outline | Glow | `glowColor` | #006680 | Oculto vía Slide2; dual #2 |
| `SlidesContainer/Slide2/NameInputPanel/InputContainer/NameInput` | Image | InputBackground | `inputBackground` | #141923 | Oculto vía Slide2; dual #1 |
| `SlidesContainer/Slide2/NameInputPanel/InputContainer/NameInput` | Outline | InputBorder | `inputBorder` | #006680 | Oculto vía Slide2; dual #2 |
| `SlidesContainer/Slide2/NameInputPanel/InputContainer/NameInput/Text Area/Placeholder` | TextMeshProUGUI | InputPlaceholder | `inputPlaceholder` | #666673 | Oculto vía Slide2 |
| `SlidesContainer/Slide2/NameInputPanel/InputContainer/NameInput/Text Area/Text` | TextMeshProUGUI | TextPrimary | `textPrimary` | #F2F2F2 | Oculto vía Slide2; texto escrito por el usuario |
| `SlidesContainer/Slide2/NameInputPanel/InputContainer/ConfirmNameButton` | Image | ButtonPrimary | `buttonPrimary` | #00FFFF | Oculto vía Slide2; dual #1 |
| `SlidesContainer/Slide2/NameInputPanel/InputContainer/ConfirmNameButton` | Outline | ButtonGlowPrimary | `glowColor` | #006680 | Oculto vía Slide2; dual #2 |
| `SlidesContainer/Slide2/NameInputPanel/InputContainer/ConfirmNameButton/Text` | TextMeshProUGUI | TextOnPrimary | `textOnPrimary` | #050D14 | Oculto vía Slide2 |
| `SlidesContainer/Slide3/IconGlow` | Image | Glow | `glowColor` | rgba(0,255,255,0.06) | Oculto vía Slide3 |
| `SlidesContainer/Slide3/SlideIcon` | Image | Accent | `primaryAccent` | #00FFFF | ⚠️ BORDERLINE — AvatarDefault.png, silueta blanca; necesita revisión del diseñador |
| `SlidesContainer/Slide3/SlideTitle` | TextMeshProUGUI | TextTitle | `textTitle` | #00FFFF | Oculto vía Slide3 |
| `SlidesContainer/Slide4/IconGlow` | Image | Glow | `glowColor` | rgba(0,255,255,0.06) | Oculto vía Slide4 |
| `SlidesContainer/Slide4/SlideTitle` | TextMeshProUGUI | TextTitle | `textTitle` | #00FFFF | Oculto vía Slide4 |
| `SlidesContainer/Slide4/ContentCard` | Image | CardBackground | `cardBackground` | #0F1422 | Oculto vía Slide4; dual #1 |
| `SlidesContainer/Slide4/ContentCard` | Outline | Glow | `glowColor` | #006680 | Oculto vía Slide4; dual #2 |
| `SlidesContainer/Slide4/ContentCard/Content/Text` (desc) | TextMeshProUGUI | TextPrimary | `textPrimary` | #F2F2F2 | Oculto vía Slide4 |
| `SlidesContainer/Slide4/ContentCard/Content/Text` (highlight "6 UNIQUE…") | TextMeshProUGUI | Accent | `primaryAccent` | #00FFFF | Oculto vía Slide4 |
| `SlidesContainer/Slide4/ContentCard/Content/Text` (bullets ×5) | TextMeshProUGUI | TextPrimary | `textPrimary` | #F2F2F2 | Oculto vía Slide4; patrón ×5 |
| `SlidesContainer/Slide5/IconGlow` | Image | Glow | `glowColor` | rgba(0,255,255,0.06) | Oculto vía Slide5 |
| `SlidesContainer/Slide5/SlideIcon` | Image | Accent | `primaryAccent` | #00FFFF | CashBattleIcon.png — logo blanco confirmado en screenshots (V angular + rayos) |
| `SlidesContainer/Slide5/SlideTitle` | TextMeshProUGUI | TextTitle | `textTitle` | #00FFFF | Oculto vía Slide5 |
| `SlidesContainer/Slide5/ContentCard` | Image | CardBackground | `cardBackground` | #0F1422 | Oculto vía Slide5; dual #1 |
| `SlidesContainer/Slide5/ContentCard` | Outline | Glow | `glowColor` | #006680 | Oculto vía Slide5; dual #2 |
| `SlidesContainer/Slide5/ContentCard/Content/Text` (desc) | TextMeshProUGUI | TextPrimary | `textPrimary` | #F2F2F2 | Oculto vía Slide5 |
| `SlidesContainer/Slide5/ContentCard/Content/Text` (highlight "COMPETE…") | TextMeshProUGUI | Accent | `primaryAccent` | #00FFFF | Oculto vía Slide5 |
| `SlidesContainer/Slide5/ContentCard/Content/Text` (bullets ×3) | TextMeshProUGUI | TextPrimary | `textPrimary` | #F2F2F2 | Oculto vía Slide5; patrón ×3 |
| `SlidesContainer/Slide6/IconGlow` | Image | Glow | `glowColor` | rgba(0,255,255,0.06) | Oculto vía Slide6 |
| `SlidesContainer/Slide6/SlideTitle` | TextMeshProUGUI | TextTitle | `textTitle` | #00FFFF | Oculto vía Slide6 |
| `SlidesContainer/Slide6/ContentCard` | Image | CardBackground | `cardBackground` | #0F1422 | Oculto vía Slide6; dual #1 |
| `SlidesContainer/Slide6/ContentCard` | Outline | Glow | `glowColor` | #006680 | Oculto vía Slide6; dual #2 |
| `SlidesContainer/Slide6/ContentCard/Content/Text` (desc) | TextMeshProUGUI | TextPrimary | `textPrimary` | #F2F2F2 | Oculto vía Slide6 |
| `SlidesContainer/Slide6/ContentCard/Content/Text` (highlight "WIN BIG…") | TextMeshProUGUI | Accent | `primaryAccent` | #00FFFF | Oculto vía Slide6 |
| `SlidesContainer/Slide6/ContentCard/Content/Text` (bullets ×3) | TextMeshProUGUI | TextPrimary | `textPrimary` | #F2F2F2 | Oculto vía Slide6; patrón ×3 |
| `SlidesContainer/Slide7/IconGlow` | Image | Glow | `glowColor` | rgba(0,255,255,0.06) | Oculto vía Slide7 |
| `SlidesContainer/Slide7/SlideTitle` | TextMeshProUGUI | TextTitle | `textTitle` | #00FFFF | Oculto vía Slide7 |
| `SlidesContainer/Slide7/ContentCard` | Image | CardBackground | `cardBackground` | #0F1422 | Oculto vía Slide7; dual #1 |
| `SlidesContainer/Slide7/ContentCard` | Outline | Glow | `glowColor` | #006680 | Oculto vía Slide7; dual #2 |
| `SlidesContainer/Slide7/ContentCard/Content/Text` (desc) | TextMeshProUGUI | TextPrimary | `textPrimary` | #F2F2F2 | Oculto vía Slide7 |
| `SlidesContainer/Slide7/ContentCard/Content/Text` (highlight "REWARDS…") | TextMeshProUGUI | Accent | `primaryAccent` | #00FFFF | Oculto vía Slide7 |
| `SlidesContainer/Slide7/ContentCard/Content/Text` (bullets ×3) | TextMeshProUGUI | TextPrimary | `textPrimary` | #F2F2F2 | Oculto vía Slide7; patrón ×3 |
| `SlidesContainer/Slide8/CompletionPanel/CompletionTitle` | TextMeshProUGUI | TextTitle | `textTitle` | #00FFFF | Oculto vía Slide8 |
| `SlidesContainer/Slide8/CompletionPanel/CompletionMessage` | TextMeshProUGUI | TextPrimary | `textPrimary` | #F2F2F2 | Oculto vía Slide8 |
| `SlidesContainer/Slide8/CompletionPanel/RewardsCard` | Image | CardBackground | `cardBackground` | #0F1422 | Oculto vía Slide8; dual #1 |
| `SlidesContainer/Slide8/CompletionPanel/RewardsCard` | Outline | AccentTertiary | `tertiaryAccent` | #FFD700 | Oculto vía Slide8; dual #2 — borde dorado semántico de recompensa |
| `SlidesContainer/Slide8/CompletionPanel/RewardsCard/RewardText` | TextMeshProUGUI | AccentTertiary | `tertiaryAccent` | #FFD700 | Oculto vía Slide8; "+500 DigitCoins \| +50 DigitGems" |
| `SlidesContainer/Slide8/CompletionPanel/StartPlayingButton` | Image | ButtonSuccess | `buttonSuccess` | #33E666 | Oculto vía Slide8; dual #1 |
| `SlidesContainer/Slide8/CompletionPanel/StartPlayingButton` | Outline | ButtonGlowSuccess | `successColor` | #1A7A33 | Oculto vía Slide8; dual #2 |
| `SlidesContainer/Slide8/CompletionPanel/StartPlayingButton/Text` | TextMeshProUGUI | TextOnSuccess | `textOnPrimary` | #050D14 | Oculto vía Slide8 |
| `NavigationPanel/PrevButton` | Image | ButtonSecondary | `buttonSecondary` | #141D2B | Dual #1 (TABLE 3) |
| `NavigationPanel/PrevButton` | Outline | Glow | `glowColor` | rgba(77,77,89,0.5) | Dual #2 — borde sutil |
| `NavigationPanel/PrevButton/Text` | TextMeshProUGUI | TextSecondary | `textSecondary` | #9999A6 | "BACK" |
| `NavigationPanel/NextButton` | Image | ButtonPrimary | `buttonPrimary` | #00FFFF | Dual #1 |
| `NavigationPanel/NextButton` | Outline | ButtonGlowPrimary | `glowColor` | #006680 | Dual #2 |
| `NavigationPanel/NextButton/Text` | TextMeshProUGUI | TextOnPrimary | `textOnPrimary` | #050D14 | Runtime cambia texto (NEXT/CONFIRM/SELECT/BEGIN) |

#### TABLE 2 — Objetos que NO se tintan
| Objeto (GameObject path) | Razón |
|---|---|
| `ProgressBar` (root) | Slider root con Image.color=clear (transparente) — solo sirve como targetGraphic del Slider |
| `ProgressBar/Fill Area` | RectTransform container — sin Image |
| `TopBar` | RectTransform container — sin Image de fondo |
| `TopBar/SkipButton` | Image casi transparente (alpha 0.05) — botón solo-texto; ThemeApplier añadiría fondo visible no deseado |
| `SlidesContainer` | RectTransform, sin Image |
| `SlidesContainer/Slide1` … `Slide8` (contenedores) | RectTransform, sin Image |
| `SlidesContainer/Slide1/SlideIcon` | WelcomeIcon.png — ilustración de cerebro coloreado (multi-color art) |
| `SlidesContainer/Slide4/SlideIcon` | GamesIcon.png — ilustración de cartas de juego coloridas (multi-color art) |
| `SlidesContainer/Slide6/SlideIcon` | TournamentsIcon.png — trofeo dorado ilustración (multi-color art) |
| `SlidesContainer/Slide7/SlideIcon` | RewardsIcon.png — medalla colorida ilustración (multi-color art) |
| `SlidesContainer/Slide8/CompletionPanel/CompletionIcon` | CompleteIcon.png — icono de celebración colorido (multi-color art) |
| `SlidesContainer/Slide1/ContentCard/Content` (×5 slides info) | VLG container — sin Image |
| `SlidesContainer/Slide2/NameInputPanel/InputContainer` | VLG container — sin Image |
| `SlidesContainer/Slide2/NameInputPanel/InputContainer/NameInput/Text Area` | RectMask2D container — sin Image |
| `SlidesContainer/Slide2/NameInputPanel/InputContainer/NameErrorText` | TMP — color gestionado en runtime: RED_ERROR (error) / CYAN (hint de confirmación); no temático |
| `SlidesContainer/Slide3/AvatarSelectionPanel` | RectTransform — sin Image |
| `SlidesContainer/Slide3/AvatarSelectionPanel/AvatarContainer` | GridLayoutGroup container — sin Image |
| `Avatar_avatar_0N` Image (×6, runtime) | Color gestionado por `OnAvatarSelected()`: selected=cyan-blue, unselected=dark gray — estado de selección |
| `Avatar_avatar_0N/AvatarImage` (×6, runtime) | Arte de avatar del jugador o placeholder gris |
| `Avatar_avatar_0N/Name` TMP (×6, runtime) | Runtime-creados en `CreateAvatarOptionFallback()`; ThemeApplier debe añadirse via código en Manager |
| `DotsContainer` | HLG container — sin Image |
| `Dot_0` … `Dot_7` (×8, runtime) | Image — color gestionado por `UpdateNavigationDots()`: active=cyan, past=dark cyan, inactive=gray — estado runtime |
| `NavigationPanel` | RectTransform — sin Image de fondo |
| `SlidesContainer/Slide8/CompletionPanel` | VLG container — sin Image |
| `SlidesContainer/Slide8/CompletionPanel/Spacer` | LayoutElement only — sin Image |
| Spacer objects en ContentCard/Content (×10) | LayoutElement only — sin Image |

#### TABLE 3 — Casos especiales (2 ThemeAppliers en mismo objeto)
| Objeto | ThemeApplier #1 | ThemeApplier #2 | Razón |
|---|---|---|---|
| `SlidesContainer/Slide1/ContentCard` | Image → CardBackground | Outline → Glow | Card con borde neon |
| `SlidesContainer/Slide2/NameInputPanel` | Image → CardBackground | Outline → Glow | Panel de input con borde neon |
| `SlidesContainer/Slide2/.../NameInput` | Image → InputBackground | Outline → InputBorder | Input field con borde |
| `SlidesContainer/Slide2/.../ConfirmNameButton` | Image → ButtonPrimary | Outline → ButtonGlowPrimary | Botón primario con glow |
| `SlidesContainer/Slide4/ContentCard` | Image → CardBackground | Outline → Glow | Card con borde neon |
| `SlidesContainer/Slide5/ContentCard` | Image → CardBackground | Outline → Glow | Card con borde neon |
| `SlidesContainer/Slide6/ContentCard` | Image → CardBackground | Outline → Glow | Card con borde neon |
| `SlidesContainer/Slide7/ContentCard` | Image → CardBackground | Outline → Glow | Card con borde neon |
| `SlidesContainer/Slide8/.../RewardsCard` | Image → CardBackground | Outline → AccentTertiary | Borde dorado semántico de recompensa |
| `SlidesContainer/Slide8/.../StartPlayingButton` | Image → ButtonSuccess | Outline → ButtonGlowSuccess | Botón éxito con glow verde |
| `NavigationPanel/PrevButton` | Image → ButtonSecondary | Outline → Glow | Botón back con borde sutil |
| `NavigationPanel/NextButton` | Image → ButtonPrimary | Outline → ButtonGlowPrimary | Botón primario con glow |

#### TABLE 4 — Ocultos (SetActive false en Start — slides 2–8 via SetupUI())
| Objeto | ¿ThemeApplier? | Razón |
|---|---|---|
| `Slide2` y todos sus hijos | Ver TABLE 1/2 por hijo | SetActive(false) en `SetupUI()` hasta que usuario llegue al paso 2 |
| `Slide3` y todos sus hijos | Ver TABLE 1/2 por hijo | SetActive(false) hasta paso 3 |
| `Slide4` y todos sus hijos | Ver TABLE 1/2 por hijo | SetActive(false) hasta paso 4 |
| `Slide5` y todos sus hijos | Ver TABLE 1/2 por hijo | SetActive(false) hasta paso 5 |
| `Slide6` y todos sus hijos | Ver TABLE 1/2 por hijo | SetActive(false) hasta paso 6 |
| `Slide7` y todos sus hijos | Ver TABLE 1/2 por hijo | SetActive(false) hasta paso 7 |
| `Slide8` y todos sus hijos | Ver TABLE 1/2 por hijo | SetActive(false) hasta paso 8 |

#### Notas de escena
- **Sin runtime color-overrides en UI estructural**: `OnboardingManager.Start()` no sobreescribe colores de Image/TMP estáticos → ThemeApplier seguro en todos los objetos de TABLE 1.
- **NameErrorText dual-state**: `ShowNameError()` → RED_ERROR; `OnConfirmName()` hint → CYAN. Color semántico de estado → TABLE 2. No añadir ThemeApplier.
- **Navigation dots runtime**: `UpdateNavigationDots()` usa colores hardcoded (cyan activo, dark cyan pasado, gray futuro). Para theming completo: usar `ThemeManager.current.primaryAccent` para el dot activo en lugar del hardcoded `new Color(0f, 0.83f, 1f)`.
- **Avatar options runtime**: `OnAvatarSelected()` sobreescribe `Image.color` con cyan/dark según estado de selección → TABLE 2. ThemeApplier via código en `CreateAvatarOptionFallback()` para consistencia temática.
- **IconGlow alpha 0.06**: Image.color = CYAN_GLOW (0,1,1,0.06). Para ThemeApplier(Glow): resetear Image.color a white primero. El `ThemeData.glowColor` debe incluir el alpha apropiado (~0.06) para mantener el efecto sutil.
- **SkipButton Image**: casi transparente (alpha 0.05) — NO ThemeApplier. Solo el Text child recibe Accent.
- **SlideIcon Slide5 (CashBattle)**: logo blanco plano confirmado en screenshots → YES Accent.
- **SlideIcons Slides 2+3** (ProfileIcon/AvatarDefault): siluetas blancas → BORDERLINE YES Accent — requiere confirmación del diseñador de que el tintado cyan es deseable.
- **PrevButton** oculto en Slide 1 (`prevButton.gameObject.SetActive(false)` al inicio) — ThemeApplier igual aplicado ya que el GO existe y se activa en siguientes slides.

---

### 08 · `Onboarding/CashBattleOnboarding.unity` — 🚫 Excluida

> **Zona CashBattle** — paleta gold estática, sin ThemeApplier.

---

### 09 · `Games/Navigation/GameSelector.unity` — 📝 Auditado

#### TABLE 1 — Objetos a TINTAR (YES ThemeApplier)
| Objeto (GameObject path) | Componente | ElementType | ThemeData Property | Color NeonDark | Notas |
|---|---|---|---|---|---|
| Canvas/Background | Image | `PrimaryBackground` | primaryBackground | #050810 | fondo principal de escena |
| Canvas/Header/GameSelectorTitleText | TextMeshPro | `Accent` | primaryAccent | #00FFFF | "SELECT A GAME" — cyan title |
| Canvas/CognitiveSprintPanel | Image | `Overlay` | overlayColor | #000000E6 | fondo bloqueador 90% negro, oculto por defecto |
| Canvas/CognitiveSprintPanel/InnerPanel | Image | `CardBackground` | cardBackground | #0D1A26 | panel interior modal — DUAL con Outline |
| Canvas/CognitiveSprintPanel/InnerPanel | Outline | `Glow` | glowColor | #00FFFF | borde cyan del modal |
| Canvas/CognitiveSprintPanel/InnerPanel/PanelTitle | TextMeshPro | `Accent` | primaryAccent | #00FFFF | "Cognitive Sprint" — cyan title |
| Canvas/CognitiveSprintPanel/InnerPanel/CognitiveSprintDescText | TextMeshPro | `TextSecondary` | textSecondary | #8899AA | "Select 3-5 games for the sprint" |
| Canvas/.../TogglesContainer/Toggle_DigitRush | Image | `CardBackground` | cardBackground | #141E2E | fondo de fila toggle — aplica a Toggle_DigitRush/MemoryPairs/QuickMath/FlashTap/OddOneOut (×5) |
| Canvas/.../TogglesContainer/Toggle_DigitRush | Outline | `Glow` | glowColor | #005555 | borde cyan oscuro — DUAL; aplica ×5 |
| Canvas/.../Toggle_DigitRush/Background | Image | `SecondaryBackground` | secondaryBackground | #050810 | fondo del checkbox — aplica ×5 |
| Canvas/.../Toggle_DigitRush/Background/Checkmark | Image | `ToggleCheckmark` | toggleCheckmark | #00FFFF | checkmark cyan — aplica ×5 |
| Canvas/.../Toggle_DigitRush/Label | TextMeshPro | `TextPrimary` | textPrimary | #FFFFFF | nombre del juego — aplica ×5 |
| Canvas/CognitiveSprintPanel/InnerPanel/ButtonsContainer/CancelSprintButton | Image | `ButtonSecondary` | buttonSecondary | #404040 | botón gris cancelar — DUAL |
| Canvas/CognitiveSprintPanel/InnerPanel/ButtonsContainer/CancelSprintButton | Outline | `Glow` | glowColor | #005555 | borde sutil cancelar |
| Canvas/CognitiveSprintPanel/InnerPanel/ButtonsContainer/CancelSprintButton/Text | TextMeshPro | `TextPrimary` | textPrimary | #FFFFFF | "Cancel" |
| Canvas/CognitiveSprintPanel/InnerPanel/ButtonsContainer/StartSprintButton | Image | `ButtonPrimary` | buttonPrimary | #00FFFF | botón primario cyan — DUAL |
| Canvas/CognitiveSprintPanel/InnerPanel/ButtonsContainer/StartSprintButton | Outline | `ButtonGlowPrimary` | glowColor | #00FFFF80 | glow cyan botón primario |
| Canvas/CognitiveSprintPanel/InnerPanel/ButtonsContainer/StartSprintButton/Text | TextMeshPro | `TextOnPrimary` | textOnPrimary | #050810 | "Start Sprint" — texto oscuro sobre cyan |
| Canvas/RulesPanel/InnerPanel | Image | `CardBackground` | cardBackground | #0D1A26 | ⚠️ PENDIENTE — RulesPanel será reemplazado por InfoButton (aún no diseñado); se audita la estructura actual como referencia |
| Canvas/RulesPanel/InnerPanel | Outline | `Glow` | glowColor | #00FFFF | borde cyan del modal de reglas (pendiente rediseño) |
| Canvas/RulesPanel/InnerPanel/GameTitle | TextMeshPro | `Accent` | primaryAccent | #00FFFF | título del juego (dinámico por código) — pendiente rediseño |
| Canvas/RulesPanel/InnerPanel/RulesDescText | TextMeshPro | `TextSecondary` | textSecondary | #8899AA | "Game Rules" |
| Canvas/RulesPanel/InnerPanel/RulesContainer/RulesText | TextMeshPro | `TextPrimary` | textPrimary | #E6E6E6 | contenido de reglas dinámico |
| Canvas/RulesPanel/InnerPanel/CheckboxContainer/DontShowToggle | Image | `CardBackground` | cardBackground | #1A2633 | fondo del checkbox pequeño — DUAL |
| Canvas/RulesPanel/InnerPanel/CheckboxContainer/DontShowToggle | Outline | `Glow` | glowColor | #005555 | borde sutil del checkbox |
| Canvas/RulesPanel/InnerPanel/CheckboxContainer/DontShowToggle/Checkmark | Image | `ToggleCheckmark` | toggleCheckmark | #00FFFF | checkmark cyan |
| Canvas/RulesPanel/InnerPanel/CheckboxContainer/ToggleLabel | TextMeshPro | `TextSecondary` | textSecondary | #B3B3B3 | "Don't show these rules again" |
| Canvas/RulesPanel/InnerPanel/ButtonsContainer/CancelButton | Image | `ButtonSecondary` | buttonSecondary | #404040 | botón cancelar reglas — DUAL |
| Canvas/RulesPanel/InnerPanel/ButtonsContainer/CancelButton | Outline | `Glow` | glowColor | #005555 | borde sutil cancelar |
| Canvas/RulesPanel/InnerPanel/ButtonsContainer/CancelButton/Text | TextMeshPro | `TextPrimary` | textPrimary | #FFFFFF | "Cancel" |
| Canvas/RulesPanel/InnerPanel/ButtonsContainer/PlayButton | Image | `ButtonPrimary` | buttonPrimary | #00FFFF | botón play cyan — DUAL |
| Canvas/RulesPanel/InnerPanel/ButtonsContainer/PlayButton | Outline | `ButtonGlowPrimary` | glowColor | #00FFFF80 | glow cyan play |
| Canvas/RulesPanel/InnerPanel/ButtonsContainer/PlayButton/Text | TextMeshPro | `TextOnPrimary` | textOnPrimary | #050810 | "Play!" — texto oscuro sobre cyan |
| Canvas/RulesPanel | Image | `Overlay` | overlayColor | #000000EB | fondo bloqueador 92% negro, oculto por defecto |

#### TABLE 2 — Objetos que NO se tintan
| Objeto (GameObject path) | Razón |
|---|---|
| Canvas/Header | Contenedor sin Image — solo RectTransform, sin componente visual |
| Canvas/Header/CurrencyPills (+ sub-objetos: CoinsPill, GemsPill, CoinsIcon, GemsIcon, CoinsValueText, GemsValueText, CoinsAddButton, GemsAddButton) | Currency pills — objetos estáticos, no se modifican con ThemeApplier (decisión de diseño) |
| Canvas/GamesPanel | GridLayoutGroup sin Image — contenedor estructural |
| Canvas/GamesPanel/DigitRushButton | Image.color=white para mostrar arte ilustrado; UIBuilder elimina ThemeApplier explícitamente (línea 207-212) |
| Canvas/GamesPanel/DigitRushButton/Outline ×2 | Glow fijo CYAN_NEON — parte del arte del game card, no tema |
| Canvas/GamesPanel/MemoryPairsButton | Ídem DigitRushButton — arte ilustrado |
| Canvas/GamesPanel/MemoryPairsButton/Outline ×2 | Glow fijo CYAN_NEON — parte del arte |
| Canvas/GamesPanel/QuickMathButton | Ídem — arte ilustrado |
| Canvas/GamesPanel/QuickMathButton/Outline ×2 | Glow fijo CYAN_NEON |
| Canvas/GamesPanel/FlashTapButton | Ídem — arte ilustrado |
| Canvas/GamesPanel/FlashTapButton/Outline ×2 | Glow fijo CYAN_NEON |
| Canvas/GamesPanel/OddOneOutButton | Ídem — arte ilustrado |
| Canvas/GamesPanel/OddOneOutButton/Outline ×2 | Glow fijo CYAN_NEON |
| Canvas/GamesPanel/CognitiveSprintButton | Ídem — arte ilustrado; outline GOLD (isGold=true), fijo |
| Canvas/GamesPanel/CognitiveSprintButton/Outline ×2 | Glow fijo GOLD — parte del arte del card especial |
| Canvas/CognitiveSprintPanel/InnerPanel/SelectedCountText | `UpdateSprintUI()` sobreescribe color en runtime: verde cuando count≥MIN_GAMES, blanco en caso contrario — color semántico de validación |
| Canvas/BackButton | Prefab compartido — ThemeApplier configurado en el prefab |
| Canvas/---ANIMATION_MANAGERS--- | Sin componentes visuales de UI |

#### TABLE 3 — Casos especiales (2 ThemeAppliers en mismo objeto)
| Objeto | ThemeApplier #1 | ThemeApplier #2 | Razón |
|---|---|---|---|
| Canvas/CognitiveSprintPanel/InnerPanel | Image → `CardBackground` (applyToImage) | Outline → `Glow` (applyToOutline) | Panel modal con borde cyan |
| Canvas/.../Toggle_DigitRush | Image → `CardBackground` (applyToImage) | Outline → `Glow` (applyToOutline) | Fila toggle con borde sutil — patrón ×5 |
| Canvas/.../CancelSprintButton | Image → `ButtonSecondary` (applyToImage) | Outline → `Glow` (applyToOutline) | Botón cancelar con borde |
| Canvas/.../StartSprintButton | Image → `ButtonPrimary` (applyToImage) | Outline → `ButtonGlowPrimary` (applyToOutline) | Botón primario con glow cyan |
| Canvas/RulesPanel/InnerPanel | Image → `CardBackground` (applyToImage) | Outline → `Glow` (applyToOutline) | Panel reglas — pendiente rediseño InfoButton |
| Canvas/.../DontShowToggle | Image → `CardBackground` (applyToImage) | Outline → `Glow` (applyToOutline) | Checkbox "no volver a mostrar" |
| Canvas/.../CancelButton (RulesPanel) | Image → `ButtonSecondary` (applyToImage) | Outline → `Glow` (applyToOutline) | Botón cancelar reglas |
| Canvas/.../PlayButton (RulesPanel) | Image → `ButtonPrimary` (applyToImage) | Outline → `ButtonGlowPrimary` (applyToOutline) | Botón play con glow cyan |

#### TABLE 4 — Objetos ocultos/inactivos en Awake
| Objeto (GameObject path) | Estado inicial | Cuándo se activa | ThemeApplier igual que activo |
|---|---|---|---|
| Canvas/CognitiveSprintPanel | `SetActive(false)` en `SetupCognitiveSprintPanel()` | Al presionar CognitiveSprintButton | ✅ Sí — ThemeApplier funciona aunque esté oculto |
| Canvas/RulesPanel | `SetActive(false)` en `SetupRulesPanel()` | Al presionar un game card (si ShouldShowRules=true) | ✅ Sí — pendiente rediseño InfoButton |

#### Notas de escena
- **Game cards — NO ThemeApplier**: Los 6 botones de juego (DigitRushButton, MemoryPairsButton, QuickMathButton, FlashTapButton, OddOneOutButton, CognitiveSprintButton) muestran arte ilustrado con `Image.color = Color.white`. El UIBuilder elimina ThemeApplier explícitamente en `CreateGameCard()` (líneas 207-212): *"Limpiar ThemeApplier (causa tinte azul al aplicar cardBackground del tema)"*. Sus Outlines son CYAN_NEON fijo (GOLD para CognitiveSprint). Nunca añadir ThemeApplier a estos objetos.
- **RulesPanel — PENDIENTE REDISEÑO**: El panel de reglas actual (mostrado al presionar un card) será reemplazado por un botón de info (InfoButton) en una futura iteración. La estructura actual se audita como referencia, pero no se implementa ThemeApplier hasta confirmar el nuevo diseño.
- **SelectedCountText — color semántico**: `UpdateSprintUI()` asigna `Color.green` cuando la selección es válida (≥3 juegos) y `Color.white` en caso contrario. Color controlado 100% por lógica de validación → NO ThemeApplier.
- **CurrencyPills — estáticos**: Todos los objetos generados por `CurrencyHeaderBarHelper` son estáticos y no se modifican con ThemeApplier en ninguna escena.
- **Header sin fondo**: A diferencia de BetSelection, el Header del GameSelector no tiene Image component — es un contenedor puro. Solo el TitleText recibe ThemeApplier.

---

### 10 · `Games/Navigation/PlayModeSelection.unity` — 📝 Auditado

#### TABLE 1 — Objetos a TINTAR (ThemeApplier = YES)
| Objeto (GameObject path) | Componente | ElementType | ThemeData Property | Color NeonDark | Notas |
|---|---|---|---|---|---|
| Canvas/Background | Image | PrimaryBackground | primaryBackground | #050A14 | Reset Image color a blanco primero |
| Canvas/SafeArea/Header/TitleText | TextMeshProUGUI | TextTitle | textTitle | #FFFFFF | "SELECT MODE" — hardcodeado CYAN_NEON, reset a blanco (dual #1) |
| Canvas/SafeArea/TitleSection/SubtitleText | TextMeshProUGUI | TextSecondary | textSecondary | #99A6B2 | "Choose how you want to play" |
| Canvas/SafeArea/CardsSection/SoloCard/Side | Image | Accent | primaryAccent | #00FFFF @30% | Borde inferior de profundidad 3D de la card — derivado de accentColor×0.3 |
| Canvas/SafeArea/CardsSection/SoloCard/Face | Image | CardBackground | cardBackground | #0A1420 | Cara principal de la card (dual #1) |
| Canvas/SafeArea/CardsSection/SoloCard/Face/IconContainer/Icon | Image | Accent | primaryAccent | #00FFFF | ⚠️ BORDERLINE — UIBuilder L298 comenta "WHITE color for ThemeApplier tinting"; Image.color=white; verificar que sprite sea glifo blanco puro |
| Canvas/SafeArea/CardsSection/SoloCard/Face/Arrow | TextMeshProUGUI | Accent | primaryAccent | #00FFFF | Indicador ">" — texto cyan acento |
| Canvas/SafeArea/CardsSection/SoloCard/Face/TitleText | TextMeshProUGUI | Accent | primaryAccent | #00FFFF | "SOLO" — título H2 en color acento |
| Canvas/SafeArea/CardsSection/SoloCard/Face/DescText | TextMeshProUGUI | TextSecondary | textSecondary | #99A6B2 | Descripción del modo |
| Canvas/SafeArea/CardsSection/OneVsOneCard/Side | Image | Accent | primaryAccent | #00FFFF @30% | Borde inferior de profundidad 3D |
| Canvas/SafeArea/CardsSection/OneVsOneCard/Face | Image | CardBackground | cardBackground | #0A1420 | Cara principal de la card (dual #1) |
| Canvas/SafeArea/CardsSection/OneVsOneCard/Face/IconContainer/Icon | Image | Accent | primaryAccent | #00FFFF | ⚠️ BORDERLINE — verificar sprite blanco puro |
| Canvas/SafeArea/CardsSection/OneVsOneCard/Face/Arrow | TextMeshProUGUI | Accent | primaryAccent | #00FFFF | Indicador ">" |
| Canvas/SafeArea/CardsSection/OneVsOneCard/Face/TitleText | TextMeshProUGUI | Accent | primaryAccent | #00FFFF | "1 VS 1" |
| Canvas/SafeArea/CardsSection/OneVsOneCard/Face/DescText | TextMeshProUGUI | TextSecondary | textSecondary | #99A6B2 | Descripción del modo |
| Canvas/SafeArea/CardsSection/TournamentsCard/Side | Image | Accent | primaryAccent | #00FFFF @30% | Borde inferior de profundidad 3D |
| Canvas/SafeArea/CardsSection/TournamentsCard/Face | Image | CardBackground | cardBackground | #0A1420 | Cara principal de la card (dual #1) |
| Canvas/SafeArea/CardsSection/TournamentsCard/Face/IconContainer/Icon | Image | Accent | primaryAccent | #00FFFF | ⚠️ BORDERLINE — verificar sprite blanco puro |
| Canvas/SafeArea/CardsSection/TournamentsCard/Face/Arrow | TextMeshProUGUI | Accent | primaryAccent | #00FFFF | Indicador ">" |
| Canvas/SafeArea/CardsSection/TournamentsCard/Face/TitleText | TextMeshProUGUI | Accent | primaryAccent | #00FFFF | "TOURNAMENTS" |
| Canvas/SafeArea/CardsSection/TournamentsCard/Face/DescText | TextMeshProUGUI | TextSecondary | textSecondary | #99A6B2 | Descripción del modo |

#### TABLE 2 — Objetos que NO se tintan
| Objeto (GameObject path) | Razón |
|---|---|
| Canvas/SafeArea | RectTransform container — no Image |
| Canvas/SafeArea/Header | RectTransform container — no Image |
| Canvas/SafeArea/Header/BackButton | Prefab compartido — ThemeApplier configurado dentro del prefab, no por escena |
| Canvas/SafeArea/Header/CurrencyPills | Currency pills — objetos estáticos, no se modifican con ThemeApplier (decisión de diseño) |
| Canvas/SafeArea/Header/CurrencyPills/CoinsPill | Currency pills — estáticos |
| Canvas/SafeArea/Header/CurrencyPills/GemsPill | Currency pills — estáticos |
| Canvas/SafeArea/Header/CurrencyPills/CoinsAddButton | Currency pills — estáticos |
| Canvas/SafeArea/Header/CurrencyPills/GemsAddButton | Currency pills — estáticos |
| Canvas/SafeArea/Header/CurrencyPills/CoinsIcon | Currency pills — estáticos |
| Canvas/SafeArea/Header/CurrencyPills/GemsIcon | Currency pills — estáticos |
| Canvas/SafeArea/Header/CurrencyPills/CoinsValueText | Currency pills — estáticos |
| Canvas/SafeArea/Header/CurrencyPills/GemsValueText | Currency pills — estáticos |
| Canvas/SafeArea/TitleSection | RectTransform container — no Image |
| Canvas/SafeArea/CardsSection | RectTransform + VerticalLayoutGroup — no Image |
| Canvas/SafeArea/CardsSection/SoloCard | RectTransform + Button (targetGraphic=Face Image) — no Image propia en el container |
| Canvas/SafeArea/CardsSection/SoloCard/Shadow | Efecto de sombra decorativa — negro 50% hardcodeado, no temático |
| Canvas/SafeArea/CardsSection/SoloCard/Face/IconContainer | RectTransform container — no Image |
| Canvas/SafeArea/CardsSection/OneVsOneCard | RectTransform + Button — no Image propia en el container |
| Canvas/SafeArea/CardsSection/OneVsOneCard/Shadow | Efecto de sombra decorativa — negro 50% hardcodeado, no temático |
| Canvas/SafeArea/CardsSection/OneVsOneCard/Face/IconContainer | RectTransform container — no Image |
| Canvas/SafeArea/CardsSection/TournamentsCard | RectTransform + Button — no Image propia en el container |
| Canvas/SafeArea/CardsSection/TournamentsCard/Shadow | Efecto de sombra decorativa — negro 50% hardcodeado, no temático |
| Canvas/SafeArea/CardsSection/TournamentsCard/Face/IconContainer | RectTransform container — no Image |

#### TABLE 3 — Casos especiales (2 ThemeAppliers en mismo objeto)
| Objeto | ThemeApplier #1 | ThemeApplier #2 | Razón |
|---|---|---|---|
| Canvas/SafeArea/Header/TitleText | TextMeshProUGUI → `TextTitle` (applyToText=true) | Outline → `Glow` (applyToOutline=true) | Título "SELECT MODE" + glow de contorno |
| Canvas/SafeArea/CardsSection/SoloCard/Face | Image → `CardBackground` (applyToImage=true) | Outline → `Glow` (applyToOutline=true) | Fondo card + borde neon |
| Canvas/SafeArea/CardsSection/OneVsOneCard/Face | Image → `CardBackground` (applyToImage=true) | Outline → `Glow` (applyToOutline=true) | Fondo card + borde neon |
| Canvas/SafeArea/CardsSection/TournamentsCard/Face | Image → `CardBackground` (applyToImage=true) | Outline → `Glow` (applyToOutline=true) | Fondo card + borde neon |

#### Notas de escena
- **Iconos de modo — BORDERLINE**: Los 3 iconos (Solo, 1v1, Tournaments) tienen `Image.color = Color.white` y el UIBuilder comenta explícitamente "WHITE color for ThemeApplier tinting" (L298). Esto indica intención del desarrollador de tintarlos con ThemeApplier. Sin embargo, el tintado solo funciona si el sprite PNG es 100% blanco puro. **Acción requerida**: verificar en Inspector si las imágenes `PlayModeSelectionSoloIcon.png`, `PlayModeSelection1v1Icon.png`, `PlayModeSelectionTorunamentIcon.png` son glífos blancos. Si son ilustraciones con colores → mover a TABLE 2.
- **Side (profundidad 3D)**: Cada card tiene un `Side` Image con color `CYAN_NEON * 0.3` (#004D4D aprox.) que simula profundidad 3D. Al recibir ThemeApplier Accent (reset a blanco), el color resultante dependerá del tema pero la opacidad 30% debe ajustarse vía alpha en ThemeData o como `Glow` (también mapea a primaryAccent). Se asigna Accent para que escale con el color de acento del tema.
- **Shadow no temática**: Los elementos `Shadow` de cada card usan negro 50% como sombra de profundidad — efecto visual fijo, no relacionado con paleta de temas.
- **GridGlowPulse**: Componente de animación en `Face` que pulsa el glow del Outline. No afecta la auditoría — ThemeApplier en Face sigue siendo válido.
- **CurrencyPills estáticos**: Todos los objetos de `CurrencyHeaderBarHelper.CreateCurrencyPills()` son estáticos — nunca ThemeApplier en ninguna escena.
- **Sin cambios de color en runtime**: `PlayModeSelectionManager.UpdateTexts()` solo cambia `.text` (AutoLocalizer). No hay sobreescrituras de color en runtime. Todos los colores son estables.

---

### 11 · `Games/Navigation/BetSelection.unity` — 📝 Auditado

#### TABLE 1 — Objetos a TINTAR (YES ThemeApplier)
| Objeto (GameObject path) | Componente | ElementType | ThemeData Property | Color NeonDark | Notas |
|---|---|---|---|---|---|
| Canvas/Background | Image | `Background` | background | #0A0A1A | raycastTarget=false |
| Canvas/HeaderSection | Image | `Header` | headerBackground | #0D1B2A | DUAL con Outline abajo |
| Canvas/HeaderSection | Outline | `HeaderGlow` | primaryAccent 40% | #00E5FF66 | applyToOutline=true |
| Canvas/HeaderSection/TitleText | TextMeshPro | `PrimaryAccent` | primaryAccent | #00E5FF | "bet_title" |
| Canvas/HeaderSection/GameNameText | TextMeshPro | `AccentGold` | accentGold | #FFD700 | nombre del juego seleccionado |
| Canvas/ScrollArea/Viewport/Content/FreeBetOption | Image | `CardBackground` | cardBackground | #1A2A3A | DUAL con Outline; HighlightCard() sobreescribe en selección |
| Canvas/ScrollArea/Viewport/Content/FreeBetOption | Outline | `Glow` | primaryAccent | #00E5FF | applyToOutline=true; HighlightCard() sobreescribe en selección |
| Canvas/ScrollArea/Viewport/Content/FreeBetOption/AccentBar | Image | `PrimaryAccent` | primaryAccent | #00E5FF | barra izquierda cyan |
| Canvas/ScrollArea/Viewport/Content/FreeBetOption/FreeBetText | TextMeshPro | `PrimaryAccent` | primaryAccent | #00E5FF | "bet_free" |
| Canvas/ScrollArea/Viewport/Content/FreeBetOption/DescriptionText | TextMeshPro | `TextSecondary` | textSecondary | #8899AA | "bet_free_desc" |
| Canvas/ScrollArea/Viewport/Content/FreeBetOption/FreeLabel | TextMeshPro | `PrimaryAccent` | primaryAccent | #00E5FF | badge "FREE" |
| Canvas/ScrollArea/Viewport/Content/CoinBetsHeader/LineLeft | Image | `AccentGold` | accentGold | #FFD70050 | línea decorativa izquierda (30% alpha) |
| Canvas/ScrollArea/Viewport/Content/CoinBetsHeader/SectionText | TextMeshPro | `AccentGold` | accentGold | #FFD700 | "DIGITCOINS" |
| Canvas/ScrollArea/Viewport/Content/CoinBetsHeader/LineRight | Image | `AccentGold` | accentGold | #FFD70050 | línea decorativa derecha (30% alpha) |
| Canvas/.../Coins50BetOption | Image | `CardBackground` | cardBackground | #1A2A3A | DUAL; HighlightCard() sobreescribe en selección — aplica a Coins50/100/250/500/1000 (×5) |
| Canvas/.../Coins50BetOption | Outline | `AccentGold` | accentGold | #FFD700 | applyToOutline=true; HighlightCard() sobreescribe — aplica ×5 |
| Canvas/.../Coins50BetOption/AccentBar | Image | `AccentGold` | accentGold | #FFD700 | barra dorada izquierda — aplica ×5 |
| Canvas/.../Coins50BetOption/CostText | TextMeshPro | `AccentGold` | accentGold | #FFD700 | "bet_coins_cost" — aplica ×5 |
| Canvas/.../Coins50BetOption/RewardText | TextMeshPro | `TextSecondary` | textSecondary | #8899AA | "bet_coins_wager" — aplica ×5 |
| Canvas/.../Coins500BetOption/Badge | Image | `CardBackground` | cardBackground | #0F1C2C | badge "x2" bg — aplica ×2 (500+1000) |
| Canvas/.../Coins500BetOption/BadgeText | TextMeshPro | `AccentGold` | accentGold | #FFD700 | texto "x2" — aplica ×2 |
| Canvas/ScrollArea/Viewport/Content/GemBetsHeader/LineLeft | Image | `AccentSecondary` | accentSecondary | #7B33FF50 | línea decorativa izquierda gems (30% alpha) |
| Canvas/ScrollArea/Viewport/Content/GemBetsHeader/SectionText | TextMeshPro | `AccentSecondary` | accentSecondary | #7B33FF | "DIGITGEMS" |
| Canvas/ScrollArea/Viewport/Content/GemBetsHeader/LineRight | Image | `AccentSecondary` | accentSecondary | #7B33FF50 | línea decorativa derecha gems (30% alpha) |
| Canvas/.../Gems10BetOption | Image | `CardBackground` | cardBackground | #1A2A3A | DUAL; HighlightCard() sobreescribe en selección — aplica a Gems10/50/100/250/500 (×5) |
| Canvas/.../Gems10BetOption | Outline | `AccentSecondary` | accentSecondary | #7B33FF | applyToOutline=true; HighlightCard() sobreescribe — aplica ×5 |
| Canvas/.../Gems10BetOption/AccentBar | Image | `AccentSecondary` | accentSecondary | #7B33FF | barra violet izquierda — aplica ×5 |
| Canvas/.../Gems10BetOption/CostText | TextMeshPro | `AccentSecondary` | accentSecondary | #7B33FF | costo en gemas — aplica ×5 |
| Canvas/.../Gems10BetOption/RewardText | TextMeshPro | `TextSecondary` | textSecondary | #8899AA | recompensa — aplica ×5 |
| Canvas/.../Gems250BetOption/Badge | Image | `CardBackground` | cardBackground | #0F1C2C | badge "x2" bg — aplica ×2 (250+500) |
| Canvas/.../Gems250BetOption/BadgeText | TextMeshPro | `AccentSecondary` | accentSecondary | #7B33FF | texto "x2" — aplica ×2 |
| Canvas/ScrollArea/Viewport/Content/CustomBetsHeader/LineLeft | Image | `PrimaryAccent` | primaryAccent | #00E5FF50 | línea decorativa custom (30% alpha) |
| Canvas/ScrollArea/Viewport/Content/CustomBetsHeader/SectionText | TextMeshPro | `PrimaryAccent` | primaryAccent | #00E5FF | "CUSTOM" |
| Canvas/ScrollArea/Viewport/Content/CustomBetsHeader/LineRight | Image | `PrimaryAccent` | primaryAccent | #00E5FF50 | línea decorativa custom (30% alpha) |
| Canvas/ScrollArea/Viewport/Content/CustomBetCard | Image | `CardBackground` | cardBackground | #101E2E | DUAL; fondo diferenciado (CUSTOM_CARD_BG más oscuro) |
| Canvas/ScrollArea/Viewport/Content/CustomBetCard | Outline | `Glow` | primaryAccent | #00E5FF | applyToOutline=true |
| Canvas/.../CustomBetCard/ToggleRow/CustomCoinsToggle | Image | `ButtonSecondary` | buttonSecondary | #1E3A5F | SetToggleVisual() sobreescribe en runtime (TOGGLE_ON/OFF) |
| Canvas/.../CustomBetCard/ToggleRow/CoinsToggleText | TextMeshPro | `PrimaryAccent` | primaryAccent | #00E5FF | "DIGITCOINS" |
| Canvas/.../CustomBetCard/ToggleRow/CustomGemsToggle | Image | `ButtonSecondary` | buttonSecondary | #1E3A5F | SetToggleVisual() sobreescribe en runtime |
| Canvas/.../CustomBetCard/ToggleRow/GemsToggleText | TextMeshPro | `AccentSecondary` | accentSecondary | #7B33FF | "DIGITGEMS" |
| Canvas/.../CustomBetCard/InputRow/CustomMinusButton | Image | `ButtonSecondary` | buttonSecondary | #1E3A5F | stepper "-" |
| Canvas/.../CustomBetCard/InputRow/CustomPlusButton | Image | `ButtonSecondary` | buttonSecondary | #1E3A5F | stepper "+" |
| Canvas/.../CustomBetCard/InputRow/CustomAmountInput | Image | `InputBackground` | inputBackground | #0D1B2A | DUAL con Outline |
| Canvas/.../CustomBetCard/InputRow/CustomAmountInput | Outline | `Glow` | primaryAccent 70% | #00E5FFB3 | applyToOutline=true |
| Canvas/.../CustomBetCard/InputRow/CustomAmountInput/Text Area/AmountPlaceholder | TextMeshPro | `TextDim` | textDim | #445566 | placeholder hint |
| Canvas/.../CustomBetCard/InputRow/CustomAmountInput/Text Area/Text | TextMeshPro | `TextPrimary` | textPrimary | #FFFFFF | texto ingresado |
| Canvas/.../CustomBetCard/CustomRewardText | TextMeshPro | `PrimaryAccent` | primaryAccent | #00E5FF | "bet_custom_reward" — solo .text cambia en runtime, color seguro |
| Canvas/ScrollArea/Viewport/Content/RoundsPanel | Image | `CardBackground` | cardBackground | #1A2A3A | DUAL con Outline |
| Canvas/ScrollArea/Viewport/Content/RoundsPanel | Outline | `Glow` | primaryAccent | #00E5FF | applyToOutline=true |
| Canvas/.../RoundsPanel/RoundsLabel | TextMeshPro | `PrimaryAccent` | primaryAccent | #00E5FF | "ROUNDS" |
| Canvas/.../RoundsPanel/RoundsButtonsRow/Rounds1Button | Image | `AccentGold` | accentGold | #FFD700 | SelectRounds() sobreescribe TODOS en runtime (Gold=activo/dark=inactivo) |
| Canvas/.../RoundsPanel/RoundsButtonsRow/Rounds3Button | Image | `ButtonSecondary` | buttonSecondary | #1E3A5F | SelectRounds() sobreescribe |
| Canvas/.../RoundsPanel/RoundsButtonsRow/Rounds5Button | Image | `ButtonSecondary` | buttonSecondary | #1E3A5F | SelectRounds() sobreescribe |
| Canvas/.../RoundsPanel/RoundsButtonsRow/Rounds1ButtonText | TextMeshPro | `TextPrimary` | textPrimary | #FFFFFF | "1" |
| Canvas/.../RoundsPanel/RoundsButtonsRow/Rounds3ButtonText | TextMeshPro | `TextPrimary` | textPrimary | #FFFFFF | "3" |
| Canvas/.../RoundsPanel/RoundsButtonsRow/Rounds5ButtonText | TextMeshPro | `TextPrimary` | textPrimary | #FFFFFF | "5" |
| Canvas/ButtonsRow/PlayButton | Image | `ButtonPrimary` | buttonPrimary | #00C853 | DUAL con Outline |
| Canvas/ButtonsRow/PlayButton | Outline | `ButtonPrimaryGlow` | buttonPrimaryGlow | #00FF8880 | applyToOutline=true |
| Canvas/ButtonsRow/PlayButton/PlayText | TextMeshPro | `TextPrimary` | textPrimary | #FFFFFF | "PLAY" |
| Canvas/ButtonsRow/CancelButton | Image | `ButtonDestructive` | buttonDestructive | #8B1A1A | sin outline |
| Canvas/ButtonsRow/CancelButton/CancelText | TextMeshPro | `TextPrimary` | textPrimary | #FFFFFF | "CANCEL" |

#### TABLE 2 — Objetos que NO se tintan
| Objeto (GameObject path) | Razón |
|---|---|
| Canvas/ScrollArea | Image Color.clear + ScrollRect — contenedor estructural transparente |
| Canvas/ScrollArea/Viewport | Image Color.clear + RectMask2D — contenedor estructural transparente |
| Canvas/ScrollArea/Viewport/Content | Image Color.clear + VerticalLayoutGroup — contenedor estructural transparente |
| Canvas/.../[cualquier]BetOption/Shadow | Image negro/oscuro fijo — efecto de profundidad 3D, no temático |
| Canvas/.../[cualquier]BetOption/GlassOverlay | Image blanco 4% alpha fijo — glassmorphism overlay, siempre igual |
| Canvas/HeaderSection/CurrencyPills (container + CoinsPill + GemsPill + CoinsAddButton + GemsAddButton + CoinsIcon + GemsIcon + CoinsValueText + GemsValueText) | Currency pills — objetos estáticos, no se modifican con ThemeApplier (decisión de diseño) |
| Canvas/BackButton | Prefab compartido — ThemeApplier configurado en el prefab, no en esta escena |
| Canvas/.../CustomBetCard/InputRow/FlexSpacer × 2 | LayoutElement vacío — sin renderer |
| Canvas/Spacers × N | LayoutElement vacío — sin renderer |

#### TABLE 3 — Casos especiales (2 ThemeAppliers en mismo objeto)
| Objeto | ThemeApplier #1 | ThemeApplier #2 | Razón |
|---|---|---|---|
| Canvas/HeaderSection | Image → `Header` (applyToImage) | Outline → `HeaderGlow` (applyToOutline) | Header con glow cyan en borde |
| Canvas/.../FreeBetOption | Image → `CardBackground` (applyToImage) | Outline → `Glow` (applyToOutline) | Tarjeta free con glow primario |
| Canvas/.../Coins50BetOption | Image → `CardBackground` (applyToImage) | Outline → `AccentGold` (applyToOutline) | Tarjeta coins dorada — patrón ×5 (50/100/250/500/1000) |
| Canvas/.../Gems10BetOption | Image → `CardBackground` (applyToImage) | Outline → `AccentSecondary` (applyToOutline) | Tarjeta gems violet — patrón ×5 (10/50/100/250/500) |
| Canvas/.../CustomBetCard | Image → `CardBackground` (applyToImage) | Outline → `Glow` (applyToOutline) | Tarjeta custom con glow primario |
| Canvas/.../CustomAmountInput | Image → `InputBackground` (applyToImage) | Outline → `Glow` (applyToOutline) | Input field con glow teal |
| Canvas/.../RoundsPanel | Image → `CardBackground` (applyToImage) | Outline → `Glow` (applyToOutline) | Panel rondas con glow primario |
| Canvas/ButtonsRow/PlayButton | Image → `ButtonPrimary` (applyToImage) | Outline → `ButtonPrimaryGlow` (applyToOutline) | Botón play con glow verde |

#### TABLE 4 — Objetos ocultos/inactivos en Awake
| Objeto (GameObject path) | Estado inicial | Cuándo se activa | ThemeApplier igual que activo |
|---|---|---|---|
| *(ninguno identificado — todos los elementos son visibles en estado inicial)* | | | |

#### Notas de escena
- **HighlightCard() runtime override**: `BetSelectionPanel.HighlightCard(btn, on)` cambia `Image.color` (CARD_SEL vs CARD_BG) **y** `Outline.effectColor` en la tarjeta seleccionada. ThemeApplier establece el color base; la selección activa lo sobreescribe en runtime. Al deseleccionar, el código restaura CARD_BG (hardcoded) en lugar de releer ThemeApplier → potencial desync de color en temas no-neon. **Refactor pendiente**: `HighlightCard()` debería leer `ThemeManager.current.cardBackground` / `theme.primaryAccent` en lugar de constantes.
- **SelectRounds() runtime override**: `SelectRounds(int n)` pinta el botón activo con Gold hardcoded y los inactivos con color fijo oscuro. Mismo problema que HighlightCard. Los 3 Rounds buttons tienen ThemeApplier como base pero se sobreescriben al primer tap.
- **SetToggleVisual() runtime override**: `SetToggleVisual(btn, active)` en CustomCoinsToggle/CustomGemsToggle hardcodea TOGGLE_ON / TOGGLE_OFF. Mismo patrón que UpdateToggleVisual() en los minijuegos.
- **Accent por sección**: Coins=`AccentGold` (dorado), Gems=`AccentSecondary` (violet/purple), Free+Custom=`PrimaryAccent` (cyan). Separación clara por tipo de moneda.
- **GlassOverlay**: Cada tarjeta tiene una Image semitransparente (blanco 4%) encima. Siempre igual en todos los temas — NO ThemeApplier.
- **Badge "x2"**: Solo en Coins500/1000 y Gems250/500. Fondo `CardBackground` (más oscuro), texto `AccentGold` o `AccentSecondary` según sección.

---

### 12 · `Games/Navigation/Matchmaking.unity` — 📝 Auditado

#### TABLE 1 — Objetos a TINTAR (ThemeApplier = YES)
| Objeto (GameObject path) | Componente | ElementType | ThemeData Property | Color NeonDark | Notas |
|---|---|---|---|---|---|
| Canvas/SafeArea/Background | Image | PrimaryBackground | primaryBackground | #050A14 | Reset Image color a blanco primero |
| Canvas/SafeArea/Background/Spotlight | Image | Accent | primaryAccent | #00FFFF | Tint ambiental sutil (alpha 2.5%) — reset Image color a blanco, mantener alpha bajo |
| Canvas/SafeArea/Header/GameIconContainer/IconBackground | Image | CardBackground | cardBackground | #101428 | Fondo cuadrado del ícono de juego (dual #1) |
| Canvas/SafeArea/Header/GameIconContainer/GameIcon/Placeholder | TextMeshProUGUI | Accent | primaryAccent | #00FFFF | Solo visible cuando no hay sprite de juego asignado; Manager.ShowIconPlaceholder() L307-309 hardcodea #00F5FF → usar ThemeManager.current.primaryAccent |
| Canvas/SafeArea/Header/GameNameText | TextMeshProUGUI | TextSecondary | textSecondary | #C4CCFF | Nombre del juego (p.ej. "DIGIT RUSH") |
| Canvas/SafeArea/TitleText | TextMeshProUGUI | TextTitle | textTitle | #FFFFFF | Estado dinámico: "SEARCHING...", "MATCH FOUND!", etc. — hardcodeado CYAN_NEON, reset a blanco (dual #1) |
| Canvas/SafeArea/BattleArea/PlayerCard/AvatarSection/AvatarContainer/AvatarMask | Image | CardBackground | cardBackground | #141A33 | Fondo circular interior del avatar — NO referenciado por BattleCardApplier; visible entre AvatarFrame y PlayerAvatar |
| Canvas/SafeArea/BattleArea/PlayerCard/PlayerInfo/PlayerLevel/LevelText | TextMeshProUGUI | Accent | primaryAccent | #00FFFF | Texto "Lv. X" — NO referenciado por BattleCardApplier (solo levelPillBg Image es referenciado) |
| Canvas/SafeArea/BattleArea/PlayerCard/PlayerInfo/YouBadge | Image | Accent | primaryAccent | #00FFFF | Badge "YOU" — fondo accent 20% (dual #1); NO referenciado por BattleCardApplier |
| Canvas/SafeArea/BattleArea/PlayerCard/PlayerInfo/YouBadge/YouText | TextMeshProUGUI | Accent | primaryAccent | #00FFFF | Texto "YOU" |
| Canvas/SafeArea/BattleArea/VSContainer/VSText | TextMeshProUGUI | TextPrimary | textPrimary | #FFFFFF | Texto "VS" — oculto hasta que se encuentra oponente; ver TABLE 4 |
| Canvas/SafeArea/BattleArea/OpponentCard/AvatarSection/AvatarContainer/AvatarMask | Image | CardBackground | cardBackground | #141A33 | Fondo circular interior del avatar oponente — NO referenciado por BattleCardApplier |
| Canvas/SafeArea/BattleArea/OpponentCard/OpponentInfo/OpponentLevel/LevelText | TextMeshProUGUI | TextSecondary | textSecondary | #707399 | Nivel oponente (muted) — NO referenciado por BattleCardApplier |
| Canvas/SafeArea/SearchSection/StatusText | TextMeshProUGUI | TextSecondary | textSecondary | #C4CCFF | "Searching for opponent..." — se borra al encontrar partida |
| Canvas/SafeArea/SearchSection/TimerText | TextMeshProUGUI | Accent | primaryAccent | #00FFFF | Contador de tiempo de búsqueda ("0:00") — solo `.text` cambia en runtime, color estable |
| Canvas/SafeArea/CancelButtonContainer/CancelButton | Image | ButtonDanger | buttonDanger | #FF3366 | Fondo oscuro rojo (RED_NEON × 15%) — reset Image color a blanco (dual #1) |
| Canvas/SafeArea/CancelButtonContainer/CancelButton/Text | TextMeshProUGUI | TextOnDanger | textOnDanger | #FF3366 | Texto "CANCEL" |
| Canvas/SafeArea/CountdownPanel | Image | Overlay | overlayColor | #000000 @88% | Overlay fullscreen oscuro — oculto hasta match found; ver TABLE 4 |
| Canvas/SafeArea/CountdownPanel/GetReadyText | TextMeshProUGUI | TextPrimary | textPrimary | #FFFFFF | Texto "GET READY!" — ver TABLE 4 |
| Canvas/SafeArea/CountdownPanel/CountdownText | TextMeshProUGUI | Success | successColor | #3CFF6B | Countdown 3/2/1 y "GO!" — verde semántico (dual #1); Manager L745 hardcodea `new Color(0.2353f, 1f, 0.4196f)` → cambiar a `ThemeManager.current.successColor` |

#### TABLE 2 — Objetos que NO se tintan
| Objeto (GameObject path) | Razón |
|---|---|
| Canvas/SafeArea | RectTransform container — no Image |
| Canvas/SafeArea/Background/AmbientParticles | Placeholder GO vacío — sin Image ni componentes relevantes |
| Canvas/SafeArea/Header | RectTransform container — no Image |
| Canvas/SafeArea/Header/GameIconContainer | RectTransform container — no Image |
| Canvas/SafeArea/Header/GameIconContainer/GameIcon | Arte ilustrado del juego (sprite multi-color) — regla iconos: NO tintable |
| Canvas/SafeArea/BattleArea | RectTransform container — no Image |
| Canvas/SafeArea/BattleArea/PlayerCard | RectTransform container + BattleCardApplier — la cosmética de la tarjeta la gestiona BattleCardApplier |
| Canvas/SafeArea/BattleArea/PlayerCard/CardBackground | BattleCardApplier referencia `cardBackground` (Image) y `outlineBorder` (Outline) — runtime cosmético controla estos |
| Canvas/SafeArea/BattleArea/PlayerCard/AvatarSection | RectTransform container — no Image |
| Canvas/SafeArea/BattleArea/PlayerCard/AvatarSection/AvatarContainer | RectTransform + AspectRatioFitter — no Image |
| Canvas/SafeArea/BattleArea/PlayerCard/AvatarSection/AvatarContainer/AvatarGlow | BattleCardApplier referencia `avatarGlow` Image — runtime cosmético |
| Canvas/SafeArea/BattleArea/PlayerCard/AvatarSection/AvatarContainer/AvatarFrame | BattleCardApplier referencia `avatarFrame` Image — runtime cosmético |
| Canvas/SafeArea/BattleArea/PlayerCard/AvatarSection/AvatarContainer/AvatarMask/PlayerAvatar | Foto/avatar del jugador — arte de perfil, no tintable |
| Canvas/SafeArea/BattleArea/PlayerCard/PlayerInfo | RectTransform container — no Image |
| Canvas/SafeArea/BattleArea/PlayerCard/PlayerInfo/PlayerName | BattleCardApplier referencia `playerNameText` TMP — runtime cosmético controla |
| Canvas/SafeArea/BattleArea/PlayerCard/PlayerInfo/PlayerLevel | BattleCardApplier referencia `levelPillBg` Image + Outline — runtime cosmético controla |
| Canvas/SafeArea/BattleArea/VSContainer | RectTransform container — no Image |
| Canvas/SafeArea/BattleArea/OpponentCard | RectTransform container + BattleCardApplier — cosmética gestionada por BattleCardApplier |
| Canvas/SafeArea/BattleArea/OpponentCard/CardBackground | BattleCardApplier referencia `cardBackground` + `outlineBorder` — runtime cosmético |
| Canvas/SafeArea/BattleArea/OpponentCard/AvatarSection | RectTransform container — no Image |
| Canvas/SafeArea/BattleArea/OpponentCard/AvatarSection/AvatarContainer | RectTransform + AspectRatioFitter — no Image |
| Canvas/SafeArea/BattleArea/OpponentCard/AvatarSection/AvatarContainer/AvatarGlow | BattleCardApplier referencia `avatarGlow` — runtime cosmético |
| Canvas/SafeArea/BattleArea/OpponentCard/AvatarSection/AvatarContainer/AvatarFrame | BattleCardApplier referencia `avatarFrame` — runtime cosmético |
| Canvas/SafeArea/BattleArea/OpponentCard/AvatarSection/AvatarContainer/AvatarMask/OpponentAvatar | Foto/avatar del oponente — arte de perfil, no tintable |
| Canvas/SafeArea/BattleArea/OpponentCard/OpponentInfo | RectTransform container — no Image |
| Canvas/SafeArea/BattleArea/OpponentCard/OpponentInfo/OpponentName | BattleCardApplier referencia `playerNameText` — runtime cosmético |
| Canvas/SafeArea/BattleArea/OpponentCard/OpponentInfo/OpponentLevel | BattleCardApplier referencia `levelPillBg` Image + Outline — runtime cosmético |
| Canvas/SafeArea/SearchSection | RectTransform container — no Image |
| Canvas/SafeArea/CancelButtonContainer | RectTransform container — no Image |
| Canvas/SafeArea/ScreenFlash | Efecto de animación puro — runtime corrutina controla alpha (0→0.6→0); no temático |
| ---ANIMATION_MANAGERS--- | Marcador organizativo — sin componentes UI |

#### TABLE 3 — Casos especiales (2 ThemeAppliers en mismo objeto)
| Objeto | ThemeApplier #1 | ThemeApplier #2 | Razón |
|---|---|---|---|
| Canvas/SafeArea/Header/GameIconContainer/IconBackground | Image → `CardBackground` (applyToImage=true) | Outline → `Glow` (applyToOutline=true) | Fondo card del ícono + borde glow cyan |
| Canvas/SafeArea/TitleText | TextMeshProUGUI → `TextTitle` (applyToText=true) | Outline → `Glow` (applyToOutline=true) | Texto estado principal + glow de contorno |
| Canvas/SafeArea/BattleArea/PlayerCard/PlayerInfo/YouBadge | Image → `Accent` (applyToImage=true) | Outline → `Glow` (applyToOutline=true) | Badge "YOU": fondo accent semitransparente + borde glow |
| Canvas/SafeArea/CancelButtonContainer/CancelButton | Image → `ButtonDanger` (applyToImage=true) | Outline → `ButtonGlowDanger` (applyToOutline=true) | Botón cancelar: fondo rojo oscuro + borde rojo neon |
| Canvas/SafeArea/CountdownPanel/CountdownText | TextMeshProUGUI → `Success` (applyToText=true) | Outline → `ButtonGlowSuccess` (applyToOutline=true) | Countdown verde semántico + glow verde |

#### TABLE 4 — Objetos ocultos/inactivos en Awake
| Objeto (GameObject path) | Estado inicial | Cuándo se activa | ThemeApplier igual que activo |
|---|---|---|---|
| Canvas/SafeArea/BattleArea/VSContainer | `SetActive(false)` en `StartSearching()` | Al encontrar oponente (`MatchFoundSequence`) | VSText → TextPrimary |
| Canvas/SafeArea/CountdownPanel | `SetActive(false)` en `CreateCountdownPanel()` | Al iniciar countdown después del VS | Overlay + GetReadyText + CountdownText (dual) |
| Canvas/SafeArea/ScreenFlash | `SetActive(false)` en `CreateScreenFlash()` | Efecto flash al encontrar oponente | NO ThemeApplier — efecto de animación puro (alpha corrutina) |

#### Notas de escena
- **BattleCardApplier — sistema cosmético de tarjetas**: Cada PlayerCard y OpponentCard tiene `BattleCardApplier` que controla 6 referencias: `cardBackground` (Image), `outlineBorder` (Outline), `avatarGlow` (Image), `avatarFrame` (Image), `levelPillBg` (Image), `playerNameText` (TMP). Estos 6 elementos por card (12 total) NO deben recibir ThemeApplier — el cosmético los gestiona en runtime vía `ApplyCard(card)`.
- **CountdownText runtime override**: `MatchmakingManager.CountdownSequence()` L745 hardcodea `countdownText.color = new Color(0.2353f, 1f, 0.4196f)` al mostrar "GO!". Si se añade ThemeApplier (Success), este hardcode sobreescribe el color. Fix: reemplazar esa línea con `countdownText.color = ThemeManager.current?.successColor ?? new Color(0.2353f, 1f, 0.4196f)`.
- **GameIcon runtime swap**: `SetupGameIcon()` en Manager asigna el sprite correcto según el tipo de juego (DigitRush, MemoryPairs, etc.). La Image del GameIcon es arte ilustrado multi-color → NO ThemeApplier independientemente del sprite asignado.
- **Placeholder text hardcodeado**: `ShowIconPlaceholder()` L307-309 establece color #00F5FF hardcodeado. En flujo normal está oculto (hay ícono asignado). Fix: usar `ThemeManager.current?.primaryAccent ?? Color.cyan`.
- **AvatarMask — fondo visible**: El AvatarMask es el contenedor circular de recorte. Su color `CARD_BG_LIGHT` (#141A33) es visible en el anillo interior entre AvatarFrame y el avatar. No lo controla BattleCardApplier → YES ThemeApplier como CardBackground.
- **MatchmakingAnimator** (componente separado): El archivo `MatchmakingAnimator.cs` tiene SerializeField adicionales (`searchRing`, `searchDots[]`, `vsGlow`, `opponentSilhouette`, `revealParticles`) que NO son creados por MatchmakingUIBuilder. Si este componente se añade manualmente a la escena, sus objetos referenciados necesitarán auditoría adicional. La escena actual no los incluye en el UIBuilder.

---

### 13 · `Games/Minigames/DigitRush.unity` — 📝 Documentado

62 objetos evaluados · 41 a tintar (66 componentes ThemeApplier) · 21 NO · 10 duales · 3 ocultos

#### Objetos a TINTAR
| Objeto (GameObject path) | Componente | ElementType | ThemeData Property | Color NeonDark | Notas |
|---|---|---|---|---|---|
| `Canvas/Background` | Image | `PrimaryBackground` | `primaryBackground` | #050A14 | Reset Image.color a blanco |
| `Canvas/SafeArea/Header` | Image | `SecondaryBackground` | `secondaryBackground` | rgba(0,0,0,0.3) | Header translúcido persistente |
| `Canvas/SafeArea/Header/TitleText` | TextMeshProUGUI | `TextTitle` | `textTitle` | #00FFFF | "DIGIT RUSH" — reset color a blanco |
| `Canvas/SafeArea/StatsBar` | Image | `CardBackground` | `cardBackground` | #0D1926 | Reset Image.color a blanco |
| `Canvas/SafeArea/StatsBar` | Outline | `Glow` | `glowColor` | #00FFFF | Outline neón del panel stats |
| `Canvas/SafeArea/StatsBar/TimerContainer/TimerIcon` | Image | `Accent` | `primaryAccent` | #00FFFF | ⚠️ BORDERLINE — verificar que el sprite TimerIcon.png sea 100% blanco puro |
| `Canvas/SafeArea/StatsBar/TimerContainer/TimerText` | TextMeshProUGUI | `TextPrimary` | `textPrimary` | #FFFFFF | Color.white — sin conflicto |
| `Canvas/SafeArea/StatsBar/RoundContainer/RoundIcon` | Image | `Accent` | `primaryAccent` | #00FFFF | ⚠️ BORDERLINE — verificar que RoundIcon.png sea 100% blanco puro |
| `Canvas/SafeArea/StatsBar/RoundContainer/RoundText` | TextMeshProUGUI | `Accent` | `primaryAccent` | #00FFFF | Reset color a blanco |
| `Canvas/SafeArea/StatsBar/ErrorsContainer/ErrorsIcon` | Image | `Error` | `errorColor` | #FF4C4C | ⚠️ BORDERLINE — verificar que ErrorIcon.png sea 100% blanco puro |
| `Canvas/SafeArea/StatsBar/ErrorsContainer/ErrorsText` | TextMeshProUGUI | `Error` | `errorColor` | #FF4C4C | Semántico de gameplay — ver decisión diseñador en Notas |
| `Canvas/SafeArea/GamePanel` | Image | `TertiaryBackground` | `tertiaryBackground` | rgba(8,15,31,0.8) | Reset Image.color a blanco |
| `Canvas/SafeArea/GamePanel` | Outline | `Glow` | `glowColor` | rgba(0,204,204,0.4) | Outline neón del game panel |
| `Canvas/SafeArea/GamePanel/GridContainer/Cell_1/Face` | Image | `CardBackground` | `cardBackground` | #141F33 | Reset Image.color a blanco |
| `Canvas/SafeArea/GamePanel/GridContainer/Cell_1/Face` | Outline | `Glow` | `glowColor` | #00FFFF | Outline neón de celda |
| `Canvas/SafeArea/GamePanel/GridContainer/Cell_1/Face/Text (TMP)` | TextMeshProUGUI | `TextPrimary` | `textPrimary` | #FFFFFF | Color.white — sin conflicto |
| `Canvas/SafeArea/GamePanel/GridContainer/Cell_2/Face` | Image | `CardBackground` | `cardBackground` | #141F33 | Igual que Cell_1 |
| `Canvas/SafeArea/GamePanel/GridContainer/Cell_2/Face` | Outline | `Glow` | `glowColor` | #00FFFF | — |
| `Canvas/SafeArea/GamePanel/GridContainer/Cell_2/Face/Text (TMP)` | TextMeshProUGUI | `TextPrimary` | `textPrimary` | #FFFFFF | — |
| `Canvas/SafeArea/GamePanel/GridContainer/Cell_3/Face` | Image | `CardBackground` | `cardBackground` | #141F33 | — |
| `Canvas/SafeArea/GamePanel/GridContainer/Cell_3/Face` | Outline | `Glow` | `glowColor` | #00FFFF | — |
| `Canvas/SafeArea/GamePanel/GridContainer/Cell_3/Face/Text (TMP)` | TextMeshProUGUI | `TextPrimary` | `textPrimary` | #FFFFFF | — |
| `Canvas/SafeArea/GamePanel/GridContainer/Cell_4/Face` | Image | `CardBackground` | `cardBackground` | #141F33 | — |
| `Canvas/SafeArea/GamePanel/GridContainer/Cell_4/Face` | Outline | `Glow` | `glowColor` | #00FFFF | — |
| `Canvas/SafeArea/GamePanel/GridContainer/Cell_4/Face/Text (TMP)` | TextMeshProUGUI | `TextPrimary` | `textPrimary` | #FFFFFF | — |
| `Canvas/SafeArea/GamePanel/GridContainer/Cell_5/Face` | Image | `CardBackground` | `cardBackground` | #141F33 | — |
| `Canvas/SafeArea/GamePanel/GridContainer/Cell_5/Face` | Outline | `Glow` | `glowColor` | #00FFFF | — |
| `Canvas/SafeArea/GamePanel/GridContainer/Cell_5/Face/Text (TMP)` | TextMeshProUGUI | `TextPrimary` | `textPrimary` | #FFFFFF | — |
| `Canvas/SafeArea/GamePanel/GridContainer/Cell_6/Face` | Image | `CardBackground` | `cardBackground` | #141F33 | — |
| `Canvas/SafeArea/GamePanel/GridContainer/Cell_6/Face` | Outline | `Glow` | `glowColor` | #00FFFF | — |
| `Canvas/SafeArea/GamePanel/GridContainer/Cell_6/Face/Text (TMP)` | TextMeshProUGUI | `TextPrimary` | `textPrimary` | #FFFFFF | — |
| `Canvas/SafeArea/GamePanel/GridContainer/Cell_7/Face` | Image | `CardBackground` | `cardBackground` | #141F33 | — |
| `Canvas/SafeArea/GamePanel/GridContainer/Cell_7/Face` | Outline | `Glow` | `glowColor` | #00FFFF | — |
| `Canvas/SafeArea/GamePanel/GridContainer/Cell_7/Face/Text (TMP)` | TextMeshProUGUI | `TextPrimary` | `textPrimary` | #FFFFFF | — |
| `Canvas/SafeArea/GamePanel/GridContainer/Cell_8/Face` | Image | `CardBackground` | `cardBackground` | #141F33 | — |
| `Canvas/SafeArea/GamePanel/GridContainer/Cell_8/Face` | Outline | `Glow` | `glowColor` | #00FFFF | — |
| `Canvas/SafeArea/GamePanel/GridContainer/Cell_8/Face/Text (TMP)` | TextMeshProUGUI | `TextPrimary` | `textPrimary` | #FFFFFF | — |
| `Canvas/SafeArea/GamePanel/GridContainer/Cell_9/Face` | Image | `CardBackground` | `cardBackground` | #141F33 | — |
| `Canvas/SafeArea/GamePanel/GridContainer/Cell_9/Face` | Outline | `Glow` | `glowColor` | #00FFFF | — |
| `Canvas/SafeArea/GamePanel/GridContainer/Cell_9/Face/Text (TMP)` | TextMeshProUGUI | `TextPrimary` | `textPrimary` | #FFFFFF | — |
| `Canvas/SafeArea/ComboContainer` | Image | `CardBackground` | `cardBackground` | rgba(26,20,38,0.8) | Reset Image.color a blanco |
| `Canvas/SafeArea/ComboContainer` | Outline | `AccentTertiary` | `tertiaryAccent` | #FFD700 | Outline dorado del combo badge |
| `Canvas/SafeArea/ComboContainer/ComboText` | TextMeshProUGUI | `AccentTertiary` | `tertiaryAccent` | #FFD700 | Reset color a blanco |
| `Canvas/SafeArea/SettingsPanel` | Image | `Overlay` | `overlayColor` | rgba(0,0,0,0.9) | Overlay oscuro al abrir settings |
| `Canvas/SafeArea/SettingsPanel/SettingsCard` | Image | `CardBackground` | `cardBackground` | rgba(10,20,36,0.98) | Reset Image.color a blanco |
| `Canvas/SafeArea/SettingsPanel/SettingsCard` | Outline | `Glow` | `glowColor` | #00FFFF | Outline de la settings card |
| `Canvas/SafeArea/SettingsPanel/SettingsCard/DigitRushTitle` | TextMeshProUGUI | `TextTitle` | `textTitle` | #00FFFF | Reset color a blanco |
| `Canvas/SafeArea/SettingsPanel/SettingsCard/DigitRushSubtitle` | TextMeshProUGUI | `TextSecondary` | `textSecondary` | #7F7F99 | — |
| `Canvas/SafeArea/SettingsPanel/SettingsCard/Divider` | Image | `InputBorder` | `inputBorder` | rgba(255,255,255,0.1) | Línea divisoria — TertiaryBackground también válido |
| `Canvas/SafeArea/SettingsPanel/SettingsCard/RoundsHeader` | Image | `SecondaryBackground` | `secondaryBackground` | rgba(0,31,20,0.5) | Header sección rounds |
| `Canvas/SafeArea/SettingsPanel/SettingsCard/RoundsHeader/RoundsHeaderText` | TextMeshProUGUI | `AccentSecondary` | `secondaryAccent` | rgba(179,255,204,1) | Texto "ROUNDS" verde claro |
| `Canvas/SafeArea/SettingsPanel/SettingsCard/RoundsContainer/ToggleRounds1` | Image | `TabActive` | `tabActive` | #00FFFF | Toggle ON por defecto — reset color |
| `Canvas/SafeArea/SettingsPanel/SettingsCard/RoundsContainer/ToggleRounds1` | Outline | `Glow` | `glowColor` | rgba(0,179,179,0.5) | Outline del toggle |
| `Canvas/SafeArea/SettingsPanel/SettingsCard/RoundsContainer/ToggleRounds1/Label` | TextMeshProUGUI | `TextOnPrimary` | `textOnPrimary` | #0A0A1A | Texto oscuro sobre fondo cyan activo |
| `Canvas/SafeArea/SettingsPanel/SettingsCard/RoundsContainer/ToggleRounds3` | Image | `TabInactive` | `tabInactive` | #141F2E | Toggle OFF — reset color |
| `Canvas/SafeArea/SettingsPanel/SettingsCard/RoundsContainer/ToggleRounds3` | Outline | `Glow` | `glowColor` | rgba(0,179,179,0.5) | — |
| `Canvas/SafeArea/SettingsPanel/SettingsCard/RoundsContainer/ToggleRounds3/Label` | TextMeshProUGUI | `TextPrimary` | `textPrimary` | #FFFFFF | — |
| `Canvas/SafeArea/SettingsPanel/SettingsCard/RoundsContainer/ToggleRounds5` | Image | `TabInactive` | `tabInactive` | #141F2E | Toggle OFF — reset color |
| `Canvas/SafeArea/SettingsPanel/SettingsCard/RoundsContainer/ToggleRounds5` | Outline | `Glow` | `glowColor` | rgba(0,179,179,0.5) | — |
| `Canvas/SafeArea/SettingsPanel/SettingsCard/RoundsContainer/ToggleRounds5/Label` | TextMeshProUGUI | `TextPrimary` | `textPrimary` | #FFFFFF | — |
| `Canvas/SafeArea/SettingsPanel/SettingsCard/StartGameButton` | Image | `ButtonSuccess` | `buttonSuccess` | #4DFF80 | Reset Image.color a blanco |
| `Canvas/SafeArea/SettingsPanel/SettingsCard/StartGameButton` | Outline | `ButtonGlowSuccess` | `successColor` | #1A8040 | Outline glow del botón start |
| `Canvas/SafeArea/SettingsPanel/SettingsCard/StartGameButton/StartText` | TextMeshProUGUI | `TextOnSuccess` | `textOnSuccess` | #050A14 | Texto oscuro sobre verde |
| `Canvas/SafeArea/CountdownPanel/Overlay` | Image | `Overlay` | `overlayColor` | rgba(0,0,0,0.6) | Overlay semitransparente countdown |
| `Canvas/SafeArea/CountdownPanel/NumberContainer/CountdownText` | TextMeshProUGUI | `TextTitle` | `textTitle` | #00FFFF | Reset color a blanco |
| `Canvas/SafeArea/CountdownPanel/NumberContainer/CountdownText` | Outline | `Glow` | `glowColor` | rgba(0,128,128,0.8) | Glow outline del número grande |

#### Objetos que NO se tintan
| Objeto (GameObject path) | Razón |
|---|---|
| `Canvas/SafeArea/GamePanel/GridContainer/Cell_N/Shadow` (×9) | Sombra 3D decorativa — negro fijo (0,0,0,0.4), semántico de profundidad, no es elemento temático |
| `Canvas/SafeArea/GamePanel/GridContainer/Cell_N/Side` (×9) | Lateral del efecto 3D del botón — color muy oscuro fijo, parte del estilo escultórico del botón |
| `Canvas/SafeArea/GamePanel/GridContainer/Cell_N` root (×9) | Image con Color.clear (transparente) — solo container de layout |
| `Canvas/SafeArea/GamePanel/GridContainer/Cell_N/Face` — Shadow component | Glow interno manejado por `Cell3DButton` en runtime (flash rojo en error, glow cyan en hover) — conflicto con ThemeApplier |
| `Canvas/SafeArea/ParticleEffects` | UISparkleEffect/ParticleSystem — no Image ni TMP; usa confettiPalette independiente |
| `Canvas/SafeArea/ActionButtonsContainer` | Contenedor vacío sin Image ni TMP — solo layout |
| `Main Camera` | Cámara — sin componentes UI |
| `EventSystem` | Sistema de eventos — sin UI visual |
| `DigitRushController` | MonoBehaviour puro de gestión de lógica — sin componentes UI |

#### Casos especiales (2 ThemeAppliers en mismo objeto)
| Objeto | ThemeApplier #1 | ThemeApplier #2 | Razón |
|---|---|---|---|
| `Canvas/SafeArea/StatsBar` | `CardBackground` · applyToImage=true | `Glow` · applyToOutline=true | Panel con Image de fondo + Outline neón |
| `Canvas/SafeArea/GamePanel` | `TertiaryBackground` · applyToImage=true | `Glow` · applyToOutline=true | Panel con Image de fondo + Outline neón |
| `Canvas/SafeArea/GamePanel/GridContainer/Cell_N/Face` (×9) | `CardBackground` · applyToImage=true | `Glow` · applyToOutline=true | Superficie de celda: Image + Outline neón |
| `Canvas/SafeArea/ComboContainer` | `CardBackground` · applyToImage=true | `AccentTertiary` · applyToOutline=true | Combo badge: Image fondo + Outline dorado |
| `Canvas/SafeArea/SettingsPanel/SettingsCard` | `CardBackground` · applyToImage=true | `Glow` · applyToOutline=true | Card con Image + Outline cyan |
| `Canvas/SafeArea/SettingsPanel/SettingsCard/StartGameButton` | `ButtonSuccess` · applyToImage=true | `ButtonGlowSuccess` · applyToOutline=true | Botón verde con Image + Outline glow |
| `Canvas/SafeArea/SettingsPanel/SettingsCard/RoundsContainer/ToggleRounds1` | `TabActive` · applyToImage=true | `Glow` · applyToOutline=true | Toggle radio: Image + Outline |
| `Canvas/SafeArea/SettingsPanel/SettingsCard/RoundsContainer/ToggleRounds3` | `TabInactive` · applyToImage=true | `Glow` · applyToOutline=true | Toggle radio: Image + Outline |
| `Canvas/SafeArea/SettingsPanel/SettingsCard/RoundsContainer/ToggleRounds5` | `TabInactive` · applyToImage=true | `Glow` · applyToOutline=true | Toggle radio: Image + Outline |
| `Canvas/SafeArea/CountdownPanel/NumberContainer/CountdownText` | `TextTitle` · applyToText=true | `Glow` · applyToOutline=true | Texto con TMP + Outline neón |

#### Objetos ocultos (inactivos en escena)
| Objeto (GameObject path) | Componente | Color hardcoded | ¿Necesita ThemeApplier? | Notas |
|---|---|---|---|---|
| `Canvas/SafeArea/SettingsPanel` | Image (Overlay) | rgba(0,0,0,0.9) | SÍ | `SetActive(false)` en UIBuilder — se activa en runtime (modo Practice). Todos sus hijos (SettingsCard, toggles, StartButton) necesitan ThemeApplier |
| `Canvas/SafeArea/CountdownPanel` | CountdownUI component | — | SÍ (hijos) | `SetActive(false)` en UIBuilder — se activa antes de cada ronda. CountdownPanel/Overlay y CountdownText sí necesitan ThemeApplier |
| `Canvas/SafeArea/ComboContainer` | Image + CanvasGroup | rgba(26,20,38,0.8) | SÍ | `CanvasGroup.alpha=0` al inicio (invisible pero activo) — ThemeApplier aplica igualmente |

#### Notas de escena
- **Implementación**: Todos los ThemeApplier deben añadirse vía `DigitRushUIBuilder.cs`, NO por Inspector.
- **Cell3DButton colores runtime**: `Cell3DButton` tiene `glowColor`, `errorFaceColor`, `errorGlowColor`, `errorTextColor` hardcoded. Estos se activan en gameplay (flash rojo en error, cyan en hover). **Acción requerida**: refactorizar `Cell3DButton` para leer colores base desde `ThemeManager.Instance.CurrentTheme` en lugar de hardcoded. Los colores de error pueden mantenerse fijos (semánticos de gameplay).
- **UpdateToggleVisual sobreescritura**: `DigitRushController.UpdateToggleVisual()` sobreescribe colores de `ToggleRounds` con `CYAN_NEON`/`darkblue` hardcoded en runtime. Tras cada interacción del usuario, los toggles vuelven a cyan/darkblue ignorando el tema activo. **Acción requerida**: refactorizar para leer `theme.tabActive`/`theme.tabInactive` desde ThemeManager.
- **ErrorsText/ErrorsIcon semántica**: El rojo de errores es feedback de gameplay (contador de errores). Se recomienda mantener `ElementType.Error` para que adapte el rojo al tema activo, pero decisión final del diseñador.
- **3 iconos stats BORDERLINE**: `TimerIcon`, `RoundIcon`, `ErrorIcon` — verificar en Inspector si los sprites `.png` son 100% blancos puros. Si tienen píxeles de color propios → cambiar de YES a NO para esas Images.
- **ComboVisualController runtime**: Si crea TMP texts en runtime, deben leer `ThemeManager.Instance.CurrentTheme.primaryAccent` en lugar de colores hardcoded.
- **WinPanels excluidos**: `WinPanel_Normal`, `LosePanel_Normal` etc. se instancian como prefabs globales — auditados por separado.
- **CashThemeForcer**: `MinigameBase.Start()` añade este componente en modo CashTournament. Como CashBattle está excluido de ThemeApplier, no hay conflicto.

---

### 14 · `Games/Minigames/FlashTap.unity` — 📝 Documentado

37 objetos evaluados · 27 a tintar (34 componentes ThemeApplier) · 10 NO · 7 duales · 3 ocultos

#### Objetos a TINTAR
| Objeto (GameObject path) | Componente | ElementType | ThemeData Property | Color NeonDark | Notas |
|---|---|---|---|---|---|
| `Canvas/Background` | Image | `PrimaryBackground` | `primaryBackground` | #080A14 | Reset Image.color a blanco |
| `Canvas/SafeArea/Header/TitleText` | TextMeshProUGUI | `TextTitle` | `textTitle` | #00FFFF | Reset color a blanco |
| `Canvas/SafeArea/Header/TitleText` | Outline | `Glow` | `glowColor` | rgba(0,102,102,0.6) | Glow outline decorativo del título |
| `Canvas/SafeArea/StatsBar` | Image | `CardBackground` | `cardBackground` | #0D1A26 | Reset Image.color a blanco |
| `Canvas/SafeArea/StatsBar` | Outline | `Glow` | `glowColor` | #00FFFF | Outline neón del stats bar |
| `Canvas/SafeArea/StatsBar/TimerContainer/ReactionTimeText` | TextMeshProUGUI | `TextPrimary` | `textPrimary` | #FFFFFF | Runtime sobreescribe con colores de rendimiento — ver Notas |
| `Canvas/SafeArea/StatsBar/RoundContainer/RoundText` | TextMeshProUGUI | `Accent` | `primaryAccent` | #00FFFF | Reset color a blanco |
| `Canvas/SafeArea/StatsBar/ErrorsContainer/ErrorsText` | TextMeshProUGUI | `Error` | `errorColor` | #FF4D4D | Reset color a blanco |
| `Canvas/SafeArea/InstructionText` | TextMeshProUGUI | `Warning` | `warningColor` | #FFB34D | "WAIT..." / "TAP!" — color naranja constante, no semántico diferencial |
| `Canvas/SafeArea/FeedbackPanel` | Image | `Overlay` | `overlayColor` | rgba(0,0,0,0.7) | SetActive(false) al inicio — overlay oscuro del feedback |
| `Canvas/SafeArea/FeedbackPanel/FeedbackCard` | Image | `CardBackground` | `cardBackground` | #0F1723 | Reset Image.color a blanco |
| `Canvas/SafeArea/FeedbackPanel/FeedbackCard` | Outline | `Glow` | `glowColor` | #00FFFF | Outline cyan del feedback card |
| `Canvas/SafeArea/FeedbackPanel/FeedbackCard/FeedbackText` | TextMeshProUGUI | `AccentSecondary` | `secondaryAccent` | verde/rojo semántico | Runtime asigna verde=correcto / rojo=incorrecto — ver Notas |
| `Canvas/SafeArea/CountdownPanel/Overlay` | Image | `Overlay` | `overlayColor` | rgba(0,0,0,0.6) | SetActive(false) al inicio |
| `Canvas/SafeArea/CountdownPanel/NumberContainer/CountdownText` | TextMeshProUGUI | `TextTitle` | `textTitle` | #00FFFF | Reset color a blanco |
| `Canvas/SafeArea/SettingsPanel` | Image | `Overlay` | `overlayColor` | rgba(0,0,0,0.9) | SetActive(false) al inicio — overlay fullscreen settings |
| `Canvas/SafeArea/SettingsPanel/SettingsCard` | Image | `CardBackground` | `cardBackground` | #0A1424 | Reset Image.color a blanco |
| `Canvas/SafeArea/SettingsPanel/SettingsCard` | Outline | `Glow` | `glowColor` | #00FFFF | Outline cyan de la settings card |
| `Canvas/SafeArea/SettingsPanel/SettingsCard/FlashTapTitle` | TextMeshProUGUI | `TextTitle` | `textTitle` | #00FFFF | Reset color a blanco |
| `Canvas/SafeArea/SettingsPanel/SettingsCard/FlashTapSubtitle` | TextMeshProUGUI | `TextSecondary` | `textSecondary` | #808099 | — |
| `Canvas/SafeArea/SettingsPanel/SettingsCard/Divider` | Image | `TextDisabled` | `textDisabled` | rgba(255,255,255,0.1) | Línea divisoria tenue |
| `Canvas/SafeArea/SettingsPanel/SettingsCard/RoundsHeader` | Image | `TertiaryBackground` | `tertiaryBackground` | rgba(0,31,20,0.5) | Header sección rounds — verdoso semitransparente |
| `Canvas/SafeArea/SettingsPanel/SettingsCard/RoundsHeader/RoundsHeaderText` | TextMeshProUGUI | `AccentSecondary` | `secondaryAccent` | #B3FFCC | "ROUNDS" en verde claro |
| `Canvas/SafeArea/SettingsPanel/SettingsCard/RoundsContainer/ToggleRounds1` | Image | `TabInactive` | `tabInactive` | #141320 | Toggle OFF — reset color |
| `Canvas/SafeArea/SettingsPanel/SettingsCard/RoundsContainer/ToggleRounds1` | Outline | `Glow` | `glowColor` | rgba(0,179,179,0.5) | Outline del toggle |
| `Canvas/SafeArea/SettingsPanel/SettingsCard/RoundsContainer/ToggleRounds1/Label` | TextMeshProUGUI | `TextPrimary` | `textPrimary` | #FFFFFF | Toggle inactivo — runtime sobreescribe, ver Notas |
| `Canvas/SafeArea/SettingsPanel/SettingsCard/RoundsContainer/ToggleRounds3` | Image | `TabInactive` | `tabInactive` | #141320 | — |
| `Canvas/SafeArea/SettingsPanel/SettingsCard/RoundsContainer/ToggleRounds3` | Outline | `Glow` | `glowColor` | rgba(0,179,179,0.5) | — |
| `Canvas/SafeArea/SettingsPanel/SettingsCard/RoundsContainer/ToggleRounds3/Label` | TextMeshProUGUI | `TextPrimary` | `textPrimary` | #FFFFFF | — |
| `Canvas/SafeArea/SettingsPanel/SettingsCard/RoundsContainer/ToggleRounds5` | Image | `TabActive` | `tabActive` | #00FFFF | Toggle ON por defecto (5 rounds) — reset color |
| `Canvas/SafeArea/SettingsPanel/SettingsCard/RoundsContainer/ToggleRounds5` | Outline | `Glow` | `glowColor` | rgba(0,179,179,0.5) | — |
| `Canvas/SafeArea/SettingsPanel/SettingsCard/RoundsContainer/ToggleRounds5/Label` | TextMeshProUGUI | `TextOnPrimary` | `textOnPrimary` | #000000 | Texto oscuro sobre toggle activo cyan |
| `Canvas/SafeArea/SettingsPanel/SettingsCard/StartGameButton` | Image | `ButtonSuccess` | `buttonSuccess` | #4DFF4D | Reset Image.color a blanco |
| `Canvas/SafeArea/SettingsPanel/SettingsCard/StartGameButton/StartText` | TextMeshProUGUI | `TextOnSuccess` | `textOnSuccess` | #000000 | Texto oscuro sobre botón verde |

#### Objetos que NO se tintan
| Objeto (GameObject path) | Razón |
|---|---|
| `Canvas/SafeArea/Header` | Contenedor puro (RectTransform únicamente) — sin Image ni Text |
| `Canvas/SafeArea/TapButton3D` | Image transparente `(0,0,0,0)` — solo raycast target, sin visual |
| `Canvas/SafeArea/TapButton3D/ButtonImage` | Arte del juego — sprite con colores propios (naranja/rojo/verde), cambia programáticamente. NO es blanco puro |
| `Canvas/SafeArea/StatsBar/TimerContainer` | Contenedor LayoutElement — sin Image/Text propio |
| `Canvas/SafeArea/StatsBar/TimerContainer/TimerIcon` | ⚠️ BORDERLINE — verificar en Inspector si sprite es 100% blanco puro. Si tiene píxeles de color → NO |
| `Canvas/SafeArea/StatsBar/RoundContainer` | Contenedor LayoutElement — sin Image/Text propio |
| `Canvas/SafeArea/StatsBar/RoundContainer/RoundIcon` | ⚠️ BORDERLINE — verificar en Inspector si sprite es 100% blanco puro |
| `Canvas/SafeArea/StatsBar/ErrorsContainer` | Contenedor LayoutElement — sin Image/Text propio |
| `Canvas/SafeArea/StatsBar/ErrorsContainer/ErrorsIcon` | ⚠️ BORDERLINE — verificar en Inspector si sprite es 100% blanco puro |
| `Canvas/SafeArea/SettingsPanel/SettingsCard/StartGameButton/Shadow` | Sombra decorativa `(0,0.3,0.15,0.6)` — estético secundario, no elemento de tema primario |

#### Casos especiales (2 ThemeAppliers en mismo objeto)
| Objeto | ThemeApplier #1 | ThemeApplier #2 | Razón |
|---|---|---|---|
| `Canvas/SafeArea/Header/TitleText` | `TextTitle` · applyToText=true | `Glow` · applyToOutline=true | TMP con Outline de glow decorativo |
| `Canvas/SafeArea/StatsBar` | `CardBackground` · applyToImage=true | `Glow` · applyToOutline=true | Image fondo + Outline neón |
| `Canvas/SafeArea/FeedbackPanel/FeedbackCard` | `CardBackground` · applyToImage=true | `Glow` · applyToOutline=true | Card con fondo + Outline cyan |
| `Canvas/SafeArea/SettingsPanel/SettingsCard` | `CardBackground` · applyToImage=true | `Glow` · applyToOutline=true | Card con fondo + Outline cyan |
| `Canvas/SafeArea/SettingsPanel/SettingsCard/RoundsContainer/ToggleRounds1` | `TabInactive` · applyToImage=true | `Glow` · applyToOutline=true | Toggle: Image + Outline |
| `Canvas/SafeArea/SettingsPanel/SettingsCard/RoundsContainer/ToggleRounds3` | `TabInactive` · applyToImage=true | `Glow` · applyToOutline=true | Toggle: Image + Outline |
| `Canvas/SafeArea/SettingsPanel/SettingsCard/RoundsContainer/ToggleRounds5` | `TabActive` · applyToImage=true | `Glow` · applyToOutline=true | Toggle activo: Image + Outline |

#### Objetos ocultos (inactivos en escena)
| Objeto (GameObject path) | Componente | Color hardcoded | ¿Necesita ThemeApplier? | Notas |
|---|---|---|---|---|
| `Canvas/SafeArea/FeedbackPanel` | Image + CanvasGroup | rgba(0,0,0,0.7) | SÍ | `SetActive(false)` al crear. Activado por `ShowFeedback()` — ThemeApplier se ejecuta en OnEnable correctamente |
| `Canvas/SafeArea/CountdownPanel` | CountdownUI component | — | SÍ (hijos) | `SetActive(false)` al crear. Activado por `CountdownAnimator.Play()` |
| `Canvas/SafeArea/SettingsPanel` | Image | rgba(0,0,0,0.9) | SÍ | `SetActive(false)` al crear. Activado en modo Practice |

#### Notas de escena
- **Implementación**: Todos los ThemeApplier vía `FlashTapUIBuilder.cs`, NO por Inspector.
- **UpdateToggleVisual sobreescritura**: Igual que DigitRush — `FlashTapController.UpdateToggleVisual()` sobreescribe colores de toggles con `CYAN_NEON`/darkblue hardcoded en runtime. **Acción requerida**: refactorizar para leer `theme.tabActive`/`theme.tabInactive`.
- **ReactionTimeText colores semánticos**: `FlashTapController.ValidTapSequence()` asigna verde/amarillo/rojo según velocidad de reacción (<200ms, <300ms, <400ms, >400ms). Son feedback de rendimiento, no estética. Mantener hardcoded — el ThemeApplier establece solo el color inicial (blanco).
- **FeedbackText verde/rojo**: `ShowFeedback()` asigna color semántico correcto/incorrecto. El ThemeApplier no debe sobreescribir estos colores en runtime — añadir solo si `applyToText=false` en ese componente.
- **TapButton3D — no es UI temático**: El botón de tap usa sprites de arte del juego que cambian (Up/Down/Flash/Error). Son gráficos de gameplay, no UI de navegación.
- **3 iconos stats BORDERLINE**: `TimerIcon`, `RoundIcon`, `ErrorIcon` — verificar en Inspector. Si sprites son blancos puros con tint via Image.color → mover a YES con ElementTypes `Accent`/`Accent`/`Error` respectivamente.
- **CountdownText goColor**: CountdownUI tiene `goColor = GREEN_NEON` hardcoded. Refactorizar para leer `theme.buttonSuccess` o mantener fijo (decisión de diseño).

---

### 15 · `Games/Minigames/MemoryPairs.unity` — 📝 Documentado

68 objetos evaluados · 34 a tintar · 34 NO · 7 duales · 4 ocultos

#### Objetos a TINTAR
| Objeto (GameObject path) | Componente | ElementType | ThemeData Property | Color NeonDark | Notas |
|---|---|---|---|---|---|
| `Canvas/Background` | Image | `PrimaryBackground` | `primaryBackground` | #050A14 | Reset Image.color a blanco |
| `Canvas/SafeArea/Header` | Image | `SecondaryBackground` | `secondaryBackground` | rgba(0,0,0,0.3) | Header translúcido |
| `Canvas/SafeArea/Header/TitleText` | TextMeshProUGUI | `TextTitle` | `textTitle` | #00FFFF | Reset color a blanco |
| `Canvas/SafeArea/Header/TitleText` | Outline | `Glow` | `glowColor` | rgba(0,255,255,0.5) | Glow decorativo del título |
| `Canvas/SafeArea/StatsBar` | Image | `CardBackground` | `cardBackground` | #0D1A26 | Reset Image.color a blanco |
| `Canvas/SafeArea/StatsBar` | Outline | `Glow` | `glowColor` | rgba(0,255,255,0.5) | Outline neón del stats bar |
| `Canvas/SafeArea/StatsBar/TimerContainer/TimerText` | TextMeshProUGUI | `TextPrimary` | `textPrimary` | #FFFFFF | — |
| `Canvas/SafeArea/StatsBar/RoundContainer/RoundText` | TextMeshProUGUI | `Accent` | `primaryAccent` | #00FFFF | Reset color a blanco |
| `Canvas/SafeArea/StatsBar/PairsContainer/PairsFoundText` | TextMeshProUGUI | `Success` | `successColor` | #4DFF4D | Color semántico de éxito — ver Notas |
| `Canvas/SafeArea/StatsBar/ErrorsContainer/ErrorsText` | TextMeshProUGUI | `Error` | `errorColor` | #FF4D4D | Color semántico de error — ver Notas |
| `Canvas/SafeArea/GamePanel` | Image | `TertiaryBackground` | `tertiaryBackground` | rgba(8,15,31,0.8) | Reset Image.color a blanco |
| `Canvas/SafeArea/GamePanel` | Outline | `Glow` | `glowColor` | rgba(0,255,255,0.4) | Outline neón del área de cartas |
| `Canvas/SafeArea/FeedbackPanel` | Image | `Overlay` | `overlayColor` | rgba(0,0,0,0.7) | SetActive(false) al inicio |
| `Canvas/SafeArea/FeedbackPanel/FeedbackCard` | Image | `CardBackground` | `cardBackground` | #0F1723 | Reset Image.color a blanco |
| `Canvas/SafeArea/FeedbackPanel/FeedbackCard` | Outline | `Glow` | `glowColor` | rgba(0,255,255,0.5) | ⚠️ applyToText=false obligatorio — FeedbackText usa colores semánticos runtime |
| `Canvas/SafeArea/CountdownPanel/Overlay` | Image | `Overlay` | `overlayColor` | rgba(0,0,0,0.6) | SetActive(false) al inicio |
| `Canvas/SafeArea/CountdownPanel/NumberContainer/CountdownText` | TextMeshProUGUI | `Accent` | `primaryAccent` | #00FFFF | Reset color a blanco |
| `Canvas/SafeArea/CountdownPanel/NumberContainer/CountdownText` | Outline | `Glow` | `glowColor` | rgba(0,255,255,0.5) | Glow del número countdown |
| `Canvas/SafeArea/SettingsPanel` | Image | `Overlay` | `overlayColor` | rgba(0,0,0,0.9) | SetActive(false) al inicio |
| `Canvas/SafeArea/SettingsPanel/SettingsCard` | Image | `CardBackground` | `cardBackground` | #0A1424 | Reset Image.color a blanco |
| `Canvas/SafeArea/SettingsPanel/SettingsCard` | Outline | `Glow` | `glowColor` | rgba(0,255,255,0.5) | Outline cyan de la settings card |
| `Canvas/SafeArea/SettingsPanel/SettingsCard/MemoryPairsTitle` | TextMeshProUGUI | `TextTitle` | `textTitle` | #00FFFF | Reset color a blanco |
| `Canvas/SafeArea/SettingsPanel/SettingsCard/MemoryPairsSubtitle` | TextMeshProUGUI | `TextSecondary` | `textSecondary` | #808099 | — |
| `Canvas/SafeArea/SettingsPanel/SettingsCard/Divider` | Image | `Overlay` | `overlayColor` | rgba(255,255,255,0.1) | Línea divisoria tenue |
| `Canvas/SafeArea/SettingsPanel/SettingsCard/RoundsHeader` | Image | `TertiaryBackground` | `tertiaryBackground` | rgba(0,31,20,0.5) | Header sección rounds |
| `Canvas/SafeArea/SettingsPanel/SettingsCard/RoundsHeader/RoundsHeaderText` | TextMeshProUGUI | `TextSecondary` | `textSecondary` | #B3FFCC | ⚠️ BORDERLINE — color verde claro vs gris textSecondary; usar `Success` si se quiere preservar tono verdoso |
| `Canvas/SafeArea/SettingsPanel/SettingsCard/RoundsContainer/ToggleRounds1` | Image | `TabActive` | `tabActive` | #00FFFF | Toggle ON por defecto — reset color |
| `Canvas/SafeArea/SettingsPanel/SettingsCard/RoundsContainer/ToggleRounds1` | Outline | `Glow` | `glowColor` | rgba(0,179,179,0.5) | Outline del toggle |
| `Canvas/SafeArea/SettingsPanel/SettingsCard/RoundsContainer/ToggleRounds1/Label` | TextMeshProUGUI | `TextOnPrimary` | `textOnPrimary` | #000000 | Texto oscuro sobre toggle activo cyan |
| `Canvas/SafeArea/SettingsPanel/SettingsCard/RoundsContainer/ToggleRounds3` | Image | `TabInactive` | `tabInactive` | #141F2E | Toggle OFF — reset color |
| `Canvas/SafeArea/SettingsPanel/SettingsCard/RoundsContainer/ToggleRounds3` | Outline | `Glow` | `glowColor` | rgba(0,179,179,0.5) | — |
| `Canvas/SafeArea/SettingsPanel/SettingsCard/RoundsContainer/ToggleRounds3/Label` | TextMeshProUGUI | `TextPrimary` | `textPrimary` | #FFFFFF | — |
| `Canvas/SafeArea/SettingsPanel/SettingsCard/RoundsContainer/ToggleRounds5` | Image | `TabInactive` | `tabInactive` | #141F2E | — |
| `Canvas/SafeArea/SettingsPanel/SettingsCard/RoundsContainer/ToggleRounds5` | Outline | `Glow` | `glowColor` | rgba(0,179,179,0.5) | — |
| `Canvas/SafeArea/SettingsPanel/SettingsCard/RoundsContainer/ToggleRounds5/Label` | TextMeshProUGUI | `TextPrimary` | `textPrimary` | #FFFFFF | — |
| `Canvas/SafeArea/SettingsPanel/SettingsCard/StartGameButton` | Image | `ButtonSuccess` | `buttonSuccess` | #4DFF4D | Reset Image.color a blanco |
| `Canvas/SafeArea/SettingsPanel/SettingsCard/StartGameButton/StartText` | TextMeshProUGUI | `TextOnSuccess` | `textOnSuccess` | #000000 | Texto oscuro sobre botón verde |

#### Objetos que NO se tintan
| Objeto (GameObject path) | Razón |
|---|---|
| `Canvas/SafeArea` | RectTransform puro — sin Image/TMP |
| `Canvas/SafeArea/StatsBar/TimerContainer` | LayoutElement container — sin Image/TMP propio |
| `Canvas/SafeArea/StatsBar/TimerContainer/TimerIcon` | ⚠️ BORDERLINE — verificar en Inspector si sprite es 100% blanco puro |
| `Canvas/SafeArea/StatsBar/RoundContainer` | LayoutElement container — sin Image/TMP propio |
| `Canvas/SafeArea/StatsBar/RoundContainer/RoundIcon` | ⚠️ BORDERLINE — verificar en Inspector si sprite es 100% blanco puro |
| `Canvas/SafeArea/StatsBar/PairsContainer` | LayoutElement container — sin Image/TMP propio |
| `Canvas/SafeArea/StatsBar/PairsContainer/PairsIcon` | ⚠️ BORDERLINE — verificar en Inspector si sprite es 100% blanco puro |
| `Canvas/SafeArea/StatsBar/ErrorsContainer` | LayoutElement container — sin Image/TMP propio |
| `Canvas/SafeArea/StatsBar/ErrorsContainer/ErrorsIcon` | Muy probable que tenga píxeles rojos propios — verificar en Inspector |
| `Canvas/SafeArea/GamePanel/CardsGrid` | GridLayoutGroup container — sin Image |
| `Canvas/SafeArea/GamePanel/CardsGrid/Card_N` root (×16) | Image `Color.clear` — transparente, solo raycast target |
| `Canvas/SafeArea/GamePanel/CardsGrid/Card_N/Shadow` (×16) | Sombra 3D decorativa `(0,0,0,0.4)` — `Card3DEffect` la controla directamente |
| `Canvas/SafeArea/GamePanel/CardsGrid/Card_N/Side` (×16) | Lateral 3D — `Card3DEffect` controla `faceDownSideColor`/`faceUpSideColor`/`matchedSideColor`/`errorSideColor` en runtime |
| `Canvas/SafeArea/GamePanel/CardsGrid/Card_N/Face` Image (×16) | `Card3DEffect.EnforceColorsNextFrame()` anula cualquier ThemeApplier explícitamente — colores semánticos de gameplay |
| `Canvas/SafeArea/GamePanel/CardsGrid/Card_N/Face` Outline (×16) | `Card3DEffect` controla `faceDownGlowColor`/`faceUpGlowColor`/`matchedGlowColor`/`errorGlowColor` en runtime |
| `Canvas/SafeArea/GamePanel/CardsGrid/Card_N/Face/CardImage_N` (×16) | `Color.clear` — sprite de arte del juego, asignado en runtime. NO es UI temático |
| `Canvas/SafeArea/GamePanel/CardsGrid/Card_N/Face/CardText_N` (×16) | `Card3DEffect` controla `symbolColor`/`matchedSymbolColor`/`errorSymbolColor` — colores semánticos de gameplay |
| `Canvas/SafeArea/FeedbackPanel/FeedbackCard/FeedbackText` | Runtime asigna verde=correcto / rojo=incorrecto — colores semánticos, `applyToText=false` en FeedbackCard |
| `Canvas/SafeArea/CountdownPanel/NumberContainer` | RectTransform container puro |
| `Canvas/SafeArea/ComboText` | Colores semánticos de combo (verde→naranja→dorado) asignados por `UpdateUI()` — no estéticos |
| `Canvas/SafeArea/ComboText` Outline | Outline verde oscuro decorativo del sistema de combo — no de tema |
| `Canvas/SafeArea/SettingsPanel/SettingsCard/StartGameButton/Shadow` | Sombra decorativa `(0,0.3,0.15,0.6)` — estético secundario |
| `Canvas/SafeArea/ParticleEffects` | UISparkleEffect — partículas de gameplay |
| `EventSystem` | Sin UI |
| `MemoryPairsController` | MonoBehaviour puro — sin UI |
| `---ANIMATION_MANAGERS---` | Animadores del sistema — sin UI |
| `Main Camera` | Cámara — sin UI |

#### Casos especiales (2 ThemeAppliers en mismo objeto)
| Objeto | ThemeApplier #1 | ThemeApplier #2 | Razón |
|---|---|---|---|
| `Canvas/SafeArea/Header/TitleText` | `TextTitle` · applyToText=true | `Glow` · applyToOutline=true | TMP con Outline decorativo de glow |
| `Canvas/SafeArea/StatsBar` | `CardBackground` · applyToImage=true | `Glow` · applyToOutline=true | Image fondo + Outline neón |
| `Canvas/SafeArea/GamePanel` | `TertiaryBackground` · applyToImage=true | `Glow` · applyToOutline=true | Fondo área de cartas + Outline cyan |
| `Canvas/SafeArea/FeedbackPanel/FeedbackCard` | `CardBackground` · applyToImage=true | `Glow` · applyToOutline=true ⚠️ applyToText=false | Card con fondo + Outline; applyToText=false obligatorio |
| `Canvas/SafeArea/CountdownPanel/NumberContainer/CountdownText` | `Accent` · applyToText=true | `Glow` · applyToOutline=true | Número countdown con Outline glow |
| `Canvas/SafeArea/SettingsPanel/SettingsCard` | `CardBackground` · applyToImage=true | `Glow` · applyToOutline=true | Card modal con fondo + Outline |
| `Canvas/SafeArea/SettingsPanel/SettingsCard/RoundsContainer/ToggleRounds1` | `TabActive` · applyToImage=true | `Glow` · applyToOutline=true | Toggle activo: Image + Outline |
| `Canvas/SafeArea/SettingsPanel/SettingsCard/RoundsContainer/ToggleRounds3` | `TabInactive` · applyToImage=true | `Glow` · applyToOutline=true | Toggle inactivo: Image + Outline |
| `Canvas/SafeArea/SettingsPanel/SettingsCard/RoundsContainer/ToggleRounds5` | `TabInactive` · applyToImage=true | `Glow` · applyToOutline=true | Toggle inactivo: Image + Outline |

#### Objetos ocultos (inactivos en escena)
| Objeto (GameObject path) | Componente | Color hardcoded | ¿Necesita ThemeApplier? | Notas |
|---|---|---|---|---|
| `Canvas/SafeArea/ComboText` | TextMeshProUGUI | verde→naranja→dorado runtime | NO | `SetActive(false)` al crear; `UpdateUI()` asigna colores semánticos de combo. ThemeApplier sobreescribiría el feedback visual |
| `Canvas/SafeArea/FeedbackPanel` | Image | rgba(0,0,0,0.7) | SÍ | `SetActive(false)` al crear; activado por `ShowFeedback()`. El Overlay Image sí recibe ThemeApplier; FeedbackText NO |
| `Canvas/SafeArea/CountdownPanel` | CountdownUI | — | SÍ (hijos) | `SetActive(false)` al crear; activado por `CountdownAnimator.Play()`. Overlay + CountdownText hijos sí tienen ThemeApplier |
| `Canvas/SafeArea/SettingsPanel` | Image | rgba(0,0,0,0.9) | SÍ | `SetActive(false)` al crear; activado en modo Practice |

#### Notas de escena
- **Implementación**: Todos los ThemeApplier vía `MemoryPairsUIBuilder.cs`, NO por Inspector.
- **Card3DEffect — bloqueo intencional**: `Card3DEffect.Start()` llama `EnforceColorsNextFrame()` que reaplica `SetFaceDown()` un frame después, anulando cualquier ThemeApplier. **Esto es correcto e intencional** — los colores de las 16 cartas son 100% semánticos de gameplay (oculta/descubierta/emparejada/error). NO añadir ThemeApplier a ningún sub-elemento de las cartas.
- **UpdateToggleVisual sobreescritura**: Igual que DigitRush/FlashTap — `MemoryPairsController.UpdateToggleVisual()` sobreescribe colores de toggles con hardcoded. **Acción requerida**: refactorizar para leer `ThemeManager.Instance.CurrentTheme.tabActive`/`tabInactive`.
- **FeedbackCard applyToText=false obligatorio**: El ThemeApplier #2 (Glow en Outline) del FeedbackCard DEBE tener `applyToText=false` para no interferir con los colores semánticos que `ShowFeedback()` asigna a FeedbackText.
- **ComboText**: colores de combo (verde/naranja/dorado) son feedback de gameplay. NO añadir ThemeApplier.
- **PenaltyText runtime**: GameObject temporal creado por `AnimatePenaltyText()` con color rojo hardcoded. Es efecto transitorio de gameplay — NO requiere ThemeApplier.
- **4 iconos StatsBar BORDERLINE**: TimerIcon, RoundIcon, PairsIcon, ErrorsIcon — verificar en Inspector si sprites son 100% blancos puros con tint vía Image.color.

---

### 16 · `Games/Minigames/OddOneOut.unity` — 📝 Auditado

**Jerarquía:** Canvas → Background | SafeArea → Header / StatsBar / ComboContainer / GridsContainer (LeftGrid + RightGrid, 16 cells cada uno) / FeedbackPanel / ParticleEffects / CountdownPanel / SettingsPanel

#### Objetos a TINTAR
| Objeto (GameObject path) | Componente | ElementType | ThemeData Property | Color NeonDark | Notas |
|---|---|---|---|---|---|
| Canvas/Background | Image | `BackgroundPurple` | `backgroundPurple` | #050a14 | Fondo principal OddOneOut — usa tipo scene-specific para diferenciar entre temas |
| Canvas/SafeArea/Header | Image | `HeaderPurple` | `headerPurple` | #00000050 | Header OddOneOut — tipo scene-specific; resetear Image color a white primero |
| Canvas/SafeArea/Header/TitleText | TextMeshProUGUI | `TextTitle` | `textTitle` | #00FFFF | Título principal "ODD ONE OUT" |
| Canvas/SafeArea/StatsBar | Image | `SecondaryBackground` | `secondaryBackground` | #0d1a26 | Panel stats; DUAL — ver casos especiales |
| Canvas/SafeArea/StatsBar | Outline | `Glow` | `glowColor` | #00FFFF | Borde neón del stats bar; DUAL |
| Canvas/SafeArea/StatsBar/TimerContainer/TimerIcon | Image | `Accent` | `primaryAccent` | #00FFFF | ⚠️ BORDERLINE — imagen colored white en UIBuilder; confirmar sprite blanco puro en Inspector |
| Canvas/SafeArea/StatsBar/TimerContainer/TimerText | TextMeshProUGUI | `TextPrimary` | `textPrimary` | #FFFFFF | Texto del cronómetro "00:00" |
| Canvas/SafeArea/StatsBar/RoundContainer/RoundIcon | Image | `Accent` | `primaryAccent` | #00FFFF | ⚠️ BORDERLINE — misma verificación que TimerIcon |
| Canvas/SafeArea/StatsBar/RoundContainer/RoundText | TextMeshProUGUI | `Accent` | `primaryAccent` | #00FFFF | Texto ronda "1/5" — usa color acento cyan |
| Canvas/SafeArea/StatsBar/ErrorsContainer/ErrorsIcon | Image | `Error` | `errorColor` | #FF4D4D | ⚠️ BORDERLINE — colored rojo en UIBuilder; solo válido si sprite es blanco puro |
| Canvas/SafeArea/StatsBar/ErrorsContainer/ErrorsText | TextMeshProUGUI | `Error` | `errorColor` | #FF4D4D | Contador de errores — semánticamente siempre rojo/error; controller no sobreescribe color |
| Canvas/SafeArea/ComboContainer | Image | `CardBackground` | `cardBackground` | #1a1426 | Pastilla del combo; DUAL — ver casos especiales; SetActive(false) al inicio |
| Canvas/SafeArea/ComboContainer | Outline | `AccentTertiary` | `tertiaryAccent` | #FFD700 | Borde dorado del combo; DUAL |
| Canvas/SafeArea/GridsContainer/LeftGrid | Image | `CardBackground` | `cardBackground` | #080f1f | Grid izquierdo; DUAL — ver casos especiales |
| Canvas/SafeArea/GridsContainer/LeftGrid | Outline | `Glow` | `glowColor` | #00FFFF | Borde cyan del grid izquierdo; DUAL |
| Canvas/SafeArea/GridsContainer/LeftGrid/LeftButton_N/Face (×16) | Image | `CardBackground` | `cardBackground` | #14202e | Face de cada celda; DUAL — ver casos especiales; ⚠️ OddOneOutCell3D.ResetToNormal() sobreescribe en runtime — necesita leer ThemeManager |
| Canvas/SafeArea/GridsContainer/LeftGrid/LeftButton_N/Face (×16) | Outline | `Glow` | `glowColor` | #00FFFF | Borde cyan de cada celda izquierda; DUAL; ⚠️ OddOneOutCell3D sobreescribe con combo colors |
| Canvas/SafeArea/GridsContainer/LeftGrid/LeftButton_N/Face/LeftButtonText_N (×16) | TextMeshProUGUI | `TextPrimary` | `textPrimary` | #FFFFFF | Dígito en cada celda izquierda |
| Canvas/SafeArea/GridsContainer/RightGrid | Image | `CardBackground` | `cardBackground` | #080f1f | Grid derecho; DUAL — ver casos especiales |
| Canvas/SafeArea/GridsContainer/RightGrid | Outline | `AccentSecondary` | `secondaryAccent` | #FF00CC | Borde magenta del grid derecho; DUAL |
| Canvas/SafeArea/GridsContainer/RightGrid/RightButton_N/Face (×16) | Image | `CardBackground` | `cardBackground` | #14202e | Face de cada celda derecha; DUAL; ⚠️ mismo problema OddOneOutCell3D |
| Canvas/SafeArea/GridsContainer/RightGrid/RightButton_N/Face (×16) | Outline | `AccentSecondary` | `secondaryAccent` | #FF00CC | Borde magenta de cada celda derecha; DUAL |
| Canvas/SafeArea/GridsContainer/RightGrid/RightButton_N/Face/RightButtonText_N (×16) | TextMeshProUGUI | `TextPrimary` | `textPrimary` | #FFFFFF | Dígito en cada celda derecha |
| Canvas/SafeArea/FeedbackPanel | Image | `Overlay` | `overlayColor` | #000000B3 | Overlay fullscreen del feedback; SetActive(false) al inicio |
| Canvas/SafeArea/FeedbackPanel/FeedbackCard | Image | `CardBackground` | `cardBackground` | #0f1724 | Card central del feedback; DUAL — ver casos especiales |
| Canvas/SafeArea/FeedbackPanel/FeedbackCard | Outline | `Glow` | `glowColor` | #00FFFF | Borde neón del feedback card; DUAL |
| Canvas/SafeArea/CountdownPanel/Overlay | Image | `Overlay` | `overlayColor` | #00000099 | Overlay del countdown; CountdownPanel SetActive(false) al inicio |
| Canvas/SafeArea/CountdownPanel/NumberContainer/CountdownText | TextMeshProUGUI | `TextTitle` | `textTitle` | #00FFFF | Número de cuenta regresiva grande; DUAL — ver casos especiales |
| Canvas/SafeArea/CountdownPanel/NumberContainer/CountdownText | Outline | `Glow` | `glowColor` | #006680 | Glow del número countdown; DUAL |
| Canvas/SafeArea/SettingsPanel | Image | `Overlay` | `overlayColor` | #000000E6 | Overlay fullscreen del settings; SetActive(false) al inicio |
| Canvas/SafeArea/SettingsPanel/SettingsCard | Image | `TertiaryBackground` | `tertiaryBackground` | #0a1424 | Card central del settings panel; DUAL — ver casos especiales |
| Canvas/SafeArea/SettingsPanel/SettingsCard | Outline | `Glow` | `glowColor` | #00FFFF | Borde neón del settings card; DUAL |
| Canvas/SafeArea/SettingsPanel/SettingsCard/OddOneOutTitle | TextMeshProUGUI | `TextTitle` | `textTitle` | #00FFFF | Título del settings panel; DUAL — ver casos especiales |
| Canvas/SafeArea/SettingsPanel/SettingsCard/OddOneOutTitle | Outline | `Glow` | `glowColor` | #00668080 | Glow del título; DUAL |
| Canvas/SafeArea/SettingsPanel/SettingsCard/OddOneOutSubtitle | TextMeshProUGUI | `TextSecondary` | `textSecondary` | #808099 | Subtítulo "Find the difference!" |
| Canvas/SafeArea/SettingsPanel/SettingsCard/Divider | Image | `Accent` | `primaryAccent` | #FFFFFF1A | Línea divisora decorativa — color 10% blanco en NeonDark; resetear a white para ThemeApplier |
| Canvas/SafeArea/SettingsPanel/SettingsCard/RoundsHeader | Image | `SecondaryBackground` | `secondaryBackground` | #001f14 | Fondo del header "ROUNDS" |
| Canvas/SafeArea/SettingsPanel/SettingsCard/RoundsHeader/RoundsHeaderText | TextMeshProUGUI | `TextSecondary` | `textSecondary` | #B3FFCC | Texto "ROUNDS" — claro/hint |
| Canvas/SafeArea/SettingsPanel/SettingsCard/RoundsContainer/ToggleRounds1 | Image | `TabInactive` | `tabInactive` | #14202e | Toggle inactivo; DUAL — ver casos especiales; ⚠️ UpdateToggleVisual() sobreescribe con CYAN/BUTTON_BG hardcoded — misma bug que DigitRush/FlashTap/MemoryPairs |
| Canvas/SafeArea/SettingsPanel/SettingsCard/RoundsContainer/ToggleRounds1 | Outline | `Glow` | `glowColor` | #00B3B380 | Borde del toggle; DUAL |
| Canvas/SafeArea/SettingsPanel/SettingsCard/RoundsContainer/ToggleRounds1/Label | TextMeshProUGUI | `TextPrimary` | `textPrimary` | #FFFFFF | Texto "1" en toggle inactivo; ⚠️ UpdateToggleVisual() sobreescribe color |
| Canvas/SafeArea/SettingsPanel/SettingsCard/RoundsContainer/ToggleRounds3 | Image | `TabInactive` | `tabInactive` | #14202e | Toggle inactivo; DUAL; misma bug UpdateToggleVisual() |
| Canvas/SafeArea/SettingsPanel/SettingsCard/RoundsContainer/ToggleRounds3 | Outline | `Glow` | `glowColor` | #00B3B380 | Borde del toggle; DUAL |
| Canvas/SafeArea/SettingsPanel/SettingsCard/RoundsContainer/ToggleRounds3/Label | TextMeshProUGUI | `TextPrimary` | `textPrimary` | #FFFFFF | Texto "3" en toggle inactivo |
| Canvas/SafeArea/SettingsPanel/SettingsCard/RoundsContainer/ToggleRounds5 | Image | `TabActive` | `tabActive` | #00FFFF | Toggle activo por defecto (5 rondas); DUAL |
| Canvas/SafeArea/SettingsPanel/SettingsCard/RoundsContainer/ToggleRounds5 | Outline | `Glow` | `glowColor` | #00B3B380 | Borde del toggle activo; DUAL |
| Canvas/SafeArea/SettingsPanel/SettingsCard/RoundsContainer/ToggleRounds5/Label | TextMeshProUGUI | `TextOnPrimary` | `textOnPrimary` | #050a14 | Texto "5" sobre toggle activo (oscuro sobre cyan) |
| Canvas/SafeArea/SettingsPanel/SettingsCard/StartGameButton | Image | `ButtonSuccess` | `buttonSuccess` | #4DFF80 | Botón START verde; DUAL — ver casos especiales |
| Canvas/SafeArea/SettingsPanel/SettingsCard/StartGameButton | Outline | `ButtonGlowSuccess` | `successColor` | #1A8040 | Glow del botón START; DUAL |
| Canvas/SafeArea/SettingsPanel/SettingsCard/StartGameButton/StartText | TextMeshProUGUI | `TextOnSuccess` | `textOnSuccess` | #050a14 | Texto "START" sobre botón verde |

#### Objetos que NO se tintan
| Objeto (GameObject path) | Razón |
|---|---|
| Canvas/SafeArea | SafeAreaHandler utility — sin componente visual |
| Canvas/SafeArea/StatsBar/TimerContainer | LayoutElement — sin componente visual propio |
| Canvas/SafeArea/StatsBar/RoundContainer | LayoutElement — sin componente visual propio |
| Canvas/SafeArea/StatsBar/ErrorsContainer | LayoutElement — sin componente visual propio |
| Canvas/SafeArea/ComboContainer/ComboText | Runtime asigna colores semánticos de combo: cyan x2, verde x3, naranja x4, dorado x5+ — ThemeApplier sería sobreescrito |
| Canvas/SafeArea/GridsContainer | Container puro — sin Image component |
| Canvas/SafeArea/GridsContainer/LeftGrid/LeftButton_N (root) (×16) | Image.color = Color.clear — invisible; el Button usa Face como targetGraphic |
| Canvas/SafeArea/GridsContainer/LeftGrid/LeftButton_N/Shadow (×16) | Sombra negra decorativa — siempre debe permanecer negra; OddOneOutCell3D gestiona |
| Canvas/SafeArea/GridsContainer/LeftGrid/LeftButton_N/Side (×16) | Cara lateral 3D depth (#0a0f1a) — OddOneOutCell3D gestiona colores de profundidad |
| Canvas/SafeArea/GridsContainer/RightGrid/RightButton_N (root) (×16) | Image.color = Color.clear — mismo caso que LeftGrid |
| Canvas/SafeArea/GridsContainer/RightGrid/RightButton_N/Shadow (×16) | Sombra negra — siempre negra |
| Canvas/SafeArea/GridsContainer/RightGrid/RightButton_N/Side (×16) | Cara lateral 3D depth — OddOneOutCell3D gestiona |
| Canvas/SafeArea/FeedbackPanel/FeedbackCard/FeedbackText | Runtime asigna colores semánticos: verde (#4DFF80) = correcto, rojo (#FF4D4D) = incorrecto — no debe ser sobreescrito |
| Canvas/SafeArea/ParticleEffects | UISparkleEffect — partículas usan confettiPalette, sin Image |
| Canvas/SafeArea/CountdownPanel/NumberContainer | Container puro — sin Image |
| Canvas/SafeArea/SettingsPanel/SettingsCard/RoundsContainer | LayoutGroup + ToggleGroup — sin Image |
| Canvas/SafeArea/SettingsPanel/SettingsCard/StartGameButton/Shadow | Sombra 3D decorativa del botón (dark green 60%) — elemento de profundidad, siempre debe ser oscuro |
| PenaltyText (runtime) | TMP "+1" rojo creado en runtime vía Instantiate/AddComponent, auto-destruido tras animación — no puede tener ThemeApplier |

#### Casos especiales (2 ThemeAppliers en mismo objeto)
| Objeto | ThemeApplier #1 | ThemeApplier #2 | Razón |
|---|---|---|---|
| Canvas/SafeArea/StatsBar | `SecondaryBackground` applyToImage=true | `Glow` applyToOutline=true | Panel con borde neón |
| Canvas/SafeArea/ComboContainer | `CardBackground` applyToImage=true | `AccentTertiary` applyToOutline=true | Pastilla combo: fondo card + borde dorado |
| Canvas/SafeArea/GridsContainer/LeftGrid | `CardBackground` applyToImage=true | `Glow` applyToOutline=true | Grid izquierdo: fondo + borde cyan |
| Canvas/SafeArea/GridsContainer/RightGrid | `CardBackground` applyToImage=true | `AccentSecondary` applyToOutline=true | Grid derecho: fondo + borde magenta (accent secundario) |
| Canvas/SafeArea/GridsContainer/LeftGrid/LeftButton_N/Face (×16) | `CardBackground` applyToImage=true | `Glow` applyToOutline=true | Cara izquierda: fondo + borde cyan |
| Canvas/SafeArea/GridsContainer/RightGrid/RightButton_N/Face (×16) | `CardBackground` applyToImage=true | `AccentSecondary` applyToOutline=true | Cara derecha: fondo + borde magenta |
| Canvas/SafeArea/FeedbackPanel/FeedbackCard | `CardBackground` applyToImage=true | `Glow` applyToOutline=true | Card feedback: fondo + borde neón |
| Canvas/SafeArea/CountdownPanel/NumberContainer/CountdownText | `TextTitle` applyToText=true | `Glow` applyToOutline=true | Número grande + glow |
| Canvas/SafeArea/SettingsPanel/SettingsCard | `TertiaryBackground` applyToImage=true | `Glow` applyToOutline=true | Card settings: fondo elevado + borde |
| Canvas/SafeArea/SettingsPanel/SettingsCard/OddOneOutTitle | `TextTitle` applyToText=true | `Glow` applyToOutline=true | Título settings + glow decorativo |
| Canvas/SafeArea/SettingsPanel/SettingsCard/RoundsContainer/ToggleRounds1 | `TabInactive` applyToImage=true | `Glow` applyToOutline=true | Toggle inactivo + borde |
| Canvas/SafeArea/SettingsPanel/SettingsCard/RoundsContainer/ToggleRounds3 | `TabInactive` applyToImage=true | `Glow` applyToOutline=true | Toggle inactivo + borde |
| Canvas/SafeArea/SettingsPanel/SettingsCard/RoundsContainer/ToggleRounds5 | `TabActive` applyToImage=true | `Glow` applyToOutline=true | Toggle activo (default=5) + borde |
| Canvas/SafeArea/SettingsPanel/SettingsCard/StartGameButton | `ButtonSuccess` applyToImage=true | `ButtonGlowSuccess` applyToOutline=true | Botón verde + glow success |

#### Objetos ocultos (inactivos en escena)
| Objeto | Motivo de ocultación | Color inicial | ThemeApplier | Notas |
|---|---|---|---|---|
| Canvas/SafeArea/ComboContainer | SetActive(false) + CanvasGroup.alpha=0 | Image: #1a142699 dark purple | Sí — `CardBackground`+`AccentTertiary` | Activado por controller cuando combo ≥ 2 |
| Canvas/SafeArea/FeedbackPanel | SetActive(false) | Image: black 70% | Sí — `Overlay` | Activado tras cada respuesta (correcto/incorrecto) |
| Canvas/SafeArea/CountdownPanel | SetActive(false) | — | Sí — Overlay+CountdownText | Activado al iniciar partida; CountdownUI gestiona show/hide |
| Canvas/SafeArea/SettingsPanel | SetActive(false) | Image: black 90% | Sí — `Overlay` | Solo visible en Practice Mode; primer objeto visible al cargar |

#### Notas de escena
- **OddOneOut usa `BackgroundPurple` / `HeaderPurple`** — ElementTypes scene-specific; permiten dar a OddOneOut identidad visual propia en temas distintos de NeonDark
- **OddOneOut usa `AccentSecondary` (magenta)** para el grid derecho — diferencia visual izquierda=cyan / derecha=magenta; mantener esta distinción en ThemeApplier
- **Systemic bug `UpdateToggleVisual()`** — mismo problema que DigitRush, FlashTap, MemoryPairs: sobreescribe colores de toggle con hardcoded CYAN_NEON/BUTTON_BG. Refactor pendiente: leer `ThemeManager.Instance.CurrentTheme.tabActive/tabInactive`
- **OddOneOutCell3D sobreescribe colores de celda en runtime**: `ResetToNormal()` restaura `faceImage.color = normalFaceColor` (hardcoded BUTTON_BG), `AnimateCorrect()` pone colores de combo, `AnimateError()` pone rojo. Necesita ThemeManager refactor igual que Cell3DButton en DigitRush
- **ProgressFill / RoundIndicator ausentes del UIBuilder** — Controller los referencia pero UIBuilder no los crea; referencias null en runtime (sin crash por null-checks)
- **PenaltyText runtime** — "+1" rojo creado dinámicamente, no puede recibir ThemeApplier; color semántico correcto
- **StartGameButton/Shadow** — sombra 3D del botón (dark green), NO ThemeApplier; es un efecto de profundidad estático
- **3 iconos BORDERLINE** (TimerIcon, RoundIcon, ErrorsIcon): mismos iconos que otras minigames — verificar en Inspector que sprite sea blanco puro (RGBA 255,255,255,255)

---

### 17 · `Games/Minigames/QuickMath.unity` — 📝 Auditado

**Jerarquía:** Canvas → Background | SafeArea → Header / StatsBar / ComboContainer / EquationPanel / AnswersContainer (3 botones) / FeedbackPanel / ParticleEffects / SettingsPanel (Operations + Difficulty + Rounds)

#### Objetos a TINTAR
| Objeto (GameObject path) | Componente | ElementType | ThemeData Property | Color NeonDark | Notas |
|---|---|---|---|---|---|
| Canvas/Background | Image | `BackgroundNavy` | `backgroundNavy` | #050a14 | Fondo principal QuickMath — scene-specific para identidad visual propia |
| Canvas/SafeArea/Header | Image | `HeaderNavy` | `headerNavy` | #00000050 | Header QuickMath — scene-specific; resetear Image color a white primero |
| Canvas/SafeArea/Header/TitleText | TextMeshProUGUI | `TextTitle` | `textTitle` | #00FFFF | Título "QUICK MATH"; DUAL — ver casos especiales |
| Canvas/SafeArea/Header/TitleText | Outline | `Glow` | `glowColor` | #00FFFF80 | Glow del título; DUAL |
| Canvas/SafeArea/StatsBar | Image | `SecondaryBackground` | `secondaryBackground` | #0d1a26 | Panel stats; DUAL — ver casos especiales |
| Canvas/SafeArea/StatsBar | Outline | `Glow` | `glowColor` | #00FFFF | Borde neón del stats bar; DUAL |
| Canvas/SafeArea/StatsBar/TimerContainer/TimerIcon | Image | `Accent` | `primaryAccent` | #FFFFFF | ⚠️ BORDERLINE — Image.color=white en UIBuilder; confirmar sprite blanco puro en Inspector |
| Canvas/SafeArea/StatsBar/TimerContainer/TimerText | TextMeshProUGUI | `TextPrimary` | `textPrimary` | #FFFFFF | Cronómetro "00:00" |
| Canvas/SafeArea/StatsBar/RoundContainer/RoundIcon | Image | `Accent` | `primaryAccent` | #00FFFF | ⚠️ BORDERLINE — misma verificación que TimerIcon |
| Canvas/SafeArea/StatsBar/RoundContainer/RoundText | TextMeshProUGUI | `Accent` | `primaryAccent` | #00FFFF | Texto ronda "1/10" |
| Canvas/SafeArea/StatsBar/ErrorsContainer/ErrorsIcon | Image | `Error` | `errorColor` | #FF4D4D | ⚠️ BORDERLINE — colored rojo; solo válido si sprite blanco puro |
| Canvas/SafeArea/StatsBar/ErrorsContainer/ErrorsText | TextMeshProUGUI | `Error` | `errorColor` | #FF4D4D | Contador errores — semánticamente siempre rojo; controller no sobreescribe color |
| Canvas/SafeArea/ComboContainer | Image | `CardBackground` | `cardBackground` | #1a0d26 | Pastilla combo/streak; solo 1 ThemeApplier (Outline es ORANGE_NEON semántico — NO se tinta); SetActive(false) |
| Canvas/SafeArea/EquationPanel/PanelFace | Image | `CardBackground` | `cardBackground` | #0a1426 | Face del panel ecuación 3D; DUAL — ver casos especiales |
| Canvas/SafeArea/EquationPanel/PanelFace | Outline | `Glow` | `glowColor` | #00FFFF | Borde neón del panel ecuación; DUAL |
| Canvas/SafeArea/EquationPanel/PanelFace/EquationContainer/NumberA | TextMeshProUGUI | `TextPrimary` | `textPrimary` | #FFFFFF | Primer operando "?" — siempre blanco |
| Canvas/SafeArea/EquationPanel/PanelFace/EquationContainer/OperatorText | TextMeshProUGUI | `Accent` | `primaryAccent` | #00FFFF | Operador (+/-/×/÷) — siempre cyan; controller solo cambia .text, no .color |
| Canvas/SafeArea/EquationPanel/PanelFace/EquationContainer/NumberB | TextMeshProUGUI | `TextPrimary` | `textPrimary` | #FFFFFF | Segundo operando "?" — siempre blanco |
| Canvas/SafeArea/AnswersContainer/AnswerButton_N/Face (×3) | Image | `CardBackground` | `cardBackground` | #14202e | Face de cada botón respuesta; DUAL — ver casos especiales; ⚠️ QuickMathCell3D sobreescribe en runtime |
| Canvas/SafeArea/AnswersContainer/AnswerButton_N/Face (×3) | Outline | `Glow` | `glowColor` | #00FFFF | Borde cyan de cada botón; DUAL; ⚠️ QuickMathCell3D sobreescribe con correct/error colors |
| Canvas/SafeArea/AnswersContainer/AnswerButton_N/Face/AnswerText_N (×3) | TextMeshProUGUI | `Accent` | `primaryAccent` | #00FFFF | Texto respuesta "?" — cyan; DUAL — ver casos especiales; ⚠️ QuickMathCell3D puede sobreescribir |
| Canvas/SafeArea/AnswersContainer/AnswerButton_N/Face/AnswerText_N (×3) | Outline | `Glow` | `glowColor` | #00808080 | Glow decorativo del texto respuesta; DUAL |
| Canvas/SafeArea/FeedbackPanel | Image | `Overlay` | `overlayColor` | #000000B3 | Overlay fullscreen del feedback; SetActive(false) |
| Canvas/SafeArea/FeedbackPanel/FeedbackCard | Image | `CardBackground` | `cardBackground` | #0f1724 | Card del feedback; DUAL — ver casos especiales |
| Canvas/SafeArea/FeedbackPanel/FeedbackCard | Outline | `Glow` | `glowColor` | #00FFFF | Borde neón del feedback card; DUAL |
| Canvas/SafeArea/SettingsPanel | Image | `Overlay` | `overlayColor` | #000000E6 | Overlay fullscreen settings; SetActive(false) — primer objeto visible en Practice Mode |
| Canvas/SafeArea/SettingsPanel/SettingsCard | Image | `TertiaryBackground` | `tertiaryBackground` | #0a1424 | Card elevada del settings; DUAL — ver casos especiales |
| Canvas/SafeArea/SettingsPanel/SettingsCard | Outline | `Glow` | `glowColor` | #00FFFF | Borde neón del settings card; DUAL |
| Canvas/SafeArea/SettingsPanel/SettingsCard/QuickMathTitle | TextMeshProUGUI | `TextTitle` | `textTitle` | #00FFFF | Título settings "QUICK MATH"; DUAL — ver casos especiales |
| Canvas/SafeArea/SettingsPanel/SettingsCard/QuickMathTitle | Outline | `Glow` | `glowColor` | #00808099 | Glow del título settings; DUAL |
| Canvas/SafeArea/SettingsPanel/SettingsCard/QuickMathSubtitle | TextMeshProUGUI | `TextSecondary` | `textSecondary` | #808099 | Subtítulo "Configure your game" |
| Canvas/SafeArea/SettingsPanel/SettingsCard/Divider (×3) | Image | `Accent` | `primaryAccent` | #FFFFFF1A | Líneas divisoras decorativas (3 instancias); resetear a white para ThemeApplier |
| Canvas/SafeArea/SettingsPanel/SettingsCard/OperationsHeader | Image | `SecondaryBackground` | `secondaryBackground` | #002633 | Fondo header "OPERATIONS" (dark teal 50%) |
| Canvas/SafeArea/SettingsPanel/SettingsCard/OperationsHeader/OperationsHeaderText | TextMeshProUGUI | `TextSecondary` | `textSecondary` | #B3E6FF | Texto "OPERATIONS" (light blue) |
| Canvas/SafeArea/SettingsPanel/SettingsCard/OperationsContainer/ToggleAddition | Image | `TabActive` | `tabActive` | #00FFFF | Toggle "+" activo por defecto; DUAL — ver casos especiales; ⚠️ UpdateToggleVisual() bug |
| Canvas/SafeArea/SettingsPanel/SettingsCard/OperationsContainer/ToggleAddition | Outline | `Glow` | `glowColor` | #00B3B380 | Borde toggle; DUAL |
| Canvas/SafeArea/SettingsPanel/SettingsCard/OperationsContainer/ToggleAddition/Label | TextMeshProUGUI | `TextOnPrimary` | `textOnPrimary` | #050a14 | Texto "+" sobre toggle activo |
| Canvas/SafeArea/SettingsPanel/SettingsCard/OperationsContainer/ToggleSubtraction | Image | `TabActive` | `tabActive` | #00FFFF | Toggle "-" activo por defecto; DUAL; ⚠️ UpdateToggleVisual() bug |
| Canvas/SafeArea/SettingsPanel/SettingsCard/OperationsContainer/ToggleSubtraction | Outline | `Glow` | `glowColor` | #00B3B380 | Borde toggle; DUAL |
| Canvas/SafeArea/SettingsPanel/SettingsCard/OperationsContainer/ToggleSubtraction/Label | TextMeshProUGUI | `TextOnPrimary` | `textOnPrimary` | #050a14 | Texto "-" sobre toggle activo |
| Canvas/SafeArea/SettingsPanel/SettingsCard/OperationsContainer/ToggleMultiplication | Image | `TabInactive` | `tabInactive` | #14202e | Toggle "×" inactivo; DUAL; ⚠️ UpdateToggleVisual() bug; disabled en Easy mode por runtime |
| Canvas/SafeArea/SettingsPanel/SettingsCard/OperationsContainer/ToggleMultiplication | Outline | `Glow` | `glowColor` | #00B3B380 | Borde toggle; DUAL |
| Canvas/SafeArea/SettingsPanel/SettingsCard/OperationsContainer/ToggleMultiplication/Label | TextMeshProUGUI | `TextPrimary` | `textPrimary` | #FFFFFF | Texto "×" en toggle inactivo |
| Canvas/SafeArea/SettingsPanel/SettingsCard/OperationsContainer/ToggleDivision | Image | `TabInactive` | `tabInactive` | #14202e | Toggle "÷" inactivo; DUAL; ⚠️ UpdateToggleVisual() bug; disabled en Easy mode |
| Canvas/SafeArea/SettingsPanel/SettingsCard/OperationsContainer/ToggleDivision | Outline | `Glow` | `glowColor` | #00B3B380 | Borde toggle; DUAL |
| Canvas/SafeArea/SettingsPanel/SettingsCard/OperationsContainer/ToggleDivision/Label | TextMeshProUGUI | `TextPrimary` | `textPrimary` | #FFFFFF | Texto "÷" en toggle inactivo |
| Canvas/SafeArea/SettingsPanel/SettingsCard/DifficultyHeader | Image | `SecondaryBackground` | `secondaryBackground` | #261A00 | Fondo header "DIFFICULTY" (dark amber 50%) |
| Canvas/SafeArea/SettingsPanel/SettingsCard/DifficultyHeader/DifficultyHeaderText | TextMeshProUGUI | `TextSecondary` | `textSecondary` | #FFE5B3 | Texto "DIFFICULTY" (warm yellow) |
| Canvas/SafeArea/SettingsPanel/SettingsCard/DifficultyContainer/ToggleEasy | Image | `TabInactive` | `tabInactive` | #14202e | Toggle "EASY" inactivo; DUAL; ⚠️ UpdateToggleVisual() bug |
| Canvas/SafeArea/SettingsPanel/SettingsCard/DifficultyContainer/ToggleEasy | Outline | `Glow` | `glowColor` | #00B3B380 | Borde toggle; DUAL |
| Canvas/SafeArea/SettingsPanel/SettingsCard/DifficultyContainer/ToggleEasy/Label | TextMeshProUGUI | `TextPrimary` | `textPrimary` | #FFFFFF | Texto "EASY" en toggle inactivo |
| Canvas/SafeArea/SettingsPanel/SettingsCard/DifficultyContainer/ToggleNormal | Image | `TabActive` | `tabActive` | #00FFFF | Toggle "NORMAL" activo por defecto; DUAL; ⚠️ UpdateToggleVisual() bug |
| Canvas/SafeArea/SettingsPanel/SettingsCard/DifficultyContainer/ToggleNormal | Outline | `Glow` | `glowColor` | #00B3B380 | Borde toggle; DUAL |
| Canvas/SafeArea/SettingsPanel/SettingsCard/DifficultyContainer/ToggleNormal/Label | TextMeshProUGUI | `TextOnPrimary` | `textOnPrimary` | #050a14 | Texto "NORMAL" sobre toggle activo |
| Canvas/SafeArea/SettingsPanel/SettingsCard/DifficultyContainer/ToggleHard | Image | `TabInactive` | `tabInactive` | #14202e | Toggle "HARD" inactivo; DUAL; ⚠️ UpdateToggleVisual() bug |
| Canvas/SafeArea/SettingsPanel/SettingsCard/DifficultyContainer/ToggleHard | Outline | `Glow` | `glowColor` | #00B3B380 | Borde toggle; DUAL |
| Canvas/SafeArea/SettingsPanel/SettingsCard/DifficultyContainer/ToggleHard/Label | TextMeshProUGUI | `TextPrimary` | `textPrimary` | #FFFFFF | Texto "HARD" en toggle inactivo |
| Canvas/SafeArea/SettingsPanel/SettingsCard/DifficultyDescText | TextMeshProUGUI | `TextSecondary` | `textSecondary` | #808099 | Descripción dificultad — controller cambia .text pero NO .color; ThemeApplier seguro |
| Canvas/SafeArea/SettingsPanel/SettingsCard/RoundsHeader | Image | `SecondaryBackground` | `secondaryBackground` | #001f14 | Fondo header "ROUNDS" (dark green 50%) |
| Canvas/SafeArea/SettingsPanel/SettingsCard/RoundsHeader/RoundsHeaderText | TextMeshProUGUI | `TextSecondary` | `textSecondary` | #B3FFCC | Texto "ROUNDS" (light green) |
| Canvas/SafeArea/SettingsPanel/SettingsCard/RoundsContainer/ToggleRounds1 | Image | `TabInactive` | `tabInactive` | #14202e | Toggle "1" inactivo; DUAL; ⚠️ UpdateToggleVisual() bug |
| Canvas/SafeArea/SettingsPanel/SettingsCard/RoundsContainer/ToggleRounds1 | Outline | `Glow` | `glowColor` | #00B3B380 | Borde toggle; DUAL |
| Canvas/SafeArea/SettingsPanel/SettingsCard/RoundsContainer/ToggleRounds1/Label | TextMeshProUGUI | `TextPrimary` | `textPrimary` | #FFFFFF | Texto "1" en toggle inactivo |
| Canvas/SafeArea/SettingsPanel/SettingsCard/RoundsContainer/ToggleRounds3 | Image | `TabInactive` | `tabInactive` | #14202e | Toggle "3" inactivo; DUAL; ⚠️ UpdateToggleVisual() bug |
| Canvas/SafeArea/SettingsPanel/SettingsCard/RoundsContainer/ToggleRounds3 | Outline | `Glow` | `glowColor` | #00B3B380 | Borde toggle; DUAL |
| Canvas/SafeArea/SettingsPanel/SettingsCard/RoundsContainer/ToggleRounds3/Label | TextMeshProUGUI | `TextPrimary` | `textPrimary` | #FFFFFF | Texto "3" en toggle inactivo |
| Canvas/SafeArea/SettingsPanel/SettingsCard/RoundsContainer/ToggleRounds5 | Image | `TabActive` | `tabActive` | #00FFFF | Toggle "5" activo por defecto; DUAL; ⚠️ UpdateToggleVisual() bug |
| Canvas/SafeArea/SettingsPanel/SettingsCard/RoundsContainer/ToggleRounds5 | Outline | `Glow` | `glowColor` | #00B3B380 | Borde toggle; DUAL |
| Canvas/SafeArea/SettingsPanel/SettingsCard/RoundsContainer/ToggleRounds5/Label | TextMeshProUGUI | `TextOnPrimary` | `textOnPrimary` | #050a14 | Texto "5" sobre toggle activo |
| Canvas/SafeArea/SettingsPanel/SettingsCard/StartGameButton | Image | `ButtonSuccess` | `buttonSuccess` | #4DFF80 | Botón START verde; DUAL — ver casos especiales |
| Canvas/SafeArea/SettingsPanel/SettingsCard/StartGameButton | Outline | `ButtonGlowSuccess` | `successColor` | #1A8040 | Glow botón START; DUAL |
| Canvas/SafeArea/SettingsPanel/SettingsCard/StartGameButton/StartText | TextMeshProUGUI | `TextOnSuccess` | `textOnSuccess` | #050a14 | Texto "START" sobre botón verde |

#### Objetos que NO se tintan
| Objeto (GameObject path) | Razón |
|---|---|
| Canvas/SafeArea | SafeAreaHandler utility — sin componente visual |
| Canvas/SafeArea/StatsBar/TimerContainer | LayoutElement — sin componente visual propio |
| Canvas/SafeArea/StatsBar/RoundContainer | LayoutElement — sin componente visual propio |
| Canvas/SafeArea/StatsBar/ErrorsContainer | LayoutElement — sin componente visual propio |
| Canvas/SafeArea/ComboContainer/StreakIcon | Image (ORANGE_NEON, sin sprite) — color semántico de streak; siempre naranja cuando activo |
| Canvas/SafeArea/ComboContainer/ComboText | Runtime asigna colores semánticos: naranja (streak normal), dorado (streak alto) — ThemeApplier sería sobreescrito |
| Canvas/SafeArea/EquationPanel | Container RectTransform puro — sin Image component |
| Canvas/SafeArea/EquationPanel/PanelShadow | Sombra negra decorativa — siempre debe permanecer negra; raycastTarget=false |
| Canvas/SafeArea/EquationPanel/PanelSide | Cara lateral 3D depth (#004D59 dark teal) — elemento de profundidad, gestionado visualmente como panel 3D; raycastTarget=false |
| Canvas/SafeArea/EquationPanel/PanelFace/EquationContainer | HorizontalLayoutGroup container — sin Image |
| Canvas/SafeArea/EquationPanel/ProblemText | TMP hidden: Color.clear + enabled=false — elemento de compatibilidad, nunca visible |
| Canvas/SafeArea/AnswersContainer | HorizontalLayoutGroup container — sin Image |
| Canvas/SafeArea/AnswersContainer/AnswerButton_N (root) (×3) | Button component sin Image propia; targetGraphic apunta a Face child — sin visual propio |
| Canvas/SafeArea/AnswersContainer/AnswerButton_N/Shadow (×3) | Sombra negra decorativa — siempre negra; QuickMathCell3D gestiona |
| Canvas/SafeArea/AnswersContainer/AnswerButton_N/Side (×3) | Cara lateral 3D depth (#004D59) — mismo patrón que EquationPanel/PanelSide |
| Canvas/SafeArea/FeedbackPanel/FeedbackCard/FeedbackText | Runtime asigna semánticos: verde (correcto), rojo (incorrecto) — no debe sobreescribirse |
| Canvas/SafeArea/ParticleEffects | UISparkleEffect — partículas usan confettiPalette, sin Image |
| Canvas/SafeArea/SettingsPanel/SettingsCard/OperationsContainer | HorizontalLayoutGroup + ToggleGroup — sin Image |
| Canvas/SafeArea/SettingsPanel/SettingsCard/DifficultyContainer | HorizontalLayoutGroup + ToggleGroup — sin Image |
| Canvas/SafeArea/SettingsPanel/SettingsCard/RoundsContainer | HorizontalLayoutGroup + ToggleGroup — sin Image |
| Canvas/SafeArea/SettingsPanel/SettingsCard/StartGameButton/Shadow | Sombra 3D decorativa del botón (dark green 60%) — elemento de profundidad, siempre oscuro |
| PenaltyText (runtime) | TMP "+1s" rojo creado en runtime, auto-destruido — no puede tener ThemeApplier |

#### Casos especiales (2 ThemeAppliers en mismo objeto)
| Objeto | ThemeApplier #1 | ThemeApplier #2 | Razón |
|---|---|---|---|
| Canvas/SafeArea/Header/TitleText | `TextTitle` applyToText=true | `Glow` applyToOutline=true | Título + glow decorativo |
| Canvas/SafeArea/StatsBar | `SecondaryBackground` applyToImage=true | `Glow` applyToOutline=true | Panel con borde neón |
| Canvas/SafeArea/EquationPanel/PanelFace | `CardBackground` applyToImage=true | `Glow` applyToOutline=true | Panel ecuación: fondo + borde |
| Canvas/SafeArea/AnswersContainer/AnswerButton_N/Face (×3) | `CardBackground` applyToImage=true | `Glow` applyToOutline=true | Face botón respuesta: fondo + borde cyan |
| Canvas/SafeArea/AnswersContainer/AnswerButton_N/Face/AnswerText_N (×3) | `Accent` applyToText=true | `Glow` applyToOutline=true | Texto respuesta cyan + glow |
| Canvas/SafeArea/FeedbackPanel/FeedbackCard | `CardBackground` applyToImage=true | `Glow` applyToOutline=true | Card feedback: fondo + borde |
| Canvas/SafeArea/SettingsPanel/SettingsCard | `TertiaryBackground` applyToImage=true | `Glow` applyToOutline=true | Card settings elevada + borde |
| Canvas/SafeArea/SettingsPanel/SettingsCard/QuickMathTitle | `TextTitle` applyToText=true | `Glow` applyToOutline=true | Título settings + glow |
| Canvas/SafeArea/SettingsPanel/SettingsCard/OperationsContainer/ToggleAddition | `TabActive` applyToImage=true | `Glow` applyToOutline=true | Toggle activo (+) + borde |
| Canvas/SafeArea/SettingsPanel/SettingsCard/OperationsContainer/ToggleSubtraction | `TabActive` applyToImage=true | `Glow` applyToOutline=true | Toggle activo (-) + borde |
| Canvas/SafeArea/SettingsPanel/SettingsCard/OperationsContainer/ToggleMultiplication | `TabInactive` applyToImage=true | `Glow` applyToOutline=true | Toggle inactivo (×) + borde |
| Canvas/SafeArea/SettingsPanel/SettingsCard/OperationsContainer/ToggleDivision | `TabInactive` applyToImage=true | `Glow` applyToOutline=true | Toggle inactivo (÷) + borde |
| Canvas/SafeArea/SettingsPanel/SettingsCard/DifficultyContainer/ToggleEasy | `TabInactive` applyToImage=true | `Glow` applyToOutline=true | Toggle inactivo (EASY) + borde |
| Canvas/SafeArea/SettingsPanel/SettingsCard/DifficultyContainer/ToggleNormal | `TabActive` applyToImage=true | `Glow` applyToOutline=true | Toggle activo (NORMAL) + borde |
| Canvas/SafeArea/SettingsPanel/SettingsCard/DifficultyContainer/ToggleHard | `TabInactive` applyToImage=true | `Glow` applyToOutline=true | Toggle inactivo (HARD) + borde |
| Canvas/SafeArea/SettingsPanel/SettingsCard/RoundsContainer/ToggleRounds1 | `TabInactive` applyToImage=true | `Glow` applyToOutline=true | Toggle inactivo (1) + borde |
| Canvas/SafeArea/SettingsPanel/SettingsCard/RoundsContainer/ToggleRounds3 | `TabInactive` applyToImage=true | `Glow` applyToOutline=true | Toggle inactivo (3) + borde |
| Canvas/SafeArea/SettingsPanel/SettingsCard/RoundsContainer/ToggleRounds5 | `TabActive` applyToImage=true | `Glow` applyToOutline=true | Toggle activo (5) + borde |
| Canvas/SafeArea/SettingsPanel/SettingsCard/StartGameButton | `ButtonSuccess` applyToImage=true | `ButtonGlowSuccess` applyToOutline=true | Botón verde + glow success |

#### Objetos ocultos (inactivos en escena)
| Objeto | Motivo de ocultación | Color inicial | ThemeApplier | Notas |
|---|---|---|---|---|
| Canvas/SafeArea/ComboContainer | SetActive(false) + CanvasGroup.alpha=0 | Image: #1a0d26 dark purple | Sí — `CardBackground` | Activado por controller cuando streak activo |
| Canvas/SafeArea/FeedbackPanel | SetActive(false) | Image: black 70% | Sí — `Overlay` | Activado tras cada respuesta |
| Canvas/SafeArea/SettingsPanel | SetActive(false) | Image: black 90% | Sí — `Overlay` | Solo visible en Practice Mode; primer objeto visible al cargar |

#### Notas de escena
- **QuickMath usa `BackgroundNavy` / `HeaderNavy`** — ElementTypes scene-specific; permiten dar a QuickMath identidad visual propia en temas distintos de NeonDark
- **ComboContainer: solo 1 ThemeApplier** — a diferencia de OddOneOut, el Outline de ComboContainer es ORANGE_NEON semántico (color de streak) — NO usar ThemeApplier en el Outline
- **EquationPanel 3D**: PanelFace recibe ThemeApplier; PanelSide y PanelShadow son elementos de profundidad 3D estáticos — NO ThemeApplier
- **Mismo patrón 3D en AnswerButtons**: AnswerButton_N/Face → ThemeApplier; Side y Shadow → NO (profundidad)
- **Systemic bug `UpdateToggleVisual()`** — 5ª escena consecutiva: DigitRush, FlashTap, MemoryPairs, OddOneOut, QuickMath. Afecta a **10 toggles** aquí: Add, Sub, Mult, Div, Easy, Normal, Hard, R1, R3, R5. Hardcoded CYAN_NEON/BUTTON_BG sobreescribe ThemeApplier en cada interacción
- **`ToggleMultiplication` / `ToggleDivision` disabled en Easy mode** — controller llama `.interactable = false` cuando dificultad=Easy. ThemeApplier se aplica igual (OnEnable) — no hay conflicto
- **QuickMathCell3D sobreescribe colores de botones** — igual que OddOneOutCell3D. Necesita ThemeManager refactor para leer `faceColor`/`glowColor` del tema actual
- **`DifficultyDescText` SEGURO para ThemeApplier** — controller cambia `.text` pero NUNCA `.color`. Color permanece gray/textSecondary durante todo el gameplay
- **`ProblemText` permanentemente oculto** — TMP con `enabled=false` y `Color.clear`; es un campo de compatibilidad. NO ThemeApplier
- **No hay CountdownPanel en UIBuilder** — countdown usa `CountdownAnimator.Play(canvas, ...)` que es un helper compartido; no crea objetos persistentes en esta escena
- **3 iconos BORDERLINE** (TimerIcon, RoundIcon, ErrorsIcon): mismos sprites que otras minigames — verificar en Inspector que sean blancos puros (RGBA 255,255,255,255)

---

### 18 · `Social/Profile/Profile.unity` — 📝 V52

#### Objetos a TINTAR
| Objeto (GameObject path) | Componente | ElementType | ThemeData Property | Color NeonDark | Notas |
|---|---|---|---|---|---|
| `Canvas/Background` | Image | `PrimaryBackground` | `primaryBackground` | #0D0D1A | Reset Image.color a white |
| `Canvas/Header` | Image | `SecondaryBackground` | `secondaryBackground` | #1A1A26 | Reset Image.color a white |
| `Canvas/Header/TitleText` | TextMeshProUGUI | `Accent` | `primaryAccent` | #00FFFF | Reset TMP.color a white |
| `Canvas/AvatarCard` | Image | `CardBackground` | `cardBackground` | #1F1F2E | Dual con Outline; reset Image.color a white |
| `Canvas/AvatarCard` | Outline | `Glow` | `glowColor` | #00FFFF80 | Dual con Image; reset effectColor a white |
| `Canvas/AvatarCard/GlowRing` | Image | `Glow` | `glowColor` | #00FFFF80 | Reset Image.color a white |
| `Canvas/AvatarCard/AvatarMask` | Image | `SecondaryBackground` | `secondaryBackground` | #1A1A26 | Reset Image.color a white |
| `Canvas/AvatarCard/EditButton` | Image | `ButtonSecondary` | `buttonSecondary` | #333340 | Dual con Outline; reset Image.color a white |
| `Canvas/AvatarCard/EditButton` | Outline | `Glow` | `glowColor` | #00FFFF80 | Dual con Image; reset effectColor a white |
| `Canvas/AvatarCard/EditButton/Icon` | Image | `Accent` | `primaryAccent` | #00FFFF | ⚠️BORDERLINE — sprite 100% blanco puro; tintado opcional; reset Image.color a white |
| `Canvas/AvatarCard/UsernameText` | TextMeshProUGUI | `TextPrimary` | `textPrimary` | #FFFFFF | Reset TMP.color a white |
| `Canvas/GeneralStatsCard` | Image | `CardBackground` | `cardBackground` | #1F1F2E | Dual con Outline; reset Image.color a white |
| `Canvas/GeneralStatsCard` | Outline | `Glow` | `glowColor` | #00FFFF80 | Dual con Image; reset effectColor a white |
| `Canvas/GeneralStatsCard/Title/LeftLine` | Image | `Accent` | `primaryAccent` | #00FFFF | Reset Image.color a white |
| `Canvas/GeneralStatsCard/Title/TitleText` | TextMeshProUGUI | `Accent` | `primaryAccent` | #00FFFF | Reset TMP.color a white |
| `Canvas/GeneralStatsCard/Title/RightLine` | Image | `Accent` | `primaryAccent` | #00FFFF | Reset Image.color a white |
| `Canvas/GeneralStatsCard/TotalGamesBlock` | Image | `SecondaryBackground` | `secondaryBackground` | #1A1A26 | Reset Image.color a white |
| `Canvas/GeneralStatsCard/TotalGamesBlock/Value` | TextMeshProUGUI | `Accent` | `primaryAccent` | #00FFFF | ⚠️BORDERLINE — no hay ElementType perfecto para count stat; reset TMP.color a white |
| `Canvas/GeneralStatsCard/TotalGamesBlock/Label` | TextMeshProUGUI | `TextSecondary` | `textSecondary` | #B3B3B3 | Reset TMP.color a white |
| `Canvas/GeneralStatsCard/WinsBlock` | Image | `SecondaryBackground` | `secondaryBackground` | #1A1A26 | Reset Image.color a white |
| `Canvas/GeneralStatsCard/WinsBlock/Value` | TextMeshProUGUI | `Success` | `successColor` | #4DFF4D | ⚠️BORDERLINE — verde éxito; reset TMP.color a white |
| `Canvas/GeneralStatsCard/WinsBlock/Label` | TextMeshProUGUI | `TextSecondary` | `textSecondary` | #B3B3B3 | Reset TMP.color a white |
| `Canvas/GeneralStatsCard/WinRateBlock` | Image | `SecondaryBackground` | `secondaryBackground` | #1A1A26 | Reset Image.color a white |
| `Canvas/GeneralStatsCard/WinRateBlock/Value` | TextMeshProUGUI | `AccentTertiary` | `tertiaryAccent` | #FFD700 | ⚠️BORDERLINE — gold; reset TMP.color a white |
| `Canvas/GeneralStatsCard/WinRateBlock/Label` | TextMeshProUGUI | `TextSecondary` | `textSecondary` | #B3B3B3 | Reset TMP.color a white |
| `Canvas/GeneralStatsCard/BestTimeBlock` | Image | `SecondaryBackground` | `secondaryBackground` | #1A1A26 | Reset Image.color a white |
| `Canvas/GeneralStatsCard/BestTimeBlock/Value` | TextMeshProUGUI | `Warning` | `warningColor` | #FFB333 | ⚠️BORDERLINE — naranja warning ≈ ORANGE_ACCENT; reset TMP.color a white |
| `Canvas/GeneralStatsCard/BestTimeBlock/Label` | TextMeshProUGUI | `TextSecondary` | `textSecondary` | #B3B3B3 | Reset TMP.color a white |
| `Canvas/GeneralStatsCard/AvgTimeBlock` | Image | `SecondaryBackground` | `secondaryBackground` | #1A1A26 | Reset Image.color a white |
| `Canvas/GeneralStatsCard/AvgTimeBlock/Value` | TextMeshProUGUI | `AccentSecondary` | `secondaryAccent` | #FF0080 | ⚠️BORDERLINE — magenta más cercano a PURPLE_ACCENT (0.6,0.3,1); reset TMP.color a white |
| `Canvas/GeneralStatsCard/AvgTimeBlock/Label` | TextMeshProUGUI | `TextSecondary` | `textSecondary` | #B3B3B3 | Reset TMP.color a white |
| `Canvas/GameStatsCard` | Image | `CardBackground` | `cardBackground` | #1F1F2E | Dual con Outline; reset Image.color a white |
| `Canvas/GameStatsCard` | Outline | `Glow` | `glowColor` | #00FFFF80 | Dual con Image; reset effectColor a white |
| `Canvas/GameStatsCard/Title/LeftLine` | Image | `Accent` | `primaryAccent` | #00FFFF | Reset Image.color a white |
| `Canvas/GameStatsCard/Title/TitleText` | TextMeshProUGUI | `Accent` | `primaryAccent` | #00FFFF | Reset TMP.color a white |
| `Canvas/GameStatsCard/Title/RightLine` | Image | `Accent` | `primaryAccent` | #00FFFF | Reset Image.color a white |
| `Canvas/GameStatsCard/DigitRushRow/Label` | TextMeshProUGUI | `TextPrimary` | `textPrimary` | #FFFFFF | Reset TMP.color a white |
| `Canvas/GameStatsCard/DigitRushRow/BarBG` | Image | `SliderTrack` | `sliderTrack` | #333340 | Reset Image.color a white |
| `Canvas/GameStatsCard/FlashTapRow/Label` | TextMeshProUGUI | `TextPrimary` | `textPrimary` | #FFFFFF | Reset TMP.color a white |
| `Canvas/GameStatsCard/FlashTapRow/BarBG` | Image | `SliderTrack` | `sliderTrack` | #333340 | Reset Image.color a white |
| `Canvas/GameStatsCard/MemoryPairsRow/Label` | TextMeshProUGUI | `TextPrimary` | `textPrimary` | #FFFFFF | Reset TMP.color a white |
| `Canvas/GameStatsCard/MemoryPairsRow/BarBG` | Image | `SliderTrack` | `sliderTrack` | #333340 | Reset Image.color a white |
| `Canvas/GameStatsCard/OddOneOutRow/Label` | TextMeshProUGUI | `TextPrimary` | `textPrimary` | #FFFFFF | Reset TMP.color a white |
| `Canvas/GameStatsCard/OddOneOutRow/BarBG` | Image | `SliderTrack` | `sliderTrack` | #333340 | Reset Image.color a white |
| `Canvas/GameStatsCard/QuickMathRow/Label` | TextMeshProUGUI | `TextPrimary` | `textPrimary` | #FFFFFF | Reset TMP.color a white |
| `Canvas/GameStatsCard/QuickMathRow/BarBG` | Image | `SliderTrack` | `sliderTrack` | #333340 | Reset Image.color a white |
| `Canvas/ActionRow/FriendsButton` | Image | `ButtonSecondary` | `buttonSecondary` | #333340 | Dual con Outline; reset Image.color a white |
| `Canvas/ActionRow/FriendsButton` | Outline | `Glow` | `glowColor` | #00FFFF80 | Dual con Image; reset effectColor a white |
| `Canvas/ActionRow/FriendsButton/Text` | TextMeshProUGUI | `Accent` | `primaryAccent` | #00FFFF | Reset TMP.color a white |
| `Canvas/ActionRow/HistoryButton` | Image | `ButtonSecondary` | `buttonSecondary` | #333340 | ⚠️BORDERLINE — UIBuilder usa PURPLE_ACCENT para fill; dual con Outline; reset Image.color a white |
| `Canvas/ActionRow/HistoryButton` | Outline | `AccentSecondary` | `secondaryAccent` | #FF0080 | ⚠️BORDERLINE — PURPLE_ACCENT en UIBuilder → AccentSecondary es lo más cercano; dual con Image; reset effectColor a white |
| `Canvas/ActionRow/HistoryButton/Text` | TextMeshProUGUI | `AccentSecondary` | `secondaryAccent` | #FF0080 | ⚠️BORDERLINE — PURPLE_ACCENT; reset TMP.color a white |
| `Canvas/CTASection/ChallengeButton` | Image | `ButtonPrimary` | `buttonPrimary` | #00FFFF | Dual con Outline; reset Image.color a white |
| `Canvas/CTASection/ChallengeButton` | Outline | `ButtonGlowPrimary` | `glowColor` | #00FFFF80 | Dual con Image; reset effectColor a white |
| `Canvas/CTASection/ChallengeText` | TextMeshProUGUI | `TextOnPrimary` | `textOnPrimary` | #000000 | Reset TMP.color a white |
| `Canvas/GameSelectionPanel/DarkOverlay` | Image | `Overlay` | `overlayColor` | #000000D9 | SetActive(false) por defecto; reset Image.color a white |
| `Canvas/GameSelectionPanel/Container` | Image | `TertiaryBackground` | `tertiaryBackground` | #262633 | Dual con Outline; reset Image.color a white |
| `Canvas/GameSelectionPanel/Container` | Outline | `Glow` | `glowColor` | #00FFFF80 | Dual con Image; reset effectColor a white |
| `Canvas/GameSelectionPanel/Container/GameSelectionTitle` | TextMeshProUGUI | `Accent` | `primaryAccent` | #00FFFF | Reset TMP.color a white |
| `Canvas/GameSelectionPanel/Container/GamesList/DigitRushButton` | Image | `ButtonSecondary` | `buttonSecondary` | #333340 | Reset Image.color a white |
| `Canvas/GameSelectionPanel/Container/GamesList/FlashTapButton` | Image | `ButtonSecondary` | `buttonSecondary` | #333340 | Reset Image.color a white |
| `Canvas/GameSelectionPanel/Container/GamesList/MemoryPairsButton` | Image | `ButtonSecondary` | `buttonSecondary` | #333340 | Reset Image.color a white |
| `Canvas/GameSelectionPanel/Container/GamesList/OddOneOutButton` | Image | `ButtonSecondary` | `buttonSecondary` | #333340 | Reset Image.color a white |
| `Canvas/GameSelectionPanel/Container/GamesList/QuickMathButton` | Image | `ButtonSecondary` | `buttonSecondary` | #333340 | Reset Image.color a white |
| `Canvas/GameSelectionPanel/Container/CancelButton` | Image | `ButtonSecondary` | `buttonSecondary` | #333340 | Reset Image.color a white |
| `Canvas/GameSelectionPanel/Container/CancelButton/Text` | TextMeshProUGUI | `TextSecondary` | `textSecondary` | #B3B3B3 | Reset TMP.color a white |
| `Canvas/ChangeNamePanel/BlockerPanel` | Image | `Overlay` | `overlayColor` | #000000D9 | SetActive(false) por defecto; reset Image.color a white |
| `Canvas/ChangeNamePanel/Panel` | Image | `TertiaryBackground` | `tertiaryBackground` | #262633 | Reset Image.color a white |
| `Canvas/ChangeNamePanel/Panel/TitleText` | TextMeshProUGUI | `Accent` | `primaryAccent` | #00FFFF | Reset TMP.color a white |
| `Canvas/ChangeNamePanel/Panel/InputField` | Image | `InputBackground` | `inputBackground` | #1A1A26 | Reset Image.color a white |
| `Canvas/ChangeNamePanel/Panel/InputField/Placeholder` | TextMeshProUGUI | `InputPlaceholder` | `inputPlaceholder` | #808080 | Reset TMP.color a white |
| `Canvas/ChangeNamePanel/Panel/InputField/Text` | TextMeshProUGUI | `TextPrimary` | `textPrimary` | #FFFFFF | Reset TMP.color a white |
| `Canvas/ChangeNamePanel/Panel/ConfirmButton` | Image | `ButtonPrimary` | `buttonPrimary` | #00FFFF | Reset Image.color a white |
| `Canvas/ChangeNamePanel/Panel/ConfirmButton/Text` | TextMeshProUGUI | `TextOnPrimary` | `textOnPrimary` | #000000 | Reset TMP.color a white |
| `Canvas/ChangeNamePanel/Panel/CancelButton` | Image | `ButtonSecondary` | `buttonSecondary` | #333340 | Reset Image.color a white |
| `Canvas/ChangeNamePanel/Panel/CancelButton/Text` | TextMeshProUGUI | `TextSecondary` | `textSecondary` | #B3B3B3 | Reset TMP.color a white |

#### Objetos que NO se tintan
| Objeto (GameObject path) | Razón |
|---|---|
| `Canvas/Header/BackButton` | Prefab compartido — no modificar desde UIBuilder de escena |
| `Canvas/Header/AddFriendButton` | Image casi-transparente (10% opacidad); ProfileManager sobreescribe color a gris cuando solicitud pendiente → runtime override |
| `Canvas/Header/AddFriendButton/Icon` | Runtime semantic — verde (#00FF88) o gris según estado de amistad → NO |
| `Canvas/Header/CoinsPill` | CurrencyHeaderBarHelper estático — regla global NO |
| `Canvas/Header/CoinsPill/CoinsIcon` | CurrencyHeaderBarHelper estático — regla global NO |
| `Canvas/Header/CoinsPill/CoinsValueText` | CurrencyHeaderBarHelper estático — regla global NO |
| `Canvas/Header/CoinsAddButton` | CurrencyHeaderBarHelper estático — regla global NO |
| `Canvas/Header/GemsPill` | CurrencyHeaderBarHelper estático — regla global NO |
| `Canvas/Header/GemsPill/GemsIcon` | CurrencyHeaderBarHelper estático — regla global NO |
| `Canvas/Header/GemsPill/GemsValueText` | CurrencyHeaderBarHelper estático — regla global NO |
| `Canvas/Header/GemsAddButton` | CurrencyHeaderBarHelper estático — regla global NO |
| `Canvas/AvatarCard/AvatarFrame` | Contenedor HLG — sin Image component |
| `Canvas/AvatarCard/BorderRing` | Tiene componente `FrameRenderer` — gestiona frame cosmético → NO ThemeApplier |
| `Canvas/AvatarCard/AvatarImage` | Foto del jugador (arte no-temático) → NO |
| `Canvas/AvatarCard/StatusText` | `SetStatusText(text, color)` sobreescribe TMP.color en runtime con cyan/green/yellow/grey según estado → NO |
| `Canvas/GeneralStatsCard/Title` | Contenedor HLG — sin Image component |
| `Canvas/GameStatsCard/Title` | Contenedor HLG — sin Image component |
| `Canvas/GameStatsCard/DigitRushRow` | Sin Image component en root |
| `Canvas/GameStatsCard/DigitRushRow/AccentBar` | Color semántico DigitRush (cyan identity) → NO |
| `Canvas/GameStatsCard/DigitRushRow/BarFill` | Color semántico DigitRush fill → NO |
| `Canvas/GameStatsCard/DigitRushRow/Value` | Color semántico DigitRush → NO |
| `Canvas/GameStatsCard/DigitRushRow/Separator` | ~5% opacidad blanco — near-invisible → NO |
| `Canvas/GameStatsCard/FlashTapRow` | Sin Image component en root |
| `Canvas/GameStatsCard/FlashTapRow/AccentBar` | Color semántico FlashTap (verde identity) → NO |
| `Canvas/GameStatsCard/FlashTapRow/BarFill` | Color semántico FlashTap fill → NO |
| `Canvas/GameStatsCard/FlashTapRow/Value` | Color semántico FlashTap → NO |
| `Canvas/GameStatsCard/FlashTapRow/Separator` | ~5% opacidad blanco → NO |
| `Canvas/GameStatsCard/MemoryPairsRow` | Sin Image component en root |
| `Canvas/GameStatsCard/MemoryPairsRow/AccentBar` | Color semántico MemoryPairs (purple identity) → NO |
| `Canvas/GameStatsCard/MemoryPairsRow/BarFill` | Color semántico MemoryPairs fill → NO |
| `Canvas/GameStatsCard/MemoryPairsRow/Value` | Color semántico MemoryPairs → NO |
| `Canvas/GameStatsCard/MemoryPairsRow/Separator` | ~5% opacidad blanco → NO |
| `Canvas/GameStatsCard/OddOneOutRow` | Sin Image component en root |
| `Canvas/GameStatsCard/OddOneOutRow/AccentBar` | Color semántico OddOneOut (rojo/gold identity) → NO |
| `Canvas/GameStatsCard/OddOneOutRow/BarFill` | Color semántico OddOneOut fill → NO |
| `Canvas/GameStatsCard/OddOneOutRow/Value` | Color semántico OddOneOut → NO |
| `Canvas/GameStatsCard/OddOneOutRow/Separator` | ~5% opacidad blanco → NO |
| `Canvas/GameStatsCard/QuickMathRow` | Sin Image component en root |
| `Canvas/GameStatsCard/QuickMathRow/AccentBar` | Color semántico QuickMath (naranja identity) → NO |
| `Canvas/GameStatsCard/QuickMathRow/BarFill` | Color semántico QuickMath fill → NO |
| `Canvas/GameStatsCard/QuickMathRow/Value` | Color semántico QuickMath → NO |
| `Canvas/GameStatsCard/QuickMathRow/Separator` | ~5% opacidad blanco → NO |
| `Canvas/ActionRow` | Contenedor HLG — sin Image component |
| `Canvas/CTASection` | Contenedor layout — sin Image component |
| `Canvas/GameSelectionPanel` | Contenedor — sin Image (DarkOverlay hijo separado) |
| `Canvas/GameSelectionPanel/Container/GamesList` | Contenedor VLG — sin Image component |
| `Canvas/GameSelectionPanel/Container/GamesList/DigitRushButton` | Outline | Color semántico DigitRush → NO |
| `Canvas/GameSelectionPanel/Container/GamesList/DigitRushButton/Text` | Color semántico DigitRush → NO |
| `Canvas/GameSelectionPanel/Container/GamesList/FlashTapButton` | Outline | Color semántico FlashTap → NO |
| `Canvas/GameSelectionPanel/Container/GamesList/FlashTapButton/Text` | Color semántico FlashTap → NO |
| `Canvas/GameSelectionPanel/Container/GamesList/MemoryPairsButton` | Outline | Color semántico MemoryPairs → NO |
| `Canvas/GameSelectionPanel/Container/GamesList/MemoryPairsButton/Text` | Color semántico MemoryPairs → NO |
| `Canvas/GameSelectionPanel/Container/GamesList/OddOneOutButton` | Outline | Color semántico OddOneOut → NO |
| `Canvas/GameSelectionPanel/Container/GamesList/OddOneOutButton/Text` | Color semántico OddOneOut → NO |
| `Canvas/GameSelectionPanel/Container/GamesList/QuickMathButton` | Outline | Color semántico QuickMath → NO |
| `Canvas/GameSelectionPanel/Container/GamesList/QuickMathButton/Text` | Color semántico QuickMath → NO |
| `Canvas/ChangeNamePanel` | Root contenedor — sin Image component |
| `Canvas/ChangeNamePanel/Panel/InputField/TextArea` | RectMask2D clip region — sin rol visual temático |
| `ErrorPanel` | Instanciado desde prefab — ThemeApplier se añade en el prefab directamente |

#### Casos especiales (2 ThemeAppliers en mismo objeto)
| Objeto | ThemeApplier #1 | ThemeApplier #2 | Razón |
|---|---|---|---|
| `Canvas/AvatarCard` | Image → `CardBackground` | Outline → `Glow` | Card principal con borde neón |
| `Canvas/AvatarCard/EditButton` | Image → `ButtonSecondary` | Outline → `Glow` | Botón con borde neón |
| `Canvas/GeneralStatsCard` | Image → `CardBackground` | Outline → `Glow` | Card stats con borde neón |
| `Canvas/GameStatsCard` | Image → `CardBackground` | Outline → `Glow` | Card stats con borde neón |
| `Canvas/ActionRow/FriendsButton` | Image → `ButtonSecondary` | Outline → `Glow` | Botón acción con borde cyan |
| `Canvas/ActionRow/HistoryButton` | Image → `ButtonSecondary` | Outline → `AccentSecondary` | ⚠️BORDERLINE — borde purple-accent mapeado a AccentSecondary (magenta) |
| `Canvas/CTASection/ChallengeButton` | Image → `ButtonPrimary` | Outline → `ButtonGlowPrimary` | CTA principal con glow |
| `Canvas/GameSelectionPanel/Container` | Image → `TertiaryBackground` | Outline → `Glow` | Panel overlay con borde neón |

#### Notas de escena
- **5 stat value texts BORDERLINE**: TotalGames→`Accent`, Wins→`Success`, WinRate→`AccentTertiary`, BestTime→`Warning`, AvgTime→`AccentSecondary`. Ningún ElementType es perfecto — son colores de diseño (CYAN/GREEN/GOLD/ORANGE/PURPLE). Coordinar con diseñador si se quieren mantener los valores de color exactos del tema neon_dark.
- **HistoryButton BORDERLINE**: UIBuilder usa `PURPLE_ACCENT (0.6,0.3,1)` para Outline y Text pero no existe `ElementType.Purple`. Mapeado a `AccentSecondary` (magenta #FF0080 en NeonDark). El cambio visual es aceptable en otros temas.
- **BorderRing → NO**: Tiene `FrameRenderer` component que gestiona cosmética del marco → mismo patrón que FriendCard/AvatarFrame.
- **AddFriendButton → NO**: `ProfileManager.UpdateFriendButton()` sobreescribe `Image.color` a gris semitransparente cuando hay solicitud pendiente.
- **StatusText → NO**: `ProfileManager.SetStatusText(text, color)` envía cyan/green/yellow/grey según estado de amistad.
- **EditButton/Icon → BORDERLINE YES**: Solo si el sprite `edit_icon` es 100% blanco puro en la textura. Verificar antes de implementar.
- **GameSelectionPanel**: SetActive(false) por defecto. ThemeApplier aplica al activarse (OnEnable se dispara).
- **ChangeNamePanel**: SetActive(false) por defecto. Mismo patrón.
- **Implementar via**: `ProfileUIBuilder.cs` — llamar `AddThemeApplier()` helper en cada objeto listado. Los paneles ocultos reciben ThemeApplier aunque estén inactivos (se aplica al activarse).
- **⚠️ Bug encontrado**: `SetupManagerReferences()` referencia `Header/TotalCountText` pero `CreateHeader()` nunca crea este GO. Investigar si TotalCountText fue eliminado o falta crearla.

---

### 19 · `Social/Profile/Scores.unity` — 📝 V52

#### Objetos a TINTAR
| Objeto (GameObject path) | Componente | ElementType | ThemeData Property | Color NeonDark | Notas |
|---|---|---|---|---|---|
| `Canvas/Background` | Image | `PrimaryBackground` | `primaryBackground` | #0D0D1A | Reset Image.color a white |
| `Canvas/Header` | Image | `SecondaryBackground` | `secondaryBackground` | #1A1A26 | Reset Image.color a white |
| `Canvas/Header/TitleText` | TextMeshProUGUI | `Accent` | `primaryAccent` | #00FFFF | Reset TMP.color a white |
| `Canvas/ScoresPanel/TabsContainer` (TA #1) | Image | `SecondaryBackground` | `secondaryBackground` | #1A1A26 | Dual con Outline; reset Image.color a white |
| `Canvas/ScoresPanel/TabsContainer` (TA #2) | Outline | `Glow` | `glowColor` | #00FFFF80 | ⚠️BORDERLINE — UIBuilder usa CYAN_DARK (teal oscuro); Glow es lo más cercano; dual con Image; reset effectColor a white |
| `Canvas/ScoresPanel/TabsContainer/TabDivider` | Image | `Accent` | `primaryAccent` | #00FFFF | Reset Image.color a white |
| `Canvas/ScoresPanel/PlayerPositionPanel` | Image | `SecondaryBackground` | `secondaryBackground` | #1A1A26 | Reset Image.color a white |
| `Canvas/ScoresPanel/PlayerPositionPanel/TopLine` | Image | `Accent` | `primaryAccent` | #00FFFF | Reset Image.color a white |
| `Canvas/ScoresPanel/PlayerPositionPanel/ScoresPositionLabel` | TextMeshProUGUI | `TextSecondary` | `textSecondary` | #B3B3B3 | Reset TMP.color a white |
| `Canvas/ScoresPanel/LoadingPanel` (TA #1) | Image | `TertiaryBackground` | `tertiaryBackground` | #262633 | SetActive(false) por defecto; dual con Outline; reset Image.color a white |
| `Canvas/ScoresPanel/LoadingPanel` (TA #2) | Outline | `Glow` | `glowColor` | #00FFFF80 | ⚠️BORDERLINE — UIBuilder usa CYAN_DARK; dual con Image; reset effectColor a white |
| `Canvas/ScoresPanel/LoadingPanel/LoadingText` | TextMeshProUGUI | `Accent` | `primaryAccent` | #00FFFF | SetActive(false) por defecto; reset TMP.color a white |
| `Canvas/ScoresPanel/EmptyState/TrophyIcon` | TextMeshProUGUI | `AccentTertiary` | `tertiaryAccent` | #FFD700 | Emoji ⭐ renderizado como TMP gold; SetActive(false) por defecto; reset TMP.color a white |
| `Canvas/ScoresPanel/EmptyState/EmptyTitle` | TextMeshProUGUI | `TextPrimary` | `textPrimary` | #FFFFFF | Reset TMP.color a white |
| `Canvas/ScoresPanel/EmptyState/EmptySubtitle` | TextMeshProUGUI | `TextSecondary` | `textSecondary` | #B3B3B3 | Reset TMP.color a white |
| `Canvas/ScoresPanel/EmptyState/PlayButton` (TA #1) | Image | `ButtonPrimary` | `buttonPrimary` | #00FFFF | Dual con Outline; reset Image.color a white |
| `Canvas/ScoresPanel/EmptyState/PlayButton` (TA #2) | Outline | `ButtonGlowPrimary` | `glowColor` | #00FFFF80 | Dual con Image; reset effectColor a white |
| `Canvas/ScoresPanel/EmptyState/PlayButton/Text` | TextMeshProUGUI | `TextOnPrimary` | `textOnPrimary` | #000000 | Reset TMP.color a white |

#### Objetos que NO se tintan
| Objeto (GameObject path) | Razón |
|---|---|
| `Canvas/Header/BackButton` | Prefab compartido — no modificar desde UIBuilder de escena |
| `Canvas/Header/CoinsPill` | CurrencyHeaderBarHelper estático — regla global NO |
| `Canvas/Header/CoinsPill/CoinsIcon` | CurrencyHeaderBarHelper estático — regla global NO |
| `Canvas/Header/CoinsPill/CoinsValueText` | CurrencyHeaderBarHelper estático — regla global NO |
| `Canvas/Header/CoinsAddButton` | CurrencyHeaderBarHelper estático — regla global NO |
| `Canvas/Header/GemsPill` | CurrencyHeaderBarHelper estático — regla global NO |
| `Canvas/Header/GemsPill/GemsIcon` | CurrencyHeaderBarHelper estático — regla global NO |
| `Canvas/Header/GemsPill/GemsValueText` | CurrencyHeaderBarHelper estático — regla global NO |
| `Canvas/Header/GemsAddButton` | CurrencyHeaderBarHelper estático — regla global NO |
| `Canvas/ScoresPanel` | Contenedor — sin Image component |
| `Canvas/ScoresPanel/GameSelectorPanel` | HLG contenedor — sin Image component |
| `Canvas/ScoresPanel/GameSelectorPanel/GameButton_DigitRush` (Image) | `UpdateGameSelectorVisuals()` sobreescribe bg color con selected/normal hardcoded → NO |
| `Canvas/ScoresPanel/GameSelectorPanel/GameButton_DigitRush` (Outline) | `UpdateGameSelectorVisuals()` sobreescribe `outline.effectColor` con selected/normal hardcoded → NO |
| `Canvas/ScoresPanel/GameSelectorPanel/GameButton_DigitRush/Icon` | Icono de juego multi-color (arte ilustrado) → NO |
| `Canvas/ScoresPanel/GameSelectorPanel/GameButton_DigitRush/Label` | `UpdateGameSelectorVisuals()` sobreescribe TMP.color white/grey → NO |
| `Canvas/ScoresPanel/GameSelectorPanel/GameButton_FlashTap` (Image) | Runtime override por `UpdateGameSelectorVisuals()` → NO |
| `Canvas/ScoresPanel/GameSelectorPanel/GameButton_FlashTap` (Outline) | Runtime override por `UpdateGameSelectorVisuals()` → NO |
| `Canvas/ScoresPanel/GameSelectorPanel/GameButton_FlashTap/Icon` | Arte ilustrado multi-color → NO |
| `Canvas/ScoresPanel/GameSelectorPanel/GameButton_FlashTap/Label` | Runtime override por `UpdateGameSelectorVisuals()` → NO |
| `Canvas/ScoresPanel/GameSelectorPanel/GameButton_MemoryPairs` (Image) | Runtime override → NO |
| `Canvas/ScoresPanel/GameSelectorPanel/GameButton_MemoryPairs` (Outline) | Runtime override → NO |
| `Canvas/ScoresPanel/GameSelectorPanel/GameButton_MemoryPairs/Icon` | Arte ilustrado multi-color → NO |
| `Canvas/ScoresPanel/GameSelectorPanel/GameButton_MemoryPairs/Label` | Runtime override → NO |
| `Canvas/ScoresPanel/GameSelectorPanel/GameButton_OddOneOut` (Image) | Runtime override → NO |
| `Canvas/ScoresPanel/GameSelectorPanel/GameButton_OddOneOut` (Outline) | Runtime override → NO |
| `Canvas/ScoresPanel/GameSelectorPanel/GameButton_OddOneOut/Icon` | Arte ilustrado multi-color → NO |
| `Canvas/ScoresPanel/GameSelectorPanel/GameButton_OddOneOut/Label` | Runtime override → NO |
| `Canvas/ScoresPanel/GameSelectorPanel/GameButton_QuickMath` (Image) | Runtime override → NO |
| `Canvas/ScoresPanel/GameSelectorPanel/GameButton_QuickMath` (Outline) | Runtime override → NO |
| `Canvas/ScoresPanel/GameSelectorPanel/GameButton_QuickMath/Icon` | Arte ilustrado multi-color → NO |
| `Canvas/ScoresPanel/GameSelectorPanel/GameButton_QuickMath/Label` | Runtime override → NO |
| `Canvas/ScoresPanel/TabsContainer/NationalTab` (Image) | `SetTabButtonState()` DOColor override hardcoded → NO |
| `Canvas/ScoresPanel/TabsContainer/NationalTab/Text` | `SetTabButtonState()` DOColor override hardcoded → NO |
| `Canvas/ScoresPanel/TabsContainer/GlobalTab` (Image) | `SetTabButtonState()` DOColor override hardcoded → NO |
| `Canvas/ScoresPanel/TabsContainer/GlobalTab/Text` | `SetTabButtonState()` DOColor override hardcoded → NO |
| `Canvas/ScoresPanel/LeaderboardScrollView` | ScrollRect — sin Image component |
| `Canvas/ScoresPanel/LeaderboardScrollView/Viewport` | RectMask2D — sin Image component (UIBuilder elimina Image) |
| `Canvas/ScoresPanel/LeaderboardScrollView/Viewport/LeaderboardContainer` | VLG contenedor — sin Image component |
| `Canvas/ScoresPanel/EmptyState` | Contenedor — sin Image component en root |
| `Canvas/ScoresPanel/PlayerPositionPanel/PositionNumber` | `UpdatePlayerPositionPanel()` sobreescribe color: gold si ranked, grey si no → runtime override |
| `Canvas/ScoresPanel/PlayerPositionPanel/PositionTime` | Color semántico TIME_COLOR (#00FF87 verde) — puntuación → NO |
| `Canvas/ScoresPanel/.../SampleEntry_1` (root + todos los hijos) | Editor preview — creados por `CreateSampleEntries()` para previsualización, destruidos en runtime por `ClearLeaderboard()` |
| `Canvas/ScoresPanel/.../SampleEntry_1/PositionText` | Editor preview + color semántico rank (gold/silver/bronze) |
| `Canvas/ScoresPanel/.../SampleEntry_1/AvatarFrame` | Editor preview + FrameRenderer gestiona frame cosmético |
| `Canvas/ScoresPanel/.../SampleEntry_1/AvatarFrame/AvatarMask` | Editor preview |
| `Canvas/ScoresPanel/.../SampleEntry_1/AvatarFrame/AvatarMask/AvatarImage` | Editor preview + foto de jugador (arte no-temático) |
| `Canvas/ScoresPanel/.../SampleEntry_1/UsernameText` | Editor preview |
| `Canvas/ScoresPanel/.../SampleEntry_1/TimeText` | Editor preview + TIME_COLOR semántico verde |
| `Canvas/ScoresPanel/.../SampleEntry_2` a `_5` (×6 items cada uno) | Idéntico a SampleEntry_1 — Editor preview, destruidos en runtime |
| `LeaderboardEntry` *(prefab, root Image)* | `LeaderboardEntryUI.Setup()` sobreescribe `backgroundImage.color` con cyan (jugador actual) / even-odd alternante hardcoded → NO en estado actual |
| `LeaderboardEntry/PositionText` *(prefab)* | `Setup()` sobreescribe color: gold/silver/bronze/grey hardcoded. Para theming real se necesita `Setup()` use `ThemeData.GetRankColor()` |
| `LeaderboardEntry/UsernameText` *(prefab)* | `Setup()` sobreescribe color: cyan (jugador actual) o white (otros) hardcoded → NO |
| `LeaderboardEntry/TimeText` *(prefab)* | `Setup()` usa `timeColor` hardcoded verde (#00FF87) — semántico score → NO |
| `LeaderboardEntry/VerticalDivider1` *(prefab)* | Separador decorativo con color fijo (0.5,0.5,0.6,0.8) — cosmético no temático |
| `LeaderboardEntry/VerticalDivider2` *(prefab)* | Separador decorativo con color fijo (0.5,0.5,0.6,0.8) — cosmético no temático |
| `LeaderboardEntry/HorizontalDivider` *(prefab)* | Separador decorativo con color fijo (0.4,0.4,0.5,0.5) — cosmético no temático |

#### Casos especiales (2 ThemeAppliers en mismo objeto)
| Objeto | ThemeApplier #1 | ThemeApplier #2 | Razón |
|---|---|---|---|
| `Canvas/ScoresPanel/TabsContainer` | Image → `SecondaryBackground` | Outline → `Glow` | Panel tabs con borde neón |
| `Canvas/ScoresPanel/LoadingPanel` | Image → `TertiaryBackground` | Outline → `Glow` | Panel flotante con borde neón |
| `Canvas/ScoresPanel/EmptyState/PlayButton` | Image → `ButtonPrimary` | Outline → `ButtonGlowPrimary` | CTA con glow |

#### Notas de escena
- **Tabs NO**: `SetTabButtonState()` usa `DOColor()` con colores hardcoded (no ThemeData) en cada cambio de tab → cualquier ThemeApplier en NationalTab/GlobalTab quedaría sobreescrito inmediatamente.
- **Game buttons NO**: `UpdateGameSelectorVisuals()` hardcodea bg, Outline.effectColor y Label.color en cada selección de juego → NO ThemeApplier en ningún GameButton.
- **LeaderboardEntry prefab → mejora necesaria**: Para que los rank colors respeten temas, `LeaderboardEntryUI.Setup()` debe usar `ThemeManager.Instance?.CurrentTheme?.GetRankColor(position)` en vez de `goldColor/silverColor/bronzeColor` fijos. Lo mismo para `evenRowColor/oddRowColor` → `theme.rowEven/rowOdd`. Esto requiere actualizar `LeaderboardEntryUI.cs`.
- **SampleEntries**: 5 entradas creadas por el UIBuilder para previsualización en Editor. Se destruyen en runtime cuando `ClearLeaderboard()` se ejecuta. NO ThemeApplier.
- **TrophyIcon**: Es un TextMeshProUGUI que renderiza el emoji ⭐, no una Image. ThemeApplier con `applyToText=true` aplica color sobre TMP → funciona correctamente con AccentTertiary.
- **Implementar via**: `ScoresUIBuilder.cs` — añadir ThemeApplier solo en los 18 objetos estáticos listados. El LoadingPanel y EmptyState reciben ThemeApplier aunque estén SetActive(false) (OnEnable se dispara al activarse).
- **PositionNumber AddTextGlow**: Tiene Shadow con GOLD glow. NO añadir ThemeApplier al Shadow (afectaría al glow decorativo). El Shadow se queda tal cual.

---

### 20 · `Social/Profile/MatchHistory.unity` — 📝 V52

#### Objetos a TINTAR
| Objeto (GameObject path) | Componente | ElementType | ThemeData Property | Color NeonDark | Notas |
|---|---|---|---|---|---|
| `Canvas/Background` | Image | `PrimaryBackground` | `primaryBackground` | #050A14 | Reset Image.color a white |
| `Canvas/Header` | Image | `SecondaryBackground` | `secondaryBackground` | #0A0F19 | Reset Image.color a white |
| `Canvas/Header/TitleText` | TextMeshProUGUI | `Accent` | `primaryAccent` | #00FFFF | Reset TMP.color a white |
| `Canvas/ScrollView/Viewport/Content/EmptyText` | TextMeshProUGUI | `TextSecondary` | `textSecondary` | #99999A | Reset TMP.color a white |
| `Canvas/ScrollView/Viewport/Content/LoadingIndicator` | TextMeshProUGUI | `Accent` | `primaryAccent` | #00FFFF | SetActive(false) al inicio — ThemeApplier aplica al activar; reset TMP.color a white |
| `Canvas/ScrollView/Viewport/Content/LoadMoreButton` | Image | `ButtonSecondary` | `buttonSecondary` | #1E2430 | SetActive(false) al inicio; reset Image.color a white; dual con Outline |
| `Canvas/ScrollView/Viewport/Content/LoadMoreButton` | Outline | `Glow` | `glowColor` | #00FFFF80 | Reset effectColor a white; dual con Image |
| `Canvas/ScrollView/Viewport/Content/LoadMoreButton/Text` | TextMeshProUGUI | `Accent` | `primaryAccent` | #00FFFF | Reset TMP.color a white |
| `MatchHistoryEntry` *(prefab, root)* | Image | `CardBackground` | `cardBackground` | #0F141F | Runtime-instanciado; añadir via `CreateMatchEntryPrefab()`; reset Image.color a white; dual con Outline |
| `MatchHistoryEntry` *(prefab, root)* | Outline | `Glow` | `glowColor` | #00FFFF80 | Dual con Image; reset effectColor a white |
| `MatchHistoryEntry/InfoSection/GameName` *(prefab)* | TextMeshProUGUI | `TextPrimary` | `textPrimary` | #F2F2F2 | Reset TMP.color a white |
| `MatchHistoryEntry/InfoSection/OpponentText` *(prefab)* | TextMeshProUGUI | `TextSecondary` | `textSecondary` | #99999A | Reset TMP.color a white; runtime cambia solo el contenido, no el color |
| `MatchHistoryEntry/InfoSection/DetailText` *(prefab)* | TextMeshProUGUI | `Accent` | `primaryAccent` | #00FFFF | Reset TMP.color a white; runtime cambia solo el contenido |
| `MatchHistoryEntry/DateText` *(prefab)* | TextMeshProUGUI | `TextSecondary` | `textSecondary` | #99999A | Reset TMP.color a white; runtime cambia solo el contenido |

#### Objetos que NO se tintan
| Objeto (GameObject path) | Razón |
|---|---|
| `Canvas/Header/BackButton` | Prefab compartido (`BackButton.prefab`) — ThemeApplier gestionado a nivel de prefab |
| `Canvas/Header/CurrencyPills` | CurrencyHeaderBarHelper — container de pills estáticas; NUNCA ThemeApplier |
| `Canvas/Header/CurrencyPills/CoinsPill` | Pill estática |
| `Canvas/Header/CurrencyPills/GemsPill` | Pill estática |
| `Canvas/Header/CurrencyPills/CoinsAddButton` | Botón estático |
| `Canvas/Header/CurrencyPills/GemsAddButton` | Botón estático |
| `Canvas/Header/CurrencyPills/CoinsIcon` | Icono de moneda — arte fijo |
| `Canvas/Header/CurrencyPills/GemsIcon` | Icono de gema — arte fijo |
| `Canvas/Header/CurrencyPills/CoinsValueText` | Texto estático |
| `Canvas/Header/CurrencyPills/GemsValueText` | Texto estático |
| `Canvas/GameFilters` | Image `Color.clear` — contenedor transparente sin rol visual |
| `Canvas/GameFilters/FilterAll` Image | `UpdateFilterButtonVisual()` sobreescribe Image.color en runtime (CYAN_NEON activo / CHIP_INACTIVE inactivo) — conflicto con ThemeApplier |
| `Canvas/GameFilters/FilterAll` Outline | `UpdateFilterButtonVisual()` sobreescribe Outline.effectColor en runtime |
| `Canvas/GameFilters/FilterAll/Text` | `UpdateFilterButtonVisual()` sobreescribe TMP.color en runtime (TEXT_DARK / TEXT_SECONDARY) |
| `Canvas/GameFilters/FilterDigitRush` Image | Runtime override igual que FilterAll |
| `Canvas/GameFilters/FilterDigitRush` Outline | Runtime override |
| `Canvas/GameFilters/FilterDigitRush/Icon` | Icono de arte multi-color del juego — regla Icon Tinting; no es sprite blanco puro |
| `Canvas/GameFilters/FilterMemoryPairs` Image | Runtime override |
| `Canvas/GameFilters/FilterMemoryPairs` Outline | Runtime override |
| `Canvas/GameFilters/FilterMemoryPairs/Icon` | Arte multi-color del juego |
| `Canvas/GameFilters/FilterQuickMath` Image | Runtime override |
| `Canvas/GameFilters/FilterQuickMath` Outline | Runtime override |
| `Canvas/GameFilters/FilterQuickMath/Icon` | Arte multi-color del juego |
| `Canvas/GameFilters/FilterFlashTap` Image | Runtime override |
| `Canvas/GameFilters/FilterFlashTap` Outline | Runtime override |
| `Canvas/GameFilters/FilterFlashTap/Icon` | Arte multi-color del juego |
| `Canvas/GameFilters/FilterOddOneOut` Image | Runtime override |
| `Canvas/GameFilters/FilterOddOneOut` Outline | Runtime override |
| `Canvas/GameFilters/FilterOddOneOut/Icon` | Arte multi-color del juego |
| `Canvas/GameFilters/FilterCognitiveSprint` Image | Runtime override |
| `Canvas/GameFilters/FilterCognitiveSprint` Outline | Runtime override |
| `Canvas/GameFilters/FilterCognitiveSprint/Icon` | Arte multi-color del juego |
| `Canvas/ScrollView` | Image `Color.clear`, Mask — contenedor transparente |
| `Canvas/ScrollView/Viewport` | Image `Color.clear`, Mask — contenedor transparente |
| `Canvas/ScrollView/Viewport/Content` | Sin componente Image — solo VerticalLayoutGroup + ContentSizeFitter |
| `MatchHistoryEntry/ColorBar` *(prefab)* | `SetupMatchEntryItem()` sobreescribe Image.color con `GAME_COLORS[entry.gameType]` — color semántico fijo por juego |
| `MatchHistoryEntry/InfoSection` *(prefab)* | Contenedor puro — sin componente Image |
| `MatchHistoryEntry/InfoSection/ResultBadge` *(prefab)* | `SetupMatchEntryItem()` sobreescribe TMP.color con `entry.GetResultColor()` (verde=WIN, rojo=LOSS, gris=PRACTICE) — semántico |
| `MatchHistoryEntry` Shadow *(prefab)* | Sombra decorativa negra — `applyToShadow=false` por defecto; no requiere ThemeApplier |

#### Casos especiales (2 ThemeAppliers en mismo objeto)
| Objeto | ThemeApplier #1 | ThemeApplier #2 | Razón |
|---|---|---|---|
| `Canvas/ScrollView/Viewport/Content/LoadMoreButton` | `ButtonSecondary` · applyToImage=true | `Glow` · applyToOutline=true | Card oscura con borde cyan — colorear Image + Outline independientemente |
| `MatchHistoryEntry` root *(prefab)* | `CardBackground` · applyToImage=true | `Glow` · applyToOutline=true | Card de partida con borde glow — colorear Image + Outline independientemente |

#### Notas de escena
- **Filter chips runtime-overridden**: `UpdateFilterButtonVisual()` sobreescribe Image.color y TMP.color de TODOS los filter chips en cada `ApplyFilter()`. ThemeApplier en estos objetos colisionaría; mantener sin ThemeApplier.
- **MatchHistoryEntry prefab**: Los 6 ThemeApplier (Image, Outline, GameName, OpponentText, DetailText, DateText) deben añadirse en `CreateMatchEntryPrefab()` dentro de `MatchHistoryUIBuilder.cs`. NO via Inspector.
- **ColorBar semántico**: Toma el color del juego (cyan=DigitRush, purple=MemoryPairs, orange=QuickMath, green=FlashTap, red=OddOneOut, gold=CognitiveSprint) — gestionado 100% por `SetupMatchEntryItem()`.
- **ResultBadge semántico**: Verde/rojo/gris según WIN/LOSS/PRACTICE — gestionado por `entry.GetResultColor()`.
- **⚠️ TotalCountText ausente en UIBuilder**: El Manager referencia `Header/TotalCountText` en `SetupManagerReferences()` y lo usa en `UpdateHeader()`, pero `CreateHeader()` no crea ese GO. Si se añade, incluir ThemeApplier `TextSecondary` · applyToText=true.
- **Screenshot 1 stale**: Primer screenshot muestra la escena Notifications de la sesión anterior — ignorar. Screenshot 2 confirma MatchHistory correctamente (Header "HISTORY" cyan, filter row, currency pills, fondo oscuro).

---

### 21 · `Social/Friends/Friends.unity` — 📝 V52

#### Objetos a TINTAR
| Objeto (GameObject path) | Componente | ElementType | ThemeData Property | Color NeonDark | Notas |
|---|---|---|---|---|---|
| Background | Image | PrimaryBackground | `primaryBackground` | #050A14 | Reset color a white antes de ThemeApplier |
| Header | Image | SecondaryBackground | `secondaryBackground` | #0A1019 | Reset color a white |
| Header/TitleText | TextMeshProUGUI | Accent | `primaryAccent` | #00FFFF | Reset color a white; título en accent cyan |
| Header/FriendsCountText | TextMeshProUGUI | TextSecondary | `textSecondary` | #999AA6 | Reset color a white; "0 friends" / "N online" |
| SearchBar | Image | InputBackground | `inputBackground` | #141929 | Dual con Outline; reset color a white |
| SearchBar | Outline | InputBorder | `inputBorder` | #006680 | Dual con Image; reset effectColor a white |
| SearchBar/SearchInput/Text Area/Placeholder | TextMeshProUGUI | InputPlaceholder | `inputPlaceholder` | #66666B | Reset color a white; "Search friends..." |
| SearchBar/SearchInput/Text Area/Text | TextMeshProUGUI | TextPrimary | `textPrimary` | #F2F2F2 | Texto tecleado por el usuario; reset color a white |
| RequestsNav | Image | CardBackground | `cardBackground` | #0F1420 | Dual con Outline; reset color a white |
| RequestsNav | Outline | AccentSecondary | `secondaryAccent` | #9933FF | Dual con Image; borde purple del panel de requests; reset effectColor a white |
| RequestsNav/FriendRequestsLabel | TextMeshProUGUI | AccentSecondary | `secondaryAccent` | #9933FF | Reset color a white; "Friend requests" en purple |
| RequestsNav/Arrow | TextMeshProUGUI | AccentSecondary | `secondaryAccent` | #9933FF | Reset color a white; flecha chevron "›" en purple |
| ScrollView/Viewport/Content/FriendsEmptyText | TextMeshProUGUI | TextSecondary | `textSecondary` | #999AA6 | SetActive(false) al inicio; ThemeApplier aplica antes de mostrarse |
| ScrollView/Viewport/Content/LoadingIndicator | TextMeshProUGUI | Accent | `primaryAccent` | #00FFFF | SetActive(false) al inicio |
| **— FriendCard prefab (×N instancias runtime) —** | | | | | |
| FriendCard | Image | CardBackground | `cardBackground` | #0F1420 | Dual con Outline; reset color a white; añadir ThemeApplier vía código en CreateFriendCard() |
| FriendCard | Outline | Glow | `glowColor` | cyan glow @35% | Dual con Image; reset effectColor a white |
| FriendCard/AvatarFrame/AvatarMask | Image | CardBackground | `cardBackground` | #0F1420 | Círculo placeholder detrás del avatar mientras carga; reset color a white |
| FriendCard/InfoSection/Username | TextMeshProUGUI | TextPrimary | `textPrimary` | #F2F2F2 | Reset color a white |
| FriendCard/InfoSection/StatsText | TextMeshProUGUI | TextSecondary | `textSecondary` | #999AA6 | "65% WR · Digit Rush"; reset color a white |
| FriendCard/InfoSection/ButtonsRow/ChallengeButton | Image | ButtonPrimary | `buttonPrimary` | #00FFFF | Dual con Outline; botón CTA cyan; reset color a white |
| FriendCard/InfoSection/ButtonsRow/ChallengeButton | Outline | ButtonGlowPrimary | `glowColor` | cyan glow | Dual con Image; reset effectColor a white |
| FriendCard/InfoSection/ButtonsRow/ChallengeButton/Text | TextMeshProUGUI | TextOnPrimary | `textOnPrimary` | #1A1A1A | Texto oscuro sobre botón cyan; reset color a white |
| FriendCard/InfoSection/ButtonsRow/ViewProfileButton | Image | ButtonSecondary | `buttonSecondary` | #141929 | Dual con Outline; botón secundario; reset color a white |
| FriendCard/InfoSection/ButtonsRow/ViewProfileButton | Outline | Glow | `glowColor` | cyan glow @30% | Dual con Image; reset effectColor a white |
| FriendCard/InfoSection/ButtonsRow/ViewProfileButton/Text | TextMeshProUGUI | Accent | `primaryAccent` | #00FFFF | Texto cyan sobre botón secundario; reset color a white |
| FriendCard/InfoSection/ButtonsRow/RemoveButton | Image | ButtonSecondary | `buttonSecondary` | #141929 | Dual con Outline; SetActive(false); reset color a white |
| FriendCard/InfoSection/ButtonsRow/RemoveButton | Outline | Glow | `glowColor` | red glow @30% | Dual con Image; reset effectColor a white |
| FriendCard/InfoSection/ButtonsRow/RemoveButton/Text | TextMeshProUGUI | Error | `errorColor` | #FF3333 | Texto rojo "Remove"; SetActive(false); reset color a white |

#### Objetos que NO se tintan
| Objeto (GameObject path) | Razón |
|---|---|
| Header/BackButton | Prefab gestionado independientemente |
| SearchBar/SearchInput | TMP_InputField controller — sin Image propia |
| SearchBar/SearchInput/Text Area | RectMask2D — sin Image |
| ScrollView | ScrollRect — sin Image (eliminada explícitamente en builder) |
| ScrollView/Viewport | RectMask2D — sin Image (eliminada explícitamente en builder) |
| ScrollView/Viewport/Content | RectTransform + VerticalLayoutGroup — sin Image |
| FriendCard/AvatarFrame | FrameRenderer gestiona marcos cosméticos de perfil — ThemeApplier entraría en conflicto con los frames equipados |
| FriendCard/AvatarFrame/AvatarMask/AvatarImage | Foto/avatar del jugador — contenido de foto, no tintable |
| FriendCard/AvatarFrame/OnlineIndicator (Image) | Runtime sobreescribe color: verde (online) o gris (offline) — indicador semántico de estado |
| FriendCard/AvatarFrame/OnlineIndicator (Outline) | Borde del indicador online — color semántico (DARK_BG, contraste fijo) |
| FriendCard/InfoSection | RectTransform — sin Image |
| FriendCard/InfoSection/StatusText | Runtime sobreescribe color: verde/gris según estado online — color semántico, siempre fijo |
| FriendCard/InfoSection/ButtonsRow | RectTransform + VerticalLayoutGroup — sin Image |
| RequestsNav/RequestsBadge | Badge rojo semántico (indicador de notificaciones pendientes) — siempre rojo por convención UX |
| RequestsNav/RequestsBadge/Text | Texto blanco sobre badge semántico rojo — siempre blanco, no tintable |

#### Casos especiales (2 ThemeAppliers en mismo objeto)
| Objeto | ThemeApplier #1 | ThemeApplier #2 | Razón |
|---|---|---|---|
| SearchBar | InputBackground (applyToImage=true) | InputBorder (applyToOutline=true) | Fondo de input + borde del input field |
| RequestsNav | CardBackground (applyToImage=true) | AccentSecondary (applyToOutline=true) | Card background + borde purple del panel de requests |
| FriendCard | CardBackground (applyToImage=true) | Glow (applyToOutline=true) | Card fill + glow border del friend card |
| FriendCard/InfoSection/ButtonsRow/ChallengeButton | ButtonPrimary (applyToImage=true) | ButtonGlowPrimary (applyToOutline=true) | Botón CTA + glow del botón |
| FriendCard/InfoSection/ButtonsRow/ViewProfileButton | ButtonSecondary (applyToImage=true) | Glow (applyToOutline=true) | Botón secundario + glow cyan |
| FriendCard/InfoSection/ButtonsRow/RemoveButton | ButtonSecondary (applyToImage=true) | Glow (applyToOutline=true) | Botón secundario + glow (SetActive false) |

#### Objetos ocultos al inicio (SetActive false)
| Objeto | ElementType | Condición de activación |
|---|---|---|
| ScrollView/Viewport/Content/FriendsEmptyText | TextSecondary | Mostrado cuando la lista de amigos está vacía |
| ScrollView/Viewport/Content/LoadingIndicator | Accent | Mostrado mientras carga la lista de amigos |
| FriendCard/InfoSection/ButtonsRow/RemoveButton | ButtonSecondary+Glow+Error | Disponible para activar via lógica de UI futura |

#### Notas de escena
1. **FriendCard prefab runtime-instanciado**: ThemeApplier debe añadirse al prefab en `Assets/_Project/Prefabs/Social/FriendCard.prefab` — se aplicará a todas las instancias al instanciar.
2. **StatusText runtime-overridden**: `SetupFriendCard()` sobreescribe `statusText.color` con verde/gris según `friend.isOnline` — ThemeApplier NO puede gestionar este texto. Color semántico intencional.
3. **OnlineIndicator runtime-overridden**: `SetupFriendCard()` sobreescribe `onlineIndicator.color` con verde/gris — mismo patrón semántico.
4. **RequestsBadge**: Color rojo semántico para "notificaciones pendientes". No tintable. Texto siempre blanco.
5. **Sin runtime color-overrides en UI estructural**: A diferencia de FriendRequests (SwitchTab()), FriendsManager no sobreescribe colores de la UI principal → ThemeApplier seguro para Header, SearchBar, RequestsNav.

---

### 22 · `Social/Friends/FriendRequests.unity` — 📝 V52

#### Objetos a TINTAR
| Objeto (GameObject path) | Componente | ElementType | ThemeData Property | Color NeonDark | Notas |
|---|---|---|---|---|---|
| Background | Image | PrimaryBackground | `primaryBackground` | #050A14 | Reset color a white antes de ThemeApplier |
| Header | Image | SecondaryBackground | `secondaryBackground` | #0A1019 | Reset color a white |
| Header/TitleText | TextMeshProUGUI | Accent | `primaryAccent` | #00FFFF | Reset color a white; título en accent cyan |
| Header/PendingCountText | TextMeshProUGUI | TextSecondary | `textSecondary` | #999AA6 | Reset color a white |
| TabsBar/ReceivedTab | Image | TabActive | `tabActive` | #00FFFF@12% | ⚠️ SwitchTab() hardcodea ACTIVE_TAB → Manager debe usar ThemeManager.current.tabActive; reset color a white |
| TabsBar/ReceivedTab | Outline | Glow | `glowColor` | cyan glow | Dual con Image; reset effectColor a white |
| TabsBar/ReceivedTab/ReceivedTabText | TextMeshProUGUI | TextOnPrimary | `textOnPrimary` | #0D0D14 | ⚠️ SwitchTab() hardcodea ACTIVE_TEXT → Manager debe usar ThemeManager.current.textOnPrimary; reset color a white |
| TabsBar/ReceivedTab/Indicator | Image | Accent | `primaryAccent` | #00FFFF | Barra indicador de 3px bajo tab activo; reset color a white |
| TabsBar/SentTab | Image | TabInactive | `tabInactive` | #262B38 | ⚠️ SwitchTab() hardcodea INACTIVE_TAB → Manager debe usar ThemeManager.current.tabInactive; reset color a white |
| TabsBar/SentTab | Outline | Glow | `glowColor` | cyan glow dim | Dual con Image; reset effectColor a white |
| TabsBar/SentTab/SentTabText | TextMeshProUGUI | TextSecondary | `textSecondary` | #999AA6 | ⚠️ SwitchTab() hardcodea INACTIVE_TEXT → Manager debe usar ThemeManager.current.textSecondary; reset color a white |
| ScrollView/Viewport/Content/RequestsEmptyText | TextMeshProUGUI | TextSecondary | `textSecondary` | #999AA6 | SetActive(false) al inicio; ThemeApplier aplica antes de mostrarse |
| ScrollView/Viewport/Content/LoadingIndicator | TextMeshProUGUI | Accent | `primaryAccent` | #00FFFF | SetActive(false) al inicio |
| **— RequestItem prefab (×N instancias runtime) —** | | | | | |
| RequestItem | Image | CardBackground | `cardBackground` | #0F1420 | Reset color a white; prefab añadir vía código en CreateRequestItem() |
| RequestItem | Outline | Glow | `glowColor` | cyan glow @35% | Dual con Image; reset effectColor a white |
| RequestItem/AvatarFrame/AvatarMask | Image | CardBackground | `cardBackground` | #0F1420 | Fondo circular interior antes de cargar avatar; reset color a white |
| RequestItem/InfoSection/Username | TextMeshProUGUI | TextPrimary | `textPrimary` | #F2F2F2 | Reset color a white |
| RequestItem/InfoSection/TimestampText | TextMeshProUGUI | TextSecondary | `textSecondary` | #999AA6 | Reset color a white |
| RequestItem/ButtonsRow/AcceptButton | Image | ButtonSuccess | `buttonSuccess` | #33E666 | Reset color a white |
| RequestItem/ButtonsRow/AcceptButton | Outline | ButtonGlowSuccess | `successColor` | #33E666@30% | Dual con Image; reset effectColor a white |
| RequestItem/ButtonsRow/AcceptButton/AcceptButtonText | TextMeshProUGUI | TextOnSuccess | `textOnSuccess` | #0D1408 | Reset color a white |
| RequestItem/ButtonsRow/RejectButton | Image | ButtonDanger | `buttonDanger` | #FF4D4D | Reset color a white |
| RequestItem/ButtonsRow/RejectButton | Outline | ButtonGlowDanger | `errorColor` | #FF4D4D@30% | Dual con Image; reset effectColor a white |
| RequestItem/ButtonsRow/RejectButton/RejectButtonText | TextMeshProUGUI | TextOnDanger | `textOnDanger` | #FFFFFF | Reset color a white |
| RequestItem/ButtonsRow/CancelButton | Image | ButtonSecondary | `buttonSecondary` | #141A24 | SetActive(false) al inicio; solo visible en tab Sent |
| RequestItem/ButtonsRow/CancelButton | Outline | Glow | `glowColor` | orange-tinted glow | Dual con Image; reset effectColor a white |
| RequestItem/ButtonsRow/CancelButton/CancelRequestText | TextMeshProUGUI | Warning | `warningColor` | #FF8000 | Texto naranja acción cancelar; reset color a white |

#### Objetos que NO se tintan
| Objeto (GameObject path) | Razón |
|---|---|
| Header/BackButton | Prefab compartido — ThemeApplier gestionado internamente por el prefab |
| TabsBar | Image Color.clear — contenedor transparente, sin rol visual |
| TabsBar/SentTab/Indicator | Image Color.clear — transparente por defecto (tab inactivo); Builder no lo actualiza al cambiar tab (BUG menor, fuera de scope) |
| ScrollView | Sin Image — solo ScrollRect, contenedor de scroll |
| ScrollView/Viewport | Sin Image — RectMask2D solo para clipping |
| ScrollView/Viewport/Content | Sin Image — VLG + ContentSizeFitter, contenedor de layout |
| RequestItem/AvatarFrame | FrameRenderer gestiona esta Image — sistema de marcos cosméticos del perfil (overrides la Image); ThemeApplier conflictiría |
| RequestItem/AvatarFrame/AvatarMask/AvatarImage | Foto/avatar del jugador — contenido de usuario, nunca tintable |
| RequestItem/InfoSection | Solo RectTransform, sin Image |
| RequestItem/ButtonsRow | Solo RectTransform + HorizontalLayoutGroup, sin Image |

#### Casos especiales (2 ThemeAppliers en mismo objeto)
| Objeto | ThemeApplier #1 | ThemeApplier #2 | Razón |
|---|---|---|---|
| TabsBar/ReceivedTab | TabActive · applyToImage=true | Glow · applyToOutline=true | Image de fondo del tab + Outline de borde |
| TabsBar/SentTab | TabInactive · applyToImage=true | Glow · applyToOutline=true | Image de fondo del tab inactivo + Outline de borde |
| RequestItem root | CardBackground · applyToImage=true | Glow · applyToOutline=true | Fondo de card + borde neon |
| RequestItem/ButtonsRow/AcceptButton | ButtonSuccess · applyToImage=true | ButtonGlowSuccess · applyToOutline=true | Botón accept verde + glow |
| RequestItem/ButtonsRow/RejectButton | ButtonDanger · applyToImage=true | ButtonGlowDanger · applyToOutline=true | Botón reject rojo + glow |
| RequestItem/ButtonsRow/CancelButton | ButtonSecondary · applyToImage=true | Glow · applyToOutline=true | Botón cancel oscuro + glow (SetActive false, visible solo en tab Sent) |

#### Objetos ocultos al inicio (TABLE 4 — referencia)
| Objeto | Motivo oculto | Visible cuando |
|---|---|---|
| Content/RequestsEmptyText | SetActive(false) en código | No hay solicitudes en la lista activa |
| Content/LoadingIndicator | SetActive(false) en código | Durante carga async de solicitudes |
| RequestItem/ButtonsRow/CancelButton | SetActive(false) en prefab | Tab Sent activo (solicitudes enviadas) |

#### Notas de escena
1. **SwitchTab() runtime override CRÍTICO**: `FriendRequestsSceneManager.SwitchTab()` asigna `receivedTabBg.color`, `receivedTabText.color`, `sentTabBg.color`, `sentTabText.color` con colores hardcodeados (`ACTIVE_TAB`, `INACTIVE_TAB`, `ACTIVE_TEXT`, `INACTIVE_TEXT`). Esto sobrescribe ThemeApplier. Fix: reemplazar constantes por `ThemeManager.current.tabActive`, `ThemeManager.current.tabInactive`, `ThemeManager.current.textOnPrimary`, `ThemeManager.current.textSecondary`.
2. **Indicator bars bug (no scope)**: El Builder crea `ReceivedTab/Indicator` con CYAN_NEON y `SentTab/Indicator` con Color.clear. El Manager `SwitchTab()` no actualiza los Indicators al cambiar de tab — la barra indicadora siempre queda en Received. Es un bug visual menor fuera del scope de ThemeApplier.
3. **TabsBar.Image = Color.clear**: El contenedor de tabs no tiene fondo visible — es solo un layout container. NO añadir ThemeApplier.
4. **FrameRenderer en AvatarFrame**: El componente `FrameRenderer.SetRenderMode(Reduced)` controla la Image de AvatarFrame para mostrar marcos cosméticos del perfil. ThemeApplier conflictiría — NO añadir.
5. **AvatarMask como placeholder**: `AvatarMask.Image` (color CARD_BG_LIGHT) es visible antes de que cargue el avatar. Una vez carga, `AvatarImage` lo cubre completamente. Asignar `CardBackground` para que coincida con el tema del card.
6. **ThemeApplier en prefab vía código**: Los RequestItems se instancian en runtime por `CreateRequestItem()`. ThemeApplier debe añadirse programáticamente (o el prefab debe tenerlo ya configurado) para que se respete el tema al instanciar. Añadir ThemeApplier directo al prefab `RequestItem.prefab` es suficiente — Unity aplica los componentes del prefab al instanciar.
7. **CancelButton semántico naranja**: El texto `CancelRequestText` usa `ORANGE_CANCEL` — mapea a `Warning` ElementType (`warningColor`). Matches la intención visual de "acción con consecuencia" sin ser ButtonDanger (rojo).
8. **Screenshot confirmada**: Screenshot muestra escena FriendRequests con "FRIEND REQUESTS" + "2 pending", tabs Received(activo/cyan)/Sent, 3 cards (CoolPlayer42, GamerPro, DigitFan) con Accept(verde) + Reject(rojo). Coincide exactamente con los sample names del Builder y la jerarquía del código.

---

### 23 · `Social/Friends/SearchPlayers.unity` — 📝 V52

#### Objetos a TINTAR
| Objeto (GameObject path) | Componente | ElementType | ThemeData Property | Color NeonDark | Notas |
|---|---|---|---|---|---|
| Background | Image | PrimaryBackground | `primaryBackground` | #050A14 | Reset color a white |
| Header | Image | SecondaryBackground | `secondaryBackground` | #0A1019 | Reset color a white |
| Header/TitleText | TextMeshProUGUI | Accent | `primaryAccent` | #00FFFF | Reset color a white; "SEARCH PLAYERS" |
| SearchBar | Image | InputBackground | `inputBackground` | #141E2E | Dual con Outline; reset color a white |
| SearchBar | Outline | InputBorder | `inputBorder` | #006666 | Dual con Image; reset effectColor a white |
| SearchBar/SearchIcon | Image | InputPlaceholder | `inputPlaceholder` | #66666B | ⚠️ BORDERLINE — verificar que ProfileIcon/SearchIcon.png sea sprite blanco puro; reset color a white; tint debe coincidir con placeholder |
| SearchBar/SearchInputField/Text Area/Placeholder | TextMeshProUGUI | InputPlaceholder | `inputPlaceholder` | #66666B | Reset color a white; "Search by username..." |
| SearchBar/SearchInputField/Text Area/Text | TextMeshProUGUI | TextPrimary | `textPrimary` | #F2F2F2 | Texto tecleado; reset color a white |
| SearchBar/ClearButton | Image | ButtonPrimary | `buttonPrimary` | #00FFFF | Dual con Outline; SetActive(false) al inicio — mostrado cuando hay texto; reset color a white |
| SearchBar/ClearButton | Outline | ButtonGlowPrimary | `glowColor` | cyan @40% | Dual con Image; reset effectColor a white |
| SearchBar/ClearButton/ClearSearchText | TextMeshProUGUI | TextOnPrimary | `textOnPrimary` | #050A14 | Texto oscuro sobre botón cyan; reset color a white |
| ResultsPanel/ResultsScrollView/Scrollbar Vertical | Image | ScrollbarTrack | `scrollbarTrack` | #1A1A1A @50% | Pista del scrollbar; reset color a white |
| ResultsPanel/ResultsScrollView/Scrollbar Vertical/Sliding Area/Handle | Image | ScrollbarHandle | `scrollbarHandle` | #006666 | Handle del scrollbar; reset color a white |
| ResultsPanel/EmptyState/EmptyIcon | Image | Accent | `primaryAccent` | #00FFFF | ⚠️ BORDERLINE — verificar que ProfileIcon.png sea sprite blanco puro; reset color a white |
| ResultsPanel/EmptyState/SearchEmptyTitle | TextMeshProUGUI | TextPrimary | `textPrimary` | #F2F2F2 | Reset color a white; "Search players" |
| ResultsPanel/EmptyState/SearchEmptyDesc | TextMeshProUGUI | TextSecondary | `textSecondary` | #999999 | Reset color a white; descripción gris |
| ResultsPanel/NoResultsText | TextMeshProUGUI | TextSecondary | `textSecondary` | #66666B | SetActive(false); "No players found" |
| ResultsPanel/LoadingIndicator/Text | TextMeshProUGUI | Accent | `primaryAccent` | #00FFFF | SetActive(false) junto con LoadingIndicator; "Searching..." |
| **— PlayerCard prefab (×N instancias runtime) —** | | | | | |
| PlayerCard | Image | CardBackground | `cardBackground` | #0F1420 | Sin Outline en builder; reset color a white; añadir ThemeApplier vía código en CreatePlayerItem() |
| PlayerCard/AvatarFrame/AvatarMask | Image | CardBackground | `cardBackground` | #141929 | Círculo placeholder detrás del avatar; reset color a white |
| PlayerCard/ContentSection/TopRow/Username | TextMeshProUGUI | TextPrimary | `textPrimary` | #F2F2F2 | Reset color a white |
| PlayerCard/ContentSection/StatsRow/StatsText | TextMeshProUGUI | TextSecondary | `textSecondary` | #808080 | "0% WR · Digit Rush"; reset color a white |
| PlayerCard/ContentSection/ButtonsRow/AddFriendButton | Image | ButtonPrimary | `buttonPrimary` | #00FFFF | Dual con Outline; puede estar SetActive(false) si ya es amigo; reset color a white |
| PlayerCard/ContentSection/ButtonsRow/AddFriendButton | Outline | ButtonGlowPrimary | `glowColor` | cyan @50% | Dual con Image; reset effectColor a white |
| PlayerCard/ContentSection/ButtonsRow/AddFriendButton/Text | TextMeshProUGUI | TextOnPrimary | `textOnPrimary` | #050A14 | "Add" / "Request Sent"; reset color a white |
| PlayerCard/ContentSection/ButtonsRow/ViewProfileButton | Image | ButtonSecondary | `buttonSecondary` | #0D1926 | Dual con Outline; reset color a white |
| PlayerCard/ContentSection/ButtonsRow/ViewProfileButton | Outline | Glow | `glowColor` | cyan @100% | Dual con Image; borde cyan del botón secundario; reset effectColor a white |
| PlayerCard/ContentSection/ButtonsRow/ViewProfileButton/ViewProfileBtnText | TextMeshProUGUI | Accent | `primaryAccent` | #00FFFF | Texto cyan sobre botón secundario; reset color a white |

#### Objetos que NO se tintan
| Objeto (GameObject path) | Razón |
|---|---|
| Header/BackButton | Prefab gestionado independientemente |
| SearchBar/SearchInputField | TMP_InputField controller — sin Image propia |
| SearchBar/SearchInputField/Text Area | RectMask2D — sin Image |
| SearchBar/SearchButton | Vestigial: SetActive(false) permanente, solo tiene Button (sin Image) — sin rol visual |
| ResultsPanel | Image set a Color.clear explícitamente — contenedor transparente sin rol visual |
| ResultsPanel/ResultsScrollView | ScrollRect — sin Image |
| ResultsPanel/ResultsScrollView/Viewport | Image Color.clear — solo para raycast/drag detection, sin rol visual |
| ResultsPanel/ResultsScrollView/Viewport/ResultsContainer | RectTransform + LayoutGroup — sin Image |
| ResultsPanel/ResultsScrollView/Scrollbar Vertical/Sliding Area | RectTransform — sin Image |
| ResultsPanel/EmptyState | RectTransform — sin Image |
| ResultsPanel/LoadingIndicator | RectTransform — sin Image (SetActive false) |
| PlayerCard/AvatarFrame | FrameRenderer gestiona marcos cosméticos de perfil — ThemeApplier entraría en conflicto |
| PlayerCard/AvatarFrame/AvatarMask/AvatarImage | Foto/avatar del jugador — contenido de foto, no tintable |
| PlayerCard/ContentSection | RectTransform — sin Image |
| PlayerCard/ContentSection/TopRow | RectTransform — sin Image |
| PlayerCard/ContentSection/TopRow/OnlineStatus | Runtime sobreescribe color (verde=online / gris=offline) — indicador semántico de estado |
| PlayerCard/ContentSection/TopRow/OnlineLabel | Runtime sobreescribe color y texto (verde/gris + "Online"/"Offline") — semántico |
| PlayerCard/ContentSection/StatsRow | RectTransform — sin Image |
| PlayerCard/ContentSection/ButtonsRow | RectTransform + HorizontalLayoutGroup — sin Image |

#### Casos especiales (2 ThemeAppliers en mismo objeto)
| Objeto | ThemeApplier #1 | ThemeApplier #2 | Razón |
|---|---|---|---|
| SearchBar | InputBackground (applyToImage=true) | InputBorder (applyToOutline=true) | Fondo input + borde del search field |
| SearchBar/ClearButton | ButtonPrimary (applyToImage=true) | ButtonGlowPrimary (applyToOutline=true) | Botón cyan CTA + glow |
| PlayerCard/…/AddFriendButton | ButtonPrimary (applyToImage=true) | ButtonGlowPrimary (applyToOutline=true) | Botón "+ Add" cyan + glow |
| PlayerCard/…/ViewProfileButton | ButtonSecondary (applyToImage=true) | Glow (applyToOutline=true) | Botón secundario oscuro + borde cyan |

#### Objetos ocultos al inicio (SetActive false)
| Objeto | ElementType | Condición de activación |
|---|---|---|
| SearchBar/ClearButton | ButtonPrimary+ButtonGlowPrimary | Mostrado cuando el input tiene texto (`UpdateClearButtonVisibility()`) |
| ResultsPanel/NoResultsText | TextSecondary | Mostrado cuando la búsqueda no devuelve resultados |
| ResultsPanel/LoadingIndicator | RectTransform (con Text hijo: Accent) | Mostrado durante búsqueda async en Firebase |

#### Notas de escena
1. **PlayerCard prefab** (`Assets/_Project/Prefabs/Common/PlayerCard.prefab`): Sin Outline en el root — solo Image → un único ThemeApplier CardBackground. ThemeApplier debe añadirse vía código en `CreatePlayerItem()`.
2. **SearchIcon ⚠️**: Builder asigna `color = PLACEHOLDER_COLOR` (gray) sobre `SearchIcon.png` — diseñado para ser tintable. Verificar que el sprite sea blanco puro antes de aplicar ThemeApplier InputPlaceholder. Si tiene color baked in → mover a TABLE 2 NO.
3. **EmptyIcon ⚠️**: Builder asigna `color = CYAN_NEON` sobre `ProfileIcon.png` — diseñado para ser tintable. Verificar que el sprite sea blanco puro antes de aplicar ThemeApplier Accent. Si tiene color baked in → mover a TABLE 2 NO.
4. **OnlineStatus + OnlineLabel**: `SetupBasicPlayerItem()` sobreescribe colores (verde/gris) en runtime — ambos son indicadores de estado semántico. NO ThemeApplier.
5. **Sin runtime overrides en UI estructural**: El Manager no sobreescribe colores de Header/SearchBar/Panel → ThemeApplier completamente seguro en la UI principal.

---

### 24 · `Social/Notifications/Notifications.unity` — 📝 Auditado V52

#### TABLE 1 — Objetos a TINTAR
| Objeto (GameObject path) | Componente | ElementType | ThemeData Property | Color NeonDark | Notas |
|---|---|---|---|---|---|
| `Background` | Image | PrimaryBackground | `primaryBackground` | #050A14 | Reset Image.color a blanco |
| `Header` | Image | SecondaryBackground | `secondaryBackground` | #0A1020 | Reset Image.color a blanco |
| `Header/BackButton` | Image (Button root) | ButtonSecondary | `buttonSecondary` | #1A2030 | Prefab compartido — ThemeApplier configurado en prefab |
| `Header/TitleText` | TextMeshProUGUI | Accent | `primaryAccent` | #00E5FF | Título en CYAN_NEON |
| `Header/CountText` | TextMeshProUGUI | TextSecondary | `textSecondary` | #6B7280 | Ej. "0 unread" |
| `Tabs` | Image | SecondaryBackground | `secondaryBackground` | #0A1020 | Barra contenedora de tabs; DUAL con Outline (TABLE 3) |
| `Tabs/TabAll` | Image | TabActive | `tabActive` | theme | Tab inicial activo |
| `Tabs/TabAll/Label` | TextMeshProUGUI | TabActive | `tabActive` | theme | Texto del tab activo |
| `Tabs/TabSocial` | Image | TabInactive | `tabInactive` | theme | |
| `Tabs/TabSocial/Label` | TextMeshProUGUI | TabInactive | `tabInactive` | theme | |
| `Tabs/TabGames` | Image | TabInactive | `tabInactive` | theme | |
| `Tabs/TabGames/Label` | TextMeshProUGUI | TabInactive | `tabInactive` | theme | |
| `Tabs/TabRewards` | Image | TabInactive | `tabInactive` | theme | |
| `Tabs/TabRewards/Label` | TextMeshProUGUI | TabInactive | `tabInactive` | theme | |
| `ScrollView/Content/EmptyText` | TextMeshProUGUI | TextSecondary | `textSecondary` | #6B7280 | Estado vacío |
| `ScrollView/Content/LoadingIndicator` | Image | Accent | `primaryAccent` | #00E5FF | Oculto al inicio; reset Image.color a blanco |
| `NotificationCard/Outline` | Outline (en card root) | Glow | `glowColor` | #00E5FF @35% | El Image del root es NO (runtime); solo el Outline recibe ThemeApplier |
| `NotificationCard/UnreadDot` | Image | Accent | `primaryAccent` | #00E5FF | Solo SetActive cambia; color no overridden en runtime; reset a blanco |
| `NotificationCard/TypeIcon/bg` | Image | SecondaryBackground | `secondaryBackground` | white@6% | Fondo sutil detrás del icono de tipo; reset a blanco |
| `NotificationCard/InfoSection/Timestamp` | TextMeshProUGUI | TextSecondary | `textSecondary` | #6B7280 | No overridden en runtime |
| `NotificationCard/InfoSection/Body` | TextMeshProUGUI | TextSecondary | `textSecondary` | #6B7280 | No overridden en runtime |
| `NotificationCard/InfoSection/SenderName` | TextMeshProUGUI | AccentSecondary | `secondaryAccent` | #8B5CF6 | SOCIAL_COLOR fijo (azul social); no overridden en runtime · ⚠️ BORDERLINE — verificar con diseñador si debe ser AccentSecondary o color fijo |
| `NotificationCard/ActionsRow/PrimaryButton` | Image | ButtonPrimary | `buttonPrimary` | #00E5FF | DUAL con Outline (TABLE 3); reset a blanco |
| `NotificationCard/ActionsRow/PrimaryButton/Text` | TextMeshProUGUI | TextOnPrimary | `textOnPrimary` | #050A14 | Texto oscuro sobre botón cyan |
| `NotificationCard/ActionsRow/SecondaryButton` | Image | ButtonSecondary | `buttonSecondary` | #1A2030 | DUAL con Outline (TABLE 3); reset a blanco |
| `NotificationCard/ActionsRow/SecondaryButton/Text` | TextMeshProUGUI | TextSecondary | `textSecondary` | #6B7280 | |
| `Footer` | Image | SecondaryBackground | `secondaryBackground` | #0A1020 | Reset a blanco |
| `Footer/TopLine` | Image | Glow | `glowColor` | #00E5FF @30% | Línea divisora accent |
| `Footer/MarkAllReadButton` | Image | ButtonSecondary | `buttonSecondary` | white@4% | DUAL con Outline (TABLE 3); fondo muy sutil; reset a blanco |
| `GroupSeparator` | TextMeshProUGUI | TextSecondary | `textSecondary` | #6B7280 | Creado en runtime por `CreateGroupSeparator()` — ThemeApplier debe añadirse via código en NotificationsManager, no en Inspector |

#### TABLE 2 — Objetos que NO se tintan
| Objeto (GameObject path) | Razón |
|---|---|
| `Tabs/TabAll/Indicator` | Runtime: `SetTabState()` usa `DOColor(CYAN_NEON)` en tab activo y por categoría; Tab All = cyan, Social = azul social, Games = naranja, Rewards = gold — multi-color semántico |
| `Tabs/TabSocial/Indicator` | Runtime: `SetTabState()` asigna color social-blue específico via DOColor — semántico por categoría |
| `Tabs/TabGames/Indicator` | Runtime: `SetTabState()` asigna GAMES_COLOR (naranja/verde) via DOColor — semántico por categoría |
| `Tabs/TabRewards/Indicator` | Runtime: `SetTabState()` asigna REWARDS_COLOR (gold) via DOColor — semántico por categoría |
| `ScrollView` | Image Color.clear — contenedor transparente, sin rol visual |
| `ScrollView/Viewport` | Image Color.clear — viewport del ScrollRect, solo máscara de recorte |
| `ScrollView/Content` | Sin componente Image ni Text — solo layout group (VLG + CSF) |
| `NotificationCard` (root Image) | Runtime: `SetupNotificationCard()` sobreescribe `cardBg.color = isRead ? READ_BG : UNREAD_BG` — estado de lectura semántico |
| `NotificationCard/TypeIcon/IconImage` | Runtime: `typeIconImg.color = GetTypeColor(notification.type)` — cyan/azul/naranja/gold por tipo de notificación; multi-color semántico |
| `NotificationCard/InfoSection/Title` | Runtime: `titleTmp.color` = gris apagado (leída) o blanco brillante (no leída) — estado de lectura semántico |
| `NotificationCard/InfoSection/ActionsRow` | Sin componente Image — solo HLG layout |
| `Footer/MarkAllReadButton/MarkAllReadText` | Runtime: `UpdateMarkAllReadButton()` sobreescribe `markAllReadText.color = unread > 0 ? CYAN_NEON : TAB_INACTIVE` — estado de interacción semántico |

#### TABLE 3 — Casos especiales (2 ThemeAppliers en mismo objeto)
| Objeto | ThemeApplier #1 | ThemeApplier #2 | Razón |
|---|---|---|---|
| `Tabs` | SecondaryBackground · applyToImage=true | Glow · applyToOutline=true | Image = fondo barra de tabs; Outline = borde cyan sutil |
| `NotificationCard/ActionsRow/PrimaryButton` | ButtonPrimary · applyToImage=true | ButtonGlowPrimary · applyToOutline=true | Image = fill cyan del botón; Outline = glow |
| `NotificationCard/ActionsRow/SecondaryButton` | ButtonSecondary · applyToImage=true | Glow · applyToOutline=true | Image = fill oscuro; Outline = borde sutil |
| `Footer/MarkAllReadButton` | ButtonSecondary · applyToImage=true | Glow · applyToOutline=true | Image = fondo muy sutil (white@4%); Outline = borde cyan |

#### Objetos ocultos al inicio (SetActive false)
| Objeto | Componente | Estado inicial | Nota |
|---|---|---|---|
| `ScrollView/Content/EmptyText` | TextMeshProUGUI | Oculto | Mostrado cuando lista vacía |
| `ScrollView/Content/LoadingIndicator` | Image | Oculto | Mostrado durante carga |

#### Notas de escena
1. **Tab Indicators — runtime multi-color**: Los 4 indicadores (`TabAll/Indicator`, `TabSocial/Indicator`, `TabGames/Indicator`, `TabRewards/Indicator`) usan DOColor con colores por categoría (CYAN_NEON, SOCIAL_COLOR, GAMES_COLOR, REWARDS_COLOR). No se pueden tematizar — cada categoría tiene su color semántico fijo.
2. **NotificationCard root Image — NO**: `SetupNotificationCard()` sobreescribe el color del Image según `isRead` (READ_BG vs UNREAD_BG). El Outline del card SÍ recibe ThemeApplier (Glow), ya que no está overridden en runtime.
3. **GroupSeparator — ThemeApplier via code**: El separador de grupo es un TextMeshProUGUI creado dinámicamente en `CreateGroupSeparator()`. ThemeApplier debe registrarse via código en NotificationsManager al instanciar el separador (igual que el patrón de otros managers que crean UI en runtime).
4. **SenderName BORDERLINE**: `InfoSection/SenderName` usa SOCIAL_COLOR (azul) fijo, no overridden en runtime. Mapeado a AccentSecondary — verificar con diseñador si debe seguir siendo azul social fijo en todos los temas o usar el acento secundario del tema.
5. **Tab backgrounds SÍ tematizables**: A diferencia de FriendRequests (donde `SwitchTab()` sobreescribía ACTIVE_TAB/INACTIVE_TAB hardcodeados), en Notifications el Manager solo sobreescribe los Indicators vía DOColor — los fondos de los tabs Images NO son overridden y pueden recibir ThemeApplier seguro.

---

### 25 · `Monetization/Shop.unity` — 📝 Auditado V52

#### TABLE 1 — Objetos a TINTAR
| Objeto (GameObject path) | Componente | ElementType | ThemeData Property | Color NeonDark | Notas |
|---|---|---|---|---|---|
| `Background` | Image | PrimaryBackground | `primaryBackground` | #050A14 | — |
| `SafeArea/Header` | Image | SecondaryBackground | `secondaryBackground` | #0A1020 | Banda header oscura |
| `SafeArea/Header/TitleText` | TextMeshProUGUI | TextTitle | `textTitle` | #00FFFF | — |
| `SafeArea/Header/BackButton` | Button + Image | ButtonSecondary | `buttonSecondary` | #1A2030 | Prefab — ThemeApplier ya configurado en prefab |
| `PurchaseBlocker` | Image | Overlay | `overlayColor` | rgba(0,0,0,0.7) | Oculto SetActive false; mostrado al confirmar compra; dual #1 (TABLE 3) |
| `PurchaseBlocker/PurchasePopup/Preview/Amount` | TextMeshProUGUI | TextPrimary | `textPrimary` | #E8F2FF | Oculto vía parent; runtime escribe nombre/cantidad del ítem |
| `PurchaseBlocker/PurchasePopup/Price` | TextMeshProUGUI | TextSecondary | `textSecondary` | #9999A6 | Oculto vía parent; runtime escribe precio formateado |
| `PurchaseBlocker/PurchasePopup/Buttons/CancelButton` | Image | ButtonSecondary | `buttonSecondary` | #1A2030 | Oculto vía parent |
| `PurchaseBlocker/PurchasePopup/Buttons/ConfirmButton` | Image | ButtonSuccess | `buttonSuccess` | #33E666 | Oculto vía parent |
| `NotEnoughBlocker` | Image | Overlay | `overlayColor` | rgba(0,0,0,0.7) | Oculto SetActive false; mostrado cuando gems insuficientes; dual #1 (TABLE 3) |
| `NotEnoughBlocker/NotEnoughPopup/NotEnoughText` | TextMeshProUGUI | TextPrimary | `textPrimary` | #E8F2FF | Oculto vía parent |
| `NotEnoughBlocker/NotEnoughPopup/Buttons/CloseButton` | Image | ButtonSecondary | `buttonSecondary` | #1A2030 | Oculto vía parent |
| `NotEnoughBlocker/NotEnoughPopup/Buttons/GetGemsButton` | Image | ButtonPrimary | `buttonPrimary` | #00E5FF | Oculto vía parent |
| Todos los `ThemeCardV4` root (×~24 cards — PremiumThemes + EarnableThemes) | Image | TertiaryBackground | `tertiaryBackground` | #0F1724 | Patrón repetido; UIBuilder loop añade ThemeApplier a cada card |
| Todos los `ThemeCardV4/NameText` (×~24) | TextMeshProUGUI | TextPrimary | `textPrimary` | #E8F2FF | Patrón repetido |
| Todos los `CosmeticCardV4` root (×~32 — Frames ×25 + Effects ×7) | Image | TertiaryBackground | `tertiaryBackground` | #0F1724 | Patrón repetido |
| Todos los `CosmeticCardV4/NameText` (×~32) | TextMeshProUGUI | TextPrimary | `textPrimary` | #E8F2FF | Patrón repetido |
| Todos los `TitleCardV4` root (×~20) | Image | TertiaryBackground | `tertiaryBackground` | #0F1724 | Patrón repetido |
| Todos los `TitleCardV4/NameText` (×~20) | TextMeshProUGUI | TextPrimary | `textPrimary` | #E8F2FF | Patrón repetido |
| Todos los `BattleCardV4` root (×~16) | Image | TertiaryBackground | `tertiaryBackground` | #0F1724 | Patrón repetido |
| Todos los `BattleCardV4/NameText` (×~16) | TextMeshProUGUI | TextPrimary | `textPrimary` | #E8F2FF | Patrón repetido |
| Todos los `CurrencyCardV4` root (×~11 — GemsGrid ×7 + CoinsGrid ×4) | Image | TertiaryBackground | `tertiaryBackground` | #0F1724 | Patrón repetido |
| Todos los `CurrencyCardV4/NameText` (×~11) | TextMeshProUGUI | TextPrimary | `textPrimary` | #E8F2FF | Patrón repetido |

#### TABLE 2 — Objetos que NO se tintan
| Objeto (GameObject path) | Razón |
|---|---|
| `SafeArea/Header/CurrencyDisplay` | Container sin Image |
| `SafeArea/Header/CurrencyDisplay/GemsDisplay` | Container sin Image |
| `SafeArea/Header/CurrencyDisplay/GemsDisplay/Icon` | Color semántico GEM_COLOR (cyan fijo) — currency display estático |
| `SafeArea/Header/CurrencyDisplay/GemsDisplay/Amount` | Color semántico GEM_COLOR; `AnimateCurrencyChange()` flash temporal que restaura color original — no themed |
| `SafeArea/Header/CurrencyDisplay/CoinsDisplay` | Container sin Image |
| `SafeArea/Header/CurrencyDisplay/CoinsDisplay/Icon` | Color semántico COIN_COLOR (gold fijo) — currency display estático |
| `SafeArea/Header/CurrencyDisplay/CoinsDisplay/Amount` | Color semántico COIN_COLOR; mismo patrón de flash que Gems |
| `ShopScrollView` | ScrollRect container — Image transparente (solo input) |
| `ShopScrollView/Viewport` | RectMask2D — sin rol visual propio |
| `ShopScrollView/Viewport/Content` | VerticalLayoutGroup container — sin Image |
| Todos los `SectionDividerV5` y sus hijos (TopBorder, ContentArea, BottomBorder, SectionTitle) ×~10 dividers | Color de acento semántico por sección (GOLD=currency, PURPLE_PREMIUM=themes/bundles, CYAN_NEON=daily/featured) — identidad visual fija por sección |
| Todos los `ThemeCardV4/ColorPreview` ó `ColorSwatch` | Arte cosmético: color real del tema en venta — identificador visual del producto; NO themed |
| Todos los `CosmeticCardV4/FramePreview` ó `ItemPreview` (Image) | Arte cosmético (diseño del frame/efecto/battlecard) — color fijo del producto |
| Todos los `TitleCardV4/TitleColorDot` ó `TitlePreview` | Color del título en venta — semántico del producto |
| Todos los items `Outline` (card outline, ×~103) | `itemColor` del cosmético — color del producto, no del tema |
| Todos los `BuyButton` Image (×~103) | Color semántico por tipo de precio: verde=IAP real, cyan=DigitGems, gold=DigitCoins |
| Todos los `BuyButton/Text` TMP (×~103) | TEXT_DARK sobre botón coloreado — contraste fijo |
| Todos los badges POPULAR / BEST VALUE / EQUIPPED / LOCKED / COMING SOON | Estado semántico fijo |
| `WelcomePackBanner`/`OfferBannerV4`/`PremiumBundleBanner`/`VIPBundle` BannerBg (Image) | Arte de marketing con gradiente/color propio de la oferta |
| `WelcomePackBanner`/offer banners `Shadow` y `Side` (Image) | Sombra y borde 3D — colores derivados del banner, no del tema |
| `DailyOffersContainer/DailyTimerBar` y `CountdownText` | Countdown vivo gestionado por `DailyOfferUIController`; color ORANGE semántico |
| `FooterSection/DisclaimerText` | Texto legal muted — color fijo TEXT_MUTED |
| `PurchaseBlocker/PurchasePopup/Preview/Icon` | `image.color = itemData.accentColor` asignado en runtime al abrir popup — color del ítem en compra, no del tema |

#### TABLE 3 — Casos especiales (2 ThemeAppliers en mismo objeto)
| Objeto | ThemeApplier #1 | ThemeApplier #2 | Razón |
|---|---|---|---|
| `PurchaseBlocker/PurchasePopup` | Image → SecondaryBackground | Outline → AccentPrimary | Panel de confirmación con borde temático |
| `NotEnoughBlocker/NotEnoughPopup` | Image → SecondaryBackground | Outline → AccentDanger | Panel de error con borde rojo/danger |

#### TABLE 4 — Ocultos (SetActive false en Start)
| Objeto | ¿ThemeApplier? | Razón |
|---|---|---|
| `PurchaseBlocker` | **SÍ** — Overlay | Overlay oscuro del popup de compra |
| `PurchaseBlocker/PurchasePopup` | **SÍ** — SecondaryBackground + Outline dual | Panel principal del popup |
| `PurchaseBlocker/PurchasePopup/Preview/Icon` | **NO** | `itemData.accentColor` runtime |
| `PurchaseBlocker/PurchasePopup/Preview/Amount` | **SÍ** — TextPrimary | Nombre del ítem |
| `PurchaseBlocker/PurchasePopup/Price` | **SÍ** — TextSecondary | Precio formateado |
| `PurchaseBlocker/PurchasePopup/Buttons/CancelButton` | **SÍ** — ButtonSecondary | Botón cancelar |
| `PurchaseBlocker/PurchasePopup/Buttons/ConfirmButton` | **SÍ** — ButtonSuccess | Botón confirmar |
| `NotEnoughBlocker` | **SÍ** — Overlay | Overlay oscuro |
| `NotEnoughBlocker/NotEnoughPopup` | **SÍ** — SecondaryBackground + Outline dual | Panel principal |
| `NotEnoughBlocker/NotEnoughPopup/NotEnoughText` | **SÍ** — TextPrimary | Mensaje "no tienes suficientes gems" |
| `NotEnoughBlocker/NotEnoughPopup/Buttons/CloseButton` | **SÍ** — ButtonSecondary | Botón cerrar |
| `NotEnoughBlocker/NotEnoughPopup/Buttons/GetGemsButton` | **SÍ** — ButtonPrimary | Botón "conseguir gems" |

#### Notas de escena
- **Sin runtime color-overrides**: `ShopManager.Start()` no sobreescribe colores de Image/TMP estructurales — ThemeApplier es seguro en toda la escena.
- **AnimateCurrencyChange**: DOTween captura `headerText.color` al momento de la animación y restaura el original — los textos de CurrencyDisplay son semánticos (GEM_COLOR / COIN_COLOR), no temáticos.
- **popupItemIcon runtime**: `_popupItemIcon.color = itemData.accentColor` asignado cada vez que se abre el popup con un ítem → TABLE 2 (NO ThemeApplier).
- **Patrón de cards ×103**: ThemeApplier añadido en los loops del UIBuilder para cada card creada. Dos entradas por card: Image(TertiaryBackground) + NameText(TextPrimary). Total representado: ~206 componentes ThemeApplier en cards.
- **Section dividers**: color semántico por sección (oro para currency, púrpura para premium, cian para featured) — corresponden a la identidad de cada categoría, no al tema de la app. Sin ElementType equivalente → fijos.
- **Item outlines**: cada card usa `itemColor` (el color cosmético del producto) como outline → identificador visual del producto; no temático.
- **BackButton prefab**: ThemeApplier ya configurado en el prefab (ButtonSecondary). No añadir second ThemeApplier en escena.

---

### 26 · `Monetization/DailyRewards.unity` — 📝 Auditado V52

#### TABLE 1 — Objetos a TINTAR
| Objeto (GameObject path) | Componente | ElementType | ThemeData Property | Color NeonDark | Notas |
|---|---|---|---|---|---|
| `Background` | Image | PrimaryBackground | `primaryBackground` | #050A14 | — |
| `TopBar/TitleText` | TextMeshProUGUI | TextTitle | `textTitle` | #00FFFF | Reset Image.color a blanco antes de aplicar |
| `TopBar/BackButton` | Button + Image | ButtonSecondary | `buttonSecondary` | #1A2030 | Prefab — ThemeApplier ya configurado en el prefab |
| `ClaimButton` | Image | ButtonSuccess | `buttonSuccess` | #33E666 | Reset Image.color a blanco; dual #1 (TABLE 3) |
| `ClaimButton/ClaimRewardText` | TextMeshProUGUI | TextOnSuccess | `textOnPrimary` | #050D14 | Runtime escribe texto localizado (dr_claim_reward / dr_claimed) |
| `TimerBar/TimeText` | TextMeshProUGUI | TextSecondary | `textSecondary` | #9999A6 | Runtime escribe countdown localizado cada segundo |
| `ClaimAnimationBlocker/DarkOverlay` | Image | Overlay | `overlayColor` | rgba(0,0,0,0.7) | Oculto vía parent (ClaimAnimationBlocker SetActive false) |
| `ClaimAnimationBlocker/RewardContainer` | Image | TertiaryBackground | `tertiaryBackground` | #0F1724 | Oculto vía parent; dual #1 (TABLE 3); Reset Image.color |
| `ClaimAnimationBlocker/RewardContainer/CelebTitle` | TextMeshProUGUI | AccentTertiary | `tertiaryAccent` | #FFD700 | Oculto vía parent; Reset color a blanco |
| `ClaimAnimationBlocker/ContinueButton/TapToContinueText` | TextMeshProUGUI | TextSecondary | `textSecondary` | #9999A6 | Oculto vía parent; texto fijo "TAP TO CONTINUE" |

#### TABLE 2 — Objetos que NO se tintan
| Objeto (GameObject path) | Razón |
|---|---|
| `TopBar` | Container RectTransform — sin componente Image |
| `TopBar/CurrencyRow` | Currency pills — objetos estáticos, no se modifican con ThemeApplier (decisión de diseño) |
| `TopBar/CurrencyRow/CoinsPill` | Currency pill estático |
| `TopBar/CurrencyRow/GemsPill` | Currency pill estático |
| `TopBar/CurrencyRow/CoinsAddButton` | Currency pill estático |
| `TopBar/CurrencyRow/GemsAddButton` | Currency pill estático |
| `TopBar/CurrencyRow/CoinsIcon` | Currency pill estático |
| `TopBar/CurrencyRow/GemsIcon` | Currency pill estático |
| `TopBar/CurrencyRow/CoinsValueText` | Currency pill estático |
| `TopBar/CurrencyRow/GemsValueText` | Currency pill estático |
| `RewardsScrollView` | Image clear — input de scroll únicamente, transparente |
| `RewardsScrollView/Viewport` | Image clear — RectMask2D, sin rol visual |
| `RewardsScrollView/Viewport/Content` | Container VerticalLayoutGroup — sin Image |
| `RewardsScrollView/Viewport/Content/Day1` (Image) | Color gestionado en runtime por `UpdateExistingDayCards()` con colores hardcoded según estado claimed/today/locked — ThemeApplier sería sobreescrito inmediatamente en Start() |
| `RewardsScrollView/Viewport/Content/Day1` (Outline) | Color semántico de estado (claimed=GREEN_SUCCESS, today=GOLD, locked=gris) — gestionado en runtime |
| `RewardsScrollView/Viewport/Content/Day1/DayLabel` | Color semántico de estado runtime (claimed=verde, today=gold, locked=TEXT_SECONDARY hardcoded) |
| `RewardsScrollView/Viewport/Content/Day1/GiftIcon` | Arte multi-color (caja de regalo coloreada) — no es glifo blanco tintable |
| `RewardsScrollView/Viewport/Content/Day2` (Image) | Color gestionado en runtime según estado — ThemeApplier sobreescrito en Start() |
| `RewardsScrollView/Viewport/Content/Day2` (Outline) | Color semántico de estado gestionado en runtime |
| `RewardsScrollView/Viewport/Content/Day2/DayLabel` | Color semántico de estado runtime |
| `RewardsScrollView/Viewport/Content/Day2/GiftIcon` | Arte multi-color |
| `RewardsScrollView/Viewport/Content/Day3` (Image) | Color gestionado en runtime según estado |
| `RewardsScrollView/Viewport/Content/Day3` (Outline) | Color semántico de estado runtime |
| `RewardsScrollView/Viewport/Content/Day3/DayLabel` | Color semántico de estado runtime |
| `RewardsScrollView/Viewport/Content/Day3/GiftIcon` | Arte multi-color |
| `RewardsScrollView/Viewport/Content/Day4` (Image) | Color gestionado en runtime según estado |
| `RewardsScrollView/Viewport/Content/Day4` (Outline) | Color semántico de estado runtime |
| `RewardsScrollView/Viewport/Content/Day4/DayLabel` | Color semántico de estado runtime |
| `RewardsScrollView/Viewport/Content/Day4/GiftIcon` | Arte multi-color |
| `RewardsScrollView/Viewport/Content/Day5` (Image) | Color gestionado en runtime según estado |
| `RewardsScrollView/Viewport/Content/Day5` (Outline) | Color GOLD semántico (día actual = today) |
| `RewardsScrollView/Viewport/Content/Day5/DayLabel` | Color GOLD semántico (hoy) |
| `RewardsScrollView/Viewport/Content/Day5/GiftIcon` | Arte multi-color (caja dorada día 5) |
| `RewardsScrollView/Viewport/Content/Day5/TodayBadge` | UIBuilder placeholder — destruido y recreado por `UpdateExistingDayCards()` en Start(); color GOLD semántico |
| `RewardsScrollView/Viewport/Content/Day5/TodayBadge/Text` | UIBuilder placeholder — destruido y recreado por runtime; color TEXT_DARK semántico |
| `RewardsScrollView/Viewport/Content/Day6` (Image) | Color gestionado en runtime según estado |
| `RewardsScrollView/Viewport/Content/Day6` (Outline) | Color tier-based semántico (purple para días 6) — gestionado en runtime |
| `RewardsScrollView/Viewport/Content/Day6/DayLabel` | Color semántico de estado runtime |
| `RewardsScrollView/Viewport/Content/Day6/GiftIcon` | Arte multi-color (caja purple día 6) |
| `RewardsScrollView/Viewport/Content/Day7` (Image) | Color gestionado en runtime según estado |
| `RewardsScrollView/Viewport/Content/Day7` (Outline) | Color GOLD semántico — día 7 siempre gold (premio especial) |
| `RewardsScrollView/Viewport/Content/Day7/DayLabel` | Color GOLD semántico — day 7 siempre gold |
| `RewardsScrollView/Viewport/Content/Day7/GiftIcon` | Arte multi-color (cofre del tesoro) |
| `RewardsScrollView/Viewport/Content/Day7/GrandPrizeLabel` | Color GOLD semántico — siempre gold para el gran premio |
| `ClaimGlow` | Glow de fondo detrás del botón — alpha 0.15 fijo; runtime show/hide vía `SetActive`; ThemeApplier sobreescribiría el alpha |
| `TimerBar` | Container RectTransform — sin componente Image |
| `ClaimAnimationBlocker` | Image transparent (color=clear) — container invisible del popup |
| `ClaimAnimationBlocker/GiftGlow` | Brillo gold semántico (GOLD @ 25%) — efecto de animación de reclamación, runtime-swapped |
| `ClaimAnimationBlocker/GiftBox` | Arte multi-color (sprite de caja de regalo) — runtime-swapped según día reclamado |
| `ClaimAnimationBlocker/LightBurst` | Flash de animación (blanco cálido @ 0 alpha) — efecto de transición animado |
| `ClaimAnimationBlocker/RewardContainer/RewardRow` | Container HLG — sin Image |
| `ClaimAnimationBlocker/RewardContainer/RewardRow/ClaimRewardIcon` | Arte de icono de moneda/gema — runtime-swapped por tipo de recompensa; no glifo blanco |
| `ClaimAnimationBlocker/RewardContainer/RewardRow/ClaimRewardText` | Color semántico por tipo de recompensa (COIN_COLOR/GEM_COLOR/XP_COLOR) — runtime-set en `PlayClaimAnimation()` |
| `ClaimAnimationBlocker/RewardContainer/StreakInfo` | Color ORANGE_FIRE semántico (fuego/racha) — sin ElementType equivalente en ThemeApplier |
| `ClaimAnimationBlocker/ContinueButton` | Image transparent (color=clear) + Button; área de tap fullscreen; Transition.None |

#### TABLE 3 — Casos especiales (2 ThemeAppliers en mismo objeto)
| Objeto | ThemeApplier #1 | ThemeApplier #2 | Razón |
|---|---|---|---|
| `ClaimButton` | `ButtonSuccess` · applyToImage=true | `ButtonGlowSuccess` · applyToOutline=true | Botón con relleno verde + borde glow success |
| `ClaimAnimationBlocker/RewardContainer` | `TertiaryBackground` · applyToImage=true | `ButtonGlowPremium` · applyToOutline=true | Card elevada en modal + borde gold premium |

#### TABLE 4 — Objetos ocultos (SetActive false)
| Objeto | Estado | YES/NO ThemeApplier |
|---|---|---|
| `ClaimAnimationBlocker` | SetActive(false) — popup de claim animation | NO — container transparente |
| `ClaimAnimationBlocker/DarkOverlay` | Oculto vía parent | **YES** — Overlay |
| `ClaimAnimationBlocker/GiftGlow` | Oculto vía parent | NO — glow gold semántico |
| `ClaimAnimationBlocker/GiftBox` | Oculto vía parent | NO — arte multi-color, runtime-swapped |
| `ClaimAnimationBlocker/LightBurst` | Oculto vía parent | NO — flash de animación |
| `ClaimAnimationBlocker/RewardContainer` | Oculto vía parent | **YES** — dual TertiaryBackground + ButtonGlowPremium |
| `ClaimAnimationBlocker/RewardContainer/CelebTitle` | Oculto vía parent | **YES** — AccentTertiary |
| `ClaimAnimationBlocker/RewardContainer/RewardRow` | Oculto vía parent | NO — container HLG |
| `ClaimAnimationBlocker/RewardContainer/RewardRow/ClaimRewardIcon` | Oculto vía parent | NO — arte coloreado |
| `ClaimAnimationBlocker/RewardContainer/RewardRow/ClaimRewardText` | Oculto vía parent | NO — color semántico por tipo de recompensa |
| `ClaimAnimationBlocker/RewardContainer/StreakInfo` | Oculto vía parent | NO — ORANGE_FIRE semántico |
| `ClaimAnimationBlocker/ContinueButton` | Oculto vía parent | NO — transparente |
| `ClaimAnimationBlocker/ContinueButton/TapToContinueText` | Oculto vía parent | **YES** — TextSecondary |

#### Notas de escena
1. **Day cards runtime override**: `UpdateExistingDayCards()` corre en `Start()` y sobreescribe `Image.color`, `Outline.effectColor` y `DayLabel.color` con colores hardcoded (CARD_BG, GREEN_CLAIMED, GOLD, etc.). ThemeApplier en cards sería sobreescrito inmediatamente. Para soporte completo de temas, el Manager necesita actualización para usar `ThemeManager.current.cardBackground` etc. en lugar de constantes.
2. **Gift icons son arte**: Los iconos `icon_gift_day1–7.png` son imágenes multi-color (cajas coloreadas, cofre del tesoro) — ninguno es glifo blanco. Runtime los asigna como sprites con preserveAspect=true y ajusta opacity según estado.
3. **ClaimGlow**: Semántico del botón success. Runtime solo hace `SetActive(canClaimToday)`. Alpha 0.15 fijo — ThemeApplier sobreescribiría el alpha. Queda como NO.
4. **ORANGE_FIRE StreakInfo**: `StreakInfo` usa `ORANGE_FIRE = (1,0.5,0.1)` para el texto de racha. No existe ElementType para naranja fuego en ThemeApplier. Queda fijo.
5. **TodayBadge runtime**: El UIBuilder crea TodayBadge en Day5 como muestra. `UpdateExistingDayCards()` lo destruye y `CreateTodayBadge()` lo recrea. El badge runtime usa GOLD semántico. Si se quisiera tematizar, habría que añadir ThemeApplier via código en `CreateTodayBadge()`.
6. **CheckOverlay runtime**: `AnimateClaimTransition()` crea CheckOverlay con GREEN_SUCCESS. Runtime-only, no en escena estática.
7. **BackButton prefab**: Prefab con ThemeApplier ya configurado como ButtonSecondary desde audit del prefab.

---

### 27 · `Monetization/DailyMissions.unity` — 📝 Auditado

~75 objetos evaluados · 24 a tintar · ~45 NO · 4 duales · 16 ocultos (RewardClaimBlocker subtree)

#### TABLE 1 — Objetos a TINTAR (ThemeApplier = YES)

**Escena — objetos persistentes visibles:**
| Objeto (GameObject path) | Componente | ElementType | ThemeData Property | Color NeonDark | Notas |
|---|---|---|---|---|---|
| `Canvas/Background` | Image | `PrimaryBackground` | `primaryBackground` | #050A14 | Reset Image.color a blanco |
| `Canvas/TopBar/BackButton` | Image | `ButtonSecondary` | `buttonSecondary` | — | Prefab BackButton |
| `Canvas/TopBar/TitleText` | TextMeshProUGUI | `TextTitle` | `textTitle` | #00FFFF | "MISSIONS" — reset from CYAN_NEON |
| `Canvas/TimerBar` | Image | `CardBackground` | `cardBackground` | #0F1420 | Solo Image — Outline permanece orange semántico del timer |
| `Canvas/TimerBar/ResetsInLabel` | TextMeshProUGUI | `TextSecondary` | `textSecondary` | #9999A6 | "Resets in:" label |
| `Canvas/TabBar/DailyTab/Text` | TextMeshProUGUI | `TextPrimary` | `textPrimary` | #FFFFFF | Tab activo (blanco); runtime `DOColor` → actualizar Manager |
| `Canvas/TabBar/DailyTab/Indicator` | Image | `Accent` | `primaryAccent` | #00FFFF | Línea bottom del tab activo |
| `Canvas/TabBar/WeeklyTab/Text` | TextMeshProUGUI | `TextSecondary` | `textSecondary` | #9999A6 | Tab inactivo; runtime `DOColor` → actualizar Manager |
| `Canvas/TabBar/SpecialTab/Text` | TextMeshProUGUI | `TextSecondary` | `textSecondary` | #9999A6 | Tab inactivo; runtime `DOColor` → actualizar Manager |

**MissionCard.prefab — añadir ThemeApplier al prefab via `MissionCardPrefabBuilder`:**
| Objeto (path en prefab) | Componente | ElementType | ThemeData Property | Color NeonDark | Notas |
|---|---|---|---|---|---|
| `MissionCard/IconContainer/IconGlow` | Image | `Glow` | `glowColor` | #00FFFF (15%) | Halo de fondo del icono — añadir al prefab |
| `MissionCard/Content/DescriptionText` | TextMeshProUGUI | `TextSecondary` | `textSecondary` | #9999A6 | Descripción siempre gris — añadir al prefab |
| `MissionCard/Content/ProgressBar` | Image | `SliderTrack` | `sliderTrack` | #1A1E26 | Fondo del mini-slider de progreso |
| `MissionCard/ClaimButton` | Image | `ButtonSuccess` | `buttonSuccess` | #33E666 | Oculto por defecto; se muestra al completar |
| `MissionCard/ClaimButton/Text` | TextMeshProUGUI | `TextOnSuccess` | `textOnSuccess` | #000000 | Texto sobre botón Claim |

**Ocultos en RewardClaimBlocker (TABLE 4):**
| Objeto (GameObject path) | Componente | ElementType | ThemeData Property | Color NeonDark | Notas |
|---|---|---|---|---|---|
| `Canvas/RewardClaimBlocker` | Image | `Overlay` | `overlayColor` | rgba(0,0,0,0.85) | Blocker — SetActive(false); también `missionDetailPanel` |
| `RewardClaimBlocker/RewardPopup/MissionCompletedTitle` | TextMeshProUGUI | `Premium` | `premiumColor` | #FFD700 | "Mission Completed!" — GOLD; oculto vía parent |
| `RewardClaimBlocker/RewardPopup/MissionName` | TextMeshProUGUI | `TextSecondary` | `textSecondary` | #9999A6 | Nombre de la misión; oculto vía parent |
| `RewardClaimBlocker/RewardPopup/DetailDescription` | TextMeshProUGUI | `TextSecondary` | `textSecondary` | #9999A6 | Descripción de la misión; oculto vía parent |
| `RewardClaimBlocker/RewardPopup/DetailProgressBar/Background` | Image | `SliderTrack` | `sliderTrack` | #1A1E26 | Fondo slider de detalle; oculto vía parent |
| `RewardClaimBlocker/RewardPopup/DetailProgressBar/Fill Area/Fill` | Image | `SliderFill` | `sliderFill` | #00FFFF | Relleno slider — CYAN_NEON; oculto vía parent |
| `RewardClaimBlocker/RewardPopup/DetailProgressText` | TextMeshProUGUI | `Accent` | `primaryAccent` | #00FFFF | "3/5" — CYAN_NEON; oculto vía parent |
| `RewardClaimBlocker/RewardPopup/CollectButton` | Image | `ButtonSuccess` | `buttonSuccess` | #33E666 | Botón "Collect" verde; oculto vía parent |
| `RewardClaimBlocker/RewardPopup/CollectButton/CollectButtonText` | TextMeshProUGUI | `TextOnSuccess` | `textOnSuccess` | #000000 | Texto sobre Collect; oculto vía parent |

#### TABLE 2 — Objetos que NO se tintan
| Objeto (GameObject path) | Razón |
|---|---|
| `Canvas/TopBar` | No tiene componente Image (solo RectTransform container) |
| `Canvas/TopBar/CurrencyPills` (container + 8 hijos: CoinsPill, GemsPill, CoinsAddButton, GemsAddButton, CoinsIcon, GemsIcon, CoinsValueText, GemsValueText) | Currency pills — objetos estáticos, no se modifican con ThemeApplier (decisión de diseño) |
| `Canvas/TimerBar` Outline component | Color ORANGE_TIMER semántico de urgencia del countdown — no existe ElementType equivalente |
| `Canvas/TimerBar/TimerIcon` | Image tintada ORANGE_TIMER — color semántico del timer, no cambia con el tema |
| `Canvas/TimerBar/CountdownText` | Runtime-actualizado con valor countdown; ORANGE_TIMER = color semántico de urgencia del timer |
| `Canvas/TabBar` | No tiene componente Image (solo HLG container) |
| `Canvas/TabBar/WeeklyTab/Indicator` | Image (Color.clear) — invisible en escena; `UpdateTabButton()` no lo actualiza |
| `Canvas/TabBar/SpecialTab/Indicator` | Image (Color.clear) — invisible en escena; `UpdateTabButton()` no lo actualiza |
| `Canvas/ScrollView` | Image (Color.clear) — transparente, solo para input del ScrollRect |
| `Canvas/ScrollView/Viewport` | Image (Color.clear) — RectMask2D, sin color visual |
| `Canvas/ScrollView/Viewport/Content` | No tiene componente Image |
| `Content/DailyHeader` | UIBuilder placeholder — destruido en runtime por `ClearItems()` (no Image) |
| `Content/DailyHeader/LeftLine` | UIBuilder placeholder — destruido en runtime por `ClearItems()` |
| `Content/DailyHeader/TitleText` | UIBuilder placeholder — destruido en runtime por `ClearItems()` |
| `Content/DailyHeader/RightLine` | UIBuilder placeholder — destruido en runtime por `ClearItems()` |
| `Content/WeeklyHeader` | UIBuilder placeholder — destruido en runtime por `ClearItems()` |
| `Content/WeeklyHeader/LeftLine` | UIBuilder placeholder — destruido en runtime por `ClearItems()` |
| `Content/WeeklyHeader/TitleText` | UIBuilder placeholder — destruido en runtime por `ClearItems()` |
| `Content/WeeklyHeader/RightLine` | UIBuilder placeholder — destruido en runtime por `ClearItems()` |
| `Content/Mission1 … Mission6` (6 cards + ~10 hijos c/u) | UIBuilder placeholder cards de previsualización — destruidos en runtime por `ClearItems()` al instanciar prefabs MissionCard |
| `Content/Weekly1 … Weekly3` (3 cards + ~10 hijos c/u) | UIBuilder placeholder weekly cards — destruidos en runtime por `ClearItems()` |
| `MissionCard` (root Image) | Color gestionado en runtime por MissionCardUI: CARD_BG / CARD_BG_COMPLETED / CARD_BG_CLAIMED según estado |
| `MissionCard/CategoryBorder` | Color de categoría asignado en runtime: CYAN_NEON / PURPLE_WEEKLY / GOLD_SPECIAL según MissionCategory |
| `MissionCard/IconContainer` | No tiene componente Image |
| `MissionCard/IconContainer/MissionIcon` | Sprite de arte cargado desde Resources/Icons/Missions/ — colores propios |
| `MissionCard/Content/TitleText` | Color runtime-driven: white (en progreso) / gray (reclamado) |
| `MissionCard/Content/ProgressBar/ProgressFill` (= ProgressFill Image) | Color runtime-driven: verde (completado) / acento de categoría (en progreso) |
| `MissionCard/Content/ProgressText` | Color runtime-driven: verde (completado/reclamado) / gris (en progreso) |
| `MissionCard/RewardSection` | No tiene componente Image |
| `MissionCard/RewardSection/RewardIcon` | Currency icon — color runtime: COIN_COLOR o GEM_COLOR según rewardType |
| `MissionCard/RewardSection/RewardAmountText` | Currency text — color runtime: COIN_COLOR o GEM_COLOR |
| `MissionCard/DifficultyIndicator` | Color semántico de dificultad (verde) — no cambia con el tema |
| `MissionCard/CompletedOverlay` | Overlay de estado completado — color semántico fijo (verde oscuro) |
| `MissionCard/ClaimedCheckmark` | Checkmark semántico verde "✓" — color de estado reclamado |
| `RewardClaimBlocker/RewardPopup/CelebrationIcon` | Sprite de regalo art (icon_gift_open_basic.png) con colores propios; oculto vía parent |
| `RewardClaimBlocker/RewardPopup/DetailProgressBar` (root) | Image (Color.clear) — sin color visual; oculto vía parent |
| `RewardClaimBlocker/RewardPopup/RewardDisplay` | No tiene componente Image; oculto vía parent |
| `RewardClaimBlocker/RewardPopup/RewardDisplay/Icon` | Runtime currency icon: COIN_COLOR o GEM_COLOR; oculto vía parent |
| `RewardClaimBlocker/RewardPopup/RewardDisplay/Amount` | Runtime currency text (también `detailRewardText`); oculto vía parent |

#### TABLE 3 — Casos especiales (2 ThemeAppliers en mismo objeto)
| Objeto | ThemeApplier #1 | ThemeApplier #2 | Razón |
|---|---|---|---|
| `Canvas/TabBar/DailyTab` | `TabActive`, applyToImage=true | `Glow`, applyToOutline=true | Tab activo tiene background coloreado + borde glow |
| `Canvas/TabBar/WeeklyTab` | `TabInactive`, applyToImage=true | `Glow`, applyToOutline=true | Tab inactivo tiene background oscuro + borde glow |
| `Canvas/TabBar/SpecialTab` | `TabInactive`, applyToImage=true | `Glow`, applyToOutline=true | Tab inactivo tiene background oscuro + borde glow |
| `RewardClaimBlocker/RewardPopup` (TABLE 4) | `CardBackground`, applyToImage=true | `ButtonGlowPremium`, applyToOutline=true | Popup card con borde dorado premium; oculto vía parent |

#### TABLE 4 — Objetos ocultos (SetActive false al inicio de escena)
| Objeto (path) | ThemeApplier | ElementType | Notas |
|---|---|---|---|
| `Canvas/RewardClaimBlocker` | YES | `Overlay` | SetActive(false) al inicio — también `missionDetailPanel` del Manager |
| `RewardClaimBlocker/RewardPopup` | YES (DUAL) | `CardBackground` + `ButtonGlowPremium` | Oculto vía parent — ver TABLE 3 |
| `RewardClaimBlocker/RewardPopup/CelebrationIcon` | NO | — | Oculto vía parent — sprite de regalo con arte propio |
| `RewardClaimBlocker/RewardPopup/MissionCompletedTitle` | YES | `Premium` | Oculto vía parent |
| `RewardClaimBlocker/RewardPopup/MissionName` | YES | `TextSecondary` | Oculto vía parent |
| `RewardClaimBlocker/RewardPopup/DetailDescription` | YES | `TextSecondary` | Oculto vía parent |
| `RewardClaimBlocker/RewardPopup/DetailProgressBar` (root) | NO | — | Oculto vía parent — Image (Color.clear) |
| `RewardClaimBlocker/RewardPopup/DetailProgressBar/Background` | YES | `SliderTrack` | Oculto vía parent |
| `RewardClaimBlocker/RewardPopup/DetailProgressBar/Fill Area/Fill` | YES | `SliderFill` | Oculto vía parent |
| `RewardClaimBlocker/RewardPopup/DetailProgressText` | YES | `Accent` | Oculto vía parent |
| `RewardClaimBlocker/RewardPopup/RewardDisplay` | NO | — | Oculto vía parent — no Image |
| `RewardClaimBlocker/RewardPopup/RewardDisplay/Icon` | NO | — | Oculto vía parent — runtime currency color |
| `RewardClaimBlocker/RewardPopup/RewardDisplay/Amount` | NO | — | Oculto vía parent — runtime currency color |
| `RewardClaimBlocker/RewardPopup/CollectButton` | YES | `ButtonSuccess` | Oculto vía parent |
| `RewardClaimBlocker/RewardPopup/CollectButton/CollectButtonText` | YES | `TextOnSuccess` | Oculto vía parent |
| `Content/EmptyStateText` | NO | — | SetActive(false) — destruido en runtime por `ClearItems()`; Manager crea EmptyState dinámicamente |

#### Notas de escena
1. **Runtime overrides en tabs**: `UpdateTabButton()` anima `Image.color` y `Text.color` con DOTween al cambiar tab. ThemeApplier establece el estado inicial; para theming completo el Manager debe reemplazar `CYAN_NEON`/`PURPLE_WEEKLY`/`GOLD_SPECIAL` hardcoded por `ThemeManager.current?.tabActive` etc.
2. **Timer colors fijos (naranja)**: TimerBar `Outline`, `TimerIcon` y `CountdownText` usan `ORANGE_TIMER` como color semántico de urgencia. No existe `ElementType` equivalente — mantener fijos en todos los temas.
3. **MissionCard.prefab**: 5 ThemeApplier entries deben añadirse al prefab `Assets/_Project/Prefabs/Monetization/DailyMissions/MissionCard.prefab` via `MissionCardPrefabBuilder`. Root Image, CategoryBorder, TitleText, ProgressFill y objetos de estado son gestionados en runtime por `MissionCardUI` — NO ThemeApplier en esos.
4. **UIBuilder placeholder cards**: `Mission1–Mission6` y `Weekly1–Weekly3` (con ~10 hijos c/u) creados por UIBuilder en Content son destruidos por `ClearItems()` en `Start()`. No añadir ThemeApplier.
5. **RewardClaimBlocker doble función**: Funciona simultáneamente como `rewardPopup` Y como `missionDetailPanel` (ambos campos del Manager apuntan al mismo GameObject).
6. **EmptyState dinámico**: El Manager crea `EmptyState` (Icon/Title/Subtitle) en runtime si no hay misiones activas. ThemeApplier (`TextSecondary` en Title y Subtitle) debe añadirse via code en `ShowEmptyState()`.
7. **OverallProgress eliminado**: `CreateOverallProgress()` existe en el UIBuilder pero NO es llamado por `RebuildMissions()` — la sección NO existe en la escena actual.

---

### 28 · `Monetization/Achievements.unity` — 📝 Auditado

38 objetos evaluados · 38 a tintar · 34 NO · 8 duales · 25 ocultos

#### TABLE 1 — Objetos a TINTAR (ThemeApplier = YES)
| Objeto (GameObject path) | Componente | ElementType | ThemeData Property | Color NeonDark | Notas |
|---|---|---|---|---|---|
| `Canvas/SafeArea/Background` | Image | `PrimaryBackground` | `primaryBackground` | #050A14 | Reset Image.color a blanco |
| `Canvas/SafeArea/Header` | Image | `SecondaryBackground` | `secondaryBackground` | rgba(0,0,0,0.4) | Barra header translúcida |
| `Canvas/SafeArea/Header/BackButton` | Image | `ButtonSecondary` | `buttonSecondary` | — | Prefab BackButton — reset Image.color a blanco |
| `Canvas/SafeArea/Header/TitleText` | TextMeshProUGUI | `TextTitle` | `textTitle` | #00FFFF | "ACHIEVEMENTS" — CYAN_NEON; sin Outline en UIBuilder |
| `Canvas/SafeArea/CategoryDropdownRow/CategoryDropdown/CategoryDropdownLabel` | TextMeshProUGUI | `Accent` | `primaryAccent` | #00FFFF | Etiqueta activa del dropdown de categoría |
| `Canvas/SafeArea/CategoryDropdownRow/CategoryDropdown/Arrow` | TextMeshProUGUI | `Accent` | `primaryAccent` | #00FFFF | Flecha del dropdown (carácter TMP) |
| `Canvas/SafeArea/CategoryDropdownRow/CategoryDropdown/Template/Viewport/Content/Item/Item Background` | Image | `AccentSecondary` | `secondaryAccent` | rgba(0,229,255,0.15) | Highlight de ítem seleccionado — ver TABLE 4 (Template oculto) |
| `Canvas/SafeArea/CategoryDropdownRow/CategoryDropdown/Template/Viewport/Content/Item/Item Checkmark` | Image | `Accent` | `primaryAccent` | #00FFFF | Checkmark de selección — ver TABLE 4 |
| `Canvas/SafeArea/CategoryDropdownRow/CategoryDropdown/Template/Viewport/Content/Item/Item Label` | TextMeshProUGUI | `TextPrimary` | `textPrimary` | #FFFFFF | Texto del ítem de dropdown — ver TABLE 4 |
| `Canvas/SafeArea/ProgressSection/LabelRow/AchProgressLeft` | TextMeshProUGUI | `TextSecondary` | `textSecondary` | #C4CCFF | "X Completed" — etiqueta fija izquierda |
| `Canvas/SafeArea/ProgressSection/LabelRow/AchProgressRight` | TextMeshProUGUI | `Accent` | `primaryAccent` | #00FFFF | "X/52" contador — cyan derecho |
| `Canvas/SafeArea/ProgressSection/OverallProgressBar/Fill Area/Fill` | Image | `Premium` | `premiumColor` | #FFD700 | Relleno de progreso global — dorado |

#### TABLE 2 — Objetos que NO se tintan
| Objeto (GameObject path) | Razón |
|---|---|
| `Canvas/SafeArea/Background/AmbientParticles` | Overlay ambiental decorativo — color fijo (blue-tint 10%) |
| `Canvas/SafeArea/Header/BottomGlow` | Tira de glow gold decorativa en borde inferior del header — color fijo (#FFD700) |
| `Canvas/SafeArea/Header/CoinsPill` | Currency pill — objeto estático, decisión de diseño |
| `Canvas/SafeArea/Header/GemsPill` | Currency pill — objeto estático, decisión de diseño |
| `Canvas/SafeArea/Header/CoinsAddButton` | Currency pill — objeto estático, decisión de diseño |
| `Canvas/SafeArea/Header/GemsAddButton` | Currency pill — objeto estático, decisión de diseño |
| `Canvas/SafeArea/Header/CoinsIcon` | Currency pill — objeto estático, decisión de diseño |
| `Canvas/SafeArea/Header/GemsIcon` | Currency pill — objeto estático, decisión de diseño |
| `Canvas/SafeArea/Header/CoinsValueText` | Currency pill — objeto estático, decisión de diseño |
| `Canvas/SafeArea/Header/GemsValueText` | Currency pill — objeto estático, decisión de diseño |
| `Canvas/SafeArea/CategoryDropdownRow/CategoryDropdown/Template/Viewport` | Image transparente + RectMask2D — sin color visible |
| `Canvas/SafeArea/TrophyShowcaseScrollView` | Image transparente — contenedor ScrollRect |
| `Canvas/SafeArea/TrophyShowcaseScrollView/Viewport` | Image transparente + RectMask2D — sin color visible |
| `Canvas/SafeArea/TrophyShowcaseScrollView/Viewport/Content` | Sin componente Image — solo GridLayoutGroup (53 cards) |
| `TrophyCard(prefab)/CardContainer` | TrophyCardUI.UpdateVisualState() sobreescribe Image.color en runtime (locked/inProgress/completed) — conflicto con ThemeApplier |
| `TrophyCard(prefab)/CardContainer [Outline]` | Outline usa `glowColor` por categoría (GOLD, CAT_GAMES, CAT_COMPETITION…) — color semántico de categoría, no temático |
| `TrophyCard(prefab)/CardContainer/GlassOverlay` | Efecto glass decorativo — blanco fijo (alpha 5–10%) |
| `TrophyCard(prefab)/CardContainer/BorderGlow` | Glow de borde decorativo — color fijo por instancia |
| `TrophyCard(prefab)/CardContainer/TrophyIcon` | Arte de achievement multi-color — NUNCA tintable (regla de iconos) |
| `TrophyCard(prefab)/CardContainer/TrophyIcon/TrophyShadow` | Sombra negra fija (30% alpha) — decorativa |
| `TrophyCard(prefab)/CardContainer/TrophyIcon/LockedOverlay` | Overlay negro fijo 50% — sin color temático; oculto por defecto |
| `TrophyCard(prefab)/CardContainer/TrophyIcon/QuestionMark` | Color CAT_SECRET (morado fijo) — color de categoría secreta; oculto por defecto |
| `TrophyCard(prefab)/CardContainer/ProgressContainer/ProgressFill` | Color de categoría del achievement (accentColor) — asignado en runtime; no temático |
| `TrophyCard(prefab)/CardContainer/TitleText` | TrophyCardUI.UpdateVisualState() sobreescribe en runtime (blanco / muted / morado secreto) |
| `TrophyCard(prefab)/CardContainer/CompletedBadge/Checkmark` | Sprite de checkmark — arte de icono, no tintable |
| `TrophyCard(prefab)/CardContainer/ShineEffect` | Efecto shine animado — blanco 15% fijo |
| `Canvas/SafeArea/DetailPanelBlocker/DetailPanel/CloseButton/Text` | Sprite de icono X — imagen de arte del botón |
| `Canvas/SafeArea/DetailPanelBlocker/DetailPanel/DetailTrophyIcon` | AchievementsManager L629 sobreescribe: gris (locked) / blanco (unlocked) en runtime |
| `Canvas/SafeArea/DetailPanelBlocker/DetailPanel/DetailProgressSection/DetailProgressText` | AchievementsManager L666/671 hardcodea `new Color(0f,1f,0.5f)` (completado) / `new Color(1f,0.84f,0f)` (en progreso) — ThemeApplier sobrescrito |
| `Canvas/SafeArea/DetailPanelBlocker/DetailPanel/DetailRewardSection/RewardIcon` | Color gem currency azul fijo `new Color(0.4f,0.8f,1f)` — análogo a CurrencyPill |
| `Canvas/SafeArea/DetailPanelBlocker/DetailPanel/DetailRewardSection/RewardAmount` | Color gem currency azul fijo (#66CCFF) — análogo a CurrencyPill |
| `Canvas/SafeArea/RewardCelebration/CelebrationGlow` | AchievementsManager L816 anima a `new Color(1f,0.84f,0f,0.5f)` en runtime — no temático |
| `Canvas/SafeArea/RewardCelebration/CenterContent/CelebrationTrophyIcon` | Arte de achievement — NUNCA tintable |
| `Canvas/SafeArea/RewardCelebration/CenterContent/CelebrationRewardDisplay/Icon` | Color gem currency azul fijo — análogo a CurrencyPill |
| `Canvas/SafeArea/RewardCelebration/CenterContent/CelebrationRewardDisplay/Amount` | Color gem currency azul fijo — análogo a CurrencyPill |

#### TABLE 3 — Casos especiales (2 ThemeAppliers en mismo objeto)
| Objeto | ThemeApplier #1 | ThemeApplier #2 | Razón |
|---|---|---|---|
| `Canvas/SafeArea/CategoryDropdownRow/CategoryDropdown` | Image → `CardBackground` (applyToImage=true) | Outline → `Glow` (applyToOutline=true) | Panel dropdown: fondo card oscuro + borde glow cyan |
| `Canvas/SafeArea/ProgressSection/OverallProgressBar` | Image → `CardBackground` (applyToImage=true) | Outline → `AccentSecondary` (applyToOutline=true) | Barra de progreso global: fondo oscuro + borde cyan |

#### TABLE 4 — Objetos ocultos/inactivos en Awake
| Objeto (GameObject path) | Estado inicial | Cuándo se activa | ThemeApplier igual que activo |
|---|---|---|---|
| `Canvas/SafeArea/CategoryDropdownRow/CategoryDropdown/Template` | SetActive(false) (Unity dropdown convention) | Al abrir el dropdown | Image → `CardBackground` |
| `TrophyCard(prefab)/CardContainer/ProgressContainer` | SetActive(false) | Cuando achievement está en progreso | Sin Image — container VLG; sus hijos sí tienen ThemeApplier |
| `TrophyCard(prefab)/CardContainer/ProgressContainer/ProgressBackground` | Oculto vía ProgressContainer | Cuando achievement está en progreso | Image → `CardBackground` |
| `TrophyCard(prefab)/CardContainer/ProgressContainer/ProgressText` | Oculto vía ProgressContainer | Cuando achievement está en progreso | TextMeshProUGUI → `TextSecondary` |
| `TrophyCard(prefab)/CardContainer/CompletedBadge` | SetActive(false) | Al completar el achievement | Image → `ButtonSuccess` |
| `Canvas/SafeArea/DetailPanelBlocker` | SetActive(false) | Al pulsar cualquier trophy card | Image → `Overlay` |
| `Canvas/SafeArea/DetailPanelBlocker/DetailPanel` | Oculto vía parent Blocker | Al pulsar trophy card | Image → `CardBackground` + Outline → `Glow` (DUAL) |
| `Canvas/SafeArea/DetailPanelBlocker/DetailPanel/CloseButton` | Oculto vía parent | Siempre visible cuando panel abre | Image → `ButtonDanger` + Outline → `ButtonGlowDanger` (DUAL) |
| `Canvas/SafeArea/DetailPanelBlocker/DetailPanel/DetailTitle` | Oculto vía parent | Al abrir panel de detalle | TextMeshProUGUI → `Premium` |
| `Canvas/SafeArea/DetailPanelBlocker/DetailPanel/DetailDescription` | Oculto vía parent | Al abrir panel de detalle | TextMeshProUGUI → `TextSecondary` |
| `Canvas/SafeArea/DetailPanelBlocker/DetailPanel/DetailCategoryText` | Oculto vía parent | Al abrir panel de detalle | TextMeshProUGUI → `Accent` |
| `Canvas/SafeArea/DetailPanelBlocker/DetailPanel/DetailProgressSection/DetailProgressBar` | Oculto vía parent | Al abrir panel de detalle | Image → `CardBackground` + Outline → `AccentSecondary` (DUAL) |
| `Canvas/SafeArea/DetailPanelBlocker/DetailPanel/DetailProgressSection/DetailProgressBar/Fill Area/Fill` | Oculto vía parent | Al abrir panel de detalle | Image → `ButtonSuccess` |
| `Canvas/SafeArea/DetailPanelBlocker/DetailPanel/DetailRewardSection` | Oculto vía parent | Al abrir panel de detalle | Image → `CardBackground` + Outline → `ButtonGlowPremium` (DUAL) |
| `Canvas/SafeArea/DetailPanelBlocker/DetailPanel/DetailRewardSection/AchRewardLabel` | Oculto vía parent | Al abrir panel de detalle | TextMeshProUGUI → `TextSecondary` |
| `Canvas/SafeArea/DetailPanelBlocker/DetailPanel/DetailRewardSection/ClaimRewardButton` | Oculto vía parent | Al abrir panel si hay recompensa claimable | Image → `ButtonSuccess` |
| `Canvas/SafeArea/DetailPanelBlocker/DetailPanel/DetailRewardSection/ClaimRewardButton/ClaimButtonText` | Oculto vía parent | Al abrir panel si hay recompensa claimable | TextMeshProUGUI → `TextOnSuccess` |
| `Canvas/SafeArea/DetailPanelBlocker/DetailPanel/CancelButton` | Oculto vía parent | Al abrir panel de detalle | Image → `ButtonSecondary` + Outline → `AccentSecondary` (DUAL) |
| `Canvas/SafeArea/DetailPanelBlocker/DetailPanel/CancelButton/CancelButtonText` | Oculto vía parent | Al abrir panel de detalle | TextMeshProUGUI → `TextSecondary` |
| `Canvas/SafeArea/RewardCelebration` | SetActive(false) | Al reclamar una recompensa | Image → `Overlay` |
| `Canvas/SafeArea/RewardCelebration/CenterContent` | Oculto vía parent RewardCelebration | Al reclamar recompensa | Image → `CardBackground` + Outline → `ButtonGlowPremium` (DUAL) |
| `Canvas/SafeArea/RewardCelebration/CenterContent/CelebrationTitle` | Oculto vía parent | Al reclamar recompensa | TextMeshProUGUI → `Premium` |
| `Canvas/SafeArea/RewardCelebration/CenterContent/CelebrationAchievementName` | Oculto vía parent | Al reclamar recompensa | TextMeshProUGUI → `TextPrimary` |
| `Canvas/SafeArea/RewardCelebration/CenterContent/ContinueButton` | Oculto vía parent | Al reclamar recompensa | Image → `ButtonPrimary` |
| `Canvas/SafeArea/RewardCelebration/CenterContent/ContinueButton/ContinueButtonText` | Oculto vía parent | Al reclamar recompensa | TextMeshProUGUI → `TextOnPrimary` |

#### Notas de escena
- **TrophyCardUI — gestión de estado runtime**: `UpdateVisualState()` controla `CardContainer` (Image.color) según estado: locked → CARD_BG normal, unlocked → CARD_BG tintado, completed → card dorado. Esto sobreescribe ThemeApplier → CardContainer NO ThemeApplier. La Outline también es por categoría (GOLD, CAT_GAMES, etc.) → tampoco temática.
- **DetailProgressText runtime override**: `AchievementsManager.ShowAchievementDetail()` L666 hardcodea `new Color(0f,1f,0.5f)` (verde) al completar; L671 `new Color(1f,0.84f,0f)` (gold) en progreso. ThemeApplier sobrescribería estos colores semánticos de estado → NO ThemeApplier.
- **CelebrationGlow runtime controlled**: `ShowRewardCelebration()` L816 anima el color a `new Color(1f,0.84f,0f,0.5f)` (gold). No es color temático — es un efecto de celebración fijo → NO ThemeApplier.
- **TrophyCard prefab — 53 instancias**: El prefab `TrophyCard.prefab` se instancia 53 veces en runtime. Los ThemeApplier se añaden al prefab fuente; todas las instancias los heredan.
- **RewardIcon / Amount / gem currency**: Color gem azul fijo `new Color(0.4f, 0.8f, 1f)` — mismo tratamiento que CurrencyPills. Color semántico de moneda, no temático.
- **Iconos de achievement (TrophyIcon / CelebrationTrophyIcon)**: Arte multi-color de trofeos, medallas y achievements — NUNCA tintable. Los marcos y fondos del card SÍ reciben ThemeApplier.
- **DetailTrophyIcon**: AchievementsManager L629 sobreescribe el color: gris cuando locked, blanco cuando unlocked. Dado el override runtime → NO ThemeApplier.
- **BottomGlow en Header**: helper `CreateBottomGlow(header, GOLD)` añade una tira de 3px en el borde inferior del header con color gold fijo (#FFD700). Decoración fija, no temática.

---

### 29 · `Tournaments/TournamentsBrowser.unity` — 📝 V52

#### Objetos a TINTAR
| Objeto (GameObject path) | Componente | ElementType | ThemeData Property | Color NeonDark | Notas |
|---|---|---|---|---|---|
| Canvas/Background | Image | PrimaryBackground | primaryBackground | #080820 | Reset Image.color a white |
| SafeArea/Header | Image | SecondaryBackground | secondaryBackground | #12122A | Reset Image.color a white |
| SafeArea/Header/BottomGlow | Image | Accent | primaryAccent | #00E5FF | Reset Image.color a white |
| SafeArea/Header/BackButton | Image | ButtonSecondary | buttonSecondary | #1E1E3F | Prefab — TA interno al prefab |
| SafeArea/Header/TitleText | TextMeshProUGUI | TextTitle | textTitle | #00E5FF | DUAL #1 — ver TABLE 3 |
| SafeArea/Header/TitleText | Outline | Glow | glowColor | #00E5FF | DUAL #2 — ver TABLE 3 |
| SafeArea/TabsPanel | Image | SecondaryBackground | secondaryBackground | #12122A | Reset Image.color a white |
| TabsPanel/SearchTournamentsTab/Indicator | Image | TabActive | tabActive | #00E5FF | Color estático CYAN_NEON — no modificado por manager |
| TabsPanel/MyTournamentsTab/Indicator | Image | TabInactive | tabInactive | #808080 | Color estático clear en UIBuilder; TA actualiza a tabInactive |
| TabsPanel/FeaturedTab/Indicator | Image | TabInactive | tabInactive | #808080 | Color estático clear en UIBuilder; TA actualiza a tabInactive |
| SafeArea/SearchBar | Image | InputBackground | inputBackground | #060618 | DUAL #1 — ver TABLE 3 |
| SafeArea/SearchBar | Outline | InputBorder | inputBorder | #00E5FF80 | DUAL #2 — ver TABLE 3 |
| SearchBar/SearchIcon | Image | TextSecondary | textSecondary | #8888BB | ⚠️ BORDERLINE — SearchIcon.png asumido white glyph (Navigation folder); verificar en Inspector |
| SearchBar/SearchInput/Text Area/Placeholder | TextMeshProUGUI | InputPlaceholder | inputPlaceholder | #666699 | Reset TMP.color a white |
| SearchBar/SearchInput/Text Area/Text | TextMeshProUGUI | TextPrimary | textPrimary | #FFFFFF | Reset TMP.color a white |
| SearchBar/FilterButton | Image | ButtonSecondary | buttonSecondary | #1E1E3F | DUAL #1 — ver TABLE 3 |
| SearchBar/FilterButton | Outline | Glow | glowColor | #00E5FF | DUAL #2 — ver TABLE 3 |
| SearchBar/FilterButton/FiltersButtonText | TextMeshProUGUI | TextPrimary | textPrimary | #FFFFFF | Reset TMP.color a white |
| SafeArea/FilterPanel | Image | CardBackground | cardBackground | #0D0D24 | DUAL #1 — panel oculto por defecto |
| SafeArea/FilterPanel | Outline | InputBorder | inputBorder | #00E5FF80 | DUAL #2 — ver TABLE 3 |
| FilterPanel/GameTypeFilter | Image | InputBackground | inputBackground | #060618 | Dropdown root — sin Outline en UIBuilder |
| FilterPanel/GameTypeFilter/Label | TextMeshProUGUI | TextPrimary | textPrimary | #FFFFFF | Reset TMP.color a white |
| FilterPanel/EntryFeeFilter | Image | InputBackground | inputBackground | #060618 | Dropdown root — sin Outline |
| FilterPanel/EntryFeeFilter/Label | TextMeshProUGUI | TextPrimary | textPrimary | #FFFFFF | Reset TMP.color a white |
| FilterPanel/StatusFilter | Image | InputBackground | inputBackground | #060618 | Dropdown root — sin Outline |
| FilterPanel/StatusFilter/Label | TextMeshProUGUI | TextPrimary | textPrimary | #FFFFFF | Reset TMP.color a white |
| FilterPanel/ClearFiltersButton | Image | ButtonSecondary | buttonSecondary | #1E1E3F | Sin Outline en UIBuilder |
| FilterPanel/ClearFiltersButton/ClearFiltersText | TextMeshProUGUI | TextSecondary | textSecondary | #8888BB | Reset TMP.color a white |
| SafeArea/EmptyState/Icon | Image | Accent | primaryAccent | #00E5FF | DUAL #1 — oculto por defecto; sin sprite asignado en UIBuilder |
| SafeArea/EmptyState/Icon | Outline | Glow | glowColor | #00E5FF | DUAL #2 — ver TABLE 3 |
| SafeArea/EmptyState/EmptyStateText | TextMeshProUGUI | TextPrimary | textPrimary | #FFFFFF | Reset TMP.color a white |
| SafeArea/EmptyState/Subtitle | TextMeshProUGUI | TextSecondary | textSecondary | #8888BB | Reset TMP.color a white |
| SafeArea/EmptyState/CreateButton | Image | ButtonPrimary | buttonPrimary | #00E5FF | DUAL #1 — ver TABLE 3 |
| SafeArea/EmptyState/CreateButton | Outline | ButtonGlowPrimary | glowColor | #00E5FF | DUAL #2 — ver TABLE 3 |
| SafeArea/EmptyState/CreateButton/CreateTournamentText | TextMeshProUGUI | TextOnPrimary | textOnPrimary | #1A1A2E | Reset TMP.color a white |
| SafeArea/RefreshIndicator | Image | Overlay | overlayColor | #00000080 | Banner semi-transparente; oculto por defecto |
| SafeArea/RefreshIndicator/Text | TextMeshProUGUI | Accent | primaryAccent | #00E5FF | Reset TMP.color a white |
| SafeArea/LoadMoreButton | Image | ButtonSecondary | buttonSecondary | #1E1E3F | DUAL #1 — oculto por defecto |
| SafeArea/LoadMoreButton | Outline | Glow | glowColor | #00E5FF | DUAL #2 — ver TABLE 3 |
| SafeArea/LoadMoreButton/LoadMoreText | TextMeshProUGUI | TextPrimary | textPrimary | #FFFFFF | Reset TMP.color a white |
| SafeArea/LoadingIndicator/Spinner | Image | Accent | primaryAccent | #00E5FF | DUAL #1 — oculto por defecto |
| SafeArea/LoadingIndicator/Spinner | Outline | Glow | glowColor | #00E5FF | DUAL #2 — ver TABLE 3 |
| SafeArea/LoadingIndicator/Text | TextMeshProUGUI | TextSecondary | textSecondary | #8888BB | Reset TMP.color a white |
| SafeArea/CreateTournamentButton | Image | ButtonPrimary | buttonPrimary | #00E5FF | DUAL #1 — FAB full-width |
| SafeArea/CreateTournamentButton | Outline | ButtonGlowPrimary | glowColor | #00E5FF | DUAL #2 — ver TABLE 3 |
| SafeArea/CreateTournamentButton/CreateTournamentText | TextMeshProUGUI | TextOnPrimary | textOnPrimary | #1A1A2E | Reset TMP.color a white |

#### Objetos que NO se tintan
| Objeto (GameObject path) | Razón |
|---|---|
| SafeArea | Contenedor layout puro — sin Image |
| Header/CurrencyPills | CurrencyHeaderBarHelper root — siempre NO |
| Header/CurrencyPills/CoinsPill | CurrencyPills rule |
| Header/CurrencyPills/GemsPill | CurrencyPills rule |
| Header/CurrencyPills/CoinsAddButton | CurrencyPills rule |
| Header/CurrencyPills/GemsAddButton | CurrencyPills rule |
| Header/CurrencyPills/CoinsIcon | CurrencyPills rule |
| Header/CurrencyPills/GemsIcon | CurrencyPills rule |
| Header/CurrencyPills/CoinsValueText | CurrencyPills rule |
| Header/CurrencyPills/GemsValueText | CurrencyPills rule |
| TabsPanel/SearchTournamentsTab | Image — runtime override: `UpdateTabButton()` llama `image.DOColor(active/inactiveColor)` cada tab switch |
| TabsPanel/SearchTournamentsTab/Text | TMP — runtime override: `UpdateTabButton()` llama `text.DOColor(...)` cada tab switch |
| TabsPanel/MyTournamentsTab | Image — runtime override: `UpdateTabButton()` DOColor |
| TabsPanel/MyTournamentsTab/Text | TMP — runtime override: `UpdateTabButton()` DOColor |
| TabsPanel/FeaturedTab | Image — runtime override: `UpdateTabButton()` DOColor |
| TabsPanel/FeaturedTab/Text | TMP — runtime override: `UpdateTabButton()` DOColor |
| SearchBar/SearchInput | Image(clear/transparent) — sin contenido visual |
| SearchBar/SearchInput/Text Area | RectMask2D container — sin Image visual |
| TournamentsScrollView | Image(clear) — scroll container transparente |
| TournamentsScrollView/Viewport | Image(clear) + RectMask2D — máscara de scroll |
| TournamentsScrollView/Viewport/Content | VLG layout container — sin Image |
| FilterPanel/GameTypeFilter/Template | TMP_Dropdown template interno — 7 objetos, regla dropdown-template |
| FilterPanel/GameTypeFilter/Template/Viewport | Dropdown template interno |
| FilterPanel/GameTypeFilter/Template/Viewport/Content | Dropdown template interno |
| FilterPanel/GameTypeFilter/Template/Viewport/Content/Item | Dropdown template interno |
| FilterPanel/GameTypeFilter/Template/Viewport/Content/Item/Item Background | Dropdown template interno |
| FilterPanel/GameTypeFilter/Template/Viewport/Content/Item/Item Checkmark | Dropdown template interno |
| FilterPanel/GameTypeFilter/Template/Viewport/Content/Item/Item Label | Dropdown template interno |
| FilterPanel/EntryFeeFilter/Template | TMP_Dropdown template interno |
| FilterPanel/EntryFeeFilter/Template/Viewport | Dropdown template interno |
| FilterPanel/EntryFeeFilter/Template/Viewport/Content | Dropdown template interno |
| FilterPanel/EntryFeeFilter/Template/Viewport/Content/Item | Dropdown template interno |
| FilterPanel/EntryFeeFilter/Template/Viewport/Content/Item/Item Background | Dropdown template interno |
| FilterPanel/EntryFeeFilter/Template/Viewport/Content/Item/Item Checkmark | Dropdown template interno |
| FilterPanel/EntryFeeFilter/Template/Viewport/Content/Item/Item Label | Dropdown template interno |
| FilterPanel/StatusFilter/Template | TMP_Dropdown template interno |
| FilterPanel/StatusFilter/Template/Viewport | Dropdown template interno |
| FilterPanel/StatusFilter/Template/Viewport/Content | Dropdown template interno |
| FilterPanel/StatusFilter/Template/Viewport/Content/Item | Dropdown template interno |
| FilterPanel/StatusFilter/Template/Viewport/Content/Item/Item Background | Dropdown template interno |
| FilterPanel/StatusFilter/Template/Viewport/Content/Item/Item Checkmark | Dropdown template interno |
| FilterPanel/StatusFilter/Template/Viewport/Content/Item/Item Label | Dropdown template interno |
| EmptyState | VLG layout container — sin Image (no se crea Image en CreateEmptyState para el root) |
| LoadingIndicator | Root container sin Image — solo aloja Spinner y Text |
| Runtime tournament items | Instanciados en runtime desde `tournamentItemPrefab` (o fallback) — ThemeApplier va en el prefab, no aquí |

#### Casos especiales (2 ThemeAppliers en mismo objeto)
| Objeto | ThemeApplier #1 | ThemeApplier #2 | Razón |
|---|---|---|---|
| Header/TitleText | TextTitle · applyToText=true | Glow · applyToOutline=true | TMP cyan + Outline glow |
| SearchBar | InputBackground · applyToImage=true | InputBorder · applyToOutline=true | Barra búsqueda: fondo input + borde cyan |
| SearchBar/FilterButton | ButtonSecondary · applyToImage=true | Glow · applyToOutline=true | Botón filtros oscuro + borde glow |
| FilterPanel | CardBackground · applyToImage=true | InputBorder · applyToOutline=true | Panel filtros: card fondo + borde cyan |
| EmptyState/Icon | Accent · applyToImage=true | Glow · applyToOutline=true | Icono vacío cyan + glow outline |
| EmptyState/CreateButton | ButtonPrimary · applyToImage=true | ButtonGlowPrimary · applyToOutline=true | Botón crear cyan + glow |
| LoadMoreButton | ButtonSecondary · applyToImage=true | Glow · applyToOutline=true | Botón cargar más + borde glow |
| LoadingIndicator/Spinner | Accent · applyToImage=true | Glow · applyToOutline=true | Spinner cyan + glow |
| CreateTournamentButton | ButtonPrimary · applyToImage=true | ButtonGlowPrimary · applyToOutline=true | FAB crear torneo cyan + glow |

#### Notas de escena
- **Tab Image + Text → NO**: `UpdateTabVisuals()` → `UpdateTabButton()` usa `image.DOColor()` y `text.DOColor()` en cada tab switch — ambos runtime override.
- **Tab/Indicator → YES**: `UpdateTabButton()` NO toca el Indicator child (solo `button.GetComponent<Image>()` y `button.GetComponentInChildren<TextMeshProUGUI>()`). Indicadores son estáticos post-UIBuilder. Se usa TabActive para active, TabInactive para los inactivos.
- **SearchIcon ⚠️**: Cargado desde `Art/Icons/Navigation/SearchIcon.png` — asumir white glyph tintable. Verificar en Inspector antes de aplicar.
- **3 dropdowns × 7 template-internals = 21 NO objects** — misma regla que TournamentCreate.
- **FilterPanel oculto por defecto**: `filterPanel.SetActive(false)`. ThemeApplier debe aplicarse igualmente (se activa al pulsar FilterButton).
- **EmptyState/Icon sin sprite**: UIBuilder no asigna sprite explícitamente — puede ser un cuadro de color sin arte. ThemeApplier aplica de todas formas.
- **Runtime tournament items**: La lista se popula desde `tournamentItemPrefab` → ese prefab tiene su propio ThemeApplier. El Content container es solo layout.
- **9 dual cases** — ver TABLE 3.
- **CreateTournamentButton** aparece tanto en EmptyState (oculto) como como FAB principal (siempre visible). Son dos GOs distintos con el mismo GO name "CreateTournamentButton".

---

### 30 · `Tournaments/TournamentCreate.unity` — 📝 V52

#### Objetos a TINTAR (102)
| Objeto (GameObject path) | Componente | ElementType | ThemeData Property | Color NeonDark | Notas |
|---|---|---|---|---|---|
| `Canvas/Background` | Image | `PrimaryBackground` | primaryBackground | #050A14 | Reset color a white |
| `SafeArea/Header` | Image | `SecondaryBackground` | secondaryBackground | #0F1724 | Reset color a white |
| `SafeArea/Header/BottomGlow` | Image | `Accent` | primaryAccent | #00FFFF | Glow line 3px |
| `SafeArea/Header/TitleText` | TMP | `TextTitle` | textTitle | #00FFFF | Dual — ver TABLE 3 |
| `Content/NameSection` | Image | `CardBackground` | cardBackground | #141E2E | Dual — ver TABLE 3 |
| `NameSection/SectionHeader/TournamentNameLabel` | TMP | `TextSecondary` | textSecondary | #99B3BF | Reset color a white |
| `NameSection/SectionHeader/Line` | Image | `Accent` | primaryAccent | #00FFFF@0.2 | Separador dim |
| `NameSection/TournamentNameInput` | Image | `InputBackground` | inputBackground | #0A1219 | Dual — ver TABLE 3 |
| `TournamentNameInput/Text Area/Placeholder` | TMP | `InputPlaceholder` | inputPlaceholder | #808080 | Reset color a white |
| `TournamentNameInput/Text Area/Text` | TMP | `TextPrimary` | textPrimary | #F2F2F2 | Reset color a white |
| `NameSection/NameCharCountText` | TMP | `TextSecondary` | textSecondary | #99B3BF | Reset color a white |
| `Content/GameSelectionSection` | Image | `CardBackground` | cardBackground | #141E2E | Dual — ver TABLE 3 |
| `GameSelectionSection/SectionHeader/GameTypeLabel` | TMP | `TextSecondary` | textSecondary | #99B3BF | Reset color a white |
| `GameSelectionSection/SectionHeader/Line` | Image | `Accent` | primaryAccent | #00FFFF@0.2 | Separador dim |
| `GameSelectionSection/GameRow/GameTypeDropdown` | Image | `InputBackground` | inputBackground | #0A1219 | Dual — ver TABLE 3 |
| `GameTypeDropdown/Label` | TMP | `TextPrimary` | textPrimary | #F2F2F2 | Caption del dropdown |
| `GameTypeDropdown/Arrow` | TMP | `TextSecondary` | textSecondary | #99B3BF | Símbolo ▼ |
| `GameSelectionSection/GameRow/SelectedGameIcon` | Image+Outline | `InputBackground` | inputBackground | #0A1219 | Single TA: applyToImage=true, applyToOutline=true (mismo color) |
| `Content/EntryFeeSection` | Image | `CardBackground` | cardBackground | #141E2E | Dual — ver TABLE 3 |
| `EntryFeeSection/SectionHeader/EntryFeeLabel` | TMP | `TextSecondary` | textSecondary | #99B3BF | Reset color a white |
| `EntryFeeSection/SectionHeader/Line` | Image | `Accent` | primaryAccent | #00FFFF@0.2 | Separador dim |
| `EntryFeeSection/EntryFeeDropdown` | Image | `InputBackground` | inputBackground | #0A1219 | Dual — ver TABLE 3 |
| `EntryFeeDropdown/Label` | TMP | `TextPrimary` | textPrimary | #F2F2F2 | Caption del dropdown |
| `EntryFeeDropdown/Arrow` | TMP | `TextSecondary` | textSecondary | #99B3BF | Símbolo ▼ |
| `EntryFeeSection/SliderRow/SliderLabel` | TMP | `TextSecondary` | textSecondary | #99B3BF | "Adjust:" label |
| `EntryFeeSection/CustomEntryFeeInput` | Image | `InputBackground` | inputBackground | #0A1219 | Dual — ver TABLE 3 · hidden por defecto |
| `CustomEntryFeeInput/Text Area/Placeholder` | TMP | `InputPlaceholder` | inputPlaceholder | #808080 | Reset color a white |
| `CustomEntryFeeInput/Text Area/Text` | TMP | `TextPrimary` | textPrimary | #F2F2F2 | Reset color a white |
| `EntryFeeSection/EntryFeeDisplayText` | TMP | `Accent` | primaryAccent | #00FFFF | "FREE" / precio seleccionado |
| `Content/MaxPlayersSection` | Image | `CardBackground` | cardBackground | #141E2E | Dual — ver TABLE 3 |
| `MaxPlayersSection/SectionHeader/PlayersAndPrizeLabel` | TMP | `TextSecondary` | textSecondary | #99B3BF | Reset color a white |
| `MaxPlayersSection/SectionHeader/Line` | Image | `Accent` | primaryAccent | #00FFFF@0.2 | Separador dim |
| `MaxPlayersSection/MaxPlayersDropdown` | Image | `InputBackground` | inputBackground | #0A1219 | Dual — ver TABLE 3 |
| `MaxPlayersDropdown/Label` | TMP | `TextPrimary` | textPrimary | #F2F2F2 | Caption del dropdown |
| `MaxPlayersDropdown/Arrow` | TMP | `TextSecondary` | textSecondary | #99B3BF | Símbolo ▼ |
| `MaxPlayersSection/EstimatedPrizeText` | TMP | `Success` | successColor | #4DFF4D | GREEN_ACCENT → Success semántico |
| `Content/ScheduleSection` | Image | `CardBackground` | cardBackground | #141E2E | Dual — ver TABLE 3 |
| `ScheduleSection/SectionHeader/StartScheduleLabel` | TMP | `TextSecondary` | textSecondary | #99B3BF | Reset color a white |
| `ScheduleSection/SectionHeader/Line` | Image | `Accent` | primaryAccent | #00FFFF@0.2 | Separador dim |
| `ScheduleSection/StartImmediatelyToggle` | Image | `InputBackground` | inputBackground | #0A1219 | Dual — ver TABLE 3 |
| `StartImmediatelyToggle/Background` | Image | `ToggleBackground` | toggleOff | #4D4D59 | Dual — ver TABLE 3 |
| `StartImmediatelyToggle/Background/Checkmark` | Image | `ToggleCheckmark` | toggleCheckmark | #000000 | Visible solo cuando toggle=ON |
| `StartImmediatelyToggle/Label` | TMP | `TextPrimary` | textPrimary | #F2F2F2 | Reset color a white |
| `ScheduleSection/StartTimeDropdown` | Image | `InputBackground` | inputBackground | #0A1219 | Dual — ver TABLE 3 · hidden cuando Start Immediately = ON |
| `StartTimeDropdown/Label` | TMP | `TextPrimary` | textPrimary | #F2F2F2 | Caption del dropdown |
| `StartTimeDropdown/Arrow` | TMP | `TextSecondary` | textSecondary | #99B3BF | Símbolo ▼ |
| `ScheduleSection/ScheduledTimeText` | TMP | `TextSecondary` | textSecondary | #99B3BF | "Starts immediately when full" |
| `Content/RulesSection` | Image | `CardBackground` | cardBackground | #141E2E | Dual — ver TABLE 3 |
| `RulesSection/SectionHeader/TournamentRulesLabel` | TMP | `TextSecondary` | textSecondary | #99B3BF | Reset color a white |
| `RulesSection/SectionHeader/Line` | Image | `Accent` | primaryAccent | #00FFFF@0.2 | Separador dim |
| `RulesSection/RoundsDropdown` | Image | `InputBackground` | inputBackground | #0A1219 | Dual — ver TABLE 3 |
| `RoundsDropdown/Label` | TMP | `TextPrimary` | textPrimary | #F2F2F2 | Caption del dropdown |
| `RoundsDropdown/Arrow` | TMP | `TextSecondary` | textSecondary | #99B3BF | Símbolo ▼ |
| `RulesSection/TimeLimitDropdown` | Image | `InputBackground` | inputBackground | #0A1219 | Dual — ver TABLE 3 |
| `TimeLimitDropdown/Label` | TMP | `TextPrimary` | textPrimary | #F2F2F2 | Caption del dropdown |
| `TimeLimitDropdown/Arrow` | TMP | `TextSecondary` | textSecondary | #99B3BF | Símbolo ▼ |
| `RulesSection/MaxAttemptsDropdown` | Image | `InputBackground` | inputBackground | #0A1219 | Dual — ver TABLE 3 |
| `MaxAttemptsDropdown/Label` | TMP | `TextPrimary` | textPrimary | #F2F2F2 | Caption del dropdown |
| `MaxAttemptsDropdown/Arrow` | TMP | `TextSecondary` | textSecondary | #99B3BF | Símbolo ▼ |
| `RulesSection/AllowSpectatorsToggle` | Image | `InputBackground` | inputBackground | #0A1219 | Dual — ver TABLE 3 |
| `AllowSpectatorsToggle/Background` | Image | `ToggleBackground` | toggleOff | #4D4D59 | Dual — ver TABLE 3 |
| `AllowSpectatorsToggle/Background/Checkmark` | Image | `ToggleCheckmark` | toggleCheckmark | #000000 | Visible solo cuando toggle=ON |
| `AllowSpectatorsToggle/Label` | TMP | `TextPrimary` | textPrimary | #F2F2F2 | Reset color a white |
| `Content/PrivacySection` | Image | `CardBackground` | cardBackground | #141E2E | Dual — ver TABLE 3 |
| `PrivacySection/SectionHeader/PrivacyLabel` | TMP | `TextSecondary` | textSecondary | #99B3BF | Reset color a white |
| `PrivacySection/SectionHeader/Line` | Image | `Accent` | primaryAccent | #00FFFF@0.2 | Separador dim |
| `PrivacySection/PrivateToggle` | Image | `InputBackground` | inputBackground | #0A1219 | Dual — ver TABLE 3 |
| `PrivateToggle/Background` | Image | `ToggleBackground` | toggleOff | #4D4D59 | Dual — ver TABLE 3 |
| `PrivateToggle/Background/Checkmark` | Image | `ToggleCheckmark` | toggleCheckmark | #000000 | Visible solo cuando toggle=ON |
| `PrivateToggle/Label` | TMP | `TextPrimary` | textPrimary | #F2F2F2 | Reset color a white |
| `PrivacySection/PrivateCodeInput` | Image | `InputBackground` | inputBackground | #0A1219 | Dual — ver TABLE 3 · hidden, visible cuando Private=ON |
| `PrivateCodeInput/Text Area/Placeholder` | TMP | `InputPlaceholder` | inputPlaceholder | #808080 | Reset color a white |
| `PrivateCodeInput/Text Area/Text` | TMP | `TextPrimary` | textPrimary | #F2F2F2 | Reset color a white |
| `Content/PreviewPanel` | Image | `CardBackground` | cardBackground | #141E2E | Dual — ver TABLE 3 · hidden por defecto, mostrado por TogglePreview() |
| `PreviewPanel/SectionHeader/PreviewLabel` | TMP | `TextSecondary` | textSecondary | #99B3BF | Reset color a white |
| `PreviewPanel/SectionHeader/Line` | Image | `Accent` | primaryAccent | #00FFFF@0.2 | Separador dim |
| `PreviewPanel/PreviewNameText_Row` | Image | `TertiaryBackground` | tertiaryBackground | #262633 | Fila par (índice 0) |
| `PreviewPanel/PreviewNameText_Row/PreviewNameText` | TMP | `TextPrimary` | textPrimary | #F2F2F2 | Reset color a white |
| `PreviewPanel/PreviewGameText_Row/PreviewGameText` | TMP | `TextPrimary` | textPrimary | #F2F2F2 | Fila impar — row sin Image |
| `PreviewPanel/PreviewEntryText_Row` | Image | `TertiaryBackground` | tertiaryBackground | #262633 | Fila par (índice 2) |
| `PreviewPanel/PreviewEntryText_Row/PreviewEntryText` | TMP | `TextPrimary` | textPrimary | #F2F2F2 | Reset color a white |
| `PreviewPanel/PreviewPrizeText_Row/PreviewPrizeText` | TMP | `TextPrimary` | textPrimary | #F2F2F2 | Fila impar — row sin Image |
| `PreviewPanel/PreviewPlayersText_Row` | Image | `TertiaryBackground` | tertiaryBackground | #262633 | Fila par (índice 4) |
| `PreviewPanel/PreviewPlayersText_Row/PreviewPlayersText` | TMP | `TextPrimary` | textPrimary | #F2F2F2 | Reset color a white |
| `ActionSection/PreviewButton` | Image | `ButtonSecondary` | buttonSecondary | #1F2939 | Dual — ver TABLE 3 |
| `ActionSection/PreviewButton/Text` | TMP | `TextPrimary` | textPrimary | #F2F2F2 | Reset color a white |
| `ActionSection/CreationFeeText` | TMP | `TextSecondary` | textSecondary | #99B3BF | "Creation fee: $5.00" |
| `ActionSection/CreateTournamentButton` | Image | `ButtonPrimary` | buttonPrimary | #00FFFF | Dual — ver TABLE 3 |
| `ActionSection/CreateTournamentButton/Text` | TMP | `TextOnPrimary` | textOnPrimary | #050A14 | Reset color a white |
| `ActionSection/CreateButtonGlow` | Image | `Glow` | glowColor | #00FFFF@0.06 | Panel decorativo de glow |
| `ActionSection/CreateButtonText` | TMP | `TextSecondary` | textSecondary | #99B3BF | "Name must be at least 3 characters" |
| `Canvas/LoadingOverlay` | Image | `Overlay` | overlayColor | #000000@0.75 | SetActive(false) por defecto |
| `LoadingOverlay/Center/Spinner` | Image | `Accent` | primaryAccent | #00FFFF | Dual — ver TABLE 3 |
| `Canvas/ConfirmBlocker` | Image | `Overlay` | overlayColor | #000000@0.85 | SetActive(false) por defecto |
| `ConfirmBlocker/ConfirmPopup` | Image | `CardBackground` | cardBackground | #141E2E | Dual — ver TABLE 3 |
| `ConfirmPopup/TopGlow` | Image | `Accent` | primaryAccent | #00FFFF | Glow line superior |
| `ConfirmPopup/Title` | TMP | `TextTitle` | textTitle | #00FFFF | "Confirm Creation" |
| `ConfirmPopup/Message` | TMP | `TextPrimary` | textPrimary | #F2F2F2 | Reset color a white |
| `ConfirmPopup/Buttons/CancelButton` | Image | `ButtonSecondary` | buttonSecondary | #1F2939 | Dual — ver TABLE 3 |
| `ConfirmPopup/Buttons/CancelButton/Text` | TMP | `TextPrimary` | textPrimary | #F2F2F2 | Reset color a white |
| `ConfirmPopup/Buttons/ConfirmButton` | Image | `ButtonPrimary` | buttonPrimary | #00FFFF | Dual — ver TABLE 3 |
| `ConfirmPopup/Buttons/ConfirmButton/Text` | TMP | `TextOnPrimary` | textOnPrimary | #050A14 | Reset color a white |

#### Objetos que NO se tintan (81)
| Objeto (GameObject path) | Razón |
|---|---|
| `Canvas/SafeArea` | Layout container sin Image |
| `SafeArea/FormScrollView` | Image.color = clear (transparente) |
| `FormScrollView/Viewport` | Image.color = clear + RectMask2D |
| `FormScrollView/Viewport/Content` | Sin Image — layout VLG |
| `NameSection/SectionHeader` | Sin Image — layout HLG |
| `GameSelectionSection/SectionHeader` | Sin Image — layout HLG |
| `EntryFeeSection/SectionHeader` | Sin Image — layout HLG |
| `MaxPlayersSection/SectionHeader` | Sin Image — layout HLG |
| `ScheduleSection/SectionHeader` | Sin Image — layout HLG |
| `RulesSection/SectionHeader` | Sin Image — layout HLG |
| `PrivacySection/SectionHeader` | Sin Image — layout HLG |
| `PreviewPanel/SectionHeader` | Sin Image — layout HLG |
| `TournamentNameInput/Text Area` | Sin Image visible — RectMask2D interno |
| `CustomEntryFeeInput/Text Area` | Sin Image visible — RectMask2D interno |
| `PrivateCodeInput/Text Area` | Sin Image visible — RectMask2D interno |
| `GameSelectionSection/GameRow` | Sin Image — layout HLG |
| `EntryFeeSection/SliderRow` | Sin Image — layout HLG |
| `EntryFeeSection/EntryFeeSlider` | Slider component — sin Image explícita en root |
| `ActionSection` | Sin Image — layout VLG |
| `LoadingOverlay/Center` | Sin Image — layout VLG |
| `ConfirmPopup/Buttons` | Sin Image — layout HLG |
| `PreviewPanel/PreviewGameText_Row` | Sin Image (fila impar, i=1) — layout only |
| `PreviewPanel/PreviewPrizeText_Row` | Sin Image (fila impar, i=3) — layout only |
| `Canvas/LoadingOverlay/Center/StatusText` | Runtime override: `ShowStatus()` establece color error rojo o success verde → NO ThemeApplier |
| `GameTypeDropdown/Template` | TMP_Dropdown internal — SetActive(false), clonado en runtime |
| `GameTypeDropdown/Template/Viewport` | TMP_Dropdown internal |
| `GameTypeDropdown/Template/Viewport/Content` | TMP_Dropdown internal |
| `GameTypeDropdown/Template/.../Item` | TMP_Dropdown internal — Toggle |
| `GameTypeDropdown/Template/.../Item Background` | TMP_Dropdown internal |
| `GameTypeDropdown/Template/.../Item Checkmark` | TMP_Dropdown internal |
| `GameTypeDropdown/Template/.../Item Label` | TMP_Dropdown internal |
| `EntryFeeDropdown/Template` | TMP_Dropdown internal — SetActive(false) |
| `EntryFeeDropdown/Template/Viewport` | TMP_Dropdown internal |
| `EntryFeeDropdown/Template/Viewport/Content` | TMP_Dropdown internal |
| `EntryFeeDropdown/Template/.../Item` | TMP_Dropdown internal |
| `EntryFeeDropdown/Template/.../Item Background` | TMP_Dropdown internal |
| `EntryFeeDropdown/Template/.../Item Checkmark` | TMP_Dropdown internal |
| `EntryFeeDropdown/Template/.../Item Label` | TMP_Dropdown internal |
| `MaxPlayersDropdown/Template` | TMP_Dropdown internal — SetActive(false) |
| `MaxPlayersDropdown/Template/Viewport` | TMP_Dropdown internal |
| `MaxPlayersDropdown/Template/Viewport/Content` | TMP_Dropdown internal |
| `MaxPlayersDropdown/Template/.../Item` | TMP_Dropdown internal |
| `MaxPlayersDropdown/Template/.../Item Background` | TMP_Dropdown internal |
| `MaxPlayersDropdown/Template/.../Item Checkmark` | TMP_Dropdown internal |
| `MaxPlayersDropdown/Template/.../Item Label` | TMP_Dropdown internal |
| `StartTimeDropdown/Template` | TMP_Dropdown internal — SetActive(false), hidden widget |
| `StartTimeDropdown/Template/Viewport` | TMP_Dropdown internal |
| `StartTimeDropdown/Template/Viewport/Content` | TMP_Dropdown internal |
| `StartTimeDropdown/Template/.../Item` | TMP_Dropdown internal |
| `StartTimeDropdown/Template/.../Item Background` | TMP_Dropdown internal |
| `StartTimeDropdown/Template/.../Item Checkmark` | TMP_Dropdown internal |
| `StartTimeDropdown/Template/.../Item Label` | TMP_Dropdown internal |
| `RoundsDropdown/Template` | TMP_Dropdown internal — SetActive(false) |
| `RoundsDropdown/Template/Viewport` | TMP_Dropdown internal |
| `RoundsDropdown/Template/Viewport/Content` | TMP_Dropdown internal |
| `RoundsDropdown/Template/.../Item` | TMP_Dropdown internal |
| `RoundsDropdown/Template/.../Item Background` | TMP_Dropdown internal |
| `RoundsDropdown/Template/.../Item Checkmark` | TMP_Dropdown internal |
| `RoundsDropdown/Template/.../Item Label` | TMP_Dropdown internal |
| `TimeLimitDropdown/Template` | TMP_Dropdown internal — SetActive(false) |
| `TimeLimitDropdown/Template/Viewport` | TMP_Dropdown internal |
| `TimeLimitDropdown/Template/Viewport/Content` | TMP_Dropdown internal |
| `TimeLimitDropdown/Template/.../Item` | TMP_Dropdown internal |
| `TimeLimitDropdown/Template/.../Item Background` | TMP_Dropdown internal |
| `TimeLimitDropdown/Template/.../Item Checkmark` | TMP_Dropdown internal |
| `TimeLimitDropdown/Template/.../Item Label` | TMP_Dropdown internal |
| `MaxAttemptsDropdown/Template` | TMP_Dropdown internal — SetActive(false) |
| `MaxAttemptsDropdown/Template/Viewport` | TMP_Dropdown internal |
| `MaxAttemptsDropdown/Template/Viewport/Content` | TMP_Dropdown internal |
| `MaxAttemptsDropdown/Template/.../Item` | TMP_Dropdown internal |
| `MaxAttemptsDropdown/Template/.../Item Background` | TMP_Dropdown internal |
| `MaxAttemptsDropdown/Template/.../Item Checkmark` | TMP_Dropdown internal |
| `MaxAttemptsDropdown/Template/.../Item Label` | TMP_Dropdown internal |
| `Header/CurrencyPills` (y los 8 hijos) | Regla fija CurrencyHeaderBarHelper — siempre estático |

#### Casos especiales (31 GOs con 2 ThemeAppliers)
| Objeto | ThemeApplier #1 | ThemeApplier #2 | Razón |
|---|---|---|---|
| `Header/TitleText` | `TextTitle` applyToText | `Accent` applyToOutline | TMP cyan + Outline glow cyan |
| `NameSection` | `CardBackground` applyToImage | `Accent` applyToOutline | Card bg + cyan border |
| `NameSection/TournamentNameInput` | `InputBackground` applyToImage | `InputBorder` applyToOutline | Input field |
| `GameSelectionSection` | `CardBackground` applyToImage | `Accent` applyToOutline | Card bg + cyan border |
| `GameSelectionSection/GameRow/GameTypeDropdown` | `InputBackground` applyToImage | `InputBorder` applyToOutline | Dropdown styled as input |
| `EntryFeeSection` | `CardBackground` applyToImage | `Accent` applyToOutline | Card bg + cyan border |
| `EntryFeeSection/EntryFeeDropdown` | `InputBackground` applyToImage | `InputBorder` applyToOutline | Dropdown styled as input |
| `EntryFeeSection/CustomEntryFeeInput` | `InputBackground` applyToImage | `InputBorder` applyToOutline | Hidden input — mismo patrón |
| `MaxPlayersSection` | `CardBackground` applyToImage | `Accent` applyToOutline | Card bg + cyan border |
| `MaxPlayersSection/MaxPlayersDropdown` | `InputBackground` applyToImage | `InputBorder` applyToOutline | Dropdown styled as input |
| `ScheduleSection` | `CardBackground` applyToImage | `Accent` applyToOutline | Card bg + cyan border |
| `ScheduleSection/StartImmediatelyToggle` | `InputBackground` applyToImage | `InputBorder` applyToOutline | Toggle row styled as input |
| `StartImmediatelyToggle/Background` | `ToggleBackground` applyToImage | `InputBorder` applyToOutline | Checkmark box bg + border |
| `ScheduleSection/StartTimeDropdown` | `InputBackground` applyToImage | `InputBorder` applyToOutline | Hidden dropdown |
| `RulesSection` | `CardBackground` applyToImage | `Accent` applyToOutline | Card bg + cyan border |
| `RulesSection/RoundsDropdown` | `InputBackground` applyToImage | `InputBorder` applyToOutline | Dropdown styled as input |
| `RulesSection/TimeLimitDropdown` | `InputBackground` applyToImage | `InputBorder` applyToOutline | Dropdown styled as input |
| `RulesSection/MaxAttemptsDropdown` | `InputBackground` applyToImage | `InputBorder` applyToOutline | Dropdown styled as input |
| `RulesSection/AllowSpectatorsToggle` | `InputBackground` applyToImage | `InputBorder` applyToOutline | Toggle row styled as input |
| `AllowSpectatorsToggle/Background` | `ToggleBackground` applyToImage | `InputBorder` applyToOutline | Checkmark box bg + border |
| `PrivacySection` | `CardBackground` applyToImage | `Accent` applyToOutline | Card bg + cyan border |
| `PrivacySection/PrivateToggle` | `InputBackground` applyToImage | `InputBorder` applyToOutline | Toggle row styled as input |
| `PrivateToggle/Background` | `ToggleBackground` applyToImage | `InputBorder` applyToOutline | Checkmark box bg + border |
| `PrivacySection/PrivateCodeInput` | `InputBackground` applyToImage | `InputBorder` applyToOutline | Hidden input — mismo patrón |
| `PreviewPanel` | `CardBackground` applyToImage | `Accent` applyToOutline | Card bg + accent border (CYAN_ACCENT) |
| `ActionSection/PreviewButton` | `ButtonSecondary` applyToImage | `Accent` applyToOutline | Botón secundario + cyan border |
| `ActionSection/CreateTournamentButton` | `ButtonPrimary` applyToImage | `Glow` applyToOutline | CTA principal + glow outline |
| `LoadingOverlay/Center/Spinner` | `Accent` applyToImage | `Glow` applyToOutline | Spinner cyan + glow outline |
| `ConfirmBlocker/ConfirmPopup` | `CardBackground` applyToImage | `Accent` applyToOutline | Popup card bg + cyan border |
| `ConfirmPopup/Buttons/CancelButton` | `ButtonSecondary` applyToImage | `Accent` applyToOutline | Botón cancel + cyan border |
| `ConfirmPopup/Buttons/ConfirmButton` | `ButtonPrimary` applyToImage | `Glow` applyToOutline | Botón confirm CTA + glow |

#### Notas de escena
- **StatusText = NO**: `TournamentCreateManager.ShowStatus(message, isError)` establece `.color` a error rojo (`#FF4D4D`) o success verde (`#4DFF4D`) hardcodeados. Sin ThemeApplier.
- **SelectedGameIcon single TA**: Image y Outline usan el mismo CYAN_DARK; un único ThemeApplier con `applyToImage=true, applyToOutline=true` es suficiente (ElementType = `InputBackground`).
- **Dropdown templates = 49 NO objects**: 7 dropdowns × 7 sub-objetos internos. No tocar — TMP_Dropdown gestiona su propio render.
- **3 toggles auditados**: StartImmediatelyToggle, AllowSpectatorsToggle, PrivateToggle — todos necesitan TA en la row + en el Background child.
- **3 inputs hidden**: CustomEntryFeeInput, StartTimeDropdown, PrivateCodeInput — `SetActive(false)` por defecto pero ThemeApplier sí debe añadirse (se activan condicionalmente).
- **PreviewPanel hidden**: `card.SetActive(false)` en el builder; `TogglePreview()` lo activa/oculta. ThemeApplier debe añadirse.
- **EstimatedPrizeText = Success**: GREEN_ACCENT (`#00FF80`) → `successColor`. Valor semántico de premio ganado.
- **ConfirmBlocker + LoadingOverlay = Overlay**: Ambos usan fondos oscuros semi-transparentes de bloqueo → `overlayColor`.

---

### 31 · `Tournaments/TournamentLobby.unity` — 📝 V52

#### Objetos a TINTAR
| Objeto (GameObject path) | Componente | ElementType | ThemeData Property | Color NeonDark | Notas |
|---|---|---|---|---|---|
| Background | Image | PrimaryBackground | primaryBackground | #080820 | reset color a white |
| Header | Image | SecondaryBackground | secondaryBackground | #12122A | reset color a white |
| Header/BottomGlow | Image | Accent | primaryAccent | #00E5FF | reset color a white |
| Header/BackButton | Image | ButtonSecondary | buttonSecondary | #1E1E3F | prefab — TA interno al prefab |
| Header/TournamentNameText | TextMeshProUGUI | TextTitle | textTitle | #00E5FF | DUAL #1 — ver TABLE 3 |
| Header/TournamentNameText | Outline | Glow | glowColor | #00E5FF | DUAL #2 — ver TABLE 3 |
| InfoCard | Image | CardBackground | cardBackground | #0D0D24 | DUAL #1 — ver TABLE 3 |
| InfoCard | Outline | InputBorder | inputBorder | #00E5FF80 | DUAL #2 — ver TABLE 3 |
| InfoCard/StatusRow/StatusBadgeText | TextMeshProUGUI | TextOnSuccess | textOnSuccess | #1A1A2E | color estático TEXT_DARK; badge Image es NO (runtime override) |
| InfoCard/StatusRow/PrizesButton | Image | AccentTertiary | tertiaryAccent | #FFD700 | DUAL #1 — ver TABLE 3 |
| InfoCard/StatusRow/PrizesButton | Outline | ButtonGlowPremium | premiumColor | #FFD700 | DUAL #2 — ver TABLE 3 |
| InfoCard/StatusRow/PrizesButton/Text | TextMeshProUGUI | TextOnPrimary | textOnPrimary | #1A1A2E | texto sobre botón gold |
| InfoCard/TopRow/GameTypeRow/GameIcon | Image | Accent | primaryAccent | #00E5FF | ⚠️ BORDERLINE — si sprite es white glyph YES; si arte a color NO. UIBuilder tinta con CYAN_DARK → asumir tintable |
| InfoCard/TopRow/GameTypeRow/GameTypeText | TextMeshProUGUI | TextPrimary | textPrimary | #FFFFFF | |
| InfoCard/TopRow/TimeRow/ClockIcon | Image | TextSecondary | textSecondary | #8888BB | ⚠️ BORDERLINE — asumir white glyph tintable; reset color a white |
| InfoCard/MiddleRow/EntryFeeGroup/Icon | Image | AccentTertiary | tertiaryAccent | #FFD700 | icono gold; asumir white glyph tintable |
| InfoCard/MiddleRow/EntryFeeGroup/EntryFeeText | TextMeshProUGUI | Success | successColor | #00FF88 | BUTTON_SUCCESS → successColor |
| InfoCard/MiddleRow/PrizePoolGroup/Icon | Image | AccentTertiary | tertiaryAccent | #FFD700 | icono gold; asumir white glyph tintable |
| InfoCard/MiddleRow/PrizePoolGroup/PrizePoolText | TextMeshProUGUI | AccentTertiary | tertiaryAccent | #FFD700 | texto gold |
| InfoCard/ProgressRow/PlayersProgressBarBg | Image | SliderTrack | sliderTrack | #0D0D24 | fondo barra de progreso |
| InfoCard/ProgressRow/PlayersProgressBar | Image | SliderFill | sliderFill | #00E5FF | relleno barra de progreso |
| InfoCard/ProgressRow/PlayersProgressText | TextMeshProUGUI | TextPrimary | textPrimary | #FFFFFF | |
| InfoCard/RulesRow/AttemptsRuleText | TextMeshProUGUI | TextSecondary | textSecondary | #8888BB | |
| InfoCard/RulesRow/TimeLimitRuleText | TextMeshProUGUI | TextSecondary | textSecondary | #8888BB | |
| TabBar | Image | SecondaryBackground | secondaryBackground | #12122A | fondo barra de tabs |
| TabBar/ParticipantsTab/ParticipantsTabText | TextMeshProUGUI | TextPrimary | textPrimary | #FFFFFF | tab activo; color no cambia via SerializeField |
| TabBar/ChatTab/ChatTabText | TextMeshProUGUI | TextSecondary | textSecondary | #8888BB | tab inactivo |
| TabBar/ChatTab/ChatBadge | Image | Error | errorColor | #FF3366 | badge rojo notificaciones; color estático BADGE_RED |
| TabBar/ChatTab/ChatBadge/ChatBadgeText | TextMeshProUGUI | TextPrimary | textPrimary | #FFFFFF | texto sobre badge rojo |
| ParticipantsContent/LeaderboardHeader | Image | SecondaryBackground | secondaryBackground | #12122A | cabecera leaderboard |
| ParticipantsContent/LeaderboardHeader/RankCol | TextMeshProUGUI | TextSecondary | textSecondary | #8888BB | etiqueta columna |
| ParticipantsContent/LeaderboardHeader/PlayerCol | TextMeshProUGUI | TextSecondary | textSecondary | #8888BB | etiqueta columna |
| ParticipantsContent/LeaderboardHeader/TimeCol/TimerIcon | Image | TextSecondary | textSecondary | #8888BB | ⚠️ BORDERLINE — asumir white glyph; reset color a white |
| ParticipantsContent/LeaderboardHeader/TimeCol/TimeLabel | TextMeshProUGUI | TextSecondary | textSecondary | #8888BB | |
| ParticipantsContent/MyPositionPanel | Image | CardBackground | cardBackground | #0D0D24 | DUAL #1 — ver TABLE 3 |
| ParticipantsContent/MyPositionPanel | Outline | Accent | primaryAccent | #00E5FF | DUAL #2 — ver TABLE 3 |
| ParticipantsContent/MyPositionPanel/MyRank | TextMeshProUGUI | Accent | primaryAccent | #00E5FF | |
| ParticipantsContent/MyPositionPanel/MyName | TextMeshProUGUI | TextPrimary | textPrimary | #FFFFFF | |
| ParticipantsContent/MyPositionPanel/MyTime | TextMeshProUGUI | TextPrimary | textPrimary | #FFFFFF | |
| ChatContent/ChatInputRow | Image | SecondaryBackground | secondaryBackground | #12122A | fondo fila input chat |
| ChatContent/ChatInputRow/ChatInput | Image | InputBackground | inputBackground | #060618 | DUAL #1 — ver TABLE 3 |
| ChatContent/ChatInputRow/ChatInput | Outline | InputBorder | inputBorder | #00E5FF80 | DUAL #2 — ver TABLE 3 |
| ChatContent/ChatInputRow/ChatInput/Placeholder | TextMeshProUGUI | InputPlaceholder | inputPlaceholder | #666699 | |
| ChatContent/ChatInputRow/ChatInput/Text | TextMeshProUGUI | TextPrimary | textPrimary | #FFFFFF | texto escrito por usuario |
| ChatContent/ChatInputRow/SendChatButton | Image | ButtonPrimary | buttonPrimary | #00E5FF | |
| ChatContent/ChatInputRow/SendChatButton/Text | TextMeshProUGUI | TextOnPrimary | textOnPrimary | #1A1A2E | |
| ActionBar | Image | SecondaryBackground | secondaryBackground | #12122A | fondo barra acciones |
| ActionBar/JoinButton | Image | ButtonPrimary | buttonPrimary | #00E5FF | DUAL #1 — ver TABLE 3 |
| ActionBar/JoinButton | Outline | ButtonGlowPrimary | glowColor | #00E5FF | DUAL #2 — ver TABLE 3 |
| ActionBar/JoinButton/JoinButtonText | TextMeshProUGUI | TextOnPrimary | textOnPrimary | #1A1A2E | |
| ActionBar/ShareButton | Image | ButtonSecondary | buttonSecondary | #1E1E3F | DUAL #1 — ver TABLE 3 |
| ActionBar/ShareButton | Outline | Glow | glowColor | #00E5FF | DUAL #2 — ver TABLE 3 |
| ActionBar/ShareButton/ShareButtonText | TextMeshProUGUI | TextPrimary | textPrimary | #FFFFFF | |
| ActionBar/LeaveButton | Image | ButtonDanger | buttonDanger | #FF3366 | DUAL #1 — ver TABLE 3 |
| ActionBar/LeaveButton | Outline | ButtonGlowDanger | errorColor | #FF3366 | DUAL #2 — ver TABLE 3 |
| ActionBar/LeaveButton/LeaveButtonText | TextMeshProUGUI | TextOnDanger | textOnDanger | #FFFFFF | |
| SafeArea/StatusText | TextMeshProUGUI | TextSecondary | textSecondary | #8888BB | ShowStatus() solo cambia .text, no .color → YES |
| PrizesBlocker | Image | Overlay | overlayColor | #00000099 | bloqueador semi-transparente |
| PrizesPopup | Image | TertiaryBackground | tertiaryBackground | #1A1A35 | DUAL #1 — ver TABLE 3 |
| PrizesPopup | Outline | AccentTertiary | tertiaryAccent | #FFD700 | DUAL #2 — ver TABLE 3 |
| PrizesPopup/PrizesPopupTitle | TextMeshProUGUI | AccentTertiary | tertiaryAccent | #FFD700 | título popup gold |
| PrizesPopup/Prize_1stPlace/FirstPlaceLabel | TextMeshProUGUI | Rank1 | rank1Color | #FFD700 | gold 1er lugar |
| PrizesPopup/Prize_1stPlace/Amount | TextMeshProUGUI | Rank1 | rank1Color | #FFD700 | |
| PrizesPopup/Prize_2ndPlace/SecondPlaceLabel | TextMeshProUGUI | Rank2 | rank2Color | #C0C0C0 | silver 2do lugar |
| PrizesPopup/Prize_2ndPlace/Amount | TextMeshProUGUI | Rank2 | rank2Color | #C0C0C0 | |
| PrizesPopup/Prize_3rdPlace/ThirdPlaceLabel | TextMeshProUGUI | Rank3 | rank3Color | #CD7F32 | bronze 3er lugar |
| PrizesPopup/Prize_3rdPlace/Amount | TextMeshProUGUI | Rank3 | rank3Color | #CD7F32 | |
| PrizesPopup/CloseButton | Image | ButtonSecondary | buttonSecondary | #1E1E3F | DUAL #1 — ver TABLE 3 |
| PrizesPopup/CloseButton | Outline | Glow | glowColor | #00E5FF | DUAL #2 — ver TABLE 3 |
| PrizesPopup/CloseButton/PrizesCloseText | TextMeshProUGUI | TextPrimary | textPrimary | #FFFFFF | |
| LeaveBlocker | Image | Overlay | overlayColor | #00000099 | bloqueador semi-transparente |
| LeavePopup | Image | TertiaryBackground | tertiaryBackground | #1A1A35 | DUAL #1 — ver TABLE 3 |
| LeavePopup | Outline | ButtonGlowDanger | errorColor | #FF3366 | DUAL #2 — ver TABLE 3 |
| LeavePopup/LeavePopupTitle | TextMeshProUGUI | Error | errorColor | #FF3366 | texto danger rojo |
| LeavePopup/LeavePopupMessage | TextMeshProUGUI | TextPrimary | textPrimary | #FFFFFF | |
| LeavePopup/StayButton | Image | ButtonPrimary | buttonPrimary | #00E5FF | DUAL #1 — ver TABLE 3 |
| LeavePopup/StayButton | Outline | ButtonGlowPrimary | glowColor | #00E5FF | DUAL #2 — ver TABLE 3 |
| LeavePopup/StayButton/StayButtonText | TextMeshProUGUI | TextOnPrimary | textOnPrimary | #1A1A2E | |
| LeavePopup/ConfirmLeaveButton | Image | ButtonDanger | buttonDanger | #FF3366 | sin Outline en UIBuilder |
| LeavePopup/ConfirmLeaveButton/LeaveButtonText | TextMeshProUGUI | TextOnDanger | textOnDanger | #FFFFFF | |
| LoadingOverlay | Image | Overlay | overlayColor | #000000BF | negro @75% alpha |
| LoadingOverlay/Spinner | Image | Accent | primaryAccent | #00E5FF | DUAL #1 — ver TABLE 3 |
| LoadingOverlay/Spinner | Outline | Glow | glowColor | #00E5FF | DUAL #2 — ver TABLE 3 |
| LoadingOverlay/LoadingText | TextMeshProUGUI | TextPrimary | textPrimary | #FFFFFF | |
| StartingOverlay | Image | Overlay | overlayColor | #000000D9 | negro @85% alpha |
| StartingOverlay/StartingCountdownText | TextMeshProUGUI | Accent | primaryAccent | #00E5FF | DUAL #1 — ver TABLE 3 |
| StartingOverlay/StartingCountdownText | Outline | Glow | glowColor | #00E5FF | DUAL #2 — ver TABLE 3 |
| StartingOverlay/StartingSubtitle | TextMeshProUGUI | TextPrimary | textPrimary | #FFFFFF | |

#### Objetos que NO se tintan
| Objeto (GameObject path) | Razón |
|---|---|
| SafeArea | Contenedor layout puro — sin Image |
| Header/CurrencyPills | CurrencyHeaderBarHelper root — siempre NO |
| Header/CurrencyPills/CoinsPill | CurrencyPills rule |
| Header/CurrencyPills/GemsPill | CurrencyPills rule |
| Header/CurrencyPills/CoinsAddButton | CurrencyPills rule |
| Header/CurrencyPills/GemsAddButton | CurrencyPills rule |
| Header/CurrencyPills/CoinsIcon | CurrencyPills rule |
| Header/CurrencyPills/GemsIcon | CurrencyPills rule |
| Header/CurrencyPills/CoinsValueText | CurrencyPills rule |
| Header/CurrencyPills/GemsValueText | CurrencyPills rule |
| InfoCard/StatusRow | Contenedor layout puro |
| InfoCard/StatusRow/Spacer | Espaciador layout — sin Image |
| InfoCard/StatusRow/StatusBadge | Image — runtime override: `UpdateUI()` → `statusBadgeImage.color = GetStatusColor(status)` → green/gold/grey/red según estado |
| InfoCard/TopRow | Contenedor layout puro |
| InfoCard/TopRow/GameTypeRow | Contenedor layout puro |
| InfoCard/TopRow/TimeRow | Contenedor layout puro |
| InfoCard/TopRow/TimeRow/CountdownText | TMP — runtime override: `UpdateTimeDisplay()` cambia color a gold/green/white según tiempo restante |
| InfoCard/MiddleRow | Contenedor layout puro |
| InfoCard/MiddleRow/EntryFeeGroup | Contenedor layout puro |
| InfoCard/MiddleRow/PrizePoolGroup | Contenedor layout puro |
| InfoCard/ProgressRow | Contenedor layout puro |
| InfoCard/RulesRow | Contenedor layout puro |
| TabBar/ParticipantsTab | Image(clear/transparent) — sin contenido visual |
| TabBar/ParticipantsTab/ParticipantsTabIndicator | Image — runtime override: `SwitchToTab()` hardcodea `.color` con activeColor/inactiveColor |
| TabBar/ChatTab | Image(clear/transparent) — sin contenido visual |
| TabBar/ChatTab/ChatTabIndicator | Image — runtime override: `SwitchToTab()` hardcodea `.color` con activeColor/inactiveColor |
| ContentArea | Contenedor layout puro |
| ParticipantsContent | Contenedor layout puro |
| ParticipantsContent/LeaderboardHeader/TimeCol | Contenedor layout puro |
| ParticipantsContent/ParticipantsScrollView | Image(clear/transparent) — sin contenido visual |
| ParticipantsContent/ParticipantsScrollView/Viewport | Máscara de scroll — sin contenido visual |
| ParticipantsContent/ParticipantsScrollView/Content | Contenedor layout puro |
| ParticipantsContent/MyPositionPanel/MyAvatar | Image — avatar del jugador (foto de perfil, no tintable) |
| ChatContent | Contenedor layout puro |
| ChatContent/ChatScrollView | Image(clear/transparent) — sin contenido visual |
| ChatContent/ChatScrollView/Viewport | Máscara de scroll — sin contenido visual |
| ChatContent/ChatScrollView/ChatMessagesContainer | Contenedor runtime — filas creadas por `CreateChatMessage()` con colores hardcoded CHAT_COLOR_ME/CHAT_COLOR_OTHER |
| PrizesPopup/Prize_1stPlace | Contenedor layout puro |
| PrizesPopup/Prize_2ndPlace | Contenedor layout puro |
| PrizesPopup/Prize_3rdPlace | Contenedor layout puro |
| PrizesPopup/prizeDistributionContainer | Contenedor runtime — filas de `prizeRowPrefab` instanciadas por `UpdatePrizeDistribution()` |

#### Casos especiales (2 ThemeAppliers en mismo objeto)
| Objeto | ThemeApplier #1 | ThemeApplier #2 | Razón |
|---|---|---|---|
| Header/TournamentNameText | TextTitle · applyToText=true | Glow · applyToOutline=true | TMP cyan + Outline glow |
| InfoCard | CardBackground · applyToImage=true | InputBorder · applyToOutline=true | Card fill + borde cyan oscuro |
| InfoCard/StatusRow/PrizesButton | AccentTertiary · applyToImage=true | ButtonGlowPremium · applyToOutline=true | Botón gold fill + glow gold |
| ParticipantsContent/MyPositionPanel | CardBackground · applyToImage=true | Accent · applyToOutline=true | Panel "mi posición" fill + borde cyan |
| ChatContent/ChatInputRow/ChatInput | InputBackground · applyToImage=true | InputBorder · applyToOutline=true | Input field fondo + borde |
| ActionBar/JoinButton | ButtonPrimary · applyToImage=true | ButtonGlowPrimary · applyToOutline=true | Botón join cyan + glow |
| ActionBar/ShareButton | ButtonSecondary · applyToImage=true | Glow · applyToOutline=true | Botón share oscuro + borde glow |
| ActionBar/LeaveButton | ButtonDanger · applyToImage=true | ButtonGlowDanger · applyToOutline=true | Botón leave rojo + glow rojo |
| PrizesPopup | TertiaryBackground · applyToImage=true | AccentTertiary · applyToOutline=true | Modal fondo + borde gold |
| PrizesPopup/CloseButton | ButtonSecondary · applyToImage=true | Glow · applyToOutline=true | Botón cerrar popup |
| LeavePopup | TertiaryBackground · applyToImage=true | ButtonGlowDanger · applyToOutline=true | Modal fondo + borde rojo danger |
| LeavePopup/StayButton | ButtonPrimary · applyToImage=true | ButtonGlowPrimary · applyToOutline=true | Botón "quedarse" cyan |
| LoadingOverlay/Spinner | Accent · applyToImage=true | Glow · applyToOutline=true | Spinner cyan + glow |
| StartingOverlay/StartingCountdownText | Accent · applyToText=true | Glow · applyToOutline=true | Countdown cyan + glow outline |

#### Notas de escena
- **StatusBadge Image → NO**: `UpdateUI()` llama `statusBadgeImage.color = GetStatusColor(status)` → devuelve green/gold/grey/red por estado del torneo. StatusBadgeText SÍ (color estático TEXT_DARK).
- **CountdownText → NO**: `UpdateTimeDisplay()` cambia `countdownText.color` a gold (≤0s) / green (<60min) / white según tiempo restante.
- **Tab indicators → NO**: `SwitchToTab()` hardcodea `.color` de participantsTabIndicator y chatTabIndicator cada vez que se cambia de tab.
- **ChatMessages → NO**: `CreateChatMessage()` crea GOs en runtime con CHAT_COLOR_ME (cyan) o CHAT_COLOR_OTHER (grey) — objetos creados en runtime.
- **StatusText (ActionBar) → YES**: `ShowStatus()` solo cambia `.text`, nunca `.color` → color estático TEXT_SECONDARY → YES TextSecondary.
- **GameIcon ⚠️**: UIBuilder tinta con CYAN_DARK → sprite debe ser white glyph para TA funcionar. Verificar en Inspector antes de aplicar.
- **ClockIcon / TimerIcon ⚠️**: UIBuilder tinta con TEXT_SECONDARY → sprites deben ser white glyphs. Verificar en Inspector.
- **EntryFeeGroup/Icon y PrizePoolGroup/Icon ⚠️**: UIBuilder tinta con GOLD → sprites deben ser white glyphs (típicamente iconos de moneda).
- **15 dual cases** — ver TABLE 3 completa.
- **prizeDistributionContainer**: container para filas runtime creadas por `UpdatePrizeDistribution()` via `prizeRowPrefab`. Las filas del prefab tienen su propio TA.
- **participantsContainer/Content**: filas runtime via `participantItemPrefab`. Las filas del prefab tienen su propio TA.

---

### 32 · `CashBattle/CashBattleHub.unity` — 🚫 Excluida

> **Zona CashBattle** — paleta gold estática, sin ThemeApplier.

---

### 33 · `CashBattle/CashBattle1v1.unity` — 🚫 Excluida

> **Zona CashBattle** — paleta gold estática, sin ThemeApplier.

#### Objetos a TINTAR — ELIMINADO
| *(zona excluida)* | | | | | |

---

### 34 · `CashBattle/CashHistory.unity` — 🚫 Excluida

> **Zona CashBattle** — paleta gold estática, sin ThemeApplier.

---

### 35 · `CashBattle/CashProfile.unity` — 🚫 Excluida

> **Zona CashBattle** — paleta gold estática, sin ThemeApplier.

---

### 36 · `CashBattle/CashWallet.unity` — 🚫 Excluida

> **Zona CashBattle** — paleta gold estática, sin ThemeApplier.

---

### 37 · `CashBattle/CashMatchmaking.unity` — 🚫 Excluida

> **Zona CashBattle** — paleta gold estática, sin ThemeApplier.

---

### 38 · `CashBattle/CashTournaments/CashTournaments.unity` — 🚫 Excluida

> **Zona CashBattle** — paleta gold estática, sin ThemeApplier.

---

### 39 · `CashBattle/CashTournaments/CashTournamentCreate.unity` — 🚫 Excluida

> **Zona CashBattle** — paleta gold estática, sin ThemeApplier.

---

### 40 · `CashBattle/CashTournaments/CashTournamentLobby.unity` — 🚫 Excluida

> **Zona CashBattle** — paleta gold estática, sin ThemeApplier.

---

## 📈 PROGRESO GLOBAL

| Grupo | Total | ✅ Implementado | 📝 Documentado | ⬜ Pendiente |
|-------|-------|----------------|----------------|-------------|
| Core | 3 | 0 | 0 | 3 |
| Auth | 3 | 0 | 0 | 3 |
| Onboarding | 2 | 0 | 0 | 2 |
| Games Nav | 4 | 0 | 0 | 4 |
| Minigames | 5 | 0 | 0 | 5 |
| Social | 7 | 0 | 0 | 7 |
| Monetization | 4 | 0 | 0 | 4 |
| Tournaments | 3 | 0 | 0 | 3 |
| CashBattle | 9 | 0 | 0 | 9 |
| **TOTAL** | **40** | **0** | **0** | **40** |
