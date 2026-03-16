# DIGITPARK — COSMETIC BACKGROUNDS DESIGN DOC
**Estado**: Diseño / Pre-implementación
**Última actualización**: 2026-03-15
**Autor**: Diseño + Claude

---

## 🎯 Concepto Central

El sistema de backgrounds es **una segunda dimensión cosmética**, independiente de los temas.

```
VISUAL FINAL = Color base del tema (PrimaryBackground)
             + Patrón encima (BackgroundPattern @ opacidad fija)
```

- El **color** lo controla el tema activo (`ThemeData.primaryBackground`)
- El **patrón** lo elige y compra el usuario (cosmético independiente)
- Una vez comprado, aplica en **todas las 28 escenas temáticas** de la app

### Por qué funciona bien

Un usuario con tema `Electric Blue` puede combinar:
- Solid (default gratuito) → limpio y minimalista
- Circuit Board → feel de hacker
- Neural Network → feel cognitivo/intelectual
- Constellation → feel premium/espacial

Mismo tema, personalidades distintas. El valor percibido de cada compra es alto porque el cambio es global y visible en cada pantalla.

---

## 📦 Catálogo de Backgrounds

### TIER 0 — Incluido gratis

| ID | Nombre | Descripción | Opacidad | Estado |
|----|--------|-------------|----------|--------|
| `bg_solid` | **Solid** | Sin patrón. Color sólido puro del tema activo. | N/A | ✅ Default |

---

### TIER DC — Ganables (DigitCoins)

| ID | Nombre | Descripción | Opacidad | Precio | Estado |
|----|--------|-------------|----------|--------|--------|
| `bg_dots` | **Dot Matrix** | Grid perfecto de puntos pequeños. Limpio, minimalista, el más versátil. | **8%** | **8,000 DC** | ✅ Integrado |
| `bg_grid` | **Graph Grid** | Cuadriculado fino estilo papel milimetrado. Preciso, técnico. | **7%** | **12,000 DC** | ✅ Integrado |

---

### TIER 1 — Entry DG (70–110 DG)

| ID | Nombre | Descripción | Opacidad | Precio | Estado |
|----|--------|-------------|----------|--------|--------|
| `bg_crosshatch` | **Crosshatch** | Líneas diagonales cruzadas, muy finas. Elegante y discreto. | **6%** | **70 DG** | ✅ Integrado |
| `bg_hexgrid` | **Hex Grid** | Hexágonos outline dispersos. Geométrico y moderno. | **6%** | **80 DG** | ✅ Integrado |
| `bg_binary` | **Binary Rain** | Columnas verticales de 0s y 1s cayendo, estilo Matrix. | **6%** | **90 DG** | ✅ Integrado |
| `bg_digits` | **Digit Rain** | Lluvia de dígitos 0-9. El más on-brand para DigitPark. | **12%** | **100 DG** | ✅ Integrado |
| `bg_waveform` | **Waveform** | Forma de onda de audio / ECG, horizontal repeating. | **8%** | **110 DG** | ✅ Integrado |

---

### TIER 2 — Mid DG (120–160 DG)

| ID | Nombre | Descripción | Opacidad | Precio | Estado |
|----|--------|-------------|----------|--------|--------|
| `bg_triangles` | **Triangular Mesh** | Malla de triángulos irregulares, estilo low-poly geométrico. | **7%** | **120 DG** | ✅ Integrado |
| `bg_constellation` | **Constellation** | Puntos dispersos conectados por líneas tenues, como un mapa estelar. | **7%** | **150 DG** | ✅ Integrado |
| `bg_dna` | **DNA Helix** | Doble hélice de ADN repeating pattern. Científico, premium. | **6%** | **160 DG** | ✅ Integrado |

---

### TIER 3 — Premium DG (140–190 DG)

