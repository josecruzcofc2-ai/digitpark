# Integración de Skillz en digitPark

## Resumen

Este documento explica cómo completar la integración de Skillz para torneos con dinero real.

## Archivos Creados

| Archivo | Descripción |
|---------|-------------|
| `SkillzManager.cs` | Manager principal, inicializa SDK y maneja torneos |
| `SkillzDelegate.cs` | Recibe callbacks del SDK de Skillz |
| `SkillzGameController.cs` | Integra Skillz con el GameManager |
| `PrizeDistributionConfig.cs` | Configuración de distribución de premios (80/20) |

## Pasos para Completar la Integración

### 1. Crear Cuenta de Desarrollador Skillz

1. Ir a: https://developers.skillz.com
2. Crear cuenta
3. Crear nuevo juego "digitPark"
4. Obtener el **Game ID**
5. Actualizar `SkillzManager.cs` con tu Game ID

### 2. Descargar e Instalar el SDK

**Opción A: Unity Asset Store**
1. Buscar "Skillz" en Asset Store
2. Importar el paquete

**Opción B: Portal de Skillz**
1. Descargar desde el portal de desarrolladores
2. Importar el .unitypackage

### 3. Configurar el SDK

Después de instalar el SDK, descomentar las líneas marcadas con `// SKILLZ_SDK` en:

- `SkillzManager.cs`
- `SkillzDelegate.cs`
- `SkillzGameController.cs`

### 4. Configurar en Unity

1. **Crear ScriptableObject de Premios:**
   - Assets > Create > DigitPark > Skillz > Prize Distribution Config
   - Configurar porcentajes (80/20 por defecto)

2. **Crear GameObject SkillzManager:**
   - Crear empty GameObject "SkillzManager"
   - Agregar componente `SkillzManager`
   - Agregar componente `SkillzDelegate`
   - Asignar el Prize Distribution Config
   - Configurar Game ID

3. **Agregar SkillzGameController a la escena Game:**
   - En la escena Game, agregar `SkillzGameController`
   - Asignar referencia al GameManager

### 5. Modificar GameManager

En `GameManager.cs`, agregar al final del juego:

```csharp
// Al terminar una partida
if (SkillzGameController.Instance != null && SkillzGameController.Instance.IsSkillzMatch)
{
    SkillzGameController.Instance.EndMatch(finalTime, true);
}
```

### 6. Agregar Botón de Skillz en MainMenu

Agregar botón "Torneos con Premios" que llame a:

```csharp
SkillzManager.Instance.LaunchSkillz();
```

## Distribución de Premios

Por defecto está configurado así:

```
Pozo Total: $100 (ejemplo: 10 jugadores x $10)
├── Skillz (50%): $50
└── Disponible (50%): $50
    ├── Ganadores (80%): $40
    │   ├── 1er lugar (50%): $20
    │   ├── 2do lugar (30%): $12
    │   └── 3er lugar (20%): $8
    └── Desarrolladores (20%): $10
```

## Flujo del Usuario

```
1. Usuario abre digitPark
2. Presiona "Torneos con Premios"
3. Skillz UI se abre (lista de torneos)
4. Usuario elige torneo y paga entrada
5. Skillz carga la escena Game
6. Usuario juega
7. Al terminar, score se reporta a Skillz
8. Skillz muestra resultados y ranking
9. Si gana, el dinero se deposita en su cuenta Skillz
```

## Requisitos para Dinero Real

Antes de que Skillz active dinero real:

- [ ] 1,000+ partidas completadas en moneda Z
- [ ] Rating 4+ estrellas en App Store
- [ ] SDK actualizado
- [ ] Skillz Random implementado
- [ ] Sin ads de terceros

## Testing

1. Usar `SkillzEnvironment.Sandbox` para pruebas
2. No se cobra dinero real en Sandbox
3. Cuando esté listo, cambiar a `SkillzEnvironment.Production`

## Soporte

- Documentación Skillz: https://docs.skillz.com
- Soporte: support@skillz.com
