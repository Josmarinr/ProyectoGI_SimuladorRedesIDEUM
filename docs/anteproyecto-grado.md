# Anteproyecto de Trabajo de Grado

## Modelo de Interacción a Través de Reconocimiento de Objetos Tangibles para la Simulación de Procesos de Conectividad Física y Lógica de Dispositivos Networking

---

## 1. Información General

| Campo | Detalle |
|-------|---------|
| **Título** | Modelo de interacción tangible para la simulación de procesos de conectividad física y lógica de dispositivos networking en mesas interactivas IDEUM |
| **Área** | Redes de Computadores / Interacción Humano-Computador |
| **Duración estimada** | 3 meses |
| **Tipo** | Trabajo de Grado — Investigación + Desarrollo |
| **Palabras clave** | Interacción tangible, simulación de redes, mesas interactivas, objetos programables, educación en networking |

---

## 2. Planteamiento del Problema

### 2.1 Contexto

La enseñanza de redes de computadores enfrenta una brecha significativa entre la teoría y la práctica. Si bien existen simuladores software (Packet Tracer, GNS3, EVE-NG), estos carecen de la dimensión física y kinestésica que facilita la comprensión de conceptos abstractos como topología de red, enrutamiento, y conectividad.

Las mesas interactivas IDEUM, disponibles en la Universidad, permiten hasta 80 puntos táctiles simultáneos y reconocimiento de objetos tangibles (discos/PUCs). Esto abre la posibilidad de crear un laboratorio de redes tangible donde los estudiantes puedan "tocar" los dispositivos de red y construir topologías físicamente.

### 2.2 Problema

Actualmente no existe un sistema que integre:

- **Objetos tangibles** (discos) que representen dispositivos de red (routers, switches, PCs)
- **Simulación de enrutamiento** (estático y dinámico) en tiempo real
- **Detección de topologías** a partir de la disposición física de objetos
- **Evaluación formativa** mediante scoring y detección de fallos
- **Métricas de usabilidad** que validen la efectividad del enfoque tangible vs. el enfoque tradicional

### 2.3 Preguntas de Investigación

1. ¿Un modelo de interacción tangible mejora la comprensión de conceptos de conectividad y enrutamiento en estudiantes de redes?
2. ¿Qué estrategias de usabilidad son más efectivas para el despliegue de simulaciones de red en mesas interactivas?
3. ¿Es posible emular el comportamiento de routers y switches mediante objetos programables (discos) con suficiente fidelidad para fines académicos?

---

## 3. Objetivos

### 3.1 Objetivo General

Desarrollar y evaluar un modelo de interacción basado en reconocimiento de objetos tangibles para la simulación de procesos de conectividad física y lógica de dispositivos networking, utilizando las mesas interactivas IDEUM.

### 3.2 Objetivos Específicos

| # | Objetivo | Producto |
|:-:|----------|----------|
| OE1 | Adaptar el simulador de redes existente para su operación con discos tangibles (PUCs) en la mesa IDEUM | 6 actividades funcionales con discos físicos |
| OE2 | Implementar escenarios de aprendizaje que cubran topología, detección de fallos, tablas de enrutamiento, mejor ruta, enrutamiento estático y dinámico | Módulo de 6 actividades tangibles |
| OE3 | Evaluar la usabilidad del sistema mediante pruebas con usuarios y métricas estándar (SUS, TAM) | Informe de usabilidad |
| OE4 | Determinar estrategias de despliegue para la integración del simulador tangible en el currículo académico | Guía de implementación docente |

---

## 4. Marco Teórico

### 4.1 Aprendizaje Tangible

La interacción tangible (Tangible User Interfaces — TUI) se fundamenta en la teoría de aprendizaje embodied cognition, que postula que los procesos cognitivos están influenciados por el cuerpo y las interacciones físicas con el entorno. Al manipular objetos físicos que representan conceptos abstractos (como una tabla de enrutamiento), el estudiante internaliza más efectivamente el conocimiento.

