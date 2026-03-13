# DIGITPARK — MEGA PROMPT: Análisis Profesional de Economía In-Game

> **Uso:** Lee este prompt completo antes de iniciar el análisis. Cubre TODAS las áreas
> económicas de DigitPark desde una perspectiva de Game Economy Designer senior.
> El objetivo es llegar a recomendaciones accionables con números concretos.

---

## DECISIONES DE DISEÑO YA TOMADAS (no cuestionar, aplicar como constraints)

> Estas decisiones son firmes y el análisis debe respetarlas:

1. **DigitGems (DG) = moneda de COMPRA ÚNICAMENTE.** No se ganan por gameplay, daily rewards,
   ni achievements. Solo se obtienen via IAP. Esto fue verificado legalmente:
   cosméticos a precio fijo con moneda premium son 100% legales en todos los mercados
   (USA, EU, Bélgica, Holanda, UK, China, Corea, Japón, Brasil). La única restricción legal
   aplica si DG se usara para mecánicas ALEATORIAS (gacha/loot boxes) — DigitPark NO tiene esto.

2. **CashBattle opera con Triumph SDK.** No involucra DG ni DC en ningún momento.
   Es un sistema separado con entrada/salida en USD real via Triumph. No forma parte
   de esta economía y NO debe analizarse aquí.

3. **No existe Battle Pass.** No evaluar ni recomendar uno.

4. **Los 4 temas "ganables" (Emerald, Electric Blue, Electric Violet, Monochrome)** cambian
   de precio USD a precio en DG. Son desbloqueables de DOS formas:
   - **Forma earn:** Achievement específico muy difícil (único por tema)
   - **Forma buy:** Compra directa con DG
   El precio en DG debe determinarse en el análisis (ver Área 6).

5. **Daily Rewards y Achievements ya NO dan DG.** Los días que antes daban DG ahora dan DC.
   Los achievements que daban gems ahora dan DC o desbloquean directamente temas ganables.

6. **El exchange DG→DC (PurchaseCoinsWithGems) debe eliminarse.** Si DG es solo-compra,
   no tiene sentido convertirlos en moneda gratis.

---

## CONTEXTO DEL PROYECTO

**DigitPark** es un juego mobile de mini-juegos competitivos (iOS + Android) con:
- 5 mini-juegos cognitivos: DigitRush, FlashTap, MemoryPairs, OddOneOut, QuickMath
- Modos: Práctica (free), Ranked 1v1, Cognitive Sprint, Tournaments
- Audiencia: jugadores casuales-competitivos, 15–45 años
- Modelo de negocio: F2P + Cosmetics IAP (DG) + Feature Unlocks (USD directo)
- Plataformas: iOS + Android

**Monedas del juego:**
| Símbolo | Nombre | Tipo | Fuente |
|---------|--------|------|--------|
| `DC` | DigitCoins | Soft currency | Gameplay, misiones, daily rewards, achievements |
| `DG` | DigitGems | Hard currency (SOLO COMPRA) | IAP únicamente |

---

## ÁREA 1 — MONEDAS SUAVES (DigitCoins)

### Estado actual a analizar:
- Starting coins: 1,000 DC
- Fuentes de ingreso: Daily Rewards (todos los días ahora en DC), Missions, Achievements,
  posiblemente wins en ranked
- Usos: Shop (frames, avatars, bundles DC→cosmética DC-priced)
- **NUEVO:** Los días 3 y 7 de Daily Rewards que antes daban DG ahora deben dar DC.
  Los achievements que antes daban DG ahora dan DC.

### Preguntas clave:
1. ¿1,000 DC de inicio es la cantidad correcta para el feeling de "valor inmediato"?
2. ¿A qué velocidad acumula DC un jugador activo diario vs. casual (3x/semana)?
3. ¿Qué precio en DC tienen los items DC-comprable del Shop (frames, avatars)?
   ¿Están calibrados para que F2P compre algo en 7–14 días?
4. ¿Deberían existir recompensas de DC por win/loss en Ranked 1v1? ¿Cuánto exactamente?
5. ¿Cuál es el "coin sink" principal? ¿Se acumulan sin propósito después de mes 2?
6. ¿Debería haber una "First Win of the Day" bonus en DC?

