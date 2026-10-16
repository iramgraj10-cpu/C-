using CSharpMastery.Content.Infrastructure;
using CSharpMastery.Domain.Curriculum;
using CSharpMastery.Domain.Items;
using CSharpMastery.Domain.Progress;
using static CSharpMastery.Content.Infrastructure.ContentRegistry;
using static CSharpMastery.Content.Infrastructure.L;

namespace CSharpMastery.Content.Nivel4;

internal static class U02Funciones
{
    public const string UnitId = "n4.u2";

    public static Unit Build(ContentRegistry r) => new(UnitId, "n4", "Funciones como datos",
        "Delegados, eventos, lambdas, LINQ e iteradores.",
        [Delegados(r), Lambdas(r), Linq(r), Consultas(r), Iteradores(r)]);

    private static Skill Delegados(ContentRegistry r)
    {
        const string s = "n4.delegados";
        r.Short("n4.delegados.pred1", s, ItemPurpose.Prediction,
            "¿Qué muestra este programa?",
            "Un delegado guarda una referencia a un método. op primero apunta a Sumar y después a Multiplicar; llamar op(3, 4) ejecuta el método al que apunta en ese momento.",
            ["7\n12"], match: AnswerMatch.Output,
            code: """
                Func<int, int, int> op = Sumar;
                Console.WriteLine(op(3, 4));
                op = Multiplicar;
                Console.WriteLine(op(3, 4));

                int Sumar(int a, int b) => a + b;
                int Multiplicar(int a, int b) => a * b;
                """);

        r.Short("n4.delegados.pred2", s, ItemPurpose.Prediction,
            "¿Qué muestra este programa?",
            "Un evento puede tener varios suscriptores: al dispararse, se llaman todos en el orden en que se suscribieron. Después de -=, el primer suscriptor ya no recibe avisos.",
            ["Pantalla: 50\nRegistro: 50\nRegistro: 80"], match: AnswerMatch.Output,
            code: """
                Sensor s = new();
                Action<int> pantalla = v => Console.WriteLine($"Pantalla: {v}");
                s.Cambio += pantalla;
                s.Cambio += v => Console.WriteLine($"Registro: {v}");
                s.Medir(50);
                s.Cambio -= pantalla;
                s.Medir(80);

                class Sensor
                {
                    public event Action<int>? Cambio;
                    public void Medir(int valor) => Cambio?.Invoke(valor);
                }
                """,
            difficulty: 2);

        r.Choice("n4.delegados.concepto1", s, ItemPurpose.Concept,
            "¿Qué problema resuelve un evento en una clase como Sensor?",
            "Permite que el sensor avise a otros objetos sin conocerlos: cualquiera puede suscribirse. El sensor no depende de la pantalla ni del registro, y se pueden agregar oyentes nuevos sin modificarlo.",
            [
                Ok("Avisar a otros objetos sin depender de ellos; se pueden agregar oyentes sin modificar la clase.", "Correcto."),
                No("Hacer que el código se ejecute en paralelo.", "Los manejadores se ejecutan uno tras otro en el mismo hilo."),
                No("Guardar datos de forma permanente.", "Un evento es un mecanismo de notificación."),
            ],
            dimension: KnowledgeDimension.Design);

        r.Choice("n4.delegados.recall1", s, ItemPurpose.Recall,
            "¿Qué diferencia hay entre Func<int, string> y Action<int>?",
            "Ambos son delegados genéricos predefinidos. Func<..., TResult> devuelve un valor (el último tipo es el de retorno). Action<...> no devuelve nada.",
            [
                Ok("Func<int, string> recibe un int y devuelve un string; Action<int> recibe un int y no devuelve nada.", "Correcto."),
                No("Func recibe un string y devuelve un int.", "El último parámetro de tipo de Func es el resultado."),
                No("Action solo sirve para eventos.", "Action se usa en muchos lugares, no solo en eventos."),
            ]);

        r.Choice("n4.delegados.recall2", s, ItemPurpose.Recall,
            "¿Por qué se escribe Cambio?.Invoke(valor) en lugar de Cambio(valor)?",
            "Si nadie se ha suscrito, el evento es null. ?.Invoke solo llama si hay suscriptores; Cambio(valor) lanzaría NullReferenceException.",
            [
                Ok("Porque el evento es null si nadie se suscribió.", "Correcto."),
                No("Porque Invoke es más rápido.", "La razón es evitar el null."),
                No("Porque los eventos no se pueden llamar directamente.", "Dentro de la clase sí se pueden; el problema es el null."),
            ],
            targets: [ErrorCategory.NullReference]);

        r.Choice("n4.delegados.bug1", s, ItemPurpose.BugFinding,
            "Al ejecutar este código, el aviso aparece dos veces en cada cambio. ¿Por qué?",
            "Configurar() se llama dos veces y cada vez suscribe el mismo manejador con +=. Un evento admite suscripciones repetidas, y cada una se ejecuta. Suscribe una sola vez (o desuscribe antes).",
            [
                Ok("El manejador se suscribe dos veces, porque Configurar() se llama dos veces.", "Correcto."),
                No("Los eventos siempre se ejecutan dos veces.", "Se ejecutan una vez por suscripción.", ErrorCategory.ConceptMisunderstanding),
                No("Falta usar -= dentro de Avisar.", "El problema es la doble suscripción.", ErrorCategory.WrongLogic),
            ],
            code: """
                Termostato t = new();
                Configurar();
                Configurar();
                t.Ajustar(22);

                void Configurar() => t.Cambio += Avisar;
                void Avisar(int grados) => Console.WriteLine($"Nueva temperatura: {grados}");

                class Termostato
                {
                    public event Action<int>? Cambio;
                    public void Ajustar(int g) => Cambio?.Invoke(g);
                }
                """,
            targets: [ErrorCategory.WrongLogic]);

        r.Code("n4.delegados.guiado", s, ItemPurpose.Guided, CodeMode.Function,
            "Completa Aplicar: recibe una lista y una función Func<int, int>, y devuelve una lista nueva con la función aplicada a cada elemento. Las funciones Doble y Cuadrado ya están escritas.",
            """
            public static class Transformar
            {
                public static List<int> Aplicar(List<int> numeros, Func<int, int> f)
                {
                    List<int> resultado = [];
                    // agrega f(n) por cada n
                    return resultado;
                }

                public static int Doble(int n) => n * 2;
                public static int Cuadrado(int n) => n * n;
            }
            """,
            """
            public static class Transformar
            {
                public static List<int> Aplicar(List<int> numeros, Func<int, int> f)
                {
                    List<int> resultado = [];
                    foreach (int n in numeros)
                    {
                        resultado.Add(f(n));
                    }
                    return resultado;
                }

                public static int Doble(int n) => n * 2;
                public static int Cuadrado(int n) => n * n;
            }
            """,
            [
                Is("Doble", "Transformar.Aplicar([1, 2, 3], Transformar.Doble)", "[2, 4, 6]"),
                Is("Cuadrado", "Transformar.Aplicar([1, 2, 3], Transformar.Cuadrado)", "[1, 4, 9]"),
                Is("Vacía", "Transformar.Aplicar([], Transformar.Doble)", "[]", hidden: true, edge: true),
            ],
            "Aplicar no sabe qué transformación hará: la recibe como dato. Es la idea detrás de Select de LINQ.",
            ["Recorre numeros y agrega f(n) a resultado.", "Una variable de tipo Func se llama igual que un método: f(n)."],
            requires: [CodeConstruct.Loop]);

        r.Code("n4.delegados.indep", s, ItemPurpose.Independent, CodeMode.Function,
            "Crea la clase Temporizador con un evento Action<int>? Tic y un evento Action? Fin. El método Correr(int segundos) dispara Tic con 3, 2, 1… (cuenta regresiva desde segundos hasta 1) y después Fin. Si segundos < 1, solo dispara Fin.",
            """
            public class Temporizador
            {
                public void Correr(int segundos)
                {
                }
            }
            """,
            """
            public class Temporizador
            {
                public event Action<int>? Tic;
                public event Action? Fin;

                public void Correr(int segundos)
                {
                    for (int s = segundos; s >= 1; s--)
                    {
                        Tic?.Invoke(s);
                    }
                    Fin?.Invoke();
                }
            }
            """,
            [
                Is("Cuenta", "{ var t = new Temporizador(); var log = new List<string>(); t.Tic += s => log.Add(s.ToString()); t.Fin += () => log.Add(\"fin\"); t.Correr(3); return string.Join(\",\", log); }", "3,2,1,fin"),
                Is("Cero", "{ var t = new Temporizador(); var log = new List<string>(); t.Tic += s => log.Add(s.ToString()); t.Fin += () => log.Add(\"fin\"); t.Correr(0); return string.Join(\",\", log); }", "fin", edge: true),
                Is("Sin suscriptores", "{ new Temporizador().Correr(2); return \"ok\"; }", "ok", hidden: true, edge: true, hint: "Usa ?.Invoke para no fallar cuando nadie está suscrito."),
            ],
            "El temporizador no sabe qué harán sus oyentes; las pruebas se suscriben con lambdas que registran lo ocurrido.",
            ["Declara public event Action<int>? Tic; y public event Action? Fin;.", "Dispara con Tic?.Invoke(s) y Fin?.Invoke()."],
            difficulty: 2, requires: [CodeConstruct.Class], targets: [ErrorCategory.NullReference]);

        r.Code("n4.delegados.app", s, ItemPurpose.Transfer, CodeMode.Function,
            "Aplicación: un carrito que notifica cambios. Carrito con evento Action<decimal>? TotalCambio (se dispara con el nuevo total cada vez que cambia) y evento Action<string>? LimiteSuperado (se dispara con el nombre del producto que hizo pasar el total por encima de un límite dado en el constructor, solo en el momento en que se cruza). Métodos Agregar(string producto, decimal precio) y Quitar(string producto) (quita la primera aparición; si no existe, no hace nada y no notifica).",
            """
            public class Carrito(decimal limite)
            {
                public decimal Total { get; private set; }

                public void Agregar(string producto, decimal precio)
                {
                }

                public void Quitar(string producto)
                {
                }
            }
            """,
            """
            public class Carrito(decimal limite)
            {
                private readonly List<(string Producto, decimal Precio)> _lineas = [];

                public event Action<decimal>? TotalCambio;
                public event Action<string>? LimiteSuperado;

                public decimal Total { get; private set; }

                public void Agregar(string producto, decimal precio)
                {
                    bool antesDentro = Total <= limite;
                    _lineas.Add((producto, precio));
                    Total += precio;
                    TotalCambio?.Invoke(Total);
                    if (antesDentro && Total > limite)
                    {
                        LimiteSuperado?.Invoke(producto);
                    }
                }

                public void Quitar(string producto)
                {
                    for (int i = 0; i < _lineas.Count; i++)
                    {
                        if (_lineas[i].Producto == producto)
                        {
                            Total -= _lineas[i].Precio;
                            _lineas.RemoveAt(i);
                            TotalCambio?.Invoke(Total);
                            return;
                        }
                    }
                }
            }
            """,
            [
                Is("Notifica totales", "{ var c = new Carrito(1000m); var log = new List<decimal>(); c.TotalCambio += t => log.Add(t); c.Agregar(\"pan\", 30m); c.Agregar(\"café\", 120m); c.Quitar(\"pan\"); return log; }", "[30, 150, 120]"),
                Is("Límite una vez", "{ var c = new Carrito(100m); var log = new List<string>(); c.LimiteSuperado += p => log.Add(p); c.Agregar(\"a\", 60m); c.Agregar(\"b\", 50m); c.Agregar(\"c\", 10m); return log; }", "[\"b\"]"),
                Is("Volver a cruzar", "{ var c = new Carrito(100m); var log = new List<string>(); c.LimiteSuperado += p => log.Add(p); c.Agregar(\"a\", 150m); c.Quitar(\"a\"); c.Agregar(\"b\", 120m); return log; }", "[\"a\", \"b\"]", hidden: true, edge: true, hint: "Avisa cada vez que el total pasa de estar dentro del límite a superarlo."),
                Is("Quitar inexistente", "{ var c = new Carrito(100m); int avisos = 0; c.TotalCambio += _ => avisos++; c.Quitar(\"x\"); return avisos; }", "0", hidden: true, edge: true),
            ],
            "Los eventos separan «qué pasó» (el carrito) de «qué hacer» (mostrar, registrar, alertar), que decide cada suscriptor.",
            ["Antes de sumar guarda si el total estaba dentro del límite; después de sumar compara.", "Quitar recorre con for y, al encontrar el producto, ajusta el total, lo elimina, notifica y termina."],
            difficulty: 3, requires: [CodeConstruct.Class], related: ["n3.encapsulacion"]);

        return new Skill(s, UnitId, "Delegados y eventos",
            "Referencias a métodos y notificaciones.",
            Tier.Intermediate, ["n3.clases"],
            new Lesson(s,
                "Guardar y pasar métodos como valores con delegados (Func, Action), y usar eventos para notificar cambios sin acoplar clases.",
                [
                    P("Un delegado es un tipo cuyas variables guardan una referencia a un método. Permite pasar comportamiento como argumento o guardarlo para ejecutarlo después."),
                    P("Func<T1, …, TResult> representa métodos que devuelven un valor; Action<T1, …> representa métodos void. Cubren casi todos los casos sin declarar delegados propios."),
                    Code("""
                        Func<double, double> conIva = Iva;
                        Action<string> mostrar = Console.WriteLine;
                        mostrar($"Total: {conIva(100):F2}");
                        Console.WriteLine(Ejecutar(conIva, 50));

                        double Iva(double monto) => monto * 1.16;
                        double Ejecutar(Func<double, double> f, double x) => f(x);
                        """, "Func y Action"),
                    P("Un evento (event) es un delegado especial: desde fuera de la clase solo se puede suscribir (+=) o desuscribir (-=); solo la clase que lo declara puede dispararlo."),
                    Warn("Si nadie se ha suscrito, el evento es null: dispáralo con Evento?.Invoke(...)."),
                    Tip("Desuscribe (-=) los manejadores de objetos que viven poco cuando el emisor vive mucho; si no, el emisor los mantiene en memoria."),
                ],
                new WorkedExample("Un descargador que informa su progreso",
                    """
                    Descarga d = new(4);
                    d.Progreso += p => Console.WriteLine($"{p}%");
                    d.Completada += () => Console.WriteLine("¡Listo!");
                    d.Iniciar();

                    class Descarga(int partes)
                    {
                        public event Action<int>? Progreso;
                        public event Action? Completada;

                        public void Iniciar()
                        {
                            for (int i = 1; i <= partes; i++)
                            {
                                Progreso?.Invoke(i * 100 / partes);
                            }
                            Completada?.Invoke();
                        }
                    }
                    """,
                    [
                        Line(2, "La suscripción es una lambda; la verás en detalle en la próxima lección."),
                        Line(8, "event Action<int> declara un evento que envía un entero."),
                        Line(15, "La clase dispara el evento; no sabe quién escucha."),
                        Line(17, "Un segundo evento sin datos."),
                    ],
                    "25%\n50%\n75%\n100%\n¡Listo!"),
                new CommonMistake(
                    "Disparar un evento sin comprobar si tiene suscriptores.",
                    "public event Action<int>? Cambio;\npublic void Medir(int v) => Cambio(v);",
                    "Sin suscriptores el evento es null y se lanza NullReferenceException.",
                    "class Sensor\n{\n    public event Action<int>? Cambio;\n    public void Medir(int v) => Cambio?.Invoke(v);\n}"),
                "n4.delegados.pred1", "n4.delegados.guiado", "n4.delegados.indep", "n4.delegados.app",
                ["n4.delegados.recall1", "n4.delegados.recall2", "n4.delegados.pred2", "n4.delegados.concepto1"],
                "Pasas comportamiento como dato con Func y Action, y diseñas clases que notifican cambios mediante eventos.",
                18));
    }

