using Nuuvify.CommonPack.Extensions.Implementation;
using Nuuvify.CommonPack.Extensions.Notificator;

namespace Nuuvify.CommonPack.Domain.ValueObjects;


public class TipoOperacaoExecutadaVo : NotifiableR
{

    protected TipoOperacaoExecutadaVo() { }

    public TipoOperacaoExecutadaVo(int numero)
    {

        if (!Validar(numero))
            AddNotification(typeof(TipoOperacaoExecutadaVo).Name, "Tipo da Operacao Executada invalido");
    }

    public int Codigo { get; private set; }

    private bool Validar(int numero)
    {
        var _numero = numero.ToEnumCodigo<TipoOperacaoExecutada>();
        if (_numero == null) return false;

        if (!_numero.Equals(int.MaxValue.ToString()))
        {
            Codigo = numero;
            return true;
        }
        else
        {
            Codigo = int.MaxValue;
        }

        return false;
    }

    public const int maxTipoOperacaoExecutada = 1;
}
