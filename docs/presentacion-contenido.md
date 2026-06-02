# Interacción tangible aplicada a la simulación de redes: desarrollo y validación de un prototipo sobre mesa IDEUM

**Autor:** Johan Sebastian Marin Rojas
**Director:** Prof. Paulo Alonso Gaona Garcia
**Grupo de Investigación:** Multimedia Interactiva
**Universidad:** Universidad Distrital Francisco Jose de Caldas

---

## Resumen Ejecutivo

Esta investigación presenta el desarrollo de un simulador de redes basado en interacción tangible, utilizando las mesas interactivas IDEUM de la Universidad Distrital Francisco Jose de Caldas. El sistema permite a los estudiantes construir topologías de red mediante la colocación de discos físicos programables (PUCs) sobre una superficie táctil de 55 pulgadas con resolución 4K, los cuales representan routers, switches y PCs. Se implementaron siete actividades académicas que cubren desde construcción de topologías hasta enrutamiento dinámico con RIP, OSPF y EIGRP. El código fuente consta de 46 scripts en C# respaldados por 328 pruebas unitarias distribuidas en 18 suites. La documentación incluye 21 diagramas UML, manuales de usuario y de desarrollador, y la especificación de las APIs del sistema. Se identificaron tres problemas técnicos que requieren resolución previa a la evaluación formal de usabilidad con estudiantes. Los resultados demuestran la viabilidad de emular dispositivos de red mediante objetos tangibles genéricos con fines académicos.

---

## Problema y Contexto

### Planteamiento del problema

La enseñanza de redes de computadores enfrenta una brecha significativa entre la teoría y la práctica. Conceptos como topología, enrutamiento y conectividad son inherentemente abstractos, y los simuladores tradicionales como Packet Tracer, GNS3 y EVE-NG operan exclusivamente mediante interfaz gráfica con mouse y teclado, sin ofrecer una dimensión física o kinestésica que facilite la comprensión. La teoría de la cognición corporeizada (*embodied cognition*) sostiene que los procesos cognitivos están influenciados por las interacciones físicas con el entorno, lo que sugiere que la manipulación de objetos tangibles podría mejorar la internalización de conceptos abstractos en el aprendizaje de redes.

### Contexto tecnológico

La Universidad Distrital Francisco Jose de Caldas cuenta con mesas interactivas IDEUM de 55 pulgadas, las cuales soportan hasta 80 puntos táctiles simultáneos y reconocen objetos físicos (discos PUCs) con identificadores únicos mediante el protocolo TUIO. No obstante, no existía una aplicación educativa que aprovechara esta tecnología para la enseñanza de redes de computadores. Esta investigación surge de la oportunidad de integrar dicha capacidad tecnológica con las necesidades pedagógicas del área.

### Preguntas de investigación

1. ?Un modelo de interacción tangible mejora la comprensión de conceptos de conectividad y enrutamiento en estudiantes de redes?
2. ?Qué estrategias de usabilidad resultan más efectivas para el despliegue de simulaciones de red en mesas interactivas?
3. ?Es posible emular el comportamiento de routers y switches mediante objetos programables (discos) con suficiente fidelidad para fines académicos?

### Relevancia del estudio

La investigación tiene impacto en múltiples dimensiones. En el ámbito académico, propone un modelo de integración entre interfaces tangibles y simulación de redes que puede ser replicado por otras instituciones. En el plano educativo, reduce la brecha entre teoría y práctica al ofrecer una experiencia de aprendizaje kinestésica. Tecnológicamente, demuestra la viabilidad de extender un motor de simulación como Unity para operar con dispositivos de entrada tangible a través del TangibleEngine SDK. Socialmente, democratiza el acceso a laboratorios de redes al eliminar la necesidad de equipos físicos costosos.

---

## Objetivos

### Objetivo general

Desarrollar y evaluar un modelo de interacción basado en el reconocimiento de objetos tangibles para la simulación de procesos de conectividad física y lógica de dispositivos de red, utilizando las mesas interactivas IDEUM.

