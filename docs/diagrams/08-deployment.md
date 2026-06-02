# DD-01: Diagrama de Despliegue

> **Propósito**: Mostrar la arquitectura de despliegue del sistema, incluyendo la mesa IDEUM física, los componentes de software y sus conexiones.

```mermaid
graph TB
    subgraph "Mesa IDEUM 55\""
        PANTALLA[Display Táctil<br/>1920x1080p<br/>Multi-touch]
        DISCS[Discos Físicos PUCs<br/>6 discos detectables<br/>por RFID/patrones]
        TE_SERVICE[TangibleEngine Service<br/>Windows Service<br/>TCP Puerto 4949]
    end

    subgraph "PC Windows (embebido en mesa)"
        subgraph "SimuladorRedes App"
            UNITY[Unity App<br/>Windows x86_64<br/>6000.4.5f1]
            CANVAS[Canvas 4096x2160<br/>UI ScaleWithScreenSize]
            TE_CLIENT[TangibleEngine Client<br/>SDK embebido]
            SCENES[Escenas<br/>Main.unity (principal)<br/>GetStarted_Scene.unity (backup)]
        end
        BUILD_FOLDER[Build Output<br/>SimuladorRedes.exe + Data/]
    end

    subgraph "PC Desarrollo (macOS)"
        UNITY_EDITOR[Unity Editor<br/>6000.4.5f1<br/>Platform: Windows]
        SOURCE[Código Fuente<br/>Assets/Scripts/]
        TESTS[50 Tests EditMode<br/>NUnit 3.x]
        AGENTS[opencode Agents<br/>GitHub Copilot-like]
        GIT[Git Repository<br/>SimuladorRedes]
    end

    subgraph "Build Output"
        BUILD_IDEUM[Build IDEUM<br/>enableSimulation=false<br/>TE Service mode]
        BUILD_PC[Build PC Testing<br/>enableSimulation=true<br/>Debug teclado]
    end

    %% Conexiones físicas (mesa)
    DISCS -->|RFID / Pattern Recognition| TE_SERVICE
    TE_SERVICE -->|TCP localhost:4949| TE_CLIENT
    TE_CLIENT --> UNITY
    PANTALLA -->|HDMI| UNITY

    %% Build y desarrollo
    UNITY_EDITOR -->|Build Settings| BUILD_IDEUM
    UNITY_EDITOR -->|Build Settings| BUILD_PC
    BUILD_IDEUM -->|Copiar carpeta| BUILD_FOLDER
    SOURCE -->|Compila en| UNITY_EDITOR
    TESTS -->|Verifica| SOURCE
    AGENTS -->|Automatiza| UNITY_EDITOR
    GIT -->|Versiona| SOURCE
```

## Modos de Operación

| Modo | Uso | enableSimulation | TangibleEngine |
|------|-----|:----------------:|:--------------:|
| **Producción** (mesa IDEUM) | Estudiantes con discos físicos | `false` | Service TCP:4949 |
| **Debug** (PC con teclado) | Testing sin discos | `true` | Fallo silencioso |
| **Editor** (Unity Play) | Desarrollo | `true` (default `false`) | Deshabilitado |

## Configuración de Escenas

| Escena | SceneSetup | Uso en Build |
|--------|:----------:|:------------:|
| `Assets/Main.unity` | Si | **Principal** — indice 0 en Build Settings |
| `Assets/Scenes/GetStarted_Scene.unity` | No | Backup — **NO incluir en build** |

## Build Pipeline

```
1. Abrir Assets/Main.unity
2. File → Build Settings → Windows x86_64
3. Verificar escena correcta en Scenes In Build
4. Elegir modo: IDEUM (prod) o PC (debug)
   - IDEUM: DebugDiscSimulator.enableSimulation = false
   - PC: DebugDiscSimulator.enableSimulation = true
5. Build → copiar ejecutable al destino
```

## Archivos Relacionados

| Archivo | Propósito |
|---------|-----------|
| `Assets/Scripts/Tangible/DebugDiscSimulator.cs` | Toggle `enableSimulation` |
| `Assets/Scripts/Simulation/SceneSetup.cs` | Configuración de escena al despertar |
| `Assets/Main.unity` | Escena principal del build |
| `Assets/Scenes/GetStarted_Scene.unity` | Escena de respaldo |
