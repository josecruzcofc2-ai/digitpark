# 📊 IMPLEMENTACIÓN DEL SISTEMA DE SCORES - DIGIT PARK

## ✅ LO QUE SE HA IMPLEMENTADO

### 1. **UI de Scores (ScoresUIBuilder.cs)**

#### ✅ Diseño según boceto
- **Tabla con líneas divisorias sutiles** (verticales y horizontales)
- **Modo Personal**: Solo muestra Nombre y Tiempo (2 columnas)
- **Modo Local/Global**: Muestra TOP#, Nombre y Tiempo (3 columnas)

#### ✅ Elementos visuales
- Fondo oscuro semi-transparente para el ScrollView
- Divisores verticales sutiles entre columnas (color gris sutil)
- Divisores horizontales sutiles entre entradas
- Colores especiales para TOP 3 (oro, plata, bronce)
- Resaltado del jugador actual con fondo azul eléctrico

#### ✅ Tabs de navegación
- **MEJORES PERSONALES**: Muestra historial personal del jugador
- **CLASIFICACIÓN LOCAL**: Top jugadores del mismo país
- **CLASIFICACIÓN GLOBAL**: Top jugadores mundiales

---

### 2. **Sistema de Guardado Automático (GameManager.cs)**

#### ✅ Guardado automático al completar partida
Cuando el jugador completa el juego (toca del 1 al 9 correctamente):

1. **Historial Personal**:
   - Se guarda el tiempo en `currentPlayer.scoreHistory`
   - Se mantienen las últimas 50 partidas
   - Se actualiza el promedio automáticamente

2. **Mejor Tiempo Personal**:
   - Se actualiza `currentPlayer.bestTime` si es récord
   - Se guarda en `PlayerData` en Firebase

3. **Leaderboards (Global y Local)**:
   - Se llama a `DatabaseService.SaveScore()`
   - **SOLO actualiza si es el mejor tiempo del jugador**
   - Guarda en:
     - `leaderboards/global/{userId}`
     - `leaderboards/country_{countryCode}/{userId}`

---

### 3. **Lógica de Leaderboards (DatabaseService.cs)**

#### ✅ Sistema inteligente de guardado
```csharp
// Verifica si ya existe un score
// Si no existe → guarda directamente
// Si existe → solo actualiza si el nuevo tiempo es MEJOR (menor)
```

**Ventaja**: Ahorra escrituras en Firebase y mantiene solo el mejor tiempo de cada jugador.

#### ✅ Métodos implementados
- `SaveScore()` - Guarda/actualiza mejor tiempo en leaderboards
- `GetGlobalLeaderboard()` - Obtiene top global (200 jugadores)
- `GetCountryLeaderboard()` - Obtiene top por país (100 jugadores)
- `SavePlayerData()` - Guarda datos completos del jugador
- `LoadPlayerData()` - Carga datos del jugador

---

### 4. **Estructura de Datos (PlayerData.cs)**

#### ✅ Campos implementados
```csharp
public class PlayerData
{
    // ... otros campos ...

    public float bestTime;                      // Mejor tiempo del jugador
    public float averageTime;                   // Promedio de tiempos
    public List<ScoreEntry> scoreHistory;       // Últimas 50 partidas
    public int totalGamesPlayed;                // Total de partidas
    public int totalGamesWon;                   // Partidas ganadas
}

public class ScoreEntry
{
    public float time;                          // Tiempo de la partida
    public DateTime timestamp;                  // Cuándo se jugó
    public string tournamentId;                 // null si es partida casual
}
```

#### ✅ Métodos útiles
- `AddScore(float time)` - Añade partida al historial (máx 50)
- `UpdateAverageTime()` - Recalcula el promedio automáticamente

---

## 🔧 ESTRUCTURA DE FIREBASE

### Realtime Database
```
digitpark-7d772-default-rtdb/
│
├── players/
│   └── {userId}/
│       ├── username
│       ├── email
│       ├── bestTime
│       ├── averageTime
│       ├── scoreHistory: [...]
│       ├── totalGamesPlayed
│       └── ... (otros datos)
│
└── leaderboards/
    ├── global/
    │   └── {userId}/
    │       ├── userId
    │       ├── username
    │       ├── time          ← MEJOR tiempo del jugador
    │       ├── countryCode
    │       └── timestamp
    │
    └── country_{countryCode}/
        └── {userId}/
            ├── userId
            ├── username
            ├── time          ← MEJOR tiempo del jugador
            ├── countryCode
            └── timestamp
```

### Por qué esta estructura:

1. **Eficiencia**:
   - Solo 1 entrada por usuario en cada leaderboard
   - Queries rápidas con `orderByChild("time").limitToFirst(200)`