### 4.2 Simulación de Redes

Los simuladores de redes tradicionales operan en dos dimensiones (pantalla) y requieren arrastrar y soltar elementos con mouse. La transición a un modelo tangible agrega una tercera dimensión: la disposición espacial de objetos en una superficie física, que corresponde directamente a la topología de red que se está modelando.

### 4.3 Mesas IDEUM

Las mesas IDEUM proporcionan:
- Reconocimiento de hasta 80 puntos táctiles simultáneos
- Detección de objetos (discos) con IDs únicos (PUCs)
- Resolución nativa de 4096x2160 píxeles
- Integración con Unity 3D para renderizado y lógica de aplicación

### 4.4 Estado del Arte

Trabajos relacionados incluyen:
- **FlowBlocks** (Harvard): bloques programables para simulación de protocolos de red
- **IDEUM Planes of Fame**: navegación por interfaces tangibles en mesas interactivas
- **Cisco Packet Tracer**: simulador tradicional (no tangible) para CCNA

El presente proyecto se diferencia al combinar **objetos tangibles genéricos** (discos reutilizables) con **simulación de red completa** (enrutamiento estático, dinámico, detección de fallos) en un entorno académico integrado.

---

## 5. Metodología

### 5.1 Enfoque

Investigación mixta: desarrollo iterativo (ingeniería de software) + evaluación de usabilidad (IHC).

### 5.2 Fases

| Fase | Actividades | Duración | Entregable |
|:----:|-------------|:--------:|------------|
| **F1** | Preparación del entorno de pruebas (mesa IDEUM + build del simulador) | 2 semanas | Build funcional en mesa IDEUM |
| **F2** | Validación de actividades con discos físicos (pruebas de detección, mapeo, auto-conexión) | 2 semanas | 6 actividades operativas con discos |
| **F3** | Refinamiento de UI/UX basado en pruebas preliminares | 2 semanas | Interfaz ajustada |
| **F4** | Evaluación de usabilidad con estudiantes (SUS + observación) | 3 semanas | Informe de usabilidad |
| **F5** | Análisis de resultados y documentación | 3 semanas | Documento final de grado |

### 5.3 Instrumentos de Evaluación

| Instrumento | Propósito | Métrica |
|-------------|-----------|---------|
| **System Usability Scale (SUS)** | Usabilidad percibida | Puntaje 0-100 |
| **Technology Acceptance Model (TAM)** | Aceptación tecnológica | Utilidad percibida + facilidad de uso |
| **Observación directa** | Interacción real | Tiempo por tarea, errores, caminos de navegación |
| **Pre-test / Post-test** | Aprendizaje | Diferencia de conocimiento antes/después |

### 5.4 Población y Muestra

- **Población**: Estudiantes de ingeniería que cursan redes de computadores
- **Muestra**: 20-30 estudiantes (grupo control con simulador tradicional + grupo experimental con simulador tangible)

---

## 6. Resultados Esperados

| # | Resultado | Indicador |
|:-:|-----------|-----------|
| R1 | Sistema funcional de simulación de redes con interacción tangible | 6 actividades operativas, 231+ tests unitarios pasando |
| R2 | Evidencia de mejora en comprensión de conceptos de red | Diferencia significativa (p < 0.05) en post-test |
| R3 | Puntaje de usabilidad aceptable (SUS > 68) | SUS score ≥ 68 (promedio industria) |
| R4 | Guía de implementación para docentes | Documento replicable |

---

## 7. Cronograma (3 meses)

| Semana | F1 | F2 | F3 | F4 | F5 |
|:------:|:--:|:--:|:--:|:--:|:--:|
| 1-2 | ██ | | | | |
| 3-4 | | ██ | | | |
| 5-6 | | | ██ | | |
| 7-9 | | | | ███ | |
| 10-12 | | | | | ███ |

### Hitos

