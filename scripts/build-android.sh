#!/usr/bin/env bash
# Compila el paquete Android (APK y AAB) en Release.
set -euo pipefail
cd "$(dirname "$0")/.."
export DOTNET_ROOT="${DOTNET_ROOT:-/opt/dotnet-ms}"
export PATH="$DOTNET_ROOT:$PATH"
export ANDROID_HOME="${ANDROID_HOME:-/opt/android-sdk}"
export JAVA_HOME="${JAVA_HOME:-/usr/lib/jvm/java-21-openjdk-amd64}"
dotnet build src/CSharpMastery.App -f net10.0-android -c Release "$@"
ls -la src/CSharpMastery.App/bin/Release/net10.0-android/*.apk src/CSharpMastery.App/bin/Release/net10.0-android/*.aab
