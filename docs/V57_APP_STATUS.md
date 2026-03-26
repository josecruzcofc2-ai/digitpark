# DigitPark V57 — Estado actual post-simplificación
**Fecha**: 2026-03-25
**Rama**: master
**Objetivo**: App simple, normal, lista para subir rápido

---

## 1. ESTADO ACTUAL — LO QUE QUEDÓ

### Escenas activas (19)
| Categoría | Escena |
|---|---|
| Core | Boot, MainMenu, Settings |
| Auth | Login, Register |
| Juegos | DigitRush, FlashTap, MemoryPairs, OddOneOut, QuickMath |
| Navegación juegos | GameSelector, PlayModeSelection, BetSelection, Matchmaking |
| Monetización | Shop, DailyRewards |
| Social | MatchHistory, Scores (leaderboard) |
| Onboarding | Onboarding |

### Sistemas activos
- Firebase Auth (Email, Google, Apple)
- Firebase Realtime DB + Storage + Analytics + FCM + Cloud Functions
- Apple IAP + Google Play IAP
- Shop (scroll continuo, 4 pestañas)
- Cosmetics: Frames, Titles, WinEffects, BattleCards
- Welcome Packs (D1–D5)
- Daily Rewards
- Achievements
- Tournaments (1v1 ranked + torneos)
- Leaderboards
- Amigos / Social
- Onboarding
- Settings

---

## 2. DOCS/FIREBASE — QUÉ HACER

### `FIREBASE_DEEP_REVIEW.md`
- **Estado**: Histórico. Todos los 126 fixes aplicados en V49 (2026-03-19).
- **Recomendación**: BORRAR. Ya no tiene valor accionable — es registro de deuda técnica ya pagada.

### `FIREBASE_USAGE_AUDIT.md`
- **Estado**: Actualizado 2026-03-24. Documenta todos los paths, servicios y endpoints activos.
- **Recomendación**: CONSERVAR como referencia mientras se hacen pruebas de Firebase.

---

## 3. TAREAS_MANUALES.md — TAREAS OBSOLETAS

Las siguientes tareas ya no aplican por sistemas eliminados:

| # | Tarea | Por qué está obsoleta |
|---|---|---|
| 2 | App Store ID en ReviewService | `ReviewService.cs` fue eliminado en V57 |
| 13 | AdMob / Unity Ads SDK | No hay sistema de anuncios en la app actual; pendiente de decisión |

**El resto (1, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 14, 15) siguen vigentes.**

**Acción**: Borrar tarea #2. Marcar #13 como "pendiente de decisión futura".

---

## 4. DEAD CODE DETECTADO POST-V57

### BattleCards (3 archivos — órfanos de CashBattle)
```
Assets/_Project/Scripts/Runtime/Features/Cosmetics/BattleCards/BattleCardData.cs
Assets/_Project/Scripts/Runtime/Features/Cosmetics/BattleCards/BattleCardApplier.cs
Assets/_Project/Scripts/Runtime/Features/Cosmetics/BattleCards/BattleCardService.cs
```
- Eran cosméticos del matchmaking de CashBattle (cartas de batalla).
- CashBattle fue eliminado. Estas clases ya no tienen escena ni UI que las use.
- `ShopItemType.BattleCard` aún existe en el enum.
- **Recomendación**: Eliminar los 3 scripts + `ShopItemType.BattleCard` del enum.

### WinEffects — productos sin UI
- `ProductCatalog.cs` tiene 3 productos: Cosmic Shatter ($1.99), Quantum Rift ($3.99), Divine Ascension ($6.99)
- La UI que los mostraba fue eliminada en la simplificación.
- **Recomendación**: Mantener en `ProductCatalog` si se va a reimplementar la UI. Si no, eliminar.

### ShopItemType.TemporaryDecoration
- Existe en el enum pero no hay ningún producto activo de este tipo.
- **Recomendación**: Eliminar del enum por ahora.

---

## 5. SHOP — CATÁLOGO ACTUAL (47 productos)

