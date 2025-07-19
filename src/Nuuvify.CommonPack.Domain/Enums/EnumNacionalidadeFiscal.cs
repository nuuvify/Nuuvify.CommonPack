using System.ComponentModel;

namespace Nuuvify.CommonPack.Domain.ValueObjects
{
    // <summary>
    // Informar "N" para Nacional
    // Informar “S” Estrangeiro sem CPF/CNPJ
    // Informar “C” Estrangeiro com CPF/CNPJ
    // </summary>
    public enum EnumNacionalidadeFiscal
    {

        [Description("N")]
        Nacional = 0,

        [Description("S")]
        EstrangeiroSemCpfCnpj = 1,

        [Description("C")]
        EstrangeiroComCpfCnpj = 2,


    }

}
