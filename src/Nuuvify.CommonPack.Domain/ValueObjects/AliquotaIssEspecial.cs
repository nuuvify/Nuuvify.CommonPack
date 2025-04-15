using Nuuvify.CommonPack.Extensions.Implementation;
using Nuuvify.CommonPack.Extensions.Notificator;

namespace Nuuvify.CommonPack.Domain.ValueObjects;


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
                .AssertIsBetween(x => aliquota, minAliquota, maxAliquota, $"Aliquota deve estar entre {minAliquota} e {maxAliquota}");

            var valido = IsValid();

            if (valido)
                AliquotaIssEspecialParaContribuinteOptantePeloSimples = aliquota;
            else
                AliquotaIssEspecialParaContribuinteOptantePeloSimples = 0;
        }

    }

    public const int minAliquota = 0;
    public const int maxAliquota = 99;

}