| Semana | Hito |
|:------:|------|
| 2 | Build funcionando en mesa IDEUM con detección de discos |
| 4 | 6 actividades validades con discos físicos |
| 6 | UI ajustada + 231 tests pasando |
| 9 | Evaluación de usabilidad completada |
| 12 | Documento final entregado |

---

## 8. Recursos Necesarios

### 8.1 Hardware

| Recurso | Especificación | Estado |
|---------|---------------|:------:|
| Mesa IDEUM | 55" táctil, 4096x2160 | ✅ Disponible en Universidad |
| Discos PUCs | 6 discos programables (Router, Switch, PC, Enlace, etc.) | ✅ Disponibles |
| PC de desarrollo | Windows 10+, Unity 6000 | ✅ Disponible |

### 8.2 Software

| Componente | Tecnología | Estado |
|------------|-----------|:------:|
| Motor de simulación | Unity 6000.4.5f1 | ✅ Instalado |
| Framework de tests | NUnit + Unity Test Framework | ✅ Integrado |
| Control de versiones | Git + GitHub | ✅ Configurado |
| Tangible Engine | SDK IDEUM | ✅ Integrado |

### 8.3 Capital Humano

- **Investigador principal**: Estudiante de grado
- **Tutor/Director**: Docente del área de redes
- **Soporte técnico**: Administrador de laboratorio IDEUM

---

## 9. Riesgos y Mitigación

| Riesgo | Probabilidad | Impacto | Mitigación |
|--------|:-----------:|:-------:|------------|
| Mesa IDEUM no disponible para pruebas | Baja | Alto | Simulador tiene modo Debug por teclado (ya implementado) |
| Discos PUCs no detectan correctamente | Media | Alto | Logging detallado en TangibleBridge, pruebas unitarias |
| Baja participación en evaluación | Media | Medio | Incentivos académicos, horarios flexibles |
| Complejidad técnica de integración | Baja | Medio | 85% del código ya implementado y probado |

---

## 10. Impacto Esperado

### Académico
- Publicación de artículo sobre interacción tangible para educación en redes
- Repositorio open-source del simulador para uso de otras instituciones
- Base para trabajos futuros (VLAN, ACL, NAT como actividades adicionales)

### Social
- Democratización del acceso a laboratorios de redes (costo cero vs. equipos físicos)
- Reducción de la brecha entre teoría y práctica
- Herramienta inclusiva para estudiantes con diferentes estilos de aprendizaje

### Tecnológico
- Modelo de integración Tangible Engine + Unity replicable
- Arquitectura de factories y actividades extensible
- Sistema de evaluación formativa incorporado (scoring)

---

## 11. Referencias

1. Ishii, H., & Ullmer, B. (1997). Tangible bits: towards seamless interfaces between people, bits and atoms. *Proceedings of CHI '97*.
2. Shen, C., et al. (2023). FlowBlocks: Programmable Tangible Blocks for Network Protocol Simulation. *Harvard SEAS Technical Report*.
3. IDEUM. (2024). Tangible Engine SDK Documentation.
4. Brooke, J. (1996). SUS: A quick and dirty usability scale. *Usability Evaluation in Industry*.
5. Davis, F. D. (1989). Perceived usefulness, perceived ease of use, and user acceptance of information technology. *MIS Quarterly*.
6. Kolb, D. A. (1984). *Experiential Learning: Experience as the Source of Learning and Development*.
7. Cisco Systems. (2024). Packet Tracer Documentation.

---

## 12. Anexos

- **Anexo A**: Código fuente del simulador (repositorio GitHub)
- **Anexo B**: Manual de usuario del simulador tangible
- **Anexo C**: Guía de instalación y configuración de mesa IDEUM
- **Anexo D**: Formulario de consentimiento informado para evaluación
- **Anexo E**: Instrumento SUS (System Usability Scale) — a diseñar en F4
- **Anexo F**: Pre-test y Post-test de conocimientos — a diseñar en F4

---

> **Documento generado como parte del proyecto SimuladorRedes IDEUM**
> Versión: 1.0 — Mayo 2026
