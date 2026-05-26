---
name: testing-guide
description: >-
  Use when writing, modifying, or running tests for SimuladorRedes IDEUM.
  Covers test project structure, how to write EditMode tests, test patterns
  (AAA), mocking, what to test per class, and how to run tests from CLI
  and Editor. Essential for the tester agent and for any agent writing
  new tests.
---

# Skill: Testing Guide for SimuladorRedes

## 📁 Estructura de Tests

Los tests están en `Assets/Editor/Tests/` y son 209 tests EditMode:

```
Assets/Editor/Tests/
├── Network/
│   ├── TestIPValidation.cs           # 18 tests
│   ├── TestRoutingTable.cs           # 14 tests
│   ├── TestRoutePersistence.cs       # 3 tests (B1)
│   └── TestDiscToRouteIntegration.cs # 10 tests (B4)
├── Tangible/
│   └── TestRouteBuilderState.cs      # 15 tests
└── Simulation/
    ├── TestScoringSystem.cs          # 19 tests
    ├── TestBestRouteActivity.cs      # 12 tests
    ├── TestSceneCleanupService.cs    # 4 tests
    └── TestPredefinedScenarios.cs    # 13 tests
```

**No hay PlayMode tests** — todos son EditMode (no requieren escena).

## 🧪 Cómo escribir tests

### Patrón AAA (Arrange-Act-Assert)

```csharp
using NUnit.Framework;
using SimRedes.Network;

public class TestMiClase
{
    [Test]
    public void Metodo_ResultadoEsperado()
    {
        // Arrange
        var obj = new MiClase();

        // Act
        var result = obj.Metodo(param);

        // Assert
        Assert.IsTrue(result);
    }
}
```

### Test con múltiples casos

```csharp
[TestCase("192.168.1.1", true)]
[TestCase("999.999.999.999", false)]
[TestCase("", false)]
[TestCase(null, false)]
public void IsValidIP_VariosCasos_RetornaEsperado(string ip, bool esperado)
{
    var result = IPValidation.IsValidIP(ip);
    Assert.AreEqual(esperado, result);
}
```

### Test que verifica excepciones

```csharp
[Test]
public void Metodo_ParametroInvalido_LanzaExcepcion()
{
    Assert.Throws<System.ArgumentNullException>(() => {
        obj.Metodo(null);
    });
}
```

## 📐 Convenciones para tests

| Regla | Estándar |
|-------|----------|
| **Clase** | `Test[NombreClase]` — ej: `TestIPValidation` |
| **Método** | `[Metodo]_[Escenario]_[Resultado]` — ej: `IsValidIP_IPv4Valida_RetornaTrue` |
| **Namespace** | Usar los mismos que el código bajo test |
| **Arrange** | Crear mocks/objetos necesarios |
| **Act** | Ejecutar el método a testear (1 línea) |
| **Assert** | Verificar el resultado esperado |

## 🎯 Qué testear

### 1. Validación (IPValidation)
- IPs válidas e inválidas (formato, rangos, null, vacío)
- Máscaras de subred válidas e inválidas
- Cálculos: prefix length, network address, broadcast, gateway
- Comparación: misma subred

### 2. Tablas (RoutingTable)
- Agregar rutas estáticas, RIP, OSPF
- FindBestRoute: longest prefix match, métrica, default, sin match
- GetAllEntries: debe devolver copia, no referencia
- Clear: debe limpiar todo

### 3. Estado transitorio (RouteBuilderState)
- IsComplete: varios campos, strings vacíos, protocol no requerido
- ApplyToRouter: ruta completa, protocol override, router inexistente
- Reset: no borra Protocol

## 🔧 Cómo ejecutar tests

### Desde Unity Editor
Window → General → Test Runner → EditMode → Run All

