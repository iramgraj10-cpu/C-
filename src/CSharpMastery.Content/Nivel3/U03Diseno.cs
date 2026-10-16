using CSharpMastery.Content.Infrastructure;
using CSharpMastery.Domain.Curriculum;
using CSharpMastery.Domain.Items;
using CSharpMastery.Domain.Progress;
using static CSharpMastery.Content.Infrastructure.ContentRegistry;
using static CSharpMastery.Content.Infrastructure.L;

namespace CSharpMastery.Content.Nivel3;

internal static class U03Diseno
{
    public const string UnitId = "n3.u3";

    public static Unit Build(ContentRegistry r) => new(UnitId, "n3", "Tipos modernos y diseño",
        "Records, structs, nulabilidad y SOLID.",
        [Records(r), Nullability(r), Solid(r)]);

    private static Skill Records(ContentRegistry r)
    {
        const string s = "n3.records";
        r.Short("n3.records.pred1", s, ItemPurpose.Prediction,
            "¿Qué muestra este programa?",
            "Los records se comparan por valor: dos records con los mismos datos son iguales. Las clases se comparan por referencia: dos objetos distintos no son iguales aunque tengan los mismos datos.",
            ["True False"], match: AnswerMatch.Output,
            code: """
                PuntoR a = new(1, 2);
                PuntoR b = new(1, 2);
                PuntoC c = new(1, 2);
                PuntoC d = new(1, 2);
                Console.WriteLine($"{a == b} {c == d}");

                record PuntoR(int X, int Y);

                class PuntoC(int x, int y)
                {
                    public int X => x;
                    public int Y => y;
                }
                """,
            wrong: new() { ["True True"] = "Las clases comparan referencias: c y d son dos objetos distintos.", ["False False"] = "Los records comparan sus valores." });

        r.Short("n3.records.pred2", s, ItemPurpose.Prediction,
            "¿Qué muestra este programa?",
            "with crea una copia con algunas propiedades cambiadas; el original no se modifica. Además, los records generan un ToString con sus datos.",
            ["Producto { Nombre = té, Precio = 30 }\nProducto { Nombre = té, Precio = 25 }"], match: AnswerMatch.Output,
            code: """
                Producto p = new("té", 30m);
                Producto oferta = p with { Precio = 25m };
                Console.WriteLine(p);
                Console.WriteLine(oferta);

                record Producto(string Nombre, decimal Precio);
                """,
            difficulty: 2);

        r.Choice("n3.records.concepto1", s, ItemPurpose.Concept,
            "¿Qué ocurre al asignar un struct a otra variable, a diferencia de una clase?",
            "Un struct es un tipo por valor: la asignación copia todos sus datos. Modificar la copia no afecta al original. Una clase es por referencia: ambas variables apuntan al mismo objeto.",
            [
                Ok("Se copian sus datos: cambiar la copia no afecta al original.", "Correcto."),
                No("Ambas variables apuntan al mismo struct.", "Eso es lo que ocurre con las clases.", ErrorCategory.ConceptMisunderstanding),
                No("No se puede asignar un struct a otra variable.", "Sí se puede; se copia."),
            ]);

        r.Choice("n3.records.recall1", s, ItemPurpose.Recall,
            "¿Qué genera automáticamente «record Persona(string Nombre, int Edad);»?",
            "Propiedades init Nombre y Edad, un constructor, igualdad por valor (Equals, ==, GetHashCode), ToString con los datos, deconstrucción y soporte para with.",
            [
                Ok("Propiedades, constructor, igualdad por valor, ToString y soporte para with.", "Correcto."),
                No("Solo un constructor.", "Genera mucho más."),
                No("Una tabla en la base de datos.", "Un record es un tipo de C#, no tiene relación con bases de datos."),
            ]);

        r.Choice("n3.records.recall2", s, ItemPurpose.Recall,
            "¿Cuándo conviene un record?",
            "Para datos que se identifican por su valor y conviene que no cambien: coordenadas, montos con moneda, respuestas de una API, mensajes.",
            [
                Ok("Para datos que se comparan por valor y conviene que sean inmutables.", "Correcto."),
                No("Para objetos con mucho comportamiento y estado que cambia.", "Para eso suele ser mejor una clase."),
                No("Siempre en lugar de clases.", "Cada uno tiene su propósito."),
            ]);

        r.Choice("n3.records.bug1", s, ItemPurpose.BugFinding,
            "Este código no compila. ¿Por qué?",
            "Las propiedades de un record posicional son init: solo se asignan al crear el objeto. Para obtener un valor distinto se crea una copia con with.",
            [
                Ok("Las propiedades del record son de solo inicialización: usa p with { Edad = 31 }.", "Correcto."),
                No("Falta new antes de Edad.", "No es así como se modifica.", ErrorCategory.Syntax),
                No("Los records no pueden tener int.", "Pueden tener cualquier tipo.", ErrorCategory.ConceptMisunderstanding),
            ],
            code: """
                Persona p = new("Ana", 30);
                p.Edad = 31;

                record Persona(string Nombre, int Edad);
                """);

        r.Code("n3.records.guiado", s, ItemPurpose.Guided, CodeMode.Function,
            "Completa el record Dinero(decimal Monto, string Moneda) con el método Sumar(Dinero otro): devuelve un Dinero nuevo con la suma; si las monedas son distintas, lanza InvalidOperationException.",
            """
            public record Dinero(decimal Monto, string Moneda)
            {
                public Dinero Sumar(Dinero otro) => this;
            }
            """,
            """
            public record Dinero(decimal Monto, string Moneda)
            {
                public Dinero Sumar(Dinero otro)
                {
                    if (otro.Moneda != Moneda) throw new InvalidOperationException("Monedas distintas");
                    return this with { Monto = Monto + otro.Monto };
                }
            }
            """,
            [
                Is("Suma", "new Dinero(10m, \"MXN\").Sumar(new Dinero(5.5m, \"MXN\"))", "Dinero { Monto = 15.5, Moneda = MXN }"),
                Is("Igualdad", "new Dinero(1m, \"USD\") == new Dinero(1m, \"USD\")", "true"),
                Throws("Monedas distintas", "new Dinero(1m, \"USD\").Sumar(new Dinero(1m, \"MXN\"))", "InvalidOperationException", hidden: true),
                Is("No modifica", "{ var a = new Dinero(1m, \"MXN\"); a.Sumar(new Dinero(2m, \"MXN\")); return a.Monto; }", "1", hidden: true, hint: "Devuelve un record nuevo; no cambies this."),
            ],
            "Un valor monetario inmutable evita errores: nadie puede cambiar un monto que otro código está usando.",
            ["Compara las monedas antes de sumar.", "return this with { Monto = Monto + otro.Monto };"],
            requires: [CodeConstruct.Record]);

        r.Code("n3.records.indep", s, ItemPurpose.Independent, CodeMode.Function,
            "Crea el record struct Coordenada(double Lat, double Lon) con el método DistanciaKm(Coordenada otra) usando la aproximación equirectangular: x = Δlon·cos(latMedia), y = Δlat (en radianes); distancia = √(x² + y²) · 6371, redondeada a 1 decimal.",
            """
            public readonly record struct Coordenada(double Lat, double Lon)
            {
            }
            """,
            """
            public readonly record struct Coordenada(double Lat, double Lon)
            {
                public double DistanciaKm(Coordenada otra)
                {
                    double rad = Math.PI / 180;
                    double latMedia = (Lat + otra.Lat) / 2 * rad;
                    double x = (otra.Lon - Lon) * rad * Math.Cos(latMedia);
                    double y = (otra.Lat - Lat) * rad;
                    return Math.Round(Math.Sqrt(x * x + y * y) * 6371, 1);
                }
            }
            """,
            [
                Is("Misma", "new Coordenada(19.43, -99.13).DistanciaKm(new Coordenada(19.43, -99.13))", "0", edge: true),
                Is("Un grado de latitud", "new Coordenada(0, 0).DistanciaKm(new Coordenada(1, 0))", "111.2"),
                Is("CDMX a Puebla", "new Coordenada(19.4326, -99.1332).DistanciaKm(new Coordenada(19.0414, -98.2063))", "106.6", hidden: true),
                Is("Igualdad por valor", "new Coordenada(1, 2) == new Coordenada(1, 2)", "true", hidden: true),
            ],
            "Un record struct es un tipo por valor con igualdad por valor: ideal para datos pequeños como una coordenada.",
            ["Convierte grados a radianes multiplicando por Math.PI / 180.", "Usa Math.Cos, Math.Sqrt y Math.Round(valor, 1)."],
            difficulty: 2, requires: [CodeConstruct.Record], related: ["n0.aritmetica"]);

        r.Code("n3.records.app", s, ItemPurpose.Transfer, CodeMode.Function,
            "Aplicación: historial de cambios de un perfil. Record Perfil(string Nombre, string Correo, int Version). Clase Editor que guarda el perfil actual y su historial: Editor(Perfil inicial), CambiarCorreo(string) (crea una versión nueva con with, Version + 1; si el correo es igual al actual no hace nada), Deshacer() (vuelve a la versión anterior; InvalidOperationException si no hay), Actual y Versiones (cuántas versiones hay en el historial, incluida la actual).",
            """
            public record Perfil(string Nombre, string Correo, int Version);

            public class Editor(Perfil inicial)
            {
                public Perfil Actual => inicial;
            }
            """,
            """
            public record Perfil(string Nombre, string Correo, int Version);

            public class Editor(Perfil inicial)
            {
                private readonly Stack<Perfil> _historial = new([inicial]);

                public Perfil Actual => _historial.Peek();
                public int Versiones => _historial.Count;

                public void CambiarCorreo(string correo)
                {
                    if (correo == Actual.Correo) return;
                    _historial.Push(Actual with { Correo = correo, Version = Actual.Version + 1 });
                }

                public void Deshacer()
                {
                    if (_historial.Count == 1) throw new InvalidOperationException("No hay cambios que deshacer");
                    _historial.Pop();
                }
            }
            """,
            [
                Is("Cambio", "{ var e = new Editor(new Perfil(\"Ana\", \"a@x.com\", 1)); e.CambiarCorreo(\"ana@y.com\"); return e.Actual; }", "Perfil { Nombre = Ana, Correo = ana@y.com, Version = 2 }"),
                Is("Deshacer", "{ var e = new Editor(new Perfil(\"Ana\", \"a@x.com\", 1)); e.CambiarCorreo(\"b@x.com\"); e.CambiarCorreo(\"c@x.com\"); e.Deshacer(); return $\"{e.Actual.Correo} {e.Versiones}\"; }", "b@x.com 2"),
                Is("Sin cambio real", "{ var e = new Editor(new Perfil(\"Ana\", \"a@x.com\", 1)); e.CambiarCorreo(\"a@x.com\"); return e.Versiones; }", "1", hidden: true, edge: true),
                Throws("Nada que deshacer", "{ var e = new Editor(new Perfil(\"Ana\", \"a@x.com\", 1)); e.Deshacer(); return 0; }", "InvalidOperationException", hidden: true),
            ],
            "Como cada versión es inmutable, guardarlas en una pila basta para deshacer: ninguna versión anterior puede haber cambiado.",
            ["Guarda las versiones en un Stack<Perfil>; Peek es la actual.", "Actual with { Correo = correo, Version = Actual.Version + 1 } crea la nueva versión."],
            difficulty: 3, requires: [CodeConstruct.Record, CodeConstruct.Class], related: ["n2.listas"]);

        return new Skill(s, UnitId, "Records y tipos por valor",
            "Igualdad por valor, inmutabilidad y struct.",
            Tier.Intermediate, ["n3.encapsulacion"],
            new Lesson(s,
                "Usar records para datos inmutables con igualdad por valor, crear copias con with y distinguir tipos por valor (struct) de tipos por referencia (class).",
                [
                    P("Un record es un tipo pensado para datos. En una línea declara propiedades inmutables, constructor, igualdad por valor y un ToString legible."),
                    Code("""
                        Libro a = new("Ficciones", "Borges", 1944);
                        Libro b = new("Ficciones", "Borges", 1944);
                        Console.WriteLine(a == b);
                        Console.WriteLine(a);
                        Libro reedicion = a with { Anio = 1956 };
                        Console.WriteLine(reedicion.Anio);
                        var (titulo, autor, _) = a;
                        Console.WriteLine($"{titulo} de {autor}");

                        record Libro(string Titulo, string Autor, int Anio);
                        """, "Igualdad, ToString, with y deconstrucción"),
                    P("Un struct es un tipo por valor: se copia al asignarse o pasarse como argumento. int, double, bool, DateTime y decimal son structs."),
                    P("record struct combina ambas ideas: tipo por valor con las ventajas de un record. readonly record struct además garantiza la inmutabilidad."),
                    Design("Usa records para valores (dinero, coordenadas, mensajes, resultados) y clases para entidades con identidad y estado que cambia (una cuenta, un jugador)."),
                ],
                new WorkedExample("Value objects: dinero con moneda",
                    """
                    Monto precio = new(150m, "MXN");
                    Monto envio = new(50m, "MXN");
                    Monto total = precio + envio;
                    Console.WriteLine(total);
                    Console.WriteLine(total == new Monto(200m, "MXN"));
                    Monto copia = total;
                    copia = copia with { Valor = 0m };
                    Console.WriteLine(total.Valor);

                    readonly record struct Monto(decimal Valor, string Moneda)
                    {
                        public static Monto operator +(Monto a, Monto b) =>
                            a.Moneda == b.Moneda ? new(a.Valor + b.Valor, a.Moneda) : throw new InvalidOperationException("Monedas distintas");

                        public override string ToString() => $"{Valor:F2} {Moneda}";
                    }
                    """,
                    [
                        Line(3, "Se puede definir el operador + para el tipo."),
                        Line(5, "Igualdad por valor: mismo valor y misma moneda."),
                        Line(7, "copia es una copia independiente (struct) y with crea otra."),
                        Line(10, "readonly garantiza que ninguna propiedad cambie."),
                    ],
                    "200.00 MXN\nTrue\n200"),
                new CommonMistake(
                    "Intentar modificar una propiedad de un record.",
                    "Libro l = new(\"Ficciones\", \"Borges\", 1944);\nl.Anio = 1956;",
                    "Las propiedades posicionales son init: solo se asignan al crear. No compila (CS8852).",
                    "Libro l = new(\"Ficciones\", \"Borges\", 1944);\nLibro otra = l with { Anio = 1956 };\nConsole.WriteLine(otra.Anio);\n\nrecord Libro(string Titulo, string Autor, int Anio);"),
                "n3.records.pred1", "n3.records.guiado", "n3.records.indep", "n3.records.app",
                ["n3.records.recall1", "n3.records.recall2", "n3.records.pred2", "n3.records.concepto1"],
                "Eliges entre record, struct y class según la semántica del dato, y trabajas con copias inmutables usando with.",
                18));
    }

