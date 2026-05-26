---
name: build-and-deploy
description: >-
  Use when building or deploying the SimuladorRedes IDEUM project. Covers
  scene selection, build for IDEUM (Windows) vs PC testing, TangibleEngine
  Editor vs Runtime modes, and the black-screen fix. Also use when the user
  asks about deployment, build errors, or IDEUM hardware setup.
---

# Skill: Build & Deploy

## ⚠ Escena principal (causa #1 de pantalla negra)

| Escena | Tiene SceneSetup | Build |
|--------|:----------------:|:-----:|
| `Assets/Main.unity` | ✅ Sí | **Usar esta** |
| `Assets/Scenes/GetStarted_Scene.unity` | ❌ No | Ignorar |

`SceneSetup.Awake()` ejecuta `SetupScene()` → crea cámara ortográfica, canvas, menú principal.  
Si la build usa `GetStarted_Scene.unity`, `SceneSetup` nunca corre → **pantalla negra**.

### Fix

`ProjectSettings/EditorBuildSettings.asset` debe tener `Assets/Main.unity` como única escena activa:

```yaml
m_Scenes:
  - enabled: 1
    path: Assets/Main.unity
    guid: 04eec930076f14208b5997a6ef282c09
```

## Build para mesa IDEUM (discos físicos)

1. `DebugDiscSimulator.enableSimulation = false` (default)
2. File → Build Settings → Windows x86_64
3. Build → copiar carpeta a la mesa IDEUM
4. En la mesa debe estar corriendo el **TangibleEngine Windows service** (TCP puerto 4949)

## Build para PC normal (testing con teclado)

1. `DebugDiscSimulator.enableSimulation = true`
2. File → Build Settings → Windows x86_64
3. El TangibleEngine en Runtime intenta conectar a localhost:4949, falla silenciosamente → no afecta
4. Teclas: **1** Router, **2** Switch, **3** PC, **4** Enlace, **5** Fallo

## TangibleEngine: Editor vs Runtime

```csharp
// TangibleEngine.cs línea 327-335
if (_properties == null) {
#if UNITY_EDITOR
    _properties = EditorProperties;      // Modo Simulator (no necesita servicio)
#else
    _properties = RuntimeProperties;     // Modo Service (TCP localhost:4949)
#endif
}
```

- **Editor**: usa `TangibleProviderSimulator` → funciona sin hardware
- **Build**: usa `TangibleProviderService` → requiere el servicio IDEUM
- Si falla la conexión TCP → `OnFailedToConnect()` → no crashea, solo log
