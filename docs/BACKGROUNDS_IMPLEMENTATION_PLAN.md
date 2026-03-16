# DIGITPARK — PLAN DE IMPLEMENTACIÓN: COSMÉTICO DE BACKGROUNDS
**Estado**: Listo para implementar
**Última actualización**: 2026-03-15
**Prioridad**: P1 (post-audit, antes del primer release)

---

## ⚖️ ANÁLISIS LEGAL Y DE DISEÑO — 3 PUNTOS CRÍTICOS

---

### PUNTO 1 — ¿Pueden los backgrounds aplicarse a escenas CashBattle?

#### Investigación realizada
Se auditaron exhaustivamente:
- `CashThemeForcer.cs` — componente que gestiona la paleta gold en CashBattle
- `TriumphServices.cs` — SDK de real money (stubs, sin restricciones visuales documentadas)
- `CashBattle1v1Manager.cs` + todos los managers de CashBattle — audit de colores hardcodeados
- `BattleCardApplier.cs` — cosmético de cartas de jugador

#### Hallazgos clave

| Pregunta | Respuesta |
|----------|-----------|
| ¿CashThemeForcer modifica el Background Image? | **NO** — solo reemplaza acentos (cyan→gold) en UI elements (Text, Outline, Button) |
| ¿El Background Image de CashBattle está hardcodeado en gold? | **NO** — usa `ThemeData.primaryBackground` (dark color del tema) |
| ¿Triumph tiene restricciones documentadas sobre cosméticos visuales? | **NINGUNA** en el código. Solo restricciones financieras (entry fees, withdrawal caps) |
| ¿BattleCardApplier toca backgrounds de escena? | **NO** — solo gestiona los cosmetics de player cards (avatarFrame, border glow, etc.) |
| ¿Los avatar frames y cosméticos ya funcionan en CashBattle? | **SÍ** — BattleCardApplier activo en Matchmaking, lo que confirma que Triumph permite cosméticos |

#### Veredicto legal: ✅ VERDE — Sin inconveniente

Los backgrounds son **técnica y legalmente seguros** en CashBattle porque:

1. **No interfieren con información financiera**: el patrón al 5–12% de opacidad no oculta cantidades de dinero real, saldos de wallet, ni elementos KYC
2. **No modifican la mecánica del juego**: puramente cosmético, sin impacto en fairness ni competitive integrity
3. **Triumph ya permite cosméticos** (avatar frames, BattleCardApplier) — un overlay de fondo es menos visible que un frame de avatar
4. **La restricción gold es de tema, no de cosmético**: la regla "no ThemeApplier en CashBattle" afecta a los colores de acento del tema (botones, textos), no a una capa cosmética independiente
5. **El jugador eligió el cosmético** — no es una imposición del sistema, así que no puede alegar "distracción no consentida"

**Recomendación**: Incluir CashBattle (escenas 32–40 + 08) en el sistema de BackgroundPattern.

---

### PUNTO 3 — Temas "Chromatic" con 2 colores: RECOMENDACIÓN: ✅ SÍ, con matices

#### El concepto

El usuario propone: para ciertos temas premium, el patrón cosmético no use blanco como tint sino el **color de acento del tema** (`primaryAccent`), creando una experiencia de 2 colores:

```
[ESTÁNDAR]   Background = #0A0A14 (dark)  +  Pattern = white @ 6%
             → Resultado: sutil shimmer blanco sobre fondo oscuro

[CHROMATIC]  Background = #0A0A14 (dark)  +  Pattern = #00FFFF @ 6%
             → Resultado: trazas de PCB CYAN brillantes sobre fondo oscuro
```

#### Análisis de impacto visual

| Combinación | Resultado | Wow factor |
|-------------|-----------|-----------|
| NeonDark + bg_circuit (blanco) | Trazas blancas sutiles | ★★★ |
| NeonDark + bg_circuit **CHROMATIC** (cyan) | Trazas neón cyan | ★★★★★ |
| Synthwave + bg_neural (blanco) | Sinapsis blancas | ★★★ |
| Synthwave + bg_neural **CHROMATIC** (magenta) | Sinapsis magenta sobre fondo púrpura | ★★★★★ |
| Infrared + bg_fingerprint (blanco) | Huella blanca | ★★★ |
| Infrared + bg_fingerprint **CHROMATIC** (rojo neón) | Huella roja biométrica | ★★★★★ |

El impacto visual con 2 colores es dramáticamente superior. Es la diferencia entre "patrón decorativo" y "identidad visual única".

#### Modelo de implementación recomendado

**NO crear 30 temas nuevos** — eso es inmanejable. En su lugar:

```
ThemeData nueva propiedad:
  public bool isChromatic = false;          // toggle en Inspector
  public Color patternTintColor = Color.white; // si white → modo estándar
                                               // si otro → modo chromatic
```

El `BackgroundPatternReceiver` ya suscribe a ThemeManager. Si el tema activo tiene `isChromatic = true`, usa `theme.patternTintColor` en lugar de `Color.white`.

**Dos categorías de temas (nueva clasificación):**

| Categoría | patternTintColor | Descripción | Impacto precio |
|-----------|-----------------|-------------|----------------|
| **Standard** | `Color.white` | El patrón es siempre blanco. Overlay sutil. | Sin cambio |
| **Chromatic** | `theme.primaryAccent` (o custom) | El patrón usa el color de acento del tema. Explosión de color. | **+100–150 DG** |