| ID | Nombre | Descripción | Opacidad | Precio | Estado |
|----|--------|-------------|----------|--------|--------|
| `bg_fingerprint` | **Fingerprint** | Líneas de huella dactilar concéntricas y curvas. Biométrico, único. | **5%** | **140 DG** | ✅ Integrado |
| `bg_circuit` | **Circuit Board** | Trazas de PCB con nodos. El más icónico y on-brand. | **5%** | **170 DG** | ✅ Integrado |
| `bg_neural` | **Neural Network** | Nodos conectados por sinapsis, como un grafo cerebral. | **6%** | **190 DG** | ✅ Integrado |

---

## 🎨 Prompts DALL-E para generar los faltantes

> **REGLA para todos los prompts**: El PNG resultante debe ser blanco puro sobre negro puro, seamless tileable, sin gradientes en las líneas. Se usará como overlay layer con color blanco @ opacidad indicada.

---

### `bg_grid` — Graph Grid

```
Seamless tileable graph paper grid pattern,
thin white lines on pure black background,
regular square grid with major grid lines every 5 squares (slightly thicker),
minor grid lines 1px, major grid lines 1.5px,
flat 2D geometric, no gradients, no shading,
clean technical drafting style,
512x512 pixels, pure white lines pure black background
```

---

### `bg_crosshatch` — Crosshatch

```
Seamless tileable crosshatch pattern,
two sets of parallel diagonal lines at 45 degrees crossing each other,
very thin white lines (1px) on pure black background,
lines spaced 20px apart, perfectly even,
clean flat vector, no gradients, no variation in line weight,
512x512 pixels, pure white on pure black
```

---

### `bg_binary` — Binary Rain

```
Seamless tileable binary code pattern,
vertical columns of 0s and 1s falling downward,
monospace font digits, white on black,
digits small (8px), columns spaced 18px apart,
varying vertical positions of digits within each column,
clean flat 2D, no glow effects, no gradients,
matrix-style data stream aesthetic,
512x512 pixels, pure white digits on pure black
```

---

### `bg_constellation` — Constellation

```
Seamless tileable constellation star map pattern,
small white dots (stars) of varying sizes (2-4px) scattered randomly,
thin white lines (0.5px) connecting nearby stars in geometric groups,
low density (about 25 stars total in tile),
clean flat 2D, no glow, no gradients,
deep space astronomical chart style,
512x512 pixels, pure white on pure black
```

---

### `bg_triangles` — Triangular Mesh

```
Seamless tileable low-poly triangular mesh pattern,
irregular triangles formed by connected vertices,
thin white outline strokes (1px) on pure black background,
triangles of varying sizes, densely packed with no gaps,
flat 2D wireframe, no fill, no gradients, no shading,
geometric abstract technical style,
512x512 pixels, pure white lines on pure black
```

---

### `bg_neural` — Neural Network

```
Seamless tileable neural network graph pattern,
white circular nodes (4-6px diameter) connected by thin white lines (0.5px),
nodes scattered organically, each connected to 3-5 neighbors,
medium density (about 20 nodes per tile),
no directionality (not a flow chart), purely spatial graph,
flat 2D, no gradients, no glow effects,
AI brain network aesthetic,
512x512 pixels, pure white on pure black
```

---

### `bg_fingerprint` — Fingerprint

```
Seamless tileable fingerprint ridge pattern,
concentric curved white lines (1px) mimicking fingerprint whorls,
dense ridges spaced 6px apart, slight organic curvature,
full tile coverage, no gaps,
flat 2D, no gradients, perfectly even line weight,
biometric identification aesthetic,
512x512 pixels, pure white lines on pure black
```

---

### `bg_dna` — DNA Helix

```
Seamless tileable DNA double helix pattern,
two intertwining helical strands shown as white lines (1.5px),
horizontal rungs connecting the strands every 20px,
clean side-view projection, perfectly symmetric,
repeating vertically with no visible tile edge,
flat 2D scientific diagram style, no gradients,
512x512 pixels, pure white on pure black
```

---

### `bg_waveform` — Waveform

