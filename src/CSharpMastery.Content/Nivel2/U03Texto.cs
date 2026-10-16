using CSharpMastery.Content.Infrastructure;
using CSharpMastery.Domain.Curriculum;
using CSharpMastery.Domain.Items;
using CSharpMastery.Domain.Progress;
using static CSharpMastery.Content.Infrastructure.ContentRegistry;
using static CSharpMastery.Content.Infrastructure.L;

namespace CSharpMastery.Content.Nivel2;

internal static class U03Texto
{
    public const string UnitId = "n2.u3";

    public static Unit Build(ContentRegistry r) => new(UnitId, "n2", "Texto y tipos simples",
        "Manipulación de strings, enumeraciones y tuplas.",
        [Strings(r), Enums(r), Tuplas(r)]);

    private static Skill Strings(ContentRegistry r)
    {
        const string s = "n2.strings";
        r.Short("n2.strings.pred1", s, ItemPurpose.Prediction,
            "¿Qué muestra este programa?",
            "Trim quita los espacios de los extremos: «Hola Mundo» (10 caracteres). ToUpper no modifica t; devuelve un string nuevo que se descarta. IndexOf(\"M\") es 5.",
            ["10 Hola Mundo 5"], match: AnswerMatch.Output,
            code: """
                string t = "  Hola Mundo ".Trim();
                t.ToUpper();
                Console.WriteLine($"{t.Length} {t} {t.IndexOf("M")}");
                """,
            difficulty: 2,
            wrong: new() { ["10 HOLA MUNDO 5"] = "Los strings son inmutables: t.ToUpper() devuelve un texto nuevo, pero nadie lo guarda." });

        r.Choice("n2.strings.concepto1", s, ItemPurpose.Concept,
            "¿Qué significa que los strings de C# sean inmutables?",
            "Un string nunca cambia después de crearse. Métodos como Replace, ToUpper o Trim devuelven un string nuevo; hay que guardar el resultado.",
            [
                Ok("Que sus métodos devuelven un string nuevo en lugar de modificar el original.", "Correcto."),
                No("Que una variable string no puede reasignarse.", "La variable sí puede apuntar a otro string: texto = texto.Trim();"),
                No("Que no se pueden concatenar.", "Se pueden concatenar; el resultado es un string nuevo."),
            ],
            targets: [ErrorCategory.ConceptMisunderstanding]);

        r.Choice("n2.strings.recall1", s, ItemPurpose.Recall,
            "¿Qué devuelve \"a,b,,c\".Split(',')?",
            "Split corta en cada separador, así que entre las dos comas queda un elemento vacío: [\"a\", \"b\", \"\", \"c\"].",
            [
                Ok("Un array de 4 elementos, uno de ellos vacío.", "Correcto: para descartarlos existe StringSplitOptions.RemoveEmptyEntries."),
                No("Un array de 3 elementos.", "El texto vacío entre las dos comas también es un elemento."),
                No("Un string sin comas.", "Split devuelve un array de strings."),
            ]);

        r.Choice("n2.strings.recall2", s, ItemPurpose.Recall,
            "¿Por qué se usa StringBuilder al construir un texto en un bucle con muchas vueltas?",
            "Con += cada vuelta crea un string nuevo copiando todo lo anterior. StringBuilder va agregando sobre un búfer interno y evita esas copias.",
            [
                Ok("Porque += crea un string nuevo en cada vuelta y StringBuilder evita esas copias.", "Correcto."),
                No("Porque += no funciona dentro de un bucle.", "Funciona; solo es costoso con muchas vueltas."),
                No("Porque StringBuilder admite caracteres especiales.", "Ambos admiten cualquier carácter."),
            ]);

        r.Choice("n2.strings.bug1", s, ItemPurpose.BugFinding,
            "Este código debía mostrar «HOLA», pero muestra «hola». ¿Por qué?",
            "ToUpper no modifica el string: devuelve uno nuevo. Hay que guardar el resultado: saludo = saludo.ToUpper();",
            [
                Ok("No se guarda el resultado de ToUpper.", "Correcto."),
                No("ToUpper solo funciona con letras sin acento.", "Funciona con cualquier letra.", ErrorCategory.ConceptMisunderstanding),
                No("Falta convertir saludo a char.", "ToUpper existe para string.", ErrorCategory.TypeMismatch),
            ],
            code: """
                string saludo = "hola";
                saludo.ToUpper();
                Console.WriteLine(saludo);
                """,
            targets: [ErrorCategory.ConceptMisunderstanding]);

        r.Code("n2.strings.guiado", s, ItemPurpose.Guided, CodeMode.Function,
            "Completa EsPalindromo: devuelve true si el texto se lee igual al derecho y al revés, ignorando mayúsculas y espacios.",
            """
            public static class Palabras
            {
                public static bool EsPalindromo(string texto)
                {
                    string limpio = texto.Replace(" ", "").ToLower();
                    // compara limpio[i] con limpio[limpio.Length - 1 - i]
                    return false;
                }
            }
            """,
            """
            public static class Palabras
            {
                public static bool EsPalindromo(string texto)
                {
                    string limpio = texto.Replace(" ", "").ToLower();
                    for (int i = 0; i < limpio.Length / 2; i++)
                    {
                        if (limpio[i] != limpio[limpio.Length - 1 - i])
                        {
                            return false;
                        }
                    }
                    return true;
                }
            }
            """,
            [
                Is("Simple", "Palabras.EsPalindromo(\"reconocer\")", "true"),
                Is("Con espacios", "Palabras.EsPalindromo(\"Anita lava la tina\")", "true"),
                Is("No", "Palabras.EsPalindromo(\"programa\")", "false"),
                Is("Vacío", "Palabras.EsPalindromo(\"\")", "true", hidden: true, edge: true),
            ],
            "Basta con recorrer la mitad: cada comparación revisa un carácter de cada extremo.",
            ["Recorre i desde 0 hasta limpio.Length / 2.", "Si algún par no coincide, devuelve false; si terminas, true."],
            requires: [CodeConstruct.Loop]);

        r.Code("n2.strings.indep", s, ItemPurpose.Independent, CodeMode.Function,
            "Escribe Iniciales(string nombreCompleto) que devuelva las iniciales en mayúscula separadas por punto, por ejemplo «ana maría lópez» → «A.M.L.». Ignora los espacios repetidos.",
            """
            public static class Nombres
            {
                public static string Iniciales(string nombreCompleto)
                {
                    return nombreCompleto;
                }
            }
            """,
            """
            using System.Text;

            public static class Nombres
            {
                public static string Iniciales(string nombreCompleto)
                {
                    StringBuilder sb = new();
                    foreach (string parte in nombreCompleto.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                    {
                        sb.Append(char.ToUpper(parte[0])).Append('.');
                    }
                    return sb.ToString();
                }
            }
            """,
            [
                Is("Tres nombres", "Nombres.Iniciales(\"ana maría lópez\")", "A.M.L."),
                Is("Ya en mayúsculas", "Nombres.Iniciales(\"Luis Pérez\")", "L.P."),
                Is("Espacios extra", "Nombres.Iniciales(\"  eva   ruiz \")", "E.R.", hidden: true, edge: true, hint: "Usa StringSplitOptions.RemoveEmptyEntries para descartar las partes vacías."),
                Is("Vacío", "Nombres.Iniciales(\"\").Length", "0", hidden: true, edge: true),
            ],
            "RemoveEmptyEntries evita que los espacios repetidos produzcan partes vacías (y un IndexOutOfRangeException al pedir parte[0]).",
            ["Divide con Split(' ', StringSplitOptions.RemoveEmptyEntries).", "Toma parte[0], conviértelo con char.ToUpper y agrega un punto."],
            difficulty: 2, requires: [CodeConstruct.Loop], targets: [ErrorCategory.IndexOutOfRange]);

        r.Code("n2.strings.app", s, ItemPurpose.Transfer, CodeMode.Function,
            "Aplicación: genera el «slug» de un título para una URL. Slug(string titulo): pasa a minúsculas, cambia á é í ó ú ü ñ por a e i o u u n, conserva letras y dígitos, convierte cualquier otro carácter en separador y une las palabras con un guion, sin guiones al inicio, al final ni repetidos.",
            """
            public static class Url
            {
                public static string Slug(string titulo)
                {
                    return titulo.ToLower().Replace(" ", "-");
                }
            }
            """,
            """
            using System.Text;

            public static class Url
            {
                public static string Slug(string titulo)
                {
                    StringBuilder sb = new();
                    bool separadorPendiente = false;
                    foreach (char original in titulo.ToLowerInvariant())
                    {
                        char c = original switch
                        {
                            'á' => 'a',
                            'é' => 'e',
                            'í' => 'i',
                            'ó' => 'o',
                            'ú' or 'ü' => 'u',
                            'ñ' => 'n',
                            _ => original,
                        };
                        if (c is (>= 'a' and <= 'z') or (>= '0' and <= '9'))
                        {
                            if (separadorPendiente && sb.Length > 0)
                            {
                                sb.Append('-');
                            }
                            sb.Append(c);
                            separadorPendiente = false;
                        }
                        else
                        {
                            separadorPendiente = true;
                        }
                    }
                    return sb.ToString();
                }
            }
            """,
            [
                Is("Simple", "Url.Slug(\"Hola Mundo\")", "hola-mundo"),
                Is("Acentos y ñ", "Url.Slug(\"Año de programación\")", "ano-de-programacion"),
                Is("Signos", "Url.Slug(\"¿Qué es C#? ¡Guía 2025!\")", "que-es-c-guia-2025", hidden: true, edge: true, hint: "Los signos cuentan como separadores, pero nunca debe haber dos guiones seguidos ni uno al final."),
                Is("Extremos", "Url.Slug(\"  --Listas--  \")", "listas", hidden: true, edge: true),
            ],
            "Posponer el guion hasta ver el siguiente carácter válido resuelve a la vez los repetidos, el inicio y el final.",
            ["Recorre carácter por carácter y usa un bool que recuerde si viste un separador.", "Agrega el guion justo antes de la siguiente letra válida, y solo si ya hay algo escrito."],
            difficulty: 3, requires: [CodeConstruct.Loop], related: ["n1.switch"]);

        return new Skill(s, UnitId, "Strings y manipulación de texto",
            "Recorrer, buscar, dividir y construir texto.",
            Tier.Essential, ["n2.arrays"],
            new Lesson(s,
                "Recorrer, buscar, dividir, transformar y construir texto entendiendo que los strings son inmutables.",
                [
                    P("Un string es una secuencia de caracteres. Se puede indexar como un array (texto[0] es un char) y su largo está en Length."),
                    Code("""
                        string frase = "  El lenguaje C#  ";
                        string limpia = frase.Trim();
                        Console.WriteLine(limpia.ToUpper());
                        Console.WriteLine(limpia.Contains("C#"));
                        Console.WriteLine(limpia.IndexOf("lenguaje"));
                        Console.WriteLine(limpia.Replace("C#", "Python"));
                        string[] palabras = limpia.Split(' ');
                        Console.WriteLine(palabras.Length);
                        Console.WriteLine(limpia.Substring(3, 8));
                        """, "Métodos frecuentes"),
                    P("Los strings son inmutables: Trim, ToUpper, Replace y los demás métodos devuelven un string nuevo. Si no guardas el resultado, se pierde."),
                    P("Para construir un texto pieza a pieza en un bucle largo, usa StringBuilder (en System.Text) en lugar de +=."),
                    Tip("string.IsNullOrWhiteSpace(texto) comprueba de una vez si un texto es null, vacío o solo espacios."),
                    Warn("Comparar texto escrito por personas suele requerir normalizar primero: Trim y ToLowerInvariant."),
                ],
                new WorkedExample("Contar palabras y encontrar la más larga",
                    """
                    string texto = "La práctica  espaciada mejora la memoria";
                    string[] palabras = texto.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    string masLarga = "";
                    foreach (string p in palabras)
                    {
                        if (p.Length > masLarga.Length)
                        {
                            masLarga = p;
                        }
                    }
                    Console.WriteLine($"Palabras: {palabras.Length}");
                    Console.WriteLine($"Más larga: {masLarga} ({masLarga.Length})");
                    """,
                    [
                        Line(1, "Hay dos espacios seguidos entre «práctica» y «espaciada»."),
                        Line(2, "RemoveEmptyEntries descarta el elemento vacío que producirían."),
                        Line(5, "Con > nos quedamos con la primera en caso de empate."),
                    ],
                    "Palabras: 6\nMás larga: espaciada (9)"),
                new CommonMistake(
                    "Llamar a un método de string sin guardar el resultado.",
                    "string nombre = \"  ana \";\nnombre.Trim();\nConsole.WriteLine($\"[{nombre}]\");",
                    "Trim devuelve un string nuevo; nombre sigue teniendo los espacios.",
                    "string nombre = \"  ana \";\nnombre = nombre.Trim();\nConsole.WriteLine($\"[{nombre}]\");"),
                "n2.strings.pred1", "n2.strings.guiado", "n2.strings.indep", "n2.strings.app",
                ["n2.strings.recall1", "n2.strings.recall2", "n2.strings.concepto1"],
                "Procesas texto con los métodos de string, guardas sus resultados y usas StringBuilder cuando construyes texto en bucles.",
                18));
    }

