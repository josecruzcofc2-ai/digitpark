# Digit Park Pro — Runbook de Emergencia: Abort Protocol

> **Version**: 1.0
> **Fecha**: 2026-03-10
> **Proposito**: Guia paso a paso para responder a una falla critica del sistema de pagos de Stripe.
> **Tiempo objetivo de respuesta**: Paso 1 completado en menos de 5 minutos desde deteccion del problema.

---

## Evaluacion Inicial: Como Saber si Hay una Emergencia

Responder SI/NO a estas preguntas. Si alguna es SI, ejecutar el Paso 1 inmediatamente:

- [ ] Stripe enviando errores 4xx/5xx consecutivos en los logs del backend?
- [ ] Clientes reportando que no pueden completar compras?
- [ ] Email de Stripe sobre violation de ToS o actividad sospechosa?
- [ ] `stripe_abort_executed` disparado en Firebase Analytics?
- [ ] Backend de pagos caido o inaccesible?
- [ ] Fondos en hold o cuenta en revision por Stripe?

---

## Paso 1: Flipear Remote Config en Firebase Console (tiempo: ~2 minutos)

Este es el **paso mas critico**. Activa el abort en todos los dispositivos activos dentro del siguiente ciclo de polling (maximo 15 minutos).

### Instrucciones

1. Abrir navegador e ir a: `https://console.firebase.google.com`
2. Seleccionar proyecto: `digitpark-[tu-project-id]`
3. En el menu lateral izquierdo, hacer clic en **Remote Config** (bajo la seccion "Engage")
4. Buscar el parametro **`payment_provider`**:
   - Clic en el lapiz (editar) junto al parametro
   - Cambiar el valor de `"stripe"` a `"apple_iap"`
   - Clic en **Save**
5. Buscar el parametro **`stripe_enabled`**:
   - Clic en editar
   - Cambiar de `true` a `false`
   - Clic en **Save**
6. En la parte superior derecha, clic en el boton azul **"Publish changes"**
7. En el dialogo de confirmacion, clic en **"Publish changes"** nuevamente

### Verificacion

Despues de publicar, esperar 2 minutos y verificar en Firebase Analytics:
- Ir a `console.firebase.google.com` → Analytics → DebugView
- O revisar los eventos en tiempo real buscando `provider_switched`

### Efecto Inmediato vs Diferido

- **Efecto inmediato**: El abort local ya ocurrio si fue disparado por `_stripeFailureCount >= 3` o por gesture de 5 dedos. Stripe ya esta desactivado en el dispositivo que detecto el problema.
- **Efecto para todos los dispositivos**: Cuando `RemoteConfigService` haga su siguiente poll (maximo 15 minutos), todos los dispositivos recibiran la nueva configuracion.
- **Forzar efecto inmediato en todos**: No es posible forzar remotamente todos los dispositivos al instante. El maximo es 15 minutos. Si se necesita faster, la unica opcion es un hotfix de app (no recomendado para emergencias).

---

## Paso 2: Verificar que Apple IAP Esta Operativo en App Store Connect (tiempo: ~3 minutos)

Una vez que Stripe esta desactivado, confirmar que Apple IAP puede procesar pagos antes de que los usuarios lo intenten.

### Instrucciones

1. Ir a: `https://appstoreconnect.apple.com`
2. Iniciar sesion con la cuenta de Apple Developer
3. Ir a: `Apps` → seleccionar `Digit Park Pro`
4. En el menu lateral, seleccionar `In-App Purchases`
5. Verificar que los 8 productos tengan estado **"Ready to Submit"** o **"Approved"**:

| Product ID | Nombre | Tipo | Estado Esperado |
|---|---|---|---|
| `com.matrixsoftware.digitpark.gems_100` | 100 Sparks | Consumable | Approved |
| `com.matrixsoftware.digitpark.gems_500` | 500 Sparks | Consumable | Approved |
| `com.matrixsoftware.digitpark.gems_1200` | 1,200 Sparks | Consumable | Approved |
| `com.matrixsoftware.digitpark.gems_2500` | 2,500 Sparks | Consumable | Approved |
| `com.matrixsoftware.digitpark.gems_6500` | 6,500 Sparks | Consumable | Approved |
| `com.matrixsoftware.digitpark.gems_14000` | 14,000 Sparks | Consumable | Approved |
| `com.matrixsoftware.digitpark.premium_bundle` | Premium Theme Bundle | Non-Consumable | Approved |
| `com.matrixsoftware.digitpark.complete_bundle` | Complete Theme Collection | Non-Consumable | Approved |

