using CSharpMastery.Content.Infrastructure;
using CSharpMastery.Domain.Curriculum;
using CSharpMastery.Domain.Items;
using CSharpMastery.Domain.Progress;
using static CSharpMastery.Content.Infrastructure.ContentRegistry;
using static CSharpMastery.Content.Infrastructure.L;

namespace CSharpMastery.Content.Nivel3;

internal static class U01Clases
{
    public const string UnitId = "n3.u1";

    public static Unit Build(ContentRegistry r) => new(UnitId, "n3", "Clases y objetos",
        "Clases, instancias, miembros, constructores y encapsulación.",
        [Clases(r), Propiedades(r), Constructores(r), Encapsulacion(r), Static(r)]);

    private static Skill Clases(ContentRegistry r)
    {
        const string s = "n3.clases";
        r.Choice("n3.clases.pred1", s, ItemPurpose.Prediction,
            "¿Qué muestra este programa?",
            "Las clases son tipos por referencia. b = a no crea otro perro: ambas variables apuntan al mismo objeto, así que cambiar b.Nombre cambia lo que ve a.",
            [
                Ok("Toby Toby", "Correcto: a y b son el mismo objeto."),
                No("Max Toby", "b = a no copia el objeto, copia la referencia.", ErrorCategory.ConceptMisunderstanding),
                No("No compila", "Es código válido.", ErrorCategory.ConceptMisunderstanding),
            ],
            code: """
                Perro a = new Perro();
                a.Nombre = "Max";
                Perro b = a;
                b.Nombre = "Toby";
                Console.WriteLine($"{a.Nombre} {b.Nombre}");

                class Perro
                {
                    public string Nombre = "";
                }
                """,
            difficulty: 2);

        r.Choice("n3.clases.pred2", s, ItemPurpose.Prediction,
            "¿Qué ocurre al ejecutar este programa?",
            "c se declaró pero nunca se le asignó un objeto con new: vale null. Acceder a un miembro de null lanza NullReferenceException.",
            [
                Ok("Se lanza NullReferenceException", "Correcto: declarar la variable no crea el objeto."),
                No("Muestra 0", "No hay ningún objeto del que leer Saldo.", ErrorCategory.NullReference),
                No("Muestra una línea vacía", "Se lanza una excepción antes de mostrar nada.", ErrorCategory.NullReference),
            ],
            code: """
                Cuenta? c = null;
                Console.WriteLine(c!.Saldo);

                class Cuenta
                {
                    public decimal Saldo;
                }
                """,
            targets: [ErrorCategory.NullReference]);

        r.Choice("n3.clases.recall1", s, ItemPurpose.Recall,
            "¿Cuál es la diferencia entre una clase y un objeto?",
            "La clase es la definición (el plano); un objeto o instancia es un ejemplar concreto creado con new a partir de ella. De una clase se pueden crear muchos objetos.",
            [
                Ok("La clase es la definición; el objeto es una instancia concreta creada con new.", "Correcto."),
                No("Son sinónimos.", "Una clase existe una vez; puede haber muchos objetos de ella."),
                No("El objeto es la definición y la clase la instancia.", "Es al revés."),
            ]);

        r.Choice("n3.clases.concepto1", s, ItemPurpose.Concept,
            "Al diseñar un programa de biblioteca, ¿qué es más razonable modelar como clase?",
            "Una clase representa un concepto con datos y comportamiento propios: un Libro tiene título, autor y si está prestado. «Título» es solo un dato de ese concepto.",
            [
                Ok("Libro, con datos como título, autor y si está prestado.", "Correcto."),
                No("Titulo, con un único campo de texto.", "Un título es un dato de un libro, no un concepto con comportamiento."),
                No("Una clase por cada libro concreto: ElQuijote, Rayuela…", "Cada libro concreto es un objeto de la clase Libro.", ErrorCategory.ConceptMisunderstanding),
            ],
            dimension: KnowledgeDimension.Design);

        r.Choice("n3.clases.bug1", s, ItemPurpose.BugFinding,
            "El programa debería mostrar dos alumnos distintos, pero muestra «Eva Eva». ¿Por qué?",
            "Solo se creó un objeto con new. Al agregarlo dos veces, la lista contiene dos referencias al mismo alumno, y el segundo cambio de nombre afecta a ambas posiciones.",
            [
                Ok("Se reutiliza el mismo objeto: hay que crear otro con new para el segundo alumno.", "Correcto."),
                No("List no admite objetos.", "Puede contener objetos de cualquier clase.", ErrorCategory.ConceptMisunderstanding),
                No("Hay que usar un array en lugar de una lista.", "Con un array ocurriría lo mismo.", ErrorCategory.ConceptMisunderstanding),
            ],
            code: """
                List<Alumno> grupo = [];
                Alumno a = new Alumno();
                a.Nombre = "Ana";
                grupo.Add(a);
                a.Nombre = "Eva";
                grupo.Add(a);
                Console.WriteLine($"{grupo[0].Nombre} {grupo[1].Nombre}");

                class Alumno
                {
                    public string Nombre = "";
                }
                """);

        r.Code("n3.clases.guiado", s, ItemPurpose.Guided, CodeMode.Function,
            "Completa la clase Punto: agrega los campos públicos double X y double Y, y el método DistanciaAlOrigen() que devuelve √(X² + Y²) (usa Math.Sqrt).",
            """
            public class Punto
            {
                // campos X e Y

                public double DistanciaAlOrigen()
                {
                    return 0;
                }
            }
            """,
            """
            public class Punto
            {
                public double X;
                public double Y;

                public double DistanciaAlOrigen()
                {
                    return Math.Sqrt(X * X + Y * Y);
                }
            }
            """,
            [
                Is("3, 4", "{ var p = new Punto(); p.X = 3; p.Y = 4; return p.DistanciaAlOrigen(); }", "5"),
                Is("Origen", "new Punto().DistanciaAlOrigen()", "0", edge: true),
                Is("Negativos", "{ var p = new Punto(); p.X = -6; p.Y = -8; return p.DistanciaAlOrigen(); }", "10", hidden: true, edge: true),
            ],
            "Dentro de un método de instancia, X e Y se refieren a los campos del objeto sobre el que se llamó.",
            ["public double X; public double Y;", "return Math.Sqrt(X * X + Y * Y);"],
            requires: [CodeConstruct.Class]);

        r.Code("n3.clases.indep", s, ItemPurpose.Independent, CodeMode.Function,
            "Crea la clase Contador con un campo público int Valor y los métodos Incrementar() (suma 1), Reiniciar() (vuelve a 0) e IncrementarEn(int n) (suma n). Cada objeto debe llevar su propia cuenta.",
            """
            public class Contador
            {
            }
            """,
            """
            public class Contador
            {
                public int Valor;

                public void Incrementar() => Valor++;

                public void IncrementarEn(int n) => Valor += n;

                public void Reiniciar() => Valor = 0;
            }
            """,
            [
                Is("Incrementar", "{ var c = new Contador(); c.Incrementar(); c.Incrementar(); return c.Valor; }", "2"),
                Is("IncrementarEn", "{ var c = new Contador(); c.IncrementarEn(5); c.Incrementar(); return c.Valor; }", "6"),
                Is("Reiniciar", "{ var c = new Contador(); c.IncrementarEn(3); c.Reiniciar(); return c.Valor; }", "0", hidden: true),
                Is("Independientes", "{ var a = new Contador(); var b = new Contador(); a.Incrementar(); return $\"{a.Valor} {b.Valor}\"; }", "1 0", hidden: true, hint: "Valor no debe ser static: cada contador lleva su propia cuenta."),
            ],
            "Los métodos de instancia trabajan con el estado del objeto concreto sobre el que se llaman.",
            ["Declara public int Valor; dentro de la clase.", "Cada método modifica Valor; ninguno necesita devolver nada (void)."],
            requires: [CodeConstruct.Class]);

        r.Code("n3.clases.app", s, ItemPurpose.Transfer, CodeMode.Program,
            "Aplicación: un carrito de compras. Crea una clase Producto con campos públicos Nombre (string), Precio (decimal) y Cantidad (int), y un método Subtotal(). Lee líneas «nombre precio cantidad» hasta una línea vacía o el fin de la entrada. Muestra cada producto como «nombre x cantidad = subtotal» (subtotal con 2 decimales) y al final «Total: X» con 2 decimales.",
            """
            string? linea = Console.ReadLine();
            """,
            """
            List<Producto> carrito = [];
            string? linea = Console.ReadLine();
            while (!string.IsNullOrWhiteSpace(linea))
            {
                string[] p = linea.Split(' ');
                Producto prod = new Producto();
                prod.Nombre = p[0];
                prod.Precio = decimal.Parse(p[1]);
                prod.Cantidad = int.Parse(p[2]);
                carrito.Add(prod);
                linea = Console.ReadLine();
            }

            decimal total = 0;
            foreach (Producto prod in carrito)
            {
                Console.WriteLine($"{prod.Nombre} x {prod.Cantidad} = {prod.Subtotal():F2}");
                total += prod.Subtotal();
            }
            Console.WriteLine($"Total: {total:F2}");

            class Producto
            {
                public string Nombre = "";
                public decimal Precio;
                public int Cantidad;

                public decimal Subtotal() => Precio * Cantidad;
            }
            """,
            [
                In("Dos productos", "pan 2.5 4\nleche 18 1", "pan x 4 = 10.00\nleche x 1 = 18.00\nTotal: 28.00"),
                In("Vacío", "", "Total: 0.00", hidden: true, edge: true),
                In("Decimales", "chicle 0.35 3", "chicle x 3 = 1.05\nTotal: 1.05", hidden: true),
            ],
            "Agrupar los datos de un producto en un objeto evita mantener tres listas paralelas sincronizadas.",
            ["Declara la clase Producto al final del programa.", "Crea un objeto con new por cada línea y guárdalo en una List<Producto>."],
            difficulty: 2, requires: [CodeConstruct.Class, CodeConstruct.Loop], related: ["n2.listas"]);

        return new Skill(s, UnitId, "Clases e instancias",
            "Definir un tipo y crear objetos.",
            Tier.Essential, ["n2.parametros"],
            new Lesson(s,
                "Declarar clases con datos y comportamiento, crear objetos con new y entender que las variables de clase guardan referencias.",
                [
                    P("Una clase define un tipo nuevo que agrupa datos (campos) y comportamiento (métodos). Un objeto es una instancia concreta de la clase, creada con new."),
                    Code("""
                        Rectangulo r1 = new Rectangulo();
                        r1.Ancho = 3;
                        r1.Alto = 4;
                        Rectangulo r2 = new Rectangulo();
                        r2.Ancho = 10;
                        r2.Alto = 1;
                        Console.WriteLine(r1.Area());
                        Console.WriteLine(r2.Area());

                        class Rectangulo
                        {
                            public double Ancho;
                            public double Alto;

                            public double Area() => Ancho * Alto;
                        }
                        """, "Una clase, dos objetos independientes"),
                    P("Cada objeto tiene sus propios valores. Al llamar r1.Area(), el método usa el Ancho y el Alto de r1."),
                    P("Las clases son tipos por referencia: la variable guarda una referencia al objeto. Asignar una variable a otra no copia el objeto."),
                    Warn("Una variable de clase sin objeto vale null; usarla lanza NullReferenceException."),
                    Note("En un programa con instrucciones de nivel superior, las clases se declaran después de las instrucciones."),
                ],
                new WorkedExample("Un objeto que recuerda su estado",
                    """
                    Lampara sala = new Lampara();
                    Lampara cocina = new Lampara();
                    sala.Alternar();
                    sala.Alternar();
                    cocina.Alternar();
                    Console.WriteLine($"Sala: {sala.Estado()}, cocina: {cocina.Estado()}");
                    Console.WriteLine($"Cambios en la sala: {sala.Cambios}");

                    class Lampara
                    {
                        public bool Encendida;
                        public int Cambios;

                        public void Alternar()
                        {
                            Encendida = !Encendida;
                            Cambios++;
                        }

                        public string Estado() => Encendida ? "encendida" : "apagada";
                    }
                    """,
                    [
                        Line(1, "Dos objetos de la misma clase, cada uno con su estado."),
                        Line(4, "La sala se enciende y se apaga."),
                        Line(12, "Los campos numéricos empiezan en 0 y los bool en false."),
                        Line(16, "El método cambia el estado del objeto sobre el que se llama."),
                    ],
                    "Sala: apagada, cocina: encendida\nCambios en la sala: 2"),
                new CommonMistake(
                    "Declarar la variable pero no crear el objeto.",
                    "Lampara l;\nl.Alternar();",
                    "Declarar no crea nada: falta new. El compilador avisa que la variable no está asignada, y si valiera null se lanzaría NullReferenceException.",
                    "Lampara l = new Lampara();\nl.Alternar();\n\nclass Lampara\n{\n    public bool Encendida;\n    public void Alternar() => Encendida = !Encendida;\n}"),
                "n3.clases.pred1", "n3.clases.guiado", "n3.clases.indep", "n3.clases.app",
                ["n3.clases.recall1", "n3.clases.pred2", "n3.clases.concepto1"],
                "Modelas conceptos como clases con datos y métodos, creas instancias independientes y predices el efecto de compartir referencias.",
                15));
    }

