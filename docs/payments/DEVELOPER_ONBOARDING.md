# Digit Park Pro — Guia de Onboarding para Desarrolladores

> **Version**: 2.0
> **Fecha**: 2026-03-10
> **Prerequisitos**: Unity 2022.3 LTS o superior, Firebase CLI instalado, acceso al repo de GitHub.
> **Estado del sistema**: Firebase Cloud Functions desplegadas, Stripe y Apple IAP configurados.

---

## 1. Configuracion de Unity (Inspector)

El sistema de pagos vive en el GameObject `PaymentManager` en la escena `Boot`.
Las URLs de Firebase Cloud Functions ya estan desplegadas — solo hay que configurarlas en el Inspector.

### 1.1 PaymentConfig en el Inspector

Seleccionar el GameObject `PaymentManager` en la escena `Boot` y completar en el Inspector:

```
stripePublishableKey:          pk_live_... (o pk_test_... para validar en sandbox)
stripeCreateCheckoutUrl:       https://us-central1-TU-PROYECTO.cloudfunctions.net/stripeCreateCheckout
stripeSessionStatusUrl:        https://us-central1-TU-PROYECTO.cloudfunctions.net/stripeSessionStatus
stripeWebhookUrl:              https://us-central1-TU-PROYECTO.cloudfunctions.net/stripeWebhook
iapValidateReceiptUrl:         https://us-central1-TU-PROYECTO.cloudfunctions.net/iapValidateReceipt
getEntitlementsUrl:            https://us-central1-TU-PROYECTO.cloudfunctions.net/getEntitlements
syncEntitlementsUrl:           https://us-central1-TU-PROYECTO.cloudfunctions.net/syncEntitlements
paymentsHealthUrl:             https://us-central1-TU-PROYECTO.cloudfunctions.net/paymentsHealth
adminForceSwitchUrl:           https://us-central1-TU-PROYECTO.cloudfunctions.net/adminForceSwitch
stripeCheckoutTimeoutSeconds:  300
stripePollingIntervalMs:       2000
maxStripeRetries:              3
sessionPollingTimeoutMinutes:  5.0
```

Reemplazar `TU-PROYECTO` con el Project ID de Firebase (ej: `digitpark-a1b2c`).

### 1.2 Firebase en Unity (GoogleService-Info.plist)

1. Ir a: `https://console.firebase.google.com` → seleccionar el proyecto
2. Configuracion del proyecto (icono de engranaje) → General → sección "Your apps"
3. Seleccionar la app iOS → descargar `GoogleService-Info.plist`
4. Colocar el archivo en `Assets/` (raiz del proyecto Unity)
5. **Este archivo NO se commitea** — ya esta en `.gitignore`

### 1.3 Scripting Define Symbols

En Unity: `Edit` → `Project Settings` → `Player` → iOS → `Other Settings` → `Scripting Define Symbols`

- **Build Pro**: `DIGIT_PARK_PRO;HAS_TRIUMPH;HAS_STRIPE;HAS_APPLE_IAP`
- **Build Global**: `DIGIT_PARK_GLOBAL;HAS_APPLE_IAP`

---

## 2. Como Testear Stripe

Las Firebase Cloud Functions ya estan desplegadas y apuntan a Stripe. Para testing pre-lanzamiento usar las claves de **test** de Stripe (`pk_test_...`, `sk_test_...`) configuradas como secrets en Firebase Secret Manager.

### 2.1 Verificar que las Functions estan Activas

Antes de testear, verificar el health endpoint:

```bash
curl https://us-central1-TU-PROYECTO.cloudfunctions.net/paymentsHealth
# Respuesta esperada:
# {"stripe":{"status":"healthy"},"iap":{"status":"healthy"},"timestamp":"..."}
```

Si este endpoint no responde, ir a Firebase Console → Functions → revisar logs de errores.

### 2.2 Tarjetas de Test de Stripe

Con las claves de test configuradas en Firebase Secret Manager, estas tarjetas funcionan directamente contra las Cloud Functions desplegadas:

| Numero de Tarjeta | Comportamiento |
|---|---|
| `4242 4242 4242 4242` | Pago exitoso |
| `4000 0000 0000 9995` | Fondos insuficientes (decline) |
| `4000 0000 0000 0002` | Tarjeta declinada generica |
| `4000 0000 0000 3220` | Requiere autenticacion 3D Secure |
| `4000 0025 0000 3155` | 3DS obligatorio, autenticacion exitosa |
| `4000 0000 0000 9987` | Cuenta cerrada |
| `4100 0000 0000 0019` | Bloqueada por Stripe Radar |

