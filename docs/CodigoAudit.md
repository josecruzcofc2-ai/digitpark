# CODIGO AUDIT — DigitPark V54
**Fecha**: 2026-03-18
**Fuente**: SuperAudit completo (CRASH_HUNTER + WARNING_SCANNER + SECURITY_AUDITOR + LOCALIZATION_AUDITOR + PRODUCTION_READINESS) + PENDIENTES_ECONOMIA + AVATAR_REMOVAL_PLAN + PENDIENTES_REALES + TAREAS_MANUALES
**Estado**: 0 items implementados — todo pendiente de sesión de código con Claude

---

## RESUMEN EJECUTIVO

| Prioridad | Cantidad | Descripción |
|---|---|---|
| P0 | 10 | Crashes garantizados + localization P0 + IAP roto + Location hardcoded |
| P1 | 34 | Crashes condicionados, warnings críticos, security, localization, production |
| P2 | 28 | Edge-cases, mejoras de calidad, economy features |
| Bloqueados | 4 | Esperan App Store ID real o Triumph SDK |
| Decisiones pendientes | 3 | Requieren decisión tuya antes de implementar |
| **TOTAL** | **79** | |

---

## P0 — BLOQUEANTES (impiden publicación)

---

### C-P0-01. AudioManager.cs — null AudioSource en LoadAudioSettings
- **Archivo**: `Assets/_Project/Scripts/Runtime/Core/Audio/AudioManager.cs:431,437`
- **Problema**: `musicSource.volume = musicVolume;` y `sfxSource.volume = sfxVolume;` sin null-check. Si los AudioSources no están asignados en el Inspector → NullReferenceException al arrancar cualquier escena con audio.
- **Fix**:
  ```csharp
  if (musicSource != null) musicSource.volume = musicVolume;
  if (sfxSource != null) sfxSource.volume = sfxVolume;
  ```
- **Fuente**: CRASH_HUNTER/CR-01

---

### C-P0-02. GameSessionManager.cs — SceneNavigator.Instance sin ?. en CancelSession
- **Archivo**: `Assets/_Project/Scripts/Runtime/Features/Games/Core/GameSessionManager.cs:466`
- **Problema**: `SceneNavigator.Instance.NavigateTo("MainMenu");` sin null-safe. En iOS al cerrar la app, crash garantizado.
- **Fix**: `SceneNavigator.Instance?.NavigateTo("MainMenu");`
- **Fuente**: CRASH_HUNTER/CR-02

---

### C-P0-03. MinigameBase.cs — SceneNavigator.Instance sin ?. (2 ubicaciones)
- **Archivo**: `Assets/_Project/Scripts/Runtime/Features/Games/Core/MinigameBase.cs:105,516`
- **Problema**: `SceneNavigator.Instance.NavigateTo("GameSelector");` en `OnBackClicked` y `OnPanelAcceptClicked` sin null-safe. Crash al pulsar Back o Accept en cualquiera de los 5 minijuegos.
- **Fix**: `SceneNavigator.Instance?.NavigateTo("GameSelector");` en ambas líneas.
- **Fuente**: CRASH_HUNTER/CR-03

---

### C-P0-04. TrophyShowcaseAnimator.cs — Camera.main sin null-guard
- **Archivo**: `Assets/_Project/Scripts/Runtime/Animations/Animators/TrophyShowcaseAnimator.cs:489`
- **Problema**: `Camera.main.ScreenToWorldPoint(...)` en rama del ternario donde `target == null`. Si no hay cámara con tag "MainCamera" → NullReferenceException.
- **Fix**:
  ```csharp
  Vector3 targetPos = target != null ? target.position :
      (Camera.main != null
          ? Camera.main.ScreenToWorldPoint(new Vector3(Screen.width * 0.9f, Screen.height * 0.95f, 0))
          : Vector3.zero);
  ```
- **Fuente**: CRASH_HUNTER/CR-04

---

### C-P0-05. UIAnimationManager.cs — DOTween.Sequence SetLink en clone destruido
- **Archivo**: `Assets/_Project/Scripts/Runtime/Animations/Core/UIAnimationManager.cs:177`
- **Problema**: `.SetLink(clone)` sobre el clone que se destruye en OnComplete. Si la escena cambia mid-tween → MissingReferenceException. El MonoBehaviour owner tampoco está vinculado.
- **Fix**: Añadir `.SetLink(gameObject)` del MonoBehaviour caller además del clone. Verificar null en el callback de OnComplete antes de operar.
- **Fuente**: CRASH_HUNTER/CR-05

---

### C-P0-06. Translations.txt — "contrasena" sin ñ en 3 claves ES (P0 semántico)
- **Archivo**: `Assets/_Project/Resources/Translations.txt`
- **Problema**: "contrasena"/"Contrasena" en lugar de "contraseña"/"Contraseña" — la ñ es obligatoria. Sin ella, la palabra significa "mujer extraña" en español.
- **Claves afectadas**:
  - `error_password_empty` ES
  - `error_register_password_empty` ES
  - `confirm_password_placeholder` ES
- **Fix**: Reemplazar las 3 ocurrencias.
- **Fuente**: LOCALIZATION_AUDITOR/LOC-11

---

### C-P0-07. ShopItemUI.cs — IAP real-money no implementado (compras fallan silenciosamente)
- **Archivo**: `Assets/_Project/Scripts/Runtime/Features/Monetization/Shop/ShopItemUI.cs:308-313`
- **Problema**: `Debug.LogWarning("[ShopItemUI] IAP not implemented yet")` + `return false`. El usuario toca el botón de compra y no ocurre nada visible. Bloquea la monetización principal.
- **Fix**: Conectar con `PaymentManager.Instance.Purchase(itemData.iapProductId)` → flujo `ProductCatalog` → `AppleIAPBridge`.
- **Fuente**: PRODUCTION_READINESS/PR-02

---

### C-P0-08. LocationRestrictionService.cs — "California" hardcodeado (compliance riesgo legal)
- **Archivo**: `Assets/_Project/Scripts/Runtime/Services/LocationRestrictionService.cs:91-103`
- **Problema**: `SimulateLocationCheck()` siempre asigna `CurrentState = "California"` e `IsRestricted = false`. Usuarios en 18 estados prohibidos acceden a CashBattle sin restricción.
- **Fix**: Si Triumph SDK no disponible: bloquear CashBattle si `!IsLocationKnown` (no asumir California). Implementar geolocalización real o deshabilitar completamente hasta tener el SDK.
- **Fuente**: PRODUCTION_READINESS/PR-03

---

### C-P0-09. BootManager.cs — Application.Quit() en GDPR decline (falla en iOS)
- **Archivo**: `Assets/_Project/Scripts/Runtime/Core/Boot/BootManager.cs:787-790`
- **Problema**: Apple guideline 2.4.5 prohíbe que apps se cierren programáticamente en iOS. Si el usuario declina GDPR → pantalla en negro congelada.
- **Fix**: Reemplazar `Application.Quit()` por mensaje "La app requiere tu consentimiento para funcionar. Por favor, acepta para continuar." y mantener el popup activo. O permitir uso limitado sin analytics.
- **Fuente**: PRODUCTION_READINESS/PR-04

