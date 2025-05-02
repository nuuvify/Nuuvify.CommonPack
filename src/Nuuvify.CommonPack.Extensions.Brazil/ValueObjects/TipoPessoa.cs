using System.Globalization;
using Nuuvify.CommonPack.Domain.ValueObjects;
using Nuuvify.CommonPack.Extensions.Implementation;
using Nuuvify.CommonPack.Mediator.Implementation;

namespace Nuuvify.CommonPack.Extensions.Brazil;

/// <summary>
/// Toda pessoa fisica precisa ter uma data de nascimento valida
/// </summary>
public class TipoPessoa : NotifiableR
{

    protected TipoPessoa() { }

    public TipoPessoa(string pessoa, DateTime? nascimento)
    {
        if (!pessoa.IsEnum<EnumFisicaJuridica>(out int enumCodigo))
        {
            Codigo = null;

            AddNotification(nameof(TipoPessoa), MsgValueObjects.ResourceManager.GetString("ValueObjectInvalid",
                CultureInfo.CurrentCulture).Replace("{valueObject}", ToString()));
            return;
        }
        else
        {
            Codigo = enumCodigo.ToEnumDescricao<EnumFisicaJuridica>();
        }

        if (enumCodigo == EnumFisicaJuridica.Fisica.GetHashCode())
        {
            var ehValido = ValidarNascimento(nascimento);
            if (!ehValido)
            {
                Codigo = null;
            }
        }
    }

    public string Codigo { get; private set; }
    public DateTime DataDeNascimento { get; private set; }

    private bool ValidarNascimento(DateTime? nascimento)
    {

        if (nascimento == null)
        {
            AddNotification(nameof(DataDeNascimento), MsgValueObjects.ResourceManager.GetString("ValueObjectInvalidDate",
            CultureInfo.CurrentCulture).Replace("{property}", nameof(DataDeNascimento)));
            return false;
        }

        if (IsValid())
        {
            DataDeNascimento = new DateTime(nascimento.Value.Year, nascimento.Value.Month, nascimento.Value.Day);
            return true;
        }

        return false;
    }

    public const int MaxCodigo = 1;

    public override string ToString()
    {
        return Codigo?.ToString();
    }

    public override int GetHashCode()
    {
        var textoEnum = Codigo.GetCodeEnumByDescription<EnumFisicaJuridica>();
        return textoEnum.ToEnumNumero<EnumFisicaJuridica>();
    }

    public override bool Equals(object obj)
    {
        return ReferenceEquals(this, obj);
    }
}