6. Si alguno muestra **"Missing Metadata"** o **"Developer Action Needed"**: hacer clic en el producto → completar la informacion faltante → Submit for Review (puede tomar 24-48 horas — esto es un problema mayor, ver Contactos de Emergencia).

### Verificacion de Estado del Sistema Apple

- Revisar `https://www.apple.com/support/systemstatus/` → buscar "App Store" y "In-App Purchases"
- Si Apple esta con problemas, no hay nada que hacer desde nuestro lado. Monitorear y esperar.

---

## Paso 3: Recuperar Fondos Retenidos de Stripe (timeline: hasta 90 dias)

Si Stripe esta reteniendo fondos (cuenta en review o fondos en reserve), el proceso es:

### Timeline Tipico de Retencion por Stripe

| Situacion | Duracion Tipica |
|---|---|
| Cuenta nueva sin historial | 7-14 dias de reserva en primeros pagos |
| Dispute / chargeback abierto | Hasta 75-90 dias (tiempo que tiene el banco del cliente) |
| Cuenta bajo investigation | Hasta 90 dias post-resolucion |
| Cuenta cerrada por violacion | Hasta 180 dias (politica de Stripe para cuentas cerradas) |

### Pasos para Solicitar Liberacion de Fondos

1. **Documentar el caso** antes de contactar a Stripe:
   - Exportar de Firebase Analytics todos los eventos `purchase_completed` de los ultimos 90 dias.
   - Exportar de Stripe Dashboard → Payments → (filtrar por fecha) → Export CSV.
   - Preparar evidencia de que todos los pagos son por bienes cosmeticos digitales entregados.

2. **Contactar Stripe Support**:
   - Ir a: `https://support.stripe.com`
   - Clic en **"Contact support"** (esquina superior derecha)
   - Seleccionar: `Payouts & Balances` → `Funds are on hold`
   - Describir la situacion y adjuntar la evidencia documentada.

3. **Responder rapidamente** a cualquier solicitud de Stripe — cada dia sin respuesta puede extender el periodo de retencion.

4. **Formulario de apelacion**: Si la cuenta fue cerrada, ver Paso 5.

### Informacion que Stripe Puede Solicitar

- Identificacion del negocio (LLC/Corp docs, pasaporte del owner)
- Prueba de entrega de bienes digitales (capturas de Firebase Realtime DB mostrando entitlements)
- Terminos de Servicio de la app (disponible en `digitpark.com/terms`)
- Politica de reembolsos publicada
- Capturas del producto en la App Store

---

## Paso 4: Evaluar Paddle como Reemplazo Permanente de Stripe

Si la relacion con Stripe termina definitivamente, Paddle es el reemplazo preferido por varias razones:

### Por que Paddle en lugar de Stripe

| Criterio | Stripe | Paddle |
|---|---|---|
| Modelo | Payment processor (tu eres el merchant) | Merchant of Record (Paddle es el merchant) |
| Taxes globales | Tu los manejas | Paddle los maneja automaticamente |
| Cumplimiento fiscal | Requiere tu asesor | Paddle lo gestiona |
| Chargeback disputes | Tu las manejas | Paddle las maneja |
| Riesgo de cierre por ToS | Medio | Bajo (Paddle ya sabe que es un juego) |
| Integracion Unity | Plugin no oficial | Plugin oficial |

### Pasos para Evaluar Paddle

