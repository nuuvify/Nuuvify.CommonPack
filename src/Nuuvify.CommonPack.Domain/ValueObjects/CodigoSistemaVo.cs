using System.Globalization;
using Nuuvify.CommonPack.Extensions.Implementation;
using Nuuvify.CommonPack.Extensions.Notificator;

namespace Nuuvify.CommonPack.Domain.ValueObjects;


public class CodigoSistemaVo : NotifiableR
{

    protected CodigoSistemaVo() { }

    public CodigoSistemaVo(string situacao)
    {
        if (!ValidarDescricao(situacao) && !ValidarLiteral(situacao))
        {
            this.Codigo = null;

            AddNotification(this.ToString(), MsgValueObjects.ResourceManager.GetString("ValueObjectInvalid", CultureInfo.CurrentCulture).Replace("{valueObject}", this.ToString()));

        }

    }

    public string Codigo { get; private set; }

    private bool ValidarLiteral(string literal)
    {
        var intCodigo = literal.ToEnumNumero<CodigoSistema>();
        var ehValido = !intCodigo.Equals(int.MaxValue);

        if (ehValido)
        {
            Codigo = literal.ToEnumDescricao<CodigoSistema>();
        }

        return ehValido;
    }

    private bool ValidarDescricao(string descricao)
    {

        descricao = descricao?.ToUpper();

        var literal = descricao.GetCodeEnumByDescription<CodigoSistema>();
        var intCodigo = literal.ToEnumNumero<CodigoSistema>();
        var ehValido = !intCodigo.Equals(int.MaxValue);

        if (ehValido)
        {
            Codigo = descricao;
        }

        return ehValido;
    }

    public const int maxCodigo = 5;

    public override string ToString()
    {
        return Codigo?.ToString();
    }

    public override int GetHashCode()
    {
        var textoEnum = Codigo.GetCodeEnumByDescription<CodigoSistema>();
        return textoEnum.ToEnumNumero<CodigoSistema>();
    }

    public override bool Equals(object obj)
    {
        return obj is CodigoSistemaVo vo &&
               EqualityComparer<IReadOnlyCollection<NotificationR>>.Default.Equals(Notifications, vo.Notifications) &&
               Codigo == vo.Codigo;
    }
}
