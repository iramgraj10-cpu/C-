using CSharpMastery.Content.Infrastructure;
using CSharpMastery.Domain.Curriculum;
using CSharpMastery.Domain.Items;
using CSharpMastery.Domain.Progress;
using static CSharpMastery.Content.Infrastructure.ContentRegistry;
using static CSharpMastery.Content.Infrastructure.L;

namespace CSharpMastery.Content.Nivel4;

internal static class U01ErroresGenericos
{
    public const string UnitId = "n4.u1";

    public static Unit Build(ContentRegistry r) => new(UnitId, "n4", "Errores y genéricos",
        "Manejo de excepciones y código genérico.",
        [Excepciones(r), Genericos(r)]);

    private static Skill Excepciones(ContentRegistry r)
    {
        const string s = "n4.excepciones";
        r.Short("n4.excepciones.pred1", s, ItemPurpose.Prediction,
            "¿Qué muestra este programa?",
            "La división entre cero lanza DivideByZeroException: se salta el resto del try y se ejecuta el catch correspondiente. finally se ejecuta siempre, haya excepción o no. Después el programa continúa.",
            ["A\nC\nD\nE"], match: AnswerMatch.Output,
            code: """
                int cero = 0;
                try
                {
                    Console.WriteLine("A");
                    int x = 10 / cero;
                    Console.WriteLine("B");
                }
                catch (DivideByZeroException)
                {
                    Console.WriteLine("C");
                }
                finally
                {
                    Console.WriteLine("D");
                }
                Console.WriteLine("E");
                """,
            wrong: new() { ["A\nB\nC\nD\nE"] = "Al lanzarse la excepción se abandona el try: la línea de B nunca se ejecuta." },
            difficulty: 2);

        r.Choice("n4.excepciones.pred2", s, ItemPurpose.Prediction,
            "¿Qué muestra este programa?",
            "Los catch se revisan en orden y se usa el primero compatible. int.Parse(\"doce\") lanza FormatException, que no es una ArgumentException, así que se salta el primero y entra en el segundo (Exception atrapa cualquier excepción).",
            [
                Ok("Otro error", "Correcto."),
                No("Argumento inválido", "FormatException no deriva de ArgumentException.", ErrorCategory.ConceptMisunderstanding),
                No("Se lanza FormatException y el programa termina", "El segundo catch la atrapa.", ErrorCategory.ConceptMisunderstanding),
            ],
            code: """
                try
                {
                    int.Parse("doce");
                }
                catch (ArgumentException)
                {
                    Console.WriteLine("Argumento inválido");
                }
                catch (Exception)
                {
                    Console.WriteLine("Otro error");
                }
                """,
            difficulty: 2);

        r.Choice("n4.excepciones.concepto1", s, ItemPurpose.Concept,
            "¿Cuándo es apropiado lanzar una excepción y cuándo devolver un resultado como bool (por ejemplo, TryParse)?",
            "Las excepciones son para situaciones excepcionales que el código que llama no debería tener que comprobar a cada paso (un archivo corrupto, un argumento imposible). Si el fallo es esperable y frecuente (texto escrito por una persona), conviene un patrón Try que devuelva bool.",
            [
                Ok("Excepción para errores inesperados o violaciones de contrato; bool/Try para fallos esperables y frecuentes.", "Correcto."),
                No("Siempre excepciones: son más modernas.", "Usarlas para el flujo normal hace el código lento y difícil de leer."),
                No("Nunca excepciones: siempre devolver -1 o null.", "Los códigos especiales se olvidan de comprobar con facilidad.", ErrorCategory.ConceptMisunderstanding),
            ],
            dimension: KnowledgeDimension.Design);

        r.Choice("n4.excepciones.recall1", s, ItemPurpose.Recall,
            "¿Para qué sirve el bloque finally?",
            "Para código que debe ejecutarse siempre al salir del try, haya o no excepción: liberar recursos, restaurar un estado, registrar el fin de una operación.",
            [
                Ok("Para código que se ejecuta siempre, haya o no excepción.", "Correcto."),
                No("Para atrapar las excepciones que no atrapó ningún catch.", "finally no atrapa nada; la excepción sigue su camino después de ejecutarlo."),
                No("Para ejecutar código solo si no hubo errores.", "Eso iría al final del try."),
            ]);

        r.Choice("n4.excepciones.recall2", s, ItemPurpose.Recall,
            "Dentro de un catch, ¿qué diferencia hay entre «throw;» y «throw e;»?",
            "throw; vuelve a lanzar la misma excepción conservando la pila de llamadas original. throw e; la relanza como si se hubiera originado ahí, y se pierde dónde ocurrió realmente.",
            [
                Ok("throw; conserva la pila de llamadas original; throw e; la reinicia.", "Correcto."),
                No("Son equivalentes.", "throw e; pierde información valiosa para depurar."),
                No("throw; termina el programa.", "Solo relanza la excepción hacia quien llamó."),
            ]);

        r.Choice("n4.excepciones.bug1", s, ItemPurpose.BugFinding,
            "Este código «funciona», pero un revisor lo rechaza. ¿Cuál es el problema principal?",
            "Atrapar Exception y no hacer nada oculta cualquier error, incluso los que indican un defecto (un null inesperado, un índice inválido). El programa sigue con datos incorrectos y nadie se entera. Atrapa solo las excepciones que sabes manejar.",
            [
                Ok("El catch vacío de Exception oculta cualquier error, incluso defectos reales.", "Correcto."),
                No("Falta un finally.", "No es necesario aquí; el problema es silenciar errores.", ErrorCategory.ConceptMisunderstanding),
                No("int.Parse nunca lanza excepciones.", "Lanza FormatException y OverflowException.", ErrorCategory.ParsingFormat),
            ],
            code: """
                int total = 0;
                foreach (string linea in lineas)
                {
                    try
                    {
                        total += int.Parse(linea);
                    }
                    catch (Exception)
                    {
                    }
                }
                """,
            targets: [ErrorCategory.UnhandledException]);

        r.Code("n4.excepciones.guiado", s, ItemPurpose.Guided, CodeMode.Program,
            "Completa el programa: lee números (uno por línea) hasta el fin de la entrada y muestra su suma. Si una línea no es un número entero, atrapa la FormatException, muestra «Ignorado: TEXTO» y continúa.",
            """
            int suma = 0;
            string? linea;
            while ((linea = Console.ReadLine()) is not null)
            {
                suma += int.Parse(linea); // protege esta línea con try/catch
            }
            Console.WriteLine($"Suma: {suma}");
            """,
            """
            int suma = 0;
            string? linea;
            while ((linea = Console.ReadLine()) is not null)
            {
                try
                {
                    suma += int.Parse(linea);
                }
                catch (FormatException)
                {
                    Console.WriteLine($"Ignorado: {linea}");
                }
            }
            Console.WriteLine($"Suma: {suma}");
            """,
            [
                In("Todo válido", "3\n4", "Suma: 7"),
                In("Con texto", "3\ndos\n4", "Ignorado: dos\nSuma: 7"),
                In("Vacío y espacios", "\n5", "Ignorado: \nSuma: 5", hidden: true, edge: true),
            ],
            "Atrapar solo FormatException deja que otros errores inesperados sigan siendo visibles.",
            ["Envuelve la línea de suma en try { ... }.", "catch (FormatException) { Console.WriteLine($\"Ignorado: {linea}\"); }"],
            requires: [CodeConstruct.TryCatch], targets: [ErrorCategory.ParsingFormat]);

        r.Code("n4.excepciones.indep", s, ItemPurpose.Independent, CodeMode.Function,
            "Crea la excepción propia SaldoInsuficienteException (hereda de Exception) con las propiedades Disponible y Solicitado, y un mensaje «Saldo insuficiente: disponible X, solicitado Y». Escribe Cajero.Retirar(decimal saldo, decimal monto) que devuelva el saldo restante, lance ArgumentOutOfRangeException si monto <= 0 y SaldoInsuficienteException si monto > saldo.",
            """
            public static class Cajero
            {
                public static decimal Retirar(decimal saldo, decimal monto) => saldo - monto;
            }
            """,
            """
            public class SaldoInsuficienteException(decimal disponible, decimal solicitado)
                : Exception($"Saldo insuficiente: disponible {disponible}, solicitado {solicitado}")
            {
                public decimal Disponible => disponible;
                public decimal Solicitado => solicitado;
            }

            public static class Cajero
            {
                public static decimal Retirar(decimal saldo, decimal monto)
                {
                    if (monto <= 0) throw new ArgumentOutOfRangeException(nameof(monto), "El monto debe ser positivo");
                    if (monto > saldo) throw new SaldoInsuficienteException(saldo, monto);
                    return saldo - monto;
                }
            }
            """,
            [
                Is("Retiro válido", "Cajero.Retirar(500m, 120m)", "380"),
                Throws("Sin saldo", "Cajero.Retirar(100m, 150m)", "SaldoInsuficienteException"),
                Is("Mensaje", "{ try { Cajero.Retirar(100m, 150m); return \"\"; } catch (SaldoInsuficienteException e) { return e.Message; } }", "Saldo insuficiente: disponible 100, solicitado 150", hidden: true),
                Is("Propiedades", "{ try { Cajero.Retirar(80m, 90m); return 0m; } catch (SaldoInsuficienteException e) { return e.Solicitado - e.Disponible; } }", "10", hidden: true),
                Throws("Monto cero", "Cajero.Retirar(100m, 0m)", "ArgumentOutOfRangeException", hidden: true),
            ],
            "Una excepción propia con datos permite que quien la atrapa reaccione con precisión (por ejemplo, ofrecer retirar el disponible).",
            ["class SaldoInsuficienteException(decimal disponible, decimal solicitado) : Exception(mensaje)", "Valida el monto primero y después el saldo."],
            difficulty: 2, requires: [CodeConstruct.Class], related: ["n3.herencia"]);

        r.Code("n4.excepciones.app", s, ItemPurpose.Transfer, CodeMode.Function,
            "Aplicación: importar un archivo CSV de calificaciones. Importador.Procesar(string[] lineas) recibe líneas «nombre,calificación» y devuelve un record Resultado(int Validas, double Promedio, List<string> Errores). Cada línea inválida agrega «Línea N: motivo» a Errores, donde el motivo es «faltan campos», «calificación no numérica» o «fuera de rango» (0 a 10). El promedio es de las líneas válidas, redondeado a 2 decimales (0 si no hay). Usa excepciones internamente para cada tipo de error y atrápalas por línea.",
            """
            public record Resultado(int Validas, double Promedio, List<string> Errores);

            public static class Importador
            {
                public static Resultado Procesar(string[] lineas) => new(0, 0, []);
            }
            """,
            """
            public record Resultado(int Validas, double Promedio, List<string> Errores);

            public class LineaInvalidaException(string motivo) : Exception(motivo);

            public static class Importador
            {
                public static Resultado Procesar(string[] lineas)
                {
                    List<string> errores = [];
                    double suma = 0;
                    int validas = 0;
                    for (int i = 0; i < lineas.Length; i++)
                    {
                        try
                        {
                            suma += Calificacion(lineas[i]);
                            validas++;
                        }
                        catch (LineaInvalidaException e)
                        {
                            errores.Add($"Línea {i + 1}: {e.Message}");
                        }
                    }
                    double promedio = validas == 0 ? 0 : Math.Round(suma / validas, 2);
                    return new Resultado(validas, promedio, errores);
                }

                private static double Calificacion(string linea)
                {
                    string[] partes = linea.Split(',');
                    if (partes.Length != 2) throw new LineaInvalidaException("faltan campos");
                    if (!double.TryParse(partes[1].Trim(), out double c)) throw new LineaInvalidaException("calificación no numérica");
                    if (c < 0 || c > 10) throw new LineaInvalidaException("fuera de rango");
                    return c;
                }
            }
            """,
            [
                Is("Todo válido", "{ var r = Importador.Procesar([\"Ana,9\", \"Luis,7.5\"]); return $\"{r.Validas} {r.Promedio} {r.Errores.Count}\"; }", "2 8.25 0"),
                Is("Errores", "Importador.Procesar([\"Ana,9\", \"Luis\", \"Eva,diez\", \"Leo,11\"]).Errores", "[\"Línea 2: faltan campos\", \"Línea 3: calificación no numérica\", \"Línea 4: fuera de rango\"]"),
                Is("Sin válidas", "Importador.Procesar([\"x\"]).Promedio", "0", hidden: true, edge: true),
                Is("Límites", "Importador.Procesar([\"a,0\", \"b,10\"]).Validas", "2", hidden: true, edge: true),
            ],
            "Una excepción propia por línea deja que la validación esté en un método pequeño y que el bucle decida qué hacer con cada error sin detener la importación.",
            ["Crea una excepción propia y lánzala con el motivo desde un método que valide una línea.", "En el bucle, atrapa solo esa excepción y agrega el mensaje con el número de línea (i + 1)."],
            difficulty: 3, requires: [CodeConstruct.TryCatch], related: ["n1.validacion"], targets: [ErrorCategory.ParsingFormat]);

        return new Skill(s, UnitId, "Excepciones y manejo de errores",
            "try, catch, finally, throw y excepciones propias.",
            Tier.Essential, ["n3.clases"],
            new Lesson(s,
                "Atrapar excepciones específicas, usar finally, lanzar y relanzar correctamente, y crear excepciones propias cuando aportan información.",
                [
                    P("Una excepción es un objeto que representa un error en tiempo de ejecución. Cuando se lanza, el programa abandona el código actual y busca un catch compatible subiendo por la pila de llamadas. Si no lo encuentra, el programa termina."),
                    Code("""
                        string[] entradas = ["42", "x", "99999999999"];
                        foreach (string e in entradas)
                        {
                            try
                            {
                                int n = int.Parse(e);
                                Console.WriteLine($"Leído {n}");
                            }
                            catch (FormatException)
                            {
                                Console.WriteLine($"«{e}» no es un número");
                            }
                            catch (OverflowException)
                            {
                                Console.WriteLine($"«{e}» es demasiado grande");
                            }
                        }
                        """, "Un catch por cada tipo de error esperado"),
                    P("Los catch se evalúan en orden: el primero compatible gana. Por eso los tipos más específicos van primero. finally se ejecuta siempre al salir del try."),
                    P("throw lanza una excepción. Dentro de un catch, throw; (sin nada más) la relanza conservando dónde ocurrió."),
                    Warn("No atrapes Exception para ignorarla. Atrapa solo lo que sabes manejar; el resto debe hacerse visible."),
                    Design("Lanza la excepción más específica que exista (ArgumentNullException, ArgumentOutOfRangeException, InvalidOperationException). Crea una propia solo si quien la atrape necesita distinguirla o leer datos adicionales."),
                ],
                new WorkedExample("Atrapar, enriquecer y relanzar",
                    """
                    try
                    {
                        CargarConfiguracion(["puerto=8080", "tiempo=diez"]);
                    }
                    catch (InvalidOperationException e)
                    {
                        Console.WriteLine(e.Message);
                        Console.WriteLine(e.InnerException is FormatException ? "Causa: formato numérico" : "Causa: otra");
                    }

                    void CargarConfiguracion(string[] lineas)
                    {
                        foreach (string l in lineas)
                        {
                            string[] partes = l.Split('=');
                            try
                            {
                                int valor = int.Parse(partes[1]);
                                Console.WriteLine($"{partes[0]} = {valor}");
                            }
                            catch (FormatException ex)
                            {
                                throw new InvalidOperationException($"Valor inválido para «{partes[0]}»", ex);
                            }
                        }
                    }
                    """,
                    [
                        Line(3, "El código de alto nivel solo conoce un tipo de error de configuración."),
                        Line(7, "InnerException conserva el error original para diagnosticar."),
                        Line(17, "Aquí se sabe qué clave falló: se agrega ese contexto."),
                        Line(22, "Se lanza una excepción nueva que envuelve a la original."),
                    ],
                    "puerto = 8080\nValor inválido para «tiempo»\nCausa: formato numérico"),
                new CommonMistake(
                    "Atrapar Exception y continuar como si nada.",
                    "try\n{\n    Procesar(pedido);\n}\ncatch (Exception)\n{\n}",
                    "Cualquier defecto (null, índice fuera de rango, estado inválido) queda oculto y el programa sigue con datos incorrectos.",
                    "string texto = \"12a\";\ntry\n{\n    Console.WriteLine(int.Parse(texto));\n}\ncatch (FormatException)\n{\n    Console.WriteLine(\"Formato inválido\");\n}"),
                "n4.excepciones.pred1", "n4.excepciones.guiado", "n4.excepciones.indep", "n4.excepciones.app",
                ["n4.excepciones.recall1", "n4.excepciones.recall2", "n4.excepciones.pred2", "n4.excepciones.concepto1"],
                "Atrapas solo las excepciones que puedes manejar, lanzas las adecuadas con mensajes útiles y no ocultas errores.",
                20));
    }

