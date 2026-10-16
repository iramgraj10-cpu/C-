using CSharpMastery.Content.Infrastructure;
using CSharpMastery.Domain.Curriculum;
using CSharpMastery.Domain.Items;
using CSharpMastery.Domain.Progress;
using static CSharpMastery.Content.Infrastructure.ContentRegistry;
using static CSharpMastery.Content.Infrastructure.L;

namespace CSharpMastery.Content.Nivel2;

internal static class U01Metodos
{
    public const string UnitId = "n2.u1";

    public static Unit Build(ContentRegistry r) => new(UnitId, "n2", "Métodos",
        "Declarar, invocar, parámetros, retorno, alcance y sobrecarga.",
        [Metodos(r), Parametros(r), Alcance(r), Sobrecarga(r)]);

    private static Skill Metodos(ContentRegistry r)
    {
        const string s = "n2.metodos";
        r.Choice("n2.metodos.pred1", s, ItemPurpose.Prediction,
            "¿Qué muestra este programa?",
            "Declarar un método no lo ejecuta. Se ejecuta cada vez que se llama, y el programa principal lo llama dos veces antes de mostrar «Fin».",
            [
                Ok("Hola\nHola\nFin", "Correcto: cada llamada ejecuta el cuerpo completo del método."),
                No("Fin\nHola\nHola", "El orden lo deciden las llamadas, no el lugar donde se declara el método.", ErrorCategory.ConceptMisunderstanding),
                No("Hola\nFin", "Hay dos llamadas a Saludar.", ErrorCategory.WrongLogic),
            ],
            code: """
                Saludar();
                Saludar();
                Console.WriteLine("Fin");

                void Saludar()
                {
                    Console.WriteLine("Hola");
                }
                """);

        r.Choice("n2.metodos.concepto1", s, ItemPurpose.Concept,
            "¿Cuál es la principal razón para extraer un fragmento de código a un método?",
            "Un método da nombre a una tarea. Así el programa se lee como una lista de pasos, el código no se repite y un cambio se hace en un solo lugar.",
            [
                Ok("Dar nombre a una tarea, evitar repetir código y poder cambiarla en un solo lugar.", "Correcto."),
                No("Que el programa se ejecute más rápido.", "Llamar a un método no acelera el programa; el beneficio es la organización."),
                No("Es obligatorio: todo código debe estar dentro de un método con nombre propio.", "Puedes escribir instrucciones de nivel superior; los métodos son una herramienta de organización."),
            ],
            dimension: KnowledgeDimension.Design);

        r.Choice("n2.metodos.recall1", s, ItemPurpose.Recall,
            "¿Qué significa la palabra void en «void MostrarMenu()»?",
            "void indica que el método no devuelve ningún valor: hace algo (por ejemplo, mostrar texto) pero no produce un resultado para quien lo llama.",
            [
                Ok("Que el método no devuelve ningún valor.", "Correcto."),
                No("Que el método no recibe parámetros.", "Los paréntesis vacíos indican eso; void habla del valor de retorno."),
                No("Que el método está vacío.", "Un método void puede tener muchas instrucciones."),
            ]);

        r.Choice("n2.metodos.recall2", s, ItemPurpose.Recall,
            "¿Qué ocurre con un método que se declara pero nunca se llama?",
            "Su código existe pero nunca se ejecuta. Declarar es definir la receta; llamar es cocinarla.",
            [
                Ok("Nunca se ejecuta.", "Correcto."),
                No("Se ejecuta una vez al iniciar el programa.", "Solo se ejecuta cuando alguien lo llama."),
                No("El programa no compila.", "Compila; como mucho el entorno te avisa de que no se usa."),
            ]);

        r.Choice("n2.metodos.bug1", s, ItemPurpose.BugFinding,
            "Este programa debería mostrar una línea de guiones, pero no compila. ¿Por qué?",
            "Para llamar a un método hacen falta los paréntesis: Linea();. Sin ellos, Linea es solo el nombre del método y no una instrucción válida.",
            [
                Ok("Falta escribir los paréntesis al llamar: Linea();", "Correcto."),
                No("Los métodos deben declararse antes de usarse.", "En C# un método local puede declararse después de la llamada.", ErrorCategory.ConceptMisunderstanding),
                No("Un método void no puede usar Console.WriteLine.", "Puede usar cualquier instrucción.", ErrorCategory.ConceptMisunderstanding),
            ],
            code: """
                Linea;

                void Linea()
                {
                    Console.WriteLine("----------");
                }
                """,
            targets: [ErrorCategory.Syntax]);

        r.Code("n2.metodos.guiado", s, ItemPurpose.Guided, CodeMode.Program,
            "Completa el método MostrarMenu para que muestre tres líneas: «1. Jugar», «2. Opciones» y «3. Salir». El programa ya lo llama dos veces separadas por «---».",
            """
            MostrarMenu();
            Console.WriteLine("---");
            MostrarMenu();

            void MostrarMenu()
            {
                // escribe aquí las tres líneas
            }
            """,
            """
            MostrarMenu();
            Console.WriteLine("---");
            MostrarMenu();

            void MostrarMenu()
            {
                Console.WriteLine("1. Jugar");
                Console.WriteLine("2. Opciones");
                Console.WriteLine("3. Salir");
            }
            """,
            [
                In("Menú dos veces", "", "1. Jugar\n2. Opciones\n3. Salir\n---\n1. Jugar\n2. Opciones\n3. Salir"),
            ],
            "Escribes las tres líneas una sola vez; las dos llamadas reutilizan el mismo código.",
            ["Dentro de las llaves del método escribe tres Console.WriteLine.", "No cambies las llamadas de arriba: ya están listas."],
            requires: [CodeConstruct.Method]);

        r.Code("n2.metodos.indep", s, ItemPurpose.Independent, CodeMode.Program,
            "Declara un método Separador() que muestre «==========» (10 signos =). Lee un nombre y muestra: separador, «Bienvenido, NOMBRE», separador. Usa el método, no repitas el texto.",
            """
            string nombre = Console.ReadLine()!;
            """,
            """
            string nombre = Console.ReadLine()!;
            Separador();
            Console.WriteLine($"Bienvenido, {nombre}");
            Separador();

            void Separador()
            {
                Console.WriteLine("==========");
            }
            """,
            [
                In("Ana", "Ana", "==========\nBienvenido, Ana\n=========="),
                In("Otro nombre", "Luis", "==========\nBienvenido, Luis\n==========", hidden: true),
            ],
            "Un método local puede usarse antes de su declaración dentro del mismo programa.",
            ["Declara void Separador() { ... } al final del archivo.", "Llámalo con Separador(); antes y después del saludo."],
            requires: [CodeConstruct.Method]);

        r.Code("n2.metodos.app", s, ItemPurpose.Transfer, CodeMode.Program,
            "Aplicación: un ticket de compra. Lee nombres de productos (uno por línea) hasta la palabra «fin». Muestra un encabezado «TIENDA», una línea de 20 guiones, cada producto como «- producto», otra línea de 20 guiones y «Artículos: N». Crea un método para la línea de guiones.",
            """
            string linea = Console.ReadLine()!;
            """,
            """
            List<string> productos = [];
            string linea = Console.ReadLine()!;
            while (linea != "fin")
            {
                productos.Add(linea);
                linea = Console.ReadLine()!;
            }

            Console.WriteLine("TIENDA");
            Linea();
            foreach (string p in productos)
            {
                Console.WriteLine($"- {p}");
            }
            Linea();
            Console.WriteLine($"Artículos: {productos.Count}");

            void Linea()
            {
                Console.WriteLine(new string('-', 20));
            }
            """,
            [
                In("Dos productos", "pan\nleche\nfin", "TIENDA\n--------------------\n- pan\n- leche\n--------------------\nArtículos: 2"),
                In("Sin productos", "fin", "TIENDA\n--------------------\n--------------------\nArtículos: 0", hidden: true, edge: true),
                In("Varios", "a\nb\nc\nfin", "TIENDA\n--------------------\n- a\n- b\n- c\n--------------------\nArtículos: 3", hidden: true),
            ],
            "Leer todo primero y mostrar después separa la entrada de la presentación; el método evita escribir la línea de guiones dos veces.",
            ["Guarda los productos en una lista o cuenta y muestra cada uno al leerlo después del encabezado.", "new string('-', 20) crea 20 guiones."],
            difficulty: 2, requires: [CodeConstruct.Method, CodeConstruct.Loop], related: ["n1.while"]);

        return new Skill(s, UnitId, "Declaración e invocación de métodos",
            "Dividir un programa en piezas con nombre.",
            Tier.Essential, ["n1.acumuladores"],
            new Lesson(s,
                "Declarar métodos sin parámetros, llamarlos y entender que declarar no es ejecutar.",
                [
                    P("Un método es un bloque de código con nombre. Se declara una vez y se ejecuta cada vez que se llama escribiendo su nombre seguido de paréntesis."),
                    Code("""
                        Saludo();
                        Saludo();

                        void Saludo()
                        {
                            Console.WriteLine("¡Hola!");
                        }
                        """, "Una declaración, dos llamadas"),
                    P("void significa que el método no devuelve ningún valor. En la lección siguiente verás métodos que reciben datos y devuelven resultados."),
                    P("Cuando el programa llega a una llamada, salta al método, ejecuta su cuerpo y vuelve justo después de la llamada."),
                    Note("En un programa con instrucciones de nivel superior (sin Main), los métodos que declaras ahí son métodos locales y pueden escribirse después de usarse."),
                    Design("Nombra los métodos con un verbo que describa lo que hacen: MostrarMenu, ImprimirTicket, LimpiarPantalla."),
                ],
                new WorkedExample("Un programa que se lee como una lista de pasos",
                    """
                    MostrarTitulo();
                    MostrarInstrucciones();
                    MostrarTitulo();

                    void MostrarTitulo()
                    {
                        Console.WriteLine("*** ADIVINA ***");
                    }

                    void MostrarInstrucciones()
                    {
                        Console.WriteLine("Piensa un número del 1 al 10.");
                    }
                    """,
                    [
                        Line(1, "Primera llamada: salta a MostrarTitulo y vuelve."),
                        Line(2, "Las tres primeras líneas resumen el programa."),
                        Line(5, "La declaración no se ejecuta por sí misma."),
                        Line(10, "Cada método se ocupa de una sola tarea."),
                    ],
                    "*** ADIVINA ***\nPiensa un número del 1 al 10.\n*** ADIVINA ***"),
                new CommonMistake(
                    "Olvidar los paréntesis al llamar al método.",
                    "MostrarTitulo;\n\nvoid MostrarTitulo()\n{\n    Console.WriteLine(\"Título\");\n}",
                    "Sin paréntesis no hay llamada: el compilador ve solo un nombre y rechaza la instrucción (CS0201).",
                    "MostrarTitulo();\n\nvoid MostrarTitulo()\n{\n    Console.WriteLine(\"Título\");\n}"),
                "n2.metodos.pred1", "n2.metodos.guiado", "n2.metodos.indep", "n2.metodos.app",
                ["n2.metodos.recall1", "n2.metodos.recall2", "n2.metodos.concepto1"],
                "Organizas un programa en métodos con nombres claros, los llamas correctamente y predices el orden de ejecución.",
                12));
    }

