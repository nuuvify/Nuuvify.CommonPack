using System.ComponentModel;

namespace Nuuvify.CommonPack.Domain.ValueObjects
{
    /// <summary>
    /// REINF
    /// SAP
    /// </summary>
    public enum CodigoSistema
    {
        [Description(nameof(REINF))]
        REINF = 1,

        [Description(nameof(SAP))]
        SAP = 2,

    }
}
