using System.IO;

namespace Nuuvify.CommonPack.StandardHttpClient.Results
{
    /// <summary>
    /// Essa classe padroniza o retorno de todos os requests Http, é utilizada pela classe:
    /// <see cref="BaseStandardHttpClient"/>
    /// </summary>
    public class HttpStandardStreamReturn
    {
        public bool Success { get; set; }
        public string ReturnCode { get; set; }
        public Stream ReturnMessage { get; set; }



    }


}
