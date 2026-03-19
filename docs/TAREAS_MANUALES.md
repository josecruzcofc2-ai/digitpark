# TAREAS MANUALES - Pasos que debe hacer el usuario
**Ultima actualizacion**: 2026-03-17 (ThemeApplier audit + V53 state)

Estas tareas NO se pueden automatizar con codigo. Requieren accion manual tuya.

---

# BLOQUE 1: TAREAS MANUALES GENERALES
*(No dependen del SDK de Triumph — se pueden completar ahora)*

---

## P0 - BLOQUEANTES (hacer antes de publicar)

### 1. Firebase Credentials - Rotar API keys (SEC-C01)
- **Archivos**: `Assets/GoogleService-Info.plist` (movido de StreamingAssets — ya en .gitignore)
- **Que hacer**:
  1. Rotar API keys en Firebase Console (las actuales estuvieron expuestas en el repo)
  2. ~~Agregar a .gitignore~~ YA HECHO — .gitignore excluye `**/GoogleService-Info.plist`
  3. ~~Implementar Firebase Security Rules~~ YA HECHO — ver tarea #7 abajo
- **Riesgo**: Si las API keys antiguas siguen activas, alguien que las vio en el repo puede abusar de ellas

### 2. App Store ID en ReviewService (SEC-B01)
- **Archivo**: `Scripts/Runtime/Services/ReviewService.cs:221`
- **Que hacer**: Reemplazar `idXXXXXXXXXX` con tu App Store ID real
- **Como**: App Store Connect > Tu App > General > Apple ID

### 3. Deploy Firebase Cloud Functions (NUEVO — V49)
- **Carpeta**: `functions/` (implementacion completa lista — Stripe, Apple IAP, Entitlements, Admin)
- **Que hacer**:
  ```bash
  cd functions
  npm install
  firebase login
  firebase use digitpark-7d772
  firebase deploy --only functions
  ```
- **Riesgo CRITICO**: Sin deploy, el sistema de pagos (Stripe + Apple IAP) NO funciona. Las Cloud Functions son el backend de validacion.

### 4. Firebase Secrets Manager - Configurar claves secretas (NUEVO — V49)
- **Que hacer**: Establecer cada secreto antes del deploy de Functions:
  ```bash
  firebase functions:secrets:set STRIPE_SECRET_KEY
  # (pegar tu Stripe Secret Key: sk_live_xxx)

  firebase functions:secrets:set STRIPE_WEBHOOK_SECRET
  # (pegar el webhook signing secret de Stripe Dashboard)

  firebase functions:secrets:set APPLE_SHARED_SECRET
  # (pegar el App-Specific Shared Secret de App Store Connect)

  firebase functions:secrets:set SLACK_WEBHOOK_URL
  # (pegar URL del webhook de Slack para alertas del sistema — opcional)
  ```
- **Donde obtener cada secreto**:
  - `STRIPE_SECRET_KEY`: stripe.com/dashboard > Developers > API Keys > Secret key
  - `STRIPE_WEBHOOK_SECRET`: stripe.com/dashboard > Developers > Webhooks > (tu endpoint) > Signing secret
  - `APPLE_SHARED_SECRET`: App Store Connect > Mis Apps > Tu App > Compras dentro de la app > Informacion del contrato > App-Specific Shared Secret
- **Riesgo CRITICO**: Sin estos secretos, todas las Cloud Functions crashean al arrancar

