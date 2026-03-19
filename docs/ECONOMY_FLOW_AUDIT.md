# ECONOMY FLOW AUDIT — DigitPark
> Generado: 2026-03-19 | Montos extraídos directamente del código fuente

Documento completo del flujo económico: quién da monedas, quién las quita, cómo se apuestan, y todos los archivos involucrados.

---

## GLOSARIO DE MONEDAS

| Nombre | Símbolo | Tipo | Forma de obtener |
|---|---|---|---|
| **DigitCoins** | DC | Moneda de juego (soft currency) | Jugar, misiones, daily rewards, logros, progresión |
| **DigitGems** (Sparks) | DG | Moneda premium (hard currency) | **Solo compra IAP** con dinero real. Drip mínimo en milestones y misiones |
| **Dinero real USD** | $ | Cash Battle | Triumph SDK (fuera del alcance de CurrencyManager) |

> **Regla económica**: DG→DC exchange está **DESHABILITADO** (`PurchaseCoinsWithGems` es `[Obsolete]`). DG es moneda solo-compra.

---

## ARCHIVO CENTRAL

### `CurrencyManager.cs`
**Path**: `Runtime/Features/Monetization/Currency/CurrencyManager.cs`

Singleton que persiste entre escenas. Es el único punto de verdad para el saldo.

**Almacenamiento**: PlayerPrefs obfuscados con XOR (`dp_cg_v2`, `dp_cc_v2`) + HMAC-SHA256 de integridad (`dp_bal_hmac`). En paralelo sincroniza a Firebase (`players/{uid}/gems` + `players/{uid}/coins`).

**Saldo inicial**: `DEFAULT_COINS = 1000`, `DEFAULT_GEMS = 0`

**API pública**:

| Método | Qué hace |
|---|---|
| `AddCoins(int)` | Añade DC al saldo. Thread-safe (lock). Overflow-protected. |
| `SpendCoins(int)` | Descuenta DC. Emite `OnNotEnoughCoins` si falla. |
| `HasEnoughCoins(int)` | Consulta sin modificar. |
| `AddGems(int)` | Añade DG al saldo. Thread-safe. |
| `SpendGems(int)` | Descuenta DG. Emite `OnNotEnoughGems` si falla. |
| `HasEnoughGems(int)` | Consulta sin modificar. |
| `TrySpendGemsOrNavigateToShop(int)` | Intenta gastar DG; si no hay, navega al Shop. |
| `ProcessGemsPurchase(int gems, int bonus)` | Para IAP: llama `AddGems(gems + bonus)`. |
| `GrantDailyReward(int gems, int coins)` | Llama internamente `AddGems` + `AddCoins`. |
| `GrantMissionReward(int gems, int coins)` | Idem. |
| `GrantAchievementReward(int gems, int coins)` | Idem. |
| `EscrowCoins(int)` | **Apuesta**: descuenta DC y los pone en escrow. Persiste en PlayerPrefs (crash-safe). |
| `EscrowGems(int)` | **Apuesta**: descuenta DG y los pone en escrow. |
| `SettleBet(bool won)` | Liquida la apuesta. Victoria: devuelve `amount × 1.9` (rake 5%). Derrota: escrow se pierde. |
| `CancelEscrow()` | Devuelve el escrow al saldo (cancelación de partida). |
| `RestoreFromFirebaseValues(int, int)` | Llamado por BootManager en reinstalación. |
| `RestoreFromFirebaseAsync()` | Al init: toma el MAX(local, Firebase) para prevenir pérdida de datos. |

**Eventos**:
- `OnCoinsChanged(int newAmount, int delta)` — suscrito por `CurrencyDisplayUI`, `BetSelectionPanel`
- `OnGemsChanged(int newAmount, int delta)` — suscrito por `CurrencyDisplayUI`
- `OnNotEnoughCoins(int deficit)` — para mostrar UI de "no tienes suficiente"
- `OnNotEnoughGems(int deficit)` — idem

**Seguridad**:
- `_currencyLock` — previene double-spend en race conditions
- HMAC-SHA256 — detecta edición directa de PlayerPrefs
- XOR obfuscation — ofusca valores en disco
- Escrow keys en PlayerPrefs — sobrevive crashes entre escrow y settle
- Firebase como fuente de verdad al reinstalar

