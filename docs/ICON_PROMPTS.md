# DigitPark - ICON PROMPTS (DALL-E)

> Todos los prompts para generar iconos minimalist de la app.
> Fecha: 2026-03-07

---

## REGLAS DALL-E

### Formato obligatorio para TODOS los iconos
- **Formato**: PNG
- **Resolucion**: 1024x1024
- **Fondo**: TRANSPARENTE — absolutamente transparente
- **DALL-E tiende a generar fondos blancos/grises o cuadros checkered que Unity NO renderiza como transparente.** Cada prompt DEBE especificar esto.

### Prefijo universal (copiar al inicio de CADA prompt)
```
PNG format, 1024x1024 pixels, perfectly square. The background MUST be 100% transparent — NOT white, NOT gray, NOT checkered, NOT any solid color, NOT any gradient. There must be ZERO background of any kind. The icon floats on pure empty transparent space. This will be used as a UI icon in a mobile app where any non-transparent pixel will be visible. Style: flat minimalist UI glyph, single color with NO gradients, NO 3D effects, NO shadows, NO glow, NO glossy highlights. Clean geometric shapes, consistent 2px stroke weight, like Apple SF Symbols or Google Material Icons. The icon must be perfectly centered, filling ~70% of the canvas with even margins.
```

---

## SECCION 1: ICONOS A BORRAR (Huerfanos — sin referencias en codigo)

Estos iconos NO son referenciados por ningun .cs. Borrar el .png + .meta:

| # | Icono | Path | Razon |
|---|-------|------|-------|
| DEL-1 | stat_avgearnings | `Art/Icons/CashBattle/Stats/` | CashHistoryUIBuilder no lo usa |
| DEL-2 | stat_totalearned | `Art/Icons/CashBattle/Stats/` | CashHistoryUIBuilder no lo usa |
| DEL-3 | stat_totalspent | `Art/Icons/CashBattle/Stats/` | CashHistoryUIBuilder no lo usa |
| DEL-4 | CreateTournamentIcon | `Art/Icons/CashBattle/Tournaments/` | CashBattlePrefabBuilder usa cards text-only sin iconos |
| DEL-5 | PlayersCountIcon | `Art/Icons/CashBattle/Tournaments/` | Sin referencia |
| DEL-6 | TournamentBracketIcon | `Art/Icons/CashBattle/Tournaments/` | Sin referencia |
| DEL-7 | TournamentLiveIcon | `Art/Icons/CashBattle/Tournaments/` | Sin referencia |
| DEL-8 | TournamentTimerIcon | `Art/Icons/CashBattle/Tournaments/` | Sin referencia |
| DEL-9 | TxDepositIcon | `Art/Icons/CashBattle/Wallet/` | WalletPrefabGenerator es text-only |
| DEL-10 | TxLossIcon | `Art/Icons/CashBattle/Wallet/` | Sin referencia |
| DEL-11 | TxWinIcon | `Art/Icons/CashBattle/Wallet/` | Sin referencia |
| DEL-12 | VerifiedBadgeIcon | `Art/Icons/CashBattle/Wallet/` | Sin referencia |
| DEL-13 | WalletIcon | `Art/Icons/CashBattle/Wallet/` | Sin referencia (WalletCashIcon es el que se usa) |
| DEL-14 | MissionLockedIcon | `Resources/Icons/` | Sin referencia — missions usan ms_* icons ahora |

**Total: 14 iconos a borrar.**

---

## SECCION 2: ICONOS QUE NO NECESITAN CAMBIO

Estos ya son minimalistas, ya funcionan, o son iconos de contenido (no UI):

