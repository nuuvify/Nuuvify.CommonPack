using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Nuuvify.CommonPack.StandardHttpClient.Results;
using Nuuvify.CommonPack.StandardHttpClient.Extensions;
using System.Linq;
using System.Collections.Generic;

namespace Nuuvify.CommonPack.StandardHttpClient
{

    public partial class StandardHttpClient
    {


        public async Task<HttpStandardReturn> Post(
            string urlRoute,
            object messageBody,
            CancellationToken cancellationToken = default)
        {
            var url = $"{urlRoute}{_queryString.ToQueryString()}";


            var message = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(messageBody),
                    Encoding.UTF8, "application/json"
                )
            }
            .CustomRequestHeader(_headerStandard)
            .AddAuthorizationHeader(_headerAuthorization);


            return await StandardSendAsync(url, message, cancellationToken);

        }

        ///<inheritdoc/>
        public async Task<HttpStandardReturn> Post(
            string urlRoute,
            object messageBody,
            string mediaType,
            CancellationToken cancellationToken = default)
        {
            var url = $"{urlRoute}{_queryString.ToQueryString()}";



            var message = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(
                    messageBody.ToString(),
                    Encoding.UTF8,
                    mediaType
                )
            }
            .CustomRequestHeader(_headerStandard)
            .AddAuthorizationHeader(_headerAuthorization);


            return await StandardSendAsync(url, message, cancellationToken);

        }

        ///<inheritdoc/>
        public async Task<HttpStandardReturn> Post(
            string urlRoute,
            MultipartFormDataContent messageBody,
            string mediaType = "multipart/form-data",
            CancellationToken cancellationToken = default)
        {
            var url = $"{urlRoute}{_queryString.ToQueryString()}";


            WithHeader("Accept", new MediaTypeWithQualityHeaderValue(mediaType));

            var message = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = messageBody
            }
            .CustomRequestHeader(_headerStandard)
            .AddAuthorizationHeader(_headerAuthorization);


            return await StandardSendAsync(url, message, cancellationToken);

        }

        public async Task<HttpStandardReturn> Post(
            string urlRoute,
            CancellationToken cancellationToken = default)
        {
            var url = $"{urlRoute}{_queryString.ToQueryString()}";



            var message = new HttpRequestMessage(HttpMethod.Post, url);

            IEnumerable<KeyValuePair<string, string>> enumerable = _formParameter;

            var content = new FormUrlEncodedContent(enumerable);

            message.Content = content;
            message.CustomRequestHeader(_headerStandard)
                   .AddAuthorizationHeader(_headerAuthorization);


            return await StandardSendAsync(url, message, cancellationToken);

        }

        public async Task<HttpStandardStreamReturn> PostStream(
            string urlRoute,
            MultipartFormDataContent messageBody,
            string mediaType = "multipart/form-data",
            CancellationToken cancellationToken = default)
        {
            var url = $"{urlRoute}{_queryString.ToQueryString()}";


            WithHeader("Accept", new MediaTypeWithQualityHeaderValue(mediaType));

            var message = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = messageBody
            }
            .CustomRequestHeader(_headerStandard)
            .AddAuthorizationHeader(_headerAuthorization);


            return await StandardStreamSendAsync(url, message, cancellationToken);
        }



    }
}