Para todas las tarjetas de test:
- **Fecha de expiracion**: cualquier mes/año futuro (ej: `12/28`)
- **CVC**: cualquier 3 digitos (ej: `123`)
- **ZIP**: cualquier 5 digitos (ej: `10001`)

### 2.3 Flujo de Test Completo (dispositivo fisico)

1. Asegurarse de que `DIGIT_PARK_PRO` esta activo en Scripting Defines
2. Instalar el build en dispositivo via Xcode (`Product` → `Run`)
3. Abrir la tienda en la app
4. Seleccionar un producto (ej: "100 Sparks")
5. El flujo llama directamente a `stripeCreateCheckoutUrl` (Firebase Function)
6. Se abre Safari con la pagina de Stripe Checkout
7. Ingresar la tarjeta de test `4242 4242 4242 4242`
8. Safari se cierra y la app recibe el deep link `digitpark://stripe-return?session_id=xxx`
9. `StripeSessionPoller` confirma el estado via `stripeSessionStatusUrl`
10. Los Sparks se otorgan y el entitlement se guarda en Firestore

En la consola de Unity verificar:
```
[StripeProvider] Sesión creada: cs_test_XXXXX
[StripePoller] Status: complete
[PaymentManager] Compra exitosa: sparks_100 via Stripe
[EntitlementService] Entitlement otorgado: sparks_100
```

### 2.4 Testear el Fallback Automatico a Apple IAP

Para simular 3 fallas de Stripe y activar el fallback:

```
Metodo 1: Usar tarjeta de rechazo 3 veces consecutivas (4000 0000 0000 9995)
Metodo 2: Usar el boton "Simular 3 Fallos Stripe" en Payment Debug Window (Play Mode)
```

En la consola:
```
[PaymentManager] Stripe fallo (1/3): ...
[PaymentManager] Stripe fallo (2/3): ...
[PaymentManager] Stripe fallo (3/3): ...
[FeatureFlag] Switch forzado: Stripe -> AppleIAP. Razon: Stripe fallo 3 veces
```

### 2.5 Webhooks

Los webhooks de Stripe ya apuntan directamente a la Cloud Function `stripeWebhook`:
```
https://us-central1-TU-PROYECTO.cloudfunctions.net/stripeWebhook
```

Para verificar que los webhooks se procesan correctamente:
1. Ir a Stripe Dashboard → Developers → Webhooks → seleccionar el endpoint
2. En "Recent deliveries" se ven todos los eventos recibidos con su estado (200 = procesado)
3. Para reenviar un webhook fallido: clic en el evento → "Resend"

---

## 3. Como Testear Apple IAP en Sandbox

### 3.1 Crear Sandbox Tester Accounts en App Store Connect

1. Ir a: `https://appstoreconnect.apple.com`
2. Menu principal → **Users and Access** → **Sandbox** → **Testers**
3. Clic en **"+"** para agregar nuevo tester
4. Llenar el formulario:
   - **First Name**: Test
   - **Last Name**: User01
   - **Email**: email ficticio que NO exista en Apple (ej: `digitpark.test01+sandbox@gmail.com`)
   - **Password**: minimo 8 caracteres, con mayuscula, minuscula, numero y simbolo
   - **Date of Birth**: fecha que indique mayor de 18 anos
   - **App Store Territory**: United States
5. Clic en **Save**
6. Crear al menos 2-3 cuentas sandbox para testing paralelo

**Nota**: Las cuentas sandbox permiten compras ilimitadas sin cargo real. Los Non-Consumables comprados en sandbox se resetean periodicamente.

### 3.2 Configurar Cuenta Sandbox en el Dispositivo iOS

**Solo en dispositivos fisicos** (el simulador no soporta IAP real).

1. Abrir `Ajustes` en el iPhone/iPad
2. Ir a: **App Store**
3. Desplazarse hasta la seccion **"SANDBOX ACCOUNT"**
4. Tocar **Sign In**
5. Ingresar las credenciales de la cuenta sandbox creada en App Store Connect
6. **NO** usar el Apple ID personal para testing IAP

### 3.3 Instalar Build para Testing IAP

Las compras IAP en sandbox requieren build via Xcode o TestFlight:

1. En Unity: `Build Settings` → `iOS` → `Build`
2. Abrir el proyecto generado en Xcode
3. Conectar dispositivo iOS
4. `Product` → `Run` (Cmd+R) para instalar y correr

Una vez instalado, el flujo de compra usa automaticamente el entorno sandbox de Apple.

### 3.4 Verificar Receipt Validation