### Desde CLI (Mac) — probar varias versiones de Unity
```bash
# Encontrar Unity instalado
UNITY_PATH=$(ls /Applications/Unity/Hub/Editor/6000.4.5f1/Unity.app/Contents/MacOS/Unity \
  2>/dev/null || ls /Applications/Unity/Hub/Editor/6000.*/Unity.app/Contents/MacOS/Unity \
  2>/dev/null || ls /Applications/Unity/Hub/Editor/*/Unity.app/Contents/MacOS/Unity \
  2>/dev/null || echo "")

# Todos los tests EditMode (Unity 6000 — a veces -quit mata los tests)
$UNITY_PATH \
  -runTests -testPlatform EditMode \
  -projectPath /Users/sebastianmarin/ProyectoSimRedes/SimuladorRedes \
  -logFile /tmp/unity-test-log.txt \
  -testResults /tmp/test-results.xml \
  -batchmode -quit

# Si no se generan resultados, probar SIN -quit:
$UNITY_PATH \
  -runTests -testPlatform EditMode \
  -projectPath /Users/sebastianmarin/ProyectoSimRedes/SimuladorRedes \
  -logFile /tmp/unity-test-log.txt \
  -testResults /tmp/test-results.xml \
  -batchmode
```

### Tests específicos con filtro
```bash
$UNITY_PATH -runTests -testPlatform EditMode \
  -projectPath /Users/sebastianmarin/ProyectoSimRedes/SimuladorRedes \
  -logFile - -testResults /tmp/test-results.xml \
  -batchmode -quit \
  -testFilter TestDiscToRouteIntegration
```

## ⚠️ Reglas importantes

1. **Tests en `Assets/Editor/Tests/`** — usar esta ubicación siempre
2. **No usar asmdef** — los tests de Editor tienen visibilidad total a Assembly-CSharp
3. **No usar PlayMode** — todos los tests deben ser EditMode
4. **No depender de escenas** — los tests deben funcionar sin escena
5. **Usar `[TestCase]`** para múltiples variantes del mismo test
6. **Los tests no deben tener efectos secundarios** — no modificar estado global
7. **Un test por concepto** — no mezclar múltiples asserts no relacionados
8. **⚠️ `Awake()` no se ejecuta automáticamente en EditMode** — al hacer `AddComponent<T>()` en un test, el método `Awake()` NO se invoca (comportamiento documentado de Unity Test Framework). Si tu clase depende de `Awake()` para inicialización, debes invocarlo manualmente vía reflexión en `[SetUp]`:

```csharp
[SetUp]
public void Setup()
{
    go = new GameObject();
    component = go.AddComponent<MyComponent>();
    var awakeMethod = typeof(MyComponent).GetMethod("Awake",
        System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
    if (awakeMethod != null)
        awakeMethod.Invoke(component, null);
}
```

Este patrón se usa en `TestSceneCleanupService` y `TestPredefinedScenarios`.

## 📊 Cobertura actual

| Suite | Tests | Archivo |
|-------|:-----:|---------|
| IPValidation | 18 | `Network/TestIPValidation.cs` |
| RoutingTable | 14 | `Network/TestRoutingTable.cs` |
| RouteBuilderState | 15 | `Tangible/TestRouteBuilderState.cs` |
| RoutePersistence | 3 | `Network/TestRoutePersistence.cs` |
| DiscToRouteIntegration | 10 | `Network/TestDiscToRouteIntegration.cs` |
| TopologyManager | 32 | `Network/TestTopologyManager.cs` |
| DynamicRoutingProtocol | 16 | `Simulation/TestDynamicRoutingProtocol.cs` |
| ActivityLoader | 20 | `Simulation/TestActivityLoader.cs` |
| DiscEventHandler | 18 | `Tangible/TestDiscEventHandler.cs` |
| TangibleBridge | 15 | `Tangible/TestTangibleBridge.cs` |
| ScoringSystem | 19 | `Simulation/TestScoringSystem.cs` |
| BestRouteActivity | 12 | `Simulation/TestBestRouteActivity.cs` |
| SceneCleanupService | 4 | `Simulation/TestSceneCleanupService.cs` |
| PredefinedScenarios | 13 | `Simulation/TestPredefinedScenarios.cs` |
| **Total** | **209** | |

## ✍️ Cómo agregar un test nuevo

1. Ir a `Assets/Editor/Tests/` en la carpeta correspondiente
2. Agregar archivo `Test[NombreClase].cs`
3. Usar `using NUnit.Framework;`
4. Usar `using SimRedes.[Namespace];`
5. Escribir tests con patrón AAA
6. Verificar que compile y pase desde Test Runner
