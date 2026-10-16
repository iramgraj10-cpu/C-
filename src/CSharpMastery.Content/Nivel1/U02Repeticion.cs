using CSharpMastery.Content.Infrastructure;
using CSharpMastery.Domain.Curriculum;
using CSharpMastery.Domain.Items;
using CSharpMastery.Domain.Progress;
using static CSharpMastery.Content.Infrastructure.ContentRegistry;
using static CSharpMastery.Content.Infrastructure.L;

namespace CSharpMastery.Content.Nivel1;

internal static class U02Repeticion
{
    public const string UnitId = "n1.u2";

    public static Unit Build(ContentRegistry r) => new(UnitId, "n1", "Repetición",
        "Bucles, contadores y acumuladores.",
        [While(r), For(r), BreakContinue(r), Acumuladores(r)]);

    private static Skill While(ContentRegistry r)
    {
        const string s = "n1.while";
        r.Short("n1.while.pred1", s, ItemPurpose.Prediction,
            "¿Qué muestra este programa? Escribe cada línea tal como aparecería.",
            "El bucle se repite mientras n > 0: muestra 3, 2 y 1. Cuando n llega a 0 la condición es falsa y el bucle termina; después se muestra «Despegue».",
            ["3\n2\n1\nDespegue"], match: AnswerMatch.Output,
            code: """
                int n = 3;
                while (n > 0)
                {
                    Console.WriteLine(n);
                    n--;
                }
                Console.WriteLine("Despegue");
                """,
            wrong: new() { ["3\n2\n1\n0\nDespegue"] = "Cuando n vale 0, la condición n > 0 es falsa y el cuerpo ya no se ejecuta." },
            targets: [ErrorCategory.OffByOne]);

        r.Choice("n1.while.pred2", s, ItemPurpose.Prediction,
            "¿Qué muestra este programa?",
            "do-while ejecuta el cuerpo primero y evalúa la condición después. Aunque la condición es falsa desde el principio, el cuerpo se ejecuta una vez.",
            [
                Ok("Hola", "Correcto: do-while siempre ejecuta al menos una vez."),
                No("Nada: la condición es falsa desde el inicio.", "Eso pasaría con while; do-while evalúa la condición al final.", ErrorCategory.ConceptMisunderstanding),
                No("«Hola» repetido sin fin.", "La condición es falsa, así que el bucle termina después de la primera vuelta.", ErrorCategory.InfiniteLoopOrTimeout),
            ],
            code: """
                int intentos = 10;
                do
                {
                    Console.WriteLine("Hola");
                    intentos++;
                } while (intentos < 5);
                """,
            difficulty: 2);

        r.Choice("n1.while.concepto1", s, ItemPurpose.Concept,
            "¿Qué tienen que cumplir los bucles while para no ser infinitos?",
            "Algo dentro del cuerpo debe cambiar las variables de la condición de forma que, tarde o temprano, la condición se vuelva falsa.",
            [
                Ok("El cuerpo debe modificar algo de lo que depende la condición, acercándola a ser falsa.", "Correcto."),
                No("Deben tener un break obligatorio.", "break es una opción, pero lo habitual es que la condición se vuelva falsa."),
                No("El compilador detiene automáticamente los bucles largos.", "El compilador no puede detectar en general si un bucle termina.", ErrorCategory.InfiniteLoopOrTimeout),
            ],
            targets: [ErrorCategory.InfiniteLoopOrTimeout]);

        r.Choice("n1.while.recall1", s, ItemPurpose.Recall,
            "¿En qué se diferencian while y do-while?",
            "while evalúa la condición antes de cada vuelta (puede ejecutarse cero veces). do-while la evalúa después (se ejecuta al menos una vez).",
            [
                Ok("while revisa la condición antes; do-while después, así que se ejecuta al menos una vez.", "Correcto."),
                No("do-while solo funciona con números.", "Ambos aceptan cualquier condición bool."),
                No("No hay diferencia, es cuestión de estilo.", "La diferencia está en cuándo se evalúa la condición."),
            ]);

        r.Short("n1.while.recall2", s, ItemPurpose.Recall,
            "Un while cuya condición nunca se vuelve falsa se llama bucle… (una palabra)",
            "Bucle infinito. En los ejercicios, el motor lo detiene por tiempo y lo reporta.",
            ["infinito"]);

        r.Choice("n1.while.bug1", s, ItemPurpose.BugFinding,
            "Este programa debería mostrar los números del 1 al 5, pero nunca termina. ¿Por qué?",
            "El cuerpo nunca cambia i, así que i <= 5 siempre es verdadera: es un bucle infinito. Falta i++ dentro del bucle.",
            [
                Ok("Falta incrementar i dentro del bucle.", "Correcto."),
                No("La condición debería ser i < 5.", "Eso cambiaría cuántos números se muestran, pero seguiría sin terminar.", ErrorCategory.OffByOne),
                No("Console.WriteLine no se puede usar dentro de un while.", "Sí se puede."),
            ],
            code: """
                int i = 1;
                while (i <= 5)
                {
                    Console.WriteLine(i);
                }
                """,
            targets: [ErrorCategory.InfiniteLoopOrTimeout]);

        r.Code("n1.while.guiado", s, ItemPurpose.Guided, CodeMode.Program,
            "Completa el bucle: lee un número n y muestra la cuenta regresiva desde n hasta 1, un número por línea, y al final «¡Listo!».",
            """
            int n = int.Parse(Console.ReadLine()!);
            while (false) // cambia la condición
            {
                Console.WriteLine(n);
                // actualiza n
            }
            Console.WriteLine("¡Listo!");
            """,
            """
            int n = int.Parse(Console.ReadLine()!);
            while (n >= 1)
            {
                Console.WriteLine(n);
                n--;
            }
            Console.WriteLine("¡Listo!");
            """,
            [
                In("Desde 3", "3", "3\n2\n1\n¡Listo!"),
                In("Desde 5", "5", "5\n4\n3\n2\n1\n¡Listo!", hidden: true),
                In("Desde 0", "0", "¡Listo!", hidden: true, edge: true, hint: "Si n es 0 el bucle no debe ejecutarse ni una vez."),
            ],
            "La condición y la actualización trabajan juntas: n >= 1 detiene el bucle y n-- lo acerca a ese punto.",
            ["La condición puede ser n >= 1.", "Dentro del bucle, disminuye n con n--."],
            requires: [CodeConstruct.Loop], targets: [ErrorCategory.InfiniteLoopOrTimeout]);

        r.Code("n1.while.indep", s, ItemPurpose.Independent, CodeMode.Program,
            "Lee un entero positivo y muestra cuántos dígitos tiene, usando un bucle (divide entre 10 hasta llegar a 0). Para 0 la respuesta es 1.",
            """
            int numero = int.Parse(Console.ReadLine()!);
            """,
            """
            int numero = int.Parse(Console.ReadLine()!);
            int digitos = 0;
            do
            {
                digitos++;
                numero /= 10;
            } while (numero > 0);
            Console.WriteLine(digitos);
            """,
            [
                In("Tres dígitos", "472", "3"),
                In("Un dígito", "7", "1"),
                In("Muchos dígitos", "1000000", "7", hidden: true),
                In("Cero", "0", "1", hidden: true, edge: true, hint: "0 tiene un dígito: un do-while garantiza al menos una vuelta."),
            ],
            "Cada división entera entre 10 elimina el último dígito. Contar cuántas divisiones caben equivale a contar dígitos. do-while maneja naturalmente el caso del 0.",
            ["numero /= 10 elimina el último dígito.", "Cuenta cuántas veces divides antes de llegar a 0; piensa qué pasa si el número ya es 0."],
            difficulty: 2, requires: [CodeConstruct.Loop]);

        r.Code("n1.while.app", s, ItemPurpose.Transfer, CodeMode.Program,
            "Aplicación: una cuenta de ahorro empieza con un saldo y gana 5 % de interés al año (se suma al saldo). Lee el saldo inicial y la meta (ambos enteros) y muestra cuántos años se necesitan para alcanzar o superar la meta. Usa decimal para el saldo.",
            """
            decimal saldo = decimal.Parse(Console.ReadLine()!);
            decimal meta = decimal.Parse(Console.ReadLine()!);
            """,
            """
            decimal saldo = decimal.Parse(Console.ReadLine()!);
            decimal meta = decimal.Parse(Console.ReadLine()!);
            int anios = 0;
            while (saldo < meta)
            {
                saldo += saldo * 0.05m;
                anios++;
            }
            Console.WriteLine(anios);
            """,
            [
                In("De 1000 a 1100", "1000\n1100", "2"),
                In("De 1000 a 2000", "1000\n2000", "15"),
                In("Ya alcanzada", "500\n500", "0", hidden: true, edge: true, hint: "Si el saldo ya alcanza la meta, no hace falta ningún año."),
                In("Meta lejana", "100\n1000", "48", hidden: true),
            ],
            "No sabemos de antemano cuántas vueltas habrá: es el caso típico de while.",
            ["Repite mientras saldo < meta.", "En cada vuelta suma saldo * 0.05m y cuenta un año."],
            difficulty: 2, related: ["n0.aritmetica"]);

        return new Skill(s, UnitId, "Bucles while y do-while",
            "Repetir mientras se cumpla una condición.",
            Tier.Essential, ["n1.if-else"],
            new Lesson(s,
                "Escribir bucles while y do-while que terminan, y elegir entre ellos según si el cuerpo debe ejecutarse al menos una vez.",
                [
                    P("while repite un bloque mientras su condición sea verdadera. La condición se revisa antes de cada vuelta, así que si es falsa desde el inicio, el cuerpo no se ejecuta nunca."),
                    Code("""
                        int vidas = 3;
                        while (vidas > 0)
                        {
                            Console.WriteLine($"Te quedan {vidas} vidas");
                            vidas--;
                        }
                        """),
                    P("Todo bucle necesita tres cosas: un estado inicial (vidas = 3), una condición (vidas > 0) y un cambio que acerque la condición a ser falsa (vidas--). Si falta el cambio, el bucle es infinito."),
                    P("do-while evalúa la condición al final: el cuerpo se ejecuta al menos una vez. Es útil, por ejemplo, para mostrar un menú y repetirlo hasta que el usuario elija salir."),
                    Tip("Usa while cuando no sabes cuántas vueltas habrá (hasta que el usuario acierte, hasta alcanzar una meta). Si conoces el número de repeticiones, for suele ser más claro."),
                ],
                new WorkedExample("Duplicar hasta superar un límite",
                    """
                    int valor = 1;
                    int pasos = 0;
                    while (valor <= 100)
                    {
                        valor *= 2;
                        pasos++;
                    }
                    Console.WriteLine($"{valor} después de {pasos} pasos");
                    """,
                    [
                        Line(1, "Estado inicial."),
                        Line(3, "La condición se revisa antes de cada vuelta."),
                        Line(5, "El cambio: valor crece, así que en algún momento superará 100."),
                        Line(8, "Al salir, valor es el primer múltiplo de 2 mayor que 100: 128, tras 7 duplicaciones."),
                    ],
                    "128 después de 7 pasos"),
                new CommonMistake(
                    "Olvidar actualizar la variable de la condición.",
                    "int i = 0;\nwhile (i < 3)\n{\n    Console.WriteLine(i);\n}",
                    "i nunca cambia, así que i < 3 siempre es verdadera y el programa no termina.",
                    "int i = 0;\nwhile (i < 3)\n{\n    Console.WriteLine(i);\n    i++;\n}"),
                "n1.while.pred1", "n1.while.guiado", "n1.while.indep", "n1.while.app",
                ["n1.while.recall1", "n1.while.recall2", "n1.while.concepto1"],
                "Escribes bucles que terminan siempre, incluido el caso de cero repeticiones, y explicas cuándo conviene do-while.",
                14));
    }

