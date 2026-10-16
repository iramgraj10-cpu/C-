using CSharpMastery.Content.Infrastructure;
using CSharpMastery.Domain.Curriculum;
using CSharpMastery.Domain.Items;
using CSharpMastery.Domain.Progress;
using static CSharpMastery.Content.Infrastructure.ContentRegistry;
using static CSharpMastery.Content.Infrastructure.L;

namespace CSharpMastery.Content.Nivel2;

internal static class U04Recursion
{
    public const string UnitId = "n2.u4";

    public static Unit Build(ContentRegistry r) => new(UnitId, "n2", "Recursividad",
        "Problemas que se definen en términos de sí mismos.",
        [Recursion(r)]);

    private static Skill Recursion(ContentRegistry r)
    {
        const string s = "n2.recursion";
        r.Short("n2.recursion.pred1", s, ItemPurpose.Prediction,
            "¿Qué muestra este programa?",
            "Cuenta(3) muestra 3 y llama a Cuenta(2), que muestra 2 y llama a Cuenta(1)… Cuenta(0) es el caso base: muestra «¡Ya!» y no vuelve a llamarse.",
            ["3\n2\n1\n¡Ya!"], match: AnswerMatch.Output,
            code: """
                Cuenta(3);

                void Cuenta(int n)
                {
                    if (n == 0)
                    {
                        Console.WriteLine("¡Ya!");
                        return;
                    }
                    Console.WriteLine(n);
                    Cuenta(n - 1);
                }
                """);

        r.Choice("n2.recursion.pred2", s, ItemPurpose.Prediction,
            "¿Qué muestra este programa? Observa que la línea Console.WriteLine está DESPUÉS de la llamada recursiva.",
            "Cada llamada primero baja hasta el caso base y solo al volver muestra su número. Por eso el orden se invierte: la pila de llamadas se deshace desde la más profunda.",
            [
                Ok("1\n2\n3", "Correcto: se muestra al regresar de cada llamada."),
                No("3\n2\n1", "Eso ocurriría si se mostrara antes de la llamada recursiva.", ErrorCategory.ConceptMisunderstanding),
                No("3", "Las tres llamadas llegan a ejecutar su Console.WriteLine.", ErrorCategory.ConceptMisunderstanding),
            ],
            code: """
                Subir(3);

                void Subir(int n)
                {
                    if (n == 0) return;
                    Subir(n - 1);
                    Console.WriteLine(n);
                }
                """,
            difficulty: 3);

        r.Choice("n2.recursion.recall1", s, ItemPurpose.Recall,
            "¿Cuáles son las dos partes imprescindibles de un método recursivo?",
            "Un caso base que se resuelve sin recursión y un caso recursivo que se acerca al caso base en cada llamada.",
            [
                Ok("Un caso base y un caso recursivo que se acerca a él.", "Correcto."),
                No("Un bucle y un contador.", "La recursión reemplaza al bucle por llamadas."),
                No("Un parámetro y un valor de retorno.", "Ayudan, pero sin caso base la recursión no termina."),
            ]);

        r.Choice("n2.recursion.recall2", s, ItemPurpose.Recall,
            "¿Qué ocurre si un método recursivo nunca alcanza su caso base?",
            "Cada llamada ocupa espacio en la pila de llamadas. Si nunca terminan, la pila se agota y el programa termina con StackOverflowException, que no se puede atrapar.",
            [
                Ok("Se agota la pila de llamadas: StackOverflowException.", "Correcto."),
                No("Se queda en un bucle infinito que no termina nunca.", "A diferencia de un bucle, cada llamada consume memoria de la pila y el programa cae.", ErrorCategory.StackOverflowRecursion),
                No("El compilador lo detecta y no compila.", "En general el compilador no puede detectarlo.", ErrorCategory.StackOverflowRecursion),
            ],
            targets: [ErrorCategory.StackOverflowRecursion]);

        r.Choice("n2.recursion.concepto1", s, ItemPurpose.Concept,
            "Fib(n) = Fib(n-1) + Fib(n-2) escrito de forma recursiva directa tarda muchísimo con n = 45. ¿Por qué?",
            "Cada llamada genera dos más y los mismos valores se recalculan una y otra vez: el número de llamadas crece de forma exponencial. Un bucle o guardar resultados ya calculados (memoización) lo resuelve.",
            [
                Ok("Recalcula los mismos valores muchísimas veces; las llamadas crecen exponencialmente.", "Correcto."),
                No("Porque la recursión siempre es lenta.", "Muchas soluciones recursivas son eficientes; el problema es recalcular.", ErrorCategory.ConceptMisunderstanding),
                No("Porque int no alcanza para Fib(45).", "Fib(45) = 1 134 903 170 cabe en int; el problema es el tiempo.", ErrorCategory.ConceptMisunderstanding),
            ],
            related: ["n1.complejidad"]);

        r.Choice("n2.recursion.bug1", s, ItemPurpose.BugFinding,
            "Factorial(5) termina con StackOverflowException. ¿Cuál es el error?",
            "La llamada recursiva usa n en lugar de n - 1, así que nunca se acerca al caso base.",
            [
                Ok("La llamada recursiva no reduce n: debe ser Factorial(n - 1).", "Correcto."),
                No("El caso base debe ser n == 0.", "n <= 1 es un caso base válido; el problema es que nunca se alcanza.", ErrorCategory.StackOverflowRecursion),
                No("Debe devolver long en lugar de int.", "Eso ayudaría con números grandes, pero no evita la recursión infinita.", ErrorCategory.TypeMismatch),
            ],
            code: """
                int Factorial(int n)
                {
                    if (n <= 1) return 1;
                    return n * Factorial(n);
                }
                """,
            targets: [ErrorCategory.StackOverflowRecursion]);

        r.Code("n2.recursion.guiado", s, ItemPurpose.Guided, CodeMode.Function,
            "Completa SumaDigitos de forma recursiva: la suma de los dígitos de n es su último dígito (n % 10) más la suma de los dígitos de n / 10. El caso base es n < 10.",
            """
            public static class Recursivo
            {
                public static int SumaDigitos(int n)
                {
                    if (n < 10)
                    {
                        return n;
                    }
                    // caso recursivo
                    return 0;
                }
            }
            """,
            """
            public static class Recursivo
            {
                public static int SumaDigitos(int n)
                {
                    if (n < 10)
                    {
                        return n;
                    }
                    return n % 10 + SumaDigitos(n / 10);
                }
            }
            """,
            [
                Is("Tres dígitos", "Recursivo.SumaDigitos(123)", "6"),
                Is("Un dígito", "Recursivo.SumaDigitos(7)", "7", edge: true),
                Is("Con ceros", "Recursivo.SumaDigitos(1005)", "6", hidden: true, edge: true),
            ],
            "n / 10 quita el último dígito, así que cada llamada trabaja con un número más corto hasta llegar al caso base.",
            ["El último dígito es n % 10.", "return n % 10 + SumaDigitos(n / 10);"],
            requires: [CodeConstruct.Recursion], related: ["n0.aritmetica"]);

        r.Code("n2.recursion.indep", s, ItemPurpose.Independent, CodeMode.Function,
            "Escribe Potencia(long b, int e) recursiva que calcule b elevado a e (e >= 0) sin usar Math.Pow ni bucles. Debe ser eficiente: usa que b^e = (b^(e/2))² cuando e es par.",
            """
            public static class Recursivo
            {
                public static long Potencia(long b, int e)
                {
                    return 1;
                }
            }
            """,
            """
            public static class Recursivo
            {
                public static long Potencia(long b, int e)
                {
                    if (e == 0)
                    {
                        return 1;
                    }
                    long mitad = Potencia(b, e / 2);
                    return e % 2 == 0 ? mitad * mitad : mitad * mitad * b;
                }
            }
            """,
            [
                Is("2^10", "Recursivo.Potencia(2, 10)", "1024"),
                Is("Exponente 0", "Recursivo.Potencia(7, 0)", "1", edge: true),
                Is("Impar", "Recursivo.Potencia(3, 5)", "243", hidden: true),
                Is("Base negativa", "Recursivo.Potencia(-2, 3)", "-8", hidden: true, edge: true),
                Is("Exponente grande", "Recursivo.Potencia(1, 1000000)", "1", hidden: true, hint: "Si reduces e de uno en uno, un millón de llamadas agota la pila. Divide el exponente a la mitad en cada paso."),
            ],
            "Dividir el exponente a la mitad hace unas 20 llamadas para e = 1 000 000 en lugar de un millón.",
            ["Caso base: e == 0 devuelve 1.", "Calcula la mitad una sola vez y guárdala en una variable; si e es impar, multiplica una vez más por b."],
            difficulty: 3, requires: [CodeConstruct.Recursion], targets: [ErrorCategory.StackOverflowRecursion]);

        r.Code("n2.recursion.app", s, ItemPurpose.Transfer, CodeMode.Function,
            "Aplicación: búsqueda binaria recursiva. Buscar(int[] ordenado, int x) devuelve la posición de x en el array ordenado de menor a mayor, o -1 si no está. Implementa un método auxiliar recursivo con los límites (izquierda, derecha). No uses bucles.",
            """
            public static class Busqueda
            {
                public static int Buscar(int[] ordenado, int x)
                {
                    return -1;
                }
            }
            """,
            """
            public static class Busqueda
            {
                public static int Buscar(int[] ordenado, int x) => Buscar(ordenado, x, 0, ordenado.Length - 1);

                private static int Buscar(int[] a, int x, int izquierda, int derecha)
                {
                    if (izquierda > derecha)
                    {
                        return -1;
                    }
                    int medio = izquierda + (derecha - izquierda) / 2;
                    if (a[medio] == x) return medio;
                    return a[medio] < x
                        ? Buscar(a, x, medio + 1, derecha)
                        : Buscar(a, x, izquierda, medio - 1);
                }
            }
            """,
            [
                Is("Encontrado", "Busqueda.Buscar([1, 3, 5, 7, 9], 7)", "3"),
                Is("No está", "Busqueda.Buscar([1, 3, 5], 4)", "-1"),
                Is("Extremos", "Busqueda.Buscar([2, 4, 6], 2) * 10 + Busqueda.Buscar([2, 4, 6], 6)", "2", hidden: true, edge: true),
                Is("Vacío", "Busqueda.Buscar([], 1)", "-1", hidden: true, edge: true),
                Is("Un millón", "Busqueda.Buscar(Enumerable.Range(0, 1000000).Select(i => i * 2).ToArray(), 1999998)", "999999", hidden: true, hint: "Cada llamada debe descartar la mitad del rango; así basta con unas 20 llamadas."),
            ],
            "Cada llamada descarta la mitad del rango: el caso base llega en unos log₂(n) pasos y la pila nunca crece demasiado.",
            ["Crea una sobrecarga privada con los límites izquierda y derecha.", "Caso base: izquierda > derecha → -1. Compara con a[medio] y llama con la mitad correspondiente."],
            difficulty: 3, requires: [CodeConstruct.Recursion], related: ["n1.complejidad", "n2.sobrecarga"], targets: [ErrorCategory.OffByOne]);

        return new Skill(s, UnitId, "Recursividad",
            "Caso base, caso recursivo y pila de llamadas.",
            Tier.Intermediate, ["n2.parametros", "n2.arrays"],
            new Lesson(s,
                "Escribir métodos recursivos con caso base, seguir la pila de llamadas y reconocer cuándo la recursión es ineficiente.",
                [
                    P("Un método recursivo se llama a sí mismo para resolver una versión más pequeña del mismo problema."),
                    P("Siempre tiene dos partes: un caso base que se resuelve directamente y un caso recursivo que se acerca al caso base."),
                    Code("""
                        Console.WriteLine(Factorial(5));

                        long Factorial(int n)
                        {
                            if (n <= 1) return 1;
                            return n * Factorial(n - 1);
                        }
                        """, "5! = 5 × 4!"),
                    P("Cada llamada pendiente espera en la pila de llamadas hasta que la siguiente devuelva su resultado. Cuando se llega al caso base, la pila se deshace multiplicando los resultados."),
                    Warn("Sin caso base, o si la llamada no se acerca a él, la pila se agota y el programa cae con StackOverflowException."),
                    Design("Usa recursión cuando el problema tiene estructura recursiva (árboles, dividir a la mitad). Para recorrer una lista, un bucle suele ser más claro y no consume pila."),
                ],
                new WorkedExample("Seguir la pila de llamadas",
                    """
                    Console.WriteLine(Suma([4, 7, 1], 0));

                    int Suma(int[] a, int desde)
                    {
                        if (desde == a.Length)
                        {
                            return 0;
                        }
                        int resto = Suma(a, desde + 1);
                        Console.WriteLine($"desde {desde}: {a[desde]} + {resto}");
                        return a[desde] + resto;
                    }
                    """,
                    [
                        Line(5, "Caso base: no quedan elementos, la suma es 0."),
                        Line(9, "Caso recursivo: la suma del resto del array."),
                        Line(10, "Esta línea se ejecuta al regresar, desde la llamada más profunda."),
                        Line(11, "Cada llamada suma su elemento al resultado del resto."),
                    ],
                    "desde 2: 1 + 0\ndesde 1: 7 + 1\ndesde 0: 4 + 8\n12"),
                new CommonMistake(
                    "Olvidar que la llamada debe acercarse al caso base.",
                    "int Contar(int n)\n{\n    if (n == 0) return 0;\n    return 1 + Contar(n);\n}",
                    "Contar(n) vuelve a llamarse con el mismo n: nunca llega a 0 y la pila se agota.",
                    "int Contar(int n)\n{\n    if (n == 0) return 0;\n    return 1 + Contar(n - 1);\n}\nConsole.WriteLine(Contar(3));"),
                "n2.recursion.pred1", "n2.recursion.guiado", "n2.recursion.indep", "n2.recursion.app",
                ["n2.recursion.recall1", "n2.recursion.recall2", "n2.recursion.pred2", "n2.recursion.concepto1"],
                "Escribes soluciones recursivas correctas con caso base, predices el orden de ejecución y eliges entre recursión y bucle.",
                20));
    }
}
