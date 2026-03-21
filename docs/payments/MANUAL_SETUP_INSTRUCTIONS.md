# Digit Park Pro — Manual Setup Instructions

> Everything the developer must do manually that code cannot automate.
>
> **Version**: 1.0
> **Fecha**: 2026-03-10
> **Tiempo estimado total**: 4-6 horas la primera vez

---

## A. Stripe Account Setup (stripe.com)

### A.1 Crear la Cuenta de Stripe con la Descripcion Correcta

1. Abrir: `https://stripe.com` → clic en **"Start now"** (esquina superior derecha)
2. Completar el registro:
   - **Email**: usar el email corporativo del negocio (no personal)
   - **Full name**: nombre del desarrollador o representante legal
   - **Password**: minimo 8 caracteres
3. Verificar el email haciendo clic en el enlace que llega
4. Iniciar sesion y completar el onboarding:
   - Clic en **"Activate your account"** en el banner del dashboard
   - **Country**: United States (o el pais de registro del negocio)
   - **Business type**: Company (si es LLC/Corp) o Individual (si es freelance)

5. En **Business details**:
   - **Legal business name**: `Matrix Software LLC` (o razon social registrada)
   - **Business website**: `https://digitpark.com`
   - **Product description** — usar EXACTAMENTE este texto:
     ```
     Mobile game selling cosmetic digital goods. Players purchase virtual
     currency (Sparks) and cosmetic theme packs for a skill-based mobile
     game. All purchases are for non-fungible digital cosmetics with no
     cash value. No gambling, wagering, or tournament entry fees are
     processed through this account.
     ```
   - **Industry**: Software → Mobile Apps & Games
   - **Business website**: `https://digitpark.com`

6. En **Customer support details**:
   - **Support phone**: numero de contacto del negocio
   - **Support email**: `support@digitpark.com`
   - **Support website**: `https://digitpark.com/support`
   - **Statement descriptor**: `DIGITPARK COSMETIC` (exactamente, max 22 chars)

7. Completar la verificacion de identidad:
   - Proveer informacion del dueno con >25% de ownership
   - Subir documento de identidad cuando se solicite (pasaporte o licencia de conducir)

8. Agregar cuenta bancaria para payouts:
   - En dashboard: **Settings** → **Payouts** → **Add bank account**
   - Ingresar routing number y account number de la cuenta bancaria del negocio

### A.2 Crear los Productos en el Dashboard de Stripe

Stripe requiere que los productos y precios se creen en el Dashboard antes de poder usarlos.

1. En el Stripe Dashboard, ir a: `https://dashboard.stripe.com/products`
2. Clic en **"Add product"** (boton azul, esquina superior derecha)
3. Crear cada uno de los siguientes productos — los nombres deben coincidir EXACTAMENTE con los valores en `ProductCatalog.cs`:

#### Producto 1: 150 Sparks
- **Name**: `150 Sparks`
- **Description**: `150 units of virtual currency for Digit Park cosmetic shop`
- **Image**: (opcional) subir imagen del paquete de Sparks
- En **Pricing**, clic en **"Add pricing"**:
  - **Pricing model**: Standard pricing
  - **Currency**: USD
  - **Price**: `0.99`
  - **Billing period**: One time
- Clic en **"Save product"**
- **IMPORTANTE**: Copiar el `Price ID` que aparece (formato `price_xxxxxxxxxxxxx`) — se necesita en el siguiente paso

#### Producto 2: 500 Sparks
- **Name**: `500 Sparks`
- **Description**: `500 units of virtual currency for Digit Park cosmetic shop`
- **Price**: `4.99` (one time, USD)
- Copiar el Price ID

#### Producto 3: 1,200 Sparks
- **Name**: `1,200 Sparks`
- **Description**: `1200 units of virtual currency for Digit Park cosmetic shop`
- **Price**: `9.99` (one time, USD)
- Copiar el Price ID

#### Producto 4: 2,500 Sparks
- **Name**: `2,500 Sparks`
- **Description**: `2500 units of virtual currency for Digit Park cosmetic shop`
- **Price**: `19.99` (one time, USD)
- Copiar el Price ID

#### Producto 5: 6,500 Sparks
- **Name**: `6,500 Sparks`
- **Description**: `6500 units of virtual currency for Digit Park cosmetic shop`
- **Price**: `49.99` (one time, USD)
- Copiar el Price ID

#### Producto 6: 14,000 Sparks
- **Name**: `14,000 Sparks`
- **Description**: `14000 units of virtual currency for Digit Park cosmetic shop`
- **Price**: `99.99` (one time, USD)
- Copiar el Price ID

#### Producto 7: Premium Theme Bundle
- **Name**: `Premium Theme Bundle`
- **Description**: `15 premium cosmetic themes for Digit Park`
- **Price**: `26.25` (one time, USD)
- Copiar el Price ID

#### Producto 8: Complete Theme Collection
- **Name**: `Complete Theme Collection`
- **Description**: `Complete collection of 20 cosmetic themes for Digit Park`
- **Price**: `30.45` (one time, USD)
- Copiar el Price ID

### A.3 Agregar los Price IDs a ProductCatalog.cs

Abrir `Assets/_Project/Scripts/Runtime/Payments/Core/ProductCatalog.cs` y agregar el `StripePriceId` a cada producto:

