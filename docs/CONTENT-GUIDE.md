# Guía para agregar contenido

## Dónde va
- Cada nivel es `src/CSharpMastery.Content/NivelN/NivelNModule.cs` y lista sus unidades.
- Cada unidad completa es una clase `UxxNombre` con `public static Unit Build(ContentRegistry r)`.
- Un tema solo con temario se declara con `Outline.Skill(...)` y la app lo muestra como «contenido en preparación».
- Los proyectos integradores están en `Nivel7/Projects.cs`.

## Una habilidad completa
```csharp
new Skill("n4.patrones", UnitId, "Pattern matching", "Resumen…", Tier.Intermediate, ["n1.switch"],
    new Lesson("n4.patrones", "Objetivo…",
        [L.P("Explicación…"), L.Code("código ejecutable", "Título"), L.Fragment("código que no se ejecuta"), L.Tip("…")],
        new WorkedExample("Título", "código", ["nota línea a línea"], "salida esperada"),
        new CommonMistake("descripción", "código incorrecto", "por qué falla", "código corregido"),
        predId, guidedId, independentId, transferId, [recall1, recall2],
        "Criterio de dominio…", minutes))
```
Los ítems se crean con las fábricas de `ContentRegistry`:
- `r.Choice(...)`: opción múltiple; exactamente una correcta (`Ok`) y retroalimentación específica en cada incorrecta (`No`).
- `r.Short(...)`: respuesta corta, comparada como texto, número o salida.
- `r.Code(...)` con `CodeMode.Program` (casos `In(nombre, entrada, salida esperada)`) o `CodeMode.Function` (casos `Is(nombre, expresión, valor esperado)` y `Throws(nombre, expresión, excepción)`).

Cada habilidad necesita una predicción, un ejercicio guiado, uno independiente, uno de transferencia, al menos 2 de recuperación, uno de búsqueda de errores y uno de concepto o recuperación.

## Reglas que verifican las pruebas
`dotnet test --filter "FullyQualifiedName~Content"` compila y ejecuta todo el contenido:
- La solución modelo pasa todos sus casos; el código inicial no.
- La respuesta correcta de una predicción coincide con la salida real. Si el código no compila, la respuesta debe decir «no compila»; si lanza una excepción, debe nombrarla.
- El ejemplo resuelto y los bloques `L.Code` producen la salida declarada. Los bloques cuyo título empieza con «Fragmento» no se ejecutan.
- La versión corregida del error común compila.
- Los prerrequisitos existen y aparecen antes en la ruta.
- Los hitos de proyecto tienen al menos una prueba oculta y la rúbrica al menos 3 criterios.

## Restricciones del motor
- En programas de nivel superior, las clases, records y enums van después de las instrucciones.
- No se permiten archivos reales, red, procesos ni reflexión (`GetType()`, `typeof(...).Assembly`). Para enseñar E/S usa `StringReader` y `StringWriter`.
- Los valores de retorno se formatean así: `true`/`false`, listas `[1, 2]`, cadenas dentro de colecciones con comillas, diccionarios `{'a': 2}`, tuplas `("x", 5)`. Los `decimal` conservan su escala (`90.00`), así que en las pruebas conviene convertir a `double` cuando la escala depende de cómo se escriba la solución.
- Las expresiones de prueba en modo función no pueden declarar clases; los dobles van en el código inicial.