    private static Skill Propiedades(ContentRegistry r)
    {
        const string s = "n3.propiedades";
        r.Short("n3.propiedades.pred1", s, ItemPurpose.Prediction,
            "¿Qué muestra este programa?",
            "Celsius es una propiedad normal. Fahrenheit es una propiedad calculada: no guarda nada y se recalcula cada vez que se lee a partir de Celsius.",
            ["212"], match: AnswerMatch.Number,
            code: """
                Temperatura t = new Temperatura();
                t.Celsius = 25;
                t.Celsius = 100;
                Console.WriteLine(t.Fahrenheit);

                class Temperatura
                {
                    public double Celsius { get; set; }
                    public double Fahrenheit => Celsius * 9 / 5 + 32;
                }
                """,
            wrong: new() { ["77"] = "Fahrenheit se calcula al leerla, con el valor actual de Celsius (100)." });

        r.Choice("n3.propiedades.concepto1", s, ItemPurpose.Concept,
            "¿Qué ventaja tiene una propiedad { get; set; } frente a un campo público?",
            "Desde fuera se usa igual, pero es un par de métodos: más adelante puedes agregar validación o cálculo sin cambiar el código que la usa. Además permite get público y set privado.",
            [
                Ok("Permite agregar validación o restringir el acceso sin cambiar el código que la usa.", "Correcto."),
                No("Es más rápida.", "La ventaja es el control, no la velocidad."),
                No("Ninguna, son idénticos.", "Un campo no puede validar ni tener set privado."),
            ],
            dimension: KnowledgeDimension.Design);

        r.Choice("n3.propiedades.recall1", s, ItemPurpose.Recall,
            "¿Qué significa «public string Nombre { get; private set; }»?",
            "Cualquiera puede leer Nombre, pero solo el código de la propia clase puede cambiarlo.",
            [
                Ok("Se puede leer desde fuera, pero solo la clase puede modificarla.", "Correcto."),
                No("Es privada: no se puede leer desde fuera.", "El get es público."),
                No("Solo se puede asignar una vez.", "Eso describe init o readonly."),
            ]);

        r.Choice("n3.propiedades.recall2", s, ItemPurpose.Recall,
            "¿Qué es una propiedad calculada como «public int Edad => DateTime.Today.Year - AnioNacimiento;»?",
            "Una propiedad de solo lectura sin campo propio: su valor se calcula cada vez que se lee.",
            [
                Ok("Una propiedad de solo lectura que se recalcula cada vez que se lee.", "Correcto."),
                No("Un campo que se calcula una vez al crear el objeto.", "Se recalcula en cada lectura."),
                No("Un método que hay que llamar con paréntesis.", "Se lee como una propiedad, sin paréntesis."),
            ]);

        r.Choice("n3.propiedades.bug1", s, ItemPurpose.BugFinding,
            "Este código termina con StackOverflowException al asignar Nombre. ¿Por qué?",
            "Dentro del set se asigna a la propia propiedad Nombre, que vuelve a llamar al set, infinitamente. Hay que guardar el valor en un campo privado (_nombre).",
            [
                Ok("El set se llama a sí mismo al asignar Nombre; debe usar un campo privado.", "Correcto."),
                No("Trim no se puede usar en una propiedad.", "Sí se puede.", ErrorCategory.ConceptMisunderstanding),
                No("Falta un get.", "Hay get; el problema es la recursión del set.", ErrorCategory.StackOverflowRecursion),
            ],
            code: """
                class Persona
                {
                    public string Nombre
                    {
                        get => Nombre;
                        set => Nombre = value.Trim();
                    }
                }
                """,
            targets: [ErrorCategory.StackOverflowRecursion]);

        r.Code("n3.propiedades.guiado", s, ItemPurpose.Guided, CodeMode.Function,
            "Completa la clase Rectangulo: Ancho y Alto son propiedades automáticas; Area y Perimetro deben ser propiedades calculadas (con =>).",
            """
            public class Rectangulo
            {
                public double Ancho { get; set; }
                public double Alto { get; set; }
                // Area y Perimetro
            }
            """,
            """
            public class Rectangulo
            {
                public double Ancho { get; set; }
                public double Alto { get; set; }
                public double Area => Ancho * Alto;
                public double Perimetro => 2 * (Ancho + Alto);
            }
            """,
            [
                Is("Área", "new Rectangulo { Ancho = 3, Alto = 4 }.Area", "12"),
                Is("Perímetro", "new Rectangulo { Ancho = 3, Alto = 4 }.Perimetro", "14"),
                Is("Se recalcula", "{ var r = new Rectangulo { Ancho = 1, Alto = 1 }; r.Ancho = 5; return r.Area; }", "5", hidden: true, hint: "Area debe calcularse al leerla, no guardarse al crear el objeto."),
            ],
            "Las propiedades calculadas nunca quedan desactualizadas porque no guardan valor.",
            ["public double Area => Ancho * Alto;", "Perimetro es 2 * (Ancho + Alto)."]);

        r.Code("n3.propiedades.indep", s, ItemPurpose.Independent, CodeMode.Function,
            "Crea la clase Producto con: Nombre (get; set;), Precio con validación (si se asigna un valor negativo, lanza ArgumentOutOfRangeException), Existencias (int, get público y set privado), y los métodos Agregar(int n) y Vender(int n) (si no hay suficientes existencias, lanza InvalidOperationException).",
            """
            public class Producto
            {
                public string Nombre { get; set; } = "";
                public decimal Precio { get; set; }
                public int Existencias { get; set; }
            }
            """,
            """
            public class Producto
            {
                private decimal _precio;

                public string Nombre { get; set; } = "";

                public decimal Precio
                {
                    get => _precio;
                    set
                    {
                        if (value < 0) throw new ArgumentOutOfRangeException(nameof(value), "El precio no puede ser negativo");
                        _precio = value;
                    }
                }

                public int Existencias { get; private set; }

                public void Agregar(int n) => Existencias += n;

                public void Vender(int n)
                {
                    if (n > Existencias) throw new InvalidOperationException("No hay suficientes existencias");
                    Existencias -= n;
                }
            }
            """,
            [
                Is("Precio", "new Producto { Precio = 9.5m }.Precio", "9.5"),
                Is("Agregar y vender", "{ var p = new Producto(); p.Agregar(10); p.Vender(3); return p.Existencias; }", "7"),
                Throws("Precio negativo", "new Producto { Precio = -1 }", "ArgumentOutOfRangeException", hidden: true),
                Throws("Sin existencias", "{ var p = new Producto(); p.Agregar(1); p.Vender(2); return p.Existencias; }", "InvalidOperationException", hidden: true),
                Is("Vender todo", "{ var p = new Producto(); p.Agregar(2); p.Vender(2); return p.Existencias; }", "0", hidden: true, edge: true),
            ],
            "La validación vive en el set: no hay manera de asignar un precio inválido desde fuera.",
            ["Usa un campo privado _precio y valida en el set antes de asignarlo.", "Existencias { get; private set; } impide cambiarla desde fuera; Vender comprueba antes de restar."],
            difficulty: 2, targets: [ErrorCategory.EdgeCaseMissed]);

        r.Code("n3.propiedades.app", s, ItemPurpose.Transfer, CodeMode.Function,
            "Aplicación: un termostato. Clase Termostato con: Objetivo (double) que solo acepta valores entre 10 y 30 (si no, lanza ArgumentOutOfRangeException); Actual (double, get; set;); Modo, propiedad calculada de tipo string: «Calentar» si Actual < Objetivo - 0.5, «Enfriar» si Actual > Objetivo + 0.5, y «Reposo» en otro caso. Objetivo empieza en 21.",
            """
            public class Termostato
            {
                public double Objetivo { get; set; }
                public double Actual { get; set; }
            }
            """,
            """
            public class Termostato
            {
                private double _objetivo = 21;

                public double Objetivo
                {
                    get => _objetivo;
                    set
                    {
                        if (value < 10 || value > 30) throw new ArgumentOutOfRangeException(nameof(value));
                        _objetivo = value;
                    }
                }

                public double Actual { get; set; }

                public string Modo =>
                    Actual < Objetivo - 0.5 ? "Calentar" :
                    Actual > Objetivo + 0.5 ? "Enfriar" :
                    "Reposo";
            }
            """,
            [
                Is("Frío", "new Termostato { Actual = 18 }.Modo", "Calentar"),
                Is("Calor", "new Termostato { Objetivo = 22, Actual = 25 }.Modo", "Enfriar"),
                Is("Margen", "new Termostato { Actual = 21.4 }.Modo", "Reposo", hidden: true, edge: true),
                Is("Valor inicial", "new Termostato().Objetivo", "21", hidden: true, edge: true),
                Throws("Fuera de rango", "new Termostato { Objetivo = 35 }", "ArgumentOutOfRangeException", hidden: true),
            ],
            "Las reglas del dispositivo quedan dentro de la clase: quien la usa no puede ponerla en un estado inválido.",
            ["Valida Objetivo en su set con un campo privado inicializado en 21.", "Modo es una propiedad calculada; un operador ternario encadenado lo resuelve."],
            difficulty: 3, related: ["n1.if-else"]);

        return new Skill(s, UnitId, "Campos, propiedades y métodos",
            "Estado y comportamiento de un objeto.",
            Tier.Essential, ["n3.clases"],
            new Lesson(s,
                "Usar propiedades automáticas, con validación y calculadas, distinguir campos de propiedades y controlar quién puede modificar el estado.",
                [
                    P("Un campo es una variable que pertenece al objeto. Una propiedad parece un campo desde fuera, pero por dentro son dos métodos: get (leer) y set (asignar)."),
                    Code("""
                        Cuenta c = new Cuenta { Titular = "Ana" };
                        c.Depositar(100);
                        Console.WriteLine($"{c.Titular}: {c.Saldo}");

                        class Cuenta
                        {
                            public string Titular { get; set; } = "";
                            public decimal Saldo { get; private set; }

                            public void Depositar(decimal monto) => Saldo += monto;
                        }
                        """, "Propiedad automática y set privado"),
                    P("Con un campo privado de respaldo puedes validar en el set. Con => creas una propiedad calculada de solo lectura."),
                    Fragment("""
                        private int _edad;
                        public int Edad
                        {
                            get => _edad;
                            set => _edad = value >= 0 ? value : throw new ArgumentOutOfRangeException(nameof(value));
                        }
                        public bool EsMayor => Edad >= 18;
                        """, "validación y propiedad calculada"),
                    P("La sintaxis new Cuenta { Titular = \"Ana\" } es un inicializador de objeto: asigna propiedades justo después de crear el objeto."),
                    Tip("Convención: propiedades y métodos en PascalCase; campos privados con guion bajo (_saldo)."),
                ],
                new WorkedExample("Una propiedad que se protege a sí misma",
                    """
                    Alumno a = new Alumno { Nombre = "Luis" };
                    a.Calificacion = 8.5;
                    Console.WriteLine($"{a.Nombre}: {a.Calificacion} ({a.Situacion})");
                    try
                    {
                        a.Calificacion = 12;
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        Console.WriteLine("Calificación rechazada");
                    }
                    Console.WriteLine(a.Calificacion);

                    class Alumno
                    {
                        private double _calificacion;
                        public string Nombre { get; set; } = "";
                        public double Calificacion
                        {
                            get => _calificacion;
                            set
                            {
                                if (value < 0 || value > 10) throw new ArgumentOutOfRangeException(nameof(value));
                                _calificacion = value;
                            }
                        }
                        public string Situacion => Calificacion >= 6 ? "aprobado" : "reprobado";
                    }
                    """,
                    [
                        Line(5, "Intentar un valor inválido lanza una excepción…"),
                        Line(11, "…y el valor anterior se conserva."),
                        Line(15, "Campo privado de respaldo."),
                        Line(22, "La validación ocurre antes de guardar."),
                        Line(26, "Situacion se calcula siempre a partir de la calificación actual."),
                    ],
                    "Luis: 8.5 (aprobado)\nCalificación rechazada\n8.5"),
                new CommonMistake(
                    "Asignar a la propia propiedad dentro de su set.",
                    "public int Edad\n{\n    get => Edad;\n    set => Edad = value;\n}",
                    "El set se llama a sí mismo sin fin y el programa cae con StackOverflowException.",
                    "class Persona\n{\n    private int _edad;\n    public int Edad\n    {\n        get => _edad;\n        set => _edad = value;\n    }\n}"),
                "n3.propiedades.pred1", "n3.propiedades.guiado", "n3.propiedades.indep", "n3.propiedades.app",
                ["n3.propiedades.recall1", "n3.propiedades.recall2", "n3.propiedades.concepto1"],
                "Expones el estado con propiedades adecuadas: validadas, calculadas o de solo lectura según el caso.",
                15));
    }