Al completar una compra sandbox en dispositivo, la validacion va directamente a la Cloud Function:

```
[AppleIAPProvider] Compra exitosa: com.matrixsoftware.digitpark.gems_100
[AppleIAPProvider] Validando receipt server-side para: sparks_100
POST https://us-central1-TU-PROYECTO.cloudfunctions.net/iapValidateReceipt
[EntitlementService] Entitlement otorgado: sparks_100
```

La Cloud Function intenta primero contra `buy.itunes.apple.com`. Si Apple retorna codigo `21007` (receipt de sandbox), el backend automaticamente reintenta contra `sandbox.itunes.apple.com`.

### 3.5 Testear Restore Purchases

Para testear el restore de Non-Consumables (`premium_bundle`, `complete_bundle`):
1. Instalar la app limpia (sin compras previas)
2. Comprar un Non-Consumable con la cuenta sandbox
3. Desinstalar y reinstalar la app
4. Usar el boton "Restore Purchases" en la tienda
5. iOS mostrara un popup pidiendo credenciales — ingresar las del sandbox tester
6. La compra debe restaurarse correctamente

---

## 4. Como Simular el Abort Protocol en Editor

### Metodo 1: Payment Debug Window (recomendado)

1. Asegurarse de estar en Play Mode
2. Menu: `DigitPark` → `Payment Debug Window`
3. Clic en **"Simular Abort Protocol"**
4. Observar en la consola:
   ```
   [AbortProtocol] EJECUTANDO ABORT: ManualDeveloperTrigger
   [AbortProtocol] Paso 1: Switch a Apple IAP completado
   [FeatureFlag] Switch forzado: Stripe -> AppleIAP
   [AbortProtocol] Paso 3: Analytics logueado
   [AbortProtocol] Apple IAP saludable. Continuando operacion.
   [AbortProtocol] Abort completado. Razon: ManualDeveloperTrigger
   ```
5. Verificar en la ventana de debug: `Provider Activo: AppleIAP`
6. Para resetear: clic en **"Reset Abort Protocol"**

### Metodo 2: Codigo Directo

```csharp
// Ejecutar desde cualquier script en Play Mode:
DigitPark.Payments.StripeAbortProtocol.ExecuteAbort(
    DigitPark.Payments.AbortReason.ManualDeveloperTrigger);

// Para resetear:
DigitPark.Payments.StripeAbortProtocol.Reset();
```

### Metodo 3: Forzar Switch desde la Ventana de Debug

La Payment Debug Window tiene botones para switch manual sin pasar por el abort protocol:
- **"Forzar Switch → Apple IAP"**: cambia el provider sin loguear abort
- **"Forzar Switch → Stripe"**: vuelve a Stripe (util post-reset)

---

## 5. Como Cambiar entre Build Pro y Build Global

### Configurar Build Pro (DIGIT_PARK_PRO)

1. En Unity: `Edit` → `Project Settings` → `Player`
2. Seleccionar la plataforma iOS (icono de iPhone)
3. En la seccion **"Other Settings"** → **"Scripting Define Symbols"**
4. Ingresar: `DIGIT_PARK_PRO;HAS_TRIUMPH;HAS_STRIPE;HAS_APPLE_IAP`
5. Cambiar el **Bundle Identifier** a: `com.matrixsoftware.digitpark.pro`
6. Clic en **Apply**

Con `DIGIT_PARK_PRO` activo:
- Stripe esta habilitado (si Remote Config lo permite)
- Triumph esta habilitado
- CashBattle UI es visible
- `VersionGuard.CanAccessStripe()` retorna `true`

**Atajo**: Usar el menu `DigitPark` → `Build Profile Switcher` que automatiza este proceso.

### Configurar Build Global (DIGIT_PARK_GLOBAL)

1. Mismos pasos, pero en Scripting Define Symbols:
2. Ingresar: `DIGIT_PARK_GLOBAL;HAS_APPLE_IAP`
3. Cambiar Bundle Identifier a: `com.matrixsoftware.digitpark`
4. Clic en **Apply**

Con `DIGIT_PARK_GLOBAL` activo:
- Stripe SIEMPRE deshabilitado (ignorado aunque Remote Config diga stripe)
- Triumph SIEMPRE deshabilitado
- CashBattle UI oculta
- Solo Apple IAP disponible

### Verificar que el Define esta Correcto

En la Payment Debug Window (Play Mode), la linea `Version App:` debe mostrar `Pro` o `Global`.

```csharp
Debug.Log(PaymentFeatureFlag.CurrentVersion); // Pro o Global
Debug.Log(PaymentFeatureFlag.IsProVersion);   // true o false
```

