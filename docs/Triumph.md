# TRIUMPH SDK — Pendientes completos
**Última actualización**: 2026-03-20
**Estado**: Bloqueado — esperando recibir el SDK de Triumph
**Definición**: TODO lo pendiente de Triumph, tanto código como manual. Una vez recibido el SDK, ejecutar en el orden indicado.

---

## FASE 0 — TODOs identificados en el resto del proyecto

### T-00a. LocationRestrictionService.cs:83 — verificación de ubicación real
- Actualmente usa mock (simula estado permitido)
- Reemplazar con llamada real al SDK de Triumph para geovalidación
- **Archivo**: `Assets/_Project/Scripts/Runtime/Services/LocationRestrictionService.cs:83`

### T-00b. DigitRushController.cs:1173 — resultado real del oponente (1v1 dinero real)
- `ShowRealMoneyResult()` usa `bool playerWon = result.Completed` como placeholder
- Necesita comparar el score del jugador contra el score del oponente desde Firebase/Triumph
- **Archivo**: `Assets/_Project/Scripts/Runtime/Features/Games/DigitRush/DigitRushController.cs:1173`

---

## FASE 1 — INTEGRACIÓN CORE (código + Unity)

### T-C1. ServiceLocator — conectar servicios reales
**Archivo:** `Assets/_Project/Scripts/Runtime/Services/ServiceLocator.cs:236–258`
**Bug QA:** B0-A / A07-001
**Problema:** `InitializeTriumphServices()` siempre cae a `InitializeMockServices()`. Toda la economía CashBattle opera con dinero simulado.

**Qué hacer en código:**
1. Añadir el SDK de Triumph al proyecto (Package Manager o carpeta `Plugins/`)
2. En `ServiceLocator.InitializeTriumphServices()`: reemplazar la rama mock por la real
3. Conectar las 4 interfaces con las implementaciones del SDK:
   - `IKYCService` → `TriumphKYCService`
   - `IWalletService` → `TriumphWalletService`
   - `IMatchmakingService` → `TriumphMatchmakingService`
   - `ITournamentService` → `TriumphTournamentService`
4. Cambiar `ServiceMode.Mock` → `ServiceMode.Live` en línea 237

**Criterio de done:** CashBattle arranca en staging sin errores y conecta con backend Triumph real.

---

### T-C2. KYC — mover persistencia a server-side
**Archivos:** `MockKYCService.cs:13,40`
**Bug QA:** B1-F / A07-002
**Problema:** `PlayerPrefs.GetInt(PREFS_KYC_STATUS)` es manipulable en dispositivos jailbroken. Un menor puede poner 0→2 y acceder a CashBattle. Riesgo legal severo.

**Qué hacer en código:**
- `KYCService` real: el estado KYC debe venir del servidor Triumph, nunca de `PlayerPrefs`
- Eliminar `MockKYCService.cs` del build de producción — mover a `#if DEVELOPMENT_BUILD`
- En `CashBattleManager.cs:77`, verificar KYC al entrar al hub:

```csharp
private async void Start() {
    var kycStatus = await ServiceLocator.KYC.GetKYCStatus();
    if (kycStatus != KYCStatus.Verified) {
        ShowKYCRequired();
        return;
    }
    ShowHub();
}
```

---

### T-C3. Wallet — mover balance a server-side
**Archivos:** `MockWalletService.cs:15–16,43–48`
**Bug QA:** B1-F / A07-003
**Problema:** El balance de dinero real (USD) se lee/escribe en `PlayerPrefs`. En dispositivo jailbroken se puede modificar el saldo antes de una partida — fraude directo con dinero real.

**Qué hacer en código:**
- `WalletService` real: el balance USD debe consultarse al SDK Triumph, no a `PlayerPrefs`
- Eliminar `MockWalletService.cs` del build de producción — mover a `#if DEVELOPMENT_BUILD`

---

### T-C4. PlayerPrefs CashBattle — eliminar datos sensibles de prefs locales
**Archivos:** `CashMatchmakingManager.cs:94,681`, `CashTournamentLobbyManager.cs:477`
**Bug QA:** B2-F / A07-004, A07-005, A07-010

**Qué hacer en código:**
- **Entry fee**: pasarlo como parámetro de escena (no PlayerPrefs) — usar `SceneDataCarrier` singleton o `CustomData` de NavigationService
- **`IsCashMatch`**: usarlo solo en memoria de sesión, no en PlayerPrefs
- **`AttemptsUsed`**: almacenarlo en Firebase/Triumph SDK, validado server-side

---

### T-C5. LocationRestriction — validación server-side
**Archivo:** `LocationRestrictionService.cs:76`
**Bug QA:** B2-G / A07-006

**Qué hacer en código:**
- La validación de geolocalización debe confirmarse en el backend Triumph antes de crear la partida
- El cliente puede pre-verificar para UX, pero el servidor debe ser la fuente de verdad
- Pasar la verificación de región como parte de la request de creación de partida al SDK

---

### T-C6. Auto-exclusión — conectar a API Triumph
**Archivo:** `SettingsManager.cs:1204`
**Bug QA:** B2-I / A07-008
**Problema:** El botón de auto-exclusión muestra un mensaje de "contacta soporte" en lugar de llamar a la API real.

**Qué hacer en código:**
```csharp
private async void OnSelfExclusionConfirmed(int durationDays) {
    var result = await ServiceLocator.ResponsibleGaming.SelfExclude(durationDays);
    if (result.Success) {
        AuthenticationService.Instance.Logout();
        // Bloquear acceso a CashBattle por durationDays
    }
}
```

---

## FASE 2 — INFRAESTRUCTURA NATIVA (manual — Xcode/Gradle)

