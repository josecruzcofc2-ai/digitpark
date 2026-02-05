# Pasos manuales en Unity - Post sesion limpieza

## CRITICOS (se rompen si no se hacen)

### 1. FlashTap.unity - Re-asignar tapButton
- Abrir escena: `Assets/_Project/Scenes/Games/FlashTap.unity`
- Seleccionar el Canvas (tiene `FlashTapManager`)
- En el Inspector, buscar el campo `tapButton` (esta vacio/None)
- Arrastrar el boton correcto de gameplay al campo `tapButton`
- Guardar escena (Ctrl+S)

### 2. CashWallet.unity - Re-asignar backButton
- Abrir escena: `Assets/_Project/Scenes/CashBattle/CashWallet.unity`
- Seleccionar el Canvas (tiene `CashWalletManager`)
- En el Inspector, buscar el campo `backButton` (esta vacio/None)
- Arrastrar el `BackButtonGold` (ya existe en la jerarquia) al campo `backButton`
- Guardar escena (Ctrl+S)

## RECOMENDADOS (para ver cambios nuevos)

### 3. Shop.unity - Regenerar UI con seccion de Temas
- Abrir escena: `Assets/_Project/Scenes/Monetization/Shop.unity`
- Menu: `DigitPark > UI Builders > Monetization > Shop Premium (Clash Royale Style)`
- Confirmar en el dialogo
- Luego ejecutar: `DigitPark > Tools > Auto Assign > Shop Manager References`
- Guardar escena (Ctrl+S)
- **Nota**: Esto regenera toda la UI. La escena actual ya NO tiene cofres pero tampoco tiene la nueva seccion de Temas.

### 4. Scores.unity - Regenerar UI con sample entries
- Abrir escena: `Assets/_Project/Scenes/Social/Scores.unity`
- Menu: `DigitPark > UI Builders > Social > Scores` (o similar)
- Confirmar en el dialogo
- Luego ejecutar: `DigitPark > Tools > Auto Assign > Leaderboard References`
- Guardar escena (Ctrl+S)
- **Nota**: Esto agrega 5 filas de ejemplo con medallas (oro/plata/bronce).

## VERIFICACION (deberian funcionar solos)

### 5. Reimportar assets
- Al abrir Unity, detectara automaticamente los archivos eliminados
- Los GUIDs en las escenas ya fueron migrados a los iconos originales
- No deberia haber sprites rotos (iconos morados "missing")
- Si ves algun sprite roto, verificar en estas escenas:
  - CashBattle1v1, CashHistory, CashTournaments (iconos de juegos)
  - Settings (iconos de idioma y temas)

### 6. Splash Screen
- Verificar en: `Edit > Project Settings > Player > Splash Image`
- Debe aparecer LogoDigitPark.png con duracion 2 segundos
- Estilo: logo claro sobre fondo oscuro

## Archivos eliminados en esta sesion

### Iconos duplicados (Games/CashBattle/)
- DigitRushIcon.png (+.meta)
- FlashTapIcon.png (+.meta)
- MemoryPairsIcon.png (+.meta)
- OddOneOutIcon.png (+.meta)
- QuickMathIcon.png (+.meta)
- CognitiveSprintIcon.png (+.meta)
- CashBattle.meta (carpeta)

### Iconos genericos no-neon (UI/)
- earth.png (+.meta) → reemplazado por EarthIconNeon en Settings
- paint.png (+.meta) → reemplazado temporalmente por EarthIconNeon en Settings
- TablaColores.png (+.meta) → sin referencias, eliminado limpio
- themes.png (+.meta) → sin referencias, eliminado limpio

## Archivos C# modificados
- `Scripts/Editor/ShopPremiumUIBuilder.cs` - Nueva seccion ThemesSection
- `Scripts/Editor/ScoresUIBuilder.cs` - Nuevo metodo CreateSampleEntries

## Escenas modificadas
- Shop.unity - 35 GameObjects de cofres eliminados (4,964 lineas)
- CashBattle1v1.unity - GUIDs migrados (iconos de juegos)
- CashHistory.unity - GUIDs migrados (iconos de juegos)
- CashTournaments.unity - GUIDs migrados (iconos de juegos)
- CashWallet.unity - BackButton cyan eliminado
- FlashTap.unity - BackButton prefab eliminado
- Settings.unity - GUIDs de earth/paint reemplazados por EarthIconNeon