```csharp
new CosmeticProduct
{
    ProductId = "sparks_100",
    DisplayName = "150 Sparks",
    StripePriceId = "price_XXXXX_EL_QUE_COPIASTE",  // <-- agregar aqui
    AppleProductId = "com.matrixsoftware.digitpark.gems_100",
    ...
}
```

Repetir para los 8 productos.

**Guardar el archivo y hacer commit** — estos IDs son necesarios para que el backend cree sesiones correctamente.

### A.4 Configurar Webhooks en Stripe

Los webhooks permiten que Stripe notifique al backend cuando un pago se completa (incluso si el usuario cierra la app antes de que el polling termine).

1. En el Stripe Dashboard: `Developers` → `Webhooks`
2. Clic en **"Add endpoint"**
3. **Endpoint URL**: `https://us-central1-TU-PROYECTO.cloudfunctions.net/stripeWebhook`
   - Para testing local usar Stripe CLI (ver DEVELOPER_ONBOARDING.md)
4. En **"Select events"**, seleccionar exactamente estos eventos:
   - `checkout.session.completed`
   - `checkout.session.expired`
   - `payment_intent.payment_failed`
   - `charge.refunded`
   - `charge.dispute.created`
5. Clic en **"Add endpoint"**
6. En la pagina del webhook recien creado, hacer clic en **"Reveal"** bajo "Signing secret"
7. Copiar el valor `whsec_xxxxxxxxxxxxx`
8. Guardarlo en Firebase Secret Manager:
   ```bash
   firebase functions:secrets:set STRIPE_WEBHOOK_SECRET
   # Pegar cuando pregunte: whsec_xxxxxxxxxxxxx
   ```

### A.5 Obtener las API Keys

1. En el Stripe Dashboard: `Developers` → `API keys`
2. **Publishable key** (empieza con `pk_`): copiar y pegar en:
   - Unity Inspector → `PaymentManager` → `PaymentConfig` → `stripePublishableKey`
3. **Secret key** (empieza con `sk_`): guardar en Firebase Secret Manager:
   ```bash
   firebase functions:secrets:set STRIPE_SECRET_KEY
   # Pegar cuando pregunte: sk_live_... (o sk_test_... para desarrollo)
   ```
   - **NUNCA poner el Secret Key en codigo de Unity o en git**

**Para desarrollo local**: usar las claves de TEST (`pk_test_...`, `sk_test_...`)
**Para produccion**: usar las claves de LIVE (`pk_live_...`, `sk_live_...`)

Las claves de test y live son diferentes y NO son intercambiables.

### A.6 Testear con las Tarjetas de Test

Con la cuenta configurada y el backend corriendo localmente:
```
Tarjeta exitosa:           4242 4242 4242 4242
Tarjeta con fallo:         4000 0000 0000 9995  (fondos insuficientes)
Expiracion (cualquiera):   12/28
CVC (cualquiera):          123
ZIP (cualquiera):          10001
```

Un pago de test exitoso aparecera en Stripe Dashboard → Payments con una etiqueta "TEST".

---

## B. Apple App Store Connect Setup (appstoreconnect.apple.com)

### B.1 Crear "Digit Park Pro" como Nueva App

1. Ir a: `https://appstoreconnect.apple.com`
2. Iniciar sesion con la Apple Developer Account (cuenta paga, $99/año)
3. En la pantalla principal, clic en **"My Apps"**
4. Clic en el boton **"+"** (esquina superior izquierda) → **"New App"**
5. Completar el formulario:
   - **Platforms**: iOS
   - **Name**: `Digit Park Pro`
   - **Primary Language**: English (U.S.)
   - **Bundle ID**: Seleccionar `com.matrixsoftware.digitpark.pro` del dropdown
     - Si no aparece, primero crear el App ID en `developer.apple.com` → Certificates, Identifiers & Profiles → Identifiers → "+" → App IDs → Bundle ID: `com.matrixsoftware.digitpark.pro`
   - **SKU**: `DIGITPARKPRO001` (identificador interno, cualquier string unico)
   - **User Access**: Full Access
6. Clic en **"Create"**

### B.2 Crear los 8 In-App Purchases

Los IAP deben coincidir EXACTAMENTE con los Apple Product IDs en `ProductCatalog.cs`.

1. En la app recien creada, en el menu lateral izquierdo: **"In-App Purchases"**
2. Clic en **"Create"** (boton azul)

#### IAP 1: 150 Sparks (Consumable)
- **Type**: Consumable
- **Reference Name**: `150 Sparks` (nombre interno para App Store Connect)
- **Product ID**: `com.matrixsoftware.digitpark.gems_100`
- Clic en **"Create"**
- En la pagina del IAP:
  - **Pricing**: clic en **"Add Pricing"** → seleccionar **Tier 1** ($0.99)
  - En **Localizations**: clic en **"Add Localization"** → English (U.S.)
    - **Display Name**: `150 Sparks`
    - **Description**: `150 units of virtual currency to spend in the Digit Park cosmetic shop`
  - Clic en **"Save"**

#### IAP 2: 500 Sparks (Consumable)
- **Type**: Consumable
- **Product ID**: `com.matrixsoftware.digitpark.gems_500`
- **Pricing**: Tier 5 ($4.99)
- **Display Name**: `500 Sparks`
- **Description**: `500 units of virtual currency with 10% bonus`

