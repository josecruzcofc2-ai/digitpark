# MEGA AUDIT PROMPT V49 — DigitPark Full App Review
> Generado: 2026-03-10 | Revisado: 2026-03-10 | Cobertura: 390 archivos .cs (Runtime + Editor) + `database.rules.json` + `functions/` + `Tests/` | **100% del codebase**
> Objetivo: auditoría meticulosa de pies a cabeza, cero exclusiones (salvo Triumph SDK internals)
> Secciones: 0–33 | Ejecutar en 3 sesiones (A/B/C) — ver ESTRATEGIA DE EJECUCIÓN

---

## CONTEXTO DEL PROYECTO

**App**: DigitPark — juego móvil Unity C# (iOS + Android)
**Scripts root**: `Assets/_Project/Scripts/`
**Patrón principal**: Singleton services, UIBuilders programáticos, AutoLocalizer para i18n
**Firebase**: Auth + Realtime Database + Analytics + Notifications (FCM)
**Triumph SDK**: Área excluida del audit Firebase (CashBattle real-money internals) — PERO sus archivos .cs sí se auditan para crashes, warnings y seguridad
**DOTween**: Librería de animaciones principal
**TMP**: TextMeshPro para todo texto UI
**5 idiomas**: EN (base), ES, FR, PT, DE — via `AutoLocalizer.Get("key")`

---

## INSTRUCCIONES GENERALES

Eres un auditor experto en Unity C#, Firebase, seguridad móvil y localización. Tu misión es revisar **absolutamente todos los archivos .cs** del proyecto (runtime + editor), organizados por sección. Para cada archivo:

1. **Léelo completo** — no hagas asunciones sin leer el código
2. **Reporta issues por severidad**: 🔴 P0 (crash/blocker) · 🟠 P1 (bug funcional) · 🟡 P2 (warning/degradación) · 🔵 P3 (mejora/limpieza)
3. **Propón el fix exacto** (código corregido, no descripción vaga)
4. **No omitas ningún archivo** — si lo lees y está limpio, márcalo como ✅ OK
5. Al finalizar cada sección, produce un **resumen de issues encontrados**

---

## ESTRATEGIA DE EJECUCIÓN (context window management)

Este audit cubre ~390 archivos .cs + `database.rules.json` + `functions/`. Ejecutar en **3 sesiones separadas** para no exceder el contexto:

- **Sesión A — Infraestructura** (Secciones 0–11): Firebase Rules, Cloud Functions, Boot, Navigation, Network, Popups, Audio, Animations Core, Animations Components, Animations Animators, Effects, Firebase Services, Core Services
- **Sesión B — Features** (Secciones 12–22): Triumph, Localization, Auth, Games Core, Games Minigames, Games Navigation & Results, Monetization, Social, Tournaments, Onboarding, Settings & MainMenu
- **Sesión C — CashBattle + Payments + Themes + UI + Editor** (Secciones 23–33): CashBattle, Payments, Themes, UI Components, UI Builders Runtime, Data Models, DevTools, Editor UIBuilders, Editor AutoAssigners, Editor Tools, Tests

Al iniciar cada sesión, indica qué sesión es (A/B/C) y acumula los issues en el reporte final unificado.

---

## CHECKLIST GLOBAL (aplica a TODOS los archivos)

### A. FIREBASE INTEGRATION
- [ ] ¿Cada manager/service que necesita persistir datos usa `DatabaseService`?
- [ ] ¿Todos los reads/writes de Firebase tienen manejo de errores (try-catch o `.ContinueWith`)?
- [ ] ¿Se llama `DatabaseService.UpdatePlayerFields()` en vez de sobrescribir el nodo raíz?
- [ ] ¿Las suscripciones Firebase (`.ValueChanged`) se desuscriben en `OnDestroy`?
- [ ] ¿`AuthenticationService` valida que el usuario esté autenticado antes de cualquier operación DB?
- [ ] ¿`AnalyticsService.LogCustomEvent()` se llama en todos los eventos importantes (login, compra, partida, logro)?
- [ ] ¿`NotificationService` procesa callbacks FCM en el hilo principal via `UnityMainThreadDispatcher`?
- [ ] ¿No hay llamadas directas a `FirebaseDatabase.DefaultInstance` fuera de `DatabaseService`?
- [ ] ¿No hay llamadas directas a `FirebaseAuth.DefaultInstance` fuera de `AuthenticationService`?

### B. FLUJO DE APP (No crashes de lógica)
- [ ] ¿Todos los `GetComponent<>` críticos tienen null-check antes de usarse?
- [ ] ¿Los `FindObjectOfType<>` y `GameObject.Find()` tienen null-check?
- [ ] ¿Las coroutines usan `yield return null` correctamente (no loops infinitos sin exit)?
- [ ] ¿DOTween tweens son terminados/killed en `OnDestroy`/`OnDisable` (sin memory leaks)?
- [ ] ¿Los `async void` tienen try-catch para evitar unhandled exceptions?
- [ ] ¿Los singletons verifican `Instance != null` antes de crear uno nuevo?
- [ ] ¿`DontDestroyOnLoad` sólo se aplica a singletons de escena global (no managers de escena)?
- [ ] ¿Cambios de escena cancelan coroutines/tweens pendientes?
- [ ] ¿No hay referencias a objetos destruidos (uso de `this == null` checks post-await)?
- [ ] ¿El flujo Boot → Login/Register → MainMenu → Game → Result es correcto sin saltos?

### C. WARNINGS DE COMPILACIÓN
- [ ] ¿No hay variables declaradas pero no usadas?
- [ ] ¿No hay métodos privados no usados?
- [ ] ¿No hay `using` statements innecesarios?
- [ ] ¿No hay obsolete API usage (Unity/Firebase deprecated)?
- [ ] ¿No hay conversiones implícitas peligrosas (int→float, etc.)?
- [ ] ¿No hay `#pragma warning disable` sin justificación?
- [ ] ¿Todos los `override` tienen `base.Method()` donde corresponde?

### D. ANIMACIONES (DOTween / Coroutines)
- [ ] ¿Todos los `DOTween.Sequence()` tienen `.SetAutoKill(true)` o se guardan para kill manual?
- [ ] ¿Los tweens de UI (`DOFade`, `DOScale`, `DOMove`) verifican que el target no sea null?
- [ ] ¿Animaciones de entrada/salida de paneles no se solapan (secuencia correcta)?
- [ ] ¿`UIAnimationManager` es accesible en cada escena (no falta en algunas)?
- [ ] ¿Las animaciones de resultado (win/lose) completan antes de habilitar botones?
- [ ] ¿`SceneTransitionManager` limpia tweens antes de cargar nueva escena?
- [ ] ¿`BootAnimator` tiene fallback si los elementos UI no existen?
- [ ] ¿Los animadores de partícula (`ParticleSystemManager`, `CelebrationManager`) no crashean si el Shader.Find() retorna null?

### E. AUTOLOCALIZER — SEMÁNTICA 100%
- [ ] ¿Cada texto visible en UI tiene su `AutoLocalizer.Get("key")` correcto?
- [ ] ¿Cada key usada en código existe en `Translations.txt`?
- [ ] ¿No hay strings hardcodeados en español o inglés en archivos Runtime (excepto Debug.Log)?
- [ ] ¿Las keys semánticamente hacen sentido con su contenido? (ej: `"login_button"` → "Log In", no "Play")
- [ ] ¿Los GameObjects en escena tienen nombres que coincidan con el `TextNameToKeyMap` en `AutoLocalizer.cs`?
- [ ] ¿No hay keys duplicadas en `Translations.txt`?
- [ ] ¿`AutoLocalizer.Get(key, args)` con placeholders `{0}`, `{1}` reciben los argumentos correctos?
- [ ] ¿Los 5 idiomas (EN/ES/FR/PT/DE) están presentes para cada key?
- [ ] ¿Las traducciones FR/DE no se cortan por overflow (textos críticos tienen autoSizing habilitado)?
- [ ] ¿`LocalizedTextComponent` se refresca cuando el idioma cambia en runtime?

