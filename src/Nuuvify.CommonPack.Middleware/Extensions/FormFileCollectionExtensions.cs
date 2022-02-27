using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Http;

namespace Nuuvify.CommonPack.Middleware.Extensions
{
    public static class FormFileCollectionExtensions
    {
        public static IDictionary<string, string> Notifications { get; private set; }

        /// <summary>
        /// Retorna uma lista de arquivos, onde:
        /// <para>Key = é composto por prefixFileName_FileName</para>
        /// <para>Value = conteudo do arquivo no formato byte[]</para>
        /// </summary>
        /// <param name="files"></param>
        /// <param name="prefixFileName">Example: IdDocument_TypeDocument</param>
        /// <returns></returns>
        public static IDictionary<string, byte[]> GetFilesBase64(this IFormFileCollection files, string prefixFileName = null)
        {

            var list = new Dictionary<string, byte[]>();

            if (files == null || files.Count == 0) 
                return list;


            string fileName;
            byte[] fileBytes;

            foreach (var file in files)
            {
                using (var ms = new MemoryStream())
                {
                    file.CopyTo(ms);
                    fileBytes = ms.ToArray();
                }


                fileName = $"{prefixFileName}{file.FileName}";
                if (!list.TryGetValue(fileName.Trim(), out byte[] value))
                {
                    list.Add(fileName.Trim(), fileBytes);
                }

            }

            return list;

        }

    }


}