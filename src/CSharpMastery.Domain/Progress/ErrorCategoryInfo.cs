namespace CSharpMastery.Domain.Progress;

/// <summary>Nombres y consejos en español para cada categoría de error.</summary>
public static class ErrorCategoryInfo
{
    public static string DisplayName(ErrorCategory category) => category switch
    {
        ErrorCategory.None => "Sin error",
        ErrorCategory.Syntax => "Sintaxis",
        ErrorCategory.UndeclaredIdentifier => "Nombre no declarado o mal escrito",
        ErrorCategory.TypeMismatch => "Tipos incompatibles",
        ErrorCategory.UninitializedVariable => "Variable sin inicializar",
        ErrorCategory.Scope => "Alcance de variables",
        ErrorCategory.StaticVersusInstance => "Miembros estáticos y de instancia",
        ErrorCategory.AccessModifier => "Modificadores de acceso",
        ErrorCategory.InheritanceContract => "Herencia e interfaces",
        ErrorCategory.AsyncMisuse => "Uso de async/await",
        ErrorCategory.MissingReturn => "Falta un return",
        ErrorCategory.WrongArguments => "Argumentos incorrectos",
        ErrorCategory.OperatorMisuse => "Operadores",
        ErrorCategory.NullReference => "Referencias nulas",
        ErrorCategory.IndexOutOfRange => "Índices fuera de rango",
        ErrorCategory.DivideByZero => "División entre cero",
        ErrorCategory.ParsingFormat => "Conversión de texto a número",
        ErrorCategory.InfiniteLoopOrTimeout => "Bucle infinito o demasiado lento",
        ErrorCategory.StackOverflowRecursion => "Recursión sin caso base",
        ErrorCategory.OffByOne => "Error por uno (límites de bucles)",
        ErrorCategory.WrongLogic => "Lógica incorrecta",
        ErrorCategory.EdgeCaseMissed => "Caso límite no considerado",
        ErrorCategory.OutputFormat => "Formato de salida",
        ErrorCategory.ConceptMisunderstanding => "Concepto mal comprendido",
        ErrorCategory.PredictionError => "Predicción de comportamiento",
        ErrorCategory.ForbiddenApi => "API no permitida",
        ErrorCategory.MissingConstruct => "No usó la construcción pedida",
        ErrorCategory.UnhandledException => "Excepción no controlada",
        _ => "Otro",
    };

    public static string Advice(ErrorCategory category) => category switch
    {
        ErrorCategory.Syntax => "Lee el mensaje del compilador y revisa la línea indicada y la anterior: suele faltar ; ) o }.",
        ErrorCategory.UndeclaredIdentifier => "C# distingue mayúsculas de minúsculas. Verifica que el nombre exista y esté declarado antes de usarse.",
        ErrorCategory.TypeMismatch => "Comprueba el tipo de cada lado de la asignación. Convierte de forma explícita solo cuando tenga sentido.",
        ErrorCategory.UninitializedVariable => "Asigna un valor inicial a la variable en todas las rutas antes de leerla.",
        ErrorCategory.Scope => "Una variable solo existe dentro del bloque { } donde se declaró.",
        ErrorCategory.MissingReturn => "Todas las rutas de un método que devuelve un valor deben terminar en return.",
        ErrorCategory.IndexOutOfRange => "Los índices van de 0 a Length - 1 (o Count - 1). Revisa las condiciones de los bucles.",
        ErrorCategory.OffByOne => "Revisa si el bucle debe usar < o <= y dónde empieza.",
        ErrorCategory.NullReference => "Antes de usar un objeto, asegúrate de que no sea null.",
        ErrorCategory.InfiniteLoopOrTimeout => "Asegúrate de que la condición del bucle llegue a ser falsa.",
        ErrorCategory.StackOverflowRecursion => "Toda recursión necesita un caso base alcanzable.",
        ErrorCategory.EdgeCaseMissed => "Prueba mentalmente con valores extremos: vacío, cero, negativos, uno solo.",
        ErrorCategory.OutputFormat => "Compara letra por letra la salida esperada con la tuya: espacios, mayúsculas y signos.",
        ErrorCategory.ParsingFormat => "Valida el texto antes de convertirlo, por ejemplo con int.TryParse.",
        _ => "Revisa la explicación y vuelve a intentarlo.",
    };
}
