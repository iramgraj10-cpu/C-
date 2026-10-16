using CSharpMastery.Content.Infrastructure;
using CSharpMastery.Domain.Curriculum;
using CSharpMastery.Domain.Items;
using CSharpMastery.Domain.Progress;
using static CSharpMastery.Content.Infrastructure.ContentRegistry;
using static CSharpMastery.Content.Infrastructure.L;

namespace CSharpMastery.Content.Nivel0;

internal static class U02Datos
{
    public const string UnitId = "n0.u2";

    /// <summary>Encabezado común: las líneas de lectura se estudian a fondo en la unidad 3.</summary>
    internal const string InputNote = "// Las primeras líneas leen los datos de entrada (las estudiarás en la unidad 3).\n";

    public static Unit Build(ContentRegistry r) => new(UnitId, "n0", "Datos y operaciones",
        "Variables, constantes, tipos de datos, operadores aritméticos, relacionales y lógicos, y texto.",
        [Variables(r), Aritmetica(r), Relacionales(r), Texto(r)]);

    private static Skill Variables(ContentRegistry r)
    {
        const string s = "n0.variables";
        r.Choice("n0.variables.pred1", s, ItemPurpose.Prediction,
            "¿Qué muestra este programa?",
            "Una variable guarda un solo valor a la vez. Cada asignación reemplaza el valor anterior; al final puntos vale 3.",
            [
                Ok("3", "Correcto: la última asignación reemplaza los valores anteriores."),
                No("18", "Las asignaciones no se acumulan: cada una reemplaza el valor."),
                No("15", "Después de valer 15, la variable recibe 3."),
            ],
            code: """
                int puntos = 10;
                puntos = puntos + 5;
                puntos = 3;
                Console.WriteLine(puntos);
                """);

        r.Short("n0.variables.pred2", s, ItemPurpose.Prediction,
            "¿Qué número muestra este programa?",
            "b = a copia el valor 4 en b. Cambiar a después no afecta a b: los int se copian por valor.",
            ["4"], match: AnswerMatch.Number,
            code: """
                int a = 4;
                int b = a;
                a = 9;
                Console.WriteLine(b);
                """,
            wrong: new() { ["9"] = "b recibió una copia del valor de a en ese momento; no queda «enlazada» con a." },
            difficulty: 2);

        r.Choice("n0.variables.concepto1", s, ItemPurpose.Concept,
            "¿Qué tipo es más apropiado para guardar el precio de un producto con centavos en una aplicación de ventas?",
            "decimal representa exactamente valores en base 10 como 19.99, por eso es el tipo recomendado para dinero. double es binario y puede introducir pequeños errores de redondeo.",
            [
                Ok("decimal", "Correcto: precisión exacta en base 10, ideal para dinero."),
                No("int", "int no admite decimales."),
                No("double", "double funciona para cálculos científicos, pero puede tener errores de redondeo con dinero."),
                No("string", "Como texto no podrías hacer operaciones aritméticas directamente."),
            ],
            dimension: KnowledgeDimension.Design);

        r.Short("n0.variables.recall1", s, ItemPurpose.Recall,
            "¿Qué palabra clave se usa para declarar un valor que nunca cambia, como const double Pi = 3.1416;?",
            "const declara una constante: su valor se fija al compilar y no se puede reasignar.",
            ["const"]);

        r.Choice("n0.variables.recall2", s, ItemPurpose.Recall,
            "¿Cuál de estas declaraciones es válida?",
            "El valor debe ser compatible con el tipo: bool solo admite true o false; char va entre comillas simples; string entre dobles.",
            [
                Ok("char inicial = 'A';", "Correcto: un char es un solo carácter entre comillas simples."),
                No("int edad = \"20\";", "\"20\" es texto (string), no un número.", ErrorCategory.TypeMismatch),
                No("bool activo = \"true\";", "\"true\" entre comillas es un string; el bool es true sin comillas.", ErrorCategory.TypeMismatch),
                No("string nombre = 'Ana';", "Las comillas simples son para un solo carácter (char).", ErrorCategory.TypeMismatch),
            ]);

        r.Choice("n0.variables.bug1", s, ItemPurpose.BugFinding,
            "¿Por qué no compila este programa?",
            "Una constante no puede cambiar después de declararse. El compilador marca error en la asignación de la línea 2.",
            [
                Ok("Se intenta modificar una constante.", "Correcto: const significa que el valor es fijo."),
                No("0.16 no es un double válido.", "0.16 es un literal double válido."),
                No("Las constantes deben escribirse en mayúsculas.", "Es una convención (en C# se usa PascalCase), no una regla del compilador."),
            ],
            code: """
                const double Iva = 0.16;
                Iva = 0.08;
                Console.WriteLine(Iva);
                """,
            targets: [ErrorCategory.AccessModifier]);

        r.Code("n0.variables.guiado", s, ItemPurpose.Guided, CodeMode.Program,
            "Declara una variable int llamada edad con el valor 17 para que el programa muestre «Luis» y «17».",
            """
            string nombre = "Luis";
            // Declara aquí la variable edad
            Console.WriteLine(nombre);
            Console.WriteLine(edad);
            """,
            """
            string nombre = "Luis";
            int edad = 17;
            Console.WriteLine(nombre);
            Console.WriteLine(edad);
            """,
            [In("Salida", "", "Luis\n17")],
            "Una variable se declara con su tipo, su nombre y, opcionalmente, un valor inicial: int edad = 17;",
            ["Sigue el patrón de la línea 1, pero con el tipo int.", "La declaración debe ir antes de usarse en WriteLine."],
            targets: [ErrorCategory.UndeclaredIdentifier]);

        r.Code("n0.variables.indep", s, ItemPurpose.Independent, CodeMode.Program,
            "El programa lee dos enteros a y b. Intercambia sus valores usando una variable auxiliar y muestra a y luego b, cada uno en su línea.\nEjemplo: con 5 y 12 debe mostrar 12 y 5.",
            InputNote + """
            int a = int.Parse(Console.ReadLine()!);
            int b = int.Parse(Console.ReadLine()!);
            // Intercambia los valores de a y b aquí

            Console.WriteLine(a);
            Console.WriteLine(b);
            """,
            """
            int a = int.Parse(Console.ReadLine()!);
            int b = int.Parse(Console.ReadLine()!);
            int auxiliar = a;
            a = b;
            b = auxiliar;
            Console.WriteLine(a);
            Console.WriteLine(b);
            """,
            [
                In("Ejemplo", "5\n12", "12\n5"),
                In("Negativos", "-3\n7", "7\n-3", hidden: true),
                In("Iguales", "4\n4", "4\n4", edge: true, hidden: true),
            ],
            "Si haces a = b primero, el valor original de a se pierde. La variable auxiliar lo conserva.",
            ["¿Qué pasa con el valor original de a si escribes a = b; directamente?", "Guarda a en una variable auxiliar antes de sobrescribirla."],
            difficulty: 2, targets: [ErrorCategory.WrongLogic]);

        r.Code("n0.variables.app", s, ItemPurpose.Transfer, CodeMode.Program,
            "Aplicación: tres corredores intercambian posiciones en una rotación. Lee tres enteros a, b y c y rota los valores: a recibe el de b, b el de c y c el de a. Muestra a, b y c en líneas separadas.\nEjemplo: 1, 2, 3 → 2, 3, 1.",
            InputNote + """
            int a = int.Parse(Console.ReadLine()!);
            int b = int.Parse(Console.ReadLine()!);
            int c = int.Parse(Console.ReadLine()!);

            """,
            """
            int a = int.Parse(Console.ReadLine()!);
            int b = int.Parse(Console.ReadLine()!);
            int c = int.Parse(Console.ReadLine()!);
            int primero = a;
            a = b;
            b = c;
            c = primero;
            Console.WriteLine(a);
            Console.WriteLine(b);
            Console.WriteLine(c);
            """,
            [
                In("Ejemplo", "1\n2\n3", "2\n3\n1"),
                In("Otros valores", "10\n-5\n0", "-5\n0\n10", hidden: true),
            ],
            "Es la misma idea del intercambio: guarda el valor que se perdería antes de sobrescribirlo.",
            ["¿Qué valor se pierde primero si empiezas con a = b;?", "Basta una sola variable auxiliar para el valor original de a."],
            difficulty: 2);

        return new Skill(s, UnitId, "Variables, constantes y tipos de datos",
            "Declarar, asignar y elegir el tipo adecuado: int, double, decimal, bool, char y string.",
            Tier.Essential, ["n0.salida"],
            new Lesson(s,
                "Declarar variables y constantes con el tipo adecuado y predecir su valor después de varias asignaciones.",
                [
                    P("Una variable es un espacio con nombre donde el programa guarda un valor. En C# cada variable tiene un tipo que indica qué valores puede guardar y qué operaciones admite. El compilador verifica los tipos antes de ejecutar."),
                    Code("""
                        int edad = 20;            // números enteros
                        double estatura = 1.68;   // números con decimales (binarios)
                        decimal precio = 19.99m;  // decimales exactos: dinero (sufijo m)
                        bool activo = true;       // verdadero o falso
                        char inicial = 'A';       // un solo carácter, comillas simples
                        string nombre = "Ana";    // texto, comillas dobles
                        Console.WriteLine(nombre);
                        """),
                    P("Asignar con = reemplaza el valor anterior. La expresión del lado derecho se evalúa primero, por eso x = x + 1 tiene sentido: toma el valor actual, le suma uno y lo guarda."),
                    P("Con var el compilador deduce el tipo a partir del valor inicial: var total = 0; crea un int. El tipo sigue siendo fijo; var solo evita escribirlo."),
                    P("Una constante (const) es un valor que no cambia durante el programa, como una tasa de impuesto. Por convención, en C# las constantes se nombran en PascalCase: const decimal TasaIva = 0.16m;"),
                    Design("Usa nombres descriptivos en camelCase para variables locales (precioUnitario, totalVentas). Un buen nombre explica el propósito sin necesidad de comentarios."),
                ],
                new WorkedExample("Asignaciones sucesivas",
                    """
                    int saldo = 100;
                    saldo = saldo - 30;
                    saldo = saldo + 50;
                    const int Bono = 20;
                    saldo = saldo + Bono;
                    Console.WriteLine(saldo);
                    """,
                    [
                        Line(1, "Se declara saldo de tipo int con valor inicial 100."),
                        Line(2, "Se evalúa saldo - 30 (70) y el resultado se guarda en saldo."),
                        Line(3, "70 + 50 = 120."),
                        Line(4, "Bono es una constante: su valor 20 no puede cambiar."),
                        Line(5, "120 + 20 = 140."),
                    ],
                    "140"),
                new CommonMistake(
                    "Asignar un valor de un tipo incompatible.",
                    "int edad = \"20\";",
                    "\"20\" es un string. C# no convierte texto a número automáticamente: el compilador marca CS0029 (no se puede convertir string en int).",
                    "int edad = 20;"),
                "n0.variables.pred1", "n0.variables.guiado", "n0.variables.indep", "n0.variables.app",
                ["n0.variables.recall1", "n0.variables.recall2", "n0.variables.concepto1"],
                "Eliges el tipo adecuado para un dato, predices el valor de una variable tras varias asignaciones e intercambias valores sin perder información.",
                15));
    }