#### ¿Qué temas deberían ser Chromatic?

Los temas con acentos más impactantes visualmente como Chromatic dan el mayor beneficio:

| Tema | Acento | Como Chromatic |
|------|--------|----------------|
| Neon Dark | Cyan `#00FFFF` | ★★★★★ — el más iconic |
| Synthwave | Magenta/Pink | ★★★★★ — definición del synthwave |
| Infrared | Rojo neón | ★★★★★ — heat signature |
| Toxic Lime | Verde lima | ★★★★ — radioactivo |
| Plasma Indigo | Violeta | ★★★★ |
| Electric Blue | Azul eléctrico | ★★★★ |
| Blood Moon | Rojo oscuro | ★★★ |

**Recomendación**: Los **8–10 temas más vendidos** se convierten en Chromatic (precio mayor). Los temas de acentos suaves (Arctic, Monochrome, Titanium) quedan en Standard (overlay blanco sigue siendo adecuado para sus paletas frías/neutras).

#### Impacto en precios de TEMAS (user's clarification)

> El usuario aclara que la subida de precio es para los **temas** que ofrecen 2 colores, no los backgrounds.

| Tier tema | Standard (1 color) | Chromatic (2 colores) | Diferencia |
|-----------|-------------------|----------------------|-----------|
| Earnable (desbloqueables) | precio DG base | +100 DG | +~25–30% |
| Premium paid (StylesPro / compra individual) | precio DG base | +120 DG | +~25–30% |

**Ejemplo concreto:**
- Synthwave Standard: 350 DG → Synthwave Chromatic: **470 DG**
- Infrared Standard: 300 DG → Infrared Chromatic: **420 DG**

> Importante: el usuario puede comprar primero el Standard y luego hacer "upgrade" a Chromatic pagando la diferencia, o comprar directamente el Chromatic. Esto crea un **upsell in-app sin recompra**.

#### ¿Complejidad de implementación?

Baja. Solo requiere:
1. Añadir `isChromatic` + `patternTintColor` a `ThemeData.cs`
2. `BackgroundPatternReceiver` lee `ThemeManager.Instance.CurrentTheme.patternTintColor` en `ApplyPatternToScene()`
3. Actualizar los ScriptableObjects de los temas Chromatic (cambiar los campos en Inspector)
4. Actualizar Shop UI para mostrar badge `CHROMATIC ✨` en los temas que lo soporten

**No requiere nuevos assets**. El mismo PNG de background se ve radicalmente diferente según el tint.

---

## 🏆 RANKING DE BACKGROUNDS — MEJOR A PEOR

> Criterios evaluados: impacto visual, fit con la marca DigitPark, versatilidad entre temas, sensación premium, singularidad.

| # | ID | Nombre | Descripción visual | ⭐ | Moneda | Precio |
|---|----|--------|-------------------|-----|--------|--------|
| 1 | `bg_neural` | Neural Network | Nodos conectados por sinapsis. Representa el cerebro activo — literalmente la identidad de DigitPark. Complejo, orgánico, premium. | ⭐⭐⭐⭐⭐ | **DG** | **190 DG** |
| 2 | `bg_circuit` | Circuit Board | Trazas de PCB con nodos. El patrón más "on-brand" de todos. Icónico, tecnológico, muy reconocible. | ⭐⭐⭐⭐⭐ | **DG** | **170 DG** |
| 3 | `bg_dna` | DNA Helix | Doble hélice en columnas verticales. Único, científico, memorable. Funciona especialmente bien con temas fríos. | ⭐⭐⭐⭐½ | **DG** | **160 DG** |
| 4 | `bg_constellation` | Constellation | Mapa estelar con líneas. Elegante, espacioso, aspiracional. El que más gusta por primera impresión. | ⭐⭐⭐⭐½ | **DG** | **150 DG** |
| 5 | `bg_fingerprint` | Fingerprint | Curvas biométricas concéntricas. Orgánico, denso, muy singular. Alta sensación de exclusividad. | ⭐⭐⭐⭐ | **DG** | **140 DG** |
| 6 | `bg_triangles` | Triangular Mesh | Malla low-poly geométrica. Moderno, limpio, sofisticado. Funciona con cualquier tema. | ⭐⭐⭐⭐ | **DG** | **120 DG** |
| 7 | `bg_waveform` | Waveform | Ondas de audio entrelazadas. Dinámico, rítmico, muy "alive". Elegante con temas fríos o neutros. | ⭐⭐⭐⭐ | **DG** | **110 DG** |
| 8 | `bg_digits` | Digit Rain | Lluvia de dígitos 0-9 scattered. **El más on-brand para DigitPark** — dígitos son la identidad del juego. | ⭐⭐⭐½ | **DG** | **100 DG** |
| 9 | `bg_binary` | Binary Rain | Columnas de 0s y 1s. Estética Matrix. Cool y reconocible, aunque más "genérico" digitalmente. | ⭐⭐⭐½ | **DG** | **90 DG** |
| 10 | `bg_hexgrid` | Hex Grid | Hexágonos outline dispersos. Limpio, geométrico, moderno. Buen "middle ground". | ⭐⭐⭐ | **DG** | **80 DG** |
| 11 | `bg_crosshatch` | Crosshatch | Líneas diagonales cruzadas. Elegante y clásico, menos "digital native". Más artístico que técnico. | ⭐⭐⭐ | **DG** | **70 DG** |
| 12 | `bg_grid` | Graph Grid | Cuadrícula milimetrada. Limpio y técnico. Poco sorprendente visualmente pero muy versátil. | ⭐⭐½ | **DC** | **12,000 DC** |
| 13 | `bg_dots` | Dot Matrix | Puntos equidistantes. El más minimal de todos. El "básico" del catálogo. | ⭐⭐ | **DC** | **8,000 DC** |
| 14 | `bg_solid` | Solid (default) | Sin patrón. Color sólido puro. | — | **GRATIS** | — |