---

### C-P0-10. StarterPack — No implementado (pérdida de ventana de primera compra D1-D3)
- **Archivos**: `ShopPremiumUIBuilder.cs` + crear `StarterPackService.cs`
- **Problema**: `ShopItemType.StarterPack` existe en el enum pero el timer D1-D3 y la lógica de activación no están implementados. La ventana D1-D3 tiene conversión 3-5× mayor que cualquier otro momento.
- **Fix**: Crear `StarterPackService.cs` con timer desde primer login. Oferta: 150 DG + Frame exclusivo + Título "Rookie" por $1.99. Solo visible D1-D3 (72h). Mostrar en `ShopPremiumUIBuilder.cs`.
- **Fuente**: PENDIENTES_ECONOMIA/EC-01
- **Prerequisito manual**: M-41 (ejecutar Build Shop Premium UI tras el código)

---

## P1 — CRÍTICOS PRE-LAUNCH

---

### C-P1-01. UIAnimations.cs — DOTween.Sequence sin SetLink (7 métodos estáticos)
- **Archivo**: `Assets/_Project/Scripts/Runtime/Animations/Core/UIAnimations.cs:42,64,176,189,231,320,331`
- **Problema**: Métodos de utilidad estáticos retornan `Sequence` sin `.SetLink()`. Los callers (ProfileManager, LeaderboardManager, GameSelectorManager) tampoco encadenan SetLink. Si el MonoBehaviour se destruye mid-tween → MissingReferenceException.
- **Fix**: En todos los callers: `.SetLink(gameObject)` al resultado de cada llamada a UIAnimations. Alternativa: añadir parámetro `GameObject owner = null` a los métodos estáticos y aplicar SetLink internamente si != null.
- **Fuente**: CRASH_HUNTER/CR-06

---

### C-P1-02. TournamentsBrowserManager.cs — DOTween.Sequence sin SetLink
- **Archivo**: `Assets/_Project/Scripts/Runtime/Features/Tournaments/TournamentsBrowserManager.cs:372`
- **Problema**: Secuencia de fade-in staggered de torneos sin `.SetLink(gameObject)`. Si el usuario pulsa Back durante la animación → CanvasGroups destruidos → crash.
- **Fix**: `seq.SetLink(gameObject);` después de construir la secuencia.
- **Fuente**: CRASH_HUNTER/CR-07

---

### C-P1-03. DatabaseService.cs — JsonUtility.FromJson con string vacío
- **Archivo**: `Assets/_Project/Scripts/Runtime/Services/Firebase/DatabaseService.cs:95`
- **Problema**: `PlayerPrefs.GetString("SimLeaderboard")` retorna `""` en primer arranque. `JsonUtility.FromJson<T>("")` lanza `ArgumentException` antes del null-check. Crash en primer arranque.
- **Fix**:
  ```csharp
  string json = PlayerPrefs.GetString("SimLeaderboard", "");
  if (string.IsNullOrEmpty(json)) return;
  var wrapper = JsonUtility.FromJson<LeaderboardWrapper>(json);
  ```
- **Fuente**: CRASH_HUNTER/CR-08

---

### C-P1-04. MatchmakingManager.cs — SelectedGames null/vacío en CognitiveSprintMatch
- **Archivo**: `Assets/_Project/Scripts/Runtime/Features/Games/Navigation/MatchmakingManager.cs:440`
- **Problema**: Si `SelectedGames` es null o lista vacía (flujo de deep-link o navegación rota) → `FindCognitiveSprintMatch` recibe null → crash probable en iteración.
- **Fix**: `if (CognitiveSprintManager.Instance?.SelectedGames?.Count > 0)` antes de iniciar matchmaking. Mostrar error al usuario si false.
- **Fuente**: CRASH_HUNTER/CR-09

---

### C-P1-05. OnlineResultManager.cs — Lambda captura `this` después de destrucción de escena
- **Archivo**: `Assets/_Project/Scripts/Runtime/Features/Games/Results/OnlineResultManager.cs:157-176`
- **Problema**: Lambda en `ListenForOpponentResult` captura `this` implícitamente. Si el callback llega con la escena cambiada y el Canvas destruido → `Instantiate` sobre padre nulo → NullReferenceException.
- **Fix**: `if (this == null) return;` al inicio del lambda. Verificar existencia del Canvas padre antes de Instantiate.
- **Fuente**: CRASH_HUNTER/CR-10

---

### C-P1-06. AchievementsManager.cs — DOTween seq sin SetLink en cierre de detail panel
- **Archivo**: `Assets/_Project/Scripts/Runtime/Features/Monetization/Achievements/AchievementsManager.cs:747`
- **Problema**: Secuencia de animación de cierre del panel de detalles de logro sin `.SetLink(gameObject)`. Si el usuario pulsa Back durante el cierre → MissingReferenceException.
- **Fix**: `seq.SetLink(gameObject);` en la construcción de la secuencia de cierre.
- **Fuente**: CRASH_HUNTER/CR-11

---

### C-P1-07. UISparkleEffect.cs — parentCanvas null en Awake
- **Archivo**: `Assets/_Project/Scripts/Runtime/Features/Games/Results/UISparkleEffect.cs:57`
- **Problema**: `GetComponentInParent<Canvas>()` retorna null si el efecto se instancia fuera de un Canvas. Usos posteriores de `parentCanvas` sin null-check → NullReferenceException en efectos de victoria.
- **Fix**: Null-guard en cada uso de `parentCanvas`, o `if (parentCanvas == null) { enabled = false; return; }` en Awake.
- **Fuente**: CRASH_HUNTER/CR-12

---

### C-P1-08. MemoryPairsController.cs — OnCardFlipped nunca se desuscribe
- **Archivo**: `Assets/_Project/Scripts/Runtime/Features/Games/MemoryPairs/MemoryPairsController.cs:207,972`
- **Problema**: `card3DEffects[i].OnCardFlipped += OnCard3DFlipped;` en SetupCardReferences pero OnDestroy (línea 972) nunca hace el `-=`. Al reiniciar el juego se acumulan subscripciones duplicadas. Con 16+ cartas y varios reintentos el handler se ejecuta N veces por flip.
- **Fix**: En OnDestroy:
  ```csharp
  if (card3DEffects != null)
      foreach (var c in card3DEffects)
          if (c != null) c.OnCardFlipped -= OnCard3DFlipped;
  ```
- **Fuente**: WARNING_SCANNER/WS-01

---

### C-P1-09. InAppNotificationManager.cs — OnToastDismissed/OnToastAction sin -=
- **Archivo**: `Assets/_Project/Scripts/Runtime/UI/Notifications/InAppNotificationManager.cs:246-247`
- **Problema**: Suscripciones `_currentToast.OnToastDismissed += ...` y `OnToastAction += ...` nunca tienen `-=` correspondiente. Memory leak del delegado si el toast se destruye externamente.
- **Fix**: Antes de reasignar `_currentToast` o en OnDestroy:
  ```csharp
  if (_currentToast != null) {
      _currentToast.OnToastDismissed -= OnToastDismissed;
      _currentToast.OnToastAction -= OnToastAction;
  }
  ```