    private static Skill Parametros(ContentRegistry r)
    {
        const string s = "n2.parametros";
        r.Short("n2.parametros.pred1", s, ItemPurpose.Prediction,
            "¿Qué muestra este programa?",
            "Doble(4) devuelve 8 y Doble(8) devuelve 16. La llamada interior se evalúa primero y su resultado es el argumento de la exterior.",
            ["16"], match: AnswerMatch.Number,
            code: """
                Console.WriteLine(Doble(Doble(4)));

                int Doble(int n)
                {
                    return n * 2;
                }
                """,
            wrong: new() { ["8"] = "Doble se llama dos veces: el resultado de la primera llamada es la entrada de la segunda." });

        r.Choice("n2.parametros.pred2", s, ItemPurpose.Prediction,
            "¿Qué muestra este programa?",
            "Un int se pasa por valor: el método recibe una copia. Cambiar n dentro de Incrementar no modifica la variable x del programa principal.",
            [
                Ok("5", "Correcto: el método modifica su propia copia."),
                No("6", "El método cambió su copia; x sigue valiendo 5.", ErrorCategory.ConceptMisunderstanding),
                No("No compila", "Es código válido; simplemente no tiene el efecto que parece.", ErrorCategory.ConceptMisunderstanding),
            ],
            code: """
                int x = 5;
                Incrementar(x);
                Console.WriteLine(x);

                void Incrementar(int n)
                {
                    n = n + 1;
                }
                """,
            difficulty: 2);

        r.Choice("n2.parametros.recall1", s, ItemPurpose.Recall,
            "En «int Sumar(int a, int b)», ¿qué indica el primer int?",
            "El tipo que aparece antes del nombre es el tipo del valor de retorno: Sumar devuelve un int.",
            [
                Ok("El tipo del valor que devuelve el método.", "Correcto."),
                No("El tipo del primer parámetro.", "Ese es el int que aparece dentro de los paréntesis, antes de a."),
                No("Que el método solo se puede llamar una vez.", "No existe tal restricción."),
            ]);

        r.Choice("n2.parametros.recall2", s, ItemPurpose.Recall,
            "¿Qué hace la instrucción return dentro de un método?",
            "return termina el método inmediatamente y entrega el valor indicado a quien lo llamó.",
            [
                Ok("Termina el método y entrega un valor a quien lo llamó.", "Correcto."),
                No("Muestra el valor en la consola.", "return no muestra nada; quien llama decide qué hacer con el valor.", ErrorCategory.ConceptMisunderstanding),
                No("Vuelve a ejecutar el método desde el principio.", "Eso sería un bucle o recursividad."),
            ]);

        r.Choice("n2.parametros.bug1", s, ItemPurpose.BugFinding,
            "Este método no compila: «no todas las rutas de código devuelven un valor». ¿Por qué?",
            "Si n no es mayor que 0 (por ejemplo, 0 o un negativo) el método llega al final sin return. Un método que no es void debe devolver un valor en todos los caminos.",
            [
                Ok("Si n <= 0 el método termina sin devolver nada.", "Correcto: agrega un return para ese caso."),
                No("No se puede usar if dentro de un método que devuelve int.", "Sí se puede, mientras todos los caminos terminen en return.", ErrorCategory.ConceptMisunderstanding),
                No("Falta void antes de string.", "El método debe devolver texto; void diría que no devuelve nada.", ErrorCategory.ConceptMisunderstanding),
            ],
            code: """
                string Signo(int n)
                {
                    if (n > 0)
                    {
                        return "positivo";
                    }
                }
                """,
            targets: [ErrorCategory.MissingReturn]);

        r.Code("n2.parametros.guiado", s, ItemPurpose.Guided, CodeMode.Function,
            "Completa el método Area para que devuelva el área de un rectángulo (base × altura).",
            """
            public static class Geometria
            {
                public static double Area(double ancho, double alto)
                {
                    return 0; // reemplaza por el cálculo
                }
            }
            """,
            """
            public static class Geometria
            {
                public static double Area(double ancho, double alto)
                {
                    return ancho * alto;
                }
            }
            """,
            [
                Is("3 × 4", "Geometria.Area(3, 4)", "12"),
                Is("Decimales", "Geometria.Area(2.5, 2)", "5"),
                Is("Lado cero", "Geometria.Area(0, 7)", "0", hidden: true, edge: true),
            ],
            "Los parámetros son variables que ya traen el valor que puso quien llama.",
            ["Usa los nombres de los parámetros: ancho y alto.", "return ancho * alto;"],
            requires: [CodeConstruct.Method]);

        r.Code("n2.parametros.indep", s, ItemPurpose.Independent, CodeMode.Function,
            "Escribe Maximo(int a, int b, int c) que devuelva el mayor de los tres números, sin usar Math.Max.",
            """
            public static class Numeros
            {
                public static int Maximo(int a, int b, int c)
                {
                    throw new NotImplementedException();
                }
            }
            """,
            """
            public static class Numeros
            {
                public static int Maximo(int a, int b, int c)
                {
                    int mayor = a;
                    if (b > mayor) mayor = b;
                    if (c > mayor) mayor = c;
                    return mayor;
                }
            }
            """,
            [
                Is("Al final", "Numeros.Maximo(1, 2, 3)", "3"),
                Is("Al inicio", "Numeros.Maximo(9, 2, 3)", "9"),
                Is("En medio", "Numeros.Maximo(1, 8, 3)", "8", hidden: true),
                Is("Negativos", "Numeros.Maximo(-5, -2, -9)", "-2", hidden: true, edge: true, hint: "No empieces con mayor = 0: con números negativos sería incorrecto."),
                Is("Iguales", "Numeros.Maximo(4, 4, 4)", "4", hidden: true, edge: true),
            ],
            "Empezar suponiendo que el primero es el mayor funciona con cualquier rango de valores.",
            ["Guarda el primero como candidato y compáralo con los otros dos.", "Recuerda devolver el resultado con return."],
            difficulty: 2, requires: [CodeConstruct.Method, CodeConstruct.Conditional], targets: [ErrorCategory.EdgeCaseMissed]);

        r.Code("n2.parametros.app", s, ItemPurpose.Transfer, CodeMode.Function,
            "Aplicación: una tienda calcula el total a pagar. Escribe Total(decimal precio, int cantidad, bool esSocio). El subtotal es precio × cantidad. Si la cantidad es 10 o más, hay 10 % de descuento. Si es socio, un 5 % adicional sobre lo ya descontado. Devuelve el total redondeado a 2 decimales (Math.Round). Si la cantidad es negativa, lanza ArgumentOutOfRangeException.",
            """
            public static class Caja
            {
                public static decimal Total(decimal precio, int cantidad, bool esSocio)
                {
                    return precio * cantidad;
                }
            }
            """,
            """
            public static class Caja
            {
                public static decimal Total(decimal precio, int cantidad, bool esSocio)
                {
                    if (cantidad < 0)
                    {
                        throw new ArgumentOutOfRangeException(nameof(cantidad));
                    }

                    decimal total = precio * cantidad;
                    if (cantidad >= 10)
                    {
                        total *= 0.90m;
                    }
                    if (esSocio)
                    {
                        total *= 0.95m;
                    }
                    return Math.Round(total, 2);
                }
            }
            """,
            [
                Is("Sin descuentos", "Caja.Total(12.50m, 2, false)", "25.00"),
                Is("Por volumen", "Caja.Total(10m, 10, false)", "90.00", hint: "decimal conserva los decimales de la operación: 100 × 0.90 = 90.00."),
                Is("Volumen y socio", "Caja.Total(10m, 10, true)", "85.50", hidden: true),
                Is("Solo socio", "Caja.Total(3.33m, 3, true)", "9.49", hidden: true, edge: true),
                Throws("Cantidad negativa", "Caja.Total(5m, -1, false)", "ArgumentOutOfRangeException", hidden: true),
            ],
            "Cada regla del negocio es un paso; validar los argumentos al inicio evita resultados absurdos.",
            ["Valida primero la cantidad y lanza la excepción con throw.", "Aplica los descuentos multiplicando: 0.90m y 0.95m (la m indica decimal)."],
            difficulty: 3, requires: [CodeConstruct.Method], related: ["n1.if-else"], targets: [ErrorCategory.EdgeCaseMissed]);

        return new Skill(s, UnitId, "Parámetros y valores de retorno",
            "Pasar datos a un método y obtener un resultado.",
            Tier.Essential, ["n2.metodos"],
            new Lesson(s,
                "Declarar métodos con parámetros y tipo de retorno, devolver valores en todos los caminos y distinguir parámetro de argumento.",
                [
                    P("Los parámetros son variables que el método recibe al ser llamado. Los valores concretos que se pasan en la llamada se llaman argumentos."),
                    Code("""
                        int resultado = Sumar(3, 4);
                        Console.WriteLine(resultado);

                        int Sumar(int a, int b)
                        {
                            return a + b;
                        }
                        """, "a y b son parámetros; 3 y 4 son argumentos"),
                    P("El tipo antes del nombre (int en Sumar) es el tipo del valor que se devuelve. return termina el método y entrega ese valor."),
                    P("Los tipos simples como int, double o bool se pasan por valor: el método recibe una copia y modificarla no afecta a la variable original."),
                    Warn("Un método que no es void debe terminar con return en todos los caminos posibles; si no, no compila (CS0161)."),
                    Design("Prefiere métodos que devuelven un resultado a métodos que lo muestran: así puedes reutilizar el cálculo y probarlo."),
                ],
                new WorkedExample("Calcular el promedio en un método",
                    """
                    double p = Promedio(7, 9, 8);
                    Console.WriteLine($"Promedio: {p:F1}");
                    Console.WriteLine(Aprobado(p) ? "Aprobado" : "Reprobado");

                    double Promedio(double a, double b, double c)
                    {
                        return (a + b + c) / 3;
                    }

                    bool Aprobado(double promedio)
                    {
                        return promedio >= 6;
                    }
                    """,
                    [
                        Line(1, "El valor devuelto se guarda en una variable."),
                        Line(3, "El resultado de un método puede usarse directamente en una expresión."),
                        Line(7, "return entrega el cálculo a quien llamó."),
                        Line(12, "Un método puede devolver bool: ideal para preguntas de sí o no."),
                    ],
                    "Promedio: 8.0\nAprobado"),
                new CommonMistake(
                    "Mostrar el resultado dentro del método en vez de devolverlo.",
                    "void Doble(int n)\n{\n    Console.WriteLine(n * 2);\n}\n\nint x = Doble(5); // no compila",
                    "Un método void no devuelve nada, así que su resultado no se puede guardar ni reutilizar.",
                    "int x = Doble(5);\nConsole.WriteLine(x);\n\nint Doble(int n)\n{\n    return n * 2;\n}"),
                "n2.parametros.pred1", "n2.parametros.guiado", "n2.parametros.indep", "n2.parametros.app",
                ["n2.parametros.recall1", "n2.parametros.recall2", "n2.parametros.pred2"],
                "Diseñas métodos con parámetros y retorno adecuados, devuelves valores en todos los caminos y validas los argumentos.",
                15));
    }

