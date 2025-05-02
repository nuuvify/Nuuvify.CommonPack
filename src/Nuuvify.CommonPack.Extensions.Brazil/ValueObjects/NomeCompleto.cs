using Nuuvify.CommonPack.Extensions.Implementation;
using Nuuvify.CommonPack.Mediator.Implementation;

namespace Nuuvify.CommonPack.Extensions.Brazil;

public class NomeCompleto : NotifiableR
{

    protected NomeCompleto() { }
    public NomeCompleto(string nome, string sobrenome)
    {

        if (string.IsNullOrWhiteSpace(nome))
        {
            AddNotification(nameof(Nome), "Nome não pode ser nulo ou vazio.");
        }
        else if (nome.Length < MinNome)
        {
            AddNotification(nameof(Nome), $"Nome deve ter no mínimo {MinNome} caracteres.");
        }
        else if (nome.Length > MaxNome)
        {
            AddNotification(nameof(Nome), $"Nome deve ter no máximo {MaxNome} caracteres.");
        }

        if (string.IsNullOrWhiteSpace(sobrenome))
        {
            AddNotification(nameof(SobreNome), "Sobrenome não pode ser nulo ou vazio.");
        }
        else if (sobrenome.Length < MinSobreNome)
        {
            AddNotification(nameof(SobreNome), $"Sobrenome deve ter no mínimo {MinSobreNome} caracteres.");
        }
        else if (sobrenome.Length > MaxSobreNome)
        {
            AddNotification(nameof(SobreNome), $"Sobrenome deve ter no máximo {MaxSobreNome} caracteres.");
        }

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