- **Fuente**: WARNING_SCANNER/WS-02

---

### C-P1-10. ShopManager.cs — async void ProcessIAPPurchase sin try-catch
- **Archivo**: `Assets/_Project/Scripts/Runtime/Features/Monetization/Shop/ShopManager.cs:423`
- **Problema**: `private async void ProcessIAPPurchase(ShopItemData itemData)` sin try/catch. Es el método crítico de compra IAP con dinero real. Una excepción no capturada puede silenciar el error o crashear.
- **Fix**:
  ```csharp
  private async void ProcessIAPPurchase(ShopItemData itemData)
  {
      try { /* cuerpo existente */ }
      catch (Exception ex) { Debug.LogError($"[ShopManager] IAP error: {ex.Message}"); }
  }
  ```
- **Fuente**: WARNING_SCANNER/WS-03

---

### C-P1-11. GameSessionManager vs OnlineResultManager — Doble pago de recompensas online
- **Archivos**: `GameSessionManager.cs:545,556` y `OnlineResultManager.cs:320,345`
- **Problema**: Ambos managers llaman `currency.AddCoins()` para el mismo evento de victoria online con valores contradictorios (50 DC en GameSessionManager, 15 DC en OnlineResultManager). Posible doble pago.
- **Fix**: Determinar qué flujo es el canonical para partidas Online Ranked y eliminar o suprimir el otro. Documentar explícitamente en comentario qué manager gestiona el pago.
- **Decisión requerida**: ¿GameSessionManager o OnlineResultManager gestiona el pago?
- **Fuente**: WARNING_SCANNER/WS-12

---

### C-P1-12. ShopManager.cs — RegisterShopItem doble-subscribe asimétrico
- **Archivo**: `Assets/_Project/Scripts/Runtime/Features/Monetization/Shop/ShopManager.cs:516`
- **Problema**: Si el mismo item fue removido y re-añadido, `item.OnPurchaseRequested` puede acumular múltiples suscripciones mientras `_shopItems` solo lo tiene una vez. La protección no es simétrica.
- **Fix**: `item.OnPurchaseRequested -= OnItemPurchaseRequested; item.OnPurchaseRequested += OnItemPurchaseRequested;`
- **Fuente**: WARNING_SCANNER/WS-06

---

### C-P1-13. PremiumManager.cs — async void sin try-catch (2 métodos)
- **Archivo**: `Assets/_Project/Scripts/Runtime/Features/Monetization/Premium/PremiumManager.cs:255,286`
- **Métodos**: `SyncPremiumToFirebase()` y `RestoreFromFirebaseAsync()`
- **Problema**: Tocan estado de suscripción premium (datos de pago). Si Firebase está offline o el token expiró → error silencioso o crash.
- **Fix**: Envolver ambos métodos en `try { ... } catch (Exception ex) { Debug.LogError(...); }`.
- **Fuente**: WARNING_SCANNER/WS-07

---

### C-P1-14. OnlineResultPanelController + CashBattleResultPanelController — onClick sin RemoveAllListeners
- **Archivos**: `OnlineResultPanelController.cs:108,111` y `CashBattleResultPanelController.cs:93,95`
- **Problema**: `continueButton.onClick.AddListener(...)` y `rematchButton.onClick.AddListener(...)` sin RemoveAllListeners en OnDestroy. Si SetupButtons() se llama más de una vez → listeners acumulados → eventos se disparan N veces.
- **Fix**: En OnDestroy de ambas clases:
  ```csharp
  continueButton?.onClick.RemoveAllListeners();
  rematchButton?.onClick.RemoveAllListeners(); // o newMatchButton
  ```
- **Fuente**: WARNING_SCANNER/WS-10

---

### C-P1-15. ServiceLocator.cs — Guard Mock no activa en DEVELOPMENT_BUILD
- **Archivo**: `Assets/_Project/Scripts/Runtime/Services/ServiceLocator.cs:143-148`
- **Problema**: El guard `#if !UNITY_EDITOR && !DEVELOPMENT_BUILD` permite que builds de QA (TestFlight/Internal Track) corran con `ServiceMode.Mock`. Cuando Triumph SDK se integre, esto activaría el wallet real en builds QA.
- **Fix**: Cambiar a:
  ```csharp
  #if !UNITY_EDITOR
      if (_serviceMode == ServiceMode.Mock) { _serviceMode = ServiceMode.Production; }
  #endif
  ```
- **Fuente**: SECURITY_AUDITOR/SEC-P1-01

---

### C-P1-16. ConsentService.cs — Usar SecurePrefs en lugar de PlayerPrefs
- **Archivo**: `Assets/_Project/Scripts/Runtime/Services/ConsentService.cs:11-26`
- **Problema**: `DP_ConsentGiven` en PlayerPrefs sin cifrar. En Android (root) se puede escribir `DP_ConsentGiven=1` y saltarse el popup GDPR → Firebase Analytics se activa sin consentimiento → violación Art. 7 GDPR.
- **Fix**:
  ```csharp
  public static bool HasConsent() => SecurePrefs.GetInt(CONSENT_KEY, 0) == 1;
  public static void Accept()   { SecurePrefs.SetInt(CONSENT_KEY, 1); }
  public static void Decline()  { SecurePrefs.SetInt(CONSENT_KEY, 0); }
  ```
- **Nota**: `SecurePrefs.cs` ya existe en el proyecto.
- **Fuente**: SECURITY_AUDITOR/SEC-P1-03

---

### C-P1-17. Translations.txt — Acentos ES faltantes (7 claves de alta frecuencia)
- **Archivo**: `Assets/_Project/Resources/Translations.txt`
- **Fixes**:
  | Clave | Actual | Correcto |
  |---|---|---|
  | `logout_button` | "Cerrar Sesion" | "Cerrar Sesión" |
  | `duration` | "Duracion" | "Duración" |
  | `position` | "Posicion" | "Posición" |
  | `country_tab` | "Pais" | "País" |
  | `leaderboard_header` | "CLASIFICACION" | "CLASIFICACIÓN" |
  | `later_button` | "Despues" | "Después" |
  | `net_restored` | "Conexion restaurada" | "Conexión restaurada" |
  | `public` | "Publico" | "Público" |
- **Fuente**: LOCALIZATION_AUDITOR/LOC-09, LOC-10

---

### C-P1-18. Translations.txt — Claves duplicadas `free` y `purchased`
- **Archivo**: `Assets/_Project/Resources/Translations.txt`
- **Problema**: `free` aparece en líneas 6507 y 9695. `purchased` aparece en líneas 6514 y 9702. La segunda definición puede silenciosamente sobreescribir la primera.
- **Fix**: Eliminar las entradas duplicadas. Conservar la versión con traducciones más completas en los 5 idiomas.
- **Fuente**: LOCALIZATION_AUDITOR/LOC-02

---