    private static Skill Alcance(ContentRegistry r)
    {
        const string s = "n2.alcance";
        r.Choice("n2.alcance.pred1", s, ItemPurpose.Prediction,
            "¿Qué ocurre con este programa?",
            "total se declara dentro de las llaves del if, así que solo existe ahí. Fuera del bloque el nombre no está definido y el compilador rechaza el programa (CS0103).",
            [
                Ok("No compila: total no existe fuera del bloque del if.", "Correcto: cada variable vive dentro de las llaves donde se declaró."),
                No("Muestra 10", "total dejó de existir al cerrar la llave del if.", ErrorCategory.Scope),
                No("Muestra 0", "C# no crea variables automáticamente con valor por defecto.", ErrorCategory.Scope),
            ],
            code: """
                int x = 5;
                if (x > 0)
                {
                    int total = x * 2;
                }
                Console.WriteLine(total);
                """,
            targets: [ErrorCategory.Scope]);

        r.Choice("n2.alcance.concepto1", s, ItemPurpose.Concept,
            "Dos métodos distintos declaran cada uno una variable llamada contador. ¿Qué pasa?",
            "Cada método tiene su propio alcance. Son dos variables distintas que comparten nombre; no hay conflicto ni se afectan entre sí.",
            [
                Ok("Son variables independientes; cada una existe solo en su método.", "Correcto."),
                No("Error de compilación por nombre repetido.", "El conflicto solo ocurre si ambas están en el mismo alcance.", ErrorCategory.Scope),
                No("Comparten el valor: cambiar una cambia la otra.", "No hay relación entre ellas.", ErrorCategory.Scope),
            ],
            targets: [ErrorCategory.Scope]);

        r.Choice("n2.alcance.recall1", s, ItemPurpose.Recall,
            "¿Cuál es la regla del alcance de una variable local?",
            "Una variable local existe desde su declaración hasta la llave que cierra el bloque donde se declaró.",
            [
                Ok("Existe desde su declaración hasta el cierre del bloque { } que la contiene.", "Correcto."),
                No("Existe en todo el programa una vez declarada.", "Eso sería una variable global, que C# no tiene."),
                No("Existe hasta que se le asigna otro valor.", "Asignar no termina la vida de la variable."),
            ]);

        r.Choice("n2.alcance.recall2", s, ItemPurpose.Recall,
            "En un for (int i = 0; i < n; i++), ¿dónde se puede usar i?",
            "La variable declarada en la cabecera del for solo existe dentro del for (cabecera y cuerpo).",
            [
                Ok("Solo dentro del for.", "Correcto."),
                No("En todo el método después del for.", "Al terminar el for, i deja de existir."),
                No("Solo en la condición.", "También en el cuerpo y en el incremento."),
            ]);

        r.Choice("n2.alcance.bug1", s, ItemPurpose.BugFinding,
            "Este programa quiere contar los números pares, pero siempre muestra 0 (o 1). ¿Cuál es el error?",
            "contador se declara dentro del bucle, así que se crea de nuevo (con 0) en cada vuelta. Tampoco existe después del bucle; hay que declararlo antes del for.",
            [
                Ok("contador se declara dentro del bucle y se reinicia en cada vuelta.", "Correcto: decláralo antes del bucle."),
                No("La condición n % 2 == 0 está mal.", "Es la forma correcta de detectar pares.", ErrorCategory.WrongLogic),
                No("Falta un break.", "El problema es dónde vive contador.", ErrorCategory.WrongLogic),
            ],
            code: """
                int[] numeros = [2, 4, 5, 6];
                foreach (int n in numeros)
                {
                    int contador = 0;
                    if (n % 2 == 0) contador++;
                }
                """,
            targets: [ErrorCategory.Scope]);

        r.Code("n2.alcance.guiado", s, ItemPurpose.Guided, CodeMode.Program,
            "El programa no compila porque mayor se declara dentro del bucle. Muévela para que muestre el mayor de los números leídos (una línea con números separados por espacios).",
            """
            string[] partes = Console.ReadLine()!.Split(' ');
            foreach (string p in partes)
            {
                int mayor = int.MinValue;
                int n = int.Parse(p);
                if (n > mayor) mayor = n;
            }
            Console.WriteLine(mayor);
            """,
            """
            string[] partes = Console.ReadLine()!.Split(' ');
            int mayor = int.MinValue;
            foreach (string p in partes)
            {
                int n = int.Parse(p);
                if (n > mayor) mayor = n;
            }
            Console.WriteLine(mayor);
            """,
            [
                In("Varios", "3 9 2", "9"),
                In("Negativos", "-4 -1 -7", "-1", hidden: true, edge: true),
            ],
            "Una variable que debe acumular entre vueltas se declara antes del bucle.",
            ["Mueve la línea int mayor = int.MinValue; antes del foreach.", "n puede quedarse dentro: solo se necesita en cada vuelta."],
            targets: [ErrorCategory.Scope]);

        r.Code("n2.alcance.indep", s, ItemPurpose.Independent, CodeMode.Function,
            "Escribe ContarVocales(string texto) que devuelva cuántas vocales (a, e, i, o, u, en minúscula o mayúscula, sin acentos) tiene el texto. Declara cada variable en el alcance más pequeño posible.",
            """
            public static class Texto
            {
                public static int ContarVocales(string texto)
                {
                    return -1;
                }
            }
            """,
            """
            public static class Texto
            {
                public static int ContarVocales(string texto)
                {
                    int cuenta = 0;
                    foreach (char c in texto)
                    {
                        char m = char.ToLower(c);
                        if (m == 'a' || m == 'e' || m == 'i' || m == 'o' || m == 'u')
                        {
                            cuenta++;
                        }
                    }
                    return cuenta;
                }
            }
            """,
            [
                Is("Palabra", "Texto.ContarVocales(\"programa\")", "3"),
                Is("Mayúsculas", "Texto.ContarVocales(\"HOLA Mundo\")", "4"),
                Is("Vacío", "Texto.ContarVocales(\"\")", "0", hidden: true, edge: true),
                Is("Sin vocales", "Texto.ContarVocales(\"xyz 123\")", "0", hidden: true, edge: true),
            ],
            "cuenta vive en todo el método porque acumula; m solo en cada vuelta porque se recalcula.",
            ["Declara el contador antes del foreach.", "char.ToLower(c) permite comparar solo con minúsculas."],
            difficulty: 2, requires: [CodeConstruct.Loop], related: ["n1.acumuladores"]);

        r.Code("n2.alcance.app", s, ItemPurpose.Transfer, CodeMode.Program,
            "Aplicación: lee calificaciones de varios grupos. Cada línea tiene las calificaciones de un grupo separadas por espacios. Por cada grupo muestra «Grupo k: promedio» con un decimal. Al final muestra «General: promedio» de todas las calificaciones. Ojo: unas variables deben reiniciarse por grupo y otras no.",
            """
            string? linea = Console.ReadLine();
            """,
            """
            string? linea = Console.ReadLine();
            int grupo = 0;
            double sumaTotal = 0;
            int cuentaTotal = 0;
            while (linea is not null && linea != "")
            {
                grupo++;
                double suma = 0;
                string[] partes = linea.Split(' ');
                foreach (string p in partes)
                {
                    suma += double.Parse(p);
                }
                Console.WriteLine($"Grupo {grupo}: {suma / partes.Length:F1}");
                sumaTotal += suma;
                cuentaTotal += partes.Length;
                linea = Console.ReadLine();
            }
            Console.WriteLine($"General: {sumaTotal / cuentaTotal:F1}");
            """,
            [
                In("Dos grupos", "8 9 10\n6 7", "Grupo 1: 9.0\nGrupo 2: 6.5\nGeneral: 8.0"),
                In("Un grupo", "5", "Grupo 1: 5.0\nGeneral: 5.0", hidden: true, edge: true),
                In("Tres grupos", "10 10\n0\n7 8 9", "Grupo 1: 10.0\nGrupo 2: 0.0\nGrupo 3: 8.0\nGeneral: 7.3", hidden: true, hint: "El promedio general se calcula con todas las calificaciones, no promediando los promedios."),
            ],
            "La suma del grupo vive dentro del bucle (se reinicia); la suma total vive fuera (acumula todo).",
            ["Declara suma y cuenta del grupo dentro del while; las totales, antes.", "El promedio general es sumaTotal / cuentaTotal, no el promedio de los promedios."],
            difficulty: 3, requires: [CodeConstruct.Loop], related: ["n1.acumuladores"], targets: [ErrorCategory.Scope, ErrorCategory.WrongLogic]);

        return new Skill(s, UnitId, "Alcance de variables",
            "Dónde existe cada variable.",
            Tier.Essential, ["n2.parametros"],
            new Lesson(s,
                "Determinar dónde existe una variable, declararla en el alcance adecuado y distinguir variables con el mismo nombre en métodos distintos.",
                [
                    P("El alcance (scope) de una variable es la región del código donde existe. Para una variable local va desde su declaración hasta la llave } que cierra su bloque."),
                    Code("""
                        int afuera = 1;
                        {
                            int adentro = 2;
                            Console.WriteLine(afuera + adentro);
                        }
                        Console.WriteLine(afuera);
                        """, "adentro deja de existir al cerrar la llave"),
                    P("Los parámetros y las variables de un método solo existen dentro de ese método. Otro método puede usar los mismos nombres sin conflicto."),
                    P("Regla práctica: si una variable debe conservar su valor entre vueltas de un bucle (un acumulador), se declara antes del bucle. Si se recalcula en cada vuelta, dentro."),
                    Design("Declara cada variable en el alcance más pequeño posible: así hay menos lugares donde puede cambiar por error."),
                ],
                new WorkedExample("Variables por vuelta y variables acumuladas",
                    """
                    int[][] semanas = [[2, 3, 1], [4, 0, 5]];
                    int total = 0;
                    for (int i = 0; i < semanas.Length; i++)
                    {
                        int semana = 0;
                        foreach (int horas in semanas[i])
                        {
                            semana += horas;
                        }
                        Console.WriteLine($"Semana {i + 1}: {semana} h");
                        total += semana;
                    }
                    Console.WriteLine($"Total: {total} h");
                    """,
                    [
                        Line(2, "total acumula durante todo el programa: se declara fuera."),
                        Line(5, "semana se reinicia en cada vuelta: se declara dentro."),
                        Line(6, "horas solo existe dentro del foreach."),
                        Line(13, "Aquí semana e i ya no existen; total sí."),
                    ],
                    "Semana 1: 6 h\nSemana 2: 9 h\nTotal: 15 h"),
                new CommonMistake(
                    "Declarar el acumulador dentro del bucle.",
                    "foreach (int n in numeros)\n{\n    int suma = 0;\n    suma += n;\n}\nConsole.WriteLine(suma);",
                    "suma se crea con 0 en cada vuelta y además no existe después del bucle: el programa no compila.",
                    "int[] numeros = [1, 2, 3];\nint suma = 0;\nforeach (int n in numeros)\n{\n    suma += n;\n}\nConsole.WriteLine(suma);"),
                "n2.alcance.pred1", "n2.alcance.guiado", "n2.alcance.indep", "n2.alcance.app",
                ["n2.alcance.recall1", "n2.alcance.recall2", "n2.alcance.concepto1"],
                "Predices si una variable existe en un punto del código y eliges dónde declararla según su propósito.",
                12));
    }

