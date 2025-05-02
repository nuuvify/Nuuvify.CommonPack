using Nuuvify.CommonPack.Domain.ValueObjects;
using Nuuvify.CommonPack.Extensions.Implementation;
using Nuuvify.CommonPack.Mediator.Implementation;

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

        if (dataOpcaoPeloSimples == default)
        {
            AddNotification(nameof(DataOpcaoPeloSimples), "A data de opção pelo Simples não pode ser nula ou inválida.");
        }

        if (validacao.Equals(Notifications.Count))
            DataOpcaoPeloSimples = dataOpcaoPeloSimples;
    }

}
