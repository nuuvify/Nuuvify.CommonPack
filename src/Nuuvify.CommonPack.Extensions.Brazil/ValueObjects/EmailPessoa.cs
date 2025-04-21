using Nuuvify.CommonPack.Extensions.Notificator;

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

        _ = new ValidationConcernR<EmailPessoa>(this)
            .AssertIsEmail(x => endereco)
            .AssertHasMaxLength(x => endereco, maxEndereco);

        if (!IsValid())
        {
            Endereco = null;
        }

        Endereco = endereco;
    }

    public const int maxEndereco = 256;

    public override string ToString()
    {
        return Endereco?.ToString();
    }

}
