using CSharpMastery.Content.Infrastructure;
using CSharpMastery.Domain.Curriculum;
using CSharpMastery.Domain.Items;
using CSharpMastery.Domain.Progress;
using static CSharpMastery.Content.Infrastructure.ContentRegistry;
using static CSharpMastery.Content.Infrastructure.L;

namespace CSharpMastery.Content.Nivel3;

internal static class U02Relaciones
{
    public const string UnitId = "n3.u2";

    public static Unit Build(ContentRegistry r) => new(UnitId, "n3", "Relaciones entre tipos",
        "Composición, herencia, polimorfismo, clases abstractas e interfaces.",
        [Composicion(r), Herencia(r), Polimorfismo(r), AbstractasInterfaces(r)]);

    private static Skill Composicion(ContentRegistry r)
    {
        const string s = "n3.composicion";
        r.Short("n3.composicion.pred1", s, ItemPurpose.Prediction,
            "¿Qué muestra este programa?",
            "El Auto se compone de un Motor. Arrancar delega en el motor: el auto no sabe cómo funciona un motor, solo le pide que encienda.",
            ["Motor de 150 HP encendido\nAuto listo"], match: AnswerMatch.Output,
            code: """
                Auto a = new Auto(new Motor(150));
                a.Arrancar();

                class Motor(int caballos)
                {
                    public void Encender() => Console.WriteLine($"Motor de {caballos} HP encendido");
                }

                class Auto(Motor motor)
                {
                    public void Arrancar()
                    {
                        motor.Encender();
                        Console.WriteLine("Auto listo");
                    }
                }
                """);

        r.Choice("n3.composicion.concepto1", s, ItemPurpose.Concept,
            "¿Qué relación describe mejor la composición?",
            "Composición es «tiene un»: un Pedido tiene líneas, un Auto tiene un motor. La herencia es «es un»: un Perro es un Animal.",
            [
                Ok("«Tiene un»: un objeto contiene a otros y les delega trabajo.", "Correcto."),
                No("«Es un»: una clase especializa a otra.", "Eso describe la herencia."),
                No("Dos clases con el mismo nombre en distintos archivos.", "No tiene relación."),
            ],
            dimension: KnowledgeDimension.Design);

        r.Choice("n3.composicion.recall1", s, ItemPurpose.Recall,
            "¿Qué significa «delegar» en un diseño por composición?",
            "Que un objeto resuelve parte de su trabajo pidiéndoselo a otro objeto que contiene, en lugar de hacerlo él mismo.",
            [
                Ok("Pedirle a un objeto contenido que haga parte del trabajo.", "Correcto."),
                No("Copiar el código de otra clase.", "Delegar evita justamente copiar."),
                No("Heredar los métodos de otra clase.", "Eso es herencia."),
            ]);

        r.Choice("n3.composicion.recall2", s, ItemPurpose.Recall,
            "En «class Pedido(Cliente cliente)», ¿qué es cliente?",
            "Es un parámetro del constructor primario (C# 12): está disponible en toda la clase sin declarar un campo a mano.",
            [
                Ok("Un parámetro del constructor primario, disponible en toda la clase.", "Correcto."),
                No("Una clase base de Pedido.", "La herencia se escribe con dos puntos: class Pedido : Base."),
                No("Una propiedad pública de Pedido.", "No es pública a menos que declares una propiedad que la exponga."),
            ]);

        r.Choice("n3.composicion.bug1", s, ItemPurpose.BugFinding,
            "Este Carrito calcula mal el total si alguien agrega un producto y después cambia su precio. ¿Cuál es el problema de diseño?",
            "El carrito guarda el total ya calculado (un dato duplicado) en lugar de calcularlo a partir de sus líneas. Si las líneas cambian, el total queda desactualizado. Mejor una propiedad calculada que delegue en las líneas.",
            [
                Ok("Guarda un total duplicado en vez de calcularlo a partir de las líneas que contiene.", "Correcto."),
                No("Debería heredar de List<Linea>.", "Heredar de List expone operaciones que romperían el carrito.", ErrorCategory.ConceptMisunderstanding),
                No("Total debería ser static.", "Cada carrito tiene su propio total.", ErrorCategory.StaticVersusInstance),
            ],
            code: """
                class Linea
                {
                    public decimal Precio { get; set; }
                    public int Cantidad { get; set; }
                }

                class Carrito
                {
                    private readonly List<Linea> _lineas = [];
                    public decimal Total { get; private set; }

                    public void Agregar(Linea l)
                    {
                        _lineas.Add(l);
                        Total += l.Precio * l.Cantidad;
                    }
                }
                """,
            targets: [ErrorCategory.WrongLogic]);

        r.Code("n3.composicion.guiado", s, ItemPurpose.Guided, CodeMode.Function,
            "Completa Pedido: contiene una lista privada de Linea. Agregar(producto, precio, cantidad) crea la línea; Total suma los subtotales de las líneas (delegando en Linea.Subtotal).",
            """
            public class Linea(string producto, decimal precio, int cantidad)
            {
                public string Producto => producto;
                public decimal Subtotal => precio * cantidad;
            }

            public class Pedido
            {
                private readonly List<Linea> _lineas = [];

                public void Agregar(string producto, decimal precio, int cantidad)
                {
                }

                public decimal Total => 0;
            }
            """,
            """
            public class Linea(string producto, decimal precio, int cantidad)
            {
                public string Producto => producto;
                public decimal Subtotal => precio * cantidad;
            }

            public class Pedido
            {
                private readonly List<Linea> _lineas = [];

                public void Agregar(string producto, decimal precio, int cantidad) =>
                    _lineas.Add(new Linea(producto, precio, cantidad));

                public decimal Total
                {
                    get
                    {
                        decimal total = 0;
                        foreach (Linea l in _lineas)
                        {
                            total += l.Subtotal;
                        }
                        return total;
                    }
                }
            }
            """,
            [
                Is("Dos líneas", "{ var p = new Pedido(); p.Agregar(\"café\", 50m, 2); p.Agregar(\"pan\", 10m, 3); return p.Total; }", "130"),
                Is("Vacío", "new Pedido().Total", "0", edge: true),
            ],
            "El pedido no sabe calcular un subtotal: se lo pide a cada línea.",
            ["En Agregar: _lineas.Add(new Linea(producto, precio, cantidad));", "Total recorre las líneas sumando l.Subtotal."],
            requires: [CodeConstruct.Class, CodeConstruct.Loop]);

        r.Code("n3.composicion.indep", s, ItemPurpose.Independent, CodeMode.Function,
            "Modela una Playlist que contiene canciones. Cancion(string titulo, int segundos). Playlist(string nombre) con Agregar(Cancion), DuracionTotal (string «m:ss»), y MasLarga (la canción más larga, o null si está vacía).",
            """
            public class Cancion(string titulo, int segundos)
            {
                public string Titulo => titulo;
                public int Segundos => segundos;
            }

            public class Playlist
            {
            }
            """,
            """
            public class Cancion(string titulo, int segundos)
            {
                public string Titulo => titulo;
                public int Segundos => segundos;
            }

            public class Playlist(string nombre)
            {
                private readonly List<Cancion> _canciones = [];

                public string Nombre => nombre;

                public void Agregar(Cancion c) => _canciones.Add(c);

                public string DuracionTotal
                {
                    get
                    {
                        int total = 0;
                        foreach (Cancion c in _canciones) total += c.Segundos;
                        return $"{total / 60}:{total % 60:D2}";
                    }
                }

                public Cancion? MasLarga
                {
                    get
                    {
                        Cancion? mayor = null;
                        foreach (Cancion c in _canciones)
                        {
                            if (mayor is null || c.Segundos > mayor.Segundos) mayor = c;
                        }
                        return mayor;
                    }
                }
            }
            """,
            [
                Is("Duración", "{ var p = new Playlist(\"Viaje\"); p.Agregar(new Cancion(\"A\", 200)); p.Agregar(new Cancion(\"B\", 185)); return p.DuracionTotal; }", "6:25"),
                Is("Más larga", "{ var p = new Playlist(\"Viaje\"); p.Agregar(new Cancion(\"A\", 200)); p.Agregar(new Cancion(\"B\", 260)); return p.MasLarga!.Titulo; }", "B"),
                Is("Vacía", "{ var p = new Playlist(\"Nada\"); return $\"{p.DuracionTotal} {p.MasLarga is null}\"; }", "0:00 True", hidden: true, edge: true, hint: "Sin canciones, MasLarga debe ser null y la duración 0:00."),
            ],
            "La playlist reúne canciones y calcula a partir de ellas; no duplica sus datos.",
            ["Usa un constructor primario Playlist(string nombre) y una lista privada.", "Para MasLarga, empieza con null y quédate con la de más segundos."],
            difficulty: 2, requires: [CodeConstruct.Class], related: ["n3.encapsulacion"]);

        r.Code("n3.composicion.app", s, ItemPurpose.Transfer, CodeMode.Function,
            "Aplicación: una biblioteca. Libro(string isbn, string titulo) con Prestado (bool, set privado) y métodos Prestar()/Devolver() que lanzan InvalidOperationException si el estado no lo permite. Biblioteca con Agregar(Libro), Prestar(string isbn) y Devolver(string isbn) (KeyNotFoundException si el ISBN no existe) y Disponibles (cantidad de libros no prestados).",
            """
            public class Libro(string isbn, string titulo)
            {
                public string Isbn => isbn;
                public string Titulo => titulo;
            }

            public class Biblioteca
            {
            }
            """,
            """
            public class Libro(string isbn, string titulo)
            {
                public string Isbn => isbn;
                public string Titulo => titulo;
                public bool Prestado { get; private set; }

                public void Prestar()
                {
                    if (Prestado) throw new InvalidOperationException($"{titulo} ya está prestado");
                    Prestado = true;
                }

                public void Devolver()
                {
                    if (!Prestado) throw new InvalidOperationException($"{titulo} no estaba prestado");
                    Prestado = false;
                }
            }

            public class Biblioteca
            {
                private readonly Dictionary<string, Libro> _libros = [];

                public void Agregar(Libro libro) => _libros.Add(libro.Isbn, libro);

                public void Prestar(string isbn) => Buscar(isbn).Prestar();

                public void Devolver(string isbn) => Buscar(isbn).Devolver();

                public int Disponibles
                {
                    get
                    {
                        int n = 0;
                        foreach (Libro l in _libros.Values)
                        {
                            if (!l.Prestado) n++;
                        }
                        return n;
                    }
                }

                private Libro Buscar(string isbn) =>
                    _libros.TryGetValue(isbn, out Libro? libro) ? libro : throw new KeyNotFoundException($"No existe el ISBN {isbn}");
            }
            """,
            [
                Is("Prestar", "{ var b = new Biblioteca(); b.Agregar(new Libro(\"1\", \"A\")); b.Agregar(new Libro(\"2\", \"B\")); b.Prestar(\"1\"); return b.Disponibles; }", "1"),
                Is("Devolver", "{ var b = new Biblioteca(); b.Agregar(new Libro(\"1\", \"A\")); b.Prestar(\"1\"); b.Devolver(\"1\"); return b.Disponibles; }", "1"),
                Throws("Doble préstamo", "{ var b = new Biblioteca(); b.Agregar(new Libro(\"1\", \"A\")); b.Prestar(\"1\"); b.Prestar(\"1\"); return 0; }", "InvalidOperationException", hidden: true),
                Throws("ISBN inexistente", "{ var b = new Biblioteca(); b.Prestar(\"9\"); return 0; }", "KeyNotFoundException", hidden: true),
                Throws("Devolver sin préstamo", "{ var b = new Biblioteca(); b.Agregar(new Libro(\"1\", \"A\")); b.Devolver(\"1\"); return 0; }", "InvalidOperationException", hidden: true, hint: "La regla del préstamo es del libro; la biblioteca solo lo busca y delega."),
            ],
            "Cada objeto cuida sus propias reglas: el libro decide si se puede prestar; la biblioteca solo lo encuentra y le delega la operación.",
            ["Las reglas de préstamo van en Libro; Biblioteca guarda un Dictionary<string, Libro> por ISBN.", "Un método privado Buscar evita repetir la búsqueda y el error."],
            difficulty: 3, requires: [CodeConstruct.Class], related: ["n2.diccionarios", "n3.encapsulacion"]);

        return new Skill(s, UnitId, "Composición",
            "Construir objetos a partir de otros objetos.",
            Tier.Essential, ["n3.encapsulacion"],
            new Lesson(s,
                "Construir objetos que contienen a otros, delegarles responsabilidades y evitar datos duplicados.",
                [
                    P("La composición consiste en construir un objeto a partir de otros: un Pedido contiene líneas, un Auto contiene un motor. Es la relación «tiene un»."),
                    P("El objeto contenedor delega trabajo en sus partes. Cada clase se ocupa de lo suyo, lo que hace el código más fácil de cambiar y probar."),
                    Code("""
                        Direccion d = new Direccion("Reforma 10", "CDMX");
                        Cliente c = new Cliente("Ana", d);
                        Console.WriteLine(c.Etiqueta());

                        class Direccion(string calle, string ciudad)
                        {
                            public string Formato() => $"{calle}, {ciudad}";
                        }

                        class Cliente(string nombre, Direccion direccion)
                        {
                            public string Etiqueta() => $"{nombre} — {direccion.Formato()}";
                        }
                        """, "Un cliente tiene una dirección"),
                    P("class Cliente(string nombre, Direccion direccion) usa un constructor primario (C# 12): los parámetros quedan disponibles en toda la clase."),
                    Design("«Prefiere composición sobre herencia» es una recomendación clásica de diseño: combinar objetos suele ser más flexible que heredar de ellos."),
                ],
                new WorkedExample("Un reloj compuesto por contadores",
                    """
                    Reloj r = new Reloj();
                    for (int i = 0; i < 125; i++)
                    {
                        r.Tic();
                    }
                    Console.WriteLine(r);

                    class ContadorCiclico(int limite)
                    {
                        public int Valor { get; private set; }

                        public bool Avanzar()
                        {
                            Valor = (Valor + 1) % limite;
                            return Valor == 0;
                        }
                    }

                    class Reloj
                    {
                        private readonly ContadorCiclico _segundos = new(60);
                        private readonly ContadorCiclico _minutos = new(60);

                        public void Tic()
                        {
                            if (_segundos.Avanzar())
                            {
                                _minutos.Avanzar();
                            }
                        }

                        public override string ToString() => $"{_minutos.Valor:D2}:{_segundos.Valor:D2}";
                    }
                    """,
                    [
                        Line(8, "Una pieza reutilizable: cuenta hasta un límite y vuelve a 0."),
                        Line(13, "Avanzar indica si dio la vuelta."),
                        Line(20, "El reloj se compone de dos contadores."),
                        Line(25, "Cuando los segundos dan la vuelta, avanza un minuto."),
                    ],
                    "02:05"),
                new CommonMistake(
                    "Guardar una copia de datos que ya tienen las partes.",
                    "class Pedido\n{\n    private readonly List<Linea> _lineas = [];\n    public decimal Total { get; private set; }\n    // Total se actualiza a mano en cada operación\n}",
                    "Si una línea cambia, el total guardado queda desactualizado. Dos fuentes de verdad acaban contradiciéndose.",
                    "class Linea(decimal precio)\n{\n    public decimal Subtotal => precio;\n}\n\nclass Pedido\n{\n    private readonly List<Linea> _lineas = [];\n    public decimal Total => _lineas.Sum(l => l.Subtotal);\n}"),
                "n3.composicion.pred1", "n3.composicion.guiado", "n3.composicion.indep", "n3.composicion.app",
                ["n3.composicion.recall1", "n3.composicion.recall2", "n3.composicion.concepto1"],
                "Diseñas objetos compuestos que delegan responsabilidades en sus partes sin duplicar información.",
                18));
    }