### F. SEGURIDAD (Cybersecurity)
- [ ] ¿No hay API keys, secrets, tokens hardcodeados en código fuente?
- [ ] ¿No hay URLs de backend hardcodeadas con credenciales?
- [ ] ¿Los datos de usuario en Firebase tienen reglas de acceso (no world-readable)?
- [ ] ¿Receipts de IAP son validados server-side (no sólo client-side)?
- [ ] ¿No hay `Debug.Log` que exponga datos sensibles (emails, tokens, UIDs) en builds release?
- [ ] ¿`AppleReceiptValidator` usa fail-closed (rechaza si no puede validar)?
- [ ] ¿`PaymentManager` bloquea usuarios anónimos de realizar compras?
- [ ] ¿No hay SQL/NoSQL injection en queries Firebase (keys construidas con input de usuario)?
- [ ] ¿`ChatFilterService` aplica filtro antes de guardar en DB?
- [ ] ¿No hay información PII del usuario en Analytics events?
- [ ] ¿Las contraseñas nunca se loguean ni se guardan en PlayerPrefs/local?
- [ ] ¿Los DeepLinks validan el esquema y parámetros antes de procesarlos?
- [ ] ¿`StripeComplianceGuard` y `TriumphIsolationGuard` están activos en los paths correctos?
- [ ] ¿No hay campos de input sin sanitización enviados a Firebase?

