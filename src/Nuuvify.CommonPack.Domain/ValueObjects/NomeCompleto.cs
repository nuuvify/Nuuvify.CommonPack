using Nuuvify.CommonPack.Extensions.Implementation;
using Nuuvify.CommonPack.Extensions.Notificator;

namespace Nuuvify.CommonPack.Domain.ValueObjects;

public class NomeCompleto : NotifiableR
{

    protected NomeCompleto() { }
    public NomeCompleto(string nome, string sobrenome)
    {

        _ = new ValidationConcernR<NomeCompleto>(this)
            .AssertNotIsNullOrWhiteSpace(x => nome, nome)
            .AssertHasMinLength(x => nome, minNome)
            .AssertHasMaxLength(x => nome, maxNome)
            .AssertNotIsNullOrWhiteSpace(x => sobrenome, sobrenome)
            .AssertHasMinLength(x => sobrenome, minSobreNome)
            .AssertHasMaxLength(x => sobrenome, maxSobreNome);

        if (!IsValid())
        {
            Nome = null;
            SobreNome = null;
        }
        else
        {
            Nome = nome.ToTitleCase();
            SobreNome = sobrenome.ToTitleCase();

        }
    }

    public string Nome { get; private set; }
    public string SobreNome { get; private set; }

    public const int minNome = 3;
    public const int maxNome = 60;
    public const int minSobreNome = 3;
    public const int maxSobreNome = 60;

    public override string ToString()
    {
        var nomeSobrenome = $"{Nome} {SobreNome}";
        return nomeSobrenome.Trim();
    }
}
