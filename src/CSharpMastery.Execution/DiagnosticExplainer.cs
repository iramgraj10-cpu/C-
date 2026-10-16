using CSharpMastery.Domain.Progress;

namespace CSharpMastery.Execution;

/// <summary>Explicación en español de un error de compilación o de ejecución.</summary>
public sealed record Explanation(string Text, ErrorCategory Category);

/// <summary>Traduce los diagnósticos más comunes del compilador a explicaciones didácticas.</summary>
public static class DiagnosticExplainer
{
    private static readonly Dictionary<string, Explanation> Known = new()
    {
        ["CS1002"] = new("Falta un punto y coma ';' al final de una instrucción.", ErrorCategory.Syntax),
        ["CS1513"] = new("Falta una llave de cierre '}'. Cada '{' debe tener su pareja.", ErrorCategory.Syntax),
        ["CS1514"] = new("Falta una llave de apertura '{'.", ErrorCategory.Syntax),
        ["CS1026"] = new("Falta un paréntesis de cierre ')'.", ErrorCategory.Syntax),
        ["CS1003"] = new("Error de sintaxis: el compilador esperaba otro símbolo en esa posición. A menudo falta un ; al final de la línea anterior.", ErrorCategory.Syntax),
        ["CS1001"] = new("Se esperaba un identificador (un nombre) en esa posición.", ErrorCategory.Syntax),
        ["CS1525"] = new("Hay un término inválido en la expresión: revisa operadores o paréntesis sobrantes.", ErrorCategory.Syntax),
        ["CS1010"] = new("Una cadena de texto no está cerrada: falta la comilla doble final.", ErrorCategory.Syntax),
        ["CS1012"] = new("Un literal char ('a') solo puede contener un carácter. Para texto usa comillas dobles.", ErrorCategory.Syntax),
        ["CS1022"] = new("Hay código fuera de lugar: se esperaba una definición de tipo o el final del archivo.", ErrorCategory.Syntax),
        ["CS0201"] = new("Esa expresión no hace nada por sí sola: solo asignaciones, llamadas, ++/-- y new pueden ser instrucciones.", ErrorCategory.Syntax),
        ["CS0103"] = new("Usas un nombre que no existe en este contexto. ¿Está mal escrito (C# distingue mayúsculas) o se declaró en otro bloque?", ErrorCategory.UndeclaredIdentifier),
        ["CS0246"] = new("No se encuentra ese tipo. Revisa el nombre y las mayúsculas.", ErrorCategory.UndeclaredIdentifier),
        ["CS0117"] = new("Ese tipo no tiene un miembro con ese nombre.", ErrorCategory.UndeclaredIdentifier),
        ["CS1061"] = new("El objeto no tiene ese método o propiedad. Revisa el nombre y el tipo de la variable.", ErrorCategory.UndeclaredIdentifier),
        ["CS0029"] = new("No se puede convertir implícitamente un tipo en otro. Revisa el tipo de la variable y del valor.", ErrorCategory.TypeMismatch),
        ["CS0266"] = new("La conversión puede perder información, así que C# exige una conversión explícita (cast) o cambiar el tipo.", ErrorCategory.TypeMismatch),
        ["CS0030"] = new("No existe conversión entre esos tipos.", ErrorCategory.TypeMismatch),
        ["CS0019"] = new("Ese operador no se puede aplicar a esos tipos (por ejemplo, && entre números o + entre tipos incompatibles).", ErrorCategory.OperatorMisuse),
        ["CS0023"] = new("Ese operador no se puede aplicar a ese tipo.", ErrorCategory.OperatorMisuse),
        ["CS0131"] = new("El lado izquierdo de una asignación debe ser una variable, propiedad o indizador.", ErrorCategory.OperatorMisuse),
        ["CS0165"] = new("Lees una variable local antes de asignarle valor en todas las rutas.", ErrorCategory.UninitializedVariable),
        ["CS0128"] = new("Ya existe una variable con ese nombre en este ámbito.", ErrorCategory.Scope),
        ["CS0136"] = new("Ese nombre ya se usa en un ámbito que contiene a este; elige otro nombre.", ErrorCategory.Scope),
        ["CS0161"] = new("No todas las rutas del método devuelven un valor: falta un return en algún caso.", ErrorCategory.MissingReturn),
        ["CS0126"] = new("El método debe devolver un valor: el return necesita una expresión.", ErrorCategory.MissingReturn),
        ["CS0127"] = new("Un método void no puede devolver un valor.", ErrorCategory.MissingReturn),
        ["CS1501"] = new("Ninguna versión del método acepta esa cantidad de argumentos.", ErrorCategory.WrongArguments),
        ["CS1503"] = new("Un argumento tiene un tipo que no coincide con el parámetro.", ErrorCategory.WrongArguments),
        ["CS7036"] = new("Falta un argumento obligatorio en la llamada.", ErrorCategory.WrongArguments),
        ["CS0120"] = new("Intentas usar un miembro de instancia sin un objeto. Crea una instancia o marca el miembro como static.", ErrorCategory.StaticVersusInstance),
        ["CS0176"] = new("Un miembro static se usa con el nombre del tipo, no con una instancia.", ErrorCategory.StaticVersusInstance),
        ["CS0122"] = new("Ese miembro no es accesible desde aquí por su modificador de acceso (private, protected...).", ErrorCategory.AccessModifier),
        ["CS0200"] = new("La propiedad es de solo lectura: no tiene set accesible.", ErrorCategory.AccessModifier),
        ["CS0191"] = new("Un campo readonly solo se asigna en su declaración o en el constructor.", ErrorCategory.AccessModifier),
        ["CS0535"] = new("La clase no implementa todos los miembros de la interfaz.", ErrorCategory.InheritanceContract),
        ["CS0534"] = new("La clase no implementa un miembro abstracto heredado.", ErrorCategory.InheritanceContract),
        ["CS0115"] = new("No hay un método virtual o abstracto que se pueda sobrescribir con ese nombre y firma.", ErrorCategory.InheritanceContract),
        ["CS0144"] = new("No se puede crear una instancia de una clase abstracta o de una interfaz.", ErrorCategory.InheritanceContract),
        ["CS0506"] = new("Solo se puede usar override sobre miembros virtual, abstract u override.", ErrorCategory.InheritanceContract),
        ["CS4032"] = new("await solo se puede usar dentro de un método marcado como async.", ErrorCategory.AsyncMisuse),
        ["CS4033"] = new("await solo se puede usar dentro de un método marcado como async.", ErrorCategory.AsyncMisuse),
        ["CS0163"] = new("En un switch, cada sección debe terminar con break, return o throw.", ErrorCategory.Syntax),
        ["CS0152"] = new("Hay dos case con el mismo valor en el switch.", ErrorCategory.Syntax),
        ["CS0150"] = new("Un case necesita un valor constante.", ErrorCategory.Syntax),
        ["CS8805"] = new("Este ejercicio pide definir clases y métodos, no instrucciones sueltas. Coloca tu código dentro de la clase indicada.", ErrorCategory.Syntax),
        ["CS5001"] = new("El programa no tiene punto de entrada: escribe instrucciones de nivel superior o un método Main.", ErrorCategory.Syntax),
        ["CS0017"] = new("Hay más de un punto de entrada (Main) en el programa.", ErrorCategory.Syntax),
        ["CS8600"] = new("Advertencia: podrías estar guardando null en una variable que no admite null.", ErrorCategory.NullReference),
        ["CS8602"] = new("Advertencia: podrías estar usando una referencia que puede ser null.", ErrorCategory.NullReference),
        ["CS8604"] = new("Advertencia: podrías pasar null a un parámetro que no lo admite.", ErrorCategory.NullReference),
        ["CS8618"] = new("Advertencia: una propiedad no anulable queda sin inicializar al salir del constructor.", ErrorCategory.NullReference),
        ["CS0168"] = new("Advertencia: declaraste una variable que nunca usas.", ErrorCategory.None),
        ["CS0219"] = new("Advertencia: asignas un valor a una variable que nunca usas.", ErrorCategory.None),
        ["CS0162"] = new("Advertencia: hay código inalcanzable (nunca se ejecutará).", ErrorCategory.WrongLogic),
    };

