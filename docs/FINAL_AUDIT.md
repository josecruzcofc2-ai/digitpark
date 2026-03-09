# AUDITORÍA FINAL DE LOCALIZACIÓN — Pre App Store

**Fecha**: 2026-03-08
**Objetivo**: Encontrar CUALQUIER texto sin traducir que pueda causar rechazo en App Store
**Estado**: Solo documentación — NO se aplicaron fixes

---

## RESUMEN EJECUTIVO

| Categoría | Cantidad | Severidad |
|-----------|----------|-----------|
| Hardcoded Spanish en runtime | 12 strings | **CRÍTICO** |
| Hardcoded English en runtime | 20 strings | **CRÍTICO** |
| Keys dinámicas faltantes en Translations.txt | 32 keys | **CRÍTICO** |
| GO names sin TextNameToKeyMap (genéricos) | ~45 textos | **ALTO** |
| **Total issues** | **~109** | |

---

## SECCIÓN 1 — HARDCODED SPANISH EN RUNTIME (12 strings)

### 1.1 NotificationStorageService.cs — Timestamps y grupos

**Archivo**: `Scripts/Runtime/Services/NotificationStorageService.cs`

| Línea | String | Contexto |
|-------|--------|----------|
| 333 | `"Ahora"` | GetRelativeTime() — timestamp de notificación |
| 334 | `"Hace {min} min"` | GetRelativeTime() — minutos atrás |
| 335 | `"Hace {h}h"` | GetRelativeTime() — horas atrás |
| 336 | `"Ayer"` | GetRelativeTime() — ayer |
| 350 | `"Hoy"` | GetTimeGroup() — grupo de hoy |
| 351 | `"Ayer"` | GetTimeGroup() — grupo de ayer |
| 352 | `"Esta semana"` | GetTimeGroup() — grupo semanal |
| 353 | `"Anteriores"` | GetTimeGroup() — grupo anterior |

**Impacto**: Usuarios EN/FR/DE/PT ven español en la pantalla de notificaciones.

### 1.2 MatchHistoryData.cs — Timestamps

**Archivo**: `Scripts/Runtime/Data/MatchHistoryData.cs`

| Línea | String | Contexto |
|-------|--------|----------|
| 205 | `"Ahora"` | GetFormattedDate() |
| 206 | `"Hace {min} min"` | GetFormattedDate() |
| 207 | `"Hace {h}h"` | GetFormattedDate() |
| 208 | `"Hace {d}d"` | GetFormattedDate() |

**Impacto**: Historial de partidas muestra fechas en español.

---

## SECCIÓN 2 — HARDCODED ENGLISH EN RUNTIME (20 strings)

### 2.1 MatchHistoryData.cs — Result labels

**Archivo**: `Scripts/Runtime/Data/MatchHistoryData.cs`

| Línea | String | Contexto |
|-------|--------|----------|
| 219 | `"WIN"` | GetResultText() |
| 220 | `"LOSS"` | GetResultText() |
| 221 | `"DRAW"` | GetResultText() |
| 222 | `"PRACT."` | GetResultText() |

### 2.2 WalletData.cs — Transaction dates

**Archivo**: `Scripts/Runtime/Features/CashBattle/Wallet/WalletData.cs`

| Línea | String | Contexto |
|-------|--------|----------|
| 175 | `"Now"` | GetFormattedDate() |
| 177 | `"{min}m ago"` | GetFormattedDate() |
| 179 | `"{h}h ago"` | GetFormattedDate() |
| 181 | `"{d}d ago"` | GetFormattedDate() |

### 2.3 CashProfileSceneController.cs — Stats labels

**Archivo**: `Scripts/Runtime/Features/CashBattle/Profile/CashProfileSceneController.cs`

| Línea | String | Contexto |
|-------|--------|----------|
| 200 | `"{streak} W"` / `"{streak} L"` | Streak display |
| 209 | `"{bestStreak} W"` | Best streak |
| 228 | `"{wins}W · {losses}L · {draws}D"` | Record display |
| 230 | `"{winRate}% Win Rate"` | Win rate label |

### 2.4 CashBattle1v1Manager.cs — Entry fee

**Archivo**: `Scripts/Runtime/Features/CashBattle/Hub/CashBattle1v1Manager.cs`

