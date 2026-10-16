# Empaquetado

## Android (APK y AAB)

### Lo que ya se generó
`scripts/build-android.sh` produce en `src/CSharpMastery.App/bin/Release/net10.0-android/`:
- `com.csharpmastery.app-Signed.apk`: instalable directamente. **Firmado con la clave de depuración de Android**, válido para probar en tu propio teléfono, no para publicar.
- `com.csharpmastery.app-Signed.aab`: para Google Play (también con clave de depuración).

Datos del paquete: id `com.csharpmastery.app`, versión 0.1.0 (código 1), Android 7.0 (API 24) o superior, objetivo API 36, sin permiso de Internet.

### Instalar en tu teléfono
1. Copia el APK al teléfono.
2. Ábrelo y permite «instalar apps de origen desconocido» para la app con la que lo abres.
3. O, con el teléfono conectado por USB y la depuración activada: `adb install com.csharpmastery.app-Signed.apk`.

### Firmar con tu propia clave (para distribuir)
```bash
# Una sola vez: crea la clave y guárdala fuera del repositorio.
keytool -genkeypair -v -keystore csharpmastery.keystore -alias csharpmastery \
  -keyalg RSA -keysize 2048 -validity 10000

dotnet publish src/CSharpMastery.App -f net10.0-android -c Release \
  -p:AndroidKeyStore=true \
  -p:AndroidSigningKeyStore=/ruta/csharpmastery.keystore \
  -p:AndroidSigningKeyAlias=csharpmastery \
  -p:AndroidSigningKeyPass=env:CSM_KEY_PASS \
  -p:AndroidSigningStorePass=env:CSM_STORE_PASS
```
Las contraseñas se pasan por variables de entorno; nunca se guardan en el repositorio. Para Google Play usa el `.aab` y activa la firma de apps de Play.

### Requisitos de compilación
.NET SDK 10 con el workload `maui-android`, Android SDK (plataforma 36 y build-tools) y JDK 17 o superior. En Linux, `scripts/setup-android-sdk-linux.sh` documenta cómo se preparó el SDK en el entorno de compilación de esta versión.

### En Android no hay motor de ejecución
Android no permite compilar y ejecutar C# arbitrario dentro de la app. Los ejercicios de código muestran la solución modelo para autoevaluarse, y esa evidencia no cuenta para el dominio. Para practicar con calificación automática usa la versión de escritorio.

## Windows

**No verificado**: no se pudo compilar para Windows en el entorno de esta versión. Estos pasos siguen la configuración del proyecto.

### Requisitos
Windows 10 (1809) o superior, .NET SDK 10 y `dotnet workload install maui` (o Visual Studio con la carga de trabajo de .NET MAUI).

### Carpeta distribuible (sin instalador, configuración actual)
El proyecto usa `WindowsPackageType=None` (app «unpackaged») y Windows App SDK autocontenido.
```powershell
dotnet publish src\CSharpMastery.App -f net10.0-windows10.0.19041.0 -c Release `
  -p:RuntimeIdentifier=win-x64 -p:SelfContained=true -o publish\win-x64
```
El objetivo `PublishCodeRunnerWithApp` del `.csproj` publica también el ejecutor de código autocontenido en `publish\win-x64\runner\`. **Sin esa carpeta los ejercicios no se califican**: la app lo indica en Ajustes con el botón «Probar el motor».

Se distribuye comprimiendo la carpeta `publish\win-x64` o con un instalador:

### Instalador con Inno Setup (opción sencilla)
```ini
[Setup]
AppName=C# Mastery
AppVersion=0.1.0
DefaultDirName={autopf}\CSharpMastery
DefaultGroupName=C# Mastery
OutputBaseFilename=CSharpMastery-0.1.0-setup
ArchitecturesInstallIn64BitMode=x64compatible

[Files]
Source: "publish\win-x64\*"; DestDir: "{app}"; Flags: recursesubdirs

[Icons]
Name: "{group}\C# Mastery"; Filename: "{app}\CSharpMastery.App.exe"
```
Compila el `.iss` con Inno Setup y firma el instalador con tu certificado de firma de código (`signtool sign /fd SHA256 ...`) para evitar el aviso de SmartScreen.

### MSIX (opción con Microsoft Store)
1. Cambia `WindowsPackageType` a `MSIX` en el `.csproj` y agrega `Platforms/Windows/Package.appxmanifest`.
2. Verifica que la carpeta `runner` quede dentro del paquete y que el proceso pueda lanzarse desde la ubicación del paquete.
3. `dotnet publish -f net10.0-windows10.0.19041.0 -c Release -p:RuntimeIdentifierOverride=win-x64 -p:PackageCertificateThumbprint=<huella>`.

## iOS y macOS

**No verificado.** Requieren macOS con Xcode. En iOS no hay motor de ejecución (igual que en Android). En Mac Catalyst el proyecto publica el ejecutor para `osx-arm64`, pero el lanzamiento del proceso dentro del sandbox de apps de macOS no se ha probado.
