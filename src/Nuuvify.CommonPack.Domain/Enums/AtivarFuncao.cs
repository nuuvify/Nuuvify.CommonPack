using System.ComponentModel;

namespace Nuuvify.CommonPack.Domain.ValueObjects;

/// <summary>
/// N = Desativar
/// S = Ativar
/// </summary>
public enum AtivarFuncao
{

    [Description("N")]
    Desativar = 0,
    [Description("S")]
    Ativar = 1,

}
