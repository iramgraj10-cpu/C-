#!/usr/bin/env bash
# Compila todo lo que no depende de una plataforma, ejecuta las pruebas y verifica que el XAML compile.
set -euo pipefail
cd "$(dirname "$0")/.."
dotnet build tests/CSharpMastery.Tests
dotnet test tests/CSharpMastery.Tests --no-build
# Verificación del XAML (Release valida los bindings compilados; un binding roto es error).
dotnet build src/CSharpMastery.App -f net10.0 -c Release
