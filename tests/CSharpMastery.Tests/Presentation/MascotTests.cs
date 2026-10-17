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
}
