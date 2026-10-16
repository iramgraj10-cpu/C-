using CSharpMastery.Content.Infrastructure;
using CSharpMastery.Domain.Curriculum;
using CSharpMastery.Domain.Items;
using CSharpMastery.Domain.Progress;
using static CSharpMastery.Content.Infrastructure.ContentRegistry;
using static CSharpMastery.Content.Infrastructure.L;

namespace CSharpMastery.Content.Nivel1;

internal static class U03Problemas
{
    public const string UnitId = "n1.u3";

    public static Unit Build(ContentRegistry r) => new(UnitId, "n1", "Resolver problemas",
        "Validación, descomposición y costo de los algoritmos.",
        [Validacion(r), Descomposicion(r), Complejidad(r)]);

    private static Skill Validacion(ContentRegistry r)
    {
        const string s = "n1.validacion";
        r.Choice("n1.validacion.pred1", s, ItemPurpose.Prediction,
            "¿Qué muestra este programa?",
            "\"abc\" y \"-4\" no cumplen (el primero no es número; el segundo es negativo), así que se rechazan. \"7\" es el primer valor válido.",
            [
                Ok("Inválido\nInválido\nAceptado: 7", "Correcto: se repite hasta obtener un valor válido."),
                No("Aceptado: -4", "La condición exige un número mayor que 0.", ErrorCategory.WrongLogic),
                No("Se lanza FormatException", "TryParse no lanza excepción con texto inválido.", ErrorCategory.ParsingFormat),
            ],
            code: """
                string[] entradas = ["abc", "-4", "7"];
                int i = 0;
                int valor;
                while (!int.TryParse(entradas[i], out valor) || valor <= 0)
                {
                    Console.WriteLine("Inválido");
                    i++;
                }
                Console.WriteLine($"Aceptado: {valor}");
                """,
            difficulty: 2);

        r.Choice("n1.validacion.concepto1", s, ItemPurpose.Concept,
            "¿Por qué se prefiere int.TryParse a int.Parse para leer datos que escribe una persona?",
            "Las personas se equivocan al escribir. TryParse permite detectar el error y volver a pedir el dato, sin que el programa termine con una excepción.",
            [
                Ok("Porque permite detectar una entrada inválida y volver a pedirla sin que el programa falle.", "Correcto."),
                No("Porque es más rápido.", "La diferencia importante es el manejo de errores."),
                No("Porque acepta letras y las convierte en números.", "No convierte letras: informa que la conversión no fue posible.", ErrorCategory.ParsingFormat),
            ],
            targets: [ErrorCategory.ParsingFormat]);

        r.Choice("n1.validacion.recall1", s, ItemPurpose.Recall,
            "¿Qué devuelve int.TryParse y dónde deja el número convertido?",
            "Devuelve un bool (true si pudo convertir) y deja el número en la variable indicada con out.",
            [
                Ok("Devuelve true o false, y el número queda en el parámetro out.", "Correcto."),
                No("Devuelve el número o null si falla.", "Eso describiría otro diseño; TryParse devuelve bool."),
                No("Devuelve el número o lanza FormatException.", "Eso es int.Parse."),
            ]);

        r.Choice("n1.validacion.recall2", s, ItemPurpose.Recall,
            "Además del formato, ¿qué más suele hacer falta validar en un dato numérico?",
            "Que esté dentro del rango con sentido para el problema (una edad no puede ser negativa, un mes va de 1 a 12).",
            [
                Ok("Que el valor esté en un rango válido para el problema.", "Correcto."),
                No("Nada: si se puede convertir, ya es válido.", "\"-5\" se convierte, pero no es una edad válida.", ErrorCategory.EdgeCaseMissed),
                No("Que tenga exactamente dos dígitos.", "Depende del problema; la regla general es el rango con sentido."),
            ]);

        r.Choice("n1.validacion.bug1", s, ItemPurpose.BugFinding,
            "El programa debe pedir una edad entre 0 y 120 hasta que sea válida, pero acepta 150. ¿Por qué?",
            "La condición del bucle solo comprueba el formato. Si TryParse tiene éxito, el bucle termina aunque el valor esté fuera de rango. Debe ser: !int.TryParse(...) || edad < 0 || edad > 120.",
            [
                Ok("La condición del bucle no comprueba el rango, solo que sea número.", "Correcto."),
                No("TryParse no acepta números mayores que 100.", "Acepta cualquier número que quepa en int.", ErrorCategory.ParsingFormat),
                No("Falta un break dentro del bucle.", "El problema es qué condición mantiene el bucle.", ErrorCategory.WrongLogic),
            ],
            code: """
                string[] entradas = ["150"];
                int i = 0;
                int edad;
                while (!int.TryParse(entradas[i], out edad))
                {
                    i++;
                }
                Console.WriteLine($"Edad: {edad}");
                """,
            targets: [ErrorCategory.EdgeCaseMissed]);

        r.Code("n1.validacion.guiado", s, ItemPurpose.Guided, CodeMode.Program,
            "Completa la validación: lee líneas hasta obtener un número entero entre 1 y 10. Por cada entrada inválida muestra «Intenta de nuevo». Al final muestra «Elegiste n».",
            """
            int numero;
            string linea = Console.ReadLine()!;
            while (!int.TryParse(linea, out numero)) // falta validar el rango
            {
                Console.WriteLine("Intenta de nuevo");
                linea = Console.ReadLine()!;
            }
            Console.WriteLine($"Elegiste {numero}");
            """,
            """
            int numero;
            string linea = Console.ReadLine()!;
            while (!int.TryParse(linea, out numero) || numero < 1 || numero > 10)
            {
                Console.WriteLine("Intenta de nuevo");
                linea = Console.ReadLine()!;
            }
            Console.WriteLine($"Elegiste {numero}");
            """,
            [
                In("Válido de inmediato", "4", "Elegiste 4"),
                In("Texto y luego válido", "hola\n7", "Intenta de nuevo\nElegiste 7"),
                In("Fuera de rango", "0\n11\n10", "Intenta de nuevo\nIntenta de nuevo\nElegiste 10", hidden: true, edge: true, hint: "0 y 11 se pueden convertir, pero no están entre 1 y 10."),
                In("Vacío", "\n3", "Intenta de nuevo\nElegiste 3", hidden: true, edge: true),
            ],
            "Gracias al cortocircuito, numero < 1 solo se evalúa si TryParse tuvo éxito.",
            ["Agrega a la condición: || numero < 1 || numero > 10.", "Así el bucle sigue si el formato o el rango fallan."],
            requires: [CodeConstruct.Loop], targets: [ErrorCategory.EdgeCaseMissed]);

        r.Code("n1.validacion.indep", s, ItemPurpose.Independent, CodeMode.Program,
            "Lee un precio con decimales (usa decimal.TryParse). Mientras no sea un número mayor que 0, muestra «Precio inválido» y vuelve a leer. Si se acaba la entrada (ReadLine devuelve null), muestra «Sin precio» y termina. Si es válido, muestra «Precio: X» con dos decimales (:F2).",
            """
            string? linea = Console.ReadLine();
            """,
            """
            string? linea = Console.ReadLine();
            decimal precio;
            while (linea is not null && (!decimal.TryParse(linea, out precio) || precio <= 0))
            {
                Console.WriteLine("Precio inválido");
                linea = Console.ReadLine();
            }
            if (linea is null || !decimal.TryParse(linea, out precio))
            {
                Console.WriteLine("Sin precio");
            }
            else
            {
                Console.WriteLine($"Precio: {precio:F2}");
            }
            """,
            [
                In("Válido", "19.5", "Precio: 19.50"),
                In("Negativo y luego válido", "-3\n2", "Precio inválido\nPrecio: 2.00"),
                In("Cero no vale", "0\n0.99", "Precio inválido\nPrecio: 0.99", hidden: true, edge: true),
                In("Se acaba la entrada", "abc\n-1", "Precio inválido\nPrecio inválido\nSin precio", hidden: true, edge: true, hint: "Cuando ReadLine devuelve null ya no habrá más datos: no sigas pidiendo."),
            ],
            "Un programa robusto contempla que la entrada se termine (null) en lugar de suponer que siempre habrá otra línea.",
            ["Usa string? y compara con null antes de intentar convertir.", "La condición del bucle: hay línea y (no se puede convertir o es <= 0)."],
            difficulty: 3, requires: [CodeConstruct.Loop], related: ["n0.conversion"], targets: [ErrorCategory.NullReference, ErrorCategory.EdgeCaseMissed]);

        r.Code("n1.validacion.app", s, ItemPurpose.Transfer, CodeMode.Program,
            "Aplicación: valida una contraseña nueva. Lee contraseñas (una por línea) hasta que una cumpla: al menos 8 caracteres, al menos un dígito y al menos una letra mayúscula. Por cada inválida muestra qué le falta, en este orden y separado por «, »: «8 caracteres», «un dígito», «una mayúscula». Al aceptar una muestra «Contraseña aceptada».",
            """
            string clave = Console.ReadLine()!;
            """,
            """
            string clave = Console.ReadLine()!;
            while (true)
            {
                bool largo = clave.Length >= 8;
                bool digito = false;
                bool mayuscula = false;
                foreach (char c in clave)
                {
                    if (char.IsDigit(c)) digito = true;
                    if (char.IsUpper(c)) mayuscula = true;
                }
                if (largo && digito && mayuscula)
                {
                    break;
                }
                List<string> faltan = [];
                if (!largo) faltan.Add("8 caracteres");
                if (!digito) faltan.Add("un dígito");
                if (!mayuscula) faltan.Add("una mayúscula");
                Console.WriteLine("Falta: " + string.Join(", ", faltan));
                clave = Console.ReadLine()!;
            }
            Console.WriteLine("Contraseña aceptada");
            """,
            [
                In("Válida", "Segura123", "Contraseña aceptada"),
                In("Corta", "Ab1\nAbcdefg1", "Falta: 8 caracteres\nContraseña aceptada"),
                In("Sin nada", "abc\nAbcdefgh9", "Falta: 8 caracteres, un dígito, una mayúscula\nContraseña aceptada", hidden: true, edge: true),
                In("Sin mayúscula", "abcdefg12\nabcdefG12", "Falta: una mayúscula\nContraseña aceptada", hidden: true),
            ],
            "Cada regla se calcula por separado; así es posible decir exactamente qué falta, que es mejor retroalimentación para el usuario.",
            ["Recorre la contraseña con foreach (char c in clave) y usa char.IsDigit y char.IsUpper.", "Junta los mensajes de lo que falta en una lista y únelos con string.Join(\", \", lista)."],
            difficulty: 3, requires: [CodeConstruct.Loop], related: ["n0.texto"]);

        return new Skill(s, UnitId, "Validación de datos",
            "Rechazar entradas inválidas y volver a pedirlas.",
            Tier.Essential, ["n1.while", "n0.conversion"],
            new Lesson(s,
                "Validar formato y rango de la entrada, pedirla de nuevo con un mensaje útil y manejar el fin de la entrada.",
                [
                    P("Un programa que recibe datos de personas debe asumir que llegarán errores: letras donde iba un número, valores fuera de rango o líneas vacías."),
                    P("El patrón básico combina un bucle con TryParse y comprobaciones de rango. El bucle se repite mientras el dato sea inválido:"),
                    Fragment("""
                        int mes;
                        Console.Write("Mes (1-12): ");
                        while (!int.TryParse(Console.ReadLine(), out mes) || mes < 1 || mes > 12)
                        {
                            Console.Write("Escribe un número del 1 al 12: ");
                        }
                        """),
                    P("Gracias al cortocircuito, las comprobaciones de rango solo se evalúan cuando TryParse tuvo éxito."),
                    P("Console.ReadLine() devuelve null cuando la entrada se termina (por ejemplo, si los datos vienen de un archivo). Un programa robusto lo contempla en lugar de pedir datos para siempre."),
                    Design("Explica qué esperabas: «Escribe un número del 1 al 12» ayuda más que «Error»."),
                ],
                new WorkedExample("Validar varias entradas simuladas",
                    """
                    string[] intentos = ["", "doce", "15", "8"];
                    int indice = 0;
                    int horas;
                    while (!int.TryParse(intentos[indice], out horas) || horas < 0 || horas > 12)
                    {
                        Console.WriteLine($"«{intentos[indice]}» no es válido");
                        indice++;
                    }
                    Console.WriteLine($"Horas: {horas}");
                    """,
                    [
                        Line(1, "Simulamos lo que escribiría una persona para ver cada caso."),
                        Line(4, "Se repite si no es número o si está fuera de 0–12."),
                        Line(6, "Mensaje específico con el dato rechazado."),
                        Line(9, "Solo se llega aquí con un valor válido."),
                    ],
                    "«» no es válido\n«doce» no es válido\n«15» no es válido\nHoras: 8"),
                new CommonMistake(
                    "Validar el formato pero olvidar el rango.",
                    "int edad;\nwhile (!int.TryParse(Console.ReadLine(), out edad))\n{\n    Console.WriteLine(\"Escribe un número\");\n}",
                    "-5 o 900 se convierten sin problema y se aceptan como edades.",
                    "int edad;\nwhile (!int.TryParse(Console.ReadLine(), out edad) || edad < 0 || edad > 120)\n{\n    Console.WriteLine(\"Escribe una edad entre 0 y 120\");\n}"),
                "n1.validacion.pred1", "n1.validacion.guiado", "n1.validacion.indep", "n1.validacion.app",
                ["n1.validacion.recall1", "n1.validacion.recall2", "n1.validacion.concepto1"],
                "Escribes validaciones que rechazan formato y rango inválidos, informan qué falta y no fallan cuando se acaba la entrada.",
                15));
    }

