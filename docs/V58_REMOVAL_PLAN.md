# V58 — Plan de eliminación: Achievements, Tournaments, Social/Friends, DailyRewards, BattleCards
**Fecha**: 2026-03-25
**Objetivo**: Dejar la app limpia, sin sistemas muertos, lista para Firebase + pruebas + subir

---

## ESTADO ACTUAL

| Sistema | Archivos en disco | Código activo que lo referencia |
|---|---|---|
| Achievements | ✅ YA BORRADO | refs muertas en Translations.txt |
| Tournaments | ✅ YA BORRADO | refs muertas en Translations.txt |
| Friends | ✅ YA BORRADO | refs muertas en Translations.txt |
| **DailyRewards** | ❌ AÚN EXISTE | BootManager, MainMenuManager, SceneNavigator, BuildScenesConfigurator |
| **BattleCards** | ❌ AÚN EXISTE | BootManager, MatchmakingManager, ShopItemData, SceneNavigator |

---

## BLOQUE 1 — Borrar archivos DailyRewards

### Scripts Runtime
```
git rm -f Assets/_Project/Scripts/Runtime/Features/Monetization/DailyRewards/DailyRewardsManager.cs
git rm -f Assets/_Project/Scripts/Runtime/Features/Monetization/DailyRewards/DailyRewardsManager.cs.meta
git rm -f Assets/_Project/Scripts/Runtime/Features/Monetization/DailyRewards/RewardDayItemUI.cs
git rm -f Assets/_Project/Scripts/Runtime/Features/Monetization/DailyRewards/RewardDayItemUI.cs.meta
git rm -f Assets/_Project/Scripts/Runtime/Services/DailyRewardService.cs
git rm -f Assets/_Project/Scripts/Runtime/Services/DailyRewardService.cs.meta
```

### Scripts Editor
```
git rm -f Assets/_Project/Scripts/Editor/Monetization/DailyRewardsPremiumUIBuilder.cs
git rm -f Assets/_Project/Scripts/Editor/Monetization/DailyRewardsPremiumUIBuilder.cs.meta
```

### Escena
```
git rm -f Assets/_Project/Scenes/Monetization/DailyRewards.unity
git rm -f Assets/_Project/Scenes/Monetization/DailyRewards.unity.meta
```

### Prefabs (verificar si existen)
```
git rm -rf Assets/_Project/Prefabs/Monetization/DailyRewards/
```

### Folder meta orphans (si quedan vacíos)
```
git rm -f Assets/_Project/Scripts/Runtime/Features/Monetization/DailyRewards.meta
git rm -f Assets/_Project/Scripts/Editor/AutoAssigners/Monetization/DailyRewardsReferenceAssigner.cs
git rm -f Assets/_Project/Scripts/Editor/AutoAssigners/Monetization/DailyRewardsReferenceAssigner.cs.meta
```

---

## BLOQUE 2 — Borrar archivos BattleCards

### Scripts Runtime
```
git rm -f Assets/_Project/Scripts/Runtime/Features/Cosmetics/BattleCards/BattleCardData.cs
git rm -f Assets/_Project/Scripts/Runtime/Features/Cosmetics/BattleCards/BattleCardData.cs.meta
git rm -f Assets/_Project/Scripts/Runtime/Features/Cosmetics/BattleCards/BattleCardApplier.cs
git rm -f Assets/_Project/Scripts/Runtime/Features/Cosmetics/BattleCards/BattleCardApplier.cs.meta
git rm -f Assets/_Project/Scripts/Runtime/Features/Cosmetics/BattleCards/BattleCardService.cs
git rm -f Assets/_Project/Scripts/Runtime/Features/Cosmetics/BattleCards/BattleCardService.cs.meta
```

### Scripts Editor (si existe)
```
git rm -f Assets/_Project/Scripts/Editor/Cosmetics/BattleCards/BattleCardCatalogBuilder.cs
git rm -f Assets/_Project/Scripts/Editor/Cosmetics/BattleCards/BattleCardCatalogBuilder.cs.meta
```

### Folder meta orphans
```
git rm -rf Assets/_Project/Resources/BattleCards/
git rm -f Assets/_Project/Scripts/Runtime/Features/Cosmetics/BattleCards.meta
```

