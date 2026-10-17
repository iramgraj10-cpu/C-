using CSharpMastery.Content.Infrastructure;
using CSharpMastery.Domain.Curriculum;
using CSharpMastery.Domain.Items;
using CSharpMastery.Domain.Progress;
using static CSharpMastery.Content.Infrastructure.ContentRegistry;
using static CSharpMastery.Content.Infrastructure.L;

namespace CSharpMastery.Content.Nivel2;

internal static class U02Colecciones
{
    public const string UnitId = "n2.u2";

    public static Unit Build(ContentRegistry r) => new(UnitId, "n2", "Colecciones",
        "Arrays, matrices, List<T>, Dictionary<TKey,TValue> y HashSet<T>.",
        [Arrays(r), Matrices(r), Listas(r), Diccionarios(r), Eleccion(r)]);

    private static Skill Arrays(ContentRegistry r)
    {
        const string s = "n2.arrays";
        r.Choice("n2.arrays.pred1", s, ItemPurpose.Prediction,
            "¿Qué ocurre al ejecutar este programa?",
            "El array tiene 3 elementos con índices 0, 1 y 2. a[a.Length - 1] es a[2] = 7. Después, a[3] no existe y se lanza IndexOutOfRangeException.",
            [
                Ok("Muestra 7 y luego lanza IndexOutOfRangeException", "Correcto: el último índice válido es Length - 1."),
                No("Muestra 7 y luego 0", "C# no devuelve un valor por defecto fuera del rango: lanza una excepción.", ErrorCategory.IndexOutOfRange),
                No("Muestra 5 y luego 7", "Los índices empiezan en 0: a[2] es el tercer elemento.", ErrorCategory.OffByOne),
            ],
            code: """
                int[] a = [3, 5, 7];
                Console.WriteLine(a[a.Length - 1]);
                Console.WriteLine(a[3]);
                """,
            targets: [ErrorCategory.IndexOutOfRange, ErrorCategory.OffByOne]);

        r.Choice("n2.arrays.pred2", s, ItemPurpose.Prediction,
            "¿Qué muestra este programa?",
            "Un array es un tipo por referencia: b = a no copia los elementos, hace que b apunte al mismo array. Cambiar b[0] cambia lo que ve a.",
            [
                Ok("99", "Correcto: a y b son dos nombres para el mismo array."),
                No("1", "b no es una copia: es el mismo array.", ErrorCategory.ConceptMisunderstanding),
                No("No compila", "Asignar un array a otra variable es válido.", ErrorCategory.ConceptMisunderstanding),
            ],
            code: """
                int[] a = [1, 2, 3];
                int[] b = a;
                b[0] = 99;
                Console.WriteLine(a[0]);
                """,
            difficulty: 2);

        r.Choice("n2.arrays.recall1", s, ItemPurpose.Recall,
            "Un array tiene Length = n. ¿Cuáles son su primer y su último índice?",
            "Los índices van de 0 a n - 1.",
            [
                Ok("0 y n - 1", "Correcto."),
                No("1 y n", "C# cuenta desde 0.", ErrorCategory.OffByOne),
                No("0 y n", "El índice n ya está fuera del array.", ErrorCategory.OffByOne),
            ],
            targets: [ErrorCategory.OffByOne]);

        r.Choice("n2.arrays.recall2", s, ItemPurpose.Recall,
            "¿Qué contiene un array recién creado con new int[4]?",
            "Cuatro elementos con el valor por defecto de int, que es 0. Su tamaño no puede cambiar después.",
            [
                Ok("Cuatro ceros.", "Correcto."),
                No("Nada: está vacío hasta que se agregan elementos.", "Un array tiene tamaño fijo y todos sus elementos existen desde que se crea."),
                No("Valores aleatorios.", "C# inicializa siempre con el valor por defecto del tipo."),
            ]);

        r.Choice("n2.arrays.bug1", s, ItemPurpose.BugFinding,
            "Este bucle lanza IndexOutOfRangeException. ¿Dónde está el error?",
            "La condición i <= numeros.Length deja que i llegue a Length, que ya no es un índice válido. Debe ser i < numeros.Length.",
            [
                Ok("La condición debe ser i < numeros.Length.", "Correcto: es un error por uno."),
                No("El índice debe empezar en 1.", "Los índices empiezan en 0.", ErrorCategory.OffByOne),
                No("suma debe ser double.", "El tipo de suma no causa la excepción.", ErrorCategory.TypeMismatch),
            ],
            code: """
                int[] numeros = [4, 8, 15];
                int suma = 0;
                for (int i = 0; i <= numeros.Length; i++)
                {
                    suma += numeros[i];
                }
                """,
            targets: [ErrorCategory.OffByOne, ErrorCategory.IndexOutOfRange]);

        r.Code("n2.arrays.guiado", s, ItemPurpose.Guided, CodeMode.Function,
            "Completa Promedio para que devuelva el promedio de los elementos del array. Si el array está vacío, devuelve 0.",
            """
            public static class Arreglos
            {
                public static double Promedio(int[] numeros)
                {
                    if (numeros.Length == 0)
                    {
                        return 0;
                    }
                    int suma = 0;
                    // recorre el array y acumula en suma
                    return suma / numeros.Length;
                }
            }
            """,
            """
            public static class Arreglos
            {
                public static double Promedio(int[] numeros)
                {
                    if (numeros.Length == 0)
                    {
                        return 0;
                    }
                    int suma = 0;
                    foreach (int n in numeros)
                    {
                        suma += n;
                    }
                    return (double)suma / numeros.Length;
                }
            }
            """,
            [
                Is("Exacto", "Arreglos.Promedio([2, 4, 6])", "4"),
                Is("Con decimales", "Arreglos.Promedio([1, 2])", "1.5", hint: "Si divides dos int el resultado es int: convierte la suma a double."),
                Is("Vacío", "Arreglos.Promedio([])", "0", hidden: true, edge: true),
            ],
            "La conversión (double)suma evita la división entera.",
            ["Usa foreach (int n in numeros) para sumar.", "Convierte antes de dividir: (double)suma / numeros.Length."],
            requires: [CodeConstruct.Loop], related: ["n0.aritmetica"], targets: [ErrorCategory.TypeMismatch]);

        r.Code("n2.arrays.indep", s, ItemPurpose.Independent, CodeMode.Function,
            "Escribe Invertido(int[] a) que devuelva un array NUEVO con los elementos en orden inverso, sin modificar el original y sin usar Array.Reverse.",
            """
            public static class Arreglos
            {
                public static int[] Invertido(int[] a)
                {
                    return a;
                }
            }
            """,
            """
            public static class Arreglos
            {
                public static int[] Invertido(int[] a)
                {
                    int[] r = new int[a.Length];
                    for (int i = 0; i < a.Length; i++)
                    {
                        r[i] = a[a.Length - 1 - i];
                    }
                    return r;
                }
            }
            """,
            [
                Is("Tres", "Arreglos.Invertido([1, 2, 3])", "[3, 2, 1]"),
                Is("Uno", "Arreglos.Invertido([7])", "[7]", edge: true),
                Is("Vacío", "Arreglos.Invertido([])", "[]", hidden: true, edge: true),
                Is("No modifica el original", "{ int[] o = [1, 2]; Arreglos.Invertido(o); return o; }", "[1, 2]", hidden: true, hint: "Crea un array nuevo con new int[a.Length]; no cambies a."),
            ],
            "El elemento i del resultado es el elemento Length - 1 - i del original.",
            ["Crea el resultado con new int[a.Length].", "r[i] = a[a.Length - 1 - i];"],
            difficulty: 2, requires: [CodeConstruct.Loop], targets: [ErrorCategory.OffByOne]);

        r.Code("n2.arrays.app", s, ItemPurpose.Transfer, CodeMode.Program,
            "Aplicación: temperaturas de la semana. Lee una línea con temperaturas enteras separadas por espacios. Muestra «Mínima: X», «Máxima: Y», «Promedio: Z» (un decimal) y «Días sobre el promedio: N».",
            """
            string[] partes = Console.ReadLine()!.Split(' ');
            """,
            """
            string[] partes = Console.ReadLine()!.Split(' ');
            int[] t = new int[partes.Length];
            for (int i = 0; i < partes.Length; i++)
            {
                t[i] = int.Parse(partes[i]);
            }

            int min = t[0], max = t[0], suma = 0;
            foreach (int x in t)
            {
                if (x < min) min = x;
                if (x > max) max = x;
                suma += x;
            }
            double promedio = (double)suma / t.Length;

            int sobre = 0;
            foreach (int x in t)
            {
                if (x > promedio) sobre++;
            }

            Console.WriteLine($"Mínima: {min}");
            Console.WriteLine($"Máxima: {max}");
            Console.WriteLine($"Promedio: {promedio:F1}");
            Console.WriteLine($"Días sobre el promedio: {sobre}");
            """,
            [
                In("Semana", "20 22 19 25 24 18 21", "Mínima: 18\nMáxima: 25\nPromedio: 21.3\nDías sobre el promedio: 3"),
                In("Un día", "15", "Mínima: 15\nMáxima: 15\nPromedio: 15.0\nDías sobre el promedio: 0", hidden: true, edge: true),
                In("Negativas", "-3 -1 -5 2", "Mínima: -5\nMáxima: 2\nPromedio: -1.8\nDías sobre el promedio: 2", hidden: true, edge: true),
            ],
            "Contar los días sobre el promedio exige un segundo recorrido: el promedio solo se conoce al final del primero. Para eso sirve guardar los datos en un array.",
            ["Primero convierte las partes a un int[].", "Calcula mínimo, máximo y suma en un recorrido; cuenta los días sobre el promedio en otro."],
            difficulty: 3, requires: [CodeConstruct.Loop], related: ["n1.acumuladores"]);

        return new Skill(s, UnitId, "Arrays",
            "Colecciones de tamaño fijo con índices.",
            Tier.Essential, ["n1.for"],
            new Lesson(s,
                "Crear arrays, acceder por índice sin salirse del rango, recorrerlos y entender que son tipos por referencia.",
                [
                    P("Un array guarda varios valores del mismo tipo en posiciones numeradas. Su tamaño se fija al crearlo y no cambia."),
                    Code("""
                        int[] edades = [18, 25, 31];
                        string[] nombres = new string[2];
                        nombres[0] = "Ana";
                        nombres[1] = "Luis";
                        Console.WriteLine(edades[0] + " " + nombres[1] + " " + edades.Length);
                        """, "Dos formas de crear un array"),
                    P("Los índices empiezan en 0: el primer elemento es a[0] y el último a[a.Length - 1]. Acceder fuera de ese rango lanza IndexOutOfRangeException."),
                    P("for es útil cuando necesitas el índice; foreach cuando solo necesitas cada valor."),
                    Warn("Un array es un tipo por referencia: int[] b = a; no copia los datos. Ambas variables apuntan al mismo array."),
                    Tip("[1, 2, 3] es una expresión de colección (C# 12). También verás new int[] { 1, 2, 3 } en código más antiguo."),
                ],
                new WorkedExample("Buscar la posición del mayor",
                    """
                    int[] ventas = [120, 340, 90, 340, 200];
                    int posicion = 0;
                    for (int i = 1; i < ventas.Length; i++)
                    {
                        if (ventas[i] > ventas[posicion])
                        {
                            posicion = i;
                        }
                    }
                    Console.WriteLine($"Mayor venta: {ventas[posicion]} el día {posicion + 1}");
                    """,
                    [
                        Line(2, "Guardamos la posición, no el valor: así sabemos ambos."),
                        Line(3, "Empezamos en 1 porque el elemento 0 ya es el candidato."),
                        Line(5, "Con > (no >=) nos quedamos con la primera aparición del máximo."),
                        Line(10, "Los días se cuentan desde 1 para las personas: posicion + 1."),
                    ],
                    "Mayor venta: 340 el día 2"),
                new CommonMistake(
                    "Recorrer hasta i <= Length.",
                    "for (int i = 0; i <= a.Length; i++)\n{\n    Console.WriteLine(a[i]);\n}",
                    "En la última vuelta i vale a.Length, que no es un índice válido: IndexOutOfRangeException.",
                    "int[] a = [1, 2, 3];\nfor (int i = 0; i < a.Length; i++)\n{\n    Console.WriteLine(a[i]);\n}"),
                "n2.arrays.pred1", "n2.arrays.guiado", "n2.arrays.indep", "n2.arrays.app",
                ["n2.arrays.recall1", "n2.arrays.recall2", "n2.arrays.pred2"],
                "Recorres arrays sin errores de índice, eliges entre for y foreach, y predices el efecto de compartir referencias.",
                15));
    }

