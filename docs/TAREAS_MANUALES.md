# TAREAS MANUALES - Pasos que debe hacer el usuario
**Ultima actualizacion**: 2026-03-09

Estas tareas NO se pueden automatizar con codigo. Requieren accion manual tuya.

---

## P0 - BLOQUEANTES (hacer antes de publicar)

### 1. Firebase Credentials - Rotar y proteger (SEC-C01)
- **Archivos**: `Assets/StreamingAssets/google-services.json`, `GoogleService-Info.plist`
- **Que hacer**:
  1. Rotar API keys en Firebase Console (las actuales estan expuestas en el repo)
  2. Agregar ambos archivos a `.gitignore`
  3. Implementar Firebase Security Rules estrictas (`database.rules.json`)
- **Riesgo**: Sin esto, cualquiera con acceso al repo puede leer/escribir toda tu base de datos

### 2. App Store ID en ReviewService (SEC-B01)
- **Archivo**: `Scripts/Runtime/Services/ReviewService.cs:221`
- **Que hacer**: Reemplazar `idXXXXXXXXXX` con tu App Store ID real
- **Como**: App Store Connect > Tu App > General > Apple ID

### 3. Triumph SDK Integration (CashBattle)
- **Archivos**: `ServiceLocator.cs:237`, `TriumphServices.cs` (todos stubs)
- **Que hacer**: Integrar SDK real de Triumph para:
  - KYC/verificacion de identidad real
  - Wallet con transacciones reales
  - Matchmaking servidor
  - Torneos servidor
- **Nota**: Mientras tanto, `ServiceMode.Mock` es correcto para desarrollo

### 4. Configurar IAP en las tiendas
```
App Store Connect + Google Play Console - Crear 6 In-App Purchases (Consumable):

  com.matrixsoftware.digitpark.gems_100    $0.99
  com.matrixsoftware.digitpark.gems_500    $4.99
  com.matrixsoftware.digitpark.gems_1200   $9.99
  com.matrixsoftware.digitpark.gems_2500   $19.99
  com.matrixsoftware.digitpark.gems_6500   $49.99
  com.matrixsoftware.digitpark.gems_14000  $99.99

Receipt Validation:
  Unity: Window > Unity IAP > Receipt Validation Obfuscator
  Pegar llaves de App Store y Google Play > Click Obfuscate
```

### 5. Server-Side Receipt Validation (SEC-C08)
- **Archivo**: `PremiumManager.cs:702-726`
- **Que hacer**: Implementar backend que valide receipts con Apple/Google servers
- **Riesgo**: Sin esto, las compras se pueden falsificar

### 6. DEVELOPMENT_BUILD en Release Builds
- **Que hacer**: Verificar que en Unity Build Settings > Player Settings NO este habilitado `Development Build` para builds de produccion
- **Riesgo**: DevTools (DebugManager, PremiumDebugController) se incluyen si esta activo

---

## P1 - IMPORTANTES (hacer antes o poco despues de publicar)

### 7. Firebase Security Rules (SEC-A14)
- **Que hacer**: Crear/auditar `database.rules.json` y subirlas a Firebase Console
- **Incluir**: Validar `request.auth.uid` en todas las lecturas/escrituras
- **Riesgo**: Sin reglas, cualquier usuario puede leer/escribir datos de otros

### 8. Privacy Policy Consent en First Launch (SEC-B03)
- **Que hacer**: Mostrar pantalla de consentimiento ANTES de iniciar analytics (Android)
- **Archivo**: Crear nuevo flujo entre Boot y Login
- **Requisito**: GDPR requiere consentimiento antes de recopilar datos

### 9. Certificate Pinning (SEC-A04)
- **Que hacer**: Integrar SDK de certificate pinning (ej: TrustKit) en llamadas HTTP
- **Cuando**: Al integrar Triumph SDK para transacciones reales

### 10. Jailbreak/Root Detection SDK (SEC-A03)
- **Que hacer**: Integrar rootbeer (Android) / IOSSecuritySuite (iOS)
- **Riesgo**: Dispositivos comprometidos pueden modificar PlayerPrefs, memoria, inyectar codigo

### 11. SecurePlayerPrefs - Cifrar datos sensibles (SEC-A02, SEC-C15)
- **Que hacer**: Integrar libreria de PlayerPrefs cifrados (ej: EasySave, SecurePlayerPrefs)
- **Datos afectados**: UserIDs, FCM tokens, historial, daily rewards, currency, achievement state
- **Riesgo**: PlayerPrefs son texto plano accesible en dispositivos rooteados

### 12. Server-Side Score Validation (SEC-C07, SEC-C16)
- **Que hacer**: Implementar Firebase Cloud Functions para validar scores
- **Incluir**: Validar que scores no sean imposibles (ej: completar juego de 10 min en 0.5 seg)
- **Archivos**: `DatabaseService.cs:373-458`, `MinigameBase.cs:368-373`
- **Riesgo**: Leaderboards corrompidos, premios injustos en torneos

### 13. Multi-Accounting Detection (SEC-C18)
- **Que hacer**: Implementar device fingerprinting + rate limiting en servidor
- **Archivo**: `MatchmakingService.cs:74` (fallback a `SystemInfo.deviceUniqueIdentifier` es spoofeable)
- **Riesgo**: Cuentas infinitas para farmear rewards y referral abuse

### 14. ~~Username Uniqueness Validation (SEC-A16)~~ COMPLETADO
- **Fix aplicado**: `DatabaseService.IsUsernameTaken()` + check en `RegisterWithEmail()` antes de crear cuenta
- **Archivo**: `AuthenticationService.cs`, `DatabaseService.cs`