---

## BLOQUE 3 — Limpiar referencias en código activo

### A. BootManager.cs
**Remover**: inicialización de BattleCardService (líneas ~502–507)
```csharp
// BORRAR ESTE BLOQUE:
// BattleCardService — cosmético de matchmaking (independiente de ThemeManager)
if (DigitPark.Cosmetics.BattleCardService.Instance == null)
{
    GameObject bcObj = new GameObject("BattleCardService");
    bcObj.AddComponent<DigitPark.Cosmetics.BattleCardService>();
    Debug.Log("[Boot] BattleCardService creado");
}
```
**Remover**: sync de DailyRewards a Firebase (líneas ~389–403)
```csharp
// BORRAR ESTE BLOQUE:
string dailyData = PlayerPrefs.GetString("DailyReward_Data", "");
...
updates["dailyRewardsManager/lastClaim"] = ...
```
**Remover**: comentario "// Seasonal BattleCards + Monthly Frames + Limited Theme Variants" (~línea 527)

### B. SceneNavigator.cs
**Remover línea ~41**:
```csharp
public const string DAILY_REWARDS = "DailyRewards";
```
**Remover línea ~261** del enum `ShopTab` o equivalente:
```csharp
BattleCards
```

### C. ShopItemData.cs
**Remover del enum `ShopItemType` (~línea 20)**:
```csharp
BattleCard,         // BattleCard cosmético de matchmaking
```
**Remover case en `GrantRewards()` (~líneas 255–259)**:
```csharp
case ShopItemType.BattleCard:
    var bcService = DigitPark.Cosmetics.BattleCardService.Instance;
    if (bcService != null) bcService.UnlockCard(itemId);
    break;
```
**Remover case en shopTab switch (~líneas 334–335)**:
```csharp
case ShopItemType.BattleCard:
    shopTab = ShopTab.BattleCards;
```
**Remover** `ShopItemType.TemporaryDecoration` del enum (sin productos activos).
**Remover** `ShopItemType.BattleCard` handling en cualquier otro switch.

### D. MainMenuManager.cs
**Remover listener (~línea 92)**:
```csharp
dailyRewardsButton?.onClick.AddListener(OnDailyRewardsButtonClicked);
```
**Remover campo** (si aún existe):
```csharp
[SerializeField] private Button dailyRewardsButton;
```
**Remover método (~líneas 286–290)**:
```csharp
private void OnDailyRewardsButtonClicked()
{
    Debug.Log("[MainMenu] Navegando a DailyRewards");
    SceneNavigator.Instance?.NavigateTo("DailyRewards");
}
```

### E. MatchmakingManager.cs
**Remover todos los bloques BattleCard** (~líneas 312, 318–335, 741–758):
- `BattleCardService.Instance` calls ×2
- `BattleCardApplier` GetComponent calls ×4
- `ForEachBattleCardApplier()` método completo
- `PauseAnimation()` / `ResumeAnimation()` calls

---

## BLOQUE 4 — Limpiar Translation Keys

Ejecutar script Python para borrar de los 3 Translations.txt:

**Keys a borrar:**
```
daily_reward_title
daily_rewards
daily_reward_coins
daily_reward_special
daily_reward_claimed
daily_reward_day
daily_reward_streak
daily_reward_claim
(+ cualquier otra que empiece con daily_reward_)

friend_request
friend_add
friend_remove
friend_list
friend_pending
friend_accepted
friend_online
friend_offline
(+ cualquier otra que empiece con friend_)

achievement_title
achievement_locked
achievement_unlocked
achievement_progress
(+ cualquier otra que empiece con achievement_)

tournament_title
tournament_create
tournament_join
tournament_lobby
tournament_results
(+ cualquier otra que empiece con tournament_)
```

**Archivos a actualizar (los 3 deben quedar sincronizados):**
```
Assets/_Project/Scripts/Localization/Translations.txt
Assets/_Project/Localization/Translations.txt
Assets/_Project/Resources/Translations.txt
```

---

## BLOQUE 5 — Verificar BuildScenesConfigurator + AllScenesBatchBuilder

