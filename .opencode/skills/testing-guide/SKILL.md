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

## Estructura de Tests

316+ tests EditMode en Assets/Editor/Tests/. Sin PlayMode tests.

```
Assets/Editor/Tests/
  Network/   TestIPValidation(18) TestRoutingTable(18) TestRoutePersistence(3) TestDiscToRouteIntegration(10) TestTopologyManager(32) TestARPTable(13) TestVLANManager(18) TestACLManager(29) TestNATManager(25)
  Tangible/  TestRouteBuilderState(15) TestDiscEventHandler(18) TestTangibleBridge(15)
  Simulation/ TestScoringSystem(19) TestBestRouteActivity(12) TestSceneCleanupService(4) TestPredefinedScenarios(13) TestDynamicRoutingProtocol(24) TestActivityLoader(20)
  UI/        TestUIPanelFactory(7) TestActivityPanelFactory(8) TestConfigPanelFactory(7)
```

## Como escribir tests

### Patron AAA (Arrange-Act-Assert)
```csharp
using NUnit.Framework;
using SimRedes.Network;

public class TestMiClase
{
    [Test]
    public void Metodo_ResultadoEsperado() {
        // Arrange
        var obj = new MiClase();
        // Act
        var result = obj.Metodo(param);
        // Assert
        Assert.IsTrue(result);
    }
}
```

### Multiples casos con TestCase
```csharp
[TestCase("192.168.1.1", true)]
[TestCase("999.999.999.999", false)]
[TestCase("", false)]
[TestCase(null, false)]
public void IsValidIP_VariosCasos_RetornaEsperado(string ip, bool esperado) {
    var result = IPValidation.IsValidIP(ip);
    Assert.AreEqual(esperado, result);
}
```

### Excepciones
```csharp
Assert.Throws<ArgumentNullException>(() => obj.Metodo(null));
```

### Regla IMPORTANTE: Awake() en EditMode
Al hacer AddComponent<T>() en un test, Awake() NO se invoca automaticamente. Invocarlo manualmente:
```csharp
[SetUp]
public void Setup() {
    go = new GameObject();
    component = go.AddComponent<MyComponent>();
    var awakeMethod = typeof(MyComponent).GetMethod("Awake",
        BindingFlags.Instance | BindingFlags.NonPublic);
    awakeMethod?.Invoke(component, null);
}
```

## Convenciones

| Regla | Estandar |
|-------|----------|
| Clase | Test[NombreClase] |
| Metodo | [Metodo]_[Escenario]_[Resultado] |
| Namespace | Mismos que el codigo bajo test |
| Ubicacion | Assets/Editor/Tests/ siempre |
| No asmdef | Tests tienen visibilidad total a Assembly-CSharp |
| No PlayMode | Todos EditMode |
| No depender escenas | Funcionar sin escena |
| Sin efectos secundarios | No modificar estado global |

## Ejecucion

### CLI
```bash
UNITY=$(ls /Applications/Unity/Hub/Editor/6000.4.5f1/Unity.app/Contents/MacOS/Unity 2>/dev/null || echo "")
$UNITY -runTests -testPlatform EditMode -projectPath /Users/sebastianmarin/ProyectoSimRedes/SimuladorRedes -testResults /tmp/test-results.xml -logFile /tmp/unity-test-log.txt -batchmode -quit
# Suite especifica: anyadir -testFilter TestFoo
```

Problema conocido: -quit a veces mata tests en Unity 6000. Si no hay resultados, probar sin -quit.

## Cobertura actual (316+ tests)

| Suite | Tests | Suite | Tests |
|-------|:-----:|-------|:-----:|
| TestIPValidation | 18 | TestRoutingTable | 18 |
| TestRouteBuilderState | 15 | TestRoutePersistence | 3 |
| TestDiscToRouteIntegration | 10 | TestTopologyManager | 32 |
| TestDynamicRoutingProtocol | 24 | TestActivityLoader | 20 |
| TestDiscEventHandler | 18 | TestTangibleBridge | 15 |
| TestScoringSystem | 19 | TestBestRouteActivity | 12 |
| TestSceneCleanupService | 4 | TestPredefinedScenarios | 13 |
| TestUIPanelFactory | 7 | TestActivityPanelFactory | 8 |
| TestConfigPanelFactory | 7 | TestARPTable | 13 |
| TestVLANManager | 18 | TestACLManager | 29 |
| TestNATManager | 25 | | |
| **Total** | **~316+** | | |
