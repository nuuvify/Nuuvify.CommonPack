using System;
using System.IO;


namespace Nuuvify.CommonPack.Extensions
{
    public class FileData
    {

        public string Id { get; set; }
        public string Name { get; private set; }
        public byte[] Content { get; private set; }



        /// <summary>
        /// Retorna uma string concatenando Id_Name
        /// </summary>
        /// <returns></returns>
        public string GetIdAndNameFile()
        {
            return string.IsNullOrWhiteSpace(Id)
                ? Name
                : $"{Id}_{Name}";

        }


        /// <summary>
        /// Informe um Stream, e fileLength, sera populado as propriedades: <br/>
        /// Name = Nome do arquivo que foi informado <br/>
        /// Content = Conteudo do arquivo no formato byte array <br/> 
        /// </summary>
        /// <param name="stream">Arquivo em stream</param>
        /// <param name="fileLength">byte[].Length</param>
        /// <param name="fileName">Nome completo do arquivo ao qual pertence o conteudo, se informado sera populado a propriedade Name</param>
        public void StreamToByteArray(Stream stream, int fileLength = 0, string fileName = null)
        {
            if (!string.IsNullOrWhiteSpace(fileName))
                Name = fileName;

            using var binaryReader = new BinaryReader(stream);
            Content = binaryReader.ReadBytes(fileLength);

            binaryReader.Close();
            binaryReader.Dispose();

        }

        /// <summary>
        /// Informe o caminho nome de um arquivo, será populado as propriedades <br/>
        /// Name = Nome do arquivo que foi informado <br/>
        /// Content = Conteudo do arquivo no formato byte array <br/> 
        /// </summary>
        /// <param name="fileName">Caminho e nome do arquivo</param>
        public void FileToByteArray(string fileName)
        {
            byte[] fileContent;
            using var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            var binaryReader = new BinaryReader(fs);
            long byteLength = new FileInfo(fileName).Length;

            fileContent = binaryReader.ReadBytes((Int32)byteLength);
            Content = fileContent;
            Name = fs.Name;

            fs.Close();
            fs.Dispose();
            binaryReader.Close();

        }

        /// <summary>
        /// Converte um byte array em string, util para armazenamento de arquivos
        /// </summary>
        /// <param name="fileContent">Conteudo de um arquivo</param>
        /// <param name="fileName">Nome completo do arquivo ao qual pertence o conteudo, se informado sera populado a propriedade Name</param>
        /// <returns></returns>
        public string FileFromByteArray(byte[] fileContent, string fileName = null)
        {
            if (!string.IsNullOrWhiteSpace(fileName))
                Name = fileName;

            var content = Convert.ToBase64String(fileContent);
            return content;
        }

        /// <summary>
        /// Faz download de byte[] para um arquivo `pathFileName`, criando a pasta caso não exista
        /// Importante: Só use esse metodo se estiver usando um worker, em caso de front end ou api, use FileStreamResultCustom
        /// </summary>
        /// <param name="fileContent">Conteudo de um arquivo em byte[]</param>
        /// <param name="pathFileName">Nome completo do arquivo ao qual pertence o conteudo, sera criado caso não exista</param>
        /// <returns></returns>
        public void Download(byte[] fileContent, string pathFileName)
        {

            Directory.CreateDirectory(Path.GetDirectoryName(pathFileName));
            File.WriteAllBytes(pathFileName, fileContent);

            Name = pathFileName;
            Content = fileContent;

        }


    }
}