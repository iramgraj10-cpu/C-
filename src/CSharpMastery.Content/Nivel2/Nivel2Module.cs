using CSharpMastery.Content.Infrastructure;
using CSharpMastery.Domain.Curriculum;

namespace CSharpMastery.Content.Nivel2;

internal static class Nivel2Module
{
    public static CurriculumModule Build(ContentRegistry r) => new("n2", 2, "Métodos y estructuras de datos",
        "Organizar el código en métodos y elegir la estructura de datos adecuada.",
        [
            new Unit("n2.u1", "n2", "Métodos", "Declarar, invocar, parámetros, retorno, alcance y sobrecarga.",
            [
                Outline.Skill("n2.metodos", "n2.u1", "Declaración e invocación de métodos", "Dividir un programa en piezas con nombre.", Tier.Essential, "n1.acumuladores"),
                Outline.Skill("n2.parametros", "n2.u1", "Parámetros y valores de retorno", "Pasar datos a un método y obtener un resultado.", Tier.Essential, "n2.metodos"),
                Outline.Skill("n2.alcance", "n2.u1", "Alcance de variables", "Dónde existe cada variable.", Tier.Essential, "n2.parametros"),
                Outline.Skill("n2.sobrecarga", "n2.u1", "Sobrecarga de métodos", "Varios métodos con el mismo nombre y distinta firma.", Tier.Intermediate, "n2.parametros"),
            ]),
            new Unit("n2.u2", "n2", "Colecciones", "Arrays, matrices, List<T>, Dictionary<TKey,TValue> y HashSet<T>.",
            [
                Outline.Skill("n2.arrays", "n2.u2", "Arrays", "Colecciones de tamaño fijo con índices.", Tier.Essential, "n1.for"),
                Outline.Skill("n2.matrices", "n2.u2", "Matrices (arrays bidimensionales)", "Datos en filas y columnas.", Tier.Intermediate, "n2.arrays"),
                Outline.Skill("n2.listas", "n2.u2", "List<T>", "Colecciones que crecen y se reducen.", Tier.Essential, "n2.arrays"),
                Outline.Skill("n2.diccionarios", "n2.u2", "Dictionary<TKey,TValue> y HashSet<T>", "Búsqueda por clave y conjuntos sin duplicados.", Tier.Essential, "n2.listas"),
                Outline.Skill("n2.eleccion", "n2.u2", "Elegir la estructura de datos", "Comparar costo y propósito de cada colección.", Tier.Intermediate, "n2.diccionarios"),
            ]),
            new Unit("n2.u3", "n2", "Texto y tipos simples", "Manipulación de strings, enumeraciones y tuplas.",
            [
                Outline.Skill("n2.strings", "n2.u3", "Strings y manipulación de texto", "Recorrer, buscar, dividir y construir texto.", Tier.Essential, "n2.arrays"),
                Outline.Skill("n2.enums", "n2.u3", "Enumeraciones", "Conjuntos cerrados de valores con nombre.", Tier.Essential, "n1.switch"),
                Outline.Skill("n2.tuplas", "n2.u3", "Tuplas", "Agrupar valores sin crear un tipo.", Tier.Intermediate, "n2.parametros"),
            ]),
            new Unit("n2.u4", "n2", "Recursividad", "Problemas que se definen en términos de sí mismos.",
            [
                Outline.Skill("n2.recursion", "n2.u4", "Recursividad", "Caso base, caso recursivo y pila de llamadas.", Tier.Intermediate, "n2.parametros", "n2.arrays"),
            ]),
        ]);
}