### 5. PaymentConfig ScriptableObject - Crear y asignar en Inspector (NUEVO — V49)
- **Que hacer**:
  1. En Unity: click derecho en `Assets/_Project/Resources/` > Create > DigitPark > Payment Config
  2. Nombrar el archivo `PaymentConfig`
  3. Rellenar en Inspector:
     - `Payments Health Url`: `https://us-central1-digitpark-7d772.cloudfunctions.net/paymentsHealth`
     - `Stripe Checkout Url`: `https://us-central1-digitpark-7d772.cloudfunctions.net/stripeCreateCheckout`
     - `Stripe Session Status Url`: `https://us-central1-digitpark-7d772.cloudfunctions.net/stripeSessionStatus`
     - `Iap Validate Url`: `https://us-central1-digitpark-7d772.cloudfunctions.net/iapValidateReceipt`
     - `Stripe Publishable Key`: tu clave publica de Stripe (`pk_live_xxx`)
     - `Iap Product Ids`: los 6 Product IDs de App Store (ver tarea #6)
  4. Seleccionar `PaymentManager` en la escena Boot > arrastrar el ScriptableObject al campo `Config`
- **Riesgo CRITICO**: Sin esto, `PaymentManager` loguea error y no inicializa Stripe

### 6. Configurar IAP en las tiendas
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

### 7. Stripe Dashboard - Crear productos y configurar webhook (NUEVO — V49)
- **Que hacer**:
  1. En stripe.com/dashboard > Productos: crear los 6 productos de gems con sus precios
  2. En stripe.com/dashboard > Developers > Webhooks > Add endpoint:
     - URL: `https://us-central1-digitpark-7d772.cloudfunctions.net/stripeWebhook`
     - Eventos a escuchar: `checkout.session.completed`, `payment_intent.payment_failed`
  3. Copiar el Signing Secret generado → usar en tarea #4 como `STRIPE_WEBHOOK_SECRET`
- **Riesgo**: Sin el webhook, los pagos Stripe exitosos no otorgan entitlements al usuario

### 8. DEVELOPMENT_BUILD en Release Builds
- **Que hacer**: Verificar que en Unity Build Settings > Player Settings NO este habilitado `Development Build` para builds de produccion
- **Riesgo**: DevTools (DebugManager, PremiumDebugController) se incluyen si esta activo

---

## P1 - IMPORTANTES (hacer antes o poco despues de publicar)

### ~~9. Firebase Security Rules — Subir a Console (SEC-A14)~~ ✅ COMPLETADO 2026-03-17
- `firebase deploy --only database` ejecutado — reglas publicadas en `digitpark-7d772-default-rtdb`
- SEC-M01/M02/M03 + friendRequests activos en producción

### 10. Activar Firebase Cloud Messaging (FIREBASE_MESSAGING)
- **Que hacer**:
  1. `Player Settings > Other Settings > Scripting Define Symbols` → agregar `FIREBASE_MESSAGING`
  2. Configurar APNs en Firebase Console > Cloud Messaging > Apple app configuration
  3. En Xcode: Signing & Capabilities > + Capability > Push Notifications
- **Estado actual**: `NotificationService.cs` esta 100% listo pero inactivo por falta del define
- **Impacto**: Sin esto las push notifications NO funcionan en produccion

### 11. Instalar Firebase Remote Config
- **Que hacer**: `Window > Package Manager > Add by name`: `com.google.firebase.remote-config`
- **Para que**: Feature flags del sistema de pagos (Stripe on/off, maintenance mode, force update)
- **Defaults requeridos en Remote Config Console**:
  - `payment_provider = "stripe"`, `stripe_enabled = true`, `maintenance_mode = false`, `min_app_version = "1.0.0"`

### 12. Instalar Firebase Crashlytics
- **Que hacer**: `Window > Package Manager > Add by name`: `com.google.firebase.crashlytics`
- **Post-instalacion**: Llamar `Crashlytics.SetUserId(userId)` en `AuthenticationService` OnLoginSuccess
- **Para que**: Monitoreo de crashes en produccion — imprescindible antes de lanzar

### 13. Sign In with Apple — Verificar entitlement en Xcode
- **Que hacer**: Abrir proyecto Xcode generado por Unity > Signing & Capabilities > verificar que `Sign In with Apple` aparece como capability habilitada
- **Si no aparece**: + Capability > Sign In with Apple
- **Riesgo**: App Store rechaza apps con login social sin Sign In with Apple en iOS

### 14. Privacy Policy Consent en First Launch (SEC-B03)
- **Que hacer**: Mostrar pantalla de consentimiento ANTES de iniciar analytics (Android)
- **Archivo**: Crear nuevo flujo entre Boot y Login
- **Requisito**: GDPR requiere consentimiento antes de recopilar datos

### 15. SecurePlayerPrefs - Cifrar datos sensibles (SEC-A02, SEC-C15)
- **Que hacer**: Integrar libreria de PlayerPrefs cifrados (ej: EasySave, SecurePlayerPrefs)
- **Datos afectados**: UserIDs, FCM tokens, historial, daily rewards, currency, achievement state
- **Riesgo**: PlayerPrefs son texto plano accesible en dispositivos rooteados

### 16. Server-Side Score Validation / Cloud Functions (SEC-C07, SEC-C16)
- **Que hacer**: Implementar Firebase Cloud Functions para validar scores antes de escribir al leaderboard
- **Incluir**: Validar que scores no sean imposibles (ej: completar juego de 10 min en 0.5 seg)
- **Archivos**: `DatabaseService.cs:373-458`, `MinigameBase.cs:368-373`
- **Riesgo**: Leaderboards corrompidos, premios injustos en torneos de dinero real

### 17. Multi-Accounting Detection (SEC-C18)
- **Que hacer**: Implementar device fingerprinting + rate limiting en servidor
- **Archivo**: `MatchmakingService.cs:74` (fallback a `SystemInfo.deviceUniqueIdentifier` es spoofeable)
- **Riesgo**: Cuentas infinitas para farmear rewards y referral abuse

### 18. Rate Limiting + Anti-Replay en Servidor (SEC-M01, SEC-M07, SEC-M11)
- **Que hacer**: Implementar en Firebase Cloud Functions:
  - Rate limiting en score submissions
  - IDs unicos + timestamps server-side para anti-replay
  - Idempotency keys en match result submissions
- **Archivos**: `DatabaseService.cs`, `OnlineResultManager.cs`

### 19. Admin API Key para adminForceSwitch (NUEVO — V49)
- **Archivo**: `functions/src/index.ts` — endpoint `adminForceSwitch`
- **Que hacer**: Crear documento en Firestore → coleccion `payment_config` → documento `admin`:
  ```json
  { "adminKey": "<clave-aleatoria-segura-de-256-bits>" }
  ```
  Generar la clave: `openssl rand -hex 32` o cualquier generador de tokens seguro
- **Para que**: El endpoint `adminForceSwitch` valida esta clave para autenticar comandos de emergencia
- **Nota**: La clave actual en el codigo es un placeholder — debe reemplazarse antes de produccion

### 20. Decidir politica FR tu/vous
- **Opciones**:
  - A) Todo informal "tu" (consistente con tono de juego)
  - B) "vous" solo para acciones graves (borrar cuenta, legal)
