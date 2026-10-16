using CSharpMastery.Content.Infrastructure;
using CSharpMastery.Domain.Curriculum;
using CSharpMastery.Domain.Items;
using static CSharpMastery.Content.Infrastructure.ContentRegistry;

namespace CSharpMastery.Content.Nivel7;

/// <summary>
/// Proyectos integradores. Cada hito es un ejercicio de código con pruebas visibles y ocultas; el código inicial
/// de cada hito incluye lo construido en los anteriores, de modo que el proyecto crece paso a paso.
/// </summary>
internal static class Projects
{
    public static IReadOnlyList<IntegrativeProject> Build(ContentRegistry r) => [Gastos(r), Biblioteca(r), Tarjetas(r)];

    private static CodeExercise Hito(ContentRegistry r, string id, string skill, CodeMode mode, string prompt, string starter, string solution,
        CodeTestCase[] tests, string explanation, string[] hints, CodeConstruct[]? requires = null) =>
        r.Code(id, skill, ItemPurpose.Project, mode, prompt, starter, solution, tests, explanation, hints,
            difficulty: 4, requires: requires, minutes: 25);

    // ───────────────────────── Proyecto 1: control de gastos ─────────────────────────

    private const string GastosModelo = """
        public record Movimiento(DateOnly Fecha, string Categoria, decimal Monto);
        """;

    private const string GastosParser = """
        public static class Parser
        {
            public static Movimiento Leer(string linea)
            {
                string[] partes = linea.Split(';');
                if (partes.Length != 3)
                {
                    throw new FormatException("Se esperaban 3 campos separados por ;");
                }
                if (!DateOnly.TryParseExact(partes[0].Trim(), "yyyy-MM-dd", out DateOnly fecha))
                {
                    throw new FormatException("Fecha inválida");
                }
                string categoria = partes[1].Trim().ToLowerInvariant();
                if (categoria.Length == 0)
                {
                    throw new FormatException("Falta la categoría");
                }
                if (!decimal.TryParse(partes[2].Trim(), out decimal monto) || monto == 0)
                {
                    throw new FormatException("Monto inválido");
                }
                return new Movimiento(fecha, categoria, monto);
            }
        }
        """;

    private const string GastosResumen = """
        public static class Resumen
        {
            public static List<(string Categoria, decimal Total)> GastosPorCategoria(List<Movimiento> movimientos)
            {
                Dictionary<string, decimal> totales = [];
                foreach (Movimiento m in movimientos)
                {
                    if (m.Monto < 0)
                    {
                        totales[m.Categoria] = totales.GetValueOrDefault(m.Categoria) - m.Monto;
                    }
                }

                List<(string Categoria, decimal Total)> lista = [];
                foreach (KeyValuePair<string, decimal> par in totales)
                {
                    lista.Add((par.Key, par.Value));
                }
                lista.Sort(Comparar);
                return lista;
            }

            private static int Comparar((string Categoria, decimal Total) a, (string Categoria, decimal Total) b) =>
                a.Total != b.Total ? b.Total.CompareTo(a.Total) : string.CompareOrdinal(a.Categoria, b.Categoria);
        }
        """;

    private const string GastosReporte = """
        public static class Reporte
        {
            public static string Mensual(List<Movimiento> movimientos, int anio, int mes)
            {
                List<Movimiento> delMes = [];
                foreach (Movimiento m in movimientos)
                {
                    if (m.Fecha.Year == anio && m.Fecha.Month == mes)
                    {
                        delMes.Add(m);
                    }
                }

                List<string> lineas = [$"Reporte {anio}-{mes:D2}"];
                if (delMes.Count == 0)
                {
                    lineas.Add("Sin movimientos");
                    return string.Join("\n", lineas);
                }

                decimal ingresos = 0;
                decimal gastos = 0;
                foreach (Movimiento m in delMes)
                {
                    if (m.Monto > 0) ingresos += m.Monto;
                    else gastos -= m.Monto;
                }
                lineas.Add($"Ingresos: {ingresos:F2}");
                lineas.Add($"Gastos: {gastos:F2}");
                lineas.Add($"Balance: {ingresos - gastos:F2}");

                List<(string Categoria, decimal Total)> categorias = Resumen.GastosPorCategoria(delMes);
                lineas.Add(categorias.Count > 0
                    ? $"Mayor gasto: {categorias[0].Categoria} ({categorias[0].Total:F2})"
                    : "Mayor gasto: ninguno");
                return string.Join("\n", lineas);
            }
        }
        """;

    private const string GastosPrograma = """
        List<Movimiento> movimientos = [];
        int numero = 0;
        string? linea = Console.ReadLine();
        while (linea is not null && linea != "fin")
        {
            numero++;
            try
            {
                movimientos.Add(Parser.Leer(linea));
            }
            catch (FormatException)
            {
                Console.WriteLine($"Línea {numero} inválida");
            }
            linea = Console.ReadLine();
        }

        decimal balance = 0;
        foreach (Movimiento m in movimientos)
        {
            balance += m.Monto;
        }
        Console.WriteLine($"Movimientos: {movimientos.Count}");
        Console.WriteLine($"Balance: {balance:F2}");
        foreach ((string categoria, decimal total) in Resumen.GastosPorCategoria(movimientos))
        {
            Console.WriteLine($"{categoria}: {total:F2}");
        }
        """;

    private static string Join(params string[] partes) => string.Join("\n\n", partes);

    private const string MovimientosDemo =
        "[Parser.Leer(\"2025-03-01;sueldo;15000\"), Parser.Leer(\"2025-03-02;renta;-5000\"), Parser.Leer(\"2025-03-05;comida;-200.50\"), " +
        "Parser.Leer(\"2025-03-09;comida;-100\"), Parser.Leer(\"2025-04-01;renta;-5000\")]";

