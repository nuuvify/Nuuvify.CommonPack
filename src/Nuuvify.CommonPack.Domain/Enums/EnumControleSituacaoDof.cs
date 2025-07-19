using System.ComponentModel;

namespace Nuuvify.CommonPack.Domain.ValueObjects
{
    public enum EnumControleSituacaoDof
    {
        [Description("S")]
        Cancelado = 0,

        [Description("N")]
        Normal = 1,

        [Description("I")]
        Inutilizado = 2,

        [Description("D")]
        Denegado = 3,

    }
}