### Objetivos específicos

| Objetivo | Descripción | Producto |
|----------|-------------|----------|
| OE1 | Adaptar el simulador de redes para su operación con discos tangibles (PUCs) en la mesa IDEUM | Siete actividades funcionales con discos físicos |
| OE2 | Implementar escenarios de aprendizaje que cubran topología, detección de fallos, tablas de enrutamiento, mejor ruta, enrutamiento estático y enrutamiento dinámico | Módulo de actividades tangibles |
| OE3 | Evaluar la usabilidad del sistema mediante pruebas con usuarios y métricas estandarizadas (SUS, TAM) | Informe de usabilidad (pendiente de aplicación) |
| OE4 | Determinar estrategias de despliegue para la integración del simulador tangible en el currículo académico | Guía de implementación docente |

---

## Metodología

### Enfoque metodológico

Se empleó un enfoque mixto que combina desarrollo iterativo de software con evaluación de experiencia de usuario. La componente cuantitativa comprende métricas de calidad de código (cobertura de pruebas, densidad de defectos) y puntajes de usabilidad (SUS). La componente cualitativa incluye observación directa de la interacción y preguntas abiertas en los instrumentos de evaluación.

### Fases de la investigación

El proyecto se estructuró en cinco fases secuenciales:

**Fase 1 - Preparación del entorno.** Configuración de la mesa IDEUM, integración del TangibleEngine SDK con Unity 6000.4.5f1, y generación de un build de prueba para verificar la comunicación entre el motor de simulación y la mesa.

**Fase 2 - Validación con discos físicos.** Pruebas de detección de discos, mapeo de coordenadas TUIO (1920x1080) a coordenadas de Canvas (4096x2160), implementación del sistema de antirrebote para evitar parpadeo de discos, y verificación de la auto-conexión de enlaces entre dispositivos cercanos.

**Fase 3 - Refinamiento de interfaz.** Corrección de problemas de actualización del panel de dispositivos, centralización de la creación de fuentes tipográficas, implementación del EventSystem requerido para la interacción táctil, y ajuste de dimensiones de paneles para la resolución objetivo.

**Fase 4 - Evaluación de usabilidad.** Diseño de instrumentos de evaluación: cuestionario pre-test (datos demográficos, autoevaluación de conocimientos en nueve temas de redes, expectativas), cuestionario SUS adaptado (diez preguntas en escala Likert de cinco puntos), y cuestionario post-test (re-evaluación de conocimientos, satisfacción específica, preguntas abiertas). La aplicación de estos instrumentos está pendiente.

**Fase 5 - Documentación.** Elaboración de diagramas UML (21 en total), manual de usuario, guía de desarrollo, documentación de APIs, y el presente informe de investigación.

### Herramientas y tecnologías

| Componente | Especificación |
|------------|----------------|
| Motor de simulación | Unity 6000.4.5f1 (Unity 6) |
| Lenguaje de programación | C# (46 scripts, aproximadamente 11,847 líneas de código) |
| Framework de pruebas | NUnit con Unity Test Framework (328 pruebas, 18 suites) |
| Sistema de entrada | Input System Package 1.19.0 (migración completa, sin dependencia del API legacy) |
| Integración tangible | IDEUM TangibleEngine SDK (comunicación TCP, puerto 4949) |
| Control de versiones | Git con repositorio en GitHub |

### Población y muestra

La evaluación está diseñada para una muestra de 20 a 30 estudiantes de ingeniería que hayan cursado o estén cursando la materia de redes de computadores. Se contempla la división en dos grupos: un grupo experimental que utilizará el simulador tangible y un grupo de control que utilizará Packet Tracer, con el fin de comparar resultados de aprendizaje y satisfacción.

### Procedimiento de evaluación

El flujo de evaluación para cada participante comprende tres etapas:

