using MimeKit;
using Nuuvify.CommonPack.Email.Abstraction;
using Nuuvify.CommonPack.Mediator.Implementation;

namespace Nuuvify.CommonPack.Email;

public partial class Email
{
    private Dictionary<Stream, EmailAttachment> EmailStreamAttachments { get; set; }

    private async Task<bool> AddAttachmentsInMessage(
        TextPart message,
        IDictionary<Stream, EmailAttachment> attachments,
        CancellationToken cancellationToken = default)
    {
        if (!(attachments is null) && attachments?.Count > 0)
        {
            var logText = string.Empty;
            MultipartMessage = MultipartMessage is null ? new Multipart("mixed") : MultipartMessage;

            foreach (var attachment in attachments)
            {
                if (cancellationToken.IsCancellationRequested)
                    return await Task.FromCanceled<bool>(cancellationToken);

                logText = $"Anexando: {attachment.Key}";

                if (attachment.Value.EmailMidia.EmailMidiaFile.Key == EmailMidiaType.Image)
                {
                    logText += $" EmailMidiaType: {EmailMidiaType.Image}";

                    var attachmentMime = new MimePart(EmailMidiaType.Image.ToString().ToLower(), attachment.Value.EmailMidia.EmailMidiaFile.Value.ToString().ToLower())
                    {
                        Content = new MimeContent(attachment.Key, ContentEncoding.Default),
                        ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                        ContentTransferEncoding = ContentEncoding.Base64,
                        FileName = attachment.Value.FullFileName
                    };

                    MultipartMessage.Add(message);
                    MultipartMessage.Add(attachmentMime);

                }
                else if (attachment.Value.EmailMidia.EmailMidiaFile.Key == EmailMidiaType.Text)
                {
                    logText += $" EmailMidiaType: {EmailMidiaType.Text}";

                    var attachmentMime = new MimePart(EmailMidiaType.Text.ToString().ToLower(), attachment.Value.EmailMidia.EmailMidiaFile.Value.ToString().ToLower())
                    {
                        Content = new MimeContent(attachment.Key, ContentEncoding.Default),
                        ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                        FileName = attachment.Value.FullFileName
                    };

                    MultipartMessage.Add(message);
                    MultipartMessage.Add(attachmentMime);

                }
                else if (attachment.Value.EmailMidia.EmailMidiaFile.Key == EmailMidiaType.Application)
                {
                    logText += $" EmailMidiaType: {EmailMidiaType.Application}";

                    var attachmentMime = new MimePart(EmailMidiaType.Application.ToString().ToLower(), attachment.Value.EmailMidia.EmailMidiaFile.Value.ToString().ToLower())
                    {
                        Content = new MimeContent(attachment.Key, ContentEncoding.Default),
                        ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                        ContentTransferEncoding = ContentEncoding.Base64,
                        FileName = attachment.Value.FullFileName
                    };

                    MultipartMessage.Add(message);
                    MultipartMessage.Add(attachmentMime);

                }
                else
                {
                    logText += $" . Não foi possivel anexar, EmailMidiaType não esperado: {attachment.Value.EmailMidia.EmailMidiaFile.Key}";

                }

                LogMessage.Add(new NotificationR(nameof(AddAttachmentsInMessage), logText));

            }
        }

        return await Task.FromResult(true);
    }
}
