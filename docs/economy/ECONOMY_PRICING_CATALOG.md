# Economy Pricing Catalog — DigitPark

> Precios definitivos de todos los items comprables.
> V56 — 2026-03-14 | Precios DC corregidos ×5-8 para target 14-23 meses
> Actualizado 2026-03-19: columnas "EN CÓDIGO" verificadas contra ProductCatalog.cs

Leyenda de estado:
- ✅ **EN CÓDIGO** — productId existe en `ProductCatalog.cs` y/o lógica implementada
- ⏳ **PENDIENTE EDITOR** — lógica en código existe, falta crear el ShopItemData asset en Unity
- ❌ **PENDIENTE CÓDIGO** — falta añadir a ProductCatalog.cs y/o implementar lógica

---

## 1. DigitGems (DG) — IAP Packs

> Los packs en código usan amounts distintos a los nombres del diseño original.
> La tabla muestra la realidad del código (ProductCatalog.cs).

| ProductId | Display | DG entregados | Bonus | Precio USD | Estado |
|---|---|---|---|---|---|
| `sparks_100` | 150 Sparks | 150 DG | — | $0.99 | ✅ EN CÓDIGO |
| _(no existe)_ | ~300 DG pack | — | — | $2.99 | ❌ PENDIENTE CÓDIGO |
| `sparks_500` | 500 Sparks | 550 DG | +10% | $4.99 | ✅ EN CÓDIGO |
| `sparks_1200` | 1,200 Sparks | 1,440 DG | +20% | $9.99 | ✅ EN CÓDIGO |
| `sparks_2500` | 2,500 Sparks | 3,125 DG | +25% | $19.99 | ✅ EN CÓDIGO |
| `sparks_6500` | 6,500 Sparks | 8,450 DG | +30% | $49.99 | ✅ EN CÓDIGO |
| `sparks_14000` | 14,000 Sparks | 18,900 DG | +35% | $99.99 | ✅ EN CÓDIGO |

**Tasa efectiva:** $0.0053–$0.0091/DG según pack
**Decisión pendiente:** Ver ECONOMY_TASKS_CODE.md C-02 (añadir $2.99 pack vs mantener como está)

---

## 2. Temas (34 total)

### Free (4) — desbloqueados al inicio
Default (Neon Cyan), Sunset, Midnight, Ocean

### Earnable (4) — gratis via achievements, shortcut 350 DG
| Tema | Achievement requerido | Shortcut |
|------|-----------------------|----------|
| Emerald | digitrush_master | 350 DG |
| Electric Blue | flashtap_master | 350 DG |
| Monochrome | memorypairs_master | 350 DG |
| Electric Violet | oddoneout_master | 350 DG |

### Tier A — Standard (10 temas, 150 DG ≈ $1.20) — ⏳ PENDIENTE EDITOR
Arctic, DeepOcean, ToxicLime, CoralSurge, Thunder, Titanium, ElectricOrange, Phantom, Infrared, IceFire
> Lógica de compra: ✅ ShopItemData.GrantRewards → ThemeManager.UnlockTheme. Falta: crear 10 assets SO.

### Tier B — Premium (10 temas, 350 DG ≈ $2.80) — ⏳ PENDIENTE EDITOR
Sakura, Matrix, CyberFuchsia, PlasmaIndigo, Nebula, Aurora, Synthwave, Vaporwave, Bioluminescence, Ultraviolet
> Solo DG — no disponible con DC. Temas = territorio exclusivo DG/IAP.
> Falta: crear 10 assets SO con priceType=DigitGems y coinsPrice=0.

### Tier C — Legendary (6 temas, 600 DG only ≈ $4.80) — ⏳ PENDIENTE EDITOR
Volcanic, Outrun, Glitch, Y2KChrome, BloodMoon, Void
> Sin opción DC — fuerza conversión IAP para legendarios.
> Falta: crear 6 assets SO.

---

## 3. Frames (26 total)

### DC Frames (8) — ⏳ PENDIENTE EDITOR
| Frame | DC Price | Tiempo activo | Tiempo casual | Estado |
|-------|----------|---------------|---------------|--------|
| Basic | 2,000 | ~2 días | ~3 días | ⏳ Crear SO |
| Bronze | 5,000 | ~5 días | ~1 sem | ⏳ Crear SO |
| Silver | 12,000 | ~1.5 sem | ~2.5 sem | ⏳ Crear SO |
| Gold | 25,000 | ~3 sem | ~5.5 sem | ⏳ Crear SO |
| Neon | 40,000 | ~5 sem | ~9 sem | ⏳ Crear SO |
| Diamond | 60,000 | ~8 sem | ~13 sem | ⏳ Crear SO |
| Crystal | 80,000 | ~10.5 sem | ~18 sem | ⏳ Crear SO |
| Platinum | 100,000 | ~13 sem | ~22 sem | ⏳ Crear SO |
| **Total** | **324,000** | — | — | — |