    private static Skill Constructores(ContentRegistry r)
    {
        const string s = "n3.constructores";
        r.Short("n3.constructores.pred1", s, ItemPurpose.Prediction,
            "¿Qué muestra este programa?",
            "El primer objeto usa el constructor de un parámetro, que llama con this(...) al de dos parámetros con 1 como cantidad. El segundo usa directamente el de dos parámetros.",
            ["café x1\ntaza x3"], match: AnswerMatch.Output,
            code: """
                Linea a = new Linea("café");
                Linea b = new Linea("taza", 3);
                Console.WriteLine($"{a.Producto} x{a.Cantidad}");
                Console.WriteLine($"{b.Producto} x{b.Cantidad}");

                class Linea
                {
                    public string Producto { get; }
                    public int Cantidad { get; }

                    public Linea(string producto) : this(producto, 1) { }

                    public Linea(string producto, int cantidad)
                    {
                        Producto = producto;
                        Cantidad = cantidad;
                    }
                }
                """,
            difficulty: 2);

        r.Choice("n3.constructores.concepto1", s, ItemPurpose.Concept,
            "¿Cuál es el propósito principal de un constructor?",
            "Garantizar que el objeto nace en un estado válido: recibe los datos obligatorios, los valida y los asigna. Así no existe ningún momento en que el objeto esté «a medias».",
            [
                Ok("Que el objeto empiece en un estado válido con todos sus datos obligatorios.", "Correcto."),
                No("Reservar memoria para el objeto.", "Eso lo hace new automáticamente."),
                No("Mostrar un mensaje cuando se crea el objeto.", "Podría hacerlo, pero no es su propósito."),
            ],
            dimension: KnowledgeDimension.Design);

        r.Choice("n3.constructores.recall1", s, ItemPurpose.Recall,
            "¿Cómo se reconoce un constructor en una clase?",
            "Tiene el mismo nombre que la clase y no declara tipo de retorno (ni siquiera void).",
            [
                Ok("Tiene el nombre de la clase y no tiene tipo de retorno.", "Correcto."),
                No("Se llama Init y devuelve void.", "En C# el constructor lleva el nombre de la clase."),
                No("Lleva la palabra constructor antes del nombre.", "Esa palabra no existe en C#."),
            ]);

        r.Choice("n3.constructores.recall2", s, ItemPurpose.Recall,
            "Si una clase declara un constructor con parámetros y ninguno sin parámetros, ¿se puede escribir new Clase()?",
            "No. El constructor sin parámetros implícito solo existe si la clase no declara ningún constructor.",
            [
                Ok("No: al declarar un constructor, desaparece el implícito sin parámetros.", "Correcto."),
                No("Sí, siempre existe un constructor vacío.", "Solo existe si no declaras ninguno."),
                No("Sí, pero los parámetros quedan en null.", "No compila."),
            ]);

        r.Choice("n3.constructores.bug1", s, ItemPurpose.BugFinding,
            "Tras new Usuario(\"ana\"), la propiedad Nombre queda vacía. ¿Por qué?",
            "En el constructor, «nombre = nombre» asigna el parámetro a sí mismo. Hay que asignar a la propiedad: Nombre = nombre (o this.Nombre = nombre).",
            [
                Ok("Se asigna el parámetro a sí mismo en lugar de a la propiedad.", "Correcto."),
                No("Los constructores no pueden asignar propiedades.", "Es justo su función.", ErrorCategory.ConceptMisunderstanding),
                No("Falta escribir return this.", "Los constructores no devuelven nada.", ErrorCategory.Syntax),
            ],
            code: """
                class Usuario
                {
                    public string Nombre { get; } = "";

                    public Usuario(string nombre)
                    {
                        nombre = nombre;
                    }
                }
                """,
            targets: [ErrorCategory.WrongLogic]);

        r.Code("n3.constructores.guiado", s, ItemPurpose.Guided, CodeMode.Function,
            "Completa el constructor de Fraccion: guarda numerador y denominador; si el denominador es 0, lanza ArgumentException.",
            """
            public class Fraccion
            {
                public int Numerador { get; }
                public int Denominador { get; }

                public Fraccion(int numerador, int denominador)
                {
                    // valida y asigna
                }

                public double Valor => (double)Numerador / Denominador;
            }
            """,
            """
            public class Fraccion
            {
                public int Numerador { get; }
                public int Denominador { get; }

                public Fraccion(int numerador, int denominador)
                {
                    if (denominador == 0)
                    {
                        throw new ArgumentException("El denominador no puede ser 0", nameof(denominador));
                    }
                    Numerador = numerador;
                    Denominador = denominador;
                }

                public double Valor => (double)Numerador / Denominador;
            }
            """,
            [
                Is("Un medio", "new Fraccion(1, 2).Valor", "0.5"),
                Is("Partes", "{ var f = new Fraccion(3, 4); return $\"{f.Numerador}/{f.Denominador}\"; }", "3/4"),
                Throws("Denominador cero", "new Fraccion(1, 0)", "ArgumentException", hidden: true),
            ],
            "Validar en el constructor garantiza que nunca existirá una fracción con denominador 0.",
            ["Comprueba denominador == 0 y lanza la excepción antes de asignar.", "Las propiedades { get; } solo pueden asignarse en el constructor."],
            targets: [ErrorCategory.EdgeCaseMissed]);

        r.Code("n3.constructores.indep", s, ItemPurpose.Independent, CodeMode.Function,
            "Crea la clase Hora con propiedades de solo lectura Horas y Minutos. Constructor Hora(int horas, int minutos) que valide 0–23 y 0–59 (si no, ArgumentOutOfRangeException). Un segundo constructor Hora(int totalMinutos) que llame al primero con this(...). Sobrescribe ToString() para devolver «HH:MM».",
            """
            public class Hora
            {
            }
            """,
            """
            public class Hora
            {
                public int Horas { get; }
                public int Minutos { get; }

                public Hora(int horas, int minutos)
                {
                    if (horas < 0 || horas > 23) throw new ArgumentOutOfRangeException(nameof(horas));
                    if (minutos < 0 || minutos > 59) throw new ArgumentOutOfRangeException(nameof(minutos));
                    Horas = horas;
                    Minutos = minutos;
                }

                public Hora(int totalMinutos) : this(totalMinutos / 60, totalMinutos % 60) { }

                public override string ToString() => $"{Horas:D2}:{Minutos:D2}";
            }
            """,
            [
                Is("Formato", "new Hora(9, 5).ToString()", "09:05"),
                Is("Desde minutos", "new Hora(135).ToString()", "02:15"),
                Throws("Hora inválida", "new Hora(24, 0)", "ArgumentOutOfRangeException", hidden: true),
                Throws("Minutos inválidos", "new Hora(10, 60)", "ArgumentOutOfRangeException", hidden: true),
                Throws("Demasiados minutos", "new Hora(1440)", "ArgumentOutOfRangeException", hidden: true, hint: "Si el segundo constructor delega en el primero, la validación se aplica sola."),
            ],
            "Encadenar constructores con this(...) deja la validación en un solo lugar.",
            ["El segundo constructor: public Hora(int totalMinutos) : this(totalMinutos / 60, totalMinutos % 60) { }", "public override string ToString() => $\"{Horas:D2}:{Minutos:D2}\";"],
            difficulty: 2, targets: [ErrorCategory.EdgeCaseMissed]);

        r.Code("n3.constructores.app", s, ItemPurpose.Transfer, CodeMode.Function,
            "Aplicación: reservas de un hotel. Clase Reserva con Huesped (string), Entrada y Salida (DateOnly) y Noches (calculada). El constructor exige un huésped no vacío (ArgumentException) y que la salida sea posterior a la entrada (ArgumentException). Agrega Costo(decimal tarifaPorNoche) que aplica 10 % de descuento a partir de 7 noches.",
            """
            public class Reserva
            {
                public Reserva(string huesped, DateOnly entrada, DateOnly salida)
                {
                }
            }
            """,
            """
            public class Reserva
            {
                public string Huesped { get; }
                public DateOnly Entrada { get; }
                public DateOnly Salida { get; }
                public int Noches => Salida.DayNumber - Entrada.DayNumber;

                public Reserva(string huesped, DateOnly entrada, DateOnly salida)
                {
                    if (string.IsNullOrWhiteSpace(huesped)) throw new ArgumentException("Falta el huésped", nameof(huesped));
                    if (salida <= entrada) throw new ArgumentException("La salida debe ser posterior a la entrada", nameof(salida));
                    Huesped = huesped.Trim();
                    Entrada = entrada;
                    Salida = salida;
                }

                public decimal Costo(decimal tarifaPorNoche)
                {
                    decimal total = tarifaPorNoche * Noches;
                    return Noches >= 7 ? total * 0.9m : total;
                }
            }
            """,
            [
                Is("Noches", "new Reserva(\"Ana\", new DateOnly(2025, 3, 1), new DateOnly(2025, 3, 4)).Noches", "3"),
                Is("Costo normal", "new Reserva(\"Ana\", new DateOnly(2025, 3, 1), new DateOnly(2025, 3, 4)).Costo(1000m)", "3000"),
                Is("Costo con descuento", "new Reserva(\"Luis\", new DateOnly(2025, 2, 25), new DateOnly(2025, 3, 4)).Costo(1000m)", "6300.0", hidden: true, hint: "Del 25 de febrero al 4 de marzo de 2025 hay 7 noches (2025 no es bisiesto)."),
                Throws("Fechas invertidas", "new Reserva(\"Eva\", new DateOnly(2025, 3, 4), new DateOnly(2025, 3, 4))", "ArgumentException", hidden: true, hint: "Una reserva de 0 noches tampoco es válida."),
                Throws("Sin huésped", "new Reserva(\"  \", new DateOnly(2025, 3, 1), new DateOnly(2025, 3, 2))", "ArgumentException", hidden: true),
            ],
            "Todas las reglas de una reserva válida se comprueban al crearla; el resto del programa puede confiar en ellas.",
            ["DayNumber convierte una fecha en un número de día: la resta da las noches.", "Valida con string.IsNullOrWhiteSpace y compara fechas con <=."],
            difficulty: 3, related: ["n0.variables"]);

        return new Skill(s, UnitId, "Constructores",
            "Crear objetos en un estado válido.",
            Tier.Essential, ["n3.propiedades"],
            new Lesson(s,
                "Escribir constructores que reciben y validan los datos obligatorios, encadenarlos con this(...) y combinar constructores con propiedades de solo lectura.",
                [
                    P("Un constructor es un método especial que se ejecuta al crear el objeto con new. Tiene el nombre de la clase y no tiene tipo de retorno."),
                    Code("""
                        Libro l = new Libro("Rayuela", "Julio Cortázar");
                        Console.WriteLine($"{l.Titulo} de {l.Autor}");

                        class Libro
                        {
                            public string Titulo { get; }
                            public string Autor { get; }

                            public Libro(string titulo, string autor)
                            {
                                Titulo = titulo;
                                Autor = autor;
                            }
                        }
                        """, "Datos obligatorios en el constructor"),
                    P("Las propiedades { get; } (sin set) solo pueden asignarse en el constructor: después el objeto no puede cambiarlas."),
                    P("Un constructor puede llamar a otro con : this(...) para no repetir código."),
                    Tip("C# 12 también permite constructores primarios: class Libro(string titulo, string autor) { ... }. Los verás en código moderno."),
                    Design("Si un objeto no tiene sentido sin cierto dato, pídelo en el constructor y valídalo ahí."),
                ],
                new WorkedExample("Una cuenta que no puede nacer inválida",
                    """
                    Cuenta c = new Cuenta("Ana", 500m);
                    Console.WriteLine($"{c.Titular}: {c.Saldo}");
                    Cuenta vacia = new Cuenta("Luis");
                    Console.WriteLine($"{vacia.Titular}: {vacia.Saldo}");
                    try
                    {
                        Cuenta mala = new Cuenta("", 10m);
                    }
                    catch (ArgumentException e)
                    {
                        Console.WriteLine(e.ParamName);
                    }

                    class Cuenta
                    {
                        public string Titular { get; }
                        public decimal Saldo { get; private set; }

                        public Cuenta(string titular) : this(titular, 0m) { }

                        public Cuenta(string titular, decimal saldoInicial)
                        {
                            if (string.IsNullOrWhiteSpace(titular)) throw new ArgumentException("Titular requerido", nameof(titular));
                            if (saldoInicial < 0) throw new ArgumentOutOfRangeException(nameof(saldoInicial));
                            Titular = titular;
                            Saldo = saldoInicial;
                        }
                    }
                    """,
                    [
                        Line(3, "El constructor de un parámetro delega en el completo."),
                        Line(6, "Un titular vacío se rechaza antes de crear el objeto."),
                        Line(10, "ParamName indica qué argumento era inválido."),
                        Line(19, ": this(titular, 0m) reutiliza la validación."),
                    ],
                    "Ana: 500\nLuis: 0\ntitular"),
                new CommonMistake(
                    "Asignar el parámetro a sí mismo en el constructor.",
                    "public Usuario(string nombre)\n{\n    nombre = nombre;\n}",
                    "Ambos nombres se refieren al parámetro; la propiedad no cambia.",
                    "class Usuario\n{\n    public string Nombre { get; }\n\n    public Usuario(string nombre)\n    {\n        Nombre = nombre;\n    }\n}"),
                "n3.constructores.pred1", "n3.constructores.guiado", "n3.constructores.indep", "n3.constructores.app",
                ["n3.constructores.recall1", "n3.constructores.recall2", "n3.constructores.concepto1"],
                "Diseñas constructores que garantizan objetos válidos desde su creación y sin duplicar validaciones.",
                15));
    }