---

## FLUJO COMPLETO: FUENTES DE DC (DigitCoins ENTRADA)

### 1. PARTIDAS DE JUEGO — `GameSessionManager.cs`
**Path**: `Runtime/Features/Games/Core/GameSessionManager.cs`

Método `CalculatePostGameReward()` + `RegisterGameResult()`:

| Modo | Victoria/Completar | Derrota/Abandono |
|---|---|---|
| **Practice** | +30 DC (base) + 15 DC si bate personal best | 0 DC |
| **SingleGame (1v1)** | +50 DC | +15 DC |
| **Tournament** | +100 DC | +25 DC |
| **CognitiveSprint** | +60 DC | +15 DC |
| **Online (Ranked)** | 0 DC aquí → ver OnlineResultManager | 0 DC |
| **CashTournament** | 0 DC (dinero real only) | 0 DC |

### 2. RANKED 1v1 ONLINE — `OnlineResultManager.cs`
**Path**: `Runtime/Features/Games/Results/OnlineResultManager.cs`

Método `GrantRankedRewards()` — Economy Rebalance V55:

| Evento | DC |
|---|---|
| Derrota ranked | +5 DC |
| Victoria ranked | +15 DC |
| Victoria con perfect score (0 errores) | +15 + 25 = **+40 DC** |
| First Win of the Day (FWOTD) — primera victoria del día UTC | +50 DC extra |
| Victoria + FWOTD + perfect | **+90 DC** (máximo en una partida) |

> Target del diseño: ~185 DC/día para jugador activo (10 partidas, 6 victorias).
> FWOTD guardado en `PlayerPrefs["DP_FWOTD_LastDate"]` (UTC date string).

### 3. DAILY REWARDS — `DailyRewardsManager.cs`
**Path**: `Runtime/Features/Monetization/DailyRewards/DailyRewardsManager.cs`

Ciclo de 14 días (7+7, sin reset por días perdidos). Método `ApplyReward()`:

| Día | Tipo | DC | DG |
|---|---|---|---|
| 1 | coins | **50** | — |
| 2 | coins | **75** | — |
| 3 | coins | **125** | — |
| 4 | coins | **175** | — |
| 5 | coins | **250** | — |
| 6 | coins | **400** | — |
| 7 | mixed ⭐ | **750** | **3 DG** |
| 8 | coins | **50** | — |
| 9 | coins | **75** | — |
| 10 | coins | **125** | — |
| 11 | coins | **175** | — |
| 12 | coins | **250** | — |
| 13 | coins | **400** | — |
| 14 | mixed ⭐⭐ | **750** | **8 DG** |

> Ciclo completo (14 días): **3,650 DC** + **11 DG total**

### 4. DAILY MISSIONS — `DailyMissionsManager.cs`
**Path**: `Runtime/Features/Monetization/DailyMissions/DailyMissionsManager.cs`

Recompensas por misión: definidas en `MissionDefinitionSO` (configurable en Inspector).
Bonus por completar las 3 misiones diarias requeridas:
- **+100 DC** (`dailyBonusReward = 100`)
- **+1 DG** (`dailyBonusGems = 1`) — drip lento, máx 1 DG/día

Método `ClaimMissionReward()` → `CurrencyManager.Instance.GrantMissionReward(gems, coins)`
Método `ClaimDailyBonus()` → `CurrencyManager.Instance.GrantMissionReward(0, dailyBonusReward)` + `AddGems(dailyBonusGems)`

### 5. LOGROS — `AchievementService.cs`
**Path**: `Runtime/Services/AchievementService.cs`

Método de desbloqueo llama directamente `currency.AddCoins(achievement.rewardCoins)` y `currency.AddGems(achievement.rewardGems)`.

Los montos por logro están en los datos de cada `AchievementDefinition` (no hay tabla fija — varía por logro).
Ejemplo desde `OnboardingManager`: el logro `tutorial_complete` da +75 DC + 2 DG.

### 6. LEVEL UP REWARDS — `PlayerProgressionSystem.cs`
**Path**: `Runtime/Features/Monetization/Progression/PlayerProgressionSystem.cs`