    private static Skill Matrices(ContentRegistry r)
    {
        const string s = "n2.matrices";
        r.Short("n2.matrices.pred1", s, ItemPurpose.Prediction,
            "¿Qué muestra este programa?",
            "La matriz tiene 2 filas (dimensión 0) y 3 columnas (dimensión 1). m[1, 2] es la fila 1, columna 2: el 6.",
            ["2 3 6"], match: AnswerMatch.Output,
            code: """
                int[,] m =
                {
                    { 1, 2, 3 },
                    { 4, 5, 6 },
                };
                Console.WriteLine($"{m.GetLength(0)} {m.GetLength(1)} {m[1, 2]}");
                """,
            wrong: new() { ["3 2 6"] = "GetLength(0) es el número de filas y GetLength(1) el de columnas." });

        r.Choice("n2.matrices.concepto1", s, ItemPurpose.Concept,
            "¿Qué diferencia hay entre int[,] y int[][]?",
            "int[,] es una matriz rectangular: todas las filas tienen el mismo largo. int[][] es un array de arrays (escalonado): cada fila es un array independiente y puede tener otro largo.",
            [
                Ok("int[,] es rectangular; int[][] es un array de arrays cuyas filas pueden tener distinto largo.", "Correcto."),
                No("Son dos formas de escribir lo mismo.", "Se crean, se recorren y se indexan de forma distinta: m[i, j] frente a m[i][j]."),
                No("int[][] solo puede tener dos filas.", "Puede tener cualquier cantidad de filas."),
            ]);

        r.Choice("n2.matrices.recall1", s, ItemPurpose.Recall,
            "En una matriz m de tipo int[,], ¿qué devuelve m.GetLength(1)?",
            "GetLength(0) es el número de filas y GetLength(1) el de columnas. m.Length es el total de elementos.",
            [
                Ok("El número de columnas.", "Correcto."),
                No("El número de filas.", "Ese es GetLength(0)."),
                No("El número total de elementos.", "Ese es m.Length."),
            ]);

        r.Choice("n2.matrices.recall2", s, ItemPurpose.Recall,
            "Para recorrer todos los elementos de una matriz con índices, ¿qué estructura se usa normalmente?",
            "Dos for anidados: el exterior recorre filas y el interior columnas.",
            [
                Ok("Dos bucles for anidados: filas afuera, columnas adentro.", "Correcto."),
                No("Un solo for hasta m.Length.", "Con un índice no puedes escribir m[i, j]."),
                No("Un while que se detiene al encontrar un 0.", "Las matrices no terminan en un valor especial."),
            ]);

        r.Choice("n2.matrices.bug1", s, ItemPurpose.BugFinding,
            "Con una matriz de 2 filas y 3 columnas este código lanza IndexOutOfRangeException. ¿Por qué?",
            "Los límites están intercambiados: i recorre filas pero se limita con GetLength(1) (3), y j recorre columnas pero se limita con GetLength(0). Con i = 2 se sale de las filas.",
            [
                Ok("Los límites GetLength(0) y GetLength(1) están intercambiados.", "Correcto."),
                No("Las matrices se indexan con m[i][j].", "Esa sintaxis es para arrays escalonados (int[][]).", ErrorCategory.Syntax),
                No("Los bucles deberían empezar en 1.", "Los índices empiezan en 0.", ErrorCategory.OffByOne),
            ],
            code: """
                int[,] m = { { 1, 2, 3 }, { 4, 5, 6 } };
                for (int i = 0; i < m.GetLength(1); i++)
                {
                    for (int j = 0; j < m.GetLength(0); j++)
                    {
                        Console.Write(m[i, j]);
                    }
                }
                """,
            targets: [ErrorCategory.IndexOutOfRange]);

        r.Code("n2.matrices.guiado", s, ItemPurpose.Guided, CodeMode.Function,
            "Completa SumaFila para que devuelva la suma de los elementos de la fila indicada.",
            """
            public static class Matrices
            {
                public static int SumaFila(int[,] m, int fila)
                {
                    int suma = 0;
                    // recorre las columnas de esa fila
                    return suma;
                }
            }
            """,
            """
            public static class Matrices
            {
                public static int SumaFila(int[,] m, int fila)
                {
                    int suma = 0;
                    for (int j = 0; j < m.GetLength(1); j++)
                    {
                        suma += m[fila, j];
                    }
                    return suma;
                }
            }
            """,
            [
                Is("Fila 0", "Matrices.SumaFila(new int[,] { { 1, 2, 3 }, { 4, 5, 6 } }, 0)", "6"),
                Is("Fila 1", "Matrices.SumaFila(new int[,] { { 1, 2, 3 }, { 4, 5, 6 } }, 1)", "15"),
                Is("Una columna", "Matrices.SumaFila(new int[,] { { 7 }, { 9 } }, 1)", "9", hidden: true, edge: true),
            ],
            "La fila queda fija; solo varía la columna j.",
            ["Recorre j desde 0 hasta m.GetLength(1).", "Suma m[fila, j]."],
            requires: [CodeConstruct.Loop]);

        r.Code("n2.matrices.indep", s, ItemPurpose.Independent, CodeMode.Function,
            "Escribe Transponer(int[,] m) que devuelva una matriz nueva donde las filas sean las columnas de m (el elemento [i, j] pasa a [j, i]).",
            """
            public static class Matrices
            {
                public static int[,] Transponer(int[,] m)
                {
                    return m;
                }
            }
            """,
            """
            public static class Matrices
            {
                public static int[,] Transponer(int[,] m)
                {
                    int filas = m.GetLength(0);
                    int columnas = m.GetLength(1);
                    int[,] t = new int[columnas, filas];
                    for (int i = 0; i < filas; i++)
                    {
                        for (int j = 0; j < columnas; j++)
                        {
                            t[j, i] = m[i, j];
                        }
                    }
                    return t;
                }
            }
            """,
            [
                Is("2×3", "{ var t = Matrices.Transponer(new int[,] { { 1, 2, 3 }, { 4, 5, 6 } }); return $\"{t.GetLength(0)}x{t.GetLength(1)} {t[0, 1]} {t[2, 0]}\"; }", "3x2 4 3"),
                Is("Cuadrada", "{ var t = Matrices.Transponer(new int[,] { { 1, 2 }, { 3, 4 } }); return $\"{t[0, 1]} {t[1, 0]}\"; }", "3 2"),
                Is("Una fila", "{ var t = Matrices.Transponer(new int[,] { { 5, 6, 7 } }); return $\"{t.GetLength(0)}x{t.GetLength(1)} {t[2, 0]}\"; }", "3x1 7", hidden: true, edge: true),
            ],
            "La matriz resultado tiene las dimensiones intercambiadas: columnas × filas.",
            ["Crea t con new int[columnas, filas].", "Dentro de los dos bucles: t[j, i] = m[i, j];"],
            difficulty: 2, requires: [CodeConstruct.Loop], targets: [ErrorCategory.IndexOutOfRange]);

        r.Code("n2.matrices.app", s, ItemPurpose.Transfer, CodeMode.Program,
            "Aplicación: gato (tres en raya). Lee 3 líneas de 3 caracteres cada una (X, O o .). Muestra «Gana X», «Gana O» o «Sin ganador», revisando filas, columnas y las dos diagonales.",
            """
            char[,] tablero = new char[3, 3];
            """,
            """
            char[,] tablero = new char[3, 3];
            for (int i = 0; i < 3; i++)
            {
                string fila = Console.ReadLine()!;
                for (int j = 0; j < 3; j++)
                {
                    tablero[i, j] = fila[j];
                }
            }

            char ganador = '.';
            for (int i = 0; i < 3; i++)
            {
                if (tablero[i, 0] != '.' && tablero[i, 0] == tablero[i, 1] && tablero[i, 1] == tablero[i, 2]) ganador = tablero[i, 0];
                if (tablero[0, i] != '.' && tablero[0, i] == tablero[1, i] && tablero[1, i] == tablero[2, i]) ganador = tablero[0, i];
            }
            if (tablero[1, 1] != '.' && tablero[0, 0] == tablero[1, 1] && tablero[1, 1] == tablero[2, 2]) ganador = tablero[1, 1];
            if (tablero[1, 1] != '.' && tablero[0, 2] == tablero[1, 1] && tablero[1, 1] == tablero[2, 0]) ganador = tablero[1, 1];

            Console.WriteLine(ganador == '.' ? "Sin ganador" : $"Gana {ganador}");
            """,
            [
                In("Fila", "XXX\nO.O\n...", "Gana X"),
                In("Columna", "OX.\nOX.\nO..", "Gana O"),
                In("Diagonal", "X.O\n.XO\n..X", "Gana X", hidden: true),
                In("Antidiagonal", "X.O\nXO.\nO..", "Gana O", hidden: true),
                In("Vacío", "...\n...\n...", "Sin ganador", hidden: true, edge: true, hint: "Tres puntos iguales no son una victoria."),
                In("Empate", "XOX\nXOO\nOXX", "Sin ganador", hidden: true),
            ],
            "Una matriz modela el tablero tal como lo piensas: fila y columna.",
            ["Llena la matriz con dos bucles: tablero[i, j] = fila[j].", "Comprueba que la casilla no sea '.' además de que las tres sean iguales."],
            difficulty: 3, requires: [CodeConstruct.Loop], related: ["n1.booleanos"], targets: [ErrorCategory.EdgeCaseMissed]);

        return new Skill(s, UnitId, "Matrices (arrays bidimensionales)",
            "Datos en filas y columnas.",
            Tier.Intermediate, ["n2.arrays"],
            new Lesson(s,
                "Crear y recorrer matrices rectangulares, distinguir filas de columnas y conocer los arrays escalonados.",
                [
                    P("Una matriz int[,] organiza datos en filas y columnas. Se accede con dos índices: m[fila, columna]."),
                    Code("""
                        int[,] asientos = new int[2, 4];
                        asientos[1, 3] = 1;
                        for (int f = 0; f < asientos.GetLength(0); f++)
                        {
                            for (int c = 0; c < asientos.GetLength(1); c++)
                            {
                                Console.Write(asientos[f, c] == 1 ? "X" : "_");
                            }
                            Console.WriteLine();
                        }
                        """, "Mapa de asientos de 2 filas por 4 columnas"),
                    P("GetLength(0) da el número de filas y GetLength(1) el de columnas. Length es el total de elementos."),
                    P("Un array escalonado int[][] es un array cuyos elementos son arrays; cada fila puede tener un largo distinto y se indexa con m[i][j]."),
                    Tip("Nombra los índices f y c (o fila y columna) en lugar de i y j cuando ayude a no confundirlos."),
                ],
                new WorkedExample("Sumar cada columna",
                    """
                    int[,] ventas =
                    {
                        { 10, 20, 30 },
                        { 5, 15, 25 },
                    };
                    for (int c = 0; c < ventas.GetLength(1); c++)
                    {
                        int suma = 0;
                        for (int f = 0; f < ventas.GetLength(0); f++)
                        {
                            suma += ventas[f, c];
                        }
                        Console.WriteLine($"Producto {c + 1}: {suma}");
                    }
                    """,
                    [
                        Line(1, "Cada fila es una sucursal; cada columna, un producto."),
                        Line(6, "Para sumar columnas, el bucle exterior recorre columnas."),
                        Line(8, "La suma se reinicia por cada columna."),
                        Line(11, "Fila variable, columna fija."),
                    ],
                    "Producto 1: 15\nProducto 2: 35\nProducto 3: 55"),
                new CommonMistake(
                    "Usar m.Length como límite de las filas.",
                    "for (int f = 0; f < m.Length; f++)\n    for (int c = 0; c < m.GetLength(1); c++)\n        Console.Write(m[f, c]);",
                    "m.Length es el total de elementos (filas × columnas), no el número de filas: se sale del rango.",
                    "int[,] m = { { 1, 2 }, { 3, 4 } };\nfor (int f = 0; f < m.GetLength(0); f++)\n    for (int c = 0; c < m.GetLength(1); c++)\n        Console.Write(m[f, c]);"),
                "n2.matrices.pred1", "n2.matrices.guiado", "n2.matrices.indep", "n2.matrices.app",
                ["n2.matrices.recall1", "n2.matrices.recall2", "n2.matrices.concepto1"],
                "Recorres matrices por filas o por columnas sin confundir dimensiones y eliges entre matriz rectangular y array escalonado.",
                15));
    }

