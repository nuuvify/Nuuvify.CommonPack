using MailKit.Net.Smtp;
using Nuuvify.CommonPack.Email.Abstraction;
using Nuuvify.CommonPack.Extensions.Notificator;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nuuvify.CommonPack.Email
{
    public partial class Email : IEmail
    {
        ///<inheritdoc/>
        public List<NotificationR> Notifications { get; set; }

        ///<inheritdoc/>
        public List<NotificationR> LogMessage { get; set; }

        /// <summary>
        /// Caso estiver configurando um servidor Smpt externamente, utilize USING
        /// e chame o Email.SendAsync dentro do using.
        /// </summary>
        /// <value></value>
        public SmtpClient SmtpCustomClient { get; set; }

        ///<inheritdoc/>
        public EmailServerConfiguration EmailServerConfiguration
        {
            get => _emailServerConfiguration;
            set
            {
                Notifications.RemoveAll(x => x.Property == nameof(EmailServerConfiguration));

                if (string.IsNullOrWhiteSpace(value.ServerHost))
                {
                    Notifications.Add(new NotificationR(nameof(EmailServerConfiguration),
                        $"Host de email não informado"));
                }
                else
                    _emailServerConfiguration = value;
            }
        }

        private EmailServerConfiguration _emailServerConfiguration;

        public Email(EmailServerConfiguration emailServerConfiguration)
        {
            Notifications = new List<NotificationR>();
            LogMessage = new List<NotificationR>();

            _emailServerConfiguration = emailServerConfiguration;

            EmailServerConfiguration = _emailServerConfiguration;
        }

        ///<inheritdoc/>
        public IEmail WithAttachment(
            string pathFileFullName,
            EmailMidiaType midiaType,
            EmailMidiaSubType midiaSubType)
        {
            EmailAttachments ??= new Dictionary<string, EmailMidia>();
            EmailAttachments.Add(pathFileFullName, new EmailMidia(midiaType, midiaSubType));

            return this;
        }

        ///<inheritdoc/>
        public IEmail WithAttachment(
            Stream fileStream,
            EmailMidiaType midiaType,
            EmailMidiaSubType midiaSubType,
            string fullFileName)
        {
            EmailStreamAttachments ??= new Dictionary<Stream, EmailAttachment>();
            EmailStreamAttachments.Add(fileStream, new EmailAttachment(new EmailMidia(midiaType, midiaSubType), fullFileName));

            return this;
        }

        ///<inheritdoc/>
        public IEmail RemoveAllAttachments()
        {
            EmailAttachments?.Clear();
            EmailStreamAttachments?.Clear();

            return this;
        }

        ///<inheritdoc/>
        public IEmail RemoveAllLogMessage()
        {
            LogMessage?.Clear();

            return this;
        }

        ///<inheritdoc/>
        public IEmail RemoveAllNotifications()
        {
            Notifications?.Clear();

            return this;
        }

        ///<inheritdoc/>
        public void ResetMailInstance()
        {
            RemoveAllAttachments()
            .RemoveAllLogMessage()
            .RemoveAllNotifications();
        }

        ///<inheritdoc/>
        public int CountAttachments()
        {
            var count = EmailAttachments?.Count ?? 0 + EmailStreamAttachments?.Count ?? 0;
            return count;
        }

        ///<inheritdoc/>
        public virtual async Task<object> EnviarAsync(
            Dictionary<string, string> recipients,
            Dictionary<string, string> senders,
            string subject,
            string message,
            CancellationToken cancellationToken = default)
        {

            if (EmailServerConfiguration is null ||
                string.IsNullOrWhiteSpace(EmailServerConfiguration.ServerHost))
            {
                Notifications.Add(new NotificationR(nameof(SendEmailCustom), $"Configurações do servidor de email: {nameof(EmailServerConfiguration)} não encontrado"));

                return false;
            }

            return await EmailHandler(
                SetAddressEmailList(PessoasEmail.To, recipients),
                SetAddressEmailList(PessoasEmail.From, senders),
                CreateEmailMessage(subject),
                message,
                cancellationToken);
        }

        public bool IsValid() => Notifications?.Count == 0;

        ///<inheritdoc/>
        public string GetEmailTemplate(
            Dictionary<string, string> variables,
            string templateFullName)
        {
            if (!File.Exists(templateFullName))
            {
                Notifications.Add(new NotificationR(nameof(GetEmailTemplate), $"Nome do template ou caminho informado não existe: {templateFullName}"));
                return string.Empty;
            }

            if (!(variables?.Count > 0))
            {
                Notifications.Add(new NotificationR(nameof(GetEmailTemplate), $"Pelo meno uma variavel precisa fazer parte do seu template: {variables?.Count}"));
                return string.Empty;
            }

            StringBuilder newHtml;

            using (var fs = File.OpenRead(templateFullName))
                newHtml = ReadTemplate(fs);

            var newTemplate = newHtml.ToString();

            foreach (var variable in variables)
                newTemplate = newTemplate.Replace(variable.Key.ToString(), variable.Value.ToString());

            return newTemplate;
        }
    }
}