1. **Crear cuenta en** `https://paddle.com` → Sign Up → seleccionar "Gaming" como industria.
2. **Revisar documentacion**: `https://developer.paddle.com` → SDK para Unity (si existe) o Web Checkout.
3. **Evaluar precios**: Paddle cobra entre 5% y 10% + $0.50 por transaccion (vs Stripe 2.9% + $0.30). Para volumenes altos, Stripe es mas barato. Para volumenes bajos o mercados internacionales complejos, Paddle puede ser mejor.
4. **Plan de migracion**: Paddle usa `price_id` similar a Stripe. La migracion requiere:
   - Crear productos en Paddle Dashboard (equivalente a Stripe Products)
   - Reemplazar `StripePaymentProvider.cs` con `PaddlePaymentProvider.cs` (misma interfaz `IPaymentProvider`)
   - Actualizar `ProductCatalog.cs` con los Paddle Price IDs
   - Actualizar backend con Paddle SDK en lugar de Stripe SDK

5. **Tiempo estimado de migracion**: 2-3 semanas de desarrollo + 1-2 semanas de testing.

---

## Paso 5: Proceso de Apelacion con Stripe

Si la cuenta de Stripe fue suspendida o cerrada, el proceso de apelacion:

### Caso A: Cuenta Suspendida (Restricted)

La cuenta puede seguir recibiendo pagos pero no hacer payouts. Esto es temporal durante una investigacion.

1. Ir a: `https://dashboard.stripe.com` → el dashboard mostrara un banner de restriccion con un enlace directo al formulario de apelacion.
2. Completar el formulario con:
   - Descripcion del negocio (usar texto de Seccion 1 de STRIPE_COMPLIANCE.md)
   - Explicacion de la restriccion (si Stripe la provee)
   - Evidencia de que los productos son cosmeticos
3. Tiempo de respuesta tipico: 3-7 dias habiles.

### Caso B: Cuenta Cerrada por Violacion de ToS

1. **Leer el email de Stripe** cuidadosamente — generalmente especifica la violacion exacta.
2. **Ir a**: `https://support.stripe.com` → Contact support → `Account & settings` → `Account closure`
3. **Argumentar**:
   - "Nuestro uso de Stripe se limita exclusivamente a bienes cosmeticos digitales (skins y moneda virtual) en un juego movil."
   - "El sistema de torneos con dinero real usa un proveedor separado (Triumph SDK) que opera en su propio sistema de pagos sin ningun vinculo con nuestra cuenta de Stripe."
   - "Toda la metadata de sesiones de Stripe incluye `type: cosmetic` y `has_tournament_benefit: false`."
   - Adjuntar este documento (STRIPE_COMPLIANCE.md) como evidencia de separacion arquitectonica.
4. Si la apelacion es rechazada, considerar contratar un abogado especializado en fintech/payments para la apelacion formal.

### Caso C: Chargeback / Dispute

1. En Stripe Dashboard → Radar → Disputes → responder a cada dispute individualmente.
2. Para disputes de "did not recognize" o "product not received":
   - Adjuntar capturas del entitlement en Firebase Realtime DB mostrando que el producto fue entregado.
   - Adjuntar el analytics event `purchase_completed` con timestamp.
   - Adjuntar capturas del producto en uso dentro de la app.
3. Para disputes de "not as described":
   - Adjuntar las capturas de la App Store donde se describe el producto.
   - Adjuntar los Terminos de Servicio (digitpark.com/terms).

---

## Paso 6: Trigger Manual del Abort Protocol en la App

### Desde el Editor (desarrollo / QA)

Opcion A — Payment Debug Window:
1. Abrir Unity Editor
2. Menu: `DigitPark` → `Payment Debug Window`
3. Clic en el boton **"Simular Abort Protocol"**
4. Verificar en la consola: `[AbortProtocol] EJECUTANDO ABORT: ManualDeveloperTrigger`

Opcion B — Codigo:
```csharp
StripeAbortProtocol.ExecuteAbort(AbortReason.ManualDeveloperTrigger);
```

### Desde la App en Dispositivo (builds de staging/debug solamente)

El gesture de emergencia es: **5 toques simultaneos en cualquier lugar de la pantalla de la tienda** (ShopManager).

Este gesture esta habilitado SOLO en builds con el scripting define `DEBUG_PAYMENTS` activo. NUNCA en builds de produccion distribuidos por App Store.