### ¿Por qué bg_grid y bg_dots con DigitCoins?

- Son los **menos visualmente impactantes** → correcto no cobrar moneda premium por ellos
- Dar 2 patrones "ganables" crea un **loop de retención** para jugadores free-to-play
- El **precio alto en DC** (8k–12k) los hace aspiracionales: ~3–4 semanas de juego regular
- Los jugadores premium que compran DG ignoran el DC; los free players tienen un objetivo
- Psicología: el free player que gana bg_dots está más enganchado y más dispuesto a convertir

---

## 📦 Assets integrados

Todos los PNGs están en `Assets/_Project/Resources/Backgrounds/` con sus `.meta`.

| ID | Archivo | Estado | Opacidad |
|----|---------|--------|----------|
| `bg_solid` | *(sin sprite)* | ✅ Default gratis | — |
| `bg_neural` | `bg_neural.png` | ✅ Listo | **6%** |
| `bg_circuit` | `bg_circuit.png` | ✅ Listo | **5%** |
| `bg_dna` | `bg_dna.png` | ✅ Listo | **6%** |
| `bg_constellation` | `bg_constellation.png` | ✅ Listo | **7%** |
| `bg_fingerprint` | `bg_fingerprint.png` | ✅ Listo | **5%** |
| `bg_triangles` | `bg_triangles.png` | ✅ Listo | **7%** |
| `bg_waveform` | `bg_waveform.png` | ✅ Listo | **8%** |
| `bg_digits` | `bg_digits.png` | ✅ Listo (regenerado alto contraste) | **12%** |
| `bg_binary` | `bg_binary.png` | ✅ Listo | **6%** |
| `bg_hexgrid` | `bg_hexgrid.png` | ✅ Listo | **6%** |
| `bg_crosshatch` | `bg_crosshatch.png` | ✅ Listo | **6%** |
| `bg_grid` | `bg_grid.png` | ✅ Listo | **7%** |
| `bg_dots` | `bg_dots.png` | ✅ Listo | **8%** |

> **Nota técnica**: Los PNGs están importados como Sprite, wrap mode = **Repeat** (wrapU/V = 0).
> Se cargan via `Resources.Load<Sprite>("Backgrounds/bg_circuit")`.

---

## 🏗 FASE 1 — BackgroundPatternManager (Runtime Singleton)

**Archivo**: `Assets/_Project/Scripts/Runtime/Features/Cosmetics/BackgroundPatternManager.cs`

### Responsabilidades
- Cargar el patrón activo desde PlayerPrefs al inicio
- Aplicar el sprite y la opacidad a todos los `BackgroundPattern` activos en escena
- Suscribirse a `SceneManager.sceneLoaded` para aplicar automáticamente en cada cambio de escena
- Guardar la selección del usuario en PlayerPrefs
- Exponer `SetPattern(string patternId)` para el Shop

### Código base

```csharp
namespace DigitPark.Cosmetics
{
    public class BackgroundPatternManager : MonoBehaviour
    {
        private static BackgroundPatternManager _instance;
        public static BackgroundPatternManager Instance => _instance;

        private const string PREF_KEY = "active_background";
        private const string DEFAULT_ID = "bg_solid";

        // Mapa id → opacidad fija del patrón
        private static readonly Dictionary<string, float> PatternOpacity = new Dictionary<string, float>
        {
            { "bg_solid",        0.00f },
            { "bg_dots",         0.08f },
            { "bg_grid",         0.07f },
            { "bg_crosshatch",   0.06f },
            { "bg_circuit",      0.05f },
            { "bg_binary",       0.06f },
            { "bg_constellation",0.07f },
            { "bg_triangles",    0.07f },
            { "bg_neural",       0.06f },
            { "bg_fingerprint",  0.05f },
            { "bg_dna",          0.06f },
            { "bg_waveform",     0.08f },
            { "bg_hexgrid",      0.06f },
            { "bg_digits",       0.12f },
        };

        private string _activePatternId;
        private Sprite _activeSprite; // null = bg_solid

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                LoadSavedPattern();
            }
            else Destroy(gameObject);
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            ApplyPatternToScene();
        }

        private void LoadSavedPattern()
        {
            _activePatternId = PlayerPrefs.GetString(PREF_KEY, DEFAULT_ID);
            _activeSprite = _activePatternId == "bg_solid"
                ? null
                : Resources.Load<Sprite>($"Backgrounds/{_activePatternId}");
            ApplyPatternToScene();
        }

        public void SetPattern(string patternId)
        {
            _activePatternId = patternId;
            _activeSprite = patternId == "bg_solid"
                ? null
                : Resources.Load<Sprite>($"Backgrounds/{patternId}");

            PlayerPrefs.SetString(PREF_KEY, patternId);
            PlayerPrefs.Save();
            ApplyPatternToScene();
        }

        public string ActivePatternId => _activePatternId;

        public void ApplyPatternToScene()
        {
            float opacity = PatternOpacity.TryGetValue(_activePatternId, out float op) ? op : 0f;
            var targets = FindObjectsByType<BackgroundPatternReceiver>(FindObjectsSortMode.None);
            foreach (var t in targets)
                t.Apply(_activeSprite, opacity);
        }
    }
}
```