    private static Skill Descomposicion(ContentRegistry r)
    {
        const string s = "n1.descomposicion";
        r.Short("n1.descomposicion.pred1", s, ItemPurpose.Prediction,
            "Este programa calcula el cambio con la menor cantidad de monedas de 10, 5 y 1. ¿Qué muestra?",
            "Se toman primero las monedas más grandes: 37 → 3 de 10 (quedan 7) → 1 de 5 (quedan 2) → 2 de 1.",
            ["10:3 5:1 1:2"], match: AnswerMatch.Output,
            code: """
                int cambio = 37;
                int de10 = cambio / 10;
                cambio %= 10;
                int de5 = cambio / 5;
                cambio %= 5;
                Console.WriteLine($"10:{de10} 5:{de5} 1:{cambio}");
                """,
            difficulty: 2, related: ["n0.aritmetica"]);

        r.Choice("n1.descomposicion.concepto1", s, ItemPurpose.Concept,
            "¿Cuál es el primer paso recomendable antes de escribir código para un problema nuevo?",
            "Entender el problema: qué datos entran, qué debe salir y resolver a mano uno o dos ejemplos, incluidos los casos límite. Sin eso, el código suele resolver otro problema.",
            [
                Ok("Entender entradas y salidas y resolver un ejemplo a mano.", "Correcto."),
                No("Escribir el código lo más rápido posible y corregir con las pruebas.", "Probar es importante, pero sin entender el problema se pierde mucho tiempo."),
                No("Buscar la solución completa en internet.", "Copiar no desarrolla la habilidad de resolver problemas."),
            ]);

        r.Choice("n1.descomposicion.recall1", s, ItemPurpose.Recall,
            "¿Qué significa «descomponer» un problema?",
            "Dividirlo en subproblemas más pequeños que se pueden resolver y verificar por separado (leer datos, calcular, mostrar).",
            [
                Ok("Dividirlo en partes más pequeñas que se resuelven y prueban por separado.", "Correcto."),
                No("Borrar el código y empezar de nuevo.", "No es eso."),
                No("Reducir el número de variables al mínimo.", "Puede ser un efecto, no la idea central."),
            ]);

        r.Choice("n1.descomposicion.recall2", s, ItemPurpose.Recall,
            "¿Qué es un caso límite (edge case)?",
            "Una entrada en el borde de lo permitido: cero, uno, una lista vacía, el valor máximo, el límite exacto de un rango. Es donde más errores aparecen.",
            [
                Ok("Una entrada en el borde de lo permitido, como 0, una lista vacía o el valor máximo.", "Correcto."),
                No("Un error de compilación difícil.", "Es una categoría de entradas, no de errores."),
                No("Un caso que nunca ocurre en la práctica.", "Ocurren, y son los que más fallan."),
            ]);

        r.Choice("n1.descomposicion.bug1", s, ItemPurpose.BugFinding,
            "Para convertir segundos en «horas:minutos:segundos», este programa muestra 1:61:1 con 3661 segundos. ¿Cuál es el error?",
            "minutos debe ser el residuo después de quitar las horas: (total % 3600) / 60, o total / 60 % 60. Al dividir el total entre 60 se cuentan también los minutos que ya forman horas.",
            [
                Ok("Los minutos deben calcularse con lo que sobra después de quitar las horas.", "Correcto."),
                No("Las horas deberían ser total / 60.", "Eso daría minutos totales, no horas.", ErrorCategory.WrongLogic),
                No("Falta convertir a double.", "Con enteros es correcto: queremos divisiones enteras.", ErrorCategory.OperatorMisuse),
            ],
            code: """
                int total = 3661;
                int horas = total / 3600;
                int minutos = total / 60;
                int segundos = total % 60;
                Console.WriteLine($"{horas}:{minutos}:{segundos}");
                """,
            targets: [ErrorCategory.WrongLogic]);

        r.Code("n1.descomposicion.guiado", s, ItemPurpose.Guided, CodeMode.Program,
            "El problema: lee un total de segundos y muéstralo como «h:mm:ss» (minutos y segundos con dos dígitos). Ya está resuelto el paso 1 (leer) y el paso 3 (mostrar). Completa el paso 2 (calcular).",
            """
            // Paso 1: leer
            int total = int.Parse(Console.ReadLine()!);
            // Paso 2: calcular horas, minutos y segundos
            int horas = 0;
            int minutos = 0;
            int segundos = 0;
            // Paso 3: mostrar
            Console.WriteLine($"{horas}:{minutos:D2}:{segundos:D2}");
            """,
            """
            int total = int.Parse(Console.ReadLine()!);
            int horas = total / 3600;
            int minutos = total % 3600 / 60;
            int segundos = total % 60;
            Console.WriteLine($"{horas}:{minutos:D2}:{segundos:D2}");
            """,
            [
                In("Una hora y algo", "3661", "1:01:01"),
                In("Menos de un minuto", "45", "0:00:45"),
                In("Muchas horas", "100000", "27:46:40", hidden: true),
                In("Cero", "0", "0:00:00", hidden: true, edge: true),
            ],
            "Dividir el problema en leer → calcular → mostrar permite concentrarse en una parte a la vez. :D2 rellena con ceros a la izquierda.",
            ["horas = total / 3600.", "minutos = lo que sobra de las horas (total % 3600) dividido entre 60."],
            related: ["n0.aritmetica"]);

        r.Code("n1.descomposicion.indep", s, ItemPurpose.Independent, CodeMode.Program,
            "Lee un número entero positivo y muestra en una línea: la suma de sus dígitos, el dígito mayor y el número invertido, separados por espacios. Ejemplo: 4091 → «14 9 1904».",
            """
            int numero = int.Parse(Console.ReadLine()!);
            """,
            """
            int numero = int.Parse(Console.ReadLine()!);
            int suma = 0;
            int mayor = 0;
            int invertido = 0;
            int resto = numero;
            do
            {
                int digito = resto % 10;
                suma += digito;
                if (digito > mayor)
                {
                    mayor = digito;
                }
                invertido = invertido * 10 + digito;
                resto /= 10;
            } while (resto > 0);
            Console.WriteLine($"{suma} {mayor} {invertido}");
            """,
            [
                In("4091", "4091", "14 9 1904"),
                In("Un dígito", "7", "7 7 7"),
                In("Termina en cero", "120", "3 2 21", hidden: true, edge: true, hint: "Al invertir 120 se obtiene 021, que como número es 21."),
                In("Repetidos", "5555", "20 5 5555", hidden: true),
            ],
            "Las tres tareas comparten el mismo recorrido de dígitos (% 10 y / 10); identificar esa parte común es el corazón de la descomposición.",
            ["Extrae dígitos con % 10 y elimínalos con / 10, en un bucle.", "Para invertir: invertido = invertido * 10 + digito."],
            difficulty: 3, requires: [CodeConstruct.Loop], related: ["n1.while", "n1.acumuladores"]);

        r.Code("n1.descomposicion.app", s, ItemPurpose.Transfer, CodeMode.Program,
            "Aplicación: una tienda vende boletos a 85 cada uno. Por cada 4 boletos, uno es gratis (paga 3 de cada 4). Si el total a pagar supera 1000, se aplica 10 % de descuento sobre ese total. Lee la cantidad de boletos y muestra el total con dos decimales (:F2).",
            """
            int boletos = int.Parse(Console.ReadLine()!);
            """,
            """
            int boletos = int.Parse(Console.ReadLine()!);
            int gratis = boletos / 4;
            decimal total = (boletos - gratis) * 85m;
            if (total > 1000)
            {
                total *= 0.9m;
            }
            Console.WriteLine($"{total:F2}");
            """,
            [
                In("3 boletos", "3", "255.00"),
                In("4 boletos", "4", "255.00"),
                In("16 boletos", "16", "918.00", hidden: true, hint: "16 boletos: 4 gratis, pagan 12 → 1020, que supera 1000: aplica 10 %."),
                In("0 boletos", "0", "0.00", hidden: true, edge: true),
                In("15 boletos", "15", "918.00", hidden: true, edge: true),
            ],
            "Resuelve primero a mano: cuántos son gratis, cuánto se paga y si aplica el descuento. Cada regla es un paso.",
            ["Gratis = boletos / 4 (división entera).", "Calcula el total, y solo después decide si aplica el 10 %."],
            difficulty: 2, related: ["n1.if-else", "n0.aritmetica"]);

        return new Skill(s, UnitId, "Resolución de problemas paso a paso",
            "Entender, planear, ejecutar y verificar.",
            Tier.Essential, ["n1.acumuladores"],
            new Lesson(s,
                "Aplicar un proceso de cuatro pasos (entender, planear, programar, verificar) y descomponer un problema en partes comprobables.",
                [
                    P("Programar no empieza en el teclado. Un proceso que funciona: 1) entender el problema: ¿qué entra?, ¿qué sale?, ¿qué casos límite hay?; 2) planear: resolver un ejemplo a mano y anotar los pasos; 3) programar cada paso; 4) verificar con ejemplos, incluidos los límites."),
                    P("Descomponer es separar el problema en partes pequeñas: leer datos, calcular, mostrar resultados. Cada parte se puede probar por separado. Más adelante, cada parte será un método."),
                    Fragment("""
                        // 1. Leer: precio y cantidad
                        // 2. Calcular: subtotal, descuento según reglas, total
                        // 3. Mostrar: total con dos decimales
                        """, "plan escrito como comentarios antes de programar"),
                    Tip("Escribir el plan como comentarios y luego rellenarlos con código evita perderse en problemas largos."),
                    Note("Cuando algo falla, vuelve a resolver a mano el ejemplo que falla y compara paso a paso con lo que hace tu código."),
                ],
                new WorkedExample("Cambio con billetes y monedas",
                    """
                    int cambio = 286;
                    int[] denominaciones = [100, 50, 20, 10, 5, 1];
                    foreach (int d in denominaciones)
                    {
                        int cantidad = cambio / d;
                        if (cantidad > 0)
                        {
                            Console.WriteLine($"{cantidad} x {d}");
                        }
                        cambio %= d;
                    }
                    """,
                    [
                        Line(1, "Entender: entra un monto y sale cuántas piezas de cada denominación."),
                        Line(2, "Planear: de la mayor a la menor, tomar todas las que quepan."),
                        Line(5, "Cuántas piezas de esta denominación caben."),
                        Line(10, "Lo que queda por entregar."),
                    ],
                    "2 x 100\n1 x 50\n1 x 20\n1 x 10\n1 x 5\n1 x 1"),
                new CommonMistake(
                    "Programar sin haber resuelto un ejemplo a mano.",
                    "int total = 3661;\nint minutos = total / 60; // 61: olvida las horas",
                    "Sin el ejemplo resuelto a mano (3661 s = 1 h, 1 min, 1 s) es fácil no notar que los minutos deben descontar las horas.",
                    "int total = 3661;\nint horas = total / 3600;\nint minutos = total % 3600 / 60;\nint segundos = total % 60;\nConsole.WriteLine($\"{horas}:{minutos:D2}:{segundos:D2}\");"),
                "n1.descomposicion.pred1", "n1.descomposicion.guiado", "n1.descomposicion.indep", "n1.descomposicion.app",
                ["n1.descomposicion.recall1", "n1.descomposicion.recall2", "n1.descomposicion.concepto1"],
                "Ante un problema nuevo escribes un plan por pasos, lo verificas a mano con un caso límite y lo traduces a código que pasa las pruebas ocultas.",
                15));
    }