### C-P1-19. Translations.txt — Acentos FR faltantes en UI de alta frecuencia (5 claves)
- **Archivo**: `Assets/_Project/Resources/Translations.txt`
- **Fixes**:
  | Clave | Actual | Correcto |
  |---|---|---|
  | `settings_button` | "Parametres" | "Paramètres" |
  | `settings_title` | "Parametres" | "Paramètres" |
  | `register_button` | "Creer" | "Créer" |
  | `create_tab` | "Creer" | "Créer" |
  | `create_tournament` | "Creer un tournoi" | "Créer un tournoi" |
- **Fuente**: LOCALIZATION_AUDITOR/LOC-14

---

### C-P1-20. Translations.txt — `you_won` FR sin acento (pantalla más visible de la app)
- **Archivo**: `Assets/_Project/Resources/Translations.txt`
- **Clave**: `you_won` FR: "VOUS AVEZ GAGNE!" → "VOUS AVEZ GAGNÉ!"
- **Fuente**: LOCALIZATION_AUDITOR/LOC-16

---

### C-P1-21. Translations.txt — Diacríticos PT faltantes (6 claves de alta frecuencia)
- **Archivo**: `Assets/_Project/Resources/Translations.txt`
- **Fixes**:
  | Clave | Actual | Correcto |
  |---|---|---|
  | `settings_button` | "Configuracoes" | "Configurações" |
  | `settings_title` | "Configuracoes" | "Configurações" |
  | `duration` | "Duracao" | "Duração" |
  | `country_tab` | "Pais" | "País" |
  | `msg_unstoppable_force` | "FORCA IMPARAVEL!" | "FORÇA IMPARÁVEL!" |
  | `friends_no_friends` | "adiciona-los" | "adicioná-los" |
- **Nota**: El problema es sistémico — probablemente 50-100+ claves PT afectadas por pipeline ASCII. Se necesita revisión con native speaker (M-49).
- **Fuente**: LOCALIZATION_AUDITOR/LOC-18

---

### C-P1-22. Translations.txt — Umlauts/ß DE faltantes (7 claves)
- **Archivo**: `Assets/_Project/Resources/Translations.txt`
- **Fixes**:
  | Clave | Actual | Correcto |
  |---|---|---|
  | `delete_confirm_message` | "ruckgangig machen" | "rückgängig machen" |
  | `msg_exceptional_reflexes` | "AUSSERGEWOHNLICHE REFLEXE" | "AUSSERGEWÖHNLICHE REFLEXE" |
  | `msg_legendary_speed` | "LEGENDARE GESCHWINDIGKEIT" | "LEGENDÄRE GESCHWINDIGKEIT" |
  | `msg_good_effort` | "Gute Muhe" | "Gute Mühe" |
  | `msg_next_will_be_better` | "nachste Mal" | "nächste Mal" |
  | `close` | "Schliessen" | "Schließen" |
  | `clear` | "Loschen" | "Löschen" |
- **Nota**: Probable que haya más casos no detectados — revisión con native speaker (M-50).
- **Fuente**: LOCALIZATION_AUDITOR/LOC-20

---

### C-P1-23. AutoLocalizer.cs — CelebrationAchievementName siempre muestra "First Step"
- **Archivo**: `Assets/_Project/Scripts/Runtime/Localization/AutoLocalizer.cs`
- **Problema**: `TextNameToKeyMap` tiene `{ "CelebrationAchievementName", "ach_first_game" }`. El GO de celebración siempre muestra el nombre del primer logro sin importar qué logro se desbloqueó.
- **Fix**: Eliminar esta entrada del TextNameToKeyMap. En `AchievementService` (o donde se dispara la celebración): `celebrationAchievementName.text = AutoLocalizer.Get(achievement.nameKey);`
- **Fuente**: LOCALIZATION_AUDITOR/LOC-23

---

### C-P1-24. DailyOfferService.cs — Cobra moneda pero NO otorga el item (bug grave)
- **Archivo**: `Assets/_Project/Scripts/Runtime/Services/DailyOfferService.cs:412-418`
- **Problema**: `PurchaseOffer` deduce DG/DC del balance pero el item nunca se otorga. Solo loguea el purchase y marca `offer.purchased = true`. El jugador pierde moneda real sin recibir nada.
- **Fix**: Implementar `GrantDailyOfferReward(offer)` usando:
  - Tema: `ThemeManager.UnlockTheme(offer.itemId)`
  - Frame: `PlayerFrameService.UnlockFrame(offer.itemId)`
  - Efecto: `VictoryEffectService.UnlockEffect(offer.itemId)`
  - Título: `PlayerTitleService.UnlockTitle(offer.itemId)`
  - Card: `BattleCardService.UnlockCard(offer.itemId)`
- **Fuente**: PRODUCTION_READINESS/PR-05

---

### C-P1-25. PremiumCard.cs + StylesProPromptPanel.cs — Precios hardcodeados en fallbacks
- **Archivos**: `PremiumCard.cs:138-159`, `StylesProPromptPanel.cs:587,661`
- **Problema**: Fallbacks con `"$14.99"`, `"$2.99"`, etc. Viola App Store Review 3.1.1 — en países no-USA se muestra precio en USD que no coincide con lo que cobra la tienda local.
- **Fix**: Cambiar fallbacks a `string.Empty` o `AutoLocalizer.Get("price_unavailable")`. Solo usar precios de `PremiumManager.Instance?.GetProductPrice(...)`.
- **Fuente**: PRODUCTION_READINESS/PR-06

---

### C-P1-26. BootManager.cs — Screen.sleepTimeout = NeverSleep nunca se restaura
- **Archivo**: `Assets/_Project/Scripts/Runtime/Core/Boot/BootManager.cs:118`
- **Problema**: `Screen.sleepTimeout = SleepTimeout.NeverSleep` activado en boot y nunca restaurado. La batería se consume mientras el usuario navega menús o en lobby.
- **Fix**: Eliminar de Boot. Añadir en `MinigameBase.OnGameStart()`: `Screen.sleepTimeout = SleepTimeout.NeverSleep;`. Añadir en `MinigameBase.OnGameEnd()`: `Screen.sleepTimeout = SleepTimeout.SystemSetting;`
- **Fuente**: PRODUCTION_READINESS/PR-07

---

### C-P1-27. MainMenuManager.cs — Añadir check RemoteConfig para ocultar botón CashBattle
- **Archivo**: `Assets/_Project/Scripts/Runtime/Features/MainMenu/MainMenuManager.cs`
- **Problema**: El botón CashBattle es visible aunque el servicio sea no-funcional. Apple puede rechazar la app por "features incompletas".
- **Fix**: En Awake/Start: `bool cashEnabled = RemoteConfigService.GetBool("cash_battle_enabled", false); cashBattleButton?.gameObject.SetActive(cashEnabled);`
- **Prerequisito manual**: M-44 (crear parámetro en Firebase Remote Config Console).
- **Fuente**: PRODUCTION_READINESS/PR-08 + PENDIENTES_NUEVOS/CÓDIGO-19

---