    private static IntegrativeProject Gastos(ContentRegistry r)
    {
        const string p = "proyecto.gastos";

        Hito(r, $"{p}.h1", "n3.records", CodeMode.Function,
            "Hito 1: el modelo y la lectura de datos. Implementa Parser.Leer(string linea) para líneas con el formato «AAAA-MM-DD;categoría;monto» (por ejemplo «2025-03-14;Comida;-120.50»). Los gastos son negativos y los ingresos positivos. La categoría se guarda sin espacios en los extremos y en minúsculas. Lanza FormatException si no hay exactamente 3 campos, la fecha no es válida, la categoría está vacía o el monto no es un número distinto de 0.",
            Join(GastosModelo, """
                public static class Parser
                {
                    public static Movimiento Leer(string linea)
                    {
                        throw new NotImplementedException();
                    }
                }
                """),
            Join(GastosModelo, GastosParser),
            [
                Is("Línea válida", "{ var m = Parser.Leer(\"2025-03-14;Comida ;-120.50\"); return $\"{m.Fecha:yyyy-MM-dd} {m.Categoria} {m.Monto}\"; }", "2025-03-14 comida -120.50"),
                Is("Ingreso", "Parser.Leer(\"2025-03-01;sueldo;15000\").Monto", "15000"),
                Throws("Pocos campos", "Parser.Leer(\"2025-03-01;renta\")", "FormatException"),
                Throws("Fecha imposible", "Parser.Leer(\"2025-02-30;renta;-10\")", "FormatException", hidden: true, hint: "DateOnly.TryParseExact rechaza fechas que no existen."),
                Throws("Categoría vacía", "Parser.Leer(\"2025-03-01;  ;-10\")", "FormatException", hidden: true),
                Throws("Monto no numérico", "Parser.Leer(\"2025-03-01;renta;mucho\")", "FormatException", hidden: true),
                Throws("Monto cero", "Parser.Leer(\"2025-03-01;renta;0\")", "FormatException", hidden: true),
            ],
            "Toda la validación de la entrada vive en un solo lugar; el resto del programa trabaja con objetos Movimiento que ya son válidos.",
            ["Divide con Split(';') y comprueba que haya 3 partes.", "DateOnly.TryParseExact(texto, \"yyyy-MM-dd\", out DateOnly fecha) y decimal.TryParse validan sin lanzar excepciones; lanza tú la FormatException con un mensaje claro."]);

        Hito(r, $"{p}.h2", "n2.diccionarios", CodeMode.Function,
            "Hito 2: gastos por categoría. Implementa Resumen.GastosPorCategoria(List<Movimiento>) que devuelva una lista de (Categoria, Total) solo con los gastos, con el total en positivo, ordenada de mayor a menor total y, en empate, por nombre de categoría.",
            Join(GastosModelo, GastosParser, """
                public static class Resumen
                {
                    public static List<(string Categoria, decimal Total)> GastosPorCategoria(List<Movimiento> movimientos)
                    {
                        return [];
                    }
                }
                """),
            Join(GastosModelo, GastosParser, GastosResumen),
            [
                Is("Varias categorías", $"Resumen.GastosPorCategoria({MovimientosDemo})", "[(\"renta\", 10000), (\"comida\", 300.50)]"),
                Is("Sin gastos", "Resumen.GastosPorCategoria([Parser.Leer(\"2025-01-01;sueldo;100\")]).Count", "0", edge: true),
                Is("Empate alfabético", "Resumen.GastosPorCategoria([Parser.Leer(\"2025-01-01;taxi;-50\"), Parser.Leer(\"2025-01-02;cine;-50\")])", "[(\"cine\", 50), (\"taxi\", 50)]", hidden: true, edge: true),
            ],
            "Un Dictionary acumula por categoría y después se ordena una lista. List.Sort acepta un método que compara dos elementos y devuelve un número negativo, cero o positivo.",
            ["Acumula con totales[m.Categoria] = totales.GetValueOrDefault(m.Categoria) - m.Monto; solo para montos negativos.", "Escribe un método static int Comparar(a, b) y pásalo a lista.Sort(Comparar)."]);

        Hito(r, $"{p}.h3", "n2.strings", CodeMode.Function,
            "Hito 3: reporte mensual. Implementa Reporte.Mensual(movimientos, anio, mes) que devuelva un texto de varias líneas: «Reporte AAAA-MM», «Ingresos: X», «Gastos: Y», «Balance: Z» y «Mayor gasto: categoría (total)», con 2 decimales. Si no hay movimientos en ese mes, solo «Reporte AAAA-MM» y «Sin movimientos». Si hay movimientos pero ningún gasto, la última línea es «Mayor gasto: ninguno».",
            Join(GastosModelo, GastosParser, GastosResumen, """
                public static class Reporte
                {
                    public static string Mensual(List<Movimiento> movimientos, int anio, int mes)
                    {
                        return "";
                    }
                }
                """),
            Join(GastosModelo, GastosParser, GastosResumen, GastosReporte),
            [
                Is("Marzo", $"Reporte.Mensual({MovimientosDemo}, 2025, 3)", "Reporte 2025-03\nIngresos: 15000.00\nGastos: 5300.50\nBalance: 9699.50\nMayor gasto: renta (5000.00)"),
                Is("Mes vacío", $"Reporte.Mensual({MovimientosDemo}, 2025, 7)", "Reporte 2025-07\nSin movimientos", edge: true),
                Is("Solo ingresos", "Reporte.Mensual([Parser.Leer(\"2025-05-10;venta;300\")], 2025, 5)", "Reporte 2025-05\nIngresos: 300.00\nGastos: 0.00\nBalance: 300.00\nMayor gasto: ninguno", hidden: true, edge: true),
                Is("Otro año", $"Reporte.Mensual({MovimientosDemo}, 2024, 3)", "Reporte 2024-03\nSin movimientos", hidden: true, edge: true, hint: "Filtra por año y por mes."),
            ],
            "El reporte reutiliza GastosPorCategoria del hito anterior en lugar de recalcular las categorías.",
            ["Filtra primero los movimientos del mes en una lista nueva.", "Arma las líneas en una List<string> y únelas con string.Join(\"\\n\", lineas)."]);

        Hito(r, $"{p}.h4", "n1.validacion", CodeMode.Program,
            "Hito 4: el programa completo. Lee líneas de movimientos hasta «fin» o el fin de la entrada. Por cada línea inválida muestra «Línea N inválida» (N cuenta todas las líneas leídas, desde 1) y continúa. Al final muestra «Movimientos: X», «Balance: Y» (2 decimales) y, por cada categoría de gasto en el orden de GastosPorCategoria, «categoría: total».",
            "// Escribe aquí el programa. Las clases de los hitos anteriores ya están abajo.\n\n" + Join(GastosModelo, GastosParser, GastosResumen),
            Join(GastosPrograma, GastosModelo, GastosParser, GastosResumen),
            [
                In("Mes típico", "2025-03-01;sueldo;15000\n2025-03-02;renta;-5000\n2025-03-05;comida;-200.50\nfin", "Movimientos: 3\nBalance: 9799.50\nrenta: 5000.00\ncomida: 200.50"),
                In("Líneas inválidas", "2025-03-01;sueldo;100\nhola\n2025-13-01;x;5\n2025-03-02;cafe;-30\nfin", "Línea 2 inválida\nLínea 3 inválida\nMovimientos: 2\nBalance: 70.00\ncafe: 30.00"),
                In("Sin fin", "2025-03-01;libros;-80", "Movimientos: 1\nBalance: -80.00\nlibros: 80.00", hidden: true, edge: true, hint: "Si ReadLine devuelve null, la entrada terminó aunque no haya «fin»."),
                In("Vacío", "fin", "Movimientos: 0\nBalance: 0.00", hidden: true, edge: true),
            ],
            "try/catch convierte un error de formato en un mensaje para la persona y permite seguir procesando el resto del archivo.",
            ["Usa un while que termine con «fin» o con null, y un contador de líneas.", "Envuelve Parser.Leer en try/catch (FormatException)."],
            [CodeConstruct.TryCatch, CodeConstruct.Loop]);

        return new IntegrativeProject(p, "Control de gastos personales",
            "Una herramienta de consola que lee movimientos de dinero, valida cada línea, resume los gastos por categoría y genera un reporte mensual.",
            ["n1.validacion", "n2.strings", "n2.diccionarios", "n3.records", "n2.metodos"],
            [
                "Leer movimientos en formato «AAAA-MM-DD;categoría;monto».",
                "Rechazar líneas inválidas sin detener el programa.",
                "Agrupar los gastos por categoría y ordenarlos.",
                "Generar un reporte mensual con ingresos, gastos, balance y mayor gasto.",
            ],
            [
                "Todas las pruebas visibles y ocultas de los cuatro hitos pasan.",
                "Ninguna línea mal escrita provoca que el programa termine con una excepción.",
                "Los montos se muestran siempre con 2 decimales.",
                "La lógica de lectura, resumen y reporte está en clases separadas.",
            ],
            [
                new("Lectura y validación", "Modelo Movimiento y lectura validada de una línea.", $"{p}.h1"),
                new("Resumen por categoría", "Totales de gasto por categoría, ordenados.", $"{p}.h2"),
                new("Reporte mensual", "Texto con ingresos, gastos, balance y mayor gasto del mes.", $"{p}.h3"),
                new("Programa completo", "Lectura de la entrada, manejo de errores y salida final.", $"{p}.h4"),
            ],
            [
                new("Corrección", "Algunas pruebas fallan o hay casos sin contemplar.", "Todas las pruebas pasan.", "Además, probaste por tu cuenta casos no incluidos (montos con muchos decimales, categorías con mayúsculas)."),
                new("Validación y errores", "Una entrada inválida puede detener el programa.", "Las líneas inválidas se informan y se ignoran.", "Los mensajes de error explican qué estaba mal en la línea."),
                new("Organización del código", "Todo está en un solo bloque o hay lógica repetida.", "Lectura, resumen y reporte están separados en clases.", "Cada método hace una sola cosa y sus nombres lo explican sin comentarios."),
                new("Legibilidad", "Nombres poco claros o código difícil de seguir.", "Nombres descriptivos y formato consistente.", "Otra persona podría agregar una función nueva (por ejemplo, filtrar por categoría) sin preguntarte nada."),
            ]);
    }

