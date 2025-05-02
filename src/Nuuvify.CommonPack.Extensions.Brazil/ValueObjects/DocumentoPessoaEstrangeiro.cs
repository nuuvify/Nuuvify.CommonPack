using Nuuvify.CommonPack.Domain.ValueObjects;
using Nuuvify.CommonPack.Mediator.Implementation;

namespace Nuuvify.CommonPack.Extensions.Brazil;

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
        if (string.IsNullOrEmpty(codigoInfBeneficiario) || codigoInfBeneficiario.Length > MaxCodigoInfBeneficiarioRendimento)
        {
            AddNotification(nameof(CodigoInfBeneficiarioRendimento), $"O código do beneficiário deve ter no máximo {MaxCodigoInfBeneficiarioRendimento} caracteres.");
            return;
        }

        CodigoInfBeneficiarioRendimento = codigoInfBeneficiario;
    }

    private void DefinirNumeroIdentificacaoFiscal(string numeroIdentificacaoFiscal)
    {
        if (string.IsNullOrEmpty(numeroIdentificacaoFiscal) || numeroIdentificacaoFiscal.Length > MaxNumeroIdentificacaoFiscal)
        {
            AddNotification(nameof(NumeroIdentificacaoFiscal), $"O número de identificação fiscal deve ter no máximo {MaxNumeroIdentificacaoFiscal} caracteres.");
            return;
        }

        NumeroIdentificacaoFiscal = numeroIdentificacaoFiscal;
    }

    private void DefinirProvincia(string provincia)
    {
        var provinciaContribuinte = provincia?.ToUpper();

        if (string.IsNullOrEmpty(provinciaContribuinte) || provinciaContribuinte.Length > MaxProvincia)
        {
            AddNotification(nameof(Provincia), $"A província deve ter no máximo {MaxProvincia} caracteres.");
            return;
        }

        Provincia = provinciaContribuinte;
    }

    private void DefinirNacionalidade(NacionalidadeFiscal nacionalidade)
    {
        if (nacionalidade.IsValid())
            Nacionalidade = nacionalidade.Codigo;
        else
            AddNotifications(nacionalidade.Notifications);
    }

    public const int MinCodigoInfBeneficiarioRendimento = 0;
    public const int MaxCodigoInfBeneficiarioRendimento = 3;
    public const int MinNumeroIdentificacaoFiscal = 0;
    public const int MaxNumeroIdentificacaoFiscal = 20;
    public const int MinProvincia = 0;
    public const int MaxProvincia = 40;

}
