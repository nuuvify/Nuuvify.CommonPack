using Nuuvify.CommonPack.Extensions.Implementation;

namespace Nuuvify.CommonPack.StandardHttpClient.Results
{
    /// <summary>
    /// Essa classe padroniza o retorno de todos os requests Http, é utilizada pela classe:
    /// <see cref="BaseStandardHttpClient"/>
    /// </summary>
    public class HttpStandardReturn
    {
        public bool Success { get; set; }
        public string ReturnCode { get; set; }
        public string ReturnMessage { get; set; }


        /// <summary>
        /// Esse metodo retorna o conteudo do ReturnMessage sem os caracteres \r\n no json inteiro e espacos no inicio do json
        /// </summary>
        /// <returns></returns>
        public string GetReturnMessageWithoutRn()
        {

            return ReturnMessage.GetReturnMessageWithoutRn();
        }

    }


}
