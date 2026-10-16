using CSharpMastery.Content.Infrastructure;
using CSharpMastery.Domain.Curriculum;
using CSharpMastery.Domain.Items;
using CSharpMastery.Domain.Progress;
using static CSharpMastery.Content.Infrastructure.ContentRegistry;
using static CSharpMastery.Content.Infrastructure.L;

namespace CSharpMastery.Content.Nivel1;

internal static class U01Decisiones
{
    public const string UnitId = "n1.u1";

    public static Unit Build(ContentRegistry r) => new(UnitId, "n1", "Decisiones",
        "Booleanos, if/else, switch y evaluación en cortocircuito.",
        [Booleanos(r), IfElse(r), Switch(r)]);

    private static Skill Booleanos(ContentRegistry r)
    {
        const string s = "n1.booleanos";
        r.Choice("n1.booleanos.pred1", s, ItemPurpose.Prediction,
            "¿Qué muestra este programa?",
            "&& exige que ambas condiciones sean verdaderas: edad >= 18 es true, pero tieneBoleto es false, así que el resultado es False. Console.WriteLine muestra los bool como True o False.",
            [
                Ok("False", "Correcto: basta con que una condición de && sea falsa."),
                No("True", "Con && las dos partes deben ser verdaderas; tieneBoleto es false."),
                No("false", "El valor es falso, pero Console.WriteLine escribe los bool con mayúscula inicial: False.", ErrorCategory.OutputFormat),
            ],
            code: """
                int edad = 20;
                bool tieneBoleto = false;
                bool puedeEntrar = edad >= 18 && tieneBoleto;
                Console.WriteLine(puedeEntrar);
                """);

        r.Choice("n1.booleanos.pred2", s, ItemPurpose.Prediction,
            "¿Qué muestra este programa?",
            "Con ||, si la parte izquierda ya es true, la derecha no se evalúa (cortocircuito). Por eso 10 / divisor nunca se calcula y no hay división entre cero.",
            [
                Ok("True", "Correcto: el cortocircuito evita evaluar la división."),
                No("Se lanza DivideByZeroException", "La división está a la derecha de || y no se evalúa, porque la izquierda ya es true.", ErrorCategory.DivideByZero),
                No("False", "divisor == 0 es true, y true || cualquier cosa es true."),
            ],
            code: """
                int divisor = 0;
                bool resultado = divisor == 0 || 10 / divisor > 1;
                Console.WriteLine(resultado);
                """,
            difficulty: 2, targets: [ErrorCategory.DivideByZero]);

        r.Choice("n1.booleanos.concepto1", s, ItemPurpose.Concept,
            "¿Qué condición es verdadera exactamente cuando x está entre 1 y 10, incluidos ambos?",
            "Hay que comprobar los dos límites y unirlos con &&: x >= 1 && x <= 10. En C# no se puede escribir 1 <= x <= 10.",
            [
                Ok("x >= 1 && x <= 10", "Correcto: ambos límites deben cumplirse."),
                No("x >= 1 || x <= 10", "Con || cualquier número cumple al menos una de las dos condiciones."),
                No("1 <= x <= 10", "C# no encadena comparaciones: 1 <= x produce un bool, y un bool no se compara con 10.", ErrorCategory.TypeMismatch),
            ]);

        r.Short("n1.booleanos.recall1", s, ItemPurpose.Recall,
            "¿Qué operador lógico niega un bool (convierte true en false y viceversa)? Escribe solo el símbolo.",
            "! es la negación lógica: !true es false.",
            ["!"]);

        r.Choice("n1.booleanos.recall2", s, ItemPurpose.Recall,
            "¿Qué significa que && y || se evalúan «en cortocircuito»?",
            "Si el lado izquierdo ya determina el resultado, el derecho no se evalúa: false && … es false, true || … es true. Sirve para proteger operaciones como una división o el acceso a algo que podría ser null.",
            [
                Ok("Que el lado derecho no se evalúa si el izquierdo ya decide el resultado.", "Correcto."),
                No("Que siempre se evalúan ambos lados, de derecha a izquierda.", "Se evalúan de izquierda a derecha y el derecho puede omitirse."),
                No("Que son más rápidos que & y | porque usan menos memoria.", "La diferencia es que pueden omitir el lado derecho, no la memoria."),
            ],
            difficulty: 2);

        r.Choice("n1.booleanos.bug1", s, ItemPurpose.BugFinding,
            "Este programa debería indicar si el número es par, pero no compila. ¿Por qué?",
            "En C# = asigna y == compara. numero % 2 = 0 intenta asignar a una expresión, lo cual no es válido (CS0131).",
            [
                Ok("Usa = (asignación) donde debía usar == (comparación).", "Correcto."),
                No("% no funciona con int.", "% es el residuo de la división entera y funciona con int.", ErrorCategory.OperatorMisuse),
                No("bool no puede guardar el resultado de una comparación.", "Las comparaciones producen bool."),
            ],
            code: """
                int numero = 8;
                bool esPar = numero % 2 = 0;
                Console.WriteLine(esPar);
                """,
            targets: [ErrorCategory.OperatorMisuse]);

        r.Code("n1.booleanos.guiado", s, ItemPurpose.Guided, CodeMode.Program,
            "Completa la condición: un año es bisiesto si es divisible entre 4 y no entre 100, o si es divisible entre 400. El programa debe mostrar True o False.",
            """
            int anio = int.Parse(Console.ReadLine()!);
            bool bisiesto = false; // reemplaza false por la condición
            Console.WriteLine(bisiesto);
            """,
            """
            int anio = int.Parse(Console.ReadLine()!);
            bool bisiesto = (anio % 4 == 0 && anio % 100 != 0) || anio % 400 == 0;
            Console.WriteLine(bisiesto);
            """,
            [
                In("2024", "2024", "True"),
                In("2023", "2023", "False"),
                In("1900 (divisible entre 100)", "1900", "False", hidden: true, edge: true, hint: "Los años divisibles entre 100 no son bisiestos, salvo que también lo sean entre 400."),
                In("2000 (divisible entre 400)", "2000", "True", hidden: true, edge: true, hint: "Los divisibles entre 400 sí son bisiestos."),
            ],
            "Las reglas se traducen casi palabra por palabra: «y» es &&, «o» es ||, «no» es !=. Los paréntesis dejan clara la agrupación.",
            ["Divisible entre 4 se escribe anio % 4 == 0.", "Agrupa la primera regla entre paréntesis y únela con || a la segunda."],
            difficulty: 2, targets: [ErrorCategory.EdgeCaseMissed]);

        r.Code("n1.booleanos.indep", s, ItemPurpose.Independent, CodeMode.Program,
            "Lee la edad de una persona y si tiene credencial («si» o «no»). Muestra True si puede votar: debe tener 18 años o más y credencial.",
            """
            int edad = int.Parse(Console.ReadLine()!);
            string credencial = Console.ReadLine()!;
            """,
            """
            int edad = int.Parse(Console.ReadLine()!);
            string credencial = Console.ReadLine()!;
            Console.WriteLine(edad >= 18 && credencial == "si");
            """,
            [
                In("Adulto con credencial", "30\nsi", "True"),
                In("Menor", "16\nsi", "False"),
                In("Sin credencial", "40\nno", "False", hidden: true),
                In("Justo 18", "18\nsi", "True", hidden: true, edge: true, hint: "18 años ya cumple: usa >=, no >."),
            ],
            "Las dos condiciones deben cumplirse, así que se unen con &&. Para comparar textos se usa == entre strings.",
            ["Compara la edad con >= 18.", "Compara el texto con == \"si\" y une ambas condiciones con &&."],
            targets: [ErrorCategory.OffByOne]);

        r.Code("n1.booleanos.app", s, ItemPurpose.Transfer, CodeMode.Program,
            "Aplicación: una tienda da envío gratis si la compra es de 500 o más, o si el cliente es miembro («si») y la compra es de al menos 200. Lee el monto (entero) y si es miembro, y muestra True o False.",
            """
            int monto = int.Parse(Console.ReadLine()!);
            string miembro = Console.ReadLine()!;
            """,
            """
            int monto = int.Parse(Console.ReadLine()!);
            string miembro = Console.ReadLine()!;
            bool envioGratis = monto >= 500 || (miembro == "si" && monto >= 200);
            Console.WriteLine(envioGratis);
            """,
            [
                In("Compra grande", "650\nno", "True"),
                In("Miembro con 250", "250\nsi", "True"),
                In("No miembro con 250", "250\nno", "False", hidden: true),
                In("Miembro con 199", "199\nsi", "False", hidden: true, edge: true),
                In("Justo 500", "500\nno", "True", hidden: true, edge: true),
            ],
            "Se combinan && y || con paréntesis para expresar la regla de negocio sin ambigüedad.",
            ["Hay dos formas de obtener envío gratis: únelas con ||.", "La segunda forma tiene dos requisitos unidos con &&."],
            difficulty: 2, related: ["n0.relacionales"]);

        return new Skill(s, UnitId, "Booleanos, expresiones y cortocircuito",
            "Evaluación de condiciones compuestas y evaluación en cortocircuito de && y ||.",
            Tier.Essential, ["n0.relacionales"],
            new Lesson(s,
                "Construir condiciones compuestas con &&, || y !, y predecir su valor, incluido el efecto del cortocircuito.",
                [
                    P("Un bool solo puede valer true o false. Las comparaciones (==, !=, <, >, <=, >=) producen bool, y los operadores lógicos combinan bools: && (y), || (o) y ! (no)."),
                    Code("""
                        int temperatura = 28;
                        bool llueve = false;
                        bool salirAlParque = temperatura > 20 && !llueve;
                        Console.WriteLine(salirAlParque);
                        """),
                    P("Precedencia: ! se aplica primero, luego &&, y al final ||. Aun así, usa paréntesis cuando mezcles && y || para que la intención sea evidente."),
                    P("Cortocircuito: C# evalúa de izquierda a derecha y se detiene en cuanto conoce el resultado. En false && X nunca evalúa X; en true || X tampoco. Esto permite escribir condiciones «protegidas», como divisor != 0 && total / divisor > 3."),
                    Warn("Una variable bool ya es una condición. Escribe if (activo) en lugar de if (activo == true)."),
                ],
                new WorkedExample("Una regla de acceso",
                    """
                    int edad = 15;
                    bool conAdulto = true;
                    bool esFeriado = false;
                    bool acceso = edad >= 18 || (conAdulto && !esFeriado);
                    Console.WriteLine(acceso);
                    Console.WriteLine(edad >= 18);
                    """,
                    [
                        Line(4, "edad >= 18 es false, así que se evalúa el lado derecho de ||."),
                        Line(4, "!esFeriado es true; conAdulto && true es true; false || true es true."),
                        Line(6, "Console.WriteLine escribe True o False, con mayúscula."),
                    ],
                    "True\nFalse"),
                new CommonMistake(
                    "Escribir un rango como en matemáticas.",
                    "int x = 5;\nbool enRango = 1 <= x <= 10;",
                    "1 <= x produce un bool, y no se puede comparar un bool con 10 (CS0019). Cada comparación es independiente.",
                    "int x = 5;\nbool enRango = x >= 1 && x <= 10;\nConsole.WriteLine(enRango);"),
                "n1.booleanos.pred1", "n1.booleanos.guiado", "n1.booleanos.indep", "n1.booleanos.app",
                ["n1.booleanos.recall1", "n1.booleanos.recall2", "n1.booleanos.concepto1"],
                "Traduces una regla en español a una condición correcta, incluidos los casos límite, y predices cuándo el cortocircuito evita evaluar una parte.",
                12));
    }