---

## 🏗 FASE 2 — BackgroundPatternReceiver (Componente por escena)

**Archivo**: `Assets/_Project/Scripts/Runtime/Features/Cosmetics/BackgroundPatternReceiver.cs`

### Responsabilidades
- Componente que se añade al GO `Canvas/BackgroundPattern` en cada escena temática
- Recibe el sprite y la opacidad desde el Manager
- NO tiene ThemeApplier — color siempre `white @ opacidad`

```csharp
namespace DigitPark.Cosmetics
{
    [RequireComponent(typeof(RawImage))]
    public class BackgroundPatternReceiver : MonoBehaviour
    {
        private RawImage _rawImage;

        private void Awake()
        {
            _rawImage = GetComponent<RawImage>();
            _rawImage.raycastTarget = false;
        }

        private void Start()
        {
            // Registrar con el Manager si ya está activo
            if (BackgroundPatternManager.Instance != null)
            {
                BackgroundPatternManager.Instance.ApplyPatternToScene();
            }
        }

        public void Apply(Sprite sprite, float opacity)
        {
            if (sprite == null)
            {
                _rawImage.enabled = false;
                return;
            }
            _rawImage.enabled = true;
            _rawImage.texture = sprite.texture;
            _rawImage.color = new Color(1f, 1f, 1f, opacity);

            // Tiling: calcular cuántas veces repite el patrón en pantalla
            // 512px de patrón, pantalla típica ~1080px → ~2.1 tiles
            float tilesX = Screen.width / 512f;
            float tilesY = Screen.height / 512f;
            _rawImage.uvRect = new Rect(0, 0, tilesX, tilesY);
        }
    }
}
```

> **Por qué RawImage y no Image**: `Image` en Unity UI no soporta tiling nativo.
> `RawImage` con `uvRect` permite repetir la textura sin escalarla, manteniendo
> el tamaño visual correcto del patrón independientemente del tamaño de pantalla.

---

## 🏗 FASE 3 — BackgroundPatternSetup (Editor Tool)

**Archivo**: `Assets/_Project/Scripts/Editor/Tools/BackgroundPatternSetup.cs`

### Responsabilidades
- Añadir `Canvas/BackgroundPattern` (RawImage + BackgroundPatternReceiver) a las 28 escenas temáticas
- Colocarlo siempre en índice `sibling = 1` (justo después de Background, antes de SafeArea)
- Ejecutable desde `DigitPark/Backgrounds/Add Pattern Layer to All Scenes`
- Ejecutable en escena activa desde `DigitPark/Backgrounds/Add Pattern Layer to Current Scene`

### Escenas a modificar (37 total)

**28 escenas temáticas** (patrón = white @ opacity):
```
_Core/Boot.unity               _Core/MainMenu.unity
_Core/Settings.unity           Auth/Login.unity
Auth/Register.unity            Games/Navigation/GameSelector.unity
Games/Navigation/PlayModeSelection.unity
Games/Navigation/BetSelection.unity
Games/Navigation/Matchmaking.unity
Tournaments/TournamentsBrowser.unity
Tournaments/TournamentCreate.unity
Tournaments/TournamentLobby.unity
Games/Minigames/DigitRush.unity     Games/Minigames/FlashTap.unity
Games/Minigames/MemoryPairs.unity   Games/Minigames/OddOneOut.unity
Games/Minigames/QuickMath.unity
Social/Profile/Profile.unity        Social/Profile/Scores.unity
Social/Profile/MatchHistory.unity   Social/Friends/Friends.unity
Social/Friends/FriendRequests.unity Social/Friends/SearchPlayers.unity
Social/Notifications/Notifications.unity
Monetization/Shop.unity             Monetization/DailyMissions.unity
Monetization/DailyRewards.unity     Monetization/Achievements.unity
```

**9 escenas CashBattle** (patrón = white @ opacity, igual que las temáticas):
```
CashBattle/CashBattleHub.unity
CashBattle/CashBattle1v1.unity
CashBattle/CashHistory.unity
CashBattle/CashMatchmaking.unity
CashBattle/CashProfile.unity
CashBattle/CashTournaments/CashTournamentCreate.unity
CashBattle/CashTournaments/CashTournamentLobby.unity
CashBattle/CashTournaments/CashTournaments.unity
CashBattle/CashWallet.unity
```

> El editor tool detecta automáticamente si la escena es CashBattle por nombre y aplica el tint correcto en `BackgroundPatternReceiver`.

