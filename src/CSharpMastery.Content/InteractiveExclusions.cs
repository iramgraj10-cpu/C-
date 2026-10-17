namespace CSharpMastery.Content;

/// <summary>
/// Pasos interactivos generados que la validación encontró ambiguos (un distractor que funciona igual que la
/// respuesta, u otro orden de líneas válido). El generador los salta y usa el siguiente candidato.
/// Lista mantenida a partir de <c>InteractiveValidationTests</c>.
/// </summary>
public static class InteractiveExclusionList
{
    public static readonly IReadOnlySet<string> Keys = new HashSet<string>(StringComparer.Ordinal)
    {
        "n0.programa|cloze|card2|8", // «Write» en lugar de «WriteLine»: mismo resultado (Hola, mundo)
        "n0.relacionales|cloze|card0|81", // «!=» en lugar de «>=»: mismo resultado (True⏎False⏎False)
        "n0.salida|cloze|card1|8", // «Write» en lugar de «WriteLine»: mismo resultado (Nombre:	Ada⏎Edad:	36)
        "n1.booleanos|cloze|card0|76", // «!=» en lugar de «>»: mismo resultado (True)
        "n1.break-continue|cloze|card1|45", // «<=» en lugar de «==»: mismo resultado (1⏎3⏎5)
        "n1.descomposicion|cloze|example|150", // «!=» en lugar de «>»: mismo resultado (2 x 100⏎1 x 50⏎1 x 20⏎1 x 10⏎1 x 5⏎1 x 1)
        "n1.if-else|cloze|card0|102", // «!=» en lugar de «>»: mismo resultado (Multa leve)
        "n1.switch|cloze|card0|100", // «<» en lugar de «>=»: mismo resultado (No encontrado)
        "n1.validacion|cloze|example|144", // «>=» en lugar de «>»: mismo resultado («» no es válido⏎«doce» no es válido⏎«15» no es válido⏎Horas:…)
        "n2.eleccion|cloze|card1|68", // «<=» en lugar de «<»: mismo resultado (1000 1000)
        "n2.enums|cloze|card0|97", // «>=» en lugar de «==»: mismo resultado (Intermedio⏎1⏎False⏎Avanzado)
        "n2.sobrecarga|cloze|card0|145", // «>=» en lugar de «>»: mismo resultado (8⏎8)
        "n3.composicion|cloze|card0|94", // «Write» en lugar de «WriteLine»: mismo resultado (Ana — Reforma 10, CDMX)
        "n3.constructores|cloze|card0|58", // «Write» en lugar de «WriteLine»: mismo resultado (Rayuela de Julio Cortázar)
        "n3.encapsulacion|parsons|4", // intercambiar «if (n > Cantidad(producto)) return false;» y «_stock[producto] = Cantidad(producto) - n;»: mismo resultado (70⏎False⏎0)
        "n3.solid|cloze|card2|200", // «Write» en lugar de «WriteLine»: mismo resultado ([consola] Aviso: Pago recibido)
        "n4.genericos|parsons|4", // intercambiar «if (valor.CompareTo(minimo) < 0) return minimo;» y «if (valor.CompareTo(maximo) > 0) return maximo;»: mismo resultado (10⏎0⏎f)
        "n0.programa|cloze|example|92", // «Write» en lugar de «WriteLine»: mismo resultado (Hola, mundo⏎Estoy aprendiendo C#)
        "n1.booleanos|cloze|card0|81", // «||» en lugar de «&&»: mismo resultado (True)
        "n1.break-continue|cloze|card1|18", // «!=» en lugar de «<=»: mismo resultado (1⏎3⏎5)
        "n1.if-else|cloze|card0|34", // «>=» en lugar de «>»: mismo resultado (Multa leve)
        "n1.switch|cloze|card0|111", // «<» en lugar de «<=»: mismo resultado (No encontrado)
        "n1.validacion|cloze|example|131", // «==» en lugar de «<»: mismo resultado («» no es válido⏎«doce» no es válido⏎«15» no es válido⏎Horas:…)
        "n2.sobrecarga|cloze|card0|72", // «4» en lugar de «5»: mismo resultado (8⏎8)
        "n3.encapsulacion|parsons|3", // intercambiar «if (n > Cantidad(producto)) return false;» y «_stock[producto] = Cantidad(producto) - n;»: mismo resultado (70⏎False⏎0)
        "n4.genericos|parsons|3", // intercambiar «if (valor.CompareTo(minimo) < 0) return minimo;» y «if (valor.CompareTo(maximo) > 0) return maximo;»: mismo resultado (10⏎0⏎f)
        "n1.if-else|cloze|card0|16", // «71» en lugar de «72»: mismo resultado (Multa leve)
        "n1.switch|cloze|card0|0", // «decimal» en lugar de «int»: mismo resultado (No encontrado)
        "n1.if-else|cloze|card0|36", // «101» en lugar de «100»: mismo resultado (Multa leve)
        "n1.switch|cloze|card0|103", // «499» en lugar de «500»: mismo resultado (No encontrado)
        "n1.if-else|cloze|card0|104", // «59» en lugar de «60»: mismo resultado (Multa leve)
        "n1.switch|cloze|card0|114", // «598» en lugar de «599»: mismo resultado (No encontrado)
        "n1.if-else|cloze|card0|0", // «long» en lugar de «int»: mismo resultado (Multa leve)
        "n1.switch|cloze|card0|178", // «Write» en lugar de «WriteLine»: mismo resultado (No encontrado)
        "n1.if-else|cloze|card0|55", // «Write» en lugar de «WriteLine»: mismo resultado (Multa leve)
        "n1.if-else|cloze|card0|168", // «Write» en lugar de «WriteLine»: mismo resultado (Multa leve)
        "n1.if-else|cloze|card0|122", // «Write» en lugar de «WriteLine»: mismo resultado (Multa leve)
    };
}