    private static Skill Listas(ContentRegistry r)
    {
        const string s = "n2.listas";
        r.Short("n2.listas.pred1", s, ItemPurpose.Prediction,
            "¿Qué muestra este programa?",
            "Remove(\"b\") quita la b: [a, c]. Add agrega al final: [a, c, d]. Insert(0, \"z\") inserta al inicio: [z, a, c, d]. Hay 4 elementos.",
            ["z,a,c,d 4"], match: AnswerMatch.Output,
            code: """
                List<string> l = ["a", "b", "c"];
                l.Remove("b");
                l.Add("d");
                l.Insert(0, "z");
                Console.WriteLine(string.Join(",", l) + " " + l.Count);
                """);

        r.Choice("n2.listas.concepto1", s, ItemPurpose.Concept,
            "¿Cuándo conviene List<T> en lugar de un array?",
            "Cuando no sabes de antemano cuántos elementos habrá o necesitas agregar y quitar. Un array tiene tamaño fijo.",
            [
                Ok("Cuando la cantidad de elementos cambia durante el programa.", "Correcto."),
                No("Siempre: los arrays están obsoletos.", "Los arrays siguen siendo útiles cuando el tamaño es fijo."),
                No("Cuando los elementos son de distintos tipos.", "List<T> también exige un único tipo T."),
            ],
            dimension: KnowledgeDimension.Design);

        r.Choice("n2.listas.recall1", s, ItemPurpose.Recall,
            "¿Cómo se obtiene la cantidad de elementos de una List<T>?",
            "Las listas usan Count; los arrays, Length.",
            [
                Ok("Con la propiedad Count.", "Correcto."),
                No("Con la propiedad Length.", "Length es de los arrays y strings."),
                No("Con el método Size().", "No existe en C#."),
            ]);

        r.Choice("n2.listas.recall2", s, ItemPurpose.Recall,
            "¿Qué diferencia hay entre lista.Remove(x) y lista.RemoveAt(i)?",
            "Remove busca el valor x y quita su primera aparición. RemoveAt quita el elemento que está en la posición i.",
            [
                Ok("Remove quita por valor; RemoveAt quita por posición.", "Correcto."),
                No("Son iguales.", "Con una List<int> la diferencia es muy importante: Remove(2) no es RemoveAt(2)."),
                No("Remove quita todas las apariciones.", "Solo quita la primera; para todas existe RemoveAll."),
            ]);

        r.Choice("n2.listas.bug1", s, ItemPurpose.BugFinding,
            "Este código quiere quitar los números negativos, pero lanza InvalidOperationException. ¿Por qué?",
            "No se puede modificar una colección mientras se recorre con foreach. Usa un for que vaya de atrás hacia adelante, o RemoveAll(n => n < 0).",
            [
                Ok("Se modifica la lista mientras se recorre con foreach.", "Correcto."),
                No("Remove no acepta números negativos.", "Remove acepta cualquier valor.", ErrorCategory.ConceptMisunderstanding),
                No("Falta convertir n a int.", "n ya es int.", ErrorCategory.TypeMismatch),
            ],
            code: """
                List<int> numeros = [3, -1, 4, -5];
                foreach (int n in numeros)
                {
                    if (n < 0) numeros.Remove(n);
                }
                """,
            targets: [ErrorCategory.UnhandledException]);

        r.Code("n2.listas.guiado", s, ItemPurpose.Guided, CodeMode.Function,
            "Completa Pares para que devuelva una lista nueva solo con los números pares, en el mismo orden.",
            """
            public static class Listas
            {
                public static List<int> Pares(List<int> numeros)
                {
                    List<int> resultado = [];
                    // agrega a resultado los pares
                    return resultado;
                }
            }
            """,
            """
            public static class Listas
            {
                public static List<int> Pares(List<int> numeros)
                {
                    List<int> resultado = [];
                    foreach (int n in numeros)
                    {
                        if (n % 2 == 0)
                        {
                            resultado.Add(n);
                        }
                    }
                    return resultado;
                }
            }
            """,
            [
                Is("Mezclados", "Listas.Pares([1, 2, 3, 4])", "[2, 4]"),
                Is("Sin pares", "Listas.Pares([1, 3])", "[]", edge: true),
                Is("Negativos", "Listas.Pares([-2, -3, 0])", "[-2, 0]", hidden: true, edge: true),
            ],
            "Construir una lista nueva evita modificar la que se está recorriendo.",
            ["Recorre numeros con foreach.", "Si n % 2 == 0, usa resultado.Add(n)."],
            requires: [CodeConstruct.Loop]);

        r.Code("n2.listas.indep", s, ItemPurpose.Independent, CodeMode.Function,
            "Escribe SinDuplicados(List<string> palabras) que devuelva una lista nueva con cada palabra una sola vez, en el orden de su primera aparición.",
            """
            public static class Listas
            {
                public static List<string> SinDuplicados(List<string> palabras)
                {
                    return palabras;
                }
            }
            """,
            """
            public static class Listas
            {
                public static List<string> SinDuplicados(List<string> palabras)
                {
                    List<string> resultado = [];
                    foreach (string p in palabras)
                    {
                        if (!resultado.Contains(p))
                        {
                            resultado.Add(p);
                        }
                    }
                    return resultado;
                }
            }
            """,
            [
                Is("Repetidas", "Listas.SinDuplicados([\"a\", \"b\", \"a\", \"c\", \"b\"])", "[\"a\", \"b\", \"c\"]"),
                Is("Sin repetir", "Listas.SinDuplicados([\"x\", \"y\"])", "[\"x\", \"y\"]"),
                Is("Vacía", "Listas.SinDuplicados([])", "[]", hidden: true, edge: true),
                Is("Mayúsculas distintas", "Listas.SinDuplicados([\"Sol\", \"sol\"])", "[\"Sol\", \"sol\"]", hidden: true, edge: true, hint: "\"Sol\" y \"sol\" son textos distintos."),
            ],
            "Contains recorre la lista: está bien para listas pequeñas. En la habilidad «Elegir la estructura» verás una opción más rápida.",
            ["Crea una lista vacía para el resultado.", "Agrega cada palabra solo si resultado.Contains(p) es false."],
            difficulty: 2, requires: [CodeConstruct.Loop]);

        r.Code("n2.listas.app", s, ItemPurpose.Transfer, CodeMode.Program,
            "Aplicación: lista de pendientes. Lee comandos hasta «fin»: «agregar TEXTO» añade un pendiente; «hecho N» quita el pendiente número N (contando desde 1) y muestra «Completado: TEXTO», o «No existe» si N no es válido; «listar» muestra cada pendiente como «N. TEXTO» o «Sin pendientes».",
            """
            string comando = Console.ReadLine()!;
            """,
            """
            List<string> pendientes = [];
            string comando = Console.ReadLine()!;
            while (comando != "fin")
            {
                if (comando.StartsWith("agregar "))
                {
                    pendientes.Add(comando.Substring(8));
                }
                else if (comando.StartsWith("hecho "))
                {
                    if (int.TryParse(comando.Substring(6), out int n) && n >= 1 && n <= pendientes.Count)
                    {
                        Console.WriteLine($"Completado: {pendientes[n - 1]}");
                        pendientes.RemoveAt(n - 1);
                    }
                    else
                    {
                        Console.WriteLine("No existe");
                    }
                }
                else if (comando == "listar")
                {
                    if (pendientes.Count == 0)
                    {
                        Console.WriteLine("Sin pendientes");
                    }
                    for (int i = 0; i < pendientes.Count; i++)
                    {
                        Console.WriteLine($"{i + 1}. {pendientes[i]}");
                    }
                }
                comando = Console.ReadLine()!;
            }
            """,
            [
                In("Básico", "agregar pan\nagregar leche\nlistar\nfin", "1. pan\n2. leche"),
                In("Completar", "agregar a\nagregar b\nagregar c\nhecho 2\nlistar\nfin", "Completado: b\n1. a\n2. c"),
                In("Número inválido", "agregar a\nhecho 3\nhecho 0\nhecho x\nfin", "No existe\nNo existe\nNo existe", hidden: true, edge: true, hint: "Valida que N esté entre 1 y Count antes de usarlo."),
                In("Vacía", "listar\nagregar a\nhecho 1\nlistar\nfin", "Sin pendientes\nCompletado: a\nSin pendientes", hidden: true, edge: true),
            ],
            "Las personas cuentan desde 1 y la lista desde 0: la conversión n - 1 debe hacerse en un solo lugar y validarse.",
            ["StartsWith(\"agregar \") y Substring(8) separan el comando del texto.", "Antes de RemoveAt(n - 1) comprueba que 1 <= n <= Count."],
            difficulty: 3, requires: [CodeConstruct.Loop], related: ["n1.validacion"], targets: [ErrorCategory.OffByOne, ErrorCategory.EdgeCaseMissed]);

        return new Skill(s, UnitId, "List<T>",
            "Colecciones que crecen y se reducen.",
            Tier.Essential, ["n2.arrays"],
            new Lesson(s,
                "Usar List<T> para colecciones de tamaño variable: agregar, insertar, quitar, buscar y recorrer sin modificarla durante un foreach.",
                [
                    P("List<T> es una colección que crece y se reduce. T es el tipo de los elementos: List<int>, List<string>…"),
                    Code("""
                        List<string> compras = [];
                        compras.Add("pan");
                        compras.Add("café");
                        compras.Insert(0, "leche");
                        Console.WriteLine(compras.Count);
                        Console.WriteLine(compras[0]);
                        Console.WriteLine(compras.Contains("café"));
                        compras.RemoveAt(1);
                        Console.WriteLine(string.Join(", ", compras));
                        """, "Operaciones básicas"),
                    P("Se indexa igual que un array (lista[0]) y su tamaño está en Count. Remove(x) quita por valor; RemoveAt(i) quita por posición."),
                    Warn("No agregues ni quites elementos de una lista mientras la recorres con foreach: se lanza InvalidOperationException."),
                    Tip("string.Join(\", \", lista) es una forma rápida de mostrar todos los elementos."),
                ],
                new WorkedExample("Filtrar sin modificar durante el recorrido",
                    """
                    List<int> notas = [8, 4, 10, 5, 7];
                    List<int> reprobadas = [];
                    foreach (int n in notas)
                    {
                        if (n < 6) reprobadas.Add(n);
                    }
                    for (int i = notas.Count - 1; i >= 0; i--)
                    {
                        if (notas[i] < 6) notas.RemoveAt(i);
                    }
                    Console.WriteLine($"Aprobadas: {string.Join(" ", notas)}");
                    Console.WriteLine($"Reprobadas: {reprobadas.Count}");
                    """,
                    [
                        Line(2, "Una lista nueva para el resultado del filtro."),
                        Line(7, "Para quitar, se recorre de atrás hacia adelante con for."),
                        Line(9, "Quitar el elemento i no altera los índices que faltan por visitar."),
                        Line(11, "string.Join une los elementos con un separador."),
                    ],
                    "Aprobadas: 8 10 7\nReprobadas: 2"),
                new CommonMistake(
                    "Quitar elementos dentro de un foreach.",
                    "foreach (int n in numeros)\n{\n    if (n < 0) numeros.Remove(n);\n}",
                    "foreach detecta que la colección cambió y lanza InvalidOperationException.",
                    "List<int> numeros = [3, -1, 4];\nfor (int i = numeros.Count - 1; i >= 0; i--)\n{\n    if (numeros[i] < 0) numeros.RemoveAt(i);\n}"),
                "n2.listas.pred1", "n2.listas.guiado", "n2.listas.indep", "n2.listas.app",
                ["n2.listas.recall1", "n2.listas.recall2", "n2.listas.concepto1"],
                "Manipulas listas sin errores de índice ni modificaciones durante el recorrido.",
                15));
    }

