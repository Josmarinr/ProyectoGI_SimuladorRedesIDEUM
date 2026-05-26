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

## 🔄 Flujo de trabajo

```
Usuario → main → architect → programmer → reviewer → tester → main → Usuario
         ↑___________ retroalimentación en cada paso ____________|
```

### Responsabilidades de cada agente

| Agente | Qué hace | Qué produce | A quién se lo pasa |
|--------|----------|-------------|-------------------|
| `main` | Coordina, decide, reporta al usuario | Estado, resumen, decisiones | Usuario + subagentes |
| `architect` | Diseña plan de implementación | Plan detallado con archivos y cambios | `main` |
| `programmer` | Implementa el plan | Código editado, archivos modificados | `main` |
| `reviewer` | Revisa cambios | Lista de issues (crítico/advertencia/nota) | `main` |
| `tester` | Ejecuta tests | Resultados (pasaron/fallaron) | `main` |
| `builder` | Build & deploy | Build output, errores | `main` |

## 📤 Formato de respuesta para subagentes

### Architect → Main
```markdown
## Plan: [título]

### Resumen
[1-2 líneas]

### Archivos a modificar
1. `ruta/archivo.cs` — qué cambiar

### Orden
1. Primero X, luego Y

### Cambios específicos
- `archivo.cs:45-60`: cambiar X por Y
```

### Programmer → Main
```markdown
## Implementación completada

### Modificados
- `archivo1.cs`: [cambio]
- `archivo2.cs`: [cambio]

### Creados
- `nuevo.cs`: [propósito]

### Convenciones
- ✅ Naming PascalCase
- ✅ FindObjectOfType en Start
- ✅ OnDestroy cleanup
```

### Reviewer → Main
```markdown
## Revisión: [archivos]

### ❌ Crítico (debe corregirse)
- `archivo.cs:45`: NullReferenceException posible

### ⚠️ Advertencia
- `archivo.cs:120`: Variable no usada

### ✅ No se encontraron problemas adicionales
```

### Tester → Main
```markdown
## Resultados de Tests

### ✅ Pasaron: 47/47
- Suite IPValidation: 18/18
- Suite RoutingTable: 14/14
- Suite RouteBuilderState: 15/15

### Resumen
Todos los tests pasan sin regresión.
```

### Builder → Main
```markdown
## Resultado de Build

### Build: IDEUM / PC
### Estado: ✅ Completado

### Archivos
- `/ruta/al/build.exe`

### Próximos pasos
- Copiar a mesa IDEUM
```

## 📝 Reglas de comunicación

1. **Sé conciso** — ve al grano, no añadas prosa innecesaria
2. **Sé específico** — menciona archivos:línea, nombres exactos
3. **Clasifica por severidad** — crítico > advertencia > nota
4. **Si hay error, di cómo solucionarlo** — no solo reportes el problema
5. **No escondas resultados** — el usuario debe ver todo

## 🚨 Escalamiento de problemas

Si un subagente encuentra un problema que no puede resolver:

```markdown
## ⛔ Bloqueante

### Problema
[descripción clara]

### Causa raíz
[qué lo está causando]

### Opciones
1. [opción A] — [pros/cons]
2. [opción B] — [pros/cons]

### Sugerencia
[qué recomienda el agente]
```

## ✅ Verificación post-implementación

Antes de marcar una tarea como completada:

1. ¿El código compila? (verificación mental)
2. ¿Los tests pasan? (ejecutar suite relevante)
3. ¿Las convenciones del proyecto se respetan?
4. ¿El ROADMAP.md está actualizado?
5. ¿El usuario fue notificado del resultado?

## 📋 Checklist para main antes de delegar

- [ ] ¿Tarea requiere 2+ pasos? → delegar a architect → programmer → reviewer → tester
- [ ] ¿Tarea es simple (1-2 tool calls)? → hacerla directamente, no delegar
- [ ] ¿El subagente tiene el contexto necesario? (skills a cargar, archivos a leer)
- [ ] ¿El subagente sabe qué devolver?