- **Impacto**: ~15+ keys FR a cambiar segun la decision
- **Accion**: Dime tu decision y lo implemento

### 21. ~~Username Uniqueness Validation (SEC-A16)~~ COMPLETADO
- **Fix aplicado**: `DatabaseService.IsUsernameTaken()` + check en `RegisterWithEmail()` antes de crear cuenta

---

## P2 - MEJORAS (post-lanzamiento)

### 22. GDPR Right-to-Delete completo (SEC-A07)
- **Archivo**: `AuthenticationService.cs:438-505`
- **Falta borrar**: Analytics, Match History, Notifications, Achievements, Tournament records, Friends list references

### 23. GDPR Data Export (Right to Portability)
- **Que hacer**: Crear mecanismo para que usuarios exporten sus datos (Article 20)

### 24. Loot Box / RandomBox Odds Disclosure
- **Archivo**: `PlayerProgressionSystem.cs:403-406`
- **Que hacer**: Apple/Google requieren mostrar probabilidades de recompensas aleatorias

### 25. Legal URLs configurables (SEC-B05)
- **Archivo**: `SettingsManager.cs:88-91`, `AgeVerificationManager.cs:35-36`
- **Actual**: Hardcoded `https://docs.triumpharcade.com/terms-of-use` y `https://digitpark.com/terms`
- **Que hacer**: Verificar que los dominios son tuyos o hacerlas configurables via Remote Config

### 26. OnApplicationPause Session Management (SEC-M05)
- **Que hacer**: Invalidar sesion o re-autenticar despues de volver de background
- **Riesgo**: Sesion activa indefinidamente sin re-verificacion

### 27. Auth Operation Timeouts (SEC-M03)
- **Que hacer**: Agregar CancellationTokenSource con timeout a operaciones async de auth
- **Archivo**: `LoginManager.cs`

### 28. PlayerPrefs Key Namespacing (SEC-B04)
- **Que hacer**: Agregar prefijo `DP_` a todas las keys de PlayerPrefs
- **Riesgo**: Bajo — conflicto con otras apps si comparten espacio

### 29. Sistema de Frames — tareas manuales post-V50

