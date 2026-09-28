# DC-01: Diagramas de Casos de Uso

> **Propósito**: Mostrar la interacción entre los actores del sistema y las funcionalidades del SimuladorRedes IDEUM.
> **Distribución**: Actor a la izquierda, casos de uso a la derecha. Cada diagrama agrupa funcionalidades relacionadas.

---

## DC-01a: Gestión de Simulación

```mermaid
graph LR
  E[Estudiante] --> UC1[Iniciar Simulación]
  E --> UC2[Configurar Topología]
  E --> UC3[Probar Conectividad]
  E --> UC4[Configurar IP]
  M[Mesa IDEUM] --> UC1
  M --> UC2
```

---

## DC-01b: Actividades Académicas

```mermaid
graph LR
  E[Estudiante] --> UC5[Construir Topología]
  E --> UC6[Encuentra el Fallo]
  E --> UC11[Cargar Escenarios]
```

---

## DC-01c: Enrutamiento

```mermaid
graph LR
  E[Estudiante] --> UC7[Ver Tabla de Enrutamiento Tangible]
  E --> UC8[Seleccionar Mejor Ruta]
  E --> UC9[Configurar Rutas Estáticas]
  E --> UC10[Simular Enrutamiento Dinámico]
```

---

## DC-01d: Configuración Avanzada de Red

```mermaid
graph LR
  E[Estudiante] --> UC12[Configurar VLANs]
  E --> UC13[Configurar ACLs]
  E --> UC14[Configurar NAT]
  E --> UC15[Ver Tabla ARP]
  E --> UC16[Ver Tabla de Rutas]
```

---

## DC-01e: Sistema de Evaluación

```mermaid
graph LR
  E[Estudiante] --> UC17[Ver Puntaje]
  E --> UC18[Obtener Calificación]
  P[Profesor] --> UC17
  P --> UC18
```

---

## Leyenda de Actores

| Actor | Descripción |
|-------|-------------|
| **Estudiante** | Usuario final que interactúa con la mesa IDEUM |
| **Profesor** | Supervisor que puede ver calificaciones |
| **Mesa IDEUM** | Sistema de discos físicos que envía eventos táctiles |

## Archivos Relacionados

- `Assets/Scripts/Simulation/SceneSetup.cs` — Orquestador de la escena
- `Assets/Scripts/Simulation/ActivityLoader.cs` — Carga de actividades
- `Assets/Scripts/Simulation/ScoringSystem.cs` — Sistema de puntajes
