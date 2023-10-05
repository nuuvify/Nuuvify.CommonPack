using System.ComponentModel;

namespace Nuuvify.CommonPack.Email.Abstraction
{
    public enum PessoasEmail
    {
        [Description("Remetente")]
        From = 0,
        [Description("Destinatario")]
        To = 1,
        [Description("Destinatario Com Copia")]
        Cc = 2,
        [Description("Destinatario Com Copia Oculta")]
        Bcc = 3
    }
}