1. **Pre-test (10 minutos):** Cuestionario demográfico, autoevaluación de conocimientos en escala de 1 a 5 en nueve temas de redes, y registro de expectativas sobre la experiencia tangible.
2. **Sesión práctica (45 a 60 minutos):** El participante interactúa con el simulador tangible realizando actividades guiadas sobre la mesa IDEUM.
3. **Post-test y SUS (15 minutos):** Re-evaluación de conocimientos, aplicación del cuestionario SUS, preguntas de satisfacción específica y preguntas abiertas sobre la experiencia.

---

## Resultados

### Actividades implementadas

Se desarrollaron siete actividades académicas funcionales, cada una orientada a competencias específicas del área de redes:

| Actividad | Descripción | Competencia |
|-----------|-------------|-------------|
| Construye la Topología | El estudiante coloca discos sobre la mesa; el sistema detecta el tipo de dispositivo y construye la representación digital de la red, identificando automáticamente topologías de tipo estrella, bus, anillo, árbol y malla | Comprensión de estructuras de red |
| Encuentra el Fallo | La mesa presenta una red con fallos preconfigurados (cable desconectado, IP incorrecta, máscara inválida, PC sin dirección IP); el estudiante debe diagnosticar y reparar | Diagnóstico y resolución de problemas |
| Tabla de Enrutamiento | Visualización de las tablas de enrutamiento de todos los routers en la topología activa | Comprensión de tablas de rutas |
| Mejor Ruta | Modalidad tipo cuestionario: se presentan rutas alternativas y el estudiante selecciona la óptima según métricas y prefijos | Algoritmo de mejor ruta |
| Enrutamiento Estático | Configuración manual de rutas en los routers mediante discos virtuales | Configuración de rutas estáticas |
| Enrutamiento Dinámico | Simulación de protocolos RIP, OSPF y EIGRP con convergencia automática de rutas | Protocolos de enrutamiento dinámico |
| Escenarios | Cinco topologías preconfiguradas con objetivos específicos para uso en clase | Aplicación integrada de conocimientos |

### Métricas del proyecto

| Métrica | Valor |
|---------|-------|
| Pruebas unitarias | 328, todas con resultado exitoso |
| Suites de prueba | 18 |
| Discos físicos soportados | Router (ID 1), Switch (ID 2), PC (ID 3) |
| Discos virtuales para configuración de routing | 15 (IDs 4 al 18) |
| Tipos de topología detectables | 5 (estrella, bus, anillo, árbol, malla) |
| Protocolos de enrutamiento dinámico | 3 (RIP, OSPF, EIGRP) |
| Escenarios preconfigurados | 5 |
| Diagramas UML | 21 (casos de uso, clases, secuencia, actividad, estado, despliegue, paquetes) |
| Documentación técnica | 9 archivos de especificación de APIs, 2 manuales |
| Defectos corregidos durante el desarrollo | 6 (seguridad contra nulos, EventSystem, fuentes, coordenadas TUIO, entre otros) |
| Advertencias de compilación eliminadas | Aproximadamente 188 |

### Funcionalidades verificadas

- Detección y posicionamiento de discos físicos en tiempo real: al colocar un disco Router sobre la mesa, el nodo correspondiente aparece en la pantalla en la posición exacta del disco.
- Creación automática de enlaces entre dispositivos cuya distancia en la superficie de la mesa es inferior a 300 píxeles, y creación manual mediante el modo CONECTAR.
- Simulación de conectividad con animación visual de paquetes durante pruebas de ping entre dispositivos.
- Convergencia de protocolos de enrutamiento dinámico con visualización de rutas aprendidas en las tablas de enrutamiento.
- Sistema de puntuación que asigna calificaciones en escala de 0 a 100 en función de las acciones del estudiante.
- Configuración de funcionalidades avanzadas de red: VLAN, ACL y NAT.

### Problemas detectados durante las pruebas

