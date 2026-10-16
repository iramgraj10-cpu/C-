using CSharpMastery.Content.Infrastructure;
using CSharpMastery.Domain.Curriculum;
using CSharpMastery.Domain.Items;
using CSharpMastery.Domain.Progress;
using static CSharpMastery.Content.Infrastructure.ContentRegistry;
using static CSharpMastery.Content.Infrastructure.L;

namespace CSharpMastery.Content.Nivel0;

internal static class U03EntradaErrores
{
    public const string UnitId = "n0.u3";

    public static Unit Build(ContentRegistry r) => new(UnitId, "n0", "Entrada, conversión y errores",
        "Leer datos del usuario, convertir entre tipos y distinguir errores de compilación, de ejecución y de lógica.",
        [Entrada(r), Conversion(r), Errores(r)]);

    private static Skill Entrada(ContentRegistry r)
    {
        const string s = "n0.entrada";
        r.Choice("n0.entrada.pred1", s, ItemPurpose.Prediction,
            "Si el usuario escribe 5 y luego 3, ¿qué muestra este programa?",
            "ReadLine siempre devuelve texto. \"5\" + \"3\" concatena y produce \"53\". Para sumar hay que convertir a número.",
            [
                Ok("53", "Correcto: se concatenaron dos strings."),
                No("8", "Los valores leídos son texto, no números; + los concatena.", ErrorCategory.TypeMismatch),
                No("No compila", "Sumar dos strings con + es válido: los concatena."),
            ],
            code: """
                string a = "5"; // lo que devolvería Console.ReadLine()
                string b = "3";
                Console.WriteLine(a + b);
                """,
            targets: [ErrorCategory.TypeMismatch]);

        r.Choice("n0.entrada.concepto1", s, ItemPurpose.Concept,
            "¿Qué devuelve Console.ReadLine()?",
            "Devuelve la línea escrita como string, o null si ya no hay más entrada (por eso su tipo es string?). El valor nunca es un número aunque el usuario escriba dígitos.",
            [
                Ok("Un string con la línea escrita, o null si no hay más entrada.", "Correcto."),
                No("Un int si el usuario escribe un número.", "ReadLine no interpreta el contenido: siempre es texto.", ErrorCategory.TypeMismatch),
                No("Un char con la primera tecla.", "Eso se parece a Console.Read, no a ReadLine."),
            ]);

        r.Short("n0.entrada.recall1", s, ItemPurpose.Recall,
            "¿Qué tipo de dato devuelve Console.ReadLine()? (escribe el nombre del tipo)",
            "string (técnicamente string?, porque puede devolver null al terminar la entrada).",
            ["string", "string?"]);

        r.Choice("n0.entrada.recall2", s, ItemPurpose.Recall,
            "¿Qué significa el ! en Console.ReadLine()! ?",
            "Es el operador que perdona null: le dice al compilador «confío en que aquí no habrá null». No convierte ni valida nada; si llega null, el error ocurrirá después.",
            [
                Ok("Le indica al compilador que confías en que el valor no será null.", "Correcto: solo silencia la advertencia de nulabilidad."),
                No("Convierte el texto en número.", "Para convertir se usa int.Parse u otro método.", ErrorCategory.TypeMismatch),
                No("Niega el valor, como en !true.", "Detrás de una expresión, ! es el operador que perdona null, no la negación lógica."),
            ],
            difficulty: 2);

        r.Choice("n0.entrada.bug1", s, ItemPurpose.BugFinding,
            "¿Por qué no compila este programa?",
            "ReadLine devuelve string y no se puede guardar en int sin convertir (CS0029). Se necesita int.Parse(Console.ReadLine()!).",
            [
                Ok("Intenta guardar un string en una variable int sin convertirlo.", "Correcto."),
                No("ReadLine necesita un mensaje entre paréntesis.", "ReadLine no recibe argumentos."),
                No("Las variables int no pueden leerse del teclado.", "Pueden, pero primero hay que convertir el texto."),
            ],
            code: """
                int edad = Console.ReadLine();
                Console.WriteLine(edad);
                """,
            targets: [ErrorCategory.TypeMismatch]);

        r.Code("n0.entrada.guiado", s, ItemPurpose.Guided, CodeMode.Program,
            "Completa el programa: lee un nombre y muestra «Bienvenido, <nombre>!».",
            """
            Console.WriteLine("¿Cómo te llamas?");
            string nombre = ""; // reemplaza "" por la lectura de una línea
            Console.WriteLine($"Bienvenido, {nombre}!");
            """,
            """
            Console.WriteLine("¿Cómo te llamas?");
            string nombre = Console.ReadLine()!;
            Console.WriteLine($"Bienvenido, {nombre}!");
            """,
            [
                In("Ana", "Ana", "¿Cómo te llamas?\nBienvenido, Ana!"),
                In("Nombre compuesto", "María José", "¿Cómo te llamas?\nBienvenido, María José!", hidden: true),
            ],
            "Console.ReadLine() espera a que el usuario escriba una línea y la devuelve como texto.",
            ["Necesitas Console.ReadLine().", "string nombre = Console.ReadLine()!;"]);

        r.Code("n0.entrada.indep", s, ItemPurpose.Independent, CodeMode.Program,
            "Lee tres líneas: ciudad, país y año. Muestra «Ciudad, País (Año)» en una sola línea.\nEjemplo: Monterrey / México / 2026 → «Monterrey, México (2026)».",
            "",
            """
            string ciudad = Console.ReadLine()!;
            string pais = Console.ReadLine()!;
            string anio = Console.ReadLine()!;
            Console.WriteLine($"{ciudad}, {pais} ({anio})");
            """,
            [
                In("Monterrey", "Monterrey\nMéxico\n2026", "Monterrey, México (2026)"),
                In("Otra", "Oaxaca\nMéxico\n1532", "Oaxaca, México (1532)", hidden: true),
            ],
            "Cada llamada a ReadLine lee la siguiente línea, en orden.",
            ["Lee las tres líneas en variables string.", "El año no necesita convertirse: solo se muestra."]);

        r.Code("n0.entrada.app", s, ItemPurpose.Transfer, CodeMode.Program,
            "Aplicación: lee una palabra y muestra en una línea la palabra en mayúsculas seguida de su longitud entre paréntesis.\nEjemplo: hola → «HOLA (4)».",
            "",
            """
            string palabra = Console.ReadLine()!;
            Console.WriteLine($"{palabra.ToUpper()} ({palabra.Length})");
            """,
            [
                In("hola", "hola", "HOLA (4)"),
                In("Mixta", "CSharp", "CSHARP (6)", hidden: true),
                In("Una letra", "x", "X (1)", hidden: true, edge: true),
            ],
            "Combina lo que ya sabes de texto con la lectura de datos.",
            ["ToUpper() devuelve el texto en mayúsculas.", "Length da la cantidad de caracteres."],
            difficulty: 2, related: ["n0.texto"]);

        return new Skill(s, UnitId, "Entrada con Console.ReadLine",
            "Leer datos escritos por el usuario y entender que siempre llegan como texto.",
            Tier.Essential, ["n0.texto"],
            new Lesson(s,
                "Leer una o varias líneas de entrada y usarlas en la salida, sabiendo que llegan como string.",
                [
                    P("Console.ReadLine() detiene el programa hasta que el usuario escribe una línea y presiona Enter. Devuelve esa línea como string."),
                    P("Si ya no hay más entrada (por ejemplo, cuando los datos vienen de un archivo y se acabaron), ReadLine devuelve null. Por eso su tipo es string? (string que admite null). Al escribir Console.ReadLine()! le dices al compilador que asumes que habrá un valor."),
                    Fragment("""
                        Console.Write("Escribe tu ciudad: ");
                        string ciudad = Console.ReadLine()!;
                        Console.WriteLine($"Vives en {ciudad}");
                        """, "lectura desde el teclado"),
                    P("Cada llamada a ReadLine lee la siguiente línea. En los ejercicios, la entrada se proporciona automáticamente, una línea por dato."),
                    Warn("Aunque el usuario escriba 25, ReadLine devuelve el texto \"25\". Para hacer cálculos debes convertirlo; lo verás en la siguiente lección."),
                ],
                new WorkedExample("Leer y responder",
                    """
                    string nombre = "Ada";        // en un programa real: Console.ReadLine()!
                    string lenguaje = "C#";       // en un programa real: Console.ReadLine()!
                    Console.WriteLine($"{nombre} programa en {lenguaje}");
                    Console.WriteLine(nombre + lenguaje);
                    """,
                    [
                        Line(1, "Simulamos la entrada para poder ver la salida; con ReadLine el valor vendría del usuario."),
                        Line(3, "La interpolación combina ambos textos con espacios."),
                        Line(4, "+ entre strings concatena sin agregar espacios."),
                    ],
                    "Ada programa en C#\nAdaC#"),
                new CommonMistake(
                    "Tratar la entrada como número sin convertirla.",
                    "string a = Console.ReadLine()!;\nstring b = Console.ReadLine()!;\nConsole.WriteLine(a + b); // con 2 y 3 muestra 23",
                    "a y b son textos; + los concatena. No es un error de compilación, sino de lógica.",
                    "int a = int.Parse(Console.ReadLine()!);\nint b = int.Parse(Console.ReadLine()!);\nConsole.WriteLine(a + b); // 5"),
                "n0.entrada.pred1", "n0.entrada.guiado", "n0.entrada.indep", "n0.entrada.app",
                ["n0.entrada.recall1", "n0.entrada.recall2", "n0.entrada.concepto1"],
                "Lees varias líneas de entrada en el orden correcto y explicas por qué un número leído no se puede sumar sin convertirlo.",
                12));
    }