    private static Skill Encapsulacion(ContentRegistry r)
    {
        const string s = "n3.encapsulacion";
        r.Choice("n3.encapsulacion.pred1", s, ItemPurpose.Prediction,
            "¿Qué ocurre con este programa?",
            "_saldo es private: solo el código dentro de Cuenta puede usarlo. Desde fuera, el compilador rechaza el acceso (CS0122).",
            [
                Ok("No compila: _saldo es privado.", "Correcto: el compilador protege el estado interno."),
                No("Muestra -1000", "El compilador no permite tocar el campo privado.", ErrorCategory.AccessModifier),
                No("Lanza una excepción al ejecutar", "El error se detecta al compilar, no al ejecutar.", ErrorCategory.AccessModifier),
            ],
            code: """
                Cuenta c = new Cuenta();
                c._saldo = -1000;
                Console.WriteLine(c.Saldo);

                class Cuenta
                {
                    private decimal _saldo;
                    public decimal Saldo => _saldo;
                }
                """,
            targets: [ErrorCategory.AccessModifier]);

        r.Choice("n3.encapsulacion.concepto1", s, ItemPurpose.Concept,
            "¿Qué es un invariante de una clase?",
            "Una regla que siempre debe cumplirse para los objetos de esa clase (por ejemplo, «el saldo nunca es negativo»). La encapsulación existe para que ningún código externo pueda romperla.",
            [
                Ok("Una regla que el objeto debe cumplir siempre, como que el saldo nunca sea negativo.", "Correcto."),
                No("Una variable que no cambia nunca.", "Eso sería una constante; un invariante es una regla."),
                No("Un método que no recibe parámetros.", "No tiene relación."),
            ],
            dimension: KnowledgeDimension.Design);

        r.Choice("n3.encapsulacion.recall1", s, ItemPurpose.Recall,
            "Relaciona: ¿quién puede acceder a un miembro protected?",
            "private: solo la propia clase. protected: la clase y sus clases derivadas. internal: todo el código del mismo ensamblado (proyecto). public: cualquiera.",
            [
                Ok("La propia clase y las clases que heredan de ella.", "Correcto."),
                No("Todo el código del mismo proyecto.", "Eso es internal."),
                No("Solo la propia clase.", "Eso es private."),
            ]);

        r.Choice("n3.encapsulacion.recall2", s, ItemPurpose.Recall,
            "Si no escribes ningún modificador de acceso en un campo de una clase, ¿qué acceso tiene?",
            "Los miembros de una clase son private por defecto.",
            [
                Ok("private", "Correcto."),
                No("public", "Habría que escribirlo explícitamente."),
                No("internal", "internal es el valor por defecto de las clases de nivel superior, no de sus miembros."),
            ]);

        r.Choice("n3.encapsulacion.bug1", s, ItemPurpose.BugFinding,
            "La clase Pila expone su lista. ¿Qué problema de diseño tiene?",
            "Cualquier código puede hacer pila.Elementos.Clear() o insertar en cualquier posición, saltándose las reglas de una pila. Expón solo operaciones (Apilar, Desapilar, Cantidad) y mantén la lista privada.",
            [
                Ok("Al exponer la lista, cualquiera puede modificarla sin pasar por Apilar y Desapilar.", "Correcto."),
                No("Una pila no puede usar List<T> internamente.", "Puede; el problema es exponerla.", ErrorCategory.ConceptMisunderstanding),
                No("El get debería ser private set.", "private set impide reasignar la lista, pero no modificar su contenido.", ErrorCategory.ConceptMisunderstanding),
            ],
            code: """
                class Pila
                {
                    public List<int> Elementos { get; } = [];
                    public void Apilar(int x) => Elementos.Add(x);
                    public int Desapilar()
                    {
                        int x = Elementos[^1];
                        Elementos.RemoveAt(Elementos.Count - 1);
                        return x;
                    }
                }
                """,
            targets: [ErrorCategory.AccessModifier]);

        r.Code("n3.encapsulacion.guiado", s, ItemPurpose.Guided, CodeMode.Function,
            "La clase Pila expone su lista. Hazla privada (campo _elementos) y ofrece: Apilar(int), Desapilar() (lanza InvalidOperationException si está vacía), Cima (propiedad, mismo error si está vacía) y Cantidad.",
            """
            public class Pila
            {
                public List<int> Elementos = [];

                public void Apilar(int x) => Elementos.Add(x);
            }
            """,
            """
            public class Pila
            {
                private readonly List<int> _elementos = [];

                public int Cantidad => _elementos.Count;

                public int Cima => _elementos.Count > 0 ? _elementos[^1] : throw new InvalidOperationException("La pila está vacía");

                public void Apilar(int x) => _elementos.Add(x);

                public int Desapilar()
                {
                    int x = Cima;
                    _elementos.RemoveAt(_elementos.Count - 1);
                    return x;
                }
            }
            """,
            [
                Is("Orden LIFO", "{ var p = new Pila(); p.Apilar(1); p.Apilar(2); p.Apilar(3); return $\"{p.Desapilar()}{p.Desapilar()}{p.Cantidad}\"; }", "321"),
                Is("Cima no quita", "{ var p = new Pila(); p.Apilar(7); var c = p.Cima; return $\"{c} {p.Cantidad}\"; }", "7 1"),
                Throws("Vacía", "new Pila().Desapilar()", "InvalidOperationException", hidden: true, hint: "Comprueba la cantidad antes de quitar."),
                Throws("Cima vacía", "new Pila().Cima", "InvalidOperationException", hidden: true),
            ],
            "Al ocultar la lista, la única forma de modificar la pila es a través de sus reglas.",
            ["private readonly List<int> _elementos = [];", "_elementos[^1] es el último elemento; ^1 significa «el primero desde el final»."],
            requires: [CodeConstruct.Class], targets: [ErrorCategory.AccessModifier]);

        r.Code("n3.encapsulacion.indep", s, ItemPurpose.Independent, CodeMode.Function,
            "Crea CuentaBancaria con: constructor (titular), Saldo de solo lectura desde fuera, Depositar(decimal) y Retirar(decimal) que rechacen montos <= 0 (ArgumentOutOfRangeException) y Retirar que no permita saldo negativo (InvalidOperationException). Agrega Movimientos: una IReadOnlyList<string> con «+monto» o «-monto» por cada operación exitosa.",
            """
            public class CuentaBancaria
            {
                public decimal Saldo;
            }
            """,
            """
            public class CuentaBancaria
            {
                private readonly List<string> _movimientos = [];

                public string Titular { get; }
                public decimal Saldo { get; private set; }
                public IReadOnlyList<string> Movimientos => _movimientos;

                public CuentaBancaria(string titular) => Titular = titular;

                public void Depositar(decimal monto)
                {
                    if (monto <= 0) throw new ArgumentOutOfRangeException(nameof(monto));
                    Saldo += monto;
                    _movimientos.Add($"+{monto}");
                }

                public void Retirar(decimal monto)
                {
                    if (monto <= 0) throw new ArgumentOutOfRangeException(nameof(monto));
                    if (monto > Saldo) throw new InvalidOperationException("Saldo insuficiente");
                    Saldo -= monto;
                    _movimientos.Add($"-{monto}");
                }
            }
            """,
            [
                Is("Depósito y retiro", "{ var c = new CuentaBancaria(\"Ana\"); c.Depositar(100); c.Retirar(30); return c.Saldo; }", "70"),
                Is("Movimientos", "{ var c = new CuentaBancaria(\"Ana\"); c.Depositar(50); c.Retirar(20); return c.Movimientos; }", "[\"+50\", \"-20\"]"),
                Throws("Sin saldo", "{ var c = new CuentaBancaria(\"Ana\"); c.Retirar(1); return 0; }", "InvalidOperationException", hidden: true),
                Throws("Monto cero", "{ var c = new CuentaBancaria(\"Ana\"); c.Depositar(0); return 0; }", "ArgumentOutOfRangeException", hidden: true),
                Is("Fallo no registra", "{ var c = new CuentaBancaria(\"Ana\"); c.Depositar(10); try { c.Retirar(50); } catch (InvalidOperationException) { } return $\"{c.Saldo} {c.Movimientos.Count}\"; }", "10 1", hidden: true, edge: true, hint: "Valida antes de modificar nada: una operación rechazada no debe dejar rastro."),
            ],
            "Validar antes de modificar mantiene el objeto consistente incluso cuando una operación falla.",
            ["Saldo { get; private set; } y una lista privada de movimientos.", "Expón la lista como IReadOnlyList<string> para que no se pueda modificar desde fuera."],
            difficulty: 2, requires: [CodeConstruct.Class], targets: [ErrorCategory.EdgeCaseMissed]);

        r.Code("n3.encapsulacion.app", s, ItemPurpose.Transfer, CodeMode.Function,
            "Aplicación: control de aforo de un evento. Clase Evento(int capacidad) con: Registrar(string correo) que devuelve true si lo registró y false si el correo ya estaba (sin distinguir mayúsculas) o si el cupo está lleno; Cancelar(string correo) que libera el lugar y devuelve si existía; Ocupados y Disponibles. La capacidad debe ser mayor que 0 (ArgumentOutOfRangeException).",
            """
            public class Evento
            {
                public List<string> Asistentes = [];
            }
            """,
            """
            public class Evento
            {
                private readonly HashSet<string> _asistentes = [];
                private readonly int _capacidad;

                public Evento(int capacidad)
                {
                    if (capacidad <= 0) throw new ArgumentOutOfRangeException(nameof(capacidad));
                    _capacidad = capacidad;
                }

                public int Ocupados => _asistentes.Count;
                public int Disponibles => _capacidad - _asistentes.Count;

                public bool Registrar(string correo)
                {
                    if (Disponibles == 0) return false;
                    return _asistentes.Add(Normalizar(correo));
                }

                public bool Cancelar(string correo) => _asistentes.Remove(Normalizar(correo));

                private static string Normalizar(string correo) => correo.Trim().ToLowerInvariant();
            }
            """,
            [
                Is("Registrar", "{ var e = new Evento(2); e.Registrar(\"a@x.com\"); return $\"{e.Ocupados} {e.Disponibles}\"; }", "1 1"),
                Is("Duplicado", "{ var e = new Evento(5); e.Registrar(\"a@x.com\"); return e.Registrar(\"A@X.com\"); }", "false"),
                Is("Lleno", "{ var e = new Evento(1); e.Registrar(\"a@x.com\"); return e.Registrar(\"b@x.com\"); }", "false", hidden: true, edge: true),
                Is("Cancelar libera", "{ var e = new Evento(1); e.Registrar(\"a@x.com\"); e.Cancelar(\"a@x.com\"); return e.Registrar(\"b@x.com\"); }", "true", hidden: true),
                Throws("Capacidad cero", "new Evento(0)", "ArgumentOutOfRangeException", hidden: true),
            ],
            "Los detalles internos (HashSet, normalización) pueden cambiar sin afectar a quien usa Evento, porque solo ve sus operaciones públicas.",
            ["Guarda los correos en un HashSet privado, normalizados con ToLowerInvariant.", "Un método private static Normalizar evita repetir la normalización."],
            difficulty: 3, requires: [CodeConstruct.Class], related: ["n2.diccionarios"]);

        return new Skill(s, UnitId, "Encapsulación y modificadores de acceso",
            "Proteger invariantes con private, public, protected e internal.",
            Tier.Essential, ["n3.constructores"],
            new Lesson(s,
                "Ocultar el estado interno, exponer operaciones que respetan las reglas de la clase y elegir el modificador de acceso adecuado.",
                [
                    P("Encapsular es ocultar cómo está hecho un objeto y exponer solo lo que se puede hacer con él. Así ningún código externo puede dejarlo en un estado inválido."),
                    P("Modificadores de acceso: private (solo la clase), protected (la clase y sus derivadas), internal (el mismo proyecto), public (cualquiera). Los miembros son private por defecto."),
                    Code("""
                        Termometro t = new Termometro();
                        t.Registrar(18.5);
                        t.Registrar(22.1);
                        Console.WriteLine($"{t.Maxima} {t.Lecturas}");

                        class Termometro
                        {
                            private readonly List<double> _lecturas = [];

                            public int Lecturas => _lecturas.Count;
                            public double Maxima => _lecturas.Max();

                            public void Registrar(double grados) => _lecturas.Add(grados);
                        }
                        """, "El estado es privado; las operaciones, públicas"),
                    Warn("Exponer una List<T> pública permite que cualquiera la modifique. Para dar acceso de lectura, exponla como IReadOnlyList<T>."),
                    Design("Empieza con todo private y abre solo lo necesario. Es fácil hacer público algo privado; lo contrario rompe a quien ya lo usaba."),
                ],
                new WorkedExample("Un inventario que protege sus reglas",
                    """
                    Inventario inv = new Inventario();
                    inv.Entrada("tornillo", 100);
                    inv.Salida("tornillo", 30);
                    Console.WriteLine(inv.Cantidad("tornillo"));
                    Console.WriteLine(inv.Salida("tornillo", 500));
                    Console.WriteLine(inv.Cantidad("tuerca"));

                    class Inventario
                    {
                        private readonly Dictionary<string, int> _stock = [];

                        public int Cantidad(string producto) => _stock.GetValueOrDefault(producto);

                        public void Entrada(string producto, int n) => _stock[producto] = Cantidad(producto) + n;

                        public bool Salida(string producto, int n)
                        {
                            if (n > Cantidad(producto)) return false;
                            _stock[producto] = Cantidad(producto) - n;
                            return true;
                        }
                    }
                    """,
                    [
                        Line(5, "Una salida imposible se rechaza; el stock no cambia."),
                        Line(10, "El diccionario es privado: nadie puede poner cantidades negativas."),
                        Line(18, "Toda modificación pasa por un método que comprueba la regla."),
                    ],
                    "70\nFalse\n0"),
                new CommonMistake(
                    "Hacer públicos los campos para «ahorrar código».",
                    "class Cuenta\n{\n    public decimal Saldo;\n}\n// en cualquier parte: cuenta.Saldo = -500;",
                    "Cualquier línea del programa puede romper la regla «el saldo no es negativo» y el error aparecerá lejos de su causa.",
                    "class Cuenta\n{\n    public decimal Saldo { get; private set; }\n\n    public void Retirar(decimal monto)\n    {\n        if (monto > Saldo) throw new InvalidOperationException(\"Saldo insuficiente\");\n        Saldo -= monto;\n    }\n}"),
                "n3.encapsulacion.pred1", "n3.encapsulacion.guiado", "n3.encapsulacion.indep", "n3.encapsulacion.app",
                ["n3.encapsulacion.recall1", "n3.encapsulacion.recall2", "n3.encapsulacion.concepto1"],
                "Diseñas clases cuyos invariantes no pueden romperse desde fuera y eliges modificadores de acceso con criterio.",
                18));
    }