#### IAP 3: 1,200 Sparks (Consumable)
- **Type**: Consumable
- **Product ID**: `com.matrixsoftware.digitpark.gems_1200`
- **Pricing**: Tier 10 ($9.99)
- **Display Name**: `1,200 Sparks`
- **Description**: `1200 units of virtual currency with 20% bonus`

#### IAP 4: 2,500 Sparks (Consumable)
- **Type**: Consumable
- **Product ID**: `com.matrixsoftware.digitpark.gems_2500`
- **Pricing**: Tier 20 ($19.99)
- **Display Name**: `2,500 Sparks`
- **Description**: `2500 units of virtual currency with 25% bonus`

#### IAP 5: 6,500 Sparks (Consumable)
- **Type**: Consumable
- **Product ID**: `com.matrixsoftware.digitpark.gems_6500`
- **Pricing**: Tier 50 ($49.99)
- **Display Name**: `6,500 Sparks`
- **Description**: `6500 units of virtual currency with 30% bonus`

#### IAP 6: 14,000 Sparks (Consumable)
- **Type**: Consumable
- **Product ID**: `com.matrixsoftware.digitpark.gems_14000`
- **Pricing**: Tier 100 ($99.99)
- **Display Name**: `14,000 Sparks`
- **Description**: `14000 units of virtual currency with 35% bonus`

#### IAP 7: Premium Theme Bundle (Non-Consumable)
- **Type**: Non-Consumable
- **Product ID**: `com.matrixsoftware.digitpark.premium_bundle`
- **Pricing**: Tier 27 ($26.99 — el tier mas cercano a $26.25, o crear custom price si está disponible)
  - **Nota**: Si Apple no tiene un tier exacto para $26.25, usar el tier mas cercano o contactar a Apple Developer Support para precios personalizados.
- **Display Name**: `Premium Theme Bundle`
- **Description**: `Unlock 15 premium visual themes for the Digit Park game interface`

#### IAP 8: Complete Theme Collection (Non-Consumable)
- **Type**: Non-Consumable
- **Product ID**: `com.matrixsoftware.digitpark.complete_bundle`
- **Pricing**: Tier 30 ($29.99 — el tier mas cercano a $30.45)
- **Display Name**: `Complete Theme Collection`
- **Description**: `Unlock the complete collection of 20 visual themes for Digit Park`

**Estado requerido para testing**: Cada IAP debe estar en estado **"Ready to Submit"** antes de poder testearse en sandbox. Despues de crear cada uno, su estado deberia cambiar automaticamente a "Ready to Submit" si todos los campos estan completos.

### B.3 Obtener el App-Specific Shared Secret

El shared secret se usa para validar receipts de Apple IAP en el backend.

1. En App Store Connect, con la app seleccionada
2. Menu lateral → **"In-App Purchases"** → **"App-Specific Shared Secret"** (en la parte inferior de la pagina)
   - O ir directamente a la seccion de cualquier IAP → en la parte inferior aparece el enlace
3. Clic en **"Manage"** o **"Generate"** si aun no existe
4. Se genera un string hexadecimal de 32 caracteres, por ejemplo: `a1b2c3d4e5f6a1b2c3d4e5f6a1b2c3d4`
5. Guardarlo en Firebase Secret Manager:
   ```bash
   firebase functions:secrets:set APPLE_SHARED_SECRET
   # Pegar cuando pregunte: a1b2c3d4e5f6...
   ```
6. **Este secreto NO va en codigo de Unity** — solo en Firebase Secret Manager (server-side)

### B.4 Crear Cuentas de Sandbox Tester

Ver instrucciones detalladas en `DEVELOPER_ONBOARDING.md` → Seccion 3.1.

Resumen rapido:
1. `Users and Access` → `Sandbox` → `Testers` → **"+"**
2. Email: usar email ficticio que no exista en Apple
3. Territory: United States

### B.5 Configurar Sandbox en Dispositivo

Ver instrucciones detalladas en `DEVELOPER_ONBOARDING.md` → Seccion 3.2.

### B.6 Receipt Validation — Como Funciona

Apple envia un receipt (dato binario firmado) al completar cada compra. El backend lo valida contra los servidores de Apple:

- **Sandbox**: `https://sandbox.itunes.apple.com/verifyReceipt`
- **Produccion**: `https://buy.itunes.apple.com/verifyReceipt`

El backend siempre intenta primero con produccion. Si Apple responde con codigo `21007` (receipt es de sandbox), el backend reintenta contra el URL de sandbox. Esto permite usar el mismo codigo en produccion y en desarrollo.

### B.7 Unity IAP Receipt Validation Obfuscator

Unity IAP incluye una herramienta para ofuscar el shared secret en el build de iOS (para mayor seguridad):

1. En Unity, ir a: `Window` → `Unity IAP` → `Receipt Validation Obfuscator`
2. En la ventana que se abre:
   - **Apple Root CA**: dejar el valor por defecto (auto-generado por Unity)
   - **Google Play Root** (si aplica): no necesario para iOS
3. Clic en **"Obfuscate Secrets"**
4. Unity genera dos archivos en `Assets/Plugins/UnityPurchasing/generated/`:
   - `AppleTangle.cs`
   - `GooglePlayTangle.cs`
