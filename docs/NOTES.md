
#### =================================================================== ####

FASE 5: Configurar IAP en las tiendas

  App Store Connect - Crear 6 In-App Purchases (Consumable):

  ┌─────────────────────────────────────────┬────────┐
  │               Product ID                │ Precio │
  ├─────────────────────────────────────────┼────────┤
  │ com.matrixsoftware.digitpark.gems_100   │ $0.99  │
  ├─────────────────────────────────────────┼────────┤
  │ com.matrixsoftware.digitpark.gems_500   │ $4.99  │
  ├─────────────────────────────────────────┼────────┤
  │ com.matrixsoftware.digitpark.gems_1200  │ $9.99  │
  ├─────────────────────────────────────────┼────────┤
  │ com.matrixsoftware.digitpark.gems_2500  │ $19.99 │
  ├─────────────────────────────────────────┼────────┤
  │ com.matrixsoftware.digitpark.gems_6500  │ $49.99 │
  ├─────────────────────────────────────────┼────────┤
  │ com.matrixsoftware.digitpark.gems_14000 │ $99.99 │
  └─────────────────────────────────────────┴────────┘

  Google Play Console - Mismos 6 productos en Monetize > Products

  Receipt Validation (una sola vez)

  7. En Unity: Window > Unity IAP > Receipt Validation Obfuscator
    - Pegar las llaves de App Store y Google Play
    - Click Obfuscate - genera GooglePlayTangle.cs y AppleTangle.cs