### C-P1-28. Translations.txt — Error EN: "logout" (sustantivo, no verbo)
- **Archivo**: `Assets/_Project/Resources/Translations.txt`
- **Clave**: `logout_confirm_title` EN: "Are you sure you want to logout?" → "Are you sure you want to log out?"
- **Fuente**: LOCALIZATION_AUDITOR/LOC-07

---

### C-P1-29. Loot Box Odds Disclosure obligatorio (App Store guideline 3.1.1b)
- **Estado**: ✅ N/A — No existen loot boxes en el codebase. `PlayerProgressionSystem:403-406` es sync Firebase (`gamesWon`), no recompensas aleatorias. `DailyOfferService` usa selección determinista por seed de fecha UTC. Si se añaden loot boxes en el futuro, añadir modal de disclosure antes de la compra.
- **Fuente**: PENDIENTES_ECONOMIA/EC-02 (C-73)

---

### C-P1-30. PremiumManager.cs + WelcomePackService.cs — IAP precios hardcodeados en USD
- **Archivos**: `PremiumManager.cs`, `WelcomePackService.cs`
- **Problema**: Precios hardcodeados ("$3.99", "$7.99") en lugar de `product.metadata.localizedPriceString`. En EU/UK/MX el usuario ve precios en USD que no coinciden con lo que se le cobra.
- **Fix**: Reemplazar `string price = "$3.99";` por `string price = product.metadata.localizedPriceString;` en cada ocurrencia.
- **Prerequisito**: M-06 (IAP configurado en App Store + Google Play).
- **Fuente**: PENDIENTES_ECONOMIA/EC-03 (C-54)

---

### C-P1-31. BackgroundPatternManager.cs + BattleCardService.cs — async void sin try-catch
- **Archivos**: `BackgroundPatternManager.cs:246` — `SyncToFirebase`, `BattleCardService.cs:215` — `SyncToFirebase`
- **Problema**: Métodos async void que tocan Firebase sin try/catch. Si Firebase está offline → error silencioso.
- **Fix**: Envolver cada uno en `try { ... } catch (Exception ex) { Debug.LogError(...); }`.
- **Fuente**: WARNING_SCANNER/WS-07 (extended)

---

### C-P1-32. NotificationService.cs — UnityMainThreadDispatcher sin ?. en FCM callbacks
- **Archivo**: `Assets/_Project/Scripts/Runtime/Services/Firebase/NotificationService.cs:303,322`
- **Problema**: `UnityMainThreadDispatcher.Instance().Enqueue(...)` sin `?.`. Si el dispatcher fue destruido antes (orden de destrucción), crash no-catchable desde Unity en hilo de red Firebase — más probable en iOS al cerrar la app con notificaciones pendientes.
- **Fix**: `UnityMainThreadDispatcher.Instance()?.Enqueue(() => { ... });`
- **Fuente**: CRASH_HUNTER/CR-16

---

### C-P1-33. BetSelectionPanel.cs — GetComponent<Image>() sin null-check (3 botones)
- **Archivo**: `Assets/_Project/Scripts/Runtime/Features/Games/Navigation/BetSelectionPanel.cs:426-428`
- **Problema**: `if (_rounds1Button != null) _rounds1Button.GetComponent<Image>().color = ...;` — el null-check verifica el Button pero no la Image resultante.
- **Fix**: `_rounds1Button?.GetComponent<Image>()?.color = ...;` (×3 para rounds1/3/5)
- **Fuente**: CRASH_HUNTER/CR-14

---

### C-P1-35. AuthenticationService.cs — Contraseña mínimo 8 caracteres (cuentas con dinero real)
- **Archivos**: `Assets/_Project/Scripts/Runtime/Services/Firebase/AuthenticationService.cs` (registro) + `Assets/_Project/Resources/Translations.txt` + `Assets/_Project/Localization/Translations.txt`
- **Problema**: Firebase acepta contraseñas de 6 caracteres por defecto. Las cuentas CashBattle tienen dinero real — 6 caracteres es insuficiente. Translations.txt ES ya dice "mínimo 8 caracteres" pero el código no lo valida.
- **Fix**:
  1. En `RegisterUser()` (o equivalente): añadir validación `if (password.Length < 8)` antes de llamar a Firebase, mostrar `error_password_too_short`.
  2. Actualizar `auth_error_weak_password` EN: "Password is too weak (minimum 6 characters)" → "Password is too weak (minimum 8 characters)".
  3. Añadir clave nueva `error_password_too_short` en Translations.txt (5 idiomas).
- **Fuente**: Sesión V54 — cuentas CashBattle con dinero real

---

### C-P1-34. DebugManager.cs — PlayerPrefs.DeleteAll accesible en DEVELOPMENT_BUILD en device
- **Archivo**: `Assets/_Project/Scripts/Runtime/DevTools/DebugManager.cs:183-189`
- **Problema**: El guard interno usa `Application.isEditor` que es `false` en device. En Development Build físico, el botón no tiene guardia efectiva — ejecuta `PlayerPrefs.DeleteAll()` sin protección.
- **Fix**: Cambiar `if (Application.isEditor)` a `if (Debug.isDebugBuild)`.
- **Fuente**: PRODUCTION_READINESS/PR-09

---

## P2 — MEJORAS POST-LAUNCH

---

### C-P2-01. MainMenuManager.cs — DOFade sin SetLink en fade de entrada
- **Archivo**: `Assets/_Project/Scripts/Runtime/Features/MainMenu/MainMenuManager.cs:92`
- **Problema**: `cg.DOFade(1f, 0.4f)` en Start() sin `.SetLink(gameObject)`. Si la escena se descarga inmediatamente (sesión expirada + redirect) → MissingReferenceException.
- **Fix**: `cg.DOFade(1f, 0.4f).SetEase(Ease.OutQuad).SetLink(gameObject);`
- **Fuente**: CRASH_HUNTER/CR-18

---

### C-P2-02. CashBattle1v1Manager.cs — GetComponent<Image>() sin null-check
- **Archivo**: `Assets/_Project/Scripts/Runtime/Features/CashBattle/Hub/CashBattle1v1Manager.cs:692-694`
- **Problema**: `rounds1Button.GetComponent<Image>().color = ...` — si el prefab no tiene Image → NullReferenceException.
- **Fix**: `rounds1Button?.GetComponent<Image>()?.color = ...;` (×3)
- **Fuente**: CRASH_HUNTER/CR-13

---

### C-P2-03. CurrencyDisplayUI.cs — Magic number 0.3f ignorando campo serializado
- **Archivo**: `Assets/_Project/Scripts/Runtime/Features/Monetization/Currency/CurrencyDisplayUI.cs:283`
- **Problema**: Línea 283 usa `float duration = 0.3f;` hardcodeado ignorando `[SerializeField] _animationDuration`. Si el diseñador cambia el campo en Inspector, la animación de shake permanece en 0.3s.
- **Fix**: `float duration = _animationDuration;`
- **Fuente**: WARNING_SCANNER/WS-13

---

