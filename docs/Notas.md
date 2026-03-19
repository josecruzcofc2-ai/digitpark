BLOQUE 1 — Fundación (sin esto nada más funciona)

  Estos dos son prerequisitos de todo el sistema. Sin ellos las pruebas de cualquier otro fix  
  son imposibles.

  ┌─────┬─────────┬──────────────────────────┬─────────────────────────────────────────────┐   
  │  #  │  Issue  │         Archivo          │                   Acción                    │   
  ├─────┼─────────┼──────────────────────────┼─────────────────────────────────────────────┤   
  │     │         │                          │ Crear el archivo que inyecta todos los      │   
  │ 1.1 │ BUG-03  │ Nuevo                    │ delegates: Auth, Currency, Analytics,       │   
  │     │         │ PaymentBridgeWiring.cs   │ DeepLink. Sin esto 100% de compras fallan   │   
  │     │         │                          │ con "user_not_authenticated".               │   
  ├─────┼─────────┼──────────────────────────┼─────────────────────────────────────────────┤   
  │     │         │                          │ Decidir: convertir a ScriptableObject       │   
  │ 1.2 │ DISC-03 │ Core/PaymentConfig.cs +  │ (recomendado) o corregir solo el mensaje de │   
  │     │         │ PaymentManager.cs        │  error engañoso. Depende de si se quiere el │   
  │     │         │                          │  .asset compartible.                        │   
  └─────┴─────────┴──────────────────────────┴─────────────────────────────────────────────┘   

  ---
  BLOQUE 2 — Catálogo de Productos (datos correctos antes de probar flujos)
                                                                                               
  Si los productos tienen datos incorrectos, los flujos de compra fallan aunque el código esté   bien.                                                                                           
  ┌─────┬─────────┬──────────────────────────┬─────────────────────────────────────────────┐   
  │  #  │  Issue  │         Archivo          │                   Acción                    │ 
  ├─────┼─────────┼──────────────────────────┼─────────────────────────────────────────────┤     │     │         │                          │ Decidir destino de los 9 frames: añadir     │ 
  │ 2.1 │ BUG-02  │ Core/ProductCatalog.cs   │ StripePriceId + crear en Stripe/AppStore, O │   
  │     │         │                          │  marcarlos IsAvailable=false hasta estar    │     │     │         │                          │ listos.                                     │ 
  ├─────┼─────────┼──────────────────────────┼─────────────────────────────────────────────┤   
  │     │         │                          │ Alinear DisplayName "150 Sparks" en código  │     │ 2.2 │ DISC-01 │ Core/ProductCatalog.cs + │ con "100 Sparks" en                         │
  │     │         │  docs setup              │ MANUAL_SETUP_INSTRUCTIONS y ABORT_RUNBOOK,  │   
  │     │         │                          │ o al revés.                                 │
  ├─────┼─────────┼──────────────────────────┼─────────────────────────────────────────────┤
  │     │         │                          │ Añadir "triumph" a prohibitedTerms en       │
  │ 2.3 │ DISC-02 │ Core/ProductCatalog.cs   │ ValidateCatalogCompliance() para            │     │     │         │                          │ sincronizar con StripeComplianceGuard.      │
  └─────┴─────────┴──────────────────────────┴─────────────────────────────────────────────┘   
  
  ---
  BLOQUE 3 — Flujo Stripe (provider primario)
                                                                                               
  Con la fundación y el catálogo correctos, se pueden arreglar y probar los flujos de Stripe.
                                                                                                 ┌─────┬───────┬──────────────────────────┬───────────────────────────────────────────────┐     │  #  │ Issue │         Archivo          │                    Acción                     │   
  ├─────┼───────┼──────────────────────────┼───────────────────────────────────────────────┤   
  │     │       │                          │ Reemplazar                                    │
  │ 3.1 │ BUG-0 │ Abort/StripeAbortProtoco │ PlayerPrefs.GetString("dp_backend_url") por   │
  │     │ 1     │ l.cs                     │ la URL real desde PaymentManager.Instance._co │     │     │       │                          │ nfig.adminForceSwitchUrl.                     │
  ├─────┼───────┼──────────────────────────┼───────────────────────────────────────────────┤   
  │     │ BUG-0 │                          │ No hacer fallback a Apple IAP si Stripe       │
  │ 3.2 │ 4     │ Core/PaymentManager.cs   │ retornó session_expired o timeout — el pago   │     │     │       │                          │ pudo haber completado ya en Stripe.           │
  └─────┴───────┴──────────────────────────┴───────────────────────────────────────────────┘   
  
  ---
  BLOQUE 4 — Apple IAP (failsafe)
                                                                                               
  Después de que Stripe esté sólido, asegurar que el failsafe de Apple IAP también valida 
  correctamente server-side.                                                                      
  ┌─────┬────────┬──────────────────────────────┬───────────────────────────────────────────┐    │  #  │ Issue  │           Archivo            │                  Acción                   │
  ├─────┼────────┼──────────────────────────────┼───────────────────────────────────────────┤  
  │     │        │                              │ Propagar el receipt data real desde el    │
  │ 4.1 │ BUG-05 │ AppleIAP/AppleIAPProvider.cs │ delegate InvokePurchase hacia             │
  │     │        │                              │ ValidateReceiptAsync() y conectar con     │    │     │        │                              │ _receiptValidator.ValidateReceipt().      │
  └─────┴────────┴──────────────────────────────┴───────────────────────────────────────────┘  
  
  ---
  BLOQUE 5 — Entitlements / Persistencia Firebase
                                                                                               
  Con ambos providers funcionando y validando, implementar la sincronización real con Firebase   para que las compras sobrevivan reinstalaciones.                                                
  ┌─────┬────────┬────────────────────────────────────┬─────────────────────────────────────┐    │  #  │ Issue  │              Archivo               │               Acción                │
  ├─────┼────────┼────────────────────────────────────┼─────────────────────────────────────┤  
  │     │        │                                    │ Implementar SyncWithServer() real   │
  │ 5.1 │ BUG-06 │ Entitlements/EntitlementService.cs │ llamando a getEntitlementsUrl de    │
  │     │        │                                    │ Firebase Cloud Functions.           │    └─────┴────────┴────────────────────────────────────┴─────────────────────────────────────┘
                                                                                               
  ---
  BLOQUE 6 — Warnings y Pulido
                                                                                               
  Últimos ajustes de robustez. No bloquean funcionalidad pero son necesarios para producción.
                                                                                                 #: 6.1                                                                                         Issue: WARN-03                                                                               
  Archivo: FeatureFlags/RemoteConfigService.cs                                                 
  Acción: Reemplazar .AsIEnumerator() con while (!task.IsCompleted) yield return null. Riesgo
  de
    compile error con ciertas versiones del Firebase SDK.                                        ────────────────────────────────────────
  #: 6.2                                                                                         Issue: WARN-02
  Archivo: FeatureFlags/PaymentFeatureFlag.cs                                                  
  Acción: Manejar PaymentProvider.None en SaveProviderOverride() para no guardar "apple_iap"
    cuando ambos providers están caídos.
  ────────────────────────────────────────
  #: 6.3                                                                                         Issue: WARN-01
  Archivo: Stripe/StripeCheckoutController.cs + Core/PaymentBridge.cs                          
  Acción: Añadir UnregisterDeepLinkHandler y llamarlo al completar o cancelar la compra.

  ---
  Mapa de dependencias
                                                                                                 BLOQUE 1 (Fundación)
    └── BLOQUE 2 (Catálogo correcto)                                                                     └── BLOQUE 3 (Stripe funcional)
                ├── BLOQUE 4 (Apple IAP validado)                                              
                │     └── BLOQUE 5 (Entitlements Firebase)                                     
                │           └── BLOQUE 6 (Pulido final)                                        
                └── BLOQUE 6 (puede ir en paralelo con 4 y 5)                                  
                                                                                                 ---                                                                                            Resumen de archivos a tocar por bloque                                                                                                                                                      
  ┌────────┬────────────────────────┬───────────────────────────────────────────────────────┐
  │ Bloque │    Archivos nuevos     │                 Archivos modificados                  │  
  ├────────┼────────────────────────┼───────────────────────────────────────────────────────┤    │ 1      │ PaymentBridgeWiring.cs │ PaymentConfig.cs, PaymentManager.cs                   │  
  ├────────┼────────────────────────┼───────────────────────────────────────────────────────┤  
  │ 2      │ —                      │ ProductCatalog.cs, docs (MANUAL_SETUP, ABORT_RUNBOOK) │
  ├────────┼────────────────────────┼───────────────────────────────────────────────────────┤    │ 3      │ —                      │ StripeAbortProtocol.cs, PaymentManager.cs             │
  ├────────┼────────────────────────┼───────────────────────────────────────────────────────┤    │ 4      │ —                      │ AppleIAPProvider.cs                                   │
  ├────────┼────────────────────────┼───────────────────────────────────────────────────────┤    │ 5      │ —                      │ EntitlementService.cs                                 │
  ├────────┼────────────────────────┼───────────────────────────────────────────────────────┤  
  │ 6      │ —                      │ RemoteConfigService.cs, PaymentFeatureFlag.cs,        │
  │        │                        │ StripeCheckoutController.cs, PaymentBridge.cs         │    └────────┴────────────────────────┴───────────────────────────────────────────────────────┘
                           