    private static Skill For(ContentRegistry r)
    {
        const string s = "n1.for";
        r.Short("n1.for.pred1", s, ItemPurpose.Prediction,
            "¿Qué muestra este programa? (escribe los valores tal como aparecen, en una línea)",
            "i empieza en 0 y aumenta de 2 en 2 mientras sea menor que 7: 0, 2, 4, 6. Write no agrega salto de línea.",
            ["0 2 4 6 "], match: AnswerMatch.Output,
            code: """
                for (int i = 0; i < 7; i += 2)
                {
                    Console.Write(i + " ");
                }
                """,
            wrong: new() { ["0 2 4 6 8 "] = "8 no es menor que 7, así que el bucle termina antes." });

        r.Short("n1.for.pred2", s, ItemPurpose.Prediction,
            "¿Cuántas veces se ejecuta el cuerpo de este bucle? Escribe solo el número.",
            "i toma los valores 1, 2, …, 10: son 10 vueltas. Con i <= 10 se incluye el 10.",
            ["10"], match: AnswerMatch.Number,
            code: """
                int vueltas = 0;
                for (int i = 1; i <= 10; i++)
                {
                    vueltas++;
                }
                Console.WriteLine(vueltas);
                """,
            wrong: new() { ["9"] = "Con <= 10 el último valor de i es 10.", ["11"] = "i empieza en 1, no en 0." },
            targets: [ErrorCategory.OffByOne]);

        r.Choice("n1.for.concepto1", s, ItemPurpose.Concept,
            "¿En qué orden se ejecutan las tres partes de for (inicio; condición; actualización)?",
            "El inicio se ejecuta una vez. Luego, en cada vuelta: se revisa la condición, si es verdadera se ejecuta el cuerpo y después la actualización.",
            [
                Ok("Inicio una vez; luego condición → cuerpo → actualización en cada vuelta.", "Correcto."),
                No("Inicio → actualización → condición → cuerpo en cada vuelta.", "La actualización ocurre después del cuerpo."),
                No("Las tres partes se ejecutan antes de cada vuelta.", "El inicio solo se ejecuta una vez."),
            ]);

        r.Choice("n1.for.recall1", s, ItemPurpose.Recall,
            "¿Qué bucle es más adecuado para repetir algo exactamente n veces?",
            "for agrupa en una línea la variable de control, la condición y la actualización, que es justo lo que necesita un conteo conocido.",
            [
                Ok("for", "Correcto."),
                No("do-while", "Sirve cuando el cuerpo debe ejecutarse al menos una vez y la condición se conoce después."),
                No("Ninguno: hay que copiar el código n veces.", "Precisamente para eso existen los bucles."),
            ]);

        r.Choice("n1.for.recall2", s, ItemPurpose.Recall,
            "¿Dónde existe la variable i declarada en for (int i = 0; …)?",
            "Su ámbito es el bucle: existe en la condición, la actualización y el cuerpo, pero no después del bucle.",
            [
                Ok("Solo dentro del bucle for.", "Correcto."),
                No("En todo el programa a partir de esa línea.", "Fuera del for, i ya no existe (CS0103).", ErrorCategory.Scope),
                No("Solo en la condición.", "También en el cuerpo y en la actualización."),
            ]);

        r.Choice("n1.for.bug1", s, ItemPurpose.BugFinding,
            "Se quiere sumar los números del 1 al 5 (resultado 15), pero muestra 10. ¿Dónde está el error?",
            "La condición i < 5 excluye el 5: suma 1+2+3+4 = 10. Es un error «por uno» (off-by-one); debe ser i <= 5.",
            [
                Ok("La condición i < 5 deja fuera el 5; debe ser i <= 5.", "Correcto: es el clásico error por uno."),
                No("suma debería empezar en 1.", "Entonces el resultado sería 11, tampoco 15.", ErrorCategory.WrongLogic),
                No("Debe usarse while en lugar de for.", "for es adecuado; el problema es el límite."),
            ],
            code: """
                int suma = 0;
                for (int i = 1; i < 5; i++)
                {
                    suma += i;
                }
                Console.WriteLine(suma);
                """,
            targets: [ErrorCategory.OffByOne]);

        r.Code("n1.for.guiado", s, ItemPurpose.Guided, CodeMode.Program,
            "Completa el for: lee un número n y muestra su tabla de multiplicar del 1 al 10 con el formato «n x i = resultado».",
            """
            int n = int.Parse(Console.ReadLine()!);
            for (int i = 1; i < 1; i++) // corrige la condición
            {
                // muestra la línea de la tabla
            }
            """,
            """
            int n = int.Parse(Console.ReadLine()!);
            for (int i = 1; i <= 10; i++)
            {
                Console.WriteLine($"{n} x {i} = {n * i}");
            }
            """,
            [
                In("Tabla del 3", "3", "3 x 1 = 3\n3 x 2 = 6\n3 x 3 = 9\n3 x 4 = 12\n3 x 5 = 15\n3 x 6 = 18\n3 x 7 = 21\n3 x 8 = 24\n3 x 9 = 27\n3 x 10 = 30"),
                In("Tabla del 7", "7", "7 x 1 = 7\n7 x 2 = 14\n7 x 3 = 21\n7 x 4 = 28\n7 x 5 = 35\n7 x 6 = 42\n7 x 7 = 49\n7 x 8 = 56\n7 x 9 = 63\n7 x 10 = 70", hidden: true),
            ],
            "El for recorre i de 1 a 10 y la interpolación arma cada línea.",
            ["La condición debe incluir el 10: i <= 10.", "Usa $\"{n} x {i} = {n * i}\"."],
            requires: [CodeConstruct.Loop]);

        r.Code("n1.for.indep", s, ItemPurpose.Independent, CodeMode.Program,
            "Lee un entero n (n ≥ 0) y muestra n! (factorial: 1 × 2 × … × n). Por definición 0! = 1. Usa long para el resultado.",
            """
            int n = int.Parse(Console.ReadLine()!);
            """,
            """
            int n = int.Parse(Console.ReadLine()!);
            long factorial = 1;
            for (int i = 2; i <= n; i++)
            {
                factorial *= i;
            }
            Console.WriteLine(factorial);
            """,
            [
                In("5!", "5", "120"),
                In("1!", "1", "1"),
                In("0!", "0", "1", hidden: true, edge: true, hint: "0! es 1: si el bucle no se ejecuta, el resultado debe seguir siendo 1."),
                In("20!", "20", "2432902008176640000", hidden: true, edge: true, hint: "20! no cabe en int: usa long."),
            ],
            "Un acumulador multiplicativo empieza en 1 (el elemento neutro de la multiplicación).",
            ["Empieza con long factorial = 1;", "Multiplica por cada i desde 2 hasta n."],
            difficulty: 2, requires: [CodeConstruct.Loop], targets: [ErrorCategory.EdgeCaseMissed]);

        r.Code("n1.for.app", s, ItemPurpose.Transfer, CodeMode.Program,
            "Aplicación: dibuja una escalera. Lee la altura h y muestra h líneas: la línea i tiene i asteriscos. Por ejemplo, con 3: «*», «**», «***».",
            """
            int h = int.Parse(Console.ReadLine()!);
            """,
            """
            int h = int.Parse(Console.ReadLine()!);
            for (int i = 1; i <= h; i++)
            {
                Console.WriteLine(new string('*', i));
            }
            """,
            [
                In("Altura 3", "3", "*\n**\n***"),
                In("Altura 5", "5", "*\n**\n***\n****\n*****", hidden: true),
                In("Altura 1", "1", "*", hidden: true, edge: true),
            ],
            "Puedes construir cada línea con un bucle anidado o con new string('*', i), que repite un carácter i veces.",
            ["Un for externo recorre las líneas de 1 a h.", "Para cada línea, crea i asteriscos (bucle interno o new string('*', i))."],
            difficulty: 2, requires: [CodeConstruct.Loop], related: ["n0.texto"]);

        return new Skill(s, UnitId, "Bucle for",
            "Repetir un número conocido de veces.",
            Tier.Essential, ["n1.while"],
            new Lesson(s,
                "Usar for para repetir un número conocido de veces, controlar exactamente los límites y evitar errores por uno.",
                [
                    P("for reúne en una línea las tres piezas de un bucle con contador: inicio, condición y actualización."),
                    Code("""
                        for (int i = 1; i <= 3; i++)
                        {
                            Console.WriteLine($"Vuelta {i}");
                        }
                        """),
                    P("El inicio (int i = 1) se ejecuta una vez. Antes de cada vuelta se revisa la condición (i <= 3); si es verdadera se ejecuta el cuerpo y luego la actualización (i++)."),
                    P("Los límites importan: for (int i = 0; i < n; i++) da n vueltas (de 0 a n-1); for (int i = 1; i <= n; i++) también da n vueltas (de 1 a n). Mezclar ambos estilos produce errores «por uno»."),
                    Tip("Para verificar un bucle, prueba mentalmente la primera y la última vuelta, y el caso en que no debe ejecutarse ninguna (n = 0)."),
                ],
                new WorkedExample("Suma de los pares hasta n",
                    """
                    int n = 10;
                    int suma = 0;
                    for (int i = 2; i <= n; i += 2)
                    {
                        suma += i;
                    }
                    Console.WriteLine(suma);
                    """,
                    [
                        Line(3, "Empieza en 2 y avanza de 2 en 2: solo visita pares."),
                        Line(3, "<= n incluye a n si es par."),
                        Line(5, "Acumula: 2 + 4 + 6 + 8 + 10."),
                        Line(7, "Resultado: 30."),
                    ],
                    "30"),
                new CommonMistake(
                    "Equivocarse por uno en el límite del bucle.",
                    "// mostrar del 1 al 5\nfor (int i = 1; i < 5; i++)\n{\n    Console.WriteLine(i);\n}",
                    "Con < 5 el bucle termina en 4. Si el rango incluye el 5, la condición debe ser <= 5.",
                    "for (int i = 1; i <= 5; i++)\n{\n    Console.WriteLine(i);\n}"),
                "n1.for.pred1", "n1.for.guiado", "n1.for.indep", "n1.for.app",
                ["n1.for.recall1", "n1.for.recall2", "n1.for.concepto1"],
                "Escribes bucles for con los límites correctos a la primera, incluido el caso de cero vueltas, y predices cuántas veces se ejecutan.",
                14));
    }