    private static Skill Nullability(ContentRegistry r)
    {
        const string s = "n3.nullability";
        r.Short("n3.nullability.pred1", s, ItemPurpose.Prediction,
            "¿Qué muestra este programa?",
            "c es null. c?.Nombre no lanza excepción: devuelve null. ?? reemplaza ese null por «anónimo». Con un cliente real, ?. accede normalmente.",
            ["anónimo\nAna"], match: AnswerMatch.Output,
            code: """
                Cliente? c = null;
                Console.WriteLine(c?.Nombre ?? "anónimo");
                c = new Cliente("Ana");
                Console.WriteLine(c?.Nombre ?? "anónimo");

                record Cliente(string Nombre);
                """);

        r.Choice("n3.nullability.concepto1", s, ItemPurpose.Concept,
            "Con los tipos de referencia anulables activados, ¿qué expresa «string?» frente a «string»?",
            "string? declara que el valor puede ser null y el compilador te obliga a comprobarlo antes de usarlo. string declara que nunca debería ser null, y el compilador avisa si intentas asignarle null.",
            [
                Ok("string? puede ser null y debe comprobarse; string no debería serlo nunca.", "Correcto."),
                No("Son idénticos en tiempo de ejecución, así que no importa cuál uses.", "En ejecución son el mismo tipo, pero la anotación permite que el compilador encuentre errores antes de ejecutar."),
                No("string? es un string vacío.", "Vacío (\"\") y null son cosas distintas."),
            ],
            targets: [ErrorCategory.NullReference]);

        r.Choice("n3.nullability.recall1", s, ItemPurpose.Recall,
            "¿Qué hace el operador ??= en «lista ??= [];»?",
            "Asigna el valor de la derecha solo si la variable es null. Es una forma corta de inicializar bajo demanda.",
            [
                Ok("Asigna [] a lista solo si lista es null.", "Correcto."),
                No("Compara lista con [].", "No compara: asigna condicionalmente."),
                No("Vacía la lista.", "Si la lista ya existe, no la toca."),
            ]);

        r.Choice("n3.nullability.recall2", s, ItemPurpose.Recall,
            "¿Qué significa el operador ! en «nombre!.Length»?",
            "Es el operador que suprime la advertencia de nulabilidad: le dices al compilador «sé que no es null». No hace ninguna comprobación; si te equivocas, se lanza NullReferenceException.",
            [
                Ok("Que confías en que no es null y silencias la advertencia, sin ninguna comprobación.", "Correcto: úsalo solo cuando realmente lo sabes."),
                No("Que se convierte a string vacío si es null.", "No convierte nada."),
                No("Que se niega el valor.", "Ese es el ! de los bool; aquí va después de la expresión."),
            ],
            targets: [ErrorCategory.NullReference]);

        r.Choice("n3.nullability.bug1", s, ItemPurpose.BugFinding,
            "Este método lanza NullReferenceException cuando el diccionario no tiene la clave. ¿Por qué y cómo se corrige mejor?",
            "GetValueOrDefault devuelve null si la clave no existe y el ! oculta la advertencia que lo habría avisado. Hay que manejar el caso: usuario?.Correo ?? \"sin correo\", o comprobar con is null.",
            [
                Ok("El ! oculta que el valor puede ser null; hay que manejar el caso null.", "Correcto."),
                No("Los diccionarios no pueden guardar records.", "Pueden guardar cualquier tipo.", ErrorCategory.ConceptMisunderstanding),
                No("Hay que poner ! también en Correo.", "Más ! solo esconde más el problema.", ErrorCategory.NullReference),
            ],
            code: """
                string CorreoDe(Dictionary<int, Usuario> usuarios, int id)
                {
                    Usuario usuario = usuarios.GetValueOrDefault(id)!;
                    return usuario.Correo;
                }

                record Usuario(string Correo);
                """,
            targets: [ErrorCategory.NullReference]);

        r.Code("n3.nullability.guiado", s, ItemPurpose.Guided, CodeMode.Function,
            "Completa NombreParaMostrar: si apodo no es null ni vacío, devuélvelo; si no, el nombre; si ambos son null o vacíos, «Usuario». Usa string.IsNullOrWhiteSpace.",
            """
            public static class Perfil
            {
                public static string NombreParaMostrar(string? nombre, string? apodo)
                {
                    return apodo!;
                }
            }
            """,
            """
            public static class Perfil
            {
                public static string NombreParaMostrar(string? nombre, string? apodo)
                {
                    if (!string.IsNullOrWhiteSpace(apodo)) return apodo;
                    if (!string.IsNullOrWhiteSpace(nombre)) return nombre;
                    return "Usuario";
                }
            }
            """,
            [
                Is("Apodo", "Perfil.NombreParaMostrar(\"Ana López\", \"anita\")", "anita"),
                Is("Sin apodo", "Perfil.NombreParaMostrar(\"Ana López\", null)", "Ana López"),
                Is("Apodo vacío", "Perfil.NombreParaMostrar(\"Luis\", \"  \")", "Luis", hidden: true, edge: true),
                Is("Nada", "Perfil.NombreParaMostrar(null, null)", "Usuario", hidden: true, edge: true),
            ],
            "Después de comprobar con IsNullOrWhiteSpace, el compilador sabe que el valor no es null y no pide el operador !.",
            ["Comprueba primero el apodo y después el nombre.", "!string.IsNullOrWhiteSpace(apodo) significa «tiene texto útil»."],
            targets: [ErrorCategory.NullReference]);

        r.Code("n3.nullability.indep", s, ItemPurpose.Independent, CodeMode.Function,
            "Escribe Buscar(List<Contacto> contactos, string nombre) que devuelva el Contacto? con ese nombre (sin distinguir mayúsculas) o null. Y Telefono(List<Contacto>, string nombre) que devuelva el teléfono del contacto o «sin teléfono» si el contacto no existe o su Telefono es null. Usa ?. y ??.",
            """
            public record Contacto(string Nombre, string? Telefono);

            public static class Agenda
            {
                public static Contacto? Buscar(List<Contacto> contactos, string nombre) => null;

                public static string Telefono(List<Contacto> contactos, string nombre) => "";
            }
            """,
            """
            public record Contacto(string Nombre, string? Telefono);

            public static class Agenda
            {
                public static Contacto? Buscar(List<Contacto> contactos, string nombre)
                {
                    foreach (Contacto c in contactos)
                    {
                        if (string.Equals(c.Nombre, nombre, StringComparison.OrdinalIgnoreCase)) return c;
                    }
                    return null;
                }

                public static string Telefono(List<Contacto> contactos, string nombre) =>
                    Buscar(contactos, nombre)?.Telefono ?? "sin teléfono";
            }
            """,
            [
                Is("Encontrado", "Agenda.Telefono([new Contacto(\"Ana\", \"555\")], \"ana\")", "555"),
                Is("No existe", "Agenda.Telefono([new Contacto(\"Ana\", \"555\")], \"Luis\")", "sin teléfono", edge: true),
                Is("Sin teléfono", "Agenda.Telefono([new Contacto(\"Eva\", null)], \"Eva\")", "sin teléfono", hidden: true, edge: true),
                Is("Buscar null", "Agenda.Buscar([], \"x\") is null", "true", hidden: true),
            ],
            "?. corta la cadena en cuanto encuentra null y ?? da el valor por defecto: una sola expresión cubre los dos casos.",
            ["string.Equals(a, b, StringComparison.OrdinalIgnoreCase) compara sin distinguir mayúsculas.", "Buscar(...)?.Telefono ?? \"sin teléfono\""],
            difficulty: 2, targets: [ErrorCategory.NullReference]);

        r.Code("n3.nullability.app", s, ItemPurpose.Transfer, CodeMode.Function,
            "Aplicación: leer configuración opcional. Configuracion.Leer(Dictionary<string, string?> valores) devuelve un record Opciones(string Idioma, int MinutosSesion, bool Oscuro) con estos valores por defecto: «es», 45, false. Usa el valor del diccionario solo si existe, no es null y es válido (MinutosSesion entre 5 y 90; Oscuro «true» o «false» sin distinguir mayúsculas).",
            """
            public record Opciones(string Idioma, int MinutosSesion, bool Oscuro);

            public static class Configuracion
            {
                public static Opciones Leer(Dictionary<string, string?> valores) =>
                    new(valores["idioma"]!, int.Parse(valores["minutos"]!), bool.Parse(valores["oscuro"]!));
            }
            """,
            """
            public record Opciones(string Idioma, int MinutosSesion, bool Oscuro);

            public static class Configuracion
            {
                public static Opciones Leer(Dictionary<string, string?> valores)
                {
                    string idioma = valores.GetValueOrDefault("idioma") is { Length: > 0 } i ? i : "es";

                    int minutos = 45;
                    if (int.TryParse(valores.GetValueOrDefault("minutos"), out int m) && m >= 5 && m <= 90)
                    {
                        minutos = m;
                    }

                    bool oscuro = bool.TryParse(valores.GetValueOrDefault("oscuro"), out bool o) && o;

                    return new Opciones(idioma, minutos, oscuro);
                }
            }
            """,
            [
                Is("Todo presente", "Configuracion.Leer(new() { [\"idioma\"] = \"en\", [\"minutos\"] = \"25\", [\"oscuro\"] = \"True\" })", "Opciones { Idioma = en, MinutosSesion = 25, Oscuro = True }"),
                Is("Vacío", "Configuracion.Leer([])", "Opciones { Idioma = es, MinutosSesion = 45, Oscuro = False }", edge: true),
                Is("Valores null", "Configuracion.Leer(new() { [\"idioma\"] = null, [\"minutos\"] = null })", "Opciones { Idioma = es, MinutosSesion = 45, Oscuro = False }", hidden: true, edge: true),
                Is("Fuera de rango", "Configuracion.Leer(new() { [\"minutos\"] = \"300\", [\"oscuro\"] = \"quizá\" })", "Opciones { Idioma = es, MinutosSesion = 45, Oscuro = False }", hidden: true, edge: true, hint: "Un valor presente pero inválido también usa el valor por defecto."),
            ],
            "Cada lectura tiene un valor por defecto: la configuración incompleta o corrupta nunca impide arrancar la aplicación.",
            ["GetValueOrDefault devuelve null si la clave no existe; TryParse acepta null y devuelve false.", "El patrón is { Length: > 0 } i comprueba a la vez que no sea null y que no esté vacío."],
            difficulty: 3, related: ["n1.validacion", "n2.diccionarios"], targets: [ErrorCategory.NullReference, ErrorCategory.EdgeCaseMissed]);

        return new Skill(s, UnitId, "Nulabilidad y prevención de errores con null",
            "Tipos anulables, ?. y ??.",
            Tier.Essential, ["n3.clases"],
            new Lesson(s,
                "Declarar cuándo un valor puede ser null, usar ?., ?? y ??= para manejarlo y aprovechar las advertencias del compilador para evitar NullReferenceException.",
                [
                    P("null significa «ninguna referencia». Usar un miembro de algo que es null lanza NullReferenceException, uno de los errores más frecuentes."),
                    P("Con los tipos de referencia anulables (activados por defecto en proyectos modernos), string? significa «puede ser null» y string «no debería serlo». El compilador avisa cuando usas un string? sin comprobarlo."),
                    Code("""
                        string? segundoNombre = null;
                        Console.WriteLine(segundoNombre?.Length);
                        Console.WriteLine(segundoNombre?.Length ?? 0);
                        segundoNombre ??= "(ninguno)";
                        Console.WriteLine(segundoNombre);
                        if (segundoNombre is not null)
                        {
                            Console.WriteLine(segundoNombre.ToUpper());
                        }
                        """, "?. , ?? , ??= e is not null"),
                    P("int? (Nullable<int>) permite que un tipo por valor también represente «sin valor»."),
                    Warn("El operador ! («confía en mí») silencia la advertencia pero no comprueba nada. Cada ! es un posible NullReferenceException."),
                    Design("Prefiere que los métodos devuelvan colecciones vacías en lugar de null, y valida los parámetros que no deberían ser null al inicio del método."),
                ],
                new WorkedExample("Encadenar accesos opcionales",
                    """
                    Pedido p1 = new(new Cliente("Ana", new Direccion("CDMX")));
                    Pedido p2 = new(new Cliente("Luis", null));
                    Pedido p3 = new(null);
                    foreach (Pedido p in new[] { p1, p2, p3 })
                    {
                        Console.WriteLine(p.Cliente?.Direccion?.Ciudad ?? "sin ciudad");
                    }

                    record Direccion(string Ciudad);
                    record Cliente(string Nombre, Direccion? Direccion);
                    record Pedido(Cliente? Cliente);
                    """,
                    [
                        Line(2, "Un cliente sin dirección."),
                        Line(3, "Un pedido sin cliente."),
                        Line(6, "La cadena se corta en el primer null; ?? da el valor por defecto."),
                        Line(10, "Los ? en los tipos documentan qué puede faltar."),
                    ],
                    "CDMX\nsin ciudad\nsin ciudad"),
                new CommonMistake(
                    "Silenciar la advertencia con ! en lugar de manejar el null.",
                    "string? linea = Console.ReadLine();\nint n = int.Parse(linea!);",
                    "Si la entrada se termina, linea es null y el programa falla. El ! solo escondió el aviso.",
                    "string? linea = Console.ReadLine();\nint n = int.TryParse(linea, out int v) ? v : 0;\nConsole.WriteLine(n);"),
                "n3.nullability.pred1", "n3.nullability.guiado", "n3.nullability.indep", "n3.nullability.app",
                ["n3.nullability.recall1", "n3.nullability.recall2", "n3.nullability.concepto1"],
                "Declaras explícitamente qué puede ser null, lo manejas con los operadores adecuados y evitas el operador ! salvo cuando está justificado.",
                15));
    }