    private static Skill Aritmetica(ContentRegistry r)
    {
        const string s = "n0.aritmetica";
        r.Choice("n0.aritmetica.pred1", s, ItemPurpose.Prediction,
            "¿Qué muestra este programa?",
            "Entre enteros, / descarta la parte decimal (7 / 2 = 3) y % da el residuo (1). Si un operando es double, la división conserva los decimales (3.5).",
            [
                Ok("3\n1\n3.5", "Correcto."),
                No("3.5\n1\n3.5", "7 / 2 entre int es una división entera: el resultado es 3.", ErrorCategory.OperatorMisuse),
                No("3\n3.5\n3.5", "% es el residuo de la división, no la división.", ErrorCategory.OperatorMisuse),
            ],
            code: """
                Console.WriteLine(7 / 2);
                Console.WriteLine(7 % 2);
                Console.WriteLine(7.0 / 2);
                """,
            targets: [ErrorCategory.OperatorMisuse]);

        r.Short("n0.aritmetica.pred2", s, ItemPurpose.Prediction,
            "¿Qué número muestra este programa?",
            "La multiplicación tiene prioridad sobre la suma: 2 + (3 * 4) = 14. Usa paréntesis para cambiar el orden.",
            ["14"], match: AnswerMatch.Number,
            code: """
                int x = 2 + 3 * 4;
                Console.WriteLine(x);
                """,
            wrong: new() { ["20"] = "La multiplicación se evalúa antes que la suma, salvo que uses paréntesis." });

        r.Short("n0.aritmetica.recall1", s, ItemPurpose.Recall,
            "Sin calcular en papel: ¿cuánto vale 17 % 5?",
            "17 = 5 * 3 + 2, así que el residuo es 2.",
            ["2"], match: AnswerMatch.Number);

        r.Choice("n0.aritmetica.recall2", s, ItemPurpose.Recall,
            "¿A qué equivale total += 5; ?",
            "Los operadores compuestos (+=, -=, *=, /=) combinan la operación con la asignación.",
            [
                Ok("total = total + 5;", "Correcto."),
                No("total = 5;", "Eso reemplaza el valor, no lo incrementa."),
                No("total + 5;", "Una expresión suelta no modifica la variable (ni compila como instrucción)."),
            ]);

        r.Choice("n0.aritmetica.concepto1", s, ItemPurpose.Concept,
            "¿Para qué sirve el operador % en la práctica?",
            "% da el residuo. Sirve para saber si un número es par (n % 2 == 0), extraer el último dígito (n % 10) o repetir ciclos (horas % 24).",
            [
                Ok("Para obtener el residuo de una división entera, por ejemplo para saber si un número es par.", "Correcto."),
                No("Para calcular porcentajes.", "Aunque se lee «por ciento», en C# % es el residuo, no un porcentaje.", ErrorCategory.OperatorMisuse),
                No("Para dividir con decimales.", "La división es /. % devuelve el residuo.", ErrorCategory.OperatorMisuse),
            ]);

        r.Choice("n0.aritmetica.bug1", s, ItemPurpose.BugFinding,
            "Este programa debería mostrar 3.5, pero muestra 3. ¿Por qué?",
            "a + b es int (7) y 2 es int, así que la división es entera (3) ANTES de guardarse en el double. Solución: (a + b) / 2.0.",
            [
                Ok("La división se hace entre enteros y descarta los decimales antes de asignarse a double.", "Correcto: el tipo del resultado depende de los operandos, no de la variable destino."),
                No("double no puede guardar 3.5.", "double sí puede; el problema ocurre antes de la asignación.", ErrorCategory.TypeMismatch),
                No("Falta un paréntesis.", "Los paréntesis están bien; el problema son los tipos.", ErrorCategory.Syntax),
            ],
            code: """
                int a = 3;
                int b = 4;
                double promedio = (a + b) / 2;
                Console.WriteLine(promedio);
                """,
            targets: [ErrorCategory.OperatorMisuse, ErrorCategory.WrongLogic], difficulty: 2);

        r.Code("n0.aritmetica.guiado", s, ItemPurpose.Guided, CodeMode.Program,
            "El programa lee una cantidad de minutos. Completa el cálculo para mostrar horas y minutos con el formato «2 h 5 min» (para 125).",
            U02Datos.InputNote + """
            int total = int.Parse(Console.ReadLine()!);
            int horas = total / 60;
            int minutos = 0; // corrige este cálculo usando %
            Console.WriteLine(horas + " h " + minutos + " min");
            """,
            """
            int total = int.Parse(Console.ReadLine()!);
            int horas = total / 60;
            int minutos = total % 60;
            Console.WriteLine(horas + " h " + minutos + " min");
            """,
            [
                In("125 minutos", "125", "2 h 5 min"),
                In("Menos de una hora", "45", "0 h 45 min", edge: true),
                In("Exacto", "180", "3 h 0 min", hidden: true),
            ],
            "La división entera da las horas completas y el residuo da los minutos sobrantes.",
            ["Los minutos sobrantes son lo que queda después de quitar las horas completas.", "minutos = total % 60;"]);

        r.Code("n0.aritmetica.indep", s, ItemPurpose.Independent, CodeMode.Program,
            "Lee un número entero de tres cifras (100 a 999) y muestra la suma de sus dígitos. Ejemplo: 472 → 13. Usa / y %; no conviertas a texto.",
            U02Datos.InputNote + """
            int n = int.Parse(Console.ReadLine()!);

            """,
            """
            int n = int.Parse(Console.ReadLine()!);
            int centenas = n / 100;
            int decenas = n / 10 % 10;
            int unidades = n % 10;
            Console.WriteLine(centenas + decenas + unidades);
            """,
            [
                In("472", "472", "13"),
                In("Con cero en medio", "405", "9", hidden: true, edge: true),
                In("999", "999", "27", hidden: true),
                In("100", "100", "1", hidden: true, edge: true),
            ],
            "n % 10 da la última cifra; n / 10 quita la última cifra; n / 100 da la primera.",
            ["¿Qué operación te da la última cifra de un número?", "Para la cifra del medio: primero quita la última con / 10 y luego toma la última con % 10."],
            difficulty: 2, targets: [ErrorCategory.OperatorMisuse]);

        r.Code("n0.aritmetica.app", s, ItemPurpose.Transfer, CodeMode.Program,
            "Aplicación: un cajero entrega una cantidad entera de pesos usando la menor cantidad de billetes de 500, 200, 100 y monedas de 1 (sin otras denominaciones). Lee la cantidad y muestra cuántas piezas de cada una, en este formato:\n500: 1\n200: 1\n100: 0\n1: 37\n(ejemplo para 737)",
            U02Datos.InputNote + """
            int cantidad = int.Parse(Console.ReadLine()!);

            """,
            """
            int cantidad = int.Parse(Console.ReadLine()!);
            int de500 = cantidad / 500;
            cantidad %= 500;
            int de200 = cantidad / 200;
            cantidad %= 200;
            int de100 = cantidad / 100;
            cantidad %= 100;
            Console.WriteLine("500: " + de500);
            Console.WriteLine("200: " + de200);
            Console.WriteLine("100: " + de100);
            Console.WriteLine("1: " + cantidad);
            """,
            [
                In("737", "737", "500: 1\n200: 1\n100: 0\n1: 37"),
                In("1999", "1999", "500: 3\n200: 2\n100: 0\n1: 99", hidden: true),
                In("Cero", "0", "500: 0\n200: 0\n100: 0\n1: 0", hidden: true, edge: true),
            ],
            "Cada denominación usa la división entera para contar piezas y el residuo para lo que falta por entregar.",
            ["Empieza por la denominación más grande.", "Después de contar los de 500, actualiza la cantidad con cantidad %= 500;"],
            difficulty: 3, related: ["n0.variables"]);

        return new Skill(s, UnitId, "Operadores aritméticos",
            "Suma, resta, multiplicación, división entera y decimal, residuo, precedencia y asignación compuesta.",
            Tier.Essential, ["n0.variables"],
            new Lesson(s,
                "Calcular con +, -, *, / y %, predecir el resultado de divisiones entre enteros y respetar la precedencia de operadores.",
                [
                    P("C# tiene los operadores aritméticos +, -, *, / y %. El tipo de los operandos decide el tipo del resultado: si ambos son int, el resultado es int."),
                    P("Por eso 7 / 2 vale 3: la división entre enteros descarta la parte decimal (no redondea). Si al menos uno es double (7.0 / 2 o 7 / 2.0), el resultado es 3.5."),
                    Code("""
                        Console.WriteLine(7 / 2);    // 3
                        Console.WriteLine(7 % 2);    // 1 (residuo)
                        Console.WriteLine(7 / 2.0);  // 3.5
                        Console.WriteLine(-7 / 2);   // -3 (se trunca hacia cero)
                        """),
                    P("La precedencia sigue las reglas de matemáticas: *, / y % antes que + y -. Ante la duda, usa paréntesis: también hacen el código más claro."),
                    P("Las asignaciones compuestas abrevian operaciones frecuentes: x += 3 equivale a x = x + 3. Los operadores ++ y -- suman o restan 1."),
                    Warn("Dividir un int entre 0 lanza DivideByZeroException al ejecutar. Con double, el resultado es infinito o NaN: no hay excepción, pero el valor no tiene sentido."),
                ],
                new WorkedExample("Descomponer segundos",
                    """
                    int segundosTotales = 3725;
                    int horas = segundosTotales / 3600;
                    int resto = segundosTotales % 3600;
                    int minutos = resto / 60;
                    int segundos = resto % 60;
                    Console.WriteLine(horas + ":" + minutos + ":" + segundos);
                    """,
                    [
                        Line(2, "3725 / 3600 = 1 hora completa (división entera)."),
                        Line(3, "El residuo, 125 segundos, es lo que falta repartir."),
                        Line(4, "125 / 60 = 2 minutos completos."),
                        Line(5, "125 % 60 = 5 segundos."),
                        Line(6, "Se concatenan los valores con dos puntos."),
                    ],
                    "1:2:5"),
                new CommonMistake(
                    "Esperar decimales de una división entre enteros.",
                    "int a = 5, b = 2;\ndouble mitad = a / b; // 2, no 2.5",
                    "La división a / b se calcula entre int (resultado 2) y después se guarda en double como 2.0. El tipo de la variable destino no cambia cómo se calcula.",
                    "int a = 5, b = 2;\ndouble mitad = (double)a / b; // 2.5"),
                "n0.aritmetica.pred1", "n0.aritmetica.guiado", "n0.aritmetica.indep", "n0.aritmetica.app",
                ["n0.aritmetica.recall1", "n0.aritmetica.recall2", "n0.aritmetica.concepto1"],
                "Predices correctamente divisiones enteras y residuos, y resuelves sin ayuda problemas de descomposición con / y %.",
                15));
    }