### Benchmark de referencia:
- Clash Royale: ~200 gold/día F2P activo; items baratos accesibles en ~5 días
- Duolingo: streak → in-app currency → cosmetics (loop de engagement)
- 8 Ball Pool: coins por jugar, entry fees para torneos

---

## ÁREA 2 — MONEDAS PREMIUM (DigitGems) — SOLO COMPRA

### CONSTRAINT FIRME: DG solo se obtienen via IAP. Nunca gratis.

### Estado actual a analizar:
- Starting gems: 100 DG (regalo de bienvenida, no ganado — revisar si mantener)
- Fuentes: IAP únicamente
- IAP packs actuales:
  | Pack | Gems base | Bonus | Precio estimado |
  |------|-----------|-------|----------------|
  | Starter | 100 DG | 0% | ~$0.99 |
  | Popular | 500 DG | 10% | ~$4.99 |
  | Value | 1,200 DG | 20% | ~$9.99 |
  | Best Value | 2,500 DG | 25% | ~$19.99 |
  | Super | 6,500 DG | 30% | ~$49.99 |
  | Mega | 14,000 DG | 35% | ~$99.99 |
- Usos: Temas premium (precio a definir en DG), temas ganables buy-path

### Preguntas clave:
1. ¿Los 100 DG de bienvenida deben mantenerse, reducirse a 50, o eliminarse?
   - Si se mantienen, ¿generan conversión o simplemente retrasan la primera compra?
   - ¿Es mejor una "welcome offer" de 50% off el primer pack en lugar de DG gratis?
2. ¿La curva de valor de los packs ($0.99→$99.99) maximiza ARPPU?
   - ¿Falta un pack intermedio de $1.99 entre $0.99 y $4.99?
3. ¿Los bonus porcentuales (10%–35%) son suficientemente motivantes para la compra grande?
   - Benchmark: Fortnite da 20% en el tier más popular — ¿DigitPark es competitivo?
4. ¿Debe existir un "Starter Pack" especial (tema + DG a 50–60% off) visible solo los
   primeros 3 días para maximizar la primera conversión?
5. ¿Cuál es el precio en DG de los temas premium y de los temas ganables? (Ver Área 6)

### Benchmark de referencia:
- Supercell model: gems extremadamente escasos y valiosos, raramente gratis
- Fortnite V-Bucks: valor percibido por bonus en packs + starter pack con skin exclusiva
- Brawl Stars: gems solo de compra (casi), skins caras pero aspiracionales

---

## ÁREA 3 — SISTEMA DE DAILY REWARDS

### Estado actual (ciclo 7 días) — ACTUALIZADO:
| Día | Recompensa | Cantidad (actual) | Propuesta a analizar |
|-----|-----------|-------------------|---------------------|
| 1 | DigitCoins | 50 DC | ¿Correcto para D1? |
| 2 | DigitCoins | 75 DC | ? |
| 3 | DigitCoins | **reemplaza los 5 DG** | ¿Cuánto DC equivale al "valor" de 5 DG? |
| 4 | DigitCoins | 100 DC | ? |
| 5 | XP | 200 XP | ¿Anticlímax? ¿Reemplazar con DC? |
| 6 | DigitCoins | 150 DC | ? |
| 7 | DigitCoins | **reemplaza los 25 DG** | ¿Cuánto DC para que día 7 sea impactante? |

**Milestones de streak actuales:** 7d → +100 DC | 14d → +250 DC | 30d → +500 DC

### Preguntas clave:
1. ¿Cuánto DC debe dar el Día 3 para mantener la motivación que antes daba "5 gems"?
   (El jugador ya no puede obtener DG gratis, así que la curva DC debe compensar ese feeling)
2. ¿Cuánto DC debe dar el Día 7 para ser el "gran reward semanal" sin DG?
   Propuesta: ¿500 DC? ¿750 DC? ¿Un multiplicador de lo ganado esa semana?
3. ¿El Día 5 de XP es un anticlímax que genera abandono de streak? ¿Cambiar a DC?
4. ¿Los milestones de streak (7/14/30 días) deberían ser la vía para desbloquear temas
   ganables DIRECTAMENTE (sin DG), complementando el path de achievements?
5. ¿Debería el ciclo escalar semana a semana (semana 2 > semana 1) o reiniciarse?
6. ¿Una "catch-up mechanic" (faltó 1 día → sin penalización) mejora D7–D30 retention?
7. ¿Cuál es el valor DC total semanal F2P para que se sienta generoso pero sin
   inflar el mercado DC?