Método `GrantLevelReward()` → `currency.AddCoins(coinAmount)` + `currency.AddGems(reward.bonusGems)`.

Tabla completa de recompensas por nivel:

| Nivel | Recompensa DC | Recompensa DG | Cosmético |
|---|---|---|---|
| 5 | — | 3 DG | Avatar: Beginner |
| 10 | — | 3 DG | Title: Novice |
| 15 | 500 DC | 2 DG | — |
| 20 | — | — | Avatar: Player |
| 25 | — | 5 DG | Title: Player |
| 30 | 1,000 DC | 3 DG | — |
| 40 | — | — | Avatar: Veteran |
| 50 | — | 25 DG ⭐ | Title: Veteran |
| 60 | — | — | Frame: Bronze |
| 65 | 50 DC | — | — |
| 70 | 75 DC | — | — |
| 75 | 2,000 DC | 8 DG | — |
| 85 | 100 DC | — | — |
| 90 | 100 DC | — | — |
| 95 | 100 DC | — | — |
| 100 | — | 10 DG | Title: Centurion |
| 105 | — | — | Avatar: Centurion |
| 110 | 100 DC | — | — |
| 115 | 100 DC | — | — |
| 120 | 150 DC | — | — |
| 125 | — | — | Frame: Silver |
| 130 | 150 DC | — | — |
| 140 | 150 DC | — | — |
| 150 | 5,000 DC | 8 DG | — |
| 160 | 150 DC | — | — |
| 170 | 200 DC | — | — |
| 175 | — | — | Title: Expert |
| 180 | 200 DC | — | — |
| 190 | 200 DC | — | — |
| 200 | — | 10 DG | Avatar: Expert |
| 250 | — | — | Frame: Gold |
| 300 | — | 8 DG | Title: Master |
| 350 | — | — | Avatar: Master |
| 400 | — | — | Frame: Platinum |
| 450 | — | — | Title: Grand Master |
| 475 | — | — | Avatar: Legend |
| 490 | — | — | Frame: Diamond |
| 500 | — | — | Title: Legend |

> Total acumulable (lifetime hasta nivel 500): ~**10,075 DC** + ~**85 DG** en moneda. Más cosméticos.

### 7. ONBOARDING — `OnboardingManager.cs`
**Path**: `Runtime/Features/Onboarding/OnboardingManager.cs`

Al completar el tutorial (slide final): `currency.AddCoins(500)` + `currency.AddGems(0)`.
> `completionRewardCoins = 500`, `completionRewardGems = 0` (configurable en Inspector)
> Adicionalmente dispara logro `tutorial_complete` → +75 DC +2 DG (vía AchievementService)

### 8. DAILY OFFER — `DailyOfferService.cs`
**Path**: `Runtime/Services/DailyOfferService.cs`

Si la oferta diaria incluye free reward: `currency?.AddCoins(offer.freeRewardAmount)`.
Monto depende de la oferta configurada (variable).

### 9. VICTORIA EN APUESTA — `CurrencyManager.SettleBet(true)`
Cuando el jugador gana una apuesta (BetSelectionPanel → GameSessionManager → SettleBet):
- Devuelve **`amount × 1.9`** (rake del 5% del premio total)
- Ejemplo: apuesta 100 DC → gana 190 DC netos

### 10. XP BOOST — `PlayerProgressionSystem.cs`
No da monedas directamente, pero activa `×1.25` en XP ganado:
- Fuente 1: streak de 30 días → boost de 7 días
- Fuente 2: Daily Offer con XP boost
- Stored en `PlayerPrefs["DP_XPBoost_Expiry"]`

---

## FLUJO COMPLETO: FUENTES DE DG (DigitGems ENTRADA)

