using CSharpMastery.Domain.Curriculum;
using CSharpMastery.Learning.Interactive;
using CSharpMastery.Presentation.ViewModels.Activity;

namespace CSharpMastery.Tests.Presentation;

public sealed class MascotTests
{
    [Fact]
    public void Una_tarjeta_con_mucho_texto_cuenta_como_lectura_larga_y_el_codigo_no_suma()
    {
        var corta = new ConceptCardViewModel(new ConceptCardStep("k1", [new Paragraph("Un int guarda enteros."), new CodeBlock(new string('x', 500))], 1, 2));
        var larga = new ConceptCardViewModel(new ConceptCardStep("k2", [new Paragraph(new string('a', ConceptCardViewModel.LongReadChars))], 2, 2));

        Assert.False(corta.IsLongRead);
        Assert.True(larga.IsLongRead);
    }

    [Fact]
    public void El_robot_es_la_mascota_principal_y_las_fotos_salen_de_vez_en_cuando()
    {
        var keys = Enumerable.Range(0, 300).Select(i => $"paso-{i}").ToList();
        var elegidas = keys.Select(k => Mascots.ForThinking(k))
            .Concat(keys.Select(k => Mascots.ForGreeting(k)))
            .Concat(keys.Select(k => Mascots.ForReading(k, longText: false)))
            .ToList();

        var robot = elegidas.Count(m => m == Mascots.Robot);
        Assert.True(robot > elegidas.Count / 2, $"robot {robot} de {elegidas.Count}");
        Assert.Contains(Mascots.Thinking, elegidas);
        Assert.Contains(Mascots.Photo, elegidas);
        Assert.All(keys, k => Assert.Equal(Mascots.Robot, Mascots.ForReading(k, longText: false)));
        Assert.Contains(keys, k => Mascots.ForReading(k, longText: true) == Mascots.Sleeping);
    }
}
