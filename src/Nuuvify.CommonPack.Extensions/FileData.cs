using System;
using System.IO;

namespace Nuuvify.CommonPack.Extensions
{
    public class FileData
    {

        public string Id { get; set; }
        public string Name { get; private set; }
        public byte[] Content { get; private set; }



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


        public string FileFromByteArray(byte[] fileContent)
        {
            var content = Convert.ToBase64String(fileContent);
            return content;
        }


    }
}