### T-M1. Integrar SDK de Triumph (Unity)
- Una vez recibido el SDK: importar package en Unity
- Conectar las 4 interfaces (ver T-C1)
- Probar en staging: KYC flow, wallet balance, matchmaking, tournaments

### T-M2. Certificate Pinning para transacciones de dinero real
- **iOS**: TrustKit via CocoaPods — añadir en `Podfile` del build Xcode
- **Android**: OkHttp Certificate Pinner — añadir en `build.gradle`
- Integrar en las llamadas HTTP del SDK de Triumph
- Configurar los certificados del servidor Triumph en el pin

### T-M3. Jailbreak/Root Detection para CashBattle
- **iOS**: IOSSecuritySuite via Swift Package Manager o CocoaPods
- **Android**: rootbeer via Gradle dependency
- En código (una vez instalado): bloquear acceso a CashBattle si dispositivo comprometido:
```csharp
if (SecuritySuite.IsJailbroken()) {
    ShowError(AutoLocalizer.Get("device_not_supported"));
    NavigateToMainMenu();
    return;
}
```

### T-M4. Responsible Gaming (backend Triumph)
- **Self-exclusion**: conectar T-C6 con API real de Triumph (requiere endpoint de Triumph)
- **Límites de gasto**: configurables desde backend Triumph — exponer en Settings como "Límite mensual"
- **Recordatorios de sesión**: activar notificaciones de tiempo de sesión y links a recursos de juego responsable
- Verificar que el backend de Triumph tiene endpoint `/responsible-gaming/self-exclude`

### T-M5. Anti-Fraud CashBattle
- Validación server-side de resultados antes de acreditar winnings
- Rate limiting en submisión de match results
- Detección de patrones anómalos (wins imposibles, timing sospechoso)

### T-C8. CashTournamentCreateManager — flujo de creación real
**Archivo:** `CashTournamentCreateManager.cs`

**Qué hacer en código:**
- Conectar `OnCreateTournament()` con la API real de `ITournamentService.CreateTournament()`
- **Estado actual:** Mock/simulado localmente — el torneo no se persiste en el servidor

---

### T-C9. WalletManager — estadísticas y transacciones reales
**Archivo:** `WalletManager.cs`

**Qué hacer en código:**
1. `GetStats()` → implementar con datos reales del wallet de Triumph SDK (actualmente retorna datos simulados)
2. `CreditWinnings()` → implementar con API de transacciones del SDK (actualmente solo simula cambios locales)
3. `ProcessRefund()` → implementar con API de reembolso del SDK (actualmente sin transacción real en servidor)

---

## FASE 3 — LIMPIEZA POST-INTEGRACIÓN (código)

Una vez que T-C1 a T-C6 estén completos y testeados:

- Eliminar `MockKYCService.cs` y `MockWalletService.cs` completamente (o mover a carpeta `Editor/Debug/`)
- Eliminar `ServiceMode.Mock` del `ServiceLocator` o dejarlo solo tras `#if DEVELOPMENT_BUILD`
- Verificar que `TriumphServices.cs` no lanza `NotImplementedException` en ningún path de producción

### T-C7. Eliminar PlayerPrefs restantes de CashBattle (torneos y partidas)

`CashGame_EntryFee`, `IsCashMatch` y `DigitPark_BetAmount` ya fueron migrados a campos `static` en V54 (Bloque 4 del Security Audit).

Los siguientes siguen en PlayerPrefs y requieren la arquitectura del SDK para resolverse correctamente:

**`CashTournamentLobbyManager.cs` (L175-184, L477):**
- `CashTournament_Id`, `CashTournament_Name`, `CashTournament_GameType`
- `CashTournament_CurrentPlayers`, `CashTournament_MaxPlayers`, `CashTournament_MaxAttempts`
- `CashTournament_{tournamentId}_AttemptsUsed`

**`MinigameBase.cs` (L368-381):**
- `CashGame_TournamentId`, `CashGame_AttemptNumber`, `CashGame_MaxAttempts`
- `CashTournament_{tournamentId}_BestScore`

**Fix pendiente:** Una vez integrado el SDK de Triumph, reemplazar estos PlayerPrefs con datos que vengan directamente del servidor de Triumph (el SDK entrega el torneo activo y el estado del jugador via su API). No usar campos `static` para estos ya que el SDK es la fuente de verdad.

---

## Resumen de dependencias

```
SDK recibido
    ↓
T-M1 (importar en Unity)
    ↓
T-C1 (ServiceLocator → Live)
    ↓
T-C2 (KYC server-side)    T-C3 (Wallet server-side)
    ↓                           ↓
T-C4 (eliminar PlayerPrefs CashBattle)
    ↓
T-C5 (Location server)    T-C6 (Auto-exclusión API)
    ↓
T-M2 (Certificate Pinning)
T-M3 (Jailbreak Detection)
T-M4 (Responsible Gaming config)
    ↓
Fase 3 (Limpieza)
```

---

## Criterio de done global

- [ ] CashBattle arranca en staging y conecta con backend Triumph real
- [ ] KYC: un usuario no puede pasar editando PlayerPrefs
- [ ] Wallet: balance viene del servidor, no del dispositivo
- [ ] Self-exclusión bloquea efectivamente el acceso a CashBattle
- [ ] Jailbreak detection activo en iOS y Android
- [ ] Certificate pinning configurado para URLs de Triumph
- [ ] 0 referencias a `MockKYCService` / `MockWalletService` fuera de `#if DEVELOPMENT_BUILD`

---
*Fuentes: QA_RESOLUTION_PLAN.md (B0-A, B1-F, B2-F, B2-G, B2-H, B2-I) + PENDIENTES_MANUALES_V2.md (T-M1 a T-M4)*