    private static Skill Lambdas(ContentRegistry r)
    {
        const string s = "n4.lambdas";
        r.Short("n4.lambdas.pred1", s, ItemPurpose.Prediction,
            "¿Qué muestra este programa?",
            "La lambda captura la variable contador (no una copia de su valor). Cada llamada a siguiente() la incrementa, y el cambio se ve también fuera de la lambda.",
            ["1 2 2"], match: AnswerMatch.Output,
            code: """
                int contador = 0;
                Func<int> siguiente = () => ++contador;
                int a = siguiente();
                int b = siguiente();
                Console.WriteLine($"{a} {b} {contador}");
                """,
            difficulty: 2);

        r.Short("n4.lambdas.pred2", s, ItemPurpose.Prediction,
            "¿Qué muestra este programa?",
            "En C#, la variable de un foreach es nueva en cada vuelta, así que cada lambda captura su propio valor (1, 2 y 3).",
            ["123"], match: AnswerMatch.Output,
            code: """
                List<Func<int>> funciones = [];
                foreach (int i in new[] { 1, 2, 3 })
                {
                    funciones.Add(() => i);
                }
                foreach (Func<int> f in funciones)
                {
                    Console.Write(f());
                }
                """,
            difficulty: 3);

        r.Choice("n4.lambdas.concepto1", s, ItemPurpose.Concept,
            "¿Qué es una lambda como «x => x * 2»?",
            "Es una función anónima: se escribe en el lugar donde se necesita, sin declarar un método con nombre. A la izquierda de => van los parámetros y a la derecha el resultado (o un bloque de instrucciones).",
            [
                Ok("Una función sin nombre que recibe x y devuelve x * 2.", "Correcto."),
                No("Una comparación entre x y x * 2.", "=> no es un operador de comparación."),
                No("Una variable llamada x que vale el doble.", "Es una función; no se evalúa hasta que se llama."),
            ]);

        r.Choice("n4.lambdas.recall1", s, ItemPurpose.Recall,
            "¿Cuál es la forma correcta de una lambda con dos parámetros y un bloque de instrucciones?",
            "Los parámetros van entre paréntesis y el cuerpo con varias instrucciones entre llaves, con return explícito si devuelve un valor.",
            [
                Ok("(a, b) => { int s = a + b; return s * 2; }", "Correcto."),
                No("a, b => a + b", "Con más de un parámetro hacen falta paréntesis.", ErrorCategory.Syntax),
                No("(a, b) -> a + b", "En C# la flecha es =>.", ErrorCategory.Syntax),
            ]);

        r.Choice("n4.lambdas.recall2", s, ItemPurpose.Recall,
            "¿Qué es una «variable capturada» por una lambda?",
            "Una variable externa que la lambda usa. La lambda conserva acceso a la variable (no a una copia) mientras la lambda exista, aunque el método donde se creó ya haya terminado.",
            [
                Ok("Una variable externa que la lambda sigue usando, aunque el método original ya haya terminado.", "Correcto."),
                No("Un parámetro de la lambda.", "Los parámetros se declaran a la izquierda de =>."),
                No("Una copia del valor en el momento de crear la lambda.", "Se captura la variable, no su valor de ese instante."),
            ]);

        r.Choice("n4.lambdas.bug1", s, ItemPurpose.BugFinding,
            "Se esperaba «0 1 2», pero se muestra «3 3 3». ¿Por qué?",
            "Con for, la variable i es una sola para todo el bucle. Las tres lambdas capturan la misma variable, que al final vale 3. Solución: copiar a una variable local dentro del cuerpo (int copia = i;) y capturar la copia.",
            [
                Ok("Todas las lambdas capturan la misma variable i, que al terminar el for vale 3.", "Correcto."),
                No("Las lambdas no pueden usar variables de un for.", "Pueden; el problema es que capturan la misma.", ErrorCategory.Scope),
                No("Console.Write no admite lambdas.", "Se llama a f(), que devuelve un int.", ErrorCategory.ConceptMisunderstanding),
            ],
            code: """
                List<Func<int>> fs = [];
                for (int i = 0; i < 3; i++)
                {
                    fs.Add(() => i);
                }
                foreach (var f in fs) Console.Write($"{f()} ");
                """,
            targets: [ErrorCategory.Scope]);

        r.Code("n4.lambdas.guiado", s, ItemPurpose.Guided, CodeMode.Function,
            "Completa Filtrar(lista, condicion) para que devuelva los elementos que cumplen la condición. Las pruebas la llaman con lambdas como n => n > 5.",
            """
            public static class Listas
            {
                public static List<int> Filtrar(List<int> lista, Func<int, bool> condicion)
                {
                    return lista;
                }
            }
            """,
            """
            public static class Listas
            {
                public static List<int> Filtrar(List<int> lista, Func<int, bool> condicion)
                {
                    List<int> resultado = [];
                    foreach (int x in lista)
                    {
                        if (condicion(x)) resultado.Add(x);
                    }
                    return resultado;
                }
            }
            """,
            [
                Is("Mayores que 5", "Listas.Filtrar([3, 8, 5, 10], n => n > 5)", "[8, 10]"),
                Is("Pares", "Listas.Filtrar([1, 2, 3, 4], n => n % 2 == 0)", "[2, 4]"),
                Is("Ninguno", "Listas.Filtrar([1, 2], n => n > 100)", "[]", hidden: true, edge: true),
            ],
            "La condición llega como dato; la misma función sirve para cualquier criterio.",
            ["Recorre la lista y usa if (condicion(x)).", "Construye una lista nueva con los que cumplen."],
            requires: [CodeConstruct.Loop]);

        r.Code("n4.lambdas.indep", s, ItemPurpose.Independent, CodeMode.Function,
            "Escribe Fabrica.Multiplicador(int factor) que devuelva una Func<int, int> que multiplique por factor, y Fabrica.Componer(Func<int, int> f, Func<int, int> g) que devuelva una función que aplique primero f y después g. Escribe ambas con lambdas.",
            """
            public static class Fabrica
            {
                public static Func<int, int> Multiplicador(int factor) => x => x;

                public static Func<int, int> Componer(Func<int, int> f, Func<int, int> g) => f;
            }
            """,
            """
            public static class Fabrica
            {
                public static Func<int, int> Multiplicador(int factor) => x => x * factor;

                public static Func<int, int> Componer(Func<int, int> f, Func<int, int> g) => x => g(f(x));
            }
            """,
            [
                Is("Triple", "Fabrica.Multiplicador(3)(7)", "21"),
                Is("Componer", "Fabrica.Componer(Fabrica.Multiplicador(2), x => x + 1)(5)", "11"),
                Is("Orden", "Fabrica.Componer(x => x + 1, Fabrica.Multiplicador(2))(5)", "12", hidden: true, hint: "Primero f y después g: g(f(x))."),
            ],
            "Multiplicador devuelve una lambda que captura factor: cada función creada recuerda su propio factor.",
            ["Multiplicador: return x => x * factor;", "Componer: return x => g(f(x));"],
            difficulty: 2, requires: [CodeConstruct.Lambda]);

        r.Code("n4.lambdas.app", s, ItemPurpose.Transfer, CodeMode.Function,
            "Aplicación: reglas de precios configurables. Tarifario mantiene una lista de reglas, cada una Func<decimal, decimal> que transforma un precio. Agregar(Func<decimal, decimal> regla) las añade en orden; Precio(decimal basePrecio) aplica todas en ese orden y redondea a 2 decimales. Además crea los métodos estáticos que devuelven reglas: Descuento(int porcentaje), Recargo(decimal monto) y Minimo(decimal minimo) (si el precio queda por debajo, lo sube al mínimo).",
            """
            public class Tarifario
            {
                public void Agregar(Func<decimal, decimal> regla)
                {
                }

                public decimal Precio(decimal basePrecio) => basePrecio;

                public static Func<decimal, decimal> Descuento(int porcentaje) => p => p;
                public static Func<decimal, decimal> Recargo(decimal monto) => p => p;
                public static Func<decimal, decimal> Minimo(decimal minimo) => p => p;
            }
            """,
            """
            public class Tarifario
            {
                private readonly List<Func<decimal, decimal>> _reglas = [];

                public void Agregar(Func<decimal, decimal> regla) => _reglas.Add(regla);

                public decimal Precio(decimal basePrecio)
                {
                    decimal p = basePrecio;
                    foreach (var regla in _reglas)
                    {
                        p = regla(p);
                    }
                    return Math.Round(p, 2);
                }

                public static Func<decimal, decimal> Descuento(int porcentaje) => p => p * (100 - porcentaje) / 100;
                public static Func<decimal, decimal> Recargo(decimal monto) => p => p + monto;
                public static Func<decimal, decimal> Minimo(decimal minimo) => p => Math.Max(p, minimo);
            }
            """,
            [
                Is("Descuento y recargo", "{ var t = new Tarifario(); t.Agregar(Tarifario.Descuento(10)); t.Agregar(Tarifario.Recargo(15m)); return (double)t.Precio(200m); }", "195"),
                Is("El orden importa", "{ var t = new Tarifario(); t.Agregar(Tarifario.Recargo(15m)); t.Agregar(Tarifario.Descuento(10)); return (double)t.Precio(200m); }", "193.5"),
                Is("Mínimo", "{ var t = new Tarifario(); t.Agregar(Tarifario.Descuento(90)); t.Agregar(Tarifario.Minimo(50m)); return (double)t.Precio(100m); }", "50", hidden: true),
                Is("Regla propia", "{ var t = new Tarifario(); t.Agregar(p => p * 2); return (double)t.Precio(3.333m); }", "6.67", hidden: true, edge: true),
                Is("Sin reglas", "(double)new Tarifario().Precio(9.99m)", "9.99", hidden: true, edge: true),
            ],
            "Las reglas son datos: se pueden combinar, reordenar o cargar de una configuración sin escribir clases nuevas.",
            ["Guarda las reglas en una List<Func<decimal, decimal>>.", "Cada método estático devuelve una lambda que captura su parámetro."],
            difficulty: 3, requires: [CodeConstruct.Lambda], related: ["n4.delegados"]);

        return new Skill(s, UnitId, "Expresiones lambda",
            "Funciones anónimas, Func y Action.",
            Tier.Essential, ["n4.delegados"],
            new Lesson(s,
                "Escribir lambdas de expresión y de bloque, pasarlas a métodos que reciben Func o Action y entender la captura de variables.",
                [
                    P("Una lambda es una función anónima que se escribe donde se necesita: parámetros => resultado. Se puede asignar a un Func o un Action, o pasar directamente como argumento."),
                    Code("""
                        Func<int, int> cuadrado = x => x * x;
                        Func<int, int, int> sumar = (a, b) => a + b;
                        Action<string> saludar = nombre =>
                        {
                            string mensaje = $"Hola, {nombre}";
                            Console.WriteLine(mensaje);
                        };
                        Console.WriteLine(cuadrado(5));
                        Console.WriteLine(sumar(2, 3));
                        saludar("Ana");
                        List<string> nombres = ["Luis", "ana", "Eva"];
                        nombres.Sort((a, b) => string.Compare(a, b, StringComparison.OrdinalIgnoreCase));
                        Console.WriteLine(string.Join(",", nombres));
                        """, "Lambdas de expresión, de bloque y como argumento"),
                    P("Una lambda puede usar variables del método donde se escribe: las captura. Captura la variable misma, así que ve los cambios posteriores y puede modificarla."),
                    Warn("En un for, todas las lambdas creadas dentro capturan la misma variable del bucle. Si necesitas el valor de cada vuelta, cópialo a una variable local dentro del cuerpo."),
                    Tip("Usa el descarte _ para parámetros que no necesitas: (_, e) => ..."),
                ],
                new WorkedExample("Un contador creado por una función",
                    """
                    Func<int> a = CrearContador();
                    Func<int> b = CrearContador();
                    Console.WriteLine(a());
                    Console.WriteLine(a());
                    Console.WriteLine(b());

                    Func<int> CrearContador()
                    {
                        int cuenta = 0;
                        return () => ++cuenta;
                    }
                    """,
                    [
                        Line(1, "Cada llamada a CrearContador crea una variable cuenta nueva."),
                        Line(4, "a sigue usando su propia cuenta, aunque CrearContador ya terminó."),
                        Line(5, "b tiene una cuenta independiente."),
                        Line(10, "La lambda captura cuenta."),
                    ],
                    "1\n2\n1"),
                new CommonMistake(
                    "Capturar la variable de un for esperando el valor de cada vuelta.",
                    "for (int i = 0; i < 3; i++)\n{\n    acciones.Add(() => Console.Write(i));\n}",
                    "Todas las lambdas comparten la misma i; al ejecutarlas después, todas ven su valor final.",
                    "List<Action> acciones = [];\nfor (int i = 0; i < 3; i++)\n{\n    int copia = i;\n    acciones.Add(() => Console.Write(copia));\n}\nforeach (Action a in acciones) a();"),
                "n4.lambdas.pred1", "n4.lambdas.guiado", "n4.lambdas.indep", "n4.lambdas.app",
                ["n4.lambdas.recall1", "n4.lambdas.recall2", "n4.lambdas.pred2", "n4.lambdas.concepto1"],
                "Escribes lambdas claras, las pasas a métodos que reciben comportamiento y predices el efecto de las variables capturadas.",
                15));
    }