    // ───────────────────────── Proyecto 2: biblioteca ─────────────────────────

    private const string BibliotecaModelo = """
        public class Libro(string isbn, string titulo)
        {
            public string Isbn => isbn;
            public string Titulo => titulo;
            public bool Disponible { get; internal set; } = true;
        }

        public class Socio(int id, string nombre)
        {
            public int Id => id;
            public string Nombre => nombre;
            public int Prestamos { get; internal set; }
        }

        public record Prestamo(string Isbn, int SocioId, DateOnly Fecha, DateOnly Vence);
        """;

    private const string BibliotecaDemo = """
        public static class Demo
        {
            public static Biblioteca Crear(Biblioteca? biblioteca = null)
            {
                Biblioteca b = biblioteca ?? new Biblioteca();
                b.AgregarLibro(new Libro("111", "Rayuela"));
                b.AgregarLibro(new Libro("222", "Ficciones"));
                b.AgregarLibro(new Libro("333", "Aura"));
                b.AgregarLibro(new Libro("444", "Pedro Páramo"));
                b.RegistrarSocio(new Socio(1, "Ana"));
                b.RegistrarSocio(new Socio(2, "Luis"));
                return b;
            }
        }
        """;

    private const string BibliotecaCabecera = """
        public class Biblioteca
        {
            public const int LimitePrestamos = 3;
            public const int DiasPrestamo = 14;

            private readonly Dictionary<string, Libro> _libros = [];
            private readonly Dictionary<int, Socio> _socios = [];
            private readonly List<Prestamo> _activos = [];

            public IReadOnlyList<Prestamo> Activos => _activos;

            public void AgregarLibro(Libro libro) => _libros.Add(libro.Isbn, libro);

            public void RegistrarSocio(Socio socio) => _socios.Add(socio.Id, socio);
        """;

    private const string BibliotecaPrestar = """
            public Prestamo Prestar(string isbn, int socioId, DateOnly fecha)
            {
                Libro libro = _libros.TryGetValue(isbn, out Libro? l) ? l : throw new KeyNotFoundException($"No existe el libro {isbn}");
                Socio socio = _socios.TryGetValue(socioId, out Socio? s) ? s : throw new KeyNotFoundException($"No existe el socio {socioId}");
                if (!libro.Disponible)
                {
                    throw new InvalidOperationException($"{libro.Titulo} no está disponible");
                }
                if (socio.Prestamos >= LimitePrestamos)
                {
                    throw new InvalidOperationException($"{socio.Nombre} alcanzó el límite de préstamos");
                }

                Prestamo prestamo = new(isbn, socioId, fecha, fecha.AddDays(DiasPrestamo));
                libro.Disponible = false;
                socio.Prestamos++;
                _activos.Add(prestamo);
                return prestamo;
            }
        """;

    private const string BibliotecaDevolverFijo = """
            public decimal Devolver(string isbn, DateOnly fecha)
            {
                Prestamo prestamo = BuscarActivo(isbn);
                _activos.Remove(prestamo);
                _libros[isbn].Disponible = true;
                _socios[prestamo.SocioId].Prestamos--;
                int retraso = fecha.DayNumber - prestamo.Vence.DayNumber;
                return retraso > 0 ? retraso * 5m : 0m;
            }

            private Prestamo BuscarActivo(string isbn)
            {
                foreach (Prestamo p in _activos)
                {
                    if (p.Isbn == isbn) return p;
                }
                throw new InvalidOperationException($"El libro {isbn} no está prestado");
            }
        """;

    private const string BibliotecaPoliticas = """
        public interface IPoliticaMulta
        {
            decimal Calcular(int diasRetraso);
        }

        public class PoliticaEstandar : IPoliticaMulta
        {
            public decimal Calcular(int diasRetraso) => diasRetraso > 0 ? diasRetraso * 5m : 0m;
        }

        public class PoliticaConTope(decimal tope) : IPoliticaMulta
        {
            public decimal Calcular(int diasRetraso) => Math.Min(tope, new PoliticaEstandar().Calcular(diasRetraso));
        }

        public class SinMulta : IPoliticaMulta
        {
            public decimal Calcular(int diasRetraso) => 0m;
        }
        """;

    private const string BibliotecaCabeceraConPolitica = """
        public class Biblioteca(IPoliticaMulta politica)
        {
            public const int LimitePrestamos = 3;
            public const int DiasPrestamo = 14;

            private readonly Dictionary<string, Libro> _libros = [];
            private readonly Dictionary<int, Socio> _socios = [];
            private readonly List<Prestamo> _activos = [];

            public Biblioteca() : this(new PoliticaEstandar()) { }

            public IReadOnlyList<Prestamo> Activos => _activos;

            public void AgregarLibro(Libro libro) => _libros.Add(libro.Isbn, libro);

            public void RegistrarSocio(Socio socio) => _socios.Add(socio.Id, socio);
        """;

