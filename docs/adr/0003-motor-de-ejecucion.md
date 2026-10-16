# ADR 0003: Ejecutar el código del estudiante en un proceso separado

**Estado:** aceptada

## Contexto
Los ejercicios deben compilarse y ejecutarse de verdad, sin servicios externos. Ejecutar código arbitrario dentro del proceso de la app podría colgarla, agotar su memoria o dañar sus datos.

## Decisión
Un ejecutable aparte, `CSharpMastery.Runner`, que compila con Roslyn y ejecuta las pruebas. La app lo lanza como proceso hijo de un solo uso y se comunica por JSON en la entrada y salida estándar. Controles: política de API analizada semánticamente antes de ejecutar, límite de tiempo por caso y total (mata el árbol de procesos), límite de memoria administrada, límite de salida, entorno mínimo y reinicio de estáticos entre casos.

## Alternativas descartadas
- Ejecutar en el proceso de la app con `AssemblyLoadContext`: no aísla bucles infinitos ni desbordamientos de pila.
- Servicio remoto de compilación: rompe el uso sin conexión y enviaría el código a un servidor.

## Consecuencias
- No es un sandbox del sistema operativo. El modelo de amenaza es una persona practicando en su propio equipo, no código hostil de terceros. Se documenta así y no se presenta como sandbox seguro.
- La política impide que el estudiante use archivos reales, red, procesos y reflexión. Esos temas se enseñan con dobles en memoria.
- Android e iOS no permiten lanzar este proceso; allí los ejercicios se autoevalúan y no cuentan como evidencia de dominio.
- Las pruebas de contenido usan `InProcessRunner` (el mismo compilador y la misma política, en el proceso de pruebas) por velocidad; las pruebas del motor usan el proceso real.