    private static Skill Herencia(ContentRegistry r)
    {
        const string s = "n3.herencia";
        r.Short("n3.herencia.pred1", s, ItemPurpose.Prediction,
            "¿Qué muestra este programa?",
            "Al crear un Gerente, primero se ejecuta el constructor de la clase base (Empleado) y después el de la derivada. Gerente hereda Nombre y Saludo de Empleado.",
            ["Empleado creado\nGerente creado\nHola, soy Eva"], match: AnswerMatch.Output,
            code: """
                Gerente g = new Gerente("Eva");
                Console.WriteLine(g.Saludo());

                class Empleado
                {
                    public string Nombre { get; }

                    public Empleado(string nombre)
                    {
                        Nombre = nombre;
                        Console.WriteLine("Empleado creado");
                    }

                    public string Saludo() => $"Hola, soy {Nombre}";
                }

                class Gerente : Empleado
                {
                    public Gerente(string nombre) : base(nombre)
                    {
                        Console.WriteLine("Gerente creado");
                    }
                }
                """,
            difficulty: 2);

        r.Choice("n3.herencia.concepto1", s, ItemPurpose.Concept,
            "¿En cuál de estos casos la herencia es una mala elección?",
            "Una Pila no «es una» List: si heredara de List<T>, cualquiera podría insertar en medio o quitar del fondo, rompiendo sus reglas. Para reutilizar la lista, la Pila debe contener una (composición).",
            [
                Ok("class Pila : List<int>, solo para reutilizar Add y RemoveAt.", "Correcto: hereda operaciones que rompen las reglas de una pila."),
                No("class Perro : Animal", "Un perro es un animal: es un uso razonable."),
                No("class CuentaAhorro : Cuenta", "Una cuenta de ahorro es una cuenta: puede ser razonable."),
            ],
            dimension: KnowledgeDimension.Design);

        r.Choice("n3.herencia.recall1", s, ItemPurpose.Recall,
            "¿Qué hace «: base(nombre)» en el constructor de una clase derivada?",
            "Llama al constructor de la clase base con ese argumento antes de ejecutar el cuerpo del constructor derivado.",
            [
                Ok("Llama al constructor de la clase base con nombre.", "Correcto."),
                No("Crea un objeto nuevo de la clase base.", "No crea otro objeto: inicializa la parte base del mismo objeto."),
                No("Copia el campo nombre de la base.", "Llama a su constructor."),
            ]);

        r.Choice("n3.herencia.recall2", s, ItemPurpose.Recall,
            "¿De cuántas clases puede heredar directamente una clase en C#?",
            "De una sola clase base. Para combinar varios contratos se usan interfaces, que se pueden implementar en cantidad.",
            [
                Ok("De una sola.", "Correcto."),
                No("De tantas como se quiera.", "C# no tiene herencia múltiple de clases."),
                No("De dos como máximo.", "Solo de una."),
            ]);

        r.Choice("n3.herencia.bug1", s, ItemPurpose.BugFinding,
            "Este código no compila con el error CS7036 (falta un argumento para el constructor de la base). ¿Por qué?",
            "Vehiculo solo tiene un constructor con parámetro. El constructor de Moto debe indicar cómo construir la base: public Moto(string placa) : base(placa).",
            [
                Ok("Moto no llama a base(placa) y Vehiculo no tiene constructor sin parámetros.", "Correcto."),
                No("Moto debe redeclarar la propiedad Placa.", "La hereda; el problema es el constructor.", ErrorCategory.InheritanceContract),
                No("Una clase derivada no puede tener constructor.", "Sí puede, y aquí lo necesita.", ErrorCategory.InheritanceContract),
            ],
            code: """
                class Vehiculo
                {
                    public string Placa { get; }
                    public Vehiculo(string placa) => Placa = placa;
                }

                class Moto : Vehiculo
                {
                    public Moto(string placa)
                    {
                    }
                }
                """,
            targets: [ErrorCategory.InheritanceContract]);

        r.Code("n3.herencia.guiado", s, ItemPurpose.Guided, CodeMode.Function,
            "Completa CuentaAhorro, que hereda de Cuenta: su constructor recibe titular y tasa (porcentaje anual) y llama al de la base; agrega AplicarInteres() que deposita Saldo × tasa / 100.",
            """
            public class Cuenta(string titular)
            {
                public string Titular => titular;
                public decimal Saldo { get; private set; }
                public void Depositar(decimal monto) => Saldo += monto;
            }

            public class CuentaAhorro : Cuenta
            {
                private readonly decimal _tasa;

                public CuentaAhorro(string titular, decimal tasa) : base(titular)
                {
                    _tasa = tasa;
                }

                public void AplicarInteres()
                {
                }
            }
            """,
            """
            public class Cuenta(string titular)
            {
                public string Titular => titular;
                public decimal Saldo { get; private set; }
                public void Depositar(decimal monto) => Saldo += monto;
            }

            public class CuentaAhorro : Cuenta
            {
                private readonly decimal _tasa;

                public CuentaAhorro(string titular, decimal tasa) : base(titular)
                {
                    _tasa = tasa;
                }

                public void AplicarInteres() => Depositar(Saldo * _tasa / 100);
            }
            """,
            [
                Is("Interés", "{ var c = new CuentaAhorro(\"Ana\", 5m); c.Depositar(1000m); c.AplicarInteres(); return c.Saldo; }", "1050"),
                Is("Titular heredado", "new CuentaAhorro(\"Luis\", 1m).Titular", "Luis"),
                Is("Dos veces", "{ var c = new CuentaAhorro(\"Ana\", 10m); c.Depositar(100m); c.AplicarInteres(); c.AplicarInteres(); return c.Saldo; }", "121", hidden: true),
            ],
            "La derivada no puede tocar el set privado de Saldo, pero sí usar el método público Depositar que heredó.",
            ["Saldo tiene set privado en la base: usa Depositar.", "public void AplicarInteres() => Depositar(Saldo * _tasa / 100);"],
            requires: [CodeConstruct.Class], targets: [ErrorCategory.AccessModifier]);

        r.Code("n3.herencia.indep", s, ItemPurpose.Independent, CodeMode.Function,
            "Crea la clase Figura con propiedad Nombre (desde el constructor) y el método Describir() que devuelve «NOMBRE». Crea Circulo : Figura (radio) con Area() y Rectangulo : Figura (ancho, alto) con Area(). Los nombres son «Círculo» y «Rectángulo». Redondea las áreas a 2 decimales.",
            """
            public class Figura
            {
            }
            """,
            """
            public class Figura(string nombre)
            {
                public string Nombre => nombre;
                public string Describir() => Nombre;
            }

            public class Circulo(double radio) : Figura("Círculo")
            {
                public double Area() => Math.Round(Math.PI * radio * radio, 2);
            }

            public class Rectangulo(double ancho, double alto) : Figura("Rectángulo")
            {
                public double Area() => Math.Round(ancho * alto, 2);
            }
            """,
            [
                Is("Círculo", "new Circulo(1).Area()", "3.14"),
                Is("Rectángulo", "new Rectangulo(2, 3.5).Area()", "7"),
                Is("Nombre heredado", "new Circulo(2).Describir()", "Círculo", hidden: true),
                Is("Es una figura", "{ Figura f = new Rectangulo(1, 1); return f.Nombre; }", "Rectángulo", hidden: true, hint: "Rectangulo debe heredar de Figura para poder asignarse a una variable Figura."),
            ],
            "Con constructores primarios, la llamada a la base se escribe en la declaración: class Circulo(double radio) : Figura(\"Círculo\").",
            ["class Figura(string nombre) con Nombre y Describir.", "class Circulo(double radio) : Figura(\"Círculo\") { ... }"],
            difficulty: 2, requires: [CodeConstruct.Class]);

        r.Code("n3.herencia.app", s, ItemPurpose.Transfer, CodeMode.Function,
            "Aplicación: nómina. Empleado(nombre, salarioBase) con SalarioMensual() que devuelve el salario base. Vendedor : Empleado agrega Ventas (decimal, get; set;) y el método SalarioConComision() = base + 5 % de las ventas. Gerente : Empleado agrega un bono fijo en el constructor y SalarioConBono() = base + bono. Escribe además la clase estática Nomina con Total(List<Empleado>) que sume SalarioMensual() de todos.",
            """
            public class Empleado(string nombre, decimal salarioBase)
            {
                public string Nombre => nombre;
                public decimal SalarioMensual() => salarioBase;
            }
            """,
            """
            public class Empleado(string nombre, decimal salarioBase)
            {
                public string Nombre => nombre;
                public decimal SalarioMensual() => salarioBase;
            }

            public class Vendedor(string nombre, decimal salarioBase) : Empleado(nombre, salarioBase)
            {
                public decimal Ventas { get; set; }
                public decimal SalarioConComision() => SalarioMensual() + Ventas * 0.05m;
            }

            public class Gerente(string nombre, decimal salarioBase, decimal bono) : Empleado(nombre, salarioBase)
            {
                public decimal SalarioConBono() => SalarioMensual() + bono;
            }

            public static class Nomina
            {
                public static decimal Total(List<Empleado> empleados)
                {
                    decimal total = 0;
                    foreach (Empleado e in empleados)
                    {
                        total += e.SalarioMensual();
                    }
                    return total;
                }
            }
            """,
            [
                Is("Comisión", "new Vendedor(\"Ana\", 10000m) { Ventas = 20000m }.SalarioConComision()", "11000.00"),
                Is("Bono", "new Gerente(\"Luis\", 30000m, 5000m).SalarioConBono()", "35000"),
                Is("Nómina", "Nomina.Total([new Empleado(\"A\", 100m), new Vendedor(\"B\", 200m) { Ventas = 1000m }, new Gerente(\"C\", 300m, 50m)])", "600", hidden: true, hint: "Total usa SalarioMensual, que es el salario base en todas las clases."),
            ],
            "Una List<Empleado> puede contener vendedores y gerentes porque ambos son empleados. Observa la limitación: Total no puede incluir comisiones ni bonos sin preguntar el tipo de cada uno. La lección de polimorfismo resuelve esto.",
            ["Las derivadas se declaran como class Vendedor(string nombre, decimal salarioBase) : Empleado(nombre, salarioBase).", "Nomina.Total recorre la lista y suma e.SalarioMensual()."],
            difficulty: 3, requires: [CodeConstruct.Class], related: ["n3.static"]);

        return new Skill(s, UnitId, "Herencia",
            "Reutilizar y especializar; cuándo no usarla.",
            Tier.Intermediate, ["n3.composicion"],
            new Lesson(s,
                "Derivar clases que especializan a una base, encadenar constructores con base(...) y reconocer cuándo la herencia es inadecuada.",
                [
                    P("La herencia permite definir una clase a partir de otra. La clase derivada hereda los miembros de la base y puede agregar los suyos. Es la relación «es un»."),
                    Code("""
                        Perro p = new Perro("Firulais");
                        Console.WriteLine(p.Nombre);
                        Console.WriteLine(p.Ladrar());
                        Animal a = p;
                        Console.WriteLine(a.Nombre);

                        class Animal(string nombre)
                        {
                            public string Nombre => nombre;
                        }

                        class Perro(string nombre) : Animal(nombre)
                        {
                            public string Ladrar() => "¡Guau!";
                        }
                        """, "Un Perro es un Animal"),
                    P("Un objeto derivado puede asignarse a una variable del tipo base (Animal a = p), pero a través de esa variable solo se ven los miembros de Animal."),
                    P("El constructor derivado debe inicializar la base con : base(...). Con constructores primarios se escribe en la declaración: class Perro(string nombre) : Animal(nombre)."),
                    Warn("Los miembros private de la base no son accesibles desde la derivada; los protected sí."),
                    Design("Usa herencia solo si la derivada realmente «es un» tipo de la base y puede usarse en cualquier lugar donde se use la base. Si solo quieres reutilizar código, prefiere composición."),
                ],
                new WorkedExample("Base con lo común, derivadas con lo específico",
                    """
                    Publicacion[] items = [new Libro("Pedro Páramo", 1955, 124), new Revista("Ciencia", 2024, 37)];
                    foreach (Publicacion p in items)
                    {
                        Console.WriteLine(p.Ficha());
                    }
                    Console.WriteLine(((Libro)items[0]).Paginas);

                    class Publicacion(string titulo, int anio)
                    {
                        public string Titulo => titulo;
                        public int Anio => anio;
                        public string Ficha() => $"{Titulo} ({Anio})";
                    }

                    class Libro(string titulo, int anio, int paginas) : Publicacion(titulo, anio)
                    {
                        public int Paginas => paginas;
                    }

                    class Revista(string titulo, int anio, int numero) : Publicacion(titulo, anio)
                    {
                        public int Numero => numero;
                    }
                    """,
                    [
                        Line(1, "Un array del tipo base puede contener libros y revistas."),
                        Line(4, "Ficha está definida una sola vez, en la base."),
                        Line(6, "Para ver Paginas hay que convertir al tipo derivado."),
                        Line(15, "La derivada pasa los datos comunes a la base."),
                    ],
                    "Pedro Páramo (1955)\nCiencia (2024)\n124"),
                new CommonMistake(
                    "Heredar solo para reutilizar código.",
                    "class Pila : List<int>\n{\n    public void Apilar(int x) => Add(x);\n}",
                    "La Pila hereda Insert, Clear, RemoveAt… y cualquiera puede romper sus reglas. Una pila no es una lista.",
                    "class Pila\n{\n    private readonly List<int> _datos = [];\n    public void Apilar(int x) => _datos.Add(x);\n    public int Cantidad => _datos.Count;\n}"),
                "n3.herencia.pred1", "n3.herencia.guiado", "n3.herencia.indep", "n3.herencia.app",
                ["n3.herencia.recall1", "n3.herencia.recall2", "n3.herencia.concepto1"],
                "Creas jerarquías pequeñas donde la derivada realmente es un tipo de la base y eliges composición cuando no lo es.",
                18));
    }

