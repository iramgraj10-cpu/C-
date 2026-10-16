# ADR 0004: Variante de SM-2 por habilidad

**Estado:** aceptada

## Contexto
Se necesita repetición espaciada sin conexión, transparente y configurable.

## Decisión
SM-2 (Wozniak, 1987) aplicado por habilidad, no por tarjeta, con estas diferencias:
1. Al fallar, la facilidad baja 0.2 además de reiniciar el intervalo.
2. El intervalo tiene un tope de 120 días.
3. La calidad 0 a 5 se deriva del intento (correcto, pistas usadas, confianza declarada), no de una autocalificación. La rapidez no suma.

## Consecuencias
- No se afirma que sea óptimo; los parámetros están en `SchedulerOptions` y cubiertos por `SchedulerTests`.
- La cola de repaso toma un ítem por habilidad vencida.