    private static Skill Conversion(ContentRegistry r)
    {
        const string s = "n0.conversion";
        r.Choice("n0.conversion.pred1", s, ItemPurpose.Prediction,
            "¿Qué muestra este programa?",
            "Una conversión explícita (cast) de double a int trunca: descarta la parte decimal sin redondear. 9.99 se convierte en 9.",
            [
                Ok("9", "Correcto: el cast trunca hacia cero."),
                No("10", "El cast no redondea; para redondear usa Math.Round.", ErrorCategory.TypeMismatch),
                No("9.99", "n es int: no puede guardar decimales.", ErrorCategory.TypeMismatch),
            ],
            code: """
                double d = 9.99;
                int n = (int)d;
                Console.WriteLine(n);
                """,
            targets: [ErrorCategory.TypeMismatch]);

        r.Choice("n0.conversion.pred2", s, ItemPurpose.Prediction,
            "¿Qué muestra este programa?",
            "int.TryParse no lanza excepción: devuelve false si el texto no es un número válido y deja el resultado en 0.",
            [
                Ok("False\n0", "Correcto."),
                No("True\n12", "\"12a\" no es un entero válido."),
                No("Se lanza FormatException", "Eso haría int.Parse. TryParse informa el fallo devolviendo false.", ErrorCategory.ParsingFormat),
            ],
            code: """
                bool ok = int.TryParse("12a", out int numero);
                Console.WriteLine(ok);
                Console.WriteLine(numero);
                """,
            difficulty: 2, targets: [ErrorCategory.ParsingFormat]);

        r.Short("n0.conversion.pred3", s, ItemPurpose.Prediction,
            "¿Qué número muestra este programa?",
            "a / 2 es una división entre int (3), y después ese 3 se convierte implícitamente a double: 3. El double se imprime sin decimales.",
            ["3"], match: AnswerMatch.Number,
            code: """
                int a = 7;
                double b = a / 2;
                Console.WriteLine(b);
                """,
            wrong: new() { ["3.5"] = "La división ocurre entre enteros antes de convertirse a double." },
            difficulty: 2, targets: [ErrorCategory.OperatorMisuse]);

        r.Choice("n0.conversion.concepto1", s, ItemPurpose.Concept,
            "¿Por qué int → double es implícito, pero double → int requiere un cast?",
            "Las conversiones implícitas solo se permiten cuando no se pierde información. De double a int se pierden los decimales, así que C# exige que lo pidas explícitamente.",
            [
                Ok("Porque de double a int se puede perder información y C# exige confirmarlo.", "Correcto."),
                No("Porque double es más lento.", "La velocidad no tiene que ver con la regla."),
                No("Es un error del lenguaje.", "Es una decisión de diseño para evitar pérdidas silenciosas."),
            ]);

        r.Short("n0.conversion.recall1", s, ItemPurpose.Recall,
            "¿Qué método convierte el texto \"42\" en el entero 42 y lanza una excepción si el texto no es válido? (escríbelo como Tipo.Método)",
            "int.Parse convierte texto a int y lanza FormatException si el formato es inválido. int.TryParse no lanza excepción.",
            ["int.Parse", "Int32.Parse", "int.Parse()"]);

        r.Choice("n0.conversion.recall2", s, ItemPurpose.Recall,
            "¿Cuál es la forma segura de convertir texto escrito por un usuario a int sin que el programa falle?",
            "TryParse devuelve true/false indicando si la conversión fue posible, sin lanzar excepción.",
            [
                Ok("int.TryParse(texto, out int valor)", "Correcto."),
                No("(int)texto", "No existe cast de string a int.", ErrorCategory.TypeMismatch),
                No("int.Parse(texto)", "Funciona, pero lanza FormatException si el texto es inválido.", ErrorCategory.ParsingFormat),
            ]);

        r.Choice("n0.conversion.bug1", s, ItemPurpose.BugFinding,
            "¿Por qué no compila?",
            "10.5 es double y no cabe en int sin perder información. El compilador exige una conversión explícita (CS0266) o un tipo adecuado.",
            [
                Ok("No se puede asignar un double a un int de forma implícita.", "Correcto."),
                No("Los números con punto deben escribirse con coma.", "En C# el separador decimal del código siempre es el punto.", ErrorCategory.Syntax),
                No("Falta el sufijo m.", "El sufijo m crea un decimal, que tampoco cabe en int.", ErrorCategory.TypeMismatch),
            ],
            code: """
                int total = 10.5;
                Console.WriteLine(total);
                """,
            targets: [ErrorCategory.TypeMismatch]);

        r.Code("n0.conversion.guiado", s, ItemPurpose.Guided, CodeMode.Program,
            "El programa lee dos números como texto. Conviértelos a int para que muestre su suma (con 5 y 3 debe mostrar 8, no 53).",
            """
            string textoA = Console.ReadLine()!;
            string textoB = Console.ReadLine()!;
            int a = 0; // convierte textoA
            int b = 0; // convierte textoB
            Console.WriteLine(a + b);
            """,
            """
            string textoA = Console.ReadLine()!;
            string textoB = Console.ReadLine()!;
            int a = int.Parse(textoA);
            int b = int.Parse(textoB);
            Console.WriteLine(a + b);
            """,
            [
                In("5 y 3", "5\n3", "8"),
                In("Negativos", "-10\n4", "-6", hidden: true),
            ],
            "int.Parse convierte un texto con dígitos en un número entero.",
            ["Usa int.Parse(textoA).", "Haz lo mismo con textoB."],
            targets: [ErrorCategory.TypeMismatch]);

        r.Code("n0.conversion.indep", s, ItemPurpose.Independent, CodeMode.Program,
            "Lee una temperatura en grados Celsius (puede tener decimales) y muéstrala en Fahrenheit con un decimal: F = C × 9 / 5 + 32.\nEjemplo: 25 → 77.0",
            "",
            """
            double celsius = double.Parse(Console.ReadLine()!);
            double fahrenheit = celsius * 9 / 5 + 32;
            Console.WriteLine($"{fahrenheit:F1}");
            """,
            [
                In("25", "25", "77.0"),
                In("Con decimales", "36.6", "97.9", hidden: true),
                In("-40", "-40", "-40.0", hidden: true, edge: true),
            ],
            "Usa double para admitir decimales y el formato F1 para mostrar un decimal.",
            ["Convierte la entrada con double.Parse.", "Para mostrar un decimal: {fahrenheit:F1}"],
            difficulty: 2, related: ["n0.aritmetica", "n0.texto"]);

        r.Code("n0.conversion.app", s, ItemPurpose.Transfer, CodeMode.Program,
            "Aplicación: lee el precio de un artículo (decimal) y un porcentaje de descuento (int). Muestra el precio final con dos decimales.\nEjemplo: 250 y 15 → 212.50",
            "",
            """
            decimal precio = decimal.Parse(Console.ReadLine()!);
            int descuento = int.Parse(Console.ReadLine()!);
            decimal final = precio - precio * descuento / 100;
            Console.WriteLine($"{final:F2}");
            """,
            [
                In("250 y 15", "250\n15", "212.50"),
                In("Sin descuento", "99.99\n0", "99.99", hidden: true, edge: true),
                In("Total", "80\n100", "0.00", hidden: true, edge: true),
                In("Centavos", "19.90\n10", "17.91", hidden: true),
            ],
            "Con decimal no hay errores de redondeo binario; el descuento es precio × porcentaje / 100.",
            ["Convierte cada línea con el tipo adecuado.", "Cuidado con el orden: multiplica antes de dividir para no perder precisión."],
            difficulty: 2, related: ["n0.aritmetica"]);

        return new Skill(s, UnitId, "Conversión de tipos",
            "Conversiones implícitas y explícitas, Parse, TryParse y el truncamiento al convertir a int.",
            Tier.Essential, ["n0.entrada", "n0.aritmetica"],
            new Lesson(s,
                "Convertir texto a números y entre tipos numéricos, anticipando cuándo se pierde información.",
                [
                    P("Una conversión implícita ocurre sola cuando no hay pérdida de información: un int cabe en un double, así que double d = 5; es válido."),
                    P("Una conversión explícita (cast) se escribe con el tipo entre paréntesis y la pides tú porque puede perder información: (int)9.99 vale 9. El cast trunca, no redondea."),
                    Code("""
                        int entero = 42;
                        double real = entero;       // implícita
                        double precio = 7.8;
                        int truncado = (int)precio;   // explícita: 7
                        Console.WriteLine(real);
                        Console.WriteLine(truncado);
                        """),
                    P("Para convertir texto en número se usa Parse (int.Parse, double.Parse, decimal.Parse). Si el texto no es válido, lanza FormatException al ejecutar."),
                    P("TryParse intenta convertir y devuelve true o false; el resultado queda en una variable out. Es la opción adecuada para datos escritos por personas."),
                    Code("""
                        bool valido = int.TryParse("123", out int numero);
                        Console.WriteLine(valido);  // True
                        Console.WriteLine(numero);  // 123
                        """),
                    Note("Parse usa la configuración regional del sistema para el separador decimal. En los ejercicios se usa el punto: 3.5."),
                ],
                new WorkedExample("Promedio a partir de texto",
                    """
                    string textoA = "7";
                    string textoB = "8";
                    int a = int.Parse(textoA);
                    int b = int.Parse(textoB);
                    double promedio = (a + b) / 2.0;
                    Console.WriteLine(promedio);
                    Console.WriteLine((int)promedio);
                    """,
                    [
                        Line(3, "int.Parse convierte el texto \"7\" en el número 7."),
                        Line(5, "Dividir entre 2.0 (double) evita la división entera: 7.5."),
                        Line(7, "El cast a int descarta los decimales: 7."),
                    ],
                    "7.5\n7"),
                new CommonMistake(
                    "Convertir texto con un cast.",
                    "string texto = \"10\";\nint n = (int)texto;",
                    "No existe conversión directa de string a int con cast (CS0030). El texto debe interpretarse con int.Parse o int.TryParse.",
                    "string texto = \"10\";\nint n = int.Parse(texto);"),
                "n0.conversion.pred1", "n0.conversion.guiado", "n0.conversion.indep", "n0.conversion.app",
                ["n0.conversion.recall1", "n0.conversion.recall2", "n0.conversion.concepto1"],
                "Eliges entre conversión implícita, cast, Parse y TryParse, y predices cuándo se truncan los decimales.",
                15));
    }

