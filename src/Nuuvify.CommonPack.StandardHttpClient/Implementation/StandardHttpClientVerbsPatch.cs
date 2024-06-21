﻿using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Nuuvify.CommonPack.StandardHttpClient.Results;
using Nuuvify.CommonPack.StandardHttpClient.Extensions;
using System.Net.Http.Headers;
using System.Collections.Generic;

namespace Nuuvify.CommonPack.StandardHttpClient
{

    public partial class StandardHttpClient
    {


        public async Task<HttpStandardReturn> Patch(
            string urlRoute,
            object messageBody,
            CancellationToken cancellationToken = default)
        {
            var url = $"{urlRoute}{_queryString.ToQueryString()}";


            var message = new HttpRequestMessage(HttpMethod.Patch, url)
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

        public async Task<HttpStandardReturn> Patch(
            string urlRoute,
            CancellationToken cancellationToken = default)
        {
            var url = $"{urlRoute}{_queryString.ToQueryString()}";



            var message = new HttpRequestMessage(HttpMethod.Patch, url);

            IEnumerable<KeyValuePair<string, string>> enumerable = _formParameter;

            var content = new FormUrlEncodedContent(enumerable);

            message.Content = content;
            message.CustomRequestHeader(_headerStandard)
                   .AddAuthorizationHeader(_headerAuthorization);


            return await StandardSendAsync(url, message, cancellationToken);

        }

        public async Task<HttpStandardStreamReturn> PatchStream(
            string urlRoute,
            MultipartFormDataContent messageBody,
            string mediaType = "multipart/form-data",
            CancellationToken cancellationToken = default)
        {
            var url = $"{urlRoute}{_queryString.ToQueryString()}";


            WithHeader("Accept", new MediaTypeWithQualityHeaderValue(mediaType));

            var message = new HttpRequestMessage(HttpMethod.Patch, url)
            {
                Content = messageBody
            }
            .CustomRequestHeader(_headerStandard)
            .AddAuthorizationHeader(_headerAuthorization);


            return await StandardStreamSendAsync(url, message, cancellationToken);
        }



    }
}