```
Seamless tileable audio waveform pattern,
multiple horizontal sine waves of varying amplitudes stacked vertically,
thin white lines (1px), waves slightly offset from each other,
smooth continuous curves, 5-7 wave rows per tile,
no sharp edges, organic flowing wave aesthetic,
flat 2D, no gradients, no glow,
512x512 pixels, pure white on pure black
```

---

### `bg_digits` — Digit Rain (REGENERAR)

```
Seamless tileable matrix of random digits 0-9,
white monospace digits on pure black background,
digit size 10px, arranged in a tight irregular grid,
digits at slightly varying rotations (±5 degrees maximum),
no columns or rows — fully random scattered placement,
high contrast, crisp edges, flat 2D,
512x512 pixels, pure white digits on pure black,
NO gradients, NO glow, NO blur
```

---

## 💡 Notas de Implementación (futura)

### Jerarquía en escena
```
Canvas
  ├── Background         (Image — PrimaryBackground — cambia con tema)
  ├── BackgroundPattern  (Image — sprite PNG — color white @ opacidad fija)
  └── SafeArea
        └── ...UI...
```

### Lo que cambia vs lo que no cambia
- `Background.color` → cambia con el tema (`ThemeApplier PrimaryBackground`)
- `BackgroundPattern.sprite` → cambia con la elección cosmética del usuario
- `BackgroundPattern.color` → **FIJO** `new Color(1,1,1, opacidad_del_patron)` — nunca cambia
- `BackgroundPattern` → **NO recibe ThemeApplier**

### UserPreferences (a implementar en el futuro)
```csharp
// Se guarda separado del tema activo
PlayerPrefs.SetString("active_background", "bg_circuit"); // o bg_solid, bg_dots, etc.
```

### Escenas afectadas
Las 28 escenas temáticas (todo excepto las 13 excluidas). Ver THEME_AUDIT_MASTER.md.

---

## 📊 Resumen económico

| Tier | Moneda | Precio | Backgrounds | Total máximo |
|------|--------|--------|-------------|--------------|
| DC | DigitCoins | 8,000–12,000 DC | 2 | grind gratuito |
| Entry | DG | 70–110 DG | 5 | ~450 DG |
| Mid | DG | 120–160 DG | 3 | ~430 DG |
| Premium | DG | 140–190 DG | 3 | ~500 DG |
| Default | — | Gratis | 1 (Solid) | — |

**Máximo potencial (usuario que compra todos los DG)**: 1,090 DG ≈ ~$10.90 USD
Precio bajo, impulse-buy territory, sin pay-to-win. Ver análisis completo en `BACKGROUNDS_IMPLEMENTATION_PLAN.md`.

---

## 🎨 Color `primaryBackground` por tema (Capa 1)

> Estos son los colores exactos que verá el usuario en `Canvas/Background` cuando tenga cada tema activo.
> La Capa 2 (patrón cosmético) se superpone encima con blanco @ opacidad fija.
>
> **Regla de diseño**: todos los fondos son oscuros (luminosidad ~2–7%) con un tinte de hue que identifica la familia de color del tema. Los acentos neón deben POP sobre este fondo.
>
> **Leyenda evaluación**: ✅ Correcto · ⚠️ Revisar (tinte demasiado sutil) · 🔧 Sugerido (hex propuesto)

### Temas gratuitos + ganables

| Tema | `themeId` | `primaryBackground` actual | Descripción visual | Evaluación |
|------|-----------|---------------------------|-------------------|------------|
| **Neon Dark** | `neon_dark` | `#0A0A14` | Negro con tinte azul-violeta. Base de referencia del sistema. | ✅ Referencia |
| **Monochrome** | `Monochrome` | `#18181B` | Gris oscuro casi puro, R≈G≈B. El más neutro y limpio. | ✅ Perfecto |
| **Emerald** | `Emerald` | `#0A1A14` | Verde-teal oscuro equilibrado. G alto, B medio. | ✅ Correcto |
| **Electric Blue** | `ElectricBlue` | `#08101E` | Azul profundo. B>>G>R, claramente azul. | ✅ Correcto |

