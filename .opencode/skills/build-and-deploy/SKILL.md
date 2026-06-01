---
name: build-and-deploy
description: >-
  Use when building or deploying the SimuladorRedes IDEUM project. Covers
  scene selection, build for IDEUM (Windows) vs PC testing, TangibleEngine
  Editor vs Runtime modes, and the black-screen fix. Also use when the user
  asks about deployment, build errors, or IDEUM hardware setup.
---

# Skill: Build & Deploy

## Escena principal (causa #1 de pantalla negra)

| Escena | Tiene SceneSetup | Build |
|--------|:----------------:|:-----:|
| Assets/Main.unity | Si | USAR ESTA |
| Assets/Scenes/GetStarted_Scene.unity | No | IGNORAR |

SceneSetup.Awake() ejecuta SetupScene() -> crea camara, canvas, menu principal.
Si build usa GetStarted_Scene.unity, SceneSetup nunca corre -> pantalla negra.

### Fix
ProjectSettings/EditorBuildSettings.asset debe tener Assets/Main.unity como unica escena activa.

## Build para mesa IDEUM (discos fisicos)
1. DebugDiscSimulator.enableSimulation = false (default)
2. File -> Build Settings -> Windows x86_64
3. Build -> copiar carpeta a la mesa IDEUM
4. Mesa debe tener TangibleEngine Windows service corriendo (TCP puerto 4949)

## Build para PC normal (testing con teclado)
1. DebugDiscSimulator.enableSimulation = true
2. File -> Build Settings -> Windows x86_64
3. TangibleEngine en Runtime intenta conectar localhost:4949, falla silenciosamente
4. Teclas: 1 Router, 2 Switch, 3 PC, 4 Enlace, 5 Fallo

## TangibleEngine: Editor vs Runtime

```csharp
// TangibleEngine.cs linea 327-335
if (_properties == null) {
#if UNITY_EDITOR
    _properties = EditorProperties;      // Modo Simulator (no necesita servicio)
#else
    _properties = RuntimeProperties;     // Modo Service (TCP localhost:4949)
#endif
}
```

- Editor: TangibleProviderSimulator -> funciona sin hardware
- Build: TangibleProviderService -> requiere servicio IDEUM
- Falla conexion TCP -> OnFailedToConnect() -> no crashea, solo log