### C-P2-04. CashProfileAnimator.cs — FindObjectOfType en animación path (performance)
- **Archivo**: `Assets/_Project/Scripts/Runtime/Animations/Animators/CashProfileAnimator.cs:183,510`
- **Problema**: `FindObjectOfType<Canvas>()` dentro de métodos llamados durante animaciones, y `FindObjectOfType<HistoryManager>()` sin caché. O(n) sobre todos los objetos en cada frame.
- **Fix**: Cachear referencias en Awake. Para HistoryManager: campo privado `_historyManager`, asignar en Awake, solo buscar si es null.
- **Fuente**: WARNING_SCANNER/WS-08

---

### C-P2-05. 36+ FindObjectOfType en Runtime — deprecation Unity 6
- **Archivos**: LocalizationManager.cs, BootManager.cs, WalletManager.cs, CurrencyManager.cs, PremiumManager.cs, OnlineResultManager.cs, RemoteConfigService.cs, PaymentManager.cs, TriumphIsolationGuard.cs, EntitlementService.cs, FriendService.cs, TriumphManager.cs, ServiceLocator.cs, UICanvasHelper.cs (ver WARNING_SCANNER/WS-04 para lista completa con líneas)
- **Problema**: `FindObjectOfType<T>()` está deprecado en Unity 6 (CS0618 en modo strict).
- **Fix**: Reemplazar con `Object.FindFirstObjectByType<T>()` en Unity 2022.2+.
- **Fuente**: WARNING_SCANNER/WS-04

---

### C-P2-06. Economy — Magic numbers sin constantes nombradas
- **Archivos**: `GameSessionManager.cs:529,538,545,548`, `OnlineResultManager.cs:320,326,337`, `MinigameBase.cs:326,368`
- **Problema**: Valores de recompensa hardcodeados (30, 15, 50, 100, 25, 5, 75) sin constantes nombradas en múltiples archivos.
- **Fix**: Crear `Assets/_Project/Scripts/Runtime/Economy/EconomyConstants.cs`:
  ```csharp
  public static class EconomyConstants {
      public const int PracticeBaseReward = 30;
      public const int PersonalBestBonus = 15;
      public const int OnlineWinReward = 50;
      public const int OnlineLossReward = 15;
      public const int TournamentWinReward = 100;
      public const int TournamentLossReward = 25;
      public const int RankedWinDC = 15;
      public const int RankedLossDC = 5;
      public const int PerfectBonusDC = 25;
      public const int FWOTDBonusDC = 50;
      public const int DefaultMaxAttempts = 3;
  }
  ```
- **Fuente**: WARNING_SCANNER/WS-05

---

### C-P2-07. Translations.txt — Acentos FR en mensajes de feedback de juego (4 claves)
- **Archivo**: `Assets/_Project/Resources/Translations.txt`
- **Fixes**:
  | Clave | Actual | Correcto |
  |---|---|---|
  | `msg_exceptional_reflexes` | "REFLEXES EXCEPTIONNELS" | "RÉFLEXES EXCEPTIONNELS" |
  | `msg_impressive_reflexes` | "REFLEXES IMPRESSIONNANTS" | "RÉFLEXES IMPRESSIONNANTS" |
  | `msg_good_reflexes` | "bons reflexes" | "bons réflexes" |
  | `msg_pure_genius` | "GENIE PUR" | "GÉNIE PUR" |
- **Fuente**: LOCALIZATION_AUDITOR/LOC-15

---

### C-P2-08. Translations.txt — Claves duplicadas de achievement (ach_daily_7, ach_daily_30)
- **Archivo**: `Assets/_Project/Resources/Translations.txt`
- **Problema**: `ach_daily_7` y `ach_daily_streak_7` tienen contenido idéntico. Ídem para `ach_daily_30` y `ach_daily_streak_30`.
- **Fix**: Verificar en `AchievementService.cs` cuál clave se usa actualmente. Eliminar la no usada.
- **Fuente**: LOCALIZATION_AUDITOR/LOC-05

---

### C-P2-09. Translations.txt — Claves duplicadas `bet_title` y `choose_your_bet`
- **Archivo**: `Assets/_Project/Resources/Translations.txt`
- **Problema**: Ambas claves se traducen a "Choose Your Bet" en los 5 idiomas — contenido idéntico.
- **Fix**: Usar `choose_your_bet` como clave canónica. Reemplazar todos los usos de `bet_title` y eliminarla.
- **Fuente**: LOCALIZATION_AUDITOR/LOC-06

---

### C-P2-10. Translations.txt — Clave `chat_badge` no es traducible (valor "0")
- **Archivo**: `Assets/_Project/Resources/Translations.txt` + `AutoLocalizer.cs`
- **Problema**: `chat_badge` mapea al literal "0" en los 5 idiomas — no es texto traducible, es un contador.
- **Fix**: Eliminar de Translations.txt. Eliminar `{ "ChatBadge", "chat_badge" }` de TextNameToKeyMap. Inicializar el badge a `"0"` en código directamente.
- **Fuente**: LOCALIZATION_AUDITOR/LOC-04

---

### C-P2-11. AutoLocalizer.cs — Fuzzy-match sin logging de diagnóstico
- **Archivo**: `Assets/_Project/Scripts/Runtime/Localization/AutoLocalizer.cs`
- **Problema**: El fuzzy match (strip "Text"/"Label"/"Button" + 80% similaridad) puede producir falsos positivos silenciosamente. Sin logging no se puede debuggear.
- **Fix**: `Debug.LogWarning($"[AutoLocalizer] Fuzzy match: '{goName}' → '{key}'");` cuando se aplica un match no-exacto.
- **Fuente**: LOCALIZATION_AUDITOR/LOC-26

---

### C-P2-12. Translations.txt — FR register tu/vous inconsistente (~8-12 claves)
- **Archivo**: `Assets/_Project/Resources/Translations.txt`
- **Problema**: Mezcla de "tu" informal y "vous" formal en la misma app (ej: `msg_on_fire` usa "tu", `you_won` usa "vous").
- **Fix**: Estandarizar a **vous** en toda la app (más apropiado para mercado FR/BE/CA). Actualizar todas las ocurrencias de "tu/te/ton/ta/tes/toi" en valores FR.
- **Pendiente decisión**: Ver M-17 (decidir política FR tu/vous).
- **Fuente**: LOCALIZATION_AUDITOR/LOC-13

---

### C-P2-13. Translations.txt — Correcciones batch adicionales (LOC-03, LOC-44)
- **Archivo**: `Assets/_Project/Resources/Translations.txt` + `AutoLocalizer.cs`
- **Fixes**:
  - Renombrar 5 claves PascalCase a snake_case: `MessageText`, `TournamentInfoText`, `ExitConfirmTitleText`, `ConfirmExitButton`, `CancelExitButton` → equivalentes snake_case. Actualizar TextNameToKeyMap.
  - Eliminar `bet_title` (duplicado de `choose_your_bet`)
- **Fuente**: LOCALIZATION_AUDITOR/LOC-03

---