    private static Skill Sobrecarga(ContentRegistry r)
    {
        const string s = "n2.sobrecarga";
        r.Choice("n2.sobrecarga.pred1", s, ItemPurpose.Prediction,
            "¿Qué muestra este programa?",
            "El compilador elige la versión según los tipos de los argumentos: 2 y 3 son int, así que se usa Describir(int); 2.5 es double y se usa Describir(double).",
            [
                Ok("entero 5\ndecimal 2.5", "Correcto: se elige por el tipo del argumento."),
                No("decimal 5\ndecimal 2.5", "Existe una versión exacta para int; esa es la preferida.", ErrorCategory.TypeMismatch),
                No("No compila: dos métodos con el mismo nombre", "Se permite si las listas de parámetros son distintas.", ErrorCategory.ConceptMisunderstanding),
            ],
            code: """
                Info.Describir(2 + 3);
                Info.Describir(2.5);

                static class Info
                {
                    public static void Describir(int n) => Console.WriteLine($"entero {n}");
                    public static void Describir(double n) => Console.WriteLine($"decimal {n}");
                }
                """,
            difficulty: 2);

        r.Choice("n2.sobrecarga.concepto1", s, ItemPurpose.Concept,
            "¿Cuál de estos pares NO es una sobrecarga válida?",
            "Los métodos sobrecargados deben diferir en sus parámetros (cantidad, tipo u orden de tipos). Cambiar solo el tipo de retorno no basta, porque en la llamada no se ve cuál se quiere.",
            [
                Ok("int Medir(string s) y double Medir(string s)", "Correcto: solo difieren en el tipo de retorno."),
                No("int Sumar(int a, int b) y int Sumar(int a, int b, int c)", "Difieren en la cantidad de parámetros: es válida."),
                No("void Mostrar(int x) y void Mostrar(string x)", "Difieren en el tipo del parámetro: es válida."),
            ],
            targets: [ErrorCategory.ConceptMisunderstanding]);

        r.Choice("n2.sobrecarga.recall1", s, ItemPurpose.Recall,
            "¿Qué forma la «firma» de un método para la sobrecarga?",
            "El nombre y la lista de tipos de parámetros (en orden). El tipo de retorno y los nombres de los parámetros no cuentan.",
            [
                Ok("El nombre y los tipos de sus parámetros, en orden.", "Correcto."),
                No("El nombre y el tipo de retorno.", "El tipo de retorno no distingue sobrecargas."),
                No("Los nombres de los parámetros.", "Cambiar solo los nombres no crea otra firma."),
            ]);

        r.Choice("n2.sobrecarga.recall2", s, ItemPurpose.Recall,
            "¿Qué es un parámetro opcional, como en «void Saludar(string nombre = \"invitado\")»?",
            "Es un parámetro con valor por defecto: si quien llama no lo indica, se usa ese valor. Es una alternativa a escribir una sobrecarga más.",
            [
                Ok("Un parámetro con valor por defecto que se puede omitir en la llamada.", "Correcto."),
                No("Un parámetro que puede ser null.", "Eso depende del tipo, no de que sea opcional."),
                No("Un parámetro que el método puede ignorar.", "El método lo recibe siempre, con el valor dado o el de por defecto."),
            ]);

        r.Choice("n2.sobrecarga.bug1", s, ItemPurpose.BugFinding,
            "Este código no compila. ¿Por qué?",
            "Los dos métodos tienen el mismo nombre y los mismos tipos de parámetros; solo cambia el nombre del parámetro, que no forma parte de la firma.",
            [
                Ok("Ambos métodos tienen la misma firma: el nombre del parámetro no cuenta.", "Correcto."),
                No("No se pueden declarar métodos que devuelven double.", "Sí se pueden.", ErrorCategory.ConceptMisunderstanding),
                No("Falta la palabra overload.", "C# no tiene esa palabra; la sobrecarga es implícita.", ErrorCategory.Syntax),
            ],
            code: """
                static class Figuras
                {
                    public static double Area(double lado) => lado * lado;
                    public static double Area(double radio) => Math.PI * radio * radio;
                }
                """);

        r.Code("n2.sobrecarga.guiado", s, ItemPurpose.Guided, CodeMode.Function,
            "Ya existe Area(double lado) para un cuadrado. Agrega una sobrecarga Area(double ancho, double alto) para un rectángulo.",
            """
            public static class Figuras
            {
                public static double Area(double lado) => lado * lado;

                // agrega aquí la sobrecarga para rectángulos
            }
            """,
            """
            public static class Figuras
            {
                public static double Area(double lado) => lado * lado;

                public static double Area(double ancho, double alto) => ancho * alto;
            }
            """,
            [
                Is("Cuadrado", "Figuras.Area(3)", "9"),
                Is("Rectángulo", "Figuras.Area(3, 5)", "15"),
                Is("Rectángulo decimal", "Figuras.Area(1.5, 4)", "6", hidden: true),
            ],
            "El compilador elige la versión según cuántos argumentos se pasan.",
            ["Mismo nombre, Area, con dos parámetros double.", "public static double Area(double ancho, double alto) => ancho * alto;"],
            requires: [CodeConstruct.Method]);

        r.Code("n2.sobrecarga.indep", s, ItemPurpose.Independent, CodeMode.Function,
            "Escribe dos sobrecargas de Repetir: Repetir(string texto, int veces) devuelve el texto repetido «veces» veces; Repetir(char c, int veces) devuelve el carácter repetido. Si veces es negativo, ambas lanzan ArgumentOutOfRangeException.",
            """
            public static class Cadenas
            {
                public static string Repetir(string texto, int veces) => "";
            }
            """,
            """
            public static class Cadenas
            {
                public static string Repetir(string texto, int veces)
                {
                    if (veces < 0) throw new ArgumentOutOfRangeException(nameof(veces));
                    string resultado = "";
                    for (int i = 0; i < veces; i++)
                    {
                        resultado += texto;
                    }
                    return resultado;
                }

                public static string Repetir(char c, int veces) => Repetir(c.ToString(), veces);
            }
            """,
            [
                Is("Texto", "Cadenas.Repetir(\"ab\", 3)", "ababab"),
                Is("Carácter", "Cadenas.Repetir('*', 4)", "****"),
                Is("Cero veces", "Cadenas.Repetir(\"x\", 0).Length", "0", hidden: true, edge: true),
                Throws("Negativo", "Cadenas.Repetir('-', -1)", "ArgumentOutOfRangeException", hidden: true),
            ],
            "Una sobrecarga puede reutilizar a otra: así la lógica está en un solo lugar.",
            ["Implementa la versión de string con un bucle.", "La versión de char puede llamar a la de string con c.ToString()."],
            difficulty: 2, requires: [CodeConstruct.Method], related: ["n1.for"]);

        r.Code("n2.sobrecarga.app", s, ItemPurpose.Transfer, CodeMode.Function,
            "Aplicación: un formateador de duraciones. Formatear(int segundos) devuelve «m:ss» (por ejemplo 125 → «2:05»). Formatear(int minutos, int segundos) suma ambos y devuelve el mismo formato. Formatear(int segundos, bool conHoras) devuelve «h:mm:ss» si conHoras es true y «m:ss» si es false.",
            """
            public static class Duracion
            {
                public static string Formatear(int segundos) => segundos.ToString();
            }
            """,
            """
            public static class Duracion
            {
                public static string Formatear(int segundos) => $"{segundos / 60}:{segundos % 60:D2}";

                public static string Formatear(int minutos, int segundos) => Formatear(minutos * 60 + segundos);

                public static string Formatear(int segundos, bool conHoras)
                {
                    if (!conHoras)
                    {
                        return Formatear(segundos);
                    }
                    int horas = segundos / 3600;
                    int resto = segundos % 3600;
                    return $"{horas}:{resto / 60:D2}:{resto % 60:D2}";
                }
            }
            """,
            [
                Is("Segundos", "Duracion.Formatear(125)", "2:05"),
                Is("Minutos y segundos", "Duracion.Formatear(3, 70)", "4:10"),
                Is("Con horas", "Duracion.Formatear(3725, true)", "1:02:05"),
                Is("Sin horas", "Duracion.Formatear(3725, false)", "62:05", hidden: true),
                Is("Cero", "Duracion.Formatear(0)", "0:00", hidden: true, edge: true),
            ],
            "Tres formas de pedir lo mismo, una sola lógica de formato reutilizada.",
            ["El formato :D2 rellena con ceros a la izquierda hasta 2 dígitos.", "Haz que las versiones nuevas llamen a Formatear(int segundos)."],
            difficulty: 3, requires: [CodeConstruct.Method], related: ["n0.aritmetica"]);

        return new Skill(s, UnitId, "Sobrecarga de métodos",
            "Varios métodos con el mismo nombre y distinta firma.",
            Tier.Intermediate, ["n2.parametros"],
            new Lesson(s,
                "Declarar sobrecargas válidas, predecir cuál se elige en una llamada y reutilizar una sobrecarga desde otra.",
                [
                    P("Sobrecargar un método es declarar varias versiones con el mismo nombre y distintos parámetros. El compilador elige cuál usar según los argumentos de cada llamada."),
                    Code("""
                        Console.WriteLine(Calc.Mayor(3, 8));
                        Console.WriteLine(Calc.Mayor(3, 8, 5));

                        static class Calc
                        {
                            public static int Mayor(int a, int b) => a > b ? a : b;
                            public static int Mayor(int a, int b, int c) => Mayor(Mayor(a, b), c);
                        }
                        """, "Dos versiones de Mayor"),
                    Warn("Los métodos locales (los que declaras directamente entre las instrucciones del programa) no admiten sobrecarga. Las sobrecargas se declaran dentro de una clase, como Calc en el ejemplo; static class indica que la clase solo agrupa métodos."),
                    P("La firma de un método es su nombre más los tipos de sus parámetros. Dos métodos no pueden tener la misma firma, aunque cambien el tipo de retorno o los nombres de los parámetros."),
                    P("La sintaxis => de las líneas anteriores es un cuerpo de expresión: equivale a { return ...; } para métodos de una sola expresión."),
                    Tip("Los parámetros opcionales (int veces = 1) son otra forma de ofrecer varias maneras de llamar a un método."),
                    Design("Usa sobrecargas cuando todas las versiones hacen conceptualmente lo mismo; si hacen cosas distintas, usa nombres distintos."),
                ],
                new WorkedExample("Sobrecargas que se apoyan unas en otras",
                    """
                    Console.WriteLine(Formato.Precio(100m));
                    Console.WriteLine(Formato.Precio(100m, 15));
                    Console.WriteLine(Formato.Precio(100m, 15, "MXN"));

                    static class Formato
                    {
                        public static string Precio(decimal monto) => Precio(monto, 0);
                        public static string Precio(decimal monto, int descuento) => Precio(monto, descuento, "$");
                        public static string Precio(decimal monto, int descuento, string moneda)
                        {
                            decimal final = monto * (100 - descuento) / 100;
                            return $"{moneda} {final:F2}";
                        }
                    }
                    """,
                    [
                        Line(5, "Las sobrecargas viven dentro de una clase."),
                        Line(7, "La versión simple delega en la de dos parámetros."),
                        Line(8, "Que a su vez delega en la completa."),
                        Line(9, "Toda la lógica vive en una sola versión."),
                    ],
                    "$ 100.00\n$ 85.00\nMXN 85.00"),
                new CommonMistake(
                    "Creer que cambiar solo el tipo de retorno crea una sobrecarga.",
                    "static class Lector\n{\n    public static int Leer(string s) => int.Parse(s);\n    public static double Leer(string s) => double.Parse(s);\n}",
                    "Ambos métodos tienen la misma firma Leer(string); el compilador no podría saber cuál llamar (CS0111).",
                    "static class Lector\n{\n    public static int LeerEntero(string s) => int.Parse(s);\n    public static double LeerDecimal(string s) => double.Parse(s);\n}"),
                "n2.sobrecarga.pred1", "n2.sobrecarga.guiado", "n2.sobrecarga.indep", "n2.sobrecarga.app",
                ["n2.sobrecarga.recall1", "n2.sobrecarga.recall2", "n2.sobrecarga.concepto1"],
                "Escribes sobrecargas válidas sin duplicar lógica y predices cuál se ejecuta en cada llamada.",
                12));
    }
}