### Familia azul / frío

| Tema | `themeId` | `primaryBackground` actual | Descripción visual | Evaluación |
|------|-----------|---------------------------|-------------------|------------|
| **Arctic** | `Arctic` | `#0C1929` | Azul acero oscuro. El más luminoso de todos — adecuado para tema "helado". | ✅ Correcto |
| **Deep Ocean** | `DeepOcean` | `#0A1520` | Marino-teal oscuro. Muy similar a Arctic visualmente. | ⚠️ Poco distinto de Arctic — propuesto `#071820` (más teal) |
| **Thunder** | `Thunder` | `#040610` | Casi negro azul-marino. Muy similar a Aurora y Bioluminescence. | ⚠️ Poco distinto — propuesto `#050814` (más eléctrico) |
| **Titanium** | `Titanium` | `#10131A` | Gris azulado metálico oscuro. Bien diferenciado de Monochrome. | ✅ Correcto |
| **Y2K Chrome** | `Y2KChrome` | `#0A0C10` | Gris azulado frío. Ligero, cromado. Correcto. | ✅ Correcto |
| **Plasma Indigo** | `PlasmaIndigo` | `#0A0A1E` | Igual que NeonDark pero con B más alto (14→1E). Claro índigo. | ✅ Correcto |

### Familia verde / natura

| Tema | `themeId` | `primaryBackground` actual | Descripción visual | Evaluación |
|------|-----------|---------------------------|-------------------|------------|
| **Toxic Lime** | `ToxicLime` | `#0C1A08` | Verde oscuro con G dominante. Perfecto para el feel lime. | ✅ Correcto |
| **Matrix** | `Matrix` | `#080F08` | Verde casi negro, más oscuro que ToxicLime. R=B mínimo. | ✅ Correcto |
| **Aurora Borealis** | `Aurora` | `#040A0C` | Casi negro con tinte teal mínimo. Aurora necesita más verde. | ⚠️ Propuesto `#040E0A` (más verde-teal visible) |
| **Bioluminescence** | `Bioluminescence` | `#02050A` | El más oscuro de todos. Mar abismal. Intencional: los acentos brillan más. | ✅ Válido (bioluminiscencia = oscuridad + glow) |

### Familia rojo / cálido / fuego

| Tema | `themeId` | `primaryBackground` actual | Descripción visual | Evaluación |
|------|-----------|---------------------------|-------------------|------------|
| **Infrared** | `Infrared` | `#1A0808` | Rojo oscuro puro. R alto, G=B bajos iguales. | ✅ Correcto |
| **Blood Moon** | `BloodMoon` | `#080202` | Casi negro con tinte rojo mínimo. Más oscuro que Infrared. | ✅ Correcto |
| **Volcanic** | `Volcanic` | `#1A0C08` | Naranja-marrón oscuro. R alto, G medio, B bajo. | ✅ Correcto |
| **Electric Orange** | `ElectricOrange` | `#0C0602` | Marrón-naranja muy oscuro. R>G>>B. | ✅ Correcto |
| **Coral Surge** | `CoralSurge` | `#1A0C10` | Rosa-rojo oscuro. El coral es más naranja. | ⚠️ Propuesto `#1A0E0C` (más naranja-coral) |
| **Sakura** | `Sakura` | `#1A0F14` | Rosa oscuro cálido. R alto, G medio, B medio-bajo. | ✅ Correcto |

### Familia púrpura / violeta / rosa