    private static Skill Complejidad(ContentRegistry r)
    {
        const string s = "n1.complejidad";
        r.Short("n1.complejidad.pred1", s, ItemPurpose.Prediction,
            "¿Qué número muestra este programa? (cuántas veces se ejecuta la línea interna)",
            "Por cada uno de los 4 valores de i, j recorre 4 valores: 4 × 4 = 16 operaciones. Con n elementos serían n × n.",
            ["16"], match: AnswerMatch.Number,
            code: """
                int n = 4;
                int operaciones = 0;
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        operaciones++;
                    }
                }
                Console.WriteLine(operaciones);
                """,
            wrong: new() { ["8"] = "Los bucles anidados multiplican, no suman: 4 vueltas internas por cada vuelta externa." });

        r.Short("n1.complejidad.pred2", s, ItemPurpose.Prediction,
            "¿Cuántas veces se divide n entre 2 antes de llegar a 1? Escribe el número que muestra el programa.",
            "1024 → 512 → … → 1: diez divisiones, porque 2^10 = 1024. Dividir a la mitad en cada paso crece muy lento (logarítmico).",
            ["10"], match: AnswerMatch.Number,
            code: """
                int n = 1024;
                int pasos = 0;
                while (n > 1)
                {
                    n /= 2;
                    pasos++;
                }
                Console.WriteLine(pasos);
                """,
            difficulty: 2);

        r.Choice("n1.complejidad.concepto1", s, ItemPurpose.Concept,
            "Un algoritmo tarda 1 segundo con 1 000 datos y hace n × n operaciones. Aproximadamente, ¿cuánto tardará con 10 000 datos?",
            "Si el trabajo crece como n², multiplicar n por 10 multiplica el trabajo por 100: alrededor de 100 segundos.",
            [
                Ok("Alrededor de 100 segundos.", "Correcto: 10² = 100."),
                No("Alrededor de 10 segundos.", "Eso sería si el trabajo creciera de forma lineal.", ErrorCategory.ConceptMisunderstanding),
                No("Lo mismo, 1 segundo.", "Más datos implican más trabajo."),
            ],
            difficulty: 2);

        r.Choice("n1.complejidad.recall1", s, ItemPurpose.Recall,
            "¿Qué mide el análisis de complejidad de un algoritmo?",
            "Cómo crece la cantidad de trabajo (o memoria) cuando crece el tamaño de la entrada, sin depender de la velocidad de la computadora.",
            [
                Ok("Cómo crece el trabajo cuando crece el tamaño de la entrada.", "Correcto."),
                No("Cuántos segundos tarda exactamente en mi computadora.", "Eso depende del equipo; la complejidad describe el crecimiento."),
                No("Cuántas líneas de código tiene.", "Un programa corto puede ser muy costoso y uno largo muy eficiente."),
            ]);

        r.Choice("n1.complejidad.recall2", s, ItemPurpose.Recall,
            "Ordena de menor a mayor crecimiento: lineal (n), cuadrático (n²) y logarítmico (log n).",
            "log n crece más lento que n, y n más lento que n².",
            [
                Ok("log n, n, n²", "Correcto."),
                No("n, log n, n²", "log n crece más lento que n."),
                No("n², n, log n", "Ese es el orden inverso."),
            ]);

        r.Choice("n1.complejidad.bug1", s, ItemPurpose.BugFinding,
            "Este programa da el resultado correcto, pero con n = 1 000 000 tarda muchísimo. ¿Cuál es el problema?",
            "Los bucles anidados hacen n × n operaciones (10¹² con un millón). La suma de 1 a n se puede calcular con un solo bucle, o directamente con la fórmula n(n+1)/2.",
            [
                Ok("Usa bucles anidados innecesarios: n × n operaciones para algo que necesita n (o una fórmula).", "Correcto."),
                No("Console.WriteLine es lento.", "Se llama una sola vez; el costo está en los bucles."),
                No("long es más lento que int.", "La diferencia es despreciable frente a n² operaciones."),
            ],
            code: """
                int n = 1000;
                long suma = 0;
                for (int i = 1; i <= n; i++)
                {
                    for (int j = 1; j <= i; j++)
                    {
                        suma++;
                    }
                }
                Console.WriteLine(suma);
                """,
            difficulty: 2, targets: [ErrorCategory.InfiniteLoopOrTimeout]);

        r.Code("n1.complejidad.guiado", s, ItemPurpose.Guided, CodeMode.Program,
            "El programa cuenta los pares (i, j) con 1 ≤ i < j ≤ n usando dos bucles. Funciona para n pequeño, pero las pruebas incluyen n = 200 000. Reemplaza los bucles por la fórmula n × (n − 1) / 2 (usa long).",
            """
            int n = int.Parse(Console.ReadLine()!);
            long pares = 0;
            for (int i = 1; i <= n; i++)
            {
                for (int j = i + 1; j <= n; j++)
                {
                    pares++;
                }
            }
            Console.WriteLine(pares);
            """,
            """
            int n = int.Parse(Console.ReadLine()!);
            long pares = (long)n * (n - 1) / 2;
            Console.WriteLine(pares);
            """,
            [
                In("n = 4", "4", "6"),
                In("n = 1", "1", "0", edge: true),
                In("n grande", "200000", "19999900000", hidden: true, hint: "Con bucles anidados son unos 20 mil millones de vueltas: usa la fórmula.", edge: true),
            ],
            "Contar vueltas una por una cuesta n²; la fórmula cuesta lo mismo para cualquier n (tiempo constante).",
            ["Elimina los dos bucles.", "Convierte a long antes de multiplicar: (long)n * (n - 1) / 2."],
            difficulty: 2, targets: [ErrorCategory.InfiniteLoopOrTimeout]);

        r.Code("n1.complejidad.indep", s, ItemPurpose.Independent, CodeMode.Program,
            "Lee n y luego n enteros ordenados de menor a mayor (en una sola línea, separados por espacios). Después lee un valor buscado y muestra en qué posición está (empezando en 0) o -1 si no está. Debe funcionar con 1 000 000 de números y muchas búsquedas: usa búsqueda binaria (dividir el rango a la mitad en cada paso). La última línea de la entrada contiene k y después vienen k valores a buscar, uno por línea; muestra una posición por línea.",
            """
            int n = int.Parse(Console.ReadLine()!);
            int[] datos = Array.ConvertAll(Console.ReadLine()!.Split(' ', StringSplitOptions.RemoveEmptyEntries), int.Parse);
            int k = int.Parse(Console.ReadLine()!);
            for (int q = 0; q < k; q++)
            {
                int buscado = int.Parse(Console.ReadLine()!);
                int posicion = -1;
                for (int i = 0; i < n; i++)
                {
                    if (datos[i] == buscado)
                    {
                        posicion = i;
                    }
                }
                Console.WriteLine(posicion);
            }
            """,
            """
            int n = int.Parse(Console.ReadLine()!);
            int[] datos = Array.ConvertAll(Console.ReadLine()!.Split(' ', StringSplitOptions.RemoveEmptyEntries), int.Parse);
            int k = int.Parse(Console.ReadLine()!);
            for (int q = 0; q < k; q++)
            {
                int buscado = int.Parse(Console.ReadLine()!);
                int izquierda = 0;
                int derecha = n - 1;
                int posicion = -1;
                while (izquierda <= derecha)
                {
                    int medio = izquierda + (derecha - izquierda) / 2;
                    if (datos[medio] == buscado)
                    {
                        posicion = medio;
                        break;
                    }
                    if (datos[medio] < buscado)
                    {
                        izquierda = medio + 1;
                    }
                    else
                    {
                        derecha = medio - 1;
                    }
                }
                Console.WriteLine(posicion);
            }
            """,
            [
                In("Pequeño", "5\n1 3 5 7 9\n3\n7\n1\n4", "3\n0\n-1"),
                In("Extremos", "4\n2 4 6 8\n2\n8\n2", "3\n0", hidden: true, edge: true),
                In("Vacío", "0\n\n1\n5", "-1", hidden: true, edge: true),
                new CodeTestCase
                {
                    Name = "Un millón de datos y 3000 búsquedas",
                    Input = Grande(),
                    Expected = GrandeEsperado(),
                    Hidden = true,
                    FailureHint = "La búsqueda lineal revisa hasta un millón de valores por consulta; la binaria solo unos 20.",
                },
            ],
            "La búsqueda binaria descarta la mitad del rango en cada paso: con un millón de datos necesita unas 20 comparaciones en lugar de un millón. Solo funciona si los datos están ordenados.",
            ["Mantén dos índices, izquierda y derecha, y mira el elemento del medio.", "Si el del medio es menor que el buscado, descarta la mitad izquierda (izquierda = medio + 1); si es mayor, la derecha."],
            difficulty: 3, requires: [CodeConstruct.Loop], related: ["n1.while"], minutes: 12);

        r.Code("n1.complejidad.app", s, ItemPurpose.Transfer, CodeMode.Program,
            "Aplicación: lee n y luego n números (uno por línea). Muestra cuántos valores distintos hay. Las pruebas incluyen hasta 200 000 números entre 0 y 1 000 000: compararlos todos contra todos es demasiado lento. Pista de enfoque: usa un arreglo de bool de tamaño 1 000 001 para marcar los que ya viste.",
            """
            int n = int.Parse(Console.ReadLine()!);
            """,
            """
            int n = int.Parse(Console.ReadLine()!);
            bool[] visto = new bool[1_000_001];
            int distintos = 0;
            for (int i = 0; i < n; i++)
            {
                int valor = int.Parse(Console.ReadLine()!);
                if (!visto[valor])
                {
                    visto[valor] = true;
                    distintos++;
                }
            }
            Console.WriteLine(distintos);
            """,
            [
                In("Algunos repetidos", "6\n3\n1\n3\n7\n1\n3", "3"),
                In("Todos distintos", "3\n10\n20\n30", "3", hidden: true),
                In("Ninguno", "0", "0", hidden: true, edge: true),
                new CodeTestCase
                {
                    Name = "Muchos valores",
                    Input = Distintos(),
                    Expected = "100000",
                    Hidden = true,
                    FailureHint = "Con 200 000 números, comparar cada uno con todos los anteriores son unos 20 mil millones de comparaciones.",
                },
            ],
            "Marcar en un arreglo cambia un algoritmo n² por uno lineal a cambio de memoria: un intercambio frecuente en programación. Más adelante usarás HashSet para lo mismo con cualquier tipo de valor.",
            ["Crea bool[] visto = new bool[1_000_001];", "Si visto[valor] es false, márcalo y cuenta uno más."],
            difficulty: 3, requires: [CodeConstruct.Loop], related: ["n1.acumuladores"], minutes: 10);

        return new Skill(s, UnitId, "Análisis intuitivo de complejidad",
            "Cuántas operaciones hace un algoritmo según el tamaño de la entrada.",
            Tier.Intermediate, ["n1.for"],
            new Lesson(s,
                "Estimar cómo crece el trabajo de un algoritmo (constante, logarítmico, lineal, cuadrático) y elegir un enfoque que escale.",
                [
                    P("Dos programas pueden dar el mismo resultado y tardar tiempos muy distintos. La complejidad describe cómo crece el trabajo cuando crece el tamaño de la entrada n."),
                    P("Reglas prácticas: un bucle que recorre n elementos hace del orden de n operaciones (lineal). Dos bucles anidados sobre n hacen del orden de n² (cuadrático). Un bucle que divide el problema a la mitad en cada paso hace del orden de log₂ n (logarítmico). Una fórmula que no depende de n es constante."),
                    Code("""
                        int n = 1_000_000;
                        long lineal = n;
                        long cuadratico = (long)n * n;
                        int logaritmico = 0;
                        for (int m = n; m > 1; m /= 2)
                        {
                            logaritmico++;
                        }
                        Console.WriteLine($"{logaritmico} {lineal} {cuadratico}");
                        """),
                    P("Con un millón de datos, un algoritmo cuadrático hace un billón de operaciones: puede tardar minutos. Uno lineal hace un millón: milisegundos."),
                    Note("Aquí usamos una idea intuitiva. Más adelante verás la notación O grande, que formaliza este análisis."),
                ],
                new WorkedExample("Contar operaciones",
                    """
                    int n = 8;
                    int lineal = 0, cuadratico = 0, logaritmico = 0;
                    for (int i = 0; i < n; i++)
                    {
                        lineal++;
                        for (int j = 0; j < n; j++)
                        {
                            cuadratico++;
                        }
                    }
                    for (int m = n; m > 1; m /= 2)
                    {
                        logaritmico++;
                    }
                    Console.WriteLine($"n={n}: log={logaritmico}, lineal={lineal}, cuadrático={cuadratico}");
                    """,
                    [
                        Line(5, "Se ejecuta una vez por cada i: n veces."),
                        Line(8, "Se ejecuta n veces por cada i: n × n en total."),
                        Line(13, "m se reduce a la mitad: 8 → 4 → 2 → 1, son 3 pasos (log₂ 8)."),
                    ],
                    "n=8: log=3, lineal=8, cuadrático=64"),
                new CommonMistake(
                    "Recorrer todo de nuevo en cada paso cuando bastaría con recordar un resultado.",
                    "for (int i = 0; i < n; i++)\n{\n    long suma = 0;\n    for (int j = 0; j <= i; j++) suma += datos[j];\n    Console.WriteLine(suma);\n}",
                    "Cada suma parcial se recalcula desde cero: n² operaciones. Basta con ir acumulando.",
                    "int[] datos = [3, 1, 4];\nlong suma = 0;\nfor (int i = 0; i < datos.Length; i++)\n{\n    suma += datos[i];\n    Console.WriteLine(suma);\n}"),
                "n1.complejidad.pred1", "n1.complejidad.guiado", "n1.complejidad.indep", "n1.complejidad.app",
                ["n1.complejidad.recall1", "n1.complejidad.recall2", "n1.complejidad.concepto1"],
                "Estimas el crecimiento de un algoritmo por su estructura de bucles y reemplazas soluciones cuadráticas por lineales o logarítmicas cuando los datos son grandes.",
                15));
    }

    private static string Grande()
    {
        var sb = new System.Text.StringBuilder();
        sb.Append("1000000\n");
        for (var i = 0; i < 1_000_000; i++)
        {
            sb.Append(i * 2).Append(i < 999_999 ? " " : "\n");
        }

        sb.Append("3000\n");
        for (var q = 0; q < 3000; q++)
        {
            sb.Append(q * 666 + 1).Append('\n');
        }

        return sb.ToString();
    }

    private static string GrandeEsperado()
    {
        var sb = new System.Text.StringBuilder();
        for (var q = 0; q < 3000; q++)
        {
            var valor = q * 666 + 1;
            sb.Append(valor % 2 == 0 ? valor / 2 : -1).Append('\n');
        }

        return sb.ToString();
    }

    private static string Distintos()
    {
        var sb = new System.Text.StringBuilder();
        sb.Append("200000\n");
        for (var i = 0; i < 200_000; i++)
        {
            sb.Append(i % 100_000 * 7).Append('\n');
        }

        return sb.ToString();
    }
}