### Target de retención:
- D1→D7: 30% (industria casual)
- D7→D30: 15%
- Daily Rewards deben contribuir ~20% a esos targets

---

## ÁREA 4 — MISIONES DIARIAS Y SEMANALES

### Estado actual:
**Misiones Diarias (ejemplos):**
- Jugar 3/5 partidas, Ganar 1/3 partidas
- Score 500/2,000/5,000, Score total 1,000/3,000
- Por juego específico: digitrush_2, flashtap_2, memory_2, oddoneout_2, quickmath_2
- Precisión 80%+, Perfect score, Sprint x1, Jugar los 5 juegos

**Misiones Semanales (30 misiones):**
- Ganar 10/20 partidas, Jugar 20/30 partidas
- Score acumulado 25k/50k, Precisión 5 veces, Racha 5 wins
- Tournament games, All 5 games

**CONSTRAINT:** Misiones dan DC únicamente (no DG).

### Preguntas clave:
1. ¿Cuántas misiones diarias simultáneas? (Recomendado industria: 3 activas)
2. ¿Las misiones "all 5 games" fuerzan demasiado al jugador?
3. ¿Cuál es la recompensa DC correcta por dificultad?
   - Fácil (jugar 3 partidas): ? DC
   - Media (ganar 3 partidas): ? DC
   - Difícil (perfect score / precisión 80%): ? DC
   - Muy difícil (ganar 10 ranked): ? DC
4. ¿Debería existir una "misión estrella" diaria de muy alta dificultad con recompensa
   que acerque al jugador al desbloqueo de un tema gannable?
5. ¿Las misiones semanales tienen peso suficiente para que el jugador planifique su semana?
6. ¿Necesitan las misiones semanales un objetivo visual de "progreso hacia tema gannable"?

---

## ÁREA 5 — ACHIEVEMENTS (52 en total)

### Estado actual — ACTUALIZADO (sin DG):
| Tier | DC reward | DG reward | Desbloqueo especial |
|------|-----------|-----------|---------------------|
| Fácil | 50–100 DC | 0 DG | — |
| Medio | 150–300 DC | 0 DG | — |
| Difícil | 300–600 DC | 0 DG | — |
| Legendario | 600–1,500 DC | 0 DG | ¿Desbloquea tema gannable directamente? |

**Total si se completan los 52 (solo DC):** a determinar en análisis

**Los 4 temas ganables deben tener cada uno un achievement legendario específico:**
| Tema | Achievement propuesto | Dificultad |
|------|----------------------|-----------|
| Emerald | ? (ej: 365 días login) | Extrema |
| Electric Blue | ? (ej: 1,000 wins ranked) | Extrema |
| Electric Violet | ? (ej: Perfect score 100 veces) | Extrema |
| Monochrome | ? (ej: Nivel 50) | Extrema |

### Preguntas clave:
1. ¿Cuánto DC total lifetime (52 achievements) es el número correcto?
   - Demasiado: infla DC, el shop pierde valor
   - Muy poco: los achievements no motivan
   - Target sugerido: entre 8,000–15,000 DC total lifetime
2. ¿Cuáles son los 4 achievements específicos más adecuados para cada tema gannable?
   (Deben ser extremadamente difíciles para que la earn-path sea aspiracional)
3. ¿Los achievements tienen distribución temporal correcta?
   (algunos en D1, algunos en mes 6, algunos en año 1)
4. ¿Deberían existir achievements "ocultos" para sorpresa?
5. ¿El salto de dificultad Medio→Difícil→Legendario es percibido como justo?

---

## ÁREA 6 — SHOP (Cosmética — PRECIOS A DEFINIR EN DG)

### Estado actual a reformular:

**Gem Packs (IAP — igual, en USD):**
| Pack | Gems | Bonus | Precio |
|------|------|-------|--------|
| Starter | 100 DG | 0% | ~$0.99 |
| Popular | 500 DG | 10% | ~$4.99 |
| Value | 1,200 DG | 20% | ~$9.99 |
| Best Value | 2,500 DG | 25% | ~$19.99 |
| Super | 6,500 DG | 30% | ~$49.99 |
| Mega | 14,000 DG | 35% | ~$99.99 |