### 15. Rate Limiting + Anti-Replay en Servidor (SEC-M01, SEC-M07, SEC-M11)
- **Que hacer**: Implementar en Firebase Cloud Functions:
  - Rate limiting en score submissions
  - IDs unicos + timestamps server-side para anti-replay
  - Idempotency keys en match result submissions
- **Archivos**: `DatabaseService.cs`, `OnlineResultManager.cs`

### 16. ~~Server-Side Auth Tokens (SEC-A06)~~ NO APLICA
- **Verificado**: En modo Firebase (produccion), `SavedUserId` nunca se usa para auto-login. Firebase Auth maneja sesiones nativamente via `firebaseAuth.CurrentUser`. La ruta de simulacion es dev-only.

### 17. Decidir politica FR tu/vous
- **Opciones**:
  - A) Todo informal "tu" (consistente con tono de juego)
  - B) "vous" solo para acciones graves (borrar cuenta, legal)
- **Impacto**: ~15+ keys FR a cambiar segun la decision
- **Accion**: Dime tu decision y lo implemento

### 18. ~~DailyRewardService - Implementar rewards reales~~ COMPLETADO
- **Fix aplicado**: `ApplyReward()` ahora llama `CurrencyManager.Instance.AddCoins()` y `.AddGems()` para DigitCoins y DigitGems

## P1 - TRIUMPH SDK (pendiente integracion)

### 19. CashTournamentCreateManager - Flujo de creacion real
- **Archivo**: `CashTournamentCreateManager.cs`
- **Que hacer**: Conectar flujo de creacion de torneos con API real de Triumph SDK
- **Estado actual**: Mock/simulado localmente

### 20. WalletManager.GetStats - Estadisticas reales de wallet
- **Archivo**: `WalletManager.cs`
- **Que hacer**: Implementar `GetStats` con datos reales del wallet de Triumph
- **Estado actual**: Retorna datos simulados

### 21. WalletManager.CreditWinnings / ProcessRefund - Transacciones reales
- **Archivo**: `WalletManager.cs`
- **Que hacer**: Implementar `CreditWinnings` y `ProcessRefund` con API de transacciones de Triumph
- **Estado actual**: Solo simula cambios locales sin transaccion real

---

## P2 - MEJORAS (post-lanzamiento)

### 22. GDPR Right-to-Delete completo (SEC-A07)
- **Archivo**: `AuthenticationService.cs:438-505`
- **Falta borrar**: Analytics, Match History, Notifications, Achievements, Tournament records, Friends list references

### 23. GDPR Data Export (Right to Portability)
- **Que hacer**: Crear mecanismo para que usuarios exporten sus datos (Article 20)

### 24. Responsible Gaming Features (CashBattle)
- Self-exclusion mecanismo real
- Limites de gasto
- Recordatorios de tiempo de sesion
- Links a recursos de juego responsable

### 25. Loot Box / RandomBox Odds Disclosure
- **Archivo**: `PlayerProgressionSystem.cs:403-406`
- **Que hacer**: Apple/Google requieren mostrar probabilidades de recompensas aleatorias

### 26. Legal URLs configurables (SEC-B05)
- **Archivo**: `SettingsManager.cs:88-91`, `AgeVerificationManager.cs:35-36`
- **Actual**: Hardcoded `https://docs.triumpharcade.com/terms-of-use` y `https://digitpark.com/terms`
- **Que hacer**: Verificar que los dominios son tuyos o hacerlas configurables

### 27. OnApplicationPause Session Management (SEC-M05)
- **Que hacer**: Invalidar sesion o re-autenticar despues de volver de background
- **Riesgo**: Sesion activa indefinidamente sin re-verificacion

### 28. Service Initialization Ordering (SEC-M16)
- **Que hacer**: Implementar boot sequence explicito que espere a todos los servicios antes de cargar gameplay
- **Archivo**: `ServiceLocator.cs:141`

### 29. Auth Operation Timeouts (SEC-M03)
- **Que hacer**: Agregar CancellationTokenSource con timeout a operaciones async de auth
- **Archivo**: `LoginManager.cs`

### 30. PlayerPrefs Key Namespacing (SEC-B04)
- **Que hacer**: Agregar prefijo `DP_` a todas las keys de PlayerPrefs (ej: `DP_RememberMe`, `DP_FCM_TOKEN`)
- **Riesgo**: Bajo - conflicto con otras apps si comparten PlayerPrefs

### 31. JSON Schema Validation (SEC-M02)
- **Que hacer**: Validar esquema despues de `JsonUtility.FromJson` en DatabaseService y AuthenticationService
- **Riesgo**: Bajo con JsonUtility (no ejecuta codigo arbitrario)

### 32. TODOs de features incompletas
- `ShopItemData.cs:250` - Compra avatar no hace nada
- `MainMenuManager.cs:420,433` - Perfil/busqueda sin abrir
- `SettingsManager.cs:1127` - Auto-exclusion Triumph SDK
- `LocationRestrictionService.cs:83` - Verificacion ubicacion real
- `DeepLinkService.cs:198` - Pass userId a ProfileManager
- `LeaderboardEntryUI.cs:65` - Load avatar image async
- `DigitRushController.cs:1172` - Get real opponent result from server

---

## Referencia: Generacion de Iconos (DALL-E)

Iconos minimalistas TOTALMENTE BLANCOS o dorados. Excepciones de color:
- Rojo/fuego para items de poder
- Verde para items de dinero/social
- Iconos de escena Game son excepciones (detallados) menos el icono VS (regenerar minimalista)
- Achievements y App Icon son excepciones (detallados)

Cada prompt DALL-E debe especificar 2 veces: "transparent background, do NOT generate fake transparency checkerboard pattern"
