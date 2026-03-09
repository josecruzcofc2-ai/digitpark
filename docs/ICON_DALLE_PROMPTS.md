# DALL-E Icon Prompts - DigitPark Minimalist Icons

> **Style Guide**: All icons must be minimalist, flat UI glyphs. Primarily **pure white** silhouettes with subtle **light gray (#C0C0C0) outline** for edge definition. Some icons use **gold (#FFD700)** or special colors noted per-icon. Size: **512x512px**.

---

## ICONOS NO USADOS (se pueden eliminar)

| Icono | Ruta | Estado |
|-------|------|--------|
| `StarRecommended.png` | `UI/StarRecommended.png` | **NO USADO** - zero code references |
| `RoundIcon.png` | `UI/RoundIcon.png` | **NO USADO** - zero code references |

---

## EXCEPCIONES (NO regenerar - ya estan bien)

Los siguientes iconos son excepciones y **NO necesitan regenerarse**:

- **Achievements/** - Todos los 52 iconos de logros (son detallados por diseno)
- **AppIcon/** - Iconos de la app (requieren diseno especial)
- **Auth/** - Apple/Google logos oficiales
- **Currency/** - DigitCoin/DigitGem packs (iconos de moneda detallados)
- **DailyRewards/** - Iconos de regalo (iconos decorativos)
- **Logos/** - LogoDigitPark_Text, LogoDigitPark_Brain
- **Games/FlashTap/** - Botones del juego (son assets de gameplay)
- **Games/ (todos EXCEPTO VSIcon)** - DigitRushIcon, FlashTapIcon, MemoryPairsIcon, OddOneOutIcon, QuickMathIcon, CognitiveSprintIcon (son iconos neon detallados de juego - excepciones)
- **Onboarding/** - WelcomeIcon, GamesIcon, CashBattleIcon, TournamentsIcon, CompleteIcon, RewardsIcon (iconos ilustrativos detallados)
- **CashBattle/Hub/** - Battles1v1Icon, CashProfileIcon, HistoryCashIcon, TournamentsCashIcon, WalletCashIcon (iconos detallados de CashBattle)
- **CashBattle/Stats/** - Todos los stat_* icons (iconos de colores neon por diseno)
- **CashBattle/UI/** - CashBattleIcon, icon_cash, VerificationIcon (iconos detallados)
- **CashBattle/Wallet/** - DepositIcon, WithdrawIcon (iconos detallados)
- **CashBattle/Tournaments/** - TrophyPrizeIcon (icono detallado)
- **UI/warning.png** - Icono de advertencia amarillo/gris (ya tiene buen estilo)
- **UI/icon_lock_gold.png** - Candado dorado 3D (excepcion dorada - ya esta bien)
- **UI/icon_lock_silver.png** - Candado plateado 3D (excepcion plateada - ya esta bien)
- **UI/WhiteSquare.png** - Utility sprite (cuadrado blanco puro)
- **UI/CircleSprite.png** - Utility sprite (circulo blanco puro)
- **UI/RoundedRect.png** - Utility sprite (rectangulo redondeado)
- **UI/StarRecommended.png** - NO USADO, eliminar
- **UI/RoundIcon.png** - NO USADO, eliminar

### Excepciones de color especial (ya correctos):
- **AddFriendIcon.png** - Verde (#4CAF50) - fomenta social/amistad - **EXCEPCION VERDE, NO regenerar**
- **PairsIcon.png** - Cyan (#00E5FF) - icono de juego Memory - **EXCEPCION CYAN, NO regenerar**
- **UI/AchievementsIcon.png** - Dorado (#FFD700) filled medal - **EXCEPCION DORADA, NO regenerar**
- **UI/ShopIcon.png** - Dorado (#FFD700) filled bag - **EXCEPCION DORADA, NO regenerar**
- **UI/PremiumIcon.png** - Dorado (#FFD700) filled crown - **EXCEPCION DORADA, NO regenerar**

---

## ICONOS A REGENERAR - Prompts DALL-E

### 1. VSIcon.png (Games/)
**Necesita regenerar** - Actualmente es demasiado detallado con neon/fuego. Debe ser minimalista.
```
A minimalist flat "VS" text icon for a mobile game UI. The letters "V" and "S" in bold,
clean sans-serif font, pure white color, centered. A very subtle light gray (#C0C0C0)
thin outline around the letters for edge definition. No effects, no glow, no fire, no
neon, no circles, no backgrounds. Just the clean "VS" text as a simple UI glyph symbol.
The background MUST be completely transparent (PNG with alpha channel).
The background MUST be transparent - do NOT generate a fake transparent background
made of gray and white checkerboard squares. Output a true transparent PNG background.
512x512 pixels.
```

### 2. BackIcon.png (Navigation/)
**Necesita regenerar** - Actualmente tiene fondo azul neon 3D con marco. Debe ser minimalista.
```
A minimalist flat left-pointing chevron arrow icon for a mobile app back button.
Pure white color, single clean stroke, no fill. Simple "<" chevron shape with rounded
ends and uniform 3px stroke weight. Very subtle light gray (#C0C0C0) outer edge for
definition. No background, no frame, no 3D effects, no glow, no gradients.
Just a clean minimal left chevron UI glyph symbol.
The background MUST be completely transparent (PNG with alpha channel).
The background MUST be transparent - do NOT generate a fake transparent background
made of gray and white checkerboard squares. Output a true transparent PNG background.
512x512 pixels.
```

### 3. BackIconGold.png (Navigation/)
**Necesita regenerar** - Actualmente tiene fondo dorado 3D con marco. Debe ser minimalista dorado.
```
A minimalist flat left-pointing chevron arrow icon for a mobile app back button.
Solid gold color (#F5C842), single clean stroke, no fill. Simple "<" chevron shape
with rounded ends and uniform 3px stroke weight. Very subtle darker gold (#C9A030)
outer edge for definition. No background, no frame, no 3D effects, no glow, no
gradients. Just a clean minimal left chevron UI glyph symbol in gold color.
The background MUST be completely transparent (PNG with alpha channel).
The background MUST be transparent - do NOT generate a fake transparent background
made of gray and white checkerboard squares. Output a true transparent PNG background.
512x512 pixels.
```

### 4. SettingsIcon.png (Navigation/)
**Necesita regenerar** - Actualmente es outline blanco pero los bordes son demasiado finos y apenas visibles.
```
A minimalist flat gear/cog icon for a mobile app settings button. Pure white filled
gear shape with 8 teeth, clean edges. A very subtle light gray (#C0C0C0) thin outline
around the outer edge for definition. Small circular hole in the center. No background,
no 3D effects, no shadows, no gradients. Simple, solid, recognizable gear UI glyph symbol.
The background MUST be completely transparent (PNG with alpha channel).
The background MUST be transparent - do NOT generate a fake transparent background
made of gray and white checkerboard squares. Output a true transparent PNG background.
512x512 pixels.
```

### 5. NotificationsIcon.png (Navigation/)
**Necesita regenerar** - Actualmente es outline blanco pero bordes muy finos/invisibles.
```
A minimalist flat bell icon for a mobile app notifications button. Pure white filled
bell shape, clean rounded form with a small circle clapper at the bottom. A very subtle
light gray (#C0C0C0) thin outline around the outer edge for definition. No background,
no 3D effects, no shadows, no gradients. Simple, solid, recognizable bell UI glyph symbol.
The background MUST be completely transparent (PNG with alpha channel).
The background MUST be transparent - do NOT generate a fake transparent background
made of gray and white checkerboard squares. Output a true transparent PNG background.
512x512 pixels.
```

### 6. SearchIcon.png (Navigation/)
**Necesita regenerar** - Actualmente es outline blanco pero bordes muy finos/invisibles.
```
A minimalist flat magnifying glass icon for a mobile app search button. Pure white filled
magnifying glass shape - circular lens with a diagonal handle extending to the bottom-right.
A very subtle light gray (#C0C0C0) thin outline around the outer edge for definition.
No background, no 3D effects, no shadows, no gradients. Simple, solid, recognizable
magnifying glass UI glyph symbol.
The background MUST be completely transparent (PNG with alpha channel).
The background MUST be transparent - do NOT generate a fake transparent background
made of gray and white checkerboard squares. Output a true transparent PNG background.
512x512 pixels.
```

### 7. EyeOpen.png (Navigation/)
**Necesita regenerar** - Actualmente es outline blanco pero extremadamente fino, casi invisible.
```
A minimalist flat open eye icon for a mobile app password visibility toggle. Pure white
filled eye shape - almond/lens shape with a solid circle pupil in the center. A very
subtle light gray (#C0C0C0) thin outline around the outer edge for definition.
No background, no 3D effects, no shadows, no gradients. Simple, solid, recognizable
open eye UI glyph symbol.
The background MUST be completely transparent (PNG with alpha channel).
The background MUST be transparent - do NOT generate a fake transparent background
made of gray and white checkerboard squares. Output a true transparent PNG background.
512x512 pixels.
```

### 8. EyeClosed.png (Navigation/)
**Necesita regenerar** - Actualmente es outline blanco pero extremadamente fino, casi invisible.
```
A minimalist flat closed eye icon with a diagonal strikethrough line for a mobile app
password hide toggle. Pure white filled eye shape - almond/lens shape with a solid
circle pupil, and a diagonal line crossing through from top-left to bottom-right.
A very subtle light gray (#C0C0C0) thin outline around the outer edge for definition.
No background, no 3D effects, no shadows, no gradients. Simple, solid, recognizable
closed eye with slash UI glyph symbol.
The background MUST be completely transparent (PNG with alpha channel).
The background MUST be transparent - do NOT generate a fake transparent background
made of gray and white checkerboard squares. Output a true transparent PNG background.
512x512 pixels.
```

### 9. ProfileIcon.png (Social/)
**Necesita regenerar** - Actualmente es outline blanco pero extremadamente fino, casi invisible.
```
A minimalist flat user profile icon for a mobile app. Pure white filled silhouette -
circular head on top and rounded shoulders/torso below. A very subtle light gray
(#C0C0C0) thin outline around the outer edge for definition. No background, no 3D
effects, no shadows, no gradients. Simple, solid, recognizable person/user UI glyph symbol.
The background MUST be completely transparent (PNG with alpha channel).
The background MUST be transparent - do NOT generate a fake transparent background
made of gray and white checkerboard squares. Output a true transparent PNG background.
512x512 pixels.
```

### 10. EditIcon.png (Social/)
**Necesita regenerar** - Actualmente es outline blanco pero extremadamente fino, casi invisible.
```
A minimalist flat pencil/edit icon for a mobile app edit button. Pure white filled
pencil shape - diagonal pencil pointing down-left with a small eraser cap at top-right.
A very subtle light gray (#C0C0C0) thin outline around the outer edge for definition.
No background, no 3D effects, no shadows, no gradients. Simple, solid, recognizable
pencil/edit UI glyph symbol.
The background MUST be completely transparent (PNG with alpha channel).
The background MUST be transparent - do NOT generate a fake transparent background
made of gray and white checkerboard squares. Output a true transparent PNG background.
512x512 pixels.
```

### 11. AvatarDefault.png (Social/)
**Necesita regenerar** - Actualmente es outline blanco pero extremadamente fino, casi invisible.
```
A minimalist flat default avatar icon for a mobile app user placeholder. Pure white
filled silhouette - large circular head centered above a wider rounded shoulders/torso
shape. A very subtle light gray (#C0C0C0) thin outline around the outer edge for
definition. No background, no 3D effects, no shadows, no gradients. Simple, solid,
recognizable generic person avatar UI glyph symbol.
The background MUST be completely transparent (PNG with alpha channel).
The background MUST be transparent - do NOT generate a fake transparent background
made of gray and white checkerboard squares. Output a true transparent PNG background.
512x512 pixels.
```

### 12. PlayIcon.png (UI/)
**Necesita regenerar** - Actualmente es blanco pero con sombra gris que parece un error.
```
A minimalist flat play button triangle icon for a mobile game. Pure white filled
right-pointing triangle with slightly rounded corners. A very subtle light gray
(#C0C0C0) thin outline around the outer edge for definition. No background, no 3D
effects, no shadows, no drop shadows, no gradients. Simple, solid, recognizable
play button triangle UI glyph symbol.
The background MUST be completely transparent (PNG with alpha channel).
The background MUST be transparent - do NOT generate a fake transparent background
made of gray and white checkerboard squares. Output a true transparent PNG background.
512x512 pixels.
```

### 13. RankingsIcon.png (UI/)
**Necesita regenerar** - Actualmente es outline blanco con bordes finos. Necesita mas cuerpo.
```
A minimalist flat trophy cup icon for a mobile app rankings/leaderboard button.
Pure white filled trophy shape - classic cup with two handles on the sides and a
rectangular base/pedestal. A very subtle light gray (#C0C0C0) thin outline around
the outer edge for definition. No background, no 3D effects, no shadows, no gradients.
Simple, solid, recognizable trophy UI glyph symbol.
The background MUST be completely transparent (PNG with alpha channel).
The background MUST be transparent - do NOT generate a fake transparent background
made of gray and white checkerboard squares. Output a true transparent PNG background.
512x512 pixels.
```

### 14. CloseIcon.png (UI/)
**Necesita regenerar** - Actualmente es muy pequeno y apenas visible.
```
A minimalist flat X close icon for a mobile app close/dismiss button. Pure white
color, two diagonal lines crossing to form an "X" shape with rounded ends and uniform
4px stroke weight. A very subtle light gray (#C0C0C0) outer edge for definition.
No background, no circle around it, no 3D effects, no shadows, no gradients.
Simple, clean X mark UI glyph symbol.
The background MUST be completely transparent (PNG with alpha channel).
The background MUST be transparent - do NOT generate a fake transparent background
made of gray and white checkerboard squares. Output a true transparent PNG background.
512x512 pixels.
```

### 15. ErrorIcon.png (UI/)
**Necesita regenerar** - Actualmente es blanco pero casi invisible, parece un circulo con X.
```
A minimalist flat circle with exclamation mark icon for a mobile app error state.
Pure white filled circle with a white exclamation mark "!" cut out in the center
(negative space). A very subtle light gray (#C0C0C0) thin outline around the outer
circle edge for definition. No background, no 3D effects, no shadows, no gradients.
Simple, solid, recognizable error/alert circle UI glyph symbol.
The background MUST be completely transparent (PNG with alpha channel).
The background MUST be transparent - do NOT generate a fake transparent background
made of gray and white checkerboard squares. Output a true transparent PNG background.
512x512 pixels.
```

### 16. ShareIcon.png (UI/)
**Necesita regenerar** - Actualmente outline gris fino. Necesita mas peso visual.
```
A minimalist flat share/upload icon for a mobile app share button. Pure white filled
shape - a tray/box open at the top with an upward-pointing arrow emerging from it.
A very subtle light gray (#C0C0C0) thin outline around the outer edge for definition.
No background, no 3D effects, no shadows, no gradients. Simple, solid, recognizable
iOS-style share UI glyph symbol.
The background MUST be completely transparent (PNG with alpha channel).
The background MUST be transparent - do NOT generate a fake transparent background
made of gray and white checkerboard squares. Output a true transparent PNG background.
512x512 pixels.
```

### 17. TimerIcon.png (UI/)
**Necesita regenerar** - Actualmente outline gris fino. Necesita mas peso visual.
```
A minimalist flat stopwatch/timer icon for a mobile game timer display. Pure white
filled stopwatch shape - circular clock face with a small button on top and a small
side button. Two clock hands inside showing approximately 10:10. A very subtle light
gray (#C0C0C0) thin outline around the outer edge for definition. No background, no
3D effects, no shadows, no gradients. Simple, solid, recognizable stopwatch UI glyph symbol.
The background MUST be completely transparent (PNG with alpha channel).
The background MUST be transparent - do NOT generate a fake transparent background
made of gray and white checkerboard squares. Output a true transparent PNG background.
512x512 pixels.
```

### 18. icon_location_pin.png (UI/)
**Necesita regenerar** - Actualmente outline gris fino. Necesita mas peso visual.
```
A minimalist flat map pin/location icon for a mobile app. Pure white filled
teardrop/pin shape pointing downward with a small circular hole near the top center.
A very subtle light gray (#C0C0C0) thin outline around the outer edge for definition.
No background, no 3D effects, no shadows, no gradients. Simple, solid, recognizable
location pin/marker UI glyph symbol.
The background MUST be completely transparent (PNG with alpha channel).
The background MUST be transparent - do NOT generate a fake transparent background
made of gray and white checkerboard squares. Output a true transparent PNG background.
512x512 pixels.
```

### 19. icon_handshake.png (UI/)
**Necesita regenerar** - Actualmente outline gris fino. Necesita mas peso visual.
```
A minimalist flat handshake icon for a mobile app representing friendship or agreement.
Pure white filled two hands clasped together in a handshake gesture. A very subtle
light gray (#C0C0C0) thin outline around the outer edge for definition. No background,
no 3D effects, no shadows, no gradients. Simple, solid, recognizable handshake UI
glyph symbol.
The background MUST be completely transparent (PNG with alpha channel).
The background MUST be transparent - do NOT generate a fake transparent background
made of gray and white checkerboard squares. Output a true transparent PNG background.
512x512 pixels.
```

### 20. icon_trophy.png (UI/)
**Necesita regenerar** - Actualmente outline gris fino. Necesita mas peso visual.
```
A minimalist flat trophy cup icon for a mobile app tournament/victory display.
Pure white filled trophy shape - classic cup with two handles on the sides, narrow
stem, and rectangular base. A very subtle light gray (#C0C0C0) thin outline around
the outer edge for definition. No background, no 3D effects, no shadows, no gradients.
Simple, solid, recognizable trophy UI glyph symbol.
The background MUST be completely transparent (PNG with alpha channel).
The background MUST be transparent - do NOT generate a fake transparent background
made of gray and white checkerboard squares. Output a true transparent PNG background.
512x512 pixels.
```

### 21. icon_medal.png (UI/)
**Necesita regenerar** - Actualmente outline gris fino. Necesita mas peso visual.
```
A minimalist flat medal icon for a mobile app awards display. Pure white filled
medal shape - circular medallion with a V-shaped ribbon/lanyard at the top, and a
small 5-pointed star in the center of the circle. A very subtle light gray (#C0C0C0)
thin outline around the outer edge for definition. No background, no 3D effects, no
shadows, no gradients. Simple, solid, recognizable medal with ribbon UI glyph symbol.
The background MUST be completely transparent (PNG with alpha channel).
The background MUST be transparent - do NOT generate a fake transparent background
made of gray and white checkerboard squares. Output a true transparent PNG background.
512x512 pixels.
```

### 22. icon_gift.png (UI/)
**Necesita regenerar** - Actualmente outline gris fino. Necesita mas peso visual.
```
A minimalist flat gift box icon for a mobile app rewards display. Pure white filled
gift box shape - square box with a horizontal ribbon across the middle and a bow on
top with two loops. A very subtle light gray (#C0C0C0) thin outline around the outer
edge for definition. No background, no 3D effects, no shadows, no gradients.
Simple, solid, recognizable wrapped gift box UI glyph symbol.
The background MUST be completely transparent (PNG with alpha channel).
The background MUST be transparent - do NOT generate a fake transparent background
made of gray and white checkerboard squares. Output a true transparent PNG background.
512x512 pixels.
```

### 23. MissionsIcon.png (Missions/)
**Necesita regenerar** - Actualmente es outline blanco fino. Necesita mas peso visual.
```
A minimalist flat checklist/clipboard icon for a mobile app daily missions button.
Pure white filled rounded rectangle (clipboard shape) with three horizontal lines
inside, each preceded by a small checkmark. A very subtle light gray (#C0C0C0) thin
outline around the outer edge for definition. No background, no 3D effects, no shadows,
no gradients. Simple, solid, recognizable checklist/tasks UI glyph symbol.
The background MUST be completely transparent (PNG with alpha channel).
The background MUST be transparent - do NOT generate a fake transparent background
made of gray and white checkerboard squares. Output a true transparent PNG background.
512x512 pixels.
```

### 24. PlayModeSelection1v1Icon.png (PlayMode/)
**Necesita regenerar** - Actualmente outline blanco muy fino de espadas cruzadas. Necesita cuerpo.
```
A minimalist flat crossed swords icon for a mobile game 1v1 battle mode selection.
Pure white filled two swords crossed in an X pattern - simple straight blades with
small crossguards and round pommels. A very subtle light gray (#C0C0C0) thin outline
around the outer edge for definition. No background, no 3D effects, no shadows, no
gradients. Simple, solid, recognizable crossed swords duel UI glyph symbol.
The background MUST be completely transparent (PNG with alpha channel).
The background MUST be transparent - do NOT generate a fake transparent background
made of gray and white checkerboard squares. Output a true transparent PNG background.
512x512 pixels.
```

### 25. PlayModeSelectionTorunamentIcon.png (PlayMode/)
**Necesita regenerar** - Actualmente outline blanco muy fino de trofeo. Necesita cuerpo.
```
A minimalist flat trophy cup icon for a mobile game tournament mode selection.
Pure white filled trophy shape - classic cup with two small handles, narrow stem,
and a flat rectangular base. Slightly smaller than full canvas to leave breathing room.
A very subtle light gray (#C0C0C0) thin outline around the outer edge for definition.
No background, no 3D effects, no shadows, no gradients. Simple, solid, recognizable
tournament trophy UI glyph symbol.
The background MUST be completely transparent (PNG with alpha channel).
The background MUST be transparent - do NOT generate a fake transparent background
made of gray and white checkerboard squares. Output a true transparent PNG background.
512x512 pixels.
```

### 26. PlayModeSelectionSoloIcon.png (PlayMode/)
**Necesita regenerar** - Actualmente outline gris de persona con burbuja pensamiento. Necesita cuerpo.
```
A minimalist flat single person silhouette icon with a small thought bubble for a
mobile game solo/practice mode selection. Pure white filled person bust (circle head
+ rounded shoulders) with a small thought cloud bubble above-right consisting of one
small cloud shape and two tiny circles leading to it. A very subtle light gray
(#C0C0C0) thin outline around the outer edge for definition. No background, no 3D
effects, no shadows, no gradients. Simple, solid, recognizable solo player thinking
UI glyph symbol.
The background MUST be completely transparent (PNG with alpha channel).
The background MUST be transparent - do NOT generate a fake transparent background
made of gray and white checkerboard squares. Output a true transparent PNG background.
512x512 pixels.
```

---

## RESUMEN

| Categoria | Total | Regenerar | Excepciones | No usados |
|-----------|-------|-----------|-------------|-----------|
| Navigation | 7 | 7 (BackIcon, BackIconGold, Settings, Notifications, Search, EyeOpen, EyeClosed) | 0 | 0 |
| Social | 4 | 3 (ProfileIcon, EditIcon, AvatarDefault) | 1 (AddFriendIcon - verde) | 0 |
| UI | 23 | 10 (PlayIcon, RankingsIcon, CloseIcon, ErrorIcon, ShareIcon, TimerIcon, icon_location_pin, icon_handshake, icon_trophy, icon_medal, icon_gift) | 10 (warning, locks, WhiteSquare, CircleSprite, RoundedRect, Achievements, Shop, Premium, PairsIcon) | 2 (StarRecommended, RoundIcon) |
| PlayMode | 3 | 3 | 0 | 0 |
| Missions | 1 | 1 | 0 | 0 |
| Games | 7 | 1 (VSIcon) | 6 (todos los iconos de juegos neon) | 0 |
| **TOTAL** | **45** | **26** | **17** | **2** |

### Excepciones de color detectadas:
- **AddFriendIcon** (Social/) - **VERDE** (#4CAF50) - Correcto, fomenta conexion social
- **PairsIcon** (UI/) - **CYAN** (#00E5FF) - Correcto, icono de juego MemoryPairs
- **AchievementsIcon** (UI/) - **DORADO** (#FFD700) - Correcto, medalla de logros
- **ShopIcon** (UI/) - **DORADO** (#FFD700) - Correcto, bolsa de tienda
- **PremiumIcon** (UI/) - **DORADO** (#FFD700) - Correcto, corona premium
- **BackIconGold** (Navigation/) - **DORADO** (#F5C842) - Correcto, back button especial
- **icon_lock_gold** (UI/) - **DORADO 3D** - Excepcion, candado premium
- **icon_lock_silver** (UI/) - **PLATEADO 3D** - Excepcion, candado bloqueado
- **warning** (UI/) - **AMARILLO/GRIS** - Excepcion, icono de advertencia
