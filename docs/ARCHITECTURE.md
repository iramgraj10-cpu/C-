# Arquitectura

## Capas

```
            ┌──────────────────────────┐
            │   CSharpMastery.App      │  .NET MAUI: XAML, páginas, servicios de plataforma
            └────────────┬─────────────┘
                         │
            ┌────────────▼─────────────┐
            │ CSharpMastery.Presentation│  ViewModels (CommunityToolkit.Mvvm), navegación y
            └──┬────────┬────────┬─────┘  diálogos por interfaz. Sin referencia a MAUI.
               │        │        │
   ┌───────────▼──┐ ┌───▼──────┐ ┌▼──────────────┐
   │  Learning    │ │ Content  │ │ Persistence   │
   │ SM-2, dominio│ │ banco de │ │ SQLite,       │
   │ sesiones,    │ │ lecciones│ │ migraciones,  │
   │ evaluaciones │ │ y ejerc. │ │ respaldo      │
   └──┬───────┬───┘ └───┬──────┘ └──────┬────────┘
      │       │         │               │
      │  ┌────▼─────┐   │               │
      │  │Execution │◄──┼───────────────┼─────── CSharpMastery.Runner (proceso aparte)
      │  │contratos,│   │               │        Roslyn + política de API + pruebas
      │  │calificad.│   │               │
      │  └────┬─────┘   │               │
   ┌──▼───────▼─────────▼───────────────▼──┐
   │            CSharpMastery.Domain        │  Modelos inmutables, sin dependencias
   └────────────────────────────────────────┘
```

Las dependencias van siempre hacia abajo. `Domain` no depende de nada. `Presentation` no conoce MAUI, así que todos los flujos de pantalla se prueban con xUnit (`tests/.../Presentation/FlowTests.cs`) usando dobles de navegación y diálogos. `App` solo contiene XAML, convertidores, el control de código y la configuración de servicios por plataforma (`MauiProgram.cs`).

## Flujo de una actividad

1. `HomeViewModel` pide a `LearningService` un plan; `SessionPlanner` escala la plantilla de 45 minutos a la duración elegida y elige ítems: repasos vencidos (SM-2), concepto nuevo de la primera habilidad no dominada con prerrequisitos practicados, práctica guiada → independiente, intercalado de temas ya practicados y refuerzo de errores recurrentes.
2. `ActivityViewModel` recorre los pasos; cada ítem es un ViewModel (`ChoiceItemViewModel`, `ShortAnswerItemViewModel`, `CodeExerciseViewModel`).
3. Un ejercicio de código se envía a `IExecutionService`. En escritorio es `ProcessExecutionService`, que lanza `CSharpMastery.Runner`; `CodeGrader` compara salidas o valores de retorno con los casos visibles y ocultos y revisa las construcciones exigidas (por ejemplo, «usa un bucle»).
4. El intento se registra una sola vez (los reintentos no duplican evidencia) con corrección, pistas usadas, confianza, categoría de error y si fue calificado automáticamente.
5. `Sm2Scheduler` actualiza el repaso de la habilidad y `MasteryEvaluator` recalcula el nivel de dominio. `DifficultyAdapter` ajusta la dificultad solo ante desempeño sostenido.
6. `SqliteProgressStore` persiste todo en una base local; la sesión se puede pausar y retomar en el mismo paso.

## Motor de ejecución

