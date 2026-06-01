---
name: agent-workflow
description: >-
  Use when coordinating multi-agent workflows or reporting results in
  SimuladorRedes IDEUM. Covers how agents should communicate, format
  responses, escalate issues, and document progress. Essential for the
  main coordinator agent and for any agent reporting back to the user
  or to another agent.
---

# Skill: Agent Workflow & Communication

## Flujo de trabajo

```
Usuario -> main -> architect -> programmer -> reviewer -> tester -> main -> Usuario
         ^___________ retroalimentacion en cada paso ____________|
```

### Responsabilidades

| Agente | Que hace | Que produce | A quien se lo pasa |
|--------|----------|-------------|-------------------|
| main | Coordina, decide, reporta | Estado, resumen, decisiones | Usuario + subagentes |
| architect | Disena plan implementacion | Plan detallado con archivos y cambios | main |
| programmer | Implementa el plan | Codigo editado, archivos modificados | main |
| reviewer | Revisa cambios | Lista de issues (critico/advertencia/nota) | main |
| tester | Ejecuta tests | Resultados (pasaron/fallaron) | main |
| builder | Build & deploy | Build output, errores | main |

## Formato de respuesta para subagentes

### Architect -> Main
```markdown
## Plan: [titulo]

### Resumen
[1-2 lineas]

### Archivos a modificar
1. `ruta/archivo.cs` -- que cambiar

### Orden
1. Primero X, luego Y

### Cambios especificos
- `archivo.cs:45-60`: cambiar X por Y
```

### Programmer -> Main
```markdown
## Implementacion completada

### Modificados
- `archivo1.cs`: [cambio]
- `archivo2.cs`: [cambio]

### Creados
- `nuevo.cs`: [proposito]

### Convenciones
- Naming PascalCase: OK
- FindObjectOfType en Start: OK
- OnDestroy cleanup: OK
```

### Reviewer -> Main
```markdown
## Revision: [archivos]

### Problemas
- `archivo.cs:45`: [CRITICO] NullReferenceException posible
- `archivo.cs:120`: [WARN] Variable no usada

### Resumen
[N] criticos, [N] advertencias, [N] notas
```

### Tester -> Main
```markdown
## Resultados de Tests

### Pasaron: 47/47
- Suite IPValidation: 18/18
- Suite RoutingTable: 14/14

### Resumen
Todos los tests pasan sin regresion.
```

### Builder -> Main
```markdown
## Resultado de Build

### Build: IDEUM / PC
### Estado: Completado

### Archivos
- `/ruta/al/build.exe`

### Proximos pasos
- Copiar a mesa IDEUM
```

## Reglas de comunicacion

1. Se conciso -- ve al grano, no anadas prosa innecesaria.
2. Se especifico -- menciona archivos:linea, nombres exactos.
3. Clasifica por severidad -- critico > advertencia > nota.
4. Si hay error, di como solucionarlo -- no solo reportes.
5. No escondas resultados -- el usuario debe ver todo.

## Escalamiento de problemas

Si un subagente encuentra un problema que no puede resolver:

```markdown
## Bloqueante

### Problema
[descripcion clara]

### Causa raiz
[que lo esta causando]

### Opciones
1. [opcion A] -- [pros/cons]
2. [opcion B] -- [pros/cons]

### Sugerencia
[que recomienda el agente]
```

## Verificacion post-implementacion

Antes de marcar completada:
1. El codigo compila? (verificacion mental)
2. Los tests pasan? (ejecutar suite relevante)
3. Las convenciones del proyecto se respetan?
4. El ROADMAP.md esta actualizado?
5. El usuario fue notificado del resultado?

## Checklist para main antes de delegar

- Tarea requiere 2+ pasos? -> delegar a architect -> programmer -> reviewer -> tester
- Tarea simple (1-2 tool calls)? -> hacerla directamente
- El subagente tiene el contexto necesario? (skills, archivos)
- El subagente sabe que devolver?