    private static Skill BreakContinue(ContentRegistry r)
    {
        const string s = "n1.break-continue";
        r.Short("n1.break-continue.pred1", s, ItemPurpose.Prediction,
            "¿Qué muestra este programa? (una línea)",
            "continue salta el resto de la vuelta cuando i es múltiplo de 3 (3 y 6), y break termina el bucle al llegar a 8. Se muestran 1 2 4 5 7.",
            ["1 2 4 5 7 "], match: AnswerMatch.Output,
            code: """
                for (int i = 1; i <= 10; i++)
                {
                    if (i == 8)
                    {
                        break;
                    }
                    if (i % 3 == 0)
                    {
                        continue;
                    }
                    Console.Write(i + " ");
                }
                """,
            difficulty: 2);

        r.Choice("n1.break-continue.concepto1", s, ItemPurpose.Concept,
            "¿Qué diferencia hay entre break y continue dentro de un bucle?",
            "break termina el bucle por completo; continue termina solo la vuelta actual y pasa a la siguiente (en un for, ejecutando la actualización).",
            [
                Ok("break sale del bucle; continue salta a la siguiente vuelta.", "Correcto."),
                No("break salta a la siguiente vuelta; continue sale del bucle.", "Es al revés."),
                No("Ambos terminan el programa.", "Solo afectan al bucle más cercano que los contiene."),
            ]);

        r.Choice("n1.break-continue.recall1", s, ItemPurpose.Recall,
            "Si hay dos bucles anidados y se ejecuta break en el interno, ¿qué bucle termina?",
            "break solo sale del bucle más cercano que lo contiene: el interno. El externo continúa con su siguiente vuelta.",
            [
                Ok("Solo el interno.", "Correcto."),
                No("Los dos.", "break no sale de varios niveles a la vez.", ErrorCategory.WrongLogic),
                No("Solo el externo.", "Afecta al bucle que lo contiene directamente."),
            ],
            difficulty: 2);

        r.Short("n1.break-continue.recall2", s, ItemPurpose.Recall,
            "¿Qué instrucción usarías para dejar de buscar en cuanto encuentras lo que buscabas? (una palabra)",
            "break: termina el bucle en cuanto ya no tiene sentido seguir.",
            ["break"]);

        r.Choice("n1.break-continue.bug1", s, ItemPurpose.BugFinding,
            "Se quiere mostrar los impares del 1 al 9, pero el programa nunca termina. ¿Por qué?",
            "Cuando i es par se ejecuta continue antes de i++, así que i nunca cambia y el bucle se repite para siempre. En un while hay que actualizar antes de continue (o usar for).",
            [
                Ok("continue salta el i++ cuando i es par, así que i deja de cambiar.", "Correcto."),
                No("% no funciona en un while.", "% funciona en cualquier expresión."),
                No("continue termina el bucle.", "continue pasa a la siguiente vuelta; el problema es que i no avanza.", ErrorCategory.ConceptMisunderstanding),
            ],
            code: """
                int i = 1;
                while (i <= 9)
                {
                    if (i % 2 == 0)
                    {
                        continue;
                    }
                    Console.WriteLine(i);
                    i++;
                }
                """,
            difficulty: 2, targets: [ErrorCategory.InfiniteLoopOrTimeout]);

        r.Code("n1.break-continue.guiado", s, ItemPurpose.Guided, CodeMode.Program,
            "Completa el programa: lee números (uno por línea) y súmalos hasta que aparezca un 0, que indica el final. Ignora (no sumes) los números negativos. Muestra la suma.",
            """
            int suma = 0;
            while (true)
            {
                int numero = int.Parse(Console.ReadLine()!);
                // si es 0, termina el bucle
                // si es negativo, pasa al siguiente número
                suma += numero;
            }
            Console.WriteLine(suma);
            """,
            """
            int suma = 0;
            while (true)
            {
                int numero = int.Parse(Console.ReadLine()!);
                if (numero == 0)
                {
                    break;
                }
                if (numero < 0)
                {
                    continue;
                }
                suma += numero;
            }
            Console.WriteLine(suma);
            """,
            [
                In("Solo positivos", "4\n6\n0", "10"),
                In("Con negativos", "5\n-3\n2\n0", "7"),
                In("Termina de inmediato", "0", "0", hidden: true, edge: true),
                In("Datos después del 0", "1\n0\n99", "1", hidden: true, edge: true, hint: "Después del 0 no se debe leer ni sumar nada más."),
            ],
            "while (true) con break es una forma clara de «repetir hasta encontrar el valor centinela».",
            ["if (numero == 0) break;", "if (numero < 0) continue;"],
            requires: [CodeConstruct.Loop]);

        r.Code("n1.break-continue.indep", s, ItemPurpose.Independent, CodeMode.Program,
            "Lee un entero n ≥ 2 y muestra «Primo» si es primo o «Compuesto: d» con su menor divisor d mayor que 1. Detén la búsqueda en cuanto encuentres un divisor.",
            """
            int n = int.Parse(Console.ReadLine()!);
            """,
            """
            int n = int.Parse(Console.ReadLine()!);
            int divisor = 0;
            for (int d = 2; (long)d * d <= n; d++)
            {
                if (n % d == 0)
                {
                    divisor = d;
                    break;
                }
            }
            Console.WriteLine(divisor == 0 ? "Primo" : $"Compuesto: {divisor}");
            """,
            [
                In("7 es primo", "7", "Primo"),
                In("15 es compuesto", "15", "Compuesto: 3"),
                In("2 es primo", "2", "Primo", hidden: true, edge: true),
                In("Cuadrado de primo", "49", "Compuesto: 7", hidden: true, edge: true, hint: "Revisa los divisores hasta la raíz incluida: 7 * 7 = 49."),
                In("Primo grande", "2147483647", "Primo", hidden: true, hint: "Para números grandes no revises todos los divisores: basta con llegar a la raíz cuadrada."),
            ],
            "Si n tiene un divisor, tiene uno menor o igual que su raíz cuadrada; por eso basta probar mientras d * d <= n. break evita trabajo inútil.",
            ["Prueba divisores desde 2 y detente con break al primero que divida a n.", "No hace falta probar hasta n: basta mientras d * d <= n (usa long si temes desbordamiento)."],
            difficulty: 3, requires: [CodeConstruct.Loop], related: ["n1.complejidad"]);

        r.Code("n1.break-continue.app", s, ItemPurpose.Transfer, CodeMode.Program,
            "Aplicación: un juego da al jugador 3 intentos para adivinar un número secreto. Lee el número secreto y luego hasta 3 intentos (uno por línea). Si acierta, muestra «¡Acertaste en el intento k!» y no leas más. Si falla los 3, muestra «Sin intentos. Era s».",
            """
            int secreto = int.Parse(Console.ReadLine()!);
            """,
            """
            int secreto = int.Parse(Console.ReadLine()!);
            bool acerto = false;
            for (int intento = 1; intento <= 3; intento++)
            {
                int valor = int.Parse(Console.ReadLine()!);
                if (valor == secreto)
                {
                    Console.WriteLine($"¡Acertaste en el intento {intento}!");
                    acerto = true;
                    break;
                }
            }
            if (!acerto)
            {
                Console.WriteLine($"Sin intentos. Era {secreto}");
            }
            """,
            [
                In("Acierta al segundo", "42\n10\n42", "¡Acertaste en el intento 2!"),
                In("Falla todo", "7\n1\n2\n3", "Sin intentos. Era 7"),
                In("Acierta al primero", "5\n5", "¡Acertaste en el intento 1!", hidden: true, edge: true, hint: "Si acierta, no leas más líneas."),
                In("Acierta al último", "9\n1\n2\n9", "¡Acertaste en el intento 3!", hidden: true, edge: true),
            ],
            "Un bool «bandera» recuerda si se salió por break o porque se agotaron las vueltas.",
            ["Usa un for de 1 a 3 y break al acertar.", "Una variable bool te dice, después del bucle, si hubo acierto."],
            difficulty: 2, requires: [CodeConstruct.Loop], related: ["n1.for"]);

        return new Skill(s, UnitId, "break y continue",
            "Salir de un bucle o saltar una iteración.",
            Tier.Essential, ["n1.for"],
            new Lesson(s,
                "Controlar el flujo dentro de un bucle con break y continue sin provocar bucles infinitos.",
                [
                    P("break termina inmediatamente el bucle que lo contiene. Es útil para dejar de buscar en cuanto encuentras lo que necesitas o para detenerte ante un valor centinela."),
                    P("continue termina la vuelta actual y pasa a la siguiente. En un for, la actualización (i++) se ejecuta igualmente; en un while hay que cuidar que la variable avance antes del continue."),
                    Code("""
                        for (int i = 1; i <= 6; i++)
                        {
                            if (i % 2 == 0)
                            {
                                continue; // salta los pares
                            }
                            Console.WriteLine(i);
                        }
                        """),
                    Design("Usa break y continue para simplificar, no para saltar por todas partes. Si un bucle tiene muchos break, quizá la condición del bucle debería expresar mejor cuándo terminar."),
                ],
                new WorkedExample("Buscar el primer múltiplo",
                    """
                    int buscado = -1;
                    for (int n = 50; n <= 100; n++)
                    {
                        if (n % 7 != 0)
                        {
                            continue;
                        }
                        buscado = n;
                        break;
                    }
                    Console.WriteLine(buscado);
                    """,
                    [
                        Line(1, "-1 indica «todavía no encontrado»."),
                        Line(4, "Si n no es múltiplo de 7, continue pasa al siguiente n."),
                        Line(8, "Al encontrar el primero (56) se guarda y break termina la búsqueda."),
                    ],
                    "56"),
                new CommonMistake(
                    "Usar continue en un while antes de actualizar la variable de control.",
                    "int i = 0;\nwhile (i < 10)\n{\n    if (i % 2 == 0) continue;\n    Console.WriteLine(i);\n    i++;\n}",
                    "Con i = 0, continue salta el i++ y el bucle se repite con el mismo valor para siempre.",
                    "int i = 0;\nwhile (i < 10)\n{\n    i++;\n    if (i % 2 == 0)\n    {\n        continue;\n    }\n    Console.WriteLine(i);\n}"),
                "n1.break-continue.pred1", "n1.break-continue.guiado", "n1.break-continue.indep", "n1.break-continue.app",
                ["n1.break-continue.recall1", "n1.break-continue.recall2", "n1.break-continue.concepto1"],
                "Usas break para terminar búsquedas y continue para filtrar, y detectas cuándo continue provoca un bucle infinito.",
                12));
    }

