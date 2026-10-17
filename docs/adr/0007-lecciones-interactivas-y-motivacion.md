# ADR 0007: Lecciones interactivas generadas y motivación por práctica

**Estado:** aceptada

## Contexto
La primera versión mostraba cada lección como un texto largo seguido de ejercicios. Quien la probó lo describió como «leer un libro»: mucho texto, poca acción y una interfaz sin vida. Aprender a programar exige recuperar, completar, ordenar y depurar código, no solo leerlo.

## Decisión
1. **La lección se recorre haciendo.** `LessonFlowBuilder` convierte cada lección escrita en pasos cortos, de forma determinista (la misma lección produce siempre los mismos pasos):
   - tarjetas de una idea (unas líneas y, casi siempre, un fragmento de código);
   - completar el hueco en el código (opciones generadas por `ClozeGenerator`);
   - recorrido del ejemplo resuelto línea a línea;
   - ordenar líneas (Parsons) sobre una cadena de líneas que dependen entre sí (`LineDependency`);
   - encontrar la línea con el error, o elegir entre la versión correcta y la incorrecta.
   Después siguen la predicción, una pregunta de recuerdo y el ejercicio guiado de la lección.
2. **Nada ambiguo llega al estudiante.** `InteractiveValidationTests` ejecuta con el motor real cada distractor de cada hueco y cada intercambio de líneas vecinas en cada ejercicio de ordenar: el distractor debe no compilar o cambiar la salida, y el intercambio debe cambiar el comportamiento. Lo que no cumple se excluye por clave en `InteractiveExclusions.cs`.
3. **Un pie fijo «Comprobar / Continuar»** en todos los pasos, con una franja verde o roja que explica la respuesta. Todos los pasos derivan de `StepViewModel`.
4. **Motivación ligada a practicar, no a leer.** XP por acertar (paso corto 5, pregunta 10, código 20, lección 15, sin errores +10), racha de días con práctica y meta diaria configurable (20, 50, 100 o 150 XP). Se guardan en los ajustes, así que viajan con la exportación.
5. **Interfaz con identidad propia**, inspirada en apps de idiomas pero sin copiar su marca: violeta como color principal, botones en relieve (`ChunkyButton`), tipografía Nunito (OFL), íconos Material Icons Round (Apache 2.0), mascotas (el robot violeta es la principal y sale la mayor parte del tiempo; tres fotos del autor aparecen de vez en cuando: de frente en algunas introducciones, dormida en algunas tarjetas con mucho texto y pensando junto a algunas preguntas; al terminar, todas juntas dicen «¡Felicidades!») y un camino de lecciones en zigzag.

## Consecuencias
- Leer los apuntes completos sigue disponible en la portada de cada lección, pero no da XP.
- Los pasos generados dependen del texto de la lección: al cambiar contenido hay que volver a ejecutar `InteractiveValidationTests` (y `scripts/update-interactive-exclusions.sh` agrega las exclusiones nuevas si aparecen ambigüedades).
- La racha y la meta no castigan: perder la racha solo la reinicia; no hay vidas ni bloqueo por errores.
- La apariencia final no se ha comprobado en pantalla en este entorno (sin emulador); los flujos se prueban a nivel de ViewModel.
