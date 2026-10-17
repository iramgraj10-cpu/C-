#!/usr/bin/env bash
# Ejecuta los bloques de código de las explicaciones y regenera src/CSharpMastery.Content/CodeOutputData.cs con su
# salida real. Los ejercicios de completar huecos muestran esa salida en el enunciado.
# Úsalo después de modificar el código de una lección (antes de scripts/update-interactive-exclusions.sh).
set -euo pipefail
cd "$(dirname "$0")/.."
dotnet build tests/CSharpMastery.Tests
UPDATE_CODE_OUTPUTS=1 dotnet test tests/CSharpMastery.Tests --no-build --filter "FullyQualifiedName~Las_salidas_de_los_bloques"
grep -c '^        \["' src/CSharpMastery.Content/CodeOutputData.cs | sed 's/^/Salidas registradas: /'
