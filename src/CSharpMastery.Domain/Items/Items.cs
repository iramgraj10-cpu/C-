using CSharpMastery.Domain.Curriculum;
using CSharpMastery.Domain.Progress;

namespace CSharpMastery.Domain.Items;

/// <summary>Para qué sirve el ítem dentro de la secuencia de aprendizaje.</summary>
public enum ItemPurpose
{
    /// <summary>Pregunta conceptual.</summary>
    Concept,
    /// <summary>Predecir la salida o el comportamiento de un fragmento.</summary>
    Prediction,
    /// <summary>Detectar y explicar o corregir un error.</summary>
    BugFinding,
    /// <summary>Recuperación activa: recordar sin apuntes.</summary>
    Recall,
    /// <summary>Ejercicio con andamiaje (ejemplo parcialmente completo).</summary>
    Guided,
    /// <summary>Escribir código desde cero sin ayuda.</summary>
    Independent,
    /// <summary>Aplicar la habilidad en un contexto nuevo (transferencia).</summary>
    Transfer,
}

/// <summary>Base de todos los ítems evaluables.</summary>
public abstract record LearningItem
{
    public required string Id { get; init; }
    public required string SkillId { get; init; }
    /// <summary>Otras habilidades que el ítem ejercita (para intercalado).</summary>
    public IReadOnlyList<string> RelatedSkillIds { get; init; } = [];
    public required ItemPurpose Purpose { get; init; }
    public KnowledgeDimension Dimension { get; init; } = KnowledgeDimension.Semantics;
    /// <summary>Dificultad de 1 (muy fácil) a 5 (muy difícil).</summary>
    public int Difficulty { get; init; } = 1;
    public required string Prompt { get; init; }
    /// <summary>Código asociado al enunciado (para predecir, encontrar errores, etc.).</summary>
    public string? Code { get; init; }
    /// <summary>Pistas progresivas: de la más general a la más concreta.</summary>
    public IReadOnlyList<string> Hints { get; init; } = [];
    /// <summary>Explicación del razonamiento correcto, mostrada al terminar.</summary>
    public required string Explanation { get; init; }
    /// <summary>Errores que el ítem ayuda a corregir (para práctica de refuerzo).</summary>
    public IReadOnlyList<ErrorCategory> TargetsErrors { get; init; } = [];
    /// <summary>Minutos estimados para resolverlo con calma.</summary>
    public double EstimatedMinutes { get; init; } = 2;

    public IEnumerable<string> AllSkillIds()
    {
        yield return SkillId;
        foreach (var id in RelatedSkillIds)
        {
            yield return id;
        }
    }

    public bool IsCode => this is CodeExercise;
}

/// <summary>Opción de una pregunta de selección, con retroalimentación específica.</summary>
public sealed record ChoiceOption(string Text, bool IsCorrect, string Feedback, ErrorCategory Error = ErrorCategory.ConceptMisunderstanding);

/// <summary>Pregunta de opción múltiple (concepto, predicción o detección de errores).</summary>
public sealed record ChoiceItem : LearningItem
{
    public required IReadOnlyList<ChoiceOption> Options { get; init; }
}

public enum AnswerMatch
{
    /// <summary>Igual ignorando mayúsculas y espacios en los extremos.</summary>
    Text,
    /// <summary>Compara la salida de consola línea a línea, ignorando espacios finales.</summary>
    Output,
    /// <summary>Compara como número (acepta 2.50 = 2.5).</summary>
    Number,
}

/// <summary>Respuesta corta escrita por el estudiante (recuperación activa o predicción).</summary>
public sealed record ShortAnswerItem : LearningItem
{
    public required IReadOnlyList<string> AcceptedAnswers { get; init; }
    public AnswerMatch Match { get; init; } = AnswerMatch.Text;
    /// <summary>Respuestas incorrectas frecuentes con retroalimentación específica.</summary>
    public IReadOnlyDictionary<string, string> KnownWrongAnswers { get; init; } = new Dictionary<string, string>();
}

/// <summary>Cómo se ejecuta y evalúa un ejercicio de código.</summary>
public enum CodeMode
{
    /// <summary>Programa completo: se le da entrada estándar y se compara la salida.</summary>
    Program,
    /// <summary>El estudiante implementa miembros; las pruebas evalúan expresiones.</summary>
    Function,
}

/// <summary>Construcciones del lenguaje que el ejercicio exige usar.</summary>
public enum CodeConstruct
{
    Loop,
    Conditional,
    Method,
    Recursion,
    Class,
    Interface,
    Record,
    Lambda,
    Linq,
    TryCatch,
    Async,
    Switch,
    Generic,
}

/// <summary>Caso de prueba de un ejercicio de código.</summary>
public sealed record CodeTestCase
{
    public required string Name { get; init; }
    /// <summary>Entrada estándar (modo Program).</summary>
    public string? Input { get; init; }
    /// <summary>Expresión o bloque con <c>return</c> a evaluar (modo Function).</summary>
    public string? Check { get; init; }
    /// <summary>Resultado esperado (salida de consola o valor formateado).</summary>
    public string Expected { get; init; } = "";
    /// <summary>Si se espera una excepción, su nombre de tipo (p. ej. "ArgumentException").</summary>
    public string? ExpectedException { get; init; }
    /// <summary>Las pruebas ocultas no muestran entrada ni salida esperada.</summary>
    public bool Hidden { get; init; }
    /// <summary>Marca la prueba como caso límite.</summary>
    public bool EdgeCase { get; init; }
    /// <summary>Pista específica si esta prueba falla.</summary>
    public string? FailureHint { get; init; }
}

/// <summary>Ejercicio de programación evaluado con pruebas reales.</summary>
public sealed record CodeExercise : LearningItem
{
    public required CodeMode Mode { get; init; }
    public required string StarterCode { get; init; }
    public required string ReferenceSolution { get; init; }
    public required IReadOnlyList<CodeTestCase> Tests { get; init; }
    public IReadOnlyList<CodeConstruct> RequiredConstructs { get; init; } = [];
    /// <summary>Explicación de la solución modelo (otras soluciones válidas también aprueban).</summary>
    public string SolutionExplanation { get; init; } = "";
}
