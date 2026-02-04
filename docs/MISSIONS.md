# Sistema de Misiones - DigitPark

## Resumen

El sistema de misiones proporciona objetivos temporales y repetibles que recompensan a los jugadores con **XP**, **Monedas** y **Gemas**.

| Tipo | Cantidad Activa | Reinicio | Selección |
|------|-----------------|----------|-----------|
| **Diarias** | 4 misiones | Cada 24h (medianoche UTC) | Aleatorias de 13 templates |
| **Semanales** | 5 misiones | Cada lunes (medianoche UTC) | Aleatorias de 11 templates |
| **Temporada** | 15 misiones | Cada 60 días | Todas activas |

---

## Misiones Diarias (13 Templates)

Se seleccionan **4 misiones aleatorias** cada día.

| ID | Nombre | Descripción | XP | Monedas | Gemas |
|----|--------|-------------|-----|---------|-------|
| `daily_play_1` | Jugador Casual | Juega 1 partida | 25 | 50 | 0 |
| `daily_play_3` | Jugador Activo | Juega 3 partidas | 50 | 100 | 0 |
| `daily_play_5` | Maratón Diario | Juega 5 partidas | 75 | 150 | 0 |
| `daily_win_1` | Victoria Diaria | Gana 1 partida | 50 | 100 | 0 |
| `daily_win_3` | Triple Victoria | Gana 3 partidas | 100 | 200 | 0 |
| `daily_digitrush` | Especialista DigitRush | Juega 2 partidas de DigitRush | 50 | 100 | 0 |
| `daily_flashtap` | Reflejos del Día | Juega 2 partidas de FlashTap | 50 | 100 | 0 |
| `daily_memorypairs` | Memoria Diaria | Juega 2 partidas de MemoryPairs | 50 | 100 | 0 |
| `daily_quickmath` | Matemáticas del Día | Juega 2 partidas de QuickMath | 50 | 100 | 0 |
| `daily_oddoneout` | Observador Diario | Juega 2 partidas de OddOneOut | 50 | 100 | 0 |
| `daily_score_high` | Puntaje Alto | Alcanza 5,000 puntos en cualquier juego | 75 | 150 | 0 |
| `daily_precision` | Precisión | Completa una partida con 80%+ precisión | 75 | 150 | 0 |

### Recompensas Totales Diarias (si completas las 4)
- **Mínimo**: 100 XP + 200 Monedas
- **Máximo**: 300 XP + 600 Monedas
- **Promedio**: ~200 XP + 400 Monedas

---

## Misiones Semanales (11 Templates)

Se seleccionan **5 misiones aleatorias** cada semana.

| ID | Nombre | Descripción | XP | Monedas | Gemas |
|----|--------|-------------|-----|---------|-------|
| `weekly_play_15` | Jugador Dedicado | Juega 15 partidas esta semana | 300 | 750 | 10 |
| `weekly_play_30` | Maratón Semanal | Juega 30 partidas esta semana | 500 | 1,500 | 25 |
| `weekly_win_7` | Ganador Semanal | Gana 7 partidas esta semana | 400 | 1,000 | 15 |
| `weekly_win_15` | Dominador | Gana 15 partidas esta semana | 600 | 2,000 | 35 |
| `weekly_all_games` | Versatilidad | Juega todos los minijuegos | 400 | 1,000 | 20 |
| `weekly_streak` | Racha Semanal | Consigue una racha de 3 victorias | 350 | 800 | 15 |
| `weekly_cash_1` | Apostador Semanal | Completa 1 Cash Battle | 300 | 500 | 20 |
| `weekly_cash_3` | High Stakes | Completa 3 Cash Battles | 500 | 1,000 | 40 |
| `weekly_tournament` | Competidor | Participa en un torneo | 400 | 1,000 | 25 |
| `weekly_xp` | Grinder | Gana 3,000 XP esta semana | 500 | 1,500 | 30 |
| `weekly_score` | Alto Rendimiento | Alcanza 50,000 puntos totales | 400 | 1,000 | 20 |