**Temas Premium (15 — PRECIO EN DG A DEFINIR):**
- Actualmente: $2.50 USD directo
- Nueva propuesta: precio en DG (¿250 DG? ¿300 DG?)
- Bundle 15 temas en DG: precio con descuento del 30%

**Temas Ganables (4: Emerald, Electric Blue, Electric Violet, Monochrome — PRECIO EN DG A DEFINIR):**
- Actualmente: $1.50 USD directo
- Nueva propuesta: precio en DG (¿300 DG? ¿350 DG? ¿400 DG?)
- También desbloqueables via achievement específico (gratis si lo completas)
- La lógica de precio: deben costar MÁS que los premium regulares en DG,
  porque tienen el earn-path como alternativa (eso los hace aspiracionales)

**Features Premium (USD directo — sin cambios):**
| Feature | Precio |
|---------|--------|
| Crear Torneos | $3.99 |
| CashBattle Create | $6.99 |
| Tournament Bundle | $8.99 |

**Cosmética DC-priced (frames, avatars):**
| Item | Precio actual |
|------|--------------|
| Frame Bronze | 200 DC |
| Frame Silver | 400 DC |
| Frame Platinum | 1,200 DC |

### Preguntas clave:
1. **CENTRAL:** ¿Cuántos DG deben costar los 15 temas premium? (Referencia: $2.50 ≈ 250 DG)
   ¿Es 250 DG el precio correcto o debe ser diferente para que la economía tenga sentido?
2. **CENTRAL:** ¿Cuántos DG deben costar los 4 temas ganables (earn OR buy)?
   - Deben costar MÁS que premium regular para que el earn-path sea valorado
   - ¿300 DG? ¿350 DG? ¿400 DG?
   - Verificar: con el pack Popular ($4.99 = 550 DG), ¿el jugador puede comprar cuántos?
3. ¿Deben los temas premium también tener precio en USD directo además de DG,
   o solo en DG para forzar la conversión de packs?
4. ¿El bundle de 15 temas premium en DG con 30% off es el descuento correcto?
5. ¿Debe existir un "bundle completo 19 temas" en DG?
6. ¿Existe una "primera oferta" (Starter Pack: 1 tema + 200 DG a 60% off) para conversión D1–D3?
7. ¿Los precios DC de frames están calibrados para que no compitan con los DG-themes?

---

## ÁREA 7 — PREMIUM / SUSCRIPCIÓN

### Estado actual:
- No hay suscripción mensual
- Features de pago único: Crear Torneos ($3.99), CashBattle Create ($6.99), Bundle ($8.99)

### Preguntas clave:
1. ¿Debería existir una suscripción mensual "DigitPark Plus" (~$4.99/mes)?
   - Beneficios posibles: Double DC earning, exclusive monthly cosmetic, extra daily missions
   - **No incluir DG en beneficios** (DG es solo IAP directo, no suscripción)
2. ¿Los features de pago único ($3.99/$6.99) están correctamente valorados?
3. ¿El Bundle de $8.99 tiene suficiente percepción de ahorro vs. $3.99+$6.99=$10.98?
4. ¿La ausencia de suscripción es una pérdida de ARPU recurrente?

---

## ÁREA 8 — LOOP DE PROGRESIÓN Y RETENCIÓN

### Ciclo completo del jugador:

**D1 (Día 1):**
- ¿Qué recibe el jugador nuevo? (1,000 DC, 100 DG bienvenida, tutorial)
- ¿El tutorial muestra claramente que DG = moneda premium de compra?
- ¿Hay una "first win reward" en DC para enganche inmediato?
- ¿Se muestra una "welcome offer" de DG en las primeras 24h?

**Semana 1 (F2P sin comprar DG):**
- ¿El jugador tiene un objetivo económico claro en DC para 7 días?
- ¿Puede comprar algún frame/avatar en 7 días F2P?
- ¿La Daily Reward Día 7 (en DC) es impactante suficiente?
- ¿El jugador entiende el camino hacia un tema gannable (achievement épico)?

**Mes 1 (jugador activo diario):**
- ¿Cuánto DC acumula un jugador activo en 30 días?
- ¿Puede comprar el Frame Platinum (1,200 DC) en mes 1?
- ¿Tiene suficiente variedad de objetivos en DC para mantener motivación?
- ¿El jugador está progresando visiblemente hacia algún tema gannable?