    private static Skill Static(ContentRegistry r)
    {
        const string s = "n3.static";
        r.Short("n3.static.pred1", s, ItemPurpose.Prediction,
            "¿Qué muestra este programa?",
            "Creados es static: hay una sola copia compartida por todos los objetos, y cada constructor la incrementa. Numero es de instancia: cada ticket tiene el suyo.",
            ["3 2"], match: AnswerMatch.Output,
            code: """
                Ticket a = new Ticket();
                Ticket b = new Ticket();
                Ticket c = new Ticket();
                Console.WriteLine($"{Ticket.Creados} {b.Numero}");

                class Ticket
                {
                    public static int Creados;
                    public int Numero { get; }

                    public Ticket()
                    {
                        Creados++;
                        Numero = Creados;
                    }
                }
                """,
            difficulty: 2);

        r.Choice("n3.static.concepto1", s, ItemPurpose.Concept,
            "¿Por qué un método static no puede usar directamente un campo de instancia?",
            "Un método static pertenece al tipo, no a un objeto: se llama como Clase.Metodo(), sin ningún objeto. No hay un «this» del cual leer el campo.",
            [
                Ok("Porque no se ejecuta sobre ningún objeto: no hay this del cual leer el campo.", "Correcto."),
                No("Porque los campos de instancia son privados.", "El acceso no tiene que ver con private o public.", ErrorCategory.StaticVersusInstance),
                No("Sí puede, siempre que el campo sea public.", "Ni siquiera siendo public: necesita un objeto concreto.", ErrorCategory.StaticVersusInstance),
            ],
            targets: [ErrorCategory.StaticVersusInstance]);

        r.Choice("n3.static.recall1", s, ItemPurpose.Recall,
            "¿Cómo se llama a un método static Convertir de la clase Moneda?",
            "A través del nombre del tipo: Moneda.Convertir(...). No hace falta crear un objeto.",
            [
                Ok("Moneda.Convertir(...)", "Correcto."),
                No("new Moneda().Convertir(...)", "Un miembro static no se usa a través de una instancia.", ErrorCategory.StaticVersusInstance),
                No("Convertir.Moneda(...)", "Primero el tipo, luego el miembro."),
            ]);

        r.Choice("n3.static.recall2", s, ItemPurpose.Recall,
            "¿Qué es una static class, como Math?",
            "Una clase que solo contiene miembros static y de la que no se pueden crear objetos. Sirve para agrupar funciones utilitarias.",
            [
                Ok("Una clase que solo tiene miembros static y no se puede instanciar.", "Correcto."),
                No("Una clase cuyos objetos no pueden cambiar.", "Eso describe un tipo inmutable."),
                No("Una clase que solo puede tener un objeto.", "Eso es el patrón Singleton."),
            ]);

        r.Choice("n3.static.bug1", s, ItemPurpose.BugFinding,
            "Cada jugador debería tener su propio puntaje, pero al sumar puntos a uno, todos cambian. ¿Por qué?",
            "Puntos es static, así que hay un solo valor compartido por todos los jugadores. Debe ser un miembro de instancia.",
            [
                Ok("Puntos es static: es un único valor compartido por todos.", "Correcto: quita static."),
                No("Falta un constructor.", "El problema es dónde vive el dato.", ErrorCategory.StaticVersusInstance),
                No("Sumar debería ser static.", "Haría el problema todavía más evidente, no lo resolvería.", ErrorCategory.StaticVersusInstance),
            ],
            code: """
                class Jugador
                {
                    public string Nombre { get; set; } = "";
                    public static int Puntos;

                    public void Sumar(int p) => Puntos += p;
                }
                """,
            targets: [ErrorCategory.StaticVersusInstance]);

        r.Code("n3.static.guiado", s, ItemPurpose.Guided, CodeMode.Function,
            "Completa la clase estática Conversor con los métodos CelsiusAFahrenheit(double c) y KmAMillas(double km) (1 milla = 1.609344 km). Redondea ambos resultados a 2 decimales con Math.Round.",
            """
            public static class Conversor
            {
                public static double CelsiusAFahrenheit(double c) => 0;
            }
            """,
            """
            public static class Conversor
            {
                public static double CelsiusAFahrenheit(double c) => Math.Round(c * 9 / 5 + 32, 2);

                public static double KmAMillas(double km) => Math.Round(km / 1.609344, 2);
            }
            """,
            [
                Is("Hervor", "Conversor.CelsiusAFahrenheit(100)", "212"),
                Is("Maratón", "Conversor.KmAMillas(42.195)", "26.22"),
                Is("Bajo cero", "Conversor.CelsiusAFahrenheit(-40)", "-40", hidden: true, edge: true),
            ],
            "Una conversión no necesita estado: es una función pura y encaja como miembro static.",
            ["Ambos métodos llevan public static.", "Math.Round(valor, 2) redondea a 2 decimales."]);

        r.Code("n3.static.indep", s, ItemPurpose.Independent, CodeMode.Function,
            "Crea la clase Pedido con un folio automático: cada pedido nuevo recibe Folio = «P-0001», «P-0002»… según el orden de creación. Usa un contador static privado. Agrega el método static Reiniciar() que vuelve el contador a 0 (útil para pruebas).",
            """
            public class Pedido
            {
                public string Folio { get; } = "";
            }
            """,
            """
            public class Pedido
            {
                private static int _siguiente;

                public string Folio { get; }

                public Pedido()
                {
                    _siguiente++;
                    Folio = $"P-{_siguiente:D4}";
                }

                public static void Reiniciar() => _siguiente = 0;
            }
            """,
            [
                Is("Primero", "{ Pedido.Reiniciar(); return new Pedido().Folio; }", "P-0001"),
                Is("Consecutivos", "{ Pedido.Reiniciar(); new Pedido(); new Pedido(); return new Pedido().Folio; }", "P-0003"),
                Is("Cada uno conserva el suyo", "{ Pedido.Reiniciar(); var a = new Pedido(); var b = new Pedido(); return $\"{a.Folio} {b.Folio}\"; }", "P-0001 P-0002", hidden: true, hint: "Folio es de instancia; solo el contador es static."),
            ],
            "El contador es del tipo (uno para todos); el folio es de cada objeto.",
            ["private static int _siguiente;", "En el constructor incrementa _siguiente y arma el folio con :D4."],
            difficulty: 2, targets: [ErrorCategory.StaticVersusInstance]);

        r.Code("n3.static.app", s, ItemPurpose.Transfer, CodeMode.Function,
            "Aplicación: validación de un formulario de contacto. Crea la clase estática Validador con: EsCorreo(string) (tiene exactamente una @, algo antes y un punto después de la @ que no esté al final); EsTelefono(string) (exactamente 10 dígitos, se permiten espacios y guiones que se ignoran); y Errores(string correo, string telefono) que devuelve una List<string> con «correo» y/o «teléfono» según lo que sea inválido.",
            """
            public static class Validador
            {
            }
            """,
            """
            public static class Validador
            {
                public static bool EsCorreo(string correo)
                {
                    int arroba = correo.IndexOf('@');
                    if (arroba <= 0 || arroba != correo.LastIndexOf('@')) return false;
                    int punto = correo.IndexOf('.', arroba + 2);
                    return punto > 0 && punto < correo.Length - 1;
                }

                public static bool EsTelefono(string telefono)
                {
                    int digitos = 0;
                    foreach (char c in telefono)
                    {
                        if (char.IsDigit(c)) digitos++;
                        else if (c != ' ' && c != '-') return false;
                    }
                    return digitos == 10;
                }

                public static List<string> Errores(string correo, string telefono)
                {
                    List<string> errores = [];
                    if (!EsCorreo(correo)) errores.Add("correo");
                    if (!EsTelefono(telefono)) errores.Add("teléfono");
                    return errores;
                }
            }
            """,
            [
                Is("Correo válido", "Validador.EsCorreo(\"ana@mail.com\")", "true"),
                Is("Teléfono con guiones", "Validador.EsTelefono(\"55-1234-5678\")", "true"),
                Is("Dos arrobas", "Validador.EsCorreo(\"a@b@c.com\")", "false", hidden: true, edge: true),
                Is("Punto al final", "Validador.EsCorreo(\"ana@mail.\")", "false", hidden: true, edge: true),
                Is("Sin usuario", "Validador.EsCorreo(\"@mail.com\")", "false", hidden: true, edge: true),
                Is("Letras", "Validador.EsTelefono(\"55 1234 567a\")", "false", hidden: true),
                Is("Errores", "Validador.Errores(\"malo\", \"123\")", "[\"correo\", \"teléfono\"]"),
            ],
            "Las validaciones no dependen de ningún objeto, así que un tipo static con funciones puras es apropiado y fácil de probar.",
            ["IndexOf y LastIndexOf de '@' deben coincidir para que haya solo una.", "Recorre el teléfono contando dígitos y rechaza cualquier carácter que no sea dígito, espacio o guion."],
            difficulty: 3, requires: [CodeConstruct.Loop], related: ["n2.strings"]);

        return new Skill(s, UnitId, "Miembros estáticos y de instancia",
            "Qué pertenece al tipo y qué a cada objeto.",
            Tier.Essential, ["n3.propiedades"],
            new Lesson(s,
                "Distinguir miembros static (del tipo) de miembros de instancia (de cada objeto) y decidir cuándo usar cada uno.",
                [
                    P("Un miembro de instancia pertenece a cada objeto: cada uno tiene su copia. Un miembro static pertenece al tipo: hay una sola copia compartida y se usa con el nombre de la clase."),
                    Code("""
                        Console.WriteLine(Math.Max(3, 9));
                        Console.WriteLine(int.Parse("42") + 1);
                        Console.WriteLine(Circulo.Area(2));

                        static class Circulo
                        {
                            public static double Area(double radio) => Math.Round(Math.PI * radio * radio, 2);
                        }
                        """, "Miembros static: se usan con el nombre del tipo"),
                    P("Un método static no tiene this: no puede usar campos ni propiedades de instancia sin recibir un objeto como parámetro."),
                    P("static class declara una clase que solo tiene miembros static y no se puede instanciar, como Math o Console."),
                    Warn("Un campo static mutable es estado global compartido: cualquier parte del programa puede cambiarlo y es fuente de errores difíciles de rastrear. Úsalo con moderación."),
                ],
                new WorkedExample("Fábrica static y datos de instancia",
                    """
                    Color rojo = Color.DesdeHex("#FF0000");
                    Color gris = Color.DesdeHex("#808080");
                    Console.WriteLine(rojo.Brillo());
                    Console.WriteLine(gris.Brillo());
                    Console.WriteLine(Color.Creados);

                    class Color
                    {
                        public static int Creados { get; private set; }
                        public int R { get; }
                        public int G { get; }
                        public int B { get; }

                        private Color(int r, int g, int b)
                        {
                            R = r;
                            G = g;
                            B = b;
                            Creados++;
                        }

                        public static Color DesdeHex(string hex) => new Color(
                            Convert.ToInt32(hex.Substring(1, 2), 16),
                            Convert.ToInt32(hex.Substring(3, 2), 16),
                            Convert.ToInt32(hex.Substring(5, 2), 16));

                        public int Brillo() => (R + G + B) / 3;
                    }
                    """,
                    [
                        Line(1, "Se crea el objeto a través de un método static (fábrica)."),
                        Line(9, "Creados es del tipo: cuenta todos los colores."),
                        Line(14, "Constructor privado: la única forma de crear un Color es DesdeHex."),
                        Line(28, "Brillo es de instancia: usa los valores de cada color."),
                    ],
                    "85\n128\n2"),
                new CommonMistake(
                    "Usar un campo de instancia desde un método static.",
                    "class Contador\n{\n    public int Valor;\n    public static void Incrementar() => Valor++;\n}",
                    "No compila (CS0120): el método static no sabe de qué contador es el Valor.",
                    "class Contador\n{\n    public int Valor;\n    public void Incrementar() => Valor++;\n}"),
                "n3.static.pred1", "n3.static.guiado", "n3.static.indep", "n3.static.app",
                ["n3.static.recall1", "n3.static.recall2", "n3.static.concepto1"],
                "Decides con criterio qué pertenece al tipo y qué a cada objeto, y evitas estado global innecesario.",
                15));
    }
}