| Identificador | Problema | Severidad | Estado |
|:-------------:|----------|:---------:|--------|
| P1 | La aplicación experimenta fallos por agotamiento de memoria tras periodos prolongados de uso, posiblemente asociados a la creación y destrucción repetida de paneles de interfaz | Crítica | En análisis con Unity Profiler |
| P2 | Las líneas de conexión entre dispositivos no se renderizan visiblemente sobre la superficie de la mesa, a pesar de haberse configurado con RawImage, grosor de 10 píxeles y alpha en 1 | Alta | Sin solución identificada |
| P3 | Las dimensiones de los elementos de interfaz requieren ajustes adicionales para una visualización óptima en resolución 4096x2160 | Media | Avance parcial |

---

## Interpretación de los resultados

El cumplimiento del objetivo general se evidencia en la operatividad del sistema y su capacidad para ejecutar las siete actividades planificadas. La cobertura temática abarca desde conceptos fundamentales de topología hasta configuración avanzada de redes, equivalente al contenido de un curso introductorio de redes basado en el currículo CCNA.

Las 328 pruebas unitarias constituyen un mecanismo de aseguramiento de calidad que permite detectar regresiones durante el desarrollo iterativo. La arquitectura modular basada en factories y controladores facilita la extensión del sistema sin comprometer la estabilidad de los componentes existentes.

La validación exitosa de la detección de discos en la mesa IDEUM confirma la viabilidad técnica del enfoque. La conversión de coordenadas TUIO, el sistema de antirrebote y el mapeo de identificadores funcionan de manera consistente después de múltiples iteraciones de corrección.

Los problemas P1, P2 y P3 representan limitaciones técnicas que deben resolverse antes de proceder con la evaluación de usabilidad con usuarios. El problema P1 es crítico debido a que la duración estimada de las sesiones de evaluación (45 a 60 minutos) excede el tiempo de operación estable del sistema en su estado actual.

---

## Comparación con el estado del arte

### Trabajos relacionados

La literatura reporta antecedentes relevantes en el campo de la interacción tangible aplicada a la educación en redes. Ishii y Ullmer (1997) sentaron las bases de las interfaces de usuario tangibles. Shen et al. (2023) desarrollaron FlowBlocks, un sistema de bloques programables para la simulación de protocolos de red en Harvard. En el ámbito de los simuladores tradicionales, Packet Tracer (Cisco) y GNS3 son las herramientas de referencia para la enseñanza de redes, aunque ambas operan exclusivamente mediante interacción gráfica convencional.

### Análisis comparativo

| Característica | Este prototipo | Packet Tracer | GNS3 | FlowBlocks (Harvard) |
|:---------------|:--------------:|:-------------:|:----:|:--------------------:|
| Interacción tangible | Sí (discos físicos) | No | No | Sí (bloques especializados) |
| Enrutamiento estático | Sí | Sí | Sí | No |
| Enrutamiento dinámico | RIP, OSPF, EIGRP | RIP, OSPF, EIGRP | Todos | Parcial |
| Detección de topología | Automática (5 tipos) | Manual | Manual | No aplica |
| Sistema de evaluación | Integrado (scoring) | No | No | No |
| Configuración avanzada | VLAN, ACL, NAT | VLAN, ACL, NAT | VLAN, ACL, NAT | No |
| Hardware requerido | Mesa IDEUM (existente) | PC estándar | PC estándar | Hardware custom |
| Costo de implementación | Bajo (hardware disponible) | Bajo | Medio | Alto |

### Aportaciones del prototipo

La principal contribución de esta investigación consiste en la combinación de objetos tangibles genéricos (discos PUCs reutilizables) con un simulador de red funcional que incluye enrutamiento estático y dinámico, detección automática de topología y sistema de evaluación integrado. A diferencia de FlowBlocks, que requiere bloques especializados, este prototipo utiliza discos programables que pueden reconfigurarse para diferentes propósitos. La detección automática de topología a partir de la disposición física de los objetos es una funcionalidad no presente en ningún simulador tradicional.