### BuildScenesConfigurator.cs
Buscar y eliminar entrada `DailyRewards.unity`.

### AllScenesBatchBuilder.cs
Buscar y eliminar entrada `DailyRewards.unity`.

### EffectsSetup.cs
Buscar y eliminar `DailyRewards.unity` si está en el array de escenas.

---

## BLOQUE 6 — Actualizar TAREAS_MANUALES.md

**Borrar tarea #2** (ReviewService App Store ID) — `ReviewService.cs` ya no existe.
**Borrar tarea #14** (Achievements.unity TrophyCard.prefab wiring) — escena ya no existe.
**Actualizar tarea #13** (AdMob) — marcar como "pendiente de decisión futura, sin bloquear lanzamiento".

---

## BLOQUE 7 — Actualizar docs/firebase/FIREBASE_USAGE_AUDIT.md

**Secciones a borrar o marcar como eliminadas:**
- Achievements (AchievementService, Firebase paths `players/{uid}/achievements/`)
- Tournaments (TournamentManager, Firebase paths `tournaments/`)
- Friends/Social (FriendService, Firebase paths `players/{uid}/friends/`)
- DailyRewards (DailyRewardService, Firebase path `players/{uid}/dailyRewardsManager/`)
- BattleCards (BattleCardService)

---

## ESCENAS FINALES POST-V58 (objetivo: 12 escenas)

| # | Escena | Estado |
|---|---|---|
| 1 | `_Core/Boot.unity` | ✅ |
| 2 | `_Core/MainMenu.unity` | ✅ |
| 3 | `_Core/Settings.unity` | ✅ |
| 4 | `Auth/Login.unity` | ✅ |
| 5 | `Auth/Register.unity` | ✅ |
| 6 | `Games/Minigames/DigitRush.unity` | ✅ |
| 7 | `Games/Minigames/FlashTap.unity` | ✅ |
| 8 | `Games/Minigames/MemoryPairs.unity` | ✅ |
| 9 | `Games/Minigames/OddOneOut.unity` | ✅ |
| 10 | `Games/Minigames/QuickMath.unity` | ✅ |
| 11 | `Games/Navigation/GameSelector.unity` | ✅ |
| 12 | `Games/Navigation/PlayModeSelection.unity` | ✅ (solo Practice + Ranked) |
| 13 | `Games/Navigation/BetSelection.unity` | ✅ |
| 14 | `Games/Navigation/Matchmaking.unity` | ✅ |
| 15 | `Monetization/Shop.unity` | ✅ |
| 16 | `Social/Profile/MatchHistory.unity` | ✅ |
| 17 | `Social/Profile/Scores.unity` | ✅ |
| 18 | `Onboarding/Onboarding.unity` | ✅ |
| ~~Monetization/DailyRewards.unity~~ | BORRAR | ❌ |

---

## SISTEMAS ACTIVOS POST-V58 (mínimos para lanzar)

✅ Firebase Auth (Email, Google, Apple)
✅ Firebase Realtime DB (scores, profiles, matchmaking)
✅ Firebase Analytics
✅ Apple IAP + Google Play IAP (gem packs + cosmetics)
✅ Shop (frames + titles + gem packs)
✅ Welcome Packs (D1–D5)
✅ 5 Minijuegos + 1v1 Ranked
✅ Leaderboard (Scores.unity)
✅ Match History
✅ Onboarding
✅ Settings
✅ Login/Register

---

## ORDEN DE EJECUCIÓN RECOMENDADO

```
1. BLOQUE 1 → git rm DailyRewards files
2. BLOQUE 2 → git rm BattleCards files
3. BLOQUE 3 → Edit activos: BootManager, SceneNavigator, ShopItemData, MainMenuManager, MatchmakingManager
4. BLOQUE 4 → Python script: borrar translation keys
5. BLOQUE 5 → Edit builders: BuildScenesConfigurator, AllScenesBatchBuilder, EffectsSetup
6. BLOQUE 6 → Edit TAREAS_MANUALES.md
7. BLOQUE 7 → Edit FIREBASE_USAGE_AUDIT.md
8. Verificar compilación → commit
```