**Largo plazo (3–6 meses):**
- ¿Los 52 achievements proveen progresión suficiente?
- ¿Qué pasa cuando el jugador ha comprado todo lo DC-priced?
- ¿Los temas ganables (muy difíciles) son el contenido de largo plazo para el F2P hardcore?

---

## ÁREA 9 — ANÁLISIS ANTI-PAY-TO-WIN Y PERCEPCIÓN

### Evaluar:
1. ¿DG compra-only crea percepción de P2W aunque cosméticos no den ventaja?
   (Un jugador con todos los temas premium puede percibirse como "mejor" aunque no lo sea)
2. ¿Los frames/avatars comprados con DC dan alguna ventaja indirecta (ej: intimidación)?
3. ¿El sistema de ranked 1v1 puede ser influenciado por cualquier item comprado?
4. ¿Existe transparencia suficiente en la UI sobre qué es DC-earned vs. DG-purchased?
5. ¿Los 4 temas ganables por achievement crean un "sello de honor" que distingue jugadores hardcore?
   (Esto puede ser positivo: el earn-path crea status social, no ventaja gameplay)

---

## ÁREA 10 — BENCHMARKS Y MÉTRICAS OBJETIVO

### KPIs target:

| Métrica | Target industria casual | Target DigitPark |
|---------|------------------------|-----------------|
| D1 Retention | 40% | ? |
| D7 Retention | 20% | ? |
| D30 Retention | 10% | ? |
| ARPU (avg all users) | $0.50–$2/mes | ? |
| ARPPU (paying users) | $5–$30/mes | ? |
| Conversion F2P→Paying | 2–5% | ? |
| Sessions/day active | 3–5 | ? |
| Session length | 5–15 min | ? |

### Segmentación de monetización (analizar por separado):
| Segmento | Descripción | Objetivo económico DigitPark |
|----------|-------------|------------------------------|
| F2P puro | Nunca compra DG | Retener con DC loop + earn-path themes |
| Minnow | $1–$5/mes | Starter pack + 1 tema premium |
| Dolphin | $5–$20/mes | 2–4 temas premium/mes |
| Whale | $20+/mes | Bundle completo + múltiples packs |

### Preguntas de calibración:
1. ¿La economía DC soporta retener al F2P puro 3–6 meses?
2. ¿Los packs de DG tienen precio de entrada correcto para Minnows ($0.99)?
3. ¿Hay suficiente incentivo para que un Minnow suba a Dolphin (qué lo convierte)?
4. ¿El bundle completo en DG es atractivo para la whale tier?

---

## ÁREA 11 — PROPUESTA DE ECONOMÍA REBALANCEADA

### Al finalizar el análisis, proponer estas tablas completas:

**Tabla de recompensas DC recomendadas:**

| Fuente | DC recomendado | Frecuencia |
|--------|---------------|-----------|
| Daily login D1 | ? | Diario |
| Daily login D3 (reemplaza 5 DG) | ? | En ciclo |
| Daily login D7 (reemplaza 25 DG) | ? | En ciclo |
| Milestone streak 7d | ? | Semanal |
| Milestone streak 30d | ? | Mensual |
| Win ranked 1v1 | ? | Por partida |
| Loss ranked 1v1 | ? | Por partida |
| First Win of the Day | ? | Diario |
| Misión diaria fácil | ? | Diario |
| Misión diaria difícil | ? | Diario |
| Misión semanal | ? | Semanal |
| Achievement fácil | ? | Único |
| Achievement legendario | ? | Único |
| Perfect score bonus | ? | Por partida |

**Tabla de precios recomendados:**

| Item | Precio actual | Precio recomendado | Razón |
|------|--------------|-------------------|-------|
| Gem Pack entrada | $0.99 / 100 DG | ? | ? |
| Gem Pack popular | $4.99 / 550 DG | ? | ? |
| Tema premium (x15) | $2.50 USD directo | ? DG | ? |
| Tema gannable (x4): earn-path | Gratis (achievement épico) | Gratis — mantener | — |
| Tema gannable (x4): buy-path | $1.50 USD directo | ? DG | ? |
| Bundle 15 temas premium | $26.25 USD | ? DG (30% off) | ? |
| Bundle completo 19 temas | $30.45 USD | ? DG (30% off) | ? |
| Frame Bronze | 200 DC | ? | ? |
| Frame Platinum | 1,200 DC | ? | ? |
| Crear torneos | $3.99 USD | ? | ? |
| CashBattle Create | $6.99 USD | ? | ? |