#### 29a. Tag "FrameLayer" en TagManager (OBLIGATORIO antes de build)
- **Donde**: Unity Editor > Edit > Project Settings > Tags & Layers
- **Que hacer**: Agregar tag `FrameLayer` en la lista de tags
- **Por que**: `FrameRenderer.Awake()` hace `gameObject.tag = "FrameLayer"` para que `CashThemeForcer` excluya el frame del recolor dorado. Si el tag no existe, Unity lanza MissingReferenceException en runtime.
- **Alternativa**: El `ApplyToAllImages()` ya tiene el fallback `if (img.gameObject.name == "AvatarFrame") continue` — el tag es solo para mayor robustez.

#### 29b. Registrar 9 nuevos frames IAP en ProductCatalog.cs
- **Que hacer**: Agregar los 9 product IDs en `ProductCatalog.cs` con formato `com.matrixsoftware.digitpark.frame_[id]`:
  - `com.matrixsoftware.digitpark.frame_plasma_spark` ($0.99)
  - `com.matrixsoftware.digitpark.frame_prism_shift` ($0.99)
  - `com.matrixsoftware.digitpark.frame_aurora_borealis` ($3.99)
  - `com.matrixsoftware.digitpark.frame_void_walker` ($5.99)
  - `com.matrixsoftware.digitpark.frame_storm_surge` ($5.99)
  - `com.matrixsoftware.digitpark.frame_cosmic_rift` ($9.99)
  - `com.matrixsoftware.digitpark.frame_infernal_god` ($9.99)
  - `com.matrixsoftware.digitpark.frame_divine_light` ($14.99)
  - `com.matrixsoftware.digitpark.frame_quantum_break` ($14.99)
- **Por que**: Sin esto, el PaymentManager no puede procesar la compra de estos frames.

#### 29c. Regenerar UIBuilders en Unity Editor
- **Que hacer**: Después de compilar sin errores, ejecutar desde el menú:
  - DigitPark > Friends > Build Friends UI
  - DigitPark > Profile > Build Profile UI
  - DigitPark > Profile > Build Scores UI
  - DigitPark > Matchmaking > Build Matchmaking UI
  - DigitPark > Friends > Build Friend Requests UI
  - DigitPark > Friends > Build Search Players UI
  - DigitPark > Core > Build Main Menu UI
  - DigitPark > Monetization > Build Shop Premium UI
- **Por que**: Los builders ahora agregan `FrameRenderer` — las escenas existentes no tienen el componente hasta regenerarlas.

### 30. Unity IAP Localized Prices (C-66, B-59, B-68)
- **Archivos**: `PremiumManager.cs`, `WelcomePackService.cs`, `ShopPremiumUIBuilder.cs`, `ShopEffectsTabBuilder.cs`
- **Que hacer**: Reemplazar precios hardcodeados en USD ("$3.99", "$7.99", etc.) con `product.metadata.localizedPriceString` de Unity IAP
- **Por que**: Usuarios fuera de USA ven precios en dolares cuando la tienda cobra en su moneda local — confuso y potencialmente ilegal en EU
- **Requisito**: Las tareas #5 y #6 (PaymentConfig + IAP en tiendas) deben estar completadas primero
- **Impacto**: 4+ archivos, ~20 strings de precio a reemplazar

### 31. Google Sign-In SDK nativo (D-34)
- **Archivo**: `AuthenticationService.cs:341`
- **Que hacer**: Reemplazar `FederatedOAuthProvider` con el SDK nativo de Google Sign-In
  - Android: Integrar `com.google.android.gms:play-services-auth`
  - iOS: Integrar Google Sign-In SDK via CocoaPods
- **Estado actual**: Usa Firebase Auth federated provider que abre WebView — puede fallar en dispositivos reales
- **Riesgo**: Login con Google puede no funcionar en produccion en algunos dispositivos

### 32. Server-Side Time Validation (D-68, D-69, D-77)
- **Archivos**: `DailyRewardsManager.cs`, `DailyMissionsManager.cs`, `DailyOfferService.cs`
- **Que hacer**: Implementar Firebase Cloud Function que retorne `serverTimestamp` y usarlo para:
  - Validar claims de daily rewards (en vez de `DateTime.UtcNow` del dispositivo)
  - Validar resets de daily missions
  - Validar streak shield cooldown (14 dias)