    private static Skill Polimorfismo(ContentRegistry r)
    {
        const string s = "n3.polimorfismo";
        r.Short("n3.polimorfismo.pred1", s, ItemPurpose.Prediction,
            "¿Qué muestra este programa?",
            "Sonido es virtual y Gato lo sobrescribe con override. Aunque la variable es de tipo Animal, se ejecuta la versión del objeto real (Gato). Eso es polimorfismo.",
            ["Miau"], match: AnswerMatch.Output,
            code: """
                Animal a = new Gato();
                Console.WriteLine(a.Sonido());

                class Animal
                {
                    public virtual string Sonido() => "...";
                }

                class Gato : Animal
                {
                    public override string Sonido() => "Miau";
                }
                """,
            wrong: new() { ["..."] = "Con virtual/override decide el tipo del objeto, no el de la variable." });

        r.Choice("n3.polimorfismo.pred2", s, ItemPurpose.Prediction,
            "¿Qué muestra este programa? Fíjate en que Gato usa new en lugar de override.",
            "Con new, el método del Gato oculta al de Animal pero no lo sobrescribe. Al llamar a través de una variable Animal, se usa la versión de Animal. Por eso new casi nunca es lo que se quiere.",
            [
                Ok("...\nMiau", "Correcto: con new decide el tipo de la variable."),
                No("Miau\nMiau", "Eso pasaría con override.", ErrorCategory.InheritanceContract),
                No("...\n...", "La segunda llamada es sobre una variable Gato: se ve la versión nueva.", ErrorCategory.InheritanceContract),
            ],
            code: """
                Gato g = new Gato();
                Animal a = g;
                Console.WriteLine(a.Sonido());
                Console.WriteLine(g.Sonido());

                class Animal
                {
                    public string Sonido() => "...";
                }

                class Gato : Animal
                {
                    public new string Sonido() => "Miau";
                }
                """,
            difficulty: 3,
            targets: [ErrorCategory.InheritanceContract]);

        r.Choice("n3.polimorfismo.recall1", s, ItemPurpose.Recall,
            "¿Qué palabras hacen falta para que una clase derivada pueda sobrescribir un método de la base?",
            "La base marca el método como virtual (o abstract) y la derivada lo redefine con override.",
            [
                Ok("virtual en la base y override en la derivada.", "Correcto."),
                No("override en la base y virtual en la derivada.", "Es al revés."),
                No("Ninguna: todos los métodos se pueden sobrescribir.", "En C# los métodos no son virtuales por defecto."),
            ]);

        r.Choice("n3.polimorfismo.recall2", s, ItemPurpose.Recall,
            "Dentro de un override, ¿cómo se llama a la versión de la clase base?",
            "Con base.Metodo(...). Sirve para extender el comportamiento en lugar de reemplazarlo por completo.",
            [
                Ok("base.Metodo()", "Correcto."),
                No("super.Metodo()", "Esa es la sintaxis de Java."),
                No("this.Metodo()", "Llamaría al propio override: recursión infinita."),
            ]);

        r.Choice("n3.polimorfismo.bug1", s, ItemPurpose.BugFinding,
            "Este código calcula el total de la nómina, pero hay que modificarlo cada vez que se crea un tipo de empleado nuevo. ¿Qué diseño lo evita?",
            "Preguntar por el tipo con is en cada cálculo obliga a tocar este código ante cada tipo nuevo. Con un método virtual Salario() que cada derivada sobrescribe, el total solo llama e.Salario() y funciona con cualquier tipo futuro.",
            [
                Ok("Un método virtual Salario() sobrescrito en cada derivada, y llamar siempre e.Salario().", "Correcto: eso es polimorfismo."),
                No("Agregar un else if por cada tipo nuevo.", "Es lo que ya hace y es justo el problema.", ErrorCategory.ConceptMisunderstanding),
                No("Guardar el tipo en un string y usar switch.", "Mismo problema, con menos ayuda del compilador.", ErrorCategory.ConceptMisunderstanding),
            ],
            code: """
                decimal total = 0;
                foreach (Empleado e in empleados)
                {
                    if (e is Vendedor v) total += v.Base + v.Comision;
                    else if (e is Gerente g) total += g.Base + g.Bono;
                    else total += e.Base;
                }
                """,
            dimension: KnowledgeDimension.Design);

        r.Code("n3.polimorfismo.guiado", s, ItemPurpose.Guided, CodeMode.Function,
            "Haz que Area sea polimórfica: márcala como virtual en Figura y sobrescríbela en Cuadrado (lado²) y Circulo (π·r², redondeado a 2 decimales).",
            """
            public class Figura
            {
                public double Area() => 0;
            }

            public class Cuadrado(double lado) : Figura
            {
            }

            public class Circulo(double radio) : Figura
            {
            }
            """,
            """
            public class Figura
            {
                public virtual double Area() => 0;
            }

            public class Cuadrado(double lado) : Figura
            {
                public override double Area() => lado * lado;
            }

            public class Circulo(double radio) : Figura
            {
                public override double Area() => Math.Round(Math.PI * radio * radio, 2);
            }
            """,
            [
                Is("Cuadrado como Figura", "{ Figura f = new Cuadrado(3); return f.Area(); }", "9"),
                Is("Círculo como Figura", "{ Figura f = new Circulo(1); return f.Area(); }", "3.14"),
                Is("Lista mixta", "{ List<Figura> fs = [new Cuadrado(2), new Circulo(2)]; double t = 0; foreach (var f in fs) t += f.Area(); return t; }", "16.57", hidden: true),
            ],
            "Gracias a virtual/override, el código que recorre figuras no necesita saber de qué tipo es cada una.",
            ["public virtual double Area() => 0; en la base.", "public override double Area() => lado * lado; en Cuadrado."],
            requires: [CodeConstruct.Class], targets: [ErrorCategory.InheritanceContract]);

        r.Code("n3.polimorfismo.indep", s, ItemPurpose.Independent, CodeMode.Function,
            "Rediseña la nómina con polimorfismo: Empleado(nombre, salarioBase) con virtual decimal Salario(). Vendedor sobrescribe sumando 5 % de Ventas. Gerente sobrescribe sumando un bono. Sobrescribe también ToString() en Empleado para devolver «NOMBRE: SALARIO» (con el salario polimórfico, formato F2). Nomina.Total(List<Empleado>) suma Salario() de todos.",
            """
            public class Empleado(string nombre, decimal salarioBase)
            {
                public string Nombre => nombre;
                public decimal Salario() => salarioBase;
            }
            """,
            """
            public class Empleado(string nombre, decimal salarioBase)
            {
                public string Nombre => nombre;
                public decimal SalarioBase => salarioBase;
                public virtual decimal Salario() => salarioBase;
                public override string ToString() => $"{Nombre}: {Salario():F2}";
            }

            public class Vendedor(string nombre, decimal salarioBase) : Empleado(nombre, salarioBase)
            {
                public decimal Ventas { get; set; }
                public override decimal Salario() => base.Salario() + Ventas * 0.05m;
            }

            public class Gerente(string nombre, decimal salarioBase, decimal bono) : Empleado(nombre, salarioBase)
            {
                public override decimal Salario() => base.Salario() + bono;
            }

            public static class Nomina
            {
                public static decimal Total(List<Empleado> empleados)
                {
                    decimal total = 0;
                    foreach (Empleado e in empleados) total += e.Salario();
                    return total;
                }
            }
            """,
            [
                Is("Vendedor", "new Vendedor(\"Ana\", 1000m) { Ventas = 2000m }.Salario()", "1100.00"),
                Is("ToString polimórfico", "new Gerente(\"Luis\", 3000m, 500m).ToString()", "Luis: 3500.00"),
                Is("Nómina", "Nomina.Total([new Empleado(\"A\", 100m), new Vendedor(\"B\", 200m) { Ventas = 1000m }, new Gerente(\"C\", 300m, 50m)])", "700.00", hidden: true, hint: "Ahora el total incluye comisiones y bonos sin preguntar el tipo."),
            ],
            "ToString en la base llama a Salario(), que es virtual: cada objeto muestra su propio salario sin que la base conozca a sus derivadas.",
            ["Marca Salario como virtual y en las derivadas usa base.Salario() + extra.", "public override string ToString() => $\"{Nombre}: {Salario():F2}\";"],
            difficulty: 2, requires: [CodeConstruct.Class], related: ["n3.herencia"]);

        r.Code("n3.polimorfismo.app", s, ItemPurpose.Transfer, CodeMode.Function,
            "Aplicación: tarifas de envío. Envio(decimal pesoKg) con virtual Costo(): 50 + 10 por kg. EnvioExpress sobrescribe: el doble del costo normal. EnvioInternacional(peso, pais) sobrescribe: costo normal + 300, y +500 más si el país es «Australia». Cotizacion.MasBarato(List<Envio>) devuelve el índice del envío más barato (el primero en empate) o -1 si la lista está vacía.",
            """
            public class Envio(decimal pesoKg)
            {
                public decimal PesoKg => pesoKg;
                public decimal Costo() => 0;
            }
            """,
            """
            public class Envio(decimal pesoKg)
            {
                public decimal PesoKg => pesoKg;
                public virtual decimal Costo() => 50 + 10 * PesoKg;
            }

            public class EnvioExpress(decimal pesoKg) : Envio(pesoKg)
            {
                public override decimal Costo() => base.Costo() * 2;
            }

            public class EnvioInternacional(decimal pesoKg, string pais) : Envio(pesoKg)
            {
                public override decimal Costo() => base.Costo() + 300 + (pais == "Australia" ? 500 : 0);
            }

            public static class Cotizacion
            {
                public static int MasBarato(List<Envio> opciones)
                {
                    int mejor = -1;
                    for (int i = 0; i < opciones.Count; i++)
                    {
                        if (mejor == -1 || opciones[i].Costo() < opciones[mejor].Costo())
                        {
                            mejor = i;
                        }
                    }
                    return mejor;
                }
            }
            """,
            [
                Is("Normal", "new Envio(2).Costo()", "70"),
                Is("Express", "new EnvioExpress(2).Costo()", "140"),
                Is("Internacional", "new EnvioInternacional(1, \"Australia\").Costo()", "860", hidden: true),
                Is("Más barato", "Cotizacion.MasBarato([new EnvioExpress(1), new EnvioInternacional(1, \"Chile\"), new Envio(5)])", "2", hidden: true),
                Is("Vacía", "Cotizacion.MasBarato([])", "-1", hidden: true, edge: true),
            ],
            "Agregar un nuevo tipo de envío no obliga a cambiar Cotizacion: solo hay que sobrescribir Costo().",
            ["Envio.Costo es virtual; las derivadas usan base.Costo() y agregan lo suyo.", "MasBarato recorre con un índice y guarda el del menor costo."],
            difficulty: 3, requires: [CodeConstruct.Class], targets: [ErrorCategory.InheritanceContract]);

        return new Skill(s, UnitId, "Polimorfismo, sobrescritura y ocultamiento",
            "virtual, override y new.",
            Tier.Intermediate, ["n3.herencia"],
            new Lesson(s,
                "Sobrescribir métodos virtuales, reutilizar la versión base con base., distinguir override de new y aprovechar el polimorfismo para eliminar condicionales por tipo.",
                [
                    P("Polimorfismo significa que un mismo llamado (f.Area()) produce comportamientos distintos según el tipo real del objeto."),
                    P("En la base, el método se marca como virtual. En la derivada se redefine con override. Al llamar a través de una variable del tipo base, se ejecuta la versión del objeto real."),
                    Code("""
                        List<Notificacion> pendientes = [new Correo("ana@x.com"), new Sms("5512345678")];
                        foreach (Notificacion n in pendientes)
                        {
                            Console.WriteLine(n.Enviar("Tu pedido salió"));
                        }

                        class Notificacion
                        {
                            public virtual string Enviar(string texto) => texto;
                        }

                        class Correo(string direccion) : Notificacion
                        {
                            public override string Enviar(string texto) => $"Correo a {direccion}: {texto}";
                        }

                        class Sms(string numero) : Notificacion
                        {
                            public override string Enviar(string texto) => $"SMS a {numero}: {base.Enviar(texto)}";
                        }
                        """, "Una lista, comportamientos distintos"),
                    P("ToString, Equals y GetHashCode son métodos virtuales de object: cualquier clase puede sobrescribirlos."),
                    Warn("new en lugar de override oculta el método sin sobrescribirlo: a través de la base se sigue llamando a la versión original. Casi siempre es un error."),
                    Design("Si encuentras cadenas de if (x is TipoA) … else if (x is TipoB), probablemente ese comportamiento debería ser un método virtual."),
                ],
                new WorkedExample("Formatear distintos tipos de mensajes",
                    """
                    Mensaje[] chat = [new Mensaje("Ana", "Hola"), new Sistema("Luis entró"), new Mensaje("Luis", "¿Qué tal?")];
                    foreach (Mensaje m in chat)
                    {
                        Console.WriteLine(m);
                    }

                    class Mensaje(string autor, string texto)
                    {
                        public string Autor => autor;
                        public string Texto => texto;
                        public override string ToString() => $"{Autor}: {Texto}";
                    }

                    class Sistema(string texto) : Mensaje("sistema", texto)
                    {
                        public override string ToString() => $"* {Texto} *";
                    }
                    """,
                    [
                        Line(4, "WriteLine llama a ToString(), que es virtual."),
                        Line(10, "Mensaje sobrescribe el ToString de object."),
                        Line(13, "Sistema hereda de Mensaje y lo vuelve a sobrescribir."),
                        Line(15, "Cada objeto decide cómo mostrarse."),
                    ],
                    "Ana: Hola\n* Luis entró *\nLuis: ¿Qué tal?"),
                new CommonMistake(
                    "Olvidar virtual en la base (o usar new en la derivada).",
                    "class A { public string Nombre() => \"A\"; }\nclass B : A { public new string Nombre() => \"B\"; }\nA x = new B();\nConsole.WriteLine(x.Nombre()); // A",
                    "Sin virtual/override no hay polimorfismo: decide el tipo de la variable, no el del objeto.",
                    "A x = new B();\nConsole.WriteLine(x.Nombre());\n\nclass A { public virtual string Nombre() => \"A\"; }\nclass B : A { public override string Nombre() => \"B\"; }"),
                "n3.polimorfismo.pred1", "n3.polimorfismo.guiado", "n3.polimorfismo.indep", "n3.polimorfismo.app",
                ["n3.polimorfismo.recall1", "n3.polimorfismo.recall2", "n3.polimorfismo.pred2"],
                "Reemplazas condicionales por tipo con métodos virtuales y predices qué versión se ejecuta en cada llamada.",
                20));
    }