    private static Skill IfElse(ContentRegistry r)
    {
        const string s = "n1.if-else";
        r.Choice("n1.if-else.pred1", s, ItemPurpose.Prediction,
            "¿Qué muestra este programa?",
            "Las condiciones de una cadena if / else if se revisan en orden y solo se ejecuta el primer bloque cuya condición sea verdadera. 85 >= 70 es true, así que se muestra «Aprobado» y ya no se revisa nada más.",
            [
                Ok("Aprobado", "Correcto: se ejecuta solo la primera rama verdadera."),
                No("Aprobado\nNotable", "En una cadena else if solo se ejecuta una rama.", ErrorCategory.WrongLogic),
                No("Notable", "Las condiciones se revisan en orden; la primera verdadera es nota >= 70.", ErrorCategory.WrongLogic),
            ],
            code: """
                int nota = 85;
                if (nota >= 70)
                {
                    Console.WriteLine("Aprobado");
                }
                else if (nota >= 80)
                {
                    Console.WriteLine("Notable");
                }
                else
                {
                    Console.WriteLine("Reprobado");
                }
                """,
            targets: [ErrorCategory.WrongLogic]);

        r.Short("n1.if-else.pred2", s, ItemPurpose.Prediction,
            "¿Qué número muestra este programa?",
            "Sin llaves, el if solo controla la siguiente instrucción. total += 5 se ejecuta siempre, sin importar la condición: 10 + 5 = 15.",
            ["15"], match: AnswerMatch.Number,
            code: """
                int total = 10;
                bool descuento = false;
                if (descuento)
                    total -= 3;
                    total += 5;
                Console.WriteLine(total);
                """,
            wrong: new() { ["10"] = "La sangría no define bloques en C#: sin llaves, solo la primera instrucción depende del if." },
            difficulty: 2, targets: [ErrorCategory.Scope]);

        r.Choice("n1.if-else.concepto1", s, ItemPurpose.Concept,
            "Quieres clasificar una calificación en «A» (90+), «B» (80–89) o «C» (menos de 80). ¿Qué orden de condiciones es correcto con if / else if?",
            "Con else if hay que ir del rango más restrictivo al más amplio: primero >= 90 y luego >= 80. Si se comprueba >= 80 primero, un 95 caería en «B».",
            [
                Ok("Primero nota >= 90, luego nota >= 80, y else para el resto.", "Correcto."),
                No("Primero nota >= 80, luego nota >= 90, y else para el resto.", "Un 95 cumpliría la primera condición y se clasificaría como B.", ErrorCategory.WrongLogic),
                No("El orden no importa con else if.", "Importa: se ejecuta la primera condición verdadera.", ErrorCategory.WrongLogic),
            ]);

        r.Choice("n1.if-else.recall1", s, ItemPurpose.Recall,
            "¿Cuándo se ejecuta el bloque else de una cadena if / else if / else?",
            "else se ejecuta solo si ninguna de las condiciones anteriores fue verdadera.",
            [
                Ok("Cuando ninguna de las condiciones anteriores fue verdadera.", "Correcto."),
                No("Siempre, después de las demás ramas.", "Solo se ejecuta una rama de la cadena."),
                No("Cuando la última condición fue falsa, aunque otra anterior fuera verdadera.", "Si alguna anterior fue verdadera, la cadena ya terminó."),
            ]);

        r.Choice("n1.if-else.recall2", s, ItemPurpose.Recall,
            "¿Por qué se recomienda usar siempre llaves { } en los bloques de un if?",
            "Sin llaves, solo la siguiente instrucción pertenece al if. Al agregar otra línea con sangría es fácil creer que también depende de la condición.",
            [
                Ok("Porque sin llaves solo la siguiente instrucción depende de la condición, y es fácil equivocarse al agregar líneas.", "Correcto."),
                No("Porque sin llaves el programa no compila.", "Compila, pero el comportamiento puede no ser el esperado."),
                No("Porque las llaves hacen que la condición se evalúe más rápido.", "No tienen efecto en la velocidad."),
            ]);

        r.Choice("n1.if-else.bug1", s, ItemPurpose.BugFinding,
            "Este programa debería mostrar «Negativo» solo para números negativos, pero con -5 muestra «Negativo» y también «Cero». ¿Cuál es el error?",
            "Son dos if independientes: el segundo se evalúa aunque el primero se haya cumplido, y su condición numero <= 0 también es verdadera para -5. Debería ser else if (numero == 0).",
            [
                Ok("El segundo if es independiente y su condición <= 0 también es verdadera; debería ser else if (numero == 0).", "Correcto."),
                No("Falta un else al final.", "El problema está en la segunda condición y en que no forma parte de una cadena.", ErrorCategory.WrongLogic),
                No("-5 no es menor que 0 en C#.", "Sí lo es; la primera condición funciona."),
            ],
            code: """
                int numero = -5;
                if (numero < 0)
                {
                    Console.WriteLine("Negativo");
                }
                if (numero <= 0)
                {
                    Console.WriteLine("Cero");
                }
                """,
            difficulty: 2, targets: [ErrorCategory.WrongLogic]);

        r.Code("n1.if-else.guiado", s, ItemPurpose.Guided, CodeMode.Program,
            "Completa el programa: lee una temperatura entera y muestra «Frío» si es menor que 15, «Templado» si está entre 15 y 25 (incluidos) y «Calor» si es mayor que 25.",
            """
            int temperatura = int.Parse(Console.ReadLine()!);
            if (temperatura < 15)
            {
                Console.WriteLine("Frío");
            }
            // agrega aquí las ramas que faltan
            """,
            """
            int temperatura = int.Parse(Console.ReadLine()!);
            if (temperatura < 15)
            {
                Console.WriteLine("Frío");
            }
            else if (temperatura <= 25)
            {
                Console.WriteLine("Templado");
            }
            else
            {
                Console.WriteLine("Calor");
            }
            """,
            [
                In("10 grados", "10", "Frío"),
                In("20 grados", "20", "Templado"),
                In("30 grados", "30", "Calor"),
                In("Justo 15", "15", "Templado", hidden: true, edge: true, hint: "15 ya es templado."),
                In("Justo 25", "25", "Templado", hidden: true, edge: true, hint: "25 todavía es templado: usa <= 25."),
            ],
            "Como la primera rama ya descartó los menores que 15, la segunda solo necesita revisar el límite superior.",
            ["Usa else if para la segunda rama y else para la última.", "La condición de «Templado» puede ser temperatura <= 25."],
            targets: [ErrorCategory.OffByOne]);

        r.Code("n1.if-else.indep", s, ItemPurpose.Independent, CodeMode.Program,
            "Lee tres enteros (uno por línea) y muestra el mayor. No uses Math.Max: resuélvelo con if.",
            """
            int a = int.Parse(Console.ReadLine()!);
            int b = int.Parse(Console.ReadLine()!);
            int c = int.Parse(Console.ReadLine()!);
            """,
            """
            int a = int.Parse(Console.ReadLine()!);
            int b = int.Parse(Console.ReadLine()!);
            int c = int.Parse(Console.ReadLine()!);
            int mayor = a;
            if (b > mayor)
            {
                mayor = b;
            }
            if (c > mayor)
            {
                mayor = c;
            }
            Console.WriteLine(mayor);
            """,
            [
                In("El primero", "9\n4\n1", "9"),
                In("El último", "3\n5\n8", "8"),
                In("El de en medio", "2\n7\n-1", "7", hidden: true),
                In("Todos negativos", "-4\n-9\n-2", "-2", hidden: true, edge: true),
                In("Empate", "6\n6\n3", "6", hidden: true, edge: true),
            ],
            "Una técnica sencilla: suponer que el primero es el mayor y corregir la suposición si aparece uno más grande.",
            ["Guarda en una variable el mayor encontrado hasta el momento.", "Compara b y c con esa variable, cada uno con su propio if."],
            difficulty: 2, requires: [CodeConstruct.Conditional]);

        r.Code("n1.if-else.app", s, ItemPurpose.Transfer, CodeMode.Program,
            "Aplicación: calcula la tarifa de un estacionamiento. Lee las horas (entero). La primera hora cuesta 20 y cada hora adicional 15, pero nunca se cobra más de 120 (tarifa máxima del día). Con 0 horas se cobra 0. Muestra solo el número.",
            """
            int horas = int.Parse(Console.ReadLine()!);
            """,
            """
            int horas = int.Parse(Console.ReadLine()!);
            int tarifa = 0;
            if (horas > 0)
            {
                tarifa = 20 + (horas - 1) * 15;
                if (tarifa > 120)
                {
                    tarifa = 120;
                }
            }
            Console.WriteLine(tarifa);
            """,
            [
                In("1 hora", "1", "20"),
                In("3 horas", "3", "50"),
                In("7 horas", "7", "110", hidden: true),
                In("8 horas (tope)", "8", "120", hidden: true, edge: true, hint: "Con 8 horas la fórmula da 125: aplica el tope de 120."),
                In("12 horas", "12", "120", hidden: true),
                In("0 horas", "0", "0", hidden: true, edge: true),
            ],
            "Primero se calcula con la fórmula y después se aplica el tope. Separar «calcular» y «limitar» hace la lógica más fácil de verificar.",
            ["Trata aparte el caso de 0 horas.", "Calcula 20 + (horas - 1) * 15 y, si pasa de 120, usa 120."],
            difficulty: 2, related: ["n0.aritmetica"]);

        return new Skill(s, UnitId, "if, else if y else",
            "Tomar decisiones y cubrir todos los casos.",
            Tier.Essential, ["n1.booleanos"],
            new Lesson(s,
                "Escribir cadenas if / else if / else que cubran todos los casos sin solaparse y predecir qué rama se ejecuta.",
                [
                    P("if ejecuta un bloque solo si su condición es verdadera. else ofrece el camino alternativo. Con else if se encadenan varias condiciones: se revisan en orden y se ejecuta únicamente la primera que sea verdadera."),
                    Code("""
                        int velocidad = 72;
                        if (velocidad > 100)
                        {
                            Console.WriteLine("Multa grave");
                        }
                        else if (velocidad > 60)
                        {
                            Console.WriteLine("Multa leve");
                        }
                        else
                        {
                            Console.WriteLine("Sin multa");
                        }
                        """),
                    P("Como las ramas se revisan en orden, cada condición puede asumir que las anteriores fueron falsas. En el ejemplo, la segunda rama no necesita comprobar velocidad <= 100."),
                    Tip("Antes de escribir el código, lista los casos y sus límites: ¿qué pasa exactamente en 60? ¿y en 100? La mayoría de los errores ocurren en los bordes."),
                    Warn("Sin llaves, el if solo controla la siguiente instrucción, aunque la sangría sugiera otra cosa."),
                ],
                new WorkedExample("Clasificar un número",
                    """
                    int[] valores = [-3, 0, 12];
                    foreach (int n in valores)
                    {
                        if (n < 0)
                        {
                            Console.WriteLine($"{n}: negativo");
                        }
                        else if (n == 0)
                        {
                            Console.WriteLine($"{n}: cero");
                        }
                        else
                        {
                            Console.WriteLine($"{n}: positivo");
                        }
                    }
                    """,
                    [
                        Line(1, "Probamos tres valores para ver las tres ramas (los arreglos y foreach se estudian después)."),
                        Line(4, "Primera condición: los negativos terminan aquí."),
                        Line(8, "Solo se llega aquí si n no es negativo; basta comprobar si es cero."),
                        Line(12, "else atrapa todo lo demás: los positivos."),
                    ],
                    "-3: negativo\n0: cero\n12: positivo"),
                new CommonMistake(
                    "Usar varios if independientes cuando los casos son excluyentes.",
                    "int nota = 95;\nif (nota >= 90) Console.WriteLine(\"A\");\nif (nota >= 80) Console.WriteLine(\"B\");",
                    "Cada if se evalúa por separado: un 95 cumple ambas condiciones y muestra A y B.",
                    "int nota = 95;\nif (nota >= 90)\n{\n    Console.WriteLine(\"A\");\n}\nelse if (nota >= 80)\n{\n    Console.WriteLine(\"B\");\n}"),
                "n1.if-else.pred1", "n1.if-else.guiado", "n1.if-else.indep", "n1.if-else.app",
                ["n1.if-else.recall1", "n1.if-else.recall2", "n1.if-else.concepto1"],
                "Escribes decisiones que cubren todos los casos, incluidos los valores límite, y explicas por qué importa el orden de las condiciones.",
                15));
    }

