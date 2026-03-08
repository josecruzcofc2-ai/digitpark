# VISUAL AUDIT - DigitPark App Store Readiness
**Date**: 2026-03-07
**Screenshots**: 215 (in C:\Users\josec\OneDrive\Pictures\Screenshots)
**Status**: AUDIT COMPLETE - FIXES PENDING

---

## AUDIT CRITERIA (Checklist per scene)

### Text & Typography
- [ ] ALL text must be BOLD
- [ ] All text < 20px increased (barely visible on mobile)
- [ ] Auto-sizing enabled on ALL text (prevent clipping after AutoLocalizer translation)
- [ ] No text cut off, overflowing, or escaping panels
- [ ] No emoji or text-as-icon (Unity doesn't render emoji)

### Localization
- [ ] AutoLocalizer.Get() on ALL runtime strings (except pure numbers)
- [ ] English base state for all text
- [ ] TextNameToKeyMap entries exist for all GO text names

### Layout & Spacing
- [ ] ZERO overlapping elements (critical - even 1px overlap is unacceptable)
- [ ] No text too close to edges or other elements
- [ ] Scene uses 100% screen space (exceptions: scrollable scenes that fill on device, minigame scenes allowed dead space - player is focused on gameplay)
- [ ] All elements inside their parent panels (nothing outside bounds)

### Headers & Navigation
- [ ] Headers consistent: same Pos Y across all scenes
- [ ] TitleText same size across all scenes (except Auth which has different sizes)
- [ ] BackButton prefab present where needed (check screenshots for presence)
- [ ] CurrencyPills: Shop-style green + squares (not blue/yellow), same proportions everywhere (MainMenu exception)

### Backgrounds & Theme
- [ ] Same background across ALL scenes (except CashBattle zone)
- [ ] CashBattle scenes have distinct background (real money distinction)
- [ ] AgeVerification counts as CashBattle-style background
- [ ] OddOneOut/QuickMath multiple backgrounds: leave for now (future standardization)

### Icons
- [ ] All icons must be MINIMALIST style (like top apps)
- [ ] No detailed/realistic icons - replace or remove
- [ ] Icons consistent in size and style across scenes

### Cross-Scene Consistency
- [ ] Loading indicators identical across all scenes (size, text, style)
- [ ] Error panels identical style
- [ ] Empty states identical style
- [ ] Confirm/blocker dialogs identical style
- [ ] Shop divider texts x2 bigger + fix overlap

### Logic & Code
- [ ] No logic errors in scene setup
- [ ] No values that could corrupt at runtime
- [ ] Remove unused/orphan GameObjects
- [ ] No potential runtime errors
- [ ] Minimize warnings

### App Store Blockers
- [ ] No broken sprites or missing assets
- [ ] No placeholder text visible to users
- [ ] No debug/test content
- [ ] Professional polish on every visible element

---

## PRE-AUDIT NOTES (User observations)

### Objects to DELETE from project:
1. **MainMenu > NotificationsPanel** - Dedicated Notifications scene exists
2. **DigitRush > ResultPanel** - Obsolete, global panels replaced it
3. **ALL minigame per-scene Win/Lose panels** (Normal + RealMoney) in DigitRush, FlashTap, MemoryPairs, OddOneOut, QuickMath - Global panels exist
4. **MemoryPairs > PlayAgainButton** - Global panel handles this
5. **Achievements > EmptyStateContainer** - All categories have achievements, no empty state needed
6. **TournamentLobby > LeaveBlocker** - Can't leave once entered (verify warning shown BEFORE entering, like CashTournament)

### Objects needing REDESIGN:
7. **AgeVerification > LoadingIndicator** - Broken yellow square sprite. Investigate if animated. If not, redesign: darken screen + large centered "Loading..." text (professional loading)
8. **Login > LoadingIndicator** - Same broken sprite issue
9. **CashHistory > DetailPanel** - Doesn't look like a panel, needs full redesign
10. **Achievements hidden objects** - Need redesign (DetailPanelBlocker, RewardCelebration)

### Objects with NO VISIBLE EFFECT in editor (investigate runtime behavior):
11. **CashHistory > LoadingIndicator** - No visible change
12. **CashHistory > TransactionHistoryPanel** - No visible change
13. **CashWallet > ErrorOverlay** - Only black screen
14. **FlashTap > FeedbackPanel** - No visible change
15. **OddOneOut > ComboContainer** - No visible change
16. **OddOneOut > FeedbackPanel** - No visible change
17. **QuickMath > ComboContainer** - No visible change
18. **SearchPlayers > SearchButton** - No visible change when activated
19. **DailyRewards > ClaimAnimationBlocker** - Blocks clicks in scene

### Previously audited categories (mostly minor remaining):
- Core, Auth, Games, Monetization, Social, CashBattle
- Focus harder on NON-audited: Onboarding, Tournaments

### Special rules:
- CurrencyPills: change + square colors to GREEN (like Shop), not blue(gems)/yellow(coins)
- Shop divider section texts: make x2 bigger, fix pixel overlap
- All TitleText: same font size (except Auth scenes)
- Backgrounds: standardize (except CashBattle zone + AgeVerification)

---

## FINDINGS BY CATEGORY

### 00_Core (14 screenshots)
**Scenes**: Boot, MainMenu, Settings

#### 00_Core_Boot
*(Runtime-only scene, no UIBuilder)*

| # | Severity | Issue | Fix |
|---|----------|-------|-----|
| B1 | **P0-BLOCKER** | "DBG" debug label visible on left side of screen | Remove debug overlay from Boot scene - App Store rejection risk |
| B2 | P1 | "Completado!" hardcoded in Spanish | Must use AutoLocalizer.Get("loading_complete") with English base "Completed!" |
| B3 | P2 | "ARCADE EXPERIENCE" / "TRAIN YOUR MIND" | Verify these use AutoLocalizer (may be image-based, in which case OK) |

#### 00_Core_MainMenu
**Screenshots analyzed**: runtime, PremiumPanel_context, PremiumPanel_detail

| # | Severity | Issue | Fix |
|---|----------|-------|-----|
| M1 | **P0-BLOCKER** | "DBG" debug label visible on left side | Remove debug overlay - App Store rejection risk |
| M2 | P1 | CurrencyPills + squares: gems=blue, coins=yellow | Change BOTH to GREEN like Shop style |
| M3 | P1 | PremiumPanel when enabled shows empty dark area with only tab bar | Investigate: is this populated at runtime? If not, needs content or removal |
| M4 | P2 | CashBattle card icon (swords+coin) is detailed/realistic | Evaluate simplification to minimalist style |
| M5 | P2 | Daily Reward banner mixes "Day 3 of 7 - Claim your reward!" with "Reclamar" | Verify AutoLocalizer consistency on subtitle text |
| M6 | P3 | Bottom row cards (Logros/Tienda/Premium) slightly clipped at very bottom edge | Verify SafeArea padding on smaller devices |
| M7 | P2 | NotificationsPanel still exists in hierarchy (greyed out) | DELETE - dedicated Notifications scene exists (per user) |

#### 00_Core_Settings
**Screenshots analyzed**: runtime_1of2, runtime_2of2, LogoutConfirmPanel, DeleteAccountPanel, SelfExclusionPanel, ThemeDropdown(=ChangeNamePanel)

| # | Severity | Issue | Fix |
|---|----------|-------|-----|
| S1 | **P0-BLOCKER** | "DBG" debug label visible on left side | Remove debug overlay |
| S2 | P1 | Screenshot "ThemeDropdown" actually shows ChangeNamePanel - MISLABELED | Rename files: ThemeDropdown_context/detail -> ChangeNamePanel_context/detail |
| S3 | P2 | Missing accents in Spanish translations: "Terminos" should be "Términos", "Politica" should be "Política" | Fix in Translations.txt |
| S4 | P2 | "Cancel" button text in LogoutConfirmPanel appears lighter/thinner than "Confirm" | Ensure both buttons use same bold font weight |
| S5 | P3 | Section divider lines between items are very subtle | Acceptable but could be more visible |
| S6 | P2 | No CurrencyPills in Settings header (only "SETTINGS" + back button) | Verify: intentional design choice? Other non-game scenes have pills |
| S7 | P2 | Settings scrollable content: adequate spacing between sections | OK - no issues |
| S8 | P2 | All 3 confirm panels (Logout/Delete/SelfExclusion) use consistent ConfirmPanelUI design | GOOD - consistent |
| S9 | P2 | ChangeNamePanel uses cyan Confirm (non-destructive) vs red for danger actions | GOOD - correct color coding |
| S10 | P3 | ">" arrows for navigation items are small | Acceptable for mobile |

**00_Core Summary**: 3 P0 blockers (all same "DBG" debug label), 3 P1 issues, 6 P2, 2 P3

---

### 01_Auth (8 screenshots)
**Scenes**: AgeVerification, Login, Register

#### 01_Auth_AgeVerification
**Screenshots**: runtime, LoadingIndicator_context, LoadingIndicator_detail

| # | Severity | Issue | Fix |
|---|----------|-------|-----|
| AV1 | **P0-BLOCKER** | LoadingIndicator is a 60x60 yellow/orange square (WhiteSquare sprite tinted) - broken/unprofessional | Redesign: fullscreen semi-dark overlay + centered spinner + "Verifying..." text (professional loading) |
| AV2 | P1 | 18+ shield icon is detailed/realistic (3D-style with glow) | Replace with minimalist flat 18+ badge |
| AV3 | P2 | Large dead space between "Digit Park" logo and progress bar | Acceptable for Auth scene (special layout) but could tighten |
| AV4 | P2 | "Powered by Triumph" text at bottom is very small/faint | Ensure font size >= 20px and auto-sizing enabled |
| AV5 | P2 | Background is dark/black - should match CashBattle style (this is a real-money gate) | Verify background matches CashBattle zone theme |

#### 01_Auth_Login
**Screenshots**: runtime, editor, LoadingIndicator_context, LoadingIndicator_detail

| # | Severity | Issue | Fix |
|---|----------|-------|-----|
| L1 | **P0-BLOCKER** | LoadingIndicator same broken sprite - cyan WhiteSquare 60x60 + "Loading..." text overlapping checkbox area | Redesign: fullscreen semi-dark overlay + spinner + "Signing in..." text |
| L2 | P1 | "Remember me" checkbox + label very small, hard to tap on mobile | Increase checkbox size and label font |
| L3 | P2 | "or" divider between Sign In button and social logins is tiny text | Increase font size, ensure auto-sizing |
| L4 | P2 | "Don't have an account?" text is small/faint | Ensure >= 20px |
| L5 | P3 | "Forgot your password?" link is small cyan text | Acceptable but verify touch target size |
| L6 | P2 | Eye icon for password visibility toggle is very small | Increase size for better tap target |
| L7 | P2 | Google icon in "Sign in with Google" button visible, Apple button has no icon in runtime | Verify Apple icon loads correctly at runtime |

#### 01_Auth_Register
**Screenshots**: runtime (detail + context with LoadingPanel)

| # | Severity | Issue | Fix |
|---|----------|-------|-----|
| R1 | **P0-BLOCKER** | LoadingPanel same issue: cyan WhiteSquare overlapping "Confirm Password" field + "Creating account..." text | Redesign: same professional loading overlay as Login/AgeVerification |
| R2 | P1 | Massive dead space below "Create Account" button (60%+ of screen empty) | Add "Already have an account? Sign In" link, or Terms/Privacy links, or shrink spacing |
| R3 | P2 | No "Back to Login" option visible in runtime screenshot | Verify BackButton prefab is functional (visible in hierarchy) |
| R4 | P2 | Password eye toggle icons small | Same fix as Login L6 |
| R5 | P3 | Input field placeholder text ("Username", "Email", etc.) styling consistent | OK - consistent across fields |

**01_Auth Summary**: 3 P0 blockers (all broken LoadingIndicator sprites), 3 P1, 9 P2, 2 P3
**Cross-Auth note**: All 3 Auth LoadingIndicators use WhiteSquare sprite as placeholder - need unified professional loading overlay component

---

### 02_Games (35 screenshots)
**Scenes**: DigitRush, FlashTap, MemoryPairs, OddOneOut, QuickMath, GameSelector, BetSelection, Matchmaking, PlayModeSelection

#### 02_Games_Minigames_DigitRush
**Screenshots**: runtime

| # | Severity | Issue | Fix |
|---|----------|-------|-----|
| DR1 | P2 | Dead space below game grid area (~30% of screen) | ACCEPTABLE - minigame exception per user rule |
| DR2 | P2 | Header bar (game name + timer + round + errors) consistent with other minigames | OK - GOOD consistency |
| DR3 | P3 | Number grid cells could be slightly larger for fat-finger tapping | Verify touch target size >= 44pt on device |

#### 02_Games_Minigames_FlashTap
**Screenshots**: runtime, FeedbackPanel_context

| # | Severity | Issue | Fix |
|---|----------|-------|-----|
| FT1 | P1 | Large orange 3D-style button is detailed/realistic (glossy gradient, shadow) - NOT minimalist | Replace with flat minimalist tap button (solid color, no 3D effect) |
| FT2 | P2 | FeedbackPanel hidden object: no visible change when activated in editor | Investigate runtime behavior - may be correct/transparent flash |
| FT3 | P2 | Dead space around tap button area | ACCEPTABLE - minigame exception |
| FT4 | **P1-DELETE** | Hierarchy still contains WinPanel_Normal, LosePanel_Normal, WinPanel_RealMoney, LosePanel_RealMoney | DELETE all 4 - global panels replaced these (per user pre-audit note #3) |

#### 02_Games_Minigames_MemoryPairs
**Screenshots**: runtime, FeedbackPanel_context, FeedbackPanel_detail

| # | Severity | Issue | Fix |
|---|----------|-------|-----|
| MP1 | P2 | FeedbackPanel: shows small centered panel with "Correct!" / "Wrong!" text - functional | Verify font is bold and >= 20px |
| MP2 | P2 | Card grid well-spaced, consistent sizing | OK - GOOD |
| MP3 | P2 | Dead space below card grid | ACCEPTABLE - minigame exception |
| MP4 | **P1-DELETE** | PlayAgainButton exists in hierarchy | DELETE - global panel handles replay (per user pre-audit note #4) |
| MP5 | **P1-DELETE** | Hierarchy contains WinPanel_Normal, LosePanel_Normal, WinPanel_RealMoney, LosePanel_RealMoney | DELETE all 4 - global panels replaced these (per user pre-audit note #3) |

#### 02_Games_Minigames_OddOneOut
**Screenshots**: runtime, Backgrounds_context, ComboContainer_context+detail, CountdownPanel_context+detail, FeedbackPanel_context+detail

| # | Severity | Issue | Fix |
|---|----------|-------|-----|
| OO1 | P2 | Multiple background variants visible in Backgrounds_context | Leave for now - future standardization (per user pre-audit note) |
| OO2 | P2 | ComboContainer: hidden object, no visible change when activated | Investigate runtime behavior - likely shows combo streak counter |
| OO3 | P2 | CountdownPanel: shows large centered countdown number (3, 2, 1) - functional | Verify countdown text is bold |
| OO4 | P2 | FeedbackPanel: hidden object, no visible change in editor | Investigate runtime behavior (per user pre-audit note #16) |
| OO5 | P2 | Dead space around grid area | ACCEPTABLE - minigame exception |
| OO6 | **P1-DELETE** | Hierarchy contains WinPanel_Normal, LosePanel_Normal, WinPanel_RealMoney, LosePanel_RealMoney | DELETE all 4 - global panels replaced these (per user pre-audit note #3) |

#### 02_Games_Minigames_QuickMath
**Screenshots**: runtime, Backgrounds_context, ComboContainer_context, FeedbackPanel_context+detail, SettingsPanel_context+detail

| # | Severity | Issue | Fix |
|---|----------|-------|-----|
| QM1 | P2 | Multiple background variants in Backgrounds_context | Leave for now - future standardization (per user pre-audit note) |
| QM2 | P2 | ComboContainer: hidden, no visible change (per user pre-audit note #17) | Investigate runtime behavior |
| QM3 | P2 | FeedbackPanel: shows feedback text - functional | Verify bold + size |
| QM4 | **P0-BLOCKER** | SettingsPanel: "NOR..." truncated text for "NORMAL" difficulty | Fix: enable auto-sizing on difficulty label OR increase width to fit "NORMAL" fully |
| QM5 | P2 | SettingsPanel: rounds selector shows +/- buttons, functional | OK |
| QM6 | **P1-DELETE** | Hierarchy contains WinPanel_Normal, LosePanel_Normal, WinPanel_RealMoney, LosePanel_RealMoney | DELETE all 4 - global panels replaced these (per user pre-audit note #3) |

#### 02_Games_Navigation_GameSelector
**Screenshots**: runtime, CognitiveSprintPanel_context+detail, RulesPanel_context+detail

| # | Severity | Issue | Fix |
|---|----------|-------|-----|
| GS1 | P1 | All 6 game card icons are detailed neon-style illustrations (realistic gradients, glows, complex shapes) - NOT minimalist | Replace all 6 with flat minimalist game icons matching top-app style |
| GS2 | P1 | CognitiveSprintPanel: "Select 3-5 games for the sprint" subtitle text appears to overlap/crowd with title text | Fix spacing between title and subtitle, increase vertical gap |
| GS3 | **P0-BLOCKER** | RulesPanel: shows placeholder content "Rule 1", "Rule 2", "Rule 3" - clearly placeholder/debug text | Replace with actual game rules OR populate dynamically at runtime. App Store rejection risk |
| GS4 | P1 | RulesPanel: "Game Rules" subtitle overlaps with title "DIGIT RUSH" | Fix vertical spacing between title and subtitle |
| GS5 | P2 | Game cards are scrollable, good spacing between cards | OK - GOOD |
| GS6 | P2 | CognitiveSprintPanel: game selection checkboxes are functional | Verify checkbox tap targets >= 44pt |

#### 02_Games_Navigation_BetSelection
**Screenshots**: editor_1of2, editor_2of2

| # | Severity | Issue | Fix |
|---|----------|-------|-----|
| BS1 | P2 | Shows raw AutoLocalizer key placeholders as text: "bet_title", "bet_coins_cost", "bet_gems_wager", "bet_free", "bet_free_desc", "bet_custom_reward" | These are English base placeholders translated at runtime - verify all keys exist in Translations.txt and render correctly |
| BS2 | P2 | Editor-only screenshots (no runtime) - cannot verify final appearance | Capture runtime screenshot to verify layout and translations |
| BS3 | P2 | Bet option cards layout appears functional with coin/gem icons | Verify icons are minimalist style |
| BS4 | P2 | CurrencyPills visible in header | Verify GREEN + square style (not blue/yellow) |

#### 02_Games_Navigation_PlayModeSelection
**Screenshots**: runtime

| # | Severity | Issue | Fix |
|---|----------|-------|-----|
| PM1 | P1 | Brain icon (Solo Practice) is detailed 3D-style with glow effects - NOT minimalist | Replace with flat minimalist brain outline icon |
| PM2 | P1 | Swords icon (1v1 Ranked) is detailed 3D-style with glow - NOT minimalist | Replace with flat minimalist crossed swords icon |
| PM3 | P2 | Trophy icon (Tournaments) is outline/minimalist style - INCONSISTENT with other two | Good style but inconsistent - make all 3 match (all minimalist) |
| PM4 | P2 | Layout spacing between 3 mode cards is good | OK |
| PM5 | P2 | CurrencyPills visible in header | Verify GREEN + square style |

#### 02_Games_Navigation_Matchmaking
**Screenshots**: runtime, VSContainer_context+detail, CountdownPanel_context+detail, ScreenFlash_context

| # | Severity | Issue | Fix |
|---|----------|-------|-----|
| MM1 | P2 | Runtime: Professional matchmaking screen with opponent "???" placeholder | OK - standard matchmaking UX |
| MM2 | P2 | VSContainer: Shows "VS" text between two player cards - well designed | Verify VS text is bold |
| MM3 | P2 | CountdownPanel: Large centered countdown number, clean design | OK - consistent with OddOneOut countdown |
| MM4 | P2 | ScreenFlash: White flash overlay, no visible UI issues | OK - transition effect |
| MM5 | P2 | Player avatar icons are circular with border | OK - consistent style |
| MM6 | P2 | CurrencyPills in header | Verify GREEN + square style |

**02_Games Summary**: 2 P0 blockers (QuickMath truncated "NOR...", GameSelector placeholder rules), 8 P1 (detailed icons x5, overlapping text x2, delete panels), 5 P1-DELETE (win/lose panels in all 5 minigames + PlayAgainButton), 25 P2, 1 P3
**Cross-Games note**: All 5 minigames still have obsolete per-scene Win/Lose panels (Normal + RealMoney) that must be deleted. All navigation icons need minimalist redesign.

---

### 03_CashBattle (65 screenshots)
**Scenes**: CashBattleHub, CashBattle1v1, CashMatchmaking, CashHistory, CashProfile, CashWallet, CashTournaments, CashTournamentCreate, CashTournamentLobby

**CRITICAL NOTE**: ~20 hidden-object screenshots in this category are MISLABELED (wrong scene name in filename). The batch rename assigned screenshots to wrong scenes. Each mislabeled file is noted below with its ACTUAL content.

#### 03_CashBattle_CashBattleHub
**Screenshots**: runtime, WinPanel(=ConfirmBetPanel)_context+detail, LosePanel(=MatchmakingPanel)_context+detail

| # | Severity | Issue | Fix |
|---|----------|-------|-----|
| CBH1 | P1 | All 5 menu card icons (swords, trophy, wallet, profile, history) are detailed/realistic 3D style - NOT minimalist | Replace with flat minimalist icons matching top-app style |
| CBH2 | P2 | Subtitle text on cards ("Challenge other players in real time", etc.) is small/faint | Verify font size >= 20px and bold |
| CBH3 | P2 | No BackButton visible in header (only "Cash Battle" title + $0.00 pill) | Verify back navigation exists (may be system back only) |
| CBH4 | P2 | ConfirmBetPanel (MISLABELED as CashBattle1v1_WinPanel): Small dialog "Bet $0.00 on DigitRush?" with Cancel/Confirm buttons | Dialog is functional but small - verify touch targets |
| CBH5 | **P0-BLOCKER** | MatchmakingPanel (MISLABELED as CashBattle1v1_LosePanel): Uses YELLOW SQUARE as spinner (WhiteSquare sprite tinted) - same broken loading as Auth | Redesign: use proper spinner animation or "Searching..." text with professional overlay |
| CBH6 | P2 | MatchmakingPanel: "Searching for opponent..." + "00:00" timer + "Cancel" button - functional layout | OK but spinner must be fixed (CBH5) |
| CBH7 | P2 | Background is dark/distinct from main app - CORRECT for CashBattle zone | OK - GOOD |

#### 03_CashBattle_CashBattle1v1
**Screenshots**: runtime, SettingsPanel(=GameSelectionModal)_context+detail, CognitiveSprintPanel_context+detail
**MISLABELED files**: ComboContainer(=CashHistory runtime), CountdownPanel(=CashHistory LoadingIndicator), FeedbackPanel(=CashHistory DetailPanel), WinPanel(=CashBattleHub ConfirmBetPanel), LosePanel(=CashBattleHub MatchmakingPanel)

| # | Severity | Issue | Fix |
|---|----------|-------|-----|
| CB1v1_1 | P2 | Runtime: Clean bet selection layout with game dropdown, bet amounts, "Find opponent" button | OK - GOOD layout |
| CB1v1_2 | P1 | GameSelectionModal: Game names severely truncated - "Digit R...", "Flash T...", "Memor...", "Odd O...", "Quick ...", "Cognitiv" | Enable auto-sizing on game name labels OR increase card width to fit full names |
| CB1v1_3 | P2 | GameSelectionModal: Game description text also truncated ("Type numbers a...", "Tap the correct tar...") | Enable auto-sizing or increase card height for descriptions |
| CB1v1_4 | P2 | GameSelectionModal: Game card icons are detailed neon style - NOT minimalist | Replace with minimalist game icons (same fix as GameSelector GS1) |
| CB1v1_5 | P2 | GameSelectionModal: Large empty space between game cards grid and "Confirm" button | Reduce padding or make cards larger |
| CB1v1_6 | P2 | CognitiveSprintPanel: Clean layout with game list, checkboxes, "Selected: 0/5 (min: 2)", CANCEL/ACCEPT | OK - GOOD |
| CB1v1_7 | P2 | CognitiveSprintPanel: "PRO" badge on Cognitive Sprint option partially overlaps text | Verify badge positioning doesn't clip text |
| CB1v1_8 | P2 | Bet preset buttons ($1, $5, $10, $25, $50, $100) are small and tightly spaced | Verify touch targets >= 44pt on device |
| CB1v1_9 | P2 | "OK" button for custom amount is cyan (inconsistent with other button colors in scene) | Consider matching to scene color scheme |
| CB1v1_10 | P2 | "If you win you receive" panel shows "$0.00" in large yellow text - functional | OK |

#### 03_CashBattle_CashHistory
**Screenshots**: runtime, LoadingIndicator_context, DetailPanel(=CashMatchmaking)_context, TransactionHistoryPanel(=CashMatchmaking ScreenFlash)_context+detail
**MISLABELED files**: DetailPanel_context+detail show CashMatchmaking, TransactionHistoryPanel shows CashMatchmaking ScreenFlash

| # | Severity | Issue | Fix |
|---|----------|-------|-----|
| CH1 | P2 | Runtime: Professional match history with colored indicators (green=victory, pink/red=defeat) | OK - GOOD |
| CH2 | P1 | Game icons in history cards (DigitRush, QuickMath, FlashTap) are detailed neon illustrations - NOT minimalist | Replace with minimalist game icons |
| CH3 | P2 | "1v1" and "Tournament" badges on cards use different colors (cyan vs orange) - intentional distinction | OK - GOOD color coding |
| CH4 | P2 | "Load More" button at bottom is subtle/small | Verify visibility and touch target |
| CH5 | P2 | Filter tabs (All / Victories / Defeats) functional | OK |
| CH6 | P2 | LoadingIndicator: No visible change when activated in editor (per user pre-audit note #11) | Investigate runtime behavior |
| CH7 | **P1-REDESIGN** | DetailPanel (visible in CashBattle1v1_FeedbackPanel screenshots): Shows "Title", "Subtitle" placeholder labels + "VICTORY" + stats. Does NOT look like a proper panel - just floating text on semi-dark overlay | Full redesign needed: proper panel background, remove placeholder labels, professional card layout (per user pre-audit note #9) |

#### 03_CashBattle_CashMatchmaking
**Screenshots**: runtime, CountdownPanel(=CashProfile ChangeNamePanel)_context, ScreenFlash(=CashProfile ErrorPanel)_context+detail
**MISLABELED files**: CountdownPanel shows CashProfile ChangeNamePanel, ScreenFlash shows CashProfile ErrorPanel

| # | Severity | Issue | Fix |
|---|----------|-------|-----|
| CMM1 | P2 | Runtime: Professional matchmaking screen - game icon, "DIGIT RUSH", "Entry: $0.00", "SEARCHING...", player cards, VS icon, timer, CANCEL button | OK - GOOD |
| CMM2 | P1 | VS icon between player cards is detailed/ornate (fire/glow effect) - NOT minimalist | Replace with simple flat "VS" text or minimalist icon |
| CMM3 | P2 | Opponent card shows "???" with grey avatar and "---" level badge | OK - standard placeholder |
| CMM4 | P2 | Player card has yellow border, opponent has grey border - good visual distinction | OK - GOOD |
| CMM5 | P2 | CountdownPanel (visible in CashHistory_LoadingIndicator screenshot): "GET READY!" text with countdown "3" overlapping "Lv. 1" badge on player card | Fix: ensure countdown number doesn't overlap player card elements |
| CMM6 | P2 | ScreenFlash: White flash fullscreen overlay - transition effect | OK - investigate if too bright |

#### 03_CashBattle_CashProfile
**Screenshots**: runtime, ChangeNamePanel(=CashWallet)_context+detail, GameSelectionPanel(=CashWallet BonusBalance)_context+detail, ErrorPanel(in CashMatchmaking_ScreenFlash screenshots)
**MISLABELED files**: ChangeNamePanel shows CashWallet, GameSelectionPanel shows CashWallet BonusBalance view

| # | Severity | Issue | Fix |
|---|----------|-------|-----|
| CP1 | P2 | Runtime: Professional profile layout - avatar, "Player", "Member since 2024", "Lv. 1 - Beginner" | OK - GOOD |
| CP2 | P2 | Overall Record section: "0W - 0L - 0D" with yellow progress bar and "0% Win Rate" | OK - functional |
| CP3 | P2 | Current Streak / Best Streak boxes with yellow borders | OK - consistent styling |
| CP4 | P2 | Stats by Game: All 5 games listed with colored progress bars and "-- | 0%" | OK - placeholder data for new user |
| CP5 | P1 | ErrorPanel (visible in CashMatchmaking_ScreenFlash detail): "Error" text overlaps with Quick Math stats bar + "Accept" button. Unpolished overlay | Redesign: fullscreen semi-dark overlay + centered error message + proper padding |
| CP6 | P2 | ChangeNamePanel (visible in CashMatchmaking_CountdownPanel detail): "Change Name" dialog with input field, Cancel/Save buttons. Save button is yellow. | OK - functional. Verify "Cancel" button text is bold |
| CP7 | P3 | Small pencil/edit icon next to "Player" name is subtle | Acceptable |

#### 03_CashBattle_CashWallet
**Screenshots**: runtime, DepositPanel_context+detail, WithdrawPanel_context+detail, ErrorOverlay_context+detail, KYCPanel(=SuccessOverlay mislabel)_context, LoadingOverlay(=SuccessOverlay mislabel)_detail, SuccessOverlay(actual, in ErrorOverlay_detail), LoadMoreButton(=PrizesBlocker mislabel)_context+detail
**MISLABELED files**: SuccessOverlay screenshots actually show KYCPanel and LoadingOverlay. PrizesBlocker screenshots show CashWallet LoadMoreButton.

| # | Severity | Issue | Fix |
|---|----------|-------|-----|
| CW1 | P2 | Runtime: Clean wallet layout - balance, weekly limit, DEPOSIT/WITHDRAW buttons, filter tabs, transaction history | OK - GOOD |
| CW2 | P2 | DEPOSIT/WITHDRAW buttons have money bag icons - detailed | Verify minimalist style |
| CW3 | P2 | Transaction history: First 2 items ("$10.00 Deposit", "$25.00 Deposit") lack date/details, while bottom 2 items ("Deposit via PayPal", "1v1 Battle Won") have full timestamps and status | Inconsistent item layout - standardize all history items to show date + status |
| CW4 | P2 | DepositPanel: "Select an amount" + "Choose the amount you want to deposit" + empty area + "Payment method" at bottom | Very empty panel - will Triumph SDK populate the amount options at runtime? If not, needs preset amounts |
| CW5 | P2 | WithdrawPanel: "Withdraw funds" + input field + "$0.00 available" + "Minimum: $10.00" + "Fee: $0.00" + WITHDRAW button | Clean layout, functional |
| CW6 | P1 | ErrorOverlay: Only shows dark screen overlay - no error message visible, no buttons | Redesign: add error message text + retry/dismiss button. Currently unusable (per user pre-audit note #13) |
| CW7 | **P0-BLOCKER** | KYCPanel (MISLABELED as SuccessOverlay_context): Nearly empty dark screen with ONLY a small "Verify Identity" green button centered - no title, no explanation, no context | Full redesign needed: add title "Identity Verification Required", explanation text, progress indicator, professional layout |
| CW8 | P2 | LoadingOverlay: Semi-dark overlay with "Processing..." text - functional | OK - verify text is centered and bold |
| CW9 | P2 | SuccessOverlay (visible in ErrorOverlay_detail): "Operation successful!" green text on dark overlay | OK - verify text is bold and >= 20px |
| CW10 | P2 | BonusBalanceText visible in some views: "+$0.00 bonus" next to weekly limit | OK - verify font size |
| CW11 | P2 | "Load more" button visible at bottom of transaction history | OK - verify touch target |

#### 03_CashBattle_CashTournaments
**Screenshots**: runtime, EmptyState_context+detail, FilterPanel(=CashTournamentLobby StartingOverlay)_context, LoadingIndicator(=PremiumBlockPanel)_context+detail

| # | Severity | Issue | Fix |
|---|----------|-------|-----|
| CT1 | P2 | Runtime: Clean tournament browser with filter tabs (All/Active/Completed), tournament cards, "Create Tournament" button | OK - GOOD |
| CT2 | P2 | Tournament cards show game name, prize pool, player count, entry fee, time, "Join" button | Functional layout |
| CT3 | **P0-BLOCKER** | EmptyState BUG: "No tournaments available" text is visible BELOW the existing tournament cards - both tournament items AND empty state shown simultaneously | Fix: EmptyState should only show when TournamentsList is empty. Hide EmptyState when cards exist |
| CT4 | P2 | "Join" buttons are bright green - good call-to-action visibility | OK - GOOD |
| CT5 | P2 | PremiumBlockPanel (MISLABELED as LoadingIndicator): "Premium Required" dialog with explanation text cut off ("reating tournaments requires a Premium subscription. Join existing tournaments for fre-") | Fix text overflow: enable auto-sizing or increase panel width. Full text is clipped |
| CT6 | P2 | PremiumBlockPanel: "Get Premium" yellow button + "Maybe Later" text link | OK - functional |
| CT7 | P2 | No balance pill in header (just "CASH TOURNAMENTS" title) | Verify: intentional for tournament browser? |

#### 03_CashBattle_CashTournamentCreate
**Screenshots**: runtime_1of2, runtime_2of2, LoadingOverlay(=LeaveBlocker mislabel)_context+detail

| # | Severity | Issue | Fix |
|---|----------|-------|-----|
| CTC1 | P2 | Runtime 1of2: Clean form - Tournament Name, Game dropdown, Entry Fee dropdown, Custom amount, Max Players, Estimated prize, Schedule, Start Immediately toggle | OK - GOOD |
| CTC2 | P2 | Runtime 2of2: Rules section (Rounds, Time Limit, Max Attempts dropdowns), Allow Spectators/Private Tournament toggles, Preview section, Creation fee, Create button | OK - GOOD |
| CTC3 | P2 | Entry Fee has a yellow square slider/handle that looks like WhiteSquare sprite | Verify if this is intentional slider or broken sprite (similar to Auth loading issue) |
| CTC4 | P2 | Preview section shows "--" for all fields (Name, Game, Entry Fee, Est. Prize, Max Players) | OK - placeholder for unfilled form, updates dynamically |
| CTC5 | P2 | LoadingOverlay (MISLABELED as LeaveBlocker): "Creating tournament..." text overlaps with "Custom..." entry fee text below | Fix: loading overlay should fully cover form content with proper dark overlay |
| CTC6 | P2 | "Creation fee: $5.00" text at bottom is small | Verify >= 20px and bold |
| CTC7 | P2 | No header balance pill visible | Verify: intentional? |

#### 03_CashBattle_CashTournamentLobby
**Screenshots**: runtime, ChatBadge_context+detail, LoadingOverlay(=actual, but labeled as StartingOverlay)_context+detail, StartingOverlay(in FilterPanel screenshots)_context+detail
**MISLABELED files**: PrizesBlocker shows CashWallet, LeaveBlocker shows CashTournamentCreate, StartingOverlay shows actual LoadingOverlay

| # | Severity | Issue | Fix |
|---|----------|-------|-----|
| CTL1 | P2 | Runtime: Professional lobby - tournament name, game info, prize pool, player count, progress bar, timer, prize distribution, participants/chat tabs, Play button | OK - GOOD |
| CTL2 | P2 | "OPEN" badge + share icon in header | OK - functional |
| CTL3 | P2 | Prize Distribution shows 1st/2nd/3rd with dollar amounts - clean layout | OK - GOOD |
| CTL4 | P2 | Participants tab: Shows #1 You (Ready) and #2 QuickFingers99 (Waiting) with scores and times | OK - GOOD |
| CTL5 | P2 | ChatBadge: Small red circle with "0" on Chat tab - badge is small (36x36) | Verify badge is visible on device |
| CTL6 | P2 | Chat view (in LoadingOverlay_context): Shows "Type a message..." input + "Send" button | OK - functional |
| CTL7 | P2 | LoadingOverlay: Semi-dark overlay with "Loading..." text centered | OK - professional |
| CTL8 | P2 | StartingOverlay (in FilterPanel screenshots): "Tournament Starting!" + countdown "3" + "Play" button. Text overlaps with lobby content behind | Verify overlay is dark enough to hide background content |
| CTL9 | P2 | LeaveBlocker noted by user as "can't leave once entered" - should show warning BEFORE entering | Verify warning dialog exists pre-entry (per user pre-audit note #6) |

#### 03_CashBattle SCREENSHOT MISLABELING SUMMARY
The following files need renaming to reflect actual content:

| Current Filename | Actually Shows |
|---|---|
| CashBattle1v1_ComboContainer | CashHistory runtime (LoadMoreButton) |
| CashBattle1v1_CountdownPanel | CashHistory LoadingIndicator |
| CashBattle1v1_FeedbackPanel | CashHistory DetailPanel |
| CashBattle1v1_WinPanel | CashBattleHub ConfirmBetPanel |
| CashBattle1v1_LosePanel | CashBattleHub MatchmakingPanel |
| CashHistory_DetailPanel | CashMatchmaking VSContainer |
| CashHistory_TransactionHistoryPanel | CashMatchmaking ScreenFlash |
| CashMatchmaking_CountdownPanel | CashProfile ChangeNamePanel |
| CashMatchmaking_ScreenFlash | CashProfile ErrorPanel |
| CashProfile_ChangeNamePanel | CashWallet (HistoryTabButton) |
| CashProfile_GameSelectionPanel | CashWallet (BonusBalanceText) |
| CashWallet_SuccessOverlay_context | CashWallet KYCPanel |
| CashWallet_SuccessOverlay_detail | CashWallet LoadingOverlay |
| CashTournaments_FilterPanel | CashTournamentLobby StartingOverlay |
| CashTournamentLobby_LeaveBlocker | CashTournamentCreate LoadingOverlay |
| CashTournamentLobby_PrizesBlocker | CashWallet LoadMoreButton |
| CashTournamentLobby_StartingOverlay | CashTournamentLobby LoadingOverlay |

**03_CashBattle Summary**: 2 P0 blockers (yellow square spinner in MatchmakingPanel, empty KYCPanel, EmptyState bug), 5 P1 (detailed icons, truncated names, ErrorPanel redesign, DetailPanel redesign), 40+ P2 consistency issues, 1 P3. PLUS 17 mislabeled screenshot files.

---

### 04_Monetization (29 screenshots)
**Scenes**: Achievements, DailyMissions, DailyRewards, Shop

#### 04_Monetization_Achievements
**Screenshots**: editor_1of4 through 4of4, DetailPanelBlocker_context+detail, RewardCelebration_context+detail

| # | Severity | Issue | Fix |
|---|----------|-------|-----|
| ACH1 | P1 | CurrencyPills in header: gems icon (blue) + coins icon (yellow/orange) - NOT GREEN | Change to GREEN + square style like Shop (per audit criteria) |
| ACH2 | P2 | Editor shows Spanish achievement names ("Primer Paso", "Aprendiz", "Primera Victoria", etc.) | Verify AutoLocalizer translates at runtime - these come from achievement data, may need translation keys |
| ACH3 | P2 | Achievement category dropdown shows "Todos" (Spanish) | Verify English base "All" is shown before AutoLocalizer kicks in |
| ACH4 | P2 | "Progreso Total 2/52 (4%)" text in Spanish | Verify AutoLocalizer handles this |
| ACH5 | **P0-BLOCKER** | DetailPanelBlocker: Large YELLOW RECTANGLE at top of popup where achievement icon should be | Broken sprite/placeholder - needs to load actual achievement icon dynamically |
| ACH6 | P2 | DetailPanelBlocker: "First Victory" title, description, progress bar, "CLAIM REWARD" + "CANCEL" - functional layout | OK - layout is clean |
| ACH7 | P1 | DetailPanelBlocker: Reward icon is a CYAN SQUARE (placeholder for gem icon) | Load proper gem sprite for reward display |
| ACH8 | **P0-BLOCKER** | RewardCelebration: Same YELLOW RECTANGLE as icon + "Achievement Unlocked!" + CYAN SQUARE for reward | Same broken sprites as DetailPanel - fix icon loading |
| ACH9 | P2 | RewardCelebration: "+50" reward text and "CONTINUE" button - functional | OK |
| ACH10 | P2 | Achievement grid: 3 columns, consistent sizing, some highlighted (completed) with "V" badge | OK - GOOD layout |
| ACH11 | P2 | Achievement icons are detailed/realistic style (3D illustrations) | These are custom achievement icons - may be acceptable for this context, but inconsistent with minimalist audit criteria |
| ACH12 | **P1-DELETE** | EmptyStateContainer exists in hierarchy | DELETE - all categories have achievements (per user pre-audit note #5) |

#### 04_Monetization_DailyMissions
**Screenshots**: runtime, RewardClaimBlocker_context+detail

| # | Severity | Issue | Fix |
|---|----------|-------|-----|
| DM1 | P1 | CurrencyPills in header: gems (blue) + coins (yellow) - NOT GREEN | Change to GREEN + square style |
| DM2 | P2 | Runtime: Clean layout - tabs (DAILY/WEEKLY/SPECIAL), progress bar with milestone markers (50/100), mission cards | OK - GOOD |
| DM3 | P1 | Mission card icons are WHITE SQUARES (placeholder/broken sprites) on left side of each card | Load proper mission type icons (play, win, earn, etc.) |
| DM4 | P2 | "Resets in: 12:34:56" timer text - functional | Verify bold and >= 20px |
| DM5 | P2 | Mission cards show title, description, progress bar, reward amount, status ("In Progress" / "Claim") | OK - GOOD layout |
| DM6 | P2 | "Claim" buttons are green - good call-to-action | OK |
| DM7 | **P0-BLOCKER** | RewardClaimBlocker: Shows "Mission name" and "Mission description" as PLACEHOLDER TEXT - not populated with actual mission data | Fix: populate with actual mission title and description. App Store rejection risk |
| DM8 | P1 | RewardClaimBlocker: Chest icon at top is detailed/realistic | Consider minimalist replacement |
| DM9 | P2 | RewardClaimBlocker: Cyan progress bar + "+100" reward + "Collect" button - functional | OK |
| DM10 | P2 | Console warning: "Unicode character \u2713 not found in LiberationSans SDF font" | Checkbox character not rendering - verify TMP fallback fonts handle this |

#### 04_Monetization_DailyRewards
**Screenshots**: runtime, ClaimAnimationBlocker_context+detail, MilestoneBlocker_context+detail

| # | Severity | Issue | Fix |
|---|----------|-------|-----|
| DR1 | P1 | CurrencyPills in header: gems (blue) + coins (yellow) - NOT GREEN | Change to GREEN + square style |
| DR2 | P2 | Runtime: Clean 7-day grid layout (3+3+1 grand prize), streak indicator, "CLAIM REWARD" button, timer | OK - GOOD design |
| DR3 | P2 | Day reward icons are detailed chest/gem/coin illustrations | Consistent within DailyRewards but doesn't match minimalist audit criteria |
| DR4 | P2 | Small green squares in top-right corner of each day card (checkmarks?) | Verify these are functional claim indicators |
| DR5 | P2 | "DAY 7 - GRAND PRIZE" card with "500 DigitCoins + 50 DigitGems + Exclusive Item" text | OK - GOOD |
| DR6 | P2 | "Unlocks in 2 days" text is small | Verify >= 20px |
| DR7 | P2 | ClaimAnimationBlocker: "Reward Obtained!" + "+300 DigitCoins" + "Streak: 6 days" + animated chest | Functional celebration overlay |
| DR8 | P2 | ClaimAnimationBlocker: "TAP TO CONTINUE" + "Next reward in: 14h 32m 15s" at bottom | OK |
| DR9 | **P0-BLOCKER** | MilestoneBlocker: YELLOW RECTANGLE as milestone icon (broken sprite placeholder) + "7 days in a row!" + "+100 bonus DigitGems" + "CONTINUE" | Same broken WhiteSquare sprite issue - needs proper milestone celebration icon |
| DR10 | P2 | MilestoneBlocker: "CONTINUE" button is cyan - consistent with non-destructive action color | OK |

#### 04_Monetization_Shop
**Screenshots**: runtime_1of9 through 9of9, NotEnoughBlocker_context+detail, PurchaseBlocker_context+detail

| # | Severity | Issue | Fix |
|---|----------|-------|-----|
| SH1 | P2 | CurrencyPills in header: GREEN + square style | OK - GOOD (this IS the reference style) |
| SH2 | **P0-BLOCKER** | Starter Pack icon: YELLOW SQUARE (broken WhiteSquare sprite placeholder) where pack icon should be | Load proper Starter Pack promotional icon |
| SH3 | P1 | Section divider texts ("SPECIAL OFFERS", "DAILY OFFERS", "DIGITGEMS", "DIGITCOINS", "PREMIUM THEMES", "EARNABLE THEMES", "FRAMES", "TITLES") are small | Make ALL section divider texts x2 BIGGER (per user audit criteria) |
| SH4 | P1 | DigitGems item icons: All 6 tiers show CYAN/LIGHT BLUE rectangles (placeholder for gem icons) | Load proper gem pack illustrations for each tier |
| SH5 | P1 | DigitCoins item icons: BLACK/DARK SQUARES (placeholder for coin icons) visible in small box on each card | Load proper coin icons |
| SH6 | P2 | Special Offers: "WEEKEND PACK" and "MEGA DIGITCOINS" with discount badges (50% OFF, 40% OFF) - good layout | OK |
| SH7 | P2 | Daily Offers: 3 items (FREE / 25 DigitGems / 5,000 DigitCoins) with timer "12:34:56" | OK - functional |
| SH8 | P2 | Premium Themes: 15 themes in 2-column grid with color swatches + name + "$2.50" price | OK - GOOD clean design |
| SH9 | P2 | Earnable Themes: 4 themes at $1.50 each, separate section | OK |
| SH10 | P2 | Theme bundles: "PREMIUM BUNDLE" ($26.25) and "COMPLETE COLLECTION" ($30.45) with "SAVE 30%" badges | OK - GOOD |
| SH11 | P2 | Frames section: Color rectangles with names and gem prices (200-2000), "EQUIPPED" / "IN USE" for active | OK - functional |
| SH12 | P2 | Premium frames (Legendary $1.99, Mythic $2.99, Celestial $4.99) have real-money pricing | OK |
| SH13 | P2 | Titles section: "Rookie IN USE", "Veteran", "Champion", "Legend", "Grand Master", "Digital Genius", "Speedster", "Bright Mind" | OK |
| SH14 | P2 | Title items have small DARK SQUARES that are barely visible (lock icons?) | Increase lock icon size/contrast for visibility |
| SH15 | P1 | "Elite" and "Immortal" title prices ("$0.9" and "$1.9") appear TRUNCATED - missing trailing digits | Fix: enable auto-sizing or increase width. Should show "$0.99" and "$1.99" |
| SH16 | P1 | VIP Bundle at bottom: "BUNDLE P..." and "50 levels of exclusive rew..." text TRUNCATED | Enable auto-sizing or increase VIP card width |
| SH17 | P2 | NotEnoughBlocker: "Insufficient DigitGems" dialog with cyan rectangle icon (placeholder) | Load proper gem icon in dialog |
| SH18 | P2 | NotEnoughBlocker: "Get DigitGem" text may be truncated (should be "Get DigitGems") | Verify full text renders: "Get DigitGems" |
| SH19 | P2 | PurchaseBlocker: "Confirm Purchase" + "1,200 DigitGems" + cyan square icon + "Price: $9.99" + Cancel/Purchase | Functional - but cyan square should be proper gem icon |
| SH20 | P2 | PurchaseBlocker: "Purchase" button is green, "Cancel" is grey - good color coding | OK - GOOD |
| SH21 | P2 | Daily Offers FREE item has green "FREE" badge | OK |
| SH22 | P3 | Shop scroll is very long (9 screenshots) - may benefit from jump-to-section | Consider section shortcuts, but not blocking |

**04_Monetization Summary**: 4 P0 blockers (yellow rectangle broken sprites in Achievements DetailPanel + RewardCelebration, DailyRewards MilestoneBlocker, Shop Starter Pack; DailyMissions placeholder text), 8 P1 (CurrencyPills x3, white square mission icons, section dividers too small, gem/coin placeholder icons, truncated prices, truncated VIP text), 25+ P2, 1 P3

---

### 05_Onboarding (13 screenshots)
**Scenes**: Onboarding (8 slides), CashBattleOnboarding (5 slides)

#### 05_Onboarding_MainOnboarding (8 slides)
**Screenshots**: editor_1of8_Bienvenido through 8of8_Completado

| # | Severity | Issue | Fix |
|---|----------|-------|-----|
| OB1 | P2 | Slide 1 (Welcome): Brain illustration is detailed/3D style - NOT minimalist | Consider replacing with minimalist brain icon, but acceptable for onboarding wow factor |
| OB2 | P2 | Slide 1: Bullet points "5 cognitive mini-games", "Compete against players from around the world", "Daily rewards and achievements" - text is English, GOOD | OK - English base |
| OB3 | P2 | Slide 2 (Name): "What's your name?" + input field + "CONFIRM" button. Large empty space below | Acceptable - focused input slide |
| OB4 | P2 | Slide 3 (Avatar): "Choose your Avatar" + large default avatar. Empty area below where avatars should be | Verify avatar grid populates at runtime. If empty, needs avatar options |
| OB5 | P1 | Slide 4 (Games): Game cards illustration is detailed/realistic (3D game cards with icons) - NOT minimalist | Consider simplification |
| OB6 | P1 | Slide 4: Description text area below "Cognitive Games" title is EMPTY - no content | Must add description text explaining the games feature |
| OB7 | P1 | Slide 5 (CashBattle): Shield/coin illustration is detailed/realistic - NOT minimalist | Consider simplification |
| OB8 | P1 | Slide 5: Description text area below "CashBattle" title is EMPTY | Must add description text explaining CashBattle |
| OB9 | P1 | Slide 6 (Tournaments): Trophy illustration is detailed/realistic - NOT minimalist | Consider simplification |
| OB10 | P1 | Slide 6: Description text area below "Tournaments" title is EMPTY | Must add description text |
| OB11 | P1 | Slide 7 (Rewards): Medal/confetti illustration is detailed - NOT minimalist | Consider simplification |
| OB12 | P1 | Slide 7: Description text area below "Daily Rewards" title is EMPTY | Must add description text |
| OB13 | P2 | Slide 8 (Completed): "Well done!" + small sun emoji icon + reward display "+500 DigitCoins | +50 DigitGems" + "START PLAYING!" button | OK - functional completion slide |
| OB14 | P2 | Slide 8: Emoji icon (sun) may not render in Unity (emoji rendering issue) | Verify icon renders properly or replace with sprite |
| OB15 | P2 | All slides: "DIGITPARK" header + "1/8" counter + "SKIP" button consistent | OK - GOOD |
| OB16 | P2 | All slides: "BACK" (grey) + "NEXT" (cyan) buttons consistent | OK - GOOD |
| OB17 | P2 | Page counter always shows "1/8" even on different slides | BUG: counter should update to show current slide number |

#### 05_Onboarding_CashBattleOnboarding (5 slides)
**Screenshots**: editor_1of5_Bienvenida through 5of5_GanaRetira

| # | Severity | Issue | Fix |
|---|----------|-------|-----|
| CBO1 | P2 | Slide 1 (Welcome): Crossed swords + coin illustration is detailed/3D | Consistent with CashBattle theme but not minimalist |
| CBO2 | P2 | Slide 1: "WELCOME TO CASH BATTLE!" + bullet points: 1v1 competitions, tournaments, withdrawals - clear and informative | OK - GOOD content |
| CBO3 | P2 | Slide 1: "Powered by Triump" at bottom - should be "Triumph" (typo?) | Verify correct spelling of partner name |
| CBO4 | P2 | Slide 2 (Verification): 18+ shield icon is detailed/3D with glow | Same icon as AgeVerification - replace with minimalist flat badge |
| CBO5 | P2 | Slide 2: Checkbox items "Be 18 years or older", "Verify your identity with Triump", "Confirm your banking information" | Verify "Triump" vs "Triumph" spelling |
| CBO6 | P2 | Slide 2: "100% secure and confidential process" at bottom | OK |
| CBO7 | P2 | Slide 2: Console warning about Unicode \u2713 (checkbox) not found in font | Same TMP fallback issue as DailyMissions |
| CBO8 | P2 | Slide 3 (Deposit): Coin/download illustration is detailed/3D | Not minimalist but acceptable for onboarding |
| CBO9 | P2 | Slide 3: Payment methods list, "Minimum deposit: $5 USD" | OK - clear information |
| CBO10 | P2 | Slide 4 (Play & Bet): Trophy with money illustration is detailed | Not minimalist |
| CBO11 | P2 | Slide 4: "1v1 COMPETITIONS" and "TOURNAMENTS" sections with details | OK - informative |
| CBO12 | P1 | Slide 5 (Win & Withdraw): "NI" text visible at bottom-left overlapping "BACK" button area | Text clipping/overflow bug - "NEXT" text leaking outside button bounds |
| CBO13 | P2 | Slide 5: Wallet illustration is detailed/3D gold | Not minimalist |
| CBO14 | P2 | Slide 5: Withdrawal details (min $10, max $500, 1-3 business days) | OK - clear |
| CBO15 | P2 | All slides: "CASH BATTLE" header + "1 / 5" counter + "SKIP" consistent | OK - GOOD |
| CBO16 | P2 | All slides: Dark/gold theme - distinct from main onboarding (cyan theme) | OK - GOOD distinction |
| CBO17 | P2 | "Powered by Triump - Responsible gaming - 18+" footer on all slides | Verify spelling |
| CBO18 | P2 | Counter always shows "1 / 5" on all slides | Same BUG as main onboarding - counter not updating |

**05_Onboarding Summary**: 0 P0 blockers, 8 P1 (empty description areas x4, detailed icons x4 could be P2, text clipping), 18 P2 (counter bug x2, spelling, detailed icons), 0 P3

---

### 06_Social (25 screenshots)
**Scenes**: Friends, FriendRequests, SearchPlayers, Notifications, Profile, MatchHistory, Scores

#### 06_Social_Friends
**Screenshots**: Friends_runtime, FriendRequests_runtime, SearchPlayers_runtime

| # | Severity | Issue | Fix |
|---|----------|-------|-----|
| FR1 | P2 | Friends: Clean layout - search bar, "Friend requests" banner with badge "3", friend cards with Challenge/Profile buttons | OK - GOOD |
| FR2 | P2 | Friends: "0 friends" count in header while showing 3 friend cards | BUG: count doesn't match displayed friends (editor placeholder data?) |
| FR3 | P2 | Friends: No CurrencyPills in header | Verify: intentional for Social scenes? |
| FR4 | P2 | FriendRequests: Received/Sent tabs, request cards with Accept (green) / Reject (red) buttons | OK - GOOD color coding |
| FR5 | P2 | FriendRequests: "2 pending" in header while 3 requests shown | Minor count mismatch - verify runtime accuracy |
| FR6 | P2 | SearchPlayers: Search bar with "Clear" button, player cards with + Add / View Profile buttons | OK - GOOD |
| FR7 | P2 | SearchPlayers: All 3 result players show "Online" with green dot | OK - status indicator works |

#### 06_Social_Notifications
**Screenshots**: Notifications_runtime

| # | Severity | Issue | Fix |
|---|----------|-------|-----|
| NT1 | P2 | Runtime: "NOTIFICATIONS" header + "0 unread" + filter tabs (All/Social/Games/Cash) + "Mark all as read" button | OK - GOOD |
| NT2 | P1 | Completely empty screen - no empty state message | Add empty state: "No notifications yet" or similar. Large dead space looks broken |
| NT3 | P2 | "Mark all as read" button at very bottom is far from content area | OK - always-visible action |

#### 06_Social_Profile
**Screenshots**: Profile_runtime, MatchHistory_runtime, Scores_runtime, ChangeNamePanel_context+detail, ErrorPanel_context+detail, GameSelectionPanel_context+detail

| # | Severity | Issue | Fix |
|---|----------|-------|-----|
| PR1 | P1 | CurrencyPills in header: gems (blue) + coins (yellow) - NOT GREEN | Change to GREEN + square style |
| PR2 | P2 | Profile runtime: Clean layout - avatar, "@Username", "Your profile", statistics, stats by game, Friends/History buttons, CHALLENGE button | OK - GOOD |
| PR3 | P2 | "GENERAL STATISTICS" section divider has cyan accent bars on sides | OK - consistent styling |
| PR4 | P2 | Stats by Game: 5 games with colored progress bars and "-- | 0%" | OK - placeholder for new user |
| PR5 | P2 | ChangeNamePanel: "Change Name" + input field + Cancel/Save (cyan) | OK - consistent with CashProfile and Settings versions |
| PR6 | P1 | ErrorPanel: "Error" text + "Accept" button on grey semi-transparent overlay - overlaps with profile content behind. Unprofessional positioning | Redesign: center the error panel properly, add error description text, darken overlay more |
| PR7 | P2 | ErrorPanel: Only says "Error" with no description of what went wrong | Add dynamic error message text |
| PR8 | P2 | GameSelectionPanel: "CHOOSE A GAME" + list of 5 games (Digit Rush, Memory Pairs, Quick Math, Flash Tap, Odd One Out) + Cancel | OK - clean list |
| PR9 | P2 | GameSelectionPanel: Game names are cyan text on dark background, good readability | OK |

#### 06_Social_MatchHistory
**Screenshots**: MatchHistory_runtime

| # | Severity | Issue | Fix |
|---|----------|-------|-----|
| MH1 | P1 | CurrencyPills: gems (blue) + coins (yellow) - NOT GREEN | Change to GREEN |
| MH2 | P1 | Game filter icons at top are detailed neon illustrations (7 small icons: All + 5 games + 1 extra) - NOT minimalist | Replace with minimalist game icons |
| MH3 | P2 | Empty state - no match history items visible | OK for new user, but verify empty state message exists |
| MH4 | P2 | "All" filter is highlighted cyan | OK |

#### 06_Social_Scores (Rankings)
**Screenshots**: Scores_runtime, EmptyState_context+detail, ErrorPanel_context+detail, LoadingPanel_context+detail

| # | Severity | Issue | Fix |
|---|----------|-------|-----|
| SC1 | P1 | CurrencyPills: gems (blue) + coins (yellow/GOLD square) - NOT GREEN | Change to GREEN |
| SC2 | P2 | Scores runtime: "RANKINGS" header, game filter icons, National/Global tabs, leaderboard with 5 entries, "YOUR POSITION" footer | OK - GOOD layout |
| SC3 | P1 | Game filter icons are detailed neon illustrations - NOT minimalist | Replace with minimalist game icons |
| SC4 | P2 | Leaderboard entries: rank number, avatar, name, time score - clean layout | OK - GOOD |
| SC5 | P2 | Top 3 use large numbers (1, 2, 3), rest use "#4", "#5" format | OK - visual hierarchy |
| SC6 | **P0-BLOCKER** | EmptyState: Shows raw AutoLocalizer keys as text - "ranking..." (small orange), "empty_leaderboard_title", "empty_leaderboard_subtitle", "play_now" button | These keys are NOT translating - verify they exist in Translations.txt and TextNameToKeyMap |
| SC7 | P1 | ErrorPanel: Grey rectangle with "Error" text + "Accept" cyan button - same unprofessional overlay as Profile ErrorPanel | Redesign: proper centered error panel with description |
| SC8 | **P0-BLOCKER** | LoadingPanel: Shows raw AutoLocalizer key "loading_rankings" as visible text | Key not translating - verify Translations.txt entry exists |
| SC9 | P2 | "YOUR POSITION" footer: "#--" and "--" placeholder text | OK for unranked user |

#### 06_Social_SearchPlayers (hidden objects)
**Screenshots**: EmptyState_context+detail, LoadingIndicator_context+detail, NoResultsText_context+detail

| # | Severity | Issue | Fix |
|---|----------|-------|-----|
| SP1 | P2 | EmptyState: "Search players" + "Find players to add as friends or challenge" + player icon - professional empty state | OK - GOOD |
| SP2 | P2 | EmptyState: Shown BELOW search results (both results and empty state visible) | BUG: EmptyState should only show when no results AND no search active |
| SP3 | P2 | LoadingIndicator: "Searching..." text centered below results | OK - but should hide results while searching |
| SP4 | P2 | NoResultsText: "No players found" text centered | OK - GOOD |
| SP5 | P2 | NoResultsText: Also shown BELOW existing search results | BUG: should replace results, not appear alongside them |

**06_Social Summary**: 2 P0 blockers (raw AutoLocalizer keys in Scores EmptyState + LoadingPanel), 7 P1 (CurrencyPills x3, empty Notifications, detailed icons x2, ErrorPanel design), 25+ P2

---

### 07_Tournaments (26 screenshots)
**Scenes**: TournamentsBrowser, TournamentCreate, TournamentLobby

#### 07_Tournaments_TournamentsBrowser
**Screenshots**: runtime, EmptyState_context+detail, FilterPanel_context+detail, LoadingIndicator_context+detail, LoadMoreButton_context+detail, RefreshIndicator_context+detail

| # | Severity | Issue | Fix |
|---|----------|-------|-----|
| TB1 | P1 | CurrencyPills: gems (blue) + coins (yellow) - NOT GREEN | Change to GREEN + square style |
| TB2 | P2 | Runtime: "TOURNAMENTS" header, 3 tabs (Search Tournaments / My Tournaments / Featured), search bar + "Filters" button, "+" FAB | OK - GOOD layout |
| TB3 | P1 | Runtime: Completely empty - no tournament cards visible, no empty state message | BUG: either populate with tournaments OR show EmptyState. Currently shows nothing |
| TB4 | P2 | EmptyState: Cyan rectangle icon + "No tournaments available" + "Be the first to create one or come back later" + "Create Tournament" button | OK - GOOD empty state content |
| TB5 | P2 | EmptyState: Icon at top is a large cyan rectangle (placeholder?) | Replace with proper empty state illustration/icon |
| TB6 | P1 | FilterPanel: Shows 3 dropdown rows ALL labeled "All" with no context labels (Game? Status? Entry Fee?) | Add filter category labels above each dropdown |
| TB7 | P2 | FilterPanel: "Clear Filters" button at bottom | OK |
| TB8 | **P0-BLOCKER** | LoadingIndicator: CYAN SQUARE (WhiteSquare sprite tinted) + "Loading..." text | Same broken loading pattern as Auth - redesign with proper spinner |
| TB9 | P2 | LoadMoreButton: "Load More" button at bottom of empty list | OK - functional |
| TB10 | P2 | RefreshIndicator: "Refreshing..." text centered on empty screen | OK - functional |
| TB11 | P2 | Console warning about font asset visible at bottom | Same TMP fallback issue |

#### 07_Tournaments_TournamentCreate
**Screenshots**: editor_1of2, editor_2of2, ConfirmBlocker_context+detail, LoadingOverlay_context+detail

| # | Severity | Issue | Fix |
|---|----------|-------|-----|
| TC1 | P1 | Header: "CREATE TOURNAME" - title TEXT TRUNCATED, missing last letters | Fix: enable auto-sizing on title OR reduce font size to fit "CREATE TOURNAMENT" |
| TC2 | P1 | CurrencyPills: gems (blue) + coins (yellow) - NOT GREEN | Change to GREEN |
| TC3 | **P0-BLOCKER** | Multiple raw AutoLocalizer keys visible as text: "free_label", "tournament_players_count", "tournament_1round", "tournament_30sec", "tournament_attempts_recommended", "tournament_creation_fee" | Keys NOT translating in editor - verify all exist in Translations.txt and TextNameToKeyMap |
| TC4 | P2 | Form sections: Tournament Name, Game Type, Entry Fee, Players & Prize, Start Schedule, Tournament Rules, Privacy, Preview | OK - GOOD form structure |
| TC5 | P2 | "Estimated prize: $0.00" in green text | OK |
| TC6 | P2 | "Name must be at least 3 characters" validation text at bottom | OK - GOOD UX |
| TC7 | P2 | ConfirmBlocker: "Confirm Creation" + "Are you sure you want to create this tournament?" + Cancel/Confirm buttons | OK - proper confirmation dialog |
| TC8 | P2 | ConfirmBlocker: "Confirm" button is cyan (non-destructive action) | OK - correct color |
| TC9 | P1 | LoadingOverlay: Large CYAN RECTANGLE (WhiteSquare sprite) + "Creating tournament..." text. Rectangle overlaps with "FREE" text below | Same broken loading sprite - redesign with proper fullscreen dark overlay |
| TC10 | P2 | Section accent bars (cyan rectangles on right side of each section header) | OK - consistent styling |

#### 07_Tournaments_TournamentLobby
**Screenshots**: editor, LeaveBlocker_context+detail, LoadingOverlay_context+detail, PrizesBlocker_context+detail, StartingOverlay_context+detail

| # | Severity | Issue | Fix |
|---|----------|-------|-----|
| TL1 | P2 | Editor: "Tournament Name" + "OPEN" badge, "Digit Rush", "FREE" + "$50" prize, timer "02:45:30", "7/10" players, progress bar | OK - GOOD layout |
| TL2 | P2 | Participants/Chat tabs, "# Player" + "Time s" columns | OK |
| TL3 | P2 | Footer: "#5 You" position + JOIN (cyan) / Share (grey) / Leave (red) buttons | OK - GOOD color coding |
| TL4 | P2 | LeaveBlocker: "Leave Tournament?" + "You will lose your progress and your entry fee will not be refunded." + Stay (cyan) / Leave (red) | OK - proper warning dialog with correct colors |
| TL5 | P1 | LoadingOverlay: Large CYAN RECTANGLE (WhiteSquare sprite) + "Loading..." text | Same broken loading pattern - redesign |
| TL6 | P2 | PrizesBlocker: "PRIZES" title + "1st Place $25" + "2nd Place $15" + "3rd Place $10" + "Close" button | OK - clean prize display |
| TL7 | P2 | PrizesBlocker: 1st prize text is yellow, 2nd white, 3rd green - good visual hierarchy | OK |
| TL8 | P2 | StartingOverlay: Large "3" countdown + "Tournament starts in..." text - clean design | OK - professional |
| TL9 | P2 | Console warning about font asset visible | Same TMP fallback issue |
| TL10 | P2 | Small yellow/green squares visible next to "FREE" and "$50" - may be broken currency icons | Verify if these are functional icons or broken sprites |

**07_Tournaments Summary**: 2 P0 blockers (cyan square loading indicator, raw AutoLocalizer keys in TournamentCreate), 6 P1 (CurrencyPills x2, truncated title, filter labels missing, loading overlays x2), 15+ P2

---

## CROSS-SCENE CONSISTENCY CHECK

### 1. Loading Indicators - BROKEN EVERYWHERE
**Pattern**: WhiteSquare sprite tinted (yellow/cyan/orange) used as "spinner" placeholder across the entire app.
**Affected scenes** (9 total):
- Auth: AgeVerification (yellow), Login (cyan), Register (cyan)
- CashBattle: MatchmakingPanel (yellow), CashTournamentCreate LoadingOverlay (cyan)
- Tournaments: TournamentsBrowser LoadingIndicator (cyan), TournamentCreate LoadingOverlay (cyan), TournamentLobby LoadingOverlay (cyan)
- Monetization: DailyRewards MilestoneBlocker (yellow), Achievements DetailPanel (yellow), Achievements RewardCelebration (yellow), Shop Starter Pack (yellow)
**Verdict**: Need ONE unified professional loading overlay component (fullscreen semi-dark + centered spinner + contextual text). Apply to ALL scenes.

### 2. CurrencyPills Color - WRONG IN 10+ SCENES
**Pattern**: Gems=blue, coins=yellow/gold. Should ALL be GREEN + square style (like Shop reference).
**Affected scenes**: MainMenu, Achievements, DailyMissions, DailyRewards, Profile, MatchHistory, Scores, TournamentsBrowser, TournamentCreate, BetSelection, PlayModeSelection, Matchmaking
**Exception**: MainMenu has different proportions (acceptable per criteria).
**Reference**: Shop (SH1) is the correct GREEN style.
**Verdict**: Single CurrencyHeaderBarHelper fix propagates to all scenes via UIBuilders.

### 3. ErrorPanels - UNPROFESSIONAL DESIGN
**Pattern**: Grey rectangle with just "Error" text + "Accept" button. No error description, no proper centering, overlaps content behind.
**Affected scenes**: Profile (PR6), Scores (SC7), CashProfile (CP5), CashWallet (CW6)
**Verdict**: Need ONE unified error panel component: proper dark overlay, centered card, title + description text, dismiss button.

### 4. EmptyState Visibility Bugs
**Pattern**: EmptyState visible SIMULTANEOUSLY with actual content (should be mutually exclusive).
**Affected scenes**: SearchPlayers (SP2/SP5 - empty state + results both visible), CashTournaments (CT3 - empty state below tournament cards)
**Verdict**: Fix visibility logic: EmptyState.SetActive only when list count == 0.

### 5. Game Icons - DETAILED NEON vs MINIMALIST
**Pattern**: All game-related icons use detailed 3D neon illustrations with glows/gradients instead of flat minimalist style.
**Affected scenes**: GameSelector (6 icons), PlayModeSelection (brain, swords), CashBattleHub (5 menu icons), CashMatchmaking (VS icon), CashHistory (game icons in cards), MatchHistory (filter icons), Scores (filter icons), Onboarding slides 4-7, CashBattleOnboarding slides 1-5
**Verdict**: Need minimalist icon set for all 5 games + navigation icons. Onboarding icons are lower priority (acceptable for "wow factor").

### 6. Raw AutoLocalizer Keys Visible
**Pattern**: Translation keys showing as raw text instead of translated content.
**Affected scenes**:
- Scores EmptyState: `empty_leaderboard_title`, `empty_leaderboard_subtitle`, `play_now` (SC6)
- Scores LoadingPanel: `loading_rankings` (SC8)
- TournamentCreate: `free_label`, `tournament_players_count`, `tournament_1round`, `tournament_30sec`, `tournament_attempts_recommended`, `tournament_creation_fee` (TC3)
- BetSelection: `bet_title`, `bet_coins_cost`, `bet_gems_wager`, etc. (BS1 - editor only, may work at runtime)
**Verdict**: Verify keys exist in Translations.txt + TextNameToKeyMap. If missing, add them.

### 7. "DBG" Debug Label
**Pattern**: "DBG" text visible on left side of screen in Boot, MainMenu, Settings.
**Root cause**: Likely a single debug overlay GameObject active in Build scene or Canvas.
**Verdict**: Single fix - find and disable/delete the debug label GO.

### 8. Text Truncation
**Pattern**: Text cut off due to missing auto-sizing or insufficient container width.
**Affected**:
- QuickMath SettingsPanel: "NOR..." (QM4 - P0)
- TournamentCreate header: "CREATE TOURNAME" (TC1)
- CashBattle1v1 GameSelectionModal: "Digit R...", "Flash T..." (CB1v1_2)
- Shop: "$0.9", "$1.9" prices (SH15), "BUNDLE P..." VIP text (SH16)
- CashTournaments PremiumBlockPanel: text clipped (CT5)
- Onboarding slide 5: "NI" text overflow (CBO12)
**Verdict**: Enable TMP auto-sizing on all affected text components.

### 9. Placeholder/Debug Content
**Pattern**: Placeholder text visible to end users.
**Affected**:
- GameSelector RulesPanel: "Rule 1", "Rule 2", "Rule 3" (GS3 - P0)
- DailyMissions RewardClaimBlocker: "Mission name", "Mission description" (DM7 - P0)
- CashHistory DetailPanel: "Title", "Subtitle" labels (CH7)
**Verdict**: Replace with dynamic content or proper default text.

### 10. Onboarding Counters Frozen
**Pattern**: Slide counter shows "1/8" (or "1/5") on ALL slides instead of updating.
**Affected**: Main Onboarding (OB17), CashBattle Onboarding (CBO18)
**Verdict**: Bug in OnboardingManager - counter text not updating on slide change.

### 11. Confirm/Blocker Dialogs - MOSTLY CONSISTENT
**Pattern**: Most confirm dialogs (Logout, Delete, SelfExclusion, LeaveBlocker, ConfirmBet, PurchaseBlocker) use consistent style with proper color coding (cyan=safe, red=destructive, green=purchase).
**Verdict**: OK - good consistency. Only ErrorPanels need redesign (see #3).

### 12. Spelling Issues
**Pattern**: "Triump" appears multiple times in CashBattleOnboarding (CBO3, CBO5, CBO17).
**Should be**: "Triumph" (partner name).
**Also**: Missing accents in Settings translations: "Terminos" → "Términos", "Politica" → "Política" (S3).

---

## FIX PLAN (Priority order)

### P0 - App Store Blockers (14 issues → ~7 fixes)

| # | Fix | Scenes Affected | Effort |
|---|-----|-----------------|--------|
| P0-1 | **Remove "DBG" debug label** - Find and delete/disable debug overlay GO | Boot, MainMenu, Settings (B1, M1, S1) | LOW - single GO delete |
| P0-2 | **Redesign Loading Indicators** - Create unified LoadingOverlay prefab (dark overlay + spinner + text), replace ALL WhiteSquare placeholders | Auth x3, CashBattle x2, Tournaments x3, Monetization x3 (AV1, L1, R1, CBH5, TB8, TC9, TL5, ACH5, ACH8, DR9, SH2) | HIGH - new prefab + apply to ~12 scenes |
| P0-3 | **Fix raw AutoLocalizer keys in Scores** - Add missing keys to Translations.txt + TextNameToKeyMap | Scores EmptyState + LoadingPanel (SC6, SC8) | LOW - add ~4 keys |
| P0-4 | **Fix raw AutoLocalizer keys in TournamentCreate** - Add missing keys | TournamentCreate (TC3) | LOW - add ~6 keys |
| P0-5 | **Replace placeholder text in GameSelector RulesPanel** - Populate with real game rules or dynamic content | GameSelector (GS3) | MEDIUM - need rule text per game |
| P0-6 | **Replace placeholder text in DailyMissions RewardClaimBlocker** - Populate with actual mission data | DailyMissions (DM7) | MEDIUM - wire mission data to popup |
| P0-7 | **Fix QuickMath "NOR..." truncation** - Enable auto-sizing on difficulty label | QuickMath SettingsPanel (QM4) | LOW - single text component |
| P0-8 | **Fix CashTournaments EmptyState bug** - Hide EmptyState when tournament cards exist | CashTournaments (CT3) | LOW - visibility logic fix |
| P0-9 | **Fix CashWallet KYCPanel** - Add title, explanation text, proper layout | CashWallet (CW7) | MEDIUM - panel redesign |

### P1 - Critical Visual Issues (~40 issues → ~12 fixes)

| # | Fix | Scenes Affected | Effort |
|---|-----|-----------------|--------|
| P1-1 | **CurrencyPills → GREEN + square** - Update CurrencyHeaderBarHelper colors | 10+ scenes (M2, ACH1, DM1, DR1, PR1, MH1, SC1, TB1, TC2, etc.) | LOW - single helper change, rebuild all |
| P1-2 | **Delete obsolete Win/Lose panels from minigames** - Remove per-scene panels replaced by global ones | DigitRush, FlashTap, MemoryPairs, OddOneOut, QuickMath (FT4, MP4, MP5, OO6, QM6) | LOW - delete GOs from 5 scenes |
| P1-3 | **Delete MemoryPairs PlayAgainButton** | MemoryPairs (MP4) | LOW |
| P1-4 | **Delete Achievements EmptyStateContainer** | Achievements (ACH12) | LOW |
| P1-5 | **Replace game icons with minimalist versions** - All 5 game icons + navigation icons | GameSelector, PlayModeSelection, CashBattleHub, CashHistory, MatchHistory, Scores (~20 icon instances) | HIGH - need new icon assets |
| P1-6 | **Fix text truncation** - Enable auto-sizing on all truncated text | TournamentCreate (TC1), CashBattle1v1 (CB1v1_2), Shop (SH15, SH16), CashTournaments (CT5), Onboarding (CBO12) | MEDIUM - multiple scenes |
| P1-7 | **Redesign ErrorPanels** - Unified error panel with dark overlay + centered card + description | Profile, Scores, CashProfile, CashWallet (PR6, SC7, CP5, CW6) | MEDIUM - new component |
| P1-8 | **Fix DailyMissions white square icons** - Load proper mission type icons | DailyMissions (DM3) | MEDIUM - need icon assets |
| P1-9 | **Shop section dividers x2 bigger** - Increase font size of section header texts | Shop (SH3) | LOW - font size change |
| P1-10 | **Shop gem/coin placeholder icons** - Load proper gem/coin pack icons | Shop (SH4, SH5) | MEDIUM - need icon assets |
| P1-11 | **Add Onboarding slide descriptions** - Fill empty description areas on slides 4-7 | Onboarding (OB6, OB8, OB10, OB12) | LOW - add text content |
| P1-12 | **Add Notifications empty state message** | Notifications (NT2) | LOW - add empty state text |
| P1-13 | **Redesign CashHistory DetailPanel** - Remove "Title"/"Subtitle" placeholders, proper card layout | CashHistory (CH7) | MEDIUM |
| P1-14 | **Boot "Completado!" hardcoded Spanish** - Use AutoLocalizer.Get() | Boot (B2) | LOW |
| P1-15 | **Fix TournamentsBrowser filter labels** - Add category labels above dropdowns | TournamentsBrowser (TB6) | LOW |

### P2 - Consistency & Minor Issues (~130 issues → grouped fixes)

| # | Fix | Effort |
|---|-----|--------|
| P2-1 | **Fix Onboarding slide counters** - Update counter text on slide change | LOW |
| P2-2 | **Fix "Triump" → "Triumph" spelling** - CashBattleOnboarding 3 instances | LOW |
| P2-3 | **Fix accent marks** - "Terminos" → "Términos", "Politica" → "Política" in Translations.txt | LOW |
| P2-4 | **Fix SearchPlayers EmptyState/NoResults visibility** - Mutually exclusive with results | LOW |
| P2-5 | **Verify BetSelection keys exist** - `bet_title`, `bet_coins_cost`, etc. in Translations.txt | LOW |
| P2-6 | **Rename 17 mislabeled CashBattle screenshots** | LOW - file rename only |
| P2-7 | **Delete MainMenu NotificationsPanel GO** | LOW |
| P2-8 | **Verify emoji icon renders in Onboarding slide 8** | LOW |
| P2-9 | **Friends/FriendRequests count mismatch** - Verify runtime accuracy | LOW |
| P2-10 | **Shop NotEnoughBlocker/PurchaseBlocker placeholder icons** - Replace cyan squares with gem icons | MEDIUM |
| P2-11 | **CashMatchmaking countdown overlaps player card** | LOW |
| P2-12 | **Verify all font sizes >= 20px** - Auth small text, DailyRewards "Unlocks in...", CashTournamentCreate creation fee | MEDIUM |
| P2-13 | **Standardize CashWallet transaction history item layout** | MEDIUM |
| P2-14 | **TMP fallback font warnings** - Unicode \u2713 checkbox character | LOW - verify fallback chain |
| P2-15 | **Achievements DetailPanel/RewardCelebration broken icon sprites** - Load actual achievement icons | MEDIUM |

### P3 - Polish (5 issues)

| # | Fix | Effort |
|---|-----|--------|
| P3-1 | Verify SafeArea padding on MainMenu bottom row | LOW |
| P3-2 | Settings section divider visibility | COSMETIC |
| P3-3 | Settings ">" arrow size | COSMETIC |
| P3-4 | Register input placeholder styling | OK - no fix needed |
| P3-5 | Shop jump-to-section shortcuts | FUTURE - not blocking |

---

## EXECUTION ORDER (Recommended)

**Phase 1 - Blockers** (P0-1 through P0-9): Remove debug labels, fix broken loading indicators, add missing translation keys, fix placeholder text, fix truncation, fix EmptyState bugs.

**Phase 2 - Visual Cleanup** (P1-1 through P1-15): CurrencyPills color, delete obsolete GOs, fix text truncation, redesign ErrorPanels, add missing content.

**Phase 3 - Icon Overhaul** (P1-5, P1-8, P1-10): Replace all detailed neon icons with minimalist versions. This is the largest single effort and requires new icon assets.

**Phase 4 - Consistency** (P2-1 through P2-15): Fix counters, spelling, visibility logic, verify translations, rename screenshots.

**Phase 5 - Polish** (P3): SafeArea, cosmetic tweaks.

---

## STATS SUMMARY

| Severity | Count | Status |
|----------|-------|--------|
| P0 - App Store Blockers | 14 issues → 9 fixes | PENDING |
| P1 - Critical Visual | ~40 issues → 15 fixes | PENDING |
| P2 - Consistency | ~130 issues → 15 grouped fixes | PENDING |
| P3 - Polish | 5 issues | PENDING |
| **TOTAL** | **~190 issues** | **0% fixed** |

**Estimated unique fixes needed**: ~44 (many issues share root cause)
**Scenes requiring changes**: 30+ out of ~35 total
**New assets needed**: Minimalist game icons (5), loading spinner, mission icons, error panel prefab

---
---

# CODE AUDIT - Full Codebase Analysis
**Date**: 2026-03-07
**Scope**: ALL runtime + Editor scripts (~355 .cs files)
**Purpose**: Logic errors, dead code, missing localization, font sizes, mock services, unreferenced values

---

## CA-1: MOCK/SIMULATION SERVICES - PRODUCTION BLOCKER

**Status: CRITICAL - App ships with FAKE backend**

### Mock Services Folder (Assets/_Project/Scripts/Services/Mock/)

| # | File | Lines | What It Simulates | Production Risk |
|---|------|-------|-------------------|-----------------|
| MOCK-1 | **MockWalletService.cs** | 1-360 | Real money wallet via PlayerPrefs. Deposits, withdrawals, promo codes ("WELCOME50"), transaction history. Uses `Mock_Wallet_Balance` PlayerPrefs key | **CRITICAL** - Users' money tracked in LOCAL storage, not backend |
| MOCK-2 | **MockKYCService.cs** | 1-205 | KYC identity verification. Age check = local date calc. Document+selfie = fake delay. Uses `Mock_KYC_Status` PlayerPrefs | **CRITICAL** - Legal compliance bypassed. ANY user claims 18+ |
| MOCK-3 | **MockTournamentService.cs** | 1-332 | 4 hardcoded fake tournaments (tournament_001-004). Fake entry fees ($5-$25), fake prize pools ($450-$2250), fake leaderboards with names like "ProGamer", "SpeedKing" | **CRITICAL** - Users see fake tournaments with real money at stake |
| MOCK-4 | **MockMatchmakingService.cs** | 1-250 | Fake opponents ("ProGamer99", "SpeedDemon", etc.). Random stats, fake avatars. Opponent score = `Random.Range(score*0.7, score*1.3)`. Entry fees $0.50-$10.00 | **CRITICAL** - Users think they play real people |

### ServiceLocator Default = Mock Mode

| # | File | Line | Issue | Risk |
|---|------|------|-------|------|
| MOCK-5 | **ServiceLocator.cs** | 63 | `_serviceMode = ServiceMode.Mock` (DEFAULT) | **CRITICAL** - App ships in Mock mode unless manually changed |
| MOCK-6 | ServiceLocator.cs | 68 | `initialBalance = $100` mock config | HIGH |
| MOCK-7 | ServiceLocator.cs | 77 | `alwaysFindMatch = true` mock config | HIGH |
| MOCK-8 | ServiceLocator.cs | 230-248 | If Production mode BUT no `_triumphApiKey` → falls back to Mock silently | **CRITICAL** - Silent fallback to fake services |

### Triumph SDK = Not Implemented (All Stubs)

| # | File | Lines | Issue | Risk |
|---|------|-------|-------|------|
| MOCK-9 | **TriumphServices.cs** | 1-304 | 4 service classes, ALL methods throw `NotImplementedException()`. 19 TODO comments | **CRITICAL** - Production mode = instant crash |
| MOCK-10 | **TriumphManager.cs** | 29 | `_isEnabled = false` (disabled by default) | HIGH |
| MOCK-11 | TriumphManager.cs | 87-93 | SDK init code is ALL commented out | HIGH |
| MOCK-12 | TriumphManager.cs | 223-245 | 4 event handlers (`HandleTriumphReady`, etc.) never called - no subscriptions | MEDIUM |
| MOCK-13 | TriumphManager.cs | 199-205 | ContextMenu debug methods: AddFunds($50), ToggleVerification, SimulateMatch | MEDIUM |

### Fake Geolocation

| # | File | Line | Issue | Risk |
|---|------|------|-------|------|
| MOCK-14 | **LocationRestrictionService.cs** | 87-100 | TODO: "usar Triumph SDK para verificar ubicacion real". Hardcoded `CurrentState = "California"` | HIGH - Users in restricted states bypass restrictions |
| MOCK-15 | LocationRestrictionService.cs | 121-140 | `SetRestrictedState()` / `SetAllowedState()` test methods exposed | MEDIUM |

### Editor Auth Bypass

| # | File | Line | Issue | Risk |
|---|------|------|-------|------|
| MOCK-16 | **EditorBootConfig.cs** | 56-63 | Sets `CashBattleBypassAuth=1`, `Mock_KYC_Status=3` (FullyVerified), `AgeVerified=1` in PlayerPrefs. Wrapped in `#if UNITY_EDITOR` but PlayerPrefs persist | MEDIUM - Verify doesn't bleed into builds |

### Incomplete Firebase Integration

| # | File | Line | Issue | Risk |
|---|------|------|-------|------|
| MOCK-17 | **NotificationService.cs** | 213 | Firebase Database saving not implemented | MEDIUM |
| MOCK-18 | **DailyRewardService.cs** | 219, 376-398 | Partial features not implemented (5 TODOs) | MEDIUM |
| MOCK-19 | **AchievementService.cs** | 979 | Firebase saving not implemented | MEDIUM |
| MOCK-20 | **WalletManager.cs** | 220 | `return (0m, 0m, 0m)` - TODO: "Get from service when available" | LOW |
| MOCK-21 | **DigitRushController.cs** | 1161 | "Get real opponent result from server" - currently simulated | MEDIUM |

**Mock Services Summary**: 21 issues. 8 CRITICAL, 7 HIGH, 5 MEDIUM, 1 LOW. **24 Triumph TODOs** pending implementation.

---

## CA-2: FONT SIZES BELOW 22px

**Rule**: All text must be ≥22px minimum to be readable on mobile devices.

### fontSizeMin = 10 (Dangerously Small)

| # | File | Line | Element | fontSize | fontSizeMin | Risk |
|---|------|------|---------|----------|-------------|------|
| FS-1 | CashTournamentCreateUIBuilder.cs | 752 | Display Price | auto | **10** | Text can shrink to 10px - UNREADABLE |
| FS-2 | CashTournamentCreateUIBuilder.cs | 783 | Estimated Prize | auto | **10** | Same |
| FS-3 | CashTournamentCreateUIBuilder.cs | 822 | Toggle "Start Immediately" | auto | **10** | Same |
| FS-4 | CashTournamentCreateUIBuilder.cs | 895 | "Allow Spectators" label | auto | **10** | Same |
| FS-5 | CashTournamentCreateUIBuilder.cs | 923 | "Private Tournament" label | auto | **10** | Same |
| FS-6 | CashTournamentCreateUIBuilder.cs | 969 | Player Count Placeholder | auto | **10** | Same |
| FS-7 | CashTournamentCreateUIBuilder.cs | 1023 | Fee Text | auto | **10** | Same |
| FS-8 | CashTournamentCreateUIBuilder.cs | 1063 | Fee TMP | auto | **10** | Same |
| FS-9 | CashTournamentCreateUIBuilder.cs | 1106 | Create Button text | auto | **10** | Same |
| FS-10 | DailyMissionsUIBuilder.cs | 813-816 | Reward Text | **18f** | **10f** | fontSize=18 + min=10 - BOTH under 22 |

### fontSizeMin = 20 (Borderline)

| # | File | Line | Element | fontSizeMin | Risk |
|---|------|------|---------|-------------|------|
| FS-11 | FriendRequestsUIBuilder.cs | 625 | Request Button Text | **20f** | Borderline - can shrink to 20px |

**Font Size Summary**: 11 instances with minimums below 22px. 10 at fontSizeMin=10 (CRITICAL), 1 at fontSizeMin=20 (borderline). All in CashTournamentCreate (9) + DailyMissions (1) + FriendRequests (1).

---

## CA-3: AUTOLOCALIZER AUDIT

### Duplicate Translation Keys in Translations.txt

| # | Key | Line 1 | Line 2 | Issue |
|---|-----|--------|--------|-------|
| AL-1 | **bet_custom_reward** | 4760 | 7145 | Different ES translations: "Ganar" vs "Gana" - second overrides first |
| AL-2 | **filter_all** | 7779 | 9853 | Exact duplicate (identical content) - wasteful |
| AL-3 | **logout_confirm_message** | 2349 | 7956 | Different content: short vs long version. Second (longer) overrides |

### Missing Translation Keys (in TextNameToKeyMap but NOT in Translations.txt)

| # | Key | Impact |
|---|-----|--------|
| AL-4 | `1v1_battles` | Will show raw key at runtime |
| AL-5 | `1v1_description` | Same |
| AL-6 | `1v1_title` | Same |

### Hardcoded Spanish Strings in Runtime Code (User-Facing)

| # | File | Lines | Strings | Fix |
|---|------|-------|---------|-----|
| AL-7 | **FriendData.cs** | 102-114 | "En línea", "Desconocido", "Hace un momento", "Hace {int} min", "Hace {int} horas", "Hace {int} días" (6 strings) | AutoLocalizer.Get() with time keys |
| AL-8 | **TournamentsBrowserManager.cs** | 429 | "No estás participando en ningún torneo.\n¡Únete a uno!" | AutoLocalizer.Get("tournament_empty_state_my") |
| AL-9 | **MockTournamentService.cs** | 240, 292 | "No estás en este torneo" (2 occurrences) | AutoLocalizer.Get("tournament_not_member") |
| AL-10 | **InputPanelUI.cs** | 246, 252, 258 | "El campo no puede estar vacío", "Debe tener al menos {int} caracteres", "Debe tener máximo {int} caracteres" | AutoLocalizer.Get() with validation keys |
| AL-11 | **MatchHistorySceneManager.cs** | 386 | "Desconocido" (unknown game type) | AutoLocalizer.Get("game_type_unknown") |
| AL-12 | **NotificationStorageService.cs** | 308 | "Hace {int} días" (duplicate of FriendData pattern) | AutoLocalizer.Get() with time keys |

### LocalizationManager Fallback Strings (Spanish)

| # | File | Lines | Status |
|---|------|-------|--------|
| AL-13 | LocalizationManager.cs | 500-528 | Hardcoded Spanish backup translations. ACCEPTABLE as fallback system but should be in Translations.txt only |

**AutoLocalizer Summary**: 3 duplicate keys, 3 missing keys, 6 files with hardcoded Spanish (13+ strings total).

---

## CA-4: BOLD ENFORCEMENT VIOLATIONS

**Rule**: ALL visible text must use FontStyles.Bold.

| # | File | Line | Issue | Fix |
|---|------|------|-------|-----|
| BF-1 | **TournamentManager.cs** | 1046 | `tmp.fontStyle = FontStyles.Normal;` | Change to `FontStyles.Bold` |
| BF-2 | **PremiumPanelUI.cs** | 953 | `tmp.fontStyle = FontStyles.Underline;` (button text) | Change to `FontStyles.Bold` |

**Bold Summary**: 2 violations found in runtime code.

---

## CA-5: LOGIC ERRORS & CODE QUALITY

### Async Void Methods (Exception Swallowing Risk)

| # | File | Line | Method | Risk |
|---|------|------|--------|------|
| LG-1 | LeaderboardManager.cs | 69 | `private async void Start()` | Exceptions silently swallowed |
| LG-2 | LeaderboardManager.cs | 310 | `private async void LoadLeaderboard()` | Same |
| LG-3 | AvatarUI.cs | 50 | `private async void Start()` | Same |
| LG-4 | LoginManager.cs | 289 | `private async void OnLoginButtonClicked()` | Same |
| LG-5 | LoginManager.cs | 377 | `private async void OnGoogleLoginClicked()` | Same |
| LG-6 | LoginManager.cs | 404 | `private async void OnAppleLoginClicked()` | Same |
| LG-7 | AgeVerificationManager.cs | 192 | `private async void OnVerifyClicked()` | Same |
| LG-8 | AgeVerificationManager.cs | 277 | `public static async void ResetVerification()` | Same |
| LG-9 | ReviewService.cs | 166 | `private async void RequestAndroidReview()` | Same |
| LG-10 | CashWalletSceneController.cs | 475 | `private async void OnDepositOptionSelected()` | Same |
| LG-11 | CashProfileSceneController.cs | 280 | `private async void OnConfirmNameChange()` | Same |

**Impact**: 11 methods. If any async operation throws, the exception is lost - no error handling, no user feedback, silent failure.

---

## CA-6: DEAD / UNREFERENCED CODE

### Unused Fields

| # | File | Line | Field | Issue |
|---|------|------|-------|-------|
| DC-1 | WinPanelController.cs | 19-21 | `isRealMoneyPanel` (pragma suppressed) | Declared, never read |
| DC-2 | AchievementService.cs | 25-27 | `_isInitialized` (pragma suppressed) | Set to true, never checked |
| DC-3 | TapButtonEffect.cs | 18 | `innerFill` ([SerializeField]) | Auto-found but never used |
| DC-4 | LocationRestrictionService.cs | 60 | `OnLocationCheckFailed` event (pragma suppressed) | Declared, never invoked |

### Dead Event Handlers (No Subscribers)

| # | File | Line | Method | Issue |
|---|------|------|--------|-------|
| DC-5 | TriumphManager.cs | 225 | `HandleTriumphReady()` | No subscription - SDK init commented out |
| DC-6 | TriumphManager.cs | 231 | `HandleBalanceChanged()` | Same |
| DC-7 | TriumphManager.cs | 237 | `HandleGameStart()` | Same |
| DC-8 | TriumphManager.cs | 242 | `HandleMatchComplete()` | Same |

### Empty Methods

| # | File | Line | Method | Issue |
|---|------|------|--------|-------|
| DC-9 | AvatarOptionItemUI.cs | 104 | `OnDisable() { }` | Empty callback - remove |
| DC-10 | QuickMathController.cs | 1146-1148 | `OnGamePaused() { }` / `OnGameResumed() { }` | Empty overrides (acceptable - abstract base) |
| DC-11 | OddOneOutController.cs | 893, 895 | Same pattern | Same |
| DC-12 | MemoryPairsController.cs | 902, 904 | Same pattern | Same |

### Commented-Out Code Blocks

| # | File | Lines | Description |
|---|------|-------|-------------|
| DC-13 | TriumphManager.cs | 87-93 | Entire SDK initialization commented out (6 lines) |
| DC-14 | TriumphServices.cs | multiple | 19 TODO blocks with commented Triumph SDK calls |

**Dead Code Summary**: 4 unused fields, 4 dead event handlers, 4 empty methods, 2 large commented blocks.

---

## CA-7: DUPLICATE / ORPHAN PREFABS

### Orphan Prefabs (Not Referenced)

| # | Prefab | Path | Status |
|---|--------|------|--------|
| DP-1 | **MissionItem.prefab** | Prefabs/Monetization/DailyMissions/ | ORPHANED - replaced by MissionCard.prefab. No manager references it |
| DP-2 | **TournamentItem.prefab** | Prefabs/Tournaments/Browser/ | LEGACY FALLBACK only (TournamentManager lines 752, 827) - replaced by TournamentSearchItem + TournamentMyItem |

### Orphan Scripts

| # | Script | Path | Status |
|---|--------|------|--------|
| DP-3 | **MissionItemUI.cs** | Scripts/UI/Items/DailyMissions/ | Component for orphan MissionItem.prefab - unused |

### Deleted Assets (Staged in Git - Safe to Commit)

| # | Asset | Count | Status |
|---|-------|-------|--------|
| DP-4 | Mission icons (Art/Icons/Missions/) | 7 pairs (png+meta) | NOT referenced in code - safe to delete |
| DP-5 | LockIcon.png (Art/Icons/Navigation/) | 1 pair | Code uses Resources/ versions instead |

### Intentional Duplications (NOT issues)

- WinPanel prefabs in 2 locations: Prefabs/Games/ (practice) vs Resources/Prefabs/ (online/cash) → different game modes
- CashBattleIcon.png in 2 locations: CashBattle/UI/ vs Onboarding/ → different contexts
- Logo files in Art/ and Resources/ → different loading systems

**All 39 scenes are actively referenced.** No unused scene files found.

---

## CA-8: SPELLING ERRORS

| # | Location | Error | Correct | Instances |
|---|----------|-------|---------|-----------|
| SP-1 | CashBattleOnboarding | "Triump" | "Triumph" | 3 (CBO3, CBO5, CBO17) |
| SP-2 | Translations.txt (Settings) | "Terminos" | "Términos" | 1 |
| SP-3 | Translations.txt (Settings) | "Politica" | "Política" | 1 |

---

## COMPLETE STATS SUMMARY (Visual + Code Audit)

| Category | Count | Severity |
|----------|-------|----------|
| **MOCK-1 to MOCK-21**: Mock/Simulation services | 21 issues | 8 CRITICAL, 7 HIGH, 5 MEDIUM, 1 LOW |
| **FS-1 to FS-11**: Font sizes < 22px | 11 issues | 10 CRITICAL, 1 MEDIUM |
| **AL-1 to AL-13**: AutoLocalizer problems | 13 issues | 3 duplicate keys, 3 missing keys, 6 files hardcoded Spanish |
| **BF-1 to BF-2**: Bold enforcement | 2 issues | 2 HIGH |
| **LG-1 to LG-11**: Async void (logic risk) | 11 issues | 11 HIGH |
| **DC-1 to DC-14**: Dead/unreferenced code | 14 issues | 4 unused fields, 4 dead handlers, 4 empty methods, 2 comment blocks |
| **DP-1 to DP-5**: Duplicate/orphan assets | 5 issues | 2 orphan prefabs, 1 orphan script, 2 deleted assets pending commit |
| **SP-1 to SP-3**: Spelling | 3 issues | 5 instances total |
| **P0-P3 (Visual)**: Original visual audit | ~190 issues | 14 P0, ~40 P1, ~130 P2, 5 P3 |
| **GRAND TOTAL** | **~270 issues** | **Visual: ~190 + Code: ~80** |

---

## PRODUCTION READINESS VERDICT

### BLOCKERS for App Store (Must fix before submission):
1. **Mock services active by default** (MOCK-1 to MOCK-8) - App runs on fake backend
2. **Triumph SDK not implemented** (MOCK-9 to MOCK-13) - Production mode crashes
3. **"DBG" debug label visible** (P0-1) - App Store rejection
4. **Placeholder text visible** (P0-5, P0-6) - "Rule 1", "Mission name"
5. **Broken loading indicators** (P0-2) - White/cyan squares everywhere
6. **fontSizeMin=10** (FS-1 to FS-9) - Text unreadable on mobile
7. **Raw AutoLocalizer keys visible** (P0-3, P0-4) - Untranslated key strings shown to users

### HIGH PRIORITY (Should fix before submission):
1. Hardcoded Spanish strings (AL-7 to AL-12) - 13+ strings not localized
2. Bold enforcement violations (BF-1, BF-2)
3. Missing translation keys (AL-4 to AL-6) - will show raw keys
4. Async void methods (LG-1 to LG-11) - silent failures
5. CurrencyPills wrong color (P1-1)
6. Obsolete Win/Lose panels in minigames (P1-2)

### CAN SHIP WITH (Fix post-launch):
1. Dead code cleanup (DC-1 to DC-14)
2. Orphan prefabs (DP-1 to DP-3)
3. Spelling fixes (SP-1 to SP-3)
4. Empty method overrides (DC-10 to DC-12)

---
---

# CA-9: FIREBASE COVERAGE AUDIT - What's REAL vs FAKE vs MISSING

**Every system must work with a real backend before App Store submission.**

---

## FIREBASE STATUS MAP (Complete App)

### REAL (Firebase Functional) - 4 Systems

| # | System | Service File | Firebase Product | Status |
|---|--------|-------------|-----------------|--------|
| FB-R1 | **Authentication** | AuthenticationService.cs | Firebase Auth | REAL - Email/password, Google OAuth, Apple OAuth |
| FB-R2 | **Analytics** | AnalyticsService.cs | Firebase Analytics | REAL - All events logged, ATT-aware, GDPR toggle |
| FB-R3 | **Push Notifications** | NotificationService.cs | Firebase Cloud Messaging | REAL - Token registration, topic subscriptions |
| FB-R4 | **Leaderboards** | DatabaseService.cs | Firebase Realtime DB | REAL - Global + country leaderboards, score saving |

### LOCAL ONLY (PlayerPrefs - NO Firebase sync) - 6 Systems

| # | System | File | PlayerPrefs Key | What's Lost if User Changes Device |
|---|--------|------|-----------------|-------------------------------------|
| FB-L1 | **Achievements** | AchievementService.cs | `Achievements_Progress` | ALL 52 achievement progress lost |
| FB-L2 | **Daily Missions** | DailyMissionsManager.cs | `DM_State_v2` | Active missions, progress, streaks lost |
| FB-L3 | **Daily Rewards** | DailyRewardService.cs | `DailyReward_Data` | Consecutive days, claim history lost |
| FB-L4 | **Friends List** | FriendService.cs | `FriendRequests` | ALL friends and requests lost |
| FB-L5 | **Match History** | MatchHistoryData.cs | `MatchHistory_Entries` | Up to 100 match records lost |
| FB-L6 | **Virtual Currency** | CurrencyManager.cs | `DigitCoins` / `DigitGems` | ALL coins and gems lost (including purchased gems!) |

**CRITICAL PROBLEM with FB-L6**: If a user buys gems with real money (IAP) and then changes phone, their gems are GONE. Apple/Google will flag this - purchased items must be restorable.

### HYBRID (Firebase + Local Fallback) - 2 Systems

| # | System | Files | Firebase Part | Local Part |
|---|--------|-------|--------------|------------|
| FB-H1 | **User Profile** | AuthenticationService.cs + DatabaseService.cs | Username, avatar, country saved to `players/{userId}` | PlayerPrefs fallback if Firebase offline |
| FB-H2 | **Tournaments** | DatabaseService.cs | Tournament data at `tournaments/{id}` | In-memory cache + PlayerPrefs fallback |

### MOCK (Completely Fake) - 4 Systems

| # | System | Mock File | What It Fakes | Real Backend Needed |
|---|--------|-----------|---------------|---------------------|
| FB-M1 | **Wallet** | MockWalletService.cs | Deposits, withdrawals, balance tracking | Triumph SDK or Stripe |
| FB-M2 | **KYC Verification** | MockKYCService.cs | Age + identity verification | Triumph SDK or Jumio/Onfido |
| FB-M3 | **Matchmaking (Cash)** | MockMatchmakingService.cs | 1v1 opponent matching, entry fees | Triumph SDK or custom Firebase |
| FB-M4 | **Tournaments (Cash)** | MockTournamentService.cs | Cash prize tournaments | Triumph SDK or custom Firebase |

### MISSING (Not Implemented At All) - 3 Systems

| # | System | Impact | Needed For |
|---|--------|--------|-----------|
| FB-X1 | **Advertisement (AdMob)** | No ad revenue stream | Free-to-play monetization |
| FB-X2 | **Server-side Receipt Validation** | IAP fraud risk | App Store compliance |
| FB-X3 | **Remote Config** | No A/B testing, no kill switch | Live ops, emergency updates |

---

## WHAT MUST MIGRATE TO FIREBASE BEFORE APP STORE

### MANDATORY (App Store will reject or users will lose data):

| Priority | System | Current | Target | Effort | Why Mandatory |
|----------|--------|---------|--------|--------|---------------|
| **1** | **Virtual Currency (Gems)** | PlayerPrefs | Firebase Realtime DB | MEDIUM | Users BUY gems with real $. Apple requires purchased items to be restorable via Restore Purchases. If gems are only in PlayerPrefs, device change = money lost = App Store rejection + refund storm |
| **2** | **Achievements** | PlayerPrefs | Firebase Realtime DB | MEDIUM | Cross-device sync expected. Game Center/Play Games integration expected |
| **3** | **Friends List** | PlayerPrefs | Firebase Realtime DB | MEDIUM | Friends are TWO-WAY relationships - currently only stored on sender's device. Recipient never sees requests in production |
| **4** | **Match History** | PlayerPrefs | Firebase Realtime DB | LOW | Users expect history across devices |
| **5** | **Daily Rewards** | PlayerPrefs | Firebase Realtime DB | LOW | Prevent exploitation (reinstall = reset streak for free rewards) |
| **6** | **Daily Missions** | PlayerPrefs | Firebase Realtime DB | LOW | Same exploitation risk |

### REQUIRED FOR CASHBATTLE (Before enabling real money):

| Priority | System | Current | Target | Effort | Why Required |
|----------|--------|---------|--------|--------|--------------|
| **7** | **Wallet/Payments** | Mock | Triumph SDK | HIGH | Legal requirement for real money gaming |
| **8** | **KYC** | Mock | Triumph SDK | HIGH | Legal compliance - age/identity verification |
| **9** | **Cash Matchmaking** | Mock | Triumph SDK | HIGH | Can't match real players with fake opponent generator |
| **10** | **Cash Tournaments** | Mock | Triumph SDK | HIGH | Can't run real prize tournaments with fake data |
| **11** | **Geolocation** | Hardcoded "California" | Triumph SDK | MEDIUM | State-by-state gambling restrictions |

---

## CA-10: MONETIZATION STRATEGY

### Current Revenue Streams (What's REAL and working):

| # | Stream | Status | Implementation | Revenue Model |
|---|--------|--------|---------------|--------------|
| MON-1 | **Gem Packs (IAP)** | REAL | PremiumManager.cs + Unity IAP + StoreKit/Google Play Billing | 6 packs: $0.99 → $49.99 |
| MON-2 | **Premium Features (IAP)** | REAL | CreateTournaments ($3.99), CashBattleCreate ($6.99), TournamentBundle ($8.99) | One-time purchases |
| MON-3 | **Premium Themes (IAP)** | REAL | 15 premium themes × $2.50 each, or PremiumBundle ($26.25) / CompleteBundle ($30.45) | One-time purchases |
| MON-4 | **Virtual Betting** | REAL | Gem/Coin escrow system in CurrencyManager.cs | Players bet virtual currency on matches |

### NOT Yet Revenue (Needs Work):

| # | Stream | Status | What's Needed | Revenue Potential |
|---|--------|--------|--------------|-------------------|
| MON-5 | **CashBattle (Real Money)** | MOCK | Triumph SDK integration | Platform takes 10% fee per match |
| MON-6 | **Cash Tournaments** | MOCK | Triumph SDK integration | Entry fee commission |
| MON-7 | **Advertisements** | MISSING | AdMob SDK integration | Rewarded videos, interstitials, banners |
| MON-8 | **Subscriptions** | MISSING | Unity IAP subscription product | Monthly VIP / Premium pass |

### How Payments Work - Explained:

**For Virtual Items (Gems, Themes, Features):**
- Apple App Store / Google Play handle ALL payment processing
- You set prices in App Store Connect / Google Play Console
- Apple takes 30% commission (15% if <$1M/year via Small Business Program)
- Google takes 15% first $1M, then 30%
- Money goes to your bank account via App Store Connect / Google Play Console
- NO Stripe needed - the stores ARE your payment processor

**For Real Money Gaming (CashBattle):**
- Triumph SDK handles ALL real money operations
- Triumph provides: wallet, deposits, withdrawals, KYC, matchmaking, prize distribution
- Triumph takes a commission (varies by contract)
- Triumph handles legal compliance (gambling licenses, state restrictions)
- You do NOT need Stripe - Triumph IS the payment processor for cash gaming
- WITHOUT Triumph: CashBattle section must be HIDDEN or DISABLED in V1

### Recommended Launch Strategy:

**V1 (App Store Launch - NOW):**
1. Ship with IAP working (gems, themes, features) → ALREADY DONE
2. Migrate PlayerPrefs data to Firebase (achievements, friends, currency)
3. DISABLE CashBattle section entirely (hide from MainMenu)
4. Add AdMob for rewarded videos (watch ad = free gems/coins)
5. Receipt validation (server-side or at minimum local CrossPlatformValidator)

**V2 (Post-Launch - When Triumph SDK is ready):**
1. Enable CashBattle with real Triumph integration
2. Real KYC verification
3. Real wallet deposits/withdrawals
4. Real cash matchmaking and tournaments
5. Geo-restriction enforcement

---

## CA-11: FIREBASE PATHS NEEDED (Database Structure)

Current Firebase structure (DatabaseService.cs):
```
players/{userId}/          ← EXISTS (profile data)
leaderboards/global/       ← EXISTS (scores)
leaderboards/country/{cc}/ ← EXISTS (country scores)
tournaments/{id}/          ← EXISTS (tournament data)
```

Needs to be ADDED for full production:
```
players/{userId}/achievements/     ← NEW (52 achievements progress)
players/{userId}/friends/          ← NEW (friend list + requests)
players/{userId}/currency/         ← NEW (coins + gems balances)
players/{userId}/matchHistory/     ← NEW (last 100 matches)
players/{userId}/dailyRewards/     ← NEW (streak, lastClaim)
players/{userId}/dailyMissions/    ← NEW (active missions, progress)
players/{userId}/purchases/        ← NEW (IAP receipt log for restore)
players/{userId}/themes/           ← NEW (owned theme IDs)
players/{userId}/settings/         ← NEW (language, vibration, etc.)
```

---

## UPDATED GRAND TOTAL

| Category | Count | Severity |
|----------|-------|----------|
| **MOCK-1 to MOCK-21**: Mock/Simulation services | 21 issues | 8 CRITICAL, 7 HIGH |
| **FB-L1 to FB-L6**: Systems without Firebase (LOCAL only) | 6 systems | ALL need migration |
| **FB-M1 to FB-M4**: Completely mocked services | 4 services | Need Triumph SDK |
| **FB-X1 to FB-X3**: Missing systems | 3 systems | AdMob, Receipt Validation, Remote Config |
| **MON-5 to MON-8**: Missing revenue streams | 4 streams | Triumph, AdMob, Subscriptions |
| **FS-1 to FS-11**: Font sizes < 22px | 11 issues | 10 CRITICAL |
| **AL-1 to AL-13**: AutoLocalizer problems | 13 issues | 6 HIGH |
| **BF-1 to BF-2**: Bold enforcement | 2 issues | 2 HIGH |
| **LG-1 to LG-11**: Async void (logic risk) | 11 issues | 11 HIGH |
| **DC-1 to DC-14**: Dead/unreferenced code | 14 issues | LOW-MEDIUM |
| **DP-1 to DP-5**: Duplicate/orphan assets | 5 issues | LOW-MEDIUM |
| **SP-1 to SP-3**: Spelling | 3 issues | LOW |
| **P0-P3 (Visual)**: Original visual audit | ~190 issues | 14 P0, ~40 P1 |
| **GRAND TOTAL** | **~290+ issues** | **Visual: ~190 + Code: ~100** |

---

## PANEL COVERAGE AUDIT: Hidden Objects — "Panel No Cubre Sus Hijos"

**Date**: 2026-03-07
**Scope**: All 77 hidden-object screenshots analyzed for panel construction quality
**Critical question**: Does each panel's background/container properly cover ALL its child objects?

### Severity Definitions
- **CRITICAL**: No background at all, or panel has zero dimensions — completely broken
- **HIGH**: Children (buttons, text) extend outside panel bounds, or no blocker overlay on modal dialogs
- **MEDIUM**: Content very tight against edges, minor overflow, or overlay too transparent
- **LOW**: Minor visual polish issues, acceptable for launch

---

### SECTION 1: Core (00_Core)

| ID | Panel | Scene | Covers Children? | Severity | Issue | Fix |
|----|-------|-------|-----------------|----------|-------|-----|
| PC-1 | **PremiumPanel** | MainMenu | NO | HIGH | Tab bar labels (Achievements/Shop/Premium) sit below panel bottom edge; teal elements bleed outside right edge; panel content area appears empty | Expand panel RectTransform to include tab bar; investigate empty content area |
| PC-2 | **DeleteAccountPanel** | Settings | NO | HIGH | Cancel/Delete buttons extend below panel's dark background; Settings content (Vibration, Language) readable through panel | Increase panel height to contain buttons; increase background opacity to ≥0.85 |
| PC-3 | **LogoutConfirmPanel** | Settings | NO | HIGH | Cancel/Confirm buttons below panel background; Settings text bleeds through | Same fix as PC-2: increase height + opacity |
| PC-4 | **SelfExclusionPanel** | Settings | NO | HIGH | Cancel/Confirm buttons below panel background; Settings text bleeds through | Same fix as PC-2: increase height + opacity |
| PC-5 | **ThemeDropdown** | Settings | NO | MEDIUM | Cancel/Confirm buttons extend below panel background; Confirm button uses cyan (inconsistent with red/coral on other panels) | Increase height; standardize button color to match other confirm panels |

**Pattern**: All 4 Settings confirm panels share the same defect — panel background too short, buttons overflow below. Likely a shared template/prefab issue.

---

### SECTION 2: Auth (01_Auth)

| ID | Panel | Scene | Covers Children? | Severity | Issue | Fix |
|----|-------|-------|-----------------|----------|-------|-----|
| PC-6 | **LoadingIndicator** | AgeVerification | NO | CRITICAL | Only 60x60 yellow square — no fullscreen blocker, no spinner, no text. User can interact with UI behind it during loading | Rebuild as fullscreen stretch overlay (like Login LoadingPanel) with semi-transparent dark BG + centered spinner + "Verifying..." text |
| PC-7 | **LoadingPanel** | Login | YES | LOW | Properly fullscreen stretch, but overlay opacity could be higher; spinner is a basic square, not polished animation | Increase alpha; consider animated spinner |

---

### SECTION 3: Games (02_Games)

| ID | Panel | Scene | Covers Children? | Severity | Issue | Fix |
|----|-------|-------|-----------------|----------|-------|-----|
| PC-8 | **FeedbackPanel** | FlashTap | NO | HIGH | 600x70 — far too small, no Source Image, no blocker overlay. If Win/Lose sub-panels activate, they overflow completely | Convert to fullscreen stretch panel with semi-transparent dark overlay (like SettingsPanel pattern) |
| PC-9 | **FeedbackPanel** | MemoryPairs | NO | HIGH | 700x80 — too small, no background, no overlay. PlayAgainButton extends to panel edge | Same fix as PC-8 |
| PC-10 | **FeedbackPanel** | OddOneOut | NO | HIGH | 500x60 — too small, no background, no overlay. Win/Lose children would overflow if activated | Same fix as PC-8 |
| PC-11 | **FeedbackPanel** | QuickMath | NO | HIGH | 700x80 — too small, no background, no overlay | Same fix as PC-8 |
| PC-12 | **ComboContainer** | OddOneOut | YES | LOW | 180x45 — very tight for combo text but acceptable for its purpose | Monitor for text overflow with long combo strings |
| PC-13 | **ComboContainer** | QuickMath | YES | LOW | 250x60 — tight but acceptable | Same as PC-12 |
| PC-14 | **CountdownPanel** | OddOneOut | YES | — | Fullscreen stretch, properly constructed | OK |
| PC-15 | **SettingsPanel** | OddOneOut | YES | — | Fullscreen stretch, properly constructed | OK |
| PC-16 | **SettingsPanel** | QuickMath | YES | MEDIUM | Fullscreen stretch but "NORMAL" text truncated as "NOR..."; description text overlaps DIFFICULTY label | Enable auto-sizing on difficulty label (already tracked as QM4 P0) |
| PC-17 | **CognitiveSprintPanel** | GameSelector | NO | HIGH | "Start Sprint" button text clipped; subtitle text garbled/overlapping ("Select 835 games" artifact) | Fix subtitle text element; ensure button width accommodates full text |
| PC-18 | **RulesPanel** | GameSelector | YES | LOW | "Game Rules" subtitle has double-rendering artifact | Investigate duplicate text GameObjects stacked |
| PC-19 | **CountdownPanel** | Matchmaking | YES | — | Fullscreen stretch, properly constructed | OK |
| PC-20 | **ScreenFlash** | Matchmaking | YES | — | Fullscreen stretch, Raycast Target off (correct for flash effect) | OK |
| PC-21 | **VSContainer** | Matchmaking | YES | — | 200x200 decorative element, VS icon within bounds | OK |

**Pattern**: All 4 minigame FeedbackPanels (PC-8 to PC-11) are broken — tiny fixed-size rects that cannot contain their Win/Lose children. Need conversion to fullscreen stretch overlays.

---

### SECTION 4: CashBattle (03_CashBattle)

#### CashBattle1v1 Panels

| ID | Panel | Scene | Covers Children? | Severity | Issue | Fix |
|----|-------|-------|-----------------|----------|-------|-----|
| PC-22 | **CognitiveSprintPanel** | CashBattle1v1 | YES | MEDIUM | Panel covers children but NO fullscreen blocker overlay behind it — scene content tappable behind panel | Add fullscreen semi-transparent blocker behind panel |
| PC-23 | **GameSelectionModal** | CashBattle1v1 | YES | MEDIUM | Panel + blocker properly constructed; game names severely truncated ("Digit R...", "Flash T...") | Increase card width or enable auto-sizing (already tracked as CB1v1_2 P1) |
| PC-24 | **ConfirmBetPanel** | CashBattle1v1 | YES | HIGH | Panel covers children but NO fullscreen blocker overlay — financial confirmation dialog without interaction blocking | Add fullscreen blocker — CRITICAL for financial dialog |

#### CashHistory Panels

| ID | Panel | Scene | Covers Children? | Severity | Issue | Fix |
|----|-------|-------|-----------------|----------|-------|-----|
| PC-25 | **DetailPanel** | CashHistory | NO | CRITICAL | Panel is stretch-all but background is fully transparent/missing — content (Title, Subtitle, VICTORY, stats) floats over history list, text unreadable | Add opaque/semi-opaque dark background; add blocker overlay |
| PC-26 | **LoadingIndicator** | CashHistory | N/A | — | 80x80 centered spinner, not a panel | OK |
| PC-27 | **TransactionHistoryPanel** | CashHistory | NO | LOW | BonusBalanceText floats in ambiguous area; HistoryTabButton positioned off-screen (Y=-600) | Verify HistoryTabButton placement; contain BonusBalanceText within balance card |

#### CashMatchmaking Panels

| ID | Panel | Scene | Covers Children? | Severity | Issue | Fix |
|----|-------|-------|-----------------|----------|-------|-----|
| PC-28 | **CountdownPanel** | CashMatchmaking | YES | — | Fullscreen stretch, properly constructed | OK |
| PC-29 | **ScreenFlash** | CashMatchmaking | YES | — | Fullscreen stretch, Raycast Target off | OK |
| PC-30 | **MatchmakingPanel** | CashBattleHub | YES | HIGH | Panel covers children but NO fullscreen blocker overlay — user can tap menu items behind matchmaking dialog | Add fullscreen blocker to prevent navigation during matchmaking |

#### CashProfile Panels

| ID | Panel | Scene | Covers Children? | Severity | Issue | Fix |
|----|-------|-------|-----------------|----------|-------|-----|
| PC-31 | **ChangeNamePanel** | CashProfile | YES | MEDIUM | Panel covers children; blocker reference exists but no visible dimming at editor time; Save button very close to bottom edge | Verify blocker opacity at runtime; add padding below Save button |
| PC-32 | **ErrorPanel** | CashProfile | NO | CRITICAL | Width=0, Height=0 RectTransform — panel has zero dimensions. "Error" text + "Accept" button float uncontained over profile. No blocker overlay. Off-theme gray color | Full rebuild: set proper dimensions, add dark neon-themed background, add fullscreen blocker overlay |

#### CashTournamentLobby Panels

| ID | Panel | Scene | Covers Children? | Severity | Issue | Fix |
|----|-------|-------|-----------------|----------|-------|-----|
| PC-33 | **ChatBadge** | CashTournamentLobby | YES | — | 36x36 notification badge, properly positioned | OK |
| PC-34 | **LeaveBlocker** | CashTournamentLobby | YES | MEDIUM | Overlay too transparent — underlying lobby content fully readable; loading text floats with no backing card | Increase overlay alpha to ≥0.7; add centered card behind loading text |
| PC-35 | **LoadingOverlay** | CashTournamentLobby | YES | MEDIUM | Same transparency issue — underlying UI fully visible through overlay | Increase overlay alpha to ≥0.7 |
| PC-36 | **PrizesBlocker** | CashTournamentLobby | N/A | HIGH | MISLABELED screenshot — actually shows CashWallet LoadMoreButton overlapping transaction entries | Fix LoadMoreButton layout positioning in CashWallet |
| PC-37 | **StartingOverlay** | CashTournamentLobby | YES | MEDIUM | Overlay too transparent; countdown "3" is plain text with no visual emphasis | Increase overlay alpha; make countdown text larger/bolder with glow |

#### CashTournaments Panels

| ID | Panel | Scene | Covers Children? | Severity | Issue | Fix |
|----|-------|-------|-----------------|----------|-------|-----|
| PC-38 | **EmptyState** | CashTournaments | NO | HIGH | Shown simultaneously with tournament cards — logic bug (already tracked as CT3 P0) | Hide EmptyState when tournament list has items |
| PC-39 | **PremiumBlockPanel** | CashTournaments | NO | HIGH | Body text clipped on both sides ("reating tournaments requires...for fre-"); tournament card partially visible through top of panel | Increase panel width; enable text auto-sizing; ensure panel covers full list area behind it |
| PC-40 | **LoadingIndicator** | CashTournaments | N/A | — | Not visible as distinct panel in screenshots | Investigate at runtime |

#### CashWallet Panels

| ID | Panel | Scene | Covers Children? | Severity | Issue | Fix |
|----|-------|-------|-----------------|----------|-------|-----|
| PC-41 | **DepositPanel** | CashWallet | NO | HIGH | Close button (red X) extends outside panel top-right corner; NO blocker overlay behind panel | Move X button inward or expand panel; add fullscreen blocker |
| PC-42 | **WithdrawPanel** | CashWallet | NO | HIGH | Same close button overflow as DepositPanel; NO blocker overlay | Same fix as PC-41 |
| PC-43 | **KYCPanel** | CashWallet | YES | HIGH | Fullscreen stretch but severely underbuilt: no title text, no explanation, no close/back button. Single "Verify Identity" button floating on blank screen | Add title, explanation text, back button. Redesign as proper KYC onboarding panel |
| PC-44 | **LoadingOverlay** | CashWallet | YES | MEDIUM | Fullscreen but background too transparent — wallet content almost fully legible; no spinner, just "Processing..." text | Increase alpha to ≥0.7; add spinner animation |
| PC-45 | **SuccessOverlay** | CashWallet | YES | MEDIUM | Background too transparent; no success icon; no dismiss button or auto-dismiss | Increase alpha; add checkmark icon; add dismiss mechanism |
| PC-46 | **ErrorOverlay** | CashWallet | YES | HIGH | Background extremely transparent — almost invisible; no error message, no icon, no retry/dismiss button. Non-functional | Full rebuild: increase alpha, add error icon + message + retry button |

---

### SECTION 5: Monetization (04_Monetization)

| ID | Panel | Scene | Covers Children? | Severity | Issue | Fix |
|----|-------|-------|-----------------|----------|-------|-----|
| PC-47 | **DetailPanelBlocker** | Achievements | NO | MEDIUM | Close button (red X) extends outside panel top-right corner; yellow icon placeholder (no Source Image); blocker overlay present (good) | Move X button inward or expand panel rect; assign icon sprite |
| PC-48 | **RewardCelebration** | Achievements | NO | CRITICAL | NO background panel at all — icon, "Achievement Unlocked!" text, reward row, CONTINUE button all float over the achievement grid with no container. Blocker overlay appears transparent/invisible | Add dark rounded-corner card behind all child elements; set blocker overlay alpha to ≥0.7 |
| PC-49 | **RewardClaimBlocker** | DailyMissions | NO | MEDIUM | "Collect" button extends below panel background; reward row at very bottom edge clipping; cyan progress bar has no context | Expand panel height to contain Collect button with padding |
| PC-50 | **ClaimAnimationBlocker** | DailyRewards | NO | CRITICAL | NO background card — "Reward Obtained!", "+300 DigitCoins", gift icon, streak badge all float over the daily rewards grid. Blocker overlay alpha too low, underlying UI fully visible | Add dark rounded-corner card behind all elements; increase blocker alpha |
| PC-51 | **MilestoneBlocker** | DailyRewards | NO | CRITICAL | NO background card — yellow icon, "7 days in a row!", "+100 bonus DigitGems", CONTINUE button float over reward grid. No visible dimming | Same fix as PC-50 |
| PC-52 | **NotEnoughBlocker** | Shop | NO | MEDIUM | "Get DigitGems" button text clipped/truncated ("et DigitGem"); cyan icon placeholder has no sprite | Increase button width or enable auto-sizing; assign icon sprite |
| PC-53 | **PurchaseBlocker** | Shop | YES | — | Best-constructed panel of the group. Dark card properly contains all children. Blocker overlay correct | OK — minor: add more left/right padding |

---

### SECTION 6: Social (06_Social)

| ID | Panel | Scene | Covers Children? | Severity | Issue | Fix |
|----|-------|-------|-----------------|----------|-------|-----|
| PC-54 | **ChangeNamePanel** | Profile | YES | — | Panel background properly covers title, input field, Cancel/Save buttons. Has BlockerPanel reference | OK |
| PC-55 | **ErrorPanel** | Profile | NO | CRITICAL | Width=0, Height=0 RectTransform — zero dimensions. "Error" text + "Accept" button float uncontained. No blocker overlay. Off-theme gray color | Full rebuild: proper dimensions, dark neon background, fullscreen blocker (same prefab as PC-32) |
| PC-56 | **GameSelectionPanel** | Profile | YES | — | Fullscreen stretch, properly covers all 5 game buttons + Cancel. Has dimmed blocker | OK |
| PC-57 | **EmptyState** | Scores | YES | LOW | Content within bounds; raw translation keys visible in editor (runtime OK if AutoLocalizer resolves) | Verify keys resolve at runtime |
| PC-58 | **ErrorPanel** | Scores | NO | HIGH | Same ErrorPanel prefab — no blocker overlay, "Error" text not centered, off-theme gray. Game selector tabs tappable behind panel | Same rebuild as PC-55 |
| PC-59 | **LoadingPanel** | Scores | YES | MEDIUM | 300x150, no Source Image sprite, no blocker overlay, no spinner. Just "loading_rankings" text floating in tiny dark rect | Rebuild: increase size, add blocker overlay, add spinner animation |
| PC-60 | **EmptyState** | SearchPlayers | YES | LOW | Content within bounds; overlap with results is editor-time artifact (both enabled for inspection) | Verify mutual exclusivity at runtime |
| PC-61 | **LoadingIndicator** | SearchPlayers | N/A | LOW | 200x100 with just "Searching..." text — no background, no spinner. Acceptable as inline indicator | Add subtle spinner animation |
| PC-62 | **NoResultsText** | SearchPlayers | N/A | — | Single text element, not a panel | OK |

---

### SECTION 7: Tournaments (07_Tournaments)

| ID | Panel | Scene | Covers Children? | Severity | Issue | Fix |
|----|-------|-------|-----------------|----------|-------|-----|
| PC-63 | **ConfirmBlocker** | TournamentCreate | NO | MEDIUM | Buttons flush with/exceeding panel edges; almost no internal padding. Has fullscreen blocker (good) | Add 20-30px horizontal padding inside panel; increase panel width |
| PC-64 | **LoadingOverlay** | TournamentCreate | YES | LOW | Fullscreen stretch, content centered. Progress bar is raw cyan rect with no track | Add progress bar track/background for polish |
| PC-65 | **LeaveBlocker** | TournamentLobby | NO | HIGH | Title "Leave Tournament?" bleeds past panel left edge; body text extends to/past left/right edges; "Leave" button flush with right edge | Increase panel width by 40-60px; add horizontal padding |
| PC-66 | **LoadingOverlay** | TournamentLobby | YES | LOW | Same as PC-64 — raw progress bar | Add track background |
| PC-67 | **PrizesBlocker** | TournamentLobby | NO | MEDIUM | "Close" button flush with bottom edge; prize dollar amounts tight against right edge | Add bottom padding below Close button; add right padding for amounts |
| PC-68 | **StartingOverlay** | TournamentLobby | YES | — | Fullscreen stretch, countdown properly constructed | OK |
| PC-69 | **EmptyState** | TournamentsBrowser | YES | — | Inline content panel, properly contained | OK |
| PC-70 | **FilterPanel** | TournamentsBrowser | YES | MEDIUM | Content within bounds but NO tap-outside-to-dismiss blocker behind dropdown | Add fullscreen transparent blocker with Button component for dismiss |
| PC-71 | **LoadingIndicator** | TournamentsBrowser | N/A | LOW | 150x150 with icon + "Loading..." text, no background, no interaction blocking | Add subtle background or overlay |
| PC-72 | **LoadMoreButton** | TournamentsBrowser | YES | LOW | Button properly sized; minor concern about overlap with CreateTournamentFAB on small screens | Test on small device resolutions |
| PC-73 | **RefreshIndicator** | TournamentsBrowser | YES | — | Small horizontal strip, content within bounds | OK |

---

### PANEL COVERAGE AUDIT — SUMMARY

#### By Severity

| Severity | Count | Panel IDs |
|----------|-------|-----------|
| **CRITICAL** | 7 | PC-6, PC-25, PC-32, PC-48, PC-50, PC-51, PC-55 |
| **HIGH** | 17 | PC-1, PC-2, PC-3, PC-4, PC-8, PC-9, PC-10, PC-11, PC-17, PC-24, PC-30, PC-38, PC-39, PC-41, PC-42, PC-43, PC-46, PC-58, PC-65 |
| **MEDIUM** | 14 | PC-5, PC-16, PC-22, PC-23, PC-31, PC-34, PC-35, PC-37, PC-44, PC-45, PC-47, PC-49, PC-52, PC-59, PC-63, PC-67, PC-70 |
| **LOW** | 9 | PC-7, PC-12, PC-13, PC-18, PC-27, PC-57, PC-60, PC-61, PC-64, PC-66, PC-71, PC-72 |
| **OK (no issue)** | 16 | PC-14, PC-15, PC-19, PC-20, PC-21, PC-26, PC-28, PC-29, PC-33, PC-40, PC-53, PC-54, PC-56, PC-62, PC-68, PC-69, PC-73 |

#### Recurring Patterns (Fix Once, Fix Many)

| Pattern | Affected Panels | Root Cause | Single Fix |
|---------|----------------|------------|------------|
| **Settings confirm panels too short** | PC-2, PC-3, PC-4, PC-5 | Shared template — panel height doesn't include button row | Fix the shared ConfirmPanelUI prefab/template: increase height + opacity |
| **FeedbackPanels tiny fixed-size** | PC-8, PC-9, PC-10, PC-11 | All 4 minigame FeedbackPanels are 500-700x60-80 instead of fullscreen | Convert all to fullscreen stretch with dark overlay (like CountdownPanel pattern) |
| **No background card on celebration panels** | PC-48, PC-50, PC-51 | Children float with no container behind them | Add standard dark rounded-corner card to RewardCelebration, ClaimAnimationBlocker, MilestoneBlocker |
| **Close button (X) outside panel** | PC-41, PC-42, PC-47 | X button positioned at panel corner, extends beyond | Move X button 10-15px inward or expand panel RectTransform |
| **Overlay too transparent** | PC-34, PC-35, PC-37, PC-44, PC-45, PC-46 | Background Image alpha too low on overlay panels | Set overlay Image color alpha to ≥0.7 (180/255) across all overlays |
| **Missing blocker overlay on modals** | PC-1, PC-22, PC-24, PC-30, PC-41, PC-42, PC-70 | Modal/popup panels without fullscreen blocker behind them | Add stretch-all Image + Button component behind each modal |
| **ErrorPanel prefab broken** | PC-32, PC-55, PC-58 | Same prefab: zero dimensions, no blocker, off-theme gray | Rebuild ErrorPanel prefab: proper size, dark neon BG, fullscreen blocker |

#### Priority Fix Order

| Priority | Action | Panels | Impact |
|----------|--------|--------|--------|
| **1** | Rebuild ErrorPanel prefab | PC-32, PC-55, PC-58 | Fixes 3 CRITICAL/HIGH panels with 1 prefab change |
| **2** | Add background cards to celebration panels | PC-48, PC-50, PC-51 | Fixes 3 CRITICAL panels |
| **3** | Fix Settings confirm panel template height + opacity | PC-2, PC-3, PC-4, PC-5 | Fixes 4 HIGH panels with 1 template change |
| **4** | Convert minigame FeedbackPanels to fullscreen | PC-8, PC-9, PC-10, PC-11 | Fixes 4 HIGH panels |
| **5** | Rebuild AgeVerification LoadingIndicator | PC-6 | Fixes 1 CRITICAL panel |
| **6** | Add fullscreen blocker overlays to modals | PC-1, PC-22, PC-24, PC-30, PC-41, PC-42 | Fixes 6 HIGH panels |
| **7** | Rebuild CashHistory DetailPanel background | PC-25 | Fixes 1 CRITICAL panel |
| **8** | Increase overlay alpha across all overlays | PC-34, PC-35, PC-37, PC-44, PC-45, PC-46 | Fixes 6 MEDIUM panels |
| **9** | Fix CashWallet ErrorOverlay + KYCPanel content | PC-43, PC-46 | Fixes 2 HIGH panels |
| **10** | Fix panel padding/overflow on remaining | PC-39, PC-49, PC-52, PC-63, PC-65, PC-67 | Fixes 6 MEDIUM panels |