    private static Skill AbstractasInterfaces(ContentRegistry r)
    {
        const string s = "n3.abstractas-interfaces";
        r.Choice("n3.abstractas-interfaces.pred1", s, ItemPurpose.Prediction,
            "¿Qué ocurre con este programa?",
            "Figura es abstract: no se pueden crear objetos de ella directamente, solo de clases derivadas que implementen sus miembros abstractos. El compilador lo rechaza (CS0144).",
            [
                Ok("No compila: no se puede crear una instancia de una clase abstracta.", "Correcto."),
                No("Muestra 0", "Area no tiene implementación en Figura, y no se puede instanciar.", ErrorCategory.InheritanceContract),
                No("Lanza una excepción al ejecutar.", "El error se detecta al compilar.", ErrorCategory.InheritanceContract),
            ],
            code: """
                Figura f = new Figura();
                Console.WriteLine(f.Area());

                abstract class Figura
                {
                    public abstract double Area();
                }
                """,
            targets: [ErrorCategory.InheritanceContract]);

        r.Short("n3.abstractas-interfaces.pred2", s, ItemPurpose.Prediction,
            "¿Qué muestra este programa?",
            "Pato implementa dos interfaces, así que puede usarse donde se espera IVolador y donde se espera INadador. Avion solo implementa IVolador.",
            ["Pato vuela\nAvión vuela\nPato nada"], match: AnswerMatch.Output,
            code: """
                IVolador[] voladores = [new Pato(), new Avion()];
                foreach (IVolador v in voladores)
                {
                    Console.WriteLine(v.Volar());
                }
                INadador n = new Pato();
                Console.WriteLine(n.Nadar());

                interface IVolador { string Volar(); }
                interface INadador { string Nadar(); }

                class Pato : IVolador, INadador
                {
                    public string Volar() => "Pato vuela";
                    public string Nadar() => "Pato nada";
                }

                class Avion : IVolador
                {
                    public string Volar() => "Avión vuela";
                }
                """,
            difficulty: 2);

        r.Choice("n3.abstractas-interfaces.concepto1", s, ItemPurpose.Concept,
            "¿Cuándo conviene una interfaz y cuándo una clase abstracta?",
            "Una interfaz define un contrato (qué se puede hacer) que pueden cumplir tipos sin relación entre sí; una clase puede implementar varias. Una clase abstracta además comparte estado y código entre derivadas que sí son variantes de lo mismo, pero solo se hereda de una.",
            [
                Ok("Interfaz para un contrato que pueden cumplir tipos no relacionados; clase abstracta para compartir estado y código en una familia.", "Correcto."),
                No("Son intercambiables; es cuestión de gusto.", "Difieren en herencia múltiple, estado y propósito."),
                No("Las interfaces son más rápidas.", "La diferencia es de diseño, no de rendimiento."),
            ],
            dimension: KnowledgeDimension.Design);

        r.Choice("n3.abstractas-interfaces.recall1", s, ItemPurpose.Recall,
            "¿Qué obliga un miembro abstract a una clase derivada no abstracta?",
            "A implementarlo con override; si no, la derivada no compila (CS0534).",
            [
                Ok("A implementarlo con override.", "Correcto."),
                No("A no usarlo.", "Al contrario: debe darle implementación."),
                No("Nada; es opcional.", "Eso describe a un miembro virtual."),
            ]);

        r.Choice("n3.abstractas-interfaces.recall2", s, ItemPurpose.Recall,
            "Por convención, ¿cómo empiezan los nombres de las interfaces en .NET?",
            "Con I mayúscula: IComparable, IEnumerable, IDisposable.",
            [
                Ok("Con la letra I: IRepositorio.", "Correcto."),
                No("Con Interface: InterfaceRepositorio.", "La convención es solo la I."),
                No("Con minúscula: repositorio.", "Los tipos usan PascalCase con prefijo I."),
            ]);

        r.Choice("n3.abstractas-interfaces.bug1", s, ItemPurpose.BugFinding,
            "Este código no compila. ¿Por qué?",
            "Celsius implementa ISensor pero no proporciona Unidad, que es parte del contrato. Una clase debe implementar todos los miembros de la interfaz (CS0535).",
            [
                Ok("Celsius no implementa Unidad, que exige la interfaz.", "Correcto."),
                No("Las interfaces no pueden tener propiedades.", "Sí pueden.", ErrorCategory.ConceptMisunderstanding),
                No("Falta override en Leer.", "Al implementar una interfaz no se usa override.", ErrorCategory.InheritanceContract),
            ],
            code: """
                interface ISensor
                {
                    double Leer();
                    string Unidad { get; }
                }

                class Celsius : ISensor
                {
                    public double Leer() => 21.5;
                }
                """,
            targets: [ErrorCategory.InheritanceContract]);

        r.Code("n3.abstractas-interfaces.guiado", s, ItemPurpose.Guided, CodeMode.Function,
            "Completa: la interfaz IDescuento declara decimal Aplicar(decimal precio). Implementa DescuentoPorcentaje(int porcentaje) y DescuentoFijo(decimal monto) (sin bajar de 0). Caja.Cobrar(decimal precio, IDescuento d) aplica el descuento.",
            """
            public interface IDescuento
            {
                decimal Aplicar(decimal precio);
            }

            public class DescuentoPorcentaje(int porcentaje) : IDescuento
            {
                public decimal Aplicar(decimal precio) => precio;
            }

            public class DescuentoFijo(decimal monto) : IDescuento
            {
                public decimal Aplicar(decimal precio) => precio;
            }

            public static class Caja
            {
                public static decimal Cobrar(decimal precio, IDescuento d) => d.Aplicar(precio);
            }
            """,
            """
            public interface IDescuento
            {
                decimal Aplicar(decimal precio);
            }

            public class DescuentoPorcentaje(int porcentaje) : IDescuento
            {
                public decimal Aplicar(decimal precio) => precio * (100 - porcentaje) / 100;
            }

            public class DescuentoFijo(decimal monto) : IDescuento
            {
                public decimal Aplicar(decimal precio) => Math.Max(0, precio - monto);
            }

            public static class Caja
            {
                public static decimal Cobrar(decimal precio, IDescuento d) => d.Aplicar(precio);
            }
            """,
            [
                Is("Porcentaje", "Caja.Cobrar(200m, new DescuentoPorcentaje(25))", "150"),
                Is("Fijo", "Caja.Cobrar(200m, new DescuentoFijo(30m))", "170"),
                Is("Fijo mayor que el precio", "Caja.Cobrar(20m, new DescuentoFijo(30m))", "0", hidden: true, edge: true),
            ],
            "Caja depende solo del contrato IDescuento: acepta cualquier descuento presente o futuro.",
            ["Porcentaje: precio * (100 - porcentaje) / 100.", "Fijo: Math.Max(0, precio - monto)."],
            requires: [CodeConstruct.Interface]);

        r.Code("n3.abstractas-interfaces.indep", s, ItemPurpose.Independent, CodeMode.Function,
            "Crea la clase abstracta Cuenta(string titular) con Saldo (set protegido), Depositar(decimal) y el método abstracto bool PuedeRetirar(decimal monto). Retirar(monto) llama a PuedeRetirar y lanza InvalidOperationException si es false. Implementa CuentaDebito (no permite saldo negativo) y CuentaCredito(titular, limite) (permite bajar hasta -limite).",
            """
            public abstract class Cuenta(string titular)
            {
                public string Titular => titular;
            }
            """,
            """
            public abstract class Cuenta(string titular)
            {
                public string Titular => titular;
                public decimal Saldo { get; protected set; }

                public void Depositar(decimal monto) => Saldo += monto;

                protected abstract bool PuedeRetirar(decimal monto);

                public void Retirar(decimal monto)
                {
                    if (!PuedeRetirar(monto)) throw new InvalidOperationException("Fondos insuficientes");
                    Saldo -= monto;
                }
            }

            public class CuentaDebito(string titular) : Cuenta(titular)
            {
                protected override bool PuedeRetirar(decimal monto) => Saldo - monto >= 0;
            }

            public class CuentaCredito(string titular, decimal limite) : Cuenta(titular)
            {
                protected override bool PuedeRetirar(decimal monto) => Saldo - monto >= -limite;
            }
            """,
            [
                Is("Débito", "{ Cuenta c = new CuentaDebito(\"Ana\"); c.Depositar(100); c.Retirar(40); return c.Saldo; }", "60"),
                Is("Crédito", "{ Cuenta c = new CuentaCredito(\"Ana\", 500); c.Retirar(300); return c.Saldo; }", "-300"),
                Throws("Débito sin fondos", "{ Cuenta c = new CuentaDebito(\"Ana\"); c.Retirar(1); return 0; }", "InvalidOperationException", hidden: true),
                Throws("Límite de crédito", "{ Cuenta c = new CuentaCredito(\"Ana\", 100); c.Retirar(101); return 0; }", "InvalidOperationException", hidden: true),
                Is("Justo al límite", "{ Cuenta c = new CuentaCredito(\"Ana\", 100); c.Retirar(100); return c.Saldo; }", "-100", hidden: true, edge: true),
            ],
            "La base fija el algoritmo (validar y restar) y deja a cada derivada solo la regla que cambia: es el patrón «método plantilla».",
            ["protected abstract bool PuedeRetirar(decimal monto); en la base.", "Saldo { get; protected set; } permite que las derivadas lo lean y la base lo modifique."],
            difficulty: 3, requires: [CodeConstruct.Class], targets: [ErrorCategory.InheritanceContract]);

        r.Code("n3.abstractas-interfaces.app", s, ItemPurpose.Transfer, CodeMode.Function,
            "Aplicación: un sistema de reglas para validar contraseñas. Interfaz IRegla con string Nombre { get; } y bool Cumple(string clave). Implementa LongitudMinima(int n) («mínimo N caracteres»), TieneDigito («al menos un dígito») y TieneMayuscula («al menos una mayúscula»). Validador(List<IRegla> reglas) con Fallas(string clave) que devuelve una List<string> con los nombres de las reglas que no se cumplen, en orden.",
            """
            public interface IRegla
            {
            }

            public class Validador(List<IRegla> reglas)
            {
                public List<string> Fallas(string clave) => [];
            }
            """,
            """
            public interface IRegla
            {
                string Nombre { get; }
                bool Cumple(string clave);
            }

            public class LongitudMinima(int n) : IRegla
            {
                public string Nombre => $"mínimo {n} caracteres";
                public bool Cumple(string clave) => clave.Length >= n;
            }

            public class TieneDigito : IRegla
            {
                public string Nombre => "al menos un dígito";
                public bool Cumple(string clave) => clave.Any(char.IsDigit);
            }

            public class TieneMayuscula : IRegla
            {
                public string Nombre => "al menos una mayúscula";
                public bool Cumple(string clave) => clave.Any(char.IsUpper);
            }

            public class Validador(List<IRegla> reglas)
            {
                public List<string> Fallas(string clave)
                {
                    List<string> fallas = [];
                    foreach (IRegla r in reglas)
                    {
                        if (!r.Cumple(clave)) fallas.Add(r.Nombre);
                    }
                    return fallas;
                }
            }
            """,
            [
                Is("Todo bien", "new Validador([new LongitudMinima(8), new TieneDigito(), new TieneMayuscula()]).Fallas(\"Segura123\").Count", "0"),
                Is("Varias fallas", "new Validador([new LongitudMinima(8), new TieneDigito(), new TieneMayuscula()]).Fallas(\"abc\")", "[\"mínimo 8 caracteres\", \"al menos un dígito\", \"al menos una mayúscula\"]"),
                Is("Configurable", "new Validador([new TieneDigito()]).Fallas(\"abc\")", "[\"al menos un dígito\"]", hidden: true),
                Is("Sin reglas", "new Validador([]).Fallas(\"x\").Count", "0", hidden: true, edge: true),
            ],
            "Agregar una regla nueva es crear una clase; el Validador no cambia. Es el principio abierto/cerrado de SOLID.",
            ["La interfaz declara Nombre y Cumple.", "Cada regla es una clase pequeña; el Validador recorre la lista y junta los nombres de las que fallan."],
            difficulty: 3, requires: [CodeConstruct.Interface], related: ["n1.validacion"]);

        return new Skill(s, UnitId, "Clases abstractas e interfaces",
            "Contratos y comportamiento compartido.",
            Tier.Essential, ["n3.polimorfismo"],
            new Lesson(s,
                "Definir contratos con interfaces, compartir código con clases abstractas y programar contra abstracciones en lugar de clases concretas.",
                [
                    P("Una interfaz es un contrato: declara qué miembros debe tener un tipo, sin decir cómo se implementan. Una clase puede implementar varias interfaces."),
                    Code("""
                        List<IExportador> exportadores = [new ExportadorCsv(), new ExportadorJson()];
                        foreach (IExportador e in exportadores)
                        {
                            Console.WriteLine(e.Exportar(["Ana", "Luis"]));
                        }

                        interface IExportador
                        {
                            string Exportar(string[] nombres);
                        }

                        class ExportadorCsv : IExportador
                        {
                            public string Exportar(string[] nombres) => string.Join(",", nombres);
                        }

                        class ExportadorJson : IExportador
                        {
                            public string Exportar(string[] nombres) => "[\"" + string.Join("\",\"", nombres) + "\"]";
                        }
                        """, "Dos implementaciones de un contrato"),
                    P("Una clase abstracta no se puede instanciar. Puede tener campos, métodos con implementación y miembros abstract que las derivadas deben implementar con override."),
                    P("Programar contra la interfaz (IExportador) en lugar de la clase concreta permite cambiar la implementación sin tocar el código que la usa."),
                    Tip("Las interfaces de .NET que más verás: IEnumerable<T>, IComparable<T>, IDisposable, IEquatable<T>."),
                ],
                new WorkedExample("Ordenar con un contrato de .NET",
                    """
                    List<Tarea> tareas = [new Tarea("Pagar luz", 2), new Tarea("Estudiar", 1), new Tarea("Correr", 3)];
                    tareas.Sort();
                    foreach (Tarea t in tareas)
                    {
                        Console.WriteLine(t);
                    }

                    class Tarea(string titulo, int prioridad) : IComparable<Tarea>
                    {
                        public int Prioridad => prioridad;

                        public int CompareTo(Tarea? otra) => otra is null ? 1 : Prioridad.CompareTo(otra.Prioridad);

                        public override string ToString() => $"[{prioridad}] {titulo}";
                    }
                    """,
                    [
                        Line(2, "Sort funciona porque Tarea cumple el contrato IComparable<Tarea>."),
                        Line(8, "Implementar la interfaz es declararla después de los dos puntos."),
                        Line(12, "CompareTo devuelve negativo, 0 o positivo según el orden."),
                    ],
                    "[1] Estudiar\n[2] Pagar luz\n[3] Correr"),
                new CommonMistake(
                    "Depender de la clase concreta en lugar del contrato.",
                    "class Reporte\n{\n    private readonly ExportadorCsv _exportador = new();\n}",
                    "Cambiar el formato obliga a modificar Reporte. Además no se puede probar con un exportador falso.",
                    "interface IExportador { string Exportar(string[] datos); }\n\nclass Reporte(IExportador exportador)\n{\n    public string Generar(string[] datos) => exportador.Exportar(datos);\n}"),
                "n3.abstractas-interfaces.pred1", "n3.abstractas-interfaces.guiado", "n3.abstractas-interfaces.indep", "n3.abstractas-interfaces.app",
                ["n3.abstractas-interfaces.recall1", "n3.abstractas-interfaces.recall2", "n3.abstractas-interfaces.pred2", "n3.abstractas-interfaces.concepto1"],
                "Defines contratos con interfaces, compartes comportamiento con clases abstractas y escribes código que depende de abstracciones.",
                20));
    }
}