2. **Escalabilidad**:
   - No importa cuántas partidas juegue un usuario
   - El leaderboard solo crece 1 entrada por usuario nuevo

3. **Costos**:
   - Mínimas escrituras (solo cuando se mejora récord)
   - Lecturas optimizadas (top N con límite)

---

## 🚀 PRÓXIMOS PASOS

### **Paso 1: Descomentar código Firebase**

En estos 3 archivos, busca los bloques comentados con `/* ... */` y descomentar:

#### `DatabaseService.cs`
```csharp
// Líneas 173-219: Descomentar todo el bloque en SaveScore()
// Líneas 208-232: Descomentar GetGlobalLeaderboard()
// Líneas 254-278: Descomentar GetCountryLeaderboard()
```

#### `AuthenticationService.cs`
```csharp
// Ya debería estar descomentado si Firebase está configurado
```

---

### **Paso 2: Configurar Firebase Realtime Database**

1. **Ve a Firebase Console**: https://console.firebase.google.com/
2. **Selecciona tu proyecto**: `digitpark-53ad5` o `digitpark-7d772`
3. **Realtime Database** → **Crear base de datos**
4. **Ubicación**: Selecciona la más cercana (ej: us-central1)
5. **Modo**: Iniciar en **modo de prueba** (luego cambiar reglas)

---

### **Paso 3: Configurar Reglas de Seguridad**

#### **Reglas para desarrollo (modo prueba)**
```json
{
  "rules": {
    ".read": "auth != null",
    ".write": "auth != null"
  }
}
```

#### **Reglas para producción** (recomendadas)
```json
{
  "rules": {
    "players": {
      "$uid": {
        ".read": "auth != null && auth.uid == $uid",
        ".write": "auth != null && auth.uid == $uid"
      }
    },
    "leaderboards": {
      "global": {
        ".read": "auth != null",
        "$uid": {
          ".write": "auth != null && auth.uid == $uid"
        }
      },
      "$countryLeaderboard": {
        ".read": "auth != null",
        "$uid": {
          ".write": "auth != null && auth.uid == $uid"
        }
      }
    }
  }
}
```

**Explicación**:
- ✅ Usuarios solo pueden editar sus propios datos
- ✅ Leaderboards son de lectura pública (para todos los autenticados)
- ✅ Usuarios solo pueden escribir su propia entrada en leaderboards
- ❌ Nadie puede borrar datos de otros jugadores

---

### **Paso 4: Verificar la URL de la Database**

En `DatabaseService.cs` línea 25:
```csharp
private const string DATABASE_URL = "https://digitpark-7d772-default-rtdb.firebaseio.com";
```

**¿Cómo obtener tu URL?**
1. Firebase Console → Realtime Database
2. Copia la URL que aparece arriba (ej: `https://tu-proyecto-default-rtdb.firebaseio.com`)
3. Reemplaza en el código

---

## 🧪 TESTING

### **Test sin Firebase (Modo Simulado)**

El código actual funciona en modo simulado:
- ✅ Guarda scores en memoria (PlayerData)
- ✅ Muestra en UI de Scores (Personales)
- ❌ NO guarda en Firebase (solo logs)
- ❌ NO muestra Local/Global (listas vacías)

### **Test con Firebase (Modo Real)**

Después de descomentar código:

#### **1. Test de Guardado**
1. Juega una partida completa (toca 1-9)
2. Revisa la consola:
   ```
   [Game] Score guardado en historial personal y leaderboards: 5.234s
   [Database] Verificando score: TuUsuario - 5.234s
   [Database] Primer score del usuario, guardando...
   [Database] Score actualizado en leaderboards (global y local)
   ```
3. Ve a Firebase Console → Realtime Database
4. Verifica que aparezca:
   - `players/{tuUserId}/scoreHistory/0/time = 5.234`
   - `leaderboards/global/{tuUserId}/time = 5.234`
   - `leaderboards/country_US/{tuUserId}/time = 5.234`

#### **2. Test de Mejora de Récord**
1. Juega otra partida MÁS RÁPIDA (ej: 4.500s)
2. Revisa la consola:
   ```
   [Game] ¡NUEVO RÉCORD! 4.500s
   [Database] Nuevo récord! 4.500s < 5.234s, actualizando...
   ```
3. Firebase debe mostrar time actualizado a 4.500s

#### **3. Test de NO Mejora**
1. Juega otra partida MÁS LENTA (ej: 6.000s)
2. Revisa la consola:
   ```
   [Database] Tiempo no mejoró (6.000s >= 4.500s), no se actualiza leaderboard
   ```
