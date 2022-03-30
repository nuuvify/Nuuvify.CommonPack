using System;
using System.Net;

namespace Nuuvify.CommonPack.Extensions.Implementation
{
    public class WebProxyConfigureMethod
    {


        /// <summary>
        /// Retorna IWebProxy com configurações informadas nos parametros ou no caso de null, <br/>
        /// ira utilizar as variaveis de ambiente do OS. <br/>
        /// Uma maneira de utilizar esse metodo é:   WebRequest.DefaultWebProxy = GetProxyWithVariable(); <br/>
        /// </summary>
        /// <returns></returns>
        /// <param name="httpProxyField">Informe o proxy nesse formato: proxy.sudominio.com:80 ou qualquer outra porta</param>
        /// <param name="httpNoProxyField">Lista de dominios para desconsiderar no proxy. Exemplo: <br/>
        /// httpNoProxyField = new string[] {"meudominio.com","outrodominio.com.br"}
        /// </param>
        public WebProxyConfigureMethod(string httpProxyField = null, string[] httpNoProxyField = null)
        {

            HttpProxyField = httpProxyField ?? Environment.GetEnvironmentVariable("HTTP_PROXY", EnvironmentVariableTarget.Process);
            HttpNoProxyField = httpNoProxyField;
            HttpNoProxyString = Environment.GetEnvironmentVariable("NO_PROXY", EnvironmentVariableTarget.Process);

        }

        public string HttpProxyField { get; private set; }
        public string[] HttpNoProxyField { get; private set; }
        private string HttpNoProxyString { get; set; }





        public IWebProxy GetProxyWithVariable()
        {

            if (string.IsNullOrWhiteSpace(HttpProxyField)) return null;


            HttpProxyField = HttpProxyField.Replace("https://", "");


            Uri uriProxy;
            if (!HttpProxyField.StartsWith("http://", StringComparison.InvariantCultureIgnoreCase))
            {
                uriProxy = new Uri($"http://{HttpProxyField}");
            }
            else
            {
                uriProxy = new Uri(HttpProxyField);
            }


            HttpNoProxyField ??= HttpNoProxyString.Split(separator: new string[] { "," }, options: StringSplitOptions.RemoveEmptyEntries);


            var webProxy = new WebProxy(Address: uriProxy,
                BypassOnLocal: true,
                BypassList: HttpNoProxyField);

            return webProxy;

        }

    }

}