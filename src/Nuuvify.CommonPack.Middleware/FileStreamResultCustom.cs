using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace Nuuvify.CommonPack.Middleware
{
    public class FileStreamResultCustom
    {

        private readonly IDictionary<string, byte[]> _blobFiles;

        public FileStreamResultCustom(IDictionary<string, byte[]> blobFiles)
        {
            _blobFiles = blobFiles;
        }


        /// <summary>
        /// Retorna o ContentType do arquivo informado, ou retorna o default
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="defaultContentType"></param>
        /// <returns></returns>
        public string GetFileContentType(
            string fileName,
            string defaultContentType = "application/octet-stream")
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return null;
            }

            var contentTypeProvider = new FileExtensionContentTypeProvider();
            if (contentTypeProvider.TryGetContentType(fileName, out var contentType))
            {
                return contentType;
            }

            return defaultContentType;

        }


        /// <summary>
        /// Informe o nome do arquivo que sera utilizado no download, ou mantenha null <br/>
        /// para baixar com o nome do storage. Exemplo de uso, na Controller:
        /// <example>
        /// <code>
        ///      var fileStreamResult = fileStreamCustom.Create();
        ///
        ///      if (fileStreamResult.NotNullOrZero())
        ///      {
        ///          foreach (var item in fileStreamResult)
        ///          {
        ///              await item.ExecuteResultAsync(this.ControllerContext);
        ///          }
        ///
        ///          return new OkResult();
        ///      }
        ///
        ///      return new NoContentResult();
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="fileName">Nome para download</param>
        /// <returns></returns>
        public IEnumerable<FileStreamResult> Create(string fileName = null)
        {
            FileStreamResult fileStreamResult;
            MemoryStream memoryStream;
            string contentType;

            foreach (var file in _blobFiles)
            {
                contentType = GetFileContentType(file.Key);

                memoryStream = new MemoryStream(file.Value);

                fileStreamResult = new FileStreamResult(memoryStream, contentType);
                if (string.IsNullOrWhiteSpace(fileName))
                {
                    fileStreamResult.FileDownloadName = file.Key;
                }
                else
                {
                    fileStreamResult.FileDownloadName = file.Key.Replace(file.Key, fileName);
                }

                yield return fileStreamResult;
            }

        }
    }
}