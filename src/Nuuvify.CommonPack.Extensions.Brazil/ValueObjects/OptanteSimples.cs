using Nuuvify.CommonPack.Extensions.Implementation;
using Nuuvify.CommonPack.Extensions.Notificator;

namespace Nuuvify.CommonPack.Extensions.Brazil;


public class OptanteSimples : NotifiableR
{

    public OptanteSimples(EnumSimNao simNao, DateTime dataEntradaOuSaida)
    {

        DefinirIndicador(simNao);
        DefinirDataDaOpcao(dataEntradaOuSaida);
    }

    public string OptantePeloSimples { get; private set; }
    public DateTime DataOpcaoPeloSimples { get; private set; }

    private void DefinirIndicador(EnumSimNao simNao)
    {

        if (simNao == EnumSimNao.Sim)
            OptantePeloSimples = EnumSimNao.Sim.GetDescription();
        else
            OptantePeloSimples = EnumSimNao.Nao.GetDescription();
    }

    private void DefinirDataDaOpcao(DateTime dataOpcaoPeloSimples)
    {
        var validacao = Notifications.Count;

        _ = new ValidationConcernR<OptanteSimples>(this)
            .AssertNotDateTimeNull(x => dataOpcaoPeloSimples);

        if (validacao.Equals(Notifications.Count))
            DataOpcaoPeloSimples = dataOpcaoPeloSimples;
    }

}