5. **Estos archivos se deben commitear al repositorio** — no contienen el secreto real, sino una version ofuscada.

**Nota**: Si el Shared Secret de Apple cambia, repetir este paso.

---

## C. Firebase Remote Config Setup (console.firebase.google.com)

### C.1 Instalar el Paquete de Firebase Remote Config en Unity

1. En Unity: `Window` → `Package Manager`
2. Clic en **"+"** → **"Add package from git URL"**
3. Ingresar: `https://dl.google.com/firebase/sdk/unity/firebase_unity_sdk.zip` — o mejor:
4. Descargar el Firebase Unity SDK desde: `https://firebase.google.com/docs/unity/setup`
5. Importar el paquete `FirebaseRemoteConfig.unitypackage`
6. En `RemoteConfigService.cs`, el `#if FIREBASE_REMOTE_CONFIG` se activara automaticamente si el scripting define `FIREBASE_REMOTE_CONFIG` esta configurado.
7. Agregar `FIREBASE_REMOTE_CONFIG` a los Scripting Define Symbols (junto con `DIGIT_PARK_PRO`).

### C.2 Descargar google-services.json / GoogleService-Info.plist

1. Ir a: `https://console.firebase.google.com`
2. Seleccionar el proyecto `digitpark-xxxxx`
3. Ir a: Configuracion del proyecto (icono de engranaje) → General
4. En la seccion "Your apps":
   - Para iOS: clic en el icono de iOS → descargar `GoogleService-Info.plist`
   - Colocar el archivo en `Assets/` (raiz del proyecto Unity)
5. **Este archivo NO se debe commitear** (ya esta en `.gitignore`).

### C.3 Crear los 7 Parametros de Remote Config

1. Ir a: `https://console.firebase.google.com` → seleccionar proyecto
2. En el menu lateral izquierdo, clic en **"Remote Config"** (bajo la seccion "Engage")
3. Clic en **"Add parameter"** para cada uno de los siguientes:

#### Parametro 1: payment_provider
- **Parameter key**: `payment_provider`
- **Data type**: String
- **Default value**: `apple_iap`
  - **Razon**: el default es el mas seguro. Stripe se activa explicitamente cuando este listo.
- Clic en **"Save"**

#### Parametro 2: stripe_enabled
- **Parameter key**: `stripe_enabled`
- **Data type**: Boolean
- **Default value**: `false`
  - **Razon**: Stripe deshabilitado por defecto. Se activa manualmente cuando el entorno de Stripe esta verificado.
- Clic en **"Save"**

#### Parametro 3: apple_iap_enabled
- **Parameter key**: `apple_iap_enabled`
- **Data type**: Boolean
- **Default value**: `true`
- Clic en **"Save"**

#### Parametro 4: triumph_enabled
- **Parameter key**: `triumph_enabled`
- **Data type**: Boolean
- **Default value**: `false`
  - **Razon**: deshabilitado hasta que el SDK de Triumph este integrado.
- Clic en **"Save"**

#### Parametro 5: cosmetic_store_enabled
- **Parameter key**: `cosmetic_store_enabled`
- **Data type**: Boolean
- **Default value**: `true`
- Clic en **"Save"**

#### Parametro 6: app_version
- **Parameter key**: `app_version`
- **Data type**: String
- **Default value**: `pro`
- Clic en **"Save"**

#### Parametro 7: maintenance_mode
- **Parameter key**: `maintenance_mode`
- **Data type**: Boolean
- **Default value**: `false`
- Clic en **"Save"**

### C.4 Publicar los Cambios

1. En la pantalla de Remote Config, clic en el boton azul **"Publish changes"** (esquina superior derecha)
2. En el dialogo de confirmacion: **"Publish changes"**
3. Los cambios estan ahora activos para todos los usuarios

### C.5 Verificar que Remote Config Funciona

1. En Unity Editor, con Firebase instalado y `FIREBASE_REMOTE_CONFIG` en los defines
2. Entrar en Play Mode
3. En la consola buscar: `[RemoteConfig] Config actualizada desde Firebase`
4. En la Payment Debug Window verificar que los valores coinciden con los configurados

### C.6 Como Activar Stripe via Remote Config

Cuando Stripe este listo para produccion:
1. En Remote Config: editar `payment_provider` → cambiar a `"stripe"`
2. Editar `stripe_enabled` → cambiar a `true`
3. Clic en **"Publish changes"**

Los dispositivos activos recibiran el cambio en el siguiente poll (maximo 15 minutos).

---

## D. Firebase Cloud Functions — Deploy del Backend

El backend de Digit Park usa **Firebase Cloud Functions** — sin servidor que mantener,
se despliega directamente desde el mismo proyecto Firebase que ya tienes configurado.

### D.1 Instalar Firebase CLI

```bash
npm install -g firebase-tools
firebase login
```

### D.2 Inicializar Functions en el proyecto

En la raiz de tu proyecto Unity (C:\Users\josec\digitPark\):

```bash
firebase init functions
```

Seleccionar:
- **Use an existing project** → seleccionar tu proyecto `digitpark-xxxxx`
- **Language**: TypeScript
- **ESLint**: Yes
- **Install dependencies now**: Yes (o luego con `npm install`)

> NOTA: El codigo ya esta creado en `functions/`. Si Firebase init pregunta si sobreescribir, decir NO.

