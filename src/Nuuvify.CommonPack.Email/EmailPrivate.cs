using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Text;
using Nuuvify.CommonPack.Email.Abstraction;
using Nuuvify.CommonPack.Extensions.Implementation;
using Nuuvify.CommonPack.Extensions.Notificator;

namespace Nuuvify.CommonPack.Email
{

    public partial class Email
    {

        private Dictionary<string, EmailMidia> EmailAttachments { get; set; }


        private bool EmailIsvalid(string email)
        {
            var isValid = Regex.IsMatch(email, @"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$");

            return isValid;
        }

        private InternetAddressList SetAddressEmailList(
            PessoasEmail tipo,
            IDictionary<string, string> pessoas)
        {
            var addressList = new InternetAddressList();

            if (pessoas is null)
            {
                Notifications.Add(new NotificationR(nameof(SetAddressEmailList), $"Lista de {tipo.GetDescription()} não deve ser nula"));
                return addressList;
            }

            if (pessoas?.Count > 0)
            {
                foreach (var item in pessoas)
                {
                    var destino = item.Key.Trim();
                    var destinos = destino.Split(',');

                    if (destinos is null)
                    {
                        destinos?.SetValue(new { Key = item.Key.Trim(), Value = item.Value.Trim() }, 0);
                    }

                    if (destinos?.Length > 0)
                    {
                        foreach (var itemEmail in destinos)
                        {

                            var nome = item.Value.Trim();
                            var endereco = itemEmail.Trim();


                            if (EmailIsvalid(endereco))
                            {
                                addressList.Add(new MailboxAddress(nome, endereco));
                            }
                            else
                                Notifications.Add(new NotificationR(nameof(SetAddressEmailList),
                                    $"Formato de e-mail de {tipo.GetDescription()} invalido {endereco}"));
                        }
                    }

                }
            }


            if (addressList.Count < 1)
            {
                Notifications.Add(new NotificationR(nameof(SetAddressEmailList),
                    $"Não há nenhum {tipo.GetDescription()} para o email"));
            }

            return addressList;
        }

        private async Task<bool> AddAttachmentsInMessage(
            MimeMessage emailMessage,
            TextPart message,
            IDictionary<string, EmailMidia> attachments,
            CancellationToken cancellationToken = default)
        {
            if (attachments is null || attachments?.Count == 0)
            {
                emailMessage.Body = message;
            }
            else
            {

                var logText = string.Empty;
                var multipart = new Multipart("mixed");

                foreach (var attachment in attachments)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return await Task.FromCanceled<bool>(cancellationToken);
                    }

                    logText = $"Anexando: {attachment.Key}";


                    if (!File.Exists(attachment.Key))
                    {
                        var textLog = $"Anexo informado não encontrado ou sem acesso {attachment.Key})";
                        Notifications.Add(new NotificationR(nameof(AddAttachmentsInMessage), textLog));

                        return false;
                    }


                    if (attachment.Value.EmailMidiaFile.Key == EmailMidiaType.Image)
                    {
                        logText += $" EmailMidiaType: {EmailMidiaType.Image}";

                        var attachmentMime = new MimePart(EmailMidiaType.Image.ToString().ToLower(), attachment.Value.EmailMidiaFile.Value.ToString().ToLower())
                        {
                            Content = new MimeContent(File.OpenRead(attachment.Key), ContentEncoding.Default),
                            ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                            ContentTransferEncoding = ContentEncoding.Base64,
                            FileName = Path.GetFileName(attachment.Key)
                        };

                        multipart.Add(message);
                        multipart.Add(attachmentMime);

                        emailMessage.Body = multipart;
                    }
                    else if (attachment.Value.EmailMidiaFile.Key == EmailMidiaType.Text)
                    {

                        logText += $" EmailMidiaType: {EmailMidiaType.Text}";

                        var attachmentMime = new MimePart(EmailMidiaType.Text.ToString().ToLower(), attachment.Value.EmailMidiaFile.Value.ToString().ToLower())
                        {
                            Content = new MimeContent(File.OpenRead(attachment.Key), ContentEncoding.Default),
                            ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                            FileName = Path.GetFileName(attachment.Key),
                        };

                        multipart.Add(message);
                        multipart.Add(attachmentMime);

                        emailMessage.Body = multipart;
                    }
                    else if (attachment.Value.EmailMidiaFile.Key == EmailMidiaType.Application)
                    {
                        logText += $" EmailMidiaType: {EmailMidiaType.Application}";

                        var attachmentMime = new MimePart(EmailMidiaType.Application.ToString().ToLower(), attachment.Value.EmailMidiaFile.Value.ToString().ToLower())
                        {
                            Content = new MimeContent(File.OpenRead(attachment.Key), ContentEncoding.Default),
                            ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                            ContentTransferEncoding = ContentEncoding.Base64,
                            FileName = Path.GetFileName(attachment.Key)
                        };

                        multipart.Add(message);
                        multipart.Add(attachmentMime);

                        emailMessage.Body = multipart;

                    }

                    LogMessage.Add(new NotificationR(nameof(AddAttachmentsInMessage), logText));

                }
            }