    private static Skill Diccionarios(ContentRegistry r)
    {
        const string s = "n2.diccionarios";
        r.Short("n2.diccionarios.pred1", s, ItemPurpose.Prediction,
            "¿Qué muestra este programa?",
            "Cada palabra es una clave y su valor es cuántas veces apareció. «sol» aparece 2 veces, «mar» 1. El orden de un Dictionary no está garantizado, por eso se consulta por clave.",
            ["2 1"], match: AnswerMatch.Output,
            code: """
                string[] palabras = ["sol", "mar", "sol"];
                Dictionary<string, int> cuenta = [];
                foreach (string p in palabras)
                {
                    cuenta[p] = cuenta.GetValueOrDefault(p) + 1;
                }
                Console.WriteLine($"{cuenta["sol"]} {cuenta["mar"]}");
                """,
            difficulty: 2);

        r.Choice("n2.diccionarios.pred2", s, ItemPurpose.Prediction,
            "¿Qué ocurre al ejecutar este programa?",
            "Leer con el indexador una clave que no existe lanza KeyNotFoundException. Para consultar sin riesgo se usa TryGetValue o ContainsKey.",
            [
                Ok("Se lanza KeyNotFoundException", "Correcto."),
                No("Muestra 0", "El indexador no devuelve un valor por defecto al leer: eso hace GetValueOrDefault.", ErrorCategory.ConceptMisunderstanding),
                No("Muestra una línea vacía", "No se muestra nada: se lanza una excepción.", ErrorCategory.ConceptMisunderstanding),
            ],
            code: """
                Dictionary<string, int> stock = new() { ["pan"] = 3 };
                Console.WriteLine(stock["leche"]);
                """,
            targets: [ErrorCategory.UnhandledException]);

        r.Choice("n2.diccionarios.recall1", s, ItemPurpose.Recall,
            "¿Qué hace dic.TryGetValue(clave, out var valor)?",
            "Devuelve true si la clave existe (y deja su valor en valor) o false si no existe, sin lanzar excepción.",
            [
                Ok("Devuelve si la clave existe y, si existe, deja su valor en la variable out.", "Correcto."),
                No("Agrega la clave si no existe.", "Solo consulta; no modifica el diccionario."),
                No("Lanza una excepción si la clave no existe.", "Ese es el comportamiento del indexador al leer."),
            ]);

        r.Choice("n2.diccionarios.recall2", s, ItemPurpose.Recall,
            "¿Qué devuelve conjunto.Add(x) en un HashSet<T> si x ya estaba?",
            "Devuelve false y el conjunto no cambia: un HashSet no admite duplicados.",
            [
                Ok("false, y el conjunto no cambia.", "Correcto."),
                No("Lanza una excepción.", "Eso hace Dictionary.Add con una clave repetida, no HashSet.Add."),
                No("true, y queda dos veces.", "Un conjunto nunca guarda duplicados."),
            ]);

        r.Choice("n2.diccionarios.bug1", s, ItemPurpose.BugFinding,
            "Este conteo lanza KeyNotFoundException con la primera palabra. ¿Por qué?",
            "cuenta[p]++ primero lee cuenta[p], y la clave aún no existe. Hay que inicializarla: cuenta[p] = cuenta.GetValueOrDefault(p) + 1, o comprobar con ContainsKey.",
            [
                Ok("cuenta[p]++ intenta leer una clave que todavía no existe.", "Correcto."),
                No("Los diccionarios no aceptan string como clave.", "string es la clave más común.", ErrorCategory.ConceptMisunderstanding),
                No("Hay que usar Add en lugar del indexador siempre.", "Add lanzaría excepción la segunda vez que aparezca la palabra.", ErrorCategory.ConceptMisunderstanding),
            ],
            code: """
                Dictionary<string, int> cuenta = [];
                foreach (string p in "uno dos uno".Split(' '))
                {
                    cuenta[p]++;
                }
                """,
            targets: [ErrorCategory.UnhandledException]);

        r.Code("n2.diccionarios.guiado", s, ItemPurpose.Guided, CodeMode.Function,
            "Completa Frecuencias para que devuelva cuántas veces aparece cada carácter del texto (ignora los espacios).",
            """
            public static class Conteo
            {
                public static Dictionary<char, int> Frecuencias(string texto)
                {
                    Dictionary<char, int> f = [];
                    foreach (char c in texto)
                    {
                        // si no es espacio, suma 1 a f[c]
                    }
                    return f;
                }
            }
            """,
            """
            public static class Conteo
            {
                public static Dictionary<char, int> Frecuencias(string texto)
                {
                    Dictionary<char, int> f = [];
                    foreach (char c in texto)
                    {
                        if (c != ' ')
                        {
                            f[c] = f.GetValueOrDefault(c) + 1;
                        }
                    }
                    return f;
                }
            }
            """,
            [
                Is("Palabra", "Conteo.Frecuencias(\"abca\")", "{'a': 2, 'b': 1, 'c': 1}"),
                Is("Con espacios", "Conteo.Frecuencias(\"a a\")", "{'a': 2}"),
                Is("Vacío", "Conteo.Frecuencias(\"\").Count", "0", hidden: true, edge: true),
            ],
            "GetValueOrDefault devuelve 0 para una clave nueva, así que la misma línea sirve para la primera vez y las siguientes.",
            ["f[c] = f.GetValueOrDefault(c) + 1;", "Envuelve la línea en if (c != ' ')."]);

        r.Code("n2.diccionarios.indep", s, ItemPurpose.Independent, CodeMode.Function,
            "Escribe PrimerRepetido(int[] numeros) que devuelva el primer número que aparece por segunda vez al recorrer el array, o -1 si ninguno se repite. Usa un HashSet<int>.",
            """
            public static class Conjuntos
            {
                public static int PrimerRepetido(int[] numeros)
                {
                    return -1;
                }
            }
            """,
            """
            public static class Conjuntos
            {
                public static int PrimerRepetido(int[] numeros)
                {
                    HashSet<int> vistos = [];
                    foreach (int n in numeros)
                    {
                        if (!vistos.Add(n))
                        {
                            return n;
                        }
                    }
                    return -1;
                }
            }
            """,
            [
                Is("Repetido", "Conjuntos.PrimerRepetido([3, 1, 4, 1, 5, 3])", "1", hint: "El 1 es el primero que se ve por segunda vez, aunque el 3 apareció antes."),
                Is("Ninguno", "Conjuntos.PrimerRepetido([1, 2, 3])", "-1"),
                Is("Pegados", "Conjuntos.PrimerRepetido([7, 7])", "7", hidden: true, edge: true),
                Is("Vacío", "Conjuntos.PrimerRepetido([])", "-1", hidden: true, edge: true),
                Is("Grande", "Conjuntos.PrimerRepetido(Enumerable.Range(0, 200000).Append(199999).ToArray())", "199999", hidden: true, hint: "Con 200 000 números, buscar en una lista en cada paso es demasiado lento; HashSet responde casi al instante."),
            ],
            "HashSet.Add devuelve false si el elemento ya estaba: una sola operación para comprobar y agregar.",
            ["Crea un HashSet<int> vacío para los números vistos.", "if (!vistos.Add(n)) return n;"],
            difficulty: 2, requires: [CodeConstruct.Loop]);

        r.Code("n2.diccionarios.app", s, ItemPurpose.Transfer, CodeMode.Program,
            "Aplicación: agenda telefónica. Lee comandos hasta «fin»: «agregar NOMBRE TELEFONO» guarda o reemplaza el teléfono; «buscar NOMBRE» muestra «NOMBRE: TELEFONO» o «NOMBRE no encontrado»; «borrar NOMBRE» lo elimina y muestra «Borrado» o «NOMBRE no encontrado». Al terminar muestra «Contactos: N».",
            """
            string comando = Console.ReadLine()!;
            """,
            """
            Dictionary<string, string> agenda = [];
            string comando = Console.ReadLine()!;
            while (comando != "fin")
            {
                string[] partes = comando.Split(' ');
                switch (partes[0])
                {
                    case "agregar":
                        agenda[partes[1]] = partes[2];
                        break;
                    case "buscar":
                        Console.WriteLine(agenda.TryGetValue(partes[1], out string? tel)
                            ? $"{partes[1]}: {tel}"
                            : $"{partes[1]} no encontrado");
                        break;
                    case "borrar":
                        Console.WriteLine(agenda.Remove(partes[1]) ? "Borrado" : $"{partes[1]} no encontrado");
                        break;
                }
                comando = Console.ReadLine()!;
            }
            Console.WriteLine($"Contactos: {agenda.Count}");
            """,
            [
                In("Agregar y buscar", "agregar ana 5551234\nbuscar ana\nfin", "ana: 5551234\nContactos: 1"),
                In("No existe", "buscar luis\nfin", "luis no encontrado\nContactos: 0"),
                In("Reemplazar", "agregar ana 1\nagregar ana 2\nbuscar ana\nfin", "ana: 2\nContactos: 1", hidden: true, edge: true, hint: "Con el indexador agenda[nombre] = tel se agrega o reemplaza; Add lanzaría excepción."),
                In("Borrar", "agregar a 1\nagregar b 2\nborrar a\nborrar a\nbuscar a\nfin", "Borrado\na no encontrado\na no encontrado\nContactos: 1", hidden: true),
            ],
            "El diccionario busca por nombre directamente, sin recorrer todos los contactos.",
            ["Divide cada comando con Split(' ') y decide con un switch sobre partes[0].", "Usa TryGetValue para buscar y Remove (que devuelve bool) para borrar."],
            difficulty: 3, requires: [CodeConstruct.Loop], related: ["n1.switch"]);

        return new Skill(s, UnitId, "Dictionary<TKey,TValue> y HashSet<T>",
            "Búsqueda por clave y conjuntos sin duplicados.",
            Tier.Essential, ["n2.listas"],
            new Lesson(s,
                "Guardar y consultar datos por clave con Dictionary, contar apariciones y usar HashSet para detectar duplicados.",
                [
                    P("Un Dictionary<TKey,TValue> guarda pares clave → valor. Cada clave es única y permite encontrar su valor sin recorrer toda la colección."),
                    Code("""
                        Dictionary<string, decimal> precios = new()
                        {
                            ["pan"] = 2.50m,
                            ["café"] = 8.00m,
                        };
                        precios["leche"] = 3.20m;
                        Console.WriteLine(precios["café"]);
                        if (precios.TryGetValue("té", out decimal p))
                        {
                            Console.WriteLine(p);
                        }
                        else
                        {
                            Console.WriteLine("No hay té");
                        }
                        """, "Agregar y consultar"),
                    P("Escribir dic[clave] = valor agrega o reemplaza. Leer dic[clave] con una clave inexistente lanza KeyNotFoundException; para evitarlo usa TryGetValue, ContainsKey o GetValueOrDefault."),
                    P("Un HashSet<T> es un conjunto: guarda valores sin duplicados y responde muy rápido si un valor está o no."),
                    Note("El orden en que un Dictionary o un HashSet devuelve sus elementos no está garantizado; no dependas de él."),
                ],
                new WorkedExample("Contar votos",
                    """
                    string[] votos = ["rojo", "azul", "rojo", "verde", "rojo", "azul"];
                    Dictionary<string, int> conteo = [];
                    HashSet<string> opciones = [];
                    foreach (string v in votos)
                    {
                        conteo[v] = conteo.GetValueOrDefault(v) + 1;
                        opciones.Add(v);
                    }
                    Console.WriteLine($"Opciones distintas: {opciones.Count}");
                    Console.WriteLine($"rojo: {conteo["rojo"]}, azul: {conteo["azul"]}, verde: {conteo["verde"]}");
                    """,
                    [
                        Line(2, "La clave es la opción; el valor, cuántos votos tiene."),
                        Line(3, "El conjunto solo guarda cada opción una vez."),
                        Line(6, "GetValueOrDefault da 0 para una opción nueva."),
                        Line(7, "Agregar un duplicado no tiene efecto."),
                    ],
                    "Opciones distintas: 3\nrojo: 3, azul: 2, verde: 1"),
                new CommonMistake(
                    "Incrementar una clave que todavía no existe.",
                    "Dictionary<string, int> cuenta = [];\ncuenta[\"a\"]++;",
                    "Para incrementar primero hay que leer, y leer una clave inexistente lanza KeyNotFoundException.",
                    "Dictionary<string, int> cuenta = [];\ncuenta[\"a\"] = cuenta.GetValueOrDefault(\"a\") + 1;\nConsole.WriteLine(cuenta[\"a\"]);"),
                "n2.diccionarios.pred1", "n2.diccionarios.guiado", "n2.diccionarios.indep", "n2.diccionarios.app",
                ["n2.diccionarios.recall1", "n2.diccionarios.recall2", "n2.diccionarios.pred2"],
                "Usas Dictionary y HashSet para buscar, contar y eliminar duplicados sin provocar KeyNotFoundException.",
                18));
    }

