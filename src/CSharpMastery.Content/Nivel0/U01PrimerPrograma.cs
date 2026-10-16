using CSharpMastery.Content.Infrastructure;
using CSharpMastery.Domain.Curriculum;
using CSharpMastery.Domain.Items;
using CSharpMastery.Domain.Progress;
using static CSharpMastery.Content.Infrastructure.ContentRegistry;
using static CSharpMastery.Content.Infrastructure.L;

namespace CSharpMastery.Content.Nivel0;

internal static class U01PrimerPrograma
{
    public const string UnitId = "n0.u1";

    public static Unit Build(ContentRegistry r) => new(UnitId, "n0", "Tu primer programa",
        "Qué es un programa, cómo se compila y se ejecuta, y cómo mostrar información en la consola.",
        [Programa(r), Sintaxis(r), Salida(r)]);

    private static Skill Programa(ContentRegistry r)
    {
        const string s = "n0.programa";
        r.Choice("n0.programa.pred1", s, ItemPurpose.Prediction,
            "¿Qué muestra este programa en la consola?",
            "Las instrucciones se ejecutan en orden, de arriba hacia abajo, y cada WriteLine escribe una línea completa.",
            [
                Ok("Uno\nDos", "Correcto: cada WriteLine escribe su texto y un salto de línea."),
                No("Dos\nUno", "Las instrucciones se ejecutan en el orden en que aparecen, de arriba hacia abajo."),
                No("UnoDos", "WriteLine agrega un salto de línea al final; Write no lo hace."),
                No("No compila", "El programa es válido: dos instrucciones bien formadas."),
            ],
            code: """
                Console.WriteLine("Uno");
                Console.WriteLine("Dos");
                """);

        r.Choice("n0.programa.concepto1", s, ItemPurpose.Concept,
            "¿Qué hace el compilador de C#?",
            "El compilador revisa que el código sea válido y lo traduce a un lenguaje intermedio (IL) que el runtime de .NET ejecuta.",
            [
                Ok("Revisa el código y lo traduce a un formato que .NET puede ejecutar.", "Exacto. Si encuentra errores, no genera el programa."),
                No("Ejecuta el código línea por línea mientras lo lee.", "Eso describe a un intérprete. C# se compila primero y se ejecuta después."),
                No("Corrige automáticamente los errores del programador.", "El compilador detecta errores y los informa, pero no los corrige."),
                No("Descarga bibliotecas de Internet.", "Eso lo hace NuGet al restaurar paquetes, no el compilador."),
            ]);

        r.Short("n0.programa.recall1", s, ItemPurpose.Recall,
            "Sin mirar tus notas: ¿qué instrucción completa usas para mostrar la línea Hola en la consola?",
            "Console.WriteLine(\"Hola\"); escribe el texto y luego un salto de línea. Recuerda el punto y coma final.",
            ["Console.WriteLine(\"Hola\");", "Console.WriteLine(\"Hola\")"],
            wrong: new() { ["console.writeline(\"Hola\");"] = "C# distingue mayúsculas: es Console.WriteLine." });

        r.Choice("n0.programa.recall2", s, ItemPurpose.Recall,
            "Ordena las etapas: ¿qué ocurre primero cuando ejecutas un programa de C#?",
            "Primero se compila el código fuente; si no hay errores, el runtime de .NET ejecuta el resultado.",
            [
                Ok("Se compila el código fuente y luego se ejecuta el resultado.", "Correcto: compilación y después ejecución."),
                No("Se ejecuta y, si falla, se compila.", "Sin compilación no hay nada que ejecutar."),
                No("Se ejecuta directamente el archivo .cs.", "El archivo .cs es texto; primero debe compilarse."),
            ]);

        r.Choice("n0.programa.bug1", s, ItemPurpose.BugFinding,
            "Este programa no compila. ¿Cuál es el error?",
            "C# distingue mayúsculas de minúsculas: console no es lo mismo que Console. El compilador dice que el nombre 'console' no existe (CS0103).",
            [
                Ok("console debe escribirse con C mayúscula: Console.", "Correcto. Los nombres en C# son sensibles a mayúsculas."),
                No("Falta el punto y coma.", "El punto y coma está presente al final de la línea."),
                No("El texto debe ir entre comillas simples.", "Las cadenas de texto van entre comillas dobles."),
            ],
            code: """
                console.WriteLine("Hola");
                """,
            targets: [ErrorCategory.UndeclaredIdentifier]);

        r.Code("n0.programa.guiado", s, ItemPurpose.Guided, CodeMode.Program,
            "Completa el programa para que muestre exactamente dos líneas: «Hola, mundo» y después «Me llamo Ada».",
            """
            Console.WriteLine("Hola, mundo");
            // Agrega aquí la segunda línea
            """,
            """
            Console.WriteLine("Hola, mundo");
            Console.WriteLine("Me llamo Ada");
            """,
            [In("Salida completa", "", "Hola, mundo\nMe llamo Ada")],
            "Cada línea de salida necesita su propia instrucción Console.WriteLine.",
            ["Copia la estructura de la primera línea.", "Escribe Console.WriteLine(\"Me llamo Ada\"); debajo del comentario."]);

        r.Code("n0.programa.indep", s, ItemPurpose.Independent, CodeMode.Program,
            "Escribe un programa que muestre esta ficha, con tres líneas exactas:\nNombre: Ada\nLenguaje: C#\nNivel: 0",
            "// Escribe tu programa aquí\n",
            """
            Console.WriteLine("Nombre: Ada");
            Console.WriteLine("Lenguaje: C#");
            Console.WriteLine("Nivel: 0");
            """,
            [In("Ficha", "", "Nombre: Ada\nLenguaje: C#\nNivel: 0")],
            "Tres instrucciones WriteLine, una por línea, en el orden pedido.",
            ["Necesitas una instrucción por cada línea de la ficha.", "Respeta los dos puntos y el espacio: «Nombre: Ada»."]);

        r.Code("n0.programa.app", s, ItemPurpose.Transfer, CodeMode.Program,
            "Aplicación: dibuja este marco con texto en la consola (5 líneas exactas):\n+--------+\n|        |\n|   C#   |\n|        |\n+--------+",
            "// Dibuja el marco\n",
            """
            Console.WriteLine("+--------+");
            Console.WriteLine("|        |");
            Console.WriteLine("|   C#   |");
            Console.WriteLine("|        |");
            Console.WriteLine("+--------+");
            """,
            [In("Marco", "", "+--------+\n|        |\n|   C#   |\n|        |\n+--------+")],
            "La consola muestra los caracteres tal cual: los espacios dentro de las comillas también cuentan.",
            ["Cuenta los guiones y los espacios de cada línea.", "Hay 8 caracteres entre los bordes en cada línea."],
            difficulty: 2);

        return new Skill(s, UnitId, "Qué es un programa: compilación y ejecución",
            "Un programa es una secuencia de instrucciones; el compilador la valida y la traduce para que .NET la ejecute.",
            Tier.Essential, [],
            new Lesson(s,
                "Explicar qué hace el compilador y escribir un programa que muestre texto en la consola en el orden indicado.",
                [
                    P("Un programa es una lista de instrucciones que la computadora ejecuta en orden. En C# escribes esas instrucciones en archivos de texto con extensión .cs, llamados código fuente."),
                    P("Antes de ejecutarse, el código pasa por el compilador. El compilador revisa que todo esté bien escrito (sintaxis y tipos) y lo traduce a un lenguaje intermedio llamado IL, que se guarda en un ensamblado (.dll o .exe). Después, el runtime de .NET carga ese ensamblado y lo convierte en instrucciones de máquina mientras se ejecuta."),
                    P("Si el compilador encuentra un error, no genera el programa: debes corregirlo primero. Por eso distinguimos entre errores de compilación (el programa ni siquiera se crea) y errores de ejecución (el programa se crea, pero falla al correr)."),
                    Code("Console.WriteLine(\"Hola, mundo\");", "El programa más pequeño: una sola instrucción."),
                    P("En C# moderno puedes escribir instrucciones directamente en el archivo principal (instrucciones de nivel superior). El compilador genera por ti el método Main, que es el punto de entrada del programa."),
                    Note("Console es una clase de .NET que representa la consola. WriteLine es un método que escribe un texto y después un salto de línea."),
                ],
                new WorkedExample("Dos líneas en orden",
                    """
                    // Esto es un comentario: el compilador lo ignora
                    Console.WriteLine("Hola, mundo");
                    Console.WriteLine("Estoy aprendiendo C#");
                    """,
                    [
                        Line(1, "Los comentarios empiezan con // y sirven para explicar el código a las personas."),
                        Line(2, "Llama al método WriteLine de la clase Console con el texto entre comillas dobles. El punto y coma termina la instrucción."),
                        Line(3, "Se ejecuta después de la línea 2: el orden del código es el orden de ejecución."),
                    ],
                    "Hola, mundo\nEstoy aprendiendo C#"),
                new CommonMistake(
                    "Escribir console.writeline en minúsculas.",
                    "console.writeline(\"Hola\");",
                    "C# distingue mayúsculas de minúsculas. El nombre correcto es Console.WriteLine; con otra capitalización el compilador no encuentra el nombre (error CS0103).",
                    "Console.WriteLine(\"Hola\");"),
                "n0.programa.pred1", "n0.programa.guiado", "n0.programa.indep", "n0.programa.app",
                ["n0.programa.recall1", "n0.programa.recall2", "n0.programa.concepto1"],
                "Escribes sin ayuda un programa que muestra varias líneas exactas en orden y explicas la diferencia entre compilar y ejecutar.",
                12));
    }

