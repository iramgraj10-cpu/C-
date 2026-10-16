# ADR 0005: El contenido es código verificado por ejecución

**Estado:** aceptada

## Contexto
Una lección con un ejemplo que no compila o una respuesta mal calculada enseña algo falso.

## Decisión
Las lecciones y ejercicios se escriben como código C# en `CSharpMastery.Content`. `ContentValidationTests` compila y ejecuta todo: las soluciones modelo deben pasar todas sus pruebas, el código inicial no debe pasar, las respuestas de predicción se comparan con la salida real, los ejemplos resueltos y bloques de código deben producir la salida declarada, y la versión corregida de cada error común debe compilar.

## Consecuencias
- Agregar contenido exige pasar las pruebas; varios errores de contenido se detectaron así antes de publicarse.
- Los temas aún sin lección quedan como temario y la app los muestra como «contenido en preparación», sin ítems inventados.