    private static Skill Linq(ContentRegistry r)
    {
        const string s = "n4.linq";
        r.Short("n4.linq.pred1", s, ItemPurpose.Prediction,
            "¿Qué muestra este programa?",
            "Where deja los pares (2, 4, 6), Select los eleva al cuadrado (4, 16, 36) y Sum los suma: 56.",
            ["56"], match: AnswerMatch.Number,
            code: """
                int[] numeros = [1, 2, 3, 4, 5, 6];
                int resultado = numeros
                    .Where(n => n % 2 == 0)
                    .Select(n => n * n)
                    .Sum();
                Console.WriteLine(resultado);
                """);

        r.Short("n4.linq.pred2", s, ItemPurpose.Prediction,
            "¿Qué muestra este programa?",
            "GroupBy agrupa por la primera letra; OrderBy ordena los grupos por su clave; para cada grupo se muestra la clave y cuántos elementos tiene.",
            ["a:2 l:1 m:3"], match: AnswerMatch.Output,
            code: """
                string[] palabras = ["mar", "ala", "luz", "mes", "ave", "miel"];
                var grupos = palabras
                    .GroupBy(p => p[0])
                    .OrderBy(g => g.Key)
                    .Select(g => $"{g.Key}:{g.Count()}");
                Console.WriteLine(string.Join(" ", grupos));
                """,
            difficulty: 2);

        r.Choice("n4.linq.concepto1", s, ItemPurpose.Concept,
            "¿Qué tienen en común Where, Select y OrderBy?",
            "Son métodos de extensión sobre IEnumerable<T> que reciben una lambda y devuelven otra secuencia. Por eso se pueden encadenar, y no modifican la colección original.",
            [
                Ok("Reciben una lambda, devuelven una secuencia nueva y no modifican la original.", "Correcto."),
                No("Modifican la lista sobre la que se llaman.", "LINQ produce secuencias nuevas; la original no cambia.", ErrorCategory.ConceptMisunderstanding),
                No("Solo funcionan con arrays.", "Funcionan con cualquier IEnumerable<T>: listas, diccionarios, conjuntos…"),
            ]);

        r.Choice("n4.linq.recall1", s, ItemPurpose.Recall,
            "¿Qué diferencia hay entre First() y FirstOrDefault() cuando ningún elemento cumple la condición?",
            "First lanza InvalidOperationException; FirstOrDefault devuelve el valor por defecto del tipo (null, 0, false…).",
            [
                Ok("First lanza InvalidOperationException; FirstOrDefault devuelve el valor por defecto.", "Correcto."),
                No("Ambos devuelven null.", "First lanza una excepción."),
                No("FirstOrDefault devuelve el primer elemento de la lista aunque no cumpla.", "Devuelve el valor por defecto del tipo."),
            ]);

        r.Choice("n4.linq.recall2", s, ItemPurpose.Recall,
            "¿Qué operador usarías para saber si algún elemento cumple una condición, sin contar ni filtrar todos?",
            "Any(condición) devuelve true en cuanto encuentra el primero; es más claro y eficiente que Count(...) > 0.",
            [
                Ok("Any", "Correcto."),
                No("Count(...) > 0", "Funciona, pero recorre todos los elementos y expresa peor la intención."),
                No("Select", "Select transforma; no responde sí o no."),
            ]);

        r.Choice("n4.linq.bug1", s, ItemPurpose.BugFinding,
            "Este código pretende ordenar los productos por precio, pero la lista sigue igual. ¿Por qué?",
            "OrderBy no ordena la lista: devuelve una secuencia nueva ordenada. Hay que usar el resultado (por ejemplo, productos = productos.OrderBy(...).ToList();) o usar List.Sort.",
            [
                Ok("OrderBy devuelve una secuencia nueva y su resultado se descarta.", "Correcto."),
                No("OrderBy solo funciona con números enteros.", "Funciona con cualquier clave comparable.", ErrorCategory.ConceptMisunderstanding),
                No("Falta un Select después de OrderBy.", "El problema es que el resultado no se usa.", ErrorCategory.ConceptMisunderstanding),
            ],
            code: """
                List<Producto> productos = ObtenerProductos();
                productos.OrderBy(p => p.Precio);
                foreach (Producto p in productos) Console.WriteLine(p);
                """,
            targets: [ErrorCategory.ConceptMisunderstanding]);

        r.Code("n4.linq.guiado", s, ItemPurpose.Guided, CodeMode.Function,
            "Completa las consultas con LINQ: Aprobados devuelve los nombres de los alumnos con calificación >= 6 ordenados alfabéticamente; Promedio devuelve el promedio redondeado a 1 decimal (0 si no hay alumnos).",
            """
            public record Alumno(string Nombre, double Calificacion);

            public static class Consultas
            {
                public static List<string> Aprobados(List<Alumno> alumnos) => [];

                public static double Promedio(List<Alumno> alumnos) => 0;
            }
            """,
            """
            public record Alumno(string Nombre, double Calificacion);

            public static class Consultas
            {
                public static List<string> Aprobados(List<Alumno> alumnos) =>
                    alumnos.Where(a => a.Calificacion >= 6)
                           .OrderBy(a => a.Nombre)
                           .Select(a => a.Nombre)
                           .ToList();

                public static double Promedio(List<Alumno> alumnos) =>
                    alumnos.Count == 0 ? 0 : Math.Round(alumnos.Average(a => a.Calificacion), 1);
            }
            """,
            [
                Is("Aprobados", "Consultas.Aprobados([new(\"Luis\", 8), new(\"Ana\", 9), new(\"Eva\", 5)])", "[\"Ana\", \"Luis\"]"),
                Is("Promedio", "Consultas.Promedio([new(\"Luis\", 8), new(\"Ana\", 9), new(\"Eva\", 5)])", "7.3"),
                Is("Vacío", "Consultas.Promedio([])", "0", hidden: true, edge: true, hint: "Average lanza InvalidOperationException con una secuencia vacía."),
            ],
            "Cada paso de la consulta expresa una intención: filtrar, ordenar, proyectar.",
            ["Where → OrderBy → Select → ToList.", "Average sobre una lista vacía lanza excepción: comprueba Count antes."],
            requires: [CodeConstruct.Linq]);

        r.Code("n4.linq.indep", s, ItemPurpose.Independent, CodeMode.Function,
            "Usando LINQ, escribe: TopCategorias(ventas, n) que devuelva las n categorías con mayor monto total, como «categoría: total», de mayor a menor (en empate, por nombre); y ClienteMasFrecuente(ventas) que devuelva el cliente con más compras (en empate, el primero alfabéticamente) o null si no hay ventas.",
            """
            public record Venta(string Cliente, string Categoria, decimal Monto);

            public static class Analisis
            {
                public static List<string> TopCategorias(List<Venta> ventas, int n) => [];

                public static string? ClienteMasFrecuente(List<Venta> ventas) => null;
            }
            """,
            """
            public record Venta(string Cliente, string Categoria, decimal Monto);

            public static class Analisis
            {
                public static List<string> TopCategorias(List<Venta> ventas, int n) =>
                    ventas.GroupBy(v => v.Categoria)
                          .Select(g => (Categoria: g.Key, Total: g.Sum(v => v.Monto)))
                          .OrderByDescending(x => x.Total)
                          .ThenBy(x => x.Categoria)
                          .Take(n)
                          .Select(x => $"{x.Categoria}: {x.Total}")
                          .ToList();

                public static string? ClienteMasFrecuente(List<Venta> ventas) =>
                    ventas.GroupBy(v => v.Cliente)
                          .OrderByDescending(g => g.Count())
                          .ThenBy(g => g.Key)
                          .Select(g => g.Key)
                          .FirstOrDefault();
            }
            """,
            [
                Is("Top 2", "Analisis.TopCategorias([new(\"ana\", \"libros\", 300m), new(\"luis\", \"música\", 120m), new(\"ana\", \"juegos\", 500m), new(\"eva\", \"libros\", 250m)], 2)", "[\"libros: 550\", \"juegos: 500\"]"),
                Is("Cliente frecuente", "Analisis.ClienteMasFrecuente([new(\"luis\", \"x\", 1m), new(\"ana\", \"x\", 1m), new(\"luis\", \"y\", 1m)])", "luis"),
                Is("Empate de clientes", "Analisis.ClienteMasFrecuente([new(\"zoe\", \"x\", 1m), new(\"bea\", \"x\", 1m)])", "bea", hidden: true, edge: true),
                Is("Sin ventas", "Analisis.ClienteMasFrecuente([]) is null", "true", hidden: true, edge: true),
                Is("n mayor que categorías", "Analisis.TopCategorias([new(\"a\", \"x\", 5m)], 10).Count", "1", hidden: true, edge: true),
            ],
            "GroupBy + agregación + OrderByDescending + ThenBy + Take es un patrón muy frecuente en reportes.",
            ["Agrupa por categoría y calcula g.Sum(v => v.Monto).", "OrderByDescending(...).ThenBy(...) define el desempate; Take(n) limita."],
            difficulty: 3, requires: [CodeConstruct.Linq], related: ["n2.diccionarios"]);

        r.Code("n4.linq.app", s, ItemPurpose.Transfer, CodeMode.Function,
            "Aplicación: analizar un registro de accesos. Cada línea tiene «AAAA-MM-DD HH:MM usuario código» (código HTTP, por ejemplo 200 o 404). Escribe Accesos.Resumen(string[] lineas) que devuelva un record Informe(int Total, int Errores, string HoraPico, List<string> UsuariosConErrores): Errores son los códigos >= 400; HoraPico es la hora («HH») con más accesos (en empate, la menor) o «--» si no hay líneas; UsuariosConErrores son los usuarios distintos con al menos un error, ordenados alfabéticamente. Ignora las líneas vacías.",
            """
            public record Informe(int Total, int Errores, string HoraPico, List<string> UsuariosConErrores);

            public static class Accesos
            {
                public static Informe Resumen(string[] lineas) => new(0, 0, "--", []);
            }
            """,
            """
            public record Informe(int Total, int Errores, string HoraPico, List<string> UsuariosConErrores);

            public record Acceso(string Hora, string Usuario, int Codigo);

            public static class Accesos
            {
                public static Informe Resumen(string[] lineas)
                {
                    List<Acceso> accesos = lineas
                        .Where(l => !string.IsNullOrWhiteSpace(l))
                        .Select(Leer)
                        .ToList();

                    string horaPico = accesos
                        .GroupBy(a => a.Hora)
                        .OrderByDescending(g => g.Count())
                        .ThenBy(g => g.Key)
                        .Select(g => g.Key)
                        .FirstOrDefault() ?? "--";

                    List<string> conErrores = accesos
                        .Where(a => a.Codigo >= 400)
                        .Select(a => a.Usuario)
                        .Distinct()
                        .Order()
                        .ToList();

                    return new Informe(accesos.Count, accesos.Count(a => a.Codigo >= 400), horaPico, conErrores);
                }

                private static Acceso Leer(string linea)
                {
                    string[] p = linea.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    return new Acceso(p[1][..2], p[2], int.Parse(p[3]));
                }
            }
            """,
            [
                Is("Resumen", "{ var i = Accesos.Resumen([\"2025-05-01 09:15 ana 200\", \"2025-05-01 09:40 luis 404\", \"2025-05-01 10:05 ana 500\", \"2025-05-01 09:59 eva 200\"]); return $\"{i.Total} {i.Errores} {i.HoraPico} {string.Join(\",\", i.UsuariosConErrores)}\"; }", "4 2 09 ana,luis"),
                Is("Vacío", "{ var i = Accesos.Resumen([]); return $\"{i.Total} {i.HoraPico} {i.UsuariosConErrores.Count}\"; }", "0 -- 0", edge: true),
                Is("Empate de horas", "Accesos.Resumen([\"2025-05-01 14:00 a 200\", \"2025-05-01 08:00 b 200\"]).HoraPico", "08", hidden: true, edge: true),
                Is("Usuarios sin repetir", "Accesos.Resumen([\"2025-05-01 14:00 zoe 404\", \"2025-05-01 14:01 zoe 403\", \"\", \"2025-05-01 15:00 bea 500\"]).UsuariosConErrores", "[\"bea\", \"zoe\"]", hidden: true, edge: true),
            ],
            "Convertir primero cada línea en un record y después consultar con LINQ separa el análisis del formato del texto.",
            ["Transforma las líneas en objetos (Select) antes de agrupar o filtrar.", "GroupBy por hora + OrderByDescending(Count) + ThenBy(Key) da la hora pico; Distinct + Order da los usuarios."],
            difficulty: 3, requires: [CodeConstruct.Linq], related: ["n2.strings", "n3.records"]);

        return new Skill(s, UnitId, "LINQ",
            "Consultar colecciones con Where, Select, OrderBy, GroupBy...",
            Tier.Essential, ["n4.lambdas", "n2.listas"],
            new Lesson(s,
                "Consultar colecciones con los operadores principales de LINQ (filtrar, proyectar, ordenar, agrupar, agregar) y elegir el operador que expresa la intención.",
                [
                    P("LINQ (Language Integrated Query) es un conjunto de métodos de extensión para consultar cualquier IEnumerable<T>. Cada operador recibe una lambda y devuelve una secuencia nueva, así que se encadenan."),
                    Code("""
                        record Producto(string Nombre, string Categoria, decimal Precio);
                        """, "Fragmento: el tipo que usan los ejemplos"),
                    Code("""
                        List<(string Nombre, string Categoria, decimal Precio)> productos =
                        [
                            ("Té", "bebidas", 35m), ("Café", "bebidas", 60m),
                            ("Pan", "panadería", 12m), ("Pastel", "panadería", 250m),
                        ];
                        var baratos = productos.Where(p => p.Precio < 50).Select(p => p.Nombre);
                        Console.WriteLine(string.Join(", ", baratos));
                        var porCategoria = productos
                            .GroupBy(p => p.Categoria)
                            .Select(g => $"{g.Key}: {g.Count()} productos, máx {g.Max(p => p.Precio)}");
                        foreach (string linea in porCategoria) Console.WriteLine(linea);
                        Console.WriteLine(productos.OrderByDescending(p => p.Precio).First().Nombre);
                        Console.WriteLine(productos.Any(p => p.Precio > 200));
                        """, "Filtrar, proyectar, agrupar, ordenar"),
                    P("Operadores frecuentes: Where (filtrar), Select (transformar), OrderBy/ThenBy (ordenar), GroupBy (agrupar), Count/Sum/Average/Min/Max (agregar), Any/All (preguntar), First/FirstOrDefault/Single (obtener uno), Distinct, Take, Skip, ToList/ToDictionary (materializar)."),
                    Warn("LINQ no modifica la colección original. Si necesitas el resultado, guárdalo (con ToList() si lo vas a recorrer varias veces)."),
                    Tip("También existe la sintaxis de consulta (from p in productos where ... select ...). Ambas generan lo mismo; en código moderno se usa más la sintaxis de métodos."),
                ],
                new WorkedExample("Reporte de calificaciones con LINQ",
                    """
                    var notas = new[]
                    {
                        (Alumno: "Ana", Materia: "Mate", Nota: 9),
                        (Alumno: "Luis", Materia: "Mate", Nota: 6),
                        (Alumno: "Ana", Materia: "Historia", Nota: 10),
                        (Alumno: "Luis", Materia: "Historia", Nota: 5),
                        (Alumno: "Eva", Materia: "Mate", Nota: 8),
                    };
                    var promedios = notas
                        .GroupBy(n => n.Alumno)
                        .Select(g => (Alumno: g.Key, Promedio: g.Average(n => n.Nota)))
                        .OrderByDescending(x => x.Promedio)
                        .ThenBy(x => x.Alumno);
                    foreach (var p in promedios)
                    {
                        Console.WriteLine($"{p.Alumno}: {p.Promedio:F1}");
                    }
                    Console.WriteLine($"Reprobadas: {notas.Count(n => n.Nota < 6)}");
                    """,
                    [
                        Line(9, "Agrupar por alumno: cada grupo tiene todas sus notas."),
                        Line(10, "Proyectar cada grupo a una tupla con su promedio."),
                        Line(11, "Ordenar de mayor a menor promedio…"),
                        Line(12, "…y desempatar por nombre."),
                        Line(17, "Count con condición cuenta sin filtrar primero."),
                    ],
                    "Ana: 9.5\nEva: 8.0\nLuis: 5.5\nReprobadas: 1"),
                new CommonMistake(
                    "Llamar a un operador de LINQ y descartar su resultado.",
                    "productos.OrderBy(p => p.Precio);\nproductos.Where(p => p.Precio > 0);",
                    "Ninguna de las dos líneas cambia productos: devuelven secuencias nuevas que nadie usa.",
                    "List<decimal> precios = [30m, 10m, 20m];\nList<decimal> ordenados = precios.OrderBy(p => p).ToList();\nConsole.WriteLine(string.Join(\",\", ordenados));"),
                "n4.linq.pred1", "n4.linq.guiado", "n4.linq.indep", "n4.linq.app",
                ["n4.linq.recall1", "n4.linq.recall2", "n4.linq.pred2", "n4.linq.concepto1"],
                "Expresas consultas sobre colecciones con LINQ de forma legible, eligiendo el operador adecuado y materializando cuando hace falta.",
                25));
    }