### Recompensas Totales Semanales (si completas las 5)
- **Mínimo**: 1,500 XP + 3,500 Monedas + 60 Gemas
- **Máximo**: 2,500 XP + 6,500 Monedas + 145 Gemas
- **Promedio**: ~2,000 XP + 5,000 Monedas + 100 Gemas

---

## Misiones de Temporada (15 Templates)

**Todas las 15 misiones** están activas durante la temporada (60 días).

| ID | Nombre | Descripción | XP | Monedas | Gemas |
|----|--------|-------------|-----|---------|-------|
| `season_bp_25` | Medio Camino | Alcanza nivel 25 del Pase de Batalla | 2,000 | 5,000 | 100 |
| `season_bp_50` | Pase Completo | Completa el Pase de Batalla (nivel 50) | 5,000 | 10,000 | 250 |
| `season_wins_50` | Veterano de Temporada | Gana 50 partidas | 1,500 | 3,000 | 75 |
| `season_wins_100` | Centurión | Gana 100 partidas | 3,000 | 7,500 | 150 |
| `season_wins_250` | Leyenda de Temporada | Gana 250 partidas | 5,000 | 15,000 | 300 |
| `season_cash_10` | Apostador Serio | Gana 10 Cash Battles | 2,000 | 5,000 | 100 |
| `season_cash_25` | Cash Pro | Gana 25 Cash Battles | 4,000 | 10,000 | 200 |
| `season_tournament_3` | Competidor Elite | Participa en 3 torneos | 1,500 | 3,000 | 75 |
| `season_tournament_win` | Campeón de Temporada | Gana un torneo | 3,000 | 7,500 | 150 |
| `season_friends` | Social Butterfly | Añade 5 amigos | 1,000 | 2,500 | 50 |
| `season_login_30` | Dedicación | Inicia sesión 30 días | 2,000 | 5,000 | 100 |
| `season_login_50` | Compromiso Total | Inicia sesión 50 días | 3,000 | 7,500 | 150 |
| `season_mastery` | Polivalente | Alcanza 10,000 puntos en cada minijuego | 4,000 | 10,000 | 200 |
| `season_rank_oro` | Rango Oro | Alcanza rango Oro en Cash Battle | 2,000 | 5,000 | 100 |
| `season_rank_diamante` | Rango Diamante | Alcanza rango Diamante en Cash Battle | 4,000 | 10,000 | 200 |

### Recompensas Totales de Temporada (si completas las 15)
- **Total**: 43,000 XP + 106,000 Monedas + 2,200 Gemas

---

## Tipos de Misiones (MissionType)

```csharp
public enum MissionType
{
    PlayGames,          // Jugar X partidas
    WinGames,           // Ganar X partidas
    PlaySpecificGame,   // Jugar X partidas de un juego específico
    WinStreak,          // Conseguir racha de X victorias
    ReachScore,         // Alcanzar X puntos en una partida
    TotalScore,         // Acumular X puntos totales
    PrecisionGame,      // Completar partida con X% precisión
    PlayCashBattle,     // Jugar X Cash Battles
    WinCashBattle,      // Ganar X Cash Battles
    PlayTournament,     // Participar en X torneos
    WinTournament,      // Ganar X torneos
    EarnXP,             // Ganar X XP
    AddFriends,         // Añadir X amigos
    LoginDays,          // Iniciar sesión X días
    PlayAllGames,       // Jugar todos los tipos de juego
    MasteryAllGames,    // Alcanzar maestría en todos los juegos
    ReachRank,          // Alcanzar rango X en Cash Battle
}
```

---

## Mecánicas de Reinicio

### Misiones Diarias
- Se reinician a las **00:00 UTC** cada día
- Se seleccionan **4 misiones aleatorias** del pool de 13
- El progreso se pierde al reiniciar
- Las misiones completadas pero no reclamadas también se pierden

### Misiones Semanales
- Se reinician cada **lunes a las 00:00 UTC**
- Se seleccionan **5 misiones aleatorias** del pool de 11
- El progreso se pierde al reiniciar

