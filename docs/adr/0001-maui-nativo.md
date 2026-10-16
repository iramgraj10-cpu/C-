# ADR 0001: .NET MAUI con XAML para una app nativa

**Estado:** aceptada

## Contexto
La app debe ser nativa (sin web, PWA, WebView ni Electron), funcionar sin conexión y apuntar primero a Windows y Android, luego a iOS y macOS. El tema que se enseña es C#.

## Decisión
.NET MAUI 10 con XAML y bindings compilados. Un solo código para las cuatro plataformas.

## Consecuencias
- El estudiante usa una app escrita en el mismo lenguaje que aprende.
- El resaltado de código se hace con `FormattedString` y un tokenizador propio en lugar de un editor web.
- La compilación para Windows requiere Windows; la de iOS y macOS requiere macOS con Xcode.