Pasos:
1. Abrir la tienda en la app
2. Colocar 5 dedos en la pantalla al mismo tiempo
3. Mantener 2 segundos
4. Aparecera un dialogo de confirmacion: "Ejecutar Abort Protocol?"
5. Confirmar → el abort se ejecuta inmediatamente

### Desde Firebase Cloud Function (adminForceSwitch)

```bash
curl -X POST https://us-central1-TU-PROYECTO.cloudfunctions.net/adminForceSwitch \
  -H "Content-Type: application/json" \
  -H "X-Admin-Secret: [ADMIN_SECRET_KEY]" \
  -d '{"provider": "apple_iap", "reason": "manual_admin_trigger"}'
```

Esto actualiza el estado en Firestore y notifica al sistema. Los clientes activos recibiran el cambio en el siguiente poll de Remote Config (maximo 15 minutos).

### Reset del Abort (solo cuando Stripe este confirmado como saludable)

```csharp
// En editor / debug:
StripeAbortProtocol.Reset();
PaymentFeatureFlag.ForceSwitch(PaymentProvider.Stripe, "manual_reset_post_abort");
```

Y actualizar Firebase Remote Config:
- `payment_provider` → `"stripe"`
- `stripe_enabled` → `true`

---

## Paso 7: Contactos de Emergencia

### Soporte Stripe

- **Dashboard**: `https://dashboard.stripe.com`
- **Support Chat** (cuentas live): `https://support.stripe.com` → clic "Contact support" (chat disponible 24/7 para cuentas con volumen)
- **Email general**: support@stripe.com
- **Disputes**: Manejar directamente desde Dashboard → Radar → Disputes
- **Status page** (verificar si Stripe tiene problemas): `https://status.stripe.com`

### Apple Developer Support

- **App Store Connect** (para IAP issues): `https://appstoreconnect.apple.com`
- **Developer Support**: `https://developer.apple.com/contact/` → seleccionar "App Store Connect"
- **System Status**: `https://www.apple.com/support/systemstatus/`
- **Tiempo de respuesta tipico**: 24-72 horas

### Firebase Support

- **Console**: `https://console.firebase.google.com`
- **Documentacion Remote Config**: `https://firebase.google.com/docs/remote-config`
- **Status page**: `https://status.firebase.google.com`

### Triumph Support (para issues del SDK, cuando este disponible)

- **Documentacion**: `https://docs.triumph.app`
- **Dashboard**: Proporcionado por Triumph al activar la cuenta
- **Soporte**: support@triumph.app (verificar con Triumph al onboarding)

### Escalacion Interna

En caso de emergencia de pagos que no se pueda resolver en 30 minutos:

1. **Notificacion inmediata** a: [correo del CTO / fundador]
2. **Canal de Slack** (si existe): #payments-emergency
3. **Si hay fondos en riesgo superiores a $1,000 USD**: contactar al abogado del negocio para revisar opciones de apelacion formal.

---

## Checklist de Respuesta a Emergencia

Copiar y pegar este checklist en el canal de comunicacion del equipo al inicio de un incidente:

```
INCIDENTE DE PAGOS - [FECHA Y HORA]
Tipo: [Stripe caido / Cuenta suspendida / Fondos retenidos / Otro]

[ ] Paso 1: Remote Config actualizado en Firebase Console
    payment_provider = "apple_iap"
    stripe_enabled = false
    Publicado: [SI/NO] a las [HORA]

[ ] Paso 2: Apple IAP verificado en App Store Connect
    Todos los productos en estado Approved: [SI/NO]
    Status Apple: [OK / Con problemas]

[ ] Paso 3: Fondos — evaluacion
    Monto en hold: $[MONTO]
    Ticket de soporte Stripe abierto: [SI/NO - ticket ID]

[ ] Paso 4: Decision sobre reemplazo permanente
    Paddle evaluado: [SI/NO/PENDIENTE]

[ ] Paso 5: Apelacion Stripe
    Enviada: [SI/NO/NO APLICA]
    Numero de caso: [ID]

[ ] Usuarios afectados: [ESTIMADO]
[ ] Comunicacion a usuarios enviada: [SI/NO]
[ ] Resolucion: [PENDIENTE/COMPLETADA]
```