    private const string BibliotecaDevolverConPolitica = """
            public decimal Devolver(string isbn, DateOnly fecha)
            {
                Prestamo prestamo = BuscarActivo(isbn);
                _activos.Remove(prestamo);
                _libros[isbn].Disponible = true;
                _socios[prestamo.SocioId].Prestamos--;
                int retraso = fecha.DayNumber - prestamo.Vence.DayNumber;
                return politica.Calcular(Math.Max(0, retraso));
            }

            private Prestamo BuscarActivo(string isbn)
            {
                foreach (Prestamo p in _activos)
                {
                    if (p.Isbn == isbn) return p;
                }
                throw new InvalidOperationException($"El libro {isbn} no está prestado");
            }
        """;

    private const string BibliotecaVencidos = """
            public List<string> Vencidos(DateOnly hoy)
            {
                List<(int Dias, string Titulo, string Linea)> vencidos = [];
                foreach (Prestamo p in _activos)
                {
                    int dias = hoy.DayNumber - p.Vence.DayNumber;
                    if (dias > 0)
                    {
                        string titulo = _libros[p.Isbn].Titulo;
                        vencidos.Add((dias, titulo, $"{titulo} — {_socios[p.SocioId].Nombre} ({dias} días)"));
                    }
                }
                vencidos.Sort(Comparar);

                List<string> lineas = [];
                foreach (var v in vencidos)
                {
                    lineas.Add(v.Linea);
                }
                return lineas;
            }

            private static int Comparar((int Dias, string Titulo, string Linea) a, (int Dias, string Titulo, string Linea) b) =>
                a.Dias != b.Dias ? b.Dias.CompareTo(a.Dias) : string.CompareOrdinal(a.Titulo, b.Titulo);
        """;

    private static string Clase(params string[] miembros) => string.Join("\n", miembros) + "}\n";

