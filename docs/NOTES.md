
#### =================================================================== ####

VISUAL AUDIT - RUNTIME VERIFICATIONS PENDIENTES (Post-V41)

  Ejecutar en Unity Editor (Play Mode) y verificar visualmente:

  P0-9: CashWallet KYCPanel
    - Navegar a CashBattle > Wallet > sin KYC completado
    - Verificar que KYCPanel muestra titulo, texto explicativo y boton
    - Si solo se ve un boton "Verify Identity" sin contexto → necesita redesign

  P2-5: BetSelection translation keys
    - Navegar a Play > 1v1 Ranked > BetSelection
    - Verificar que "bet_title", "bet_coins_cost", "bet_free" se traducen
    - Si se ven keys crudas → faltan en TextNameToKeyMap o Translations.txt

  P2-8: Emoji en Onboarding slide 8
    - Abrir Onboarding > avanzar hasta slide 8 "Completado"
    - Verificar que el icono sol se renderiza (no cuadrado vacio)
    - Si no renderiza → reemplazar emoji por sprite

  P2-9: Friends count mismatch
    - Abrir Friends con amigos agregados
    - Verificar que "X friends" coincide con el numero de cards visibles
    - Es posible que sea solo datos placeholder del editor (OK si runtime es correcto)

  P2-11: CashMatchmaking countdown overlap
    - Iniciar CashBattle 1v1 matchmaking, esperar countdown
    - Verificar que el numero "3, 2, 1" NO se superpone con el badge "Lv.X"
    - Si hay overlap → ajustar anchoredPosition del countdown

  P2-12: Font sizes >= 20px
    - Revisar en dispositivo real o Game view a resolucion movil:
      - Auth: "or" divider, "Don't have an account?", "Forgot password?"
      - DailyRewards: "Unlocks in X days"
      - CashTournamentCreate: "Creation fee: $X.XX"
    - Si algun texto es ilegible → aumentar fontSize o fontSizeMin

  P2-13: CashWallet transaction history layout
    - Abrir CashBattle > Wallet con historial de transacciones
    - Verificar que TODOS los items muestran fecha + estado (no solo algunos)
    - Si hay items sin fecha → estandarizar layout en WalletUIBuilder

  P2-14: TMP fallback Unicode checkmark
    - Abrir DailyMissions o CashBattleOnboarding
    - Verificar que el caracter ✓ se renderiza (no cuadrado vacio)
    - Si hay warning "Unicode character not found" → verificar TMP fallback chain

  P2-15: Achievements icon sprites en DetailPanel/RewardCelebration
    - Abrir Achievements > click en un logro completado
    - Verificar que DetailPanelBlocker muestra el icono real (no rectangulo amarillo)
    - Click Claim → verificar que RewardCelebration muestra icono + gem sprite
    - Si se ven cuadrados amarillos/cyan → verificar Resources.Load path

#### =================================================================== ####

ANIMATION AUDIT - PENDIENTE (Post-V41)

  Todo el codigo ya existe. Solo falta ejecutar en Unity Editor:

  T1a: Repair ANIMATION_MANAGERS
    - Menu: DigitPark/Polish/Animation Batch/FASE 1: Repair All ANIMATION_MANAGERS
    - Repara estructura ---ANIMATION_MANAGERS--- en todas las escenas
    - Herramienta: AnimationManagersRepairTool.cs

  T1b: Apply Button3D + SimplePulse a todas las escenas
    - Menu: DigitPark/Polish/Animation Batch/APPLY ALL ANIMATIONS TO ALL SCENES
    - Convierte botones planos a Button3D (face/shadow/glow con depth)
    - Agrega SimplePulse a CTAs principales
    - Herramienta: AnimationSystemBatchSetup.cs

  T1c: Setup ButtonEffects en 40 escenas
    - Menu: DigitPark/Polish/Effects/Setup All Scenes
    - Conecta ButtonEffects (press/release micro-interactions) a todos los botones UI
    - Herramienta: EffectsSetup.cs

  T1d: Wiring BadgeAnimator en MainMenu + Navigation — COMPLETADO
    - BadgeAnimator ya estaba en MainMenuUIBuilder (linea 268, autoPulse=true)
    - MainMenuManager.cs actualizado: usa badgeAnim.Show/Hide/PlayUpdate()
      en vez de DOTween inline manual

  ORDEN RECOMENDADO: T1a primero (repair), luego T1b (3D buttons), luego T1c (effects)

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