| Fuente | Cantidad | Archivo |
|---|---|---|
| IAP — 150 Sparks | 150 DG | `ProductCatalog.cs` + `PaymentManager.cs` |
| IAP — 500 Sparks | 550 DG (10% bonus) | `ProductCatalog.cs` |
| IAP — 1,200 Sparks | 1,440 DG (20% bonus) | `ProductCatalog.cs` |
| IAP — 2,500 Sparks | 3,125 DG (25% bonus) | `ProductCatalog.cs` |
| IAP — 6,500 Sparks | 8,450 DG (30% bonus) | `ProductCatalog.cs` |
| IAP — 14,000 Sparks | 18,900 DG (35% bonus) | `ProductCatalog.cs` |
| Daily Reward día 7 | 3 DG | `DailyRewardsManager.cs` |
| Daily Reward día 14 | 8 DG | `DailyRewardsManager.cs` |
| Daily Missions bonus diario | 1 DG/día | `DailyMissionsManager.cs` |
| Level 5 | 3 DG | `PlayerProgressionSystem.cs` |
| Level 10 | 3 DG | `PlayerProgressionSystem.cs` |
| Level 15 | 2 DG | `PlayerProgressionSystem.cs` |
| Level 25 | 5 DG | `PlayerProgressionSystem.cs` |
| Level 30 | 3 DG | `PlayerProgressionSystem.cs` |
| Level 50 | 25 DG | `PlayerProgressionSystem.cs` |
| Level 75 | 8 DG | `PlayerProgressionSystem.cs` |
| Level 100 | 10 DG | `PlayerProgressionSystem.cs` |
| Level 150 | 8 DG | `PlayerProgressionSystem.cs` |
| Level 200 | 10 DG | `PlayerProgressionSystem.cs` |
| Level 300 | 8 DG | `PlayerProgressionSystem.cs` |
| Logro `tutorial_complete` | 2 DG | `AchievementService.cs` |
| Logros varios | variable | `AchievementService.cs` |
| Welcome Pack (D1–D3) | variable | `WelcomePackService.cs` |

> **Total DG gratuito (lifetime estimado hasta nivel 300, sin IAP)**: ~95 DG (excluyendo logros variables y WelcomePack)

---

## FLUJO COMPLETO: SINKS DE DC (DigitCoins SALIDA)

| Sink | Costo | Archivo |
|---|---|---|
| Apuesta preset 1 | 50 DC | `BetSelectionPanel.cs` → `CurrencyManager.EscrowCoins()` |
| Apuesta preset 2 | 100 DC | `BetSelectionPanel.cs` |
| Apuesta preset 3 | 250 DC | `BetSelectionPanel.cs` |
| Apuesta preset 4 | 500 DC | `BetSelectionPanel.cs` |
| Apuesta preset 5 | 1,000 DC | `BetSelectionPanel.cs` |
| Apuesta custom | 5–5,000 DC (múltiplos de 5) | `BetSelectionPanel.cs` |
| Compra en Shop (ítems que cuestan DC) | variable | `ShopItemData.cs` → `CurrencyManager.SpendCoins()` |

> Las apuestas pasan por **escrow** (no se descuentan hasta que el resultado llega). Si el jugador cancela, el escrow se devuelve via `CancelEscrow()`.

### Sistema de Apuestas (Escrow) — flujo completo:
```
BetSelectionPanel.OnPlayClicked()
  → CurrencyManager.EscrowCoins(amount)   ← DC salen del saldo, van a escrow
    → PlayerPrefs guarda escrow (crash-safe)
  → LoadScene("Matchmaking")
    → Juego se juega
  → GameSessionManager.RegisterGameResult()
    → CurrencyManager.SettleBet(won)
      Si won=true:  AddCoins(amount × 1.9)  ← DC vuelven multiplicados
      Si won=false: escrow se pierde
  // O si se cancela:
  → CurrencyManager.CancelEscrow()         ← DC devueltos al saldo
```

---

## FLUJO COMPLETO: SINKS DE DG (DigitGems SALIDA)

| Sink | Costo | Archivo |
|---|---|---|
| Compra de tema premium | variable (DG) | `ShopItemData.cs` → `CurrencyManager.SpendGems()` |
| Compra de cosméticos premium (BattleCards, Frames, Titles, Win Effects) | variable (DG) | `ShopItemData.cs` |
| Compra de Backgrounds (perfiles) | variable (DG) | `ShopItemData.cs` |
| Compra de Emotes | variable (DG) | `ShopItemData.cs` |
| `TrySpendGemsOrNavigateToShop()` | variable | `CurrencyManager.cs` → `ShopManager` si falla |

> Los temas `Emerald`, `Electric Blue`, `Electric Violet`, `Monochrome` tienen precio en DG (por definir). También desbloqueables por achievement épico.

