# TAREAS MANUALES - Pasos que debe hacer el usuario
**Ultima actualizacion**: 2026-03-21 (V56 — tareas de codigo movidas a TAREAS_CODIGO.md)

Estas tareas NO se pueden automatizar con codigo. Requieren accion tuya en consolas externas, Unity Editor, Xcode o herramientas de diseno.

> Las tareas automatizables por codigo estan en `docs/TAREAS_CODIGO.md`

---

# BLOQUE 1: TAREAS MANUALES GENERALES
*(No dependen del SDK de Triumph — se pueden completar ahora)*

---

## P0 - BLOQUEANTES (hacer antes de publicar)

### 1. Firebase Credentials - Rotar API keys expuestas
- Ir a `https://console.cloud.google.com/apis/credentials?project=digitpark-7d772`
- Revocar/regenerar la API key antigua (estuvo expuesta en el repo)
- Firebase Console > Project Settings > General > descargar nuevo `GoogleService-Info.plist` (iOS) y `google-services.json` (Android)
- Reemplazar los archivos en `Assets/` (ya estan en .gitignore)
- **Riesgo**: Si las API keys antiguas siguen activas, alguien que las vio en el repo puede abusar de ellas

### 2. App Store ID en ReviewService
- Abrir App Store Connect > Mis Apps > DigitPark > campo "Apple ID" (numero de 10 digitos)
- Decirle a Claude el numero → el edita `Scripts/Runtime/Services/ReviewService.cs:226` por ti
- **Riesgo**: Sin esto, la solicitud de review nunca abre la App Store

### 3. Terminal — Firebase Secrets (ANTES del deploy de Functions)
Ejecutar en `C:\Users\josec\digitPark`:
```bash
firebase functions:secrets:set STRIPE_SECRET_KEY
firebase functions:secrets:set STRIPE_WEBHOOK_SECRET
firebase functions:secrets:set APPLE_SHARED_SECRET
firebase functions:secrets:set SLACK_WEBHOOK_URL   # opcional
```
- `STRIPE_SECRET_KEY`: stripe.com/dashboard > Developers > API Keys > Secret key (`sk_live_xxx`)
- `STRIPE_WEBHOOK_SECRET`: se obtiene en tarea #9 → hacer en paralelo
- `APPLE_SHARED_SECRET`: App Store Connect > Mis Apps > Compras dentro de la app > App-Specific Shared Secret
- **Riesgo CRITICO**: Sin estos secretos, todas las Cloud Functions crashean al arrancar

### 4. Terminal — Deploy Firebase Cloud Functions
```bash
cd C:\Users\josec\digitPark\functions
npm install
firebase login
firebase use digitpark-7d772
firebase deploy --only functions
```
Verificar que aparecen: `paymentsHealth`, `stripeCreateCheckout`, `stripeSessionStatus`, `iapValidateReceipt`, `stripeWebhook`, `adminForceSwitch`, `getEntitlements`, `checkEntitlement`, `syncEntitlements`
- **Riesgo CRITICO**: Sin deploy, el sistema de pagos (Stripe + Apple IAP) NO funciona

### 5. Terminal — Actualizar dependencias Cloud Functions y generar lock file
Ejecutar en `C:\Users\josec\digitPark\functions`:
```bash
npm install
git add package-lock.json
```
- Genera `package-lock.json` (builds deterministas)
- Stripe v17 y axios v1.7.9 ya actualizados en `package.json`
- **Verificar**: `npm run build` no debe mostrar errores TypeScript

### 6. Terminal — Deploy Firebase rules (RTDB + Firestore + Storage)
Ejecutar en `C:\Users\josec\digitPark`:
```bash
firebase deploy --only database,storage,firestore
```
- Despliega `database.rules.json` (RTDB), `firestore.rules` (Firestore) y `Firebase/storage.rules` (Storage)
- **Nota**: RTDB ya fue desplegado en V49; Firestore y Storage son nuevos (creados en V55 security audit)

