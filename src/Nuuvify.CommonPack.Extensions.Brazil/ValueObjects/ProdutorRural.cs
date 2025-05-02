using Nuuvify.CommonPack.Domain.ValueObjects;
using Nuuvify.CommonPack.Extensions.Implementation;
using Nuuvify.CommonPack.Mediator.Implementation;

namespace Nuuvify.CommonPack.Extensions.Brazil;

public class ProdutorRural : NotifiableR
{

    public ProdutorRural(EnumSimNao simNao, string cei)
    {

        DefinirIndicador(simNao);
        DefinirCeiDoProdutorRural(cei, simNao);

    }

    public string IndicadorProdutorRural { get; private set; }
    public string CeiDoProdutorRural { get; private set; }

    private void DefinirIndicador(EnumSimNao simNao)
    {

        if (simNao == EnumSimNao.Sim)
            IndicadorProdutorRural = EnumSimNao.Sim.GetDescription();
        else
            IndicadorProdutorRural = EnumSimNao.Nao.GetDescription();
    }

    private void DefinirCeiDoProdutorRural(string cei, EnumSimNao simNao)
    {

        if (simNao == EnumSimNao.Sim)
        {
            var validacao = Notifications.Count;

            if (cei.Length < MinCei || cei.Length > MaxCei)
            {
                AddNotification(nameof(CeiDoProdutorRural), $"CEI must be between {MinCei} and {MaxCei} characters.");
            }

            if (validacao.Equals(Notifications.Count))
                CeiDoProdutorRural = cei;

        }

    }

    public const int MinCei = 0;
    public const int MaxCei = 12;

}
