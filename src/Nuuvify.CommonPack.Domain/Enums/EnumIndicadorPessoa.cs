using System.ComponentModel;

namespace Nuuvify.CommonPack.Domain.ValueObjects
{
    public enum EnumIndicadorPessoa
    {

        [Description("C")]
        Cliente = 0,
        [Description("F")]
        Fornecedor = 1,
        [Description("T")]
        Transportador = 2,
        [Description("E")]
        Estabelecimento = 3,
        [Description("U")]
        Funcionario = 4,
        [Description("Z")]
        Todas = 999999

    }

}
