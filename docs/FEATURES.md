# Funciones: implementado y pendiente

«Verificado» significa cubierto por pruebas automatizadas que se ejecutaron (ver `BUILD-RESULTS.md`). «Compila» significa que el código de la interfaz compila en Release, pero no se ha probado en un dispositivo.

## Implementado

### Aprendizaje
| Función | Evidencia |
|---|---|
| Bienvenida con nivel de partida y nombre | Verificado (`FlowTests`) |
| Sesiones de 25, 45, 75 o 10 a 90 minutos personalizados; fuera de rango se rechaza | Verificado (`SessionTests`, `FlowTests`) |
| Plantilla de sesión 5/8/7/15/7/3 escalada y redistribuida según el estado | Verificado (`SessionTests`) |
| Pausa y reanudación en el mismo paso; solo cuenta tiempo activo; inactividad larga no cuenta; aviso a los 90 minutos | Verificado (`SessionTests`, `FlowTests`) |
| Repasos vencidos primero, intercalado de temas practicados, refuerzo de errores recurrentes | Verificado (`SessionTests`) |
| Lección interactiva: tarjetas de una idea, completar el hueco, recorrido del ejemplo línea a línea, ordenar líneas, encontrar el error, predicción, recuerdo y ejercicio guiado (ADR 0007) | Verificado (`FlowTests`) |
| Cada hueco y cada ejercicio de ordenar tiene una sola respuesta correcta: los distractores se ejecutan con el motor real y deben fallar o cambiar la salida | Verificado (`InteractiveValidationTests`) |
| Pie fijo «Comprobar / Continuar» con franja verde o roja, «Reintentar» y «Ver la respuesta» | Verificado el modelo (`FlowTests`); la vista compila |
| XP por acertar, racha de días, meta diaria configurable (20/50/100/150 XP) | Verificado (`GamificationTests`, `FlowTests`) |
| Lección de 10 partes con retirada gradual de ayuda (ejemplo → guiado → independiente → transferencia) | Verificado (`ContentValidationTests`, `FlowTests`) |
| Pistas progresivas; la solución exige intentar y pedir pista | Verificado (`FlowTests`) |
| Retroalimentación específica por opción y tolerancia en respuestas cortas | Verificado (`AssessmentTests`) |
| Preguntas de confianza en algunos ítems y análisis de calibración | Verificado (`AdaptationAndAnalyticsTests`) |
| Dificultad adaptativa por desempeño sostenido | Verificado (`AdaptationAndAnalyticsTests`) |
| Variante de SM-2 por habilidad (ADR 0004) | Verificado (`SchedulerTests`) |
| Dominio por evidencia con 5 niveles y criterios configurables (ADR 0006) | Verificado (`MasteryEvaluatorTests`, `LearningServiceTests`) |
| Evaluaciones de unidad y acumulativas, resultados ocultos hasta el final, ruta de recuperación | Verificado (`AssessmentTests`, `FlowTests`) |
| 3 proyectos integradores con hitos evaluados y rúbrica | Verificado (`ContentValidationTests`) |

### Motor de ejecución (escritorio)
| Función | Evidencia |
|---|---|
| Compilación y ejecución reales en proceso separado, con entrada estándar | Verificado (`ExecutionTests`) |
| Errores de compilación explicados en español con número de línea | Verificado |
| Modo programa (salida) y modo función (valor de retorno o excepción esperada) | Verificado |
| Pruebas visibles y ocultas; salida memorizada no pasa las ocultas | Verificado |
| Construcciones exigidas (bucle, recursión, LINQ, etc.) | Verificado |
| Límite de tiempo, salida y memoria; desbordamiento de pila aislado | Verificado |
| Política de API: archivos, red, procesos, reflexión, entorno, `dynamic`, `unsafe` rechazados | Verificado (10 casos) |
| Borradores de código guardados automáticamente | Verificado (`FlowTests`) |

### Datos e interfaz
| Función | Evidencia |
|---|---|
| SQLite local con migraciones; base más nueva se rechaza | Verificado (`PersistenceTests`) |
| Exportar e importar progreso; respaldo futuro se rechaza | Verificado |
| Panel de progreso con secciones separadas (visto, practicado, dominado, retenido, hábitos, errores, calibración, por habilidad) | Verificado el modelo (`FlowTests`); la vista compila |
| Camino de lecciones en zigzag por nivel y unidad, con nodo por habilidad (bloqueada, disponible, en curso, dominada, retenida) y la siguiente recomendada | Verificado el modelo (`FlowTests`); la vista compila |
| Diseño propio con mascota, botones en relieve, tipografía Nunito e íconos Material; celebración al terminar | Compila; no visto en pantalla |
| Tema claro, oscuro o del sistema | Compila |
| Atajos de teclado en escritorio (Ctrl+Enter comprueba o continúa, F5, Ctrl+H) | Compila; no probado en Windows |
| Resaltado de sintaxis sin WebView | Compila |
| Paquete Android (APK y AAB) | Compilado en Release (ver `BUILD-RESULTS.md`); no ejecutado en un dispositivo o emulador |

## Pendiente

| Pendiente | Nota |
|---|---|
| Nivel 4, unidad 3: pattern matching, métodos de extensión, `IDisposable`, archivos y JSON, inmutabilidad, organización de soluciones | Temario definido; se muestra como «en preparación» |
| Nivel 5: CLI, NuGet, depuración, pruebas, refactorización, Git, documentación, rendimiento, arquitectura | Temario definido |
| Nivel 6: tareas, async/await, cancelación, concurrencia, hilos, HTTP, JSON, SQLite, seguridad | Temario definido. Varios temas requieren E/S real, que la política del motor impide; habrá que diseñar dobles en memoria |
| Compilar y probar en Windows | Requiere Windows con el workload de MAUI |
| Compilar y probar en iOS y macOS | Requiere macOS con Xcode. En Mac Catalyst el lanzamiento del proceso ejecutor no está verificado |
| Probar el APK en un dispositivo o emulador | No había emulador disponible; el diseño nuevo no se ha visto en pantalla |
| Pruebas de interfaz automatizadas (Appium o similar) | No implementadas |
| Instalador MSIX firmado | Documentado en `PACKAGING.md`; la configuración actual es «unpackaged» |
| Sandbox del sistema operativo para el ejecutor (AppContainer en Windows) | No implementado; ver ADR 0003 |
| Más proyectos integradores (niveles 4 a 6) | Los tres actuales cubren niveles 1 a 3 |
