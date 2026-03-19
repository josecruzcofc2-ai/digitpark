# PENDIENTES EDITOR / CÓDIGO
**Última actualización**: 2026-03-19
**Definición**: Tareas que estaban en PENDIENTES_MANUALES.md pero que SÍ se pueden implementar con Editor scripts (menú DigitPark/...) o cambios de código .cs. El usuario solo necesita ejecutar el menú o confirmar el código.

---

## E-01. Editor Script — Agregar tag "FrameLayer"
**Origen**: M-21
- Crear `[MenuItem("DigitPark/Setup/Add FrameLayer Tag")]`
- Usar `UnityEditorInternal.InternalEditorUtility` o `SerializedObject` de `TagManager.asset`
- Una sola ejecución, sin parámetros
- **Por qué no es manual**: no requiere criterio humano, es determinista

---

## E-02. Editor Script — Actualizar precios DC en ShopItemData ScriptableObjects
**Origen**: M-22
- `AssetDatabase.FindAssets("t:ShopItemData", new[]{"Assets/_Project/Resources/Shop/"})`
- Para cada asset encontrado, aplicar la tabla de precios:

| Frame DC | Precio actual | Precio nuevo |
|----------|---------------|--------------|
| Basic | 500 | 2000 |
| Bronze | 1000 | 5000 |
| Silver | 2500 | 12000 |
| Gold | 5000 | 25000 |
| Neon | 7500 | 40000 |
| Diamond | 10000 | 60000 |
| Crystal | 12000 | 80000 |
| Platinum | 15000 | 100000 |

| Title DC | Precio actual | Precio nuevo |
|----------|---------------|--------------|
| Strategist / Analyst | 2000 | 8000 |
| Champion / Gladiator | 5000 | 20000 |

| Efecto DC | Precio actual | Precio nuevo |
|-----------|---------------|--------------|
| Confetti Burst | 3000 | 12000 |
| Fireworks | 5000 | 20000 |

- `EditorUtility.SetDirty(asset)` + `AssetDatabase.SaveAssets()` al final
- **Prerequisito**: los ShopItemData deben tener campo `coinsPrice` (verificar nombre exacto en `ShopItemData.cs`)

---

## E-03. Editor Script — Crear todos los ShopItemData ScriptableObjects del catálogo
**Origen**: M-23
- `ScriptableObject.CreateInstance<ShopItemData>()` + `AssetDatabase.CreateAsset()`
- Directorio: `Assets/_Project/Resources/Shop/`
- Crear un SO por cada item del catálogo (frames DC, frames DG, frames USD, titles, effects, themes)
- Rellenar `itemType`, `priceType`, precio en el script
- `[MenuItem("DigitPark/Shop/Create All Shop Items")]`
- **Prerequisito**: confirmar los nombres de item exactos del catálogo final antes de implementar

---

## E-04. Editor Script — Renombrar GO "MemoryParisController" en MemoryPairs.unity
**Origen**: M-38
- `EditorSceneManager.OpenScene("Assets/_Project/Scenes/Games/MemoryPairs.unity", OpenSceneMode.Additive)`
- `GameObject typo = GameObject.Find("MemoryParisController")`
- `typo.name = "MemoryPairsController"`
- `EditorSceneManager.SaveScene(scene)`
- `EditorSceneManager.CloseScene(scene, true)`
- `[MenuItem("DigitPark/Fix/Rename MemoryPairsController Typo")]`
- **Nota**: no edita YAML directamente — usa Unity API

---

## E-05. Editor Script — Crear 3 ShopItemData "TemporaryDecoration"
**Origen**: M-42
- Igual que E-03 pero solo 3 items específicos:
  1. `DecorationNeonBorder` → itemType=TemporaryDecoration, priceType=DigitCoins, coinsPrice=1000, durationDays=30
  2. `DecorationGoldAura` → itemType=TemporaryDecoration, priceType=DigitCoins, coinsPrice=2500, durationDays=30
  3. `DecorationChampionBadge` → itemType=TemporaryDecoration, priceType=DigitCoins, coinsPrice=500, durationDays=7
- **PREREQUISITO BLOQUEANTE**: `ShopItemType.TemporaryDecoration` debe estar en el enum de `ShopItemData.cs` primero
- `[MenuItem("DigitPark/Shop/Create Temporary Decoration Items")]`

---

## E-06. Código Runtime — Remote Config check para ocultar botón CashBattle
**Origen**: M-44 Opción A (recomendada)
- En `MainMenuManager.cs`: al inicializar, leer `RemoteConfigService.Instance.GetValueAsync("cash_battle_enabled")`
- Si `false` → `cashBattleButton.SetActive(false)`
- **Prerequisito**: Firebase Remote Config instalado (M-12) y parámetro creado en consola (M-13 + `cash_battle_enabled=false`)
- Alternativa mientras no esté Remote Config: hacer M-44 Opción B (Inspector manual)

---

## NOTA — M-25 (mislabeled)
M-25 en PENDIENTES_MANUALES.md dice explícitamente *"es código. Ver PENDIENTES_CODIGO.md"*.
Tarea: registrar 9 frames IAP en `ProductCatalog.cs`. Mover a PENDIENTES_CODIGO.md cuando se actualice.