### D.3 Configurar Secrets (valores sensibles)

Firebase Secret Manager guarda las credenciales de forma segura:

```bash
# Stripe
firebase functions:secrets:set STRIPE_SECRET_KEY
# Pegar: sk_live_... (o sk_test_... para desarrollo)

firebase functions:secrets:set STRIPE_WEBHOOK_SECRET
# Pegar: whsec_...

# Apple IAP
firebase functions:secrets:set APPLE_SHARED_SECRET
# Pegar: el shared secret de App Store Connect

# Slack (opcional, para alertas)
firebase functions:secrets:set SLACK_WEBHOOK_URL
# Pegar: https://hooks.slack.com/services/...
```

Para verificar que los secrets estan configurados:
```bash
firebase functions:secrets:access STRIPE_SECRET_KEY
```

### D.4 Configurar firestore.rules para entitlements

En Firebase Console → Firestore Database → Rules, agregar:

```javascript
rules_version = '2';
service cloud.firestore {
  match /databases/{database}/documents {
    // Entitlements: solo el usuario dueno puede leer sus propios
    match /entitlements/{userId}/products/{productId} {
      allow read: if request.auth != null && request.auth.uid == userId;
      allow write: if false; // Solo Cloud Functions pueden escribir
    }
    // Auditoria: solo lectura admin
    match /payment_audit/{doc} {
      allow read: if false;
      allow write: if false;
    }
    // Config de pagos (para abort protocol)
    match /payment_config/{doc} {
      allow read: if request.auth != null;
      allow write: if false;
    }
  }
}
```

### D.5 Build y Deploy

```bash
cd C:/Users/josec/digitPark/functions
npm install
npm run build

# Deploy solo las functions de pagos
firebase deploy --only functions
```

El output mostrara las URLs de cada function:
```
functions[stripeCreateCheckout]: https://us-central1-PROYECTO.cloudfunctions.net/stripeCreateCheckout
functions[stripeWebhook]: https://us-central1-PROYECTO.cloudfunctions.net/stripeWebhook
functions[stripeSessionStatus]: https://us-central1-PROYECTO.cloudfunctions.net/stripeSessionStatus
functions[iapValidateReceipt]: https://us-central1-PROYECTO.cloudfunctions.net/iapValidateReceipt
functions[getEntitlements]: https://us-central1-PROYECTO.cloudfunctions.net/getEntitlements
functions[syncEntitlements]: https://us-central1-PROYECTO.cloudfunctions.net/syncEntitlements
functions[paymentsHealth]: https://us-central1-PROYECTO.cloudfunctions.net/paymentsHealth
functions[adminForceSwitch]: https://us-central1-PROYECTO.cloudfunctions.net/adminForceSwitch
```

### D.6 Agregar las URLs al Inspector de Unity

En Unity, seleccionar el GameObject `PaymentManager` en la escena Boot y configurar en el Inspector:

**PaymentConfig** — completar cada URL con el ID real del proyecto Firebase:
- `stripePublishableKey`: `pk_live_...` (o `pk_test_...` para desarrollo)
- `stripeCreateCheckoutUrl`: `https://us-central1-TU-PROYECTO.cloudfunctions.net/stripeCreateCheckout`
- `stripeSessionStatusUrl`: `https://us-central1-TU-PROYECTO.cloudfunctions.net/stripeSessionStatus`
- `stripeWebhookUrl`: `https://us-central1-TU-PROYECTO.cloudfunctions.net/stripeWebhook`
- `iapValidateReceiptUrl`: `https://us-central1-TU-PROYECTO.cloudfunctions.net/iapValidateReceipt`
- `getEntitlementsUrl`: `https://us-central1-TU-PROYECTO.cloudfunctions.net/getEntitlements`
- `syncEntitlementsUrl`: `https://us-central1-TU-PROYECTO.cloudfunctions.net/syncEntitlements`
- `paymentsHealthUrl`: `https://us-central1-TU-PROYECTO.cloudfunctions.net/paymentsHealth`
- `adminForceSwitchUrl`: `https://us-central1-TU-PROYECTO.cloudfunctions.net/adminForceSwitch`

Reemplazar `TU-PROYECTO` con el Project ID de Firebase (ejemplo: `digitpark-a1b2c`).

### D.7 Actualizar URL del Webhook en Stripe Dashboard

Con las functions ya desplegadas, actualizar la URL del webhook en Stripe:

1. Stripe Dashboard → **Developers** → **Webhooks**
2. Editar el webhook existente (o crear uno nuevo)
3. **Endpoint URL**: `https://us-central1-TU-PROYECTO.cloudfunctions.net/stripeWebhook`
4. Guardar y copiar el nuevo Signing Secret → `firebase functions:secrets:set STRIPE_WEBHOOK_SECRET`

### D.8 Monitoreo

- Firebase Console → **Functions** → ver logs en tiempo real
- Firebase Console → **Firestore** → revisar coleccion `entitlements` y `payment_audit`
- Stripe Dashboard → **Developers** → **Webhooks** → ver eventos recibidos

---

## E. Unity Build Configuration

### E.1 Scripting Define Symbols