### 7. Unity Inspector — Crear PaymentConfig ScriptableObject
1. Project panel > `Assets/_Project/Resources/` > click derecho > Create > DigitPark > Payment Config > nombrar `PaymentConfig`
2. Rellenar en Inspector:
   - `Payments Health Url`: `https://us-central1-digitpark-7d772.cloudfunctions.net/paymentsHealth`
   - `Stripe Checkout Url`: `https://us-central1-digitpark-7d772.cloudfunctions.net/stripeCreateCheckout`
   - `Stripe Session Status Url`: `https://us-central1-digitpark-7d772.cloudfunctions.net/stripeSessionStatus`
   - `Iap Validate Url`: `https://us-central1-digitpark-7d772.cloudfunctions.net/iapValidateReceipt`
   - `Stripe Publishable Key`: clave publica Stripe (`pk_live_xxx`)
   - `Iap Product Ids`: los 7 IDs consumibles (ver tarea #8)
3. Abrir `Boot.unity` > seleccionar GO con `PaymentManager` > arrastrar el ScriptableObject al campo `Config`
4. Guardar la escena
- **Nota**: Claude puede crear un editor script (C-14 en TAREAS_CODIGO.md) que auto-rellena las URLs — solo necesitaras poner la Stripe key y los IAP IDs
- **Riesgo CRITICO**: Sin esto, `PaymentManager` loguea error y no inicializa Stripe

### 8. App Store Connect + Google Play Console — Crear IAP products
**7 consumibles (Gem Packs):**
| Product ID | Precio |
|---|---|
| `com.matrixsoftware.digitpark.gems_100` | $0.99 |
| `com.matrixsoftware.digitpark.gems_300` | $2.99 |
| `com.matrixsoftware.digitpark.gems_500` | $4.99 |
| `com.matrixsoftware.digitpark.gems_1200` | $9.99 |
| `com.matrixsoftware.digitpark.gems_2500` | $19.99 |
| `com.matrixsoftware.digitpark.gems_6500` | $49.99 |
| `com.matrixsoftware.digitpark.gems_14000` | $99.99 |

**Non-consumables (Frames, Titles, Effects, Packs):** Ver `ProductCatalog.cs` — los campos `AppleProductId` de cada producto son los IDs a registrar.
- App Store Connect: Mis Apps > Compras dentro de la app > + > Consumable / Non-Consumable
- Google Play Console: Monetizacion > Productos > Productos dentro de la app > Crear producto
- Subscription (`premium_pass_monthly`): seccion separada en ambas consolas
- Despues: Unity > Window > Unity IAP > Receipt Validation Obfuscator > pegar claves > Obfuscate

### 9. Stripe Dashboard — Crear productos + webhook
**Productos**: stripe.com/dashboard/products > + Add product (uno por cada producto con `StripePriceId` en ProductCatalog — gem packs + frames + titles + effects + bundles + welcome packs)

**Webhook**: Developers > Webhooks > + Add endpoint:
- URL: `https://us-central1-digitpark-7d772.cloudfunctions.net/stripeWebhook`
- Eventos: `checkout.session.completed`, `payment_intent.payment_failed`
- Copiar el Signing Secret (`whsec_xxx`) → usar en tarea #3 como `STRIPE_WEBHOOK_SECRET`
- **Riesgo**: Sin el webhook, los pagos Stripe exitosos no otorgan entitlements

### 10. Unity Build Settings — Verificar Development Build desmarcado
- File > Build Settings > desmarcar `Development Build`
- Player Settings > Other Settings > Scripting Define Symbols: verificar que NO esta `DEVELOPMENT_BUILD`
- **Riesgo**: DevTools (DebugManager, PremiumDebugController) se incluyen si esta activo

---

## P1 - IMPORTANTES (hacer antes o poco despues de publicar)

### ~~11. Firebase Security Rules — RTDB~~ ✅ COMPLETADO V49
- `firebase deploy --only database` ejecutado. Ver tarea #6 para Firestore + Storage (pendiente).

### 12. Activar Firebase Cloud Messaging (FIREBASE_MESSAGING)
- `Player Settings > Other Settings > Scripting Define Symbols` → agregar `FIREBASE_MESSAGING`
- Firebase Console > Cloud Messaging > Apple app configuration > subir archivo `.p8` de APNs
  - Generar en developer.apple.com > Certificates > Keys > + > Apple Push Notifications service
  - Rellenar Key ID y Team ID
- Xcode: Signing & Capabilities > + Capability > Push Notifications
- Xcode: + Capability > Background Modes > marcar "Remote notifications"
- **Estado actual**: `NotificationService.cs` lista pero inactiva por falta del define

### 13. Instalar Firebase Remote Config
- Window > Package Manager > + > Add package by name: `com.google.firebase.remote-config`
- Firebase Console (`https://console.firebase.google.com/project/digitpark-7d772/config`) > Crear 4 parametros:
  - `payment_provider` = `stripe` (String)
  - `stripe_enabled` = `true` (Boolean)
  - `maintenance_mode` = `false` (Boolean)
  - `min_app_version` = `1.0.0` (String)
- Hacer click en Publish changes

### 14. Instalar Firebase Crashlytics
- Window > Package Manager > + > Add package by name: `com.google.firebase.crashlytics`
- Post-instalacion: llamar `Crashlytics.SetUserId(userId)` en `AuthenticationService` OnLoginSuccess

### 15. Sign In with Apple — Verificar entitlement en Xcode
- Target > Signing & Capabilities > verificar que "Sign In with Apple" esta en la lista
- Si no: + Capability > Sign In with Apple
- developer.apple.com > Identifiers > App ID de DigitPark > Sign In with Apple debe estar Enabled
- **Riesgo**: App Store rechaza apps con login social sin Sign In with Apple en iOS

### 16. Firebase Console Firestore — Crear Admin API Key
- `https://console.firebase.google.com/project/digitpark-7d772/firestore`
- Crear coleccion `payment_config` > documento ID `admin` > campo `adminKey` (String) = clave de 256 bits
- Generar la clave ejecutando en terminal: `openssl rand -hex 32`

### 17. Privacy Policy Consent en First Launch
- Mostrar pantalla de consentimiento ANTES de iniciar analytics (Android)
- **Requisito**: GDPR requiere consentimiento antes de recopilar datos
- Evaluar integracion de Consent Management Platform (ej: Google UMP SDK)

### 18. SecurePlayerPrefs — Cifrar datos sensibles
- Integrar libreria de PlayerPrefs cifrados (ej: EasySave, SecurePlayerPrefs)
- Datos afectados: UserIDs, FCM tokens, historial, daily rewards, currency, achievement state
- **Riesgo**: PlayerPrefs son texto plano accesible en dispositivos rooteados

### 19. Multi-Accounting Detection
- Implementar device fingerprinting + rate limiting en servidor
- Requiere SDK de fingerprinting (ej: DeviceCheck de Apple, Play Integrity de Google)
- **Riesgo**: Cuentas infinitas para farmear rewards y referral abuse

### 20. Google Sign-In SDK nativo
- **Archivo**: `AuthenticationService.cs:341`
- Android: Integrar `com.google.android.gms:play-services-auth` via mainTemplate.gradle
- iOS: Integrar Google Sign-In SDK via Podfile / CocoaPods
- **Estado actual**: Usa Firebase Auth federated provider que abre WebView — puede fallar en dispositivos reales

---

## P1 — THEMEAPPLIER (requieren Unity Editor abierto)

### TA-01. Unity Editor — Ejecutar "Add to ALL Scenes"
- Menu: DigitPark > Polish > Themes > Add to ALL Scenes
- Hacerlo DESPUES de ejecutar todos los UIBuilders de la sesion
- Escenas excluidas automaticamente: CashBattle (#08, #32-40), AgeVerification

### TA-02. Unity Editor — BackButton.prefab: ThemeApplier en child Icon
- Abrir `Assets/_Project/Prefabs/Common/BackButton.prefab` en modo prefab edit
- Seleccionar child `Icon` (o `Arrow`)
- Add Component > ThemeApplier → ElementType = `Accent`, applyToImage = true
- Guardar prefab (se propaga a todas las escenas)

### TA-03. Unity Editor — Regenerar sprite BackButton como white glyph
- Crear en Figma/Illustrator/Photoshop: chevron `<` blanco puro `#FFFFFF`, fondo transparente, 128×128 px
- Exportar como PNG con alpha
- Reemplazar el sprite en `Assets/_Project/Art/Icons/`
- En Unity Inspector del sprite: Texture Type = Sprite, Alpha Is Transparency = ✓, Apply

---

## P2 - MEJORAS (post-lanzamiento, requieren Unity Editor)

### 21. Unity Project Settings — Agregar tag "FrameLayer"
- Edit > Project Settings > Tags and Layers > Tags > + > escribir `FrameLayer`
- Necesario para `FrameRenderer.cs` y `CashThemeForcer.cs`
- **Alternativa**: Pedir a Claude el editor script C-15 de TAREAS_CODIGO.md

### 22. Unity Inspector — Economy Rebalance: precios DC en ShopItemData ScriptableObjects
- Navegar a `Assets/_Project/Resources/Shop/`
- Frames DC: Basic 500→2000 | Bronze 1000→5000 | Silver 2500→12000 | Gold 5000→25000 | Neon 7500→40000 | Diamond 10000→60000 | Crystal 12000→80000 | Platinum 15000→100000
- Titles DC: Strategist/Analyst 2000→8000 | Champion/Gladiator 5000→20000
- Efectos DC: Confetti Burst 3000→12000 | Fireworks 5000→20000
- **Alternativa**: Pedir a Claude el editor script C-16 de TAREAS_CODIGO.md (lo hace en batch)

### 23. Unity Inspector — Verificar Tier B themes priceType
- Abrir cada ScriptableObject de tema Tier B (Sakura, Matrix, CyberFuchsia, etc.)
- Verificar: `priceType = DigitGems`, `gemsPrice = 350`
- **Alternativa**: Pedir a Claude el editor script C-17 de TAREAS_CODIGO.md

### 24. Unity Inspector — Achievements.unity: asignar TrophyCard.prefab
- Abrir `Achievements.unity` > seleccionar GO con `AchievementsManager`
- Arrastrar `Assets/_Project/Prefabs/Monetization/TrophyCard.prefab` al campo `Trophy Card Prefab`

### 25. Unity Inspector — Shop.unity: desactivar GO debug D80/D90
- Abrir `Shop.unity` > buscar GO con texto "D80" o "D90" visible
- Si esta activo: desactivarlo (checkbox off) o eliminarlo

### 26. Unity Inspector — Canvas Scaler: verificar 6 escenas
Para cada escena, seleccionar Canvas > Canvas Scaler y verificar:
- UI Scale Mode = Scale With Screen Size
- Reference Resolution = 1080 × 1920
- Match Width Or Height = 0.5
- Escenas: Boot.unity, MainMenu.unity, Settings.unity, AgeVerification.unity, Register.unity, CashBattleHub.unity

### 27. Unity Inspector — Boot.unity: verificar BootManager fields wired
- Abrir `Boot.unity` > seleccionar GO con `BootManager`
- Verificar que `loadingBar`, `loadingText`, `versionText` NO estan vacios
- Si estan vacios: DigitPark > Scenes > Build Scene > Core > Boot

### 28. Unity Editor — Regenerar UIBuilders post-V50
Ejecutar desde el menu despues de compilar sin errores:
- DigitPark > Friends > Build Friends UI
- DigitPark > Profile > Build Profile UI
- DigitPark > Profile > Build Scores UI
- DigitPark > Matchmaking > Build Matchmaking UI
- DigitPark > Friends > Build Friend Requests UI
- DigitPark > Friends > Build Search Players UI
- DigitPark > Core > Build Main Menu UI
- DigitPark > Monetization > Build Shop Premium UI
- **Por que**: Los builders ahora agregan `FrameRenderer` — las escenas existentes no tienen el componente

---

> **Triumph SDK**: todas las tareas de Triumph (codigo + manual) estan en `docs/Triumph.md`.

*Verificado contra codigo real en 2026-03-21. Tareas de codigo movidas a docs/TAREAS_CODIGO.md*
