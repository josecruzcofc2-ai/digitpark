# TAREAS MANUALES - Pasos que debe hacer el usuario
**Ultima actualizacion**: 2026-03-23 (Simplificacion — eliminado CashBattle, Stripe, Triumph, Temas)

Estas tareas NO se pueden automatizar con codigo. Requieren accion tuya en consolas externas, Unity Editor, Xcode o herramientas de diseno.

> Las tareas automatizables por codigo estan en `docs/TAREAS_CODIGO.md`

---

## P0 - BLOQUEANTES (hacer antes de publicar)

### 1. Firebase Credentials - Rotar API keys expuestas
- Ir a `https://console.cloud.google.com/apis/credentials?project=digitpark-7d772`
- Revocar/regenerar la API key antigua (estuvo expuesta en el repo)
- Firebase Console > Project Settings > General > descargar nuevo `GoogleService-Info.plist` (iOS) y `google-services.json` (Android)
- Reemplazar los archivos en `Assets/` (ya estan en .gitignore)
- **Riesgo**: Si las API keys antiguas siguen activas, alguien puede abusar de ellas

### 3. Terminal — Firebase Secrets (ANTES del deploy de Functions)
Ejecutar en `C:\Users\josec\digitPark`:
```bash
firebase functions:secrets:set APPLE_SHARED_SECRET
firebase functions:secrets:set SLACK_WEBHOOK_URL   # opcional
```
- `APPLE_SHARED_SECRET`: App Store Connect > Mis Apps > Compras dentro de la app > App-Specific Shared Secret
- **Riesgo CRITICO**: Sin estos secretos, las Cloud Functions crashean al arrancar

### 4. Terminal — Deploy Firebase Cloud Functions
```bash
cd C:\Users\josec\digitPark\functions
npm install
firebase login
firebase use digitpark-7d772
firebase deploy --only functions
```
Verificar que aparecen: `iapValidateReceipt`, `getEntitlements`, `checkEntitlement`, `syncEntitlements`, `paymentsHealth`, `validateScore`, `deleteUserData`
- **Riesgo CRITICO**: Sin deploy, la validacion de compras IAP NO funciona

### 5. Terminal — Deploy Firebase rules
```bash
firebase deploy --only database,storage,firestore
```

### 6. App Store Connect + Google Play Console — Crear IAP products
**Consumibles (Gem Packs):**
| Product ID | Precio |
|---|---|
| `com.matrixsoftware.digitpark.gems_100` | $0.99 |
| `com.matrixsoftware.digitpark.gems_300` | $2.99 |
| `com.matrixsoftware.digitpark.gems_500` | $4.99 |
| `com.matrixsoftware.digitpark.gems_1200` | $9.99 |
| `com.matrixsoftware.digitpark.gems_2500` | $19.99 |
| `com.matrixsoftware.digitpark.gems_6500` | $49.99 |
| `com.matrixsoftware.digitpark.gems_14000` | $99.99 |

**Non-consumables (Frames, Titles, Effects):** Ver `ProductCatalog.cs` — los campos `AppleProductId` de cada producto.
- App Store Connect: Mis Apps > Compras dentro de la app > + > Consumable / Non-Consumable
- Google Play Console: Monetizacion > Productos > Productos dentro de la app > Crear producto
- Despues: Unity > Window > Unity IAP > Receipt Validation Obfuscator > pegar claves > Obfuscate

### 7. Unity Build Settings — Verificar Development Build desmarcado
- File > Build Settings > desmarcar `Development Build`
- Player Settings > Other Settings > Scripting Define Symbols: verificar que NO esta `DEVELOPMENT_BUILD`

---

## P1 - IMPORTANTES (hacer antes o poco despues de publicar)

### 8. Activar Firebase Cloud Messaging
- `Player Settings > Other Settings > Scripting Define Symbols` → agregar `FIREBASE_MESSAGING`
- Firebase Console > Cloud Messaging > Apple app configuration > subir archivo `.p8` de APNs
- Xcode: Signing & Capabilities > + Capability > Push Notifications + Background Modes > Remote notifications

### 9. Instalar Firebase Crashlytics
- Window > Package Manager > + > Add package by name: `com.google.firebase.crashlytics`

### 10. Sign In with Apple — Verificar entitlement en Xcode
- Target > Signing & Capabilities > verificar que "Sign In with Apple" esta en la lista
- developer.apple.com > Identifiers > App ID de DigitPark > Sign In with Apple debe estar Enabled
- **Riesgo**: App Store rechaza apps con login social sin Sign In with Apple en iOS

### 11. Privacy Policy Consent en First Launch
- Mostrar pantalla de consentimiento ANTES de iniciar analytics (Android)
- **Requisito**: GDPR requiere consentimiento antes de recopilar datos

### 12. Google Sign-In SDK nativo
- Android: Integrar `com.google.android.gms:play-services-auth` via mainTemplate.gradle
- iOS: Integrar Google Sign-In SDK via Podfile / CocoaPods

### 13. AdMob / Unity Ads — Integrar SDK de anuncios
- Elegir red: AdMob (recomendado) o Unity Ads
- Instalar SDK via Package Manager
- Crear App ID y Ad Unit IDs en la consola correspondiente
- Conectar al wrapper de ads (C-06 en TAREAS_CODIGO.md)

---

## P2 - MEJORAS (post-lanzamiento)

### 14. Unity Inspector — Canvas Scaler: verificar escenas restantes
Para cada escena, seleccionar Canvas > Canvas Scaler y verificar:
- UI Scale Mode = Scale With Screen Size
- Reference Resolution = 1080 × 1920
- Match Width Or Height = 0.5

---

## ~~Decisiones pendientes~~ ✅ RESUELTAS (C-S15 y C-S16 ejecutados)

### ~~16.~~ ✅ `_hasStylesPro` repropósito decidido
- **Decisión**: Opción A — `StylesPro` = bundle de Frames + Titles + WinEffects
- **Ejecutado**: C-S15 en PremiumManager.cs — descriptions actualizadas

### ~~17.~~ ✅ `ShopItemType.Theme` y `ShopItemType.Avatar` eliminados
- **Decisión**: Opción A — eliminados del enum + casos en switch removidos
- **Ejecutado**: C-S16 en ShopItemData.cs

*Actualizado 2026-03-24 — eliminado todo lo relacionado con Stripe, Triumph y CashBattle. Tarea #16 Multi-Accounting Detection eliminada (era exclusiva de CashBattle real-money). Cloud Functions list actualizada.*
*Actualizado 2026-03-24 — añadidas 2 decisiones pendientes (#16 y #17) que bloquean tareas de código C-S15/C-S16 en TAREAS_CODIGO.md*
*Actualizado 2026-03-24 — decisiones #16 y #17 resueltas y ejecutadas (C-S15/C-S16 completados en V56)*