---

## ÁREA 12 — SISTEMA DE XP Y NIVELES

### Decisiones de diseño confirmadas:

1. **XP es 100% cosmético** — no afecta matchmaking, no da ventaja competitiva.
   Matchmaking usa ELO/MMR separado, completamente independiente del nivel.
2. **CashBattle SÍ da XP, pero al 50% del rate normal** (`cashBattleXPMultiplier = 0.5f`).
   Justificación: es juego real y competitivo, merece recompensa cosmética. No crea
   ventaja porque el XP solo desbloquea cosméticos.
3. **El nivel NO se puede resetear ni manipular** para afectar el matchmaking —
   el ELO es el único criterio de emparejamiento.

### Fuentes de XP actuales (configurables en Inspector):
| Actividad | XP base | XP en CashBattle (×0.5) |
|-----------|---------|------------------------|
| Jugar partida | 25 XP | 12 XP |
| Ganar partida | +50 XP | +25 XP |
| Partida perfecta | +100 XP | +50 XP |
| Top 90% score | ×1.25 | ×1.25 (sobre resultado) |
| Participar torneo | 75 XP | 37 XP |
| Top 3 torneo | +200 XP | +100 XP |
| Ganar torneo | +500 XP | +250 XP |
| Misión semanal "Grinder" | 3,000 XP target | — |

### Recompensas cosméticas por nivel (implementadas):
| Nivel | Recompensa actual | Tipo |
|-------|------------------|------|
| 5 | Avatar: Beginner | Avatar |
| 10 | Title: Novice | Título |
| 15 | 500 DC | DigitCoins |
| 20 | Avatar: Player | Avatar |
| 25 | Title: Player | Título |
| 30 | 1,000 DC | DigitCoins |
| 40 | Avatar: Veteran | Avatar |
| 50 | Title: Veteran | Título |
| 60 | Frame: Bronze | Marco |
| 75 | 2,000 DC | DigitCoins |
| 100 | Title: Centurion | Título |
| 105 | Avatar: Centurion | Avatar |
| 125 | Frame: Silver | Marco |
| 150 | 5,000 DC | DigitCoins |
| 175 | Title: Expert | Título |
| 200 | Avatar: Expert | Avatar |
| 250 | Frame: Gold | Marco |
| 300 | Title: Master | Título |
| 350 | Avatar: Master | Avatar |
| 400 | Frame: Platinum | Marco |
| 450 | Title: Grand Master | Título |
| 475 | Avatar: Legend | Avatar |
| 490 | Frame: Diamond | Marco |
| 500 | Title: Legend | Título (MAX) |

### Recompensas cosméticas adicionales a evaluar (gaps entre niveles):
Actualmente hay desiertos largos sin recompensa (niveles 60–75, 105–125, etc.).
Propuestas para llenar gaps:

| Tipo | Descripción | Nivel sugerido |
|------|------------|---------------|
| **Tema cosmético** | Desbloquear tema "earnable" por nivel extremo | 200, 300, 400, 500 |
| **Título personalizable** | Badges de "Veterano X días", "Imbatible", etc. | cada 25 niveles |
| **Efecto de perfil** | Borde animado, partículas en avatar | niveles 75, 150, 250 |
| **Emotes/Reacciones** | Reacciones post-partida (pulgar arriba, etc.) | cada 10 niveles |
| **Badge de nivel visible** | Color de badge cambia: Bronce→Plata→Oro→Diamante | 1–25 / 26–75 / 76–200 / 200+ |
| **Bonus DC temporal** | "Semana XP doble" al alcanzar múltiplos de 50 | niveles 50, 100, 150... |
| **Avatar frame exclusivo** | Frame animado solo por nivel, no comprable | 100, 200, 300 |
| **Título épico** | "El Imparable", "Leyenda Viviente" visibles en ranking | 250, 350, 450, 500 |

### Preguntas clave:
1. ¿La curva de XP (×1.15 por nivel) es correcta o muy empinada para jugadores casual?
   - Nivel 1→10: ~870 XP total (asequible en días)
   - Nivel 1→50: ~21,000 XP total (¿semanas o meses?)
   - Nivel 1→100: ~180,000 XP total (¿meses o años?)
   ¿Es esa progresión la correcta para retención D7/D30?
