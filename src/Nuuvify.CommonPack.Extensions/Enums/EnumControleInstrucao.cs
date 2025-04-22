using System.ComponentModel;

namespace Nuuvify.CommonPack.Domain.ValueObjects;

public enum EnumControleInstrucao
{
    [Description("I")]
    Inclui = 0,

    [Description("A")]
    Altera = 1,

    [Description("E")]
    Exclui = 2,

    [Description("M")]
    IncluiAltera = 3,

}