- **Por que**: Con `DateTime.UtcNow` local, el jugador puede adelantar el reloj para reclamar recompensas multiples veces
- **Complejidad**: Media — requiere nuevo endpoint en Cloud Functions + modificar 3 managers
- **Mitigacion actual**: Ya se cambio a `DateTime.UtcNow` (consistente), pero sigue siendo manipulable

### 33. Client-Side Grant Validation (C-65)
- **Archivos**: `DailyOfferService.cs`, `WelcomePackService.cs`, `RotatingContentService.cs`
- **Que hacer**: Los grants de items (frames, temas, efectos) ocurren client-side sin validacion server.
  Un jugador puede modificar PlayerPrefs para "comprar" items sin pagar.
  Para items IAP: usar Cloud Function `iapValidateReceipt` (ya existe) ANTES de otorgar.
  Para items DG: aceptable client-side (DG es moneda virtual no-comprable con dinero real... wait, DG SI se compra con IAP)
- **Solucion**: Mover toda la logica de grant detras de una Cloud Function que valide recibo + deduzca balance server-side
- **Prioridad**: Alta para items de dinero real, media para items de moneda virtual

### 34. DailyOfferService Seed Predecible (B-70) — DECISION TUYA
- **Archivo**: `DailyOfferService.cs:190`
- **Estado**: El seed es `Year * 10000 + Month * 100 + Day` — cualquier jugador puede calcular ofertas futuras
- **Opciones**:
  - A) Dejarlo asi (las ofertas son iguales para todos = justo, aunque predecible)
  - B) Anadir userId al seed (ofertas personalizadas, no predecibles)
  - C) Generar ofertas server-side via Cloud Function
- **Riesgo**: Bajo — solo afecta planificacion de gasto de DG, no seguridad

### 35. RotatingContentService Catalogo Vacio (B-75)
- **Archivo**: `RotatingContentService.cs:107-151`
- **Estado**: Todas las entradas del catalogo estan comentadas — intencional para post-launch
- **Que hacer**: Cuando estes listo para activar contenido rotativo:
  1. Descomentar las entradas del catalogo
  2. Asegurarse de que los items referenciados (frames, themes, battle cards) existen
  3. Verificar que `BattleCardService.UnlockCard()` exista para grants de tipo `SeasonalBattleCard`

### 36. ~~Regenerar 3 Iconos Pendientes~~ COMPLETADO (V52)
- Los 3 iconos (stat_earnings, DepositIcon, WithdrawIcon) ya estan integrados con .meta files

### 37. TODOs de features incompletas
- `ShopItemData.cs:250` - Compra avatar no hace nada
- `MainMenuManager.cs:420,433` - Perfil/busqueda sin abrir
- `LocationRestrictionService.cs:83` - Verificacion ubicacion real
- `DeepLinkService.cs:198` - Pasar userId a ProfileManager
- `LeaderboardEntryUI.cs:65` - Cargar avatar image async
- `DigitRushController.cs:1172` - Obtener resultado real de oponente desde servidor

---

## P1 - THEMEAPPLIER (implementacion sistema de temas)

> Estado al 2026-03-17: 0% implementado. Todas las escenas tienen 0 ThemeApplier components.
> Los bugs de clasificacion en ThemeApplierSetup.cs ya fueron corregidos en codigo.
> Ver plan detallado en GUIA_IMPLEMENTACION_PASO_A_PASO.md > SECCION C > PLAN 6 PASOS.

