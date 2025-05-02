using Nuuvify.CommonPack.Mediator.Implementation;

namespace Nuuvify.CommonPack.Extensions.Brazil;

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

    public const int MinNome = 3;
    public const int MaxNome = 60;
    public const int MinSobreNome = 3;
    public const int MaxSobreNome = 60;

    public override string ToString()
    {
        var nomeSobrenome = $"{Nome} {SobreNome}";
        return nomeSobrenome.Trim();
    }
}
