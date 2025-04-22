
using Nuuvify.CommonPack.Mediator.Implementation;

namespace Nuuvify.CommonPack.Extensions.Brazil;

public class AliquotaIssEspecial : NotifiableR
{

    public AliquotaIssEspecial(double aliquota, EnumSimNao temAliquota)
    {

        Validar(aliquota, temAliquota);

    }

    public double AliquotaIssEspecialParaContribuinteOptantePeloSimples { get; private set; }
    public string TemAliquotaIssEspecialParaContribuinteOptantePeloSimples { get; private set; } = "S";

    private void Validar(double aliquota, EnumSimNao situacao)
    {

        if (situacao == EnumSimNao.Nao)
        {
            AliquotaIssEspecialParaContribuinteOptantePeloSimples = 0;
            TemAliquotaIssEspecialParaContribuinteOptantePeloSimples = EnumSimNao.Nao.GetDescription();

        }
        else
        {
            TemAliquotaIssEspecialParaContribuinteOptantePeloSimples = EnumSimNao.Sim.GetDescription();

            _ = new ValidationConcernR<AliquotaIssEspecial>(this)
                .AssertIsBetween(x => aliquota, MinAliquota, MaxAliquota, $"Aliquota deve estar entre {MinAliquota} e {MaxAliquota}");

            var valido = IsValid();

            if (valido)
                AliquotaIssEspecialParaContribuinteOptantePeloSimples = aliquota;
            else
                AliquotaIssEspecialParaContribuinteOptantePeloSimples = 0;
        }

    }

    public const int MinAliquota = 0;
    public const int MaxAliquota = 99;

}