### Misiones de Temporada
- Duran **60 días** (una temporada completa)
- **Todas las 15 misiones** están activas simultáneamente
- El progreso persiste durante toda la temporada
- Al iniciar nueva temporada, se reinicia todo

---

## Integración con Otros Sistemas

### Sistema de Progresión (PlayerProgressionSystem)
- Las misiones otorgan XP que suma al nivel permanente del jugador
- Solo aplica para modos gratuitos (no Cash Battles)

### Sistema de Cash Battle (CashBattleRankSystem)
- Las misiones de rango (`season_rank_oro`, `season_rank_diamante`) verifican el MMR del jugador
- Los Cash Battles reportan victorias/derrotas al MissionsManager

### Sistema Social
- La misión `season_friends` cuenta amigos añadidos
- Se actualiza cuando se confirma una solicitud de amistad

---

## API del MissionsManager

### Reportar Progreso
```csharp
// Reportar partida jugada
MissionsManager.Instance.ReportGamePlayed(gameId, isWin, score, precision, isCashBattle);

// Reportar torneo
MissionsManager.Instance.ReportTournament(placement, totalParticipants);

// Reportar racha
MissionsManager.Instance.ReportWinStreak(streakCount);

// Reportar XP ganado
MissionsManager.Instance.ReportXPEarned(amount);

// Reportar rango alcanzado
MissionsManager.Instance.ReportRank(rankIndex);

// Reportar login diario
MissionsManager.Instance.ReportLogin();

// Reportar amigo añadido
MissionsManager.Instance.ReportFriendAdded();
```

### Obtener Misiones
```csharp
// Obtener misiones por tipo
List<Mission> daily = MissionsManager.Instance.GetDailyMissions();
List<Mission> weekly = MissionsManager.Instance.GetWeeklyMissions();
List<Mission> season = MissionsManager.Instance.GetSeasonMissions();

// Obtener todas las misiones activas
List<Mission> all = MissionsManager.Instance.GetAllActiveMissions();

// Contadores
int completed = MissionsManager.Instance.GetCompletedMissionsCount(MissionPeriod.Daily);
int unclaimed = MissionsManager.Instance.GetUnclaimedRewardsCount();

// Tiempo hasta reinicio
TimeSpan dailyReset = MissionsManager.Instance.GetTimeUntilDailyReset();
TimeSpan weeklyReset = MissionsManager.Instance.GetTimeUntilWeeklyReset();
```

### Reclamar Recompensas
```csharp
// Reclamar una misión específica
bool success = MissionsManager.Instance.ClaimReward(mission);

// Reclamar todas las completadas
MissionReward total = MissionsManager.Instance.ClaimAllRewards();
```

---

## Eventos

```csharp
// Suscribirse a eventos
MissionsManager.Instance.OnMissionProgress += (mission) => { /* actualizar UI */ };
MissionsManager.Instance.OnMissionCompleted += (mission) => { /* mostrar notificación */ };
MissionsManager.Instance.OnRewardClaimed += (reward) => { /* animar recompensa */ };
MissionsManager.Instance.OnDailyMissionsReset += () => { /* refrescar lista */ };
MissionsManager.Instance.OnWeeklyMissionsReset += () => { /* refrescar lista */ };
```

---

## Notas de Diseño

### Balance de Recompensas
- Las misiones diarias son fáciles y dan recompensas modestas para incentivar juego diario
- Las misiones semanales requieren más dedicación pero incluyen gemas
- Las misiones de temporada son objetivos a largo plazo con recompensas significativas

### Engagement
- El sistema de misiones aleatorias mantiene la experiencia fresca cada día
- Los temporizadores de reinicio crean urgencia para completar misiones
- El "Claim All" facilita reclamar múltiples recompensas

### Monetización
- Las gemas solo aparecen en misiones semanales y de temporada
- Esto incentiva juego sostenido sin regalar gemas gratis fácilmente
- Las misiones de Cash Battle promocionan el modo de pago
