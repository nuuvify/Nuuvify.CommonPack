using Nuuvify.CommonPack.Mediator.Implementation;

namespace Nuuvify.CommonPack.Extensions.Brazil;

public class EmailPessoa : NotifiableR
{

    protected EmailPessoa() { }

    public EmailPessoa(string endereco)
    {
        Validate(endereco);
    }

    public string Endereco { get; private set; }

    private void Validate(string endereco)
    {

        if (string.IsNullOrWhiteSpace(endereco) || !endereco.Contains("@"))
        {
            AddNotification(nameof(Endereco), "Invalid email address.");
        }

        if (endereco.Length > MaxEndereco)
        {
            AddNotification(nameof(Endereco), $"Email address exceeds maximum length of {MaxEndereco} characters.");
        }

        if (!IsValid())
        {
            Endereco = null;
        }

        Endereco = endereco;
    }

    public const int MaxEndereco = 256;

    public override string ToString()
    {
        return Endereco?.ToString();
    }

}