### G. PLATFORM GUARDS (iOS / Android)
- [ ] ¿Todo código iOS-only (ATTService, StoreKit, Apple IAP) está bajo `#if UNITY_IOS`?
- [ ] ¿Todo código Android-only está bajo `#if UNITY_ANDROID`?
- [ ] ¿Los `DllImport` de plugins nativos (`ATTBridge`, `StoreKitBridge`) tienen guard de plataforma?
- [ ] ¿`ATTService` no corre en Android (crash garantizado sin guard)?
- [ ] ¿`AppleIAPProvider` solo se inicializa en iOS?
- [ ] ¿`StripePaymentProvider` (Stripe web checkout) funciona en ambas plataformas o solo una?
- [ ] ¿No hay rutas de archivo que usen backslash (`\`) hardcodeado (falla en iOS/Android)?

### H. FIREBASE RULES & CLOUD FUNCTIONS
- [ ] ¿`database.rules.json` no tiene `".read": true` o `".write": true` en la raíz?
- [ ] ¿Cada nodo de usuario (`/users/$uid/`) solo es leíble/escribible por su propio `$uid`?
- [ ] ¿Los paths de matchmaking/torneo tienen reglas de escritura restringidas (no public write)?
- [ ] ¿El leaderboard es read-only para todos (no permite que un usuario escriba el score de otro)?
- [ ] ¿Las Cloud Functions en `functions/` validan receipts de Apple IAP server-side?
- [ ] ¿Las Cloud Functions tienen manejo de errores y retornan HTTP codes correctos?
- [ ] ¿Las Cloud Functions no loguean datos financieros o PII en Firebase Logs?
- [ ] ¿Las Cloud Functions tienen autenticación obligatoria (verifican `context.auth` antes de operar)?
- [ ] ¿Existen reglas `.validate` para tipos de datos en Firebase (no acepta strings donde debe ir número)?

### I. PERFORMANCE & MEMORY (crítico para juego móvil)
- [ ] ¿No hay concatenación de strings con `+` en `Update()` o hot paths (usar `StringBuilder`)?
- [ ] ¿No hay `foreach` sobre arrays en `Update()` (genera GC en versiones antiguas de Unity)?
- [ ] ¿`Camera.main` está cacheado en `Awake()` (no acceder en Update)?
- [ ] ¿`FloatingText`, `ParticleSystemManager` usan object pooling (no Instantiate/Destroy por frame)?
- [ ] ¿Las texturas de avatares tienen límite de caché (no crecimiento ilimitado en memoria)?
- [ ] ¿Los `List<T>` usados frecuentemente son pre-allocated con capacidad inicial?
- [ ] ¿No hay `FindObjectOfType<>` en `Update()` o paths frecuentes?
- [ ] ¿Los listeners Firebase se desuscriben correctamente (no accumulation por recarga de escenas)?
- [ ] ¿`NeonGlowEffect` y `ButtonEffects` no generan garbage en cada frame de animación?

### J. GDPR & PRIVACY COMPLIANCE (FR/DE usuarios europeos)
- [ ] ¿Existe pantalla de consentimiento de analytics/tracking antes de `AnalyticsService.Initialize()`?
- [ ] ¿`DatabaseService` tiene método para borrar todos los datos de un usuario (GDPR Art. 17 — right to erasure)?
- [ ] ¿Los eventos Analytics no incluyen nombre completo, email, UID completo como parámetro?
- [ ] ¿El Privacy Manifest de iOS (`PrivacyInfo.xcprivacy`) **existe** en `Assets/Plugins/iOS/` y lista todos los APIs usados (NSUserDefaults, NSUserDefaults para PlayerPrefs, FileManager, Network)?
- [ ] ¿La política de privacidad es accesible desde la pantalla de registro (antes de crear cuenta)?
- [ ] ¿`PlayerPrefs` no almacena datos de comportamiento que requieran consentimiento?
- [ ] ¿El flujo de delete account en `SettingsManager` borra datos de Firebase además de la cuenta Auth?

---

## SECCIONES A AUDITAR

### SECCIÓN 0 — FIREBASE RULES & CLOUD FUNCTIONS
**Archivos**:
- `database.rules.json` (raíz del repo)
- `functions/index.js` (o equivalente — listar todos los archivos en `functions/`)

**Verificar específicamente**:
- ¿La regla raíz NO es `".read": true, ".write": true`?
- ¿`/users/$uid/` solo permite `auth.uid === $uid` para read y write?
- ¿`/leaderboard/` es `".read": true` pero `".write": auth !== null && data.child('uid').val() === auth.uid`?
- ¿`/matchmaking/` y `/tournaments/` tienen reglas que impidan que un usuario modifique el registro de otro?
- ¿Hay reglas `.validate` que restrinjan tipos (ej: score debe ser número, no string)?
- ¿Las Cloud Functions verifican `context.auth` como primer paso antes de cualquier operación?
- ¿Las Cloud Functions que validan recibos de Apple IAP llaman a `https://buy.itunes.apple.com/verifyReceipt` (producción) o `https://sandbox.itunes.apple.com/verifyReceipt` correctamente según el entorno?
- ¿Las Cloud Functions no hardcodean el Apple shared secret en el código (debe estar en Firebase environment variables)?
- ¿Las Cloud Functions tienen manejo de errores con HTTP status codes semánticos (400, 401, 500)?
- ¿No hay `console.log()` con datos financieros o PII en las funciones?
- ¿Las dependencias de `functions/package.json` no tienen vulnerabilidades conocidas (versiones desactualizadas)?

---

### SECCIÓN 1 — CORE BOOT
**Archivos**:
- `Runtime/Core/Boot/BootManager.cs`
- `Runtime/Core/Boot/BootAnimator.cs`
- `Runtime/Core/Boot/EditorBootConfig.cs`

**Verificar específicamente**:
- Orden de inicialización de servicios (Firebase primero, luego rest)
- ¿Se espera `FirebaseApp.CheckAndFixDependenciesAsync()` antes de usar Firebase?
- ¿Se maneja el caso de no-internet en boot?
- ¿`BootAnimator` no crashea si Canvas o elementos son null?
- ¿El flujo post-boot navega correctamente (usuario logueado → MainMenu, no logueado → Login)?
- `EditorBootConfig`:
  - ¿Todo el cuerpo de la clase está bajo `#if UNITY_EDITOR` (garantiza exclusión en builds)?
  - ¿Los PlayerPrefs de bypass (`CashBattleBypassAuth`, `Mock_KYC_Status`) no interfieren con builds release (verificar que BootManager los limpia en producción)?
  - ¿`skipBootFlow = false` es el valor por defecto (no se activa accidentalmente)?

---

### SECCIÓN 2 — CORE NAVIGATION
**Archivos**:
- `Runtime/Core/Navigation/BackButton.cs`
- `Runtime/Core/Navigation/BackButtonGold.cs`
- `Runtime/Core/Navigation/SceneNavigator.cs`
- `Runtime/Navigation/SceneNames.cs`

**Verificar específicamente**:
- ¿`SceneNavigator` tiene manejo de transición doble (anti-spam)?
- ¿Los nombres de escena en `SceneNames.cs` coinciden con los builds settings?
- ¿BackButton no navega a escenas inválidas?
- ¿Se limpia el estado (tweens, coroutines) antes de navegar?

---

### SECCIÓN 3 — CORE NETWORK
**Archivos**:
- `Runtime/Core/Network/NetworkService.cs`
- `Runtime/Core/Network/NetworkStatusBanner.cs`

**Verificar específicamente**:
- ¿`NetworkService` usa `Application.internetReachability` o ping real?
- ¿El banner se muestra/oculta correctamente en hilo principal?
- ¿Las operaciones Firebase se reintentan automáticamente al recuperar conexión?

---

### SECCIÓN 4 — CORE POPUPS
**Archivos**:
- `Runtime/Core/Popups/ConfirmationPopup.cs`
- `Runtime/Core/Popups/ConfirmPanelUI.cs`
- `Runtime/Core/Popups/ErrorPanelUI.cs`
- `Runtime/Core/Popups/InputPanelUI.cs`
- `Runtime/Core/Popups/LogoutConfirmationPopup.cs`
- `Runtime/Core/Popups/PopupManager.cs`
- `Runtime/Core/Popups/UsernamePopup.cs`

**Verificar específicamente**:
- ¿`PopupManager` gestiona la pila de popups correctamente (no se superponen)?
- ¿Los popups bloquean input de fondo mientras están activos?
- ¿Los textos de popups usan `AutoLocalizer.Get()`?
- ¿`InputPanelUI` sanitiza el input antes de usarlo?

---

### SECCIÓN 5 — CORE SAFE AREA & AUDIO
**Archivos**:
- `Runtime/Core/SafeArea/SafeAreaHandler.cs`
- `Runtime/Core/SafeArea/SafeAreaManager.cs`
- `Runtime/Core/Audio/AudioManager.cs`

**Verificar específicamente**:
- ¿`SafeAreaHandler` se actualiza en rotación de pantalla?
- ¿`AudioManager` tiene DontDestroyOnLoad y no crea duplicados?
- ¿El volumen respeta las settings de usuario guardadas?

---

### SECCIÓN 6 — ANIMATIONS CORE
**Archivos**:
- `Runtime/Animations/Core/UIAnimationManager.cs`
- `Runtime/Animations/Core/UIAnimations.cs`
- `Runtime/Animations/AnimConstants.cs`

**Verificar específicamente**:
- ¿`UIAnimationManager` existe en todas las escenas (o es DontDestroyOnLoad)?
- ¿`UIAnimations` métodos estáticos verifican que el target RectTransform no sea null?
- ¿Las constantes de duración en `AnimConstants` son razonables (no 0, no >5s)?

---

### SECCIÓN 7 — ANIMATIONS COMPONENTS
**Archivos**:
- `Runtime/Animations/Components/AnimatedLoadingState.cs`
- `Runtime/Animations/Components/AnimatedPanel.cs`
- `Runtime/Animations/Components/BadgeAnimator.cs`
- `Runtime/Animations/Components/Button3D.cs`
- `Runtime/Animations/Components/CountdownAnimator.cs`
- `Runtime/Animations/Components/EmptyStateAnimator.cs`
- `Runtime/Animations/Components/NavTransitionAnimator.cs`
- `Runtime/Animations/Components/SceneTransitionManager.cs`
- `Runtime/Animations/Components/ScoreRevealAnimator.cs`
- `Runtime/Animations/Components/StaggeredListAnimator.cs`
- `Runtime/Animations/Components/TabTransitionAnimator.cs`
- `Runtime/Animations/Components/UIEffects.cs`

**Verificar específicamente**:
- ¿Todos los tweens se matan en `OnDestroy`?
- ¿`SceneTransitionManager` no deja tweens activos al cambiar escena?
- ¿`CountdownAnimator` no crashea si se activa con el juego ya terminado?
- ¿`StaggeredListAnimator` maneja listas vacías sin error?
- ¿`Button3D` no crashea si no tiene collider?
- ¿`AnimatedPanel` no crashea si `CanvasGroup` es null?

---

### SECCIÓN 8 — ANIMATIONS ANIMATORS
**Archivos**:
- `Runtime/Animations/Animators/CashProfileAnimator.cs`
- `Runtime/Animations/Animators/CurrencyAnimator.cs`
- `Runtime/Animations/Animators/GameSelectorAnimator.cs`
- `Runtime/Animations/Animators/MainMenuAnimator.cs`
- `Runtime/Animations/Animators/MatchmakingAnimator.cs`
- `Runtime/Animations/Animators/ParticleEffectSpawner.cs`
- `Runtime/Animations/Animators/RewardClaimAnimator.cs`
- `Runtime/Animations/Animators/TrophyShowcaseAnimator.cs`

**Verificar específicamente**:
- ¿`CurrencyAnimator` maneja cantidades negativas/cero sin errores visuales?
- ¿`MatchmakingAnimator` cancela animaciones cuando se cancela el matchmaking?
- ¿`ParticleEffectSpawner` usa pooling o crea/destruye correctamente?
- ¿`TrophyShowcaseAnimator` no crashea con lista vacía de trofeos?

---

### SECCIÓN 9 — EFFECTS
**Archivos**:
- `Runtime/Effects/ButtonEffects.cs`
- `Runtime/Effects/CelebrationManager.cs`
- `Runtime/Effects/FeedbackManager.cs`
- `Runtime/Effects/FloatingText.cs`
- `Runtime/Effects/NeonGlowEffect.cs`
- `Runtime/Effects/ParticleSystemManager.cs`

**Verificar específicamente**:
- ¿`ButtonEffects` usa `while(isActiveAndEnabled)` (no `while(true)`)?
- ¿`CelebrationManager` tiene fallbacks para todos los `Shader.Find()` (≥4 shaders)?
- ¿`ParticleSystemManager` tiene fallbacks si `Shader.Find()` retorna null?
- ¿`FloatingText` tiene `DontDestroyOnLoad` y refresca canvas al recargar escena?
- ¿`FeedbackManager` carga settings en `Initialize()` antes de usarlas?
- ¿`NeonGlowEffect` no genera garbage excesivo en Update()?

---

### SECCIÓN 10 — SERVICES FIREBASE
**Archivos**:
- `Runtime/Services/Firebase/AuthenticationService.cs`
- `Runtime/Services/Firebase/DatabaseService.cs`
- `Runtime/Services/Firebase/AnalyticsService.cs`
- `Runtime/Services/Firebase/NotificationService.cs`

**Verificar específicamente**:
- `AuthenticationService`:
  - ¿Null-check post `JsonUtility.FromJson`?
  - ¿Maneja token expirado (silent re-login)?
  - ¿No loguea email/contraseña?
  - ¿Tiene listener de estado de auth (`OnAuthStateChanged`)?
- `DatabaseService`:
  - ¿`UpdatePlayerFields()` hace merge, no overwrite?
  - ¿Null-check post `JsonUtility.FromJson` para TournamentData?
  - ¿Todos los paths de DB son correctos (no typos)?
  - ¿No hay escrituras en paths de otros usuarios?
- `AnalyticsService`:
  - ¿Respeta ATT (no loguea si no hay consent en iOS)?
  - ¿`LogCustomEvent()` existe y funciona?
  - ¿No loguea PII (nombres, emails, UIDs completos)?
- `NotificationService`:
  - ¿Callbacks FCM van via `UnityMainThreadDispatcher`?
  - ¿No crashea si el token FCM es null?
  - ¿Las notificaciones locales se cancelan correctamente?

---

### SECCIÓN 11 — SERVICES CORE
**Archivos**:
- `Runtime/Services/AchievementService.cs`
- `Runtime/Services/ATTService.cs`
- `Runtime/Services/AvatarService.cs`
- `Runtime/Services/ChatFilterService.cs`
- `Runtime/Services/DailyRewardService.cs`
- `Runtime/Services/DeepLinkService.cs`
- `Runtime/Services/EmoteService.cs`
- `Runtime/Services/FriendService.cs`
- `Runtime/Services/LocationRestrictionService.cs`
- `Runtime/Services/MatchmakingService.cs`
- `Runtime/Services/NotificationStorageService.cs`
- `Runtime/Services/PlayerFrameService.cs`
- `Runtime/Services/PlayerTitleService.cs`
- `Runtime/Services/ReviewService.cs`
- `Runtime/Services/ServiceLocator.cs`
- `Runtime/Services/UnityMainThreadDispatcher.cs`
- `Runtime/Services/VictoryEffectService.cs`

**Verificar específicamente**:
- `AchievementService`:
  - ¿Icons se cargan desde `Resources/Icons/Achievements/` correctamente?
  - ¿Firebase persiste progreso de logros?
  - ¿No hay race condition al completar múltiples logros simultáneos?
- `ATTService`:
  - ¿Solo corre en iOS (no en Android)?
  - ¿El resultado se propaga a `AnalyticsService`?
- `AvatarService`:
  - ¿Las texturas se cachean correctamente (no descarga repetida)?
  - ¿Maneja URLs de avatar inválidas/null?
- `DailyRewardService`:
  - ¿Usa server timestamp Firebase (no tiempo local)?
  - ¿Persiste estado en Firebase?
- `DeepLinkService`:
  - ¿Valida el esquema `digitpark://` antes de procesar?
  - ¿Sanitiza parámetros de deep link?
- `LocationRestrictionService`:
  - ¿No bloquea la app completa, solo CashBattle?
- `MatchmakingService`:
  - ¿Limpia listeners Firebase al cancelar/completar matchmaking?
  - ¿Tiene timeout para no quedarse buscando indefinidamente?
- `NotificationStorageService`:
  - ¿Sincroniza con Firebase (no solo PlayerPrefs)?
- `ServiceLocator`:
  - ¿Registra/desregistra servicios correctamente?
  - ¿Lanza excepción clara si se pide un servicio no registrado?
- `UnityMainThreadDispatcher`:
  - ¿Existe en escena boot como DontDestroyOnLoad?
  - ¿La queue se procesa en Update()?
- `ReviewService`:
  - ¿No muestra review prompt más de una vez por sesión?
  - ¿Respeta las condiciones mínimas (N partidas jugadas)?

---

### SECCIÓN 12 — TRIUMPH SERVICES (Solo audit crashes/warnings/security — NO Firebase)
**Archivos**:
- `Runtime/Services/Triumph/TriumphManager.cs`
- `Runtime/Services/Triumph/TriumphServices.cs`
- `Runtime/Services/Interfaces/IKYCService.cs`
- `Runtime/Services/Interfaces/IMatchmakingService.cs`
- `Runtime/Services/Interfaces/ITournamentService.cs`
- `Runtime/Services/Interfaces/IWalletService.cs`
- `Runtime/Services/Mock/MockKYCService.cs`
- `Runtime/Services/Mock/MockMatchmakingService.cs`
- `Runtime/Services/Mock/MockTournamentService.cs`
- `Runtime/Services/Mock/MockWalletService.cs`

**Verificar específicamente**:
- ¿Las implementaciones Mock no crashean con datos null?
- ¿Las interfaces están completamente implementadas (no `NotImplementedException`)?
- ¿`TriumphManager` tiene fallback graceful si el SDK no está disponible?
- ¿No hay `Debug.Log` con datos financieros sensibles?

---

### SECCIÓN 13 — LOCALIZATION
**Archivos**:
- `Runtime/Localization/AutoLocalizer.cs`
- `Runtime/Localization/LocalizationManager.cs`
- `Runtime/Localization/LocalizedTextComponent.cs`
- `Assets/_Project/Localization/Translations.txt`
- `Assets/_Project/Resources/Translations.txt`
- `Assets/_Project/Scripts/Localization/Translations.txt`

**Verificar específicamente**:
- ¿Los 3 archivos `Translations.txt` son idénticos?
- ¿Cada key tiene exactamente 5 traducciones (EN/ES/FR/PT/DE)?
- ¿No hay keys duplicadas?
- ¿Todas las keys usadas en código (buscar `AutoLocalizer.Get("`) existen en Translations.txt?
- ¿`TextNameToKeyMap` en AutoLocalizer.cs cubre todos los GameObjects de texto importantes?
- ¿No hay GO names duplicados en TextNameToKeyMap que causen conflictos de diccionario?
- ¿`LocalizationManager` notifica a todos los `LocalizedTextComponent` al cambiar idioma?
- ¿`AutoLocalizer.Get()` retorna la key como fallback (no string vacío o null) si la key no existe?
- ¿Los placeholders `{0}`, `{1}` en traducciones coinciden con los argumentos pasados?
- Hacer cross-check semántico: leer las 20 keys más importantes y verificar que EN/ES/FR/PT/DE tienen el significado correcto

---

### SECCIÓN 14 — AUTH
**Archivos**:
- `Runtime/Features/Auth/AgeVerificationManager.cs`
- `Runtime/Features/Auth/ForgotPasswordHoverEffect.cs`
- `Runtime/Features/Auth/ForgotPasswordPopup.cs`
- `Runtime/Features/Auth/LoginManager.cs`
- `Runtime/Features/Auth/PasswordToggle.cs`
- `Runtime/Features/Auth/RegisterManager.cs`

**Verificar específicamente**:
- ¿`LoginManager` usa `AuthenticationService` (no Firebase directo)?
- ¿`RegisterManager` valida email y contraseña antes de llamar Firebase?
- ¿Se loguea evento Analytics en login exitoso/fallido?
- ¿`ForgotPasswordPopup` sanitiza el email antes de enviarlo?
- ¿`AgeVerificationManager` usa la key correcta de localization?
- ¿Los errores de Firebase Auth se muestran con mensajes localizados (no error codes raw)?
- ¿`PasswordToggle` no expone la contraseña en logs?
- ¿El flujo de registro completa onboarding antes de ir a MainMenu?

---

### SECCIÓN 15 — GAMES CORE
**Archivos**:
- `Runtime/Features/Games/Core/CognitiveSprintManager.cs`
- `Runtime/Features/Games/Core/GameContext.cs`
- `Runtime/Features/Games/Core/GameMode.cs`
- `Runtime/Features/Games/Core/GameSelectorManager.cs`
- `Runtime/Features/Games/Core/GameSessionManager.cs`
- `Runtime/Features/Games/Core/GameType.cs`
- `Runtime/Features/Games/Core/IMinigame.cs`
- `Runtime/Features/Games/Core/MinigameBase.cs`
- `Runtime/Features/Games/Core/MinigameConfig.cs`
- `Runtime/Features/Games/Core/MinigameResult.cs`

**Verificar específicamente**:
- ¿`GameSessionManager` guarda resultados en Firebase al terminar una partida?
- ¿`MinigameBase` cancela coroutines en `OnDestroy`?
- ¿`GameSelectorManager` loguea Analytics al seleccionar juego?
- ¿`CognitiveSprintManager` maneja correctamente la secuencia de juegos?
- ¿`MinigameResult` incluye timestamp Firebase (no `DateTime.Now`)?
- ¿El flujo Ranked actualiza el ranking en Firebase?

---

### SECCIÓN 16 — GAMES MINIGAMES
**Archivos**:
- `Runtime/Features/Games/DigitRush/DigitRushController.cs`
- `Runtime/Features/Games/DigitRush/EffectsController.cs`
- `Runtime/Features/Games/DigitRush/TileController.cs`
- `Runtime/Features/Games/FlashTap/FlashTapButton3D.cs`
- `Runtime/Features/Games/FlashTap/FlashTapController.cs`
- `Runtime/Features/Games/FlashTap/TapButtonEffect.cs`
- `Runtime/Features/Games/MemoryPairs/Card3DEffect.cs`
- `Runtime/Features/Games/MemoryPairs/MemoryPairsController.cs`
- `Runtime/Features/Games/OddOneOut/Cell3DButton.cs`
- `Runtime/Features/Games/OddOneOut/OddOneOutCell3D.cs`
- `Runtime/Features/Games/OddOneOut/OddOneOutController.cs`
- `Runtime/Features/Games/QuickMath/QuickMathCell3D.cs`
- `Runtime/Features/Games/QuickMath/QuickMathController.cs`

**Verificar específicamente**:
- ¿Cada controller resetea estado al iniciar nueva partida?
- ¿Los timers usan `Time.deltaTime` (no `DateTime.Now`)?
- ¿Los efectos 3D (`*Button3D`, `*Cell3D`) tienen null-checks en sus colliders?
- ¿`MemoryPairsController` maneja el caso de voltear 2 cartas simultáneamente (race condition)?
- ¿`OddOneOutController` genera correctamente el "odd one out" (no puede ser que todos sean iguales)?
- ¿`QuickMathController` valida que la respuesta correcta esté entre las opciones?
- ¿`DigitRushController` maneja el input durante el contador "GO!" (no debería aceptar)?
- ¿Los efectos de partícula se destruyen correctamente al terminar la partida?

---

### SECCIÓN 17 — GAMES NAVIGATION & RESULTS
**Archivos**:
- `Runtime/Features/Games/Navigation/BetSelectionPanel.cs`
- `Runtime/Features/Games/Navigation/CountdownUI.cs`
- `Runtime/Features/Games/Navigation/GameCardEffect.cs`
- `Runtime/Features/Games/Navigation/GridGlowPulse.cs`
- `Runtime/Features/Games/Navigation/MatchmakingManager.cs`
- `Runtime/Features/Games/Navigation/PlayModeSelectionManager.cs`
- `Runtime/Features/Games/Results/ComboVisualController.cs`
- `Runtime/Features/Games/Results/OnlineResultManager.cs`
- `Runtime/Features/Games/Results/OnlineResultPanelController.cs`
- `Runtime/Features/Games/Results/ResultPanelManager.cs`
- `Runtime/Features/Games/Results/SprintSummaryPanelController.cs`
- `Runtime/Features/Games/Results/UISparkleEffect.cs`
- `Runtime/Features/Games/Results/WinCelebrationAnimator.cs`
- `Runtime/Features/Games/Results/WinPanelController.cs`

**Verificar específicamente**:
- ¿`MatchmakingManager` tiene timeout y cancela listeners Firebase?
- ¿`OnlineResultManager` sincroniza resultado con Firebase (ambos jugadores)?
- ¿`WinPanelController` maneja correctamente empates?
- ¿`ResultPanelManager` loguea Analytics del resultado?
- ¿`SprintSummaryPanelController` muestra datos de todos los juegos del sprint?
- ¿`ComboVisualController` no crashea con combo = 0?
- ¿`UISparkleEffect` limpia partículas en OnDisable?
- ¿`BetSelectionPanel` valida que el usuario tiene saldo suficiente?

---

### SECCIÓN 18 — MONETIZATION
**Archivos**:
- `Runtime/Features/Monetization/Achievements/AchievementItemUI.cs`
- `Runtime/Features/Monetization/Achievements/AchievementsManager.cs`
- `Runtime/Features/Monetization/Achievements/CategoryHeaderUI.cs`
- `Runtime/Features/Monetization/Achievements/TrophyCardUI.cs`
- `Runtime/Features/Monetization/Currency/CurrencyDisplayUI.cs`
- `Runtime/Features/Monetization/Currency/CurrencyManager.cs`
- `Runtime/Features/Monetization/DailyMissions/DailyMissionsManager.cs`
- `Runtime/Features/Monetization/DailyMissions/MissionCardUI.cs`
- `Runtime/Features/Monetization/DailyRewards/DailyRewardsManager.cs`
- `Runtime/Features/Monetization/DailyRewards/RewardDayItemUI.cs`
- `Runtime/Features/Monetization/Premium/PremiumManager.cs`
- `Runtime/Features/Monetization/Progression/MissionsManager.cs`
- `Runtime/Features/Monetization/Progression/PlayerProgressionSystem.cs`
- `Runtime/Features/Monetization/Shop/ShopItemData.cs`
- `Runtime/Features/Monetization/Shop/ShopItemUI.cs`
- `Runtime/Features/Monetization/Shop/ShopManager.cs`

**Verificar específicamente**:
- `AchievementsManager`:
  - ¿Usa `DatabaseService` para persistir en Firebase?
  - ¿Icons se cargan de `Resources/Icons/Achievements/`?
  - ¿`AchievementService` y `AchievementsManager` no están duplicando lógica?
- `CurrencyManager`:
  - ¿Persiste monedas en Firebase (no solo PlayerPrefs)?
  - ¿No permite saldo negativo?
  - ¿Las transacciones son atómicas (no race conditions con Firebase)?
- `DailyMissionsManager`:
  - ¿Reset diario usa server timestamp Firebase?
  - ¿Persiste progreso en Firebase?
- `DailyRewardsManager`:
  - ¿Usa server timestamp Firebase (no tiempo local)?
  - ¿No permite reclamar dos veces el mismo día?
- `ShopManager`:
  - ¿Loguea eventos Analytics de compra?
  - ¿Verifica fondos antes de completar compra?
  - ¿Las compras se reflejan en Firebase?
- `PremiumManager`:
  - ¿Verifica el estado premium desde Firebase (no solo local)?

---

### SECCIÓN 19 — SOCIAL
**Archivos**:
- `Runtime/Features/Social/Friends/FriendRequestsSceneManager.cs`
- `Runtime/Features/Social/Friends/FriendsManager.cs`
- `Runtime/Features/Social/Friends/PlayerSearchItemUI.cs`
- `Runtime/Features/Social/Friends/SearchPlayersManager.cs`
- `Runtime/Features/Social/Notifications/NotificationsManager.cs`
- `Runtime/Features/Social/Profile/LeaderboardManager.cs`
- `Runtime/Features/Social/Profile/MatchHistorySceneManager.cs`
- `Runtime/Features/Social/Profile/ProfileManager.cs`

**Verificar específicamente**:
- `FriendsManager`:
  - ¿Solicitudes de amistad se guardan en Firebase?
  - ¿Se puede enviar solicitud solo una vez (no duplicados)?
  - ¿Se notifica al receptor via `NotificationService`?
- `SearchPlayersManager`:
  - ¿Sanitiza el input de búsqueda antes de consultar Firebase?
  - ¿No expone datos de usuarios que no deben ser públicos?
- `LeaderboardManager`:
  - ¿Tiene retry logic y cache offline?
  - ¿Paginación funciona correctamente?
- `MatchHistorySceneManager`:
  - ¿Carga historial de Firebase correctamente?
  - ¿Ordena por timestamp descendente?
- `NotificationsManager`:
  - ¿Sincroniza con `NotificationStorageService` y Firebase?
  - ¿Marca como leídas en Firebase?
- `InAppNotificationManager.AcceptFriendRequest`:
  - ¿Tiene try-catch?

---

### SECCIÓN 20 — TOURNAMENTS
**Archivos**:
- `Runtime/Features/Tournaments/ParticipantItemUI.cs`
- `Runtime/Features/Tournaments/PrizeRowItemUI.cs`
- `Runtime/Features/Tournaments/TournamentCreateManager.cs`
- `Runtime/Features/Tournaments/TournamentItemUI.cs`
- `Runtime/Features/Tournaments/TournamentLobbyManager.cs`
- `Runtime/Features/Tournaments/TournamentManager.cs`
- `Runtime/Features/Tournaments/TournamentMyItemUI.cs`
- `Runtime/Features/Tournaments/TournamentResultPanelController.cs`
- `Runtime/Features/Tournaments/TournamentsBrowserManager.cs`
- `Runtime/Features/Tournaments/TournamentSearchItemUI.cs`

**Verificar específicamente**:
- ¿`TournamentManager` usa `DatabaseService` para CRUD de torneos?
- ¿`TournamentLobbyManager` escucha cambios en tiempo real (Firebase listener)?
- ¿`TournamentCreateManager` valida todos los campos antes de crear?
- ¿`TournamentResultPanelController` cierra correctamente el torneo en Firebase?
- ¿Los textos usan `AutoLocalizer.Get()` (no strings hardcodeados)?
- ¿`TournamentsBrowserManager` limpia listeners al salir de la escena?

---

### SECCIÓN 21 — ONBOARDING
**Archivos**:
- `Runtime/Features/Onboarding/AvatarOptionItemUI.cs`
- `Runtime/Features/Onboarding/CashBattleOnboardingManager.cs`
- `Runtime/Features/Onboarding/OnboardingManager.cs`
- `Runtime/Features/Onboarding/StepDotItemUI.cs`

**Verificar específicamente**:
- ¿`OnboardingManager` marca onboarding como completado en Firebase?
- ¿`CashBattleOnboardingManager` solo se muestra una vez?
- ¿El avatar seleccionado se guarda en Firebase en `AvatarOptionItemUI`?
- ¿Todos los textos de onboarding están localizados?

---

### SECCIÓN 22 — SETTINGS & MAIN MENU
**Archivos**:
- `Runtime/Features/Settings/ForceShowSettingsLabels.cs`
- `Runtime/Features/Settings/SettingsManager.cs`
- `Runtime/Features/MainMenu/MainMenuManager.cs`

**Verificar específicamente**:
- `SettingsManager`:
  - ¿Guarda preferencias en Firebase (no solo PlayerPrefs)?
  - ¿Cambio de idioma refresca todos los textos inmediatamente?
  - ¿El toggle de vibración funciona correctamente (fallback dinámico)?
  - ¿Las confirmaciones de delete/logout usan `AutoLocalizer.Get()`?
- `MainMenuManager`:
  - ¿Carga datos de usuario desde Firebase (nivel, monedas, avatar)?
  - ¿`AutoLocalizer.Get("no_username")` en lugar de "Sin Usuario"?
  - ¿Los badges de notificaciones se actualizan correctamente?

---

### SECCIÓN 23 — CASHBATTLE (crash/warning/security audit — no Firebase audit Triumph areas)
**Archivos**:
- `Runtime/Features/CashBattle/History/CashHistorySceneController.cs`
- `Runtime/Features/CashBattle/History/HistoryData.cs`
- `Runtime/Features/CashBattle/History/HistoryEntryItemUI.cs`
- `Runtime/Features/CashBattle/History/HistoryManager.cs`
- `Runtime/Features/CashBattle/Hub/CashBattle1v1Manager.cs`
- `Runtime/Features/CashBattle/Hub/CashBattleManager.cs`
- `Runtime/Features/CashBattle/Hub/CashMatchmakingManager.cs`
- `Runtime/Features/CashBattle/Hub/LocationRestrictionOverlay.cs`
- `Runtime/Features/CashBattle/Profile/CashProfileSceneController.cs`
- `Runtime/Features/CashBattle/Results/CashBattleResultPanelController.cs`
- `Runtime/Features/CashBattle/Tournaments/CashTournamentCreateManager.cs`
- `Runtime/Features/CashBattle/Tournaments/CashTournamentLobbyManager.cs`
- `Runtime/Features/CashBattle/Tournaments/CashTournamentResultsPanelController.cs`
- `Runtime/Features/CashBattle/Tournaments/CashTournamentsManager.cs`
- `Runtime/Features/CashBattle/Wallet/CashWalletSceneController.cs`
- `Runtime/Features/CashBattle/Wallet/DepositOptionUI.cs`
- `Runtime/Features/CashBattle/Wallet/TransactionItemUI.cs`
- `Runtime/Features/CashBattle/Wallet/WalletData.cs`
- `Runtime/Features/CashBattle/Wallet/WalletManager.cs`

**Verificar específicamente**:
- ¿Todos los textos usan `AutoLocalizer.Get()` con prefijos "Cash" en GO names?
- ¿`LocationRestrictionOverlay` no crashea si el servicio de localización falla?
- ¿`CashBattle1v1Manager` tiene null-checks en ServiceLocator.Get()?
- ¿Los datos financieros no se loguean en Debug.Log?
- ¿`WalletManager` valida que el monto de depósito sea positivo y dentro de límites?
- ¿`CashBattleResultPanelController` maneja el caso de resultado nulo/error?
- ¿No hay `NotImplementedException` que pueda crashear en runtime?
- ¿Los CashGO names (CashTimerText, CashJoinButtonText, etc.) coinciden con TextNameToKeyMap?

---

### SECCIÓN 24 — PAYMENTS
**Archivos**:
- `Runtime/Payments/Core/PaymentManager.cs`
- `Runtime/Payments/Core/PaymentConfig.cs`
- `Runtime/Payments/Core/PaymentEvents.cs`
- `Runtime/Payments/Core/PaymentResult.cs`
- `Runtime/Payments/Core/ProductCatalog.cs`
- `Runtime/Payments/Core/IPaymentProvider.cs`
- `Runtime/Payments/Core/AbortReason.cs`
- `Runtime/Payments/AppleIAP/AppleIAPProvider.cs`
- `Runtime/Payments/AppleIAP/AppleReceiptValidator.cs`
- `Runtime/Payments/Stripe/StripeCheckoutController.cs`
- `Runtime/Payments/Stripe/StripeComplianceGuard.cs`
- `Runtime/Payments/Stripe/StripePaymentProvider.cs`
- `Runtime/Payments/Stripe/StripeSessionPoller.cs`
- `Runtime/Payments/Abort/StripeAbortProtocol.cs`
- `Runtime/Payments/Compliance/TriumphIsolationGuard.cs`
- `Runtime/Payments/Compliance/VersionGuard.cs`
- `Runtime/Payments/Entitlements/EntitlementRecord.cs`
- `Runtime/Payments/Entitlements/EntitlementService.cs`
- `Runtime/Payments/FeatureFlags/LocalFlagCache.cs`
- `Runtime/Payments/FeatureFlags/PaymentFeatureFlag.cs`
- `Runtime/Payments/FeatureFlags/RemoteConfigService.cs`
- `Runtime/Payments/UI/PaymentErrorDialog.cs`
- `Runtime/Payments/UI/PaymentLoadingOverlay.cs`

**Verificar específicamente**:
- `PaymentManager`:
  - ¿Bloquea usuarios anónimos?
  - ¿Loguea evento Analytics de compra?
  - ¿Maneja timeout de pago?
- `AppleReceiptValidator`:
  - ¿Fail-closed (rechaza si receipt vacío O backend unreachable)?
  - ¿No expone el receipt en logs?
- `StripePaymentProvider`:
  - ¿No hardcodea API keys?
  - ¿Usa HTTPS para todas las llamadas?
- `StripeSessionPoller`:
  - ¿Tiene máximo de intentos (no loop infinito)?
- `EntitlementService`:
  - ¿Persiste entitlements en Firebase?
  - ¿Verifica entitlements server-side (no solo local)?
- `RemoteConfigService`:
  - ¿Tiene valores por defecto para todos los flags?
  - ¿No cachea indefinidamente (tiene TTL)?
- `PaymentErrorDialog` / `PaymentLoadingOverlay`:
  - ¿Usan `AutoLocalizer.Get()`?
  - ¿Se cierran correctamente en todos los paths?

---

### SECCIÓN 25 — THEMES
**Archivos**:
- `Runtime/Themes/CashThemeForcer.cs`
- `Runtime/Themes/NeonThemeColors.cs`
- `Runtime/Themes/ThemeApplier.cs`
- `Runtime/Themes/ThemeData.cs`
- `Runtime/Themes/ThemeInitializer.cs`
- `Runtime/Themes/ThemeManager.cs`

**Verificar específicamente**:
- ¿`ThemeManager` tiene `#if UNITY_EDITOR` guard para `PremiumDebugController`?
- ¿`ThemeApplier` está conectado a las escenas (no flotando sin referencia)?
- ¿`ThemeInitializer` inicializa antes de que cualquier UI se renderice?
- ¿`ThemeManager` persiste el tema seleccionado en Firebase?
- ¿`ThemeSelector` abre `PremiumPanelUI` para temas bloqueados (no desbloquea directamente)?
- ¿`CashThemeForcer` no interfiere con el sistema principal de temas?

---

### SECCIÓN 26 — UI COMPONENTS & PANELS
**Archivos**:
- `Runtime/UI/Common/FontSizes.cs`
- `Runtime/UI/Common/LocalizedTextLayoutFixer.cs`
- `Runtime/UI/Common/UICanvasHelper.cs`
- `Runtime/UI/Common/UIFactory.cs`
- `Runtime/UI/Components/AccessibilityHelper.cs`
- `Runtime/UI/Components/AvatarInitialGenerator.cs`
- `Runtime/UI/Components/AvatarUI.cs`
- `Runtime/UI/Components/DropdownScrollFix.cs`
- `Runtime/UI/Components/LanguageDropdownStyler.cs`
- `Runtime/UI/Components/NeonButtonGlow.cs`
- `Runtime/UI/Components/RoundedCorners.cs`
- `Runtime/UI/Components/ThemeDropdownController.cs`
- `Runtime/UI/Components/ThemeSelector.cs`
- `Runtime/UI/Components/UIPolish.cs`
- `Runtime/UI/Items/LeaderboardEntryUI.cs`
- `Runtime/UI/Notifications/AchievementNotificationInitializer.cs`
- `Runtime/UI/Notifications/AchievementNotificationManager.cs`
- `Runtime/UI/Notifications/AchievementToastUI.cs`
- `Runtime/UI/Notifications/InAppNotificationInitializer.cs`
- `Runtime/UI/Notifications/InAppNotificationManager.cs`
- `Runtime/UI/Notifications/InAppToastUI.cs`
- `Runtime/UI/Panels/PremiumCard.cs`
- `Runtime/UI/Panels/PremiumPanelUI.cs`
- `Runtime/UI/Panels/StylesProPromptPanel.cs`

**Verificar específicamente**:
- ¿`LanguageDropdownStyler` tiene el namespace correcto?
- ¿`DropdownScrollFix` reparenta el Dropdown List fuera del RectMask2D?
- ¿`AccessibilityHelper` funciona para VoiceOver/TalkBack correctamente?
- ¿`NeonButtonGlow` tiene throttle a 30fps (no Update completo)?
- ¿`InAppNotificationManager.AcceptFriendRequest` tiene try-catch?
- ¿`AchievementToastUI` limpia tweens en OnDisable?
- ¿`PremiumPanelUI` verifica estado real de premium desde Firebase?
- ¿`ThemeSelector` abre el panel premium en vez de desbloquear directamente?
- ¿`LocalizedTextLayoutFixer` se ejecuta después de que AutoLocalizer actualiza el texto?

---

### SECCIÓN 27 — UI BUILDERS (Runtime)
**Archivos**:
- `Runtime/UI/Builders/BootUIBuilder.cs`
- `Runtime/UI/Builders/DailyRewardPanelBuilder.cs`

**Verificar específicamente**:
- ¿Usan `AutoLocalizer.Get()` para todos los textos?
- ¿Los elementos TMP tienen `autoSizing = true` y `fontSizeMin >= 24`?
- ¿No crean objetos duplicados si se llaman múltiples veces?

---

### SECCIÓN 28 — DATA MODELS
**Archivos**:
- `Runtime/Data/FrameData.cs`
- `Runtime/Data/FriendData.cs`
- `Runtime/Data/MatchHistoryData.cs`
- `Runtime/Data/Missions/MissionDefinitionSO.cs`
- `Runtime/Data/Missions/MissionPoolSO.cs`
- `Runtime/Data/Missions/MissionProgressReporter.cs`
- `Runtime/Data/PlayerData.cs`
- `Runtime/Data/PlayerSettings.cs`
- `Runtime/Data/TournamentData.cs`

**Verificar específicamente**:
- ¿`PlayerData` tiene todos los campos que se usan en código?
- ¿`TournamentData` tiene null-check post `JsonUtility.FromJson`?
- ¿`PlayerSettings` no almacena datos sensibles (contraseñas, tokens)?
- ¿`MissionProgressReporter` reporta a Firebase correctamente?
- ¿Los ScriptableObjects (`MissionDefinitionSO`, `MissionPoolSO`) son inmutables en runtime?

---

### SECCIÓN 29 — DEV TOOLS
**Archivos**:
- `Runtime/DevTools/AchievementDebugPanel.cs`
- `Runtime/DevTools/DebugManager.cs`
- `Runtime/DevTools/PremiumDebugController.cs`
- `Runtime/DevTools/SettingsTextRuntimeDebug.cs`

**Verificar específicamente**:
- ¿Todos están bajo `#if UNITY_EDITOR || DEVELOPMENT_BUILD`?
- ¿`PremiumDebugController` no está accesible en builds release?
- ¿`DebugManager` no expone endpoints o datos sensibles?

---

### SECCIÓN 30 — EDITOR UIBUILDERS
**Archivos** (todos los de `Editor/`):
- Auth: `AgeVerificationUIBuilder.cs`, `LoginUIBuilder.cs`, `RegisterUIBuilder.cs`
- Games: `DigitRushUIBuilder.cs`, `FlashTapUIBuilder.cs`, `MemoryPairsUIBuilder.cs`, `OddOneOutUIBuilder.cs`, `QuickMathUIBuilder.cs`
- Navigation: `BetSelectionPanelUIBuilder.cs`, `GameSelectorUIBuilder.cs`, `MatchmakingUIBuilder.cs`, `PlayModeSelectionUIBuilder.cs`
- Monetization: `AchievementsUIBuilder.cs`, `DailyMissionsUIBuilder.cs`, `DailyRewardsPremiumUIBuilder.cs`, `ShopPremiumUIBuilder.cs`
- Social: `FriendRequestsUIBuilder.cs`, `FriendsUIBuilder.cs`, `SearchPlayersUIBuilder.cs`, `NotificationsUIBuilder.cs`, `MatchHistoryUIBuilder.cs`, `ProfileUIBuilder.cs`, `ScoresUIBuilder.cs`
- Tournaments: `TournamentCreateUIBuilder.cs`, `TournamentLobbyUIBuilder.cs`, `TournamentsBrowserUIBuilder.cs`
- CashBattle: todos los `Cash*UIBuilder.cs`
- WinPanels: todos los `*PanelUIBuilder.cs`
- Settings/Onboarding/Core: `SettingsUIBuilder.cs`, `OnboardingUIBuilder.cs`, `MainMenuUIBuilder.cs`

**Verificar en TODOS los UIBuilders**:
- ¿Los textos placeholder son en inglés (base lang)?
- ¿Todos los TMP texts tienen `autoSizing = true`?
- ¿`fontSizeMax >= fontSizeMin` (no invertidos, ej: max=24, min=36 es bug)?
- ¿`fontSizeMin >= 16` (nunca menor)?
- ¿Todos los textos visibles tienen nombre de GO que coincide con TextNameToKeyMap?
- ¿No hay `fontStyle = Bold` faltante en títulos/botones?
- ¿Los botones tienen `Navigation.mode = None` para no interferir con accessibility?

---

### SECCIÓN 31 — EDITOR AUTOASSIGNERS
**Archivos**: Listar con `Glob("Assets/_Project/Scripts/Editor/**/*ReferenceAssigner.cs")` y `Glob("Assets/_Project/Scripts/Editor/**/*IconAssigner.cs")` al inicio de la sesión para obtener la lista completa. Auditar **todos** los que aparezcan.

**Verificar en TODOS los AutoAssigners**:
- ¿Usan `transform.Find("NombreExacto")` con null-check, NO fuzzy match ni `GetComponentsInChildren` sin filtro?
- ¿Usan nombres exactos de GO (revisar contra la lista de GO en el UIBuilder correspondiente)?
- ¿Loguean `Debug.LogError` claro indicando qué referencia no se encontró (incluyendo nombre del GO buscado)?
- ¿Son idempotentes — ejecutar dos veces seguidas no genera duplicados ni estados corruptos?
- ¿No asignan referencias cruzadas de escenas distintas (ej: asignar un GO de MainMenu a un script de Game)?
- ¿Los `*IconAssigner` verifican que los Sprites existen en Resources antes de asignar?
- ¿Están bajo `[MenuItem("DigitPark/...")]` con la ruta de menú correcta y no se ejecutan en runtime?

---

### SECCIÓN 32 — EDITOR TOOLS
**Archivos**: Listar con los siguientes Globs al inicio de la sesión para obtener la lista completa:
- `Glob("Assets/_Project/Scripts/Editor/Tools/**/*.cs")`
- `Glob("Assets/_Project/Scripts/Editor/Games/**/*.cs")` ← incluye `CashThemePreview.cs`
- `Glob("Assets/_Project/Scripts/Editor/Payments/**/*.cs")` ← incluye `BuildProfileSwitcher.cs`, `PaymentDebugWindow.cs`

**Verificar en TODOS los Editor Tools**:
- ¿`BuildScenesConfigurator` incluye **todas** las escenas en el orden correcto y coincide con el Build Settings actual?
- ¿`AllScenesBatchBuilder` invoca los UIBuilders via `MenuItem` o métodos estáticos — NO modifica `.unity`/`.prefab` directamente con YAML?
- ¿`AnimationManagersRepairTool` hace backup o tiene undo antes de modificar escenas?
- ¿Todas las operaciones destructivas (borrar objetos, reemplazar prefabs) muestran `EditorUtility.DisplayDialog` de confirmación?
- ¿Ninguna herramienta usa `AssetDatabase.SaveAssets()` en un loop sin `AssetDatabase.StartAssetEditing()` / `StopAssetEditing()` (performance)?
- ¿Las herramientas están bajo `#if UNITY_EDITOR` y no se compilan en builds?

---

### SECCIÓN 33 — TESTS
**Archivos**: Listar con `Glob("Assets/_Project/Scripts/Tests/**/*.cs")` al inicio de la sesión.

**Verificar**:
- ¿Los tests cubren al menos los flujos críticos de pago (`PaymentManager`, `AppleReceiptValidator`)?
- ¿Los tests cubren `AuthenticationService` (login, registro, token expirado)?
- ¿Los tests cubren `CurrencyManager` (saldo negativo, race conditions)?
- ¿Los tests usan mocks/stubs para Firebase (no dependen de red real)?
- ¿Los tests se ejecutan en modo `EditMode` o `PlayMode` correctamente según su naturaleza?
- ¿No hay tests que modifiquen datos de Firebase de producción?
- ¿Los tests están en el Assembly Definition correcto (no se compilan en builds release)?
- Reportar la **cobertura estimada**: qué áreas críticas NO tienen tests.

---

## FORMATO DE REPORTE FINAL

Al terminar toda la auditoría (las 3 sesiones), generar reporte unificado:

```markdown
# AUDIT REPORT V49 — DigitPark

## RESUMEN EJECUTIVO
- Total archivos auditados: X (.cs) + database.rules.json + functions/
- 🔴 P0 (crashes/blockers): X issues
- 🟠 P1 (bugs funcionales): X issues
- 🟡 P2 (warnings/degradación): X issues
- 🔵 P3 (mejoras): X issues
- ✅ Archivos sin issues: X

## TOP 10 ISSUES CRÍTICOS (P0+P1)
[lista ordenada por impacto — incluye Firebase rules y Cloud Functions si aplica]

## ISSUES POR SECCIÓN
### Sección X — Nombre
| Archivo | Severidad | Descripción | Fix |
|---------|-----------|-------------|-----|

## FIREBASE RULES STATUS
| Path | Regla actual | ¿Segura? | Fix recomendado |
|------|-------------|----------|-----------------|

## CLOUD FUNCTIONS STATUS
| Función | Propósito | Issues | Fix |
|---------|-----------|--------|-----|

## FIREBASE INTEGRATION STATUS
| Área | Estado | Issues |
|------|--------|--------|

## AUTOLOCALIZER SEMANTIC STATUS
| Keys problemáticas | Descripción | Corrección |
|--------------------|-------------|------------|

## SECURITY FINDINGS
| Severidad | Archivo | Vulnerabilidad | Fix |
|-----------|---------|---------------|-----|

## PLATFORM GUARD ISSUES
| Archivo | Plataforma afectada | Issue | Fix |
|---------|---------------------|-------|-----|

## GDPR COMPLIANCE STATUS
| Área | Compliant | Gap | Fix |
|------|-----------|-----|-----|

## PERFORMANCE RISKS
| Archivo | Issue | Impacto estimado | Fix |
|---------|-------|-----------------|-----|

## ANIMATION CRASH RISKS
| Archivo | Risk | Fix |
|---------|------|-----|

## TEST COVERAGE REPORT
| Área crítica | Tiene tests | Cobertura estimada |
|-------------|-------------|-------------------|
```

---

## ARCHIVOS A IGNORAR (fuera de scope)
- `Assets/Plugins/` — plugins importados externos
- `Assets/GoogleService-Info.plist` — config Firebase (no contiene lógica)
- `Packages/` — Unity packages
- `*.json` — configuración, **EXCEPTO `database.rules.json`** (auditado en Sección 0)
- `*.yaml` — Unity serialized files
- `*.prefab` — Unity prefabs (no editar directamente)
- `*.unity` — Unity scenes (no editar directamente)
- `*.asset` — Unity assets

## ARCHIVOS INCLUIDOS (no obvios)
- `database.rules.json` — **SÍ auditar** (Sección 0) — reglas de seguridad Firebase
- `functions/` — **SÍ auditar** (Sección 0) — Cloud Functions backend (Node.js)
- `Assets/_Project/Scripts/Tests/` — **SÍ auditar** (Sección 33)

---

## NOTAS IMPORTANTES PARA EL AUDITOR
1. **Triumph SDK**: Los archivos `TriumphManager.cs`, `TriumphServices.cs` y `Mock*` se auditan SOLO para crashes/warnings/security — NO para Firebase integration
2. **NUNCA modificar** `.yaml/.prefab/.unity/.asset` directamente — usar UIBuilders via menú DigitPark/
3. **Sincronizar Translations.txt**: si se detectan issues, copiar `Resources/Translations.txt` → `Localization/Translations.txt` Y `Scripts/Localization/Translations.txt`
4. **Meta files**: cualquier asset nuevo necesita su `.meta` correspondiente
5. **Windows paths**: usar `/` en código, `py -3` para Python scripts en este entorno
6. **DOTween**: importado como plugin — no auditar su código interno
7. **Firebase SDK**: importado como plugin — no auditar su código interno
8. **database.rules.json**: es JSON (no .cs) pero es Sección 0 obligatoria — la seguridad de TODA la DB depende de él
9. **functions/**: es Node.js/TypeScript — auditarlo como backend, no como Unity C#. Enfocarse en auth, receipt validation y logging de PII
10. **AutoAssigners & Editor Tools**: usar Glob al inicio de las secciones 31/32/33 para obtener la lista actualizada de archivos — no asumir que la lista es fija
11. **Performance**: en un juego móvil competitivo, un GC spike de 50ms puede perder una partida. Tratar los issues de performance en `Update()` como P1, no P3
