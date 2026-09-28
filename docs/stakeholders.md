# Stakeholders — SimuladorRedes IDEUM

> Identificación y análisis de actores interesados en el proyecto.

---

## Matriz de Stakeholders

| Stakeholder | Tipo | Interés | Influencia | Rol Principal |
|-------------|------|---------|------------|---------------|
| Estudiantes de Redes | Primario | Alto | Bajo | Usuarios finales |
| Profesores de Redes | Primario | Alto | Medio | Implementadores pedagógicos |
| Desarrollador | Primario | Alto | Alto | Creador y mantenedor |
| Universidad Distrital | Secundario | Medio | Alto | Propietario de infraestructura |
| Grupo de Investigación Multimedia | Secundario | Medio | Medio | Supervisor académico |
| IDEUM (proveedor hardware) | Terciario | Bajo | Medio | Soporte técnico |

---

## Análisis por Stakeholder

### Estudiantes de Redes

**Perfil:** Estudiantes de ingeniería que cursan o han cursado la materia de Redes de Computadores.

**Necesidades:**
- Aprender conceptos abstractos de redes (topología, enrutamiento, subnetting)
- Practicar configuración de dispositivos sin riesgo de dañar equipo real
- Recibir retroalimentación inmediata sobre sus acciones
- Experimentar con protocolos de enrutamiento dinámico (RIP, OSPF, EIGRP)

**Expectativas:**
- Interfaz intuitiva que no requiera capacitación extensa
- Actividades guiadas con objetivos claros
- Sistema de puntaje que motive la mejora continua
- Experiencia de aprendizaje diferenciada al software tradicional

**Nivel de Involucramiento:** Alto (usuarios directos del sistema)

---

### Profesores de Redes

**Perfil:** Docentes responsables de cursos de redes en universidades.

**Necesidades:**
- Evaluar comprensión de conceptos de redes en estudiantes
- Diseñar actividades prácticas que complementen la teoría
- Monitorear progreso de estudiantes mediante métricas de scoring
- Configurar escenarios personalizados para diferentes temas

**Expectativas:**
- Sistema que facilite la planificación de sesiones
- Reportes de rendimiento de estudiantes
- Flexibilidad para crear escenarios personalizados
- Integración con currículo existente (CCNA, redes I, redes II)

**Nivel de Involucramiento:** Medio (configuran y supervisan sesiones)

---

### Desarrollador

**Perfil:** Johan Sebastian Marin Rojas, creador del sistema.

**Necesidades:**
- Mantener el código limpio y documentado
- Extender funcionalidades sin romper existentes
- Corregir bugs reportados por usuarios
- Generar builds estables para la mesa IDEUM
- Publicar investigación sobre el prototipo

**Expectativas:**
- Arquitectura modular que facilite mantenimiento
- Cobertura de tests completa (399+ pruebas)
- Documentación técnica actualizada
- Comunidad activa de contribuidores (futuro open-source)

**Nivel de Involucramiento:** Alto (responsable de todo el ciclo de vida)

---

### Universidad Distrital Francisco José de Caldas

**Perfil:** Institución propietaria de la infraestructura IDEUM.

**Necesidades:**
- Aprovechar la inversión en mesas interactivas IDEUM
- Generar investigación de alto impacto
- Ofrecer herramientas innovadoras a sus estudiantes
- Publicar resultados en conferencias y revistas científicas

**Expectativas:**
- Sistema estable y escalable
- Documentación completa para transferencia tecnológica
- Resultados medibles de impacto educativo
- Potencial de adopción en otras instituciones

**Nivel de Involucramiento:** Bajo (proveen infraestructura y supervisión)

---

### Grupo de Investigación Multimedia Interactiva

**Perfil:** Grupo de investigación de la Universidad Distrital.

**Necesidades:**
- Producir publicaciones científicas sobre interacción tangible
- Formar estudiantes en investigación aplicada
- Desarrollar prototipos innovadores de interfaces tangible
- Establecer colaboraciones con otras instituciones

**Expectativas:**
- Metodología de investigación rigurosa
- Resultados replicables y documentados
- Potencial de publicación en congresos ACM/IEEE
- Contribución al conocimiento en HCI y Tangible Computing

**Nivel de Involucramiento:** Medio (supervisan dirección de investigación)

---

### IDEUM (Proveedor de Hardware)

**Perfil:** Empresa fabricante de mesas interactivas IDEUM.

**Necesidades:**
- Promover el uso de su tecnología en educación
- Generar casos de éxito que demuestren valor de sus productos
- Recibir retroalimentación sobre usabilidad del SDK

**Expectativas:**
- Aplicaciones que muestren capacidades del hardware
- Documentación técnica del SDK para futuros desarrolladores
- Referencias institucionales que validen su producto

**Nivel de Involucramiento:** Bajo (proveen soporte técnico del SDK)

---

## Matriz de Poder/Interés

```
                    ALTO INTERÉS
                         │
    ┌────────────────────┼────────────────────┐
    │                    │                    │
    │  GESTIONAR         │  MANTENER          │
    │  DE CERCA          │  INFORMADOS        │
    │                    │                    │
    │  - Desarrollador   │  - Estudiantes     │
    │                    │  - Profesores      │
    │                    │                    │
ALTO├────────────────────┼────────────────────┤BAJO
PODER│                    │                    │PODER
    │                    │                    │
    │  MANTENER          │  MONITOREAR        │
    │  SATISFECHOS       │                    │
    │                    │  - Grupo Invest.   │
    │  - Universidad     │  - IDEUM           │
    │                    │                    │
    └────────────────────┼────────────────────┘
                         │
                    BAJO INTERÉS
```

> **Nota de ubicación**: Las valoraciones de la tabla superior no se modifican.
> En cada eje solo el valor Alto ocupa la celda Alta; los actores con valoración
> Media (Profesores, Universidad, Grupo de Investigación e IDEUM) se ubican en la
> celda Baja de ese eje.

---

## Estrategia de Comunicación

| Stakeholder | Frecuencia | Canal | Contenido |
|-------------|------------|-------|-----------|
| Estudiantes | Por sesión | Mesa IDEUM | Actividades, scoring, feedback |
| Profesores | Semanal | Email/Reunión | Reportes de uso, métricas |
| Desarrollador | Diario | GitHub/Slack | Issues, PRs, builds |
| Universidad | Mensual | Informe | Progreso, publicaciones |
| Grupo Investigación | Quincenal | Reunión | Resultados, dirección investigación |
| IDEUM | Bajo demanda | Email | Feedback del SDK, bugs |

---

## Riesgos por Stakeholder

| Stakeholder | Riesgo | Impacto | Mitigación |
|-------------|--------|---------|------------|
| Estudiantes | Frustración por bugs | Alto | Testing exhaustivo, feedback rápido |
| Profesores | No adoptar la herramienta | Medio | Capacitación, documentación clara |
| Desarrollador | Burnout por mantenimiento | Medio | Comunidad open-source, documentación |
| Universidad | No ver ROI | Alto | Métricas de uso, publicaciones |
| Grupo Invest. | No publicar | Medio | Metodología rigurosa, resultados |
| IDEUM | Descontinuar SDK | Bajo | Documentar dependencias, alternativas |

---

*Documento generado automáticamente. Última actualización: 2026-09-07.*