    private static Skill Sintaxis(ContentRegistry r)
    {
        const string s = "n0.sintaxis";
        r.Choice("n0.sintaxis.pred1", s, ItemPurpose.Prediction,
            "¿Qué muestra este programa?",
            "Console.WriteLine recibe una expresión. 2 + 3 es una expresión numérica que vale 5; \"2 + 3\" es solo texto.",
            [
                Ok("5\n2 + 3", "Correcto: la primera línea evalúa la suma; la segunda imprime el texto literal."),
                No("2 + 3\n2 + 3", "Sin comillas, 2 + 3 se evalúa como una expresión numérica."),
                No("5\n5", "Lo que está entre comillas es texto literal y no se evalúa."),
                No("No compila", "Ambas instrucciones son válidas."),
            ],
            code: """
                Console.WriteLine(2 + 3);
                Console.WriteLine("2 + 3");
                """);

        r.Choice("n0.sintaxis.concepto1", s, ItemPurpose.Concept,
            "¿Cuál es la diferencia entre una instrucción y una expresión?",
            "Una expresión produce un valor (2 + 3, \"hola\", x > 5). Una instrucción es una acción completa del programa y normalmente termina en punto y coma.",
            [
                Ok("Una expresión produce un valor; una instrucción realiza una acción y suele terminar con ;", "Correcto."),
                No("Son lo mismo con distinto nombre.", "No: 2 + 3 es una expresión, pero por sí sola no es una instrucción válida."),
                No("Una expresión siempre ocupa varias líneas.", "La longitud no define si algo es expresión o instrucción."),
            ]);

        r.Choice("n0.sintaxis.bug1", s, ItemPurpose.BugFinding,
            "¿Por qué no compila este programa?",
            "Cada instrucción debe terminar con punto y coma. El compilador reporta CS1002 (se esperaba ;) cerca de la línea 1.",
            [
                Ok("Falta el punto y coma al final de la primera línea.", "Correcto. El error se reporta en esa línea o la siguiente."),
                No("Falta el punto y coma en la segunda línea.", "La segunda línea sí termina en ;."),
                No("Los paréntesis están mal.", "Los paréntesis están balanceados en ambas líneas."),
            ],
            code: """
                Console.WriteLine("Inicio")
                Console.WriteLine("Fin");
                """,
            targets: [ErrorCategory.Syntax]);

        r.Short("n0.sintaxis.recall1", s, ItemPurpose.Recall,
            "¿Con qué carácter termina normalmente una instrucción en C#?",
            "El punto y coma (;) marca el final de una instrucción.",
            [";", "punto y coma", "el punto y coma"]);

        r.Choice("n0.sintaxis.recall2", s, ItemPurpose.Recall,
            "¿Cuál de estas líneas es una instrucción válida por sí sola?",
            "Una llamada a un método es una instrucción válida. Una expresión suelta como 4 * 2; no hace nada y el compilador la rechaza (CS0201).",
            [
                Ok("Console.WriteLine(4 * 2);", "Correcto: es una llamada a método."),
                No("4 * 2;", "Una expresión aritmética suelta no es una instrucción válida (CS0201).", ErrorCategory.Syntax),
                No("\"hola\";", "Un valor suelto no es una instrucción.", ErrorCategory.Syntax),
            ]);

        r.Code("n0.sintaxis.guiado", s, ItemPurpose.Guided, CodeMode.Program,
            "El programa tiene dos errores de sintaxis. Corrígelos para que muestre «Sintaxis» y luego «Correcta».",
            """
            Console.WriteLine("Sintaxis")
            Console.WriteLine("Correcta";
            """,
            """
            Console.WriteLine("Sintaxis");
            Console.WriteLine("Correcta");
            """,
            [In("Salida", "", "Sintaxis\nCorrecta")],
            "Cada instrucción termina en ; y cada paréntesis abierto se cierra.",
            ["Revisa el final de la línea 1.", "En la línea 2 falta cerrar un paréntesis antes del ;."],
            targets: [ErrorCategory.Syntax]);

        r.Code("n0.sintaxis.indep", s, ItemPurpose.Independent, CodeMode.Program,
            "Escribe un programa que muestre en una línea el resultado de 7 * 6 y en la siguiente el texto «7 * 6».",
            "",
            """
            Console.WriteLine(7 * 6);
            Console.WriteLine("7 * 6");
            """,
            [In("Salida", "", "42\n7 * 6")],
            "Sin comillas, 7 * 6 es una expresión que vale 42; con comillas es texto.",
            ["Usa dos instrucciones WriteLine.", "La primera recibe la expresión sin comillas."]);

        r.Code("n0.sintaxis.app", s, ItemPurpose.Transfer, CodeMode.Program,
            "Aplicación: una tienda vende 3 cuadernos a 25 pesos y 2 plumas a 12 pesos. Muestra en tres líneas: el costo de los cuadernos, el de las plumas y el total, calculando cada valor con una expresión (no escribas los resultados a mano).",
            "",
            """
            Console.WriteLine(3 * 25);
            Console.WriteLine(2 * 12);
            Console.WriteLine(3 * 25 + 2 * 12);
            """,
            [In("Totales", "", "75\n24\n99")],
            "Las expresiones aritméticas se evalúan antes de imprimirse; la multiplicación tiene prioridad sobre la suma.",
            ["Cuadernos: 3 * 25.", "El total es la suma de ambas multiplicaciones."],
            difficulty: 2);

        return new Skill(s, UnitId, "Sintaxis: instrucciones y expresiones",
            "Reglas de escritura de C#: instrucciones, expresiones, punto y coma, llaves y comentarios.",
            Tier.Essential, ["n0.programa"],
            new Lesson(s,
                "Distinguir instrucciones de expresiones y corregir errores de sintaxis comunes (;, paréntesis y comillas).",
                [
                    P("La sintaxis son las reglas de escritura de un lenguaje. Si no se cumplen, el compilador no puede entender el código y muestra un error de compilación."),
                    P("Una expresión es algo que produce un valor: 2 + 3, \"hola\", 10 > 4. Una instrucción es una acción completa: mostrar algo, guardar un valor, tomar una decisión. La mayoría de las instrucciones terminan en punto y coma."),
                    Code("""
                        Console.WriteLine(10 - 4);   // la expresión 10 - 4 vale 6
                        Console.WriteLine("10 - 4"); // texto literal: no se evalúa
                        """),
                    P("Los paréntesis, llaves y comillas siempre van en pares. Un error muy común es abrir un paréntesis o una comilla y olvidar cerrarlo."),
                    Tip("Cuando el compilador marca un error de punto y coma, mira la línea indicada y también la anterior: a veces el problema está justo antes."),
                ],
                new WorkedExample("Expresiones dentro de instrucciones",
                    """
                    Console.WriteLine("Precio unitario:");
                    Console.WriteLine(15);
                    Console.WriteLine("Total por 4 piezas:");
                    Console.WriteLine(15 * 4);
                    """,
                    [
                        Line(1, "Instrucción que muestra un texto literal."),
                        Line(2, "15 es una expresión muy simple: un número literal."),
                        Line(4, "15 * 4 es una expresión; se calcula (60) y luego se muestra."),
                    ],
                    "Precio unitario:\n15\nTotal por 4 piezas:\n60"),
                new CommonMistake(
                    "Olvidar el punto y coma o un paréntesis de cierre.",
                    "Console.WriteLine(\"Hola\"",
                    "El compilador espera ) y ; para terminar la instrucción. Reporta CS1026 (se esperaba ')') y CS1002 (se esperaba ';').",
                    "Console.WriteLine(\"Hola\");"),
                "n0.sintaxis.pred1", "n0.sintaxis.guiado", "n0.sintaxis.indep", "n0.sintaxis.app",
                ["n0.sintaxis.recall1", "n0.sintaxis.recall2", "n0.sintaxis.concepto1"],
                "Identificas y corriges sin ayuda errores de punto y coma y paréntesis, y usas expresiones calculadas en lugar de resultados escritos a mano.",
                12));
    }