    private static Skill Enums(ContentRegistry r)
    {
        const string s = "n2.enums";
        r.Short("n2.enums.pred1", s, ItemPurpose.Prediction,
            "¿Qué muestra este programa?",
            "Al mostrar un enum se escribe su nombre. Cada valor tiene además un número: por defecto el primero es 0, el siguiente 1, etc. Miercoles es el tercero (2).",
            ["Miercoles 2"], match: AnswerMatch.Output,
            code: """
                Dia d = Dia.Miercoles;
                Console.WriteLine($"{d} {(int)d}");

                enum Dia { Lunes, Martes, Miercoles }
                """,
            wrong: new() { ["Miercoles 3"] = "Los valores empiezan en 0: Lunes = 0, Martes = 1, Miercoles = 2." });

        r.Choice("n2.enums.concepto1", s, ItemPurpose.Concept,
            "¿Por qué es mejor un enum EstadoPedido { Pendiente, Enviado, Entregado } que guardar el estado como string?",
            "El compilador solo acepta valores del enum: un error de escritura como \"Enviaod\" no compila. Además el editor sugiere los valores posibles y switch puede comprobarlos.",
            [
                Ok("El compilador rechaza valores inexistentes y los errores de escritura.", "Correcto."),
                No("Ocupa menos memoria, y es la única ventaja.", "La ventaja principal es la seguridad: valores inválidos no compilan.", ErrorCategory.ConceptMisunderstanding),
                No("Permite agregar estados nuevos en tiempo de ejecución.", "Al contrario: el conjunto de valores es fijo y se conoce al compilar."),
            ],
            dimension: KnowledgeDimension.Design);

        r.Choice("n2.enums.recall1", s, ItemPurpose.Recall,
            "¿Cómo se convierte el texto \"Rojo\" al valor Color.Rojo de forma segura?",
            "Enum.TryParse<Color>(\"Rojo\", out var c) devuelve true y deja el valor en c; si el texto no corresponde a ningún valor devuelve false.",
            [
                Ok("Enum.TryParse<Color>(\"Rojo\", out var c)", "Correcto."),
                No("(Color)\"Rojo\"", "No se puede convertir un string a enum con un cast.", ErrorCategory.TypeMismatch),
                No("Color.Parse(\"Rojo\")", "Ese método no existe en el enum."),
            ]);

        r.Choice("n2.enums.recall2", s, ItemPurpose.Recall,
            "¿Qué número tiene Talla.M en «enum Talla { S = 1, M, L }»?",
            "Después de un valor explícito, los siguientes continúan sumando 1: S = 1, M = 2, L = 3.",
            [
                Ok("2", "Correcto."),
                No("1", "Ese es S."),
                No("0", "Solo sería 0 si no se hubiera asignado S = 1."),
            ]);

        r.Choice("n2.enums.bug1", s, ItemPurpose.BugFinding,
            "Este método debería describir todas las prioridades, pero con Prioridad.Critica devuelve «Desconocida». ¿Qué falta?",
            "Se agregó Critica al enum pero no al switch, y el caso por defecto oculta el olvido. Hay que añadir el caso que falta.",
            [
                Ok("Falta un caso para Critica en el switch; el _ oculta el olvido.", "Correcto."),
                No("Los enums no se pueden usar en switch.", "Se pueden y es su uso más común.", ErrorCategory.ConceptMisunderstanding),
                No("Critica debería escribirse en mayúsculas.", "El nombre es correcto; falta el caso.", ErrorCategory.Syntax),
            ],
            code: """
                string Describir(Prioridad p) => p switch
                {
                    Prioridad.Baja => "Puede esperar",
                    Prioridad.Alta => "Hoy mismo",
                    _ => "Desconocida",
                };

                enum Prioridad { Baja, Alta, Critica }
                """,
            targets: [ErrorCategory.EdgeCaseMissed]);

        r.Code("n2.enums.guiado", s, ItemPurpose.Guided, CodeMode.Function,
            "Completa Siguiente para que devuelva la luz que sigue en un semáforo: Verde → Amarillo → Rojo → Verde.",
            """
            public enum Luz { Verde, Amarillo, Rojo }

            public static class Semaforo
            {
                public static Luz Siguiente(Luz actual) => actual switch
                {
                    Luz.Verde => Luz.Amarillo,
                    // completa los otros dos casos
                    _ => actual,
                };
            }
            """,
            """
            public enum Luz { Verde, Amarillo, Rojo }

            public static class Semaforo
            {
                public static Luz Siguiente(Luz actual) => actual switch
                {
                    Luz.Verde => Luz.Amarillo,
                    Luz.Amarillo => Luz.Rojo,
                    Luz.Rojo => Luz.Verde,
                    _ => throw new ArgumentOutOfRangeException(nameof(actual)),
                };
            }
            """,
            [
                Is("Verde", "Semaforo.Siguiente(Luz.Verde)", "Amarillo"),
                Is("Amarillo", "Semaforo.Siguiente(Luz.Amarillo)", "Rojo"),
                Is("Rojo", "Semaforo.Siguiente(Luz.Rojo)", "Verde", hidden: true, edge: true),
            ],
            "Con un caso para cada valor, el _ solo atrapa valores imposibles; lanzar una excepción ahí hace visible cualquier error.",
            ["Agrega Luz.Amarillo => Luz.Rojo y Luz.Rojo => Luz.Verde.", "En el caso _ puedes lanzar ArgumentOutOfRangeException."],
            requires: [CodeConstruct.Switch]);

        r.Code("n2.enums.indep", s, ItemPurpose.Independent, CodeMode.Function,
            "Declara enum Categoria { Bajo, Normal, Sobrepeso, Obesidad } y escribe Clasificar(double imc): menor que 18.5 Bajo; menor que 25 Normal; menor que 30 Sobrepeso; resto Obesidad. Si imc <= 0, lanza ArgumentOutOfRangeException.",
            """
            public static class Salud
            {
            }
            """,
            """
            public enum Categoria { Bajo, Normal, Sobrepeso, Obesidad }

            public static class Salud
            {
                public static Categoria Clasificar(double imc)
                {
                    if (imc <= 0) throw new ArgumentOutOfRangeException(nameof(imc));
                    if (imc < 18.5) return Categoria.Bajo;
                    if (imc < 25) return Categoria.Normal;
                    if (imc < 30) return Categoria.Sobrepeso;
                    return Categoria.Obesidad;
                }
            }
            """,
            [
                Is("Normal", "Salud.Clasificar(22)", "Normal"),
                Is("Bajo", "Salud.Clasificar(17.9)", "Bajo"),
                Is("Límite 25", "Salud.Clasificar(25)", "Sobrepeso", hidden: true, edge: true, hint: "25 ya no es menor que 25."),
                Is("Obesidad", "Salud.Clasificar(31.2)", "Obesidad", hidden: true),
                Throws("Inválido", "Salud.Clasificar(0)", "ArgumentOutOfRangeException", hidden: true),
            ],
            "Devolver un enum en lugar de un string permite que quien llama use switch y que el compilador detecte errores.",
            ["Declara el enum fuera de la clase: public enum Categoria { ... }.", "Ordena las condiciones de menor a mayor y usa return en cada una."],
            difficulty: 2, requires: [CodeConstruct.Conditional], related: ["n1.if-else"]);

        r.Code("n2.enums.app", s, ItemPurpose.Transfer, CodeMode.Program,
            "Aplicación: seguimiento de un pedido. Declara enum Estado { Creado, Pagado, Enviado, Entregado, Cancelado }. El pedido empieza en Creado. Lee acciones hasta «fin»: «pagar» (solo desde Creado), «enviar» (solo desde Pagado), «entregar» (solo desde Enviado), «cancelar» (desde Creado o Pagado). Si la acción es válida muestra el nuevo estado; si no, «No se puede ACCION en ESTADO». Al final muestra «Final: ESTADO».",
            """
            string accion = Console.ReadLine()!;
            """,
            """
            Estado estado = Estado.Creado;
            string accion = Console.ReadLine()!;
            while (accion != "fin")
            {
                Estado? nuevo = (accion, estado) switch
                {
                    ("pagar", Estado.Creado) => Estado.Pagado,
                    ("enviar", Estado.Pagado) => Estado.Enviado,
                    ("entregar", Estado.Enviado) => Estado.Entregado,
                    ("cancelar", Estado.Creado or Estado.Pagado) => Estado.Cancelado,
                    _ => null,
                };
                if (nuevo is Estado e)
                {
                    estado = e;
                    Console.WriteLine(estado);
                }
                else
                {
                    Console.WriteLine($"No se puede {accion} en {estado}");
                }
                accion = Console.ReadLine()!;
            }
            Console.WriteLine($"Final: {estado}");

            enum Estado { Creado, Pagado, Enviado, Entregado, Cancelado }
            """,
            [
                In("Camino feliz", "pagar\nenviar\nentregar\nfin", "Pagado\nEnviado\nEntregado\nFinal: Entregado"),
                In("Acción inválida", "enviar\npagar\nfin", "No se puede enviar en Creado\nPagado\nFinal: Pagado"),
                In("Cancelar tarde", "pagar\nenviar\ncancelar\nfin", "Pagado\nEnviado\nNo se puede cancelar en Enviado\nFinal: Enviado", hidden: true, edge: true),
                In("Cancelar y seguir", "cancelar\npagar\nfin", "Cancelado\nNo se puede pagar en Cancelado\nFinal: Cancelado", hidden: true, edge: true),
            ],
            "Un enum y una tabla de transiciones válidas modelan una máquina de estados: cualquier combinación no listada se rechaza.",
            ["Declara el enum al final del programa.", "Un switch sobre la tupla (accion, estado) expresa cada transición en una línea; también puedes usar if anidados."],
            difficulty: 3, requires: [CodeConstruct.Loop], related: ["n1.switch"], targets: [ErrorCategory.EdgeCaseMissed]);

        return new Skill(s, UnitId, "Enumeraciones",
            "Conjuntos cerrados de valores con nombre.",
            Tier.Essential, ["n1.switch"],
            new Lesson(s,
                "Declarar enums, usarlos con switch, convertirlos a número y desde texto, y elegirlos en lugar de strings para estados.",
                [
                    P("Un enum define un tipo con un conjunto cerrado de valores con nombre. Es ideal para estados, categorías o días de la semana."),
                    Code("""
                        Nivel n = Nivel.Intermedio;
                        Console.WriteLine(n);
                        Console.WriteLine((int)n);
                        Console.WriteLine(n == Nivel.Avanzado);
                        if (Enum.TryParse("Avanzado", out Nivel leido))
                        {
                            Console.WriteLine(leido);
                        }

                        enum Nivel { Basico, Intermedio, Avanzado }
                        """, "Declarar, mostrar, convertir"),
                    P("Cada valor tiene un número entero asociado; por defecto empiezan en 0. Puedes asignarlos: enum Talla { S = 1, M, L }."),
                    P("En un programa con instrucciones de nivel superior, los enums se declaran al final, después de las instrucciones."),
                    Design("Cuando agregues un valor nuevo a un enum, revisa todos los switch que lo usan. Un caso _ que devuelve un valor genérico puede ocultar el olvido."),
                ],
                new WorkedExample("Precio según el tipo de boleto",
                    """
                    Boleto[] compra = [Boleto.Adulto, Boleto.Nino, Boleto.Adulto, Boleto.Mayor];
                    decimal total = 0;
                    foreach (Boleto b in compra)
                    {
                        total += Precio(b);
                    }
                    Console.WriteLine($"Total: {total}");

                    decimal Precio(Boleto b) => b switch
                    {
                        Boleto.Adulto => 80m,
                        Boleto.Nino => 50m,
                        Boleto.Mayor => 40m,
                        _ => throw new ArgumentOutOfRangeException(nameof(b)),
                    };

                    enum Boleto { Adulto, Nino, Mayor }
                    """,
                    [
                        Line(1, "Un array de valores del enum."),
                        Line(9, "Un switch con un caso por valor."),
                        Line(14, "Si alguien agrega un tipo de boleto y olvida su precio, el error es visible."),
                        Line(17, "El enum se declara después de las instrucciones."),
                    ],
                    "Total: 250"),
                new CommonMistake(
                    "Representar estados con strings.",
                    "string estado = \"Enviado\";\nif (estado == \"enviado\")\n{\n    Console.WriteLine(\"En camino\");\n}",
                    "La comparación falla por una mayúscula y el compilador no puede detectarlo; cualquier texto es un estado «válido».",
                    "Estado estado = Estado.Enviado;\nif (estado == Estado.Enviado)\n{\n    Console.WriteLine(\"En camino\");\n}\n\nenum Estado { Pendiente, Enviado }"),
                "n2.enums.pred1", "n2.enums.guiado", "n2.enums.indep", "n2.enums.app",
                ["n2.enums.recall1", "n2.enums.recall2", "n2.enums.concepto1"],
                "Modelas conjuntos cerrados de valores con enums y los procesas con switch sin olvidar casos.",
                12));
    }