1. En Unity: `Edit` → `Project Settings` → `Player` → seleccionar iOS
2. En **"Other Settings"** → **"Scripting Define Symbols"**
3. Para build Pro: `DIGIT_PARK_PRO;FIREBASE_REMOTE_CONFIG;HAS_STRIPE`
4. Para build Global: `DIGIT_PARK_GLOBAL;FIREBASE_REMOTE_CONFIG`
5. Presionar Enter para aplicar

**Nota**: `HAS_STRIPE` activa el codigo de `StripePaymentProvider.cs` (la seccion `#if HAS_STRIPE || UNITY_EDITOR`). Sin este define, Stripe esta completamente excluido de la compilacion.

### E.2 Bundle Identifier

1. En `Project Settings` → `Player` → iOS
2. **Bundle Identifier**:
   - Build Pro: `com.matrixsoftware.digitpark.pro`
   - Build Global: `com.matrixsoftware.digitpark`
3. **Version**: actualizar segun la version a subir (ej: `1.0.0`)
4. **Build**: incrementar el build number (ej: `1`)

### E.3 App Transport Security (iOS)

Stripe Checkout y Firebase Cloud Functions usan HTTPS. iOS requiere HTTPS para todas las conexiones — no se necesita ninguna excepcion.

1. En `Project Settings` → `Player` → iOS → `Other Settings`
2. Verificar que **"Allow HTTP"** esta **DESACTIVADO** — todas las URLs del sistema usan HTTPS
3. Las URLs de Firebase (`cloudfunctions.net`) y Stripe (`checkout.stripe.com`) son HTTPS por defecto

### E.4 URL Scheme para Deep Links

El sistema de retorno de Stripe usa deep links con el scheme `digitpark://`.

1. En `Project Settings` → `Player` → iOS → `Other Settings`
2. En **"Supported URL schemes"**, agregar: `digitpark`
3. Esto genera automaticamente la configuracion en el `Info.plist` del proyecto Xcode

### E.5 Unity IAP Receipt Validation Obfuscator

Ver Seccion B.7 para el paso completo. Resumen:
1. `Window` → `Unity IAP` → `Receipt Validation Obfuscator`
2. Clic en **"Obfuscate Secrets"**
3. Commitear los archivos `AppleTangle.cs` y `GooglePlayTangle.cs` generados

### E.6 Configurar Firebase Functions URLs en PaymentConfig

Despues de hacer deploy de las Cloud Functions (Seccion D.5), configurar las URLs en Unity:

1. En Unity, seleccionar el GameObject `PaymentManager` en la escena Boot
2. En el Inspector → **PaymentConfig**, completar todos los campos de URL (ver Seccion D.6 para la lista completa con nombres exactos de campo)
3. Reemplazar `TU-PROYECTO` con el Project ID de Firebase (ejemplo: `digitpark-a1b2c`)
4. Guardar la escena

---

## F. Universal Links para Retorno desde Stripe (Xcode)

Universal Links son la forma preferida de retornar a la app desde el browser de Stripe. Son mas confiables que URL Schemes porque Apple los valida.

### F.1 Archivo apple-app-site-association en el Servidor

1. En el backend, crear el archivo en la ruta publica `/.well-known/apple-app-site-association`
2. El contenido del archivo (JSON sin extension):
   ```json
   {
     "applinks": {
       "apps": [],
       "details": [
         {
           "appID": "TEAM_ID.com.matrixsoftware.digitpark.pro",
           "paths": ["/stripe-return*", "/payment-complete*"]
         }
       ]
     }
   }
   ```
   - Reemplazar `TEAM_ID` con el Team ID de Apple Developer (encontrarlo en `developer.apple.com` → Membership → Team ID, ejemplo: `A1B2C3D4E5`)

3. El archivo DEBE servirse sin redirecciones y con Content-Type `application/json`
4. Verificar que es accesible: `https://api-pro.digitpark.com/.well-known/apple-app-site-association`

### F.2 Xcode: Agregar Associated Domains Capability

1. Abrir el proyecto Xcode generado por Unity
2. Seleccionar el Target principal (nombre del proyecto)
3. Ir a la pestaña **"Signing & Capabilities"**
4. Clic en **"+ Capability"** → buscar y agregar **"Associated Domains"**
5. En la lista de Associated Domains, clic en **"+"** y agregar:
   - `applinks:api-pro.digitpark.com`
6. Asegurarse de que el Provisioning Profile incluye Associated Domains (requiere App ID con la capability habilitada en `developer.apple.com`)

### F.3 Habilitar Associated Domains en el App ID

1. Ir a: `https://developer.apple.com` → Certificates, Identifiers & Profiles → Identifiers
2. Buscar y seleccionar el App ID `com.matrixsoftware.digitpark.pro`
3. En la lista de capabilities, habilitar **"Associated Domains"**
4. Clic en **"Save"**
5. Regenerar el Provisioning Profile (los profiles existentes se invalidan al cambiar capabilities)

### F.4 Testear el Deep Link Return

1. Build e instalar en dispositivo
2. Iniciar una compra en la app
3. El checkout se abrira en Safari
4. Completar (o cancelar) el pago
5. Stripe redirige a `https://api-pro.digitpark.com/stripe-return?session_id=xxx`
6. iOS intercepta el Universal Link y vuelve a la app automaticamente
7. La app recibe el deep link y `StripeCheckoutController.OnStripeReturn()` lo procesa