    private static Skill Salida(ContentRegistry r)
    {
        const string s = "n0.salida";
        r.Choice("n0.salida.pred1", s, ItemPurpose.Prediction,
            "¿Qué muestra este programa?",
            "Write no agrega salto de línea, así que «A» y «B» quedan juntas; WriteLine sí lo agrega después de «C».",
            [
                Ok("ABC\nD", "Correcto: Write deja el cursor en la misma línea."),
                No("A\nB\nC\nD", "Write no salta de línea; solo WriteLine lo hace."),
                No("AB\nC\nD", "WriteLine(\"C\") escribe C en la misma línea que AB y luego salta."),
            ],
            code: """
                Console.Write("A");
                Console.Write("B");
                Console.WriteLine("C");
                Console.WriteLine("D");
                """);

        r.Short("n0.salida.pred2", s, ItemPurpose.Prediction,
            "Escribe exactamente lo que muestra este programa (respeta los saltos de línea).",
            "Console.WriteLine() sin argumentos escribe una línea vacía, por eso hay un renglón en blanco entre Inicio y Fin.",
            ["Inicio\n\nFin"],
            match: AnswerMatch.Output,
            code: """
                Console.WriteLine("Inicio");
                Console.WriteLine();
                Console.WriteLine("Fin");
                """,
            difficulty: 2);

        r.Choice("n0.salida.concepto1", s, ItemPurpose.Concept,
            "¿Cómo se muestra una comilla doble dentro de un texto?",
            "Dentro de una cadena, \\\" representa una comilla doble. Las secuencias de escape empiezan con barra invertida: \\n es salto de línea y \\t es tabulador.",
            [
                Ok("Con la secuencia de escape \\\"", "Correcto."),
                No("Con dos comillas simples ''", "Las comillas simples delimitan un char, no producen una comilla doble."),
                No("No es posible", "Sí es posible con secuencias de escape."),
            ]);

        r.Short("n0.salida.recall1", s, ItemPurpose.Recall,
            "¿Qué método escribe texto en la consola SIN agregar un salto de línea al final?",
            "Console.Write escribe el texto y deja el cursor en la misma línea.",
            ["Console.Write", "Write", "Console.Write()"]);

        r.Choice("n0.salida.recall2", s, ItemPurpose.Recall,
            "¿Qué secuencia de escape produce un salto de línea dentro de una cadena?",
            "\\n representa un salto de línea. Así \"a\\nb\" se muestra en dos líneas.",
            [
                Ok("\\n", "Correcto."),
                No("\\t", "\\t es un tabulador."),
                No("/n", "Las secuencias de escape usan barra invertida (\\)."),
            ]);

        r.Choice("n0.salida.bug1", s, ItemPurpose.BugFinding,
            "Se quería mostrar: Ella dijo \"hola\". ¿Por qué no compila?",
            "La comilla interior cierra la cadena antes de tiempo. Hay que escaparla con \\\".",
            [
                Ok("Las comillas internas cierran el texto antes de tiempo; deben escribirse como \\\".", "Correcto."),
                No("Console.WriteLine no acepta texto largo.", "No hay límite práctico de longitud."),
                No("Falta un punto y coma.", "El punto y coma está presente."),
            ],
            code: """
                Console.WriteLine("Ella dijo "hola"");
                """,
            targets: [ErrorCategory.Syntax]);

        r.Code("n0.salida.guiado", s, ItemPurpose.Guided, CodeMode.Program,
            "Completa el programa para que la salida sea:\nCargando...listo\nFin",
            """
            Console.Write("Cargando...");
            // Completa: «listo» debe quedar en la misma línea y «Fin» en la siguiente
            """,
            """
            Console.Write("Cargando...");
            Console.WriteLine("listo");
            Console.WriteLine("Fin");
            """,
            [In("Salida", "", "Cargando...listo\nFin")],
            "Write deja el cursor en la misma línea; WriteLine agrega el salto después del texto.",
            ["«listo» debe escribirse con WriteLine para que después haya salto.", "Agrega Console.WriteLine(\"Fin\"); al final."]);

        r.Code("n0.salida.indep", s, ItemPurpose.Independent, CodeMode.Program,
            "Escribe un programa que muestre exactamente estas dos líneas (incluidas las comillas):\nEl archivo se llama \"datos.txt\"\nRuta: C:\\temp",
            "",
            """
            Console.WriteLine("El archivo se llama \"datos.txt\"");
            Console.WriteLine("Ruta: C:\\temp");
            """,
            [In("Salida", "", "El archivo se llama \"datos.txt\"\nRuta: C:\\temp")],
            "\\\" produce una comilla y \\\\ produce una barra invertida.",
            ["Para mostrar una comilla doble usa \\\".", "Para mostrar una barra invertida escribe dos: \\\\."],
            difficulty: 2, targets: [ErrorCategory.Syntax, ErrorCategory.OutputFormat]);

        r.Code("n0.salida.app", s, ItemPurpose.Transfer, CodeMode.Program,
            "Aplicación: muestra un ticket con una sola instrucción Console.WriteLine, usando \\n para separar líneas:\nTICKET\nCafé: 35\nPan: 18",
            "",
            """
            Console.WriteLine("TICKET\nCafé: 35\nPan: 18");
            """,
            [In("Ticket", "", "TICKET\nCafé: 35\nPan: 18")],
            "Una sola cadena puede contener varios saltos de línea con \\n.",
            ["Escribe todo en una sola cadena.", "Separa cada renglón con \\n."],
            difficulty: 2);

        return new Skill(s, UnitId, "Salida en consola: Write, WriteLine y secuencias de escape",
            "Mostrar información con formato preciso: líneas, saltos y caracteres especiales.",
            Tier.Essential, ["n0.sintaxis"],
            new Lesson(s,
                "Producir una salida exacta en la consola combinando Write, WriteLine y secuencias de escape.",
                [
                    P("Console.WriteLine escribe un valor y después un salto de línea. Console.Write escribe el valor y deja el cursor en la misma línea."),
                    Code("""
                        Console.Write("Hola, ");
                        Console.WriteLine("mundo");
                        Console.WriteLine();
                        Console.WriteLine("Siguiente bloque");
                        """),
                    P("Algunos caracteres no se pueden escribir directamente dentro de una cadena. Para eso existen las secuencias de escape, que empiezan con barra invertida: \\n (salto de línea), \\t (tabulador), \\\" (comilla doble) y \\\\ (barra invertida)."),
                    Code("Console.WriteLine(\"Nombre:\\tAda\\nEdad:\\t36\");"),
                    Tip("En los ejercicios la salida se compara línea por línea. Un espacio o una mayúscula de más hacen que la salida sea distinta: precisión ante todo."),
                ],
                new WorkedExample("Combinar Write y WriteLine",
                    """
                    Console.Write("Total: ");
                    Console.WriteLine(120);
                    Console.WriteLine("Gracias por su compra\n¡Vuelva pronto!");
                    """,
                    [
                        Line(1, "Write escribe «Total: » y el cursor se queda en la misma línea."),
                        Line(2, "WriteLine continúa en esa línea con 120 y luego salta."),
                        Line(3, "\\n dentro de la cadena divide el texto en dos líneas."),
                    ],
                    "Total: 120\nGracias por su compra\n¡Vuelva pronto!"),
                new CommonMistake(
                    "Usar comillas dobles dentro de un texto sin escaparlas.",
                    "Console.WriteLine(\"Dijo \"sí\"\");",
                    "La segunda comilla cierra la cadena; lo que sigue ya no es texto válido y el compilador marca error de sintaxis.",
                    "Console.WriteLine(\"Dijo \\\"sí\\\"\");"),
                "n0.salida.pred1", "n0.salida.guiado", "n0.salida.indep", "n0.salida.app",
                ["n0.salida.recall1", "n0.salida.recall2", "n0.salida.concepto1"],
                "Reproduces sin ayuda una salida con varias líneas, comillas y barras invertidas exactamente como se pide.",
                12));
    }
}