    private static Skill Consultas(ContentRegistry r)
    {
        const string s = "n4.ienumerable-iqueryable";
        r.Short("n4.ienumerable-iqueryable.pred1", s, ItemPurpose.Prediction,
            "¿Qué muestra este programa?",
            "Where no se ejecuta al escribirlo: la consulta se evalúa cuando se recorre (ejecución diferida). Cuando Count() la recorre, la lista ya tiene el 4, así que hay 3 elementos mayores que 1.",
            ["3"], match: AnswerMatch.Number,
            code: """
                List<int> numeros = [1, 2, 3];
                IEnumerable<int> mayores = numeros.Where(n => n > 1);
                numeros.Add(4);
                Console.WriteLine(mayores.Count());
                """,
            wrong: new() { ["2"] = "La consulta no guarda resultados: se evalúa al recorrerla, cuando el 4 ya está en la lista." },
            difficulty: 2);

        r.Short("n4.ienumerable-iqueryable.pred2", s, ItemPurpose.Prediction,
            "¿Cuántas veces se muestra «evaluando» en total?",
            "La consulta es diferida y no se guarda: cada recorrido (Count y después el foreach) vuelve a ejecutar el Select sobre los 3 elementos. 3 + 3 = 6. Con ToList() se evaluaría una sola vez.",
            ["6"], match: AnswerMatch.Number,
            code: """
                int evaluaciones = 0;
                int[] datos = [1, 2, 3];
                var dobles = datos.Select(x => { evaluaciones++; return x * 2; });
                int cantidad = dobles.Count();
                foreach (int d in dobles) { }
                Console.WriteLine(evaluaciones);
                """,
            difficulty: 3);

        r.Choice("n4.ienumerable-iqueryable.concepto1", s, ItemPurpose.Concept,
            "¿Cuál es la diferencia esencial entre IEnumerable<T> e IQueryable<T> en una consulta con Where?",
            "Con IEnumerable<T>, Where recibe un delegado y filtra en memoria, elemento por elemento. Con IQueryable<T> (por ejemplo, Entity Framework), Where recibe un árbol de expresión que el proveedor traduce a otro lenguaje, como SQL, y el filtro se ejecuta en la base de datos.",
            [
                Ok("IEnumerable filtra en memoria con delegados; IQueryable traduce la expresión (por ejemplo a SQL) para ejecutarla en el origen de datos.", "Correcto."),
                No("IQueryable es siempre más rápido.", "Depende de dónde están los datos; traducir tiene costo y no todo se puede traducir."),
                No("Son idénticos; IQueryable es el nombre antiguo.", "Representan modelos de ejecución diferentes."),
            ],
            dimension: KnowledgeDimension.Design);

        r.Choice("n4.ienumerable-iqueryable.recall1", s, ItemPurpose.Recall,
            "¿Qué operadores fuerzan la ejecución inmediata de una consulta LINQ?",
            "Los que necesitan un resultado concreto: ToList, ToArray, ToDictionary, Count, Sum, First, Any, Max… Los que devuelven otra secuencia (Where, Select, OrderBy) son diferidos.",
            [
                Ok("ToList, ToArray, Count, Sum, First, Any…", "Correcto."),
                No("Where y Select.", "Esos son diferidos."),
                No("Ninguno: LINQ siempre es inmediato.", "La mayoría de los operadores que devuelven secuencias son diferidos."),
            ]);

        r.Choice("n4.ienumerable-iqueryable.recall2", s, ItemPurpose.Recall,
            "Con un IQueryable conectado a una base de datos, ¿por qué conviene filtrar antes de llamar a ToList()?",
            "Lo que va antes de ToList se traduce y se ejecuta en la base de datos, que devuelve solo las filas necesarias. Si llamas ToList() primero, traes toda la tabla a memoria y filtras después.",
            [
                Ok("Para que el filtro se ejecute en la base de datos y solo se traigan las filas necesarias.", "Correcto."),
                No("Porque ToList no admite Where después.", "Se puede, pero se filtraría en memoria con todos los datos ya descargados."),
                No("No importa el orden.", "El orden decide qué se ejecuta en la base de datos y qué en memoria."),
            ]);

        r.Choice("n4.ienumerable-iqueryable.bug1", s, ItemPurpose.BugFinding,
            "Esta consulta sobre una base de datos con millones de pedidos es muy lenta. ¿Por qué?",
            "AsEnumerable() (o un ToList()) convierte la consulta en IEnumerable: desde ahí, Where y Take se ejecutan en memoria, después de traer todos los pedidos desde la base de datos.",
            [
                Ok("AsEnumerable hace que todos los pedidos se traigan a memoria antes de filtrar.", "Correcto: filtra antes, en el IQueryable."),
                No("Take(10) siempre es lento.", "Take es barato si se ejecuta en la base de datos.", ErrorCategory.ConceptMisunderstanding),
                No("Falta un OrderBy.", "Puede ser necesario para Take, pero no explica la lentitud.", ErrorCategory.ConceptMisunderstanding),
            ],
            code: """
                var recientes = contexto.Pedidos
                    .AsEnumerable()
                    .Where(p => p.Fecha > hace30Dias)
                    .Take(10)
                    .ToList();
                """,
            dimension: KnowledgeDimension.Design);

        r.Code("n4.ienumerable-iqueryable.guiado", s, ItemPurpose.Guided, CodeMode.Function,
            "Resumen recorre la secuencia tres veces (Min, Max y Count). La clase FuenteContada cuenta cuántas veces se recorre. Cambia Resumen para que la secuencia se recorra una sola vez (materializándola con ToList).",
            """
            using System.Collections;

            public class FuenteContada(int[] datos) : IEnumerable<int>
            {
                public int Recorridos { get; private set; }

                public IEnumerator<int> GetEnumerator()
                {
                    Recorridos++;
                    foreach (int d in datos) yield return d;
                }

                IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
            }

            public static class Reporte
            {
                public static string Resumen(IEnumerable<int> datos) =>
                    $"{datos.Min()} {datos.Max()} {datos.Count()}";
            }
            """,
            """
            using System.Collections;

            public class FuenteContada(int[] datos) : IEnumerable<int>
            {
                public int Recorridos { get; private set; }

                public IEnumerator<int> GetEnumerator()
                {
                    Recorridos++;
                    foreach (int d in datos) yield return d;
                }

                IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
            }

            public static class Reporte
            {
                public static string Resumen(IEnumerable<int> datos)
                {
                    List<int> lista = datos.ToList();
                    return $"{lista.Min()} {lista.Max()} {lista.Count}";
                }
            }
            """,
            [
                Is("Un solo recorrido", "{ var f = new FuenteContada([3, 1, 2]); var r = Reporte.Resumen(f); return $\"{r} | {f.Recorridos}\"; }", "1 3 3 | 1"),
                Is("Resultado", "Reporte.Resumen(new FuenteContada([5]))", "5 5 1", edge: true),
            ],
            "Una secuencia puede ser costosa de recorrer (o dar resultados distintos cada vez). Materializarla una vez evita trabajo repetido.",
            ["Convierte datos a lista al inicio: List<int> lista = datos.ToList();", "Usa lista.Count (propiedad) en lugar de Count() sobre la secuencia."],
            requires: [CodeConstruct.Linq]);

        r.Code("n4.ienumerable-iqueryable.indep", s, ItemPurpose.Independent, CodeMode.Function,
            "Escribe Paginacion.Pagina<T>(IEnumerable<T> origen, int numero, int tamano) que devuelva la página indicada (desde 1) usando Skip y Take, sin materializar el origen completo: debe funcionar con una secuencia infinita. Si numero < 1 o tamano < 1, lanza ArgumentOutOfRangeException.",
            """
            public static class Paginacion
            {
                public static List<T> Pagina<T>(IEnumerable<T> origen, int numero, int tamano) =>
                    origen.ToList();
            }
            """,
            """
            public static class Paginacion
            {
                public static List<T> Pagina<T>(IEnumerable<T> origen, int numero, int tamano)
                {
                    if (numero < 1) throw new ArgumentOutOfRangeException(nameof(numero));
                    if (tamano < 1) throw new ArgumentOutOfRangeException(nameof(tamano));
                    return origen.Skip((numero - 1) * tamano).Take(tamano).ToList();
                }
            }
            """,
            [
                Is("Página 2", "Paginacion.Pagina(Enumerable.Range(1, 10), 2, 3)", "[4, 5, 6]"),
                Is("Última incompleta", "Paginacion.Pagina(Enumerable.Range(1, 10), 4, 3)", "[10]", edge: true),
                Is("Secuencia infinita", "Paginacion.Pagina(Enumerable.Range(0, int.MaxValue), 3, 2)", "[4, 5]", hidden: true, hint: "No llames a ToList sobre el origen completo: con una secuencia infinita no termina nunca."),
                Throws("Página cero", "Paginacion.Pagina(Enumerable.Range(1, 5), 0, 2)", "ArgumentOutOfRangeException", hidden: true),
            ],
            "Gracias a la ejecución diferida, Skip y Take solo recorren los elementos necesarios. Con un IQueryable, el mismo código se traduciría a OFFSET/FETCH en SQL.",
            ["Saltar (numero - 1) * tamano elementos y tomar tamano.", "Materializa con ToList solo después de Take."],
            difficulty: 2, requires: [CodeConstruct.Linq, CodeConstruct.Generic]);

        r.Code("n4.ienumerable-iqueryable.app", s, ItemPurpose.Transfer, CodeMode.Function,
            "Aplicación: un buscador sobre un catálogo grande. Buscador.Primeros(IEnumerable<Libro> catalogo, string texto, int n) devuelve los títulos de los primeros n libros cuyo título contiene el texto (sin distinguir mayúsculas), en el orden del catálogo. El catálogo de prueba cuenta cuántos libros se leyeron: la búsqueda debe detenerse en cuanto tiene n resultados.",
            """
            using System.Collections;

            public record Libro(string Titulo);

            public class Catalogo(int tamano) : IEnumerable<Libro>
            {
                public int Leidos { get; private set; }

                public IEnumerator<Libro> GetEnumerator()
                {
                    for (int i = 1; i <= tamano; i++)
                    {
                        Leidos++;
                        yield return new Libro(i % 10 == 0 ? $"Guía de C# volumen {i}" : $"Novela {i}");
                    }
                }

                IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
            }

            public static class Buscador
            {
                public static List<string> Primeros(IEnumerable<Libro> catalogo, string texto, int n) =>
                    catalogo.ToList()
                            .Where(l => l.Titulo.Contains(texto, StringComparison.OrdinalIgnoreCase))
                            .Take(n)
                            .Select(l => l.Titulo)
                            .ToList();
            }
            """,
            """
            using System.Collections;

            public record Libro(string Titulo);

            public class Catalogo(int tamano) : IEnumerable<Libro>
            {
                public int Leidos { get; private set; }

                public IEnumerator<Libro> GetEnumerator()
                {
                    for (int i = 1; i <= tamano; i++)
                    {
                        Leidos++;
                        yield return new Libro(i % 10 == 0 ? $"Guía de C# volumen {i}" : $"Novela {i}");
                    }
                }

                IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
            }

            public static class Buscador
            {
                public static List<string> Primeros(IEnumerable<Libro> catalogo, string texto, int n) =>
                    catalogo.Where(l => l.Titulo.Contains(texto, StringComparison.OrdinalIgnoreCase))
                            .Take(n)
                            .Select(l => l.Titulo)
                            .ToList();
            }
            """,
            [
                Is("Resultados", "Buscador.Primeros(new Catalogo(100), \"guía\", 2)", "[\"Guía de C# volumen 10\", \"Guía de C# volumen 20\"]"),
                Is("Se detiene", "{ var c = new Catalogo(1000000); Buscador.Primeros(c, \"guía\", 3); return c.Leidos; }", "30", hint: "Si materializas el catálogo antes de filtrar, se leen todos los libros."),
                Is("Menos resultados que n", "Buscador.Primeros(new Catalogo(25), \"GUÍA\", 5).Count", "2", hidden: true, edge: true),
            ],
            "Mientras la consulta sea diferida, Take(n) detiene la lectura del origen en cuanto tiene suficientes resultados.",
            ["Quita el ToList() del principio: materializa solo al final.", "Where → Take → Select → ToList."],
            difficulty: 2, requires: [CodeConstruct.Linq], related: ["n4.iteradores"]);

        return new Skill(s, UnitId, "IEnumerable<T> frente a IQueryable<T>",
            "Ejecución diferida en memoria y consultas traducidas.",
            Tier.Advanced, ["n4.linq"],
            new Lesson(s,
                "Entender la ejecución diferida de LINQ, evitar recorridos múltiples y distinguir consultas en memoria (IEnumerable) de consultas traducidas a otro origen (IQueryable).",
                [
                    P("La mayoría de los operadores de LINQ son diferidos: escribir numeros.Where(...) no filtra nada todavía; crea una consulta que se ejecuta cuando alguien la recorre (foreach, ToList, Count…)."),
                    Code("""
                        List<string> nombres = ["Ana", "Luis"];
                        IEnumerable<string> conA = nombres.Where(n => n.Contains('a', StringComparison.OrdinalIgnoreCase));
                        nombres.Add("Sara");
                        Console.WriteLine(string.Join(",", conA));
                        List<string> foto = conA.ToList();
                        nombres.Add("Paco");
                        Console.WriteLine($"{conA.Count()} {foto.Count}");
                        """, "La consulta se evalúa al recorrerla; ToList toma una «foto»"),
                    P("Cada recorrido vuelve a ejecutar la consulta. Si la recorres varias veces y es costosa, materialízala una vez con ToList()."),
                    P("IQueryable<T> (usado por Entity Framework y otros) no ejecuta lambdas: recibe árboles de expresión que un proveedor traduce, por ejemplo, a SQL. Así Where y Take se ejecutan en la base de datos."),
                    Fragment("""
                        // Con Entity Framework (requiere una base de datos configurada):
                        var caros = contexto.Productos      // IQueryable<Producto>
                            .Where(p => p.Precio > 1000)   // se traduce a WHERE
                            .OrderBy(p => p.Nombre)        // se traduce a ORDER BY
                            .Take(20)                      // se traduce a TOP / LIMIT
                            .ToList();                     // aquí se ejecuta la consulta SQL
                        """, "consulta traducida a SQL"),
                    Warn("Con IQueryable, no todo lo que escribes en una lambda se puede traducir (por ejemplo, llamar a tus propios métodos). Y un ToList() o AsEnumerable() a mitad de la consulta trae los datos a memoria."),
                ],
                new WorkedExample("Ver cuándo se ejecuta una consulta",
                    """
                    int[] datos = [5, 12, 8, 20];
                    var grandes = datos.Where(d =>
                    {
                        Console.WriteLine($"  revisando {d}");
                        return d > 10;
                    });
                    Console.WriteLine("Consulta creada");
                    Console.WriteLine($"Primero: {grandes.First()}");
                    Console.WriteLine($"Total: {grandes.Count()}");
                    """,
                    [
                        Line(2, "Crear la consulta no ejecuta la lambda."),
                        Line(7, "Esta línea aparece antes de cualquier «revisando»."),
                        Line(8, "First recorre solo hasta encontrar el primero que cumple."),
                        Line(9, "Count vuelve a recorrer todo desde el principio."),
                    ],
                    "Consulta creada\n  revisando 5\n  revisando 12\nPrimero: 12\n  revisando 5\n  revisando 12\n  revisando 8\n  revisando 20\nTotal: 2"),
                new CommonMistake(
                    "Recorrer varias veces una consulta costosa.",
                    "var activos = usuarios.Where(EsActivo);\nif (activos.Any())\n    Console.WriteLine($\"{activos.Count()} activos, primero {activos.First()}\");",
                    "La consulta se ejecuta tres veces. Si EsActivo es costoso o el origen es una base de datos, se repite todo el trabajo.",
                    "string[] usuarios = [\"ana\", \"luis\"];\nList<string> activos = usuarios.Where(u => u.Length > 3).ToList();\nif (activos.Count > 0)\n    Console.WriteLine($\"{activos.Count} activos, primero {activos[0]}\");"),
                "n4.ienumerable-iqueryable.pred1", "n4.ienumerable-iqueryable.guiado", "n4.ienumerable-iqueryable.indep", "n4.ienumerable-iqueryable.app",
                ["n4.ienumerable-iqueryable.recall1", "n4.ienumerable-iqueryable.recall2", "n4.ienumerable-iqueryable.pred2", "n4.ienumerable-iqueryable.concepto1"],
                "Predices cuándo se ejecuta una consulta, evitas recorridos repetidos y explicas qué parte de una consulta IQueryable se ejecuta en el origen de datos.",
                20));
    }

