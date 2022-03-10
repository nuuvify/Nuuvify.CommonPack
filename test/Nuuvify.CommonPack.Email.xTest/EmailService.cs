using MailKit.Net.Smtp;
using Moq;
using Nuuvify.CommonPack.Email.Abstraction;

namespace Nuuvify.CommonPack.Email.xTest
{
    public class EmailService : Email
    {
        public static bool Teste { get; set; }
        public static Mock<SmtpClient> MockSmpt { get; set; }


        public EmailService(EmailServerConfiguration emailServerConfiguration)
            : base(emailServerConfiguration)
        {


            SmtpCustomClient = MockSmpt.Object;
        }


    }
}
