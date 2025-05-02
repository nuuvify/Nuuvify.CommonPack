using System.Globalization;
using Nuuvify.CommonPack.Domain.ValueObjects;
using Nuuvify.CommonPack.Extensions.Implementation;
using Nuuvify.CommonPack.Mediator.Implementation;

namespace Nuuvify.CommonPack.Extensions.Brazil;

public class NacionalidadeFiscal : NotifiableR
{

    protected NacionalidadeFiscal() { }

    public NacionalidadeFiscal(string situacao)
    {
        if (!ValidarDescricao(situacao) &&
            !ValidarLiteral(situacao))
        {
            Codigo = null;

            AddNotification(ToString(),
                MsgValueObjects.ResourceManager.GetString("ValueObjectInvalid", CultureInfo.CurrentCulture)
                    .Replace("{valueObject}", ToString()));
        }

    }

    public string Codigo { get; private set; }

    private bool ValidarLiteral(string literal)
    {
        var intCodigo = literal.ToEnumNumero<EnumNacionalidadeFiscal>();
        var ehValido = !intCodigo.Equals(int.MaxValue);

        if (ehValido)
        {
            Codigo = literal.ToEnumDescricao<EnumNacionalidadeFiscal>();

        }

        return ehValido;
    }

    private bool ValidarDescricao(string descricao)
    {

        descricao = descricao?.ToUpper();

        var literal = descricao.GetCodeEnumByDescription<EnumNacionalidadeFiscal>();
        var intCodigo = literal.ToEnumNumero<EnumNacionalidadeFiscal>();
        var ehValido = !intCodigo.Equals(int.MaxValue);

        if (ehValido)
        {
            Codigo = descricao;

        }

        return ehValido;
    }

    public const int MaxCodigo = 1;

    public override string ToString()
    {
        return Codigo?.ToString();
    }

    public override int GetHashCode()
    {
        var textoEnum = Codigo.GetCodeEnumByDescription<EnumNacionalidadeFiscal>();
        return textoEnum.ToEnumNumero<EnumNacionalidadeFiscal>();
    }

    public override bool Equals(object obj)
    {
        return obj is NacionalidadeFiscal fiscal &&
               EqualityComparer<IReadOnlyCollection<NotificationR>>.Default.Equals(Notifications, fiscal.Notifications) &&
               Codigo == fiscal.Codigo;
    }
}