    private static Skill Acumuladores(ContentRegistry r)
    {
        const string s = "n1.acumuladores";
        r.Short("n1.acumuladores.pred1", s, ItemPurpose.Prediction,
            "¿Qué número muestra este programa?",
            "mayor empieza en el primer valor (4) y se actualiza cada vez que aparece uno más grande: 9. El -2 no cambia nada.",
            ["9"], match: AnswerMatch.Number,
            code: """
                int[] datos = [4, 9, -2, 7];
                int mayor = datos[0];
                foreach (int d in datos)
                {
                    if (d > mayor)
                    {
                        mayor = d;
                    }
                }
                Console.WriteLine(mayor);
                """);

        r.Choice("n1.acumuladores.pred2", s, ItemPurpose.Prediction,
            "¿Qué muestra este programa?",
            "La división suma / cuenta es entre int: 10 / 4 = 2 (se descartan decimales). Para obtener 2.5 hay que convertir a double antes de dividir.",
            [
                Ok("2", "Correcto: división entera."),
                No("2.5", "suma y cuenta son int; la división descarta la parte decimal.", ErrorCategory.OperatorMisuse),
                No("3", "La división entera trunca; no redondea.", ErrorCategory.OperatorMisuse),
            ],
            code: """
                int suma = 0;
                int cuenta = 0;
                foreach (int n in new[] { 1, 2, 3, 4 })
                {
                    suma += n;
                    cuenta++;
                }
                Console.WriteLine(suma / cuenta);
                """,
            difficulty: 2, targets: [ErrorCategory.OperatorMisuse]);

        r.Choice("n1.acumuladores.concepto1", s, ItemPurpose.Concept,
            "Para encontrar el mínimo de una serie de números que puede tener negativos, ¿con qué valor conviene inicializar la variable mínimo?",
            "Con el primer elemento (o con int.MaxValue). Si empiezas en 0, nunca encontrarás un mínimo positivo mayor que 0 y el resultado será incorrecto.",
            [
                Ok("Con el primer número de la serie (o int.MaxValue).", "Correcto: así cualquier valor real puede reemplazarlo."),
                No("Con 0.", "Si todos los números son positivos, 0 nunca sería reemplazado.", ErrorCategory.WrongLogic),
                No("Con -1.", "Un valor inventado puede ser menor que todos los reales.", ErrorCategory.WrongLogic),
            ],
            targets: [ErrorCategory.WrongLogic]);

        r.Choice("n1.acumuladores.recall1", s, ItemPurpose.Recall,
            "¿Con qué valor se inicializa un acumulador de producto?",
            "Con 1, el elemento neutro de la multiplicación. Con 0 el producto siempre sería 0.",
            [
                Ok("1", "Correcto."),
                No("0", "Cualquier número multiplicado por 0 es 0.", ErrorCategory.WrongLogic),
                No("Con el último elemento.", "Se inicializa antes del bucle con el neutro."),
            ]);

        r.Choice("n1.acumuladores.recall2", s, ItemPurpose.Recall,
            "¿Cuál es la diferencia entre un contador y un acumulador?",
            "Un contador aumenta en una cantidad fija (normalmente 1) cada vez que ocurre algo; un acumulador suma (o combina) valores variables.",
            [
                Ok("El contador suma 1 por cada evento; el acumulador suma valores que cambian.", "Correcto."),
                No("Son lo mismo con distinto nombre.", "El contador cuenta ocurrencias; el acumulador combina valores."),
                No("El contador solo existe en for y el acumulador en while.", "Ambos se usan en cualquier bucle."),
            ]);

        r.Choice("n1.acumuladores.bug1", s, ItemPurpose.BugFinding,
            "Este programa debería sumar 1 + 2 + 3 (= 6), pero muestra 3. ¿Por qué?",
            "suma se declara dentro del bucle, así que se reinicia en 0 en cada vuelta y solo «recuerda» el último valor. Además, fuera del bucle no existiría: aquí la línea final muestra otra variable. El acumulador debe declararse antes del bucle.",
            [
                Ok("El acumulador se reinicia en cada vuelta porque se declara dentro del bucle.", "Correcto."),
                No("El for debería empezar en 0.", "Sumar 0 no cambia el resultado.", ErrorCategory.OffByOne),
                No("+= no funciona con int.", "+= funciona con int.", ErrorCategory.OperatorMisuse),
            ],
            code: """
                int ultimo = 0;
                for (int i = 1; i <= 3; i++)
                {
                    int suma = 0;
                    suma += i;
                    ultimo = suma;
                }
                Console.WriteLine(ultimo);
                """,
            difficulty: 2, targets: [ErrorCategory.Scope]);

        r.Code("n1.acumuladores.guiado", s, ItemPurpose.Guided, CodeMode.Program,
            "Completa el programa: lee la cantidad n y luego n calificaciones enteras. Muestra el promedio con un decimal (usa :F1) y cuántas son aprobatorias (≥ 60), en dos líneas.",
            """
            int n = int.Parse(Console.ReadLine()!);
            int suma = 0;
            int aprobadas = 0;
            for (int i = 0; i < n; i++)
            {
                int nota = int.Parse(Console.ReadLine()!);
                // acumula y cuenta
            }
            double promedio = 0; // calcula el promedio
            Console.WriteLine($"{promedio:F1}");
            Console.WriteLine(aprobadas);
            """,
            """
            int n = int.Parse(Console.ReadLine()!);
            int suma = 0;
            int aprobadas = 0;
            for (int i = 0; i < n; i++)
            {
                int nota = int.Parse(Console.ReadLine()!);
                suma += nota;
                if (nota >= 60)
                {
                    aprobadas++;
                }
            }
            double promedio = (double)suma / n;
            Console.WriteLine($"{promedio:F1}");
            Console.WriteLine(aprobadas);
            """,
            [
                In("Tres notas", "3\n80\n50\n95", "75.0\n2"),
                In("Promedio con decimales", "2\n70\n75", "72.5\n2", hidden: true, hint: "Convierte a double antes de dividir para no perder los decimales."),
                In("Todas reprobadas", "2\n10\n59", "34.5\n0", hidden: true, edge: true),
            ],
            "La suma es un acumulador, aprobadas es un contador. El cast a double evita la división entera.",
            ["suma += nota; y, si nota >= 60, aprobadas++;", "promedio = (double)suma / n;"],
            requires: [CodeConstruct.Loop], targets: [ErrorCategory.OperatorMisuse]);

        r.Code("n1.acumuladores.indep", s, ItemPurpose.Independent, CodeMode.Program,
            "Lee números enteros (uno por línea) hasta encontrar «fin». Muestra en una línea el mínimo y el máximo separados por un espacio. Si no hubo ningún número, muestra «Sin datos».",
            """
            string linea = Console.ReadLine()!;
            """,
            """
            int? minimo = null;
            int? maximo = null;
            string linea = Console.ReadLine()!;
            while (linea != "fin")
            {
                int n = int.Parse(linea);
                if (minimo is null || n < minimo)
                {
                    minimo = n;
                }
                if (maximo is null || n > maximo)
                {
                    maximo = n;
                }
                linea = Console.ReadLine()!;
            }
            Console.WriteLine(minimo is null ? "Sin datos" : $"{minimo} {maximo}");
            """,
            [
                In("Varios", "5\n-2\n8\n3\nfin", "-2 8"),
                In("Uno solo", "4\nfin", "4 4"),
                In("Todos positivos", "7\n9\n12\nfin", "7 12", hidden: true, edge: true, hint: "No inicialices el mínimo en 0: si todos son positivos, 0 nunca se reemplaza."),
                In("Sin datos", "fin", "Sin datos", hidden: true, edge: true),
            ],
            "Inicializar mínimo y máximo con el primer dato (o marcar «sin datos» con null) evita valores inventados.",
            ["Usa una variable que indique si ya viste un número (un bool o int? con null).", "Actualiza el mínimo si n es menor y el máximo si n es mayor."],
            difficulty: 2, requires: [CodeConstruct.Loop], targets: [ErrorCategory.WrongLogic, ErrorCategory.EdgeCaseMissed]);

        r.Code("n1.acumuladores.app", s, ItemPurpose.Transfer, CodeMode.Program,
            "Aplicación: un registro de temperaturas diarias. Lee n y luego n temperaturas enteras. Muestra tres líneas: la cantidad de días con helada (temperatura < 0), la racha más larga de días consecutivos sin helada y el promedio con un decimal.",
            """
            int n = int.Parse(Console.ReadLine()!);
            """,
            """
            int n = int.Parse(Console.ReadLine()!);
            int heladas = 0;
            int rachaActual = 0;
            int mejorRacha = 0;
            int suma = 0;
            for (int i = 0; i < n; i++)
            {
                int t = int.Parse(Console.ReadLine()!);
                suma += t;
                if (t < 0)
                {
                    heladas++;
                    rachaActual = 0;
                }
                else
                {
                    rachaActual++;
                    if (rachaActual > mejorRacha)
                    {
                        mejorRacha = rachaActual;
                    }
                }
            }
            Console.WriteLine(heladas);
            Console.WriteLine(mejorRacha);
            Console.WriteLine($"{(double)suma / n:F1}");
            """,
            [
                In("Semana", "7\n3\n-1\n2\n5\n6\n-4\n1", "2\n3\n1.7"),
                In("Sin heladas", "3\n10\n12\n11", "0\n3\n11.0", hidden: true, edge: true, hint: "Si nunca hay helada, la racha más larga son todos los días."),
                In("Todas heladas", "2\n-3\n-5", "2\n0\n-4.0", hidden: true, edge: true),
            ],
            "Se combinan un contador (heladas), un acumulador (suma) y un «máximo de rachas», que es un contador que se reinicia.",
            ["Lleva la racha actual y la mejor racha; reinicia la actual cuando hay helada.", "Actualiza la mejor racha cada vez que la actual la supere."],
            difficulty: 3, requires: [CodeConstruct.Loop], related: ["n1.if-else"]);

        return new Skill(s, UnitId, "Contadores y acumuladores",
            "Contar, sumar, encontrar máximos y mínimos.",
            Tier.Essential, ["n1.for"],
            new Lesson(s,
                "Recorrer datos acumulando información: conteos, sumas, promedios, máximos, mínimos y rachas.",
                [
                    P("Muchos problemas siguen el mismo patrón: declarar variables de resultado antes del bucle, actualizarlas en cada vuelta y usarlas después."),
                    P("Un contador suma 1 cada vez que ocurre algo (cuántos aprobaron). Un acumulador combina valores (la suma de las notas). Un máximo o mínimo guarda el mejor valor visto hasta el momento."),
                    Code("""
                        int[] ventas = [120, 80, 200, 50];
                        int total = 0;
                        int diasBuenos = 0;
                        int mejor = ventas[0];
                        foreach (int v in ventas)
                        {
                            total += v;
                            if (v >= 100) diasBuenos++;
                            if (v > mejor) mejor = v;
                        }
                        Console.WriteLine($"{total} {diasBuenos} {mejor}");
                        """),
                    Warn("Inicializa con el elemento neutro: 0 para sumas, 1 para productos, y el primer dato para máximos y mínimos."),
                    Tip("Para promedios, convierte a double antes de dividir: (double)suma / cantidad."),
                ],
                new WorkedExample("Estadísticas de una lista",
                    """
                    int[] edades = [23, 17, 35, 17, 41];
                    int suma = 0;
                    int menores = 0;
                    int minimo = edades[0];
                    foreach (int e in edades)
                    {
                        suma += e;
                        if (e < 18)
                        {
                            menores++;
                        }
                        if (e < minimo)
                        {
                            minimo = e;
                        }
                    }
                    double promedio = (double)suma / edades.Length;
                    Console.WriteLine($"Promedio {promedio:F1}, menores {menores}, mínimo {minimo}");
                    """,
                    [
                        Line(2, "Acumulador de suma: empieza en 0."),
                        Line(3, "Contador: empieza en 0 y suma 1 por cada menor de edad."),
                        Line(4, "El mínimo empieza con el primer dato, no con 0."),
                        Line(16, "El cast a double conserva los decimales: 133 / 5 = 26.6."),
                    ],
                    "Promedio 26.6, menores 2, mínimo 17"),
                new CommonMistake(
                    "Declarar el acumulador dentro del bucle.",
                    "for (int i = 1; i <= 3; i++)\n{\n    int suma = 0;\n    suma += i;\n}",
                    "La variable se crea de nuevo en cada vuelta con valor 0, así que nunca acumula; además no existe fuera del bucle.",
                    "int suma = 0;\nfor (int i = 1; i <= 3; i++)\n{\n    suma += i;\n}\nConsole.WriteLine(suma);"),
                "n1.acumuladores.pred1", "n1.acumuladores.guiado", "n1.acumuladores.indep", "n1.acumuladores.app",
                ["n1.acumuladores.recall1", "n1.acumuladores.recall2", "n1.acumuladores.concepto1"],
                "Resuelves problemas de conteo, suma, promedio, máximo y mínimo con la inicialización correcta y manejas el caso sin datos.",
                15));
    }
}