    public static Explanation Explain(CompilerDiagnostic diagnostic)
    {
        if (diagnostic.Message.StartsWith("[pruebas]", StringComparison.Ordinal))
        {
            return new Explanation(
                "Las pruebas no pudieron usar tu código. Revisa que los nombres de clases y métodos, sus parámetros y su tipo de retorno coincidan exactamente con lo que pide el enunciado.",
                ErrorCategory.WrongArguments);
        }

        return Known.TryGetValue(diagnostic.Id, out var known)
            ? known
            : new Explanation("El compilador encontró un problema. Lee el mensaje original y revisa la línea indicada.",
                diagnostic.Severity == "Error" ? ErrorCategory.Syntax : ErrorCategory.None);
    }

    public static Explanation ExplainException(RuntimeFailure failure)
    {
        bool Is(string name) => failure.TypeChain.Contains(name);

        if (Is("IndexOutOfRangeException") || Is("ArgumentOutOfRangeException"))
        {
            return new("Accediste a una posición que no existe. Los índices van de 0 a Length - 1. Revisa los límites de tus bucles.", ErrorCategory.IndexOutOfRange);
        }

        if (Is("NullReferenceException"))
        {
            return new("Usaste un miembro de una referencia que vale null. Verifica que el objeto se haya creado o comprueba null antes.", ErrorCategory.NullReference);
        }

        if (Is("DivideByZeroException"))
        {
            return new("Dividiste un entero entre cero. Valida el divisor antes de dividir.", ErrorCategory.DivideByZero);
        }

        if (Is("FormatException"))
        {
            return new("Intentaste convertir un texto que no tiene el formato esperado (por ejemplo, int.Parse(\"hola\")). Considera int.TryParse.", ErrorCategory.ParsingFormat);
        }

        if (Is("OverflowException"))
        {
            return new("El resultado no cabe en el tipo numérico (desbordamiento). Usa un tipo más grande o revisa el cálculo.", ErrorCategory.OperatorMisuse);
        }

        if (Is("InvalidCastException"))
        {
            return new("Una conversión de tipo falló en tiempo de ejecución: el objeto no es del tipo al que intentaste convertirlo.", ErrorCategory.TypeMismatch);
        }

        if (Is("KeyNotFoundException"))
        {
            return new("Buscaste una clave que no existe en el diccionario. Usa ContainsKey o TryGetValue.", ErrorCategory.IndexOutOfRange);
        }

        if (Is("StackOverflowException"))
        {
            return new("La pila se desbordó: probablemente una recursión sin caso base o que nunca lo alcanza.", ErrorCategory.StackOverflowRecursion);
        }

        if (Is("OutOfMemoryException") || Is("InsufficientExecutionStackException"))
        {
            return new("El programa consumió demasiada memoria. Revisa si una colección crece sin control.", ErrorCategory.InfiniteLoopOrTimeout);
        }

        if (Is("InvalidOperationException"))
        {
            return new("Operación no válida en el estado actual (por ejemplo, modificar una colección mientras se recorre con foreach, o pedir First/Max de una secuencia vacía).", ErrorCategory.UnhandledException);
        }

        return new($"Se lanzó una excepción {failure.ExceptionType} que nadie controló.", ErrorCategory.UnhandledException);
    }
}