- **Proceso separado de un solo uso** por ejecución. El proceso principal de la app nunca compila ni ejecuta código del estudiante. Se mantiene un proceso precalentado para reducir la latencia.
- **Protocolo**: la app envía un `ExecutionRequest` en JSON por la entrada estándar; el ejecutor responde con una línea JSON por evento. El `Console.Out` del estudiante se redirige a un escritor limitado, así que no puede falsificar mensajes del protocolo.
- **Política de API** (`Runner/ApiPolicy.cs`): análisis semántico con Roslyn antes de ejecutar. Lista de espacios de nombres permitidos (colecciones, LINQ, texto, JSON, tareas, etc.) y rechazo explícito de archivos, red, procesos, reflexión, `dynamic`, código `unsafe`, interoperabilidad nativa, variables de entorno, `Environment.Exit` y redirección de la consola. Se permiten tipos de E/S solo en memoria (`StringReader`, `StringWriter`, `MemoryStream`).
- **Límites** (`ExecutionLimits`): 3 s por caso de prueba, 30 s en total, 20 000 caracteres de salida, 256 MB de memoria administrada (`DOTNET_GCHeapHardLimit`). Al vencer el tiempo se mata el árbol de procesos.
- **Aislamiento entre casos**: los campos estáticos se reinician entre casos de prueba para que un caso no contamine otro.
- **Verificado por pruebas** (`tests/.../Execution/ExecutionTests.cs`, contra el proceso real): bucle infinito detenido, salida excesiva truncada, desbordamiento de pila sin afectar a la app, consumo de memoria detenido, 10 API peligrosas rechazadas antes de ejecutar, soluciones alternativas válidas aceptadas, salida «memorizada» que no pasa las pruebas ocultas.
- **No es un sandbox del sistema operativo**: no hay contenedor, AppContainer ni seccomp. Es defensa en profundidad para código de práctica escrito por la misma persona que usa la app. Ver ADR 0003.
- **Android e iOS**: no se puede compilar y ejecutar C# arbitrario dentro de la app. Se usa `UnavailableExecutionService`: el estudiante ve la solución modelo y se autoevalúa, y esa evidencia se marca como no verificada y no cuenta para el dominio.

## Persistencia

SQLite (`Microsoft.Data.Sqlite`) con migraciones versionadas en `schema_migrations`. Tablas: `profile`, `skill_progress`, `attempts`, `sessions`, `assessments`, `projects`, `settings`, `drafts`, `submissions`. Una base creada por una versión más nueva se rechaza en lugar de corromperse. Exportación e importación a JSON con versión de esquema; un respaldo de una versión futura se rechaza. Los borradores de código se guardan mientras se escribe.

## Contenido

El banco de contenidos es código C# (`src/CSharpMastery.Content`), no archivos sueltos, para que el compilador detecte referencias rotas. Cada nivel es un módulo; cada unidad, una clase con un método `Build`. La prueba `ContentValidationTests` ejecuta de verdad todo el contenido. Ver `docs/CONTENT-GUIDE.md` y ADR 0005.

## Interfaz

- Shell con cinco secciones: Inicio, Ruta, Progreso, Proyectos y Ajustes, más la bienvenida inicial.
- Bindings compilados (`x:DataType`); en Release un binding roto es error de compilación (`XC0045`, `XC0022`, `XC0025` como errores).
- Tema claro, oscuro o del sistema; descripciones semánticas para lectores de pantalla en los controles principales.
- Pantalla de actividad: barra superior (cerrar, progreso, racha de aciertos y XP), el paso actual y un pie fijo con «Comprobar / Continuar» y la franja de retroalimentación. Cada paso es un `StepViewModel` (Presentation) con su vista (`App/Views`): tarjeta, hueco, ordenar líneas, recorrido, encontrar el error, comparar, pregunta, respuesta corta, código y celebración. Los pasos interactivos los genera `Learning/Interactive/LessonFlowBuilder` (ADR 0007).
- Controles propios: `ChunkyButton` (botón en relieve), `FatProgressBar`, `RingProgress` (meta diaria) y `CodeView` (código con números de línea y tramo resaltado).
- Atajos de escritorio en la pantalla de actividad: Ctrl+Enter comprueba o continúa, F5 ejecuta, Ctrl+H pide pista.
- Resaltado de sintaxis propio (`Presentation/Highlighting/CSharpHighlighter.cs`), sin WebView.

## Decisiones

Ver [`docs/adr/`](adr).
