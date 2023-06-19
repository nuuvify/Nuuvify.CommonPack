using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MimeKit;
using Nuuvify.CommonPack.Email.Abstraction;

namespace Nuuvify.CommonPack.Email
{

    public partial class Email
    {

        private Dictionary<Stream, EmailMidia> EmailStreamAttachments { get; set; }


        private async Task<bool> AddAttachmentsInMessage(
            MimeMessage emailMessage,
            TextPart message,
            IDictionary<Stream, EmailMidia> attachments,
            CancellationToken cancellationToken = default)
        {
            if (attachments is null || attachments?.Count == 0)
            {
                emailMessage.Body = message;
            }
            else
            {

                var multipart = new Multipart("mixed");

                foreach (var attachment in attachments)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return await Task.FromCanceled<bool>(cancellationToken);
                    }


                    if (attachment.Value.EmailMidiaFile.Key == EmailMidiaType.Image)
                    {
                        var attachmentMime = new MimePart(EmailMidiaType.Image.ToString().ToLower(), attachment.Value.EmailMidiaFile.Value.ToString().ToLower())
                        {
                            Content = new MimeContent(attachment.Key, ContentEncoding.Default),
                            ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                            ContentTransferEncoding = ContentEncoding.Base64,
                            //FileName = Path.GetFileName(attachment.Key)
                        };

                        multipart.Add(message);
                        multipart.Add(attachmentMime);

                        emailMessage.Body = multipart;
                    }
                    else if (attachment.Value.EmailMidiaFile.Key == EmailMidiaType.Text)
                    {

                        var attachmentMime = new MimePart(EmailMidiaType.Text.ToString().ToLower(), attachment.Value.EmailMidiaFile.Value.ToString().ToLower())
                        {
                            Content = new MimeContent(attachment.Key, ContentEncoding.Default),
                            ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                            // FileName = Path.GetFileName(attachment.Key),
                        };

                        multipart.Add(message);
                        multipart.Add(attachmentMime);

                        emailMessage.Body = multipart;
                    }
                    else if (attachment.Value.EmailMidiaFile.Key == EmailMidiaType.Application)
                    {
                        var attachmentMime = new MimePart(EmailMidiaType.Application.ToString().ToLower(), attachment.Value.EmailMidiaFile.Value.ToString().ToLower())
                        {
                            Content = new MimeContent(attachment.Key, ContentEncoding.Default),
                            ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                            ContentTransferEncoding = ContentEncoding.Base64,
                            // FileName = Path.GetFileName(attachment.Key)
                        };

                        multipart.Add(message);
                        multipart.Add(attachmentMime);

                        emailMessage.Body = multipart;
                    }
                }
            }

            return await Task.FromResult(true);

        }



    }
}