| Línea | String | Contexto |
|-------|--------|----------|
| 571 | `"Entry: ${fee}"` | Bet selection screen |

### 2.5 SearchPlayersManager.cs — Win rate

**Archivo**: `Scripts/Runtime/Features/Social/Friends/SearchPlayersManager.cs`

| Línea | String | Contexto |
|-------|--------|----------|
| 302 | `"{winRate}% WR · {game}"` | Player search results |

### 2.6 ShopItemUI.cs — Bonus badge

**Archivo**: `Scripts/Runtime/Features/Monetization/Shop/ShopItemUI.cs`

| Línea | String | Contexto |
|-------|--------|----------|
| 198 | `"{bonus} BONUS"` | Shop item badge |

### 2.7 FloatingText.cs — Game effects

**Archivo**: `Scripts/Runtime/Effects/FloatingText.cs`

| Línea | String | Contexto |
|-------|--------|----------|
| 75 | `"COMBO x{count}!"` | Floating text in-game |
| 85 | `"PERFECT!"` | Floating text in-game |
| 93 | `"EXCELLENT!"` | Floating text in-game |
| 101 | `"NEW RECORD!"` | Floating text in-game |

### 2.8 ComboVisualController.cs — Combo text

**Archivo**: `Scripts/Runtime/Features/Games/Results/ComboVisualController.cs`

| Línea | String | Contexto |
|-------|--------|----------|
| 341 | `"COMBO x{combo}"` | During gameplay |

---

## SECCIÓN 3 — KEYS DINÁMICAS FALTANTES EN TRANSLATIONS.TXT (32 keys)

### 3.1 Player Titles — 20 keys faltantes

**Archivo**: `Scripts/Runtime/Services/PlayerTitleService.cs`
**Uso**: `AutoLocalizer.Get(title.nameKey)` línea 372

| Key | Descripción |
|-----|------------|
| `title_novato` | Novice |
| `title_jugador` | Player |
| `title_veterano` | Veteran |
| `title_leyenda` | Legend |
| `title_inmortal` | Immortal |
| `title_estratega` | Strategist |
| `title_genio` | Genius |
| `title_maestro` | Master |
| `title_iluminado` | Enlightened |
| `title_primer_paso` | First Step |
| `title_imparable` | Unstoppable |
| `title_madrugador` | Early Bird |
| `title_perfeccionista` | Perfectionist |
| `title_campeon` | Champion |
| `title_coleccionista` | Collector |
| `title_custom` | Custom |
| `title_fantasma` | Ghost |
| `title_velocista` | Speedster |
| `title_rey_comeback` | Comeback King |
| `title_completo` | Complete |

**Impacto**: Títulos de jugador muestran el key name crudo (ej: "title_novato").

### 3.2 Theme Names — 12 keys faltantes

**Archivo**: `Scripts/Runtime/Features/Settings/SettingsManager.cs` líneas 242, 473
**Uso**: `AutoLocalizer.Get($"theme_{theme.themeId}")`

| Key faltante | Nota |
|-------------|------|
| `theme_CoralSurge` | |
| `theme_CyberFuchsia` | |
| `theme_default` | |
| `theme_ElectricBlue` | |
| `theme_Infrared` | |
| `theme_Matrix` | |
| `theme_Nebula` | |
| `theme_Phantom` | |
| `theme_PlasmaIndigo` | |
| `theme_Titanium` | |
| `theme_ToxicLime` | |
| `theme_Volcanic` | |

**Nota**: 8 de 20 temas YA tienen key (Arctic, CrimsonBlaze, DeepOcean, ElectricViolet, Emerald, Monochrome, Sakura, Sunset).

**Impacto**: Nombre de tema muestra "theme_Matrix" en vez de "Matrix" en Settings y Shop.

---

## SECCIÓN 4 — GO NAMES GENÉRICOS SIN TEXTNAMETOKEYMAP (~45 textos)

Estos son textos placeholder en UIBuilders que usan GO names genéricos ("Text", "Title", "Label", "Message") que AutoLocalizer no puede matchear.

### 4.1 Shop Popups — ShopPremiumUIBuilder.cs