    private static IntegrativeProject Biblioteca(ContentRegistry r)
    {
        const string p = "proyecto.biblioteca";
        const string fecha = "new DateOnly(2025, 3, 1)";

        Hito(r, $"{p}.h1", "n3.encapsulacion", CodeMode.Function,
            "Hito 1: préstamos. Implementa Biblioteca.Prestar(isbn, socioId, fecha): lanza KeyNotFoundException si el libro o el socio no existen e InvalidOperationException si el libro no está disponible o el socio ya tiene LimitePrestamos préstamos. Si todo es válido, marca el libro como no disponible, suma un préstamo al socio, registra el préstamo (vence DiasPrestamo días después) y lo devuelve. Demo.Crear() prepara 4 libros y 2 socios para las pruebas.",
            Join(BibliotecaModelo, Clase(BibliotecaCabecera, """
                    public Prestamo Prestar(string isbn, int socioId, DateOnly fecha)
                    {
                        throw new NotImplementedException();
                    }
                """), BibliotecaDemo),
            Join(BibliotecaModelo, Clase(BibliotecaCabecera, BibliotecaPrestar), BibliotecaDemo),
            [
                Is("Préstamo válido", $"{{ var b = Demo.Crear(); var p = b.Prestar(\"111\", 1, {fecha}); return $\"{{p.Vence:yyyy-MM-dd}} {{b.Activos.Count}}\"; }}", "2025-03-15 1"),
                Throws("Libro ya prestado", $"{{ var b = Demo.Crear(); b.Prestar(\"111\", 1, {fecha}); b.Prestar(\"111\", 2, {fecha}); return 0; }}", "InvalidOperationException"),
                Throws("Libro inexistente", $"Demo.Crear().Prestar(\"999\", 1, {fecha})", "KeyNotFoundException", hidden: true),
                Throws("Socio inexistente", $"Demo.Crear().Prestar(\"111\", 7, {fecha})", "KeyNotFoundException", hidden: true),
                Throws("Límite de préstamos", $"{{ var b = Demo.Crear(); b.Prestar(\"111\", 1, {fecha}); b.Prestar(\"222\", 1, {fecha}); b.Prestar(\"333\", 1, {fecha}); b.Prestar(\"444\", 1, {fecha}); return 0; }}", "InvalidOperationException", hidden: true, hint: "El cuarto préstamo del mismo socio debe rechazarse."),
                Is("Un rechazo no cambia nada", $"{{ var b = Demo.Crear(); b.Prestar(\"111\", 1, {fecha}); try {{ b.Prestar(\"111\", 2, {fecha}); }} catch (InvalidOperationException) {{ }} return b.Activos.Count; }}", "1", hidden: true, edge: true, hint: "Valida todo antes de modificar el estado."),
            ],
            "Biblioteca es la única puerta para cambiar el estado: Disponible y Prestamos tienen set interno, así que ningún código externo puede saltarse las reglas.",
            ["Busca el libro y el socio con TryGetValue y lanza KeyNotFoundException si faltan.", "Comprueba las dos reglas antes de modificar nada; después actualiza libro, socio y lista de activos."]);

        Hito(r, $"{p}.h2", "n3.records", CodeMode.Function,
            "Hito 2: devoluciones y multas. Implementa Devolver(isbn, fecha): si el libro no tiene un préstamo activo lanza InvalidOperationException. Si lo tiene, quita el préstamo de los activos, vuelve a poner el libro disponible, resta un préstamo al socio y devuelve la multa: 5 por cada día de retraso después de la fecha de vencimiento (0 si se devuelve a tiempo).",
            Join(BibliotecaModelo, Clase(BibliotecaCabecera, BibliotecaPrestar, """
                    public decimal Devolver(string isbn, DateOnly fecha)
                    {
                        return 0m;
                    }
                """), BibliotecaDemo),
            Join(BibliotecaModelo, Clase(BibliotecaCabecera, BibliotecaPrestar, BibliotecaDevolverFijo), BibliotecaDemo),
            [
                Is("A tiempo", $"{{ var b = Demo.Crear(); b.Prestar(\"111\", 1, {fecha}); return b.Devolver(\"111\", new DateOnly(2025, 3, 15)); }}", "0"),
                Is("Con retraso", $"{{ var b = Demo.Crear(); b.Prestar(\"111\", 1, {fecha}); return b.Devolver(\"111\", new DateOnly(2025, 3, 18)); }}", "15"),
                Throws("No estaba prestado", "Demo.Crear().Devolver(\"111\", new DateOnly(2025, 3, 1))", "InvalidOperationException", hidden: true),
                Is("Se puede volver a prestar", $"{{ var b = Demo.Crear(); b.Prestar(\"111\", 1, {fecha}); b.Devolver(\"111\", {fecha}); b.Prestar(\"111\", 2, {fecha}); return b.Activos.Count; }}", "1", hidden: true),
                Is("Libera cupo del socio", $"{{ var b = Demo.Crear(); b.Prestar(\"111\", 1, {fecha}); b.Prestar(\"222\", 1, {fecha}); b.Prestar(\"333\", 1, {fecha}); b.Devolver(\"222\", {fecha}); b.Prestar(\"444\", 1, {fecha}); return b.Activos.Count; }}", "3", hidden: true, edge: true, hint: "Al devolver, el socio debe recuperar un lugar en su límite."),
            ],
            "Prestamo es un record: se compara por valor, así que Remove encuentra el préstamo aunque sea otra referencia con los mismos datos.",
            ["Busca el préstamo activo con un foreach por Isbn.", "fecha.DayNumber - prestamo.Vence.DayNumber da los días de retraso (negativo si llegó antes)."]);

        Hito(r, $"{p}.h3", "n3.abstractas-interfaces", CodeMode.Function,
            "Hito 3: políticas de multa intercambiables. Implementa las clases de IPoliticaMulta: PoliticaEstandar (5 por día), PoliticaConTope(tope) (como la estándar pero nunca más que tope) y SinMulta (siempre 0). Haz que Devolver use la política recibida en el constructor de Biblioteca en lugar de la regla fija.",
            Join(BibliotecaModelo, """
                public interface IPoliticaMulta
                {
                    decimal Calcular(int diasRetraso);
                }

                public class PoliticaEstandar : IPoliticaMulta
                {
                    public decimal Calcular(int diasRetraso) => 0m;
                }

                public class PoliticaConTope(decimal tope) : IPoliticaMulta
                {
                    public decimal Calcular(int diasRetraso) => 0m;
                }

                public class SinMulta : IPoliticaMulta
                {
                    public decimal Calcular(int diasRetraso) => 0m;
                }
                """, Clase(BibliotecaCabeceraConPolitica, BibliotecaPrestar, BibliotecaDevolverFijo), BibliotecaDemo),
            Join(BibliotecaModelo, BibliotecaPoliticas, Clase(BibliotecaCabeceraConPolitica, BibliotecaPrestar, BibliotecaDevolverConPolitica), BibliotecaDemo),
            [
                Is("Estándar", "new PoliticaEstandar().Calcular(4)", "20"),
                Is("Con tope", "new PoliticaConTope(50m).Calcular(30)", "50"),
                Is("Biblioteca sin multa", $"{{ var b = Demo.Crear(new Biblioteca(new SinMulta())); b.Prestar(\"111\", 1, {fecha}); return b.Devolver(\"111\", new DateOnly(2025, 4, 30)); }}", "0", hidden: true, hint: "Devolver debe delegar el cálculo en la política recibida."),
                Is("Biblioteca con tope", $"{{ var b = Demo.Crear(new Biblioteca(new PoliticaConTope(40m))); b.Prestar(\"111\", 1, {fecha}); return b.Devolver(\"111\", new DateOnly(2025, 3, 20)); }}", "25", hidden: true),
                Is("A tiempo con tope", "new PoliticaConTope(40m).Calcular(0)", "0", hidden: true, edge: true),
            ],
            "Biblioteca ya no conoce las reglas de multa: depende de la interfaz. Agregar una política nueva (por ejemplo, descuento para docentes) no requiere modificarla.",
            ["Implementa las tres clases; PoliticaConTope puede reutilizar PoliticaEstandar y aplicar Math.Min.", "En Devolver reemplaza el cálculo fijo por politica.Calcular(Math.Max(0, retraso))."],
            [CodeConstruct.Interface]);

        Hito(r, $"{p}.h4", "n3.composicion", CodeMode.Function,
            "Hito 4: reporte de vencidos. Implementa Biblioteca.Vencidos(hoy) que devuelva una línea por cada préstamo vencido (hoy posterior a la fecha de vencimiento) con el formato «Título — Socio (N días)», ordenada de más a menos días de retraso y, en empate, por título.",
            Join(BibliotecaModelo, BibliotecaPoliticas, Clase(BibliotecaCabeceraConPolitica, BibliotecaPrestar, BibliotecaDevolverConPolitica, """
                    public List<string> Vencidos(DateOnly hoy)
                    {
                        return [];
                    }
                """), BibliotecaDemo),
            Join(BibliotecaModelo, BibliotecaPoliticas, Clase(BibliotecaCabeceraConPolitica, BibliotecaPrestar, BibliotecaDevolverConPolitica, BibliotecaVencidos), BibliotecaDemo),
            [
                Is("Dos vencidos", "{ var b = Demo.Crear(); b.Prestar(\"111\", 1, new DateOnly(2025, 3, 1)); b.Prestar(\"222\", 2, new DateOnly(2025, 3, 5)); b.Prestar(\"333\", 2, new DateOnly(2025, 3, 20)); return b.Vencidos(new DateOnly(2025, 3, 25)); }", "[\"Rayuela — Ana (10 días)\", \"Ficciones — Luis (6 días)\"]"),
                Is("Ninguno", "{ var b = Demo.Crear(); b.Prestar(\"111\", 1, new DateOnly(2025, 3, 1)); return b.Vencidos(new DateOnly(2025, 3, 15)).Count; }", "0", edge: true),
                Is("Empate por título", "{ var b = Demo.Crear(); b.Prestar(\"444\", 1, new DateOnly(2025, 3, 1)); b.Prestar(\"333\", 2, new DateOnly(2025, 3, 1)); return b.Vencidos(new DateOnly(2025, 3, 17)); }", "[\"Aura — Luis (2 días)\", \"Pedro Páramo — Ana (2 días)\"]", hidden: true, edge: true),
                Is("Devueltos no cuentan", "{ var b = Demo.Crear(); b.Prestar(\"111\", 1, new DateOnly(2025, 3, 1)); b.Devolver(\"111\", new DateOnly(2025, 3, 20)); return b.Vencidos(new DateOnly(2025, 4, 1)).Count; }", "0", hidden: true),
            ],
            "El reporte combina datos de tres colecciones (préstamos, libros y socios) que la Biblioteca compone; nadie fuera de ella necesita conocer su organización interna.",
            ["Recorre los préstamos activos y calcula los días de retraso con DayNumber.", "Guarda tuplas (días, título, línea), ordénalas con un método Comparar y devuelve solo las líneas."]);

        return new IntegrativeProject(p, "Biblioteca con préstamos y multas",
            "Un sistema de préstamos con reglas de negocio protegidas por encapsulación, multas configurables mediante una interfaz y un reporte de vencidos.",
            ["n3.encapsulacion", "n3.composicion", "n3.records", "n3.abstractas-interfaces", "n2.diccionarios"],
            [
                "Prestar libros respetando disponibilidad y un límite de préstamos por socio.",
                "Registrar devoluciones y calcular multas por retraso.",
                "Permitir cambiar la política de multas sin modificar la clase Biblioteca.",
                "Listar los préstamos vencidos ordenados por días de retraso.",
            ],
            [
                "Todas las pruebas de los cuatro hitos pasan.",
                "Ninguna operación rechazada deja el sistema en un estado inconsistente.",
                "El estado de libros y socios solo se modifica a través de Biblioteca.",
                "Agregar una política de multa nueva solo requiere una clase nueva.",
            ],
            [
                new("Préstamos", "Reglas de préstamo y validación de libros y socios.", $"{p}.h1"),
                new("Devoluciones y multas", "Devolver libros y calcular la multa por retraso.", $"{p}.h2"),
                new("Políticas de multa", "Interfaz con varias implementaciones inyectadas en la biblioteca.", $"{p}.h3"),
                new("Reporte de vencidos", "Listado ordenado de préstamos vencidos.", $"{p}.h4"),
            ],
            [
                new("Corrección", "Algunas reglas no se cumplen o hay pruebas que fallan.", "Todas las pruebas pasan.", "Además, consideraste casos como devolver el mismo día o un socio sin préstamos."),
                new("Encapsulación", "El estado se puede modificar desde fuera de Biblioteca.", "Solo Biblioteca cambia el estado y valida antes de hacerlo.", "Las colecciones internas nunca se exponen como modificables."),
                new("Diseño con interfaces", "Las multas dependen de condicionales por tipo.", "Las multas se calculan a través de IPoliticaMulta.", "Puedes explicar cómo probar cada política por separado y por qué Biblioteca no cambió al agregarlas."),
                new("Legibilidad", "Métodos largos o nombres poco claros.", "Métodos cortos con nombres descriptivos.", "El código se lee como la descripción del negocio."),
            ]);
    }