**Si no funciona**: verificar que el archivo `apple-app-site-association` es accesible desde el dispositivo (no desde localhost), que el formato JSON es exacto, y que el Team ID es correcto.

---

## G. Triumph SDK (cuando este disponible)

### G.1 Donde Obtener el SDK

1. Contactar a Triumph: `https://docs.triumph.app` o email a `partnerships@triumph.app`
2. El SDK se entrega como un paquete Unity (`.unitypackage`) o via Package Manager
3. Se necesita una cuenta de Triumph aprobada para mercados de skill gaming en EE.UU.

### G.2 Archivos a Modificar para Integrar el SDK Real

Una vez recibido el SDK, los stubs a reemplazar son:

- `Assets/_Project/Scripts/Runtime/Services/Triumph/TriumphServices.cs`
  - Reemplazar los `throw new NotImplementedException()` con las implementaciones reales del SDK
  - Los metodos: `InitiateDeposit()`, `InitiateWithdrawal()`, `GetBalance()`, `StartKYC()`, etc.

- `Assets/_Project/Scripts/Runtime/Services/Triumph/TriumphManager.cs`
  - Reemplazar el mock mode con la inicializacion real del SDK

**REGLA CRITICA**: Al integrar el SDK de Triumph, NO modificar ningun archivo dentro de `Assets/_Project/Scripts/Runtime/Payments/`. La integracion de Triumph es completamente independiente del sistema de cosmeticos.

### G.3 Como Testear en Sandbox de Triumph

1. El SDK de Triumph provee un entorno sandbox para testing sin dinero real
2. Segun la documentacion de Triumph (`docs.triumph.app`), hay credenciales de sandbox similares a las de Stripe
3. Configurar `TriumphManager` con las credenciales de sandbox antes de testear

---

## H. Pre-Launch Checklist

Verificar cada item antes de subir a App Store Connect para review.

### H.1 Stripe

- [ ] Cuenta de Stripe verificada y activa (no en modo "test only")
- [ ] Descripcion del negocio correcta en Business Settings
- [ ] Los 8 productos creados en Stripe Dashboard con nombres exactos
- [ ] Price IDs (`price_xxx`) ingresados en `ProductCatalog.cs`
- [ ] Webhook configurado con los 5 eventos requeridos
- [ ] `STRIPE_WEBHOOK_SECRET` configurado en el backend de produccion
- [ ] Compra de test exitosa con tarjeta `4242 4242 4242 4242`
- [ ] Rechazo de test verificado con tarjeta `4000 0000 0000 9995`
- [ ] Claves LIVE (`pk_live_...`, `sk_live_...`) configuradas en produccion (NO las de test)
- [ ] Statement descriptor visible y correcto
- [ ] Politica de reembolsos publicada en `digitpark.com/refunds`

### H.2 Apple IAP

- [ ] App "Digit Park Pro" creada en App Store Connect
- [ ] Bundle ID `com.matrixsoftware.digitpark.pro` registrado
- [ ] Los 8 IAP creados con Product IDs exactos
- [ ] Todos los IAP en estado "Ready to Submit" o "Approved"
- [ ] App-Specific Shared Secret generado y configurado en backend
- [ ] Compra de test exitosa en dispositivo con cuenta sandbox
- [ ] Restore Purchases verificado para Non-Consumables
- [ ] Receipts validados correctamente contra backend
- [ ] `AppleTangle.cs` y `GooglePlayTangle.cs` generados y commiteados

### H.3 Firebase Remote Config

- [ ] Firebase SDK instalado en Unity
- [ ] `GoogleService-Info.plist` en el proyecto
- [ ] Los 7 parametros creados con valores correctos
- [ ] Remote Config publicado
- [ ] Valores leidos correctamente en la app (verificado en consola)
- [ ] Scripting define `FIREBASE_REMOTE_CONFIG` activo

### H.4 Firebase Cloud Functions

- [ ] `firebase deploy --only functions` exitoso sin errores TypeScript
- [ ] 9 functions visibles en Firebase Console → Functions
- [ ] `paymentsHealth` retorna `{"stripe":{"status":"healthy"},...}`
- [ ] Secrets configurados: `STRIPE_SECRET_KEY`, `STRIPE_WEBHOOK_SECRET`, `APPLE_SHARED_SECRET`
- [ ] Webhook de Stripe apuntando a la URL de la Cloud Function (`/stripeWebhook`)
- [ ] Reglas de Firestore aplicadas (entitlements solo lectura propia, write solo functions)
- [ ] Logs visibles en Firebase Console → Functions → Logs

### H.5 Unity Build

- [ ] Scripting defines correctos: `DIGIT_PARK_PRO;FIREBASE_REMOTE_CONFIG;HAS_STRIPE`
- [ ] Bundle Identifier: `com.matrixsoftware.digitpark.pro`
- [ ] URL Scheme `digitpark` configurado en Player Settings
- [ ] Associated Domains capability habilitada en Xcode
- [ ] `apple-app-site-association` accesible en el servidor
- [ ] HTTP deshabilitado en builds de produccion (App Transport Security)
- [ ] Build number incrementado respecto al anterior
- [ ] Todas las API keys son de PRODUCCION (no de test)
- [ ] Todos los campos `*Url` en PaymentConfig apuntan a URLs de produccion (no localhost)
- [ ] `ProductCatalog.ValidateCatalogCompliance()` retorna `true`