| Tema | `themeId` | `primaryBackground` actual | Descripción visual | Evaluación |
|------|-----------|---------------------------|-------------------|------------|
| **Nebula** | `Nebula` | `#0C0816` | Púrpura espacial oscuro. B alto vs G bajo. | ✅ Correcto |
| **Ultraviolet** | `Ultraviolet` | `#070210` | Casi negro violeta. B alto, G y R mínimos. | ✅ Correcto |
| **Synthwave** | `Synthwave` | `#0F0414` | Púrpura-magenta oscuro. R>B>>G. Feel synthwave clásico. | ✅ Correcto |
| **Vaporwave** | `Vaporwave` | `#0C0614` | Índigo-púrpura oscuro. Similar a Synthwave pero más azul. | ✅ Correcto |
| **Outrun** | `Outrun` | `#0C040F` | Púrpura-magenta oscuro profundo. Ligeramente más rosa que Vaporwave. | ✅ Correcto |
| **Cyber Fuchsia** | `CyberFuchsia` | `#1A0A1A` | Magenta oscuro. R=B simétrico con G bajo. Feel fuchsia claro. | ✅ Correcto |
| **Phantom** | `Phantom` | `#08060E` | Casi negro con tinte violeta. Prácticamente indistinguible del negro. | ⚠️ Propuesto `#0C0812` (más presencia violeta) |

### Familia oscura / abstracta

| Tema | `themeId` | `primaryBackground` actual | Descripción visual | Evaluación |
|------|-----------|---------------------------|-------------------|------------|
| **Glitch** | `Glitch` | `#06080A` | Casi negro puro con mínimo tinte azul-teal. Sin identidad visible. | ⚠️ Propuesto `#060D0C` (teal digital sutil) |
| **Ice x Fire** | `IceFire` | `#07070A` | Casi negro neutro. Background neutral es una elección consciente (equilibrio hielo/fuego). | ⚠️ Propuesto `#07090B` (mínimo tinte hielo) |
| **Void** | `Void` | `#020408` | El segundo más oscuro. El más cercano al negro puro. Intencional: "el vacío". | ✅ Perfecto |

---

### Resumen de correcciones sugeridas

| Tema | Hex actual | Hex propuesto | Motivo |
|------|-----------|---------------|--------|
| Deep Ocean | `#0A1520` | `#071820` | Más teal, diferencia visual con Arctic |
| Thunder | `#040610` | `#050814` | Más azul eléctrico, menos similar a Aurora |
| Aurora Borealis | `#040A0C` | `#040E0A` | Más verde-teal para aurora boreal |
| Coral Surge | `#1A0C10` | `#1A0E0C` | Más naranja-coral, menos rojo-rosado |
| Phantom | `#08060E` | `#0C0812` | Más presencia de violeta visible |
| Glitch | `#06080A` | `#060D0C` | Tinte teal digital, diferencia de Void |
| Ice x Fire | `#07070A` | `#07090B` | Mínimo tinte helado, diferencia de Void |

> **Nota**: estos cambios son sugerencias de diseño. Son cambios mínimos que no afectan la legibilidad ni los contrastes — solo añaden más identidad de color al background. Requieren editar el valor `primaryBackground` en cada ScriptableObject (`.asset`) en Unity → no hay cambio de código, solo de asset.

---

## ✅ Estado actual

| Tarea | Estado |
|-------|--------|
| 12 PNGs integrados en `Resources/Backgrounds/` con .meta | ✅ Hecho |
| bg_waveform — integrado ✅ | ✅ Hecho |
| bg_digits — regenerado con alto contraste ✅ | ✅ Hecho |
| Precios finales DG definidos | ✅ Ver tabla arriba |
| Plan de implementación completo | ✅ `docs/BACKGROUNDS_IMPLEMENTATION_PLAN.md` |
| Implementar `BackgroundPatternManager.cs` | ⬜ Pendiente |
| Implementar `BackgroundPatternReceiver.cs` | ⬜ Pendiente |
| Implementar `BackgroundPatternSetup.cs` (editor tool) | ⬜ Pendiente |
| Ejecutar tool en 28 escenas | ⬜ Pendiente |
| Sección Backgrounds en Shop UI | ⬜ Pendiente |
| Persistencia Firebase (`equippedBackground`) | ⬜ Pendiente |
