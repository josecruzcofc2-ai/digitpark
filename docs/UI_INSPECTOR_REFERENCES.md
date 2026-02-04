# DigitPark - Referencias del Inspector por Escena

Esta documentacion lista todos los campos SerializeField que necesita cada Manager/Controller.
Para cada campo se indica el tipo de componente y el nombre sugerido para el objeto en el Hierarchy.

---

## INDICE

1. [Boot](#1-boot)
2. [Login](#2-login)
3. [Register](#3-register)
4. [AgeVerification](#4-ageverification)
5. [MainMenu](#5-mainmenu)
6. [Settings](#6-settings)
7. [Profile](#7-profile)
8. [Scores (Leaderboard)](#8-scores-leaderboard)
9. [SearchPlayers](#9-searchplayers)
10. [PlayModeSelection](#10-playmodeselection)
11. [GameSelector](#11-gameselector)
12. [Matchmaking](#12-matchmaking)
13. [DigitRush](#13-digitrush)
14. [CashBattleHub](#14-cashbattlehub)
15. [TournamentsBrowser](#15-tournamentsbrowser)
16. [TournamentCreate](#16-tournamentcreate)
17. [TournamentLobby](#17-tournamentlobby)
18. [Shop](#18-shop)
19. [DailyMissions](#19-dailymissions)
20. [DailyRewards](#20-dailyrewards)
21. [Achievements](#21-achievements)
22. [Onboarding](#22-onboarding)
23. [AudioManager (Global)](#23-audiomanager-global)

---

## 1. Boot
**Escena:** `Core/Boot.unity`
**Manager:** `BootManager`

| Campo | Tipo | Nombre Sugerido en Hierarchy |
|-------|------|------------------------------|
| `loadingBar` | Image | LoadingBar |
| `loadingText` | TextMeshProUGUI | LoadingText |
| `versionText` | TextMeshProUGUI | VersionText |

**Configuracion:**
- `minimumLoadTime` (float): 2f

---

## 2. Login
**Escena:** `Auth/Login.unity`
**Manager:** `LoginManager`

### UI - Login Panel
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `loginPanel` | GameObject | LoginPanel |
| `titleText` | TextMeshProUGUI | TitleText |
| `emailInput` | TMP_InputField | EmailInput |
| `passwordInput` | TMP_InputField | PasswordInput |
| `rememberToggle` | Toggle | RememberToggle |
| `loginButton` | Button | LoginButton |
| `googleButton` | Button | GoogleButton |
| `appleButton` | Button | AppleButton |
| `registerButton` | Button | RegisterButton |
| `forgotPasswordButton` | Button | ForgotPasswordButton |

### UI - Other
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `loadingPanel` | GameObject | LoadingPanel |
| `backButton` | Button | BackButton |

### UI - Panels
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `errorPanel` | ErrorPanelUI | ErrorPanel |

### Animation
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `titleAnimator` | Animator | TitleAnimator |

---

## 3. Register
**Escena:** `Auth/Register.unity`
**Manager:** `RegisterManager`

### UI - Title
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `titleText` | TextMeshProUGUI | TitleText |

### UI - Input Fields
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `usernameInput` | TMP_InputField | UsernameInput |
| `emailInput` | TMP_InputField | EmailInput |
| `passwordInput` | TMP_InputField | PasswordInput |
| `confirmPasswordInput` | TMP_InputField | ConfirmPasswordInput |

### UI - Buttons
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `createAccountButton` | Button | CreateAccountButton |
| `backButton` | Button | BackButton |

### UI - Loading
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `loadingPanel` | GameObject | LoadingPanel |

### UI - Panels
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `errorPanel` | ErrorPanelUI | ErrorPanel |

---

## 4. AgeVerification
**Escena:** `Auth/AgeVerification.unity`
**Manager:** `AgeVerificationManager`

### UI - Main
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `titleText` | TextMeshProUGUI | TitleText |
| `descriptionText` | TextMeshProUGUI | DescriptionText |
| `logoImage` | Image | LogoImage |

### UI - Date Input
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `dayDropdown` | TMP_Dropdown | DayDropdown |
| `monthDropdown` | TMP_Dropdown | MonthDropdown |
| `yearDropdown` | TMP_Dropdown | YearDropdown |
| `dateInputField` | TMP_InputField | DateInputField |

### UI - Buttons
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `verifyButton` | Button | VerifyButton |
| `backButton` | Button | BackButton |
| `termsButton` | Button | TermsButton |
| `privacyButton` | Button | PrivacyButton |

### UI - Checkbox
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `termsToggle` | Toggle | TermsToggle |
| `termsText` | TextMeshProUGUI | TermsText |

### UI - Status
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `statusText` | TextMeshProUGUI | StatusText |
| `loadingIndicator` | GameObject | LoadingIndicator |
| `successIcon` | GameObject | SuccessIcon |
| `errorIcon` | GameObject | ErrorIcon |

---

## 5. MainMenu
**Escena:** `Core/MainMenu.unity`
**Manager:** `MainMenuManager`

### UI - Main
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `mainMenuPanel` | GameObject | MainMenuPanel |
| `titleText` | TextMeshProUGUI | TitleText |
| `playButton` | Button | PlayButton |
| `scoresButton` | Button | ScoresButton |
| `cashBattleButton` | Button | CashBattleButton |
| `settingsButton` | Button | SettingsButton |

### UI - User
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `userButton` | Button | UserButton |
| `userText` | TextMeshProUGUI | UserText |
| `searchButton` | Button | SearchButton |

### UI - Premium
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `premiumButton` | Button | PremiumButton |
| `premiumBadge` | GameObject | PremiumBadge |
| `premiumPanel` | PremiumPanelUI | PremiumPanel |

### Animation
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `titleAnimator` | Animator | TitleAnimator |

---

## 6. Settings
**Escena:** `Core/Settings.unity`
**Manager:** `SettingsManager`

### UI - Main
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `settingsPanel` | GameObject | SettingsPanel |
| `titleText` | TextMeshProUGUI | TitleText |

### UI - Audio
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `soundVolumeSlider` | Slider | SoundVolumeSlider |
| `soundValueText` | TextMeshProUGUI | SoundValueText |
| `effectsVolumeSlider` | Slider | EffectsVolumeSlider |
| `effectsValueText` | TextMeshProUGUI | EffectsValueText |

### UI - Language
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `languageDropdown` | TMP_Dropdown | LanguageDropdown |
| `changeLangLabel` | TextMeshProUGUI | ChangeLangLabel |
| `languageStyler` | LanguageDropdownStyler | LanguageStyler |

### UI - Theme
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `themeDropdown` | TMP_Dropdown | ThemeDropdown |
| `changeThemeLabel` | TextMeshProUGUI | ChangeThemeLabel |

### UI - Account Buttons
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `changeNameButton` | Button | ChangeNameButton |
| `logoutButton` | Button | LogoutButton |
| `deleteAccountButton` | Button | DeleteAccountButton |
| `backButton` | Button | BackButton |

### UI - Premium Section
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `premiumSection` | GameObject | PremiumSection |
| `removeAdsButton` | Button | RemoveAdsButton |
| `removeAdsButtonText` | TextMeshProUGUI | RemoveAdsButtonText |
| `premiumFullButton` | Button | PremiumFullButton |
| `premiumFullButtonText` | TextMeshProUGUI | PremiumFullButtonText |
| `restorePurchasesButton` | Button | RestorePurchasesButton |
| `premiumButton` | Button | PremiumButton |
| `premiumBadge` | GameObject | PremiumBadge |
| `premiumPanel` | PremiumPanelUI | PremiumPanel |

### UI - Panels
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `changeNamePanel` | InputPanelUI | ChangeNamePanel |
| `deleteConfirmPanel` | ConfirmPanelUI | DeleteConfirmPanel |
| `logoutConfirmPanel` | ConfirmPanelUI | LogoutConfirmPanel |
| `errorPanel` | ErrorPanelUI | ErrorPanel |

---

## 7. Profile
**Escena:** `Social/Profile.unity`
**Manager:** `ProfileManager`

### UI - Header
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `backButton` | Button | BackButton |
| `addFriendIconButton` | Button | AddFriendIconButton |

### UI - Profile Info
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `usernameText` | TextMeshProUGUI | UsernameText |
| `avatarImage` | Image | AvatarImage |
| `statusText` | TextMeshProUGUI | StatusText |

### UI - Stats
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `totalGamesText` | TextMeshProUGUI | TotalGamesText |
| `winsText` | TextMeshProUGUI | WinsText |
| `winRateText` | TextMeshProUGUI | WinRateText |
| `bestTimeText` | TextMeshProUGUI | BestTimeText |
| `averageTimeText` | TextMeshProUGUI | AverageTimeText |

### UI - Game Records
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `digitRushValueText` | TextMeshProUGUI | DigitRushValueText |
| `memoryPairsValueText` | TextMeshProUGUI | MemoryPairsValueText |
| `quickMathValueText` | TextMeshProUGUI | QuickMathValueText |
| `flashTapValueText` | TextMeshProUGUI | FlashTapValueText |
| `oddOneOutValueText` | TextMeshProUGUI | OddOneOutValueText |

### UI - Action Buttons
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `friendsButton` | Button | FriendsButton |
| `historyButton` | Button | HistoryButton |
| `challengeButton` | Button | ChallengeButton |

### UI - Game Selection Panel
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `gameSelectionPanel` | GameObject | GameSelectionPanel |
| `darkOverlayButton` | Button | DarkOverlayButton |
| `cancelButton` | Button | CancelButton |
| `digitRushButton` | Button | DigitRushButton |
| `memoryPairsButton` | Button | MemoryPairsButton |
| `quickMathButton` | Button | QuickMathButton |
| `flashTapButton` | Button | FlashTapButton |
| `oddOneOutButton` | Button | OddOneOutButton |

---

## 8. Scores (Leaderboard)
**Escena:** `Social/Scores.unity`
**Manager:** `LeaderboardManager`

### UI - Tabs
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `nacionalTab` | Button | NacionalTab |
| `mundialTab` | Button | MundialTab |

### UI - Content
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `leaderboardContainer` | Transform | LeaderboardContainer |
| `leaderboardEntryPrefab` | GameObject | LeaderboardEntryPrefab |
| `scrollRect` | ScrollRect | ScrollRect |

### UI - Loading
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `loadingPanel` | GameObject | LoadingPanel |
| `loadingText` | TextMeshProUGUI | LoadingText |

### UI - Navigation
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `backButton` | Button | BackButton |

### UI - Empty State
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `emptyState` | GameObject | EmptyState |
| `playButton` | Button | PlayButton |

### UI - Player Position
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `playerPositionPanel` | GameObject | PlayerPositionPanel |
| `positionNumberText` | TextMeshProUGUI | PositionNumberText |
| `positionTimeText` | TextMeshProUGUI | PositionTimeText |

---

## 9. SearchPlayers
**Escena:** `Social/SearchPlayers.unity`
**Manager:** `SearchPlayersManager`

### UI - Search
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `searchInputField` | TMP_InputField | SearchInputField |
| `searchButton` | Button | SearchButton |
| `clearButton` | Button | ClearButton |

### UI - Results
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `resultsContainer` | Transform | ResultsContainer |
| `playerItemPrefab` | GameObject | PlayerItemPrefab |
| `noResultsText` | TextMeshProUGUI | NoResultsText |
| `loadingIndicator` | GameObject | LoadingIndicator |

### UI - Navigation
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `backButton` | Button | BackButton |

---

## 10. PlayModeSelection
**Escena:** `Games/PlayModeSelection.unity`
**Manager:** `PlayModeSelectionManager`

### UI - Header
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `titleText` | TextMeshProUGUI | TitleText |
| `backButton` | Button | BackButton |

### UI - Mode Cards
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `soloCard` | Button | SoloCard |
| `oneVsOneCard` | Button | OneVsOneCard |
| `tournamentsCard` | Button | TournamentsCard |

### UI - Card Texts
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `soloTitleText` | TextMeshProUGUI | SoloTitleText |
| `soloDescText` | TextMeshProUGUI | SoloDescText |
| `oneVsOneTitleText` | TextMeshProUGUI | OneVsOneTitleText |
| `oneVsOneDescText` | TextMeshProUGUI | OneVsOneDescText |
| `tournamentsTitleText` | TextMeshProUGUI | TournamentsTitleText |
| `tournamentsDescText` | TextMeshProUGUI | TournamentsDescText |

### UI - Icons
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `soloIcon` | Image | SoloIcon |
| `oneVsOneIcon` | Image | OneVsOneIcon |
| `tournamentsIcon` | Image | TournamentsIcon |

---

## 11. GameSelector
**Escena:** `Games/GameSelector.unity`
**Manager:** `GameSelectorManager`

*(Revisar GameSelectorManager.cs para campos especificos)*

---

## 12. Matchmaking
**Escena:** `Games/Matchmaking.unity`
**Manager:** `MatchmakingManager`

### UI - Header
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `titleText` | TextMeshProUGUI | TitleText |
| `gameTypeText` | TextMeshProUGUI | GameTypeText |

### UI - Player Card
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `playerAvatar` | Image | PlayerAvatar |
| `playerNameText` | TextMeshProUGUI | PlayerNameText |
| `playerCard` | GameObject | PlayerCard |

### UI - Opponent Card
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `opponentAvatar` | Image | OpponentAvatar |
| `opponentNameText` | TextMeshProUGUI | OpponentNameText |
| `opponentCard` | GameObject | OpponentCard |
| `opponentSearchingIndicator` | GameObject | OpponentSearchingIndicator |

### UI - VS
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `vsContainer` | GameObject | VsContainer |
| `vsText` | TextMeshProUGUI | VsText |

### UI - Status
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `statusText` | TextMeshProUGUI | StatusText |
| `timerText` | TextMeshProUGUI | TimerText |

### UI - Searching
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `searchingSpinner` | GameObject | SearchingSpinner |
| `searchingRing` | Image | SearchingRing |

### UI - Countdown
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `countdownPanel` | GameObject | CountdownPanel |
| `countdownText` | TextMeshProUGUI | CountdownText |

### UI - Cancel
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `cancelButton` | Button | CancelButton |

---

## 13. DigitRush
**Escena:** `Games/DigitRush.unity`
**Manager:** `DigitRushController`

### UI - Grid
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `gridButtons` | Button[] | GridButton_0 a GridButton_8 (array de 9) |

### UI - Game Info
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `timerText` | TextMeshProUGUI | TimerText |
| `bestTimeText` | TextMeshProUGUI | BestTimeText |
| `comboText` | TextMeshProUGUI | ComboText |
| `playAgainButton` | Button | PlayAgainButton |
| `backButton` | Button | BackButton |

### UI - Effects
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `countdownUI` | CountdownUI | CountdownUI |
| `sparkleEffect` | UISparkleEffect | SparkleEffect |

### UI - Win Message
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `winMessagePanel` | GameObject | WinMessagePanel |
| `winMessageCanvasGroup` | CanvasGroup | WinMessageCanvasGroup |
| `successText` | TextMeshProUGUI | SuccessText |

### UI - Premium Banner
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `premiumBannerContainer` | GameObject | PremiumBannerContainer |
| `premiumBannerButton` | Button | PremiumBannerButton |
| `premiumBannerText` | TextMeshProUGUI | PremiumBannerText |

---

## 14. CashBattleHub
**Escena:** `CashBattle/CashBattleHub.unity`
**Manager:** `CashBattleManager`

### UI - Header
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `titleText` | TextMeshProUGUI | TitleText |
| `balanceText` | TextMeshProUGUI | BalanceText |
| `backButton` | Button | BackButton |

### UI - Main Panel
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `mainPanel` | GameObject | MainPanel |
| `battles1v1Card` | Button | Battles1v1Card |
| `cashTournamentsCard` | Button | CashTournamentsCard |
| `walletCard` | Button | WalletCard |
| `historyCard` | Button | HistoryCard |

### UI - Panels
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `gameSelectionPanel` | GameSelectionPanel | GameSelectionPanel |
| `tournamentListPanel` | TournamentListPanel | TournamentListPanel |

### UI - Age Verification
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `ageVerificationPanel` | GameObject | AgeVerificationPanel |
| `verifyAgeButton` | Button | VerifyAgeButton |
| `verificationStatusText` | TextMeshProUGUI | VerificationStatusText |
| `verificationTitleText` | TextMeshProUGUI | VerificationTitleText |
| `verificationDescText` | TextMeshProUGUI | VerificationDescText |

### UI - Matchmaking
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `matchmakingPanel` | GameObject | MatchmakingPanel |
| `matchmakingStatusText` | TextMeshProUGUI | MatchmakingStatusText |
| `cancelMatchmakingButton` | Button | CancelMatchmakingButton |

---

## 15. TournamentsBrowser
**Escena:** `Tournaments/TournamentsBrowser.unity`
**Manager:** `TournamentsBrowserManager`

### UI - Header
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `backButton` | Button | BackButton |
| `titleText` | TextMeshProUGUI | TitleText |
| `createTournamentButton` | Button | CreateTournamentButton |

### UI - Filters
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `gameTypeFilter` | TMP_Dropdown | GameTypeFilter |
| `entryFeeFilter` | TMP_Dropdown | EntryFeeFilter |
| `statusFilter` | TMP_Dropdown | StatusFilter |
| `searchInput` | TMP_InputField | SearchInput |
| `clearFiltersButton` | Button | ClearFiltersButton |

### UI - Tabs
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `allTournamentsTab` | Button | AllTournamentsTab |
| `myTournamentsTab` | Button | MyTournamentsTab |
| `featuredTab` | Button | FeaturedTab |

### UI - Content
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `tournamentsContainer` | Transform | TournamentsContainer |
| `tournamentItemPrefab` | GameObject | TournamentItemPrefab |
| `scrollRect` | ScrollRect | ScrollRect |
| `emptyStateText` | TextMeshProUGUI | EmptyStateText |
| `loadMoreButton` | Button | LoadMoreButton |

### UI - Loading
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `loadingIndicator` | GameObject | LoadingIndicator |
| `refreshIndicator` | GameObject | RefreshIndicator |

---

## 16. TournamentCreate
**Escena:** `Tournaments/TournamentCreate.unity`
**Manager:** `TournamentCreateManager`

### UI - Header
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `backButton` | Button | BackButton |
| `titleText` | TextMeshProUGUI | TitleText |

### UI - Tournament Name
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `tournamentNameInput` | TMP_InputField | TournamentNameInput |
| `nameCharCountText` | TextMeshProUGUI | NameCharCountText |

### UI - Game Selection
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `gameTypeDropdown` | TMP_Dropdown | GameTypeDropdown |
| `selectedGameIcon` | Image | SelectedGameIcon |

### UI - Entry Fee
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `entryFeeDropdown` | TMP_Dropdown | EntryFeeDropdown |
| `entryFeeSlider` | Slider | EntryFeeSlider |
| `customEntryFeeInput` | TMP_InputField | CustomEntryFeeInput |
| `entryFeeDisplayText` | TextMeshProUGUI | EntryFeeDisplayText |

### UI - Players
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `maxPlayersDropdown` | TMP_Dropdown | MaxPlayersDropdown |
| `estimatedPrizeText` | TextMeshProUGUI | EstimatedPrizeText |

### UI - Start Time
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `startTimeDropdown` | TMP_Dropdown | StartTimeDropdown |
| `startImmediatelyToggle` | Toggle | StartImmediatelyToggle |
| `scheduledTimeText` | TextMeshProUGUI | ScheduledTimeText |

### UI - Advanced Options
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `roundsDropdown` | TMP_Dropdown | RoundsDropdown |
| `timeLimitDropdown` | TMP_Dropdown | TimeLimitDropdown |
| `allowSpectatorsToggle` | Toggle | AllowSpectatorsToggle |
| `privateToggle` | Toggle | PrivateToggle |
| `privateCodeInput` | TMP_InputField | PrivateCodeInput |

### UI - Preview
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `previewPanel` | GameObject | PreviewPanel |
| `previewNameText` | TextMeshProUGUI | PreviewNameText |
| `previewGameText` | TextMeshProUGUI | PreviewGameText |
| `previewEntryText` | TextMeshProUGUI | PreviewEntryText |
| `previewPrizeText` | TextMeshProUGUI | PreviewPrizeText |
| `previewPlayersText` | TextMeshProUGUI | PreviewPlayersText |

### UI - Actions
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `createButton` | Button | CreateButton |
| `previewButton` | Button | PreviewButton |
| `createButtonText` | TextMeshProUGUI | CreateButtonText |
| `creationFeeText` | TextMeshProUGUI | CreationFeeText |

### UI - Loading
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `loadingOverlay` | GameObject | LoadingOverlay |
| `statusText` | TextMeshProUGUI | StatusText |

---

## 17. TournamentLobby
**Escena:** `Tournaments/TournamentLobby.unity`
**Manager:** `TournamentLobbyManager`

### UI - Header
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `backButton` | Button | BackButton |
| `tournamentNameText` | TextMeshProUGUI | TournamentNameText |
| `statusBadgeText` | TextMeshProUGUI | StatusBadgeText |
| `statusBadgeImage` | Image | StatusBadgeImage |

### UI - Tournament Info
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `gameTypeText` | TextMeshProUGUI | GameTypeText |
| `gameTypeIcon` | Image | GameTypeIcon |
| `entryFeeText` | TextMeshProUGUI | EntryFeeText |
| `prizePoolText` | TextMeshProUGUI | PrizePoolText |
| `playersCountText` | TextMeshProUGUI | PlayersCountText |
| `playersProgressBar` | Slider | PlayersProgressBar |
| `startTimeText` | TextMeshProUGUI | StartTimeText |
| `countdownText` | TextMeshProUGUI | CountdownText |

### UI - Rules
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `roundsText` | TextMeshProUGUI | RoundsText |
| `timeLimitText` | TextMeshProUGUI | TimeLimitText |
| `formatText` | TextMeshProUGUI | FormatText |

### UI - Prize Distribution
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `prizeDistributionContainer` | Transform | PrizeDistributionContainer |
| `prizeRowPrefab` | GameObject | PrizeRowPrefab |

### UI - Participants
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `participantsContainer` | Transform | ParticipantsContainer |
| `participantItemPrefab` | GameObject | ParticipantItemPrefab |
| `participantsHeaderText` | TextMeshProUGUI | ParticipantsHeaderText |
| `viewAllParticipantsButton` | Button | ViewAllParticipantsButton |

### UI - Chat
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `chatPanel` | GameObject | ChatPanel |
| `chatMessagesContainer` | Transform | ChatMessagesContainer |
| `chatInput` | TMP_InputField | ChatInput |
| `sendChatButton` | Button | SendChatButton |
| `chatScrollRect` | ScrollRect | ChatScrollRect |

### UI - Actions
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `joinButton` | Button | JoinButton |
| `leaveButton` | Button | LeaveButton |
| `shareButton` | Button | ShareButton |
| `readyButton` | Button | ReadyButton |
| `joinButtonText` | TextMeshProUGUI | JoinButtonText |

### UI - Overlays
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `loadingOverlay` | GameObject | LoadingOverlay |
| `statusText` | TextMeshProUGUI | StatusText |
| `startingOverlay` | GameObject | StartingOverlay |
| `startingCountdownText` | TextMeshProUGUI | StartingCountdownText |

---

## 18. Shop
**Escena:** `Monetization/Shop.unity`
**Manager:** `ShopManager`

### Tab References
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `_gemsTabButton` | Button | GemsTabButton |
| `_coinsTabButton` | Button | CoinsTabButton |
| `_themesTabButton` | Button | ThemesTabButton |
| `_offersTabButton` | Button | OffersTabButton |

### Content References
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `_gemsContent` | GameObject | GemsContent |
| `_coinsContent` | GameObject | CoinsContent |
| `_themesContent` | GameObject | ThemesContent |
| `_offersContent` | GameObject | OffersContent |

### Popups
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `_purchasePopup` | GameObject | PurchasePopup |
| `_notEnoughGemsPopup` | GameObject | NotEnoughGemsPopup |

### Popup UI
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `_popupItemIcon` | Image | PopupItemIcon |
| `_popupItemName` | TextMeshProUGUI | PopupItemName |
| `_popupItemPrice` | TextMeshProUGUI | PopupItemPrice |
| `_popupConfirmButton` | Button | PopupConfirmButton |
| `_popupCancelButton` | Button | PopupCancelButton |
| `_notEnoughCloseButton` | Button | NotEnoughCloseButton |
| `_notEnoughGetGemsButton` | Button | NotEnoughGetGemsButton |

### Navigation
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `_backButton` | Button | BackButton |

### Currency Display
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `_gemsDisplay` | CurrencyDisplayUI | GemsDisplay |
| `_coinsDisplay` | CurrencyDisplayUI | CoinsDisplay |
| `_headerGemsText` | TextMeshProUGUI | HeaderGemsText |
| `_headerCoinsText` | TextMeshProUGUI | HeaderCoinsText |

---

## 19. DailyMissions
**Escena:** `Monetization/DailyMissions.unity`
**Manager:** `DailyMissionsManager`

### UI - Header
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `backButton` | Button | BackButton |
| `titleText` | TextMeshProUGUI | TitleText |
| `refreshTimerText` | TextMeshProUGUI | RefreshTimerText |
| `totalPointsText` | TextMeshProUGUI | TotalPointsText |

### UI - Tabs
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `dailyTab` | Button | DailyTab |
| `weeklyTab` | Button | WeeklyTab |
| `specialTab` | Button | SpecialTab |

### UI - Progress
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `dailyProgressBar` | Slider | DailyProgressBar |
| `dailyProgressText` | TextMeshProUGUI | DailyProgressText |
| `bonusRewardText` | TextMeshProUGUI | BonusRewardText |
| `claimBonusButton` | Button | ClaimBonusButton |

### UI - Missions List
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `missionsContainer` | Transform | MissionsContainer |
| `missionItemPrefab` | GameObject | MissionItemPrefab |
| `scrollRect` | ScrollRect | ScrollRect |
| `emptyStateText` | TextMeshProUGUI | EmptyStateText |

### UI - Detail Panel
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `missionDetailPanel` | GameObject | MissionDetailPanel |
| `detailTitleText` | TextMeshProUGUI | DetailTitleText |
| `detailDescriptionText` | TextMeshProUGUI | DetailDescriptionText |
| `detailProgressBar` | Slider | DetailProgressBar |
| `detailProgressText` | TextMeshProUGUI | DetailProgressText |
| `detailRewardText` | TextMeshProUGUI | DetailRewardText |
| `claimRewardButton` | Button | ClaimRewardButton |
| `closeDetailButton` | Button | CloseDetailButton |

### UI - Reward Popup
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `rewardPopup` | GameObject | RewardPopup |
| `rewardPopupText` | TextMeshProUGUI | RewardPopupText |
| `rewardPopupIcon` | Image | RewardPopupIcon |

---

## 20. DailyRewards
**Escena:** `Monetization/DailyRewards.unity`
**Manager:** `DailyRewardsManager`

### UI - Header
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `backButton` | Button | BackButton |
| `titleText` | TextMeshProUGUI | TitleText |
| `streakText` | TextMeshProUGUI | StreakText |
| `nextResetText` | TextMeshProUGUI | NextResetText |

### UI - Current Day
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `currentDayHighlight` | GameObject | CurrentDayHighlight |
| `currentDayText` | TextMeshProUGUI | CurrentDayText |
| `currentDayRewardIcon` | Image | CurrentDayRewardIcon |
| `currentDayRewardText` | TextMeshProUGUI | CurrentDayRewardText |

### UI - Rewards Grid
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `rewardsContainer` | Transform | RewardsContainer |
| `rewardDayPrefab` | GameObject | RewardDayPrefab |

### UI - Claim
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `claimButton` | Button | ClaimButton |
| `claimButtonText` | TextMeshProUGUI | ClaimButtonText |
| `claimGlow` | GameObject | ClaimGlow |

### UI - Streak
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `bonusInfoText` | TextMeshProUGUI | BonusInfoText |
| `streakProgressBar` | Slider | StreakProgressBar |
| `streakBonusText` | TextMeshProUGUI | StreakBonusText |

### UI - Claim Animation
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `claimAnimationPanel` | GameObject | ClaimAnimationPanel |
| `claimRewardText` | TextMeshProUGUI | ClaimRewardText |
| `claimRewardIcon` | Image | ClaimRewardIcon |
| `claimParticles` | ParticleSystem | ClaimParticles |
| `continueButton` | Button | ContinueButton |

### UI - Milestone
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `milestonePanel` | GameObject | MilestonePanel |
| `milestoneText` | TextMeshProUGUI | MilestoneText |
| `milestoneBonusText` | TextMeshProUGUI | MilestoneBonusText |

### Icons (Sprites)
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `coinIcon` | Sprite | (Asset en Resources) |
| `gemIcon` | Sprite | (Asset en Resources) |
| `xpIcon` | Sprite | (Asset en Resources) |
| `mysteryIcon` | Sprite | (Asset en Resources) |

---

## 21. Achievements
**Escena:** `Monetization/Achievements.unity`
**Manager:** `AchievementsManager`

### UI - Header
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `backButton` | Button | BackButton |
| `titleText` | TextMeshProUGUI | TitleText |
| `totalPointsText` | TextMeshProUGUI | TotalPointsText |
| `completionText` | TextMeshProUGUI | CompletionText |

### UI - Tabs
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `allTab` | Button | AllTab |
| `inProgressTab` | Button | InProgressTab |
| `completedTab` | Button | CompletedTab |
| `secretTab` | Button | SecretTab |

### UI - Categories
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `categoriesContainer` | Transform | CategoriesContainer |
| `categoryHeaderPrefab` | GameObject | CategoryHeaderPrefab |

### UI - Achievements List
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `achievementsContainer` | Transform | AchievementsContainer |
| `achievementItemPrefab` | GameObject | AchievementItemPrefab |
| `scrollRect` | ScrollRect | ScrollRect |
| `emptyStateText` | TextMeshProUGUI | EmptyStateText |

### UI - Detail Panel
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `detailPanel` | GameObject | DetailPanel |
| `detailIcon` | Image | DetailIcon |
| `detailTitleText` | TextMeshProUGUI | DetailTitleText |
| `detailDescriptionText` | TextMeshProUGUI | DetailDescriptionText |
| `detailProgressBar` | Slider | DetailProgressBar |
| `detailProgressText` | TextMeshProUGUI | DetailProgressText |
| `detailRewardText` | TextMeshProUGUI | DetailRewardText |
| `claimRewardButton` | Button | ClaimRewardButton |
| `closeDetailButton` | Button | CloseDetailButton |

### UI - Reward Popup
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `rewardPopup` | GameObject | RewardPopup |
| `rewardPopupText` | TextMeshProUGUI | RewardPopupText |
| `rewardPopupIcon` | Image | RewardPopupIcon |

---

## 22. Onboarding
**Escena:** `Monetization/Onboarding.unity`
**Manager:** `OnboardingManager`

### UI - Header
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `skipButton` | Button | SkipButton |
| `skipButtonText` | TextMeshProUGUI | SkipButtonText |
| `backButton` | Button | BackButton |

### UI - Content
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `stepImage` | Image | StepImage |
| `titleText` | TextMeshProUGUI | TitleText |
| `descriptionText` | TextMeshProUGUI | DescriptionText |
| `characterContainer` | GameObject | CharacterContainer |
| `characterAnimator` | Animator | CharacterAnimator |

### UI - Navigation
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `nextButton` | Button | NextButton |
| `prevButton` | Button | PrevButton |
| `nextButtonText` | TextMeshProUGUI | NextButtonText |
| `dotsContainer` | Transform | DotsContainer |
| `dotPrefab` | GameObject | DotPrefab |

### UI - Progress
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `progressBar` | Slider | ProgressBar |
| `stepCounterText` | TextMeshProUGUI | StepCounterText |

### UI - Highlight
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `highlightOverlay` | GameObject | HighlightOverlay |
| `highlightTarget` | RectTransform | HighlightTarget |
| `highlightTooltipText` | TextMeshProUGUI | HighlightTooltipText |
| `tapToContinuePrompt` | GameObject | TapToContinuePrompt |

### UI - Name Input
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `nameInputPanel` | GameObject | NameInputPanel |
| `nameInput` | TMP_InputField | NameInput |
| `confirmNameButton` | Button | ConfirmNameButton |
| `nameErrorText` | TextMeshProUGUI | NameErrorText |

### UI - Avatar Selection
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `avatarSelectionPanel` | GameObject | AvatarSelectionPanel |
| `avatarContainer` | Transform | AvatarContainer |
| `avatarOptionPrefab` | GameObject | AvatarOptionPrefab |

### UI - Completion
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `completionPanel` | GameObject | CompletionPanel |
| `completionTitleText` | TextMeshProUGUI | CompletionTitleText |
| `completionMessageText` | TextMeshProUGUI | CompletionMessageText |
| `rewardText` | TextMeshProUGUI | RewardText |
| `startPlayingButton` | Button | StartPlayingButton |

### Sprites (Assets)
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `welcomeImage` | Sprite | (Asset en Resources) |
| `gamesImage` | Sprite | (Asset en Resources) |
| `cashBattleImage` | Sprite | (Asset en Resources) |
| `tournamentsImage` | Sprite | (Asset en Resources) |
| `rewardsImage` | Sprite | (Asset en Resources) |
| `socialImage` | Sprite | (Asset en Resources) |

---

## 23. AudioManager (Global)
**Escena:** Persistente (DontDestroyOnLoad)
**Manager:** `AudioManager`

### Audio Sources
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `musicSource` | AudioSource | MusicSource |
| `sfxSource` | AudioSource | SfxSource |

### Music Clips
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `mainMenuMusic` | AudioClip | (Asset en Resources) |
| `gameplayMusic` | AudioClip | (Asset en Resources) |
| `leaderboardMusic` | AudioClip | (Asset en Resources) |
| `tournamentMusic` | AudioClip | (Asset en Resources) |

### SFX Clips
| Campo | Tipo | Nombre Sugerido |
|-------|------|-----------------|
| `buttonClickSFX` | AudioClip | (Asset en Resources) |
| `correctTouchSFX` | AudioClip | (Asset en Resources) |
| `wrongTouchSFX` | AudioClip | (Asset en Resources) |
| `gameCompleteSFX` | AudioClip | (Asset en Resources) |
| `newRecordSFX` | AudioClip | (Asset en Resources) |
| `coinsSFX` | AudioClip | (Asset en Resources) |
| `levelUpSFX` | AudioClip | (Asset en Resources) |
| `tournamentJoinSFX` | AudioClip | (Asset en Resources) |

---

## Notas Importantes

1. **Prefabs**: Los campos que terminan en `Prefab` deben apuntar a prefabs en la carpeta `Assets/_Project/Prefabs/`

2. **Tipos Custom**: Campos como `ErrorPanelUI`, `ConfirmPanelUI`, `PremiumPanelUI`, etc. son componentes custom del proyecto

3. **Sprites/AudioClips**: Estos son assets, no objetos en el Hierarchy. Deben estar en `Assets/_Project/Art/` o `Assets/_Project/Audio/`

4. **Jerarquia sugerida**:
   ```
   Scene
   ├── Canvas
   │   ├── Header
   │   │   ├── BackButton
   │   │   └── TitleText
   │   ├── Content
   │   │   └── [Contenido especifico]
   │   ├── Footer
   │   │   └── [Botones de accion]
   │   └── Popups
   │       ├── LoadingPanel
   │       └── ErrorPanel
   └── [Manager]
   ```

5. **Convenciones de nombres**:
   - Buttons: `[Nombre]Button` (ej: BackButton, PlayButton)
   - Texts: `[Nombre]Text` (ej: TitleText, StatusText)
   - Panels: `[Nombre]Panel` (ej: LoadingPanel, MainPanel)
   - Inputs: `[Nombre]Input` (ej: EmailInput, SearchInput)
   - Containers: `[Nombre]Container` (ej: RewardsContainer)