### Código base del tool
```csharp
[MenuItem("DigitPark/Backgrounds/Add Pattern Layer to Current Scene")]
public static void AddPatternLayerToCurrentScene()
{
    Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
    if (canvas == null) { Debug.LogWarning("[BGPattern] No Canvas found"); return; }

    // Evitar duplicados
    if (canvas.transform.Find("BackgroundPattern") != null)
    {
        Debug.Log("[BGPattern] Already exists in this scene");
        return;
    }

    var go = new GameObject("BackgroundPattern");
    go.transform.SetParent(canvas.transform, false);

    // Posición: índice 1 (después de Background = índice 0)
    Transform bg = canvas.transform.Find("Background");
    int siblingIndex = bg != null ? bg.GetSiblingIndex() + 1 : 1;
    go.transform.SetSiblingIndex(siblingIndex);

    // RawImage — stretch completo
    var rawImg = go.AddComponent<RawImage>();
    rawImg.color = new Color(1f, 1f, 1f, 0f); // transparent por defecto (bg_solid)
    rawImg.raycastTarget = false;

    var rt = go.GetComponent<RectTransform>();
    rt.anchorMin = Vector2.zero;
    rt.anchorMax = Vector2.one;
    rt.offsetMin = Vector2.zero;
    rt.offsetMax = Vector2.zero;

    // Receiver
    go.AddComponent<BackgroundPatternReceiver>();

    EditorUtility.SetDirty(go);
    EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
    Debug.Log($"[BGPattern] Added BackgroundPattern to {EditorSceneManager.GetActiveScene().name}");
}
```

---

## 💰 ANÁLISIS DE ECONOMÍA — PRECIOS Y POSICIONAMIENTO

### Contexto del sistema económico de DigitPark

| Moneda | Cómo se obtiene | Uso |
|--------|----------------|-----|
| **DC (DigitCoins)** | Jugando partidas, misiones diarias, logros | Solo cosméticos DC-tier |
| **DG (DigitGems)** | Solo compra IAP (real money) | Temas, cosméticos premium |

> Ratio de conversión orientativo: 100 DG ≈ $1 USD (ajustar según IAP tiers del proyecto)

---

### Benchmarking: precios actuales en el shop vs backgrounds

| Categoría | Precio actual | Impacto visual | Alcance |
|-----------|---------------|----------------|---------|
| Tema premium (ej. Synthwave) | ~300–500 DG | 🔴 Alto — cambia todos los colores | Global en todas las escenas |
| Background pattern | **70–190 DG** | 🟡 Medio — overlay sutil | Global en todas las escenas |
| (futuro) Title/Badge | TBD | 🟡 Medio — solo en perfil | Solo perfil y lobbies |
| (futuro) Win Effect | TBD | 🔴 Alto — solo en victoria | Solo win screens |

**Conclusión de benchmarking**: Los backgrounds son el segundo cosmético más global (igual alcance que los temas), pero con menor impacto visual. El precio correcto es ~30–50% del precio de un tema. La escala 70–190 DG es coherente.

---

### Tabla de precios final

| # | ID | Nombre | Moneda | Precio | USD equiv. | Razonamiento |
|---|----|--------|--------|--------|------------|--------------|
| 1 | `bg_neural` | Neural Network | DG | **190 DG** | ~$1.90 | El más premium: nodos/sinapsis = identidad cognitiva de DigitPark. Price anchor del catálogo. |
| 2 | `bg_circuit` | Circuit Board | DG | **170 DG** | ~$1.70 | Segundo más premium: el más icónico y on-brand. PCB = tecnología = DigitPark. |
| 3 | `bg_dna` | DNA Helix | DG | **160 DG** | ~$1.60 | Alta singularidad. La hélice en columnas es visualmente espectacular. |
| 4 | `bg_constellation` | Constellation | DG | **150 DG** | ~$1.50 | Alto aspiracional. El que más atrae en la primera vista del catálogo. |
| 5 | `bg_fingerprint` | Fingerprint | DG | **140 DG** | ~$1.40 | Muy único. Curvas orgánicas que contrastan con el resto de patrones geométricos. |
| 6 | `bg_triangles` | Triangular Mesh | DG | **120 DG** | ~$1.20 | Sofisticado y versátil. Good value mid-tier. |
| 7 | `bg_waveform` | Waveform | DG | **110 DG** | ~$1.10 | Dinámico, elegante. Precio atractivo para el medio del catálogo. |
| 8 | `bg_digits` | Digit Rain | DG | **100 DG** | ~$1.00 | Psicología: precio redondo = impulse buy. Alta identidad de marca. |
| 9 | `bg_binary` | Binary Rain | DG | **90 DG** | ~$0.90 | Reconocible y cool. Precio lower-mid para captar compradores ocasionales. |
| 10 | `bg_hexgrid` | Hex Grid | DG | **80 DG** | ~$0.80 | Limpio y moderno. Entry-level DG. |
| 11 | `bg_crosshatch` | Crosshatch | DG | **70 DG** | ~$0.70 | El más barato DG. Primera compra para jugadores que quieren estrenar el sistema. |
| 12 | `bg_grid` | Graph Grid | DC | **12,000 DC** | $0 (grind) | ~3–4 semanas de play regular. Objetivo mid-term para free players. |
| 13 | `bg_dots` | Dot Matrix | DC | **8,000 DC** | $0 (grind) | ~2 semanas. Primer objetivo alcanzable. Funciona como tutorial del sistema. |
| 14 | `bg_solid` | Solid | — | **GRATIS** | $0 | Default permanente. Nunca se puede "perder". |

**Total DG si usuario compra todos los DG**: 1,090 DG ≈ ~$10.90 USD

---

### Impacto en la economía actual

#### ✅ Efectos positivos