    private static Skill Errores(ContentRegistry r)
    {
        const string s = "n0.errores";
        r.Choice("n0.errores.pred1", s, ItemPurpose.Prediction,
            "¿Qué ocurre al ejecutar este programa?",
            "El programa compila (los tipos son correctos), pero al ejecutarse la división entera entre 0 lanza DivideByZeroException.",
            [
                Ok("Se lanza DivideByZeroException al ejecutar.", "Correcto: es un error de ejecución."),
                No("No compila.", "El compilador no sabe que b valdrá 0: los tipos son válidos."),
                No("Muestra 0.", "La división entera entre cero no está definida."),
                No("Muestra Infinito.", "Eso pasa con double, no con int.", ErrorCategory.DivideByZero),
            ],
            code: """
                int a = 10;
                int b = 0;
                Console.WriteLine(a / b);
                """,
            targets: [ErrorCategory.DivideByZero]);

        r.Choice("n0.errores.concepto1", s, ItemPurpose.Concept,
            "int.Parse(\"hola\") es un error de...",
            "El código compila (Parse recibe un string), pero falla al ejecutarse con FormatException. Es un error de ejecución.",
            [
                Ok("ejecución (FormatException).", "Correcto."),
                No("compilación.", "El compilador no evalúa el contenido del texto."),
                No("lógica, sin excepción.", "Sí se lanza una excepción que detiene el programa."),
            ],
            targets: [ErrorCategory.ParsingFormat]);

        r.Choice("n0.errores.concepto2", s, ItemPurpose.Concept,
            "Un programa compila y se ejecuta sin excepciones, pero calcula mal el total. ¿Qué tipo de error es?",
            "Un error de lógica: el programa hace exactamente lo que escribiste, pero no lo que querías. Se detecta con pruebas y comparando con resultados esperados.",
            [
                Ok("Un error de lógica.", "Correcto: solo las pruebas lo revelan."),
                No("Un error de compilación.", "Si compila, no es un error de compilación."),
                No("Un error de ejecución.", "No hubo excepción."),
            ]);

        r.Choice("n0.errores.bug1", s, ItemPurpose.BugFinding,
            "El compilador muestra: Solucion.cs(3,19): error CS0103: The name 'totl' does not exist in the current context. ¿Qué indica?",
            "(3,19) es línea 3, columna 19. CS0103 significa que el nombre no existe: 'totl' está mal escrito; debería ser 'total'.",
            [
                Ok("En la línea 3 se usa el nombre totl, que no está declarado (probablemente es total mal escrito).", "Correcto: lee primero la línea y el código del error."),
                No("Falta un punto y coma en la línea 19.", "19 es la columna, no la línea.", ErrorCategory.Syntax),
                No("El programa falló al ejecutarse.", "Es un error de compilación: el programa no llegó a ejecutarse."),
            ],
            code: """
                int total = 10;
                total = total * 2;
                Console.WriteLine(totl);
                """,
            targets: [ErrorCategory.UndeclaredIdentifier]);

        r.Short("n0.errores.recall1", s, ItemPurpose.Recall,
            "Si el compilador encuentra un error, ¿se ejecuta alguna parte del programa? (sí/no)",
            "No. Si hay errores de compilación no se genera el programa, así que nada se ejecuta.",
            ["no"]);

        r.Choice("n0.errores.recall2", s, ItemPurpose.Recall,
            "En el mensaje «Programa.cs(12,5): error CS1002: ; expected», ¿qué significan 12 y 5?",
            "El primer número es la línea y el segundo la columna donde el compilador detectó el problema.",
            [
                Ok("Línea 12, columna 5.", "Correcto."),
                No("12 errores en 5 archivos.", "Es la posición del error."),
                No("Columna 12, línea 5.", "El orden es (línea, columna)."),
            ]);

        r.Code("n0.errores.guiado", s, ItemPurpose.Guided, CodeMode.Program,
            "Este programa tiene tres errores de compilación. Corrígelos para que muestre 360 (el precio por la cantidad).",
            """
            int precio = 120
            int cantidad = 3;
            Console.WriteLine(precio * cantidd);
            """,
            """
            int precio = 120;
            int cantidad = 3;
            Console.WriteLine(precio * cantidad);
            """,
            [In("Total", "", "360")],
            "Lee cada mensaje de error: CS1002 indica un punto y coma faltante y CS0103 un nombre inexistente.",
            ["El primer error está al final de la línea 1.", "Revisa la ortografía del nombre de la variable en la línea 3."],
            targets: [ErrorCategory.Syntax, ErrorCategory.UndeclaredIdentifier]);

        r.Code("n0.errores.indep", s, ItemPurpose.Independent, CodeMode.Program,
            "Este programa compila, pero calcula mal el promedio de tres calificaciones (error de lógica). Encuentra el error y corrígelo. Debe mostrar el promedio con un decimal.\nEjemplo: 8, 9, 10 → 9.0",
            """
            double a = double.Parse(Console.ReadLine()!);
            double b = double.Parse(Console.ReadLine()!);
            double c = double.Parse(Console.ReadLine()!);
            double promedio = a + b + c / 3;
            Console.WriteLine($"{promedio:F1}");
            """,
            """
            double a = double.Parse(Console.ReadLine()!);
            double b = double.Parse(Console.ReadLine()!);
            double c = double.Parse(Console.ReadLine()!);
            double promedio = (a + b + c) / 3;
            Console.WriteLine($"{promedio:F1}");
            """,
            [
                In("8, 9, 10", "8\n9\n10", "9.0"),
                In("Decimales", "7.5\n6\n9", "7.5", hidden: true),
                In("Ceros", "0\n0\n0", "0.0", hidden: true, edge: true),
            ],
            "Por precedencia, c / 3 se calcula antes de las sumas. Hay que agrupar la suma con paréntesis.",
            ["Calcula a mano lo que hace el programa con 8, 9 y 10. ¿Por qué da 20.3?", "Agrupa la suma con paréntesis antes de dividir."],
            difficulty: 2, targets: [ErrorCategory.WrongLogic, ErrorCategory.OperatorMisuse], related: ["n0.aritmetica"]);

        r.Code("n0.errores.app", s, ItemPurpose.Transfer, CodeMode.Program,
            "Aplicación: este programa debería mostrar el porcentaje de alumnos aprobados con un decimal, pero siempre muestra 0.0. Corrígelo.\nEjemplo: 7 aprobados de 9 → 77.8",
            """
            int aprobados = int.Parse(Console.ReadLine()!);
            int total = int.Parse(Console.ReadLine()!);
            double porcentaje = aprobados / total * 100;
            Console.WriteLine($"{porcentaje:F1}");
            """,
            """
            int aprobados = int.Parse(Console.ReadLine()!);
            int total = int.Parse(Console.ReadLine()!);
            double porcentaje = aprobados * 100.0 / total;
            Console.WriteLine($"{porcentaje:F1}");
            """,
            [
                In("7 de 9", "7\n9", "77.8"),
                In("Todos", "30\n30", "100.0", hidden: true, edge: true),
                In("Ninguno", "0\n12", "0.0", hidden: true, edge: true),
                In("Uno de tres", "1\n3", "33.3", hidden: true),
            ],
            "aprobados / total es una división entre enteros: con 7 / 9 da 0. Multiplica por 100.0 antes de dividir.",
            ["Evalúa la expresión paso a paso con 7 y 9.", "Haz que la división ocurra con double: aprobados * 100.0 / total."],
            difficulty: 3, targets: [ErrorCategory.OperatorMisuse, ErrorCategory.WrongLogic], related: ["n0.conversion", "n0.aritmetica"]);

        return new Skill(s, UnitId, "Errores de compilación, de ejecución y de lógica",
            "Leer mensajes del compilador, reconocer excepciones y detectar errores de lógica con pruebas.",
            Tier.Essential, ["n0.conversion"],
            new Lesson(s,
                "Clasificar un error como de compilación, ejecución o lógica, y usar el mensaje (línea, columna y código) para corregirlo.",
                [
                    P("Hay tres familias de errores. Los de compilación impiden que el programa se genere: sintaxis inválida, nombres inexistentes o tipos incompatibles. El compilador indica archivo, línea, columna, un código (como CS1002) y una descripción."),
                    P("Los errores de ejecución ocurren mientras el programa corre: dividir un int entre cero, convertir un texto inválido o acceder a una posición inexistente. Se manifiestan como excepciones (DivideByZeroException, FormatException...) que detienen el programa si nadie las controla."),
                    P("Los errores de lógica son los más difíciles: el programa compila y no falla, pero el resultado es incorrecto. Se descubren comparando la salida con resultados esperados, es decir, probando."),
                    Fragment("""
                        Programa.cs(4,30): error CS1002: ; expected
                                 │ │        │
                                 │ │        └── código del error (búscalo si no lo conoces)
                                 │ └── columna
                                 └── línea
                        """, "anatomía de un mensaje"),
                    Tip("Corrige el primer error primero y vuelve a compilar: un solo error puede provocar varios mensajes en cascada."),
                ],
                new WorkedExample("Del síntoma a la causa",
                    """
                    int articulos = 5;
                    int cajas = 2;
                    // Queremos el promedio de artículos por caja: 2.5
                    double incorrecto = articulos / cajas;
                    double correcto = (double)articulos / cajas;
                    Console.WriteLine(incorrecto);
                    Console.WriteLine(correcto);
                    """,
                    [
                        Line(4, "Error de lógica: la división entera produce 2 antes de convertirse a double."),
                        Line(5, "Al convertir un operando a double, la división conserva los decimales."),
                        Line(6, "La salida 2 revela el error: compila y no lanza excepción, pero el resultado es incorrecto."),
                    ],
                    "2\n2.5"),
                new CommonMistake(
                    "Leer solo la palabra «error» e ignorar la línea y el código.",
                    "int edad = 30\nConsole.WriteLine(edad);",
                    "El mensaje CS1002 en la línea 1 dice exactamente qué falta (;) y dónde. Leer con atención la ubicación y el código ahorra mucho tiempo.",
                    "int edad = 30;\nConsole.WriteLine(edad);"),
                "n0.errores.pred1", "n0.errores.guiado", "n0.errores.indep", "n0.errores.app",
                ["n0.errores.recall1", "n0.errores.recall2", "n0.errores.concepto1"],
                "Clasificas errores correctamente y corriges sin ayuda errores de compilación y de lógica usando los mensajes y las pruebas.",
                15));
    }
}