    private static Skill Genericos(ContentRegistry r)
    {
        const string s = "n4.genericos";
        r.Short("n4.genericos.pred1", s, ItemPurpose.Prediction,
            "¿Qué muestra este programa?",
            "El método genérico Primero<T> funciona con cualquier tipo: el compilador deduce T a partir del argumento (string en la primera llamada, int en la segunda).",
            ["ana 7"], match: AnswerMatch.Output,
            code: """
                string a = Primero(["ana", "luis"]);
                int b = Primero([7, 8, 9]);
                Console.WriteLine($"{a} {b}");

                T Primero<T>(T[] elementos) => elementos[0];
                """);

        r.Choice("n4.genericos.concepto1", s, ItemPurpose.Concept,
            "¿Qué ventaja tiene List<int> frente a una lista que guarde object (como el antiguo ArrayList)?",
            "Con genéricos el compilador conoce el tipo de los elementos: detecta errores al compilar (no puedes agregar un string a List<int>) y no hacen falta conversiones al leer.",
            [
                Ok("Seguridad de tipos al compilar y sin conversiones al leer.", "Correcto."),
                No("Ninguna; es solo otra forma de escribirlo.", "Con object los errores de tipo aparecen al ejecutar, como InvalidCastException.", ErrorCategory.TypeMismatch),
                No("List<int> puede guardar elementos de cualquier tipo.", "Solo int; esa es la ventaja."),
            ]);

        r.Choice("n4.genericos.recall1", s, ItemPurpose.Recall,
            "¿Qué significa «where T : IComparable<T>» en un método genérico?",
            "Es una restricción: T solo puede ser un tipo que implemente IComparable<T>. A cambio, dentro del método puedes llamar a CompareTo.",
            [
                Ok("T debe implementar IComparable<T>, y por eso el método puede usar CompareTo.", "Correcto."),
                No("T se convierte automáticamente a IComparable.", "No convierte: restringe qué tipos se aceptan."),
                No("El método solo funciona con int.", "Funciona con cualquier tipo comparable: int, string, DateTime…"),
            ]);

        r.Choice("n4.genericos.recall2", s, ItemPurpose.Recall,
            "¿Qué devuelve default(T) cuando T es int, cuando es bool y cuando es string?",
            "default es el valor por defecto del tipo: 0 para int, false para bool y null para los tipos por referencia como string.",
            [
                Ok("0, false y null.", "Correcto."),
                No("0, false y \"\".", "Para string (tipo por referencia) es null, no la cadena vacía."),
                No("null en los tres casos.", "Los tipos por valor no pueden ser null."),
            ]);

        r.Choice("n4.genericos.bug1", s, ItemPurpose.BugFinding,
            "Este método genérico no compila (CS0019: el operador > no se puede aplicar a T). ¿Cómo se corrige?",
            "El compilador no sabe si T admite >. Hay que restringir T a tipos comparables (where T : IComparable<T>) y usar a.CompareTo(b) > 0.",
            [
                Ok("Agregar where T : IComparable<T> y usar CompareTo.", "Correcto."),
                No("Cambiar T por object.", "object tampoco tiene el operador >.", ErrorCategory.TypeMismatch),
                No("Convertir con (int)a > (int)b.", "Rompe el propósito genérico y falla con tipos no numéricos.", ErrorCategory.TypeMismatch),
            ],
            code: """
                T Mayor<T>(T a, T b)
                {
                    return a > b ? a : b;
                }
                """,
            targets: [ErrorCategory.TypeMismatch]);

        r.Code("n4.genericos.guiado", s, ItemPurpose.Guided, CodeMode.Function,
            "Completa el método genérico Maximo<T>, que devuelve el mayor elemento de una lista no vacía usando CompareTo. Si la lista está vacía, lanza InvalidOperationException.",
            """
            public static class Util
            {
                public static T Maximo<T>(List<T> elementos) where T : IComparable<T>
                {
                    T mayor = elementos[0];
                    // recorre y compara con CompareTo
                    return mayor;
                }
            }
            """,
            """
            public static class Util
            {
                public static T Maximo<T>(List<T> elementos) where T : IComparable<T>
                {
                    if (elementos.Count == 0) throw new InvalidOperationException("La lista está vacía");
                    T mayor = elementos[0];
                    foreach (T e in elementos)
                    {
                        if (e.CompareTo(mayor) > 0) mayor = e;
                    }
                    return mayor;
                }
            }
            """,
            [
                Is("Enteros", "Util.Maximo([3, 9, 2])", "9"),
                Is("Textos", "Util.Maximo([\"pera\", \"uva\", \"manzana\"])", "uva"),
                Is("Fechas", "Util.Maximo([new DateOnly(2024, 5, 1), new DateOnly(2025, 1, 1)]).Year", "2025", hidden: true),
                Throws("Vacía", "Util.Maximo(new List<int>())", "InvalidOperationException", hidden: true),
            ],
            "Un mismo método sirve para números, textos y fechas porque todos implementan IComparable<T>.",
            ["e.CompareTo(mayor) > 0 significa «e es mayor que mayor».", "Comprueba la lista vacía antes de leer elementos[0]."],
            requires: [CodeConstruct.Generic, CodeConstruct.Loop]);

        r.Code("n4.genericos.indep", s, ItemPurpose.Independent, CodeMode.Function,
            "Crea la clase genérica Par<TClave, TValor> con propiedades Clave y Valor (constructor) y el método Invertir() que devuelve un Par<TValor, TClave>. Sobrescribe ToString() para devolver «(clave, valor)».",
            """
            public class Par
            {
            }
            """,
            """
            public class Par<TClave, TValor>(TClave clave, TValor valor)
            {
                public TClave Clave => clave;
                public TValor Valor => valor;

                public Par<TValor, TClave> Invertir() => new(valor, clave);

                public override string ToString() => $"({Clave}, {Valor})";
            }
            """,
            [
                Is("ToString", "new Par<string, int>(\"edad\", 30).ToString()", "(edad, 30)"),
                Is("Invertir", "new Par<string, int>(\"edad\", 30).Invertir().ToString()", "(30, edad)"),
                Is("Tipos invertidos", "new Par<string, int>(\"x\", 5).Invertir().Clave + 1", "6", hidden: true, hint: "Invertir debe devolver Par<TValor, TClave> para que Clave sea un int."),
            ],
            "Los parámetros de tipo también pueden intercambiarse en el tipo de retorno: el compilador conoce el tipo exacto de cada propiedad.",
            ["class Par<TClave, TValor>(TClave clave, TValor valor)", "public Par<TValor, TClave> Invertir() => new(valor, clave);"],
            difficulty: 2, requires: [CodeConstruct.Generic, CodeConstruct.Class]);

        r.Code("n4.genericos.app", s, ItemPurpose.Transfer, CodeMode.Function,
            "Aplicación: una caché con capacidad limitada. Crea Cache<TClave, TValor> (TClave : notnull) con constructor (int capacidad), Guardar(clave, valor), TryObtener(clave, out TValor valor) y Cantidad. Si al guardar una clave nueva se supera la capacidad, se elimina la clave que lleva más tiempo sin usarse (guardarse u obtenerse). Capacidad <= 0 lanza ArgumentOutOfRangeException.",
            """
            public class Cache<TClave, TValor> where TClave : notnull
            {
                public Cache(int capacidad)
                {
                }

                public int Cantidad => 0;

                public void Guardar(TClave clave, TValor valor)
                {
                }

                public bool TryObtener(TClave clave, out TValor valor)
                {
                    valor = default!;
                    return false;
                }
            }
            """,
            """
            public class Cache<TClave, TValor> where TClave : notnull
            {
                private readonly int _capacidad;
                private readonly Dictionary<TClave, TValor> _datos = [];
                private readonly List<TClave> _uso = [];

                public Cache(int capacidad)
                {
                    if (capacidad <= 0) throw new ArgumentOutOfRangeException(nameof(capacidad));
                    _capacidad = capacidad;
                }

                public int Cantidad => _datos.Count;

                public void Guardar(TClave clave, TValor valor)
                {
                    if (!_datos.ContainsKey(clave) && _datos.Count == _capacidad)
                    {
                        TClave antigua = _uso[0];
                        _uso.RemoveAt(0);
                        _datos.Remove(antigua);
                    }
                    _datos[clave] = valor;
                    MarcarUso(clave);
                }

                public bool TryObtener(TClave clave, out TValor valor)
                {
                    if (_datos.TryGetValue(clave, out TValor? v))
                    {
                        MarcarUso(clave);
                        valor = v;
                        return true;
                    }
                    valor = default!;
                    return false;
                }

                private void MarcarUso(TClave clave)
                {
                    _uso.Remove(clave);
                    _uso.Add(clave);
                }
            }
            """,
            [
                Is("Guardar y obtener", "{ var c = new Cache<string, int>(2); c.Guardar(\"a\", 1); return c.TryObtener(\"a\", out var v) ? v : -1; }", "1"),
                Is("Expulsa la menos usada", "{ var c = new Cache<string, int>(2); c.Guardar(\"a\", 1); c.Guardar(\"b\", 2); c.TryObtener(\"a\", out _); c.Guardar(\"c\", 3); return $\"{c.TryObtener(\"a\", out _)} {c.TryObtener(\"b\", out _)} {c.Cantidad}\"; }", "True False 2"),
                Is("Actualizar no expulsa", "{ var c = new Cache<int, string>(2); c.Guardar(1, \"x\"); c.Guardar(2, \"y\"); c.Guardar(1, \"z\"); c.TryObtener(1, out var v); return $\"{v} {c.Cantidad} {c.TryObtener(2, out _)}\"; }", "z 2 True", hidden: true, edge: true),
                Throws("Capacidad inválida", "new Cache<int, int>(0)", "ArgumentOutOfRangeException", hidden: true),
            ],
            "La misma caché sirve para cualquier combinación de tipos; la restricción notnull refleja que un Dictionary no admite claves null.",
            ["Usa un Dictionary para los datos y una List<TClave> para el orden de uso (el primero es el menos usado).", "En cada Guardar u obtención exitosa, mueve la clave al final de la lista."],
            difficulty: 3, requires: [CodeConstruct.Generic, CodeConstruct.Class], related: ["n2.diccionarios", "n2.eleccion"]);

        return new Skill(s, UnitId, "Genéricos",
            "Tipos y métodos con parámetros de tipo y restricciones.",
            Tier.Intermediate, ["n3.abstractas-interfaces"],
            new Lesson(s,
                "Escribir métodos y clases genéricos, aplicar restricciones con where y entender por qué las colecciones de .NET son genéricas.",
                [
                    P("Un genérico es código parametrizado por un tipo. En lugar de escribir una pila de int, otra de string y otra de fechas, escribes Pila<T> una vez y T se reemplaza por el tipo concreto al usarla."),
                    Code("""
                        Pila<string> nombres = new();
                        nombres.Apilar("Ana");
                        nombres.Apilar("Luis");
                        Console.WriteLine(nombres.Desapilar());
                        Pila<int> numeros = new();
                        numeros.Apilar(42);
                        Console.WriteLine(numeros.Desapilar() + 1);

                        class Pila<T>
                        {
                            private readonly List<T> _datos = [];
                            public void Apilar(T x) => _datos.Add(x);
                            public T Desapilar()
                            {
                                T x = _datos[^1];
                                _datos.RemoveAt(_datos.Count - 1);
                                return x;
                            }
                        }
                        """, "Una clase genérica"),
                    P("Las restricciones (where) limitan los tipos aceptados y a cambio permiten usar sus capacidades: where T : IComparable<T> permite CompareTo; where T : class exige un tipo por referencia; where T : new() permite crear instancias con new T()."),
                    P("default(T) es el valor por defecto de T: 0, false o null según el tipo."),
                    Tip("List<T>, Dictionary<TKey,TValue>, Func<T,TResult> y Task<T> son genéricos que ya usas."),
                ],
                new WorkedExample("Un método genérico con restricción",
                    """
                    Console.WriteLine(Limitar(15, 0, 10));
                    Console.WriteLine(Limitar(-2.5, 0.0, 1.0));
                    Console.WriteLine(Limitar("m", "a", "f"));

                    T Limitar<T>(T valor, T minimo, T maximo) where T : IComparable<T>
                    {
                        if (valor.CompareTo(minimo) < 0) return minimo;
                        if (valor.CompareTo(maximo) > 0) return maximo;
                        return valor;
                    }
                    """,
                    [
                        Line(1, "T se deduce como int."),
                        Line(2, "Aquí T es double."),
                        Line(3, "Y aquí string: también es comparable."),
                        Line(5, "La restricción habilita CompareTo dentro del método."),
                    ],
                    "10\n0\nf"),
                new CommonMistake(
                    "Usar object en lugar de genéricos.",
                    "List<object> datos = [];\ndatos.Add(5);\ndatos.Add(\"cinco\");\nint suma = (int)datos[0] + (int)datos[1];",
                    "El compilador no puede ayudar: la conversión (int)\"cinco\" falla al ejecutar con InvalidCastException.",
                    "List<int> datos = [];\ndatos.Add(5);\ndatos.Add(7);\nConsole.WriteLine(datos[0] + datos[1]);"),
                "n4.genericos.pred1", "n4.genericos.guiado", "n4.genericos.indep", "n4.genericos.app",
                ["n4.genericos.recall1", "n4.genericos.recall2", "n4.genericos.concepto1"],
                "Escribes código genérico reutilizable con las restricciones mínimas necesarias y sin perder seguridad de tipos.",
                18));
    }
}