3. Firebase NO debe cambiar (sigue en 4.500s)
4. PERO scoreHistory SÍ se actualiza (tiene las 3 partidas)

#### **4. Test de Leaderboards**
1. Abre escena `Scores`
2. Tabs:
   - **Personales**: Debes ver tus 3 partidas (6.000s, 5.234s, 4.500s)
   - **Local**: Debe cargar top del país (si hay otros jugadores)
   - **Global**: Debe cargar top mundial

---

## 📝 NOTAS IMPORTANTES

### **Diferencia entre Historial y Leaderboards**

#### **Historial Personal** (`scoreHistory`)
- ✅ Guarda TODAS las partidas (últimas 50)
- ✅ Se muestra en tab "MEJORES PERSONALES"
- ✅ Ordenadas de mejor a peor
- ✅ Solo el jugador ve su historial completo

#### **Leaderboards** (Global/Local)
- ✅ Guarda SOLO el MEJOR tiempo de cada jugador
- ✅ Se muestra en tabs "CLASIFICACIÓN LOCAL" y "GLOBAL"
- ✅ Todos los jugadores autenticados pueden verlos
- ✅ Ordenados por mejor tiempo (más rápido = posición más alta)

---

### **Formato de Tiempo**

Todos los tiempos se muestran con 3 decimales:
```csharp
$"{time:F3}s"  // Ejemplo: "5.234s"
```

---

### **Colores de Medallas (TOP 3)**

```csharp
1er lugar: Oro     (R:1.0, G:0.84, B:0.0)
2do lugar: Plata   (R:0.75, G:0.75, B:0.75)
3er lugar: Bronce  (R:0.8, G:0.5, B:0.2)
```

---

## 🎨 PERSONALIZACIÓN

### **Cambiar colores de divisores**

En `LeaderboardManager.cs`:
```csharp
// Línea 441 (divisor vertical)
divImage.color = new Color(0.3f, 0.3f, 0.4f, 0.5f);

// Línea 460 (divisor horizontal)
divImage.color = new Color(0.3f, 0.3f, 0.4f, 0.3f);
```

### **Cambiar tamaño de entradas**

En `LeaderboardManager.cs` línea 331:
```csharp
entryRT.sizeDelta = new Vector2(1040, 80); // Ancho y altura
```

### **Cambiar límite de top**

En `LeaderboardManager.cs`:
```csharp
// Línea 263 (local)
localScores = await DatabaseService.Instance.GetCountryLeaderboard(currentPlayer.countryCode, 100);

// Línea 282 (global)
globalScores = await DatabaseService.Instance.GetGlobalLeaderboard(200);
```

---

## ❓ FAQ

### **¿Por qué no se muestran Local/Global?**
R: Necesitas descomentar el código Firebase en `DatabaseService.cs` y tener Firebase configurado.

### **¿Dónde se guardan los scores personales?**
R: En `PlayerData.scoreHistory` (máximo 50 entradas).

### **¿Cuántas veces se escribe en Firebase por partida?**
R:
- 2 escrituras si NO es récord (PlayerData + verificación)
- 4 escrituras si ES récord (PlayerData + global + local + verificación)

### **¿Puedo cambiar el máximo de scoreHistory?**
R: Sí, en `PlayerData.cs` línea 143:
```csharp
if (scoreHistory.Count > 50)  // Cambia 50 por el número que quieras
```

### **¿Se pueden eliminar entradas antiguas del leaderboard?**
R: Firebase no lo hace automáticamente. Podrías crear una Cloud Function para limpiar jugadores inactivos.

---

## ✅ CHECKLIST FINAL

Antes de publicar:

- [ ] Código Firebase descomentado
- [ ] Realtime Database creada en Firebase Console
- [ ] URL de Database actualizada en código
- [ ] Reglas de seguridad configuradas (producción)
- [ ] Probado guardado de scores
- [ ] Probado leaderboards (personal, local, global)
- [ ] Verificado que solo se actualiza el mejor tiempo
- [ ] Colores y diseño según boceto
- [ ] Divisores sutiles funcionando

---

## 🎉 ¡FELICIDADES!

Has implementado:
- ✅ Sistema completo de scores con UI según boceto
- ✅ Guardado automático al terminar partidas
- ✅ Leaderboards inteligentes (solo mejor tiempo)
- ✅ Tabs de navegación (Personal, Local, Global)
- ✅ Diseño con divisores sutiles
- ✅ Visualización diferente para Personal vs Local/Global
- ✅ Resaltado del jugador actual
- ✅ Colores para TOP 3

**¡El sistema está listo para funcionar con Firebase! 🚀**

---

**Documentación creada por Claude Code**
**Proyecto: digitPark**
**Fecha: 2025-11-06**
