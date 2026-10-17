# C# Mastery

Aplicación nativa (.NET MAUI) para aprender C# desde cero hasta un nivel profesional sólido, con práctica activa, repetición espaciada, ejercicios de código que se compilan y ejecutan de verdad, y evaluación del dominio basada en evidencia.

Es una aplicación de estudio personal: funciona sin conexión, guarda el progreso en una base SQLite local en el propio dispositivo y no tiene servidor, cuentas ni telemetría. Ninguna función esencial usa servicios de IA de pago.

## Estado de esta versión (0.2.0)

| Área | Estado |
|---|---|
| Niveles 0 a 3 (fundamentos, lógica, métodos y colecciones, POO) | Completos: 45 habilidades con lección de 10 partes |
| Nivel 4 (C# moderno) | 7 de 13 habilidades completas (excepciones, genéricos, delegados, lambdas, LINQ, IEnumerable/IQueryable, iteradores) |
| Niveles 5 y 6 | Solo temario; la app los muestra como «contenido en preparación» |
| Nivel 7 | 3 proyectos integradores con 4 hitos evaluados cada uno y rúbrica |
| Motor de ejecución | Proceso separado con límites (escritorio). En Android/iOS no hay compilador: autoevaluación marcada como no verificada |
| Lecciones | Interactivas: tarjetas cortas, completar código, ordenar líneas, recorrido del ejemplo y encontrar el error, validados con el motor real (ADR 0007) |
| Motivación | XP por acertar, racha de días, meta diaria y celebración al terminar |
| Pruebas automatizadas | 125 pruebas, todas pasan (ver `docs/BUILD-RESULTS.md`) |
| Diseño | Nuevo en 0.2.0; compila, pero no se ha visto en pantalla (no había emulador) |
| Compilación verificada | Bibliotecas, XAML en Release y paquete Android (APK y AAB) en Linux |
| Sin verificar | Windows, iOS y macOS (no hay herramientas para esas plataformas en el entorno donde se construyó) |

El detalle está en [`docs/FEATURES.md`](docs/FEATURES.md).

## Cómo enseña

- **Aprender haciendo**: cada lección se recorre con pasos cortos que piden una acción (completar, ordenar, encontrar el error) y corrigen al instante. Los apuntes completos quedan para consultar.
- **Recuperación activa**: cada sesión empieza recordando, no releyendo.
- **Repetición espaciada** con una variante documentada de SM-2 por habilidad (ver ADR 0004).
- **Ejemplos resueltos con retirada gradual de ayuda**: ejemplo completo → ejercicio guiado → independiente → transferencia.
- **Práctica intercalada** de temas ya practicados.
- **Retroalimentación inmediata** con pistas progresivas antes de la solución; la solución exige intentar primero.
- **Dificultad adaptativa** por desempeño sostenido, no por un error aislado.
- **Metacognición**: preguntas de confianza en algunos ítems (no en todos) y un panel de calibración.
- **Sesiones** de 25, 45 o 75 minutos o personalizadas (10 a 90), con pausa y reanudación. Plantilla de 45 minutos: recuperación 5, concepto nuevo 8, ejemplo guiado 7, práctica 15, aplicación 7, cierre 3.
- **Dominio por evidencia**: leer una lección nunca cuenta como dominio. Se exige precisión en una ventana de intentos calificados automáticamente, variedad de ítems, código independiente, transferencia y, para «retenido», un repaso correcto tras un intervalo. Los criterios se pueden ajustar.
- **Evaluaciones** de unidad y acumulativas con resultados ocultos hasta el final y ruta de recuperación para habilidades débiles.

No se promete un plazo para llegar a nivel experto ni se citan estudios de eficacia: la app mide lo que el estudiante demuestra.

## Estructura

```
src/
  CSharpMastery.Domain         Modelos puros: currículo, ítems, progreso, sesiones
  CSharpMastery.Content        Banco de lecciones, ejercicios y proyectos (código C#)
  CSharpMastery.Learning       Planificador SM-2, dominio, sesiones, evaluaciones, adaptación, analítica
  CSharpMastery.Execution      Contratos del motor, calificador, explicación de errores, servicio de proceso
  CSharpMastery.Runner         Proceso ejecutor: Roslyn, política de API, ejecución de pruebas
  CSharpMastery.Persistence    SQLite con migraciones, exportación e importación
  CSharpMastery.Presentation   ViewModels MVVM (CommunityToolkit.Mvvm), sin dependencia de MAUI
  CSharpMastery.App            Interfaz .NET MAUI (XAML), servicios de plataforma
tests/CSharpMastery.Tests      xUnit: dominio, motor real, persistencia, flujos de UI, validación del contenido
docs/                          Arquitectura, ADR, currículo, funciones, resultados, empaquetado
scripts/                       Compilación y pruebas
```

## Compilar y probar

Requisitos: .NET SDK 10 y, para la app, el workload de MAUI (`dotnet workload install maui`, o `maui-android` en Linux).

```bash
./scripts/build-and-test.sh          # bibliotecas + 115 pruebas + XAML en Release
./scripts/build-android.sh           # APK y AAB en Release (requiere Android SDK y JDK 17+)
```

En Windows, con Visual Studio 2022/2026 y el workload de MAUI, abrir `CSharpMastery.sln` y ejecutar el proyecto `CSharpMastery.App` con el destino Windows. El empaquetado está en [`docs/PACKAGING.md`](docs/PACKAGING.md).

## Documentación

- [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md): capas, flujo de datos, motor de ejecución y seguridad.
- [`docs/adr/`](docs/adr): decisiones de arquitectura.
- [`docs/CURRICULUM.md`](docs/CURRICULUM.md): mapa completo de niveles, unidades, habilidades y prerrequisitos.
- [`docs/FEATURES.md`](docs/FEATURES.md): implementado frente a pendiente.
- [`docs/BUILD-RESULTS.md`](docs/BUILD-RESULTS.md): resultados reales de compilación y pruebas.
- [`docs/PACKAGING.md`](docs/PACKAGING.md): instalador de Windows y paquete de Android.
- [`docs/CONTENT-GUIDE.md`](docs/CONTENT-GUIDE.md): cómo agregar lecciones y cómo se verifican.

## Limitaciones conocidas

- El motor de ejecución no es un sandbox del sistema operativo. Es defensa en profundidad: proceso separado, lista de API permitidas analizada con Roslyn antes de ejecutar, límites de tiempo, memoria y salida, y entorno mínimo. Las pruebas que lo verifican están en `tests/CSharpMastery.Tests/Execution`. Ver ADR 0003.
- Por esa política, el código del estudiante no puede usar archivos reales, red, procesos ni reflexión (`GetType()` incluido). Los temas que los requieren se practican con `StringReader`, `StringWriter` y dobles en memoria.
- Android e iOS no permiten compilar y ejecutar C# arbitrario dentro de la app; allí los ejercicios de código se autoevalúan y esa evidencia no cuenta para el dominio.
- Las compilaciones para Windows, iOS y macOS no se han ejecutado; los proyectos están configurados pero sin verificar.
- El APK de esta versión está firmado con la clave de depuración. Para distribuirlo hay que firmarlo con una clave propia (ver `docs/PACKAGING.md`).

## Recursos de terceros

- Tipografía [Nunito](https://github.com/googlefonts/nunito), licencia SIL Open Font License 1.1 (`src/CSharpMastery.App/Resources/Fonts/Nunito-OFL.txt`).
- Íconos [Material Icons Round](https://github.com/google/material-design-icons), licencia Apache 2.0 (`src/CSharpMastery.App/Resources/Fonts/MaterialIcons-LICENSE.txt`).
- La mascota y el ícono de la app son dibujos propios de este proyecto.
