# Resultados reales de compilación y pruebas

Versión 0.2.0. Fecha: 17 de octubre de 2026 (UTC). Entorno: contenedor Linux x86_64 (kernel 6.18), sin pantalla, sin Windows ni macOS.

Los registros completos están en [`docs/evidencia/`](evidencia): `build-and-test.log`, `build-android.log` y `pruebas.trx` (resultado de pruebas en formato de Visual Studio).

## Herramientas
| Herramienta | Versión |
|---|---|
| .NET SDK | 10.0.402 (con workload `maui-android`) |
| .NET MAUI (Microsoft.Maui.Controls) | 10.0.110 |
| Roslyn (Microsoft.CodeAnalysis.CSharp) | 5.0.0 |
| Microsoft.Data.Sqlite | 10.0.12 |
| CommunityToolkit.Mvvm | 8.4.2 |
| xUnit | 2.9.3 |
| JDK | OpenJDK 21.0.12 |
| Android SDK | plataforma 36 y build-tools armados a mano (ver abajo) |

## 1. Bibliotecas y pruebas (`scripts/build-and-test.sh`)
```
Build succeeded.
    0 Warning(s)
    0 Error(s)
Passed!  - Failed:     0, Passed:   125, Skipped:     0, Total:   125, Duration: 2 m 18 s - CSharpMastery.Tests.dll (net10.0)
```

| Clase de pruebas | Pruebas | Resultado |
|---|---|---|
| ContentValidationTests (todo el contenido ejecutado de verdad) | 7 | Pasan |
| InteractiveValidationTests (cada hueco y cada orden de líneas, con el motor real) | 1 | Pasa |
| ExecutionTests (motor real en proceso separado) | 25 | Pasan |
| FormattingTests | 6 | Pasan |
| SchedulerTests | 8 | Pasan |
| MasteryEvaluatorTests | 9 | Pasan |
| SessionTests | 16 | Pasan |
| AssessmentTests | 7 | Pasan |
| AdaptationAndAnalyticsTests | 11 | Pasan |
| LearningServiceTests (con SQLite real) | 7 | Pasan |
| GamificationTests (XP, racha y meta diaria) | 5 | Pasan |
| PersistenceTests | 6 | Pasan |
| FlowTests (flujos de pantalla en ViewModels, incluida la lección interactiva) | 17 | Pasan |
| **Total** | **125** | **125 pasan, 0 fallan** |

`ContentValidationTests` ejecuta 168 ejercicios de código (soluciones modelo y código inicial) con 610 casos de prueba, todas las predicciones y todos los ejemplos resueltos. `InteractiveValidationTests` revisa 350 pasos generados (53 huecos, 23 de ordenar líneas, 40 de encontrar el error): compila 206 variantes y ejecuta 77 para comprobar que ningún distractor ni intercambio de líneas da el mismo resultado que la respuesta.

Una ejecución anterior falló una vez en `Las_predicciones_coinciden_con_la_ejecucion_real`: un hilo de una ejecución terminada por tiempo seguía escribiendo en la consola de la siguiente (solo ocurre con el ejecutor en proceso que usan las pruebas). Se corrigió con una consola por hilo de ejecución (`Runner/ConsoleRoute.cs`); la ejecución registrada arriba es posterior a la corrección.

## 2. Interfaz XAML en Release (`net10.0`)
Mismo script. Compila todas las páginas y vistas con bindings compilados; un binding roto sería error.
```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

## 3. Paquete Android (`scripts/build-android.sh`)
(Tiempo de la compilación completa; `evidencia/build-android.log` es una recompilación posterior sin cambios, por eso tarda segundos.)
```
Build succeeded.
    0 Warning(s)
    0 Error(s)
Time Elapsed 00:01:59.99
```
| Archivo | Tamaño | SHA-256 |
|---|---|---|
| `com.csharpmastery.app-Signed.apk` (0.2.0) | 35.0 MB | `8a7ae9d98f9b4d2ca29f29aec1e5186a1a288f3fe039e05d146b10ead2d8a35a` |
| `com.csharpmastery.app-Signed.aab` (0.2.0) | 34.5 MB | `53b3071386ada8f0d5418fa29bdaae682f2a04d20be18d9c8826ca7b2f88d4bc` |

El APK incluye las fuentes Nunito y Material Icons Round (`assets/`) y la mascota en todas las densidades (`res/drawable-*/mascot_*.png`).

`apksigner verify` confirma la firma con el certificado de depuración (`CN=Android Debug`). Manifiesto final: `minSdkVersion 24`, `targetSdkVersion 36`, sin permiso de Internet. El APK incluye las bibliotecas compiladas AOT de la app y del contenido para arm64.

### Procedencia del Android SDK
En este entorno no se pudo descargar el SDK oficial de Google, así que se armó a mano:
- `android.jar` (API 36) de un espejo de terceros, `Reginer/aosp-android-jar`, SHA-256 `231e29e345e85c7107bde3e08ece052c067ebbe9d72256f036b9a126e4dde0f6`. Solo se usa para compilar (son las firmas de la API); no se incluye en el APK.
- `zipalign` y `apksigner` de los paquetes de Ubuntu.
Para un paquete de distribución conviene recompilar con el SDK oficial (Android Studio o `sdkmanager`).

## Lo que no se pudo verificar
| Qué | Por qué |
|---|---|
| Ejecutar el APK | No hay emulador ni dispositivo en el entorno |
| Compilar para Windows | Requiere Windows (`net10.0-windows`) |
| Compilar para iOS y macOS | Requiere macOS con Xcode |
| Probar la interfaz visualmente (incluido el diseño nuevo de la 0.2.0) | Sin pantalla ni emulador; los flujos se probaron a nivel de ViewModel y el XAML compila con bindings verificados |