### UI/Navigation — Ya minimalistas (confirmado en V36)
- SettingsIcon, NotificationsIcon, SearchIcon, RankingsIcon, MissionsIcon — outline blanco
- PlayIcon (#00E5FF), AchievementsIcon (#FFD700), ShopIcon (#FF6B35), PremiumIcon (#FFD700) — filled color
- BackIcon, BackIconGold, CloseIcon, EditIcon — funcionales
- icon_lock_gold, icon_lock_silver — funcionales
- ErrorIcon, TimerIcon, RoundIcon, ShareIcon, StarRecommended — utilitarios

### Contenido — No requieren estilo minimalista (son "producto", no "UI")
- Achievement icons (52) — estilo 3D cartoon, consistente, aceptable para recompensas
- Currency pack icons (icon_digitcoin_pack_*, icon_digitgem_pack_*) — ilustraciones de producto
- icon_digitcoin_single, icon_digitgem_single, icon_xp — currency indicators
- DigitCoinIcon, DigitGemIcon — runtime currency
- DailyReward icons (icon_gift_day1-7, icon_gift_open_*, icon_daily_*) — contenido de recompensa

### Utilitarios — No son iconos visuales
- WhiteSquare — placeholder sprite para Image components
- CircleSprite — mask/shape util
- AvatarDefault — placeholder de avatar
- PairsIcon — icono especifico de MemoryPairs gameplay
- Apple.png, google_icon_dark.png — logos de terceros, NO tocar

### Mission icons (ms_*) — Ya regenerados en nuevo set minimalista
- ms_play, ms_trophy, ms_game, ms_star, ms_target, ms_grid, ms_flame, ms_brain, ms_coin, ms_cup, ms_friend
- Cargados via `MissionCardUI.ActionTypeIconPaths` desde `Resources/Icons/Missions/`

---

## SECCION 3: ICONOS QUE DEBEN REGENERARSE COMO MINIMALISTAS

### Criterio: Son iconos de UI/navegacion que actualmente son 3D detallados con glow/gradients.
### Las apps profesionales (Duolingo, Spotify, Instagram) usan flat icons para navegacion.

---

### 3A. Game Icons (6 iconos — usados en ~20 lugares)

Estos son los MAS importantes. Se usan en GameSelector, PlayModeSelection, CashBattleHub, MatchHistory, Scores, filters, CashBattle1v1 GameSelectionModal.

**Ruta**: `Art/Icons/Games/{name}.png` + copiar a `Resources/Icons/Games/{name}.png`

#### ICON-1: DigitRushIcon
**Color**: #00E5FF (cyan neon)
```
PNG format, 1024x1024 pixels, perfectly square. The background MUST be 100% transparent — NOT white, NOT gray, NOT checkered, NOT any solid color, NOT any gradient. There must be ZERO background of any kind. The icon floats on pure empty transparent space. This will be used as a UI icon in a mobile app where any non-transparent pixel will be visible. Style: flat minimalist UI glyph, single color with NO gradients, NO 3D effects, NO shadows, NO glow, NO glossy highlights. Clean geometric shapes, consistent 2px stroke weight, like Apple SF Symbols or Google Material Icons. The icon must be perfectly centered, filling ~70% of the canvas with even margins. The icon is: a minimalist numeric keypad or number grid — 3x3 grid of small rounded squares suggesting digits, like a phone keypad. Single color: #00E5FF (cyan). No numbers inside the squares, just empty rounded rectangles in a grid pattern. Represents a fast number-typing game.
```

#### ICON-2: FlashTapIcon
**Color**: #FF6B35 (naranja)
```
PNG format, 1024x1024 pixels, perfectly square. The background MUST be 100% transparent — NOT white, NOT gray, NOT checkered, NOT any solid color, NOT any gradient. There must be ZERO background of any kind. The icon floats on pure empty transparent space. This will be used as a UI icon in a mobile app where any non-transparent pixel will be visible. Style: flat minimalist UI glyph, single color with NO gradients, NO 3D effects, NO shadows, NO glow, NO glossy highlights. Clean geometric shapes, consistent 2px stroke weight, like Apple SF Symbols or Google Material Icons. The icon must be perfectly centered, filling ~70% of the canvas with even margins. The icon is: a minimalist finger tap symbol — a single pointing finger (index finger) touching a circular ripple effect (2-3 concentric circles emanating from the touch point). Single color: #FF6B35 (orange). Clean outline style, like a touch/tap gesture indicator. Represents a fast reflex tapping game.
```

#### ICON-3: MemoryPairsIcon
**Color**: #A855F7 (purpura)
```
PNG format, 1024x1024 pixels, perfectly square. The background MUST be 100% transparent — NOT white, NOT gray, NOT checkered, NOT any solid color, NOT any gradient. There must be ZERO background of any kind. The icon floats on pure empty transparent space. This will be used as a UI icon in a mobile app where any non-transparent pixel will be visible. Style: flat minimalist UI glyph, single color with NO gradients, NO 3D effects, NO shadows, NO glow, NO glossy highlights. Clean geometric shapes, consistent 2px stroke weight, like Apple SF Symbols or Google Material Icons. The icon must be perfectly centered, filling ~70% of the canvas with even margins. The icon is: two minimalist playing cards side by side, slightly overlapping, both face-down with a simple "?" mark on each. Single color: #A855F7 (purple). Clean outline style. Represents a memory matching card game.
```

#### ICON-4: OddOneOutIcon
**Color**: #22C55E (verde)
```
PNG format, 1024x1024 pixels, perfectly square. The background MUST be 100% transparent — NOT white, NOT gray, NOT checkered, NOT any solid color, NOT any gradient. There must be ZERO background of any kind. The icon floats on pure empty transparent space. This will be used as a UI icon in a mobile app where any non-transparent pixel will be visible. Style: flat minimalist UI glyph, single color with NO gradients, NO 3D effects, NO shadows, NO glow, NO glossy highlights. Clean geometric shapes, consistent 2px stroke weight, like Apple SF Symbols or Google Material Icons. The icon must be perfectly centered, filling ~70% of the canvas with even margins. The icon is: a 2x2 grid of four circles where three are identical (outline only) and one is filled/solid, representing the "odd one out" concept. Single color: #22C55E (green). The filled circle should be visually distinct from the three outline circles. Clean geometric style.
```

#### ICON-5: QuickMathIcon
**Color**: #F59E0B (amarillo/dorado)
```
PNG format, 1024x1024 pixels, perfectly square. The background MUST be 100% transparent — NOT white, NOT gray, NOT checkered, NOT any solid color, NOT any gradient. There must be ZERO background of any kind. The icon floats on pure empty transparent space. This will be used as a UI icon in a mobile app where any non-transparent pixel will be visible. Style: flat minimalist UI glyph, single color with NO gradients, NO 3D effects, NO shadows, NO glow, NO glossy highlights. Clean geometric shapes, consistent 2px stroke weight, like Apple SF Symbols or Google Material Icons. The icon must be perfectly centered, filling ~70% of the canvas with even margins. The icon is: minimalist math operation symbols arranged in a 2x2 pattern — plus (+), minus (-), multiply (x), divide (÷). Single color: #F59E0B (golden yellow). Clean geometric style, each symbol in its own invisible quadrant. Represents a fast math solving game.
```

#### ICON-6: CognitiveSprintIcon
**Color**: #EC4899 (rosa/magenta)
```
PNG format, 1024x1024 pixels, perfectly square. The background MUST be 100% transparent — NOT white, NOT gray, NOT checkered, NOT any solid color, NOT any gradient. There must be ZERO background of any kind. The icon floats on pure empty transparent space. This will be used as a UI icon in a mobile app where any non-transparent pixel will be visible. Style: flat minimalist UI glyph, single color with NO gradients, NO 3D effects, NO shadows, NO glow, NO glossy highlights. Clean geometric shapes, consistent 2px stroke weight, like Apple SF Symbols or Google Material Icons. The icon must be perfectly centered, filling ~70% of the canvas with even margins. The icon is: a minimalist brain silhouette with a small lightning bolt on the right side, suggesting mental speed. Single color: #EC4899 (pink/magenta). The brain is a simplified outline (not anatomically detailed), and the lightning bolt is small and geometric. Represents a multi-game cognitive challenge sprint.
```

---

### 3B. PlayMode Selection Icons (3 iconos)

**Ruta**: `Art/Icons/PlayMode/{name}.png`

#### ICON-7: PlayModeSelectionSoloIcon
**Color**: #00E5FF (cyan)
```
PNG format, 1024x1024 pixels, perfectly square. The background MUST be 100% transparent — NOT white, NOT gray, NOT checkered, NOT any solid color, NOT any gradient. There must be ZERO background of any kind. The icon floats on pure empty transparent space. This will be used as a UI icon in a mobile app where any non-transparent pixel will be visible. Style: flat minimalist UI glyph, single color with NO gradients, NO 3D effects, NO shadows, NO glow, NO glossy highlights. Clean geometric shapes, consistent 2px stroke weight, like Apple SF Symbols or Google Material Icons. The icon must be perfectly centered, filling ~70% of the canvas with even margins. The icon is: a minimalist single person silhouette (head + shoulders, like a user icon) with a small brain/thought bubble above. Single color: #00E5FF (cyan). Clean outline style. Represents solo practice mode.
```

#### ICON-8: PlayModeSelection1v1Icon
**Color**: #FF4444 (rojo)
```
PNG format, 1024x1024 pixels, perfectly square. The background MUST be 100% transparent — NOT white, NOT gray, NOT checkered, NOT any solid color, NOT any gradient. There must be ZERO background of any kind. The icon floats on pure empty transparent space. This will be used as a UI icon in a mobile app where any non-transparent pixel will be visible. Style: flat minimalist UI glyph, single color with NO gradients, NO 3D effects, NO shadows, NO glow, NO glossy highlights. Clean geometric shapes, consistent 2px stroke weight, like Apple SF Symbols or Google Material Icons. The icon must be perfectly centered, filling ~70% of the canvas with even margins. The icon is: two minimalist crossed swords forming an X shape. Single color: #FF4444 (red). Clean geometric style — straight blades with simple crossguards, no ornate details. Represents 1v1 ranked competitive mode.
```

#### ICON-9: PlayModeSelectionTorunamentIcon
**Color**: #FFD700 (dorado)
```
PNG format, 1024x1024 pixels, perfectly square. The background MUST be 100% transparent — NOT white, NOT gray, NOT checkered, NOT any solid color, NOT any gradient. There must be ZERO background of any kind. The icon floats on pure empty transparent space. This will be used as a UI icon in a mobile app where any non-transparent pixel will be visible. Style: flat minimalist UI glyph, single color with NO gradients, NO 3D effects, NO shadows, NO glow, NO glossy highlights. Clean geometric shapes, consistent 2px stroke weight, like Apple SF Symbols or Google Material Icons. The icon must be perfectly centered, filling ~70% of the canvas with even margins. The icon is: a minimalist trophy cup — simple U-shape cup on a small rectangular base with two small handles on the sides. Single color: #FFD700 (gold). Clean outline style, no stars or decorations inside. Represents tournament mode.
```

---

### 3C. CashBattle Hub Icons (5 iconos)

**Ruta**: `Art/Icons/CashBattle/Hub/{name}.png`

#### ICON-10: Battles1v1Icon
**Color**: #00FF88 (verde cash)
```
PNG format, 1024x1024 pixels, perfectly square. The background MUST be 100% transparent — NOT white, NOT gray, NOT checkered, NOT any solid color, NOT any gradient. There must be ZERO background of any kind. The icon floats on pure empty transparent space. This will be used as a UI icon in a mobile app where any non-transparent pixel will be visible. Style: flat minimalist UI glyph, single color with NO gradients, NO 3D effects, NO shadows, NO glow, NO glossy highlights. Clean geometric shapes, consistent 2px stroke weight, like Apple SF Symbols or Google Material Icons. The icon must be perfectly centered, filling ~70% of the canvas with even margins. The icon is: two minimalist crossed swords forming an X shape with a small circular coin symbol at the center intersection. Single color: #00FF88 (cash green). Clean geometric style. Represents real-money 1v1 battles.
```

#### ICON-11: TournamentsCashIcon
**Color**: #00FF88 (verde cash)
```
PNG format, 1024x1024 pixels, perfectly square. The background MUST be 100% transparent — NOT white, NOT gray, NOT checkered, NOT any solid color, NOT any gradient. There must be ZERO background of any kind. The icon floats on pure empty transparent space. This will be used as a UI icon in a mobile app where any non-transparent pixel will be visible. Style: flat minimalist UI glyph, single color with NO gradients, NO 3D effects, NO shadows, NO glow, NO glossy highlights. Clean geometric shapes, consistent 2px stroke weight, like Apple SF Symbols or Google Material Icons. The icon must be perfectly centered, filling ~70% of the canvas with even margins. The icon is: a minimalist trophy cup (simple U-shape with two handles on a base) with a small dollar sign ($) centered inside the cup. Single color: #00FF88 (cash green). Clean outline style. Represents real-money tournaments.
```

#### ICON-12: WalletCashIcon
**Color**: #00FF88 (verde cash)
```
PNG format, 1024x1024 pixels, perfectly square. The background MUST be 100% transparent — NOT white, NOT gray, NOT checkered, NOT any solid color, NOT any gradient. There must be ZERO background of any kind. The icon floats on pure empty transparent space. This will be used as a UI icon in a mobile app where any non-transparent pixel will be visible. Style: flat minimalist UI glyph, single color with NO gradients, NO 3D effects, NO shadows, NO glow, NO glossy highlights. Clean geometric shapes, consistent 2px stroke weight, like Apple SF Symbols or Google Material Icons. The icon must be perfectly centered, filling ~70% of the canvas with even margins. The icon is: a minimalist billfold wallet shown from the front, slightly open at the top, with a small dollar bill peeking out. Single color: #00FF88 (cash green). Clean outline style, no detailed stitching or textures. Represents the cash wallet.
```

#### ICON-13: CashProfileIcon
**Color**: #00FF88 (verde cash)
```
PNG format, 1024x1024 pixels, perfectly square. The background MUST be 100% transparent — NOT white, NOT gray, NOT checkered, NOT any solid color, NOT any gradient. There must be ZERO background of any kind. The icon floats on pure empty transparent space. This will be used as a UI icon in a mobile app where any non-transparent pixel will be visible. Style: flat minimalist UI glyph, single color with NO gradients, NO 3D effects, NO shadows, NO glow, NO glossy highlights. Clean geometric shapes, consistent 2px stroke weight, like Apple SF Symbols or Google Material Icons. The icon must be perfectly centered, filling ~70% of the canvas with even margins. The icon is: a minimalist person silhouette (head + shoulders) inside a rounded square frame, like a profile badge. Single color: #00FF88 (cash green). Clean outline style. Represents the cash battle profile.
```

#### ICON-14: HistoryCashIcon
**Color**: #00FF88 (verde cash)
```
PNG format, 1024x1024 pixels, perfectly square. The background MUST be 100% transparent — NOT white, NOT gray, NOT checkered, NOT any solid color, NOT any gradient. There must be ZERO background of any kind. The icon floats on pure empty transparent space. This will be used as a UI icon in a mobile app where any non-transparent pixel will be visible. Style: flat minimalist UI glyph, single color with NO gradients, NO 3D effects, NO shadows, NO glow, NO glossy highlights. Clean geometric shapes, consistent 2px stroke weight, like Apple SF Symbols or Google Material Icons. The icon must be perfectly centered, filling ~70% of the canvas with even margins. The icon is: a minimalist clock face with a small circular arrow going counterclockwise around it, suggesting history/past events. Single color: #00FF88 (cash green). Clean outline style — no numbers on the clock face, just hour and minute hands. Represents match history.
```

---

### 3D. VS Icon (1 icono)

**Ruta**: `Art/Icons/Games/VSIcon.png`

#### ICON-15: VSIcon
**Color**: #FF4444 (rojo)
```
PNG format, 1024x1024 pixels, perfectly square. The background MUST be 100% transparent — NOT white, NOT gray, NOT checkered, NOT any solid color, NOT any gradient. There must be ZERO background of any kind. The icon floats on pure empty transparent space. This will be used as a UI icon in a mobile app where any non-transparent pixel will be visible. Style: flat minimalist UI glyph, single color with NO gradients, NO 3D effects, NO shadows, NO glow, NO glossy highlights. Clean geometric shapes, consistent 2px stroke weight, like Apple SF Symbols or Google Material Icons. The icon must be perfectly centered, filling ~70% of the canvas with even margins. The icon is: the letters "VS" in a bold geometric sans-serif font, contained inside a hexagonal or diamond-shaped outline. Single color: #FF4444 (red). The letters should be thick and impactful. No fire, no glow, no ornate effects. Clean and bold. Represents versus/competitive matchup.
```

---

### 3E. Verification Icon (1 icono)

**Ruta**: `Art/Icons/CashBattle/UI/VerificationIcon.png`

#### ICON-16: VerificationIcon
**Color**: #FFD700 (dorado)
```
PNG format, 1024x1024 pixels, perfectly square. The background MUST be 100% transparent — NOT white, NOT gray, NOT checkered, NOT any solid color, NOT any gradient. There must be ZERO background of any kind. The icon floats on pure empty transparent space. This will be used as a UI icon in a mobile app where any non-transparent pixel will be visible. Style: flat minimalist UI glyph, single color with NO gradients, NO 3D effects, NO shadows, NO glow, NO glossy highlights. Clean geometric shapes, consistent 2px stroke weight, like Apple SF Symbols or Google Material Icons. The icon must be perfectly centered, filling ~70% of the canvas with even margins. The icon is: a minimalist shield shape with "18+" text centered inside (or the number 18 with a plus sign). Single color: #FFD700 (gold). The shield is a simple pointed-bottom polygon outline. Clean and authoritative. Represents age verification (18+ requirement).
```

---

### 3F. Onboarding Icons (6 iconos)

**Ruta**: `Art/Icons/Onboarding/{name}.png`

> NOTA: Onboarding es la UNICA zona donde iconos un poco mas expresivos son aceptables (el usuario lo ve 1 vez, debe causar "wow"). Pero aun asi deben ser minimalist-flat, no 3D detallados.

#### ICON-17: WelcomeIcon
**Color**: #00E5FF (cyan)
```
PNG format, 1024x1024 pixels, perfectly square. The background MUST be 100% transparent — NOT white, NOT gray, NOT checkered, NOT any solid color, NOT any gradient. There must be ZERO background of any kind. The icon floats on pure empty transparent space. This will be used as a UI icon in a mobile app where any non-transparent pixel will be visible. Style: flat minimalist UI glyph, single color with NO gradients, NO 3D effects, NO shadows, NO glow, NO glossy highlights. Clean geometric shapes, consistent 2px stroke weight, like Apple SF Symbols or Google Material Icons. The icon must be perfectly centered, filling ~70% of the canvas with even margins. The icon is: a minimalist waving hand emoji-style — an open palm with fingers together, tilted slightly, with 2-3 small motion lines suggesting a wave gesture. Single color: #00E5FF (cyan). Friendly and welcoming. Represents the welcome/intro slide.
```

#### ICON-18: GamesIcon
**Color**: #A855F7 (purpura)
```
PNG format, 1024x1024 pixels, perfectly square. The background MUST be 100% transparent — NOT white, NOT gray, NOT checkered, NOT any solid color, NOT any gradient. There must be ZERO background of any kind. The icon floats on pure empty transparent space. This will be used as a UI icon in a mobile app where any non-transparent pixel will be visible. Style: flat minimalist UI glyph, single color with NO gradients, NO 3D effects, NO shadows, NO glow, NO glossy highlights. Clean geometric shapes, consistent 2px stroke weight, like Apple SF Symbols or Google Material Icons. The icon must be perfectly centered, filling ~70% of the canvas with even margins. The icon is: a minimalist game controller viewed from the front — a rounded rectangular shape with a D-pad on the left and two small circles (buttons) on the right. Single color: #A855F7 (purple). Clean outline style. Represents the games overview slide.
```

#### ICON-19: CashBattleIcon (Onboarding)
**Color**: #00FF88 (verde cash)
```
PNG format, 1024x1024 pixels, perfectly square. The background MUST be 100% transparent — NOT white, NOT gray, NOT checkered, NOT any solid color, NOT any gradient. There must be ZERO background of any kind. The icon floats on pure empty transparent space. This will be used as a UI icon in a mobile app where any non-transparent pixel will be visible. Style: flat minimalist UI glyph, single color with NO gradients, NO 3D effects, NO shadows, NO glow, NO glossy highlights. Clean geometric shapes, consistent 2px stroke weight, like Apple SF Symbols or Google Material Icons. The icon must be perfectly centered, filling ~70% of the canvas with even margins. The icon is: a minimalist shield with a dollar sign ($) centered inside. Single color: #00FF88 (cash green). The shield is a simple rounded-top, pointed-bottom shape. Clean and bold. Represents the CashBattle introduction slide.
```

#### ICON-20: TournamentsIcon (Onboarding)
**Color**: #FFD700 (dorado)
```
PNG format, 1024x1024 pixels, perfectly square. The background MUST be 100% transparent — NOT white, NOT gray, NOT checkered, NOT any solid color, NOT any gradient. There must be ZERO background of any kind. The icon floats on pure empty transparent space. This will be used as a UI icon in a mobile app where any non-transparent pixel will be visible. Style: flat minimalist UI glyph, single color with NO gradients, NO 3D effects, NO shadows, NO glow, NO glossy highlights. Clean geometric shapes, consistent 2px stroke weight, like Apple SF Symbols or Google Material Icons. The icon must be perfectly centered, filling ~70% of the canvas with even margins. The icon is: a minimalist trophy cup with a small star above it, suggesting championship. Single color: #FFD700 (gold). The trophy is a simple U-shape with handles on a base, and the star is a simple 5-pointed star centered above. Represents the tournaments introduction slide.
```

#### ICON-21: RewardsIcon (Onboarding)
**Color**: #FF6B35 (naranja)
```
PNG format, 1024x1024 pixels, perfectly square. The background MUST be 100% transparent — NOT white, NOT gray, NOT checkered, NOT any solid color, NOT any gradient. There must be ZERO background of any kind. The icon floats on pure empty transparent space. This will be used as a UI icon in a mobile app where any non-transparent pixel will be visible. Style: flat minimalist UI glyph, single color with NO gradients, NO 3D effects, NO shadows, NO glow, NO glossy highlights. Clean geometric shapes, consistent 2px stroke weight, like Apple SF Symbols or Google Material Icons. The icon must be perfectly centered, filling ~70% of the canvas with even margins. The icon is: a minimalist gift box — a square box with a ribbon cross on top and a small bow at the center top. Single color: #FF6B35 (orange). Clean outline style, no wrapping paper patterns. Represents the rewards/monetization introduction slide.
```

#### ICON-22: CompleteIcon (Onboarding)
**Color**: #22C55E (verde)
```
PNG format, 1024x1024 pixels, perfectly square. The background MUST be 100% transparent — NOT white, NOT gray, NOT checkered, NOT any solid color, NOT any gradient. There must be ZERO background of any kind. The icon floats on pure empty transparent space. This will be used as a UI icon in a mobile app where any non-transparent pixel will be visible. Style: flat minimalist UI glyph, single color with NO gradients, NO 3D effects, NO shadows, NO glow, NO glossy highlights. Clean geometric shapes, consistent 2px stroke weight, like Apple SF Symbols or Google Material Icons. The icon must be perfectly centered, filling ~70% of the canvas with even margins. The icon is: a minimalist checkmark inside a circle — a bold check/tick mark centered inside a circular outline. Single color: #22C55E (green). The checkmark is thick and confident. Represents onboarding completion.
```

---

### 3G. CashBattle Stat Icons (10 iconos — usados en CashHistory)

**Ruta**: `Art/Icons/CashBattle/Stats/{name}.png`
**Color**: Todos en #FFFFFF (blanco) — se colorean via Image.color en el UIBuilder

#### ICON-23: stat_victories
```
PNG format, 1024x1024 pixels, perfectly square. The background MUST be 100% transparent — NOT white, NOT gray, NOT checkered, NOT any solid color, NOT any gradient. There must be ZERO background of any kind. The icon floats on pure empty transparent space. This will be used as a UI icon in a mobile app where any non-transparent pixel will be visible. Style: flat minimalist UI glyph, single color with NO gradients, NO 3D effects, NO shadows, NO glow, NO glossy highlights. Clean geometric shapes, consistent 2px stroke weight, like Apple SF Symbols or Google Material Icons. The icon must be perfectly centered, filling ~70% of the canvas with even margins. The icon is: a minimalist trophy cup outline. Single color: white (#FFFFFF). Simple U-shape with handles on a small base.
```

#### ICON-24: stat_defeats
```
PNG format, 1024x1024 pixels, perfectly square. The background MUST be 100% transparent — NOT white, NOT gray, NOT checkered, NOT any solid color, NOT any gradient. There must be ZERO background of any kind. The icon floats on pure empty transparent space. This will be used as a UI icon in a mobile app where any non-transparent pixel will be visible. Style: flat minimalist UI glyph, single color with NO gradients, NO 3D effects, NO shadows, NO glow, NO glossy highlights. Clean geometric shapes, consistent 2px stroke weight, like Apple SF Symbols or Google Material Icons. The icon must be perfectly centered, filling ~70% of the canvas with even margins. The icon is: a minimalist thumbs-down hand outline. Single color: white (#FFFFFF). Simple geometric hand with thumb pointing down.
```

#### ICON-25: stat_winrate
```
PNG format, 1024x1024 pixels, perfectly square. The background MUST be 100% transparent — NOT white, NOT gray, NOT checkered, NOT any solid color, NOT any gradient. There must be ZERO background of any kind. The icon floats on pure empty transparent space. This will be used as a UI icon in a mobile app where any non-transparent pixel will be visible. Style: flat minimalist UI glyph, single color with NO gradients, NO 3D effects, NO shadows, NO glow, NO glossy highlights. Clean geometric shapes, consistent 2px stroke weight, like Apple SF Symbols or Google Material Icons. The icon must be perfectly centered, filling ~70% of the canvas with even margins. The icon is: a minimalist pie chart with approximately 3/4 filled, representing a percentage/ratio. Single color: white (#FFFFFF). Clean circle with one wedge separated.
```

#### ICON-26: stat_earnings
```
PNG format, 1024x1024 pixels, perfectly square. The background MUST be 100% transparent — NOT white, NOT gray, NOT checkered, NOT any solid color, NOT any gradient. There must be ZERO background of any kind. The icon floats on pure empty transparent space. This will be used as a UI icon in a mobile app where any non-transparent pixel will be visible. Style: flat minimalist UI glyph, single color with NO gradients, NO 3D effects, NO shadows, NO glow, NO glossy highlights. Clean geometric shapes, consistent 2px stroke weight, like Apple SF Symbols or Google Material Icons. The icon must be perfectly centered, filling ~70% of the canvas with even margins. The icon is: a minimalist dollar sign ($) inside a circle. Single color: white (#FFFFFF). Bold S-shaped dollar symbol with two vertical strokes through it.
```

#### ICON-27: stat_draws
```
PNG format, 1024x1024 pixels, perfectly square. The background MUST be 100% transparent — NOT white, NOT gray, NOT checkered, NOT any solid color, NOT any gradient. There must be ZERO background of any kind. The icon floats on pure empty transparent space. This will be used as a UI icon in a mobile app where any non-transparent pixel will be visible. Style: flat minimalist UI glyph, single color with NO gradients, NO 3D effects, NO shadows, NO glow, NO glossy highlights. Clean geometric shapes, consistent 2px stroke weight, like Apple SF Symbols or Google Material Icons. The icon must be perfectly centered, filling ~70% of the canvas with even margins. The icon is: a minimalist equals sign (=) inside a circle, representing a tie/draw. Single color: white (#FFFFFF). Two horizontal parallel lines inside a circular outline.
```

#### ICON-28: stat_total
```
PNG format, 1024x1024 pixels, perfectly square. The background MUST be 100% transparent — NOT white, NOT gray, NOT checkered, NOT any solid color, NOT any gradient. There must be ZERO background of any kind. The icon floats on pure empty transparent space. This will be used as a UI icon in a mobile app where any non-transparent pixel will be visible. Style: flat minimalist UI glyph, single color with NO gradients, NO 3D effects, NO shadows, NO glow, NO glossy highlights. Clean geometric shapes, consistent 2px stroke weight, like Apple SF Symbols or Google Material Icons. The icon must be perfectly centered, filling ~70% of the canvas with even margins. The icon is: a minimalist bar chart with three vertical bars of ascending height (short, medium, tall). Single color: white (#FFFFFF). Clean geometric rectangles side by side.
```

#### ICON-29: stat_streak
```
PNG format, 1024x1024 pixels, perfectly square. The background MUST be 100% transparent — NOT white, NOT gray, NOT checkered, NOT any solid color, NOT any gradient. There must be ZERO background of any kind. The icon floats on pure empty transparent space. This will be used as a UI icon in a mobile app where any non-transparent pixel will be visible. Style: flat minimalist UI glyph, single color with NO gradients, NO 3D effects, NO shadows, NO glow, NO glossy highlights. Clean geometric shapes, consistent 2px stroke weight, like Apple SF Symbols or Google Material Icons. The icon must be perfectly centered, filling ~70% of the canvas with even margins. The icon is: a minimalist flame/fire icon — a single teardrop-shaped flame. Single color: white (#FFFFFF). Smooth curved outline suggesting fire, not realistic.
```

#### ICON-30: stat_beststreak
```
PNG format, 1024x1024 pixels, perfectly square. The background MUST be 100% transparent — NOT white, NOT gray, NOT checkered, NOT any solid color, NOT any gradient. There must be ZERO background of any kind. The icon floats on pure empty transparent space. This will be used as a UI icon in a mobile app where any non-transparent pixel will be visible. Style: flat minimalist UI glyph, single color with NO gradients, NO 3D effects, NO shadows, NO glow, NO glossy highlights. Clean geometric shapes, consistent 2px stroke weight, like Apple SF Symbols or Google Material Icons. The icon must be perfectly centered, filling ~70% of the canvas with even margins. The icon is: a minimalist flame with a small star at its tip, representing a best/record streak. Single color: white (#FFFFFF). The flame is a teardrop shape and the star is tiny, at the top.
```

#### ICON-31: stat_tourneysplayed
```
PNG format, 1024x1024 pixels, perfectly square. The background MUST be 100% transparent — NOT white, NOT gray, NOT checkered, NOT any solid color, NOT any gradient. There must be ZERO background of any kind. The icon floats on pure empty transparent space. This will be used as a UI icon in a mobile app where any non-transparent pixel will be visible. Style: flat minimalist UI glyph, single color with NO gradients, NO 3D effects, NO shadows, NO glow, NO glossy highlights. Clean geometric shapes, consistent 2px stroke weight, like Apple SF Symbols or Google Material Icons. The icon must be perfectly centered, filling ~70% of the canvas with even margins. The icon is: a minimalist tournament bracket — two short lines on the left connecting to one line on the right, repeated twice vertically, forming a simple elimination bracket shape. Single color: white (#FFFFFF).
```

#### ICON-32: stat_tourneyswins
PNG format, 1024x1024 pixels, perfectly square. The background MUST be 100% transparent — NOT white, NOT gray, NOT checkered, NOT any solid color, NOT any gradient. There must be ZERO background of any kind. The icon floats on pure empty transparent space. This will be used as a UI icon in a mobile app where any non-transparent pixel will be visible. Style: flat minimalist UI glyph, single color with NO gradients, NO 3D effects, NO shadows, NO glow, NO glossy highlights. Clean geometric shapes, consistent 2px stroke weight, like Apple SF Symbols or Google Material Icons. The icon must be perfectly centered, filling ~70% of the canvas with even margins. The icon is: a minimalist trophy cup with a checkmark inside it. Single color: white (#FFFFFF). Simple U-shape trophy with a bold check mark centered in the cup area.
```

---

## SECCION 4: ICONOS QUE NO NECESITAN ICONO (Evaluar si quitar)

Estos son lugares donde el audit visual detecto iconos que probablemente NO deberian tener icono, o donde el icono actual es innecesario:

| Zona | Icono actual | Veredicto | Razon |
|------|-------------|-----------|-------|
| CashBattle Tournament cards | 5 iconos huerfanos (DEL-4 a DEL-8) | YA text-only | `CashBattlePrefabBuilder` crea cards sin iconos — los iconos existian pero nunca se integraron |
| CashWallet Transaction items | 3 iconos huerfanos (DEL-9 a DEL-11) | YA text-only | `WalletPrefabGenerator` crea items sin iconos |
| CashWallet VerifiedBadge | DEL-12 | BORRAR | Sin referencia, badge se maneja con texto |
| Mission locked state | MissionLockedIcon (DEL-14) | BORRAR | Las missions usan ms_* set, locked state se maneja via opacity/overlay, no icono aparte |

**Nota sobre CashBattle**: La zona de dinero real (CashBattle) ya fue disenada con un enfoque text-only/minimal para las cards de torneos y transacciones. Los iconos detallados que existian ahi NUNCA se integraron al codigo — fueron creados pero nunca referenciados. Esto es correcto: en apps de dinero real, menos decoracion = mas confianza.

---

## RESUMEN

| Accion | Cantidad |
|--------|----------|
| **Borrar** (huerfanos) | 14 iconos |
| **No tocar** (ya OK) | ~65 iconos |
| **Regenerar** (detallados → minimalistas) | 32 iconos (ICON-1 a ICON-32) |

### Orden de generacion recomendado
1. **Game Icons** (ICON-1 a ICON-6) — se usan en ~20 lugares, maximo impacto
2. **CashBattle Hub** (ICON-10 a ICON-14) — segunda pantalla mas visitada
3. **PlayMode** (ICON-7 a ICON-9) + **VS** (ICON-15) — navegacion de juegos
4. **Stat Icons** (ICON-23 a ICON-32) — consistencia visual en historial
5. **Onboarding** (ICON-17 a ICON-22) — baja prioridad, se ve 1 vez
6. **Verification** (ICON-16) — se ve 1 vez en AgeVerification

### Post-generacion
Despues de generar cada PNG en DALL-E:
1. Verificar en Photoshop/GIMP que el fondo sea REALMENTE transparente (no blanco)
2. Si DALL-E genero fondo blanco/gris: usar "Remove Background" antes de guardar
3. Guardar como PNG-24 con alpha channel
4. Copiar a la ruta indicada en cada seccion
5. Si el icono existe en `Resources/` tambien, copiar ahi + verificar .meta
