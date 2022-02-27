using System.ComponentModel;

namespace Nuuvify.CommonPack.Domain.ValueObjects
{
    /// <summary>
    /// Informar "F" para pessoa Fisica
    /// Informar “J” para pessoa Juridica
    /// </summary>
    public enum EnumFisicaJuridica
    {

        [Description("F")]
        Fisica = 0,

        [Description("J")]
        Juridica = 1,


    }

}