    private static Skill Iteradores(ContentRegistry r)
    {
        const string s = "n4.iteradores";
        r.Short("n4.iteradores.pred1", s, ItemPurpose.Prediction,
            "¿Qué muestra este programa?",
            "Un iterador con yield return produce los valores de uno en uno, solo cuando se piden. Take(4) pide 4 valores y la secuencia infinita nunca se recorre completa.",
            ["0 1 1 2"], match: AnswerMatch.Output,
            code: """
                Console.WriteLine(string.Join(" ", Fibonacci().Take(4)));

                IEnumerable<long> Fibonacci()
                {
                    long a = 0, b = 1;
                    while (true)
                    {
                        yield return a;
                        (a, b) = (b, a + b);
                    }
                }
                """);

        r.Short("n4.iteradores.pred2", s, ItemPurpose.Prediction,
            "¿Qué muestra este programa?",
            "El cuerpo del iterador no se ejecuta al llamar a Numeros(): se ejecuta paso a paso mientras el foreach pide elementos. Por eso «inicio» aparece después de «antes», y los mensajes se intercalan.",
            ["antes\ninicio\n1\nsigue\n2\nfin"], match: AnswerMatch.Output,
            code: """
                var seq = Numeros();
                Console.WriteLine("antes");
                foreach (int n in seq)
                {
                    Console.WriteLine(n);
                }

                IEnumerable<int> Numeros()
                {
                    Console.WriteLine("inicio");
                    yield return 1;
                    Console.WriteLine("sigue");
                    yield return 2;
                    Console.WriteLine("fin");
                }
                """,
            difficulty: 3);

        r.Choice("n4.iteradores.concepto1", s, ItemPurpose.Concept,
            "¿Qué ventaja tiene un iterador con yield return frente a construir y devolver una List<T> completa?",
            "El iterador produce cada elemento cuando se pide: no necesita memoria para todos a la vez, puede representar secuencias infinitas y permite detenerse pronto (con Take o First) sin calcular el resto.",
            [
                Ok("Produce los elementos bajo demanda: menos memoria, secuencias infinitas y posibilidad de detenerse pronto.", "Correcto."),
                No("Es siempre más rápido en todos los casos.", "Si vas a recorrer la secuencia muchas veces, una lista puede ser mejor."),
                No("Permite modificar la colección mientras se recorre.", "No tiene relación con eso."),
            ]);

        r.Choice("n4.iteradores.recall1", s, ItemPurpose.Recall,
            "¿Qué hace yield break dentro de un iterador?",
            "Termina la secuencia: el foreach que la recorre deja de recibir elementos.",
            [
                Ok("Termina la secuencia.", "Correcto."),
                No("Sale solo de la vuelta actual, como continue.", "Termina el iterador completo."),
                No("Devuelve null como último elemento.", "No produce ningún elemento más."),
            ]);

        r.Choice("n4.iteradores.recall2", s, ItemPurpose.Recall,
            "¿Qué tipos de retorno puede tener un método con yield return?",
            "IEnumerable<T> o IEnumerator<T> (y sus versiones no genéricas). Para iteradores asíncronos, IAsyncEnumerable<T>.",
            [
                Ok("IEnumerable<T> o IEnumerator<T>.", "Correcto."),
                No("List<T>", "Un iterador no construye una lista."),
                No("Cualquier tipo.", "Solo los tipos enumerables."),
            ]);

        r.Choice("n4.iteradores.bug1", s, ItemPurpose.BugFinding,
            "Este método debería lanzar ArgumentOutOfRangeException al llamarlo con un paso de 0, pero la excepción aparece mucho después, al recorrer el resultado. ¿Por qué?",
            "Todo el cuerpo de un iterador, incluida la validación, se ejecuta de forma diferida: recién en el primer MoveNext. Para validar de inmediato, se separa en un método normal que valida y un iterador privado que produce los valores.",
            [
                Ok("La validación está dentro del iterador y se ejecuta solo al empezar a recorrerlo.", "Correcto."),
                No("throw no funciona dentro de un iterador.", "Funciona, pero de forma diferida.", ErrorCategory.ConceptMisunderstanding),
                No("Falta un yield break antes del throw.", "No cambiaría el momento de la validación.", ErrorCategory.ConceptMisunderstanding),
            ],
            code: """
                IEnumerable<int> Rango(int desde, int hasta, int paso)
                {
                    if (paso == 0) throw new ArgumentOutOfRangeException(nameof(paso));
                    for (int i = desde; i < hasta; i += paso)
                    {
                        yield return i;
                    }
                }
                """,
            targets: [ErrorCategory.EdgeCaseMissed]);

        r.Code("n4.iteradores.guiado", s, ItemPurpose.Guided, CodeMode.Function,
            "Completa el iterador Pares(int limite) para que produzca con yield return los números pares desde 0 hasta limite (incluido).",
            """
            public static class Secuencias
            {
                public static IEnumerable<int> Pares(int limite)
                {
                    yield break;
                }
            }
            """,
            """
            public static class Secuencias
            {
                public static IEnumerable<int> Pares(int limite)
                {
                    for (int i = 0; i <= limite; i += 2)
                    {
                        yield return i;
                    }
                }
            }
            """,
            [
                Is("Hasta 6", "Secuencias.Pares(6)", "[0, 2, 4, 6]"),
                Is("Hasta 5", "Secuencias.Pares(5)", "[0, 2, 4]", edge: true),
                Is("Negativo", "Secuencias.Pares(-1).Count()", "0", hidden: true, edge: true),
            ],
            "yield return entrega un valor y pausa el método hasta que se pida el siguiente.",
            ["Usa un for que avance de 2 en 2.", "Dentro del for: yield return i;"]);

        r.Code("n4.iteradores.indep", s, ItemPurpose.Independent, CodeMode.Function,
            "Escribe el iterador Lotes<T>(IEnumerable<T> origen, int tamano) que produzca List<T> de hasta tamano elementos (el último puede ser más corto). Debe funcionar con secuencias infinitas (produciendo lotes bajo demanda) y validar tamano < 1 con ArgumentOutOfRangeException EN EL MOMENTO de la llamada, no al recorrer.",
            """
            public static class Secuencias
            {
                public static IEnumerable<List<T>> Lotes<T>(IEnumerable<T> origen, int tamano)
                {
                    yield break;
                }
            }
            """,
            """
            public static class Secuencias
            {
                public static IEnumerable<List<T>> Lotes<T>(IEnumerable<T> origen, int tamano)
                {
                    if (tamano < 1) throw new ArgumentOutOfRangeException(nameof(tamano));
                    return Producir(origen, tamano);
                }

                private static IEnumerable<List<T>> Producir<T>(IEnumerable<T> origen, int tamano)
                {
                    List<T> actual = new(tamano);
                    foreach (T x in origen)
                    {
                        actual.Add(x);
                        if (actual.Count == tamano)
                        {
                            yield return actual;
                            actual = new(tamano);
                        }
                    }
                    if (actual.Count > 0)
                    {
                        yield return actual;
                    }
                }
            }
            """,
            [
                Is("Lotes de 2", "Secuencias.Lotes([1, 2, 3, 4, 5], 2)", "[[1, 2], [3, 4], [5]]"),
                Is("Exacto", "Secuencias.Lotes([\"a\", \"b\"], 2).Count()", "1", edge: true),
                Throws("Validación inmediata", "{ var l = Secuencias.Lotes([1], 0); return 0; }", "ArgumentOutOfRangeException", hidden: true, hint: "Un iterador valida al recorrerse. Separa la validación en un método normal que devuelve el iterador privado."),
                Is("Infinita", "Secuencias.Lotes(Enumerable.Range(0, int.MaxValue), 3).Skip(2).First()", "[6, 7, 8]", hidden: true),
            ],
            "Separar validación e iterador es el patrón que usa el propio .NET en LINQ: los errores de argumentos aparecen donde se cometen.",
            ["Un método público valida y devuelve Producir(origen, tamano).", "En el iterador privado acumula en una lista y haz yield return cuando se llena; al final entrega el resto."],
            difficulty: 3, requires: [CodeConstruct.Generic]);

        r.Code("n4.iteradores.app", s, ItemPurpose.Transfer, CodeMode.Function,
            "Aplicación: leer un archivo de registro línea por línea sin cargarlo completo. Lector.Errores(TextReader lector) es un iterador que lee con ReadLine hasta null y produce, para cada línea que empieza con «ERROR», el texto que sigue después de «ERROR: ». Las pruebas usan StringReader; en una aplicación real sería un StreamReader de un archivo grande.",
            """
            using System.IO;

            public static class Lector
            {
                public static IEnumerable<string> Errores(TextReader lector)
                {
                    yield break;
                }
            }
            """,
            """
            using System.IO;

            public static class Lector
            {
                public static IEnumerable<string> Errores(TextReader lector)
                {
                    string? linea;
                    while ((linea = lector.ReadLine()) is not null)
                    {
                        if (linea.StartsWith("ERROR: "))
                        {
                            yield return linea["ERROR: ".Length..];
                        }
                    }
                }
            }
            """,
            [
                Is("Errores", "Lector.Errores(new StringReader(\"INFO: inicio\\nERROR: disco lleno\\nWARN: lento\\nERROR: sin red\"))", "[\"disco lleno\", \"sin red\"]"),
                Is("Ninguno", "Lector.Errores(new StringReader(\"INFO: ok\")).Count()", "0", edge: true),
                Is("Lectura perezosa", "{ var sr = new StringReader(\"ERROR: a\\nERROR: b\\nERROR: c\"); var primero = Lector.Errores(sr).First(); return $\"{primero} {sr.ReadLine()}\"; }", "a ERROR: b", hidden: true, hint: "First debe detener la lectura: el resto del texto sigue sin leer en el lector."),
            ],
            "El iterador lee solo lo necesario: First() detiene la lectura en el primer error, aunque el archivo tenga millones de líneas.",
            ["while ((linea = lector.ReadLine()) is not null)", "linea[\"ERROR: \".Length..] toma el texto después del prefijo."],
            difficulty: 2, related: ["n2.strings"]);

        return new Skill(s, UnitId, "Iteradores con yield",
            "Secuencias perezosas.",
            Tier.Intermediate, ["n4.linq"],
            new Lesson(s,
                "Escribir iteradores con yield return y yield break, entender su ejecución diferida y usarlos para secuencias grandes o infinitas.",
                [
                    P("Un iterador es un método que devuelve IEnumerable<T> y usa yield return para producir los elementos de uno en uno. El método se pausa después de cada yield return y continúa cuando se pide el siguiente."),
                    Code("""
                        foreach (int n in Cuenta(3))
                        {
                            Console.WriteLine(n);
                        }
                        Console.WriteLine(string.Join(",", Potencias(2).Take(5)));

                        IEnumerable<int> Cuenta(int hasta)
                        {
                            for (int i = 1; i <= hasta; i++)
                            {
                                yield return i;
                            }
                        }

                        IEnumerable<long> Potencias(long b)
                        {
                            long p = 1;
                            while (true)
                            {
                                yield return p;
                                p *= b;
                            }
                        }
                        """, "Una secuencia finita y una infinita"),
                    P("yield break termina la secuencia. Todo el cuerpo del iterador es diferido: no se ejecuta nada hasta que alguien empieza a recorrerlo."),
                    Tip("LINQ está construido con iteradores: Where y Select son, en esencia, bucles con yield return."),
                    Design("Si un iterador debe validar sus argumentos, sepáralo en un método normal que valida y un iterador privado que produce los valores."),
                ],
                new WorkedExample("Tu propio Where con un iterador",
                    """
                    int[] datos = [4, 9, 2, 15, 7];
                    foreach (int x in Filtrar(datos, n => n > 5))
                    {
                        Console.WriteLine(x);
                    }

                    IEnumerable<T> Filtrar<T>(IEnumerable<T> origen, Func<T, bool> condicion)
                    {
                        foreach (T elemento in origen)
                        {
                            if (condicion(elemento))
                            {
                                yield return elemento;
                            }
                        }
                    }
                    """,
                    [
                        Line(2, "Se recorre como cualquier secuencia."),
                        Line(7, "Un iterador genérico que recibe la condición como lambda."),
                        Line(13, "Entrega el elemento y se pausa hasta que el foreach pida el siguiente."),
                    ],
                    "9\n15\n7"),
                new CommonMistake(
                    "Construir una lista enorme cuando solo se necesitan unos pocos elementos.",
                    "List<int> TodosLosPrimos(int limite) { /* calcula todos hasta limite */ }\nvar primeros = TodosLosPrimos(10_000_000).Take(5);",
                    "Se calculan millones de valores para usar cinco.",
                    "Console.WriteLine(string.Join(\",\", Primos().Take(5)));\n\nIEnumerable<int> Primos()\n{\n    for (int n = 2; ; n++)\n    {\n        bool primo = true;\n        for (int d = 2; d * d <= n; d++) if (n % d == 0) { primo = false; break; }\n        if (primo) yield return n;\n    }\n}"),
                "n4.iteradores.pred1", "n4.iteradores.guiado", "n4.iteradores.indep", "n4.iteradores.app",
                ["n4.iteradores.recall1", "n4.iteradores.recall2", "n4.iteradores.pred2", "n4.iteradores.concepto1"],
                "Escribes iteradores que producen datos bajo demanda, validas sus argumentos de inmediato y predices su orden de ejecución.",
                18));
    }
}
