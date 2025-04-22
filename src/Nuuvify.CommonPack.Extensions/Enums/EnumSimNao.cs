using System.ComponentModel;

namespace Nuuvify.CommonPack.Domain.ValueObjects;

/// <summary>
/// Informar "S" para Sim
/// Informar "N" para Não
/// </summary>
public enum EnumSimNao
{
    [Description("S")]
    Sim = 0,

    [Description("N")]
    Nao = 1,

}
