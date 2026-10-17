#!/usr/bin/env bash
# Ejecuta la validación de los pasos interactivos y agrega a InteractiveExclusions.cs las claves que resultaron
# ambiguas (un distractor que también funciona, o dos líneas que se pueden intercambiar sin cambiar el resultado).
# Úsalo después de modificar el texto o el código de una lección.
set -euo pipefail
cd "$(dirname "$0")/.."
log="$(mktemp)"
dotnet build tests/CSharpMastery.Tests
dotnet test tests/CSharpMastery.Tests --no-build --filter "FullyQualifiedName~Interactive" --logger "console;verbosity=detailed" > "$log" 2>&1 || true
python3 - "$log" <<'PY'
import re, sys
p = 'src/CSharpMastery.Content/InteractiveExclusions.cs'
s = open(p, encoding='utf-8').read()
seen, lines = set(), []
for raw in open(sys.argv[1], encoding='utf-8'):
    m = re.match(r'\s+("([^"]+)",.*)$', raw)
    if not m:
        continue
    key = m.group(2)
    if key in seen or f'"{key}"' in s:
        continue
    seen.add(key)
    lines.append('        ' + m.group(1).strip())
if lines:
    i = s.rindex('    };')
    s = s[:i] + '\n'.join(lines) + '\n' + s[i:]
    open(p, 'w', encoding='utf-8').write(s)
print(f'{len(lines)} claves nuevas agregadas')
PY
grep -E "Pasos|Variantes|Passed!|Failed!" "$log" | sort -u
rm -f "$log"