### Gem Packs (consumibles) — TODOS VIGENTES
| ID | Precio | Gems |
|---|---|---|
| gems_100 | $0.99 | 100 |
| gems_300 | $2.99 | 300 |
| gems_500 | $4.99 | 650 |
| gems_1200 | $9.99 | 1,200 |
| gems_2500 | $19.99 | 2,500 |
| gems_6500 | $49.99 | 6,500 |
| gems_14000 | $99.99 | 18,900 |

### Bundles (no-consumibles)
| Producto | Precio | Observación |
|---|---|---|
| premium_bundle | $26.25 | Todos los frames + titles (30% off) |
| complete_bundle | $30.45 | Frames + titles + win effects (30% off) |
| welcome_pack_basic | $1.99 | WelcomePack D1–D3 |
| welcome_pack_vip | $4.99 | WelcomePack D1–D5 |
| starter_pack | $2.99 | Starter D1–D3 |
| ad_free | $4.99 | Sin anuncios (no hay anuncios aún) |
| premium_pass | $9.99/mes | Suscripción mensual |

### Titles (4), WinEffects (3), Frames (10), Ads-free, PremiumPass
Ver `ProductCatalog.cs` para IDs completos.

---

## 6. PARA QUÉ FALTA — ANTES DE SUBIR

### Firebase (backend)
- [ ] Rotar API keys (están expuestas en historial git)
- [ ] `firebase functions:secrets:set APPLE_SHARED_SECRET`
- [ ] `firebase deploy --only functions`
- [ ] `firebase deploy --only database,storage,firestore`

### App Stores
- [ ] Crear productos IAP en App Store Connect + Google Play (7 gem packs + non-consumables)
- [ ] Receipt Validation Obfuscator en Unity (Window > Unity IAP)
- [ ] Sign In with Apple entitlement en Xcode

### Unity
- [ ] Development Build desmarcado
- [ ] `#define FIREBASE_MESSAGING` si se quiere FCM activo
- [ ] Canvas Scaler verificado en todas las escenas (1080×1920, Match=0.5)

### Pruebas mínimas antes de subir
- [ ] Flujo completo: Login → GameSelector → Matchmaking → Partida → Resultado
- [ ] Compra de gems (sandbox iOS + Android)
- [ ] Welcome Pack aparece D1 y desaparece al comprar
- [ ] Daily Rewards funciona y persiste en Firebase
- [ ] Leaderboard carga y muestra datos reales

---

## 7. ANÁLISIS: ¿ESTÁ SIMPLE LA APP?

### Lo que se siente "normal" para 2024
✅ 5 minijuegos simples
✅ 1v1 ranked
✅ Leaderboard
✅ Shop con gems
✅ Login social
✅ Settings básico

### Lo que aún se siente "sobrecargado" para una v1 simple
⚠️ **Tournaments** — Infraestructura compleja (lobby, brackets, servidor), difícil de testear, propenso a bugs de sincronización
⚠️ **Achievements** — Escena completa + wiring pendiente (TrophyCard.prefab sin asignar)
⚠️ **DailyRewards** — Añade retención pero es una escena extra + lógica de servidor
⚠️ **Friends/Social** — Firebase paths complejos, funcionalidad poco visible
⚠️ **BattleCards** — 3 scripts muertos, `ShopItemType.BattleCard` en enum
⚠️ **Premium Pass ($9.99/mes)** — Suscripción sin beneficio claro definido actualmente
⚠️ **47 productos IAP** — Demasiados para configurar y testear en stores; muchos sin UI activa
⚠️ **WinEffects sin UI** — Productos en catálogo pero nadie puede verlos ni comprarlos
⚠️ **Ad-Free IAP** — No hay anuncios, el producto no tiene sentido ahora

### Cosmético razonable para v1
✅ Frames (10) — Cosmético simple y claro
✅ Titles (4) — Simple, añade status
⚠️ WinEffects (3) — Requiere reimplementar UI antes de activar