### DG Frames (6) — ⏳ PENDIENTE EDITOR
| Frame | DG | USD equiv | Estado |
|-------|-----|-----------|--------|
| Sapphire | 100 | ~$0.80 | ⏳ Crear SO |
| Ruby | 200 | ~$1.60 | ⏳ Crear SO |
| Emerald | 350 | ~$2.80 | ⏳ Crear SO |
| Amethyst | 500 | ~$4.00 | ⏳ Crear SO |
| Topaz | 750 | ~$6.00 | ⏳ Crear SO |
| Obsidian | 1,000 | ~$8.00 | ⏳ Crear SO |

### USD Frames (12) — IAP directo
| Frame | USD | ProductId en código | Estado |
|-------|-----|---------------------|--------|
| Plasma Spark | $0.99 | `frame_plasma_spark` | ✅ EN CÓDIGO |
| Prism Shift | $0.99 | `frame_prism_shift` | ✅ EN CÓDIGO |
| Holographic | $1.99 | _(falta)_ | ❌ PENDIENTE CÓDIGO |
| Quantum Fire | $2.99 | _(falta)_ | ❌ PENDIENTE CÓDIGO |
| Aurora Borealis | $3.99 | `frame_aurora_borealis` | ✅ EN CÓDIGO |
| Legendary Crown | $4.99 | _(falta)_ | ❌ PENDIENTE CÓDIGO |
| Void Walker | $5.99 | `frame_void_walker` | ✅ EN CÓDIGO |
| Storm Surge | $5.99 |
| Cosmic Rift | $9.99 |
| Storm Surge | $5.99 | `frame_storm_surge` | ✅ EN CÓDIGO |
| Cosmic Rift | $9.99 | `frame_cosmic_rift` | ✅ EN CÓDIGO |
| Infernal God | $9.99 | `frame_infernal_god` | ✅ EN CÓDIGO |
| Divine Light | $14.99 | `frame_divine_light` | ✅ EN CÓDIGO |
| Quantum Break | $14.99 | `frame_quantum_break` | ✅ EN CÓDIGO |

> ✅ 9 de 12 frames USD en código. ❌ 3 faltantes: Holographic, Quantum Fire, Legendary Crown — ver ECONOMY_TASKS_CODE.md C-01.

### Level Frames (5) — solo via nivel, no comprables — ✅ EN CÓDIGO
Bronze (L60), Silver (L125), Gold (L250), Platinum (L400), Diamond (L490)
> Implementados en `PlayerProgressionSystem.cs` → `GrantLevelReward()`. No necesitan ShopItemData.

---

## 4. Titles (20 total)

### Level Titles (7) — gratis — ✅ EN CÓDIGO
Novice (L10), Player (L25), Veteran (L50), Centurion (L100), Expert (L175), Grand Master (L450), Legend (L500)
> Implementados en `PlayerProgressionSystem.cs`. No necesitan ShopItemData.

### Purchasable Titles (13) — ✅ EN CÓDIGO (PlayerTitleService.cs, English IDs)
| Title | titleId | Precio | Moneda | Estado |
|-------|---------|--------|--------|--------|
| Strategist | `strategist` | 8,000 | DC | ✅ EN CÓDIGO |
| Analyst | `analyst` | 8,000 | DC | ✅ EN CÓDIGO |
| Champion | `champion` | 20,000 | DC | ✅ EN CÓDIGO |
| Gladiator | `gladiator` | 20,000 | DC | ✅ EN CÓDIGO |
| Mastermind | `mastermind` | 150 | DG | ✅ EN CÓDIGO |
| Prodigy | `prodigy` | 150 | DG | ✅ EN CÓDIGO |
| Titan | `titan` | 300 | DG | ✅ EN CÓDIGO |
| Oracle | `oracle` | 300 | DG | ✅ EN CÓDIGO |
| Phoenix | `phoenix` | 500 | DG | ✅ EN CÓDIGO |
| Quantum | `quantum` | $1.99 | USD | ✅ EN CÓDIGO |
| Immortal | `immortal_title` | $2.99 | USD | ✅ EN CÓDIGO |
| Transcendent | `transcendent` | $4.99 | USD | ✅ EN CÓDIGO |
| Apex Predator | `apex_predator` | $9.99 | USD | ✅ EN CÓDIGO |

