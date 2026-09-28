# DS-04: Secuencia — Navegación de Menús

> **Propósito**: Mostrar el flujo de navegación desde el menú principal hasta las actividades y el ciclo de vuelta (GoBackToMainMenu).

```mermaid
sequenceDiagram
    participant User as Estudiante
    participant SS as SceneSetup
    actor Menu as MenuPrincipal
    participant AL as ActivityLoader
    participant SCS as SceneCleanupService
    participant MNV as MenuNavigator
    participant SIM as Simulación

    User->>SS: App inicia
    SS->>SS: Awake()
    SS->>SS: SetupResolution() → 1920x1080
    SS->>SS: SetupCamera() → Ortho, size 540
    SS->>SS: SetupCanvas() → 4096x2160, Expand
    SS->>Menu: CreateMainMenu()
    Note over Menu: 5 opciones

    alt Opción 1: INICIAR SIMULACIÓN
        User->>Menu: Click "INICIAR SIMULACIÓN"
        Menu->>SS: StartSimulation()
        SS->>SS: DestroyMainMenu() [DestroyImmediate]
        SS->>SS: SetupManagers()
        Note over SS: Crea TopologyManager, TangibleDiscManager,<br/>5 Controllers, SimulationControls, etc.
        SS->>SS: CreateVisualizer()
        SS->>AL: StartSimulation(canvas)
        AL->>AL: CreateSimulationHUDPanel()
        AL->>SIM: Modo libre, discos 1-6
    end

    alt Opción 2: ACTIVIDADES
        User->>Menu: Click "ACTIVIDADES"
        Menu->>SS: ShowActivities()
        SS->>SS: DestroyMainMenu()
        SS->>SS: CreateActivitiesPanel()
        Note over SS: 7 botones + VOLVER

        User->>SS: Selecciona actividad N
        SS->>SS: SetupManagers()
        SS->>SS: CreateVisualizer()
        SS->>AL: SelectActivity(N)
        AL->>AL: Añade componente actividad
        AL->>SIM: Actividad académica ejecutándose
    end

    alt Opción 3: PRUEBAS Y CONEXIONES
        User->>Menu: Click "PRUEBAS Y CONEXIONES"
        Menu->>SS: ShowConnectivity()
        SS->>SS: DestroyMainMenu()
        SS->>AL: EnsureManagersForConnectivity()
        AL->>AL: CreateConnectivityPanel()
        Note over SIM: Modo pruebas sin SimulationControls
    end

    alt Opción 4: CÓMO USAR
        User->>Menu: Click "CÓMO USAR"
        Menu->>SS: ShowInstructions()
        SS->>SS: DestroyMainMenu()
        SS->>MNV: SetupPanel(panel, onEscape→CreateMainMenu)
        Note over MNV: Panel 2 columnas con instrucciones
        User->>MNV: Presiona ESC o VOLVER
        MNV->>SS: CreateMainMenuPublic()
    end

    alt Opción 5: SALIR
        User->>Menu: Click "SALIR"
        Menu->>SS: ExitApplication()
        Note over SS: Application.Quit()
    end

    Note over User,SIM: === CICLO DE VUELTA (GoBackToMainMenu) ===
    User->>SIM: Presiona ESC o VOLVER
    SIM->>SCS: GoBackToMainMenu(canvas, callback)
    SCS->>SCS: Destruye managers (TopologyManager, etc.)
    SCS->>SCS: Destruye paneles (por nombre)
    SCS->>SS: callback → CreateMainMenuPublic()
    SS->>Menu: CreateMainMenu()
    Note over Menu: Todo limpio, menú reconstruido
```

## Mapa de Navegación

```
                    ┌─────────────────────┐
                    │   MENÚ PRINCIPAL     │
                    │ 1. INICIAR SIMULACIÓN│
                    │ 2. ACTIVIDADES       │
                    │ 3. PRUEBAS Y CONEX.  │
                    │ 4. CÓMO USAR         │
                    │ 5. SALIR             │
                    └──────┬──────┬───────┘
                           │      │
          ┌────────────────┘      └────────────────┐
          ▼                                          ▼
  ┌──────────────┐                          ┌──────────────┐
  │ ACTIVIDADES   │                          │ SIMULACIÓN   │
  │ 0-6 botones   │                          │ Topología    │
  │ VOLVER → Menú │                          │ PING, VLAN   │
  └──────┬───────┘                          │ ESC → Menú   │
         │                                   └──────────────┘
         ▼
  ┌──────────────┐
  │ Actividad N   │
  │ Panel + lógica │
  │ ESC → Menú    │
  └──────────────┘
```

## Archivos Relacionados

| Archivo | Rol |
|---------|-----|
| `Assets/Scripts/Simulation/SceneSetup.cs` | Orquestador de navegación |
| `Assets/Scripts/Simulation/SceneCleanupService.cs` | Limpieza y retorno |
| `Assets/Scripts/Simulation/ActivityLoader.cs` | Carga de actividades |
| `Assets/Scripts/UI/MenuNavigator.cs` | Navegación por teclado |
| `Assets/Scripts/UI/UIPanelFactory.cs` | Creación de todos los paneles |
| `Assets/Scripts/Simulation/SimulationControls.cs` | ESC unificado |
