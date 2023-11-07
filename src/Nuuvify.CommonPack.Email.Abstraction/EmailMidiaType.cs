using System.Collections.Generic;

namespace Nuuvify.CommonPack.Email.Abstraction
{
    public enum EmailMidiaType
    {
        Text,
        Image,
        Application
    }

    public enum EmailMidiaSubType
    {
        Text,
        Gif,
        Jpg,
        Png,
        Pdf
    }

    public class EmailMidia
    {
        public KeyValuePair<EmailMidiaType, EmailMidiaSubType> EmailMidiaFile { get; set; }

        public EmailMidia(EmailMidiaType midiaType, EmailMidiaSubType midiaSubType)
        {
            EmailMidiaFile = new KeyValuePair<EmailMidiaType, EmailMidiaSubType>(midiaType, midiaSubType);
        }
    }
}