    private static Skill Tuplas(ContentRegistry r)
    {
        const string s = "n2.tuplas";
        r.Short("n2.tuplas.pred1", s, ItemPurpose.Prediction,
            "¿Qué muestra este programa?",
            "MinMax devuelve una tupla con dos valores con nombre. La deconstrucción (var (a, b) = ...) los separa en dos variables.",
            ["1 9"], match: AnswerMatch.Output,
            code: """
                var (a, b) = MinMax([4, 1, 9, 3]);
                Console.WriteLine($"{a} {b}");

                (int Min, int Max) MinMax(int[] numeros)
                {
                    int min = numeros[0], max = numeros[0];
                    foreach (int n in numeros)
                    {
                        if (n < min) min = n;
                        if (n > max) max = n;
                    }
                    return (min, max);
                }
                """);

        r.Choice("n2.tuplas.pred2", s, ItemPurpose.Prediction,
            "¿Qué muestra este programa?",
            "La deconstrucción con tuplas permite intercambiar dos variables sin una variable auxiliar: se evalúa (y, x) completo y después se asigna.",
            [
                Ok("2 1", "Correcto: los valores se intercambian."),
                No("2 2", "Eso ocurriría con x = y; y = x; en dos pasos, no con la tupla.", ErrorCategory.ConceptMisunderstanding),
                No("1 2", "La asignación sí cambia ambas variables.", ErrorCategory.ConceptMisunderstanding),
            ],
            code: """
                int x = 1, y = 2;
                (x, y) = (y, x);
                Console.WriteLine($"{x} {y}");
                """);

        r.Choice("n2.tuplas.concepto1", s, ItemPurpose.Concept,
            "¿Cuándo es razonable devolver una tupla y cuándo conviene crear un tipo propio (clase o record)?",
            "Las tuplas son prácticas para devolver unos pocos valores relacionados en un uso local. Si el dato se pasa por muchas partes del programa o tiene comportamiento, un tipo con nombre lo documenta mejor.",
            [
                Ok("Tupla para pocos valores en un uso local; un tipo propio si el dato circula por el programa o tiene reglas.", "Correcto."),
                No("Siempre tuplas: los tipos propios son innecesarios.", "Con muchos campos o usos, una tupla se vuelve difícil de entender.", ErrorCategory.ConceptMisunderstanding),
                No("Nunca tuplas: son una mala práctica.", "Son una herramienta útil para casos simples."),
            ],
            dimension: KnowledgeDimension.Design);

        r.Choice("n2.tuplas.recall1", s, ItemPurpose.Recall,
            "¿Cómo se accede al segundo valor de «(string Nombre, int Edad) p»?",
            "Con su nombre, p.Edad. Sin nombres, los elementos se llaman Item1, Item2…",
            [
                Ok("p.Edad (o p.Item2)", "Correcto."),
                No("p[1]", "Las tuplas no se indexan con corchetes.", ErrorCategory.Syntax),
                No("p.Second", "Ese nombre no existe."),
            ]);

        r.Choice("n2.tuplas.bug1", s, ItemPurpose.BugFinding,
            "Este código no compila. ¿Por qué?",
            "El método declara que devuelve (int, int) pero el return entrega un solo int. Debe devolver dos valores: return (cociente, resto);",
            [
                Ok("El return entrega un solo valor y el método promete una tupla de dos.", "Correcto."),
                No("Las tuplas no pueden contener int.", "Pueden contener cualquier tipo.", ErrorCategory.ConceptMisunderstanding),
                No("Falta la palabra tuple.", "La sintaxis (int, int) ya declara la tupla.", ErrorCategory.Syntax),
            ],
            code: """
                (int Cociente, int Resto) Dividir(int a, int b)
                {
                    int cociente = a / b;
                    int resto = a % b;
                    return cociente;
                }
                """,
            targets: [ErrorCategory.TypeMismatch]);

        r.Code("n2.tuplas.guiado", s, ItemPurpose.Guided, CodeMode.Function,
            "Completa Dividir para que devuelva el cociente y el resto de la división entera.",
            """
            public static class Division
            {
                public static (int Cociente, int Resto) Dividir(int a, int b)
                {
                    return (0, 0);
                }
            }
            """,
            """
            public static class Division
            {
                public static (int Cociente, int Resto) Dividir(int a, int b)
                {
                    return (a / b, a % b);
                }
            }
            """,
            [
                Is("17 / 5", "Division.Dividir(17, 5)", "(3, 2)"),
                Is("Exacta", "Division.Dividir(12, 4)", "(3, 0)"),
                Is("Por nombre", "Division.Dividir(9, 2).Resto", "1", hidden: true),
                Throws("Entre cero", "Division.Dividir(1, 0)", "DivideByZeroException", hidden: true),
            ],
            "Una tupla devuelve dos resultados de una sola operación sin crear un tipo nuevo.",
            ["El cociente es a / b y el resto a % b.", "return (a / b, a % b);"],
            related: ["n0.aritmetica"]);

        r.Code("n2.tuplas.indep", s, ItemPurpose.Independent, CodeMode.Function,
            "Escribe Estadisticas(double[] datos) que devuelva (double Min, double Max, double Promedio). Si el array está vacío, lanza ArgumentException.",
            """
            public static class Datos
            {
                public static (double Min, double Max, double Promedio) Estadisticas(double[] datos)
                {
                    throw new NotImplementedException();
                }
            }
            """,
            """
            public static class Datos
            {
                public static (double Min, double Max, double Promedio) Estadisticas(double[] datos)
                {
                    if (datos.Length == 0) throw new ArgumentException("Se necesita al menos un dato", nameof(datos));
                    double min = datos[0], max = datos[0], suma = 0;
                    foreach (double d in datos)
                    {
                        if (d < min) min = d;
                        if (d > max) max = d;
                        suma += d;
                    }
                    return (min, max, suma / datos.Length);
                }
            }
            """,
            [
                Is("Básico", "Datos.Estadisticas([2, 8, 5])", "(2, 8, 5)"),
                Is("Un dato", "Datos.Estadisticas([3.5])", "(3.5, 3.5, 3.5)", edge: true),
                Is("Promedio", "Datos.Estadisticas([1, 2]).Promedio", "1.5", hidden: true),
                Throws("Vacío", "Datos.Estadisticas([])", "ArgumentException", hidden: true),
            ],
            "Un solo recorrido calcula los tres valores; la tupla los entrega juntos.",
            ["Inicializa min y max con el primer elemento.", "return (min, max, suma / datos.Length);"],
            difficulty: 2, requires: [CodeConstruct.Loop], related: ["n2.arrays"]);

        r.Code("n2.tuplas.app", s, ItemPurpose.Transfer, CodeMode.Function,
            "Aplicación: el mejor vendedor. MejorVendedor(string[] ventas) recibe líneas «nombre monto» (un vendedor puede aparecer varias veces). Devuelve (string Nombre, decimal Total) del vendedor con mayor total; en empate, el primero en orden alfabético. Si no hay ventas devuelve (\"\", 0).",
            """
            public static class Ventas
            {
                public static (string Nombre, decimal Total) MejorVendedor(string[] ventas)
                {
                    return ("", 0);
                }
            }
            """,
            """
            public static class Ventas
            {
                public static (string Nombre, decimal Total) MejorVendedor(string[] ventas)
                {
                    Dictionary<string, decimal> totales = [];
                    foreach (string v in ventas)
                    {
                        string[] partes = v.Split(' ');
                        totales[partes[0]] = totales.GetValueOrDefault(partes[0]) + decimal.Parse(partes[1]);
                    }

                    (string Nombre, decimal Total) mejor = ("", 0);
                    List<string> nombres = [.. totales.Keys];
                    nombres.Sort(string.CompareOrdinal);
                    foreach (string n in nombres)
                    {
                        if (totales[n] > mejor.Total)
                        {
                            mejor = (n, totales[n]);
                        }
                    }
                    return mejor;
                }
            }
            """,
            [
                Is("Varios", "Ventas.MejorVendedor([\"ana 100\", \"luis 80\", \"ana 30\", \"luis 60\"])", "(\"luis\", 140)"),
                Is("Uno", "Ventas.MejorVendedor([\"eva 12.5\"])", "(\"eva\", 12.5)", edge: true),
                Is("Empate", "Ventas.MejorVendedor([\"zoe 50\", \"bea 50\"])", "(\"bea\", 50)", hidden: true, edge: true, hint: "En empate gana el primer nombre en orden alfabético."),
                Is("Vacío", "Ventas.MejorVendedor([]).Nombre.Length", "0", hidden: true, edge: true),
            ],
            "El Dictionary acumula; la tupla devuelve el nombre y el total sin crear un tipo.",
            ["Acumula los montos por nombre en un Dictionary<string, decimal>.", "Ordena los nombres y quédate con el primero que tenga el mayor total (usa > para no reemplazar en empate)."],
            difficulty: 3, requires: [CodeConstruct.Loop], related: ["n2.diccionarios"]);

        return new Skill(s, UnitId, "Tuplas",
            "Agrupar valores sin crear un tipo.",
            Tier.Intermediate, ["n2.parametros"],
            new Lesson(s,
                "Crear tuplas con nombres, devolverlas desde métodos, deconstruirlas y decidir cuándo conviene un tipo propio.",
                [
                    P("Una tupla agrupa varios valores en uno solo sin declarar un tipo. Es útil, por ejemplo, para que un método devuelva más de un resultado."),
                    Code("""
                        (string Nombre, int Edad) persona = ("Ana", 30);
                        Console.WriteLine(persona.Nombre);
                        Console.WriteLine(persona.Edad + 1);

                        var (nombre, edad) = persona;
                        Console.WriteLine($"{nombre} tiene {edad}");
                        """, "Crear y deconstruir"),
                    P("Los elementos pueden tener nombre (persona.Nombre) o usarse por posición (Item1, Item2). Deconstruir es separar la tupla en variables."),
                    P("Las tuplas se comparan por valor: (1, 2) == (1, 2) es true."),
                    Design("Si una tupla tiene más de tres elementos o aparece en muchos lugares, probablemente merece ser un record con nombre (lo verás en el Nivel 3)."),
                ],
                new WorkedExample("Devolver éxito y valor en una sola llamada",
                    """
                    var (ok, valor) = LeerPorcentaje("85");
                    Console.WriteLine(ok ? $"Leído: {valor}%" : "Inválido");
                    (ok, valor) = LeerPorcentaje("150");
                    Console.WriteLine(ok ? $"Leído: {valor}%" : "Inválido");

                    (bool Ok, int Valor) LeerPorcentaje(string texto)
                    {
                        if (int.TryParse(texto, out int n) && n >= 0 && n <= 100)
                        {
                            return (true, n);
                        }
                        return (false, 0);
                    }
                    """,
                    [
                        Line(1, "Se deconstruye el resultado en dos variables nuevas."),
                        Line(3, "Se reutilizan las variables existentes."),
                        Line(6, "El tipo de retorno es una tupla con nombres."),
                        Line(10, "return entrega ambos valores a la vez."),
                    ],
                    "Leído: 85%\nInválido"),
                new CommonMistake(
                    "Usar Item1 e Item2 cuando los elementos tienen significado.",
                    "(int, int) Rango() => (1, 10);\nvar r = Rango();\nConsole.WriteLine(r.Item2 - r.Item1);",
                    "Funciona, pero el lector debe adivinar qué es Item1 y qué es Item2.",
                    "(int Desde, int Hasta) Rango() => (1, 10);\nvar r = Rango();\nConsole.WriteLine(r.Hasta - r.Desde);"),
                "n2.tuplas.pred1", "n2.tuplas.guiado", "n2.tuplas.indep", "n2.tuplas.app",
                ["n2.tuplas.recall1", "n2.tuplas.pred2", "n2.tuplas.concepto1"],
                "Devuelves y deconstruyes tuplas con nombres claros y sabes cuándo un tipo propio sería mejor.",
                12));
    }
}
