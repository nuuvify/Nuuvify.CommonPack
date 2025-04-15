using System.ComponentModel;

namespace Nuuvify.CommonPack.Domain.ValueObjects;

/// <summary>
/// N = Não possui conta bancaria
/// S = Conta Corrente
/// P = Conta Poupança
/// C = Conta Conjunta
/// </summary>
public enum TipoContaBancaria
{
    [Description("N")]
    NaoPossuiConta = 0,

    [Description("S")]
    ContaCorrente = 1,

    [Description("P")]
    ContaPoupanca = 2,

    [Description("C")]
    ContaConjunta = 3,

}