### Como Hacer un Build de Distribucion para App Store

1. Configurar los Scripting Defines segun la version objetivo
2. Asegurarse de que las claves de Stripe en Firebase Secret Manager son de **PRODUCCION** (`pk_live_...`, `sk_live_...`)
3. Verificar que todas las URLs en PaymentConfig del Inspector apuntan al proyecto Firebase de produccion
4. `Build Settings` → `iOS` → `Build`
5. Abrir en Xcode, configurar Team y Provisioning Profile
6. `Product` → `Archive` → Upload a App Store Connect

---

## 6. Verificar y Monitorear Firebase Cloud Functions

Las Cloud Functions son el backend del sistema de pagos. Para monitorearlas:

### Ver Logs en Tiempo Real

1. Ir a: `https://console.firebase.google.com` → seleccionar proyecto
2. Menu lateral → **Functions** → seleccionar una funcion (ej: `stripeCreateCheckout`)
3. Clic en la pestana **"Logs"**
4. Los logs de cada compra, error y webhook aparecen aqui en tiempo real

### Verificar Health desde Terminal

```bash
# Verificar que todas las functions estan up
curl https://us-central1-TU-PROYECTO.cloudfunctions.net/paymentsHealth

# Verificar entitlements de un usuario
curl https://us-central1-TU-PROYECTO.cloudfunctions.net/getEntitlements \
  -H "Authorization: Bearer TU_FIREBASE_ID_TOKEN" \
  -H "X-App-Version: pro"
```

### Redesplegar si hay cambios

```bash
cd C:/Users/josec/digitPark/functions
npm run build
firebase deploy --only functions
```

---

## 7. Guia Rapida de Debugging

### Problema: "PaymentManager no inicializado"

**Sintoma**: La tienda muestra spinner infinito.
**Causa probable**: `PaymentManager` no esta en la escena de Boot.
**Solucion**: Verificar que hay un `GameObject` en la escena `Boot` con el componente `PaymentManager`. En consola debe aparecer: `[PaymentManager] Sistema de pagos inicializado correctamente`.

### Problema: "Stripe Provider no disponible"

**Sintoma**: Provider activo es `AppleIAP` cuando deberia ser `Stripe`.
**Causa probable 1**: Scripting Define `DIGIT_PARK_PRO` no esta configurado.
**Causa probable 2**: Firebase Cloud Functions no responden (verificar en Firebase Console → Functions).
**Causa probable 3**: Firebase Remote Config tiene `stripe_enabled = false`.
**Verificar**: Payment Debug Window → `Stripe Habilitado: True/False`.

### Problema: Firebase Function retorna 500

**Sintoma**: `[StripeProvider] Error creando sesion: ...`
**Causa probable**: Secrets de Firebase no configurados o incorrectos.
**Verificar**:
```bash
firebase functions:secrets:access STRIPE_SECRET_KEY
```
Si retorna error, re-configurar el secret con `firebase functions:secrets:set STRIPE_SECRET_KEY`.

### Problema: "Compliance violation"

**Sintoma**: `[StripeCompliance] VIOLATION: Producto 'xxx' contiene termino prohibido 'yyy'`
**Causa**: `productId` o `displayName` contiene terminos prohibidos.
**Solucion**: Revisar `ProductCatalog.cs` — ningun campo puede contener: `tournament`, `prize`, `cash_game`, `skill_game`, `real_money`, `entry_fee`, `wager`, `bet`, `gambling`, `triumph`.

### Problema: Deep link de retorno no llega

**Sintoma**: Despues de completar el pago en Stripe, la app no recibe el retorno.
**Causa en Editor**: El Editor no soporta deep links nativos de iOS — normal.
**Solucion para Editor**: El `StripeSessionPoller` detecta el completion via polling a `stripeSessionStatusUrl`. Si Stripe esta en modo test y la Function esta activa, el polling detecta `status: complete` dentro de 10 segundos.
**Causa en dispositivo**: La configuracion de `digitpark://` URL Scheme falta en Xcode.
**Solucion en dispositivo**: Verificar en Xcode → Target → Info → URL Types que existe `digitpark` como URL Scheme.

### Problema: Entitlement no se guarda en Firestore

**Sintoma**: Compra exitosa pero el item no aparece despues de reinstalar.
**Verificar**: Firebase Console → Firestore → coleccion `entitlements` → buscar el `userId`.
**Causa probable**: El usuario no esta autenticado en Firebase cuando se hace la compra.
**Solucion**: Asegurarse de que Firebase Auth esta inicializado antes que `PaymentManager` (verificar orden en `BootManager`).
