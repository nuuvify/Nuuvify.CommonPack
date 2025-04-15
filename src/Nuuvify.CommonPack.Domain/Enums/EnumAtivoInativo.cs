using System.ComponentModel;

namespace Nuuvify.CommonPack.Domain.ValueObjects;

/// <summary>
/// Informar "A" para Ativo
/// Informar "I" para Inativo
/// Informar "N" Ativo e Inativo
///      N - Deve ser utilizado para selecionar dados apenas, não para classificar um cadastro.
/// </summary>
public enum EnumAtivoInativo
{
    [Description("A")]
    Ativo = 0,

    [Description("I")]
    Inativo = 1,

    [Description("N")]
    Ambos = 2,

}