**1. Nuevo segmento de IAP de bajo riesgo ($0.70–$1.90)**
El catálogo de temas (300–500 DG) puede intimidar a usuarios nuevos. Los backgrounds dan un punto de entrada de $0.70–$1.00 — el precio psicológico de un café. Primera compra de DG más probable.

**2. ARPU incremental sobre compradores de temas**
Un usuario que ya compró un tema (300 DG) percibe el background como un "complemento" natural. La frase mental es: *"Ya gasté $3 en el tema, el background combina perfecto por $1.50 más."* Esto eleva el ARPU sin canibalizar ventas de temas.

**3. Retención free-to-play via DC goals**
Los jugadores que nunca comprarán DG ahora tienen 2 objetivos concretos (bg_dots a 8,000 DC, bg_grid a 12,000 DC). Esto:
- Aumenta sesiones diarias (misiones)
- Aumenta duración de sesión (rankeds para ganar DC)
- Mejora Day-7, Day-14, Day-30 retention metrics

**4. DG sink no inflacionario**
Los backgrounds absorben DG que el usuario ya tiene acumulado pero "no sabe en qué gastar". No crean DG extra — solo redistribuyen el existente en más compras.

**5. Combinatoria = más FOMO**
Con 30 temas × 13 patrones = **390 combinaciones visuales únicas**. Los usuarios que coleccionan sienten el pull de probar su tema favorito con distintos patrones.

#### ⚠️ Riesgos y mitigaciones

| Riesgo | Probabilidad | Mitigación |
|--------|-------------|------------|
| Los backgrounds son "demasiado sutiles" y los usuarios no los valoran | Media | Mostrar preview over el fondo del tema activo en el Shop (no sobre negro puro) |
| Los 2 backgrounds DC se perciben como "basura" y nadie los grindea | Baja | bg_dots y bg_grid son visualmente limpios y funcionales — no son "malos", solo menos espectaculares |
| Demasiados cosméticos = decisión paralysis en el Shop | Baja | Filtrar por moneda (DC / DG) y ordenar por ranking automáticamente |
| bg_digits y bg_neural parecen demasiado similares en el preview | Baja | Los thumbnails deben mostrarlos a opacidad real sobre fondo de tema, no al 100% de contraste |

---

### Estrategia de lanzamiento

#### Momento correcto
Lanzar con la primera actualización de contenido post-release (no en v1.0). El catálogo de temas necesita ser descubierto primero para que el upsell de backgrounds tenga contexto.

#### Bundle sugerido: "DigitPark Starter Pack" (ya planeado)
Incluir **bg_dots** GRATIS en el Starter Pack (pack D1–D3). Sirve como tutorial del sistema: el usuario instala el patrón, ve que funciona, quiere más. Primera semilla del funnel.

#### Orden de aparición en el Shop
```
Sección BACKGROUNDS
  ├── 🌟 DESTACADO: bg_neural (190 DG) — el más espectacular arriba
  ├── bg_circuit (170 DG)
  ├── bg_dna (160 DG)
  ├── bg_constellation (150 DG)
  ├── bg_fingerprint (140 DG)
  ├── bg_triangles (120 DG)
  ├── bg_waveform (110 DG)
  ├── bg_digits (100 DG)
  ├── bg_binary (90 DG)
  ├── bg_hexgrid (80 DG)
  ├── bg_crosshatch (70 DG)
  ├── 🪙 bg_grid (12,000 DC) — badge "GANABLE"
  └── 🪙 bg_dots (8,000 DC) — badge "GANABLE"
```

> El orden premium-primero maximiza la percepción de valor de la categoría.
> Los DC items al final con badge "GANABLE" no compiten con DG items visualmente.

---

## 🏗 FASE 4 — ShopItem Data (Configurar en ScriptableObjects)

Los 13 patrones comprables necesitan entradas en el sistema de Shop.

### ShopItemType nuevo
```csharp
// En ShopItemData.cs — añadir al enum ShopItemType:
BackgroundPattern = 8, // (o el siguiente disponible)
```

### Campos a rellenar por cada item

| ID | Nombre | Moneda | Precio | Rank | `currencyType` |
|----|--------|--------|--------|------|---------------|
| `bg_neural` | Neural Network | DG | **190** | 1 | `DigitGems` |
| `bg_circuit` | Circuit Board | DG | **170** | 2 | `DigitGems` |
| `bg_dna` | DNA Helix | DG | **160** | 3 | `DigitGems` |
| `bg_constellation` | Constellation | DG | **150** | 4 | `DigitGems` |
| `bg_fingerprint` | Fingerprint | DG | **140** | 5 | `DigitGems` |
| `bg_triangles` | Triangular Mesh | DG | **120** | 6 | `DigitGems` |
| `bg_waveform` | Waveform | DG | **110** | 7 | `DigitGems` |
| `bg_digits` | Digit Rain | DG | **100** | 8 | `DigitGems` |
| `bg_binary` | Binary Rain | DG | **90** | 9 | `DigitGems` |
| `bg_hexgrid` | Hex Grid | DG | **80** | 10 | `DigitGems` |
| `bg_crosshatch` | Crosshatch | DG | **70** | 11 | `DigitGems` |
| `bg_grid` | Graph Grid | DC | **12,000** | 12 | `DigitCoins` |
| `bg_dots` | Dot Matrix | DC | **8,000** | 13 | `DigitCoins` |