### TA-01. Ejecutar "Add to ALL Scenes" en Unity Editor
- **Menu**: DigitPark > Polish > Themes > Add to ALL Scenes
- **Resultado**: ~700+ ThemeApplier components en 29 escenas
- **Cuando**: DESPUES de ejecutar todos los UIBuilders (para que no se borre)
- **Escenas excluidas**: CashBattle (#08, #32-40), AgeVerification (paletas propias)

### TA-02. BackButton.prefab — Child Arrow/Icon necesita ThemeApplier manual
- **Archivo**: `Assets/_Project/Resources/Prefabs/BackButton.prefab`
- **Por que**: El root GO (tiene `Button`) ya recibe ThemeApplier `Accent` via `ProcessButtons()`.
  Pero el child `Arrow` o `Icon` (solo tiene `Image`, sin `Button`) → `ClassifyImage("Arrow")` retorna `None` → no se le asigna nada.
- **Que hacer en Inspector**:
  1. Abrir el prefab en modo prefab edit
  2. Child `Arrow`/`Icon`: Add Component > ThemeApplier → ElementType = `Accent`, applyToImage = true


# BLOQUE 2: TAREAS QUE REQUIEREN EL SDK DE TRIUMPH
*(Solo se pueden completar una vez que Triumph responda el correo y envie el SDK)*

---

## P0 - BLOQUEANTES TRIUMPH

### T1. Integrar SDK de Triumph (CashBattle — core)
- **Archivos**: `ServiceLocator.cs:237`, `TriumphServices.cs` (todos stubs — NotImplementedException reemplazados por graceful failures)
- **Que hacer**: Con el SDK recibido, implementar las interfaces reales:
  - `IKYCService` — KYC/verificacion de identidad real
  - `IWalletService` — Wallet con transacciones reales
  - `IMatchmakingService` — Matchmaking en servidor
  - `ITournamentService` — Torneos en servidor
- **Estado actual**: `ServiceMode.Mock` — todo el CashBattle funciona con datos simulados
- **Nota**: El codigo de integracion en `TriumphManager.cs` y `TriumphServices.cs` ya tiene la estructura lista. Solo falta conectar las implementaciones reales.

### T2. ~~Server-Side Receipt Validation (SEC-C08)~~ COMPLETADO via Cloud Functions
- **Fix aplicado**: `iapValidateReceipt` Cloud Function valida con Apple servers + otorga entitlement
- **Pendiente manual**: Solo configurar `APPLE_SHARED_SECRET` (ver tarea #4 del Bloque 1)

---

## P1 - IMPORTANTES TRIUMPH

### T3. CashTournamentCreateManager - Flujo de creacion real
- **Archivo**: `CashTournamentCreateManager.cs`
- **Que hacer**: Conectar `OnCreateTournament()` con API real de `ITournamentService.CreateTournament()`
- **Estado actual**: Mock/simulado localmente

### T4. WalletManager.GetStats - Estadisticas reales de wallet
- **Archivo**: `WalletManager.cs`
- **Que hacer**: Implementar `GetStats()` con datos reales del wallet de Triumph SDK
- **Estado actual**: Retorna datos simulados

### T5. WalletManager.CreditWinnings / ProcessRefund - Transacciones reales
- **Archivo**: `WalletManager.cs`
- **Que hacer**: Implementar `CreditWinnings()` y `ProcessRefund()` con API de transacciones del SDK
- **Estado actual**: Solo simula cambios locales sin transaccion real en el servidor

### T6. Certificate Pinning para transacciones de dinero real (SEC-A04)
- **Que hacer**: Integrar SDK de certificate pinning (ej: TrustKit iOS / OkHttp Certificate Pinner Android) en las llamadas HTTP del SDK de Triumph
- **Cuando**: Al activar el SDK de Triumph en produccion
- **Riesgo**: Sin pinning, ataques MITM pueden interceptar transacciones de dinero real

### T7. Jailbreak/Root Detection para CashBattle (SEC-A03)
- **Que hacer**: Integrar rootbeer (Android) / IOSSecuritySuite (iOS)
- **Cuando**: Antes de activar transacciones reales de dinero
- **Riesgo**: Dispositivos comprometidos pueden modificar PlayerPrefs, memoria, inyectar codigo para manipular resultados

---

## P2 - MEJORAS TRIUMPH (post-integracion)

### T8. Responsible Gaming Features (CashBattle)
- Self-exclusion mecanismo real (conectar `SettingsManager.cs:1127` con API de Triumph)
- Limites de gasto configurables desde el backend
- Recordatorios de tiempo de sesion
- Links a recursos de juego responsable

### T9. Anti-Fraud CashBattle
- Validacion server-side de resultados antes de acreditar winnings
- Rate limiting en submision de match results
- Deteccion de patrones anomalos (wins imposibles, timing sospechoso)

---

## Referencia: Generacion de Iconos (DALL-E)

Iconos minimalistas TOTALMENTE BLANCOS o dorados. Excepciones de color:
- Rojo/fuego para items de poder
- Verde para items de dinero/social
- Iconos de escena Game son excepciones (detallados) menos el icono VS (regenerar minimalista)
- Achievements y App Icon son excepciones (detallados)

Cada prompt DALL-E debe especificar 2 veces: "transparent background, do NOT generate fake transparency checkerboard pattern"