    private static Skill Relacionales(ContentRegistry r)
    {
        const string s = "n0.relacionales";
        r.Choice("n0.relacionales.pred1", s, ItemPurpose.Prediction,
            "¿Qué muestra este programa?",
            "Las comparaciones producen bool. 17 >= 18 es False. 17 > 12 y 17 < 20 son verdaderas, así que && da True. Console muestra los bool como True/False.",
            [
                Ok("False\nTrue", "Correcto."),
                No("True\nTrue", "17 no es mayor o igual que 18."),
                No("False\nFalse", "&& es verdadero cuando ambas condiciones lo son, y aquí ambas lo son."),
            ],
            code: """
                int edad = 17;
                Console.WriteLine(edad >= 18);
                Console.WriteLine(edad > 12 && edad < 20);
                """);

        r.Short("n0.relacionales.pred2", s, ItemPurpose.Prediction,
            "¿Qué muestra este programa? (True o False)",
            "5 > 3 es True y ! lo invierte: False. Luego False || (2 == 2) da True, porque basta una condición verdadera.",
            ["True"],
            code: """
                bool resultado = !(5 > 3) || 2 == 2;
                Console.WriteLine(resultado);
                """,
            difficulty: 2);

        r.Choice("n0.relacionales.concepto1", s, ItemPurpose.Concept,
            "¿Cuál es la diferencia entre = y == ?",
            "= asigna un valor a una variable. == compara dos valores y produce true o false.",
            [
                Ok("= asigna; == compara y devuelve bool.", "Correcto."),
                No("Son equivalentes.", "Confundirlos es un error frecuente: uno guarda, el otro pregunta.", ErrorCategory.OperatorMisuse),
                No("== se usa solo con texto.", "== compara números, caracteres, bool, texto y más.", ErrorCategory.OperatorMisuse),
            ]);

        r.Short("n0.relacionales.recall1", s, ItemPurpose.Recall,
            "¿Qué operador significa «distinto de»?",
            "!= devuelve true cuando los valores son diferentes.",
            ["!="]);

        r.Choice("n0.relacionales.recall2", s, ItemPurpose.Recall,
            "¿Cuándo es verdadera la expresión a || b?",
            "|| (o lógico) es verdadero cuando al menos uno de los operandos es verdadero.",
            [
                Ok("Cuando al menos una de las dos es verdadera.", "Correcto."),
                No("Solo cuando ambas son verdaderas.", "Eso describe a && (y lógico)."),
                No("Cuando ambas son falsas.", "Si ambas son falsas, a || b es falso."),
            ]);

        r.Choice("n0.relacionales.bug1", s, ItemPurpose.BugFinding,
            "¿Por qué no compila la línea 2?",
            "numero % 2 = 0 intenta asignar 0 a una expresión. Para comparar se usa ==: numero % 2 == 0. El compilador marca CS0131.",
            [
                Ok("Usa = (asignación) en lugar de == (comparación).", "Correcto."),
                No("% no se puede usar con int.", "% funciona con enteros.", ErrorCategory.OperatorMisuse),
                No("Una variable bool no puede guardar comparaciones.", "Sí puede: una comparación produce un bool.", ErrorCategory.OperatorMisuse),
            ],
            code: """
                int numero = 8;
                bool esPar = numero % 2 = 0;
                Console.WriteLine(esPar);
                """,
            targets: [ErrorCategory.OperatorMisuse]);

        r.Code("n0.relacionales.guiado", s, ItemPurpose.Guided, CodeMode.Program,
            "Completa la expresión para que esAdulto sea True cuando la edad leída sea 18 o más.",
            U02Datos.InputNote + """
            int edad = int.Parse(Console.ReadLine()!);
            bool esAdulto = false; // reemplaza false por una comparación
            Console.WriteLine(esAdulto);
            """,
            """
            int edad = int.Parse(Console.ReadLine()!);
            bool esAdulto = edad >= 18;
            Console.WriteLine(esAdulto);
            """,
            [
                In("20 años", "20", "True"),
                In("15 años", "15", "False"),
                In("Exactamente 18", "18", "True", edge: true, hint: "¿Tu comparación incluye el 18?"),
            ],
            "Una comparación produce directamente un bool; no hace falta if.",
            ["¿Qué operador significa «mayor o igual»?", "bool esAdulto = edad >= 18;"],
            targets: [ErrorCategory.OffByOne]);

        r.Code("n0.relacionales.indep", s, ItemPurpose.Independent, CodeMode.Program,
            "Lee un entero. En la primera línea muestra si está entre 10 y 20, ambos incluidos (True/False). En la segunda, si es par.",
            U02Datos.InputNote + """
            int n = int.Parse(Console.ReadLine()!);

            """,
            """
            int n = int.Parse(Console.ReadLine()!);
            Console.WriteLine(n >= 10 && n <= 20);
            Console.WriteLine(n % 2 == 0);
            """,
            [
                In("15", "15", "True\nFalse"),
                In("10", "10", "True\nTrue", edge: true),
                In("21", "21", "False\nFalse", hidden: true, edge: true),
                In("-4", "-4", "False\nTrue", hidden: true),
            ],
            "Un rango se expresa con dos comparaciones unidas por &&.",
            ["«Entre 10 y 20 incluidos» son dos condiciones que deben cumplirse a la vez.", "Un número es par si el residuo de dividirlo entre 2 es 0."],
            targets: [ErrorCategory.OffByOne, ErrorCategory.OperatorMisuse], related: ["n0.aritmetica"]);

        r.Code("n0.relacionales.app", s, ItemPurpose.Transfer, CodeMode.Program,
            "Aplicación: un año es bisiesto si es divisible entre 4, excepto los divisibles entre 100, salvo que también lo sean entre 400. Lee un año y muestra True si es bisiesto o False si no.",
            U02Datos.InputNote + """
            int anio = int.Parse(Console.ReadLine()!);

            """,
            """
            int anio = int.Parse(Console.ReadLine()!);
            bool bisiesto = (anio % 4 == 0 && anio % 100 != 0) || anio % 400 == 0;
            Console.WriteLine(bisiesto);
            """,
            [
                In("2024", "2024", "True"),
                In("2023", "2023", "False"),
                In("1900", "1900", "False", hidden: true, edge: true, hint: "1900 es divisible entre 100 pero no entre 400."),
                In("2000", "2000", "True", hidden: true, edge: true, hint: "2000 es divisible entre 400."),
            ],
            "Traduce la regla a condiciones: (divisible entre 4 y no entre 100) o divisible entre 400.",
            ["Separa la regla en partes y escribe cada una como una comparación con %.", "Usa paréntesis para agrupar la parte «4 y no 100»."],
            difficulty: 3, related: ["n0.aritmetica"]);

        return new Skill(s, UnitId, "Operadores relacionales y lógicos",
            "Comparar valores (==, !=, <, >, <=, >=) y combinar condiciones con &&, || y !.",
            Tier.Essential, ["n0.aritmetica"],
            new Lesson(s,
                "Escribir expresiones booleanas que comparen valores y combinen condiciones, y predecir si son verdaderas o falsas.",
                [
                    P("Los operadores relacionales comparan dos valores y producen un bool: == (igual), != (distinto), <, >, <= y >=."),
                    P("Los operadores lógicos combinan valores bool: && (y) es verdadero solo si ambos lo son; || (o) es verdadero si al menos uno lo es; ! (no) invierte el valor."),
                    Code("""
                        int temperatura = 28;
                        bool calor = temperatura > 25;
                        bool templado = temperatura >= 15 && temperatura <= 25;
                        Console.WriteLine(calor);     // True
                        Console.WriteLine(templado);  // False
                        Console.WriteLine(!calor);    // False
                        """),
                    P("Para expresar un rango necesitas dos comparaciones: 15 <= t <= 25 no es válido en C#; se escribe t >= 15 && t <= 25."),
                    Tip("Los valores bool se muestran como True y False al imprimirse con Console.WriteLine, aunque en el código se escriben true y false."),
                ],
                new WorkedExample("Condiciones de acceso",
                    """
                    int edad = 16;
                    bool conPermiso = true;
                    bool puedeEntrar = edad >= 18 || conPermiso;
                    bool descuento = edad < 18 && !conPermiso;
                    Console.WriteLine(puedeEntrar);
                    Console.WriteLine(descuento);
                    """,
                    [
                        Line(3, "edad >= 18 es False, pero conPermiso es True; con || basta uno: True."),
                        Line(4, "edad < 18 es True, !conPermiso es False; con && se necesitan ambos: False."),
                    ],
                    "True\nFalse"),
                new CommonMistake(
                    "Escribir rangos como en matemáticas.",
                    "int n = 15;\nbool enRango = 10 <= n <= 20;",
                    "10 <= n produce un bool, y no se puede comparar un bool con 20 usando <=. El compilador marca CS0019.",
                    "int n = 15;\nbool enRango = n >= 10 && n <= 20;\nConsole.WriteLine(enRango);"),
                "n0.relacionales.pred1", "n0.relacionales.guiado", "n0.relacionales.indep", "n0.relacionales.app",
                ["n0.relacionales.recall1", "n0.relacionales.recall2", "n0.relacionales.concepto1"],
                "Traduces reglas en lenguaje natural a expresiones booleanas correctas, incluidos los casos límite de los rangos.",
                15));
    }