---

## CATÁLOGO IAP COMPLETO — `ProductCatalog.cs`
**Path**: `Runtime/Payments/Core/ProductCatalog.cs`

### DigitGems (Sparks — consumable)

| ProductId | Display | Precio USD | DG | Bonus |
|---|---|---|---|---|
| `sparks_100` | 150 Sparks | $0.99 | 150 | 0% |
| `sparks_500` | 500 Sparks | $4.99 | 550 | +10% |
| `sparks_1200` | 1,200 Sparks | $9.99 | 1,440 | +20% |
| `sparks_2500` | 2,500 Sparks | $19.99 | 3,125 | +25% |
| `sparks_6500` | 6,500 Sparks | $49.99 | 8,450 | +30% |
| `sparks_14000` | 14,000 Sparks | $99.99 | 18,900 | +35% |

### Cosméticos (non-consumable)

| ProductId | Display | Precio USD | Tipo |
|---|---|---|---|
| `premium_bundle` | Premium Theme Bundle | $26.25 | Bundle temas premium |
| `complete_bundle` | Complete Theme Collection | $30.45 | Bundle completo |
| `frame_plasma_spark` | Plasma Spark Frame | $0.99 | Marco de perfil |
| `frame_prism_shift` | Prism Shift Frame | $0.99 | Marco |
| `frame_aurora_borealis` | Aurora Borealis Frame | $3.99 | Marco |
| `frame_void_walker` | Void Walker Frame | $5.99 | Marco |
| `frame_storm_surge` | Storm Surge Frame | $5.99 | Marco |
| `frame_cosmic_rift` | Cosmic Rift Frame | $9.99 | Marco |
| `frame_infernal_god` | Infernal God Frame | $9.99 | Marco |
| `frame_divine_light` | Divine Light Frame | $14.99 | Marco |
| `frame_quantum_break` | Quantum Break Frame | $14.99 | Marco |

### Flujo IAP → DG:
```
Usuario toca "Comprar" en Shop
  → ShopItemUI.cs / PremiumManager.cs
  → PaymentManager.cs — decide proveedor (Stripe vs Apple IAP)
  → AppleIAPProvider.cs O StripeCheckoutController.cs
  → [Pago procesado]
  → AppleReceiptValidator.cs → Cloud Function iapValidateReceipt
  → PaymentBridge.ProcessGemsPurchase?.Invoke(gems, bonus)
    → PaymentBridgeWiring.cs conecta al:
  → CurrencyManager.Instance.ProcessGemsPurchase(gems, bonus)
    → CurrencyManager.AddGems(total)
      → Persiste en PlayerPrefs + Firebase
```

---

## XP — SISTEMA DE PROGRESIÓN

**No da monedas directamente** pero desbloquea cosméticos y tiene level rewards (ver sección 6).

### Fórmula XP por nivel:
- Niveles 1–50: `baseXP(100) × 1.15^(nivel-1)`
- Niveles 51+: `xpAt50 × 1.12^(nivel-50)`
- Nivel 100 alcanzable en ~1.5 años de juego activo

### XP por actividad:

| Actividad | XP base |
|---|---|
| Cualquier partida completada | +25 XP |
| Victoria | +50 XP |
| Partida perfecta (0 errores) | +100 XP |
| Top 90% de score | ×1.25 |
| Top 75–89% de score | ×1.10 |
| Participación en torneo | +75 XP |
| Torneo top 3 | +200 XP |
| Victoria de torneo | +500 XP |
| CashBattle (cualquier modo) | ×0.5 (rate reducido) |
| Streak 30 días (7 días) | ×1.25 en todo XP |

### Flujo XP en GameSessionManager:
```
RegisterGameResult()
  → PlayerProgressionSystem.Instance.AddGameXP(xpResult)
    → CalculateGameXP() → XP base
    → Si isCashBattle: ×0.5
    → Si XPBoost activo: ×1.25
    → AddXP(amount) → CheckLevelUp()
      → Si sube nivel: GrantLevelReward() → AddCoins() / AddGems()
  → MissionsManager.Instance.ReportXPEarned(xpGained)
```

---

## ANIMACIONES DE MONEDA