### H.6 Compliance

- [ ] `StripeComplianceGuard.ValidateProduct()` verificado para todos los productos
- [ ] Ningun producto contiene terminos prohibidos (tournament, prize, cash_game, etc.)
- [ ] Session metadata siempre incluye `type: cosmetic` y `has_tournament_benefit: false`
- [ ] Backend con `triumphIsolation.middleware.ts` rechaza requests con campos de Triumph
- [ ] Documentos STRIPE_COMPLIANCE.md y PAYMENT_ARCHITECTURE.md actualizados
- [ ] Politica de privacidad actualizada con info de Stripe como procesador de pagos
- [ ] Terminos de Servicio actualizados con politica de reembolso de cosmeticos

### H.7 Testing de Regresion Pre-Launch

Ejecutar este script de testing manual en un dispositivo fisico (no simulador):

1. **Test 1 — Compra exitosa via Stripe**:
   - Abrir tienda → comprar "150 Sparks" → tarjeta 4242... → verificar que los Sparks se otorgan

2. **Test 2 — Rechazo via Stripe**:
   - Abrir tienda → comprar "500 Sparks" → tarjeta 4000...9995 → verificar mensaje de error correcto

3. **Test 3 — Fallback a Apple IAP**:
   - Bajar el backend → intentar comprar → verificar que el sistema cambia a Apple IAP automaticamente

4. **Test 4 — Restore Purchases**:
   - Desinstalar y reinstalar → usar "Restore Purchases" → verificar que Premium Bundle se restaura

5. **Test 5 — Abort Protocol**:
   - Desde Payment Debug Window → "Simular Abort Protocol" → verificar switch a Apple IAP
   - Verificar que Stripe permanece desactivado tras reabrir la app

6. **Test 6 — Remote Config**:
   - En Firebase Console, cambiar `stripe_enabled` a `false` → publicar → esperar 15 minutos o reabrir la app → verificar que Stripe se desactiva sin reinstalar

7. **Test 7 — Build Global**:
   - Cambiar define a `DIGIT_PARK_GLOBAL` → build → verificar que Stripe no esta disponible y CashBattle UI no aparece

8. **Test 8 — Compliance**:
   - En Payment Debug Window (Edit Mode) → "Validar Catalogo" → verificar resultado OK

---

## I. Verificacion Final — Pre-Launch Checklist Completo

### I.1 Firebase Cloud Functions
- [ ] `firebase deploy --only functions` exitoso sin errores
- [ ] 8 functions visibles en Firebase Console → Functions
- [ ] `paymentsHealth` endpoint retorna `{"stripe":{"status":"healthy"}, ...}`
- [ ] Secrets configurados: STRIPE_SECRET_KEY, STRIPE_WEBHOOK_SECRET, APPLE_SHARED_SECRET

### I.2 Stripe
- [ ] Cuenta activada y verificada (no en modo test para produccion)
- [ ] 8 productos creados con nombres exactos
- [ ] Price IDs agregados en ProductCatalog.cs (StripePriceId)
- [ ] Webhook configurado apuntando a Firebase Function URL
- [ ] Webhook Signing Secret guardado como secret
- [ ] Test exitoso con tarjeta 4242 4242 4242 4242

### I.3 Apple IAP
- [ ] App "Digit Park Pro" creada con bundle ID correcto
- [ ] 8 In-App Purchases configurados con IDs exactos
- [ ] Shared Secret guardado como secret en Firebase
- [ ] Unity IAP Receipt Validation Obfuscator ejecutado (Tangle files generados)
- [ ] Test exitoso con Sandbox Tester account

### I.4 Firebase Remote Config
- [ ] 7 parametros creados con nombres exactos
- [ ] `payment_provider = "stripe"` (o "apple_iap" si Stripe no esta listo)
- [ ] `stripe_enabled = true`
- [ ] `apple_iap_enabled = true`
- [ ] Cambios publicados

### I.5 Unity Build
- [ ] Scripting Define Symbols: `DIGIT_PARK_PRO;HAS_TRIUMPH;HAS_STRIPE;HAS_APPLE_IAP`
- [ ] Bundle ID iOS: `com.matrixsoftware.digitpark.pro`
- [ ] PaymentConfig en Inspector tiene las URLs de Firebase Functions
- [ ] Build compila sin errores con PRO symbols
- [ ] Build compila sin errores con GLOBAL symbols (`DIGIT_PARK_GLOBAL;HAS_APPLE_IAP`)

### I.6 Aislamiento Triumph
- [ ] TriumphIsolationGuard no reporta violaciones en Play Mode
- [ ] ProductCatalog.ValidateCatalogCompliance() retorna true
- [ ] VersionGuard.CanAccessStripe() retorna true en Pro, false en Global
- [ ] Editor: DigitPark → Payment Debug Window → sin errores

### I.7 Flujo End-to-End
- [ ] Compra Sparks via Stripe Checkout funciona completa
- [ ] Gems aparecen en el juego despues de compra
- [ ] Entitlement guardado en Firestore
- [ ] Compra via Apple IAP funciona (sandbox)
- [ ] Fallback: desactivar Stripe via Remote Config → compra usa Apple IAP automaticamente
- [ ] Abort protocol: 3 fallos de Stripe → auto-switch a Apple IAP