            return await Task.FromResult(true);

        }

        private async Task<object> EmailHandler(InternetAddressList recipients,
            InternetAddressList senders,
            MimeMessage emailMessage,
            string message,
            CancellationToken cancellationToken = default)
        {


            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled(cancellationToken);
            }


            var body = new TextPart(TextFormat.Html)
            {
                Text = message
            };


            emailMessage.From.AddRange(senders);
            emailMessage.To.AddRange(recipients);
            await AddAttachmentsInMessage(emailMessage, body, EmailAttachments, cancellationToken);
            await AddAttachmentsInMessage(emailMessage, body, EmailStreamAttachments, cancellationToken);


            if (!IsValid()) return false;



            SmtpCustomClient ??= new SmtpClient();

            bool returnTask = await SendEmailCustom(SmtpCustomClient, emailMessage, cancellationToken);


            return returnTask;
        }

        private async Task<bool> SendEmailCustom(
            SmtpClient client,
            MimeMessage emailMessage,
            CancellationToken cancellationToken = default)
        {
            try
            {

                if (EmailServerConfiguration is null)
                {
                    Notifications.Add(new NotificationR(nameof(SendEmailCustom), $"Configurações do servidor de email: {nameof(EmailServerConfiguration)} não encontrado"));

                    return false;
                }

                if (!client.IsConnected)
                {
                    if (string.IsNullOrWhiteSpace(EmailServerConfiguration.AccountUserName))
                    {
                        await client.ConnectAsync(
                            EmailServerConfiguration.ServerHost,
                            EmailServerConfiguration.Port,
                            cancellationToken: cancellationToken);
                    }
                    else
                    {
                        var protocol = MailKit.Security.SecureSocketOptions.Auto;

                        if (EmailServerConfiguration.Security.ToString().Contains("Ssl"))
                        {
                            protocol = MailKit.Security.SecureSocketOptions.SslOnConnect;
                        }
                        else if (EmailServerConfiguration.Security.ToString().Contains("Tls"))
                        {
                            protocol = MailKit.Security.SecureSocketOptions.StartTlsWhenAvailable;
                        }

                        await client.ConnectAsync(
                            EmailServerConfiguration.ServerHost,
                            EmailServerConfiguration.Port,
                            protocol,
                            cancellationToken);


                        await client.AuthenticateAsync(
                            EmailServerConfiguration.AccountUserName,
                            EmailServerConfiguration.AccountPassword,
                            cancellationToken: cancellationToken);
                    }

                }

                if (cancellationToken.IsCancellationRequested)
                {
                    return false;
                }

                await client.SendAsync(
                    emailMessage,
                    cancellationToken: cancellationToken);


                LogMessage.Add(new NotificationR(nameof(SendEmailCustom), $"Email enviado pelo host {EmailServerConfiguration.ServerHost} {emailMessage.Subject?.ToString()}"));


                return true;
            }
            catch (Exception ex)
            {
                Notifications.Add(new NotificationR(nameof(SendEmailCustom), $"Houve erro no envio do e-mail: {ex.Message}", $"{EmailServerConfiguration.ServerHost} {emailMessage.Subject?.ToString()}"));

                return false;
            }
            finally
            {
                await client.DisconnectAsync(true);
            }

        }



        private MimeMessage CreateEmailMessage(string subject)
        {

            var emailMessage = new MimeMessage
            {
                Date = DateTimeOffset.Now,
                Priority = MessagePriority.Normal,
                Subject = subject
            };

            return emailMessage;
        }


        private StringBuilder ReadTemplate(Stream fs)
        {
            var newHtml = new StringBuilder();

            using var reader = new StreamReader(fs);

            while (!reader.EndOfStream)
            {
                newHtml.AppendLine(reader.ReadLine());
            }

            return newHtml;
        }




    }
}