    // ───────────────────────── Proyecto 3: tarjetas de repaso espaciado ─────────────────────────

    private const string TarjetaModelo = """
        public record Tarjeta(string Frente, string Reverso)
        {
            public int Repeticiones { get; init; }
            public double Facilidad { get; init; } = 2.5;
            public int IntervaloDias { get; init; }
            public DateOnly Proxima { get; init; }

            public Tarjeta Calificar(int calidad, DateOnly hoy)
            {
                if (calidad < 0 || calidad > 5)
                {
                    throw new ArgumentOutOfRangeException(nameof(calidad));
                }

                int q = 5 - calidad;
                double facilidad = Math.Round(Math.Max(1.3, Facilidad + 0.1 - q * (0.08 + q * 0.02)), 2);
                if (calidad < 3)
                {
                    return this with { Repeticiones = 0, IntervaloDias = 1, Facilidad = facilidad, Proxima = hoy.AddDays(1) };
                }

                int intervalo = Repeticiones switch
                {
                    0 => 1,
                    1 => 6,
                    _ => (int)Math.Round(IntervaloDias * Facilidad),
                };
                return this with { Repeticiones = Repeticiones + 1, IntervaloDias = intervalo, Facilidad = facilidad, Proxima = hoy.AddDays(intervalo) };
            }
        }
        """;

    private const string TarjetaMazo = """
        public class Mazo
        {
            private readonly List<Tarjeta> _tarjetas = [];

            public int Total => _tarjetas.Count;

            public void Agregar(Tarjeta tarjeta)
            {
                foreach (Tarjeta t in _tarjetas)
                {
                    if (t.Frente == tarjeta.Frente)
                    {
                        throw new InvalidOperationException($"Ya existe la tarjeta «{tarjeta.Frente}»");
                    }
                }
                _tarjetas.Add(tarjeta);
            }

            public List<Tarjeta> Pendientes(DateOnly hoy)
            {
                List<Tarjeta> pendientes = [];
                foreach (Tarjeta t in _tarjetas)
                {
                    if (t.Proxima <= hoy) pendientes.Add(t);
                }
                pendientes.Sort(Comparar);
                return pendientes;
            }

            public Tarjeta Responder(string frente, int calidad, DateOnly hoy)
            {
                for (int i = 0; i < _tarjetas.Count; i++)
                {
                    if (_tarjetas[i].Frente == frente)
                    {
                        _tarjetas[i] = _tarjetas[i].Calificar(calidad, hoy);
                        return _tarjetas[i];
                    }
                }
                throw new KeyNotFoundException($"No existe la tarjeta «{frente}»");
            }

            private static int Comparar(Tarjeta a, Tarjeta b)
            {
                int porFecha = a.Proxima.CompareTo(b.Proxima);
                return porFecha != 0 ? porFecha : string.CompareOrdinal(a.Frente, b.Frente);
            }
        }
        """;

    private const string TarjetaReloj = """
        public interface IReloj
        {
            DateOnly Hoy { get; }
        }

        public class RelojFijo(DateOnly hoy) : IReloj
        {
            public DateOnly Hoy => hoy;
        }
        """;

    private const string TarjetaSesion = """
        public class SesionRepaso(Mazo mazo, IReloj reloj)
        {
            private readonly Queue<Tarjeta> _cola = new(mazo.Pendientes(reloj.Hoy));

            public int Repasadas { get; private set; }
            public int Restantes => _cola.Count;
            public Tarjeta? Actual => _cola.Count > 0 ? _cola.Peek() : null;

            public void Responder(int calidad)
            {
                if (_cola.Count == 0)
                {
                    throw new InvalidOperationException("No hay tarjetas pendientes");
                }

                Tarjeta tarjeta = _cola.Peek();
                Tarjeta nueva = mazo.Responder(tarjeta.Frente, calidad, reloj.Hoy);
                _cola.Dequeue();
                Repasadas++;
                if (calidad < 3)
                {
                    _cola.Enqueue(nueva);
                }
            }
        }
        """;

    private const string TarjetaPrograma = """
        Mazo mazo = new();
        string? linea = Console.ReadLine();
        while (!string.IsNullOrEmpty(linea))
        {
            string[] partes = linea.Split('=');
            mazo.Agregar(new Tarjeta(partes[0].Trim(), partes[1].Trim()));
            linea = Console.ReadLine();
        }

        SesionRepaso sesion = new(mazo, new RelojFijo(new DateOnly(2025, 1, 1)));
        bool terminoEntrada = false;
        while (sesion.Actual is Tarjeta actual && !terminoEntrada)
        {
            Console.WriteLine($"¿{actual.Frente}?");
            int calidad;
            while (true)
            {
                string? respuesta = Console.ReadLine();
                if (respuesta is null)
                {
                    terminoEntrada = true;
                    break;
                }
                if (int.TryParse(respuesta, out calidad) && calidad >= 0 && calidad <= 5)
                {
                    sesion.Responder(calidad);
                    break;
                }
                Console.WriteLine("Calificación inválida");
            }
        }

        Console.WriteLine($"Repasadas: {sesion.Repasadas}");
        Console.WriteLine($"Pendientes: {sesion.Restantes}");
        """;

