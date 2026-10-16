# ADR 0002: Capas con Presentation independiente de MAUI

**Estado:** aceptada

## Contexto
La lógica pedagógica (repaso, dominio, sesiones, evaluaciones) es lo que más importa probar. La UI de MAUI no se puede probar con facilidad en un entorno sin pantalla.

## Decisión
Separar Domain, Content, Learning, Execution, Persistence y Presentation en bibliotecas `net10.0` sin referencia a MAUI. Los ViewModels usan CommunityToolkit.Mvvm y dependen de interfaces (`INavigationService`, `IDialogService`, `IExecutionService`, `IProgressStore`). `App` solo implementa esas interfaces y contiene XAML.

## Consecuencias
- Los flujos completos (bienvenida, sesión, pausa y reanudación, evaluación, ajustes, exportación) se prueban con xUnit.
- Errores de binding en XAML se detectan al compilar en Release, porque los bindings compilados están marcados como error.
