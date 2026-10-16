# Resultados reales de compilación y pruebas

Fecha: 16 de octubre de 2026 (UTC). Entorno: contenedor Linux x86_64 (kernel 6.18), sin pantalla, sin Windows ni macOS.

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
Passed!  - Failed:     0, Passed:   115, Skipped:     0, Total:   115, Duration: 1 m 9 s - CSharpMastery.Tests.dll (net10.0)
```

| Clase de pruebas | Pruebas | Resultado |
|---|---|---|
| ContentValidationTests (todo el contenido ejecutado de verdad) | 7 | Pasan |
| ExecutionTests (motor real en proceso separado) | 25 | Pasan |
| FormattingTests | 6 | Pasan |
| SchedulerTests | 8 | Pasan |
| MasteryEvaluatorTests | 9 | Pasan |
| SessionTests | 16 | Pasan |
| AssessmentTests | 7 | Pasan |
| AdaptationAndAnalyticsTests | 11 | Pasan |
| LearningServiceTests (con SQLite real) | 7 | Pasan |
| PersistenceTests | 6 | Pasan |
| FlowTests (flujos de pantalla en ViewModels) | 13 | Pasan |
| **Total** | **115** | **115 pasan, 0 fallan** |

`ContentValidationTests` ejecuta 168 ejercicios de código (soluciones modelo y código inicial) con 610 casos de prueba, todas las predicciones y todos los ejemplos resueltos.

## 2. Interfaz XAML en Release (`net10.0`)
Mismo script. Compila todas las páginas y vistas con bindings compilados; un binding roto sería error.
```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

## 3. Paquete Android (`scripts/build-android.sh`)
```
Build succeeded.
    0 Warning(s)
    0 Error(s)
Time Elapsed 00:01:15.99
```
| Archivo | Tamaño | SHA-256 |
|---|---|---|
| `com.csharpmastery.app-Signed.apk` | 33.4 MB | `26edaee8e60d052a632f8de844d41a53b9d7c11bb35f4f3aa585c202b238fe84` |
| `com.csharpmastery.app-Signed.aab` | 32.9 MB | `086a616922197cf91a69c073d0f9ccb101f40edfbc476272c2196763139b1437` |

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
| Probar la interfaz visualmente | Sin pantalla; los flujos se probaron a nivel de ViewModel |