| Archivo | Descripción |
|---|---|
| `Runtime/Animations/Animators/CurrencyAnimator.cs` | Anima el cambio de saldo (fly coins, número animado). |
| `Runtime/Animations/Animators/RewardClaimAnimator.cs` | Animación de reclamar recompensa (monedas volando al pill). |
| `Runtime/UI/Components/UIPolish.cs` | Efectos visuales de moneda (shake, glow, pulse). |
| `Runtime/Animations/Animators/MainMenuAnimator.cs` | Incluye animación del currency pill en main menu. |

---

## TODOS LOS ARCHIVOS QUE LLAMAN `AddCoins`/`AddGems`

| Archivo | Método/Contexto | DC/DG |
|---|---|---|
| `GameSessionManager.cs` | Post-game reward por modo | DC |
| `OnlineResultManager.cs` | Ranked rewards (win/loss/perfect/FWOTD) | DC |
| `DailyRewardsManager.cs` | Login rewards días 1-14 | DC + DG |
| `DailyMissionsManager.cs` | Claim misión + daily bonus | DC + DG |
| `AchievementService.cs` | Desbloqueo de logro | DC + DG |
| `PlayerProgressionSystem.cs` | Level up milestone | DC + DG |
| `OnboardingManager.cs` | Completar tutorial | DC |
| `DailyOfferService.cs` | Free reward de oferta diaria | DC |
| `WelcomePackService.cs` | Welcome Pack grant | DG |
| `ShopItemData.cs` | Compra de packs de moneda en Shop | DC o DG |
| `PaymentManager.cs` | IAP exitoso (via PaymentBridge) | DG |
| `PremiumManager.cs` | Pack premium con gems | DG |
| `DailyRewardService.cs` | Servicio de persistencia de daily rewards | DC + DG |
| `CurrencyManager.cs` | Todas las llamadas internas (GrantX, SettleBet) | DC + DG |

---

## TODOS LOS ARCHIVOS QUE LLAMAN `SpendCoins`/`SpendGems`/`EscrowX`

| Archivo | Método/Contexto | DC/DG |
|---|---|---|
| `BetSelectionPanel.cs` | Apuesta (EscrowCoins) | DC |
| `ShopItemData.cs` | Compra de ítem en Shop (SpendCoins/SpendGems) | DC o DG |
| `ShopManager.cs` | Coordina compras vía ShopItemData | DC o DG |
| `CurrencyManager.cs` | EscrowGems (apuestas en DG — legacy) | DG |
| `GameSessionManager.cs` | SettleBet / CancelEscrow tras resultado | DC o DG |

---

## TODOS LOS ARCHIVOS QUE LEEN EL SALDO (display/checks)

| Archivo | Qué lee |
|---|---|
| `CurrencyDisplayUI.cs` | `CurrencyManager.Coins` + `CurrencyManager.Gems` (tiempo real via eventos) |
| `BetSelectionPanel.cs` | `HasEnoughCoins()` para habilitar/deshabilitar botones |
| `ShopItemUI.cs` | `HasEnoughCoins()` / `HasEnoughGems()` para estado locked/available |
| `ShopManager.cs` | Balance para filtros de affordability |
| `PremiumPanelUI.cs` | Muestra saldo actual |
| `ProfileManager.cs` | Lee perfil (incluye coins/gems para mostrar en perfil) |
| `MainMenuManager.cs` | Muestra saldo en header |
| `DailyMissionsManager.cs` | Muestra reward vs saldo actual |
| `DailyRewardsManager.cs` | Muestra reward vs saldo |

---

## SEGURIDAD ANTI-CHEAT