2. ¿Los gaps entre recompensas (ej: niveles 60–75, 105–125) crean sensación de abandono?
   ¿Añadir micro-recompensas (50–100 DC) cada 5 niveles en los tramos vacíos?
3. ¿El 50% de XP en CashBattle es el multiplicador correcto?
   - Muy bajo → desmotiva a jugadores de CashBattle
   - Igual al normal → podría percibirse como "farming" con dinero real
   - Recomendación: 50% es el sweet spot. ¿Confirmamos?
4. ¿Debe mostrarse la barra de XP en la pantalla de resultados de CashBattle?
   (Refuerzo positivo: "Ganaste 37 XP" aunque hayas perdido dinero)
5. ¿Debería existir un "XP boost temporal" como recompensa de streak de Daily Rewards
   (ej: 30 días seguidos → XP doble por 7 días)?
6. ¿Los títulos y avatars por nivel deben ser visibles en el perfil del oponente
   antes de la partida? (efecto de "intimidación cosmética" positiva para retención)
7. ¿Qué pasa con los jugadores que ya tienen nivel alto cuando se lanza el juego?
   ¿Migración de datos de PlayerPrefs a Firebase?

### Benchmark de referencia:
- **League of Legends S1**: nivel visible en perfil, recompensas cosméticas por temporada
- **Duolingo**: XP diario con streaks, leaderboards semanales de XP
- **Clash Royale**: King Level — cosmético puro, desbloquea emotes y torres
- **Fortnite**: Account Level — cosmético, sin impacto en partidas

---

## ARCHIVOS CLAVE A LEER PARA EL ANÁLISIS

```
Assets/_Project/Scripts/Runtime/Features/Monetization/Currency/CurrencyManager.cs
Assets/_Project/Scripts/Runtime/Features/Monetization/Shop/ShopManager.cs
Assets/_Project/Scripts/Runtime/Features/Monetization/DailyRewards/DailyRewardsManager.cs
Assets/_Project/Scripts/Runtime/Features/Monetization/DailyMissions/DailyMissionsManager.cs
Assets/_Project/Scripts/Runtime/Features/Monetization/Achievements/AchievementsManager.cs
Assets/_Project/Scripts/Runtime/Features/Monetization/Premium/PremiumManager.cs
Assets/_Project/Scripts/Runtime/Themes/ThemeManager.cs
Assets/_Project/Scripts/Runtime/Features/Games/Results/OnlineResultManager.cs
Assets/_Project/Scripts/Runtime/Payments/Core/PaymentManager.cs
docs/TAREAS_MANUALES.md
```

---

## FORMATO DE RESPUESTA ESPERADO

Para cada área (1–11), responde con esta estructura:

```
### ÁREA X — [NOMBRE]

**ESTADO ACTUAL:**
[Resumen de lo que existe hoy]

**PROBLEMAS IDENTIFICADOS:**
- [Problema 1]: [Impacto concreto]
- [Problema 2]: ...

**RECOMENDACIÓN:**
[Propuesta con números específicos y justificación]

**IMPACTO ESTIMADO EN RETENCIÓN/MONETIZACIÓN:**
- Segmento F2P: [impacto cualitativo]
- Segmento Minnow: [impacto]
- Segmento Whale: [impacto]
- Riesgo P2W: [Bajo/Medio/Alto]
```

Al finalizar las 11 áreas, genera:
1. **Tabla resumen de recomendaciones priorizadas** (P0 urgente → P3 nice-to-have)
2. **Tabla de precios final recomendada** con todos los valores completados
3. **Flujo de economía diaria** de un jugador activo (DC earned vs. DC spent en 30 días)

---

## NOTAS FINALES

- **CashBattle** usa Triumph SDK (sistema separado) — NO forma parte de esta economía
- **No existe Battle Pass** — no evaluar
- **DG = solo compra** — verificado legalmente. No agregar mecánicas gratuitas de DG
- **4 temas ganables** = earn (achievement épico) OR buy (DG) — NO en USD directo
- **Localización en 5 idiomas** — precios en USD, considerar paridad de poder adquisitivo
- **Firebase backend** — cambios en economía requieren actualizar DB rules y Cloud Functions

---

*Prompt actualizado V51 — Área 12 XP/Niveles añadida, CashBattle XP policy confirmada*
*Fecha: 2026-03-10*