    private static IntegrativeProject Tarjetas(ContentRegistry r)
    {
        const string p = "proyecto.tarjetas";
        const string hoy = "new DateOnly(2025, 1, 1)";

        Hito(r, $"{p}.h1", "n3.records", CodeMode.Function,
            "Hito 1: calificar una tarjeta. Implementa Tarjeta.Calificar(calidad, hoy) con esta variante del algoritmo SM-2 y devuelve una tarjeta NUEVA (with): calidad debe estar entre 0 y 5 (si no, ArgumentOutOfRangeException). Con q = 5 − calidad, la nueva facilidad es max(1.3, Facilidad + 0.1 − q·(0.08 + q·0.02)) redondeada a 2 decimales. Si calidad < 3: Repeticiones = 0 e IntervaloDias = 1. Si no: el intervalo es 1 en la primera repetición, 6 en la segunda y después round(IntervaloDias × Facilidad anterior); Repeticiones aumenta en 1. Proxima = hoy + intervalo.",
            """
            public record Tarjeta(string Frente, string Reverso)
            {
                public int Repeticiones { get; init; }
                public double Facilidad { get; init; } = 2.5;
                public int IntervaloDias { get; init; }
                public DateOnly Proxima { get; init; }

                public Tarjeta Calificar(int calidad, DateOnly hoy)
                {
                    return this;
                }
            }
            """,
            TarjetaModelo,
            [
                Is("Primera respuesta perfecta", $"{{ var t = new Tarjeta(\"hola\", \"hello\").Calificar(5, {hoy}); return $\"{{t.Repeticiones}} {{t.IntervaloDias}} {{t.Facilidad}} {{t.Proxima:yyyy-MM-dd}}\"; }}", "1 1 2.6 2025-01-02"),
                Is("Tres repasos", "{ var t = new Tarjeta(\"a\", \"b\").Calificar(5, new DateOnly(2025, 1, 1)).Calificar(4, new DateOnly(2025, 1, 2)).Calificar(5, new DateOnly(2025, 1, 8)); return $\"{t.Repeticiones} {t.IntervaloDias} {t.Facilidad} {t.Proxima:yyyy-MM-dd}\"; }", "3 16 2.7 2025-01-24"),
                Is("Olvido", $"{{ var t = new Tarjeta(\"a\", \"b\").Calificar(5, {hoy}).Calificar(1, {hoy}); return $\"{{t.Repeticiones}} {{t.IntervaloDias}} {{t.Facilidad}}\"; }}", "0 1 2.06", hidden: true),
                Is("Facilidad mínima", $"{{ var t = new Tarjeta(\"a\", \"b\").Calificar(0, {hoy}).Calificar(0, {hoy}).Calificar(0, {hoy}); return t.Facilidad; }}", "1.3", hidden: true, edge: true, hint: "La facilidad nunca baja de 1.3."),
                Throws("Calidad inválida", $"new Tarjeta(\"a\", \"b\").Calificar(6, {hoy})", "ArgumentOutOfRangeException", hidden: true),
                Is("Inmutable", $"{{ var t = new Tarjeta(\"a\", \"b\"); t.Calificar(5, {hoy}); return t.Repeticiones; }}", "0", hidden: true, hint: "Calificar devuelve una tarjeta nueva; la original no cambia."),
            ],
            "Al ser un record inmutable, cada repaso produce una versión nueva de la tarjeta: es fácil probar el algoritmo y no hay estados intermedios.",
            ["Calcula primero la nueva facilidad y después decide el intervalo según Repeticiones (un switch es claro).", "Usa la Facilidad anterior para el intervalo y devuelve this with { ... }."],
            [CodeConstruct.Record]);

        Hito(r, $"{p}.h2", "n2.listas", CodeMode.Function,
            "Hito 2: el mazo. Implementa Mazo con Agregar (InvalidOperationException si ya hay una tarjeta con el mismo Frente), Pendientes(hoy) (las tarjetas con Proxima <= hoy, ordenadas por Proxima y luego por Frente) y Responder(frente, calidad, hoy) (reemplaza la tarjeta por su versión calificada y la devuelve; KeyNotFoundException si no existe). Las tarjetas nuevas tienen Proxima = DateOnly.MinValue, así que están pendientes desde el inicio.",
            Join(TarjetaModelo, """
                public class Mazo
                {
                    public int Total => 0;

                    public void Agregar(Tarjeta tarjeta)
                    {
                    }

                    public List<Tarjeta> Pendientes(DateOnly hoy) => [];

                    public Tarjeta Responder(string frente, int calidad, DateOnly hoy) => throw new NotImplementedException();
                }
                """),
            Join(TarjetaModelo, TarjetaMazo),
            [
                Is("Pendientes ordenadas", $"{{ var m = new Mazo(); m.Agregar(new Tarjeta(\"sol\", \"sun\")); m.Agregar(new Tarjeta(\"luna\", \"moon\")); return string.Join(\",\", m.Pendientes({hoy}).Select(t => t.Frente)); }}", "luna,sol"),
                Is("Responder saca de pendientes", $"{{ var m = new Mazo(); m.Agregar(new Tarjeta(\"sol\", \"sun\")); m.Responder(\"sol\", 5, {hoy}); return m.Pendientes({hoy}).Count; }}", "0"),
                Is("Vuelve a estar pendiente", $"{{ var m = new Mazo(); m.Agregar(new Tarjeta(\"sol\", \"sun\")); m.Responder(\"sol\", 5, {hoy}); return m.Pendientes(new DateOnly(2025, 1, 2)).Count; }}", "1", hidden: true),
                Throws("Duplicada", "{ var m = new Mazo(); m.Agregar(new Tarjeta(\"sol\", \"sun\")); m.Agregar(new Tarjeta(\"sol\", \"sol\")); return 0; }", "InvalidOperationException", hidden: true),
                Throws("No existe", $"new Mazo().Responder(\"x\", 5, {hoy})", "KeyNotFoundException", hidden: true),
                Is("Orden por fecha", "{ var m = new Mazo(); m.Agregar(new Tarjeta(\"b\", \"1\")); m.Agregar(new Tarjeta(\"a\", \"2\")); m.Responder(\"a\", 5, new DateOnly(2025, 1, 1)); return string.Join(\",\", m.Pendientes(new DateOnly(2025, 1, 5)).Select(t => t.Frente)); }", "b,a", hidden: true, edge: true, hint: "Primero las de fecha más antigua; «b» nunca se repasó (fecha mínima)."),
            ],
            "Como las tarjetas son inmutables, «actualizar» una tarjeta es reemplazarla en la lista por su versión nueva.",
            ["Guarda las tarjetas en una List<Tarjeta> privada.", "Para reemplazar recorre con for: _tarjetas[i] = _tarjetas[i].Calificar(calidad, hoy);"]);

        Hito(r, $"{p}.h3", "n3.solid", CodeMode.Function,
            "Hito 3: una sesión de repaso con la fecha inyectada. Implementa SesionRepaso(Mazo mazo, IReloj reloj): al crearse toma las tarjetas pendientes de hoy en una cola. Actual es la tarjeta al frente de la cola (o null). Responder(calidad) califica la tarjeta actual en el mazo, la quita de la cola, suma 1 a Repasadas y, si calidad < 3, la vuelve a poner al final para preguntarla otra vez en la misma sesión. Si no quedan tarjetas, lanza InvalidOperationException.",
            Join(TarjetaModelo, TarjetaMazo, TarjetaReloj, """
                public class SesionRepaso(Mazo mazo, IReloj reloj)
                {
                    public int Repasadas { get; private set; }
                    public int Restantes => 0;
                    public Tarjeta? Actual => null;

                    public void Responder(int calidad)
                    {
                    }
                }
                """),
            Join(TarjetaModelo, TarjetaMazo, TarjetaReloj, TarjetaSesion),
            [
                Is("Recorre las pendientes", $"{{ var m = new Mazo(); m.Agregar(new Tarjeta(\"sol\", \"sun\")); m.Agregar(new Tarjeta(\"luna\", \"moon\")); var s = new SesionRepaso(m, new RelojFijo({hoy})); var primera = s.Actual!.Frente; s.Responder(5); s.Responder(4); return $\"{{primera}} {{s.Repasadas}} {{s.Restantes}} {{s.Actual is null}}\"; }}", "luna 2 0 True"),
                Is("Olvidada vuelve al final", $"{{ var m = new Mazo(); m.Agregar(new Tarjeta(\"sol\", \"sun\")); m.Agregar(new Tarjeta(\"luna\", \"moon\")); var s = new SesionRepaso(m, new RelojFijo({hoy})); s.Responder(1); return $\"{{s.Actual!.Frente}} {{s.Restantes}}\"; }}", "sol 2"),
                Throws("Sin pendientes", $"{{ var s = new SesionRepaso(new Mazo(), new RelojFijo({hoy})); s.Responder(5); return 0; }}", "InvalidOperationException", hidden: true),
                Is("Usa el reloj", "{ var m = new Mazo(); m.Agregar(new Tarjeta(\"sol\", \"sun\")); m.Responder(\"sol\", 5, new DateOnly(2025, 1, 1)); return new SesionRepaso(m, new RelojFijo(new DateOnly(2025, 1, 1))).Restantes + new SesionRepaso(m, new RelojFijo(new DateOnly(2025, 1, 2))).Restantes * 10; }", "10", hidden: true, edge: true, hint: "Las pendientes dependen de reloj.Hoy, no de la fecha real."),
            ],
            "Inyectar IReloj hace que la sesión sea determinista: las pruebas eligen el día, y la aplicación real usaría un reloj del sistema.",
            ["Inicializa una Queue<Tarjeta> con mazo.Pendientes(reloj.Hoy) en un campo.", "Responder: Peek, mazo.Responder, Dequeue, Repasadas++ y, si calidad < 3, Enqueue de la versión nueva."],
            [CodeConstruct.Interface]);

        Hito(r, $"{p}.h4", "n1.validacion", CodeMode.Program,
            "Hito 4: el programa de repaso. Lee tarjetas «frente=reverso» hasta una línea vacía. Después crea una SesionRepaso con RelojFijo(new DateOnly(2025, 1, 1)) y, mientras haya una tarjeta actual, muestra «¿frente?» y lee una calificación de 0 a 5; si no es válida muestra «Calificación inválida» y vuelve a leer. Si la entrada se termina, deja de preguntar. Al final muestra «Repasadas: N» y «Pendientes: R».",
            "// Escribe aquí el programa. Las clases de los hitos anteriores ya están abajo.\n\n" + Join(TarjetaModelo, TarjetaMazo, TarjetaReloj, TarjetaSesion),
            Join(TarjetaPrograma, TarjetaModelo, TarjetaMazo, TarjetaReloj, TarjetaSesion),
            [
                In("Sesión completa", "sol=sun\nluna=moon\n\n5\nx\n2\n4", "¿luna?\n¿sol?\nCalificación inválida\n¿sol?\nRepasadas: 3\nPendientes: 0"),
                In("Fuera de rango", "uno=one\n\n9\n-1\n3", "¿uno?\nCalificación inválida\nCalificación inválida\nRepasadas: 1\nPendientes: 0"),
                In("Entrada incompleta", "a=1\nb=2\n\n4", "¿a?\n¿b?\nRepasadas: 1\nPendientes: 1", hidden: true, edge: true, hint: "Cuando ReadLine devuelve null, termina la sesión y muestra el resumen."),
                In("Sin tarjetas", "\n", "Repasadas: 0\nPendientes: 0", hidden: true, edge: true),
            ],
            "El programa principal solo coordina entrada y salida; todas las reglas viven en las clases probadas en los hitos anteriores.",
            ["Primero un while que agregue tarjetas hasta la línea vacía; después un while sobre sesion.Actual.", "Dentro, un bucle que lea hasta obtener un número válido de 0 a 5 o hasta que ReadLine devuelva null."],
            [CodeConstruct.Loop]);

        return new IntegrativeProject(p, "Tarjetas de repaso espaciado",
            "Una versión reducida del motor de repaso de esta aplicación: tarjetas inmutables calificadas con una variante de SM-2, un mazo con pendientes por fecha y una sesión de repaso en consola.",
            ["n3.records", "n2.listas", "n3.solid", "n1.validacion", "n2.metodos"],
            [
                "Calcular el siguiente repaso de una tarjeta según la calidad de la respuesta.",
                "Mantener un mazo y obtener las tarjetas pendientes para una fecha.",
                "Conducir una sesión en la que las tarjetas olvidadas se vuelven a preguntar.",
                "Ofrecer un programa de consola que valide las calificaciones.",
            ],
            [
                "Todas las pruebas de los cuatro hitos pasan.",
                "Las tarjetas nunca se modifican: cada repaso crea una versión nueva.",
                "La sesión no depende de la fecha real del sistema.",
                "Una calificación inválida nunca detiene el programa.",
            ],
            [
                new("Algoritmo de repaso", "Variante de SM-2 en un record inmutable.", $"{p}.h1"),
                new("Mazo", "Colección de tarjetas con pendientes ordenadas.", $"{p}.h2"),
                new("Sesión de repaso", "Cola de repaso con el reloj inyectado.", $"{p}.h3"),
                new("Programa de consola", "Interfaz de texto con validación de la entrada.", $"{p}.h4"),
            ],
            [
                new("Corrección del algoritmo", "Los intervalos o la facilidad no coinciden con la especificación.", "Todas las pruebas pasan.", "Puedes explicar con un ejemplo por qué una respuesta con calidad 3 hace bajar la facilidad."),
                new("Inmutabilidad y modelado", "Se modifican tarjetas existentes.", "Cada repaso produce una tarjeta nueva con with.", "El diseño hace imposible tener una tarjeta en un estado inválido."),
                new("Dependencias", "La sesión usa la fecha del sistema directamente.", "La fecha llega a través de IReloj.", "Podrías reemplazar el mazo en memoria por uno guardado en archivo sin cambiar la sesión."),
                new("Experiencia de uso", "Entradas inválidas producen errores o bucles infinitos.", "Las entradas inválidas se rechazan con un mensaje.", "Los mensajes indican qué valores son válidos y el resumen final es claro."),
            ]);
    }
}