| Mecanismo | Archivo | Descripción |
|---|---|---|
| **XOR obfuscation** | `CurrencyManager.cs` | PlayerPrefs keys `dp_cg_v2`, `dp_cc_v2` guardados con XOR salt |
| **HMAC-SHA256** | `CurrencyManager.cs` | Hash del balance almacenado en `dp_bal_hmac`. Si no coincide, reset local + espera Firebase |
| **Thread lock** | `CurrencyManager.cs` | `_currencyLock` previene double-spend concurrente |
| **Overflow protection** | `CurrencyManager.cs` | Clamp a `int.MaxValue` en sumas |
| **Firebase sync** | `CurrencyManager.cs` | Balance autoritativo en backend. Al reinstalar: `MAX(local, Firebase)` |
| **Escrow crash-safe** | `CurrencyManager.cs` | Escrow persiste en PlayerPrefs. Al boot, si hay escrow residual → refund automático |
| **validateScore** | `functions/src/index.ts` | Cloud Function: rate-limit 1 submit/30s por usuario. Anti-cheat de score. |
| **Bet rake** | `CurrencyManager.cs` | `BET_MULTIPLIER = 1.9` → 5% rake. Impide farming infinito de apuestas |
| **Custom bet cap** | `BetSelectionPanel.cs` | `MAX_CUSTOM_BET = 5,000 DC` — evita whale farming |

---

## MAPA DE DEPENDENCIAS ECONÓMICAS

```
[IAP / Stripe / Apple]
    └─→ PaymentManager
         └─→ PaymentBridge.ProcessGemsPurchase
              └─→ CurrencyManager.AddGems()
                   └─→ PlayerPrefs (XOR+HMAC) + Firebase sync

[Partida completada]
    └─→ GameSessionManager.RegisterGameResult()
         ├─→ CurrencyManager.AddCoins()  [post-game reward]
         ├─→ CurrencyManager.SettleBet() [si había apuesta]
         └─→ PlayerProgressionSystem.AddGameXP()
              └─→ [Si level up] CurrencyManager.AddCoins() / AddGems()

[Login diario]
    └─→ DailyRewardsManager.ClaimReward()
         └─→ CurrencyManager.AddCoins() / AddGems()

[Misión completada]
    └─→ DailyMissionsManager.ClaimMissionReward()
         └─→ CurrencyManager.GrantMissionReward()

[Logro desbloqueado]
    └─→ AchievementService.UnlockAchievement()
         └─→ CurrencyManager.AddCoins() / AddGems()

[Apuesta seleccionada]
    └─→ BetSelectionPanel.OnPlayClicked()
         └─→ CurrencyManager.EscrowCoins()
              └─→ [Partida] → SettleBet(won) → AddCoins(×1.9) O pérdida

[Compra en Shop]
    └─→ ShopItemData.Purchase()
         └─→ CurrencyManager.SpendCoins() / SpendGems()
```

---

## DATOS: `PlayerData.cs`
**Path**: `Runtime/Data/PlayerData.cs`

Modelo serializado a Firebase. Campos económicos relevantes:
- `isPremium` — estado premium
- `premiumExpiryDate` — expiración premium
- `totalGamesPlayed` / `totalGamesWon` — afectan achievements económicos
- `tournaments` — historial de torneos con `prizeWon`

> **Nota**: `coins` y `gems` NO están en `PlayerData.cs`. Se guardan directamente en `players/{uid}/coins` y `players/{uid}/gems` via `CurrencyManager.SyncCurrencyToFirebase()`.

---

## INVENTARIO COMPLETO DE ARCHIVOS (ECONOMÍA)

### Runtime — Core Economy
| Archivo | Rol |
|---|---|
| `Monetization/Currency/CurrencyManager.cs` | **CENTRAL** — toda la economía pasa por aquí |
| `Monetization/Currency/CurrencyDisplayUI.cs` | Display tiempo real |
| `Games/Navigation/BetSelectionPanel.cs` | Apuestas DC: escrow y selección |
| `Games/Core/GameSessionManager.cs` | Distribución post-game + settle bet + XP |
| `Games/Results/OnlineResultManager.cs` | Ranked rewards (DC por partida) |
| `Monetization/Progression/PlayerProgressionSystem.cs` | XP + Level rewards |
| `Monetization/DailyMissions/DailyMissionsManager.cs` | Misiones diarias → DC + DG |
| `Monetization/DailyRewards/DailyRewardsManager.cs` | Login rewards 14 días |
| `Services/AchievementService.cs` | Logros → DC + DG |
| `Features/Onboarding/OnboardingManager.cs` | Welcome reward +500 DC |
| `Services/DailyOfferService.cs` | Oferta diaria → DC free reward |
| `Services/DailyRewardService.cs` | Persistencia de daily rewards |

