#!/usr/bin/env bash
# Arma un Android SDK mínimo en Linux cuando no se puede usar el instalador oficial (sdkmanager / dl.google.com).
# Así se compiló el APK en el entorno de desarrollo de este proyecto. En una máquina normal es preferible
# instalar Android Studio o el SDK oficial y omitir este script.
#
# Procedencia de android.jar: se descarga de un espejo público de terceros en GitHub
# (Reginer/aosp-android-jar). Solo se usa como referencia de compilación (no se incluye en el APK).
# SHA-256 del archivo usado al compilar: 231e29e345e85c7107bde3e08ece052c067ebbe9d72256f036b9a126e4dde0f6
set -euo pipefail
SDK="${ANDROID_HOME:-/opt/android-sdk}"
DOTNET_ROOT="${DOTNET_ROOT:-/opt/dotnet-ms}"
AAPT2="$(ls -d "$DOTNET_ROOT"/packs/Microsoft.Android.Sdk.Linux/*/tools/Linux/aapt2 | sort -V | tail -1)"

sudo apt-get install -y zipalign apksigner openjdk-21-jdk-headless

mkdir -p "$SDK/platforms/android-36" "$SDK/build-tools/36.0.0/lib" "$SDK/platform-tools"
curl -fSL -o "$SDK/platforms/android-36/android.jar" \
  https://raw.githubusercontent.com/Reginer/aosp-android-jar/main/android-36/android.jar
printf "Pkg.Desc=Android SDK Platform 36\nPlatform.Version=16\nAndroidVersion.ApiLevel=36\nPkg.Revision=1\n" > "$SDK/platforms/android-36/source.properties"
printf "ro.build.version.sdk=36\nro.build.version.release=16\n" > "$SDK/platforms/android-36/build.prop"

ln -sf "$(command -v zipalign)" "$SDK/build-tools/36.0.0/zipalign"
ln -sf "$(command -v apksigner)" "$SDK/build-tools/36.0.0/apksigner"
cp /usr/lib/android-sdk/build-tools/debian/apksigner.jar "$SDK/build-tools/36.0.0/lib/apksigner.jar"
ln -sf "$AAPT2" "$SDK/build-tools/36.0.0/aapt2"
printf "Pkg.Desc=Android SDK Build-Tools 36\nPkg.Revision=36.0.0\n" > "$SDK/build-tools/36.0.0/source.properties"

# El validador del SDK exige platform-tools/adb; para compilar no se usa.
printf '#!/bin/sh\necho "adb no instalado" >&2\nexit 1\n' > "$SDK/platform-tools/adb"
chmod +x "$SDK/platform-tools/adb"
printf "Pkg.Revision=36.0.0\n" > "$SDK/platform-tools/source.properties"
echo "SDK listo en $SDK"