    private static Skill Switch(ContentRegistry r)
    {
        const string s = "n1.switch";
        r.Choice("n1.switch.pred1", s, ItemPurpose.Prediction,
            "¿Qué muestra este programa?",
            "La expresión switch compara el valor con cada patrón en orden; 6 coincide con el patrón 6 or 7 y devuelve «Fin de semana».",
            [
                Ok("Fin de semana", "Correcto."),
                No("Entre semana", "6 coincide con el patrón 6 or 7, que aparece antes que el descarte _.", ErrorCategory.WrongLogic),
                No("Día inválido", "El patrón _ solo se usa si ningún otro coincide."),
            ],
            code: """
                int dia = 6;
                string tipo = dia switch
                {
                    6 or 7 => "Fin de semana",
                    >= 1 and <= 5 => "Entre semana",
                    _ => "Día inválido",
                };
                Console.WriteLine(tipo);
                """);

        r.Short("n1.switch.pred2", s, ItemPurpose.Prediction,
            "¿Qué muestra este programa?",
            "\"B\" coincide con case \"B\" y con case \"C\" (están agrupados): se ejecuta su bloque, que muestra «Bien», y break termina el switch.",
            ["Bien"], match: AnswerMatch.Output,
            code: """
                string letra = "B";
                switch (letra)
                {
                    case "A":
                        Console.WriteLine("Excelente");
                        break;
                    case "B":
                    case "C":
                        Console.WriteLine("Bien");
                        break;
                    default:
                        Console.WriteLine("Revisa");
                        break;
                }
                """);

        r.Choice("n1.switch.concepto1", s, ItemPurpose.Concept,
            "¿Cuándo conviene más un switch que una cadena de if / else if?",
            "switch es más claro cuando se elige entre muchos valores discretos de una misma expresión (un código, un día, una opción de menú). Para rangos y condiciones variadas, if suele ser más natural, aunque la expresión switch también admite patrones relacionales.",
            [
                Ok("Cuando se compara una misma expresión contra muchos valores concretos.", "Correcto."),
                No("Siempre: switch es más rápido que if.", "La elección es por claridad, no por velocidad."),
                No("Cuando cada condición usa variables distintas.", "Para condiciones heterogéneas es más claro if / else if."),
            ]);

        r.Choice("n1.switch.recall1", s, ItemPurpose.Recall,
            "En una expresión switch, ¿qué hace el patrón _?",
            "_ es el descarte: coincide con cualquier valor. Se coloca al final como caso por defecto; si falta y ningún patrón coincide, se lanza una excepción en tiempo de ejecución.",
            [
                Ok("Coincide con cualquier valor; funciona como caso por defecto.", "Correcto."),
                No("Coincide solo con null.", "Para null se usa el patrón null."),
                No("Ignora el resultado del switch.", "El switch produce el valor del brazo que coincide."),
            ]);

        r.Short("n1.switch.recall2", s, ItemPurpose.Recall,
            "En una instrucción switch clásica, ¿qué palabra clave termina cada sección case para que no continúe con la siguiente?",
            "break. En C# no se permite «caer» de una sección con código a la siguiente: hay que terminarla con break (o return/throw).",
            ["break"]);

        r.Choice("n1.switch.bug1", s, ItemPurpose.BugFinding,
            "¿Por qué no compila este switch?",
            "En C# una sección case con instrucciones no puede continuar implícitamente a la siguiente (CS0163). Falta break al final de case 1.",
            [
                Ok("Falta break al final de la sección case 1.", "Correcto."),
                No("Los case deben escribirse en orden descendente.", "El orden de los case no importa para compilar."),
                No("switch no funciona con int.", "switch funciona con int, string, enums y más."),
            ],
            code: """
                int opcion = 1;
                switch (opcion)
                {
                    case 1:
                        Console.WriteLine("Nuevo");
                    case 2:
                        Console.WriteLine("Abrir");
                        break;
                }
                """,
            targets: [ErrorCategory.Syntax]);

        r.Code("n1.switch.guiado", s, ItemPurpose.Guided, CodeMode.Program,
            "Completa la expresión switch: lee el número de un mes (1–12) y muestra la estación en el hemisferio norte: 12, 1 y 2 → «Invierno»; 3–5 → «Primavera»; 6–8 → «Verano»; 9–11 → «Otoño»; cualquier otro → «Mes inválido».",
            """
            int mes = int.Parse(Console.ReadLine()!);
            string estacion = mes switch
            {
                12 or 1 or 2 => "Invierno",
                // completa los demás brazos
                _ => "Mes inválido",
            };
            Console.WriteLine(estacion);
            """,
            """
            int mes = int.Parse(Console.ReadLine()!);
            string estacion = mes switch
            {
                12 or 1 or 2 => "Invierno",
                >= 3 and <= 5 => "Primavera",
                >= 6 and <= 8 => "Verano",
                >= 9 and <= 11 => "Otoño",
                _ => "Mes inválido",
            };
            Console.WriteLine(estacion);
            """,
            [
                In("Enero", "1", "Invierno"),
                In("Abril", "4", "Primavera"),
                In("Julio", "7", "Verano", hidden: true),
                In("Noviembre", "11", "Otoño", hidden: true),
                In("Mes 13", "13", "Mes inválido", hidden: true, edge: true),
                In("Mes 0", "0", "Mes inválido", hidden: true, edge: true),
            ],
            "Los patrones relacionales (>= 3 and <= 5) permiten expresar rangos dentro de un switch.",
            ["Un brazo para un rango: >= 3 and <= 5 => \"Primavera\",", "Cada brazo termina en coma; el descarte _ va al final."],
            requires: [CodeConstruct.Switch]);

        r.Code("n1.switch.indep", s, ItemPurpose.Independent, CodeMode.Program,
            "Lee dos enteros y un operador (+, -, * o /), uno por línea, y muestra el resultado usando switch. Para / usa división entera. Si el operador no es válido muestra «Operador no válido».",
            """
            int a = int.Parse(Console.ReadLine()!);
            int b = int.Parse(Console.ReadLine()!);
            string operador = Console.ReadLine()!;
            """,
            """
            int a = int.Parse(Console.ReadLine()!);
            int b = int.Parse(Console.ReadLine()!);
            string operador = Console.ReadLine()!;
            string resultado = operador switch
            {
                "+" => (a + b).ToString(),
                "-" => (a - b).ToString(),
                "*" => (a * b).ToString(),
                "/" => (a / b).ToString(),
                _ => "Operador no válido",
            };
            Console.WriteLine(resultado);
            """,
            [
                In("Suma", "7\n5\n+", "12"),
                In("Multiplicación", "7\n5\n*", "35"),
                In("Resta", "3\n10\n-", "-7", hidden: true),
                In("División entera", "17\n5\n/", "3", hidden: true),
                In("Operador inválido", "1\n2\n%", "Operador no válido", hidden: true, edge: true),
            ],
            "Como cada brazo de una expresión switch debe producir el mismo tipo, aquí todos producen string.",
            ["Puedes usar una instrucción switch con case \"+\": … break; o una expresión switch.", "Si usas la expresión switch, convierte los números con .ToString() para que todos los brazos sean string."],
            difficulty: 2, requires: [CodeConstruct.Switch]);

        r.Code("n1.switch.app", s, ItemPurpose.Transfer, CodeMode.Program,
            "Aplicación: lee un código de producto (A, B o C, en mayúsculas o minúsculas) y una cantidad. Los precios son A = 12, B = 30, C = 7. Muestra el total. Si el código no existe, muestra «Producto desconocido».",
            """
            string codigo = Console.ReadLine()!;
            int cantidad = int.Parse(Console.ReadLine()!);
            """,
            """
            string codigo = Console.ReadLine()!;
            int cantidad = int.Parse(Console.ReadLine()!);
            int precio = codigo.ToUpper() switch
            {
                "A" => 12,
                "B" => 30,
                "C" => 7,
                _ => -1,
            };
            if (precio < 0)
            {
                Console.WriteLine("Producto desconocido");
            }
            else
            {
                Console.WriteLine(precio * cantidad);
            }
            """,
            [
                In("3 de A", "A\n3", "36"),
                In("2 de C", "C\n2", "14"),
                In("Minúscula", "b\n4", "120", hidden: true, edge: true, hint: "Normaliza el código con ToUpper() antes de compararlo."),
                In("Desconocido", "Z\n1", "Producto desconocido", hidden: true, edge: true),
            ],
            "Normalizar la entrada (ToUpper) antes de decidir evita duplicar casos.",
            ["Convierte el código a mayúsculas con ToUpper().", "Puedes usar un valor especial (como -1) para «no encontrado» y decidir después con if."],
            difficulty: 2, related: ["n0.texto", "n1.if-else"]);

        return new Skill(s, UnitId, "switch y expresiones switch",
            "Elegir entre muchos casos discretos.",
            Tier.Essential, ["n1.if-else"],
            new Lesson(s,
                "Elegir entre varios valores con switch (instrucción y expresión), incluidos patrones relacionales y un caso por defecto.",
                [
                    P("Cuando una misma expresión se compara contra muchos valores concretos, switch es más legible que una larga cadena de else if."),
                    P("La expresión switch (C# 8+) produce un valor. Cada brazo tiene la forma patrón => resultado, y _ captura cualquier otro valor:"),
                    Code("""
                        int codigo = 404;
                        string mensaje = codigo switch
                        {
                            200 => "OK",
                            404 => "No encontrado",
                            >= 500 and <= 599 => "Error del servidor",
                            _ => "Otro código",
                        };
                        Console.WriteLine(mensaje);
                        """),
                    P("La instrucción switch clásica ejecuta bloques. Cada sección case termina con break; varias etiquetas case pueden compartir el mismo bloque."),
                    Fragment("""
                        switch (opcion)
                        {
                            case "s":
                            case "S":
                                Guardar();
                                break;
                            default:
                                Console.WriteLine("Opción no reconocida");
                                break;
                        }
                        """),
                    Warn("Si una expresión switch no tiene _ y ningún patrón coincide, se lanza SwitchExpressionException al ejecutar. Incluye siempre un caso por defecto."),
                ],
                new WorkedExample("Calificación con letras",
                    """
                    int[] notas = [95, 83, 70, 40];
                    foreach (int nota in notas)
                    {
                        string letra = nota switch
                        {
                            >= 90 => "A",
                            >= 80 => "B",
                            >= 70 => "C",
                            _ => "F",
                        };
                        Console.WriteLine($"{nota} → {letra}");
                    }
                    """,
                    [
                        Line(4, "La expresión switch produce un string que se guarda en letra."),
                        Line(6, "Los brazos se evalúan en orden, igual que else if: el más restrictivo primero."),
                        Line(9, "_ atrapa todo lo que no coincidió antes."),
                    ],
                    "95 → A\n83 → B\n70 → C\n40 → F"),
                new CommonMistake(
                    "Olvidar break en una sección case de la instrucción switch.",
                    "switch (opcion)\n{\n    case 1:\n        Console.WriteLine(\"Uno\");\n    case 2:\n        Console.WriteLine(\"Dos\");\n        break;\n}",
                    "C# no permite pasar de una sección con instrucciones a la siguiente (CS0163). Cada sección debe terminar con break, return o throw.",
                    "int opcion = 1;\nswitch (opcion)\n{\n    case 1:\n        Console.WriteLine(\"Uno\");\n        break;\n    case 2:\n        Console.WriteLine(\"Dos\");\n        break;\n}"),
                "n1.switch.pred1", "n1.switch.guiado", "n1.switch.indep", "n1.switch.app",
                ["n1.switch.recall1", "n1.switch.recall2", "n1.switch.concepto1"],
                "Eliges entre switch e if según el problema y escribes switch con caso por defecto y patrones de rango sin errores.",
                14));
    }
}