    private static Skill Texto(ContentRegistry r)
    {
        const string s = "n0.texto";
        r.Choice("n0.texto.pred1", s, ItemPurpose.Prediction,
            "¿Qué muestra este programa?",
            "Las expresiones se evalúan de izquierda a derecha. \"1\" + 2 es texto \"12\", y + 3 da \"123\". En cambio 1 + 2 es 3 (número) y luego + \"3\" da \"33\".",
            [
                Ok("123\n33", "Correcto: en cuanto aparece un string, + concatena."),
                No("6\n6", "Si uno de los operandos es string, + concatena en lugar de sumar."),
                No("123\n123", "1 + 2 se evalúa primero como suma de enteros: 3."),
                No("15\n33", "\"1\" + 2 concatena: \"12\"."),
            ],
            code: """
                Console.WriteLine("1" + 2 + 3);
                Console.WriteLine(1 + 2 + "3");
                """,
            difficulty: 2, targets: [ErrorCategory.TypeMismatch]);

        r.Short("n0.texto.pred2", s, ItemPurpose.Prediction,
            "Escribe exactamente lo que muestra este programa.",
            "En la cadena interpolada, las expresiones entre llaves se evalúan: {edad + 1} vale 31. Length cuenta caracteres: \"Ana\" tiene 3.",
            ["Ana tendrá 31 años\n3"],
            match: AnswerMatch.Output,
            code: """
                string nombre = "Ana";
                int edad = 30;
                Console.WriteLine($"{nombre} tendrá {edad + 1} años");
                Console.WriteLine(nombre.Length);
                """);

        r.Choice("n0.texto.concepto1", s, ItemPurpose.Concept,
            "¿Qué ventaja tiene la interpolación $\"...\" frente a concatenar con +?",
            "La interpolación muestra la forma final del texto y permite formato ({precio:F2}); con muchos + es fácil olvidar espacios.",
            [
                Ok("Es más legible y permite dar formato a los valores dentro del texto.", "Correcto."),
                No("Es la única forma de unir texto con números.", "También se puede con +, aunque es menos legible."),
                No("Hace que el programa no necesite compilarse.", "Todo el código C# se compila."),
            ],
            dimension: KnowledgeDimension.Design);

        r.Short("n0.texto.recall1", s, ItemPurpose.Recall,
            "¿Qué propiedad de un string indica cuántos caracteres tiene?",
            "Length devuelve la cantidad de caracteres: \"hola\".Length vale 4.",
            ["Length", ".Length"]);

        r.Choice("n0.texto.recall2", s, ItemPurpose.Recall,
            "¿Qué muestra Console.WriteLine($\"{3.14159:F2}\"); ?",
            "El formato F2 muestra el número con exactamente dos decimales, redondeando.",
            [
                Ok("3.14", "Correcto."),
                No("3.14159", "F2 limita a dos decimales."),
                No("F2", "Lo que va después de los dos puntos es el formato, no texto literal."),
            ]);

        r.Choice("n0.texto.bug1", s, ItemPurpose.BugFinding,
            "Este programa muestra «Total: {total}» en lugar de «Total: 50». ¿Qué falta?",
            "Sin el signo $ antes de las comillas, las llaves son texto normal. Con $\"Total: {total}\" se interpola el valor.",
            [
                Ok("El signo $ antes de las comillas.", "Correcto: $ activa la interpolación."),
                No("Convertir total a string.", "La interpolación convierte el valor automáticamente."),
                No("Usar corchetes en lugar de llaves.", "La interpolación usa llaves."),
            ],
            code: """
                int total = 50;
                Console.WriteLine("Total: {total}");
                """,
            targets: [ErrorCategory.OutputFormat]);

        r.Code("n0.texto.guiado", s, ItemPurpose.Guided, CodeMode.Program,
            "El programa lee un nombre y una edad. Completa la cadena interpolada para mostrar: «Hola, Ana. El próximo año tendrás 31 años.»",
            U02Datos.InputNote + """
            string nombre = Console.ReadLine()!;
            int edad = int.Parse(Console.ReadLine()!);
            Console.WriteLine($"Hola, {nombre}. El próximo año tendrás ... años.");
            """,
            """
            string nombre = Console.ReadLine()!;
            int edad = int.Parse(Console.ReadLine()!);
            Console.WriteLine($"Hola, {nombre}. El próximo año tendrás {edad + 1} años.");
            """,
            [
                In("Ana, 30", "Ana\n30", "Hola, Ana. El próximo año tendrás 31 años."),
                In("Luis, 9", "Luis\n9", "Hola, Luis. El próximo año tendrás 10 años.", hidden: true),
            ],
            "Dentro de las llaves puedes escribir cualquier expresión, como edad + 1.",
            ["Reemplaza los puntos suspensivos por una expresión entre llaves.", "{edad + 1}"]);

        r.Code("n0.texto.indep", s, ItemPurpose.Independent, CodeMode.Program,
            "Lee un nombre y un apellido (uno por línea). Muestra en la primera línea «Apellido, Nombre» y en la segunda «Letras: N», donde N es la suma de las longitudes de ambos.\nEjemplo: Ana / Pérez → «Pérez, Ana» y «Letras: 8».",
            U02Datos.InputNote + """
            string nombre = Console.ReadLine()!;
            string apellido = Console.ReadLine()!;

            """,
            """
            string nombre = Console.ReadLine()!;
            string apellido = Console.ReadLine()!;
            Console.WriteLine($"{apellido}, {nombre}");
            Console.WriteLine($"Letras: {nombre.Length + apellido.Length}");
            """,
            [
                In("Ana Pérez", "Ana\nPérez", "Pérez, Ana\nLetras: 8"),
                In("Otro", "Guadalupe\nLi", "Li, Guadalupe\nLetras: 11", hidden: true),
            ],
            "Length da la longitud de cada texto; puedes sumarlas dentro de la interpolación.",
            ["Usa una cadena interpolada para cada línea.", "nombre.Length + apellido.Length"],
            targets: [ErrorCategory.OutputFormat]);

        r.Code("n0.texto.app", s, ItemPurpose.Transfer, CodeMode.Program,
            "Aplicación: lee el nombre de un producto, su precio unitario (decimal) y la cantidad (int). Muestra:\nProducto: <nombre>\nTotal: <total con 2 decimales>\nEjemplo: Café, 35, 3 → «Producto: Café» y «Total: 105.00».",
            U02Datos.InputNote + """
            string producto = Console.ReadLine()!;
            decimal precio = decimal.Parse(Console.ReadLine()!);
            int cantidad = int.Parse(Console.ReadLine()!);

            """,
            """
            string producto = Console.ReadLine()!;
            decimal precio = decimal.Parse(Console.ReadLine()!);
            int cantidad = int.Parse(Console.ReadLine()!);
            decimal total = precio * cantidad;
            Console.WriteLine($"Producto: {producto}");
            Console.WriteLine($"Total: {total:F2}");
            """,
            [
                In("Café", "Café\n35\n3", "Producto: Café\nTotal: 105.00"),
                In("Con centavos", "Pan dulce\n12.5\n4", "Producto: Pan dulce\nTotal: 50.00", hidden: true),
                In("Cantidad cero", "Té\n20.75\n0", "Producto: Té\nTotal: 0.00", hidden: true, edge: true),
            ],
            "Calcula primero el total y luego dale formato con :F2 dentro de la interpolación.",
            ["decimal * int produce decimal.", "{total:F2} muestra dos decimales."],
            difficulty: 2, related: ["n0.aritmetica", "n0.variables"]);

        return new Skill(s, UnitId, "Texto: concatenación, interpolación y formato",
            "Unir texto con +, interpolar valores con $\"...\", dar formato numérico y conocer la longitud de un string.",
            Tier.Essential, ["n0.variables"],
            new Lesson(s,
                "Construir mensajes de texto exactos combinando valores con interpolación y formato numérico.",
                [
                    P("Un string es una secuencia de caracteres. Con + puedes unir (concatenar) textos, y si uno de los operandos es string, el otro se convierte a texto."),
                    P("La interpolación es más legible: se antepone $ a las comillas y se escriben expresiones entre llaves. Después de dos puntos puedes indicar un formato, como F2 para dos decimales."),
                    Code("""
                        string producto = "Libro";
                        decimal precio = 249.5m;
                        Console.WriteLine("Producto: " + producto);
                        Console.WriteLine($"{producto} cuesta {precio:F2}");
                        Console.WriteLine($"El nombre tiene {producto.Length} letras");
                        """),
                    P("La propiedad Length devuelve la cantidad de caracteres. Los métodos ToUpper() y ToLower() devuelven una copia en mayúsculas o minúsculas: los strings no se modifican, se crean nuevos."),
                    Warn("El orden importa con +: se evalúa de izquierda a derecha. 1 + 2 + \"3\" es \"33\", pero \"1\" + 2 + 3 es \"123\"."),
                ],
                new WorkedExample("Un mensaje con formato",
                    """
                    string cliente = "Sofía";
                    int articulos = 3;
                    decimal total = 457.8m;
                    Console.WriteLine($"Hola, {cliente.ToUpper()}");
                    Console.WriteLine($"Compraste {articulos} artículos por ${total:F2}");
                    """,
                    [
                        Line(4, "ToUpper() devuelve una copia en mayúsculas; cliente no cambia."),
                        Line(5, "El primer $ activa la interpolación; el segundo $ (antes de la llave) es texto literal. :F2 muestra 457.80."),
                    ],
                    "Hola, SOFÍA\nCompraste 3 artículos por $457.80"),
                new CommonMistake(
                    "Olvidar el $ de la interpolación.",
                    "int total = 50;\nConsole.WriteLine(\"Total: {total}\");",
                    "Sin $, las llaves son caracteres normales y se imprime literalmente «{total}». No es un error de compilación, sino de lógica: hay que leer la salida con atención.",
                    "int total = 50;\nConsole.WriteLine($\"Total: {total}\");"),
                "n0.texto.pred1", "n0.texto.guiado", "n0.texto.indep", "n0.texto.app",
                ["n0.texto.recall1", "n0.texto.recall2", "n0.texto.concepto1"],
                "Produces mensajes exactos con interpolación y formato, y predices el resultado de mezclar + con números y texto.",
                15));
    }
}
