using Nuuvify.CommonPack.Extensions.Notificator;

namespace Nuuvify.CommonPack.Domain.ValueObjects;

public class DocumentoPessoaEstrangeiro : NotifiableR
{

    public DocumentoPessoaEstrangeiro(string codigoInfBeneficiario,
        string numeroIdentificacaoFiscal,
        string provincia,
        NacionalidadeFiscal nacionalidade)
    {

        if (!nacionalidade.IsValid())
            AddNotifications(nacionalidade.Notifications);
        else
        {
            DefinirNacionalidade(nacionalidade);

            if (!nacionalidade.Codigo.GetHashCode().Equals(EnumNacionalidadeFiscal.Nacional.GetHashCode()) &&
                !nacionalidade.Codigo.GetHashCode().Equals(int.MaxValue))
            {
                DefinirCodigoInfBeneficiarioRendimento(codigoInfBeneficiario);
                DefinirNumeroIdentificacaoFiscal(numeroIdentificacaoFiscal);
                DefinirProvincia(provincia);
            }
        }

    }

    public string CodigoInfBeneficiarioRendimento { get; private set; }
    public string NumeroIdentificacaoFiscal { get; private set; }
    public string Provincia { get; private set; }
    public string Nacionalidade { get; private set; }

    private void DefinirCodigoInfBeneficiarioRendimento(string codigoInfBeneficiario)
    {
        var validacao = Notifications.Count;

        _ = new ValidationConcernR<DocumentoPessoaEstrangeiro>(this)
            .AssertHasMaxLength(x => codigoInfBeneficiario, maxCodigoInfBeneficiarioRendimento);

        if (validacao.Equals(Notifications.Count))
            CodigoInfBeneficiarioRendimento = codigoInfBeneficiario;
    }

    private void DefinirNumeroIdentificacaoFiscal(string numeroIdentificacaoFiscal)
    {
        var validacao = Notifications.Count;

        _ = new ValidationConcernR<DocumentoPessoaEstrangeiro>(this)
            .AssertHasMaxLength(x => numeroIdentificacaoFiscal, maxNumeroIdentificacaoFiscal);

        if (validacao.Equals(Notifications.Count))
            NumeroIdentificacaoFiscal = numeroIdentificacaoFiscal;
    }

    private void DefinirProvincia(string provincia)
    {
        var validacao = Notifications.Count;

        var provinciaContribuinte = provincia?.ToUpper();

        _ = new ValidationConcernR<DocumentoPessoaEstrangeiro>(this)
            .AssertHasMaxLength(x => provinciaContribuinte, maxProvincia);

        if (validacao.Equals(Notifications.Count))
            Provincia = provinciaContribuinte;
    }

    private void DefinirNacionalidade(NacionalidadeFiscal nacionalidade)
    {
        if (nacionalidade.IsValid())
            Nacionalidade = nacionalidade.Codigo;
        else
            AddNotifications(nacionalidade.Notifications);
    }

    public const int minCodigoInfBeneficiarioRendimento = 0;
    public const int maxCodigoInfBeneficiarioRendimento = 3;

    public const int minNumeroIdentificacaoFiscal = 0;
    public const int maxNumeroIdentificacaoFiscal = 20;

    public const int minProvincia = 0;
    public const int maxProvincia = 40;

}