    private static Skill Solid(ContentRegistry r)
    {
        const string s = "n3.solid";
        r.Choice("n3.solid.pred1", s, ItemPurpose.Prediction,
            "¿Qué muestra este programa?",
            "Saludo no crea su propio reloj: lo recibe (inyección de dependencias). Por eso se le puede pasar un reloj fijo y el resultado es predecible, algo esencial para probar el código.",
            [
                Ok("Buenas noches", "Correcto: el reloj fijo marca las 22 horas."),
                No("Depende de la hora actual", "Saludo usa el reloj que recibe, no la hora del sistema.", ErrorCategory.ConceptMisunderstanding),
                No("No compila", "RelojFijo implementa IReloj y puede pasarse al constructor.", ErrorCategory.ConceptMisunderstanding),
            ],
            code: """
                Saludo s = new(new RelojFijo(22));
                Console.WriteLine(s.Texto());

                interface IReloj { int Hora { get; } }

                class RelojFijo(int hora) : IReloj
                {
                    public int Hora => hora;
                }

                class Saludo(IReloj reloj)
                {
                    public string Texto() => reloj.Hora switch
                    {
                        < 12 => "Buenos días",
                        < 19 => "Buenas tardes",
                        _ => "Buenas noches",
                    };
                }
                """,
            dimension: KnowledgeDimension.Design);

        r.Choice("n3.solid.concepto1", s, ItemPurpose.Concept,
            "Una clase Factura calcula impuestos, genera el PDF y envía el correo. ¿Qué principio se recomienda revisar?",
            "Responsabilidad única (S): la clase cambia por tres motivos distintos (reglas fiscales, formato, envío). Separarla en piezas hace que cada cambio toque una sola clase.",
            [
                Ok("Responsabilidad única: tiene tres motivos distintos para cambiar.", "Correcto."),
                No("Sustitución de Liskov.", "Liskov trata de herencia; aquí no hay jerarquía."),
                No("Ninguno: es más simple tener todo en una clase.", "Puede serlo al principio, pero cada cambio arriesga romper las otras dos funciones."),
            ],
            dimension: KnowledgeDimension.Design);

        r.Choice("n3.solid.recall1", s, ItemPurpose.Recall,
            "¿Qué dice el principio de inversión de dependencias (D)?",
            "Los módulos de alto nivel (las reglas del negocio) no deberían depender de detalles concretos (base de datos, correo, reloj) sino de abstracciones (interfaces). Los detalles se inyectan.",
            [
                Ok("Depender de abstracciones (interfaces) en lugar de implementaciones concretas.", "Correcto."),
                No("Invertir el orden de los métodos de una clase.", "No tiene relación con el orden."),
                No("Que las clases derivadas no dependan de la base.", "Eso no es lo que afirma."),
            ]);

        r.Choice("n3.solid.recall2", s, ItemPurpose.Recall,
            "¿Qué afirma el principio abierto/cerrado (O)?",
            "El código debería poder extenderse con comportamiento nuevo (abierto) sin modificar lo que ya funciona (cerrado), por ejemplo agregando una implementación de una interfaz.",
            [
                Ok("Extender con comportamiento nuevo sin modificar el código existente.", "Correcto."),
                No("Que las clases deben ser públicas o privadas.", "No trata de modificadores de acceso."),
                No("Que un archivo abierto debe cerrarse.", "Eso es manejo de recursos."),
            ]);

        r.Choice("n3.solid.bug1", s, ItemPurpose.BugFinding,
            "Cuadrado hereda de Rectangulo y sincroniza ancho y alto. Este código, correcto para un rectángulo, falla con un cuadrado. ¿Qué principio se viola?",
            "Sustitución de Liskov (L): un Cuadrado debería poder usarse donde se espera un Rectangulo sin romper el comportamiento esperado. Asignar el ancho no debería cambiar el alto. Un cuadrado no es un rectángulo modificable.",
            [
                Ok("Sustitución de Liskov: el derivado rompe lo que se espera del tipo base.", "Correcto."),
                No("Segregación de interfaces.", "No hay interfaces involucradas.", ErrorCategory.ConceptMisunderstanding),
                No("Ninguno: es un error de aritmética.", "La aritmética es correcta; el problema es el diseño de la jerarquía.", ErrorCategory.ConceptMisunderstanding),
            ],
            code: """
                void Ampliar(Rectangulo r)
                {
                    r.Ancho = 5;
                    r.Alto = 4;
                    Console.WriteLine(r.Area()); // se espera 20; con un Cuadrado da 16
                }
                """,
            dimension: KnowledgeDimension.Design, targets: [ErrorCategory.InheritanceContract]);

        r.Code("n3.solid.guiado", s, ItemPurpose.Guided, CodeMode.Function,
            "Aplica inversión de dependencias: Recordatorio usa DateTime.Now directamente y no se puede probar. Haz que reciba un IReloj en el constructor y use reloj.Ahora. RelojFijo ya está escrito para las pruebas.",
            """
            public interface IReloj { DateTime Ahora { get; } }

            public class RelojFijo(DateTime ahora) : IReloj
            {
                public DateTime Ahora => ahora;
            }

            public class Recordatorio(DateTime limite)
            {
                public bool Vencido() => DateTime.Now > limite;

                public int DiasRestantes() => (limite.Date - DateTime.Now.Date).Days;
            }
            """,
            """
            public interface IReloj { DateTime Ahora { get; } }

            public class RelojFijo(DateTime ahora) : IReloj
            {
                public DateTime Ahora => ahora;
            }

            public class Recordatorio(DateTime limite, IReloj reloj)
            {
                public bool Vencido() => reloj.Ahora > limite;

                public int DiasRestantes() => (limite.Date - reloj.Ahora.Date).Days;
            }
            """,
            [
                Is("Faltan días", "new Recordatorio(new DateTime(2030, 1, 10), new RelojFijo(new DateTime(2030, 1, 3))).DiasRestantes()", "7"),
                Is("Vencido", "new Recordatorio(new DateTime(2030, 1, 10), new RelojFijo(new DateTime(2030, 1, 11))).Vencido()", "true"),
                Is("No vencido", "new Recordatorio(new DateTime(2030, 1, 10), new RelojFijo(new DateTime(2030, 1, 9))).Vencido()", "false", hidden: true),
            ],
            "Al recibir el reloj, Recordatorio funciona igual en producción (con un reloj real) y en pruebas (con uno fijo).",
            ["Agrega un segundo parámetro IReloj reloj al constructor primario.", "Reemplaza DateTime.Now por reloj.Ahora."],
            requires: [CodeConstruct.Interface], dimension: KnowledgeDimension.Design);

        r.Code("n3.solid.indep", s, ItemPurpose.Independent, CodeMode.Function,
            "Aplica abierto/cerrado: CalculadoraEnvio usa un switch por tipo de envío y hay que modificarla para cada tipo nuevo. Crea la interfaz ITarifa (decimal Costo(decimal kg)), las clases TarifaNormal (40 + 8·kg), TarifaExpress (90 + 12·kg) y TarifaGratis (0), y haz que CalculadoraEnvio.Total(decimal kg, ITarifa tarifa) delegue en la tarifa.",
            """
            public static class CalculadoraEnvio
            {
                public static decimal Total(decimal kg, string tipo) => tipo switch
                {
                    "normal" => 40 + 8 * kg,
                    "express" => 90 + 12 * kg,
                    _ => throw new ArgumentException("Tipo desconocido"),
                };
            }
            """,
            """
            public interface ITarifa
            {
                decimal Costo(decimal kg);
            }

            public class TarifaNormal : ITarifa
            {
                public decimal Costo(decimal kg) => 40 + 8 * kg;
            }

            public class TarifaExpress : ITarifa
            {
                public decimal Costo(decimal kg) => 90 + 12 * kg;
            }

            public class TarifaGratis : ITarifa
            {
                public decimal Costo(decimal kg) => 0;
            }

            public static class CalculadoraEnvio
            {
                public static decimal Total(decimal kg, ITarifa tarifa) => tarifa.Costo(kg);
            }
            """,
            [
                Is("Normal", "CalculadoraEnvio.Total(2.5m, new TarifaNormal())", "60.0"),
                Is("Express", "CalculadoraEnvio.Total(1m, new TarifaExpress())", "102"),
                Is("Gratis", "CalculadoraEnvio.Total(10m, new TarifaGratis())", "0", hidden: true),
            ],
            "Agregar una tarifa nueva ya no exige tocar CalculadoraEnvio: basta con otra clase que implemente ITarifa.",
            ["Declara la interfaz con un único método Costo(decimal kg).", "Total recibe ITarifa y devuelve tarifa.Costo(kg)."],
            difficulty: 2, requires: [CodeConstruct.Interface], related: ["n3.abstractas-interfaces"], dimension: KnowledgeDimension.Design);

        r.Code("n3.solid.app", s, ItemPurpose.Transfer, CodeMode.Function,
            "Aplicación: separa responsabilidades en un registro de usuarios. ServicioRegistro(IRepositorioUsuarios repo, INotificador notificador) con Registrar(string correo): valida el correo (debe contener @; si no, ArgumentException), rechaza duplicados (InvalidOperationException si repo.Existe(correo)), guarda con repo.Guardar(correo) y llama a notificador.Bienvenida(correo). Las implementaciones en memoria para pruebas ya están escritas.",
            """
            public interface IRepositorioUsuarios
            {
                bool Existe(string correo);
                void Guardar(string correo);
            }

            public interface INotificador
            {
                void Bienvenida(string correo);
            }

            public class RepoMemoria : IRepositorioUsuarios
            {
                public List<string> Guardados { get; } = [];
                public bool Existe(string correo) => Guardados.Contains(correo);
                public void Guardar(string correo) => Guardados.Add(correo);
            }

            public class NotificadorFalso : INotificador
            {
                public List<string> Enviados { get; } = [];
                public void Bienvenida(string correo) => Enviados.Add(correo);
            }

            public class ServicioRegistro
            {
            }
            """,
            """
            public interface IRepositorioUsuarios
            {
                bool Existe(string correo);
                void Guardar(string correo);
            }

            public interface INotificador
            {
                void Bienvenida(string correo);
            }

            public class RepoMemoria : IRepositorioUsuarios
            {
                public List<string> Guardados { get; } = [];
                public bool Existe(string correo) => Guardados.Contains(correo);
                public void Guardar(string correo) => Guardados.Add(correo);
            }

            public class NotificadorFalso : INotificador
            {
                public List<string> Enviados { get; } = [];
                public void Bienvenida(string correo) => Enviados.Add(correo);
            }

            public class ServicioRegistro(IRepositorioUsuarios repo, INotificador notificador)
            {
                public void Registrar(string correo)
                {
                    string normal = correo.Trim().ToLowerInvariant();
                    if (!normal.Contains('@')) throw new ArgumentException("Correo inválido", nameof(correo));
                    if (repo.Existe(normal)) throw new InvalidOperationException("El correo ya está registrado");
                    repo.Guardar(normal);
                    notificador.Bienvenida(normal);
                }
            }
            """,
            [
                Is("Registra y notifica", "{ var r = new RepoMemoria(); var n = new NotificadorFalso(); new ServicioRegistro(r, n).Registrar(\"ana@x.com\"); return $\"{r.Guardados.Count} {n.Enviados.Count}\"; }", "1 1"),
                Throws("Correo inválido", "{ new ServicioRegistro(new RepoMemoria(), new NotificadorFalso()).Registrar(\"sin-arroba\"); return 0; }", "ArgumentException"),
                Throws("Duplicado", "{ var s = new ServicioRegistro(new RepoMemoria(), new NotificadorFalso()); s.Registrar(\"a@x.com\"); s.Registrar(\"a@x.com\"); return 0; }", "InvalidOperationException", hidden: true),
                Is("Falla no notifica", "{ var r = new RepoMemoria(); var n = new NotificadorFalso(); var s = new ServicioRegistro(r, n); s.Registrar(\"a@x.com\"); try { s.Registrar(\"a@x.com\"); } catch (InvalidOperationException) { } return n.Enviados.Count; }", "1", hidden: true, edge: true, hint: "Valida antes de guardar y de notificar."),
            ],
            "El servicio contiene solo la regla del negocio; guardar y notificar son detalles que se inyectan y se reemplazan por versiones en memoria al probar.",
            ["Recibe ambas interfaces en un constructor primario.", "Orden: validar formato, comprobar duplicado, guardar, notificar."],
            difficulty: 3, requires: [CodeConstruct.Interface], related: ["n3.nullability"], dimension: KnowledgeDimension.Design);

        return new Skill(s, UnitId, "Principios SOLID con ejemplos",
            "Diseño mantenible sin dogmas.",
            Tier.Advanced, ["n3.abstractas-interfaces"],
            new Lesson(s,
                "Reconocer los cinco principios SOLID en código concreto y aplicarlos donde reducen el costo de cambiar el programa, sin convertirlos en reglas absolutas.",
                [
                    P("SOLID agrupa cinco recomendaciones de diseño orientado a objetos. No son leyes: son herramientas para que un cambio toque pocas partes del programa."),
                    P("S — Responsabilidad única: una clase debería tener un solo motivo para cambiar. O — Abierto/cerrado: extender sin modificar lo que funciona. L — Sustitución de Liskov: un derivado debe poder usarse donde se espera la base sin sorpresas."),
                    P("I — Segregación de interfaces: interfaces pequeñas y específicas mejor que una enorme. D — Inversión de dependencias: las reglas del negocio dependen de interfaces, y los detalles (base de datos, reloj, correo) se inyectan."),
                    Code("""
                        Notificador n = new(new CanalConsola());
                        n.Avisar("Pago recibido");

                        interface ICanal { void Enviar(string mensaje); }

                        class CanalConsola : ICanal
                        {
                            public void Enviar(string mensaje) => Console.WriteLine($"[consola] {mensaje}");
                        }

                        class Notificador(ICanal canal)
                        {
                            public void Avisar(string evento) => canal.Enviar($"Aviso: {evento}");
                        }
                        """, "Inversión de dependencias: el canal se inyecta"),
                    Design("Aplica un principio cuando resuelve un problema real (código difícil de probar, cambios que tocan muchas clases). Abstraer todo por adelantado también tiene costo: más tipos y más indirección."),
                ],
                new WorkedExample("De una clase que hace todo a piezas con una responsabilidad",
                    """
                    IFormato formato = new FormatoTexto();
                    ICalculoImpuesto impuesto = new IvaMexico();
                    GeneradorRecibo gen = new(impuesto, formato);
                    Console.WriteLine(gen.Generar("Curso C#", 1000m));

                    interface ICalculoImpuesto { decimal Calcular(decimal subtotal); }
                    interface IFormato { string Dar(string concepto, decimal subtotal, decimal impuesto); }

                    class IvaMexico : ICalculoImpuesto
                    {
                        public decimal Calcular(decimal subtotal) => Math.Round(subtotal * 0.16m, 2);
                    }

                    class FormatoTexto : IFormato
                    {
                        public string Dar(string concepto, decimal subtotal, decimal impuesto) =>
                            $"{concepto}: {subtotal:F2} + IVA {impuesto:F2} = {subtotal + impuesto:F2}";
                    }

                    class GeneradorRecibo(ICalculoImpuesto impuesto, IFormato formato)
                    {
                        public string Generar(string concepto, decimal subtotal) =>
                            formato.Dar(concepto, subtotal, impuesto.Calcular(subtotal));
                    }
                    """,
                    [
                        Line(3, "Las piezas se ensamblan en un solo lugar."),
                        Line(9, "El cálculo de impuestos cambia por motivos fiscales…"),
                        Line(14, "…y el formato, por motivos de presentación: clases separadas (S)."),
                        Line(20, "El generador depende de interfaces (D) y acepta nuevos formatos sin cambiar (O)."),
                    ],
                    "Curso C#: 1000.00 + IVA 160.00 = 1160.00"),
                new CommonMistake(
                    "Crear las dependencias dentro de la clase.",
                    "class Recordatorio\n{\n    public bool Vencido(DateTime limite) => DateTime.Now > limite;\n}",
                    "El resultado depende de la hora real: no se puede probar de forma repetible ni simular otra fecha.",
                    "interface IReloj { DateTime Ahora { get; } }\n\nclass Recordatorio(IReloj reloj)\n{\n    public bool Vencido(DateTime limite) => reloj.Ahora > limite;\n}"),
                "n3.solid.pred1", "n3.solid.guiado", "n3.solid.indep", "n3.solid.app",
                ["n3.solid.recall1", "n3.solid.recall2", "n3.solid.concepto1"],
                "Identificas qué principio SOLID ayuda en un problema concreto, refactorizas hacia él y justificas cuándo no vale la pena aplicarlo.",
                25));
    }
}