### Runtime — IAP / Payments
| Archivo | Rol |
|---|---|
| `Payments/Core/ProductCatalog.cs` | Catálogo IAP (precios, gems amounts) |
| `Payments/Core/PaymentManager.cs` | Orquesta compra → ProcessGemsPurchase |
| `Payments/Core/PaymentBridge.cs` | Delegates bridge (desacoplan IAP de Firebase) |
| `Services/PaymentBridgeWiring.cs` | Conecta PaymentBridge → CurrencyManager |
| `Payments/AppleIAP/AppleReceiptValidator.cs` | Valida receipt → Cloud Function |
| `Payments/AppleIAP/AppleIAPProvider.cs` | Provider Apple IAP |
| `Payments/Stripe/StripeCheckoutController.cs` | Checkout Stripe |
| `Payments/Stripe/StripeSessionPoller.cs` | Polling confirmación pago |
| `Payments/Stripe/StripePaymentProvider.cs` | Provider Stripe |
| `Payments/Entitlements/EntitlementService.cs` | Sync entitlements post-pago |
| `Payments/Entitlements/EntitlementRecord.cs` | Modelo de entitlement |
| `Payments/FeatureFlags/RemoteConfigService.cs` | Decide proveedor activo |
| `Payments/Abort/StripeAbortProtocol.cs` | Emergencia: cambia proveedor |

### Runtime — Shop
| Archivo | Rol |
|---|---|
| `Monetization/Shop/ShopManager.cs` | Manager escena Shop |
| `Monetization/Shop/ShopItemData.cs` | Modelo ítem + lógica de compra (SpendCoins/SpendGems) |
| `Monetization/Shop/ShopItemUI.cs` | UI ítem + affordability check |
| `Monetization/Shop/WelcomePackService.cs` | Welcome Pack D1-D3 (DG grant) |
| `Monetization/Shop/WelcomePackUIController.cs` | UI Welcome Pack |
| `Monetization/Shop/DailyOfferUIController.cs` | UI oferta diaria |
| `Monetization/Shop/WinEffectPreviewPanel.cs` | Preview de Win Effects |
| `Monetization/Premium/PremiumManager.cs` | Gestión premium + gem packs |
| `UI/Panels/PremiumPanelUI.cs` | UI upgrade premium |

### Runtime — Modelos de Datos
| Archivo | Rol |
|---|---|
| `Data/PlayerData.cs` | Modelo Firebase (premium, stats) |
| `Data/Missions/MissionDefinitionSO.cs` | Define misión: tipo, objetivo, reward |
| `Data/Missions/MissionPoolSO.cs` | Pool de misiones disponibles |
| `Data/Missions/MissionProgressReporter.cs` | Puente juego → misiones |

### Runtime — Animaciones Económicas
| Archivo | Rol |
|---|---|
| `Animations/Animators/CurrencyAnimator.cs` | Anima saldo |
| `Animations/Animators/RewardClaimAnimator.cs` | Animación claim |
| `UI/Components/UIPolish.cs` | Efectos visuales moneda |
| `Animations/Animators/MainMenuAnimator.cs` | Currency pill en main menu |

### Editor — Builders Económicos
| Archivo | Rol |
|---|---|
| `Editor/Monetization/ShopPremiumUIBuilder.cs` | Construye escena Shop |
| `Editor/Monetization/ShopEffectsTabBuilder.cs` | Tab efectos en Shop |
| `Editor/Monetization/BackgroundShopItemBuilder.cs` | Ítems de Backgrounds |
| `Editor/Monetization/DailyMissionsUIBuilder.cs` | Construye UI misiones |
| `Editor/Monetization/DailyRewardsPremiumUIBuilder.cs` | Construye UI daily rewards |
| `Editor/Monetization/AchievementsUIBuilder.cs` | Construye UI logros |
| `Editor/Monetization/MonetizationPrefabBuilder.cs` | Builder general |
| `Editor/Monetization/LevelUpPanelBuilder.cs` | Panel level up |
| `Editor/Games/Navigation/BetSelectionPanelUIBuilder.cs` | Construye escena BetSelection |

### Backend
| Archivo | Rol |
|---|---|
| `functions/src/index.ts` | `validateScore`, `iapValidateReceipt`, `stripeCreateCheckout`, `getEntitlements`, `syncEntitlements` |