> El campo `previewSprite` de cada ShopItem apunta al PNG en `Resources/Backgrounds/`.
> La UI del Shop debe mostrar badge **"GANABLE 🪙"** en los 2 items DC.

---

## 🏗 FASE 5 — Shop UI (Nueva sección "Backgrounds")

**Archivo**: `Assets/_Project/Scripts/Editor/Monetization/ShopUIBuilder.cs` (modificar)

### Cambios necesarios
1. Añadir nueva sección `"Backgrounds"` entre `"Themes"` y `"Titles"` (si existen)
2. Cada card muestra:
   - Preview: el patrón PNG en miniatura sobre un fondo de color (`#0A0A14`)
   - Nombre del patrón
   - Precio en DG (o "FREE" para bg_solid)
   - Botón: "APPLY" (si desbloqueado) / "BUY X DG" (si bloqueado)
3. Al presionar "APPLY": llamar `BackgroundPatternManager.Instance.SetPattern(id)`
4. Al presionar "BUY": flujo normal de compra con DG → al confirmar, llamar `SetPattern(id)`
5. El item activo muestra un badge "ACTIVE" (igual que temas)

### Preview card
```
┌────────────────────────┐
│  [miniatura patrón     │  ← RawImage 80×80, fondo #0A0A14
│   blanco sobre azul]   │
│                        │
│  Circuit Board         │  ← TextPrimary
│  ◈ 100 DG              │  ← AccentTertiary (gold)
│  [  APPLY  ]           │  ← ButtonPrimary (o ButtonSecondary si no activo)
└────────────────────────┘
```

---

## 🏗 FASE 5b — Temas Chromatic (2 colores)

### Cambios en ThemeData.cs

```csharp
[Header("=== BACKGROUND PATTERN TINT ===")]
[Tooltip("Si true, el patrón cosmético usa patternTintColor en lugar de blanco")]
public bool isChromatic = false;

[Tooltip("Color del patrón cuando isChromatic=true. Dejar blanco si Standard.")]
public Color patternTintColor = Color.white;
```

### Cambios en BackgroundPatternReceiver.Apply()

```csharp
private Color GetPatternTint(float opacity)
{
    var theme = ThemeManager.Instance?.CurrentTheme;
    if (theme != null && theme.isChromatic)
        return new Color(theme.patternTintColor.r, theme.patternTintColor.g,
                         theme.patternTintColor.b, opacity);

    return new Color(1f, 1f, 1f, opacity); // white estándar
}
```

### Temas recomendados como Chromatic (configurar en Inspector)

| Tema | patternTintColor sugerido | Por qué |
|------|--------------------------|---------|
| Neon Dark | `#00FFFF` (primaryAccent) | El más icónico — trazas cyan puras |
| Synthwave | `#FF1493` (magenta) | Definición visual del synthwave |
| Infrared | `#FF3333` (rojo neón) | Heat signature — muy impactante |
| Toxic Lime | `#AAFF00` (lime) | Radioactivo — muy único |
| Plasma Indigo | `#8A2BE2` (índigo) | Plasma en movimiento |
| Electric Blue | `#007FFF` (eléctrico) | Energía visible |
| Aurora Borealis | `#00FF7F` (verde aurora) | El más natural del catálogo |
| Nebula | `#9D4BFF` (púrpura espacial) | Nebulosa brillante |

### Precios de temas Chromatic (sobre precio base)
```
Tema Standard = precio_base DG
Tema Chromatic = precio_base + 120 DG
Upgrade Standard → Chromatic = 120 DG (upsell diferido)
```

> El upgrade permite que un usuario que ya compró el tema Standard pueda "actualizar" a Chromatic sin pagar de nuevo por el tema completo. Esto respeta la compra anterior y crea ingresos incrementales.

---

## 🏗 FASE 6 — Persistencia Firebase

Al comprar un background, sincronizar con la base de datos:

```csharp
// En BackgroundPatternManager.SetPattern():
_ = SyncPatternToFirebase(patternId);

// En DatabaseService.cs — añadir campo:
await UpdatePlayerFields(userId, new Dictionary<string, object>
{
    { "equippedBackground", patternId }
});

// En AuthenticationService al cargar perfil:
string savedBg = playerData.equippedBackground ?? "bg_solid";
BackgroundPatternManager.Instance?.SetPattern(savedBg);
```

### Cambios en PlayerData
```csharp
// En PlayerData.cs — añadir campo:
public string equippedBackground = "bg_solid";
// Añadir también a owned list:
public List<string> ownedBackgrounds = new List<string> { "bg_solid" };
```

---

## 🏗 FASE 7 — Preview en tiempo real (Shop) — OBLIGATORIO

> **Por qué es obligatorio**: los patrones están entre 5–8% de opacidad. Un thumbnail 80×80 en el Shop no transmite el efecto. Sin preview, la conversión en backgrounds será significativamente inferior a la esperada.
> El preview debe mostrar el patrón **sobre el `primaryBackground` del tema activo del usuario**, no sobre negro puro.

Para que el usuario vea el efecto del patrón antes de comprarlo:

```csharp
// BackgroundPatternManager — método preview temporal:
public void PreviewPattern(string patternId)
{
    // Aplica visualmente SIN guardar en PlayerPrefs
    float opacity = PatternOpacity.TryGetValue(patternId, out float op) ? op : 0f;
    var targets = FindObjectsByType<BackgroundPatternReceiver>(FindObjectsSortMode.None);
    var sprite = patternId == "bg_solid" ? null : Resources.Load<Sprite>($"Backgrounds/{patternId}");
    foreach (var t in targets) t.Apply(sprite, opacity);
}

public void CancelPreview()
{
    // Restaura el patrón guardado sin modificar PlayerPrefs
    ApplyPatternToScene();
}
```

