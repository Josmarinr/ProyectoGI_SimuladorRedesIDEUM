# Skill: Code Documentation for SimuladorRedes IDEUM

## Propósito
Estandarizar y mantener la documentacion XML (`/// <summary>`) en todo el codigo C# del proyecto. Cada vez que se programa un cambio NUEVO, el desarrollador debe documentar los metodos/ clases tocadas. Cuando se toma una tarea de documentacion, se documentan TODOS los miembros publicos del archivo.

## Formato obligatorio

### Clases
```csharp
/// <summary>
/// [Breve descripcion de la responsabilidad de la clase.
/// 1-2 lineas. Explica QUE hace, no COMO.]
/// </summary>
public class MiClase : MonoBehaviour
```

### Metodos publicos
```csharp
/// <summary>
/// [Que hace el metodo, en que contexto llamarlo, efecto secundario si hay.]
/// </summary>
/// <param name="paramName">[Que representa, valores esperados.]</param>
/// <returns>[Que devuelve, o "n/a" si es void.]</returns>
public void MiMetodo(string paramName)
```

### Propiedades publicas
```csharp
/// <summary>
/// [Descripcion breve de la propiedad.]
/// </summary>
public string MiPropiedad { get; set; }
```

### Metodos privados (solo si son no-triviales, >5 lineas o logica compleja)
```csharp
/// <summary>
/// [Breve descripcion de la logica interna.]
/// </summary>
private void LogicaCompleja()
```

## Reglas de estilo

1. **Idioma**: Espanol (MX/AR consistente con el resto del proyecto).
2. **Formato**: XML doc comments (`///`), NO `//` ni `/* */`.
3. **Extensión**: 1-3 lineas. No repetir el nombre del metodo en la descripcion.
4. **Qué documentar**: QUE hace y POR QUE es necesario, no COMO lo hace internamente.
5. **@param**: Solo si el parametro no es obvio (ej: un string con formato especifico, un indice con rango).
6. **@returns**: Solo si no es void y el valor de retorno no es obvio.
7. **@see**: Usar para referencias cruzadas cuando un metodo delega en otro.
8. **Unity lifecycle**: `Awake()`, `Start()`, `Update()`, `OnDestroy()` NO necesitan doc a menos que hagan algo NO obvio (ej: suscribirse a eventos, inicializar sistemas externos).
9. **Propiedades auto-implementadas**: NO necesitan doc si el nombre es autodescriptivo. Si tienen logica en getter/setter, poner doc breve.
10. **Eventos**: Documentar cuando se invocan y quien deberia suscribirse.
11. **Enums**: Documentar cada valor si no es obvio.

## Ejemplos correctos

```csharp
/// <summary>
/// Agrega una ruta estatica a la tabla. Si ya existe una ruta identica, la omite.
/// </summary>
/// <param name="destination">Red destino en formato IPv4 (ej: "10.0.0.0").</param>
/// <param name="subnetMask">Mascara en formato IPv4 (ej: "255.0.0.0").</param>
/// <param name="nextHop">Direccion IPv4 del siguiente salto.</param>
/// <param name="outInterface">Nombre de la interfaz de salida (ej: "G0/0").</param>
public void AddStaticRoute(string destination, string subnetMask, string nextHop, string outInterface)
```

```csharp
/// <summary>
/// Busca la mejor ruta para una IP destino usando longest prefix match.
/// Si hay empate en prefijo, elige la de menor metrica.
/// </summary>
/// <param name="destinationIP">IP destino a resolver.</param>
/// <returns>Entrada de ruta con mejor coincidencia, o null si no hay ruta.</returns>
public RoutingEntry FindBestRoute(string destinationIP)
```

## Que NO hacer

- ❌ `/// <summary>Gets or sets the value.</summary>` — Esto no aporta informacion.
- ❌ Documentar `private int _cache` con `///` — Solo si es necesario, usar `//` comentario inline.
- ❌ Poner doc en properties triviales como `public int Id { get; set; }` — El nombre ya lo dice.
- ❌ Mezclar `///` con comentarios `//` en el mismo bloque.
- ❌ Documentar override de metodos virtuales de Unity (ej: `Start()`, `Update()`) a menos que tengan logica no obvia.

## Checklist para documenter

Al documentar un archivo:
1. Leer el archivo COMPLETO para entender su proposito.
2. Identificar: clase(s) publicas, metodos publicos, propiedades publicas, enums publicos.
3. Para cada uno, escribir `/// <summary>` siguiendo las reglas arriba.
4. NO modificar ninguna linea de codigo existente — solo INSERTAR documentacion.
5. Verificar que el archivo compila (revision mental, no hay sintaxis rota).
6. Reportar: cuantos metodos/qué cobertura tenia antes y despues.

## Prioridad de archivos para documentar

1. 🔴 `TopologyManager.cs` — Clase central del proyecto (518 lineas, 29 metodos)
2. 🔴 `SceneSetup.cs` — Bootstrap de escena (480 lineas, 19 metodos)
3. 🟡 `UIPanelFactory.cs`, `ActivityPanelFactory.cs`, `ConfigPanelFactory.cs` — Factories UI
4. 🟡 `UIComponents.cs` — Componentes UI base
5. 🟡 `ActivityLoader.cs` — Cargador de actividades
6. 🟢 `RoutingTable.cs`, `IPValidation.cs`, `NetworkNode.cs` — Clases de red base
7. 🟢 `ACLManager.cs`, `NATManager.cs`, `VLANManager.cs` — Modulos de red avanzados
8. 🟢 `DynamicRoutingProtocol.cs`, `DynamicRoutingActivity.cs` — Enrutamiento dinamico (ya tienen parcial)
9. 🟢 Actividades: `BestRouteActivity.cs`, `BuildTopologyActivity.cs`, `FindFaultActivity.cs`, `StaticRoutingActivity.cs`, `RoutingTablesActivity.cs`
10. 🟢 Tangible: `TangibleBridge.cs`, `TangibleDiscManager.cs`, `DebugDiscSimulator.cs`, `DiscEventHandler.cs`
11. 🟢 Resto: `ScoringSystem.cs`, `SceneCleanupService.cs`, `MenuNavigator.cs`, `RoutingProtocols.cs`, `DiscConfiguration.cs`