### C-P2-14. CashTournamentLobbyManager.cs — Eliminar simulación de player count
- **Archivo**: `Assets/_Project/Scripts/Runtime/Features/CashBattle/Tournaments/CashTournamentLobbyManager.cs:576-581`
- **Problema**: `UnityEngine.Random.Range(-1, 3)` en `AutoRefreshCoroutine` para simular cambios en el contador. En un torneo de dinero real, el contador debe ser real.
- **Fix**: Eliminar el Random.Range. Mostrar solo el valor real del último refresh o 0.
- **Fuente**: PRODUCTION_READINESS/PR-15

---

### C-P2-15. MainMenuManager.cs — Eliminar TODO comments residuales
- **Archivo**: `Assets/_Project/Scripts/Runtime/Features/MainMenu/MainMenuManager.cs:420,433`
- **Problema**: `// TODO: Abrir panel de perfil` y `// TODO: Buscar jugadores` presentes pero las acciones ya están implementadas debajo.
- **Fix**: Eliminar los 2 comentarios TODO obsoletos.
- **Fuente**: PRODUCTION_READINESS/PR-13

---

### C-P2-16. DC Sink — Añadir items temporales para evitar inflación DC
- **Archivos**: `ShopItemData.cs` (enum ShopItemType), `ShopManager.cs`
- **Problema**: Un jugador activo acumula ~8,500 DC/mes. Después de 2 meses, tiene todo el contenido DC. Los DC se acumulan indefinidamente → hyperinflación → misiones diarias pierden valor percibido.
- **Fix**: Añadir `ShopItemType.TemporaryDecoration` al enum. Implementar lógica de expiración (30/7 días). Crear 3 items en ShopManager. Manual: M-42 (ScriptableObjects).
- **Fuente**: PENDIENTES_ECONOMIA/EC-04

---

### C-P2-17. WishlistService.cs — Sistema de Wishlist para FOMO en Exclusive Themes
- **Archivos**: Crear `WishlistService.cs` + modificar `DailyOffersManager.cs` + `ShopItemUI.cs`
- **Problema**: Los 5 temas Exclusive solo aparecen en Daily Offers rotativos pero el jugador no sabe cuándo → sin FOMO → baja conversión. Con wishlist + notificación local → conversión puede subir de ~2-3% a ~15-25%.
- **Fix**: Botón "♡ Wishlist" en cada item. Al cargar Daily Offers: verificar coincidencia con wishlist. Si coincide: local notification a las 10:00 AM + badge en icono de tienda.
- **Fuente**: PENDIENTES_ECONOMIA/EC-05
- **Nota**: `WishlistService.cs` ya existe como archivo en el proyecto.

---

### C-P2-18. Daily Rewards — Reordenar patrón a progresión monotónica
- **Archivo**: `Assets/_Project/Scripts/Runtime/Features/Monetization/DailyRewards/DailyRewardsManager.cs`
- **Problema actual**: Día 3 (200 DC) > Día 4 (100 DC) — regresión psicológica. Los sistemas de recompensa son más efectivos cuando son monotónicamente crecientes.
- **Fix**: Cambiar el array de recompensas a: [50, 75, 100, 125, 150, 200, 500 DC + 3 DG]
- **Fuente**: PENDIENTES_ECONOMIA/EC-07

---

### C-P2-19. GDPR Right-to-Delete — Borrado incompleto en DeleteAccount
- **Archivo**: `Assets/_Project/Scripts/Runtime/Services/Firebase/AuthenticationService.cs:438-505`
- **Problema**: El flujo de eliminar cuenta no borra: Analytics events, Match History, Notifications, Achievements, Tournament records, Friends list references.
- **Fix**: En `DeleteAccount()`: llamar endpoints Firebase para borrar cada tipo de dato: `/matchHistory/$uid`, `/achievements/$uid`, `/notifications/$uid`, `/tournamentHistory/$uid`, `/friends/$uid`.
- **Fuente**: TAREAS_MANUALES/task-22

---

### C-P2-20. Legal URLs — Verificar que dominios de Terms/Privacy son del proyecto
- **Archivos**: `SettingsManager.cs:88-91`, `AgeVerificationManager.cs:35-36`
- **Problema**: Hardcoded `https://docs.triumpharcade.com/terms-of-use` y `https://digitpark.com/terms`. Si estos dominios no son del proyecto o caducan → links rotos.
- **Fix**: Hacer las URLs configurables vía Firebase Remote Config (parámetros `terms_url` y `privacy_url`). Fallback a las URLs hardcodeadas actuales.
- **Fuente**: TAREAS_MANUALES/task-25

---

### C-P2-21. Server-Side Time Validation — Daily rewards manipulables via reloj del dispositivo
- **Archivos**: `DailyRewardsManager.cs`, `DailyMissionsManager.cs`, `DailyOfferService.cs`
- **Problema**: Usan `DateTime.UtcNow` local — el jugador puede adelantar el reloj para reclamar recompensas múltiples veces. Ya se cambió a UtcNow (consistente), pero sigue siendo manipulable.
- **Fix**: Implementar Firebase Cloud Function que retorne `serverTimestamp` y usarlo para validar claims de daily rewards, resets de misiones y cooldown de streak shield (14 días).
- **Fuente**: TAREAS_MANUALES/task-32

---

### C-P2-22. RotatingContentService.cs — Activar catálogo cuando items estén listos
- **Archivo**: `Assets/_Project/Scripts/Runtime/Services/RotatingContentService.cs:107-151`
- **Problema**: Todo el catálogo está comentado intencionalmente. Pendiente de activar post-launch.
- **Fix cuando esté listo**: Descomentar las entradas del catálogo. Verificar que los items referenciados (frames, themes, battle cards) existen. Verificar que `BattleCardService.UnlockCard()` existe para grants de tipo `SeasonalBattleCard`.
- **Fuente**: PENDIENTES_ECONOMIA/EC-09

---

### C-P2-23. ShopItemData.cs — Compra de Avatar no hace nada
- **Archivo**: `Assets/_Project/Scripts/Runtime/Features/Monetization/Shop/ShopItemData.cs:262`
- **Estado**: Muestra "feature_coming_soon". Pendiente decisión del usuario.
- **Decisión requerida**: ¿Implementar `ProfileManager.Instance?.SetAvatar(itemId)` o mantener "coming soon" hasta tener sistema de avatares completo?
- **Nota**: Con AVATAR_REMOVAL_PLAN en curso, este punto puede quedar obsoleto si se elimina el sistema de avatares.
- **Fuente**: PENDIENTES_REALES/F-01

---

## AVATAR REMOVAL PLAN (13 escenas — implementar 1 por sesión)

> Plan completo en docs/AVATAR_REMOVAL_PLAN.md (mientras exista). Orden recomendado:

| # | Escena | Cambio | Complejidad |
|---|---|---|---|
| 1 | Onboarding | Eliminar Slide 3 (Avatar Selection), renumerar slides 4→3, 5→4 | Baja |
| 2 | Tournament Lobby | Eliminar icono persona junto a "You" | Baja |
| 3 | Friend Requests | Eliminar AvatarFrame+Mask+Image, expandir InfoSection +100px | Baja |
| 4 | Friends | Eliminar AvatarFrame, añadir "Lv. X" en status text | Baja |
| 5 | Search Players | Eliminar AvatarFrame, añadir nivel, consistencia con Friends | Baja |
| 6 | Rankings/Scores | Eliminar AvatarFrame, expandir UsernameText +44px | Baja |
| 7 | Matchmaking Games | Reemplazar Avatar por ColorBar 8px (CYAN player, GRAY opponent) | Media |
| 8 | Matchmaking CashBattle | Ídem pero ColorBar GOLD player | Media |
| 9 | Online Result | Reemplazar Avatar por ColorBar pattern | Media |
| 10 | CashBattle Result | Ídem con paleta gold | Media |
| 11 | MainMenu | Crear ProfileBanner compacto (68px): Username + LevelBadge + XP bar | Media |
| 12 | CashBattle Profile | Crear CashProfileHeaderCard (100px): Username GOLD + Rank badge + earnings bar | Alta |
| 13 | Profile | Crear ProfileHeaderCard (96px): Username + Lv + XP bar + subtitle | Alta |

**Archivos a eliminar completamente tras implementar todo**:
- `AvatarUI.cs`
- `AvatarService.cs`
- `AvatarOptionItemUI.cs`
- `AvatarInitialGenerator.cs`
- `ShopItemType.Avatar` del enum (verificar primero en Inspector que ningún ScriptableObject usa ese tipo)

**Regla**: Sin YAML directo — todos los cambios via UIBuilder. 1 escena por sesión.

---

## ICONS PENDING (19 iconos a crear en DALL-E/Figma)

> Fuente: docs/ICONS_PENDING.md

### UI Generales (4)
| Icono | Color | Ubicación destino |
|---|---|---|
| `warning.png` | Blanco | `Art/Icons/UI/` |
| `icon_lock_gold.png` | Dorado #FFD700 | `Art/Icons/UI/` + `Resources/UI/Icons/` |
| `icon_lock_silver.png` | Gris #AAAAAA | `Art/Icons/UI/` + `Resources/UI/Icons/` |
| `StarRecommended.png` | Dorado #FFD700 | `Art/Icons/UI/` + `Resources/Icons/UI/` |

### CashBattle Stats (7)
| Icono | Color | Ubicación |
|---|---|---|
| `stat_earnings.png` | Dorado #FFD700 | `Art/Icons/CashBattle/Stats/` + `Resources/Icons/` |
| `stat_defeats.png` | Blanco | `Art/Icons/CashBattle/Stats/` |
| `stat_draws.png` | Blanco | `Art/Icons/CashBattle/Stats/` |
| `stat_total.png` | Blanco | `Art/Icons/CashBattle/Stats/` |
| `stat_tourneysplayed.png` | Blanco | `Art/Icons/CashBattle/Stats/` |
| `stat_victories.png` | Dorado #FFD700 | `Art/Icons/CashBattle/Stats/` |
| `stat_winrate.png` | Blanco | `Art/Icons/CashBattle/Stats/` |

### CashBattle Hub (2)
| Icono | Color | Ubicación |
|---|---|---|
| `Battles1v1Icon.png` | Blanco | `Art/Icons/CashBattle/Hub/` |
| `CashProfileIcon.png` | Blanco | `Art/Icons/CashBattle/Hub/` |

### CashBattle Tournaments (1)
| Icono | Color | Ubicación |
|---|---|---|
| `TrophyPrizeIcon.png` | Dorado #FFD700 | `Art/Icons/CashBattle/Tournaments/` |

### CashBattle UI (3)
| Icono | Color | Ubicación |
|---|---|---|
| `CashBattleIcon.png` | Blanco | `Art/Icons/CashBattle/UI/` + `Art/Icons/Onboarding/` |
| `icon_cash.png` | Blanco | `Art/Icons/CashBattle/UI/` |
| `VerificationIcon.png` | Blanco | `Art/Icons/CashBattle/UI/` |

### CashBattle Wallet (2)
| Icono | Color | Ubicación |
|---|---|---|
| `DepositIcon.png` | Blanco | `Art/Icons/CashBattle/Wallet/` |
| `WithdrawIcon.png` | Blanco | `Art/Icons/CashBattle/Wallet/` |

**Prompt base para DALL-E**: Usar "UI glyph symbol" (NO "app icon"). Especificar 2 veces "transparent background, do NOT generate fake transparency checkerboard pattern".

---

## PENDIENTES BLOQUEADOS (esperan dependencias externas)

### BLOQ-01. ReviewService.cs — App Store ID placeholder
- **Archivo**: `Assets/_Project/Scripts/Runtime/Services/ReviewService.cs:226`
- **Bloqueado hasta**: completar M-02 (obtener Apple ID en App Store Connect → reemplazar `idXXXXXXXXXX`).

### BLOQ-02. TriumphServices.cs — Implementar interfaces reales IKYCService / IWalletService / etc.
- **Archivo**: `Assets/_Project/Scripts/Runtime/Services/Triumph/TriumphServices.cs`
- **Bloqueado hasta**: recibir SDK de Triumph.

### BLOQ-03. WalletManager.cs — Transacciones reales (CreditWinnings, ProcessRefund, GetStats)
- **Archivo**: `Assets/_Project/Scripts/Runtime/Features/CashBattle/Wallet/WalletManager.cs`
- **Bloqueado hasta**: Triumph SDK.

### BLOQ-04. CashTournamentCreateManager.cs — Flujo de creación real
- **Archivo**: `Assets/_Project/Scripts/Runtime/Features/CashBattle/Tournaments/CashTournamentCreateManager.cs`
- **Bloqueado hasta**: Triumph SDK.

---

## DECISIONES PENDIENTES DEL USUARIO

### DEC-01. FR tu/vous — Política de registro formal/informal
- ¿Todo informal "tu" o "vous" solo para acciones graves (borrar cuenta, legal)?
- Impacto: ~8-12 claves FR a cambiar. Ver M-17 en PENDIENTES_MANUALES.

### DEC-02. Online rewards — ¿Qué manager es el canonical?
- GameSessionManager paga 50 DC por victoria online. OnlineResultManager paga 15 DC. ¿Cuál se elimina?
- Ver C-P1-11 arriba.

### DEC-03. DailyOfferService seed — ¿Mantener seed predecible o personalizar?
- **Opción A**: Dejarlo (seed por fecha = ofertas iguales para todos = justo, predecible)
- **Opción B**: `seed = userId + date` (ofertas personalizadas, no predecibles)
- **Opción C**: Generar server-side via Cloud Function
- Riesgo actual: bajo (solo afecta planificación de gasto DG, no seguridad)

---

*Compilado el 2026-03-18 de: SuperAudit/CRASH_HUNTER.md, SuperAudit/WARNING_SCANNER.md, SuperAudit/SECURITY_AUDITOR.md, SuperAudit/LOCALIZATION_AUDITOR.md, SuperAudit/PRODUCTION_READINESS.md, SuperAudit/PENDIENTES_NUEVOS.md, PENDIENTES_ECONOMIA.md, AVATAR_REMOVAL_PLAN.md, ICONS_PENDING.md, TAREAS_MANUALES.md, PENDIENTES_REALES.md*