---

## 📋 CHECKLIST DE IMPLEMENTACIÓN

### Paso a paso (en orden)

**Backgrounds (sistema base)**
- [ ] **FASE 1** — Crear `BackgroundPatternManager.cs`
- [ ] **FASE 2** — Crear `BackgroundPatternReceiver.cs`
- [ ] Compilar sin errores en Unity
- [ ] **FASE 3** — Crear `BackgroundPatternSetup.cs` (editor tool)
- [ ] Ejecutar tool → **37 escenas** (28 temáticas + 9 CashBattle)
- [ ] Verificar en 3 escenas que `BackgroundPattern` GO existe en índice 1
- [ ] Añadir `BackgroundPatternManager` al prefab de BootManager (DontDestroyOnLoad)
- [ ] Probar: `SetPattern("bg_circuit")` → patrón aparece en MainMenu y CashBattleHub ✅
- [ ] Probar: cambiar escena → patrón persiste ✅
- [ ] **FASE 4** — Añadir `BackgroundPattern` a `ShopItemType` enum
- [ ] Crear 13 `ShopItemData` ScriptableObjects en `Resources/Configs/Shop/Backgrounds/`
- [ ] **FASE 5** — Añadir sección Backgrounds al Shop UIBuilder (orden: premium→DC)
- [ ] **FASE 6** — Añadir `equippedBackground` a `PlayerData.cs` + sync Firebase
- [ ] **FASE 7** — Preview en tiempo real en Shop (obligatorio — preview sobre primaryBackground del tema activo)

**Temas Chromatic (2 colores)**
- [ ] **FASE 5b** — Añadir `isChromatic` + `patternTintColor` a `ThemeData.cs`
- [ ] Actualizar `BackgroundPatternReceiver.Apply()` para leer `theme.patternTintColor`
- [ ] Configurar 8 temas Chromatic en Inspector (ver tabla FASE 5b)
- [ ] Actualizar precios de temas Chromatic (+120 DG sobre precio base)
- [ ] Añadir badge `CHROMATIC ✨` en Shop UI para temas Chromatic
- [ ] Implementar upgrade path Standard → Chromatic (120 DG adicional)
- [ ] Probar: Neon Dark + bg_circuit → trazas cyan ✅ | Arctic + bg_circuit → trazas blancas ✅

**QA final**
- [ ] AgeVerification + Onboarding: confirmar que NO tienen `BackgroundPattern` GO
- [ ] bg_solid: confirmar que no muestra ningún patrón en ninguna escena
- [ ] Chromatic ON/OFF: tema Standard → blanco, tema Chromatic → acento ✅
- [ ] Probar en dispositivo iOS + Android

---

## ✅ Assets — todos integrados

**14 patrones en `Resources/Backgrounds/`** — 13 PNGs + 13 .meta + bg_solid (sin sprite).

| Asset | Estado | Notas |
|-------|--------|-------|
| `bg_neural.png` | ✅ | Integrado 15-mar 18:12 |
| `bg_circuit.png` | ✅ | Integrado 15-mar 17:50 |
| `bg_dna.png` | ✅ | Integrado 15-mar 18:12 |
| `bg_constellation.png` | ✅ | Integrado 15-mar 18:12 |
| `bg_fingerprint.png` | ✅ | Integrado 15-mar 18:12 |
| `bg_triangles.png` | ✅ | Integrado 15-mar 18:12 |
| `bg_waveform.png` | ✅ | Integrado 15-mar 18:24 |
| `bg_digits.png` | ✅ | Regenerado 15-mar 18:24 (alta calidad) |
| `bg_binary.png` | ✅ | Integrado 15-mar 18:12 |
| `bg_hexgrid.png` | ✅ | Integrado 15-mar 17:50 |
| `bg_crosshatch.png` | ✅ | Integrado 15-mar 18:12 |
| `bg_grid.png` | ✅ | Integrado 15-mar 18:12 |
| `bg_dots.png` | ✅ | Integrado 15-mar 17:50 |

---

## 📁 Estructura de archivos final

```
Assets/_Project/
├── Resources/
│   └── Backgrounds/          ← sprites cargados en runtime
│       ├── bg_binary.png + .meta
│       ├── bg_circuit.png + .meta
│       ├── bg_constellation.png + .meta
│       ├── bg_crosshatch.png + .meta
│       ├── bg_digits.png + .meta
│       ├── bg_dna.png + .meta
│       ├── bg_dots.png + .meta
│       ├── bg_fingerprint.png + .meta
│       ├── bg_grid.png + .meta
│       ├── bg_hexgrid.png + .meta
│       ├── bg_neural.png + .meta
│       ├── bg_triangles.png + .meta
│       └── bg_waveform.png + .meta
│
├── Scripts/Runtime/
│   └── Features/Cosmetics/
│       ├── BackgroundPatternManager.cs   ← FASE 1
│       └── BackgroundPatternReceiver.cs  ← FASE 2
│
└── Scripts/Editor/
    └── Tools/
        └── BackgroundPatternSetup.cs     ← FASE 3
```
