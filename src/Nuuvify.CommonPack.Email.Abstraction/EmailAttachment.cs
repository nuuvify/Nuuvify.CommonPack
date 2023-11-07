namespace Nuuvify.CommonPack.Email.Abstraction
{
    public class EmailAttachment
    {
        public EmailAttachment(EmailMidia emailMidia, string fullFileName)
        {
            EmailMidia = emailMidia;
            FullFileName = fullFileName;
        }

        public EmailMidia EmailMidia { get; set; }
        public string FullFileName { get; set; }
    }
}