| Línea | GO Name | Texto | Contexto |
|-------|---------|-------|----------|
| 1753 | `Title` | "Confirm Purchase" | Purchase popup title |
| 1860 | `Title` | "Insufficient DigitGems" | Not-enough popup title |
| 1874 | `Message` | "You don't have enough DigitGems..." | Not-enough popup message |
| 1893 | `Text` (GetGemsButton child) | "Get DigitGems" | Button text |
| 1377/1523/1662 | `Status` | "IN USE" | Theme/frame/title status badge |
| 451 | `Title` | "STARTER PACK" | Banner title |
| 465 | `Contents` | "500 DigitGems + Exclusive Theme + Avatar" | Banner contents |
| 807 | `Title` | "BUNDLE PREMIUM" | Bundle title |
| 821 | `Description` | "50 levels of exclusive rewards" | Bundle description |

### 4.2 Profile — ProfileUIBuilder.cs

| Línea | GO Name | Texto | Contexto |
|-------|---------|-------|----------|
| 870 | `Text` (ChallengeButton child) | "CHALLENGE" | Challenge button |
| 932 | `Title` (GameSelectionPanel child) | "CHOOSE A GAME" | Game selector popup title |
| 985 | `Text` (Cancel button child) | "Cancel" | Cancel button in popup |

### 4.3 CashBattle Onboarding — CashBattleOnboardingUIBuilder.cs

| Línea | GO Name | Texto | Contexto |
|-------|---------|-------|----------|
| 253 | `TitleLabel` | "CASH BATTLE" | Onboarding title |
| 294 | `Text` (SkipButton child) | "SKIP" | Skip button |
| 604 | `Text` (BackButton child) | "BACK" | Back button |
| 634 | `Text` (NextButton child) | "NEXT" | Next button |
| 663 | `LegalText` | "Powered by Triumph... 18+ only" | Legal text |

**Nota**: CashBattle Onboarding es territorio Triumph SDK.

### 4.4 Daily Missions — DailyMissionsUIBuilder.cs

| Línea | GO Name | Texto | Contexto |
|-------|---------|-------|----------|
| 579 | `Label` | "Resets in:" | Timer label |
| 724 | `TitleLeft` | "Daily Progress" | Section title |
| 1243 | `Text` (ActionButton child) | "Claim" | Claim button |
| 1256 | `Text` (ActionButton child) | "In Progress" | Status text |
| 1325 | `Title` (RewardClaimPopup child) | "Mission Completed!" | Popup title |
| 1457 | `Text` (CollectButton child) | "Collect" | Collect button |

### 4.5 Daily Rewards — DailyRewardsPremiumUIBuilder.cs

| Línea | GO Name | Texto | Contexto |
|-------|---------|-------|----------|
| 859 | `DayLabel` | "DAY 7 - GRAND PRIZE" | Grand prize day label |
| 879 | `Reward2` | "+ Exclusive Item" | Reward description |
| 954 | `Text` (ClaimButton child) | "CLAIM REWARD" | Claim button |
| 1014 | `Label` | "Next reward in:" | Timer label |
| 1219 | `Text` (TapToContinueButton child) | "TAP TO CONTINUE" | Button text |
| 1310 | `Text` (ContinueButton child) | "CONTINUE" | Milestone popup button |

### 4.6 Achievements — AchievementsUIBuilder.cs

| Línea | GO Name | Texto | Contexto |
|-------|---------|-------|----------|
| 496 | `CategoryDropdownLabel` | "All" | Dropdown label |
| 1398 | `Text` (ContinueButton child) | "CONTINUE" | Button text |

### 4.7 Notifications — NotificationsUIBuilder.cs

| Línea | GO Name | Texto | Contexto |
|-------|---------|-------|----------|
| 456 | `Text` (MarkAllReadButton child) | "Mark all as read" | Button text |

### 4.8 Friends — FriendsUIBuilder.cs

| Línea | GO Name | Texto | Contexto |
|-------|---------|-------|----------|
| 356 | `Label` (FriendRequestsNav child) | "Friend requests" | Navigation label |

### 4.9 Tournament Lobby — TournamentLobbyUIBuilder.cs

| Línea | GO Name | Texto | Contexto |
|-------|---------|-------|----------|
| 1323 | `Text` (ShareButton child) | "Share" | Button text |
| 1346 | `Text` (LeaveButton child) | "Leave" | Button text |

### 4.10 Tournaments Browser — TournamentsBrowserUIBuilder.cs