    private static Skill Eleccion(ContentRegistry r)
    {
        const string s = "n2.eleccion";
        r.Short("n2.eleccion.pred1", s, ItemPurpose.Prediction,
            "¿Qué muestra este programa?",
            "La lista conserva todos los elementos, incluido el 2 repetido (4). El conjunto descarta el duplicado (3).",
            ["3 4"], match: AnswerMatch.Output,
            code: """
                HashSet<int> conjunto = [1, 2, 2, 3];
                List<int> lista = [1, 2, 2, 3];
                Console.WriteLine($"{conjunto.Count} {lista.Count}");
                """);

        r.Choice("n2.eleccion.concepto1", s, ItemPurpose.Concept,
            "Un sistema registra nombres de usuario y debe comprobar miles de veces por segundo si un nombre ya está tomado entre millones. ¿Qué estructura conviene?",
            "Un HashSet<string> responde «¿está x?» en un tiempo que, en promedio, no crece con la cantidad de elementos. List.Contains tiene que revisarlos uno a uno.",
            [
                Ok("HashSet<string>", "Correcto: la pregunta es solo si existe, sin orden ni valor asociado."),
                No("List<string>", "Contains recorre la lista: con millones de nombres es lento.", ErrorCategory.ConceptMisunderstanding),
                No("string[]", "Además de tener tamaño fijo, buscar requiere recorrerlo.", ErrorCategory.ConceptMisunderstanding),
            ],
            dimension: KnowledgeDimension.Design);

        r.Choice("n2.eleccion.recall1", s, ItemPurpose.Recall,
            "Si duplicas la cantidad de elementos, ¿cómo cambia aproximadamente el tiempo de lista.Contains(x) cuando x no está?",
            "Contains en una lista revisa los elementos uno a uno: con el doble de elementos, el doble de trabajo (costo lineal).",
            [
                Ok("Se duplica: revisa los elementos uno por uno.", "Correcto."),
                No("No cambia.", "Eso describe a HashSet.Contains en promedio, no a List."),
                No("Se multiplica por cuatro.", "Eso sería un costo cuadrático."),
            ],
            related: ["n1.complejidad"]);

        r.Choice("n2.eleccion.recall2", s, ItemPurpose.Recall,
            "¿Qué colección eliges si necesitas conservar el orden, permitir repetidos y acceder por posición?",
            "List<T> (o un array si el tamaño es fijo). Dictionary y HashSet no ofrecen acceso por posición.",
            [
                Ok("List<T>", "Correcto."),
                No("HashSet<T>", "No permite repetidos ni acceso por posición."),
                No("Dictionary<TKey,TValue>", "Organiza por clave, no por posición."),
            ]);

        r.Choice("n2.eleccion.bug1", s, ItemPurpose.BugFinding,
            "Este método es correcto pero tarda minutos con 200 000 pedidos. ¿Cuál es la causa?",
            "Por cada pedido se llama a procesados.Contains, que recorre toda la lista: con n pedidos son cerca de n²/2 comparaciones. Con un HashSet cada comprobación es casi inmediata.",
            [
                Ok("List.Contains dentro del bucle hace el trabajo cuadrático; conviene un HashSet.", "Correcto."),
                No("El foreach es más lento que un for.", "La diferencia entre ambos es mínima comparada con la búsqueda lineal.", ErrorCategory.ConceptMisunderstanding),
                No("Agregar a una lista es lento.", "Add al final de una lista es muy rápido en promedio.", ErrorCategory.ConceptMisunderstanding),
            ],
            code: """
                Console.WriteLine(Unicos([5, 3, 5, 8])); // 3

                int Unicos(int[] pedidos)
                {
                    List<int> procesados = [];
                    foreach (int id in pedidos)
                    {
                        if (!procesados.Contains(id))
                        {
                            procesados.Add(id);
                        }
                    }
                    return procesados.Count;
                }
                """,
            targets: [ErrorCategory.InfiniteLoopOrTimeout]);

        r.Code("n2.eleccion.guiado", s, ItemPurpose.Guided, CodeMode.Function,
            "Completa TodosDistintos para que devuelva true si ningún número se repite. Debe funcionar rápido con cientos de miles de números: usa un HashSet<int>.",
            """
            public static class Datos
            {
                public static bool TodosDistintos(int[] numeros)
                {
                    HashSet<int> vistos = [];
                    // recorre y devuelve false si encuentras uno ya visto
                    return false;
                }
            }
            """,
            """
            public static class Datos
            {
                public static bool TodosDistintos(int[] numeros)
                {
                    HashSet<int> vistos = [];
                    foreach (int n in numeros)
                    {
                        if (!vistos.Add(n))
                        {
                            return false;
                        }
                    }
                    return true;
                }
            }
            """,
            [
                Is("Distintos", "Datos.TodosDistintos([1, 2, 3])", "true"),
                Is("Repetido", "Datos.TodosDistintos([1, 2, 1])", "false"),
                Is("Vacío", "Datos.TodosDistintos([])", "true", hidden: true, edge: true),
                Is("200 000 distintos", "Datos.TodosDistintos(Enumerable.Range(0, 200000).ToArray())", "true", hidden: true, hint: "Si comparas cada número con todos los demás, con 200 000 números son unos 20 000 millones de comparaciones."),
            ],
            "Cada número se agrega una vez y la comprobación es casi inmediata: el trabajo crece en proporción a n.",
            ["vistos.Add(n) devuelve false si n ya estaba.", "Si llegas al final del bucle sin repetidos, devuelve true."],
            difficulty: 2, requires: [CodeConstruct.Loop]);

        r.Code("n2.eleccion.indep", s, ItemPurpose.Independent, CodeMode.Function,
            "Escribe Comunes(int[] a, int[] b) que devuelva una List<int> con los valores de a que también están en b, sin repetidos y en el orden en que aparecen en a. Debe ser rápido con arrays grandes.",
            """
            public static class Datos
            {
                public static List<int> Comunes(int[] a, int[] b)
                {
                    return [];
                }
            }
            """,
            """
            public static class Datos
            {
                public static List<int> Comunes(int[] a, int[] b)
                {
                    HashSet<int> enB = [.. b];
                    HashSet<int> agregados = [];
                    List<int> resultado = [];
                    foreach (int x in a)
                    {
                        if (enB.Contains(x) && agregados.Add(x))
                        {
                            resultado.Add(x);
                        }
                    }
                    return resultado;
                }
            }
            """,
            [
                Is("Básico", "Datos.Comunes([1, 2, 3, 4], [4, 2, 9])", "[2, 4]"),
                Is("Repetidos en a", "Datos.Comunes([5, 5, 6], [5, 6])", "[5, 6]", edge: true),
                Is("Ninguno", "Datos.Comunes([1], [2])", "[]", hidden: true, edge: true),
                Is("Grande", "Datos.Comunes(Enumerable.Range(0, 150000).ToArray(), Enumerable.Range(149990, 20).ToArray()).Count", "10", hidden: true, hint: "Convierte b en un HashSet antes del bucle para no recorrerlo en cada paso."),
            ],
            "Convertir b en un conjunto una sola vez cuesta proporcional a su tamaño, y después cada consulta es casi inmediata.",
            ["Crea un HashSet<int> con los elementos de b: HashSet<int> enB = [.. b];", "Usa otro HashSet para no agregar repetidos al resultado."],
            difficulty: 3, requires: [CodeConstruct.Loop], related: ["n1.complejidad"]);

        r.Code("n2.eleccion.app", s, ItemPurpose.Transfer, CodeMode.Function,
            "Aplicación: inventario. Resumen(string[] movimientos) recibe líneas como «+manzana 5» (entrada) o «-manzana 2» (salida). Devuelve el inventario final como «producto=cantidad» separados por «, », ordenado alfabéticamente y omitiendo los productos con 0. Si una salida deja un producto en negativo, lanza InvalidOperationException.",
            """
            public static class Almacen
            {
                public static string Resumen(string[] movimientos)
                {
                    return "";
                }
            }
            """,
            """
            public static class Almacen
            {
                public static string Resumen(string[] movimientos)
                {
                    Dictionary<string, int> stock = [];
                    foreach (string m in movimientos)
                    {
                        string[] partes = m.Substring(1).Split(' ');
                        string producto = partes[0];
                        int cantidad = int.Parse(partes[1]);
                        int actual = stock.GetValueOrDefault(producto);
                        int nuevo = m[0] == '+' ? actual + cantidad : actual - cantidad;
                        if (nuevo < 0)
                        {
                            throw new InvalidOperationException($"No hay suficiente {producto}");
                        }
                        stock[producto] = nuevo;
                    }

                    List<string> claves = [.. stock.Keys];
                    claves.Sort(string.CompareOrdinal);
                    List<string> partesResultado = [];
                    foreach (string k in claves)
                    {
                        if (stock[k] > 0)
                        {
                            partesResultado.Add($"{k}={stock[k]}");
                        }
                    }
                    return string.Join(", ", partesResultado);
                }
            }
            """,
            [
                Is("Básico", "Almacen.Resumen([\"+pera 3\", \"+manzana 5\", \"-manzana 2\"])", "manzana=3, pera=3"),
                Is("Se agota", "Almacen.Resumen([\"+uva 2\", \"-uva 2\", \"+kiwi 1\"])", "kiwi=1", edge: true),
                Throws("Negativo", "Almacen.Resumen([\"+pan 1\", \"-pan 3\"])", "InvalidOperationException", hidden: true),
                Is("Vacío", "Almacen.Resumen([]).Length", "0", hidden: true, edge: true),
                Is("Muchos", "Almacen.Resumen(Enumerable.Range(0, 100000).Select(i => $\"+p{i % 3} 1\").ToArray())", "p0=33334, p1=33333, p2=33333", hidden: true),
            ],
            "Un Dictionary acumula por producto; ordenar las claves al final da una salida estable sin depender del orden interno del diccionario.",
            ["El primer carácter indica el tipo de movimiento; Substring(1).Split(' ') separa producto y cantidad.", "Copia las claves a una lista ([.. stock.Keys]) y ordénala con Sort."],
            difficulty: 3, requires: [CodeConstruct.Loop], related: ["n2.strings"], targets: [ErrorCategory.EdgeCaseMissed]);

        return new Skill(s, UnitId, "Elegir la estructura de datos",
            "Comparar costo y propósito de cada colección.",
            Tier.Intermediate, ["n2.diccionarios"],
            new Lesson(s,
                "Elegir entre array, List, Dictionary y HashSet según las operaciones que necesita el problema y su costo aproximado.",
                [
                    P("Cada colección es buena para ciertas operaciones. La pregunta clave es: ¿qué voy a hacer más veces con estos datos?"),
                    P("Array: tamaño fijo, acceso por posición. List<T>: tamaño variable, orden y acceso por posición; buscar un valor implica recorrerla. Dictionary: buscar un valor por su clave. HashSet: saber si algo está, sin duplicados."),
                    Code("""
                        List<int> lista = [];
                        HashSet<int> conjunto = [];
                        for (int i = 0; i < 50000; i++)
                        {
                            lista.Add(i);
                            conjunto.Add(i);
                        }
                        int encontradosLista = 0, encontradosConjunto = 0;
                        for (int i = 0; i < 1000; i++)
                        {
                            if (lista.Contains(49999 - i)) encontradosLista++;
                            if (conjunto.Contains(49999 - i)) encontradosConjunto++;
                        }
                        Console.WriteLine($"{encontradosLista} {encontradosConjunto}");
                        """, "Ambas encuentran lo mismo; la lista revisa miles de elementos en cada búsqueda"),
                    P("Buscar en una lista cuesta proporcional a su tamaño. Buscar en un HashSet o un Dictionary cuesta, en promedio, casi lo mismo sin importar el tamaño. La diferencia se nota cuando la búsqueda está dentro de un bucle."),
                    Design("No optimices a ciegas: con 20 elementos cualquier opción sirve. Elige por claridad y cambia de estructura cuando el tamaño o la frecuencia de las búsquedas lo justifiquen."),
                ],
                new WorkedExample("Detectar correos repetidos en una lista de registro",
                    """
                    string[] registros = ["ana@x.com", "luis@x.com", "ANA@x.com", "eva@x.com", "luis@x.com"];
                    HashSet<string> vistos = [];
                    List<string> repetidos = [];
                    foreach (string correo in registros)
                    {
                        string normal = correo.ToLowerInvariant();
                        if (!vistos.Add(normal))
                        {
                            repetidos.Add(normal);
                        }
                    }
                    Console.WriteLine($"Únicos: {vistos.Count}");
                    Console.WriteLine($"Repetidos: {string.Join(", ", repetidos)}");
                    """,
                    [
                        Line(2, "Pregunta frecuente: ¿ya lo vi? → HashSet."),
                        Line(3, "Necesitamos el orden de aparición de los repetidos → List."),
                        Line(6, "Normalizar antes de comparar evita que ANA y ana cuenten como distintos."),
                        Line(7, "Add devuelve false si ya estaba."),
                    ],
                    "Únicos: 3\nRepetidos: ana@x.com, luis@x.com"),
                new CommonMistake(
                    "Buscar con List.Contains dentro de un bucle sobre muchos datos.",
                    "List<int> vistos = [];\nforeach (int x in datos)\n{\n    if (!vistos.Contains(x)) vistos.Add(x);\n}",
                    "Cada Contains recorre la lista: el trabajo total crece con el cuadrado del número de datos y se vuelve muy lento.",
                    "int[] datos = [1, 2, 2, 3];\nHashSet<int> vistos = [];\nforeach (int x in datos)\n{\n    vistos.Add(x);\n}\nConsole.WriteLine(vistos.Count);"),
                "n2.eleccion.pred1", "n2.eleccion.guiado", "n2.eleccion.indep", "n2.eleccion.app",
                ["n2.eleccion.recall1", "n2.eleccion.recall2", "n2.eleccion.concepto1"],
                "Justificas la elección de una colección según las operaciones frecuentes y evitas búsquedas lineales dentro de bucles grandes.",
                15));
    }
}