> GrantRewards para Title: ✅ `PlayerTitleService.UnlockTitle()` implementado en ShopItemData.cs.

---

## 5. Win Effects (12 total)

| Effect | Precio | Moneda | effectId | Estado |
|--------|--------|--------|----------|--------|
| Confetti Burst | 12,000 | DC | `confetti` | ✅ EN CÓDIGO |
| Fireworks | 20,000 | DC | `fireworks` | ✅ EN CÓDIGO |
| Lightning Strike | 200 | DG | `lightning` | ✅ EN CÓDIGO |
| Neon Explosion | 350 | DG | `neon_explosion` | ✅ EN CÓDIGO |
| Gold Rain | 250 | DG | `gold_rain` | ✅ EN CÓDIGO |
| Pixel Rain | 500 | DG | `pixel_rain` | ✅ EN CÓDIGO |
| Rainbow | 750 | DG | `rainbow` | ✅ EN CÓDIGO |
| Crown Drop | $1.99 | USD | `crown_drop` | ✅ EN CÓDIGO |
| Cosmic Shatter | $1.99 | USD | `cosmic_shatter` | ✅ EN CÓDIGO |
| Fire Ring | $2.99 | USD | `fire_ring` | ✅ EN CÓDIGO |
| Quantum Rift | $3.99 | USD | `quantum_rift` | ✅ EN CÓDIGO |
| Divine Ascension | $6.99 | USD | `divine_ascension` | ✅ EN CÓDIGO |

> GrantRewards para WinEffect: ✅ `VictoryEffectService.UnlockEffect()` implementado en ShopItemData.cs.

---

## 6. Premium Features

| Feature | Precio | Descripción | Estado |
|---------|--------|-------------|--------|
| Ad-Free | $4.99 | Sin ads permanente | ❌ PENDIENTE CÓDIGO (ProductCatalog + lógica skip ads) |
| Premium Pass | $9.99/mes | 2x XP, misiones exclusivas, badge | ❌ PENDIENTE CÓDIGO (Subscription + lógica 2x XP) |
| Starter Pack | $2.99 | 500 DG + 5,000 DC + Frame exclusivo (one-time) | ❌ PENDIENTE CÓDIGO + EDITOR |

> Ver ECONOMY_TASKS_CODE.md C-04, C-05, C-06.

---

## 7. Welcome Packs (one-time, primeras 72h)

| Pack | Precio | Contenido | Estado |
|------|--------|-----------|--------|
| Welcome Pack | $1.99 | 200 DG + 2,000 DC + Bronze Frame | ❌ PENDIENTE CÓDIGO (añadir a ProductCatalog) |
| VIP Welcome | $4.99 | 600 DG + 5,000 DC + Holographic Frame + Title "Prodigy" | ❌ PENDIENTE CÓDIGO (añadir a ProductCatalog) |

> WelcomePackService.cs existe y maneja el timer D1-D3. Falta: añadir productIds al catálogo y verificar grant completo. Ver ECONOMY_TASKS_CODE.md C-01 y C-07.

---

## 8. Economy Flow — Resumen

### Fuentes de ingreso (jugador activo, semanal)
| Fuente | DC/sem | DG/sem |
|--------|--------|--------|
| Daily Rewards | 1,825 | 3 |
| Daily Missions Bonus | 700 | ~5–7 |
| Post-Game (~20 partidas/día) | ~4,200 | 0 |
| Achievements (amortizado) | ~200 | ~1 |
| Level Rewards (amortizado) | ~150 | ~0.3 |
| **Total** | **~7,075** | **~10–11** |

### Sinks de DC (precios corregidos ×5-8)
| Sink | Rango DC | Total | Notas |
|------|----------|-------|-------|
| Betting (5% rake) | 5% de lo apostado | continuo | Drenaje recurrente |
| DC Frames | 2,000–100,000 | 324,000 | One-time |
| DC Titles | 8,000–20,000 | 56,000 | One-time |
| DC Win Effects | 12,000–20,000 | 32,000 | One-time |
| **Total catálogo DC** | — | **412,000** | 14 meses (activo) / 23 meses (casual) |

### Tiempo para ahorrar DG gratis (~10 DG/sem jugador activo)
| Item | DG | Semanas | Meses |
|------|----|---------|-------|
| DG Frame más barato (Sapphire) | 100 | ~10 | ~2.5 |
| Tier A Theme | 150 | ~15 | ~3.8 |
| Earnable Theme shortcut | 350 | ~35 | ~8.8 |
| Tier B Theme | 350 | ~35 | ~8.8 |
| Tier C Theme | 600 | ~60 | ~15 |