| Línea | GO Name | Texto | Contexto |
|-------|---------|-------|----------|
| 481 | `Text` (FiltersButton child) | "Filters" | Button text |
| 646 | `Text` (CreateTournamentButton child) | "Create Tournament" | Button text |
| 912 | `Text` (LoadMoreButton child) | "Load More" | Button text |
| 1002 | `Text` (CreateTournamentButton child) | "Create Tournament" | Duplicate |

---

## SECCIÓN 5 — MEDIUM/LOW PRIORITY

### 5.1 PremiumCard.cs — Timer abbreviations

| Línea | String | Contexto |
|-------|--------|----------|
| 213 | `"{days}d {hours}h"` | Premium timer countdown |

### 5.2 CashTournamentsManager.cs — Fallback placeholders

| Línea | String | Contexto |
|-------|--------|----------|
| 522 | `"Prize: $0"` | Initial placeholder (overwritten) |
| 525 | `"Entry: $0"` | Initial placeholder (overwritten) |
| 528 | `"0/0 players"` | "players" text |

---

## PLAN DE ACCIÓN RECOMENDADO

### Prioridad 1 — BLOQUEANTE (App Store rejection risk)
1. **12 Spanish strings** en NotificationStorageService + MatchHistoryData → AutoLocalizer.Get()
2. **20 player title keys** faltantes en Translations.txt → Agregar con 5 idiomas
3. **12 theme name keys** faltantes en Translations.txt → Agregar con 5 idiomas
4. **4 match result labels** (WIN/LOSS/DRAW/PRACT.) → AutoLocalizer.Get()

### Prioridad 2 — ALTO
5. **FloatingText** (COMBO, PERFECT, EXCELLENT, NEW RECORD) → AutoLocalizer.Get()
6. **WalletData** dates → AutoLocalizer.Get()
7. **CashProfile** stats labels (W/L/D, Win Rate) → AutoLocalizer.Get()
8. **CashBattle1v1** "Entry:" label → AutoLocalizer.Get()

### Prioridad 3 — MEDIO
9. **~45 GO names genéricos** en UIBuilders → Rename + TextNameToKeyMap entries
10. **Shop popups** (Confirm Purchase, Insufficient Gems, etc.)
11. **DailyMissions/DailyRewards** button texts y labels
12. **SearchPlayersManager** "WR" label, **ShopItemUI** "BONUS"

### Prioridad 4 — BAJO (Triumph SDK / minor)
13. CashBattle Onboarding slides (37 textos) — Triumph SDK territory
14. PremiumCard timer abbreviations
15. CashTournamentsManager fallback placeholders

---

## ARCHIVOS AFECTADOS

| Archivo | Issues | Prioridad |
|---------|--------|-----------|
| NotificationStorageService.cs | 8 Spanish | P1 |
| MatchHistoryData.cs | 4 Spanish + 4 English | P1 |
| PlayerTitleService.cs | 20 keys faltantes | P1 |
| SettingsManager.cs (themes) | 12 keys faltantes | P1 |
| WalletData.cs | 4 English dates | P2 |
| CashProfileSceneController.cs | 4 English stats | P2 |
| FloatingText.cs | 4 English effects | P2 |
| ComboVisualController.cs | 1 English | P2 |
| CashBattle1v1Manager.cs | 1 English | P2 |
| SearchPlayersManager.cs | 1 English | P3 |
| ShopItemUI.cs | 1 English | P3 |
| ShopPremiumUIBuilder.cs | 9 GO genéricos | P3 |
| ProfileUIBuilder.cs | 3 GO genéricos | P3 |
| DailyMissionsUIBuilder.cs | 6 GO genéricos | P3 |
| DailyRewardsPremiumUIBuilder.cs | 6 GO genéricos | P3 |
| AchievementsUIBuilder.cs | 2 GO genéricos | P3 |
| NotificationsUIBuilder.cs | 1 GO genérico | P3 |
| FriendsUIBuilder.cs | 1 GO genérico | P3 |
| TournamentLobbyUIBuilder.cs | 2 GO genéricos | P3 |
| TournamentsBrowserUIBuilder.cs | 4 GO genéricos | P3 |
| CashBattleOnboardingUIBuilder.cs | 5 GO genéricos | P4 |