### Limitaciones identificadas

El prototipo depende de la disponibilidad de la mesa IDEUM, lo que limita su portabilidad en comparación con herramientas como Packet Tracer. Los tres problemas técnicos documentados (P1, P2, P3) requieren resolución antes de la evaluación formal. El sistema no implementa protocolos avanzados como BGP o MPLS, aunque estos están fuera del alcance de un curso introductorio de redes.

---

## Conclusiones y trabajo futuro

### Conclusiones

1. **Viabilidad técnica confirmada.** Es posible simular dispositivos de red (routers, switches, PCs) mediante objetos tangibles genéricos (discos PUCs) con suficiente fidelidad para fines académicos, incluyendo enrutamiento estático, enrutamiento dinámico (RIP, OSPF, EIGRP) y configuración avanzada (VLAN, ACL, NAT).

2. **Cobertura curricular completa.** Las siete actividades implementadas abarcan el espectro completo de un curso introductorio de redes, desde topología básica hasta protocolos de enrutamiento dinámico y configuración avanzada.

3. **Base técnica sólida.** Las 328 pruebas unitarias distribuidas en 18 suites proporcionan un mecanismo confiable de detección de regresiones. La arquitectura modular basada en factories y controladores facilita la extensión del sistema.

4. **Contribución original.** La detección automática de topología a partir de la disposición física de objetos sobre la mesa no tiene equivalente en los simuladores tradicionales ni en los prototipos tangibles reportados en la literatura.

5. **Potencial del enfoque tangible.** La integración de la dimensión física con la simulación de redes representa un avance hacia modelos pedagógicos que aprovechan la cognición corporeizada para la enseñanza de conceptos abstractos.

### Trabajo futuro

| Prioridad | Tarea |
|:---------:|-------|
| Crítica | Resolver la fuga de memoria (P1) mediante el uso del Unity Profiler para identificar puntos de fuga e implementar pooling de objetos de interfaz |
| Alta | Depurar el problema de renderizado de líneas de conexión (P2) mediante revisión del orden de renderizado, shaders y componentes gráficos alternativos |
| Alta | Completar el ajuste de dimensiones de la interfaz de usuario (P3) para resolución 4096x2160 |
| Media | Ejecutar la evaluación de usabilidad con 20 a 30 estudiantes aplicando los instrumentos diseñados (SUS, pre-test, post-test) y analizar los resultados |
| Media | Elaborar el informe final de investigación con el análisis completo de resultados |
| Baja | Explorar la ampliación del sistema a nuevos tipos de dispositivos (servidores, firewalls), la publicación del código como proyecto open-source, y la incorporación de protocolos adicionales |

---

## Referencias

Bangor, A., Kortum, P. T., & Miller, J. T. (2008). An empirical evaluation of the System Usability Scale. *International Journal of Human-Computer Interaction, 24*(6), 574-594.

Brooke, J. (1996). SUS: A quick and dirty usability scale. En P. W. Jordan, B. Thomas, B. A. Weerdmeester, & I. L. McClelland (Eds.), *Usability evaluation in industry* (pp. 189-194). Taylor & Francis.

Cisco Systems. (2024). *Packet Tracer documentation*. https://www.netacad.com/resources/packet-tracer

Davis, F. D. (1989). Perceived usefulness, perceived ease of use, and user acceptance of information technology. *MIS Quarterly, 13*(3), 319-340.

IDEUM. (2024). *Tangible Engine SDK documentation* [Software development kit].

Ishii, H., & Ullmer, B. (1997). Tangible bits: towards seamless interfaces between people, bits and atoms. *Proceedings of the ACM SIGCHI Conference on Human Factors in Computing Systems (CHI '97)*, 234-241.

Kolb, D. A. (1984). *Experiential learning: Experience as the source of learning and development*. Prentice-Hall.

Shen, C., et al. (2023). FlowBlocks: Programmable tangible blocks for network protocol simulation. *Harvard SEAS Technical Report*.
